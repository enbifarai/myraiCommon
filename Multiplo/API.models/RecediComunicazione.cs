using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplo.API.models
{
    public class RecediComunicazioneAPI
    {
        public List<RecediComunicazione> RecediComunicazione { get; set; }
    }
    public class RecediComunicazione
    {
        public int id { get; set; }
        public string CodiceComunicazione { get; set; }
        public string DataFinePeriodo { get; set; }
        public SezioneSoggettoAbilitato SezioneSoggettoAbilitato { get; set; }
        public string CodTipologiaComunicazione { get; set; }
    }

    public class SezioneSoggettoAbilitato
    {
        public string codTipologiaSoggettoAbilitato { get; set; }
    }




    public class RecediComunicazioneResponse
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
