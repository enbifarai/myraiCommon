using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiData;

namespace myRaiCommonTasks
{
	public class ConteggiApprovatoreLivello1
	{
		private static List<myRaiCommonTasks.AbilitazioneSede> GetSediL2 ()
		{
			var ab = CommonTasks.getAbilitazioni();
			return ab.ListaAbilitazioni.Where( w => w.MatrLivello2.Any() ).ToList();
		}

		public static int GetTotaleEccezioniDaApprovare ( string matricola, DateTime? limit )
		{
			int result = 0;
			List<Liv1Riepilogo> riepilogo = new List<Liv1Riepilogo>();

			try
			{
				// reperimento delle sedi per le quali la matricola corrente risulta
				// essere responsabile di livello 1
				List<string> sediDiCompetenza = CommonTasks.GetSediReparti_MatricolaLivello1( matricola );

				// mese corrente
				//DateTime current = DateTime.Now;
				//current = current.AddMonths( 1 );
				//DateTime limit = new DateTime( current.Year, current.Month, 1, 23, 59, 59 );

				// ultimo giorno del mese corrente
				//limit = limit.AddDays( -1 );
				if ( !limit.HasValue )
				{
					limit = DateTime.MaxValue;
				}

				List<myRaiCommonTasks.AbilitazioneSede> livelli2Sedi = GetSediL2();

				foreach ( var sedeCorrente in sediDiCompetenza )
				{
					string sede = "";
					string reparto = "";

					if ( sedeCorrente.Length > 5 )
					{
						// sede e reparto
						sede = sedeCorrente.Substring( 0, 5 );
						reparto = sedeCorrente.Substring( 5 );

						if ( reparto == "00" || reparto == "" || reparto == " " )
						{
							reparto = null;
						}
					}
					else
					{
						sede = sedeCorrente;
						reparto = null;
					}

					int contaDaApprovare = 0;

					using ( digiGappEntities db = new digiGappEntities() )
					{
						var approvatore2 = livelli2Sedi.Where( m => m.Sede.Equals( sede ) ).FirstOrDefault();

						if ( approvatore2 != null )
						{
							var element = approvatore2.MatrLivello2.Where( w => w.Matricola.Equals( matricola ) ).FirstOrDefault();

							// se lo trova allora l'approvatore di primo livello in esame è anche
							// approvatore di secondo livello per la sede esaminata
							if ( element != null )
							{
								if ( String.IsNullOrEmpty( reparto ) )
								{
									contaDaApprovare = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
														r.codice_sede_gapp == sede &&
														r.periodo_dal <= limit );
								}
								else
								{
									contaDaApprovare = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
														r.codice_sede_gapp == sede &&
														r.reparto == reparto &&
														r.periodo_dal <= limit );
								}
								riepilogo.Add( new Liv1Riepilogo()
								{
									Matricola = matricola,
									DaApprovare = contaDaApprovare
								} );
							}
							else
							{
								// se il livello 1 non è anche livello2 per la sede esaminata, allora
								// al livello 1 dovrà approvare soltanto le richieste che non sono sue

								// conto le richieste che il livello 1 deve approvare
								string mtr = matricola.Replace( "P", "" );
								if ( String.IsNullOrEmpty( reparto ) )
								{
									contaDaApprovare = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
														r.codice_sede_gapp == sede &&
														r.periodo_dal <= limit &&
														!r.matricola_richiesta.Equals( mtr ) );
								}
								else
								{
									contaDaApprovare = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
														r.codice_sede_gapp == sede &&
														r.reparto == reparto &&
														r.periodo_dal <= limit &&
														!r.matricola_richiesta.Equals( mtr ) );
								}

								riepilogo.Add( new Liv1Riepilogo()
								{
									Matricola = matricola,
									DaApprovare = contaDaApprovare
								} );
							}
						}
						else
						{
							// se non ci sono approvatori di secondo livello allora sarà il primo livello
							// a doverle approvare
							if ( String.IsNullOrEmpty( reparto ) )
							{
								contaDaApprovare = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
													r.codice_sede_gapp == sede &&
													r.periodo_dal <= limit );
							}
							else
							{
								contaDaApprovare = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
													r.codice_sede_gapp == sede &&
													r.reparto == reparto &&
													r.periodo_dal <= limit );
							}

							riepilogo.Add( new Liv1Riepilogo()
							{
								Matricola = matricola,
								DaApprovare = contaDaApprovare
							} );
						}
					} // using ( digiGappEntities db = new digiGappEntities() ) 

					if ( riepilogo != null && riepilogo.Any() )
					{
						result = riepilogo.Sum( w => w.DaApprovare  );
					}
				}

			}
			catch ( Exception ex )
			{
			}











			return result;
		}

	}
}
