using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiCommonTasks.Helpers;
using myRaiData;

namespace myRaiCommonTasks
{
	public class NotificheLivello2FirmaPDF
	{
		public NotificheLivello2FirmaPDF()
		{
		}

		public class Liv2Riepilogo
		{
			public string Matricola { get; set; }
			public string Sede { get; set; }
			public int PDF { get; set; }
		}

		public void Start ()
		{
            List<Attachment> SlackAttachments = new List<Attachment>();
            string SlackMainTitle = "Batch InvioMailReminderPDFDaFirmare";
            int mailInviate = 0;
            string riepilogo = "";

            string lineaReport = "";

			lineaReport += String.Format( "Avvio NotificheLivello2FirmaPDF data e ora: {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) );
            Output.WriteLine( String.Format( "Avvio NotificheLivello2FirmaPDF data e ora: {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) ) );
			try
			{
				lineaReport += String.Format( "Preparazione delle notifiche {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) );
                Output.WriteLine( String.Format( "Preparazione delle notifiche {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) ) );
				List<Liv2Riepilogo> toSend = PreparaNotifichePerFirma();

				int conteggio = toSend.Where( i => i.PDF > 0 ).Sum( i => i.PDF );

				lineaReport += String.Format( "Fine preparazione delle notifiche {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) );
                Output.WriteLine( String.Format( "Fine preparazione delle notifiche {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) ) );

				lineaReport += String.Format( "------------------------------------\r\n" );
				lineaReport += String.Format( "Ci sono {0} PDF da firmare. {1} \r\n", conteggio, DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) );
                Output.WriteLine( String.Format( "Ci sono {0} PDF da firmare. {1} \r\n" , conteggio , DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) ) );

				var yyy = toSend.Where( i => i.PDF > 0 ).ToList();

				if ( toSend != null && toSend.Any() )
				{
					lineaReport += String.Format( "Invio mail {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) );

					toSend.OrderBy( y => y.Sede ).ToList().ForEach( i =>
					{
						if ( i.PDF > 0 )
						{
                            mailInviate++;

                            InvioMail( i.Matricola, i.PDF );
							lineaReport += String.Format( "Inviata mail a {0} numero pdf da firmare {1} - {2} \r\n", i.Matricola, i.PDF, DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) );
                            riepilogo += String.Format("Inviata mail a {0}, numero pdf da firmare {1}\r\n", i.Matricola, i.PDF);
                            Output.WriteLine( String.Format( "Inviata mail a {0}, numero pdf da firmare {1}\r\n" , i.Matricola , i.PDF ) );
						}
					} );

					lineaReport += String.Format( "Termine invio mail {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) );
				}

				lineaReport += String.Format( "------------------------------------\r\n" );
                Attachment c = new Attachment() { Color = "good", Title = "Inviate " + mailInviate + " email\r\n" + riepilogo };
                SlackAttachments.Add(c);
                Output.WriteLine( "Fine NotificheLivello2FirmaPDF" );
                Logger.Log( "Fine NotificheLivello2FirmaPDF" );
                CommonTasks.PostMessageAdvanced(SlackMainTitle, SlackAttachments.ToArray());
            }
			catch ( Exception ex )
			{
				Logger.Log( ex.ToString() );
				lineaReport += String.Format( "------------------------------------\r\n" );
				lineaReport += String.Format( "Si è verificato un errore {0}: \r\n{1}", DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ), ex.Message );
                Output.WriteLine( String.Format( "Si è verificato un errore {0}: \r\n{1}" , DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) , ex.Message ) );
			}

			lineaReport += String.Format( "Termine InvioMailReminderPDFDaFirmare {0} \r\n", DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) );
            Output.WriteLine( String.Format( "Termine InvioMailReminderPDFDaFirmare {0} \r\n" , DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss" ) ) );

			string nomeReport = String.Format( "NotificheLivello2FirmaPDF_{0}.txt", DateTime.Now.ToString( "ddMMyyyy" ) );
			System.IO.File.WriteAllText( AppDomain.CurrentDomain.BaseDirectory + nomeReport, lineaReport );
		}

		#region private

		private List<Liv2Riepilogo> PreparaNotifichePerFirma ()
		{
			List<Liv2Riepilogo> result = new List<Liv2Riepilogo>();
			List<Liv2Riepilogo> riepilogo = new List<Liv2Riepilogo>();

			try
			{
				Abilitazioni abilitazioni = CommonTasks.getAbilitazioni();

				if ( abilitazioni != null &&
					abilitazioni.ListaAbilitazioni != null &&
					abilitazioni.ListaAbilitazioni.Any() )
				{

					foreach ( var ab in abilitazioni.ListaAbilitazioni.Where( a => a.MatrLivello2 != null && a.MatrLivello2.Any() ).ToList() )
					{
						foreach ( var matr in ab.MatrLivello2 )
						{
							result.Add( new Liv2Riepilogo()
							{
								Matricola = matr.Matricola,
								Sede = ab.Sede,
								PDF = GetPDFCount( ab.Sede )
							} );
						}
					}

					if ( result != null && result.Any() )
					{
						result.ForEach( i =>
						{
							if ( riepilogo.Where( m => m.Matricola.Equals( i.Matricola ) ).Count() > 0 )
							{
								var x = riepilogo.Where( m => m.Matricola.Equals( i.Matricola ) ).FirstOrDefault();
								x.PDF += i.PDF;
							}
							else
							{
								riepilogo.Add( new Liv2Riepilogo()
								{
									Matricola = i.Matricola,
									Sede = i.Sede,
									PDF = i.PDF
								} );
							}
						} );
					}
				}
			}
			catch ( Exception ex )
			{

			}

			return riepilogo;
		}

		private int GetPDFCount ( string sede )
		{
			int result = 0;
			try
			{
				DateTime data = DateTime.Now;
                DateTime start = DateTime.Now;

                if (data.Month == 1)
                {
                    DateTime dtTemp = new DateTime(data.Year, data.Month, 1);
                    dtTemp = dtTemp.AddDays(-1);

                    start = new DateTime(dtTemp.Year, dtTemp.Month, 1, 0, 0, 0);
                }
                else
                {
                    start = new DateTime(data.Year, data.Month - 1, 1, 0, 0, 0);
                }
                
                //start = new DateTime( 2020 , 1 , 27 );
                //DateTime stop = new DateTime( 2020 , 1 , 31 );
				DateTime stop = new DateTime( data.Year, data.Month, 1, 23, 59, 59 );

				stop = stop.AddDays( -1 );

				using ( digiGappEntities db = new digiGappEntities() )
				{
					result = db.DIGIRESP_Archivio_PDF.Count( p => !p.data_convalida.HasValue &&
											p.numero_versione > 0 &&
											p.sede_gapp.Equals( sede ) &&
											p.data_inizio >= start &&
											p.data_inizio <= stop &&
											p.tipologia_pdf == "R" );
				}
			}
			catch ( Exception ex )
			{
			}

			return result;
		}

		private void InvioMail ( string destinatario, int numeroPDF )
		{
			try
			{
				string[] MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>( myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateLiv2SollecitoFirma );

				string body = MailParams[0];
				string MailSubject = MailParams[1];
				string dest = myRaiCommonTasks.CommonTasks.GetEmailPerMatricola( destinatario.TrimStart( 'P' ) );

				if ( String.IsNullOrWhiteSpace( dest ) )
				{
					dest = destinatario + "@rai.it";
				}

				GestoreMail mail = new GestoreMail();

				var response = mail.InvioMail( body.Replace( "#quantita", numeroPDF.ToString() ), MailSubject, dest, "raiplace.selfservice@rai.it", myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailApprovazioneFrom );

				if ( response != null && response.Errore != null )
				{
					myRaiData.MyRai_LogErrori err = new MyRai_LogErrori()
					{
						applicativo = "Batch",
						data = DateTime.Now,
						provenienza = "NotificheLivello2FirmaPDF - InvioMail",
						error_message = response.Errore + " per " + dest
					};

					using ( digiGappEntities db = new digiGappEntities() )
					{
						db.MyRai_LogErrori.Add( err );
						db.SaveChanges();
					}
				}
			}
			catch ( Exception ex )
			{
				Logger.Log( "NotificheLivello2FirmaPDF - InvioMail " + ex.ToString() );
			}

		}

		#endregion


	}
}