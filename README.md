# Rehabilitation Game for AMADEO device 

## Project Overview
This rehabilitation game was developed in collaboration with Lowenstein Rehabilitation Medical Center, Ra’anana, Israel.

Check out our [Project Book](https://github.com/amadeo-game/amadeo-unity-game/blob/master/Project%20Book%20Beaver%20Game.pdf)
 for all the details.


#### unity version 
2022.3.14f1


### Input Types

In BridgeGenerator object, BridgeClient component InputType field, you can select one of three options:

* Emulation mode: for running the game from a PC without connection to Amadeo, using the following keys to move the bridge parts:

Up:   `Y U I O P` 

Down: `H J K L ;`

* File mode: for reading inputs from a file written by Matlab (the previous game).

* Amadeo mode: real time mode, connected to Amadeo. The code is in BridgeClient.cs.


### Control Menu

The control menu is opened in Display 1; simultaneously, the game runs at Display 2.

* Active Units: marks the maximum set of fingers that should be tracked by the game.
* In Level 1, only one finger (selected at random from the Active Units set) is active --- this is a "warmup" level.
* In Level 2 onwards, the difficulty is adjusted dynamically: if the player succeeds, the game will add active fingers, up to the maximum set of active fingers. It also reduces the grace and increases the bridge heights. If the player fails, the game will remove active fingers, increase the grace and decrease the bridge heights.
* Isolated Mode: if marked, then in Level 2 onwards, only one finger from the "Active Units" set will have to move, and the others will have to remain at rest.
* Multiple Fingers: if marked, then in Level 5 onwards, two or more fingers (up to the maximum Active Units) will have to move simultaneously.

### Displays

Display 1 is used for the control menu.
Display 2 is used for the game itself.

UI Toolkit is used to control the two displays simultanously.
* IntructorOptions object controls the control menu. InstructorPanelSettings tells UI toolkit to show it on  Display 1.
* GameScreen object controls the game screen. PanelSettings tells UI toolkit to show it on  Display 2.

We need two cameras: MainCamera for Display 1, and SecondCamera for Display 2.


### Managers

* GameManager - not relevant.
* GameDataManager - preparation for future. Currently not used.
* AudioManager - preparation for future, e.g. for background music. Some features are not used.
* MonitorManager - activates the two displays.


