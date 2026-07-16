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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;

namespace AvaloniaEdit.Highlighting.Xshd
{
	/// <summary>
	/// Xshd visitor implementation that saves an .xshd file as XML.
	/// </summary>
	public sealed class SaveXshdVisitor : IXshdVisitor
	{
		/// <summary>
		/// XML namespace for XSHD.
		/// </summary>
		public const string Namespace = V2Loader.Namespace;

	    private readonly XmlWriter _writer;
		
		/// <summary>
		/// Creates a new SaveXshdVisitor instance.
		/// </summary>
		public SaveXshdVisitor(XmlWriter writer)
		{
		    _writer = writer ?? throw new ArgumentNullException(nameof(writer));
		}
		
		/// <summary>
		/// Writes the specified syntax definition.
		/// </summary>
		public void WriteDefinition(XshdSyntaxDefinition definition)
		{
			if (definition == null)
				throw new ArgumentNullException(nameof(definition));
			_writer.WriteStartElement("SyntaxDefinition", Namespace);
			if (definition.Name != null)
				_writer.WriteAttributeString("name", definition.Name);
			if (definition.Extensions != null)
				_writer.WriteAttributeString("extensions", string.Join(";", definition.Extensions.ToArray()));
			
			definition.AcceptElements(this);
			
			_writer.WriteEndElement();
		}
		
		object IXshdVisitor.VisitRuleSet(XshdRuleSet ruleSet)
		{
			_writer.WriteStartElement("RuleSet", Namespace);
			
			if (ruleSet.Name != null)
				_writer.WriteAttributeString("name", ruleSet.Name);
			WriteBoolAttribute("ignoreCase", ruleSet.IgnoreCase);
			
			ruleSet.AcceptElements(this);
			
			_writer.WriteEndElement();
			return null;
		}

	    private void WriteBoolAttribute(string attributeName, bool? value)
		{
			if (value != null) {
				_writer.WriteAttributeString(attributeName, value.Value ? "true" : "false");
			}
		}

	    private void WriteRuleSetReference(XshdReference<XshdRuleSet> ruleSetReference)
		{
			if (ruleSetReference.ReferencedElement != null) {
				if (ruleSetReference.ReferencedDefinition != null)
					_writer.WriteAttributeString("ruleSet", ruleSetReference.ReferencedDefinition + "/" + ruleSetReference.ReferencedElement);
				else
					_writer.WriteAttributeString("ruleSet", ruleSetReference.ReferencedElement);
			}
		}

	    private void WriteColorReference(XshdReference<XshdColor> color)
		{
			if (color.InlineElement != null) {
				WriteColorAttributes(color.InlineElement);
			} else if (color.ReferencedElement != null) {
				if (color.ReferencedDefinition != null)
					_writer.WriteAttributeString("color", color.ReferencedDefinition + "/" + color.ReferencedElement);
				else
					_writer.WriteAttributeString("color", color.ReferencedElement);
			}
		}
		
		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "The file format requires lowercase, and all possible values are English-only")]
		private void WriteColorAttributes(XshdColor color)
		{
			if (color.Foreground != null)
				_writer.WriteAttributeString("foreground", color.Foreground.ToString());
			if (color.Background != null)
				_writer.WriteAttributeString("background", color.Background.ToString());
			if (color.FontWeight != null)
				_writer.WriteAttributeString("fontWeight", color.FontWeight.Value.ToString().ToLowerInvariant());
			if (color.FontStyle != null)
				_writer.WriteAttributeString("fontStyle", color.FontStyle.Value.ToString().ToLowerInvariant());
		}
		
		object IXshdVisitor.VisitColor(XshdColor color)
		{
			_writer.WriteStartElement("Color", Namespace);
			if (color.Name != null)
				_writer.WriteAttributeString("name", color.Name);
			WriteColorAttributes(color);
			if (color.ExampleText != null)
				_writer.WriteAttributeString("exampleText", color.ExampleText);
			_writer.WriteEndElement();
			return null;
		}
		
		object IXshdVisitor.VisitKeywords(XshdKeywords keywords)
		{
			_writer.WriteStartElement("Keywords", Namespace);
			WriteColorReference(keywords.ColorReference);
			foreach (string word in keywords.Words) {
				_writer.WriteElementString("Word", Namespace, word);
			}
			_writer.WriteEndElement();
			return null;
		}
		
		object IXshdVisitor.VisitSpan(XshdSpan span)
		{
			_writer.WriteStartElement("Span", Namespace);
			WriteColorReference(span.SpanColorReference);
			if (span.BeginRegexType == XshdRegexType.Default && span.BeginRegex != null)
				_writer.WriteAttributeString("begin", span.BeginRegex);
			if (span.EndRegexType == XshdRegexType.Default && span.EndRegex != null)
				_writer.WriteAttributeString("end", span.EndRegex);
			WriteRuleSetReference(span.RuleSetReference);
			if (span.Multiline)
				_writer.WriteAttributeString("multiline", "true");
			
			if (span.BeginRegexType == XshdRegexType.IgnorePatternWhitespace)
				WriteBeginEndElement("Begin", span.BeginRegex, span.BeginColorReference);
			if (span.EndRegexType == XshdRegexType.IgnorePatternWhitespace)
				WriteBeginEndElement("End", span.EndRegex, span.EndColorReference);

		    span.RuleSetReference.InlineElement?.AcceptVisitor(this);

		    _writer.WriteEndElement();
			return null;
		}

	    private void WriteBeginEndElement(string elementName, string regex, XshdReference<XshdColor> colorReference)
		{
			if (regex != null) {
				_writer.WriteStartElement(elementName, Namespace);
				WriteColorReference(colorReference);
				_writer.WriteString(regex);
				_writer.WriteEndElement();
			}
		}
		
		object IXshdVisitor.VisitImport(XshdImport import)
		{
			_writer.WriteStartElement("Import", Namespace);
			WriteRuleSetReference(import.RuleSetReference);
			_writer.WriteEndElement();
			return null;
		}
		
		object IXshdVisitor.VisitRule(XshdRule rule)
		{
			_writer.WriteStartElement("Rule", Namespace);
			WriteColorReference(rule.ColorReference);
			
			_writer.WriteString(rule.Regex);
			
			_writer.WriteEndElement();
			return null;
		}
	}
}
