using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The pieces of the audio.cpp install shared by every engine that runs on it (IndexTTS 2.5,
/// Higgs Audio v3, Fish Audio S2 Pro): one binaries folder, one server executable, and one
/// backend marker. The binaries are downloaded once and reused by all of them; each engine
/// still runs its own server process on its own loopback port with its own model.
/// </summary>
public static class AudioCppRuntime
{
    /// <summary>
    /// Where the audio.cpp binaries live: <c>&lt;data&gt;/audio.cpp/</c>, a top-level folder like
    /// CrispASR and llama.cpp — audio.cpp is a whole runtime rather than one model's engine.
    /// </summary>
    public static string GetSetEngineFolder()
    {
        var folder = Se.AudioCppFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetServerExecutable() =>
        Path.Combine(GetSetEngineFolder(), OperatingSystem.IsWindows() ? "audiocpp_server.exe" : "audiocpp_server");

    /// <summary>
    /// Identity of the server binary on disk (size + last write), or empty when it is missing.
    /// The engines record it when they launch a server and restart when it changes: an engine
    /// update extracts a new binary while the old process keeps running, and reusing that
    /// process means the new build (and any family it adds) is never actually used.
    /// </summary>
    public static string GetServerExecutableStamp()
    {
        try
        {
            var fi = new FileInfo(GetServerExecutable());
            return fi.Exists ? $"{fi.Length}:{fi.LastWriteTimeUtc.Ticks}" : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// ggml backend the installed archive was built for, stored at install time. The setting
    /// keeps its historical IndexTts25-prefixed name — it predates the runtime being shared —
    /// and renaming it would throw away every existing user's stored backend choice.
    /// </summary>
    public static string GetBackend()
    {
        var saved = Se.Settings.Video.TextToSpeech.IndexTts25AudioCppBackend;
        if (!string.IsNullOrEmpty(saved))
        {
            return saved;
        }

        return OperatingSystem.IsMacOS() ? "metal" : "cpu";
    }

    /// <summary>
    /// Turns the exit codes that mean "this build cannot run on this machine" into an
    /// actionable message, since the process dies in the loader before it can print anything
    /// useful of its own:
    ///  - Windows 0xC0000135 / -1073741515 (STATUS_DLL_NOT_FOUND): a GPU build without its
    ///    runtime. The Vulkan binaries import vulkan-1.dll (from the GPU driver) at load time.
    ///  - Linux 127: the dynamic loader could not find a shared library. The Linux CUDA
    ///    archive does NOT bundle libcudart.so.12 / libcublas.so.12 the way the Windows CUDA
    ///    zip bundles its DLLs, so it needs a system CUDA 12 runtime.
    /// </summary>
    public static string DescribeStartupExit(int exitCode, string backend) => exitCode switch
    {
        -1073741515 => $"The {backend} build could not load its GPU runtime library. "
            + "Re-download the engine and pick the CPU variant.",
        -1073741795 => "The CPU build uses instructions this processor does not have.",
        127 when string.Equals(backend, "cuda", StringComparison.OrdinalIgnoreCase) =>
            "The Linux CUDA build needs the CUDA 12 runtime (libcudart.so.12 and libcublas.so.12) "
            + "installed on this system. Install the CUDA 12 runtime, or re-download the engine "
            + "and pick the CPU or Vulkan variant.",
        127 => $"The {backend} build could not load a shared library it needs.",
        _ => string.Empty,
    };
}
