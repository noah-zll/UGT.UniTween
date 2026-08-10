using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGT.UniTween.Core
{
    /// <summary>
    /// Tween 序列。管理多个 Tween 的串行/并行编排。
    /// </summary>
    public sealed class TweenSequence
    {
        private readonly List<TweenEntry> _entries = new List<TweenEntry>();
        private int _currentIndex;
        private bool _isPlaying;
        private bool _isPaused;
        private float _sequenceTime;

        public bool IsPlaying => _isPlaying && !_isPaused;
        public bool IsCompleted { get; private set; }

        public Action OnComplete { get; set; }

        private struct TweenEntry
        {
            public Tween Tween;
            public TweenSequence Sequence;
            public bool IsInterval;
            public float IntervalDuration;
            public bool IsCallback;
            public Action Callback;
            public bool IsJoin; // 与上一项并行
        }

        internal void Reset()
        {
            _entries.Clear();
            _currentIndex = 0;
            _isPlaying = false;
            _isPaused = false;
            _sequenceTime = 0f;
            IsCompleted = false;
            OnComplete = null;
        }

        public TweenSequence Append(Tween tween)
        {
            if (tween == null) return this;
            _entries.Add(new TweenEntry { Tween = tween });
            return this;
        }

        public TweenSequence Append(TweenSequence sequence)
        {
            if (sequence == null) return this;
            _entries.Add(new TweenEntry { Sequence = sequence });
            return this;
        }

        public TweenSequence AppendInterval(float duration)
        {
            _entries.Add(new TweenEntry { IsInterval = true, IntervalDuration = duration });
            return this;
        }

        public TweenSequence AppendCallback(Action callback)
        {
            _entries.Add(new TweenEntry { IsCallback = true, Callback = callback });
            return this;
        }

        public TweenSequence Join(Tween tween)
        {
            if (tween == null) return this;
            _entries.Add(new TweenEntry { Tween = tween, IsJoin = true });
            return this;
        }

        public TweenSequence Join(TweenSequence sequence)
        {
            if (sequence == null) return this;
            _entries.Add(new TweenEntry { Sequence = sequence, IsJoin = true });
            return this;
        }

        public TweenSequence Insert(float atPosition, Tween tween)
        {
            // 简化实现：直接追加（完整版本需按时间位置排序）
            Append(tween);
            return this;
        }

        public TweenSequence Play()
        {
            if (_entries.Count == 0) return this;

            _isPlaying = true;
            _isPaused = false;
            _currentIndex = 0;
            IsCompleted = false;
            _sequenceTime = 0f;

            StartNextEntry();
            TweenEngine.Instance.RegisterSequence(this);
            return this;
        }

        public void Pause()
        {
            _isPaused = true;
            var current = GetCurrentEntry();
            current?.Tween?.Pause();
        }

        public void Resume()
        {
            _isPaused = false;
            var current = GetCurrentEntry();
            current?.Tween?.Resume();
        }

        public void Kill(bool complete = false)
        {
            _isPlaying = false;
            IsCompleted = true;
            TweenEngine.Instance.UnregisterSequence(this);

            foreach (var entry in _entries)
            {
                if (entry.Tween != null)
                {
                    entry.Tween.Kill(complete);
                }
                else if (entry.Sequence != null)
                {
                    entry.Sequence.Kill(complete);
                }
            }
        }

        public void Complete()
        {
            Kill(true);
        }

        private TweenEntry? GetCurrentEntry()
        {
            if (_currentIndex < _entries.Count)
                return _entries[_currentIndex];
            return null;
        }

        private bool EntryHasContent(TweenEntry entry)
        {
            return entry.Tween != null || entry.Sequence != null;
        }

        private void StartNextEntry()
        {
            if (!_isPlaying || _isPaused || IsCompleted) return;

            // 跳过已完成
            while (_currentIndex < _entries.Count)
            {
                var entry = _entries[_currentIndex];

                if (entry.IsInterval)
                {
                    _sequenceTime = 0f;
                    return; // 由 Tick 处理间隔
                }

                if (entry.IsCallback)
                {
                    entry.Callback?.Invoke();
                    _currentIndex++;
                    continue;
                }

                if (EntryHasContent(entry))
                {
                    if (!entry.IsJoin)
                    {
                        // 串行：直接播放
                        PlayEntry(entry, OnTweenComplete);
                        // 同时启动当前项之后的所有 Join 项
                        StartJoinGroup(_currentIndex);
                    }
                    return; // 等待完成回调
                }

                _currentIndex++;
            }

            // 所有条目执行完毕
            CompleteSequence();
        }

        private void StartJoinGroup(int startIndex)
        {
            int next = startIndex + 1;
            while (next < _entries.Count && _entries[next].IsJoin)
            {
                var joinEntry = _entries[next];
                if (EntryHasContent(joinEntry))
                {
                    PlayEntry(joinEntry, OnJoinTweenComplete);
                }
                next++;
            }
        }

        private void PlayEntry(TweenEntry entry, Action onComplete)
        {
            if (entry.Tween != null)
            {
                entry.Tween.OnComplete += onComplete;
                entry.Tween.Play();
            }
            else if (entry.Sequence != null)
            {
                entry.Sequence.OnComplete = onComplete;
                entry.Sequence.Play();
            }
        }

        private void OnTweenComplete()
        {
            _currentIndex++;

            // 跳过 Join 项（它们已随当前项一起播放）
            while (_currentIndex < _entries.Count && _entries[_currentIndex].IsJoin)
            {
                _currentIndex++;
            }

            StartNextEntry();
        }

        private void OnJoinTweenComplete()
        {
            // Join Tween 完成时不需要推动进度，仅做清理
        }

        private void CompleteSequence()
        {
            _isPlaying = false;
            IsCompleted = true;
            OnComplete?.Invoke();
            TweenEngine.Instance.UnregisterSequence(this);
            TweenEngine.Instance.ReturnSequence(this);
        }

        internal void Tick(float deltaTime)
        {
            if (!_isPlaying || _isPaused || IsCompleted) return;

            // 处理 Interval 条目
            while (_currentIndex < _entries.Count)
            {
                var entry = _entries[_currentIndex];
                if (!entry.IsInterval) break;

                _sequenceTime += deltaTime;
                if (_sequenceTime >= entry.IntervalDuration)
                {
                    _sequenceTime = 0f;
                    _currentIndex++;
                    StartNextEntry();
                    return;
                }
                break;
            }
        }
    }
}
