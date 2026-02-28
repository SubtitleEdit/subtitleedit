namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageManualChosenEncoding
{
    public string Title { get; set; }
    public string SearchEncodings { get; set; }
    public string CodePage { get; set; }

    public LanguageManualChosenEncoding()
    {
        Title = "Import subtitle with manually chosen encoding";
        SearchEncodings = "Search encodings";
        CodePage = "Code page";
    }
}
