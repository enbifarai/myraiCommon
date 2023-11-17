using myRaiHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.raiplace
{
    public class CestiniModel
    {
        public Ordine ordine { get; set; }
        public List<Richiesta> richieste { get; set; }
        public Richiesta richiestaCorrente { get; set; }
        /// <summary>
        /// Conteggio delle richieste associate all'ordine
        /// </summary>
        public int CountRichieste
        {
            get
            {
                return this._countRichieste;
            }
            set
            {
                this._countRichieste = value;
            }
        }

        public bool ReadOnlyMode { get; set; }

        private int _countRichieste { get; set; }
    }
    public class Ordine
    {
        public Ordine()
        {
            this.statusOrdine = StatusOrdiniEnum.Bozza;
			
			// Imposta il destinatario cestino di default
			this.DestinatarioCestino = DestinatarioEnum.Me;
        }

        public int idOrdine { get; set; }
        [Required(ErrorMessage = "Cespite è un dato obbligatorio")]
        [Display(Name = "Cespite", Description = "Cespite")]
        public string cespite { get; set; }
        public DateTime dataOraPasto { get; set; }
        [Required(ErrorMessage = "Il tipo pasto è un dato obbligatorio")]
        [Display(Name = "Tipo pasto", Description = "Tipo pasto")]
        public string tipoPasto { get; set; }
        public string approvatore { get; set; }
        [Required(ErrorMessage = "Il referente è un dato obbligatorio")]
        public string referenteConsegna { get; set; }
        public string matricolaReferenteConsegna { get; set; }
        [Required(ErrorMessage = "Il telefono referente è un dato obbligatorio")]
        public string telefonoReferente { get; set; }
        public string titoloProduzione { get; set; }
        [Required(ErrorMessage = "La matricola dello spettacolo è un dato obbligatorio")]
        public string matricolaSpettacolo { get; set; }
        [Required(ErrorMessage = "Il centro di costo è un dato obbligatorio")]
        public string centroCosto { get; set; }
        [Required(ErrorMessage = "Il motivo è un dato obbligatorio")]
        public string motivoOrdine { get; set; }
        public string note { get; set; }
        public string codiceOrdine { get; set; }
        public string matricolaOrdine { get; set; }
        public DateTime? dataApprovazione { get; set; }
        public StatusOrdiniEnum statusOrdine { get; set; }
        public DateTime dataStatus { get; set; }
        public string luogoConsegna { get; set; }

        public DateTime dataInserimento { get; set; }
        public string puntoOrdinante { get; set; }
        public string richiedente { get; set; }
        public string telefonoRichiedente { get; set; }
        public string struttura { get; set; }

		public DestinatarioEnum DestinatarioCestino
		{
			get;
			set;
		}
    }
    public class Richiesta
    {
        public Richiesta()
        {
            this.flagRisorsa = true;
			this.Deleted = false;
        }

        public int idOrdine { get; set; }
        public int idRichiesta { get; set; }
        public int progressivo { get; set; }
        public bool flagRisorsa { get; set; }
        [Display(Name = "Matricola", Description = "Matricola")]
        public string matricolaRisorsa { get; set; }
        [Display(Name = "Cognome", Description = "Cognome")]
        public string cognomeRisorsa { get; set; }
        [Display(Name = "Nome", Description = "Nome")]
        public string nomeRisorsa { get; set; }
        [Display(Name = "Motivo", Description = "Motivo")]
        public string motivoEsterno { get; set; }
        [Display(Name = "Tipo cestino", Description = "Tipo cestino")]
        public TipoCestinoEnum tipoCestino { get; set; }
        public string codiceRichiesta { get; set; }
        public DateTime dataInserimento { get; set; }
		public bool Deleted { get; set; }
    }

    public class AnagSelect
    {
        public string id { get; set; }
        public string text { get; set; }
        public string matricola { get; set; }
    }
    
    //<s:complexType name="ObjTVRicercaAnagrafieRecord">
    //    <s:sequence>
    //      <s:element minOccurs="0" maxOccurs="1" name="UORG" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="MATRICOLA" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="CODICE_ANAGRAF" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="SERIE" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="PUNTATA" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="ANNO_CONTABILE" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="TITOLO" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="TITOLO_ORIGIN" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="TITOLO_DEFINIT" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="TITOLO_PROGRAM" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="TIPO_SPETT" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="DEST_FUNZ" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="RAGGR" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="GENERE" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="SOTTOGENERE" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="EVIDENZA" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="STATO" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="TECNICA" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="DT_INIZ_DIR_AZ" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="DT_FINE_DIR_AZ" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="NUMERO_PASSAG" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="NUM_PASSAG_RES" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="BLOCCO" type="s:string" />
    //      <s:element minOccurs="0" maxOccurs="1" name="UORGBASE" type="s:string" />
    //    </s:sequence>
    //  </s:complexType>

    /// <summary>
    /// Oggetto che rappresenta i dati di ritorno della chiamata al servizio 
    /// per il reperimento dei metadati di un programma tv
    /// </summary>
    public class RicercaProgrammaResult
    {
        public string Matricola { get; set; }
        public string UORG { get; set; }
        /// <summary>
        /// mappato col campo TITOLO_DEFINIT di ObjTVRicercaAnagrafieRecord
        /// </summary>
        public string Titolo { get; set; }
    }

    public enum StatusOrdiniEnum
    {
        [IconAttribute("fa-circle circle-orange")]
        [Display(Name = "Bozza")]
        [AmbientValue("Bozza")]
        Bozza = 1,
        [IconAttribute("fa-circle circle-green")]
        [Display(Name = "Convalidata")]
        [AmbientValue("Convalidata")]
        Convalidata = 2,
        [IconAttribute("fa-circle circle-orange")]
        [Display(Name = "In attesa di convalida")]
        [AmbientValue("In attesa di convalida")]
        AttesaConvalida = 3,
        [IconAttribute("fa-refresh fa-spin")]
        [Display(Name = "In lavorazione")]
        [AmbientValue("In lavorazione")]
        InLavorazione = 4,
        [IconAttribute("fa-circle-thin")]
        [Display(Name = "Consuntivata")]
        [AmbientValue("Consuntivata")]
        Consuntivata = 5,
        [IconAttribute("fa-circle-thin circle-red")]
        [Display(Name = "Annullata")]
        [AmbientValue("Annullata")]
        Annullata = 6,
        [IconAttribute("fa-circle circle-red")]
        [Display(Name = "Non convalidata")]
        [AmbientValue("Non convalidata")]
        NonConvalidata = 7
    }

    public enum TipoCestinoEnum
    {
        [Description("ROS")]
        [AmbientValue("PASTO ROSSO")]
        ROS = 1,
        [Description("BIA")]
        [AmbientValue("PASTO BIANCO")]
        BIA = 2,
        [Description("LGH")]
        [AmbientValue("PASTO LIGHT")]
        LGH = 3,
        [Description("LBR")]
        [AmbientValue("PASTO BRESAOLA")]
        LBR = 4,
        [Description("FFO")]
        [AmbientValue("PASTO FORMAGGIO")]
        FFO = 5,
        [Description("VEG")]
        [AmbientValue("PASTO VEGETARIANO")]
        VEG = 6,
        [Description("A10")]
        [AmbientValue("2 ROSETTE FARCITE, 1 INSALATA PICCOLA, 2 FRUTTI, 1 ACQUA")]
        A10 = 7,
        [Description("A20")]
        [AmbientValue("2 PANINI ALL' OLIO FARCITI, 1 INSALATA PICCOLA, 1 MACEDONIA, 1 ACQUA")]
        A20 = 8,
        [Description("A30")]
        [AmbientValue("2 TRANCI DI PIZZA BIANCA FARCITI, 1 INSALATA PICCOLA, 2 FRUTTI, 1 ACQUA")]
        A30 = 9,
        [Description("A40")]
        [AmbientValue("2 TRAMEZZINI, 1 INSALATA PICCOLA, 1 MACEDONIA, 1 ACQUA")]
        A40 = 10,
        [Description("B10")]
        [AmbientValue("1 INSALATA GRANDE, PANE, 2 FRUTTI, 1 ACQUA")]
        B10 = 11,
        [Description("B20")]
        [AmbientValue("1 INSALATA GRANDE, TRANCIO DI PIZZA BIANCA, 2 FRUTTI, 1 ACQUA")]
        B20 = 12,
        [Description("B30")]
        [AmbientValue("1 INSALATA GRANDE, PANE, 1 MACEDONIA, 1 ACQUA")]
        B30 = 13,
        [Description("B40")]
        [AmbientValue("1 INSALATA GRANDE, TRANCIO DI PIZZA BIANCA, 1 MACEDONIA, 1 ACQUA")]
        B40 = 14
    }

    public class Lookup
    {
        public int? Id { get; set; }
        public string Codice { get; set; }
        public string Description { get; set; }
        public bool Selected { get; set; }
    }

	/// <summary>
	/// Tipologica che descrive i possibile destinatari della richiesta cestino
	/// </summary>
	public enum DestinatarioEnum
	{
		[Description( "Me" )]
		[AmbientValue( "Per me" )]
		Me = 1,
		[Description( "Interni" )]
		[AmbientValue( "Per i colleghi" )]
		Interni = 2,
		[Description( "Esterni" )]
		[AmbientValue( "Per esterni" )]
		Esterni = 3
	}

}