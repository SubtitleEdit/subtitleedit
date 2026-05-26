# Text to Speech

Generate speech audio from subtitle text using various TTS engines.

- **Menu:** Video → Text to speech...
- **Shortcut:** Configurable

<!-- Screenshot: Text to Speech window -->
![Text to Speech](../screenshots/text-to-speech.png)

## How to Use

1. Open **Video → Text to speech...**
2. Select a TTS engine from the dropdown
3. Select a language and voice
4. Optionally enable **Review audio clips** to review each generated clip
5. Optionally enable **Generate video file** to create a video with the audio
6. Click **Generate** to start

## Supported Engines

- **Piper** — Local, open-source TTS (Windows and Linux)
- **EdgeTts** — Microsoft Edge online voices
- **AllTalk** — Local TTS server
- **ElevenLabs** — Cloud-based, high-quality voices (requires API key)
- **AzureSpeech** — Microsoft cloud TTS (requires API key and region)
- **MistralSpeech** — Cloud-based Mistral speech generation (requires API key)
- **Murf** — Cloud TTS (requires API key)
- **GoogleSpeech** — Google cloud TTS (requires key file)
- **Kokoro TTS** — Local downloadable Kokoro TTS server and models
- **OmniVoice TTS** — Local CPU TTS with voice cloning and many languages
- **Qwen3 TTS (CrispASR)** — Local Qwen3 TTS running through the CrispASR runtime (VoiceDesign and CustomVoice 1.7B models)
- **Chatterbox TTS (CrispASR)** — Chatterbox TTS via the CrispASR runtime, with voice cloning (Base or Turbo model)

Local downloadable engines are installed into the Subtitle Edit data folder when you accept the download prompt.

## Engine Settings

Some engines require additional configuration:

- **API Key** — Enter your API key for cloud-based engines
- **Region** — Select the Azure region (for Azure engine)
- **Model** — Select the voice model
- **Key file** — Browse for Google Cloud service account key file

## Local SE5 Engines

### Qwen3 TTS (CrispASR)

Qwen3 TTS runs through the CrispASR runtime and shares the `CrispASR/models` directory with the speech-to-text Crisp ASR engines.

- Available model choices are **1.7B VoiceDesign** (uses a free-text voice instruction) and **1.7B CustomVoice** (voice cloning from a reference WAV).
- Subtitle Edit downloads the engine and the selected talker + codec/tokenizer GGUFs on first use.
- Imported reference WAV voices appear in the voice dropdown.

### Chatterbox TTS (CrispASR)

Chatterbox TTS runs through the CrispASR runtime (shared with the speech-to-text feature) and supports voice cloning.

- Available model choices are **Base** and **Turbo**.
- Imported reference WAV voices are sent as the per-request voice for runtime cloning.

### Kokoro TTS

Kokoro TTS runs a local server with downloadable models.

- Subtitle Edit downloads the engine and model files when needed.
- Voice names are available immediately from the bundled voice list and can be refreshed from the running server.
- It is a good choice when you want a local, multilingual TTS engine without an API key.

### OmniVoice TTS

OmniVoice TTS runs the omnivoice-tts CLI on CPU. It supports a large set of languages and voice cloning from reference WAV files (with an accompanying transcript).

### MistralSpeech

MistralSpeech is configured with an API key and model selection. The selected model is remembered in settings.

## Review Audio Clips

When **Review audio clips** is enabled, a dedicated review window opens after generation. This window lets you inspect, play, and regenerate audio for every subtitle line before the result is used. A 120px waveform of the original video audio is shown above the grid as a reference. Per-session review state (clips, history, includes, edits) is persisted to `SubtitleEditTts.json` so you can return to the same review later.

### The Review Grid

Each subtitle line is shown as a row with the following columns:

| Column | Description |
|--------|-------------|
| **Include** | Checkbox to include or exclude the line from the final output |
| **#** | Subtitle line number |
| **Text** | The subtitle text (editable — double-click to modify before regenerating) |
| **Voice** | The voice used for that line |
| **Speed** | The speed factor applied to fit the audio into the subtitle's duration |
| **CPS** | Characters per second for the subtitle line |

### Playback Controls

- **Play** — Plays the selected line's audio clip
- **Stop** — Stops playback
- **Auto-continue** — When enabled, playback automatically advances to the next line as soon as the current clip finishes

### Regenerating a Clip

You can regenerate the audio for any individual line:

1. Select the line in the grid
2. Choose the desired engine, voice, language, model, or style from the dropdowns
3. Click **Regenerate** or press **Ctrl+R**

The new clip is trimmed for silence and automatically speed-adjusted to fit the subtitle timing. After regeneration the new clip plays back immediately for review.

For ElevenLabs, extra fine-tuning parameters are available when that engine is selected: **Stability**, **Similarity**, **Speaker Boost**, **Speed**, and **Style Exaggeration**. A **Reset** button restores all ElevenLabs parameters to their defaults.

### Regeneration History

Every time a clip is regenerated, the previous version is saved. To review the history for a line, click the **History** button on that row. The history dialog shows all generated versions with their voice name and speed, and lets you play each one. Selecting a version and clicking **OK** restores it as the active clip for that line.

### Including / Excluding Lines

Uncheck the **Include** checkbox on any row to exclude that line's audio from the final output. Excluded lines are skipped when the video file is assembled.

### Exporting Clips

Click **Export** to save all audio clips and a `SubtitleEditTts.json` metadata file to a folder of your choice. The JSON file records the audio file names, subtitle timings, voice names, engine names, speed factors, and text for each line, making it easy to re-import or post-process the clips externally.

### Keyboard Shortcuts

| Key | Action |
|-----|--------|
| Ctrl+R | Regenerate selected line |
| Escape | Close / Cancel |
| F1 | Open help |
