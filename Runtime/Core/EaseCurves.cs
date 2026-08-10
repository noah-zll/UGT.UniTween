using System;
using UnityEngine;

namespace UGT.UniTween.Core
{
    /// <summary>
    /// 缓动函数库。所有函数接收归一化时间 t (0..1)，返回归一化进度 (0..1)。
    /// </summary>
    public static class EaseCurves
    {
        public delegate float EaseFunction(float t);

        public static readonly EaseFunction[] Functions;

        static EaseCurves()
        {
            Functions = new EaseFunction[Enum.GetValues(typeof(EaseType)).Length];
            Functions[(int)EaseType.Linear] = Linear;

            Functions[(int)EaseType.InSine] = InSine;
            Functions[(int)EaseType.OutSine] = OutSine;
            Functions[(int)EaseType.InOutSine] = InOutSine;

            Functions[(int)EaseType.InQuad] = InQuad;
            Functions[(int)EaseType.OutQuad] = OutQuad;
            Functions[(int)EaseType.InOutQuad] = InOutQuad;

            Functions[(int)EaseType.InCubic] = InCubic;
            Functions[(int)EaseType.OutCubic] = OutCubic;
            Functions[(int)EaseType.InOutCubic] = InOutCubic;

            Functions[(int)EaseType.InQuart] = InQuart;
            Functions[(int)EaseType.OutQuart] = OutQuart;
            Functions[(int)EaseType.InOutQuart] = InOutQuart;

            Functions[(int)EaseType.InQuint] = InQuint;
            Functions[(int)EaseType.OutQuint] = OutQuint;
            Functions[(int)EaseType.InOutQuint] = InOutQuint;

            Functions[(int)EaseType.InExpo] = InExpo;
            Functions[(int)EaseType.OutExpo] = OutExpo;
            Functions[(int)EaseType.InOutExpo] = InOutExpo;

            Functions[(int)EaseType.InCirc] = InCirc;
            Functions[(int)EaseType.OutCirc] = OutCirc;
            Functions[(int)EaseType.InOutCirc] = InOutCirc;

            Functions[(int)EaseType.InBack] = InBack;
            Functions[(int)EaseType.OutBack] = OutBack;
            Functions[(int)EaseType.InOutBack] = InOutBack;

            Functions[(int)EaseType.InElastic] = InElastic;
            Functions[(int)EaseType.OutElastic] = OutElastic;
            Functions[(int)EaseType.InOutElastic] = InOutElastic;

            Functions[(int)EaseType.InBounce] = InBounce;
            Functions[(int)EaseType.OutBounce] = OutBounce;
            Functions[(int)EaseType.InOutBounce] = InOutBounce;
            Functions[(int)EaseType.Custom] = Linear; // 占位，实际由 Tween.CustomCurve 接管
        }

        public static float Evaluate(EaseType ease, float t)
        {
            return Functions[(int)ease](Mathf.Clamp01(t));
        }

        // Linear
        public static float Linear(float t) => t;

        // Sine
        public static float InSine(float t) => 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
        public static float OutSine(float t) => Mathf.Sin(t * Mathf.PI * 0.5f);
        public static float InOutSine(float t) => -0.5f * (Mathf.Cos(Mathf.PI * t) - 1f);

        // Quad
        public static float InQuad(float t) => t * t;
        public static float OutQuad(float t) => t * (2f - t);
        public static float InOutQuad(float t) => t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;

        // Cubic
        public static float InCubic(float t) => t * t * t;
        public static float OutCubic(float t) => --t * t * t + 1f;
        public static float InOutCubic(float t) => t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;

        // Quart
        public static float InQuart(float t) => t * t * t * t;
        public static float OutQuart(float t) => 1f - --t * t * t * t;
        public static float InOutQuart(float t) => t < 0.5f ? 8f * t * t * t * t : 1f - 8f * --t * t * t * t;

        // Quint
        public static float InQuint(float t) => t * t * t * t * t;
        public static float OutQuint(float t) => 1f + --t * t * t * t * t;
        public static float InOutQuint(float t) => t < 0.5f ? 16f * t * t * t * t * t : 1f + 16f * --t * t * t * t * t;

        // Expo
        public static float InExpo(float t) => t <= 0f ? 0f : Mathf.Pow(2f, 10f * (t - 1f));
        public static float OutExpo(float t) => t >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
        public static float InOutExpo(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            return t < 0.5f ? 0.5f * Mathf.Pow(2f, 20f * t - 10f) : 1f - 0.5f * Mathf.Pow(2f, -20f * t + 10f);
        }

        // Circ
        public static float InCirc(float t) => 1f - Mathf.Sqrt(1f - t * t);
        public static float OutCirc(float t) => Mathf.Sqrt(1f - --t * t);
        public static float InOutCirc(float t) => t < 0.5f ? 0.5f * (1f - Mathf.Sqrt(1f - 4f * t * t)) : 0.5f * (Mathf.Sqrt(1f - (--t * t * 4f - 4f * t + 1f)) + 1f);

        // Back
        private const float BackC1 = 1.70158f;
        private const float BackC2 = BackC1 * 1.525f;
        private const float BackC3 = BackC1 + 1f;

        public static float InBack(float t) => BackC3 * t * t * t - BackC1 * t * t;
        public static float OutBack(float t) => 1f + BackC3 * --t * t * t + BackC1 * t * t;
        public static float InOutBack(float t) => t < 0.5f
            ? (t * 2f) * (t * 2f) * ((BackC2 + 1f) * t * 2f - BackC2) * 0.5f
            : ((t * 2f - 2f) * (t * 2f - 2f) * ((BackC2 + 1f) * (t * 2f - 2f) + BackC2) + 2f) * 0.5f;

        // Elastic
        private const float ElasticC4 = 2f * Mathf.PI / 3f;
        private const float ElasticC5 = 2f * Mathf.PI / 4.5f;

        public static float InElastic(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * ElasticC4);
        }

        public static float OutElastic(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * ElasticC4) + 1f;
        }

        public static float InOutElastic(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            return t < 0.5f
                ? -0.5f * Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * ElasticC5)
                : 0.5f * Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * ElasticC5) + 1f;
        }

        // Bounce
        public static float InBounce(float t) => 1f - OutBounce(1f - t);

        public static float OutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
            if (t < 2.5f / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }

        public static float InOutBounce(float t) => t < 0.5f
            ? 0.5f * InBounce(t * 2f)
            : 0.5f * OutBounce(t * 2f - 1f) + 0.5f;
    }
}
