using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiData;

namespace myRaiCommonModel
{
    public class InfoGiornataModel
    {
        public string orarioIngressoTurno { get; set; }
        public string orarioUscitaTurno { get; set; }
        public string codOrarioPrevisto { get; set; }
        public string descOrarioPrevisto { get; set; }
        public string codOrarioReale { get; set; }
        public string descOrarioReale { get; set; }
        public string minutiMensa { get; set; }
        public string PresenzaMensa { get; set; }

        public string Carenza { get; set; }
        public int CarenzaMinuti { get; set; }
        public string MaggiorPresenza { get; set; }
        public string PrevistaPresenza { get; set; }

        public DateTime DataGiornata { get; set; }
        public DateTime? MensaDataOra { get; set; }
        public string MensaSede { get; set; }

        public bool RICO { get; set; }
		public string CodiceRico { get; set; }
		public string RistoranteConvensionato { get; set; }
        public string MaxDurataURH { get; set; }
		public string TipoDip { get; set; }

        public string PrimaTimbratura { get; set; }
        public string UltimaTimbratura { get; set; }
        public MyRaiServiceInterface.MyRaiServiceReference1.getOrarioResponse OrarioPrevisto { get; set; }
        public MyRaiServiceInterface.MyRaiServiceReference1.getOrarioResponse OrarioReale { get; set; }
        public string giornoSettimana { get; set; }

        public string intervallo_mensa { get; set; }
        public string intervallo_mensa_serale { get; set; }

        public int intervallo_mensa_pranzo_minuti_from { get; set; }
        public int intervallo_mensa_serale_minuti_from { get; set; }
        public int intervallo_mensa_pranzo_minuti_to { get; set; }
        public int intervallo_mensa_serale_minuti_to { get; set; }

        public int BonusCarenza { get; set; }
        public bool BilancioPositivoPerQuadraturaSettimanale { get; set; }
        public string MensaDisponibilePC { get; set; }
        public string TipoQuadratura  { get; set; }
        public string POHdaRecuperare { get; set; }

		/// <summary>
		/// Lista contenente le note associate alla giornata
		/// </summary>
		public List<MyRai_Note_Richieste_EXT> Note { get; set; }
		public string MatricolaUtente { get; set; }
		public string NomeUtente { get; set; }

        public bool CeitonAttivitaObbligatoriaPerSede { get; set; }
        public string EccezioniRichiedentiCeiton { get; set; }

        public bool HaSmap1min { get; set; }

        public bool ApertaDaResp { get; set; }

        public bool GiornataChiusa { get; set; }
        public bool NascondiMessaggio { get;  set; }
    }

    public class MyRai_Note_Richieste_EXT : MyRai_Note_Richieste
    {
        public MyRai_Note_Richieste_EXT()
        {
            this.Immagine = "";
        }

        public MyRai_Note_Richieste_EXT(MyRai_Note_Richieste w)
        {
            this.DataCreazione = w.DataCreazione;
            this.DataGiornata = w.DataGiornata;
            this.DataLettura = w.DataLettura;
            this.DataUltimaModifica = w.DataUltimaModifica;
            this.DescrizioneMittente = w.DescrizioneMittente;
            this.DescrizioneVisualizzatore = w.DescrizioneVisualizzatore;
            this.Destinatario = w.Destinatario;
            this.Id = w.Id;
            this.Immagine = "";
            this.Messaggio = w.Messaggio;
            this.Mittente = w.Mittente;
            this.SedeGapp = w.SedeGapp;
            this.Visualizzatore = w.Visualizzatore;
        }

        public string Immagine { get; set; }
    }
}