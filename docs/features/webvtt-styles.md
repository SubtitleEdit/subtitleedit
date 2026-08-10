# WebVTT Styles and Voices

Manage the `STYLE` blocks of a WebVTT file — the cue classes referenced from the subtitle text as `<c.name>` — and the `<v Name>` voices used for speaker labelling.

**Toolbar:** the styles button (shown when the current format is WebVTT)
**Right-click a line:** `Styles`, `Voices`, `Browser preview`

## WebVTT Styles

A WebVTT style is CSS attached to a cue class:

```
WEBVTT

STYLE
::cue(.narrator) { color:rgb(255,255,0); font-style:italic }
```

A line then references it with `<c.narrator>Text</c>`, and several classes can be combined: `<c.narrator.small>Text</c>`.

### How to Use

1. Open or convert a subtitle to **WebVTT**.
2. Click the styles button in the toolbar to open the style manager.
3. Add, duplicate, remove, import or export styles in the left panel.
4. Select a style to edit its properties on the right; the preview and the raw CSS update as you type.
5. Click **OK** to write the styles back into the file header.

### Style Properties

Every CSS property is optional. Each colour has its own check box — an unchecked colour is left out of the style rather than written as a default, so a style that only sets `color` stays that way after a visit to the editor.

- **Font:** name and size (`font-family`, `font-size`)
- **Style:** bold, italic, underline, strikeout
- **Color:** the text colour (`color`)
- **Background:** the cue box colour (`background-color`)
- **Shadow:** colour and width (`text-shadow`)

Sizes are written in pixels. A style whose font size uses a relative unit (`em`, `%`) is left alone — Subtitle Edit has no pixel value to show for it, so the size field stays empty and the declaration is preserved only if you do not change it.

### Reordering, Import and Export

- Reorder styles from the right-click menu — **Move up** (`Ctrl+Up`), **Move down** (`Ctrl+Down`), **Move to top**, **Move to bottom**. This is the order the styles are written to the file with.
- **Import** reads the `STYLE` blocks of another WebVTT file and lets you pick which ones to take. Imported names that clash with an existing style are given a numbered suffix.
- **Export** writes the chosen styles to a small WebVTT file containing only a `STYLE` block, for reuse in other subtitles.

Two styles with the same name are flagged in the editor: a cue class is keyed by name, so the second one would simply shadow the first when the file is read back.

### Applying a Style to Lines

Select one or more lines, right-click and choose **Styles**. The check marks start from the focused line. Checking styles wraps the selected lines in `<c...>`; clearing all of them removes the class tags again.

## Voices

WebVTT marks a speaker with a voice span: `<v Joe>Where are you?`

Select one or more lines, right-click and choose **Voices**:

- Pick one of the voices already used in the file.
- **New voice...** prompts for a name and applies it.
- **Remove voices** strips the `<v>` tags from the selected lines.

Setting a voice replaces any voice already on the line rather than nesting a second tag.

## Browser Preview

With a video loaded in a browser-playable container (`.mp4`, `.m4v`, `.webm`, `.ogv`, `.mov`), right-click a line and choose **Browser preview**. Subtitle Edit writes a temporary HTML page that plays the video with the current subtitle attached as a WebVTT text track and opens it in your default browser, so you can check the cues in a real WebVTT renderer. The temporary page is deleted again shortly after it is opened.

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| F1 | Show help |
| Ctrl+Up / Ctrl+Down | Move the selected style up / down |
| Escape | Close dialog |

## See Also

- [ASSA Styles](assa-styles.md) — the equivalent for ASS/SSA subtitles
