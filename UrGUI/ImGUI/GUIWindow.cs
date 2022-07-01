using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UrGUI.ImGUI.Utils;
using UrGUI.Utils;

namespace UrGUI.ImGUI
{
    public class GUIWindow
    {
        protected GUIWindow()
        {
        }

        public static readonly GUIStyle WhiteButtonGUIStyle = new GUIStyle
            { normal = new GUIStyleState { background = Texture2D.whiteTexture } };

        public static float DynamicWindowsCurrentX = 0;
        public static readonly float DynamicWindowsMarginX = 10;

        public static bool AllWindowsDisabled = false;
        public static bool AnyWindowDragging = false;
        public static System.Action ActiveOptionMenu = null;
        public static GUIWindow DialogDrawer = null;

        public bool IsEnabled { get; set; }

        private string _windowTitle;
        private float _x, _y, _width, _height, _margin, _controlHeight, _controlSpace, _sameLineOffset;
        private bool _isDraggable;

        private List<float> _nextLineRatios = new List<float>(0);

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
            float margin = 10, float controlHeight = 22, float controlSpace = 5, bool isEnabled = true,
            bool isDraggable = true)
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
        public static GUIWindow Begin(string windowTitle, float startX, float startY, float startWidth,
            float startHeight,
            float margin = 10, float controlHeight = 22, float controlSpace = 5, bool isEnabled = true,
            bool isDraggable = true)
        {
            GUIWindow b = new GUIWindow
            {
                IsEnabled = isEnabled,
                _windowTitle = windowTitle,
                _x = startX,
                _y = startY,
                _width = startWidth,
                _height = startHeight,
                _margin = margin,
                _controlHeight = controlHeight,
                _controlSpace = controlSpace,
                _isDraggable = isDraggable,
                _controls = new List<WControl>()
            };

            return b;
        }

        /// <summary>
        /// Add control manually (to do it dynamically use methods such as: Label(); Button(); Space()...)
        /// </summary>
        /// <param name="c"></param>
        public void Add(WControl c)
        {
            _controls.Add(c);

            // check if control control is suppose to be on the same line
            if (_nextLineRatios.Count > 0)
            {
                c.SameLineRatio = _nextLineRatios[0];
                _nextLineRatios.RemoveAt(0);
            }
        }

        /// <summary>
        /// Put 2+ controls next to each other (-1 == not included)
        /// functions on ratio points (15,5) == 3:1
        /// </summary>
        /// <param name="ratio1">(R) ratio of first control</param>
        /// <param name="ratio2">(R)ratio of second control</param>
        /// <param name="ratio3">ratio of third control</param>
        /// <param name="ratio4">ratio of fourth control</param>
        /// <param name="ratio5">ratio of fifth control</param>
        /// <returns>true if successful</returns>
        public bool SameLine(int ratio1 = 1, int ratio2 = 1, int ratio3 = -1, int ratio4 = -1, int ratio5 = -1)
        {
            if (_nextLineRatios.Count > 0) return false;
            if (ratio1 <= 0 || ratio2 <= 0) return false;

            // put all ratios in one list
            List<int> ratios = new List<int>() { ratio1, ratio2 };
            if (ratio3 > 0)
            {
                ratios.Add(ratio3);
                if (ratio4 > 0)
                {
                    ratios.Add(ratio4);
                    if (ratio5 > 0)
                        ratios.Add(ratio5);
                }
            }

            // normalize ratios
            int lowest = ratios.Min();
            List<float> normalizedRatios = new List<float>();
            foreach (var r in ratios)
                normalizedRatios.Add(r / (float)lowest);

            // calculate 
            float nOfNormalized = normalizedRatios.Sum();
            normalizedRatios.Clear();
            foreach (var r in ratios)
                normalizedRatios.Add(r / nOfNormalized);

            _nextLineRatios = normalizedRatios;
            return true;
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
                    if (e.type == EventType.MouseDown &&
                        new Rect(_x, _y, _width, _controlHeight * 1.25f).Contains(e.mousePosition))
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

            // draw controls
            foreach (var c in _controls)
                c.Draw(NextControlRect(c.SameLineRatio));
            

            // reset GUI enabled
            GUI.enabled = true;

            // draw active option menu
            if (DialogDrawer == null)
                DialogDrawer = this;
            if (DialogDrawer == this)
                if (ActiveOptionMenu != null)
                    ActiveOptionMenu();
        }

        private Rect NextControlRect(float sameLineRatio)
        {
            var fullControlWidth = _width - (_margin * 2);
            Rect r = new Rect(_x + _margin + _sameLineOffset, _nextControlY, fullControlWidth, _controlHeight);
            
            if (sameLineRatio.Equals(1)) 
                _nextControlY += _controlHeight + _controlSpace;
            
            // handle same line
            else 
            {
                r.width *= sameLineRatio;
                _sameLineOffset += r.width;
                
                // reset if it's last control
                if (_sameLineOffset.Equals(fullControlWidth))
                {
                    _nextControlY += _controlHeight + _controlSpace; // offset Y for next control
                    _sameLineOffset = 0;
                }
            }

            return r;
        }

        /// <summary>
        /// Save a configuration of GUIWindow that can be later loaded
        /// </summary>
        /// <param name="absolutePath">Absolute path to .ini file (created if it does not exists)</param>
        /// <returns>true if successful</returns>
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
        /// <returns>true if successful</returns>
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
            var c = new GUIWindowControls.WSpace();
            Add(c);
        }

        /// <summary>
        /// a simple text control
        /// </summary>
        /// <param name="text">label's text</param>
        /// <param name="alignment">alignment inside of control box</param>
        public void Label(string text,
            GUIFormatting.AlignType alignment = GUIFormatting.AlignType.LeftTop)
        {
            var c = new GUIWindowControls.WLabel(text, alignment);
            Add(c);
        }

        /// <summary>
        /// a simple button
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onPressed"></param>
        public void Button(string text, System.Action onPressed)
        {
            var c = new GUIWindowControls.WButton(onPressed, text);
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
            var c = new GUIWindowControls.WToggle(onValueChanged, value, text);
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
            var c = new GUIWindowControls.WSlider(onValueChanged, value, min, max, numberIndicator,
                numberIndicatorFormat, text);
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
            var c = new GUIWindowControls.WTextField(onValueChanged, value, maxSymbolLength, regexReplace, text);
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
            var c = new GUIWindowControls.WFloatField(onValueChanged, value, maxSymbolLength, text);
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
            var c = new GUIWindowControls.WColorPicker(onValueChanged, value, text);
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
            var c = new GUIWindowControls.WDropDown(onValueChanged, value, list, text);
            Add(c);
        }

        #endregion
    }
}