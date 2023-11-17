using myRaiCommonTasks;
using myRaiData.Incentivi;
using myRaiHelper;
using myRaiHelper.Task;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using MyRaiServiceInterface.it.rai.servizi.svilruoesercizio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsecutoreTask
{
    class Program
    {
        public enum TipoTaskUser
        {
            SERVIZIO_PAGAMENTO_ECCEZIONI,
            MODIFICA_LISTONE_STORNI
        }
        static void Main(string[] args)
        {
            var db = new IncentiviEntities();

            //var c= db.XR_MAT_GIORNI_CONGEDO.Where(x => x.ID == 860).FirstOrDefault();
            //string[] test = Utility.GetIntervalloPermessoMinuti(c, "114473");

            CommonTasks.Log("EsecutoreTask avviato");

            try
            {
                CommonTasks.Log("Controllo data esecuzione tracciati");
                CheckTaskEseguibiliDa();
            }
            catch (Exception ex)
            {
                CommonTasks.Log(ex.ToString());
            }
            
            try
            {
                ImportaEvidenze.CheckNewFile();
            }
            catch (Exception ex)
            {
                CommonTasks.Log(ex.ToString());
            } 

            string IDtaskServizioByArg = null;
            string IDtaskInCorsoByArg = null;
            string IDrichiestaByArg = null;

            if (args.Length == 2)
            {
                CommonTasks.Log("Avviato con parametri " + args[0] + " " + args[1]);
                if (args[0].ToUpper() == "SERV") IDtaskServizioByArg = args[1];
                else if (args[0].ToUpper() == "TASK") IDtaskInCorsoByArg = args[1];
                else if (args[0].ToUpper() == "RICH") IDrichiestaByArg = args[1];
                else
                {
                    Console.WriteLine("Esempi:");
                    Console.WriteLine("EsecutoreTask SERV 121 (Esegue solo task di servizio id 121)");
                    Console.WriteLine("EsecutoreTask TASK 77 (Esegue solo task pratiche utente id 77)");
                    Console.WriteLine("EsecutoreTask RICH 98 (Esegue solo pratica  id 98)");
                    return;
                }
            }

            if (IDtaskInCorsoByArg != null)
            {
                EseguiTaskInCorso(IDtaskInCorsoByArg, null);
                return;
            }

            if (IDtaskServizioByArg != null)
            {
                EseguiTaskServizio(IDtaskServizioByArg, null);
                return;
            }

            EseguiTaskServizio(IDtaskServizioByArg, IDrichiestaByArg);
            EseguiTaskInCorso(IDtaskInCorsoByArg, IDrichiestaByArg);

            //try
            //{
            //    EseguiTaskGenerici();
            //}
            //catch (Exception ex)
            //{
            //    CommonTasks.Log("Eccezione su task generici: " +ex.Message);
            //}
        }

        

        public static DateTime? GetScadenzarioPerIdUfficio(string idUfficio, int anno, int mese)
        {
            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew s = new MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew();
            s.Credentials = new System.Net.NetworkCredential(
            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.ScadenzarioMeseAnnoResponse Response =
                s.GetScadenzarioMeseAnno(idUfficio, mese, anno);
            if (Response == null || Response.scadenze == null || Response.scadenze.Length == 0)
                return null;
            else
                return Response.scadenze[0].data_scadenza;
        }
        public static void MarcaEseguitoOggi(string mark)
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;

            var directoryPath = System.IO.Path.GetDirectoryName(location);
            var logPath = System.IO.Path.Combine(directoryPath, "log");
            if (!System.IO.Directory.Exists(logPath))
                System.IO.Directory.CreateDirectory(logPath);

            System.IO.File.AppendAllText(logPath + "\\daily.txt", mark + DateTime.Today.ToString("dd/MM/yyyy") + "\r\n");
        }
        public static bool IsEseguitoOggi(string mark)
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;

            var directoryPath = System.IO.Path.GetDirectoryName(location);
            var logPath = System.IO.Path.Combine(directoryPath, "log");
            var file = logPath + "\\daily.txt";
            if (System.IO.File.Exists(file))
            {
                return System.IO.File.ReadAllText(file).Contains(mark + DateTime.Today.ToString("dd/MM/yyyy"));
            }
            else
                return false;
           
        }
        private static void CheckTaskEseguibiliDa()
        {
            if (IsEseguitoOggi("TASK_EXEC_DATE"))
            {
                CommonTasks.Log("Controllo date tracciati gia eseguito oggi");
                return;
            }

            CommonTasks.Log("Controllo date tracciati avviato");
            var db = new IncentiviEntities();
            DateTime Dnow = DateTime.Now;

            List<int> idTaskTracciati = db.XR_MAT_ELENCO_TASK.Where(x => x.TIPO == "TRACCIATO")
                                        .Select(x => x.ID).ToList();
            var TaskTracciati = db.XR_MAT_TASK_IN_CORSO
                .Where(x => x.TERMINATA == false &&
                x.ESEGUIBILE_DA_DATA > Dnow &&
                idTaskTracciati.Contains(x.ID_TASK)).ToList();

            var TaskGrouped = TaskTracciati.GroupBy(x => new { x.ANNO, x.MESE }).ToList();
            foreach (var group in TaskGrouped)
            {
                CommonTasks.Log("Controllo data per " + group.Key.MESE + "/" + group.Key.ANNO);
                try
                {
                    DateTime? Dscad = GetScadenzarioPerIdUfficio("13", group.Key.ANNO, group.Key.MESE);
                    if (Dscad == null)
                    {
                        CommonTasks.Log("Data non recuperata");
                    }
                    else
                    {
                        foreach (var task in group.ToList())
                        {
                            if (task.ESEGUIBILE_DA_DATA != null && task.ESEGUIBILE_DA_DATA != Dscad)
                            {
                                CommonTasks.Log("task id " + task.ID + " eseguibile da " + Dscad.Value.ToString("dd/MM/yyyy") +
                                    " invece che " + task.ESEGUIBILE_DA_DATA.ToString("dd/MM/yyyy"));
                                task.ESEGUIBILE_DA_DATA = Dscad.Value;
                            }
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    CommonTasks.Log(ex.ToString());
                }
            }
            MarcaEseguitoOggi("TASK_EXEC_DATE");
        }

        public static void EseguiTaskInCorso(string IDtaskInCorsoByArg, string IDrichiestaByArg)
        {
            var db = new IncentiviEntities();
            DateTime Dnow = DateTime.Now;
            List<XR_MAT_RICHIESTE> ListaRichieste = new List<XR_MAT_RICHIESTE>();
            ListaRichieste = db.XR_MAT_TASK_IN_CORSO.Where(x =>
                           x.TERMINATA == false 
                           ).Select(x => x.XR_MAT_RICHIESTE).Distinct().OrderBy(x => x.DATA_INVIO_RICHIESTA).ToList();
             

            if (IDtaskInCorsoByArg != null)
            {
                CommonTasks.Log("Ricerca ID task " + IDtaskInCorsoByArg);
                int idTaskCors = Convert.ToInt32(IDtaskInCorsoByArg);
                ListaRichieste = ListaRichieste.Where(x => x.XR_MAT_TASK_IN_CORSO.Any(z => z.ID == idTaskCors)).ToList();
            }
            if (IDrichiestaByArg != null)
            {
                CommonTasks.Log("Ricerca ID richiesta " + IDrichiestaByArg);
                int idRichiesta = Convert.ToInt32(IDrichiestaByArg);
                ListaRichieste = ListaRichieste.Where(x => x.ID == idRichiesta).ToList();
            }

            if (!ListaRichieste.Any())
            {
                CommonTasks.Log("Nessun task da eseguire");
                return;
            }
            CommonTasks.Log("Trovati task da eseguire per ID richieste:" + String.Join(",", ListaRichieste.Select(x => x.ID).ToArray()));

            foreach (XR_MAT_RICHIESTE R in ListaRichieste)
            {
                DateTime? DataCessazione = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == R.MATRICOLA)
                                            .Select (x=>x.DTA_FINE_CR).FirstOrDefault();
                 
                CommonTasks.Log("Analisi in corso richiesta ID:" + R.ID);
                if (R.PRATICA_SOSPESA_DATETIME != null)
                {
                    CommonTasks.Log("Richiesta sospesa il " + R.PRATICA_SOSPESA_DATETIME.Value
                        .ToString("dd/MM/yyyy HH.mm") +" da " + R.PRATICA_SOSPESA_MATR );
                    continue;
                }

                var ListaTask = R.XR_MAT_TASK_IN_CORSO.Where(x =>
                            x.TERMINATA == false && x.ERRORE_BATCH==null)
                            .OrderBy(x => x.ANNO).ThenBy(x => x.MESE).ThenBy(x => x.PROGRESSIVO)
                            .ToList();

                if (IDtaskInCorsoByArg != null)
                    ListaTask = ListaTask.Where(x => x.ID == Convert.ToInt32(IDtaskInCorsoByArg)).ToList();

                if (!ListaTask.Any())
                {
                    CommonTasks.Log("Nessun task da eseguire");
                    continue;
                }

                foreach (var task in ListaTask)
                {

                    CommonTasks.Log("Task da eseguire id task:" + task.ID + " progressivo:" + task.PROGRESSIVO + " mese:" + task.MESE +
                                " anno:" + task.ANNO);

                    var taskElenco = db.XR_MAT_ELENCO_TASK.Where(x => x.ID == task.ID_TASK).FirstOrDefault();
                    if (taskElenco == null)
                    {
                        CommonTasks.Log("Task non in elenco");
                        break;
                    }
                    CommonTasks.Log("Nome task:" + taskElenco.NOME_TASK + " Tipo task:" + taskElenco.TIPO);
                    CommonTasks.Log(task.INPUT);

                    if (task.BLOCCATA_DATETIME != null)
                    {
                        CommonTasks.Log("Bloccato da " + task.BLOCCATA_DA_OPERATORE + " in data " + task.BLOCCATA_DATETIME);
                        continue;
                    }
                    if (DateTime.Now < task.ESEGUIBILE_DA_DATA || DateTime.Now > task.ESEGUIBILE_FINO_A_DATA)
                    {
                        CommonTasks.Log("Task eseguibile solo tra le date " + task.ESEGUIBILE_DA_DATA.ToString("dd/MM/yyyy")
                            + "-" + task.ESEGUIBILE_FINO_A_DATA.ToString("dd/MM/yyyy"));
                        if (taskElenco.NOME_TASK == "DESCRITTIVA" && task.ESEGUIBILE_FINO_A_DATA < DateTime.Now)
                        {
                            CommonTasks.Log("E' DESCRITTIVA passata, puo continuare");
                            continue;
                        }
                        else
                            break;
                    }
                    if (DataCessazione != null)
                    {
                        DateTime DataFineTask = new DateTime(task.ANNO, task.MESE, 1).AddMonths(1).AddDays(-1);

                        if (!String.IsNullOrWhiteSpace(task.NOTE) && task.NOTE.Contains("-"))
                        {
                            string[] segm = task.NOTE.Split('-');
                            if (segm.Length >= 3)
                            {
                                DateTime Dfine;
                                if (DateTime.TryParseExact(segm[2], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out Dfine))
                                {
                                    DataFineTask = Dfine;
                                }
                            }
                        }
                        if (DataCessazione <= DataFineTask)
                        {
                            CommonTasks.Log(R.MATRICOLA+ " Dipendente cessato " + DataCessazione.Value.ToString("dd/MM/yyyy") + " Datafinetask:" + DataFineTask.ToString("dd/MM/yyyy") + " campo NOTE:" +task.NOTE);
                            task.ERRORE_BATCH = "Cessato " + DataCessazione.Value.ToString("dd/MM/yyyy");
                            task.DATA_ULTIMO_TENTATIVO = DateTime.Now;
                            task.ERRORE_RESETTABILE = false;
                            db.SaveChanges();
                            continue;
                        }
                    }
                    CommonTasks.Log("Esecuzione in corso");


                    //////////////////////////////////////////////////*
                    string esito = EseguiTask(task, taskElenco);
                    //////////////////////////////////////////////////*
                    

                    if (esito == null)
                    {
                        CommonTasks.Log("Esecuzione OK");
                        task.ERRORE_BATCH = null;
                        task.TERMINATA = true;
                        task.DATA_ULTIMO_TENTATIVO = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                    {
                        CommonTasks.Log("Esito " + esito);
                        task.ERRORE_BATCH = esito;
                        task.DATA_ULTIMO_TENTATIVO = DateTime.Now;
                        db.SaveChanges();
                        if (task.XR_MAT_ELENCO_TASK.OBBLIGATORIO_PER_CONCLUSIONE == true)
                        {
                            break;
                        }
                         
                    }
                }
            }


            ///////////////////////////////////////////////////////////



            CommonTasks.Log("Ricerca task per cestino automatico");
            var list = db.XR_MAT_TASK_IN_CORSO.Where(x =>
                x.XR_MAT_ELENCO_TASK.TIPO == "TRACCIATO" &&
                x.TERMINATA == true &&
                x.DATA_ULTIMO_TENTATIVO != null
                && x.DATA_ORA_CESTINATO == null).ToList();

            if (IDtaskInCorsoByArg != null)
            {
                list = list.Where(x => x.ID == Convert.ToInt32(IDtaskInCorsoByArg)).ToList();
            }
          
            if (list.Any())
            {
                foreach (var item in list)
                {
                    CommonTasks.Log("Cestino automatico in corso per id task:" + item.ID);
                    try
                    {
                        string esitoCestino = CestinaAutomaticamente(item);
                        if (esitoCestino == null)
                        {
                            CommonTasks.Log("Cestino automatico OK");
                            item.DATA_ORA_CESTINATO = DateTime.Now;
                            db.SaveChanges();
                        }
                        else
                        {
                            CommonTasks.Log("Cestino automatico NON COMPLETATO");
                        }
                    }
                    catch (Exception ex)
                    {
                        CommonTasks.Log("Errore in CestinaAutomaticamente: " + ex.ToString());
                    }
                }
            }
            else
            {
                CommonTasks.Log("Nessun task trovato per cestino automatico");
            }
        }
        private static void EseguiTaskServizio(string IDtaskServizio, string IDrichiestaByArg)
        {
            CommonTasks.Log("Ricerca task di servizio");
            if (IDtaskServizio != null)
            {
                CommonTasks.Log("per solo ID " + IDtaskServizio);
            }
            var db = new IncentiviEntities();
            List<XR_MAT_TASK_DI_SERVIZIO> ListaTask = new List<XR_MAT_TASK_DI_SERVIZIO>();
            ListaTask = db.XR_MAT_TASK_DI_SERVIZIO.Where(x => x.TERMINATA == false)
               .OrderBy(x => x.ANNO).ThenBy(x => x.MESE).ThenBy(x => x.PROGRESSIVO).ToList();
            if (IDtaskServizio != null)
            {
                ListaTask = ListaTask.Where(x => x.ID == Convert.ToInt32(IDtaskServizio)).ToList();
            }
            if (IDrichiestaByArg != null)
            {
                ListaTask = ListaTask.Where(x => x.ID_RICHIESTA == Convert.ToInt32(IDrichiestaByArg
                    )).ToList();
            }


            if (!ListaTask.Any())
            {
                CommonTasks.Log("Nessun task di servizio da eseguire");
                return;
            }


            foreach (var task in ListaTask)
            {
                try
                {
                    EseguiTaskServizioInternal(task, db);
                }
                catch (Exception ex)
                {
                    task.DATA_ULTIMO_TENTATIVO = DateTime.Now;
                    task.TERMINATA = false;
                    task.ERRORE_BATCH = ex.ToString();
                    db.SaveChanges();
                    CommonTasks.Log(ex.ToString());
                }
            }
        }
        public static bool IsAbilitatoRaiPerMe(string matricola)
        {
            it.rai.servizi.hrga.Sedi service = new it.rai.servizi.hrga.Sedi();
            service.Timeout = 500000;
            var db = new myRaiData.digiGappEntities();
            it.rai.servizi.hrga.CategorieDatoAbilitate resp = null;
            var rowPar = db.MyRai_ParametriSistema.Where(x => x.Chiave == "GetCategoriaDatoNetCachedNolevel").FirstOrDefault();
            if (rowPar != null)
            {
                string responseDB = rowPar.Valore1;
                resp =
                      Newtonsoft.Json.JsonConvert.DeserializeObject<it.rai.servizi.hrga.CategorieDatoAbilitate>(responseDB);
            }

            Abilitazioni AB = new Abilitazioni();

            foreach (var item in resp.CategorieDatoAbilitate_Array)
            {
                AbilitazioneSede absede = new AbilitazioneSede()
                {
                    Sede = item.Codice_categoria_dato,
                    DescrSede = item.Descrizione_categoria_dato
                };
                foreach (System.Data.DataRow row in item.DT_Utenti_CategorieDatoAbilitate.Rows)
                {
                    if (row["codice_sottofunzione"].ToString() == "01GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello1.Add(ms);
                    }

                    if (row["codice_sottofunzione"].ToString() == "02GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello2.Add(ms);
                    }

                    if (row["codice_sottofunzione"].ToString() == "03GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello3.Add(ms);
                    }

                    if (row["codice_sottofunzione"].ToString() == "04GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello4.Add(ms);
                    }

                    if (row["codice_sottofunzione"].ToString() == "05GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello5.Add(ms);
                    }
                    if (row["codice_sottofunzione"].ToString() == "06GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello6.Add(ms);
                    }
                }
                AB.ListaAbilitazioni.Add(absede);
            }
            AB.ListaAbilitazioni = AB.ListaAbilitazioni.OrderBy(x => x.Sede).ToList();

            string sedeUtente = GetSedeFromAnagrafica(matricola);
            return AB.ListaAbilitazioni.Any(x => x.Sede == sedeUtente);
        }
        public static string GetSedeFromAnagrafica(string matricola)
        {
            MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceReference1.MyRaiService1Client();
            var resp = cl.recuperaUtente(matricola, DateTime.Now.ToString("dd/MM/yyyy"));
            string sedeUtente = "";
            if (string.IsNullOrWhiteSpace(resp.data.CodiceReparto) || resp.data.CodiceReparto.Trim() == "0"
                 || resp.data.CodiceReparto.Trim() == "00")
            {
                sedeUtente = resp.data.sede_gapp;
            }
            else
            {
                sedeUtente = resp.data.sede_gapp + resp.data.CodiceReparto.Trim();
            }
            return sedeUtente;
        }

        private static void EseguiTaskServizioInternal(XR_MAT_TASK_DI_SERVIZIO task, IncentiviEntities dbHris)
        {
            var dbDigigapp = new myRaiData.digiGappEntities();
            MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                 CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                 CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
          
            string CodiceProgressivo = null;

            if (!String.IsNullOrWhiteSpace(task.XR_MAT_RICHIESTE.CF_BAMBINO))
            {
                var resp =cl.GetCodiceFiscaleInfo(task.XR_MAT_RICHIESTE.CF_BAMBINO, task.XR_MAT_RICHIESTE.MATRICOLA);
                if (resp != null && resp.CFinfo!=null && resp.CFinfo.Any())
                {
                    if (!String.IsNullOrWhiteSpace(resp.CFinfo.First().CodiceProgressivo))
                    {
                        CodiceProgressivo = resp.CFinfo.First().CodiceProgressivo;
                    }
                }
                throw new Exception("PROGRESSIVO CF NON TROVATO");
            }

            WSDigigapp DigigappService = new WSDigigapp();
            DigigappService.Credentials = new System.Net.NetworkCredential(
               CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
               CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            if (task.XR_MAT_ELENCO_TASK.NOME_TASK == "INSERIMENTO ECCEZIONI")
            {
                CommonTasks.Log("Esecuzione INSERIMENTO ECCEZIONI , idTaskDiServizio:" + task.ID);
                string[] date = task.INPUT.Split(',');

                List<myRaiData.MyRai_PianoFerieBatch> ListaAggiunte = new List<myRaiData.MyRai_PianoFerieBatch>();
                bool abilitato = IsAbilitatoRaiPerMe(task.XR_MAT_RICHIESTE.MATRICOLA);
                foreach (string data in date)
                {

                    DateTime D;
                    DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);

                    XR_MAT_GIORNI_CONGEDO giornoCongedo = Utility.GetEccezioneInPianificazione(D, task.XR_MAT_RICHIESTE);
                    string eccezione = task.XR_MAT_RICHIESTE.ECCEZIONE;

                    string dalle = "";
                    string alle = "";

                    if (giornoCongedo == null)
                    {
                        CommonTasks.Log("Task id " + task.ID + " Richiesta id " + task.XR_MAT_RICHIESTE.ID +
                            " Data " + data + " Eccezione non trovata in pianificazione");
                        continue;
                    }
                    string quantita = "1";
                    if (giornoCongedo.FRUIZIONE_DECIMAL == (decimal)0.50)
                    {
                        quantita = "0,50";
                        if (giornoCongedo.FRUIZIONE == "I") eccezione += "M";
                        if (giornoCongedo.FRUIZIONE == "F") eccezione += "P";
                    }
                    else if (giornoCongedo.FRUIZIONE_DECIMAL == (decimal)0.25)
                    {
                        quantita = "0,25";
                        eccezione += "Q";
                        string[] intervallo = Utility.GetIntervalloPermessoMinuti(giornoCongedo, task.XR_MAT_RICHIESTE.MATRICOLA);
                        if (intervallo == null)
                        {
                            CommonTasks.Log("Task id " + task.ID + " Richiesta id " + task.XR_MAT_RICHIESTE.ID +
                            " Data " + data + " Impossibile calcolare intervallo Q del permesso");
                            continue;
                        }
                        else
                        {
                            dalle = intervallo[0];
                            alle = intervallo[1];
                        }
                    }
                    if (abilitato)
                    {
                        var resp = DigigappService.getEccezioni(task.XR_MAT_RICHIESTE.MATRICOLA,
                            giornoCongedo.DATA.ToString("ddMMyyyy"), "BU", 80);
                        if (resp.eccezioni != null && resp.eccezioni.Any(x => x.cod.Trim() == "SW"))
                        {
                            string esito = StornaSW(task.XR_MAT_RICHIESTE.MATRICOLA, giornoCongedo.DATA);

                            CommonTasks.Log("Task id " + task.ID + " Richiesta id " + task.XR_MAT_RICHIESTE.ID +
                            " Data " + data + "Storno SW:" + esito);
                        }
                        //le mette da raiperme il dip
                    }
                    else
                    {
                        if (CodiceProgressivo != null)
                        {
                            //SW eventuale lo cancella il batch di inserimento
                            myRaiData.MyRai_PianoFerieBatch B = new myRaiData.MyRai_PianoFerieBatch();
                            B.codice_eccezione = eccezione;
                            B.data_creazione_record = DateTime.Now;
                            B.provenienza = "HRIS/Congedi-DA_PIANOFERIE=FALSE-APPROVATORE_UFFPERS";
                            B.matricola = task.XR_MAT_RICHIESTE.MATRICOLA;
                            B.sedegapp = task.XR_MAT_RICHIESTE.SEDEGAPP;
                            B.quantita = quantita;
                            B.data_eccezione = D;
                            B.dalle = dalle;
                            B.alle = alle;
                            B.importo = CodiceProgressivo;
                            dbDigigapp.MyRai_PianoFerieBatch.Add(B);
                            ListaAggiunte.Add(B);
                        }
                        else
                        {
                            CommonTasks.Log("Task id " + task.ID + " Richiesta id " + task.XR_MAT_RICHIESTE.ID +
                           " Data " + data + "Impossibile inserire in pianoFerieBatch, CodiceProgressivo non trovato");
                        }
                    }
                }
                dbDigigapp.SaveChanges();


                task.DATA_ULTIMO_TENTATIVO = DateTime.Now;
                foreach (var item in ListaAggiunte)
                {
                    task.OUTPUT += item.id + ":" + item.data_eccezione.ToString("dd/MM/yyyy") + ",";
                }

                task.TERMINATA = true;

                dbHris.SaveChanges();

                CommonTasks.Log("Eccezioni aggiunte in PianoferieBatch: " + task.ECCEZIONE + " - " + task.INPUT + " - IDs PianoFerieBatch:" +
                   task.OUTPUT);
            }
        }

        public static string StornaSW(string matricola, DateTime data)
        {
            Storno S = new Storno();
            string esito = S.StornoEccezione(matricola, "SW", data);

            return esito;
        }

        public static TipoTaskUser? GetTipoTask(XR_MAT_TASK_IN_CORSO task)
        {
            if (task.XR_MAT_ELENCO_TASK.NOME_TASK == "AGGIORNAMENTO PAGAMENTO ECCEZIONI")
                return TipoTaskUser.SERVIZIO_PAGAMENTO_ECCEZIONI;
            else if (task.XR_MAT_ELENCO_TASK.NOME_TASK == "MODIFICA LISTONE STORNI")
                return TipoTaskUser.MODIFICA_LISTONE_STORNI;
            else
                return null;
        }




        private static string EseguiTask(XR_MAT_TASK_IN_CORSO task, XR_MAT_ELENCO_TASK taskDaElenco)
        {

            if (GetTipoTask(task) == TipoTaskUser.SERVIZIO_PAGAMENTO_ECCEZIONI)
            {
                PagamentoEccezioni P = new PagamentoEccezioni();
                string esito = P.RunTask(task);
                return esito;
            }
            if (GetTipoTask(task) == TipoTaskUser.MODIFICA_LISTONE_STORNI)
            {
                ModificaListoneStorni P = new ModificaListoneStorni();
                string esito = P.RunTask(task);
                return esito;
            }

            if (taskDaElenco.TIPO != "TRACCIATO" && taskDaElenco.TIPO != "TRACCIATO-TE")
                return "Implementazione task non trovata";

            if ((taskDaElenco.TIPO == "TRACCIATO" || taskDaElenco.TIPO == "TRACCIATO-TE") && String.IsNullOrWhiteSpace(task.INPUT))
                return "Tracciato non trovato nel campo INPUT";


            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew service =
                new MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew();

            service.Credentials = new System.Net.NetworkCredential("srvruofpo", "zaq22?mk");

            string annomese = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0');



            Records_Inseriti DewResponse = service.InserisciRecords(
                taskDaElenco.APPKEY, annomese, new string[] { task.INPUT }, (int)taskDaElenco.ID_TRACCIATO_DEW, "HRIS", task.MATRICOLA_OPERATORE);

            if (DewResponse.Esito == 0)
            {
                CommonTasks.Log("Tracciato inserito in DEW : task_in_corso id:" + task.ID);
                return null;
            }
            else
                return DewResponse.StringaErrore;
        }

        private static string CestinaAutomaticamente(XR_MAT_TASK_IN_CORSO task)
        {
            CommonTasks.Log("Controllo eccezioni per cestino automatico");
            if (task.XR_MAT_ELENCO_TASK.NOME_TASK.ToUpper().Contains("DESCRITTIVA"))
            {
                CommonTasks.Log("Nessuna eccezione da cestinare per tracciato DESCRITTIVA");
                return null;
            }

            List<DateTime> Date = Utility.GetDateEccezioni(task);

            if (Date == null || Date.Any() == false)
            {
                CommonTasks.Log("Nessuna eccezione da cestinare");
                return null;
            }

            string ecc = task.NOTE.Split('-')[0];
            string statoCestinata = CommonHelper.GetParametri<string>(EnumParametriSistema.StatoEccezioneCongedi)[1];
            string matricola = task.XR_MAT_RICHIESTE.MATRICOLA;

            MyRaiServiceReference1.MyRaiService1Client WCFservice = new MyRaiServiceReference1.MyRaiService1Client();
            WCFservice.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                   CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                   CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);


            DateTime D1 = new DateTime(Date.First().Year, Date.First().Month, 1);
            CommonTasks.Log("Chiamata a getRuoli per " + matricola+ " " + D1.ToString("dd/MM/yyyy"));

            var Response = WCFservice.GetRuoli(matricola, D1, "   ");
            if (Response !=null)
                CommonTasks.Log("Response raw:" + Response.raw);

            if (Response.Eccezioni == null || Response.Eccezioni.Any() == false)
            {
                CommonTasks.Log("Nessuna eccezione nella raccolta, item non verrà marcato come cestinato");
                return "Nessuna eccezione trovata nella raccolta";
            }
            foreach (var data in Date)
            {
                CommonTasks.Log("Controllo eccezione " + ecc + " " + data.ToString("dd/MM/yyyy"));
                var EcceRaccolta = Response.Eccezioni.Where(x => x.DataDocumento == data && x.CodiceEccezione.Trim() == ecc).FirstOrDefault();
                if (EcceRaccolta == null)
                {
                    CommonTasks.Log("Non presente in raccolta");
                }
                else
                {
                    CommonTasks.Log("Presente in raccolta. Invio stato " + statoCestinata);
                    try
                    {
                        MyRaiServiceInterface.MyRaiServiceReference1.SetStatoEccezioneResponse SetStatoResponse =
                            WCFservice.SetStatoEccezione("ESSWEB", data, matricola, null, ecc, statoCestinata);

                        if (SetStatoResponse.esito == true)
                        {
                            CommonTasks.Log("Scrittura confermata");
                        }
                        else
                        {
                            CommonTasks.Log("Errore nella risposta: " + SetStatoResponse.error);
                            return "Errore nella risposta: " + SetStatoResponse.error;
                        }

                    }
                    catch (Exception ex)
                    {
                        CommonTasks.Log("Errore chiamata servizio: " + ex.ToString());
                        return "Errore chiamata servizio: " + ex.ToString();
                    }
                }
            }
            return null;
        }

        //private static void EseguiTaskGenerici()
        //{
        //    //Controllo prima le schedulazioni così da trovare i task già pronti
        //    TaskHelper.CreateTaskFromSchedule();

        //    var db = new IncentiviEntities();
        //    var listTask = db.XR_TSK_TASK.Where(x => x.IND_ESITO == null).ToList();

        //    if (!listTask.Any())
        //    {
        //        CommonTasks.Log("Nessun task generico da eseguire");
        //        return;
        //    }
        //    CommonTasks.Log($"Trovati {listTask.Count()} task generici da eseguire");
        //    var tipiTask = db.XR_TSK_TIPOLOGIE.ToList();

        //    foreach (var task in listTask)
        //    {
        //        bool hasParamError = false;
        //        if (String.IsNullOrWhiteSpace(task.COD_TIPOLOGIA))
        //        {
        //            hasParamError = true;
        //            task.IND_ESITO = false;
        //            task.NOT_ERRORE = "Tipologia non indicata";
        //        }
        //        else if (!tipiTask.Any(x=>x.COD_TIPOLOGIA==task.COD_TIPOLOGIA && x.COD_SOTTOTIPOLOGIA==task.COD_SOTTOTIPOLOGIA))
        //        {
        //            hasParamError = true;
        //            task.IND_ESITO = false;
        //            task.NOT_ERRORE = "Tipologia inesistente";
        //        }
        //        else if (String.IsNullOrWhiteSpace(task.INPUT))
        //        {
        //            hasParamError = true;
        //            task.IND_ESITO = false;
        //            task.NOT_ERRORE = "Nessun input indicato";
        //        }
        //        else
        //        {
        //            object instTask = null;
        //            try
        //            {
        //                Type type = Type.GetType($"myRaiHelper.Task.{task.COD_TIPOLOGIA}, myRaiHelper");
        //                instTask = Newtonsoft.Json.JsonConvert.DeserializeObject(task.INPUT, type);
        //            }
        //            catch (Exception ex)
        //            {
        //                hasParamError = true;
        //                task.IND_ESITO = false;
        //                task.NOT_ERRORE = ex.Message;
        //            }
                    
        //            try
        //            {
        //                if (instTask != null)
        //                {
        //                    ((BaseTask)instTask).SetStart(task);
        //                    if (!((BaseTask)instTask).CheckParam(out string errParam))
        //                    {
        //                        hasParamError = true;
        //                        task.IND_ESITO = false;
        //                        task.NOT_ERRORE = errParam;
        //                    }
        //                    else
        //                    {
        //                        task.IND_ESITO = ((BaseTask)instTask).Esegui(out string output, out string errore);
        //                        task.OUTPUT = output;
        //                        task.NOT_ERRORE = errore;
        //                    }
        //                    ((BaseTask)instTask).SetEnd(task);

        //                    ((BaseTask)instTask).ManageMail(task);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                task.IND_ESITO = false;
        //                task.NOT_ERRORE = ex.Message;
        //            }
        //        }

        //        if (!task.IND_ESITO.GetValueOrDefault() && !hasParamError && task.IND_RIESEGUI)
        //        {
        //            XR_TSK_TASK newTask = new XR_TSK_TASK()
        //            {
        //                ID_ORIGINALE = task.ID_ORIGINALE ?? task.ID,
        //                COD_TIPOLOGIA = task.COD_TIPOLOGIA,
        //                DATA_CREAZIONE = DateTime.Now,
        //                MATRICOLA_CREATORE = task.MATRICOLA_CREATORE,
        //                INPUT = task.INPUT
        //            };
        //            db.XR_TSK_TASK.Add(newTask);
        //        }
        //        db.SaveChanges();

                
        //    }
        //}
    }

    public class MatrSedeAppartenenza
    {
        public string Matricola { get; set; }
        public string SedeAppartenenza { get; set; }
        public string Delegante { get; set; }
        public string Delegato { get; set; }
    }
    public class Abilitazioni
    {
        public Abilitazioni()
        {
            ListaAbilitazioni = new List<AbilitazioneSede>();
        }
        public List<AbilitazioneSede> ListaAbilitazioni { get; set; }
    }
    public class AbilitazioneSede
    {
        public AbilitazioneSede()
        {
            MatrLivello1 = new List<MatrSedeAppartenenza>();
            MatrLivello2 = new List<MatrSedeAppartenenza>();
            MatrLivello3 = new List<MatrSedeAppartenenza>();
            MatrLivello4 = new List<MatrSedeAppartenenza>();
            MatrLivello5 = new List<MatrSedeAppartenenza>();
            MatrLivello6 = new List<MatrSedeAppartenenza>();
        }
        public string Sede { get; set; }
        public string DescrSede { get; set; }
        public List<MatrSedeAppartenenza> MatrLivello1 { get; set; }
        public List<MatrSedeAppartenenza> MatrLivello2 { get; set; }

        /// <summary>
        /// Livello 3 è in realtà un livello 1 con codice 03GEST. Approvatori per produzione
        /// </summary>
        public List<MatrSedeAppartenenza> MatrLivello3 { get; set; }
        public List<MatrSedeAppartenenza> MatrLivello4 { get; set; }
        public List<MatrSedeAppartenenza> MatrLivello5 { get; set; }
        public List<MatrSedeAppartenenza> MatrLivello6 { get; set; }
    }
}
