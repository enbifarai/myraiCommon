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
    
    public partial class MyRai_ApprovazioneMassiva
    {
        public int Id { get; set; }
        public int IdRichiesta { get; set; }
        public string MatricolaApprovatore { get; set; }
        public System.DateTime DataApprovazione { get; set; }
        public string Provenienza { get; set; }
        public int Status { get; set; }
        public Nullable<System.DateTime> DataUltimoTentativo { get; set; }
        public string Errore { get; set; }
        public int IdNuovoStatoRichiesta { get; set; }
        public string Nota { get; set; }
        public string MatricolaApprovatoreGAPP { get; set; }
        public string MatricolaApprovatoreDB { get; set; }
    
        public virtual MyRai_ApprovazioneMassivaStatus MyRai_ApprovazioneMassivaStatus { get; set; }
        public virtual MyRai_Stati MyRai_Stati { get; set; }
    }
}
