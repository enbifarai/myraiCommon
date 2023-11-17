using myRaiData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public class EventsClientModel
    {
        public EventsClientModel(string sedeAppartenenza)
        {
            anagraficaModel = new AnagraficaPrenotazone(sedeAppartenenza);
        }
        public string ideventoAperturaRemota { get; set; }
        public string ReturnURL { get; set; }
        public Boolean PopupAutoOpen { get; set; }
        public int timeoutMinuti { get; set; }
        public List<EventoDisponibile> EventiDisponibili { get; set; }

        public List<myRaiData.B2RaiPlace_Eventi_Evento> EventiPrenotati { get; set; }
        public AnagraficaPrenotazone anagraficaModel { get; set; }
        public string matricola;
    }

    public class EventoDisponibile
    {
        public myRaiData.B2RaiPlace_Eventi_Evento EventoDisp { get; set; }
        public int PostiRimasti { get; set; }
        public int PostiDisponibili { get; set; }
        public TimeSpan TempoRimasto { get; set; }
        public bool GeneraPdf { get; set; }
    }
    public class InfoEvento
    {
        public myRaiData.B2RaiPlace_Eventi_Evento Evento { get; set; }
        public string matricola { get; set; }
        public int PrenotazioniStessoProgramma { get; set; }
        
    }

    public class AnagraficaPrenotazone
    {
        private List<SelectListItem> GetSediInsediamento(string sedeAppartenenza)
        {
            List<SelectListItem> listSediInsediamento = new List<SelectListItem>();

            List<string> sediAnagrafate = new List<string>()
            {
                "ROMA","MILANO","TORINO","NAPOLI",
                "ANCONA","AOSTA",
                "BARI","BOLOGNA","BOLZANO",
                "CAMPOBASSO","CAGLIARI","COSENZA",
                "FIRENZE","GENOVA",
                "PALERMO","POTENZA","PERUGIA","PESCARA",
                "TRENTO","TRIESTE"
            };

            if (!sediAnagrafate.Contains(sedeAppartenenza))
                sedeAppartenenza = "*";

            if (sedeAppartenenza == "ROMA" || sedeAppartenenza=="*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem> {
                    new SelectListItem { Selected = false, Text = "Saxa Rubra - L.go V. de Luca, 4", Value = "Saxa Rubra - L.go V. de Luca, 4"},
                    new SelectListItem { Selected = false, Text = "V.le Mazzini, 14", Value = "V.le Mazzini, 14"},
                    new SelectListItem { Selected = false, Text = "Via Teulada, 66", Value = "Via Teulada, 66"},
                    new SelectListItem { Selected = false, Text = "Via Asiago, 10", Value = "Via Asiago, 10"},
                    new SelectListItem { Selected = false, Text = "B.go S. Angelo, 23", Value = "B.go S. Angelo, 23"},
                    new SelectListItem { Selected = false, Text = "Audit. Foro Italico", Value = "Audit. Foro Italico"},
                    new SelectListItem { Selected = false, Text = "C.ne Clodia, 165 int.4/A", Value = "C.ne Clodia, 165 int.4/A"},
                    new SelectListItem { Selected = false, Text = "C.ne Clodia, 80", Value = "C.ne Clodia, 80"},
                    new SelectListItem { Selected = false, Text = "CSS1 - Via Salaria, 1041-43", Value = "CSS1 - Via Salaria, 1041-43"},
                    new SelectListItem { Selected = false, Text = "CSS2 - Via Salaria, 1031", Value = "CSS2 - Via Salaria, 1031"},
                    new SelectListItem { Selected = false, Text = "L.go Font. Borghese, 77", Value = "L.go Font. Borghese, 77"},
                    new SelectListItem { Selected = false, Text = "P.le Clodio, 56", Value = "P.le Clodio, 56"},
                    new SelectListItem { Selected = false, Text = "P.zza Adriana 15", Value = "P.zza Adriana 15"},
                    new SelectListItem { Selected = false, Text = "P.zza Adriana, 12", Value = "P.zza Adriana, 12"},
                    new SelectListItem { Selected = false, Text = "Quirinale", Value = "Quirinale"},
                    new SelectListItem { Selected = false, Text = "T. d. Vittorie", Value = "T. d. Vittorie"},
                    new SelectListItem { Selected = false, Text = "Tor di Quinto", Value = "Tor di Quinto"},
                    new SelectListItem { Selected = false, Text = "V.le Angelico, 54", Value = "V.le Angelico, 54"},
                    new SelectListItem { Selected = false, Text = "V.le Giulio Cesare, 6", Value = "V.le Giulio Cesare, 6"},
                    new SelectListItem { Selected = false, Text = "V.le Mazzini, 114", Value = "V.le Mazzini, 114"},
                    new SelectListItem { Selected = false, Text = "V.le Mazzini, 6", Value = "V.le Mazzini, 6"},
                    new SelectListItem { Selected = false, Text = "Via A. Papa, 11", Value = "Via A. Papa, 11"},
                    new SelectListItem { Selected = false, Text = "Via Asiago, 1", Value = "Via Asiago, 1"},
                    new SelectListItem { Selected = false, Text = "Via Asiago, 3-7", Value = "Via Asiago, 3-7"},
                    new SelectListItem { Selected = false, Text = "Via Asiago, 6", Value = "Via Asiago, 6"},
                    new SelectListItem { Selected = false, Text = "Via Asiago, 8", Value = "Via Asiago, 8"},
                    new SelectListItem { Selected = false, Text = "Via Cadlolo, 90", Value = "Via Cadlolo, 90"},
                    new SelectListItem { Selected = false, Text = "Via Castelgomberto, 13", Value = "Via Castelgomberto, 13"},
                    new SelectListItem { Selected = false, Text = "Via Col di Lana, 11", Value = "Via Col di Lana, 11"},
                    new SelectListItem { Selected = false, Text = "Via Col di Lana, 8", Value = "Via Col di Lana, 8"},
                    new SelectListItem { Selected = false, Text = "Via E. Romagnoli, 1", Value = "via E. Romagnoli, 1"},
                    new SelectListItem { Selected = false, Text = "Via Goiran 3", Value = "Via Goiran 3"},
                    new SelectListItem { Selected = false, Text = "Via Goiran, 14-16", Value = "Via Goiran, 14-16"},
                    new SelectListItem { Selected = false, Text = "Via Gomenizza, 9", Value = "Via Gomenizza, 9"},
                    new SelectListItem { Selected = false, Text = "Via Monte Santo, 52", Value = "Via Monte Santo, 52"},
                    new SelectListItem { Selected = false, Text = "Via Monte Santo, 68", Value = "Via Monte Santo, 68"},
                    new SelectListItem { Selected = false, Text = "Via Monte Zebio, 25", Value = "Via Monte Zebio, 25"},
                    new SelectListItem { Selected = false, Text = "Via Monte Zebio, 28", Value = "Via Monte Zebio, 28"},
                    new SelectListItem { Selected = false, Text = "Via Muggia, 21", Value = "Via Muggia, 21"},
                    new SelectListItem { Selected = false, Text = "Via Novaro, 18", Value = "Via Novaro, 18"},
                    new SelectListItem { Selected = false, Text = "Via Oslavia, 12", Value = "Via Oslavia, 12"},
                    new SelectListItem { Selected = false, Text = "Via Oslavia, 40", Value = "Via Oslavia, 40"},
                    new SelectListItem { Selected = false, Text = "Via Pasubio, 11", Value = "Via Pasubio, 11"},
                    new SelectListItem { Selected = false, Text = "Via Pasubio, 2-4", Value = "Via Pasubio, 2-4"},
                    new SelectListItem { Selected = false, Text = "Via S. Pellico, 10", Value = "Via S. Pellico, 10"},
                    new SelectListItem { Selected = false, Text = "Via Sambuca Pistoiese, 51-53", Value = "Via Sambuca Pistoiese, 51-53"}
                });
            }
            
            if (sedeAppartenenza == "MILANO" || sedeAppartenenza=="*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Milano - C.so Sempione, 27", Value = "C.so Sempione, 27"},
                    new SelectListItem { Selected = false, Text = "Milano - via Mecenate, 76", Value = "Via Mecenate, 76"},
                });
            }

            if (sedeAppartenenza == "TORINO" || sedeAppartenenza=="*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Torino - via Cavalli, 6", Value = "Via Cavalli, 6"},
                    new SelectListItem { Selected = false, Text = "Torino - via Verdi,14/16", Value = "Via Verdi,14/16"},
                    new SelectListItem { Selected = false, Text = "Torino - via Verdi, 31", Value = "Via Verdi, 31"},
                });
            }

            if (sedeAppartenenza == "NAPOLI" || sedeAppartenenza=="*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Napoli - v. G. Marconi, 9", Value = "Via G. Marconi, 9"},
                });
            }

            if (sedeAppartenenza == "ANCONA" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per le Marche", Value = "Ancona"},
                });
            }

            if (sedeAppartenenza == "AOSTA" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per la Valle d'Aosta", Value = "Aosta"},
                });
            }

            if (sedeAppartenenza == "BARI" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per la Puglia", Value = "Bari"},
                });
            }

            if (sedeAppartenenza == "BOLOGNA" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per l'Emilia Romagna", Value = "Bologna"},
                });
            }

            if (sedeAppartenenza == "BOLZANO" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede di Bolzano", Value = "Bolzano"},
                });
            }

            if (sedeAppartenenza == "CAMPOBASSO" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per il Molise", Value = "Campobasso"},
                });
            }

            if (sedeAppartenenza == "CAGLIARI" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per la Sardegna", Value = "Cagliari"},
                });
            }

            if (sedeAppartenenza == "COSENZA" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per la Calabria", Value = "Cosenza"},
                });
            }

            if (sedeAppartenenza == "FIRENZE" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per la Toscana", Value = "Firenze"},
                });
            }

            if (sedeAppartenenza == "GENOVA" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per la Liguria", Value = "Genova"},
                });
            }

            if (sedeAppartenenza == "PALERMO" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per la Sicilia", Value = "Palermo"},
                });
            }

            if (sedeAppartenenza == "POTENZA" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per la Basilicata", Value = "Potenza"},
                });
            }

            if (sedeAppartenenza == "PERUGIA" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per l'Umbria", Value = "Perugia"},
                });
            }
            
            if (sedeAppartenenza == "PESCARA" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per l'Abruzzo", Value = "Pescara"},
                });
            }
            
            if (sedeAppartenenza == "TRENTO" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede di Trento", Value = "Trento"},
                });
            }

            if (sedeAppartenenza == "TRIESTE" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per il Friuli-Venezia Giulia", Value = "Trieste"},
                });
            }

            if (sedeAppartenenza == "VENEZIA" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Sede Regionale per il Veneto", Value = "Venezia"},
                });
            }

            //Rai Pubblicità
            if (sedeAppartenenza == "MILANO" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Rai Pubblicità - Milano", Value = "Rai Pubblicità - Milano"},
                });
            }
            if (sedeAppartenenza == "ROMA" || sedeAppartenenza == "*")
            {
                listSediInsediamento.AddRange(new List<SelectListItem>()
                {
                    new SelectListItem { Selected = false, Text = "Rai Pubblicità - Roma", Value = "Rai Pubblicità - Roma"},
                });
            }


            return listSediInsediamento;
        }

        public AnagraficaPrenotazone()
        {

        }

        public AnagraficaPrenotazone(string sedeAppartenenza)
        {
            var list = new List<SelectListItem>
                {
                    new SelectListItem { Selected = false, Text = "M", Value = "M"},
                    new SelectListItem { Selected = false, Text = "F", Value = "F"},
                };
            var list_grado = new List<SelectListItem>
                {
                    new SelectListItem { Selected = false, Text = "Padre", Value = "Padre"},
                    new SelectListItem { Selected = false, Text = "Madre", Value = "Madre"},
                    new SelectListItem { Selected = false, Text = "Sorella", Value = "Sorella"},
                    new SelectListItem { Selected = false, Text = "Fratello", Value = "Fratello"},
                    new SelectListItem { Selected = false, Text = "Figlio", Value = "Figlio"},
                    new SelectListItem { Selected = false, Text = "Figlia", Value = "Figlia"},
                    new SelectListItem { Selected = false, Text = "Coniuge", Value = "Coniuge"},
                    new SelectListItem { Selected = false, Text = "Altro", Value = "Altro"},
                };
            this.genere_list = new SelectList(list, "Value", "Text");
            this.grado_list = new SelectList(list_grado, "Value", "Text");

            var listSediInsediamento = GetSediInsediamento(sedeAppartenenza);
            
            this.specificaInsediamento_list = new SelectList(listSediInsediamento, "Value", "Text");

        }
        public int idevento { get; set; }
        public string nome { get; set; }
        public string cognome { get; set; }
        public string datanascita { get; set; }
        public string citta { get; set; }
        public string genere { get; set; }
        public string grado { get; set; }
        public string specificaInsediamento { get; set; }
        public string tipoDocumento { get; set; }
        public string numeroDocumento { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
        public int idPrenotazione { get; set; }
        public SelectList genere_list { get; set; }
        public SelectList grado_list { get; set; }
        public SelectList specificaInsediamento_list { get; set; }
        public bool sedeInsediamento { get; set; }
        public string nota { get; set; }

    }

    public class EventiPrenotatiVM
    {
        public EventiPrenotatiVM()
        {
            this.Eventi = new List<EventoPrenotato>();
        }

        public List<EventoPrenotato> Eventi { get; set; }
    }

    public class EventoPrenotato
    {
        public int Id { get; set; }
        public DateTime DataEvento { get; set; }
        public int? IdProgramma { get; set; }
        public string Luogo { get; set; }
        public string Titolo { get; set; }
        public int? TotalePostiPrenotati { get; set; }
    }

    public class Prenotati : EventoPrenotato
    {
        public string Matricola { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Dipendente { get; set; }
        public string Telefono { get; set; }
        public DateTime DataNascita { get; set; }
        public string Mail { get; set; }
        public string SedeInsediamento { get; set; }
        public string Citta { get; set; }
        public string Genere { get; set; }
        public string Grado { get; set; }
        public string Nota { get; set; }
    }
    public class EventsPDFmodel
    {
        public int id { get; set; }
        public int idEvento { get; set; }
        public string NomeEvento { get; set; }
        public string matricola { get; set; }
        public DateTime dataEvento { get; set; }
        public List<ListaPrenotati> listaPrenotati { get; set; }

    }
    public class ListaPrenotati
    {
        public string Nome { get; set; }
        public string Cognome { get; set; }
    }
}