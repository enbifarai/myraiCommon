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
    
    public partial class MyRai_Regole_SchedeEccezioni_Destinatari
    {
        public int id { get; set; }
        public int id_scheda_eccezione { get; set; }
        public System.DateTime data_inizio_validita { get; set; }
        public Nullable<System.DateTime> data_fine_validita { get; set; }
        public int id_destinatario { get; set; }
    
        public virtual MyRai_Regole_Destinatari MyRai_Regole_Destinatari { get; set; }
        public virtual MyRai_Regole_SchedeEccezioni MyRai_Regole_SchedeEccezioni { get; set; }
    }
}
