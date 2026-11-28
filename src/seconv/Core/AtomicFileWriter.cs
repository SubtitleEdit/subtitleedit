using System.Text;

namespace SeConv.Core;

/// <summary>
/// Writes output files via a temp file in the same folder that is then renamed over the target,
/// so an interrupted write (killed process, crash, power loss) leaves either the old file or the
/// new one - never a truncated one. With --overwrite the target is often the input itself, and
/// File.WriteAllText truncates before it writes (issue #15829).
/// </summary>
internal static class AtomicFileWriter
{
    public static void WriteAllText(string path, string content, Encoding encoding)
    {
        Write(path, stream =>
        {
            // StreamWriter writes the encoding's preamble at position 0, like File.WriteAllText.
            using var writer = new StreamWriter(stream, encoding, bufferSize: -1, leaveOpen: true);
            writer.Write(content);
        });
    }

    public static void Write(string path, Action<Stream> write)
    {
        var target = ResolveTarget(path);
        var directory = Path.GetDirectoryName(Path.GetFullPath(target));
        if (string.IsNullOrEmpty(directory))
        {
            directory = Directory.GetCurrentDirectory();
        }

        // Same folder, so the rename stays on one volume and is atomic.
        var tempPath = Path.Combine(directory, "." + Path.GetFileName(target) + "." + Path.GetRandomFileName() + ".tmp");
        FileStream tempStream;
        try
        {
            tempStream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            // The folder is not writable (the file may still be) - write in place as before.
            using var fs = new FileStream(target, FileMode.Create, FileAccess.Write);
            write(fs);
            return;
        }

        try
        {
            using (tempStream)
            {
                write(tempStream);
                tempStream.Flush(flushToDisk: true);
            }

            CopyUnixFileMode(target, tempPath);
            ReplaceFile(tempPath, target);
        }
        finally
        {
            TryDelete(tempPath);
        }
    }

    /// <summary>
    /// The same for writers that open the file themselves (the Blu-ray sup / DVD sup / VobSub
    /// export handlers): <paramref name="write"/> gets a temp path in the target's folder with the
    /// target's extension, and on success that file - plus any companion that sits next to it
    /// with one of <paramref name="companionExtensions"/>, e.g. VobSub's ".idx" - is renamed over
    /// the target. On failure the temp files are deleted and the old output is left as it was.
    /// <paramref name="write"/> must have closed its files when it returns or throws.
    /// </summary>
    public static void WriteViaTempPath(string path, Action<string> write, params string[] companionExtensions)
    {
        var target = ResolveTarget(path);
        var directory = Path.GetDirectoryName(Path.GetFullPath(target));
        if (string.IsNullOrEmpty(directory))
        {
            directory = Directory.GetCurrentDirectory();
        }

        // Ends in the target's own extension: VobSubWriter derives the .idx name from it.
        var tempPath = Path.Combine(directory,
            "." + Path.GetFileNameWithoutExtension(target) + "." + Path.GetRandomFileName().Replace('.', '_') + Path.GetExtension(target));
        try
        {
            // Reserve the name; also tells whether the folder is writable at all.
            new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None).Dispose();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            // The folder is not writable (the file may still be) - write in place as before.
            write(target);
            return;
        }

        var companions = companionExtensions
            .Select(ext => (Temp: Path.ChangeExtension(tempPath, ext), Target: Path.ChangeExtension(target, ext)))
            .ToList();
        try
        {
            write(tempPath);
            CopyUnixFileMode(target, tempPath);
            ReplaceFile(tempPath, target);
            foreach (var (temp, final) in companions)
            {
                if (File.Exists(temp))
                {
                    CopyUnixFileMode(final, temp);
                    ReplaceFile(temp, final);
                }
            }
        }
        finally
        {
            TryDelete(tempPath);
            foreach (var (temp, _) in companions)
            {
                TryDelete(temp);
            }
        }
    }

    /// <summary>
    /// A rename over a symbolic link would replace the link with a regular file - write to the
    /// file it points at instead.
    /// </summary>
    private static string ResolveTarget(string path)
    {
        try
        {
            var info = new FileInfo(path);
            if (info.Exists && info.LinkTarget != null)
            {
                return info.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? path;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Fall back to the given path.
        }

        return path;
    }

    private static void CopyUnixFileMode(string from, string to)
    {
        if (OperatingSystem.IsWindows() || !File.Exists(from))
        {
            return;
        }

        try
        {
            File.SetUnixFileMode(to, File.GetUnixFileMode(from));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The new file keeps the default (umask) mode.
        }
    }

    private static void ReplaceFile(string tempPath, string target)
    {
        if (OperatingSystem.IsWindows() && File.Exists(target))
        {
            try
            {
                // Keeps the target's attributes, ACLs and creation time.
                File.Replace(tempPath, target, destinationBackupFileName: null, ignoreMetadataErrors: true);
                return;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
            {
                // E.g. some network shares - a plain rename below.
            }
        }

        File.Move(tempPath, target, overwrite: true);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A leftover temp file is harmless; the target is intact.
        }
    }
}
