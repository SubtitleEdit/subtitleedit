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

    // Issue #14015: a 429 carrying Google's "Sorry..." abuse page is an IP-level block that
    // lasts minutes to hours, so the seconds-scale retry ladder must not run - fail fast and
    // let Translate surface the explanation instead.
    private const string SorryPage = "<html><head><meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\"/><title>Sorry...</title></head>" +
                                     "<body>Our systems have detected unusual traffic from your computer network.</body></html>";

    [Fact]
    public void ShouldRetry_NotForGoogleSorryBlockPage()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);

        Assert.False(GoogleTranslateV1.ShouldRetry(response, SorryPage));
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, SorryPage, true)]
    [InlineData(HttpStatusCode.Forbidden, SorryPage, true)]
    // A plain rate-limit 429 without the abuse page is still a transient error, not a block.
    [InlineData(HttpStatusCode.TooManyRequests, "<html><head><title>429 Too Many Requests</title></head></html>", false)]
    // The markers only count on the blocking status codes, not on a successful reply.
    [InlineData(HttpStatusCode.OK, SorryPage, false)]
    [InlineData(HttpStatusCode.InternalServerError, SorryPage, false)]
    public void IsGoogleSorryBlockPage_DetectsAbusePageOnly(HttpStatusCode statusCode, string content, bool expected)
    {
        Assert.Equal(expected, GoogleTranslateV1.IsGoogleSorryBlockPage(statusCode, content));
    }
}
