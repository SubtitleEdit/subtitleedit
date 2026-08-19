using System;
using System.Net.Http;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.UiLogic.Http
{
    public static class DownloaderFactory
    {
        public static IDownloader MakeHttpClient()
        {
            var httpClient = new HttpClient(HttpClientFactoryWithProxy.CreateHandler())
            {
                Timeout = TimeSpan.FromMinutes(30) // 30 minutes for large downloads
            };

            if (Configuration.Settings.General.UseLegacyDownloader)
            {
                return new LegacyDownloader(httpClient);
            }

            return new HttpClientDownloader(httpClient);
        }
    }
}
