using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiCommonTasks.Helpers;
using myRaiCommonTasks.sendMail;
using myRaiData;

namespace myRaiCommonTasks
{
    public static class ILinqHelper
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey> ( this IEnumerable<TSource> source , Func<TSource , TKey> keySelector )
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>( );
            foreach ( TSource element in source )
            {
                if ( seenKeys.Add( keySelector( element ) ) )
                {
                    yield return element;
                }
            }
        }
    }


    public class NotificheDestinateDipendenti
    {

		public static void SendNotificheESS_DIP ( string targetMatricola = null )
		{
			List<NotificheMatricola> LNotifiche = new List<NotificheMatricola>();

			try
			{

				//DateTime dataLimite = new DateTime( DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0 );
				DateTime dataLimite = DateTime.Now;

				string tipoDestinatario = EnumTipoDestinatarioNotifica.ESS_DIP.ToString();
				string oreMinutiRiferimento = dataLimite.Hour.ToString().PadLeft( 2, '0' ) + dataLimite.Minute.ToString().PadLeft( 2, '0' );

				// reperimento di tutte le notifiche di approvazione da inviare 

				List<NotificheMatricola> listApp = GetNotificheESS_DIP_Approvazione( dataLimite, oreMinutiRiferimento, targetMatricola );
                CommonTasks.Log("Notifiche APPR trovate:" + listApp.Count());
                if ( listApp != null && listApp.Count > 0 )
                    LNotifiche.AddRange( listApp );
				
				// reperimento di tutte le notifiche di rifiuto da inviare

				List<NotificheMatricola> listRif = GetNotificheESS_DIP_Rifiuto( dataLimite, oreMinutiRiferimento, targetMatricola );
                CommonTasks.Log("Notifiche RIF trovate:" + listRif.Count());
				LNotifiche.AddRange( listRif );

                CommonTasks.Log("Totale notifiche:" + LNotifiche.Count());

				InvioMailNotifica( LNotifiche );
			}
			catch ( Exception ex )
			{

			}
		}

		public static List<NotificheMatricola> GetNotificheESS_DIP_Approvazione ( DateTime dataLimite, string oreMinutiRiferimento, string targetMatricola = null )
		{
			List<NotificheMatricola> response = new List<NotificheMatricola>();

			try
			{
				string tipoNotificaAPPR = EnumCategoriaEccezione.ApprovazioneEccezione.ToString();
				string tipoDestinatario = EnumTipoDestinatarioNotifica.ESS_DIP.ToString();
                List<MyRai_Notifiche> ListApprovazioniPerMatricola = new List<MyRai_Notifiche>( );

				using ( digiGappEntities db = new digiGappEntities() )
				{
					//legge notifiche APPROVAZIONI
					if ( String.IsNullOrEmpty( targetMatricola ) )
					{
                        var filtro = ( from n in db.MyRai_Notifiche
                                       where n.data_inviata == null
                                       && n.data_letta == null
                                       && n.data_inserita < dataLimite
                                       && ( n.descrizione.ToUpper( ).StartsWith( "RICHIESTA APP" ) || n.descrizione.ToUpper( ).StartsWith( "STORNO APP" ) )
                                       select n ).ToList( );

                        if ( filtro != null && filtro.Any( ) )
																			{
                            ListApprovazioniPerMatricola.AddRange( filtro.OrderBy( w => w.matricola_destinatario ).ThenBy( x => x.data_inserita ).ToList( ) );
                        }

                        var distintoPerMatricola = ListApprovazioniPerMatricola.DistinctBy( w => w.matricola_destinatario ).Select( w => w.matricola_destinatario ).ToList( );
						//verifica se per ogni matricola è da inviare e aggiunge in LNotifiche
                        foreach ( var item in distintoPerMatricola )
						{
                            var notifiche = ListApprovazioniPerMatricola.Where( w => w.matricola_destinatario.Equals( item ) ).ToList( );

                            if ( notifiche != null && notifiche.Any( ) )
                            {
                                notifiche = notifiche.OrderBy( w => w.matricola_destinatario ).ThenBy( x => x.data_inserita ).ToList( );
                            }

                            if ( IsToSendNow( item , tipoDestinatario , "APPR" , oreMinutiRiferimento ) )
							{
                                response.Add( new NotificheMatricola( ) { matricola = item , notifiche = notifiche.ToList( ) } );
							}
						}
					}
					else
					{
                        var filtro = ( from n in db.MyRai_Notifiche
                                       where n.data_inviata == null
                                       && n.data_letta == null
                                       && n.data_inserita < dataLimite
                                       && ( n.descrizione.ToUpper( ).StartsWith( "RICHIESTA APP" ) || n.descrizione.ToUpper( ).StartsWith( "STORNO APP" ) )
                                       && n.matricola_destinatario == targetMatricola
                                       select n ).ToList( );

                        if ( filtro != null && filtro.Any( ) )
																			{
                            ListApprovazioniPerMatricola.AddRange( filtro.OrderBy( w => w.matricola_destinatario ).ThenBy( x => x.data_inserita ).ToList( ) );
                        }

                        var distintoPerMatricola = ListApprovazioniPerMatricola.DistinctBy( w => w.matricola_destinatario ).Select( w => w.matricola_destinatario ).ToList( );
						//verifica se per ogni matricola è da inviare e aggiunge in LNotifiche
                        foreach ( var item in distintoPerMatricola )
						{
                            var notifiche = ListApprovazioniPerMatricola.Where( w => w.matricola_destinatario.Equals( item ) ).ToList( );

                            if ( notifiche != null && notifiche.Any( ) )
                            {
                                notifiche = notifiche.OrderBy( w => w.matricola_destinatario ).ThenBy( x => x.data_inserita ).ToList( );
                            }

                            if ( IsToSendNow( item , tipoDestinatario , "APPR" , oreMinutiRiferimento ) )
							{
                                response.Add( new NotificheMatricola( ) { matricola = item , notifiche = notifiche.ToList( ) } );
							}
						}
					}
				}
			}
			catch ( Exception ex )
			{
				response = new List<NotificheMatricola>();
			}

			return response;
		}

		public static List<NotificheMatricola> GetNotificheESS_DIP_Rifiuto ( DateTime dataLimite, string oreMinutiRiferimento, string targetMatricola = null )
		{
			List<NotificheMatricola> response = new List<NotificheMatricola>();

			try
			{
				string tipoNotificaRIF = EnumCategoriaEccezione.RifiutoEccezione.ToString();
				string tipoDestinatario = EnumTipoDestinatarioNotifica.ESS_DIP.ToString();

				using ( digiGappEntities db = new digiGappEntities() )
				{

					if ( String.IsNullOrEmpty( targetMatricola ) )
					{
						//legge notifiche RIFIUTO
						var ListApprovazioniPerMatricola = db.MyRai_Notifiche.Where( x => x.data_inviata == null &&
																						x.data_letta == null &&
																					   ( 
                                                                                       //x.descrizione.ToUpper().StartsWith( "RICHIESTA CANC" ) || 
                                                                                       x.descrizione.ToUpper().StartsWith( "RICHIESTA RIF" ) ||
																					   x.descrizione.ToUpper().StartsWith( "STORNO RIF" ) ) &&
																						x.data_inserita < dataLimite )
																			.GroupBy(
																				p => p.matricola_destinatario,
																				p => p,
																				( key, g ) => new
																				{
																					matricola = key,
																					notifiche = g.OrderBy( x =>
																					x.data_inserita )
																				} );

						//verifica se per ogni matricola è da inviare e aggiunge in LNotifiche
						foreach ( var item in ListApprovazioniPerMatricola )
						{
							if ( IsToSendNow( item.matricola, tipoDestinatario, "RIF", oreMinutiRiferimento ) )
							{
								response.Add( new NotificheMatricola() { matricola = item.matricola, notifiche = item.notifiche.ToList() } );
							}
						}
					}
					else
					{
						//legge notifiche RIFIUTO
						var ListApprovazioniPerMatricola = db.MyRai_Notifiche.Where( x => x.data_inviata == null &&
																						x.data_letta == null &&
																					   ( x.descrizione.ToUpper().StartsWith( "RICHIESTA CANC" ) || x.descrizione.ToUpper().StartsWith( "RICHIESTA RIF" ) ||
																					   x.descrizione.ToUpper().StartsWith( "STORNO RIF" ) ) &&
																						x.data_inserita < dataLimite &&
																						x.matricola_destinatario.Equals( targetMatricola ) )
																			.GroupBy(
																				p => p.matricola_destinatario,
																				p => p,
																				( key, g ) => new
																				{
																					matricola = key,
																					notifiche = g.OrderBy( x =>
																					x.data_inserita )
																				} );

						//verifica se per ogni matricola è da inviare e aggiunge in LNotifiche
						foreach ( var item in ListApprovazioniPerMatricola )
						{
							if ( IsToSendNow( item.matricola, tipoDestinatario, "RIF", oreMinutiRiferimento ) )
							{
								response.Add( new NotificheMatricola() { matricola = item.matricola, notifiche = item.notifiche.ToList() } );
							}
						}
					}
				}
			}
			catch ( Exception ex )
			{
				response = new List<NotificheMatricola>();
			}

			return response;
		}

		public static Boolean IsToSendNow ( string matricola, string tipodestinatario, string tipoevento, string oreMinutiRiferimento )
		{
			using ( digiGappEntities db = new digiGappEntities() )
			{

				var ImpostazioneNotifica = GetImpostazioneNotifiche( matricola, tipodestinatario, tipoevento );

                if ( ImpostazioneNotifica == null )
                    return false;

				if ( ImpostazioneNotifica.TipoInvio == "N" )
					return false;

                if ( ImpostazioneNotifica.TipoInvio == "I" )
                    return true;


				if ( ImpostazioneNotifica.TipoInvio == "G" )
					return ( Convert.ToInt32( DateTime.Now.ToString( "HHmm" ) ) >= Convert.ToInt32( ImpostazioneNotifica.OraMinuti == null ? "0000" : ImpostazioneNotifica.OraMinuti ) );
				else
				{
                    if ( ImpostazioneNotifica.GiornoDellaSettimana == null )
                        ImpostazioneNotifica.GiornoDellaSettimana = 1;
					return ( ImpostazioneNotifica.GiornoDellaSettimana == ( int )DateTime.Today.DayOfWeek &&
						Convert.ToInt32( DateTime.Now.ToString( "HHmm" ) ) >= Convert.ToInt32( ImpostazioneNotifica.OraMinuti == null ? "0000" : ImpostazioneNotifica.OraMinuti ) );
				}
			}
		}

        public static MyRai_InvioNotifiche GetImpostazioneNotifiche ( string matricola, string tipodestinatario, string tipoevento )
		{
			MyRai_InvioNotifiche ImpostazioneNotifica = new MyRai_InvioNotifiche();
			using ( digiGappEntities db = new digiGappEntities() )
			{
				ImpostazioneNotifica = db.MyRai_InvioNotifiche.Where( x => x.InvioAttivo == true &&
																		x.TipoDestinatario == tipodestinatario &&
																		x.TipoEvento == tipoevento &&
																		( x.Matricola == matricola || x.Matricola == "*" ) )
																.OrderByDescending( x => x.Matricola )
																.FirstOrDefault();
			}

			return ImpostazioneNotifica;
		}
			
        public class NotificheTemp
        {
            public MyRai_Richieste richiesta { get; set; }

            public MyRai_Eccezioni_Richieste richiesta_ecc { get; set; }

            public MyRai_Notifiche notifica { get; set; }

            public string Nota { get; set; }
        }


		public static void InvioMailNotifica(List<NotificheMatricola> notifiche)
		{
			try
			{
                List<MyRai_Notifiche> toRemove = new List<MyRai_Notifiche>( );
				var db = new digiGappEntities();

				var NotificheDaInviarePerMatr = notifiche.GroupBy(
					 item => item.matricola,
					 item => item.notifiche,
						( key, g ) => new
						{
							matricola = key,
							notifiche = g.SelectMany( n => n ).ToList()
						} ).ToList();

				string[] MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>( myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateNotificheDipendente );

				string MailTemplate = MailParams[0];
				string MailSubject = MailParams[1];

				foreach ( var item in NotificheDaInviarePerMatr )
				{
					string dest = myRaiCommonTasks.CommonTasks.GetEmailPerMatricola( item.matricola.TrimStart( 'P' ) );

					string corpoMail = String.Empty;

					foreach ( var notifica in item.notifiche )
					{
						string td1 = String.Empty;
						string td2 = String.Empty;
						string td3 = String.Empty;

						if ( notifica.categoria == EnumCategoriaEccezione.ApprovazioneEccezione.ToString()
							|| notifica.categoria == EnumCategoriaEccezione.RifiutoEccezione.ToString() )
						{
                            List<NotificheTemp> items = new List<NotificheTemp>( );
                            // Prima il batch eseguiva soltanto l'ultimo else, questo però crea qualche difficoltà nella creazione della
                            // notifica, ad esempio per notifiche di storno, venivano presti 2 record, quello di inserimento richiesta e 
                            // quello dello storno, poi nella notifica veniva riportata la data del primo record in eccezioni richieste
                            // col relativo approvatore, cosa che creava problemi nel caso di cambio sede, perchè l'approvatore non era più
                            // lo stesso.
                            if (notifica.descrizione.ToUpper().StartsWith("RICHIESTA APPROVATA"))
                            {
                                items = ( from r_ecc in db.MyRai_Eccezioni_Richieste
                                          join r in db.MyRai_Richieste
                                              on r_ecc.id_richiesta equals r.id_richiesta
                                          join r_notifiche in db.MyRai_Notifiche
                                              on r.id_richiesta equals r_notifiche.id_riferimento
                                          where r_notifiche.id == notifica.id &&
                                                  r_ecc.azione == "I" &&
                                                  r_ecc.id_stato == 20
                                          orderby r.id_richiesta descending
                                          select new NotificheTemp( )
                                          {
                                              richiesta = r ,
                                              notifica = r_notifiche ,
                                              richiesta_ecc = r_ecc ,
                                              Nota = r_ecc.nota_rifiuto_o_approvazione
                                          } ).ToList( );
                                        
                            }
                            else if ( notifica.descrizione.ToUpper( ).StartsWith( "STORNO APPROVATO" ) )
                            {
                                items = ( from r_ecc in db.MyRai_Eccezioni_Richieste
                                          join r in db.MyRai_Richieste
                                              on r_ecc.id_richiesta equals r.id_richiesta
                                          join r_notifiche in db.MyRai_Notifiche
                                              on r.id_richiesta equals r_notifiche.id_riferimento
                                          where r_notifiche.id == notifica.id &&
                                                  r_ecc.azione == "C" &&
                                                  r_ecc.id_stato == 20
                                          orderby r.id_richiesta descending
                                          select new NotificheTemp( )
                                          {
                                              richiesta = r ,
                                              notifica = r_notifiche ,
                                              richiesta_ecc = r_ecc ,
                                              Nota = r_ecc.nota_rifiuto_o_approvazione
                                          } ).ToList( );
                            }
                            else
                            {
                                items = ( from r_ecc in db.MyRai_Eccezioni_Richieste
										join r in db.MyRai_Richieste
											on r_ecc.id_richiesta equals r.id_richiesta
										join r_notifiche in db.MyRai_Notifiche
											on r.id_richiesta equals r_notifiche.id_riferimento
										where r_notifiche.id == notifica.id
										orderby r.id_richiesta descending
                                          select new NotificheTemp( )
                                          {
                                              richiesta = r ,
                                              notifica = r_notifiche ,
                                              richiesta_ecc = r_ecc ,
                                              Nota = r_ecc.nota_rifiuto_o_approvazione
                                          } ).ToList( );
                            }

							if (items != null && items.Any())
							{
								foreach ( var i in items )
								{
                                    string data_appr_rifiuto = "";
                                    string notaRifiuto = "";

                                    if (i.notifica.descrizione.ToLower().StartsWith("richiesta app") || i.notifica.descrizione.ToLower().StartsWith("storno app"))
                                                data_appr_rifiuto =
                                                    i.richiesta_ecc.data_validazione_primo_livello==null ?"":
                                                    ((DateTime)i.richiesta_ecc.data_validazione_primo_livello).ToString("dd/MM/yyyy");
                                    else
                                    {
                                                data_appr_rifiuto =
                                                    i.richiesta_ecc.data_rifiuto_primo_livello==null ?"":
                                                    ((DateTime)i.richiesta_ecc.data_rifiuto_primo_livello).ToString("dd/MM/yyyy");
                                    }

                                    var search = db.MyRai_Eccezioni_Richieste.Where( w => w.id_eccezioni_richieste == i.richiesta_ecc.id_eccezioni_richieste ).FirstOrDefault( );
                                    if ( search != null )
                                    {
                                        notaRifiuto = search.nota_rifiuto_o_approvazione;
                                    }

                                    if ( !String.IsNullOrEmpty( i.notifica.descrizione ) )
                                    {
                                        string descApprovatore = i.richiesta_ecc.nominativo_primo_livello;
                                        // FRANCESCO chiedere conferma se attivare questa modalità
                                        // se la richiesta è assegnata ad un ufficio di produzione allora non deve essere mostrato l'utente approvatore
                                        // ma deve essere mostrato il nome dell'ufficio al quale è stata assegnata la richiesta
                                        if ( !String.IsNullOrEmpty( i.richiesta.ApprovatoreSelezionato ) &&
                                            i.richiesta.ApprovatoreSelezionato.StartsWith( "UFF" ) )
                                        {
                                            // recupera la descrizione dell'approvatore
                                            var d = db.MyRai_ApprovatoriProduzioni.Where( w => w.MatricolaApprovatore.Equals( i.richiesta.ApprovatoreSelezionato ) && w.SedeGapp.Equals( i.richiesta.codice_sede_gapp ) ).FirstOrDefault( );

                                            if ( d != null )
                                            {
                                                descApprovatore = d.Nominativo;
                                            }
                                        }

                                        string tdNota = "";
                                        if ( !String.IsNullOrEmpty( notaRifiuto ) )
                                        {
                                            tdNota = notaRifiuto.Trim( );
                                        }

                                        corpoMail += "<tr><td>" + i.notifica.descrizione + "</td><td>" + data_appr_rifiuto + "</td>" + "<td>" + descApprovatore + "</td></tr>";
                                    }
                                }
						}
                            else
                            {
                                toRemove.Add( notifica );
                            }
						}
					}

                    CommonTasks.Log("Invio in corso per " + dest + " - Notifiche:" + item.notifiche.Count());

                    bool success = false;
                    if (!String.IsNullOrEmpty( corpoMail ) )
                    {
                        success = InviaSingolaMail( MailTemplate.Replace( "#NOTIFICHE" , corpoMail ) , MailSubject , dest );
                    }

					// se si lancia in produzione per test
					// va commentato
                    if (success)
                    {
                        CommonTasks.Log("Invio ok");
                        try
                        {
                            if (toRemove != null && toRemove.Any())
                            {
                                item.notifiche.RemoveAll( w => toRemove.Contains( w ) );
                            }

                            using (digiGappEntities myDb = new digiGappEntities())
                            {

                                List<int> ids = item.notifiche.Select(x => x.id).ToList();

                                var items = myDb.MyRai_Notifiche.Where(n => ids.Contains(n.id)).ToList();

                                if (items != null && items.Any())
                                {
                                    items.ForEach(x => x.data_inviata = DateTime.Now);
                                }
                                myDb.SaveChanges();
                                CommonTasks.Log("Salvataggio DB ok");
                            }
                        }
                        catch (Exception ex)
                        {
                            CommonTasks.Log("Errore salvataggio DB SendNotificheDipendente " + ex.ToString());
                        }
                    }
                    else
                        CommonTasks.Log("Invio fallito");
				}
			}
			catch ( Exception ex )
			{
                CommonTasks.Log(ex.ToString());
			}
		}

		public static Boolean InviaSingolaMail(string body, string subj, string dest)
        {
            string from = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailApprovazioneFrom)[0];

            GestoreMail invia = new GestoreMail( );           

            try
            {
                var response = invia.InvioMail( body , subj , dest , null , from, null, null );
                if (response != null && response.Esito)
                {
                    return true;
                }
                else if ( response != null && !response.Esito )
                {
                    throw new Exception( response.Errore );
                }
                else
                {
                    throw new Exception( "Errore non specificato" );
                }
            }
            catch (Exception ex)
            {
                Output.WriteLine( "Errore invio email da [InviaSingolaMail] a " + dest + " - " + ex.ToString( ) );
                Logger.Log("Errore invio email da [InviaSingolaMail] a " + dest + " - " + ex.ToString());
                CommonTasks.Log( "Errore invio email da [InviaSingolaMail] a " + dest + " - " + ex.ToString( ) );
                return false;
            }
        }
    }
}