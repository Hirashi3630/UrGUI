using System.Collections.Generic;
using UnityEngine;

namespace UrGUI.Utils
{
    public static class GUIControl
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
            Rect numberRect = new Rect(sliderRect.x + sliderRect.width / 2f - 30f, 0, sliderRect.width / 2f, rect.height); // 30f is offset
            // draw
            GUI.BeginGroup(rect);
            GUI.Label(labelRect, label);
            var resultValue =  GUI.HorizontalSlider(sliderRect, value, min, max);
            if (numberIndicator)
                GUI.Label(numberRect, value.ToString(numberIndicatorFormat));
            GUI.EndGroup();

            return resultValue;
        }

        /// <summary>RGBA Color Picker dialog using LabelSlider</summary>
        /// <param name="leftTopCorner"></param>
        /// <param name="value">The value the color picker shows</param>
        /// <param name="mainBoxStyle">Style of main box; if null = drawing black colored texture instead of box</param>
        /// <param name="sliderWidth">Width of individual sliders</param>
        /// <param name="sliderHeight">Height of individual sliders</param>
        /// <param name="offsetY">Pixel offset between sliders</param>
        /// <param name="margin">Pixel offset from each side</param>
        /// <returns></returns>
        public static Color ColorPicker(Vector2 leftTopCorner, Color value,
            bool mainboxColoredTexture = false, float sliderWidth = 200f, float sliderHeight = 22f, float offsetY = 5f, float margin = 5f)
        {
            // calculate width of label
            float labelWidth = GUIFormatting.GetContentStringSize("X: ").x;

            // main box
            Rect mainRect = new Rect(leftTopCorner.x, leftTopCorner.y, // x, y 
                labelWidth + sliderWidth + margin * 2f, // width -> 'margin * 2' = margin from both sides
                sliderHeight * 4 + offsetY * 3f + margin * 2f);// height -> '* 4f' = 4 sliders (RGBA); '* 3f' = 3 spaces between sliders
            Rect controlsRect = new Rect(leftTopCorner.x + margin, leftTopCorner.y + margin, // x, y
                labelWidth + sliderWidth, // width
                sliderHeight * 4 + offsetY * 3f); // height
            // draw main box
            if (mainboxColoredTexture)
                ColoredBox(mainRect, Color.gray);
            else
                GUI.Box(mainRect, "");

            // inside
            GUI.BeginGroup(controlsRect);

            // draw label sliders
            var newValue = value;
            newValue.r = LabelSlider(new Rect(0, sliderHeight * 0 + offsetY * 0, controlsRect.width, sliderHeight), "R: ", value.r, 0f, 1f);
            newValue.g = LabelSlider(new Rect(0, sliderHeight * 1 + offsetY * 1, controlsRect.width, sliderHeight), "G: ", value.g, 0f, 1f);
            newValue.b = LabelSlider(new Rect(0, sliderHeight * 2 + offsetY * 2, controlsRect.width, sliderHeight), "B: ", value.b, 0f, 1f);
            newValue.a = LabelSlider(new Rect(0, sliderHeight * 3 + offsetY * 3, controlsRect.width, sliderHeight), "A: ", value.a, 0f, 1f);
            GUI.EndGroup();

            return newValue;
        }

        public static int DropDown(Vector2 leftTopCorner, int value, Dictionary<int, string> list, Vector2 scrollPos, out Vector2 scrollPosNew, out bool isOpen,
           GUIStyle optionGUIStyle, bool mainboxColoredTexture = false, float width = 0, int optionCountShown = 4, float optionHeight = 22)
        {
            // init
            int newValue;
            isOpen = true;

            // main box
            Rect mainRect = new Rect(leftTopCorner.x, leftTopCorner.y, width + 15f, optionHeight * optionCountShown); // 15 is pixel width of scrollbar

            if (mainboxColoredTexture)
                ColoredBox(mainRect, Color.gray);
            else
                GUI.Box(mainRect, "");

            // inside
            GUI.BeginGroup(mainRect);

            scrollPosNew = GUI.BeginScrollView(
                new Rect(0, 0, mainRect.width, mainRect.height), scrollPos,
                new Rect(0, 0, 0, list.Count * optionHeight));

            for (int i = 0; i < list.Count; i++)
            {
                if (GUI.Button(new Rect(0, i * optionHeight, mainRect.width - 15, optionHeight), list[i], optionGUIStyle)) // 15 is width of scrollbar
                {
                    newValue = i;
                    isOpen = false;
                }
            }

            GUI.EndScrollView();

            GUI.EndGroup();

            return value;
        }

        public static void ColoredBox(Rect r, Color color)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = oldColor;
        }
    }
}
