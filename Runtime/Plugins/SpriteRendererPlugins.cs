using UGT.UniTween.Components;
using UGT.UniTween.Core;
using UnityEngine;

namespace UGT.UniTween.Plugins
{
    /// <summary>
    /// SpriteRenderer 2D 物体相关的 Tween 插件。
    /// </summary>
    public static class SpriteRendererPlugins
    {
        public static Tween DoColor(this SpriteRenderer target, Color to, float duration)
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

        public static Tween DoFade(this SpriteRenderer target, float to, float duration)
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

        public static Tween DoAlpha(this SpriteRenderer target, float to, float duration)
        {
            return DoFade(target, to, duration);
        }

        public static Tween DoFlipX(this SpriteRenderer target, bool to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            bool from = target.flipX;
            tween.OnStart = () => { from = target.flipX; };
            tween.OnUpdate = (t) =>
            {
                // flip 是离散值，在 t >= 0.5 时切换
                if (t >= 0.5f)
                {
                    if (target != null) target.flipX = to;
                }
                else
                {
                    if (target != null) target.flipX = from;
                }
            };

            return tween;
        }

        public static Tween DoFlipY(this SpriteRenderer target, bool to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            bool from = target.flipY;
            tween.OnStart = () => { from = target.flipY; };
            tween.OnUpdate = (t) =>
            {
                if (t >= 0.5f)
                {
                    if (target != null) target.flipY = to;
                }
                else
                {
                    if (target != null) target.flipY = from;
                }
            };

            return tween;
        }

        public static Tween DoSize(this SpriteRenderer target, Vector2 to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Vector2 from = target.size;
            tween.OnStart = () => { from = target.size; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.size = Vector2.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoSpriteSequence(this SpriteRenderer target, Sprite[] sprites, float fps)
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

        // ─── SpriteRendererGroup ───

        public static Tween DoAlpha(this SpriteRendererGroup target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.Alpha;
            tween.OnStart = () => { from = target.Alpha; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.Alpha = Mathf.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoFade(this SpriteRendererGroup target, float to, float duration)
        {
            return target.DoAlpha(to, duration);
        }

        public static Tween DoColor(this SpriteRendererGroup target, Color to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            target.OverrideColor = true;
            Color from = target.Color;
            tween.OnStart = () => { from = target.Color; target.OverrideColor = true; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.Color = Color.LerpUnclamped(from, to, t);
            };

            return tween;
        }
    }
}
