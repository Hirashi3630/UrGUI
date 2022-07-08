using UnityEngine;

namespace UrGUI.Windows.Utils
{
    public static class GUIFormatting
    {
        public static GUIStyle StringStyle { get; set; } = new GUIStyle(GUI.skin.label);

        // X O O
        // # O #
        // # # X

        public static Vector2 GetContentStringSize(string text)
        {
            var content = new GUIContent(text);
            return StringStyle.CalcSize(content);
        }
    }
}
