using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UrGUI.Core;

public class UrGUI_Test : MonoBehaviour
{
    private GUIWindow window1 = null;
    private GUIWindow window2 = null;

    private void Start()
    {
        if (window1 == null)
            window1 = GUIWindow.Begin("Very good title!",20, 50, 300, 500, 5, 21, 5);

        if (window2 == null)
            window2 = GUIWindow.Begin("Very good title! 2", 330, 50, 300, 500, 5, 21, 5);
        window2.Add(GUIWindow.Label("Label without alignment"));
        window2.Add(GUIWindow.Label("Label aligned Center", UrGUI.Core.Utils.GUIFormatting.AlignType.Center));
        window2.Add(GUIWindow.Label("Label aligned Right-Bottom!!", UrGUI.Core.Utils.GUIFormatting.AlignType.RightBottom));
        window2.Add(GUIWindow.Space());
        window2.Add(GUIWindow.Button("Button 1", Button1));
        window2.Add(GUIWindow.Toggle("Toggle 1", Toggle1));
        window2.Add(GUIWindow.Slider("Slider 1", Slider1, 50, 0, 100, true));
        window2.Add(GUIWindow.ColorPicker("ColorPicker 1", ColorPicker1, Color.red));
    }

    private void Button1()
    {
        print("Button1 has been pressed!");
    }

    private void Toggle1(bool value)
    {
        print("Toggle1's value has been changed to " + value);
    }

    private void Slider1(float value)
    {
        print("Slider1's value has been changed to " + value);
    }

    private void ColorPicker1(Color value)
    {
        print("ColorPicker1's value has been changed to " + value);
    }

    private void OnGUI()
    {
        window1.Draw();
        window2.Draw();
    }
}
