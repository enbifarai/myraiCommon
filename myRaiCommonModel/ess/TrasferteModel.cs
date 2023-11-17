using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using MyRaiServiceInterface.MyRaiServiceReference1;

namespace myRaiCommonModel.ess
{
    public enum TrasferteMacroStato
{
        Aperte,
        Concluse
    }

    public class TrasferteDaRendicontareVM
    {
        public int Conteggio { get; set; }
        public string Messaggio { get; set; }
        public string Url { get; set; }
    }

	public class TrasferteViewModel
	{
		public TrasferteViewModel ()
		{
			this.Data = new Trasferta();
		}

		public Trasferta Data { get; set; }

        public List<StatoTrasferta> Stati { get; set; }

        public TrasferteMacroStato MacroStato { get; set; }
        public bool HasNext { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
	}

	public class RiepilogoTrasferteVM
	{
		public RiepilogoTrasferteVM ()
		{
		}

		public string MeseCompetenza { get; set; }
		public double TotaleAnticipi { get; set; }
		public double TotaleRimborsi { get; set; }
	}

	public class RiepilogoTrasferteInDefinizioneVM
	{
		public double SpesePreviste { get; set; }
		public double Rimborsi { get; set; }
		public double Anticipi { get; set; }
	}

	/// <summary>
	/// Classe che rappresenta l'oggetto Foglio Viaggio
	/// </summary>
	public partial class FoglioViaggio
	{
		public string NUM_FOG { get; set; }
		public string MATRICOLA_DP { get; set; }
		public string NOME { get; set; }
		public string COD_SEDE { get; set; }
		public string COD_SERV_CONT { get; set; }
		public string COD_CATEGORIA { get; set; }
		public string COD_SEZIONE { get; set; }
		public DateTime DATA_PARTENZA { get; set; }
		public DateTime DATA_ARRIVO { get; set; }
		public string COD_UORG { get; set; }
		public string MATRIC_SPETTACOLO { get; set; }
		public string COD_DF { get; set; }
		public string ITINERARIO { get; set; }
		public string SCOPO { get; set; }
		public int NUM_GIORNI { get; set; }
		public decimal ANTICIPI { get; set; }
		public string STATO { get; set; }
		public string COD_MANSIONE { get; set; }
		public decimal SPESE_PREV { get; set; }
		public string COD_UORG_EMISSIONE { get; set; }
		public string ESTERO { get; set; }
		public string FLG_ISUD { get; set; }
		public string COD_GRANDI_EVENTI { get; set; }
		public DateTime DATA_ELABORAZIONE { get; set; }
		public string MESE_LIQUIDAZIONE { get; set; }
		public string CODICE_UTENTE { get; set; }
		public DateTime? DATA_NOTA_SPESE { get; set; }
		public string AUTORIZZATA_DA { get; set; }
	}

	public partial class StatoTrasferta
	{
		public string Cod_Stato { get; set; }
		public string Desc_Stato { get; set; }
	}

	public partial class GrandeEvento
	{
		public string Cod_Grandi_Eventi { get; set; }
		public string Desc_Grandi_Eventi { get; set; }
		public string Data_Inizio_Manifestazione { get; set; }
		public string Data_Fine_Manifestazione { get; set; }
		public string Testo { get; set; }
	}

	public partial class ItinerarioTrasferta
	{
		public string NUM_FOG { get; set; }
		public int NUM_TRAS { get; set; }
		public int NUM_RIGA { get; set; }
		public string DESTINAZIONE { get; set; }
		public string PROVINCIA { get; set; }
		public string CAP { get; set; }
		public DateTime DATA_ARRIVO { get; set; }
		public string COD_MEZZO_TRASP { get; set; }
		public decimal IMPORTO_BIGLIETTI_DIP { get; set; }
		public decimal IMPORTO_BIGLIETTI_RAI { get; set; }
		public string FLG_ISUD { get; set; }
		public DateTime DATA_ELABORAZIONE { get; set; }

        // Aggiunti a seguito della sincronizzazione del batch
        public string CodicePrenotazione { get; set; }
        public string Origine { get; set; }
        public string DestCitta { get; set; }
        public string CodiceMezzo { get; set; }
        public DateTime? DataPartenza { get; set; }

        public DateTime? DataArrivoFull { get; set; }
    }

	public partial class NotaSpeseTrasferta
	{
		public string NUM_FOG { get; set; }
		public DateTime DATA_INIZIO { get; set; }
		public DateTime DATA_FINE { get; set; }
		public decimal TOT_BIGLIETTI_DOC_NO_RAI { get; set; }
		public decimal TOT_BIGLIETTI_DOC_RAI { get; set; }
		public decimal TOT_TAXI_BUS { get; set; }
		public decimal TOT_PEDAGGI { get; set; }
		public decimal TOT_ALTRE_SPESE_DOC { get; set; }
		public decimal TOT_CARBUR { get; set; }
		public decimal TOT_PER_CONTO { get; set; }
		public decimal TOT_BIGLIETTI_SMARRITI { get; set; }
		public decimal TOT_ALTRE_SPESE_NO_DOC { get; set; }
		public decimal TOT_NOLO { get; set; }
		public decimal KM_AUTO { get; set; }
		public decimal TOT_FORFAIT { get; set; }
		public decimal TOT_PDL { get; set; }
		public int NUM_GIORNI{ get; set; }
		public string FLG_ISUD{ get; set; }
		public decimal TOT_COSTO_AZIENDALE { get; set; }
		public DateTime DATA_ELABORAZIONE{ get; set; }
	}

	public partial class AlbergoTrasferta
	{
		public string foglio_viaggio  { get; set; }
		public string progressivo { get; set; }
		public string stato { get; set; }
		public string gg_prenotati { get; set; }
		public DateTime data { get; set; }
		public decimal costo_effettivo { get; set; }
		public string tipo { get; set; }
		public decimal costo_teorico { get; set; }
		public string utente { get; set; }
		public DateTime data_log { get; set; }
		public string utente_canc { get; set; }
		public DateTime? data_canc { get; set; }
		public string citta { get; set; }
		public string denominazione { get; set; }
		public DateTime data_decorrenza { get; set; }
		public string multi { get; set; }
		public string prov { get; set; }
		public string telefono { get; set; }
		public string fax { get; set; }
		public string categ { get; set; }
		public decimal costo_giornaliero { get; set; }
		public decimal costo_dus { get; set; }
		public string cod_albergo { get; set; }
		public string indirizzo { get; set; }
		public string cap { get; set; }
		public string nom_rich { get; set; }
		public string tel_rich { get; set; }
		public string automatico { get; set; }
		public string autoriz { get; set; }
		public string FLG_ISUD { get; set; }
		public DateTime DATA_ELABORAZIONE { get; set; }
	}

	public partial class CustomJsonResponse
	{
		public CustomJsonResponse ()
		{
			Errore = false;
			Azione = string.Empty;
			Messaggio = string.Empty;
		}

		public bool Errore { get; set; }
		public string Messaggio { get; set; }
		public string Azione { get; set; }
	}

	public class DettaglioTrasfertaVM
	{
		public string FViaggio { get; set; }
		public FoglioViaggio FoglioViaggio { get; set; }
		public StatoTrasferta StatoTrasferta { get; set; }
		public GrandeEvento GrandeEvento { get; set; }
		public List<ItinerarioTrasferta> Itinerario { get; set; }
		public NotaSpeseTrasferta NotaSpeseTrasferta { get; set; }
		public List<AlbergoTrasferta> Alberghi { get; set; }
		public List<BigliettoRaiTrasferta> BigliettiRai { get; set; }
		public List<DiariaTrasferta> Diaria { get; set; }
		public decimal ResiduoNetto { get; set; }
		//public string UrlQuestionario { get; set; }
	}

	public class DiariaTrasfertaVM
	{
		public List<DiariaTrasferta> Diaria { get; set; }
		public DiariaTrasfertaEnum Tipo { get; set; }
	}

	public enum MezzoTrasportoEnum
	{
		[AmbientValue( "Aereo" )]
		AE,
		[AmbientValue( "Autovettura sociale" )]
		AS,
		[AmbientValue( "Autovettura privata" )]
		AU,
		[AmbientValue( "Nave, traghetto, piroscafo, ecc." )]
		NA,
		[AmbientValue( "Pullman, corriera e simili" )]
		PU,
		[AmbientValue( "Treno" )]
		TR,
		[AmbientValue( "Altro" )]
		AL
	}

	public partial class BigliettoRaiTrasferta
	{
		public string Numero_foglio_viaggio {get;set;}
		public string Numero_biglietto { get; set; }
		public DateTime Data_partenza_viaggio { get; set; }
		public string Progressivo { get; set; }
		public string Numero_foglio_viaggio_new { get; set; }
		public string Matricola_DP { get; set; }
		public string Itinerario { get; set; }
		public DateTime Data_emissione_biglietto { get; set; }
		public string Mezzo { get; set; }
		public string Codice_vettore { get; set; }
		public string Flag_estero { get; set; }
		public string Codice_tariffa { get; set; }
		public DateTime Data_fattura { get; set; }
		public string Numero_fattura { get; set; }
		public decimal Importo_fattura { get; set; }
		public decimal Importo_rimborso { get; set; }
		public DateTime Data_acquisizione { get; set; }
		public DateTime Data_elaborazione { get; set; }
	}

	public partial class DiariaTrasferta
	{
		public string NUM_FOG { get; set; }
		public int NUM_RIGA { get; set; }
		public DateTime DATA { get; set; }
		public decimal PASTO_PDL_1 { get; set; }
		public decimal PASTO_FOR_1 { get; set; }
		public decimal PASTO_PDL_2 { get; set; }
		public decimal PASTO_FOR_2 { get; set; }
		public decimal PERNOTTO_PDL { get; set; }
		public decimal PERNOTTO_FOR { get; set; }
		public decimal COLAZ_PDL { get; set; }
		public decimal COLAZ_FOR { get; set; }
		public decimal PICCOLE_SPESE { get; set; }
		public decimal INDENNITA_TRASFERTA { get; set; }
		public string PERNOTTO_DESC { get; set; }
		public string PERNOTTO_CAP { get; set; }
		public string FLG_ISUD { get; set; }
		public string FRUIZIONE_PRANZO { get; set; }
		public string FRUIZIONE_CENA { get; set; }
		public string FRUIZIONE_PERNOTTO { get; set; }
		public string FRUIZIONE_COLAZ { get; set; }
		public string DIRITTO_PRANZO { get; set; }
		public string DIRITTO_CENA { get; set; }
		public string DIRITTO_PERNOTTO { get; set; }
		public string DIRITTO_COLAZ { get; set; }
		public DateTime DATA_ELABORAZIONE { get; set; }
		public decimal PDL_SOGGETTO { get; set; }
		public decimal FORFAIT_SOGGETTO { get; set; }
	}

	public partial class Viaggio
	{
		public string Stato { get; set; }
	}

	public enum DiariaTrasfertaEnum
	{
		[AmbientValue( "Aereo" )]
		Complessivo,
		[AmbientValue( "Piè di lista" )]
		PieDiLista,
		[AmbientValue( "Forfait" )]
		Forfait
	}
}