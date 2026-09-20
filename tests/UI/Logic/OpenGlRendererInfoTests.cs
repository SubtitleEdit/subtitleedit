using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Xunit;

namespace Tests.Logic;

public class OpenGlRendererInfoTests
{
    [Theory]
    [InlineData("llvmpipe (LLVM 21.1.8, 256 bits)", true)]
    [InlineData("softpipe", true)]
    [InlineData("Mesa Software Rasterizer", true)]
    [InlineData("Google SwiftShader", true)]
    [InlineData("NVIDIA GeForce RTX 4070/PCIe/SSE2", false)]
    [InlineData("Apple M2", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsSoftwareRenderer_DetectsKnownRasterizers(string? renderer, bool expected)
    {
        Assert.Equal(expected, OpenGlRendererInfo.IsSoftwareRenderer(renderer));
    }

    [Fact]
    public void GetPlayerSubName_HardwareRendererStaysPlainOpenGl()
    {
        Assert.Equal("OpenGL", OpenGlRendererInfo.GetPlayerSubName("NVIDIA GeForce RTX 4070/PCIe/SSE2"));
        Assert.Equal("OpenGL", OpenGlRendererInfo.GetPlayerSubName(null));
    }

    [Fact]
    public void GetPlayerSubName_SoftwareRendererShowsShortName()
    {
        Assert.Equal("OpenGL (llvmpipe)", OpenGlRendererInfo.GetPlayerSubName("llvmpipe (LLVM 21.1.8, 256 bits)"));
        Assert.Equal("OpenGL (softpipe)", OpenGlRendererInfo.GetPlayerSubName("softpipe"));
    }
}
