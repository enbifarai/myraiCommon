using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiHelper.APISW.Models.AnnullaComunicazione
{
    public class AnnullaComunicazioneAPI
    {
        public List<AnnullaComunicazione> AnnullaComunicazione { get; set; }
    }
    public class AnnullaComunicazione
    {
        public int id { get; set; }
        public string CodiceComunicazione { get; set; }
        public string CodTipologiaComunicazione { get; set; }
    }




    public class AnnullaComunicazioneResponse
    {
        public List<Esito> Esito { get; set; }
    }
    public class Esito
    {
        public int idComunicazione { get; set; }
        public string codiceComunicazione { get; set; }
        public string codice { get; set; }
        public string messaggio { get; set; }
        public string linguaggio { get; set; }
    }
}
