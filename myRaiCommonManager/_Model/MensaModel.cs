using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class MensaModel
    {
        public DateTime mese { get; set; }
        public List<DatiScontrino> elencoScontrini { get; set; }
        public string quantiPasti { get; set; }
        public double totalePrezzoPasti { get; set; }
    }

    public class DatiScontrino
    {
        public string dataScontrino { get; set; }
        public string oraScontrino { get; set; }
        public string idScontrino { get; set; }
        public string mensa { get; set; }
        public List<Pasto> pasti { get; set; }
        public string prezzoTotale { get; set; }

    }
    public class Pasto
    {
        public string descrizione { get; set; }
        public string prezzo { get; set; }

    }
}