using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiHelper.APISW.Models.DettaglioComunicazione
{

    public class DettaglioComunicazioneAPI
    {
        public string CodiceComunicazione { get; set; }
    }


    public class DettaglioComunicazioneResponse
    {
        public Esito Esito { get; set; }
        public Comunicazione Comunicazione { get; set; }
    }
    public class Comunicazione
    {
        public string desTipologiaComunicazione { get; set; }
        public string CFAzienda { get; set; }
        public string codComuneStatoEsteroNascitaLavoratore { get; set; }
        public string codiceIdentificativoPeriodoSmartWorking { get; set; }
        public string denominazioneAzienda { get; set; }
        public string tariffaINAIL { get; set; }
        public object idComunicazionePrecedente { get; set; }
        public DateTime dataInizioRapporto { get; set; }
        public DateTime dataInvio { get; set; }
        public object cFSoggettoAbilitato { get; set; }
        public object codTipoAltroSoggetto { get; set; }
        public string codiceComunicazione { get; set; }
        public string cognomeLavoratore { get; set; }
        public object codiceFiscaleOperatore { get; set; }
        public string desTipologiaRapportoLavoro { get; set; }
        public object idLavoratore { get; set; }
        public object dataInizioPeriodo { get; set; }
        public DateTime dataSottoscrizioneAccordo { get; set; }
        public DateTime dataNascitaLavoratore { get; set; }
        public string tipologiaDurataPeriodo { get; set; }
        public string applicazione { get; set; }
        public string posizioneINAIL { get; set; }
        public DateTime dataFineAccordo { get; set; }
        public string codTipologiaRapportoLavoro { get; set; }
        public string nomeLavoratore { get; set; }
        public string GUIDUtente { get; set; }
        public object mesiDurata { get; set; }
        public int flagUltimo { get; set; }
        public string codTipologiaComunicazione { get; set; }
        public object ricevutaPDF { get; set; }
        public string CFLavoratore { get; set; }
        public object desTipoAltroSoggetto { get; set; }
        public string idAzienda { get; set; }
        public DateTime dataUltimaModifica { get; set; }
        public object streamPDF { get; set; }
    }

    public class Esito
    {
        public string Messaggio { get; set; }
        public string Linguaggio { get; set; }
        public string Codice { get; set; }
    }
}
