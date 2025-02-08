using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic
{
    /// <summary>
    /// Provides methods to interact with the Free Dictionary API for checking the existence of words.
    /// </summary>
    /// <remarks>
    /// This class communicates with the API located at "https://api.dictionaryapi.dev".
    /// It is designed to check the existence of entries in the dictionary.
    /// The client uses HTTP GET requests and expects JSON responses.
    /// </remarks>
    public class FreeDictionaryClient : IDisposable
    {
        private readonly HttpClient _client;
        private readonly Dictionary<string, bool> _cachedWords = new Dictionary<string, bool>();

        public FreeDictionaryClient()
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri("https://api.dictionaryapi.dev"),
            };
            _client.DefaultRequestHeaders.Add("User-Agent", "SubtitleEdit");
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        /// <summary>
        /// Checks if a given word exists in the dictionary by querying the Free Dictionary API.
        /// </summary>
        /// <param name="word">The word to check for existence in the dictionary.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a boolean value where <c>true</c> indicates that the word exists in the dictionary,
        /// and <c>false</c> indicates it does not.
        /// </returns>
        public async Task<bool> ExistsAsync(string word)
        {
            string mapKey = word.ToLowerInvariant();

            // don't hit the internet if already checked
            if (_cachedWords.TryGetValue(mapKey, out var result))
            {
                return result;
            }

            // hit the internet
            var apiResponse = await _client.GetAsync($"/api/v2/entries/en/{word}").ConfigureAwait(false);

            // cache the result
            _cachedWords[mapKey] = apiResponse.StatusCode == System.Net.HttpStatusCode.OK;

            // return the cached result
            return _cachedWords[mapKey];
        }

        public void Dispose() => _client?.Dispose();
    }
}