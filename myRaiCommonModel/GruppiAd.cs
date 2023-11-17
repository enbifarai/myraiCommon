using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class GruppoAd
    {
        public string Nome { get; set; }
        public List<UserAD> Persone { get; set; }
    }

    public class UserAD
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string Mail { get; set; }
        public string Sede { get; set; }
        public string Servizio { get; set; }
    }

    public class UserAdSearch
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string Sede { get; set; }
        public string Servizio { get; set; }
    }
}