using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.SituazioneDebitoria.ESS
{
	/// <summary>
	/// Classe che rappresenta il modello utilizzato dall'interfaccia Situazione Debitoria
	/// </summary>
	public class SituazioneDebitoriaModel
	{
		/// <summary>
		/// Nome della compagnia.
		/// Questo dato verrà rappresentato in grassetto nella tabella
		/// </summary>
		public string Descrizione { get; set; }

		/// <summary>
		/// Importo totale del debito
		/// </summary>
		public double Addebito { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string MeseDa { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string MeseA { get; set; }

		/// <summary>
		/// Importo della singola rata
		/// </summary>
		public double ImportoRata { get; set; }

		/// <summary>
		/// Numero di rate del contratto
		/// </summary>
		public int NumeroRate { get; set; }

		/// <summary>
		/// Importo rimanente
		/// </summary>
		public double ImportoRateResidue { get; set; }

		/// <summary>
		/// Numero di rate rimanenti
		/// </summary>
		public int NumeroRateResidue { get; set; }

		public int IntMeseDa { get; set; }
		public int IntMeseA { get; set; }
		public int AnnoDa { get; set; }
		public int AnnoA { get; set; }
	}

	public class SituazioneDebitoriaVM
	{
		public List<SituazioneDebitoriaModel> Data { get; set; }

		public DateTime CurrentDate { get; set; }
	}

	public enum MesiEnum
	{
		[AmbientValue( "Gennaio" )]
		Gennaio = 1,
		[AmbientValue( "Febbraio" )]
		Febbraio = 2,
		[AmbientValue( "Marzo" )]
		Marzo = 3,
		[AmbientValue( "Aprile" )]
		Aprile = 4,
		[AmbientValue( "Maggio" )]
		Maggio = 5,
		[AmbientValue( "Giugno" )]
		Giugno = 6,
		[AmbientValue( "Luglio" )]
		Luglio = 7,
		[AmbientValue( "Agosto" )]
		Agosto = 8,
		[AmbientValue( "Settembre" )]
		Settembre = 9,
		[AmbientValue( "Ottobre" )]
		Ottobre = 10,
		[AmbientValue( "Novembre" )]
		Novembre = 11,
		[AmbientValue( "Dicembre" )]
		Dicembre = 12
	}


	public class SpesaMensileModel
	{
		public MesiEnum MeseCorrente { get; set; }
		public int Year { get; set; }
		public int NumeroRateNelMese { get; set; }
		public double TotaleSpesaMese { get; set; }
		/// <summary>
		/// Totale dato dalla sommatoria di tutti i debiti da estinguere
		/// </summary>
		public double TotaleSpesaPrevista { get; set; }
		public double TotaleRimborsato { get; set; }

		public int PercentualeCompletamento { get; set; }
	}
}