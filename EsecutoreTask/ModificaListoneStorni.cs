using myRaiCommonTasks;
using myRaiData.Incentivi;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.svilruoesercizio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsecutoreTask
{
    public class ModificaListoneStorni
    {
        public WSDew DEWservice { get; set; }
        public string RunTask(XR_MAT_TASK_IN_CORSO task)
        {
            DEWservice = new WSDew();
            DEWservice.Credentials = new System.Net.NetworkCredential(
              CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
              CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            DateTime D = new DateTime(task.ANNO, task.MESE, 1);
            CommonTasks.Log("Ricerca tramite servizio dew di record da cancellare : POSTD9701, " +
                task.XR_MAT_RICHIESTE.ECCEZIONE + "," + task.XR_MAT_RICHIESTE.MATRICOLA + D.ToString("yyMM"));

            Tracciato_richiamato[] response = null;

            try
            {
                response = DEWservice.GetRecordDaCancellare("POSTD9701", "",//task.XR_MAT_RICHIESTE.ECCEZIONE,
                                   task.XR_MAT_RICHIESTE.MATRICOLA + D.ToString("yyMM"), null);
                response= myRaiCommonTasks.CommonTasks.GetRecordRewDaCancellare(response, task.XR_MAT_RICHIESTE, D);
            }
            catch (Exception ex)
            {
                return "Errore chiamata servizio: " + ex.ToString();
            }
            if (response == null || !response.Any())
            {
                CommonTasks.Log("Nessun record trovato");
                return null;
            }
            if (response != null & response.Any())
            {
                string ContenutoTracciati = "";

                foreach (var row in response)
                {
                    CommonTasks.Log("Record id " + row.Id_tracciato_richiamato + ", flag_cancellazione:" + row.Flag_cancellazione);
                    if (row.Flag_cancellazione != "1 " && row.Flag_cancellazione.Trim() != "1")
                    {
                        CommonTasks.Log("Chiamata servizio CancellaRecordRew");

                        ChangeRecordRewResponse respCanc = DEWservice.CancellaRecordRew("AzWtNLyBD44Aw012", row.Id_tracciato_richiamato,
                         task.MATRICOLA_OPERATORE);

                        if (respCanc.esito == false)
                        {
                            CommonTasks.Log("Errore cancellazione:" + respCanc.errore);
                            return "Errore cancellazione:" + respCanc.errore;
                        }
                        else
                        {
                            ContenutoTracciati += row.Testo_record + ",";
                            CommonTasks.Log("Cancellato");
                        }
                    }
                    else
                    {
                        CommonTasks.Log("Record id " + row.Id_tracciato_richiamato + " è gia cancellato");
                    }
                }
                if (!String.IsNullOrWhiteSpace(ContenutoTracciati))
                {
                    try
                    {
                        var db = new IncentiviEntities();
                        var taskInCorso = db.XR_MAT_TASK_IN_CORSO.Where(x => x.ID == task.ID).FirstOrDefault();
                        if (taskInCorso != null)
                        {
                            taskInCorso.OUTPUT = ContenutoTracciati;
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        CommonTasks.Log("Errore aggiornamento OUTPUT per listone storni:" + ex.ToString());
                    }
                }
            }
            return null;
        }
    }
}
