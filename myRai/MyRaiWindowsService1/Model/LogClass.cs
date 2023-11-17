using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MyRaiWindowsService1.Model
{
	/// <summary>
	/// Definizione dei tipi di log
	/// </summary>
	public enum TipoLogEnum
	{
		[AmbientValue( "Log errori" )]
		LogErrori = 0,

		[AmbientValue( "Log API" )]
		LogApi = 1,

		[AmbientValue( "Log database" )]
		LogDB = 2,

		[AmbientValue( "Log azioni" )]
		LogAzioni = 3
	}
}