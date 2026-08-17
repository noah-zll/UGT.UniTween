using UnityEngine;

namespace UGT.UniTween.Components
{
    /// <summary>
    /// 生成模式。
    /// </summary>
    public enum SpawnMode
    {
        /// <summary>禁用，不自动生成。</summary>
        None,
        /// <summary>一次性生成指定数量。</summary>
        Burst,
        /// <summary>按间隔循环生成。</summary>
        Loop,
    }

    /// <summary>
    /// 通过 TweenPlayer 预制体批量生成动画播放实例。
    /// 支持一次性爆发生成和间隔循环生成（使用对象池复用）。
    /// </summary>
    [AddComponentMenu("UGT UniTween/Tween Player Spawner")]
    [ExecuteAlways]
    public sealed class TweenPlayerSpawner : MonoBehaviour
    {
        [Header("预制体")]
        [Tooltip("TweenPlayer 预制体")]
        [SerializeField] private TweenPlayer _prefab;

        [Header("生成配置")]
        [Tooltip("生成模式")]
        [SerializeField] private SpawnMode _mode = SpawnMode.None;
        [Tooltip("爆发模式下的一次生成数量")]
        [SerializeField] private int _burstCount = 5;
        [Tooltip("循环模式的生成间隔（秒）")]
        [SerializeField] private float _spawnInterval = 0.5f;
        [Tooltip("对象池初始容量")]
        [SerializeField] private int _poolCapacity = 10;

        [Header("时间缩放")]
        [Tooltip("是否使用独立时间缩放（忽略全局）")]
        [SerializeField] private bool _useIndependentTimeScale = false;
        [Tooltip("独立时间缩放（启用时生效，1=正常，0.5=半速）")]
        [SerializeField] private float _timeScale = 1f;

        [Header("布局")]
        [Tooltip("生成的实例是否作为当前对象的子节点")]
        [SerializeField] private bool _spawnAsChild = true;
        [Tooltip("生成位置随机偏移范围")]
        [SerializeField] private Vector3 _randomOffset = Vector3.zero;

        private TweenPlayerPool _pool;
        private float _timer;
        private bool _isLooping;

        public SpawnMode CurrentMode => _mode;
        public bool IsLooping => _isLooping;

        private void Awake()
        {
            if (!Application.isPlaying) return;

            if (_mode == SpawnMode.Loop && _prefab != null)
            {
                InitPool();
            }
        }

        private void Start()
        {
            if (!Application.isPlaying) return;

            if (_mode == SpawnMode.Burst)
            {
                SpawnBurst(_burstCount);
            }
            else if (_mode == SpawnMode.Loop)
            {
                StartLoop();
            }
        }

        private void Update()
        {
            if (!_isLooping) return;

            float deltaTime = Time.deltaTime * (_useIndependentTimeScale ? _timeScale : UniTween.TimeScale);
            _timer += deltaTime;
            if (_timer >= _spawnInterval)
            {
                _timer -= _spawnInterval;
                SpawnOne();
            }
        }

        private void OnDestroy()
        {
            _pool?.DestroyAll();
            _pool = null;
        }

        private void OnDisable()
        {
            StopLoop();
        }

        // ─── 公开方法 ───

        /// <summary>
        /// 一次性生成指定数量的实例。
        /// </summary>
        public void SpawnBurst(int count)
        {
            if (_prefab == null)
            {
                Debug.LogWarning("TweenPlayerSpawner: Prefab is null.");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var player = Instantiate(_prefab, GetSpawnParent());
                player.transform.SetLocalPositionAndRotation(GetRandomOffset(), Quaternion.identity);
                player.Play();
            }
        }

        /// <summary>
        /// 开始循环生成（使用对象池）。
        /// </summary>
        public void StartLoop()
        {
            if (_prefab == null)
            {
                Debug.LogWarning("TweenPlayerSpawner: Prefab is null.");
                return;
            }

            if (_pool == null)
                InitPool();

            _isLooping = true;
            _timer = 0f;
        }

        /// <summary>
        /// 停止循环生成并回收所有实例。
        /// </summary>
        public void StopLoop()
        {
            _isLooping = false;
            _timer = 0f;
            _pool?.RecycleAll();
        }

        public void StopLoopByEditor()
        {
            StopLoop();
            _pool?.DestroyAll();
        }

        // ─── 内部方法 ───

        private void InitPool()
        {
            _pool = new TweenPlayerPool(_prefab, GetSpawnParent(), _poolCapacity);
        }

        private void SpawnOne()
        {
            var player = _pool.Spawn();
            player.transform.SetLocalPositionAndRotation(GetRandomOffset(), Quaternion.identity);
        }

        private Transform GetSpawnParent()
        {
            return _spawnAsChild ? transform : null;
        }

        private Vector3 GetRandomOffset()
        {
            if (_randomOffset == Vector3.zero) return Vector3.zero;
            return new Vector3(
                Random.Range(-_randomOffset.x, _randomOffset.x),
                Random.Range(-_randomOffset.y, _randomOffset.y),
                Random.Range(-_randomOffset.z, _randomOffset.z)
            );
        }
    }
}
