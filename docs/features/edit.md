# Edit Menu

The Edit menu provides tools for finding, replacing, and modifying subtitle text and selections.

<!-- Screenshot: Edit menu -->
![Edit Menu](../screenshots/edit-menu.png)

## Undo / Redo

Undo or redo the last editing action.

- **Undo:** `Ctrl+Z`
- **Redo:** `Ctrl+Y`

## Show History

View the complete history of changes made to the subtitle file and restore any previous state.

<!-- Screenshot: Show history window -->
![Show History](../screenshots/show-history.png)

## Find

Search for text in the subtitle.

- **Menu:** Edit → Find
- **Shortcut:** `Ctrl+F`
- **Find next:** `F3`
- **Find previous:** `Shift+F3`

Options:
- Whole word (checkbox)
- Search type (radio buttons): Case sensitive, Case insensitive, or Regular expression

> **Matching a line break with a regular expression:** use `\n` between the words on the two lines (for example `ear\ntwice`). `\r\n` and `\r` are accepted too and are treated the same as `\n`, so a rule works regardless of how it was written or which platform created it.

> **`^` and `$` in Find and Replace** match the start and the end of the whole subtitle text, so in a two-line subtitle `^-` only finds the dash on the first line. Write `(?m)` in front of the pattern - `(?m)^- ` - to make them match at every line break instead. (Multiple replace is the other way around, see below.)

> **Slow patterns:** a pattern that has not finished with a single subtitle after five seconds is given up on and treated as no match for that subtitle, so an expression that backtracks forever - `(a+)+b` and friends - cannot lock up the program.

> **Translator mode:** with an original subtitle loaded, both the text and the original text are searched. Within a line the text is searched first, then the original text, and the match is selected in the text box of the column it was found in.

<!-- Screenshot: Find window -->
![Find](../screenshots/find.png)

## Replace

Find and replace text in the subtitle.

- **Menu:** Edit → Replace
- **Shortcut:** `Ctrl+H`

With an editable original subtitle loaded, a **Replace/search in** drop-down appears with three choices:

- **Text and original text** - both columns (the default, and what Find always does)
- **Text only** - leave the original subtitle alone
- **Original text only** - only change the original subtitle

The choice is remembered between sessions. It also applies to `F3` / `Shift+F3` until the Find window is used again, which always searches both columns. The drop-down is hidden when there is no original subtitle, or when the original is opened as a read-only reference - a read-only original is never written to.

> **Replacement text:** group references work - `$1`, `$&`, `${name}` - and `\n` inserts a line break. Other backslash escapes are not expanded there: `\t` and `\u00A0` are inserted as those literal characters. To insert a special character, paste the character itself into the **Replace with** box. In the search pattern all .NET escapes work as usual, `\u00A0` included.

<!-- Screenshot: Replace window -->
![Replace](../screenshots/replace.png)

## Multiple Replace

Apply multiple find-and-replace rules at once, organized into named categories. Rules are persisted across sessions.

- **Menu:** Edit → Multiple replace

<!-- Screenshot: Multiple replace window -->
![Multiple Replace](../screenshots/multiple-replace.png)

### Window layout

The window is split into two resizable panels:

| Panel | Description |
|---|---|
| **Left — Rules** | Tree of categories and their rules. Each rule shows its type icon, find pattern, replacement, and an optional description. |
| **Right — Fixes** | Preview of all lines that will be changed. The **Before** column highlights removed characters in red and the **After** column highlights added characters in green. Selecting a row reveals an **Applied rules** detail panel at the bottom listing every rule that matched that line. |

### Choosing which fixes to apply

Every line in the preview starts ticked in the **Apply** column, and only ticked lines are changed by **OK** or **Apply**. Untick a line to leave it as it is.

Right-click the preview to tick or untick many rows at once:

- **Select all** (`Ctrl+A`) — tick every row
- **Select none** (`Ctrl+D`) — untick every row, so single rows can be picked
- **Invert selection** (`Ctrl+Shift+I`) — tick the unticked rows and untick the ticked ones

`Space` toggles the checkbox of the rows that are highlighted, so a range selected with Shift+click can be flipped in one go. These gestures work while the preview has focus; the rules tree keeps its own shortcuts below.

Editing a rule regenerates the preview, which ticks every row again.

### Rule types

Each rule has one of three match types, shown as an icon in the tree:

| Type | Description |
|---|---|
| Case insensitive | Plain text match, ignores case |
| Case sensitive | Plain text match, exact case |
| Regular expression | Full .NET regex syntax. Use `\n` to match a line break between two lines (`\r\n` and `\r` are accepted too and treated as `\n`). |

Unlike Find and Replace, regular expression rules here are matched in multiline mode: `^` and `$` match at the start and the end of *every* line, so `^- ` strips the dash from both lines of a two-line subtitle. Put `(?-m)` in front of the pattern to anchor to the whole subtitle text instead. The replacement text follows the same rules as in Replace above - `$1` and `\n` work, other backslash escapes do not.

### Managing categories

Right-click a category node to open its context menu:

- **Edit** — rename the category
- **New category** — add a sibling category
- **New rule** — add a rule to this category
- **Move up / Move down** — reorder categories
- **Delete** — remove the category and all its rules
- **Import** — load rules from a `.template` file (JSON or legacy SE4 XML), a `.csv` file, or a Subtitle Edit 4 `Settings.xml` (its multiple replace groups are imported directly)
- **Export** — save selected categories to a `.template` (JSON) or `.csv` file

### Managing rules

Right-click a rule node to open its context menu:

- **Edit rule** — change find/replace text, type, and description
- **Duplicate** — insert a copy of the rule above the current one
- **Insert before / Insert after** — add a new rule relative to this one
- **Move up / Move down** — reorder within the category
- **Delete** — remove the rule

Double-clicking a rule also opens the **Edit rule** dialog.

### Keyboard shortcuts

| Shortcut | Action |
|---|---|
| `Ctrl+N` | Add a new rule to the selected category, or insert after the selected rule |
| `Ctrl+D` | Duplicate the selected rule |
| `Ctrl+F` | Find a rule by name / text |
| `Ctrl+Shift+-` | Collapse all categories |
| `Ctrl+Shift++` | Expand all categories |
| `Delete` | Delete the selected rule (focus must be in the rules tree) |
| `Space` | Toggle the selected node (category or rule) on/off |
| `Escape` | Close the window |
| `F1` | Open help |

The expand/collapse buttons (`+` / `−`) above the tree expand or collapse all categories at once (`Ctrl+Shift++` / `Ctrl+Shift+-`).

> **Note:** `Ctrl+D` duplicates the selected rule, except while the fixes preview has focus — there it unticks every row, as in the other preview lists (see [Choosing which fixes to apply](#choosing-which-fixes-to-apply)).

### Import / Export

Rule sets are stored as JSON `.template` files and can be shared across installations. The export dialog lets you choose which categories to include. SE4-format XML files can also be imported — both rule files exported from Subtitle Edit 4 and a full Subtitle Edit 4 `Settings.xml` (found in `%AppData%\Subtitle Edit`, or next to `SubtitleEdit.exe` for portable installs), whose multiple replace groups are then imported.

Rules can also be exported to and imported from **CSV** (choose the `.csv` type in the export/import dialog), which is convenient for editing rules in a spreadsheet or sharing them as a simple table.

The CSV has one row per rule with this header:

```csv
Category,Find,ReplaceWith,Description,Active,Type
```

| Column | Description |
|---|---|
| `Category` | Category the rule belongs to (rules with the same name are grouped; empty becomes `Default`) |
| `Find` | Text or pattern to search for |
| `ReplaceWith` | Replacement text (may be empty) |
| `Description` | Optional note |
| `Active` | `true` or `false` — whether the rule is enabled |
| `Type` | `CaseInsensitive`, `CaseSensitive`, or `RegularExpression` |

Values are quoted per RFC 4180, so `Find`/`ReplaceWith` may contain commas, double quotes (written as `""`) and line breaks. The header row is optional on import; unknown `Type` values fall back to `CaseInsensitive`. Files are written as UTF-8 (with BOM) so non-ASCII rules open correctly in spreadsheet apps.

Example:

```csv
Category,Find,ReplaceWith,Description,Active,Type
General,"hello, world","say ""hi""",greeting,true,CaseInsensitive
Regex,\d+,#,strip numbers,true,RegularExpression
```

## Modify Selection

Select or deselect subtitle lines based on rules (e.g., text contains, duration, etc.).

- **Menu:** Edit → Modify selection...

See [Modify Selection](modify-selection.md) for the full list of rules and selection actions.

## Select All

Select all subtitle lines.

- **Shortcut:** `Ctrl+A`

## Inverse Selection

Invert the current selection (select unselected lines, deselect selected ones).

## Toggle Right-to-Left

Toggle right-to-left text direction for languages like Arabic and Hebrew.
