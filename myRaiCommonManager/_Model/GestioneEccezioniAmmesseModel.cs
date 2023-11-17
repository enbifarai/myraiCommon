


using myRaiHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace myRaiCommonModel
{


    public class GestioneParametriModel
    {
        public List<ParametroDiSistema> ListaParametri { get; set; }
        public sidebarModel SidebarModel { get; set; }

        public long ParametroId { get; set; }
        public string ParametroChiave { get; set; }
        public string ParametroValore1 { get; set; }
        public string ParametroValore2 { get; set; }
    }
    public class ParametroDiSistema
    {
        public long id { get; set; }
        public string nomeparametro { get; set; }
        public Boolean selected { get; set; }
    }





    public class GestioneEccezioniAmmesseModel
    {
        public EccezioneAmmessaModel EccezioneAmmessa { get; set; }
        public List<EccezioneShort > ListaEccezioni {get;set;}

        public SelectList StatoGiornataList { get; set; }

        public SelectList RaggruppamentiList { get; set; }

        public SelectList WorkflowsList { get; set; }

        public sidebarModel SidebarModel {get;set;}

        public Boolean SoloConValidazioneJS { get; set; }
        public Boolean SoloConValidazioneCsharp { get; set; }

        public int? idEcc { get; set; }

    }


    public class EccezioneShort 
    {
        public string DescEccezione {get;set;}
        public int id {get;set;}
        public Boolean selected { get; set; }
        public Boolean validazSelected { get; set; }
    }

    public class EccezioneAmmessaModel
    {

       

        public int id { get; set; }

        [Required]
        [StringLength(4)]
        public string cod_eccezione { get; set; }

        [Required]
        [StringLength(25)]
        public string desc_eccezione { get; set; }

   
        public Nullable<System.DateTime> data_inizio_validita { get; set; }

     
        public Nullable<System.DateTime> data_fine_validita { get; set; }
        public bool flag_attivo { get; set; }

         [StringLength(4)]
        public string tipo_controllo { get; set; }

        [Required]
        public Nullable<int> id_raggruppamento { get; set; }

        [StringLength(1)]
        public string periodo { get; set; }

        [StringLength(1000)]
        public string controlli_specifici { get; set; }

        public int OreInPassato { get; set; }

        public int OreInFuturo { get; set; }

        [StringLength(100)]
        public string PartialView { get; set; }

        public string FunzioneJS { get; set; }

        [StringLength(100)]
        public string TipiDipendente { get; set; }
        
        public string ValoriParamExtraJSON { get; set; }

        [StringLength(10)]
        public string Categoria { get; set; }

        [StringLength(10)]
        public string OrarioPrevisto { get; set; }

        [StringLength(10)]
        public string OrarioEffettivo { get; set; }

        [StringLength(10)]
        public string TipoGiornata { get; set; }

        public Nullable<int> CaratteriMotivoRichiesta { get; set; }

        [StringLength(10)]
        public string StatoGiornata { get; set; }

        public bool Prontuario { get; set; }

        [StringLength(100)]
        public string TipiDipendenteEsclusi { get; set; }

        [Required]
        [Display(Name="Tipo di workflow")]
        public int id_workflow { get; set; }

        [StringLength(1000)]
        public string descrizione_eccezione { get; set; }


        public string CodiceCsharp { get; set; }
    }
}