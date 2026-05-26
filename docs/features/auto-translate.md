# Auto Translate

Automatically translate subtitles using various translation engines and AI services.

- **Menu:** Auto translate → Auto translate...
- **Shortcut:** Configurable

<!-- Screenshot: Auto translate window -->
![Auto Translate](../screenshots/auto-translate.png)

## How to Use

1. Open **Auto translate → Auto translate...**
2. Select a translation engine
3. Select the source and target languages
4. Click **Translate** to start
5. Review the translations in the grid
6. Click **OK** to apply

## Supported Engines

- **Google Translate V1 API** — Free Google Translate
- **Google Translate V2 API** — Google Cloud Translation (requires API key)
- **Bing Microsoft Translator** — Azure Cognitive Services (requires API key)
- **DeepL V2 translate** — DeepL translation (requires API key)
- **LibreTranslate** — Open-source, self-hosted translation
- **MyMemory Translate** — Free translation memory
- **ChatGPT** — OpenAI AI translation (requires API key)
- **LM Studio (local LLM)** — Local LLM translation
- **Ollama (local LLM)** — Local LLM-based translation
- **llama.cpp (local LLM)** — Server-managed local LLM translation; Subtitle Edit downloads llama.cpp and a curated TranslateGemma model and runs a local `llama-server` for you
- **Anthropic Claude** — AI translation (requires API key)
- **Groq** — AI translation (requires API key)
- **OpenRouter** — AI translation (requires API key)
- **Lara** — AI translation (requires API key)
- **Perplexity** — AI translation (requires API key)
- **Google Gemini** — AI translation (requires API key)
- **NVIDIA** — AI translation (requires API key)
- **Mistral AI Translate** — AI translation (requires API key)
- **DeepSeek** — AI translation (requires API key)
- **Papago Translate** — Naver Papago translation (requires API key)
- **thammegowda-nllb-serve** — Self-hosted NLLB (No Language Left Behind) server
- **winstxnhdw-nllb-api** — NLLB (No Language Left Behind) API
- **Baidu Translate** — Baidu translation (requires App ID and secret)
- **CrispASR MADLAD** — Local MADLAD-based translation with downloadable models (shown with size and install status); also available in Batch Convert

## Engine Configuration

Depending on the selected engine, you may need to provide:

- **API Key** — Authentication key for cloud services
- **API URL** — Custom endpoint URL (for self-hosted services)
- **Model** — Specific model to use (for AI engines)

## Translation Review

The translation grid shows the original text alongside the translated text. You can edit individual translations before accepting them.

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| Escape | Close / Cancel |
| F1 | Open help |
