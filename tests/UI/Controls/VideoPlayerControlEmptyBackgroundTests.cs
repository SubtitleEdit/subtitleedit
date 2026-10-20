using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// The theme art in the empty video area sits on top of the player surface. A native player
/// window (mpv "wid", the Windows default) is drawn above everything Avalonia draws, so it has
/// to be hidden while the art shows - and shown again as soon as a video opens.
/// </summary>
public class VideoPlayerControlEmptyBackgroundTests
{
    private static WriteableBitmap MakeArt() => new(new PixelSize(2, 2), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);

    private static (ContentPresenter Presenter, Image Art) GetParts(VideoPlayerControl control)
    {
        var grid = Assert.IsType<Grid>(control.Content);
        return (grid.Children.OfType<ContentPresenter>().Single(), grid.Children.OfType<Image>().Single());
    }

    [AvaloniaFact]
    public void NativePlayerWindowIsHiddenWhileTheArtShows()
    {
        var control = new VideoPlayerControl(new EmptyVideoPlayer()) { PlayerContent = new NativeControlHost() };
        var (presenter, art) = GetParts(control);

        control.EmptyBackground = MakeArt();

        Assert.True(art.IsVisible);
        Assert.False(presenter.IsVisible);
    }

    [AvaloniaFact]
    public async Task OpeningAVideoShowsTheNativePlayerWindowAgain()
    {
        var control = new VideoPlayerControl(new EmptyVideoPlayer()) { PlayerContent = new NativeControlHost() };
        var (presenter, art) = GetParts(control);
        control.EmptyBackground = MakeArt();

        await control.Open("fake.mkv");

        Assert.False(art.IsVisible);
        Assert.True(presenter.IsVisible);

        control.Close();

        Assert.True(art.IsVisible);
        Assert.False(presenter.IsVisible);
    }

    [AvaloniaFact]
    public void ArtBeforeAnOpenThatIsStillLoadingStaysHidden()
    {
        // The rebuild after Settings sets the art once the reopened video is up; a later set
        // must not bring the art back over a video that is open
        var control = new VideoPlayerControl(new EmptyVideoPlayer()) { PlayerContent = new NativeControlHost() };
        var (presenter, art) = GetParts(control);
        _ = control.Open("fake.mkv");

        control.EmptyBackground = MakeArt();

        Assert.False(art.IsVisible);
        Assert.True(presenter.IsVisible);
    }

    [AvaloniaFact]
    public void ComposedPlayerSurfaceStaysVisibleUnderTheArt()
    {
        // OpenGL/software renderers are drawn by Avalonia, so the art simply covers them
        var control = new VideoPlayerControl(new EmptyVideoPlayer()) { PlayerContent = new Border() };
        var (presenter, art) = GetParts(control);

        control.EmptyBackground = MakeArt();

        Assert.True(art.IsVisible);
        Assert.True(presenter.IsVisible);
    }

    [AvaloniaFact]
    public void NoArtKeepsTheNativePlayerWindowVisible()
    {
        var control = new VideoPlayerControl(new EmptyVideoPlayer()) { PlayerContent = new NativeControlHost() };
        var (presenter, art) = GetParts(control);

        control.EmptyBackground = null;

        Assert.False(art.IsVisible);
        Assert.True(presenter.IsVisible);
    }
}
