using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class SoggiorniArcal
    {
        public StoricoSoggiorni StoricoSoggiorni { get; set; }
    }
    
    public class StoricoSoggiorni
    {
        public List<Soggiorno> SoggiorniRichiesti { get; set; }
        public List<StatoRichiesta> StatiRichieste { get; set; }

        public StoricoSoggiorni()
        {
            SoggiorniRichiesti = new List<Soggiorno>();
            StatiRichieste = new List<StatoRichiesta>();
        }
    }

    public class Soggiorno 
    {
        public int CodeCatalago { get; set; }
        public int CodeRichiesta { get; set; }
        public int CodeScelta { get; set; }
        public string CodeStatusRichiesta { get; set; }
        public string CodeStatusValutazione { get; set; }
        public string NomeCatalago { get; set; }
        public string NomeStruttura { get; set; }         
        public DateTime? InizioSoggiorno { get; set; }
        public DateTime? FineSoggiorno { get; set; }
        public int? NottiSoggiorno { get; set; }
        public List<Partecipante> Partecipanti { get; set; }

        public Soggiorno()
        {
            Partecipanti = new List<Partecipante>();
        }
    }

    public class Partecipante 
    {
        public string Nominativo { get; set; }
        public DateTime? DataDiNascita { get; set; }
        public string CodeProgAlloggio { get; set; }
        public string DescrizAlloggio { get; set; }
    }

    public class StatoRichiesta
    {
        public string StatoRichiestaCode { get; set; }
        public string StatoRichiestaBreve { get; set; }
        public string StatoRichiestaDetail { get; set; }
        public byte[] StatoRichiestaIcona { get; set; }
    }
}