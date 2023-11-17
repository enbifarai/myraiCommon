using System;
using System.Collections.Generic;

using MyRaiServiceInterface.it.rai.servizi.digigappws;
using myRaiHelper;
using myRaiData;

namespace myRaiCommonModel
{

    public class FerieDipendente
    {
        private float _Residue;
        public float Residue { get { return CommonHelper.Tronca2Dec(_Residue); } set { _Residue = value; } }

        private float _Spettanti;
        public float Spettanti { get { return CommonHelper.Tronca2Dec(_Spettanti); } set { _Spettanti = value; } }

        private float _Usufruite;
        public float Usufruite { get { return CommonHelper.Tronca2Dec(_Usufruite); } set { _Usufruite = value; } }

        private float _Pianificate;
        public float Pianificate { get { return CommonHelper.Tronca2Dec(_Pianificate); } set { _Pianificate = value; } }

        private float _AnnoPrec;
        public float AnnoPrec { get { return CommonHelper.Tronca2Dec(_AnnoPrec); } set { _AnnoPrec = value; } }

        private float _Ceduti;
        public float Ceduti { get { return CommonHelper.Tronca2Dec( _Ceduti ); } set { _Ceduti = value; } }

        private float _Donati;
        public float Donati { get { return CommonHelper.Tronca2Dec( _Donati ); } set { _Donati = value; } }

        private float _Ricevuti;
        public float Ricevuti { get { return CommonHelper.Tronca2Dec( _Ricevuti ); } set { _Ricevuti = value; } }

        private float _InScadenza;
        public float InScadenza { get { return CommonHelper.Tronca2Dec(_InScadenza); } set { _InScadenza = value; } }

        public Giornata[] GiorniFerie { get; set; }

        public List<MNDettaglioScadenza> MNScadenze { get; set; }

        public bool IsGiornalista { get; set; }
    }

    public class MNDettaglioScadenza
    {
        public string Matricola { get; set; }
        public DateTime DataEccezione { get; set; }
        public DateTime DataScadenza1 { get; set; }
        public DateTime DataScadenza2 { get; set; }
    }

    public class PermessiExFest
    {
        private float _Residue;
        public float Residue { get { return CommonHelper.Tronca2Dec(_Residue); } set { _Residue = value; } }

        private float _Spettanti;
        public float Spettanti { get { return CommonHelper.Tronca2Dec(_Spettanti); } set { _Spettanti = value; } }

        private float _Usufruite;
        public float Usufruite { get { return CommonHelper.Tronca2Dec(_Usufruite); } set { _Usufruite = value; } }

        private float _Pianificate;
        public float Pianificate { get { return CommonHelper.Tronca2Dec(_Pianificate); } set { _Pianificate = value; } }

        private float _AnnoPrec;
        public float AnnoPrec { get { return CommonHelper.Tronca2Dec(_AnnoPrec); } set { _AnnoPrec = value; } }

        public Giornata[] GiorniFerie { get; set; }
    }
    public class PermessiRetr
    {
        private float _Residue;
        public float Residue { get { return CommonHelper.Tronca2Dec(_Residue); } set { _Residue = value; } }

        private float _Spettanti;
        public float Spettanti { get { return CommonHelper.Tronca2Dec(_Spettanti); } set { _Spettanti = value; } }

        private float _Usufruite;
        public float Usufruite { get { return CommonHelper.Tronca2Dec(_Usufruite); } set { _Usufruite = value; } }

        private float _Pianificate;
        public float Pianificate { get { return CommonHelper.Tronca2Dec(_Pianificate); } set { _Pianificate = value; } }

        private float _AnnoPrec;
        public float AnnoPrec { get { return CommonHelper.Tronca2Dec(_AnnoPrec); } set { _AnnoPrec = value; } }

        public Giornata[] GiorniFerie { get; set; }
    }
    public class CalendarioFerie
    {
        public CalendarioFerie()
        {
            PianoFerieDip = new PianoFerieDipendente();
            PianoFerieDip.GiorniPianoFerie = new List<myRaiData.MyRai_PianoFerieGiorni>();
            DateToBeGray = new List<DateTime>();
            HaEstensioneMarzo = false;// myRaiCommonTasks.CommonTasks.Richiede2021(CommonManager.GetCurrentUserMatricola());
        }
        public bool IsSedeSenzaVincoli { get; set; }
        public bool OltreSogliaModifica { get; set; }
        public string CodiceOrario1 { get; set; }
        public string CodiceOrario2 { get; set; }
        public DateTime? DataFineLavoro = UtenteHelper.Data_Fine_Rapporto_lavorativo();
        public bool IsMazziniDatesCoveredByC1 { get; set; }
        public bool IsMazziniDatesCoveredByC2 { get; set; }
        public float  NonPartecipantiRaggiungimentoPercheFruiteMarzo { get; set; }
        public MyRai_ArretratiExcel2019 ItemFoglioExcelArretrati { get; set; }
        public float RRperFoglioExcelArretrati { get; set; }
        public float RFperFoglioExcelArretrati { get; set; }
        public float FEperFoglioExcelArretrati { get; set; }
        public float TOTALEperFoglioExcelArretrati { get; set; }

        public int Mese { get; set; }
        public int Anno { get; set; }
        public string MeseCorrente { get; set; }
        public Boolean ShowPreviousButton { get; set; }
        public Boolean ShowNextButton { get; set; }
        public int MeseNext { get; set; }
        public int AnnoNext { get; set; }
        public int MesePrev { get; set; }
        public int AnnoPrev { get; set; }
        public List<TipoPermessoFerieUsato> tipiGiornataSel { get; set; }
        public List<CalendarioDay> DaysShowed { get; set; }

        public Ferie resocontoFerie { get; set; }

		/// <summary>
		/// Lista contentente giorno per giorno informazioni riguardanti
		/// il Visualizzato, se ci sono eccezioni in approvazione o se approvate
		/// </summary>
		public List<MyRaiServiceInterface.MyRaiServiceReference1.MyRai_StatoEccezioniGiornate_Custom> Giornate { get; set; }

		/// <summary>
		/// Elenco di date in cui l'utente ha delle evidenze
		/// </summary>
		public List<DateTime> GiornateDaEvidenziare { get; set; }


        public PianoFerieDipendente PianoFerieDip { get; set; }
        public DateTime DataChiusuraPianoFerie { get; set; }

        public DateTime? DataApprovazioneSedeDipendente { get; set; }
        public string ApprovatoreSedeDipendente { get; set; }
      
        public DateTime? DataApprovazionePfDipendente { get; set;
        }
        public DateTime? DataFirmaSedeDipendente { get; set; }
        public string ConvalidatoreSedeDipendente { get; set; }


        public List<DateTime> DateRR { get; set; }
        public List<DateTime> DateRN { get; set; }
        public List<DateTime> DateRF { get; set; }
        public List<DateTime> DatePX { get; set; }
        public List<DateTime> DateToBeGray { get; set; }
        public List<DateTime> DateFEM_FEP { get;  set; }
        public List<DateTime> DatePF { get;  set; }
        public List<DateTime> DatePR { get;  set; }
        public List<DateTime> DatePRX { get;  set; }
        public bool UtenteProduzione { get;  set; }
        public bool HaEstensioneMarzo { get;  set; }
        public int ArretratiRR15 { get; set; }
        public int ArretratiRN15 { get; set; }
        public int ArretratiFE15 { get; set; }
        public int ArretratiRRRN15 { get; set; }
        public int ArretratiEntroMarzoG { get; set; }
        public int ArretratiEntroDicembreG { get; set; }
        public bool IsTorinoCavalliCoveredByC { get; set; }
        public string CodiceOrarioTorinoCavalli1 { get; set; }
       

        //public int PF_PercentualeSuSpettanza { get; set; }
        //public int PF_PercentualeSuArretrati { get; set; }

       // public int PF_PercentualeSuTotale { get; set; }
        //public DateTime PF_DataLimite { get; set; }

        //public int PF_InteroSuSpettanza { get; set; }
       // public int PF_InteroSuArretrati { get; set; }
       // public int PF_Intero75percento { get; set; }

       // public float PF_TotaleSpettanza { get; set; }
       // public float PF_TotaleArretrati { get; set; }
        //public float PF_Residue { get; set; }

        //public List<myRaiData.MyRai_PianoFerieGiorni> GiorniPianoFerie { get; set; }
       // public int PF_GiaSalvatiTotali { get; set; }
        //public int PF_GiaSalvatiEntro30Settembre { get; set; }
    }
    public class PianoFerieDipendente
    {
        public int PF_PercentualeSuSpettanza { get; set; }
        public int PF_PercentualeSuArretrati { get; set; }
        public int PF_PercentualeSuTotale { get; set; }
        public DateTime PF_DataLimite { get; set; }
        public float PF_InteroSuSpettanza { get; set; }
        public int PF_InteroSuArretrati { get; set; }
        public int PF_Intero75percento { get; set; }
        public float PF_TotaleSpettanza { get; set; }
        public float PF_TotaleArretrati { get; set; }
        public float PF_Residue { get; set; }
        public float PF_GiaSalvatiTotali { get; set; }
        public float PF_GiaSalvatiEntro30Settembre { get; set; }
        public float PF_GiaSalvatiEntro31Ottobre { get; set; }
        public float PF_GiaSalvatiEntro31Ottobre_RR { get; set; }
        public float PF_GiaSalvatiEntro31Ottobre_RF { get; set; }
        public float PF_GiaSalvatiEntro31Ottobre_FE { get; set; }
        public List<myRaiData.MyRai_PianoFerieGiorni> GiorniPianoFerie { get; set; }
        public bool ArretratiSoglia1_Ok { get; set; }
        public bool ArretratiSoglia2_Ok { get; set; }
        public bool ArretratiSoglia3_Ok { get; set; }
        public int ArretratiCumulativiPercentuale { get; set; }
        public myRaiData.MyRai_PianoFerie PianoFerieDB { get; set; }

        public StatiPianoFerie StatoPianoFerie { get; set; }
        public myRaiData.MyRai_PianoFerieDefinizioni Definizioni { get; set; }
        public bool HaMezzaGiornata { get;  set; }

        public enum StatiPianoFerie
        {
            In_Compilazione,
            In_Approvazione,
            Approvato
        }

    }


    public class CalendarioDay
    {
        public DateTime giorno { get; set; }
        public Boolean isCurrentMonth { get; set; }
        public string  Frazione { get; set; }
        public string tipoFeriePermesso { get; set; }
        public string tipoGiornata { get; set; }
        public string friendly { get; set; }

    }
    public class GraficiFerieModel
    {
        public pianoFerie PianoFerie { get; set; }
		public bool IsGiornalista { get; set; }
		public int TotalePXC { get; set; }
		public float TotaleMN { get; set; }
        public Boolean HaAssenzeIngiustificate { get; set; }

        public Boolean HaGiorniCarenza { get; set; }
    }

    public class TipoPermessoFerieUsato {
        public string sigla { get; set; }
        public string siglaSemplice { get; set; }
        public string tipoDesc { get; set; }
        public FerieDipendente resoconto { get; set; }
    }

    public class EccezioneContatore
    {

    }
}