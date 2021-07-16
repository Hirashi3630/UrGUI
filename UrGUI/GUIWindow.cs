using System.Collections.Generic;
using UnityEngine;
using UrGUI.Utils;

namespace UrGUI
{
    public class GUIWindow
    {
        protected GUIWindow() { }

        public static bool AllWindowsDisabled = false;
        public static bool AnyWindowDragging = false;
        private bool isActive, isDragging;
        private string windowTitle;
        private float x, y, width, height, margin, controlHeight, controlSpace; 

        private float nextControlY; 
        private List<WControl> controls;

        public static GUIWindow Begin(string windowTitle, float startX, float startY, float startWidth, float startHeight, float margin, float controlHeight, float controlSpace)
        {
            GUIWindow b = new GUIWindow();
            b.isActive = true;
            b.windowTitle = windowTitle;
            b.x = startX;
            b.y = startY;
            b.width = startWidth;
            b.height = startHeight;
            b.margin = margin;
            b.controlHeight = controlHeight;
            b.controlSpace = controlSpace;

            
            b.controls = new List<WControl>();

            return b;
        }

        public void Add(WControl c)
        {
            controls.Add(c);
        }

        public void Draw()
        {
            // disable if it's required
            if (AllWindowsDisabled) GUI.enabled = false;

            // mouse drag
            if (!AnyWindowDragging || isDragging)
            {
                var e = Event.current;
                if (e.type == EventType.MouseDown && new Rect(x, y, width, controlHeight * 1.25f).Contains(e.mousePosition))
                {
                    isDragging = true;
                    AnyWindowDragging = true;
                }
                else if (e.type == EventType.MouseUp)
                {
                    isDragging = false;
                    AnyWindowDragging = false;
                }
                if (e.type == EventType.MouseDrag && isDragging)
                {
                    x += e.delta.x;
                    y += e.delta.y;
                }
            }

            // check if window isn't outside of screen
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x + width  > Screen.width) x = Screen.width - width;
            if (y + height > Screen.height) y = Screen.height - height;

            // reset nextControlY
            nextControlY = y + controlHeight + margin;

            // Main window
            GUI.Box(new Rect(x, y, width, height), "");

            // window's title
            GUI.Box(new Rect(x, y, width, controlHeight * 1.25f), windowTitle);
            //nextControlY += controlSpace; // add more space between title and first control

            // draw all controls
            foreach (var c in controls)
                c.Draw(NextControlRect());

            // reset GUI enabled
            GUI.enabled = true;
        }

        private Rect NextControlRect()
        {
            Rect r = new Rect(x + margin, nextControlY, width - margin * 2, controlHeight);
            nextControlY += controlHeight + controlSpace;
            return r;
        }

        //#################
        // ### CONTROLS ###
        // ################

        public static WControl Space()
        {
            var c = new WSpace();

            return c;
        }

        public static WControl Label(string text,
                GUIFormatting.AlignType alignment = GUIFormatting.AlignType.LeftTop)
        {
            var c = new WLabel(text, alignment);

            return c;
        }

        public static WControl Button(string text, System.Action onPressed)
        {
            var c = new WButton(onPressed, text);
            
            return c;
        }

        public static WControl Toggle(string text, System.Action<bool> onValueChanged,
                bool value = false)
        {
            var c = new WToggle(onValueChanged, value, text);

            return c;
        }

        public static WControl Slider(string text, System.Action<float> onValueChanged, float value, float min, float max,
                bool numberIndicator = false, string numberIndicatorFormat = "0.##")
        {
            var c = new WSlider(onValueChanged, value, min, max, numberIndicator, numberIndicatorFormat, text);

            return c;
        }

        public static WControl ColorPicker(string text, System.Action<Color> onValueChanged, Color value)
        {
            var c = new WColorPicker(onValueChanged, value, text);

            return c;
        }

        internal class WSpace : WControl
        {
            internal WSpace() : base(string.Empty) { }

            internal override void Draw(Rect r) { }
        }

        internal class WLabel : WControl
        {
            private GUIFormatting.AlignType alignment;
            private Rect rect = new Rect();

            internal WLabel(
                string displayedString, GUIFormatting.AlignType alignment)
                : base (displayedString)
            {
                this.alignment = alignment;
            }

            internal override void Draw(Rect r)
            {
                rect = r;
                Vector2 pos = GUIFormatting.AlignControl(alignment, new Vector2(rect.x, rect.y),
                    GUIFormatting.GetContentStringSize(displayedString), new Vector2(rect.width, rect.height));
                rect.x = pos.x;
                rect.y = pos.y;

                GUI.Label(rect, displayedString);
            }
        }

        internal class WButton : WControl
        {
            private System.Action onPressed;

            internal WButton(System.Action onPressed,
                string displayedString) 
                : base(displayedString)
            {
                this.onPressed = onPressed;
            }

            internal override void Draw(Rect r)
            {
                if (GUI.Button(r, displayedString))
                    onPressed();
            }
        }

        internal class WToggle : WControl
        {
            public System.Action<bool> onValueChanged;
            public bool value;

            internal WToggle(System.Action<bool> onValueChanged, bool value,
                string displayedString)
                : base(displayedString)
            {
                this.value = value;
                this.onValueChanged = onValueChanged;
            }

            internal override void Draw(Rect r)
            {
                var newValue = GUI.Toggle(r, value, displayedString);
                if (newValue != value)
                    onValueChanged(newValue);
                value = newValue;
            }
        }

        internal class WSlider : WControl
        {
            public System.Action<float> onValueChanged;
            public float value;

            private float min, max;
            private bool numberIndicator;
            private string numberIndicatorFormat;

            internal WSlider(System.Action<float> onValueChanged, float value, float min, float max, bool numberIndicator, string numberIndicatorFormat,
                string displayedString)
                : base(displayedString)
            {
                this.onValueChanged = onValueChanged;
                this.value = value;
                this.min = min;
                this.max = max;
                this.numberIndicator = numberIndicator;
                this.numberIndicatorFormat = numberIndicatorFormat;
            }

            internal override void Draw(Rect r)
            {
                // get next control rect
                Rect sliderRect = r;
                Rect labelRect = sliderRect;
                Rect numberRect;

                // get size of text
                Vector2 textSize = GUIFormatting.GetContentStringSize(displayedString);

                // move slider rect to the right by size of text
                sliderRect.x += textSize.x + 5f; // 5f is margin
                sliderRect.y += 5f; // 5f is margin
                sliderRect.width -= textSize.x + 5f; // 5f is margin

                // change width of label rect by slider width
                labelRect.width -= sliderRect.width - 5f; // 5f is margin

                // draw
                GUI.Label(labelRect, displayedString);
                if (numberIndicator)
                {
                    numberRect = new Rect(sliderRect.x + sliderRect.width / 2f - 30f, sliderRect.y, sliderRect.width / 2f, 20);
                    GUI.Label(numberRect, value.ToString(numberIndicatorFormat));
                }
                var newValue = GUI.HorizontalSlider(sliderRect, value, min, max);

                // handle onChangedValue event 
                if (newValue != value)
                    onValueChanged(newValue);
                value = newValue;
            }
        }

        internal class WColorPicker : WControl
        {
            public Color clr;

            private System.Action<Color> onValueChanged;
            private bool IsPickerOpen { get => _isPickerOpen; set { _isPickerOpen = value; GUIWindow.AllWindowsDisabled = value; } }
            private bool _isPickerOpen = false;
            private static readonly GUIStyle whiteButtonGUIStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.whiteTexture } };

            internal WColorPicker(System.Action<Color> onValueChanged, Color clr,
                string displayedString)
                : base(displayedString)
            {
                this.onValueChanged = onValueChanged;
                this.clr = clr;
            }

            internal override void Draw(Rect r)
            {
                Rect previewButtonRect = r;
                previewButtonRect.width /= 3f;
                previewButtonRect.x += previewButtonRect.width * 2f;
                Rect labelRect = r;
                labelRect.width -= previewButtonRect.width * 2f;

                // # LABEL #
                GUI.Label(labelRect, displayedString);

                // # COLOR PICKER #
                // draw preview button
                var oldColor = GUI.color;
                GUI.color = clr;
                if (GUIWindow.AllWindowsDisabled && IsPickerOpen) GUI.enabled = true;
                if (GUI.Button(previewButtonRect, string.Empty, whiteButtonGUIStyle))
                    IsPickerOpen = !IsPickerOpen;
                if (GUIWindow.AllWindowsDisabled && IsPickerOpen) GUI.enabled = false;
                GUI.color = oldColor;

                // draw color picker
                // TODO: refactor this, to: future me
                if (IsPickerOpen)
                {
                    GUI.enabled = true;
                    // main box
                    Rect pRect = new Rect(r.x + r.width - 200, r.height + r.y, 200, 80); // picker rect
                    GUI.Box(pRect, "");

                    // inside
                    GUI.BeginGroup(pRect);
                    // labels
                    GUI.Label(new Rect(0,  0, 20, 20), "R: ");
                    GUI.Label(new Rect(0, 20, 20, 20), "G: ");
                    GUI.Label(new Rect(0, 40, 20, 20), "B: ");
                    GUI.Label(new Rect(0, 60, 20, 20), "A: ");
                    
                    // sliders
                    clr.r = GUI.HorizontalSlider(new Rect(20,  5, pRect.width - 20 - 25, 20), clr.r, 0, 1);
                    clr.g = GUI.HorizontalSlider(new Rect(20, 25, pRect.width - 20 - 25, 20), clr.g, 0, 1);
                    clr.b = GUI.HorizontalSlider(new Rect(20, 45, pRect.width - 20 - 25, 20), clr.b, 0, 1);
                    clr.a = GUI.HorizontalSlider(new Rect(20, 65, pRect.width - 20 - 25, 20), clr.a, 0, 1);

                    // values
                    GUI.Label(new Rect(pRect.width - 25,  0, 25, 20), clr.r.ToString("0.00"));
                    GUI.Label(new Rect(pRect.width - 25, 20, 25, 20), clr.g.ToString("0.00"));
                    GUI.Label(new Rect(pRect.width - 25, 40, 25, 20), clr.b.ToString("0.00"));
                    GUI.Label(new Rect(pRect.width - 25, 60, 25, 20), clr.a.ToString("0.00"));
                    GUI.EndGroup();

                    GUI.enabled = false;
                }
            }
        }

        public abstract class WControl
        {
            internal string displayedString;

            internal WControl(string displayedString)
            {
                this.displayedString = displayedString;
            }

            internal abstract void Draw(Rect r);
        }
    }
}
