using UGT.UniTween;
using UGT.UniTween.Components;
using UnityEditor;
using UnityEngine;

namespace UGT.UniTween.Editor
{
    /// <summary>
    /// TweenClip 的 PropertyDrawer，使 Clip 在 Inspector 中以折叠式面板展示。
    /// </summary>
    [CustomPropertyDrawer(typeof(TweenClip))]
    public sealed class TweenClipDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineH = EditorGUIUtility.singleLineHeight;
            float gap = EditorGUIUtility.standardVerticalSpacing;

            if (!property.isExpanded)
                return lineH + gap;

            TweenPropertyType propType = (TweenPropertyType)property.FindPropertyRelative("PropertyType").enumValueIndex;
            bool showCurve = property.FindPropertyRelative("Ease").enumValueIndex == (int)EaseType.Custom;
            bool isSpriteSeq = propType == TweenPropertyType.SpriteSequence;
            bool isBool = IsBoolType(propType);
            bool needsValueMode = NeedsValueMode(propType);

            float extraHeight = 0f;

            if (isSpriteSeq)
            {
                var spriteFramesProp = property.FindPropertyRelative("SpriteFrames");
                extraHeight = EditorGUI.GetPropertyHeight(spriteFramesProp, true) + gap;
            }

            int lines = 7; // 基础行
            lines++; // TimeScale 行
            if (needsValueMode) lines++;
            if (!isBool && !isSpriteSeq) lines++; // From 行
            if (showCurve) lines++;

            return (lineH + gap) * (lines + 1) + extraHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float lineH = EditorGUIUtility.singleLineHeight;
            float gap = EditorGUIUtility.standardVerticalSpacing;

            // ── 折叠头 ──
            var clipNameProp = property.FindPropertyRelative("ClipName");
            string clipName = clipNameProp.stringValue;
            if (string.IsNullOrEmpty(clipName))
                clipName = "Unnamed Clip";

            var foldoutRect = new Rect(position.x, position.y, position.width, lineH);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, clipName, true);

            if (!property.isExpanded) return;

            // ── 展开内容区域 ──
            var contentRect = EditorGUI.IndentedRect(new Rect(position.x, position.y + lineH + gap, position.width, position.height - lineH - gap));
            float w = contentRect.width;
            float x = contentRect.x;
            float y = contentRect.y;

            var durationProp = property.FindPropertyRelative("Duration");
            var delayProp = property.FindPropertyRelative("Delay");
            var easeProp = property.FindPropertyRelative("Ease");
            var loopsProp = property.FindPropertyRelative("Loops");
            var loopTypeProp = property.FindPropertyRelative("LoopType");
            var targetTypeProp = property.FindPropertyRelative("TargetType");
            var propertyTypeProp = property.FindPropertyRelative("PropertyType");
            var valueModeProp = property.FindPropertyRelative("ValueMode");
            var useFromProp = property.FindPropertyRelative("UseFrom");
            var fromValueProp = property.FindPropertyRelative("FromValue");
            var toValueProp = property.FindPropertyRelative("ToValue");
            var customCurveProp = property.FindPropertyRelative("CustomCurve");
            var spriteFramesProp = property.FindPropertyRelative("SpriteFrames");
            var customTimeScaleProp = property.FindPropertyRelative("CustomTimeScale");
            var timeScaleProp = property.FindPropertyRelative("TimeScale");

            float halfW = (w - gap) * 0.5f;
            TweenPropertyType propType = (TweenPropertyType)propertyTypeProp.enumValueIndex;
            bool isSpriteSeq = propType == TweenPropertyType.SpriteSequence;

            // Row 1: ClipName
            EditorGUI.PropertyField(MakeRect(x, y, w, lineH), clipNameProp, new GUIContent("Name"));
            y += lineH + gap;

            // Row 2: Duration + Delay
            EditorGUI.PropertyField(MakeRect(x, y, halfW, lineH), durationProp, new GUIContent("Duration"));
            EditorGUI.PropertyField(MakeRect(x + halfW + gap, y, halfW, lineH), delayProp, new GUIContent("Delay"));
            y += lineH + gap;

            // Speed: Custom Speed [☑] [value]，与其他字段对齐
            float speedLabelW = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(MakeRect(x, y, speedLabelW, lineH), "Custom Speed");
            float toggleX = x + speedLabelW;
            customTimeScaleProp.boolValue = EditorGUI.Toggle(MakeRect(toggleX, y, 20f, lineH), customTimeScaleProp.boolValue);
            if (customTimeScaleProp.boolValue)
            {
                EditorGUI.PropertyField(MakeRect(toggleX + 22f, y, w - speedLabelW - 22f, lineH), timeScaleProp, GUIContent.none);
            }
            y += lineH + gap;

            // Row 3: Ease
            EditorGUI.PropertyField(MakeRect(x, y, w, lineH), easeProp, new GUIContent("Ease"));
            y += lineH + gap;

            // Row 4: Custom Curve
            if (easeProp.enumValueIndex == (int)EaseType.Custom)
            {
                EditorGUI.PropertyField(MakeRect(x, y, w, lineH), customCurveProp, new GUIContent("Custom Curve"));
                y += lineH + gap;
            }

            // Row 5: Loops + LoopType
            EditorGUI.PropertyField(MakeRect(x, y, halfW, lineH), loopsProp, new GUIContent("Loops"));
            EditorGUI.PropertyField(MakeRect(x + halfW + gap, y, halfW, lineH), loopTypeProp, new GUIContent("Loop Type"));
            y += lineH + gap;

            // Row 6: TargetType
            EditorGUI.PropertyField(MakeRect(x, y, w, lineH), targetTypeProp, new GUIContent("Target"));
            y += lineH + gap;

            // Row 7: PropertyType（根据 TargetType 过滤显示）
            TweenTargetType targetType = (TweenTargetType)targetTypeProp.enumValueIndex;
            DrawFilteredPropertyType(propType, targetType, propertyTypeProp, MakeRect(x, y, w, lineH));
            propType = (TweenPropertyType)propertyTypeProp.enumValueIndex;
            isSpriteSeq = propType == TweenPropertyType.SpriteSequence;
            y += lineH + gap;

            // Row 8: ValueMode（SpriteSequence / Color / Bool 不需要）
            if (NeedsValueMode(propType))
            {
                EditorGUI.PropertyField(MakeRect(x, y, w, lineH), valueModeProp, new GUIContent("Value Mode"));
                y += lineH + gap;
            }

            if (isSpriteSeq)
            {
                // SpriteSequence 专用：FPS + SpriteFrames 列表
                var v4 = toValueProp.vector4Value;
                v4.x = EditorGUI.FloatField(MakeRect(x, y, w, lineH), "FPS", v4.x);
                toValueProp.vector4Value = v4;
                y += lineH + gap;

                float framesHeight = EditorGUI.GetPropertyHeight(spriteFramesProp, true);
                EditorGUI.PropertyField(MakeRect(x, y, w, framesHeight), spriteFramesProp, new GUIContent("Sprite Frames"), true);
            }
            else
            {
                // Row 9-10: From / To
                bool isBool = IsBoolType(propType);

                if (!isBool)
                {
                    float toggleW = 16f;
                    float labelW = EditorGUIUtility.labelWidth;
                    float fieldX = x + labelW + toggleW + 4f;
                    float fieldW = w - labelW - toggleW - 4f;

                    if (useFromProp.boolValue)
                    {
                        EditorGUI.LabelField(MakeRect(x, y, labelW, lineH), "From");
                        useFromProp.boolValue = EditorGUI.Toggle(MakeRect(x + labelW, y, toggleW, lineH), useFromProp.boolValue);
                        DrawFromValueField(propType, MakeRect(fieldX, y, fieldW, lineH), fromValueProp, "");
                        y += lineH + gap;
                    }
                    else
                    {
                        var oldColor = GUI.color;
                        GUI.color = Color.gray;
                        EditorGUI.LabelField(MakeRect(x, y, labelW, lineH), "From");
                        useFromProp.boolValue = EditorGUI.Toggle(MakeRect(x + labelW, y, toggleW, lineH), useFromProp.boolValue);
                        EditorGUI.LabelField(MakeRect(fieldX, y, fieldW, lineH), "(disabled)");
                        GUI.color = oldColor;
                        y += lineH + gap;
                    }
                }

                DrawToValueField(propType, MakeRect(x, y, w, lineH), toValueProp);
            }
        }

        // ─── TargetType → 允许的 PropertyType 映射 ───

        private static readonly TweenPropertyType[][] AllowedProperties =
        {
            // Transform
            new[] { TweenPropertyType.Position, TweenPropertyType.PositionX, TweenPropertyType.PositionY, TweenPropertyType.PositionZ,
                    TweenPropertyType.Rotation, TweenPropertyType.Scale, TweenPropertyType.ScaleX, TweenPropertyType.ScaleY, TweenPropertyType.ScaleZ },
            // RectTransform
            new[] { TweenPropertyType.AnchorPosition, TweenPropertyType.AnchorMin, TweenPropertyType.AnchorMax,
                    TweenPropertyType.SizeDelta, TweenPropertyType.Pivot },
            // CanvasGroup
            new[] { TweenPropertyType.Alpha },
            // Image
            new[] { TweenPropertyType.Color, TweenPropertyType.Fade, TweenPropertyType.SpriteSequence },
            // Text
            new[] { TweenPropertyType.Color, TweenPropertyType.Fade },
            // SpriteRenderer
            new[] { TweenPropertyType.SpriteColor, TweenPropertyType.SpriteAlpha,
                    TweenPropertyType.FlipX, TweenPropertyType.FlipY, TweenPropertyType.SpriteSize, TweenPropertyType.SpriteSequence },
            // SpriteRendererGroup
            new[] { TweenPropertyType.SpriteAlpha, TweenPropertyType.Color },
            // Material
            new TweenPropertyType[0],
        };

        private void DrawFilteredPropertyType(TweenPropertyType current, TweenTargetType targetType,
            SerializedProperty propertyTypeProp, Rect rect)
        {
            var allowed = AllowedProperties[(int)targetType];
            if (allowed.Length == 0)
            {
                EditorGUI.LabelField(rect, "Property", "(no properties available)");
                return;
            }

            int selectedIndex = 0;
            var names = new string[allowed.Length];
            for (int i = 0; i < allowed.Length; i++)
            {
                names[i] = allowed[i].ToString();
                if (allowed[i] == current)
                    selectedIndex = i;
            }

            int newIndex = EditorGUI.Popup(rect, "Property", selectedIndex, names);
            if (newIndex >= 0 && newIndex < allowed.Length)
            {
                propertyTypeProp.enumValueIndex = (int)allowed[newIndex];
            }
        }

        // ─── ValueType / Draw ───

        private static bool IsBoolType(TweenPropertyType type)
        {
            return type == TweenPropertyType.FlipX || type == TweenPropertyType.FlipY;
        }

        private static bool NeedsValueMode(TweenPropertyType type)
        {
            if (type == TweenPropertyType.Color || type == TweenPropertyType.SpriteColor) return false;
            if (type == TweenPropertyType.SpriteSequence) return false;
            return !IsBoolType(type);
        }

        private void DrawFromValueField(TweenPropertyType propType, Rect rect, SerializedProperty prop, string label)
        {
            Vector4 v4 = prop.vector4Value;
            switch (GetValueType(propType))
            {
                case ClipValueType.Float:
                    v4.x = EditorGUI.FloatField(rect, label, v4.x);
                    break;
                case ClipValueType.Vector2:
                    {
                        var v2 = EditorGUI.Vector2Field(rect, label, new Vector2(v4.x, v4.y));
                        v4.x = v2.x; v4.y = v2.y;
                    }
                    break;
                case ClipValueType.Vector3:
                    {
                        var v3 = EditorGUI.Vector3Field(rect, label, new Vector3(v4.x, v4.y, v4.z));
                        v4.x = v3.x; v4.y = v3.y; v4.z = v3.z;
                    }
                    break;
                case ClipValueType.Color:
                    {
                        var c = EditorGUI.ColorField(rect, label, new Color(v4.x, v4.y, v4.z, v4.w));
                        v4.x = c.r; v4.y = c.g; v4.z = c.b; v4.w = c.a;
                    }
                    break;
            }
            prop.vector4Value = v4;
        }

        private void DrawToValueField(TweenPropertyType propType, Rect rect, SerializedProperty prop)
        {
            Vector4 v4 = prop.vector4Value;
            switch (GetValueType(propType))
            {
                case ClipValueType.Float:
                    v4.x = EditorGUI.FloatField(rect, "To", v4.x);
                    break;
                case ClipValueType.Vector2:
                    {
                        var v2 = EditorGUI.Vector2Field(rect, "To", new Vector2(v4.x, v4.y));
                        v4.x = v2.x; v4.y = v2.y;
                    }
                    break;
                case ClipValueType.Vector3:
                    {
                        var v3 = EditorGUI.Vector3Field(rect, "To", new Vector3(v4.x, v4.y, v4.z));
                        v4.x = v3.x; v4.y = v3.y; v4.z = v3.z;
                    }
                    break;
                case ClipValueType.Color:
                    {
                        var c = EditorGUI.ColorField(rect, "To", new Color(v4.x, v4.y, v4.z, v4.w));
                        v4.x = c.r; v4.y = c.g; v4.z = c.b; v4.w = c.a;
                    }
                    break;
                case ClipValueType.Bool:
                    {
                        string boolLabel = propType == TweenPropertyType.FlipX ? "Flip X" : "Flip Y";
                        v4.x = EditorGUI.Toggle(rect, boolLabel, v4.x > 0.5f) ? 1f : 0f;
                    }
                    break;
            }
            prop.vector4Value = v4;
        }

        private enum ClipValueType { Float, Vector2, Vector3, Color, Bool }

        private static ClipValueType GetValueType(TweenPropertyType type)
        {
            switch (type)
            {
                case TweenPropertyType.PositionX:
                case TweenPropertyType.PositionY:
                case TweenPropertyType.PositionZ:
                case TweenPropertyType.RotationX:
                case TweenPropertyType.RotationY:
                case TweenPropertyType.RotationZ:
                case TweenPropertyType.ScaleX:
                case TweenPropertyType.ScaleY:
                case TweenPropertyType.ScaleZ:
                case TweenPropertyType.Alpha:
                case TweenPropertyType.Fade:
                case TweenPropertyType.SpriteAlpha:
                case TweenPropertyType.FloatValue:
                    return ClipValueType.Float;

                case TweenPropertyType.AnchorMin:
                case TweenPropertyType.AnchorMax:
                case TweenPropertyType.SizeDelta:
                case TweenPropertyType.Pivot:
                case TweenPropertyType.SpriteSize:
                    return ClipValueType.Vector2;

                case TweenPropertyType.Position:
                case TweenPropertyType.Rotation:
                case TweenPropertyType.Scale:
                case TweenPropertyType.AnchorPosition:
                    return ClipValueType.Vector3;

                case TweenPropertyType.Color:
                case TweenPropertyType.SpriteColor:
                    return ClipValueType.Color;

                case TweenPropertyType.FlipX:
                case TweenPropertyType.FlipY:
                    return ClipValueType.Bool;

                default:
                    return ClipValueType.Float;
            }
        }

        private static Rect MakeRect(float x, float y, float w, float h)
        {
            return new Rect(x, y, w, h);
        }
    }
}
