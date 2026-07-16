// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// We re-use the CommandBinding and InputBinding instances between multiple text areas,
    /// so this class is static.
    /// </summary>
    internal static class EditingCommandHandler
    {
        /// <summary>
        /// Creates a new <see cref="TextAreaInputHandler"/> for the text area.
        /// </summary>
        public static TextAreaInputHandler Create(TextArea textArea)
        {
            var handler = new TextAreaInputHandler(textArea);
            handler.CommandBindings.AddRange(CommandBindings);
            handler.KeyBindings.AddRange(KeyBindings);
            return handler;
        }

        private static readonly List<RoutedCommandBinding> CommandBindings = new List<RoutedCommandBinding>();
        private static readonly List<KeyBinding> KeyBindings = new List<KeyBinding>();

        private static void AddBinding(RoutedCommand command, KeyModifiers modifiers, Key key,
            EventHandler<ExecutedRoutedEventArgs> handler)
        {
            CommandBindings.Add(new RoutedCommandBinding(command, handler));
            KeyBindings.Add(TextAreaDefaultInputHandler.CreateKeyBinding(command, modifiers, key));
        }

        private static void AddBinding(RoutedCommand command, EventHandler<ExecutedRoutedEventArgs> handler, EventHandler<CanExecuteRoutedEventArgs> canExecuteHandler = null)
        {
            CommandBindings.Add(new RoutedCommandBinding(command, handler, canExecuteHandler));
        }

        static EditingCommandHandler()
        {
            AddBinding(EditingCommands.Delete, KeyModifiers.None, Key.Delete, OnDelete(CaretMovementType.CharRight));
            AddBinding(EditingCommands.DeleteNextWord, KeyModifiers.Control, Key.Delete,
                OnDelete(CaretMovementType.WordRight));
            AddBinding(EditingCommands.Backspace, KeyModifiers.None, Key.Back, OnDelete(CaretMovementType.Backspace));
            KeyBindings.Add(
                TextAreaDefaultInputHandler.CreateKeyBinding(EditingCommands.Backspace, KeyModifiers.Shift,
                    Key.Back)); // make Shift-Backspace do the same as plain backspace
            AddBinding(EditingCommands.DeletePreviousWord, KeyModifiers.Control, Key.Back,
                OnDelete(CaretMovementType.WordLeft));
            AddBinding(EditingCommands.EnterParagraphBreak, KeyModifiers.None, Key.Enter, OnEnter);
            AddBinding(EditingCommands.EnterLineBreak, KeyModifiers.Shift, Key.Enter, OnEnter);

            AddBinding(ApplicationCommands.Delete, OnDelete(CaretMovementType.None), CanDelete);
            AddBinding(ApplicationCommands.Copy, OnCopy, CanCopy);
            AddBinding(ApplicationCommands.Cut, OnCut, CanCut);
            AddBinding(ApplicationCommands.Paste, OnPaste, CanPaste);

            AddBinding(AvaloniaEditCommands.ToggleOverstrike, OnToggleOverstrike);
            AddBinding(AvaloniaEditCommands.DeleteLine, OnDeleteLine);

            AddBinding(AvaloniaEditCommands.RemoveLeadingWhitespace, OnRemoveLeadingWhitespace);
            AddBinding(AvaloniaEditCommands.RemoveTrailingWhitespace, OnRemoveTrailingWhitespace);
            AddBinding(AvaloniaEditCommands.ConvertToUppercase, OnConvertToUpperCase);
            AddBinding(AvaloniaEditCommands.ConvertToLowercase, OnConvertToLowerCase);
            AddBinding(AvaloniaEditCommands.ConvertToTitleCase, OnConvertToTitleCase);
            AddBinding(AvaloniaEditCommands.InvertCase, OnInvertCase);
            AddBinding(AvaloniaEditCommands.ConvertTabsToSpaces, OnConvertTabsToSpaces);
            AddBinding(AvaloniaEditCommands.ConvertSpacesToTabs, OnConvertSpacesToTabs);
            AddBinding(AvaloniaEditCommands.ConvertLeadingTabsToSpaces, OnConvertLeadingTabsToSpaces);
            AddBinding(AvaloniaEditCommands.ConvertLeadingSpacesToTabs, OnConvertLeadingSpacesToTabs);
            AddBinding(AvaloniaEditCommands.IndentSelection, OnIndentSelection);
        }

        private static TextArea GetTextArea(object target)
        {
            return target as TextArea;
        }

        #region Text Transformation Helpers

        private enum DefaultSegmentType
        {
            WholeDocument,
            CurrentLine
        }

        /// <summary>
        /// Calls transformLine on all lines in the selected range.
        /// transformLine needs to handle read-only segments!
        /// </summary>
        private static void TransformSelectedLines(Action<TextArea, DocumentLine> transformLine, object target,
            RoutedEventArgs args, DefaultSegmentType defaultSegmentType)
        {
            var textArea = GetTextArea(target);
            if (textArea?.Document != null)
            {
                using (textArea.Document.RunUpdate())
                {
                    DocumentLine start, end;
                    if (textArea.Selection.IsEmpty)
                    {
                        if (defaultSegmentType == DefaultSegmentType.CurrentLine)
                        {
                            start = end = textArea.Document.GetLineByNumber(textArea.Caret.Line);
                        }
                        else if (defaultSegmentType == DefaultSegmentType.WholeDocument)
                        {
                            start = textArea.Document.Lines[0];
                            end = textArea.Document.Lines[^1];
                        }
                        else
                        {
                            start = end = null;
                        }
                    }
                    else
                    {
                        var segment = textArea.Selection.SurroundingSegment;
                        start = textArea.Document.GetLineByOffset(segment.Offset);
                        end = textArea.Document.GetLineByOffset(segment.EndOffset);
                        // don't include the last line if no characters on it are selected
                        if (start != end && end.Offset == segment.EndOffset)
                            end = end.PreviousLine;
                    }
                    if (start != null)
                    {
                        transformLine(textArea, start);
                        while (start != end)
                        {
                            start = start.NextLine;
                            transformLine(textArea, start);
                        }
                    }
                }
                textArea.Caret.BringCaretToView();
                args.Handled = true;
            }
        }

        /// <summary>
        /// Calls transformLine on all writable segment in the selected range.
        /// </summary>
        private static void TransformSelectedSegments(Action<TextArea, ISegment> transformSegment, object target,
            ExecutedRoutedEventArgs args, DefaultSegmentType defaultSegmentType)
        {
            var textArea = GetTextArea(target);
            if (textArea?.Document != null)
            {
                using (textArea.Document.RunUpdate())
                {
                    IEnumerable<ISegment> segments;
                    if (textArea.Selection.IsEmpty)
                    {
                        if (defaultSegmentType == DefaultSegmentType.CurrentLine)
                        {
                            segments = new ISegment[] { textArea.Document.GetLineByNumber(textArea.Caret.Line) };
                        }
                        else if (defaultSegmentType == DefaultSegmentType.WholeDocument)
                        {
                            segments = textArea.Document.Lines;
                        }
                        else
                        {
                            segments = null;
                        }
                    }
                    else
                    {
                        segments = textArea.Selection.Segments;
                    }
                    if (segments != null)
                    {
                        // Use Enumerable.Reverse explicitly to avoid a breaking change in C# 14 where Reverse() now resolves to MemoryExtensions.Reverse instead of Enumerable.Reverse
                        // see https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/breaking-changes/compiler%20breaking%20changes%20-%20dotnet%2010#enumerablereverse
                        foreach (var segment in System.Linq.Enumerable.Reverse(segments))
                        {
                            foreach (var writableSegment in System.Linq.Enumerable.Reverse(textArea.GetDeletableSegments(segment)))
                            {
                                transformSegment(textArea, writableSegment);
                            }
                        }
                    }
                }
                textArea.Caret.BringCaretToView();
                args.Handled = true;
            }
        }

        #endregion

        #region EnterLineBreak

        private static void OnEnter(object target, ExecutedRoutedEventArgs args)
        {
            var textArea = GetTextArea(target);
            if (textArea != null && textArea.IsFocused)
            {
                textArea.PerformTextInput("\n");
                args.Handled = true;
            }
        }

        #endregion

        #region Tab

        public static void OnTab(object target, RoutedEventArgs args)
        {
            var textArea = GetTextArea(target);
            if (textArea?.Document != null)
            {
                using (textArea.Document.RunUpdate())
                {
                    if (textArea.Selection.IsMultiline)
                    {
                        var segment = textArea.Selection.SurroundingSegment;
                        var start = textArea.Document.GetLineByOffset(segment.Offset);
                        var end = textArea.Document.GetLineByOffset(segment.EndOffset);
                        // don't include the last line if no characters on it are selected
                        if (start != end && end.Offset == segment.EndOffset)
                            end = end.PreviousLine;
                        var current = start;
                        while (true)
                        {
                            var offset = current.Offset;
                            if (textArea.ReadOnlySectionProvider.CanInsert(offset))
                                textArea.Document.Replace(offset, 0, textArea.Options.IndentationString,
                                    OffsetChangeMappingType.KeepAnchorBeforeInsertion);
                            if (current == end)
                                break;
                            current = current.NextLine;
                        }
                    }
                    else
                    {
                        var indentationString = textArea.Options.GetIndentationString(textArea.Caret.Column);
                        textArea.ReplaceSelectionWithText(indentationString);
                    }
                }
                textArea.Caret.BringCaretToView();
                args.Handled = true;
            }
        }

        public static void OnShiftTab(object target, RoutedEventArgs args)
        {
            TransformSelectedLines(
                delegate (TextArea textArea, DocumentLine line)
                {
                    var offset = line.Offset;
                    var s = TextUtilities.GetSingleIndentationSegment(textArea.Document, offset,
                        textArea.Options.IndentationSize);
                    if (s.Length > 0)
                    {
                        s = textArea.GetDeletableSegments(s).FirstOrDefault();
                        if (s != null && s.Length > 0)
                        {
                            textArea.Document.Remove(s.Offset, s.Length);
                        }
                    }
                }, target, args, DefaultSegmentType.CurrentLine);
        }

        #endregion

        #region Delete

        private static EventHandler<ExecutedRoutedEventArgs> OnDelete(CaretMovementType caretMovement)
        {
            return (target, args) =>
            {
                var textArea = GetTextArea(target);
                if (textArea?.Document != null)
                {
                    if (textArea.Selection.IsEmpty)
                    {
                        var startPos = textArea.Caret.Position;
                        var enableVirtualSpace = textArea.Options.EnableVirtualSpace;
                        // When pressing delete; don't move the caret further into virtual space - instead delete the newline
                        if (caretMovement == CaretMovementType.CharRight)
                            enableVirtualSpace = false;
                        var desiredXPos = textArea.Caret.DesiredXPos;
                        var endPos = CaretNavigationCommandHandler.GetNewCaretPosition(
                            textArea.TextView, startPos, caretMovement, enableVirtualSpace, ref desiredXPos);
                        // GetNewCaretPosition may return (0,0) as new position,
                        // thus we need to validate endPos before using it in the selection.
                        if (endPos.Line < 1 || endPos.Column < 1)
                            endPos = new TextViewPosition(Math.Max(endPos.Line, 1), Math.Max(endPos.Column, 1));
                        // Don't do anything if the number of lines of a rectangular selection would be changed by the deletion.
                        if (textArea.Selection is RectangleSelection && startPos.Line != endPos.Line)
                            return;
                        // Don't select the text to be deleted; just reuse the ReplaceSelectionWithText logic
                        // Reuse the existing selection, so that we continue using the same logic
                        textArea.Selection.StartSelectionOrSetEndpoint(startPos, endPos)
                            .ReplaceSelectionWithText(string.Empty);
                    }
                    else
                    {
                        textArea.RemoveSelectedText();
                    }
                    textArea.Caret.BringCaretToView();
                    args.Handled = true;
                }
            };
        }

        private static void CanDelete(object target, CanExecuteRoutedEventArgs args)
        {
            var textArea = GetTextArea(target);
            if (textArea is { Document: not null })
            {
                args.CanExecute = true;
                args.Handled = true;
            }
        }

        #endregion

        #region Clipboard commands

        private static void CanCut(object target, CanExecuteRoutedEventArgs args)
        {
            // HasSomethingSelected for copy and cut commands
            var textArea = GetTextArea(target);
            if (textArea is { Document: not null })
            {
                args.CanExecute = (textArea.Options.CutCopyWholeLine || !textArea.Selection.IsEmpty) && !textArea.IsReadOnly;
                args.Handled = true;
            }
        }

        private static void CanCopy(object target, CanExecuteRoutedEventArgs args)
        {
            // HasSomethingSelected for copy and cut commands
            var textArea = GetTextArea(target);
            if (textArea is { Document: not null })
            {
                args.CanExecute = textArea.Options.CutCopyWholeLine || !textArea.Selection.IsEmpty;
                args.Handled = true;
            }
        }

        private static void OnCopy(object target, ExecutedRoutedEventArgs args)
        {
            var textArea = GetTextArea(target);
            if (textArea?.Document != null)
            {
                if (textArea.Selection.IsEmpty && textArea.Options.CutCopyWholeLine)
                {
                    var currentLine = textArea.Document.GetLineByNumber(textArea.Caret.Line);
                    CopyWholeLine(textArea, currentLine);
                }
                else
                {
                    CopySelectedText(textArea);
                }
                args.Handled = true;
            }
        }

        private static void OnCut(object target, ExecutedRoutedEventArgs args)
        {
            var textArea = GetTextArea(target);
            if (textArea?.Document != null)
            {
                if (textArea.Selection.IsEmpty && textArea.Options.CutCopyWholeLine)
                {
                    var currentLine = textArea.Document.GetLineByNumber(textArea.Caret.Line);
                    if (CopyWholeLine(textArea, currentLine))
                    {
                        var segmentsToDelete =
                            textArea.GetDeletableSegments(
                                new SimpleSegment(currentLine.Offset, currentLine.TotalLength));
                        for (var i = segmentsToDelete.Length - 1; i >= 0; i--)
                        {
                            textArea.Document.Remove(segmentsToDelete[i]);
                        }
                    }
                }
                else
                {
                    if (CopySelectedText(textArea))
                        textArea.RemoveSelectedText();
                }
                textArea.Caret.BringCaretToView();
                args.Handled = true;
            }
        }

        private static bool CopySelectedText(TextArea textArea)
        {
            var text = textArea.Selection.GetText();
            text = TextUtilities.NormalizeNewLines(text, Environment.NewLine);


            using var df = new DataTransfer();
            var item = new DataTransferItem();
            item.Set(DataFormat.Text, text);
            df.Add(item);
            SetClipboardText(df, textArea);

            textArea.OnTextCopied(new TextEventArgs(text));
            return true;
        }

        public static bool ConfirmDataFormat<T>(TextArea textArea, DataTransfer dataObject, DataFormat<T> format) where T : class
        {
            return true;
            ////var e = new DataObjectSettingDataEventArgs(dataObject, format);
            ////textArea.RaiseEvent(e);
            ////return !e.CommandCancelled;
        }

        private static void SetClipboardText(DataTransfer text, Visual visual)
        {
            try
            {
                TopLevel.GetTopLevel(visual)?.Clipboard?.SetDataAsync(text);
            }
            catch (Exception)
            {
                // Apparently this exception sometimes happens randomly.
                // The MS controls just ignore it, so we'll do the same.
            }
        }

        private static bool CopyWholeLine(TextArea textArea, DocumentLine line)
        {
            ISegment wholeLine = new SimpleSegment(line.Offset, line.TotalLength);
            var text = textArea.Document.GetText(wholeLine);
            // Ignore empty line copy
            if (string.IsNullOrEmpty(text)) return false;
            // Ensure we use the appropriate newline sequence for the OS
            text = TextUtilities.NormalizeNewLines(text, Environment.NewLine);

            // TODO: formats
            //DataObject data = new DataObject();
            //if (ConfirmDataFormat(textArea, data, DataFormats.UnicodeText))
            //    data.SetText(text);

            //// Also copy text in HTML format to clipboard - good for pasting text into Word
            //// or to the SharpDevelop forums.
            //if (ConfirmDataFormat(textArea, data, DataFormats.Html))
            //{
            //    IHighlighter highlighter = textArea.GetService(typeof(IHighlighter)) as IHighlighter;
            //    HtmlClipboard.SetHtml(data,
            //        HtmlClipboard.CreateHtmlFragment(textArea.Document, highlighter, wholeLine,
            //            new HtmlOptions(textArea.Options)));
            //}

            //if (ConfirmDataFormat(textArea, data, LineSelectedType))
            //{
            //    var lineSelected = new MemoryStream(1);
            //    lineSelected.WriteByte(1);
            //    data.SetData(LineSelectedType, lineSelected, false);
            //}

            //var copyingEventArgs = new DataObjectCopyingEventArgs(data, false);
            //textArea.RaiseEvent(copyingEventArgs);
            //if (copyingEventArgs.CommandCancelled)
            //    return false;
            using var df = new DataTransfer();
            var item = new DataTransferItem();
            item.Set(DataFormat.Text, text);
            df.Add(item);
            SetClipboardText(df, textArea);

            textArea.OnTextCopied(new TextEventArgs(text));
            return true;
        }

        private static void CanPaste(object target, CanExecuteRoutedEventArgs args)
        {
            var textArea = GetTextArea(target);
            if (textArea is { Document: not null })
            {
                args.CanExecute = textArea.ReadOnlySectionProvider.CanInsert(textArea.Caret.Offset);
                args.Handled = true;
            }
        }

        private static async void OnPaste(object target, ExecutedRoutedEventArgs args)
        {
            var textArea = GetTextArea(target);
            if (textArea?.Document != null)
            {
                textArea.Document.BeginUpdate();

                string text = null;
                try
                {
                    var data = await TopLevel.GetTopLevel(textArea)?.Clipboard?.TryGetDataAsync();
                    text = await data.TryGetTextAsync();
                }
                catch (Exception)
                {
                    textArea.Document.EndUpdate();
                    return;
                }

                if (text == null)
                {
                    textArea.Document.EndUpdate();
                    return;
                }


                text = GetTextToPaste(text, textArea);

                if (!string.IsNullOrEmpty(text))
                {
                    textArea.ReplaceSelectionWithText(text);
                }

                textArea.Caret.BringCaretToView();
                args.Handled = true;

                textArea.Document.EndUpdate();

                textArea.OnTextPasted(new TextEventArgs(text));
            }
        }

        internal static string GetTextToPaste(IDataTransfer dataObject, TextArea textArea)
        {
            if (dataObject.Contains(DataFormat.Text))
            {
                return GetTextToPaste((string)dataObject.TryGetText() ?? "", textArea);
            }

            return null;
        }

        internal static string GetTextToPaste(string text, TextArea textArea)
        {
            try
            {
                // Try retrieving the text as one of:
                //  - the FormatToApply
                //  - UnicodeText
                //  - Text
                // (but don't try the same format twice)
                //if (pastingEventArgs.FormatToApply != null && dataObject.GetDataPresent(pastingEventArgs.FormatToApply))
                //    text = (string)dataObject.GetData(pastingEventArgs.FormatToApply);
                //else if (pastingEventArgs.FormatToApply != DataFormats.UnicodeText &&
                //         dataObject.GetDataPresent(DataFormats.UnicodeText))
                //    text = (string)dataObject.GetData(DataFormats.UnicodeText);
                //else if (pastingEventArgs.FormatToApply != DataFormats.Text &&
                //         dataObject.GetDataPresent(DataFormats.Text))
                //    text = (string)dataObject.GetData(DataFormats.Text);
                //else
                //    return null; // no text data format
                // convert text back to correct newlines for this document
                var newLine = TextUtilities.GetNewLineFromDocument(textArea.Document, textArea.Caret.Line);
                text = TextUtilities.NormalizeNewLines(text, newLine);
                text = textArea.Options.ConvertTabsToSpaces
                    ? text.Replace("\t", new String(' ', textArea.Options.IndentationSize))
                    : text;
                return text;
            }
            catch (OutOfMemoryException)
            {
                // may happen when trying to paste a huge string
                return null;
            }
        }

        #endregion

        #region Toggle Overstrike

        private static void OnToggleOverstrike(object target, ExecutedRoutedEventArgs args)
        {
            var textArea = GetTextArea(target);
            if (textArea != null && textArea.Options.AllowToggleOverstrikeMode)
                textArea.OverstrikeMode = !textArea.OverstrikeMode;
        }

        #endregion

        #region DeleteLine

        private static void OnDeleteLine(object target, ExecutedRoutedEventArgs args)
        {
            var textArea = GetTextArea(target);
            if (textArea?.Document != null)
            {
                int firstLineIndex, lastLineIndex;
                if (textArea.Selection.Length == 0)
                {
                    // There is no selection, simply delete current line
                    firstLineIndex = lastLineIndex = textArea.Caret.Line;
                }
                else
                {
                    // There is a selection, remove all lines affected by it (use Min/Max to be independent from selection direction)
                    firstLineIndex = Math.Min(textArea.Selection.StartPosition.Line,
                        textArea.Selection.EndPosition.Line);
                    lastLineIndex = Math.Max(textArea.Selection.StartPosition.Line,
                        textArea.Selection.EndPosition.Line);
                }
                var startLine = textArea.Document.GetLineByNumber(firstLineIndex);
                var endLine = textArea.Document.GetLineByNumber(lastLineIndex);
                textArea.Selection = Selection.Create(textArea, startLine.Offset,
                    endLine.Offset + endLine.TotalLength);
                textArea.RemoveSelectedText();
                args.Handled = true;
            }
        }

        #endregion

        #region Remove..Whitespace / Convert Tabs-Spaces

        private static void OnRemoveLeadingWhitespace(object target, ExecutedRoutedEventArgs args)
        {
            TransformSelectedLines(
                delegate (TextArea textArea, DocumentLine line)
                {
                    textArea.Document.Remove(TextUtilities.GetLeadingWhitespace(textArea.Document, line));
                }, target, args, DefaultSegmentType.WholeDocument);
        }

        private static void OnRemoveTrailingWhitespace(object target, ExecutedRoutedEventArgs args)
        {
            TransformSelectedLines(
                delegate (TextArea textArea, DocumentLine line)
                {
                    textArea.Document.Remove(TextUtilities.GetTrailingWhitespace(textArea.Document, line));
                }, target, args, DefaultSegmentType.WholeDocument);
        }

        private static void OnConvertTabsToSpaces(object target, ExecutedRoutedEventArgs args)
        {
            TransformSelectedSegments(ConvertTabsToSpaces, target, args, DefaultSegmentType.WholeDocument);
        }

        private static void OnConvertLeadingTabsToSpaces(object target, ExecutedRoutedEventArgs args)
        {
            TransformSelectedLines(
                delegate (TextArea textArea, DocumentLine line)
                {
                    ConvertTabsToSpaces(textArea, TextUtilities.GetLeadingWhitespace(textArea.Document, line));
                }, target, args, DefaultSegmentType.WholeDocument);
        }

        private static void ConvertTabsToSpaces(TextArea textArea, ISegment segment)
        {
            var document = textArea.Document;
            var endOffset = segment.EndOffset;
            var indentationString = new string(' ', textArea.Options.IndentationSize);
            for (var offset = segment.Offset; offset < endOffset; offset++)
            {
                if (document.GetCharAt(offset) == '\t')
                {
                    document.Replace(offset, 1, indentationString, OffsetChangeMappingType.CharacterReplace);
                    endOffset += indentationString.Length - 1;
                }
            }
        }

        private static void OnConvertSpacesToTabs(object target, ExecutedRoutedEventArgs args)
        {
            TransformSelectedSegments(ConvertSpacesToTabs, target, args, DefaultSegmentType.WholeDocument);
        }

        private static void OnConvertLeadingSpacesToTabs(object target, ExecutedRoutedEventArgs args)
        {
            TransformSelectedLines(
                delegate (TextArea textArea, DocumentLine line)
                {
                    ConvertSpacesToTabs(textArea, TextUtilities.GetLeadingWhitespace(textArea.Document, line));
                }, target, args, DefaultSegmentType.WholeDocument);
        }

        private static void ConvertSpacesToTabs(TextArea textArea, ISegment segment)
        {
            var document = textArea.Document;
            var endOffset = segment.EndOffset;
            var indentationSize = textArea.Options.IndentationSize;
            var spacesCount = 0;
            for (var offset = segment.Offset; offset < endOffset; offset++)
            {
                if (document.GetCharAt(offset) == ' ')
                {
                    spacesCount++;
                    if (spacesCount == indentationSize)
                    {
                        document.Replace(offset - (indentationSize - 1), indentationSize, "\t",
                            OffsetChangeMappingType.CharacterReplace);
                        spacesCount = 0;
                        offset -= indentationSize - 1;
                        endOffset -= indentationSize - 1;
                    }
                }
                else
                {
                    spacesCount = 0;
                }
            }
        }

        #endregion

        #region Convert...Case

        private static void ConvertCase(Func<string, string> transformText, object target, ExecutedRoutedEventArgs args)
        {
            TransformSelectedSegments(
                delegate (TextArea textArea, ISegment segment)
                {
                    var oldText = textArea.Document.GetText(segment);
                    var newText = transformText(oldText);
                    textArea.Document.Replace(segment.Offset, segment.Length, newText,
                        OffsetChangeMappingType.CharacterReplace);
                }, target, args, DefaultSegmentType.WholeDocument);
        }

        private static void OnConvertToUpperCase(object target, ExecutedRoutedEventArgs args)
        {
            ConvertCase(CultureInfo.CurrentCulture.TextInfo.ToUpper, target, args);
        }

        private static void OnConvertToLowerCase(object target, ExecutedRoutedEventArgs args)
        {
            ConvertCase(CultureInfo.CurrentCulture.TextInfo.ToLower, target, args);
        }

        private static void OnConvertToTitleCase(object target, ExecutedRoutedEventArgs args)
        {
            throw new NotSupportedException();
            //ConvertCase(CultureInfo.CurrentCulture.TextInfo.ToTitleCase, target, args);
        }

        private static void OnInvertCase(object target, ExecutedRoutedEventArgs args)
        {
            ConvertCase(InvertCase, target, args);
        }

        private static string InvertCase(string text)
        {
            // TODO: culture
            //var culture = CultureInfo.CurrentCulture;
            var buffer = text.ToCharArray();
            for (var i = 0; i < buffer.Length; ++i)
            {
                var c = buffer[i];
                buffer[i] = char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c);
            }
            return new string(buffer);
        }

        #endregion

        private static void OnIndentSelection(object target, ExecutedRoutedEventArgs args)
        {
            var textArea = GetTextArea(target);
            if (textArea?.Document != null && textArea.IndentationStrategy != null)
            {
                using (textArea.Document.RunUpdate())
                {
                    int start, end;
                    if (textArea.Selection.IsEmpty)
                    {
                        start = 1;
                        end = textArea.Document.LineCount;
                    }
                    else
                    {
                        start = textArea.Document.GetLineByOffset(textArea.Selection.SurroundingSegment.Offset)
                            .LineNumber;
                        end = textArea.Document.GetLineByOffset(textArea.Selection.SurroundingSegment.EndOffset)
                            .LineNumber;
                    }
                    textArea.IndentationStrategy.IndentLines(textArea.Document, start, end);
                }
                textArea.Caret.BringCaretToView();
                args.Handled = true;
            }
        }
    }
}
