namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class OcrLanguage2
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public OcrLanguage2(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}