using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public static class DownloadHelper
{
    public static async Task DownloadFileAsync(
        HttpClient httpClient,
        string url,
        string destinationPath,
        IProgress<float>? progress,
        CancellationToken cancellationToken)
    {
        await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
        await DownloadFileAsync(httpClient, url, fileStream, progress, cancellationToken);
    }

    public static async Task DownloadFileAsync(
      HttpClient httpClient,
      string url,
      Stream destination,
      IProgress<float>? progress,
      CancellationToken cancellationToken,
      int maxRetries = 5,
      int timeoutSeconds = 1800) // 30 minutes for large files
    {
        if (httpClient == null)
        {
            throw new ArgumentNullException(nameof(httpClient));
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        if (destination == null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        if (!destination.CanWrite)
        {
            throw new ArgumentException("Destination stream must be writable", nameof(destination));
        }

        var attempt = 0;
        Exception? lastException = null;
        long? totalBytes = null;
        var startPosition = destination.CanSeek ? destination.Position : 0;

        // Check if server supports range requests
        var supportsRangeRequests = await CheckRangeSupport(httpClient, url, cancellationToken);

        while (attempt < maxRetries)
        {
            attempt++;

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

                var currentPosition = destination.CanSeek ? destination.Position : startPosition;

                // Create request with range header if resuming and supported
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (supportsRangeRequests && currentPosition > 0 && destination.CanSeek)
                {
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(currentPosition, null);
                }

                using var response = await httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cts.Token).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                { 
                    throw new FileNotFoundException($"The requested URL was not found: {url}");
                }

                response.EnsureSuccessStatusCode();

                // Get total size from headers
                if (totalBytes == null)
                {
                    totalBytes = response.Content.Headers.ContentLength;

                    // For range requests, get total from Content-Range header
                    if (response.Content.Headers.ContentRange != null)
                    {
                        totalBytes = response.Content.Headers.ContentRange.Length;
                    }
                }

                var totalReadBytes = currentPosition; // Start from current position

                await using var contentStream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);

                var buffer = new byte[81920]; // 80KB buffer
                int readBytes;
                var lastProgressReport = DateTime.UtcNow;

                while ((readBytes = await contentStream.ReadAsync(buffer, cts.Token).ConfigureAwait(false)) > 0)
                {
                    await destination.WriteAsync(buffer.AsMemory(0, readBytes), cts.Token).ConfigureAwait(false);
                    totalReadBytes += readBytes;

                    // Report progress at most once per 100ms to avoid overwhelming the UI
                    if (progress != null && totalBytes > 0)
                    {
                        var now = DateTime.UtcNow;
                        if ((now - lastProgressReport).TotalMilliseconds >= 100)
                        {
                            var progressPercentage = (float)totalReadBytes / totalBytes.Value;
                            progress.Report(Math.Clamp(progressPercentage, 0f, 1f));
                            lastProgressReport = now;
                        }
                    }
                }

                // Verify download completeness if Content-Length was provided
                if (totalBytes > 0 && totalReadBytes != totalBytes.Value)
                {
                    throw new InvalidOperationException(
                        $"Download incomplete: expected {totalBytes} bytes, received {totalReadBytes} bytes");
                }

                await destination.FlushAsync(cts.Token).ConfigureAwait(false);

                // Success - report 100%
                progress?.Report(1f);
                return;
            }
            catch (Exception ex) when (
                ex is HttpRequestException ||
                ex is TaskCanceledException ||
                (ex is IOException && ex is not FileNotFoundException) ||
                ex is InvalidOperationException)
            {
                lastException = ex;

                // If cancellation was requested by user, don't retry
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // If this was the last retry, don't wait
                if (attempt >= maxRetries)
                {
                    break;
                }

                // Exponential backoff with jitter: wait before retrying
                var baseDelay = Math.Min(2000 * (int)Math.Pow(2, attempt - 1), 30000);
                var jitter = Random.Shared.Next(0, 1000);
                var delayMs = baseDelay + jitter;

                await Task.Delay(delayMs, CancellationToken.None).ConfigureAwait(false);

                // Don't reset stream position - we'll resume from where we left off
                // Only reset if we can't seek (which means we can't resume anyway)
                if (!destination.CanSeek && destination.Position > 0)
                {
                    throw new InvalidOperationException(
                        "Cannot retry download on non-seekable stream after partial download",
                        lastException);
                }
            }
        }

        // All retries exhausted
        var bytesDownloaded = destination.CanSeek ? destination.Position : 0;
        throw new InvalidOperationException(
            $"Failed to download file after {maxRetries} attempts. URL: {url}. Downloaded: {bytesDownloaded}/{totalBytes ?? 0} bytes",
            lastException);
    }

    private static async Task<bool> CheckRangeSupport(
        HttpClient httpClient,
        string url,
        CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await httpClient.SendAsync(request, cts.Token).ConfigureAwait(false);

            return response.Headers.AcceptRanges?.Contains("bytes") == true;
        }
        catch
        {
            // If HEAD request fails, assume no range support
            return false;
        }
    }
}