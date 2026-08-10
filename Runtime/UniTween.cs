using System;
using UGT.UniTween.Core;
using UGT.UniTween.Plugins;
using UnityEngine;

namespace UGT.UniTween
{
    /// <summary>
    /// UniTween 顶层静态 API，提供快捷创建 Tween 和 Sequence 的入口。
    /// </summary>
    public static class UniTween
    {
        /// <summary>
        /// 创建一个新的 TweenSequence。
        /// </summary>
        public static TweenSequence CreateSequence()
        {
            return TweenEngine.Instance.GetSequence();
        }

        /// <summary>
        /// 创建纯浮点数值 Tween。
        /// </summary>
        public static Tween DoFloat(float from, float to, float duration, Action<float> onUpdate)
        {
            return ValuePlugins.DoFloat(from, to, duration, onUpdate);
        }

        /// <summary>
        /// 创建颜色 Tween。
        /// </summary>
        public static Tween DoColor(Color from, Color to, float duration, Action<Color> onUpdate)
        {
            return ValuePlugins.DoColor(from, to, duration, onUpdate);
        }

        /// <summary>
        /// 创建 Vector3 Tween。
        /// </summary>
        public static Tween DoVector3(Vector3 from, Vector3 to, float duration, Action<Vector3> onUpdate)
        {
            return ValuePlugins.DoVector3(from, to, duration, onUpdate);
        }

        /// <summary>
        /// 销毁所有活跃动画。
        /// </summary>
        public static void KillAll()
        {
            TweenEngine.Instance.KillAll();
        }

        /// <summary>
        /// 全局时间缩放。
        /// </summary>
        public static float TimeScale
        {
            get => TweenEngine.Instance.TimeScale;
            set => TweenEngine.Instance.TimeScale = value;
        }

        /// <summary>
        /// 当前活跃 Tween 总数。
        /// </summary>
        public static int ActiveTweenCount => TweenEngine.Instance.ActiveTweenCount;

        /// <summary>
        /// 当前活跃序列总数。
        /// </summary>
        public static int ActiveSequenceCount => TweenEngine.Instance.ActiveSequenceCount;
    }
}
