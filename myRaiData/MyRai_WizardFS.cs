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
    
    public partial class MyRai_WizardFS
    {
        public int id { get; set; }
        public string matricola { get; set; }
        public System.DateTime data { get; set; }
        public int id_stato { get; set; }
        public string orario_richiesto { get; set; }
        public string orario_alla_richiesta { get; set; }
        public string eccezioni_richieste { get; set; }
        public System.DateTime data_ultimo_aggiornamento { get; set; }
        public string Spostamento { get; set; }
    
        public virtual MyRai_StatiWizardFS MyRai_StatiWizardFS { get; set; }
    }
}
