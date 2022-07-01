using System.Collections.Generic;
using UnityEngine;
using UrGUI.ImGUI.Utils;
using UrGUI.Utils;

namespace UrGUI.ImGUI
{
    internal static class GUIWindowControls
    {
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

            private readonly System.Action<Color> _onValueChanged;
            private bool IsPickerOpen { get => _isPickerOpen; set
                {
                    _isPickerOpen = value;
                    GUIWindow.AllWindowsDisabled = value;
                    if (value)
                        GUIWindow.ActiveOptionMenu = DrawPicker;
                    else
                        GUIWindow.ActiveOptionMenu = null;
                }
            }
            private bool _isPickerOpen = false;
            private Rect _rect;
            internal WColorPicker(System.Action<Color> onValueChanged, Color clr,
                string displayedString)
                : base(displayedString)
            {
                this._onValueChanged = onValueChanged;
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
                if (GUI.Button(previewButtonRect, string.Empty, GUIWindow.WhiteButtonGUIStyle))
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
                    _onValueChanged.Invoke(newValue);

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
                        GUIWindow.ActiveOptionMenu = DrawDropDown;
                    else
                        GUIWindow.ActiveOptionMenu = null;
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

                // draw opened selection list
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
    }
    
    public abstract class WControl
    {
        internal float SameLineRatio = 1;
        
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
