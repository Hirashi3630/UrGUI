using UnityEngine;

namespace UrGUI.Utils
{
    public static class GUIFormatting
    {
        public static GUIStyle StringStyle { get; set; } = new GUIStyle(GUI.skin.label);

        // X O O
        // # O #
        // # # X

        public enum AlignType
        {
            LeftTop,
            LeftMiddle,
            LeftBottom,
            TopMiddle,
            Center,
            BottomMiddle,
            RightTop,
            RightMiddle,
            RightBottom
        }

        public static Vector2 AlignControl(AlignType type, float x, float y, float width, float height)
        {
            return AlignControl(type, new Vector2(x, y), new Vector2(width, height));
        }

        public static Vector2 AlignControl(AlignType type, Vector2 pos, Vector2 size, Vector2 container = default)
        {
            switch (type)
            {
                case AlignType.LeftTop:
                    return AlignControlLeftTop(pos, size, container);
                case AlignType.LeftMiddle:
                    return AlignControlLeftMiddle(pos, size, container);
                case AlignType.LeftBottom:
                    return AlignControlLeftBottom(pos, size, container);
                case AlignType.TopMiddle:
                    return AlignControlTopMiddle(pos, size, container);
                case AlignType.Center:
                    return AlignControlCenter(pos, size, container);
                case AlignType.BottomMiddle:
                    return AlignControlBottomMiddle(pos, size, container);
                case AlignType.RightTop:
                    return AlignControlRightTop(pos, size, container);
                case AlignType.RightMiddle:
                    return AlignControlRightMiddle(pos, size, container);
                case AlignType.RightBottom:
                    return AlignControlRightBottom(pos, size, container);
                default:
                    return pos;
            }
        }

        public static Vector2 GetContentStringSize(string text)
        {
            var content = new GUIContent(text);
            return StringStyle.CalcSize(content);
        }

        public static Vector2 AlignControlLeftTop(Vector2 pos, Vector2 size, Vector2 container = default)
        {
            return pos;
        }

        public static Vector2 AlignControlLeftMiddle(Vector2 pos, Vector2 size, Vector2 container = default)
        {
            size.x = 0;
            size.y /= 2f;
            container.x = 0;
            container.y /= 2f;
            var result = pos - size + container;
            return result;
        }

        public static Vector2 AlignControlLeftBottom(Vector2 pos, Vector2 size, Vector2 container = default)
        {
            size.x = 0;
            container.x = 0;
            var result = pos - size + container;
            return result;
        }

        public static Vector2 AlignControlBottomMiddle(Vector2 pos, Vector2 size, Vector2 container = default)
        {
            size.x /= 2f;
            container.x /= 2f;
            var result = pos - size + container;
            return result;
        }

        public static Vector2 AlignControlRightBottom(Vector2 pos, Vector2 size, Vector2 container = default)
        {
            var result = pos - size + container;
            return result;
        }

        public static Vector2 AlignControlCenter(Vector2 pos, Vector2 size, Vector2 container = default)
        {
            size /= 2f;
            container /= 2f;
            var result = pos - size + container;
            return result;
        }

        public static Vector2 AlignControlTopMiddle(Vector2 pos, Vector2 size, Vector2 container = default)
        {
            size.x /= 2f;
            size.y = 0;
            container.x /= 2f;
            container.y = 0;
            var result = pos - size + container;
            return result;
        }

        public static Vector2 AlignControlRightTop(Vector2 pos, Vector2 size, Vector2 container = default)
        {
            size.y = 0;
            container.y = 0;
            var result = pos - size + container;
            return result;
        }

        public static Vector2 AlignControlRightMiddle(Vector2 pos, Vector2 size, Vector2 container = default)
        {
            size.y /= 2f;
            container.y /= 2f;
            var result = pos - size + container;
            return result;
        }
    }
}
