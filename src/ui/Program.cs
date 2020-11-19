using Nikse.SubtitleEdit.Forms;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
#if !DEBUG
            // Add the event handler for handling UI thread exceptions to the event.
            Application.ThreadException += Application_ThreadException;

            // Set the unhandled exception mode to force all Windows Forms errors to go through our handler.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }

        // Handle the UI exceptions by showing a dialog box, and asking the user whether or not they wish to abort execution.
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            var exc = e.Exception;
            // Ignore CultureNotFoundException to avoid error when changing language (on some computers) - see https://github.com/SubtitleEdit/subtitleedit/issues/719
            if (!(exc is System.Globalization.CultureNotFoundException))
            {
                var dr = DialogResult.Abort;
                try
                {
                    var cap = "Windows Forms Thread Exception";
                    var msg = "An application error occurred in Subtitle Edit " + GetVersion() + ". " +
                              "\nPlease report at https://github.com/SubtitleEdit/subtitleedit/issues with the following information:" +
                              "\n\nError Message:\n" + exc.Message +
                              "\n\nStack Trace:\n" + exc.StackTrace;
                    dr = MessageBox.Show(msg, cap, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1);
                }
                catch
                {
                }
                if (dr == DialogResult.Abort)
                {
                    Application.Exit();
                }
            }
        }

        private static string GetVersion()
        {
            try
            {
                return System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        // Handle the non-UI exceptions by logging the event to the ThreadException event log before
        // the system default handler reports the exception to the user and terminates the application.
        // NOTE: This exception handler cannot prevent the termination of the application.
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exc = e.ExceptionObject as Exception;
                var msg = "A fatal non-UI error occurred in Subtitle Edit " + GetVersion() + "." +
                          "\nPlease report at https://github.com/SubtitleEdit/subtitleedit/issues with the following information:" +
                          "\n\nError Message:\n" + exc.Message +
                          "\n\nStack Trace:\n" + exc.StackTrace;

                // Since we can't prevent the app from terminating, log this to the event log.
                if (!EventLog.SourceExists("ThreadException"))
                {
                    EventLog.CreateEventSource("ThreadException", "Application");
                }

                // Create an EventLog instance and assign its source.
                using (var eventLog = new EventLog { Source = "ThreadException" })
                {
                    eventLog.WriteEntry(msg);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var cap = "Non-UI Thread Exception";
                    var msg = "A fatal non-UI error occurred in Subtitle Edit " + GetVersion() + "." +
                              "\nCould not write the error to the event log." +
                              "\nReason: " + ex.Message;
                    MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                finally
                {
                    Application.Exit();
                }
            }
        }

    }
}
