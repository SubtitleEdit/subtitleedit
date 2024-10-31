using DiscordRPC;

public class DiscordRPCMain
{
    private DiscordRpcClient _client;

    public void Initialize()
    {
        // Aplication ID
        _client = new DiscordRpcClient("1301527670818996244");
        _client.Initialize();
    }

    public void UpdatePresence(string fileName = "No file opened")
    {
        if (_client != null)
        {
            _client.SetPresence(new RichPresence
            {
                State = fileName,
                Timestamps = Timestamps.Now,
                Assets = new Assets
                {
                    LargeImageKey = "logo", // Image
                    LargeImageText = "Subtitle Edit"
                }
            });
        }
    }

    public void Shutdown()
    {
        _client?.Dispose();
    }
}
