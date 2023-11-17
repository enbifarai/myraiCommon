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
    
    public partial class EDIZIONE
    {
        public EDIZIONE()
        {
            this.TREQUESTS_STEP = new HashSet<TREQUESTS_STEP>();
            this.EDITIONCALENDAR = new HashSet<EDITIONCALENDAR>();
            this.CURRFORM = new HashSet<CURRFORM>();
        }
    
        public int ID_EDIZIONE { get; set; }
        public Nullable<int> ID_TRACENTER { get; set; }
        public Nullable<int> ID_DFTEMPLATE { get; set; }
        public Nullable<int> ID_ENTE { get; set; }
        public Nullable<int> ID_TRACLASSROOM { get; set; }
        public int ID_CORSO { get; set; }
        public Nullable<int> ID_TRAOFFERING { get; set; }
        public Nullable<int> ID_TRPLAN { get; set; }
        public string COD_EDIZIONE { get; set; }
        public string DES_EDIZIONE { get; set; }
        public Nullable<int> ID_TPPART { get; set; }
        public Nullable<int> ID_QSTCATALOG { get; set; }
        public string COD_CITTA { get; set; }
        public System.DateTime DTA_INIZIO { get; set; }
        public System.DateTime DTA_FINE { get; set; }
        public string DES_LOCALITA { get; set; }
        public int QTA_PARTECFFETIV { get; set; }
        public string NOT_EDIZIONE { get; set; }
        public string IND_STATOEDIZ { get; set; }
        public int NMB_MINATTENDANCES { get; set; }
        public Nullable<int> NMB_OPTATTENDANCES { get; set; }
        public int NMB_MAXATTENDANCES { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public Nullable<decimal> QTA_DAYSDURATION { get; set; }
        public Nullable<decimal> QTA_HOURSDURATION { get; set; }
        public string IND_DELIVERYINTERNE { get; set; }
        public string IND_DELIVERYINTER { get; set; }
        public string COD_TRACKMODE { get; set; }
        public string COD_ATTENDANCETYPE { get; set; }
        public string IND_ISDIRTY { get; set; }
        public string ID_CFJOB { get; set; }
        public string COD_JOBSTATUS { get; set; }
        public Nullable<decimal> PRC_JOBDONE { get; set; }
        public Nullable<System.DateTime> DTA_JOBSCHEDULE { get; set; }
        public string IND_CHECKLIST { get; set; }
        public Nullable<int> ID_FEEDBACKTYPE { get; set; }
        public Nullable<int> ID_PROFQUALTYPE { get; set; }
        public string COD_STUDIO { get; set; }
        public Nullable<int> ID_DFTEMPLATE_TST { get; set; }
        public Nullable<int> ID_QSTCATALOG_TST { get; set; }
        public string COD_ASSIGNMENTTYPE { get; set; }
        public string COD_ATTESTATO { get; set; }
        public Nullable<int> QTA_ATTVALID { get; set; }
        public Nullable<decimal> QTA_FEEDBACKPOINTS { get; set; }
        public string IND_ISTEST { get; set; }
        public string COD_ATTVALIDTU { get; set; }
        public Nullable<decimal> QTA_MINSCORE { get; set; }
        public string COD_COMPLETEDOPT { get; set; }
        public Nullable<long> QTA_AVAILABLETILL { get; set; }
        public string COD_AVAILABLETILLTYPE { get; set; }
        public string DES_LANGUAGE { get; set; }
        public string IND_ANONYMOUS { get; set; }
        public string COD_PLATFORM { get; set; }
        public string ID_CFJOBSC { get; set; }
        public string COD_JOBSTATUSSC { get; set; }
        public Nullable<decimal> PRC_JOBDONESC { get; set; }
        public Nullable<System.DateTime> DTA_JOBSCHEDULESC { get; set; }
        public string COD_AUTHORIZATION { get; set; }
        public string COD_TESTSOURCE { get; set; }
        public string IND_SYNCRONIZE { get; set; }
        public Nullable<System.DateTime> DTA_EXPORTCICS { get; set; }
    
        public virtual ICollection<TREQUESTS_STEP> TREQUESTS_STEP { get; set; }
        public virtual CORSO CORSO { get; set; }
        public virtual ICollection<EDITIONCALENDAR> EDITIONCALENDAR { get; set; }
        public virtual TRACENTER TRACENTER { get; set; }
        public virtual ENTE ENTE { get; set; }
        public virtual TRACLASSROOM TRACLASSROOM { get; set; }
        public virtual ICollection<CURRFORM> CURRFORM { get; set; }
    }
}
