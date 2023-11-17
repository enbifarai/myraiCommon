using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.Tech
{
	public class ChartLogVM
	{
		public List<Punto> Punti { get; set; }
	}

	public class ChartRichiesteVM
	{
		public List<PuntoRichiesta> Punti { get; set; }
	}

	public class Punto
	{
		// X
		public int Ascissa { get; set; }
		// Y
		public int Ordinata { get; set; }
	}

	public class PuntoRichiesta
	{
		// X
		public string Codice { get; set; }
		// Y
		public int Valore { get; set; }

		public string Tooltip { get; set; }
	}

	public enum ChartLogTypesEnum
	{
		[AmbientValue( "Timeout" )]
		Timeout = 0,
		[AmbientValue( "Errore lettura archivio" )]
		ErrLetturaArchivio = 1
	}

}