using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiData;

namespace myRaiCommonModel
{
    public class PianoFerieExt2021Model
    {
        public PianoFerieExt2021Model()
        {
            DatesOff = new List<DateTime>();
            GappDays = new List<GappDay>();
        }
        public List<DateTime> DatesOff { get; set; }
        public int statopf { get;  set; }
        public List<MyRai_PianoFerieGiorni> GiorniPFDB { get;  set; }
        public List<GappDay> GappDays { get; set; }
    }
    public class GappDay
    {
        public DateTime Data { get; set; }
        public string Ecc { get; set; }
    }
}