using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Http
{
    public class LegacyHttpClient : IHttpClient
    {
        public Task DownloadAsync(string requestUri, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            return Task.Factory.StartNew(delegate
            {
                try
                {
                    const int bufferSize = 4096;
                    var request = (HttpWebRequest)WebRequest.Create(requestUri);
                    request.Timeout = Timeout.Infinite;
                    var response = (HttpWebResponse)request.GetResponse();
                    var responseStream = response.GetResponseStream();
                    var tempFileName = Path.GetTempFileName();
                    var fileStream = new FileStream(tempFileName, FileMode.OpenOrCreate, FileAccess.Write);
                    var buff = new byte[bufferSize];
                    int bytesRead;
                    long totalBytesRead = 0;
                    while ((bytesRead = responseStream.Read(buff, 0, bufferSize)) > 0 && !cancellationToken.IsCancellationRequested)
                    {
                        fileStream.Write(buff, 0, bytesRead);
                        fileStream.Flush();

                        totalBytesRead += bytesRead;

                        if (progress != null && response.ContentLength > 0)
                        {
                            progress.Report(totalBytesRead / (float)response.ContentLength);
                        }
                    }
                    fileStream.Close();
                    responseStream.Close();

                    using (var fs = new FileStream(tempFileName, FileMode.Open))
                    {
                        fs.CopyTo(destination, 2048);
                    }
                    try
                    {
                        File.Delete(tempFileName);
                    }
                    catch
                    {
                        // ignore
                    }
                }
                catch (Exception exception)
                {
                    SeLogger.Error(exception, $"Error downloading {requestUri}");
                    throw;
                }
            }, cancellationToken);
        }

        private readonly HttpClient _httpClient;

        public LegacyHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Uri BaseAddress
        {
            get => _httpClient.BaseAddress;
            set => _httpClient.BaseAddress = value;
        }

        public HttpRequestHeaders DefaultRequestHeaders => _httpClient.DefaultRequestHeaders;

        public Task<HttpResponseMessage> PostAsync(string uri, StringContent stringContent)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            return _httpClient.PostAsync(uri, stringContent);
        }

        public async Task<string> GetStringAsync(string url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var webClient = new WebClient { Proxy = GetProxy() };
            foreach (var header in _httpClient.DefaultRequestHeaders)
            {
                foreach (var v in header.Value)
                {
                    webClient.Headers.Add(header.Key, v);
                }
            }

            if (_httpClient.BaseAddress != null)
            {
                url = _httpClient.BaseAddress.AbsoluteUri.TrimEnd('/') + "/" + url.TrimStart('/');
            }

            return await Task.Run(() => webClient.DownloadStringTaskAsync(new Uri(url))).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private static WebProxy GetProxy()
        {
            if (string.IsNullOrEmpty(Configuration.Settings.Proxy.ProxyAddress))
            {
                return null;
            }

            var proxy = new WebProxy(Configuration.Settings.Proxy.ProxyAddress);
            if (!string.IsNullOrEmpty(Configuration.Settings.Proxy.UserName))
            {
                if (string.IsNullOrEmpty(Configuration.Settings.Proxy.Domain))
                {
                    proxy.Credentials = new NetworkCredential(Configuration.Settings.Proxy.UserName, Configuration.Settings.Proxy.DecodePassword());
                }
                else
                {
                    proxy.Credentials = new NetworkCredential(Configuration.Settings.Proxy.UserName, Configuration.Settings.Proxy.DecodePassword(), Configuration.Settings.Proxy.Domain);
                }
            }
            else
            {
                proxy.UseDefaultCredentials = true;
            }

            return proxy;
        }
    }
}