using System;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

public class LensError : Exception
{
    public int Code { get; }
    public System.Net.Http.Headers.HttpResponseHeaders? Headers { get; }
    public string? Body { get; }

    public LensError(string message, int code, System.Net.Http.Headers.HttpResponseHeaders? headers = null, string? body = null)
        : base(message)
    {
        Code = code;
        Headers = headers;
        Body = body;
    }
}
