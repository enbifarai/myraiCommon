using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiData;

namespace myRaiCommonTasks
{
    public class Avviso
    {
        public string Ambiente { get; set; }
        public string Titolo { get; set; }
        public string Corpo { get; set; }
    }
	public class Liv1Riepilogo
	{
		public string Matricola { get; set; }
		public int DaApprovare { get; set; }
		public string SedeGapp { get; set; }
		public int RichiesteUrg { get; set; }
		public int RichiesteSca { get; set; }
		public int RichiesteOrd { get; set; }
		public int RichiesteUrgStorno { get; set; }
		public int RichiesteScaStorno { get; set; }
		public int RichiesteOrdStorno { get; set; }
	}

	public class SollecitoApprovazione
	{
		public string sedeGapp { get; set; }
		public int RichiesteCount { get; set; }
		public IEnumerable<MyRai_Richieste> Richieste { get; set; }
		public List<string> MatricoleRespLiv1 { get; set; }
	}
}
