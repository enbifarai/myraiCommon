using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRai.DataControllers;
using myRaiHelper;
using MyRaiServiceInterface.MyRaiServiceReference1;

namespace myRaiCommonModel.DashboardResponsabile
{
	public class DashboardResponsabileVM
	{
		public DashboardResponsabileVM ()
		{
			this.MeseRiferimento = 0;
			this.Items = new List<DashboardResponsabileItem>();
			this.WidgetId = this.GeneraGuid();
		}

		public string WidgetId { get; set; }
		public int MeseRiferimento { get; set; }
		public List<DashboardResponsabileItem> Items { get; set; }


		private string GeneraGuid ()
		{
			Guid id = Guid.NewGuid();

			string result = id.ToString().Replace( "\\", "" );

			result = result.Replace( "-", "" );
			result = result.Replace( "&", "" );
			result = result.Replace( "%", "" );

			return result;
		}
	}

	public class ItemCarenzaMaggiorPresenza
	{
		public int Settimana { get; set; }
		public string CAR { get; set; }
		public string MP { get; set; }
	}

	public class DashboardResponsabileItem
	{
		public string Nominativo { get; set; }
		public string Foto { get; set; }
		public List<UtenteDataControllerResult> ListaPOH { get; set; }
		public List<UtenteDataControllerResult> ListaROH { get; set; }
		public string TotaleOreLista1 { get; set; }
		public string TotaleOreLista2 { get; set; }
		public string TotaleOreLista3 { get; set; }
		public string Saldo { get; set; }
		public int NumeroOccorrenze { get; set; }
		public List<UtenteDataControllerResult> ListaSTR { get; set; }
		public List<UtenteDataControllerResult> ListaSTRF { get; set; }
		public List<UtenteDataControllerResult> ListaRE20 { get; set; }
		public List<UtenteDataControllerResult> ListaRE22 { get; set; }
		public List<UtenteDataControllerResult> ListaRE25 { get; set; }
		public List<ItemCarenzaMaggiorPresenza> ListaCarMp { get; set; }
	}

    public class WidgetVM
    {
        public WidgetVM()
        {
            this.MeseRiferimento = DateTime.Now.Month;
            this.ActionName = "";
            this.ViewName = "";
            this.Title = "";
            this.WidgetId = this.GeneraGuid( );
        }

        public string WidgetId { get; set; }
        public int MeseRiferimento { get; set; }
        public string ActionName { get; set; }
        public string ViewName { get; set; }
        public int DimensioneColonna { get; set; }
        public string Title { get; set; }

        private string GeneraGuid ( )
        {
            Guid id = Guid.NewGuid( );

            string result = id.ToString( ).Replace( "\\" , "" );

            result = result.Replace( "-" , "" );
            result = result.Replace( "&" , "" );
            result = result.Replace( "%" , "" );

            return result;
        }
    }

    public class WidgetRow
    {
        public WidgetRow()
        {
            this.Widgets = new List<WidgetVM>( );
        }

        public List<WidgetVM> Widgets { get; set; }
    }

    public class DashBoardResponsabileCustomVM
    {
        public DashBoardResponsabileCustomVM()
        {
            this.Rows = new List<WidgetRow>( );
        }

        public List<WidgetRow> Rows { get; set; }
    }

    public class PianificazioneAttivitaVM
    {
        //public DateTime DataPartenza { get; set; }
        //public ConteggioGiorniConsecutivi_Response Dati { get; set; }
        public List<Sede> SediGapp { get; set; }
    }

    public class StatoFerieStatsVM
    {        
        public string WidgetId { get; set; }
        public List<StatoFerieObj> Dati { get; set; }
    }

    public class StatoFerieVM
    {
        public StatoFerieVM ( )
        {
            this.WidgetId = "statoFerie";
            this.Anno = DateTime.Now.Year;
            this.Page = 1;
            this.Pianificato = new ParametriGrafico( );
            this.Effettivo = new ParametriGrafico( );
            this.CodiceSedeSelected = null;
            this.SediGapp = new List<SedeRepartiForSelect>( );
            this.ItemsInlist = 60;
            this.Action = "ChartStatoFerie";
            this.Dipendenti = new List<DipendenteScostamento>( );
            this.EtichetteGrafico = new List<string>( );
        }

        public string WidgetId { get; set; }
        public string CodiceSedeSelected { get; set; }
        public string DescrizioneSedeGapp { get; set; }
        public List<SedeRepartiForSelect> SediGapp { get; set; }
        public ParametriGrafico Pianificato { get; set; }
        public ParametriGrafico Effettivo { get; set; }
        public int Anno { get; set; }
        public int Page { get; set; }
        public int GiorniPianificatiAdOggi { get; set; }
        public int GiorniEffettiviAdOggi { get; set; }
        public int GiorniPianificatiTotali { get; set; }
        public int GiorniEffettiviTotali { get; set; }
        public int ItemsInlist { get; set; }
        public string Action { get; set; }
        public List<DipendenteScostamento> Dipendenti { get; set; }
        public List<string> EtichetteGrafico { get; set; }
        public List<int> PianificateGrafico { get; set; }
        public List<int> EffettiveGrafico { get; set; }
    }

    public class ParametriGrafico
    {
        public List<DateTime> Giorno { get; set; }
        public List<int> TotalePerGiorno { get; set; }

        public ParametriGrafico()
        {
            this.Giorno = new List<DateTime>( );
            this.TotalePerGiorno = new List<int>( );
        }
    }

    public class TabellaGiornateVM
    {
        public DateTime DataPartenza { get; set; }
        public List<ConteggioGiorniConsecutivi> Dati { get; set; }
    }


    public class StatoFerieObj
    {
        public StatoFerieObj ( )
        {
            this.Pianificato = new ParametriGrafico( );
            this.Effettivo = new ParametriGrafico( );
            this.Dipendenti = new List<DipendenteScostamento>( );
        }

        public ParametriGrafico Pianificato { get; set; }
        public ParametriGrafico Effettivo { get; set; }
        public int GiorniPianificatiAdOggi { get; set; }
        public int GiorniEffettiviAdOggi { get; set; }
        public int GiorniPianificatiTotali { get; set; }
        public int GiorniEffettiviTotali { get; set; }
        public string CodiceSedeGapp { get; set; }
        public string DescrizioneSedeGapp { get; set; }
        public List<DipendenteScostamento> Dipendenti { get; set; }
    }

    public class DipendenteScostamento
    {
        public string Matricola { get; set; }

        public string Nominativo { get; set; }

        public int GiorniPianificati { get; set; }
        public int GiorniPianificatiAdOggi { get; set; }
        public int GiorniEffettivi { get; set; }
        public int Scostamento { get; set; }
        public int Percentuale { get; set; }
    }
}