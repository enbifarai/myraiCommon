using myRaiCommonModel.raiplace;
using myRaiData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace myRaiCommonModel
{

    public class DisponibilitaRisorse
    {
       public List<DServizio> servizi { get; set; }
       public List<DConProf> dConProfs { get; set; }
       public List<DFiguraPro> dFiguras { get; set; }
  }

    public class RicercaRisorse
    {
        public string Nominativo { get; set; }
        public string Matricola { get; set; }
        public string TipoRicerca { get; set; }
        /*public DisponibilitaRisorse disponibilitaRisorse { get; set; }*/
}

    public class ProduzioniTelevisive
    {
        public List<RicercaProgrammaResult> Programmi { get; set; }
        /*public DisponibilitaRisorse disponibilitaRisorse { get; set; }*/
    }
  
    public class RicercaUnica
    {
        public string Nominativo { get; set; }
        public string Matricola { get; set; }
        [Required(ErrorMessage = "Selezionare il tipo di ricerca")]
        public string RicercaScelta { get; set; }
        public List<SelectListItem> TipoRicerca { get; set; }
        [Required]
        public DateTime DataDal { get; set; }
        [Required]
        public DateTime DataAl { get; set; }
        public string[] ServiziSel { get; set; }
        public List<DServizio> Servizi { get; set; }
        public string Dipendente { get; set; }
        public List<SelectListItem> TipoDipendente { get; set; }
        public string[] ConprofSel { get; set; }
        public List<DConProf> DConProfs { get; set; }
        public string[] FiguraSel { get; set; }
        public List<DFiguraPro> DFiguras { get; set; }
        public string Programma { get; set; }
        public List<RicercaProgrammaResult> Programmi { get; set; }
    }
    public class RisorsaProd
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string Direzione { get; set; }
        public string Figura { get; set; }
        public string dataInizio { get; set; }
        public string dataFine { get; set; }
        public string Disponibile { get; set; }
        public string numProd { get; set; }
        public string percentuale { get; set; }
            }
    public class EsperienzeProduzioneModel
    {
        /* public RicercaRisorse ricercaRisorse { get; set; }
         public DisponibilitaRisorse disponibilitaRisorse { get; set; }
         public ProduzioniTelevisive produzioniTelevisive { get; set; }*/
        public RicercaUnica parametriricerca { get; set; }
        public int numeroRisorse { get; set; }
        public List<RisorsaProd> risorse { get; set; }
    }
}
