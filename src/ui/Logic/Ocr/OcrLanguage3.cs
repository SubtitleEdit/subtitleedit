namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class OcrLanguage3
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public OcrLanguage3(string code, string name)
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