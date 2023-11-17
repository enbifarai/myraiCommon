using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using myRaiCommonTasks.Helpers;
using myRaiData;

namespace myRaiCommonTasks
{
	public class ReminderLiv1RichiesteDaApp
	{
		public ReminderLiv1RichiesteDaApp ()
		{
		}

		public void Start ()
		{
			string lineaReport = "";

			lineaReport += String.Format( "Avvio InvioReminderApprovatori data e ora: {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) );
            //Console.WriteLine( String.Format( "Avvio InvioReminderApprovatori data e ora: {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
            Output.WriteLine(String.Format("Avvio InvioReminderApprovatori data e ora: {0} \r\n", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            try
			{
				// mese corrente
				DateTime current = DateTime.Now;
				DateTime limit = new DateTime( current.Year, current.Month, 1, 23, 59, 59 );

				// ultimo giorno del mese precedente
				limit = limit.AddDays( -1 );

                // PRENDE LE RICHIESTE FINO ALLA DATA CORRENTE
				List<string> matricoleApprovatori = new List<string>();
				List<string> approvatori = new List<string>();
				List<Liv1Riepilogo> riepilogo = new List<Liv1Riepilogo>();
				Liv1Riepilogo toUpdate = new Liv1Riepilogo();

				lineaReport += String.Format( "Reperimento matricole approvatori di livello 1 {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) );
                Output.WriteLine( String.Format( "Reperimento matricole approvatori di livello 1 {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
				List<myRaiCommonTasks.AbilitazioneSede> livelli1Sedi = GetSediL1();

				lineaReport += String.Format( "Reperimento matricole approvatori di livello 2 {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) );
                Output.WriteLine( String.Format( "Reperimento matricole approvatori di livello 2 {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
				List<myRaiCommonTasks.AbilitazioneSede> livelli2Sedi = GetSediL2();

				if ( livelli1Sedi != null && livelli1Sedi.Any() )
				{
					string sede = "";
					string reparto = "";

					foreach ( var livello1 in livelli1Sedi )
					{
						if ( livello1.Sede.Length > 5 )
						{
							// sede e reparto
							sede = livello1.Sede.Substring( 0, 5 );
							reparto = livello1.Sede.Substring( 5 );

							if ( reparto == "00" || reparto == "" || reparto == " " )
							{
								reparto = null;
							}
						}
						else
						{
							sede = livello1.Sede;
							reparto = null;
						}

						foreach ( var liv1 in livello1.MatrLivello1 )
						{
                            //if (liv1.Matricola.Contains("314706"))
                            //{
                            //    Output.WriteLine("SONO QUI");
                            //}
							lineaReport += String.Format( "Conteggio delle richieste da approvare per la matricola {0} - {1} \r\n", liv1.Matricola, DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) );
                            Output.WriteLine( String.Format( "Conteggio delle richieste da approvare per la matricola {0} - {1} \r\n" , liv1.Matricola , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
							int contaDaApprovare = 0;

							using ( digiGappEntities db = new digiGappEntities() )
							{
								string mySede = livello1.Sede.Substring( 0, 5 );
								var approvatore2 = livelli2Sedi.Where( m => m.Sede.Equals( mySede ) ).FirstOrDefault();

								if ( approvatore2 != null )
								{
									var element = approvatore2.MatrLivello2.Where( w => w.Matricola.Equals( liv1.Matricola ) ).FirstOrDefault();

									// se lo trova allora l'approvatore di primo livello in esame è anche
									// approvatore di secondo livello per la sede esaminata
									if ( element != null )
									{
										if ( String.IsNullOrEmpty( reparto ) )
										{
											contaDaApprovare = db.MyRai_Richieste.Count( r =>
                                                                r.id_stato == 10 &&
                                                                r.MyRai_Eccezioni_Richieste.Count(e => e.id_stato == 10) > 0 &&
                                                                r.codice_sede_gapp == sede &&
																r.periodo_dal <= limit );
										}
										else
										{
											contaDaApprovare = db.MyRai_Richieste.Count( r =>
                                                                r.id_stato == 10 &&
                                                                r.MyRai_Eccezioni_Richieste.Count(e => e.id_stato == 10) > 0 &&
																r.codice_sede_gapp == sede &&
																r.reparto == reparto &&
																r.periodo_dal <= limit );
										}

										matricoleApprovatori.Add( liv1.Matricola );
									}
									else
									{
										// se il livello 1 non è anche livello2 per la sede esaminata, allora
										// al livello 1 dovrà approvare soltanto le richieste che non sono sue

										// conto le richieste che il livello 1 deve approvare
										string mtr = liv1.Matricola.Replace( "P", "" );
										if ( String.IsNullOrEmpty( reparto ) )
										{
											contaDaApprovare = db.MyRai_Richieste.Count( r =>
                                                                r.id_stato == 10 &&
                                                                r.MyRai_Eccezioni_Richieste.Count(e => e.id_stato == 10) > 0 &&
                                                                r.codice_sede_gapp == sede &&
																r.periodo_dal <= limit &&
																!r.matricola_richiesta.Equals( mtr ) );
										}
										else
										{
											contaDaApprovare = db.MyRai_Richieste.Count( r =>
                                                                r.id_stato == 10 &&
                                                                r.MyRai_Eccezioni_Richieste.Count(e => e.id_stato == 10) > 0 &&
                                                                r.codice_sede_gapp == sede &&
																r.reparto == reparto &&
																r.periodo_dal <= limit &&
																!r.matricola_richiesta.Equals( mtr ) );
										}

										toUpdate = riepilogo.Where( m => m.Matricola.Equals( liv1.Matricola ) ).FirstOrDefault();

										if ( toUpdate != null )
										{
											string complete = sede + ( reparto != null ? reparto : "" );

											if ( !toUpdate.SedeGapp.Contains( complete ) )
											{
												toUpdate.DaApprovare += contaDaApprovare;
												toUpdate.SedeGapp += " ; " + sede + ( reparto != null ? reparto : "" );
											}
										}
										else
										{
											riepilogo.Add( new Liv1Riepilogo()
											{
												Matricola = liv1.Matricola,
												DaApprovare = contaDaApprovare,
												SedeGapp = sede + ( reparto != null ? reparto : "" )
											} );
										}

										// ora invece bisogna contare le richieste che devono approvare 
										// i livelli 2 per quella sede.. 
										// richieste effettuate dal livello 1 che non se le può autoapprovare

										matricoleApprovatori.AddRange( approvatore2.MatrLivello2.Select( x => x.Matricola ).ToList() );

										if ( String.IsNullOrEmpty( reparto ) )
										{
											contaDaApprovare = db.MyRai_Richieste.Count( r =>
                                                                r.id_stato == 10 &&
                                                                r.MyRai_Eccezioni_Richieste.Count(e => e.id_stato == 10) > 0 &&
                                                                r.codice_sede_gapp == sede &&
																r.periodo_dal <= limit &&
																r.matricola_richiesta.Equals( mtr ) );
										}
										else
										{
											contaDaApprovare = db.MyRai_Richieste.Count( r =>
                                                                r.id_stato == 10 &&
                                                                r.MyRai_Eccezioni_Richieste.Count(e => e.id_stato == 10) > 0 &&
                                                                r.codice_sede_gapp == sede &&
																r.reparto == reparto &&
																r.periodo_dal <= limit &&
																r.matricola_richiesta.Equals( mtr ) );
										}
									}
								}
								else
								{
									// se non ci sono approvatori di secondo livello allora sarà il primo livello
									// a doverle approvare
									if ( String.IsNullOrEmpty( reparto ) )
									{
										contaDaApprovare = db.MyRai_Richieste.Count( r =>
                                                            r.id_stato == 10 &&
                                                            r.MyRai_Eccezioni_Richieste.Count(e => e.id_stato == 10) > 0 &&
                                                            r.codice_sede_gapp == sede &&
															r.periodo_dal <= limit);
									}
									else
									{
										contaDaApprovare = db.MyRai_Richieste.Count( r =>
                                                            r.id_stato == 10 &&
                                                            r.MyRai_Eccezioni_Richieste.Count(e => e.id_stato == 10) > 0 &&
                                                            r.codice_sede_gapp == sede &&
															r.reparto == reparto &&
															r.periodo_dal <= limit);
									}

									matricoleApprovatori.Add( liv1.Matricola );
								}
							}

							if ( matricoleApprovatori != null && matricoleApprovatori.Any() )
							{
								foreach ( var matr in matricoleApprovatori )
								{
									toUpdate = riepilogo.Where( m => m.Matricola.Equals( matr ) ).FirstOrDefault();

									if ( toUpdate != null )
									{
										string complete = sede + ( reparto != null ? reparto : "" );

										if ( !toUpdate.SedeGapp.Contains( complete ) )
										{
											toUpdate.DaApprovare += contaDaApprovare;
											toUpdate.SedeGapp += " ; " + sede + ( reparto != null ? reparto : "" );
										}
									}
									else
									{
										riepilogo.Add( new Liv1Riepilogo()
										{
											Matricola = matr,
											DaApprovare = contaDaApprovare,
											SedeGapp = sede + ( reparto != null ? reparto : "" )
										} );
									}
								}
							}

							matricoleApprovatori.Clear();
						}
					}
				}

				lineaReport += String.Format( "Termine conteggio {0}\r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) );
                Output.WriteLine( String.Format( "Termine conteggio {0}\r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

				lineaReport += String.Format( "------------------------------------\r\n" );
                Output.WriteLine( String.Format( "------------------------------------\r\n" ) );

                string linee = "";
				foreach ( var yyy in riepilogo )
				{
					lineaReport += yyy.Matricola + " ; " + yyy.SedeGapp.PadRight( 7, ' ' ) + " ; " + yyy.DaApprovare.ToString() + "\r\n";
                    linee += yyy.Matricola + " ; " + yyy.SedeGapp.PadRight( 7 , ' ' ) + " ; " + yyy.DaApprovare.ToString( ) + "\r\n"; ;
				}
                Output.WriteLine( linee );
				lineaReport += String.Format( "------------------------------------\r\n" );
                Output.WriteLine( String.Format( "------------------------------------\r\n" ) );

				lineaReport += String.Format( "Invio mail agli approvatori {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) );
                Output.WriteLine( String.Format( "Invio mail agli approvatori {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
				InviaMailReminderLivello1( riepilogo );

				lineaReport += String.Format( "Termine invio mail agli approvatori {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) );
                Output.WriteLine( String.Format( "Termine invio mail agli approvatori {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
                Output.WriteLine( linee );
			}
			catch ( Exception ex )
			{
				Logger.Log( ex.ToString() );

				lineaReport += String.Format( "------------------------------------\r\n" );
                Output.WriteLine( String.Format( "------------------------------------\r\n" ) );
				lineaReport += String.Format( "Si è verificato un errore {0}: \r\n{1}", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ), ex.Message );
                Output.WriteLine( String.Format( "Si è verificato un errore {0}: \r\n{1}" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) , ex.Message ) );
			}

			string nomeReport = String.Format( "InvioReminderApprovatori_{0}.txt", DateTime.Now.ToString( "ddMMyyyy" ) );
            Output.WriteLine( String.Format( "InvioReminderApprovatori_{0}.txt" , DateTime.Now.ToString( "ddMMyyyy" ) ) );
			System.IO.File.WriteAllText( AppDomain.CurrentDomain.BaseDirectory + nomeReport, lineaReport );
		}

		#region private

		/// <summary>
		/// Invio della mail
		/// </summary>
		/// <param name="toSend"></param>
		private void InviaMailReminderLivello1 ( List<Liv1Riepilogo> toSend )
		{
			try
			{
				string[] par = myRaiCommonTasks.CommonTasks.GetParametri<string>( myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateLiv1SollecitoApprovazioni );

				foreach ( var item in toSend )
				{
					if ( item.DaApprovare > 0 )
					{
						string body = par[0].Replace( "#QUANTITA", item.DaApprovare.ToString() );
						string dest = myRaiCommonTasks.CommonTasks.GetEmailPerMatricola( item.Matricola.TrimStart( 'P' ) );

						if ( String.IsNullOrWhiteSpace( dest ) ) dest = item.Matricola + "@rai.it";

						GestoreMail mail = new GestoreMail();

						var response = mail.InvioMail( body, par[1], dest, "raiplace.selfservice@rai.it", myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailApprovazioneFrom );                        
                        if ( response != null && response.Errore != null )
						{
							using ( digiGappEntities db = new digiGappEntities() )
							{
								myRaiData.MyRai_LogErrori err = new MyRai_LogErrori()
								{
									applicativo = "Batch",
									data = DateTime.Now,
									provenienza = "InviaMailReminderLivello1",
									error_message = response.Errore + " per " + dest
								};
								db.MyRai_LogErrori.Add( err );
								try
								{
									db.SaveChanges();
                                    Output.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " Invio mail a " + dest + " esito ok");
                                }
								catch ( Exception ex )
								{
                                    Output.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " Invio mail a " + dest + " errore " + ex.ToString());
                                    Logger.Log( "InviaMailReminderLivello1 " + ex.ToString() );
								}
							}
						}
					}
				}
			}
			catch ( Exception ex )
			{
				Logger.Log( ex.ToString() );
			}
		}

		private static List<myRaiCommonTasks.AbilitazioneSede> GetSediL1 ()
		{
			var ab = CommonTasks.getAbilitazioni();
			return ab.ListaAbilitazioni.Where( w => w.MatrLivello1.Any() ).ToList();
		}

		private static List<myRaiCommonTasks.AbilitazioneSede> GetSediL2 ()
		{
			var ab = CommonTasks.getAbilitazioni();
			return ab.ListaAbilitazioni.Where( w => w.MatrLivello2.Any() ).ToList();
		}

		#endregion
	}
}