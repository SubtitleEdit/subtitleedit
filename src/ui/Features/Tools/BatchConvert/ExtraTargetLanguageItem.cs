using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

/// <summary>One extra "To" combo box in Batch Convert's auto-translate.</summary>
public partial class ExtraTargetLanguageItem : ObservableObject
{
    [ObservableProperty] private TranslationPair? _selectedLanguage;
}
