using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
	/// <summary>
	/// Classe che rappresenta l'oggetto utilizzato dalle views di jobposting
	/// </summary>
	public class JobPostingModel
	{
		public string Utente { get; set; }
		public string IdJob { get; set; }
		public string Matricola { get; set; }
		public string Produzione { get; set; }
		public string NumeroJP { get; set; }
	}
}