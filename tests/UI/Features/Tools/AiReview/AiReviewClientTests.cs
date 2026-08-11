using System.Net.Sockets;
using System.Text;
using Nikse.SubtitleEdit.Features.Tools.AiReview;

namespace UITests.Features.Tools.AiReview;

/// <summary>
/// Regression tests for issue #13473: AI review always sent "temperature": 0, which OpenAI's
/// reasoning models (gpt-5*, o*) reject with "Only the default (1) value is supported", and the
/// single retry only dropped response_format - so every request failed.
/// </summary>
public class AiReviewClientTests
{
    private const string TemperatureError =
        "{ \"error\": { \"message\": \"Unsupported value: 'temperature' does not support 0 with this model. " +
        "Only the default (1) value is supported.\", \"type\": \"invalid_request_error\", " +
        "\"param\": \"temperature\", \"code\": \"unsupported_value\" } }";

    private const string ResponseFormatError =
        "{ \"error\": { \"message\": \"Unrecognized request argument supplied: response_format\", " +
        "\"type\": \"invalid_request_error\" } }";

    private const string RateLimitError =
        "{ \"error\": { \"message\": \"Rate limit reached for gpt-5.6-luna, please try again later.\", " +
        "\"type\": \"requests\", \"code\": \"rate_limit_exceeded\" } }";

    // An error body that echoes the submitted request: it contains both parameter names and the
    // word "invalid" - but about the API key, far away from either parameter, so it must not be
    // read as a parameter rejection.
    private const string EchoedRequestError =
        "{ \"request\": { \"model\": \"gpt-5.6-luna\", \"temperature\": 0, \"stream\": false, " +
        "\"response_format\": { \"type\": \"json_object\" }, \"messages\": [ { \"role\": \"system\", " +
        "\"content\": \"You review subtitles for grammar, casing and punctuation problems and reply " +
        "with a json object listing each line number and the corrected text, leaving correct lines " +
        "out of the reply entirely so the caller can apply the fixes one by one.\" } ] }, " +
        "\"error\": { \"message\": \"invalid api key\", \"code\": 401 } }";

    private const string Reply = "{ \"choices\": [ { \"message\": { \"content\": \"all good\" } } ] }";

    [Fact]
    public void BuildRequestJson_IncludesTemperatureAndJsonMode()
    {
        var json = AiReviewClient.BuildRequestJson("gpt-5.6-luna", "system", "user", jsonMode: true, includeTemperature: true);

        Assert.Contains("\"temperature\":0", json);
        Assert.Contains("\"response_format\":{\"type\":\"json_object\"}", json);
    }

    [Fact]
    public void BuildRequestJson_CanOmitTemperature()
    {
        var json = AiReviewClient.BuildRequestJson("gpt-5.6-luna", "system", "user", jsonMode: false, includeTemperature: false);

        Assert.DoesNotContain("temperature", json);
        Assert.DoesNotContain("response_format", json);
        Assert.Contains("\"model\":\"gpt-5.6-luna\"", json);
    }

    [Theory]
    [InlineData(TemperatureError, "temperature", true)]
    [InlineData(TemperatureError, "response_format", false)]
    [InlineData(ResponseFormatError, "response_format", true)]
    [InlineData(ResponseFormatError, "temperature", false)]
    [InlineData(RateLimitError, "temperature", false)]
    [InlineData(RateLimitError, "response_format", false)]
    [InlineData(EchoedRequestError, "temperature", false)]
    [InlineData(EchoedRequestError, "response_format", false)]
    [InlineData("", "temperature", false)]
    public void IsUnsupportedParameter_OnlyMatchesParameterRejections(string body, string parameterName, bool expected)
    {
        Assert.Equal(expected, AiReviewClient.IsUnsupportedParameter(body, parameterName));
    }

    [Fact]
    public async Task ChatAsync_RetriesWithoutTemperature_WhenTheModelRejectsIt()
    {
        using var server = new StubServer(body => body.Contains("\"temperature\"")
            ? (400, TemperatureError)
            : (200, Reply));
        using var client = new AiReviewClient();

        var reply = await client.ChatAsync(server.Url, "gpt-5.6-luna", "system", "user", CancellationToken.None);

        Assert.Equal("all good", reply);
        Assert.Equal(2, server.Requests.Count);
        Assert.Contains("\"temperature\"", server.Requests[0]);
        Assert.DoesNotContain("\"temperature\"", server.Requests[1]);

        // The rejection sticks, so later chunks go straight out without temperature.
        await client.ChatAsync(server.Url, "gpt-5.6-luna", "system", "user two", CancellationToken.None);
        Assert.Equal(3, server.Requests.Count);
        Assert.DoesNotContain("\"temperature\"", server.Requests[2]);
    }

    [Fact]
    public async Task ChatAsync_DropsTemperatureAndJsonMode_WhenTheServerRejectsBoth()
    {
        using var server = new StubServer(body =>
        {
            if (body.Contains("\"response_format\""))
            {
                return (400, ResponseFormatError);
            }

            return body.Contains("\"temperature\"") ? (400, TemperatureError) : (200, Reply);
        });
        using var client = new AiReviewClient();

        var reply = await client.ChatAsync(server.Url, "some-model", "system", "user", CancellationToken.None);

        Assert.Equal("all good", reply);
        Assert.Equal(3, server.Requests.Count);
        Assert.DoesNotContain("\"response_format\"", server.Requests[2]);
        Assert.DoesNotContain("\"temperature\"", server.Requests[2]);
    }

    [Fact]
    public async Task ChatAsync_KeepsJsonMode_WhenTheFailureIsNotAboutParameters()
    {
        using var server = new StubServer(_ => (429, RateLimitError));
        using var client = new AiReviewClient();

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.ChatAsync(server.Url, "gpt-5.6-luna", "system", "user", CancellationToken.None));

        // Today's behavior: a JSON request still retries plain once, but a rate limit must not
        // downgrade the rest of the run.
        Assert.Equal(2, server.Requests.Count);
        Assert.Contains("\"response_format\"", server.Requests[0]);
        Assert.Contains(RateLimitError, client.Error);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.ChatAsync(server.Url, "gpt-5.6-luna", "system", "user two", CancellationToken.None));
        Assert.Contains("\"response_format\"", server.Requests[2]);
    }

    /// <summary>
    /// Minimal HTTP/1.1 server on loopback - HttpListener needs a URL ACL on Windows, and the
    /// requests here are simple enough to answer from a raw socket.
    /// </summary>
    private sealed class StubServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly List<string> _requests = new();
        private readonly object _lock = new();

        public StubServer(Func<string, (int status, string body)> respond)
        {
            _listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
            _listener.Start();
            Url = "http://127.0.0.1:" + ((System.Net.IPEndPoint)_listener.LocalEndpoint).Port + "/v1/chat/completions";
            _ = Task.Run(() => AcceptLoopAsync(respond));
        }

        public string Url { get; }

        public List<string> Requests
        {
            get
            {
                lock (_lock)
                {
                    return new List<string>(_requests);
                }
            }
        }

        private async Task AcceptLoopAsync(Func<string, (int status, string body)> respond)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
                }
                catch (Exception)
                {
                    return; // the test disposed the server
                }

                using (client)
                {
                    try
                    {
                        var requestBody = await ReadRequestBodyAsync(client.GetStream());
                        lock (_lock)
                        {
                            _requests.Add(requestBody);
                        }

                        var (status, body) = respond(requestBody);
                        var payload = Encoding.UTF8.GetBytes(body);
                        var header = Encoding.ASCII.GetBytes(
                            "HTTP/1.1 " + status + " \r\n" +
                            "Content-Type: application/json\r\n" +
                            "Content-Length: " + payload.Length + "\r\n" +
                            "Connection: close\r\n\r\n");
                        await client.GetStream().WriteAsync(header);
                        await client.GetStream().WriteAsync(payload);
                        await client.GetStream().FlushAsync();
                    }
                    catch (Exception)
                    {
                        // a half-open connection is not worth failing the test over
                    }
                }
            }
        }

        private static async Task<string> ReadRequestBodyAsync(NetworkStream stream)
        {
            var buffer = new byte[8192];
            var received = new MemoryStream();
            var headerEnd = -1;
            var contentLength = 0;

            while (true)
            {
                if (headerEnd < 0)
                {
                    var text = Encoding.UTF8.GetString(received.GetBuffer(), 0, (int)received.Length);
                    headerEnd = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                    if (headerEnd >= 0)
                    {
                        contentLength = ReadContentLength(text.Substring(0, headerEnd));
                    }
                }

                if (headerEnd >= 0 && received.Length - (headerEnd + 4) >= contentLength)
                {
                    return Encoding.UTF8.GetString(received.GetBuffer(), headerEnd + 4, contentLength);
                }

                var read = await stream.ReadAsync(buffer);
                if (read == 0)
                {
                    return string.Empty;
                }

                received.Write(buffer, 0, read);
            }
        }

        private static int ReadContentLength(string header)
        {
            foreach (var line in header.Split("\r\n"))
            {
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(line.Substring("Content-Length:".Length).Trim(), out var length))
                {
                    return length;
                }
            }

            return 0;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _listener.Stop();
            _cancellationTokenSource.Dispose();
        }
    }
}
