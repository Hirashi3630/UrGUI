using System.Collections.Generic;
using UnityEngine;
using UrGUI.ImGUI;

public class CompleteExample : MonoBehaviour
{
    private GUIWindow window;
    
    private void Awake()
    {
        window = GUIWindow.Begin("Complete Showcase");
        window.Label("Label");
        window.Button("Button!", () => Debug.Log("Button has been pressed!"));
        window.Slider("Slider:", (value) => Debug.Log($"Toggle value is now {value}"), 0.69f, 0f, 1f, true);
        window.Toggle("Toggle:", (value) => Debug.Log($"Toggle value is now {value}"));
        window.ColorPicker("ColorPicker:", (clr) => Debug.Log($"Color has been changed to {clr}"), Color.red);
        var selection = new Dictionary<int, string>();
        for (int i = 0; i < 10; i++)
            selection.Add(i, $"Option n.{i}");
        window.DropDown("DropDown:", (id) => Debug.Log($"'{id}'. has been selected!"), 0,  selection); 
        window.TextField("TextField:", (value) => Debug.Log($"TextField has been changed to '{value}'"), "Sample Text");
        window.IntField("IntField:", (value) => Debug.Log($"IntField has been changed to '{value}'"), 123456);
        window.FloatField("FloatField:", (value) => Debug.Log($"FloatField has been changed to '{value}'"), 1.23456f);
    }

    private void OnGUI()
    {
        window.Draw();
    }
}