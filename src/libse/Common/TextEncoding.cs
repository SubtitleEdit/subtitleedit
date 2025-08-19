using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class TextEncoding : IEquatable<Encoding>, IEquatable<string>
    {
        public static readonly string Utf8WithBom = "UTF-8 with BOM";
        public static readonly string Utf8WithoutBom = "UTF-8 without BOM";
        public static readonly string Source = "Source";
        public static readonly int Utf8WithBomIndex = 0;
        public static readonly int Utf8WithoutBomIndex = 1;

        public Encoding Encoding { get; }

        public string DisplayName { get; }

        public TextEncoding(Encoding encoding, string utf8DisplayName)
        {
            if (utf8DisplayName != null)
            {
                Encoding = Encoding.UTF8;
                DisplayName = utf8DisplayName;
            }
            else
            {
                Encoding = encoding;
                DisplayName = encoding.CodePage + ": " + encoding.EncodingName;
            }
        }

        public bool IsUtf8 => DisplayName == Utf8WithBom || DisplayName == Utf8WithoutBom;
        public bool UseSourceEncoding => DisplayName == Source;

        public override string ToString()
        {
            return DisplayName;
        }

        public bool Equals(string other)
        {
            if (string.IsNullOrEmpty(other))
            {
                return false;
            }

            return Encoding.WebName.Equals(other, StringComparison.Ordinal)
                || Encoding.EncodingName.Equals(other, StringComparison.Ordinal)
                || DisplayName.Equals(other, StringComparison.Ordinal)
                || Encoding.CodePage.ToString().Equals(other, StringComparison.Ordinal);
        }

        public bool Equals(Encoding other)
        {
            return other != null && Encoding.CodePage == other.CodePage;
        }

        public override bool Equals(object obj)
        {
            if (obj is TextEncoding te)
            {
                return Encoding.CodePage == te.Encoding.CodePage && string.Equals(DisplayName, te.DisplayName, StringComparison.Ordinal);
            }

            if (obj is Encoding enc)
            {
                return Equals(enc);
            }

            if (obj is string s)
            {
                return Equals(s);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + Encoding.CodePage.GetHashCode();
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(DisplayName);
                return hash;
            }
        }
    }
}
