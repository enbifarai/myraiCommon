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
    
    public partial class XR_STATO_RAPPORTO
    {
        public XR_STATO_RAPPORTO()
        {
            this.XR_STATO_RAPPORTO_INFO = new HashSet<XR_STATO_RAPPORTO_INFO>();
        }
    
        public int ID_STATO_RAPPORTO { get; set; }
        public int ID_PERSONA { get; set; }
        public System.DateTime DTA_INIZIO { get; set; }
        public string COD_STATO_RAPPORTO { get; set; }
        public string IND_AUTOM { get; set; }
        public System.DateTime DTA_FINE { get; set; }
        public string NOT_NOTA { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public Nullable<System.DateTime> DTA_NOTIF_DIP { get; set; }
        public Nullable<System.DateTime> DTA_NOTIF_ENTE { get; set; }
        public string COD_TIPO_ACCORDO { get; set; }
        public Nullable<System.DateTime> VALID_DTA_INI { get; set; }
        public Nullable<System.DateTime> VALID_DTA_END { get; set; }
        public Nullable<int> ID_STATO_RAPPORTO_ORIG { get; set; }
        public string MATRICOLA { get; set; }
        public Nullable<int> ID_MOD_DIPENDENTI { get; set; }
        public string COD_NOTIF_ENTE { get; set; }
        public Nullable<System.DateTime> DTA_SCADENZA { get; set; }
        public Nullable<System.DateTime> DTA_INIZIO_VISUALIZZAZIONE { get; set; }
        public Nullable<int> ID_RICH_RECESSO { get; set; }
        public Nullable<bool> FLG_FORZA_INIZIO_ACCORDO { get; set; }
        public string SWDEROGA_SCELTA { get; set; }
        public string SWDEROGA_OPZIONE { get; set; }
        public Nullable<bool> LAVORATOREFRAGILE { get; set; }
        public string LAVORATOREFRAGILE_SCELTA { get; set; }
    
        public virtual SINTESI1 SINTESI1 { get; set; }
        public virtual XR_TB_STATO_RAPPORTO XR_TB_STATO_RAPPORTO { get; set; }
        public virtual ICollection<XR_STATO_RAPPORTO_INFO> XR_STATO_RAPPORTO_INFO { get; set; }
    }
}
