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

        public bool Equals(string name)
        {
            return !ReferenceEquals(name, null) && (
                   Encoding.WebName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                   Encoding.EncodingName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                   DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                   Encoding.CodePage.ToString().Equals(name));
        }

        public bool Equals(Encoding encoding)
        {
            return !ReferenceEquals(encoding, null) && Encoding.CodePage.Equals(encoding.CodePage);
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
