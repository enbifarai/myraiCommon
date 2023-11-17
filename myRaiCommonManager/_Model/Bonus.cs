using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class Bonus100Model
    {
        public string Nominativo { get; set; }

        public string Matricola { get; set; }

        public string CodiceFiscale { get; set; }

        public string LuogoDiNascita { get; set; }

        public string ProvinciaDiNascita { get; set; }

        public DateTime DataDiNascita { get; set; }

        public String Sesso { get; set; }

        public int Anno { get; set; }

        public bool Scelta1 { get; set; }

        public bool Scelta2 { get; set; }

        public bool GiaScelto { get; set; }

        public DateTime? DataCompilazione { get; set; }
    }

    public class DettaglioSceltaDipendente
    {
        public string Matricola { get; set; }

        public string Nominativo { get; set; }

        public DateTime DataScelta { get; set; }

        public string Scelta { get; set; }
    }
}