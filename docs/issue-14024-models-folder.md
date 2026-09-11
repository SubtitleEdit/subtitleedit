# Issue #14024 - configurable AI models folder

## Status

Implemented locally on 2026-08-23. The change is uncommitted and has not been pushed or opened as a pull request.

The original request is [Subtitle Edit issue #14024](https://github.com/SubtitleEdit/subtitleedit/issues/14024): macOS users with a small internal disk need the large downloaded AI models to live on an external SSD, without moving the whole Subtitle Edit application-data directory.

## Problem

Before this change, Subtitle Edit had several independent model locations:

| Model family | Previous location | Why this was a problem |
| --- | --- | --- |
| OpenAI Whisper Python | `~/.cache/whisper` | The cache was outside Subtitle Edit's settings and had no in-app location setting. |
| Hugging Face / CTranslate2 | `~/.cache/huggingface/hub` | Large model snapshots accumulated on the internal disk. |
| Subtitle Edit Whisper engines | under the app data folder | These were tied to the normal application-data location. |
| CrispASR and its TTS backends | `<data>/CrispASR/models` | The executable and model files were coupled to one root. |
| llama.cpp | `<data>/llama.cpp/models` | Moving the whole data folder was the only practical workaround. |
| Paddle OCR, CrispEmbed, and Tesseract | OCR-specific subfolders | OCR model downloads also consumed internal storage. |
| audio.cpp and several C++ TTS engines | engine-specific `models` folders | Each engine had to be moved independently. |

The workaround described in the issue was to symlink the complete `Subtitle Edit` application-data directory. That also moves settings, logs, dictionaries, themes, and downloaded tools, which is broader than necessary.

## Design

The setting is an optional model root, not a replacement for the application-data root.

- An empty value preserves every previous path.
- A selected path is normalized to an absolute path.
- Executables and normal Subtitle Edit state remain in the normal application-data folder.
- Model subfolders retain stable family-specific names below the selected root.
- The setting is persisted in `Settings.json` as `General.ModelsFolder`.
- Existing files are not copied or deleted automatically. This avoids destructive migration and makes changing the setting reversible. Users can copy the existing model folders to the corresponding subfolders on the new disk before using the engines.
- Invalid hand-edited paths fall back to the historical locations instead of preventing startup.

## User-facing behavior

Options > General now includes **AI models folder** with a folder picker. Leave it empty to use the old locations. Select a directory such as `/Volumes/AI-Models/Subtitle Edit` to put future model downloads there.

The selected root is used after settings are saved. Python-based engines receive cache environment variables at process launch:

- OpenAI Whisper receives `XDG_CACHE_HOME=<selected root>/SpeechToText`, which makes its normal `whisper` cache resolve under the selected root.
- CTranslate2 receives `HF_HOME=<selected root>/SpeechToText/HuggingFace`, which makes its normal `hub` cache resolve under the selected root.

## Implementation details

### Configuration and persistence

- `src/ui/Logic/Config/SeGeneral.cs` adds `ModelsFolder`, defaulting to an empty string.
- `src/ui/Logic/Config/Se.cs` adds normalized `ModelsFolder`, `HasCustomModelsFolder`, and model path helpers.
- `src/libse/Common/Configuration.cs` adds the shared `ModelsDirectory` bridge and `ResolveModelsFolder`, so `libuilogic` can use the setting without depending on the UI assembly.
- `Se.UpdateLibSeSettings()` synchronizes the selected root into the shared configuration bridge.
- `Se.SaveSettings()` refreshes the llama.cpp model override after a settings change.

### Settings UI and localization

- `src/ui/Features/Options/Settings/SettingsPage.cs` adds the folder textbox and browse button.
- `src/ui/Features/Options/Settings/SettingsViewModel.cs` loads, browses, trims, and saves the value.
- `src/ui/Logic/Config/Language/Options/LanguageSettings.cs` and `src/ui/Assets/Languages/English.json` add the **AI models folder** label. The language property has an English initializer so older translation files remain usable.

### Model families covered

- OpenAI Whisper and CTranslate2 cache folders in `src/libuilogic/AudioToText`.
- Whisper.cpp and Const-me model folders, including the C++/cuBLAS/Vulkan engine wrappers.
- Purfview Faster Whisper XXL's `_models` folder.
- All current CrispASR speech-to-text backends and CrispASR-backed TTS model folders.
- Qwen3 ASR C++ model files.
- Qwen3 TTS C++, Kokoro TTS C++, and OmniVoice TTS C++ model folders.
- llama.cpp model files, while its server executable remains in the normal data folder.
- PaddleOCR models, CrispEmbed models, and Tesseract `tessdata`.
- IndexTTS 2.5 audio.cpp model files.

The path substitutions are intentionally centralized in `Se` so a future engine can opt into the same root without duplicating settings parsing or migration logic.

## Backward compatibility and migration

No setting means no behavior change. The legacy paths are still returned exactly when `General.ModelsFolder` is empty, and the tests pin this behavior.

Changing the setting does not move existing data. This is deliberate:

1. Subtitle Edit cannot safely assume that every folder is writable, mounted, or large enough for a copy.
2. A move could be interrupted and leave a partial model.
3. The user may want to keep models on both disks temporarily.

For a manual migration, close Subtitle Edit, copy the old model subfolder to the matching path below the new root, select the root in Options, save, and verify the engine's model list before deleting the old copy.

## Verification

The focused test command was:

```text
AVALONIA_TELEMETRY_OPTOUT=1 dotnet test tests/UI/UITests.csproj --no-restore --filter FullyQualifiedName~DataFolderLocationTests
```

Result: **6 passed, 0 failed** on `net10.0`.

The test coverage verifies:

- Empty setting preserves the historical data and Whisper cache paths.
- A custom absolute path is normalized and used for CrispASR and Whisper model paths.
- The application-data folder and error-log path remain unchanged when the model root changes.

Restore/build emitted existing `NU1900` vulnerability-feed access warnings from the sandboxed NuGet cache. They did not prevent compilation or test execution. No production model was downloaded during verification.

## Not included in this slice

- Automatic copy/move of existing model files.
- Third-party caches that are hard-coded inside external tools and are not controlled by an explicit model-directory argument. For example, CrispASR's optional auto-download behavior may still use its own cache when a backend downloads additional assets internally; Subtitle Edit-managed model downloads use the selected CrispASR model folder.
- A separate per-engine UI. The request is satisfied with one root, while preserving each engine's existing subfolder layout.

## Follow-up options

1. Add an explicit “Move existing models” wizard with free-space checks and resumable copy.
2. Add a “Show model folder” action beside the setting.
3. Audit newly added third-party engines for undocumented internal caches and add environment/argument overrides where upstream supports them.
