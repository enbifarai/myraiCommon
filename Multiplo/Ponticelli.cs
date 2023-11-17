using myRaiData.Incentivi;
using myRaiHelper;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplo
{
    public class Ponticelli
    {
        public static string GetQueryVariabiliTE1(string matricola, int anno, int mese)
        {
            string query = myRaiHelper.CommonHelper.GetParametro<string>(EnumParametriSistema.MaternitaCongediTE1)

            .Replace("#MATR", matricola).Replace("#ANNO", anno.ToString()).Replace("#MESE", mese.ToString());
            return query;
        }
        public static void ElaboraPonticelliCongedi()
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            var list = db.XR_MAT_ARRETRATI_DIPENDENTE.Where(x =>
            x.DATA_ULTIMO_CK_PONTICELLI == null &&
            (x.DATA == x.PERIODO_RIFERIMENTO_DA || x.DATA == x.PERIODO_RIFERIMENTO_A) &&
            (x.ECCEZIONE.StartsWith("AF") || x.ECCEZIONE.StartsWith("BF") || x.ECCEZIONE.StartsWith("CF"))
            )
           .OrderBy(x => x.MATRICOLA).ThenBy(x => x.PERIODO_RIFERIMENTO_DA)
           .ToList();

            var ListMatrCensimentoCompl = db.XR_MAT_CENSIMENTO_CF_CONGEDI_EXTRA.Select(x => x.MATRICOLA).Distinct().ToList();
            list.RemoveAll(x => ! ListMatrCensimentoCompl.Contains(x.MATRICOLA));
          
            myRaiCommonTasks.CommonTasks.Log("Ponticelli da esaminare su XR_MAT_ARRETRATI_DIPENDENTE:" + list.Count());

            foreach (var item in list)
            {
                myRaiCommonTasks.CommonTasks.Log($"Id:{item.ID} matr: {item.MATRICOLA} {item.PERIODO_RIFERIMENTO_DA.Value.ToString("dd/MM/yyyy") } {item.PERIODO_RIFERIMENTO_A.Value.ToString("dd/MM/yyyy") }");

                //segna giorni aggiuntivi su inizio e fine del periodo :
                if (item.PERIODO_RIFERIMENTO_DA == item.DATA)
                {
                    XR_MAT_RICHIESTE Richiesta = new XR_MAT_RICHIESTE()
                    {
                        ECCEZIONE = item.ECCEZIONE,
                        INIZIO_GIUSTIFICATIVO = item.PERIODO_RIFERIMENTO_DA,
                        FINE_GIUSTIFICATIVO = item.PERIODO_RIFERIMENTO_A,
                        MATRICOLA=item.MATRICOLA
                    };
                    string q = GetQueryVariabiliTE1(item.MATRICOLA, item.PERIODO_RIFERIMENTO_DA.Value.Year, item.PERIODO_RIFERIMENTO_A.Value.Month);

                    IEnumerable<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico1> varsTE1 =
                            db.Database.SqlQuery<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico1>
                            (q).ToList();
                    string FormaContratto = varsTE1.Select(x => x.forma_contratto).FirstOrDefault();

                    GetSchedaPresenzeMeseResponse timbr = myRaiCommonManager.MaternitaCongediManager.GetDatiPeriodo(item.PERIODO_RIFERIMENTO_DA.Value, item.MATRICOLA);
                    DateTime? DateChanged = myRaiCommonManager.MaternitaCongediManager.ControllaSabDomInizio(
                        item.PERIODO_RIFERIMENTO_DA.Value, item.PERIODO_RIFERIMENTO_A, Richiesta, timbr, FormaContratto);

                    if (DateChanged != null)
                    {
                        double GiorniAggiuntiPonticelli = (item.PERIODO_RIFERIMENTO_DA.Value - DateChanged.Value).TotalDays;
                        item.GIORNI_AGG_PONTICELLI_TESTA = (decimal)GiorniAggiuntiPonticelli;
                        myRaiCommonTasks.CommonTasks.Log($"Id:{item.ID} matr: {item.MATRICOLA} periodo:{item.PERIODO_RIFERIMENTO_DA} - {item.PERIODO_RIFERIMENTO_A} Giorni aggiunti testa :{GiorniAggiuntiPonticelli}");
                    }
                }
                if (item.PERIODO_RIFERIMENTO_A == item.DATA)
                {
                    XR_MAT_RICHIESTE Richiesta = new XR_MAT_RICHIESTE()
                    {
                        ECCEZIONE = item.ECCEZIONE,
                        INIZIO_GIUSTIFICATIVO = item.PERIODO_RIFERIMENTO_DA,
                        FINE_GIUSTIFICATIVO = item.PERIODO_RIFERIMENTO_A,
                        MATRICOLA = item.MATRICOLA
                    };
                    string q = GetQueryVariabiliTE1(item.MATRICOLA, item.PERIODO_RIFERIMENTO_DA.Value.Year, item.PERIODO_RIFERIMENTO_A.Value.Month);

                    IEnumerable<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico1> varsTE1 =
                            db.Database.SqlQuery<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico1>
                            (q).ToList();
                    string FormaContratto = varsTE1.Select(x => x.forma_contratto).FirstOrDefault();

                    GetSchedaPresenzeMeseResponse timbr = myRaiCommonManager.MaternitaCongediManager.GetDatiPeriodo(item.PERIODO_RIFERIMENTO_DA.Value, item.MATRICOLA);
                    DateTime? DateChanged = myRaiCommonManager.MaternitaCongediManager.ControllaSabDomFine(
                        item.PERIODO_RIFERIMENTO_DA.Value, item.PERIODO_RIFERIMENTO_A, Richiesta, timbr, FormaContratto);

                    if (DateChanged != null)
                    {
                        double GiorniAggiuntiPonticelli = ( DateChanged.Value - item.PERIODO_RIFERIMENTO_A.Value ).TotalDays;
                        item.GIORNI_AGG_PONTICELLI_CODA = (decimal)GiorniAggiuntiPonticelli;
                        myRaiCommonTasks.CommonTasks.Log($"Id:{item.ID} matr: {item.MATRICOLA} periodo:{item.PERIODO_RIFERIMENTO_DA} - {item.PERIODO_RIFERIMENTO_A} Giorni aggiunti coda :{GiorniAggiuntiPonticelli}");
                    }
                }
                item.DATA_ULTIMO_CK_PONTICELLI = DateTime.Now;
                db.SaveChanges();
            }

        }
    }
}
