
<h1 align="center">UrGUI</h1>

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) ![GitHub issues](https://img.shields.io/github/issues/Hirashi3630/UrGUI) [![Workflow Badge](https://github.com/Hirashi3630/UrGUI/actions/workflows/ReleaseBuild.yml/badge.svg)](https://github.com/Hirashi3630/UrGUI/actions/workflows/ReleaseBuild.yml) [![CodeFactor](https://www.codefactor.io/repository/github/hirashi3630/urgui/badge/main)](https://www.codefactor.io/repository/github/hirashi3630/urgui/overview/main)

> UrGUI: easy to use library to create simple UI/GUI in Unity using bult-in IMGUI system
> <br><br>
> The main focus of this project is to create an easy-to-use library for Unity, primarily used for modding and debugging purposes. By using Unity's built-in IMGUI system you can be sure this library will work with almost any version of Unity

----

<p align="center">
  <a href="UrGUI.Examples/CompleteExample.cs" >
    <img src="Assets/Media/complete_showcase.gif?raw=true" alt="usage-sample1">
  </a>
<br>
  <a href="UrGUI.Examples/">Examples</a>
</p>

<br>

## Getting Started

1. Get built assembly from [Release page](https://github.com/Hirashi3630/UrGUI/releases) _(binary doesn't contain Unity dlls, use game's or [editor's](https://docs.unity3d.com/Manual/UsingDLL.html) or [here](https://github.com/Hirashi3630/UrGUI/tree/main/Assemblies/))_
2. Download source code and build it yourself

### MelonLoader

3. put `UrGUI.dll` in `/UserLibs/`

<br>


## Usage

### Quickstart

a simple menu with label and button

<img src="Assets/Media/quickstart_showcase.gif?raw=true" alt="usage-sample1">

```cs
using UrGUI.GUIWindow;

private GUIWindow window;

private void Start()
{
    window = GUIWindow.Begin("Quickstart");
    window.Label("Lorem ipsum");
    window.Button("Press me!", () => print("Button has been pressed!"));
}

private void OnGUI()
{
    window.Draw();
}
```

<details><summary>I want to set my own position and size!</summary><blockquote>

(x, y, width, height)

  ```cs
  window = GUIWindow.Begin("Custom size!", startX: 50, 50, 200, 600);
  ```

</blockquote></details>

<br>

## Controls

<blockquote>

<br>

#### Label

<img src="Assets/Media/label_showcase.gif?raw=true" alt="label_showcase">


  ```cs
  window.Label("Lorem ipsum dolor sit amet");
  ```

<br><br>

#### Button

  <img src="Assets/Media/button_showcase.gif?raw=true" alt="button_showcase">


  ```cs
 window.Button("Press me!", () => print("Button has been pressed!"));
  ```

<br><br>

#### Slider

  <img src="Assets/Media/slider_showcase.gif?raw=true" alt="slider_showcase">


  ```cs
  window.Slider("Slider:", (value) => print($"Slider value is now {value}"), 0.69f, 0f, 1f, true);
  ```

<br><br>

#### Toggle

  <img src="Assets/Media/toggle_showcase.gif?raw=true" alt="toggle_showcase">


  ```cs
  window.Toggle("Is UrGUI best?", (value) => print($"Toggle value is now {value}"), false);
  ```

<br><br>

#### ColorPicker

  <img src="Assets/Media/colorpicker_showcase.gif?raw=true" alt="colorpicker_showcase">


  ```cs
  window.ColorPicker("Cube clr:", (clr) => Debug.Log($"Color has been changed to {clr}"), Color.red);
  ```

<br><br>

#### DropDown

  <img src="Assets/Media/dropdown_showcase.gif?raw=true" alt="dropdown_showcase">


  ```cs
  var selection = new Dictionary<int, string>();
  for (int i = 0; i < 10; i++)
      selection.Add(i, $"Option n.{i}");

  window.DropDown("Selection:", (id) => print($"'{id}'. has been selected!"), 0,  selection); 
  ```

<br><br>

#### TextField

  <img src="Assets/Media/textfield_showcase.gif?raw=true" alt="textfield_showcase">


  ```cs
  window.TextField("Name:", (value) => Debug.Log($"TextField has been changed to '{value}'"), "Sample Text");
  ```

<br><br>

#### IntField

  <img src="Assets/Media/intfield_showcase.gif?raw=true" alt="intfield_showcase">


  ```cs
  window.IntField("Age:", (value) => Debug.Log($"FloatField has been changed to '{value}'"), 1234);
  ```

<br><br>

#### FloatField

  <img src="Assets/Media/floatfield_showcase.gif?raw=true" alt="floatfield_showcase">


  ```cs
  window.FloatField("X:", (value) => Debug.Log($"FloatField has been changed to '{value}'"), 12.34f);
  ```

<br><br>

#### Space

*same as empty label*

  ```cs
  window.Space();
  ```

</blockquote>

<br>

## Licensing & Credits:

UrGUI is licensed under the MIT License. See [LICENSE](https://github.com/Hirashi3630/UrGUI/blob/main/LICENSE) for the full License.

Third-party Libraries used as Source Code and/or bundled in Binary Form:
- Unity Runtime Libraries are part of Unity Software.
  Their usage is subject to Unity Terms of Service, including Unity Software Additional Terms.

This Repository is not sponsored by, affiliated with or endorsed by Unity Technologies or its affiliates.
"Unity" is a trademark or a registered trademark of Unity Technologies or its affiliates in the U.S. and elsewhere.

Used softwares
 - [ScreenToGif](https://github.com/NickeManarin/ScreenToGif) to create these amazing gifs