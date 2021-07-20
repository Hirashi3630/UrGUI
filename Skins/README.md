
# Skinning
> This skinning section will help you change the look of your default Unity look!
----

## Load existing skin<br>


1. Get AssetBundle file by [downloading](#featured-skins)/[creating](#creating-own-skin) one
2. Load skins (If you download skins from [here](#featured-skins), names for each skin can be found in `.txt` file attached to it)<br><br>
**GUIWindow:**
```cs
guiWindow.LoadSkinFromAssetBundle(absolutePathToAssetBundle, "nameOfMainSkin", "nameOfTitleSkin");
```

## Creating own skin
1. [Download Unity](https://unity3d.com/get-unity/download) and create new project
2. Create new GUISkin and edit

<details><summary>How to create a GUISkin?</summary><blockquote>
<img src="../Assets/Skins/Media/how-to-create-guiskin.jpg?raw=true" alt="how-to-create-guiskin">
</blockquote></details>
<details><summary>What to edit?</summary><blockquote>
Download one of the existing skins and look what has been changed and play around with it<br>
If you want help with this, create a new issue, and I'll try to help!
</blockquote></details>

3. Add it to AssetBundle

<details><summary>How?</summary><blockquote>
<img src="../Assets/Skins/Media/how-to-add-asset-to-assetbundle.jpg?raw=true" alt="how-to-add-asset-to-assetbundle">
</blockquote></details>

4. Create new C# script in `Assets/Editor` folder named `CreateAssetBundles.cs`
5. Paste this code to the script, save and let Unity compile

```cs
using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if(!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, 
                                        BuildAssetBundleOptions.None, 
                                        BuildTarget.StandaloneWindows);
    }
}
```

6. Build AssetBundle (AssetBundle will be created in `Assets/AssetBundles`)

<details><summary>How?</summary><blockquote>
<img src="../Assets/Skins/Media/how-to-build-assetbundles.jpg?raw=true" alt="how-to-build-assetbundles">
</blockquote></details>
    
7. [Load it!](#load-existing-skin)

## Featured skins

### [ocornut](ocornut/README.md)<br>
<img src="../Assets/Skins/Media/ocornut-guiwindowexample1.png?raw=true" alt="ocornut-guiwindowexample1">