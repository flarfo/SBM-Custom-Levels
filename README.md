<p align="center">
    <img src="https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/modicon.png" height="320" width="320">
</p>

# SBM Custom Levels
[![Discord](https://img.shields.io/discord/1012553301843259402?label=Discord&logo=Discord&logoColor=%237289DA&style=plastic)](https://discord.gg/QkmyuTPhbC)
[![YouTube](https://img.shields.io/youtube/channel/subscribers/UCCdz54phrEf-6Bc0KCTmfgw?label=flarfo&logo=Youtube&style=plastic)](https://www.youtube.com/@flarfo/)

## About
SBM Custom Levels is a mod for Super Bunny Man that allows users to create and play new and exciting levels!

[Installation](#manual-installation)

[Editor Information](#editor-information)

[Object Information](#object-information)

## Manual Installation
Video Install Guide: https://youtu.be/qNPceLrAVE8

**Download the [latest release](https://github.com/flarfo/SBM-Custom-Levels/releases)**

**Navigate to your Super Bunny Man installation**
<p align="left">
    <img src="https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/install-step2.png" height="392" width="532">
</p>

**Copy the folder path of your installation (default C:\Program Files (x86)\Steam\steamapps\common\Super Bunny Man)**
<p align="left">
    <img src="https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/install-step3.png" height="295" width="699">
</p>

**Extract the contents of the release directly into the install folder (I recommend using 7-Zip)**
<p align="left">
    <img src="https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/install-step4.png" height="289" width="515">
</p>

**The install folder should now look like this:**
<p align="left">
    <img src="https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/install-step5.png" height="204" width="619">
</p>

**Launch the game and have fun!**

## Editor Information

### Keybinds
| Keybind | Effect |
:--------:|:--------:
Q | Toggels stamp tool
W | Toggles move tool
E | Toogles select tool
Left Mouse | Select/move
Middle Mouse | Pan camera
Scroll | Zoom in/out
Ctrl+Z | Undo
Ctrl+Y | Redo
Ctrl+C | Copy
Ctrl+V | Paste
Ctrl+X | Cut
Ctrl+S | Save
Delete | Deletes select objects
Ctrl+Left Click | Adds/removes object from selection
` | Opens options menu
1 | Opens first world objects
2 | Opens second world objects
3 | Opens third world objects
4 | Opens fourth world objects
5 | Opens fifth world objects
6 | Opens party level objects

### Minecart Rails
- Managed by a series of **rail nodes**. These can be selected individually to adjust the spline of the rail. 
- Minecarts must be placed near the rail for them to attach. 
- By changing the **Y component** of the Up vector to -1 in the inspector, rails can be placed upside down. Note that ALL nodes must have the same Up vector.
- X-Button deletes the currently selected node.
- Add-Button adds a new rail node after the selected one.

### Water
- Each water object can be selected to reveal the **Water Keyframes Inspector**. This allows you to configure how the water moves.
- Set Time/Value pairs to set the height of the water at a specific time. This will be animated automatically in game.
- X-Button deletes the last keyframe in the inspector.
- Add-Button adds a new keyframe to the bottom, **maximum of 16 keyframes**.

### Pistons
- Each piston object can be selected to reveal the **Piston Keyframes Inspector**. This allows you to configure how the piston moves.
- Set Time/Value pairs to set the extension of the piston at a specific time. This will be animated automatically in game.
- Normalized so that value of 1 = max extension.
- X-Button deletes the last keyframe in the inspector.
- Add-Button adds a new keyframe to the bottom, **maximum of 16 keyframes**.

## Special Object Information

### Options Menu
| Object | Name | Notes |
:-------------:|:-------------:|:-----:
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/WorldBG_1.png) | WorldBG_1 | Changes background.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/World_2_BG.png) | World_2_BG | Changes background.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/World3_BG.png) | World3_BG | Changes background.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/WaterFoam_B.png) | World4_BG | Changes background.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/World5_BG.png) | World5_BG | Changes background.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Wormhole.png) | Wormhole | Exit to the level, REQUIRED.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Carrot.png) | Carrot | REQUIRED.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/KillBounds.png) | KillBounds | Kills player on collision.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/PlayerSpawn.png) | PlayerSpawn | REQUIRED.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/ColorBlock.png) | Color Block | Re-colorable with RGB values.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/ColorBlockCorner%20-%20Copy.png) | Color Block Corner | Re-colorable with RGB values.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceSledSpikesGuide.png) | IceSledSpikesGuide | Goes around spikes to allow ice sleds to slide smoothly across.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BoulderDestroyer.png) | BoulderDestroyer | Destroys boulders on collision.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BoulderSpawner.png) | Boulder Spawner | Periodically spawns boulders.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Minecart.png) | Minecart | Must be placed near minecart rail to connect.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/MinecartRail.png) | Minecart Rail | Bendable spline object with a series of adjustable nodes.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/WaterFoam_B.png) | Water | Can be configured to rise/fall.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/FlipBlock.png) | Flip Block | Configurable block that rotates x degrees every x seconds.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/PistonPlatform.png) | Piston Platform | Configurable platform that moves up and down based on keyframes.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/SeeSaw.png) | See Saw | Free rotating platform, rotation pivot point can be changed.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/SlipNSlide.png) | Slip N Slide | Slippery plaatform with adjustable size.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/SplinePlatform.png) | Spline Platform | Bendable spline platform with a series of adjustable nodes.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/StiffRod.png) | Stiff Rod | Size adjustable rod.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BasketBall.png) | Basketball | Required for Basketball levels.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BasketballHoop.png) | Basketball Hoop | Required for Basketball levels.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/CarrotGrabBasket.png) | Carrot Basket | Required for Carrot Grab levels.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/PickupSpawnPoint.png) | Pickup Spawner | Spawns a party level pickup when collected.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/PickupTrigger_Jetpack.png) | Jetpack Pickup | Spawns a jetpack when collected.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/PickupTrigger_MagnetGloves.png) | Magnet Gloves Pickup | Spawns magnet gloves when collected.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/PickupTrigger_Rollerskates.png) | Rollersaktes Pickup | Spawns rollerskates when collected.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/PickupTrigger_UnicornHorn.png) | Unicorn Horn Pickup | Spawns unicorn horn when collected.

## License
The SBM Custom Levels project is licensed under the GPL-3.0 license.
