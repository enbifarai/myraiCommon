using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using myRaiCommonManager._Model;
using myRaiCommonTasks;
using myRaiData.Incentivi;

namespace Multiplo
{
    public class ExportCongediDB2
    {
        //public static string ECCEZIONI = "HC,MG,MU,AF,BF,CF";
        public static string ECCEZIONI = "MT";

        public static void Start(bool SimulazioneExport = false)
        {
             CommonTasks.Log("Running in simulazione:" + SimulazioneExport);
            var db = new IncentiviEntities();
            bool ServeIntestazione = true;
            List<XR_MAT_RICHIESTE> ListItemsDaCorreggere = new List<XR_MAT_RICHIESTE>();

            foreach (string eccezione in ECCEZIONI.Split(','))
            {
                CommonTasks.Log("------- Eccezione : " + eccezione);

                List<XR_MAT_RICHIESTE> list = db.XR_MAT_RICHIESTE
                    .Where(x =>
                    x.ECCEZIONE.StartsWith(eccezione) &&
                                x.ESPORTATA_DB2 == null &&
                                x.XR_WKF_OPERSTATI.Any() &&
                                x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) <= 80
                                )
                    .OrderBy(x => x.MATRICOLA).ThenBy (x=>x.DATA_INIZIO_MATERNITA.Value)
                    //.OrderBy(x => x.INIZIO_GIUSTIFICATIVO)
                    .ToList();

                CommonTasks.Log("Richieste trovate non esportate: " + list.Count());

                if (SimulazioneExport)
                {
                    ListItemsDaCorreggere.AddRange(GetXR_MAT_RICHIESTEdaCorreggere(list));
                    foreach (var rich in list)
                    {
                        if (!String.IsNullOrWhiteSpace(rich.CF_BAMBINO))
                        {
                            bool CFok = CodiceFiscaleUtility.ControlloFormaleOK(rich.CF_BAMBINO);
                            if (!CFok)
                            {
                                var richPresente = ListItemsDaCorreggere.Where(x => x.ID == rich.ID).FirstOrDefault();

                                if (richPresente != null)
                                {
                                    richPresente.CFinvalid = true;
                                }
                                else
                                {
                                    rich.CFinvalid = true;
                                    ListItemsDaCorreggere.Add(rich);
                                }
                            }
                        }
                    }
                    continue;
                }


                int counter = 0;
                int InsertCounter = 0;
                DateTime Dnow = DateTime.Now;
                string fileExport = "Export_" + Dnow.ToString("yyyyMMdd_HHmm") + ".csv";
                if (ServeIntestazione)
                {
                    myRaiCommonManager.NuoviCongedi.InsertToDB2congediCSV(null, null, fileExport);
                    ServeIntestazione = false;
                }

                foreach (XR_MAT_RICHIESTE item in list)
                {

                    counter++;
                    CommonTasks.Log("processing richiesta id:" + item.ID + " - " + counter + "/" + list.Count());

                    int maxStato = item.XR_WKF_OPERSTATI.Any() ? item.XR_WKF_OPERSTATI.Max(x => x.ID_STATO) : 9999;
                    if (maxStato > 80)
                    {
                        CommonTasks.Log($"id:{item.ID} ecc:{item.ECCEZIONE} ignorata, in stato {maxStato}");
                        continue;
                    }
                    try
                    {
                        CommonTasks.Log("export in corso richiesta id " + item.ID);

                        string dataNascita = myRaiCommonManager.NuoviCongedi.GetDataNascitaPerDb2(item);
                        myRaiCommonTasks.CommonTasks.Log($"cf: {item.CF_BAMBINO} - dn:{item.DATA_NASCITA_BAMBINO} - dnDB2:{dataNascita}");
                        List<Congedo> LC = GetCongedi(item,eccezione.StartsWith("MT"));
                        bool error = false;
                        foreach (Congedo C in LC)
                        {
                            CommonTasks.Log("inserimento db2, data : " + C.Data_eccezione.ToString("dd/MM/yyyy"));

                            EsitoCongedoDB2 esito = myRaiCommonManager.NuoviCongedi.InsertToDB2congediCSV(C, dataNascita, fileExport);
                            //EsitoCongedoDB2 esito = myRaiCommonManager.NuoviCongedi.InsertToDB2congedi(C, dataNascita, commit:(InsertCounter%5==0));
                            if (esito.Success)
                            {
                                InsertCounter++;
                                CommonTasks.Log("++++ OK ID_CONGEDO su db2 : " + esito.IDinserito);
                            }
                            else
                            {
                                error = true;
                                myRaiCommonTasks.CommonTasks.Log(esito.Error);
                            }
                        }
                        if (!error && !SimulazioneExport)
                        {
                            //item.ESPORTATA_DB2 = DateTime.Now;
                            //db.SaveChanges();
                            CommonTasks.Log("++++ id XR_MAT_RICHIESTE " + item.ID + " marcata come esportata");
                        }


                        ///////////////////////////////////////////

                    }
                    catch (Exception ex)
                    {
                        myRaiCommonTasks.CommonTasks.Log(ex.ToString());
                    }
                }
            }
            if (SimulazioneExport)
            {
                string Text = "";
                foreach (var item in ListItemsDaCorreggere)
                {
                    string pbm = "";
                    if (String.IsNullOrWhiteSpace(item.CF_BAMBINO)) pbm = "CF MANCANTE";
                    else if (String.IsNullOrWhiteSpace(item.NOME_BAMBINO)) pbm = "CF " + item.CF_BAMBINO + " NOMINATIVO MANCANTE";

                    if (item.CFinvalid) pbm += " CF NON VALIDO";

                    Text += $"MATR: {item.MATRICOLA}  ECC: {item.ECCEZIONE} - PERIODO: {item.INIZIO_GIUSTIFICATIVO.Value.ToString("dd/MM/yyyy")}-{item.FINE_GIUSTIFICATIVO.Value.ToString("dd/MM/yyyy")} - {pbm} (id {item.ID}) <br />";
                }
                if (Text == "") Text = "Nessuna anomalia riscontrata nei dati delle nuove richieste da esportare.";
                InviaMailErroriCongedi(Text);
            }
        }

        public static void InviaMailErroriCongedi(string text)
        {
  
            string dest = System.Configuration.ConfigurationManager.AppSettings["SimulaCongediMail"].ToString();

            GestoreMail gm = new GestoreMail();
            CommonTasks.Log("Invio mail in corso a " + dest + "\r\nBody:" + text);

            gm.InvioMail(text,
                "Congedi parentali - Dati mancanti o in errore",
                dest, null,
                "[CG] RaiPlace - Self Service <raiplace.selfservice@rai.it>", DateTime.Now, null);

            CommonTasks.Log("Invio mail eseguito");
        }

        private static List<XR_MAT_RICHIESTE> GetXR_MAT_RICHIESTEdaCorreggere(List<XR_MAT_RICHIESTE> list)
        {
            var DaCorreggere = list
                .Where(x => String.IsNullOrWhiteSpace(x.CF_BAMBINO) || String.IsNullOrWhiteSpace(x.NOME_BAMBINO))
                .ToList();
            return DaCorreggere;
        }

        public static List<Congedo> GetCongedi(XR_MAT_RICHIESTE item, bool IsMaternita)
        {
            List<Congedo> LC = new List<Congedo>();
            if (IsMaternita)
            {
                Congedo C = new Congedo()
                {
                    CF = item.CF_BAMBINO,
                    Codice_inps = "",
                    Ctr_non_trasferibile = 0,
                    Ctr_trasferibile = 0,
                    Data_eccezione = item.DATA_INIZIO_MATERNITA.Value,
                    Data_inizio = item.INIZIO_GIUSTIFICATIVO != null ? item.INIZIO_GIUSTIFICATIVO.Value : item.DATA_INIZIO_MATERNITA.Value,
                    Data_fine = item.FINE_GIUSTIFICATIVO != null ? item.FINE_GIUSTIFICATIVO.Value : item.DATA_FINE_MATERNITA.Value,
                    Data_inserimento = DateTime.Now,
                    Data_log = DateTime.Now,
                    Data_nascita = item.DATA_NASCITA_BAMBINO,
                    Eccezione = item.ECCEZIONE,
                    Matricola = item.MATRICOLA,
                    IdHRIS = item.ID
                };
                LC.Add(C);
            }
            else for (DateTime Dcurrent = item.INIZIO_GIUSTIFICATIVO.Value; Dcurrent <= item.FINE_GIUSTIFICATIVO.Value; Dcurrent = Dcurrent.AddDays(1))
            {
                Congedo C = new Congedo()
                {
                    CF = item.CF_BAMBINO,
                    Codice_inps = "",
                    Ctr_non_trasferibile = 0,
                    Ctr_trasferibile = 0,
                    Data_eccezione = Dcurrent,
                    Data_inizio = item.INIZIO_GIUSTIFICATIVO != null ? item.INIZIO_GIUSTIFICATIVO.Value : item.DATA_INIZIO_MATERNITA.Value,
                    Data_fine = item.FINE_GIUSTIFICATIVO != null ? item.FINE_GIUSTIFICATIVO.Value : item.DATA_FINE_MATERNITA.Value,
                    Data_inserimento = DateTime.Now,
                    Data_log = DateTime.Now,
                    Data_nascita = item.DATA_NASCITA_BAMBINO,
                    Eccezione = item.ECCEZIONE,
                    Matricola = item.MATRICOLA,
                    IdHRIS = item.ID
                };
                LC.Add(C);
            }
            return LC;
        }

    }
}
