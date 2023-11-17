using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.MyRaiServiceReference1;

namespace myRai.DataControllers
{
    public class UtenteDataControllerResult
	{
		public DateTime Giorno { get; set; }
		public int Minuti { get; set; }
	}

	public class UtenteDataController
	{
		public static int GetPOH ( string matricola, DateTime? dataEnd = null )
		{
			string sessionName = String.Format( "AnalisiEccezioni{0}", matricola );
			HttpContext.Current.Session[sessionName] = GetAnalisiEcc( matricola, "POH", "ROH" );

			GetAnalisiEccezioniResponse r = HttpContext.Current.Session[sessionName] as GetAnalisiEccezioniResponse;
			int sottraiMinuti = 0;

			if (dataEnd.HasValue)
			{
				var successive = r.DettagliEccezioni.Where( x => x.data > dataEnd && x.eccezione == "POH" )
					.Select( x => x.minuti ).ToList();
				
				if ( successive != null && successive.Count > 0 )
				{
					sottraiMinuti = successive.Sum();
				}
			}
			
			if ( r != null && r.AnalisiEccezione.Length > 0 && r.AnalisiEccezione[0].totale != null )
				return Convert.ToInt32( r.AnalisiEccezione[0].totale ) - sottraiMinuti;
			else return 0;
		}

		public static List<DateTime> GetPOHdays ( string matricola )
		{
			string sessionName = String.Format( "AnalisiEccezioni{0}", matricola );
			HttpContext.Current.Session[sessionName] = GetAnalisiEcc( matricola, "POH", "ROH" );

			GetAnalisiEccezioniResponse r = HttpContext.Current.Session[sessionName] as GetAnalisiEccezioniResponse;

			if ( r == null || r.DettagliEccezioni == null ) return new List<DateTime>();

			return r.DettagliEccezioni.Where( x => x.eccezione == "POH" ).Select( x => x.data ).ToList();
		}

		public static int GetROH ( string matricola, DateTime? dataEnd = null )
		{
			string sessionName = String.Format( "AnalisiEccezioni{0}", matricola );
			HttpContext.Current.Session[sessionName] = GetAnalisiEcc( matricola, "POH", "ROH" );

			GetAnalisiEccezioniResponse r = HttpContext.Current.Session[sessionName] as GetAnalisiEccezioniResponse;

			int sottraiMinuti = 0;

			if ( dataEnd.HasValue )
			{
				var successive = r.DettagliEccezioni.Where( x => x.data > dataEnd && x.eccezione == "ROH" )
					.Select( x => x.minuti ).ToList();
				if ( successive != null && successive.Count > 0 )
				{
					sottraiMinuti = successive.Sum();
				}
			}

			if ( r != null && r.AnalisiEccezione.Length > 1 && r.AnalisiEccezione[1].totale != null )
				return Convert.ToInt32( r.AnalisiEccezione[1].totale ) - sottraiMinuti;
			else return 0;
		}

		public static List<UtenteDataControllerResult> GetPOHFullData ( string matricola, int mese )
		{
			string sessionName = String.Format( "AnalisiEccezioni{0}", matricola );
			HttpContext.Current.Session[sessionName] = GetAnalisiEcc( matricola, "POH", "ROH" );

			GetAnalisiEccezioniResponse r = HttpContext.Current.Session[sessionName] as GetAnalisiEccezioniResponse;

			List<UtenteDataControllerResult> results = r.DettagliEccezioni.Where( x => x.data.Month == mese && x.eccezione == "POH" )
				.Select( x => new UtenteDataControllerResult
				{
					Giorno = x.data,
					Minuti = x.minuti
				} ).ToList();

			return results;
		}

		public static List<UtenteDataControllerResult> GetROHFullData ( string matricola, int mese )
		{
			string sessionName = String.Format( "AnalisiEccezioni{0}", matricola );
			HttpContext.Current.Session[sessionName] = GetAnalisiEcc( matricola, "POH", "ROH" );

			GetAnalisiEccezioniResponse r = HttpContext.Current.Session[sessionName] as GetAnalisiEccezioniResponse;

			List<UtenteDataControllerResult> results = r.DettagliEccezioni.Where( x => x.data.Month == mese && x.eccezione == "ROH" )
				.Select( x => new UtenteDataControllerResult
				{
					Giorno = x.data,
					Minuti = x.minuti
				} ).ToList();

			return results;
		}

		public static List<UtenteDataControllerResult> GetSTRFullData ( string matricola, int mese )
		{
			string sessionName = String.Format( "AnalisiEccezioni{0}", matricola );
			HttpContext.Current.Session[sessionName] = GetAnalisiEcc( matricola, "STR", "STRF" );

			GetAnalisiEccezioniResponse r = HttpContext.Current.Session[sessionName] as GetAnalisiEccezioniResponse;

			List<UtenteDataControllerResult> results = r.DettagliEccezioni.Where( x => x.data.Month == mese && x.eccezione == "STR" )
				.Select( x => new UtenteDataControllerResult
				{
					Giorno = x.data,
					Minuti = x.minuti
				} ).ToList();

			return results;
		}

		public static List<UtenteDataControllerResult> GetSTRFFullData ( string matricola, int mese )
		{
			string sessionName = String.Format( "AnalisiEccezioni{0}", matricola );
			HttpContext.Current.Session[sessionName] = GetAnalisiEcc( matricola, "STR", "STRF" );

			GetAnalisiEccezioniResponse r = HttpContext.Current.Session[sessionName] as GetAnalisiEccezioniResponse;

			List<UtenteDataControllerResult> results = r.DettagliEccezioni.Where( x => x.data.Month == mese && x.eccezione == "STRF" )
				.Select( x => new UtenteDataControllerResult
				{
					Giorno = x.data,
					Minuti = x.minuti
				} ).ToList();

			return results;
		}

		public static List<UtenteDataControllerResult> GetReperibilita ( string matricola, int mese, string tipologia )
		{
			List<UtenteDataControllerResult> results = new List<UtenteDataControllerResult>();

			GetAnalisiEccezioniResponse response = GetAnalisiEcc( matricola, tipologia, "" );

			results.AddRange( response.DettagliEccezioni.Where( x => x.data.Month == mese && x.eccezione == tipologia ).Select( x => new UtenteDataControllerResult
				{
					Giorno = x.data,
					Minuti = x.minuti
				} ).ToList() );

			return results;
		}

		private static GetAnalisiEccezioniResponse GetAnalisiEcc ( string matricola, string ecc1, string ecc2 )
			{
				try
				{
                    MyRaiService1Client wcf1 = new MyRaiService1Client();

                    wcf1.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();
				
					int? anno = ( int? )SessionHelper.Get( SessionVariables.AnnoFeriePermessi );
				
					if ( anno == null )
					{
						GetAnalisiEccezioniResponse response = wcf1.GetAnalisiEccezioni( matricola, new DateTime( DateTime.Now.Year, 1, 1 ), DateTime.Now, ecc1, ecc2, null );
						if ( response != null && response.DettagliEccezioni != null )
						{
							foreach ( var d in response.DettagliEccezioni )
							{
								d.data = new DateTime( DateTime.Now.Year, d.data.Month, d.data.Day );
							}
						}
						return response;
					}
					else
					{
						GetAnalisiEccezioniResponse response = wcf1.GetAnalisiEccezioni( matricola, new DateTime( ( int )anno, 1, 1 ), new DateTime( ( int )anno, 12, 31 ), ecc1, ecc2, null );

						if ( response != null && response.DettagliEccezioni != null )
						{
							foreach ( var d in response.DettagliEccezioni )
							{
								d.data = new DateTime( ( int )anno, d.data.Month, d.data.Day );
							}
						}
						return response;
					}
				}
				catch ( Exception ex )
				{
					Logger.LogErrori( new MyRai_LogErrori()
					{
						applicativo = "PORTALE",
						data = DateTime.Now,
						matricola = matricola,
						provenienza = "Utente.GetAnalisiEcc()",
						error_message = ex.ToString()
					} );
					return null;
				}
			}

	}
}