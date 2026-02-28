# macOS App Bundle Update Script

## update-plist-version.sh

This script automatically updates the `Info.plist` file with version information extracted from `Se.cs`.

### Usage

```bash
./update-plist-version.sh [path-to-Se.cs] [path-to-Info.plist]
```

If no arguments are provided, it uses default paths:
- Se.cs: `src/UI/Logic/Config/Se.cs`
- Info.plist: `installer/macBundle/SubtitleEdit.app/Contents/Info.plist`

### What it does

1. Extracts the version from Se.cs (e.g., `"v5.0.0-preview95"`)
2. Converts it to macOS-compatible formats:
   - **CFBundleShortVersionString**: `5.0.0-preview.95` (user-visible version)
   - **CFBundleVersion**: `500095` (numeric-only build number)
3. Updates the Info.plist file with these values

### Example

If Se.cs contains:
```csharp
public static string Version { get; set; } = "v5.0.0-preview95";
```

The script will update Info.plist with:
- `CFBundleShortVersionString`: `5.0.0-preview.95`
- `CFBundleVersion`: `500095`

### Integration

This script is automatically called during the macOS build process in the GitHub Actions workflow before creating the app bundle.
