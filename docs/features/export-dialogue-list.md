# Export Excel Dialogue List

Save the subtitle as an Excel spreadsheet (`.xlsx`) with one row per line, for example as a dialogue list for a dubbing studio or for a translator to work in.

- **Menu:** File → Export → Excel dialogue list (.xlsx)...

## How to Use

1. Open **File → Export → Excel dialogue list (.xlsx)...**
2. Tick the columns to include and put them in order with the arrow buttons
3. Choose the time code format and the text options
4. Check the preview on the right. It shows the first 12 rows as they will be saved
5. Click **Save...** and choose a file name

After saving, a prompt lets you open the file or its folder. **Done** closes the window. The columns, their order and the options are remembered for the next export.

## Columns

Number, Start, End, Duration, Gap, Text, Original text, Actor, Style, Layer, Forced, CPS, WPM and Characters.

- The first time, Number, Start, End, Text, Original text and Actor are ticked
- A column with no data in this subtitle (for example Actor when no line has an actor) starts unticked. Your choice for it is kept for subtitles that have the data
- **Original text** can only be ticked when a translation is open with its original
- At least one column must be ticked to save

## Options

- **Time code format**: `hh:mm:ss:ff` (frames, at the current frame rate), `hh:mm:ss,zzz` (milliseconds) or seconds
- **Keep line breaks inside the cell**: keeps a two-line subtitle as two lines in one cell. When off, the lines are joined with " | "
- **Remove formatting**: removes italic, bold, font and other tags from the text
- **Header row**: adds a first row with the column names

To read a spreadsheet back in, see [Import Spreadsheets](import-csv-xlsx.md).
