using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using myRaiData;

namespace myRaiCommonModel
{
    public class DaFirmareModel
    {
        public List<Sede> Sedi { get; set; }
        public sidebarModel menuSidebar { get; set; }
        public ModelDash modeldash { get; set; }


        public TotaliDaFirmareModel TotaliDaFirmare { get; set; }
        public RicercaPdfModel RicercaPdf { get; set; }

        public int PianiFerieDisponibiliPerFirma { get; set; }
        public List<Alias> ListaAlias { get; set; }
        public List<int> IdNelCarrello { get; set; }

    }

    public class Alias
    {
        public string PmatricolaDaImpersonare { get; set; }
        public string NominativoDaImpersonare { get; set; }
    }

    //public class Sede
    //{
    //    public string CodiceSede { get; set; }
    //    public string DescrizioneSede { get; set; }
    //    public List<RepartoLinkedServer> RepartiSpecifici { get; set; }
    //    public IEnumerable<PeriodoPDF> PeriodiPDF { get; set; }
    //    public List<MyRai_PianoFerieSedi> ListaPianiFerie { get; set; }
    //}


    public class PeriodoPDF
    {
        public long idPdf { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public int Versione { get; set; }
        public DateTime? Data_generazione { get; set; }
        public string codSede { get; set; }
        public string descSede { get; set; }
        public DateTime? Data_letto { get; set; }
        public string tipoPDF { get; set; }
        public string statoPDF { get; set; }
        public bool inFirma { get; set; }
    }
    public class RicercaPdfModel
    {
        public SelectList listasedi { get; set; }
        public string codSede { get; set; }

        public SelectList listastati { get; set; }
        public string stato { get; set; }

    }
}