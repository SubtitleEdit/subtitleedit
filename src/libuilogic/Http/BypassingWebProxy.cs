using System;
using System.Linq;
using System.Net;

using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.UiLogic.Http
{
    /// <summary>
    /// Wraps another proxy so loopback urls (localhost, 127.x, [::1]) and hosts from the
    /// user's proxy bypass list always connect directly - .NET Framework bypassed loopback
    /// implicitly, modern .NET does not.
    /// </summary>
    public sealed class BypassingWebProxy : IWebProxy
    {
        private readonly IWebProxy _inner;
        private readonly string[] _bypassHosts;

        public BypassingWebProxy(IWebProxy inner, string bypassList)
        {
            _inner = inner;
            _bypassHosts = SplitBypassList(bypassList);
        }

        /// <summary>
        /// Splits a user-entered bypass list (semicolon, comma, or space separated) into
        /// host names; leading "*." or "." wildcards are stripped since sub domains of a
        /// listed host are always bypassed too.
        /// </summary>
        public static string[] SplitBypassList(string bypassList)
        {
            if (string.IsNullOrWhiteSpace(bypassList))
            {
                return Array.Empty<string>();
            }

            return bypassList
                .Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().TrimStart('*').TrimStart('.'))
                .Where(p => p.Length > 0)
                .ToArray();
        }

        public ICredentials? Credentials
        {
            get => _inner.Credentials;
            set => _inner.Credentials = value;
        }

        public Uri? GetProxy(Uri destination) => IsBypassed(destination) ? null : _inner.GetProxy(destination);

        public bool IsBypassed(Uri host)
        {
            if (host.IsLoopback || _inner.IsBypassed(host))
            {
                return true;
            }

            foreach (var bypassHost in _bypassHosts)
            {
                if (host.Host.Equals(bypassHost, StringComparison.OrdinalIgnoreCase) ||
                    host.Host.EndsWith("." + bypassHost, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
