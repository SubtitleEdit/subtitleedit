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
using System.Text.RegularExpressions;

namespace AvaloniaEdit.Rendering
{
    // This class is public because it can be used as a base class for custom links.

    /// <summary>
    /// Detects hyperlinks and makes them clickable.
    /// </summary>
    /// <remarks>
    /// This element generator can be easily enabled and configured using the
    /// <see cref="TextEditorOptions"/>.
    /// </remarks>
    public partial class LinkElementGenerator : VisualLineElementGenerator, IBuiltinElementGenerator
    {
        [GeneratedRegex(@"\b[\w\d\.\-\+]+\@[\w\d\.\-]+\.[a-z]{2,6}\b")]
        private static partial Regex DefaultMailGeneratedRegex();

        [GeneratedRegex(@"\b(https?://|ftp://|www\.)[\w\d\._/\-~%@()+:?&=#!]*[\w\d/]")]
        private static partial Regex DefaultLinkGeneratedRegex();

        // a link starts with a protocol (or just with www), followed by 0 or more 'link characters', followed by a link end character
        // (this allows accepting punctuation inside links but not at the end)
        internal static readonly Regex DefaultLinkRegex = DefaultLinkGeneratedRegex();

        // try to detect email addresses
        internal static readonly Regex DefaultMailRegex = DefaultMailGeneratedRegex();

        private readonly Regex _linkRegex;

        /// <summary>
        /// Gets/Sets whether the user needs to press Control to click the link.
        /// The default value is true.
        /// </summary>
        public bool RequireControlModifierForClick { get; set; }

        /// <summary>
        /// Creates a new LinkElementGenerator.
        /// </summary>
        public LinkElementGenerator()
        {
            _linkRegex = DefaultLinkRegex;
            RequireControlModifierForClick = true;
        }

        /// <summary>
        /// Creates a new LinkElementGenerator using the specified regex.
        /// </summary>
        protected LinkElementGenerator(Regex regex) : this()
        {
            _linkRegex = regex ?? throw new ArgumentNullException(nameof(regex));
        }

        void IBuiltinElementGenerator.FetchOptions(TextEditorOptions options)
        {
            RequireControlModifierForClick = options.RequireControlModifierForHyperlinkClick;
        }

        private Match GetMatch(int startOffset, out int matchOffset)
        {
            var endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
            var relevantText = CurrentContext.GetText(startOffset, endOffset - startOffset);
            var m = _linkRegex.Match(relevantText.Text, relevantText.Offset, relevantText.Count);
            matchOffset = m.Success ? m.Index - relevantText.Offset + startOffset : -1;
            return m;
        }

        /// <inheritdoc/>
        public override int GetFirstInterestedOffset(int startOffset)
        {
            GetMatch(startOffset, out var matchOffset);
            return matchOffset;
        }

        /// <inheritdoc/>
        public override VisualLineElement ConstructElement(int offset)
        {
            var m = GetMatch(offset, out var matchOffset);
            if (m.Success && matchOffset == offset)
            {
                return ConstructElementFromMatch(m);
            }

            return null;
        }

        /// <summary>
        /// Constructs a VisualLineElement that replaces the matched text.
        /// The default implementation will create a <see cref="VisualLineLinkText"/>
        /// based on the URI provided by <see cref="GetUriFromMatch"/>.
        /// </summary>
        protected virtual VisualLineElement ConstructElementFromMatch(Match m)
        {
            var uri = GetUriFromMatch(m);
            if (uri == null)
                return null;
            var linkText = new VisualLineLinkText(CurrentContext.VisualLine, m.Length)
            {
                NavigateUri = uri,
                RequireControlModifierForClick = RequireControlModifierForClick
            };
            return linkText;
        }

        /// <summary>
        /// Fetches the URI from the regex match. Returns null if the URI format is invalid.
        /// </summary>
        protected virtual Uri GetUriFromMatch(Match match)
        {
            var targetUrl = match.Value;
            if (targetUrl.StartsWith("www.", StringComparison.Ordinal))
                targetUrl = "http://" + targetUrl;
            return Uri.IsWellFormedUriString(targetUrl, UriKind.Absolute) ? new Uri(targetUrl) : null;
        }
    }

    // This class is internal because it does not need to be accessed by the user - it can be configured using TextEditorOptions.

    /// <summary>
    /// Detects e-mail addresses and makes them clickable.
    /// </summary>
    /// <remarks>
    /// This element generator can be easily enabled and configured using the
    /// <see cref="TextEditorOptions"/>.
    /// </remarks>
    internal sealed class MailLinkElementGenerator : LinkElementGenerator
    {
        /// <summary>
        /// Creates a new MailLinkElementGenerator.
        /// </summary>
        public MailLinkElementGenerator()
            : base(DefaultMailRegex)
        {
        }

        protected override Uri GetUriFromMatch(Match match)
        {
            var targetUrl = "mailto:" + match.Value;
            return Uri.IsWellFormedUriString(targetUrl, UriKind.Absolute) ? new Uri(targetUrl) : null;
        }
    }
}
