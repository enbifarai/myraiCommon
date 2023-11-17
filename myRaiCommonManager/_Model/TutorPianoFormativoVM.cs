using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class TutorPianoFormativoVM : _IdentityData
    {
        public int IdTutor { get; set; }
        public int Oid { get; set; }
        public string Categoria { get; set; }
        public DateTime? Dal { get; set; }
        public string DalStr { get; set; }
        public DateTime? Al { get; set; }
        public string AlStr { get; set; }
        public string MatricolaTutor { get; set; }
        public string Nota { get; set; }
        public DateTime? AnzCategoria { get; set; }
        public bool IsNew { get; set; }
        public string Cf { get; set; }
        public DateTime? TutorAnzCategoria { get; set; }
        public DateTime? TutorAss { get; set; }
        public string TutorCat { get; set; }
    }
}
