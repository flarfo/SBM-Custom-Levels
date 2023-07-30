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
    <img src="https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/install-step2.png" height="273" width="576">
</p>

**Copy the folder path of your installation (default C:\Program Files (x86)\Steam\steamapps\common\Super Bunny Man)**
<p align="left">
    <img src="https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/install-step3.png" height="307" width="757">
</p>

**Extract the contents of the release directly into the install folder (uncheck the box that says "SBMCustomLevels-1.0.0")**
<p align="left">
    <img src="https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/install-step4.png" height="359" width="763">
</p>

**The install folder should now look like this:**
<p align="left">
    <img src="https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/install-step5.png" height="237" width="662">
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

## Object Information

### Options Menu
| Object | Name | Notes |
:-------------:|:-------------:|:-----:
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Wormhole.png) | Wormhole | Exit to the level, REQUIRED.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Carrot.png) | Carrot | REQUIRED.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/EasterEgg.png) | EasterEgg |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/KillBounds.png) | KillBounds | Kills player on collision.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/PlayerSpawn.png) | PlayerSpawn |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Signpost_Arrow.png) | Signpost_Arrow |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Signpost_Death.png) | Signpost_Death |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Spikes.png) | Spikes |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/SpringBoard.png) | SpringBoard |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/WorldBG_1.png) | WorldBG_1 | Changes background.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/World_2_BG.png) | World_2_BG | Changes background.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/World3_BG.png) | World3_BG | Changes background.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/WaterFoam_B.png) | World4_BG | Changes background.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/World5_BG.png) | World5_BG | Changes background.


### World 1
| Object | Name | Notes |
:-------------:|:-------------:|:-----:
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Block_W1.png) | Block_W1 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Block_W1_1x1Beveled.png) | Block_W1_1x1Beveled |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Block_W1_Corner.png) | Block_W1_Corner |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/DetailRock.png) | DetailRock |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Fence.png) | Fence |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Flower_Blue.png) | Flower_Blue |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Flower_Orange.png) | Flower_Orange |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Flower_Purple.png) | Flower_Purple |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Flower_Red.png) | Flower_Red |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Grass.png) | Grass |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Grass_Angleable.png) | Grass_Angleable |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Grass_Corner_1x1_45deg_Left.png) | Grass_Corner_1x1_45deg_Left |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Grass_Corner_1x1_45deg_Left_Endcap.png) | Grass_Corner_1x1_45deg_Left_Endcap |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Grass_Corner_1x1_45deg_Right.png) | Grass_Corner_1x1_45deg_Right |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Grass_Corner_1x1_45deg_Right_Endcap.png) | Grass_Corner_1x1_45deg_Right_Endcap |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Grass_Endcap_Left.png) | Grass_Endcap_Left |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Grass_Endcap_Right.png) | Grass_Endcap_Right |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Grass_MidBlock.png) | Grass_MidBlock |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Grass_Single_1x1.png) | Grass_Single_1x1 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/MushroomShort.png) | MushroomShort |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/MushroomTall.png) | MushroomTall |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/TreeLog.png) | TreeLog |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Tree_A.png) | Tree_A |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Tree_B.png) | Tree_B |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Tree_Static.png) | Tree_Static |


### World 2
| Object | Name | Notes |
:-------------:|:-------------:|:-----:
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Block_W2.png) | Block_W2 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Block_W2_Corner.png) | Block_W2_Corner |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Daffodil.png) | Daffodil |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/DetailRock.png) | DetailRock |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Grass_Winter.png) | Grass_Winter |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceBlockBottomPanel.png) | IceBlockBottomPanel |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceBlockCorner_LD.png) | IceBlockCorner_LD |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceBlockCorner_LU.png) | IceBlockCorner_LU |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceBlockCorner_RD.png) | IceBlockCorner_RD |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceBlockCorner_RU.png) | IceBlockCorner_RU |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceBlockEdgeLeft.png) | IceBlockEdgeLeft |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceBlockEdgeRight.png) | IceBlockEdgeRight |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceBlockMidPanel.png) | IceBlockMidPanel |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceBlockMidPanel_Left.png) | IceBlockMidPanel_Left |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceBlockMidPanel_Right.png) | IceBlockMidPanel_Right |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceBlockTopPanel.png) | IceBlockTopPanel |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceCorner.png) | IceCorner |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceCorner_Midpanel.png) | IceCorner_Midpanel |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceCube.png) | IceCube |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceQuarterCircle.png) | IceQuarterCircle |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceQuarterPipe.png) | IceQuarterPipe |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceSled_1x3.png) | IceSled_1x3 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceSled_1x4.png) | IceSled_1x4 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceSled_1x5.png) | IceSled_1x5 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/IceSledSpikesGuide.png) | IceSledSpikesGuide | Goes around spikes to allow ice sleds to slide smoothly across.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Signpost_Arrow_Snow.png) | Signpost_Arrow_Snow |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Signpost_Death_Snow.png) | Signpost_Death_Snow |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Snowball.png) | Snowball |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Snowman.png) | Snowman |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Snow_Angleable.png) | Snow_Angleable |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Snow_Corner_1x1_45deg_Left.png) | Snow_Corner_1x1_45deg_Left |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Snow_Corner_1x1_45deg_Left_Endcap.png) | Snow_Corner_1x1_45deg_Left_Endcap |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Snow_Corner_1x1_45deg_Right.png) | Snow_Corner_1x1_45deg_Right |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Snow_Corner_1x1_45deg_Right_Endcap.png) | Snow_Corner_1x1_45deg_Right_Endcap |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Snow_Endcap_Left.png) | Snow_Endcap_Left |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Snow_Endcap_Right.png) | Snow_Endcap_Right |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Snow_MidBlock.png) | Snow_MidBlock |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Snow_Single_1x1.png) | Snow_Single_1x1 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/WinterPlant.png) | WinterPlant |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/WinterTree.png) | WinterTree |

### World 3
| Object | Name | Notes |
:-------------:|:-------------:|:-----:
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Block_W3.png) | Block_W3 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Block_W3_Corner.png) | Block_W3_Corner |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Boulder.png) | Boulder |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BoulderDestroyer.png) | BoulderDestroyer | Destroys boulders on collision.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BoulderRollable.png.png) | BoulderRollable |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BoulderSpawner.png) | BoulderSpawner | Periodically spawns boulders.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Boulder_Kickable.png) | Boulder_Kickable |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/CaveRock.png) | CaveRock |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/CaveRockCrystals.png) | CaveRockCrystals |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/CaveRock_small.png) | CaveRock_small |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Cave_Endcap_1x1.png) | Cave_Endcap_1x1 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Cave_Endcap_Left.png) | Cave_Endcap_Left |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Cave_Endcap_Right.png) | Cave_Endcap_Right |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Cave_Midblock.png) | Cave_Midblock |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Crystal_Blue.png) | Crystal_Blue |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Crystal_Green.png) | Crystal_Green |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Crystal_Orange.png) | Crystal_Orange |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Crystal_Red.png) | Crystal_Red |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Minecart.png) | Minecart | Must be placed near minecart rail to connect.
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/MinecartRail.png) | MinecartRail |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Minecart_Static.png) | Minecart_Static |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/MineLamp.png) | MineLamp |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Mineshaft_Support.png) | Mineshaft_Support |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Mineshaft_Support_NoLamp.png) | Mineshaft_Support_NoLamp |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Mineshaft_Support_Unlit.png) | Mineshaft_Support_Unlit |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Pickaxe.png) | Pickaxe |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Stalagmite.png) | Stalagmite |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/World3_Block_1x1_Bevel.png) | World3_Block_1x1_Bevel |


### World 4
| Object | Name | Notes |
:-------------:|:-------------:|:-----:
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BarrelRaftIsland_LShape_A.png) | BarrelRaftIsland_LShape_A |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BarrelRaftIsland_LShape_B.png) | BarrelRaftIsland_LShape_B |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BarrelRaft_2x1.png) | BarrelRaft_2x1 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BarrelRaft_2x2.png) | BarrelRaft_2x2 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BarrelRaft_2x4.png) | BarrelRaft_2x4 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BarrelRaft_Round.png) | BarrelRaft_Round |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Beachball.png) | Beachball |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BeachBarrel.png) | BeachBarrel |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BeachPlant.png) | BeachPlant |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BeachQuarterCircle.png) | BeachQuarterCircle |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BeachQuarterCircle_Cave.png) | BeachQuarterCircle_Cave |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BeachQuarterPipe.png) | BeachQuarterPipe |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BeachQuarterPipe_Cave.png) | BeachQuarterPipe_Cave |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BeachRock.png) | BeachRock |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BeachRock_RB.png) | BeachRock_RB |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Block_W4.png) | Block_W4 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Block_W4_1x1_CurvedBottom.png) | Block_W4_1x1_CurvedBottom |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Block_W4_Corner.png) | Block_W4_Corner |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Boardwalk.png) | Boardwalk |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Coconut.png) | Coconut |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Coral.png) | Coral |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/DetailRock.png) | DetailRock |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/FloatingPlatform.png) | FloatingPlatform |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/FloatingPlatform_Small.png) | FloatingPlatform_Small |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/GrassBeach_Corner_1x1_45deg_Right_Endcap.png) | GrassBeach_Corner_1x1_45deg_Right_Endcap |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/GrassBeach_Endcap_1x1.png) | GrassBeach_Endcap_1x1 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/GrassBeach_Endcap_Left.png) | GrassBeach_Endcap_Left |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/GrassBeach_Endcap_Right.png) | GrassBeach_Endcap_Right |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/GrassBeach_MidBlock.png) | GrassBeach_MidBlock |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/HibiscusBush.png) | HibiscusBush |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/JetSki.png) | JetSki |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/PalmTree.png) | PalmTree |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/SeaLeaf.png) | SeaLeaf |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/SeaMine.png) | SeaMine |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Seashell.png) | Seashell |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Seaweed.png) | Seaweed |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/TreasureChest.png) | TreasureChest |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/WaterFoam_B.png) | Water | Can be configured to rise/fall.


### World 5
| Object | Name | Notes |
:-------------:|:-------------:|:-----:
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Billboard.png) | Billboard |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BumperBar.png) | BumperBar |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BumperBarSpinWheel.png) | BumperBarSpinWheel |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BumperPad.png) | BumperPad |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BumperPadJumbo.png) | BumperPadJumbo |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BumperSpinner.png) | BumperSpinner |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BumperSpinnerJumbo.png) | BumperSpinnerJumbo |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BumperSpinnerPlatform.png) | BumperSpinnerPlatform |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/BurningPlatform.png) | BurningPlatform |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/DirtBike.png) | DirtBike |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/HopBars.png) | HopBars |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/HotdogKart.png) | HotdogKart |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/LevelBlock_W5.png) | LevelBlock_W5 |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Masher.png) | Masher |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/RingOfFire.png) | RingOfFire |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/SparkShower.png) | SparkShower |
![](https://github.com/flarfo/SBM-Custom-Levels/blob/master/icon/Spotlight.png) | Spotlight |


## License
The SBM Custom Levels project is licensed under the GPL-3.0 license.
