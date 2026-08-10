using UGT.UniTween.Core;
using UnityEditor;
using UnityEngine;

namespace UGT.UniTween.Editor
{
    /// <summary>
    /// 缓动曲线预览工具。
    /// </summary>
    internal static class EaseCurvePreviewer
    {
        /// <summary>
        /// 在 GUI 绘制缓动曲线预览图。
        /// </summary>
        public static void DrawEaseCurvePreview(Rect rect, EaseType ease)
        {
            if (rect.width <= 0 || rect.height <= 0) return;

            int steps = Mathf.Max(2, (int)rect.width);
            var points = new Vector3[steps];

            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / (steps - 1);
                float y = EaseCurves.Evaluate(ease, t);
                points[i] = new Vector3(
                    rect.x + t * rect.width,
                    rect.y + (1f - y) * rect.height,
                    0f);
            }

            Handles.color = new Color(0.2f, 0.6f, 1f, 0.8f);
            Handles.DrawAAPolyLine(2f, points);

            // 绘制基准对角线
            var diagPoints = new Vector3[2]
            {
                new Vector3(rect.x, rect.y + rect.height, 0f),
                new Vector3(rect.x + rect.width, rect.y, 0f),
            };
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            Handles.DrawAAPolyLine(1f, diagPoints);
        }
    }
}
