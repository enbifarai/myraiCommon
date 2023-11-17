using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRaiData;
using myRaiDataTalentia;

namespace myRaiCommonModel
{
    using myRaiData.Incentivi;
    using System.ComponentModel.DataAnnotations;
    public class CMINFOANAG_EXTCopy
    {
        [Required(ErrorMessage = "Campo obbligatorio")]
        [Range(0, Int64.MaxValue, ErrorMessage = "Il civico deve contenere solo caratteri numerici")]
        public string CIVICO { get; set; }
        public int ID_EVENTO { get; set; }
        public string COD_CMEVENTO { get; set; }
        public int ID_PERSONA { get; set; }
        public Nullable<int> ID_XR_DATIBANCARI { get; set; }
        public Nullable<System.DateTime> DTA_INIZIO_CC { get; set; }

        [Required(ErrorMessage = "Campo obbligatorio")]
        
        public string COD_IBAN { get; set; }

        public string COD_ABI { get; set; }
        public string COD_CAB { get; set; }
        public Nullable<System.DateTime> DTA_FINE_CC { get; set; }
        public string COD_TIPOCONTO { get; set; }
        public string COD_SUBTPCONTO { get; set; }
        public string COD_STATO { get; set; }
        public string IND_CONGELATO { get; set; }
        public string IND_VINCOLATO { get; set; }
        public string IND_CHANGED { get; set; }
        public string IND_DELETE { get; set; }
       

        [Required(ErrorMessage = "Campo obbligatorio")]
        public string DES_INTESTATARIO { get; set; }
        [Required(ErrorMessage = "Campo obbligatorio")]
        public string COD_UTILIZZO { get; set; }
        public string COD_CONVALIDA_CC { get; set; }
        public string IND_BANCA_ASSENTE { get; set; }
        public string COD_ESITO_EVENTO { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public Nullable<System.DateTime> TMS_TIMESTAMP { get; set; }

        [Required(ErrorMessage = "Campo obbligatorio")]
        [Range(0, Int64.MaxValue, ErrorMessage = "Il prefisso deve contenere solo caratteri numerici")]
        public string DES_PREFISSO { get; set; }
        [Required(ErrorMessage = "Campo obbligatorio")]
        [Range(0, Int64.MaxValue, ErrorMessage = "Il numeri di cellulare deve contenere solo caratteri numerici")]
        public string DES_CELLULARE { get; set; }
        public string TIPO_RECAPITO { get; set; }
        [Required(ErrorMessage = "Campo obbligatorio")]
        public string DES_INDIRRES { get; set; }
        [Required(ErrorMessage = "Campo obbligatorio")]
        public string COD_CITTARES { get; set; }

        [Required(ErrorMessage = "Campo obbligatorio")]
        [MaxLength(5)]
        [MinLength(5)]
        [Range(0, Int64.MaxValue, ErrorMessage = "Il CAP deve contenere solo caratteri numerici")]
        public string CAP_CAPRES { get; set; }
        [Required(ErrorMessage = "Campo obbligatorio")]
        public string DES_INDIRDOM { get; set; }
        [Required(ErrorMessage = "Campo obbligatorio")]
        public string COD_CITTADOM { get; set; }
        [Required(ErrorMessage = "Campo obbligatorio")]
        [MaxLength(5)]
        [MinLength(5)]
        [Range(0, Int64.MaxValue, ErrorMessage = "Il CAP deve contenere solo caratteri numerici")]
        public string CAP_CAPDOM { get; set; }
       
        public Nullable<System.DateTime> DTA_CAMBIO_RES { get; set; }
        public Nullable<System.DateTime> DTA_FINE_RES { get; set; }
        public string IND_ASSEGNA_DOM { get; set; }
        //public string DES_CITTARES { get; set; }
        //public string DES_CITTADOM { get; set; }
        public Nullable<bool> IND_CICS_SENT { get; set; }

        public Nullable<System.DateTime> DTA_CONVALIDA { get; set; }
        public Nullable<System.DateTime> DTA_APPLICAZIONE { get; set; }

        public myRaiData.Incentivi.CMINFOANAG_EXT GetDbObject()
{
    
            myRaiData.Incentivi.CMINFOANAG_EXT result = new myRaiData.Incentivi.CMINFOANAG_EXT()
            {
                COD_ABI = COD_ABI,
                COD_CAB = COD_CAB,
                COD_CMEVENTO = COD_CMEVENTO,
                COD_CONVALIDA_CC = COD_CONVALIDA_CC,
                COD_ESITO_EVENTO = COD_ESITO_EVENTO,
                COD_IBAN = COD_IBAN,
                COD_STATO = COD_STATO,
                COD_SUBTPCONTO = COD_SUBTPCONTO,
                COD_TERMID = COD_TERMID,
                COD_TIPOCONTO = COD_TIPOCONTO,
                COD_USER = COD_USER,
                COD_UTILIZZO = COD_UTILIZZO,
                DES_INTESTATARIO = DES_INTESTATARIO,
                DTA_FINE_CC = DTA_FINE_CC,
                DTA_INIZIO_CC = DTA_INIZIO_CC,
                ID_EVENTO = ID_EVENTO,
                ID_PERSONA = ID_PERSONA,
                ID_XR_DATIBANCARI = ID_XR_DATIBANCARI,
                IND_BANCA_ASSENTE = IND_BANCA_ASSENTE,
                IND_CHANGED = IND_CHANGED,
                IND_CONGELATO = IND_CONGELATO,
                IND_DELETE = IND_DELETE,
                IND_VINCOLATO = IND_VINCOLATO,
                TMS_TIMESTAMP = TMS_TIMESTAMP,
                DES_PREFISSO = DES_PREFISSO,
                DES_CELLULARE = DES_CELLULARE,
                TIPO_RECAPITO = TIPO_RECAPITO,
                DES_INDIRRES = DES_INDIRRES,
                COD_CITTARES = COD_CITTARES,
                CAP_CAPRES = CAP_CAPRES,
                DES_INDIRDOM = DES_INDIRDOM + ", " + CIVICO,
                COD_CITTADOM = COD_CITTADOM,
                CAP_CAPDOM = CAP_CAPDOM,
                DTA_CAMBIO_RES = DTA_CAMBIO_RES,
                DTA_FINE_RES = DTA_FINE_RES,
                IND_ASSEGNA_DOM = IND_ASSEGNA_DOM,
                IND_CICS_SENT = IND_CICS_SENT,
                DTA_CONVALIDA = DTA_CONVALIDA,
                DTA_APPLICAZIONE = DTA_APPLICAZIONE

            };
            return result;
        }
    }
    
    public class ProfiloPersonaleModel
    {
        public Residenza Residenza { get; set; }
        public Domicilio Domicilio { get; set; }
        public Cellulare Cellulare { get; set; }
        public List<ContoCorrente> ContiCorrente { get; set; }
        public List<XR_RECAPITI> Recapiti { get; set; }
        public CMINFOANAG_EXTCopy Contocorrentedb { get; set; }
        public CMINFOANAG_EXTCopy Recapitidb { get; set; }
        public CMINFOANAG_EXTCopy ResDomdb { get; set; }
        public List<ContoCorrenteDaConv> conticorrentidaconv { get; set; }
        public List<CMINFOANAG_EXT> cellularedaconv { get; set; }
        public List<Residenza> residenzadaconv { get; set; }
        public List<Domicilio> domiciliodaconv { get; set; }
        public String AccStip { get; set; }
        public String AntTrasf { get; set; }
        public String AntSpeseProd { get; set; }
        public String CellAziendale { get; set; }
        public string AncheDomicilio { get; set;}
        public myRaiData.Incentivi.XR_SSV_CODICE_OTP codice { get; set; }


        

        public MyRai_InvioNotifiche ImpostazioniDIP_APPR { get; set; }
        public MyRai_InvioNotifiche ImpostazioniDIP_RIF { get; set; }
        public MyRai_InvioNotifiche ImpostazioniDIP_SCAD { get; set; }


        public MyRai_InvioNotifiche ImpostazioniL1_INSR { get; set; }
        public MyRai_InvioNotifiche ImpostazioniL1_INSS { get; set; }
        public MyRai_InvioNotifiche ImpostazioniL1_URG { get; set; }
        public MyRai_InvioNotifiche ImpostazioniL1_SCAD { get; set; }
        public Boolean RichiestaDaConvalidareFound { get; set; }
        public Boolean CodiceNonValido { get; set; }
        public Boolean ModificaDomSospesa { get; set; }
        public Boolean RichiestaConvalidata { get; set; }


        public SelectList ListaOre { get; set; }
        public string OreDipAppr { get; set; }
        public string OreDipRif { get; set; }
        public string OreDipScad { get; set; }
        public SelectList ListaMinuti { get; set; }
        public string MinDipAppr { get; set; }
        public string MinDipRif { get; set; }
        public string MinDipScad { get; set; }

        public string OreL1Insr { get; set; }
        public string MinL1Insr { get; set; }

        public string OreL1Inss { get; set; }
        public string MinL1Inss { get; set; }

        public string OreL1Scad { get; set; }
        public string MinL1Scad { get; set; }

        public string OreL1Urg { get; set; }
        public string MinL1Urg { get; set; }


        public string JSlines { get; set; }
            }


    public class Cellulare {
        public string prefisso { get; set; }
        public string numero { get; set; }
        public string tipologia { get; set; }
    }



    public class Residenza
    {
        public string Indirizzo { get; set; }
        public string cap { get; set; }
        public string citta { get; set; }
        public string prov { get; set; }
        public string stato { get; set; }
        public string anchedom { get; set; }
        public int ID_EVENTO { get; set; }
    }



    public class Domicilio
    {
        public string Indirizzo { get; set; }
        public string cap { get; set; }
        public string citta { get; set; }
        public string prov { get; set; }
        public string stato { get; set; }
        public int ID_EVENTO { get; set; }

    }

    public class ContoCorrente
    {
        public string Tipologia { get; set; }
        public string Iban { get; set; }
        public string Agenzia { get; set; }
        public string Indirizzo_Agenzia { get; set; }
        public string Citta_Agenzia { get; set; }
        public string Attivo_dal { get; set; }
        public string flag_vincolato { get; set; }
        public string flag_congelato { get; set; }

        public string Codice_Utilizzo { get; set; }
    }



    public class ContoCorrenteDaConv
    {
        public myRaiData.Incentivi.CMINFOANAG_EXT contocDB;
        public ContoCorrente ccVis;

    }
    
}