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
    
    public partial class JNAGPERS
    {
        public int ID_PERSONA { get; set; }
        public string IND_VALCFS { get; set; }
        public string COD_DIPRAI { get; set; }
        public string DES_DIPRAI { get; set; }
        public string COD_MATCOLL { get; set; }
        public string COD_MATDIP { get; set; }
        public string COD_MATCOLL1 { get; set; }
        public string COD_MATCOLL2 { get; set; }
        public string COD_MATCOLL3 { get; set; }
        public string COD_MATCOLL4 { get; set; }
        public string COD_MATCOLL5 { get; set; }
        public string COD_TIPOGENERALITA { get; set; }
        public string COD_DIPPUBAMMIN { get; set; }
        public string IND_CARICATOINSAP { get; set; }
        public string IND_ISCRALTRIENTI { get; set; }
        public string COD_CASAGIT { get; set; }
    
        public virtual ANAGPERS ANAGPERS { get; set; }
    }
}