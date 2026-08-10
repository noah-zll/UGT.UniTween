using UGT.UniTween.Core;
using UnityEditor;
using UnityEngine;

namespace UGT.UniTween.Editor
{
    /// <summary>
    /// 在 Edit Mode 下驱动 TweenEngine，使动画在非运行时也能播放预览。
    /// </summary>
    [InitializeOnLoad]
    internal static class EditorTweenDriver
    {
        private static float _lastRealtime;

        static EditorTweenDriver()
        {
            _lastRealtime = (float)EditorApplication.timeSinceStartup;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // 确保引擎实例在 Edit Mode 下也创建
            var engine = TweenEngine.Instance;
        }

        private static void OnEditorUpdate()
        {
            if (Application.isPlaying) return;

            float now = (float)EditorApplication.timeSinceStartup;
            float dt = Mathf.Min(now - _lastRealtime, 0.1f);
            _lastRealtime = now;

            TweenEngine.Instance.ManualTick(dt);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                TweenEngine.Instance.KillAll();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                _lastRealtime = (float)EditorApplication.timeSinceStartup;
            }
        }
    }
}
