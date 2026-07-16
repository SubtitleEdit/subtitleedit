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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Avalonia.Input;

namespace AvaloniaEdit
{
    /// <summary>
    /// Custom commands for AvalonEdit.
    /// </summary>
    public static class AvaloniaEditCommands
    {
        /// <summary>
        /// Toggles Overstrike mode
        /// The default shortcut is Ins.
        /// </summary>
        public static RoutedCommand ToggleOverstrike { get; } = new RoutedCommand(nameof(ToggleOverstrike), new KeyGesture(Key.Insert));

        /// <summary>
        /// Deletes the current line.
        /// The default shortcut is Ctrl+D.
        /// </summary>
        public static RoutedCommand DeleteLine { get; } = new RoutedCommand(nameof(DeleteLine), new KeyGesture(Key.D, KeyModifiers.Control));

        /// <summary>
        /// Removes leading whitespace from the selected lines (or the whole document if the selection is empty).
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Whitespace")]
        public static RoutedCommand RemoveLeadingWhitespace { get; } = new RoutedCommand(nameof(RemoveLeadingWhitespace));

        /// <summary>
        /// Removes trailing whitespace from the selected lines (or the whole document if the selection is empty).
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Whitespace")]
        public static RoutedCommand RemoveTrailingWhitespace { get; } = new RoutedCommand(nameof(RemoveTrailingWhitespace));

        /// <summary>
        /// Converts the selected text to upper case.
        /// </summary>
        public static RoutedCommand ConvertToUppercase { get; } = new RoutedCommand(nameof(ConvertToUppercase));

        /// <summary>
        /// Converts the selected text to lower case.
        /// </summary>
        public static RoutedCommand ConvertToLowercase { get; } = new RoutedCommand(nameof(ConvertToLowercase));

        /// <summary>
        /// Converts the selected text to title case.
        /// </summary>
        public static RoutedCommand ConvertToTitleCase { get; } = new RoutedCommand(nameof(ConvertToTitleCase));

        /// <summary>
        /// Inverts the case of the selected text.
        /// </summary>
        public static RoutedCommand InvertCase { get; } = new RoutedCommand(nameof(InvertCase));

        /// <summary>
        /// Converts tabs to spaces in the selected text.
        /// </summary>
        public static RoutedCommand ConvertTabsToSpaces { get; } = new RoutedCommand(nameof(ConvertTabsToSpaces));

        /// <summary>
        /// Converts spaces to tabs in the selected text.
        /// </summary>
        public static RoutedCommand ConvertSpacesToTabs { get; } = new RoutedCommand(nameof(ConvertSpacesToTabs));

        /// <summary>
        /// Converts leading tabs to spaces in the selected lines (or the whole document if the selection is empty).
        /// </summary>
        public static RoutedCommand ConvertLeadingTabsToSpaces { get; } = new RoutedCommand(nameof(ConvertLeadingTabsToSpaces));

        /// <summary>
        /// Converts leading spaces to tabs in the selected lines (or the whole document if the selection is empty).
        /// </summary>
        public static RoutedCommand ConvertLeadingSpacesToTabs { get; } = new RoutedCommand(nameof(ConvertLeadingSpacesToTabs));

        /// <summary>InputModifiers
        /// Runs the IIndentationStrategy on the selected lines (or the whole document if the selection is empty).
        /// </summary>
        public static RoutedCommand IndentSelection { get; } = new RoutedCommand(nameof(IndentSelection), new KeyGesture(Key.I, KeyModifiers.Control));
    }


    public static class ApplicationCommands
    {
        private static readonly KeyModifiers PlatformCommandKey = GetPlatformCommandKey();

        public static RoutedCommand Delete { get; } = new RoutedCommand(nameof(Delete), new KeyGesture(Key.Delete));
        public static RoutedCommand Copy { get; } = new RoutedCommand(nameof(Copy), new KeyGesture(Key.C, PlatformCommandKey));
        public static RoutedCommand Cut { get; } = new RoutedCommand(nameof(Cut), new KeyGesture(Key.X, PlatformCommandKey));
        public static RoutedCommand Paste { get; } = new RoutedCommand(nameof(Paste), new KeyGesture(Key.V, PlatformCommandKey));
        public static RoutedCommand SelectAll { get; } = new RoutedCommand(nameof(SelectAll), new KeyGesture(Key.A, PlatformCommandKey));
        public static RoutedCommand Undo { get; } = new RoutedCommand(nameof(Undo), new KeyGesture(Key.Z, PlatformCommandKey));
        public static RoutedCommand Redo { get; } = new RoutedCommand(nameof(Redo), new KeyGesture(Key.Y, PlatformCommandKey));
        public static RoutedCommand Find { get; } = new RoutedCommand(nameof(Find), new KeyGesture(Key.F, PlatformCommandKey));
        public static RoutedCommand Replace { get; } = new RoutedCommand(nameof(Replace), GetReplaceKeyGesture());
        
        private static KeyModifiers GetPlatformCommandKey()
        {            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return KeyModifiers.Meta;
            }

            return KeyModifiers.Control;
        }

        private static KeyGesture GetReplaceKeyGesture()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new KeyGesture(Key.F, KeyModifiers.Meta | KeyModifiers.Alt);
            }

            return new KeyGesture(Key.H, PlatformCommandKey);
        }
    }

    public static class EditingCommands
    {
        public static RoutedCommand Delete { get; } = new RoutedCommand(nameof(Delete));
        public static RoutedCommand DeleteNextWord { get; } = new RoutedCommand(nameof(DeleteNextWord));
        public static RoutedCommand Backspace { get; } = new RoutedCommand(nameof(Backspace));
        public static RoutedCommand DeletePreviousWord { get; } = new RoutedCommand(nameof(DeletePreviousWord));
        public static RoutedCommand EnterParagraphBreak { get; } = new RoutedCommand(nameof(EnterParagraphBreak));
        public static RoutedCommand EnterLineBreak { get; } = new RoutedCommand(nameof(EnterLineBreak));
        public static RoutedCommand TabForward { get; } = new RoutedCommand(nameof(TabForward));
        public static RoutedCommand TabBackward { get; } = new RoutedCommand(nameof(TabBackward));
        public static RoutedCommand MoveLeftByCharacter { get; } = new RoutedCommand(nameof(MoveLeftByCharacter));
        public static RoutedCommand SelectLeftByCharacter { get; } = new RoutedCommand(nameof(SelectLeftByCharacter));
        public static RoutedCommand MoveRightByCharacter { get; } = new RoutedCommand(nameof(MoveRightByCharacter));
        public static RoutedCommand SelectRightByCharacter { get; } = new RoutedCommand(nameof(SelectRightByCharacter));
        public static RoutedCommand MoveLeftByWord { get; } = new RoutedCommand(nameof(MoveLeftByWord));
        public static RoutedCommand SelectLeftByWord { get; } = new RoutedCommand(nameof(SelectLeftByWord));
        public static RoutedCommand MoveRightByWord { get; } = new RoutedCommand(nameof(MoveRightByWord));
        public static RoutedCommand SelectRightByWord { get; } = new RoutedCommand(nameof(SelectRightByWord));
        public static RoutedCommand MoveUpByLine { get; } = new RoutedCommand(nameof(MoveUpByLine));
        public static RoutedCommand SelectUpByLine { get; } = new RoutedCommand(nameof(SelectUpByLine));
        public static RoutedCommand MoveDownByLine { get; } = new RoutedCommand(nameof(MoveDownByLine));
        public static RoutedCommand SelectDownByLine { get; } = new RoutedCommand(nameof(SelectDownByLine));
        public static RoutedCommand MoveDownByPage { get; } = new RoutedCommand(nameof(MoveDownByPage));
        public static RoutedCommand SelectDownByPage { get; } = new RoutedCommand(nameof(SelectDownByPage));
        public static RoutedCommand MoveUpByPage { get; } = new RoutedCommand(nameof(MoveUpByPage));
        public static RoutedCommand SelectUpByPage { get; } = new RoutedCommand(nameof(SelectUpByPage));
        public static RoutedCommand MoveToLineStart { get; } = new RoutedCommand(nameof(MoveToLineStart));
        public static RoutedCommand SelectToLineStart { get; } = new RoutedCommand(nameof(SelectToLineStart));
        public static RoutedCommand MoveToLineEnd { get; } = new RoutedCommand(nameof(MoveToLineEnd));
        public static RoutedCommand SelectToLineEnd { get; } = new RoutedCommand(nameof(SelectToLineEnd));
        public static RoutedCommand MoveToDocumentStart { get; } = new RoutedCommand(nameof(MoveToDocumentStart));
        public static RoutedCommand SelectToDocumentStart { get; } = new RoutedCommand(nameof(SelectToDocumentStart));
        public static RoutedCommand MoveToDocumentEnd { get; } = new RoutedCommand(nameof(MoveToDocumentEnd));
        public static RoutedCommand SelectToDocumentEnd { get; } = new RoutedCommand(nameof(SelectToDocumentEnd));
    }
}
