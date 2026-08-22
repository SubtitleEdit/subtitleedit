using System.Net;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

public class GoogleTranslateV1RetryTests
{
    [Theory]
    // Issue #14004: the free gtx endpoint answers an HTML "Error 500" page mid-run on long
    // translations; before, one such reply aborted the whole job.
    [InlineData(HttpStatusCode.InternalServerError, true)]
    [InlineData(HttpStatusCode.BadGateway, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    [InlineData(HttpStatusCode.GatewayTimeout, true)]
    [InlineData(HttpStatusCode.TooManyRequests, true)]
    // Permanent failures must still surface immediately.
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.Forbidden, false)]
    [InlineData(HttpStatusCode.NotFound, false)]
    [InlineData(HttpStatusCode.OK, false)]
    public void ShouldRetry_OnlyForTransientServerErrors(HttpStatusCode statusCode, bool expected)
    {
        using var response = new HttpResponseMessage(statusCode);

        Assert.Equal(expected, GoogleTranslateV1.ShouldRetry(response, "<html><title>Error 500 (Server Error)!!1</title></html>"));
    }
}
