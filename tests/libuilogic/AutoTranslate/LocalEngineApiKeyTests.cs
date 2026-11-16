using System.Net;
using System.Net.Sockets;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// The llama.cpp, Ollama and LM Studio engines send their optional API key as a Bearer token:
/// each of these servers can ask for one, or sit behind a reverse proxy that does. Without a key
/// the request carries no Authorization header, as before.
/// </summary>
public class LocalEngineApiKeyTests : IDisposable
{
    private readonly string _oldLlamaCppUrl = Configuration.Settings.Tools.LlamaCppApiUrl;
    private readonly string _oldOllamaUrl = Configuration.Settings.Tools.OllamaApiUrl;
    private readonly string _oldLmStudioUrl = Configuration.Settings.Tools.LmStudioApiUrl;

    public void Dispose()
    {
        var tools = Configuration.Settings.Tools;
        tools.LlamaCppApiUrl = _oldLlamaCppUrl;
        tools.OllamaApiUrl = _oldOllamaUrl;
        tools.LmStudioApiUrl = _oldLmStudioUrl;
        tools.LlamaCppApiKey = string.Empty;
        tools.OllamaApiKey = string.Empty;
        tools.LmStudioApiKey = string.Empty;
    }

    [Theory]
    [InlineData("llamacpp", "secret", "Bearer secret")]
    [InlineData("llamacpp", "", null)]
    [InlineData("ollama", "secret", "Bearer secret")]
    [InlineData("ollama", "", null)]
    [InlineData("lmstudio", "secret", "Bearer secret")]
    [InlineData("lmstudio", "", null)]
    public async Task ApiKey_IsSentAsBearerToken(string engine, string apiKey, string? expectedHeader)
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var url = "http://127.0.0.1:" + ((IPEndPoint)probe.LocalEndpoint).Port + "/";
        probe.Stop();
        using var listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();

        // One reply all three engines can read: Ollama's native "response" and the chat-completions "content".
        var authorizations = new List<string?>();
        var serverTask = Task.Run(async () =>
        {
            while (true)
            {
                HttpListenerContext context;
                try
                {
                    context = await listener.GetContextAsync();
                }
                catch (Exception)
                {
                    return; // listener closed
                }

                authorizations.Add(context.Request.Headers["Authorization"]);
                var bytes = Encoding.UTF8.GetBytes("{\"response\":\"Hallo\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"Hallo\"}}]}");
                context.Response.ContentType = "application/json";
                await context.Response.OutputStream.WriteAsync(bytes);
                context.Response.Close();
            }
        }, TestContext.Current.CancellationToken);

        var tools = Configuration.Settings.Tools;
        IAutoTranslator translator;
        if (engine == "ollama")
        {
            tools.OllamaApiUrl = url + "api/generate";
            tools.OllamaApiKey = apiKey;
            translator = new OllamaTranslate();
        }
        else if (engine == "lmstudio")
        {
            tools.LmStudioApiUrl = url + "v1/chat/completions";
            tools.LmStudioApiKey = apiKey;
            translator = new LmStudioTranslate();
        }
        else
        {
            tools.LlamaCppApiUrl = url + "v1/chat/completions";
            tools.LlamaCppApiKey = apiKey;
            translator = new LlamaCppTranslate();
        }

        using (translator as IDisposable)
        {
            translator.Initialize();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var result = await translator.Translate("Hello", "English", "German", timeout.Token);
            listener.Stop();
            await serverTask;

            Assert.Equal("Hallo", result);
            Assert.NotEmpty(authorizations);
            Assert.All(authorizations, a => Assert.Equal(expectedHeader, a));
        }
    }
}
