//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData.Incentivi
{
    using System;
    using System.Collections.Generic;
    
    public partial class XR_MAT_PROMEMORIA
    {
        public int ID { get; set; }
        public int ID_RICHIESTA { get; set; }
        public System.DateTime DATA { get; set; }
        public string TESTO { get; set; }
        public string MATRICOLA { get; set; }
        public System.DateTime DATA_INSERITO { get; set; }
    
        public virtual XR_MAT_RICHIESTE XR_MAT_RICHIESTE { get; set; }
    }
}
