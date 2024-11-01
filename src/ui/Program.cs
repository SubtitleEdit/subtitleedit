using Nikse.SubtitleEdit.Forms;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using DiscordRPC;
using System.Timers;

namespace Nikse.SubtitleEdit
{
    internal static class Program
    {
        private static DiscordRPCMain discordRpcMain;
        private static System.Windows.Forms.Timer timer;
        private static string lastFileName = "";

        [STAThread]
        private static void Main()
        {
#if !DEBUG
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize Discord RPC
            discordRpcMain = new DiscordRPCMain();
            discordRpcMain.Initialize();

            // Set up the timer to monitor the window title
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 2000; // Check every 2 seconds
            timer.Tick += CheckWindowTitle;
            timer.Start();

            Application.Run(new Main());

            // Shutdown Discord RPC
            discordRpcMain.Shutdown();
        }

        private static void CheckWindowTitle(object sender, EventArgs e)
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var windowTitle = process.MainWindowTitle;

                if (!string.IsNullOrEmpty(windowTitle) && windowTitle != lastFileName)
                {
                    lastFileName = windowTitle;

                    // Extract name from the window
                    var fileName = windowTitle.Contains(" - ") ? windowTitle.Split(new string[] { " - " }, StringSplitOptions.None)[0] : "No file opened";
                    discordRpcMain.UpdatePresence($"Editing: {fileName}");
                }
            }
            catch
            {
                timer.Stop();
                timer.Dispose();
            }
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
                return $"{System.Reflection.Assembly.GetEntryAssembly().GetName().Version} - {IntPtr.Size * 8}-bit  - {Environment.OSVersion}";
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
