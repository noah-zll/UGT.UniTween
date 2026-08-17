using UGT.UniTween.Core;
using UnityEngine;

namespace UGT.UniTween.Plugins
{
    /// <summary>
    /// Transform 相关的 Tween 插件。
    /// </summary>
    public static class TransformPlugins
    {
        public static Tween DoMove(this Transform target, Vector3 to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Vector3 from = target.position;

            tween.OnStart = () => { from = target.position; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.position = Vector3.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoMoveX(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.position.x;
            tween.OnStart = () => { from = target.position.x; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var pos = target.position;
                    pos.x = Mathf.LerpUnclamped(from, to, t);
                    target.position = pos;
                }
            };

            return tween;
        }

        public static Tween DoMoveY(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.position.y;
            tween.OnStart = () => { from = target.position.y; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var pos = target.position;
                    pos.y = Mathf.LerpUnclamped(from, to, t);
                    target.position = pos;
                }
            };

            return tween;
        }

        public static Tween DoMoveZ(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.position.z;
            tween.OnStart = () => { from = target.position.z; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var pos = target.position;
                    pos.z = Mathf.LerpUnclamped(from, to, t);
                    target.position = pos;
                }
            };

            return tween;
        }

        public static Tween DoLocalMove(this Transform target, Vector3 to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Vector3 from = target.localPosition;
            tween.OnStart = () => { from = target.localPosition; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.localPosition = Vector3.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoLocalMoveX(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.localPosition.x;
            tween.OnStart = () => { from = target.localPosition.x; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var pos = target.localPosition;
                    pos.x = Mathf.LerpUnclamped(from, to, t);
                    target.localPosition = pos;
                }
            };

            return tween;
        }

        public static Tween DoLocalMoveY(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.localPosition.y;
            tween.OnStart = () => { from = target.localPosition.y; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var pos = target.localPosition;
                    pos.y = Mathf.LerpUnclamped(from, to, t);
                    target.localPosition = pos;
                }
            };

            return tween;
        }

        public static Tween DoLocalMoveZ(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.localPosition.z;
            tween.OnStart = () => { from = target.localPosition.z; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var pos = target.localPosition;
                    pos.z = Mathf.LerpUnclamped(from, to, t);
                    target.localPosition = pos;
                }
            };

            return tween;
        }

        public static Tween DoRotate(this Transform target, Vector3 to, float duration, bool useQuaternion = false)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            if (useQuaternion)
            {
                Quaternion fromRot = target.rotation;
                Quaternion toRot = Quaternion.Euler(to);
                tween.OnStart = () => { fromRot = target.rotation; };
                tween.OnUpdate = (t) =>
                {
                    if (target != null)
                        target.rotation = Quaternion.SlerpUnclamped(fromRot, toRot, t);
                };
            }
            else
            {
                Vector3 from = target.eulerAngles;
                tween.OnStart = () => { from = target.eulerAngles; };
                tween.OnUpdate = (t) =>
                {
                    if (target != null)
                        target.eulerAngles = Vector3.LerpUnclamped(from, to, t);
                };
            }

            return tween;
        }

        public static Tween DoRotateQuaternion(this Transform target, Quaternion to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Quaternion from = target.rotation;
            tween.OnStart = () => { from = target.rotation; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.rotation = Quaternion.SlerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoLocalRotate(this Transform target, Vector3 to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Vector3 from = target.localEulerAngles;
            tween.OnStart = () => { from = target.localEulerAngles; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.localEulerAngles = Vector3.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoRotateX(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.eulerAngles.x;
            tween.OnStart = () => { from = target.eulerAngles.x; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var e = target.eulerAngles;
                    e.x = Mathf.LerpUnclamped(from, to, t);
                    target.eulerAngles = e;
                }
            };

            return tween;
        }

        public static Tween DoRotateY(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.eulerAngles.y;
            tween.OnStart = () => { from = target.eulerAngles.y; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var e = target.eulerAngles;
                    e.y = Mathf.LerpUnclamped(from, to, t);
                    target.eulerAngles = e;
                }
            };

            return tween;
        }

        public static Tween DoRotateZ(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.eulerAngles.z;
            tween.OnStart = () => { from = target.eulerAngles.z; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var e = target.eulerAngles;
                    e.z = Mathf.LerpUnclamped(from, to, t);
                    target.eulerAngles = e;
                }
            };

            return tween;
        }

        public static Tween DoLocalRotateX(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.localEulerAngles.x;
            tween.OnStart = () => { from = target.localEulerAngles.x; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var e = target.localEulerAngles;
                    e.x = Mathf.LerpUnclamped(from, to, t);
                    target.localEulerAngles = e;
                }
            };

            return tween;
        }

        public static Tween DoLocalRotateY(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.localEulerAngles.y;
            tween.OnStart = () => { from = target.localEulerAngles.y; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var e = target.localEulerAngles;
                    e.y = Mathf.LerpUnclamped(from, to, t);
                    target.localEulerAngles = e;
                }
            };

            return tween;
        }

        public static Tween DoLocalRotateZ(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.localEulerAngles.z;
            tween.OnStart = () => { from = target.localEulerAngles.z; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var e = target.localEulerAngles;
                    e.z = Mathf.LerpUnclamped(from, to, t);
                    target.localEulerAngles = e;
                }
            };

            return tween;
        }

        public static Tween DoScale(this Transform target, Vector3 to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            Vector3 from = target.localScale;
            tween.OnStart = () => { from = target.localScale; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                    target.localScale = Vector3.LerpUnclamped(from, to, t);
            };

            return tween;
        }

        public static Tween DoScale(this Transform target, float to, float duration)
        {
            return DoScale(target, Vector3.one * to, duration);
        }

        public static Tween DoScaleX(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.localScale.x;
            tween.OnStart = () => { from = target.localScale.x; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var s = target.localScale;
                    s.x = Mathf.LerpUnclamped(from, to, t);
                    target.localScale = s;
                }
            };

            return tween;
        }

        public static Tween DoScaleY(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.localScale.y;
            tween.OnStart = () => { from = target.localScale.y; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var s = target.localScale;
                    s.y = Mathf.LerpUnclamped(from, to, t);
                    target.localScale = s;
                }
            };

            return tween;
        }

        public static Tween DoScaleZ(this Transform target, float to, float duration)
        {
            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            float from = target.localScale.z;
            tween.OnStart = () => { from = target.localScale.z; };
            tween.OnUpdate = (t) =>
            {
                if (target != null)
                {
                    var s = target.localScale;
                    s.z = Mathf.LerpUnclamped(from, to, t);
                    target.localScale = s;
                }
            };

            return tween;
        }

        public static Tween DoPath(this Transform target, Vector3[] points, float duration, bool useLocal = false)
        {
            if (points == null || points.Length < 2) return null;

            var tween = TweenEngine.Instance.GetTween();
            tween.Duration = duration;

            tween.OnUpdate = (t) =>
            {
                if (target == null) return;
                var p = EvaluateCatmullRomPath(points, t);
                if (useLocal)
                    target.localPosition = p;
                else
                    target.position = p;
            };

            return tween;
        }

        private static Vector3 EvaluateCatmullRomPath(Vector3[] points, float t)
        {
            int n = points.Length;
            if (n == 1) return points[0];
            if (n == 2) return Vector3.Lerp(points[0], points[1], t);

            int segmentCount = n - 1;
            float seg = Mathf.Clamp01(t) * segmentCount;
            int segIndex = Mathf.Min((int)seg, segmentCount - 1);
            float localT = seg - segIndex;

            Vector3 p0 = points[Mathf.Max(segIndex - 1, 0)];
            Vector3 p1 = points[segIndex];
            Vector3 p2 = points[segIndex + 1];
            Vector3 p3 = points[Mathf.Min(segIndex + 2, n - 1)];

            return CatmullRom(p0, p1, p2, p3, localT);
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }
    }
}
