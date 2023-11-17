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
    
    public partial class ANAGPERS
    {
        public ANAGPERS()
        {
            this.COMPREL = new HashSet<COMPREL>();
            this.ASSQUAL = new HashSet<ASSQUAL>();
            this.ASSTPCONTR = new HashSet<ASSTPCONTR>();
            this.TRASF_SEDE = new HashSet<TRASF_SEDE>();
            this.INCARLAV = new HashSet<INCARLAV>();
            this.XR_SERVIZIO = new HashSet<XR_SERVIZIO>();
            this.CITTAD = new HashSet<CITTAD>();
        }
    
        public int ID_PERSONA { get; set; }
        public Nullable<int> ID_IDENDOC { get; set; }
        public string COD_ETHNICGROUP { get; set; }
        public string COD_CATPROTEFF { get; set; }
        public Nullable<decimal> PRC_INVALIDITA { get; set; }
        public string COD_CITTADOM { get; set; }
        public string COD_CITTAREC { get; set; }
        public string COD_CITTA { get; set; }
        public string COD_STCIV { get; set; }
        public string CSF_CFSPERSONA { get; set; }
        public string DES_TITOLOONOR { get; set; }
        public string DES_COGNOMEPERS { get; set; }
        public string DES_SECCOGNOME { get; set; }
        public string DES_NOMEPERS { get; set; }
        public System.DateTime DTA_NASCITAPERS { get; set; }
        public Nullable<System.DateTime> DTA_DEATHPERS { get; set; }
        public string COD_SESSO { get; set; }
        public string DES_COGNOMEACQ { get; set; }
        public string DES_NOMEPADRE { get; set; }
        public string COD_TESSERASAN { get; set; }
        public string IND_PATENTE { get; set; }
        public string COD_TIPOPATENTE { get; set; }
        public string NOT_PERS { get; set; }
        public string COD_POSMIL { get; set; }
        public string DES_INDIRREC { get; set; }
        public string DES_TELREC { get; set; }
        public string CAP_CAPREC { get; set; }
        public string DES_PRESSOREC { get; set; }
        public string DES_CELLULARE { get; set; }
        public string IND_RECAPITO { get; set; }
        public string DES_TELDOM { get; set; }
        public string CAP_CAPDOM { get; set; }
        public string DES_INDIRDOM { get; set; }
        public string IND_DOMICILIO { get; set; }
        public string DES_FOTO { get; set; }
        public string DES_PICTURE { get; set; }
        public string DES_EMAIL { get; set; }
        public string DES_EMCADDRESS { get; set; }
        public string COD_EMCCITY { get; set; }
        public string CAP_EMCZIP { get; set; }
        public string DES_EMCPHONE { get; set; }
        public string DES_EMCEMAIL { get; set; }
        public string COD_PLANNING { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public string DES_BLOODGROUP { get; set; }
        public string FLG_DIABETIC { get; set; }
        public string FLG_ALLERGIES { get; set; }
        public string DES_MEDICALDETAILS { get; set; }
        public string DES_NINUMBER { get; set; }
        public string DES_NILETTER { get; set; }
        public Nullable<System.DateTime> DTA_MARITALSTATUS { get; set; }
        public string DES_EXTERNALEMAIL { get; set; }
        public string DES_WORKPHONE { get; set; }
        public string DES_WORKPHONEMOBILE { get; set; }
        public string DES_WORKLOCATION { get; set; }
        public string COD_MATDIP { get; set; }
        public string COD_MATCOLL { get; set; }
        public string COD_CASAGIT { get; set; }
        public string IND_VALCFS { get; set; }
        public string COD_DIPRAI { get; set; }
        public string DES_DIPRAI { get; set; }
        public string COD_MATCOLL1 { get; set; }
        public string COD_MATCOLL2 { get; set; }
        public string COD_MATCOLL3 { get; set; }
        public string COD_MATCOLL4 { get; set; }
        public string COD_MATCOLL5 { get; set; }
        public string COD_TIPOGENERALITA { get; set; }
        public string COD_DIPPUBAMMIN { get; set; }
        public string IND_CARICATOINSAP { get; set; }
        public string IND_ISCRALTRIENTI { get; set; }
    
        public virtual ICollection<COMPREL> COMPREL { get; set; }
        public virtual ICollection<ASSQUAL> ASSQUAL { get; set; }
        public virtual ICollection<ASSTPCONTR> ASSTPCONTR { get; set; }
        public virtual ICollection<TRASF_SEDE> TRASF_SEDE { get; set; }
        public virtual ICollection<INCARLAV> INCARLAV { get; set; }
        public virtual ICollection<XR_SERVIZIO> XR_SERVIZIO { get; set; }
        public virtual ICollection<CITTAD> CITTAD { get; set; }
    }
}
