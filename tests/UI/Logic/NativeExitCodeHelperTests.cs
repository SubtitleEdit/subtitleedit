using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;

namespace UITests.Logic;

public class NativeExitCodeHelperTests
{
    private const string Engine = "OmniVoice TTS";

    // Creates a temp folder standing in for an engine install, seeded with the given file names.
    private static string MakeInstallFolder(params string[] fileNames)
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-native-exit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        foreach (var name in fileNames)
        {
            File.WriteAllText(Path.Combine(folder, name), string.Empty);
        }

        return folder;
    }

    [Fact]
    public void Describe_AddsHexForNtStatusCodes()
    {
        // -1073741515 is what the user actually sees in the error dialog (issue #13196).
        Assert.Equal("-1073741515 (0xC0000135)", NativeExitCodeHelper.Describe(NativeExitCodeHelper.StatusDllNotFound));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(139)]
    public void Describe_LeavesOrdinaryExitCodesAlone(int exitCode)
    {
        Assert.Equal(exitCode.ToString(), NativeExitCodeHelper.Describe(exitCode));
    }

    [Fact]
    public void GetHint_ReturnsNullForOrdinaryFailures()
    {
        // A tool that exits 1 has written a real error to stderr - do not bury it under a guess.
        Assert.Null(NativeExitCodeHelper.GetHint(1, Engine));
        Assert.Null(NativeExitCodeHelper.GetHint(0, Engine));
    }

    [Theory]
    [InlineData(unchecked((int)0xC0000135))]
    [InlineData(unchecked((int)0xC0000139))]
    [InlineData(unchecked((int)0xC0000142))]
    public void GetHint_ExplainsLoaderFailuresWithoutAnInstallFolder(int exitCode)
    {
        var hint = NativeExitCodeHelper.GetHint(exitCode, Engine);
        Assert.NotNull(hint);
        Assert.Contains("could not load a DLL", hint);
        Assert.Contains(Engine, hint);
    }

    [Fact]
    public void GetHint_NamesTheMissingCudaRuntimeFiles()
    {
        // Exactly the shipped omnivoice-win64-cuda.zip layout from issue #13196: the CUDA backend
        // is present, the redistributables it imports are not.
        var folder = MakeInstallFolder("omnivoice-tts.exe", "ggml.dll", "ggml-base.dll", "ggml-cpu.dll", "ggml-cuda.dll");
        try
        {
            var hint = NativeExitCodeHelper.GetHint(NativeExitCodeHelper.StatusDllNotFound, Engine, folder);
            Assert.NotNull(hint);
            Assert.Contains("CUDA runtime files are missing", hint);
            Assert.Contains("cudart64_12.dll", hint);
            Assert.Contains("cublas64_12.dll", hint);
            Assert.Contains("cublasLt64_12.dll", hint);
            Assert.Contains(folder, hint);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void GetHint_FallsBackToGenericWhenTheCudaRuntimeIsPresent()
    {
        // Redistributables all there - whatever failed to load, it is not those, so do not claim it is.
        var folder = MakeInstallFolder(
            "ggml-cuda.dll", "cudart64_12.dll", "cublas64_12.dll", "cublasLt64_12.dll");
        try
        {
            var hint = NativeExitCodeHelper.GetHint(NativeExitCodeHelper.StatusDllNotFound, Engine, folder);
            Assert.NotNull(hint);
            Assert.DoesNotContain("CUDA runtime files are missing", hint);
            Assert.Contains("could not load a DLL", hint);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void GetHint_AcceptsCuda13FileNames()
    {
        // A CUDA 13 build renames the redistributables to _13; the glob must not read that as missing.
        var folder = MakeInstallFolder(
            "ggml-cuda.dll", "cudart64_13.dll", "cublas64_13.dll", "cublasLt64_13.dll");
        try
        {
            var hint = NativeExitCodeHelper.GetHint(NativeExitCodeHelper.StatusDllNotFound, Engine, folder);
            Assert.NotNull(hint);
            Assert.DoesNotContain("CUDA runtime files are missing", hint);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void GetHint_ReportsPartiallyMissingCudaRuntime()
    {
        var folder = MakeInstallFolder("ggml-cuda.dll", "cudart64_12.dll");
        try
        {
            var hint = NativeExitCodeHelper.GetHint(NativeExitCodeHelper.StatusDllNotFound, Engine, folder);
            Assert.NotNull(hint);
            Assert.Contains("cublas64_12.dll", hint);
            Assert.Contains("cublasLt64_12.dll", hint);
            Assert.DoesNotContain("cudart64_12.dll", hint);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void GetHint_IgnoresACpuInstallFolder()
    {
        // No GPU backend at all - the folder tells us nothing, so the generic wording stands.
        var folder = MakeInstallFolder("omnivoice-tts.exe", "ggml.dll", "ggml-base.dll", "ggml-cpu.dll");
        try
        {
            var hint = NativeExitCodeHelper.GetHint(NativeExitCodeHelper.StatusDllNotFound, Engine, folder);
            Assert.NotNull(hint);
            Assert.Contains("could not load a DLL", hint);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void GetHint_SurvivesAMissingInstallFolder()
    {
        var hint = NativeExitCodeHelper.GetHint(
            NativeExitCodeHelper.StatusDllNotFound, Engine, Path.Combine(Path.GetTempPath(), "no-such-folder-" + Guid.NewGuid()));
        Assert.NotNull(hint);
        Assert.Contains("could not load a DLL", hint);
    }

    [Fact]
    public void GetHint_ExplainsIllegalInstruction()
    {
        var hint = NativeExitCodeHelper.GetHint(NativeExitCodeHelper.StatusIllegalInstruction, Engine);
        Assert.NotNull(hint);
        Assert.Contains("CPU instructions your processor does not support", hint);
    }

    [Theory]
    [InlineData(unchecked((int)0xC0000005))]
    [InlineData(unchecked((int)0xC0000409))]
    public void GetHint_ExplainsHardCrashes(int exitCode)
    {
        var hint = NativeExitCodeHelper.GetHint(exitCode, Engine);
        Assert.NotNull(hint);
        Assert.Contains("crashed", hint);
    }

    [Fact]
    public void Format_AppendsTheHintWhenThereIsOne()
    {
        var message = NativeExitCodeHelper.Format(Engine, NativeExitCodeHelper.StatusDllNotFound);
        Assert.StartsWith("OmniVoice TTS failed (exit code -1073741515 (0xC0000135)).", message);
        Assert.Contains("could not load a DLL", message);
    }

    [Fact]
    public void Format_StaysTerseForOrdinaryExitCodes()
    {
        Assert.Equal("OmniVoice TTS failed (exit code 3).", NativeExitCodeHelper.Format(Engine, 3));
    }
}
