using Avalonia.Controls;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.PickVobSubLanguage;

public class VobSubLanguageCueDisplay
{
    public int Number { get; set; }
    public TimeSpan Show { get; set; }
    public TimeSpan Duration { get; set; }
    public Image? Image { get; set; }
}
