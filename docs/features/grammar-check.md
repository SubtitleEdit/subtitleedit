# Grammar Check (LanguageTool)

Check spelling, grammar, punctuation, casing and style with a [LanguageTool](https://languagetool.org) server - the public API, or your own installation (for example the [docker image](https://github.com/Erikvl87/docker-languagetool)). Nothing is changed until you apply the fixes you agree with.

- **Menu:** Tools → Grammar check (LanguageTool)...

Unlike [AI Review](ai-review.md), LanguageTool is rule based: it points at an exact span of text and offers concrete replacements, so it never rewrites a line and never invents changes. It is also considerably faster, and with a self-hosted server the subtitle text never leaves your machine.

## Server

Enter the base address of the server in the **Server** box - `https://api.languagetool.org` for the public API, or something like `http://localhost:8010` for a local docker container. The address may include the `/v2` or `/v2/check` suffix; Subtitle Edit strips it.

The circular arrow button next to the box tests the connection and reloads the language list, which also happens automatically when the window opens.

> The public API is rate limited and rejects large amounts of text from one address. For a whole subtitle, a self-hosted server is the better choice.

## Language

The language list comes from the server. It defaults to the auto-detected language of the subtitle, and falls back to the server's own detection if you leave it on **Auto**. For languages with variants (English, German, Portuguese...) pick the right one - the variants disagree about spelling and punctuation.

**Picky** turns on the stricter rules LanguageTool leaves off by default: redundancy, wordiness and typography suggestions. Useful for a final polish, noisy for a first pass.

## Checking

Press **Check**. The subtitle is sent in batches and issues appear in the grid while the check runs - press **Stop** to keep what has been found so far.

Each row shows:

- **Apply** — checkbox deciding whether the fix is applied
- **Line number** and a **category** tag (spelling, grammar, punctuation, casing, style)
- **Issue** — LanguageTool's short description of the rule that fired
- **Before / After** — with the changed words highlighted

Selecting a row shows the full explanation below the grid. When LanguageTool offers more than one replacement, a drop-down appears next to the explanation - pick the one you want and the After column follows.

Filter the grid with the category chips. Press **Apply N fixes** to apply the checked rows; several fixes on the same line are applied together, and the whole run is a single undo step (Ctrl+Z reverts everything).

Spelling, grammar, punctuation and casing fixes are checked by default. Style suggestions are a matter of taste, so they start unchecked. Rules that only point at a problem without offering a replacement cannot be checked at all - correct those by hand.

## Formatting tags and line breaks

Italic and font tags, ASSA override blocks such as `{\an8}` and music symbols are sent as markup: LanguageTool ignores them instead of reading them as words, but the positions it reports still refer to the original line, so a fix lands exactly where it belongs and the tags stay untouched. On the rare occasion an issue spans a tag or a line break, it is dropped rather than applied - fixing it would break the formatting.

A sentence continuing into the next subtitle is checked as one sentence, the way it reads on screen, so agreement errors spread over two lines are found. Fixes are still applied per line, so timing and reading speed are unaffected.

## Settings

The **Settings** button holds the options that are rarely changed:

- **User name** and **API key** — only needed for a LanguageTool premium account, or a server that requires credentials
- **Disabled rules** — comma separated rule ids to ignore, e.g. `WHITESPACE_RULE,UPPERCASE_SENTENCE_START`. The rule id of a selected issue is shown in the explanation line
- **Lines per request** — how many subtitle lines go into one request (25 by default)
