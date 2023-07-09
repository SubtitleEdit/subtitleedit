using System;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public class TranslationPair : IEquatable<TranslationPair>
    {
        public string Name { get; }
        
        public string Code { get; }

        public TranslationPair(string name, string code)
        {
            Name = name ?? string.Empty;
            Code = code;
        }

        public override string ToString() => Name.ToLowerInvariant().Replace('_', ' ').CapitalizeFirstLetter();

        public bool Equals(TranslationPair other) => other != null && Code.Equals(other.Code);

        public override int GetHashCode() => Code != null ? Code.GetHashCode() : 0;
    }
}
