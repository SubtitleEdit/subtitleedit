# Auto Translate

Open **Translate → Auto-translate...**, choose a named **Profile**, select the source and target languages, and click **Translate**. Review the results in the grid, then click **OK** to apply them.

## Named profiles

The profile selector is shared by Auto Translate and Batch Convert. Each window remembers its own selected profile.

- **+** creates a profile.
- **…** edits the selected profile, including its name.
- The dropdown menu offers **Copy**, **Rename**, and **Delete**.

Names must be non-empty and unique, ignoring case. Copying creates an independent connection and policy, including the API key. Editing takes effect only after **OK**; closing or cancelling the editor discards the draft. Deleting the selected profile clears its selection and requires another choice. Running translations use a snapshot of their connection and policy.

## Traditional Engine

Choose the translation service and enter its required parameters. Supported services include Google Translate V1/V2, Microsoft Translator, DeepL, LibreTranslate, MyMemory, Lara, Papago, Baidu, NLLB API/Serve, and CrispASR MADLAD.

A traditional engine retains its service-specific client. For example, DeepL profiles save their API key, API URL, and formality independently. LibreTranslate and MyMemory allow an empty API key. CrispASR MADLAD profiles can use executable/model files directly or the existing model download controls.

## LLM-based Engine

![Translation profile editor](../screenshots/translation-profile-editor.png)

Choose the location:

- **Local → Choose Inference Engine:** llama.cpp, Ollama, LM Studio, or Custom.
- **Cloud → Choose Provider:** a provider URL preset or Custom.

A provider preset fills only **API URL**. Select **OpenAI Chat Completions** or **Anthropic Messages** separately, then enter the API key and model. You can edit the URL to use a compatible proxy or any other compatible service. Local and cloud profiles use the same two protocol clients.

The **Refresh** button beside Model queries the service's model list. You can always type the model name manually, including when a proxy does not expose a model-list route. LM Studio and a managed llama.cpp server can use their loaded model without a model identifier.

All LLM profiles default to **Advanced** translation. The profile editor's **Advanced...** button configures batches, context, synopsis, glossary, prompts, and sampling. See [Advanced translation](auto-translate-advanced.md).

Choose **Translate each line separately** explicitly to use the independent single-line prompt instead. Advanced translation never automatically switches to this mode after an error.

### Local llama.cpp

Subtitle Edit retains its model download, install status, server start/stop, and engine settings controls. Select a chat-compatible model from the list, or place a custom `.gguf` in the llama.cpp models folder.

To connect to a server you manage yourself, enable **Use remote server** in the profile editor and enter its API endpoint. In this mode Subtitle Edit does not download a model or start a server.

The new desktop profiles use chat APIs for both Advanced and single-line translation. Completion-only models such as MiLMMT require a replacement chat-compatible model. The command-line interface retains its existing options and behavior; see [command-line translation](../reference/command-line.md).

## Existing settings and imports

Configured engines are migrated once into named profiles. Different classic and advanced configurations remain separate. Existing LLM profiles also default to Advanced after migration; their old prompt is retained for manual single-line mode. URLs that cannot be safely converted from a completion route are preserved and marked for review.

Gemini profiles use its OpenAI-compatible endpoint. Mistral and OpenRouter keys are copied from the effective shared provider settings into the translation profile; subsequent translation edits do not change OCR or speech credentials. JSON and SE4 XML settings imports also create profiles from legacy translation settings.

## Selected lines and review

Right-click selected subtitle lines and choose **Auto-translate** to translate those lines. With no original column open, **Translate in place (do not create original)** replaces their text directly. The no-prompt shortcut uses the last selected profile and languages.

The grid shows source and translated text side by side and allows editing before acceptance. Source and target language choices are carried across profiles when supported by the new engine.

The main Settings button controls request delay, maximum request size, and whether music lines (`♪ ... ♪`) remain untranslated. Traditional profiles can also choose their line-merge strategy there. LLM translation mode and prompts belong to the profile editor.

| Key | Action |
|-----|--------|
| Escape | Close / Cancel |
| F1 | Show help |
