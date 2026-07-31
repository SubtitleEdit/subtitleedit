# Auto Translate: Advanced Local Engines

The **llama.cpp advanced (local LLM)** and **Ollama advanced (local LLM)** engines translate in batches with surrounding context, a synopsis, and a glossary — giving more consistent names, pronouns, and terminology than line-by-line translation. This page covers their **Advanced...** settings dialog and the llama.cpp engine settings.

See [Auto Translate](auto-translate.md) for the translation window itself.

## Advanced Settings

With an advanced engine selected, click **Advanced...** in the engine settings row.

<!-- Screenshot: Advanced settings window (auto-translate-advanced.png, not yet taken) -->

### Context

- **Synopsis** — Optional description of the content (genre, setting, tone), included in every request so the model picks a fitting style
- **Glossary** — Terms the model must translate a certain way, one per line: `source term = translation`
- **Prompt text** — Custom system instructions replacing the built-in prompt (`{0}` = source language, `{1}` = target language); leave empty to use the built-in prompt
- **Previous lines as context** — How many already-translated lines before the batch are sent as history for consistency (default 12)
- **Lines per batch** — Subtitle lines per request (default 10). If the model returns an invalid reply, the batch is retried and then halved automatically
- **Formality** — Default, Formal, or Informal; adds an instruction to use e.g. "Sie"/"vous"/"usted" or "du"/"tu"/"tú"
- **Keep line breaks** — Ask the model to keep the same number of line breaks per line

### Sampling Parameters

`-1` means the model/server default is used.

- **Temperature** — Randomness. When unset, Subtitle Edit uses the curated value for the model, or 0.2 — low temperatures keep terminology consistent across batches
- **Top-p**, **Top-k**, **Repeat penalty** — Standard sampling options, passed through when set
- **Max tokens per reply** — Response length cap; unset means no cap is sent
- **Server context size (tokens)** — The context window for the locally started llama.cpp server (default 16384). Bigger batches, more history, and a long synopsis/glossary need more context. Only affects the local llama.cpp server; Ollama manages its own context

## llama.cpp Engine Settings

For the local llama.cpp engines, the gear button opens an info dialog about the bundled llama.cpp server build:

- **Backend** — The detected build (e.g. CUDA, Vulkan, CPU, or Metal on macOS)
- **Status** — Whether the pinned release is installed and up to date
- **Release** — The llama.cpp release Subtitle Edit is pinned to
- **Install folder** — Where the server binaries live

Use **Download** / **Update** / **Re-download...** to (re)install the server build, for example after a graphics driver change. The same dialog is used by [AI Review](ai-review.md) and the [AI Assistant](ai-assistant.md).
