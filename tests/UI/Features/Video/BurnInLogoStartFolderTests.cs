using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video;

/// <summary>
/// The burn-in logo picker opened in the last video's folder, though logos are usually kept
/// somewhere else (#16609). It now opens in the current logo's folder, else in the folder of the
/// last logo picked.
/// </summary>
public class BurnInLogoStartFolderTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "se-logo-" + Guid.NewGuid().ToString("N"));
    private readonly string _logos;
    private readonly string _otherLogos;

    public BurnInLogoStartFolderTests()
    {
        _logos = Directory.CreateDirectory(Path.Combine(_root, "logos")).FullName;
        _otherLogos = Directory.CreateDirectory(Path.Combine(_root, "other")).FullName;
    }

    public void Dispose()
    {
        Directory.Delete(_root, true);
    }

    [Fact]
    public void CurrentLogoFolder_WinsOverLastLogoFolder()
    {
        var current = Path.Combine(_logos, "logo.png");

        Assert.Equal(_logos, BurnInLogoViewModel.GetLogoStartFolder(current, _otherLogos));
    }

    [Fact]
    public void NoCurrentLogo_OpensInLastLogoFolder()
    {
        Assert.Equal(_otherLogos, BurnInLogoViewModel.GetLogoStartFolder(string.Empty, _otherLogos));
    }

    [Fact]
    public void CurrentLogoFolderGone_FallsBackToLastLogoFolder()
    {
        var current = Path.Combine(_root, "deleted", "logo.png");

        Assert.Equal(_otherLogos, BurnInLogoViewModel.GetLogoStartFolder(current, _otherLogos));
    }

    [Fact]
    public void NothingUsable_LeavesThePickerDefault()
    {
        Assert.Null(BurnInLogoViewModel.GetLogoStartFolder(null, string.Empty));
        Assert.Null(BurnInLogoViewModel.GetLogoStartFolder(null, Path.Combine(_root, "missing")));
    }

    [Fact]
    public void PickedLogo_IsRememberedForTheNextPicker()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Assert.Equal(string.Empty, Se.Settings.Video.BurnIn.LogoFolder);

            BurnInLogoViewModel.RememberLogoFolder(Path.Combine(_logos, "logo.png"));

            Assert.Equal(_logos, Se.Settings.Video.BurnIn.LogoFolder);
            Assert.Equal(_logos, BurnInLogoViewModel.GetLogoStartFolder(string.Empty, Se.Settings.Video.BurnIn.LogoFolder));
        }
        finally
        {
            Se.Settings = saved;
        }
    }
}
