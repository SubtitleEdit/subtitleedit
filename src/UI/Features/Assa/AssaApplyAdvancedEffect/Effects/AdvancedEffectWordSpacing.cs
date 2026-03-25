using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectWordSpacing : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectWordSpacing;
    public string Description => Se.Language.Assa.AdvancedEffectWordSpacingDescription;
    public bool UsesAudio => false;
    public decimal SpacingPixels { get; set; } = 10m;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();

        foreach (var sub in subtitles)
        {
            var newSub = new SubtitleLineViewModel(sub, generateNewId: true);

            // Process the text to add \fsp tag for spaces
            string processedText = ProcessTextWithSpacing(sub.Text);
            newSub.Text = processedText;

            result.Add(newSub);
        }

        return result;
    }

    private string ProcessTextWithSpacing(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var result = new StringBuilder();
        bool insideTags = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '{')
            {
                insideTags = true;
                result.Append(c);
            }
            else if (c == '}')
            {
                insideTags = false;
                result.Append(c);
            }
            else if (c == ' ' && !insideTags)
            {
                // Replace space with tagged space using \fsp
                result.Append($"{{\\fsp{SpacingPixels}}} {{\\fsp0}}");
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
}

