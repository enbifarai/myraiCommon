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
    
    public partial class R_STRGROUP
    {
        public int ID_STRGROUP { get; set; }
        public string COD_IMPRESA { get; set; }
        public string COD_DIVISA { get; set; }
        public string COD_IMPRESAPADRE { get; set; }
        public string COD_TIPOIMPRESA { get; set; }
        public string IND_CONTRIBACC { get; set; }
        public string COD_ISO { get; set; }
        public string CFS_CODFISCIMP { get; set; }
        public short PRG_SORT { get; set; }
        public string PIV_PIVASOCIETA { get; set; }
        public string NOT_SOGGETTO { get; set; }
        public string COD_LINGUA { get; set; }
        public string DES_ISCRTRIB { get; set; }
        public string DES_ISCRCCOM { get; set; }
        public decimal IMP_CAPSOCIALE { get; set; }
        public string NOT_SHAREHOLDERS { get; set; }
        public decimal IMP_CAPITALSHARET { get; set; }
        public decimal IMP_CAPITALISSUED { get; set; }
        public decimal IMP_CAPITALPARVAL { get; set; }
        public string NOT_EXTAUDITORS { get; set; }
        public string DES_YEAREND { get; set; }
        public string NOT_BYLAWSART { get; set; }
        public string NOT_LOCSHARECERT { get; set; }
        public string NOT_LOCORGCORPDOC { get; set; }
        public string DES_CONSOLMETHOD { get; set; }
        public string NOT_NATUREOPER { get; set; }
        public string NOT_PURPOSE { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    
        public virtual STRGROUP STRGROUP { get; set; }
        public virtual CODIFYIMP CODIFYIMP { get; set; }
    }
}
