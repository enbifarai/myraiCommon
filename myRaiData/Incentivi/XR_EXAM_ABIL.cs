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
    
    public partial class XR_EXAM_ABIL
    {
        public int ID_ABIL { get; set; }
        public int ID_PERSONA { get; set; }
        public string MATRICOLA { get; set; }
        public bool IND_RICHIESTE { get; set; }
        public bool IND_APPUNTAMENTO { get; set; }
        public bool IND_EXPORT { get; set; }
        public string SEDI_INCLUSE { get; set; }
        public string SEDI_ESCLUSE { get; set; }
        public string SERV_INCLUSE { get; set; }
        public string SERV_ESCLUSE { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public bool IND_EXAM { get; set; }
        public string COD_EXAM { get; set; }
        public string SOC_INCLUSE { get; set; }
        public string SOC_ESCLUSE { get; set; }
    }
}