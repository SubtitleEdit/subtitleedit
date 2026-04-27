namespace Nikse.SubtitleEdit.Logic.Config;

public class SeChangeCasing
{
    public bool NormalCasing { get; set; }
    public bool NormalCasingFixNames { get; set; }
    public bool NormalCasingOnlyUpper { get; set; }
    public bool FixNamesOnly { get; set; }
    public bool AllUppercase { get; set; }
    public bool AllLowercase { get; set; }
    public string ExtraNames { get; set; }

    public SeChangeCasing()
    {
        NormalCasing = true;
        NormalCasingFixNames = true;
        ExtraNames = string.Empty;
    }
}