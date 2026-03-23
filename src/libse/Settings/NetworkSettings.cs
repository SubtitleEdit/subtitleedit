using System;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class NetworkSettings
    {
        public string UserName { get; set; }
        public string WebApiUrl { get; set; }
        public string SessionKey { get; set; }
        public int PollIntervalSeconds { get; set; }
        public string NewMessageSound { get; set; }

        public NetworkSettings()
        {
            UserName = string.Empty;
            SessionKey = Guid.NewGuid().ToString();
            WebApiUrl = "https://www.nikse.dk/api/SeNet";
            PollIntervalSeconds = 5;
        }
    }
}