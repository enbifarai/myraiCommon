using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiCommonTasks.sendMail;
using myRaiData;

namespace myRaiCommonTasks
{
	public class NotificheScaduteDestinateApprovatori
    {
		public NotificheScaduteDestinateApprovatori ()
		{
		}

		public void SendNotificheESS_LIV1 ( string targetMatricola = null )
		{
			CommonTasks.Log( "\r\n" );
			CommonTasks.Log( "-------------------------------" );
			CommonTasks.Log( String.Format( "Avvio batch di invio notifiche di richieste scadute {0}\r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
			List<NotificheMatricola> lNotifiche = new List<NotificheMatricola>();
			 
			try
			{
				DateTime dataLimite = DateTime.Now;

				string oreMinutiRiferimento = dataLimite.Hour.ToString().PadLeft(2, '0')+ dataLimite.Minute.ToString().PadLeft(2, '0');

				lNotifiche = GetNotificheESS_LIV1_Scadute( dataLimite, oreMinutiRiferimento, targetMatricola );
				CommonTasks.Log( String.Format( "Trovate {0} notifiche scadute - {1}\r\n", lNotifiche.Count(), DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

				if ( lNotifiche != null )
				{
					InviaMailNotificaScadute( lNotifiche );
				}				
			}
			catch ( Exception ex )
			{
				CommonTasks.Log( String.Format( "Si è verificato un errore:\r\n{0}\r\n{1}\r\n", ex.ToString(), DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
			}
			CommonTasks.Log( String.Format( "Termine batch di invio notifiche di richieste scadute {0}\r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
		}

		public static List<NotificheMatricola> GetNotificheESS_LIV1_Scadute ( DateTime dataLimite, string oreMinutiRiferimento, string targetMatricola = null )
		{
			CommonTasks.Log( String.Format( "Reperimento delle notifiche scadute - {0}\r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
			List<NotificheMatricola> notifiche = new List<NotificheMatricola>();

			try
			{
				string tipoNotifica = EnumCategoriaEccezione.MarcaturaScaduta.ToString();
				string tipoDestinatario = EnumTipoDestinatarioNotifica.ESS_LIV1.ToString();

				using ( digiGappEntities db = new digiGappEntities() )
				{
					if ( String.IsNullOrEmpty( targetMatricola ) )
					{
						var ListNotifichePerMatricola = db.MyRai_Notifiche.Where( x => 
							x.data_inviata == null && 
							x.data_letta == null && 
							x.categoria == tipoNotifica && 
							x.data_inserita < dataLimite &&
							x.descrizione.ToUpper().StartsWith( "NUOVA MARCATURA SCADUTA" ) &&
							x.categoria.Equals( tipoNotifica ) ).GroupBy(
																		 p => p.matricola_destinatario,
																		 p => p,
																		( key, g ) => new
																		{
																			matricola = key,
																			notifiche = g.OrderBy( x =>
																						   x.data_inserita )
																		} );

						foreach ( var item in ListNotifichePerMatricola )
						{
							if ( IsToSendNow( item.matricola, tipoDestinatario.ToString(), "SCA", oreMinutiRiferimento ) )
							{
								notifiche.Add( new NotificheMatricola() { matricola = item.matricola, notifiche = item.notifiche.ToList() } );
							}
						}
					}
					else
					{
						var ListNotifichePerMatricola = db.MyRai_Notifiche.Where( x => 
							x.data_inviata == null && 
							x.data_letta == null && 
							x.categoria == tipoNotifica && 
							x.data_inserita < dataLimite &&
							x.descrizione.ToUpper().StartsWith( "NUOVA MARCATURA SCADUTA" ) &&
							x.categoria.Equals( tipoNotifica ) &&
							x.matricola_destinatario.Equals( targetMatricola ) ).GroupBy(
																		 p => p.matricola_destinatario,
																		 p => p,
																		( key, g ) => new
																		{
																			matricola = key,
																			notifiche = g.OrderBy( x =>
																						   x.data_inserita )
																		} );

						foreach ( var item in ListNotifichePerMatricola )
						{
							if ( IsToSendNow( item.matricola, tipoDestinatario.ToString(), "SCA", oreMinutiRiferimento ) )
							{
								notifiche.Add( new NotificheMatricola() { matricola = item.matricola, notifiche = item.notifiche.ToList() } );
							}
						}
					}
				}
			}
			catch ( Exception ex )
			{
				CommonTasks.Log( String.Format( "Errore nel reperimento delle notifiche scadute\r\n{0}\r\n{1}\r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ), ex.ToString() ) );
				notifiche = null;
			}

			return notifiche;
		}

		public static Boolean IsToSendNow ( string matricola, string tipodestinatario, string tipoevento, string oreMinutiRiferimento )
		{
			oreMinutiRiferimento = "00";
			using ( digiGappEntities db = new digiGappEntities() )
			{

				var ImpostazioneNotifica = GetImpostazioneNotifiche( matricola, tipodestinatario, tipoevento );

				if ( ImpostazioneNotifica == null ) return false;

				if ( ImpostazioneNotifica.TipoInvio == "N" )
					return false;

				if ( ImpostazioneNotifica.TipoInvio == "I" ) return true;


				if ( ImpostazioneNotifica.TipoInvio == "G" )
					return ( Convert.ToInt32( DateTime.Now.ToString( "HHmm" ) ) >= Convert.ToInt32( ImpostazioneNotifica.OraMinuti == null ? "0000" : ImpostazioneNotifica.OraMinuti ) );
				else
				{
					if ( ImpostazioneNotifica.GiornoDellaSettimana == null ) ImpostazioneNotifica.GiornoDellaSettimana = 1;
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

		public static void InviaMailNotificaScadute ( List<NotificheMatricola> lNotifiche )
		{
			if ( lNotifiche.Count == 0 )
			{
				CommonTasks.Log( String.Format( "Nessuna notifica da inviare - {0}\r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
				return;
			}
			var NotificheDaInviarePerMatr = lNotifiche.GroupBy(
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

			foreach ( var item in NotificheDaInviarePerMatr )
			{
				string destinatario = "";
				int totScadute = 0;

				destinatario = item.notifiche.First().email_destinatario;
				totScadute = item.notifiche.Count();

				MailTemplate = MailParams[0];
				MailTemplate = MailTemplate.Replace( "#SCA", totScadute.ToString() );
				CommonTasks.Log( String.Format( "Invio mail a {0} - {1}\r\n ", destinatario, DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
				bool success = Notifiche.InviaSingolaMail( MailTemplate, MailSubject, destinatario );

				if ( success )
				{
					CommonTasks.Log( String.Format( "Mail inviata con successo - {0}\r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
					using ( digiGappEntities db = new digiGappEntities() )
					{
						item.notifiche.ForEach( w =>
						{
							var toUpdate = db.MyRai_Notifiche.Where( xx => xx.id_riferimento.Equals( w.id_riferimento ) &&
                                                                    xx.descrizione.ToUpper().StartsWith("NUOVA MARCATURA SCADUTA")).ToList();
							if ( toUpdate != null && toUpdate.Any() )
							{
								toUpdate.ForEach( t =>
								{
									t.data_inviata = DateTime.Now;
								} );
							}
						} );

						try
						{
							db.SaveChanges();
						}
						catch ( Exception ex )
						{
							CommonTasks.Log( String.Format( "Errore nel salvataggio dei dati sul db\r\n{0}\r\n{1}\r\n", ex.ToString(), DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
						}

					}
				}
				else
				{
					CommonTasks.Log( String.Format( "Si è verificato un errore durante l'invio della mail - {0}\r\n", DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
				}
			}
		}

		public static Boolean InviaSingolaMail(string body, string subj, string dest)
        {
            MailSender invia = new MailSender();

            string from = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailApprovazioneFrom)[0];
            string[] AccountUtenteServizio = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.AccountUtenteServizio);
            invia.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");

            Email eml = new Email();

            eml.From = from;
            eml.toList = new string[] { dest };
            eml.ContentType = "text/html";
            eml.Priority = 2;
            eml.SendWhen = DateTime.Now.AddSeconds(1);
            eml.Subject = subj;
            eml.Body = body;
            try
            {
                invia.Send(eml);
                return true;
            }
            catch (Exception ex)
            {
				CommonTasks.Log( String.Format( "Si è verificato un errore durante l'invio della mail all'utente {0}\r\n{1}\r\n{2}",dest, ex.ToString(), DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
                return false;
            }
        }



        #region Approvatori Produzione

        public void SendNotificheScaduteApprovatoriProduzione ( string targetMatricola = null )
        {
            CommonTasks.Log( "\r\n" );
            CommonTasks.Log( "-------------------------------" );
            CommonTasks.Log( String.Format( "Avvio batch di invio notifiche di richieste scadute approvatore produzione {0}\r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
            List<NotificheMatricola> lNotifiche = new List<NotificheMatricola>( );

            try
            {
                DateTime dataLimite = DateTime.Now;

                string oreMinutiRiferimento = dataLimite.Hour.ToString( ).PadLeft( 2 , '0' ) + dataLimite.Minute.ToString( ).PadLeft( 2 , '0' );

                lNotifiche = GetNotificheScaduteApprovatoriProduzione( dataLimite , oreMinutiRiferimento , targetMatricola );
                CommonTasks.Log( String.Format( "Trovate {0} notifiche scadute - {1}\r\n" , lNotifiche.Count( ) , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );

                if ( lNotifiche != null )
                {
                    InviaMailNotificaScadute( lNotifiche );
                }
            }
            catch ( Exception ex )
            {
                CommonTasks.Log( String.Format( "Si è verificato un errore:\r\n{0}\r\n{1}\r\n" , ex.ToString( ) , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
            }
            CommonTasks.Log( String.Format( "Termine batch di invio notifiche di richieste scadute approvatore produzione {0}\r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
        }


        public static List<NotificheMatricola> GetNotificheScaduteApprovatoriProduzione ( DateTime dataLimite , string oreMinutiRiferimento , string targetMatricola = null )
        {
            CommonTasks.Log( String.Format( "Reperimento delle notifiche scadute - {0}\r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) ) );
            List<NotificheMatricola> notifiche = new List<NotificheMatricola>( );

            try
            {
                string tipoNotifica = EnumCategoriaEccezione.MarcaturaScaduta.ToString( );
                string tipoDestinatario = EnumTipoDestinatarioNotifica.ESS_LIV1.ToString( );

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    if ( String.IsNullOrEmpty( targetMatricola ) )
                    {
                        List<string> approvatoriProduzione = new List<string>( );
                        try
                        {
                            approvatoriProduzione = db.MyRai_ApprovatoreProduzione.Select( w => w.MatricolaApprovatore ).Distinct( ).ToList( );
                        }
                        catch(Exception ex)
                        {
                            approvatoriProduzione = new List<string>( );
                        }
                        var ListNotifichePerMatricola = db.MyRai_Notifiche.Where( x =>
                            x.data_inviata == null &&
                            x.data_letta == null &&
                            x.categoria == tipoNotifica &&
                            x.data_inserita < dataLimite &&
                            x.descrizione.ToUpper( ).StartsWith( "NUOVA MARCATURA SCADUTA" ) &&
                            ( approvatoriProduzione.Count > 0 ? approvatoriProduzione.Contains( x.matricola_destinatario ) : true ) &&
                            x.categoria.Equals( tipoNotifica ) ).GroupBy(
                                                                         p => p.matricola_destinatario ,
                                                                         p => p ,
                                                                        ( key , g ) => new
                                                                        {
                                                                            matricola = key ,
                                                                            notifiche = g.OrderBy( x =>
                                                                                           x.data_inserita )
                                                                        } );

                        foreach ( var item in ListNotifichePerMatricola )
                        {
                            if ( IsToSendNow( item.matricola , tipoDestinatario.ToString( ) , "SCA" , oreMinutiRiferimento ) )
                            {
                                notifiche.Add( new NotificheMatricola( ) { matricola = item.matricola , notifiche = item.notifiche.ToList( ) } );
                            }
                        }
                    }
                    else
                    {
                        var ListNotifichePerMatricola = db.MyRai_Notifiche.Where( x =>
                            x.data_inviata == null &&
                            x.data_letta == null &&
                            x.categoria == tipoNotifica &&
                            x.data_inserita < dataLimite &&
                            x.descrizione.ToUpper( ).StartsWith( "NUOVA MARCATURA SCADUTA" ) &&
                            x.categoria.Equals( tipoNotifica ) &&
                            x.matricola_destinatario.Equals( targetMatricola ) ).GroupBy(
                                                                         p => p.matricola_destinatario ,
                                                                         p => p ,
                                                                        ( key , g ) => new
                                                                        {
                                                                            matricola = key ,
                                                                            notifiche = g.OrderBy( x =>
                                                                                           x.data_inserita )
                                                                        } );

                        foreach ( var item in ListNotifichePerMatricola )
                        {
                            if ( IsToSendNow( item.matricola , tipoDestinatario.ToString( ) , "SCA" , oreMinutiRiferimento ) )
                            {
                                notifiche.Add( new NotificheMatricola( ) { matricola = item.matricola , notifiche = item.notifiche.ToList( ) } );
                            }
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                CommonTasks.Log( String.Format( "Errore nel reperimento delle notifiche scadute\r\n{0}\r\n{1}\r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) , ex.ToString( ) ) );
                notifiche = null;
            }

            return notifiche;
        }




        #endregion
    }
}