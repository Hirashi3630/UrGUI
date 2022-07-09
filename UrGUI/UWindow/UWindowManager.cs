using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using static UrGUI.Utils.Logger;
using UnityEngine;

namespace UrGUI.UWindow
{
    /// <summary>
    /// Manages all UWindows 
    /// </summary>
    internal static class UWindowManager
    {
        // ------ manager ------ 
        public static List<UWindow> Windows { get; private set; }

        public static GameObject BManagerG = null;
        public static UWindowManagerBehaviour BManager = null;

        public static bool IsDrawing { get; set; } = true;
        public static GUISkin DefaultSkin { get; internal set; } = null;

        // ------- dialog ------- 
        public static bool AllWindowsDisabled = false;
        public static bool AnyWindowDragging = false;
        public static System.Action ActiveOptionMenu = null;
        
        public static readonly Vector2 DynamicWindowsMargin = Vector2.one * 10f;
        private static Vector2 _dynamicWindowsNext = Vector2.one * 10;
        
        internal static void Register(UWindow win)
        {
            if (Windows == null) Windows = new List<UWindow>();
            
            if (BManager == null || BManagerG == null) InitializeManager();
            
            // generate new UID
            var guid = System.Guid.NewGuid().ToString();
            win.WinGuid = guid;
            Windows.Add(win);
        }

        internal static bool LoadGlobalSkin_internal(GUISkin skin)
        {
            if (skin == null) return false;
            foreach (var w in Windows)
            {
                w.LoadSkin(skin);
            }

            return true;
        }
        
        public static Vector2 GetDynamicWindowPos(float currentWidth)
        {
            // returns current and adds margin for the next one
            // TODO: dynamic Y (no idea how)
            var current = _dynamicWindowsNext;
            _dynamicWindowsNext.x += currentWidth + DynamicWindowsMargin.x;
            
            return current;
        }
        
        internal static void BringUWindowToFront(UWindow win)
        {
            // "front" means last to render
            Windows.Remove(win);
            Windows.Add(win);
        }
        
        private static int GetIndexByGuid(string guid)
        {
            return Windows.FindIndex((win) => win.WinGuid == guid);
        }
        
        private static UWindow GetUWinByGuid(string guid)
        {
            return Windows.FirstOrDefault(w => w.WinGuid == guid);
        }
        
        internal static void OnBehaviourGUI()
        {
            if (!IsDrawing) return;
            
            foreach (var w in Windows.ToList())
            {
                // draw window
                w.Draw();
            }
            
            // draw currently active/opened dialog window
            if (ActiveOptionMenu != null)
                ActiveOptionMenu();
        }

        private static void InitializeManager()
        {
            BManagerG = new GameObject("UrGUI Manager");
            BManager = BManagerG.AddComponent<UWindowManagerBehaviour>();

            UWindowManagerBehaviour.Instance.LoadDefaultSkin();
        }
    }
}