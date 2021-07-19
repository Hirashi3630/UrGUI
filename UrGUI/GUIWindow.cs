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

        public bool IsEnabled { get; set; }

        private string windowTitle;
        private float x, y, width, height, margin, controlHeight, controlSpace;
        private bool isDraggable;

        private bool isDragging;

        private float nextControlY;
        private List<WControl> controls;

        private GUISkin mainSkin = null;
        private GUISkin titleSkin = null;

        public static GUIWindow Begin(string windowTitle, float startX, float startY, float startWidth, float startHeight,
            float margin = 10, float controlHeight = 22, float controlSpace = 5, bool isEnabled = true, bool isDraggable = true)
        {
            GUIWindow b = new GUIWindow();
            b.IsEnabled = isEnabled;
            b.windowTitle = windowTitle;
            b.x = startX;
            b.y = startY;
            b.width = startWidth;
            b.height = startHeight;
            b.margin = margin;
            b.controlHeight = controlHeight;
            b.controlSpace = controlSpace;
            b.isDraggable = isDraggable;


            b.controls = new List<WControl>();

            return b;
        }

        public void Add(WControl c)
        {
            controls.Add(c);
        }

        public void Draw()
        {
            // check if this window is enabled
            if (!IsEnabled) return;

            // disable if it's required
            if (AllWindowsDisabled) GUI.enabled = false;

            // handle drag
            if (isDraggable)
            {
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
            }

            // check if window isn't outside of screen
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x + width > Screen.width) x = Screen.width - width;
            if (y + height > Screen.height) y = Screen.height - height;

            // reset nextControlY
            nextControlY = y + controlHeight + margin;

            // Main window
            if (mainSkin != null)
                GUI.skin = mainSkin;
            GUI.Box(new Rect(x, y, width, height), "");

            // window's title
            if (titleSkin != null)
                GUI.skin = titleSkin;
            GUI.Box(new Rect(x, y, width, 25f), windowTitle);
            //nextControlY += controlSpace; // add more space between title and first control

            // draw all controls
            if (mainSkin != null)
                GUI.skin = mainSkin;
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

        public bool SaveCfg(string absolutePath)
        {
            try
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

                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public bool LoadCfg(string absolutePath)
        {
            if (!System.IO.File.Exists(absolutePath)) return false;

            try
            {
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

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool LoadSkin(GUISkin _mainSkin, GUISkin _titleSkin)
        {
            if (mainSkin == null || titleSkin == null) return false;
            mainSkin = _mainSkin;
            titleSkin = _titleSkin;

            return true;
        }

        public bool LoadSkinFromAssetBundle(string absolutePathAssetBundle, string mainSkinName, string titleSkinName)
        {
            var asset = AssetBundle.LoadFromFile(absolutePathAssetBundle);

            if (asset == null) return false;

            mainSkin = asset.LoadAsset<GUISkin>(mainSkinName);
            titleSkin = asset.LoadAsset<GUISkin>(titleSkinName);
            return true;
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
            private bool numberIndicator, labelOnLeft;
            private string numberIndicatorFormat;

            internal WSlider(System.Action<float> onValueChanged, float value, float min, float max, bool numberIndicator, string numberIndicatorFormat,
                string displayedString,
                bool labelOnLeft = true)
                : base(displayedString)
            {
                this.onValueChanged = onValueChanged;
                this.value = value;
                this.min = min;
                this.max = max;
                this.numberIndicator = numberIndicator;
                this.numberIndicatorFormat = numberIndicatorFormat;
                this.labelOnLeft = labelOnLeft;
            }

            internal override void Draw(Rect r)
            {

                var newValue = GUIControl.LabelSlider(r, displayedString, value, min, max, numberIndicator, numberIndicatorFormat, labelOnLeft, 5f);

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
            public Color value;

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
                this.value = clr;
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
                GUI.color = value;
                if (GUI.Button(previewButtonRect, string.Empty, whiteButtonGUIStyle))
                    IsPickerOpen = !IsPickerOpen;
                if (GUIWindow.AllWindowsDisabled && IsPickerOpen) GUI.enabled = false;
                GUI.color = oldColor;
            }

            internal void DrawPicker()
            {
                float sliderWidth = 200;
                float sliderHeight = 22;
                value = GUIControl.ColorPicker(new Vector2(rect.x + rect.width - sliderWidth, rect.y + rect.height), value,
                    null, sliderWidth, sliderHeight);
            }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "color", value);
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                value = ini.ReadValue(sectionName, keyBaseName + "color", value);
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

                GUI.EndScrollView();

                GUI.EndGroup();
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
