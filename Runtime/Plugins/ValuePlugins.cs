using System;
using UGT.UniTween.Core;
using UnityEngine;

namespace UGT.UniTween.Plugins
{
    /// <summary>
    /// 纯数值插值、颜色插值等不依赖特定组件的 Tween 插件。
    /// </summary>
    public static class ValuePlugins
    {
        public static Tween DoFloat(float from, float to, float duration, Action<float> onUpdate)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;
            tween.OnUpdate = (t) => onUpdate?.Invoke(Mathf.LerpUnclamped(from, to, t));
            return tween;
        }

        public static Tween DoInt(int from, int to, float duration, Action<int> onUpdate)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;
            tween.OnUpdate = (t) => onUpdate?.Invoke(Mathf.RoundToInt(Mathf.LerpUnclamped(from, to, t)));
            return tween;
        }

        public static Tween DoVector2(Vector2 from, Vector2 to, float duration, Action<Vector2> onUpdate)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;
            tween.OnUpdate = (t) => onUpdate?.Invoke(Vector2.LerpUnclamped(from, to, t));
            return tween;
        }

        public static Tween DoVector3(Vector3 from, Vector3 to, float duration, Action<Vector3> onUpdate)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;
            tween.OnUpdate = (t) => onUpdate?.Invoke(Vector3.LerpUnclamped(from, to, t));
            return tween;
        }

        public static Tween DoColor(Color from, Color to, float duration, Action<Color> onUpdate)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;
            tween.OnUpdate = (t) => onUpdate?.Invoke(Color.LerpUnclamped(from, to, t));
            return tween;
        }
    }
}
