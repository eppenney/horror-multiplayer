using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ALPG_Generator : MonoBehaviour
{
    #region Main params section
    static Progress progress = new Progress();

    List<ProbeCluster> clusters;

    [Tooltip("Layers to use for collision checking.")]
    public LayerMask colLayers;
    [Tooltip("Layers to use for visibility checking.")]
    public LayerMask visLayers;
    [Tooltip("Layers to use for navmesh generation.")]
    public LayerMask navLayers;

    [Tooltip("Threshold distance for merging regular probes")]
    public float mergeDistance = 0.75f;

    [Tooltip("Threshold distance for merging 'air probes'.")]
    public float mergeAirDistance = 6;

    [Tooltip("Vertical offset used to place probes above navmesh.")]
    public float floorOffset = 0.25f;

    [Tooltip("Vertical height checking distance, used to look for gemometry above floor probe(s) to use as reference points to encapsulate 'interior' spaces.")]
    public float heightDistanceCheck = 6f;

    [Tooltip("The radius of collision checking used to cull probes too close to level geometry. NOTE that setting this amount to a higher value than your floor offset, it will end up culling all probes placed on the navmesh!")]
    public float collisionCheckSize = 0.05f;

    [Tooltip("Amount of clusters to generate for processing. (Lower amounts recommended for smaller scenes)")]
    public int clusterCount = 128;

    [Tooltip("Allows probes to merge through geometry. Turning this off will make the generation process considerably slower due to added collision checks!")]
    public bool MergeThroughCollision = true;

    [Tooltip("Check for Ignore Collision components on scene objects.")]
    public bool tagCheck = true;
    [Tooltip("Enable the 'Visibility Checking' post-process step to cull probes outside of 'visible scene spaces'. NOTE that this step is not recommended to use unless you understand and are using a rather specific scene structure.")]
    public bool visCheck = false;
    [Tooltip("Vertical height offset from terrain object's corners to start vis checking from. This is to avoid visibility checks going too high or low, depending on your terrain sculpting and level geometry placement.")]
    public float visCheckHeight = 15f;

    [Tooltip("Show a more detailed representation of the generation process in loading bar. NOTE that this slows down the generation process to a crawl! <'Only for nerds'>")]
    public bool showProcess = false;

    [Tooltip("Object scene reference to the 'Main light probe group carrier' for ALPG generation. NOTE that it's not recommended to change this from the default prefab object!")]
    public GameObject probeObject;
    [Tooltip("Object scene reference to the 'ALPG probe zone' used for culling all probes outside of it (Also used as reference for cluster generation). NOTE that it's not recommended to change this from the default prefab object!")]
    public GameObject probeZoneObject;

    private BoxCollider probeZone;
    private LightProbeGroup probeGroup;

    [SerializeField, Tooltip("Enable generation of separate nav mesh used for probe placement, with custom settings!")]
    public bool UseLightProbeNavMesh;
    [Tooltip("Force rebuild the generated nav mesh. Enable this each time a change has been done to scene layout!")]
    public bool ForceRebuildNavMesh;
    public int m_AgentTypeID;
    private int p_AgentTypeID;

    private NavMeshSurface nms;

    [Tooltip("Nav mesh tile size. Use lower values to achieve denser results on plain nav mesh surfaces (slower)!")]
    public int navTileSize = 32;
    private int p_navTileSize;

    [SerializeField]
    bool calculateNavmesh = true;
    #endregion

    public void GenerateProbes()
    {
        if (CheckValidComponents() == false)
            return;

        //[ Start operation ]//
        ResetProgress();
        float TimeStart = Time.realtimeSinceStartup;

        if (IsDirty(m_AgentTypeID, ref p_AgentTypeID))
            calculateNavmesh = true;

        if (IsDirty(navTileSize, ref p_navTileSize))
            calculateNavmesh = true;

        if (UseLightProbeNavMesh)
        {
            if ((calculateNavmesh || nms == null) || (ForceRebuildNavMesh))
            {
                ShowProgress("Calculating light probe nav mesh", "This might take a while depending on tile size...", 0.05f, false);
                //Remove any lingering nav surface components
                FlushSurfaces();

                nms = gameObject.AddComponent<NavMeshSurface>();
                nms.hideFlags = HideFlags.HideInInspector;
                nms.overrideTileSize = true;
                nms.tileSize = navTileSize;
                nms.agentTypeID = m_AgentTypeID;
                nms.layerMask = navLayers;
                nms.BuildNavMesh();

                calculateNavmesh = false;

                if (ForceRebuildNavMesh)
                    ForceRebuildNavMesh = false;
            }
        }

        ShowProgress("Generating probes", "Initial prep...", 0.1f, false);

        NavMeshTriangulation navMesh = NavMesh.CalculateTriangulation();

        if (navMesh.areas.Length == 0 || navMesh.vertices.Length == 0)
        {
            Debug.Log("No baked navmesh found! Exiting operation...");

#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
            return;
        }

        //Refresh light probe group with a new component (avoids Edit toggle issue)
        CreateNewLightProbeGroup();

        List<Collider> meshColliders = PrepMeshColliders();
        List<Vector3> probeList = new List<Vector3>();
        List<Probe> airProbes = new List<Probe>();

        ShowProgress("Generating probes", "Generating probes...", 0.12f, false);

        //Offset navMesh verts by floor offset and add to main probe list
        foreach (Vector3 p in navMesh.vertices)
        {
            probeList.Add(p + (Vector3.up * floorOffset));
        }

        //Create clusters
        clusters = GenerateClusters();
        ResetProgress();

        //Start with trying to add air probes (avoid adding air probes from volumes!)
        foreach (Vector3 cp in probeList)
        {
            Vector3 v_pos;
            bool v_hit;

            VerticalCheck(cp, out v_pos, out v_hit);

            //Only add air probe if distance is greater than thresh
            if (v_pos.y - cp.y > 1.5f)
                airProbes.Add(new Probe(v_pos, false, !v_hit));
        }

        ShowProgress("Collecting probe volumes", "Collecting probe volumes...", 0.15f, false);

        //Collect probe volumes to be included in merge
        foreach (ALPG_ProbeVolume pv in FindObjectsOfType<ALPG_ProbeVolume>())//cp* Cluster probe
        {
            //Add probes for merge stage if it's enabled on the volume
            if (pv._IncludeInMerge)
            {
                //Calculate and add to probeList
                if (pv.probes == null)
                    probeList.AddRange(pv.CalculateProbes(true));
                else
                    probeList.AddRange(pv.probes);
            }
        }

        ShowProgress("Clustering probes", "Clustering probes...", 0.2f, false);
        ResetProgress();

        //Chunk up probes into clusters
        foreach (Vector3 cp in probeList)//cp* Cluster probe
        {
            //Check if within zone & get cluster
            ProbeCluster pc = CheckCluster(cp);

            if (pc != null)
            {
                pc.Probes.Add(new Probe(cp, false, false));
            }

            progress.done++;
            progress.percentage = (progress.done / ((float)probeList.Count));
            progress.task = "Placing probes in clusters: " + progress.done + "/" + probeList.Count + " (" + (progress.percentage * 100f).ToString("F0") + "%)";
            ShowProgress("Clustering probes");
        }

        ShowProgress("Clustering air probes", "Clustering air probes...", 0.3f, false);
        ResetProgress();
       
        //Chunk air up probes into clusters (due to them being separated out from Vector list)
        foreach (Probe ap in airProbes)//cp* Cluster probe
        {
            //Check if within zone & get cluster
            ProbeCluster pc = CheckCluster(ap.pos);

            if (pc != null)
            {
                pc.Probes.Add(ap);
            }

            progress.done++;
            progress.percentage = (progress.done / ((float)airProbes.Count));
            progress.task = "Placing air probes in clusters: " + progress.done + "/" + airProbes.Count + " (" + (progress.percentage * 100f).ToString("F0") + "%)";
            ShowProgress("Clustering air probes");
        }

        //Re-set cluster index
        List<Probe> mergedProbes = new List<Probe>();
        List<Vector3> validProbes = new List<Vector3>();
        List<Vector3> nearbyProbes = new List<Vector3>();
        
        ShowProgress("Main merge pass", "Merging clustered probes...", 0.5f, false);

        //Try to ensure cleanup is easy if something happens
        try
        {
            //First pass clustered merging
            foreach (ProbeCluster probe_cluster in clusters)
            {
                //Reset iterating numbers
                ResetProgress();

                foreach (Probe probe_stage_01 in probe_cluster.Probes)
                {
                    nearbyProbes.Clear();

                    if (probe_stage_01.used == false)
                    {
                        nearbyProbes.Add(probe_stage_01.pos);
                        probe_stage_01.used = true;

                        foreach (Probe probe_stage_02 in probe_cluster.Probes)
                        {
                            //If probes pass checking pass
                            if (CheckProbes(probe_stage_02, probe_stage_01))
                            {
                                //Add it to the valid list
                                nearbyProbes.Add(probe_stage_02.pos);
                            }
                        }

                        Vector3 newProbePos = new Vector3();

                        //Average probe location between merged probes
                        foreach (Vector3 prooo in nearbyProbes)
                        {
                            newProbePos += prooo;
                        }

                        newProbePos /= nearbyProbes.ToArray().Length;

                        //Attempt to adjust if air
                        if (probe_stage_01.airProbe)
                            newProbePos = AdjustPosRadial(newProbePos);

                        Probe newProbe = new Probe(newProbePos, false, probe_stage_01.airProbe);

                        mergedProbes.Add(newProbe);
                    }

                    progress.done++;
                    progress.probes++;
                    progress.percentage = (progress.done / ((float)probe_cluster.Probes.Count));
                    progress.task = "Merging probes in cluster #" + probe_cluster.Index + "/" + clusters.Count + ": " + progress.probes + "/" + probe_cluster.Probes.Count;
                    ShowProgress("Merging probes");
                }

                progress.clusters++;
                progress.task = "Merging probes in cluster #" + probe_cluster.Index + "/" + clusters.Count;
                progress.percentage = (((float)(clusterCount - 1)) / (float)(clusters.Count));
                ShowProgress("Merging probes");
            }

            ShowProgress("Merging probes", "Final probe merge stage (" + mergedProbes.Count + " probes)", 0.65f, false);
            ResetProgress();

            //Second/ global pass clustered merging
            foreach (Probe probe_stage_01 in mergedProbes)
            {
                nearbyProbes.Clear();

                if (probe_stage_01.used == false)
                {
                    nearbyProbes.Add(probe_stage_01.pos);
                    probe_stage_01.used = true;

                    foreach (Probe probe_stage_02 in mergedProbes)
                    {
                        //If probes pass checking pass
                        if (CheckProbes(probe_stage_02, probe_stage_01))
                        {
                            //Add it to the valid list
                            nearbyProbes.Add(probe_stage_02.pos);
                        }
                    }

                    Vector3 validProbePos = new Vector3();

                    //Average probe location between merged probes
                    foreach (Vector3 prooo in nearbyProbes)
                    {
                        validProbePos += prooo;
                    }

                    validProbePos /= nearbyProbes.ToArray().Length;
                    validProbePos = AdjustPosRadial(validProbePos);

                    validProbes.Add(validProbePos);
                }

                progress.done++;
                progress.probes++;
                progress.percentage = (progress.done / ((float)mergedProbes.Count));
                progress.task = "Final probe merge stage: " + progress.probes + "/" + mergedProbes.Count;
                    
                ShowProgress("Merging probes");
            }
        }
        catch
        {
            Debug.Log("Something went wrong! Cleaning residuals...");
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
            //Delete cluster objects
            foreach (ProbeCluster pc in clusters)
            {
                DestroyImmediate(pc.Obj);
            }

            //Fix up temporary mesh colliders from turning convex
            foreach (MeshCollider mc in meshColliders)
            {
                mc.convex = false;
            }
        }

        ShowProgress("Finalizing valid probes", "Final steps & cleanup...", 0.8f, false);

        //Collect probe volumes to be included in post-merge list
        foreach (ALPG_ProbeVolume pv in FindObjectsOfType<ALPG_ProbeVolume>())//cp* Cluster probe
        {
            //Add probes for merge stage if it's enabled on the volume
            if (!pv._IncludeInMerge)
            {
                //Calculate and add to probeList
                if (pv.probes == null)
                    validProbes.AddRange(pv.CalculateProbes(true));
                else
                    validProbes.AddRange(pv.probes);
            }
        }

        //Delete cluster objects
        foreach (ProbeCluster pc in clusters)
        {
            DestroyImmediate(pc.Obj);
        }

        //Fix up temporary mesh colliders from turning convex
        foreach (MeshCollider mc in meshColliders)
        {
            mc.convex = false;
        }

        /*Cache custom nav mesh if it's been calculated once
        if (nms != null)
            DestroyImmediate(nms, false);
        */

        ShowProgress("Finalizing valid probes", "Performing Vis Check cleanup...", 0.9f, false);

        //Perform VisCheck for out of bounds probes if enabled
        if (visCheck)
            validProbes = VisCheck(validProbes);

#if UNITY_EDITOR
        probeGroup.probePositions = validProbes.ToArray();
        EditorUtility.ClearProgressBar();
#endif

        Debug.Log("Light probe generation took " + ((Time.realtimeSinceStartup - TimeStart).ToString("F0")) + " seconds!");
    }

    private void CreateNewLightProbeGroup()
    {
        //[This method avoids the "Edit Light Probes" toggle issue]//
        probeGroup = probeObject.GetComponent<LightProbeGroup>();

        //Destroy old light probe group component
        if (probeGroup != null)
            DestroyImmediate(probeGroup);

        //Refresh it by making a new one
        probeGroup = probeObject.AddComponent<LightProbeGroup>();

        return;
    }

    private ProbeCluster CheckCluster(Vector3 p)
    {
        if (clusters == null)
        {
            Debug.Log("Clusters are null, exiting...");
            clusters = GenerateClusters();
        }

        //Include simple collision check if "enabled"
        if (collisionCheckSize > 0)
            if (InsideCol(p, collisionCheckSize))
                return null;

        //Cheap initial check if within zone volume
        if (PointInOABB(p, probeZone))
        {
            foreach (ProbeCluster pc in clusters)//pc* Probe Cluster
            {
                if (PointInOABB(p, pc.Bounds))
                {
                    //Return cluster position is within
                    return pc;
                }
            }
        }

        return null;
    }

    private bool CheckProbes(Probe p1, Probe p2)
    {
        bool valid = false;

        if (!p1.used)
        {
            if (p1.airProbe && p2.airProbe)
            {
                if (Vector3.Distance(p1.pos, p2.pos) <= mergeAirDistance)
                {
                    valid = true;
                }
            }
            else
            {
                if (Vector3.Distance(p1.pos, p2.pos) <= mergeDistance)
                {
                    valid = true;
                }
            }
        }

        if (!MergeThroughCollision)
        {
            RaycastHit r;
            Vector3 diff = (p2.pos - p1.pos);

            if (Physics.Raycast(p1.pos, diff.normalized, out r, diff.magnitude, colLayers.value))
            {
                valid = false;
            }
        }

        if (valid)
        {
            p1.used = true;
            return true;
        }

        return false;
    }

    public List<ProbeCluster> GenerateClusters()
    {
        List<ProbeCluster> clusters = new List<ProbeCluster>();

        if (probeZone == null)
            probeZone = probeZoneObject.GetComponent<BoxCollider>();

        Transform pzt = probeZone.transform;

        float clusterRatio = Mathf.Sqrt(clusterCount);
        int clusterAmount = (int)(clusterRatio);
        //int clusterAmount = (int)Mathf.Sqrt(clusterCount);
        Vector3 d = (Vector3.Scale(probeZone.size, probeZoneObject.transform.localScale) / clusterRatio);

        int clusterIndex = 1;

        for (int z = 0; z < clusterAmount; z++)
        {
            for (int x = 0; x < clusterAmount; x++)
            {
                float ox = ((clusterRatio / -2) + x);
                float oz = ((clusterRatio / -2) + z);

                Vector3 offset = new Vector3(ox * d.x + (d.x / 2), 0, oz * d.z + (d.z / 2)) + Vector3.Scale(probeZone.center, pzt.localScale);
                Vector3 inverse = pzt.TransformDirection(offset) + pzt.position;

                //Spawn cluster object
                //GameObject cluster = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //cluster.name = ("Cluster #" + clusterIndex);
                GameObject cluster = new GameObject("Cluster #" + clusterIndex);
                cluster.layer = 2; //Ignore raycast
                cluster.transform.position = inverse;
                cluster.transform.rotation = pzt.rotation;
                cluster.transform.parent = probeObject.transform;
                cluster.transform.localScale = new Vector3(d.x, (probeZone.size.y * probeZoneObject.transform.localScale.y), d.z);

                //Add cluster components
                BoxCollider cbc = cluster.AddComponent<BoxCollider>();
                cbc.isTrigger = true;

                ProbeCluster pc = new ProbeCluster();
                pc.Index = clusterIndex;
                pc.Obj = cluster;
                pc.Bounds = cluster.GetComponent<BoxCollider>();
                pc.Probes = new List<Probe>();

                clusters.Add(pc);

                clusterIndex++;
            }
        }

        return clusters;
    }

    List<Collider> PrepMeshColliders()
    {
        List<Collider> group = new List<Collider>();
        MeshCollider[] filterGroup = FindObjectsOfType<MeshCollider>();

        foreach (MeshCollider o in filterGroup)
        {
            if (o.convex == false)
            {
                o.convex = true;
                group.Add(o);
            }
        }

        return group;
    }

    List<ALPG_ProbeVolume> GetProbeVolumes()
    {
        List<ALPG_ProbeVolume> list = new List<ALPG_ProbeVolume>();
        ALPG_ProbeVolume[] volumes = FindObjectsOfType<ALPG_ProbeVolume>();

        foreach (ALPG_ProbeVolume v in volumes)
        {
            list.Add(v);
        }

        return list;
    }

    Vector3 AdjustPosRadial(Vector3 p)
    {
        float r = (floorOffset - 0.025f);

        if (InsideCol(p, collisionCheckSize))
        {
            Vector3[] dir = new Vector3[6];
            dir[0] = new Vector3(1, 0, 0);
            dir[1] = new Vector3(0, 1, 0);
            dir[2] = new Vector3(0, 0, 1);
            dir[3] = new Vector3(-1, 0, 0);
            dir[4] = new Vector3(0, -1, 0);
            dir[5] = new Vector3(0, 0, -1);

            float dis = r * 2f;
            int steps = 4;

            for (int i = 1; i <= steps; i++)
            {
                for (int direction = 0; direction < 6; direction++)
                {
                    float d = ((dis / steps) * i);
                    Vector3 pos = p + (dir[direction] * d);

                    //Check if position is free
                    if (!InsideCol(pos, r))
                    {
                        //Debug.Log("Adjusted to: " + pos);
                        return pos;
                    }
                }
            }
        }

        return p;
    }

    bool InsideCol(Vector3 p, float r)
    {
#if UNITY_EDITOR
        Collider[] collisions = Physics.OverlapSphere(p, r, colLayers.value);

        //Do simple check if tag checking is turned off
        if (!tagCheck)
        {
            if (collisions.Length == 0)
                return false;
            else
                return true;
        }
        else
        {
            //Quick return if there are no overlaps
            if (collisions.Length == 0)
            {
                return false;
            }
            else
            {
                //Check if any top prefab object carries a mask component
                foreach (Collider c in collisions)
                {
                    try
                    {
                        //If object is a part of a prefab
                        Object obj;
#if UNITY_2017
                        obj = PrefabUtility.GetPrefabObject(c.gameObject);
#else
                        obj = PrefabUtility.GetPrefabInstanceHandle(c.gameObject);
#endif

                        if (obj != null)
                        {
                            //Check if individual object check, or go to root and check
                            if (c.gameObject.GetComponent<ALPG_IgnoreCollision>() == null)
                            {
                                GameObject objPar;
#if UNITY_2017
                                objPar = PrefabUtility.FindRootGameObjectWithSameParentPrefab(c.gameObject);
#else
                                objPar = PrefabUtility.GetOutermostPrefabInstanceRoot(c.gameObject);
#endif

                                if (objPar.GetComponent<ALPG_IgnoreCollision>() == null)
                                    return true;
                                else
                                    return false;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (c.gameObject.GetComponent<ALPG_IgnoreCollision>() == null)
                                return true;
                            else
                                return false;
                        }
                    }
                    catch
                    {
                        //Debug.Log("Looking for mask on a non-prefab in object: " + c.gameObject);
                    }
                }

                return false;
            }
        }
#else
                        return false;
#endif
                    }

    void VerticalCheck(Vector3 p, out Vector3 pos, out bool hit)
    {
        RaycastHit r;

        if (Physics.Raycast(p + (Vector3.up * collisionCheckSize), Vector3.up, out r, heightDistanceCheck, colLayers.value))
        {
            pos = (r.point + Vector3.down * floorOffset);
            hit = true;
        }
        else
        {
            pos = (p + (Vector3.up * heightDistanceCheck) + (Vector3.down * collisionCheckSize));
            hit = false;
        }
    }

    List<Vector3> VisCheck(List<Vector3> probes)
    {
        Terrain t = FindObjectOfType<Terrain>();

        if (t == null)
            return probes;

        Vector3 tp = t.transform.position;
        Vector3 s = t.terrainData.size;

        List<Vector3> visCheckers = new List<Vector3>();
        visCheckers.Add(tp + new Vector3(0f, visCheckHeight, 0f));
        visCheckers.Add(tp + new Vector3(s.x, visCheckHeight, 0f));
        visCheckers.Add(tp + new Vector3(0f, visCheckHeight, s.z));
        visCheckers.Add(tp + new Vector3(s.x, visCheckHeight, s.z));

        List<Vector3> invalidProbes = new List<Vector3>();

        RaycastHit r;

        foreach (Vector3 p in probes)
        {
            foreach (Vector3 c in visCheckers)
            {
                Vector3 dis = (p - c);

                if (!Physics.Raycast(c, Vector3.Normalize(dis), out r, dis.magnitude, visLayers.value))
                {
                    invalidProbes.Add(p);
                }
            }
        }

        foreach (Vector3 p in invalidProbes)
        {
            probes.Remove(p);
        }

        return probes;
    }

    bool PointInOABB(Vector3 p, BoxCollider box)
    {
        p = box.transform.InverseTransformPoint(p) - box.center;

        float halfX = (box.size.x * 0.5f);
        float halfY = (box.size.y * 0.5f);
        float halfZ = (box.size.z * 0.5f);
        if (p.x < halfX && p.x > -halfX &&
            p.y < halfY && p.y > -halfY &&
            p.z < halfZ && p.z > -halfZ)
            return true;
        else
            return false;
    }

    bool CheckValidComponents()
    {
        if (probeZoneObject != null)
            probeZone = probeZoneObject.GetComponent<BoxCollider>();

        if (probeZone == null)
        {
            Debug.Log("No probe zone! Stopping...");
            return false;
        }
        else
        {
            //Default probe zone settings
            probeZone.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            probeZone.isTrigger = true;
        }

        if (probeObject == null)
        {
            Debug.Log("No light probe group object assigned! Stopping...");
            return false;
        }

        return true;
    }

    bool IsDirty<T>(T checkValue, ref T storedValue) where T : struct
    {
        if (!storedValue.Equals(checkValue))
        {
            storedValue = checkValue;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void FlushSurfaces()
    {
        NavMeshSurface navSurf = GetComponent<NavMeshSurface>();

        if (navSurf != null)
        {
            DestroyImmediate(navSurf);
            FlushSurfaces();
        }

        return;
    }

    private void ShowProgress(string stage, string newTask = "", float newPercentage = -1, bool explicitUpdate = true)
    {
        if (newTask != "")
            progress.task = newTask;

        if (newPercentage != -1)
            progress.percentage = newPercentage;

#if UNITY_EDITOR
        //Don't update window unless checkbox is checked for more specific data
        if (explicitUpdate && !showProcess)
            return;

        EditorUtility.DisplayProgressBar(stage, progress.task, progress.percentage);
#endif
        return;
    }

    private void ResetProgress()
    {
        progress.task = "";
        progress.done = 0;
        progress.clusters = 1;
        progress.probes = 1;
    }

    public struct Progress
    {
        public string task;
        public float percentage;
        public float done;
        public int clusters;
        public int probes;
    }

    public class Probe
    {
        public Vector3 pos;
        public bool used = false;
        public bool airProbe = false;

        public Probe(Vector3 p, bool u, bool a)
        {
            pos = p;
            used = u;
            airProbe = a;
        }
    }

    public class ProbeCluster
    {
        int index;
        public int Index { get { return index; } set { index = value; } }

        List<Probe> probes;
        public List<Probe> Probes { get { return probes; } set { probes = value; } }

        BoxCollider bounds;
        public BoxCollider Bounds { get { return bounds; } set { bounds = value; } }

        GameObject obj;
        public GameObject Obj { get { return obj; } set { obj = value; } }
    }
}