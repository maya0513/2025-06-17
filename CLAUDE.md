# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
This is a Unity VR project built on Unity 6000.0.41f1 featuring XR Interaction Toolkit integration with VIVE OpenXR support. The project serves as a comprehensive VR demo showcasing advanced interaction mechanics, locomotion systems, and device simulation capabilities.

## Key Dependencies & Architecture
- **Unity Version**: 6000.0.41f1
- **XR Interaction Toolkit**: 3.0.8 (Core VR interaction framework)
- **VIVE OpenXR**: GitHub package for HTC VIVE headset support
- **Universal Render Pipeline (URP)**: 17.0.4 for modern rendering
- **Input System**: 1.13.1 for unified input handling across devices

## Core Project Structure
```
Assets/
├── VIVEOpenXRInstaller.cs          # VIVE OpenXR package installer utility
├── InputSystem_Actions.inputactions # Comprehensive input mappings
├── Samples/XR Interaction Toolkit/3.0.8/
│   ├── Starter Assets/Scripts/      # Core VR interaction samples
│   └── XR Device Simulator/Scripts/ # Desktop VR simulation
└── TutorialInfo/Scripts/            # Project documentation system
```

## Assembly Definitions
The project uses modular assembly definitions:
- `StarterAssets.asmdef` - Runtime VR interaction code
- `StarterAssets.Editor.asmdef` - Editor validation and tools
- `Unity.XR.Interaction.Toolkit.Samples.DeviceSimulator.asmdef` - Device simulator

## Development Commands
Unity projects don't use traditional build commands. Development workflow:

1. **Open Project**: Launch Unity Hub → Open Project → Select this directory
2. **Build Project**: File → Build Settings → Build (or Build and Run)
3. **Run in Editor**: Press Play button in Unity Editor
4. **VR Testing**: Enable XR Device Simulator for desktop testing

## VR-Specific Development
- **Locomotion**: `DynamicMoveProvider.cs` handles movement with head/hand-relative options
- **Interactions**: Object spawning via `ObjectSpawner.cs` with camera-relative positioning  
- **Input**: Multi-device support including XR controllers, keyboards, and gamepads
- **Teleportation**: Advanced teleportation system with climbing mechanics
- **Device Simulation**: Use XR Device Simulator for desktop VR development without headset

## Script Organization
- Namespace: `UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets`
- Event-driven architecture for VR interactions
- XML documentation throughout codebase
- Clean separation between runtime and editor scripts

## Platform Support
- **Primary**: HTC VIVE via OpenXR
- **Secondary**: Meta Quest (via OpenXR compatibility)
- **Development**: Desktop simulation for testing without VR hardware

## Input System Configuration
The project uses Unity's Input System with comprehensive action mappings:
- Player actions (Move, Look, Attack, Interact, Jump, Sprint, Crouch)
- UI navigation actions
- Support for Keyboard/Mouse, Gamepad, Touch, Joystick, and XR controllers

## Build Profiles
Multiple build profiles configured for different platforms:
- Android (Quest/mobile VR)
- Standalone (PC VR)
- Each with optimized render pipeline settings