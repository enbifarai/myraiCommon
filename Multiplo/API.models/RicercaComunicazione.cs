using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplo.API.models.RicercaComunicazione
{
    public class RicercaComunicazioneAPI
    {
        public string CFLavoratore { get; set; }
        public string CFAzienda { get; set; }
        public DateTime dataInizio { get; set; }
        public DateTime dataFine { get; set; }
    }



    public class RicercaComunicazioneResponse
    {
        public List<Comunicazioni> Comunicazioni { get; set; }
        public Esito Esito { get; set; }
    }
    public class Comunicazioni
    {
        public DateTime dataInizioRapporto { get; set; }
        public DateTime dataInvio { get; set; }
        public object dataInizioPeriodo { get; set; }
        public string codComuneStatoEsteroNascitaLavoratore { get; set; }
        public string CFAzienda { get; set; }
        public string CFLavoratore { get; set; }
        public DateTime dataNascitaLavoratore { get; set; }
        public string denominazioneAzienda { get; set; }
        public DateTime dataFineAccordo { get; set; }
        public string codiceComunicazione { get; set; }
        public string nomeLavoratore { get; set; }
        public string cognomeLavoratore { get; set; }
    }

    public class Esito
    {
        public string Messaggio { get; set; }
        public string Linguaggio { get; set; }
        public string Codice { get; set; }
    }

  
}
