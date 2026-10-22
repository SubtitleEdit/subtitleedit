using System;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate.Llm;
namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

// Compatibility adapter for the old local-engine UI and callers.
public abstract class AdvancedTranslatorBase : UiLogic.Translate.Llm.AdvancedTranslatorBase
{
    protected override AdvancedTranslationSettings Settings => Se.Settings.AutoTranslate.LlamaCppAdvanced;
    public AdvancedTranslatorBase() => ApplyResults = action => Dispatcher.UIThread.Invoke(action);
}
