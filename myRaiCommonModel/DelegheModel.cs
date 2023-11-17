using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class DelegheModel
    {
        public DelegheModel() {
            ListaDeleghe = new List<Delega>();
        }
        public List<Delega> ListaDeleghe { get; set; }

    }
    public class Delega
    {
        public DateTime Delega_da { get; set; }
        public DateTime Delega_a { get; set; }
        public string MatricolaDelegato { get; set; }
        public string NominativoDelegato { get; set; }
        public Boolean Eliminato { get; set; }
        public string Funzione { get; set; }

    }
}