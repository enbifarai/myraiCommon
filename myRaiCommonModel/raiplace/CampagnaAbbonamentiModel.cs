using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.raiplace
{
    public class CampagnaAbbonamentiModel
    {
        public int IdCampagna { get; set; }
        public DateTime DataInizioCampagna { get; set; }
        public DateTime DataFineCampagna { get; set; }
        public List<int> idVettore { get; set; }
        public string Vettore { get; set; }
        public bool Modificabile { get; set; }

        public DateTime? DataInizioValidita { get; set; }
    }
}