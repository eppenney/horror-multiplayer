using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
#if UNITY_EDITOR
[CanEditMultipleObjects]
#endif
public class ALPG_ProbeVolume : MonoBehaviour
{
#region Params
    //Lightprobe volume placement component
    [HideInInspector]
    public BoxCollider _Volume;
    private Vector3 P_Volume_Size;
    private Vector3 P_Volume_Center;
    [HideInInspector]
    public List<Vector3> probes;

    [Header("Density / 1m^3")]
    [Range(0, 3)]
    public float _Density = 1;
    private float P_Density;

    [Header("Axis weights")]
    [Range(0, 100)]
    public float _DensityWeight_x = 100f;
    private float P_DensityWeight_x;
    [Range(0, 100)]
    public float _DensityWeight_y = 100f;
    private float P_DensityWeight_y;
    [Range(0, 100)]
    public float _DensityWeight_z = 100f;
    private float P_DensityWeight_z;

    [Header("Reflection Probe")]
    public bool _ReflectionProbe;
    private bool P_ReflectionProbe;
    public bool _ReflectionProbe_SyncVolume;
    private ReflectionProbe _RP;

    [Header("Debug")]
    [Range(0, 1)]
    public float _DebugDrawScale = 1f;
    [Range(0, 1)]
    public float _DebugDrawAlpha = 0.8f;

    private float r;
    private Vector3 tp;
    private Quaternion tr;
    private Vector3 ts;

    private int frameNumber = 0;

    private Camera sceneCam;
    private Vector3 cam = new Vector3(0, 0, 0);

    public bool _IncludeInMerge;
#endregion

    private void OnEnable()
    {
        _Volume = GetComponent<BoxCollider>();
#if UNITY_EDITOR
        EditorApplication.update += UpdateMain;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= UpdateMain;
#endif
    }

    void UpdateMain()
    {
        frameNumber++;

        if (frameNumber % 3 != 0)
            return;

        if (!Application.isEditor || Application.isPlaying)
            return;

        //Force gameObject layer to be ignore raycast
        if (gameObject.layer != 2)
        {
            gameObject.layer = 2;
        }

        if (IsDirty(_ReflectionProbe, ref P_ReflectionProbe))
        {
            if (_ReflectionProbe)
            {
                if (_RP == null)
                {
                    ReflectionProbe tryResult = GetComponent<ReflectionProbe>();

                    if (tryResult != null)
                        _RP = tryResult;
                    else
                        _RP = gameObject.AddComponent<ReflectionProbe>();
                }

            }
            else
            {
                if (_RP != null)
                    DestroyImmediate(_RP, false);
            }
        }

        if (ValueChanged())
        {
            probes = CalculateProbes(true);
            SyncReflectionProbe();
        }

        if (_ReflectionProbe_SyncVolume)
        {
            if (_RP != null)
                SyncReflectionProbe();
        }

        if (probes != null)
        {
            if (sceneCam == null)
            {
                if (Camera.current != null)
                {
                    sceneCam = Camera.current;
                }
            }
            else
            {
                if (cam != sceneCam.transform.position)
                {
                    cam = sceneCam.transform.position;
                    probes.Sort(SortByDistanceToCamera);
                }
            }
        }
    }

    public List<Vector3> CalculateProbes(bool offsetProbes)
    {
        List<Vector3> newProbes = new List<Vector3>();

        Vector3 Size = Vector3.Scale(_Volume.size, transform.localScale);
        Vector3 SizeRounded = RoundDown(Size, Vector3.one);

        Vector3 _DensityStep = RoundDown((SizeRounded * _Density) - Vector3.one, new Vector3(_DensityWeight_x / 100f, _DensityWeight_y / 100f, _DensityWeight_z / 100f));
        Vector3 _Offset = new Vector3((_DensityStep.x / -2), (_DensityStep.y / -2), (_DensityStep.z / -2));

        Vector3 p = Vector3.Scale(SizeRounded, new Vector3(1 / _DensityStep.x, 1 / _DensityStep.y, 1 / _DensityStep.z));

        //Calculate radius
        r = Mathf.Clamp(((SumDiv(RoundDown(Size, Vector3.one)) / 2f) / SumDiv(_DensityStep)) / 2f, 0.1f, _DebugDrawScale / 2f);

        for (int z = 0; z <= _DensityStep.z; z++)
        {
            for (int y = 0; y <= _DensityStep.y; y++)
            {
                for (int x = 0; x <= _DensityStep.x; x++)
                {
                    Vector3 xyz = Vector3.Scale(new Vector3(_Offset.x + x, _Offset.y + y, _Offset.z + z), p) + Vector3.Scale(_Volume.center, transform.localScale);

                    //Transform to volume rotation
                    xyz = _Volume.transform.TransformDirection(xyz);

                    Vector3 addOffset = offsetProbes ? transform.position : new Vector3(0, 0, 0);

                    Vector3 probe = (addOffset + xyz);

                    newProbes.Add(probe);
                }
            }
        }

        return newProbes;
    }

    void OnDrawGizmosSelected()
    {
        if (probes == null) { return; }

        Color c = Color.red;
        c.a = _DebugDrawAlpha;
        Gizmos.color = c;

        foreach (Vector3 p in probes)
        {
            Gizmos.DrawSphere(p, r * _DebugDrawScale);
        }
    }

    private int SortByDistanceToCamera(Vector3 p1, Vector3 p2)
    {
        return Vector3.Distance(p2, cam).CompareTo(Vector3.Distance(p1, cam));
    }

    Vector3 RoundDown(Vector3 v, Vector3 pow)
    {
        return new Vector3(Mathf.Floor(v.x * pow.x), Mathf.Floor(v.y * pow.y), Mathf.Floor(v.z * pow.z));
    }

    float SumDiv(Vector3 v)
    {
        return ((v.x + v.y + v.z) / 3f);
    }

    void SyncReflectionProbe()
    {
        if (!_ReflectionProbe_SyncVolume || _RP == null)
            return;

        _RP.center = Vector3.Scale(_Volume.center, transform.localScale);
        _RP.size = _Volume.bounds.size;

        return;
    }

    bool ValueChanged()
    {
        if (IsDirty(_Density, ref P_Density))
            return true;

        if (IsDirty(_DensityWeight_x, ref P_DensityWeight_x))
            return true;

        if (IsDirty(_DensityWeight_y, ref P_DensityWeight_y))
            return true;

        if (IsDirty(_DensityWeight_z, ref P_DensityWeight_z))
            return true;

        if (IsDirty(_Volume.size, ref P_Volume_Size))
            return true;

        if (IsDirty(_Volume.center, ref P_Volume_Center))
            return true;

        if (tp != transform.position || tr != transform.rotation || ts != transform.localScale)
        {
            tp = transform.position;
            tr = transform.rotation;
            ts = transform.localScale;

            return true;
        }

        return false;
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

    public void BakeVolume()
    {
#if UNITY_EDITOR
        //Bake down porbes into new light probe group component
        LightProbeGroup lp = gameObject.AddComponent<LightProbeGroup>();
        lp.probePositions = CalculateProbes(false).ToArray();

        //Reset transform to un-skew probe volume
        transform.localScale = new Vector3(1, 1, 1);
        transform.rotation = Quaternion.identity;

        //Get box collider reference before removing main component
        BoxCollider boxCol = GetComponent<BoxCollider>();

        //Destroy component first to enable removal of required components
        DestroyImmediate(this);

        //Delete volume reference box collider if it still exists
        if (boxCol != null)
            DestroyImmediate(boxCol);
#endif
        return;
    }
}
