---
layout: default
title: Translating Subtitle Edit
---

# Translating Subtitle Edit

You can translate Subtitle Edit's user interface into any language by creating a JSON language file. Translations are warmly welcomed — please share them so they can be included in future releases!

---

## Overview

Subtitle Edit 5 uses JSON language files. All UI strings are stored in a single file, and you create a translated copy for your language.

The English base file lives at:
`src/UI/Assets/Languages/English.json` on [GitHub](https://github.com/SubtitleEdit/subtitleedit/blob/main/src/UI/Assets/Languages/English.json)

---

## Step 1 — Download the English Base File

Download the raw English base file:

```
https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/main/src/UI/Assets/Languages/English.json
```

Save it as a starting point for your translation.

---

## Step 2 — Translate the File

You have two options:

### Option A — Use JSON Content Translator (recommended)

[JSON Content Translator](https://github.com/niksedk/json-content-translator/releases) is a dedicated tool for translating Subtitle Edit's JSON language files. It highlights untranslated entries, lets you work on one string at a time, and supports machine translation to speed up the process.

1. Download and run JSON Content Translator from the [releases page](https://github.com/niksedk/json-content-translator/releases).
2. Open the downloaded `English.json` file.
3. Translate the strings shown in the editor.
4. Save the translated file.

### Option B — Use a Text or JSON Editor

Open the JSON file in any text editor (Notepad, VS Code, etc.) and translate the **values** of the string entries. Do **not** change the keys.

For example, translate:
```json
"save": "_Save",
```
to (German example):
```json
"save": "_Speichern",
```

> **Note:** The underscore `_` before a letter marks the keyboard accelerator (underline in menus). Keep it in the translated string, placed before a suitable letter.

---

## Step 3 — Update the File Header

Near the top of the JSON file, update these fields:

```json
{
  "title": "Subtitle Edit",
  "version": null,
  "translatedBy": "Your Name (or email / homepage)",
  "cultureName": "de-DE",
  ...
}
```

| Field | Description |
|-------|-------------|
| `translatedBy` | Your name, email (`mailto:you@example.com`), or website URL |
| `cultureName` | A valid [.NET culture name](https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo) — e.g., `de-DE`, `fr-FR`, `pt-BR` |

The `cultureName` value **must** be a valid .NET culture identifier. 

---

## Step 4 — Save and Install the File

Save your translated file as **`{CultureName}.json`** (e.g., `de-DE.json`) and place it in the **`Languages`** subfolder inside Subtitle Edit's data folder:

| Platform | Languages folder path |
|----------|-----------------------|
| **Windows (installed)** | `%APPDATA%\Subtitle Edit\Languages\` |
| **Windows (portable)** | `[Subtitle Edit folder]\Languages\` |
| **Linux** | `~/.config/Subtitle Edit/Languages/` |
| **macOS** | `~/Library/Application Support/Subtitle Edit/Languages/` |

Create the `Languages` folder if it does not exist.

---

## Step 5 — Test Your Translation

1. Start (or restart) Subtitle Edit.
2. Go to **Options → Choose UI language...**.
3. Select your language from the list.
4. Verify that the translated strings appear correctly throughout the application.

---

## Step 6 — Share Your Translation

Please send your completed translation file to the Subtitle Edit team so it can be included in the official release for everyone to benefit from.

You can submit it via:
- A [pull request](https://github.com/SubtitleEdit/subtitleedit/pulls) to the repository (place the file under `src/UI/Assets/Languages/`)
- A [GitHub issue](https://github.com/SubtitleEdit/subtitleedit/issues) with the file attached
- An email to nikse.dk@gmail.com with the translation attached

---

## Tips

- **WinMerge** ([winmerge.org](https://winmerge.org)) is an excellent free tool for comparing your translation against an updated English base file to find new or changed strings after a release.
