using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

public class MissingSharedLibraryTests
{
    [Fact]
    public void GetName_GlibcLoaderError_ReturnsLibraryName()
    {
        // The exact line from issue #12970 (Flatpak, CrispASR Qwen3)
        const string line = "/home/user/.var/app/dk.nikse.subtitleedit/config/Subtitle Edit/CrispASR/crispasr: " +
                            "error while loading shared libraries: libopenblas.so.0: " +
                            "cannot open shared object file: No such file or directory";

        Assert.Equal("libopenblas.so.0", MissingSharedLibrary.GetName(line));
    }

    [Fact]
    public void GetName_DyldLoaderError_ReturnsLibraryName()
    {
        const string line = "dyld: Library not loaded: @rpath/libc2pa_c.dylib";

        Assert.Equal("@rpath/libc2pa_c.dylib", MissingSharedLibrary.GetName(line));
    }

    [Theory]
    [InlineData("whisper_init_from_file_with_params_no_state: loading model from 'ggml-base.bin'")]
    [InlineData("error: unknown argument: --nonsense")]
    [InlineData("")]
    [InlineData(null)]
    public void GetName_UnrelatedOutput_ReturnsNull(string? line)
    {
        Assert.Null(MissingSharedLibrary.GetName(line));
    }

    [Fact]
    public void GetName_WhisperCppLoaderError_ReturnsLibraryName()
    {
        // The exact line from issue #13680 (EndeavourOS, whisper.cpp)
        const string line = "/home/user/.config/Subtitle Edit/SpeechToText/Cpp/whisper-cli: " +
                            "error while loading shared libraries: libwhisper.so.1: " +
                            "cannot open shared object file: No such file or directory";

        Assert.Equal("libwhisper.so.1", MissingSharedLibrary.GetName(line));
    }

    [Theory]
    [InlineData("libwhisper.so.1")]
    [InlineData("libggml.so.0")]
    [InlineData("libggml-base.so.0")]
    [InlineData("libllama.so")]
    [InlineData("@rpath/libwhisper.1.dylib")]
    public void IsBundledWithEngine_EngineOwnedLibrary_ReturnsTrue(string libraryName)
    {
        Assert.True(MissingSharedLibrary.IsBundledWithEngine(libraryName));
    }

    [Theory]
    [InlineData("libopenblas.so.0")]
    [InlineData("libvulkan.so.1")]
    [InlineData("libcudart.so.12")]
    [InlineData("libstdc++.so.6")]
    [InlineData("")]
    [InlineData(null)]
    public void IsBundledWithEngine_SystemLibrary_ReturnsFalse(string? libraryName)
    {
        Assert.False(MissingSharedLibrary.IsBundledWithEngine(libraryName));
    }
}
