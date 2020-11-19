namespace Nikse.SubtitleEdit.Core.Translate
{
    public class TranslationPair
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public TranslationPair()
        {

        }

        public TranslationPair(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
