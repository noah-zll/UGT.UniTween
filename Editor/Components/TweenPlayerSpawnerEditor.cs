using UGT.UniTween.Components;
using UnityEditor;
using UnityEngine;

namespace UGT.UniTween.Editor
{
    [CustomEditor(typeof(TweenPlayerSpawner))]
    public sealed class TweenPlayerSpawnerEditor : UnityEditor.Editor
    {
        private TweenPlayerSpawner _spawner;

        private SerializedProperty _prefabProp;
        private SerializedProperty _modeProp;
        private SerializedProperty _burstCountProp;
        private SerializedProperty _spawnIntervalProp;
        private SerializedProperty _poolCapacityProp;
        private SerializedProperty _spawnAsChildProp;
        private SerializedProperty _randomOffsetProp;
        private SerializedProperty _useIndependentTimeScaleProp;
        private SerializedProperty _timeScaleProp;

        private void OnEnable()
        {
            _spawner = (TweenPlayerSpawner)target;

            _prefabProp = serializedObject.FindProperty("_prefab");
            _modeProp = serializedObject.FindProperty("_mode");
            _burstCountProp = serializedObject.FindProperty("_burstCount");
            _spawnIntervalProp = serializedObject.FindProperty("_spawnInterval");
            _poolCapacityProp = serializedObject.FindProperty("_poolCapacity");
            _spawnAsChildProp = serializedObject.FindProperty("_spawnAsChild");
            _randomOffsetProp = serializedObject.FindProperty("_randomOffset");
            _useIndependentTimeScaleProp = serializedObject.FindProperty("_useIndependentTimeScale");
            _timeScaleProp = serializedObject.FindProperty("_timeScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ── 配置 ──
            EditorGUILayout.PropertyField(_prefabProp);
            EditorGUILayout.PropertyField(_modeProp);

            SpawnMode mode = (SpawnMode)_modeProp.enumValueIndex;
            if (mode == SpawnMode.Burst)
            {
                EditorGUILayout.PropertyField(_burstCountProp);
            }
            else if (mode == SpawnMode.Loop)
            {
                EditorGUILayout.PropertyField(_spawnIntervalProp);
                EditorGUILayout.PropertyField(_poolCapacityProp);

                EditorGUILayout.Space(2f);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Independent Time Scale", GUILayout.Width(EditorGUIUtility.labelWidth));
                    _useIndependentTimeScaleProp.boolValue = EditorGUILayout.Toggle(_useIndependentTimeScaleProp.boolValue, GUILayout.Width(20f));
                    if (_useIndependentTimeScaleProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(_timeScaleProp, GUIContent.none);
                    }
                }
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.PropertyField(_spawnAsChildProp);
            EditorGUILayout.PropertyField(_randomOffsetProp);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8f);

            // ── 手动控制 ──
            EditorGUILayout.LabelField("Preview Controls", EditorStyles.boldLabel);

            if (_prefabProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("请先指定 Prefab 字段。", MessageType.Warning);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Spawn Burst", GUILayout.Height(28f)))
                {
                    int count = _burstCountProp.intValue;
                    if (count <= 0) count = 5;
                    _spawner.SpawnBurst(count);
                }

                GUI.enabled = !_spawner.IsLooping;
                if (GUILayout.Button("Start Loop", GUILayout.Height(28f)))
                {
                    _spawner.StartLoop();
                }
                GUI.enabled = true;

                GUI.enabled = _spawner.IsLooping;
                if (GUILayout.Button("Stop Loop", GUILayout.Height(28f)))
                {
                    _spawner.StopLoopByEditor();
                }
                GUI.enabled = true;
            }

            // ── 池状态 ──
            if (_spawner.IsLooping)
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.HelpBox("Loop 运行中...", MessageType.Info);
            }
        }
    }
}
