using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    internal sealed class KillProcessHelper
    {
        internal const int CTRL_C_EVENT = 0;
        
        [DllImport("kernel32.dll")]
        internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);
        
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();
        
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add);

        private delegate bool ConsoleCtrlDelegate(uint ctrlType);

        internal static void TryToKillProcessViaCtrlC(Process process)
        {
            SetConsoleCtrlHandler(null, true);
            try
            {
                if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
                {
                    process.Kill();
                }

                process.WaitForExit();
            }
            finally
            {
                SetConsoleCtrlHandler(null, false);
                FreeConsole();
            }
        }
    }
}