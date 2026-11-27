using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

/// <summary>
/// Spots a Linux CUDA build of Crisp ASR that quietly ran on the CPU (#17216).
///
/// The Linux CUDA archives ship libggml-cuda.so but not the CUDA runtime it links against
/// (libcudart.so.12 / libcublas.so.12, or the .13 pair for the CUDA 13 build) - unlike the
/// Windows zips, which bundle their DLLs. ggml loads its GPU backends with dlopen at run time,
/// and a failed dlopen is not an error: the engine logs "loaded CPU backend", runs the whole job
/// on the CPU and exits cleanly. Nothing in the result tells the user, so a GPU machine just
/// looks very slow. The version banner still says "[backends: cpu,cuda]", which is what makes
/// the fallback detectable from the output alone.
/// </summary>
public static class CrispAsrGpuFallback
{
    internal const string CudaLibraryFileName = "libggml-cuda.so";

    /// <summary>
    /// True when the output is from a CUDA build that loaded its CPU backend but never its CUDA
    /// one. Requires the dynamic-backend log lines, so a build that links its backends statically
    /// (and does not print them) is never flagged.
    /// </summary>
    public static bool IsCudaCpuFallback(IEnumerable<string> output)
    {
        var isCudaBuild = false;
        var loadedCpu = false;
        foreach (var line in output)
        {
            if (line.Contains("[backends:", StringComparison.Ordinal) &&
                line.Contains("cuda", StringComparison.OrdinalIgnoreCase))
            {
                isCudaBuild = true;
            }

            if (line.Contains("load_backend: loaded CPU backend", StringComparison.Ordinal))
            {
                loadedCpu = true;
            }

            if (line.Contains("load_backend: loaded CUDA backend", StringComparison.Ordinal) ||
                line.Contains("ggml_cuda_init: found", StringComparison.Ordinal))
            {
                return false;
            }
        }

        return isCudaBuild && loadedCpu;
    }

    /// <summary>The library names ldd reports as "not found", in output order.</summary>
    public static List<string> ParseMissingLibraries(string? lddOutput)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(lddOutput))
        {
            return result;
        }

        foreach (var rawLine in lddOutput.Split('\n'))
        {
            var line = rawLine.Trim();
            var arrow = line.IndexOf("=>", StringComparison.Ordinal);
            if (arrow <= 0 || !line.EndsWith("not found", StringComparison.Ordinal))
            {
                continue;
            }

            var name = line.Substring(0, arrow).Trim();
            if (name.Length > 0 && !result.Contains(name))
            {
                result.Add(name);
            }
        }

        return result;
    }

    /// <summary>
    /// Runs ldd on the engine's libggml-cuda.so and returns the libraries it cannot resolve.
    /// Best effort: an empty list when the file or ldd is missing, or ldd fails.
    /// </summary>
    public static List<string> FindMissingLibraries(string engineFolder)
    {
        try
        {
            var path = Path.Combine(engineFolder, CudaLibraryFileName);
            if (!File.Exists(path))
            {
                return new List<string>();
            }

            var psi = new ProcessStartInfo("ldd")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add(path);

            using var process = Process.Start(psi);
            if (process == null)
            {
                return new List<string>();
            }

            var output = process.StandardOutput.ReadToEndAsync();
            if (!process.WaitForExit(5000))
            {
                try { process.Kill(); } catch { /* ignore */ }
                return new List<string>();
            }

            return ParseMissingLibraries(output.Result);
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>The user-facing explanation, naming the missing libraries when ldd found them.</summary>
    public static string Describe(IReadOnlyList<string> missingLibraries)
    {
        var nl = Environment.NewLine;
        var message = "The CUDA build of Crisp ASR is installed, but this run used the CPU: its CUDA backend " +
                      $"({CudaLibraryFileName}) could not be loaded.{nl}{nl}";

        if (missingLibraries.Count == 0)
        {
            return message +
                   "Check that the NVIDIA driver is installed and working (\"nvidia-smi\" should list the GPU), and " +
                   "that the CUDA toolkit is installed. Or download the speech to text engine again and choose the " +
                   "Vulkan build, which only needs the GPU driver.";
        }

        var cudaMajor = missingLibraries.Any(p => p.EndsWith(".so.13", StringComparison.Ordinal)) ? "13" :
                        missingLibraries.Any(p => p.EndsWith(".so.12", StringComparison.Ordinal)) ? "12" : null;
        var toolkit = cudaMajor == null ? "the CUDA toolkit" : $"the CUDA {cudaMajor} toolkit";

        return message +
               $"Missing libraries: {string.Join(", ", missingLibraries)}{nl}{nl}" +
               "The Linux CUDA build does not include the CUDA runtime. Install " + toolkit + " from your " +
               "distribution (it provides libcudart and libcublas). If your distribution ships a different CUDA " +
               "version, download the speech to text engine again and choose the CUDA build that matches it, or " +
               "choose Vulkan, which only needs the GPU driver.";
    }
}
