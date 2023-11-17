using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class DatiApprendistaPianoFormativoVM
    {
        public int LivelloTitoloDiStudio { get; set; }
        public string LivelloTitoloDiStudioDes { get; set; }
        public string TitoloDiStudioDes { get; set; }
        public string TitoloDiStudioCod { get; set; }
        public bool Lode { get; set; }
        public string Ambito { get; set; }
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public string Valutazione { get; set; }
        public string Riconoscimento { get; set; }
        public string Citta { get; set; }
        public string Informazioni { get; set; }
        public string Ente { get; set; }
        public string Scala { get; set; }
    }
}
