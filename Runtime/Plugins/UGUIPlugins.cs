using UGT.UniTween.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGT.UniTween.Plugins
{
    /// <summary>
    /// UGUI 组件（CanvasGroup / Image / Text）相关的 Tween 插件。
    /// </summary>
    public static class UGUIPlugins
    {
        // ─── CanvasGroup ───

        public static Tween DoFade(this CanvasGroup target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.alpha;
            tween.OnStart = () => { from = target.alpha; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.alpha = Mathf.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoAlpha(this CanvasGroup target, float to, float duration)
        {
            return DoFade(target, to, duration);
        }

        // ─── Image ───

        public static Tween DoColor(this Image target, Color to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Color from = target.color;
            tween.OnStart = () => { from = target.color; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.color = Color.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoFade(this Image target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.color.a;
            tween.OnStart = () => { from = target.color.a; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var c = target.color;
                    c.a = Mathf.LerpUnclamped(from, to, t);
                    target.color = c;
                }
            };

            return tween;
        }

        public static Tween DoFillAmount(this Image target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.fillAmount;
            tween.OnStart = () => { from = target.fillAmount; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.fillAmount = Mathf.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        // ─── Text (legacy) ───

        public static Tween DoColor(this Text target, Color to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Color from = target.color;
            tween.OnStart = () => { from = target.color; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.color = Color.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoFade(this Text target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.color.a;
            tween.OnStart = () => { from = target.color.a; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var c = target.color;
                    c.a = Mathf.LerpUnclamped(from, to, t);
                    target.color = c;
                }
            };

            return tween;
        }

        public static Tween DoSpriteSequence(this Image target, Sprite[] sprites, float fps)
        {
            if (sprites == null || sprites.Length == 0) return null;

            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = sprites.Length / Mathf.Max(0.001f, fps);

            tween.OnUpdate = (t) =>
            {
                if (target == null) return;
                int index = Mathf.Min((int)(t * sprites.Length), sprites.Length - 1);
                target.sprite = sprites[index];
            };

            return tween;
        }
    }
}
