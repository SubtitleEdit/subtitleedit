# Keyboard Shortcuts

A reference for the default keyboard shortcuts in Subtitle Edit. Every shortcut can be customized in **Options** → **Shortcuts** — and many more actions are available there without a default binding. On macOS, Cmd (`Win` in the shortcut editor) is used wherever Ctrl is shown below.

See also: [Shortcuts Settings](../features/shortcuts.md)

## General

| Shortcut | Action |
|----------|--------|
| Ctrl+N | New subtitle |
| Ctrl+O | Open subtitle file |
| Ctrl+S | Save subtitle |
| Ctrl+Shift+S | Save subtitle as... |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| Ctrl+F | Find |
| F3 | Find next |
| Shift+F3 | Find previous |
| Ctrl+H | Replace |
| Ctrl+Shift+R | Multiple replace |
| Ctrl+G | Go to line number |
| Ctrl+L | Focus selected line |
| Ctrl+Shift+B | Add or edit bookmark |
| Ctrl+Shift+O | Toggle translation mode |
| Ctrl+Shift+Alt+R | Toggle right-to-left |
| Ctrl+4 | Set up like Subtitle Edit 4 (classic theme, icons, waveform, replace rules and shortcuts) |
| Alt+Up | Go to previous line |
| Alt+Down | Go to next line |
| F1 | Show help |
| F2 | Show source view |

## Editing Lines

| Shortcut | Action |
|----------|--------|
| Ctrl+Shift+M | Merge selected lines |
| Alt+Insert | Insert line after |
| Ctrl+Shift+Insert | Insert line before |
| Ctrl+Alt+B | Auto-break text |
| Ctrl+Alt+V | Split line at text box cursor position |
| Ctrl+U | Selected text to lowercase |
| Ctrl+Shift+U | Selected text to uppercase |
| Ctrl+Shift+F3 | Toggle casing |
| Ctrl+Shift+E | Extend selected line to next |
| Alt+Shift+E | Extend selected line to previous |

## Subtitle Grid

| Shortcut | Action |
|----------|--------|
| Delete | Delete selected line(s) |
| Ctrl+A | Select all lines |
| Ctrl+Shift+I | Inverse selection |
| Ctrl+I | Toggle italic on selected lines or text |
| Ctrl+X / Ctrl+C / Ctrl+V | Cut / copy / paste selected lines |
| Ctrl+Shift+V | Fill selected lines with clipboard |
| Ctrl+F8 | List errors |
| F8 / Shift+F8 | Go to next / previous error |
| Alt+F7 | Spell check |

## Source View

Inside the [source view](../features/source-view.md) window (F2), which edits the raw subtitle text. These are fixed, not configurable in Options → Shortcuts.

| Shortcut | Action |
|----------|--------|
| Ctrl+F | Find in the source |
| Ctrl+H | Replace in the source (Cmd+Alt+F also works on macOS) |
| F3 / Shift+F3 | Find next / previous |
| Ctrl+G | Go to line number |
| Alt+Up / Alt+Down | Move the current line or selected block up / down |
| Ctrl+D | Duplicate the current line or selected block |
| Ctrl+Shift+K | Delete the current line |
| Ctrl+Backspace | Delete the word before the caret |
| Ctrl+Delete | Delete the word after the caret |
| Escape | Close the search bar, then the window |

> **Note:** On macOS the word deletes use Option+Backspace / Option+Delete, matching the platform. Alt+Up / Alt+Down are the same everywhere.

## Video Playback

| Shortcut | Action |
|----------|--------|
| Space | Toggle play/pause |
| Ctrl+Space | Toggle play/pause (secondary) |
| Left | One second back |
| Right | One second forward |
| Alt+Left | 500 milliseconds back |
| Alt+Right | 500 milliseconds forward |
| F5 | Play selected lines |
| Alt+Enter | Video full screen |
| Ctrl+Alt+P | Pause |

## Timing

| Shortcut | Action |
|----------|--------|
| F11 | Set start time |
| F12 | Set end time |

"Set end time and go to next line" has no default shortcut: bare F10 activates the
main menu bar (the Windows standard). Assign F10 to the action in Options →
Shortcuts if you prefer the Subtitle Edit 4 behavior — a user-assigned shortcut
wins over the menu activation.

## Waveform

| Shortcut | Action |
|----------|--------|
| Ctrl+V | Paste lines from clipboard at waveform position |
| Shift++ | Vertical zoom in |
| Shift+- | Vertical zoom out |

## Tools

| Shortcut | Action |
|----------|--------|
| Ctrl+Shift+F | Fix common errors |
| Ctrl+Shift+C | Change casing |
| Ctrl+Alt+R | AI review |
| Ctrl+B | Batch convert |
| Ctrl+Shift+H | Remove text for hearing impaired |
| Ctrl+Shift+G | Auto-translate |
| Ctrl+Shift+A | Synchronization → Adjust all times |
| Ctrl+Shift+P | Synchronization → Point sync |
| Ctrl+Shift+D | Find double words |
| Ctrl+Shift+L | Add to name list |
| Ctrl+Alt+Shift+D | Open data folder |
| Ctrl+Alt+Shift+L | Save language file |

> **Note:** Several actions (Bold, Underline, shot-change snapping/extending, green-zone in/out cues, etc.) ship without a default key. Open **Options** → **Shortcuts** to assign them, or use **Import from SE 4.x** to bring over a familiar set.
