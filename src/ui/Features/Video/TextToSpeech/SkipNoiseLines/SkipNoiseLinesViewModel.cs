using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SkipNoiseLines;

public class SkipNoiseLinesViewModel : SelectLinesViewModelBase<SkipNoiseLineRow>
{
    /// <summary>The lines the user confirmed should stay silent - no speech is generated for them.</summary>
    public List<Paragraph> SelectedParagraphs { get; private set; }

    public SkipNoiseLinesViewModel()
    {
        SelectedParagraphs = new List<Paragraph>();
    }

    public void Initialize(List<Paragraph> noiseLines)
    {
        Rows.Clear();
        foreach (var paragraph in noiseLines)
        {
            Rows.Add(new SkipNoiseLineRow(paragraph));
        }

        RowsInfo = string.Format(Se.Language.Video.TextToSpeech.SkipNoiseLinesFoundX, Rows.Count);
    }

    protected override void CollectResult()
    {
        SelectedParagraphs = Rows.Where(r => r.IsSelected).Select(r => r.Paragraph).ToList();
    }
}
