using myRaiCommonTasks;
using myRaiData.Incentivi;
using MyRaiServiceInterface.it.rai.servizi.svilruoesercizio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static myRaiCommonTasks.CommonTasks;

namespace EsecutoreTask
{
    class ImportaEvidenze
    {
        public static void CheckNewFile()
        {
             WSDew DEWservice = new WSDew();
            DEWservice.Credentials = new System.Net.NetworkCredential(
              CommonTasks.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
              CommonTasks.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            var db = new IncentiviEntities();
            if (db.XR_MAT_EVIDENZE_9000.Any(x => x.MESE_FILE == DateTime.Now.Month && x.ANNO_FILE == DateTime.Now.Year))
            {
                return;
            }
            string path =myRaiCommonTasks.CommonTasks.GetParametro<string>(EnumParametriSistema.PathFileEvidenze);
            if (Environment.MachineName == "LAPTOP-I4LIIUNA")
                path = @"C:\Users\massi\Desktop\Rai\CDD130_OUT.txt";

            string[] Linee = System.IO.File.ReadAllLines(path);
            if (Linee == null || ! Linee.Any())
            {
                myRaiCommonTasks.CommonTasks.Log("File vuoto");
                return;
            }
            string MeseAnno = DateTime.Now.ToString("MMMM yy").ToUpper();
            if (! Linee[0].Contains(MeseAnno))
            {
                CommonTasks.Log("File non trovato per " + MeseAnno );
                return;
            }
            var ecc = db.XR_MAT_CATEGORIE.Select(x => x.ECCEZIONE).ToList();
            List<string> EccezioniCongedi = new List<string>();
            foreach (string e in ecc)
            {
                if (e == null) continue;
                if ( e.Contains("-"))
                {
                    EccezioniCongedi.AddRange(e.Split('-').Select(x => x.Trim()).ToList());
                }
                else
                    EccezioniCongedi.Add(e);
            }

            Linee = Linee.Where(x => !x.StartsWith("1PGR") && !x.StartsWith("0*****")).ToArray();
            CommonTasks.Log("Import file evidenze in corso, " + Linee.Count() + " righe");
            foreach (string linea in Linee)
            {
                XR_MAT_EVIDENZE_9000 E = new XR_MAT_EVIDENZE_9000();
                string[] segmenti = linea.Split('-');
                if (segmenti.Length >= 21)
                {
                    E.MATRICOLA = segmenti[1];
                    E.ANNO_FILE =2000+ Convert.ToInt32(segmenti[2].Substring(0, 2));
                    E.MESE_FILE = Convert.ToInt32(segmenti[2].Substring(2, 2));
                    E.ECCEZIONE = segmenti[5];
                    E.ATTIVA = true;

                    DateTime D1;
                    if (DateTime.TryParseExact(segmenti[6], "ddMMyy", null, DateTimeStyles.None, out D1))
                        E.DATA_INIZIO_PERIODO = D1;

                    DateTime D2;
                    if (DateTime.TryParseExact(segmenti[7], "ddMMyy", null, DateTimeStyles.None, out D2))
                        E.DATA_FINE_PERIODO = D2;

                    E.TRACCIATO = linea;
                    E.DATA_IMPORT = DateTime.Now;
                    if (EccezioniCongedi.Contains(E.ECCEZIONE))
                    {
                        var rich =db.XR_MAT_RICHIESTE.Where(x => x.MATRICOLA == E.MATRICOLA && x.ECCEZIONE == E.ECCEZIONE
                        &&
                        ((E.DATA_INIZIO_PERIODO >= x.INIZIO_GIUSTIFICATIVO && E.DATA_FINE_PERIODO <= x.FINE_GIUSTIFICATIVO)
                        ||
                         (E.DATA_INIZIO_PERIODO >= x.DATA_INIZIO_MATERNITA && E.DATA_FINE_PERIODO <= x.DATA_FINE_MATERNITA))
                        ).FirstOrDefault();

                        if (rich != null)
                            E.ID_RICHIESTA = rich.ID;
                    }
                    string CurrentLine = linea;
                    if (!String.IsNullOrWhiteSpace(segmenti[21]))
                    {
                        segmenti[21] = "";
                    }
                    CurrentLine = string.Join("", segmenti).Substring(1).Trim();
                    //string Lin = linea.Replace("-", "");

                    var Tracc  =DEWservice.GetRecordDaCancellare("CDD13COR1", "", CurrentLine, "");
                    if (Tracc != null && Tracc.Any())
                    {
                        E.ID_TRACCIATO_RICHIAMATO = Tracc[0].Id_tracciato_richiamato;
                    }
                    else
                    {
                        CommonTasks.Log("Non trovato "+ linea);
                    }
                    db.XR_MAT_EVIDENZE_9000.Add(E);
                }
            }
            
                db.SaveChanges();
                CommonTasks.Log("Import file terminato");
           
        }
    }
}
