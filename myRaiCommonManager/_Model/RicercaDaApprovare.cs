using System.Collections.Generic;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public class RicercaDaApprovare
    {
		public RicercaDaApprovare ()
		{
			this.Visualizzato = -1;
		}

        public SelectList ListaSede { get; set; }
        public List<SelectListItem> listaSedi { get; set; }
        public List<SelectListItem> listaOpzioniRicercaVisti { get; set; }
        public SelectList ListaidStato { get; set; }
        public SelectList ListaEccezione { get; set; }
		public SelectList ListVisualizzato { get; set; }
        public SelectList ListLivelloDip { get; set; }

        public int id_stato { get; set; }
        public string Nominativo { get; set; }
        public string Sede { get; set; }
        public string OpzioneRicercaVisti { get; set; }
        public string cod_eccezione { get; set; }
        public string data1 { get; set; }
        public string data2 { get; set; }
		public int Visualizzato { get; set; }
        public string LivelloDip { get; set; }

        
    }

    public class RicercaDaApprovareAttivita
    {
        public RicercaDaApprovareAttivita()
        {
            this.ListaEccezioniCeiton = new List<SelectListItem>( );
            this.ListaStati = new List<SelectListItem>();
            this.ListaTitoli = new List<SelectListItem>( );
            this.ListaTitoliRecenti = new List<SelectListItem>( );
        }

        public string Nominativo { get; set; }

        public List<SelectListItem> ListaStati { get; set; }

        public string stato { get; set; }

        public List<SelectListItem> ListaTitoli { get; set; }

        public string titolo { get; set; }

        public List<SelectListItem> ListaEccezioniCeiton { get; set; }

        public string eccezione { get; set; }

        public List<SelectListItem> ListaTitoliRecenti { get; set; }
    }
}