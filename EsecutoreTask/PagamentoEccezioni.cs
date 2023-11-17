using myRaiCommonTasks;
using myRaiData.Incentivi;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using MyRaiServiceInterface.it.rai.servizi.svilruoesercizio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsecutoreTask
{
  
    public class PagamentoEccezioni
    {
        public WSDigigapp DigigappService { get; set; }
        public MyRaiServiceReference1.MyRaiService1Client WCFservice { get; set; }
        public WSDew DEWservice { get; set; }
        public PagamentoEccezioni()
        {
            DEWservice = new WSDew();
            DEWservice.Credentials = new System.Net.NetworkCredential(
              CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
              CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            DigigappService = new WSDigigapp();
            DigigappService.Credentials = new System.Net.NetworkCredential(
               CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
               CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            WCFservice = new MyRaiServiceReference1.MyRaiService1Client();
        }
        public string RunTask(XR_MAT_TASK_IN_CORSO task)
        {
             

            var db = new myRaiData.Incentivi.IncentiviEntities();
            DateTime D = new DateTime(task.ANNO, task.MESE, 1).AddMonths(-1);

            CommonTasks.Log("Ricerca tracciati di riferimento per pagamento eccezioni");

            var taskPerRichiesta = db.XR_MAT_TASK_IN_CORSO
                .Where(x => x.ID_RICHIESTA == task.ID_RICHIESTA && x.ANNO == D.Year && x.MESE == D.Month)
                .OrderBy(x => x.PROGRESSIVO)
                .ToList();
            if (taskPerRichiesta.Any() == false)
            {
                CommonTasks.Log("Nessun tracciato di riferimento per id richiesta " + task.ID_RICHIESTA);
                return "Nessun tracciato di riferimento per id richiesta " + task.ID_RICHIESTA;
            }
            List<EccezioneData> Ldate = new List<EccezioneData>();
            foreach (var t in taskPerRichiesta)
            {
                if (t.XR_MAT_ELENCO_TASK.TIPO != "TRACCIATO") continue;

                if (String.IsNullOrWhiteSpace(t.NOTE)) continue; //storno

                CommonTasks.Log("Analisi tracciato id " + t.ID);


                CommonTasks.Log("Recupero ANNO/MESE COMPETENZA dal tracciato");


                if (t.BLOCCATA_DATETIME != null)
                {
                    CommonTasks.Log("Tracciato in sospeso, non verrà considerato");
                    continue;
                }
                DateTime? DataPagamento = Utility.GetDataPagamentoEccezioni(t, DEWservice);
                if (DataPagamento == null)
                {
                    CommonTasks.Log("Impossibile recuperare data pagamento dal tracciato");
                    return "Impossibile recuperare data pagamento dal tracciato";
                }

                if (String.IsNullOrWhiteSpace(t.NOTE))
                {
                    CommonTasks.Log("Impossibile recuperare campo NOTE");
                    return "Impossibile recuperare campo NOTE";
                }

                string Eccezione = t.NOTE.Split('-')[0];
                List<DateTime> LdateThisTracciato = Utility.GetDateEccezioni(t);

                if (LdateThisTracciato == null)
                {
                    CommonTasks.Log("Impossibile recuperare date eccezioni dal tracciato");
                    return "Impossibile recuperare date eccezioni dal tracciato";
                }
                foreach (DateTime Da in LdateThisTracciato)
                {
                    Ldate.Add(new EccezioneData() { DataEccezione = Da, Eccezione = Eccezione, DataPagamento=DataPagamento.Value });
                }
            }
            foreach (EccezioneData  DatiEccezione in Ldate)
            {
                CommonTasks.Log("Matricola " + task.XR_MAT_RICHIESTE.MATRICOLA + " Eccezione " + DatiEccezione.Eccezione +
                    " - Data " + DatiEccezione.DataEccezione.ToString("dd/MM/yyyy") +
                    " - DataPagamento" + DatiEccezione.DataPagamento.ToString("dd/MM/yyyy"));

                string NumDoc = Utility.GetNumeroDocumento(DigigappService, task.XR_MAT_RICHIESTE.MATRICOLA, 
                    DatiEccezione.Eccezione, DatiEccezione.DataEccezione);
                if (NumDoc == null)
                    CommonTasks.Log("Impossibile ottenere numero documento eccezione, saltata");

                CommonTasks.Log("Invio richiesta pagamento per eccezione, numdoc " + NumDoc);

                try
                {
                    MyRaiServiceInterface.MyRaiServiceReference1.SetPagamentoEccezioneResponse Response =
                                WCFservice.SetPagamentoEccezione("ESSWEB", DatiEccezione.DataEccezione, task.XR_MAT_RICHIESTE.MATRICOLA,
                                (DateTime)DatiEccezione.DataPagamento, DatiEccezione.Eccezione, NumDoc);

                    if (Response.esito == true)
                        CommonTasks.Log("Esito true");
                    else
                        CommonTasks.Log("Esito false, Errore " + Response.error);
                }
                catch (Exception ex)
                {
                    CommonTasks.Log(ex.ToString());
                    return ex.Message;
                }
            }
            return null;
        }
    }

    public class EccezioneData
    {
        public string Eccezione { get; set; }
        public DateTime DataEccezione { get; set; }
        public DateTime DataPagamento { get; set; }
    }
}
