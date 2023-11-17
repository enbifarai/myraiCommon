using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Immatricolazione
{
    class Program
    {
        //Esegue XRCEIMMA ed in input diamo file.txt con dei dati (id_persona,cognome,nome,matricola,date)
        public static void ExecuteCommand(string command)
        {
            System.IO.File.WriteAllText(@"percorso creazione file di testo", command);
            var processInfo = new ProcessStartInfo("XRCEIMMA.exe", command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }

        static void Main(string[] args)
        {
            ExecuteCommand("CM2CICS.txt");
        }
    }
}

