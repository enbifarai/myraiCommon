using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplo.API.models.ModificaComunicazione
{
    public class ModificaComunicazioneAPI
    {
        public List<ModificaComunicazione> ModificaComunicazione { get; set; }
    }
    public class ModificaComunicazione
    {
        public int id { get; set; }
        public string CodiceComunicazione { get; set; }
        public SezioneRapportoLavoro SezioneRapportoLavoro { get; set; }
        public SezioneAccordoSmartWorking SezioneAccordoSmartWorking { get; set; }
        public SezioneSoggettoAbilitato SezioneSoggettoAbilitato { get; set; }
        public string CodTipologiaComunicazione { get; set; }
    }
    

    public class SezioneAccordoSmartWorking
    {
        public string DataSottoscrizioneAccordo { get; set; }
        public string DataFinePeriodo { get; set; }
        public string TipologiaDurataPeriodo { get; set; }
    }

    public class SezioneRapportoLavoro
    {
        public string CodTipologiaRapportoLavoro { get; set; }
        public string PosizioneINAIL { get; set; }
        public string TariffaINAIL { get; set; }
    }

    public class SezioneSoggettoAbilitato
    {
        public string codTipologiaSoggettoAbilitato { get; set; }
        public string codiceFiscaleSoggettoAbilitato { get; set; }
    }

    public class Esito
    {
        public int idComunicazione { get; set; }
        public string codiceComunicazione { get; set; }
        public string codice { get; set; }
        public string messaggio { get; set; }
        public string linguaggio { get; set; }
    }

    public class ModificaComunicazioneResponse
    {
        public List<Esito> Esito { get; set; }
    }
}
