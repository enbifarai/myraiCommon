//------------------------------------------------------------------------------
// <auto-generated>
//    Codice generato da un modello.
//
//    Le modifiche manuali a questo file potrebbero causare un comportamento imprevisto dell'applicazione.
//    Se il codice viene rigenerato, le modifiche manuali al file verranno sovrascritte.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiDataTalentia
{
    using System;
    using System.Collections.Generic;
    
    public partial class ENTE
    {
        public ENTE()
        {
            this.EDIZIONE = new HashSet<EDIZIONE>();
        }
    
        public int ID_ENTE { get; set; }
        public string COD_ENTE { get; set; }
        public string COD_CITTA { get; set; }
        public string DES_ENTE { get; set; }
        public string PIV_PARTITAIVA { get; set; }
        public string DES_INDIRIZZO { get; set; }
        public string CFS_CODICEFISCAL { get; set; }
        public string DES_TELEFONO { get; set; }
        public string DES_FAX { get; set; }
        public string DES_REFERENTE { get; set; }
        public string NOT_ENTE { get; set; }
        public string IND_PUBBLICO { get; set; }
        public string DES_EMAIL { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public string COD_NSIRET { get; set; }
        public string COD_APE { get; set; }
        public Nullable<decimal> QTA_TAX { get; set; }
        public string IND_TAX { get; set; }
    
        public virtual ICollection<EDIZIONE> EDIZIONE { get; set; }
    }
}
