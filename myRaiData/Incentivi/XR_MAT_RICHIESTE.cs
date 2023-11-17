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
    
    public partial class XR_MAT_RICHIESTE
    {
        public XR_MAT_RICHIESTE()
        {
            this.XR_MAT_ECCEZIONI = new HashSet<XR_MAT_ECCEZIONI>();
            this.XR_MAT_NOTE = new HashSet<XR_MAT_NOTE>();
            this.XR_MAT_PIANIFICAZIONI = new HashSet<XR_MAT_PIANIFICAZIONI>();
            this.XR_MAT_PROMEMORIA = new HashSet<XR_MAT_PROMEMORIA>();
            this.XR_MAT_TASK_IN_CORSO = new HashSet<XR_MAT_TASK_IN_CORSO>();
            this.XR_MAT_SEGNALAZIONI = new HashSet<XR_MAT_SEGNALAZIONI>();
            this.XR_MAT_TASK_DI_SERVIZIO = new HashSet<XR_MAT_TASK_DI_SERVIZIO>();
            this.XR_MAT_VOCI_CEDOLINO = new HashSet<XR_MAT_VOCI_CEDOLINO>();
            this.XR_WKF_OPERSTATI = new HashSet<XR_WKF_OPERSTATI>();
            this.XR_MAT_ALLEGATI = new HashSet<XR_MAT_ALLEGATI>();
            this.XR_DEM_DOCUMENTI = new HashSet<XR_DEM_DOCUMENTI>();
        }
    
        public int ID { get; set; }
        public string MATRICOLA { get; set; }
        public string NOMINATIVO { get; set; }
        public string SEDEGAPP { get; set; }
        public string REPARTO { get; set; }
        public System.DateTime DATA_INVIO_RICHIESTA { get; set; }
        public Nullable<System.DateTime> DATA_SCADENZA { get; set; }
        public int CATEGORIA { get; set; }
        public Nullable<System.DateTime> DATA_PRESUNTA_PARTO { get; set; }
        public Nullable<System.DateTime> DATA_PARTO { get; set; }
        public Nullable<System.DateTime> DATA_INTERDIZIONE { get; set; }
        public Nullable<System.DateTime> DATA_INGRESSO_FAMIGLIA { get; set; }
        public Nullable<System.DateTime> DATA_INGRESSO_ITALIA { get; set; }
        public Nullable<System.DateTime> DATA_INIZIO_ANTICIPATO { get; set; }
        public Nullable<System.DateTime> DATA_INIZIO_MATERNITA { get; set; }
        public Nullable<System.DateTime> DATA_FINE_MATERNITA { get; set; }
        public bool SCELTA_ANTICIPAZIONE_MESI { get; set; }
        public Nullable<int> MESE_FLESSIBILITA { get; set; }
        public Nullable<System.DateTime> DATA_NASCITA_BAMBINO { get; set; }
        public string CF_BAMBINO { get; set; }
        public Nullable<System.DateTime> GIORNO_GIUSTIFICATIVO { get; set; }
        public Nullable<System.DateTime> INIZIO_GIUSTIFICATIVO { get; set; }
        public Nullable<System.DateTime> FINE_GIUSTIFICATIVO { get; set; }
        public Nullable<int> NUMERO_GIORNI_GIUSTIFICATIVO { get; set; }
        public string PROTOCOLLO_INPS { get; set; }
        public Nullable<decimal> REDDITO { get; set; }
        public Nullable<decimal> GIORNI_USUFRUITI_CONIUGE { get; set; }
        public Nullable<System.DateTime> PRESA_VISIONE_RESP_GEST { get; set; }
        public string PRESA_VISIONE_RESP_MATR { get; set; }
        public string ECCEZIONE { get; set; }
        public Nullable<bool> PIANIFICAZIONE_BASE_ORARIA { get; set; }
        public Nullable<bool> GENITORE_SOLO { get; set; }
        public string NOMINATIVO_CONIUGE_RAI { get; set; }
        public string MATRICOLA_CONIUGE_RAI { get; set; }
        public Nullable<bool> ASSENZA_LUNGA { get; set; }
        public Nullable<System.DateTime> IMPORTATA_DATETIME { get; set; }
        public string IMPORTATA_MATRICOLA { get; set; }
        public Nullable<int> ID_RICHIESTA_PERIODO_PRECEDENTE { get; set; }
        public Nullable<decimal> GIORNI_DEFAULT26 { get; set; }
        public string NOME_BAMBINO { get; set; }
        public bool PERMESSO_FRUIBILE { get; set; }
        public string CUSTOM_JSON { get; set; }
        public Nullable<System.DateTime> PRATICA_SOSPESA_DATETIME { get; set; }
        public string PRATICA_SOSPESA_MATR { get; set; }
        public bool DA_RIAVVIARE { get; set; }
        public string VARIE { get; set; }
        public Nullable<System.DateTime> DATA_INIZIO_SW { get; set; }
        public Nullable<System.DateTime> DATA_FINE_SW { get; set; }
        public Nullable<int> GIORNI_APPROVATI { get; set; }
        public string APPROVATORE_SE_NON_ABIL { get; set; }
        public Nullable<int> TIPO_FLEX_MATERNITA { get; set; }
        public bool FORZA_ECCEZIONE_PRATICA { get; set; }
        public string SW_STATO_GENITORI { get; set; }
        public Nullable<System.DateTime> DATA_AVVIATA { get; set; }
        public string NOME_ASSISTITO { get; set; }
        public string RAPPORTO_ASSISTITO { get; set; }
        public string MATRICOLA_INS_RAIPERME { get; set; }
        public Nullable<System.DateTime> ABILITATO_RAIPERME_INIZIO { get; set; }
        public Nullable<System.DateTime> ABILITATO_RAIPERME_FINE { get; set; }
        public Nullable<System.DateTime> ESPORTATA_DB2 { get; set; }
    
        public virtual XR_MAT_CATEGORIE XR_MAT_CATEGORIE { get; set; }
        public virtual ICollection<XR_MAT_ECCEZIONI> XR_MAT_ECCEZIONI { get; set; }
        public virtual ICollection<XR_MAT_NOTE> XR_MAT_NOTE { get; set; }
        public virtual ICollection<XR_MAT_PIANIFICAZIONI> XR_MAT_PIANIFICAZIONI { get; set; }
        public virtual ICollection<XR_MAT_PROMEMORIA> XR_MAT_PROMEMORIA { get; set; }
        public virtual ICollection<XR_MAT_TASK_IN_CORSO> XR_MAT_TASK_IN_CORSO { get; set; }
        public virtual ICollection<XR_MAT_SEGNALAZIONI> XR_MAT_SEGNALAZIONI { get; set; }
        public virtual ICollection<XR_MAT_TASK_DI_SERVIZIO> XR_MAT_TASK_DI_SERVIZIO { get; set; }
        public virtual ICollection<XR_MAT_VOCI_CEDOLINO> XR_MAT_VOCI_CEDOLINO { get; set; }
        public virtual ICollection<XR_WKF_OPERSTATI> XR_WKF_OPERSTATI { get; set; }
        public virtual ICollection<XR_MAT_ALLEGATI> XR_MAT_ALLEGATI { get; set; }
        public virtual ICollection<XR_DEM_DOCUMENTI> XR_DEM_DOCUMENTI { get; set; }
    }
}