using System;
using UnityEngine;

namespace UGT.UniTween.Core
{
    /// <summary>
    /// 单段 Tween 实例。封装插值逻辑、缓动、回调。
    /// 支持链式组合为序列。从 TweenObjectPool 中获取和归还。
    /// </summary>
    public sealed class Tween
    {
        // 状态枚举
        internal enum State : byte
        {
            Created,
            Waiting,
            Playing,
            Paused,
            Completed,
            Recycled,
        }

        // ─── 公开属性 ───
        public float Duration { get; set; } = 1f;
        public float Delay { get; set; }
        public int Loops { get; set; }
        public LoopType LoopType { get; set; } = LoopType.Restart;
        public EaseType Ease { get; set; }
        public AnimationCurve CustomCurve { get; set; }

        /// <summary>独立时间缩放（1 = 正常，0.5 = 半速，2 = 双倍速）。</summary>
        public float TimeScale { get; set; } = 1f;

        /// <summary>是否使用独立时间缩放（忽略全局）。</summary>
        public bool UseIndependentTimeScale { get; set; }

        /// <summary>全局时间缩放，由 TweenEngine 设置。</summary>
        internal static float GlobalTimeScale { get; set; } = 1f;

        // 回调
        public Action<float> OnUpdate { get; set; }
        public Action OnStart { get; set; }
        public Action OnComplete { get; set; }
        public Action OnKill { get; set; }

        // ─── 内部状态 ───
        internal State CurrentState { get; private set; } = State.Created;
        internal float Elapsed { get; private set; }
        internal float CurrentLoopTime => Mathf.Clamp01(Elapsed / Duration);
        internal int CurrentLoop { get; private set; }
        internal bool IsReversed { get; private set; }
        internal bool IsAlive => CurrentState != State.Recycled && CurrentState != State.Completed;
        internal bool IsActive => CurrentState == State.Playing || CurrentState == State.Waiting;

        public Tween()
        {
            Ease = TweenSettings.Instance.DefaultEase;
        }

        internal void Reset()
        {
            Duration = 1f;
            Delay = 0f;
            Loops = 0;
            LoopType = LoopType.Restart;
            Ease = TweenSettings.Instance.DefaultEase;
            CustomCurve = null;
            OnUpdate = null;
            OnStart = null;
            OnComplete = null;
            OnKill = null;
            Elapsed = 0f;
            CurrentLoop = 0;
            IsReversed = false;
            TimeScale = 1f;
            UseIndependentTimeScale = false;
            CurrentState = State.Created;
        }

        // ─── 控制方法 ───

        public Tween Play()
        {
            if (CurrentState == State.Recycled) return this;

            if (CurrentState == State.Created || CurrentState == State.Completed)
            {
                CurrentState = State.Waiting;
                Elapsed = -Delay;
                CurrentLoop = 0;
                IsReversed = false;
                TweenEngine.Instance.Register(this);
            }
            else if (CurrentState == State.Paused)
            {
                CurrentState = State.Playing;
            }

            return this;
        }

        public Tween Pause()
        {
            if (CurrentState == State.Playing || CurrentState == State.Waiting)
            {
                CurrentState = State.Paused;
            }
            return this;
        }

        public Tween Resume()
        {
            if (CurrentState == State.Paused)
            {
                CurrentState = Elapsed < 0f ? State.Waiting : State.Playing;
            }
            return this;
        }

        public void Kill(bool complete = false)
        {
            if (complete)
            {
                Complete();
                return;
            }

            if (CurrentState == State.Recycled || CurrentState == State.Completed)
                return;

            TweenEngine.Instance.Unregister(this);
            OnKill?.Invoke();
            CurrentState = State.Completed;
            TweenEngine.Instance.ReturnToPool(this);
        }

        public void Complete()
        {
            if (CurrentState == State.Recycled || CurrentState == State.Completed)
                return;

            TweenEngine.Instance.Unregister(this);

            // 直接驱动到终点
            float endValue = (LoopType == LoopType.Yoyo && Loops % 2 != 0)
                ? 0f : 1f;
            float eased = EaseCurves.Evaluate(Ease, endValue);
            OnUpdate?.Invoke(eased);

            OnComplete?.Invoke();
            CurrentState = State.Completed;
            TweenEngine.Instance.ReturnToPool(this);
        }

        // ─── 链式配置 ───

        public Tween SetEase(EaseType ease)
        {
            Ease = ease;
            return this;
        }

        public Tween SetEase(AnimationCurve curve)
        {
            Ease = EaseType.Custom;
            CustomCurve = curve;
            return this;
        }

        public Tween SetDelay(float delay)
        {
            Delay = delay;
            return this;
        }

        public Tween SetDuration(float duration)
        {
            Duration = Mathf.Max(0.001f, duration);
            return this;
        }

        public Tween SetLoops(int loops, LoopType loopType = LoopType.Restart)
        {
            Loops = loops;
            LoopType = loopType;
            return this;
        }

        public Tween SetTimeScale(float timeScale)
        {
            TimeScale = timeScale;
            UseIndependentTimeScale = true;
            return this;
        }

        public Tween SetOnStart(Action onStart)
        {
            OnStart = onStart;
            return this;
        }

        public Tween SetOnComplete(Action onComplete)
        {
            OnComplete = onComplete;
            return this;
        }

        public Tween SetOnKill(Action onKill)
        {
            OnKill = onKill;
            return this;
        }

        public Tween SetOnUpdate(Action<float> onUpdate)
        {
            OnUpdate = onUpdate;
            return this;
        }

        // ─── 序列链式 ───

        public TweenSequence Append(Tween next)
        {
            return ToSequence().Append(next);
        }

        public TweenSequence Join(Tween parallel)
        {
            return ToSequence().Join(parallel);
        }

        public TweenSequence ToSequence()
        {
            var seq = TweenEngine.Instance.GetSequence();
            seq.Append(this);
            return seq;
        }

        // ─── 内部 Tick ───

        internal bool Tick(float deltaTime)
        {
            if (CurrentState != State.Playing && CurrentState != State.Waiting)
                return false;

            Elapsed += deltaTime * (UseIndependentTimeScale ? TimeScale : GlobalTimeScale);

            // 等待延迟
            if (Elapsed < 0f)
                return false;

            // 首次进入播放阶段
            if (CurrentState == State.Waiting)
            {
                CurrentState = State.Playing;
                OnStart?.Invoke();
            }

            float t = CurrentLoopTime;

            // 处理循环
            if (t >= 1f)
            {
                if (Loops < 0 || CurrentLoop < Loops)
                {
                    CurrentLoop++;
                    Elapsed = 0f;

                    if (LoopType == LoopType.Yoyo)
                    {
                        IsReversed = !IsReversed;
                    }

                    t = 0f;

                    if (Loops > 0 && CurrentLoop > Loops)
                    {
                        Finish();
                        return false;
                    }
                }
                else
                {
                    Finish();
                    return false;
                }
            }

            // Yoyo 反向时反转 t
            float rawT = IsReversed ? 1f - t : t;
            float easedT = EvaluateEase(rawT);
            OnUpdate?.Invoke(easedT);

            return true;
        }

        private float EvaluateEase(float t)
        {
            if (Ease == EaseType.Custom && CustomCurve != null)
            {
                return CustomCurve.Evaluate(t);
            }
            return EaseCurves.Evaluate(Ease, t);
        }

        private void Finish()
        {
            // 确保到达终点
            float endValue = IsReversed ? 0f : 1f;
            float eased = EvaluateEase(endValue);
            OnUpdate?.Invoke(eased);

            OnComplete?.Invoke();
            CurrentState = State.Completed;
            TweenEngine.Instance.Unregister(this);
            TweenEngine.Instance.ReturnToPool(this);
        }

        // ─── 序列 API ───

        public Tween AppendInterval(float duration) => throw new NotSupportedException("Use TweenSequence for AppendInterval");
        public Tween AppendCallback(Action callback) => throw new NotSupportedException("Use TweenSequence for AppendCallback");
    }
}
