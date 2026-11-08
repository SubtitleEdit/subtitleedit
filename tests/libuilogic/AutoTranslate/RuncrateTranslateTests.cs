using System.Net;
using System.Text;
using System.Text.Json;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

public class RuncrateTranslateTests
{
    [Fact]
    public void Defaults_PointAtTheOpenAiCompatibleEndpoint()
    {
        var tools = new ToolsSettings();

        Assert.Equal("https://api.runcrate.ai/v1/chat/completions", RuncrateTranslate.DefaultUrl);
        Assert.Equal(RuncrateTranslate.DefaultUrl, tools.RuncrateUrl);
        Assert.Equal(RuncrateTranslate.Models[0], tools.RuncrateModel);
        Assert.Contains("{0}", tools.RuncratePrompt);
        Assert.Contains("{1}", tools.RuncratePrompt);
    }

    [Fact]
    public void Name_And_Url_AreSet()
    {
        var engine = new RuncrateTranslate();

        Assert.Equal("Runcrate", engine.Name);
        Assert.Equal("Runcrate", engine.ToString());
        Assert.Equal("https://www.runcrate.ai/", engine.Url);
    }

    [Fact]
    public void Languages_AreTheSharedLlmList()
    {
        var engine = new RuncrateTranslate();

        Assert.Equal(
            ChatGptTranslate.ListLanguages().Select(p => p.Name),
            engine.GetSupportedTargetLanguages().Select(p => p.Name));
        Assert.Equal(
            ChatGptTranslate.ListLanguages().Select(p => p.Name),
            engine.GetSupportedSourceLanguages().Select(p => p.Name));
    }

    [Fact]
    public async Task Translate_SendsBearerKeyModelAndPrompt_AndReadsTheReply()
    {
        var port = GetFreePort();
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();

        string? authorization = null;
        string? path = null;
        string? requestBody = null;
        var served = Task.Run(async () =>
        {
            var context = await listener.GetContextAsync();
            authorization = context.Request.Headers["Authorization"];
            path = context.Request.Url?.AbsolutePath;
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            var reply = Encoding.UTF8.GetBytes("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"Bonjour le monde\"}}]}");
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.OutputStream.WriteAsync(reply);
            context.Response.Close();
        }, TestContext.Current.CancellationToken);

        var tools = Configuration.Settings.Tools;
        var saved = (tools.RuncrateUrl, tools.RuncrateApiKey, tools.RuncrateModel, tools.RuncratePrompt);
        try
        {
            tools.RuncrateUrl = $"http://127.0.0.1:{port}/v1/chat/completions";
            tools.RuncrateApiKey = "rc-test-key";
            tools.RuncrateModel = "Qwen/Qwen3-235B-A22B-Instruct-2507";
            tools.RuncratePrompt = "Translate from {0} to {1}:";

            using var engine = new RuncrateTranslate();
            engine.Initialize();
            var result = await engine.Translate("Hello world", "en", "fr", CancellationToken.None);
            await served;

            Assert.Equal("Bonjour le monde", result);
            Assert.Equal("Bearer rc-test-key", authorization);
            Assert.Equal("/v1/chat/completions", path);

            using var doc = JsonDocument.Parse(requestBody!);
            Assert.Equal("Qwen/Qwen3-235B-A22B-Instruct-2507", doc.RootElement.GetProperty("model").GetString());
            var message = doc.RootElement.GetProperty("messages")[0];
            Assert.Equal("user", message.GetProperty("role").GetString());
            var content = message.GetProperty("content").GetString()!;
            Assert.StartsWith("Translate from en to fr:", content);
            Assert.EndsWith("Hello world", content);
        }
        finally
        {
            (tools.RuncrateUrl, tools.RuncrateApiKey, tools.RuncrateModel, tools.RuncratePrompt) = saved;
            listener.Stop();
        }
    }

    [Fact]
    public async Task Translate_FallsBackToTheFirstModelAndDefaultPrompt_WhenSettingsAreEmpty()
    {
        var port = GetFreePort();
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();

        string? requestBody = null;
        var served = Task.Run(async () =>
        {
            var context = await listener.GetContextAsync();
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            var reply = Encoding.UTF8.GetBytes("{\"choices\":[{\"message\":{\"content\":\"Hola\"}}]}");
            context.Response.StatusCode = 200;
            await context.Response.OutputStream.WriteAsync(reply);
            context.Response.Close();
        }, TestContext.Current.CancellationToken);

        var tools = Configuration.Settings.Tools;
        var saved = (tools.RuncrateUrl, tools.RuncrateApiKey, tools.RuncrateModel, tools.RuncratePrompt);
        try
        {
            tools.RuncrateUrl = $"http://127.0.0.1:{port}/v1/chat/completions";
            tools.RuncrateApiKey = string.Empty;
            tools.RuncrateModel = string.Empty;
            tools.RuncratePrompt = string.Empty;

            using var engine = new RuncrateTranslate();
            engine.Initialize();
            var result = await engine.Translate("Hello", "en", "es", CancellationToken.None);
            await served;

            Assert.Equal("Hola", result);
            Assert.Equal(RuncrateTranslate.Models[0], tools.RuncrateModel);
            using var doc = JsonDocument.Parse(requestBody!);
            Assert.Equal(RuncrateTranslate.Models[0], doc.RootElement.GetProperty("model").GetString());
            Assert.Equal(new ToolsSettings().RuncratePrompt, tools.RuncratePrompt);
        }
        finally
        {
            (tools.RuncrateUrl, tools.RuncrateApiKey, tools.RuncrateModel, tools.RuncratePrompt) = saved;
            listener.Stop();
        }
    }

    [Fact]
    public async Task Translate_ThrowsAndKeepsTheErrorBody_WhenTheApiRejectsTheRequest()
    {
        var port = GetFreePort();
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();

        var served = Task.Run(async () =>
        {
            var context = await listener.GetContextAsync();
            var reply = Encoding.UTF8.GetBytes("{\"error\":\"invalid api key\"}");
            context.Response.StatusCode = 401;
            await context.Response.OutputStream.WriteAsync(reply);
            context.Response.Close();
        }, TestContext.Current.CancellationToken);

        var tools = Configuration.Settings.Tools;
        var saved = (tools.RuncrateUrl, tools.RuncrateApiKey);
        try
        {
            tools.RuncrateUrl = $"http://127.0.0.1:{port}/v1/chat/completions";
            tools.RuncrateApiKey = "bad-key";

            using var engine = new RuncrateTranslate();
            engine.Initialize();
            await Assert.ThrowsAsync<HttpRequestException>(() => engine.Translate("Hello", "en", "fr", CancellationToken.None));
            await served;

            Assert.Contains("invalid api key", engine.Error);
        }
        finally
        {
            (tools.RuncrateUrl, tools.RuncrateApiKey) = saved;
            listener.Stop();
        }
    }

    private static int GetFreePort()
    {
        var socket = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        var port = ((IPEndPoint)socket.LocalEndpoint).Port;
        socket.Stop();
        return port;
    }
}
