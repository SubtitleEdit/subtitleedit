using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class TextEncoding : IEquatable<Encoding>, IEquatable<string>
    {
        public const string Utf8WithBom = "UTF-8 with BOM";
        public const string Utf8WithoutBom = "UTF-8 without BOM";
        public const int Utf8WithBomIndex = 0;
        public const int Utf8WithoutBomIndex = 1;

        public Encoding Encoding { get; }

        public string DisplayName { get; }

        public TextEncoding(Encoding encoding, string displayName)
        {
            if (displayName != null)
            {
                Encoding = Encoding.UTF8;
                DisplayName = displayName;
            }
            else
            {
                Encoding = encoding;
                DisplayName = encoding.CodePage + ": " + encoding.EncodingName;
            }
        }

        public bool IsUtf8 => DisplayName == Utf8WithBom || DisplayName == Utf8WithoutBom;

        public override string ToString()
        {
            return DisplayName;
        }

        public bool Equals(string other)
        {
            return !ReferenceEquals(other, null) && (
                   Encoding.WebName.Equals(other, StringComparison.OrdinalIgnoreCase) ||
                   Encoding.EncodingName.Equals(other, StringComparison.OrdinalIgnoreCase) ||
                   DisplayName.Equals(other, StringComparison.OrdinalIgnoreCase) ||
                   Encoding.CodePage.ToString().Equals(other));
        }

        public bool Equals(Encoding other)
        {
            return !ReferenceEquals(other, null) && Encoding.CodePage.Equals(other.CodePage);
        }

        public override bool Equals(object obj)
        {
            var item = obj as TextEncoding;
            return !ReferenceEquals(item, null) && Equals(item.Encoding);
        }

        public override int GetHashCode()
        {
            return Encoding.CodePage.GetHashCode();
        }
    }
}
