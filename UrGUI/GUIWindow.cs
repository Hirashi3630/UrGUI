using System.Collections.Generic;
using UnityEngine;
using UrGUI.Utils;

namespace UrGUI
{
    public class GUIWindow
    {
        protected GUIWindow() { }

        public static readonly GUIStyle whiteButtonGUIStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.whiteTexture } };

        public static bool AllWindowsDisabled = false;
        public static bool AnyWindowDragging = false;
        public static System.Action ActiveOptionMenu = null;

        private string windowTitle;
        private float x, y, width, height, margin, controlHeight, controlSpace;

        private bool isActive, isDragging;

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
            if (x + width > Screen.width) x = Screen.width - width;
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

            // draw active option menu
            if (ActiveOptionMenu != null)
                ActiveOptionMenu();
        }

        private Rect NextControlRect()
        {
            Rect r = new Rect(x + margin, nextControlY, width - margin * 2, controlHeight);
            nextControlY += controlHeight + controlSpace;
            return r;
        }

        public void SaveCfg(string absolutePath)
        {
            INIParser ini = new INIParser();
            ini.Open(absolutePath);

            string sectionName = $"GUIWindow.{windowTitle}";
            // write all values for GUIWindow
            ini.WriteValue(sectionName, "windowTitle", windowTitle);
            ini.WriteValue(sectionName, "x", x);
            ini.WriteValue(sectionName, "y", y);
            ini.WriteValue(sectionName, "width", width);
            ini.WriteValue(sectionName, "height", height);
            ini.WriteValue(sectionName, "margin", margin);
            ini.WriteValue(sectionName, "controlHeight", controlHeight);
            ini.WriteValue(sectionName, "controlSpace", controlSpace);
            
            // write all controls' values
            for (int i = 0; i < controls.Count; i++)
                controls[i].ExportData(i, sectionName, $"Control{i}.", ini);

            // close and save
            ini.Close();
        }

        public void LoadCfg(string absolutePath)
        {
            if (!System.IO.File.Exists(absolutePath)) return;

            INIParser ini = new INIParser();
            ini.Open(absolutePath);

            string sectionName = $"GUIWindow.{windowTitle}";
            // read all values for GUIWindow
            windowTitle = ini.ReadValue(sectionName, "windowTitle", windowTitle);
            x = ini.ReadValue(sectionName, "x", x);
            y = ini.ReadValue(sectionName, "y", y);
            width = ini.ReadValue(sectionName, "width", width);
            height = ini.ReadValue(sectionName, "height", height);
            margin = ini.ReadValue(sectionName, "margin", margin);
            controlHeight = ini.ReadValue(sectionName, "controlHeight", controlHeight);
            controlSpace = ini.ReadValue(sectionName, "controlSpace", controlSpace);

            // read all controls' values
            for (int i = 0; i < controls.Count; i++)
                controls[i].ImportData(i, sectionName, $"Control{i}.", ini);

            // close and save
            ini.Close();
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

        public static WControl DropDown(string text, System.Action<int> onValueChanged, int value, Dictionary<int, string> list)
        {
            var c = new WDropDown(onValueChanged, value, list, text);

            return c;
        }

        internal class WSpace : WControl
        {
            internal WSpace() : base(string.Empty) { }

            internal override void Draw(Rect r) { }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini) { }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini) { }
        }

        internal class WLabel : WControl
        {
            public GUIFormatting.AlignType alignment;

            private Rect rect = new Rect();

            internal WLabel(
                string displayedString, GUIFormatting.AlignType alignment)
                : base(displayedString)
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

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "alignment", (int)alignment); // enum to int
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                alignment = (GUIFormatting.AlignType)ini.ReadValue(sectionName, keyBaseName + "alignment", (int)alignment); // int to enum
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

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "value", value);
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                value = ini.ReadValue(sectionName, keyBaseName + "value", value);
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

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "value", value);
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                value = ini.ReadValue(sectionName, keyBaseName + "value", value);
            }
        }

        internal class WColorPicker : WControl
        {
            public Color clr;

            private System.Action<Color> onValueChanged;
            private bool IsPickerOpen { get => _isPickerOpen; set
                {
                    _isPickerOpen = value;
                    GUIWindow.AllWindowsDisabled = value;
                    if (value)
                        ActiveOptionMenu = DrawPicker;
                    else
                        ActiveOptionMenu = null;
                }
            }
            private bool _isPickerOpen = false;
            private Rect rect;
            internal WColorPicker(System.Action<Color> onValueChanged, Color clr,
                string displayedString)
                : base(displayedString)
            {
                this.onValueChanged = onValueChanged;
                this.clr = clr;
            }

            internal override void Draw(Rect r)
            {
                rect = r;
                Rect previewButtonRect = rect;
                previewButtonRect.width /= 3f;
                previewButtonRect.x += previewButtonRect.width * 2f;
                Rect labelRect = rect;
                labelRect.width -= previewButtonRect.width * 2f;

                // # COLOR PICKER #
                // draw preview button
                if (GUIWindow.AllWindowsDisabled && IsPickerOpen) GUI.enabled = true;
                GUI.Label(labelRect, displayedString);
                var oldColor = GUI.color;
                GUI.color = clr;
                if (GUI.Button(previewButtonRect, string.Empty, whiteButtonGUIStyle))
                    IsPickerOpen = !IsPickerOpen;
                if (GUIWindow.AllWindowsDisabled && IsPickerOpen) GUI.enabled = false;
                GUI.color = oldColor;
            }

            internal void DrawPicker()
            {
                // TODO: refactor this, to: future me
                // main box
                Rect pRect = new Rect(rect.x + rect.width - 200, rect.height + rect.y, 200, 80); // picker rect
                var oldColor = GUI.color;
                GUI.color = Color.black;
                GUI.Box(pRect, "", whiteButtonGUIStyle);
                GUI.color = oldColor;

                // inside
                GUI.BeginGroup(pRect);
                // labels
                GUI.Label(new Rect(0, 0, 20, 20), "R: ");
                GUI.Label(new Rect(0, 20, 20, 20), "G: ");
                GUI.Label(new Rect(0, 40, 20, 20), "B: ");
                GUI.Label(new Rect(0, 60, 20, 20), "A: ");

                // sliders
                clr.r = GUI.HorizontalSlider(new Rect(20, 5, pRect.width - 20 - 25, 20), clr.r, 0, 1);
                clr.g = GUI.HorizontalSlider(new Rect(20, 25, pRect.width - 20 - 25, 20), clr.g, 0, 1);
                clr.b = GUI.HorizontalSlider(new Rect(20, 45, pRect.width - 20 - 25, 20), clr.b, 0, 1);
                clr.a = GUI.HorizontalSlider(new Rect(20, 65, pRect.width - 20 - 25, 20), clr.a, 0, 1);

                // values
                GUI.Label(new Rect(pRect.width - 25, 0, 25, 20), clr.r.ToString("0.00"));
                GUI.Label(new Rect(pRect.width - 25, 20, 25, 20), clr.g.ToString("0.00"));
                GUI.Label(new Rect(pRect.width - 25, 40, 25, 20), clr.b.ToString("0.00"));
                GUI.Label(new Rect(pRect.width - 25, 60, 25, 20), clr.a.ToString("0.00"));
                GUI.EndGroup();
            }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "color", clr);
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                clr = ini.ReadValue(sectionName, keyBaseName + "color", clr);
            }
        }

        internal class WDropDown : WControl
        {

            public System.Action<int> onValueChanged;
            public Dictionary<int, string> list;
            public int value;
            private bool IsDropDownOpen { get => _isDropDownOpen;
                set
                {
                    _isDropDownOpen = value;
                    GUIWindow.AllWindowsDisabled = value;
                    if (value)
                        ActiveOptionMenu = DrawDropDown;
                    else
                        ActiveOptionMenu = null;
                }
            }
            private bool _isDropDownOpen = false;
            private Vector2 scrollPos;
            private Rect rect;
            private Rect selectedRect;
            private static GUIStyle DropDownOptionGUIStyle = null; //= GUI.skin.label;

            internal WDropDown(System.Action<int> onValueChanged, int value, Dictionary<int, string> list,
                string displayedString)
                : base(displayedString)
            {
                this.onValueChanged = onValueChanged;
                this.value = value;
                this.list = list;
            }

            internal override void Draw(Rect r)
            {
                // init
                if (DropDownOptionGUIStyle == null)
                {
                    DropDownOptionGUIStyle = GUI.skin.label;
                    DropDownOptionGUIStyle.hover.textColor = Color.gray;
                    DropDownOptionGUIStyle.onHover.textColor = Color.gray;
                }

                // calc all rects
                rect = r;
                Rect labelRect = rect;
                labelRect.width /= 3f;
                selectedRect = rect;
                selectedRect.x += labelRect.width;
                selectedRect.width = labelRect.width * 2f;
                Rect selectedLabelRect = selectedRect;
                selectedLabelRect.x += 5f; // offset
                selectedLabelRect.width -= 5f; // offset

                if (GUIWindow.AllWindowsDisabled && IsDropDownOpen) GUI.enabled = true;
                GUI.Label(labelRect, displayedString);
                if (GUI.Button(selectedRect, string.Empty))
                    IsDropDownOpen = !IsDropDownOpen;
                GUI.Label(selectedLabelRect, list[value]);
                if (GUIWindow.AllWindowsDisabled && IsDropDownOpen) GUI.enabled = false;
            }

            internal void DrawDropDown()
            {
                GUI.enabled = true;
                // main box
                Rect pRect = new Rect(selectedRect.x, selectedRect.y + selectedRect.height, selectedRect.width, rect.height * 4); // picker rect
                var oldColor = GUI.color;
                GUI.color = Color.black;
                GUI.Box(pRect, "", whiteButtonGUIStyle);
                GUI.color = oldColor;

                // inside
                GUI.BeginGroup(pRect);

                scrollPos = GUI.BeginScrollView(
                    new Rect(0, 0, pRect.width, pRect.height), scrollPos,
                    new Rect(0, 0, 0, list.Count * rect.height));

                for (int i = 0; i < list.Count; i++)
                {
                    if (GUI.Button(new Rect(0, i * rect.height, pRect.width - 15, rect.height), list[i], DropDownOptionGUIStyle)) // 15 is width of scrollbar
                    {
                        value = i;
                        IsDropDownOpen = false;
                    }
                }


                // End the scroll view that we began above.
                GUI.EndScrollView();

                GUI.EndGroup();

                GUI.enabled = false;
            }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "value", value);
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                value = ini.ReadValue(sectionName, keyBaseName + "value", value);
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

            internal virtual void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                ini.WriteValue(sectionName, keyBaseName + "displayString", displayedString);
            }

            internal virtual void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                displayedString = ini.ReadValue(sectionName, keyBaseName + "displayString", displayedString);
            }
        }
    }
}
