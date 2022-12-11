using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class SeLogger
    {

        public static void Error(Exception exception, string message = null)
        {
            if (exception == null)
            {
                return;
            }

            try
            {
                var filePath = Path.Combine(Configuration.DataDirectory, "error_log.txt");
                using (var writer = new StreamWriter(filePath, true, Encoding.UTF8))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine($"Date: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
                    writer.WriteLine($"SE: {GetSeInfo()}");
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        writer.WriteLine("Message: " + message);
                    }

                    writer.WriteLine();

                    // write exception + all inner exceptions
                    var ex = exception;
                    while (ex != null)
                    {
                        writer.WriteLine(ex.GetType().FullName);
                        writer.WriteLine("Message: " + ex.Message);
                        writer.WriteLine("StackTrace: " + ex.StackTrace);

                        ex = ex.InnerException;
                    }
                }
            }
            catch
            {
                throw exception;
            }
        }

        public static void Error(string message)
        {
            try
            {
                var filePath = Path.Combine(Configuration.DataDirectory, "error_log.txt");
                using (var writer = new StreamWriter(filePath, true, Encoding.UTF8))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine($"Date: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
                    writer.WriteLine($"SE: {GetSeInfo()}");
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        writer.WriteLine("Message: " + message);
                    }

                    writer.WriteLine();
                }
            }
            catch 
            {
                // ignore
            }
        }

        private static string GetSeInfo()
        {
            try
            {
                return $"{System.Reflection.Assembly.GetEntryAssembly().GetName().Version} - {Environment.OSVersion} - {IntPtr.Size * 8}-bit";
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
