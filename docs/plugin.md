# Plugins (Subtitle Edit 5)

Subtitle Edit 5 plugins are **standalone executables** that communicate with
Subtitle Edit through **JSON files**. Subtitle Edit writes a request file, launches
the plugin process, waits for it to exit, then reads a response file the plugin
wrote. The plugin is free to show its own user interface while it runs.

This model is a clean break from the Subtitle Edit 4 plugin API (in-process WinForms
DLLs), which cannot run on the cross-platform Avalonia build. SE4 plugins are not
compatible with SE5 and must be ported.

> **Status:** the JSON contract, the host-side runner/catalog, the **Plugins** menu,
> the plugin manager (**Plugins → Manage plugins...** — enable/disable/remove and open
> the plugins folder), and the online installer (**Get plugins online...** — download
> and update plugins from the index) are implemented. Installing, enabling, disabling,
> and removing plugins update the **Plugins** menu live (no restart needed). A startup
> update notification is still to come.
>
> The **Plugins** menu is shown by default but can be hidden via
> **Options → Settings → Appearance → Show Plugins menu**. The menu must be enabled
> there to run or manage plugins from the main window.

## How a plugin runs

1. The user picks the plugin from a Subtitle Edit menu.
2. Subtitle Edit writes a `request.json` to a fresh temp folder.
3. Subtitle Edit launches the plugin executable with the **request file path as the
   first command-line argument**.
4. The plugin reads the request, does its work (optionally showing its own window),
   and writes a `response.json` to the path given in the request
   (`responseFilePath`).
5. The plugin exits with code `0`.
6. Subtitle Edit reads the response. On `ok` it creates an undo point and replaces
   the subtitle; on `cancelled` it does nothing; on `error` it shows the message.
7. Subtitle Edit deletes the temp folder.

Cancelling from Subtitle Edit kills the plugin process. A non-zero exit code, a
missing response file, or invalid JSON is reported to the user as an error and the
subtitle is left unchanged.

## Installation layout

Each plugin lives in its own folder under the `Plugins` directory in the Subtitle
Edit data folder (next to `Settings.json`; in portable installs that is the
Subtitle Edit program folder). Every plugin folder must contain a `plugin.json`
manifest:

```
Plugins/
  MyPlugin/
    plugin.json
    MyPlugin.exe          (Windows)
    MyPlugin              (Linux/macOS)
    icon.png              (optional)
```

Install a plugin either by copying its folder here manually, or via
**Plugins → Manage plugins... → Get plugins online...**, which downloads it from the
plugin index (see below).

## The online plugin index

**Get plugins online...** reads a JSON index and offers each listed plugin for
download. The index lives at the URL in `PluginConstants.OnlineIndexUrl`
(`https://raw.githubusercontent.com/SubtitleEdit/plugins/main/se5-plugins.json`):

```jsonc
{
  "plugins": [
    {
      "name": "Uppercase Selected Lines",
      "description": "Converts the text of the selected lines to UPPERCASE.",
      "version": "1.0.0",
      "author": "Jane Doe",
      "url": "https://github.com/example/se-uppercase-plugin",
      "date": "2026-05-01",
      "minSeVersion": "5.0.0",
      "downloads": {
        "win-x64":     "https://github.com/example/se-uppercase-plugin/releases/download/v1.0.0/Uppercase-win-x64.zip",
        "win-arm64":   "https://github.com/example/se-uppercase-plugin/releases/download/v1.0.0/Uppercase-win-arm64.zip",
        "linux-x64":   "https://github.com/example/se-uppercase-plugin/releases/download/v1.0.0/Uppercase-linux-x64.zip",
        "linux-arm64": "https://github.com/example/se-uppercase-plugin/releases/download/v1.0.0/Uppercase-linux-arm64.zip",
        "osx-x64":     "https://github.com/example/se-uppercase-plugin/releases/download/v1.0.0/Uppercase-osx-x64.zip",
        "osx-arm64":   "https://github.com/example/se-uppercase-plugin/releases/download/v1.0.0/Uppercase-osx-arm64.zip"
      }
    }
  ]
}
```

- `downloads` is a map of platform key → zip URL. Each key has the form
  `{os}-{arch}` where `os` is `win`, `linux`, or `osx` and `arch` is `x64`,
  `arm64`, `x86`, or `arm`. Keys are matched case-insensitively.
- Subtitle Edit picks the URL whose key matches the user's OS + architecture.
  A plugin with no key for the current platform is still shown in
  **Get plugins online...** but its **Install** button is disabled with the
  status *Not supported on this operating system*.
- Each URL must point to a `.zip` containing the plugin's folder (with
  `plugin.json` inside). Subtitle Edit unpacks it into the `Plugins` directory.
- If the zip has a single top-level folder it is used as the plugin folder name;
  otherwise the files are placed in a folder named after `name`.
- Installing a plugin that is already present **replaces** the existing copy, so the
  same mechanism handles updates. The installer compares the index `version` with the
  installed manifest `version` and shows *Update available* when the index is newer.

## plugin.json (manifest)

The manifest lets Subtitle Edit list the plugin in its menus without launching it.

```jsonc
{
  "apiVersion": 1,
  "name": "Uppercase Selected Lines",
  "description": "Converts the text of the selected lines to UPPERCASE.",
  "version": "1.0.0",
  "author": "Jane Doe",
  "url": "https://github.com/example/se-uppercase-plugin",
  "menu": "Tools",
  "shortcut": "Control+Shift+U",
  "minSeVersion": "5.0.0",
  "icon": "icon.png",
  "executables": {
    "windows": "MyPlugin.exe",
    "linux": "MyPlugin",
    "macos": "MyPlugin"
  }
}
```

| Field          | Required | Notes |
|----------------|----------|-------|
| `apiVersion`   | yes      | JSON contract version. Currently `1`. |
| `name`         | yes      | Menu text. |
| `description`  | no       | Shown in the plugin manager. |
| `version`      | no       | The plugin's own version. |
| `author`       | no       | |
| `url`          | no       | Project/home page. |
| `menu`         | no       | Intended menu group: `Tools` (default), `File`, `Sync`, `Translate`, `SpellCheck`, or `Assa`. Currently every plugin is listed under a single top-level **Plugins** menu; `menu`-based routing is planned. |
| `shortcut`     | no       | Suggested keyboard shortcut. |
| `minSeVersion` | no       | Minimum supported Subtitle Edit version. |
| `icon`         | no       | Icon file name, relative to the plugin folder. |
| `executables`  | yes\*    | Native executable file names per OS, relative to the plugin folder. |
| `runtime`      | no       | Set to `"dotnet"` to launch via the shared .NET runtime instead of a native exe. |
| `entry`        | no       | Entry `.dll` (relative to the plugin folder), used with `runtime: "dotnet"`. |

\* Provide either `executables` **or** `runtime` + `entry`. A `dotnet` plugin is
launched as `dotnet <entry> <requestFilePath>`.

## request.json (Subtitle Edit → plugin)

```jsonc
{
  "apiVersion": 1,
  "requestType": "run",
  "responseFilePath": "C:\\Users\\...\\Temp\\SubtitleEditPlugins\\<id>\\response.json",
  "tempDirectory": "C:\\Users\\...\\Temp\\SubtitleEditPlugins\\<id>",
  "subtitle": {
    "format": "Advanced Sub Station Alpha",
    "fileName": "C:\\videos\\episode01.ass",
    "native": "[Script Info]\n...full subtitle in its original format...",
    "subRip": "1\n00:00:01,000 --> 00:00:03,000\nHello world\n\n..."
  },
  "selectedIndices": [3, 4, 5],
  "videoFileName": "C:\\videos\\episode01.mkv",
  "frameRate": 23.976,
  "videoDurationSeconds": 1432.5,
  "videoWidth": 1920,
  "videoHeight": 1080,
  "uiLanguage": "English",
  "theme": "Dark",
  "themeColors": {
    "isDark": true,
    "backgroundColor": "#FF212121",
    "foregroundColor": "#FFDCDCDC",
    "accentColor": "#631E90FF",
    "backgroundColorLighter": "#FF262626",
    "backgroundColorHeader": "#FF303030",
    "bookmarkColor": "#FFFFD700"
  },
  "seVersion": "5.0.0",
  "settings": { "lastUsedOption": true },
  "settingsVersion": 2
}
```

- `subtitle.native` is the subtitle serialized in its **original format**;
  `subtitle.subRip` is the **same content as SubRip (.srt)** — use whichever is
  easier for your plugin.
- `selectedIndices` are zero-based line indices selected in the grid (empty if none).
- `settings` is whatever JSON your plugin returned in its previous response;
  it is `null` on first run. Use it to persist plugin-private settings.
- `settingsVersion` is the schema version your plugin attached to `settings` in
  its previous response, handed back unchanged. Use it to migrate or reset old
  settings when you change your own schema. Null on first run, and on older SE
  versions that don't track it.
- `videoDurationSeconds`, `videoWidth`, `videoHeight` describe the loaded video.
  Null when no video is loaded (or on older SE versions). Saves you from
  re-opening the video file just to read these.
- `theme` and `uiLanguage` let your plugin's own UI match Subtitle Edit.
- `themeColors` carries the active theme's colors as `#AARRGGBB` hex strings so
  your plugin's own UI can match Subtitle Edit. May be omitted in older SE
  versions — fall back to your platform's defaults when it is missing.

## response.json (plugin → Subtitle Edit)

Write this file to `responseFilePath` before exiting with code `0`.

```jsonc
{
  "apiVersion": 1,
  "status": "ok",
  "message": "Converted 3 lines to uppercase.",
  "subtitle": {
    "format": "SubRip",
    "native": "1\n00:00:01,000 --> 00:00:03,000\nHELLO WORLD\n\n..."
  },
  "settings": { "lastUsedOption": true },
  "settingsVersion": 2,
  "undoDescription": "Uppercase Selected Lines 1.0.0"
}
```

| `status`     | Effect |
|--------------|--------|
| `ok`         | Subtitle Edit re-parses `subtitle.native` (as `subtitle.format`), makes an undo point, and replaces the current subtitle. |
| `cancelled`  | Nothing changes. Use this when the user closed your plugin's window. |
| `error`      | `message` is shown to the user; the subtitle is left unchanged. |

- For `status: "ok"` you only need to set `subtitle.format` and `subtitle.native`.
  Return the format you find most convenient — `SubRip` is always safe.
- If `subtitle.native` cannot be parsed, Subtitle Edit aborts with an error and
  makes no changes.
- `settings` is stored by Subtitle Edit and handed back in the next request.
- `settingsVersion` is stored alongside `settings` and handed back via
  `request.settingsVersion` on the next run. Bump it whenever you change the
  shape of `settings` so you can detect (and migrate or discard) stale data
  written by an older build of your plugin. Optional; if you don't write it,
  SE clears any previously-stored version for this plugin.
- `undoDescription` is optional; it labels the undo-history entry.

## Notes for plugin authors

- The plugin process inherits no special permissions — it is an ordinary
  executable. Only install plugins you trust.
- The working directory is set to your plugin's folder; the temp folder
  (`tempDirectory`) is yours to use for scratch files and is deleted after the run.
- JSON property names are camelCase; parsing is case-insensitive and unknown
  members are ignored, so the contract can grow without breaking older plugins.
- Keep `apiVersion` at `1`. If a future version introduces breaking changes the
  number will be bumped and Subtitle Edit will tell the user to update the plugin.
- A plugin can be written in any language — the contract is just JSON files and a
  command-line argument. A .NET helper SDK (request/response types) is published
  separately in the [Subtitle Edit plugins repository](https://github.com/SubtitleEdit/plugins).
