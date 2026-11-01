using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// Issue #16540: the toolbar's format list has the favorites on top, but the "Save as" dialog
/// built its own list from all formats, so favorites were buried among the other formats.
/// </summary>
public class SaveFilePickerFileTypesTests
{
    [Fact]
    public void SaveAsList_HasCurrentFormatFirst_ThenFavoritesInSettingsOrder()
    {
        using var _ = new SettingsScope("General.FavoriteSubtitleFormats");
        var favorites = new SubtitleFormat[] { new WebVTT(), new SubStationAlpha(), new AdvancedSubStationAlpha() };
        Se.Settings.General.FavoriteSubtitleFormats = string.Join(";", favorites.Select(f => f.FriendlyName));

        var names = FileHelper.MakeSaveFilePickerAllFileTypes(new SubRip()).Select(f => f.Name).ToList();

        Assert.Equal(
            new[] { new SubRip().Name, new WebVTT().Name, new SubStationAlpha().Name, new AdvancedSubStationAlpha().Name },
            names.Take(4));
    }

    [Fact]
    public void SaveAsList_ListsCurrentFormatOnce_WhenItIsAlsoAFavorite()
    {
        using var _ = new SettingsScope("General.FavoriteSubtitleFormats");
        var current = new SubRip();
        Se.Settings.General.FavoriteSubtitleFormats = new WebVTT().FriendlyName + ";" + current.FriendlyName;

        var names = FileHelper.MakeSaveFilePickerAllFileTypes(current).Select(f => f.Name).ToList();

        Assert.Equal(current.Name, names[0]);
        Assert.Equal(new WebVTT().Name, names[1]);
        Assert.Single(names, n => n == current.Name);
    }

    [Fact]
    public void SaveAsList_KeepsLeavingOutBinaryFavorites_ButNotEbu()
    {
        using var _ = new SettingsScope("General.FavoriteSubtitleFormats");
        Se.Settings.General.FavoriteSubtitleFormats = new Pac().FriendlyName + ";" + new Ebu().FriendlyName;

        var names = FileHelper.MakeSaveFilePickerAllFileTypes(new SubRip()).Select(f => f.Name).ToList();

        Assert.DoesNotContain(new Pac().Name, names);
        Assert.Equal(new Ebu().Name, names[1]);
    }
}
