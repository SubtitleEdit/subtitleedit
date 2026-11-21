# Auto Translate: Advanced Local Engines

The **llama.cpp advanced (local LLM)**, **Ollama advanced (local LLM)**, **LM Studio advanced (local LLM)** and **OpenAI Compatible API advanced** engines translate in batches with surrounding context, a synopsis, and a glossary — giving more consistent names, pronouns, and terminology than line-by-line translation. This page covers their **Advanced...** settings dialog and the llama.cpp engine settings.

LM Studio advanced talks to LM Studio's OpenAI-compatible endpoint (default `http://localhost:1234/v1/chat/completions`). LM Studio can have several models loaded; leave the model empty to use the loaded one.

OpenAI Compatible API advanced talks to any OpenAI-style chat-completions endpoint, for example a hosted provider. It takes its URL, model and API key like **OpenAI Compatible API**: the same default URL (`http://localhost:8000/v1/chat/completions`), and the model and the key are sent when set. It leaves out the request fields only llama.cpp knows, which a strict server rejects, and sampling values you leave at `-1` are not filled in from a curated llama.cpp model.

A model that reasons before it answers spends the reply budget on its thoughts. When a reply runs out that way, OpenAI Compatible API advanced asks once more with a larger budget and keeps it for the rest of the file. To switch reasoning off, put the provider's own field in **Extra request JSON**, for example `{"reasoning": {"enabled": false}}` on OpenRouter. OpenAI's own reasoning models (the o-series, gpt-5) want `max_completion_tokens` in place of `max_tokens` and accept only their default temperature: `{"max_tokens": null, "temperature": null, "max_completion_tokens": 8000}`. The **Advanced...** settings, Extra request JSON included, are shared by all four advanced engines, so clear such provider fields again before a run with a local engine.

The same engines are available headlessly in `seconv` as `--translate-engine:llamacpp-advanced`, `ollama-advanced`, `lmstudio-advanced` and `openai-compatible-advanced`, with the context options below as command-line options or as `tools.llamaCppAdvanced` keys in its `--settings` file — see [Advanced local engines](../reference/command-line.md#advanced-local-engines) in the command-line reference.

See [Auto Translate](auto-translate.md) for the translation window itself, and [Best local models for translating from English](auto-translate.md#best-local-models-for-translating-from-english) for which llama.cpp model to download.

## Advanced Settings

With an advanced engine selected, click **Advanced...** in the engine settings row.

![Advanced Settings](../screenshots/auto-translate-advanced.png)

### Context

- **Synopsis** — Optional description of the content (genre, setting, tone), included in every request so the model picks a fitting style. **Generate with AI** drafts it, with names and terms, from the whole subtitle: llama.cpp advanced uses the local model, or your own server when **Use external server (URL)** is on, the other advanced engines their own server, model and key
- **Glossary** — Terms the model must translate a certain way, one per line: `source term = translation`
- **Prompt text** — Custom system instructions replacing the built-in prompt (`{0}` = source language, `{1}` = target language); leave empty to use the built-in prompt
- **Previous lines as context** — How many already-translated lines before the batch are sent as history for consistency (default 12)
- **Lines per batch** — Subtitle lines per request (default 10). If the model returns an invalid reply, the batch is retried and then halved automatically
- **Formality** — Default, Formal, or Informal; adds an instruction to use e.g. "Sie"/"vous"/"usted" or "du"/"tu"/"tú"
- **Keep line breaks** — Ask the model to keep the same number of line breaks per line

### Sampling Parameters

`-1` means the model/server default is used.

- **Temperature** — Randomness. When unset, Subtitle Edit uses the curated value for a local llama.cpp model, or 0.2 — low temperatures keep terminology consistent across batches
- **Top-p**, **Top-k**, **Repeat penalty** — Standard sampling options, passed through when set
- **Max tokens per reply** — Response length cap; unset means no cap is sent
- **Server context size (tokens)** — The context window for the locally started llama.cpp server (default 16384). Bigger batches, more history, and a long synopsis/glossary need more context. Only affects the local llama.cpp server; Ollama and LM Studio manage their own context
- **Extra server parameters** — Additional `llama-server` command-line arguments, e.g. `-ngl 30 --no-mmap`, applied when the local server starts. They are appended after Subtitle Edit's own flags, so a repeated flag overrides Subtitle Edit's value
- **Use only these parameters** — Start `llama-server` with the parameters above instead of Subtitle Edit's own tuning; only the model, host and port are still set. Ignored while the field is empty
- **Extra request JSON** — A JSON object merged into every request after Subtitle Edit's own fields, so its fields win: provider switches such as a reasoning on/off field, or sampling fields the dialog does not have (`{"top_k": 40}`). A `null` value removes a field. `messages` and `response_format` cannot be set. Like the other settings here, it is shared by all advanced engines. **Generate with AI** sends it too, as it stands in the box

## llama.cpp Engine Settings

For the local llama.cpp engines, the gear button opens an info dialog about the bundled llama.cpp server build:

- **Backend** — The detected build (e.g. CUDA, Vulkan, CPU, or Metal on macOS)
- **Status** — Whether the pinned release is installed and up to date
- **Release** — The llama.cpp release Subtitle Edit is pinned to
- **Install folder** — Where the server binaries live

Use **Download** / **Update** / **Re-download...** to (re)install the server build, for example after a graphics driver change. On Windows and Linux you pick the build each time, so this is also how to switch between CPU and GPU. The same dialog is used by [AI Review](ai-review.md) and the [AI Assistant](ai-assistant.md).

### GPU acceleration

Pick **Vulkan (GPU)** instead of **CPU** to run llama.cpp on the GPU. Vulkan works with NVIDIA, AMD and Intel GPUs that have Vulkan drivers installed. On Windows, **CUDA (NVIDIA GPU)** is also available.

If llama.cpp is already installed as a CPU build, click the gear next to the engine, click **Re-download...** in the engine settings, and pick the GPU build. **Backend** shows which build is installed. The **Re-download...** button next to the model only downloads the model again.

Subtitle Edit offloads all model layers to the GPU, so the model should fit in the GPU memory. For a model that doesn't fit, add `-ngl N` under **Advanced...** to offload only N layers. If **Use only these parameters** is on, your parameters decide this.
