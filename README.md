
<h1 align="center">UrGUI</h1>

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) ![GitHub issues](https://img.shields.io/github/issues/Hirashi3630/UrGUI) [![CodeFactor](https://www.codefactor.io/repository/github/hirashi3630/urgui/badge/main)](https://www.codefactor.io/repository/github/hirashi3630/urgui/overview/main) 

> UrGUI: easy to use extension for Unity IMGUI system
> <br><br>
> The main focus of this project is to create an easy-to-use extension library for Unity IMGUI, primarily used for modding and debugging purposes.

----

<br>

## Usage

### Quickstart

a simple menu with label and button

```cs
private GUIWindow window1;

private void Start()
{
    window1 = GUIWindow.Begin("window1's title");
    window1.Label("Lorem ipsum");
    window1.Button("Press me!", () => print("Button has been pressed!"));
}

private void OnGUI()
{
    window1.Draw();
}
```

<img src="Assets/Media/usage-sample1.png?raw=true" alt="usage-sample1">

<details><summary>I want to set my own position and size!</summary><blockquote>

  (x, y, width, height)

  ```cs
  window1 = GUIWindow.Begin("window1's title", 10, 10, 200, 400);
  ```
  
</blockquote></details>

<br>

## Controls

<blockquote>

  #### Space
  
  *same as empty label*

  ```cs
  window1.Space("Lorem ipsum");
  ```

<br><br>

  #### Label

<img src="Assets/Media/label_showcase1.png?raw=true" alt="label_showcase">
  

  ```cs
  window1.Label("Lorem ipsum");
  ```

<br><br>

  #### Button

  <img src="Assets/Media/button_showcase1.png?raw=true" alt="button_showcase">
  

  ```cs
  window1.Button("Press me!", () => print("Button has been pressed!"));
  ```

<br><br>
  
  #### Slider

  <img src="Assets/Media/slider_showcase1.gif?raw=true" alt="slider_showcase">
  

  ```cs
  window1.Slider("Slider:", (value) => print($"Toggle value is now {value}"), 0.5f, 0f, 1f, true);
  ```

<br><br>
  
  #### TextField

  <img src="Assets/Media/textfield_showcase1.gif?raw=true" alt="textfield_showcase">
  

  ```cs
  window1.TextField("Name:", (value) => Debug.Log($"TextField has been changed to '{value}'"), "Sample Text", 64);
  ```

<br><br>
  
  #### FloatField

  <img src="Assets/Media/floatfield_showcase1.gif?raw=true" alt="floatfield_showcase">
  

  ```cs
  window1.FloatField("X:", (value) => Debug.Log($"FloatField has been changed to '{value}'"), 12.34f, 20);
  ```

<br><br>
  
  #### Toggle

  <img src="Assets/Media/toggle_showcase1.gif?raw=true" alt="toggle_showcase">
  

  ```cs
  window1.Toggle("Toggle me!", (value) => print($"Toggle value is now {value}"));
  ```

<br><br>
  
  #### ColorPicker

  <img src="Assets/Media/colorpicker_showcase1.gif?raw=true" alt="colorpicker_showcase">
  

  ```cs
  window1.ColorPicker("Cube clr:", (clr) => Debug.Log($"Color has been changed to {clr}"), Color.red);
  ```

<br><br>
  
  #### DropDown

  <img src="Assets/Media/dropdown_showcase1.gif?raw=true" alt="dropdown_showcase">
  

  ```cs
  var selection = new Dictionary<int, string>();
  for (int i = 0; i < 10; i++)
      selection.Add(i, $"Choice n.{i}");

  window1.DropDown("Selection:", (id) => print($"'{id}'. has been selected!"), 0,  selection); 
  ```

</blockquote>

