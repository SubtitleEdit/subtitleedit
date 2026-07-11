namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// Combo-box row for a CrispEmbed model: keeps the backend alongside the model so the
/// item template can show the install-status dot.
/// </summary>
public class CrispEmbedModelDisplay
{
    public required CrispEmbedBackend Backend { get; init; }
    public required CrispEmbedModel Model { get; init; }

    public override string ToString()
    {
        return Model.Name;
    }
}
