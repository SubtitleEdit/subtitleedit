# Advanced Translation

Every **LLM-based Engine** profile, whether Local or Cloud, defaults to Advanced translation. Open the profile editor and click **Advanced...** to edit that profile's independent policy. Copying a profile copies its policy; edits to one profile do not affect another.

Advanced sends numbered subtitle batches with recent translated context, an optional synopsis, and a glossary. The response is checked against the requested line numbers. Invalid replies are retried and batches can shrink to one line, still using the Advanced protocol. If that fails, translation stops with an error and completed results remain available. To use ordinary single-line requests, select **Translate each line separately** yourself in the profile editor.

OpenAI-compatible servers can reject structured-output constraints. For an explicit format rejection, the client tries a JSON object and then an unconstrained response, with the same result validation throughout. Authentication failures do not trigger this format downgrade.

See [Auto Translate](auto-translate.md) for profiles, protocols, migration, and local model management.

## Advanced Settings

With **Advanced** selected as the profile translation policy, click **Advanced...** in the profile editor.

### Context

- **Synopsis** — Optional description of the content (genre, setting, tone), included in every request so the model picks a fitting style
- **Glossary** — Terms the model must translate a certain way, one per line: `source term = translation`
- **Prompt text** — Custom system instructions replacing the built-in prompt (`{0}` = source language, `{1}` = target language); leave empty to use the built-in prompt
- **Previous lines as context** — How many already-translated lines before the batch are sent as history for consistency (default 12)
- **Lines per batch** — Subtitle lines per request (default 10). If the model returns an invalid reply, the batch is retried and then halved automatically
- **Formality** — Default, Formal, or Informal; adds an instruction to use e.g. "Sie"/"vous"/"usted" or "du"/"tu"/"tú"
- **Keep line breaks** — Ask the model to keep the same number of line breaks per line

### Sampling Parameters

Only parameters supported by the selected client are shown. Server launch options appear only for a locally managed llama.cpp server.

`-1` means the model/server default is used.

- **Temperature** — Randomness. When unset, Subtitle Edit uses the curated value for the model, or 0.2 — low temperatures keep terminology consistent across batches
- **Top-p**, **Top-k**, **Repeat penalty** — Standard sampling options, passed through when set
- **Max tokens per reply** — Response length cap; unset means no cap is sent
- **Server context size (tokens)** — The context window for the locally started llama.cpp server (default 16384). Bigger batches, more history, and a long synopsis/glossary need more context. Only affects the local llama.cpp server; Ollama and LM Studio manage their own context
- **Extra server parameters** — Additional `llama-server` command-line arguments, e.g. `-ngl 30 --no-mmap`, applied when the local server starts. They are appended after Subtitle Edit's own flags, so a repeated flag overrides Subtitle Edit's value
- **Use only these parameters** — Start `llama-server` with the parameters above instead of Subtitle Edit's own tuning; only the model, host and port are still set. Ignored while the field is empty

## llama.cpp Engine Settings

For a locally managed llama.cpp profile, the gear button opens an info dialog about the bundled llama.cpp server build:

- **Backend** — The detected build (e.g. CUDA, Vulkan, CPU, or Metal on macOS)
- **Status** — Whether the pinned release is installed and up to date
- **Release** — The llama.cpp release Subtitle Edit is pinned to
- **Install folder** — Where the server binaries live

Use **Download** / **Update** / **Re-download...** to (re)install the server build, for example after a graphics driver change. The same dialog is used by [AI Review](ai-review.md) and the [AI Assistant](ai-assistant.md).
