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
    
    public partial class STUPERSONA
    {
        public int ID_PERSONA { get; set; }
        public string COD_STUDIO { get; set; }
        public Nullable<int> ID_VERIFIEDBY { get; set; }
        public string COD_CITTA { get; set; }
        public string COD_PUNTEGGIO { get; set; }
        public Nullable<short> COD_PUNTEGGIONUM { get; set; }
        public string COD_TIPOPUNTEGGIO { get; set; }
        public Nullable<short> COD_LIVELLOPESO { get; set; }
        public string COD_ATENEO { get; set; }
        public System.DateTime DTA_CONSEG { get; set; }
        public string NOT_NOTABREVE { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public Nullable<System.DateTime> DTA_STARTDATE { get; set; }
        public string IND_COMPANYFUNDED { get; set; }
        public string IND_VERIFIED { get; set; }
        public Nullable<short> COD_LIVELLOSTUDIO { get; set; }
        public string DES_LIVELLOSTUDIO { get; set; }
        public string DES_CORSO { get; set; }
        public Nullable<System.DateTime> DTA_INIZIO { get; set; }
        public string DES_RICONOSCIMENTO { get; set; }
        public string DES_ISTITUTO { get; set; }
    
        public virtual TB_ATENEO TB_ATENEO { get; set; }
        public virtual TB_TPPUNT TB_TPPUNT { get; set; }
        public virtual TB_COMUNE TB_COMUNE { get; set; }
        public virtual TB_STUDIO TB_STUDIO { get; set; }
    }
}