.## Getting started

1. Install **Unity 6000.6.0f1** (with iOS Build Support if you want iOS builds).
2. Clone the repo with Git LFS enabled (`git lfs install` before cloning).
3. Open the project folder in Unity Hub.
4. Open `Assets/_Project/Scenes/Menu.unity` and press Play.

The game is designed for a 1080x1920 portrait view, so set the Game view to that aspect ratio.

### Debug panel

Triple-tap the top-left corner (or press **F1** on Windows) in the Menu or Battle scene. From the panel
you can advance the day, force end of day, kill the monster, reset the save and toggle Take Damage.

### Building

Build profiles for **Windows** (portrait window) and **iOS** (Xcode project export) are in
`Assets/Settings/Build Profiles`. Use *File > Build Profiles*.

## Project layout

```
Assets/_Project/
  Scripts/Runtime/   game code (Spaa.Runtime assembly)
  Scripts/Editor/    editor utilities (Tools > Spaa menu)
  Tests/EditMode/    NUnit tests for the game rules
  Data/              ScriptableObject config: event channels, elements, habits, monsters
  UI/                UI Toolkit UXML, USS and panel settings
  Art/               models, textures, materials and shaders
  Scenes/            Menu, Battle, BackgroundBlockout
  Sounds/            music and SFX
```

See [CREDITS.md](CREDITS.md).
