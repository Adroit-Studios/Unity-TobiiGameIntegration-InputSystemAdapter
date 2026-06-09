# Tobii TGI Input System Adapter

Bridges Tobii Game Integration (TGI) tracking data into the Unity Input System as virtual devices.

## Features

- Virtual gaze device (screen position + delta)
- Virtual head angle device (screen position + delta + rotation)
- Virtual head position device (position + delta + distance)

## Requirements

- Unity 6.0 or newer
- Unity Input System package 1.7 or newer (`com.unity.inputsystem`)
- Tobii Game Integration API SDK v9.0.4 or newer. https://developer.tobii.com/pc-gaming/downloads/ 

## Installation (Git URL)

In Unity Package Manager:

1. Open **Window > Package Manager**.
2. Click **+** > **Add package from git URL...**
3. Enter your repository URL:

   `https://github.com/Adroit-Studios/Unity-TobiiGameIntegration-InputSystemAdapter.git`

## Tobii SDK Setup

This package does not ship the Tobii Game Integration SDK files. You need to copy the required SDK files from the official Tobii SDK into your Unity project.

### 1. Add the managed wrapper script

Copy `TobiiGameIntegrationApi.cs` from the Tobii SDK into your project under a normal runtime scripts folder, for example:

- `Assets/Imported/Tobii/Scripts/TobiiGameIntegrationApi.cs`

The exact folder name is not important, but it must be inside `Assets/` so Unity compiles it.

### 2. Add the native DLLs

Copy the Tobii native plugin DLLs into a plugin folder in your project, for example:

- `Assets/Plugins/Tobii/`

On Windows Editor x64, the important file is typically:

- `tobii_gameintegration_x64.dll`


### 3. Verify Unity import settings

After copying the DLLs into `Assets/Plugins/Tobii/`, select each DLL in Unity and confirm the Plugin Inspector settings match the platform you want to support.

For a Windows-only setup, the DLL should usually be enabled for:

- Editor
- Standalone
- Windows x86_64

### 4. Common issue

If Unity throws `DllNotFoundException`, the usual causes are:

- the DLL was not copied into the project
- the wrong DLL variant was copied
- `TobiiGameIntegrationApi.cs` is expecting a debug DLL name such as `tobii_gameintegration_x64_d.dll`, but only a release DLL is present. You may need to edit the provided `TobiiGameIntegrationApi.cs` in order to point to the non _d versions of the .dll files.


## Quick Start

1. Add `TGIHardwareManager` to a scene object.
2. Add `TGIVirtualInput` to a scene object.
3. Create Input Actions that bind to the virtual device controls (`TGIGaze`, `TGIHead`, `TGIHeadPosition`).
4. Enter Play Mode and verify functionality using the debug visualizations built into the hardware manager.

## Notes

- In Editor, use **Free Aspect** in Game view for accurate coordinate behavior.
- Ensure required Tobii native DLLs are present for your target platform and build type.

## Samples

This package includes an optional sample scene under `Samples~/TGI Demo`.

<!-- Maintainer note: Keep sample content in Samples~ so it stays hidden from normal package import and is exposed through Package Manager sample import only. -->

## License

See [LICENSE.md](LICENSE.md).
