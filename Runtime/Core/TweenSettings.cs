using UnityEngine;

namespace UGT.UniTween.Core
{
    /// <summary>
    /// 全局 Tween 配置。
    /// </summary>
    public sealed class TweenSettings : ScriptableObject
    {
        private static TweenSettings _instance;

        public static TweenSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<TweenSettings>("UGT/TweenSettings");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<TweenSettings>();
                        _instance.name = "TweenSettings";
                    }
                }
                return _instance;
            }
        }

        [SerializeField]
        [Tooltip("默认缓动类型")]
        private EaseType _defaultEase = EaseType.OutQuad;

        [SerializeField]
        [Tooltip("最大同时活跃 Tween 数量（超过则扩容池）")]
        private int _maxActiveTweens = 512;

        [SerializeField]
        [Tooltip("初始池容量")]
        private int _poolInitialCapacity = 64;

        [SerializeField]
        [Tooltip("Tween 超时自动回收时间（秒，0=禁用）")]
        private float _timeoutSeconds = 0f;

        public EaseType DefaultEase => _defaultEase;
        public int MaxActiveTweens => _maxActiveTweens;
        public int PoolInitialCapacity => _poolInitialCapacity;
        public float TimeoutSeconds => _timeoutSeconds;
    }
}
