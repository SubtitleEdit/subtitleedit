using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Common;

/// <summary>
/// Helpers shared by the engines that run a local HTTP server process on a loopback port
/// (the audio.cpp/CrispASR/*.cpp text-to-speech servers and llama.cpp).
/// </summary>
public static class LocalServerHelper
{
    /// <summary>
    /// The command line a server was started with, quoting arguments that contain whitespace,
    /// so it can be shown in error messages.
    /// </summary>
    public static string FormatLaunchCommand(string exe, Collection<string> args)
    {
        static string Quote(string s) =>
            !string.IsNullOrEmpty(s) && s.IndexOfAny(new[] { ' ', '\t' }) >= 0
                ? "\"" + s.Replace("\"", "\\\"") + "\""
                : s;

        var sb = new StringBuilder(Quote(exe));
        foreach (var a in args)
        {
            sb.Append(' ').Append(Quote(a));
        }

        return sb.ToString();
    }

    public static string LaunchCmdSuffix(string? launchCommand) =>
        string.IsNullOrEmpty(launchCommand)
            ? string.Empty
            : $"{Environment.NewLine}Launch command: {launchCommand}";

    public static async Task<string> SafeReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch (Exception ex)
        {
            return $"<failed to read error body: {ex.Message}>";
        }
    }

    /// <summary>
    /// True when the server on <paramref name="port"/> answers /health with a success status within
    /// <paramref name="timeout"/>.
    /// </summary>
    public static async Task<bool> ProbeHealthAsync(HttpClient httpClient, int port, TimeSpan timeout, CancellationToken ct)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);
            using var resp = await httpClient.GetAsync($"http://127.0.0.1:{port}/health", cts.Token);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public static int FindFreeLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }
}
