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
using System.Text;

namespace AvaloniaEdit.Indentation.CSharp
{
    internal sealed class IndentationSettings
    {
        public string IndentString { get; set; } = "\t";

        public bool LeaveEmptyLines { get; set; } = true;
    }

    internal sealed class IndentationReformatter
    {
        /// <summary>
        /// An indentation block. Tracks the state of the indentation.
        /// </summary>
        private struct Block
        {
            /// <summary>
            /// The indentation outside of the block.
            /// </summary>
            public string OuterIndent;

            /// <summary>
            /// The indentation inside the block.
            /// </summary>
            public string InnerIndent;

            /// <summary>
            /// The last word that was seen inside this block.
            /// Because parenthesis open a sub-block and thus don't change their parent's LastWord,
            /// this property can be used to identify the type of block statement (if, while, switch)
            /// at the position of the '{'.
            /// </summary>
            public string LastWord;

            /// <summary>
            /// The type of bracket that opened this block (, [ or {
            /// </summary>
            public char Bracket;

            /// <summary>
            /// Gets whether there's currently a line continuation going on inside this block.
            /// </summary>
            public bool Continuation;

            /// <summary>
            /// Gets whether there's currently a 'one-line-block' going on. 'one-line-blocks' occur
            /// with if statements that don't use '{}'. They are not represented by a Block instance on
            /// the stack, but are instead handled similar to line continuations.
            /// This property is an integer because there might be multiple nested one-line-blocks.
            /// As soon as there is a finished statement, OneLineBlock is reset to 0.
            /// </summary>
            public int OneLineBlock;

            /// <summary>
            /// The previous value of one-line-block before it was reset.
            /// Used to restore the indentation of 'else' to the correct level.
            /// </summary>
            public int PreviousOneLineBlock;

            public void ResetOneLineBlock()
            {
                PreviousOneLineBlock = OneLineBlock;
                OneLineBlock = 0;
            }

            /// <summary>
            /// Gets the line number where this block started.
            /// </summary>
            public int StartLine;

            public void Indent(IndentationSettings set)
            {
                Indent(set.IndentString);
            }

            public void Indent(string indentationString)
            {
                OuterIndent = InnerIndent;
                InnerIndent += indentationString;
                Continuation = false;
                ResetOneLineBlock();
                LastWord = "";
            }

            public readonly override string ToString()
            {
                return string.Create(
                    CultureInfo.InvariantCulture,
                    $"[Block {nameof(StartLine)}={StartLine}, {nameof(LastWord)}='{LastWord}', {nameof(Continuation)}={Continuation}, {nameof(OneLineBlock)}={OneLineBlock}, {nameof(PreviousOneLineBlock)}={PreviousOneLineBlock}]");
            }
        }

        private StringBuilder _wordBuilder;
        private Stack<Block> _blocks; // blocks contains all blocks outside of the current
        private Block _block;  // block is the current block

        private bool _inString;
        private bool _inChar;
        private bool _verbatim;
        private bool _escape;

        private bool _lineComment;
        private bool _blockComment;

        private char _lastRealChar; // last non-comment char

        public void Reformat(IDocumentAccessor doc, IndentationSettings set)
        {
            Init();

            while (doc.MoveNext())
            {
                Step(doc, set);
            }
        }

        public void Init()
        {
            _wordBuilder = new StringBuilder();
            _blocks = new Stack<Block>();
            _block = new Block
            {
                InnerIndent = "",
                OuterIndent = "",
                Bracket = '{',
                Continuation = false,
                LastWord = "",
                OneLineBlock = 0,
                PreviousOneLineBlock = 0,
                StartLine = 0
            };

            _inString = false;
            _inChar = false;
            _verbatim = false;
            _escape = false;

            _lineComment = false;
            _blockComment = false;

            _lastRealChar = ' '; // last non-comment char
        }

        public void Step(IDocumentAccessor doc, IndentationSettings set)
        {
            var line = doc.Text;
            if (set.LeaveEmptyLines && line.Length == 0) return; // leave empty lines empty
            line = line.TrimStart();

            var indent = new StringBuilder();
            if (line.Length == 0)
            {
                // Special treatment for empty lines:
                if (_blockComment || (_inString && _verbatim))
                    return;
                indent.Append(_block.InnerIndent);
                indent.Append(Repeat(set.IndentString, _block.OneLineBlock));

                if (doc.Text != indent.ToString())
                    doc.Text = indent.ToString();
                return;
            }

            if (TrimEnd(doc))
                line = doc.Text.TrimStart();

            var oldBlock = _block;
            var startInComment = _blockComment;
            var startInString = (_inString && _verbatim);

            #region Parse char by char
            _lineComment = false;
            _inChar = false;
            _escape = false;
            if (!_verbatim) _inString = false;

            _lastRealChar = '\n';

            var c = ' ';
            var nextchar = line[0];
            for (var i = 0; i < line.Length; i++)
            {
                if (_lineComment) break; // cancel parsing current line

                var lastchar = c;
                c = nextchar;
                nextchar = i + 1 < line.Length ? line[i + 1] : '\n';

                if (_escape)
                {
                    _escape = false;
                    continue;
                }

                #region Check for comment/string chars
                switch (c)
                {
                    case '/':
                        if (_blockComment && lastchar == '*')
                            _blockComment = false;
                        if (!_inString && !_inChar)
                        {
                            if (!_blockComment && nextchar == '/')
                                _lineComment = true;
                            if (!_lineComment && nextchar == '*')
                                _blockComment = true;
                        }
                        break;
                    case '#':
                        if (!(_inChar || _blockComment || _inString))
                            _lineComment = true;
                        break;
                    case '"':
                        if (!(_inChar || _lineComment || _blockComment))
                        {
                            _inString = !_inString;
                            if (!_inString && _verbatim)
                            {
                                if (nextchar == '"')
                                {
                                    _escape = true; // skip escaped quote
                                    _inString = true;
                                }
                                else
                                {
                                    _verbatim = false;
                                }
                            }
                            else if (_inString && lastchar == '@')
                            {
                                _verbatim = true;
                            }
                        }
                        break;
                    case '\'':
                        if (!(_inString || _lineComment || _blockComment))
                        {
                            _inChar = !_inChar;
                        }
                        break;
                    case '\\':
                        if ((_inString && !_verbatim) || _inChar)
                            _escape = true; // skip next character
                        break;
                }
                #endregion

                if (_lineComment || _blockComment || _inString || _inChar)
                {
                    if (_wordBuilder.Length > 0)
                        _block.LastWord = _wordBuilder.ToString();
                    _wordBuilder.Length = 0;
                    continue;
                }

                if (!char.IsWhiteSpace(c) && c != '[' && c != '/')
                {
                    if (_block.Bracket == '{')
                        _block.Continuation = true;
                }

                if (char.IsLetterOrDigit(c))
                {
                    _wordBuilder.Append(c);
                }
                else
                {
                    if (_wordBuilder.Length > 0)
                        _block.LastWord = _wordBuilder.ToString();
                    _wordBuilder.Length = 0;
                }

                #region Push/Pop the blocks
                switch (c)
                {
                    case '{':
                        _block.ResetOneLineBlock();
                        _blocks.Push(_block);
                        _block.StartLine = doc.LineNumber;
                        if (_block.LastWord == "switch")
                        {
                            _block.Indent(set.IndentString + set.IndentString);
                            /* oldBlock refers to the previous line, not the previous block
                             * The block we want is not available anymore because it was never pushed.
                             * } else if (oldBlock.OneLineBlock) {
                            // Inside a one-line-block is another statement
                            // with a full block: indent the inner full block
                            // by one additional level
                            block.Indent(set, set.IndentString + set.IndentString);
                            block.OuterIndent += set.IndentString;
                            // Indent current line if it starts with the '{' character
                            if (i == 0) {
                                oldBlock.InnerIndent += set.IndentString;
                            }*/
                        }
                        else
                        {
                            _block.Indent(set);
                        }
                        _block.Bracket = '{';
                        break;
                    case '}':
                        while (_block.Bracket != '{')
                        {
                            if (_blocks.Count == 0) break;
                            _block = _blocks.Pop();
                        }
                        if (_blocks.Count == 0) break;
                        _block = _blocks.Pop();
                        _block.Continuation = false;
                        _block.ResetOneLineBlock();
                        break;
                    case '(':
                    case '[':
                        _blocks.Push(_block);
                        if (_block.StartLine == doc.LineNumber)
                            _block.InnerIndent = _block.OuterIndent;
                        else
                            _block.StartLine = doc.LineNumber;
                        _block.Indent(Repeat(set.IndentString, oldBlock.OneLineBlock) +
                                     (oldBlock.Continuation ? set.IndentString : "") +
                                     (i == line.Length - 1 ? set.IndentString : new string(' ', i + 1)));
                        _block.Bracket = c;
                        break;
                    case ')':
                        if (_blocks.Count == 0) break;
                        if (_block.Bracket == '(')
                        {
                            _block = _blocks.Pop();
                            if (IsSingleStatementKeyword(_block.LastWord))
                                _block.Continuation = false;
                        }
                        break;
                    case ']':
                        if (_blocks.Count == 0) break;
                        if (_block.Bracket == '[')
                            _block = _blocks.Pop();
                        break;
                    case ';':
                    case ',':
                        _block.Continuation = false;
                        _block.ResetOneLineBlock();
                        break;
                    case ':':
                        if (_block.LastWord == "case"
                            || line.StartsWith("case ", StringComparison.Ordinal)
                            || line.StartsWith(_block.LastWord + ":", StringComparison.Ordinal))
                        {
                            _block.Continuation = false;
                            _block.ResetOneLineBlock();
                        }
                        break;
                }

                if (!char.IsWhiteSpace(c))
                {
                    // register this char as last char
                    _lastRealChar = c;
                }
                #endregion
            }
            #endregion

            if (_wordBuilder.Length > 0)
                _block.LastWord = _wordBuilder.ToString();
            _wordBuilder.Length = 0;

            if (startInString) return;
            if (startInComment && line[0] != '*') return;
            if (doc.Text.StartsWith("//\t", StringComparison.Ordinal) || doc.Text == "//")
                return;

            if (line[0] == '}')
            {
                indent.Append(oldBlock.OuterIndent);
                oldBlock.ResetOneLineBlock();
                oldBlock.Continuation = false;
            }
            else
            {
                indent.Append(oldBlock.InnerIndent);
            }

            if (indent.Length > 0 && oldBlock.Bracket == '(' && line[0] == ')')
            {
                indent.Remove(indent.Length - 1, 1);
            }
            else if (indent.Length > 0 && oldBlock.Bracket == '[' && line[0] == ']')
            {
                indent.Remove(indent.Length - 1, 1);
            }

            if (line[0] == ':')
            {
                oldBlock.Continuation = true;
            }
            else if (_lastRealChar == ':' && indent.Length >= set.IndentString.Length)
            {
                if (_block.LastWord == "case" || line.StartsWith("case ", StringComparison.Ordinal) || line.StartsWith(_block.LastWord + ":", StringComparison.Ordinal))
                    indent.Remove(indent.Length - set.IndentString.Length, set.IndentString.Length);
            }
            else if (_lastRealChar == ')')
            {
                if (IsSingleStatementKeyword(_block.LastWord))
                {
                    _block.OneLineBlock++;
                }
            }
            else if (_lastRealChar == 'e' && _block.LastWord == "else")
            {
                _block.OneLineBlock = Math.Max(1, _block.PreviousOneLineBlock);
                _block.Continuation = false;
                oldBlock.OneLineBlock = _block.OneLineBlock - 1;
            }

            if (doc.IsReadOnly)
            {
                // We can't change the current line, but we should accept the existing
                // indentation if possible (=if the current statement is not a multiline
                // statement).
                if (!oldBlock.Continuation && oldBlock.OneLineBlock == 0 &&
                    oldBlock.StartLine == _block.StartLine &&
                    _block.StartLine < doc.LineNumber && _lastRealChar != ':')
                {
                    // use indent StringBuilder to get the indentation of the current line
                    indent.Length = 0;
                    line = doc.Text; // get untrimmed line
                    foreach (var t in line)
                    {
                        if (!char.IsWhiteSpace(t))
                            break;
                        indent.Append(t);
                    }
                    // /* */ multiline comments have an extra space - do not count it
                    // for the block's indentation.
                    if (startInComment && indent.Length > 0 && indent[indent.Length - 1] == ' ')
                    {
                        indent.Length -= 1;
                    }
                    _block.InnerIndent = indent.ToString();
                }
                return;
            }

            if (line[0] != '{')
            {
                if (line[0] != ')' && oldBlock.Continuation && oldBlock.Bracket == '{')
                    indent.Append(set.IndentString);
                indent.Append(Repeat(set.IndentString, oldBlock.OneLineBlock));
            }

            // this is only for blockcomment lines starting with *,
            // all others keep their old indentation
            if (startInComment)
                indent.Append(' ');

            if (indent.Length != (doc.Text.Length - line.Length) ||
                !doc.Text.StartsWith(indent.ToString(), StringComparison.Ordinal) ||
                char.IsWhiteSpace(doc.Text[indent.Length]))
            {
                doc.Text = indent + line;
            }
        }

        private static string Repeat(string text, int count)
        {
            if (count == 0)
                return string.Empty;
            if (count == 1)
                return text;
            var b = new StringBuilder(text.Length * count);
            for (var i = 0; i < count; i++)
                b.Append(text);
            return b.ToString();
        }

        private static bool IsSingleStatementKeyword(string keyword)
        {
            switch (keyword)
            {
                case "if":
                case "for":
                case "while":
                case "do":
                case "foreach":
                case "using":
                case "lock":
                    return true;
                default:
                    return false;
            }
        }

        private static bool TrimEnd(IDocumentAccessor doc)
        {
            var line = doc.Text;
            if (!char.IsWhiteSpace(line[line.Length - 1])) return false;

            // one space after an empty comment is allowed
            if (line.EndsWith("// ", StringComparison.Ordinal) || line.EndsWith("* ", StringComparison.Ordinal))
                return false;

            doc.Text = line.TrimEnd();
            return true;
        }
    }
}
