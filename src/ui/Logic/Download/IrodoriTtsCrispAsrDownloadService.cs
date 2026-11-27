using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.UiLogic;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IIrodoriTtsCrispAsrDownloadService
{
    Task DownloadModels(IProgress<float> progress, bool forceRedownload, CancellationToken cancellationToken);
}

public sealed class IrodoriTtsCrispAsrDownloadService(HttpClient httpClient) : IIrodoriTtsCrispAsrDownloadService
{
    private const string BaseUrl = "https://huggingface.co/cstr/irodori-tts-GGUF/resolve/0c961a42ed181c27a129803e6d3d572e3f4eb892/";

    public async Task DownloadModels(IProgress<float> progress, bool forceRedownload, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(IrodoriTtsCrispAsr.ModelFolder);
        const float modelFraction = (float)IrodoriTtsCrispAsr.ModelSize / (IrodoriTtsCrispAsr.ModelSize + IrodoriTtsCrispAsr.CodecSize);
        await Download(IrodoriTtsCrispAsr.ModelFileName, IrodoriTtsCrispAsr.ModelSize,
            "21a96241aaa71db847854c90c8c39623d39978261eaa3c362829d3937fd0f37d",
            new Progress<float>(value => progress.Report(value * modelFraction)), forceRedownload, cancellationToken);
        await Download(IrodoriTtsCrispAsr.CodecFileName, IrodoriTtsCrispAsr.CodecSize,
            "837df284f23d2690e28868e8e93e3dcb651784b1ac169ed5440acecef64d4135",
            new Progress<float>(value => progress.Report(modelFraction + value * (1 - modelFraction))), forceRedownload, cancellationToken);
    }

    private async Task Download(string fileName, long size, string expectedHash, IProgress<float> progress, bool forceRedownload, CancellationToken cancellationToken)
    {
        if (!forceRedownload && IrodoriTtsCrispAsr.HasSize(fileName, size))
        {
            return;
        }

        var destination = Path.Combine(IrodoriTtsCrispAsr.ModelFolder, fileName);
        var temporary = destination + "." + Guid.NewGuid().ToString("N") + ".part";
        try
        {
            await DownloadHelper.DownloadFileAsync(httpClient, BaseUrl + fileName, temporary, progress, cancellationToken);
            await using (var stream = File.OpenRead(temporary))
            {
                if (stream.Length != size ||
                    !string.Equals(await Sha256Util.ComputeSha256Async(stream, cancellationToken), expectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException($"Irodori download failed integrity verification: {fileName}");
                }
            }

            File.Move(temporary, destination, overwrite: true);
        }
        finally
        {
            File.Delete(temporary);
        }
    }
}
