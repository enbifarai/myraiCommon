using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.Detassazione
{
    public class DetassazioneGraficiModel
    {
        public DetassazioneGraficiModel()
        {
            this.Tot1C = 0;
            this.Tot2C = 0;
            this.TotCompilato1C = 0;
            this.TotCompilato2C = 0;
            this.Percentuale1C = 0;
            this.Percentuale2C = 0;
        }

        public int Tot1C { get; set; }
        public int Tot2C { get; set; }
        public int TotCompilato1C { get; set; }
        public int TotCompilato2C { get; set; }
        public double Percentuale1C { get; set; }
        public double Percentuale2C { get; set; }
    }

    public class DetassazioneRicercaModel
    {
        public int? Modello { get; set; }
        public int? Stato { get; set; }
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public int Pagina { get; set; }
    }

    public class DetassazioneUser
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public bool Completato { get; set; }
        public DateTime? DataCompletamento { get; set; }
        public string TipoModello { get; set; }
    }

    public class DetassazionePaginatore
    {
        public DetassazionePaginatore ( )
        {
            this.ElementiPerPagina = 20;
            this.PaginaCorrente = 1;
            this.PaginaMin = 1;
            this.PaginaSkip = 5;
        }

        public int PaginaCorrente { get; set; }
        public int Pagine { get; set; }
        public int ElementiPerPagina { get; set; }
        public int PaginaMin { get; set; }
        public int PaginaMax { get; set; }
        public int PaginaSkip { get; set; }
    }

    public class DetassazioneListaDipendentiVM
    {
        public DetassazioneListaDipendentiVM ( )
        {
            this.Dipendenti = new List<DetassazioneUser>( );
            this.Paginatore = new DetassazionePaginatore( );
            this.DescrizioneTab = "Ultimi modelli compilati";
            this.Badge = 0;
            this.UtenteAbilitatoModifica = false;
        }

        public bool UtenteAbilitatoModifica { get; set; }
        public List<DetassazioneUser> Dipendenti { get; set; }
        public DetassazionePaginatore Paginatore { get; set; }
        public string DescrizioneTab { get; set; }
        public int Badge { get; set; }
        public DetassazioneRicercaModel Filtri { get; set; }
    }

    public class Pager
    {
        public Pager ( int totalItems , int? page , int pageSize = 20 )
        {
            // calculate total, start and end pages
            var totalPages = ( int ) Math.Ceiling( ( decimal ) totalItems / ( decimal ) pageSize );
            var currentPage = page != null ? ( int ) page : 1;
            var startPage = currentPage - 5;
            var endPage = currentPage + 4;
            if ( startPage <= 0 )
            {
                endPage -= ( startPage - 1 );
                startPage = 1;
            }
            if ( endPage > totalPages )
            {
                endPage = totalPages;
                if ( endPage > 10 )
                {
                    startPage = endPage - 9;
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;
        }

        public int TotalItems { get; private set; }
        public int CurrentPage { get; private set; }
        public int PageSize { get; private set; }
        public int TotalPages { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }
    }

    public class DetassazioneModel
    {
        public string Codice { get; set; }

        public string Nominativo { get; set; }

        public string Matricola { get; set; }

        public string CodiceFiscale { get; set; }

        public string LuogoDiNascita { get; set; }

        public DateTime DataDiNascita { get; set; }

        public string Azienda { get; set; }

        public string Sesso { get; set; }

        public int Anno { get; set; }

        public string CodiceDetassazione { get; set; }
    }
}