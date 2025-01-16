using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;

namespace UnityEditor
{
    [CustomEditor(typeof(ALPG_Generator))]
    public class ALPG_Generator_Editor : Editor
    {
        [SerializeField] public static bool foldParams;
        [SerializeField] public static bool foldChecks;
        [SerializeField] public static bool foldOther;
        [SerializeField] public static bool foldNavmesh;
        [SerializeField] public static int clusterCountRounded;

        SerializedProperty mergeDistance;
        SerializedProperty mergeAirDistance;
        SerializedProperty floorOffset;
        SerializedProperty heightDistanceCheck;
        SerializedProperty collisionCheckSize;
        SerializedProperty colLayers;

        SerializedProperty MergeThroughCollision;

        SerializedProperty UseLightProbeNavMesh;
        SerializedProperty ForceRebuildNavMesh;
        SerializedProperty m_AgentTypeID;
        SerializedProperty navTileSize;
        SerializedProperty navLayers;

        SerializedProperty tagCheck;
        SerializedProperty visCheck;
        SerializedProperty visCheckHeight;
        SerializedProperty visLayers;

        SerializedProperty showProcess;
        
        SerializedProperty probeObject;
        SerializedProperty probeZoneObject;

        void OnEnable()
        {
            mergeDistance = serializedObject.FindProperty("mergeDistance");
            mergeAirDistance = serializedObject.FindProperty("mergeAirDistance");
            floorOffset = serializedObject.FindProperty("floorOffset");
            heightDistanceCheck = serializedObject.FindProperty("heightDistanceCheck");
            collisionCheckSize = serializedObject.FindProperty("collisionCheckSize");
            colLayers = serializedObject.FindProperty("colLayers");

            MergeThroughCollision = serializedObject.FindProperty("MergeThroughCollision");

            UseLightProbeNavMesh = serializedObject.FindProperty("UseLightProbeNavMesh");
            ForceRebuildNavMesh = serializedObject.FindProperty("ForceRebuildNavMesh");
            m_AgentTypeID = serializedObject.FindProperty("m_AgentTypeID");
            navTileSize = serializedObject.FindProperty("navTileSize");
            navLayers = serializedObject.FindProperty("navLayers");

            tagCheck = serializedObject.FindProperty("tagCheck");
            visCheck = serializedObject.FindProperty("visCheck");
            visCheckHeight = serializedObject.FindProperty("visCheckHeight");
            visLayers = serializedObject.FindProperty("visLayers");

            showProcess = serializedObject.FindProperty("showProcess");

            probeObject = serializedObject.FindProperty("probeObject");
            probeZoneObject = serializedObject.FindProperty("probeZoneObject");
        }

        public override void OnInspectorGUI()
        {
            ALPG_Generator Worker = (ALPG_Generator)target;
            GUILayoutOption[] layout = { GUILayout.Width(150), GUILayout.Height(22) };

            serializedObject.Update();

            EditorGUILayout.Space();
            GUILayout.BeginVertical();

            if (GUILayout.Button("Generate probes", layout))
                Worker.GenerateProbes();

            EditorGUILayout.Space();

            StartFold(ref foldParams, "Params");

            if (foldParams)
            {
                EditorGUILayout.PropertyField(mergeDistance, new GUIContent("Merge distance"));
                EditorGUILayout.PropertyField(mergeAirDistance, new GUIContent("Merge distance (airProbe)"));
                EditorGUILayout.PropertyField(floorOffset, new GUIContent("Floor offset"));
                EditorGUILayout.PropertyField(heightDistanceCheck, new GUIContent("Height checking distance"));
                EditorGUILayout.PropertyField(collisionCheckSize, new GUIContent("Collision check radius"));
                EditorGUILayout.PropertyField(colLayers, new GUIContent("Collision check layers"));

                int clusterResult = getClusterAmount(clusterCountRounded);
                clusterCountRounded = EditorGUILayout.IntSlider("Cluster count: #" + clusterResult, clusterCountRounded, 1, 20);
                Worker.clusterCount = clusterResult;
            }

            EndFold();

            EditorGUILayout.Separator();

            StartFold(ref foldNavmesh, "Nav mesh");

            if (foldNavmesh)
            {
                EditorGUILayout.PropertyField(UseLightProbeNavMesh, new GUIContent("Light probe nav mesh (Slow)"));
                EditorGUILayout.PropertyField(ForceRebuildNavMesh, new GUIContent("Force build"));

                var agent_settings = NavMesh.GetSettingsByID(m_AgentTypeID.intValue);

                if (agent_settings.agentTypeID != -1)
                {
                    // Draw image
                    const float diagramHeight = 80.0f;
                    Rect agentDiagramRect = EditorGUILayout.GetControlRect(false, diagramHeight);
                    NavMeshEditorHelpers.DrawAgentDiagram(agentDiagramRect, agent_settings.agentRadius, agent_settings.agentHeight, agent_settings.agentClimb, agent_settings.agentSlope);
                }
                NavMeshComponentsGUIUtility.AgentTypePopup("Agent Type", m_AgentTypeID);

                EditorGUILayout.PropertyField(navTileSize, new GUIContent("Nav Tile Size"));
                EditorGUILayout.PropertyField(navLayers, new GUIContent("Nav mesh bake on layers"));

                EditorGUILayout.LabelField("Calculated nav mesh gets cached. Repeat calculations are quick!");
            }

            EndFold();

            EditorGUILayout.Separator();

            StartFold(ref foldChecks, "Checks");

            if (foldChecks)
            {
                EditorGUILayout.PropertyField(MergeThroughCollision, new GUIContent("Merge through colliders"));
                EditorGUILayout.PropertyField(tagCheck, new GUIContent("Tag Check"));
                EditorGUILayout.PropertyField(visCheck, new GUIContent("Vis Check"));
                EditorGUILayout.PropertyField(visCheckHeight, new GUIContent("Vis check height"));
                EditorGUILayout.LabelField("(Terrain Y + 'Vis check height')");
                EditorGUILayout.PropertyField(visLayers, new GUIContent("Vis check layers"));
            }

            EndFold();

            EditorGUILayout.Separator();

            StartFold(ref foldOther, "Other");

            if (foldOther)
            {
                EditorGUILayout.PropertyField(showProcess, new GUIContent("Show progress bar"));

                EditorGUILayout.PropertyField(probeObject, new GUIContent("Obj: Light probe group"));
                EditorGUILayout.PropertyField(probeZoneObject, new GUIContent("Obj: Probe zone"));

                if (GUILayout.Button("Debug Clusters", layout))
                    Worker.GenerateClusters();

                if (GUILayout.Button("Flush nav surfaces", layout))
                    Worker.FlushSurfaces();
            }

            EndFold();

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private int getClusterAmount(int v)
        {
            return (int)Mathf.Pow(v, 2);
        }

        private void StartFold(ref bool fold, string title)
        {
#if UNITY_2019_2_OR_NEWER
            fold = EditorGUILayout.BeginFoldoutHeaderGroup(fold, title);
#else
            fold = EditorGUILayout.Foldout(fold, title, true);
#endif
        }

        private void EndFold()
        {
#if UNITY_2019_2_OR_NEWER
            EditorGUILayout.EndFoldoutHeaderGroup();
#else
            return;
#endif
        }
    }
}
 