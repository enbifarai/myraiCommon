using myRaiCommonManager;
using myRaiCommonModel.Gestionale;
using myRaiCommonTasks;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaiBatch.RaiMbo
{
    public class MainClass : BatchBaseClass
    {
        public override void Entry(string[] args)
        {
            string mainOper = args[1];
            switch (mainOper)
            {
                case "CheckInvioRuo":
                    CheckInvioRuo();
                    break;
                case "ReminderConvalida":
                    InvioReminderConvalida();
                    break;
                case "CopiaScheda":
                    CopiaScheda();
                    break;
                default:
                    break;
            }
        }

        private void CheckInvioRuo()
        {
            if (MboManager.InvioARuo(out string errorMsg))
            {
                _log.Info("Invio effettuato");
            }
            else
            {
                _log.Error(errorMsg);
            }
        }

        private void InvioReminderConvalida()
        {
            var limitMin = DateTime.Today.AddDays(-1);
            var limitMax = DateTime.Today;

            var db = new IncentiviEntities();
            var tmpElenco = db.XR_MBO_SCHEDA
                    .Where(x => x.XR_MBO_INIZIATIVA.VALID_DTA_END == null || x.XR_MBO_INIZIATIVA.VALID_DTA_END > DateTime.Now)
                    .Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now);

            int[] stato = { (int)MboState.Convalidati };

            //var listaConvalidati = tmpElenco.Where(x => x.ID_PERSONA_RESP>0 && x.XR_MBO_OPERSTATI.Any(y => y.ID_STATO == (int)MboState.Convalidati && !y.VALID_DTA_END.HasValue && y.VALID_DTA_INI>=limitMin && y.VALID_DTA_INI<limitMax))
            //                        .Select(x=>new { x, MboState.Convalidati }).ToList();

            var listaConvalidati = tmpElenco.Where(x => stato.Contains(x.XR_MBO_OPERSTATI.Where(y => y.ID_STATO != 0 && (!y.VALID_DTA_END.HasValue || y.VALID_DTA_END > DateTime.Now))
                                                                            .OrderByDescending(z => z.XR_MBO_STATI.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE).FirstOrDefault().ID_STATO))
                                            .Select(x => new
                                            {
                                                x,
                                                Stato = x.XR_MBO_OPERSTATI.Where(y => y.ID_STATO != 0 && (!y.VALID_DTA_END.HasValue || y.VALID_DTA_END > DateTime.Now))
                                                                            .OrderByDescending(z => z.XR_MBO_STATI.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                                                                            .FirstOrDefault()
                                            })
                                            .Where(x => x.Stato.DTA_OPERAZIONE >= limitMin && x.Stato.DTA_OPERAZIONE < limitMax);

            foreach (var grResp in listaConvalidati.GroupBy(x => x.x.ID_PERSONA_RESP))
            {
                var sintResp = grResp.First().x.SINTESI_Responsabile;
                string destinatario = "careddu.nicolo@datamanagementitalia.it";// CommonHelper.GetEmailPerMatricola(sintResp.COD_MATLIBROMAT);

                string bloccoRich = "<ul>";
                foreach (var item in grResp)
                {
                    var sintVal = item.x.SINTESI_Valutato;
                    string divVal = "<div>";
                    divVal += "<b>" + sintVal.Nominativo() + "</b>";
                    divVal += "<br/><small>" + CezanneHelper.GetDes(sintVal.COD_UNITAORG, sintVal.DES_DENOMUNITAORG) + "</small>";

                    bloccoRich += "<li>" + divVal + "</li>";
                }
                bloccoRich += "</ul>";

                string oggetto = "MBO - Schede convalidate " + limitMin.ToString("dd/MM/yyyy");

                string testo = "<p>Gentile " + sintResp.Nominativo() + ",<br/><br/>in data " + limitMin.ToString("dd/MM/yyyy") + " ";
                if (grResp.Count() == 1)
                    testo += "è stata convalidata la scheda di:";
                else
                    testo += "sono state convalidate le schede di:";
                testo += bloccoRich;

                testo += "<br/><br/>";

                GestoreMail mail = new myRaiCommonTasks.GestoreMail();
                var response = mail.InvioMail(testo, oggetto, destinatario, null, "raiplace.selfservice@rai.it", null, null);

            }
        }

        private void CopiaScheda()
        {

        }
    }
}
