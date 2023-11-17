using System;
using System.Collections.Generic;
using System.Linq;
using myRaiData;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public class EventsModel
    {
        public Int32 id { get; set; }
        public ProgramsModel programs { get; set; }
        public string titolo { get; set; }
        public String luogo { get; set; }

        public DateTime dataInizio { get; set; }
        public DateTime dataFine { get; set; }
        public String orarioInizio { get; set; }
        public String orarioFine { get; set; }

        public DateTime dataApertura { get; set; }
        public DateTime dataChiusura { get; set; }
        public String orarioApertura { get; set; }
        public String orarioChiusura { get; set; }

        public Int32 numeroTotale { get; set; }
        public Int32 numeroMassimo { get; set; }

        public string matricola { get; set; }
        public string cod_sede_gapp { get; set; }

        public SelectList listaSedi { get; set; }
        public SelectList listaMatricole { get; set; }
        public SelectList listaEventi { get; set; }

        public Boolean ticket { get; set; }
        public List<B2RaiPlace_Eventi_Evento> elencoEventi { get; set; }

        digiGappEntities db = new digiGappEntities();

        public EventsModel(string titolo = "", string luogo = "", DateTime? data_da = null, DateTime? data_a = null)
        {
            
            var queryListaEventi = (IQueryable<B2RaiPlace_Eventi_Evento>)db.B2RaiPlace_Eventi_Evento;

            if (titolo == "" && luogo == "" && data_da == null && data_a == null)
            {
                queryListaEventi.Where(x => x.id >= 0);
            }
            else
            {
                if (titolo != "")
                { queryListaEventi = queryListaEventi.Where(x => x.titolo.Contains(titolo)); }

                if (luogo != "")
                { queryListaEventi = queryListaEventi.Where(x => x.luogo.Contains(luogo)); }

                if (data_da != null && data_a == null)
                {
                    queryListaEventi = queryListaEventi.Where(x => x.data_inizio >= data_da && x.data_inizio <= DateTime.Now);
                }
                else if (data_da != null && data_a != null)
                {
                    queryListaEventi = queryListaEventi.Where(x => x.data_inizio >= data_da && x.data_inizio <= data_a);
                }
            }


            this.elencoEventi = queryListaEventi.OrderByDescending(x => x.data_inizio).ToList();
        }
    }
}