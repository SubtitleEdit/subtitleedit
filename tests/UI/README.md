# UI Tests

This project contains unit tests for the UI project, covering areas such as OCR, spell-check, shortcuts, merge/split, find/replace, color services, downloads, undo/redo, and media helpers.

## SpellCheckRegexTests

Tests for the `SpellCheckRegex.Apply` method which applies regex-based spell checking rules to OCR text.

### Test Coverage

The tests cover various OCR correction patterns including:

1. **Bracketed text with 'l' → 'I' replacement** - e.g., `[lTEM]` → `[ITEM]`
2. **Capitalized words with 'I' → 'l'** - e.g., `HelloI` → `Hellol` (when valid)
3. **Possessive apostrophes** - e.g., `David's` → `David's`
4. **Lowercase 'l' → 'I' prefix** - e.g., `lTEM` → `ITEM`
5. **'l' → 'I' suffix** - e.g., `ITEMl` → `ITEMI` (when valid)
6. **'l' → 'I' in middle of capitals** - e.g., `SlNG` → `SING`
7. **Double 'I' → 'll'** - e.g., `chaIIenge` → `challenge`
8. **Single 'I' → 'l' in words** - e.g., `traiIing` → `trailing`
9. **Capital 'I' → 'l' prefix** - e.g., `Iaunch` → `launch`
10. **Missing space after 'of'** - e.g., `ofDavid` → `of David`
11. **Missing space after 'in'** - e.g., `inLondon` → `in London`
12. **Standalone 'l' → 'I'** - e.g., `l` → `I`
13. **Bracketed text with multiple 'l'** - e.g., `[GEARS GRlNDING]` → `[GEARS GRINDING]`

All corrections are only applied if the resulting word is found in the dictionary (via the `isWordSpelledCorrectly` function).

## Running Tests

```bash
dotnet test tests/UI/UITests.csproj
```

Or from the solution root:

```bash
dotnet test
```
