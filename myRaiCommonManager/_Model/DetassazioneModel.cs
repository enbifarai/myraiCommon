using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class DetassazioneModel
    {
        public string Codice { get; set; }

        public string Nominativo { get; set; }

        public string Matricola { get; set; }

        public string CodiceFiscale { get; set; }

        public string LuogoDiNascita { get; set; }

        public DateTime DataDiNascita { get; set; }

        public string Azienda { get; set; }

        public string Sesso { get; set; }

        public int Anno { get; set; }

        public string CodiceDetassazione { get; set; }
    }
}