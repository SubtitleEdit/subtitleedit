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

The change is `src/AvaloniaEdit/Rendering/VisualLineTextParagraphProperties.cs`,
`Rendering/TextView.cs`, `Rendering/Layer.cs` and `Rendering/VisualLine.cs`:

- Propagate `TextView.FlowDirection` into the line paragraph properties, so Avalonia's
  `TextFormatter` runs the Unicode bidi algorithm and aligns lines; redraw on direction change.
- `TextView`, its layers, and the per-line `VisualLineDrawingVisual` override
  `BypassFlowDirectionPolicies`, opting out of Avalonia's automatic mirror transform for
  `RightToLeft` controls. The formatter's output is normal unmirrored geometry; any mirror
  transform over it (including one on a single line visual) renders reversed text with
  mirror-imaged glyphs.
- New `TextView.TextAlignment` property (default `TextAlignment.Start` = left edge for LTR, right
  edge for RTL; `Center` supported). Applies while word wrap is on; with horizontal scrolling the
  format width is unbounded, so lines stay at the paragraph origin.
- `VisualLine` mouse hit-testing accounts for the line's alignment offset (`TextLine.Start`): the
  "click is behind the line end" checks previously compared against the ink width alone, so any
  click on a right-aligned or centered line counted as past the end; and in an RTL line "past the
  end" is left of the ink, not right of it. `GetVisualColumn` also snaps a trailing character hit
  to the nearer caret boundary, because Avalonia resolves hits on drawable object runs (e.g. the
  atomic tag elements Subtitle Edit uses in RTL mode) with the LTR trailing convention even in
  RTL lines.

**Set `FlowDirection` on the `TextView` itself** (Subtitle Edit does this in
`RightToLeftHelper`/`TextEditorBindingHelper`): setting it on the `TextEditor` or `TextArea` makes
those non-bypassing controls mirror the whole editor, breaking the rendering again. Pixel-level
regression tests live in `tests/UI/Logic/TextViewAlignmentTests.cs` and
`tests/UI/Logic/RightToLeftSelectionTests.cs`.

Formatting tags (`{\an8}`, `</i>`) inside RTL lines are kept literal by Subtitle Edit's
`RtlTagIsolationGenerator` (in `src/ui`, not in this vendored copy): each tag becomes one atomic
`FormattedTextElement`; the tag under the caret opens into plain text so it stays editable. Do
not fix this with Unicode directional marks in the displayed text - their zero-width glyph runs
break Avalonia's `TextLine` geometry APIs (`GetTextBounds`, `GetCharacterHitFromDistance`),
which corrupts selection rendering and mouse hit-testing.

## Other patches

- **IME preedit support** (upstream [PR #532](https://github.com/AvaloniaUI/AvaloniaEdit/pull/532)
  for [issue #524](https://github.com/AvaloniaUI/AvaloniaEdit/issues/524), not merged upstream
  yet): `TextAreaTextInputMethodClient.SupportsPreedit` is true and `SetPreeditText` stores the
  composition text on the `TextArea`; `CaretLayer` draws it inline at the caret, Visual Studio
  style. Without this, Chinese/Japanese/Korean IME composition had no visible preedit text.

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
