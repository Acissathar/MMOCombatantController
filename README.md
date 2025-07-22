# MMO Combatant Controller (ORK 3 Framework)

An MMO style character controller designed with ORK Framework in mind for the Unity game engine.

## Description

This is an attempt at emulating the MMO style character controller popularized by games like World of Warcraft, but specifically with the intention of integrating with ORK Framework.

Features:
1. 8 way directional movement that is relative to the camera, or retain direction and allow freelook when holding left mouse.
2. Can control character and camera entirely with just mouse if desired.
3. Moving platforms transfer movement to character.
4. Swimming movement in 3d space.
5. Auto run (and swim).
6. Double jumps!

## Getting Started

### Disclaimers

While this is inspired by MMO controls, it is _not_ a full replacement for such a system. This strictly handles camera and character movement, and is independent of any battle system.

### Dependencies

* [Scivolo Character Controller](https://assetstore.unity.com/packages/tools/physics/scivolo-character-controller-170660)
  * This is used as a replacement for the built in CharacterController component from Unity.
* [InternalDebug](https://github.com/Acissathar/InternalDebug)
  * Strips out Debug messages from non-dev builds.
* [ORK Framework](https://assetstore.unity.com/packages/tools/game-toolkits/rpg-editor-ork-framework-3-2025-308979)
  * This should work with ORK 3 2019 as well.
* Cinemachine 3.0
  * Cinemachine itself isn't _strictly_ necessary as the script handles all movement and collisions by itself (placed on a basic Cinemachine Camera), but the ORK Wrapper hooks into Cinemachine to help with priority control when blocking/unblocking Camera control so that you can more easily setup additional cameras for various situations.

It does not matter where these are installed (package or source) but they need to be in the project somewhere.

* Note: This is only tested with Unity 6, but in theory this should work fine in lower versions as there isn't really any Unity specific functionality tied to the camera or controller.

### Installing

#### Install via git URL

In Package Manager, add https://github.com/Acissathar/MMOCombatantController.git as a custom gitpackage.

![image](https://github.com/user-attachments/assets/eb88d6e1-4910-487c-93e6-82f4e274dc1a)

<img width="1133" height="176" alt="image" src="https://github.com/user-attachments/assets/40d62cb6-350b-425a-94e0-a138fd91dae2" />

A sample scene is provided in the package as well, but must be manually imported from the Package Manager dialogue. Additionally, there are a few example prefabs included to help quick start.

#### Source

Download repo, and copy the Runtime and Editor folders into your Unity project to modify directly.

### Executing program

#### Quick Start

* Add Character Capsule, Character Mover, and Ground Detector script to your combatant prefab (Scivolo character controller requirements).
* Add MMOCombatantCharacterController and ORKMMOPlayerControlWrapper (disable this by default to let ORK handle it for AI/Player characters) to your combatant prefab. (Or use MMO Combatant Example prefab in Samples folder and modify to fit your combatant).
  * Assign Input Keys into the appropriate fields in ORKMMOPlayerControlWrapper. You can use the circle button for quick lookup. (a set of input keys are included in the Samples folder, but to use them properly you will need to overwrite your existing keys in the GamingIsLove/_Data/Input Keys folder)
* In ORK/Makinom editor, under Base/Control -> Game Controls, remove the default player and camera controls.
  * Add a control behaivor with Blocked By Player and Placed On Player with the class name ORKMMOPlayerControlWrapper.
  * Add a control behaivor with Blocked By Camera and Placed On Camera with the class name ORKMMOCameraControlWrapper.
* In ORK/Makinom editor, under Combatants -> Combatants -> General Settings -> Default Movement Component, change this to MMO Combatant Character Controller.
  *  Tweak these values as desired (note these can be applied to individual combatants if desired).
* In scene, add a default Cinemachine Camera as a separate object with a MMOCombatantThirdPersonFollowCameraController script (or use the Combatant Third Person Follow Camera prefab in the Samples folder).
  * Tweak values as desired for collisions, positioning, and input sensitivity.
* In scene, add a Camera Brain component to your Main Camera and the ORKMMOCameraControlWrapper (or use the Camera Brain prefab in the Samples folder).
  * Assign the above Cinemachine Camera to the MMO Combatant Third Person Follow field.
  * Assign Input Keys into the appropriate fields. You can use the circle button for quick lookup. (a set of input keys are included in the Samples folder, but to use them properly you will need to overwrite your existing keys in the GamingIsLove/_Data/Input Keys folder)
* Add an ORK Game Starter with a start schematic for the combatant setup above, and play!

#### Visual Walkthrough

<img width="550" height="1225" alt="image" src="https://github.com/user-attachments/assets/c289cc58-c02c-4251-b688-7b2790ca260b" />

<img width="924" height="898" alt="image" src="https://github.com/user-attachments/assets/e7701b87-62a8-417f-9ec5-fd5ed71d9825" />

<img width="1603" height="928" alt="image" src="https://github.com/user-attachments/assets/b5db5a19-0127-4354-905a-9fa032d3f0b7" />

<img width="560" height="849" alt="image" src="https://github.com/user-attachments/assets/1617acfb-88fd-4ae6-8a10-02fe379e92b3" />

<img width="547" height="608" alt="image" src="https://github.com/user-attachments/assets/fd9b8063-9456-4a9e-88de-fba1d058a236" />

## Contact

[@Acissathar](https://twitter.com/Acissathar)

Join us in the (Unofficial) [ORKFramework discord!](https://discord.gg/Bafvu9wtvs) 

Project Link: [https://github.com/Acissathar/NavMesh-Cleaner/](https://github.com/Acissathar/NavMesh-Cleaner/)

## Version History

* 0.3
    * Added support for swim stamina. This supports using an ORK status value for diving, any swimming (including surface level), both, or neither. 

* 0.2
    * Initial Beta Release

## License

This project is licensed under the MIT License - see the LICENSE.md file for details
