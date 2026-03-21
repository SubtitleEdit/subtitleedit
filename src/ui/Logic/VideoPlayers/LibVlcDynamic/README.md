# LibVLC Control Views

This document describes the three control views available for the LibVLC video player in Subtitle Edit Avalonia.

## Overview

Three control implementations are provided, each using a different rendering approach:

1. **LibVlcDynamicNativeControl** - Native window embedding (best performance)
2. **LibVlcDynamicSoftwareControl** - Software rendering with WriteableBitmap (most compatible)
3. **LibVlcDynamicOpenGlControl** - OpenGL texture rendering (balanced approach)

## 1. LibVlcDynamicNativeControl

### Description
Uses native window embedding to let VLC render directly to a platform-specific window handle.

### How It Works
- Inherits from `NativeControlHost`
- Passes the native window handle (HWND on Windows, X11 window on Linux, NSView on macOS) to LibVLC
- VLC handles all rendering internally using platform-specific APIs
- Uses `libvlc_media_player_set_hwnd()` on Windows and `libvlc_media_player_set_xwindow()` on Linux/macOS

### Advantages
- Best performance (hardware-accelerated by VLC)
- Lowest CPU usage
- Native video output quality

### Disadvantages
- Platform-specific code paths
- Less control over rendering pipeline
- May have window embedding issues on some platforms

### Usage
```csharp
var vlcPlayer = new LibVlcDynamicPlayer();
var control = new LibVlcDynamicNativeControl(vlcPlayer);
control.LoadFile("video.mp4");
```

## 2. LibVlcDynamicSoftwareControl

### Description
Uses memory rendering callbacks to capture raw video frames and display them using Avalonia's WriteableBitmap.

### How It Works
- Inherits from `Control`
- VLC decodes frames to a memory buffer
- Frames are copied to a `WriteableBitmap`
- Avalonia's rendering system displays the bitmap

### Advantages
- Works on all platforms consistently
- No platform-specific code
- Full control over frame data
- Easy to implement overlays and effects

### Disadvantages
- Higher CPU usage (software rendering + memory copies)
- Potentially higher memory usage
- May not handle high-resolution video as smoothly

### Usage
```csharp
var vlcPlayer = new LibVlcDynamicPlayer();
var control = new LibVlcDynamicSoftwareControl(vlcPlayer);
control.LoadFile("video.mp4");
```

## 3. LibVlcDynamicOpenGlControl

### Description
Uses memory rendering callbacks to capture video frames and uploads them to OpenGL textures for hardware-accelerated display.

### How It Works
- Inherits from `OpenGlControlBase`
- VLC decodes frames to a memory buffer
- Frames are uploaded to an OpenGL texture
- Texture is rendered using OpenGL quads

### Advantages
- Hardware-accelerated display
- Cross-platform OpenGL support
- Better performance than pure software rendering
- Can integrate with OpenGL-based effects

### Disadvantages
- Requires OpenGL support
- More complex than software rendering
- Texture upload overhead
- Not as efficient as native embedding

### Usage
```csharp
var vlcPlayer = new LibVlcDynamicPlayer();
var control = new LibVlcDynamicOpenGlControl(vlcPlayer);
control.LoadFile("video.mp4");
```

## Common API

All three controls implement the same basic API:

```csharp
public interface IVlcControl
{
    LibVlcDynamicPlayer? Player { get; }
    void LoadFile(string path);
    void TogglePlayPause();
    void Unload();
}
```

## Platform Recommendations

### Windows
1. **LibVlcDynamicNativeControl** (best performance)
2. **LibVlcDynamicOpenGlControl** (balanced)
3. **LibVlcDynamicSoftwareControl** (fallback)

### Linux
1. **LibVlcDynamicNativeControl** (best performance, X11/Wayland support)
2. **LibVlcDynamicOpenGlControl** (balanced)
3. **LibVlcDynamicSoftwareControl** (fallback)

### macOS
1. **LibVlcDynamicNativeControl** (best performance)
2. **LibVlcDynamicOpenGlControl** (balanced)
3. **LibVlcDynamicSoftwareControl** (fallback)

## Implementation Notes

### Native Control
- Requires platform-specific window handle management
- Cursor management to prevent VLC from changing the cursor
- Window handle is obtained from Avalonia's native control host

### Software Control
- Uses frame buffer locking for thread-safe pixel access
- Automatic bitmap recreation when control size changes
- Black screen displayed when no video is loaded

### OpenGL Control
- Uses OpenGL 2.1 fixed-function pipeline for compatibility
- Texture filtering set to GL_LINEAR for smooth scaling
- Proper cleanup of OpenGL resources on control destruction

## Future Enhancements

Potential improvements for all controls:

1. **Callback-based rendering** - Implement LibVLC's lock/unlock/display callbacks for more efficient frame handling
2. **Subtitle rendering** - Disable VLC's built-in subtitle rendering and handle it separately
3. **Aspect ratio** - Proper aspect ratio handling and letterboxing
4. **Performance metrics** - Add frame rate and performance monitoring
5. **Hardware decoding** - Leverage VLC's hardware decoding capabilities
