using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiCommonModel.DashboardResponsabile;
using myRaiCommonModel.ess;
using myRaiCommonModel.Gestionale;
using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;

namespace myRaiCommonModel
{

    //scrivania sezione 1
    public class SectionAlertModel
    {
        public Boolean IsPreview { get; set; }
        public List<AlertModel> Alerts { get; set; }

    }
    public class AlertModel
    {
        public string CifraPrincipale { get; set; }
        public string Titolo { get; set; }
        public string ClasseIcona { get; set; }
        public string ColoreClasseIcona { get; set; }
        public string TraParentesi { get; set; }
        public string TestoRosso { get; set; }
        public string TestoPulsante { get; set; }
        public string HrefPulsante { get; set; }
        public int TipoAlert { get; set; }
        public bool Visibile { get; set; }
        public string AriaLabelSummary { get; set; }
        public string AriaLabelPulsante { get; set; }

        public string intro_datastep { get; set; }
        public string intro_dataintro { get; set; }
        public string WaitLabel { get; set; }

        public string IntestazioneWidget { get; set; }
    }

    //scrivania sezione 2
    public class SectionMetersModel
    {
        public SectionMetersModel()
        {
            this.DatiScostamento = null;
        }
        public CalendarioFerie CalendarioFerieModel { get; set; }
        public Boolean IsPreview { get; set; }
        public Boolean HaAssenzeIngiustificate { get; set; }
        public Boolean HaGiorniCarenza { get; set; }
        public Ferie ferie { get; set; }
        public string spettanzapg { get; set; }
        public pianoFerie pianoF { get; set; }
		public bool IsGiornalista { get; set; }
		public int TotalePXC { get; set; }
		public double TotaleMN { get; set; }
        public double FeCedute { get; set; }
        public double FeDonate { get; set; }
        public double MRCeduti { get; set; }
        public double MRDonati { get; set; }
        public bool PXCVisibili { get; set; }
        public bool MNVisibili { get; set; }
        public double MNAnnoPrecedente { get; set; }
        public DateTime? ScadenzaMNAnnoPrecedente { get; set; }
        public DipendenteScostamento DatiScostamento { get; set; }
    }

    //scrivania sezione 3
    public class SectionDayModel
    {
        public Boolean IsPreview { get; set; }
        public dayResponse DayResponse { get; set; }
        public List<MyRai_Notifiche> Notifiche { get; set; }
        public Boolean GappClosed { get; set; }
        public bool NotificaVisibile { get; set; }
        public bool AttivitaVisibile { get; set; }
        public bool CoseDaFareVisibile { get; set; }
        public bool BustaPagaVisibile { get; set; }
        public bool OrarioVisibile { get; set; }
        public bool TimbraturaVisibile { get; set; }
        public bool ScelteVisibile { get; set; }
        public bool WeekPlanVisibile { get; set; }

        public CalendarioFerie CalendarioModel { get; set; }

        public WeekPlan WeekPlan { get; set; }

        public Boolean AbilitatoGestionePianoFerie { get; set; }
        public Boolean PianoFerieVisible { get; set; }
        public Boolean EsentatoPianoFerie { get; set; }
        public MyRai_PianoFerie MyPianoFerie { get; set; }
        public MyRai_PianoFerieSedi MyPianoFerieSede { get; set; }

        public TrasferteDaRendicontareVM TrasfertePrecedenti { get; set; }

        public DetassazioneVM DetassazioneVM { get; set; }

        public ValutatoreEsternoContainer ValutatoreEsterno { get; set; }

        public Bonus100EVM Bonus100VM { get; set; }

        public WidgetModuloBox SmartWorkingWidget { get; set; }

        public WidgetModuloBox ProrogaSWWidget { get; set; }

        public WidgetModuloBox RinunciaWidget { get; set; }

        public WidgetModuloBox Incentivazione012021Widget { get; set; }
    }

    public class WidgetModuloBox_Azione
    {
        public string BottoneId { get; set; }
        public string TestoBottone { get; set; }
        public string UrlBottone { get; set; }
        public string UrlBottoneParametri { get; set; }
        public string Icona { get; set; }
        public string Funzione { get; set; }
    }

    public class WidgetModuloBox
    {
        public string WidgetId { get; set; }
        public string Titolo { get; set; }
        public string Sottotitolo { get; set; }
        public string Sottotitolo2 { get; set; }
        public string Icona { get; set; }
        public List<WidgetModuloBox_Azione> Bottoni { get; set; }
        public bool HaDiritto { get; set; }
        public string Messaggio { get; set; }
        public int Anno { get; set; }
        public bool GiaScelto { get; set; }
        public string Scelta { get; set; }
        public DateTime? DataCompilazione { get; set; }
        public DateTime? DataLettura { get; set; }
        public bool IsPreview { get; set; }
    }

    public class Bonus100EVM
    {
        public bool HaDiritto { get; set; }
        public string Messaggio { get; set; }
        public int Anno { get; set; }
        public bool GiaScelto { get; set; }
        public string Scelta { get; set; }
        public DateTime? DataCompilazione { get; set; }
    }

    public class DetassazioneVM
    {
        public bool HaDiritto { get; set; }
        public string CodiceModello { get; set; }
        public string Messaggio { get; set; }
        public int Anno { get; set; }
        public string CodiceDetassazione { get; set; }
        public bool GiaScelto { get; set; }
    }

    //scrivania sezione 4
    public class SituazioniEAnagraficaModel
    {
        public Boolean IsPreview { get; set; }
        public List<BoxSituazioneModel> Boxes { get; set; }
        public bool BustaPagaVisibile { get; set; }
        public Boolean GappClosed { get; set; }
        public bool Visibile { get; set; }

    }

    //public class WeekPlan
    //{
    //    public WeekPlan ( )
    //    {
    //        Days = new List<DayPlan>( );
    //    }
    //    public string Matricola { get; set; }
    //    public List<DayPlan> Days { get; set; }
    //}

    public class DayPlan
    {
        public DayPlan ( )
        {
            Activities = new List<DayActivity>( );
        }
        public DateTime Date { get; set; }
        public List<DayActivity> Activities { get; set; }

        public override string ToString ( )
        {
            return String.Format( "{0:dd/MM/yyyy} - {1} attività" , Date , Activities.Count );
        }
    }

    //public class DayActivity
    //{
    //    public string Location { get; set; }
    //    public string Title { get; set; }
    //    public string Schedule { get; set; }
    //    public TimeSpan StartTime { get; set; }
    //    public TimeSpan EndTime { get; set; }
    //    public string MainActivity { get; set; }
    //    public string SecActivity { get; set; }
    //    public string DoneActivity { get; set; }
    //    public string Manager { get; set; }
    //    public string Matricola { get; set; }
    //    public DateTime Date { get; set; }
    //    public void SetStartTime ( string startTimeStr )
    //    {
    //        TimeSpan temp;
    //        TimeSpan.TryParseExact( startTimeStr , "g" , null , out temp );
    //        StartTime = temp;
    //    }
    //    public void SetEndTime ( string endTimeStr )
    //    {
    //        TimeSpan temp;
    //        TimeSpan.TryParseExact( endTimeStr , "g" , null , out temp );
    //        EndTime = temp;
    //    }

    //    public string idAttivita { get; set; }
    //    public string Note { get; set; }
    //    public string Uorg { get; set; }

    //    public int Eccezioni { get; set; }
    //}
}