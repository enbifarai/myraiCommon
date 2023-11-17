using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public class FormPrimario
    {
        public FormPrimario()
        {
            var db = new myRaiData.digiGappEntities();
            var list = new List<SelectListItem>();
            foreach (var item in db.MyRai_FormTipologiaForm)
            {
                list.Add(new SelectListItem()
                {
                    Selected = false,
                    Text = item.tipologia,
                    Value = item.id.ToString()
                });
            }
            this.tipologia_list = new SelectList(list, "Value", "Text");
        }
        public int id { get; set; }

        [Required]
        public string titolo { get; set; }

      
        public string descrizione { get; set; }

        [Required]
        public string data_inizio_validita { get; set; }

        [Required]
        public string data_fine_validita { get; set; }

        [Required]
        public int id_tipologia { get; set; }

        public Boolean anonimo { get; set; }
        public Boolean barra_avanzamento { get; set; }
        public Boolean precompilati_ammessi { get; set; }
        public Boolean attivo { get; set; }
        public Boolean vedi_statistiche_dopo_comp { get; set; }
        public Boolean invia_mail_dopo_comp { get; set; }
        public string mail_oggetto { get; set; }
        public string mail_corpo { get; set; }

        public string filtro_gruppo { get; set; }
        public string filtro_matricola { get; set; }
        public string messaggio_fine_validita { get; set; }
        public string azione_fine_validita { get; set; }

        public string messaggio_feedback { get; set; }

        public SelectList tipologia_list { get; set; }

    }
    public class FormSecondario
    {
        [Required]
        public string titolo { get; set; }

        public int Progressivo { get; set; }
        public string descrizione { get; set; }

        public Boolean attivo { get; set; }
        public string titolo_form_primario { get; set; }
        public int id_form_primario { get; set; }
        
        public int id { get; set; }

    }
    public class FormDomanda
    {
        public FormDomanda()
        {
            var db = new myRaiData.digiGappEntities();
            var list = new List<SelectListItem>();
            foreach (var item in db.MyRai_FormTipologieDomande)
            {
                list.Add(new SelectListItem()
                {
                    Selected = false,
                    Text = item.tipologia,
                    Value = item.id.ToString()
                });
            }
            this.tipologia_list = new SelectList(list, "Value", "Text");

           
        }

        public int id { get; set; }
        public int id_form_secondario { get; set; }

        [Required]
        public int? id_domanda_parent { get; set; }

        public int Progressivo { get; set; }
        public string titolo_form_primario { get; set; }
        public string titolo_form_secondario { get; set; }

        public Boolean PrevedeSceltaRisposta { get; set; }
        public Boolean PermettiAltro { get; set; }

        [Required]
        public string titolo { get; set; }

    
        public string descrizione { get; set; }

        [Required]
        public int id_tipologia { get; set; }

        public Boolean attiva { get; set; }
        public Boolean obbligatoria { get; set; }
        public SelectList tipologia_list { get; set; }

        public SelectList DomandeMasterDisponibili_list { get; set; }
       

        public string[] risposte { get; set; }
        public int? max_scelte { get; set; }
        
    }

}