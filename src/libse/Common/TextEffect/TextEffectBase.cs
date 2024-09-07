using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Common.TextEffect
{
    public abstract class TextEffectBase
    {
        public abstract string[] Transform(string text);

        // ReSharper disable once IdentifierTypo
        protected bool IsVisibleChar(char ch)
        {
            var uc = char.GetUnicodeCategory(ch);
            return uc != UnicodeCategory.Control && uc != UnicodeCategory.SpaceSeparator;
        }
    }
}