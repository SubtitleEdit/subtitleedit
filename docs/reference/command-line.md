# Command Line

Subtitle Edit includes a command-line tool called **seconv** for batch converting subtitle files without opening the graphical interface.

## Basic Usage

```
seconv <input-file> <output-format> [options]
```

## Examples

Convert a SubRip file to Advanced SubStation Alpha:
```
seconv input.srt AdvancedSubStationAlpha
```

Convert all SRT files in a folder to WebVTT:
```
seconv *.srt WebVTT
```

## Common Output Formats

| Format Name | Typical Extension |
|------------|-------------------|
| SubRip | .srt |
| AdvancedSubStationAlpha | .ass |
| SubStationAlpha | .ssa |
| WebVTT | .vtt |
| MicroDvd | .sub |
| TimedText10 | .ttml |

## Options

Run `seconv` without arguments to see the full list of available options and supported format names.

> **Note:** The command-line converter supports the same subtitle formats as the graphical application. See [Supported Formats](supported-formats.md) for the complete list.
