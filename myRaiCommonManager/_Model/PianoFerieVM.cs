using System;
using System.Collections.Generic;
using System.ComponentModel;
using myRaiHelper;

namespace myRaiCommonModel
{
    public class PianoFerieVM
    {
        public PianoFerieVM ( )
        {
            this.WidgetId = "PianoFerie";
            this.AnnoCorrente = DateTime.Now.Year;
            this.MeseCorrente = DateTime.Now.Month;
            this.SedeGapp = UtenteHelper.SedeGapp( DateTime.Now );
            this.SediGapp = CommonHelper.GetSediGappResponsabileList( );
            this.Utenti = new List<PianoFerieCalendarItem>( );
        }

        public string WidgetId { get; set; }

        public int MeseCorrente { get; set; }
        public int AnnoCorrente { get; set; }

        public string SedeGapp { get; set; }
        public List<PianoFerieCalendarItem> Utenti { get; set; }

        public List<Sede> SediGapp { get; set; }
    }

    public class PianoFerieCalendarItem
    {
        public string Matricola { get; set; }
        public string Foto { get; set; }
        public string Nominativo { get; set; }
        public string TipoDip { get; set; }
        public List<PFCalendarDayDetail> Eccezioni { get; set; }
    }

    public class PFCalendarDayDetail
    {
        public int Giorno { get; set; }
        public string CodiceEccezione { get; set; }
        public string Eccezione { get; set; }
		//public EnumStatiRichiesta Stato { get; set; }
        public string DescrizioneEccezione { get; set; }
        public string ColorePie { get; set; }
    }
    public class CheckRipianificazione
    {
        public bool StornoPossibileSenzaRipianificazione { get; set; }
        public int FerieMinimo { get; set; }
        public float FerieInserite { get; set; }
        public int StorniNonRipianificati { get; set; }

    }

    public enum PFEccezioneColorEnum
    {
        [Icon( "bg-primary" )]
        [AmbientValue( "FERIE" )]
        [Description( "FE" )]
        FE,

        [Icon( "bg-primary" )]
        [AmbientValue( "FERIE MATTINA" )]
        [Description( "FE" )]
        FEM,

        [Icon( "bg-primary" )]
        [AmbientValue( "FERIE POMERIGGIO" )]
        [Description( "FE" )]
        FEP,

        [Icon( "bg-secondary" )]
        [AmbientValue( "PERM.RETR.(EX F.NAZ.)" )]
        [Description( "PF" )]
        PF,

        [Icon( "bg-secondary" )]
        [AmbientValue( "1/2 PF MATTINO" )]
        [Description( "PF" )]
        PFM,

        [Icon( "bg-secondary" )]
        [AmbientValue( "1/2 PF POMERIGGIO" )]
        [Description( "PF" )]
        PFP,

        [Icon( "bg-secondary" )]
        [AmbientValue( "1/4 PERMESSO EX FESTIVITA" )]
        [Description( "PF" )]
        PFQ,

        [Icon( "bg-success" )]
        [AmbientValue( "PG MATTINO" )]
        [Description( "PG" )]
        PGM,

        [Icon( "bg-success" )]
        [AmbientValue( "PG POMERIGGIO" )]
        [Description( "PG" )]
        PGP,

        [Icon( "bg-success" )]
        [AmbientValue( "1/4 PG" )]
        [Description( "PG" )]
        PGQ,

        [Icon( "bg-info" )]
        [AmbientValue( "PERMESSO RETRIBUITO" )]
        [Description( "PR" )]
        PR,

        [Icon( "bg-info" )]
        [AmbientValue( "1/2 PR MATTINO" )]
        [Description( "PR" )]
        PRM,

        [Icon( "bg-info" )]
        [AmbientValue( "1/2 PR POMERIGGIO" )]
        [Description( "PR" )]
        PRP,

        [Icon( "bg-info" )]
        [AmbientValue( "1/4 PERMESSO RETRIBUITO" )]
        [Description( "PR" )]
        PRQ,

        [Icon( "bg-dark" )]
        [AmbientValue( "PERM.STRAORD.GIORNALISTI" )]
        [Description( "PX" )]
        PX,

        [Icon( "bg-dark" )]
        [AmbientValue( "PERM.STRAORD.GIORNAL-MATT" )]
        [Description( "PX" )]
        PXM,

        [Icon( "bg-dark" )]
        [AmbientValue( "PERM.STRAORD.GIORNAL-POM" )]
        [Description( "PX" )]
        PXP,

        [Icon( "bg-danger" )]
        [AmbientValue( "RECUPERO NON LAVORATO" )]
        [Description( "RN" )]
        RN,

        [Icon( "bg-danger" )]
        [AmbientValue( "1/2 RN MATTINO" )]
        [Description( "RN" )]
        RNM,

        [Icon( "bg-danger" )]
        [AmbientValue( "1/2 RN POMERIGGIO" )]
        [Description( "RN" )]
        RNP,

        [Icon( "bg-warning" )]
        [AmbientValue( "RECUPERO RIPOSO" )]
        [Description( "RR" )]
        RR,

        [Icon( "bg-warning" )]
        [AmbientValue( "1/2 RR MATTINO" )]
        [Description( "RR" )]
        RRM,

        [Icon( "bg-warning" )]
        [AmbientValue( "1/2 RR POMERIGGIO" )]
        [Description( "RR" )]
		RRP,

        [Icon( "bg-primary" )]
        [AmbientValue( "" )]
        [Description( "" )]
        DEFAULT
    }
}
