# Vendored AvaloniaEdit

This is a source copy of [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) (the Avalonia
port of AvalonEdit), vendored into Subtitle Edit rather than consumed as the
`Avalonia.AvaloniaEdit` NuGet package.

## Why it's here

It carries a **RTL rendering fix** that isn't in an upstream release yet: the line layout now
honors the control's `FlowDirection`, so right-to-left text (Arabic, Hebrew, …) renders in correct
reading and glyph order instead of mirror-reversed. Previously `VisualLineTextParagraphProperties`
hardcoded `FlowDirection.LeftToRight` while the framework applied its automatic mirror transform for
a `RightToLeft` control, so the text came out reversed (Subtitle Edit #12386 / #12434, upstream
[AvaloniaEdit#401](https://github.com/AvaloniaUI/AvaloniaEdit/issues/401)).

The change is `src/AvaloniaEdit/Rendering/VisualLineTextParagraphProperties.cs` and
`Rendering/TextView.cs` — propagate `TextView.FlowDirection` into the paragraph properties (so
Avalonia's `TextFormatter` runs the Unicode bidi algorithm), right-align RTL lines, and redraw on
direction change. This is the **rendering** fix only; RTL caret movement and selection are follow-up.

## Consuming it

The assembly names and namespaces are unchanged (`AvaloniaEdit`, `AvaloniaEdit.TextMate`), so SE
code uses it with the usual `using AvaloniaEdit;` — the UI project just references these two projects
instead of the NuGet packages.

## Updating

To re-sync with upstream, replace the source under `AvaloniaEdit/` and `AvaloniaEdit.TextMate/`
(keeping the vendored `.csproj` files, which are trimmed to build inside SE) and re-apply the RTL
change if upstream hasn't merged it. Fork with the patch: https://github.com/niksedk/AvaloniaEdit
(branch `feature/rtl-rendering-prototype`). Upstreaming the fix would let this vendored copy be
dropped in favor of a future package release.
