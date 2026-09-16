using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace UITests.Features.Translate;

/// <summary>
/// The last-used engine is stored by display name. "API-Route" became "API Route" (c58837aba),
/// so a settings file from before the rename must still select that engine - in the
/// Auto-translate window and in batch convert - instead of falling back to the default.
/// </summary>
public class AutoTranslateLegacyEngineNameTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [Fact]
    public void LegacyApiRouteName_MapsToTheCurrentName()
    {
        Assert.Equal("API Route", AutoTranslateEngineNames.FromStored("API-Route"));
        Assert.Equal(ApiRouteTranslate.StaticName, AutoTranslateEngineNames.FromStored("API Route"));
        Assert.Equal("Google Translate V1 API", AutoTranslateEngineNames.FromStored("Google Translate V1 API"));
        Assert.Equal(string.Empty, AutoTranslateEngineNames.FromStored(null));
    }

    [AvaloniaFact]
    public void AutoTranslateWindow_RestoresApiRouteStoredUnderItsOldName()
    {
        using var _ = new SettingsScope("AutoTranslate.AutoTranslateLastName");
        Se.Settings.AutoTranslate.AutoTranslateLastName = "API-Route";

        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello there.", 0, 2000));
        vm.Initialize(subtitle);
        vm.OnLoaded();
        Dispatcher.UIThread.RunJobs();

        Assert.IsType<ApiRouteTranslate>(vm.SelectedAutoTranslator);
    }

    [AvaloniaFact]
    public void BatchConvert_StillRestoresAnEngineStoredUnderItsCurrentName()
    {
        // Batch convert does not offer API Route, so the alias cannot be exercised there; this
        // pins that routing the stored name through the alias map leaves other engines alone.
        using var _ = new SettingsScope("Tools.BatchConvert.AutoTranslateEngine");
        Se.Settings.Tools.BatchConvert.AutoTranslateEngine = new DeepLTranslate().Name;

        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<BatchConvertViewModel>();

        Assert.IsType<DeepLTranslate>(vm.SelectedAutoTranslator);
    }
}
