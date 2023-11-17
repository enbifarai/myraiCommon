using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace myRaiCommonModel.raiplace
{
    public class MyAbbonamenti
    {
        public string CittaAbbonamento { get; set; }
        public List<AbbonamentiModel> Abbonamenti { get; set; }
        public bool CreaAbbonamento { get; set; }
        public bool InviaMail { get; set; }
        public bool RateUnicaSoluzione { get; set; }
        public string VettoreAbbonamento { get; set; }
    }
    public class AbbonamentiModel
    {
        public int idAbbonamento { get; set; }
        public string Citta { get; set; }
        public string Matricola { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        [DisplayName("Data di Nascita")]
        public DateTime DataNascita { get; set; }
        public string Genere { get; set; }
        [DisplayName("Codice Fiscale")]
        public string CodiceFiscale { get; set; }
        public string TipologiaDocumento { get; set; }
        public int IdTipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string EnteRilascioDocumento { get; set; }
        public DateTime? DataRilascioDocumento { get; set; }
        public string Indirizzo { get; set; }
        public string Cap { get; set; }
        public string Comune { get; set; }
        public string Provincia { get; set; }
        [DisplayName("Nazionalità")]
        public string Nazionalita { get; set; }
        [DisplayName("E-mail")]
        public string Email{ get; set; }
        public string Telefono { get; set; }
        public string Cellulare { get; set; }
        public string VettoreDiAbbonamento { get; set; }
        public int IdVettoreAbbonamento { get; set; }
        public string UltimeCifreAbbonamento { get; set; }
        public string CodiceAbbonamento { get; set; }
        public string IdZonaAbbonamento { get; set; }
        public string ZonaAbbonamento { get; set; }
        public string TipologiaAbbonamento { get; set; }
        public DateTime GiornoInizio { get; set; }
        public DateTime GiornoFine { get; set; }
        public string Classe { get; set; }
        public int NumeroRate { get; set; }
        public string PercorsoDa { get; set; }
        public string PercorsoA { get; set; }
        public string CittaAbbonamento { get; set; }
        public bool Rinnovo { get; set; }
        public bool Approvata { get; set; }
        public bool Policy { get; set; }
        public string ComuneNascita { get; set; }
        public string ProvinciaNascita { get; set; }
        public string NumeroBipCard { get; set; }
    }
}