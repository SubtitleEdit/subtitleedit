using Microsoft.Win32;
using System.Security;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class RegistryUtil
    {
        /// <summary>
        /// Retrieves the specified registry subkey value.
        /// </summary>
        /// <param name="keyName">The path of the subkey to open.</param>
        /// <param name="valueName">The name of the value to retrieve.</param>
        /// <returns>The value of the subkey requested, or <b>null</b> if the operation failed.</returns>
        public static string GetValue(string keyName, string valueName)
        {
            RegistryKey key = null;
            try
            {
                key = Registry.LocalMachine.OpenSubKey(keyName);
                if (key != null)
                {
                    var value = key.GetValue(valueName);
                    if (value != null)
                    {
                        return (string)value;
                    }
                }
            }
            catch (SecurityException)
            {
                // The user does not have the permissions required to read the registry key.
            }
            finally
            {
                if (key != null)
                {
                    key.Dispose();
                }
            }
            return null;
        }
    }
}
