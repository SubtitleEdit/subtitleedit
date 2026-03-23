# SeConv - Subtitle Converter Command Line Tool

A modern command-line utility for batch converting subtitle files between various formats.

## Features

- ✅ Batch conversion of subtitle files
- ✅ Support for 300+ subtitle formats
- ✅ Colored console output for better readability
- ✅ Extensive subtitle processing operations
- ✅ Modern command-line interface with Spectre.Console

## Installation

Build the project:
```bash
dotnet build src/seconv/SeConv.csproj
```

Or build from the solution root:
```bash
dotnet build
```

The executable will be named `seconv` (or `seconv.exe` on Windows).

## Usage

### Basic Syntax

```bash
seconv <pattern> <format> [options]
```

### Examples

**Convert all SRT files to SAMI format:**
```bash
seconv *.srt sami
```

**Convert with specific encoding:**
```bash
seconv *.srt subrip --encoding:windows-1252
```

**List all available formats:**
```bash
seconv formats
```

## Command Line Options

### File Options
- `--inputfolder:<path>` - Input folder path
- `--outputfolder:<path>` - Output folder path
- `--outputfilename:<name>` - Output file name (for single file only)
- `--overwrite` - Overwrite existing files

### Format Options
- `--encoding:<name>` - Character encoding (e.g., utf-8, windows-1252)
- `--fps:<rate>` - Frame rate for conversion
- `--targetfps:<rate>` - Target frame rate

### Additional Options
- `--adjustduration:<ms>` - Adjust duration in milliseconds
- `--assa-style-file:<file>` - ASSA style file name
- `--ebuheaderfile:<file>` - EBU header file name
- `--forcedonly` - Process forced subtitles only
- `--multiplereplace` - Use default replace rules
- `--multiplereplace:<files>` - Comma separated file name list
- `--ocrengine:<engine>` - OCR engine (tesseract/nOCR)
- `--offset:hh:mm:ss:ms` - Time offset
- `--pac-codepage:<page>` - PAC code page
- `--profile:<name>` - Profile name
- `--renumber:<number>` - Renumber subtitles from this number
- `--resolution:<width>x<height>` - Video resolution (e.g., 1920x1080)
- `--teletextonly` - Process teletext only
- `--teletextonlypage:<page>` - Teletext page number
- `--track-number:<tracks>` - Comma separated track number list

### Processing Operations

Operations are applied in the order specified on the command line:

- `--ApplyDurationLimits` - Apply duration limits
- `--BalanceLines` - Balance line lengths
- `--BeautifyTimeCodes` - Beautify time codes
- `--FixCommonErrors` - Fix common subtitle errors
- `--MergeSameTexts` - Merge entries with same text
- `--MergeSameTimeCodes` - Merge entries with same time codes
- `--RemoveFormatting` - Remove formatting tags
- `--RemoveTextForHI` - Remove text for hearing impaired
- `--SplitLongLines` - Split long lines

And many more...

## Supported Formats

Some popular formats include:
- SubRip (.srt)
- SAMI (.smi)
- Advanced Sub Station Alpha (.ass)
- WebVTT (.vtt)
- Adobe Encore (.txt)
- MicroDVD (.sub)
- Timed Text (.xml)

Run `seconv formats` to see the complete list of 300+ supported formats.

## Getting Help

```bash
seconv --help
seconv /?
```

## Project Structure

```
src/seconv/
├── Commands/
│   ├── ConvertCommand.cs     # Main conversion command
│   └── FormatsCommand.cs     # List available formats
├── Core/
│   ├── SubtitleConverter.cs  # Core conversion logic
│   └── LibSEIntegration.cs   # LibSE integration
├── Helpers/
│   └── HelpDisplay.cs        # Help text display
├── OldCommandLineConverter.cs # Legacy converter support
└── Program.cs                 # Application entry point
```

## Dependencies

- **Spectre.Console** (v0.54.0) - Modern console UI framework
- **Spectre.Console.Cli** (v0.53.1) - Command-line parsing
- **LibSE** - Subtitle Edit core library (project reference)
- **UI** - UI project reference

## Development

The project uses .NET 10.0 and targets modern .NET features.

The executable is named `seconv` (configured via `<AssemblyName>seconv</AssemblyName>` in the .csproj file).

### Legacy Command Support

The tool supports legacy `/parameter` syntax from older versions of SubtitleEdit, automatically converting it to the modern `--parameter` syntax.

To add new features:
1. The conversion logic should be implemented in `Core/SubtitleConverter.cs`
2. Commands can be added to the `Commands/` folder
3. Register new commands in `Program.cs`

## License

Part of the Subtitle Edit Avalonia project.
