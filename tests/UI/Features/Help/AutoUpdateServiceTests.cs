using Nikse.SubtitleEdit.Features.Help.CheckForUpdates;
using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Tests.Features.Help;

public class AutoUpdateServiceTests
{
    private const string Folder = "/app";

    private static AutoUpdateKind Detect(
        bool isWindows = false,
        bool isLinux = false,
        bool isMacOs = false,
        Architecture architecture = Architecture.X64,
        bool isPackageManaged = false,
        bool writable = true,
        string folder = Folder,
        params string[] files)
    {
        var existing = new HashSet<string>(files.Select(f => f.StartsWith('/') ? f : Path.Combine(folder, f)));
        return AutoUpdateService.DetectKind(isWindows, isLinux, isMacOs, architecture, isPackageManaged, folder, existing.Contains, _ => writable);
    }

    [Fact]
    public void DetectKind_WindowsInnoInstall_IsWindowsInstaller()
    {
        Assert.Equal(AutoUpdateKind.WindowsInstaller, Detect(isWindows: true, files: "unins000.exe"));
    }

    [Fact]
    public void DetectKind_WindowsPortableZip_IsNone()
    {
        Assert.Equal(AutoUpdateKind.None, Detect(isWindows: true, files: "SubtitleEdit.exe"));
    }

    [Fact]
    public void DetectKind_WindowsArm64_IsNone()
    {
        Assert.Equal(AutoUpdateKind.None, Detect(isWindows: true, architecture: Architecture.Arm64, files: "unins000.exe"));
    }

    [Theory]
    [InlineData(Architecture.X64)]
    [InlineData(Architecture.Arm64)]
    public void DetectKind_LinuxWritableTarball_IsLinuxTarball(Architecture architecture)
    {
        Assert.Equal(AutoUpdateKind.LinuxTarball, Detect(isLinux: true, architecture: architecture, files: ["SubtitleEdit", "/usr/bin/tar"]));
    }

    [Fact]
    public void DetectKind_LinuxReadOnlyFolder_IsNone()
    {
        Assert.Equal(AutoUpdateKind.None, Detect(isLinux: true, writable: false, files: ["SubtitleEdit", "/usr/bin/tar"]));
    }

    [Fact]
    public void DetectKind_LinuxFlatpakOrAppImage_IsNone()
    {
        Assert.Equal(AutoUpdateKind.None, Detect(isLinux: true, isPackageManaged: true, files: ["SubtitleEdit", "/usr/bin/tar"]));
    }

    [Fact]
    public void DetectKind_LinuxWithoutTar_IsNone()
    {
        Assert.Equal(AutoUpdateKind.None, Detect(isLinux: true, files: "SubtitleEdit"));
    }

    private const string MacExeFolder = "/Applications/Subtitle Edit.app/Contents/MacOS/";

    [Theory]
    [InlineData(Architecture.X64)]
    [InlineData(Architecture.Arm64)]
    public void DetectKind_MacAppBundleInWritableFolder_IsMacAppBundle(Architecture architecture)
    {
        Assert.Equal(AutoUpdateKind.MacAppBundle, Detect(isMacOs: true, architecture: architecture, folder: MacExeFolder));
    }

    [Fact]
    public void DetectKind_MacReadOnlyFolder_IsNone()
    {
        Assert.Equal(AutoUpdateKind.None, Detect(isMacOs: true, writable: false, folder: MacExeFolder));
    }

    [Fact]
    public void DetectKind_MacTranslocated_IsNone()
    {
        Assert.Equal(AutoUpdateKind.None, Detect(isMacOs: true,
            folder: "/private/var/folders/x/AppTranslocation/ABC/d/Subtitle Edit.app/Contents/MacOS/"));
    }

    [Fact]
    public void DetectKind_MacNotInBundle_IsNone()
    {
        Assert.Equal(AutoUpdateKind.None, Detect(isMacOs: true, folder: "/Users/me/se/bin/Debug/net10.0/"));
    }

    [Theory]
    [InlineData("/Applications/Subtitle Edit.app/Contents/MacOS/", "/Applications/Subtitle Edit.app")]
    [InlineData("/Users/me/Apps/SE.app/Contents/MacOS", "/Users/me/Apps/SE.app")]
    [InlineData("/Users/me/se/bin/Debug/net10.0/", null)]
    [InlineData("/Applications/Subtitle Edit/Contents/MacOS/", null)]
    public void GetMacAppBundle(string exeFolder, string? expected)
    {
        Assert.Equal(expected, AutoUpdateService.GetMacAppBundle(exeFolder));
    }

    [Theory]
    [InlineData("Identifier=dk.nikse.subtitleedit\nAuthority=Developer ID Application: X (MFC9L2T4RB)\nTeamIdentifier=MFC9L2T4RB\n", "MFC9L2T4RB")]
    [InlineData("Identifier=x\nSignature=adhoc\nTeamIdentifier=not set\n", "")]
    [InlineData("code object is not signed at all\n", "")]
    public void ParseTeamIdentifier(string output, string expected)
    {
        Assert.Equal(expected, AutoUpdateService.ParseTeamIdentifier(output));
    }

    [Theory]
    [InlineData(AutoUpdateKind.WindowsInstaller, Architecture.X64, "SubtitleEdit-Windows-x64-Setup.exe")]
    [InlineData(AutoUpdateKind.LinuxTarball, Architecture.X64, "SubtitleEdit-Linux-x64.tar.gz")]
    [InlineData(AutoUpdateKind.LinuxTarball, Architecture.Arm64, "SubtitleEdit-Linux-ARM64.tar.gz")]
    [InlineData(AutoUpdateKind.MacAppBundle, Architecture.X64, "SubtitleEdit-macOS-x64.dmg")]
    [InlineData(AutoUpdateKind.MacAppBundle, Architecture.Arm64, "SubtitleEdit-macOS-ARM64.dmg")]
    [InlineData(AutoUpdateKind.None, Architecture.X64, "")]
    public void GetAssetName(AutoUpdateKind kind, Architecture architecture, string expected)
    {
        Assert.Equal(expected, AutoUpdateService.GetAssetName(kind, architecture));
    }

    private const string Digest = "93b487e115560da33c4557c780d387e311d93a6ab6abe000093576ccb9d44889";

    private static string MakeReleaseJson(string name, string url, string digest) =>
        $$"""
          {"tag_name":"v5.3.0","assets":[
            {"name":"SubtitleEdit-macOS-x64.dmg","browser_download_url":"https://github.com/SubtitleEdit/subtitleedit/releases/download/v5.3.0/SubtitleEdit-macOS-x64.dmg","digest":"sha256:{{Digest}}"},
            {"name":"{{name}}","browser_download_url":"{{url}}","digest":"{{digest}}"}]}
          """;

    [Fact]
    public void ParseReleaseAsset_PicksNamedAssetWithDigest()
    {
        const string url = "https://github.com/SubtitleEdit/subtitleedit/releases/download/v5.3.0/SubtitleEdit-Linux-x64.tar.gz";
        var json = MakeReleaseJson("SubtitleEdit-Linux-x64.tar.gz", url, "sha256:" + Digest.ToUpperInvariant());

        var asset = AutoUpdateService.ParseReleaseAsset(json, "SubtitleEdit-Linux-x64.tar.gz");

        Assert.NotNull(asset);
        Assert.Equal(url, asset.DownloadUrl);
        Assert.Equal(Digest, asset.Sha256);
    }

    [Fact]
    public void ParseReleaseAsset_NoDigest_ReturnsNull()
    {
        var json = MakeReleaseJson("SubtitleEdit-Linux-x64.tar.gz",
            "https://github.com/SubtitleEdit/subtitleedit/releases/download/v5.3.0/SubtitleEdit-Linux-x64.tar.gz", "");

        Assert.Null(AutoUpdateService.ParseReleaseAsset(json, "SubtitleEdit-Linux-x64.tar.gz"));
    }

    [Fact]
    public void ParseReleaseAsset_ForeignUrl_ReturnsNull()
    {
        var json = MakeReleaseJson("SubtitleEdit-Linux-x64.tar.gz", "https://example.com/SubtitleEdit-Linux-x64.tar.gz", "sha256:" + Digest);

        Assert.Null(AutoUpdateService.ParseReleaseAsset(json, "SubtitleEdit-Linux-x64.tar.gz"));
    }

    [Fact]
    public void ParseReleaseAsset_MissingAsset_ReturnsNull()
    {
        var json = MakeReleaseJson("SubtitleEdit-Linux-x64.tar.gz",
            "https://github.com/SubtitleEdit/subtitleedit/releases/download/v5.3.0/SubtitleEdit-Linux-x64.tar.gz", "sha256:" + Digest);

        Assert.Null(AutoUpdateService.ParseReleaseAsset(json, "SubtitleEdit-Windows-x64-Setup.exe"));
    }

    [Fact]
    public async Task IsSha256MatchAsync_ComparesCaseInsensitive()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "hello");
            var hash = Convert.ToHexString(SHA256.HashData("hello"u8.ToArray()));

            Assert.True(await AutoUpdateService.IsSha256MatchAsync(path, hash.ToLowerInvariant(), CancellationToken.None));
            Assert.False(await AutoUpdateService.IsSha256MatchAsync(path, Digest, CancellationToken.None));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string MakeTarball(params TarEntry[] entries)
    {
        var path = Path.GetTempFileName();
        using var file = File.Create(path);
        using var gzip = new GZipStream(file, CompressionLevel.Fastest);
        using var writer = new TarWriter(gzip);
        foreach (var entry in entries)
        {
            writer.WriteEntry(entry);
        }

        return path;
    }

    private static PaxTarEntry Entry(string name) => new(TarEntryType.RegularFile, name) { DataStream = new MemoryStream([1]) };

    [Fact]
    public void ValidateLinuxTarball_PublishLayout_Passes()
    {
        var path = MakeTarball(new PaxTarEntry(TarEntryType.Directory, "./"), Entry("./SubtitleEdit"), Entry("./libSkiaSharp.so"));
        try
        {
            AutoUpdateService.ValidateLinuxTarball(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("../evil")]
    [InlineData("/etc/evil")]
    [InlineData("./sub/../../evil")]
    public void ValidateLinuxTarball_UnsafePath_Throws(string name)
    {
        var path = MakeTarball(Entry("./SubtitleEdit"), Entry(name));
        try
        {
            Assert.Throws<InvalidDataException>(() => AutoUpdateService.ValidateLinuxTarball(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ValidateLinuxTarball_SymbolicLink_Throws()
    {
        var path = MakeTarball(Entry("./SubtitleEdit"), new PaxTarEntry(TarEntryType.SymbolicLink, "./link") { LinkName = "/etc/passwd" });
        try
        {
            Assert.Throws<InvalidDataException>(() => AutoUpdateService.ValidateLinuxTarball(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ValidateLinuxTarball_NoExecutable_Throws()
    {
        var path = MakeTarball(Entry("./readme.txt"));
        try
        {
            Assert.Throws<InvalidDataException>(() => AutoUpdateService.ValidateLinuxTarball(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LinuxScript_ExtractsOverFolderAfterExitAndRelaunches()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var root = Directory.CreateTempSubdirectory("se-update-test").FullName;
        try
        {
            var appFolder = Directory.CreateDirectory(Path.Combine(root, "app")).FullName;
            await File.WriteAllTextAsync(Path.Combine(appFolder, "old.txt"), "old");
            await File.WriteAllTextAsync(Path.Combine(appFolder, "version.txt"), "1");

            // The "new version": its executable records that it was started from the install folder.
            var staging = Directory.CreateDirectory(Path.Combine(root, "staging")).FullName;
            var marker = Path.Combine(root, "relaunched.txt");
            await File.WriteAllTextAsync(Path.Combine(staging, "SubtitleEdit"), $"#!/bin/sh\npwd > '{marker}'\n");
            File.SetUnixFileMode(Path.Combine(staging, "SubtitleEdit"), (UnixFileMode)0b111_101_101);
            await File.WriteAllTextAsync(Path.Combine(staging, "version.txt"), "2");
            var archive = Path.Combine(root, "update.tar.gz");
            await RunAsync("tar", "-czf", archive, "-C", staging, ".");

            var exited = System.Diagnostics.Process.Start("/bin/sh", ["-c", "exit 0"]);
            await exited.WaitForExitAsync();

            var script = Path.Combine(root, "install-update.sh");
            await File.WriteAllTextAsync(script, AutoUpdateService.LinuxScript.Replace("\r\n", "\n"));
            await RunAsync("/bin/sh", script, exited.Id.ToString(), archive, appFolder, Path.Combine(appFolder, "SubtitleEdit"));

            for (var i = 0; i < 50 && !File.Exists(marker); i++)
            {
                await Task.Delay(100);
            }

            Assert.Equal("2", await File.ReadAllTextAsync(Path.Combine(appFolder, "version.txt")));
            Assert.True(File.Exists(Path.Combine(appFolder, "old.txt"))); // user files in the folder survive
            Assert.False(File.Exists(archive));
            Assert.True(File.Exists(marker));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task MacScript_SwapsBundleAfterExitAndOpensIt()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var root = Directory.CreateTempSubdirectory("se-update-test").FullName;
        try
        {
            var target = Directory.CreateDirectory(Path.Combine(root, "Applications", "Subtitle Edit.app", "Contents")).Parent!.FullName;
            await File.WriteAllTextAsync(Path.Combine(target, "Contents", "version.txt"), "1");
            var staged = Directory.CreateDirectory(Path.Combine(root, "staged", "Subtitle Edit.app", "Contents")).Parent!.FullName;
            await File.WriteAllTextAsync(Path.Combine(staged, "Contents", "version.txt"), "2");

            // A fake "open" first on PATH records what would be launched.
            var bin = Directory.CreateDirectory(Path.Combine(root, "bin")).FullName;
            var marker = Path.Combine(root, "opened.txt");
            await File.WriteAllTextAsync(Path.Combine(bin, "open"), $"#!/bin/sh\necho \"$1\" > '{marker}'\n");
            File.SetUnixFileMode(Path.Combine(bin, "open"), (UnixFileMode)0b111_101_101);

            var exited = System.Diagnostics.Process.Start("/bin/sh", ["-c", "exit 0"]);
            await exited.WaitForExitAsync();

            var script = Path.Combine(root, "install-update.sh");
            await File.WriteAllTextAsync(script, AutoUpdateService.MacScript.Replace("\r\n", "\n"));
            var startInfo = new System.Diagnostics.ProcessStartInfo("/bin/sh", [script, exited.Id.ToString(), staged, target]);
            startInfo.Environment["PATH"] = bin + ":" + Environment.GetEnvironmentVariable("PATH");
            using (var process = System.Diagnostics.Process.Start(startInfo)!)
            {
                await process.WaitForExitAsync();
                Assert.Equal(0, process.ExitCode);
            }

            Assert.Equal("2", await File.ReadAllTextAsync(Path.Combine(target, "Contents", "version.txt")));
            Assert.False(Directory.Exists(staged));
            Assert.False(Directory.Exists(target + ".se-update-old"));
            Assert.Equal(target, (await File.ReadAllTextAsync(marker)).Trim());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static async Task RunAsync(string fileName, params string[] arguments)
    {
        using var process = System.Diagnostics.Process.Start(fileName, arguments);
        await process.WaitForExitAsync();
        Assert.Equal(0, process.ExitCode);
    }
}
