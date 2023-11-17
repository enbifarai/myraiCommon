//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData
{
    using System;
    using System.Collections.Generic;
    
    public partial class MyRai_Eccezioni_Ammesse
    {
        public int id { get; set; }
        public string IdTipologieDocumento { get; set; }
        public string cod_eccezione { get; set; }
        public string desc_eccezione { get; set; }
        public Nullable<System.DateTime> data_inizio_validita { get; set; }
        public Nullable<System.DateTime> data_fine_validita { get; set; }
        public Nullable<bool> flag_attivo { get; set; }
        public string tipo_controllo { get; set; }
        public Nullable<int> id_raggruppamento { get; set; }
        public string periodo { get; set; }
        public string controlli_specifici { get; set; }
        public int OreInPassato { get; set; }
        public int OreInFuturo { get; set; }
        public string PartialView { get; set; }
        public string FunzioneJS { get; set; }
        public string TipiDipendente { get; set; }
        public string ValoriParamExtraJSON { get; set; }
        public string Categoria { get; set; }
        public string OrarioPrevisto { get; set; }
        public string OrarioEffettivo { get; set; }
        public string TipoGiornata { get; set; }
        public Nullable<int> CaratteriMotivoRichiesta { get; set; }
        public string StatoGiornata { get; set; }
        public bool Prontuario { get; set; }
        public string TipiDipendenteEsclusi { get; set; }
        public int id_workflow { get; set; }
        public string descrizione_eccezione { get; set; }
        public string CodiceCsharp { get; set; }
        public Nullable<bool> RichiedeDocumento { get; set; }
        public Nullable<int> MaxPerGiorno { get; set; }
        public string ProponiAutoTipiDip { get; set; }
        public string AutoApprovataTipiDip { get; set; }
        public bool RichiedeAttivitaCeiton { get; set; }
        public bool CopertaDaTimbrature { get; set; }
        public bool FuoriDaTimbratureEntroOrario { get; set; }
        public bool FuoriDaTimbratureFuoriOrario { get; set; }
        public bool FuoriOrarioInTesta { get; set; }
        public bool FuoriOrarioInCoda { get; set; }
        public string dipende_da_eccezione { get; set; }
        public string esclusa_da_eccezione { get; set; }
        public string note { get; set; }
        public bool no_corrispondenza_gapp { get; set; }
        public bool non_rifiutabile { get; set; }
    
        public virtual MyRai_Raggruppamenti MyRai_Raggruppamenti { get; set; }
        public virtual MyRai_Workflows MyRai_Workflows { get; set; }
    }
}
