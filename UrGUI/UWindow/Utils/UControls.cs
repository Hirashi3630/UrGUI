using System.Collections.Generic;
using UnityEngine;
using UrGUI.Utils;

namespace UrGUI.UWindow.Utils
{
    /// <summary>
    /// Custom controls used by UWindow
    /// </summary>
    public static class UControls
    {
        /// <summary>Named HorizontalSlider control</summary>
        /// <param name="rect">Full Rect of whole control</param>
        /// <param name="label">Label displayed next to the slider</param>
        /// <param name="value">The value the slider shows</param>
        /// <param name="min">The value on the left</param>
        /// <param name="max">The value on the right</param>
        /// <param name="numberIndicator">If slider should have in middle number that shows slider value</param>
        /// <param name="labelOnLeft">Where should the label be; true = text on left, false = text on right</param>
        /// <param name="offsetX">Pixel offset between pixel and slider</param>
        /// <returns></returns>
        public static float LabelSlider(Rect rect, string label, float value, float min, float max,
            bool numberIndicator = false, string numberIndicatorFormat = "0.##", bool labelOnLeft = true, float offsetX = 5f)
        {
            // get size of string name
            Vector2 nameSize = GUIFormatting.GetContentStringSize(label);
            // get all rects
            Rect labelRect;
            if (labelOnLeft)
                labelRect = new Rect(0, 0, nameSize.x, rect.height);
            else
                labelRect = new Rect(rect.width - nameSize.x + offsetX, 0, nameSize.x, rect.height);
            Rect sliderRect = new Rect(labelOnLeft ? labelRect.width + offsetX : 0, 0, rect.width - labelRect.width - offsetX, rect.height);
            Rect numberRect = new Rect(sliderRect.x + (sliderRect.width / 2f) - 30f, 0, sliderRect.width / 2f, rect.height); // 30f is offset
            // draw
            GUI.BeginGroup(rect);
            GUI.Label(labelRect, label);
            var resultValue =  GUI.HorizontalSlider(sliderRect, value, min, max);
            if (numberIndicator)
                GUI.Label(numberRect, value.ToString(numberIndicatorFormat));
            GUI.EndGroup();

            return resultValue;
        }

        /// <summary>Named TextField</summary>
        /// <param name="rect">Full Rect of whole control</param>
        /// <param name="label">Label displayed next to the slider</param>
        /// <param name="value">The value the slider shows</param>
        /// <param name="maxSymbolLength">Maximum number of symbols that can user input</param>
        /// <param name="regexReplace">Regex replace filter</param>
        /// <param name="labelOnLeft">Where should the label be; true = text on left, false = text on right</param>
        /// <param name="offsetX">Pixel offset between pixel and slider</param>
        /// <returns></returns>
        public static string LabelTextField(Rect rect, string label, string value, int maxSymbolLength,
            string regexReplace = "", bool labelOnLeft = true, float offsetX = 5f)
        {
            // get size of string name
            Vector2 nameSize = GUIFormatting.GetContentStringSize(label);
            // get all rects
            Rect labelRect;
            if (labelOnLeft)
                labelRect = new Rect(0, 0, nameSize.x, rect.height);
            else
                labelRect = new Rect(rect.width - nameSize.x + offsetX, 0, nameSize.x, rect.height);
            Rect sliderRect = new Rect(labelOnLeft ? labelRect.width + offsetX : 0, 0, rect.width - labelRect.width - offsetX, rect.height);
            // draw
            GUI.BeginGroup(rect);
            GUI.Label(labelRect, label);
            var resultValue = GUI.TextField(sliderRect, value, maxSymbolLength);
            GUI.EndGroup();

            // apply regex
            if (!string.IsNullOrEmpty(regexReplace))
                resultValue = System.Text.RegularExpressions.Regex.Replace(resultValue, regexReplace, string.Empty);
            
            return resultValue;
        }

        public static Vector3 LabelVector3Field(Rect rect, string label, Vector3 value, Vector3 min, Vector3 max)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>RGBA Color Picker dialog using LabelSlider</summary>
        /// <param name="leftTopCorner">Position of left top corner</param>
        /// <param name="color">The value the color picker shows</param>
        /// <param name="mainBoxColoredTexture">if true, drawing black colored texture instead of box</param>
        /// <param name="sliderWidth">Width of individual sliders</param>
        /// <param name="controlHeight">Height of individual sliders</param>
        /// <param name="offsetY">Pixel offset between sliders</param>
        /// <param name="margin">Pixel offset from each side</param>
        /// <returns>True if cancel button has been pressed</returns>
        public static bool ColorPicker(Vector2 leftTopCorner, ref Color color,
            bool mainBoxColoredTexture = false, float sliderWidth = 200f, float controlHeight = 12f, float offsetY = 5f, float margin = 5f)
        {
            // calculate width of label
            float labelWidth = GUIFormatting.GetContentStringSize("X: ").x;

            // calculate rects
            var mainRect = new Rect(leftTopCorner.x, leftTopCorner.y, // x, y 
                (labelWidth + sliderWidth) + (margin * 2f), // width -> 'margin * 2' = margin from both sides
                (controlHeight * 4) + (offsetY * 3f) + (margin * 2f)); // height -> '* 4f' = 4 sliders (RGBA); '* 3f' = 3 spaces between sliders
            var controlsRect = new Rect(leftTopCorner.x + margin, leftTopCorner.y + margin, // x, y
                labelWidth + sliderWidth, // width
                (controlHeight * 4) + (offsetY * 3f)); // height
            // draw main box
            if (mainBoxColoredTexture)
                ColoredBox(mainRect, Color.black);
            else
                GUI.Box(mainRect, "");

            // draw cancel button
            var cancelRect = new Rect(
                mainRect.x, mainRect.y - controlHeight,
                mainRect.width / 3f, controlHeight);
            if (UGUI.Button(cancelRect, "Cancel"))
                return true;
            
            // inside
            GUI.BeginGroup(controlsRect);

            // draw label sliders
            var newColor = color;
            newColor.r = LabelSlider(new Rect(0, (controlHeight * 0) + (offsetY * 0), controlsRect.width, controlHeight), "R: ", color.r, 0f, 1f);
            newColor.g = LabelSlider(new Rect(0, (controlHeight * 1) + (offsetY * 1), controlsRect.width, controlHeight), "G: ", color.g, 0f, 1f);
            newColor.b = LabelSlider(new Rect(0, (controlHeight * 2) + (offsetY * 2), controlsRect.width, controlHeight), "B: ", color.b, 0f, 1f);
            newColor.a = LabelSlider(new Rect(0, (controlHeight * 3) + (offsetY * 3), controlsRect.width, controlHeight), "A: ", color.a, 0f, 1f);
            GUI.EndGroup();

            color = newColor;
            
            return false;
        }

        /// <summary>Drop Down dialog with ScrollView</summary>
        /// <param name="leftTopCorner">Position of left top corner</param>
        /// <param name="list">dictionary of all options</param>
        /// <param name="scrollPos">current scroll position of scroll view (x axis is ignored)</param>
        /// <param name="scrollPosNew">new scroll position of scroll view (x axis is ignored)</param>
        /// <param name="isOpen">false = user selected option and want's to hide drop down</param>
        /// <param name="optionGUIStyle">GUIStyle of options (recommended GUI.skin.label)</param>
        /// <param name="mainBoxColoredTexture">If main box should be replaced with colored texture</param>
        /// <param name="width">Width of whole dialog</param>
        /// <param name="optionCountShown">How many of options should be displayed without slider</param>
        /// <param name="optionHeight">Height of individual options</param>
        /// <returns></returns>
        public static int DropDown(Vector2 leftTopCorner, Dictionary<int, string> list, Vector2 scrollPos, out Vector2 scrollPosNew, out bool isOpen,
           GUIStyle optionGUIStyle, bool mainBoxColoredTexture = false, float width = 0, int optionCountShown = 4, float optionHeight = 22)
        {
            int margin = 4;
            
            // init
            int newValue = -1;
            isOpen = true;
            // main box
            Rect mainRect = new Rect(leftTopCorner.x, leftTopCorner.y, width + 15f, optionHeight * optionCountShown); // 15 is pixel width of scrollbar

            if (mainBoxColoredTexture)
                ColoredBox(mainRect, Color.black);
            else
                GUI.Box(mainRect, string.Empty);

            // inside
            GUI.BeginGroup(mainRect);

            scrollPosNew = GUI.BeginScrollView(
                new Rect(0, 0, mainRect.width, mainRect.height), scrollPos,
                new Rect(0, 0, 0, list.Count * optionHeight));

            for (int i = 0; i < list.Count; i++)
            {
                if (UGUI.Button(new Rect(margin, i * optionHeight, mainRect.width - margin - 15, optionHeight),
                        list[i], optionGUIStyle)) // 15 is width of scrollbar
                {
                    newValue = i;
                    isOpen = false;
                }
            }

            GUI.EndScrollView();

            GUI.EndGroup();

            return newValue;
        }

        /// <summary>
        /// Draw colored box
        /// </summary>
        /// <param name="r"></param>
        /// <param name="color"></param>
        public static void ColoredBox(Rect r, Color color)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = oldColor;
        }
    }
}
