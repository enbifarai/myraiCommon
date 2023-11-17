using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace myRaiHelper.Task
{
    public class BatchRunner : BaseTask
    {
        public string Path { get; set; }
        public string Arguments { get; set; }
        public bool WaitForExit { get; set; }
        public string WorkingDir { get; set; }
        
        public override bool CheckParam(out string errore)
        {
            bool result = true;
            errore = null;

            if (String.IsNullOrWhiteSpace(Path))
            {
                result = false;
                errore += "Path non indicato\r\n";
            }

            if (!File.Exists(Path))
            {
                result = false;
                errore += "File non trovato\r\n";
            }

            return result;
        }

        public override bool Esegui(out string output, out string errore)
        {
            output = null;
            errore = null;

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = Path;
                p.StartInfo.Arguments = Arguments;
                p.StartInfo.UseShellExecute = true;

                if (!String.IsNullOrWhiteSpace(WorkingDir))
                    p.StartInfo.WorkingDirectory = WorkingDir;
                else
                    p.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(Path);

                bool res = p.Start();
                if (WaitForExit)
                    p.WaitForExit();

                return true;
            }
            catch (Exception ex)
            {
                errore = ex.Message;
                return false;
            }
        }
    }
}
