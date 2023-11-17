using ComunicaCics;
using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace myRaiCommonManager
{
    public class ImmatricolazioneCicsManager
    {
       
        public static string ChiamataCics(XR_IMM_IMMATRICOLAZIONI immatricolazione, string pMatricola, string dataFineForCics,string tipoOperzione) 
        {
            var dataNascita = immatricolazione.DTA_NASCITAPERS.ToString("yyyyMMdd");
            var dataInizio = immatricolazione.DTA_INIZIO.ToString("yyyyMMdd");
            string servizio = immatricolazione.COD_SERVIZIO.ToUpper().Trim();
            string strfunction = (immatricolazione.ID_PERSONA.ToString().PadRight(18) + tipoOperzione.PadRight(1)
                    + immatricolazione.COD_MATDIP.PadRight(6) + immatricolazione.DES_COGNOMEPERS.ToUpper().PadRight(50) + immatricolazione.DES_NOMEPERS.ToUpper().PadRight(25) + dataNascita.PadRight(8)
                    + immatricolazione.COD_SEDE.ToUpper().PadRight(3)+ servizio + immatricolazione.COD_TIPORLAV.ToUpper().PadRight(1)
                    + immatricolazione.COD_QUALIFICA.ToUpper().PadRight(3) + dataInizio.PadRight(8) + dataFineForCics.PadRight(8) + pMatricola.PadRight(8)).PadRight(9);
            //var cics = new ComunicaVersoCics();
            //var cics = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

            //var risposta =/* cics.ComunicaCICS(chiamata.PadRight(9)).ToString();*/ "";

            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cics = null;
            var endpoint = HrisHelper.GetParametro(HrisParam.ImmatricolazioneServizio);
            if (endpoint==null)
                cics = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            else
                cics = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client(endpoint.COD_VALUE1, endpoint.COD_VALUE2);

            var risposta = cics.ComunicaCICS(String.Format("P956,IMM,{0},{1}", CommonHelper.GetCurrentUserPMatricola().PadRight(13), strfunction));
            HrisHelper.LogOperazione("ImmatricolazioneCICS", pMatricola+"\r\n"+ String.Format("P956,IMM,{0},{1}", CommonHelper.GetCurrentUserPMatricola().PadRight(13), strfunction)+"\r\nRisposta:\r\n"+risposta+"\r\nUrl:\r\n"+cics.Endpoint.Address.ToString(), true);

            //string path = @"C:\RAI\DIGIGAPP\Baseline\myRaiCommon\myRaiCommonManager\ImmatrcicolazioneFileText\" + "CM2CICS" + immatricolazione.ID_EVENTO.ToString() + ".txt";

            //// This text is added only once to the file.
            //if (!File.Exists(path))
            //{
            //    // Create a file to write to.
            //    string createText = strfunction + Environment.NewLine;
            //    File.WriteAllText(path, createText);
            //}
            //ExecuteCommand(path);
            return risposta;
        }

        public static void ExecuteCommand(string path)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;
            processInfo = new ProcessStartInfo("C:/RAI/DIGIGAPP/Baseline/myRaiCommon/myRaiCommonManager/HRCEIMMA/HRCEIMMA.exe",  path);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;

            // ***output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            process = Process.Start(processInfo);
            process.WaitForExit();

            // ***Legg stream***
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            exitCode = process.ExitCode;
            process.Close();
        }
    }
}
