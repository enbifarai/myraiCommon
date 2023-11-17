using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public class SedeGappModel
    {
        public L2D_SEDE_GAPP_Model sedeDB { get; set; }
        public SelectList GiorniList { get; set; }
        public SelectList MensaDisponibileList { get; set; }

        public SedeGappModel()
        {
            List<SelectListItem> L = new List<SelectListItem>();
            L.Add(new SelectListItem() { Text = "Lunedi", Value = "1", Selected = false });
            L.Add(new SelectListItem() { Text = "Martedi", Value = "2", Selected = false });
            L.Add(new SelectListItem() { Text = "Mercoledi", Value = "3", Selected = false });
            L.Add(new SelectListItem() { Text = "Giovedi", Value = "4", Selected = false });
            L.Add(new SelectListItem() { Text = "Venerdi", Value = "5", Selected = false });
            L.Add(new SelectListItem() { Text = "Sabato", Value = "6", Selected = false });
            L.Add(new SelectListItem() { Text = "Domenica", Value = "7", Selected = false });
            GiorniList = new SelectList(L,"Value","Text");


            List<SelectListItem> L2 = new List<SelectListItem>();
            L2.Add(new SelectListItem() { Text = "Pranzo/Cena", Value = "PC", Selected = false });
            L2.Add(new SelectListItem() { Text = "Pranzo", Value = "P", Selected = false });
            L2.Add(new SelectListItem() { Text = "Cena", Value = "C", Selected = false });
            MensaDisponibileList = new SelectList(L2, "Value", "Text");
            
        }
    }
    public class L2D_SEDE_GAPP_Model
    {
        [Required]
        [MaxLength(7)]
        public string CalendarioDiSede { get; set; }

        public string cod_rsu { get; set; }
        public string cod_sede { get; set; }
        public string cod_sede_gapp { get; set; }
        public string cod_serv_cont { get; set; }
        public DateTime? Data_Agg { get; set; }
        public DateTime? Data_Elim { get; set; }
        public DateTime? Data_Fine_Val { get; set; }
        public DateTime? data_fine_validita { get; set; }
        public DateTime? data_inizio_val { get; set; }
        public DateTime? data_inizio_validita { get; set; }
        public DateTime Data_Ins { get; set; }
        public DateTime? Data_Patrono { get; set; }
        public string desc_rsu { get; set; }
        public string desc_sede_gapp { get; set; }
        public string flag_ivt { get; set; }
        public string flag_presenza_sirio { get; set; }
        public string flg_ultimo { get; set; }

        public int? giorno_inizio_settimana { get; set; }

        public int id { get; set; }

        [Required]
        [MaxLength(4)]
        [RegularExpression("\\d{7}", ErrorMessage = "Il parametro richiede 7 cifre con zeri iniziali")]
        public string importo_rimborso { get; set; }

        [Required]
        [MaxLength(9)]
        [RegularExpression("\\d{4}\\/\\d{4}", ErrorMessage = "Il parametro richiede il formato hhmm/hhmm")]
        public string intervallo_mensa { get; set; }

        
        [MaxLength(9)]
        [RegularExpression("\\d{4}\\/\\d{4}", ErrorMessage = "Il parametro richiede il formato hhmm/hhmm")]
        public string intervallo_mensa_serale { get; set; }

        public string mensa_disponibile { get; set; }

        [Required]
        [MaxLength(4)]
        [RegularExpression("\\d{4}", ErrorMessage = "Il parametro richiede 4 cifre con zeri iniziali")]
        public string minimo_car { get; set; }


        public DateTime? partenza_fase_2 { get; set; }
        public DateTime? partenza_fase_3 { get; set; }


       
        [MaxLength(3)]
        [RegularExpression("\\d{2,3}", ErrorMessage = "Il parametro richiede max 3 cifre")]
        public string periodo_mensa { get; set; }

        [MaxLength(9)]
        [RegularExpression("\\d{4}\\/\\d{4}", ErrorMessage = "Il parametro richiede il formato hhmm/hhmm")]
        public string RMTR_intervallo { get; set; }


        public DateTime? scadenza { get; set; }
        public int sky_sede_gapp { get; set; }
    }
}