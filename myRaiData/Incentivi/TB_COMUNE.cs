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
    
    public partial class TB_COMUNE
    {
        public TB_COMUNE()
        {
            this.ANAGPERS = new HashSet<ANAGPERS>();
            this.ANAGPERS1 = new HashSet<ANAGPERS>();
            this.XR_ANAGBANCA = new HashSet<XR_ANAGBANCA>();
            this.STUPERSONA = new HashSet<STUPERSONA>();
            this.TB_ATENEO = new HashSet<TB_ATENEO>();
            this.RESIDENZA = new HashSet<RESIDENZA>();
            this.XR_NDI_RESIDENZA = new HashSet<XR_NDI_RESIDENZA_DOMICILIO>();
            this.XR_NDI_DOMICILIO = new HashSet<XR_NDI_RESIDENZA_DOMICILIO>();
        }
    
        public string COD_CITTA { get; set; }
        public string DES_CITTA { get; set; }
        public string COD_PROV_STATE { get; set; }
        public string DES_PROV_STATE { get; set; }
        public string COD_SIGLANAZIONE { get; set; }
        public string COD_AREA { get; set; }
        public string DES_TERRCODE { get; set; }
        public short QTA_ORDINE { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    
        public virtual ICollection<ANAGPERS> ANAGPERS { get; set; }
        public virtual ICollection<ANAGPERS> ANAGPERS1 { get; set; }
        public virtual ICollection<XR_ANAGBANCA> XR_ANAGBANCA { get; set; }
        public virtual TB_NAZIONE TB_NAZIONE { get; set; }
        public virtual ICollection<STUPERSONA> STUPERSONA { get; set; }
        public virtual ICollection<TB_ATENEO> TB_ATENEO { get; set; }
        public virtual ICollection<RESIDENZA> RESIDENZA { get; set; }
        public virtual ICollection<XR_NDI_RESIDENZA_DOMICILIO> XR_NDI_RESIDENZA { get; set; }
        public virtual ICollection<XR_NDI_RESIDENZA_DOMICILIO> XR_NDI_DOMICILIO { get; set; }
    }
}