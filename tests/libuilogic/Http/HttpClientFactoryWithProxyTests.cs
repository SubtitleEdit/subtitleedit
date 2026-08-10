using System;
using System.Net;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace LibUiLogicTests.Http;

public class HttpClientFactoryWithProxyTests
{
    [Fact]
    public void CreateHandler_WithProxyAddress_RoutesRemoteHostsThroughProxy()
    {
        var handler = HttpClientFactoryWithProxy.CreateHandler(new ProxySettings
        {
            ProxyAddress = "192.168.123.99:3128",
        });

        Assert.True(handler.UseProxy);
        Assert.NotNull(handler.Proxy);
        var proxyUri = handler.Proxy!.GetProxy(new Uri("https://github.com/"));
        Assert.Equal(new Uri("http://192.168.123.99:3128"), proxyUri);
    }

    [Fact]
    public void CreateHandler_WithProxyAddress_BypassesLoopback()
    {
        var handler = HttpClientFactoryWithProxy.CreateHandler(new ProxySettings
        {
            ProxyAddress = "http://192.168.123.99:3128",
        });

        Assert.True(handler.Proxy!.IsBypassed(new Uri("http://localhost:11434/")));
        Assert.True(handler.Proxy!.IsBypassed(new Uri("http://127.0.0.1:8080/")));
        Assert.False(handler.Proxy!.IsBypassed(new Uri("https://github.com/")));
    }

    [Fact]
    public void CreateHandler_WithBypassList_BypassesListedHostsAndSubdomains()
    {
        var handler = HttpClientFactoryWithProxy.CreateHandler(new ProxySettings
        {
            ProxyAddress = "http://192.168.123.99:3128",
            BypassList = "*.example.com; internal.local",
        });

        Assert.True(handler.Proxy!.IsBypassed(new Uri("https://example.com/")));
        Assert.True(handler.Proxy!.IsBypassed(new Uri("https://sub.example.com/")));
        Assert.True(handler.Proxy!.IsBypassed(new Uri("https://internal.local/")));
        Assert.False(handler.Proxy!.IsBypassed(new Uri("https://github.com/")));
    }

    [Fact]
    public void CreateHandler_WithUserNameAndPassword_SetsProxyCredentials()
    {
        var settings = new ProxySettings
        {
            ProxyAddress = "http://192.168.123.99:3128",
            UserName = "user",
            UseDefaultCredentials = false,
        };
        settings.EncodePassword("secret");

        var handler = HttpClientFactoryWithProxy.CreateHandler(settings);

        var credentials = handler.Proxy!.Credentials as NetworkCredential;
        Assert.NotNull(credentials);
        Assert.Equal("user", credentials!.UserName);
        Assert.Equal("secret", credentials.Password);
    }

    [Fact]
    public void CreateHandler_NoProxyAddress_StillAppliesBypassListToSystemProxy()
    {
        var handler = HttpClientFactoryWithProxy.CreateHandler(new ProxySettings
        {
            ProxyAddress = string.Empty,
            BypassList = "example.com",
        });

        // System proxy present or not, the wrapping proxy must bypass loopback and listed hosts.
        Assert.True(handler.UseProxy);
        Assert.NotNull(handler.Proxy);
        Assert.True(handler.Proxy!.IsBypassed(new Uri("http://localhost:9000/")));
        Assert.True(handler.Proxy!.IsBypassed(new Uri("https://example.com/")));
    }
}
