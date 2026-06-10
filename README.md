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

### 1. Add the managed API DLL

Copy the managed SDK assembly into your project Plugins folder:

- `Assets/Plugins/Tobii.GameIntegration.Net.dll`

Do not include `TobiiGameIntegrationApi.cs` when using the managed DLL path.

### 2. Add the native runtime DLL

Copy the native plugin DLL into Plugins:

- `Assets/Plugins/tobii_gameintegration_x64.dll`

### 3. Verify Unity import settings

Select each DLL in Unity and confirm Plugin Inspector settings.

For `Tobii.GameIntegration.Net.dll` (managed):

- `Auto Reference` enabled
- Included for Editor and target runtime platforms

For `tobii_gameintegration_x64.dll` (native, Windows x64):

- Editor enabled
- Standalone enabled
- Windows x86_64 enabled

### 4. Common issues

If Unity throws `DllNotFoundException`, usual causes are:

- native DLL not copied into `Assets/Plugins`
- wrong native DLL variant for your platform
- native DLL platform settings are incorrect

If package scripts cannot resolve `Tobii.GameIntegration.Net` types:

- managed `Tobii.GameIntegration.Net.dll` is missing
- `Auto Reference` is disabled on the managed DLL
- both `TobiiGameIntegrationApi.cs` and `Tobii.GameIntegration.Net.dll` are present and conflicting


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
