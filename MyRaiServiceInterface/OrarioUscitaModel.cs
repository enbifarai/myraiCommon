using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyRaiServiceInterface
{
	[Serializable()]
    public class OrarioUscitaModel
    {
        public string OrarioDiIngresso { get; set; }
        public string OrarioDiUscita { get; set; }
        public bool IngressoRed { get; set; }
        public bool UscitaRed { get; set; }
        public string DicituraSottoOrario { get; set; }
    }
}
