using System.Collections.Generic;
using UnityEngine;
using UrGUI.Utils;

namespace UrGUI
{
    public class GUIWindow
    {
        protected GUIWindow() { }

        private static readonly GUIStyle WhiteButtonGUIStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.whiteTexture } };

        public static float DynamicWindowsCurrentX = 0;
        public static readonly float DynamicWindowsMarginX = 10;
        
        public static bool AllWindowsDisabled = false;
        public static bool AnyWindowDragging = false;
        public static System.Action ActiveOptionMenu = null;
        public static GUIWindow DialogDrawer = null;

        public bool IsEnabled { get; set; }

        private string _windowTitle;
        private float _x, _y, _width, _height, _margin, _controlHeight, _controlSpace;
        private bool _isDraggable;

        private bool _isDragging;

        private float _nextControlY;
        private List<WControl> _controls;

        private GUISkin _mainSkin = null;
        private GUISkin _titleSkin = null;

        /// <summary>
        /// Create a GUIWindow with dynamic position and size
        /// </summary>
        /// <param name="windowTitle">Window's title shown in the header</param>
        /// <param name="startWidth">Window's width</param>
        /// <param name="startHeight">Window's height</param>
        /// <param name="margin">Margin around individual controls</param>
        /// <param name="controlHeight">Control's height</param>
        /// <param name="controlSpace">Vertical space between controls</param>
        /// <param name="isEnabled"></param>
        /// <param name="isDraggable">Ability to move control in runtime by dragging it with a mouse by header</param>
        /// <returns></returns>
        public static GUIWindow Begin(string windowTitle = "a Title", float startWidth = 200, float startHeight = 400,
            float margin = 10, float controlHeight = 22, float controlSpace = 5, bool isEnabled = true, bool isDraggable = true)
        {
            DynamicWindowsCurrentX += DynamicWindowsMarginX;
            
            // get dynamic X position based on previously created GUIWindows
            float x = DynamicWindowsCurrentX;
            float y = DynamicWindowsMarginX;
            // set default size
            float width = startWidth;
            float height = startHeight;

            DynamicWindowsCurrentX += width + DynamicWindowsMarginX;

            return Begin(windowTitle, x, y, width, height, margin, controlHeight, controlSpace, isEnabled, isDraggable);
        }


        /// <summary>
        /// Create a GUIWindow
        /// </summary>
        /// <param name="windowTitle">Window's title shown in the header</param>
        /// <param name="startX">Starting X position ([0;0] is in the top left)</param>
        /// <param name="startY">Starting Y position ([0;0] is in the top left)</param>
        /// <param name="startWidth">Width of the window</param>
        /// <param name="startHeight">Height of the window</param>
        /// <param name="margin">Margin around individual controls</param>
        /// <param name="controlHeight">Control's height</param>
        /// <param name="controlSpace">Vertical space between controls</param>
        /// <param name="isEnabled"></param>
        /// <param name="isDraggable">Ability to move control in runtime by dragging it with a mouse by header</param>
        /// <returns></returns>
        public static GUIWindow Begin(string windowTitle, float startX, float startY, float startWidth, float startHeight,
            float margin = 10, float controlHeight = 22, float controlSpace = 5, bool isEnabled = true, bool isDraggable = true)
        {
            GUIWindow b = new GUIWindow();
            b.IsEnabled = isEnabled;
            b._windowTitle = windowTitle;
            b._x = startX;
            b._y = startY;
            b._width = startWidth;
            b._height = startHeight;
            b._margin = margin;
            b._controlHeight = controlHeight;
            b._controlSpace = controlSpace;
            b._isDraggable = isDraggable;

            b._controls = new List<WControl>();

            return b;
        }

        /// <summary>
        /// Add control manually (to do it dynamically use methods such as: Label(); Button(); Space()...)
        /// </summary>
        /// <param name="c"></param>
        public void Add(WControl c)
        {
            _controls.Add(c);
        }

        /// <summary>
        /// Should be called in UnityEngine.OnGUI() method
        /// </summary>
        public void Draw()
        {
            // check if this window is enabled
            if (!IsEnabled) return;

            // disable if it's required
            if (AllWindowsDisabled) GUI.enabled = false;

            // handle drag
            if (_isDraggable)
            {
                // mouse drag
                if (!AnyWindowDragging || _isDragging)
                {
                    var e = Event.current;
                    if (e.type == EventType.MouseDown && new Rect(_x, _y, _width, _controlHeight * 1.25f).Contains(e.mousePosition))
                    {
                        _isDragging = true;
                        AnyWindowDragging = true;
                    }
                    else if (e.type == EventType.MouseUp)
                    {
                        _isDragging = false;
                        AnyWindowDragging = false;
                    }
                    if (e.type == EventType.MouseDrag && _isDragging)
                    {
                        _x += e.delta.x;
                        _y += e.delta.y;
                    }
                }
            }

            // check if window isn't outside of screen
            if (_x < 0) _x = 0;
            if (_y < 0) _y = 0;
            if (_x + _width > Screen.width) _x = Screen.width - _width;
            if (_y + _height > Screen.height) _y = Screen.height - _height;

            // reset nextControlY
            _nextControlY = _y + _controlHeight + _margin;

            // disable gui if it's dragging
            if (_isDragging)
                GUI.enabled = false;

            // Main window
            if (_mainSkin != null)
                GUI.skin = _mainSkin;
            GUI.Box(new Rect(_x, _y, _width, _height), "");

            // window's title
            if (_titleSkin != null)
                GUI.skin = _titleSkin;
            GUI.Box(new Rect(_x, _y, _width, 25f), _windowTitle);
            //nextControlY += controlSpace; // add more space between title and first control

            // draw all controls
            if (_mainSkin != null)
                GUI.skin = _mainSkin;
            foreach (var c in _controls)
                c.Draw(NextControlRect());

            // reset GUI enabled
            GUI.enabled = true;

            // draw active option menu
            if (DialogDrawer == null)
                DialogDrawer = this;
            if (DialogDrawer == this)
                if (ActiveOptionMenu != null)
                    ActiveOptionMenu();
        }

        private Rect NextControlRect()
        {
            Rect r = new Rect(_x + _margin, _nextControlY, _width - (_margin * 2), _controlHeight);
            _nextControlY += _controlHeight + _controlSpace;
            return r;
        }

        /// <summary>
        /// Save a configuration of GUIWindow that can be later loaded
        /// </summary>
        /// <param name="absolutePath">Absolute path to .ini file (created if it does not exists)</param>
        /// <returns>true if succesfull</returns>
        public bool SaveCfg(string absolutePath)
        {
            try
            {
                INIParser ini = new INIParser();
                ini.Open(absolutePath);

                string sectionName = $"GUIWindow.{_windowTitle}";
                // write all values for GUIWindow
                ini.WriteValue(sectionName, "windowTitle", _windowTitle);
                ini.WriteValue(sectionName, "x", _x);
                ini.WriteValue(sectionName, "y", _y);
                ini.WriteValue(sectionName, "width", _width);
                ini.WriteValue(sectionName, "height", _height);
                ini.WriteValue(sectionName, "margin", _margin);
                ini.WriteValue(sectionName, "controlHeight", _controlHeight);
                ini.WriteValue(sectionName, "controlSpace", _controlSpace);

                // write all controls' values
                for (int i = 0; i < _controls.Count; i++)
                    _controls[i].ExportData(i, sectionName, $"Control{i}.", ini);

                // close and save
                ini.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loads a configuration of GUIWindow and applies it 
        /// </summary>
        /// <param name="absolutePath">Absolute path to .ini file</param>
        /// <returns>true if succesfull</returns>
        public bool LoadCfg(string absolutePath)
        {
            if (!System.IO.File.Exists(absolutePath)) return false;

            try
            {
                INIParser ini = new INIParser();
                ini.Open(absolutePath);

                string sectionName = $"GUIWindow.{_windowTitle}";
                // read all values for GUIWindow
                _windowTitle = ini.ReadValue(sectionName, "windowTitle", _windowTitle);
                _x = ini.ReadValue(sectionName, "x", _x);
                _y = ini.ReadValue(sectionName, "y", _y);
                _width = ini.ReadValue(sectionName, "width", _width);
                _height = ini.ReadValue(sectionName, "height", _height);
                _margin = ini.ReadValue(sectionName, "margin", _margin);
                _controlHeight = ini.ReadValue(sectionName, "controlHeight", _controlHeight);
                _controlSpace = ini.ReadValue(sectionName, "controlSpace", _controlSpace);

                // read all controls' values
                for (int i = 0; i < _controls.Count; i++)
                    _controls[i].ImportData(i, sectionName, $"Control{i}.", ini);

                // close and save
                ini.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool LoadSkin(GUISkin mainSkin, GUISkin titleSkin)
        {
            if (this._mainSkin == null || this._titleSkin == null) return false;
            this._mainSkin = mainSkin;
            this._titleSkin = titleSkin;

            return true;
        }

        public bool LoadSkinFromAssetBundle(string absolutePathAssetBundle, string mainSkinName, string titleSkinName)
        {
            var asset = AssetBundle.LoadFromFile(absolutePathAssetBundle);

            if (asset == null) return false;

            _mainSkin = asset.LoadAsset<GUISkin>(mainSkinName);
            _titleSkin = asset.LoadAsset<GUISkin>(titleSkinName);
            return true;
        }

        #region CONTROLS

        //#################
        // ### CONTROLS ###
        // ################

        /// <summary>
        /// a blank control for organizing
        /// </summary>
        public void Space()
        {
            var c = new WSpace();
            Add(c);
        }

        /// <summary>
        /// a simple text control
        /// </summary>
        /// <param name="text">label's text</param>
        /// <param name="alignment">aligment inside of control box</param>
        public void Label(string text,
                GUIFormatting.AlignType alignment = GUIFormatting.AlignType.LeftTop)
        {
            var c = new WLabel(text, alignment);
            Add(c);
        }

        /// <summary>
        /// a simple button
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onPressed"></param>
        public void Button(string text, System.Action onPressed)
        {
            var c = new WButton(onPressed, text);
            Add(c);
        }

        /// <summary>
        /// a simple on-off control with label
        /// </summary>
        /// <param name="text">label's</param>
        /// <param name="onValueChanged"></param>
        /// <param name="value">default value</param>
        public void Toggle(string text, System.Action<bool> onValueChanged,
                bool value = false)
        {
            var c = new WToggle(onValueChanged, value, text);
            Add(c);
        }

        /// <summary>
        /// simple slider control with label
        /// </summary>
        /// <param name="text">label's text</param>
        /// <param name="onValueChanged"></param>
        /// <param name="value">default value</param>
        /// <param name="min">minimal value</param>
        /// <param name="max">maximal value</param>
        /// <param name="numberIndicator">if number indicator showing current value</param>
        /// <param name="numberIndicatorFormat">String.Format value of indicator</param>
        public void Slider(string text, System.Action<float> onValueChanged, float value, float min, float max,
                bool numberIndicator = false, string numberIndicatorFormat = "0.##")
        {
            var c = new WSlider(onValueChanged, value, min, max, numberIndicator, numberIndicatorFormat, text);
            Add(c);
        }

        /// <summary>
        /// a simple text field control with label
        /// </summary>
        /// <param name="text">label's text</param>
        /// <param name="onValueChanged"></param>
        /// <param name="value">default value</param>
        /// <param name="maxSymbolLength">maximum number of characters</param>
        /// <param name="regexReplace">regex filter for unwanted characters typed by user</param>
        public void TextField(string text, System.Action<string> onValueChanged, string value, int maxSymbolLength,
                string regexReplace = "")
        {
            var c = new WTextField(onValueChanged, value, maxSymbolLength, regexReplace, text);
            Add(c);
        }

        /// <summary>
        /// a simple float field control with label
        /// </summary>
        /// <param name="text">label's text</param>
        /// <param name="onValueChanged"></param>
        /// <param name="value">default value</param>
        /// <param name="maxSymbolLength">maximum number of characters</param>
        public void FloatField(string text, System.Action<float> onValueChanged, float value, int maxSymbolLength)
        {
            var c = new WFloatField(onValueChanged, value, maxSymbolLength, text);
            Add(c);
        }

        /// <summary>
        /// a RGBA color picking control with popup dialog (runtime update)
        /// </summary>
        /// <param name="text">label's text</param>
        /// <param name="onValueChanged"></param>
        /// <param name="value">default value</param>
        public void ColorPicker(string text, System.Action<Color> onValueChanged, Color value)
        {
            var c = new WColorPicker(onValueChanged, value, text);
            Add(c);
        }

        /// <summary>
        /// a multi-selection popup dialog
        /// </summary>
        /// <param name="text">label's text</param>
        /// <param name="onValueChanged"></param>
        /// <param name="value">default selected id</param>
        /// <param name="list">list that holds all possible values</param>
        public void DropDown(string text, System.Action<int> onValueChanged, int value, Dictionary<int, string> list)
        {
            var c = new WDropDown(onValueChanged, value, list, text);
            Add(c);
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
            public GUIFormatting.AlignType Alignment;

            private Rect _rect = new Rect();

            internal WLabel(
                string displayedString, GUIFormatting.AlignType alignment)
                : base(displayedString)
            {
                this.Alignment = alignment;
            }

            internal override void Draw(Rect r)
            {
                _rect = r;
                Vector2 pos = GUIFormatting.AlignControl(Alignment, new Vector2(_rect.x, _rect.y),
                    GUIFormatting.GetContentStringSize(DisplayedString), new Vector2(_rect.width, _rect.height));
                _rect.x = pos.x;
                _rect.y = pos.y;

                GUI.Label(_rect, DisplayedString);
            }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "alignment", (int)Alignment); // enum to int
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                Alignment = (GUIFormatting.AlignType)ini.ReadValue(sectionName, keyBaseName + "alignment", (int)Alignment); // int to enum
            }
        }

        internal class WButton : WControl
        {
            private readonly System.Action _onPressed;

            internal WButton(System.Action onPressed,
                string displayedString)
                : base(displayedString)
            {
                this._onPressed = onPressed;
            }

            internal override void Draw(Rect r)
            {
                if (GUI.Button(r, DisplayedString))
                    _onPressed();
            }
        }

        internal class WToggle : WControl
        {
            public readonly System.Action<bool> OnValueChanged;
            
            public bool Value;

            internal WToggle(System.Action<bool> onValueChanged, bool value,
                string displayedString)
                : base(displayedString)
            {
                this.Value = value;
                this.OnValueChanged = onValueChanged;
            }

            internal override void Draw(Rect r)
            {
                var newValue = GUI.Toggle(r, Value, DisplayedString);
                if (newValue != Value)
                    OnValueChanged(newValue);
                Value = newValue;
            }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "value", Value);
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                Value = ini.ReadValue(sectionName, keyBaseName + "value", Value);
            }
        }

        internal class WSlider : WControl
        {
            public readonly System.Action<float> OnValueChanged;
            public float Value;

            private float _min, _max;
            private bool _numberIndicator, _labelOnLeft;
            private string _numberIndicatorFormat;

            internal WSlider(System.Action<float> onValueChanged, float value, float min, float max, bool numberIndicator, string numberIndicatorFormat,
                string displayedString,
                bool labelOnLeft = true)
                : base(displayedString)
            {
                this.OnValueChanged = onValueChanged;
                this.Value = value;
                this._min = min;
                this._max = max;
                this._numberIndicator = numberIndicator;
                this._numberIndicatorFormat = numberIndicatorFormat;
                this._labelOnLeft = labelOnLeft;
            }

            internal override void Draw(Rect r)
            {
                var newValue = GUIControl.LabelSlider(r, DisplayedString, Value, _min, _max, _numberIndicator, _numberIndicatorFormat, _labelOnLeft, 5f);

                // handle onChangedValue event 
                if (!newValue.Equals(Value))
                    OnValueChanged(newValue);
                Value = newValue; 
            }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "value", Value);
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                Value = ini.ReadValue(sectionName, keyBaseName + "value", Value);
            }
        }

        internal class WTextField : WControl
        {
            public readonly System.Action<string> OnValueChanged;
            public string Value;

            private string _regexReplace;
            private int _maxSymbolLength;
            private bool _labelOnLeft;

            internal WTextField(System.Action<string> onValueChanged, string value, int maxSymbolLength, string regexReplace,
                string displayedString,
                bool labelOnLeft = true)
                : base(displayedString)
            {
                this.OnValueChanged = onValueChanged;
                this.Value = value;
                this._regexReplace = regexReplace;
                this._maxSymbolLength = maxSymbolLength;
                this._labelOnLeft = labelOnLeft;
            }

            internal override void Draw(Rect r)
            {
                var newValue = GUIControl.LabelTextField(r, DisplayedString, Value, _maxSymbolLength, _regexReplace, _labelOnLeft, 5f);

                // handle onChangedValue event 
                if (newValue != Value)
                    OnValueChanged(newValue);
                Value = newValue;
            }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "value", Value);
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                Value = ini.ReadValue(sectionName, keyBaseName + "value", Value);
            }
        }

        internal class WFloatField : WControl
        {
            public readonly System.Action<float> OnValueChanged;
            public float Value;

            private int _maxSymbolLength;
            private bool _labelOnLeft;

            private readonly string _regexReplace;

            internal WFloatField(System.Action<float> onValueChanged, float value, int maxSymbolLength,
                string displayedString,
                bool labelOnLeft = true)
                : base(displayedString)
            {
                this.OnValueChanged = onValueChanged;
                this.Value = value;
                this._regexReplace = "[^0-9.,]";
                this._maxSymbolLength = maxSymbolLength;
                this._labelOnLeft = labelOnLeft;
            }

            internal override void Draw(Rect r)
            {
                // create TextField and replace all symbols that are not numbers or ',' / '.'
                var stringResult = GUIControl.LabelTextField(r, DisplayedString, Value.ToString(), _maxSymbolLength, _regexReplace, _labelOnLeft, 5f);

                // replace all commas to dots
                stringResult = stringResult.Replace(',', '.');

                // finally parse to float (ignore invalid entries)
                float.TryParse(stringResult, out float floatResult);

                // handle onChangedValue event 
                if (!floatResult.Equals(Value))
                    OnValueChanged(floatResult);
                Value = floatResult;
            }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "value", Value);
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                Value = ini.ReadValue(sectionName, keyBaseName + "value", Value);
            }
        }

        internal class WColorPicker : WControl
        {
            public Color Value;

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
            private Rect _rect;
            internal WColorPicker(System.Action<Color> onValueChanged, Color clr,
                string displayedString)
                : base(displayedString)
            {
                this.onValueChanged = onValueChanged;
                this.Value = clr;
            }

            internal override void Draw(Rect r)
            {
                _rect = r;
                Rect previewButtonRect = _rect;
                previewButtonRect.width /= 3f;
                previewButtonRect.x += previewButtonRect.width * 2f;
                Rect labelRect = _rect;
                labelRect.width *= 0.66666f;

                // # COLOR PICKER #
                // draw preview button
                if (GUIWindow.AllWindowsDisabled && IsPickerOpen) GUI.enabled = true;
                GUI.Label(labelRect, DisplayedString);
                var oldColor = GUI.color;
                GUI.color = Value;
                if (GUI.Button(previewButtonRect, string.Empty, WhiteButtonGUIStyle))
                    IsPickerOpen = !IsPickerOpen;
                if (GUIWindow.AllWindowsDisabled && IsPickerOpen) GUI.enabled = false;
                GUI.color = oldColor;
            }

            internal void DrawPicker()
            {
                float sliderWidth = 200;
                float sliderHeight = 22;
                Color newValue = GUIControl.ColorPicker(new Vector2(_rect.x + _rect.width - sliderWidth, _rect.y + _rect.height), Value,
                    false, sliderWidth, sliderHeight);

                // handle event
                if (newValue != Value)
                    onValueChanged.Invoke(newValue);

                Value = newValue;
            }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "color", Value);
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                Value = ini.ReadValue(sectionName, keyBaseName + "color", Value);
            }
        }

        internal class WDropDown : WControl
        {
            public readonly System.Action<int> OnValueChanged;
            public readonly Dictionary<int, string> ValuesList;
            public int Value;
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
            private Vector2 _scrollPos;
            private Rect _rect;
            private Rect _selectedRect;
            private static GUIStyle _dropDownOptionGUIStyle = null; //= GUI.skin.label;

            internal WDropDown(System.Action<int> onValueChanged, int value, Dictionary<int, string> valuesList,
                string displayedString)
                : base(displayedString)
            {
                this.OnValueChanged = onValueChanged;
                this.Value = value;
                this.ValuesList = valuesList;
            }

            internal override void Draw(Rect r)
            {
                // init
                if (_dropDownOptionGUIStyle == null)
                {
                    _dropDownOptionGUIStyle = GUI.skin.label;
                    _dropDownOptionGUIStyle.hover.textColor = Color.gray;
                    _dropDownOptionGUIStyle.onHover.textColor = Color.gray;
                }

                // calc all rects
                _rect = r;
                Rect labelRect = _rect;
                labelRect.width /= 3f;
                _selectedRect = _rect;
                _selectedRect.x += labelRect.width;
                _selectedRect.width = labelRect.width * 2f;
                Rect selectedLabelRect = _selectedRect;
                selectedLabelRect.x += 5f; // offset
                selectedLabelRect.width -= 5f; // offset

                if (GUIWindow.AllWindowsDisabled && IsDropDownOpen) GUI.enabled = true;
                GUI.Label(labelRect, DisplayedString);
                if (GUI.Button(_selectedRect, string.Empty))
                    IsDropDownOpen = !IsDropDownOpen;
                GUI.Label(selectedLabelRect, ValuesList[Value]);
                if (GUIWindow.AllWindowsDisabled && IsDropDownOpen) GUI.enabled = false;
            }

            internal void DrawDropDown()
            {
                var newValue = GUIControl.DropDown(new Vector2(_selectedRect.x, _selectedRect.y + _selectedRect.height),
                    ValuesList, _scrollPos, out _scrollPos, out bool isOpen, _dropDownOptionGUIStyle, false, _selectedRect.width);

                // handle event
                if (newValue >= 0 & newValue != Value)
                    OnValueChanged.Invoke(newValue);

                if (newValue >= 0)
                    Value = newValue;

                IsDropDownOpen = isOpen;
            }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                ini.WriteValue(sectionName, keyBaseName + "value", Value);
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                Value = ini.ReadValue(sectionName, keyBaseName + "value", Value);
            }
        }

        public abstract class WControl
        {
            internal string DisplayedString;

            internal WControl(string displayedString)
            {
                this.DisplayedString = displayedString;
            }

            internal abstract void Draw(Rect r);

            internal virtual void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                ini.WriteValue(sectionName, keyBaseName + "displayString", DisplayedString);
            }

            internal virtual void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                DisplayedString = ini.ReadValue(sectionName, keyBaseName + "displayString", DisplayedString);
            }
        }
    }
    #endregion
}
