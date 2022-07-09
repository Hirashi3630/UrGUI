using System;
using System.Collections.Generic;
using UnityEngine;
using UrGUI.Utils;
using UrGUI.UWindow.Utils;
using static UrGUI.Utils.Logger;
using static UrGUI.UWindow.UWindowManager;

namespace UrGUI.UWindow
{
    internal static class GUIWindowControls
    {
        internal class WLabel : WControl
        {
            private Rect _rect = new Rect();

            internal WLabel(
                string displayedString)
                : base(displayedString)
            {
                if (displayedString == null ||
                    string.IsNullOrEmpty(displayedString) ||
                    string.IsNullOrWhiteSpace(displayedString))
                    war("Label is null or empty! Are you sure you don't want to use Space control instead?");
            }

            internal override void Draw(Rect r)
            {
                _rect = r;
                Vector2 pos = new Vector2(_rect.x, _rect.y);
                _rect.x = pos.x;
                _rect.y = pos.y;

                GUI.Label(_rect, DisplayedString);
            }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ExportData(id, sectionName, keyBaseName, ini);

                //ini.WriteValue(sectionName, keyBaseName + "alignment", (int)Alignment); // enum to int
            }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini)
            {
                base.ImportData(id, sectionName, keyBaseName, ini);

                //Alignment = (GUIFormatting.AlignType)ini.ReadValue(sectionName, keyBaseName + "alignment", (int)Alignment); // int to enum
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
                if (UGUI.Button(r, DisplayedString))
                    _onPressed();
            }
        }

        internal class WToggle : WControl
        {
            public readonly System.Action<bool> OnValueChanged;
            
            public bool Value;

            private GUIStyle _toggleStyle = null;

            internal WToggle(System.Action<bool> onValueChanged, bool value,
                string displayedString)
                : base(displayedString)
            {
                this.Value = value;
                this.OnValueChanged = onValueChanged;
            }

            internal override void Draw(Rect r)
            {
                // calculations consider that toggle box is 1:1 ratio (r.height : r.height)
                Rect rToggle = r;
                if (_toggleStyle == null)
                    _toggleStyle = new GUIStyle(GUI.skin.toggle); 

                // change GUIStyle overflow value to draw it on the right side and still trigger LMB
                // (not the prettiest solution but it works)
                int overValue = Mathf.RoundToInt(r.width - r.height);
                _toggleStyle.overflow.left = -overValue;
                _toggleStyle.overflow.right = overValue;
                var newValue = GUI.Toggle(rToggle, Value, string.Empty, _toggleStyle);
                
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
                var newValue = GUIControl.LabelTextField(r, DisplayedString, Value, _maxSymbolLength, _regexReplace, _labelOnLeft);

                // remove everything that matches regex
                if (_regexReplace != string.Empty) 
                    newValue = System.Text.RegularExpressions.Regex.Replace(newValue, _regexReplace, "");
                
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

        internal class WIntField : WControl
        {
            public readonly System.Action<int> OnValueChanged;
            public int Value;

            private string _regexReplace;
            private int _maxSymbolLength;
            private bool _labelOnLeft;

            internal WIntField(System.Action<int> onValueChanged, int value, int maxSymbolLength,
                string displayedString,
                bool labelOnLeft = true)
                : base(displayedString)
            {
                this.OnValueChanged = onValueChanged;
                this.Value = value;
                this._regexReplace = "[^0-9]";
                this._maxSymbolLength = maxSymbolLength;
                this._labelOnLeft = labelOnLeft;
            }

            internal override void Draw(Rect r)
            {
                var stringResult = GUIControl.LabelTextField(r, DisplayedString, Value.ToString(), _maxSymbolLength, _regexReplace, _labelOnLeft);

                // remove everything that matches regex
                if (_regexReplace != string.Empty) 
                    stringResult = System.Text.RegularExpressions.Regex.Replace(stringResult, _regexReplace, "");
                
                // finally parse to int (ignore invalid entries)
                long.TryParse(stringResult, out long longResult);
                int intResult = longResult > int.MaxValue ? int.MaxValue : (int)longResult;
                
                // handle onChangedValue event 
                if (!longResult.Equals(Value))
                    OnValueChanged(intResult);
                Value = intResult;
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

            private string _regexReplace;
            private int _maxSymbolLength;
            private bool _labelOnLeft;


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

                // remove everything that matches regex
                if (_regexReplace != string.Empty) 
                    stringResult = System.Text.RegularExpressions.Regex.Replace(stringResult, _regexReplace, "");

                // replace all commas to dots
                stringResult = stringResult.Replace(',', '.');

                // finally parse to float (ignore invalid entries)
                double.TryParse(stringResult, out double doubleResult);
                float floatResult = (float)doubleResult;

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
                    AllWindowsDisabled = value;
                    if (value)
                        ActiveOptionMenu = DrawPicker;
                    else
                        ActiveOptionMenu = null;
                }
            }
            private bool _isPickerOpen = false;
            private Rect _rect; 

            private readonly GUIStyle _whiteButtonGUIStyle = new GUIStyle
                { normal = new GUIStyleState { background = Texture2D.whiteTexture } };
            
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
                
                bool drawLabel = !string.IsNullOrEmpty(DisplayedString);
                Rect labelRect = new Rect(r.x, r.y, r.width * .666f, r.height);
                Rect previewButtonRect = new Rect(labelRect.x + labelRect.width, r.y, r.width - labelRect.width, r.height);

                if (!drawLabel)
                    previewButtonRect = new Rect(r.x + 2, r.y, r.width - 2, r.height); // 2 = offset
                
                // # COLOR PICKER #
                if (AllWindowsDisabled && IsPickerOpen) GUI.enabled = true;
                
                // draw label
                if (drawLabel)
                    GUI.Label(labelRect, DisplayedString);
                
                // draw preview button
                var oldColor = GUI.color;
                GUI.color = Value;
                if (UGUI.Button(previewButtonRect, string.Empty, _whiteButtonGUIStyle))
                    IsPickerOpen = !IsPickerOpen;
                
                if (AllWindowsDisabled && IsPickerOpen) GUI.enabled = false;
                
                GUI.color = oldColor;
            }

            internal void DrawPicker()
            {
                float sliderWidth = 200;
                float sliderHeight = 22;
                Color newValue = GUIControl.ColorPicker(
                    new Vector2(_rect.x + _rect.width - sliderWidth, _rect.y + _rect.height),
                    Value, true, sliderWidth, sliderHeight);

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
                    AllWindowsDisabled = value;
                    if (value)
                        ActiveOptionMenu = DrawDropDown;
                    else
                        ActiveOptionMenu = null;
                }
            }
            
            private bool _isDropDownOpen = false;
            private Vector2 _scrollPos;
            // private Rect _rect;
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
                    _dropDownOptionGUIStyle = new GUIStyle(GUI.skin.label);
                    _dropDownOptionGUIStyle.hover.textColor = Color.gray;
                    _dropDownOptionGUIStyle.onHover.textColor = Color.gray;
                }

                // calc all rects
                // var margin = 5f;
                // var selectedText = ValuesList[Value];
                // var selectedTextSize = GUIFormatting.GetContentStringSize(selectedText);
                // var selectedButtonWidth = selectedTextSize.x + (margin * 2);
                // _selectedRect = new Rect(r.width - selectedButtonWidth, r.y, selectedButtonWidth, r.height);
                // var labelRect = new Rect(r.x, r.y, r.width - _selectedRect.width, r.height);   

                var offset = 5f;
                
                var selectedText = ValuesList[Value];
                Rect labelRect = new Rect(r.x, r.y, r.width *.5f, r.height);
                _selectedRect = new Rect(r.x + labelRect.width + offset, r.y, r.width - labelRect.width - offset, r.height);

                if (string.IsNullOrEmpty(DisplayedString))
                    _selectedRect = r;
                
                // draw opened selection list
                if (AllWindowsDisabled && IsDropDownOpen) GUI.enabled = true;
                
                if (!string.IsNullOrEmpty(DisplayedString))
                    GUI.Label(labelRect, DisplayedString);
                
                if (UGUI.Button(_selectedRect, selectedText))
                    IsDropDownOpen = !IsDropDownOpen;
                //GUI.Label(selectedLabelRect, selectedText);
                if (AllWindowsDisabled && IsDropDownOpen) GUI.enabled = false;
            }

            internal void DrawDropDown()
            {
                var newValue = GUIControl.DropDown(new Vector2(_selectedRect.x, _selectedRect.y + _selectedRect.height),
                    ValuesList, _scrollPos, out _scrollPos, out bool isOpen, _dropDownOptionGUIStyle, true, _selectedRect.width);

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

        internal class WSeparator : WControl
        {
            private Color _lineColor;
            private float _lineThickness;
            
            public WSeparator(Color lineColor, float lineThickness)
                : base(String.Empty)
            {
                _lineColor = lineColor;
                _lineThickness = lineThickness;
            }

            internal override void Draw(Rect r)
            {
                var clr = _lineColor;

                // change alpha if gui is disabled
                if (!GUI.enabled) clr = _lineColor * new Color(1, 1, 1, .5f);//new Color(_lineColor.r, _lineColor.g, _lineColor.b, _lineColor.a * .5f);
                
                // var rLine = new Rect(r.x, r.y, r.width, _lineThickness); // up
                var rLine = new Rect(r.x, r.y + (r.height / 2f) - (_lineThickness / 2f), r.width, _lineThickness); // middle

                GUIControl.ColoredBox(rLine, clr);
            }
        }
        
        internal class WSpace : WControl
        {
            internal WSpace() : base(string.Empty) { }

            internal override void Draw(Rect r) { }

            internal override void ExportData(int id, string sectionName, string keyBaseName, INIParser ini) { }

            internal override void ImportData(int id, string sectionName, string keyBaseName, INIParser ini) { }
        }
    }
    
    public abstract class WControl
    {
        internal float SameLineRatio = 1;
        
        internal string DisplayedString = null;

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
