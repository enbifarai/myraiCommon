using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.css;
using myRai.Business;
using myRaiCommonModel;
using myRaiCommonTasks.Helpers;
using myRaiData;
using myRaiData.Incentivi;
using myRaiDataTalentia;
using myRaiHelper;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using MimeTypeMap = myRaiCommonTasks.Helpers.MimeTypeMap;

namespace myRaiCommonManager
{
    public class DematerializzazioneManager
    {
        CultureInfo cultureInfo = CultureInfo.GetCultureInfo("it-IT");
        const int fontIntestazione = 12;
        const int fontCorpo = 12;
        const string FONTNAME = "Times-Roman";
        const string SPAZIO = " ";

        private static BaseColor color = new BaseColor(System.Drawing.Color.Black);
        private static Font myFontIntestazione = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.NORMAL, color);
        private static Font myFontIntestazioneBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
        private static Font myFontIntestazioneBoldItalic = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLDITALIC, color);
        private static Font myFontCorpo = FontFactory.GetFont(FONTNAME, fontCorpo, Font.NORMAL, color);
        private static Font myFontCorpoBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
        private static Font myFontCorpoSottolineato = FontFactory.GetFont(FONTNAME, fontCorpo, Font.UNDERLINE, color);
        private static Font myFontCorpoItalic = FontFactory.GetFont(FONTNAME, fontCorpo, Font.ITALIC, color);
        private static Font myFontInterlinea = FontFactory.GetFont(FONTNAME, 7, Font.NORMAL, color);

        public static List<XR_DEM_DOCUMENTI_EXT> GetMieiDocumenti(string matricola)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    matricola = UtenteHelper.Matricola();
                }

                var db = AnagraficaManager.GetDb();

                var items = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.MatricolaDestinatario.Equals(matricola) && w.PraticaAttiva).ToList();

                if (items != null && items.Any())
                {
                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                    }

                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                    {
                        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                        result.Add(newItem);
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            if (result != null && result.Any())
            {
                result = result.DistinctBy(w => w.Id).ToList();
                result = result.OrderByDescending(w => w.Id).ToList();
            }

            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> BKGetDocumentiDaApprovare(string nominativo = null, string matricola = null, string id_Tipo_Doc = null, string statoRichiesta = null)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            IQueryable<XR_DEM_DOCUMENTI> _tempItems = null;
            List<XR_DEM_DOCUMENTI> items = null;
            try
            {
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                IncentiviEntities db = new IncentiviEntities();

                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

                // se ho la funzione 01ADM - Sono approvatore
                if (!subFunc.Contains("01ADM"))
                {
                    throw new Exception("Utente non autorizzato");
                }

                _tempItems = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva);

                if (!String.IsNullOrEmpty(nominativo) ||
                    !String.IsNullOrEmpty(matricola) ||
                    !String.IsNullOrEmpty(id_Tipo_Doc) ||
                    !String.IsNullOrEmpty(statoRichiesta))
                {
                    if (!String.IsNullOrEmpty(matricola))
                    {
                        _tempItems = _tempItems.Where(w => w.MatricolaDestinatario.Equals(matricola));
                    }

                    if (!String.IsNullOrEmpty(statoRichiesta))
                    {
                        int id_stato_filtro = 0;
                        bool converti = int.TryParse(statoRichiesta, out id_stato_filtro);

                        if (converti)
                        {
                            _tempItems = _tempItems.Where(w => w.Id_Stato.Equals(id_stato_filtro));
                        }
                        else
                        {
                            throw new Exception("Errore, stato pratica non riconosciuto");
                        }
                    }

                    if (!String.IsNullOrEmpty(id_Tipo_Doc))
                    {
                        int id_Tipo_Doc_filtro = 0;
                        bool converti = int.TryParse(id_Tipo_Doc, out id_Tipo_Doc_filtro);

                        if (converti)
                        {
                            _tempItems = _tempItems.Where(w => w.Id_Tipo_Doc.Equals(id_Tipo_Doc_filtro));
                        }
                        else
                        {
                            throw new Exception("Errore, tipologia documentale non riconosciuta");
                        }
                    }

                    if (!String.IsNullOrEmpty(nominativo))
                    {

                    }
                }
                else
                {
                    _tempItems = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva && w.Id_Stato != (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore);
                }

                items = _tempItems.ToList();

                if (items != null && items.Any())
                {
                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                    }

                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                    {
                        var r = AuthHelper.EnableToMatr(matricolaCorrente, i.MatricolaCreatore, "DEMA", "01ADM");
                        if (!r.Enabled || r.Visibilita != AbilMatrLiv.VisibilitaEnum.Filtrata)
                        {
                            continue;
                        }

                        string destinatario_Da_WKF = "";
                        bool esito = false;
                        int nextStatus = GetNextIdStato(i.Id_Stato, i.Id_WKF_Tipologia);
                        esito = GetDestinatario(i.Id_WKF_Tipologia, i.Id_Stato, out destinatario_Da_WKF);

                        // se il prossimo stato è azione automatica allora va visto lo stato ancora successivo
                        // perchè lo stato di accettato o visionato potrebbe venir dopo ad una azione automatica
                        if (nextStatus == (int)StatiDematerializzazioneDocumenti.AzioneAutomatica || nextStatus == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaContabile)
                        {
                            nextStatus = GetNextIdStato(nextStatus, i.Id_WKF_Tipologia);
                        }

                        // se il prossimo stato è ACCETTATO oppure AzioneAutomatica allora il documento deve essere approvato e va visualizzato nella pagina
                        // dell'approvatore
                        if ((nextStatus == (int)StatiDematerializzazioneDocumenti.Accettato) &&
                            esito &&
                            (String.IsNullOrEmpty(destinatario_Da_WKF) ||
                            destinatario_Da_WKF.Contains("01ADM"))
                            )
                        {
                            XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                            newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                            result.Add(newItem);
                        }
                    }
                }

                //if ( itemsAdm != null && itemsAdm.Any( ) )
                //{
                //    if ( result == null )
                //    {
                //        result = new List<XR_DEM_DOCUMENTI_EXT>( );
                //    }

                //    foreach ( var i in itemsAdm )
                //    {
                //        int nextStatus = GetNextIdStato( i.Id_Stato , i.Id_WKF_Tipologia );
                //        // se il prossimo stato è ACCETTATO allora il documento deve essere approvato e va visualizzato nella pagina
                //        // dell'approvatore
                //        if ( nextStatus == ( int ) StatiDematerializzazioneDocumenti.Accettato )
                //        {
                //            XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT( );
                //            newItem = ConvertiInXR_DEM_DOCUMENTI_EXT( i );
                //            result.Add( newItem );
                //        }
                //    }
                //}
                if (result != null && result.Any())
                {
                    result = result.DistinctBy(w => w.Id).ToList();
                    result = result.OrderByDescending(w => w.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static bool GetDestinatario(int idTipologiaWKF, int idStatoCorrente, out string destinatario)
        {
            bool esito = true;
            destinatario = "";
            try
            {
                var db = AnagraficaManager.GetDb();

                var item = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(idTipologiaWKF) && w.ID_STATO.Equals(idStatoCorrente)).FirstOrDefault();

                if (item == null)
                {
                    throw new Exception("Errore! Workflow non trovato");
                }

                destinatario = item.DESTINATARIO;
                esito = true;
            }
            catch
            {
                esito = false;
            }

            return esito;
        }
        public static bool GetDestinatarioReloaded(int idTipologiaWKF, int idStatoCorrente, List<XR_WKF_WORKFLOW> WKF, out string destinatario)
        {
            bool esito = true;
            destinatario = "";
            try
            {
                var db = AnagraficaManager.GetDb();

                var item = WKF.Where(w => w.ID_TIPOLOGIA.Equals(idTipologiaWKF) && w.ID_STATO.Equals(idStatoCorrente)).FirstOrDefault();

                if (item == null)
                {
                    esito = false; return esito;
                    throw new Exception("Errore! Workflow non trovato");
                }

                destinatario = item.DESTINATARIO;
                esito = true;
            }
            catch
            {
                esito = false;
            }

            return esito;
        }

        public static string GetNominativoByMatricola(string matricola)
        {
            string result = "";

            result = CezanneHelper.GetNominativoByMatricola(matricola);

            return result;
        }
        public static string GetNominativoByMatricolaReloaded(string matricola, List<MatrNom> NOM)
        {
            return NOM.Where(x => x.matricola == matricola).Select(x => x.nominativo).FirstOrDefault();
        }
        private static int GetStatoAvanzamentoReloaded(int idTipoWKF, int idStato, List<XR_WKF_WORKFLOW> WKF, List<XR_DEM_STATI> STATI)
        {
            int percentuale = 0;
            IncentiviEntities dbCzn = new IncentiviEntities();

            var wkf = WKF.Where(w => w.ID_TIPOLOGIA == idTipoWKF).ToList();

            int contaTuttiGliStati = wkf.DistinctBy(w => w.ORDINE).Count();

            // prende tutti gli stati azione automatica
            var auto = STATI.Where(w => w.DESCRIZIONE.Contains("Azione")).ToList().Select(w => w.ID_STATO).ToList();

            //var tmp = wkf.Where( w => w.ID_STATO <= idStato ).ToList( );

            // prende tutti gli stati <= idstato escludendo gli stati azione automatica
            //List<XR_WKF_WORKFLOW> tmp = wkf.Where(w => w.ID_STATO <= idStato).Where(w => !auto.Contains(w.ID_STATO)).ToList();            
            //int contaPosizioneStatoCorrente = tmp.DistinctBy( w => w.ORDINE ).Count( );
            int contaPosizioneStatoCorrente = 0;
            var tmp = wkf.FirstOrDefault(w => w.ID_STATO == idStato);
            if (tmp != null)
            {
                contaPosizioneStatoCorrente = tmp.ORDINE;
            }
            else
            {
                // se è null vuol dire che lo stato non è presente nel workflow allora di default mettiamo 1
                // ad esempio ci sono casi in cui nel workflow dallo stato 20 si passa al 40, ma la pratica
                // assume anche lo stato 30 (vistato) che non è previsto dal wkf, ma è impostato sulla tabella
                // comportamento, questo perchè per esempio per lo stesso flusso, la produzione ha il vistatore
                // mentre altre direzioni no
                contaPosizioneStatoCorrente = 1;
            }

            float tmpP = (float)contaPosizioneStatoCorrente / (float)contaTuttiGliStati;
            tmpP *= 100;

            percentuale = (int)tmpP;

            return percentuale;
        }

        private static int GetStatoAvanzamento(int idTipoWKF, int idStato)
        {
            int percentuale = 0;
            IncentiviEntities dbCzn = new IncentiviEntities();

            var wkf = dbCzn.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA == idTipoWKF).ToList();

            int contaTuttiGliStati = wkf.DistinctBy(w => w.ORDINE).Count();

            // prende tutti gli stati azione automatica
            var auto = dbCzn.XR_DEM_STATI.Where(w => w.DESCRIZIONE.Contains("Azione")).ToList().Select(w => w.ID_STATO).ToList();

            //var tmp = wkf.Where( w => w.ID_STATO <= idStato ).ToList( );

            // prende tutti gli stati <= idstato escludendo gli stati azione automatica
            //List<XR_WKF_WORKFLOW> tmp = wkf.Where(w => w.ID_STATO <= idStato).Where(w => !auto.Contains(w.ID_STATO)).ToList();            
            //int contaPosizioneStatoCorrente = tmp.DistinctBy( w => w.ORDINE ).Count( );
            int contaPosizioneStatoCorrente = 0;
            var tmp = wkf.FirstOrDefault(w => w.ID_STATO == idStato);
            if (tmp != null)
            {
                contaPosizioneStatoCorrente = tmp.ORDINE;
            }
            else
            {
                // se è null vuol dire che lo stato non è presente nel workflow allora di default mettiamo 1
                // ad esempio ci sono casi in cui nel workflow dallo stato 20 si passa al 40, ma la pratica
                // assume anche lo stato 30 (vistato) che non è previsto dal wkf, ma è impostato sulla tabella
                // comportamento, questo perchè per esempio per lo stesso flusso, la produzione ha il vistatore
                // mentre altre direzioni no
                contaPosizioneStatoCorrente = 1;
            }

            float tmpP = (float)contaPosizioneStatoCorrente / (float)contaTuttiGliStati;
            tmpP *= 100;

            percentuale = (int)tmpP;

            return percentuale;
        }

        private static XR_DEM_DOCUMENTI_EXT ConvertiInXR_DEM_DOCUMENTI_EXT(XR_DEM_DOCUMENTI i, List<XR_WKF_WORKFLOW> WKF = null,
            List<XR_DEM_STATI> STATI = null, List<MatrNom> NOM = null)
        {
            XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
            newItem.CodiceProtocollatore = i.CodiceProtocollatore;
            newItem.Cod_Tipologia_Documentale = i.Cod_Tipologia_Documentale;
            newItem.DataApprovazione = i.DataApprovazione;
            newItem.DataCreazione = i.DataCreazione;
            newItem.DataFirma = i.DataFirma;
            newItem.DataInvioNotifica = i.DataInvioNotifica;
            newItem.Descrizione = i.Descrizione;
            newItem.Id = i.Id;
            newItem.IdPersonaApprovatore = i.IdPersonaApprovatore;
            newItem.IdPersonaCreatore = i.IdPersonaCreatore;
            newItem.IdPersonaDestinatario = i.IdPersonaDestinatario;
            newItem.IdPersonaFirma = i.IdPersonaFirma;
            newItem.Id_Richiesta = i.Id_Richiesta;
            newItem.Id_Stato = i.Id_Stato;
            newItem.Id_Tipo_Doc = i.Id_Tipo_Doc;
            newItem.Id_WKF_Tipologia = i.Id_WKF_Tipologia;
            newItem.MatricolaApprovatore = i.MatricolaApprovatore;
            newItem.MatricolaCreatore = i.MatricolaCreatore;
            newItem.MatricolaDestinatario = i.MatricolaDestinatario;
            newItem.MatricolaFirma = i.MatricolaFirma;
            newItem.Note = i.Note;
            newItem.NumeroProtocollo = i.NumeroProtocollo;
            newItem.TimeStamp = i.TimeStamp;
            newItem.PraticaAttiva = i.PraticaAttiva;
            newItem.CustomDataJSON = i.CustomDataJSON;
            newItem.MatricolaVisualizzatore = i.MatricolaVisualizzatore;
            newItem.DataVisto = i.DataVisto;

            if (!String.IsNullOrEmpty(i.MatricolaApprovatore))
            {
                newItem.NominativoUtenteApprovatore = (NOM == null ? GetNominativoByMatricola(i.MatricolaApprovatore)
                                                                : GetNominativoByMatricolaReloaded(i.MatricolaApprovatore, NOM));
            }

            if (!String.IsNullOrEmpty(i.MatricolaCreatore))
            {
                newItem.NominativoUtenteCreatore = (NOM == null ? GetNominativoByMatricola(i.MatricolaCreatore)
                                                                : GetNominativoByMatricolaReloaded(i.MatricolaCreatore, NOM));
            }

            if (!String.IsNullOrEmpty(i.MatricolaDestinatario))
            {
                newItem.NominativoUtenteDestinatario = (NOM == null ? GetNominativoByMatricola(i.MatricolaDestinatario)
                                                                : GetNominativoByMatricolaReloaded(i.MatricolaDestinatario, NOM));
            }

            if (!String.IsNullOrEmpty(i.MatricolaVisualizzatore))
            {
                newItem.NominativoUtenteVistatore = (NOM == null ? GetNominativoByMatricola(i.MatricolaVisualizzatore)
                                                                : GetNominativoByMatricolaReloaded(i.MatricolaVisualizzatore, NOM));
            }
            if (WKF != null && STATI != null)
                newItem.Avanzamento = GetStatoAvanzamentoReloaded(i.Id_WKF_Tipologia, i.Id_Stato, WKF, STATI);
            else
                newItem.Avanzamento = GetStatoAvanzamento(i.Id_WKF_Tipologia, i.Id_Stato);
            newItem.IsDuplicable = true;

            return newItem;
        }

        /// <summary>
        /// Reperimento dello stato successivo in base alla definizione del workflow
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="idTipologia"></param>
        /// <returns></returns>
        public static int GetNextIdStato(int currentState, int idTipologia, bool skipRifiuto = false)
        {
            int result = 0;

            try
            {
                IncentiviEntities db = new IncentiviEntities();

                var item = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(idTipologia)).ToList();

                if (item == null || !item.Any())
                {
                    throw new Exception("Errore! Workflow non trovato");
                }

                if (skipRifiuto)
                {
                    item = item.Where(w => w.ID_STATO != (int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente &&
                    w.ID_STATO != (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore &&
                    w.ID_STATO != (int)StatiDematerializzazioneDocumenti.RifiutatoFirma).ToList();
                }

                if (currentState == 0)
                {
                    var ordinato = item.OrderBy(w => w.ORDINE).ToList();
                    result = ordinato.FirstOrDefault().ID_STATO;
                }
                else
                {
                    if (currentState == (int)StatiDematerializzazioneDocumenti.AzioneAutomatica || currentState == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaContabile)
                    {

                    }

                    // Visionato non è un parametro da workflow, ma è uno stato virtuale gestito da codice, 
                    // quindi se lo stato è Visionato nel workflow, va considerato lo stato subito precedente che potrebbe assumere, quindi 
                    // lo stato 20, Pronto Visione
                    if (currentState == (int)StatiDematerializzazioneDocumenti.Visionato)
                    {
                        // se lo stato è "Visionato" e nel workflow non esiste un record con questo stato, 
                        // allora va considerato lo stato precedente Pronto Visione (20)
                        bool exists = item.Count(w => w.ID_STATO == currentState) > 0;
                        if (!exists)
                        {
                            currentState = (int)StatiDematerializzazioneDocumenti.ProntoVisione;
                        }
                    }

                    //var ordinato = item.Where(w => w.ID_STATO > currentState).OrderBy(w => w.ORDINE).ThenBy(w => w.ID_STATO).ToList();

                    var ordinato = item.OrderBy(w => w.ORDINE).ThenBy(w => w.ID_STATO).Select(w => w.ID_STATO).ToList();
                    if (ordinato.Any())
                    {
                        int pos = ordinato.IndexOf(currentState);
                        if (pos < (ordinato.Count - 1))
                        {
                            result = ordinato[pos + 1];
                        }
                        else
                        {
                            result = 0;
                        }
                    }
                    else
                    {
                        result = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                result = 0;
            }
            return result;
        }

        /// <summary>
        /// Reperimento dello stato successivo in base alla definizione del workflow
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="idTipologia"></param>
        /// <returns></returns>
        public static string GetNextOrdineWKF(int currentState, int idTipologia, string descrizione)
        {
            string result = string.Empty;

            try
            {
                IncentiviEntities db = new IncentiviEntities();

                var item = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(idTipologia) && w.ID_STATO == currentState).FirstOrDefault();

                //Ci sono dei casi, tipo per maternità e Congedi dove la pratica ha un id_Stato == 30 (Visionato), ma nel Workflow non esiste l'id_Stato 30
                //Per far vedere comunque la label giusta eseguo questo blocco di codice
                if (item == null)
                {
                    var wkf = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(idTipologia)).ToList();
                    foreach (var dbx in wkf)
                    {
                        var codice = db.XR_DEM_TIPI_DOCUMENTO.Where(k => k.Descrizione.Equals(descrizione)).Select(k=>k.Codice).FirstOrDefault();

                        if (codice == "MAT" && currentState == (int)StatiDematerializzazioneDocumenti.Visionato)
                        {
                            item = wkf[0]; //Prendo la prima posizione
                            break;
                        }
                    }
                }
                //************************************************************************************************************

                var wkfTipologia = db.XR_WKF_TIPOLOGIA.AsNoTracking().Where(k => k.ID_TIPOLOGIA.Equals(idTipologia)).Select(x => x.CONFIG_TABS).FirstOrDefault();
                
                if ((item == null) && (wkfTipologia == null || !wkfTipologia.Any()))
                {
                    throw new Exception("Errore! Workflow non trovato");
                }

                List<Dematerializzazione_Config_Tab> objD = JsonConvert.DeserializeObject<List<Dematerializzazione_Config_Tab>>(wkfTipologia);


                result = objD[item.ORDINE].NomeTab;
       

            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiDaPrendereInCarico(string mese = null, string utente = null, string sede = null, string tipologia = null, string statoRichiesta = null)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = new List<XR_DEM_DOCUMENTI_EXT>();
            bool almenoUnFiltroValorizzato = false;
            try
            {
                IncentiviEntities db = new IncentiviEntities();

                IQueryable<myRaiData.Incentivi.SINTESI1> SINTESI1 = db.SINTESI1.AsQueryable();

                if (String.IsNullOrEmpty(mese) && String.IsNullOrEmpty(utente) && String.IsNullOrEmpty(sede) &&
                    String.IsNullOrEmpty(tipologia) && String.IsNullOrEmpty(statoRichiesta))
                {
                    almenoUnFiltroValorizzato = false;
                }
                else
                {
                    almenoUnFiltroValorizzato = true;
                }

                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(CommonHelper.GetCurrentUserMatricola(), "DEMA");

                // se ho la funzione 01GEST - AMMINISTRAZIONE LETTURA/SCRITTURA
                // se ho la funzione 03GEST - UFFICIO PERSONALE LETTURA/SCRITTURA
                // aggiunto perchè 01ADM ha maggiorni visibilità rispetto allo 01GEST, quindi 01ADM dovrà vedere le cose di 01GEST
                if (subFunc.Contains("01GEST") || subFunc.Contains("03GEST") || subFunc.Contains("01ADM"))
                {
                    var items = (from d in db.XR_DEM_DOCUMENTI
                                 join w in db.XR_WKF_WORKFLOW
                                 on d.Id_WKF_Tipologia equals w.ID_TIPOLOGIA
                                 where d.PraticaAttiva && (w.DESTINATARIO == "01GEST" || w.DESTINATARIO == "03GEST")
                                 select d).Distinct().ToList();

                    if (items != null && items.Any())
                    {
                        if (almenoUnFiltroValorizzato)
                        {
                            IQueryable<XR_DEM_DOCUMENTI> query = items.AsQueryable();

                            if (!String.IsNullOrEmpty(mese))
                            {
                                int meseSelezionato = 0;
                                mese = mese.Substring(0, 2);
                                bool converti = int.TryParse(mese, out meseSelezionato);

                                if (converti)
                                {
                                    query = XR_DEM_Documenti_Filter_Mese(query, meseSelezionato);
                                }
                                else
                                {
                                    throw new Exception("Errore, il mese non è corretto");
                                }
                            }

                            if (!String.IsNullOrEmpty(statoRichiesta))
                            {
                                int id_stato_filtro = 0;
                                bool converti = int.TryParse(statoRichiesta, out id_stato_filtro);

                                if (converti)
                                {
                                    query = query.Where(w => w.Id_Stato.Equals(id_stato_filtro));
                                }
                                else
                                {
                                    throw new Exception("Errore, stato pratica non riconosciuto");
                                }
                            }

                            if (!String.IsNullOrEmpty(tipologia))
                            {
                                int id_Tipo_Doc_filtro = 0;
                                bool converti = int.TryParse(tipologia, out id_Tipo_Doc_filtro);

                                if (converti)
                                {
                                    query = query.Where(w => w.Id_Tipo_Doc.Equals(id_Tipo_Doc_filtro));
                                }
                                else
                                {
                                    throw new Exception("Errore, tipologia documentale non riconosciuta");
                                }
                            }

                            if (!String.IsNullOrEmpty(utente))
                            {
                                utente = utente.Trim();
                                int n;
                                bool isNumeric = int.TryParse(utente, out n);
                                bool isNumeric2 = int.TryParse(utente.Substring(1), out n);

                                if (isNumeric)
                                {
                                    // se è stata inserita la matricola es: 103650
                                    query = query.Where(w => w.MatricolaDestinatario.Equals(utente));
                                }
                                else if (isNumeric2)
                                {
                                    // se è stata inserita la matricola es: P103650
                                    string matr = utente.Substring(1);
                                    query = query.Where(w => w.MatricolaDestinatario.Equals(matr));
                                }
                                else
                                {
                                    utente = utente.ToUpper();
                                    List<string> tempMat = new List<string>();
                                    // è un nominativo
                                    // recupera tutte le matricole da sintesi1, che rispondono al nominativo inserito
                                    tempMat.AddRange(db.SINTESI1.Where(w => w.DES_COGNOMEPERS.Contains(utente)).Select(w => w.COD_MATLIBROMAT).ToList());

                                    tempMat.AddRange(db.SINTESI1.Where(w => w.DES_NOMEPERS.Contains(utente)).Select(w => w.COD_MATLIBROMAT).ToList());

                                    string primaLettera = utente.Substring(0, 1);

                                    var _templist = db.SINTESI1.Where(w => w.DES_COGNOMEPERS.StartsWith(primaLettera) || w.DES_NOMEPERS.StartsWith(primaLettera)).Select(w => new { Matricola = w.COD_MATLIBROMAT, Nominativo_C_N = w.DES_COGNOMEPERS.Trim() + " " + w.DES_NOMEPERS.Trim(), Nominativo_N_C = w.DES_NOMEPERS.Trim() + " " + w.DES_COGNOMEPERS.Trim() }).ToList();

                                    tempMat.AddRange(_templist.Where(w => w.Nominativo_C_N.Contains(utente)).Select(w => w.Matricola).ToList());
                                    tempMat.AddRange(_templist.Where(w => w.Nominativo_N_C.Contains(utente)).Select(w => w.Matricola).ToList());

                                    tempMat = tempMat.DistinctBy(w => w).ToList();
                                    query = query.Where(w => tempMat.Contains(w.MatricolaDestinatario));
                                }
                            }

                            if (!String.IsNullOrEmpty(sede))
                            {
                                SINTESI1 = SINTESI1.Where(w => w.COD_SEDE.Equals(sede));
                            }

                            items = query.ToList();
                        }
                    }

                    if (items != null && items.Any())
                    {
                        string abilKey = "DEMA";
                        string currentMatr = CommonHelper.GetCurrentUserMatricola();
                        var tmpSint = AuthHelper.SintesiFilter(SINTESI1, currentMatr, null, abilKey);

                        List<string> matricoleDaFiltrare = new List<string>();
                        List<string> matricoleConsentite = new List<string>();

                        matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                        if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                        {
                            tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                            matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                        }

                        result = new List<XR_DEM_DOCUMENTI_EXT>();
                        foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                        {
                            int nextStatus = GetNextIdStato(i.Id_Stato, i.Id_WKF_Tipologia, true);
                            // se il prossimo stato è PRESA IN CARICO allora il documento deve essere visibile nella pagina dell'amministrazione
                            if (nextStatus == (int)StatiDematerializzazioneDocumenti.PresaInCarico ||
                                nextStatus == (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore)
                            {
                                XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                                result.Add(newItem);
                            }
                            else if (almenoUnFiltroValorizzato)
                            {
                                XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                                newItem.NascondiBottoniPrendiInCarico = true;
                                result.Add(newItem);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
            }
            if (result != null && result.Any())
            {
                result = result.DistinctBy(w => w.Id).ToList();
                result = result.OrderByDescending(w => w.Id).ToList();
            }
            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiInCaricoAMe(string matricola)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = new List<XR_DEM_DOCUMENTI_EXT>();

            try
            {
                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(CommonHelper.GetCurrentUserMatricola(), "DEMA");

                // se ho la funzione 01GEST - AMMINISTRAZIONE LETTURA/SCRITTURA
                // se ho la funzione 03GEST - UFFICIO PERSONALE LETTURA/SCRITTURA
                if (subFunc.Contains("01GEST") || subFunc.Contains("03GEST") || subFunc.Contains("01ADM"))
                {
                    var db = AnagraficaManager.GetDb();
                    //var items = db.XR_DEM_DOCUMENTI.Include( "XR_DEM_VERSIONI_DOCUMENTO" ).Where( w => w.PraticaAttiva && 
                    //w.Id_Stato == ( int ) StatiDematerializzazioneDocumenti.PresaInCarico &&
                    //w.MatricolaIncaricato == matricola &&
                    //((w.Id_WKF_Tipologia == (int)WKF_TIPOLOGIA_ENUM.DEMDOC_VSRUO) ||
                    //    (w.Id_WKF_Tipologia == (int)WKF_TIPOLOGIA_ENUM.DEMDOC_VSRUO_C) ||
                    //    (w.Id_WKF_Tipologia == (int)WKF_TIPOLOGIA_ENUM.DEMDOC_VSRUO_CON) ||
                    //    (w.Id_WKF_Tipologia == (int)WKF_TIPOLOGIA_ENUM.DEMDOC_VSRUO_ASP) ||
                    //    (w.Id_WKF_Tipologia == (int)WKF_TIPOLOGIA_ENUM.DEMDOC_VSRUO_MAT) ||
                    //    (w.Id_WKF_Tipologia == (int)WKF_TIPOLOGIA_ENUM.DEMDOC_VSRUO_PSC))).ToList();

                    var items = (from d in db.XR_DEM_DOCUMENTI
                                 join w in db.XR_WKF_WORKFLOW
                                 on d.Id_WKF_Tipologia equals w.ID_TIPOLOGIA
                                 where d.PraticaAttiva &&
                                 d.Id_Stato == (int)StatiDematerializzazioneDocumenti.PresaInCarico &&
                                 (w.DESTINATARIO == "01GEST" || w.DESTINATARIO == "03GEST") &&
                                 d.MatricolaIncaricato == matricola
                                 select d).Distinct().ToList();

                    if (items != null && items.Any())
                    {
                        string abilKey = "DEMA";
                        string currentMatr = CommonHelper.GetCurrentUserMatricola();
                        var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                        List<string> matricoleDaFiltrare = new List<string>();
                        List<string> matricoleConsentite = new List<string>();

                        matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                        if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                        {
                            tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                            matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                        }
                        result = new List<XR_DEM_DOCUMENTI_EXT>();
                        foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                        {
                            XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                            newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                            result.Add(newItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
            }
            if (result != null && result.Any())
            {
                result = result.DistinctBy(w => w.Id).ToList();
                result = result.OrderByDescending(w => w.Id).ToList();
            }
            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiInCaricoAltri(string matricola)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = new List<XR_DEM_DOCUMENTI_EXT>();

            try
            {
                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(CommonHelper.GetCurrentUserMatricola(), "DEMA");

                // se ho la funzione 01GEST - AMMINISTRAZIONE LETTURA/SCRITTURA
                // se ho la funzione 03GEST - UFFICIO PERSONALE LETTURA/SCRITTURA
                if (subFunc.Contains("01GEST") || subFunc.Contains("03GEST") || subFunc.Contains("01ADM"))
                {
                    var db = AnagraficaManager.GetDb();
                    //var items = db.XR_DEM_DOCUMENTI.Include( "XR_DEM_VERSIONI_DOCUMENTO" ).Where( w => w.PraticaAttiva &&
                    //w.Id_Stato == ( int ) StatiDematerializzazioneDocumenti.PresaInCarico &&
                    //w.MatricolaIncaricato != matricola &&
                    //((w.Id_WKF_Tipologia == (int)WKF_TIPOLOGIA_ENUM.DEMDOC_VSRUO) ||
                    //    (w.Id_WKF_Tipologia == (int)WKF_TIPOLOGIA_ENUM.DEMDOC_VSRUO_C) ||
                    //    (w.Id_WKF_Tipologia == (int)WKF_TIPOLOGIA_ENUM.DEMDOC_VSRUO_CON) ||
                    //    (w.Id_WKF_Tipologia == (int)WKF_TIPOLOGIA_ENUM.DEMDOC_VSRUO_ASP) ||
                    //    (w.Id_WKF_Tipologia == (int)WKF_TIPOLOGIA_ENUM.DEMDOC_VSRUO_MAT) ||
                    //    (w.Id_WKF_Tipologia == (int)WKF_TIPOLOGIA_ENUM.DEMDOC_VSRUO_PSC))).ToList();

                    var items = (from d in db.XR_DEM_DOCUMENTI
                                 join w in db.XR_WKF_WORKFLOW
                                 on d.Id_WKF_Tipologia equals w.ID_TIPOLOGIA
                                 where d.PraticaAttiva &&
                                 d.Id_Stato == (int)StatiDematerializzazioneDocumenti.PresaInCarico &&
                                 (w.DESTINATARIO == "01GEST" || w.DESTINATARIO == "03GEST") &&
                                 d.MatricolaIncaricato != matricola
                                 select d).Distinct().ToList();

                    if (items != null && items.Any())
                    {
                        string abilKey = "DEMA";
                        string currentMatr = CommonHelper.GetCurrentUserMatricola();
                        var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                        List<string> matricoleDaFiltrare = new List<string>();
                        List<string> matricoleConsentite = new List<string>();

                        matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                        if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                        {
                            tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                            matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                        }
                        result = new List<XR_DEM_DOCUMENTI_EXT>();
                        foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                        {
                            XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                            newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                            result.Add(newItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
            }
            if (result != null && result.Any())
            {
                result = result.DistinctBy(w => w.Id).ToList();
                result = result.OrderByDescending(w => w.Id).ToList();
            }
            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> BK_GetDocumentiDaVisionare(string nominativo = null, string matricola = null, string tipologiaDocumento = null, string statoRichiesta = null)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            try
            {
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                var db = AnagraficaManager.GetDb();

                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

                // se ho la funzione 01VIST - Sono Amministrazione: lettura/scrittura
                // VISUALIZZATORE/VISTATORE
                if (!subFunc.Contains("01VIST"))
                {
                    throw new Exception("Utente non autorizzato");
                }

                var items = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva && w.Id_Stato != (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore &&
                w.Id_Stato < (int)StatiDematerializzazioneDocumenti.Accettato &&
                w.Id_Stato != (int)StatiDematerializzazioneDocumenti.Visionato).ToList();

                if (items != null && items.Any())
                {
                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                    }

                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                    {
                        var TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(i.Id_Tipo_Doc)).FirstOrDefault();
                        string Codice_Tipo_Documento = "";
                        if (TIPI_DOCUMENTO != null)
                        {
                            Codice_Tipo_Documento = TIPI_DOCUMENTO.Codice;
                        }

                        var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(i.Cod_Tipologia_Documentale) &&
                        w.Codice_Tipo_Documento.Equals(Codice_Tipo_Documento)).FirstOrDefault();

                        if (comportamento == null)
                        {
                            throw new Exception("Comportamento non trovato");
                        }

                        if (!comportamento.Visionatore)
                        {
                            continue;
                        }

                        var r = AuthHelper.EnableToMatr(matricolaCorrente, i.MatricolaCreatore, "DEMA", "01VIST");
                        //if (!r.Enabled || r.Visibilita != AbilMatrLiv.VisibilitaEnum.Filtrata)
                        if (!r.Enabled)
                        {
                            continue;
                        }

                        string destinatario_Da_WKF = "";
                        int nextStatus = GetNextIdStato(i.Id_Stato, i.Id_WKF_Tipologia);
                        bool esito = GetDestinatario(i.Id_WKF_Tipologia, nextStatus, out destinatario_Da_WKF);

                        // se il prossimo stato è azione automatica allora va visto lo stato ancora successivo
                        // perchè lo stato di accettato o visionato potrebbe venir dopo ad una azione automatica
                        if (nextStatus == (int)StatiDematerializzazioneDocumenti.AzioneAutomatica || nextStatus == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaContabile)
                        {
                            nextStatus = GetNextIdStato(nextStatus, i.Id_WKF_Tipologia);
                            esito = GetDestinatario(i.Id_WKF_Tipologia, nextStatus, out destinatario_Da_WKF);
                        }

                        // se il prossimo stato è ACCETTATO e quel tipo documento ha VISIONATORE == 1 allora 
                        // il documento deve essere visionato e va visualizzato nella pagina
                        if (nextStatus == (int)StatiDematerializzazioneDocumenti.Accettato &&
                            esito &&
                            (String.IsNullOrEmpty(destinatario_Da_WKF) ||
                            destinatario_Da_WKF.Contains("01GEST"))
                            )
                        {
                            XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                            newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                            result.Add(newItem);
                        }
                        else if (nextStatus == (int)StatiDematerializzazioneDocumenti.Visionato &&
                                esito &&
                                (String.IsNullOrEmpty(destinatario_Da_WKF) ||
                                destinatario_Da_WKF.Contains("01GEST"))
                                )
                        {
                            // se lo stato successivo è visionato ed il destinatario è 01GEST allora
                            // il documento va visualizzato nella pagina del visionatore
                            XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                            newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                            result.Add(newItem);
                        }
                    }
                }

                if (result != null && result.Any())
                {
                    result = result.DistinctBy(w => w.Id).ToList();

                    result = result.OrderByDescending(w => w.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiInBozza()
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            try
            {
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                var db = AnagraficaManager.GetDb();

                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

                // se ho la funzione 02GEST sono operatore
                if (!subFunc.Contains("02GEST"))
                {
                    throw new Exception("Utente non autorizzato");
                }

                var bozze = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza).ToList();

                if (bozze != null && bozze.Any())
                {
                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(bozze.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                    }
                    foreach (var i in bozze.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                    {
                        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                        result.Add(newItem);
                    }
                }

                if (result != null && result.Any())
                {
                    result = result.DistinctBy(w => w.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> BkGetDocumentiPerOperatori(string nominativo = null, string matricola = null, string tipologiaDocumento = null, string statoRichiesta = null)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            try
            {
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                var db = AnagraficaManager.GetDb();

                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

                // se ho la funzione 02GEST sono operatore
                if (!subFunc.Contains("02GEST"))
                {
                    throw new Exception("Utente non autorizzato");
                }

                var listaUnica = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva || w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza).ToList();

                if (listaUnica != null && listaUnica.Any())
                {
                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(listaUnica.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                    }
                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    foreach (var i in listaUnica.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                    {
                        if (i.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza)
                        {
                            XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                            newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                            result.Add(newItem);
                        }
                        else
                        {
                            string destinatario_Da_WKF = "";
                            bool esito = false;
                            int nextStatus = GetNextIdStato(i.Id_Stato, i.Id_WKF_Tipologia);
                            esito = GetDestinatario(i.Id_WKF_Tipologia, i.Id_Stato, out destinatario_Da_WKF);

                            // poichè il visionato non è presente nel workflow, 
                            // per calcolare il prossimo stato considera lo stato 
                            // appena successivo alla creazione del documento quindi lo stato 20
                            // e non lo stato 30 nel quale si trova il documento
                            if (i.Id_Stato == (int)StatiDematerializzazioneDocumenti.Visionato)
                            {
                                nextStatus = GetNextIdStato((int)StatiDematerializzazioneDocumenti.ProntoVisione, i.Id_WKF_Tipologia);
                                esito = GetDestinatario(i.Id_WKF_Tipologia, (int)StatiDematerializzazioneDocumenti.ProntoVisione, out destinatario_Da_WKF);
                            }

                            XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                            newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                            result.Add(newItem);
                        }
                    }

                }

                //if (items != null && items.Any( ))
                //{
                //    string abilKey = "DEMA";
                //    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                //    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                //    List<string> matricoleDaFiltrare = new List<string>();
                //    List<string> matricoleConsentite = new List<string>();

                //    matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                //    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                //    {
                //        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                //        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                //    }
                //    result = new List<XR_DEM_DOCUMENTI_EXT>( );
                //    foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                //    {
                //        string destinatario_Da_WKF = "";
                //        bool esito = false;
                //        int nextStatus = GetNextIdStato(i.Id_Stato , i.Id_WKF_Tipologia);
                //        esito = GetDestinatario(i.Id_WKF_Tipologia , i.Id_Stato , out destinatario_Da_WKF);

                //        // poichè il visionato non è presente nel workflow, 
                //        // per calcolare il prossimo stato considera lo stato 
                //        // appena successivo alla creazione del documento quindi lo stato 20
                //        // e non lo stato 30 nel quale si trova il documento
                //        if (i.Id_Stato == ( int ) StatiDematerializzazioneDocumenti.Visionato)
                //        {
                //            nextStatus = GetNextIdStato(( int ) StatiDematerializzazioneDocumenti.ProntoVisione , i.Id_WKF_Tipologia);
                //            esito = GetDestinatario(i.Id_WKF_Tipologia , ( int ) StatiDematerializzazioneDocumenti.ProntoVisione , out destinatario_Da_WKF);
                //        }

                //        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT( );
                //        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                //        result.Add(newItem);
                //    }
                //}

                //if (bozze != null && bozze.Any())
                //{
                //    if (result == null)
                //    {
                //        result = new List<XR_DEM_DOCUMENTI_EXT>();
                //    }
                //    string abilKey = "DEMA";
                //    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                //    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                //    List<string> matricoleDaFiltrare = new List<string>();
                //    List<string> matricoleConsentite = new List<string>();

                //    matricoleDaFiltrare.AddRange(bozze.Select(x => x.MatricolaDestinatario).ToList());

                //    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                //    {
                //        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                //        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                //    }
                //    foreach (var i in bozze.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                //    {
                //        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                //        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                //        result.Add(newItem);
                //    }
                //}

                if (result != null && result.Any())
                {
                    result = result.DistinctBy(w => w.Id).ToList();

                    result = result.OrderByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoFirma)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza)
                        .ThenByDescending(w => w.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static object GetDestinatario(string filter, string value)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = AnagraficaManager.GetDb();

            string abilKey = "DEMA";
            string currentMatr = CommonHelper.GetCurrentUserMatricola();
            //var enabledCat = AuthHelper.EnabledCategory(currentMatr , abilKey);
            //var enabledSer = AuthHelper.EnabledDirection(currentMatr , abilKey);

            DateTime oggi = DateTime.Now;

            var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.Where(x => x.COD_MATLIBROMAT != null && x.DTA_FINE_CR != null && x.DTA_FINE_CR.Value >= DateTime.Today), currentMatr, null, abilKey);
            //if (enabledCat.HasFilter)
            //    tmpSint = tmpSint.Where(x => (enabledCat.CategorieIncluse.Count( ) == 0 || enabledCat.CategorieIncluse.Any(y => x.COD_QUALIFICA.StartsWith(y)))
            //                                && (enabledCat.CategorieEscluse.Count( ) == 0 || !enabledCat.CategorieEscluse.Any(y => x.COD_QUALIFICA.StartsWith(y))));

            //if (enabledSer.HasFilter)
            //    tmpSint = tmpSint.Where(x => enabledSer.DirezioniIncluse.Contains(x.COD_SERVIZIO));

            tmpSint = tmpSint.OrderBy(x => x.COD_MATLIBROMAT);

            var tmp = tmpSint.Select(s => new CercaDipendentiItem()
            {
                MATRICOLA = s.COD_MATLIBROMAT,
                NOME = s.DES_NOMEPERS,
                COGNOME = s.DES_COGNOMEPERS,
                SECONDO_COGNOME = s.DES_SECCOGNOME,
                DATA_ASSUNZIONE = s.DTA_INIZIO_CR,
                CONTRATTO = s.DES_TPCNTR,
                ID_PERSONA = s.ID_PERSONA,
                COD_SEDE = s.COD_SEDE,
                DES_SEDE = s.DES_SEDE,
                COD_SERVIZIO = s.COD_SERVIZIO,
                DES_SERVIZIO = s.DES_SERVIZIO,
                SmartWorker = false
            });

            if (tmp != null && tmp.Any())
            {
                foreach (var t in tmp)
                {
                    t.NOME = CommonHelper.ToTitleCase(t.NOME);
                    t.COGNOME = CommonHelper.ToTitleCase(t.COGNOME);
                    t.SECONDO_COGNOME = CommonHelper.ToTitleCase(t.SECONDO_COGNOME);
                }
            }

            if (!string.IsNullOrEmpty(filter))
            {
                var _filteredList = tmp.Where(w => !String.IsNullOrEmpty(w.MATRICOLA) && w.MATRICOLA.Contains(filter)).ToList();



                result.AddRange(_filteredList.ToList().Select(x => new SelectListItem { Value = x.MATRICOLA, Text = "<div class='rai-profile-widget'><div class='rai-profile-info'><span class='rai-font-md-bold'>" + x.MATRICOLA + " - " + x.COGNOME + " " + x.NOME + "</span><br><span class='rai-font-sm'>" + x.DES_SERVIZIO + "</span></div></div>" }));
            }

            if (!string.IsNullOrEmpty(filter))
            {
                var _filteredList = tmp.Where(w => (w.NOME + " " + w.COGNOME).ToUpper().Contains(filter.ToUpper())
                                        || (w.COGNOME + " " + w.NOME).ToUpper().Contains(filter.ToUpper())).ToList();

                result.AddRange(_filteredList.ToList().Select(x => new SelectListItem { Value = x.MATRICOLA, Text = "<div class='rai-profile-widget'><div class='rai-profile-info'><span class='rai-font-md-bold'>" + x.MATRICOLA + " - " + x.COGNOME + " " + x.NOME + "</span><br><span class='rai-font-sm'>" + x.DES_SERVIZIO + "</span></div></div>" }));
            }
            return result;
        }

        public static bool IsSegreteria()
        {
            bool result = true;
            string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
            var db = AnagraficaManager.GetDb();

            // se sono autorizzato
            var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

            if (!subFunc.Contains("01SEGR"))
            {
                result = false;
            }

            return result;
        }

        public static bool IsVisionatore()
        {
            bool result = true;
            string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
            var db = AnagraficaManager.GetDb();

            // se sono autorizzato
            var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

            // se ho la funzione 01GEST - Sono Amministrazione: lettura/scrittura
            // VISUALIZZATORE
            //if (!subFunc.Contains("01GEST"))
            //{
            //    result = false;
            //}

            if (!subFunc.Contains("01VIST"))
            {
                result = false;
            }

            return result;
        }

        public static bool IsApprovatore()
        {
            bool result = true;
            string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
            var db = AnagraficaManager.GetDb();

            // se sono autorizzato
            var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

            if (!subFunc.Contains("01ADM") && !subFunc.Contains("01APPR"))
            {
                result = false;
            }

            return result;
        }

        public static bool IsOperatore()
        {
            bool result = true;
            string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
            var db = AnagraficaManager.GetDb();

            // se sono autorizzato
            var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

            if (!subFunc.Contains("02GEST"))
            {
                result = false;
            }

            return result;
        }

        public static List<TemplateModel> GetTemplateModels()
        {
            List<TemplateModel> result = null;

            try
            {
                string matricola = UtenteHelper.Matricola();
                var db = AnagraficaManager.GetDb();

                var items = db.XR_TEMPLATES.Where(w => w.AreaUtilizzo == "DEMA").Select(w => new TemplateModel() { Id = w.Id, Name = w.NomeFile }).ToList();
                
                if (items != null && items.Any())
                {
                    result = new List<TemplateModel>();
                    result.AddRange(items.ToList());
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Reperimento dei documenti di cui sono il destinatario e che
        /// risultano in stato "Inviato al dipendente"
        /// </summary>
        /// <returns></returns>
        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiInviatiAMe(string matricola,
            DateTime? datadal = null, int tipologia = -1)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                if (String.IsNullOrEmpty(matricola))
                {
                    matricola = CommonManager.GetCurrentUserMatricola();
                }
                List<XR_DEM_DOCUMENTI> items = new List<XR_DEM_DOCUMENTI>();

                if (!datadal.HasValue && tipologia == -1)
                {
                    items = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.PraticaAttiva &&
                 w.Id_Stato == (int)StatiDematerializzazioneDocumenti.InviatoAlDipendente &&
                 w.MatricolaDestinatario.Equals(matricola)).ToList();
                }
                else if (datadal.HasValue && tipologia != -1)
                {
                    items = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.PraticaAttiva &&
                     w.Id_Stato == (int)StatiDematerializzazioneDocumenti.InviatoAlDipendente &&
                     w.MatricolaDestinatario.Equals(matricola) &&
                     w.Id_Tipo_Doc.Equals(tipologia) &&
                     EntityFunctions.TruncateTime(w.DataCreazione) == EntityFunctions.TruncateTime(datadal.Value)).ToList();
                }
                else if (datadal.HasValue && tipologia == -1)
                {
                    items = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.PraticaAttiva &&
                     w.Id_Stato == (int)StatiDematerializzazioneDocumenti.InviatoAlDipendente &&
                     w.MatricolaDestinatario.Equals(matricola) &&
                     EntityFunctions.TruncateTime(w.DataCreazione) == EntityFunctions.TruncateTime(datadal.Value)).ToList();
                }
                else if (!datadal.HasValue && tipologia != -1)
                {
                    items = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.PraticaAttiva &&
                     w.Id_Stato == (int)StatiDematerializzazioneDocumenti.InviatoAlDipendente &&
                     w.MatricolaDestinatario.Equals(matricola) &&
                     w.Id_Tipo_Doc.Equals(tipologia)).ToList();
                }

                if (items != null && items.Any())
                {
                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    foreach (var i in items)
                    {
                        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);

                        List<XR_ALLEGATI> allegati = new List<XR_ALLEGATI>();
                        allegati = GetAllegati(i.Id);
                        if (allegati != null && allegati.Any())
                        {
                            newItem.AllegatoPrincipale = new XR_ALLEGATI();
                            newItem.AllegatoPrincipale = allegati.FirstOrDefault(w => w.IsPrincipal);
                        }

                        result.Add(newItem);
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static XR_ALLEGATI GetAllegato(int idAllegato)
        {
            XR_ALLEGATI allegato = new XR_ALLEGATI();
            try
            {
                IncentiviEntities db = new IncentiviEntities();

                allegato = db.XR_ALLEGATI.Where(w => w.Id.Equals(idAllegato)).FirstOrDefault();

                if (allegato == null)
                {
                    throw new Exception("Errore impossibile reperire il file");
                }
            }
            catch (Exception ex)
            {
                allegato = null;
            }

            return allegato;
        }

        private static List<XR_ALLEGATI> GetAllegati(int idDocument)
        {
            List<XR_ALLEGATI> allegati = new List<XR_ALLEGATI>();
            try
            {
                IncentiviEntities db = new IncentiviEntities();
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDocument)).FirstOrDefault();

                if (item != null)
                {
                    var versioni = item.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id_Documento.Equals(item.Id) && !w.Deleted).OrderByDescending(w => w.Id).ThenByDescending(x => x.N_Versione).ToList();

                    if (versioni != null && versioni.Any())
                    {
                        List<int> idVersioni = new List<int>();
                        idVersioni.AddRange(versioni.Select(w => w.Id).ToList());

                        // prende tutti gli idversione e prende tutti gli idallegati associati agli idversione
                        var AllVers = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => idVersioni.Contains(w.IdVersione)).OrderByDescending(w => w.IdAllegato).ToList();

                        if (AllVers != null && AllVers.Any())
                        {
                            List<int> idsAllegati = new List<int>();
                            idsAllegati.AddRange(AllVers.Select(w => w.IdAllegato).ToList());
                            if (idsAllegati != null && idsAllegati.Any())
                            {
                                var all = db.XR_ALLEGATI.Where(w => idsAllegati.Contains(w.Id)).OrderByDescending(w => w.Id).ToList();
                                if (all != null && all.Any())
                                {
                                    // presi tutti gli allegati associati al documento
                                    // bisogna scartare quelli che hanno più versioni e
                                    // va presa solo la versione più recente
                                    // purtroppo per i vecchi documenti per vedere se c'è 
                                    // una versione differente dello stesso file, bisogna
                                    // controllare il nome file e idallegato
                                    // stesso nome file idallegato maggiore sarà la versione più recente
                                    // dell'allegato
                                    foreach (var a in all)
                                    {
                                        // prende l'allegato con lo stesso nome ma con id più grande
                                        int idMax = all.Where(w => w.NomeFile == a.NomeFile).Max(x => x.Id);

                                        // se già presente nell'elenco allora l'elemento è stato già analizzato
                                        bool giaEsiste = allegati.Count(w => w.Id == idMax) == 1;

                                        // se non esiste lo aggiunge all'elenco finale
                                        if (!giaEsiste)
                                        {
                                            var inserire = allegati.Where(w => w.Id == idMax).FirstOrDefault();
                                            allegati.Add(inserire);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }

                //if (item != null)
                //{
                //    allegati = new List<XR_ALLEGATI>();

                //    var versioni = item.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id_Documento.Equals(item.Id) && !w.Deleted).OrderByDescending(w => w.Id).ThenByDescending(x => x.N_Versione).ToList();

                //    if (versioni != null && versioni.Any())
                //    {
                //        foreach (var v in versioni)
                //        {
                //            var AllVers = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => w.IdVersione == v.Id).FirstOrDefault();

                //            if (AllVers != null)
                //            {
                //                var allegato = db.XR_ALLEGATI.Where(w => w.Id.Equals(AllVers.IdAllegato)).FirstOrDefault();

                //                if (allegato != null)
                //                {
                //                    allegati.Add(allegato);
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                allegati = null;
            }

            return allegati;
        }

        public static XR_ALLEGATI GetAllegatoPrincipale(int idDocument)
        {
            XR_ALLEGATI allegato = null;
            try
            {
                IncentiviEntities db = new IncentiviEntities();
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDocument)).FirstOrDefault();

                if (item != null)
                {
                    var versioni = item.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id_Documento.Equals(item.Id) && !w.Deleted).OrderByDescending(w => w.Id).ThenByDescending(x => x.N_Versione).ToList();

                    if (versioni != null && versioni.Any())
                    {
                        List<int> idVersioni = new List<int>();
                        idVersioni.AddRange(versioni.Select(w => w.Id).ToList());

                        // prende tutti gli idversione e prende tutti gli idallegati associati agli idversione
                        var AllVers = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => idVersioni.Contains(w.IdVersione)).OrderByDescending(w => w.IdAllegato).ToList();

                        if (AllVers != null && AllVers.Any())
                        {
                            List<int> idsAllegati = new List<int>();
                            idsAllegati.AddRange(AllVers.Select(w => w.IdAllegato).ToList());
                            if (idsAllegati != null && idsAllegati.Any())
                            {
                                var allegati = db.XR_ALLEGATI.Where(w => idsAllegati.Contains(w.Id) && w.IsPrincipal).OrderByDescending(w => w.Id).ToList();
                                if (allegati != null && allegati.Any())
                                {
                                    // presi tutti gli allegati associati al documento
                                    // bisogna scartare quelli che hanno più versioni e
                                    // va presa solo la versione più recente
                                    // purtroppo per i vecchi documenti per vedere se c'è 
                                    // una versione differente dello stesso file, bisogna
                                    // controllare il nome file e idallegato
                                    // stesso nome file idallegato maggiore sarà la versione più recente
                                    // dell'allegato
                                    foreach (var a in allegati)
                                    {
                                        // prende l'allegato con lo stesso nome ma con id più grande
                                        int idMax = allegati.Where(w => w.NomeFile == a.NomeFile).Max(x => x.Id);

                                        // se già presente nell'elenco allora l'elemento è stato già analizzato
                                        bool giaEsiste = allegato.Id == idMax;

                                        // se non esiste lo aggiunge all'elenco finale
                                        if (!giaEsiste)
                                        {
                                            var inserire = allegati.Where(w => w.Id == idMax).FirstOrDefault();
                                            allegato = inserire;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //if (item != null)
                //{
                //    allegato = new XR_ALLEGATI();

                //    var versioni = item.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id_Documento.Equals(item.Id)).OrderByDescending(w => w.Id).ToList();

                //    if (versioni != null && versioni.Any())
                //    {
                //        foreach (var v in versioni)
                //        {
                //            var AllVers = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => w.IdVersione == v.Id).FirstOrDefault();

                //            if (AllVers != null)
                //            {
                //                var al = db.XR_ALLEGATI.Where(w => w.Id.Equals(AllVers.IdAllegato) && w.IsPrincipal).FirstOrDefault();

                //                if (al != null)
                //                {
                //                    allegato = al;
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                allegato = null;
            }

            return allegato;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiDaLavorare(string matricola,
    DateTime? datadal = null, int tipologia = -1, string nominativo = null)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = new List<XR_DEM_DOCUMENTI_EXT>();
            string matricolaCorrente = CommonManager.GetCurrentUserMatricola();

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                if (String.IsNullOrEmpty(matricola))
                {
                    matricola = CommonManager.GetCurrentUserMatricola();
                }
                List<XR_DEM_DOCUMENTI> items = new List<XR_DEM_DOCUMENTI>();

                var query = from documents in db.XR_DEM_DOCUMENTI
                            where documents.PraticaAttiva &&
                            documents.Id_Stato == (int)StatiDematerializzazioneDocumenti.Accettato
                            &&
                            (documents.MatricolaFirma != null && documents.MatricolaFirma == matricolaCorrente)
                            select documents;

                if (datadal.HasValue)
                {
                    query = query.Where(w => EntityFunctions.TruncateTime(w.DataCreazione) == EntityFunctions.TruncateTime(datadal.Value));
                }

                if (tipologia != -1)
                {
                    query = query.Where(w => w.Id_Tipo_Doc.Equals(tipologia));
                }

                items = query.ToList();

                if (items != null && items.Any())
                {
                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                    }
                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                    {
                        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);

                        List<XR_ALLEGATI> allegati = new List<XR_ALLEGATI>();
                        allegati = GetAllegati(i.Id);
                        if (allegati != null && allegati.Any())
                        {
                            newItem.AllegatoPrincipale = new XR_ALLEGATI();
                            newItem.AllegatoPrincipale = allegati.FirstOrDefault(w => w.IsPrincipal);
                        }

                        result.Add(newItem);
                    }
                }

                if (result != null && result.Any())
                {
                    var docInFirma = GetDocumentiInFirma(matricola);

                    if (docInFirma != null && docInFirma.Any())
                    {
                        List<int> ids = new List<int>();
                        ids.AddRange(docInFirma.Select(w => w.Id).ToList());
                        result = result.Where(w => !ids.Contains(w.Id)).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                result = new List<XR_DEM_DOCUMENTI_EXT>();
            }

            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiRifiutati(DEM_TIPO_RIFIUTATI filtro = DEM_TIPO_RIFIUTATI.ALL)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            try
            {
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                var db = AnagraficaManager.GetDb();

                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

                // se ho la funzione 02GEST sono operatore
                if (!subFunc.Contains("02GEST"))
                {
                    throw new Exception("Utente non autorizzato");
                }

                List<XR_DEM_DOCUMENTI> items = new List<XR_DEM_DOCUMENTI>();

                if (filtro == DEM_TIPO_RIFIUTATI.ALL)
                {
                    items = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva &&
                        (w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore ||
                            w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoFirma ||
                            w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente)
                    ).ToList();
                }
                else if (filtro == DEM_TIPO_RIFIUTATI.RifiutateInApprovazione)
                {
                    items = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva && w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore).ToList();
                }
                else if (filtro == DEM_TIPO_RIFIUTATI.RifiutateInFirma)
                {
                    items = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva && w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoFirma).ToList();
                }
                else if (filtro == DEM_TIPO_RIFIUTATI.RifiutateDalDipendente)
                {
                    items = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva && w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente).ToList();
                }

                if (items != null && items.Any())
                {
                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                    }
                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                    {
                        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                        result.Add(newItem);
                    }
                }

                if (result != null && result.Any())
                {
                    result = result.DistinctBy(w => w.Id).ToList();
                    result = result.OrderByDescending(w => w.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiRifiutatiInFirma(string matricola,
    DateTime? datadal = null, int tipologia = -1, string nominativo = null)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = new List<XR_DEM_DOCUMENTI_EXT>();
            string matricolaCorrente = CommonManager.GetCurrentUserMatricola();

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                if (String.IsNullOrEmpty(matricola))
                {
                    matricola = CommonManager.GetCurrentUserMatricola();
                }
                List<XR_DEM_DOCUMENTI> items = new List<XR_DEM_DOCUMENTI>();

                var query = from documents in db.XR_DEM_DOCUMENTI
                            where documents.PraticaAttiva &&
                            documents.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoFirma
                            &&
                            (documents.MatricolaFirma != null && documents.MatricolaFirma == matricolaCorrente)
                            select documents;

                if (datadal.HasValue)
                {
                    query = query.Where(w => EntityFunctions.TruncateTime(w.DataCreazione) == EntityFunctions.TruncateTime(datadal.Value));
                }

                if (tipologia != -1)
                {
                    query = query.Where(w => w.Id_Tipo_Doc.Equals(tipologia));
                }

                items = query.ToList();

                if (items != null && items.Any())
                {
                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                    }
                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                    {
                        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);

                        List<XR_ALLEGATI> allegati = new List<XR_ALLEGATI>();
                        allegati = GetAllegati(i.Id);
                        if (allegati != null && allegati.Any())
                        {
                            newItem.AllegatoPrincipale = new XR_ALLEGATI();
                            newItem.AllegatoPrincipale = allegati.FirstOrDefault(w => w.IsPrincipal);
                        }

                        result.Add(newItem);
                    }
                }
            }
            catch (Exception ex)
            {
                result = new List<XR_DEM_DOCUMENTI_EXT>();
            }

            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiInFirma(string matricola)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = new List<XR_DEM_DOCUMENTI_EXT>();
            string matricolaCorrente = CommonManager.GetCurrentUserMatricola();

            try
            {
                IncentiviEntities dbIncentivi = new IncentiviEntities();

                using (digiGappEntities db = new digiGappEntities())
                {
                    var items = db.MyRai_CarrelloGenerico.Where(w => w.tabella.Equals("XR_DEM_DOCUMENTI") && w.matricola.Equals(matricola)).ToList();

                    if (items != null && items.Any())
                    {
                        List<int> ids = new List<int>();
                        ids.AddRange(items.Select(w => w.id_documento).ToList());

                        var query = from documents in dbIncentivi.XR_DEM_DOCUMENTI
                                    where documents.PraticaAttiva &&
                                    ids.Contains(documents.Id)
                                    select documents;

                        var docs = query.ToList();

                        if (docs != null && docs.Any())
                        {
                            string abilKey = "DEMA";
                            string currentMatr = CommonHelper.GetCurrentUserMatricola();
                            var tmpSint = AuthHelper.SintesiFilter(dbIncentivi.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                            List<string> matricoleDaFiltrare = new List<string>();
                            List<string> matricoleConsentite = new List<string>();

                            matricoleDaFiltrare.AddRange(docs.Select(x => x.MatricolaDestinatario).ToList());

                            if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                            {
                                tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                                matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                            }
                            result = new List<XR_DEM_DOCUMENTI_EXT>();
                            foreach (var i in docs.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                            {
                                XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);

                                List<XR_ALLEGATI> allegati = new List<XR_ALLEGATI>();
                                allegati = GetAllegati(i.Id);
                                if (allegati != null && allegati.Any())
                                {
                                    newItem.AllegatoPrincipale = new XR_ALLEGATI();
                                    newItem.AllegatoPrincipale = allegati.FirstOrDefault(w => w.IsPrincipal);
                                }

                                result.Add(newItem);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                result = new List<XR_DEM_DOCUMENTI_EXT>();
            }

            return result;
        }

        public static object GetCodiceSedeGappDaHRDW(string filter, string value)
        {
            if (String.IsNullOrEmpty(filter) && !String.IsNullOrEmpty(value))
            {
                filter = value;
            }

            List<SelectListItem> result = new List<SelectListItem>();
            var db = AnagraficaManager.GetDb();
            if (filter.Length == 0)
            {
                return result;
            }
            var items = db.SEDE.Where(w => w.COD_SEDE.StartsWith(filter)).ToList();

            if (items != null && items.Any())
            {
                result.AddRange(items.Select(x => new SelectListItem { Value = x.COD_SEDE, Text = x.DES_SEDE }));
            }
            return result;
        }

        public static object GetCategoriaDaHRDW(string filter, string value)
        {
            if (String.IsNullOrEmpty(filter) && !String.IsNullOrEmpty(value))
            {
                filter = value;
            }

            List<SelectListItem> result = new List<SelectListItem>();

            if (filter.Length == 0)
            {
                return result;
            }

            using (var sediDB = new PERSEOEntities())
            {
                string queryDIP = " select categoria + ' / ' + tipo_minimo + ' ' + desc_livello as Text, " +
                    " categoria + ' / ' + tipo_minimo + ' ' + desc_livello as Value " +
                    " from[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] " +
                    " where categoria LIKE '%##CAT##%' " +
                    " and flg_ultimo = '$' " +
                    " order by ordine ";

                queryDIP = queryDIP.Replace("##CAT##", filter);

                result = sediDB.Database.SqlQuery<SelectListItem>(queryDIP).ToList();
            }

            return result;
        }

        //public static object GetCodiceServizioDaHRDW(string filter, string value)
        //{
        //    if (String.IsNullOrEmpty(filter) && !String.IsNullOrEmpty(value))
        //    {
        //        filter = value;
        //    }

        //    List<SelectListItem> tmpResult = new List<SelectListItem>();
        //    List<SelectListItem> result = new List<SelectListItem>();

        //    if (filter.Length == 0)
        //    {
        //        return result;
        //    }

        //    using (var sediDB = new IncentiviEntities())
        //    {
        //        string queryDIP = "  SELECT [cod_serv_cont] as Text " +
        //                            " ,[desc_breve] as Value " +
        //                            " FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SERVIZIO_CONTABILE] " +
        //                            " where[cod_serv_cont] LIKE '%##COD##%'";

        //        string f = "";
        //        if (filter.Length > 2)
        //        {
        //            f = filter.Substring(0, 2);
        //        }
        //        else
        //        {
        //            f = filter;
        //        }
        //        queryDIP = queryDIP.Replace("##COD##", f);

        //        tmpResult = sediDB.Database.SqlQuery<SelectListItem>(queryDIP).ToList();
        //    }

        //    if (tmpResult != null && tmpResult.Any())
        //    {
        //        foreach(var r in tmpResult)
        //        {
        //            result.Add(new SelectListItem()
        //            {
        //                Text = tx,
        //                Value = r.Text + i.ToString(),
        //                Selected = false
        //            });
        //            //for(int i = 1; i<= 7; i++)
        //            //{
        //            //    if (i == 5) continue;
        //            //    string tx = r.Text + i.ToString() + " " + r.Value;
        //            //    //string tx = r.Text + i.ToString();

        //            //    result.Add(new SelectListItem()
        //            //    {
        //            //        Text = tx,
        //            //        Value = r.Text + i.ToString(),
        //            //        Selected = false
        //            //    });
        //            //}
        //        }

        //        result = result.Where(w => w.Value.StartsWith(filter)).ToList();
        //    }

        //    return result;
        //}

        public static object GetCodiceSezioneDaHRDW(string filter, string value)
        {
            if (String.IsNullOrEmpty(filter) && !String.IsNullOrEmpty(value))
            {
                filter = value;
            }

            List<SelectListItem> result = new List<SelectListItem>();

            if (filter.Length == 0)
            {
                return result;
            }

            using (var sediDB = new PERSEOEntities())
            {
                string queryDIP =
                    "SELECT [cod_sezione] as Value " +
                    ",[cod_sezione] +' - ' + [des_sezione] as Text " +
                    "FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SEZIONE] " +
                    "WHERE cod_sezione LIKE '%##COD##%' ";

                queryDIP = queryDIP.Replace("##COD##", filter);

                result = sediDB.Database.SqlQuery<SelectListItem>(queryDIP).ToList();
            }

            return result;
        }

        public static object GetMansioneDaHRDW(string filter, string value)
        {
            if (String.IsNullOrEmpty(filter) && !String.IsNullOrEmpty(value))
            {
                filter = value;
            }

            List<SelectListItem> result = new List<SelectListItem>();
            if (filter.Length == 0)
            {
                return result;
            }

            using (var sediDB = new PERSEOEntities())
            {
                string queryDIP =
                    "SELECT [cod_mansione] as Value " +
                    ",[cod_mansione] +' - ' + [desc_mansione] as Text " +
                    "FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_MANSIONE] " +
                    "WHERE cod_mansione LIKE '%##COD##%' ";

                queryDIP = queryDIP.Replace("##COD##", filter);
                result = sediDB.Database.SqlQuery<SelectListItem>(queryDIP).ToList();
            }

            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiDaApprovare(string nominativo = null, string matricola = null, string id_Tipo_Doc = null, string statoRichiesta = null)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            IQueryable<XR_DEM_DOCUMENTI> _tempItems = null;
            List<XR_DEM_DOCUMENTI> items = null;
            bool almenoUnFiltro = false;
            try
            {
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                IncentiviEntities db = new IncentiviEntities();

                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

                // se ho la funzione 01ADM - Sono approvatore
                if (!subFunc.Contains("01ADM") && !subFunc.Contains("01APPR"))
                {
                    throw new Exception("Utente non autorizzato");
                }

                // prima carica le tipologie per le quali la matricola ha visibilità
                List<string> TIPOLOGIE_DOCUMENTALI_VISIBILI = new List<string>();

                var _tempTip = db.XR_DEM_TIPOLOGIE_DOCUMENTALI.Where(w => w.Attivo && (
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                )).ToList();

                if (_tempTip != null && _tempTip.Any())
                {
                    TIPOLOGIE_DOCUMENTALI_VISIBILI.AddRange(_tempTip.Select(w => w.Codice).ToList());
                }
                else
                {
                    throw new Exception("L'Utente non ha visibilità su alcuna tipologia");
                }

                List<string> TIPI_DOCUMENTO_VISIBILI = new List<string>();
                List<int> ID_TIPO_DOC_VISIBILI = new List<int>();

                // reperisco tutti i XR_DEM_TIPIDOC_COMPORTAMENTO visibili alla matricola corrente
                var _tempTIPIDOC_COMPORTAMENTO = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => (
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                )).ToList();

                if (_tempTIPIDOC_COMPORTAMENTO != null && _tempTIPIDOC_COMPORTAMENTO.Any())
                {
                    TIPI_DOCUMENTO_VISIBILI.AddRange(_tempTIPIDOC_COMPORTAMENTO.Select(w => w.Codice_Tipo_Documento).ToList());

                    // a questo punto vanno recuperati gli dei tipi doc visibili
                    ID_TIPO_DOC_VISIBILI = db.XR_DEM_TIPI_DOCUMENTO.Where(w => TIPI_DOCUMENTO_VISIBILI.Contains(w.Codice)).Select(w => w.Id).ToList();
                }
                else
                {
                    throw new Exception("L'Utente non ha visibilità su alcuna tipologia");
                }

                _tempItems = db.XR_DEM_DOCUMENTI.Where(w => TIPOLOGIE_DOCUMENTALI_VISIBILI.Contains(w.Cod_Tipologia_Documentale) &&
                                        ID_TIPO_DOC_VISIBILI.Contains(w.Id_Tipo_Doc));

                if (!String.IsNullOrEmpty(nominativo) ||
                    !String.IsNullOrEmpty(matricola) ||
                    !String.IsNullOrEmpty(id_Tipo_Doc) ||
                    !String.IsNullOrEmpty(statoRichiesta))
                {
                    almenoUnFiltro = true;
                    if (!String.IsNullOrEmpty(matricola))
                    {
                        _tempItems = _tempItems.Where(w => w.MatricolaDestinatario.Equals(matricola));
                    }

                    if (!String.IsNullOrEmpty(statoRichiesta))
                    {
                        int id_stato_filtro = 0;
                        bool converti = int.TryParse(statoRichiesta, out id_stato_filtro);

                        if (converti)
                        {
                            _tempItems = _tempItems.Where(w => w.Id_Stato.Equals(id_stato_filtro));

                            if (id_stato_filtro == (int)StatiDematerializzazioneDocumenti.PraticaCancellata)
                            {
                                _tempItems = _tempItems.Where(w => !w.PraticaAttiva);
                            }
                            else if (id_stato_filtro != (int)StatiDematerializzazioneDocumenti.Bozza)
                            {
                                _tempItems = _tempItems.Where(w => w.PraticaAttiva);
                            }
                        }
                        else
                        {
                            throw new Exception("Errore, stato pratica non riconosciuto");
                        }
                    }
                    else
                    {
                        _tempItems = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva);
                        _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.RifiutoApprovatore));
                    }

                    if (!String.IsNullOrEmpty(id_Tipo_Doc))
                    {
                        int id_Tipo_Doc_filtro = 0;
                        bool converti = int.TryParse(id_Tipo_Doc, out id_Tipo_Doc_filtro);

                        if (converti)
                        {
                            _tempItems = _tempItems.Where(w => w.Id_Tipo_Doc.Equals(id_Tipo_Doc_filtro));
                        }
                        else
                        {
                            throw new Exception("Errore, tipologia documentale non riconosciuta");
                        }
                    }

                    if (!String.IsNullOrEmpty(nominativo))
                    {
                        List<string> matricoleDestinatario = new List<string>();
                        matricoleDestinatario.AddRange(_tempItems.Select(w => w.MatricolaDestinatario).ToList());
                        if (matricoleDestinatario != null && matricoleDestinatario.Any())
                        {
                            string abilKey = "DEMA";
                            string currentMatr = CommonHelper.GetCurrentUserMatricola();
                            DateTime oggi = DateTime.Now;
                            var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.Where(x => x.COD_MATLIBROMAT != null && x.DTA_FINE_CR != null && x.DTA_FINE_CR.Value >= DateTime.Today), currentMatr, null, abilKey);

                            tmpSint = tmpSint.OrderBy(x => x.COD_MATLIBROMAT);

                            var tmp = tmpSint.Select(s => new CercaDipendentiItem()
                            {
                                MATRICOLA = s.COD_MATLIBROMAT,
                                NOME = s.DES_NOMEPERS,
                                COGNOME = s.DES_COGNOMEPERS,
                                SECONDO_COGNOME = s.DES_SECCOGNOME,
                                DATA_ASSUNZIONE = s.DTA_INIZIO_CR,
                                CONTRATTO = s.DES_TPCNTR,
                                ID_PERSONA = s.ID_PERSONA,
                                COD_SEDE = s.COD_SEDE,
                                DES_SEDE = s.DES_SEDE,
                                COD_SERVIZIO = s.COD_SERVIZIO,
                                DES_SERVIZIO = s.DES_SERVIZIO,
                                SmartWorker = false
                            });

                            if (tmp != null && tmp.Any())
                            {
                                foreach (var t in tmp)
                                {
                                    t.NOME = CommonHelper.ToTitleCase(t.NOME);
                                    t.COGNOME = CommonHelper.ToTitleCase(t.COGNOME);
                                    t.SECONDO_COGNOME = CommonHelper.ToTitleCase(t.SECONDO_COGNOME);
                                }
                            }

                            var _filteredList = tmp.Where(w => (w.NOME + " " + w.COGNOME).ToUpper().Contains(nominativo.ToUpper())
                                                    || (w.COGNOME + " " + w.NOME).ToUpper().Contains(nominativo.ToUpper())).ToList();

                            List<string> _matricole = new List<string>();

                            if (_filteredList != null && _filteredList.Any())
                            {
                                _matricole.AddRange(_filteredList.Select(w => w.MATRICOLA).ToList());
                            }

                            _tempItems = _tempItems.Where(w => _matricole.Contains(w.MatricolaDestinatario));
                        }
                    }
                }
                else
                {
                    _tempItems = db.XR_DEM_DOCUMENTI.Where(w => (w.PraticaAttiva && w.Id_Stato != (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore) &&
                                        (TIPOLOGIE_DOCUMENTALI_VISIBILI.Contains(w.Cod_Tipologia_Documentale) &&
                                          ID_TIPO_DOC_VISIBILI.Contains(w.Id_Tipo_Doc)));
                }

                items = _tempItems.Where (x=>x.Id_Stato!=200).ToList();

                if (items != null && items.Any())
                {
                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                    }

                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                    {
                        var r = AuthHelper.EnableToMatr(matricolaCorrente, i.MatricolaCreatore, "DEMA", "01APPR");
                        if (!r.Enabled || r.Visibilita != AbilMatrLiv.VisibilitaEnum.Filtrata)
                        {
                            continue;
                        }

                        if (!almenoUnFiltro)
                        {
                            string destinatario_Da_WKF = "";
                            bool esito = false;
                            int nextStatus = GetNextIdStato(i.Id_Stato, i.Id_WKF_Tipologia);

                            // prende il workflow nello stato in cui si trova la pratica
                            if (i.Id_Stato == (int)StatiDematerializzazioneDocumenti.Visionato)
                            {
                                esito = GetDestinatario(i.Id_WKF_Tipologia, (int)StatiDematerializzazioneDocumenti.ProntoVisione, out destinatario_Da_WKF);
                            }
                            else
                            {
                                esito = GetDestinatario(i.Id_WKF_Tipologia, i.Id_Stato, out destinatario_Da_WKF);
                            }

                            // se il prossimo stato è azione automatica allora va visto lo stato ancora successivo
                            // perchè lo stato di accettato o visionato potrebbe venir dopo ad una azione automatica
                            if (nextStatus == (int)StatiDematerializzazioneDocumenti.AzioneAutomatica ||
                                nextStatus == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaContabile ||
                                nextStatus == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaAssunzionePreApprovazione ||
                                nextStatus == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaAssunzionePreFirma)
                            {
                                nextStatus = GetNextIdStato(nextStatus, i.Id_WKF_Tipologia);
                            }

                            // se il prossimo stato è ACCETTATO oppure AzioneAutomatica allora il documento deve essere approvato e va visualizzato nella pagina
                            // dell'approvatore
                            if ((nextStatus == (int)StatiDematerializzazioneDocumenti.Accettato) &&
                                esito &&
                                (String.IsNullOrEmpty(destinatario_Da_WKF) ||
                                (destinatario_Da_WKF.Contains("01ADM") || destinatario_Da_WKF.Contains("01APPR")))
                                )
                            {
                                // FRANCESCO 24/11/2022
                                // Se il tipo documento prevede il vistatore
                                // se l'abilitazione ha il vistatore 01VIST
                                // e se l'elemento corrente non ha la data visto valorizzata e
                                // la matricola vistatore valorizzata, allora questo elemento
                                // non può essere ancora approvato
                                string _cod_Tipologia_Documentale = i.Cod_Tipologia_Documentale;
                                int _idTipoDoc = i.Id_Tipo_Doc;
                                var tipoDoc = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(_idTipoDoc)).FirstOrDefault();

                                var com = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w =>
                                w.Codice_Tipologia_Documentale.Equals(_cod_Tipologia_Documentale) &&
                                w.Codice_Tipo_Documento.Equals(tipoDoc.Codice)).FirstOrDefault();
                                bool almenoUnVistatore = false;
                                List<NominativoMatricola> utentiVistatori = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

                                if (utentiVistatori != null && utentiVistatori.Any())
                                {
                                    foreach (var v in utentiVistatori)
                                    {
                                        var rr = AuthHelper.EnableToMatr(v.Matricola, i.MatricolaCreatore, "DEMA", "01VIST");
                                        if (rr.Enabled)
                                        {
                                            almenoUnVistatore = true;
                                        }
                                    }
                                }

                                if (destinatario_Da_WKF.Contains("01VIST") &&
                                    com.Visionatore &&
                                    almenoUnVistatore &&
                                    !i.DataVisto.HasValue &&
                                    !i.DataLettura.HasValue &&
                                    String.IsNullOrEmpty(i.MatricolaVisualizzatore)
                                    )
                                {
                                    continue;
                                }
                                else
                                {
                                    XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                    newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                                    result.Add(newItem);
                                }
                            }
                        }
                        else
                        {
                            XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                            newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                            result.Add(newItem);
                        }
                    }

                    // poichè alcune tipologie non hanno una matricola destinatario, vanno prese tutte le tipologie
                    // con MatricolaDestinatarioVisibile == false, a cui l'utente corrente può accedere, controlla 
                    // se ci sono documenti che appartengono a queste tipologie, se si, i documenti vanno aggiunti
                    // alla lista contenuta in result
                    var _tempFiltro1 = db.XR_DEM_TIPOLOGIE_DOCUMENTALI.Where(w => w.Attivo && (
                                (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                                (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                                (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                                (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                            )).Select(x => x.Codice).ToList();

                    // una volta prese le tipologie documentali visibili
                    // cerca i tipidoc comportamento ai quali è abilitato filtrandoli in base alle tipologie ottenute in precedenza
                    if (_tempFiltro1 != null && _tempFiltro1.Any())
                    {
                        var _tempFiltro2 = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => _tempFiltro1.Contains(w.Codice_Tipologia_Documentale) &&
                        !w.MatricolaDestinatarioVisibile &&
                            (
                                (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                                (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                                (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                                (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                            )).Select(x => new GetDocumentiInBaseAlleTipologieAbilitateResult()
                            {
                                Cod_Tipologia_Documentale = x.Codice_Tipologia_Documentale,
                                Codice_Tipo_Documento = x.Codice_Tipo_Documento,
                                Id_Tipo_Doc = 0
                            }).ToList();

                        if (_tempFiltro2 != null && _tempFiltro2.Any())
                        {
                            foreach (var i in _tempFiltro2)
                            {
                                var _TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Codice.Equals(i.Codice_Tipo_Documento)).FirstOrDefault();
                                if (_TIPI_DOCUMENTO != null)
                                {
                                    i.Id_Tipo_Doc = _TIPI_DOCUMENTO.Id;
                                }

                                foreach (var d in _tempItems.Where(w => w.Cod_Tipologia_Documentale.Equals(i.Cod_Tipologia_Documentale) && w.Id_Tipo_Doc.Equals(i.Id_Tipo_Doc)).ToList())
                                {
                                    if (!almenoUnFiltro)
                                    {
                                        string destinatario_Da_WKF = "";
                                        bool esito = false;
                                        int nextStatus = GetNextIdStato(d.Id_Stato, d.Id_WKF_Tipologia);
                                        esito = GetDestinatario(d.Id_WKF_Tipologia, d.Id_Stato, out destinatario_Da_WKF);

                                        // se il prossimo stato è azione automatica allora va visto lo stato ancora successivo
                                        // perchè lo stato di accettato o visionato potrebbe venir dopo ad una azione automatica
                                        if (nextStatus == (int)StatiDematerializzazioneDocumenti.AzioneAutomatica || nextStatus == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaContabile)
                                        {
                                            nextStatus = GetNextIdStato(nextStatus, d.Id_WKF_Tipologia);
                                        }

                                        // se il prossimo stato è ACCETTATO oppure AzioneAutomatica allora il documento deve essere approvato e va visualizzato nella pagina
                                        // dell'approvatore
                                        if (((nextStatus == (int)StatiDematerializzazioneDocumenti.Accettato) || (nextStatus == (int)StatiDematerializzazioneDocumenti.Visionato)) &&
                                            esito &&
                                            (String.IsNullOrEmpty(destinatario_Da_WKF) ||
                                            (destinatario_Da_WKF.Contains("01ADM") || destinatario_Da_WKF.Contains("01APPR")))
                                            )
                                        {
                                            // FRANCESCO 24/11/2022
                                            // Se il tipo documento prevede il vistatore
                                            // se l'abilitazione ha il vistatore 01VIST
                                            // e se l'elemento corrente non ha la data visto valorizzata e
                                            // la matricola vistatore valorizzata, allora questo elemento
                                            // non può essere ancora approvato
                                            string _cod_Tipologia_Documentale = d.Cod_Tipologia_Documentale;
                                            int _idTipoDoc = d.Id_Tipo_Doc;
                                            var tipoDoc = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(_idTipoDoc)).FirstOrDefault();

                                            var com = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w =>
                                            w.Codice_Tipologia_Documentale.Equals(_cod_Tipologia_Documentale) &&
                                            w.Codice_Tipo_Documento.Equals(tipoDoc.Codice)).FirstOrDefault();

                                            bool almenoUnVistatore = false;
                                            List<NominativoMatricola> utentiVistatori = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

                                            if (utentiVistatori != null && utentiVistatori.Any())
                                            {
                                                foreach (var v in utentiVistatori)
                                                {
                                                    var rr = AuthHelper.EnableToMatr(v.Matricola, d.MatricolaCreatore, "DEMA", "01VIST");
                                                    if (rr.Enabled)
                                                    {
                                                        almenoUnVistatore = true;
                                                    }
                                                }
                                            }

                                            if (destinatario_Da_WKF.Contains("01VIST") &&
                                                com.Visionatore &&
                                                almenoUnVistatore &&
                                                !d.DataVisto.HasValue &&
                                                !d.DataLettura.HasValue &&
                                                String.IsNullOrEmpty(d.MatricolaVisualizzatore)
                                                )
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                                newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(d);
                                                result.Add(newItem);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(d);
                                        result.Add(newItem);
                                    }
                                }
                            }
                        }
                    }
                }

                if (result != null && result.Any())
                {
                    List<NominativoMatricola> vistatori = null;
                    vistatori = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

                    result = result.DistinctBy(w => w.Id).ToList();
                    result = result.OrderByDescending(w => w.MatricolaDestinatario).ToList();

                    List<XR_DEM_DOCUMENTI_EXT> _finalList = new List<XR_DEM_DOCUMENTI_EXT>();
                    List<XR_DEM_DOCUMENTI_EXT> _tempList = new List<XR_DEM_DOCUMENTI_EXT>();
                    _tempList.AddRange(result.ToList());

                    // di tutti i documenti, se prevedono il vistatore e il documento non è stato vistato
                    // allora non va in approvazione e quindi il documento verrà scartato dalla lista
                    foreach (var l in _tempList)
                    {
                        string tipologiaDocumentale = l.Cod_Tipologia_Documentale;
                        int id_tipo_doc = l.Id_Tipo_Doc;
                        string tipologiaDocumento = "";
                        var dem_TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(id_tipo_doc)).FirstOrDefault();

                        if (dem_TIPI_DOCUMENTO != null)
                        {
                            tipologiaDocumento = dem_TIPI_DOCUMENTO.Codice;
                        }

                        var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
    w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                        if (comportamento == null)
                        {
                            throw new Exception("Comportamento non trovato");
                        }

                        if (comportamento.Visionatore)
                        {
                            // se la matricola vistatore è già valorizzata allora
                            // il documento è stato già visionato e può andare in approvazione
                            if (!String.IsNullOrEmpty(l.MatricolaVisualizzatore))
                            {
                                _finalList.Add(l);
                            }
                            else
                            {
                                if (vistatori != null && vistatori.Any())
                                {
                                    bool almenoUnVistatoreAutorizzato = false;
                                    foreach (var i in vistatori)
                                    {
                                        if (almenoUnVistatoreAutorizzato)
                                        {
                                            continue;
                                        }
                                        var r = AuthHelper.EnableToMatr(i.Matricola, l.MatricolaCreatore, "DEMA", "01VIST");
                                        if (r.Enabled)
                                        {
                                            almenoUnVistatoreAutorizzato = true;
                                        }
                                    }

                                    if (!almenoUnVistatoreAutorizzato)
                                    {
                                        // se non c'è un vistatore autorizzato allora il documento passa direttamente all'approvatore
                                        _finalList.Add(l);
                                    }
                                }
                                else
                                {
                                    // se non ci sono vistatori allora il documento va in approvazione
                                    _finalList.Add(l);
                                }
                            }
                        }
                        else
                        {
                            _finalList.Add(l);
                        }
                    }

                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    result.AddRange(_finalList.ToList());
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiFiltrati(TIPI_UTENTI tipologiaUtente, string nominativo = null, string matricola = null, string id_Tipo_Doc = null, DateTime? datadal = null)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            IQueryable<XR_DEM_DOCUMENTI> _tempItems = null;
            List<XR_DEM_DOCUMENTI> items = null;
            DateTime defaultDate = new DateTime(1900, 1, 1);
            bool tuttiGliStati = false;
            try
            {
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                IncentiviEntities db = new IncentiviEntities();

                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

                //TODO da capire che filtro mettere
                // se ho la funzione 02GEST sono operatore
                //if (!subFunc.Contains("01APPR"))
                //{
                //    throw new Exception("Utente non autorizzato");
                //}

                // Get Aree abilitate
                var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                .Where(w => w.MATRICOLA == matricolaCorrente && w.IND_ATTIVO)
                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "02GEST")
                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                .FirstOrDefault();
                var listArea = _myAbil?.GR_AREA.Split(',').Select(int.Parse).ToList();

                //prima carica le tipologie per le quali la matricola ha visibilità
                List<string> TIPOLOGIE_DOCUMENTALI_VISIBILI = new List<string>();

                var _tempTip = db.XR_DEM_TIPOLOGIE_DOCUMENTALI.Where(w => w.Attivo && (
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                )).ToList();

                if (_tempTip != null && _tempTip.Any())
                {
                    TIPOLOGIE_DOCUMENTALI_VISIBILI.AddRange(_tempTip.Select(w => w.Codice).ToList());
                }
                else
                {
                    throw new Exception("L'Utente non ha visibilità su alcuna tipologia");
                }

                List<string> TIPI_DOCUMENTO_VISIBILI = new List<string>();
                List<int> ID_TIPO_DOC_VISIBILI = new List<int>();

                // reperisco tutti i XR_DEM_TIPIDOC_COMPORTAMENTO visibili alla matricola corrente
                var _tempTIPIDOC_COMPORTAMENTO = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => (
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                )).ToList();

                if (_tempTIPIDOC_COMPORTAMENTO != null && _tempTIPIDOC_COMPORTAMENTO.Any())
                {
                    TIPI_DOCUMENTO_VISIBILI.AddRange(_tempTIPIDOC_COMPORTAMENTO.Select(w => w.Codice_Tipo_Documento).ToList());

                    // a questo punto vanno recuperati gli dei tipi doc visibili
                    ID_TIPO_DOC_VISIBILI = db.XR_DEM_TIPI_DOCUMENTO.Where(w => TIPI_DOCUMENTO_VISIBILI.Contains(w.Codice)).Select(w => w.Id).ToList();
                }
                else
                {
                    throw new Exception("L'Utente non ha visibilità su alcuna tipologia");
                }

                if (!String.IsNullOrEmpty(nominativo) || !String.IsNullOrEmpty(matricola) || !String.IsNullOrEmpty(id_Tipo_Doc) || (datadal.HasValue && datadal != defaultDate))
                {
                    _tempItems = db.XR_DEM_DOCUMENTI.Where(w => TIPOLOGIE_DOCUMENTALI_VISIBILI.Contains(w.Cod_Tipologia_Documentale) &&
                                                            ID_TIPO_DOC_VISIBILI.Contains(w.Id_Tipo_Doc) &&
                                                            (listArea == null || listArea.Contains((int)w.IdArea)));

                    switch (tipologiaUtente)
                    {
                        case TIPI_UTENTI.Vistatore:
                            _tempItems = db.XR_DEM_DOCUMENTI.Where(w => w.MatricolaVisualizzatore.Equals(matricolaCorrente));
                            break;
                        case TIPI_UTENTI.Approvatore:
                            _tempItems = db.XR_DEM_DOCUMENTI.Where(w => w.MatricolaApprovatore.Equals(matricolaCorrente));
                            break;
                        default:
                            break;
                    }

                    #region FiltroDataAPartireDa                    
                    if (datadal.HasValue && datadal != defaultDate)
                    {
                        _tempItems = _tempItems.Where(w => w.DataCreazione > datadal.Value);
                    }
                    #endregion

                    #region Filtro_matricola
                    if (!String.IsNullOrEmpty(matricola))
                    {
                        tuttiGliStati = true;
                        _tempItems = _tempItems.Where(w => w.MatricolaDestinatario.Equals(matricola));
                    }
                    #endregion

                    //#region Filtro_StatoDiRichiesta
                    //if (!String.IsNullOrEmpty(statoRichiesta))
                    //{
                    //    int id_stato_filtro = 0;
                    //    bool converti = int.TryParse(statoRichiesta, out id_stato_filtro);

                    //    if (converti)
                    //    {
                    //        _tempItems = _tempItems.Where(w => w.Id_Stato.Equals(id_stato_filtro));
                    //    }
                    //    else
                    //    {
                    //        throw new Exception("Errore, stato pratica non riconosciuto");
                    //    }
                    //}
                    //else
                    //{
                    //_tempItems = _tempItems.Where(w => w.PraticaAttiva);
                    //}
                    //#endregion

                    #region Filtro_TipoDoc
                    if (!String.IsNullOrEmpty(id_Tipo_Doc))
                    {
                        tuttiGliStati = true;
                        int id_Tipo_Doc_filtro = 0;
                        bool converti = int.TryParse(id_Tipo_Doc, out id_Tipo_Doc_filtro);

                        if (converti)
                        {
                            _tempItems = _tempItems.Where(w => w.Id_Tipo_Doc.Equals(id_Tipo_Doc_filtro));
                        }
                        else
                        {
                            throw new Exception("Errore, tipologia documentale non riconosciuta");
                        }
                    }
                    #endregion

                    #region Filtro_Nominativo
                    if (!String.IsNullOrEmpty(nominativo))
                    {
                        tuttiGliStati = true;
                        List<string> matricoleDestinatario = new List<string>();
                        matricoleDestinatario.AddRange(_tempItems.Select(w => w.MatricolaDestinatario).ToList());
                        if (matricoleDestinatario != null && matricoleDestinatario.Any())
                        {
                            string abilKey = "DEMA";
                            string currentMatr = CommonHelper.GetCurrentUserMatricola();

                            DateTime oggi = DateTime.Now;

                            var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.Where(x => x.COD_MATLIBROMAT != null && x.DTA_FINE_CR != null && x.DTA_FINE_CR.Value >= DateTime.Today), currentMatr, null, abilKey);

                            tmpSint = tmpSint.OrderBy(x => x.COD_MATLIBROMAT);

                            var tmp = tmpSint.Select(s => new CercaDipendentiItem()
                            {
                                MATRICOLA = s.COD_MATLIBROMAT,
                                NOME = s.DES_NOMEPERS,
                                COGNOME = s.DES_COGNOMEPERS,
                                SECONDO_COGNOME = s.DES_SECCOGNOME,
                                DATA_ASSUNZIONE = s.DTA_INIZIO_CR,
                                CONTRATTO = s.DES_TPCNTR,
                                ID_PERSONA = s.ID_PERSONA,
                                COD_SEDE = s.COD_SEDE,
                                DES_SEDE = s.DES_SEDE,
                                COD_SERVIZIO = s.COD_SERVIZIO,
                                DES_SERVIZIO = s.DES_SERVIZIO,
                                SmartWorker = false
                            });

                            if (tmp != null && tmp.Any())
                            {
                                foreach (var t in tmp)
                                {
                                    t.NOME = CommonHelper.ToTitleCase(t.NOME);
                                    t.COGNOME = CommonHelper.ToTitleCase(t.COGNOME);
                                    t.SECONDO_COGNOME = CommonHelper.ToTitleCase(t.SECONDO_COGNOME);
                                }
                            }

                            var _filteredList = tmp.Where(w => (w.NOME + " " + w.COGNOME).ToUpper().Contains(nominativo.ToUpper())
                                                    || (w.COGNOME + " " + w.NOME).ToUpper().Contains(nominativo.ToUpper())).ToList();

                            List<string> _matricole = new List<string>();

                            if (_filteredList != null && _filteredList.Any())
                            {
                                _matricole.AddRange(_filteredList.Select(w => w.MATRICOLA).ToList());
                            }

                            _tempItems = _tempItems.Where(w => _matricole.Contains(w.MatricolaDestinatario));
                        }
                    }
                    #endregion

                }
                else
                {
                    _tempItems = db.XR_DEM_DOCUMENTI.Where(w => (w.PraticaAttiva) &&
                                                            (TIPOLOGIE_DOCUMENTALI_VISIBILI.Contains(w.Cod_Tipologia_Documentale) &&
                                                              ID_TIPO_DOC_VISIBILI.Contains(w.Id_Tipo_Doc)) &&
                                                            (listArea == null || listArea.Contains((int)w.IdArea))

                                                            );
                }
                items = _tempItems.ToList();

                result = new List<XR_DEM_DOCUMENTI_EXT>();
                foreach (var d in items)
                {
                    XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                    newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(d);
                    result.Add(newItem);
                }

                if (result != null && result.Any())
                {
                    result = result.OrderBy(w => w.Avanzamento)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoFirma)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza)
                        .ThenByDescending(w => w.MatricolaDestinatario).ToList();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiPerOperatori(string nominativo = null, string oggetto = null, string matricola = null, string id_Tipo_Doc = null, DateTime? datadal = null)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            IQueryable<XR_DEM_DOCUMENTI> _tempItems = null;
            List<XR_DEM_DOCUMENTI> items = null;
            bool tuttiGliStati = false;
            DateTime defaultDate = new DateTime(1900, 1, 1);

            try
            {
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                IncentiviEntities db = new IncentiviEntities();

                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

                // se ho la funzione 02GEST sono operatore
                if (!subFunc.Contains("02GEST"))
                {
                    throw new Exception("Utente non autorizzato");
                }

                // Get Aree abilitate
                var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                .Where(w => w.MATRICOLA == matricolaCorrente && w.IND_ATTIVO)
                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "02GEST")
                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                .FirstOrDefault();
                var listArea = _myAbil?.GR_AREA?.Split(',').Select(int.Parse).ToList();

                //temporaneo 
                //bool cons = CommonHelper.GetParametro<bool>(EnumParametriSistema.ConsideraAreeDematerializzazione);
                //if (!cons) listArea = null;

                // prima carica le tipologie per le quali la matricola ha visibilità
                List<string> TIPOLOGIE_DOCUMENTALI_VISIBILI = new List<string>();

                var _tempTip = db.XR_DEM_TIPOLOGIE_DOCUMENTALI.Where(w => w.Attivo && (
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                )).ToList();

                if (_tempTip != null && _tempTip.Any())
                {
                    TIPOLOGIE_DOCUMENTALI_VISIBILI.AddRange(_tempTip.Select(w => w.Codice).ToList());
                }
                else
                {
                    throw new Exception("L'Utente non ha visibilità su alcuna tipologia");
                }

                List<string> TIPI_DOCUMENTO_VISIBILI = new List<string>();
                List<int> ID_TIPO_DOC_VISIBILI = new List<int>();

                // reperisco tutti i XR_DEM_TIPIDOC_COMPORTAMENTO visibili alla matricola corrente
                var _tempTIPIDOC_COMPORTAMENTO = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => (
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                )).ToList();

                if (_tempTIPIDOC_COMPORTAMENTO != null && _tempTIPIDOC_COMPORTAMENTO.Any())
                {
                    TIPI_DOCUMENTO_VISIBILI.AddRange(_tempTIPIDOC_COMPORTAMENTO.Select(w => w.Codice_Tipo_Documento).ToList());

                    // a questo punto vanno recuperati gli dei tipi doc visibili
                    ID_TIPO_DOC_VISIBILI = db.XR_DEM_TIPI_DOCUMENTO.Where(w => TIPI_DOCUMENTO_VISIBILI.Contains(w.Codice)).Select(w => w.Id).ToList();
                }
                else
                {
                    throw new Exception("L'Utente non ha visibilità su alcuna tipologia");
                }

                if (!String.IsNullOrEmpty(nominativo) || !String.IsNullOrEmpty(matricola) || !String.IsNullOrEmpty(oggetto) || !String.IsNullOrEmpty(id_Tipo_Doc) || (datadal.HasValue && datadal != defaultDate))
                {
                    _tempItems = db.XR_DEM_DOCUMENTI.Where(w => TIPOLOGIE_DOCUMENTALI_VISIBILI.Contains(w.Cod_Tipologia_Documentale) &&
                                                            ID_TIPO_DOC_VISIBILI.Contains(w.Id_Tipo_Doc));

                    if (listArea != null && listArea.Count > 0)
                    {
                        _tempItems = _tempItems.Where(w => w.IdArea != null && listArea.Contains((int)w.IdArea));
                    }
                    //_tempItems = db.XR_DEM_DOCUMENTI.Where(x => x.Id == 4807);
                    //_tempItems = db.XR_DEM_DOCUMENTI.Where(w => TIPOLOGIE_DOCUMENTALI_VISIBILI.Contains(w.Cod_Tipologia_Documentale) &&
                    //                                        ID_TIPO_DOC_VISIBILI.Contains(w.Id_Tipo_Doc) &&
                    //                                        (listArea == null || listArea.Contains((int)w.IdArea)));

                    if (!String.IsNullOrEmpty(matricola))
                    {
                        tuttiGliStati = true;
                        _tempItems = _tempItems.Where(w => w.MatricolaDestinatario.Equals(matricola));
                    }

                    if (!String.IsNullOrEmpty(oggetto))
                    {
                        tuttiGliStati = true;
                        _tempItems = _tempItems.Where(w => w.OggettoProtocollo.Contains(oggetto));
                    }

                    if (datadal.HasValue && datadal != defaultDate)
                    {
                        tuttiGliStati = true;
                        _tempItems = _tempItems.Where(w => w.DataCreazione > datadal.Value);
                    }

                    //if (!String.IsNullOrEmpty(statoRichiesta))
                    //{
                    //    int id_stato_filtro = 0;
                    //    bool converti = int.TryParse(statoRichiesta, out id_stato_filtro);

                    //    if (converti)
                    //    {
                    //        _tempItems = _tempItems.Where(w => w.Id_Stato.Equals(id_stato_filtro));

                    //        if (id_stato_filtro == (int)StatiDematerializzazioneDocumenti.PraticaCancellata)
                    //        {
                    //            _tempItems = _tempItems.Where(w => !w.PraticaAttiva);
                    //        }
                    //        else if (id_stato_filtro != (int)StatiDematerializzazioneDocumenti.Bozza)
                    //        {
                    //            _tempItems = _tempItems.Where(w => w.PraticaAttiva);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        throw new Exception("Errore, stato pratica non riconosciuto");
                    //    }
                    //}
                    //else
                    //{
                    //    _tempItems = _tempItems.Where(w => w.PraticaAttiva);// || w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza);
                    //}

                    if (!String.IsNullOrEmpty(id_Tipo_Doc))
                    {
                        tuttiGliStati = true;
                        int id_Tipo_Doc_filtro = 0;
                        bool converti = int.TryParse(id_Tipo_Doc, out id_Tipo_Doc_filtro);

                        if (converti)
                        {
                            _tempItems = _tempItems.Where(w => w.Id_Tipo_Doc.Equals(id_Tipo_Doc_filtro));
                        }
                        else
                        {
                            throw new Exception("Errore, tipologia documentale non riconosciuta");
                        }
                    }

                    if (!String.IsNullOrEmpty(nominativo))
                    {
                        tuttiGliStati = true;
                        List<string> matricoleDestinatario = new List<string>();
                        var matric = _tempItems.Select(w => w.MatricolaDestinatario).ToList();
                        matricoleDestinatario.AddRange(matric);

                        //matricoleDestinatario.AddRange(_tempItems.Select(w => w.MatricolaDestinatario).ToList());
                        if (matricoleDestinatario != null && matricoleDestinatario.Any())
                        {
                            string abilKey = "DEMA";
                            string currentMatr = CommonHelper.GetCurrentUserMatricola();
                            //var enabledCat = AuthHelper.EnabledCategory(currentMatr, abilKey);
                            //var enabledSer = AuthHelper.EnabledDirection(currentMatr, abilKey);

                            DateTime oggi = DateTime.Now;

                            var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.Where(x => x.COD_MATLIBROMAT != null && x.DTA_FINE_CR != null && x.DTA_FINE_CR.Value >= DateTime.Today), currentMatr, null, abilKey);

                            //if (enabledCat.HasFilter)
                            //    tmpSint = tmpSint.Where(x => (enabledCat.CategorieIncluse.Count() == 0 || enabledCat.CategorieIncluse.Any(y => x.COD_QUALIFICA.StartsWith(y)))
                            //                                && (enabledCat.CategorieEscluse.Count() == 0 || !enabledCat.CategorieEscluse.Any(y => x.COD_QUALIFICA.StartsWith(y))));

                            //if (enabledSer.HasFilter)
                            //    tmpSint = tmpSint.Where(x => enabledSer.DirezioniIncluse.Contains(x.COD_SERVIZIO));

                            tmpSint = tmpSint.OrderBy(x => x.COD_MATLIBROMAT);

                            var tmp = tmpSint.Select(s => new CercaDipendentiItem()
                            {
                                MATRICOLA = s.COD_MATLIBROMAT,
                                NOME = s.DES_NOMEPERS,
                                COGNOME = s.DES_COGNOMEPERS,
                                SECONDO_COGNOME = s.DES_SECCOGNOME,
                                DATA_ASSUNZIONE = s.DTA_INIZIO_CR,
                                CONTRATTO = s.DES_TPCNTR,
                                ID_PERSONA = s.ID_PERSONA,
                                COD_SEDE = s.COD_SEDE,
                                DES_SEDE = s.DES_SEDE,
                                COD_SERVIZIO = s.COD_SERVIZIO,
                                DES_SERVIZIO = s.DES_SERVIZIO,
                                SmartWorker = false
                            });

                            if (tmp != null && tmp.Any())
                            {
                                foreach (var t in tmp)
                                {
                                    t.NOME = CommonHelper.ToTitleCase(t.NOME);
                                    t.COGNOME = CommonHelper.ToTitleCase(t.COGNOME);
                                    t.SECONDO_COGNOME = CommonHelper.ToTitleCase(t.SECONDO_COGNOME);
                                }
                            }

                            var _filteredList = tmp.Where(w => (w.NOME + " " + w.COGNOME).ToUpper().Contains(nominativo.ToUpper())
                                                    || (w.COGNOME + " " + w.NOME).ToUpper().Contains(nominativo.ToUpper())).ToList();

                            List<string> _matricole = new List<string>();

                            if (_filteredList != null && _filteredList.Any())
                            {
                                _matricole.AddRange(_filteredList.Select(w => w.MATRICOLA).ToList());
                            }

                            _tempItems = _tempItems.Where(w => _matricole.Contains(w.MatricolaDestinatario));
                        }
                    }
                }
                else
                {
                    if (listArea != null)
                    {
                        _tempItems = db.XR_DEM_DOCUMENTI.Where(w => (w.PraticaAttiva || w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza) &&
                                                            (TIPOLOGIE_DOCUMENTALI_VISIBILI.Contains(w.Cod_Tipologia_Documentale) &&
                                                              ID_TIPO_DOC_VISIBILI.Contains(w.Id_Tipo_Doc))
                                                                                                            &&
                                                            ((w.IdArea != null && listArea.Contains((int)w.IdArea))));
                    }
                    else
                    {
                        _tempItems = db.XR_DEM_DOCUMENTI.Where(w => (w.PraticaAttiva || w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza) &&
                                                           (TIPOLOGIE_DOCUMENTALI_VISIBILI.Contains(w.Cod_Tipologia_Documentale) &&
                                                             ID_TIPO_DOC_VISIBILI.Contains(w.Id_Tipo_Doc)));
                        //&&
                        //((w.IdArea != null && listArea.Contains((int)w.IdArea))));
                    }
                }

                items = _tempItems.Where(x=>x.Id_Stato!=200).ToList(); //non mostrare gli eliminati

                if (items != null && items.Any())
                {
                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());

                        // mandati di cassa
                        //if (matricoleDaFiltrare.Contains(null))
                        //{
                        //    matricoleConsentite.Add(null);
                        //}
                    }
                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    var itemsF = items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList();
                    List<string> matrCoinvolte = itemsF.SelectMany(x => new[] {x.MatricolaCreatore, x.MatricolaDestinatario,
                                                 x.MatricolaApprovatore,x.MatricolaVisualizzatore }).Distinct().ToList();
                    var WKF = db.XR_WKF_WORKFLOW.ToList();
                    var STATI = db.XR_DEM_STATI.ToList();
                    var MatrNominativo = db.SINTESI1
                        .Where(x => matrCoinvolte.Contains(x.COD_MATLIBROMAT))
                        .Select(x => new MatrNom { matricola = x.COD_MATLIBROMAT, nominativo = x.DES_COGNOMEPERS + " " + x.DES_NOMEPERS }).ToList();
                    foreach (var i in itemsF)
                    {

                        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i,
                            WKF,
                            STATI,
                            MatrNominativo
                            );
                        result.Add(newItem);
                    }

                    // poichè alcune tipologie non hanno una matricola destinatario, vanno prese tutte le tipologie
                    // con MatricolaDestinatarioVisibile == false, a cui l'utente corrente può accedere, controlla 
                    // se ci sono documenti che appartengono a queste tipologie, se si, i documenti vanno aggiunti
                    // alla lista contenuta in result
                    var _tempFiltro1 = db.XR_DEM_TIPOLOGIE_DOCUMENTALI.Where(w => w.Attivo && (
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                            )).Select(x => x.Codice).ToList();

                    // una volta prese le tipologie documentali visibili
                    // cerca i tipidoc comportamento ai quali è abilitato filtrandoli in base alle tipologie ottenute in precedenza
                    if (_tempFiltro1 != null && _tempFiltro1.Any())
                    {
                        var _tempFiltro2 = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => _tempFiltro1.Contains(w.Codice_Tipologia_Documentale) &&
                        !w.MatricolaDestinatarioVisibile &&
                            (
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                            )).Select(x => new GetDocumentiInBaseAlleTipologieAbilitateResult()
                            {
                                Cod_Tipologia_Documentale = x.Codice_Tipologia_Documentale,
                                Codice_Tipo_Documento = x.Codice_Tipo_Documento,
                                Id_Tipo_Doc = 0
                            }).ToList();

                        if (_tempFiltro2 != null && _tempFiltro2.Any())
                        {
                            foreach (var i in _tempFiltro2)
                            {
                                var _TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Codice.Equals(i.Codice_Tipo_Documento)).FirstOrDefault();
                                if (_TIPI_DOCUMENTO != null)
                                {
                                    i.Id_Tipo_Doc = _TIPI_DOCUMENTO.Id;
                                }

                                foreach (var d in _tempItems.Where(w => w.Cod_Tipologia_Documentale.Equals(i.Cod_Tipologia_Documentale) && w.Id_Tipo_Doc.Equals(i.Id_Tipo_Doc)).ToList())
                                {
                                    XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                    newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(d);
                                    result.Add(newItem);
                                }
                            }
                        }
                    }
                }

                if (result != null && result.Any())
                {
                    result = result.DistinctBy(w => w.Id).ToList();

                    if (!tuttiGliStati)
                    {
                        result = result.Where(w => w.Avanzamento != 100).ToList();
                    }

                    result = result.OrderBy(w => w.Avanzamento)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoFirma)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza)
                        .ThenByDescending(w => w.MatricolaDestinatario).ToList();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static List<XR_DEM_DOCUMENTI> FiltraPerAbilitazioni(string funzione, List<XR_DEM_DOCUMENTI> items, string matricolaCorrente)//01VIST etc...
        {
            if (items == null || !items.Any()) return items;

            var db = new IncentiviEntities();
            var subf = db.XR_HRIS_ABIL_SUBFUNZIONE.Where(x => x.COD_SUBFUNZIONE == funzione).FirstOrDefault();
            if (subf != null)
            {
                var abil = db.XR_HRIS_ABIL.Where(x => x.ID_SUBFUNZ == subf.ID_SUBFUNZ && x.MATRICOLA == matricolaCorrente).FirstOrDefault();
                if (abil != null)
                {
                    if (!String.IsNullOrWhiteSpace(abil.TIP_DOC_INCLUSI))
                    {
                        items.RemoveAll(x =>
                            !abil.TIP_DOC_INCLUSI.Trim().Split(',').Contains(
                                x.Cod_Tipologia_Documentale.Trim() +
                                "_" +
                                db.XR_DEM_TIPI_DOCUMENTO.Where(z => z.Id == x.Id_Tipo_Doc).Select(z => z.Codice.Trim()).FirstOrDefault()));
                    }
                    if (items.Any() && !String.IsNullOrWhiteSpace(abil.TIP_DOC_ESCLUSI))
                    {
                        items.RemoveAll(x => abil.TIP_DOC_ESCLUSI.Trim().Split(',').Contains(
                            x.Cod_Tipologia_Documentale.Trim() +
                            "_" +
                            db.XR_DEM_TIPI_DOCUMENTO.Where(z => z.Id == x.Id_Tipo_Doc).Select(z => z.Codice.Trim()).FirstOrDefault()));
                    }
                }
            }
            return items;
        }
        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiDaVisionare(string nominativo = null, string matricola = null, string oggetto = null, string id_Tipo_Doc = null, DateTime? datadal = null)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            IQueryable<XR_DEM_DOCUMENTI> _tempItems = null;
            List<XR_DEM_DOCUMENTI> items = null;
            DateTime defaultDate = new DateTime(1900, 1, 1);

            try
            {
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                IncentiviEntities db = new IncentiviEntities();

                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

                // se ho la funzione 01VIST sono Vistatore
                if (!subFunc.Contains("01VIST"))
                {
                    throw new Exception("Utente non autorizzato");
                }

                // Get Aree abilitate
                //var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                //.Where(w => w.MATRICOLA == matricolaCorrente && w.IND_ATTIVO)
                //.Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                //.Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "02GEST")
                //.Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                //.FirstOrDefault();
                //var listArea = _myAbil.GR_AREA.Split(',').Select(int.Parse).ToList();

                // prima carica le tipologie per le quali la matricola ha visibilità
                List<string> TIPOLOGIE_DOCUMENTALI_VISIBILI = new List<string>();

                // prende tutte le tipologie documentali per le quali la matricola corrente è abilitata
                var _tempTip = db.XR_DEM_TIPOLOGIE_DOCUMENTALI.Where(w => w.Attivo && (
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                )).ToList();

                if (_tempTip != null && _tempTip.Any())
                {
                    TIPOLOGIE_DOCUMENTALI_VISIBILI.AddRange(_tempTip.Select(w => w.Codice).ToList());
                }
                else
                {
                    throw new Exception("L'Utente non ha visibilità su alcuna tipologia");
                }

                List<string> TIPI_DOCUMENTO_VISIBILI = new List<string>();
                List<int> ID_TIPO_DOC_VISIBILI = new List<int>();

                // reperisco tutti i XR_DEM_TIPIDOC_COMPORTAMENTO visibili alla matricola corrente 
                var _tempTIPIDOC_COMPORTAMENTO = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w =>
                w.Visionatore &&
                (
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                )).ToList();

                if (_tempTIPIDOC_COMPORTAMENTO != null && _tempTIPIDOC_COMPORTAMENTO.Any())
                {
                    TIPI_DOCUMENTO_VISIBILI.AddRange(_tempTIPIDOC_COMPORTAMENTO.Select(w => w.Codice_Tipo_Documento).ToList());

                    // a questo punto vanno recuperati gli dei tipi doc visibili
                    ID_TIPO_DOC_VISIBILI = db.XR_DEM_TIPI_DOCUMENTO.Where(w => TIPI_DOCUMENTO_VISIBILI.Contains(w.Codice)).Select(w => w.Id).ToList();
                }
                else
                {
                    throw new Exception("L'Utente non ha visibilità su alcuna tipologia");
                }

                if (!String.IsNullOrEmpty(nominativo) || !String.IsNullOrEmpty(matricola) || !String.IsNullOrEmpty(id_Tipo_Doc) || (datadal.HasValue && datadal != defaultDate))
                {
                    // se c'è almeno un filtro
                    _tempItems = db.XR_DEM_DOCUMENTI.Where(w => TIPOLOGIE_DOCUMENTALI_VISIBILI.Contains(w.Cod_Tipologia_Documentale) &&
                                                            ID_TIPO_DOC_VISIBILI.Contains(w.Id_Tipo_Doc)
                                                            //&&  listArea.Contains((int)w.IdArea)
                                                            );

                    if (!String.IsNullOrEmpty(matricola))
                    {
                        _tempItems = _tempItems.Where(w => w.MatricolaDestinatario.Equals(matricola));
                    }

                    if (!String.IsNullOrEmpty(oggetto))
                    {
                        _tempItems = _tempItems.Where(w => w.OggettoProtocollo.Contains(oggetto));
                    }

                    if (datadal.HasValue && datadal != defaultDate)
                    {
                        _tempItems = _tempItems.Where(w => w.DataCreazione > datadal.Value);
                    }

                    //if (!String.IsNullOrEmpty(statoRichiesta))
                    //{
                    //    int id_stato_filtro = 0;
                    //    bool converti = int.TryParse(statoRichiesta, out id_stato_filtro);

                    //    if (converti)
                    //    {
                    //        _tempItems = _tempItems.Where(w => w.Id_Stato.Equals(id_stato_filtro));

                    //        if (id_stato_filtro == (int)StatiDematerializzazioneDocumenti.PraticaCancellata)
                    //        {
                    //            _tempItems = _tempItems.Where(w => !w.PraticaAttiva);
                    //        }
                    //        else if (id_stato_filtro != (int)StatiDematerializzazioneDocumenti.Bozza)
                    //        {
                    //            _tempItems = _tempItems.Where(w => w.PraticaAttiva);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        throw new Exception("Errore, stato pratica non riconosciuto");
                    //    }
                    //}
                    //else
                    //{
                    //    _tempItems = _tempItems.Where(w => w.PraticaAttiva);
                    //}

                    if (!String.IsNullOrEmpty(id_Tipo_Doc))
                    {
                        int id_Tipo_Doc_filtro = 0;
                        bool converti = int.TryParse(id_Tipo_Doc, out id_Tipo_Doc_filtro);

                        if (converti)
                        {
                            _tempItems = _tempItems.Where(w => w.Id_Tipo_Doc.Equals(id_Tipo_Doc_filtro));
                        }
                        else
                        {
                            throw new Exception("Errore, tipologia documentale non riconosciuta");
                        }
                    }

                    if (!String.IsNullOrEmpty(nominativo))
                    {
                        List<string> matricoleDestinatario = new List<string>();
                        matricoleDestinatario.AddRange(_tempItems.Select(w => w.MatricolaDestinatario).ToList());
                        if (matricoleDestinatario != null && matricoleDestinatario.Any())
                        {
                            string abilKey = "DEMA";
                            string currentMatr = CommonHelper.GetCurrentUserMatricola();
                            //var enabledCat = AuthHelper.EnabledCategory(currentMatr, abilKey);
                            //var enabledSer = AuthHelper.EnabledDirection(currentMatr, abilKey);

                            DateTime oggi = DateTime.Now;
                            var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.Where(x => x.COD_MATLIBROMAT != null && x.DTA_FINE_CR != null && x.DTA_FINE_CR.Value >= DateTime.Today), currentMatr, null, abilKey);

                            //var tmpSint = db.SINTESI1.Where(x => x.COD_MATLIBROMAT != null && x.DTA_FINE_CR != null && x.DTA_FINE_CR.Value >= DateTime.Today);
                            //if (enabledCat.HasFilter)
                            //    tmpSint = tmpSint.Where(x => (enabledCat.CategorieIncluse.Count() == 0 || enabledCat.CategorieIncluse.Any(y => x.COD_QUALIFICA.StartsWith(y)))
                            //                                && (enabledCat.CategorieEscluse.Count() == 0 || !enabledCat.CategorieEscluse.Any(y => x.COD_QUALIFICA.StartsWith(y))));

                            //if (enabledSer.HasFilter)
                            //    tmpSint = tmpSint.Where(x => enabledSer.DirezioniIncluse.Contains(x.COD_SERVIZIO));

                            tmpSint = tmpSint.OrderBy(x => x.COD_MATLIBROMAT);

                            var tmp = tmpSint.Select(s => new CercaDipendentiItem()
                            {
                                MATRICOLA = s.COD_MATLIBROMAT,
                                NOME = s.DES_NOMEPERS,
                                COGNOME = s.DES_COGNOMEPERS,
                                SECONDO_COGNOME = s.DES_SECCOGNOME,
                                DATA_ASSUNZIONE = s.DTA_INIZIO_CR,
                                CONTRATTO = s.DES_TPCNTR,
                                ID_PERSONA = s.ID_PERSONA,
                                COD_SEDE = s.COD_SEDE,
                                DES_SEDE = s.DES_SEDE,
                                COD_SERVIZIO = s.COD_SERVIZIO,
                                DES_SERVIZIO = s.DES_SERVIZIO,
                                SmartWorker = false
                            });

                            if (tmp != null && tmp.Any())
                            {
                                foreach (var t in tmp)
                                {
                                    t.NOME = CommonHelper.ToTitleCase(t.NOME);
                                    t.COGNOME = CommonHelper.ToTitleCase(t.COGNOME);
                                    t.SECONDO_COGNOME = CommonHelper.ToTitleCase(t.SECONDO_COGNOME);
                                }
                            }

                            var _filteredList = tmp.Where(w => (w.NOME + " " + w.COGNOME).ToUpper().Contains(nominativo.ToUpper())
                                                    || (w.COGNOME + " " + w.NOME).ToUpper().Contains(nominativo.ToUpper())).ToList();

                            List<string> _matricole = new List<string>();

                            if (_filteredList != null && _filteredList.Any())
                            {
                                _matricole.AddRange(_filteredList.Select(w => w.MATRICOLA).ToList());
                            }

                            _tempItems = _tempItems.Where(w => _matricole.Contains(w.MatricolaDestinatario));
                        }
                    }
                }
                else
                {
                    _tempItems = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva &&
                                                            (TIPOLOGIE_DOCUMENTALI_VISIBILI.Contains(w.Cod_Tipologia_Documentale) &&
                                                              ID_TIPO_DOC_VISIBILI.Contains(w.Id_Tipo_Doc))
                                                            //&&
                                                            //listArea.Contains((int)w.IdArea)
                                                            );
                }
                _tempItems = _tempItems.Where(x => x.Id_Stato != 200);

                //items = _tempItems.ToList();
                ///MAX 27062023////////////////////////////////////////////////////////////
                var itemsAnonymous = _tempItems.Select(x => new { x.Id_Stato, x.Id_WKF_Tipologia, x.Id }).ToList();
                if (itemsAnonymous != null && itemsAnonymous.Any())
                {
                    List<int> DocumentsId = new List<int>();
                    var WKF = db.XR_WKF_WORKFLOW.ToList();
                    foreach (var i in itemsAnonymous)
                    {
                        if (GetDestinatarioReloaded(i.Id_WKF_Tipologia, i.Id_Stato, WKF, out string destinatario_Da_WKF))
                        {
                            if (String.IsNullOrEmpty(destinatario_Da_WKF) || destinatario_Da_WKF.Contains("01VIST"))
                                DocumentsId.Add(i.Id);
                        }
                    }
                    items = db.XR_DEM_DOCUMENTI.Where(x => DocumentsId.Contains(x.Id)).ToList();



                    //Max 04072023 ////////////////////////////////////////////////////////////////////////
                    items = FiltraPerAbilitazioni("01VIST", items, matricolaCorrente);
                    ///////////////////////////////////////////////////////////////////////////////////////

                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                    }
                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                    {
                        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                        result.Add(newItem);
                    }

                    // poichè alcune tipologie non hanno una matricola destinatario, vanno prese tutte le tipologie
                    // con MatricolaDestinatarioVisibile == false, a cui l'utente corrente può accedere, controlla 
                    // se ci sono documenti che appartengono a queste tipologie, se si, i documenti vanno aggiunti
                    // alla lista contenuta in result
                    var _tempFiltro1 = db.XR_DEM_TIPOLOGIE_DOCUMENTALI.Where(w => w.Attivo && (
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                            )).Select(x => x.Codice).ToList();

                    // una volta prese le tipologie documentali visibili
                    // cerca i tipidoc comportamento ai quali è abilitato filtrandoli in base alle tipologie ottenute in precedenza
                    if (_tempFiltro1 != null && _tempFiltro1.Any())
                    {
                        var _tempFiltro2 = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => _tempFiltro1.Contains(w.Codice_Tipologia_Documentale) &&
                        !w.MatricolaDestinatarioVisibile &&
                            (
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                            )).Select(x => new GetDocumentiInBaseAlleTipologieAbilitateResult()
                            {
                                Cod_Tipologia_Documentale = x.Codice_Tipologia_Documentale,
                                Codice_Tipo_Documento = x.Codice_Tipo_Documento,
                                Id_Tipo_Doc = 0
                            }).ToList();

                        if (_tempFiltro2 != null && _tempFiltro2.Any())
                        {
                            foreach (var i in _tempFiltro2)
                            {
                                var _TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Codice.Equals(i.Codice_Tipo_Documento)).FirstOrDefault();
                                if (_TIPI_DOCUMENTO != null)
                                {
                                    i.Id_Tipo_Doc = _TIPI_DOCUMENTO.Id;
                                }

                                foreach (var d in _tempItems.Where(w => w.Cod_Tipologia_Documentale.Equals(i.Cod_Tipologia_Documentale) && w.Id_Tipo_Doc.Equals(i.Id_Tipo_Doc)).ToList())
                                {
                                    XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                    newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(d);
                                    result.Add(newItem);
                                }
                            }
                        }
                    }
                }

                if (result != null && result.Any())
                {
                    result = result.DistinctBy(w => w.Id).ToList();
                    // FRANCESCO 14/02/2023
                    //result = result.Where(w => w.Id_Stato <= (int)StatiDematerializzazioneDocumenti.Visionato).ToList();

                    //if (String.IsNullOrEmpty(statoRichiesta))
                    //{
                    //    result = result.Where(w => w.Avanzamento != 100).ToList();
                    //}

                    List<XR_DEM_DOCUMENTI_EXT> _temp = new List<XR_DEM_DOCUMENTI_EXT>();
                    _temp.AddRange(result.ToList());
                    result.Clear();
                    var WKF = db.XR_WKF_WORKFLOW.ToList();
                    foreach (var i in _temp)
                    {
                        var r = AuthHelper.EnableToMatr(matricolaCorrente, i.MatricolaCreatore, "DEMA", "01VIST");
                        // se la matricola corrente non ha diritti come vistatore della matricola creatrice del
                        // documento, allora verrà scartato
                        if (!r.Enabled)
                        {
                            continue;
                        }

                        // se non ci sono filtri, allora
                        // bisogna controllare lo stato della richiesta
                        // perchè vanno prese soltanto quelle che sono pronte per
                        // essere vistate
                        //if (String.IsNullOrEmpty(statoRichiesta))
                        //{
                        // se il prossimo stato è vistato, allora il documento va mostrato

                        // se non ci sono filtri sullo stato richiesta, vanno presi solo i documenti
                        // che vanno visionati, quindi se il documento è già nello stato visionato
                        // va scartato
                        if (i.Id_Stato == (int)StatiDematerializzazioneDocumenti.Visionato)
                        {
                            continue;
                        }

                        // recupero lo stato attuale del documento
                        string destinatario_Da_WKF = "";
                        bool esito = GetDestinatarioReloaded(i.Id_WKF_Tipologia, i.Id_Stato, WKF, out destinatario_Da_WKF);

                        if (esito &&
                            (String.IsNullOrEmpty(destinatario_Da_WKF) ||
                            destinatario_Da_WKF.Contains("01VIST"))
                            )
                        {
                            // se allo stato attuale è previsto un vistatore
                            // allora il documento va mostrato al vistatore
                            result.Add(i);
                        }
                        //}
                        //else
                        //{
                        //    result.Add(i);
                        //}
                    }

                    result = result.OrderBy(w => w.Avanzamento)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoFirma)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza)
                        .ThenByDescending(w => w.MatricolaDestinatario).ToList();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiPerSegreteria(string nominativo = null, string matricola = null, string oggetto = null, string id_Tipo_Doc = null, DateTime? datadal = null)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            IQueryable<XR_DEM_DOCUMENTI> _tempItems = null;
            List<XR_DEM_DOCUMENTI> items = null;
            bool tuttiGliStati = false;
            List<string> tipologieAbilitate = null;
            DateTime defaultDate = new DateTime(1900, 1, 1);

            try
            {
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                IncentiviEntities db = new IncentiviEntities();

                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

                // se ho la funzione 01SEGR appartengo alla segreteria e posso continuare
                if (!subFunc.Contains("01SEGR"))
                {
                    throw new Exception("Utente non autorizzato");
                }

                // se non c'è matricola, allora è una tipologia come ad esempio 
                // APPUNTO, che non ha matricola dipendente, ma ha comunque un vistatore
                var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                                .Where(w => w.MATRICOLA == matricolaCorrente && w.IND_ATTIVO)
                                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01SEGR")
                                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                                .FirstOrDefault();

                if (_myAbil != null)
                {
                    #region POSSO SEGRETERIA 
                    //Calcolo degli elementi che posso lavorare
                    List<string> posso = new List<string>();

                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                        && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                    {
                        posso.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                    }

                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                        && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                        && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                    {
                        posso.Add(_myAbil.TIP_DOC_INCLUSI);
                    }

                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                        && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                    {
                        posso.Add("*");
                    }
                    #endregion

                    #region NON POSSO SEGRETERIA 
                    //Calcolo degli elementi che non posso lavorare
                    List<string> nonPosso = new List<string>();

                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                        && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                    {
                        nonPosso.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                    }

                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                        && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                        && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                    {
                        nonPosso.Add(_myAbil.TIP_DOC_ESCLUSI);
                    }

                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                        && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                    {
                        nonPosso.Add("*");
                    }
                    #endregion

                    #region CASO LIMITE TUTTI E DUE *
                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                        !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                            && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                    {
                        posso = new List<string>();
                        nonPosso = new List<string>();
                    }
                    #endregion

                    tipologieAbilitate = posso.Except(nonPosso).ToList();

                    if (tipologieAbilitate.Contains("*"))
                    {
                        // null non applicherà il filtro sulla tipologia perchè abilitato a tutte
                        tipologieAbilitate = null;
                    }
                }

                // recupera tutti i workflow che hanno 01SEGR tra i destinatari
                var XR_WKF_WORKFLOW = db.XR_WKF_WORKFLOW.Where(w => w.DESTINATARIO.Contains("01SEGR")).Select(w => w.ID_TIPOLOGIA).ToList();

                List<int> idWkf = XR_WKF_WORKFLOW.Distinct().ToList();
                List<int> idTipoDoc = new List<int>();

                if (tipologieAbilitate != null)
                {
                    idTipoDoc = db.XR_DEM_TIPI_DOCUMENTO.Where(w => tipologieAbilitate.Contains(w.Codice)).Select(w => w.Id).ToList();
                }

                if (idTipoDoc != null && idTipoDoc.Any())
                {
                    _tempItems = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva &&
                                    //w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Visionato &&
                                    idWkf.Contains(w.Id_WKF_Tipologia) &&
                                    idTipoDoc.Contains(w.Id_Tipo_Doc));
                }
                else
                {
                    _tempItems = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva &&
                                    //w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Visionato &&
                                    idWkf.Contains(w.Id_WKF_Tipologia));
                }

                if (!String.IsNullOrEmpty(nominativo) || !String.IsNullOrEmpty(matricola) || !String.IsNullOrEmpty(id_Tipo_Doc) || (datadal.HasValue && datadal != defaultDate))
                {
                    if (!String.IsNullOrEmpty(matricola))
                    {
                        tuttiGliStati = true;
                        _tempItems = _tempItems.Where(w => w.MatricolaDestinatario.Equals(matricola));
                    }

                    if (datadal.HasValue && datadal != defaultDate)
                    {
                        tuttiGliStati = true;
                        _tempItems = _tempItems.Where(w => w.DataCreazione > datadal.Value);
                    }

                    //if (!String.IsNullOrEmpty(statoRichiesta))
                    //{
                    //    int id_stato_filtro = 0;
                    //    bool converti = int.TryParse(statoRichiesta, out id_stato_filtro);

                    //    if (converti)
                    //    {
                    //        _tempItems = _tempItems.Where(w => w.Id_Stato.Equals(id_stato_filtro));

                    //        if (id_stato_filtro == (int)StatiDematerializzazioneDocumenti.PraticaCancellata)
                    //        {
                    //            _tempItems = _tempItems.Where(w => !w.PraticaAttiva);
                    //        }
                    //        else if (id_stato_filtro != (int)StatiDematerializzazioneDocumenti.Bozza)
                    //        {
                    //            _tempItems = _tempItems.Where(w => w.PraticaAttiva);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        throw new Exception("Errore, stato pratica non riconosciuto");
                    //    }
                    //}
                    //else
                    //{
                    //    _tempItems = _tempItems.Where(w => w.PraticaAttiva); //|| w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza);
                    //}

                    if (!String.IsNullOrEmpty(id_Tipo_Doc))
                    {
                        tuttiGliStati = true;
                        int id_Tipo_Doc_filtro = 0;
                        if (id_Tipo_Doc.Contains(","))
                        {
                            List<int> lstInt = id_Tipo_Doc.Split(',').Select(s => int.Parse(s)).ToList();

                            if (lstInt.Any())
                            {
                                _tempItems = _tempItems.Where(w => lstInt.Contains(w.Id_Tipo_Doc));
                            }
                            else
                            {
                                throw new Exception("Errore, tipologia documentale non riconosciuta");
                            }
                        }
                        else
                        {
                            bool converti = int.TryParse(id_Tipo_Doc, out id_Tipo_Doc_filtro);

                            if (converti)
                            {
                                _tempItems = _tempItems.Where(w => w.Id_Tipo_Doc.Equals(id_Tipo_Doc_filtro));
                            }
                            else
                            {
                                throw new Exception("Errore, tipologia documentale non riconosciuta");
                            }
                        }
                        
                    }

                    if (!String.IsNullOrEmpty(nominativo))
                    {
                        tuttiGliStati = true;
                        List<string> matricoleDestinatario = new List<string>();
                        matricoleDestinatario.AddRange(_tempItems.Select(w => w.MatricolaDestinatario).ToList());
                        if (matricoleDestinatario != null && matricoleDestinatario.Any())
                        {
                            string abilKey = "DEMA";
                            string currentMatr = CommonHelper.GetCurrentUserMatricola();
                            DateTime oggi = DateTime.Now;

                            var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.Where(x => x.COD_MATLIBROMAT != null && x.DTA_FINE_CR != null && x.DTA_FINE_CR.Value >= DateTime.Today), currentMatr, null, abilKey);

                            tmpSint = tmpSint.OrderBy(x => x.COD_MATLIBROMAT);

                            var tmp = tmpSint.Select(s => new CercaDipendentiItem()
                            {
                                MATRICOLA = s.COD_MATLIBROMAT,
                                NOME = s.DES_NOMEPERS,
                                COGNOME = s.DES_COGNOMEPERS,
                                SECONDO_COGNOME = s.DES_SECCOGNOME,
                                DATA_ASSUNZIONE = s.DTA_INIZIO_CR,
                                CONTRATTO = s.DES_TPCNTR,
                                ID_PERSONA = s.ID_PERSONA,
                                COD_SEDE = s.COD_SEDE,
                                DES_SEDE = s.DES_SEDE,
                                COD_SERVIZIO = s.COD_SERVIZIO,
                                DES_SERVIZIO = s.DES_SERVIZIO,
                                SmartWorker = false
                            });

                            if (tmp != null && tmp.Any())
                            {
                                foreach (var t in tmp)
                                {
                                    t.NOME = CommonHelper.ToTitleCase(t.NOME);
                                    t.COGNOME = CommonHelper.ToTitleCase(t.COGNOME);
                                    t.SECONDO_COGNOME = CommonHelper.ToTitleCase(t.SECONDO_COGNOME);
                                }
                            }

                            var _filteredList = tmp.Where(w => (w.NOME + " " + w.COGNOME).ToUpper().Contains(nominativo.ToUpper())
                                                    || (w.COGNOME + " " + w.NOME).ToUpper().Contains(nominativo.ToUpper())).ToList();

                            List<string> _matricole = new List<string>();

                            if (_filteredList != null && _filteredList.Any())
                            {
                                _matricole.AddRange(_filteredList.Select(w => w.MATRICOLA).ToList());
                            }

                            _tempItems = _tempItems.Where(w => _matricole.Contains(w.MatricolaDestinatario));
                        }
                    }
                }


                items = _tempItems.ToList();

                if (items != null && items.Any())
                {
                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                    }
                    result = new List<XR_DEM_DOCUMENTI_EXT>();

                    foreach (var i in items)
                    {
                        int nextStatus = DematerializzazioneManager.GetNextIdStato(i.Id_Stato, i.Id_WKF_Tipologia, true);

                        if (nextStatus == (int)StatiDematerializzazioneDocumenti.VisionatoSegreteria || tuttiGliStati)
                        {
                            if (String.IsNullOrEmpty(i.MatricolaDestinatario))
                            {
                                XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                                result.Add(newItem);
                            }
                            else if (matricoleConsentite.Contains(i.MatricolaDestinatario))
                            {
                                XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                                result.Add(newItem);
                            }
                        }
                    }
                }

                if (result != null && result.Any())
                {
                    result = result.DistinctBy(w => w.Id).ToList();

                    if (!tuttiGliStati)
                    {
                        result = result.Where(w => w.Avanzamento != 100).ToList();
                    }

                    result = result.OrderBy(w => w.Avanzamento)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoFirma)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza)
                        .ThenByDescending(w => w.MatricolaDestinatario).ToList();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static int GetPrevIdStato(int currentState, int idTipologia, bool skipRifiuto = false)
        {
            int result = 0;

            try
            {
                IncentiviEntities db = new IncentiviEntities();

                var item = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(idTipologia)).ToList();

                if (item == null || !item.Any())
                {
                    throw new Exception("Errore! Workflow non trovato");
                }

                int ordine = 0;
                var c_item = item.Where(w => w.ID_STATO == currentState).FirstOrDefault();
                if (c_item != null)
                {
                    ordine = c_item.ORDINE;
                    item = item.Where(w => w.ID_STATO != (int)StatiDematerializzazioneDocumenti.AzioneAutomatica || w.ID_STATO != (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaContabile).ToList();

                    if (skipRifiuto)
                    {
                        item = item.Where(w => w.ID_STATO != (int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente &&
                        w.ID_STATO != (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore).ToList();
                    }

                    if (ordine == 1)
                    {
                        // da verificare 
                        // questa situazione non si dovrebbe verificare mai
                        item = item.Where(w => w.ORDINE == ordine && w.ID_STATO < currentState).OrderByDescending(k => k.ID_STATO).ToList();
                    }
                    else
                    {
                        item = item.Where(w => w.ORDINE == (ordine - 1)).OrderByDescending(k => k.ID_STATO).ToList();
                    }

                    if (item.Any())
                        result = item.FirstOrDefault().ID_STATO;
                    else
                        result = 0;
                }
                else
                {
                    throw new Exception("Errore nel reperimento dello stato nel workflow");
                }
            }
            catch (Exception ex)
            {
                result = 0;
            }
            return result;
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentoByIdRichiesta(int idRichiesta)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            List<XR_DEM_DOCUMENTI> items = null;
            try
            {
                IncentiviEntities db = new IncentiviEntities();

                items = db.XR_DEM_DOCUMENTI.Where(w => w.Id_Richiesta != null && w.Id_Richiesta.Value == idRichiesta).ToList();

                if (items != null && items.Any())
                {
                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    foreach (var i in items)
                    {
                        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                        result.Add(newItem);
                    }
                    result = result.DistinctBy(w => w.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static List<AttributiAggiuntivi> GetListaPiatta(AttributiAggiuntivi initialFolder)
        {
            var folders = new List<AttributiAggiuntivi>();

            if (initialFolder.InLine == null)
            {
                folders.Add(initialFolder);
            }

            if (initialFolder.InLine != null && initialFolder.InLine.Any())
            {
                foreach (var f in initialFolder.InLine.ToList())
                {
                    folders.AddRange(GetListaPiatta(f));
                }
            }

            return folders;
        }

        public static string GetPeriodoByIdMatRichiesta(int idRichiesta)
        {
            string periodo = "";

            try
            {
                IncentiviEntities db = new IncentiviEntities();

                var item = db.XR_MAT_RICHIESTE.Where(w => w.ID.Equals(idRichiesta)).FirstOrDefault();
                if (item != null)
                {
                    if (item.DATA_INIZIO_MATERNITA.HasValue)
                    {
                        periodo = item.DATA_INIZIO_MATERNITA.GetValueOrDefault().ToString("dd/MM/yyyy");
                        if (item.DATA_FINE_MATERNITA.HasValue)
                        {
                            periodo += " - " + item.DATA_FINE_MATERNITA.GetValueOrDefault().ToString("dd/MM/yyyy");

                        }
                    }
                    else if (item.INIZIO_GIUSTIFICATIVO.HasValue)
                    {
                        periodo = item.INIZIO_GIUSTIFICATIVO.GetValueOrDefault().ToString("dd/MM/yyyy");
                        if (item.FINE_GIUSTIFICATIVO.HasValue)
                        {
                            periodo += " - " + item.FINE_GIUSTIFICATIVO.GetValueOrDefault().ToString("dd/MM/yyyy");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                periodo = "";
            }

            return periodo;
        }

        public static string GetDataDALByIdMatRichiesta(int idRichiesta)
        {
            string periodo = "";

            try
            {
                IncentiviEntities db = new IncentiviEntities();

                var item = db.XR_MAT_RICHIESTE.Where(w => w.ID.Equals(idRichiesta)).FirstOrDefault();
                if (item != null)
                {
                    if (item.DATA_INIZIO_MATERNITA.HasValue)
                    {
                        periodo = item.DATA_INIZIO_MATERNITA.GetValueOrDefault().ToString("dd/MM/yyyy");
                    }
                    else if (item.INIZIO_GIUSTIFICATIVO.HasValue)
                    {
                        periodo = item.INIZIO_GIUSTIFICATIVO.GetValueOrDefault().ToString("dd/MM/yyyy");
                    }
                }
            }
            catch (Exception ex)
            {
                periodo = "";
            }

            return periodo;
        }

        public static string GetDataALByIdMatRichiesta(int idRichiesta)
        {
            string periodo = "";

            try
            {
                IncentiviEntities db = new IncentiviEntities();

                var item = db.XR_MAT_RICHIESTE.Where(w => w.ID.Equals(idRichiesta)).FirstOrDefault();
                if (item != null)
                {
                    if (item.DATA_FINE_MATERNITA.HasValue)
                    {
                        periodo = item.DATA_FINE_MATERNITA.GetValueOrDefault().ToString("dd/MM/yyyy");
                    }
                    else if (item.FINE_GIUSTIFICATIVO.HasValue)
                    {
                        periodo = item.FINE_GIUSTIFICATIVO.GetValueOrDefault().ToString("dd/MM/yyyy");
                    }
                }
            }
            catch (Exception ex)
            {
                periodo = "";
            }

            return periodo;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiPerEsportazione()
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            List<XR_DEM_DOCUMENTI> items = null;
            try
            {
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                IncentiviEntities db = new IncentiviEntities();

                items = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva).ToList();

                if (items != null && items.Any())
                {
                    string abilKey = "DEMA";
                    string currentMatr = CommonHelper.GetCurrentUserMatricola();
                    var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, null, abilKey);

                    List<string> matricoleDaFiltrare = new List<string>();
                    List<string> matricoleConsentite = new List<string>();

                    matricoleDaFiltrare.AddRange(items.Select(x => x.MatricolaDestinatario).ToList());

                    if (matricoleDaFiltrare != null && matricoleDaFiltrare.Any())
                    {
                        tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricoleDaFiltrare.Contains(w.COD_MATLIBROMAT));
                        matricoleConsentite.AddRange(tmpSint.Select(w => w.COD_MATLIBROMAT).ToList());
                    }
                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    foreach (var i in items.Where(w => matricoleConsentite.Contains(w.MatricolaDestinatario)).ToList())
                    {

                        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(i);
                        result.Add(newItem);
                    }
                }

                if (result != null && result.Any())
                {
                    result = result.DistinctBy(w => w.Id).ToList();

                    //result = result.Where(w => w.Avanzamento != 100).ToList();

                    result = result.OrderBy(w => w.Avanzamento).ThenByDescending(w => w.MatricolaDestinatario).ToList();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static FileContentResult ExportSituazioneDocumentale()
        {
            FileContentResult result = null;

            try
            {
                XLWorkbook workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Riepilogo");
                MemoryStream ms = new MemoryStream();
                int row = 1;
                worksheet.Cell(row, 1).SetValue("Matricola");
                worksheet.Cell(row, 2).SetValue("Nominativo");
                worksheet.Cell(row, 3).SetValue("Dal");
                worksheet.Cell(row, 4).SetValue("Al");
                worksheet.Cell(row, 5).SetValue("Servizio contabile");
                worksheet.Cell(row, 6).SetValue("Mansione");
                worksheet.Cell(row, 7).SetValue("Sedegapp");
                worksheet.Cell(row, 8).SetValue("Stato avanzamento");
                worksheet.Row(row).Style.Font.Bold = true;


                List<XR_DEM_DOCUMENTI_EXT> documenti = new List<XR_DEM_DOCUMENTI_EXT>();
                documenti = GetDocumentiPerEsportazione();

                if (documenti != null && documenti.Any())
                {
                    row = 2;
                    foreach (var d in documenti)
                    {
                        worksheet.Cell(row, 1).SetValue(d.MatricolaDestinatario);
                        worksheet.Cell(row, 2).SetValue(d.NominativoUtenteDestinatario);

                        string periodoDal = "Creato il " + d.DataCreazione.ToString("dd/MM/yyyy");
                        string periodoAl = "Creato il " + d.DataCreazione.ToString("dd/MM/yyyy");

                        if (!String.IsNullOrEmpty(d.CustomDataJSON))
                        {
                            List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(d.CustomDataJSON);
                            List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

                            if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                            {
                                foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
                                {
                                    listaPiatta.AddRange(GetListaPiatta(obj));
                                }

                                if (listaPiatta != null && listaPiatta.Any())
                                {
                                    var findInizioMat = listaPiatta.Where(w => w.DBRefAttribute == "DATA_INIZIO_MATERNITA").FirstOrDefault();
                                    if (findInizioMat != null && !String.IsNullOrEmpty(findInizioMat.Valore))
                                    {
                                        periodoDal = findInizioMat.Valore;
                                        periodoAl = "";
                                        findInizioMat = listaPiatta.Where(w => w.DBRefAttribute == "DATA_FINE_MATERNITA").FirstOrDefault();
                                        if (findInizioMat != null)
                                        {
                                            periodoAl = findInizioMat.Valore;
                                        }
                                    }
                                    else
                                    {
                                        var inizio = listaPiatta.Where(w => w.DBRefAttribute == "INIZIO_GIUSTIFICATIVO").FirstOrDefault();
                                        var fine = listaPiatta.Where(w => w.DBRefAttribute == "FINE_GIUSTIFICATIVO").FirstOrDefault();

                                        if (inizio != null && !String.IsNullOrEmpty(inizio.Valore))
                                        {
                                            periodoDal = inizio.Valore;
                                            listaPiatta.Remove(inizio);
                                            periodoAl = "";
                                            if (fine != null && !String.IsNullOrEmpty(fine.Valore))
                                            {
                                                periodoAl = fine.Valore;
                                                listaPiatta.Remove(fine);
                                            }

                                            if (listaPiatta.Count(w => w.DBRefAttribute == "INIZIO_GIUSTIFICATIVO") > 0)
                                            {
                                                periodoAl += " (+" + listaPiatta.Count(w => w.DBRefAttribute == "INIZIO_GIUSTIFICATIVO") + ")";
                                            }
                                        }
                                        else
                                        {
                                            if (d.Id_Richiesta.HasValue)
                                            {
                                                periodoDal = GetDataDALByIdMatRichiesta(d.Id_Richiesta.GetValueOrDefault());
                                                periodoAl = GetDataALByIdMatRichiesta(d.Id_Richiesta.GetValueOrDefault());
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var findInizioMat = objModuloValorizzato.Where(w => w.DBRefAttribute == "DATA_INIZIO_MATERNITA").FirstOrDefault();
                                if (findInizioMat != null && !String.IsNullOrEmpty(findInizioMat.Valore))
                                {
                                    periodoDal = findInizioMat.Valore;
                                    periodoAl = "";
                                    findInizioMat = objModuloValorizzato.Where(w => w.DBRefAttribute == "DATA_FINE_MATERNITA").FirstOrDefault();
                                    if (findInizioMat != null && !String.IsNullOrEmpty(findInizioMat.Valore))
                                    {
                                        periodoAl = findInizioMat.Valore;
                                    }
                                }
                                else
                                {
                                    findInizioMat = objModuloValorizzato.Where(w => w.DBRefAttribute == "INIZIO_GIUSTIFICATIVO").FirstOrDefault();
                                    if (findInizioMat != null && !String.IsNullOrEmpty(findInizioMat.Valore))
                                    {
                                        periodoDal = findInizioMat.Valore;
                                        periodoAl = "";
                                        findInizioMat = objModuloValorizzato.Where(w => w.DBRefAttribute == "FINE_GIUSTIFICATIVO").FirstOrDefault();
                                        if (findInizioMat != null && !String.IsNullOrEmpty(findInizioMat.Valore))
                                        {
                                            periodoAl = findInizioMat.Valore;
                                        }
                                    }
                                    else
                                    {
                                        if (d.Id_Richiesta.HasValue)
                                        {
                                            periodoDal = GetDataDALByIdMatRichiesta(d.Id_Richiesta.GetValueOrDefault());
                                            periodoAl = GetDataALByIdMatRichiesta(d.Id_Richiesta.GetValueOrDefault());
                                        }
                                    }
                                }
                            }
                        }

                        worksheet.Cell(row, 3).SetValue(periodoDal);
                        worksheet.Cell(row, 4).SetValue(periodoAl);

                        DatiAggiuntivi rispostaDIP = null;
                        using (var sediDB = new PERSEOEntities())
                        {
                            string queryDIP = "SELECT t0.[matricola_dp] as Matricola " +
                                                ",t12.[cod_sede] + ' ' + t12.[desc_sede] as SedeGapp " +
                                                ",t2.[cod_mansione] + ' ' + t2.[desc_mansione] as Mansione " +
                                                ",t0.[cod_serv_cont] + t0.[cod_serv_inquadram] + ' ' + t10.[desc_breve] as Servizio " +
                                                "FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                                                "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON(t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                                                "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_MANSIONE] " +
                                                "        t2 on(t2.[sky_mansione] = t1.[sky_mansione]) " +
                                                "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] " +
                                                "        t3 ON(t3.[sky_livello_categ] = t1.[sky_livello_categ]) " +
                                                "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SERVIZIO_CONTABILE] " +
                                                "        t10 on(t1.sky_servizio_contabile= t10.sky_serv_cont) " +
                                                "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA] " +
                                                "        t11 on(t1.sky_categoria = t11.sky_categoria) " +
                                                "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SEDE] " +
                                                "        t12 on(t1.sky_sede = t12.sky_sede) " +
                                                "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_INSEDIAMENTO] " +
                                                "        t13 on(t0.cod_insediamento_ubicazione = t13.cod_insediamento) " +
                                                "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CONTRATTO_UNICO] " +
                                                "        t14 ON(t1.[SKY_CONTRATTO] = t14.[SKY_CONTRATTO]) " +
                                                "where " +
                                                "t0.matricola_dp='##MATRICOLA##' and t1.[flg_ultimo_record]='$' ";

                            queryDIP = queryDIP.Replace("##MATRICOLA##", d.MatricolaDestinatario);

                            rispostaDIP = sediDB.Database.SqlQuery<DatiAggiuntivi>(queryDIP).FirstOrDefault();
                        }

                        if (rispostaDIP != null)
                        {
                            worksheet.Cell(row, 5).SetValue(rispostaDIP.Servizio);
                            worksheet.Cell(row, 6).SetValue(rispostaDIP.Mansione);
                            worksheet.Cell(row, 7).SetValue(rispostaDIP.SedeGapp);
                        }

                        worksheet.Cell(row, 8).SetValue(d.Avanzamento);
                        row++;
                    }
                }

                worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Columns().AdjustToContents();
                worksheet.Rows().AdjustToContents();

                worksheet.Range(1, 1, row - 1, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(1, 1, row - 1, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

                workbook.SaveAs(ms);
                ms.Position = 0;

                result = new FileContentResult(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public static IQueryable<XR_DEM_DOCUMENTI> XR_DEM_Documenti_Filter_Mese(IQueryable<XR_DEM_DOCUMENTI> input, int mese)
        {
            IQueryable<XR_DEM_DOCUMENTI> result = input;

            List<XR_DEM_DOCUMENTI> _tempResult = new List<XR_DEM_DOCUMENTI>();

            try
            {
                DateTime temp;
                var documenti = input.ToList();
                if (documenti == null || !documenti.Any())
                {
                    return null;
                }

                foreach (var d in documenti)
                {
                    DateTime? dataDA = null;
                    DateTime? dataA = null;
                    if (!String.IsNullOrEmpty(d.CustomDataJSON))
                    {
                        List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(d.CustomDataJSON);
                        List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

                        if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                        {
                            foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
                            {
                                listaPiatta.AddRange(GetListaPiatta(obj));
                            }

                            if (listaPiatta != null && listaPiatta.Any())
                            {
                                bool altriElementi = true;
                                do
                                {
                                    var findInizioMat = listaPiatta.Where(w => w.DBRefAttribute == "DATA_INIZIO_MATERNITA").FirstOrDefault();
                                    if (findInizioMat != null && !String.IsNullOrEmpty(findInizioMat.Valore))
                                    {
                                        string dt = findInizioMat.Valore;
                                        listaPiatta.Remove(findInizioMat);
                                        if (!String.IsNullOrEmpty(dt))
                                        {
                                            if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                                throw new Exception("Errore in conversione data: " + dt);
                                            dataDA = temp;
                                        }

                                        findInizioMat = listaPiatta.Where(w => w.DBRefAttribute == "DATA_FINE_MATERNITA").FirstOrDefault();
                                        dt = findInizioMat.Valore;
                                        listaPiatta.Remove(findInizioMat);
                                        if (!String.IsNullOrEmpty(dt))
                                        {
                                            if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                                throw new Exception("Errore in conversione data: " + dt);
                                            dataA = temp;
                                        }
                                        altriElementi = true;
                                    }
                                    else
                                    {
                                        findInizioMat = listaPiatta.Where(w => w.DBRefAttribute == "INIZIO_GIUSTIFICATIVO").FirstOrDefault();
                                        if (findInizioMat != null && !String.IsNullOrEmpty(findInizioMat.Valore))
                                        {
                                            listaPiatta.Remove(findInizioMat);
                                            string dt = findInizioMat.Valore;
                                            if (!String.IsNullOrEmpty(dt))
                                            {
                                                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                                    throw new Exception("Errore in conversione data: " + dt);
                                                dataDA = temp;
                                            }

                                            findInizioMat = listaPiatta.Where(w => w.DBRefAttribute == "FINE_GIUSTIFICATIVO").FirstOrDefault();
                                            dt = findInizioMat.Valore;
                                            listaPiatta.Remove(findInizioMat);
                                            if (!String.IsNullOrEmpty(dt))
                                            {
                                                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                                    throw new Exception("Errore in conversione data: " + dt);
                                                dataA = temp;
                                            }
                                            altriElementi = true;
                                        }
                                        else
                                        {
                                            if (d.Id_Richiesta.HasValue)
                                            {
                                                string periodoDal = GetDataDALByIdMatRichiesta(d.Id_Richiesta.GetValueOrDefault());
                                                string periodoAl = GetDataALByIdMatRichiesta(d.Id_Richiesta.GetValueOrDefault());

                                                if (!String.IsNullOrEmpty(periodoDal))
                                                {
                                                    if (!DateTime.TryParseExact(periodoDal, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                                        throw new Exception("Errore in conversione data: " + periodoDal);
                                                    dataDA = temp;
                                                }

                                                if (!String.IsNullOrEmpty(periodoAl))
                                                {
                                                    if (!DateTime.TryParseExact(periodoAl, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                                        throw new Exception("Errore in conversione data: " + periodoAl);
                                                    dataA = temp;
                                                }
                                            }
                                            altriElementi = false;
                                        }
                                    }

                                    int meseDA = dataDA.GetValueOrDefault().Month;
                                    int meseA = dataA.GetValueOrDefault().Month;
                                    if (meseDA <= mese && mese <= meseA)
                                    {
                                        _tempResult.Add(d);
                                    }

                                } while (altriElementi);
                            }
                        }
                        else
                        {
                            var findInizioMat = objModuloValorizzato.Where(w => w.DBRefAttribute == "DATA_INIZIO_MATERNITA").FirstOrDefault();
                            if (findInizioMat != null && !String.IsNullOrEmpty(findInizioMat.Valore))
                            {
                                string dt = findInizioMat.Valore;
                                if (!String.IsNullOrEmpty(dt))
                                {
                                    if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                        throw new Exception("Errore in conversione data: " + dt);
                                    dataDA = temp;
                                }

                                findInizioMat = objModuloValorizzato.Where(w => w.DBRefAttribute == "DATA_FINE_MATERNITA").FirstOrDefault();
                                dt = findInizioMat.Valore;
                                if (!String.IsNullOrEmpty(dt))
                                {
                                    if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                        throw new Exception("Errore in conversione data: " + dt);
                                    dataA = temp;
                                }
                            }
                            else
                            {
                                findInizioMat = objModuloValorizzato.Where(w => w.DBRefAttribute == "INIZIO_GIUSTIFICATIVO").FirstOrDefault();
                                if (findInizioMat != null && !String.IsNullOrEmpty(findInizioMat.Valore))
                                {
                                    string dt = findInizioMat.Valore;
                                    if (!String.IsNullOrEmpty(dt))
                                    {
                                        if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                            throw new Exception("Errore in conversione data: " + dt);
                                        dataDA = temp;
                                    }

                                    findInizioMat = objModuloValorizzato.Where(w => w.DBRefAttribute == "FINE_GIUSTIFICATIVO").FirstOrDefault();
                                    dt = findInizioMat.Valore;
                                    if (!String.IsNullOrEmpty(dt))
                                    {
                                        if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                            throw new Exception("Errore in conversione data: " + dt);
                                        dataA = temp;
                                    }
                                }
                                else
                                {
                                    if (d.Id_Richiesta.HasValue)
                                    {
                                        string periodoDal = GetDataDALByIdMatRichiesta(d.Id_Richiesta.GetValueOrDefault());
                                        string periodoAl = GetDataALByIdMatRichiesta(d.Id_Richiesta.GetValueOrDefault());

                                        if (!String.IsNullOrEmpty(periodoDal))
                                        {
                                            if (!DateTime.TryParseExact(periodoDal, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                                throw new Exception("Errore in conversione data: " + periodoDal);
                                            dataDA = temp;
                                        }

                                        if (!String.IsNullOrEmpty(periodoAl))
                                        {
                                            if (!DateTime.TryParseExact(periodoAl, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                                throw new Exception("Errore in conversione data: " + periodoAl);
                                            dataA = temp;
                                        }
                                    }
                                }
                            }

                            int meseDA = dataDA.GetValueOrDefault().Month;
                            int meseA = dataA.GetValueOrDefault().Month;
                            if (meseDA <= mese && mese <= meseA)
                            {
                                _tempResult.Add(d);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _tempResult = null;
            }

            result = _tempResult.AsQueryable();

            return result;
        }

        public static object GetCodiceServizioDaHRDW(string filter, string value)
        {
            return AnagraficaManager.GetServizi(filter, value, true);
        }

        public static string GetSedeByMatricola(string matricola, int idPersona)
        {
            string result = null;
            IncentiviEntities dbCzn = new IncentiviEntities();

            if (!String.IsNullOrEmpty(matricola))
            {
                var item = dbCzn.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
                result = item.COD_SEDE;
            }
            else if (idPersona > 0)
            {
                var item = dbCzn.SINTESI1.FirstOrDefault(x => x.ID_PERSONA == idPersona);
                result = item.COD_SEDE;
            }
            return result;
        }

        public static List<SelectListItem> GetSedi(string filter, string value, bool loadAll = false, AbilSedi abil = null, bool addCodDes = true, bool addDefault = true, bool addGroup = false, string matricola = null, bool soloStessaCitta = false, bool setSelected = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            IncentiviEntities db = new IncentiviEntities();

            if (String.IsNullOrEmpty(matricola))
            {
                matricola = UtenteHelper.Matricola();
            }

            IQueryable<myRaiData.Incentivi.SEDE> tmp = db.SEDE.Where(x => x.DTA_INIZIO <= DateTime.Today && x.DTA_FINE >= DateTime.Today);
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = tmp.Where(x => x.COD_IMPRESA == "0" && x.COD_SEDE == value);
                else
                    tmp = tmp.Where(x => x.COD_IMPRESA == "0" && x.DES_SEDE.StartsWith(filter));
            }

            string codiceCitta = null;

            if (soloStessaCitta)
            {
                var item = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
                codiceCitta = item.COD_CITTASEDE;

                tmp = tmp.Where(x => x.COD_CITTA == codiceCitta);
            }

            if (abil != null && abil.HasFilter)
            {
                if (abil.SediIncluse.Any())
                    tmp = tmp.Where(x => abil.SediIncluse.Any(y => y == x.COD_SEDE || y.StartsWith(x.COD_SEDE.Substring(0, 2))));


                if (abil.SediEscluse.Any())
                    tmp = tmp.Where(x => !abil.SediEscluse.Any(y => y == x.COD_SEDE || y.StartsWith(x.COD_SEDE.Substring(0, 2))));
            }

            tmp = tmp.Where(x => x.COD_SEDE != "***");

            if (addDefault && loadAll)
                result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            result.AddRange(tmp.OrderBy(x => x.COD_SEDE).ToList().Select(x => new SelectListItem { Value = x.COD_SEDE, Text = (addCodDes ? x.COD_SEDE + " - " : "") + CezanneHelper.GetDes(x.COD_SEDE, x.DES_SEDE).TitleCase() }));

            if (!String.IsNullOrWhiteSpace(value) && setSelected)
            {
                var item = result.Where(w => w.Value == value).FirstOrDefault();
                item.Selected = true;
            }

            return result;
        }

        public static string GetDescrizioneEvento(string evDescription, bool descrizioneEstesa = false)
        {
            /*
                30	ASSEGNAZIONE
                3R	ASSEGNAZIONE TEMPORANEA
                3S	FINE ASSEGNAZIONE TEMPORANEA (anche anticipata)

                3W	TRASFERIMENTO DEFINITIVO
                3T	TRASFERIMENTO TEMPORANEO
                3U	FINE TRASFERIMENTO TEMPORANEO (anche anticipata)
                34	TRASFERIMENTO A DOMANDA

                3K	PROROGA ASSEGNAZIONE
                3Y	PROROGA TRASFERIMENTO

                31	DISTACCO
                3X	FINE DISTACCO

                3Z	VARIAZIONE SEZIONE
             */
            string descrizione = string.Empty;

            switch (evDescription)
            {
                case "30":
                    descrizione = "ASSEGNAZIONE DEFINITIVA";
                    break;
                case "3R":
                    descrizione = "ASSEGNAZIONE TEMPORANEA";
                    break;
                case "3S":
                    descrizione = "FINE ASSEGNAZIONE TEMPORANEA";
                    break;
                case "3W":
                    descrizione = "TRASFERIMENTO DEFINITIVO";
                    break;
                case "34":
                    descrizione = "TRASFERIMENTO A DOMANDA";
                    break;
                case "3T":
                    descrizione = "TRASFERIMENTO TEMPORANEO";
                    break;
                case "3U":
                    descrizione = "FINE TRASFERIMENTO TEMPORANEO";
                    break;
                case "31":
                    descrizione = "DISTACCO";
                    break;
                case "3X":
                    descrizione = "FINE DISTACCO";
                    break;
                case "3Z":
                    descrizione = "CAMBIO SEZIONE";
                    break;
                case "3K":
                    descrizione = "PROROGA ASSEGNAZIONE TEMPORANEA";
                    break;
                case "3Y":
                    descrizione = "PROROGA TRASFERIMENTO TEMPORANEO";
                    break;
                default:
                    return String.Empty;
            }

            if (descrizioneEstesa)
            {
                descrizione = String.Format("{0} ({1})", descrizione, evDescription);
            }

            return descrizione;
        }

        #region ASSUNZIONI
        public static CreaDocumentoResponse CreaDocumento_Da_Assunzione(CreaDocumentoAssunzioneRequest request)
        {
            const string COD_TIPOLOGIA_DOC = "VSRUO";
            const int ID_STATO = 20;
            const string CODICE = "ASS";
            const string COD_WKF_TIPOLOGIA = "DEMDOC_" + COD_TIPOLOGIA_DOC + "_" + CODICE;

            CreaDocumentoResponse result = new CreaDocumentoResponse()
            {
                Esito = true,
                Errore = string.Empty,
                Documento = new XR_DEM_DOCUMENTI_EXT()
            };

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                RichiestaDoc richiesta = new RichiestaDoc();
                richiesta.Documento = new XR_DEM_DOCUMENTI();
                XR_DEM_TIPIDOC_COMPORTAMENTO comportamento = null;

                int id_Tipo_Doc = 0;

                string cod_tipo_doc = "";
                string descrizione = "";

                var XR_DEM_TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Codice.Equals(CODICE)).FirstOrDefault();

                if (XR_DEM_TIPI_DOCUMENTO != null)
                {
                    id_Tipo_Doc = XR_DEM_TIPI_DOCUMENTO.Id;
                    cod_tipo_doc = XR_DEM_TIPI_DOCUMENTO.Codice.Trim();
                    descrizione = XR_DEM_TIPI_DOCUMENTO.Descrizione.Trim();
                }
                else
                {
                    throw new Exception("Tipo documento non trovato");
                }

                comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(
                    w => w.Codice_Tipologia_Documentale.Equals(COD_TIPOLOGIA_DOC) &&
                    w.Codice_Tipo_Documento.Equals(cod_tipo_doc)).FirstOrDefault();

                if (comportamento == null)
                {
                    throw new Exception("Comportamento non trovato");
                }

                #region CREAZIONE DATI JSON

                List<AttributiAggiuntivi> objBASE = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(comportamento.CustomDataJSON);
                List<AttributiAggiuntivi> objFinale = new List<AttributiAggiuntivi>();

                foreach (var o in objBASE)
                {
                    object valore = GetPropValue(request.DatiAssunzione, o.DBRefAttribute);

                    // verifica la tipologia dell'elemento selezionato
                    var tipo = o.Tipo;

                    if (valore == null)
                    {
                        o.Valore = "";
                    }
                    else
                    {
                        o.Valore = valore.ToString();
                    }

                    if (tipo == TipologiaAttributoEnum.SelectEstesa)
                    {
                        string tag = o.TagSINTESI1;

                        if (!String.IsNullOrEmpty(tag))
                        {
                            switch (tag)
                            {
                                case "DES_SEDE":
                                    o.Title = GetDescrizioneSede(o.Valore);
                                    break;
                                case "DES_SERVIZIO":
                                    o.Title = GetDescrizioneServizio(o.Valore);
                                    break;
                                case "COD_UNITAORG":
                                    o.Title = GetDescrizioneSezione_UnitaOrganizzativa(o.Valore);
                                    break;
                                case "COD_ASSINFORTUNI":
                                    o.Title = GetDescrizioneAssicurazioneInfortuni(o.Valore);
                                    break;
                                case "COD_CAUSALEMOV":
                                    o.Title = GetDescrizioneCausaleAssunzione(o.Valore);
                                    break;
                                case "COD_CANASSUNZ":
                                    o.Title = GetDescrizioneModalitaReclutamento(o.Valore);
                                    break;
                                case "COD_EVQUAL":
                                    o.Title = GetDescrizioneTipoAssunzione(o.Valore);
                                    break;
                                case "COD_RUOLO":
                                    o.Title = GetDescrizioneMansione(o.Valore);
                                    break;
                                case "COD_TIPOMINIMO":
                                    o.Title = GetDescrizioneTipoMinimo(o.Valore);
                                    break;
                                case "COD_TPCNTR":
                                    o.Title = GetDescrizioneFormaContratto(o.Valore);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    if (tipo == TipologiaAttributoEnum.Radio)
                    {
                        var setChecked = o.SelectListItems.Where(w => w.Value == o.Valore).FirstOrDefault();
                        if (setChecked != null)
                        {
                            setChecked.Selected = true;
                            o.Checked = true;
                            o.Title = o.Valore;
                        }
                    }

                    o.ValoreInModifica = o.Valore;
                    objFinale.Add(o);
                }

                string JSON = JsonConvert.SerializeObject(objFinale);

                #endregion

                XR_WKF_TIPOLOGIA xr_wkf_tipologia = new XR_WKF_TIPOLOGIA();
                xr_wkf_tipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(COD_WKF_TIPOLOGIA)).FirstOrDefault();

                string matricolaApprovatore = "451598";
                string matricolaFirma = "103650";

                richiesta.Documento = new XR_DEM_DOCUMENTI()
                {
                    Descrizione = descrizione,
                    Id_Stato = ID_STATO,
                    Id_Tipo_Doc = id_Tipo_Doc,
                    MatricolaCreatore = request.MatricolaCreatore,
                    IdPersonaCreatore = CezanneHelper.GetIdPersona(request.MatricolaCreatore),
                    MatricolaDestinatario = request.DatiAssunzione.Matricola,
                    IdPersonaDestinatario = CezanneHelper.GetIdPersona(request.DatiAssunzione.Matricola),
                    Id_WKF_Tipologia = xr_wkf_tipologia.ID_TIPOLOGIA,
                    Cod_Tipologia_Documentale = COD_TIPOLOGIA_DOC,
                    MatricolaApprovatore = matricolaApprovatore,
                    IdPersonaApprovatore = CezanneHelper.GetIdPersona(matricolaApprovatore),
                    MatricolaFirma = matricolaFirma,
                    IdPersonaFirma = CezanneHelper.GetIdPersona(matricolaFirma),
                    MatricolaIncaricato = null,
                    Id = -1,
                    CustomDataJSON = JSON,
                    PraticaAttiva = true
                };

                List<XR_DEM_VERSIONI_DOCUMENTO> versioni = new List<XR_DEM_VERSIONI_DOCUMENTO>();

                XR_DEM_DOCUMENTI doc = null;

                doc = new XR_DEM_DOCUMENTI()
                {
                    Descrizione = richiesta.Documento.Descrizione,
                    DataCreazione = DateTime.Now,
                    Id_Stato = richiesta.Documento.Id_Stato,
                    Cod_Tipologia_Documentale = richiesta.Documento.Cod_Tipologia_Documentale,
                    Id_WKF_Tipologia = richiesta.Documento.Id_WKF_Tipologia,
                    MatricolaCreatore = richiesta.Documento.MatricolaCreatore,
                    IdPersonaCreatore = richiesta.Documento.IdPersonaCreatore,
                    MatricolaDestinatario = richiesta.Documento.MatricolaDestinatario,
                    IdPersonaDestinatario = richiesta.Documento.IdPersonaDestinatario,
                    Note = richiesta.Documento.Note,
                    Id_Tipo_Doc = richiesta.Documento.Id_Tipo_Doc,
                    MatricolaApprovatore = richiesta.Documento.MatricolaApprovatore,
                    MatricolaFirma = richiesta.Documento.MatricolaFirma,
                    MatricolaIncaricato = richiesta.Documento.MatricolaIncaricato,
                    CustomDataJSON = richiesta.Documento.CustomDataJSON,
                    PraticaAttiva = true,
                    Id = -1
                };

                int tempId = 0;

                #region CARICAMENTO FILES

                List<int> id_allegati = new List<int>();
                foreach (int id_file in request.Files)
                {
                    var f = FileAssunzioneManager.GetFile(id_file);
                    if (!f.Esito && !String.IsNullOrEmpty(f.Errore))
                    {
                        throw new Exception(f.Errore);
                    }
                    else if (!f.Esito)
                    {
                        throw new Exception("Impossibile importare uno o più files");
                    }
                    else if (f.Esito && (f.Files == null || !f.Files.Any()))
                    {
                        throw new Exception("File non trovato");
                    }
                    else
                    {
                        var file_item = f.Files.FirstOrDefault();
                        byte[] data = file_item.ContentByte;
                        int length = file_item.Length;
                        string est = Path.GetExtension(file_item.NomeFile);
                        string tipoFile = MimeTypeMap.GetMimeType(est);
                        string jsonStringProtocollo = null;

                        if (file_item.Tipologia.Trim() == "ASSGEN")
                        {
                            List<PosizioneProtocolloOBJ> obj = new List<PosizioneProtocolloOBJ>();

                            float protL = 83.5f;
                            float protTop = 112.0f;
                            int pagProt = 1;

                            obj.Add(new PosizioneProtocolloOBJ()
                            {
                                Oggetto = "Protocollo",
                                PosizioneLeft = protL,
                                PosizioneTop = protTop,
                                NumeroPagina = pagProt
                            });

                            float dataLeft = 123.5f;
                            float dataTop = 158.5f;
                            int dataPagina = 1;

                            obj.Add(new PosizioneProtocolloOBJ()
                            {
                                Oggetto = "Data",
                                PosizioneLeft = dataLeft,
                                PosizioneTop = dataTop,
                                NumeroPagina = dataPagina
                            });

                            float firmaLeft = 332.5f;
                            float firmaTop = 453.0f;
                            int firmaPagina = 1;

                            obj.Add(new PosizioneProtocolloOBJ()
                            {
                                Oggetto = "Firma",
                                PosizioneLeft = firmaLeft,
                                PosizioneTop = firmaTop,
                                NumeroPagina = firmaPagina
                            });

                            jsonStringProtocollo = JsonConvert.SerializeObject(obj);
                        }

                        XR_ALLEGATI allegato = new XR_ALLEGATI()
                        {
                            NomeFile = file_item.NomeFile,
                            MimeType = tipoFile,
                            Length = length,
                            ContentByte = data,
                            IsPrincipal = (file_item.Tipologia.Trim() == "ASSGEN"),
                            PosizioneProtocollo = jsonStringProtocollo
                        };

                        db.XR_ALLEGATI.Add(allegato);
                        db.SaveChanges();
                        id_allegati.Add(allegato.Id);
                    }
                }

                foreach (var idAllegato in id_allegati)
                {
                    bool exist = db.XR_DEM_ALLEGATI_VERSIONI.Count(w => w.IdAllegato.Equals(idAllegato)) > 0;
                    // Se non esiste allora deve creare l'associazione
                    if (!exist)
                    {
                        tempId--;
                        XR_DEM_VERSIONI_DOCUMENTO version = new XR_DEM_VERSIONI_DOCUMENTO()
                        {
                            N_Versione = 1,
                            DataUltimaModifica = DateTime.Now,// da valutare se mettere questa data o quella di creazione del documento
                            Id_Documento = doc.Id,
                            Id = tempId
                        };

                        db.XR_DEM_VERSIONI_DOCUMENTO.Add(version);

                        XR_DEM_ALLEGATI_VERSIONI associativa = new XR_DEM_ALLEGATI_VERSIONI()
                        {
                            IdAllegato = idAllegato,
                            IdVersione = version.Id
                        };
                        db.XR_DEM_ALLEGATI_VERSIONI.Add(associativa);
                    }
                }

                #endregion

                db.XR_DEM_DOCUMENTI.Add(doc);
                CezanneHelper.GetCampiFirma(out CampiFirma campiFirma);
                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = campiFirma.CodTermid,
                    COD_USER = campiFirma.CodUser,
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = doc.Id,
                    ID_PERSONA = richiesta.Documento.IdPersonaCreatore,
                    ID_STATO = doc.Id_Stato,
                    ID_TIPOLOGIA = doc.Id_WKF_Tipologia,
                    NOMINATIVO = "CreaDocumento_Da_Assunzione",
                    VALID_DTA_INI = DateTime.Now,
                    TMS_TIMESTAMP = campiFirma.Timestamp
                });

                db.SaveChanges();

                // converte il documento in un XR_DEM_DOCUMENTI_EXT
                result.Documento = new XR_DEM_DOCUMENTI_EXT();
                result.Documento = ConvertiInXR_DEM_DOCUMENTI_EXT(doc);
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Recupero dell'oggetto XR_DEM_DOCUMENTI_EXT a partire dall'id del documento stesso
        /// </summary>
        /// <param name="id">Identificativo univoco del documento di cui si intende ottenere le informazioni</param>
        /// <returns></returns>
        public static XR_DEM_DOCUMENTI_EXT GetDocumento(int id)
        {
            XR_DEM_DOCUMENTI_EXT result = null;
            IncentiviEntities db = new IncentiviEntities();
            XR_DEM_DOCUMENTI doc = db.XR_DEM_DOCUMENTI.Where(w => w.Id == id).FirstOrDefault();

            if (doc != null)
            {
                result = new XR_DEM_DOCUMENTI_EXT();
                result = ConvertiInXR_DEM_DOCUMENTI_EXT(doc);
            }

            return result;
        }
        #endregion

        /// <summary>
        /// Restituisce lo stato in cui si trova una determinata pratica
        /// </summary>
        /// <param name="idPratica">Identificativo univoco della pratica di cui si intende conoscere lo stato</param>
        /// <returns></returns>
        public static StatiDematerializzazioneDocumenti GetStatoPratica(int idPratica)
        {
            StatiDematerializzazioneDocumenti result;
            IncentiviEntities db = new IncentiviEntities();
            var item = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idPratica)).FirstOrDefault();

            result = (StatiDematerializzazioneDocumenti)item.Id_Stato;
            return result;
        }

        #region PER CREAZIONE DOC DA ASSUNZIONI
        public static string GetDescrizioneSede(string code)
        {
            string result = "";
            IncentiviEntities db = new IncentiviEntities();

            myRaiData.Incentivi.SEDE tmp = db.SEDE.Where(x => x.DTA_INIZIO <= DateTime.Today && x.DTA_FINE >= DateTime.Today
                                        && x.COD_IMPRESA == "0"
                                        && x.COD_SEDE == code).FirstOrDefault();

            if (tmp != null)
            {
                //result = tmp.DES_SEDE.Trim();
                string descr = tmp.DES_SEDE.Trim();
                descr = descr.Replace(tmp.COD_SEDE.Trim() + " - ", "");
                descr = String.Format("{0} ({1})", descr, tmp.COD_SEDE.Trim());
                result = descr.Trim();
            }

            return CommonHelper.ToTitleCase(result);
        }

        public static string GetDescrizioneServizio(string code)
        {
            string result = "";
            IncentiviEntities db = new IncentiviEntities();

            IQueryable<myRaiData.Incentivi.XR_TB_SERVIZIO> tmp = db.XR_TB_SERVIZIO.Where(x => x.COD_SERVIZIO.Trim().Length == 2 && x.COD_IMPRESA != null);

            tmp = tmp.Where(x => !db.XR_TB_SERVIZIO_EXT.Any(y => y.COD_SERVIZIO.Trim() == x.COD_SERVIZIO.Trim())
                                || db.XR_TB_SERVIZIO_EXT.Any(y => y.COD_SERVIZIO.Trim() == x.COD_SERVIZIO.Trim()
                                                                    && y.DTA_INIZIO <= DateTime.Today && y.DTA_FINE >= DateTime.Today));

            var item = tmp.Where(x => x.COD_SERVIZIO == code).FirstOrDefault();

            if (item != null)
            {
                //result = item.DES_SERVIZIO.Trim();
                string descr = item.DES_SERVIZIO.Trim();
                descr = descr.Replace(item.COD_SERVIZIO.Trim() + " - ", "");
                descr = String.Format("{0} ({1})", descr, item.COD_SERVIZIO.Trim());
                result = descr.Trim();
            }
            return CommonHelper.ToTitleCase(result);
        }

        public static string GetDescrizioneSezione_UnitaOrganizzativa(string code)
        {
            string result = "";
            TalentiaEntities db = new TalentiaEntities();

            string currentDate = DateTime.Today.ToString("yyyyMMdd");
            IQueryable<myRaiDataTalentia.XR_STR_TSEZIONE> tmp = null;
            tmp = db.XR_STR_TSEZIONE.Where(x => x.data_fine_validita.CompareTo(currentDate) > 0 && x.codice_visibile == code);

            var item = tmp.FirstOrDefault();

            if (item != null)
            {
                result = String.Format("{0} ({1})", CommonHelper.ToTitleCase(item.descrizione_lunga.Trim()), item.codice_visibile?.Trim().ToUpper());
            }

            return result;
        }

        public static string GetDescrizioneAssicurazioneInfortuni(string code)
        {
            string result = "";
            IncentiviEntities db = new IncentiviEntities();

            var tmp = db.XR_TB_ASSINFORTUNI.Where(x => x.COD_ASSINFORTUNI == code).FirstOrDefault();

            if (tmp != null)
            {
                //result = tmp.DES_ASSINFORTUNI.Trim();
                result = String.Format("{0} ({1})", tmp.DES_ASSINFORTUNI.Trim(), tmp.COD_ASSINFORTUNI.Trim());
            }

            return result;
        }

        public static string GetDescrizioneCausaleAssunzione(string code)
        {
            string result = "";
            IncentiviEntities db = new IncentiviEntities();

            var tmp = db.XR_TB_CAUSALEMOV.Where(x => x.COD_CAUSALEMOV == code).FirstOrDefault();

            if (tmp != null)
            {
                //result = tmp.DES_CAUSALEMOV.Trim();
                result = String.Format("{0} ({1})", tmp.DES_CAUSALEMOV.Trim(), tmp.COD_CAUSALEMOV.Trim());
            }

            return result;
        }

        public static string GetDescrizioneModalitaReclutamento(string code)
        {
            string result = "";
            IncentiviEntities db = new IncentiviEntities();

            var tmp = db.XR_TB_CANASSUNZ.Where(x => x.COD_CANASSUNZ == code).FirstOrDefault();

            if (tmp != null)
            {
                //result = tmp.DES_CANASSUNZ.Trim();
                result = String.Format("{0} ({1})", tmp.DES_CANASSUNZ.Trim(), tmp.COD_CANASSUNZ.Trim());
            }

            return result;
        }

        public static string GetDescrizioneTipoAssunzione(string code)
        {
            string result = "";
            IncentiviEntities db = new IncentiviEntities();

            var tmp = db.TB_EVENTO.Where(x => x.COD_EVQUAL == code).FirstOrDefault();

            if (tmp != null)
            {
                //result = tmp.DES_EVQUAL.Trim();
                result = String.Format("{0} ({1})", tmp.DES_EVQUAL.Trim(), tmp.COD_EVQUAL.Trim());
            }

            return result;
        }

        public static string GetDescrizioneMansione(string code)
        {
            string result = "";
            IncentiviEntities db = new IncentiviEntities();

            IQueryable<myRaiData.Incentivi.RUOLO> tmp = null;
            tmp = db.RUOLO.Where(x => x.COD_RUOLO == code);

            //Mansioni contabili
            tmp = tmp.Where(x => db.RUOLO.Where(y => y.COD_RUOLOAGGREG == "MANC").Select(y => y.COD_RUOLO).Contains(x.COD_RUOLOAGGREG));
            //Filtro validità
            tmp = tmp.Where(x => x.DTA_INIZIO <= DateTime.Today && x.DTA_FINE >= DateTime.Today);

            var item = tmp.FirstOrDefault();
            if (item != null)
            {
                //result = item.DES_RUOLO.Trim();
                string descr = item.DES_RUOLO.Trim();
                descr = descr.Replace(item.COD_RUOLO.Trim() + " - ", "");
                descr = String.Format("{0} ({1})", descr, item.COD_RUOLO.Trim());
                result = descr.Trim();
            }

            return result;
        }

        public static string GetDescrizioneTipoMinimo(string code)
        {
            string result = "";
            IncentiviEntities db = new IncentiviEntities();

            IQueryable<myRaiData.Incentivi.XR_TB_TIPOMINIMO> tmp = null;
            tmp = db.XR_TB_TIPOMINIMO.Where(x => x.COD_TIPOMINIMO == code);

            tmp = tmp.Where(x => x.IND_WEBVISIBLE == "Y");
            var item = tmp.FirstOrDefault();
            if (item != null)
            {
                //result = item.DES_TIPOMINIMO.Trim();
                result = String.Format("{0} ({1})", item.DES_TIPOMINIMO.Trim(), item.COD_TIPOMINIMO.Trim());
            }

            return result;
        }

        public static string GetDescrizioneFormaContratto(string code)
        {
            string result = "";
            IncentiviEntities db = new IncentiviEntities();
            var tmp = db.TB_TPCNTR.Where(x => x.COD_TPCNTR == code).FirstOrDefault();

            if (tmp != null)
            {
                //result = tmp.DES_TPCNTR.Trim();
                result = String.Format("{0} ({1})", tmp.DES_TPCNTR.Trim(), tmp.COD_TPCNTR.Trim());
            }
            return result;
        }

        public static string GetDescrizioneIndennita(string valore)
        {
            string result = "";

            if (String.IsNullOrEmpty(valore))
            {
                return result;
            }

            // prima di tutto deve verificare se valore è una stringa separata da virgola oppure no
            // se è divisa da virgola allora è un array
            List<string> codici = new List<string>();
            codici.AddRange(valore.Split(',').ToList());
            IncentiviEntities db = new IncentiviEntities();

            var indennita = db.XR_TB_INDENNITA.Where(w => codici.Contains(w.COD_INDENNITA)).ToList();

            if (indennita != null && indennita.Any())
            {
                string descr = String.Empty;
                foreach (var i in indennita)
                {
                    descr += String.Format("{0} ({1})\r\n", i.DES_INDENNITA.Trim(), i.COD_INDENNITA.Trim());
                }
                result = descr;
            }

            return CommonHelper.ToTitleCase(result);
        }

        #endregion

        #region CREAZIONE STAMPE
        public static byte[] GeneraPdfVariazione(XR_DEM_DOCUMENTI doc, string codice_evento, DateTime dataInizio, DateTime dataFine)
        {
            byte[] bytes = null;
            try
            {
                string codSede = "";
                string descSede = "";
                string codServizio = "";
                string descServizio = "";
                string codSezione = "";
                string descSezione = "";
                string sesso = "";
                string titolo = "Sig.";
                string nominativo = "";
                string sede = "Roma";
                string descSocieta = "";
                string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                DateTime dataAssunzione = DateTime.Now;
                string categoria = "";

                Image png = iTextSharp.text.Image.GetInstance(_imgPath);

                bool result = SetImpostazioniStampa(doc.MatricolaDestinatario,
                                                ref nominativo,
                                                ref sesso,
                                                ref titolo,
                                                ref sede,
                                                ref descSocieta,
                                                ref png);

                result = DecodificaDatiJson(doc.MatricolaDestinatario,
                    doc.CustomDataJSON,
                    ref codSede,
                    ref descSede,
                    ref codServizio,
                    ref descServizio,
                    ref codSezione,
                    ref descSezione);

                // verifica la società di appartenenza per caricare il logo adatto
                using (IncentiviEntities sdb = new IncentiviEntities())
                {
                    var item = sdb.SINTESI1.Where(w => w.COD_MATLIBROMAT.Equals(doc.MatricolaDestinatario)).FirstOrDefault();

                    if (item != null)
                    {
                        dataAssunzione = item.DTA_ANZCONV.GetValueOrDefault();
                        categoria = item.DES_QUALIFICA;
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new DematerializzazioneManagerPdfPageEventHelper(png);
                    document.Open();
                    PdfContentByte cb = writer.DirectContent;
                    Paragraph p = null;
                    Phrase phrase = new Phrase();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    PdfPTable tableIntestazione = new PdfPTable(6);
                    tableIntestazione.DefaultCell.BorderWidth = 0;
                    tableIntestazione.TotalWidth = 550;
                    tableIntestazione.LockedWidth = true;
                    var tableIntestazioneWidth = new int[] { 30, 107, 137, 110, 134, 30 };
                    tableIntestazione.SetWidths(tableIntestazioneWidth);
                    //0=Left, 1=Centre, 2=Right
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell("MODULO DI VARIAZIONE CONTABILE", 0, 6, 0, myFontCorpoSottolineato, 1));

                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 6, 0, myFontCorpo, 0));

                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(String.Format("Matricola {0}", doc.MatricolaDestinatario), 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));

                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(String.Format("Nominativo {0}", nominativo), 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));

                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(String.Format("Data assunzione {0}", dataAssunzione.ToString("dd/MM/yyyy")), 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));

                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(String.Format("Categoria {0}", categoria), 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));

                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(String.Format("Sede attuale {0}", descSede), 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));

                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(String.Format("Servizio attuale {0}", descServizio), 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));

                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(String.Format("Sezione attuale {0}", descSezione), 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));

                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(String.Format("Tipo variazione {0}", GetDescrizioneTipoVariazione(codice_evento)), 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));

                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(String.Format("Variazione decorrente dal {0}", dataInizio.ToString("dd/MM/yyyy")), 0, 2, 0, myFontCorpo, 0));

                    if (dataFine.Date != new DateTime(2999, 12, 31))
                    {
                        tableIntestazione.AddCell(WriteCellsClass.WriteCell(String.Format("Variazione decorrente al {0}", dataFine.ToString("dd/MM/yyyy")), 0, 2, 0, myFontCorpo, 0));
                    }
                    else
                    {
                        tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontCorpo, 0));
                    }
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 0, myFontCorpo, 0));
                    tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 6, 0, myFontCorpo, 0));

                    document.Add(tableIntestazione);
                    tableIntestazione.FlushContent();
                    tableIntestazione.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    PdfPTable table2 = new PdfPTable(6);
                    table2.DefaultCell.BorderWidth = 0;
                    table2.TotalWidth = 550;
                    table2.LockedWidth = true;
                    var table2Width = new int[] { 30, 107, 137, 110, 134, 30 };
                    table2.SetWidths(table2Width);

                    //0=Left, 1=Centre, 2=Right
                    table2.AddCell(WriteCellsClass.WriteCell("MODULO DI VARIAZIONE CONTABILE", 0, 6, 0, myFontCorpoSottolineato, 1));

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaIntestazione(ref document, ref doc);

                    string par1 = String.Format("Le comunichiamo che, a decorrere dal {0} e fino al {1}, Ella è " +
                        "assegnata temporaneamente presso la Direzione {2}.",
                        dataInizio.ToString("dd/MM/yyyy"),
                        dataFine.ToString("dd/MM/yyyy"),
                        descServizio);
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par1, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par2 = String.Format("Con i migliori saluti.");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par2, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaFirma(4, ref document, ref doc);

                    document.Close();
                    writer.Close();
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return bytes;
        }

        public static byte[] GeneraPdfLetteraVariazione(XR_DEM_DOCUMENTI doc, string codice_evento, DateTime dataInizio, DateTime dataFine)
        {
            byte[] bytes = null;
            try
            {
                /*
                    30	ASSEGNAZIONE
                    3R	ASSEGNAZIONE TEMPORANEA
                    3S	FINE ASSEGNAZIONE TEMPORANEA (anche anticipata)

                    3W	TRASFERIMENTO DEFINITIVO
                    3T	TRASFERIMENTO TEMPORANEO
                    3U	FINE TRASFERIMENTO TEMPORANEO (anche anticipata)
                    34	TRASFERIMENTO A DOMANDA

                    3K	PROROGA ASSEGNAZIONE
                    3Y	PROROGA TRASFERIMENTO

                    31	DISTACCO
                    3X	FINE DISTACCO

                    3Z	VARIAZIONE SEZIONE
                 */
                switch (codice_evento)
                {
                    case "3R":
                        bytes = CreaLettera_AssegnazioneTemporanea(doc, dataInizio, dataFine);
                        break;
                    case "3S":
                        bytes = CreaLettera_FineAnticipataAssegnazioneTemporanea(doc, dataInizio, dataFine);
                        break;
                    case "3T":
                        bytes = CreaLettera_TrasferimentoTemporanea(doc, dataInizio, dataFine);
                        break;
                    case "34":
                        bytes = CreaLettera_TrasferimentoADomanda(doc, dataInizio, dataFine);
                        break;
                    case "3U":
                        bytes = CreaLettera_TrasferimentoTemporaneo(doc, dataInizio, dataFine);
                        break;
                    case "3K":
                        bytes = CreaLettera_ProrogaAssegnazioneTemporanea(doc, dataInizio, dataFine);
                        break;
                    case "3Y":
                        bytes = CreaLettera_ProrogaAssegnazioneTemporanea(doc, dataInizio, dataFine);
                        break;
                    case "31":
                        bytes = CreaLettera_Distacco(doc, dataInizio, dataFine);
                        break;
                    case "3X":
                        bytes = null;
                        break;
                    case "3Z":
                        bytes = CreaLettera_Distacco(doc, dataInizio, dataFine);
                        break;
                    case "":
                        bytes = CreaLettera_FineAnticipataAssegnazioneTemporaneaRiassegnazione(doc, dataInizio, dataFine);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return bytes;
        }

        private static string GetDescrizioneTipoVariazione(string codiceEvento)
        {
            string result = "";
            try
            {
                switch (codiceEvento)
                {
                    case "03R":
                        result = "Assegnazione temporanea";
                        break;
                    case "03S":
                        result = "Fine assegnazione temporanea";
                        break;
                    case "03S_A":
                        result = "Fine assegnazione anticipata";
                        break;
                    case "PAT":
                        result = "Proroga assegnazione temporanea";
                        break;
                    case "030":
                        result = "Assegnazione definitiva";
                        break;
                    case "03T":
                        result = "Trasferimento temporaneo";
                        break;
                    case "03U":
                        result = "Fine trasferimento temporaneo";
                        break;
                    case "031":
                        result = "Distacco";
                        break;
                    case "03Z":
                        result = "Cambio sezione";
                        break;
                    case "005":
                        result = "Trasferimento definitivo";
                        break;
                    case "034":
                        result = "Trasferimento a domanda";
                        break;
                }

                if (codiceEvento.StartsWith("0"))
                {
                    codiceEvento = codiceEvento.Substring(1);
                }

                result = String.Format("{0} ({1})", result, codiceEvento);
            }
            catch (Exception ex)
            {
                result = "";
            }

            return result;
        }

        private static bool SetImpostazioniStampa(string matricola,
            ref string nominativo,
            ref string sesso,
            ref string titolo,
            ref string sede,
            ref string descSocieta,
            ref Image png)
        {
            bool result = true;
            string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
            string codSocieta = "0";

            try
            {
                // verifica la società di appartenenza per caricare il logo adatto
                using (IncentiviEntities sdb = new IncentiviEntities())
                {
                    var item = sdb.SINTESI1.Where(w => w.COD_MATLIBROMAT.Equals(matricola)).FirstOrDefault();

                    if (item != null)
                    {
                        sesso = item.COD_SESSO.Trim();
                        codSocieta = item.COD_IMPRESACR;
                        nominativo = item.Nominativo().Trim();
                        sede = item.DES_CITTASEDE.Trim();

                        if (sesso == "F")
                        {
                            titolo = "Sig.ra";
                        }

                        if (codSocieta == "8")
                        {
                            // Rai Com Spa
                            descSocieta = "Rai Com Spa";
                            _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/Rai Com_Logo.png");
                            png = iTextSharp.text.Image.GetInstance(_imgPath);
                            png.ScaleAbsolute(120, 45);
                        }
                        else if (codSocieta == "B")
                        {
                            // Rai Way
                            descSocieta = "Rai Way";
                            _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/Rai Way_Logo.png");
                            png = iTextSharp.text.Image.GetInstance(_imgPath);
                            png.ScaleAbsolute(120, 45);
                        }
                        else if (codSocieta == "C")
                        {
                            // Rai Cinema
                            descSocieta = "Rai Cinema";
                            _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/Rai Cinema_Logo.png");
                            png = iTextSharp.text.Image.GetInstance(_imgPath);
                            png.ScaleAbsolute(150, 45);
                        }
                        else
                        {
                            if (codSocieta == "0")
                            {
                                descSocieta = "Rai";
                            }
                            else if (codSocieta == "N")
                            {
                                descSocieta = "Rai Net";
                            }
                            else if (codSocieta == "S")
                            {
                                descSocieta = "Rai Sat";
                            }
                            else if (codSocieta == "T")
                            {
                                descSocieta = "Rai Trade";
                            }
                            else if (codSocieta == "Y")
                            {
                                descSocieta = "Rai Int";
                            }
                            else
                            {
                                result = false;
                            }
                            _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                            png = iTextSharp.text.Image.GetInstance(_imgPath);
                            png.ScaleAbsolute(45, 45);
                        }
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        public static bool DecodificaDatiJson(string matricola,
            string json,
            ref string codSede,
            ref string descSede,
            ref string codServizio,
            ref string descServizio,
            ref string codSezione,
            ref string descSezione)
        {
            bool result = false;
            try
            {
                List<AttributiAggiuntivi> objModuloValorizzato = null;
                if (!String.IsNullOrEmpty(json) && json != "[]")
                {
                    objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(json);
                    if (objModuloValorizzato != null)
                    {
                        var DES_SEDE = objModuloValorizzato.Where(w => w.TagSINTESI1 != null && w.TagSINTESI1.ToUpper() == "DES_SEDE").FirstOrDefault();
                        var DES_SERVIZIO = objModuloValorizzato.Where(w => w.TagSINTESI1 != null && w.TagSINTESI1.ToUpper() == "DES_SERVIZIO").FirstOrDefault();
                        var COD_UNITAORG = objModuloValorizzato.Where(w => w.TagSINTESI1 != null && w.TagSINTESI1.ToUpper() == "COD_UNITAORG").FirstOrDefault();

                        DES_SEDE.Valore = DES_SEDE.Valore == "undefined" ? null : DES_SEDE.Valore;
                        DES_SERVIZIO.Valore = DES_SERVIZIO.Valore == "undefined" ? null : DES_SERVIZIO.Valore;
                        COD_UNITAORG.Valore = COD_UNITAORG.Valore == "undefined" ? null : COD_UNITAORG.Valore;

                        codSede = DES_SEDE.Valore;
                        codServizio = DES_SERVIZIO.Valore;
                        codSezione = COD_UNITAORG.Valore;
                        result = true;
                    }
                }

                using (IncentiviEntities sdb = new IncentiviEntities())
                {
                    var item = sdb.SINTESI1.Where(w => w.COD_MATLIBROMAT.Equals(matricola)).FirstOrDefault();

                    if (item != null)
                    {
                        codSede = (String.IsNullOrWhiteSpace(codSede) ? item.COD_SEDE : codSede);
                        codServizio = (String.IsNullOrWhiteSpace(codServizio) ? item.COD_SERVIZIO : codServizio);
                        codSezione = (String.IsNullOrWhiteSpace(codSezione) ? item.COD_UNITAORG : codSezione);
                    }
                    else
                    {
                        result = false;
                    }
                }

                descSede = DematerializzazioneManager.GetDescrizioneSede(codSede);
                descServizio = DematerializzazioneManager.GetDescrizioneServizio(codServizio);
                descSezione = DematerializzazioneManager.GetDescrizioneSezione_UnitaOrganizzativa(codSezione);
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        private static void StampaIntestazione(ref Document document, ref XR_DEM_DOCUMENTI doc)
        {
            string codSede = "";
            string descSede = "";
            string codServizio = "";
            string descServizio = "";
            string codSezione = "";
            string descSezione = "";
            string sesso = "";
            string titolo = "Sig.";
            string nominativo = "";
            string sede = "Roma";
            string descSocieta = "";
            string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
            Image png = iTextSharp.text.Image.GetInstance(_imgPath);

            bool result = SetImpostazioniStampa(doc.MatricolaDestinatario,
                                            ref nominativo,
                                            ref sesso,
                                            ref titolo,
                                            ref sede,
                                            ref descSocieta,
                                            ref png);

            result = DecodificaDatiJson(doc.MatricolaDestinatario,
                doc.CustomDataJSON,
                ref codSede,
                ref descSede,
                ref codServizio,
                ref descServizio,
                ref codSezione,
                ref descSezione);

            Paragraph p = null;
            Phrase phrase = new Phrase();

            phrase = new Phrase();
            phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
            p = new Paragraph(phrase);
            ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
            document.Add(p);

            phrase = new Phrase();
            phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
            p = new Paragraph(phrase);
            ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
            document.Add(p);

            phrase = new Phrase();
            phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
            p = new Paragraph(phrase);
            ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
            document.Add(p);

            PdfPTable tableIntestazione = new PdfPTable(6);
            tableIntestazione.DefaultCell.BorderWidth = 0;
            tableIntestazione.TotalWidth = 550;
            tableIntestazione.LockedWidth = true;
            var tableIntestazioneWidth = new int[] { 30, 107, 137, 110, 134, 30 };
            tableIntestazione.SetWidths(tableIntestazioneWidth);
            //0=Left, 1=Centre, 2=Right
            tableIntestazione.AddCell(WriteCellsClass.WriteCell("", 0, 3, 0, myFontCorpo, 0));
            tableIntestazione.AddCell(WriteCellsClass.WriteCell("RISERVATA-PERSONALE", 0, 2, 2, myFontCorpoSottolineato, 0));//right
            tableIntestazione.AddCell(WriteCellsClass.WriteCell("", 0, 1, 0, myFontCorpo, 0));

            tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 6, 0, myFontCorpo, 0));

            tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 3, 0, myFontCorpo, 0));
            tableIntestazione.AddCell(WriteCellsClass.WriteCell(titolo + " " + nominativo, 0, 2, 2, myFontCorpo, 0));//right
            tableIntestazione.AddCell(WriteCellsClass.WriteCell("", 0, 1, 0, myFontCorpo, 0));

            tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 3, 0, myFontCorpo, 0));
            tableIntestazione.AddCell(WriteCellsClass.WriteCell("RAI - Radiotelevisione italiana", 0, 2, 2, myFontCorpo, 0));//right
            tableIntestazione.AddCell(WriteCellsClass.WriteCell("", 0, 1, 0, myFontCorpo, 0));

            tableIntestazione.AddCell(WriteCellsClass.WriteCell(" ", 0, 6, 0, myFontCorpo, 0));

            tableIntestazione.AddCell(WriteCellsClass.WriteCell("", 0, 1, 0, myFontCorpo, 0));
            tableIntestazione.AddCell(WriteCellsClass.WriteCell(
                String.Format("{0}, {1}", sede, DateTime.Today.ToString("dd/MM/yyyy")), 0, 2, 0, myFontCorpo, 0));
            tableIntestazione.AddCell(WriteCellsClass.WriteCell(
                sede.ToUpper(), 0, 2, 2, myFontCorpoSottolineato, 0));//right
            tableIntestazione.AddCell(WriteCellsClass.WriteCell("", 0, 1, 0, myFontCorpo, 0));

            document.Add(tableIntestazione);
            tableIntestazione.FlushContent();
            tableIntestazione.DeleteBodyRows();

            phrase = new Phrase();
            phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
            p = new Paragraph(phrase);
            ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
            document.Add(p);

            phrase = new Phrase();
            phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
            p = new Paragraph(phrase);
            ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
            document.Add(p);

            phrase = new Phrase();
            phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
            p = new Paragraph(phrase);
            ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
            document.Add(p);
        }

        private static void StampaFirma(int spaces, ref Document document, ref XR_DEM_DOCUMENTI doc)
        {
            Paragraph p = null;
            Phrase phrase = new Phrase();

            for (int idx = 0; idx <= spaces; idx++)
            {
                phrase = new Phrase();
                phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                p = new Paragraph(phrase);
                ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                document.Add(p);
            }

            PdfPTable tableFirma = new PdfPTable(6);
            tableFirma.DefaultCell.BorderWidth = 0;
            tableFirma.TotalWidth = 550;
            tableFirma.LockedWidth = true;
            var tableFirmaWidth = new int[] { 30, 107, 137, 110, 134, 30 };
            tableFirma.SetWidths(tableFirmaWidth);
            tableFirma.AddCell(WriteCellsClass.WriteCell(" ", 0, 3, 1, myFontCorpo, 0));
            tableFirma.AddCell(WriteCellsClass.WriteCell("FIRMA", 0, 2, 1, myFontCorpo, 0));
            tableFirma.AddCell(WriteCellsClass.WriteCell(" ", 0, 1, 1, myFontCorpo, 0));

            document.Add(tableFirma);
            tableFirma.FlushContent();
            tableFirma.DeleteBodyRows();
        }

        private static byte[] CreaLettera_AssegnazioneTemporanea(XR_DEM_DOCUMENTI doc, DateTime dataInizio, DateTime dataFine)
        {
            byte[] bytes = null;
            try
            {
                string codSede = "";
                string descSede = "";
                string codServizio = "";
                string descServizio = "";
                string codSezione = "";
                string descSezione = "";
                string sesso = "";
                string titolo = "Sig.";
                string nominativo = "";
                string sede = "Roma";
                string descSocieta = "";
                string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                Image png = iTextSharp.text.Image.GetInstance(_imgPath);

                bool result = SetImpostazioniStampa(doc.MatricolaDestinatario,
                                                ref nominativo,
                                                ref sesso,
                                                ref titolo,
                                                ref sede,
                                                ref descSocieta,
                                                ref png);

                result = DecodificaDatiJson(doc.MatricolaDestinatario, doc.CustomDataJSON,
                    ref codSede,
                    ref descSede,
                    ref codServizio,
                    ref descServizio,
                    ref codSezione,
                    ref descSezione);

                using (MemoryStream ms = new MemoryStream())
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new DematerializzazioneManagerPdfPageEventHelper(png);
                    document.Open();
                    PdfContentByte cb = writer.DirectContent;
                    Paragraph p = null;
                    Phrase phrase = new Phrase();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaIntestazione(ref document, ref doc);

                    string par1 = String.Format("Le comunichiamo che, a decorrere dal {0} e fino al {1}, Ella è " +
                        "assegnata temporaneamente presso la Direzione {2}.",
                        dataInizio.ToString("dd/MM/yyyy"),
                        dataFine.ToString("dd/MM/yyyy"),
                        descServizio);
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par1, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par2 = String.Format("Con i migliori saluti.");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par2, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaFirma(4, ref document, ref doc);

                    document.Close();
                    writer.Close();
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return bytes;
        }

        private static byte[] CreaLettera_FineAnticipataAssegnazioneTemporanea(XR_DEM_DOCUMENTI doc, DateTime dataInizio, DateTime dataFine)
        {
            byte[] bytes = null;
            try
            {
                string codSede = "";
                string descSede = "";
                string codServizio = "";
                string descServizio = "";
                string codSezione = "";
                string descSezione = "";
                string sesso = "";
                string titolo = "Sig.";
                string nominativo = "";
                string sede = "Roma";
                string descSocieta = "";
                string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                Image png = iTextSharp.text.Image.GetInstance(_imgPath);

                bool result = SetImpostazioniStampa(doc.MatricolaDestinatario,
                    ref nominativo,
                    ref sesso,
                    ref titolo,
                    ref sede,
                    ref descSocieta,
                    ref png);

                result = DecodificaDatiJson(doc.MatricolaDestinatario,
                    doc.CustomDataJSON,
                    ref codSede,
                    ref descSede,
                    ref codServizio,
                    ref descServizio,
                    ref codSezione,
                    ref descSezione);

                using (MemoryStream ms = new MemoryStream())
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new DematerializzazioneManagerPdfPageEventHelper(png);
                    document.Open();
                    PdfContentByte cb = writer.DirectContent;
                    Paragraph p = null;
                    Phrase phrase = new Phrase();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaIntestazione(ref document, ref doc);

                    string par1 = String.Format("Facciamo seguito alla lettera {0} del {1}, per comunicarLe " +
                        "che il termine della sua assegnazione temporanea presso la Direzione {2}., " +
                        "inizialmente previsto al {3}, viene anticipato al {4}",
                        "",
                        "",
                        descServizio,
                        "",
                        dataFine.ToString("dd/MM/yyyy"));
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par1, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par2 = String.Format("Con i migliori saluti.");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par2, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaFirma(4, ref document, ref doc);

                    document.Close();
                    writer.Close();
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return bytes;
        }

        private static byte[] CreaLettera_TrasferimentoTemporanea(XR_DEM_DOCUMENTI doc, DateTime dataInizio, DateTime dataFine)
        {
            byte[] bytes = null;
            try
            {
                string codSede = "";
                string descSede = "";
                string codServizio = "";
                string descServizio = "";
                string codSezione = "";
                string descSezione = "";
                string sesso = "";
                string titolo = "Sig.";
                string nominativo = "";
                string sede = "Roma";
                string descSocieta = "";
                string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                Image png = iTextSharp.text.Image.GetInstance(_imgPath);

                bool result = SetImpostazioniStampa(doc.MatricolaDestinatario,
                    ref nominativo,
                    ref sesso,
                    ref titolo,
                    ref sede,
                    ref descSocieta,
                    ref png);

                result = DecodificaDatiJson(doc.MatricolaDestinatario,
                    doc.CustomDataJSON,
                    ref codSede,
                    ref descSede,
                    ref codServizio,
                    ref descServizio,
                    ref codSezione,
                    ref descSezione);

                using (MemoryStream ms = new MemoryStream())
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new DematerializzazioneManagerPdfPageEventHelper(png);
                    document.Open();
                    PdfContentByte cb = writer.DirectContent;
                    Paragraph p = null;
                    Phrase phrase = new Phrase();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaIntestazione(ref document, ref doc);

                    string par1 = String.Format("Le comunichiamo che, dal {0} e fino al {1}, " +
                        "Ella viene trasferita temporaneamente presso la Direzione {2} - sede di {3}.",
                        dataInizio.ToString("dd/MM/yyyy"),
                        dataFine.ToString("dd/MM/yyyy"),
                        descServizio,
                        descSede);
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par1, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par2 = String.Format("La preghiamo di restituirci copia della presente sottoscritta, " +
                        "in segno di integrale accettazione.");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par2, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par3 = String.Format("Con i migliori saluti.");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par3, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaFirma(4, ref document, ref doc);

                    document.Close();
                    writer.Close();
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return bytes;
        }

        private static byte[] CreaLettera_TrasferimentoADomanda(XR_DEM_DOCUMENTI doc, DateTime dataInizio, DateTime dataFine)
        {
            byte[] bytes = null;
            try
            {
                string codSede = "";
                string descSede = "";
                string codServizio = "";
                string descServizio = "";
                string codSezione = "";
                string descSezione = "";
                string sesso = "";
                string titolo = "Sig.";
                string nominativo = "";
                string sede = "Roma";
                string descSocieta = "";
                string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                Image png = iTextSharp.text.Image.GetInstance(_imgPath);

                bool result = SetImpostazioniStampa(doc.MatricolaDestinatario,
                    ref nominativo,
                    ref sesso,
                    ref titolo,
                    ref sede,
                    ref descSocieta,
                    ref png);

                result = DecodificaDatiJson(doc.MatricolaDestinatario,
                    doc.CustomDataJSON,
                    ref codSede,
                    ref descSede,
                    ref codServizio,
                    ref descServizio,
                    ref codSezione,
                    ref descSezione);

                using (MemoryStream ms = new MemoryStream())
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new DematerializzazioneManagerPdfPageEventHelper(png);
                    document.Open();
                    PdfContentByte cb = writer.DirectContent;
                    Paragraph p = null;
                    Phrase phrase = new Phrase();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaIntestazione(ref document, ref doc);

                    string par1 = String.Format("Le comunichiamo che, essendosi verificate le esigenze " +
                        "produttive che rendono possibile il Suo trasferimento, Ella viene trasferita, " +
                        "a Sua richiesta, da {0} a {1} con inquadramento presso la Direzione {2}, a " +
                        "decorrere dal {3}.",
                        descSede,
                        descSede,
                        descServizio,
                        dataInizio.ToString("dd/MM/yyyy"));
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par1, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par2 = String.Format("In tale occasione verranno applicate nei Suoi " +
                        "confronti le disposizioni contenute nell’articolo 24 del Contratto " +
                        "Collettivo di Lavoro vigente, limitatamente a quanto previsto al comma " +
                        "5 per i trasferimenti che avvengono a domanda.");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par2, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par4 = String.Format("La preghiamo di restituirci copia della presente da " +
                        "Lei firmata in segno di ricevuta e di gradire i nostri migliori saluti.");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par4, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaFirma(4, ref document, ref doc);

                    document.Close();
                    writer.Close();
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return bytes;
        }

        private static byte[] CreaLettera_TrasferimentoTemporaneo(XR_DEM_DOCUMENTI doc, DateTime dataInizio, DateTime dataFine)
        {
            byte[] bytes = null;
            try
            {
                string codSede = "";
                string descSede = "";
                string codServizio = "";
                string descServizio = "";
                string codSezione = "";
                string descSezione = "";
                string sesso = "";
                string titolo = "Sig.";
                string nominativo = "";
                string sede = "Roma";
                string descSocieta = "";
                string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                Image png = iTextSharp.text.Image.GetInstance(_imgPath);

                bool result = SetImpostazioniStampa(doc.MatricolaDestinatario,
                    ref nominativo,
                    ref sesso,
                    ref titolo,
                    ref sede,
                    ref descSocieta,
                    ref png);

                result = DecodificaDatiJson(doc.MatricolaDestinatario,
                    doc.CustomDataJSON,
                    ref codSede,
                    ref descSede,
                    ref codServizio,
                    ref descServizio,
                    ref codSezione,
                    ref descSezione);

                using (MemoryStream ms = new MemoryStream())
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new DematerializzazioneManagerPdfPageEventHelper(png);
                    document.Open();
                    PdfContentByte cb = writer.DirectContent;
                    Paragraph p = null;
                    Phrase phrase = new Phrase();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaIntestazione(ref document, ref doc);

                    string par1 = String.Format("Le comunichiamo che, dal {0} e fino al {1}, " +
                        "Ella viene trasferita temporaneamente presso la Direzione {2} - sede di {3}.",
                        dataInizio.ToString("dd/MM/yyyy"),
                        dataFine.ToString("dd/MM/yyyy"),
                        descServizio,
                        descSede);
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par1, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par2 = String.Format("La preghiamo di restituirci copia della presente sottoscritta, " +
                        "in segno di integrale accettazione.");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par2, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par3 = String.Format("Con i migliori saluti.");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par3, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaFirma(4, ref document, ref doc);

                    document.Close();
                    writer.Close();
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return bytes;
        }

        private static byte[] CreaLettera_ProrogaAssegnazioneTemporanea(XR_DEM_DOCUMENTI doc, DateTime dataInizio, DateTime dataFine)
        {
            byte[] bytes = null;
            try
            {
                string codSede = "";
                string descSede = "";
                string codServizio = "";
                string descServizio = "";
                string codSezione = "";
                string descSezione = "";
                string sesso = "";
                string titolo = "Sig.";
                string nominativo = "";
                string sede = "Roma";
                string descSocieta = "";
                string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                Image png = iTextSharp.text.Image.GetInstance(_imgPath);

                bool result = SetImpostazioniStampa(doc.MatricolaDestinatario,
                    ref nominativo,
                    ref sesso,
                    ref titolo,
                    ref sede,
                    ref descSocieta,
                    ref png);

                result = DecodificaDatiJson(doc.MatricolaDestinatario,
                    doc.CustomDataJSON,
                    ref codSede,
                    ref descSede,
                    ref codServizio,
                    ref descServizio,
                    ref codSezione,
                    ref descSezione);

                using (MemoryStream ms = new MemoryStream())
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new DematerializzazioneManagerPdfPageEventHelper(png);
                    document.Open();
                    PdfContentByte cb = writer.DirectContent;
                    Paragraph p = null;
                    Phrase phrase = new Phrase();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaIntestazione(ref document, ref doc);

                    string par1 = String.Format("Le comunichiamo che la Sua assegnazione temporanea " +
                        "nell’ambito della Direzione {0}, viene prorogata fino al {1}.",
                        descServizio,
                        dataFine.ToString("dd/MM/yyyy"));
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par1, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par2 = String.Format("Con i migliori saluti.");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par2, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaFirma(4, ref document, ref doc);

                    document.Close();
                    writer.Close();
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return bytes;
        }

        private static byte[] CreaLettera_Distacco(XR_DEM_DOCUMENTI doc, DateTime dataInizio, DateTime dataFine)
        {
            byte[] bytes = null;
            try
            {
                string codSede = "";
                string descSede = "";
                string codServizio = "";
                string descServizio = "";
                string codSezione = "";
                string descSezione = "";
                string sesso = "";
                string titolo = "Sig.";
                string nominativo = "";
                string sede = "Roma";
                string descSocieta = "";
                string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                Image png = iTextSharp.text.Image.GetInstance(_imgPath);

                bool result = SetImpostazioniStampa(doc.MatricolaDestinatario,
                    ref nominativo,
                    ref sesso,
                    ref titolo,
                    ref sede,
                    ref descSocieta,
                    ref png);

                result = DecodificaDatiJson(doc.MatricolaDestinatario,
                    doc.CustomDataJSON,
                    ref codSede,
                    ref descSede,
                    ref codServizio,
                    ref descServizio,
                    ref codSezione,
                    ref descSezione);

                using (MemoryStream ms = new MemoryStream())
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new DematerializzazioneManagerPdfPageEventHelper(png);
                    document.Open();
                    PdfContentByte cb = writer.DirectContent;
                    Paragraph p = null;
                    Phrase phrase = new Phrase();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaIntestazione(ref document, ref doc);

                    string par1 = String.Format("Le comunichiamo che, dal {0} al {1}, " +
                        "Ella e’ distaccata presso la Societa’ {2}",
                        dataInizio.ToString("dd/MM/yyyy"),
                        dataFine.ToString("dd/MM/yyyy"),
                        descSocieta
                        );
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par1, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par2 = String.Format("Nel pregarLa di restituirci copia della presente " +
                        "da Lei sottoscritta per ricevuta, Le inviamo i migliori saluti.");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par2, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaFirma(4, ref document, ref doc);

                    document.Close();
                    writer.Close();
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return bytes;
        }

        private static byte[] CreaLettera_FineAnticipataAssegnazioneTemporaneaRiassegnazione(XR_DEM_DOCUMENTI doc, DateTime dataInizio, DateTime dataFine)
        {
            byte[] bytes = null;
            try
            {
                string codSede = "";
                string descSede = "";
                string codServizio = "";
                string descServizio = "";
                string codSezione = "";
                string descSezione = "";
                string sesso = "";
                string titolo = "Sig.";
                string nominativo = "";
                string sede = "Roma";
                string descSocieta = "";
                string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                Image png = iTextSharp.text.Image.GetInstance(_imgPath);

                bool result = SetImpostazioniStampa(doc.MatricolaDestinatario,
                    ref nominativo,
                    ref sesso,
                    ref titolo,
                    ref sede,
                    ref descSocieta,
                    ref png);

                result = DecodificaDatiJson(doc.MatricolaDestinatario,
                    doc.CustomDataJSON,
                    ref codSede,
                    ref descSede,
                    ref codServizio,
                    ref descServizio,
                    ref codSezione,
                    ref descSezione);

                using (MemoryStream ms = new MemoryStream())
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new DematerializzazioneManagerPdfPageEventHelper(png);
                    document.Open();
                    PdfContentByte cb = writer.DirectContent;
                    Paragraph p = null;
                    Phrase phrase = new Phrase();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaIntestazione(ref document, ref doc);

                    string par1 = String.Format("Le comunichiamo che il termine della Sua assegnazione " +
                        "temporanea presso la Direzione {0}, come da nostra lettera {1} del {2}, " +
                        "inizialmente previsto al {3}, è stato anticipato al {4}",
                        "",
                        "",
                        "",
                        "",
                        "");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par1, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par2 = String.Format("Pertanto, a decorrere dal {0}, Le confermiamo che Lei " +
                        "viene assegnata nell’ambito della Direzione {1}.",
                        dataInizio.ToString("dd/MM/yyyy"),
                        descServizio);
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par2, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    string par3 = String.Format("Con i migliori saluti.");
                    phrase = new Phrase();
                    phrase.Add(new Chunk(par3, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                    document.Add(p);

                    StampaFirma(4, ref document, ref doc);

                    document.Close();
                    writer.Close();
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return bytes;
        }

        #endregion

        public static string GetDescrizioneAbilitazione2(XR_DEM_DOCUMENTI doc)
        {
            string result = "";
            IncentiviEntities db = new IncentiviEntities();
            try
            {
                var current_workflow_position = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(doc.Id_WKF_Tipologia) && w.ID_STATO.Equals(doc.Id_Stato)).FirstOrDefault();
                if (current_workflow_position != null)
                {
                    string dest = current_workflow_position.DESTINATARIO;
                    if (!String.IsNullOrEmpty(dest))
                    {
                        myRaiData.Incentivi.IncentiviEntities dbTal = new myRaiData.Incentivi.IncentiviEntities();

                        myRaiData.Incentivi.XR_HRIS_ABIL_FUNZIONE funzione = dbTal.XR_HRIS_ABIL_FUNZIONE.Where(w => w.COD_FUNZIONE.Equals("DEMA")).FirstOrDefault();

                        if (funzione != null)
                        {
                            int id_funzione = funzione.ID_FUNZIONE;

                            var itemDesc = dbTal.XR_HRIS_ABIL_SUBFUNZIONE.Where(w => w.ID_FUNZIONE.Equals(id_funzione) && w.COD_SUBFUNZIONE.Equals(dest)).FirstOrDefault();
                            if (itemDesc != null)
                            {
                                result = itemDesc.NOT_UFFICIO;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = "";
            }

            return result;
        }

        public class DematerializzazioneManagerPdfPageEventHelper : PdfPageEventHelper
        {
            public const int fontCorpo = 12;
            public const string FONTNAME = "Calibri"; //"Times-Roman"
            private string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
            private int pagina = 1;
            public Image _png = null;

            public DematerializzazioneManagerPdfPageEventHelper(Image png)
            {
                _png = png;
                pagina = 1;
            }

            public override void OnOpenDocument(PdfWriter writer, Document document)
            {
                try
                {
                    base.OnOpenDocument(writer, document);
                }
                catch (DocumentException de)
                {
                }
                catch (System.IO.IOException ioe)
                {
                }
            }

            public override void OnStartPage(PdfWriter writer, Document document)
            {
                base.OnStartPage(writer, document);
                pagina++;

                if (pagina > 2)
                {
                    _png.SetAbsolutePosition(25, 800);

                }
                else
                {
                    _png.SetAbsolutePosition(25, 750);
                }
                PdfContentByte cb = writer.DirectContent;
                cb.AddImage(_png);
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                const int fontCorpoFirma = 8;
                base.OnEndPage(writer, document);
                BaseColor colorFirma = new BaseColor(System.Drawing.Color.Blue);
                Font myFontFirma = FontFactory.GetFont("Times-Roman", fontCorpoFirma, Font.NORMAL, colorFirma);
                Font myFontFirmaBold = FontFactory.GetFont("Times-Roman", fontCorpoFirma, Font.BOLD, colorFirma);

                PdfPTable tableFooter = new PdfPTable(4);
                tableFooter.DefaultCell.BorderWidth = 0;
                tableFooter.TotalWidth = 550;
                tableFooter.LockedWidth = true;
                var tableFooterWidth = new int[] { 137, 137, 137, 137 };
                tableFooter.SetWidths(tableFooterWidth);

                tableFooter.AddCell(WriteCellsClass.WriteCell("Rai - RadioTelevisione Italiana Spa", 0, 4, 0, myFontFirmaBold, 0));
                tableFooter.AddCell(WriteCellsClass.WriteCell("Sede legale Viale Mazzini, 14 - 00195 Roma", 0, 4, 0, myFontFirma, 0));
                //tableFooter.AddCell(WriteCellsClass.WriteCell("www.rai.it", 0, 4, 0, myFontFirmaBold, 0));
                tableFooter.AddCell(WriteCellsClass.WriteCell("Cap. Soc. Euro 242.518.100,00 Interamente versato", 0, 4, 0, myFontFirma, 0));
                tableFooter.AddCell(WriteCellsClass.WriteCell("Ufficio del Registro delle Imprese di Roma", 0, 4, 0, myFontFirma, 0));
                tableFooter.AddCell(WriteCellsClass.WriteCell("© RAI 2015 - tutti i diritti riservati. P.Iva 06382641006", 0, 4, 0, myFontFirma, 0));
                tableFooter.WriteSelectedRows(0, -1, 60, document.Bottom, writer.DirectContent);
            }

            public override void OnCloseDocument(PdfWriter writer, Document document)
            {
                base.OnCloseDocument(writer, document);
            }
        }

        /// <summary>
        /// Prende tutti i documenti che l'utente corrente deve visionare
        /// </summary>
        /// <param name="nominativo"></param>
        /// <param name="matricola"></param>
        /// <param name="id_Tipo_Doc"></param>
        /// <param name="statoRichiesta"></param>
        /// <returns></returns>
        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiDaVisionare2(string nominativo = null, string matricola = null, string oggetto = null, string id_Tipo_Doc = null, DateTime? datadal = null)
        {
            List<XR_DEM_DOCUMENTI> _tempResult = new List<XR_DEM_DOCUMENTI>();
            List<XR_DEM_DOCUMENTI_EXT> result = new List<XR_DEM_DOCUMENTI_EXT>();
            IQueryable<XR_DEM_DOCUMENTI> _tempItems = null;
            DateTime defaultDate = new DateTime(1900, 1, 1);

            try
            {
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                IncentiviEntities db = new IncentiviEntities();

                // se sono autorizzato
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");

                // se ho la funzione 01VIST sono Vistatore
                if (!subFunc.Contains("01VIST"))
                {
                    throw new Exception("Utente non autorizzato");
                }

                // Get Aree abilitate
                //var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                //.Where(w => w.MATRICOLA == matricolaCorrente && w.IND_ATTIVO)
                //.Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                //.Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "02GEST")
                //.Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                //.FirstOrDefault();
                //var listArea = _myAbil.GR_AREA.Split(',').Select(int.Parse).ToList();

                // prende tutti i documenti il cui stato è superiore a bozza con pratica attiva
                // il cui stato non è concluso, cancellato, rifiutato o azione automatica
                _tempItems = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva
                                                        //&& listArea.Contains((int)w.IdArea)
                                                        );
                _tempItems = _tempItems.Where(w => w.Id_Stato > (int)StatiDematerializzazioneDocumenti.Bozza);
                _tempItems = _tempItems.Where(w => w.MatricolaVisualizzatore.Contains(matricolaCorrente));

                if (!String.IsNullOrEmpty(nominativo) || !String.IsNullOrEmpty(matricola) || !String.IsNullOrEmpty(id_Tipo_Doc) || (datadal.HasValue && datadal != defaultDate))
                {
                    // se c'è almeno un filtro
                    if (!String.IsNullOrEmpty(id_Tipo_Doc))
                    {
                        int _myFiltro_id_Tipo_Doc = 0;
                        bool converti = int.TryParse(id_Tipo_Doc, out _myFiltro_id_Tipo_Doc);

                        if (converti)
                        {
                            _tempItems = _tempItems.Where(w => w.Id_Tipo_Doc == _myFiltro_id_Tipo_Doc);
                        }
                        else
                        {
                            throw new Exception("Errore, tipologia documentale non riconosciuta");
                        }
                    }

                    if (!String.IsNullOrEmpty(matricola))
                    {
                        _tempItems = _tempItems.Where(w => w.MatricolaDestinatario.Equals(matricola));
                    }

                    if (!String.IsNullOrEmpty(oggetto))
                    {
                        _tempItems = _tempItems.Where(w => w.OggettoProtocollo.Contains(oggetto));
                    }
                    if (datadal.HasValue && datadal != defaultDate)
                    {
                        _tempItems = _tempItems.Where(w => w.DataCreazione > datadal.Value);
                    }

                    //if (!String.IsNullOrEmpty(statoRichiesta))
                    //{
                    //    int id_stato_filtro = 0;
                    //    bool converti = int.TryParse(statoRichiesta, out id_stato_filtro);

                    //    if (converti)
                    //    {
                    //        _tempItems = _tempItems.Where(w => w.Id_Stato.Equals(id_stato_filtro));

                    //        if (id_stato_filtro == (int)StatiDematerializzazioneDocumenti.PraticaCancellata)
                    //        {
                    //            _tempItems = _tempItems.Where(w => !w.PraticaAttiva);
                    //        }
                    //        else if (id_stato_filtro != (int)StatiDematerializzazioneDocumenti.Bozza)
                    //        {
                    //            _tempItems = _tempItems.Where(w => w.PraticaAttiva);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        throw new Exception("Errore, stato pratica non riconosciuto");
                    //    }
                    //}
                    //else
                    //{
                    //    _tempItems = _tempItems.Where(w => w.PraticaAttiva);
                    //}

                    if (!String.IsNullOrEmpty(nominativo))
                    {
                        List<string> matricoleDestinatario = new List<string>();
                        matricoleDestinatario.AddRange(_tempItems.Select(w => w.MatricolaDestinatario).ToList());
                        if (matricoleDestinatario != null && matricoleDestinatario.Any())
                        {
                            string abilKey = "DEMA";
                            string currentMatr = CommonHelper.GetCurrentUserMatricola();
                            DateTime oggi = DateTime.Now;
                            var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.Where(x => x.COD_MATLIBROMAT != null && x.DTA_FINE_CR != null && x.DTA_FINE_CR.Value >= DateTime.Today), currentMatr, null, abilKey);
                            tmpSint = tmpSint.OrderBy(x => x.COD_MATLIBROMAT);

                            var tmp = tmpSint.Select(s => new CercaDipendentiItem()
                            {
                                MATRICOLA = s.COD_MATLIBROMAT,
                                NOME = s.DES_NOMEPERS,
                                COGNOME = s.DES_COGNOMEPERS,
                                SECONDO_COGNOME = s.DES_SECCOGNOME,
                                DATA_ASSUNZIONE = s.DTA_INIZIO_CR,
                                CONTRATTO = s.DES_TPCNTR,
                                ID_PERSONA = s.ID_PERSONA,
                                COD_SEDE = s.COD_SEDE,
                                DES_SEDE = s.DES_SEDE,
                                COD_SERVIZIO = s.COD_SERVIZIO,
                                DES_SERVIZIO = s.DES_SERVIZIO,
                                SmartWorker = false
                            });

                            if (tmp != null && tmp.Any())
                            {
                                foreach (var t in tmp)
                                {
                                    t.NOME = CommonHelper.ToTitleCase(t.NOME);
                                    t.COGNOME = CommonHelper.ToTitleCase(t.COGNOME);
                                    t.SECONDO_COGNOME = CommonHelper.ToTitleCase(t.SECONDO_COGNOME);
                                }
                            }

                            var _filteredList = tmp.Where(w => (w.NOME + " " + w.COGNOME).ToUpper().Contains(nominativo.ToUpper())
                                                    || (w.COGNOME + " " + w.NOME).ToUpper().Contains(nominativo.ToUpper())).ToList();

                            List<string> _matricole = new List<string>();

                            if (_filteredList != null && _filteredList.Any())
                            {
                                _matricole.AddRange(_filteredList.Select(w => w.MATRICOLA).ToList());
                            }

                            _tempItems = _tempItems.Where(w => _matricole.Contains(w.MatricolaDestinatario));
                        }
                    }
                }
                else
                {
                    _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.RifiutoApprovatore));
                    _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.RifiutatoFirma));
                    _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente));
                    _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomatica));
                    _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaAssunzionePreApprovazione));
                    _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaAssunzionePreFirma));
                    _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaContabile));
                    _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInCreazione));
                    _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInvioDocumentoAlDipendente));
                    _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.PraticaConclusa));
                    _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.PraticaCancellata));

                }

                // una volta presi tutti i documenti dove la matricola corrente risulta essere un vistatore
                // bisogna verificare se lo stato successivo del documento è 30, vistato
                List<XR_DEM_DOCUMENTI> _tempDoc = new List<XR_DEM_DOCUMENTI>();
                _tempDoc = _tempItems.Where(x=>x.Id_Stato!=200).ToList();

                if (_tempDoc != null && _tempDoc.Any())
                {
                    foreach (var d in _tempDoc)
                    {
                        try
                        {
                            int prossimoStato = GetNextIdStato(d.Id_Stato, d.Id_WKF_Tipologia, true);

                            // prima di tutto prende l'id del workflow corrente, in base allo stato del documento
                            var wkf = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA == d.Id_WKF_Tipologia && w.ID_STATO == d.Id_Stato).FirstOrDefault();
                            if (wkf == null)
                            {
                                throw new Exception("Impossibile reperire il workflow del documento");
                            }

                            var _wkfDin = db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Where(w => w.ID_DOCUMENTO == d.Id && w.ATTIVO && w.ID_WORKFLOW == wkf.ID_WORKFLOW).ToList();

                            if (prossimoStato == (int)StatiDematerializzazioneDocumenti.Visionato)
                            {
                                // se il prossimo stato è visionato e l'utente corrente ha visibilità sul
                                // creatore

                                var r = AuthHelper.EnableToMatr(matricolaCorrente, d.MatricolaCreatore, "DEMA", "01VIST");
                                // se la matricola corrente non ha diritti come vistatore della matricola creatrice del
                                // documento, allora verrà scartato
                                if (!r.Enabled)
                                {
                                    continue;
                                }

                                // se ne ha diritto allora verifica che la matricola sia presente nell'elenco dei vistatori sulla
                                // tabella XR_WKF_WORKFLOW_DYNAMIC_STEPS
                                if (_wkfDin != null && _wkfDin.Any())
                                {
                                    foreach (var w in _wkfDin)
                                    {
                                        // per ogni elemento prende il json e verifica se tra le matricole 
                                        // vistatori c'è la matricola corrente
                                        var _stringToJson = w.JSON_INPUT;
                                        if (!String.IsNullOrEmpty(_stringToJson))
                                        {
                                            XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item _json = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item();
                                            _json = JsonConvert.DeserializeObject<XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item>(_stringToJson);

                                            if (_json != null && !String.IsNullOrEmpty(_json.MatricolaVistatore))
                                            {
                                                if (_json.MatricolaVistatore == matricolaCorrente)
                                                {
                                                    XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                                    newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(d);
                                                    if (_json.Data.HasValue)
                                                    {
                                                        newItem.DataVistoUtenteCorrente = _json.Data;
                                                    }
                                                    result.Add(newItem);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                /*
                                 * Se il prossimo stato non è visionato, cerca nella tabella XR_WKF_WORKFLOW_DYNAMIC_STEPS
                                 * se per l'idworkflow corrente ci sono degli step che contengono l'azione di un vistatore
                                 * 
                                 */
                                if (_wkfDin != null && _wkfDin.Any())
                                {
                                    foreach (var w in _wkfDin)
                                    {
                                        // per ogni elemento prende il json e verifica se tra le matricole 
                                        // vistatori ci sia la matricola corrente
                                        var _stringToJson = w.JSON_INPUT;
                                        if (!String.IsNullOrEmpty(_stringToJson))
                                        {
                                            XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item _json = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item();
                                            _json = JsonConvert.DeserializeObject<XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item>(_stringToJson);

                                            if (_json != null && !String.IsNullOrEmpty(_json.MatricolaVistatore))
                                            {
                                                if (_json.MatricolaVistatore == matricolaCorrente)
                                                {
                                                    XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                                    newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(d);
                                                    if (_json.Data.HasValue)
                                                    {
                                                        newItem.DataVistoUtenteCorrente = _json.Data;
                                                    }
                                                    result.Add(newItem);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            IncentiviEntities db2 = new IncentiviEntities();
                            XR_HRIS_LOG log = new XR_HRIS_LOG();

                            log.COD_MATRICOLA = matricolaCorrente;
                            log.ID_PERSONA = CommonHelper.GetCurrentIdPersona();
                            log.DES_OPERAZIONE = "Exception";
                            log.NOT_PARAMETRI = ex.GetBaseException().ToString();
                            log.IND_ESITO = false;

                            db2.XR_HRIS_LOG.Add(log);
                        }
                    }
                }

                if (result != null && result.Any())
                {
                    result = result.Where(w => w.Avanzamento != 100).ToList();
                }

                result = result.OrderBy(w => w.Avanzamento)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoFirma)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente)
                        .ThenByDescending(w => w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza)
                        .ThenByDescending(w => w.MatricolaDestinatario).ToList();
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiDaApprovare2(string nominativo = null, string oggetto = null, string matricola = null, string id_Tipo_Doc = null, string statoRichiesta = null)
        {
            List<XR_DEM_DOCUMENTI> _tempDoc = new List<XR_DEM_DOCUMENTI>();
            List<XR_DEM_DOCUMENTI> _tempResult = new List<XR_DEM_DOCUMENTI>();
            List<XR_DEM_DOCUMENTI_EXT> result = new List<XR_DEM_DOCUMENTI_EXT>();
            try
            {
                DateTime oggi = DateTime.Now;
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                IncentiviEntities db = new IncentiviEntities();
                string abilKey = "DEMA";
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");
                // se ho la funzione di approvatore
                if (!subFunc.Contains("01APPR") && !subFunc.Contains("01ADM"))
                {
                    throw new Exception("Utente non autorizzato");
                }

                var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), matricolaCorrente, null, abilKey);
                tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT));

                _tempDoc = GetDocumentiDaApprovareFase1(nominativo, oggetto, matricola, id_Tipo_Doc, statoRichiesta);
                if (_tempDoc != null && _tempDoc.Any())
                {
                    foreach (var d in _tempDoc)
                    {
                        /*
                         * Verifica se la matricola corrente ha diritto 01APPR sulla matricola
                         * creatore della pratica
                         */
                        var r = AuthHelper.EnableToMatr(matricolaCorrente, d.MatricolaCreatore, "DEMA", "01APPR");
                        if (!r.Enabled || r.Visibilita != AbilMatrLiv.VisibilitaEnum.Filtrata)
                        {
                            // se non è approvatore per la matricola che ha creato la pratica procede con
                            // l'analisi della pratica successiva
                            continue;
                        }

                        /*
                        * Se c'è visibilità sulla pratica bisogna verificare se si trova in uno stato
                        * in cui è possibile approvarla, questo perchè alcune pratiche potrebbero
                        * avere un vistatore prima dell'approvazione, vistatore che può essere
                        * definito del workflow, oppure non far parte del workflow, ma essere attivato
                        * sulla tabella XR_DEM_TIPIDOC_COMPORTAMENTO
                        */
                        if (PraticaApprovabile(d))
                        {
                            _tempResult.Add(d);
                        }
                    }
                }

                if (_tempResult != null && _tempResult.Any())
                {
                    _tempResult = _tempResult.DistinctBy(w => w.Id).ToList();

                    foreach (var x in _tempResult)
                    {
                        XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                        newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(x);
                        result.Add(newItem);
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }
        public static string MatricolaDelegante()
        {
            DelegheResult del = DelegheManager.RecuperaDelegheRicevute("DEMA");
            if (del != null && del.Obj != null)
            {
                if (del.Obj is List<XR_HRIS_DELEGHE>)
                {
                    var deleghe = (del.Obj as List<XR_HRIS_DELEGHE>);
                    if (deleghe.Any())
                    {
                        if (!String.IsNullOrWhiteSpace(deleghe.First().MATRICOLA_DELEGANTE))
                        {
                            return deleghe.First().MATRICOLA_DELEGANTE;
                        }
                    }
                }
            }
            return null;
        }
        private static List<XR_DEM_DOCUMENTI> GetDocumentiDaApprovareFase1(string nominativo = null,  string oggetto = null, string matricola = null, string id_Tipo_Doc = null, string statoRichiesta = null)
        {
            List<XR_DEM_DOCUMENTI> result = new List<XR_DEM_DOCUMENTI>();
            IQueryable<XR_DEM_DOCUMENTI> _tempItems = null;
            DateTime oggi = DateTime.Now;
            string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
            IncentiviEntities db = new IncentiviEntities();
            string delegante = MatricolaDelegante();

            // prende tutti i documenti il cui stato è superiore a bozza con pratica attiva
            // il cui stato non è concluso, cancellato, rifiutato o azione automatica
            _tempItems = db.XR_DEM_DOCUMENTI.Where(w => w.PraticaAttiva);
            _tempItems = _tempItems.Where(w => w.MatricolaApprovatore.Contains(matricolaCorrente) || (delegante != null &&
            w.MatricolaApprovatore.Contains(delegante)));
            _tempItems = _tempItems.Where(w => w.DataRifiuto == null);
            _tempItems = _tempItems.Where(w => w.DataApprovazione == null);

            if (String.IsNullOrEmpty(statoRichiesta))
            {
                _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.Bozza));
                _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.RifiutoApprovatore));
                _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.RifiutatoFirma));
                _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente));
                _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomatica));
                _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaAssunzionePreFirma));
                _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaContabile));
                _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInCreazione));
                _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInvioDocumentoAlDipendente));
                _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.PraticaConclusa));
                _tempItems = _tempItems.Where(w => !w.Id_Stato.Equals((int)StatiDematerializzazioneDocumenti.PraticaCancellata));
            }

            if (!String.IsNullOrEmpty(nominativo) ||
                !String.IsNullOrEmpty(oggetto) ||
                !String.IsNullOrEmpty(matricola) ||
                !String.IsNullOrEmpty(id_Tipo_Doc) ||
                !String.IsNullOrEmpty(statoRichiesta))
            {
                // se c'è almeno un filtro
                if (!String.IsNullOrEmpty(id_Tipo_Doc))
                {
                    int _myFiltro_id_Tipo_Doc = 0;
                    bool converti = int.TryParse(id_Tipo_Doc, out _myFiltro_id_Tipo_Doc);

                    if (converti)
                    {
                        _tempItems = _tempItems.Where(w => w.Id_Tipo_Doc == _myFiltro_id_Tipo_Doc);
                    }
                    else
                    {
                        throw new Exception("Errore, tipologia documentale non riconosciuta");
                    }
                }

                if (!String.IsNullOrEmpty(matricola))
                {
                    _tempItems = _tempItems.Where(w => w.MatricolaDestinatario.Equals(matricola));
                }
                if (!String.IsNullOrEmpty(oggetto))
                {
                    _tempItems = _tempItems.Where(w => w.OggettoProtocollo.Contains(oggetto));
                }
                if (!String.IsNullOrEmpty(statoRichiesta))
                {
                    int id_stato_filtro = 0;
                    bool converti = int.TryParse(statoRichiesta, out id_stato_filtro);

                    if (converti)
                    {
                        _tempItems = _tempItems.Where(w => w.Id_Stato.Equals(id_stato_filtro));

                        if (id_stato_filtro == (int)StatiDematerializzazioneDocumenti.PraticaCancellata)
                        {
                            _tempItems = _tempItems.Where(w => !w.PraticaAttiva);
                        }
                        else if (id_stato_filtro != (int)StatiDematerializzazioneDocumenti.Bozza)
                        {
                            _tempItems = _tempItems.Where(w => w.PraticaAttiva);
                        }
                    }
                    else
                    {
                        throw new Exception("Errore, stato pratica non riconosciuto");
                    }
                }
                else
                {
                    _tempItems = _tempItems.Where(w => w.PraticaAttiva);
                }

                if (!String.IsNullOrEmpty(nominativo))
                {
                    List<string> matricoleDestinatario = new List<string>();
                    matricoleDestinatario.AddRange(_tempItems.Select(w => w.MatricolaDestinatario).ToList());
                    if (matricoleDestinatario != null && matricoleDestinatario.Any())
                    {
                        string abilKey = "DEMA";
                        var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.Where(x => x.COD_MATLIBROMAT != null && x.DTA_FINE_CR != null && x.DTA_FINE_CR.Value >= DateTime.Today), matricolaCorrente, null, abilKey);
                        tmpSint = tmpSint.OrderBy(x => x.COD_MATLIBROMAT);

                        var tmp = tmpSint.Select(s => new CercaDipendentiItem()
                        {
                            MATRICOLA = s.COD_MATLIBROMAT,
                            NOME = s.DES_NOMEPERS,
                            COGNOME = s.DES_COGNOMEPERS,
                            SECONDO_COGNOME = s.DES_SECCOGNOME,
                            DATA_ASSUNZIONE = s.DTA_INIZIO_CR,
                            CONTRATTO = s.DES_TPCNTR,
                            ID_PERSONA = s.ID_PERSONA,
                            COD_SEDE = s.COD_SEDE,
                            DES_SEDE = s.DES_SEDE,
                            COD_SERVIZIO = s.COD_SERVIZIO,
                            DES_SERVIZIO = s.DES_SERVIZIO,
                            SmartWorker = false
                        });

                        if (tmp != null && tmp.Any())
                        {
                            foreach (var t in tmp)
                            {
                                t.NOME = CommonHelper.ToTitleCase(t.NOME);
                                t.COGNOME = CommonHelper.ToTitleCase(t.COGNOME);
                                t.SECONDO_COGNOME = CommonHelper.ToTitleCase(t.SECONDO_COGNOME);
                            }
                        }

                        var _filteredList = tmp.Where(w => (w.NOME + " " + w.COGNOME).ToUpper().Contains(nominativo.ToUpper())
                                                || (w.COGNOME + " " + w.NOME).ToUpper().Contains(nominativo.ToUpper())).ToList();

                        List<string> _matricole = new List<string>();

                        if (_filteredList != null && _filteredList.Any())
                        {
                            _matricole.AddRange(_filteredList.Select(w => w.MATRICOLA).ToList());
                        }

                        _tempItems = _tempItems.Where(w => _matricole.Contains(w.MatricolaDestinatario));
                    }
                }
            }

            result = _tempItems.ToList();
            return result;
        }

        private static bool PraticaApprovabile(XR_DEM_DOCUMENTI pratica)
        {
            bool result = false;
            IncentiviEntities db = new IncentiviEntities();
            List<XR_WKF_WORKFLOW_DYNAMIC_STEPS> _wkfSteps = new List<XR_WKF_WORKFLOW_DYNAMIC_STEPS>();
            string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
            string delegante = MatricolaDelegante();
            try
            {
                /*
                 * Prima di tutto verifica che nel workflow sia prensente
                 * il record relativo al tipo di pratica e allo stato in cui si trova
                 */
                var wkf = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA == pratica.Id_WKF_Tipologia && w.ID_STATO == pratica.Id_Stato).FirstOrDefault();

                if (wkf != null)
                {
                    /*
                     * Se lo stato in cui si trova è previsto dal workflow, allora cerca 
                     * in XR_WKF_WORKFLOW_DYNAMIC_STEPS la presenza di sotto steps da approvatore
                     * per quel particolare stato.
                     * In particolare cerca la matricola corrente nei record relativi all'approvazione
                     */
                    _wkfSteps = db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Where(w => w.ID_DOCUMENTO == pratica.Id && w.ATTIVO && w.ID_WORKFLOW == wkf.ID_WORKFLOW).ToList();

                    if (_wkfSteps != null && _wkfSteps.Any())
                    {
                        foreach (var w in _wkfSteps)
                        {
                            // per ogni elemento prende il json e verifica se tra le matricole 
                            // approvatore c'è la matricola corrente
                            var _stringToJson = w.JSON_INPUT;
                            if (!String.IsNullOrEmpty(_stringToJson))
                            {
                                XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item _json = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item();
                                _json = JsonConvert.DeserializeObject<XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item>(_stringToJson);

                                if (_json != null && !String.IsNullOrEmpty(_json.MatricolaApprovatore))
                                {
                                    if (_json.MatricolaApprovatore == matricolaCorrente || (delegante != null && _json.MatricolaApprovatore == delegante))
                                    {
                                        if (_json.Data.HasValue)
                                        {
                                            return false;
                                        }
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }

                /*
                 * A questo punto bisogna controllare altre due cose
                 * 1. se lo stato è 20, deve verificare se la tipologia documentale prevede
                 * il un visto, se si allora il documento va scartato, in alternativa è approvabile.
                 * 2. Partendo dallo stato corrente, deve verificare se il prossimo stato è 
                 * approvato.
                */
                if (InAttesaDiVisto(pratica))
                {
                    return false;
                }

                int statoSuccessivo = GetNextIdStato(pratica.Id_Stato, pratica.Id_WKF_Tipologia);

                while
                (statoSuccessivo == (int)StatiDematerializzazioneDocumenti.AzioneAutomatica ||
                statoSuccessivo == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaAssunzionePreApprovazione ||
                statoSuccessivo == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaAssunzionePreFirma ||
                statoSuccessivo == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaContabile ||
                statoSuccessivo == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInCreazione ||
                statoSuccessivo == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInvioDocumentoAlDipendente)
                {
                    statoSuccessivo = GetNextIdStato(statoSuccessivo, pratica.Id_WKF_Tipologia);
                }

                if (statoSuccessivo == (int)StatiDematerializzazioneDocumenti.Accettato)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        private static bool InAttesaDiVisto(XR_DEM_DOCUMENTI pratica)
        {
            bool result = true;

            if (pratica.DataVisto.HasValue)
            {
                return false;
            }

            IncentiviEntities db = new IncentiviEntities();
            List<NominativoMatricola> vistatori = null;
            vistatori = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

            string tipologiaDocumentale = pratica.Cod_Tipologia_Documentale;
            int id_tipo_doc = pratica.Id_Tipo_Doc;
            string tipologiaDocumento = "";
            var dem_TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(id_tipo_doc)).FirstOrDefault();

            if (dem_TIPI_DOCUMENTO != null)
            {
                tipologiaDocumento = dem_TIPI_DOCUMENTO.Codice;
            }

            var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

            if (comportamento == null)
            {
                throw new Exception("Comportamento non trovato");
            }

            if (comportamento.Visionatore)
            {
                if (vistatori != null && vistatori.Any())
                {
                    string tipologia = String.Format("{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());
                    bool almenoUnVistatoreAutorizzato = false;
                    foreach (var i in vistatori)
                    {
                        string _tempNominativo = "";
                        if (!String.IsNullOrEmpty(pratica.MatricolaDestinatario))
                        {
                            var r = AuthHelper.EnableToMatr(i.Matricola, pratica.MatricolaCreatore, "DEMA", "01VIST");
                            if (r.Enabled)
                            {
                                _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                            }
                        }
                        // se non c'è matricola, allora è una tipologia come ad esempio 
                        // APPUNTO, che non ha matricola dipendente, ma ha comunque un vistatore
                        var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                        .Where(w => w.MATRICOLA == i.Matricola && w.IND_ATTIVO)
                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01VIST")
                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                        .FirstOrDefault();

                        if (_myAbil != null)
                        {
                            #region POSSO VISTATORE 
                            //Calcolo degli elementi che posso firmare
                            List<string> possoVistare = new List<string>();

                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                            {
                                possoVistare.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                            }

                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                                && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                            {
                                possoVistare.Add(_myAbil.TIP_DOC_INCLUSI);
                            }

                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                            {
                                possoVistare.Add(tipologia);
                            }
                            #endregion

                            #region NON POSSO VISTATORE 
                            //Calcolo degli elementi che non posso vistare
                            List<string> nonPossoVistare = new List<string>();

                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                            {
                                nonPossoVistare.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                            }

                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                                && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                            {
                                nonPossoVistare.Add(_myAbil.TIP_DOC_ESCLUSI);
                            }

                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                            {
                                // se però possoVistare contiene tipologia allora non va aggiunta
                                if (!possoVistare.Contains(tipologia))
                                {
                                    nonPossoVistare.Add(tipologia);
                                }
                            }
                            #endregion

                            #region CASO LIMITE TUTTI E DUE *
                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                                !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                    && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                                    && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                            {
                                possoVistare = new List<string>();
                                nonPossoVistare = new List<string>();
                            }
                            #endregion

                            List<string> tipologieAbilitate = possoVistare.Except(nonPossoVistare).ToList();

                            if (tipologieAbilitate != null &&
                                tipologieAbilitate.Any())
                            {
                                if (tipologieAbilitate.Contains(tipologia))
                                {
                                    _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim())) + "<br/>";
                                }
                                else
                                {
                                    _tempNominativo = "";
                                }
                            }
                        }

                        if (!String.IsNullOrEmpty(_tempNominativo))
                        {
                            return true;
                        }
                    }

                    if (!almenoUnVistatoreAutorizzato)
                    {
                        // se non c'è un vistatore autorizzato allora il documento passa direttamente all'approvatore
                        return false;
                    }
                }
                else
                {
                    // se non ci sono vistatori allora il documento va in approvazione
                    return false;
                }
            }
            else
            {
                result = false;
            }

            return result;
        }












        public static List<XR_DEM_DOCUMENTI_EXT> GetDocumentiDaApprovare22(string nominativo = null, string matricola = null, string id_Tipo_Doc = null, string statoRichiesta = null)
        {
            List<XR_DEM_DOCUMENTI> _tempDoc = new List<XR_DEM_DOCUMENTI>();
            List<XR_DEM_DOCUMENTI> _tempResult = new List<XR_DEM_DOCUMENTI>();
            List<XR_DEM_DOCUMENTI_EXT> result = new List<XR_DEM_DOCUMENTI_EXT>();
            IQueryable<XR_DEM_DOCUMENTI> _tempItems = null;
            List<XR_WKF_WORKFLOW_DYNAMIC_STEPS> _wkfSteps = new List<XR_WKF_WORKFLOW_DYNAMIC_STEPS>();
            try
            {
                DateTime oggi = DateTime.Now;
                string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
                IncentiviEntities db = new IncentiviEntities();
                string abilKey = "DEMA";
                var subFunc = AuthHelper.EnabledSubFunc(matricolaCorrente, "DEMA");
                // se ho la funzione di approvatore
                if (!subFunc.Contains("01APPR") && !subFunc.Contains("01ADM"))
                {
                    throw new Exception("Utente non autorizzato");
                }

                var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), matricolaCorrente, null, abilKey);
                tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT));

                _tempDoc = GetDocumentiDaApprovareFase1(nominativo, matricola, id_Tipo_Doc, statoRichiesta);
                if (_tempDoc != null && _tempDoc.Any())
                {
                    foreach (var d in _tempDoc)
                    {
                        /*
                         * Verifica se la matricola corrente ha diritto 01APPR sulla matricola
                         * creatore della pratica
                         */
                        var r = AuthHelper.EnableToMatr(matricolaCorrente, d.MatricolaCreatore, "DEMA", "01APPR");
                        if (!r.Enabled || r.Visibilita != AbilMatrLiv.VisibilitaEnum.Filtrata)
                        {
                            // se non è approvatore per la matricola che ha creato la pratica procede con
                            // l'analisi della pratica successiva
                            continue;
                        }

                        // se ne ha diritto allora verifica che la matricola sia presente nell'elenco degli approvatori sulla
                        // tabella XR_WKF_WORKFLOW_DYNAMIC_STEPS
                        // prima di tutto prende l'id del workflow corrente, in base allo stato del documento
                        var wkf = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA == d.Id_WKF_Tipologia && w.ID_STATO == d.Id_Stato).FirstOrDefault();
                        if (wkf == null)
                        {
                            /*
                             * Se non trova nulla può essere dovuto al fatto che lo stato in cui si 
                             * trova il documento non sia dichiarato nel workflow, questo capita ad 
                             * esempio per lo stato vistato, perchè alcune tipologie non hanno nel
                             * workflow questo stato, ma viene abilitato attraverso la tabella dei
                             * comportamenti.
                             */
                            _wkfSteps = db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Where(w => w.ID_DOCUMENTO == d.Id && w.ATTIVO).ToList();
                        }
                        else
                        {
                            _wkfSteps = db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Where(w => w.ID_DOCUMENTO == d.Id && w.ATTIVO && w.ID_WORKFLOW == wkf.ID_WORKFLOW).ToList();
                        }

                        if (_wkfSteps != null && _wkfSteps.Any())
                        {
                            foreach (var w in _wkfSteps)
                            {
                                // per ogni elemento prende il json e verifica se tra le matricole 
                                // vistatori c'è la matricola corrente
                                var _stringToJson = w.JSON_INPUT;
                                if (!String.IsNullOrEmpty(_stringToJson))
                                {
                                    XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item _json = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item();
                                    _json = JsonConvert.DeserializeObject<XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item>(_stringToJson);

                                    if (_json != null && !String.IsNullOrEmpty(_json.MatricolaApprovatore))
                                    {
                                        if (_json.MatricolaApprovatore == matricolaCorrente)
                                        {
                                            XR_DEM_DOCUMENTI_EXT newItem = new XR_DEM_DOCUMENTI_EXT();
                                            newItem = ConvertiInXR_DEM_DOCUMENTI_EXT(d);
                                            if (_json.Data.HasValue)
                                            {
                                                if (_json.Approvato)
                                                {
                                                    newItem.DataApprovazione = _json.Data;
                                                }
                                                else
                                                {
                                                    newItem.DataRifiuto = _json.Data;
                                                }
                                            }
                                            result.Add(newItem);
                                        }
                                    }
                                }
                            }
                        }

                        //if (wkf == null &&
                        //    (_wkfSteps == null || !_wkfSteps.Any()))
                        //{
                        //    /*
                        //     * Se lo stato corrente non è nel workflow e non ci sono step definiti per il documento
                        //     * corrente, allora vuol dire che il documento è in uno stato previsto da 
                        //     * XR_DEM_TIPIDOC_COMPORTAMENTO, ma non dal workflow, come ad esempio il visto (01vist)
                        //     * 
                        //     */
                        //}
                    }
                }

                if (String.IsNullOrEmpty(statoRichiesta) &&
                    result != null && result.Any())
                {
                    result = result.Where(w => w.Avanzamento != 100).ToList();
                }

                #region IMPORTANTE
                /*
                 * QUESTO PEZZO DI CODICE SERVE PERCHè CI SONO ALCUNE TIPOLOGIE CHE 
                 * PREVEDONO IL VISTATORE NEL COMPORTAMENTO, MA IN REALTà LA DIREZIONE
                 * CHE CREA LA PRATICA NON HA VISTATORI CONFIGURATI
                 */
                if (result != null && result.Any())
                {
                    List<NominativoMatricola> vistatori = null;
                    vistatori = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

                    result = result.DistinctBy(w => w.Id).ToList();
                    result = result.OrderByDescending(w => w.MatricolaDestinatario).ToList();

                    List<XR_DEM_DOCUMENTI_EXT> _finalList = new List<XR_DEM_DOCUMENTI_EXT>();
                    List<XR_DEM_DOCUMENTI_EXT> _tempList = new List<XR_DEM_DOCUMENTI_EXT>();
                    _tempList.AddRange(result.ToList());

                    // di tutti i documenti, se prevedono il vistatore e il documento non è stato vistato
                    // allora non va in approvazione e quindi il documento verrà scartato dalla lista
                    foreach (var l in _tempList)
                    {
                        if (l.DataVisto.HasValue)
                        {
                            // se la pratica risulta vistata allora finirà nella lista per essere approvata
                            _finalList.Add(l);
                            continue;
                        }
                        // altrimenti deve proseguire e verificare che non ci siano vistatori attivi
                        // per la pratica, nel caso non ce ne fossero la pratica passerà direttamente 
                        // all'approvatore, altrimenti dovrà essere prima vista

                        string tipologiaDocumentale = l.Cod_Tipologia_Documentale;
                        int id_tipo_doc = l.Id_Tipo_Doc;
                        string tipologiaDocumento = "";
                        var dem_TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(id_tipo_doc)).FirstOrDefault();

                        if (dem_TIPI_DOCUMENTO != null)
                        {
                            tipologiaDocumento = dem_TIPI_DOCUMENTO.Codice;
                        }

                        var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
    w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                        if (comportamento == null)
                        {
                            throw new Exception("Comportamento non trovato");
                        }

                        if (comportamento.Visionatore)
                        {
                            // se la matricola vistatore è già valorizzata allora
                            // il documento è stato già visionato e può andare in approvazione
                            if (l.DataVisto.HasValue)
                            {
                                _finalList.Add(l);
                            }
                            else
                            {
                                if (vistatori != null && vistatori.Any())
                                {
                                    string tipologia = String.Format("{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());
                                    bool almenoUnVistatoreAutorizzato = false;
                                    foreach (var i in vistatori)
                                    {
                                        string _tempNominativo = "";
                                        if (!String.IsNullOrEmpty(l.MatricolaDestinatario))
                                        {
                                            var r = AuthHelper.EnableToMatr(i.Matricola, l.MatricolaCreatore, "DEMA", "01VIST");
                                            if (r.Enabled)
                                            {
                                                _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                                            }
                                        }
                                        // se non c'è matricola, allora è una tipologia come ad esempio 
                                        // APPUNTO, che non ha matricola dipendente, ma ha comunque un vistatore
                                        var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                                        .Where(w => w.MATRICOLA == i.Matricola && w.IND_ATTIVO)
                                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01VIST")
                                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                                        .FirstOrDefault();

                                        if (_myAbil != null)
                                        {
                                            #region POSSO VISTATORE 
                                            //Calcolo degli elementi che posso firmare
                                            List<string> possoVistare = new List<string>();

                                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                                && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                                            {
                                                possoVistare.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                                            }

                                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                                && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                                                && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                                            {
                                                possoVistare.Add(_myAbil.TIP_DOC_INCLUSI);
                                            }

                                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                                && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                                            {
                                                possoVistare.Add(tipologia);
                                            }
                                            #endregion

                                            #region NON POSSO VISTATORE 
                                            //Calcolo degli elementi che non posso vistare
                                            List<string> nonPossoVistare = new List<string>();

                                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                                && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                                            {
                                                nonPossoVistare.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                                            }

                                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                                && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                                                && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                            {
                                                nonPossoVistare.Add(_myAbil.TIP_DOC_ESCLUSI);
                                            }

                                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                                && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                            {
                                                // se però possoVistare contiene tipologia allora non va aggiunta
                                                if (!possoVistare.Contains(tipologia))
                                                {
                                                    nonPossoVistare.Add(tipologia);
                                                }
                                            }
                                            #endregion

                                            #region CASO LIMITE TUTTI E DUE *
                                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                                                !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                                    && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                                                    && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                            {
                                                possoVistare = new List<string>();
                                                nonPossoVistare = new List<string>();
                                            }
                                            #endregion

                                            List<string> tipologieAbilitate = possoVistare.Except(nonPossoVistare).ToList();

                                            if (tipologieAbilitate != null &&
                                                tipologieAbilitate.Any())
                                            {
                                                if (tipologieAbilitate.Contains(tipologia))
                                                {
                                                    _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim())) + "<br/>";
                                                }
                                                else
                                                {
                                                    _tempNominativo = "";
                                                }
                                            }
                                        }

                                        if (!String.IsNullOrEmpty(_tempNominativo))
                                        {
                                            almenoUnVistatoreAutorizzato = true;
                                        }
                                    }

                                    if (!almenoUnVistatoreAutorizzato)
                                    {
                                        // se non c'è un vistatore autorizzato allora il documento passa direttamente all'approvatore
                                        _finalList.Add(l);
                                    }
                                }
                                else
                                {
                                    // se non ci sono vistatori allora il documento va in approvazione
                                    _finalList.Add(l);
                                }
                            }
                        }
                        else
                        {
                            _finalList.Add(l);
                        }
                    }

                    result = new List<XR_DEM_DOCUMENTI_EXT>();
                    result.AddRange(_finalList.ToList());
                }
                #endregion
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static List<TBEmailDirezioni> GetEmailDirezione()
        {
            try
            {
                List<TBEmailDirezioni> allMail = new List<TBEmailDirezioni>();
                using (IncentiviEntities db = new IncentiviEntities())
                {
                   var email = db.XR_TB_EMAIL_DIREZIONI.Select(x => new { x.Alias, x.Email } ).ToList();

                    foreach (var item in email)
                    {
                        TBEmailDirezioni x = new TBEmailDirezioni();

                        x.Alias = item.Alias;
                        x.Email = item.Email;
                        allMail.Add(x);
                    }
                }

                return allMail;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        
    }

    public class MatrNom
    {
        public string matricola { get; set; }
        public string nominativo { get; set; }
    }
}