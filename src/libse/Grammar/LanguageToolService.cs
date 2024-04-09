using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Grammar
{
    public class LanguageToolService : IDisposable
    {
        private readonly HttpClient _httpClient = new HttpClient();
        // private readonly SeJsonParser _seJsonParser = new SeJsonParser();
        private readonly Regex _messageRegex = new Regex("(?<=message\\\":\").+?(?=\")", RegexOptions.Compiled);
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public async Task<string> CheckAsync(string text)
        {
            var httpResponse = await _httpClient.PostAsync($"http://localhost:8010/v2/check?text={Utilities.UrlEncode(text)}&language=en", null);
            var json = await httpResponse.Content.ReadAsStringAsync();
            foreach (Match match in _messageRegex.Matches(json))
            {
                _stringBuilder.AppendLine($" - {match.Value}");
            }

            try
            {
                return _stringBuilder.Length == 0 ? "All Good :)" : _stringBuilder.ToString();
            }
            finally
            {
                _stringBuilder.Clear();
            }
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(1));
                    return await _httpClient.GetAsync("http://localhost:8010", cts.Token) != null;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}