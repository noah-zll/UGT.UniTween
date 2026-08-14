using System;
using UnityEngine;

namespace UGT.UniTween.Components
{
    /// <summary>
    /// 单段动画剪辑的数据定义。描述对某个属性的一段插值动画。
    /// </summary>
    [Serializable]
    public sealed class TweenClip
    {
        [Tooltip("剪辑名称")]
        public string ClipName;

        [Tooltip("持续时间")]
        public float Duration = 0.5f;

        [Tooltip("延迟时间")]
        public float Delay;

        [Tooltip("缓动类型")]
        public EaseType Ease = EaseType.OutQuad;

        [Tooltip("自定义动画曲线（Ease 为 Custom 时生效）")]
        public AnimationCurve CustomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("循环次数（0=不循环，-1=无限循环）")]
        public int Loops;

        [Tooltip("循环模式")]
        public LoopType LoopType = LoopType.Restart;

        [Tooltip("目标类型")]
        public TweenTargetType TargetType;

        [Tooltip("属性类型")]
        public TweenPropertyType PropertyType;

        [Tooltip("值模式：绝对或相对")]
        public TweenValueMode ValueMode;

        [Tooltip("是否指定起始值")]
        public bool UseFrom;

        [Tooltip("使用世界坐标（Position 系列 / Path 时使用，勾选即世界坐标，默认局部坐标）")]
        public bool UseWorld;

        [Tooltip("是否使用独立时间缩放")]
        public bool CustomTimeScale;

        [Tooltip("独立时间缩放（CustomTimeScale 启用时生效，1=正常，0.5=半速）")]
        public float TimeScale = 1f;

        [Tooltip("起始值（UseFrom 启用时生效）")]
        public Vector4 FromValue;

        [Tooltip("结束值")]
        public Vector4 ToValue;

        [Tooltip("序列帧精灵列表（SpriteSequence 时使用）")]
        public Sprite[] SpriteFrames;

        [Tooltip("路径点列表（Path 时使用）")]
        public Vector3[] PathPoints;

        public override string ToString()
        {
            return string.IsNullOrEmpty(ClipName) ? $"{PropertyType} ({Duration}s)" : ClipName;
        }
    }
}
