using iTextSharp.text.pdf;
using myRai.Business;
using myRai.Models;
using myRaiCommonModel;
using myRaiCommonTasks;
using myRaiCommonTasks.Helpers;
using myRaiCommonTasks.sendMail;
using myRaiData;
using myRaiData.Incentivi;
using myRaiDataTalentia;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Logger = myRaiHelper.Logger;

namespace myRaiGestionale.Controllers
{
    /// <summary>
    /// Rappresenta gli acro-fields del report Variazione Quota TFR.
    /// </summary>
    public class VariazioneAliquotaTfrAcroField
    {

        public const String AZIENDA_APPARTENENZA = "AziendaAppartenenza";
        public const String CODICE_FISCALE_01 = "CodiceFiscale1";
        public const String CODICE_FISCALE_02 = "CodiceFiscale2";
        public const String CODICE_FISCALE_03 = "CodiceFiscale3";
        public const String CODICE_FISCALE_04 = "CodiceFiscale4";
        public const String CODICE_FISCALE_05 = "CodiceFiscale5";
        public const String CODICE_FISCALE_06 = "CodiceFiscale6";
        public const String CODICE_FISCALE_07 = "CodiceFiscale7";
        public const String CODICE_FISCALE_08 = "CodiceFiscale8";
        public const String CODICE_FISCALE_09 = "CodiceFiscale9";
        public const String CODICE_FISCALE_10 = "CodiceFiscale10";
        public const String CODICE_FISCALE_11 = "CodiceFiscale11";
        public const String CODICE_FISCALE_12 = "CodiceFiscale12";
        public const String CODICE_FISCALE_13 = "CodiceFiscale13";
        public const String CODICE_FISCALE_14 = "CodiceFiscale14";
        public const String CODICE_FISCALE_15 = "CodiceFiscale15";
        public const String CODICE_FISCALE_16 = "CodiceFiscale16";
        public const String FIRMA_ADERENTE = "FirmaAderente";
        public const String FIRMA_ADERENTE_LUOGO_DATA = "LuogoDataFirmaAderente";
        public const String ISCRIZIONE_CRAIPI_DATA = "DataIscrizioneCraipi";
        public const String ISCRIZIONE_CRAIPI_NUMERO = "NumeroIscrizioneCraipi";
        public const String NASCITA_ANNO = "AnnoNascita";
        public const String NASCITA_GIORNO = "GiornoNascita";
        public const String NASCITA_LUOGO = "LuogoNascita";
        public const String NASCITA_MESE = "MeseNascita";
        public const String NASCITA_PROVINCIA = "ProvinciaNascita";
        public const String NOME_COGNOME = "NomeCognome";
        public const String PERCENTUALE_TFR = "NuovaPercentualeTfr";
        public const String PERCENTUALE_TFR_ANNO_VARIAZIONE = "AnnoVariazionePercentualeTfr";
        public const String RESIDENZA_LUOGO = "LuogoResidenza";
        public const String RESIDENZA_PROVINCIA = "ProvinciaResidenza";
        public const String RESIDENZA_CAP = "CapResidenza";
        public const String RESIDENZA_INDIRIZZO = "IndirizzoResidenza";
        public const String RESIDENZA_NUMERO_CIVICO = "NumeroCivicoResidenza";
        public const String TELEFONO = "Telefono";
    }

    public class AcroFieldTemplate
    {
        // Intestazione
        public const string NOME_COGNOME = "NomeCognome";
        public const string NATO_A = "NatoA";
        public const string NATO_IL = "NatoIl";
        public const string CODICE_FISCALE = "CodiceFiscale";
        public const string DIPENDENTE_DEL = "DipendenteDel";

        // Sezione 1
        public const string SEZIONE1_IN_AZIENDA = "Sezione1_InAzienda";

        public const string SEZIONE1_NO_IN_AZIENDA = "Sezione1_NoInAzienda";
        public const string SEZIONE1_PERCENTUALE = "Sezione1_Percentuale";
        public const string SEZIONE1_FORMA_COMPLEMENTARE = "Sezione1_FormaComplementare";
        public const string SEZIONE1_GIORNO = "Sezione1_Giorno";
        public const string SEZIONE1_MESE = "Sezione1_Mese";
        public const string SEZIONE1_ANNO = "Sezione1_Anno";

        // Sezione 2
        public const string SEZIONE2_IN_AZIENDA = "Sezione2_InAzienda";

        public const string SEZIONE2_NO_IN_AZIENDA_PERCENTUALE = "Sezione2_NoInAzienda_Percentuale";
        public const string SEZIONE2_PERCENTUALE = "Sezione2_Percentuale";
        public const string SEZIONE2_FORMA_COMPLEMENTARE_PERCENTUALE = "Sezione2_FormaComplementare_Percentuale";
        public const string SEZIONE2_GIORNO_PERCENTUALE = "Sezione2_Giorno_Percentuale";
        public const string SEZIONE2_MESE_PERCENTUALE = "Sezione2_Mese_Percentuale";
        public const string SEZIONE2_ANNO_PERCENTUALE = "Sezione2_Anno_Percentuale";

        public const string SEZIONE2_NO_IN_AZIENDA_INTERO = "Sezione2_NoInAzienda_Intero";
        public const string SEZIONE2_FORMA_COMPLEMENTARE_INTERO = "Sezione2_FormaComplementare_Intero";
        public const string SEZIONE2_GIORNO_INTERO = "Sezione2_Giorno_Intero";
        public const string SEZIONE2_MESE_INTERO = "Sezione2_Mese_Intero";
        public const string SEZIONE2_ANNO_INTERO = "Sezione2_Anno_Intero";

        // Sezione 3
        public const string SEZIONE3_IN_AZIENDA = "Sezione3_InAzienda";

        public const string SEZIONE3_NO_IN_AZIENDA_PERCENTUALE = "Sezione3_NoInAzienda_Percentuale";
        public const string SEZIONE3_PERCENTUALE = "Sezione3_Percentuale";
        public const string SEZIONE3_FORMA_COMPLEMENTARE_PERCENTUALE = "Sezione3_FormaComplementare_Percentuale";
        public const string SEZIONE3_GIORNO_PERCENTUALE = "Sezione3_Giorno_Percentuale";
        public const string SEZIONE3_MESE_PERCENTUALE = "Sezione3_Mese_Percentuale";
        public const string SEZIONE3_ANNO_PERCENTUALE = "Sezione3_Anno_Percentuale";

        public const string SEZIONE3_NO_IN_AZIENDA_INTERO = "Sezione3_NoInAzienda_Intero";
        public const string SEZIONE3_FORMA_COMPLEMENTARE_INTERO = "Sezione3_FormaComplementare_Intero";
        public const string SEZIONE3_GIORNO_INTERO = "Sezione3_Giorno_Intero";
        public const string SEZIONE3_MESE_INTERO = "Sezione3_Mese_Intero";
        public const string SEZIONE3_ANNO_INTERO = "Sezione3_Anno_Intero";

        // Sezione Firma Statica
        public const string SEZIONE_DATA = "Data_Validazione";
        public const string SEZIONE_FIRMA_RIGA_1 = "Firma_Riga_1";
        public const string SEZIONE_FIRMA_RIGA_2 = "Firma_Riga_2";

        public const string SEZIONE_FIRMA_RIGA_1_PLACEHOLDER = "-----------------------------";
        public const string SEZIONE_FIRMA_RIGA_1_MESSAGGIO = "Firmato digitalmente da";
        public const string SEZIONE_FIRMA_RIGA_2_PLACEHOLDER = "      (firma leggibile)      ";
    }

    public enum SezioneDaCompilareTemplate
    {
        Nessuna,
        Sezione_1,
        Sezione_2,
        Sezione_3,
    }

    public class SceltaDestinazioneTfrController : Controller
    {
        private string CodModulo => "TFR"; // Codice del modulo utilizzato per il reperimento del modulo dal db di Talentia
        private string CodModulo_PDF => "TFR_C"; // Codice del modulo utilizzato per il reperimento del modulo dal db di Talentia
        private string CodModulo_PDF_VariazioneAliquotaTfr => "TFR_V"; // Codice del modulo utilizzato per il reperimento del modulo dal db di Talentia
        private int IdUtente { get; set; }
        private string MatricolaUtente { get; set; }
        private int IdRecordScelta { get; set; }
        private string P_MatricolaUtenteLoggato => CommonManager.GetCurrentUserPMatricola();
        //private DateTime? DataAssunzioneUtente => Utente.EsponiAnagrafica()._dataAssunzione;
        private string TipologiaFileUpload => "TFR";
        private string Applicazione => "PORTALE - TFR"; // Nome applicazione utilizzata nel log
        private string Provenienza => "Modulo scelta destinazione TFR"; // Provenienza applicazione utilizzata nel log
        private string CodiceFunzioneOtp => "04";

        // Intestazione pdf
        private string CognomeNomePdf => $"{Utente.EsponiAnagrafica()._cognome.Trim()} {Utente.EsponiAnagrafica()._nome.Trim()}";
        private string NatoAPdf => Utente.EsponiAnagrafica()._comuneNascita.Trim();
        private string NatoIlPdf => Utente.EsponiAnagrafica()._dataNascita.GetValueOrDefault().ToString("dd/MM/yyyy");
        private string CodiceFiscalePdf => Utente.EsponiAnagrafica()._cf.Trim();
        private string DipendenteDelPdf => Utente.EsponiAnagrafica()._logo.Trim();

        protected override void Initialize(RequestContext requestContext)
        {
            var context = requestContext.HttpContext;
            var queryString = context.Request.QueryString;
            var form = context.Request.Form;

            // Cerco i parametri prima in query string e poi in form

            if (queryString != null && !string.IsNullOrWhiteSpace(queryString["idUtente"]))
            {
                IdUtente = Convert.ToInt32(queryString["idUtente"]);
            }
            else if (form != null && form.Keys.Count > 0 && !string.IsNullOrWhiteSpace(form["idUtente"]))
            {
                IdUtente = Convert.ToInt32(form["idUtente"]);
            }

            if (queryString != null && !string.IsNullOrWhiteSpace(queryString["matricolaUtente"]))
            {
                MatricolaUtente = queryString["matricolaUtente"].ToString();
            }
            else if (form != null && form.Keys.Count > 0 && !string.IsNullOrWhiteSpace(form["matricolaUtente"]))
            {
                MatricolaUtente = form["matricolaUtente"].ToString();
            }

            if (queryString != null && !string.IsNullOrWhiteSpace(queryString["idRecordScelta"]))
            {
                IdRecordScelta = Convert.ToInt32(queryString["idRecordScelta"]);
            }
            else if (form != null && form.Keys.Count > 0 && !string.IsNullOrWhiteSpace(form["idRecordScelta"]))
            {
                IdRecordScelta = Convert.ToInt32(form["idRecordScelta"]);
            }

            base.Initialize(requestContext);
        }

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Metodo che restituisce il contenitore che viene aperto nel modal
        /// </summary>
        /// <returns></returns>
        public ActionResult GetModuloSceltaDestinazioneTfr()
        {
            ViewBag.MatricolaUtente = MatricolaUtente;
            ViewBag.IdUtente = IdUtente;
            ViewBag.IdRecordScelta = IdRecordScelta;

            SceltaDestinazioneTfrScelteEffettuateModel valoriModuloFromDb = GetValoriModuloFromDb();
            if (valoriModuloFromDb != null && valoriModuloFromDb.StepCorrenteAmministrazione == StepsEnum.SCELTE_EFFETTUATE)
            {
                return PartialView("ModuloSceltaDestinazioneTfrFinale");
            }
            else
            {
                return PartialView("ModuloSceltaDestinazioneTfrMain");
            }
        }

        /// <summary>
        /// Metodo che restituisce il contenitore che viene aperto nel modal
        /// </summary>
        /// <returns></returns>
        public ActionResult GetModuloSceltaDestinazioneTfrFinale()
        {
            ViewBag.MatricolaUtente = MatricolaUtente;
            ViewBag.IdUtente = IdUtente;
            ViewBag.IdRecordScelta = IdRecordScelta;

            return PartialView("ModuloSceltaDestinazioneTfrFinale");
        }

        /// <summary>
        /// Metodo che riemmpie per la prima volta il container del modal. Viene invocato da una chiamata ajax nel document.ready della view ModuloSceltaDestinazioneTfrMain del metodo precedente 
        /// </summary>
        /// <param name="matricolaUtente"></param>
        /// <param name="idUtente"></param>
        /// <param name="idRecordScelta"></param>
        /// <returns></returns>
        public ActionResult GetModulo()
        {
            SceltaDestinazioneTfrViewModel model = CreaViewModel();

            ViewBag.MatricolaUtente = MatricolaUtente;
            ViewBag.IdUtente = IdUtente;
            ViewBag.IdRecordScelta = IdRecordScelta;

            ViewBag.IsDoppioImponibileAbilitato = IsDirigenteGiornalista_OR_Giornalista() && !IsDirigente();

            return PartialView("_moduloSceltaDestinazioneTfr", model);
        }

        public ActionResult GetModuloFinale()
        {
            SceltaDestinazioneTfrViewModel model = CreaViewModelFinale();

            ViewBag.MatricolaUtente = MatricolaUtente;
            ViewBag.IdUtente = IdUtente;
            ViewBag.IdRecordScelta = IdRecordScelta;

            return PartialView("_moduloSceltaDestinazioneTfrFinale", model);
        }

        public bool VerificaPresenzaDatiUtente()
        {
            myRaiData.Incentivi.SINTESI1 sintesi;

            try
            {
                using (IncentiviEntities incentiviEntities = new IncentiviEntities())
                {
                    sintesi = incentiviEntities.SINTESI1.Where(w => w.COD_MATLIBROMAT == MatricolaUtente).FirstOrDefault();

                    if (sintesi == null || string.IsNullOrWhiteSpace(sintesi.COD_TPCNTR) || string.IsNullOrWhiteSpace(sintesi.COD_QUALIFICA))
                    {
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult PostStep1(SceltaDestinazioneTfrViewModel model)
        {
            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "PostStep1",
                descrizione_operazione = "Inizio_PostStep1-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Devo modificare la parte dei valori serializzati in json presenti nel campo SCELTA della tabella tabella XR_MOD_DIPENDENTI
            SceltaDestinazioneTfrScelteEffettuateModel valoriModuloFromDb = GetValoriModuloFromDb();

            if (valoriModuloFromDb == null)
            {
                // Non ho un record precedente quindi creo un nuovo oggetto
                valoriModuloFromDb = new SceltaDestinazioneTfrScelteEffettuateModel();
            }

            // Modifico la parte dei valori che interessa
            valoriModuloFromDb.IsSceltaPregressaEffettuata = model.IsSceltaPregressaEffettuata;
            valoriModuloFromDb.MotivoSceltaPregressa = model.MotivoSceltaPregressa;

            // Scelta effettuata
            string[] sceltaEffettuataSplit = model.SceltaEffettuata.Split('-'); // Il primo valore indica se in azienda o altro, il secondo valore indice che tipo di altro 
            valoriModuloFromDb.SceltaEffettuata = sceltaEffettuataSplit[0];
            valoriModuloFromDb.SceltaSpecificaEffettuata_Codice = sceltaEffettuataSplit[1] == "ND" ? string.Empty : sceltaEffettuataSplit[1]; // Il valore ND è nel caso sceltaEffettuataSplit[0] è In_Azienda
            valoriModuloFromDb.SceltaSpecificaEffettuata = sceltaEffettuataSplit[1] == "ND" ? string.Empty : sceltaEffettuataSplit[1]; // Il valore ND è nel caso sceltaEffettuataSplit[0] è In_Azienda
            
            //if (valoriModuloFromDb.MotivoSceltaPregressa == "Conferimento_Tacito" && valoriModuloFromDb.SceltaSpecificaEffettuata_Codice == "00061")
            //{
            //    // Nel caso di conferimento tacito, il valore stampato sul pdf è quello della tabella 
            //    var scelteSpecifiche = GetScelteAttualiDisponibili(valoriModuloFromDb.IsSceltaPregressaEffettuata, valoriModuloFromDb.MotivoSceltaPregressa, null);
            //    var sceltaSpecifica = scelteSpecifiche.FirstOrDefault(f => f.Value == "Altro-" + valoriModuloFromDb.SceltaSpecificaEffettuata_Codice);
            //    valoriModuloFromDb.SceltaSpecificaEffettuata = sceltaSpecifica?.Text;
            //}
            //else
            //{
            //    valoriModuloFromDb.SceltaSpecificaEffettuata = sceltaEffettuataSplit[1] == "ND" ? string.Empty : sceltaEffettuataSplit[1]; // Il valore ND è nel caso sceltaEffettuataSplit[0] è In_Azienda
            //}

            // Se ho scelto una forma previdenziale specifica oppure di lasciare il TFR in azienda pulisco la scelta scritta a mano dall'utente che sarà nello step successivo.
            // Mi serve nel caso in cui un utente cambia la propria scelta passandola da un fondo scelto da lui ad uno messo a disposizione da Rai o se passa a lasciarlo in azienda
            if (valoriModuloFromDb.SceltaSpecificaEffettuata != "Altro")
            {
                valoriModuloFromDb.SceltaSpecificaDefinitaDaUtente = string.Empty;
            }

            // Imposto lo step corrente con DETTAGLI ad indicare che lo step PREGRESSO è completato
            valoriModuloFromDb.StepCorrenteAmministrazione = StepsEnum.DETTAGLI;

            // Salvo i dati sul db
            SalvaStepSulDb(valoriModuloFromDb);

            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "PostStep1",
                descrizione_operazione = "Fine_PostStep1-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Ricreo il ViewModel
            model = CreaViewModel();

            // Dati che devono essere sempre presenti
            ViewBag.MatricolaUtente = MatricolaUtente;
            ViewBag.IdUtente = IdUtente;
            ViewBag.IdRecordScelta = IdRecordScelta;

            ViewBag.IsDoppioImponibileAbilitato = IsDirigenteGiornalista_OR_Giornalista() && !IsDirigente();

            return PartialView("_moduloSceltaDestinazioneTfr", model);
        }

        [HttpPost]
        public ActionResult PostStep2(SceltaDestinazioneTfrViewModel model)
        {
            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "PostStep2",
                descrizione_operazione = "Inizio_PostStep2-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Devo modificare la parte dei valori serializzati in json presenti nel campo SCELTA della tabella tabella XR_MOD_DIPENDENTI
            SceltaDestinazioneTfrScelteEffettuateModel valoriModuloFromDb = GetValoriModuloFromDb();

            // Modifico la parte dei valori che interessa
            // Scelta specifica effettuata in un altro fondo specifico
            valoriModuloFromDb.SceltaSpecificaDefinitaDaUtente_Codice = model.SceltaSpecificaDefinitaDaUtente_Codice;

            var fondiSpecifici = GetFondiSpecificiPerAltro();
            var fondoSpecifico = fondiSpecifici.FirstOrDefault(f => f.Value == model.SceltaSpecificaDefinitaDaUtente_Codice);
            valoriModuloFromDb.SceltaSpecificaDefinitaDaUtente = fondoSpecifico?.Text;

            //valoriModuloFromDb.SceltaSpecificaDefinitaDaUtente = model.SceltaSpecificaDefinitaDaUtente;

            //valoriModuloFromDb.QuotaSelezionata = model.QuotaSelezionata;
            valoriModuloFromDb.DoppioImponibile = model.DoppioImponibile;

            valoriModuloFromDb.QuotaSelezionata = valoriModuloFromDb.DoppioImponibile ? 100 : model.QuotaSelezionata;

            valoriModuloFromDb.DataAdesioneAlFondo = model.DataAdesioneAlFondo;

            //// CL - 20230104 - Aggiunta la data di prima iscrizione.
            valoriModuloFromDb.DataPrimaIscrizione = model.DataPrimaIscrizione;

            //// CL - La data di compilazione viene impostata automaticamente e non più compilata dall'utente (nella funzione SalvaStepSulDb).
            ////valoriModuloFromDb.DataCompilazione = model.DataCompilazione;

            // Imposto lo step corrente con DOCUMENTI ad indicare che lo step DETTAGLI è completato
            valoriModuloFromDb.StepCorrenteAmministrazione = StepsEnum.DOCUMENTO;

            // Salvo i dati sul db
            SalvaStepSulDb(valoriModuloFromDb);

            // Compilo e salvo il pdf
            CompilaPdf();

            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "PostStep2",
                descrizione_operazione = "Fine_PostStep2-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Ricreo il ViewModel
            model = CreaViewModel();

            // Dati che devono essere sempre presenti
            ViewBag.MatricolaUtente = MatricolaUtente;
            ViewBag.IdUtente = IdUtente;
            ViewBag.IdRecordScelta = IdRecordScelta;

            return PartialView("_moduloSceltaDestinazioneTfr", model);
        }

        [HttpPost]
        public ActionResult BackStep1(SceltaDestinazioneTfrViewModel model)
        {
            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "BackStep1",
                descrizione_operazione = "Inizio_BackStep1-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Devo modificare la parte dei valori serializzati in json presenti nel campo SCELTA della tabella tabella XR_MOD_DIPENDENTI
            SceltaDestinazioneTfrScelteEffettuateModel valoriModuloFromDb = GetValoriModuloFromDb();

            // Torno indietro e imposto lo step corrente con PREGRESSO. Non faccio altra modifica nei valori sul sul db (Da verificare da specifiche)
            valoriModuloFromDb.StepCorrenteAmministrazione = StepsEnum.PREGRESSO;

            // Salvo i dati sul db
            SalvaStepSulDb(valoriModuloFromDb);

            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "BackStep1",
                descrizione_operazione = "Fine_BackStep1-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Ricreo il ViewModel
            model = CreaViewModel();

            // Dati che devono essere sempre presenti
            ViewBag.MatricolaUtente = MatricolaUtente;
            ViewBag.IdUtente = IdUtente;
            ViewBag.IdRecordScelta = IdRecordScelta;

            return PartialView("_moduloSceltaDestinazioneTfr", model);
        }

        [HttpPost]
        public ActionResult PostStep3(SceltaDestinazioneTfrViewModel model)
        {
            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "PostStep3",
                descrizione_operazione = "Inizio_PostStep3-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Devo modificare la parte dei valori serializzati in json presenti nel campo SCELTA della tabella tabella XR_MOD_DIPENDENTI
            SceltaDestinazioneTfrScelteEffettuateModel valoriModuloFromDb = GetValoriModuloFromDb();

            // Torno indietro e imposto lo step corrente con PREGRESSO. Non faccio altra modifica nei valori sul sul db (Da verificare da specifiche)
            valoriModuloFromDb.StepCorrenteAmministrazione = StepsEnum.RIEPILOGO;

            // Salvo i dati sul db
            SalvaStepSulDb(valoriModuloFromDb);

            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "PostStep3",
                descrizione_operazione = "Fine_PostStep3-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Ricreo il ViewModel
            model = CreaViewModel();

            // Dati che devono essere sempre presenti
            ViewBag.MatricolaUtente = MatricolaUtente;
            ViewBag.IdUtente = IdUtente;
            ViewBag.IdRecordScelta = IdRecordScelta;

            return PartialView("_moduloSceltaDestinazioneTfr", model);
        }

        [HttpPost]
        public ActionResult BackStep2(SceltaDestinazioneTfrViewModel model)
        {
            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "BackStep2",
                descrizione_operazione = "Inizio_BackStep2-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Devo modificare la parte dei valori serializzati in json presenti nel campo SCELTA della tabella tabella XR_MOD_DIPENDENTI
            SceltaDestinazioneTfrScelteEffettuateModel valoriModuloFromDb = GetValoriModuloFromDb();

            // Torno indietro e imposto lo step corrente con PREGRESSO. Non faccio altra modifica nei valori sul sul db (Da verificare da specifiche)
            valoriModuloFromDb.StepCorrenteAmministrazione = StepsEnum.DETTAGLI;

            // Salvo i dati sul db
            SalvaStepSulDb(valoriModuloFromDb);

            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "BackStep2",
                descrizione_operazione = "Fine_BackStep2-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Ricreo il ViewModel
            model = CreaViewModel();

            // Dati che devono essere sempre presenti
            ViewBag.MatricolaUtente = MatricolaUtente;
            ViewBag.IdUtente = IdUtente;
            ViewBag.IdRecordScelta = IdRecordScelta;

            ViewBag.IsDoppioImponibileAbilitato = IsDirigenteGiornalista_OR_Giornalista() && !IsDirigente();

            return PartialView("_moduloSceltaDestinazioneTfr", model);
        }

        [HttpPost]
        public ActionResult PostStep4(SceltaDestinazioneTfrViewModel model)
        {
            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "PostStep4",
                descrizione_operazione = "Inizio_PostStep4-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Devo modificare la parte dei valori serializzati in json presenti nel campo SCELTA della tabella tabella XR_MOD_DIPENDENTI
            SceltaDestinazioneTfrScelteEffettuateModel valoriModuloFromDb = GetValoriModuloFromDb();

            // Se il dipendente è iscritto CRAIPI e la percentuale TFR è stata modificata deve essere generato il report Variazione Aliquota TFR.
            if (    (valoriModuloFromDb.SceltaSpecificaEffettuata is "CRAIPI" || valoriModuloFromDb.SceltaSpecificaDefinitaDaUtente_Codice is "01309") 
                &&  valoriModuloFromDb.QuotaSelezionataPrecedente > 0 
                &&  valoriModuloFromDb.QuotaSelezionata != valoriModuloFromDb.QuotaSelezionataPrecedente) {
                    
                this.CreaPdfVariazioneAliquotaTfr(valoriModuloFromDb.QuotaSelezionata, valoriModuloFromDb.AnnoVariazioneAliquota, valoriModuloFromDb.DataPrimaIscrizione, valoriModuloFromDb.NumeroIscrizioneCrai);
            }

            // Imposto lo step corrente con CONSEGNATO ad indicare che lo step RIEPILOGO è completato
            valoriModuloFromDb.StepCorrenteAmministrazione = StepsEnum.SCELTE_EFFETTUATE;
            // Se l'amministrazione ha finito, faccio in modo che l'utente veda direttamente la pagina di validazione
            valoriModuloFromDb.StepCorrente = StepsEnum.SCELTE_EFFETTUATE;
            // Il modulo è stato compilato da Hris
            valoriModuloFromDb.CompilatoDaHrisECodiceOtpDaInviare = true;

            valoriModuloFromDb.QuotaSelezionataPrecedente = valoriModuloFromDb.QuotaSelezionata;

            // Salvo i dati sul db
            SalvaStepSulDb(valoriModuloFromDb);

            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "PostStep4",
                descrizione_operazione = "Inizio_PostStep4-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Ricreo il ViewModel
            model = CreaViewModel();

            // Dati che devono essere sempre presenti
            ViewBag.MatricolaUtente = MatricolaUtente;
            ViewBag.IdUtente = IdUtente;
            ViewBag.IdRecordScelta = IdRecordScelta;

            return PartialView("_moduloSceltaDestinazioneTfr", model);
        }

        [HttpPost]
        public ActionResult BackStep3(SceltaDestinazioneTfrViewModel model)
        {
            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "BackStep3",
                descrizione_operazione = "Inizio_BackStep3-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Devo modificare la parte dei valori serializzati in json presenti nel campo SCELTA della tabella tabella XR_MOD_DIPENDENTI
            SceltaDestinazioneTfrScelteEffettuateModel valoriModuloFromDb = GetValoriModuloFromDb();

            // Torno indietro e imposto lo step corrente con DOCUMENTI. Non faccio altra modifica nei valori sul sul db (Da verificare da specifiche)
            valoriModuloFromDb.StepCorrenteAmministrazione = StepsEnum.DOCUMENTO;

            // Salvo i dati sul db
            SalvaStepSulDb(valoriModuloFromDb);

            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                operazione = "BackStep3",
                descrizione_operazione = "Fine_BackStep3-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                matricola = CommonHelper.GetCurrentUserMatricola(),
            }, CommonHelper.GetCurrentUserMatricola());

            // Ricreo il ViewModel
            model = CreaViewModel();

            // Dati che devono essere sempre presenti
            ViewBag.MatricolaUtente = MatricolaUtente;
            ViewBag.IdUtente = IdUtente;
            ViewBag.IdRecordScelta = IdRecordScelta;

            return PartialView("_moduloSceltaDestinazioneTfr", model);
        }

        [HttpGet]
        public ActionResult GetUtenteConSituazioneModuloFinale()
        {
            // Leggo i dati dell'utente da visualizzare con il ProfileWidget
            myRaiData.Incentivi.SINTESI1 sintesi;
            using (IncentiviEntities incentiviEntities = new IncentiviEntities())
            {
                sintesi = incentiviEntities.SINTESI1.Where(w => w.COD_MATLIBROMAT == MatricolaUtente).FirstOrDefault();
                if (sintesi == null)
                {
                    throw new Exception("Utente non trovato in anagrafica");
                }
            }

            // Leggo i dati della richiesta
            XR_MOD_DIPENDENTI datiRichiesta = GetAllRecordFromDb();

            ViewBag.SintesiNominativo = sintesi.Nominativo();
            ViewBag.DataInizioRichiesta = datiRichiesta.DATA_INIZIO;
            //ViewBag.IsDocumentoPresente = datiRichiesta.PDF_MODULO != null;
            ViewBag.DataValidazione = datiRichiesta.DATA_COMPILAZIONE;

            

            return PartialView("_utenteConSituazioneModuloFinale");
        }

        [HttpPost]
        public bool EliminaDatiInseriti()
        {
            SceltaDestinazioneTfrScelteEffettuateModelBase model = new SceltaDestinazioneTfrScelteEffettuateModelBase
            {
                MatricolaUtente = MatricolaUtente,
                IdUtente = IdUtente,
                IdRecordScelta = IdRecordScelta
            };

            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    operazione = "Inizio_EliminaDatiInseriti",
                    descrizione_operazione = "Inizio_BackStep3-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                }, CommonHelper.GetCurrentUserMatricola());

                // Elimino i files
                EliminaAllegati();

                // Elimino il record del modulo
                EliminaRecordUtente();

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    operazione = "Fine_EliminaDatiInseriti",
                    descrizione_operazione = "Inizio_BackStep3-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(model),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                }, CommonHelper.GetCurrentUserMatricola());

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public int GetStepAmministrazione()
        {
            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    operazione = "Inizio_GetStepAmministrazione",
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                }, CommonHelper.GetCurrentUserMatricola());

                SceltaDestinazioneTfrScelteEffettuateModel valoriModuloFromDb = GetValoriModuloFromDb();

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    operazione = "Fine_GetStepAmministrazione",
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                }, CommonHelper.GetCurrentUserMatricola());

                return (int)valoriModuloFromDb.StepCorrenteAmministrazione;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult CaricaScelteDisponibili(bool sceltaPregressaEffettuata, string motivo)
        {
            SceltaDestinazioneTfrDromDownChildModel model = new SceltaDestinazioneTfrDromDownChildModel
            {
                Id = "MotivoSceltaPregressa",
                Items = GetMotiviSceltePregresseDisponibili(sceltaPregressaEffettuata, motivo)
            };

            return View("_dropDownChild", model);
        }

        public ActionResult CaricaScelteAttualiDisponibili(bool sceltaPregressaEffettuata, string motivoSceltaPregressa)
        {
            SceltaDestinazioneTfrDromDownChildModel model = new SceltaDestinazioneTfrDromDownChildModel
            {
                Id = "SceltaEffettuata",
                Items = GetScelteAttualiDisponibili(sceltaPregressaEffettuata, motivoSceltaPregressa, null)
            };

            return View("_dropDownChild", model);
        }

        private SceltaDestinazioneTfrViewModel CreaViewModel()
        {
            // Leggo i valori dal db
            SceltaDestinazioneTfrScelteEffettuateModel valoriModuloFromDb = GetValoriModuloFromDb();

            // Creo il viewmodel
            SceltaDestinazioneTfrViewModel model = new SceltaDestinazioneTfrViewModel
            {
                StepCorrenteAmministrazione = valoriModuloFromDb != null ? valoriModuloFromDb.StepCorrenteAmministrazione : StepsEnum.PREGRESSO,
                IsSceltaPregressaEffettuata = valoriModuloFromDb?.IsSceltaPregressaEffettuata ?? false,
                MotivoSceltaPregressa = valoriModuloFromDb != null && !string.IsNullOrWhiteSpace(valoriModuloFromDb.MotivoSceltaPregressa) ? valoriModuloFromDb.MotivoSceltaPregressa : "Altri_Motivi",
                //SceltaEffettuata = valoriModuloFromDb != null && !string.IsNullOrWhiteSpace(valoriModuloFromDb.SceltaEffettuata) ? valoriModuloFromDb.SceltaEffettuata : "In_Azienda",
                SceltaSpecificaDefinitaDaUtente_Codice = valoriModuloFromDb != null && !string.IsNullOrWhiteSpace(valoriModuloFromDb.SceltaSpecificaDefinitaDaUtente_Codice) ? valoriModuloFromDb.SceltaSpecificaDefinitaDaUtente_Codice : string.Empty,
                SceltaSpecificaDefinitaDaUtente = valoriModuloFromDb != null && !string.IsNullOrWhiteSpace(valoriModuloFromDb.SceltaSpecificaDefinitaDaUtente) ? valoriModuloFromDb.SceltaSpecificaDefinitaDaUtente : string.Empty,
                QuotaSelezionata = valoriModuloFromDb?.QuotaSelezionata ?? 0,
                DataAdesioneAlFondo = valoriModuloFromDb?.DataAdesioneAlFondo,
                
                //// CL - 20230104 - Aggiunta la memorizzazione della data di prima iscrizione.
                DataPrimaIscrizione = valoriModuloFromDb?.DataPrimaIscrizione,
                
                //// CL - La data di compilazione viene impostata automaticamente nel metodo di salvataggio dei dati.
                ////DataCompilazione = valoriModuloFromDb?.DataCompilazione,
                DoppioImponibile = valoriModuloFromDb?.DoppioImponibile ?? false,
                ImportatoDaCics = valoriModuloFromDb?.ImportatoDaCics ?? false
            };

            string valoreSceltaEffettuata1 = string.Empty;
            string valoreSceltaEffettuata2 = string.Empty;
            if (valoriModuloFromDb != null)
            {
                valoreSceltaEffettuata1 = !string.IsNullOrWhiteSpace(valoriModuloFromDb.SceltaEffettuata) ? valoriModuloFromDb.SceltaEffettuata : "In_Azienda";
                valoreSceltaEffettuata2 = !string.IsNullOrWhiteSpace(valoriModuloFromDb.SceltaSpecificaEffettuata_Codice) ? valoriModuloFromDb.SceltaSpecificaEffettuata_Codice : "ND";
            }
            else
            {
                valoreSceltaEffettuata1 = "In_Azienda";
                valoreSceltaEffettuata2 = "ND";
            }
            model.SceltaEffettuata = $"{valoreSceltaEffettuata1}-{valoreSceltaEffettuata2}";


            model.MatricolaUtente = MatricolaUtente;
            model.IdUtente = IdUtente;
            model.IdRecordScelta = IdRecordScelta;

            CreaStepsViewModel(model);

            // A seconda dello step corrente creo una sezione precisa del ViewModel
            switch (model.StepCorrenteAmministrazione)
            {
                case StepsEnum.PREGRESSO:
                    model.QuotaSelezionataPrecedente = valoriModuloFromDb?.QuotaSelezionataPrecedente ?? 0;
                    CreaStep1ViewModel(model);
                    break;
                case StepsEnum.DETTAGLI:
                    CreaStep2ViewModel(model, true);
                    break;
                case StepsEnum.DOCUMENTO:
                    CreaStep3ViewModel(model);
                    break;
                case StepsEnum.RIEPILOGO:
                    CreaStep4ViewModel(model);
                    break;
            }

            return model;
        }

        private SceltaDestinazioneTfrViewModel CreaViewModelFinale()
        {
            // Leggo i valori dal db
            SceltaDestinazioneTfrScelteEffettuateModel valoriModuloFromDb = GetValoriModuloFromDb();

            // Creo il viewmodel
            SceltaDestinazioneTfrViewModel model = new SceltaDestinazioneTfrViewModel
            {
                StepCorrenteAmministrazione = valoriModuloFromDb != null ? valoriModuloFromDb.StepCorrenteAmministrazione : StepsEnum.PREGRESSO,
                IsSceltaPregressaEffettuata = valoriModuloFromDb?.IsSceltaPregressaEffettuata ?? false,
                MotivoSceltaPregressa = valoriModuloFromDb != null && !string.IsNullOrWhiteSpace(valoriModuloFromDb.MotivoSceltaPregressa) ? valoriModuloFromDb.MotivoSceltaPregressa : "Altri_Motivi",
                SceltaEffettuata = valoriModuloFromDb != null && !string.IsNullOrWhiteSpace(valoriModuloFromDb.SceltaEffettuata) ? valoriModuloFromDb.SceltaEffettuata : "In_Azienda",
                QuotaSelezionata = valoriModuloFromDb?.QuotaSelezionata ?? 0,
                DataAdesioneAlFondo = valoriModuloFromDb?.DataAdesioneAlFondo,

                //// CL - 20230104 - Aggiunta la memorizzazione della data di prima iscrizione.
                DataPrimaIscrizione = valoriModuloFromDb?.DataPrimaIscrizione,
            };

            model.MatricolaUtente = MatricolaUtente;
            model.IdUtente = IdUtente;
            model.IdRecordScelta = IdRecordScelta;

            CreaStepsViewModelFinale(model);

            // A seconda dello step corrente creo una sezione precisa del ViewModel
            switch (model.StepCorrenteAmministrazione)
            {
                case StepsEnum.SCELTE_EFFETTUATE: // Mi interessa solo l'ultimo step
                    CreaStep4ViewModel(model);
                    CreaStep5ViewModel(model);
                    break;
            }

            return model;
        }

        /// <summary>
        /// Metodo che crea la parte del ViewModel relativa al wizard
        /// </summary>
        /// <param name="model"></param>
        private void CreaStepsViewModel(SceltaDestinazioneTfrViewModel model)
        {
            // Creo l'elenco degli step completati
            List<StepsEnum> stepsCompletati = new List<StepsEnum>();

            switch (model.StepCorrenteAmministrazione)
            {
                case StepsEnum.DETTAGLI:
                    stepsCompletati.Add(StepsEnum.PREGRESSO);
                    break;
                case StepsEnum.DOCUMENTO:
                    stepsCompletati.Add(StepsEnum.PREGRESSO);
                    stepsCompletati.Add(StepsEnum.DETTAGLI);
                    break;
                case StepsEnum.RIEPILOGO:
                    stepsCompletati.Add(StepsEnum.PREGRESSO);
                    stepsCompletati.Add(StepsEnum.DETTAGLI);
                    stepsCompletati.Add(StepsEnum.DOCUMENTO);
                    break;
                case StepsEnum.SCELTE_EFFETTUATE:
                    stepsCompletati.Add(StepsEnum.PREGRESSO);
                    stepsCompletati.Add(StepsEnum.DETTAGLI);
                    stepsCompletati.Add(StepsEnum.DOCUMENTO);
                    stepsCompletati.Add(StepsEnum.RIEPILOGO);
                    break;
            }

            // Creo il wizard
            model.Steps = new List<StepItem>
            {
                new StepItem
                {
                    Step = StepsEnum.PREGRESSO,
                    IsCorrente = model.StepCorrenteAmministrazione == StepsEnum.PREGRESSO,
                    IsCompletato = stepsCompletati.Contains(StepsEnum.PREGRESSO)
                },
                new StepItem
                {
                    Step = StepsEnum.DETTAGLI,
                    IsCorrente = model.StepCorrenteAmministrazione == StepsEnum.DETTAGLI,
                    IsCompletato = stepsCompletati.Contains(StepsEnum.DETTAGLI)
                },
                new StepItem
                {
                    Step = StepsEnum.DOCUMENTO,
                    IsCorrente = model.StepCorrenteAmministrazione == StepsEnum.DOCUMENTO,
                    IsCompletato = stepsCompletati.Contains(StepsEnum.DOCUMENTO)
                },
                new StepItem
                {
                    Step = StepsEnum.RIEPILOGO,
                    IsCorrente = model.StepCorrenteAmministrazione == StepsEnum.RIEPILOGO,
                    IsCompletato = stepsCompletati.Contains(StepsEnum.RIEPILOGO)
                },
            };
        }

        /// <summary>
        /// Metodo che crea la parte del ViewModel relativa al wizard
        /// </summary>
        /// <param name="model"></param>
        private void CreaStepsViewModelFinale(SceltaDestinazioneTfrViewModel model)
        {
            // Creo l'elenco degli step completati
            List<StepsEnumHrisValidazione> stepsCompletati = new List<StepsEnumHrisValidazione>();

            // Leggo i dati della richiesta
            XR_MOD_DIPENDENTI datiRichiesta = GetAllRecordFromDb();

            stepsCompletati.Add(StepsEnumHrisValidazione.RICHIESTA_INSERITA);

            if (datiRichiesta.PDF_MODULO != null)
            {
                stepsCompletati.Add(StepsEnumHrisValidazione.RICHIESTA_IN_VALIDAZIONE); // Se ho generato il pdf allora ho completato il wizard
            }

            if (datiRichiesta.DATA_COMPILAZIONE != null) // Se l'utente ha validato, la richiesta è completa
            {
                stepsCompletati.Add(StepsEnumHrisValidazione.RICHIESTA_VALIDATA);
            }

            // Creo il wizard
            model.StepsHrisValidazione = new List<StepItemHrisValidazione>
            {
                new StepItemHrisValidazione
                {
                    Step = StepsEnumHrisValidazione.RICHIESTA_INSERITA,
                    IsCorrente = false,
                    IsCompletato = stepsCompletati.Contains(StepsEnumHrisValidazione.RICHIESTA_INSERITA),
                    DataEvento = datiRichiesta.DATA_INIZIO
                },
                new StepItemHrisValidazione
                {
                    Step = StepsEnumHrisValidazione.RICHIESTA_IN_VALIDAZIONE,
                    IsCorrente = datiRichiesta.PDF_MODULO != null && datiRichiesta.DATA_COMPILAZIONE == null,
                    IsCompletato = stepsCompletati.Contains(StepsEnumHrisValidazione.RICHIESTA_IN_VALIDAZIONE)
                },
                new StepItemHrisValidazione
                {
                    Step = StepsEnumHrisValidazione.RICHIESTA_VALIDATA,
                    IsCorrente = datiRichiesta.PDF_MODULO != null && datiRichiesta.DATA_COMPILAZIONE != null,
                    IsCompletato = stepsCompletati.Contains(StepsEnumHrisValidazione.RICHIESTA_VALIDATA),
                    DataEvento = datiRichiesta.DATA_COMPILAZIONE
                },
            };
        }

        /// <summary>
        /// Metodo che completa la parte del ViewModel relativa allo step 1 (PREGRESSO)
        /// </summary>
        /// <param name="model"></param>
        private void CreaStep1ViewModel(SceltaDestinazioneTfrViewModel model)
        {
            // Valori possibili dei Radio
            model.SceltePregresseDisponibili = GetSceltePregresseDisponibili();

            model.MotiviSceltePregresseDisponibili = GetMotiviSceltePregresseDisponibili(model.IsSceltaPregressaEffettuata, model.MotivoSceltaPregressa);

            model.ScelteDisponibili = GetScelteAttualiDisponibili(model.IsSceltaPregressaEffettuata, model.MotivoSceltaPregressa, model.SceltaEffettuata);
        }

        /// <summary>
        /// Metodo che completa la parte del ViewModel relativa allo step 2 (DETTAGLI)
        /// </summary>
        /// <param name="model"></param>
        private void CreaStep2ViewModel(SceltaDestinazioneTfrViewModel model, bool isFileCancellabile)
        {
            // Valori possibili Select
            model.QuoteDisponibili = GetQuoteDisponibili();

            // Valori fondi specifici di destinazione
            model.FondiSpecificiDisponibili = GetFondiSpecificiPerAltro();

            // Reperisco le info dei files caricati dalla tabella del db
            var allegati = GetAllegatiNoContentByte();

            SceltaDestinazioneTfrAllegatoJsonModel jsonDatiAllegatoTmp;

            if (allegati != null)
            {
                foreach (var allegato in allegati)
                {
                    jsonDatiAllegatoTmp = Newtonsoft.Json.JsonConvert.DeserializeObject<SceltaDestinazioneTfrAllegatoJsonModel>(allegato.Json);

                    if (jsonDatiAllegatoTmp.Tipo == TipoAllegato.ModuloDiAdesione.ToString())
                    {
                        model.ModuloDiAdesione = new SceltaDestinazioneTfrAllegatoViewModel()
                        {
                            Id = allegato.Id,
                            Nome = allegato.NomeFile,
                            Lunghezza = allegato.Length,
                            IsCancellabile = isFileCancellabile,
                            Tipo = TipoAllegato.ModuloDiAdesione
                        };
                    }
                    else
                    {
                        if (model.AltriDocumenti == null)
                        {
                            model.AltriDocumenti = new List<SceltaDestinazioneTfrAllegatoViewModel>();
                        }
                        model.AltriDocumenti.Add(new SceltaDestinazioneTfrAllegatoViewModel()
                        {
                            Id = allegato.Id,
                            Nome = allegato.NomeFile,
                            Lunghezza = allegato.Length,
                            IsCancellabile = isFileCancellabile,
                            Tipo = TipoAllegato.AltriDocumenti
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Metodo che completa la parte del ViewModel relativa allo step 3 (DOCUMENTI - Visualizzazione Pdf)
        /// </summary>
        /// <param name="model"></param>
        private void CreaStep3ViewModel(SceltaDestinazioneTfrViewModel model)
        {
            // Non deve fare nulla. Lo step 3 visualizza solo il pdf compilato
        }

        /// <summary>
        /// Metodo che completa la parte del ViewModel relativa allo step 3 (RIEPILOGO)
        /// </summary>
        /// <param name="model"></param>
        private void CreaStep4ViewModel(SceltaDestinazioneTfrViewModel model)
        {
            // Ottengo le liste dalle quali recuperare le descrizioni delle scelte effettuate
            var sceltePregresseDisponibili = GetSceltePregresseDisponibili();
            var motiviSceltePregresseDisponibili = GetMotiviSceltePregresseDisponibili(model.IsSceltaPregressaEffettuata, model.MotivoSceltaPregressa);
            var scelteAttualiDisponibili = GetScelteAttualiDisponibili(model.IsSceltaPregressaEffettuata, model.MotivoSceltaPregressa, model.SceltaEffettuata);
            var quoteDisponibili = GetQuoteDisponibili();

            // Valorizzo i campi del ViewModel
            model.DescrizioneSceltaPregressaEffettuata = sceltePregresseDisponibili.FirstOrDefault(f => Convert.ToBoolean(f.Value) == model.IsSceltaPregressaEffettuata)?.Text;
            model.DescrizioneMotivoSceltaPregressaEffettuata = motiviSceltePregresseDisponibili.FirstOrDefault(f => f.Value == model.MotivoSceltaPregressa)?.Text;
            model.DescrizioneSceltaAttualeEffettuata = scelteAttualiDisponibili.FirstOrDefault(f => f.Value == model.SceltaEffettuata)?.Text;
            if (model.SceltaEffettuata != "In_Azienda" && model.QuotaSelezionata != 0)
            {
                model.DescrizioneSceltaAttualeEffettuata += $" {quoteDisponibili.FirstOrDefault(f => Convert.ToInt32(f.Value) == model.QuotaSelezionata)?.Text}";
            }

            // Reperisco le info dei files caricati dalla tabella del db
            var allegati = GetAllegatiNoContentByte();

            SceltaDestinazioneTfrAllegatoJsonModel jsonDatiAllegatoTmp;

            if (allegati != null)
            {
                foreach (var allegato in allegati)
                {
                    jsonDatiAllegatoTmp = Newtonsoft.Json.JsonConvert.DeserializeObject<SceltaDestinazioneTfrAllegatoJsonModel>(allegato.Json);

                    if (jsonDatiAllegatoTmp.Tipo == TipoAllegato.ModuloDiAdesione.ToString())
                    {
                        model.ModuloDiAdesione = new SceltaDestinazioneTfrAllegatoViewModel()
                        {
                            Id = allegato.Id,
                            Nome = allegato.NomeFile,
                            Lunghezza = allegato.Length,
                            IsCancellabile = false,
                            Tipo = TipoAllegato.ModuloDiAdesione
                        };
                    }
                    else
                    {
                        if (model.AltriDocumenti == null)
                        {
                            model.AltriDocumenti = new List<SceltaDestinazioneTfrAllegatoViewModel>();
                        }
                        model.AltriDocumenti.Add(new SceltaDestinazioneTfrAllegatoViewModel()
                        {
                            Id = allegato.Id,
                            Nome = allegato.NomeFile,
                            Lunghezza = allegato.Length,
                            IsCancellabile = false,
                            Tipo = TipoAllegato.AltriDocumenti
                        });
                    }
                }
            }
        }

        private void CreaStep5ViewModel(SceltaDestinazioneTfrViewModel model)
        {
            var recordDbTalentiaEntity = GetAllRecordFromDb();

            if (recordDbTalentiaEntity.PDF_MODULO != null)
            {
                model.PdfCompilato = new SceltaDestinazioneTfrAllegatoViewModel
                {
                    Id = recordDbTalentiaEntity.XR_MOD_DIPENDENTI1,
                    Nome = "Documento compilato",
                    Lunghezza = recordDbTalentiaEntity.PDF_MODULO.Length,
                    IsCancellabile = false,
                    Tipo = TipoAllegato.PdfCompilato
                };
            }
        }

        #region Metodi Creazione Valori Disponibili
        private List<SelectListItem> GetSceltePregresseDisponibili()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "false",
                    Text = "Non ho mai effettuato una scelta"
                },
                new SelectListItem
                {
                    Value = "true",
                    Text = "Ho già effettuato una scelta"
                }
            };
        }

        private List<SelectListItem> GetMotiviSceltePregresseDisponibili(bool isSceltaPregressaEffettuata, string motivoSceltaPregressaEffettuata)
        {
            if (isSceltaPregressaEffettuata)
            {
                return new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = "Forma_Complementare_Precedente",
                        Text = "Forma complementare precedente. Destinavo l'integrale TFR ad una forma di previdenza",
                        Selected = motivoSceltaPregressaEffettuata == "Forma_Complementare_Precedente" || string.IsNullOrWhiteSpace(motivoSceltaPregressaEffettuata)
                    },
                    new SelectListItem
                    {
                        Value = "Conferimento_Tacito",
                        Text = "Conferimento tacito. Ero stato iscritto per CONFERIMENTO TACITO ad una forma di previdenza complementare",
                        Selected = motivoSceltaPregressaEffettuata == "Conferimento_Tacito"
                    },
                };
            }
            else
            {
                return new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = "Conferimento_Tacito",
                        Text = "Conferimento tacito. Ero stato iscritto per CONFERIMENTO TACITO ad una forma di previdenza complementare",
                        Selected = motivoSceltaPregressaEffettuata == "Conferimento_Tacito"
                    },
                    new SelectListItem
                    {
                        Value = "Altri_Motivi",
                        Text = "Altri motivi. Esempio: mai occupato in precedenza, occupato sempre con contratti non superiori a 6 mesi, lavoratore autonomo, ecc.",
                        Selected = motivoSceltaPregressaEffettuata == "Altri_Motivi" || string.IsNullOrWhiteSpace(motivoSceltaPregressaEffettuata)
                    }
                };
            }
        }

        private List<SelectListItem> GetScelteAttualiDisponibili(
            bool isSceltaPregressaEffettuata,
            string motivoSceltaPregressa,
            string sceltaEffettuata_SceltaSpecificaEffettuata)
        { 
            List<SelectListItem> elenco = new List<SelectListItem>();

            if (IsDirigenteGiornalista_OR_Giornalista())
            {
                if (IsDirigente())
                {
                    elenco.Add(new SelectListItem
                    {
                        Value = "Altro-FIPDRAI",
                        Text = "Essere iscritto alla forma di previdenza denominata FIPDRAI",
                        Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "Altro_FIPDRAI"
                    });
                }
                else
                {
                    elenco.Add(new SelectListItem
                    {
                        Value = "Altro-FPCGI",
                        Text = "Essere iscritto alla forma di previdenza denominata FPCGI",
                        Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "Altro-FPCGI"
                    });

                    elenco.Add(new SelectListItem
                    {
                        Value = "Altro-Altro",
                        Text = "Essere iscritto ad altra forma di previdenza",
                        Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "Altro-Altro" || (string.IsNullOrWhiteSpace(sceltaEffettuata_SceltaSpecificaEffettuata) && isSceltaPregressaEffettuata)
                    });
                }
            }
            else
            {
                if (IsTI())
                {
                    elenco.Add(new SelectListItem
                    {
                        Value = "Altro-CRAIPI",
                        Text = "Essere iscritto alla forma di previdenza denominata CRAIPI",
                        Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "Altro-CRAIPI"
                    });
                }

                elenco.Add(new SelectListItem
                {
                    Value = "Altro-Altro",
                    Text = "Essere iscritto ad altra forma di previdenza",
                    Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "Altro-Altro" || (string.IsNullOrWhiteSpace(sceltaEffettuata_SceltaSpecificaEffettuata) && isSceltaPregressaEffettuata)
                });
            }

            // Se in passato ho già effettuato una scelta, adesso non posso lasciarlo in azienda
            if (!isSceltaPregressaEffettuata)
            {
                elenco.Add(new SelectListItem
                {
                    Value = "In_Azienda-ND", // In fase di salvataffio ND sarà sostituito con la stringa vuota
                    Text = "Mantenere il TFR in azienda",
                    Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "In_Azienda-ND" || (string.IsNullOrWhiteSpace(sceltaEffettuata_SceltaSpecificaEffettuata) && !isSceltaPregressaEffettuata)
                });
            }

            //if (motivoSceltaPregressa == "Conferimento_Tacito")
            //{
            //    // Se nel motivo della scelta pregressa c'è Conferimento Tacito, i valori possono essere il Fondo Cometa oppure Altro
            //    using (HRPADBEntities dbHRPA = new HRPADBEntities())
            //    {
            //        //var records = dbHRPA.TTFR_ENTEPREVCOMPL
            //        //    .Where(w => w.ToUpper().Contains("COMETA") || w.DESCBREVE_ENTEPREVCOMPL.ToUpper().Contains("COMETA"));

            //        var records = dbHRPA.TTFR_ENTEPREVCOMPL
            //            .Where(w => w.CODICE_ENTEPREVCOMPL == "00061");

            //        foreach (var record in records)
            //        {
            //            elenco.Add(new SelectListItem
            //            {
            //                Value = "Altro-" + record.CODICE_ENTEPREVCOMPL,
            //                Text = !string.IsNullOrWhiteSpace(record.DESCBREVE_ENTEPREVCOMPL) ? record.DESCBREVE_ENTEPREVCOMPL : record.DESCR_ENTEPREVCOMPL,
            //                Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "Altro-Altro" || (string.IsNullOrWhiteSpace(sceltaEffettuata_SceltaSpecificaEffettuata) && isSceltaPregressaEffettuata)
            //            });
            //        }

            //        elenco.Add(new SelectListItem
            //        {
            //            Value = "Altro-Altro",
            //            Text = "Essere iscritto ad altra forma di previdenza",
            //            Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "Altro-Altro" || (string.IsNullOrWhiteSpace(sceltaEffettuata_SceltaSpecificaEffettuata) && isSceltaPregressaEffettuata)
            //        });
            //    }
            //}
            //else
            //{
            //    if (IsDirigenteGiornalista_OR_Giornalista())
            //    {
            //        if (IsDirigente())
            //        {
            //            elenco.Add(new SelectListItem
            //            {
            //                Value = "Altro-FIPDRAI",
            //                Text = "Essere iscritto alla forma di previdenza denominata FIPDRAI",
            //                Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "Altro_FIPDRAI"
            //            });
            //        }
            //        else
            //        {
            //            elenco.Add(new SelectListItem
            //            {
            //                Value = "Altro-FPCGI",
            //                Text = "Essere iscritto alla forma di previdenza denominata FPCGI",
            //                Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "Altro-FPCGI"
            //            });

            //            elenco.Add(new SelectListItem
            //            {
            //                Value = "Altro-Altro",
            //                Text = "Essere iscritto ad altra forma di previdenza",
            //                Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "Altro-Altro" || (string.IsNullOrWhiteSpace(sceltaEffettuata_SceltaSpecificaEffettuata) && isSceltaPregressaEffettuata)
            //            });
            //        }
            //    }
            //    else
            //    {
            //        if (IsTI())
            //        {
            //            elenco.Add(new SelectListItem
            //            {
            //                Value = "Altro-CRAIPI",
            //                Text = "Essere iscritto alla forma di previdenza denominata CRAIPI",
            //                Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "Altro-CRAIPI"
            //            });
            //        }

            //        elenco.Add(new SelectListItem
            //        {
            //            Value = "Altro-Altro",
            //            Text = "Essere iscritto ad altra forma di previdenza",
            //            Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "Altro-Altro" || (string.IsNullOrWhiteSpace(sceltaEffettuata_SceltaSpecificaEffettuata) && isSceltaPregressaEffettuata)
            //        });
            //    }

            //    // Se in passato ho già effettuato una scelta, adesso non posso lasciarlo in azienda
            //    if (!isSceltaPregressaEffettuata)
            //    {
            //        elenco.Add(new SelectListItem
            //        {
            //            Value = "In_Azienda-ND", // In fase di salvataffio ND sarà sostituito con la stringa vuota
            //            Text = "Mantenere il TFR in azienda",
            //            Selected = sceltaEffettuata_SceltaSpecificaEffettuata == "In_Azienda-ND" || (string.IsNullOrWhiteSpace(sceltaEffettuata_SceltaSpecificaEffettuata) && !isSceltaPregressaEffettuata)
            //        });
            //    }
            //}

            return elenco;
        }

        private List<SelectListItem> GetQuoteDisponibili()
        {
            List<TextValueItem> elenco = new List<TextValueItem>();

            elenco.Add(new TextValueItem
            {
                Valore = 0,
                Descrizione = "0 %"
            });

            if (IsDirigenteGiornalista_OR_Giornalista())
            {
                if (IsDirigente())
                {
                    // Dirigente
                    elenco.Add(new TextValueItem
                    {
                        Valore = 100,
                        Descrizione = "100 %"
                    });
                }
                else
                {
                    // Giornalista
                    for (int i = 50; i <= 100; i = i + 5)
                    {
                        elenco.Add(new TextValueItem
                        {
                            Valore = i,
                            Descrizione = $"{i} %"
                        });
                    }
                }
            }
            else
            {

                // Utente normale
                if (IsTI())
                {
                    // Tempo indeterminato
                    for (int i = 20; i <= 95; i = i + 5)
                    {
                        elenco.Add(new TextValueItem
                        {
                            Valore = i,
                            Descrizione = $"{i} %"
                        });
                    }
                }

                elenco.Add(new TextValueItem
                {
                    Valore = 100,
                    Descrizione = "100 %"
                });
            }

            return elenco.Select(s => new SelectListItem
            {
                Text = s.Descrizione,
                Value = s.Valore.ToString()
            }).ToList();
        }

        private List<SelectListItem> GetFondiSpecificiPerAltro()
        {
            List<TextValueItem> elenco = new List<TextValueItem>();

            elenco.Add(new TextValueItem
            {
                Valore = string.Empty,
                Descrizione = string.Empty
            });

            try
            {
                using (HRPADBEntities dbHRPA = new HRPADBEntities())
                {
                    var records = dbHRPA.TTFR_ENTEPREVCOMPL
                        .Where(w => w.CODICE_ENTEPREVCOMPL.ToUpper().StartsWith("0") || w.CODICE_ENTEPREVCOMPL.ToUpper().StartsWith("A"));

                    foreach (var record in records)
                    {
                        elenco.Add(new TextValueItem
                        {
                            Valore = record.CODICE_ENTEPREVCOMPL,
                            Descrizione = !string.IsNullOrWhiteSpace(record.DESCBREVE_ENTEPREVCOMPL) ? record.DESCBREVE_ENTEPREVCOMPL : record.DESCR_ENTEPREVCOMPL
                        });
                    }
                }

                return elenco.OrderBy(o => o.Descrizione).Select(s => new SelectListItem { Value = s.Valore.ToString(), Text = s.Descrizione }).ToList();
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = Applicazione,
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    error_message =ex.Message,
                    provenienza = Provenienza
                });
                throw ex;
            }

        }
        #endregion

        #region Metodi Operazioni DB
        [HttpPost]
        public ActionResult PostFile(
            HttpPostedFileBase file, 
            string nome, 
            string titolo, 
            string descrizione, 
            bool isObbligatorio, 
            string tipo,
            string matricolaUtente)
        {
            try
            {
                // Leggo il record principale presente nella tabella XR_MOD_DIPENDENTI del db di talentia
                var recordDbTalentiaEntity = GetAllRecordFromDb();

                // Eseguo scrittura sul db che mi restituisce il nuovo id
                string chiave = GetChiaveFile(isObbligatorio);

                // Leggo i dati del database dove è salvato il record principale del documento dell'utente
                string nomeDatabase = string.Empty;
                using (TalentiaEntities dbTalentia = new TalentiaEntities())
                {
                    nomeDatabase = dbTalentia.Database.Connection.Database;
                }

                // Creo l'oggetto che verrà salvato in formato json
                SceltaDestinazioneTfrAllegatoJsonModel jsonModel = new SceltaDestinazioneTfrAllegatoJsonModel
                {
                    MatricolaUtente = MatricolaUtente,
                    Titolo = titolo,
                    Tipo = ((TipoAllegato)Enum.Parse(typeof(TipoAllegato), tipo)).ToString(),
                    IsObbligatorio = isObbligatorio,
                    NomeDatabaseEsterno = nomeDatabase,
                    NomeTabellaEsterna = "XR_MOD_DIPENDENTI",
                    IdRecordEsterno = recordDbTalentiaEntity.XR_MOD_DIPENDENTI1.ToString()
                };

                // Salvo il file
                var esitoUpload = myRaiCommonTasks.Helpers.FileManager.UploadFile(
                    CommonManager.GetCurrentUserMatricola(),
                    TipologiaFileUpload,
                    file,
                    chiave,
                    string.Empty,
                    Newtonsoft.Json.JsonConvert.SerializeObject(jsonModel),
                    false);

                if (!esitoUpload.Esito)
                {
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        applicativo = Applicazione,
                        data = DateTime.Now,
                        matricola = CommonHelper.GetCurrentUserMatricola(),
                        error_message = chiave + ":" + esitoUpload.Errore,
                        provenienza = Provenienza
                    });
                    throw new Exception();
                }

                // Creo il ViewModel restituito alla view che costruisce una riga della tabell deil files caricati
                SceltaDestinazioneTfrAllegatoViewModel model = new SceltaDestinazioneTfrAllegatoViewModel
                {
                    Id = esitoUpload.Files[0].Id, // Recupero l'id del file salvato
                    MatricolaUtente = MatricolaUtente,
                    Nome = nome,
                    Lunghezza = esitoUpload.Files[0].Length,
                    Tipo = (TipoAllegato)Enum.Parse(typeof(TipoAllegato), tipo),
                    IsObbligatorio = isObbligatorio,
                    IsCancellabile = true
                };

                return View("_trFileUpload", model);
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = Applicazione,
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    error_message = ex.Message,
                    provenienza = Provenienza
                }, CommonHelper.GetCurrentUserMatricola());
                throw ex;
            }
        }

        [HttpPost]
        public void EliminaAllegato(int idAllegato)
        {
            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Inizio_EliminaAllegato-Id: " + idAllegato.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                var esitoDelete = myRaiCommonTasks.Helpers.FileManager.DeleteFile(idAllegato);

                if (!esitoDelete.Esito)
                {
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        applicativo = Applicazione,
                        data = DateTime.Now,
                        matricola = CommonHelper.GetCurrentUserMatricola(),
                        error_message = esitoDelete.Errore,
                        provenienza = Provenienza
                    });
                    throw new Exception(esitoDelete.Errore);
                }

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Fine_EliminaAllegato-Id: " + idAllegato.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = Applicazione,
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    error_message = ex.Message,
                    provenienza = Provenienza
                }, CommonHelper.GetCurrentUserMatricola());
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che restituice il record relativo al modulo dell'utente dal db
        /// </summary>
        /// <returns></returns>
        private XR_MOD_DIPENDENTI GetAllRecordFromDb()
        {
            try
            {
                using (TalentiaEntities dbTalentia = new TalentiaEntities())
                {
                    return dbTalentia.XR_MOD_DIPENDENTI
                        .Where(w => w.MATRICOLA.Equals(MatricolaUtente) && w.COD_MODULO.Equals(CodModulo))
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che restituice il record relativo al modulo dell'utente dal db
        /// </summary>
        /// <returns></returns>
        private XR_MOD_DIPENDENTI GetAllRecordFromDbById(int idRecord)
        {
            try
            {
                using (TalentiaEntities dbTalentia = new TalentiaEntities())
                {
                    return dbTalentia.XR_MOD_DIPENDENTI
                        .Where(w => w.XR_MOD_DIPENDENTI1 == idRecord)
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che restituisce l'oggetto deserializzato del campo SCELTA presente sul db
        /// </summary>
        /// <returns></returns>
        private SceltaDestinazioneTfrScelteEffettuateModel GetValoriModuloFromDb()
        {
            try
            {
                var recordXrModDipendenti = GetAllRecordFromDb();

                if (recordXrModDipendenti != null)
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<SceltaDestinazioneTfrScelteEffettuateModel>(recordXrModDipendenti.SCELTA);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che salva i dati di un generico step sulla tabella XR_MOD_DIPENDENTI
        /// </summary>
        /// <param name="model"></param>
        private void SalvaStepSulDb(SceltaDestinazioneTfrScelteEffettuateModel model)
        {
            try
            {
                //// CL - La data di compilazione viene impostata automaticamente e non più compilata dall'utente (nella funzione SalvaStepSulDb).
                model.DataCompilazione = DateTime.Now;

                // Reerisco il record dalla tabella XR_MOD_DIPENDENTI
                var recordXrModDipendenti = GetAllRecordFromDb();

                using (TalentiaEntities dbTalentia = new TalentiaEntities())
                {
                    if (recordXrModDipendenti == null)
                    {
                        // Non ho dati pregressi quindi si tratta di una insert
                        XR_MOD_DIPENDENTI nuovoRecord = new XR_MOD_DIPENDENTI
                        {
                            XR_MOD_DIPENDENTI1 = dbTalentia.XR_MOD_DIPENDENTI.GeneraPrimaryKey(),
                            ID_PERSONA = IdUtente,
                            MATRICOLA = MatricolaUtente,
                            COD_MODULO = CodModulo,
                            SCELTA = Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            COD_USER = P_MatricolaUtenteLoggato,
                            DATA_INIZIO = DateTime.Now,
                            COD_TERMID = Request.UserHostAddress,
                            TMS_TIMESTAMP = DateTime.Now
                        };

                        dbTalentia.XR_MOD_DIPENDENTI.Add(nuovoRecord);
                    }
                    else
                    {
                        // Ho dati pregressi quindi si tratta di un update
                        var recordDbDaModificare = dbTalentia.XR_MOD_DIPENDENTI
                            .Where(w => w.XR_MOD_DIPENDENTI1 == recordXrModDipendenti.XR_MOD_DIPENDENTI1)
                            .FirstOrDefault();

                        recordDbDaModificare.SCELTA = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                        recordDbDaModificare.COD_USER = P_MatricolaUtenteLoggato;
                        recordDbDaModificare.COD_TERMID = Request.UserHostAddress;
                        recordDbDaModificare.TMS_TIMESTAMP = DateTime.Now;
                    }

                    dbTalentia.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che elimina il record delle scelte dell'untente dalla tabella XR_MOD_DIPENDENTI
        /// </summary>
        private void EliminaRecordUtente()
        {
            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                descrizione_operazione = "Inizio_EliminaRecordUtente",
                matricola = CommonHelper.GetCurrentUserMatricola()
            }, CommonHelper.GetCurrentUserMatricola());

            using (TalentiaEntities dbTalentia = new TalentiaEntities())
            {
                var result = dbTalentia.XR_MOD_DIPENDENTI
                    .Where(w => w.MATRICOLA.Equals(MatricolaUtente) && w.COD_MODULO.Equals(CodModulo))
                    .FirstOrDefault();

                if (result != null)
                {
                    dbTalentia.XR_MOD_DIPENDENTI.Remove(result);

                    dbTalentia.SaveChanges();
                }
            }

            Logger.LogAzione(new MyRai_LogAzioni
            {
                applicativo = "Portale",
                provenienza = "SceltaDestinazioneTfrController",
                descrizione_operazione = "Fine_EliminaRecordUtente",
                matricola = CommonHelper.GetCurrentUserMatricola()
            }, CommonHelper.GetCurrentUserMatricola());
        }

        /// <summary>
        /// Metodo che restituisce i dati dei files caricati senza il contentbyte
        /// </summary>
        /// <returns></returns>
        private IEnumerable<MyRai_Files> GetAllegatiNoContentByte()
        {
            try
            {
                using (digiGappEntities dbGapp = new digiGappEntities())
                {
                    var files = dbGapp.MyRai_Files
                        .Where(w => w.MatricolaCreatore.Equals(MatricolaUtente) && w.Tipologia == TipologiaFileUpload)
                        .OrderBy(o => o.DataCreazione)
                        .Select(s => new
                        {
                            s.Id,
                            s.Chiave,
                            s.MatricolaCreatore,
                            s.DataCreazione,
                            s.Tipologia,
                            s.NomeFile,
                            s.MimeType,
                            s.Length,
                            s.Password,
                            s.Json,
                            s.Attivo
                        });

                    if (files.Any())
                    {
                        List<MyRai_Files> result = new List<MyRai_Files>();
                        foreach (var file in files)
                        {
                            result.Add(new MyRai_Files
                            {
                                Id = file.Id,
                                Chiave = file.Chiave,
                                MatricolaCreatore = file.MatricolaCreatore,
                                DataCreazione = file.DataCreazione,
                                Tipologia = file.Tipologia,
                                NomeFile = file.NomeFile,
                                MimeType = file.MimeType,
                                Length = file.Length,
                                Password = file.Password,
                                Json = file.Json,
                                Attivo = file.Attivo,
                            });
                        }

                        return result;
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = Applicazione,
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    error_message = ex.Message,
                    provenienza = Provenienza
                });
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che elimina tutti i files caricati dall'utente
        /// </summary>
        private void EliminaAllegati()
        {
            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Inizio_EliminaAllegati",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                using (digiGappEntities dbGapp = new digiGappEntities())
                {
                    var idFiles = dbGapp.MyRai_Files
                        .Where(w => w.MatricolaCreatore.Equals(MatricolaUtente) && w.Tipologia == TipologiaFileUpload)
                        .Select(s => s.Id)
                        .ToList();

                    if (idFiles.Any())
                    {
                        foreach (var idFile in idFiles)
                        {
                            var esitoDelete = myRaiCommonTasks.Helpers.FileManager.DeleteFile(idFile);

                            if (!esitoDelete.Esito)
                            {
                                Logger.LogErrori(new MyRai_LogErrori()
                                {
                                    applicativo = Applicazione,
                                    data = DateTime.Now,
                                    matricola = CommonHelper.GetCurrentUserMatricola(),
                                    error_message = esitoDelete.Errore,
                                    provenienza = Provenienza
                                });
                                throw new Exception(esitoDelete.Errore);
                            }
                        }
                    }
                }

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Fine_EliminaAllegati",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = Applicazione,
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    error_message = ex.Message,
                    provenienza = Provenienza
                }, CommonHelper.GetCurrentUserMatricola());
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che restituisce l'intero record dell'allegato principale
        /// </summary>
        /// <returns></returns>
        private MyRai_Files GetAllegatoPrincipale()
        {
            try
            {
                int idAllegatoPrincipale = 0;
                SceltaDestinazioneTfrAllegatoViewModel allegatoTmp;

                var allegati = GetAllegatiNoContentByte();

                if (allegati != null)
                {
                    foreach (var allegato in allegati)
                    {
                        allegatoTmp = Newtonsoft.Json.JsonConvert.DeserializeObject<SceltaDestinazioneTfrAllegatoViewModel>(allegato.Json);

                        if (allegatoTmp.Tipo == TipoAllegato.ModuloDiAdesione)
                        {
                            idAllegatoPrincipale = allegatoTmp.Id;
                        }
                    }

                    if (idAllegatoPrincipale != 0)
                    {
                        using (digiGappEntities dbGapp = new digiGappEntities())
                        {
                            return dbGapp.MyRai_Files.FirstOrDefault(f => f.Id == idAllegatoPrincipale);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che restituisce gli interi records degli altri allegati
        /// </summary>
        /// <returns></returns>
        private IEnumerable<MyRai_Files> GetAltriAllegati()
        {
            try
            {
                List<int> idAltriAllegati = new List<int>();
                SceltaDestinazioneTfrAllegatoViewModel allegatoTmp;

                var allegati = GetAllegatiNoContentByte();

                if (allegati != null)
                {
                    foreach (var allegato in allegati)
                    {
                        allegatoTmp = Newtonsoft.Json.JsonConvert.DeserializeObject<SceltaDestinazioneTfrAllegatoViewModel>(allegato.Json);

                        if (allegatoTmp.Tipo == TipoAllegato.AltriDocumenti)
                        {
                            idAltriAllegati.Add(allegatoTmp.Id);
                        }
                    }

                    if (idAltriAllegati.Count > 0)
                    {
                        using (digiGappEntities dbGapp = new digiGappEntities())
                        {
                            return dbGapp.MyRai_Files.Where(w => idAltriAllegati.Contains(w.Id)).ToList();
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che costruisce la stringa da utilizzare nel campo "Chiave" della tabella MyRai_Files
        /// </summary>
        /// <param name="isObbligatorio"></param>
        /// <returns></returns>
        private string GetChiaveFile(bool isObbligatorio)
        {
            char pricipaleSecondario = isObbligatorio ? 'P' : 'S';
            return $"{MatricolaUtente}_{CodModulo}_{pricipaleSecondario}";
        }
        #endregion

        #region Visualizzazione allegato
        public ActionResult GetAllegatoTemporaneo(int idAllegato)
        {
            var allegatoDbEnity = myRaiCommonTasks.Helpers.FileManager.GetFile(idAllegato);

            if (allegatoDbEnity == null)
            {
                throw new Exception("Errore impossibile reperire il file");
            }

            SceltaDestinazioneTfrAllegatoViewModel model = new SceltaDestinazioneTfrAllegatoViewModel
            {
                Id = idAllegato,
                Nome = Path.GetFileNameWithoutExtension(allegatoDbEnity.Files[0].NomeFile)
            };

            return View("~/Views/SceltaDestinazioneTfr/_Allegato_Viewer.cshtml", model);
        }

        public ActionResult GetAllegatoTemporaneoPdfCompilato(int idAllegato)
        {
            var recordXrModDipendenti = GetAllRecordFromDbById(idAllegato);

            if (recordXrModDipendenti == null)
            {
                throw new Exception("Errore impossibile reperire il file");
            }

            SceltaDestinazioneTfrAllegatoViewModel model = new SceltaDestinazioneTfrAllegatoViewModel
            {
                Id = idAllegato,
                Nome = "Scelta destinazione tfr.pdf"
            };

            return View("~/Views/SceltaDestinazioneTfr/_Allegato_Viewer_PdfCompilato.cshtml", model);
        }

        /// <summary>
        /// Metodo che invia al front end il content di un file salvato sul db. Il content viene visualizzato in un iframe
        /// </summary>
        /// <param name="idAllegato"></param>
        /// <returns></returns>
        public ActionResult GetAllegato(int idAllegato)
        {
            try
            {
                var allegatoDbEnity = myRaiCommonTasks.Helpers.FileManager.GetFile(idAllegato);

                if (allegatoDbEnity == null)
                {
                    throw new Exception("Errore impossibile reperire il file");
                }

                byte[] byteArray = null;

                if (allegatoDbEnity.Files[0].ContentByte != null)
                {
                    byteArray = allegatoDbEnity.Files[0].ContentByte;
                }

                string nomefile = allegatoDbEnity.Files[0].NomeFile;

                MemoryStream pdfStream = new MemoryStream();
                pdfStream.Write(byteArray, 0, byteArray.Length);
                pdfStream.Position = 0;

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = "documento",
                    Inline = true,
                };

                Response.AddHeader("Content-Disposition", "inline; filename=" + nomefile);

                return File(byteArray, "application/pdf");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult GetAllegatoPdfCompilato(int idAllegato)
        {
            try
            {
                var recordXrModDipendenti = GetAllRecordFromDbById(idAllegato);

                if (recordXrModDipendenti == null)
                {
                    throw new Exception("Errore impossibile reperire il file");
                }

                byte[] byteArray = null;

                if (recordXrModDipendenti.PDF_MODULO != null)
                {
                    byteArray = recordXrModDipendenti.PDF_MODULO;
                }

                string nomefile = "Scelta destinazione tfr.pdf";

                MemoryStream pdfStream = new MemoryStream();
                pdfStream.Write(byteArray, 0, byteArray.Length);
                pdfStream.Position = 0;

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = "documento",
                    Inline = true,
                };

                Response.AddHeader("Content-Disposition", "inline; filename=" + nomefile);

                return File(byteArray, "application/pdf");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Report Variazione Aliquota TFR

        /// <summary>
        /// Provvede alla compilazione del report Variazione Aliquota TFR.
        /// </summary>
        /// <param name="p_NewPercentage">
        /// Valore <see cref="Int32"/> contenente la nuova aliquota TFR.
        /// </param>
        /// <param name="p_StartYear">
        /// Valore <see cref="String"/> contenente l'anno di inizio validita' della variazione di aliquota.
        /// </param>
        /// <param name="p_SigningUpDate">
        /// Valore <see cref="DateTime?"/> contenente la data di prima iscrizione al CRAIPI.
        /// </param>
        /// <param name="p_SigningUpNumber">
        /// Valore <see cref="String"/> contenente il numero di iscrizione al CRAIPI.
        /// </param>
        private void CreaPdfVariazioneAliquotaTfr(
            Int32 p_NewPercentage
        , String p_StartYear
        , DateTime? p_SigningUpDate
        , String p_SigningUpNumber)
        {

            Byte[] l_PdfContentBytes = null;

            try
            {
                Logger.LogAzione(
                    new MyRai_LogAzioni
                    {
                        provenienza = "SceltaDestinazioneTfrController",
                        descrizione_operazione = "Inizio_CompilaPdfVariazioneAliquotaTfr",
                        matricola = this.MatricolaUtente
                    },

                    this.MatricolaUtente);

                // Lettura del template PDF per il report Variazione Aliquota TFR.
                var l_TemplateVariazioneAliquotaTfr = this.GetTemplateVariazioneAliquotaTfr();

                // Inizializzazione del motore PDF.
                var l_PdfReader = new PdfReader(l_TemplateVariazioneAliquotaTfr);
                var l_MemoryStream = new MemoryStream();
                var l_PdfStamper = new PdfStamper(l_PdfReader, l_MemoryStream, '4');

                // Compilazione degli acro-fields definiti nel template PDF.
                this.CompilaPdfVariazioneAliquotaTfr(l_PdfStamper, p_NewPercentage, p_StartYear, p_SigningUpDate, p_SigningUpNumber);

                // Scrittura del PDF.
                l_PdfStamper.Writer.CloseStream = false;
                l_PdfStamper.FormFlattening = true;
                l_PdfStamper.Close();
                l_MemoryStream.Position = 0;
                l_PdfContentBytes = l_MemoryStream.ToArray();
                l_MemoryStream.Flush();
                l_PdfReader.Close();

                this.SalvaPdfVariazioneQuotaTfr(l_PdfContentBytes); // TODO: Capire come salvare nella tabella degli allegati...
                this.InviaPdfVariazioneAliquotaTfr(l_PdfContentBytes);

                ////var file = new FileStream($"C:\\dev\\rai\\DIGIGAPP2\\Baseline\\myRai\\myRai\\myRai\\Templates\\SceltaDestinazioneTFR\\{Guid.NewGuid()}.pdf", FileMode.Create, FileAccess.Write);
                ////l_MemoryStream.WriteTo(file);
                ////file.Close();
                ////l_MemoryStream.Close();

                Logger.LogAzione(
                    new MyRai_LogAzioni
                    {
                        provenienza = "SceltaDestinazioneTfrController",
                        descrizione_operazione = "Fine_CompilaPdfVariazioneAliquotaTfr",
                        matricola = this.MatricolaUtente
                    },

                    this.MatricolaUtente);
            }
            catch (Exception l_Exception)
            {
                Logger.LogErrori(
                    new MyRai_LogErrori()
                    {
                        applicativo = this.Applicazione,
                        data = DateTime.Now,
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        error_message = l_Exception.Message,
                        provenienza = this.Provenienza
                    });

                throw;
            }
        }

        /// <summary>
        /// Compila gli acro-fields per il report Variazione Aliquota TFR.
        /// </summary>
        /// <param name="p_PdfStamper">
        /// Valore <see cref="PdfStamper"/> che contiene gli acro-fields da compilare.
        /// </param>
        /// <param name="p_NewPercentage">
        /// Valore <see cref="Int32"/> contenente la nuova aliquota TFR.
        /// </param>
        /// <param name="p_StartYear">
        /// Valore <see cref="String"/> contenente l'anno di inizio validita' della variazione di aliquota.
        /// </param>
        /// <param name="p_SigningUpDate">
        /// Valore <see cref="DateTime?"/> contenente la data di prima iscrizione al CRAIPI.
        /// </param>
        /// <param name="p_SigningUpNumber">
        /// Valore <see cref="String"/> contenente il numero di iscrizione al CRAIPI.
        /// </param>
        private void CompilaPdfVariazioneAliquotaTfr(
            PdfStamper p_PdfStamper
        , Int32 p_Percentage
        , String p_StartYear
        , DateTime? p_SigningUpDate
        , String p_SigningUpNumber)
        {

            try
            {
                Logger.LogAzione(
                    new MyRai_LogAzioni
                    {
                        provenienza = "SceltaDestinazioneTfrController",
                        descrizione_operazione = "Inizio_CompilaCampiVariazioneAliquotaTfr",
                        matricola = this.MatricolaUtente
                    },

                    this.MatricolaUtente);

                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.NOME_COGNOME, this.CognomeNomePdf);
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_01, this.CodiceFiscalePdf.Substring(0, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_02, this.CodiceFiscalePdf.Substring(1, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_03, this.CodiceFiscalePdf.Substring(2, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_04, this.CodiceFiscalePdf.Substring(3, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_05, this.CodiceFiscalePdf.Substring(4, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_06, this.CodiceFiscalePdf.Substring(5, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_07, this.CodiceFiscalePdf.Substring(6, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_08, this.CodiceFiscalePdf.Substring(7, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_09, this.CodiceFiscalePdf.Substring(8, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_10, this.CodiceFiscalePdf.Substring(9, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_11, this.CodiceFiscalePdf.Substring(10, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_12, this.CodiceFiscalePdf.Substring(11, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_13, this.CodiceFiscalePdf.Substring(12, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_14, this.CodiceFiscalePdf.Substring(13, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_15, this.CodiceFiscalePdf.Substring(14, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.CODICE_FISCALE_16, this.CodiceFiscalePdf.Substring(15, 1));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.NASCITA_LUOGO, this.NatoAPdf);
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.NASCITA_PROVINCIA, Utente.EsponiAnagrafica()._provinciaNascita.Trim());
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.NASCITA_GIORNO, Convert.ToDateTime(this.NatoIlPdf).Day.ToString());
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.NASCITA_MESE, Convert.ToDateTime(this.NatoIlPdf).Month.ToString());
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.NASCITA_ANNO, Convert.ToDateTime(this.NatoIlPdf).Year.ToString());
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.RESIDENZA_LUOGO, Utente.EsponiAnagrafica()._comuneresidenza.Trim());
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.RESIDENZA_PROVINCIA, Utente.EsponiAnagrafica()._provinciaresidenza.Trim());
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.RESIDENZA_CAP, Utente.EsponiAnagrafica()._capresidenza.Trim());
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.RESIDENZA_INDIRIZZO, Utente.EsponiAnagrafica()._indirizzoresidenza.Trim());
                ////stamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.RESIDENZA_NUMERO_CIVICO, Utente.EsponiAnagrafica()._indirizzoresidenza.Trim()); TODO: Verificare se nell'indirizzo c'e' il numero civico...
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.TELEFONO, Utente.EsponiAnagrafica()._telefono.Trim());
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.AZIENDA_APPARTENENZA, Utente.EsponiAnagrafica().SedeContabileDescrizione.Trim());
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.ISCRIZIONE_CRAIPI_DATA, p_SigningUpDate?.ToString("dd/MM/yyyy"));
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.ISCRIZIONE_CRAIPI_NUMERO, p_SigningUpNumber);
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.PERCENTUALE_TFR_ANNO_VARIAZIONE, p_StartYear);
                p_PdfStamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.PERCENTUALE_TFR, p_Percentage.ToString());
                ////stamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.FIRMA_ADERENTE_LUOGO_DATA, this.LuogoDataFirmaPdf); TODO: Verificare come viene valorizzato nell'altro report.
                ////stamper.AcroFields.SetField(VariazioneAliquotaTfrAcroField.FIRMA_ADERENTE, Utente.EsponiAnagrafica().?.Trim()); TODO: Verificare come viene valorizzato nell'altro report.

                Logger.LogAzione(
                    new MyRai_LogAzioni
                    {
                        provenienza = "SceltaDestinazioneTfrController",
                        descrizione_operazione = "Fine_CompilaCampiVariazioneAliquotaTfr",
                        matricola = this.MatricolaUtente
                    },

                    this.MatricolaUtente);
            }
            catch (Exception l_Exception)
            {
                Logger.LogErrori(
                    new MyRai_LogErrori()
                    {
                        applicativo = this.Applicazione,
                        data = DateTime.Now,
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        error_message = l_Exception.Message,
                        provenienza = this.Provenienza
                    });

                throw;
            }
        }

        /// <summary>
        /// Restituisce il modello PDF per il report Variazione Aliquota TFR.
        /// </summary>
        private Stream GetTemplateVariazioneAliquotaTfr()
        {

            try
            {
                Logger.LogAzione(
                    new MyRai_LogAzioni
                    {
                        provenienza = "SceltaDestinazioneTfrController",
                        descrizione_operazione = "Inizio_GetTemplateVariazioneAliquotaTfr",
                        matricola = this.MatricolaUtente
                    },

                    this.MatricolaUtente);

                Stream stream = null;

                //// per test
                //string path = @"D:\Lavoro\Rai\ModuloTfr_Template.pdf";
                //stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
                //// <--

                using (var db = new digiGappEntities())
                {
                    var item =
                        db.MyRai_Moduli
                            .Where(w => w.codice_applicazione.Equals(this.CodModulo_PDF_VariazioneAliquotaTfr))
                            .FirstOrDefault()
                            .bytes_content;

                    stream = new MemoryStream(item);
                }

                Logger.LogAzione(
                    new MyRai_LogAzioni
                    {
                        provenienza = "SceltaDestinazioneTfrController",
                        descrizione_operazione = "Fine_GetTemplateVariazioneAliquotaTfr",
                        matricola = this.MatricolaUtente
                    },

                    this.MatricolaUtente);

                return stream;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(
                    new MyRai_LogErrori()
                    {
                        applicativo = this.Applicazione,
                        data = DateTime.Now,
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        error_message = ex.Message,
                        provenienza = this.Provenienza
                    });

                throw;
            }
        }

        /// <summary>
        /// Provvede ad inviare la mail di notifica della variazione dell'aliquota TFR.
        /// </summary>
        /// <param name="p_PdfContentBytes">
        /// Array <see cref="Byte"/> rappresentante il contenuto del report in formato PDF.
        /// </param>
        private void InviaPdfVariazioneAliquotaTfr(
            Byte[] p_PdfContentBytes)
        {

            try
            {

                var l_Recipient = CommonTasks.GetEmailPerMatricola(this.MatricolaUtente);
                if (String.IsNullOrWhiteSpace(l_Recipient))
                {
                    l_Recipient = $"P{this.MatricolaUtente}@rai.it";
                }

                var l_MailManager = new GestoreMail();
                var l_AttachmentList = new List<Attachement>();
                var l_Attachment =
                        new Attachement()
                        {
                            AttachementName = "ModuloVariazioneAliquotaTfr.pdf",
                            AttachementType = "Application/PDF",
                            AttachementValue = p_PdfContentBytes
                        };

                l_AttachmentList.Add(l_Attachment);

                var l_Body = "In allegato alla presente il modulo compilato. ";
                var l_Response =
                        l_MailManager.InvioMail(
                            "[CG] RaiPlace - Self Service <raiplace.selfservice@rai.it>",
                            " SelfService del Dipendente - Notifiche VARIAZIONE ALIQUOTA TFR",
                            l_Recipient,
                            "raiplace.selfservice@rai.it",
                            "VARIAZIONE ALIQUOTA TFR",
                            "",
                            l_Body,
                            null,
                            null,
                            l_AttachmentList);

                if (l_Response != null && l_Response.Errore != null)
                {
                    var l_Error = new MyRai_LogErrori()
                    {
                        applicativo = "Portale",
                        data = DateTime.Now,
                        provenienza = "ModuliController - InvioMail",
                        error_message = l_Response.Errore + " per " + l_Recipient
                    };

                    using (var l_DigiGappDB = new digiGappEntities())
                    {
                        l_DigiGappDB.MyRai_LogErrori.Add(l_Error);
                        l_DigiGappDB.SaveChanges();
                    }
                }
            }
            catch (Exception l_Exception)
            {
                var l_Error =
                        new MyRai_LogErrori()
                        {
                            applicativo = "Portale",
                            data = DateTime.Now,
                            provenienza = "ModuliController - WEB - InvioMail",
                            error_message = l_Exception.Message
                        };

                using (var l_DigiGappDB = new digiGappEntities())
                {
                    l_DigiGappDB.MyRai_LogErrori.Add(l_Error);
                    l_DigiGappDB.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Provvede al salvataggio nella tabella MyRai_Files del report Variazione Aliquota Tfr.
        /// </summary>
        /// <param name="p_PdfContentBytes">
        /// Array <see cref="Byte"/> rappresentante il contenuto del report in formato PDF.
        /// </param>
        private void SalvaPdfVariazioneQuotaTfr(
            Byte[] p_PdfContentBytes)
        {

            try
            {
                Logger.LogAzione(
                    new MyRai_LogAzioni
                    {
                        provenienza = "SceltaDestinazioneTfrController",
                        descrizione_operazione = "Inizio_SalvaPdfCompilatoVariazioneQuota",
                        matricola = this.MatricolaUtente
                    },

                    this.MatricolaUtente);

                // Leggo il record principale presente nella tabella XR_MOD_DIPENDENTI del db di talentia
                var recordDbTalentiaEntity = GetAllRecordFromDb();

                // Eseguo scrittura sul db che mi restituisce il nuovo id
                string chiave = GetChiaveFile(this.CodModulo_PDF_VariazioneAliquotaTfr, false);

                // Leggo i dati del database dove è salvato il record principale del documento dell'utente
                string nomeDatabase = string.Empty;
                using (TalentiaEntities dbTalentia = new TalentiaEntities())
                {
                    nomeDatabase = dbTalentia.Database.Connection.Database;
                }

                var jsonModel =
                        new SceltaDestinazioneTfrAllegatoJsonModel
                        {
                            MatricolaUtente = MatricolaUtente,
                            Titolo = "VariazioneAliquotaTfr.pdf",
                            Tipo = ((TipoAllegato)Enum.Parse(typeof(TipoAllegato), "AltriDocumenti")).ToString(),
                            IsObbligatorio = false,
                            NomeDatabaseEsterno = nomeDatabase,
                            NomeTabellaEsterna = "XR_MOD_DIPENDENTI",
                            IdRecordEsterno = recordDbTalentiaEntity.XR_MOD_DIPENDENTI1.ToString()
                        };

                // Salvo il file
                var l_UploadResult = FileManager.UploadFile(this.MatricolaUtente, "TFR_V", "VariazioneAliquotaTfr.pdf", p_PdfContentBytes, chiave, String.Empty, Newtonsoft.Json.JsonConvert.SerializeObject(jsonModel), false);
                if (!l_UploadResult.Esito)
                {
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        applicativo = Applicazione,
                        data = DateTime.Now,
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        error_message = chiave + ":" + l_UploadResult.Errore,
                        provenienza = Provenienza
                    });

                    throw new Exception();
                }

                ////// Creo il ViewModel restituito alla view che costruisce una riga della tabell deil files caricati
                ////var l_AttachmentModel = 
                ////        new SceltaDestinazioneTfrAllegatoViewModel {
                ////                Id = l_UploadResult.Files[0].Id, // Recupero l'id del file salvato
                ////                MatricolaUtente = MatricolaUtente,
                ////                Nome = "VariazioneAliquotaTfr.pdf",
                ////                Lunghezza = l_UploadResult.Files[0].Length,
                ////                Tipo = (TipoAllegato)Enum.Parse(typeof(TipoAllegato), "AltriDocumenti"),
                ////                IsObbligatorio = false,
                ////                IsCancellabile = true
                ////            };

                ////return View("_trFileUpload", l_AttachmentModel);

                Logger.LogAzione(
                    new MyRai_LogAzioni
                    {
                        provenienza = "SceltaDestinazioneTfrController",
                        descrizione_operazione = "Fine_SalvaPdfCompilatoVariazioneQuota",
                        matricola = this.MatricolaUtente,
                    },

                    this.MatricolaUtente);
            }
            catch (Exception l_Exception)
            {
                Logger.LogErrori(
                    new MyRai_LogErrori()
                    {
                        applicativo = Applicazione,
                        data = DateTime.Now,
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        error_message = l_Exception.Message,
                        provenienza = Provenienza,
                    });
                throw;
            }
        }

        /// <summary>
        /// Metodo che costruisce la stringa da utilizzare nel campo "Chiave" della tabella MyRai_Files
        /// </summary>
        /// <param name="isObbligatorio"></param>
        /// <returns></returns>
        private String GetChiaveFile(
            String p_ModuleCode
        , Boolean p_IsMandatory)
        {

            var l_Type = p_IsMandatory ? 'P' : 'S';
            return $"{this.MatricolaUtente}_{p_ModuleCode}_{l_Type}";
        }

        #endregion Report Variazione Aliquota TFR

        #region Documento Pdf Compilato
        /// <summary>
        /// Metodo che compila il pdf con i dati inseriti dall'utente
        /// </summary>
        /// <returns></returns>
        private void CompilaPdf()
        {
            byte[] output = null;

            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Inizio_CompilaPdf",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                // Leggo il template
                Stream template = GetTemplate();

                // Instanzio l'oggetto iTextSharp
                PdfReader reader = new PdfReader(template);
                MemoryStream ms = new MemoryStream();
                PdfStamper stamper = new PdfStamper(reader, ms, '4');

                // Ottengo quale sezione compilare
                SezioneDaCompilareTemplate sezioneDaCompilare = GetSezioneDaCompilare();

                // Compilo l'intestazione con i dati del dipendente
                CompilaIntestazione(stamper);

                switch (sezioneDaCompilare)
                {
                    case SezioneDaCompilareTemplate.Sezione_1:
                        CompilaSezione1(stamper);
                        break;
                    case SezioneDaCompilareTemplate.Sezione_2:
                        CompilaSezione2(stamper);
                        break;
                    case SezioneDaCompilareTemplate.Sezione_3:
                        CompilaSezione1(stamper);
                        break;
                }

                // Compilo data e firma
                CompilaDataEFirma(stamper);

                // Scrivo il template compilato su MemoryStream
                stamper.Writer.CloseStream = false;
                stamper.FormFlattening = true;
                stamper.Close();

                ms.Position = 0;
                output = ms.ToArray();
                ms.Flush();
                reader.Close();

                // Salvo il pdf compilato
                SalvaPdfCompilato(output);

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Fine_CompilaPdf",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    error_message = ex.Message
                }, CommonHelper.GetCurrentUserMatricola());
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che restituisce lo stream del template pdf da compilare
        /// </summary>
        /// <returns></returns>
        private Stream GetTemplate()
        {
            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Inizio_GetTemplate",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                Stream stream = null;

                //// per test
                //string path = @"D:\Lavoro\Rai\ModuloTfr_Template.pdf";
                //stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
                //// <--
                ///
                using (digiGappEntities db = new digiGappEntities())
                {
                    byte[] item = db.MyRai_Moduli
                        .Where(w => w.codice_applicazione.Equals(CodModulo_PDF))
                        .FirstOrDefault()
                        .bytes_content;

                    stream = new MemoryStream(item);
                }

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Fine_GetTemplate",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                return stream;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    error_message = ex.Message,
                }, CommonManager.GetCurrentUserMatricola());
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che restituisce quale sezione pdf deve essere compilata
        /// </summary>
        /// <returns></returns>
        private SezioneDaCompilareTemplate GetSezioneDaCompilare()
        {
            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Inizio_GetSezioneDaCompilare",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                SezioneDaCompilareTemplate sezioneDaCompilare = SezioneDaCompilareTemplate.Nessuna;

                sezioneDaCompilare = SezioneDaCompilareTemplate.Sezione_1; // TODO Valore camlato che verrà tolto quando ci sarà la logica

                SceltaDestinazioneTfrScelteEffettuateModel valoriModuloFromDb = GetValoriModuloFromDb();

                // Casi:
                /*
                 * 1) Se non ho mai effettuato una scelta e adesso lo lascio in azienda -> sezione 1
                 * 2) Se non ho mai effettuato una scelta e adesso sono iscritto ad altra forma previdenziale -> sezione 1
                 * 3) Se ho già effettuato una scelta e adesso sono iscritto ad altra forma previdenziale
                 *      3.1) Se la data di adesione è > 28/04/1993 -> sezione 1
                 *      3.2) Se la data di adesione è < 29/04/1993 non prev 6 -> sezione 2
                 *      3.3) Se la data di adesione è < 29/04/1993 prev 6 -> sezione 3
                 */

                // Se non ho mai effettuato una scelta e adesso lascio il tfr in azienda -> Sezione 1
                if (!valoriModuloFromDb.IsSceltaPregressaEffettuata &&
                    !string.IsNullOrWhiteSpace(valoriModuloFromDb.SceltaEffettuata) &&
                    valoriModuloFromDb.SceltaEffettuata == "In_Azienda")
                {
                    // Caso 1
                    sezioneDaCompilare = SezioneDaCompilareTemplate.Sezione_1;
                }
                else if (!valoriModuloFromDb.IsSceltaPregressaEffettuata &&
                    !string.IsNullOrWhiteSpace(valoriModuloFromDb.SceltaEffettuata) &&
                    valoriModuloFromDb.SceltaEffettuata == "Altro")
                {
                    // Caso 2
                    sezioneDaCompilare = SezioneDaCompilareTemplate.Sezione_1;
                }
                else if (valoriModuloFromDb.IsSceltaPregressaEffettuata &&
                    !string.IsNullOrWhiteSpace(valoriModuloFromDb.SceltaEffettuata) &&
                    valoriModuloFromDb.SceltaEffettuata == "Altro")
                {
                    // Casi 3
                    //// CL - 20230401 - Nel gestionale MyRai, il caso 3 è associato alla data di prima iscrizione e non alla data di adesione al fondo.
                    ////if (valoriModuloFromDb.DataAdesioneAlFondo.HasValue)
                    ////{
                    ////    if (valoriModuloFromDb.DataAdesioneAlFondo.Value.Date > new DateTime(1993, 4, 28))
                    ////    {
                    ////        // Caso 3.1
                    ////        sezioneDaCompilare = SezioneDaCompilareTemplate.Sezione_1;
                    ////    }
                    ////    else if (valoriModuloFromDb.DataAdesioneAlFondo.Value.Date < new DateTime(1993, 4, 29) && !IsPrev6())
                    ////    {
                    ////        // Caso 3.2
                    ////        sezioneDaCompilare = SezioneDaCompilareTemplate.Sezione_2;
                    ////    }
                    ////    else if (valoriModuloFromDb.DataAdesioneAlFondo.Value.Date < new DateTime(1993, 4, 29) && IsPrev6())
                    ////    {
                    ////        // Caso 3.3
                    ////        sezioneDaCompilare = SezioneDaCompilareTemplate.Sezione_3;
                    ////    }
                    ////}
                    if (valoriModuloFromDb.DataPrimaIscrizione.HasValue)
                    {
                        if (valoriModuloFromDb.DataPrimaIscrizione.Value.Date > new DateTime(1993, 4, 28))
                        {
                            // Caso 3.1
                            sezioneDaCompilare = SezioneDaCompilareTemplate.Sezione_1;
                        }
                        else if (valoriModuloFromDb.DataPrimaIscrizione.Value.Date < new DateTime(1993, 4, 29) && !IsPrev6())
                        {
                            // Caso 3.2
                            sezioneDaCompilare = SezioneDaCompilareTemplate.Sezione_2;
                        }
                        else if (valoriModuloFromDb.DataPrimaIscrizione.Value.Date < new DateTime(1993, 4, 29) && IsPrev6())
                        {
                            // Caso 3.3
                            sezioneDaCompilare = SezioneDaCompilareTemplate.Sezione_3;
                        }
                    }
                }

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Fine_GetSezioneDaCompilare",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                return sezioneDaCompilare;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    error_message = ex.Message,
                }, CommonHelper.GetCurrentUserMatricola());
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che compila l'intestazione del pdf
        /// </summary>
        /// <param name="stamper"></param>
        private void CompilaIntestazione(PdfStamper stamper)
        {
            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Inizio_CompilaIntestazione",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                myRaiData.Incentivi.SINTESI1 sintesi;
                using (IncentiviEntities incentiviEntities = new IncentiviEntities())
                {
                    sintesi = incentiviEntities.SINTESI1.Where(w => w.COD_MATLIBROMAT == MatricolaUtente).FirstOrDefault();
                    if (sintesi == null)
                    {
                        throw new Exception("Utente non trovato in anagrafica");
                    }
                }

                stamper.AcroFields.SetField(AcroFieldTemplate.NOME_COGNOME, sintesi.DES_COGNOMEPERS + " " + sintesi.DES_NOMEPERS);
                stamper.AcroFields.SetField(AcroFieldTemplate.NATO_A, sintesi.DES_CITTANASC);
                stamper.AcroFields.SetField(AcroFieldTemplate.NATO_IL, sintesi.DTA_NASCITAPERS.Value.ToShortDateString());
                stamper.AcroFields.SetField(AcroFieldTemplate.CODICE_FISCALE, sintesi.CSF_CFSPERSONA);
                stamper.AcroFields.SetField(AcroFieldTemplate.DIPENDENTE_DEL, sintesi.COD_SOGGETTOCR);

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Fine_CompilaIntestazione",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    error_message = ex.Message,
                }, CommonHelper.GetCurrentUserMatricola());
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che compila la sezione 1 del pdf
        /// </summary>
        /// <param name="stamper"></param>
        private void CompilaSezione1(PdfStamper stamper)
        {
            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Inizio_CompilaSezione1",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                // Ottengo i valori delle scelte dell'utente salvati in json sul db
                var valoriScelte = GetValoriModuloFromDb();

                // Compilo il template
                if (valoriScelte.SceltaEffettuata == "In_Azienda")
                {
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE1_IN_AZIENDA, "X");
                }
                else
                {
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE1_NO_IN_AZIENDA, "X");
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE1_PERCENTUALE, valoriScelte.QuotaSelezionata.ToString());
                    string formaComplementare = valoriScelte.SceltaSpecificaEffettuata == "Altro" ? valoriScelte.SceltaSpecificaDefinitaDaUtente : valoriScelte.SceltaSpecificaEffettuata;
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE1_FORMA_COMPLEMENTARE, formaComplementare);

                    //// CL - 20230104 - Corretto errore di digitazione nella variabile dataAdesioneAlFonto.
                    DateTime? dataAdesioneFondo = valoriScelte.DataAdesioneAlFondo;
                    if (dataAdesioneFondo.HasValue)
                    {
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE1_GIORNO, dataAdesioneFondo.Value.Day.ToString());
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE1_MESE, dataAdesioneFondo.Value.Month.ToString());
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE1_ANNO, dataAdesioneFondo.Value.Year.ToString());
                    }
                }

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Fine_CompilaSezione1",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    error_message = ex.Message,
                }, CommonHelper.GetCurrentUserMatricola());
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che compila la sezione 1 del pdf
        /// </summary>
        /// <param name="stamper"></param>
        private void CompilaSezione2(PdfStamper stamper)
        {
            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Inizio_CompilaSezione2",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                // Ottengo i valori delle scelte dell'utente salvati in json sul db
                var valoriScelte = GetValoriModuloFromDb();

                // Compilo il template
                if (valoriScelte.SceltaEffettuata == "In_Azienda")
                {
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE2_IN_AZIENDA, "X");
                }
                else if (valoriScelte.QuotaSelezionata != 100)
                {
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE2_NO_IN_AZIENDA_PERCENTUALE, "X");
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE2_PERCENTUALE, valoriScelte.QuotaSelezionata.ToString());
                    string formaComplementare = valoriScelte.SceltaSpecificaEffettuata == "Altro" ? valoriScelte.SceltaSpecificaDefinitaDaUtente : valoriScelte.SceltaSpecificaEffettuata;
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE2_FORMA_COMPLEMENTARE_PERCENTUALE, formaComplementare);
                    DateTime? dataAdesioneAlFonto = valoriScelte.DataAdesioneAlFondo;
                    if (dataAdesioneAlFonto.HasValue)
                    {
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE2_GIORNO_PERCENTUALE, dataAdesioneAlFonto.Value.Day.ToString());
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE2_MESE_PERCENTUALE, dataAdesioneAlFonto.Value.Month.ToString());
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE2_ANNO_PERCENTUALE, dataAdesioneAlFonto.Value.Year.ToString());
                    }
                }
                else
                {
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE2_NO_IN_AZIENDA_INTERO, "X");
                    string formaComplementare = valoriScelte.SceltaSpecificaEffettuata == "Altro" ? valoriScelte.SceltaSpecificaDefinitaDaUtente : valoriScelte.SceltaSpecificaEffettuata;
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE2_FORMA_COMPLEMENTARE_INTERO, formaComplementare);
                    DateTime? dataAdesioneAlFonto = valoriScelte.DataAdesioneAlFondo;
                    if (dataAdesioneAlFonto.HasValue)
                    {
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE2_GIORNO_INTERO, dataAdesioneAlFonto.Value.Day.ToString());
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE2_MESE_INTERO, dataAdesioneAlFonto.Value.Month.ToString());
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE2_ANNO_INTERO, dataAdesioneAlFonto.Value.Year.ToString());
                    }
                }

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Fine_CompilaSezione2",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    error_message = ex.Message
                });
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che compila la sezione 1 del pdf
        /// </summary>
        /// <param name="stamper"></param>
        private void CompilaSezione3(PdfStamper stamper)
        {
            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Inizio_CompilaSezione3",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                // Ottengo i valori delle scelte dell'utente salvati in json sul db
                var valoriScelte = GetValoriModuloFromDb();

                // Compilo il template
                if (valoriScelte.SceltaEffettuata == "In_Azienda")
                {
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE3_IN_AZIENDA, "X");
                }
                else if (valoriScelte.QuotaSelezionata != 100)
                {
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE3_NO_IN_AZIENDA_PERCENTUALE, "X");
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE3_PERCENTUALE, valoriScelte.QuotaSelezionata.ToString());
                    string formaComplementare = valoriScelte.SceltaSpecificaEffettuata == "Altro" ? valoriScelte.SceltaSpecificaDefinitaDaUtente : valoriScelte.SceltaSpecificaEffettuata;
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE3_FORMA_COMPLEMENTARE_PERCENTUALE, formaComplementare);
                    DateTime? dataAdesioneAlFonto = valoriScelte.DataAdesioneAlFondo;
                    if (dataAdesioneAlFonto.HasValue)
                    {
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE3_GIORNO_PERCENTUALE, dataAdesioneAlFonto.Value.Day.ToString());
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE3_MESE_PERCENTUALE, dataAdesioneAlFonto.Value.Month.ToString());
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE3_ANNO_PERCENTUALE, dataAdesioneAlFonto.Value.Year.ToString());
                    }
                }
                else
                {
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE3_NO_IN_AZIENDA_INTERO, "X");
                    string formaComplementare = valoriScelte.SceltaSpecificaEffettuata == "Altro" ? valoriScelte.SceltaSpecificaDefinitaDaUtente : valoriScelte.SceltaSpecificaEffettuata;
                    stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE3_FORMA_COMPLEMENTARE_INTERO, formaComplementare);
                    DateTime? dataAdesioneAlFonto = valoriScelte.DataAdesioneAlFondo;
                    if (dataAdesioneAlFonto.HasValue)
                    {
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE3_GIORNO_INTERO, dataAdesioneAlFonto.Value.Day.ToString());
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE3_MESE_INTERO, dataAdesioneAlFonto.Value.Month.ToString());
                        stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE3_ANNO_INTERO, dataAdesioneAlFonto.Value.Year.ToString());
                    }
                }

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Fine_CompilaSezione3",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    error_message = ex.Message,
                }, CommonHelper.GetCurrentUserMatricola());
                throw ex;
            }
        }

        private void CompilaDataEFirma(PdfStamper stamper)
        {
            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Inizio_CompilaDataEFirma",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, MatricolaUtente);

                // Compilazione prima della validazione (HRis non ha il problema di riempiere data e firma visto che viene fatto da RaiPerMe)
                stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE_FIRMA_RIGA_1, AcroFieldTemplate.SEZIONE_FIRMA_RIGA_1_PLACEHOLDER);
                stamper.AcroFields.SetField(AcroFieldTemplate.SEZIONE_FIRMA_RIGA_2, AcroFieldTemplate.SEZIONE_FIRMA_RIGA_2_PLACEHOLDER);

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Fine_CompilaDataEFirma",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    error_message = ex.Message,
                }, CommonHelper.GetCurrentUserMatricola());
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che salva sul db il pdf compilato
        /// </summary>
        /// <param name="pdfCompilato"></param>
        private void SalvaPdfCompilato(byte[] pdfCompilato)
        {
            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Inizio_SalvaPdfCompilato",
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                // Salvo il pdf sul db
                XR_MOD_DIPENDENTI recordXrModDipendenti = null;

                using (TalentiaEntities dbTalentia = new TalentiaEntities())
                {
                    recordXrModDipendenti = dbTalentia.XR_MOD_DIPENDENTI
                        .Where(w => w.MATRICOLA.Equals(MatricolaUtente) && w.COD_MODULO.Equals(CodModulo))
                        .FirstOrDefault();

                    recordXrModDipendenti.PDF_MODULO = pdfCompilato;

                    dbTalentia.SaveChanges();
                }

                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Fine_SalvaPdfCompilato",
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                }, CommonHelper.GetCurrentUserMatricola());
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    error_message = ex.Message,
                }, CommonHelper.GetCurrentUserMatricola());
                throw ex;
            }
        }

        /// <summary>
        /// Metodo che legge il documento pdf compilato dal db
        /// </summary>
        /// <param name="matricola"></param>
        /// <returns></returns>
        public ActionResult GetDocumentoCompilato(string matricola)
        {
            string nomeFile = "Scelta per la destinazione del TFR.pdf";

            MatricolaUtente = matricola;

            try
            {
                Logger.LogAzione(new MyRai_LogAzioni
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    descrizione_operazione = "Inizio_GetDocumentoCompilato-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(matricola),
                    matricola = CommonHelper.GetCurrentUserMatricola()
                }, CommonHelper.GetCurrentUserMatricola());

                // Leggo il record dell'utente
                XR_MOD_DIPENDENTI recordXrModDipendenti = GetAllRecordFromDb();

                // Scarico il documento
                if (recordXrModDipendenti != null && recordXrModDipendenti.PDF_MODULO != null)
                {
                    byte[] byteArray = recordXrModDipendenti.PDF_MODULO;

                    MemoryStream pdfStream = new MemoryStream();
                    pdfStream.Write(byteArray, 0, byteArray.Length);
                    pdfStream.Position = 0;

                    Response.AddHeader("Content-Disposition", "inline; filename=" + nomeFile);

                    Logger.LogAzione(new MyRai_LogAzioni
                    {
                        applicativo = "Portale",
                        provenienza = "SceltaDestinazioneTfrController",
                        descrizione_operazione = "Fine_GetDocumentoCompilato-DatiJson: " + Newtonsoft.Json.JsonConvert.SerializeObject(matricola),
                        matricola = CommonHelper.GetCurrentUserMatricola()
                    }, CommonHelper.GetCurrentUserMatricola());

                    return File(byteArray, "application/pdf");
                }
                else
                {
                    throw new Exception("Non è stato possibile reperire il documento compilato");
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    provenienza = "SceltaDestinazioneTfrController",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    error_message = ex.Message,
                }, CommonHelper.GetCurrentUserMatricola());

                ViewBag.Eccezione = ex.Message;
                return View("_erroreLetturaFile");
            }
        }
        #endregion

        #region Metodi riscritti da altre sezioni per l'utente
        public bool IsDirigenteGiornalista_OR_Giornalista()
        {
            bool result = false;

            try
            {
                string tipologia = TipoDipendente(MatricolaUtente, DateTime.Today);

                string cod_Categoria = Categoria();

                if (cod_Categoria.Trim().ToUpper().StartsWith("A7") || tipologia == "G" || cod_Categoria.Trim().ToUpper().StartsWith("MXX"))
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        public bool IsDirigente()
        {
            bool result = false;

            try
            {
                string tipologia = TipoDipendente(MatricolaUtente, DateTime.Today);

                string cod_Categoria = Categoria();

                if (cod_Categoria.Trim().ToUpper().StartsWith("A01") || tipologia == "D")
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        public bool IsTI()
        {
            bool result = false;
            string c = FormaContratto();
            result = (c == "9" || c.Trim().ToUpper() == "K");
            return result;
        }

        public string FormaContratto()
        {
            myRaiData.Incentivi.SINTESI1 sintesi;
            using (IncentiviEntities incentiviEntities = new IncentiviEntities())
            {
                sintesi = incentiviEntities.SINTESI1.Where(w => w.COD_MATLIBROMAT == MatricolaUtente).FirstOrDefault();
                if (sintesi == null || string.IsNullOrWhiteSpace(sintesi.COD_TPCNTR))
                {
                    throw new Exception("Dati utente non trovati in anagrafica");
                }
            }

            return sintesi.COD_TPCNTR;
        }

        private string TipoDipendente(string matricola, DateTime D)
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.AccountUtenteServizio)[0],
                                                             myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.AccountUtenteServizio)[1]);

            MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp response = cl.recuperaUtente(matricola.PadLeft(7, '0'), D.ToString("ddMMyyyy"));
            return response.data.tipo_dipendente;
        }

        private string Categoria()
        {
            myRaiData.Incentivi.SINTESI1 sintesi;
            using (IncentiviEntities incentiviEntities = new IncentiviEntities())
            {
                sintesi = incentiviEntities.SINTESI1.Where(w => w.COD_MATLIBROMAT == MatricolaUtente).FirstOrDefault();
                if (sintesi == null)
                {
                    throw new Exception("Utente non trovato in anagrafica");
                }
            }

            return sintesi.COD_QUALIFICA;
        }

        private bool IsPrev6()
        {
            bool esito = false;

            using (var sediDB = new PERSEOEntities())
            {
                string output = "";
                string anno = DateTime.Now.Year.ToString();
                string mese = (DateTime.Now.Month - 1).ToString();
                if (mese.Length == 1)
                {
                    mese = "0" + mese;
                }

                string query = "SELECT te.CODICINI " +
                                "FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] anag " +
                                "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] " +
                                "te ON(te.[SKY_MATRICOLA] = anag.[sky_anagrafica_unica]) " +
                                "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] " +
                                "tempo ON(te.[SKY_MESE_CONTABILE] = tempo.[sky_tempo]) " +
                                "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE_IMPORTI] " +
                                "importi ON(importi.[SKY_riga_te] = te.[SKY_riga_te]) " +
                                "WHERE anag.matricola_dp = '#MATRICOLA#' " +
                                "and num_anno = #ANNO#" +
                                "and cod_mese = #MESE#" +
                                "and te.num_fine_mese = 1";

                query = query.Replace("#MATRICOLA#", MatricolaUtente);
                query = query.Replace("#ANNO#", anno);
                query = query.Replace("#MESE#", mese);
                output = sediDB.Database.SqlQuery<string>(query).FirstOrDefault();

                if (output != null)
                {
                    string byte9 = output.Substring(8, 1);
                    if (byte9.Equals("6"))
                    {
                        esito = true;
                    }
                    else
                    {
                        esito = false;
                    }
                }
            }

            return esito;
        }
        #endregion
    }
}
