using UnityEngine;

/*
*
* **** GUIButton CLASS ****
*
* this versions sends only events to the topmost button ...
*
*
* Fixes the bugs from the original GUI.Button function
* Based on the script from Joe Strout:
* http://forum.unity3d.com/threads/96563-corrected-GUI.Button-code-%28works-properly-with-layered-controls%29?p=629284#post629284
*
*
* The difference in this script is that it will only fire events (click and rollover!)
* for the topmost button when using overlapping buttons inside the same GUI.depth!
* Therefore the script finds the topmost button during the layout process, so it
* can decide which button REALLY has been clicked.
*
* Benefits:
* 1. The script will only hover the topmost button!
*    (doesn't matter whether the topmost button is defined via GUI.depth or via drawing order!)
* 2. The script will only send events to the topmost button (as opposed to Joe's original script)
* 3. The script works for overlapping buttons inside same GUI.depth levels,
*    as well as for overlapping buttons using different GUI.depth values
* 4. The script also works when overlapping buttons over buttons inside scroll-views, etc.
*
* Usage:  just like GUI.Button() ... for example:
*
*     if (UGUI.Button(new Rect(0,0,100,100), "button_action", GUI.skin.customStyles[0]))
*    {
*         Debug.Log( "Button clicked ..." );
*    }
*
* 
*
* Original script (c) by Joe Strout!
*
* Code changes:
* Copyright (c) 2012 by Frank Baumgartner, Baumgartner New Media GmbH, fb@b-nm.at
*
* 2019 by Frédéric Defoy for NWS
*
* 2022 by Hirashi3630 for completing rest of the controls
*
* */

namespace UrGUI.Utils
{
    internal static class UGUI
    {
        #region Button
        private static int highestDepthID = 0;
        private static Vector2 touchBeganPosition = Vector2.zero;
        private static EventType lastEventType = EventType.Layout;

        private static bool wasDragging = false;

        private static int frame = 0;
        private static int lastEventFrame = 0;

        public static bool Button(string text, params GUILayoutOption[] options)
        {
            GUIContent content = new GUIContent(text);
            return Button(GUILayoutUtility.GetRect(content, GUI.skin.button, options), content, GUI.skin.button);
        }

        public static bool Button(string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            GUIContent content = new GUIContent(text);
            return Button(GUILayoutUtility.GetRect(content, style, options), content, style);
        }

        public static bool Button(Texture image, params GUILayoutOption[] options)
        {
            GUIContent content = new GUIContent(image);
            return Button(GUILayoutUtility.GetRect(content, GUI.skin.button, options), content, GUI.skin.button);
        }

        public static bool Button(Texture image, GUIStyle style = null, params GUILayoutOption[] options)
        {
            GUIContent content = new GUIContent(image);
            return Button(GUILayoutUtility.GetRect(content, style, options), content, style);
        }

        public static bool Button(GUIContent content, params GUILayoutOption[] options)
        {
            return Button(GUILayoutUtility.GetRect(content, GUI.skin.button, options), content, GUI.skin.button);
        }

        public static bool Button(GUIContent content, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return Button(GUILayoutUtility.GetRect(content, style, options), content, style);
        }

        public static bool Button(Rect bounds, string text, GUIStyle style = null)
        {
            GUIContent content = new GUIContent(text);
            return Button(bounds, content, style);
        }

        public static bool Button(Rect bounds, Texture image, GUIStyle style = null)
        {
            GUIContent content = new GUIContent(image);
            return Button(bounds, content, style);
        }

        static bool Button(Rect bounds, GUIContent content, GUIStyle style = null)
        {
            int controlID = GUIUtility.GetControlID(bounds.GetHashCode(), FocusType.Passive);
            bool isMouseOver = bounds.Contains(Event.current.mousePosition);
            int depth = (1000 - GUI.depth) * 1000 + controlID;
            if (isMouseOver && depth > highestDepthID) highestDepthID = depth;
            bool isTopmostMouseOver = (highestDepthID == depth);
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
         bool paintMouseOver = isTopmostMouseOver && (Input.touchCount > 0);
#else
            bool paintMouseOver = isTopmostMouseOver;
#endif

            if (style == null)
            {
                style = GUI.skin.FindStyle("button");
            }

            if (Event.current.type == EventType.Layout && lastEventType != EventType.Layout)
            {
                highestDepthID = 0;
                frame++;
            }

            lastEventType = Event.current.type;

            if (Event.current.type == EventType.Repaint)
            {
                bool isDown = (GUIUtility.hotControl == controlID);
                style.Draw(bounds, content, paintMouseOver, isDown, false, false);
            }

#if (UNITY_IPHONE || UNITY_ANDROID)
         if ( Input.touchCount > 0 )
         {
             Touch touch = Input.GetTouch(0);
             if ( touch.phase == TouchPhase.Began )
             {
                 touchBeganPosition = touch.position;
                 wasDragging = true;
             }
             else if ( touch.phase == TouchPhase.Ended &&
                             (   (Mathf.Abs(touch.position.x - touchBeganPosition.x) > 15) ||
                                 (Mathf.Abs(touch.position.y - touchBeganPosition.y) > 15)       )
                     )
             {
                 wasDragging = true;
             }
             else
             {
                 wasDragging = false;
             }
         }
         else if ( Event.current.type == EventType.Repaint )
         {
             wasDragging = false;
         }

#endif

            // Workaround:
            // ignore duplicate mouseUp events. These can occur when running
            // unity editor with unity remote on iOS ... (anybody knows WHY?)
            if (frame <= (1 + lastEventFrame)) return false;

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                {
                    if (isTopmostMouseOver && !wasDragging)
                    {
                        GUIUtility.hotControl = controlID;
                    }

                    break;
                }

                case EventType.MouseUp:
                {
                    if (isTopmostMouseOver && !wasDragging)
                    {
                        GUIUtility.hotControl = 0;
                        lastEventFrame = frame;
                        return true;
                    }

                    break;
                }
            }

            return false;
        }
        #endregion
    }
}