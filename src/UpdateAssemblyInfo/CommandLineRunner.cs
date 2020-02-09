using System.Diagnostics;

namespace UpdateAssemblyInfo
{
    public class CommandLineRunner
    {
        public string Result { get; set; }

        public bool RunCommandAndGetOutput(string command, string arguments, string workingFolder)
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName = command,
                    Arguments = arguments,
                    WorkingDirectory = workingFolder,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            p.OutputDataReceived += OutputDataReceived;
            try
            {
                p.Start();
            }
            catch
            {
                return false;
            }
            p.BeginOutputReadLine(); // Async reading of output to prevent deadlock
            if (p.WaitForExit(5000))
            {
                return p.ExitCode == 0;
            }
            return false;
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e?.Data != null)
            {
                Result = e.Data;
            }
        }
    }
}
