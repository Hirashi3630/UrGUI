using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UrGUI.Utils;
using static UrGUI.UWindow.UWindowManager;


namespace UrGUI.UWindow
{
    public class UWindow
    {
        private protected UWindow()
        {
        }
        
        /// <summary>
        /// unique UWindow's ID
        /// </summary>
        public string WinGuid;
        
        /// <summary>
        /// This UWindow's active skin
        /// </summary>
        public GUISkin ActiveSkin { get; internal set; } = null;
        
        /// <summary>
        /// Is window being drawn
        /// </summary>
        public bool IsDrawing { get; set; }

        /// <summary>
        /// Window's title
        /// </summary>
        public string WindowTitle;
        /// <summary>
        /// Window's rect
        /// </summary>
        public float X, Y, Width, Height;
        /// <summary>
        /// 
        /// </summary>
        public bool IsDraggable, DynamicHeight;
        
        private float _startX, _startY, _margin, _controlHeight, _controlSpace, _sameLineOffset;

        private List<float> _nextLineRatios = new List<float>(0);

        private bool _isDragging;

        private float _nextControlY;
        private List<WControl> _controls;

        /// <summary>
        /// Create a UWindow with dynamic position and size
        /// </summary>
        /// <param name="windowTitle">Window's title shown in the header</param>
        /// <param name="startWidth">Window's width</param>
        /// <param name="margin">Margin around individual controls</param>
        /// <param name="controlHeight">Control's height</param>
        /// <param name="controlSpace">Vertical space between controls</param>
        /// <param name="isEnabled"></param>
        /// <param name="isDraggable">Ability to move control in runtime by dragging it with a mouse by header</param>
        /// <param name="dynamicHeight">If true, height is ignored</param>
        /// <returns></returns>
        public static UWindow Begin(string windowTitle = "a Title", float startWidth = 200,
            float margin = 10, float controlHeight = 22, float controlSpace = 5, bool isEnabled = true,
            bool isDraggable = true, bool dynamicHeight = true)
        {
            // set default size
            var width = startWidth;
            var height = 400; // this will be calculated after first added control

            var pos = GetDynamicWindowPos(width);

            return Begin(windowTitle, pos.x, pos.y, width, height, margin, controlHeight, controlSpace, isEnabled, isDraggable, dynamicHeight);
        }

        /// <summary>
        /// Create a UWindow
        /// </summary>
        /// <param name="windowTitle">Window's title shown in the header</param>
        /// <param name="startX">Starting X position ([0;0] is in the top left)</param>
        /// <param name="startY">Starting Y position ([0;0] is in the top left)</param>
        /// <param name="startWidth">Width of the window</param>
        /// <param name="startHeight">Height of the window</param>
        /// <param name="margin">Margin around individual controls</param>
        /// <param name="controlHeight">Control's height</param>
        /// <param name="controlSpace">Vertical space between controls</param>
        /// <param name="isDrawing"></param>
        /// <param name="isDraggable">Ability to move control in runtime by dragging it with a mouse by header</param>
        /// <param name="dynamicHeight">If true, startHeight is ignored</param>
        /// <returns></returns>
        public static UWindow Begin(string windowTitle, float startX, float startY, float startWidth, float startHeight,
            float margin = 10, float controlHeight = 22, float controlSpace = 5, bool isDrawing = true,
            bool isDraggable = true, bool dynamicHeight = false)
        {
            UWindow b = new UWindow
            {
                IsDrawing = isDrawing,
                WindowTitle = windowTitle,
                X = startX,
                Y = startY,
                Width = startWidth,
                Height = startHeight,
                _margin = margin,
                _controlHeight = controlHeight,
                _controlSpace = controlSpace,
                IsDraggable = isDraggable,
                DynamicHeight = dynamicHeight,
                _controls = new List<WControl>()
            };

            b._startX = startX;
            b._startY = startY;

            // register this UWindow to the manager
            Register(b);
            
            return b;
        }

        /// <summary>
        /// Should be called in UnityEngine.OnGUI() method
        /// </summary>
        internal void Draw()
        {
            // check if this window is enabled
            if (!IsDrawing) return;
            
            // load skin
            if (ActiveSkin != null)
                GUI.skin = ActiveSkin;
            
            // disable if it's required
            if (AllWindowsDisabled) GUI.enabled = false;

            // if mouse click anywhere in this window, bring to front
            var e = Event.current;
            if (e.type == EventType.MouseDown &&
                new Rect(X,Y,Width,Height).Contains(e.mousePosition))
                BringUWindowToFront(this);
            
            // handle drag
            if (IsDraggable)
            {
                // mouse drag
                if (!AnyWindowDragging || _isDragging)
                {
                    if (e.type == EventType.MouseDown &&
                        new Rect(X, Y, Width, _controlHeight * 1.25f).Contains(e.mousePosition))
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
                        X += e.delta.x;
                        Y += e.delta.y;
                    }
                }
            }

            // check if window isn't outside of screen
            if (X < 0) X = 0;
            if (Y < 0) Y = 0;
            if (X + Width > Screen.width) X = Screen.width - Width;
            if (Y + Height > Screen.height) Y = Screen.height - Height;

            // reset nextControlY
            _nextControlY = Y + _controlHeight + _margin;

            // disable gui if it's dragging
            if (_isDragging)
                GUI.enabled = false;

            // Main window
            GUI.Box(new Rect(X, Y, Width, Height), "");

            // window's title
            GUI.Box(new Rect(X, Y, Width, 25f), WindowTitle);
            //nextControlY += controlSpace; // add more space between title and first control

            if (ActiveSkin != null)
                GUI.skin = ActiveSkin;

            // draw controls
            foreach (var c in _controls)
                c.Draw(NextControlRect(c.SameLineRatio));
            
            // reset GUI enabled
            GUI.enabled = true;
        }

        /// <summary>
        /// Resets to starting position (useful when using dynamic position)
        /// </summary>
        public void ResetPosition()
        {
            X = _startX;
            Y = _startY;
        }
        
        #region Config
        
        /// <summary>
        /// Save a configuration of UWindow that can be later loaded
        /// </summary>
        /// <param name="absolutePath">Absolute path to .ini file (created if it does not exists)</param>
        /// <returns>true if successful</returns>
        public bool SaveCfg(string absolutePath)
        {
            try
            {
                INIParser ini = new INIParser();
                ini.Open(absolutePath);

                string sectionName = $"UWindow.{WindowTitle}";
                // write all values for UWindow
                ini.WriteValue(sectionName, "windowTitle", WindowTitle);
                ini.WriteValue(sectionName, "x", X);
                ini.WriteValue(sectionName, "y", Y);
                ini.WriteValue(sectionName, "width", Width);
                ini.WriteValue(sectionName, "height", Height);
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
        /// Loads a configuration of UWindow and applies it 
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

                string sectionName = $"UWindow.{WindowTitle}";
                // read all values for UWindow
                WindowTitle = ini.ReadValue(sectionName, "windowTitle", WindowTitle);
                X = ini.ReadValue(sectionName, "x", X);
                Y = ini.ReadValue(sectionName, "y", Y);
                Width = ini.ReadValue(sectionName, "width", Width);
                Height = ini.ReadValue(sectionName, "height", Height);
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

        #endregion
        
        #region Skinning
        /// <summary>
        /// Loads skin
        /// </summary>
        /// <param name="mainSkin">main skin for whole UWindow</param>
        /// <returns>true if successful</returns>
        public bool LoadSkin(GUISkin mainSkin)
        {
            if (mainSkin == null) return false;
            this.ActiveSkin = mainSkin;

            return true;
        }

        /// <summary>
        /// Loads skin using AssetBundles (<a href="https://github.com/Hirashi3630/UrGUI/tree/main/Skins#creating-own-skin">how to create your own</a>)
        /// </summary>
        /// <param name="absolutePathAssetBundle">absolute path to AssetBundle</param>
        /// <param name="mainSkinName">bundled name of GUISkin file</param>
        /// <returns>true if successful</returns>
        public bool LoadSkinFromAssetBundle(string absolutePathAssetBundle, string mainSkinName)
        {
            var asset = AssetBundle.LoadFromFile(absolutePathAssetBundle);

            return LoadSkinFromAssetBundle(asset, mainSkinName);
        }

        /// <summary>
        /// Loads skin using AssetBundles (<a href="https://github.com/Hirashi3630/UrGUI/tree/main/Skins#creating-own-skin">how to create your own</a>)
        /// </summary>
        /// <param name="assetFromStream">stream of AssetBundle</param>
        /// <param name="mainSkinName">bundled name of GUISkin file</param>
        /// <returns>true if successful</returns>
        public bool LoadSkinFromAssetBundle(System.IO.Stream assetFromStream, string mainSkinName)
        {
            var asset = AssetBundle.LoadFromStream(assetFromStream);

            return LoadSkinFromAssetBundle(asset, mainSkinName);
        }
        
        /// <summary>
        /// Loads skin using AssetBundles (<a href="https://github.com/Hirashi3630/UrGUI/tree/main/Skins#creating-own-skin">how to create your own</a>)
        /// </summary>
        /// <param name="assetFromMemory">bytes of AssetBundle</param>
        /// <param name="mainSkinName">bundled name of GUISkin file</param>
        /// <returns>true if successful</returns>
        public bool LoadSkinFromAssetBundle(byte[] assetFromMemory, string mainSkinName)
        {
            var asset = AssetBundle.LoadFromMemory(assetFromMemory);

            return LoadSkinFromAssetBundle(asset, mainSkinName);
        }
        
        /// <summary>
        /// Loads skin using AssetBundles (<a href="https://github.com/Hirashi3630/UrGUI/tree/main/Skins#creating-own-skin">how to create your own</a>)
        /// </summary>
        /// <param name="assetBundle"></param>
        /// <param name="mainSkinName">bundled name of GUISkin file</param>
        /// <returns>true if successful</returns>
        public bool LoadSkinFromAssetBundle(AssetBundle assetBundle, string mainSkinName)
        {
            if (assetBundle == null) return false;

            LoadSkin(assetBundle.LoadAsset<GUISkin>(mainSkinName));
            
            return true;
        }

        /// <summary>
        /// Restores default skin
        /// </summary>
        /// <returns>True if successful</returns>
        public bool RestoreDefaultSkin()
        {
            if (DefaultSkin == null) return false;
            ActiveSkin = DefaultSkin;
            return true;
        }
        
        /// <summary>
        /// Restores default skin to all existing windows
        /// </summary>
        /// <returns>True if successful</returns>
        public static bool RestoreGlobalDefaultSkin()
        {
            return LoadGlobalSkin_internal(DefaultSkin);
        }

        /// <summary>
        /// Loads skin to all existing windows
        /// </summary>
        /// <param name="skin"></param>
        /// <returns>True if successful</returns>
        public static bool LoadGlobalSkin(GUISkin skin)
        {
            return LoadGlobalSkin_internal(skin);
        }
            
        #endregion
        
        #region CONTROLS

        private Rect NextControlRect(float sameLineRatio)
        {
            var fullControlWidth = Width - (_margin * 2);
            Rect r = new Rect(X + _margin + _sameLineOffset, _nextControlY, fullControlWidth, _controlHeight);
            
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
            
            // calculate dynamic height
            if (DynamicHeight)
            {
                var titleHeight = 25f + _margin;
                var controlHeight= (_controlHeight + _controlSpace);
                var nLines = 0;
                var sameLineThreshold = 1f;

                // no idea how to simplify this foreach, but it works
                foreach (var i in _controls)
                {
                    // handle SameLine
                    if (i.SameLineRatio.Equals(1) ||
                        sameLineThreshold < 0.05f)
                    {
                        nLines++;
                        sameLineThreshold = 1; // reset threshold
                    }
                    else
                    {
                        sameLineThreshold -= i.SameLineRatio;
                        if (sameLineThreshold < 0.05f)
                            nLines++;
                    }

                }
                
                Height = titleHeight + (controlHeight * nLines);
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
        
        //#################
        // ### CONTROLS ###
        // ################

        /// <summary>
        /// a blank control for organizing
        /// </summary>
        public void Space()
        {
            var c = new UWindowControls.WSpace();
            Add(c);
        }

        /// <summary>
        /// a simple text control
        /// </summary>
        /// <param name="text">label's text</param>
        /// <param name="alignment">alignment inside of control box</param>
        public void Label(string text)
        {
            var c = new UWindowControls.WLabel(text);
            Add(c);
        }

        /// <summary>
        /// a simple button
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onPressed"></param>
        public void Button(string text, System.Action onPressed)
        {
            var c = new UWindowControls.WButton(onPressed, text);
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
            var c = new UWindowControls.WToggle(onValueChanged, value, text);
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
            var c = new UWindowControls.WSlider(onValueChanged, value, min, max, numberIndicator,
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
        public void TextField(string text, System.Action<string> onValueChanged, string value, int maxSymbolLength = int.MaxValue,
            string regexReplace = "")
        {
            var c = new UWindowControls.WTextField(onValueChanged, value, maxSymbolLength, regexReplace, text);
            Add(c);
        }

        /// <summary>
        /// a simple int field control with label
        /// </summary>
        /// <param name="text">label's text</param>
        /// <param name="onValueChanged"></param>
        /// <param name="value">default value</param>
        /// <param name="maxSymbolLength">maximum number of characters</param>
        public void IntField(string text, System.Action<int> onValueChanged, int value, int maxSymbolLength = int.MaxValue)
        {
            var c = new UWindowControls.WIntField(onValueChanged, value, maxSymbolLength, text);
            Add(c);
        }
        
        /// <summary>
        /// a simple float field control with label
        /// </summary>
        /// <param name="text">label's text</param>
        /// <param name="onValueChanged"></param>
        /// <param name="value">default value</param>
        /// <param name="maxSymbolLength">maximum number of characters</param>
        public void FloatField(string text, System.Action<float> onValueChanged, float value, int maxSymbolLength = int.MaxValue)
        {
            var c = new UWindowControls.WFloatField(onValueChanged, value, maxSymbolLength, text);
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
            var c = new UWindowControls.WColorPicker(onValueChanged, value, text, _controlHeight);
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
            var c = new UWindowControls.WDropDown(onValueChanged, value, list, text);
            Add(c);
        }

        /// <summary>
        /// a horizontal line to separate controls
        /// </summary>
        /// <param name="lineColor">Color of separator</param>
        /// <param name="lineThickness">Thinness of separator</param>
        public void Separator(Color lineColor = default(Color), float lineThickness = 2f)
        {
            if (lineColor == default(Color))
                lineColor = new Color(1, 1, 1, .9f);
            var c = new UWindowControls.WSeparator(lineColor,lineThickness);
            Add(c);
        }

        #endregion
    }
}