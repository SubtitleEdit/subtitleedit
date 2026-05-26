# macOS App Bundle Update Script

## update-plist-version.sh

This script automatically updates the `Info.plist` file with version information extracted from `Se.cs`.

### Usage

```bash
./update-plist-version.sh [path-to-Se.cs] [path-to-Info.plist]
```

If no arguments are provided, it uses default paths:
- Se.cs: `src/ui/Logic/Config/Se.cs`
- Info.plist: `installer/macBundle/SubtitleEdit.app/Contents/Info.plist`

### What it does

1. Extracts the version from Se.cs (e.g., `"v5.0.0-beta32"`)
2. Converts it to macOS-compatible formats:
   - **CFBundleShortVersionString**: user-visible version (e.g., `5.0.0-beta32`; for `preview` versions a dot is inserted, so `v5.0.0-preview95` becomes `5.0.0-preview.95`)
   - **CFBundleVersion**: numeric-only build number derived from the digits in the version (e.g., `50032` for `v5.0.0-beta32`, `500095` for `v5.0.0-preview95`)
3. Updates the Info.plist file with these values

### Example

If Se.cs contains:
```csharp
public static string Version { get; set; } = "v5.0.0-beta32";
```

The script will update Info.plist with:
- `CFBundleShortVersionString`: `5.0.0-beta32`
- `CFBundleVersion`: `50032`

### Integration

This script is automatically called during the macOS build process in the GitHub Actions workflow before creating the app bundle.
