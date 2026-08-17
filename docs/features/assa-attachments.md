# ASSA Attachments

Manage fonts and images embedded as attachments in an Advanced SubStation Alpha (ASS/SSA) subtitle file.

**Menu:** `ASSA tools` → `Attachments...`

![ASSA Attachments Screenshot](../screenshots/assa-attachments.png)

## How to Use

1. Open a subtitle file in ASS/SSA format.
2. Go to **ASSA tools** → **Attachments...** to open the attachments manager.
3. View the list of currently embedded fonts and images.
4. Use **Add** to attach new font or image files.
5. Select an attachment and use **Delete** or **Delete All** to remove attachments.
6. Select an image attachment to see a preview.
7. Click **OK** to save changes.

## Features

### Font Attachments
- Embed TrueType (.ttf) and OpenType (.otf) font files directly in the subtitle file.
- Ensures the correct fonts are available regardless of the system fonts installed.

### Image Attachments
- Embed image files (PNG, JPG, GIF, BMP, ICO) in the subtitle file.
- Preview embedded images directly in the dialog.

### Management
- Add one or more attachments at a time.
- Delete individual attachments or clear all attachments.
- View attachment names and sizes.

### Trim Fonts to Used Characters
- **Trim fonts to used characters...** rewrites each embedded TrueType font so that only the glyphs used by the subtitle's text keep their outlines - a full CJK font of many megabytes typically shrinks to a few hundred kilobytes.
- Character coverage, kerning, ligatures and complex-script shaping (Arabic, Indic, ...) are preserved for the text the subtitle actually contains, and the font's family name is unchanged.
- Trim as a final step: text added to the subtitle later may display with missing characters, since its glyphs were removed. Re-embed the full font if the text changes.
- Only TrueType fonts with glyf outlines can be trimmed; CFF-based OpenType fonts and font collections (.ttc) are kept unchanged.
- The same trimming is available as a checkbox in the font collector's **Embed fonts in subtitle** and in batch convert's **Embed fonts** function.

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| F1 | Show help |
| Escape | Close dialog |
