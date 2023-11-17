using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public class SceltaDestinazioneTfrScelteEffettuateModelBase
    {
        // Dati Utente
        public string MatricolaUtente { get; set; }
        public int IdUtente { get; set; }
        public int IdRecordScelta { get; set; }
    }

    public class SceltaDestinazioneTfrScelteEffettuateModel : SceltaDestinazioneTfrScelteEffettuateModelBase
    {
        // Step corrente del dipendente
        public StepsEnum StepCorrente { get; set; }

        // Step corrente dell'amministrazione
        public StepsEnum StepCorrenteAmministrazione { get; set; }

        // Campi step 1
        public bool IsSceltaPregressaEffettuata { get; set; }
        public string MotivoSceltaPregressa { get; set; }
        public string SceltaEffettuata { get; set; }
        public string SceltaSpecificaEffettuata { get; set; }
        public string SceltaSpecificaEffettuata_Codice { get; set; }

        // Campi step 2
        public string SceltaSpecificaDefinitaDaUtente { get; set; }
        public string SceltaSpecificaDefinitaDaUtente_Codice { get; set; }
        public int QuotaSelezionata { get; set; }

        /// <summary>
        /// La quota selezionata precedente serve a verificare se la percentuale TFR è stata modificata oppure no (per la generazione del report Variazione Aliquota TFR).
        /// </summary>
        public int QuotaSelezionataPrecedente { get; set; }

        /// <summary>
        /// L'anno di variazione dell'aliquota TFR e' facoltativo e viene riportato (se presente) sul report Variazione Aliquota TFR.
        /// </summary>
        public string AnnoVariazioneAliquota { get; set; }

        /// <summary>
        /// Il numero di iscrizione al CRAI e' facoltativo e viene riportato (se presente) sul report Variazione Aliquota TFR.
        /// </summary>
        public string NumeroIscrizioneCrai { get; set; }

        public bool DoppioImponibile { get; set; }
        public DateTime? DataAdesioneAlFondo { get; set; }
        public DateTime? DataCompilazione { get; set; }
        
        //// CL - Aggiunta la data di prima iscrizione.
        public DateTime? DataPrimaIscrizione { get; set; }

        // Campi step 3
        // Per HRIS lo step 3 è la sola visualizzazione del pdf compilato

        // Proprietà che viene valorizzata a true se è Hris a completare la pratica
        public bool CompilatoDaHrisECodiceOtpDaInviare { get; set; } = false;

        // Proprietà che viene valorizzata a true se è importato da CICS
        public bool ImportatoDaCics { get; set; } = false;
    }

    public class SceltaDestinazioneTfrViewModel : SceltaDestinazioneTfrScelteEffettuateModel
    {
        // Wizard
        public List<StepItem> Steps { get; set; }

        public List<StepItemHrisValidazione> StepsHrisValidazione { get; set; }

        // Campi step 1 (Oltre quelli ereditati da SceltaDestinazioneTfrScelteEffettuateModel)
        public List<SelectListItem> SceltePregresseDisponibili { get; set; }
        public List<SelectListItem> MotiviSceltePregresseDisponibili { get; set; }
        public List<SelectListItem> ScelteDisponibili { get; set; }

        // Campi step 2 (Oltre quelli ereditati da SceltaDestinazioneTfrScelteEffettuateModel)
        public List<SelectListItem> QuoteDisponibili { get; set; }
        public List<SelectListItem> FondiSpecificiDisponibili { get; set; }

        // Campi step 3 e 4
        public SceltaDestinazioneTfrAllegatoViewModel ModuloDiAdesione { get; set; } // Unico obbligatorio
        public List<SceltaDestinazioneTfrAllegatoViewModel> AltriDocumenti { get; set; }

        // Campi step 4
        public string DescrizioneSceltaPregressaEffettuata { get; set; }
        public string DescrizioneMotivoSceltaPregressaEffettuata { get; set; }
        public string DescrizioneSceltaAttualeEffettuata { get; set; }

        // Campo aggiuntivo passo Finale (I campi del riepilogo sono quelli dello step 4)
        public SceltaDestinazioneTfrAllegatoViewModel PdfCompilato { get; set; }
    }

    // ViewModel di un oggetto file
    public class SceltaDestinazioneTfrAllegatoViewModel : SceltaDestinazioneTfrScelteEffettuateModelBase
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public long Lunghezza { get; set; }
        public string Titolo { get; set; }
        public TipoAllegato Tipo { get; set; }
        public string Descrizione { get; set; }
        public bool IsObbligatorio { get; set; }
        public bool IsCancellabile { get; set; }
    }

    public class SceltaDestinazioneTfrAllegatoJsonModel : SceltaDestinazioneTfrScelteEffettuateModelBase
    {
        public string Titolo { get; set; }
        public string Tipo { get; set; }
        public bool IsObbligatorio { get; set; }
        // Campi esterni sulla tabella XR_MOD_DIPENDENTI di Talentia
        public string NomeDatabaseEsterno { get; set; }
        public string NomeTabellaEsterna { get; set; }
        public string IdRecordEsterno { get; set; }
    }

    public class SceltaDestinazioneTfrDromDownChildModel
    {
        public string Id { get; set; }
        public List<SelectListItem> Items { get; set; }
    }

    // Struttura utilizzata per i controlli che prevedono Text e Value. E' previsto che un campo titolo
    public class TextValueItem
    {
        public object Valore { get; set; }
        public string Titolo { get; set; }
        public string Descrizione { get; set; }
    }

    // i-mo step del controllo wizard
    public class StepItem
    {
        public StepsEnum Step { get; set; }
        public bool IsCompletato { get; set; }
        public bool IsCorrente { get; set; }
    }

    public class StepItemHrisValidazione
    {
        public StepsEnumHrisValidazione Step { get; set; }
        public bool IsCompletato { get; set; }
        public bool IsCorrente { get; set; }
        public DateTime? DataEvento { get; set; }
    }

    // Enumeratori
    public enum StepsEnum
    {
        PREGRESSO,
        DETTAGLI,
        DOCUMENTO,
        RIEPILOGO,
        SCELTE_EFFETTUATE
    }

    public enum StepsEnumHrisValidazione
    {
        RICHIESTA_INSERITA,
        RICHIESTA_IN_VALIDAZIONE,
        RICHIESTA_VALIDATA,
    }

    public enum TipoAllegato
    {
        PdfCompilato,
        ModuloDiAdesione,
        AltriDocumenti
    }
}
