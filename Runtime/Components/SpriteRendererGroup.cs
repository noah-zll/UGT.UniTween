using UnityEngine;

namespace UGT.UniTween.Components
{
    /// <summary>
    /// 类似 CanvasGroup 的 SpriteRenderer 批量控制组件。
    /// 统一控制自身及所有子级 SpriteRenderer 的透明度、颜色等属性。
    /// </summary>
    [AddComponentMenu("UGT UniTween/Sprite Renderer Group")]
    [ExecuteAlways]
    public sealed class SpriteRendererGroup : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] private float _alpha = 1f;

        [SerializeField] private bool _overrideColor;
        [SerializeField] private Color _color = Color.white;

        private SpriteRenderer[] _renderers;
        private float _lastAlpha = -1f;
        private Color _lastColor = Color.clear;
        private bool _lastOverride;

        /// <summary>
        /// 统一透明度（0=全透明，1=不透明）。
        /// </summary>
        public float Alpha
        {
            get => _alpha;
            set => _alpha = Mathf.Clamp01(value);
        }

        /// <summary>
        /// 是否覆盖颜色。
        /// </summary>
        public bool OverrideColor
        {
            get => _overrideColor;
            set => _overrideColor = value;
        }

        /// <summary>
        /// 覆盖颜色。
        /// </summary>
        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        /// <summary>
        /// 缓存的子级 SpriteRenderer 列表。
        /// </summary>
        public SpriteRenderer[] Renderers => _renderers;

        private void OnEnable()
        {
            RefreshRenderers();
        }

        private void OnTransformChildrenChanged()
        {
            RefreshRenderers();
        }

        private void LateUpdate()
        {
            Apply();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                RefreshRenderers();
                Apply();
            }
        }
#endif

        /// <summary>
        /// 刷新子级 SpriteRenderer 缓存列表。
        /// </summary>
        public void RefreshRenderers()
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _lastAlpha = -1f;
        }

        /// <summary>
        /// 手动应用当前属性到所有子级。
        /// </summary>
        public void Apply()
        {
            if (_alpha == _lastAlpha && _color == _lastColor && _overrideColor == _lastOverride)
                return;

            _lastAlpha = _alpha;
            _lastColor = _color;
            _lastOverride = _overrideColor;

            if (_renderers == null || _renderers.Length == 0) return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r == null) continue;

                var c = r.color;
                c.a = _alpha;
                if (_overrideColor)
                {
                    c.r = _color.r;
                    c.g = _color.g;
                    c.b = _color.b;
                }
                r.color = c;
            }
        }
    }
}
