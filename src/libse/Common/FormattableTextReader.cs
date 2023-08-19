namespace Nikse.SubtitleEdit.Core.Common
{
    public struct FormattableTextReader
    {
        private readonly string _text;
        private int _current;

        public FormattableTextReader(string text, int position = -1)
        {
            _text = text;
            _current = position;
        }

        public bool Read()
        {
            var len = _text.Length;
            if (_current + 1 < len)
            {
                _current++;

                while (IsTagStartChar(GetCurrent()))
                {
                    var closeChar = GetClosingPair(GetCurrent());

                    // skip tag
                    var tagCloseIndex = _text.IndexOf(closeChar, _current + 1);
                    // '<' found but it doesn't have a closing pair
                    if (tagCloseIndex < _current + 2) // 1 tag name index
                    {
                        return true;
                    }

                    _current = tagCloseIndex + 1;
                }

                return true;
            }

            return false;
        }

        public char GetCurrent()
        {
            if (_current < _text.Length)
            {
                return _text[_current];
            }

            return '\0';
        }

        private static char GetClosingPair(char openTag)
        {
            if (openTag == '<')
            {
                return '>';
            }

            if (openTag == '{')
            {
                return '}';
            }

            return '\0';
        }

        private static bool IsTagStartChar(char charAtPosition)
        {
            if (charAtPosition == '<')
            {
                return true;
            }

            if (charAtPosition == '{')
            {
                return true;
            }

            return false;
        }
    }
}