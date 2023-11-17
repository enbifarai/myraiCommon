using myRaiHelper;
using System;
using System.Collections.Generic;

namespace myRaiCommonModel
{
    public class PresenzaDipendenti
    {
        public List<PresenzaDipendentiPerSede> ListaDipendenti { get; set; }
        public bool IsPreview { get; set; }

        public string dataInizioDelega { get; set; }
        public string dataFineDelega { get; set; }
    }

    public class PresenzaDipendentiPerSede
    {
        public string SedeGapp { get; set; }
        public string DescrizioneSedeGap { get; set; }
        public List<DipendentePresenzaAssenza> ListaDipendentiPerSede { get; set; }
    }

    public class DipendentePresenzaAssenza
    {
        public string Foto { get; set; }
        public string Nominativo { get; set; }
        public bool Presente { get; set; }
        public string CodTeminalePrimaEntrata { get; set; }
        public string CodTeminaleUltimaEntrata { get; set; }
        public string DescrizioneCodTeminaleUltimaEntrata { get; set; }
        public string CodTeminaleUltimaUscita { get; set; }
        public string EccezioneUno { get; set; }
        public string EccezioneDue { get; set; }
        public string EccezioneTre { get; set; }
        public string OrarioPrimaEntrata { get; set; }
        public string OrarioUltimaEntrata { get; set; }
        public string OrarioUltimaUscita { get; set; }
        public string codiceOrario { get; set; }
        public string formaContratto { get; set; }
        public string matricola { get; set; }
        public string tipoDip { get; set; }
        public string NumeroRichieste { get; set; }
        public string DescrizionePresenzaDipendente { get; set; }
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData { get; set; }
        public Boolean SelezionatoPerDelega { get; set; }
		public string NotaDaSegreteria { get; set; }
        public string CodiceReparto { get; set; }
        public Boolean PertinenzaApprovatore { get; set; }
        public string DescrizioneCodiceOrario { get; set; }
        public string BilancioPOH { get; set; }
        public int POHMeseCorrente { get; set; }
        public Quadratura Quadratura { get; set; }
    }
}