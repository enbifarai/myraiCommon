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
    
    public partial class CZNDOCS
    {
        public int ID_DOC { get; set; }
        public int ID_ENTITY { get; set; }
        public Nullable<int> ID_DOCSCAT { get; set; }
        public string NME_FILENAME { get; set; }
        public string DES_DESCRIPTION { get; set; }
        public string COD_EXTENSION { get; set; }
        public byte[] OBJ_OBJECT { get; set; }
        public int NBR_SIZE { get; set; }
        public string COD_ENTITYTYPE { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    
        public virtual CORSO CORSO { get; set; }
    }
}
