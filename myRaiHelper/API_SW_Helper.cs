using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace myRaiHelper.Token
{
    public class TokenAPI
    {

        public static int InserisciApiNuova(myRaiDataTalentia.XR_STATO_RAPPORTO Record, string matricolaOperatore = null)
        {
            var dbczn = new myRaiData.Incentivi. IncentiviEntities();
            myRaiData.Incentivi. XR_SW_API api = new myRaiData.Incentivi.XR_SW_API()
            {
                DATA_CREAZIONE = DateTime.Now,
                ID_STATORAPPORTO = Record.ID_STATO_RAPPORTO,
                MATRICOLA = Record.MATRICOLA,
                MATRICOLA_OPERATORE = matricolaOperatore == null ? CommonHelper.GetCurrentUserMatricola() : matricolaOperatore,
                TIPOLOGIA_API = "I",
                PERIODO_DAL = Record.DTA_INIZIO,
                PERIODO_AL = Record.DTA_FINE
            };
            dbczn.XR_SW_API.Add(api);
            dbczn.SaveChanges();
            myRaiHelper.Logger.LogAzione(
                    new myRaiData.MyRai_LogAzioni()
                    {
                        operazione = "InvioAPI nuova",
                        descrizione_operazione = "idStatoRapporto:" + Record.ID_STATO_RAPPORTO + " id api :" + api.ID
                    });
            return api.ID;
        }

        public static string AllineaDaRestVersoDBmatricola(string matricola, string cf, WebClient wb, string token, int? ID_RowSWAPI = null)
        {
            string RICERCA_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_RicercaComunicazioni);
            var dbczn = new myRaiData.Incentivi. IncentiviEntities();
            var dbtal = new myRaiDataTalentia.TalentiaEntities();

            string response = wb.DownloadString(RICERCA_COMUNICAZIONI_URL + "?CFLavoratore=" + cf);
            myRaiHelper.APISW.Models.RicercaComunicazione.RicercaComunicazioniResponse RCR = Newtonsoft.Json.JsonConvert.DeserializeObject<myRaiHelper.APISW.Models.RicercaComunicazione.RicercaComunicazioniResponse>(response);
            if (RCR.Comunicazioni != null && RCR.Comunicazioni.Any())
            {
                var comPresente = RCR.Comunicazioni.First();
                string CodiceRiferimentoAPI = comPresente.codiceComunicazione;
                //var respDettaglio = APIsw.DettaglioComunicazione(token, CodiceRiferimentoAPI);

                DateTime Dnow = DateTime.Now;
                var SR = dbtal.XR_STATO_RAPPORTO.Where(x =>
                     x.MATRICOLA == matricola
                     && x.VALID_DTA_INI < Dnow
                     && (x.VALID_DTA_END == null || x.VALID_DTA_END > Dnow)
                     && x.DTA_INIZIO < Dnow &&
                     x.DTA_FINE > Dnow)
                .FirstOrDefault();

                if (SR == null) //se non ha trovato periodo attuale, ignora il periodo (magari è un periodo scaduto)
                {
                    SR = dbtal.XR_STATO_RAPPORTO.Where(x =>
                     x.MATRICOLA == matricola
                     && x.VALID_DTA_INI < Dnow
                     && (x.VALID_DTA_END == null || x.VALID_DTA_END > Dnow)
                      ).FirstOrDefault();
                }

                //inserisci come nuova gia con codice riferimento api
                int IDswAPI =
                    (ID_RowSWAPI == null ? InserisciApiNuova(SR)  : (int)ID_RowSWAPI);

                var RowswAPI = dbczn.XR_SW_API.Where(x => x.ID == IDswAPI).FirstOrDefault();

                RowswAPI.CODICE_COMUNICAZIONE_API = CodiceRiferimentoAPI;
                RowswAPI.DATA_RISPOSTA_API = DateTime.Now;
                RowswAPI.DATA_INVIO_API = DateTime.Now;
                RowswAPI.ERRORE = null;
                dbczn.SaveChanges();
                return null;

            }
            else
                return "Non ci sono comunicazioni nella risposta " + response;
        }

        public static void RecuperaSRinApiDB(int idSR)
        {

            TokenResponse token = GetToken();
            WebClient wb = GetWebClient(token.access_token);

            var db = new myRaiDataTalentia.TalentiaEntities();
            var SR = db.XR_STATO_RAPPORTO.Where(x => x.ID_STATO_RAPPORTO == idSR).FirstOrDefault();

            if (SR == null)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori() {
                     error_message="SR nullo per id SR " + idSR,
                      provenienza= "RecuperaSRinApiDB"
                });
            }
            var cf = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == SR.MATRICOLA).Select(x => x.CSF_CFSPERSONA).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(cf))
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    error_message = "CF non valido per id SR " + idSR,
                    provenienza = "RecuperaSRinApiDB"
                });
            }
            string esito = AllineaDaRestVersoDBmatricola(SR.MATRICOLA, cf, wb, token.access_token);
            if (!String.IsNullOrWhiteSpace(esito))
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori() {error_message=esito, provenienza= "RecuperaSRinApiDB" });
            }
        }
        public static WebClient GetWebClient(string access_token)
        {
            WebClient wb = new WebClient();
            wb.Headers.Add("Authorization", "Bearer " + access_token);
            wb.Headers.Add("Content-Type", "application/json");
            return wb;
        }
        public static TokenResponse GetToken()
        {
            string[] UrlToken = CommonHelper.GetParametri<string>(EnumParametriSistema.API_SW_UrlToken);
            WebClient wb = new WebClient();
            var nv = new System.Collections.Specialized.NameValueCollection();
            nv.Add("grant_type", "client_credentials");
            //nv.Add("scope", "https://apistg.lavoro.gov.it/SmartWorking"); //collaudo
            nv.Add("scope", UrlToken[0]);

            string[] clientID = CommonHelper.GetParametri<string>(EnumParametriSistema.API_SW_ClientID_Secret);

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(clientID[0] + ":" + clientID[1]);
            string b64 = System.Convert.ToBase64String(plainTextBytes);
            wb.Headers.Add("Authorization", "Basic " + b64);

            try
            {   //collaudo
                //byte[] b = wb.UploadValues("https://idcs-6d2d9744bc754611b6e5d0c63933296a.identity.oraclecloud.com/oauth2/v1/token",
                // nv);
                byte[] b = wb.UploadValues(UrlToken[1], nv);
                var str = System.Text.Encoding.Default.GetString(b);
                TokenResponse TR = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(str);
                return TR;
            }
            catch (Exception ex)
            {

                return null;
            }
        }
    }
    public class TokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }

        
    }
}

namespace myRaiHelper.CreaComunicazioni

{

    public class Rootobject
    {
        public Comunicazione[] Comunicazione { get; set; }
    }

    public class Comunicazione
    {
        public string id { get; set; }
        public Sezionedatorelavoro SezioneDatoreLavoro { get; set; }
        public Sezionelavoratore SezioneLavoratore { get; set; }
        public Sezionerapportolavoro SezioneRapportoLavoro { get; set; }
        public Sezioneaccordosmartworking SezioneAccordoSmartWorking { get; set; }
        public Sezionesoggettoabilitato SezioneSoggettoAbilitato { get; set; }
        public string codTipologiaComunicazione { get; set; }
    }

    public class Sezionedatorelavoro
    {
        public string codiceFiscaleDatoreLavoro { get; set; }
        public string denominazioneDatoreLavoro { get; set; }
    }

    public class Sezionelavoratore
    {
        public string codiceFiscaleLavoratore { get; set; }
        public string nomeLavoratore { get; set; }
        public string cognomeLavoratore { get; set; }
        public string dataNascitaLavoratore { get; set; }
        public string codComuneNascitaLavoratore { get; set; }
    }

    public class Sezionerapportolavoro
    {
        public string dataInizioRapportoLavoro { get; set; }
        public string codTipologiaRapportoLavoro { get; set; }
        public string posizioneINAIL { get; set; }
        public string tariffaINAIL { get; set; }
    }

    public class Sezioneaccordosmartworking
    {
        public string dataSottoscrizioneAccordo { get; set; }
        public string dataInizioPeriodo { get; set; }
        public string dataFinePeriodo { get; set; }
        public string tipologiaDurataPeriodo { get; set; }
        public int mesiDurataAccordo { get; set; }
    }

    public class Sezionesoggettoabilitato
    {
        public string codTipologiaSoggettoAbilitato { get; set; }
        public string codiceFiscaleSoggettoAbilitato { get; set; }
    }

}

namespace myRaiHelper.ModificaComunicazioni
{

    public class Rootobject
    {
        public Modificacomunicazione[] ModificaComunicazione { get; set; }
    }

    public class Modificacomunicazione
    {
        public int id { get; set; }
        public string CodiceComunicazione { get; set; }
        public Sezionerapportolavoro SezioneRapportoLavoro { get; set; }
        public Sezioneaccordosmartworking SezioneAccordoSmartWorking { get; set; }
        public Sezionesoggettoabilitato SezioneSoggettoAbilitato { get; set; }
        public string CodTipologiaComunicazione { get; set; }
    }

    public class Sezionerapportolavoro
    {
        public string CodTipologiaRapportoLavoro { get; set; }
        public string PosizioneINAIL { get; set; }
        public string TariffaINAIL { get; set; }
    }

    public class Sezioneaccordosmartworking
    {
        public string DataSottoscrizioneAccordo { get; set; }
        public string DataFinePeriodo { get; set; }
        public string TipologiaDurataPeriodo { get; set; }
    }

    public class Sezionesoggettoabilitato
    {
        public string codTipologiaSoggettoAbilitato { get; set; }
        public string codiceFiscaleSoggettoAbilitato { get; set; }
    }

}

namespace myRaiHelper.RecediComunicazioni
{

    public class Rootobject
    {
        public Recedicomunicazione[] RecediComunicazione { get; set; }
    }

    public class Recedicomunicazione
    {
        public int id { get; set; }
        public string CodiceComunicazione { get; set; }
        public string DataFinePeriodo { get; set; }
        public Sezionesoggettoabilitato SezioneSoggettoAbilitato { get; set; }
        public string CodTipologiaComunicazione { get; set; }
    }

    public class Sezionesoggettoabilitato
    {
        public string codTipologiaSoggettoAbilitato { get; set; }
    }

}

namespace myRaiHelper.AnnullaComunicazioni
{
    public class Rootobject
    {
        public Annullacomunicazione[] AnnullaComunicazione { get; set; }
    }

    public class Annullacomunicazione
    {
        public int id { get; set; }
        public string CodiceComunicazione { get; set; }
        public string CodTipologiaComunicazione { get; set; }
    }
}

namespace myRaiHelper.InfoComunicazione
{
    public class InfoComunicazioneAPI
    {
        public static myRaiHelper.InfoComunicazione.InfoComunicazioneResponse InfoComunicazione(string codice, string access_token)
        {
            //dettaglioComunicazione
            WebClient wb = new WebClient();
            wb.Headers.Add("Authorization", "Bearer " + access_token);
            wb.Headers.Add("Content-Type", "application/json");

            //InfoComunicazioneAPI api = new myRaiHelper.InfoComunicazione.InfoComunicazioneAPI() { CodiceComunicazione = codice };
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };

            string INFO_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_DettaglioComunicazioni);

            //var bodyText = Newtonsoft.Json.JsonConvert.SerializeObject(api);

            string response = wb.DownloadString(INFO_COMUNICAZIONI_URL + "?CodiceComunicazione=" + codice);

            InfoComunicazioneResponse ICR = Newtonsoft.Json.JsonConvert.DeserializeObject<InfoComunicazioneResponse>(response);
            return ICR;
        }
    }
    public class Comunicazione
    {
        public string desTipologiaComunicazione { get; set; }
        public string CFAzienda { get; set; }
        public string codComuneStatoEsteroNascitaLavoratore { get; set; }
        public string codiceIdentificativoPeriodoSmartWorking { get; set; }
        public string denominazioneAzienda { get; set; }
        public string tariffaINAIL { get; set; }
        public object idComunicazionePrecedente { get; set; }
        public DateTime dataInizioRapporto { get; set; }
        public DateTime dataInvio { get; set; }
        public object cFSoggettoAbilitato { get; set; }
        public object codTipoAltroSoggetto { get; set; }
        public string codiceComunicazione { get; set; }
        public string cognomeLavoratore { get; set; }
        public object codiceFiscaleOperatore { get; set; }
        public string desTipologiaRapportoLavoro { get; set; }
        public object idLavoratore { get; set; }
        public object dataInizioPeriodo { get; set; }
        public DateTime dataSottoscrizioneAccordo { get; set; }
        public DateTime dataNascitaLavoratore { get; set; }
        public string tipologiaDurataPeriodo { get; set; }
        public string applicazione { get; set; }
        public string posizioneINAIL { get; set; }
        public DateTime dataFineAccordo { get; set; }
        public string codTipologiaRapportoLavoro { get; set; }
        public string nomeLavoratore { get; set; }
        public string GUIDUtente { get; set; }
        public object mesiDurata { get; set; }
        public int flagUltimo { get; set; }
        public string codTipologiaComunicazione { get; set; }
        public object ricevutaPDF { get; set; }
        public string CFLavoratore { get; set; }
        public object desTipoAltroSoggetto { get; set; }
        public string idAzienda { get; set; }
        public DateTime dataUltimaModifica { get; set; }
        public object streamPDF { get; set; }
    }

    public class Esito
    {
        public string Messaggio { get; set; }
        public string Linguaggio { get; set; }
        public string Codice { get; set; }
    }

    public class InfoComunicazioneResponse
    {
        public Esito Esito { get; set; }
        public Comunicazione Comunicazione { get; set; }
    }
   
}

namespace myRaiHelper.RicercaComunicazione
{
    public class RicercaComunicazioneApi
    {
        public string CFLavoratore { get; set; }
        public string CFAzienda { get; set; }
        public DateTime? dataInizio { get; set; }
        public DateTime? dataFine { get; set; }

        public static myRaiHelper.RicercaComunicazione.RicercaComunicazioniResponse RicercaComunicazioni(string cf, string access_token)
        {
            WebClient wb = new WebClient();
            wb.Headers.Add("Authorization", "Bearer " + access_token);
            wb.Headers.Add("Content-Type", "application/json");

            var db = new myRaiData.Incentivi. IncentiviEntities();
            //myRaiHelper.RicercaComunicazione.RicercaComunicazioneApi api = new myRaiHelper.RicercaComunicazione.RicercaComunicazioneApi() { CFLavoratore = cf };
            //RicercaComunicazioneApi api = new RicercaComunicazioneApi() { CFAzienda  = "06382641006" , dataInizio= new DateTime(2022,11,1),
            //dataFine= new DateTime (2022,11,29)};

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
            string RICERCA_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_RicercaComunicazioni);

            //var bodyText = Newtonsoft.Json.JsonConvert.SerializeObject(api);
            //CompilaCSV( wb);
            string response = wb.DownloadString(RICERCA_COMUNICAZIONI_URL + "?CFLavoratore=" + cf);

            //string response = wb.DownloadString(RICERCA_COMUNICAZIONI_URL + "?CFAzienda=06382641006&dataInizio=2020-01-01&dataFine=2020-01-31" );

            myRaiHelper.RicercaComunicazione.RicercaComunicazioniResponse RCR = Newtonsoft.Json.JsonConvert.DeserializeObject<myRaiHelper.RicercaComunicazione.RicercaComunicazioniResponse>(response);
            //var lav = RCR.Comunicazioni.Where(x => x.cognomeLavoratore.ToUpper() == "POZZAN").FirstOrDefault();
            return RCR;
        }
    }

    public class ComunicazioniRicerca
    {
        public DateTime dataInizioRapporto { get; set; }
        public DateTime dataInvio { get; set; }
        public DateTime dataInizioPeriodo { get; set; }
        public string codComuneStatoEsteroNascitaLavoratore { get; set; }
        public string CFAzienda { get; set; }
        public string CFLavoratore { get; set; }
        public DateTime dataNascitaLavoratore { get; set; }
        public string denominazioneAzienda { get; set; }
        public DateTime dataFineAccordo { get; set; }
        public string codiceComunicazione { get; set; }
        public string nomeLavoratore { get; set; }
        public string cognomeLavoratore { get; set; }
    }

    public class EsitoRicerca
    {
        public string Messaggio { get; set; }
        public string Linguaggio { get; set; }
        public string Codice { get; set; }
    }

    public class RicercaComunicazioniResponse
    {
        public List<ComunicazioniRicerca> Comunicazioni { get; set; }
        public EsitoRicerca Esito { get; set; }
    }
}