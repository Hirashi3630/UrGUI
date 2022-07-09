using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UrGUI.Utils.Logger;

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

        // ------- dialog ------- 
        public static bool AllWindowsDisabled = false;
        public static bool AnyWindowDragging = false;
        public static System.Action ActiveOptionMenu = null;
        
        public static readonly Vector2 DynamicWindowsMargin = Vector2.one * 10f;
        private static Vector2 _dynamicWindowsNext = Vector2.one * 10;
        
        public static void Register(UWindow win)
        {
            if (Windows == null) Windows = new List<UWindow>();
            
            if (BManager == null || BManagerG == null) InstantiateNewBManager(); 
            
            // generate new UID
            var guid = System.Guid.NewGuid().ToString();
            win.WinGuid = guid;
            Windows.Add(win);
        }

        public static Vector2 GetDynamicWindowPos(float currentWidth)
        {
            // returns current and adds margin for the next one
            // TODO: dynamic Y (no idea how)
            var current = _dynamicWindowsNext;
            _dynamicWindowsNext.x += currentWidth + DynamicWindowsMargin.x;
            
            return current;
        }
        
        public static void OnBehaviourGUI()
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

        public static void BringUWindowToFront(UWindow win)
        {
            // "front" means last to render
            Windows.Remove(win);
            Windows.Add(win);
        }
        
        // public static void BringUWindowToFront(string guid)
        // {
        //     var win = GetUWinByGuid(guid);
        //     Windows.RemoveAll(p => p.WinGuid == guid);
        //     Windows.Insert(0, win);
        // }

        private static int GetIndexByGuid(string guid)
        {
            return Windows.FindIndex((win) => win.WinGuid == guid);
        }
        
        private static UWindow GetUWinByGuid(string guid)
        {
            return Windows.FirstOrDefault(w => w.WinGuid == guid);
        }
        
        private static void InstantiateNewBManager()
        {
            BManagerG = new GameObject("UrGUI Manager");
            BManager = BManagerG.AddComponent<UWindowManagerBehaviour>();
        }
        
    }
}