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
    
    public partial class XR_MBO_ALLEGATI
    {
        public int ID_ALLEGATO { get; set; }
        public int ID_SCHEDA { get; set; }
        public int ID_TIPOLOGIA { get; set; }
        public string NME_FILENAME { get; set; }
        public string DES_ALLEGATO { get; set; }
        public byte[] OBJ_OBJECT { get; set; }
        public string CONTENT_TYPE { get; set; }
        public Nullable<int> NMB_SIZE { get; set; }
        public string COD_TITLE { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public Nullable<System.DateTime> TMS_TIMESTAMP { get; set; }
        public Nullable<bool> IND_TEMP { get; set; }
    }
}
