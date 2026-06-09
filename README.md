# Tobii TGI Input System Adapter

Bridges Tobii Game Integration (TGI) tracking data into the Unity Input System as virtual devices.

## Features

- Virtual gaze device (screen position + delta)
- Virtual head angle device (screen position + delta + rotation)
- Virtual head position device (position + delta + distance)

## Requirements

- Unity 6.0 or newer
- Unity Input System package (`com.unity.inputsystem`)
- Tobii Game Integration SDK available here: https://developer.tobii.com/pc-gaming/downloads/ 

## Installation (Git URL)

In Unity Package Manager:

1. Open **Window > Package Manager**.
2. Click **+** > **Add package from git URL...**
3. Enter your repository URL:

   `https://github.com/Adroit-Studios/Unity-TobiiGameIntegration-InputSystemAdapter.git`


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

## License

See [LICENSE.md](LICENSE.md).
