using System.Text.Encodings.Web;
using System.Text.Json;

namespace SeConv.Helpers;

/// <summary>
/// Writes the <c>--json</c> form of a subcommand's output. Everything goes to stdout with
/// nothing else mixed in, so <c>seconv ... --json | jq</c> parses without pre-filtering;
/// human-facing notes belong on stderr.
/// </summary>
internal static class JsonOut
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        // Relaxed escaping keeps non-ASCII (format names, encoding display names) readable
        // instead of emitting \uXXXX for every accented character.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static void Write(object value) =>
        Console.Out.WriteLine(JsonSerializer.Serialize(value, Options));

    public static string Serialize(object value) =>
        JsonSerializer.Serialize(value, Options);

    /// <summary>
    /// The failure envelope for a run that never got as far as converting a file — bad usage,
    /// an unknown option, a rejected value. It has the same shape as a normal convert result
    /// so that a caller parsing stdout gets one document type on every path, rather than JSON
    /// on success and plain text on a usage error.
    /// </summary>
    public static string UsageError(string message) => Serialize(new
    {
        success = false,
        totalFiles = 0,
        successfulFiles = 0,
        failedFiles = 0,
        elapsedMs = 0,
        files = Array.Empty<object>(),
        errors = new[] { message },
        warnings = Array.Empty<string>(),
    });
}
