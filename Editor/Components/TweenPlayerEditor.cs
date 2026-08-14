using UGT.UniTween;
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

        public void OnSceneGUI()
        {
            if (_player == null) return;

            serializedObject.Update();

            for (int t = 0; t < _tracksProp.arraySize; t++)
            {
                var track = _tracksProp.GetArrayElementAtIndex(t);
                var clipsProp = track.FindPropertyRelative("Clips");

                var targetProp = track.FindPropertyRelative("Target");
                Transform target = targetProp.objectReferenceValue != null
                    ? ((GameObject)targetProp.objectReferenceValue).transform
                    : _player.transform;

                for (int c = 0; c < clipsProp.arraySize; c++)
                {
                    var clip = clipsProp.GetArrayElementAtIndex(c);
                    var propType = (TweenPropertyType)clip.FindPropertyRelative("PropertyType").enumValueIndex;
                    if (propType != TweenPropertyType.Path) continue;

                    var pathPointsProp = clip.FindPropertyRelative("PathPoints");
                    bool useLocal = clip.FindPropertyRelative("UseLocalPath").boolValue;
                    DrawPath(pathPointsProp, target, useLocal);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPath(SerializedProperty pathPointsProp, Transform target, bool useLocal)
        {
            int count = pathPointsProp.arraySize;
            if (count < 2) return;

            Vector3[] points = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                points[i] = pathPointsProp.GetArrayElementAtIndex(i).vector3Value;
            }

            // Local 模式下，路径点以目标局部坐标存储，需转换到世界空间用于显示与编辑。
            bool toWorld = useLocal && target != null;
            Quaternion handleRotation = toWorld ? target.rotation : Quaternion.identity;
            if (toWorld)
            {
                for (int i = 0; i < count; i++)
                    points[i] = target.TransformPoint(points[i]);
            }

            // 编辑点（位置手柄）
            for (int i = 0; i < count; i++)
            {
                Handles.color = (i == 0 || i == count - 1) ? Color.yellow : Color.white;

                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(points[i], handleRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Vector3 stored = toWorld ? target.InverseTransformPoint(newPos) : newPos;
                    pathPointsProp.GetArrayElementAtIndex(i).vector3Value = stored;
                    points[i] = newPos;
                }

                float size = HandleUtility.GetHandleSize(points[i]) * 0.06f;
                Handles.SphereHandleCap(0, points[i], Quaternion.identity, size, EventType.Repaint);

                Handles.Label(points[i] + Vector3.up * size * 2f, i.ToString());
            }

            // 绘制曲线（Catmull-Rom）
            Handles.color = new Color(0f, 1f, 1f, 0.6f);
            int samples = Mathf.Max(count * 16, 64);
            Vector3[] curve = new Vector3[samples + 1];
            for (int i = 0; i <= samples; i++)
            {
                curve[i] = EvaluateCatmullRomPath(points, (float)i / samples);
            }
            Handles.DrawAAPolyLine(4f, curve);
        }

        private static Vector3 EvaluateCatmullRomPath(Vector3[] points, float t)
        {
            int n = points.Length;
            if (n == 1) return points[0];
            if (n == 2) return Vector3.Lerp(points[0], points[1], t);

            int segmentCount = n - 1;
            float seg = Mathf.Clamp01(t) * segmentCount;
            int segIndex = Mathf.Min((int)seg, segmentCount - 1);
            float localT = seg - segIndex;

            Vector3 p0 = points[Mathf.Max(segIndex - 1, 0)];
            Vector3 p1 = points[segIndex];
            Vector3 p2 = points[segIndex + 1];
            Vector3 p3 = points[Mathf.Min(segIndex + 2, n - 1)];

            return CatmullRom(p0, p1, p2, p3, localT);
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
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
