using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager.VoicePacks;
using Nikse.SubtitleEdit.UiLogic;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IVoicePackDownloadService
{
    Task DownloadPack(VoicePack pack, Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads a <see cref="VoicePack"/> zip and checks it against the catalog's SHA-256, so a
/// truncated or tampered download surfaces as an error rather than as half a voice pack.
/// </summary>
public class VoicePackDownloadService : IVoicePackDownloadService
{
    private readonly HttpClient _httpClient;

    public VoicePackDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadPack(VoicePack pack, Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, pack.Url, stream, progress, cancellationToken);

        if (string.IsNullOrEmpty(pack.Sha256) || stream.Length == 0)
        {
            return;
        }

        stream.Position = 0;
        var actual = await Sha256Util.ComputeSha256Async(stream, cancellationToken);
        stream.Position = 0;
        if (!string.Equals(pack.Sha256, actual, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException($"Voice pack '{pack.Name}' failed integrity check (expected SHA-256 {pack.Sha256}, got {actual}).");
        }
    }
}
