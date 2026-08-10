using UGT.UniTween.Core;
using UnityEngine;

namespace UGT.UniTween.Plugins
{
    /// <summary>
    /// RectTransform 相关的 Tween 插件。
    /// </summary>
    public static class RectTransformPlugins
    {
        public static Tween DoAnchorPos(this RectTransform target, Vector2 to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Vector2 from = target.anchoredPosition;
            tween.OnStart = () => { from = target.anchoredPosition; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.anchoredPosition = Vector2.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoAnchorPosX(this RectTransform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.anchoredPosition.x;
            tween.OnStart = () => { from = target.anchoredPosition.x; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var pos = target.anchoredPosition;
                    pos.x = Mathf.LerpUnclamped(from, to, t);
                    target.anchoredPosition = pos;
                }
            };

            return tween;
        }

        public static Tween DoAnchorPosY(this RectTransform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.anchoredPosition.y;
            tween.OnStart = () => { from = target.anchoredPosition.y; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var pos = target.anchoredPosition;
                    pos.y = Mathf.LerpUnclamped(from, to, t);
                    target.anchoredPosition = pos;
                }
            };

            return tween;
        }

        public static Tween DoAnchorMin(this RectTransform target, Vector2 to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Vector2 from = target.anchorMin;
            tween.OnStart = () => { from = target.anchorMin; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.anchorMin = Vector2.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoAnchorMax(this RectTransform target, Vector2 to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Vector2 from = target.anchorMax;
            tween.OnStart = () => { from = target.anchorMax; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.anchorMax = Vector2.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoSizeDelta(this RectTransform target, Vector2 to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Vector2 from = target.sizeDelta;
            tween.OnStart = () => { from = target.sizeDelta; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.sizeDelta = Vector2.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoPivot(this RectTransform target, Vector2 to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Vector2 from = target.pivot;
            tween.OnStart = () => { from = target.pivot; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.pivot = Vector2.LerpUnclamped(from, to, t);
            };

            return tween;
        }
    }
}
