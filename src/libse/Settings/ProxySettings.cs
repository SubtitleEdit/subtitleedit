using System;
using System.Net;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class ProxySettings
    {
        public string ProxyAddress { get; set; }
        public string AuthType { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }

        /// <summary>
        /// Decodes the given password by converting it from base64 string representation to plain text.
        /// </summary>
        /// <returns>The decoded plain text password.</returns>
        /// <param name="password">The password to be decoded.</param>
        public string DecodePassword()
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(Password));
        }

        /// <summary>
        /// Encodes the given password by converting it to base64 string representation.
        /// </summary>
        /// <param name="unencryptedPassword">The unencrypted password to be encoded.</param>
        public void EncodePassword(string unencryptedPassword)
        {
            Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(unencryptedPassword));
        }

        /// <summary>
        /// Determines if the proxy should use default credentials.
        /// </summary>
        /// <returns>True if default credentials should be used, false otherwise.</returns>
        public bool IsDefaultCredential() => string.IsNullOrEmpty(UserName);

        /// <summary>
        /// Creates a proxy from the current proxy settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="IWebProxy"/> interface that represents the proxy defined by the current proxy settings.</returns>
        public IWebProxy CreateProxyFromSettings()
        {
            return new WebProxy(ProxyAddress)
            {
                UseDefaultCredentials = IsDefaultCredential(),
                Credentials = UseDefaultCredentials ? null : NewNetworkCredential(),
            };
        }

        /// <summary>
        /// Creates a new instance of the <see cref="NetworkCredential"/> class based on the current proxy settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="NetworkCredential"/> class that represents the network credentials defined by the current proxy settings.</returns>
        private NetworkCredential NewNetworkCredential()
        {
            return string.IsNullOrEmpty(Domain)
                ? new NetworkCredential(UserName, DecodePassword())
                : new NetworkCredential(UserName, DecodePassword(), Domain);
        }
    }
}