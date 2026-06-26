namespace Nikse.SubtitleEdit.Features.Shared.PickLanguage;

public class PickLanguageDisplay
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;

    public override string ToString() => $"{Name} ({Code})";
}
