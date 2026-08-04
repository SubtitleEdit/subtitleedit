using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Turns raw exit codes from bundled native helper processes (omnivoice-tts, qwen3-tts-server,
/// kokoro-tts-server, ...) into something a user can act on.
///
/// A process that dies inside the Windows loader never gets to write a single line to stderr, so
/// all the engine wrappers can report is a large negative number. 0xC0000135 (STATUS_DLL_NOT_FOUND)
/// shows up as -1073741515, which tells the user nothing - see issue #13196, where the CUDA build of
/// OmniVoice shipped without the CUDA redistributables and every run failed with exactly that.
/// </summary>
public static class NativeExitCodeHelper
{
    public const int StatusAccessViolation = unchecked((int)0xC0000005);
    public const int StatusIllegalInstruction = unchecked((int)0xC000001D);
    public const int StatusDllNotFound = unchecked((int)0xC0000135);
    public const int StatusEntryPointNotFound = unchecked((int)0xC0000139);
    public const int StatusDllInitFailed = unchecked((int)0xC0000142);
    public const int StatusStackBufferOverrun = unchecked((int)0xC0000409);

    // ggml names its GPU backends consistently across all the bundled *.cpp engines, so the same
    // pair of probes works for every one of them.
    private const string CudaBackendDll = "ggml-cuda.dll";
    private const string VulkanBackendDll = "ggml-vulkan.dll";

    // ggml-cuda.dll imports cudart64_*.dll and cublas64_*.dll; cuBLAS then pulls in cublasLt64_*.dll.
    // Searched by glob rather than pinned to _12 so a CUDA 13 build - which renames them to _13 - is
    // not reported as broken; the example name is only for the message, where a concrete file the
    // user can look for beats a wildcard.
    private static readonly (string Glob, string Example)[] CudaRuntimeDlls =
    {
        ("cudart64_*.dll", "cudart64_12.dll"),
        ("cublas64_*.dll", "cublas64_12.dll"),
        ("cublasLt64_*.dll", "cublasLt64_12.dll"),
    };

    /// <summary>
    /// Formats an exit code for display, adding the hex form for values that look like a Windows
    /// NTSTATUS (the 0xC00000xx range) since those are only recognisable in hex.
    /// </summary>
    public static string Describe(int exitCode)
    {
        return IsNtStatus(exitCode)
            ? $"{exitCode} (0x{(uint)exitCode:X8})"
            : exitCode.ToString();
    }

    /// <summary>
    /// A sentence explaining what the exit code means and what to try, or null when the code carries
    /// no useful meaning (a plain non-zero exit from the tool itself - stderr is better there).
    /// </summary>
    /// <param name="exitCode">Exit code of the native process.</param>
    /// <param name="engineName">User-facing engine name, e.g. "OmniVoice TTS".</param>
    /// <param name="installFolder">Engine install folder, probed to name the actual missing files.</param>
    public static string? GetHint(int exitCode, string engineName, string? installFolder = null)
    {
        switch (exitCode)
        {
            case StatusDllNotFound:
            case StatusEntryPointNotFound:
            case StatusDllInitFailed:
                return GetMissingDllHint(engineName, installFolder);

            case StatusIllegalInstruction:
                return $"{engineName} was stopped by an illegal instruction, which means this build "
                       + "uses CPU instructions your processor does not support. Re-download the engine "
                       + "and pick a different build.";

            case StatusAccessViolation:
            case StatusStackBufferOverrun:
                return $"{engineName} crashed. If it is a GPU build, re-download the engine and try the "
                       + "CPU build, which is slower but far less fragile.";

            default:
                return null;
        }
    }

    /// <summary>
    /// "OmniVoice TTS failed (exit code -1073741515 (0xC0000135)). &lt;hint&gt;" - the hint is appended
    /// only when the code is one we can explain.
    /// </summary>
    public static string Format(string engineName, int exitCode, string? installFolder = null)
    {
        var message = $"{engineName} failed (exit code {Describe(exitCode)}).";
        var hint = GetHint(exitCode, engineName, installFolder);
        return hint == null ? message : $"{message} {hint}";
    }

    private static string GetMissingDllHint(string engineName, string? installFolder)
    {
        var missing = FindMissingGpuRuntime(installFolder);
        if (missing != null)
        {
            return $"{engineName} could not start because {missing}";
        }

        return $"{engineName} could not start: Windows could not load a DLL it needs. That is usually a "
               + "missing GPU runtime (CUDA or Vulkan) or a missing Microsoft Visual C++ Redistributable. "
               + "Re-download the engine and pick the CPU build, which has no such dependencies.";
    }

    /// <summary>
    /// Best-effort look at what the install folder is missing, so the message can name real files
    /// instead of guessing. Returns null when the folder looks fine or cannot be inspected - callers
    /// then fall back to the generic wording.
    /// </summary>
    private static string? FindMissingGpuRuntime(string? installFolder)
    {
        if (string.IsNullOrEmpty(installFolder) || !Directory.Exists(installFolder))
        {
            return null;
        }

        try
        {
            if (File.Exists(Path.Combine(installFolder, CudaBackendDll)))
            {
                var missing = new List<string>();
                foreach (var (glob, example) in CudaRuntimeDlls)
                {
                    if (Directory.GetFiles(installFolder, glob).Length == 0)
                    {
                        missing.Add(example);
                    }
                }

                if (missing.Count > 0)
                {
                    return $"the CUDA runtime files are missing from {installFolder} ("
                           + string.Join(", ", missing)
                           + "). Re-download the engine and pick the Vulkan or CPU build, which need no "
                           + "CUDA runtime, or copy the files from another CUDA install.";
                }
            }

            if (File.Exists(Path.Combine(installFolder, VulkanBackendDll)) && !VulkanHelper.IsInstalled())
            {
                return "the Vulkan runtime (vulkan-1.dll) was not found. It normally ships with current "
                       + "GPU drivers - update your driver, install the Vulkan runtime from "
                       + "https://vulkan.lunarg.com/sdk/home, or re-download the engine and pick the CPU build.";
            }
        }
        catch
        {
            // diagnostics only - never let the probe turn into a second failure
        }

        return null;
    }

    private static bool IsNtStatus(int exitCode)
    {
        // NTSTATUS failure codes are 0xC0000000-based. Ordinary tools exit with small values, and
        // Unix signals surface as 128+n, so this range is unambiguous in practice.
        return ((uint)exitCode & 0xFFFF0000) == 0xC0000000;
    }
}
