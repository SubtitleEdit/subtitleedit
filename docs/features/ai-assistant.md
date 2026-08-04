# AI Assistant

Get AI help with the current subtitle line — fix errors, shorten it to fit the reading speed, change the tone, or ask a free-form question about it.

- **Open:** The robot button in the text editor's button row, or right-click the text box → **AI assistant**

<!-- Screenshot: AI assistant window (ai-assistant.png, not yet taken) -->

The assistant works on the **currently selected line only**. The three lines before and after are sent along as read-only context so the model understands the dialogue, but they are never changed.

## Quick Actions

- **Fix errors** — Correct spelling, grammar, and punctuation in the detected language without rephrasing; names, slang, formatting tags, and line breaks are kept
- **Fit reading speed** — Rephrase the line shorter so it fits your maximum line length and characters/second settings, keeping the exact meaning
- **More formal** / **More casual** — Rewrite the line's register while keeping the meaning, tags, and line breaks

You can also type your own request in the **Ask** box — either an instruction ("split this into two sentences") or a question ("what does this idiom mean?").

## Applying the Result

The model's answer appears in the **Suggestion** box, where you can still edit it. Nothing changes until you press **Apply to line**. Applied changes go through the normal undo history. If the model produced hidden reasoning, an info button shows it.

## Engines

The assistant shares its engine configuration with [AI Review](ai-review.md) — changing it here changes it there too:

- **llama.cpp** (default) — Fully local; pick a model from the download list, and Subtitle Edit manages the server. The gear button opens the llama.cpp engine settings
- **Ollama** — Uses a running Ollama server (`http://localhost:11434` by default); the **...** button lists the models on your server
- **OpenAI-compatible** — Any OpenAI-compatible endpoint: enter URL, model, and (if needed) API key

The robot button can be hidden under Options → Settings → Appearance → **Text box: show AI assistant button**.
