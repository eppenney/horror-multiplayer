using UnityEngine;

namespace UnityEditor
{
    [CustomEditor(typeof(ALPG_ProbeVolume))]
    public class ALPG_ProbeVolume_Editor : Editor
    {
        [SerializeField] public static float _DebugDrawScale = 1f;
        [SerializeField] public static float _DebugDrawAlpha = 0.8f;

        [SerializeField] public bool _ReflectionProbe;
        [SerializeField] public bool _ReflectionProbe_SyncVolume;
        [SerializeField] public bool _IncludeInMerge;

        SerializedProperty _Density;
        SerializedProperty _DensityWeight_x;
        SerializedProperty _DensityWeight_y;
        SerializedProperty _DensityWeight_z;

        //SerializedProperty _ReflectionProbe;
        //SerializedProperty _ReflectionProbe_SyncVolume;

        private void OnEnable()
        {
            _Density            = serializedObject.FindProperty("_Density");
            _DensityWeight_x    = serializedObject.FindProperty("_DensityWeight_x");
            _DensityWeight_y    = serializedObject.FindProperty("_DensityWeight_y");
            _DensityWeight_z    = serializedObject.FindProperty("_DensityWeight_z");

            //_ReflectionProbe            = serializedObject.FindProperty("_ReflectionProbe");
            //_ReflectionProbe_SyncVolume = serializedObject.FindProperty("_ReflectionProbe_SyncVolume");
        }

        public override void OnInspectorGUI()
        {
            ALPG_ProbeVolume Worker = (ALPG_ProbeVolume)target;
            GUILayoutOption[] style_01 = { GUILayout.Width(304), GUILayout.Height(25) };
            GUILayoutOption[] style_02 = { GUILayout.Width(150), GUILayout.Height(20) };

            EditorGUILayout.Separator();

            GUILayout.BeginVertical("inspectorFullWidthMargins");

            EditorGUILayout.Slider(_Density, 0, 3, new GUIContent("Density / 1m^3"));

            EditorGUILayout.Separator();

            EditorGUILayout.Slider(_DensityWeight_x, 0, 100, new GUIContent("Density X (%)"));
            EditorGUILayout.Slider(_DensityWeight_y, 0, 100, new GUIContent("Density Y (%)"));
            EditorGUILayout.Slider(_DensityWeight_z, 0, 100, new GUIContent("Density Z (%)"));

            EditorGUILayout.Separator();

            _DebugDrawScale = EditorGUILayout.Slider("Probe Scale", _DebugDrawScale, 0, 1);
            _DebugDrawAlpha = EditorGUILayout.Slider("Probe Alpha", _DebugDrawAlpha, 0, 1);
            Worker._DebugDrawScale = _DebugDrawScale;
            Worker._DebugDrawAlpha = _DebugDrawAlpha;

            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            _ReflectionProbe = Worker._ReflectionProbe;
            _ReflectionProbe = GUILayout.Toggle(_ReflectionProbe, "Reflection Probe", "Button", style_02);
            Worker._ReflectionProbe = _ReflectionProbe;

            _ReflectionProbe_SyncVolume = Worker._ReflectionProbe_SyncVolume;
            _ReflectionProbe_SyncVolume = GUILayout.Toggle(_ReflectionProbe_SyncVolume, "Sync Reflection Probe", "Button", style_02);
            Worker._ReflectionProbe_SyncVolume = _ReflectionProbe_SyncVolume;

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            _IncludeInMerge = Worker._IncludeInMerge;
            _IncludeInMerge = GUILayout.Toggle(_IncludeInMerge, "Apply merge pass", "Button", style_01);
            Worker._IncludeInMerge = _IncludeInMerge;

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Bake volume", style_01))
            {
                Worker.BakeVolume();
                DestroyImmediate(this);
                return;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}