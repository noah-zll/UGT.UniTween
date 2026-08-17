using System;
using System.Collections.Generic;
using UGT.UniTween.Core;
using UGT.UniTween.Plugins;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UGT.UniTween.Components
{
    /// <summary>
    /// 动画序列播放组件。
    /// 挂载到目标 GameObject 上，通过 Inspector 编辑动画序列。
    /// </summary>
    [AddComponentMenu("UGT UniTween/Tween Player")]
    public sealed class TweenPlayer : MonoBehaviour
    {
        // ─── Inspector 配置 ───

        [Tooltip("Enable 时自动播放")]
        public PlayMode PlayOnEnable = PlayMode.None;

        [Tooltip("更新时间模式")]
        public UpdateMode UpdateType = UpdateMode.Normal;

        [Tooltip("动画轨道列表")]
        public List<TweenTrack> Tracks = new List<TweenTrack>();

        [Tooltip("序列事件列表")]
        public List<TweenEvent> Events = new List<TweenEvent>();

        // ─── 事件 ───

        public UnityEvent OnPlay;
        public UnityEvent OnComplete;

        // ─── 运行时状态 ───

        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }

        private TweenSequence _sequence;
        private float _totalDuration;
        private float _elapsedTime;

        // ─── Unity 生命周期 ───

        private void OnEnable()
        {
            if (!Application.isPlaying) return;

            if (PlayOnEnable == PlayMode.Play && IsPlaying)
                return;

            if (PlayOnEnable == PlayMode.Play)
            {
                Play();
            }
            else if (PlayOnEnable == PlayMode.Restart)
            {
                Restart();
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;

            if (IsPlaying)
            {
                Stop();
            }
        }

        private void OnDestroy()
        {
            if (_sequence != null)
            {
                _sequence.Kill();
                _sequence = null;
            }
        }

        // ─── 公开控制方法 ───

        public void Play()
        {
            if (IsPlaying && !IsPaused) return;
            if (Tracks == null || Tracks.Count == 0) return;

            if (_sequence != null && IsPaused)
            {
                _sequence.Resume();
                IsPaused = false;
                return;
            }

            BuildAndPlay();
        }

        public void Pause()
        {
            if (!IsPlaying || IsPaused) return;

            _sequence?.Pause();
            IsPaused = true;
        }

        public void Resume()
        {
            Play();
        }

        public void Stop()
        {
            if (!IsPlaying) return;

            _sequence?.Kill();
            _sequence = null;
            IsPlaying = false;
            IsPaused = false;
            _elapsedTime = 0f;
        }

        public void Restart()
        {
            Stop();
            BuildAndPlay();
        }

        public void Seek(float normalizedTime)
        {
            // 简化实现：通过 Kill 旧序列并快速推进
            // 完整版本需要更精细的 Seek 支持
            normalizedTime = Mathf.Clamp01(normalizedTime);
            float targetElapsed = normalizedTime * _totalDuration;

            Stop();
            // TODO: 实现精确 Seek
            _elapsedTime = targetElapsed;
            _sequence?.Play();
        }

        // ─── 内部实现 ───

        private void BuildAndPlay()
        {
            _sequence = UniTween.CreateSequence();
            _totalDuration = 0f;

            CalculateTotalDuration();

            // 构建每条轨道的 Tweens
            var trackSequences = new List<TweenSequence>();
            foreach (var track in Tracks)
            {
                if (track.Clips == null || track.Clips.Count == 0) continue;

                GameObject target = track.Target != null ? track.Target : gameObject;
                var trackSeq = BuildTrackSequence(target, track.Clips);
                if (trackSeq != null)
                {
                    trackSequences.Add(trackSeq);
                }
            }

            // 所有轨道并行播放
            if (trackSequences.Count == 0)
            {
                _sequence = null;
                return;
            }

            // 第一个轨道直接 Append
            _sequence.Append(trackSequences[0]);
            // 后续轨道 Join（并行）
            for (int i = 1; i < trackSequences.Count; i++)
            {
                _sequence.Join(trackSequences[i]);
            }

            _sequence.OnComplete = OnSequenceComplete;
            _sequence.Play();

            IsPlaying = true;
            IsPaused = false;
            _elapsedTime = 0f;

            OnPlay?.Invoke();
        }

        private void CalculateTotalDuration()
        {
            _totalDuration = 0f;
            foreach (var track in Tracks)
            {
                if (track.Clips == null) continue;

                float trackDuration = 0f;
                foreach (var clip in track.Clips)
                {
                    if (clip == null) continue;
                    float clipTotal = clip.Delay + clip.Duration;
                    if (clip.Loops > 0)
                        clipTotal += clip.Duration * clip.Loops;
                    else if (clip.Loops < 0) // 无限循环不算总时长
                        clipTotal = float.MaxValue;

                    trackDuration += clipTotal;
                }

                if (trackDuration > _totalDuration)
                    _totalDuration = trackDuration;
            }
        }

        private TweenSequence BuildTrackSequence(GameObject target, List<TweenClip> clips)
        {
            if (clips.Count == 0) return null;

            var seq = UniTween.CreateSequence();

            foreach (var clip in clips)
            {
                if (clip.Delay > 0f)
                {
                    seq.AppendInterval(clip.Delay);
                }

                var tween = CreateTweenForClip(target, clip);
                if (tween != null)
                {
                    seq.Append(tween);
                }
            }

            return seq;
        }

        private Tween CreateTweenForClip(GameObject target, TweenClip clip)
        {
            Tween tween = null;
            float duration = clip.Duration;

            // 如果指定了 FromValue，先将目标属性设置到 FromValue，
            // 这样插件在创建 Tween 时捕获的 from 就是用户指定的起始值。
            if (clip.UseFrom)
                ApplyFrom(target, clip);

            switch (clip.TargetType)
            {
                case TweenTargetType.Transform:
                    tween = CreateTransformTween(target.transform, clip, duration);
                    break;
                case TweenTargetType.RectTransform:
                    tween = CreateRectTransformTween(target.GetComponent<RectTransform>(), clip, duration);
                    break;
                case TweenTargetType.CanvasGroup:
                    tween = CreateCanvasGroupTween(target.GetComponent<CanvasGroup>(), clip, duration);
                    break;
                case TweenTargetType.Image:
                    tween = CreateImageTween(target.GetComponent<Image>(), clip, duration);
                    break;
                case TweenTargetType.Text:
                    tween = CreateTextTween(target.GetComponent<Text>(), clip, duration);
                    break;
                case TweenTargetType.SpriteRenderer:
                    tween = CreateSpriteRendererTween(target.GetComponent<SpriteRenderer>(), clip, duration);
                    break;
                case TweenTargetType.SpriteRendererGroup:
                    tween = CreateSpriteRendererGroupTween(target.GetComponent<SpriteRendererGroup>(), clip, duration);
                    break;
            }

            if (tween != null)
            {
                if (clip.Ease == EaseType.Custom && clip.CustomCurve != null)
                    tween.SetEase(clip.CustomCurve);
                else
                    tween.SetEase(clip.Ease);
                tween.SetLoops(clip.Loops, clip.LoopType);

                if (clip.CustomTimeScale)
                    tween.SetTimeScale(clip.TimeScale);

            }

            return tween;
        }

        private Tween CreateTransformTween(Transform target, TweenClip clip, float duration)
        {
            if (target == null) return null;

            Vector3 to = clip.ToValue;
            if (clip.ValueMode == TweenValueMode.Relative)
            {
                Vector3 positionBase = clip.UseWorld ? target.position : target.localPosition;
                Vector3 rotationBase = clip.UseWorld ? target.eulerAngles : target.localEulerAngles;
                switch (clip.PropertyType)
                {
                    case TweenPropertyType.Position:
                        to = positionBase + (Vector3)clip.ToValue;
                        break;
                    case TweenPropertyType.PositionX:
                        to.x = positionBase.x + clip.ToValue.x;
                        break;
                    case TweenPropertyType.PositionY:
                        to.y = positionBase.y + clip.ToValue.y;
                        break;
                    case TweenPropertyType.PositionZ:
                        to.z = positionBase.z + clip.ToValue.z;
                        break;
                    case TweenPropertyType.Scale:
                        to = target.localScale + (Vector3)clip.ToValue;
                        break;
                    case TweenPropertyType.ScaleX:
                        to.x = target.localScale.x + clip.ToValue.x;
                        break;
                    case TweenPropertyType.ScaleY:
                        to.y = target.localScale.y + clip.ToValue.y;
                        break;
                    case TweenPropertyType.ScaleZ:
                        to.z = target.localScale.z + clip.ToValue.z;
                        break;
                    case TweenPropertyType.Rotation:
                        to = rotationBase + (Vector3)clip.ToValue;
                        break;
                    case TweenPropertyType.RotationX:
                        to.x = rotationBase.x + clip.ToValue.x;
                        break;
                    case TweenPropertyType.RotationY:
                        to.y = rotationBase.y + clip.ToValue.y;
                        break;
                    case TweenPropertyType.RotationZ:
                        to.z = rotationBase.z + clip.ToValue.z;
                        break;
                }
            }

            switch (clip.PropertyType)
            {
                case TweenPropertyType.Position: return clip.UseWorld ? target.DoMove(to, duration) : target.DoLocalMove(to, duration);
                case TweenPropertyType.PositionX: return clip.UseWorld ? target.DoMoveX(to.x, duration) : target.DoLocalMoveX(to.x, duration);
                case TweenPropertyType.PositionY: return clip.UseWorld ? target.DoMoveY(to.y, duration) : target.DoLocalMoveY(to.y, duration);
                case TweenPropertyType.PositionZ: return clip.UseWorld ? target.DoMoveZ(to.z, duration) : target.DoLocalMoveZ(to.z, duration);
                case TweenPropertyType.Rotation: return clip.UseWorld ? target.DoRotate(to, duration) : target.DoLocalRotate(to, duration);
                case TweenPropertyType.RotationX: return clip.UseWorld ? target.DoRotateX(to.x, duration) : target.DoLocalRotateX(to.x, duration);
                case TweenPropertyType.RotationY: return clip.UseWorld ? target.DoRotateY(to.y, duration) : target.DoLocalRotateY(to.y, duration);
                case TweenPropertyType.RotationZ: return clip.UseWorld ? target.DoRotateZ(to.z, duration) : target.DoLocalRotateZ(to.z, duration);
                case TweenPropertyType.Scale: return target.DoScale(to, duration);
                case TweenPropertyType.ScaleX: return target.DoScaleX(to.x, duration);
                case TweenPropertyType.ScaleY: return target.DoScaleY(to.y, duration);
                case TweenPropertyType.ScaleZ: return target.DoScaleZ(to.z, duration);
                case TweenPropertyType.Path: return target.DoPath(clip.PathPoints, duration, !clip.UseWorld);
            }

            return null;
        }

        private Tween CreateRectTransformTween(RectTransform target, TweenClip clip, float duration)
        {
            if (target == null) return null;

            Vector2 v2To = new Vector2(clip.ToValue.x, clip.ToValue.y);
            if (clip.ValueMode == TweenValueMode.Relative)
            {
                v2To = GetCurrentRectValue(target, clip.PropertyType) + v2To;
            }

            switch (clip.PropertyType)
            {
                case TweenPropertyType.AnchorPosition: return target.DoAnchorPos(v2To, duration);
                case TweenPropertyType.AnchorMin: return target.DoAnchorMin(v2To, duration);
                case TweenPropertyType.AnchorMax: return target.DoAnchorMax(v2To, duration);
                case TweenPropertyType.SizeDelta: return target.DoSizeDelta(v2To, duration);
                case TweenPropertyType.Pivot: return target.DoPivot(v2To, duration);
            }

            return null;
        }

        private static Vector2 GetCurrentRectValue(RectTransform rt, TweenPropertyType prop)
        {
            switch (prop)
            {
                case TweenPropertyType.AnchorPosition: return rt.anchoredPosition;
                case TweenPropertyType.AnchorMin: return rt.anchorMin;
                case TweenPropertyType.AnchorMax: return rt.anchorMax;
                case TweenPropertyType.SizeDelta: return rt.sizeDelta;
                case TweenPropertyType.Pivot: return rt.pivot;
                default: return Vector2.zero;
            }
        }

        private Tween CreateCanvasGroupTween(CanvasGroup target, TweenClip clip, float duration)
        {
            if (target == null) return null;

            float to = clip.ToValue.x;
            if (clip.ValueMode == TweenValueMode.Relative)
            {
                float current = clip.PropertyType == TweenPropertyType.Alpha ? target.alpha : target.alpha;
                to = current + to;
            }

            switch (clip.PropertyType)
            {
                case TweenPropertyType.Alpha: return target.DoAlpha(to, duration);
                case TweenPropertyType.Fade: return target.DoFade(to, duration);
            }

            return null;
        }

        private Tween CreateImageTween(Image target, TweenClip clip, float duration)
        {
            if (target == null) return null;

            switch (clip.PropertyType)
            {
                case TweenPropertyType.Color:
                    return target.DoColor(new Color(clip.ToValue.x, clip.ToValue.y, clip.ToValue.z, clip.ToValue.w), duration);
                case TweenPropertyType.Fade:
                    {
                        float to = clip.ToValue.x;
                        if (clip.ValueMode == TweenValueMode.Relative)
                            to = target.color.a + to;
                        return target.DoFade(to, duration);
                    }
                case TweenPropertyType.SpriteSequence:
                    {
                        float fps = clip.ToValue.x > 0f ? clip.ToValue.x : 10f;
                        return target.DoSpriteSequence(clip.SpriteFrames, fps);
                    }
            }

            return null;
        }

        private Tween CreateTextTween(Text target, TweenClip clip, float duration)
        {
            if (target == null) return null;

            switch (clip.PropertyType)
            {
                case TweenPropertyType.Color:
                    return target.DoColor(new Color(clip.ToValue.x, clip.ToValue.y, clip.ToValue.z, clip.ToValue.w), duration);
                case TweenPropertyType.Fade:
                    {
                        float to = clip.ToValue.x;
                        if (clip.ValueMode == TweenValueMode.Relative)
                            to = target.color.a + to;
                        return target.DoFade(to, duration);
                    }
            }

            return null;
        }

        private Tween CreateSpriteRendererTween(SpriteRenderer target, TweenClip clip, float duration)
        {
            if (target == null) return null;

            switch (clip.PropertyType)
            {
                case TweenPropertyType.SpriteColor:
                    return target.DoColor(new Color(clip.ToValue.x, clip.ToValue.y, clip.ToValue.z, clip.ToValue.w), duration);
                case TweenPropertyType.SpriteAlpha:
                    {
                        float to = clip.ToValue.x;
                        if (clip.ValueMode == TweenValueMode.Relative)
                            to = target.color.a + to;
                        return target.DoAlpha(to, duration);
                    }
                case TweenPropertyType.FlipX:
                    return target.DoFlipX(clip.ToValue.x > 0.5f, duration);
                case TweenPropertyType.FlipY:
                    return target.DoFlipY(clip.ToValue.y > 0.5f, duration);
                case TweenPropertyType.SpriteSize:
                    {
                        Vector2 to = new Vector2(clip.ToValue.x, clip.ToValue.y);
                        if (clip.ValueMode == TweenValueMode.Relative)
                            to = target.size + to;
                        return target.DoSize(to, duration);
                    }
                case TweenPropertyType.SpriteSequence:
                    {
                        float fps = clip.ToValue.x > 0f ? clip.ToValue.x : 10f;
                        return target.DoSpriteSequence(clip.SpriteFrames, fps);
                    }
            }

            return null;
        }

        private Tween CreateSpriteRendererGroupTween(SpriteRendererGroup target, TweenClip clip, float duration)
        {
            if (target == null) return null;

            switch (clip.PropertyType)
            {
                case TweenPropertyType.SpriteAlpha:
                case TweenPropertyType.Fade:
                case TweenPropertyType.Alpha:
                    {
                        float to = clip.ToValue.x;
                        if (clip.ValueMode == TweenValueMode.Relative)
                            to = target.Alpha + to;
                        return target.DoAlpha(to, duration);
                    }
                case TweenPropertyType.Color:
                case TweenPropertyType.SpriteColor:
                    {
                        Color to = new Color(clip.ToValue.x, clip.ToValue.y, clip.ToValue.z, clip.ToValue.w);
                        if (clip.ValueMode == TweenValueMode.Relative)
                        {
                            var c = target.Color;
                            to = new Color(c.r + clip.ToValue.x, c.g + clip.ToValue.y, c.b + clip.ToValue.z, c.a + clip.ToValue.w);
                        }
                        return target.DoColor(to, duration);
                    }
            }

            return null;
        }

        private void ApplyFrom(GameObject target, TweenClip clip)
        {
            Vector4 f = clip.FromValue;

            switch (clip.TargetType)
            {
                case TweenTargetType.Transform:
                    ApplyFromTransform(target.transform, clip, f);
                    break;
                case TweenTargetType.RectTransform:
                    ApplyFromRectTransform(target.GetComponent<RectTransform>(), clip, f);
                    break;
                case TweenTargetType.CanvasGroup:
                    ApplyFromCanvasGroup(target.GetComponent<CanvasGroup>(), clip, f);
                    break;
                case TweenTargetType.Image:
                    ApplyFromImage(target.GetComponent<Image>(), clip, f);
                    break;
                case TweenTargetType.Text:
                    ApplyFromText(target.GetComponent<Text>(), clip, f);
                    break;
                case TweenTargetType.SpriteRenderer:
                    ApplyFromSpriteRenderer(target.GetComponent<SpriteRenderer>(), clip, f);
                    break;
                case TweenTargetType.SpriteRendererGroup:
                    ApplyFromSpriteRendererGroup(target.GetComponent<SpriteRendererGroup>(), clip, f);
                    break;
            }
        }

        private static void ApplyFromTransform(Transform target, TweenClip clip, Vector4 f)
        {
            if (target == null) return;
            switch (clip.PropertyType)
            {
                case TweenPropertyType.Position:
                    if (clip.UseWorld) target.position = f; else target.localPosition = f;
                    break;
                case TweenPropertyType.PositionX:
                    {
                        var p = clip.UseWorld ? target.position : target.localPosition;
                        p.x = f.x;
                        if (clip.UseWorld) target.position = p; else target.localPosition = p;
                    }
                    break;
                case TweenPropertyType.PositionY:
                    {
                        var p = clip.UseWorld ? target.position : target.localPosition;
                        p.y = f.y;
                        if (clip.UseWorld) target.position = p; else target.localPosition = p;
                    }
                    break;
                case TweenPropertyType.PositionZ:
                    {
                        var p = clip.UseWorld ? target.position : target.localPosition;
                        p.z = f.z;
                        if (clip.UseWorld) target.position = p; else target.localPosition = p;
                    }
                    break;
                case TweenPropertyType.Rotation:
                    if (clip.UseWorld) target.eulerAngles = f; else target.localEulerAngles = f;
                    break;
                case TweenPropertyType.RotationX:
                    {
                        var e = clip.UseWorld ? target.eulerAngles : target.localEulerAngles;
                        e.x = f.x;
                        if (clip.UseWorld) target.eulerAngles = e; else target.localEulerAngles = e;
                    }
                    break;
                case TweenPropertyType.RotationY:
                    {
                        var e = clip.UseWorld ? target.eulerAngles : target.localEulerAngles;
                        e.y = f.y;
                        if (clip.UseWorld) target.eulerAngles = e; else target.localEulerAngles = e;
                    }
                    break;
                case TweenPropertyType.RotationZ:
                    {
                        var e = clip.UseWorld ? target.eulerAngles : target.localEulerAngles;
                        e.z = f.z;
                        if (clip.UseWorld) target.eulerAngles = e; else target.localEulerAngles = e;
                    }
                    break;
                case TweenPropertyType.Scale: target.localScale = f; break;
                case TweenPropertyType.ScaleX: { var s = target.localScale; s.x = f.x; target.localScale = s; } break;
                case TweenPropertyType.ScaleY: { var s = target.localScale; s.y = f.y; target.localScale = s; } break;
                case TweenPropertyType.ScaleZ: { var s = target.localScale; s.z = f.z; target.localScale = s; } break;
            }
        }

        private static void ApplyFromRectTransform(RectTransform target, TweenClip clip, Vector4 f)
        {
            if (target == null) return;
            var v2 = new Vector2(f.x, f.y);
            switch (clip.PropertyType)
            {
                case TweenPropertyType.AnchorPosition: target.anchoredPosition = v2; break;
                case TweenPropertyType.AnchorMin: target.anchorMin = v2; break;
                case TweenPropertyType.AnchorMax: target.anchorMax = v2; break;
                case TweenPropertyType.SizeDelta: target.sizeDelta = v2; break;
                case TweenPropertyType.Pivot: target.pivot = v2; break;
            }
        }

        private static void ApplyFromCanvasGroup(CanvasGroup target, TweenClip clip, Vector4 f)
        {
            if (target == null) return;
            switch (clip.PropertyType)
            {
                case TweenPropertyType.Alpha:
                case TweenPropertyType.Fade: target.alpha = f.x; break;
            }
        }

        private static void ApplyFromImage(Image target, TweenClip clip, Vector4 f)
        {
            if (target == null) return;
            switch (clip.PropertyType)
            {
                case TweenPropertyType.Color: target.color = new Color(f.x, f.y, f.z, f.w); break;
                case TweenPropertyType.Fade: { var c = target.color; c.a = f.x; target.color = c; } break;
                case TweenPropertyType.SpriteSequence:
                    if (clip.SpriteFrames != null && clip.SpriteFrames.Length > 0)
                        target.sprite = clip.SpriteFrames[0];
                    break;
            }
        }

        private static void ApplyFromText(Text target, TweenClip clip, Vector4 f)
        {
            if (target == null) return;
            switch (clip.PropertyType)
            {
                case TweenPropertyType.Color: target.color = new Color(f.x, f.y, f.z, f.w); break;
                case TweenPropertyType.Fade: { var c = target.color; c.a = f.x; target.color = c; } break;
            }
        }

        private static void ApplyFromSpriteRenderer(SpriteRenderer target, TweenClip clip, Vector4 f)
        {
            if (target == null) return;
            switch (clip.PropertyType)
            {
                case TweenPropertyType.SpriteColor: target.color = new Color(f.x, f.y, f.z, f.w); break;
                case TweenPropertyType.SpriteAlpha: { var c = target.color; c.a = f.x; target.color = c; } break;
                case TweenPropertyType.FlipX: target.flipX = f.x > 0.5f; break;
                case TweenPropertyType.FlipY: target.flipY = f.y > 0.5f; break;
                case TweenPropertyType.SpriteSize: target.size = new Vector2(f.x, f.y); break;
                case TweenPropertyType.SpriteSequence:
                    if (clip.SpriteFrames != null && clip.SpriteFrames.Length > 0)
                        target.sprite = clip.SpriteFrames[0];
                    break;
            }
        }

        private static void ApplyFromSpriteRendererGroup(SpriteRendererGroup target, TweenClip clip, Vector4 f)
        {
            if (target == null) return;
            switch (clip.PropertyType)
            {
                case TweenPropertyType.SpriteAlpha:
                case TweenPropertyType.Fade:
                case TweenPropertyType.Alpha:
                    target.Alpha = f.x;
                    break;
                case TweenPropertyType.Color:
                case TweenPropertyType.SpriteColor:
                    target.OverrideColor = true;
                    target.Color = new Color(f.x, f.y, f.z, f.w);
                    break;
            }
        }

        private void OnSequenceComplete()
        {
            IsPlaying = false;
            IsPaused = false;
            _sequence = null;

            OnComplete?.Invoke();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 在 Edit Mode 和 Play Mode 均可预览播放。
        /// </summary>
        public void EditorPreview()
        {
            Restart();
        }
#endif
    }
}
