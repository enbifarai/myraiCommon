using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiHelper.APISW.Models.RicercaComunicazione
{
    public class ComunicazioniRicerca
    {
        public DateTime dataInizioRapporto { get; set; }
        public DateTime dataInvio { get; set; }
        public DateTime dataInizioPeriodo { get; set; }
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

    public class EsitoRicerca
    {
        public string Messaggio { get; set; }
        public string Linguaggio { get; set; }
        public string Codice { get; set; }
    }

    public class RicercaComunicazioniResponse
    {
        public List<ComunicazioniRicerca> Comunicazioni { get; set; }
        public EsitoRicerca Esito { get; set; }
    }
}
