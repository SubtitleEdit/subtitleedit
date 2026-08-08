using System;
using System.Text;
using Nikse.SubtitleEdit.Features.WebVtt;

namespace UITests.Features.WebVtt;

public class WebVttBrowserPreviewTests
{
    [Theory]
    [InlineData("movie.mp4", true)]
    [InlineData("movie.MP4", true)]
    [InlineData("movie.webm", true)]
    [InlineData("movie.mkv", false)]
    [InlineData("movie.avi", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void OnlyBrowserPlayableContainersAreOffered(string? videoFileName, bool expected)
    {
        Assert.Equal(expected, WebVttBrowserPreview.IsSupportedVideoFile(videoFileName));
    }

    [Fact]
    public void SubtitleIsEmbeddedAsBase64Track()
    {
        var vtt = "WEBVTT" + Environment.NewLine + Environment.NewLine +
                  "00:00:01.000 --> 00:00:02.000" + Environment.NewLine + "Hello";

        var html = WebVttBrowserPreview.GenerateHtml(vtt, "/videos/movie.mp4");

        var expectedBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(vtt));
        Assert.Contains("data:text/vtt;base64," + expectedBase64, html);
    }

    [Theory]
    [InlineData("/videos/movie.mp4", "video/mp4")]
    [InlineData("/videos/movie.webm", "video/webm")]
    [InlineData("/videos/movie.ogv", "video/ogg")]
    public void MimeTypeFollowsTheContainer(string videoFileName, string expectedType)
    {
        var html = WebVttBrowserPreview.GenerateHtml("WEBVTT", videoFileName);

        Assert.Contains(expectedType, html);
    }

    [Fact]
    public void VideoIsReferencedAsAFileUri()
    {
        var html = WebVttBrowserPreview.GenerateHtml("WEBVTT", "/videos/my movie.mp4");

        // A raw path with a space would break the src attribute; it has to be percent-encoded.
        // Only the tail of the URI is portable: Path.GetFullPath() anchors a rooted path to the
        // current drive on Windows, so the preview yields file:///C:/videos/... there and
        // file:///videos/... on Linux and macOS.
        Assert.Contains("src=\"file:///", html);
        Assert.Contains("/videos/my%20movie.mp4", html);
        Assert.DoesNotContain("my movie.mp4", html);
    }
}
