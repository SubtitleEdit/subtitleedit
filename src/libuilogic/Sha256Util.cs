using System.Security.Cryptography;

namespace Nikse.SubtitleEdit.UiLogic;

/// <summary>
/// SHA-256 hashing helpers for files and streams.
/// </summary>
public static class Sha256Util
{
    /// <summary>
    /// Computes the lower-case hex SHA-256 of the file at <paramref name="filePath"/>.
    /// Returns null when the file does not exist.
    /// </summary>
    public static string? ComputeSha256(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return ComputeSha256(stream);
    }

    /// <summary>
    /// Computes the lower-case hex SHA-256 of <paramref name="stream"/> from its current position.
    /// </summary>
    public static string ComputeSha256(Stream stream)
    {
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    /// <summary>
    /// Computes the lower-case hex SHA-256 of the file at <paramref name="filePath"/>.
    /// Returns null when the file does not exist.
    /// </summary>
    public static async Task<string?> ComputeSha256Async(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        // 80 KB buffer: async reads at 4 KB are ~3x slower than sync; 80 KB restores full
        // throughput while staying under the large-object-heap threshold.
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
        return await ComputeSha256Async(stream, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Computes the lower-case hex SHA-256 of <paramref name="stream"/> from its current position.
    /// </summary>
    public static async Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken = default)
    {
        var hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
