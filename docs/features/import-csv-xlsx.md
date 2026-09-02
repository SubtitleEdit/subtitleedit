# Import Spreadsheets (CSV / XLSX / ODS)

Turn a spreadsheet or delimited text file into subtitle lines. Excel workbooks (`.xlsx`), OpenDocument spreadsheets (`.ods`) and delimited text (`.csv`, `.tsv`, `.txt`) are read directly — no conversion step, and no add-ins required.

There are two ways in:

- **Just open the file** — Subtitle Edit recognises a spreadsheet with a subtitle-like header row and imports it automatically
- **File → Import → CSV/XLSX/ODS with custom columns...** — a window where you say which column is which, with a live preview

- **Menu:** File → Import → CSV/XLSX/ODS with custom columns...
- **Shortcut:** Configurable (no default)

## Opening a Spreadsheet Directly

Works for `.csv`, `.xlsx` and `.ods`. Drag the file onto Subtitle Edit, or use **File → Open** and pick the **Spreadsheet files** type in the file dialog (the default *Subtitle files* filter only lists subtitle formats).

For this to work the file has to be self-describing:

- **Row 1 must be a header row**, and at least **two** of its cells must be column names Subtitle Edit recognises (see below). If fewer match, the automatic import is skipped and the file is parsed as an ordinary unknown text file — use *Import with custom columns* instead
- One row per subtitle, below the header
- For `.csv` the separator is detected automatically: comma, semicolon or tab. Values may be quoted per RFC 4180, so text can contain the separator, `""` for a literal quote, and line breaks
- Only the **first worksheet** of an `.xlsx`/`.ods` file is read. Cell formatting, colours and merged cells are ignored — only the text matters

### Recognised Column Names

Names are matched case-insensitively.

| Role | Header names |
|------|--------------|
| Start | `start`, `start time`, `starttime`, `start_time`, `in`, `begin`, `from`, `fromtime`, `show`, `timecode`, `tc-in`, `tc in`, `start tc`, `tc start`, plus millisecond variants like `startms`, `start_ms`, `startmillis`, `startmilliseconds` |
| End | `end`, `end time`, `endtime`, `end_time`, `out`, `stop`, `to`, `totime`, `hide`, `tc-out`, `tc out`, `end tc`, `tc end`, plus `endms`, `end_ms`, `endmillis`, `endmilliseconds` |
| Duration | `duration`, `dur`, `durationms` |
| Text | `text`, `content`, `value`, `caption`, `sentence`, `dialog`, `dialogue` |
| Actor / speaker | `speaker`, `voice`, `character`, `character name`, `role`, `name`, `actor`, `rolle`, `sprecher` |

A minimal file that imports cleanly:

```csv
Start,End,Text
00:00:01.000,00:00:03.500,"Hello there."
00:00:04.000,00:00:06.000,"General Kenobi!"
```

### Time Code Formats

Each time column is examined as a whole and interpreted as one of:

- **Time codes** — `hh:mm:ss.ms` / `hh:mm:ss,ms` (also `mm:ss.ms`)
- **Frames** — `hh:mm:ss:ff`, used when *every* value in the column has four parts with a two-digit last part. The current frame rate is used to convert
- **Milliseconds** — a plain number, when the column name ends in `ms`, `millis` or `milliseconds`

If there is a duration column but no end column, the end time is calculated as start + duration.

## Import with Custom Columns

Use this window when the file has extra columns, an unusual column order, header names Subtitle Edit does not know, or no header row at all.

1. Click **Open file** and pick a `.csv`, `.tsv`, `.txt`, `.xlsx` or `.ods` file. The detected separator is shown next to the file name
2. The upper grid shows the file as it was read. Each column header has a drop-down where you pick its role: **Show** (start time), **Hide** (end time), **Duration**, **Text**, **Character** (actor) or **None**. Columns whose header name is recognised are pre-selected for you
3. The lower grid previews the resulting subtitle, updating as you change the roles
4. **OK** loads the preview as the current subtitle

Notes:

- Every role can be used by at most one column — assigning a role that is already taken clears it from the other column
- Only **Text** is required; **OK** stays disabled until a text column is picked. Rows with an empty text cell are skipped
- When the first row does not look like a header it is kept as data and the columns are named *Column 1*, *Column 2*, ...
- **OK replaces the currently loaded subtitle**, so save your work first if you need it

## See Also

- [File Menu](file.md) — the rest of the Import menu
- [Import Plain Text](import-plain-text.md) — for text without time codes
