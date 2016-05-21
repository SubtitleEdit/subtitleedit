using System;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    internal class TextEncodingListItem : IEquatable<Encoding>, IEquatable<string>
    {
        private readonly Encoding _encoding;
        private readonly string _displayName;

        public Encoding Encoding
        {
            get
            {
                return _encoding;
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public TextEncodingListItem(Encoding encoding)
        {
            if (encoding.CodePage.Equals(Encoding.UTF8.CodePage))
            {
                _encoding = Encoding.UTF8;
                _displayName = _encoding.EncodingName;
            }
            else
            {
                _encoding = encoding;
                _displayName = encoding.CodePage + ": " + encoding.EncodingName;
            }
        }

        public override string ToString()
        {
            return _displayName;
        }

        public bool Equals(string name)
        {
            return !ReferenceEquals(name, null) && (
                   _encoding.WebName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                   _encoding.EncodingName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                   _displayName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                   _encoding.CodePage.ToString().Equals(name));
        }

        public bool Equals(Encoding encoding)
        {
            return !ReferenceEquals(encoding, null) && _encoding.CodePage.Equals(encoding.CodePage);
        }

        public override bool Equals(object obj)
        {
            var item = obj as TextEncodingListItem;
            return !ReferenceEquals(item, null) && Equals(item.Encoding);
        }

        public override int GetHashCode()
        {
            return _encoding.CodePage.GetHashCode();
        }
    }
}
