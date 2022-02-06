
<h1 align="center">UrGUI</h1>
<p>
  <a href="https://www.mit-license.org" target="_blank">
    <img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-yellow.svg" />
  </a>
</p>

> UrGUI: easy to use extension for Unity IMGUI system
> <br><br>
> The main focus of this project is to create an easy-to-use extension library for Unity IMGUI, primarily used for modding purposes.

----
## Usage

### Quickstart

a simple menu with button

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


### Controls

<blockquote>

<br>


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

