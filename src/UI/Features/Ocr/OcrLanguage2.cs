namespace Nikse.SubtitleEdit.Features.Ocr;

public  class OcrLanguage2
{
    public string Code { get; set; }
    public string Name { get; set; }

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
