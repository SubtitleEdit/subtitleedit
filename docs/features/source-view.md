# Source View

Edit the subtitle as raw text in the current subtitle format — time codes, tags, headers and all. Useful for fixing something the grid cannot express, for pasting a whole file in at once, or just for seeing exactly what will be written to disk.

- **Menu:** Edit → Source view
- **Shortcut:** F2

The editor is virtualizing: only the lines on screen are colored and laid out, so even a very large file opens and scrolls instantly.

## Status Line

Along the bottom, next to the OK/Cancel buttons:

- **Line X, column Y** — where the caret is
- **Selected: N characters in M line(s)** — only while something is selected
- **Parse result** — the source is re-parsed shortly after you stop typing and the result is shown as `<lines> lines, <subtitles> subtitles`. If the format reports bad blocks, the error count is appended; if nothing can be parsed at all, the line turns red and reads `Could not parse as <format>`

That last one is the point of the status line: a broken edit shows up while you are making it, instead of when you press OK.

Above about 4 MB of source text the parse is skipped — re-parsing a file that size on every pause would cost more than the editing does — and only the line count is shown.

## Unsaved Changes

**OK** parses the source and replaces the subtitle with the result. If the source cannot be parsed at all, Subtitle Edit says so and leaves the window open.

If you have edited the text, **Cancel**, **Escape** and the window's close button all ask before throwing the changes away.

## Find and Replace

Press **Ctrl+F** (find) or **Ctrl+H** (replace) to open the search bar below the editor. It searches the source text — including time codes and tags — not the subtitle lines.

- **Case sensitive**, **Whole word** and **Regular expression** can be combined
- Searching wraps around at the end (and at the top, going backwards)
- With **Regular expression** on, the replacement supports group references (`$1`, `$2`, …). Without it, `$` is literal
- **Replace all** is a single undo step: one **Ctrl+Z** takes the whole batch back
- **Escape** closes the search bar; a second **Escape** closes the window

Selecting a word before pressing Ctrl+F searches for that word.

## Go to Line

**Ctrl+G** opens the go-to-line dialog and selects the line you pick. Line numbers are source lines, not subtitle numbers.

## Editing

Beyond the usual typing, selection and clipboard keys, the editor supports:

| Shortcut | Action |
|----------|--------|
| Alt+Up / Alt+Down | Move the current line (or the selected block) up / down |
| Ctrl+D | Duplicate the current line or selected block |
| Ctrl+Shift+K | Delete the current line |
| Ctrl+Backspace | Delete the word before the caret |
| Ctrl+Delete | Delete the word after the caret |
| Ctrl+Z / Ctrl+Y | Undo / redo |

On macOS, use Cmd where Ctrl is shown, and Option+Backspace / Option+Delete for the word deletes. Alt+Up / Alt+Down are the same on every platform.

The same commands — plus find, replace and go to line — are in the editor's right-click menu.

**F1** opens this help page.

## See also

- [Edit Menu](edit.md) — find and replace across subtitle lines rather than raw source
- [Keyboard Shortcuts](../reference/keyboard-shortcuts.md)
