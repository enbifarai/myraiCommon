using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using myRai.DataAccess;
using myRaiCommonModel.Gestionale;
using myRaiData.Incentivi;
using myRaiHelper;
using myRaiCommonModel.cvModels.Pdf;
using myRaiCommonTasks;
using System.Web.Mvc;
using myRai.Business;
using ClosedXML.Excel;

namespace myRaiCommonManager
{
    public class MboManager
    {
        public static MboParametri GetParametri()
        {
            MboParametri parametri = null;
            var db = new IncentiviEntities();
            var tmpParam = db.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "MboParametri");
            if (tmpParam != null && !String.IsNullOrWhiteSpace(tmpParam.COD_VALUE1))
            {
                try
                {
                    parametri = Newtonsoft.Json.JsonConvert.DeserializeObject<MboParametri>(tmpParam.COD_VALUE1);
                }
                catch (Exception)
                {

                }
            }

            if (parametri == null)
                parametri = new MboParametri();

            return parametri;
        }
        public static MboIniziativa GetIniziativa(int idIniz)
        {
            MboIniziativa iniziativa = null;
            var db = new IncentiviEntities();

            if (idIniz == 0)
            {
                iniziativa = new MboIniziativa();
            }
            else
            {
                var dbIniz = db.XR_MBO_INIZIATIVA.FirstOrDefault(x => x.ID_INIZIATIVA == idIniz);
                if (dbIniz != null)
                    iniziativa = new MboIniziativa(dbIniz);
            }

            iniziativa.SchedePossibili = new List<SelectListItem>();
            iniziativa.SchedePossibili.Add(new SelectListItem() { Value = "", Text = "Seleziona un'iniziativa di valutazione" });
            iniziativa.SchedePossibili.AddRange(db.XR_VAL_CAMPAIGN.Where(x => x.VALID_DTA_END == null && x.COD_TIPOLOGIA == "MBO")
                                                    .SelectMany(x => x.XR_VAL_CAMPAIGN_SHEET.Where(y => y.VALID_DTA_END == null))
                                                    .ToList()
                                                    .Select(x => new SelectListItem()
                                                    {
                                                        Value = x.ID_CAMPAIGN_SHEET.ToString(),
                                                        Text = x.XR_VAL_CAMPAIGN.NAME + " - " + x.DESCRIPTION
                                                    }));

            return iniziativa;
        }

        public static List<MboIniziativa> GetIniziative()
        {
            List<MboIniziativa> result = new List<MboIniziativa>();

            var db = new IncentiviEntities();

            var elenco = db.XR_MBO_INIZIATIVA.ToList();
            foreach (var item in elenco)
            {
                var iniz = new MboIniziativa(item);
                result.Add(iniz);
            }

            return result;
        }

        public static List<MboScheda> GetSchede(MboRicerca model, bool isRPM = false, string matricola = "", bool loadVal = false)
        {
            List<MboScheda> result = new List<MboScheda>();

            var db = new IncentiviEntities();

            var tmpElenco = db.XR_MBO_SCHEDA
                                .Where(x => x.XR_MBO_INIZIATIVA.VALID_DTA_END == null || x.XR_MBO_INIZIATIVA.VALID_DTA_END > DateTime.Now)
                                .Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now);

            if (isRPM && !String.IsNullOrWhiteSpace(matricola))
                tmpElenco = tmpElenco.Where(x => (x.SINTESI_Responsabile != null && x.SINTESI_Responsabile.COD_MATLIBROMAT == matricola)
                                                    || (x.SINTESI1_SecRiporto != null && x.SINTESI1_SecRiporto.COD_MATLIBROMAT == matricola));

            if (model != null && model.HasFilter)
            {
                if (!String.IsNullOrWhiteSpace(model.Matricola))
                    tmpElenco = tmpElenco.Where(x => model.Matricola.Contains(x.SINTESI_Valutato.COD_MATLIBROMAT));

                if (!String.IsNullOrWhiteSpace(model.Nominativo))
                    tmpElenco = tmpElenco.Where(x => (x.SINTESI_Valutato.DES_COGNOMEPERS + " " + x.SINTESI_Valutato.DES_NOMEPERS).StartsWith(model.Nominativo.ToUpper())
                                                    || (x.SINTESI_Valutato.DES_NOMEPERS + " " + x.SINTESI_Valutato.DES_COGNOMEPERS).StartsWith(model.Nominativo.ToUpper()));

                if (model.Stato.HasValue && model.Stato.Value > 0)
                {
                    var isCurrentState = IsCurrentState(model.Stato.Value);
                    tmpElenco = tmpElenco.Where(isCurrentState);
                }

                if (model.Responsabile.HasValue)
                {
                    tmpElenco = tmpElenco.Where(x => x.ID_PERSONA_RESP == model.Responsabile.Value);
                }

                if (model.SecondoRiporto.HasValue)
                {
                    tmpElenco = tmpElenco.Where(x => x.ID_PERS_RIPORTO == model.SecondoRiporto.Value);
                }

                if (model.Consuntivatore.HasValue)
                {
                    tmpElenco = tmpElenco.Where(x => x.ID_PERSONA_CONSUNTIVAZIONE == model.Consuntivatore.Value || (x.ID_PERSONA_CONSUNTIVAZIONE == null && x.ID_PERSONA_RESP == model.Consuntivatore.Value));
                }

                if (!String.IsNullOrWhiteSpace(model.Direzione))
                    tmpElenco = tmpElenco.Where(x => x.SINTESI_Valutato.COD_SERVIZIO == model.Direzione);

                if (model.IdIniziativa.HasValue)
                    tmpElenco = tmpElenco.Where(x => x.ID_INIZIATIVA == model.IdIniziativa);
            }

            Expression<Func<XR_MBO_SCHEDA, bool>> enabledStates = null;
            if (isRPM)
            {
                enabledStates = IsCurrentState((int)MboState.DaCompilare, (int)MboState.ObiettiviSottopostiAlDir, (int)MboState.ProntiPerInvioRuo, (int)MboState.ObiettiviSottopostiRuo, (int)MboState.Convalidati);
                tmpElenco = tmpElenco.Where(enabledStates);
            }

            result = tmpElenco.Select(x => new MboScheda()
            {
                Id = x.ID_SCHEDA,
                IdPersonaResp = x.ID_PERSONA_RESP,
                IdPersonaValutato = x.ID_PERSONA_VALUTATO,
                IdPersonaSecRiporto = x.ID_PERS_RIPORTO,
                IdPersonaConsuntivazione = x.ID_PERSONA_CONSUNTIVAZIONE,
                ImportoTeorico = x.DEC_IMPORTO_TEORICO,
                Approvato = x.IND_APPROVED,
                IsSent = x.IND_SENT,
                IsApproved = x.IND_APPROVED,
                IsConsuntivazioneBlocked = x.IND_INIBISCI_CONSUNTIVAZIONE.HasValue ? x.IND_INIBISCI_CONSUNTIVAZIONE.Value : false,
                IsChief = x.IND_CHIEF.HasValue ? x.IND_CHIEF.Value : false,

                StatoCorrente = x.XR_MBO_OPERSTATI.Where(z => !z.VALID_DTA_END.HasValue & z.ID_STATO != 0).Select(w => w.XR_MBO_STATI)
                                    .OrderByDescending(s => s.XR_WKF_WORKFLOW.FirstOrDefault(y => y.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE).FirstOrDefault(),
                Ordine = x.XR_MBO_OPERSTATI.Where(z => !z.VALID_DTA_END.HasValue & z.ID_STATO != 0).Select(w => w.XR_MBO_STATI)
                                .OrderByDescending(s => s.XR_WKF_WORKFLOW.FirstOrDefault(y => y.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE).FirstOrDefault().XR_WKF_WORKFLOW.FirstOrDefault(z => z.ID_TIPOLOGIA == x.ID_TIPOLOGIA),
                Completamento = x.NMB_RESULT,
                DBLivello = x.COD_LIV,
                DBPersResp = x.SINTESI_Responsabile,
                DBPersVal = x.SINTESI_Valutato,
                DbPersRiporto = x.SINTESI1_Riporto,
                DbPersCons = x.SINTESI1_Consuntivatore,
                DBInziativa = x.XR_MBO_INIZIATIVA
            }).ToList();

            foreach (var item in result)
            {
                switch (item.DBLivello)
                {
                    case "Top":
                        item.CodLivelloDir = LivelloDirigente.Top;
                        break;
                    case "Full":
                        item.CodLivelloDir = LivelloDirigente.Full;
                        break;
                    case "Manager":
                        item.CodLivelloDir = LivelloDirigente.Manager;
                        break;
                    default:
                        break;
                }

                if (item.DBPersResp != null)
                    item.PersResp = new MboPersona(item.DBPersResp);

                if (item.DBPersVal != null)
                    item.PersValutato = new MboPersona(item.DBPersVal);

                if (item.DbPersRiporto != null)
                    item.PersSecRiporto = new MboPersona(item.DbPersRiporto);

                if (item.DbPersCons != null)
                    item.PersConsuntivazione = new MboPersona(item.DbPersCons);

                item.Iniziativa = new MboIniziativa(item.DBInziativa);

                //Carico anche l'elenco delle schede compilate da riporti, di cui non sono il 2° riporto

                if (isRPM && item.IsChief)
                {
                    var tmpSubList = db.XR_MBO_SCHEDA.Where(x => x.ID_INIZIATIVA == item.Iniziativa.Id && x.SINTESI_Responsabile != null && x.SINTESI_Responsabile.COD_MATLIBROMAT == item.PersValutato.Matricola &&
                                                                 (x.SINTESI1_SecRiporto == null || x.SINTESI1_SecRiporto.COD_MATLIBROMAT != item.PersResp.Matricola))
                                                     .Where(enabledStates)
                                                     .Select(x => new MboScheda()
                                                     {
                                                         Id = x.ID_SCHEDA,
                                                         IdPersonaResp = x.ID_PERSONA_RESP,
                                                         IdPersonaValutato = x.ID_PERSONA_VALUTATO,
                                                         IdPersonaSecRiporto = x.ID_PERS_RIPORTO,
                                                         IdPersonaConsuntivazione = x.ID_PERSONA_CONSUNTIVAZIONE,
                                                         ImportoTeorico = x.DEC_IMPORTO_TEORICO,
                                                         Approvato = x.IND_APPROVED,
                                                         IsSent = x.IND_SENT,
                                                         IsConsuntivazioneBlocked = x.IND_INIBISCI_CONSUNTIVAZIONE.HasValue ? x.IND_INIBISCI_CONSUNTIVAZIONE.Value : false,
                                                         IsApproved = x.IND_APPROVED,
                                                         //IsChief = x.IND_CHIEF.GetValueOrDefault(),
                                                         StatoCorrente = x.XR_MBO_OPERSTATI.Where(z => !z.VALID_DTA_END.HasValue & z.ID_STATO != 0).Select(w => w.XR_MBO_STATI).OrderByDescending(s => s.XR_WKF_WORKFLOW.FirstOrDefault(y => y.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE).FirstOrDefault(),
                                                         Ordine = x.XR_MBO_OPERSTATI.Where(z => !z.VALID_DTA_END.HasValue & z.ID_STATO != 0).Select(w => w.XR_MBO_STATI).OrderByDescending(s => s.XR_WKF_WORKFLOW.FirstOrDefault(y => y.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE).FirstOrDefault().XR_WKF_WORKFLOW.FirstOrDefault(z => z.ID_TIPOLOGIA == x.ID_TIPOLOGIA),
                                                         DBLivello = x.COD_LIV,
                                                         DBPersResp = x.SINTESI_Responsabile,
                                                         DBPersVal = x.SINTESI_Valutato,
                                                         DbPersRiporto = x.SINTESI1_Riporto,
                                                         DBInziativa = x.XR_MBO_INIZIATIVA
                                                     }).ToList();

                    foreach (var subItem in tmpSubList)
                    {
                        switch (subItem.DBLivello)
                        {
                            case "Top":
                                subItem.CodLivelloDir = LivelloDirigente.Top;
                                break;
                            case "Full":
                                subItem.CodLivelloDir = LivelloDirigente.Full;
                                break;
                            case "Manager":
                                subItem.CodLivelloDir = LivelloDirigente.Manager;
                                break;
                            default:
                                break;
                        }

                        if (subItem.DBPersResp != null)
                            subItem.PersResp = new MboPersona(subItem.DBPersResp);

                        if (subItem.DBPersVal != null)
                            subItem.PersValutato = new MboPersona(subItem.DBPersVal);

                        if (subItem.DbPersRiporto != null)
                            subItem.PersSecRiporto = new MboPersona(subItem.DbPersRiporto);

                        subItem.Iniziativa = new MboIniziativa(subItem.DBInziativa);

                        item.ElencoSchedeSub.Add(subItem);
                    }
                }

                if (loadVal && item.Iniziativa.IdSchedaValutazione.HasValue)
                {
                    var schedaVal = db.XR_VAL_EVALUATION.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now)
                                                            .FirstOrDefault(x => x.ID_PERSONA == item.IdPersonaValutato
                                                                                && x.XR_VAL_EVALUATOR.ID_PERSONA == item.IdPersonaResp
                                                                                && x.XR_VAL_CAMPAIGN_SHEET.ID_OBJ_COLL == item.Iniziativa.Id);

                    if (schedaVal != null)
                    {
                        item.IdSchedaVal = schedaVal.ID_EVALUATION;
                        item.StatoVal = schedaVal.XR_VAL_OPER_STATE.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).FirstOrDefault(x => x.ID_STATE == (int)ValutazioniState.Analizzata);
                    }
                }
            }

            return result;
        }
        public static bool SaveObiettiviPerc(int[] ids, decimal[] values, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();

            bool anyChanges = false;
            for (int i = 0; i < ids.Length; i++)
            {
                var ob = db.XR_MBO_OBIETTIVI.Find(ids[i]);
                if (ob.NMB_PESO_SPEC != values[i])
                {
                    anyChanges = true;
                    ob.NMB_PESO_SPEC = values[i];
                }
            }

            if (anyChanges)
            {
                result = DBHelper.Save(db, CommonManager.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
            }
            else
            {
                result = true;
            }

            return result;
        }
        public static bool ToggleAnnullaObiettivo(int idOb, int idScheda, bool annulla, string nota, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            var ob = db.XR_MBO_OBIETTIVI.Find(idOb);
            if (ob == null)
                errorMsg = "Obiettivo non trovato";
            else
            {
                ob.IND_ANNULLATO = annulla;
                ob.NMB_PERC_COMPLETAMENTO = 0;

                var scheda = db.XR_MBO_SCHEDA.Find(idScheda, 7);
                if (annulla)
                {
                    AggiungiNota(db, scheda, nota, null, null, idOb);
                }
                else
                {
                    var dbNota = scheda.XR_MBO_NOTE.FirstOrDefault(x => x.ID_OBIETTIVO == idOb);
                    if (dbNota != null)
                        db.XR_MBO_NOTE.Remove(dbNota);
                }

                result = DBHelper.Save(db, CommonManager.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
            }

            return result;
        }
        public static bool SaveConsObiettiviPerc(int[] ids, decimal?[] values, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();

            bool anyChanges = false;
            for (int i = 0; i < ids.Length; i++)
            {
                var ob = db.XR_MBO_OBIETTIVI.Find(ids[i]);
                if (ob.NMB_PERC_COMPLETAMENTO != values[i])
                {
                    anyChanges = true;
                    ob.NMB_PERC_COMPLETAMENTO = values[i];
                }
            }

            if (anyChanges)
            {
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
            }
            else
            {
                result = true;
            }

            return result;
        }

        public static bool SaveAssegnazione(int idScheda, bool isRPM, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            var scheda = db.XR_MBO_SCHEDA.Find(idScheda, (int)MboTipologia.Mbo);
            if (scheda == null)
                errorMsg = "Scheda non trovata";
            else
            {
                if (isRPM && scheda.ID_PERS_RIPORTO.HasValue && scheda.ID_PERS_RIPORTO.Value > 0)
                {
                    SalvaStato(db, idScheda, (int)MboState.ObiettiviSottopostiAlDir, false);
                }
                else
                {
                    scheda.IND_SENT = true;
                    SalvaStato(db, idScheda, (int)MboState.ProntiPerInvioRuo, false);
                    SalvaStato(db, idScheda, (int)MboState.ObiettiviSottopostiRuo, false);
                }

                foreach (var all in db.XR_MBO_ALLEGATI.Where(x => x.ID_SCHEDA == scheda.ID_SCHEDA && x.ID_TIPOLOGIA == scheda.ID_TIPOLOGIA && x.IND_TEMP != null && x.IND_TEMP.Value))
                    all.IND_TEMP = false;

                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";

                if (scheda.IND_SENT.GetValueOrDefault())
                {
                    var parametri = GetParametri();
                    if (parametri.AbilitaNotificaAssegnazione)
                        NotificheManager.InserisciNotifica(FormattaStringa(scheda, parametri.TestoNotificaAssegnazione), "AssegnazioneObiettivi", scheda.SINTESI_Valutato.COD_MATLIBROMAT, CommonManager.GetCurrentUserMatricola(), 0);
                }
            }

            return result;
        }

        public static bool SaveConsuntivazione(int idScheda, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            var scheda = db.XR_MBO_SCHEDA.Find(idScheda, 7);
            if (scheda == null)
                errorMsg = "Scheda non trovata";
            else
            {
                //Calcolare riparto
                var obiettivi = scheda.XR_MBO_OBIETTIVI;
                decimal coeffRiparto = 0;
                if (obiettivi.Any(x => x.IND_ANNULLATO.GetValueOrDefault()))
                {
                    decimal totDaRipartire = obiettivi.Where(x => x.IND_ANNULLATO.GetValueOrDefault()).Sum(x => x.NMB_PESO_SPEC);
                    decimal totAttivi = obiettivi.Where(x => !x.IND_ANNULLATO.GetValueOrDefault()).Sum(x => x.NMB_PESO_SPEC);
                    coeffRiparto = totDaRipartire / totAttivi;
                }


                foreach (var ob in obiettivi)
                {
                    if (ob.IND_ANNULLATO.GetValueOrDefault())
                        ob.NMB_PESO_EFFETTIVO = 0;
                    else
                        ob.NMB_PESO_EFFETTIVO = Math.Round(coeffRiparto * ob.NMB_PESO_SPEC + ob.NMB_PESO_SPEC, 2);
                }

                scheda.NMB_RESULT = obiettivi.Where(x => !x.IND_ANNULLATO.GetValueOrDefault()).Sum(x => x.NMB_PESO_EFFETTIVO.Value * x.NMB_PERC_COMPLETAMENTO.Value) / 100;

                SalvaStato(db, idScheda, (int)MboState.Consutivati, false);

                if (scheda.XR_MBO_INIZIATIVA.ID_SCHEDA_VAL.HasValue)
                {
                    var schedaVal = db.XR_VAL_EVALUATION.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now)
                                                            .FirstOrDefault(x => x.ID_PERSONA == scheda.ID_PERSONA_VALUTATO
                                                                                && x.XR_VAL_EVALUATOR.ID_PERSONA == scheda.ID_PERSONA_RESP
                                                                                && x.XR_VAL_CAMPAIGN_SHEET.ID_OBJ_COLL == scheda.ID_INIZIATIVA);

                    if (schedaVal != null)
                        ValutazioniManager.SalvaStato(db, schedaVal.ID_EVALUATION, (int)ValutazioniState.Analizzata);
                }

                result = DBHelper.Save(db, CommonManager.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
            }

            return result;
        }
        public static bool AnnullaConsuntivazione(int idScheda, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            var scheda = db.XR_MBO_SCHEDA.Find(idScheda, 7);
            if (scheda == null)
                errorMsg = "Scheda non trovata";
            else
            {
                InvalidaStatoCorrente(db, idScheda, (int)MboTipologia.Mbo, false);
                if (scheda.XR_MBO_INIZIATIVA.ID_SCHEDA_VAL.HasValue)
                {
                    var schedaVal = db.XR_VAL_EVALUATION.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now)
                                                            .FirstOrDefault(x => x.ID_PERSONA == scheda.ID_PERSONA_VALUTATO
                                                                                && x.XR_VAL_EVALUATOR.ID_PERSONA == scheda.ID_PERSONA_RESP
                                                                                && x.XR_VAL_CAMPAIGN_SHEET.ID_OBJ_COLL == scheda.ID_INIZIATIVA);

                    ValutazioniManager.InvalidaStato(db, schedaVal.ID_EVALUATION, (int)ValutazioniState.Analizzata);
                    ValutazioniManager.InvalidaStato(db, schedaVal.ID_EVALUATION, (int)ValutazioniState.Convalidata);
                }

                ////Calcolare riparto
                //var obiettivi = scheda.XR_MBO_OBIETTIVI;
                //decimal coeffRiparto = 0;
                //if (obiettivi.Any(x => x.IND_ANNULLATO.GetValueOrDefault()))
                //{
                //    decimal totDaRipartire = obiettivi.Where(x => x.IND_ANNULLATO.GetValueOrDefault()).Sum(x => x.NMB_PESO_SPEC);
                //    decimal totAttivi = obiettivi.Where(x => !x.IND_ANNULLATO.GetValueOrDefault()).Sum(x => x.NMB_PESO_SPEC);
                //    coeffRiparto = totDaRipartire / totAttivi;
                //}


                //foreach (var ob in obiettivi)
                //{
                //    if (ob.IND_ANNULLATO.GetValueOrDefault())
                //        ob.NMB_PESO_EFFETTIVO = 0;
                //    else
                //        ob.NMB_PESO_EFFETTIVO = Math.Round(coeffRiparto * ob.NMB_PESO_SPEC + ob.NMB_PESO_SPEC, 2);
                //}

                //scheda.NMB_RESULT = obiettivi.Where(x => !x.IND_ANNULLATO.GetValueOrDefault()).Sum(x => x.NMB_PESO_EFFETTIVO.Value * x.NMB_PERC_COMPLETAMENTO.Value) / 100;

                //SalvaStato(db, idScheda, (int)MboState.Consutivati, false);

                //if (scheda.XR_MBO_INIZIATIVA.ID_SCHEDA_VAL.HasValue)
                //{
                //    var schedaVal = db.XR_VAL_EVALUATION.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now)
                //                                            .FirstOrDefault(x => x.ID_PERSONA == scheda.ID_PERSONA_VALUTATO
                //                                                                && x.XR_VAL_EVALUATOR.ID_PERSONA == scheda.ID_PERSONA_RESP
                //                                                                && x.XR_VAL_CAMPAIGN_SHEET.ID_OBJ_COLL == scheda.ID_INIZIATIVA);

                //    if (schedaVal != null)
                //        ValutazioniManager.SalvaStato(db, schedaVal.ID_EVALUATION, (int)ValutazioniState.Analizzata);
                //}

                result = DBHelper.Save(db, CommonManager.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
            }

            return result;
        }

        public static string FormattaStringa(XR_MBO_SCHEDA scheda, string testo)
        {
            string result = testo;

            result = result.Replace("#INIZIATIVA#", scheda.XR_MBO_INIZIATIVA.NME_NOME);

            return result;
        }

        public static bool SaveValutazioneDirettore(int idScheda, bool parere, string nota, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            var scheda = db.XR_MBO_SCHEDA.Find(idScheda, (int)MboTipologia.Mbo);
            if (scheda == null)
                errorMsg = "Scheda non trovata";
            else
            {
                if (!parere)
                {
                    var currentState = GetSchedaCurrentState(db, scheda.ID_SCHEDA, scheda.ID_TIPOLOGIA);
                    InvalidaStatoCorrente(db, scheda.ID_SCHEDA, scheda.ID_TIPOLOGIA);
                    AggiungiNota(db, scheda, nota, scheda.ID_PERSONA_RESP, currentState.ID_OPERSTATI);
                }
                else
                {
                    scheda.IND_SENT = true;
                    SalvaStato(db, idScheda, (int)MboState.ProntiPerInvioRuo, false);
                    SalvaStato(db, idScheda, (int)MboState.ObiettiviSottopostiRuo, false);
                }

                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";

                if (scheda.IND_SENT.GetValueOrDefault())
                {
                    var parametri = GetParametri();
                    if (parametri.AbilitaNotificaAssegnazione)
                        NotificheManager.InserisciNotifica(FormattaStringa(scheda, parametri.TestoNotificaAssegnazione), "AssegnazioneObiettivi", scheda.SINTESI_Valutato.COD_MATLIBROMAT, CommonManager.GetCurrentUserMatricola(), 0);
                }
            }

            return result;
        }
        public static bool Save_Convalida(int idScheda, bool parere, string nota, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            var scheda = db.XR_MBO_SCHEDA.Find(idScheda, (int)MboTipologia.Mbo);
            if (scheda == null)
                errorMsg = "Scheda non trovata";
            else
            {
                if (!parere)
                {
                    var currentState = GetSchedaCurrentState(db, scheda.ID_SCHEDA, scheda.ID_TIPOLOGIA);
                    scheda.IND_APPROVED = false;
                    InvalidaStatoCorrente(db, scheda.ID_SCHEDA, scheda.ID_TIPOLOGIA, false, (int)MboState.DaCompilare);
                    AggiungiNota(db, scheda, nota, scheda.ID_PERSONA_RESP, currentState.ID_OPERSTATI);

                    //Nel caso venga richiamato per rimuovere la convalida
                    foreach (var item in scheda.XR_MBO_OBIETTIVI.Where(x => !x.VALID_DTA_END.HasValue || x.VALID_DTA_END > DateTime.Now))
                        item.IND_CONSOLIDATO = false;
                }
                else
                {
                    scheda.IND_APPROVED = true;

                    foreach (var item in scheda.XR_MBO_OBIETTIVI.Where(x => !x.VALID_DTA_END.HasValue || x.VALID_DTA_END > DateTime.Now))
                        item.IND_CONSOLIDATO = true;

                    SalvaStato(db, idScheda, (int)MboState.Convalidati, false);
                }

                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";

            }

            return result;
        }
        private static void AggiungiNota(IncentiviEntities db, XR_MBO_SCHEDA scheda, string nota, int? destinatario = null, int? idOper = null, int? idObiettivo = null)
        {
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tmsTimestamp);
            XR_MBO_NOTE newNota = new XR_MBO_NOTE()
            {
                ID_NOTA = db.XR_MBO_NOTE.GeneraPrimaryKey(),
                ID_SCHEDA = scheda.ID_SCHEDA,
                ID_TIPOLOGIA = scheda.ID_TIPOLOGIA,
                ID_PERSONA = CommonHelper.GetCurrentIdPersona(),
                NOT_NOTA = nota,

                VISIBILITA = destinatario.HasValue ? "pers" : "*",
                ID_PERSONA_DEST = destinatario,

                ID_OPERSTATO = idOper,

                ID_OBIETTIVO = idObiettivo,

                DTA_INSERIMENTO = tmsTimestamp,
                COD_USER = codUser,
                COD_TERMID = codTermid,
                TMS_TIMESTAMP = tmsTimestamp
            };
            db.XR_MBO_NOTE.Add(newNota);
        }

        public static bool SaveObiettivo(MboObiettivo model, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            XR_MBO_OBIETTIVI obiettivo = null;
            if (model.Id == 0)
            {
                obiettivo = new XR_MBO_OBIETTIVI();
                obiettivo.ID_OBIETTIVO = db.XR_MBO_OBIETTIVI.GeneraPrimaryKey(9);
                obiettivo.ID_SCHEDA = model.IdScheda;
                obiettivo.ID_TIPOLOGIA = model.IdTipologia;
            }
            else
                obiettivo = db.XR_MBO_OBIETTIVI.Find(model.Id);

            obiettivo.COD_TIPO = model.Tipo;

            obiettivo.DES_DESCRIZIONE = model.Descrizione;
            obiettivo.NOT_RISULTATO_ATTESO = model.RisultatoAtteso;
            obiettivo.NMB_PESO_SPEC = model.PesoSpecifico;
            obiettivo.IND_STRAT_DIR = model.IsStrategicoDirezione;
            obiettivo.NOT_STRAT_DIR = model.StrategicoDirezione;

            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tmsTimestamp);
            obiettivo.COD_USER = codUser;
            obiettivo.COD_TERMID = codTermid;
            obiettivo.TMS_TIMESTAMP = tmsTimestamp;

            if (model.Id == 0)
            {
                obiettivo.VALID_DTA_INI = tmsTimestamp;
                db.XR_MBO_OBIETTIVI.Add(obiettivo);
            }

            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            return result;
        }
        public static bool EliminaObiettivo(int idObiettivo, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            XR_MBO_OBIETTIVI ob = db.XR_MBO_OBIETTIVI.Find(idObiettivo);
            if (ob == null)
            {
                errorMsg = "Obiettivo non trovato";
            }
            else
            {
                db.XR_MBO_OBIETTIVI.Remove(ob);
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
            }

            return result;
        }

        public static bool SaveIniziativa(MboIniziativa model, List<MboScheda> schede, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            XR_MBO_INIZIATIVA iniziativa = null;
            if (model.Id == 0)
            {
                iniziativa = new XR_MBO_INIZIATIVA();
                iniziativa.ID_INIZIATIVA = db.XR_MBO_INIZIATIVA.GeneraPrimaryKey(9);
            }
            else
                iniziativa = db.XR_MBO_INIZIATIVA.FirstOrDefault(x => x.ID_INIZIATIVA == model.Id);

            iniziativa.NME_NOME = model.Nome;
            iniziativa.DES_DESCRIZIONE = model.Descrizione;
            iniziativa.DTA_INI_ASSEGNAZIONE = model.DataInizioAssegnazione;
            iniziativa.DTA_END_ASSEGNAZIONE = model.DataFineAssegnazione;
            iniziativa.DTA_INI_VALUT = model.DataInizioValutazione;
            iniziativa.DTA_END_VALUT = model.DataFineValutazione;

            iniziativa.DEC_IMPORTO_MAX_TOP = model.ImportoTop;
            iniziativa.DEC_IMPORTO_MAX_FULL = model.ImportoFull;
            iniziativa.DEC_IMPORTO_MAX_MANAGER = model.ImportoManager;

            iniziativa.ID_SCHEDA_VAL = model.IdSchedaValutazione;

            iniziativa.DEC_COEFF_DECURTAZIONE = model.CoeffDecurtazione;
            iniziativa.DEC_COEFF_GESTIONALE = model.CoeffGestionale;

            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tmsTimestamp);
            iniziativa.COD_USER = codUser;
            iniziativa.COD_TERMID = codTermid;
            iniziativa.TMS_TIMESTAMP = tmsTimestamp;

            if (model.Id == 0)
            {
                iniziativa.VALID_DTA_INI = tmsTimestamp;
                db.XR_MBO_INIZIATIVA.Add(iniziativa);
            }

            var sintOper = db.SINTESI1.Find(CommonHelper.GetCurrentIdPersona());

            if (model.Id == 0)
            {
                //Per velocizzare il salvataggio arrivano solo le informazioni delle schede modificate manualmente
                //Quindi richiamo il metodo per le schede master
                var elencoSchede = GetSchedeMaster(0, model.ImportoTop, model.ImportoFull, model.ImportoManager);
                foreach (var item in elencoSchede)
                {
                    XR_MBO_SCHEDA dbSch = new XR_MBO_SCHEDA();
                    dbSch.ID_INIZIATIVA = iniziativa.ID_INIZIATIVA;
                    dbSch.ID_SCHEDA = db.XR_MBO_SCHEDA.GeneraComposedPrimaryKey(9, (int)MboTipologia.Mbo);
                    dbSch.ID_TIPOLOGIA = (int)MboTipologia.Mbo;
                    dbSch.COD_LIV = item.CodLivelloDir.ToString();
                    dbSch.ID_PERSONA_RESP = item.IdPersonaResp;
                    dbSch.ID_PERSONA_VALUTATO = item.IdPersonaValutato;
                    dbSch.ID_PERS_RIPORTO = item.IdPersonaSecRiporto;
                    //dbSch.ID_PERSONA_CONSUNTIVAZIONE = item.IdPersonaConsuntivazione;
                    dbSch.DEC_IMPORTO_TEORICO = item.ImportoTeorico;
                    dbSch.VALID_DTA_INI = tmsTimestamp;
                    dbSch.COD_USER = codUser;
                    dbSch.COD_TERMID = codTermid;
                    dbSch.TMS_TIMESTAMP = tmsTimestamp;

                    MboScheda modSch = schede.FirstOrDefault(x => x.IdPersonaValutato == item.IdPersonaValutato);
                    if (modSch != null && modSch.ManChanged)
                    {
                        dbSch.ID_PERSONA_RESP = modSch.IdPersonaResp;
                        if (modSch.ImportoTeorico != dbSch.DEC_IMPORTO_TEORICO)
                            dbSch.DEC_IMPORTO_TEORICO_ORIG = dbSch.DEC_IMPORTO_TEORICO;
                        dbSch.DEC_IMPORTO_TEORICO = modSch.ImportoTeorico;
                    }

                    db.XR_MBO_SCHEDA.Add(dbSch);

                    SalvaStato(db, dbSch.ID_SCHEDA, (int)MboState.DaAvviare, false, null, false, sintOper);
                    //L'inserimento dell'importo non è necessario per la compilazione della scheda
                    //Perciò questa deve poter essere subito compilabile
                    //if (modSch != null && modSch.ManChanged)
                    SalvaStato(db, dbSch.ID_SCHEDA, (int)MboState.DaCompilare, false, null, false, sintOper);
                }
            }
            else
            {
                foreach (var item in schede.Where(x => !x.IsExtra))
                {
                    var dbSch = db.XR_MBO_SCHEDA.Find(item.Id, (int)MboTipologia.Mbo);
                    if (dbSch != null)
                    {
                        if (dbSch.DEC_IMPORTO_TEORICO != item.ImportoTeorico)
                            dbSch.DEC_IMPORTO_TEORICO_ORIG = dbSch.DEC_IMPORTO_TEORICO;
                        if (dbSch.ID_PERSONA_RESP != item.IdPersonaRespOrig)
                            dbSch.ID_PERSONA_RESP_ORIG = dbSch.ID_PERSONA_RESP;

                        dbSch.DEC_IMPORTO_TEORICO = item.ImportoTeorico;
                        dbSch.VALID_DTA_INI = tmsTimestamp;
                        dbSch.COD_USER = codUser;
                        dbSch.COD_TERMID = codTermid;
                        dbSch.TMS_TIMESTAMP = tmsTimestamp;

                        if (item.DeleteOper)
                        {
                            //InvalidaStatoCorrente(db, dbSch.ID_SCHEDA, (int)MboTipologia.Mbo, false, (int)MboState.DaCompilare);
                            dbSch.VALID_DTA_END = DateTime.Now;
                            dbSch.IND_MAN_DEL = true;
                            foreach (var ob in dbSch.XR_MBO_OBIETTIVI)
                            {
                                ob.VALID_DTA_END = DateTime.Now;
                            }
                        }
                        else
                        {
                            //L'importo può essere impostato in qualsiasi momento e lo stato da compilare è già stato creato in fase di creazione 
                            //SalvaStato(db, dbSch.ID_SCHEDA, (int)MboState.DaCompilare, false, sintOper: sintOper);
                        }
                    }
                }
            }

            IncarichiContainer incarichiCont = null;
            if (schede.Any(x => x.IsExtra))
                incarichiCont = ValutazioniManager.GetIncarichiContainer2();

            string[] incarichiADePresidente = new string[] { "000010", "000008" };

            foreach (var item in schede.Where(x => x.IsExtra))
            {
                XR_MBO_SCHEDA dbSch = new XR_MBO_SCHEDA();
                dbSch.ID_INIZIATIVA = iniziativa.ID_INIZIATIVA;
                dbSch.ID_SCHEDA = db.XR_MBO_SCHEDA.GeneraComposedPrimaryKey(9, (int)MboTipologia.Mbo);
                dbSch.ID_TIPOLOGIA = (int)MboTipologia.Mbo;
                dbSch.COD_LIV = item.CodLivelloDir.ToString();
                dbSch.ID_PERSONA_RESP = item.IdPersonaResp;
                dbSch.ID_PERSONA_VALUTATO = item.IdPersonaValutato;

                if (dbSch.ID_PERSONA_RESP != 0)
                {
                    string incarichi = "";
                    var persResp = db.SINTESI1.Find(dbSch.ID_PERSONA_RESP);

                    var incarico = incarichiCont.ListIncarichi.FirstOrDefault(x => x.matricola == persResp.COD_MATLIBROMAT);
                    if (incarico.cod_incarico == "000003")
                    {
                        string secRiporto = GetMioResponsabile(persResp, incarichiCont, ref incarichi);
                        if (!String.IsNullOrWhiteSpace(secRiporto) && persResp.COD_MATLIBROMAT != secRiporto && !incarichiADePresidente.Contains(incarichi))
                        {
                            var persRiporto = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == secRiporto);
                            if (persRiporto != null)
                                dbSch.ID_PERS_RIPORTO = persRiporto.ID_PERSONA;
                        }
                    }
                }

                dbSch.DEC_IMPORTO_TEORICO = item.ImportoTeorico;
                dbSch.VALID_DTA_INI = tmsTimestamp;
                dbSch.COD_USER = codUser;
                dbSch.COD_TERMID = codTermid;
                dbSch.TMS_TIMESTAMP = tmsTimestamp;
                dbSch.IND_MAN_ADD = true;

                db.XR_MBO_SCHEDA.Add(dbSch);

                SalvaStato(db, dbSch.ID_SCHEDA, (int)MboState.DaAvviare, false, null, false, sintOper);
                //L'inserimento dell'importo non è necessario per la compilazione della scheda
                //Perciò questa deve poter essere subito compilabile
                //if (item.ImportoTeorico > 0)
                SalvaStato(db, dbSch.ID_SCHEDA, (int)MboState.DaCompilare, false, null, false, sintOper);
            }

            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            return result;
        }

        public static bool SaveTuning(MboScheda model, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            bool hasDifference = false;
            var scheda = db.XR_MBO_SCHEDA.Find(model.Id, (int)MboTipologia.Mbo);
            if (scheda.DEC_IMPORTO_TEORICO != model.ImportoTeorico)
            {
                hasDifference = true;
                scheda.DEC_IMPORTO_TEORICO_ORIG = scheda.DEC_IMPORTO_TEORICO;
                scheda.DEC_IMPORTO_TEORICO = model.ImportoTeorico;
            }

            var oper = GetPraticaOperStato(scheda);
            if (oper.ID_STATO < (int)MboState.Convalidati)
            {
                if (scheda.ID_PERSONA_RESP != model.IdPersonaResp)
                {
                    hasDifference = true;

                    scheda.ID_PERSONA_RESP_ORIG = scheda.ID_PERSONA_RESP;
                    scheda.ID_PERSONA_RESP = model.IdPersonaResp;

                    //Verifica se c'è una scheda di valutazione associata
                    if (scheda.XR_MBO_INIZIATIVA.ID_SCHEDA_VAL != null)
                    {
                        //Cerca la scheda di valutazione con il precedente valutatore
                        var schedaVal = db.XR_VAL_EVALUATION.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now)
                                                            .FirstOrDefault(x => x.ID_PERSONA == scheda.ID_PERSONA_VALUTATO
                                                                                && x.XR_VAL_EVALUATOR.ID_PERSONA == scheda.ID_PERSONA_RESP_ORIG
                                                                                && x.XR_VAL_CAMPAIGN_SHEET.ID_OBJ_COLL == scheda.ID_INIZIATIVA);

                        if (schedaVal != null)
                        {
                            //Cerco se esiste già il nuovo valutatore
                            var newEval = db.XR_VAL_EVALUATOR.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now)
                                                             .FirstOrDefault(x => x.ID_PERSONA == scheda.ID_PERSONA_RESP && x.XR_VAL_CAMPAIGN_SHEET.ID_OBJ_COLL == scheda.ID_INIZIATIVA);

                            if (newEval == null)
                            {
                                newEval = new XR_VAL_EVALUATOR
                                {
                                    ID_EVALUATOR = db.XR_VAL_EVALUATOR.GeneraPrimaryKey(),
                                    ID_CAMPAIGN = schedaVal.XR_VAL_CAMPAIGN_SHEET.ID_CAMPAIGN,
                                    ID_CAMPAIGN_SHEET = schedaVal.ID_CAMPAIGN_SHEET,
                                    VALID_DTA_INI = DateTime.Today,
                                    VALID_DTA_END = null,
                                    ID_PERSONA = scheda.ID_PERSONA_RESP,
                                    COD_USER = CommonHelper.GetCurrentUserPMatricola(),
                                    COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                                    TMS_TIMESTAMP = DateTime.Now
                                };
                                db.XR_VAL_EVALUATOR.Add(newEval);
                            }
                            schedaVal.ID_EVALUATOR = newEval.ID_EVALUATOR;
                        }
                    }
                }
            }
            if (scheda.ID_PERS_RIPORTO != model.IdPersonaSecRiporto)
            {
                hasDifference = true;

                scheda.ID_PERS_RIPORTO_ORIG = scheda.ID_PERS_RIPORTO;
                scheda.ID_PERS_RIPORTO = model.IdPersonaSecRiporto;
            }
            if (scheda.ID_PERSONA_CONSUNTIVAZIONE != model.IdPersonaConsuntivazione)
            {
                hasDifference = true;

                scheda.ID_PERSONA_CONSUNTIVAZIONE_ORIG = scheda.ID_PERSONA_CONSUNTIVAZIONE;
                scheda.ID_PERSONA_CONSUNTIVAZIONE = model.IdPersonaConsuntivazione;
            }

            hasDifference = hasDifference || scheda.IND_CHIEF != model.IsChief;
            scheda.IND_CHIEF = model.IsChief;

            hasDifference = hasDifference || scheda.IND_INIBISCI_CONSUNTIVAZIONE != model.IsConsuntivazioneBlocked;
            scheda.IND_INIBISCI_CONSUNTIVAZIONE = model.IsConsuntivazioneBlocked;

            if (hasDifference)
            {
                CezanneHelper.GetCampiFirma(out var campiFirma);
                scheda.COD_USER = campiFirma.CodUser;
                scheda.COD_TERMID = campiFirma.CodTermid;
                scheda.TMS_TIMESTAMP = campiFirma.Timestamp;
            }

            //SalvaStato(db, scheda.ID_SCHEDA, (int)MboState.DaCompilare, false);

            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            return result;
        }

        public static bool GetSecondoRiporto(IncentiviEntities db, int idPersona, out int idPersRiporto)
        {
            idPersRiporto = 0;

            if (db == null)
                db = new IncentiviEntities();

            IncarichiContainer incarichiCont = ValutazioniManager.GetIncarichiContainer2();
            SINTESI1 persResp = db.SINTESI1.Find(idPersona);
            string resp = persResp.COD_MATLIBROMAT;
            string incarico = incarichiCont.ListIncarichi.Where(x => x.matricola == resp).Select(x => x.cod_incarico).FirstOrDefault();
            //Verifico il 2°riporto già in creazione
            //Lo verifica solo se il 1° riporto non è un direttore
            if (persResp != null && incarico != "000003")
            {
                incarico = "";
                string secRiporto = GetMioResponsabile(persResp, incarichiCont, ref incarico);
                if (!String.IsNullOrWhiteSpace(secRiporto) && resp != secRiporto)
                {
                    var persRiporto = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == secRiporto);
                    if (persRiporto != null)
                        idPersRiporto = persRiporto.ID_PERSONA;
                }
            }

            return idPersRiporto != 0;
        }

        public static MboObiettivo GetObiettivo(int idScheda, string tipo, int idOb)
        {
            MboObiettivo obiettivo = null;
            var db = new IncentiviEntities();

            if (idOb == 0)
            {
                var scheda = db.XR_MBO_SCHEDA.FirstOrDefault(x => x.ID_SCHEDA == idScheda);
                obiettivo = new MboObiettivo()
                {
                    Tipo = tipo,
                    IdScheda = idScheda,
                    IdTipologia = scheda.ID_TIPOLOGIA
                };
            }
            else
            {
                var item = db.XR_MBO_OBIETTIVI.FirstOrDefault(x => x.ID_OBIETTIVO == idOb);
                obiettivo = new MboObiettivo(item);
                obiettivo.IdScheda = idScheda;
            }

            return obiettivo;
        }

        public static MboScheda GetScheda(int idScheda, bool loadIncarichi = true)
        {
            var db = new IncentiviEntities();
            var item = db.XR_MBO_SCHEDA.Find(idScheda, (int)MboTipologia.Mbo);


            var scheda = new MboScheda(item);
            var sintResp = db.SINTESI1.Find(scheda.IdPersonaResp);
            if (sintResp != null)
                scheda.PersResp = new MboPersona(sintResp);

            var sintVal = db.SINTESI1.Find(scheda.IdPersonaValutato);
            if (sintVal != null)
                scheda.PersValutato = new MboPersona(sintVal);

            var sintSec = db.SINTESI1.Find(scheda.IdPersonaSecRiporto);
            if (sintSec != null)
                scheda.PersSecRiporto = new MboPersona(sintSec);

            var sintCons = db.SINTESI1.Find(scheda.IdPersonaConsuntivazione);
            if (sintCons != null)
                scheda.PersConsuntivazione = new MboPersona(sintCons);

            scheda.Anagrafica = AnagraficaManager.GetAnagrafica("", scheda.IdPersonaValutato, checkAbil: false);

            scheda.Iniziativa = new MboIniziativa(item.XR_MBO_INIZIATIVA);

            scheda.OperStato = GetPraticaOperStato(item);

            foreach (var ob in item.XR_MBO_OBIETTIVI.Where(x => x.VALID_DTA_END == null))
            {
                scheda.Obiettivi.Add(new MboObiettivo(ob)
                {

                });
            }

            scheda.Note = new List<MboNota>();
            if (item.XR_MBO_NOTE != null)
            {
                scheda.Note.AddRange(item.XR_MBO_NOTE
                                         .Where(x => x.VISIBILITA == null || x.ID_PERSONA_DEST == CommonHelper.GetCurrentIdPersona() || x.ID_PERSONA == CommonHelper.GetCurrentIdPersona())
                                         //.Where(x => x.ID_OPERSTATO==null || x.ID_OPERSTATO==scheda.OperStato.ID_OPERSTATI)
                                         .ToList().Select(x => new MboNota(x)));

                foreach (var nota in scheda.Note)
                {
                    nota.Autore = new MboPersona(db.SINTESI1.Find(nota.IdPersonaAutore));
                    if (nota.IdPersonaDest.HasValue)
                        nota.Destinatario = new MboPersona(db.SINTESI1.Find(nota.IdPersonaDest));
                }
            }

            //if (loadIncarichi)
            //{
            //    scheda.Incarichi = new List<MboIncaricoNode>();
            //    scheda.Incarichi.AddRange(GetMieiIncarichi(sintVal.COD_MATLIBROMAT, null));
            //}

            if (item.XR_MBO_INIZIATIVA.ID_SCHEDA_VAL.HasValue)
            {
                var schedaVal = db.XR_VAL_EVALUATION.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now)
                                                        .FirstOrDefault(x => x.ID_PERSONA == item.ID_PERSONA_VALUTATO
                                                                            && x.XR_VAL_EVALUATOR.ID_PERSONA == item.ID_PERSONA_RESP
                                                                            && x.XR_VAL_CAMPAIGN_SHEET.ID_OBJ_COLL == item.ID_INIZIATIVA);

                if (schedaVal != null)
                {
                    scheda.IdSchedaVal = schedaVal.ID_EVALUATION;
                    scheda.StatoVal = schedaVal.XR_VAL_OPER_STATE.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).FirstOrDefault(x => x.ID_STATE == (int)ValutazioniState.Analizzata);
                }
            }

            scheda.Allegati = new List<XR_MBO_ALLEGATI>();
            scheda.Allegati.AddRange(InternalGetAllegati(idScheda, (int)MboTipologia.Mbo, db, false));

            scheda.ImportoEffettivo = null;
            scheda.IsConsuntivata = false;
            scheda.IsConvalidata = (scheda.OperStato.ID_STATO >= (int) MboState.Convalidati );

            if (scheda.OperStato.ID_STATO >= (int)MboState.Consutivati)
            {
                scheda.IsConsuntivata = true;
                decimal decurtazione = scheda.Iniziativa.CoeffDecurtazione.GetValueOrDefault();
                decimal gestionale = scheda.Iniziativa.CoeffGestionale.GetValueOrDefault();
                decimal result = scheda.Completamento.GetValueOrDefault();
                scheda.ImportoEffettivo = scheda.ImportoTeorico * (1 - decurtazione) * ((1 - gestionale) * (result / 100) + gestionale);
            }

            return scheda;
        }
        public static List<MboIncaricoNode> GetMieiIncarichi(string matricola, IncarichiContainer incarichiCont)
        {
            if (incarichiCont == null)
                incarichiCont = ValutazioniManager.GetIncarichiContainer();

            List<MboIncaricoNode> nodes = new List<MboIncaricoNode>();

            var matrIncarichi = incarichiCont.ListIncarichi.Where(x => x.matricola == matricola);
            foreach (var incarico in matrIncarichi)
            {
                var node = GetMieiIncarichiSub(incarichiCont, incarico.id_sezione, matricola);
                if (node != null)
                    nodes.Add(node);
            }

            return nodes;
        }
        private static MboIncaricoNode GetMieiIncarichiSub(IncarichiContainer incarichiCont, int idSez, string matricola)
        {
            MboIncaricoNode node = null;
            List<string> uorg = new List<string>();

            var sez = incarichiCont.ListSezioni.FirstOrDefault(x => x.id == idSez);
            if (sez != null)
            {
                node = new MboIncaricoNode();
                node.CodSezione = sez.codice_visibile;
                node.DesSezione = sez.descrizione_lunga;

                var db = new IncentiviEntities();
                foreach (var item in db.SINTESI1.Where(x => x.COD_MATLIBROMAT != matricola && x.COD_UNITAORG.Trim() == sez.codice_visibile.Trim()).OrderBy(x => x.COD_MATLIBROMAT))
                    node.Persone.Add(new MboPersona(item));

                var subordinate = incarichiCont.NodiAlbero.Where(x => x.subordinato_a == idSez);
                foreach (var subordinata in subordinate)
                {
                    var subInc = incarichiCont.ListIncarichi.Where(x => x.id_sezione == subordinata.id);
                    if (subInc.Count() == 0)
                        node.Children.Add(GetMieiIncarichiSub(incarichiCont, subordinata.id, matricola));
                    else
                    {
                        var elencoMatr = subInc.Select(x => x.matricola);
                        foreach (var item in db.SINTESI1.Where(x => x.COD_MATLIBROMAT != matricola && elencoMatr.Contains(x.COD_MATLIBROMAT)).OrderBy(x => x.COD_MATLIBROMAT))
                            node.Persone.Add(new MboPersona(item));
                    }
                }
            }

            return node;
        }

        public static List<XR_MBO_ALLEGATI> InternalGetAllegati(int idScheda, int idTipologia, IncentiviEntities db, bool addNew, params int[] stati)
        {
            var allegati = db.XR_MBO_ALLEGATI.Where(x => x.ID_SCHEDA == idScheda && x.ID_TIPOLOGIA == idTipologia)
                                                    .Select(x => new
                                                    {
                                                        x.ID_ALLEGATO,
                                                        x.COD_TITLE,
                                                        x.CONTENT_TYPE,
                                                        x.DES_ALLEGATO,
                                                        x.NMB_SIZE,
                                                        x.NME_FILENAME,
                                                        x.TMS_TIMESTAMP
                                                    })
                                                    .ToList()
                                                    .Select(y => new XR_MBO_ALLEGATI()
                                                    {
                                                        ID_ALLEGATO = y.ID_ALLEGATO,
                                                        COD_TITLE = y.COD_TITLE,
                                                        CONTENT_TYPE = y.CONTENT_TYPE,
                                                        DES_ALLEGATO = y.DES_ALLEGATO,
                                                        NMB_SIZE = y.NMB_SIZE,
                                                        NME_FILENAME = y.NME_FILENAME,
                                                        TMS_TIMESTAMP = y.TMS_TIMESTAMP
                                                    });

            var Allegati = new List<XR_MBO_ALLEGATI>();
            Allegati.AddRange(allegati);
            if (addNew)
            {
                Allegati.Add(new XR_MBO_ALLEGATI()
                {
                    ID_ALLEGATO = 0,
                    COD_TITLE = "",
                });
            }
            return Allegati;
        }
        
        public static List<MboScheda> GetSchedeMaster(int idIni, decimal importoTop, decimal importoFull, decimal importoManager, bool forceReload = false)
        {
            List<MboScheda> result = new List<MboScheda>();

            var db = new IncentiviEntities();
            XR_MBO_INIZIATIVA dbScheda = null;

            if (idIni != 0)
            {
                dbScheda = db.XR_MBO_INIZIATIVA.Find(idIni);

                result = db.XR_MBO_SCHEDA.Where(x => x.ID_INIZIATIVA == idIni && (x.IND_MAN_DEL == null || x.IND_MAN_DEL == false)).Select(x => new MboScheda()
                {
                    Id = x.ID_SCHEDA,
                    IdPersonaResp = x.ID_PERSONA_RESP,
                    IdPersonaValutato = x.ID_PERSONA_VALUTATO,
                    IdPersonaSecRiporto = x.ID_PERS_RIPORTO,
                    IdPersonaConsuntivazione = x.ID_PERSONA_CONSUNTIVAZIONE,
                    ImportoTeorico = x.DEC_IMPORTO_TEORICO,

                    IdPersonaRespOrig = x.ID_PERSONA_RESP_ORIG,
                    ImportoTeoricoOrig = x.DEC_IMPORTO_TEORICO_ORIG,
                    CreatoManualmente = x.IND_MAN_ADD,

                    StatoCorrente = x.XR_MBO_OPERSTATI.Where(z => !z.VALID_DTA_END.HasValue & z.ID_STATO != 0).Select(w => w.XR_MBO_STATI)
                                        .OrderByDescending(s => s.XR_WKF_WORKFLOW.FirstOrDefault(y => y.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE).FirstOrDefault(),

                    DBLivello = x.COD_LIV,
                    DBPersResp = x.SINTESI_Responsabile,
                    DBPersVal = x.SINTESI_Valutato,
                    //DbPersRiporto = x.SINTESI1_Riporto,
                    //DBInziativa = x.XR_MBO_INIZIATIVA
                }).ToList();
            }

            if (idIni != 0)
            {
                bool useOldData = dbScheda.DEC_IMPORTO_MAX_TOP == importoTop && dbScheda.DEC_IMPORTO_MAX_FULL == importoFull && dbScheda.DEC_IMPORTO_MAX_MANAGER == importoManager;

                foreach (var scheda in result)
                {
                    switch (scheda.DBLivello)
                    {
                        case "Top":
                            scheda.CodLivelloDir = LivelloDirigente.Top;
                            break;
                        case "Full":
                            scheda.CodLivelloDir = LivelloDirigente.Full;
                            break;
                        case "Manager":
                            scheda.CodLivelloDir = LivelloDirigente.Manager;
                            break;
                        default:
                            break;
                    }

                    if (scheda.DBPersVal != null)
                        scheda.PersValutato = new MboPersona(scheda.DBPersVal);

                    if (scheda.DBPersResp != null)
                        scheda.PersResp = new MboPersona(scheda.DBPersResp);

                    //Controlla se è stato creato manualmente e/o modificato
                    if (!useOldData && (scheda.IsManualModified() || forceReload))
                    {
                        switch (scheda.CodLivelloDir)
                        {
                            case LivelloDirigente.Top:
                                scheda.ImportoTeorico = importoTop;
                                break;
                            case LivelloDirigente.Full:
                                scheda.ImportoTeorico = importoFull;
                                break;
                            case LivelloDirigente.Manager:
                                scheda.ImportoTeorico = importoManager;
                                break;
                            default:
                                break;
                        }
                    }

                    scheda.HasOper = scheda.StatoCorrente.ID_STATO != (int)MboState.DaAvviare;
                }
            }
            else
            {
                //Consociate (C2,N2,B2)
                //Distacchi (88, 92, 96 e 97)
                //Amministratore Delegato (50)
                string[] serviziEsclusi = new string[] { "C2", "N2", "B2", "50", "88", "92", "96", "97" };
                var elencoSint = db.SINTESI1.Where(x => x.DTA_FINE_CR > DateTime.Now && x.COD_QUALIFICA.StartsWith("A01") && !serviziEsclusi.Contains(x.COD_SERVIZIO))
                                .Select(x => new { x.ID_PERSONA, x.DES_COGNOMEPERS, x.DES_NOMEPERS, x.COD_MATLIBROMAT, x.COD_SERVIZIO, x.DES_SERVIZIO, x.COD_UNITAORG, x.DES_DENOMUNITAORG })
                                .ToList()
                                .Select(x => new SINTESI1()
                                {
                                    ID_PERSONA = x.ID_PERSONA,
                                    DES_COGNOMEPERS = x.DES_COGNOMEPERS,
                                    DES_NOMEPERS = x.DES_NOMEPERS,
                                    COD_MATLIBROMAT = x.COD_MATLIBROMAT,
                                    COD_SERVIZIO = x.COD_SERVIZIO,
                                    DES_SERVIZIO = x.DES_SERVIZIO,
                                    COD_UNITAORG = x.COD_UNITAORG,
                                    DES_DENOMUNITAORG = x.DES_DENOMUNITAORG
                                })
                                .ToList();

                var elencoMatrDirigenti = elencoSint.Select(x => x.COD_MATLIBROMAT);


                IncarichiContainer incarichiCont = ValutazioniManager.GetIncarichiContainer2();
                var matrResp = incarichiCont.ListIncarichi.Select(x => x.matricola).Distinct().ToList();
                var sintResp = db.SINTESI1.Where(x => matrResp.Contains(x.COD_MATLIBROMAT))
                                .Select(x => new { x.ID_PERSONA, x.DES_COGNOMEPERS, x.DES_NOMEPERS, x.COD_MATLIBROMAT, x.COD_SERVIZIO, x.DES_SERVIZIO, x.COD_UNITAORG, x.DES_DENOMUNITAORG })
                                .ToList()
                                .Select(x => new SINTESI1()
                                {
                                    ID_PERSONA = x.ID_PERSONA,
                                    DES_COGNOMEPERS = x.DES_COGNOMEPERS,
                                    DES_NOMEPERS = x.DES_NOMEPERS,
                                    COD_MATLIBROMAT = x.COD_MATLIBROMAT,
                                    COD_SERVIZIO = x.COD_SERVIZIO,
                                    DES_SERVIZIO = x.DES_SERVIZIO,
                                    COD_UNITAORG = x.COD_UNITAORG,
                                    DES_DENOMUNITAORG = x.DES_DENOMUNITAORG
                                })
                                .ToList();

                var queryRal = myRaiCommonModel.Gestionale.HRDW.HRDWData.GetQueryRAL("'" + String.Join("','", elencoMatrDirigenti) + "'", DateTime.Now.Year);
                List<myRaiCommonModel.Gestionale.HRDW.HRDWRal> dataRal = null;

                if (CommonHelper.IsProduzione())
                    dataRal = db.Database.SqlQuery<myRaiCommonModel.Gestionale.HRDW.HRDWRal>(queryRal).ToList();
                else
                    dataRal = new List<myRaiCommonModel.Gestionale.HRDW.HRDWRal>();

                //var queryTipologia = GetQueryTipologiaDir(elencoMatrDirigenti);
                //var tipologie = db.Database.SqlQuery<HRDWTipologiaDir>(queryTipologia);

                foreach (var item in elencoSint)
                {
                    //ral > 200K
                    var ral = dataRal.FirstOrDefault(x => x.Matricola == item.COD_MATLIBROMAT);
                    if (ral != null && ral.Ral_media.HasValue && ral.Ral_media > 2000000)
                    {
                        continue;
                    }

                    MboScheda scheda = new MboScheda();
                    scheda.Id = 0;
                    scheda.IdPersonaValutato = item.ID_PERSONA;
                    scheda.PersValutato = new MboPersona(item);

                    string incarico = "";
                    string resp = GetMioResponsabile(item, incarichiCont, ref incarico);
                    SINTESI1 persResp = null;
                    if (!String.IsNullOrWhiteSpace(resp))
                    {
                        persResp = sintResp.FirstOrDefault(x => x.COD_MATLIBROMAT == resp);
                        if (persResp != null && persResp.ID_PERSONA != scheda.IdPersonaValutato)
                        {
                            scheda.Interim = incarico == "000002";
                            scheda.Incarico = incarico;
                            scheda.IdPersonaResp = persResp.ID_PERSONA;
                            scheda.PersResp = new MboPersona(persResp);
                        }
                        else if (persResp != null && persResp.ID_PERSONA == scheda.IdPersonaResp)
                        {
                            scheda.IdPersonaResp = 0;
                        }
                    }

                    //Verifico il 2°riporto già in creazione
                    //Lo verifica solo se il 1° riporto non è un direttore
                    if (persResp != null && incarico != "000003")
                    {
                        incarico = "";
                        string secRiporto = GetMioResponsabile(persResp, incarichiCont, ref incarico);
                        if (!String.IsNullOrWhiteSpace(secRiporto) && resp != secRiporto)
                        {
                            var persRiporto = sintResp.FirstOrDefault(x => x.COD_MATLIBROMAT == secRiporto);
                            if (persRiporto != null)
                            {
                                scheda.IdPersonaSecRiporto = persRiporto.ID_PERSONA;
                                scheda.PersSecRiporto = new MboPersona(persRiporto);
                            }
                        }
                    }


                    //var recTip = tipologie.FirstOrDefault(x => x.Matricola == item.COD_MATLIBROMAT);
                    //switch (recTip.Tipologia)
                    //{
                    //    case "Top":
                    //    case "Ex Top":
                    //    case "Top ad personam":
                    //        scheda.CodLivelloDir = LivelloDirigente.Top;
                    //        scheda.ImportoTeorico = importoTop;
                    //        break;
                    //    case "Full":
                    //        scheda.CodLivelloDir = LivelloDirigente.Full;
                    //        scheda.ImportoTeorico = importoFull;
                    //        break;
                    //    case "Manager":
                    //        scheda.CodLivelloDir = LivelloDirigente.Manager;
                    //        scheda.ImportoTeorico = importoManager;
                    //        break;
                    //    default:
                    //        scheda.CodLivelloDir = LivelloDirigente.NonIndividuato;
                    //        scheda.ImportoTeorico = 0;
                    //        break;
                    //}
                    scheda.CodLivelloDir = LivelloDirigente.NonIndividuato;
                    scheda.ImportoTeorico = 0;

                    result.Add(scheda);
                }
            }

            return result;
        }

        private static string GetMioResponsabile(SINTESI1 sint, IncarichiContainer cont, ref string incarico)
        {
            string resp = "";

            if (cont == null)
                cont = ValutazioniManager.GetIncarichiContainer2();

            if (!String.IsNullOrWhiteSpace(sint.COD_UNITAORG))
            {
                var sez = cont.ListSezioni.FirstOrDefault(x => x.codice_visibile.Trim() == sint.COD_UNITAORG.Trim());
                if (sez != null)
                    resp = GetRespFromSezione(sez.id, cont, sint.COD_MATLIBROMAT, ref incarico);
            }

            return resp;
        }
        public static List<MboPersona> GetIncarichiResponsabili(int idPers, IncarichiContainer cont = null, int? idPersAdd = null)
        {
            List<MboPersona> elenco = new List<MboPersona>();

            if (cont == null)
                cont = ValutazioniManager.GetIncarichiContainer2(true, false, false);

            var inc = cont.ListIncarichi.Where(x => x.flag_resp == "1");
            var elencoMatr = inc.Select(x => x.matricola).Distinct().ToList();

            var db = new IncentiviEntities();

            elenco.AddRange(db.SINTESI1.Where(x => x.ID_PERSONA != idPers && ((idPersAdd.HasValue && x.ID_PERSONA == idPersAdd.Value) || elencoMatr.Contains(x.COD_MATLIBROMAT)))
                                        .Select(x => new { x.ID_PERSONA, x.DES_COGNOMEPERS, x.DES_NOMEPERS, x.COD_MATLIBROMAT }).ToList()
                                        .Select(x => new SINTESI1()
                                        {
                                            ID_PERSONA = x.ID_PERSONA,
                                            DES_COGNOMEPERS = x.DES_COGNOMEPERS,
                                            DES_NOMEPERS = x.DES_NOMEPERS,
                                            COD_MATLIBROMAT = x.COD_MATLIBROMAT,
                                        }).OrderBy(x => x.Nominativo()).Select(x => new MboPersona(x)));

            return elenco;
        }

        private static string GetRespFromSezione(int idSezione, IncarichiContainer cont, string matricola, ref string incarico)
        {
            string resp = "";

            var inc = cont.ListIncarichi.FirstOrDefault(x => x.id_sezione == idSezione && x.flag_resp == "1");
            if (inc != null && matricola != inc.matricola)
            {
                resp = inc.matricola;
                incarico = inc.cod_incarico;
            }
            else if (idSezione != 1)
            {
                var nodoPadre = cont.NodiAlbero.FirstOrDefault(x => x.id == idSezione);
                if (nodoPadre != null)
                    resp = GetRespFromSezione(nodoPadre.subordinato_a, cont, matricola, ref incarico);
            }

            return resp;
        }

        public static List<MboPersona> GetPersoneResp()
        {
            List<MboPersona> result = new List<MboPersona>();
            var db = new IncentiviEntities();
            var list = db.XR_MBO_SCHEDA.Select(x => x.ID_PERSONA_RESP).Distinct().ToList();
            foreach (var item in db.SINTESI1.Where(x => list.Contains(x.ID_PERSONA)).OrderBy(x => x.DES_COGNOMEPERS).ThenBy(x => x.DES_NOMEPERS))
                result.Add(new MboPersona(item));
            return result;
        }
        public static List<MboPersona> GetPersoneSecRiporto()
        {
            List<MboPersona> result = new List<MboPersona>();
            var db = new IncentiviEntities();
            var list = db.XR_MBO_SCHEDA.Select(x => x.ID_PERS_RIPORTO).Distinct().ToList();
            foreach (var item in db.SINTESI1.Where(x => list.Contains(x.ID_PERSONA)).OrderBy(x => x.DES_COGNOMEPERS).ThenBy(x => x.DES_NOMEPERS))
                result.Add(new MboPersona(item));
            return result;
        }
        public static List<MboPersona> GetPersoneConsuntivatori()
        {
            List<MboPersona> result = new List<MboPersona>();
            var db = new IncentiviEntities();
            var list = db.XR_MBO_SCHEDA.Select(x => x.ID_PERSONA_CONSUNTIVAZIONE.HasValue ? x.ID_PERSONA_CONSUNTIVAZIONE.Value : x.ID_PERSONA_RESP).Distinct().ToList();
            foreach (var item in db.SINTESI1.Where(x => list.Contains(x.ID_PERSONA)).OrderBy(x => x.DES_COGNOMEPERS).ThenBy(x => x.DES_NOMEPERS))
                result.Add(new MboPersona(item));
            return result;
        }

        public static List<SelectListItem> GetDirezioniSch()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = new IncentiviEntities();
            var list = db.XR_MBO_SCHEDA.Select(x => new { Value = x.SINTESI_Valutato.COD_SERVIZIO, Text = x.SINTESI_Valutato.DES_SERVIZIO }).Distinct().OrderBy(x => x.Value).ToList();
            foreach (var item in list)
                result.Add(new SelectListItem() { Value = item.Value, Text = item.Text.TitleCase() });
            return result;
        }

        public static bool InvioARuo(out string message)
        {
            bool result = true;
            message = "";

            var db = new IncentiviEntities();
            var isCurrentState = IsCurrentState((int)MboState.ObiettiviSottopostiRuo);
            var tmpElenco = db.XR_MBO_SCHEDA
                .Where(x => x.XR_MBO_INIZIATIVA.VALID_DTA_END == null || x.XR_MBO_INIZIATIVA.VALID_DTA_END > DateTime.Now)
                .Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)
                .Where(isCurrentState)
                .Where(x => x.IND_SENT == null);

            if (tmpElenco.Any())
            {
                foreach (var item in tmpElenco)
                {
                    item.IND_SENT = true;
                }

                db.SaveChanges();
            }


            //var inizOpen = db.XR_MBO_INIZIATIVA.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).ToList();
            //foreach (var iniz in inizOpen)
            //{
            //    var notSottoposteARuo = NotIsAnyState((int)MboState.ObiettiviSottopostiRuo);

            //    var list = db.XR_MBO_SCHEDA
            //                .Where(x => x.ID_INIZIATIVA == iniz.ID_INIZIATIVA)
            //                .Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)
            //                .Where(x => x.ID_PERSONA_RESP != 0)
            //                .Where(notSottoposteARuo)
            //                .Select(x => new
            //                {
            //                    IdScheda = x.ID_SCHEDA,
            //                    Matricola = x.SINTESI_Responsabile.COD_MATLIBROMAT,
            //                    IdTipologia = x.ID_TIPOLOGIA,
            //                    Stato = x.XR_MBO_OPERSTATI
            //                                .Where(z => !z.VALID_DTA_END.HasValue & z.ID_STATO != 0).Select(w => w.XR_MBO_STATI)
            //                                .OrderByDescending(s => s.XR_WKF_WORKFLOW.FirstOrDefault(y => y.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE).FirstOrDefault(),
            //                    Direzione = x.SINTESI_Valutato.COD_SERVIZIO
            //                }).ToList();

            //    foreach (var dir in list.GroupBy(x => x.Direzione))
            //    {
            //        //if (dir.All(x => x.Stato.ID_STATO == (int)MboState.ProntiPerInvioRuo))
            //        {
            //            message = message + "\r\n" + dir.Key;
            //            foreach (var sch in dir)
            //                SalvaStato(db, sch.IdScheda, (int)MboState.ObiettiviSottopostiRuo, false, sch.Matricola);

            //            try
            //            {
            //                db.SaveChanges();
            //            }
            //            catch (Exception ex)
            //            {
            //                result = false;
            //                message += " - " + ex.Message;
            //            }
            //        }
            //    }
            //}

            return result;
        }


        #region UtilityStato
        private static bool InvalidaStatoCorrente(IncentiviEntities db, int idScheda, int idTipologia, bool saveData = false, int targetState = 0)
        {
            bool result = false;

            if (db == null)
            {
                saveData = true;
                db = new IncentiviEntities();
            }

            if (targetState == 0)
            {
                var currentState = GetSchedaCurrentState(db, idScheda, (int)MboTipologia.Mbo);
                if (currentState != null)
                {
                    currentState.VALID_DTA_END = DateTime.Now;
                    CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tmsTimestamp);
                    currentState.COD_USER = codUser;
                    currentState.COD_TERMID = codTermid;
                    currentState.TMS_TIMESTAMP = tmsTimestamp;
                }

                result = !saveData || DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            }
            else
            {
                var stati = (from op in db.XR_MBO_OPERSTATI
                             join wk in db.XR_WKF_WORKFLOW on new { op.ID_TIPOLOGIA, op.ID_STATO } equals new { wk.ID_TIPOLOGIA, wk.ID_STATO }
                             where op.ID_GESTIONE == idScheda && op.ID_TIPOLOGIA == idTipologia && op.ID_STATO != 0 && op.VALID_DTA_INI < DateTime.Now && op.VALID_DTA_END == null
                             orderby wk.ORDINE descending
                             select op);

                foreach (var item in stati)
                {
                    if (item.ID_STATO == targetState)
                        break;

                    item.VALID_DTA_END = DateTime.Now;
                    CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tmsTimestamp);
                    item.COD_USER = codUser;
                    item.COD_TERMID = codTermid;
                    item.TMS_TIMESTAMP = tmsTimestamp;
                }

                result = !saveData || DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            }

            return result;
        }
        private static bool SalvaStato(IncentiviEntities db, int idGestione, int stato, bool saveData, string matr = "", bool checkCurrentState = true, SINTESI1 sintOper = null)
        {
            bool result = false;

            if (db == null)
            {
                saveData = true;
                db = new IncentiviEntities();
            }

            if (checkCurrentState)
            {
                var currentState = GetSchedaCurrentState(db, idGestione, (int)MboTipologia.Mbo);
                if (currentState != null && currentState.ID_STATO == stato)
                {
                    currentState.VALID_DTA_END = DateTime.Now;
                }
            }

            if (String.IsNullOrWhiteSpace(matr))
                matr = CommonHelper.GetCurrentUserMatricola();

            if (sintOper == null)
            {
                sintOper = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matr).ToList().Select(x => new SINTESI1()
                {
                    ID_PERSONA = x.ID_PERSONA,
                    DES_COGNOMEPERS = x.DES_COGNOMEPERS,
                    DES_NOMEPERS = x.DES_NOMEPERS
                }).FirstOrDefault();
            }

            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tmsTime);

            XR_MBO_OPERSTATI operStato = new XR_MBO_OPERSTATI()
            {
                ID_GESTIONE = idGestione,
                ID_TIPOLOGIA = (int)MboTipologia.Mbo,
                COD_TIPO_PRATICA = "MBO",

                ID_STATO = stato,
                ID_PERSONA = sintOper.ID_PERSONA,
                NOMINATIVO = sintOper.Nominativo(),
                DTA_OPERAZIONE = tmsTime,
                VALID_DTA_INI = tmsTime,
                COD_USER = codUser,
                COD_TERMID = codTermid,
                TMS_TIMESTAMP = tmsTime
            };

            db.XR_MBO_OPERSTATI.Add(operStato);

            result = !saveData || DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

            return result;
        }
        private static XR_MBO_OPERSTATI GetPraticaOperStato(XR_MBO_SCHEDA pratica)
        {
            return pratica.XR_MBO_OPERSTATI.Where(y => y.ID_STATO != 0 && !y.VALID_DTA_END.HasValue)
                    .OrderByDescending(z => z.XR_MBO_STATI.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == pratica.ID_TIPOLOGIA).ORDINE)
                    .FirstOrDefault();
        }
        private static XR_MBO_OPERSTATI GetSchedaCurrentState(IncentiviEntities db, int idScheda, int idTipologia)
        {
            return (from op in db.XR_MBO_OPERSTATI
                    join wk in db.XR_WKF_WORKFLOW on new { op.ID_TIPOLOGIA, op.ID_STATO } equals new { wk.ID_TIPOLOGIA, wk.ID_STATO }
                    where op.ID_GESTIONE == idScheda && op.ID_TIPOLOGIA == idTipologia && op.ID_STATO != 0 && op.VALID_DTA_INI < DateTime.Now && op.VALID_DTA_END == null
                    orderby wk.ORDINE descending
                    select op).FirstOrDefault();
        }
        public static Expression<Func<XR_MBO_SCHEDA, bool>> IsCurrentState(params int[] stato)
        {
            return x => stato.Contains(x.XR_MBO_OPERSTATI.Where(y => y.ID_STATO != 0 && (!y.VALID_DTA_END.HasValue || y.VALID_DTA_END > DateTime.Now))
                            .Select(w => w.XR_MBO_STATI)
                            .OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                            .Select(b => b.ID_STATO)
                            .FirstOrDefault());
        }
        public static Expression<Func<XR_MBO_SCHEDA, bool>> NotIsAnyState(params int[] stato)
        {
            return x => !x.XR_MBO_OPERSTATI.Where(y => y.ID_STATO != 0 && (!y.VALID_DTA_END.HasValue || y.VALID_DTA_END > DateTime.Now)).Select(w => w.ID_STATO).Any(y => stato.Contains(y));
        }
        #endregion

        public static List<XR_MBO_STATI> GetStati()
        {
            List<XR_MBO_STATI> stati = new List<XR_MBO_STATI>();
            var db = new IncentiviEntities();
            stati.AddRange(db.XR_MBO_STATI);
            return stati;
        }

        public static bool HasSchede(out List<MboScheda> schede)
        {
            bool result = false;
            schede = null;

            if (!CommonManager.IsProduzione())
                return result;

            var dbTal = new IncentiviEntities();
            var recAbil = dbTal.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "MboVisualizzaSchedeAssegnate");
            schede = null;
            if (recAbil == null) return result;

            if (recAbil.COD_VALUE1 == "TRUE" || (recAbil.COD_VALUE1 == "LIMITED" && recAbil.COD_VALUE2.Contains(CommonManager.GetCurrentRealUsername())))
            {
                schede = new List<MboScheda>();
                string matricola = CommonManager.GetCurrentUserMatricola();

                var db = new IncentiviEntities();
                schede = db.XR_MBO_SCHEDA.Where(x => x.XR_MBO_INIZIATIVA.VALID_DTA_END == null && (DateTime.Today <= x.XR_MBO_INIZIATIVA.DTA_END_ASSEGNAZIONE || DateTime.Today <= x.XR_MBO_INIZIATIVA.DTA_END_VALUT))
                                         .Where(x => x.VALID_DTA_END == null && x.IND_SENT.HasValue && x.IND_SENT.Value && x.SINTESI_Valutato.COD_MATLIBROMAT == matricola)
                                            .Select(x => new MboScheda()
                                            {
                                                Id = x.ID_SCHEDA,
                                                IdPersonaResp = x.ID_PERSONA_RESP,
                                                IdPersonaValutato = x.ID_PERSONA_VALUTATO,
                                                IdPersonaSecRiporto = x.ID_PERS_RIPORTO,
                                                IdPersonaConsuntivazione = x.ID_PERSONA_CONSUNTIVAZIONE,
                                                ImportoTeorico = x.DEC_IMPORTO_TEORICO,
                                                Approvato = x.IND_APPROVED,

                                                StatoCorrente = x.XR_MBO_OPERSTATI.Where(z => !z.VALID_DTA_END.HasValue & z.ID_STATO != 0).Select(w => w.XR_MBO_STATI)
                                                                .OrderByDescending(s => s.XR_WKF_WORKFLOW.FirstOrDefault(y => y.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE).FirstOrDefault(),
                                                Ordine = x.XR_MBO_OPERSTATI.Where(z => !z.VALID_DTA_END.HasValue & z.ID_STATO != 0).Select(w => w.XR_MBO_STATI)
                                                            .OrderByDescending(s => s.XR_WKF_WORKFLOW.FirstOrDefault(y => y.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE).FirstOrDefault().XR_WKF_WORKFLOW.FirstOrDefault(z => z.ID_TIPOLOGIA == x.ID_TIPOLOGIA),
                                                DBInziativa = x.XR_MBO_INIZIATIVA
                                            }).ToList();

                foreach (var item in schede)
                {
                    item.Iniziativa = new MboIniziativa(item.DBInziativa);
                }

                result = schede != null && schede.Any();
            }

            return result;
        }

        private static string GetQueryTipologiaDir(IEnumerable<string> elencoMatr)
        {
            return " select " +
                   " t0.matricola_dp as Matricola, " +
                   " case " +
                   "    when t0.pernottamento = '1' then 'Manager' " +
                   "    when t0.pernottamento = '2' then 'Full' " +
                   "    when t0.pernottamento = '3' then 'Top' " +
                   "    when t0.pernottamento = '4' then 'Ex Top' " +
                   "    when t0.pernottamento = '5' then 'Top ad personam' " +
                   "    else t0.pernottamento end as Tipologia " +
                   " from[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                   " where t0.matricola_dp in ('" + String.Join("','", elencoMatr) + "')";
        }

        public static bool CreaPdfScheda(int idScheda, out string title, out MemoryStream ms)
        {
            var db = new IncentiviEntities();
            MboScheda scheda = GetScheda(idScheda);
            //var scheda = db.XR_MBO_SCHEDA.Find(idScheda, (int)MboTipologia.Mbo);
            //Recupero compilatore e consuntivatore

            List<XR_PRV_TEMPLATE> templates = db.XR_PRV_TEMPLATE.Where(x => x.VALID_DTA_END == null).ToList();

            bool result = false;
            title = scheda.Iniziativa.Nome + " - " + scheda.PersValutato.Nominativo + ".pdf";
            ms = null;

            string logoIcon = PoliticheRetributiveManager.GetImageDoc(db.XR_PRV_TEMPLATE.FirstOrDefault(x => x.NOME == "LOGO").TEMPLATE);
            string infoSoc = PoliticheRetributiveManager.GetImageDoc(db.XR_PRV_TEMPLATE.FirstOrDefault(x => x.NOME == "INTESTAZIONE").TEMPLATE);
            var baseTemplate = db.XR_PRV_TEMPLATE.FirstOrDefault(x => x.NOME == "BASE");

            var pdf = new Mbo.PdfPrinter(logoIcon, infoSoc, title);
            pdf.Apri(baseTemplate.TEMPLATE);

            string dirStruct = CezanneHelper.GetDes(scheda.PersValutato.CodServizio, scheda.PersValutato.DesServizio).TitleCase() + " - " + CezanneHelper.GetDes(scheda.PersValutato.CodStruttura, scheda.PersValutato.DesStruttura).TitleCase();
            string nominativo = scheda.PersValutato.Nominativo;
            pdf.AggiungiTableHeader(scheda.Iniziativa.Nome, dirStruct, nominativo, scheda.PersResp, scheda.PersConsuntivazione);
            pdf.WriteLine("\n");

            if (!scheda.Completamento.HasValue)
            {
                var MBO_SCHEDA = db.XR_MBO_SCHEDA.Find(idScheda, 7);
                //Calcolare riparto
                var obiettivi = MBO_SCHEDA.XR_MBO_OBIETTIVI;
                decimal coeffRiparto = 0;
                if (obiettivi.Any(x => x.IND_ANNULLATO.GetValueOrDefault()))
                {
                    decimal totDaRipartire = obiettivi.Where(x => x.IND_ANNULLATO.GetValueOrDefault()).Sum(x => x.NMB_PESO_SPEC);
                    decimal totAttivi = obiettivi.Where(x => !x.IND_ANNULLATO.GetValueOrDefault()).Sum(x => x.NMB_PESO_SPEC);
                    coeffRiparto = totDaRipartire / totAttivi;
                }

                foreach (var ob in obiettivi)
                {
                    if (ob.IND_ANNULLATO.GetValueOrDefault())
                        ob.NMB_PESO_EFFETTIVO = 0;
                    else
                        ob.NMB_PESO_EFFETTIVO = Math.Round(coeffRiparto * ob.NMB_PESO_SPEC + ob.NMB_PESO_SPEC, 2);
                }

                MBO_SCHEDA.NMB_RESULT = obiettivi.Where(x => !x.IND_ANNULLATO.GetValueOrDefault())
                        .Sum(x => x.NMB_PESO_EFFETTIVO.GetValueOrDefault() * x.NMB_PERC_COMPLETAMENTO.GetValueOrDefault()) / 100;

                scheda.Completamento = MBO_SCHEDA.NMB_RESULT;
                scheda.Obiettivi = obiettivi.Select(x => new MboObiettivo(x)).ToList();
            }

            var listObQuant = scheda.Obiettivi.Where(x => x.Tipo == "quantitativo");
            var listObQual = scheda.Obiettivi.Where(x => x.Tipo == "qualitativo");
            if (listObQuant != null && listObQuant.Any())
            {
                pdf.AggiungiTableObiettivi("Obiettivo quantitativo", listObQuant, scheda.IsConsuntivata || scheda.IsConvalidata
                    , listObQual != null && listObQual.Any());
                //pdf.WriteLine("\n");
            }
            if (listObQual != null && listObQual.Any())
            {
                pdf.AggiungiTableObiettivi("Obiettivo qualitativo", listObQual, scheda.IsConsuntivata || scheda.IsConvalidata, false);
                //pdf.WriteLine("\n");
            }

            if (scheda.IdSchedaVal.HasValue && (scheda.IsConsuntivata || scheda.IsConvalidata))
            {
                pdf.WriteLine("\n");
                pdf.AggiungiTableResult(scheda);

                pdf.AggiungiPagina();
                var schedaVal = ValutazioniManager.GetValutazione(scheda.IdSchedaVal.Value, true);
                pdf.AggiungiValutazione(schedaVal);
            }

            //pdf.WriteLine("\n");
            //if (scheda.SINTESI_Responsabile != null)
            //    pdf.AggiungiTableResult(scheda.SINTESI_Responsabile.Nominativo());
            //else
            //    pdf.AggiungiTableResult("\n");

            pdf.Chiudi(out ms);
            result = true;

            return result;
        }

        public static bool CreaReportIniziativa(int idIniziativa, string tipo, out string title, out MemoryStream ms, out string contentType)
        {
            bool result = false;
            title = null;
            ms = null;
            contentType = null;

            switch (tipo)
            {
                case "ValResp":
                    {
                        var iniziativa = GetSchede(new MboRicerca { HasFilter = true, IdIniziativa = idIniziativa });
                        var wb = new XLWorkbook();
                        var ws = wb.AddWorksheet("Elenco schede");
                        ws.Cell(1, 1).SetValue("Matricola");
                        ws.Cell(1, 2).SetValue("Nominativo");
                        ws.Cell(1, 3).SetValue("MatricolaResponsabile");
                        ws.Cell(1, 4).SetValue("NominativoResponsabile");
                        ws.Cell(1, 5).SetValue("MatricolaSecondoRiporto");
                        ws.Cell(1, 6).SetValue("NominativoSecondoRiporto");
                        ws.Cell(1, 7).SetValue("MatricolaConsuntivatore");
                        ws.Cell(1, 8).SetValue("NominativoConsuntivatore");
                        ws.Cell(1, 9).SetValue("Note");
                        int row = 1;
                        foreach (var ini in iniziativa.GroupBy(x => x.Iniziativa.Id))
                        {
                            foreach (var item in ini.OrderBy(x => x.PersValutato.Nominativo))
                            {
                                row++;
                                ws.Cell(row, 1).SetValue(item.PersValutato.Matricola);
                                ws.Cell(row, 2).SetValue(item.PersValutato.Nominativo);
                                ws.Cell(row, 3).SetValue(item.PersResp != null ? item.PersResp.Matricola : "-");
                                ws.Cell(row, 4).SetValue(item.PersResp != null ? item.PersResp.Nominativo : "Non assegnato");
                                ws.Cell(row, 5).SetValue(item.PersSecRiporto != null ? item.PersSecRiporto.Matricola : "-");
                                ws.Cell(row, 6).SetValue(item.PersSecRiporto != null ? item.PersSecRiporto.Nominativo : "-");

                                var persConsuntivazione = item.PersConsuntivazione ?? item.PersResp;
                                ws.Cell(row, 7).SetValue(persConsuntivazione != null ? persConsuntivazione.Matricola : "-");
                                ws.Cell(row, 8).SetValue(persConsuntivazione != null ? persConsuntivazione.Nominativo : "-");
                                ws.Cell(row, 9).SetValue(item.IsConsuntivazioneBlocked ? "NO CONSUNTIVAZIONE" : "");
                            }
                        }
                        var table = ws.Range(1, 1, row, 9).CreateTable("TabellaScheda");
                        ws.Columns().AdjustToContents();

                        var wsPivot = wb.AddWorksheet("Pivot responsabili");
                        var pivot = wsPivot.PivotTables.Add("PivotResponsabili", wsPivot.Cell(1, 1), table.AsRange());
                        pivot.RowLabels.Add("NominativoResponsabile").SortType = XLPivotSortType.Ascending;
                        pivot.RowLabels.Add("Nominativo");

                        wsPivot.Column(1).Width = 50;
                        wsPivot.Column(2).Width = 50;

                        title = String.Format("{0} - Responsabili.xlsx", iniziativa.FirstOrDefault().Iniziativa.Nome);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        ms = new MemoryStream();
                        wb.SaveAs(ms);
                        ms.Position = 0;
                        result = true;
                    }
                    break;
                case "Consuntivazione":
                    {
                        var iniziativa = GetSchede(new MboRicerca { HasFilter = true, IdIniziativa = idIniziativa, Stato = (int)MboState.Consutivati }, loadVal: true);
                        var wb = new XLWorkbook();
                        var ws = wb.AddWorksheet("Elenco schede");
                        ws.Cell(1, 1).SetValue("Matricola");
                        ws.Cell(1, 2).SetValue("Nominativo");
                        ws.Cell(1, 3).SetValue("Responsabile");
                        ws.Cell(1, 4).SetValue("SecondoRiporto");
                        ws.Cell(1, 5).SetValue("Consuntivatore");
                        ws.Cell(1, 6).SetValue("Risultato complessivo");
                        ws.Cell(1, 7).SetValue("Media competenze");
                        ws.Cell(1, 8).SetValue("Giudizio sintetico sulle competenze professionali");
                        int row = 1;
                        foreach (var ini in iniziativa.GroupBy(x => x.Iniziativa.Id))
                        {
                            foreach (var item in ini.OrderBy(x => x.PersValutato.Nominativo))
                            {
                                row++;
                                ws.Cell(row, 1).SetValue(item.PersValutato.Matricola);
                                ws.Cell(row, 2).SetValue(item.PersValutato.Nominativo);
                                ws.Cell(row, 3).SetValue(item.PersResp != null ? item.PersResp.Nominativo : "Non assegnato");
                                ws.Cell(row, 4).SetValue(item.PersSecRiporto != null ? item.PersSecRiporto.Nominativo : "-");

                                var persConsuntivazione = item.PersConsuntivazione ?? item.PersResp;
                                ws.Cell(row, 5).SetValue(persConsuntivazione != null ? persConsuntivazione.Nominativo : "-");

                                ws.Cell(row, 6).SetValue(item.Completamento);

                                var schedaVal = ValutazioniManager.GetValutazione(item.IdSchedaVal.Value, true);
                                //media
                                //weight
                                //VALUE_INT
                                decimal answer = 0;
                                decimal weight = 0;
                                foreach (var qst in schedaVal.Scheda.XR_VAL_EVAL_SHEET_QST.Where(x => x.XR_VAL_QUESTION.XR_VAL_QUESTION_DISPLAY.NAME == "Radio button"))
                                {
                                    var rating = schedaVal.Rating.FirstOrDefault(x => x.ID_QUESTION == qst.ID_QUESTION);

                                    if (rating != null || !qst.IND_OPTIONAL.GetValueOrDefault())
                                    {
                                        weight += qst.WEIGHT;
                                        answer += (rating != null ? rating.VALUE_INT.GetValueOrDefault() : 0) * qst.WEIGHT;
                                    }
                                }

                                decimal average = 0;
                                if (answer > 0 && weight > 0)
                                    average = answer / weight;

                                ws.Cell(row, 7).SetValue(average);
                                //giudizio sintetico
                                var qstGS = schedaVal.Scheda.XR_VAL_EVAL_SHEET_QST.FirstOrDefault(x => x.XR_VAL_QUESTION.NAME == "Giudizio sintetico sulle competenze professionali");
                                if (qstGS != null)
                                {
                                    var rating = schedaVal.Rating.FirstOrDefault(x => x.ID_QUESTION == qstGS.ID_QUESTION);
                                    if (rating != null)
                                    {
                                        var val = rating.VALUE_INT.GetValueOrDefault();
                                        var des = qstGS.XR_VAL_QUESTION.XR_VAL_QUESTION_ANSWER.FirstOrDefault(x => x.XR_VAL_ANSWER.VALUE_INT == val);
                                        if (des != null)
                                        {
                                            ws.Cell(row, 8).SetValue(des.XR_VAL_ANSWER.DESCRIPTION);
                                        }
                                    }
                                }
                            }
                        }
                        ws.Column(6).Style.NumberFormat.Format = " 0.0";
                        ws.Column(7).Style.NumberFormat.Format = " 0.0";
                        ws.Range(1, 1, row, 8).CreateTable();

                        ws.Columns().AdjustToContents();
                        title = String.Format("{0} - Consuntivazione.xlsx", iniziativa.FirstOrDefault().Iniziativa.Nome);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        ms = new MemoryStream();
                        wb.SaveAs(ms);
                        ms.Position = 0;
                        result = true;
                    }
                    break;
                default:
                    break;
            }

            return result;
        }

        public static bool InviaReminderCompilazione(int idScheda, int idIniziativa, int idPersonaResp, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            List<XR_MBO_SCHEDA> elencoSchede = new List<XR_MBO_SCHEDA>();
            if (idScheda > 0)
                elencoSchede.Add(db.XR_MBO_SCHEDA.Find(idScheda, (int)MboTipologia.Mbo));
            else
            {
                int[] stato = { (int)MboState.DaCompilare };

                var tmp = db.XR_MBO_SCHEDA
                            .Where(x => x.ID_INIZIATIVA == idIniziativa)
                            .Where(x => (!x.IND_MAN_DEL.HasValue || !x.IND_MAN_DEL.Value) && (x.VALID_DTA_END == null || x.VALID_DTA_END.Value > DateTime.Now))
                            .Where(x => x.ID_PERSONA_RESP > 0)
                            .Where(x => stato.Contains(x.XR_MBO_OPERSTATI.Where(y => y.ID_STATO != 0 && (!y.VALID_DTA_END.HasValue || y.VALID_DTA_END > DateTime.Now))
                                            .OrderByDescending(z => z.XR_MBO_STATI.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE).FirstOrDefault().ID_STATO));

                if (idPersonaResp > 0)
                    tmp = tmp.Where(x => x.ID_PERSONA_RESP == idPersonaResp);

                elencoSchede.AddRange(tmp);
            }

            foreach (var grResp in elencoSchede.GroupBy(x => x.ID_PERSONA_RESP))
            {
                var sintResp = grResp.First().SINTESI_Responsabile;
                string destinatario = CommonHelper.GetEmailPerMatricola(sintResp.COD_MATLIBROMAT);

                string bloccoRich = "<ul>";
                foreach (var item in grResp.OrderBy(x => x.SINTESI_Valutato.Nominativo()))
                {
                    var sintVal = item.SINTESI_Valutato;
                    string divVal = "<div style=\"margin-bottom:5px;\">";
                    divVal += "<b>" + sintVal.Nominativo() + "</b>";
                    divVal += "<br/><small>" + CezanneHelper.GetDes(sintVal.COD_UNITAORG, sintVal.DES_DENOMUNITAORG) + "</small>";
                    divVal += "</div>";
                    bloccoRich += "<li>" + divVal + "</li>";
                }
                bloccoRich += "</ul>";

                string oggetto = "MBO - Schede in compilazione";

                string testo = "<p>Gentile " + sintResp.Nominativo() + ",<br/><br/>si ricorda di compilare ";

                if (grResp.Count() == 1)
                    testo += "la seguente scheda";
                else
                    testo += "le seguenti schede";

                testo += " entro il " + grResp.First().XR_MBO_INIZIATIVA.DTA_END_ASSEGNAZIONE.ToString("dd/MM/yyyy") + ":";

                testo += bloccoRich;

                testo += "<br/><br/>";

                GestoreMail mail = new myRaiCommonTasks.GestoreMail();
                var response = mail.InvioMail(testo, oggetto, destinatario, null, "raiplace.selfservice@rai.it", null, null);
                if (response != null && response.Errore != null)
                {
                    result = false;
                    errorMsg = response.Errore;
                }
                else
                    result = true;
            }

            return result;
        }

        public static bool InviaReminder(string codTipo, int[] idIniziativa, int[] idPersonaResp, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var param = GetParametri();
            MboMail mail = param != null && param.Mail != null && param.Mail.Any() ? mail = param.Mail.FirstOrDefault(x => x.CodTipo == codTipo) : null;
            if (mail == null)
            {
                result = false;
                errorMsg = "Testo mail non trovato";
                return result;
            }

            GestoreMail gestMail = new myRaiCommonTasks.GestoreMail();

            int mailToSend = idPersonaResp.Length;
            int mailSent = 0;

            List<string> nomiNotSent = new List<string>();

            var db = new IncentiviEntities();
            for (int i = 0; i < idPersonaResp.Length; i++)
            {
                SINTESI1 sint = db.SINTESI1.Find(idPersonaResp[i]);

                string testo = mail.Testo;
                string oggetto = mail.Oggetto;
                string destinatario = CommonHelper.GetEmailPerMatricola(sint.COD_MATLIBROMAT);
                string cc = mail.CC;
                string ccn = mail.CCN;

                var response = gestMail.InvioMail(testo, oggetto, destinatario, cc, "raiplace.selfservice@rai.it", null, ccn);
                if (response != null && response.Errore != null)
                    nomiNotSent.Add(sint.Nominativo());
                else
                {
                    CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);

                    mailSent++;
                    XR_MBO_MAIL_LOG log = new XR_MBO_MAIL_LOG()
                    {
                        ID_INIZIATIVA = idIniziativa[i],
                        ID_PERSONA = sint.ID_PERSONA,
                        DTA_INVIO = tms,
                        COD_TIPO = codTipo,
                        MAIL_OGGETTO = oggetto,
                        MAIL_TESTO = testo,
                        COD_USER = codUser,
                        COD_TERMID = codTermid,
                        TMS_TIMESTAMP = tms
                    };
                    db.XR_MBO_MAIL_LOG.Add(log);
                    DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "MBO Reminder: " + codTipo);
                }
            }

            if (mailSent == mailToSend)
                result = true;
            else
            {
                result = false;
                errorMsg = "Non è stato possibile inviare il reminder ai seguenti nominativi:<ul>";
                foreach (var item in nomiNotSent)
                    errorMsg += "<li>" + item + "</li>";
                errorMsg += "</ul>";
            }

            return result;
        }

        public static bool AnyIncomplete(bool getSchede, out int numSchede, out MboReminder reminder)
        {
            numSchede = 0;
            reminder = null;

            int[] stato = { (int)MboState.DaCompilare, (int)MboState.ObiettiviSottopostiAlDir };

            var db = new IncentiviEntities();
            var tmp = db.XR_MBO_SCHEDA
                        .Where(x => (x.XR_MBO_INIZIATIVA.VALID_DTA_END == null || x.XR_MBO_INIZIATIVA.VALID_DTA_END > DateTime.Now))
                        .Where(x => (!x.IND_MAN_DEL.HasValue || !x.IND_MAN_DEL.Value) && (x.VALID_DTA_END == null || x.VALID_DTA_END.Value > DateTime.Now))
                        .Where(x => x.ID_PERSONA_RESP > 0)
                        .Where(x => stato.Contains(x.XR_MBO_OPERSTATI.Where(y => y.ID_STATO != 0 && (!y.VALID_DTA_END.HasValue || y.VALID_DTA_END > DateTime.Now))
                                        .OrderByDescending(z => z.XR_MBO_STATI.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE).FirstOrDefault().ID_STATO));

            numSchede = tmp.Count();

            if (getSchede)
            {
                reminder = new MboReminder();
                reminder.CodTipo = "ReminderCompilazione";
                reminder.Schede = new List<MboScheda>();
                var rawList = tmp.Select(x => new MboScheda()
                {
                    Id = x.ID_SCHEDA,
                    IdPersonaResp = x.ID_PERSONA_RESP,
                    IdPersonaValutato = x.ID_PERSONA_VALUTATO,
                    IdPersonaSecRiporto = x.ID_PERS_RIPORTO,
                    IdPersonaConsuntivazione = x.ID_PERSONA_CONSUNTIVAZIONE,

                    StatoCorrente = x.XR_MBO_OPERSTATI.Where(z => !z.VALID_DTA_END.HasValue & z.ID_STATO != 0).Select(w => w.XR_MBO_STATI)
                        .OrderByDescending(s => s.XR_WKF_WORKFLOW.FirstOrDefault(y => y.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE).FirstOrDefault(),

                    DBLivello = x.COD_LIV,
                    DBPersResp = x.SINTESI_Responsabile,
                    DBPersVal = x.SINTESI_Valutato,
                    DbPersRiporto = x.SINTESI1_Riporto,
                    DBInziativa = x.XR_MBO_INIZIATIVA
                }).ToList();


                foreach (var item in rawList)
                {
                    if (item.DBPersResp != null)
                        item.PersResp = new MboPersona(item.DBPersResp);

                    if (item.DBPersVal != null)
                        item.PersValutato = new MboPersona(item.DBPersVal);

                    if (item.DbPersRiporto != null)
                        item.PersSecRiporto = new MboPersona(item.DbPersRiporto);

                    item.Iniziativa = new MboIniziativa(item.DBInziativa);

                    MboState currentState = (MboState)item.StatoCorrente.ID_STATO;
                    switch (currentState)
                    {
                        case MboState.DaAvviare:
                            break;
                        case MboState.DaCompilare:
                            item.CurrentOperator = item.PersResp;
                            break;
                        case MboState.ObiettiviSottopostiAlDir:
                            item.CurrentOperator = item.PersSecRiporto;
                            break;
                        case MboState.ProntiPerInvioRuo:
                            break;
                        case MboState.ObiettiviSottopostiRuo:
                            break;
                        case MboState.Convalidati:
                            break;
                        case MboState.Consutivati:
                            break;
                        default:
                            break;
                    }

                    reminder.Schede.Add(item);
                }

                reminder.LogEmail = new List<XR_MBO_MAIL_LOG>();
                var dest = reminder.Schede.Select(x => new { Iniziativa = x.Iniziativa.Id, Persona = x.CurrentOperator.Id }).Distinct();
                foreach (var item in dest)
                    reminder.LogEmail.AddRange(db.XR_MBO_MAIL_LOG.Where(x => x.COD_TIPO == "ReminderCompilazione" && x.ID_PERSONA == item.Persona && x.ID_INIZIATIVA == item.Iniziativa));
            }

            return numSchede > 0;
        }
    }
}

namespace myRaiCommonManager.Mbo
{
    public class PdfPrinter
    {
        const int BORDER_NONE = 0;
        const int BORDER_ALL = Rectangle.LEFT_BORDER | Rectangle.TOP_BORDER | Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER;

        const int fontSize = 14;
        const int fontTitleSize = 14;

        int rightAlign = PdfPCell.ALIGN_RIGHT;

        public static Image GetImage(string imagePath, bool setAbsolute = true, float width = 50f, float height = 50f)
        {
            iTextSharp.text.Image png = null;
            string pattern = @"data:image/(gif|png|jpeg|jpg);base64,";
            string imgString = Regex.Replace(imagePath, pattern, string.Empty);
            png = Image.GetInstance(Convert.FromBase64String(imgString));
            if (setAbsolute)
                png.ScaleAbsolute(width, height);
            return png;
        }

        Font myFont;
        Font myFontBold;

        private Document _document;
        private PdfWriter _writer;
        private MemoryStream _ms;
        private string _imagePath;
        private string _imageInt;
        private string _title;

        private int _maxYPage = 100;
        private int _startX = 30;

        private int _startY = 750;

        private PdfReader reader;

        public PdfPrinter(string imagePath, string imageInt, string title, int startY = 800)
        {
            _imagePath = imagePath;
            _imageInt = imageInt;
            _title = title;

            myFont = new FontManager("", BaseColor.BLACK).Normal;
            myFontBold = new FontManager("", BaseColor.BLACK).Bold;

            _startY = startY;
            _startY = 750;
        }

        public bool Apri(byte[] baseTemplate)
        {
            bool isOpened = false;
            try
            {
                reader = new PdfReader(baseTemplate);

                _ms = new MemoryStream();
                //_document = new Document(PageSize.A4);
                _document = new Document(PageSize.A4, 36f, 36f, 90f, 50f);
                _writer = PdfWriter.GetInstance(_document, _ms);
                _writer.PageEvent = new ITextEvents(_imagePath, _imageInt, 0, reader, _startY);
                _document.Open();
                isOpened = true;
            }
            catch (Exception)
            {

            }

            return isOpened;
        }

        public void NewPage()
        {
            _document.NewPage();
        }

        public bool Chiudi(out MemoryStream ms)
        {
            ms = new MemoryStream();
            bool isClosed = false;

            try
            {
                _document.Close();
                _writer.Close();
                byte[] byteInfo = _ms.ToArray();
                ms.Write(byteInfo, 0, byteInfo.Length);
                ms.Position = 0;
                isClosed = true;

            }
            catch (Exception)
            {

            }

            return isClosed;
        }

        public bool AggiungiTableHeader(string iniziativa, string dirStruct, string nominativo, MboPersona compilatore, MboPersona consuntivatore)
        {
            bool result = false;

            PdfContentByte cb = _writer.DirectContent;

            int currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            int lStartX = 30;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            float tableWidth = _document.PageSize.Width - lStartX * 2;
            PdfPTable tableDetail = new PdfPTable(1);
            tableDetail.DefaultCell.BorderWidth = 1;
            tableDetail.TotalWidth = _document.PageSize.Width - lStartX * 2;
            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = new int[] { (int)Math.Truncate(_document.PageSize.Width - lStartX * 2) };
            tableDetail.SetWidths(tableDetailWidth);

            tableDetail.AddCell(WriteCell(iniziativa + " - Scheda obiettivi\n", 1, new FontManager().H3, 0, BORDER_NONE, PdfPCell.ALIGN_MIDDLE, 0));
            tableDetail.AddCell(WriteCell("\n", 1, myFont, 0, BORDER_NONE, PdfPCell.ALIGN_MIDDLE, 0));
            tableDetail.AddCell(WriteCell(nominativo, 1, new FontManager().H2, 0, BORDER_NONE, PdfPCell.ALIGN_MIDDLE, 0));
            //tableDetail.AddCell(WriteCell(dirStruct, 1, myFont, 0, BORDER_NONE, PdfPCell.ALIGN_MIDDLE, 0));

            if (compilatore != null)
                tableDetail.AddCell(WriteCell("Compilatore: " + compilatore.Nominativo.TitleCase(), 1, myFont, 0, BORDER_NONE, PdfPCell.ALIGN_MIDDLE, 0));
            if (consuntivatore != null)
                tableDetail.AddCell(WriteCell("Consuntivatore: " + consuntivatore.Nominativo.TitleCase(), 1, myFont, 0, BORDER_NONE, PdfPCell.ALIGN_MIDDLE, 0));
            else
                tableDetail.AddCell(WriteCell("Consuntivatore: " + compilatore.Nominativo.TitleCase(), 1, myFont, 0, BORDER_NONE, PdfPCell.ALIGN_MIDDLE, 0));

            //tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            //((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();
            _document.Add(tableDetail);

            return result;
        }

        public bool AggiungiTableObiettivi(string tipo, IEnumerable<MboObiettivo> obiettivi, bool isConsuntivata, bool addLastCRLF = true)
        {
            bool result = false;

            if (obiettivi == null || obiettivi.Count() == 0)
                return result;

            PdfContentByte cb = _writer.DirectContent;
            //Da modificare nel caso vengano cambiate le altezze minime delle righe
            float minTableHeight = 140f;

            for (int i = 0; i < obiettivi.Count(); i++)
            {
                var item = obiettivi.ElementAt(i);

                int currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
                int lStartX = 30;

                if (currentY < (_maxYPage + minTableHeight))
                {
                    _document.NewPage();
                    currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
                }

                float tableWidth = _document.PageSize.Width - lStartX * 2;
                int minWidth = (int)Math.Truncate(tableWidth / 5);

                PdfPTable tableDetail = new PdfPTable(4);
                tableDetail.DefaultCell.BorderWidth = 1;
                tableDetail.TotalWidth = _document.PageSize.Width - lStartX * 2;
                tableDetail.LockedWidth = true;
                int[] tableDetailWidth = new int[] { minWidth * 2, minWidth, minWidth, minWidth };
                tableDetail.SetWidths(tableDetailWidth);

                tableDetail.AddCell(WriteCell(tipo, 4, myFontBold, PdfPCell.ALIGN_LEFT, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, new BaseColor(239, 239, 239), fixedHeight: 18f));
                tableDetail.AddCell(WriteCell(item.Descrizione, 3, myFont, PdfPCell.ALIGN_LEFT, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, minHeight: 24f, padding: 5));

                Phrase ph = new Phrase();
                ph.Add(new Chunk("Peso:      ", myFontBold));
                ph.Add(new Chunk(item.PesoSpecifico.ToString("0") + "%", myFont));
                tableDetail.AddCell(WriteCell(ph, 1, myFont, PdfPCell.ALIGN_LEFT, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, minHeight: 24f, padding: 5));

                Phrase phStr = new Phrase();
                phStr.Add(new Paragraph("Obiettivo strategico di Direzione:", myFontBold));
                phStr.Add(new Paragraph("\n", myFont));
                phStr.Add(new Paragraph(item.StrategicoDirezione, myFont));
                phStr.Add(new Paragraph("\n", myFont));

                PdfPCell cell = new PdfPCell(phStr);
                cell.Border = BORDER_ALL;
                cell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                cell.VerticalAlignment = PdfPCell.ALIGN_TOP;
                cell.Colspan = 4;
                cell.MinimumHeight = 48f;
                cell.Padding = 5;
                tableDetail.AddCell(cell);

                Phrase phRisAtt = new Phrase();
                phRisAtt.Add(new Paragraph("Risultato atteso:", myFontBold));
                phRisAtt.Add(new Paragraph("\n", myFont));
                phRisAtt.Add(new Paragraph(item.RisultatoAtteso, myFont));
                phRisAtt.Add(new Paragraph("\n", myFont));

                PdfPCell cellRisAtt = new PdfPCell(phRisAtt);
                cellRisAtt.Border = BORDER_ALL;
                cellRisAtt.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                cellRisAtt.VerticalAlignment = PdfPCell.ALIGN_TOP;
                cellRisAtt.Colspan = 4;
                cellRisAtt.MinimumHeight = 48f;
                cellRisAtt.Padding = 5;
                tableDetail.AddCell(cellRisAtt);

                if (isConsuntivata && !item.Annullato)
                {
                    decimal risultatoPonderato = item.Completamento.GetValueOrDefault() * item.PesoEffettivo.GetValueOrDefault() / 100;
                    tableDetail.AddCell(WriteCell(" ", 2, myFont, borderType: BORDER_NONE, fixedHeight: 16f, padding: 5));
                    tableDetail.AddCell(WriteCell("Risultato conseguito", 1, myFontBold, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_LEFT, 0, new BaseColor(239, 239, 239), 1, 16f));
                    tableDetail.AddCell(WriteCell("Risultato ponderato", 1, myFontBold, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_LEFT, 0, new BaseColor(239, 239, 239), 1, 16f));
                    tableDetail.AddCell(WriteCell(" ", 2, myFont, borderType: BORDER_NONE, fixedHeight: 24f, padding: 5));
                    tableDetail.AddCell(WriteCell(item.Completamento.GetValueOrDefault().ToString("0.0") + "%", 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, fixedHeight: 24f, padding: 5));
                    tableDetail.AddCell(WriteCell(risultatoPonderato.ToString("0.0") + "%", 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, fixedHeight: 24f, padding: 5));
                }
                else
                {
                    tableDetail.AddCell(WriteCell(" ", 2, myFont, borderType: BORDER_NONE, fixedHeight: 16f));
                    tableDetail.AddCell(WriteCell("Obiettivo annullato", 1, myFontBold, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, new BaseColor(239, 239, 239), 1, 16f, padding: 5));
                }

                //tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);
                //((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();
                _document.Add(tableDetail);

                if (i != obiettivi.Count() - 1 || addLastCRLF)
                    WriteLine("\n");
            }

            return result;
        }

        public bool AggiungiTableResult(MboScheda scheda)
        {
            bool result = false;

            PdfContentByte cb = _writer.DirectContent;

            int currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            int lStartX = 30;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }


            float tableWidth = _document.PageSize.Width - lStartX * 2;
            PdfPTable tableDetail = new PdfPTable(4);
            tableDetail.DefaultCell.BorderWidth = 1;
            tableDetail.TotalWidth = _document.PageSize.Width - lStartX * 2;
            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = new int[] { 150, 100, 100, 200 };
            tableDetail.SetWidths(tableDetailWidth);

            float fixedHeight = 24f;

            //tableDetail.AddCell(WriteCell("MBO al 100%", 1, myFontBold, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, new BaseColor(239, 239, 239), 1, fixedHeight));
            //tableDetail.AddCell(WriteCell(" ", 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0));
            //tableDetail.AddCell(WriteCell(" ", 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_NONE, PdfPCell.ALIGN_MIDDLE, 0));
            //tableDetail.AddCell(WriteCell("FIRMA DEL DIRETTORE/RESPONSABILE\n" + nominativoResp, 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_NONE, PdfPCell.ALIGN_TOP, 0, null, 1, fixedHeight));

            //tableDetail.AddCell(WriteCell("MBO da corrispondere", 1, myFontBold, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, new BaseColor(239, 239, 239), 1, fixedHeight));
            //tableDetail.AddCell(WriteCell(" ", 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0));
            //tableDetail.AddCell(WriteCell(" ", 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_NONE, PdfPCell.ALIGN_MIDDLE, 0));
            //tableDetail.AddCell(WriteCell(" ", 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_NONE, PdfPCell.ALIGN_TOP, 0, null, 3, fixedHeight));

            tableDetail.AddCell(WriteCell("Completamento", 1, myFontBold, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, new BaseColor(239, 239, 239), 1, fixedHeight));
            tableDetail.AddCell(WriteCell(scheda.Completamento.Value.ToString("0.0") + "%", 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0));
            tableDetail.AddCell(WriteCell(" ", 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_NONE, PdfPCell.ALIGN_MIDDLE, 0));
            tableDetail.AddCell(WriteCell("", 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_NONE, PdfPCell.ALIGN_MIDDLE, 0, null, 1, fixedHeight));

            //tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            //((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();
            _document.Add(tableDetail);

            return result;
        }

        internal static PdfPCell WriteCell(string text, int colspan, iTextSharp.text.Font f, int align = Element.ALIGN_LEFT, int borderType = BORDER_NONE, int vAlign = PdfPCell.ALIGN_TOP, int leading = 16, BaseColor bgColor = null, int rowSpan = 1, float fixedHeight = 0, float minHeight = 0, float padding = -1)
        {
            Phrase phrase = new Phrase(text, f);

            return WriteCell(phrase, colspan, f, align, borderType, vAlign, leading, bgColor, rowSpan, fixedHeight, minHeight, padding);
        }

        internal static PdfPCell WriteCell(Phrase phrase, int colspan, iTextSharp.text.Font f, int align = Element.ALIGN_LEFT, int borderType = BORDER_NONE, int vAlign = PdfPCell.ALIGN_TOP, int leading = 16, BaseColor bgColor = null, int rowSpan = 1, float fixedHeight = 0, float minHeight = 0, float padding = -1)
        {
            PdfPCell cell = new PdfPCell(phrase);
            if (fixedHeight > 0)
                cell.FixedHeight = fixedHeight;
            if (minHeight > 0)
                cell.MinimumHeight = minHeight;
            cell.Border = borderType;
            cell.HorizontalAlignment = align;
            cell.VerticalAlignment = vAlign;
            cell.Colspan = colspan;
            cell.Rowspan = rowSpan;
            if (leading > 0)
                cell.SetLeading(leading, 0);
            if (bgColor != null)
                cell.BackgroundColor = bgColor;
            if (padding > -1)
                cell.Padding = padding;
            return cell;
        }

        public void WriteLine(string p, bool isTitle = false, Font font = null, int border = BORDER_NONE, int vAlign = PdfPCell.ALIGN_TOP, int leading = 16)
        {
            int currentY = 750;
            int lStartX = 30;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            PdfPTable tableDetail = new PdfPTable(1);
            tableDetail.DefaultCell.BorderWidth = 1;
            tableDetail.TotalWidth = _document.PageSize.Width - lStartX * 2;
            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = new int[] { (int)tableDetail.TotalWidth };
            tableDetail.SetWidths(tableDetailWidth);

            if (font == null)
                font = myFont;

            tableDetail.AddCell(WriteCell(p, 1, font, Element.ALIGN_JUSTIFIED, border, vAlign, leading));

            //tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            //((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();
            _document.Add(tableDetail);
        }

        public void AggiungiFirma(iTextSharp.text.Image png, string signText)
        {
            int currentY = 750;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                currentY = _maxYPage + 10;
            }

            Font firmaFont = new FontManager("", new BaseColor(0, 0, 153)).Normal;

            PdfPTable table = new PdfPTable(3);
            table.DefaultCell.BorderWidth = 1;
            table.TotalWidth = _document.PageSize.Width - lStartX * 2;
            table.LockedWidth = true;
            int[] widths = new int[] { 250, 220, 30 };
            table.SetWidths(widths);

            table.AddCell(WriteCell(" ", 3, myFont));

            table.AddCell(WriteCell("", 1, myFont));
            table.AddCell(WriteCell(new Phrase(signText, firmaFont), 1, firmaFont, Element.ALIGN_CENTER));
            table.AddCell(WriteCell("", 1, myFont));

            int tableHeight = (int)table.CalculateHeights();
            //table.WriteSelectedRows(0, (table.Rows.Count + 1), lStartX, currentY, cb);
            //((ITextEvents)_writer.PageEvent).CurrentY -= (int)table.CalculateHeights();
            _document.Add(table);

            if (png != null)
            {
                table = new PdfPTable(2);
                table.DefaultCell.BorderWidth = 1;
                table.TotalWidth = _document.PageSize.Width - lStartX * 2;
                table.LockedWidth = true;
                widths = new int[] { 490, 30 };
                table.SetWidths(widths);

                table.AddCell(WriteCell(" ", 2, myFont));
                table.AddCell(WriteCell(" ", 2, myFont));

                PdfPCell cellImage = new PdfPCell(png);
                cellImage.Border = BORDER_NONE;
                cellImage.HorizontalAlignment = rightAlign;
                table.AddCell(cellImage);

                table.AddCell(WriteCell("", 1, myFont));
                table.AddCell(WriteCell("", 2, myFont));

                //table.WriteSelectedRows(0, (table.Rows.Count + 1), lStartX, currentY, cb);
                //((ITextEvents)_writer.PageEvent).CurrentY -= (int)table.CalculateHeights();
                _document.Add(table);
            }
        }

        public void AggiungiPagina()
        {
            _document.NewPage();
        }

        internal void AggiungiValutazione(Valutazione Model)
        {
            PdfContentByte cb = _writer.DirectContent;

            int currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            int lStartX = 30;
            float minTableHeight = 140f;

            if (currentY < (_maxYPage + minTableHeight))
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            float tableWidth = _document.PageSize.Width - lStartX * 2;

            //Calcolo il numero di colonne delle radio
            int number = 0;
            foreach (var item in Model.Scheda.XR_VAL_EVAL_SHEET_QST.Where(x => x.XR_VAL_QUESTION.XR_VAL_QUESTION_DISPLAY.NAME == "Radio button"))
            {
                int tmpCount = item.XR_VAL_QUESTION.XR_VAL_QUESTION_ANSWER.Count(x => x.VALID_DTA_END == null);
                if (tmpCount > number)
                    number = tmpCount;
            }

            //int number = Model.Scheda.XR_VAL_EVAL_SHEET_QST.Where(x => x.XR_VAL_QUESTION.XR_VAL_QUESTION_DISPLAY.NAME == "Radio button").Select(x => x.XR_VAL_QUESTION.XR_VAL_QUESTION_ANSWER.Count(y => y.VALID_DTA_END == null)).Max();
            int numCol = 2 + (number > 0 ? number : 1);
            int minWidth = (int)Math.Truncate(tableWidth / (numCol + 4));

            PdfPTable table = new PdfPTable(numCol);
            table.DefaultCell.BorderWidth = 1;
            table.TotalWidth = _document.PageSize.Width - lStartX * 2;
            table.LockedWidth = true;
            List<int> tableDetailWidth = new List<int>();
            tableDetailWidth.Add(minWidth * 3);
            tableDetailWidth.Add(minWidth * 3);
            for (int i = 2; i < numCol; i++)
            {
                tableDetailWidth.Add(minWidth);
            }
            table.SetWidths(tableDetailWidth.ToArray());

            List<XR_VAL_QUESTION_GROUP> dGroups = new List<XR_VAL_QUESTION_GROUP>();
            IEnumerable<XR_VAL_QUESTION_GROUP> groups = Model.Scheda.XR_VAL_EVAL_SHEET_QST.OrderBy(x => x.ORDER).GroupBy(x => x.XR_VAL_QUESTION.XR_VAL_QUESTION_GROUP).Select(x => x.Key);
            while (groups.Any(x => x.ID_QST_GROUP_MACRO != null))
            {
                foreach (var item in groups)
                {
                    var index = dGroups.FindIndex(x => x.ID_QST_GROUP_MACRO == item.ID_QST_GROUP);
                    if (index >= 0)
                    {
                        dGroups.Insert(index, item);
                    }
                    else
                    {
                        dGroups.Add(item);
                    }
                }

                groups = groups.GroupBy(x => x.XR_VAL_QUESTION_GROUP2).Where(x => x.Key != null).Select(x => x.Key);
            }
            foreach (var item in groups)
            {
                var index = dGroups.FindIndex(x => x.ID_QST_GROUP_MACRO == item.ID_QST_GROUP);
                if (index >= 0)
                {
                    dGroups.Insert(index, item);
                }
                else
                {
                    dGroups.Add(item);
                }
            }

            int bgColorMax = 40;
            int bgColorMin = 20;
            int bgColor = 40;
            bool hasAnswerHeader = false;

            BaseColor neutral40 = new BaseColor(194, 207, 214);
            BaseColor neutral20 = new BaseColor(236, 241, 243);
            PdfPCell cell = null;
            float padding = 5f;

            int row = 0;

            foreach (var item in dGroups)
            {
                if (item.XR_VAL_QUESTION_GROUP1 != null && item.XR_VAL_QUESTION_GROUP1.Any())
                {
                    bool drawQstHeader = false;
                    XR_VAL_QUESTION rifHeader = null;
                    var tmp = Model.Scheda.XR_VAL_EVAL_SHEET_QST.Where(x => x.XR_VAL_QUESTION.XR_VAL_QUESTION_GROUP.ID_QST_GROUP_MACRO == item.ID_QST_GROUP).Select(x => x.XR_VAL_QUESTION);

                    if (tmp.Any())
                    {
                        hasAnswerHeader = false;
                        if (tmp.All(x => x.XR_VAL_QUESTION_DISPLAY.NAME == "Radio button"))
                        {
                            var answCount = tmp.Select(x => x.XR_VAL_QUESTION_ANSWER.Count()).Distinct();
                            if (answCount.Count() == 1)
                            {
                                var answList = tmp.SelectMany(x => x.XR_VAL_QUESTION_ANSWER.Select(y => y.ID_ANSWER)).Distinct();
                                drawQstHeader = answList.Count() == answCount.ElementAt(0);
                                if (drawQstHeader)
                                {
                                    hasAnswerHeader = true;
                                    rifHeader = tmp.First();
                                }
                            }
                        }
                    }
                    else if (item.XR_VAL_QUESTION_GROUP1.All(x => x.XR_VAL_QUESTION_GROUP1 == null || !x.XR_VAL_QUESTION_GROUP1.Any()))
                    {
                        //Se il macrogruppo non ha domande nella scheda, e tutti i suoi sotto-gruppi non hanno ulteriori sotto-gruppi
                        continue;
                    }
                    else
                    {
                        bgColor = bgColor < bgColorMax ? bgColor + 20 : bgColor;
                    }

                    table.AddCell(WriteCell(item.NAME, 2, myFontBold, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, bgColor == 20 ? neutral20 : neutral40, padding: padding));
                    if (drawQstHeader)
                    {
                        foreach (var answ in rifHeader.XR_VAL_QUESTION_ANSWER.Where(x => x.VALID_DTA_END == null).OrderBy(x => x.NUM_ORDER))
                        {
                            string header = myRaiHelper.HtmlHelper.GetInnerText(answ.XR_VAL_ANSWER.DESCRIPTION) ?? "";
                            table.AddCell(WriteCell(header.Trim(), 1, myFontBold, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, bgColor == 20 ? neutral20 : neutral40, padding: padding)); //Header
                        }
                    }
                    else
                    {
                        //table.AddCell(WriteCell(item.NAME, numCol, myFontBold, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, bgColor == 20 ? neutral20 : neutral40));
                        table.AddCell(WriteCell("", number, myFont, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, bgColor == 20 ? neutral20 : neutral40, padding: padding)); //Header
                    }

                    bgColor = bgColor > bgColorMin ? bgColor - 20 : bgColor;
                }

                if (item.XR_VAL_QUESTION.Any())
                {
                    string groupname = item.NAME;
                    string groupDes = item.DESCRIPTION;
                    int qstGroup = item.XR_VAL_QUESTION.Count();
                    bool isFirst = true;

                    var lstQst = Model.Scheda.XR_VAL_EVAL_SHEET_QST.Where(x => x.XR_VAL_QUESTION.ID_QST_GROUP == item.ID_QST_GROUP);

                    foreach (var qst in lstQst.OrderBy(x => x.ORDER))
                    {
                        var question = qst.XR_VAL_QUESTION;
                        string strValue = "";
                        int? intValue = null;
                        var ratings = Model.Rating.Where(x => x.ID_QUESTION == qst.ID_SHEET_QST).OrderBy(x => x.XR_VAL_EVAL_RATING_OWNER.ORDER);
                        if (Model.Rating.Any())
                        {
                            intValue = ratings.Select(x => x.VALUE_INT).FirstOrDefault();
                            strValue = ratings.Select(x => x.VALUE_STR).FirstOrDefault();
                        }

                        bool hasTextDes = !String.IsNullOrWhiteSpace(question.NAME) || !String.IsNullOrWhiteSpace(question.DESCRIPTION);
                        if (hasTextDes)
                        {
                            string text = myRaiHelper.HtmlHelper.GetInnerText(question.NAME) ?? "";
                            table.AddCell(WriteCell(text.Trim(), 2, myFont, PdfPCell.ALIGN_LEFT, BORDER_ALL, PdfPCell.ALIGN_TOP, 0, padding: padding));
                            //teoricamente va a capo con la description
                        }

                        string qstDisplay = question.XR_VAL_QUESTION_DISPLAY.NAME;
                        if (qstDisplay == "Radio button")
                        {
                            foreach (var answ in question.XR_VAL_QUESTION_ANSWER.OrderBy(x => x.NUM_ORDER))
                            {
                                string val = answ.XR_VAL_ANSWER.VALUE_INT == intValue.GetValueOrDefault() ? "X" : "";
                                if (hasAnswerHeader)
                                {
                                    table.AddCell(WriteCell(val, 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, padding: padding));
                                }
                                else
                                {
                                    string header = myRaiHelper.HtmlHelper.GetInnerText(answ.XR_VAL_ANSWER.DESCRIPTION) ?? "";
                                    table.AddCell(WriteCell(val + " " + header.Trim(), 1, myFont, PdfPCell.ALIGN_CENTER, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, padding: padding));
                                }
                            }
                        }
                        else if (qstDisplay == "Edit")
                        {
                            table.AddCell(WriteCell(strValue, hasTextDes ? number : numCol, myFont, PdfPCell.ALIGN_LEFT, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, padding: padding));
                        }
                        else if (qstDisplay == "Select")
                        {
                            string val = "";

                            if (question.XR_VAL_QUESTION_TYPE.NAME == "Intero")
                            {
                                val = question.XR_VAL_QUESTION_ANSWER.Where(x => x.VALID_DTA_END == null && x.XR_VAL_ANSWER.VALUE_INT == intValue).Select(x => x.XR_VAL_ANSWER.DESCRIPTION).FirstOrDefault();
                            }
                            else
                            {
                                val = question.XR_VAL_QUESTION_ANSWER.Where(x => x.VALID_DTA_END == null && x.XR_VAL_ANSWER.VALUE_STR == strValue).Select(x => x.XR_VAL_ANSWER.DESCRIPTION).FirstOrDefault();
                            }
                            table.AddCell(WriteCell(val, hasTextDes ? number : numCol, myFont, PdfPCell.ALIGN_LEFT, BORDER_ALL, PdfPCell.ALIGN_MIDDLE, 0, padding: padding));
                        }


                    }
                }


                if (currentY < (_maxYPage + minTableHeight))
                {
                    _document.NewPage();
                    currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
                }

                //table.WriteSelectedRows(row, row, lStartX, currentY, cb);
                row++;
            }

            _document.Add(table);
            //((ITextEvents)_writer.PageEvent).CurrentY -= (int)table.CalculateHeights();

        }
    }

    class ITextEvents : PdfPageEventHelper
    {
        string _imgPath;
        int _pageStart;
        string _imgInfo;

        PdfReader _reader;

        iTextSharp.text.Image _pngInfo = null;
        iTextSharp.text.Image _pngImage = null;

        public ITextEvents(string imgPath = "", string imgInfo = "", int pageStart = 0, PdfReader reader = null, int startY = 800)
        {
            this._imgPath = imgPath;
            this._pageStart = pageStart;
            this._imgInfo = imgInfo;

            if (!String.IsNullOrWhiteSpace(_imgPath))
            {
                _pngImage = PdfPrinter.GetImage(_imgPath);
            }

            if (!String.IsNullOrWhiteSpace(_imgInfo))
            {
                _pngInfo = PdfPrinter.GetImage(_imgInfo);
                _pngInfo.ScalePercent(30f);
            }

            _reader = reader;

            _startY = startY;
            _startY = 750;
        }

        #region Fields
        private string _header;
        #endregion

        #region Properties
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }

        public int CurrentY
        {
            get
            {
                return this.currentY;
            }
            set
            {
                this.currentY = value;
            }
        }
        #endregion

        // This is the contentbyte object of the writer
        PdfContentByte cb;

        // we will put the final number of pages in a template
        PdfTemplate headerTemplate, footerTemplate;

        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;

        private int _startY = 750;// (int)Math.Truncate(PageSize.A4.Height);

        int currentY = (int)Math.Truncate(PageSize.A4.Height);
        const int lStartX = 30;
        const int fontSize = 12;
        BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            int _currentY = _startY;
            this.CurrentY = _currentY;

            int intestazioneHeight = 0;
            headerTemplate = cb.CreateTemplate(500, intestazioneHeight);
            cb.AddTemplate(headerTemplate, document.LeftMargin, document.PageSize.GetTop(document.TopMargin));
            writer.DirectContent.AddTemplate(writer.GetImportedPage(_reader, 1), 0, 0);
        }

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                // Aggiunta metadata al documento
                document.AddAuthor("HRIS");

                document.AddCreator("HRIS con l'ausilio di iTextSharp");

                document.AddKeywords("PDF MBO");

                document.AddSubject("Scheda MBO");

                document.AddTitle("");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            cb = writer.DirectContent;
            footerTemplate = cb.CreateTemplate(50, 50);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);
            //writer.DirectContent.AddTemplate(writer.GetImportedPage(_reader, 1), 0, 0);
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            footerTemplate.BeginText();
            footerTemplate.SetFontAndSize(bf, fontSize);
            footerTemplate.SetTextMatrix(0, 0);
            footerTemplate.ShowText("" + (writer.PageNumber));
            footerTemplate.EndText();
        }
    }
}