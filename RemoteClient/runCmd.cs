using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvidiaStuUpdater
{
    class runCmd
    {
        private Process cmd = new Process();
        public runCmd()
        {
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.Verb = "runas";
        }

        public string execute(string input)
        {
            cmd.StartInfo.Arguments = "/C " + input;
            cmd.Start();
            cmd.StandardInput.AutoFlush = true;

            string output = cmd.StandardOutput.ReadToEnd();
            cmd.WaitForExit();
            cmd.Close();
            return output;
        }
    }
}
