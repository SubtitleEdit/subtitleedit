using System;
using System.Text;

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

        public string DecodePassword()
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(Password));
            }
            catch
            {
                return null;
            }            
        }

        public void EncodePassword(string unencryptedPassword)
        {
            Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(unencryptedPassword));
        }
    }
}