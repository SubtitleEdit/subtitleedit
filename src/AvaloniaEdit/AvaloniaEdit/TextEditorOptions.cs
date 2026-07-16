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
using System.ComponentModel;
using System.Reflection;
using AvaloniaEdit.CodeCompletion;

namespace AvaloniaEdit
{
    /// <summary>
    /// A container for the text editor options.
    /// </summary>
    public class TextEditorOptions : INotifyPropertyChanged
    {
        #region ctor
        /// <summary>
        /// Initializes an empty instance of TextEditorOptions.
        /// </summary>
        public TextEditorOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of TextEditorOptions by copying all values
        /// from <paramref name="options"/> to the new instance.
        /// </summary>
        public TextEditorOptions(TextEditorOptions options)
        {
            // get all the fields in the class
            var fields = typeof(TextEditorOptions).GetRuntimeFields();

            // copy each value over to 'this'
            foreach (FieldInfo fi in fields)
            {
                if (!fi.IsStatic)
                    fi.SetValue(this, fi.GetValue(options));
            }
        }
        #endregion

        #region PropertyChanged handling
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            var args = new PropertyChangedEventArgs(propertyName);
            OnPropertyChanged(args);
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
        #endregion

        #region AccepsTab

        [DefaultValue(true)]
        public virtual bool AcceptsTab
        {
            get
            {
                return field;
            }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(AcceptsTab));
                }
            }
        } = true;

        #endregion

        #region ShowSpaces / ShowTabs / ShowEndOfLine / ShowBoxForControlCharacters

        /// <summary>
        /// Gets/Sets whether to show a visible glyph for spaces. The glyph displayed can be set via <see cref="ShowSpacesGlyph" />
        /// </summary>
        /// <remarks>The default value is <c>false</c>.</remarks>
        [DefaultValue(false)]
        public virtual bool ShowSpaces
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ShowSpaces));
                }
            }
        }

        /// <summary>
        /// Gets/Sets the char to show when ShowSpaces option is enabled
        /// </summary>
        /// <remarks>The default value is <c>·</c>.</remarks>
        [DefaultValue("\u00B7")]
        public virtual string ShowSpacesGlyph
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ShowSpacesGlyph));
                }
            }
        } = "\u00B7";

        /// <summary>
        /// Gets/Sets whether to show a visible glyph for tab. The glyph displayed can be set via <see cref="ShowTabsGlyph" />
        /// </summary>
        /// <remarks>The default value is <c>false</c>.</remarks>
        [DefaultValue(false)]
        public virtual bool ShowTabs
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ShowTabs));
                }
            }
        }

        /// <summary>
        /// Gets/Sets the char to show when ShowTabs option is enabled
        /// </summary>
        /// <remarks>The default value is <c>→</c>.</remarks>
        [DefaultValue("\u2192")]
        public virtual string ShowTabsGlyph
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ShowTabsGlyph));
                }
            }
        } = "\u2192";

        /// <summary>
        /// Gets/Sets whether to show EOL char at the end of lines. The glyphs displayed can be set via <see cref="EndOfLineCRLFGlyph" />, <see cref="EndOfLineCRGlyph" /> and <see cref="EndOfLineLFGlyph" />.
        /// </summary>
        /// <remarks>The default value is <c>false</c>.</remarks>
        [DefaultValue(false)]
        public virtual bool ShowEndOfLine
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ShowEndOfLine));
                }
            }
        }

        /// <summary>
        /// Gets/Sets the char to show for CRLF (\r\n) when ShowEndOfLine option is enabled
        /// </summary>
        /// <remarks>The default value is <c>¶</c>.</remarks>
        [DefaultValue("¶")]
        public virtual string EndOfLineCRLFGlyph
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(EndOfLineCRLFGlyph));
                }
            }
        } = "¶";

        /// <summary>
        /// Gets/Sets the char to show for CR (\r) when ShowEndOfLine option is enabled
        /// </summary>
        /// <remarks>The default value is <c>\r</c>.</remarks>
        [DefaultValue("\\r")]
        public virtual string EndOfLineCRGlyph
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(EndOfLineCRGlyph));
                }
            }
        } = "\\r";

        /// <summary>
        /// Gets/Sets the char to show for LF (\n) when ShowEndOfLine option is enabled
        /// </summary>
        /// <remarks>The default value is <c>\n</c>.</remarks>
        [DefaultValue("\\n")]
        public virtual string EndOfLineLFGlyph
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(EndOfLineLFGlyph));
                }
            }
        } = "\\n";

        /// <summary>
        /// Gets/Sets whether to show a box with the hex code for control characters.
        /// </summary>
        /// <remarks>The default value is <c>true</c>.</remarks>
        [DefaultValue(true)]
        public virtual bool ShowBoxForControlCharacters
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ShowBoxForControlCharacters));
                }
            }
        } = true;

        #endregion

        #region EnableHyperlinks

        /// <summary>
        /// Gets/Sets whether to enable clickable hyperlinks in the editor.
        /// </summary>
        /// <remarks>The default value is <c>true</c>.</remarks>
        [DefaultValue(true)]
        public virtual bool EnableHyperlinks
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(EnableHyperlinks));
                }
            }
        } = true;

        /// <summary>
        /// Gets/Sets whether to enable clickable hyperlinks for e-mail addresses in the editor.
        /// </summary>
        /// <remarks>The default value is <c>true</c>.</remarks>
        [DefaultValue(true)]
        public virtual bool EnableEmailHyperlinks
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(EnableEmailHyperlinks));
                }
            }
        } = true;

        /// <summary>
        /// Gets/Sets whether the user needs to press Control to click hyperlinks.
        /// The default value is true.
        /// </summary>
        /// <remarks>The default value is <c>true</c>.</remarks>
        [DefaultValue(true)]
        public virtual bool RequireControlModifierForHyperlinkClick
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(RequireControlModifierForHyperlinkClick));
                }
            }
        } = true;

        #endregion

        #region TabSize / IndentationSize / ConvertTabsToSpaces / GetIndentationString
        // I'm using '_' prefixes for the fields here to avoid confusion with the local variables
        // in the methods below.
        // The fields should be accessed only by their property - the fields might not be used
        // if someone overrides the property.

        /// <summary>
        /// Gets/Sets the width of one indentation unit.
        /// </summary>
        /// <remarks>The default value is 4.</remarks>
        [DefaultValue(4)]
        public virtual int IndentationSize
        {
            get;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "value must be positive");
                // sanity check; a too large value might cause a crash internally much later
                // (it only crashed in the hundred thousands for me; but might crash earlier with larger fonts)
                if (value > 1000)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "indentation size is too large");
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(IndentationSize));
                    OnPropertyChanged(nameof(IndentationString));
                }
            }
        } = 4;

        /// <summary>
        /// Gets/Sets whether to use spaces for indentation instead of tabs.
        /// </summary>
        /// <remarks>The default value is <c>false</c>.</remarks>
        [DefaultValue(false)]
        public virtual bool ConvertTabsToSpaces
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ConvertTabsToSpaces));
                    OnPropertyChanged(nameof(IndentationString));
                }
            }
        }

        /// <summary>
        /// Gets the text used for indentation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public string IndentationString => GetIndentationString(1);

        /// <summary>
        /// Gets text required to indent from the specified <paramref name="column"/> to the next indentation level.
        /// </summary>
        public virtual string GetIndentationString(int column)
        {
            if (column < 1)
                throw new ArgumentOutOfRangeException(nameof(column), column, "Value must be at least 1.");
            int indentationSize = IndentationSize;
            if (ConvertTabsToSpaces)
            {
                return new string(' ', indentationSize - ((column - 1) % indentationSize));
            }
            else
            {
                return "\t";
            }
        }
        #endregion

        /// <summary>
        /// Gets/Sets whether copying without a selection copies the whole current line.
        /// </summary>
        [DefaultValue(true)]
        public virtual bool CutCopyWholeLine
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(CutCopyWholeLine));
                }
            }
        } = true;

        /// <summary>
        /// Gets/Sets whether the user can scroll below the bottom of the document.
        /// The default value is true; but it a good idea to set this property to true when using folding.
        /// </summary>
        [DefaultValue(true)]
        public virtual bool AllowScrollBelowDocument
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(AllowScrollBelowDocument));
                }
            }
        } = true;

        /// <summary>
        /// Gets/Sets the indentation used for all lines except the first when word-wrapping.
        /// The default value is 0.
        /// </summary>
        [DefaultValue(0.0)]
        public virtual double WordWrapIndentation
        {
            get;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value), value, "value must not be NaN/infinity");
                if (value != field)
                {
                    field = value;
                    OnPropertyChanged(nameof(WordWrapIndentation));
                }
            }
        }

        /// <summary>
        /// Gets/Sets whether the indentation is inherited from the first line when word-wrapping.
        /// The default value is true.
        /// </summary>
        /// <remarks>When combined with <see cref="WordWrapIndentation"/>, the inherited indentation is added to the word wrap indentation.</remarks>
        [DefaultValue(true)]
        public virtual bool InheritWordWrapIndentation
        {
            get { return field; }
            set
            {
                if (value != field)
                {
                    field = value;
                    OnPropertyChanged(nameof(InheritWordWrapIndentation));
                }
            }
        } = true;

        /// <summary>
        /// Enables rectangular selection (press ALT and select a rectangle)
        /// </summary>
        [DefaultValue(true)]
        public bool EnableRectangularSelection
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(EnableRectangularSelection));
                }
            }
        } = true;

        /// <summary>
        /// Enable dragging text within the text area.
        /// </summary>
        [DefaultValue(true)]
        public bool EnableTextDragDrop
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(EnableTextDragDrop));
                }
            }
        } = true;

        /// <summary>
        /// Gets/Sets whether the user can set the caret behind the line ending
        /// (into "virtual space").
        /// Note that virtual space is always used (independent from this setting)
        /// when doing rectangle selections.
        /// </summary>
        [DefaultValue(false)]
        public virtual bool EnableVirtualSpace
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(EnableVirtualSpace));
                }
            }
        }

        /// <summary>
        /// Gets/Sets whether the support for Input Method Editors (IME)
        /// for non-alphanumeric scripts (Chinese, Japanese, Korean, ...) is enabled.
        /// </summary>
        [DefaultValue(true)]
        public virtual bool EnableImeSupport
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(EnableImeSupport));
                }
            }
        } = true;

        /// <summary>
        /// Gets/Sets whether the column rulers should be shown.
        /// </summary>
        [DefaultValue(false)]
        public virtual bool ShowColumnRulers
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ShowColumnRulers));
                }
            }
        }

        /// <summary>
        /// Gets/Sets the positions the column rulers should be shown.
        /// </summary>
        public virtual IEnumerable<int> ColumnRulerPositions
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ColumnRulerPositions));
                }
            }
        } = new List<int>() { 80 };

        /// <summary>
        /// Gets/Sets if current line should be shown.
        /// </summary>
        [DefaultValue(false)]
        public virtual bool HighlightCurrentLine
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(HighlightCurrentLine));
                }
            }
        }

        /// <summary>
        /// Gets/Sets if mouse cursor should be hidden while user is typing.
        /// </summary>
        [DefaultValue(true)]
        public bool HideCursorWhileTyping
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(HideCursorWhileTyping));
                }
            }
        } = true;

        /// <summary>
        /// Gets/Sets if the user is allowed to enable/disable overstrike mode.
        /// </summary>
        [DefaultValue(false)]
        public bool AllowToggleOverstrikeMode
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(AllowToggleOverstrikeMode));
                }
            }
        }

        /// <summary>
        /// Gets/Sets if the mouse up event should extend the editor selection to the mouse position.
        /// </summary>
        [DefaultValue(true)]
        public bool ExtendSelectionOnMouseUp
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ExtendSelectionOnMouseUp));
                }
            }
        } = true;

        /// <summary>
        /// Gets/Sets the pointer action used to request the insertion of a completion item.
        /// </summary>
        [DefaultValue(CompletionAcceptAction.PointerPressed)]
        public CompletionAcceptAction CompletionAcceptAction
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(CompletionAcceptAction));
                }
            }
        } = CompletionAcceptAction.PointerPressed;

        // The default LineHeightFactor matches the line height in the Visual Studio text editor.
        private const double DefaultLineHeightFactor = 1.16;

        /// <summary>
        /// Gets/Sets a factor to increase or decrease the height of a text line.
        /// (Does not affect the font size.)
        /// </summary>
        [DefaultValue(DefaultLineHeightFactor)]
        public double LineHeightFactor
        {
            get { return field; }
            set
            {
                if (value <= 0 || double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value), value, "value must be a positive number");
                
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(LineHeightFactor));
                }
            }
        } = DefaultLineHeightFactor;
    }
}
