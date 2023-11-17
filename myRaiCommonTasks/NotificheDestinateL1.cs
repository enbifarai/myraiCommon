using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonTasks
{
	public class NotificheDestinateL1
	{
		public NotificheDestinateL1 ()
		{
		}

		public static List<SollecitoAppr> GetRichieste ()
		{
			var db = new digiGappEntities();
			DateTime d = new DateTime( DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0 );

			List<SollecitoAppr> listaPending = db.MyRai_Richieste
				.Where( x => x.id_stato == 10 )
				.GroupBy( a => a.codice_sede_gapp )
				.Select( res => new
					SollecitoAppr()
				{
					RichiesteUrg = db.MyRai_Richieste
								.Where( x => x.id_stato == 10 && x.codice_sede_gapp == res.Key //&& x.periodo_dal < d
									   && !x.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" )
									   && x.urgente )
								.Count(),
					RichiesteSca = db.MyRai_Richieste
								.Where( x => x.id_stato == 10 && x.codice_sede_gapp == res.Key //&& x.periodo_dal < d
									  && !x.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" )
									  && x.scaduta )
								.Count(),
					RichiesteOrd = db.MyRai_Richieste
								.Where( x => x.id_stato == 10 && x.codice_sede_gapp == res.Key //&& x.periodo_dal < d
									  && !x.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" )
									  && !x.scaduta && !x.urgente )
								.Count(),
					RichiesteUrgS = db.MyRai_Richieste
										.Where( x => x.id_stato == 10 && x.codice_sede_gapp == res.Key //&& x.periodo_dal < d
										&& x.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" )
										&& x.urgente )
										.Count(),
					RichiesteScaS = db.MyRai_Richieste
								.Where( x => x.id_stato == 10 && x.codice_sede_gapp == res.Key //&& x.periodo_dal < d
									  && x.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" )
									  && x.scaduta )
								.Count(),
					RichiesteOrdS = db.MyRai_Richieste
								.Where( x => x.id_stato == 10 && x.codice_sede_gapp == res.Key //&& x.periodo_dal < d
									  && x.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" )
									  && !x.scaduta && !x.urgente )
								.Count(),
					sedeGapp = res.Key,
					Richieste = db.MyRai_Richieste
								.Where( x => x.id_stato == 10 && x.codice_sede_gapp == res.Key ) //&& x.periodo_dal < d)

				} ).ToList();
			return listaPending;
		}

		public static void InviaRichiesteInserite ( bool SoloStorni = false )
		{
			var rich = GetRichieste();
			Abilitazioni AB = myRaiCommonTasks.CommonTasks.getAbilitazioni();
			string[] MailParams = null;

			if ( SoloStorni )
			{
				MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>
					  ( myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateNotificheStorniBossL1 );
			}
			else
			{
				MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>
				   ( myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateNotificheBossL1 );
			}

			string MailTemplate = MailParams[0];
			string MailSubject = MailParams[1];

			List<MatrSedeAppartenenza> Livelli1 = AB.ListaAbilitazioni.SelectMany( x => x.MatrLivello1 ).Distinct().ToList();

			var db = new digiGappEntities();
			var impDefault = db.MyRai_InvioNotifiche.Where( x => x.Matricola == "*" && x.TipoDestinatario == "ESS_LIV1" && x.TipoEvento == ( SoloStorni ? "INSS" : "INSR" ) ).FirstOrDefault();

			if ( impDefault == null )
			{
				CommonTasks.Log( "Impostazione invio di default non presente" );
				return;
			}

			Boolean DontSendDefault = false;

			if ( impDefault.UltimoInvio != null && ( ( DateTime )impDefault.UltimoInvio ).Date == DateTime.Now.Date )
			{
				DontSendDefault = true;
			}

			foreach ( var liv1 in Livelli1 )
			{
				List<string> sedi = myRaiCommonTasks.CommonTasks.GetSediReparti_MatricolaLivello1( liv1.Matricola );

				int totOrdinarie = rich.Where( x => sedi.Contains( x.sedeGapp ) ).Sum( x => x.RichiesteOrd );
				int totUrgenti = rich.Where( x => sedi.Contains( x.sedeGapp ) ).Sum( x => x.RichiesteUrg );
				int totScadute = rich.Where( x => sedi.Contains( x.sedeGapp ) ).Sum( x => x.RichiesteSca );

				int totOrdinarieS = rich.Where( x => sedi.Contains( x.sedeGapp ) ).Sum( x => x.RichiesteOrdS );
				int totUrgentiS = rich.Where( x => sedi.Contains( x.sedeGapp ) ).Sum( x => x.RichiesteUrgS );
				int totScaduteS = rich.Where( x => sedi.Contains( x.sedeGapp ) ).Sum( x => x.RichiesteScaS );

				if ( SoloStorni )
				{
					if ( totOrdinarieS + totUrgentiS + totScaduteS == 0 )
					{
						continue;
					}
				}
				else
				{
					if ( totOrdinarie + totUrgenti + totScadute + totOrdinarieS + totUrgentiS + totScaduteS == 0 )
					{
						continue;
					}
				}

				MailTemplate = MailParams[0];
				if ( SoloStorni )
				{
					MailTemplate = MailTemplate
				   .Replace( "#SURG", totUrgentiS.ToString() )
				   .Replace( "#SSCA", totScaduteS.ToString() )
				   .Replace( "#SORD", totOrdinarieS.ToString() );
				}
				else
				{
					MailTemplate = MailTemplate
					.Replace( "#URG", totUrgenti.ToString() )
					.Replace( "#SCA", totScadute.ToString() )
					.Replace( "#ORD", totOrdinarie.ToString() )
					.Replace( "#SURG", totUrgentiS.ToString() )
					.Replace( "#SSCA", totScaduteS.ToString() )
					.Replace( "#SORD", totOrdinarieS.ToString() );
				}

				string oreMinutiRiferimento = DateTime.Now.Hour.ToString().PadLeft( 2, '0' ) + "00";
				
				var impNotCurrent = Notifiche.GetImpostazioneNotifiche( liv1.Matricola.Replace( "P", "" ), "ESS_LIV1", ( SoloStorni ? "INSS" : "INSR" ) );

				if ( impNotCurrent == null || impNotCurrent.TipoInvio == "I" || impNotCurrent.TipoInvio == "N" )
				{
					continue;
				}

				if ( impDefault.Id == impNotCurrent.Id )
				{
                    if ( DontSendDefault )
                        continue;
				}

				if ( impNotCurrent.UltimoInvio != null && impNotCurrent.Id != impDefault.Id &&
					( ( DateTime )impNotCurrent.UltimoInvio ).Date == DateTime.Now.Date )
				{
					continue;
				}

				if ( ( impNotCurrent.TipoInvio == "G" && impNotCurrent.OraMinuti == oreMinutiRiferimento )
					||
				   ( impNotCurrent.TipoInvio == "S" && impNotCurrent.GiornoDellaSettimana == ( int )DateTime.Now.DayOfWeek && impNotCurrent.OraMinuti == oreMinutiRiferimento )
					)
				{
					bool success = Notifiche.InviaSingolaMail( MailTemplate, MailSubject, liv1.Matricola + "@rai.it" );
					if ( success )
					{
						var n = db.MyRai_InvioNotifiche.Where( x => x.Id == impNotCurrent.Id ).FirstOrDefault();
						if ( n != null )
						{
							n.UltimoInvio = DateTime.Now;
							db.SaveChanges();
						}
					}
				}
			}
		}

		public static void InviaScaduteL1 ()
		{
			List<NotificheMatricola> LNotifiche = Notifiche.GetNotifiche( EnumTipoDestinatarioNotifica.ESS_LIV1,
				EnumCategoriaEccezione.InsRichiesta,
				EnumTipoEventoNotifica.SCAD );

			if ( LNotifiche.Count == 0 )
			{
				CommonTasks.Log( "Nessuna notifica da inviare" );
				return;
			}

			var NotificheDaInviarePerMatr = LNotifiche.GroupBy(
			 item => item.matricola,
			 item => item.notifiche,
				( key, g ) => new
				{
					matricola = key,
					notifiche = g.SelectMany( n => n ).OrderByDescending( n => n.data_inserita ).ToList()
				} ).ToList();

			string[] MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>
					( myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateNotificheScaduteL1 );
			string MailTemplate = MailParams[0];
			string MailSubject = MailParams[1];

			var db = new myRaiData.digiGappEntities();
			foreach ( var item in NotificheDaInviarePerMatr )
			{
				string MailDestinatario = "";
				int totScadute = 0;
				List<MyRai_Notifiche> DaMarcare = new List<MyRai_Notifiche>();

				foreach ( var notifica in item.notifiche )
				{
					MailDestinatario = notifica.email_destinatario;
					var richiesta = db.MyRai_Richieste.Where( x => x.id_richiesta == notifica.id_riferimento ).FirstOrDefault();
					if ( richiesta != null )
					{
						if ( richiesta.scaduta )
						{
							totScadute++;
							DaMarcare.Add( notifica );
						}
					}
				}
                if ( totScadute == 0 )
                    continue;

				MailTemplate = MailParams[0];
				MailTemplate = MailTemplate.Replace( "#SCA", totScadute.ToString() );


				bool success = Notifiche.InviaSingolaMail( MailTemplate, MailSubject, MailDestinatario );

				if ( success )
				{
					DaMarcare.ForEach( x => x.data_inviata = DateTime.Now );
					try
					{
						db.SaveChanges();
					}
					catch ( Exception ex )
					{
						CommonTasks.Log( "Errore salvataggio DB InviaScaduteL1 " + ex.ToString() );
					}
				}
			}
		}

		public static void InviaUrgentiL1 ()
		{
			List<NotificheMatricola> LNotifiche = Notifiche.GetNotifiche( EnumTipoDestinatarioNotifica.ESS_LIV1,
															  EnumCategoriaEccezione.InsRichiesta,
															  EnumTipoEventoNotifica.URG );

			if ( LNotifiche.Count == 0 )
			{
				CommonTasks.Log( "Nessuna notifica da inviare" );
				return;
			}
			var NotificheDaInviarePerMatr = LNotifiche.GroupBy(
			 item => item.matricola,
			 item => item.notifiche,
				( key, g ) => new
				{
					matricola = key,
					notifiche = g.SelectMany( n => n ).OrderByDescending( n => n.data_inserita ).ToList()
				} ).ToList();

			string[] MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>
					( myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateNotificheUrgentiL1 );
			string MailTemplate = MailParams[0];
			string MailSubject = MailParams[1];

			var db = new myRaiData.digiGappEntities();
			foreach ( var item in NotificheDaInviarePerMatr )
			{
				string MailDestinatario = "";
				int totUrgenti = 0;
				List<MyRai_Notifiche> DaMarcare = new List<MyRai_Notifiche>();

				foreach ( var notifica in item.notifiche )
				{
					MailDestinatario = notifica.email_destinatario;
					var richiesta = db.MyRai_Richieste.Where( x => x.id_richiesta == notifica.id_riferimento ).FirstOrDefault();
					if ( richiesta != null )
					{
						if ( richiesta.urgente )
						{
							totUrgenti++;
							DaMarcare.Add( notifica );
						}
					}
				}
                if ( totUrgenti == 0 )
                    continue;

				MailTemplate = MailParams[0];
				MailTemplate = MailTemplate.Replace( "#URG", totUrgenti.ToString() );


				bool success = Notifiche.InviaSingolaMail( MailTemplate, MailSubject, MailDestinatario );

				if ( success )
				{
					DaMarcare.ForEach( x => x.data_inviata = DateTime.Now );
					try
					{
						db.SaveChanges();
					}
					catch ( Exception ex )
					{
						CommonTasks.Log( "Errore salvataggio DB InviaUrgentiL1 " + ex.ToString() );
					}
				}
			}
		}

		public void Start ( bool SoloStorni = false )
		{
			CommonTasks.Log( String.Format( "\r\n" ) );
			CommonTasks.Log( String.Format( "--------------------------------------------------" ) );
			CommonTasks.Log( String.Format( "Avvio invio riepilogo notifiche per approvatore livello 1 data e ora: {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

			try
			{
				Boolean DontSendDefault = false;
				MyRai_InvioNotifiche impDefault = new MyRai_InvioNotifiche();
				string[] MailParams = null;
				List<string> matricoleApprovatori = new List<string>();
				List<Liv1Riepilogo> riepilogo = new List<Liv1Riepilogo>();
				Liv1Riepilogo toUpdate = new Liv1Riepilogo();

				digiGappEntities _db = new digiGappEntities();

				if ( SoloStorni )
				{
					MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>
						  ( myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateNotificheStorniBossL1 );
				}
				else
				{
					MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>
					   ( myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateNotificheBossL1 );
				}

				string MailTemplate = MailParams[0];
				string MailSubject = MailParams[1];

				CommonTasks.Log( String.Format( "Reperimento matricole approvatori di livello 1 {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
				List<AbilitazioneSede> livelli1Sedi = GetSediL1();

				CommonTasks.Log( String.Format( "Reperimento matricole approvatori di livello 2 {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
				List<AbilitazioneSede> livelli2Sedi = GetSediL2();

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
							impDefault = _db.MyRai_InvioNotifiche.Where( x => x.Matricola == liv1.Matricola.Replace("P","") && x.TipoDestinatario == "ESS_LIV1" && x.TipoEvento == ( SoloStorni ? "INSS" : "INSR" ) ).FirstOrDefault();

							if ( impDefault == null )
							{
								impDefault = _db.MyRai_InvioNotifiche.Where( x => x.Matricola == "*" && x.TipoDestinatario == "ESS_LIV1" && x.TipoEvento == ( SoloStorni ? "INSS" : "INSR" ) ).FirstOrDefault();

								if ( impDefault == null )
								{
									CommonTasks.Log( String.Format( "Impostazione invio di default non presente {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
									return;
								}
							}

							DontSendDefault = false;

							if ( impDefault.UltimoInvio != null && ( ( DateTime )impDefault.UltimoInvio ).Date == DateTime.Now.Date )
							{
								DontSendDefault = true;
							}

							Liv1Riepilogo current = new Liv1Riepilogo();

							CommonTasks.Log( String.Format( "Conteggio delle richieste da approvare per la matricola {0} - {1} \r\n", liv1.Matricola, DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

							using ( digiGappEntities db = new digiGappEntities() )
							{
								string mySede = livello1.Sede.Substring( 0, 5 );
								var approvatore2 = livelli2Sedi.Where( m => m.Sede.Equals( mySede ) ).FirstOrDefault();

								// se ci sono approvatori di secondo livello
								if ( approvatore2 != null )
								{
									var element = approvatore2.MatrLivello2.Where( w => w.Matricola.Equals( liv1.Matricola ) ).FirstOrDefault();

									// se l'approvatore di primo livello in esame è anche
									// approvatore di secondo livello per la sede esaminata
									if ( element != null )
									{
										current = GetRapportoConteggi( sede, reparto, null );
										current.Matricola = liv1.Matricola;

										toUpdate = riepilogo.Where( m => m.Matricola.Equals( liv1.Matricola ) ).FirstOrDefault();

										if ( toUpdate != null )
										{
											toUpdate.RichiesteUrg += current.RichiesteUrg;
											toUpdate.RichiesteSca += current.RichiesteSca;
											toUpdate.RichiesteOrd += current.RichiesteOrd;
											toUpdate.RichiesteUrgStorno += current.RichiesteUrgStorno;
											toUpdate.RichiesteScaStorno += current.RichiesteScaStorno;
											toUpdate.RichiesteOrdStorno += current.RichiesteOrdStorno;
										}
										else
										{
											riepilogo.Add( new Liv1Riepilogo()
											{
												Matricola = liv1.Matricola,
												RichiesteUrg = current.RichiesteUrg,
												RichiesteSca = current.RichiesteSca,
												RichiesteOrd = current.RichiesteOrd,
												RichiesteUrgStorno = current.RichiesteUrgStorno,
												RichiesteScaStorno = current.RichiesteScaStorno,
												RichiesteOrdStorno = current.RichiesteOrdStorno
											} );
										}
									}
									else
									{
										// se il livello 1 non è anche livello2 per la sede esaminata, allora
										// al livello 1 dovrà approvare soltanto le richieste che non sono sue

										// conto le richieste che il livello 1 deve approvare
										string mtr = liv1.Matricola.Replace( "P", "" );

										// reperimento delle richieste che non sono state create dalla matricola corrente
										current = GetRapportoConteggi( sede, reparto, mtr );
										current.Matricola = liv1.Matricola;

										toUpdate = riepilogo.Where( m => m.Matricola.Equals( liv1.Matricola ) ).FirstOrDefault();

										if ( toUpdate != null )
										{
											toUpdate.RichiesteUrg += current.RichiesteUrg;
											toUpdate.RichiesteSca += current.RichiesteSca;
											toUpdate.RichiesteOrd += current.RichiesteOrd;
											toUpdate.RichiesteUrgStorno += current.RichiesteUrgStorno;
											toUpdate.RichiesteScaStorno += current.RichiesteScaStorno;
											toUpdate.RichiesteOrdStorno += current.RichiesteOrdStorno;
										}
										else
										{
											riepilogo.Add( new Liv1Riepilogo()
											{
												Matricola = liv1.Matricola,
												RichiesteUrg = current.RichiesteUrg,
												RichiesteSca = current.RichiesteSca,
												RichiesteOrd = current.RichiesteOrd,
												RichiesteUrgStorno = current.RichiesteUrgStorno,
												RichiesteScaStorno = current.RichiesteScaStorno,
												RichiesteOrdStorno = current.RichiesteOrdStorno
											} );
										}

										// ora invece bisogna contare le richieste che devono approvare 
										// i livelli 2 per quella sede.. 
										// richieste effettuate dal livello 1 che non se le può autoapprovare
										current = GetRapportoConteggiPerLiv2( sede, reparto, liv1.Matricola );

										var apprLiv2 = approvatore2.MatrLivello2.Select( x => x.Matricola ).ToList();

										if ( apprLiv2 != null )
										{
											apprLiv2.ForEach( a =>
											{
												toUpdate = riepilogo.Where( m => m.Matricola.Equals( a ) ).FirstOrDefault();

												if ( toUpdate != null )
												{
													toUpdate.RichiesteUrg += current.RichiesteUrg;
													toUpdate.RichiesteSca += current.RichiesteSca;
													toUpdate.RichiesteOrd += current.RichiesteOrd;
													toUpdate.RichiesteUrgStorno += current.RichiesteUrgStorno;
													toUpdate.RichiesteScaStorno += current.RichiesteScaStorno;
													toUpdate.RichiesteOrdStorno += current.RichiesteOrdStorno;
												}
												else
												{
													riepilogo.Add( new Liv1Riepilogo()
													{
														Matricola = a,
														RichiesteUrg = current.RichiesteUrg,
														RichiesteSca = current.RichiesteSca,
														RichiesteOrd = current.RichiesteOrd,
														RichiesteUrgStorno = current.RichiesteUrgStorno,
														RichiesteScaStorno = current.RichiesteScaStorno,
														RichiesteOrdStorno = current.RichiesteOrdStorno
													} );
												}
											} );
										}
									}
								}
								else
								{
									// se non ci sono approvatori di secondo livello allora sarà il primo livello
									// a doverle approvare
									current = GetRapportoConteggi( sede, reparto, null );
									current.Matricola = liv1.Matricola;

									toUpdate = riepilogo.Where( m => m.Matricola.Equals( liv1.Matricola ) ).FirstOrDefault();

									if ( toUpdate != null )
									{
										toUpdate.RichiesteUrg += current.RichiesteUrg;
										toUpdate.RichiesteSca += current.RichiesteSca;
										toUpdate.RichiesteOrd += current.RichiesteOrd;
										toUpdate.RichiesteUrgStorno += current.RichiesteUrgStorno;
										toUpdate.RichiesteScaStorno += current.RichiesteScaStorno;
										toUpdate.RichiesteOrdStorno += current.RichiesteOrdStorno;
									}
									else
									{
										riepilogo.Add( new Liv1Riepilogo()
										{
											Matricola = liv1.Matricola,
											RichiesteUrg = current.RichiesteUrg,
											RichiesteSca = current.RichiesteSca,
											RichiesteOrd = current.RichiesteOrd,
											RichiesteUrgStorno = current.RichiesteUrgStorno,
											RichiesteScaStorno = current.RichiesteScaStorno,
											RichiesteOrdStorno = current.RichiesteOrdStorno
										} );
									}
								}
							}
						}
					}
				}

				if ( riepilogo != null )
				{
					foreach ( var w in riepilogo )
					{
						CommonTasks.Log( String.Format( "Matricola {0}, Urgenti {1}, Scadute {2}, Ordinarie {3}, UrgentiStorni {4}, ScaduteStorni {5}, OrdinarieStorni {6} {7}\r\n", w.Matricola,
							w.RichiesteUrg, w.RichiesteSca, w.RichiesteOrd, w.RichiesteUrgStorno, w.RichiesteScaStorno, w.RichiesteOrdStorno, DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

						MailTemplate = MailParams[0];
						if ( SoloStorni )
						{
							MailTemplate = MailTemplate
						   .Replace( "#SURG", w.RichiesteUrgStorno.ToString() )
						   .Replace( "#SSCA", w.RichiesteScaStorno.ToString() )
						   .Replace( "#SORD", w.RichiesteOrdStorno.ToString() );
						}
						else
						{
							MailTemplate = MailTemplate
							.Replace( "#URG", w.RichiesteUrg.ToString() )
							.Replace( "#SCA", w.RichiesteSca.ToString() )
							.Replace( "#ORD", w.RichiesteOrd.ToString() )
							.Replace( "#SURG", w.RichiesteUrgStorno.ToString() )
							.Replace( "#SSCA", w.RichiesteScaStorno.ToString() )
							.Replace( "#SORD", w.RichiesteOrdStorno.ToString() );
						}

						string oreMinutiRiferimento = DateTime.Now.Hour.ToString().PadLeft( 2, '0' ) + "00";

						MyRai_InvioNotifiche impNotCurrent = new MyRai_InvioNotifiche();

                        impNotCurrent = _db.MyRai_InvioNotifiche.Where( x => x.Matricola == w.Matricola.Replace( "P" , "" ) && x.TipoDestinatario == "ESS_LIV1" && x.TipoEvento == ( SoloStorni ? "INSS" : "INSR" ) ).FirstOrDefault( );

                        if ( impNotCurrent == null )
                        {
                            impNotCurrent = _db.MyRai_InvioNotifiche.Where( x => x.Matricola == "*" && x.TipoDestinatario == "ESS_LIV1" && x.TipoEvento == ( SoloStorni ? "INSS" : "INSR" ) ).FirstOrDefault( );

                            if ( impNotCurrent == null )
                            {
                                CommonTasks.Log( String.Format( "Impostazione invio di default non presente {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
                                return;
                            }
                        }

                        if ( impNotCurrent.TipoInvio == "I" || impNotCurrent.TipoInvio == "N" )
						{
							CommonTasks.Log( String.Format( "Mail a {0} non inviata perchè la configurazione non prevede l'invio per questa data - {1}\r\n", w.Matricola, DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

							continue;
						}

						if ( impDefault.Id == impNotCurrent.Id )
						{
                            if ( DontSendDefault )
                                continue;
						}

						if ( impNotCurrent.UltimoInvio != null && impNotCurrent.Id != impDefault.Id &&
							( ( DateTime )impNotCurrent.UltimoInvio ).Date == DateTime.Now.Date )
						{
							continue;
						}					

						if ( ( impNotCurrent.TipoInvio == "G" && impNotCurrent.OraMinuti == oreMinutiRiferimento )
							||
							( impNotCurrent.TipoInvio == "S" && impNotCurrent.GiornoDellaSettimana == ( int )DateTime.Now.DayOfWeek && impNotCurrent.OraMinuti == oreMinutiRiferimento )
							)
						{
							bool daInviare = false;
							if ( SoloStorni )
							{
								if ( w.RichiesteOrdStorno > 0 ||
									w.RichiesteScaStorno > 0 ||
									w.RichiesteUrgStorno > 0 )
								{
									CommonTasks.Log( String.Format( "Invio mail a {0}: - Conteggio Storni Ord = {1}, Sca = {2}, Urg = {3} - {4}\r\n", w.Matricola, w.RichiesteOrd, w.RichiesteSca, w.RichiesteUrg, DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

									daInviare = true;
								}
							}
							else
							{
								if ( w.RichiesteOrd > 0 || w.RichiesteOrdStorno > 0 ||
									w.RichiesteSca > 0 || w.RichiesteScaStorno > 0 ||
									w.RichiesteUrg > 0 || w.RichiesteUrgStorno > 0 )
								{
									CommonTasks.Log( String.Format( "Invio mail a {0}: - Conteggio Ord = {1}, Sca = {2}, Urg = {3}, StornoOrd = {4}, StornoSca = {5}, StornoUrg = {6} - {7}\r\n", w.Matricola, w.RichiesteOrd, w.RichiesteSca, w.RichiesteUrg, w.RichiesteOrdStorno, w.RichiesteScaStorno, w.RichiesteUrgStorno, DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

									daInviare = true;
								}
                                else
                                {
                                    CommonTasks.Log( String.Format( "Non ci sono richieste per la matricola {0} - {1}\r\n" , w.Matricola , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
                                }
							}

							if ( daInviare )
							{
								bool success = InvioMail( MailTemplate, MailSubject, w.Matricola );
								if ( success )
								{
									using ( digiGappEntities db = new digiGappEntities() )
									{
										var n = db.MyRai_InvioNotifiche.Where( x => x.Id == impNotCurrent.Id ).FirstOrDefault();
										if ( n != null )
										{
											n.UltimoInvio = DateTime.Now;
										}

										string dest = w.Matricola.Replace( "P", "" );

										var notificheDaMarcare = db.MyRai_Notifiche.Where( nn => nn.matricola_destinatario.Equals( dest ) ).ToList();

										if ( notificheDaMarcare != null && notificheDaMarcare.Any() )
										{
											notificheDaMarcare.ForEach( ww =>
											{
												ww.data_inviata = DateTime.Now;
											} );
										}

										db.SaveChanges();
									}
								}
							}
							else
							{
								CommonTasks.Log( String.Format( "Mail non inviata perchè l'utente non ha richieste in approvazione - {0}\r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
							}
						}
						else
						{
							CommonTasks.Log( String.Format( "Mail non inviata perchè la configurazione non prevede l'invio per questa data - {0}\r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				CommonTasks.Log( String.Format( "------------------------------------\r\n" ) );
				CommonTasks.Log( String.Format( "Si è verificato un errore \r\n{0}\r\n{1}", ex.Message, DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) ) );
			}

			CommonTasks.Log( String.Format( "Termine invio riepilogo notifiche per approvatore livello 1 data e ora: {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
		}

        #region

        public void StartApprovatoriProduzione ( bool SoloStorni = false )
        {
            CommonTasks.Log( String.Format( "\r\n" ) );
            CommonTasks.Log( String.Format( "--------------------------------------------------" ) );
            CommonTasks.Log( String.Format( "Avvio invio riepilogo notifiche per approvatore di produzione data e ora: {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
            List<string> matricoleApprovatori = new List<string>( );

            try
            {
                Boolean DontSendDefault = false;
                MyRai_InvioNotifiche impDefault = new MyRai_InvioNotifiche( );
                string[] MailParams = null;
                List<Liv1Riepilogo> riepilogo = new List<Liv1Riepilogo>( );
                Liv1Riepilogo toUpdate = new Liv1Riepilogo( );

                digiGappEntities _db = new digiGappEntities( );

                if ( SoloStorni )
                {
                    MailParams = CommonTasks.GetParametri<string>
                          ( CommonTasks.EnumParametriSistema.MailTemplateNotificheStorniBossL1 );
                }
                else
                {
                    MailParams = CommonTasks.GetParametri<string>
                       ( CommonTasks.EnumParametriSistema.MailTemplateNotificheBossL1 );
                }

                string MailTemplate = MailParams[0];
                string MailSubject = MailParams[1];

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    matricoleApprovatori = db.MyRai_ApprovatoreProduzione.Select( w => w.MatricolaApprovatore ).Distinct( ).ToList( );
                }

                if ( matricoleApprovatori != null && matricoleApprovatori.Any( ) )
                {
                    foreach ( var approvatore in matricoleApprovatori )
                    {
                        impDefault = _db.MyRai_InvioNotifiche.Where( x => x.Matricola == approvatore.Replace( "P" , "" ) && x.TipoDestinatario == "ESS_LIV1" && x.TipoEvento == ( SoloStorni ? "INSS" : "INSR" ) ).FirstOrDefault( );

                        if ( impDefault == null )
                        {
                            impDefault = _db.MyRai_InvioNotifiche.Where( x => x.Matricola == "*" && x.TipoDestinatario == "ESS_LIV1" && x.TipoEvento == ( SoloStorni ? "INSS" : "INSR" ) ).FirstOrDefault( );

                            if ( impDefault == null )
                            {
                                CommonTasks.Log( String.Format( "Impostazione invio di default non presente {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
                                return;
                            }
                        }

                        DontSendDefault = false;

                        if ( impDefault.UltimoInvio != null && ( ( DateTime ) impDefault.UltimoInvio ).Date == DateTime.Now.Date )
                        {
                            DontSendDefault = true;
                        }

                        Liv1Riepilogo current = GetRapportoConteggiApprovatoreProduzione( approvatore );

                        riepilogo.Add( new Liv1Riepilogo( )
                        {
                            Matricola = approvatore ,
                            RichiesteUrg = current.RichiesteUrg ,
                            RichiesteSca = current.RichiesteSca ,
                            RichiesteOrd = current.RichiesteOrd ,
                            RichiesteUrgStorno = current.RichiesteUrgStorno ,
                            RichiesteScaStorno = current.RichiesteScaStorno ,
                            RichiesteOrdStorno = current.RichiesteOrdStorno
                        } );
                    }
                }

                if ( riepilogo != null )
                {
                    foreach ( var w in riepilogo )
                    {
                        CommonTasks.Log( String.Format( "Matricola {0}, Urgenti {1}, Scadute {2}, Ordinarie {3}, UrgentiStorni {4}, ScaduteStorni {5}, OrdinarieStorni {6} {7}\r\n" , w.Matricola ,
                            w.RichiesteUrg , w.RichiesteSca , w.RichiesteOrd , w.RichiesteUrgStorno , w.RichiesteScaStorno , w.RichiesteOrdStorno , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

                        MailTemplate = MailParams[0];
                        if ( SoloStorni )
                        {
                            MailTemplate = MailTemplate
                           .Replace( "#SURG" , w.RichiesteUrgStorno.ToString( ) )
                           .Replace( "#SSCA" , w.RichiesteScaStorno.ToString( ) )
                           .Replace( "#SORD" , w.RichiesteOrdStorno.ToString( ) );
                        }
                        else
                        {
                            MailTemplate = MailTemplate
                            .Replace( "#URG" , w.RichiesteUrg.ToString( ) )
                            .Replace( "#SCA" , w.RichiesteSca.ToString( ) )
                            .Replace( "#ORD" , w.RichiesteOrd.ToString( ) )
                            .Replace( "#SURG" , w.RichiesteUrgStorno.ToString( ) )
                            .Replace( "#SSCA" , w.RichiesteScaStorno.ToString( ) )
                            .Replace( "#SORD" , w.RichiesteOrdStorno.ToString( ) );
                        }

                        string oreMinutiRiferimento = DateTime.Now.Hour.ToString( ).PadLeft( 2 , '0' ) + "00";

                        MyRai_InvioNotifiche impNotCurrent = new MyRai_InvioNotifiche( );

                        impNotCurrent = _db.MyRai_InvioNotifiche.Where( x => x.Matricola == w.Matricola.Replace( "P" , "" ) && x.TipoDestinatario == "ESS_LIV1" && x.TipoEvento == ( SoloStorni ? "INSS" : "INSR" ) ).FirstOrDefault( );

                        if ( impNotCurrent == null )
                        {
                            impNotCurrent = _db.MyRai_InvioNotifiche.Where( x => x.Matricola == "*" && x.TipoDestinatario == "ESS_LIV1" && x.TipoEvento == ( SoloStorni ? "INSS" : "INSR" ) ).FirstOrDefault( );

                            if ( impNotCurrent == null )
                            {
                                CommonTasks.Log( String.Format( "Impostazione invio di default non presente {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
                                return;
                            }
                        }

                        if ( impNotCurrent.TipoInvio == "I" || impNotCurrent.TipoInvio == "N" )
                        {
                            CommonTasks.Log( String.Format( "Mail a {0} non inviata perchè la configurazione non prevede l'invio per questa data - {1}\r\n" , w.Matricola , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

                            continue;
                        }

                        if ( impDefault.Id == impNotCurrent.Id )
                        {
                            if ( DontSendDefault )
                                continue;
                        }

                        if ( impNotCurrent.UltimoInvio != null && impNotCurrent.Id != impDefault.Id &&
                            ( ( DateTime ) impNotCurrent.UltimoInvio ).Date == DateTime.Now.Date )
                        {
                            continue;
                        }

                        if ( ( impNotCurrent.TipoInvio == "G" && impNotCurrent.OraMinuti == oreMinutiRiferimento )
                            ||
                            ( impNotCurrent.TipoInvio == "S" && impNotCurrent.GiornoDellaSettimana == ( int ) DateTime.Now.DayOfWeek && impNotCurrent.OraMinuti == oreMinutiRiferimento )
                            )
                        {
                            bool daInviare = false;
                            if ( SoloStorni )
                            {
                                if ( w.RichiesteOrdStorno > 0 ||
                                    w.RichiesteScaStorno > 0 ||
                                    w.RichiesteUrgStorno > 0 )
                                {
                                    CommonTasks.Log( String.Format( "Invio mail a {0}: - Conteggio Storni Ord = {1}, Sca = {2}, Urg = {3} - {4}\r\n" , w.Matricola , w.RichiesteOrd , w.RichiesteSca , w.RichiesteUrg , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

                                    daInviare = true;
                                }
                            }
                            else
                            {
                                if ( w.RichiesteOrd > 0 || w.RichiesteOrdStorno > 0 ||
                                    w.RichiesteSca > 0 || w.RichiesteScaStorno > 0 ||
                                    w.RichiesteUrg > 0 || w.RichiesteUrgStorno > 0 )
                                {
                                    CommonTasks.Log( String.Format( "Invio mail a {0}: - Conteggio Ord = {1}, Sca = {2}, Urg = {3}, StornoOrd = {4}, StornoSca = {5}, StornoUrg = {6} - {7}\r\n" , w.Matricola , w.RichiesteOrd , w.RichiesteSca , w.RichiesteUrg , w.RichiesteOrdStorno , w.RichiesteScaStorno , w.RichiesteUrgStorno , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

                                    daInviare = true;
                                }
                                else
                                {
                                    CommonTasks.Log( String.Format( "Non ci sono richieste per la matricola {0} - {1}\r\n" , w.Matricola , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
                                }
                            }

                            if ( daInviare )
                            {
                                bool success = InvioMail( MailTemplate , MailSubject , w.Matricola );
                                if ( success )
                                {
                                    using ( digiGappEntities db = new digiGappEntities( ) )
                                    {
                                        var n = db.MyRai_InvioNotifiche.Where( x => x.Id == impNotCurrent.Id ).FirstOrDefault( );
                                        if ( n != null )
                                        {
                                            n.UltimoInvio = DateTime.Now;
                                        }

                                        string dest = w.Matricola.Replace( "P" , "" );

                                        var notificheDaMarcare = db.MyRai_Notifiche.Where( nn => nn.matricola_destinatario.Equals( dest ) ).ToList( );

                                        if ( notificheDaMarcare != null && notificheDaMarcare.Any( ) )
                                        {
                                            notificheDaMarcare.ForEach( ww =>
                                            {
                                                ww.data_inviata = DateTime.Now;
                                            } );
                                        }

                                        db.SaveChanges( );
                                    }
                                }
                            }
                            else
                            {
                                CommonTasks.Log( String.Format( "Mail non inviata perchè l'utente non ha richieste in approvazione - {0}\r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
                            }
                        }
                        else
                        {
                            CommonTasks.Log( String.Format( "Mail non inviata perchè la configurazione non prevede l'invio per questa data - {0}\r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                CommonTasks.Log( String.Format( "------------------------------------\r\n" ) );
                CommonTasks.Log( String.Format( "Si è verificato un errore \r\n{0}\r\n{1}" , ex.Message , DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) ) );
            }

            CommonTasks.Log( String.Format( "Termine invio riepilogo notifiche per approvatore di produzione data e ora: {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
        }


        private Liv1Riepilogo GetRapportoConteggiApprovatoreProduzione ( string matricolaApprovatore )
        {
            Liv1Riepilogo result = new Liv1Riepilogo( );
            digiGappEntities db = new digiGappEntities( );
            List<MyRai_ApprovatoreProduzione> approvatoreData = new List<MyRai_ApprovatoreProduzione>( );
            string mtr = string.Empty;

            try
            {
                List<string> matricoleTarget = new List<string>( );

                try
                {
                    approvatoreData = db.MyRai_ApprovatoreProduzione.Where( w => w.MatricolaApprovatore.Equals( matricolaApprovatore ) ).ToList( );
                }
                catch ( Exception ex )
                {
                    approvatoreData = new List<MyRai_ApprovatoreProduzione>( );
                }

                foreach ( var app in approvatoreData )
                {
                    mtr = app.MatricolaUtente;
                    List<int> idAttivita = new List<int>( );

                    try
                    {
                        idAttivita = db.MyRai_AttivitaCeiton.Where( w => w.Titolo.Equals( app.Titolo ) ).Select( w => w.id ).ToList( );
                    }
                    catch ( Exception ex )
                    {
                        idAttivita = new List<int>( );
                    }

                    result.RichiesteUrg += db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
                                                                    !r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
                                                                    r.urgente &&
                                                                    ( r.id_Attivita_ceiton != null ? idAttivita.Contains( r.id_Attivita_ceiton.Value ) : idAttivita.Contains( 0 ) ) &&
                                                                    !r.matricola_richiesta.Equals( mtr ) );

                    result.RichiesteSca += db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
                                        !r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
                                        r.scaduta &&
                                        ( r.id_Attivita_ceiton != null ? idAttivita.Contains( r.id_Attivita_ceiton.Value ) : idAttivita.Contains( 0 ) ) &&
                                        !r.matricola_richiesta.Equals( mtr ) );

                    result.RichiesteOrd += db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
                                        !r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
                                        !r.scaduta &&
                                        !r.urgente &&
                                        ( r.id_Attivita_ceiton != null ? idAttivita.Contains( r.id_Attivita_ceiton.Value ) : idAttivita.Contains( 0 ) ) &&
                                        !r.matricola_richiesta.Equals( mtr ) );

                    result.RichiesteUrgStorno += db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
                                        r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
                                        r.urgente &&
                                        ( r.id_Attivita_ceiton != null ? idAttivita.Contains( r.id_Attivita_ceiton.Value ) : idAttivita.Contains( 0 ) ) &&
                                        !r.matricola_richiesta.Equals( mtr ) );

                    result.RichiesteScaStorno += db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
                                        r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
                                        r.scaduta &&
                                        ( r.id_Attivita_ceiton != null ? idAttivita.Contains( r.id_Attivita_ceiton.Value ) : idAttivita.Contains( 0 ) ) &&
                                        !r.matricola_richiesta.Equals( mtr ) );

                    result.RichiesteOrdStorno += db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
                                        r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
                                        !r.scaduta &&
                                        !r.urgente &&
                                        ( r.id_Attivita_ceiton != null ? idAttivita.Contains( r.id_Attivita_ceiton.Value ) : idAttivita.Contains( 0 ) ) &&
                                        !r.matricola_richiesta.Equals( mtr ) );
                }
            }
            catch ( Exception ex )
            {
                result = new Liv1Riepilogo( );
                CommonTasks.Log( String.Format( "Errore in GetRapportoConteggiFiltraMatricola. Dati di input: matricola {0}" , matricolaApprovatore , ex.ToString( ) , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
            }
            return result;
        }

        #endregion

		#region Private 

		private List<AbilitazioneSede> GetSediL1 ()
		{
			var ab = CommonTasks.getAbilitazioni();
			return ab.ListaAbilitazioni.Where( w => w.MatrLivello1.Any() ).ToList();
		}

		private List<myRaiCommonTasks.AbilitazioneSede> GetSediL2 ()
		{
			var ab = CommonTasks.getAbilitazioni();
			return ab.ListaAbilitazioni.Where( w => w.MatrLivello2.Any() ).ToList();
		}

		private Liv1Riepilogo GetRapportoConteggi ( string sede, string reparto = null, string matricola = null )
		{
			Liv1Riepilogo result = new Liv1Riepilogo();

			try
			{
				// se la matricola è diversa da null allora
				// bisogna filtrare per quella matricola
				if ( matricola != null )
				{
					result = GetRapportoConteggiFiltraMatricola( sede, reparto, matricola );
				}
				else
				{
					result = GetRapportoConteggiFiltraNOMatricola( sede, reparto );
				}
			}
			catch ( Exception ex )
			{
				result = new Liv1Riepilogo();

				CommonTasks.Log( String.Format( "Errore in GetRapportoConteggi. Dati di input: sede {0}, reparto {1}, matricola {2}\r\n{3}\r\n{4}\r\n", sede, reparto, matricola, ex.ToString(), DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
			}
			return result;
		}

		private Liv1Riepilogo GetRapportoConteggiFiltraMatricola ( string sede, string reparto = null, string matricola = null )
		{
			Liv1Riepilogo result = new Liv1Riepilogo();
			digiGappEntities db = new digiGappEntities();
			string mtr = string.Empty;

			try
			{
				mtr = matricola.Replace( "P", "" );

				if ( String.IsNullOrEmpty( reparto ) )
				{
					result.RichiesteUrg = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.urgente &&
										!r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteSca = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.scaduta &&
										!r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteOrd = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										!r.scaduta &&
										!r.urgente &&
										!r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteUrgStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.urgente &&
										!r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteScaStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.scaduta &&
										!r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteOrdStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										!r.scaduta &&
										!r.urgente &&
										!r.matricola_richiesta.Equals( mtr ) );
				}
				else
				{
					result.RichiesteUrg = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.urgente &&
										r.reparto == reparto &&
										!r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteSca = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.scaduta &&
										r.reparto == reparto &&
										!r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteOrd = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										!r.scaduta &&
										!r.urgente &&
										r.reparto == reparto &&
										!r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteUrgStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.urgente &&
										r.reparto == reparto &&
										!r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteScaStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.scaduta &&
										r.reparto == reparto &&
										!r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteOrdStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										!r.scaduta &&
										!r.urgente &&
										r.reparto == reparto &&
										!r.matricola_richiesta.Equals( mtr ) );
				}
			}
			catch ( Exception ex )
			{
				result = new Liv1Riepilogo();
				CommonTasks.Log( String.Format( "Errore in GetRapportoConteggiFiltraMatricola. Dati di input: sede {0}, reparto {1}, matricola {2}\r\n{3}\r\n{4}\r\n", sede, reparto, matricola, ex.ToString(), DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
			}
			return result;
		}

		private Liv1Riepilogo GetRapportoConteggiFiltraNOMatricola ( string sede, string reparto = null )
		{
			Liv1Riepilogo result = new Liv1Riepilogo();
			digiGappEntities db = new digiGappEntities();

			try
			{
				if ( String.IsNullOrEmpty( reparto ) )
				{
					result.RichiesteUrg = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.urgente );

					result.RichiesteSca = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.scaduta );

					result.RichiesteOrd = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										!r.scaduta &&
										!r.urgente );

					result.RichiesteUrgStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.urgente );

					result.RichiesteScaStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.scaduta );

					result.RichiesteOrdStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										!r.scaduta &&
										!r.urgente );
				}
				else
				{
					result.RichiesteUrg = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.urgente &&
										r.reparto == reparto );

					result.RichiesteSca = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.scaduta &&
										r.reparto == reparto );

					result.RichiesteOrd = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										!r.scaduta &&
										!r.urgente &&
										r.reparto == reparto );

					result.RichiesteUrgStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.urgente &&
										r.reparto == reparto );

					result.RichiesteScaStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.scaduta &&
										r.reparto == reparto );

					result.RichiesteOrdStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										!r.scaduta &&
										!r.urgente &&
										r.reparto == reparto );
				}
			}
			catch ( Exception ex )
			{
				result = new Liv1Riepilogo();
				CommonTasks.Log( String.Format( "Errore in GetRapportoConteggiFiltraNOMatricola. Dati di input: sede {0}, reparto {1}\r\n{2}\r\n{3}\r\n", sede, reparto, ex.ToString(), DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
			}
			return result;
		}

		private Liv1Riepilogo GetRapportoConteggiPerLiv2 ( string sede, string reparto = null, string matricola = null )
		{
			Liv1Riepilogo result = new Liv1Riepilogo();
			digiGappEntities db = new digiGappEntities();
			string mtr = string.Empty;

			try
			{
				mtr = matricola.Replace( "P", "" );

				if ( String.IsNullOrEmpty( reparto ) )
				{
					result.RichiesteUrg = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.urgente &&
										r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteSca = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.scaduta &&
										r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteOrd = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										!r.scaduta &&
										!r.urgente &&
										r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteUrgStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.urgente &&
										r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteScaStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.scaduta &&
										r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteOrdStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										!r.scaduta &&
										!r.urgente &&
										r.matricola_richiesta.Equals( mtr ) );
				}
				else
				{
					result.RichiesteUrg = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.urgente &&
										r.reparto == reparto &&
										r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteSca = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.scaduta &&
										r.reparto == reparto &&
										r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteOrd = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										!r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										!r.scaduta &&
										!r.urgente &&
										r.reparto == reparto &&
										r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteUrgStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.urgente &&
										r.reparto == reparto &&
										r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteScaStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										r.scaduta &&
										r.reparto == reparto &&
										r.matricola_richiesta.Equals( mtr ) );

					result.RichiesteOrdStorno = db.MyRai_Richieste.Count( r => r.id_stato == 10 &&
										r.codice_sede_gapp == sede &&
										r.MyRai_Eccezioni_Richieste.Any( a => a.azione == "C" ) &&
										!r.scaduta &&
										!r.urgente &&
										r.reparto == reparto &&
										r.matricola_richiesta.Equals( mtr ) );
				}
			}
			catch ( Exception ex )
			{
				result = new Liv1Riepilogo();
				CommonTasks.Log( String.Format( "Errore in GetRapportoConteggiPerLiv2. Dati di input: sede {0}, reparto {1}, matricola {2}\r\n{3}\r\n{4}\r\n", sede, reparto, matricola, ex.ToString(), DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
			}
			return result;
		}

		private bool InvioMail ( string body, string mailSubject, string destinatario )
		{
			bool result = true;
			try
			{
				string dest = CommonTasks.GetEmailPerMatricola( destinatario.TrimStart( 'P' ) );

				if ( String.IsNullOrWhiteSpace( dest ) )
				{
					dest = destinatario + "@rai.it";
				}

				GestoreMail mail = new GestoreMail();

                //dest = "RUO.SIP.PRESIDIOOPEN@rai.it";

				var response = mail.InvioMail( body, mailSubject, dest, "raiplace.selfservice@rai.it", myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailApprovazioneFrom );

				if ( response != null && response.Errore != null )
				{
					myRaiData.MyRai_LogErrori err = new MyRai_LogErrori()
					{
						applicativo = "Batch",
						data = DateTime.Now,
						provenienza = "NotificheDestinateL1 - InvioMail",
						error_message = response.Errore + " per " + dest
					};

					using ( digiGappEntities db = new digiGappEntities() )
					{
						db.MyRai_LogErrori.Add( err );
						db.SaveChanges();
					}

					result = false;
				}
			}
			catch ( Exception ex )
			{
				CommonTasks.Log( String.Format( "Errore in invio mail a {0}\r\n{1}\r\n{2}", destinatario,  ex.ToString(), DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
				result = false;
			}

			return result;
		}

		#endregion
	}
}