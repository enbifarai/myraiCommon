using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class ModelPianoFormativoForWord
    {
        //Azienda
        public string RagSoc { get; set; }
        public string IndirizzoSoc { get; set; }
        public string CapSoc { get; set; }
        public string IvaSoc { get; set; }
        public string CFSoc { get; set; }
        public string TelSoc { get; set; }
        public string FaxSoc { get; set; }
        public string MailSoc { get; set; }
        public string RapLegSoc { get; set; }
        //Apprendista 
        public string Cognome { get; set; }
        public string Nome { get; set; }
        public string CF { get; set; }
        public string Cittadina { get; set; }
        public string CittaNasc { get; set; }
        public DateTime DataNasc { get; set; }
        public string CittaDom { get; set; }
        public string Prov { get; set; }
        public string Indirizzo { get; set; }
        public string Telefono { get; set; }
        public string Cellulare { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        //Aspetti normativi
        public DateTime DataAss { get; set; }
        public string ProForm { get; set; }
        public string Durata { get; set; }
        public string CodQual { get; set; }
        public string TipoMinimo { get; set; }
        //Studi
        public/* List<StudioModel> */ string Studi { get; set; }
        //Tutor
        public string Tutor { get; set; }
        public string TutorCF { get; set; }
        public string TutorCat { get; set; }
        public DateTime TutorAss { get; set; }
        public DateTime TutorAnzCat { get; set; }
        //Contenuti Formativi
        public List<Competenze> Requisiti { get; set; }

    }
}
