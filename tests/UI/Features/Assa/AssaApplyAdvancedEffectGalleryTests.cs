using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect;
using Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Assa;

public class AssaApplyAdvancedEffectGalleryTests
{
    private static AssaApplyAdvancedEffectViewModel MakeViewModel(params string[] texts)
    {
        var vm = new AssaApplyAdvancedEffectViewModel();
        var lines = texts.Select((t, i) => new SubtitleLineViewModel
        {
            Text = t,
            StartTime = TimeSpan.FromSeconds(i * 3),
            EndTime = TimeSpan.FromSeconds(i * 3 + 2),
        }).ToList();
        vm.Initialize(new Subtitle(), lines, new List<SubtitleLineViewModel>(), null, null, null);
        return vm;
    }

    [Fact]
    public void EveryEffect_HasPreviewKind()
    {
        foreach (var effect in AdvancedEffectDisplayFactory.List())
        {
            Assert.NotEqual(EffectPreviewKind.None, AdvancedEffectCatalog.GetPreviewKind(effect));
        }
    }

    [AvaloniaFact]
    public void CategoryFilter_ShowsOnlyThatCategory()
    {
        var vm = MakeViewModel("Hello world");
        vm.SelectedCategory = vm.Categories.First(c => c.Category == AdvancedEffectCategory.Scenes);

        Assert.NotEmpty(vm.FilteredEffects);
        Assert.All(vm.FilteredEffects, e => Assert.Equal(AdvancedEffectCategory.Scenes, AdvancedEffectCatalog.GetCategory(e)));
        Assert.Contains(vm.FilteredEffects, e => e is AdvancedEffectSnow);
    }

    [AvaloniaFact]
    public void AllCategory_CountsAvailableEffects_WithoutAudioOnes()
    {
        var vm = MakeViewModel("Hello world");

        // no waveform - the audio pulse effect is removed, and the "All" count follows
        Assert.DoesNotContain(vm.OverrideTags, e => e.UsesAudio);
        Assert.Equal(vm.OverrideTags.Count, vm.Categories.First(c => c.Category == AdvancedEffectCategory.All).Count);
        Assert.Equal(vm.OverrideTags.Count, vm.FilteredEffects.Count);
    }

    [AvaloniaFact]
    public void Search_HidingSelectedEffect_KeepsItSelected()
    {
        var vm = MakeViewModel("Hello world");
        var snow = vm.OverrideTags.First(e => e is AdvancedEffectSnow);
        vm.SelectedOverrideTag = snow;
        Assert.Same(snow, vm.SelectedGalleryEffect);

        vm.SearchText = "typewriter";

        Assert.All(vm.FilteredEffects, e => Assert.Contains("typewriter", e.Name, StringComparison.OrdinalIgnoreCase));
        Assert.Null(vm.SelectedGalleryEffect);
        Assert.Same(snow, vm.SelectedOverrideTag);

        vm.SearchText = "zzz-no-such-effect";
        Assert.True(vm.HasNoMatches);

        vm.SearchText = string.Empty;
        Assert.False(vm.HasNoMatches);
        Assert.Same(snow, vm.SelectedGalleryEffect);
    }

    [AvaloniaFact]
    public void WordPop_AutoDetectsWords_WhenNoActiveWordMarkup()
    {
        var plain = MakeViewModel("Hello world");
        Assert.True(plain.OverrideTags.OfType<AdvancedEffectFancyKaraoke>().Single().AutoDetectActiveWord);

        var marked = MakeViewModel("Hello {\\u1}world{\\u0}");
        Assert.False(marked.OverrideTags.OfType<AdvancedEffectFancyKaraoke>().Single().AutoDetectActiveWord);
    }

    [AvaloniaFact]
    public void PreviewText_FollowsSelectedLine_ButNotForRightToLeft()
    {
        var vm = MakeViewModel("{\\i1}Hello{\\i0}\\Nworld", "שלום עולם");

        vm.SelectedParagraphIndex = 0;
        Assert.Equal("Hello world", vm.PreviewText);

        vm.SelectedParagraphIndex = 1;
        Assert.Equal("Subtitle Edit", vm.PreviewText);
    }
}
