using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using myRaiData;

namespace myRaiService
{
    public enum EnumWorkflowRichieste
    {
        TuttiISettori = 0,
        Segreteria = 2,
        UfficioPersonale = 3
    }
    public enum EnumTipoRicerca
    {
        DataRichiesta,
        DataEccezione
    }
    public enum EnumStatiRichiesta
    {
        InseritoUfficioPersonale = 5,
        InProgressUfficioPersonale = 6,

        InseritoSegreteria = 7,
        InProgressSegreteria = 8,

        InApprovazione = 10,
        Approvata = 20,
        Stampata = 30,
        Convalidata = 40,
        Rifiutata = 50,
        Cancellata = 60,
        Eliminata = 70,

        TuttiGliStati = 0
    }
    public class SbloccaEccezioniResponse
    {
        public Boolean Success { get; set; }
        public string Error { get; set; }
    }
    public class GetDocumentoEccezioneResponse
    {
        public Boolean Success { get; set; }
        public string Error { get; set; }
        public byte[] document { get; set; }
    }

    public class GetDipendentiResponse
    {
        public string raw { get; set; }
        public DataDipendente[] DatiDipendenti { get; set; }
        public DataDipendente[] DatiDipendentiPresenti { get; set; }
        public DataDip[] datadip { get; set; }
        public DataDip[] datadipPresenti { get; set; }
        public Boolean Success { get; set; }
        public string Error { get; set; }
    }
    public class GetRuoliResponse
    {
        public string sent { get; set; }
        public string raw { get; set; }
        public Boolean Success { get; set; }
        public string Error { get; set; }
        public List<GetRuoliItem> Eccezioni { get; set; }
    }
    /*  OUT-ECCEZ	X	(004)	Codice eccezione ( Primary key RPRUOLI)
  OUT-DATA-DOC 	9	(008)	Data documento  (ggmmaaaa Primary key RPRUOLI)
  OUT-NUM-DOC 	9	(006)	Num  doc eccezione (Primary key RPRUOLI)
  OUT-INECC 	9	(004)	Inizio eccezione hhmm
  OUT-FINECC 	9	(004)	Fine eccezione hhmm
  OUT-OREECC	9 	(004)	Ora eccezione hhmm
  OUT-TIPEC	X	(001)	Tipo eccezione
  OUT-STORNO	X	(001)	Flag storno
  OUT-UM	X	(001)	Unità misura eccezione
  OUT-QTA	9	(010)	Quantità eccezione
  OUT-SEDE	X	(005)	Sede Gapp
  OUT-COD-RACCO	X	(003)	Codice raccolta (AST / ASS / SCI / VAR / Spazio)
  OUT-ECC1-9000	X	(004)	Codice  eccezione 1  da  RPPRASS ( Ark 9000)
  OUT-ECC2-9000	X	(004)	Codice  eccezione 2  da  RPPRASS ( Ark 9000)
  OUT-ECC3-9000	X	(004)	Codice  eccezione 3  da  RPPRASS ( Ark 9000)
*/
    public class GetRuoliItem
    {
        public string CodiceEccezione { get; set; }
        public DateTime DataDocumento { get; set; }
        public string NumeroDocumento { get; set; }

        public string InizioEccezioneHHMM { get; set; }
        public string InizioEccezioneMinuti { get; set; }
        public DateTime InizioEccezioneDatetime { get; set; }

        public string FineEccezioneHHMM { get; set; }
        public string FineEccezioneMinuti { get; set; }
        public DateTime FineEccezioneDatetime { get; set; }

        public string OraEccezioneHHMM { get; set; }
        public string OraEccezioneMinuti { get; set; }
        public string TipoEccezione { get; set; }
        public string FlagStorno { get; set; }
        public string UnitaMisura { get; set; }
        public string Quantita { get; set; }
         
        public string Sede { get; set; }
        public string CodiceRaccolta { get; set; }
        public string CodiceEccezione1 { get; set; }
        public string CodiceEccezione2 { get; set; }
        public string CodiceEccezione3 { get; set; }

        public string CodiceOrario { get; set; }
        public string StatoEccezione { get; set; }
        public string ParseError { get; set; }

    }

    public class GetAnalisiEccezioniResponse
    {
        public string raw { get; set; }
        public List<AnalisiEccezione> AnalisiEccezione { get; set; }
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string Tipodipendente { get; set; }
        public DateTime Datainiziorapporto { get; set; }
        public DateTime Datafinerapporto { get; set; }

        public List<DateMinutiEccezione> DettagliEccezioni { get; set; }

        public Boolean Success { get; set; }
        public string Error { get; set; }
    }

    public class AnalisiEccezione
    {
        public string codice { get; set; }
        public string unitamisura { get; set; }
        public string totale { get; set; }
        public string massimale { get; set; }

        public GiornoAnalisiEccezione[] giornisingoli { get; set; }

    }

    public class GiornoAnalisiEccezione
    {
        public DateTime Data { get; set; }
        public string quantita { get; set; }
    }

    public class DataDipendente
    {
        public string matricola { get; set; }
        public string nominativo { get; set; }
        public string UrlFoto { get; set; }
        public DataEccezioni[] DettaglioDate { get; set; }
    }
    public class DataEccezioni
    {
        public DateTime data { get; set; }
        public string[] eccezioni { get; set; }
    }
    public class DataDip
    {
        public DateTime data { get; set; }
        public string matricola { get; set; }
        public string nominativo { get; set; }
        public string UrlFoto { get; set; }
        public string[] eccezioni { get; set; }
    }

    public class Riepilogo
    {
        public List<StatoRichiesta> StatiRichieste { get; set; }
        public List<SedePDF> SediPdf { get; set; }
    }
    public class SedePDF
    {
        public string Sede { get; set; }
        public List<PDFperiodo> PDFlist { get; set; }
    }
    public class PDFperiodo
    {
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public string Tipo { get; set; }
    }

    public class StatoRichiesta
    {
        public EnumStatiRichiesta stato { get; set; }
        public int TotaleRichieste { get; set; }
    }

    public class CambiaStatoResponse
    {
        public Boolean Success { get; set; }
        public string Error { get; set; }
    }
    public class DatiStorno
    {
        public string ndoc { get; set; }
        public DettaglioRichiesta Richiesta { get; set; }

    }

    //DTOs
    public class DettaglioGiornataResponse
    {
        public List<RelazioneEccezioni> RelazioniEccezioni { get; set; }
        public Boolean success { get; set; }
        public string error { get; set; }
    }
    public class RelazioneEccezioni
    {
        public int numdoc { get; set; }
        public myRaiService.it.rai.servizi.svildigigappws.Eccezione eccezioneGapp { get; set; }
        public DettaglioRichiesta eccezioneDB { get; set; }
    }


    public class DettaglioRichiesta
    {
        public int id_richiesta { get; set; }
        public int id_tipo_richiesta { get; set; }
        public System.DateTime data_richiesta { get; set; }
        public string matricola_richiesta { get; set; }
        public System.DateTime periodo_dal { get; set; }
        public System.DateTime periodo_al { get; set; }
        public string matricola_consuntivazione { get; set; }
        public string motivo_richiesta { get; set; }
        public string codice_sede_gapp { get; set; }
        public string descr_sede_gapp { get; set; }
        public string nominativo { get; set; }
        public int id_stato { get; set; }
        public Nullable<System.DateTime> data_scadenza { get; set; }
        public bool urgente { get; set; }
        public bool scaduta { get; set; }

        public string cod_eccezione { get; set; }
        public int? IdDocumentoAllegato { get; set; }

        public DettaglioEccezione[] DettagliEccezione { get; set; }

        public AttCeiton AttivitaCeiton { get; set; }
    }
    public class DettaglioEccezione
    {
        public int id_eccezioni_richieste { get; set; }
        public int id_richiesta { get; set; }
        public string azione { get; set; }
        public string cod_eccezione { get; set; }
        public System.DateTime data_eccezione { get; set; }
        public Nullable<int> numero_documento { get; set; }
        public Nullable<System.DateTime> dalle { get; set; }
        public Nullable<System.DateTime> alle { get; set; }
        public Nullable<decimal> quantita { get; set; }
        public Nullable<decimal> importo { get; set; }
        public string uorg { get; set; }
        public string df { get; set; }
        public string matricola_spettacolo { get; set; }
        public string motivo_richiesta { get; set; }
        public string recapito_durante_assenza { get; set; }
        public int id_stato { get; set; }
        public Nullable<System.DateTime> data_validazione_primo_livello { get; set; }
        public Nullable<System.DateTime> data_rifiuto_primo_livello { get; set; }
        public string nota_rifiuto_o_approvazione { get; set; }
        public string matricola_primo_livello { get; set; }
        public Nullable<System.DateTime> data_stampa { get; set; }
        public Nullable<System.DateTime> data_convalida { get; set; }
        public Nullable<System.DateTime> DataRipianificazione { get; set; }
        public string matricola_secondo_livello { get; set; }
        public string codice_sede_gapp { get; set; }
        public string nominativo_primo_livello { get; set; }
        public string nominativo_secondo_livello { get; set; }
        public Nullable<int> numero_documento_riferimento { get; set; }
        public System.DateTime data_creazione { get; set; }
        public string tipo_eccezione { get; set; }
        public string ValoriParamExtraJSON { get; set; }
        public string workflow { get; set; }
        public string desc_eccezione { get; set; }
        public Nullable<System.DateTime> data_approvazione_segreteria { get; set; }
        public string matricola_segreteria { get; set; }
        public Nullable<System.DateTime> data_approvazione_uff_personale { get; set; }
        public string matricola_uff_personale { get; set; }
    }

    public partial class AttCeiton
    {
        public int id { get; set; }
        public string AttivitaPrimaria { get; set; }
        public string AttivitaSecondaria { get; set; }
        public string AttivitaSvolta { get; set; }
        public Nullable<System.DateTime> DataAttivita { get; set; }
        public string Matricola { get; set; }
        public string MatricolaResponsabile { get; set; }
        public string Note { get; set; }
        public string OraInizioAttivita { get; set; }
        public string OraFineAttivita { get; set; }
        public string Titolo { get; set; }
        public string Uorg { get; set; }
        public Nullable<System.DateTime> DataCreazione { get; set; }
        public string idCeiton { get; set; }

    }
    public class GetFerieResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public string raw { get; set; }
        public List<GetFerieRow> datiDipendente { get; set; }
    }

    public class GetFerieRow
    {

        public string codiceEccezione { get; set; }
        public string descEccezione { get; set; }

        public float fruiti { get; set; }
        public float spettanti { get; set; }
        public float residui { get; set; }
        public float pianificati { get; set; }
    }
    public class AllineaGiornataResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public int[] IdRichiesteEliminateDB { get; set; }
    }
    public class GetEccezioniAmmesseResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public List<EccezioneAmmessa> EccezioniAmmesse { get; set; }
    }

    public class GetEccezioniComplessiveResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public List<myRaiData.L2D_ECCEZIONE> EccezioniComplessive { get; set; }
    }
    public class EccezioneAmmessa
    {
        public string codice { get; set; }
        public string descrizione { get; set; }
        public DateTime? dataInizioValidita { get; set; }
        public DateTime? dataFineValidita { get; set; }
        public Boolean? attiva { get; set; }

    }
    public class DateMinutiEccezione
    {
        public string eccezione { get; set; }
        public DateTime data { get; set; }
        public int minuti { get; set; }
        public string giorni { get; set; }
    }
    public class getOrarioResponse
    {
        public getOrarioResponse ( )
        {
            this.map = new List<string>( ){
                "CodiceOrario","2",
                "OrarioEntrataInizialeMin","4",
                "OrarioEntrataTeoricaMin","4",
                "OrarioEntrataFinaleMin","4",
                "OrarioFineTolleranzaMin","4",
                "OrarioInizioMensaMin","4",
                "OrarioFineMensaMin","4",
                "OrarioUscitaInizialeMin","4",
                "OrarioUscitaTeoricaMin","4",
                "OrarioUscitaFinaleMin","4",
                "OrarioPresenzaPrevistaMin","4",
                "TipologiaOraria","1",
                "LimiteGiornaliero","4",
                "CodiceRaggruppamentoOrario","2",
                "OrarioPresenzaMinimaMin","4"
                };
        }
        public string CodiceOrario { get; set; }

        public string OrarioEntrataIniziale { get; set; }
        public string OrarioEntrataInizialeMin { get; set; }

        public string OrarioEntrataTeorica { get; set; }
        public string OrarioEntrataTeoricaMin { get; set; }

        public string OrarioEntrataFinale { get; set; }
        public string OrarioEntrataFinaleMin { get; set; }

        public string OrarioFineTolleranza { get; set; }
        public string OrarioFineTolleranzaMin { get; set; }

        public string OrarioInizioMensa { get; set; }
        public string OrarioInizioMensaMin { get; set; }

        public string OrarioFineMensa { get; set; }
        public string OrarioFineMensaMin { get; set; }

        public string OrarioUscitaIniziale { get; set; }
        public string OrarioUscitaInizialeMin { get; set; }

        public string OrarioUscitaTeorica { get; set; }
        public string OrarioUscitaTeoricaMin { get; set; }

        public string OrarioUscitaFinale { get; set; }
        public string OrarioUscitaFinaleMin { get; set; }

        public string OrarioPresenzaPrevista { get; set; }
        public string OrarioPresenzaPrevistaMin { get; set; }

        public string TipologiaOraria { get; set; }

        public string LimiteGiornaliero { get; set; }

        public string CodiceRaggruppamentoOrario { get; set; }

        public string OrarioPresenzaMinima { get; set; }
        public string OrarioPresenzaMinimaMin { get; set; }

        public bool esito { get; set; }
        public string errore { get; set; }

        internal List<string> map { get; set; }


    }

    public class RigeneraPdfResponse
    {
        public Boolean esito { get; set; }
        public int IdPdf { get; set; }
        public string error { get; set; }
        public string raw { get; set; }
        public int ProgressivoStampa { get; set; }
    }

    public class GetSchedaPresenzeMeseResponse
    {
        public GetSchedaPresenzeMeseResponse ( )
        {
            Giorni = new List<InfoPresenza>( );
        }
        public List<InfoPresenza> Giorni { get; set; }

        public bool esito { get; set; }
        public string errore { get; set; }
        public string raw { get; set; }
    }
    public class InfoPresenza
    {
        public void GetCampi ( string giorno , DateTime dstart )
        {
            //Regex R = new Regex("(?<date>.{2})\\s(?<day>.{2})(?<cod>.{2})(?<in>.{5})(?<out>.{5})(?<ore>.{5})(?<notused1>.{1})" +
            //   "(?<m1>.{4})(?<notused2>.{1})(?<m2>.{4})(?<notused3>.{1})(?<m3>.{4})(?<mensa>.{1})(?<notused4>.{1})(?<stra1>.{6})(?<notused5>.{1})(?<stra2>.{6})(?<micro>.*)");

            Regex R = new Regex( "(?<date>.{2})\\s(?<day>.{2})(?<cod>.{2})(?<in>.{5})(?<out>.{5})(?<ore>.{5})" +
              "(?<m1>.{5})(?<m2>.{5})(?<m3>.{5})(?<mensa>.{1})(?<stra1>.{7})(?<stra2>.{7})(?<micro>.*)" );


            Match campi = R.Match( giorno );
            this.data = new DateTime( dstart.Year , dstart.Month , Convert.ToInt32( campi.Groups["date"].Value ) );
            this.day = campi.Groups["day"].Value;
            this.CodiceOrario = campi.Groups["cod"].Value;
            this.Ingresso = campi.Groups["in"].Value;
            this.Uscita = campi.Groups["out"].Value;
            this.OreServizio = campi.Groups["ore"].Value;
            this.OreServizio = campi.Groups["ore"].Value;
            MacroAssenze.Add( campi.Groups["m1"].Value.Trim( ) );
            MacroAssenze.Add( campi.Groups["m2"].Value.Trim( ) );
            MacroAssenze.Add( campi.Groups["m3"].Value.Trim( ) );
            this.Pasti = campi.Groups["mensa"].Value;
            Straordinari.Add( campi.Groups["stra1"].Value.Trim( ) );
            Straordinari.Add( campi.Groups["stra2"].Value.Trim( ) );
            string micro = campi.Groups["micro"].Value;

            Regex Rmicro = new Regex( "(?<micr>.{5})(?<notused2>.{1})(?<qua>.{6})" );
            MatchCollection campiMicro = Rmicro.Matches( micro );
            foreach ( Match ma in campiMicro )
            {
                if ( !string.IsNullOrWhiteSpace( ma.Groups["micr"].Value ) )
                {
                    MicroAssenza MA = new MicroAssenza( );
                    MA.nome = ma.Groups["micr"].Value.Trim( );
                    MA.quantita = ma.Groups["qua"].Value.Trim( );
                    MicroAssenze.Add( MA );
                }
            }
        }
        public InfoPresenza ( )
        {
            MacroAssenze = new List<string>( );
            Straordinari = new List<string>( );
            MicroAssenze = new List<MicroAssenza>( );
        }
        public DateTime data { get; set; }
        public string day { get; set; }
        public string CodiceOrario { get; set; }
        public string Ingresso { get; set; }
        public string Uscita { get; set; }
        public string OreServizio { get; set; }
        public List<string> MacroAssenze { get; set; }
        public string Pasti { get; set; }
        public List<string> Straordinari { get; set; }
        public List<MicroAssenza> MicroAssenze { get; set; }
    }

    public class MicroAssenza
    {
        public string nome { get; set; }
        public string quantita { get; set; }
    }

    public class GetTimbratureMeseResponse
    {
        public GetTimbratureMeseResponse ( )
        {
            Giorni = new List<InfoGiornata>( );
        }
        public List<InfoGiornata> Giorni { get; set; }

        public bool esito { get; set; }
        public string errore { get; set; }
        public string raw { get; set; }
    }

    public class InfoGiornata
    {
        public InfoGiornata ( )
        {
            Timbrature = new List<TimbraturaInOut>( );
        }
        public bool TimbraturaE { get; set; }
        public DateTime data { get; set; }
        public string CodiceOrario { get; set; }
        public List<TimbraturaInOut> Timbrature { get; set; }
    }

    public class TimbraturaInOut
    {
        public string Ingresso { get; set; }
        public string Uscita { get; set; }
    }

    /// <summary>
    /// Classe che rappresenta il modello utilizzato dal servizio per lo scambio delle informazioni
    /// relative allo stato debitorio di un utente
    /// </summary>
    public class SituazioneDebitoria
    {
        /// <summary>
        /// Nome della compagnia.
        /// Questo dato verrà rappresentato in grassetto nella tabella
        /// </summary>
        public string Descrizione { get; set; }

        /// <summary>
        /// Importo totale del debito
        /// </summary>
        public double Addebito { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MeseDa { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MeseA { get; set; }

        /// <summary>
        /// Importo della singola rata
        /// </summary>
        public double ImportoRata { get; set; }

        /// <summary>
        /// Numero di rate del contratto
        /// </summary>
        public int NumeroRate { get; set; }

        /// <summary>
        /// Importo rimanente
        /// </summary>
        public double ImportoRateResidue { get; set; }

        /// <summary>
        /// Numero di rate rimanenti
        /// </summary>
        public int NumeroRateResidue { get; set; }

        public int IntMeseDa { get; set; }
        public int IntMeseA { get; set; }
        public int AnnoDa { get; set; }
        public int AnnoA { get; set; }
    }

    public enum MesiEnum
    {
        [AmbientValue( "Gennaio" )]
        Gennaio = 1,
        [AmbientValue( "Febbraio" )]
        Febbraio = 2,
        [AmbientValue( "Marzo" )]
        Marzo = 3,
        [AmbientValue( "Aprile" )]
        Aprile = 4,
        [AmbientValue( "Maggio" )]
        Maggio = 5,
        [AmbientValue( "Giugno" )]
        Giugno = 6,
        [AmbientValue( "Luglio" )]
        Luglio = 7,
        [AmbientValue( "Agosto" )]
        Agosto = 8,
        [AmbientValue( "Settembre" )]
        Settembre = 9,
        [AmbientValue( "Ottobre" )]
        Ottobre = 10,
        [AmbientValue( "Novembre" )]
        Novembre = 11,
        [AmbientValue( "Dicembre" )]
        Dicembre = 12
    }

    /// <summary>
    /// Oggetto di ritorno del servizio
    /// </summary>
    public class SituazioneDebitoriaResponse
    {
        public SituazioneDebitoriaResponse ( )
        {
            List = new List<SituazioneDebitoria>( );
        }
        public List<SituazioneDebitoria> List { get; set; }

        public bool Esito { get; set; }
        public string Errore { get; set; }
        public string Risposta { get; set; }
    }

    /// <summary>
    /// Oggetto di ritorno del servizio
    /// </summary>
    public class SituazioneDebitoriaRequest
    {
        public string MatricolaRichiedente { get; set; }
        public string MatricolaRichiesta { get; set; }
    }

    /// <summary>
    /// Oggetto di ritorno del servizio
    /// </summary>
    public class TrasferteResponse
    {
        public TrasferteResponse ( )
        {
            this.ServiceResponse = string.Empty;
            this.Trasferte = new Trasferta( );
        }
        public Trasferta Trasferte { get; set; }
        public string ServiceResponse { get; set; }
        public bool Esito { get; set; }
        public string Errore { get; set; }
    }

    public class Trasferta
    {
        public Trasferta ( )
        {
            this.Viaggi = new List<Viaggio>( );
        }

        public TrasfertaCompetenzaDefinizione CompetenzaDefinizione { get; set; }
        public List<Viaggio> Viaggi { get; set; }
    }

    public class Viaggio
    {
        public string FoglioViaggio { get; set; }

        public string Data { get; set; }

        public string Descrizione { get; set; }

        public double SpesaPrevista { get; set; }

        public double Rimborso { get; set; }

        public double Anticipo { get; set; }

        public string Note { get; set; }

        public string AutorizzatoDa { get; set; }
    }

    public class TrasfertaCompetenzaDefinizione
    {
        public string MeseCompetenza { get; set; }

        public double DefPreviste { get; set; }

        public double DefRimborso { get; set; }

        public double DefAnticipo { get; set; }

        public double CRimborso { get; set; }

        public double CAnticipo { get; set; }
    }

    #region Info dipendente
    public class GetInfoDipendenteResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public List<InfoDipendente> InformazioniDipendente { get; set; }

        public GetInfoDipendenteResponse ( )
        {
            InformazioniDipendente = new List<InfoDipendente>( );
        }
    }

    public class NuoveInfoDipendenteResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
    }

    public class CambiaInfoDipendenteResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
    }

    public class EliminaInfoDipendenteResponse {
        public Boolean success { get; set; }
        public string error { get; set; }
    }

    public class GetTipologieInfoDipendenteResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public List<TipoInfoDipendente> listaTipologie { get; set; }

        public GetTipologieInfoDipendenteResponse ( )
        {
            listaTipologie = new List<TipoInfoDipendente>( );
        }
    }

    public class NuovaTipologiaInfoDipendente
    {
        public Boolean success { get; set; }
        public string error { get; set; }
    }

    public class InfoDipendente
    {

        public int id { get; set; }
        public string matricola { get; set; }
        public int idInfoDipendenteTipologia { get; set; }
        public string valore { get; set; }
        public System.DateTime dataInizioInfo { get; set; }
        public Nullable<System.DateTime> dataFineInfo { get; set; }
        public string note { get; set; }
        public string info { get; set; }
        public System.DateTime dataInizioValiditaTipologia { get; set; }
        public Nullable<System.DateTime> dataFineValiditaTipologia { get; set; }
        public string noteTipologia { get; set; }
        public string tipoValore { get; set; }

        public InfoDipendente ( )
        {
            dataFineValiditaTipologia = DateTime.Now;
            dataFineInfo = DateTime.Now;
        }
    }

    public class TipoInfoDipendente
    {
        public int id { get; set; }
        public string nomeTipo { get; set; }
        public System.DateTime dataInizioTipo { get; set; }
        public Nullable<System.DateTime> dataFineTipo { get; set; }
        public string noteTipo { get; set; }
        public string tipoValore { get; set; }

    }
    #endregion

    #region sedi
    public class GetSettimanaSedi
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public List<SedeGappSettimana> listaSedi { get; set; }
        public GetSettimanaSedi ( ) {
            listaSedi = new List<SedeGappSettimana>( );
        }
    }
    public class SedeGappSettimana
    {
        public int skySedeGapp { get; set; }
        public string codSedeGapp { get; set; }
        public string descSedeGapp { get; set; }
        public string codRsu { get; set; }
        public string descRsu { get; set; }
        public Nullable<System.DateTime> dataInizioValidita { get; set; }
        public Nullable<System.DateTime> dataFineValidita { get; set; }
        public string flagIvt { get; set; }
        public string flagPresenzaSirio { get; set; }
        public string minimoCar { get; set; }
        public string codServCont { get; set; }
        public string codSede { get; set; }
        public Nullable<System.DateTime> partenzaFase2 { get; set; }
        public Nullable<System.DateTime> partenzaFase3 { get; set; }
        public Nullable<System.DateTime> scadenza { get; set; }
        public string flgUltimo { get; set; }
        public Nullable<System.DateTime> dataInizioVal { get; set; }
        public System.DateTime dataIns { get; set; }
        public Nullable<System.DateTime> dataFineVal { get; set; }
        public Nullable<System.DateTime> dataAgg { get; set; }
        public Nullable<System.DateTime> dataElim { get; set; }
        public Nullable<int> giornoInizioSettimana { get; set; }
        public Boolean sedePresente { get; set; }

        public SedeGappSettimana ( ) {
            codSedeGapp = "";
            sedePresente = false;
            giornoInizioSettimana = 1;
        }
        public SedeGappSettimana ( string codiceSedeGapp ) {
            codSedeGapp = codiceSedeGapp;
            sedePresente = false;
            giornoInizioSettimana = 1;
        }
        public SedeGappSettimana ( string codiceSedeGapp , Boolean presente )
        {
            codSedeGapp = codiceSedeGapp;
            sedePresente = presente;
            giornoInizioSettimana = 1;
        }
    }
    public class NuovaSedeGappSettimana {
        public Boolean success { get; set; }
        public string error { get; set; }
    }
    #endregion

    #region EccezioniMalattia
    /// <summary>
    /// Oggetto di ritorno del servizio di InserisciEccezione
    /// </summary>
    public class InserisciNotaSegreteriaResponse
    {
        /// <summary>
        /// Esito della richiesta di inserimento dell'eccezione
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Eccezione inserita
        /// </summary>
        public MyRai_Note_Da_Segreteria Nota { get; set; }
    }

    /// <summary>
    /// Oggetto restituito dal metodo AggiornaEccezione
    /// </summary>
    public class AggiornaNotaSegreteriaResponse
    {
        /// <summary>
        /// Esito della richiesta di aggiornamento dei dati dell'eccezione
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Eccezione inserita
        /// </summary>
        public MyRai_Note_Da_Segreteria Nota { get; set; }
    }

    /// <summary>
    /// Oggetto restituito dal metodo RimuoviEccezione
    /// </summary>
    public class RimuoviNotaSegreteriaResponse
    {
        /// <summary>
        /// Esito della richiesta di aggiornamento dei dati dell'eccezione
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }
    }

    /// <summary>
    /// Oggetto di ritorno del servizio di NotaSegreteria
    /// </summary>
    public class NotaSegreteriaResponse
    {
        /// <summary>
        /// Esito della richiesta di reperimento delle note da segreteria
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Note inserite
        /// </summary>
        public List<MyRai_Note_Da_Segreteria> Note { get; set; }
    }
    #endregion

    #region Visualizzazione giornate da segreteria

    public class VisualizzazioneGiornataResponse
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Dati aggiornati
        /// </summary>
        public MyRai_Visualizzazione_Giornate_Da_Segreteria_Custom VisualizzazioneGiornata { get; set; }
    }

    public partial class MyRai_Visualizzazione_Giornate_Da_Segreteria_Custom
    {
        public int Id { get; set; }
        public string Matricola { get; set; }
        public int? IdRichiesta { get; set; }
        public bool Visualizzato { get; set; }
        public string MatricolaVisualizzatore { get; set; }
        public string UtenteVisualizzatore { get; set; }
        public System.DateTime DataCreazione { get; set; }
        public System.DateTime DataUltimoAccesso { get; set; }
        public System.DateTime DataRichiesta { get; set; }
        public bool InApprovazione { get; set; }
        public bool Approvate { get; set; }
    }

    public class StatoEccezioniGiornateResponse
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Dati aggiornati
        /// </summary>
        public List<MyRai_StatoEccezioniGiornate_Custom> StatoEccezioniGiornate { get; set; }
    }

    public partial class MyRai_StatoEccezioniGiornate_Custom
    {
        public System.DateTime DataEccezione { get; set; }
        public bool InApprovazione { get; set; }
        public bool Approvate { get; set; }
        public bool ApprovateTipo1 { get; set; }
        public bool Visualizzato { get; set; }
        public string CodiceOrario { get; set; }
        /// <summary>
        /// Se true per quella data non ci sono eccezioni
        /// </summary>
        public bool Empty { get; set; }
        public string Matricola { get; set; }
        public MyRai_Visualizzazione_Giornate_Da_Segreteria DettaglioVisualizzazione { get; set; }
    }

    public class MyRai_Visualizzazione_Giornate_Da_Segreteria_Report
    {
        public MyRai_Visualizzazione_Giornate_Da_Segreteria_Report ( )
        {
            this.Elenco = new List<MyRai_Visualizzazione_Giornate_Da_Segreteria_Custom>( );
        }

        public string Matricola { get; set; }

        public List<MyRai_Visualizzazione_Giornate_Da_Segreteria_Custom> Elenco { get; set; }
    }

    public class VisualizzazioneGiornataResponse_Singola
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Dati aggiornati
        /// </summary>
        public MyRai_Visualizzazione_Giornate_Da_Segreteria_Report VisualizzazioniGiornata { get; set; }
    }

    public class VisualizzazioniGiornataResponse_Elenco
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Dati aggiornati
        /// </summary>
        public List<MyRai_Visualizzazione_Giornate_Da_Segreteria_Custom> VisualizzazioniGiornate { get; set; }
    }

    #endregion


    #region NoteRichieste
    public class GetNotaRichiestaResponse
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Elenco delle note associate
        /// </summary>
        public MyRai_Note_Richieste Nota { get; set; }
    }

    public class GetNoteRichiesteResponse
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Elenco delle note associate
        /// </summary>
        public List<MyRai_Note_Richieste> Note { get; set; }
    }

    public class InserisciNotaRichiestaResponse
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Nota inserita
        /// </summary>
        public MyRai_Note_Richieste Nota { get; set; }
    }

    public class ModificaNotaRichiestaResponse
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Nota modificata
        /// </summary>
        public MyRai_Note_Richieste Nota { get; set; }
    }

    public class EliminaNotaRichiestaResponse
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        public string Tipologia { get; set; }
    }

    public class SetLetturaResponse
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Nota modificata
        /// </summary>
        public MyRai_Note_Richieste Nota { get; set; }
    }

    public class Report_POH_ROH_Response
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Report_Item> Risposta { get; set; }
    }

    public class Report_Lista_Item
    {
        public DateTime Giorno { get; set; }
        public int Minuti { get; set; }
    }

    public class Report_EccezioniGiornalisti_Response
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Report_ItemGiornalisti> Risposta { get; set; }
    }

    public class Report_Lista_ItemGiornalisti_Day
    {
        public Report_Lista_ItemGiornalisti_Day ( )
        {
            this.Giorno = DateTime.Now;
            this.Minuti = "";
        }

        public DateTime Giorno { get; set; }
        public string Minuti { get; set; }
    }

    public class Report_Lista_ItemGiornalisti
    {
        public Report_Lista_ItemGiornalisti ( )
        {
            this.Giorni = new List<Report_Lista_ItemGiornalisti_Day>( );
        }

        public string Eccezione { get; set; }
        public List<Report_Lista_ItemGiornalisti_Day> Giorni { get; set; }
        public int Totale { get; set; }
        public string Minuti { get; set; }
    }

    public class Report_ItemGiornalisti
    {
        public Report_ItemGiornalisti ( )
        {
            this.ListaItemGiornalisti = new List<Report_Lista_ItemGiornalisti>( );
        }

        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public List<Report_Lista_ItemGiornalisti> ListaItemGiornalisti { get; set; }
    }

    public class ItemCarMagPresenza
    {
        public int Settimana { get; set; }
        public string CAR { get; set; }
        public string MP { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
    }

    public class Report_Item
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public List<Report_Lista_Item> ListaPOH { get; set; }
        public List<Report_Lista_Item> ListaROH { get; set; }
        public List<Report_Lista_Item> ListaCAR { get; set; }
        public string TotaleOreLista1 { get; set; }
        public string TotaleOreLista2 { get; set; }
        public string TotaleOreLista3 { get; set; }
        public string Saldo { get; set; }
        public string SaldoPOHPrecedente { get; set; }
        public int NumeroOccorrenze { get; set; }
        public int NumeroOccorrenze20 { get; set; }
        public int NumeroOccorrenze22 { get; set; }
        public int NumeroOccorrenze25 { get; set; }
        public List<Report_Lista_Item> ListaSTR { get; set; }
        public List<Report_Lista_Item> ListaSTRF { get; set; }
        public List<Report_Lista_Item> ListaRE20 { get; set; }
        public List<Report_Lista_Item> ListaRE22 { get; set; }
        public List<Report_Lista_Item> ListaRE25 { get; set; }
        public List<ItemCarMagPresenza> ListaCarMp { get; set; }
        public List<Report_Lista_Item> ListaSTSE { get; set; }
    }

    public enum Quadratura
    {
        Giornaliera,
        Settimanale
    }

    public class Report_STSE_Response
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Oggetto risposta
        /// </summary>
        public List<Report_STSE_Risposta> Risposta { get; set; }
    }

    public class Report_STSE_Risposta: Report_STSE_Base_Item
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public List<Report_STSE_Settimana_Item> Settimane { get; set; }
        public List<Report_STSE_Giorno_Item> Giorni { get; set; }
    }

    public class Report_STSE_Settimana_Item: Report_STSE_Base_Item
    {
        public int NumeroSettimana { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
    }

    public class Report_STSE_Giorno_Item: Report_STSE_Base_Item
    {
        public DateTime Giorno { get; set; }
    }

    public class Report_STSE_Base_Item
    {
        public int Ore { get; set; }
        public int Minuti { get; set; }
        public string TempoISO8601 { get; set; }
    }

    public class Report_STR_STRF_Response
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Report_Item> Risposta { get; set; }
    }

    public class Report_Reperibilita_Response
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Report_Item> Risposta { get; set; }
    }

    public class Report_Carenza_MP_Response
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Report_Item> Risposta { get; set; }
    }

    public class EsitoMensa
    {
        public DateTime Data { get; set; }
        public string Luogo { get; set; }
    }

    public class GetServizioMensaResponse
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// Elenco delle note associate
        /// </summary>
        public EsitoMensa Response { get; set; }
    }

    #endregion

    public class GetPianoFeriePDFResponse
    {
        public string error { get; set; }
        public byte[] pdf { get; set; }
        public DateTime? DataApprovazione { get; set; }
        public DateTime? DataFirma { get; set; }
        public DateTime? DataStornoApprovazione { get; set; }
        public string MatricolaApprovazione { get; set; }
        public string MatricolaFirma { get; set; }
        public string MatricolaStorno { get; set; }
    }

    public class GetPianoFerieAnnoResponse
    {
        public GetPianoFerieAnnoResponse ( )
        {
            ListaSedi = new List<SedePianoFerie>( );
        }
        public List<SedePianoFerie> ListaSedi { get; set; }

        public myRaiData.MyRai_PianoFerieDefinizioni Definizioni { get; set; }
        public it.rai.servizi.svildigigappws.pianoFerieSedeGapp PianoFerieSedeGapp { get; set; }
        public string error { get; set; }
        public string[] MatricoleEsentate { get; set; }
    }
    public class SedePianoFerie
    {
        public SedePianoFerie ( )
        {
            Dipendenti = new List<Dip>( );
            StatoPianoFerie = new List<MyRai_PianoFerieSedi>( );
        }
        public string Sede { get; set; }
        public List<MyRai_PianoFerieSedi> StatoPianoFerie { get; set; }
        public List<Dip> Dipendenti { get; set; }


    }
    public class Dip
    {
        public String Matricola { get; set; }
        public List<MyRai_PianoFerieGiorni> Giorni { get; set; }
        public Accordi2020 Accordi { get; set; }
    }
    public class Accordi2020
    {
        public float DaFare { get; set; }
        public float EffettivamenteDaFare { get; set; }
        public float Fruite_11_31marzo { get; set; }
        public float RR_ArretratiDaInserire { get; set; }
        public float RF_ArretratiDaInserire { get; set; }
        public float FE_ArretratiDaInserire { get; set; }
        public decimal? DonateDaFoglioExcel { get; set; }
        public float FECE_MRCE_MNCE_gapp { get; set; }
    }

    public class GetModuloDetassazioneResponse
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// Stringa passata a CICS
        /// </summary>
        public string InputCics { get; set; }

        /// <summary>
        /// Stringa di output di CICS
        /// </summary>
        public string OutputCics { get; set; }
    }

    public class ResetModuloDetassazioneItem
    {
        /// <summary>
        /// Stringa passata a CICS
        /// </summary>
        public string InputCics { get; set; }

        /// <summary>
        /// Stringa di output di CICS
        /// </summary>
        public string OutputCics { get; set; }
    }

    public class ResetModuloDetassazioneResponse
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<ResetModuloDetassazioneItem> Response { get; set; }
    }

    public class SetSceltaDetassazioneResponse
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// Stringa passata a CICS
        /// </summary>
        public string InputCics { get; set; }

        /// <summary>
        /// Stringa di output di CICS
        /// </summary>
        public string OutputCics { get; set; }
    }

    public class Report_AR20_Response
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<DateTime> Risposta { get; set; }
    }

    public class ConteggioGiorniConsecutivi_Response
    {
        /// <summary>
        /// Esito della richiesta
        /// </summary>
        public bool Esito { get; set; }

        /// <summary>
        /// Eventuale errore
        /// </summary>
        public string Errore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<ConteggioGiorniConsecutivi> Risposta { get; set; }

        public string InputCics { get; set; }

        public string OutputCics { get; set; }
    }

    public class ConteggioGiorniConsecutivi
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string TipoDipendente { get; set; }
        public List<DettaglioConteggioGiorniConsecutivi> Giorni { get; set; }
    }

    public class DettaglioConteggioGiorniConsecutivi
    {
        public DateTime Giorno { get; set; }
        public bool Festivo { get; set; }
        public int ConteggioOccorrenza { get; set; }
        public string Info { get; set; }
        public string Descrizione { get; set; }
    }

    public class InfoVersion
    {
        public string NomeFile { get; set; }
        public DateTime DataModifica { get; set; }
        public long Dimensione { get; set; }
    }

    public class VersionResponse
    {
        public bool Esito { get; set; }
        public string Errore { get; set; }
        public List<InfoVersion> DLLFiles { get; set; }
    }

    public class ApprovatoriProduzioneItem
    {
        public int Id { get; set; }
        public string Titolo { get; set; }
        public string Approvatore { get; set; }
    }

    public class GetApprovatoriProduzioneResponse: ServiceResponseBase
    {
        public List<ApprovatoriProduzioneItem> Approvatori { get; set; }
    }

    public class SetApprovatoreProduzioneResponse: ServiceResponseBase
    {
        public ApprovatoriProduzioneItem Item { get; set; }
    }

    public class GetApprovatoreProduzioneResponse: ServiceResponseBase
    {
        public List<ApprovatoriProduzioneItem> Items { get; set; }
    }

    public class ServiceResponseBase
    {
        public bool Esito { get; set; }
        public string Errore { get; set; }
    }

    public class GetContatoriEccezioniResponse : ServiceResponseBase
    {
        public List<GetContatoriEccezioniItem> ContatoriEccezioni { get; set; }
    }

    public class GetContatoriEccezioniItem : ServiceResponseBase
    {
        public string CodiceEccezione { get; set; }
        public string DatoOriginale { get; set; }
        public string Totale { get; set; }
        public string Formato { get; set; }
        public string Massimale { get; set; }
        public string Raw { get; set; }
        public GiornoAnalisiEccezione[] GiorniSingoli { get; set; }
    }

    public class InserisciTimbraturaResponse: ServiceResponseBase
    {
        public string Raw { get; set; }
    }
}