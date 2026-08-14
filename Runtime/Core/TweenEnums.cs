using UnityEngine;

namespace UGT.UniTween
{
    public enum EaseType
    {
        Linear,
        InSine, OutSine, InOutSine,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InQuart, OutQuart, InOutQuart,
        InQuint, OutQuint, InOutQuint,
        InExpo, OutExpo, InOutExpo,
        InCirc, OutCirc, InOutCirc,
        InBack, OutBack, InOutBack,
        InElastic, OutElastic, InOutElastic,
        InBounce, OutBounce, InOutBounce,
        Custom,
    }

    public enum LoopType { Restart, Yoyo }

    public enum PlayMode { None, Play, Restart }

    public enum UpdateMode { Normal, Late, Fixed, Unscaled }

    public enum TweenValueMode { Absolute, Relative }

    public enum TweenTargetType { Transform, RectTransform, CanvasGroup, Image, Text, SpriteRenderer, SpriteRendererGroup, Material }

    public enum TweenPropertyType
    {
        // Transform
        Position, PositionX, PositionY, PositionZ,
        Rotation, RotationX, RotationY, RotationZ,
        Scale, ScaleX, ScaleY, ScaleZ,
        Path,
        // RectTransform
        AnchorPosition, AnchorMin, AnchorMax,
        SizeDelta, Pivot,
        // CanvasGroup
        Alpha,
        // Image / Text
        Color, Fade,
        // SpriteRenderer
        SpriteColor, SpriteAlpha, FlipX, FlipY, SpriteSize,
        // Image / SpriteRenderer 序列帧
        SpriteSequence,
        // Float
        FloatValue,
    }
}
