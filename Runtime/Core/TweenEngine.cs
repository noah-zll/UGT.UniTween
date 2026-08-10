using System.Collections.Generic;
using UnityEngine;

namespace UGT.UniTween.Core
{
    /// <summary>
    /// Tween 调度引擎。场景单例，每帧 Update 驱动所有活跃 Tween。
    /// 首次使用 Tween 时自动创建。
    /// </summary>
    public sealed class TweenEngine : MonoBehaviour
    {
        private static TweenEngine _instance;
        private static readonly object Lock = new object();

        private readonly List<Tween> _activeTweens = new List<Tween>();
        private readonly List<Tween> _deadTweens = new List<Tween>();
        private readonly List<TweenSequence> _activeSequences = new List<TweenSequence>();
        private readonly List<TweenSequence> _deadSequences = new List<TweenSequence>();

        private TweenObjectPool _tweenPool;
        private readonly Stack<TweenSequence> _sequencePool = new Stack<TweenSequence>();
        private float _globalTimeScale = 1f;
        private int _lastTickFrame;

        /// <summary>
        /// 引擎单例。
        /// </summary>
        public static TweenEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Lock)
                    {
                        if (_instance == null)
                        {
                            CreateInstance();
                        }
                    }
                }
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            if (Application.isPlaying)
                CreateInstance();
        }

        private static void CreateInstance()
        {
            var go = new GameObject("[UGT.UniTween Engine]");
            go.hideFlags = HideFlags.HideAndDontSave;
            if (Application.isPlaying)
                DontDestroyOnLoad(go);
            _instance = go.AddComponent<TweenEngine>();
            _instance.Initialize();
        }

        /// <summary>
        /// 全局时间缩放（独立于 Time.timeScale）。
        /// </summary>
        public float TimeScale
        {
            get => _globalTimeScale;
            set
            {
                _globalTimeScale = Mathf.Max(0f, value);
                Tween.GlobalTimeScale = _globalTimeScale;
            }
        }

        /// <summary>
        /// 当前活跃 Tween 数量。
        /// </summary>
        public int ActiveTweenCount => _activeTweens.Count;

        /// <summary>
        /// 当前活跃序列数量。
        /// </summary>
        public int ActiveSequenceCount => _activeSequences.Count;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            if (_tweenPool != null) return;
            _tweenPool = new TweenObjectPool(TweenSettings.Instance.PoolInitialCapacity);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            if (!CanTickThisFrame()) return;
            float dt = Time.deltaTime;
            CoreTick(dt);
        }

        private void LateUpdate()
        {
            if (!CanTickThisFrame()) return;
            float dt = Time.deltaTime;
            CoreTick(dt);
        }

        private void FixedUpdate()
        {
            if (!CanTickThisFrame()) return;
            float dt = Time.fixedDeltaTime;
            CoreTick(dt);
        }

        public void ManualTick(float deltaTime)
        {
            CoreTick(deltaTime);
        }

        private void CoreTick(float deltaTime)
        {
            TickTweens(deltaTime);
            TickSequences(deltaTime);
        }

        private bool CanTickThisFrame()
        {
            if (Time.frameCount == _lastTickFrame) return false;
            _lastTickFrame = Time.frameCount;
            return true;
        }

        private void TickTweens(float deltaTime)
        {
            _deadTweens.Clear();

            for (int i = 0; i < _activeTweens.Count; i++)
            {
                var tween = _activeTweens[i];
                if (!tween.Tick(deltaTime))
                {
                    _deadTweens.Add(tween);
                }
            }

            // 将死亡的 Tween 从活跃列表中移除（它们已自行归还池子）
            for (int i = 0; i < _deadTweens.Count; i++)
            {
                _activeTweens.Remove(_deadTweens[i]);
            }
        }

        private void TickSequences(float deltaTime)
        {
            _deadSequences.Clear();

            for (int i = 0; i < _activeSequences.Count; i++)
            {
                var seq = _activeSequences[i];
                if (seq.IsCompleted)
                {
                    _deadSequences.Add(seq);
                }
                else
                {
                    seq.Tick(deltaTime);
                }
            }

            for (int i = 0; i < _deadSequences.Count; i++)
            {
                _activeSequences.Remove(_deadSequences[i]);
            }
        }

        // ─── 注册 / 注销 ───

        internal void Register(Tween tween)
        {
            if (!_activeTweens.Contains(tween))
            {
                _activeTweens.Add(tween);
            }
        }

        internal void Unregister(Tween tween)
        {
            _activeTweens.Remove(tween);
        }

        internal void RegisterSequence(TweenSequence sequence)
        {
            if (!_activeSequences.Contains(sequence))
            {
                _activeSequences.Add(sequence);
            }
        }

        internal void UnregisterSequence(TweenSequence sequence)
        {
            _activeSequences.Remove(sequence);
        }

        // ─── 池管理 ───

        internal Tween GetTween()
        {
            return _tweenPool.Get();
        }

        internal void ReturnToPool(Tween tween)
        {
            _tweenPool.Return(tween);
        }

        internal TweenSequence GetSequence()
        {
            if (_sequencePool.Count > 0)
            {
                return _sequencePool.Pop();
            }
            return new TweenSequence();
        }

        internal void ReturnSequence(TweenSequence sequence)
        {
            sequence.Reset();
            _sequencePool.Push(sequence);
        }

        // ─── 公开 API ───

        /// <summary>
        /// 销毁所有活跃 Tween 和序列。
        /// </summary>
        public void KillAll()
        {
            for (int i = _activeTweens.Count - 1; i >= 0; i--)
            {
                _activeTweens[i].Kill();
            }

            for (int i = _activeSequences.Count - 1; i >= 0; i--)
            {
                _activeSequences[i].Kill();
            }
        }
    }
}
