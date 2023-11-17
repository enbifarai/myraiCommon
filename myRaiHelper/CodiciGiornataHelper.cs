using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiHelper
{
	public class CodiciGiornata
	{
		public CodiciGiornata ()
		{
			this.Valore = "";
			this.Descrizione = "";
			this.Codice = "";
			this.Lavorativa = true;
			this.Colore = "bg-white";
		}
		public string Valore { get; set; }
		public string Descrizione { get; set; }
		public string Codice { get; set; }
		public bool Lavorativa { get; set; }
		public string Colore { get; set; }
	}

	/// <summary>
	/// Classe helper per gli enum
	/// </summary>
	public static class CodiciGiornataHelper
	{
		/// <summary>
		/// Reperimento della descrizione dell'enum
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static CodiciGiornata GetCodiceGiornata ( this string value )
		{
			CodiciGiornata result = new CodiciGiornata()
			{
				Valore = value,
				Descrizione = "",
				Codice = "",
				Lavorativa = true
			};

			switch ( value )
			{
				case "A":
					result.Descrizione = "Non lavorativa";
					result.Codice = "";
					result.Lavorativa = false;
					result.Colore = "bg-grey";
					break;
				case "B":
					result.Descrizione = "Fuori contratto";
					result.Codice = "";
					result.Lavorativa = false;
					result.Colore = "bg-grey";
					break;
				case "D":
					result.Descrizione = "Ferie (FE) validate";
					result.Codice = "FE";
					result.Colore = "bg-success";
					break;
				case "F":
					result.Descrizione = "Assenza ingiustificata / Transito sfasato";
					result.Codice = "";
					result.Colore = "bg-pink";
					break;
				case "1":
					result.Descrizione = "PF - Permessi validati";
					result.Codice = "PF";
					result.Colore = "bg-success";
					break;
				case "2":
					result.Descrizione = "PR - Permessi validati";
					result.Codice = "PR";
					result.Colore = "bg-success";
					break;
				case "3":
					result.Descrizione = "PX - Permessi validati";
					result.Codice = "PX";
					result.Colore = "bg-success";
					break;
				case "G":
					result.Descrizione = "Ferie (FE, PF, PR, PX) da validare - Richieste";
					result.Codice = "FE|PF|PR|PX";
					result.Colore = "bg-lightgreen";
					break;
				case "4":
					result.Descrizione = "PF - Permessi da validare - Richiesti";
					result.Codice = "PF";
					result.Colore = "bg-lightgreen";
					break;
				case "5":
					result.Descrizione = "PR - Permessi da validare - Richiesti";
					result.Codice = "PR";
					result.Colore = "bg-lightgreen";
					break;
				case "6":
					result.Descrizione = "PX - Permessi da validare - Richiesti";
					result.Codice = "PX";
					result.Colore = "bg-lightgreen";
					break;
				case "L":
					result.Descrizione = "MN, MF, MR";
					result.Codice = "MN|MF|MR";
					result.Colore = "bg-pink";
					break;
				case "M":
					result.Descrizione = "TIS";
					result.Codice = "TIS";
					result.Colore = "bg-pink";
					break;
				case "E":
					result.Descrizione = "Recuperi (RN, RR, RF) validati";
					result.Codice = "RN|RR|RF";
					result.Colore = "bg-blue";
					break;
				case "7":
					result.Descrizione = "Recuperi (RN) validati";
					result.Codice = "RN";
					result.Colore = "bg-blue";
					break;
				case "8":
					result.Descrizione = "Recuperi (RF) validati";
					result.Codice = "RF";
					result.Colore = "bg-blue";
					break;
				case "H":
					result.Descrizione = "Recuperi (RN, RR, RF) da validare - Richiesti";
					result.Codice = "RN|RR|RF";
					result.Colore = "bg-info";
					break;
				case "9":
					result.Descrizione = "Recuperi (RN) da validare - Richiesti";
					result.Codice = "RN";
					result.Colore = "bg-info";
					break;
				case "0":
					result.Descrizione = "Recuperi (RF) da validare - Richiesti";
					result.Codice = "RF";
					result.Colore = "bg-info";
					break;
				case "N":
					result.Descrizione = "Altre assenze (TAS, TML, TIF, TGP)";
					result.Codice = "TAS|TML|TIF|TGP";
					result.Colore = "bg-warning";
					break;
				case "Z":
					result.Descrizione = "Possibile inserimento";
					result.Codice = "";
					result.Colore = "bg-white";
					break;
				case "K":
					result.Descrizione = "Incongruenza archivi GAPP / Assenza ingiustificata";
					result.Codice = "";
					result.Colore = "bg-danger";
					break;
				case "X":
					result.Descrizione = "Giornata inesistente (es: 30 Febbraio)";
					result.Codice = "";
					result.Colore = "bg-danger";
					break;
				case "W":
					result.Descrizione = "Giornata passata al ruolo NON modificabile";
					result.Codice = "";
					result.Colore = "bg-white";
					break;
			}

			return result;
		}


		public static string GetDescrizioneByCodice ( this string value )
		{
			string result = "";

			switch ( value )
			{
				case "bg-grey":
					result = "Non lavorativa, Fuori contratto";
					break;
				case "bg-success":
					result = "FE, PF, PR, PX validati";
					break;
				case "bg-lightgreen":
					result = "FE, PF, PR, PX da validare";
					break;
				case "bg-blue":
					result = "RN, RR, RF validati";
					break;
				case "bg-info":
					result = "RN, RR, RF da validare";
					break;
				case "bg-warning":
					result = "TAS, TML, TIF, TGP";
					break;
				case "bg-danger":
					result = "Incongruenza archivi GAPP / Assenza ingiustificata, Giornata inesistente (es: 30 Febbraio)";
					break;
				case "bg-pink":
					result = "Assenza ingiustificata / Transito sfasato, MN, MF, MR, TIS";
					break;
				case "bg-white":
					result = "Possibile inserimento, Giornata passata al ruolo NON modificabile";
					break;
			}

			return result;
		}
	}
}