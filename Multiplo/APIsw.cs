
using log4net;
using Multiplo.API.models;


using Multiplo.API.models.ModificaComunicazione;
using Multiplo.API.models.RicercaComunicazione;
using Multiplo.API.models.Token;
using myRaiData.Incentivi;
using myRaiDataTalentia;
using myRaiHelper;
using myRaiHelper.APISW.Models.AnnullaComunicazione;
using myRaiHelper.APISW.Models.CreaComunicazione;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;


namespace Multiplo
{
    public class APIsw
    {
        public static TokenResponse GetToken(log4net.ILog _log)
        {
            WebClient wb = new WebClient();
            string[] par = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.API_SW_UrlToken);

            var nv = new System.Collections.Specialized.NameValueCollection();
            nv.Add("grant_type", "client_credentials");
            nv.Add("scope", par[0]);// "https://apistg.lavoro.gov.it/SmartWorking");

            string[] clientID = CommonHelper.GetParametri<string>(EnumParametriSistema.API_SW_ClientID_Secret);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(clientID[0] + ":" + clientID[1]);
            string b64 = System.Convert.ToBase64String(plainTextBytes);
            wb.Headers.Add("Authorization", "Basic " + b64);

            try
            {
                byte[] b = wb.UploadValues(par[1], nv);
                var str = System.Text.Encoding.Default.GetString(b);
                TokenResponse TR = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(str);
                return TR;
            }
            catch (Exception ex)
            {
                _log.Error("Errore in GetToken:" + ex.ToString());
                return null;
            }

        }

        public static RecediComunicazioneResponse RecediComunicazione(string token, int rowId, WebClient webCl, ILog _log)
        {
            RecediComunicazioneResponse RCR = new RecediComunicazioneResponse();

            RecediComunicazioneAPI R = new RecediComunicazioneAPI();
            R.RecediComunicazione = new List<RecediComunicazione>();

            RecediComunicazione Cred = new RecediComunicazione();
            R.RecediComunicazione.Add(Cred);

            var db = new IncentiviEntities();
            var rowRecedi = db.XR_SW_API.Where(x => x.ID == rowId).FirstOrDefault();

            if (rowRecedi == null)
            {
                string err = "Riferimento revoca non trovato";
                _log.Error(err);
                RCR.Esito = new List<API.models.Esito> { new API.models.Esito() { codice = "Errore", messaggio = err } };
                return RCR;
            }
            var rowRiferimento = db.XR_SW_API.Where(x => x.ID == rowRecedi.ID_RIFERIMENTO_SW_API).FirstOrDefault();
            if (rowRiferimento == null)
            {
                string err = "Codice comunicazione non trovato";
                _log.Error(err);
                RCR.Esito = new List<API.models.Esito> { new API.models.Esito() { codice = "Errore", messaggio = err } };
                return RCR;
            }
            Cred.id = rowId;
            Cred.CodiceComunicazione = rowRiferimento.CODICE_COMUNICAZIONE_API;
            Cred.DataFinePeriodo = rowRecedi.PERIODO_AL.ToString("yyyy-MM-dd");
            Cred.CodTipologiaComunicazione = "R";
            Cred.SezioneSoggettoAbilitato = new API.models.SezioneSoggettoAbilitato();
            Cred.SezioneSoggettoAbilitato.codTipologiaSoggettoAbilitato = null;

            var bodyText = Newtonsoft.Json.JsonConvert.SerializeObject(R);

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
            string RECEDI_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_RecediComunicazioni);

            rowRecedi.DATA_INVIO_API = DateTime.Now;

            string response = webCl.UploadString(RECEDI_COMUNICAZIONI_URL, bodyText);

            rowRecedi.JSON_INVIATO = bodyText;
            rowRecedi.JSON_RICEVUTO = response;
            rowRecedi.DATA_RISPOSTA_API = DateTime.Now;
            db.SaveChanges();

            RCR = Newtonsoft.Json.JsonConvert.DeserializeObject<RecediComunicazioneResponse>(response);

            if (RCR == null || RCR.Esito == null)
            {
                string err = "Risposta nulla o non decodificabile";
                _log.Error(err);

                RCR.Esito = new List<API.models.Esito> {
                    new API.models.Esito() { codice = "Errore", messaggio = err } };
                return RCR;
            }
            var esito = RCR.Esito.FirstOrDefault();
            if (esito == null)
            {
                string err = "Esito non trovato nella risposta";
                _log.Error(err);

                RCR.Esito = new List<API.models.Esito> {
                    new API.models.Esito() { codice = "Errore", messaggio = err } };
                return RCR;
            }
            if (esito.codice == "E100")
            {
                rowRecedi.CODICE_COMUNICAZIONE_API = esito.codiceComunicazione;
                _log.Info("Inviata con successo");
            }
            else
            {
                rowRecedi.ERRORE = esito.codice + " + " + esito.messaggio;
                _log.Error(rowRecedi.ERRORE);
            }
            db.SaveChanges();

            return RCR;
        }
        public static AnnullaComunicazioneResponse AnnullaComunicazione(string token, int IDrowAPi, WebClient webCl, ILog _log)
        {
            WebClient wb = new WebClient();
            wb.Headers.Add("Authorization", "Bearer " + token);
            wb.Headers.Add("Content-Type", "application/json");

            AnnullaComunicazioneResponse ACR = new AnnullaComunicazioneResponse();

            AnnullaComunicazioneAPI A = new AnnullaComunicazioneAPI();
            A.AnnullaComunicazione = new List<AnnullaComunicazione>();

            AnnullaComunicazione Cann = new AnnullaComunicazione();
            A.AnnullaComunicazione.Add(Cann);
            Cann.id = IDrowAPi;
            var db = new IncentiviEntities();

            var rowAnnullaApi = db.XR_SW_API.Where(x => x.ID == IDrowAPi).FirstOrDefault();
            if (rowAnnullaApi == null || rowAnnullaApi.ID_RIFERIMENTO_SW_API == null)
            {
                string err = "Riferimento richiesta API non trovato";
                _log.Error(err);

                ACR.Esito = new List<myRaiHelper.APISW.Models.AnnullaComunicazione.Esito> {
                    new myRaiHelper.APISW.Models.AnnullaComunicazione.Esito() { codice = "Errore", messaggio = err } };
                return ACR;
            }
            var rowApiRiferimento = db.XR_SW_API.Where(x => x.ID == rowAnnullaApi.ID_RIFERIMENTO_SW_API).FirstOrDefault();
            if (rowApiRiferimento == null || String.IsNullOrWhiteSpace(rowApiRiferimento.CODICE_COMUNICAZIONE_API))
            {
                string err = "Codice comunicazione non trovato";
                _log.Error(err);

                ACR.Esito = new List<myRaiHelper.APISW.Models.AnnullaComunicazione.Esito> {
                    new myRaiHelper.APISW.Models.AnnullaComunicazione.Esito() { codice = "Errore", messaggio = err } };
                return ACR;
            }


            Cann.CodiceComunicazione = rowApiRiferimento.CODICE_COMUNICAZIONE_API;
            Cann.CodTipologiaComunicazione = "A";

            var bodyText = Newtonsoft.Json.JsonConvert.SerializeObject(A);

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
            string ANNULLA_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_AnnullaComunicazioni);

            rowAnnullaApi.DATA_INVIO_API = DateTime.Now;
            string response = wb.UploadString(ANNULLA_COMUNICAZIONI_URL, bodyText);

            rowAnnullaApi.JSON_INVIATO = bodyText;
            rowAnnullaApi.JSON_RICEVUTO = response;
            rowAnnullaApi.DATA_RISPOSTA_API = DateTime.Now;
            db.SaveChanges();

            ACR = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnullaComunicazioneResponse>(response);

            if (ACR == null || ACR.Esito == null)
            {
                string err = "Risposta nulla o non decodificabile";
                _log.Error(err);

                ACR.Esito = new List<myRaiHelper.APISW.Models.AnnullaComunicazione.Esito> {
                    new myRaiHelper.APISW.Models.AnnullaComunicazione.Esito() { codice = "Errore", messaggio = err } };
                return ACR;
            }
            var esito = ACR.Esito.FirstOrDefault();
            if (esito == null)
            {
                string err = "Esito non trovato nella risposta";
                _log.Error(err);

                ACR.Esito = new List<myRaiHelper.APISW.Models.AnnullaComunicazione.Esito> {
                    new myRaiHelper.APISW.Models.AnnullaComunicazione.Esito() { codice = "Errore", messaggio = err } };
                return ACR;
            }
            if (esito.codice == "E100")
            {
                rowAnnullaApi.CODICE_COMUNICAZIONE_API = esito.codiceComunicazione;
                _log.Info("Inviata con successo");
            }
            else
            {
                rowAnnullaApi.ERRORE = esito.codice + " + " + esito.messaggio;
                _log.Error(rowAnnullaApi.ERRORE);
            }
            db.SaveChanges();

            return ACR;
        }
        public static ModificaComunicazioneResponse ModificaComunicazione(string token, int ID, WebClient wb, log4net.ILog _log)
        {
            ModificaComunicazioneResponse MCR = new ModificaComunicazioneResponse();
            var db = new IncentiviEntities();
            var API = db.XR_SW_API.Where(x => x.ID == ID).FirstOrDefault();

            if (API == null)
            {
                string err = "Richiesta API non trovata";
                _log.Error(err);

                MCR.Esito = new List<API.models.ModificaComunicazione.Esito> {
                    new API.models.ModificaComunicazione.Esito() { codice = "Errore", messaggio = err } };
                return MCR;
            }
            if (API.ID_RIFERIMENTO_SW_API == null)
            {
                String err = "Impossibile risalire al codice della comunicazione creata";
                _log.Error(err);

                MCR.Esito = new List<API.models.ModificaComunicazione.Esito> {
                    new API.models.ModificaComunicazione.Esito() { codice = "Errore", messaggio = err } };
                return MCR;
            }
            XR_SW_API APIrif = db.XR_SW_API.Where(x => x.ID == API.ID_RIFERIMENTO_SW_API).FirstOrDefault();
            if (APIrif == null || String.IsNullOrWhiteSpace(APIrif.CODICE_COMUNICAZIONE_API))
            {
                string err = "Impossibile risalire al codice comunicazione, il rif ha codice nullo";
                _log.Error(err);

                MCR.Esito = new List<API.models.ModificaComunicazione.Esito> {
                    new API.models.ModificaComunicazione.Esito() { codice = "Errore", messaggio = err} };

                return MCR;
            }
            XR_SW_API APIrifCreazione = db.XR_SW_API.Where(x => x.ID == APIrif.ID).FirstOrDefault();

            for (int i = 1; i < 10; i++)
            {

                if (APIrifCreazione.TIPOLOGIA_API != "I")
                {
                    APIrifCreazione = db.XR_SW_API.Where(x => x.ID == APIrifCreazione.ID_RIFERIMENTO_SW_API).FirstOrDefault();
                    if (APIrifCreazione == null)
                        break;

                    if (APIrifCreazione.TIPOLOGIA_API == "I")
                        break;
                }
                else
                    break;
            }
            if (APIrifCreazione == null || APIrifCreazione.TIPOLOGIA_API != "I")
            {
                string err = "Impossibile risalire al codice di comunicazione creazione";
                _log.Error(err);

                MCR.Esito = new List<API.models.ModificaComunicazione.Esito> {
                    new API.models.ModificaComunicazione.Esito() { codice = "Errore", messaggio =err } };
                return MCR;
            }


            myRaiData.Incentivi.SINTESI1 sintesi = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == API.MATRICOLA).FirstOrDefault();
            if (sintesi == null)
            {
                string err = "Record sintesi non trovato";
                _log.Error(err);

                MCR.Esito = new List<API.models.ModificaComunicazione.Esito> {
                    new API.models.ModificaComunicazione.Esito() { codice = "Errore", messaggio = err } };
                return MCR;
            }
            if (sintesi.COD_IMPRESA != "0")
            {
                string err = "COD_IMPRESA != 0";
                _log.Error(err);
                MCR.Esito = new List<API.models.ModificaComunicazione.Esito> {
                    new API.models.ModificaComunicazione.Esito() { codice = "Errore", messaggio = err } };
                return MCR;
            }



            ModificaComunicazioneAPI M = new ModificaComunicazioneAPI();
            M.ModificaComunicazione = new List<ModificaComunicazione>();

            ModificaComunicazione Cmod = new ModificaComunicazione();
            M.ModificaComunicazione.Add(Cmod);

            Cmod.id = ID;// 1;

            Cmod.CodiceComunicazione = APIrif.CODICE_COMUNICAZIONE_API;
            Cmod.CodTipologiaComunicazione = "M";

            APIutility UT = new APIutility();
            PAT_INAIL PATinfo = UT.GetPatInail(API.MATRICOLA, API.PERIODO_DAL, API.PERIODO_AL);
            if (PATinfo.Errore != null)
            {
                _log.Error(PATinfo.Errore);

                MCR.Esito = new List<API.models.ModificaComunicazione.Esito> {
                    new API.models.ModificaComunicazione.Esito() { codice = "Errore", messaggio = PATinfo.Errore } };
                return MCR;
            }

            string RapportoDiLavoro = UT.GetCodiceTipoContratto(sintesi);
            Cmod.SezioneRapportoLavoro = new API.models.ModificaComunicazione.SezioneRapportoLavoro()
            {
                CodTipologiaRapportoLavoro = RapportoDiLavoro,
                PosizioneINAIL = PATinfo.Pat,
                TariffaINAIL = PATinfo.Inail
            };
            DateTime? DataCompilazione = GetDataCompilazioneFromNuovaComunicazione(APIrifCreazione);
            if (DataCompilazione == null)
            {
                XR_MOD_DIPENDENTI row = IsAccordoFirmato(APIrifCreazione.MATRICOLA, APIrifCreazione.PERIODO_DAL, APIrifCreazione.PERIODO_AL);
                if (row != null)
                    DataCompilazione = row.DATA_COMPILAZIONE;
            }

            if (DataCompilazione == null)
            {
                string err = "Data sottoscrizione non trovata";
                _log.Error(err);

                MCR.Esito = new List<API.models.ModificaComunicazione.Esito> {
                    new API.models.ModificaComunicazione.Esito() { codice = "Errore", messaggio = err } };
                return MCR;
            }

            Cmod.SezioneAccordoSmartWorking = new API.models.ModificaComunicazione.SezioneAccordoSmartWorking()
            {
                DataSottoscrizioneAccordo = DataCompilazione.Value.ToString("yyyy-MM-dd"),
                DataFinePeriodo = API.PERIODO_AL.ToString("yyyy-MM-dd"),
                TipologiaDurataPeriodo = "TD"
            };

            Cmod.SezioneSoggettoAbilitato = new API.models.ModificaComunicazione.SezioneSoggettoAbilitato()
            {
                codTipologiaSoggettoAbilitato = null,// "001",
                codiceFiscaleSoggettoAbilitato = null// ""
            };
            var bodyText = Newtonsoft.Json.JsonConvert.SerializeObject(M);

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
            string MODIFICA_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_ModificaComunicazioni);

            try
            {
                API.DATA_INVIO_API = DateTime.Now;
                string response = wb.UploadString(MODIFICA_COMUNICAZIONI_URL, bodyText);

                API.JSON_INVIATO = bodyText;
                API.JSON_RICEVUTO = response;
                API.DATA_RISPOSTA_API = DateTime.Now;
                db.SaveChanges();

                MCR = Newtonsoft.Json.JsonConvert.DeserializeObject<ModificaComunicazioneResponse>(response);

                if (MCR == null || MCR.Esito == null)
                {
                    string err = "Risposta nulla o non decodificabile";
                    _log.Error(err);

                    MCR.Esito = new List<API.models.ModificaComunicazione.Esito> {
                    new API.models.ModificaComunicazione.Esito() { codice = "Errore", messaggio = err } };
                    return MCR;
                }
                var esito = MCR.Esito.FirstOrDefault();
                if (esito == null)
                {
                    string err = "Esito non trovato nella risposta";
                    _log.Error(err);

                    MCR.Esito = new List<API.models.ModificaComunicazione.Esito> {
                    new API.models.ModificaComunicazione.Esito() { codice = "Errore", messaggio = err } };
                    return MCR;
                }
                if (esito.codice == "E100")
                {
                    API.CODICE_COMUNICAZIONE_API = esito.codiceComunicazione;
                    _log.Info("Inviata con successo");
                }
                else
                {
                    API.ERRORE = esito.codice + " + " + esito.messaggio;
                    _log.Error(API.ERRORE);
                }
                db.SaveChanges();

                return MCR;
            }
            catch (Exception ex)
            {
                API.ERRORE = ex.Message;
                db.SaveChanges();

                _log.Error(ex.ToString());
                MCR.Esito = new List<API.models.ModificaComunicazione.Esito> {
                    new API.models.ModificaComunicazione.Esito() { codice = "Errore", messaggio = ex.ToString() }
                };
                return MCR;
            }
        }

        private static DateTime? GetDataCompilazioneFromNuovaComunicazione(XR_SW_API cODICE_COMUNICAZIONE_API)
        {
            if (cODICE_COMUNICAZIONE_API == null || String.IsNullOrWhiteSpace(cODICE_COMUNICAZIONE_API.JSON_INVIATO))
                return null;

            CreaComunicazioneAPI C = Newtonsoft.Json.JsonConvert.DeserializeObject<CreaComunicazioneAPI>(cODICE_COMUNICAZIONE_API.JSON_INVIATO);
            if (C.Comunicazione != null)
            {
                var com = C.Comunicazione.FirstOrDefault();
                if (com != null)
                {
                    DateTime D;
                    if (DateTime.TryParseExact(com.SezioneAccordoSmartWorking.dataSottoscrizioneAccordo, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out D))
                    {
                        return D;
                    }
                }
            }
            //else
            //{
            //    var db = new TalentiaEntities();
            //    DateTime? Dsottoscrizione= db.XR_MOD_DIPENDENTI.Where(x => x.MATRICOLA == cODICE_COMUNICAZIONE_API.MATRICOLA &&
            //                                   x.DATA_INIZIO == cODICE_COMUNICAZIONE_API.PERIODO_DAL &&
            //                                   x.DATA_SCADENZA == cODICE_COMUNICAZIONE_API.PERIODO_AL &&
            //                                   x.COD_MODULO == "AccordoIndividualeDipendentiSW2022")
            //                                    .Select(x => x.DATA_COMPILAZIONE).FirstOrDefault();
            //    return Dsottoscrizione;

            //}
            return null;
        }



        public static XR_MOD_DIPENDENTI IsAccordoFirmato(string matricola, DateTime D1, DateTime D2, XR_SW_API API=null)
        {
            var db = new TalentiaEntities();

            DateTime Dnow = DateTime.Now;

            var rowSR = db.XR_STATO_RAPPORTO.Where(x =>
                        x.MATRICOLA == matricola &&
                        x.COD_STATO_RAPPORTO == "SW" &&
                        x.DTA_INIZIO <= Dnow && x.DTA_FINE >= Dnow &&
                        //x.DTA_INIZIO == D1 && 
                        x.DTA_FINE == D2 &&
                        x.VALID_DTA_INI <= Dnow &&
                        (x.VALID_DTA_END == null || x.VALID_DTA_END > Dnow) &&
                        (x.COD_TIPO_ACCORDO == "Consensuale" || x.COD_TIPO_ACCORDO == "Deroga ")).FirstOrDefault();

            if (rowSR == null && API != null && API.ID_STATORAPPORTO!=null)
            {
                rowSR = db.XR_STATO_RAPPORTO.Where(x => x.ID_STATO_RAPPORTO == API.ID_STATORAPPORTO).FirstOrDefault();
            }
            if (rowSR == null) return null;

            var rowMD = db.XR_MOD_DIPENDENTI.Where(x =>
                       x.MATRICOLA == matricola &&
                       x.SCELTA == "TRUE" &&
                       x.DATA_COMPILAZIONE != null &&

                       x.DATA_INIZIO != null &&
                       x.DATA_INIZIO<=Dnow && 
                       //&& x.DATA_INIZIO.Value.Year == rowSR.DTA_INIZIO.Year
                       //&& x.DATA_INIZIO.Value.Month == rowSR.DTA_INIZIO.Month
                       //&& x.DATA_INIZIO.Value.Day == rowSR.DTA_INIZIO.Day

                       x.DATA_SCADENZA != null &&
                       x.DATA_SCADENZA.Value.Year == rowSR.DTA_FINE.Year &&
                       x.DATA_SCADENZA.Value.Month == rowSR.DTA_FINE.Month &&
                       x.DATA_SCADENZA.Value.Day == rowSR.DTA_FINE.Day &&

                       x.COD_MODULO == "AccordoIndividualeDipendentiSW2022")
                        .FirstOrDefault();
            return rowMD;
        }
        public static CreaComunicazioneResponse CreaComunicazione(string token, int ID, WebClient wb, log4net.ILog _log,
            DateTime? dataassunzione, XR_MOD_DIPENDENTI roWModDipendenti)
        {


            CreaComunicazioneAPI C = new CreaComunicazioneAPI();
            C.Comunicazione = new List<myRaiHelper.APISW.Models.CreaComunicazione.Comunicazione>();

            myRaiHelper.APISW.Models.CreaComunicazione.Comunicazione Cnew = new myRaiHelper.APISW.Models.CreaComunicazione.Comunicazione();
            C.Comunicazione.Add(Cnew);

            var db = new IncentiviEntities();
            CreaComunicazioneResponse Response = new CreaComunicazioneResponse();

            XR_SW_API API = db.XR_SW_API.Where(x => x.ID == ID).FirstOrDefault();
            if (API == null)
            {
                string err = "Richiesta API non trovata";
                _log.Error(err);

                Response.Esito = new List<CreaComunicazioneEsito>() { new CreaComunicazioneEsito() { codice = "Errore",
                    messaggio = err} };
                return Response;
            }

            myRaiData.Incentivi.SINTESI1 sintesi = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == API.MATRICOLA).FirstOrDefault();
            if (sintesi == null)
            {
                string err = "Record sintesi non trovato";
                _log.Error(err);

                Response.Esito = new List<CreaComunicazioneEsito>() { new CreaComunicazioneEsito() { codice = "Errore",
                    messaggio = err } };
                return Response;
            }
            if (sintesi.COD_IMPRESA != "0")
            {
                string err = "COD_IMPRESA != 0";
                _log.Error(err);

                Response.Esito = new List<CreaComunicazioneEsito>() { new CreaComunicazioneEsito() { codice = "Errore",
                    messaggio = err } };
                return Response;
            }
            Cnew.id = ID.ToString();// "1";
            Cnew.SezioneDatoreLavoro = new SezioneDatoreLavoro()
            {
                codiceFiscaleDatoreLavoro = "06382641006",
                denominazioneDatoreLavoro = "RAI"
            };
            Cnew.SezioneLavoratore = new SezioneLavoratore()
            {
                codiceFiscaleLavoratore = sintesi.CSF_CFSPERSONA,
                nomeLavoratore = sintesi.DES_NOMEPERS,
                cognomeLavoratore = sintesi.DES_COGNOMEPERS,
                dataNascitaLavoratore = sintesi.DTA_NASCITAPERS.Value.ToString("yyyy-MM-dd"),
                codComuneNascitaLavoratore = sintesi.COD_CITTANASC
            };
            APIutility UT = new APIutility();
            PAT_INAIL PATinfo = UT.GetPatInail(API.MATRICOLA, API.PERIODO_DAL, API.PERIODO_AL);
            if (PATinfo.Errore != null)
            {
                _log.Error(PATinfo.Errore);

                Response.Esito = new List<CreaComunicazioneEsito>() { new CreaComunicazioneEsito() { codice = "Errore",
                    messaggio = PATinfo.Errore } };
                return Response;
            }
            string RapportoDiLavoro = UT.GetCodiceTipoContratto(sintesi);

            Cnew.SezioneRapportoLavoro = new myRaiHelper.APISW.Models.CreaComunicazione.SezioneRapportoLavoro
            {
                posizioneINAIL = PATinfo.Pat,
                tariffaINAIL = PATinfo.Inail,
                codTipologiaRapportoLavoro = RapportoDiLavoro,
                dataInizioRapportoLavoro =
                            (dataassunzione != default(DateTime) && dataassunzione != null
                                ? dataassunzione.Value.ToString("yyyy-MM-dd")
                                : sintesi.DTA_ANZCONV.Value.ToString("yyyy-MM-dd"))
            };


            DateTime? DataCompilazione = null;
            if (roWModDipendenti!= null)
                DataCompilazione = roWModDipendenti.DATA_COMPILAZIONE;

            // var dbTal = new TalentiaEntities();
            //var row = dbTal.XR_MOD_DIPENDENTI.Where(x => x.MATRICOLA == API.MATRICOLA &&
            //                        x.COD_MODULO == "AccordoIndividualeDipendentiSW2022" ).FirstOrDefault();

            //if (row == null)
            //{
            //    string err = "Info accordo non trovate";
            //    _log.Error(err);
            //    Response.Esito = new List<CreaComunicazioneEsito>() { new CreaComunicazioneEsito() { codice = "Errore",
            //        messaggio = err } };
            //    return Response;
            //}
            //else
            //{
            //    if (row.DATA_COMPILAZIONE == null)
            //    {
            //        if (DateTime.Now > row.DATA_SCADENZA)
            //        {
            //            string err = "Firma accordo non trovata (scadenza superata)";
            //            _log.Error(err);
            //            Response.Esito = new List<CreaComunicazioneEsito>() { new CreaComunicazioneEsito() { codice = "Errore",
            //                             messaggio = err } };
            //            return Response;
            //        }
            //        else
            //        {
            //            string err = "Firma accordo non ancora presente";
            //            _log.Error(err);
            //            Response.Esito = new List<CreaComunicazioneEsito>() { new CreaComunicazioneEsito() { codice = "Errore",
            //                             messaggio = err } };
            //            return Response;
            //        }
            //    }
            //    else
            //        DataCompilazione = row.DATA_COMPILAZIONE;
            //}

            Cnew.SezioneAccordoSmartWorking = new myRaiHelper.APISW.Models.CreaComunicazione.SezioneAccordoSmartWorking()
            {
                dataFinePeriodo = API.PERIODO_AL.ToString("yyyy-MM-dd"),
                dataInizioPeriodo = API.PERIODO_DAL.ToString("yyyy-MM-dd"),
                dataSottoscrizioneAccordo = DataCompilazione.Value.ToString("yyyy-MM-dd"),
                //mesiDurataAccordo = 3, //opzionale
                tipologiaDurataPeriodo = "TD" // durata determinata ?????????????????????????????????????????
            };
            if (DataCompilazione != null && API.PERIODO_DAL < DataCompilazione.Value)
            {
                Cnew.SezioneAccordoSmartWorking.dataInizioPeriodo = DataCompilazione.Value.ToString("yyyy-MM-dd");
            }
            if (API.ERRORE != null && API.ERRORE.StartsWith("E037"))
            {
                Cnew.SezioneAccordoSmartWorking.dataInizioPeriodo = DataCompilazione.Value.ToString("yyyy-MM-dd");
            }
            Cnew.SezioneSoggettoAbilitato = new myRaiHelper.APISW.Models.CreaComunicazione.SezioneSoggettoAbilitato()
            {
                codiceFiscaleSoggettoAbilitato = null,// facoltativo
                codTipologiaSoggettoAbilitato = null// facoltativo
            };
            Cnew.codTipologiaComunicazione = "I";

            var bodyText = Newtonsoft.Json.JsonConvert.SerializeObject(C);

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
            string CREA_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_CreaComunicazioni);



            try
            {
                API.DATA_INVIO_API = DateTime.Now;
                string response = wb.UploadString(CREA_COMUNICAZIONI_URL, bodyText);

                API.JSON_INVIATO = bodyText;
                API.JSON_RICEVUTO = response;
                API.DATA_RISPOSTA_API = DateTime.Now;
                db.SaveChanges();

                CreaComunicazioneResponse CCR = Newtonsoft.Json.JsonConvert.DeserializeObject<CreaComunicazioneResponse>(response);
                if (CCR == null || CCR.Esito == null)
                {
                    string err = "Risposta nulla o non decodificabile";
                    _log.Error(err);

                    Response.Esito = new List<CreaComunicazioneEsito>() { new CreaComunicazioneEsito() { codice = "Errore",
                    messaggio = err } };

                    return Response;
                }
                var esito = CCR.Esito.FirstOrDefault();
                if (esito == null)
                {
                    string err = "Esito non trovato nella risposta";
                    _log.Error(err);
                    Response.Esito = new List<CreaComunicazioneEsito>() { new CreaComunicazioneEsito() { codice = "Errore",
                    messaggio =err } };

                    return Response;
                }
                if (esito.codice == "E100")
                {
                    API.CODICE_COMUNICAZIONE_API = esito.codiceComunicazione;
                    API.ERRORE = null;
                    _log.Info("Inviata con successo");
                }
                else
                {
                    API.ERRORE = esito.codice + " + " + esito.messaggio;
                    _log.Error(API.ERRORE);
                }
                db.SaveChanges();

                return CCR;
            }
            catch (Exception ex)
            {
                API.ERRORE = ex.Message;
                db.SaveChanges();

                _log.Error(ex.ToString());

                Response.Esito = new List<CreaComunicazioneEsito>() { new CreaComunicazioneEsito() { codice = "Errore",
                    messaggio = ex.ToString() } };
                return Response;

            }
        }

        public static RicercaComunicazioneResponse RicercaComunicazione(string token)
        {
            /*
            Obbligatorio almeno uno tra CFLavoratore e CFAzienda

            Datainizio/datafine Obbligatori se non è stato indicato CFLavoratore.
            Indicare un periodo non superiore ad un mese

             */
            WebClient wb = new WebClient();
            wb.Headers.Add("Authorization", "Bearer " + token);
            wb.Headers.Add("Content-Type", "application/json");
            RicercaComunicazioneAPI R = new RicercaComunicazioneAPI();

            R.CFLavoratore = "";
            R.CFAzienda = "12312312355";
            R.dataInizio = new DateTime(2020, 1, 1);
            R.dataFine = new DateTime(2020, 12, 31);

            var bodyText = Newtonsoft.Json.JsonConvert.SerializeObject(R);

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
            string RICERCA_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_RicercaComunicazioni);

            string response = wb.UploadString(RICERCA_COMUNICAZIONI_URL, bodyText);
            RicercaComunicazioneResponse RCR = Newtonsoft.Json.JsonConvert.DeserializeObject<RicercaComunicazioneResponse>(response);
            return RCR;
        }

        public static myRaiHelper.APISW.Models.DettaglioComunicazione.DettaglioComunicazioneResponse DettaglioComunicazione(string token, string codiceComunicazione)
        {
            WebClient wb = new WebClient();
            wb.Headers.Add("Authorization", "Bearer " + token);
            wb.Headers.Add("Content-Type", "application/json");
            myRaiHelper.APISW.Models.DettaglioComunicazione.DettaglioComunicazioneAPI D = new myRaiHelper.APISW.Models.DettaglioComunicazione.DettaglioComunicazioneAPI();

            D.CodiceComunicazione = codiceComunicazione;


            var bodyText = Newtonsoft.Json.JsonConvert.SerializeObject(D);

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
            string DETTAGLIO_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_DettaglioComunicazioni);

            string response = wb.UploadString(DETTAGLIO_COMUNICAZIONI_URL, bodyText);
            myRaiHelper.APISW.Models.DettaglioComunicazione.DettaglioComunicazioneResponse DCR = Newtonsoft.Json.JsonConvert.DeserializeObject<myRaiHelper.APISW.Models.DettaglioComunicazione.DettaglioComunicazioneResponse>(response);
            return DCR;
        }


    }


}
