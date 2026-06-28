namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>
/// A DeepL "formality" option (e.g. default / more formal / less formal). The <see cref="Code"/>
/// is the value sent to the DeepL API; <see cref="Name"/> is shown in the picker.
/// </summary>
public class DeepLFormalityItem
{
    public string Code { get; }
    public string Name { get; }

    public DeepLFormalityItem(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public override string ToString() => Name;
}
