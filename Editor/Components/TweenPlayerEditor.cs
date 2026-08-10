using UGT.UniTween.Components;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UGT.UniTween.Editor
{
    /// <summary>
    /// TweenPlayer 自定义 Inspector。
    /// </summary>
    [CustomEditor(typeof(TweenPlayer))]
    public sealed class TweenPlayerEditor : UnityEditor.Editor
    {
        private TweenPlayer _player;
        private ReorderableList _trackList;

        private SerializedProperty _playOnEnableProp;
        private SerializedProperty _updateTypeProp;
        private SerializedProperty _tracksProp;
        private SerializedProperty _eventsProp;
        private SerializedProperty _onPlayProp;
        private SerializedProperty _onCompleteProp;

        // ── 按钮宽度 ──
        private static readonly GUILayoutOption BtnPlay = GUILayout.Width(60f);
        private static readonly GUILayoutOption BtnStop = GUILayout.Width(50f);
        private static readonly GUILayoutOption BtnRestart = GUILayout.Width(60f);
        private static readonly GUILayoutOption LblTitle = GUILayout.Width(100f);

        // ── 轨道列表布局 ──
        private const float TrackHeaderGap = 4f;

        private void OnEnable()
        {
            _player = (TweenPlayer)target;

            _playOnEnableProp = serializedObject.FindProperty("PlayOnEnable");
            _updateTypeProp = serializedObject.FindProperty("UpdateType");
            _tracksProp = serializedObject.FindProperty("Tracks");
            _eventsProp = serializedObject.FindProperty("Events");
            _onPlayProp = serializedObject.FindProperty("OnPlay");
            _onCompleteProp = serializedObject.FindProperty("OnComplete");

            BuildTrackList();
        }

        private void BuildTrackList()
        {
            _trackList = new ReorderableList(serializedObject, _tracksProp, true, true, true, true);

            _trackList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "Animation Tracks");
            };

            _trackList.drawElementCallback = DrawTrackElement;

            _trackList.elementHeightCallback = GetTrackElementHeight;

            _trackList.onAddCallback = (list) =>
            {
                _tracksProp.arraySize++;
                var elem = _tracksProp.GetArrayElementAtIndex(_tracksProp.arraySize - 1);
                elem.FindPropertyRelative("TrackName").stringValue = $"Track {_tracksProp.arraySize}";
                elem.FindPropertyRelative("Target").objectReferenceValue = null;
                elem.FindPropertyRelative("Clips").ClearArray();
            };
        }

        private void DrawTrackElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _tracksProp.GetArrayElementAtIndex(index);
            var trackNameProp = element.FindPropertyRelative("TrackName");
            var targetProp = element.FindPropertyRelative("Target");
            var clipsProp = element.FindPropertyRelative("Clips");

            float lineH = EditorGUIUtility.singleLineHeight;
            float gap = EditorGUIUtility.standardVerticalSpacing;
            float x = rect.x + 12f;
            float y = rect.y + gap;

            // ── 折叠头行 ──
            string trackLabel = string.IsNullOrEmpty(trackNameProp.stringValue)
                ? $"Track {index + 1}"
                : trackNameProp.stringValue;

            float foldoutW = 120f;
            element.isExpanded = EditorGUI.Foldout(
                new Rect(x, y, foldoutW, lineH),
                element.isExpanded,
                trackLabel,
                true);

            float targetW = Mathf.Max(60f, rect.width - foldoutW - TrackHeaderGap * 2 - 80f);
            EditorGUI.PropertyField(
                new Rect(x + foldoutW + TrackHeaderGap, y, targetW, lineH),
                targetProp,
                GUIContent.none);

            EditorGUI.LabelField(
                new Rect(rect.xMax - 60f, y, 60f, lineH),
                $"{clipsProp.arraySize} clips",
                EditorStyles.miniLabel);

            // ── 展开内容 ──
            if (!element.isExpanded) return;

            EditorGUI.indentLevel++;
            y += lineH + gap;

            EditorGUI.PropertyField(
                new Rect(x, y, rect.width - 12f, lineH),
                trackNameProp,
                new GUIContent("Name"));
            y += lineH + gap;

            EditorGUI.PropertyField(
                new Rect(x, y, rect.width - 12f, EditorGUI.GetPropertyHeight(clipsProp, true)),
                clipsProp,
                new GUIContent("Clips"),
                true);

            EditorGUI.indentLevel--;
        }

        private float GetTrackElementHeight(int index)
        {
            var element = _tracksProp.GetArrayElementAtIndex(index);
            float lineH = EditorGUIUtility.singleLineHeight;
            float gap = EditorGUIUtility.standardVerticalSpacing;

            if (!element.isExpanded)
                return lineH + gap * 2f;

            var clipsProp = element.FindPropertyRelative("Clips");
            float clipsHeight = EditorGUI.GetPropertyHeight(clipsProp, true);

            return lineH * 2f + clipsHeight + gap * 3f;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ── Header + Controls ──
            EditorGUILayout.Space(6f);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Tween Player", EditorStyles.boldLabel, LblTitle);

                bool isPaused = _player.IsPlaying && _player.IsPaused;
                if (GUILayout.Button(isPaused ? "Resume" : _player.IsPlaying ? "Pause" : "Play", BtnPlay))
                {
                    if (_player.IsPlaying && !_player.IsPaused)
                        _player.Pause();
                    else
                        _player.Play();
                }

                if (GUILayout.Button("Stop", BtnStop))
                    _player.Stop();

                if (GUILayout.Button("Restart", BtnRestart))
                    _player.Restart();
            }

            EditorGUILayout.Space(6f);

            // ── Settings ──
            EditorGUILayout.PropertyField(_playOnEnableProp);
            EditorGUILayout.PropertyField(_updateTypeProp);
            EditorGUILayout.Space(4f);

            // ── Tracks ──
            _trackList.DoLayoutList();
            EditorGUILayout.Space(4f);

            // ── Events ──
            EditorGUILayout.PropertyField(_eventsProp, new GUIContent("Sequence Events"), true);
            EditorGUILayout.Space(4f);

            // ── UnityEvents ──
            EditorGUILayout.PropertyField(_onPlayProp);
            EditorGUILayout.PropertyField(_onCompleteProp);

            // ── Runtime Info ──
            if (_player.IsPlaying)
            {
                EditorGUILayout.Space(8f);
                EditorGUILayout.HelpBox(
                    $"Playing{(_player.IsPaused ? " (Paused)" : "")}",
                    _player.IsPaused ? MessageType.Warning : MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
