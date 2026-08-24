using System;
using System.IO;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.UiLogic;

namespace UITests.Logic.Download;

/// <summary>
/// Faster-Whisper-XXL, whisper-ctranslate2 and Const-me Whisper wrote no .installed.sha256
/// sidecar and had no hashes on file, so however well the install went the engine-settings
/// dialog had nothing to identify it by and reported every one of them as an unrecognized
/// build - which reads as a failed install (issue #14057). These tests pin the install record:
/// the key each engine resolves to on the platform it can be downloaded for, the hash of the
/// archive that key is pinned to, and the file-based sidecar write Faster-Whisper-XXL needs
/// because its archive is streamed to disk rather than to memory.
/// </summary>
public class SpeechToTextInstallRecordTests
{
    // SHA-256 of the archives WhisperDownloadService.cs is pinned to.
    private const string PurfviewWindows = "237dee23939cdabfc96ef859fc5e584b842c3a5557e0d2ca744e1f87c14c5844";
    private const string PurfviewLinux = "510ee48ed73a7d4779fa8a7531437513ae109a76d934e983cbdaea3fc248c4f4";
    private const string CTranslate2Windows = "a076c16b184ee1a8b8c87e7765db77d345a30c504a8e98c77b2cf5b069562ccb";
    private const string CTranslate2MacArm64 = "f1c67d47be9216e9998df53d32a59fdaa0310f3e576fa4d9135aa1d579a71f86";
    private const string CTranslate2LinuxX64 = "02c6c1b738a10b8f72fbd581febbbc4f4e60abe96ad1fab76e1b432e2cce041b";
    private const string ConstMeWindows = "baa9b70c824e50fe91f1858006a24b870b7637135659f17fc42beb1af57bd447";

    [Theory]
    [InlineData(DownloadHashManager.PurfviewFasterWhisperXxl.Windows, PurfviewWindows)]
    [InlineData(DownloadHashManager.PurfviewFasterWhisperXxl.Linux, PurfviewLinux)]
    [InlineData(DownloadHashManager.WhisperCTranslate2.Windows, CTranslate2Windows)]
    [InlineData(DownloadHashManager.WhisperCTranslate2.MacArm64, CTranslate2MacArm64)]
    [InlineData(DownloadHashManager.WhisperCTranslate2.LinuxX64, CTranslate2LinuxX64)]
    [InlineData(DownloadHashManager.WhisperConstMe.Windows, ConstMeWindows)]
    public void GetStatus_PinnedArchive_IsUpToDate(string key, string hash)
    {
        // Index 0 of each list must be the archive the download URL currently points at,
        // or a fresh install reports "update available" for the build it just downloaded.
        Assert.Equal(DownloadHashManager.UpdateStatus.UpToDate, DownloadHashManager.GetStatus(key, hash));
    }

    [Fact]
    public void ResolvePurfviewFasterWhisperXxlKey_MatchesDownloadablePlatforms()
    {
        var key = DownloadHashManager.ResolvePurfviewFasterWhisperXxlKey();

        if (OperatingSystem.IsWindows())
        {
            Assert.Equal(DownloadHashManager.PurfviewFasterWhisperXxl.Windows, key);
        }
        else if (OperatingSystem.IsLinux() && !IsArm64)
        {
            Assert.Equal(DownloadHashManager.PurfviewFasterWhisperXxl.Linux, key);
        }
        else
        {
            // macOS and Linux ARM64 have no Faster-Whisper-XXL build to download.
            Assert.Null(key);
        }
    }

    [Fact]
    public void ResolveWhisperCTranslate2Key_MatchesDownloadablePlatforms()
    {
        var key = DownloadHashManager.ResolveWhisperCTranslate2Key();

        if (OperatingSystem.IsWindows())
        {
            Assert.Equal(DownloadHashManager.WhisperCTranslate2.Windows, key);
        }
        else if (OperatingSystem.IsMacOS() && IsArm64)
        {
            Assert.Equal(DownloadHashManager.WhisperCTranslate2.MacArm64, key);
        }
        else if (OperatingSystem.IsLinux() && !IsArm64)
        {
            Assert.Equal(DownloadHashManager.WhisperCTranslate2.LinuxX64, key);
        }
        else
        {
            Assert.Null(key);
        }
    }

    [Fact]
    public void ResolveWhisperConstMeKey_IsWindowsOnly()
    {
        Assert.Equal(
            OperatingSystem.IsWindows() ? DownloadHashManager.WhisperConstMe.Windows : null,
            DownloadHashManager.ResolveWhisperConstMeKey());
    }

    [Fact]
    public void EveryEngineKey_HasAtLeastOneKnownHash()
    {
        // A key with no hashes behind it resolves fine and still leaves the install
        // unidentifiable - exactly the state #14057 was about.
        foreach (var key in new[]
                 {
                     DownloadHashManager.PurfviewFasterWhisperXxl.Windows,
                     DownloadHashManager.PurfviewFasterWhisperXxl.Linux,
                     DownloadHashManager.WhisperCTranslate2.Windows,
                     DownloadHashManager.WhisperCTranslate2.MacArm64,
                     DownloadHashManager.WhisperCTranslate2.LinuxX64,
                     DownloadHashManager.WhisperConstMe.Windows,
                 })
        {
            Assert.NotEmpty(DownloadHashManager.GetKnownHashes(key));
        }
    }

    [Fact]
    public void WriteSidecar_FromArchiveFile_RecordsKeyAndArchiveHash()
    {
        var folder = MakeTempFolder();
        try
        {
            var archive = Path.Combine(folder, "engine.7z");
            File.WriteAllText(archive, "not really a 7-zip archive");
            var expectedHash = Sha256Util.ComputeSha256(archive);

            DownloadHashManager.WriteSidecar(folder, DownloadHashManager.PurfviewFasterWhisperXxl.Windows, archive);

            var sidecar = DownloadHashManager.TryReadSidecar(folder);
            Assert.NotNull(sidecar);
            Assert.Equal(DownloadHashManager.PurfviewFasterWhisperXxl.Windows, sidecar!.Value.Key);
            Assert.Equal(expectedHash, sidecar.Value.Hash);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void WriteSidecar_FromArchiveFile_KnownHashReadsAsUpToDate()
    {
        // The whole point of the sidecar: it survives the archive being deleted after unpacking,
        // and the engine-settings dialog reads the status straight back out of it.
        var folder = MakeTempFolder();
        try
        {
            var archive = Path.Combine(folder, "engine.zip");
            File.WriteAllText(archive, "cli.zip stand-in");
            File.WriteAllText(
                Path.Combine(folder, ".installed.sha256"),
                DownloadHashManager.WhisperConstMe.Windows + Environment.NewLine + ConstMeWindows);

            Assert.Equal(DownloadHashManager.UpdateStatus.UpToDate, DownloadHashManager.GetSidecarStatus(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void WriteSidecar_MissingArchiveOrNoKey_WritesNothing()
    {
        var folder = MakeTempFolder();
        try
        {
            DownloadHashManager.WriteSidecar(folder, DownloadHashManager.PurfviewFasterWhisperXxl.Windows,
                Path.Combine(folder, "does-not-exist.7z"));
            Assert.Null(DownloadHashManager.TryReadSidecar(folder));

            var archive = Path.Combine(folder, "engine.7z");
            File.WriteAllText(archive, "x");
            DownloadHashManager.WriteSidecar(folder, null, archive);
            Assert.Null(DownloadHashManager.TryReadSidecar(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    private static bool IsArm64 =>
        System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture ==
        System.Runtime.InteropServices.Architecture.Arm64;

    private static string MakeTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-install-record-" + Path.GetRandomFileName());
        Directory.CreateDirectory(folder);
        return folder;
    }
}
