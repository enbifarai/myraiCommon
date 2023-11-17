using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using myRai.Data.CurriculumVitae;
using myRai.DataControllers;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using myRaiServiceHub.it.rai.servizi.sendmail;

namespace myRai.Controllers
{
	public class JobPostingController : BaseCommonController
    {
		private FileDataController _datacontroller = new FileDataController();

		/// <summary>
		/// Restituisce la view di index del jobposting
		/// </summary>
		/// <returns></returns>
		public PartialViewResult Index ( string id, string numerojp )
        {
			// reperimento della matricola utente
			//string matricola = myRai.Models.Utente.EsponiAnagrafica()._matricola;
			// modificato per consentire la candidatura di tutte le matricole non solo
			// quelle che sono della rai es. RAI\Pxxxxxx ma anche PRA\xxxxxx
			string matricola = CommonHelper.GetCurrentUsername();

			string viewResult = string.Empty;
			
			int idJob = int.Parse( id );
			bool alreadyPrenset = this.AlreadyPresent( idJob );

			tblJobPosting job = this.GetJob( idJob );

			JobPostingModel model = new JobPostingModel();

			model.IdJob = id;
			model.Matricola = matricola;
			model.Produzione = job.produzione;
			model.NumeroJP = numerojp;
			// 20160614_101350

			DateTime? dataScadenza = null;

			try
			{
				if ( !String.IsNullOrEmpty( job.scadenza ) && job.scadenza.Length == 15 )
				{
					int YY = int.Parse( job.scadenza.Substring( 0, 4 ) );
					int MM = int.Parse( job.scadenza.Substring( 4, 2 ) );
					int DD = int.Parse( job.scadenza.Substring( 6, 2 ) );
					int HH = int.Parse( job.scadenza.Substring( 9, 2 ) );
					int mm = int.Parse( job.scadenza.Substring( 11, 2 ) );
					int SS = int.Parse( job.scadenza.Substring( 13, 2 ) );

					dataScadenza = new DateTime( YY, MM, DD, HH, mm, SS );
				}
			}
			catch
			{
				dataScadenza = null;
			}

			bool closed = false;

			if ( dataScadenza != null )
			{
				dataScadenza = dataScadenza.Value.AddMilliseconds( -dataScadenza.Value.Millisecond );
				DateTime currentDate = DateTime.Now;
				currentDate = currentDate.AddMilliseconds( -currentDate.Millisecond );

				closed = ( DateTime.Compare( dataScadenza.Value, currentDate ) < 0 );
			}

			if ( !job.attivo.Equals( "SI", StringComparison.InvariantCultureIgnoreCase ) || closed )
			{
				viewResult = "~/Views/JobPosting/JobPostClosed.cshtml";
			}
			else if ( !alreadyPrenset )
			{
				// se è la prima volta che l'utente si candida per questo job

				// verifica se la categoria dell'utente può accreditarsi per questo job
				bool categoriaConsentita = this.CategoriaConsentita( job.categorie );

				if ( categoriaConsentita )
				{
					// se l'utente si può candidare va controllato il tipo di candidatura
					// possibile (tramite cvonline oppure upload di file)
					bool cvRequired = true;

					if ( cvRequired )
					{
						bool cvOnline = false;
						bool skipCvOnline = false;

						List<string> eccezioni = this.GetJobPostingEccezioni();

						if ( eccezioni != null )
						{
							eccezioni.ForEach( e =>
							{
								if ( matricola.StartsWith( e, StringComparison.InvariantCultureIgnoreCase ) )
								{
									skipCvOnline = true;
								}

								string nomeCompleto = CommonHelper.GetCurrentUsername();

								if ( !nomeCompleto.StartsWith( "RAI\\", StringComparison.InvariantCultureIgnoreCase ) )
								{
									skipCvOnline = true;
								}
							} );
						}

						if ( !skipCvOnline &&
							!String.IsNullOrEmpty( job.cvonline ) && job.cvonline.Equals( "si", StringComparison.InvariantCultureIgnoreCase ) )
						{
							cvOnline = true;
						}

						if ( cvOnline )
						{
							viewResult = "~/Views/JobPosting/UploadFromCVOnline.cshtml";
						}
						else
						{
							viewResult = "~/Views/JobPosting/UploadCV.cshtml";
						}
					}
					else
					{
						viewResult = "~/Views/JobPosting/Index.cshtml";
					}
				}
				else
				{
					viewResult = "~/Views/JobPosting/NoAuth.cshtml";
				}
			}
			else
			{
				// se si è già candidato per questo lavoro
				viewResult = "~/Views/JobPosting/GiaCandidato.cshtml";
			}

			return PartialView( viewResult , model );
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="idJob"></param>
		/// <param name="_fileUpload"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult UploadAttached ( string idJob, string numerojp )
		{
			string result = string.Empty;
			try
			{
				if ( this.Request.Files.Count == 0 ||
					String.IsNullOrEmpty( this.Request.Files[0].FileName ) )
				{
					throw new Exception( "Si è verificato un errore.\nFile mancante" );
				}

				if ( String.IsNullOrEmpty( idJob ) )
				{
					throw new Exception( "Si è verificato un errore.\nManca l'identificativo del job per il quale si intende candidarsi" );
				}

				string matricola = CommonHelper.GetCurrentUsername();
				string pMatricola = "";
				string ext = Path.GetExtension( this.Request.Files[0].FileName );
				string nomeFile = "";

				if ( matricola.StartsWith( "RAI\\", StringComparison.InvariantCultureIgnoreCase ) )
				{
					matricola = matricola.Replace( "RAI\\", "" );
					pMatricola = matricola;
					if ( !( matricola.StartsWith( "P", StringComparison.InvariantCultureIgnoreCase ) ) )
					{
						pMatricola = "P" + matricola;
					}
				}
				else
				{
					pMatricola = matricola.Replace( "\\", "_" );
				}

				nomeFile = String.Format( "{0}_{1}{2}", idJob, pMatricola, ext );
				Files toSave = new Files();

				toSave.Id = -1;
				toSave.Matricola = matricola;
				toSave.Name = nomeFile;
				toSave.CreationDate = DateTime.Now;
				toSave.Size = this.Request.Files[0].ContentLength;
				toSave.MimeType = this.Request.Files[0].ContentType;

				byte[] buffer = new byte[this.Request.Files[0].ContentLength];
				using ( MemoryStream ms = new MemoryStream() )
				{
					int read;
					while ( ( read = this.Request.Files[0].InputStream.Read( buffer, 0, buffer.Length ) ) > 0 )
					{
						ms.Write( buffer, 0, read );
					}
					toSave.ContentType = ms.ToArray();
				}

				int iIdJob = int.Parse( idJob );

				// salvataggio dell'associativa tra file e candidatura
				this.SalvaJobPostingFiles( iIdJob, toSave );

				if ( toSave.Id > 0 )
				{
					// aggiornamento dell'archivio con le candidature
					result = this.SalvaCandidatura( iIdJob, nomeFile, numerojp, toSave.Id );
				}
				else
				{
					throw new Exception( "Si è verificato un errore durante il salvataggio del file" );
				}

				return Json( new
				{
					success = true,
					errorMessage = result
				} );
			}
			catch ( Exception ex )
			{
				return Json( new
				{
					success = false,
					errorMessage = ex.Message
				} );
			}
		}

		public ActionResult CreateCVPdf ( string idJob, string numerojp )
		{
			try
			{
				string matricola = CommonHelper.GetCurrentUsername();

				if ( matricola.StartsWith( "RAI\\", StringComparison.InvariantCultureIgnoreCase ) )
				{
					matricola = matricola.Replace( "RAI\\", "" );

					if ( !( matricola.StartsWith( "P", StringComparison.InvariantCultureIgnoreCase ) ) )
					{
						matricola = "P" + matricola;
					}
				}
				else
				{
					matricola = matricola.Replace( "\\", "_" );
				}
				
				string basePath = this.GetStorePath();

				string nomeFile = String.Format( "{0}_{1}{2}", idJob, matricola, ".pdf" );
				MemoryStream ms = new MemoryStream();

				FileStreamResult fileStreamResult = new CV_OnlineController( Server.MapPath( "~" ), Server.MapPath( "~/assets/fontG/open-sans-v13-latin-300.ttf" ) ).Pdf( null, "Data ultimo aggiornamento: \n" ) as FileStreamResult;
				fileStreamResult.FileStream.Seek( 0, SeekOrigin.Begin );
				fileStreamResult.FileStream.CopyTo( ms );
				return new FileContentResult( ms.ToArray(), "application/pdf" );
			}
			catch ( Exception ex )
			{
				return null;
			}
		}

		/// <summary>
		/// Invia la propria candidatura in base al pdf generato a partire dal curriculum
		/// </summary>
		/// <param name="idJob"></param>
		/// <returns></returns>
		public JsonResult SendMyApplication ( string idJob, string numerojp )
		{
			string result = string.Empty;
			try
			{

				if ( String.IsNullOrEmpty( idJob ) )
				{
					throw new Exception( "Si è verificato un errore.\nManca l'identificativo del job per il quale si intende candidarsi" );
				}

				string matricola = CommonHelper.GetCurrentUsername();
				string pMatricola = "";
				string nomeFile = "";

				if ( matricola.StartsWith( "RAI\\", StringComparison.InvariantCultureIgnoreCase ) )
				{
					matricola = matricola.Replace( "RAI\\", "" );
					pMatricola = matricola;
					if ( !( matricola.StartsWith( "P", StringComparison.InvariantCultureIgnoreCase ) ) )
					{
						pMatricola = "P" + matricola;
					}
				}
				else
				{
					pMatricola = matricola.Replace( "\\", "_" );
				}

				nomeFile = String.Format( "{0}_{1}{2}", idJob, pMatricola, ".pdf" );

				Files toSave = new Data.CurriculumVitae.Files();
				toSave.Id = -1;
				toSave.Matricola = matricola;
				toSave.Name = nomeFile;
				toSave.CreationDate = DateTime.Now;

				using ( MemoryStream ms = new MemoryStream() )
				{
					var fileStreamResult = new CV_OnlineController( Server.MapPath( "~" ), Server.MapPath( "~/assets/fontG/open-sans-v13-latin-300.ttf" ) ).Pdf() as FileStreamResult;
					fileStreamResult.FileStream.Seek( 0, SeekOrigin.Begin );

					byte[] buffer = new byte[fileStreamResult.FileStream.Length];

					int read;
					while ( ( read = fileStreamResult.FileStream.Read( buffer, 0, buffer.Length ) ) > 0 )
					{
						ms.Write( buffer, 0, read );
					}
					toSave.ContentType = ms.ToArray();
					toSave.Size = ms.Length;
					toSave.MimeType = "Application/pdf";
				}

				int iIdJob = int.Parse( idJob );

				// salvataggio dell'associativa tra file e candidatura
				this.SalvaJobPostingFiles( iIdJob, toSave );

				// aggiornamento dell'archivio con le candidature
				result = this.SalvaCandidatura( iIdJob, nomeFile, numerojp, toSave.Id );	

				return Json( new
				{
					success = true,
					errorMessage = result
				} );
			}
			catch ( Exception ex )
			{
				return Json( new
				{
					success = false,
					errorMessage = ex.Message
				} );
			}
		}

		public JsonResult SendMyApplicationNoCV ( string idJob, string numerojp )
		{
			string result = string.Empty;
			try
			{
				if ( String.IsNullOrEmpty( idJob ) )
				{
					throw new Exception( "Si è verificato un errore.\nManca l'identificativo del job per il quale si intende candidarsi" );
				}

				int iIdJob = int.Parse( idJob );

				result = this.SalvaCandidatura( iIdJob, null, numerojp );

				return Json( new
				{
					success = true,
					errorMessage = result
				} );
			}
			catch ( Exception ex )
			{
				return Json( new
				{
					success = false,
					errorMessage = ex.Message
				} );
			}
		}

		public PartialViewResult InviaCandidatura ( string id, string numerojp )
		{
			// reperimento della matricola utente
			string matricola = CommonHelper.GetCurrentUsername();

			if ( matricola.StartsWith( "RAI\\", StringComparison.InvariantCultureIgnoreCase ) )
			{
				matricola = matricola.Replace( "RAI\\", "" );

				if ( !( matricola.StartsWith( "P", StringComparison.InvariantCultureIgnoreCase ) ) )
				{
					matricola = "P" + matricola;
				}
			}
			else
			{
				matricola = matricola.Replace( "\\", "_" );
			}

			int idJob = int.Parse( id );

			tblJobPosting job = this.GetJob( idJob );
			JobPostingModel model = new JobPostingModel();

			model.IdJob = id;
			model.Matricola = matricola;
			model.Produzione = job.produzione;
			model.NumeroJP = numerojp;

			return PartialView( "~/Views/JobPosting/_InviaCandidatura.cshtml", model );
		}

		public PartialViewResult CVDettaglio ( string id, string numerojp )
		{
			// reperimento della matricola utente
			string matricola = CommonHelper.GetCurrentUsername();

			if ( matricola.StartsWith( "RAI\\", StringComparison.InvariantCultureIgnoreCase ) )
			{
				matricola = matricola.Replace( "RAI\\", "" );

				if ( !( matricola.StartsWith( "P", StringComparison.InvariantCultureIgnoreCase ) ) )
				{
					matricola = "P" + matricola;
				}
			}
			else
			{
				matricola = matricola.Replace( "\\", "_" );
			}

			int idJob = int.Parse( id );

			tblJobPosting job = this.GetJob( idJob );
			JobPostingModel model = new JobPostingModel();

			model.IdJob = id;
			model.Matricola = matricola;
			model.Produzione = job.produzione;
			model.NumeroJP = numerojp;

			return PartialView( "~/Views/JobPosting/CVDettaglio.cshtml", model );
		}

		public PartialViewResult UploadCVOnlineNoData ( string id, string numerojp )
		{
			// reperimento della matricola utente
			string matricola = CommonHelper.GetCurrentUsername();

			if ( matricola.StartsWith( "RAI\\", StringComparison.InvariantCultureIgnoreCase ) )
			{
				matricola = matricola.Replace( "RAI\\", "" );

				if ( !( matricola.StartsWith( "P", StringComparison.InvariantCultureIgnoreCase ) ) )
				{
					matricola = "P" + matricola;
				}
			}
			else
			{
				matricola = matricola.Replace( "\\", "_" );
			}

			int idJob = int.Parse( id );

			tblJobPosting job = this.GetJob( idJob );
			JobPostingModel model = new JobPostingModel();

			model.IdJob = id;
			model.Matricola = matricola;
			model.Produzione = job.produzione;
			model.NumeroJP = numerojp;

			return PartialView( "~/Views/JobPosting/_UploadCVOnlineNoData.cshtml", model );
		}

		/// <summary>
		/// Verifica se l'utente corrente si è già
		/// candidato per un particolare job, identificato
		/// con l'id idJob
		/// </summary>
		/// <param name="idJob">Identificativo univoco del job per il quale si intende candidarsi</param>
		/// <returns></returns>
		private bool AlreadyPresent ( int idJob )
		{
			bool already = false;
			try
			{
				string matricola = CommonHelper.GetCurrentUsername();

				//string matricola = Utente.EsponiAnagrafica()._matricola;

				if ( matricola.StartsWith( "RAI\\", StringComparison.InvariantCultureIgnoreCase ) )
				{
					matricola = matricola.Replace( "RAI\\", "" );

					if ( !( matricola.StartsWith( "P", StringComparison.InvariantCultureIgnoreCase ) ) )
					{
						matricola = "P" + matricola;
					}
				}

				using (var db = new CorsiJobEntities())
				{
					var jobPost = db.tblRichiesteJobpostingRegisti.Where( j => j.matricola.Equals( matricola, StringComparison.InvariantCultureIgnoreCase ) && j.ID_jobposting != null && j.ID_jobposting.Value == idJob ).FirstOrDefault();

					if ( jobPost != null )
					{
						already = true;
					}
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
			return already;
		}

		/// <summary>
		/// Verifica se la categoria dell'utente corrente è tra la lista delle
		/// categorie che possono candidarsi ad un particolare job
		/// </summary>
		/// <param name="categorie"></param>
		/// <returns></returns>
		private bool CategoriaConsentita ( string categorie )
		{
			string cat = UtenteHelper.EsponiAnagrafica()._categoria;
			bool consentita = false;
			try
			{
				if ( !string.IsNullOrEmpty( categorie ) )
				{
					List<string> categories = categorie.Split( ';' ).ToList();

					foreach ( var c in categories )
					{
						if ( c.Equals( "*" ) || c.Equals( cat, StringComparison.InvariantCultureIgnoreCase ) )
						{
							consentita = true;
							break;
						}
					}
				}
				else
				{
					consentita = true;
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}

			return consentita;
		}

		/// <summary>
		/// Salvataggio dei dati sul db e sul path 
		/// </summary>
		/// <param name="idJob"></param>
		/// <param name="nomeCV"></param>
		private string SalvaCandidatura ( int idJob, string nomeCV = null, string numerojp = null, int? idFile = null )
		{
			string result = string.Empty;
			try
			{
				if ( this.AlreadyPresent( idJob ) )
				{
					throw new Exception( "Candidatura già inviata per questo job" );
				}

                UtenteHelper.Anagrafica utente = UtenteHelper.EsponiAnagrafica();

				string matricola = CommonHelper.GetCurrentUsername();

				if ( matricola.StartsWith( "RAI\\", StringComparison.InvariantCultureIgnoreCase ) )
				{
					matricola = matricola.Replace( "RAI\\", "" );

					if ( !( matricola.StartsWith( "P", StringComparison.InvariantCultureIgnoreCase ) ) )
					{
						matricola = "P" + matricola;
					}
				}

				DateTime ora = DateTime.Now;

				using ( var db = new CorsiJobEntities() )
				{
					string data = string.Empty;

					data = String.Format( "{0:00}{1:00}{2:00}_{3:00}{4:00}{5:00}", ora.Year, ora.Month, ora.Day, ora.Hour, ora.Minute, ora.Second );

					it.rai.servizi.hrgb.Service wsAnag = new it.rai.servizi.hrgb.Service();
					var elenco = wsAnag.Get_RicercaAnagrafica_Net( "COMINT", matricola, "", "", "", "false" );

					if ( idFile != null )
					{
						Uri myuri = new Uri(Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, String.Empty));

						string host = myuri.Host.ToString();

						if ( host.Contains( "localhost" ) )
						{
							nomeCV = String.Format( "http://svilraiperme.intranet.rai.it/AdminJobPosting/Download?idFile={0}", idFile.GetValueOrDefault() );
						}
						else
						{
							nomeCV = String.Format( "http://{0}/AdminJobPosting/Download?idFile={1}", host, idFile.GetValueOrDefault() );
						}
					}

					tblRichiesteJobpostingRegisti richiesta = new tblRichiesteJobpostingRegisti()
					{
						curriculum = nomeCV,
						data_inserimento = data,
						dataincontro = null,
						direzione = null,
						ID_jobposting = idJob,
						livello = null,
						mail = utente._email,
						matricola = matricola,
						nominativo = String.Format("{0} {1}", utente._cognome, utente._nome),
						notazioni = null,
						numerojp = numerojp,
						profilo = utente._qualifica,
						stato = "Inviato"
					};

					if ( elenco.DT_RicercaAnagrafica.Rows.Count > 0 )
					{
						var itm = elenco.DT_RicercaAnagrafica.Rows[0];
						richiesta.livello = itm[4].ToString();
						richiesta.direzione = itm[11].ToString();
					}

					db.tblRichiesteJobpostingRegisti.Add( richiesta );
					db.SaveChanges();					
				}

				tblJobPosting job = this.GetJob( idJob );

				string profilo = "";

				if ( !String.IsNullOrEmpty( numerojp ) )
				{
					int n;
					bool isNumeric = int.TryParse( numerojp, out n );

					if ( !isNumeric )
					{
						profilo = string.Format( "profilo {0}", numerojp.ToUpper() );
					}
				}

				string desc = string.Format( "{0} {1} {2}", job.produzione, job.professionalita, profilo );

				desc = desc.Trim();

				string utenteDestinatario = string.Format( "{0} {1}", utente._nome, utente._cognome );

				Attachement a = null;

				if ( idFile != null )
				{
					Files cvFile = new Files();

					using ( var db = new cv_ModelEntities() )
					{
						int _id = idFile.GetValueOrDefault();
						cvFile = db.Files.Where( f => f.Id.Equals( _id ) ).FirstOrDefault();
					}

					if ( cvFile != null )
					{
						a = new Attachement()
						{
							AttachementName = cvFile.Name,
							AttachementType = cvFile.MimeType,
							AttachementValue = cvFile.ContentType
						};
					}
				}

				this.InviaMail( utente._email, utenteDestinatario, ora, desc, job.mail, a );

				if ( string.IsNullOrEmpty( utente._email ) )
				{
					result = "Il sistema non è stato in grado di recuperare il suo indirizzo mail, pertanto non potrà ricevere il messaggio di avvenuta candidatura. In ogni caso i suoi dati sono stati registrati correttamente.";
				}
				return result;
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

		/// <summary>
		/// Reperimento di un JobPost a partire dal suo identificativo univoco
		/// </summary>
		/// <param name="idJob"></param>
		/// <returns></returns>
		private tblJobPosting GetJob ( int idJob )
		{
			tblJobPosting result = null;
			try
			{
				using ( var db = new CorsiJobEntities() )
				{
					var jobPost = db.tblJobPosting.Where( j => j.ID.Equals( idJob ) ).FirstOrDefault();

					if ( jobPost != null )
					{
						result = new tblJobPosting();
						result = jobPost;
					}
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
			return result;
		}

		private List<string> GetJobPostingEccezioni ()
		{
			List<string> result = new List<string>();
			try
			{
				using ( var db = new digiGappEntities() )
				{
					var eccezioni = db.MyRai_ParametriSistema.Where( x => x.Chiave.Equals( "JobPostingEccezioni", StringComparison.InvariantCultureIgnoreCase ) ).ToList();

					if ( eccezioni != null )
					{
						result = eccezioni.First().Valore1.Split( ',' ).ToList();
					}
				}
				return result;
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
			return result;
		}

		/// <summary>
		/// Reperimento del path in cui salvare i files caricati
		/// </summary>
		/// <returns></returns>
		private string GetStorePath (  )
		{
			string _storePath = string.Empty;
			try
			{
				using ( var db = new digiGappEntities() )
				{
					var s = db.MyRai_ParametriSistema.Where( p => p.Chiave.Equals( "JobPostingPath", StringComparison.InvariantCultureIgnoreCase ) ).FirstOrDefault();

					if ( s != null )
					{
						string tmp = "~/" + s.Valore1;
						_storePath = Server.MapPath( tmp );
					}
					else
					{
						_storePath = @"c:\JobPosting\";
					}
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
			return _storePath;
		}

		#region Invio mail

		private void InviaMail ( string destinatario, string utente, DateTime dataCreazione, string job, string mittente, Attachement a )
		{
			try
			{
				if ( string.IsNullOrEmpty( mittente ) )
				{
					mittente = "raiplace.selfservice@rai.it";
				}

				MailSender invia = new MailSender();
				Email eml = new Email();
				eml.From = mittente;

				// se manca il destinatario allora la mail verrà 
				// inviata soltanto a raiplace.selfservice@rai.it
				// o comunque all'indirizzo indicato nel db
				if ( string.IsNullOrEmpty( destinatario ) )
				{
					eml.toList = new string[] { mittente };
				}
				else
				{
					eml.toList = new string[] { destinatario };
					eml.ccList = new string[] { mittente };
				}

				eml.ContentType = "text/html";
				eml.Priority = 2;
				eml.SendWhen = DateTime.Now.AddSeconds( 1 );

				eml.Subject = this.GetMailTemplateRegistrazioneJobPostingSubject();
				eml.Body = this.GetMailTemplateRegistrazioneJobPosting();

				eml.AttachementsList = new Attachement[] { a };
				eml.Body = eml.Body.Replace( "#JOB", job );

				//eml.Body = eml.Body.Replace( "#DATA", dataCreazione.ToString( "dd/MM/yyyy HH:mm:ss" ) )
				//					.Replace( "#JOB", job )
				//					.Replace( "#UTENTE", utente );

				string[] parametriMail = GetDatiServizioMail();

				invia.Credentials = new System.Net.NetworkCredential( parametriMail[0], parametriMail[1], "RAI" );

				try
				{
					invia.Send( eml );
				}
				catch ( Exception ex )
				{
					throw new Exception( ex.Message );
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

		/// <summary>
		/// Reperimento delle credenziali da passare al servizio di invio mail
		/// </summary>
		/// <returns></returns>
		private string[] GetDatiServizioMail ()
		{
			string[] dati = null;

			try
			{
				using ( digiGappEntities db = new digiGappEntities() )
				{
					var result = db.MyRai_ParametriSistema.Where( p => p.Chiave.Equals( "AccountUtenteServizio", StringComparison.InvariantCultureIgnoreCase ) ).ToList();

					if ( result == null )
						throw new Exception( "Parametri per invio mail non trovati" );

					dati = new string[] { result.First().Valore1, result.First().Valore2 };

					return dati;
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

		/// <summary>
		/// Reperimento del template per l'invio della mail
		/// </summary>
		/// <returns></returns>
		private string GetMailTemplateRegistrazioneJobPostingSubject ()
		{
			string subject = null;

			try
			{
				using ( digiGappEntities db = new digiGappEntities() )
				{
					var result = db.MyRai_ParametriSistema.Where( p => p.Chiave.Equals( "MailTemplateRegistrazioneJobPostingSubject", StringComparison.InvariantCultureIgnoreCase ) ).ToList();

					if ( result == null )
						throw new Exception( "Template non trovato" );

					subject = result.First().Valore1;

					return subject;
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

		/// <summary>
		/// Reperimento del template per l'invio della mail
		/// </summary>
		/// <returns></returns>
		private string GetMailTemplateRegistrazioneJobPosting ()
		{
			string template = null;

			try
			{
				using ( digiGappEntities db = new digiGappEntities() )
				{
					var result = db.MyRai_ParametriSistema.Where( p => p.Chiave.Equals( "MailTemplateRegistrazioneJobPosting", StringComparison.InvariantCultureIgnoreCase ) ).ToList();

					if ( result == null )
						throw new Exception( "Template non trovato" );

					template = result.First().Valore1;

					return template;
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}
		#endregion

		/// <summary>
		/// Salvataggio dei dati associativi tra il file ed il jobposting
		/// </summary>
		/// <param name="idJob"></param>
		/// <param name="file"></param>
		private void SalvaJobPostingFiles ( int idJob, Files file )
		{
			try
			{
				using ( var db = new cv_ModelEntities() )
				{
					JobPostingFiles jpf = new JobPostingFiles()
					{
						IdJobPosting = idJob,
						Files = file
					};

					db.JobPostingFiles.Add( jpf );
					db.SaveChanges();
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

		[HttpPost]
		public JsonResult TestUploadAttached ( string idJob , string numerojp )
		{
			string result = String.Empty;
			try
			{
				if ( this.Request.Files.Count == 0 ||
					String.IsNullOrEmpty( this.Request.Files[0].FileName ) )
				{
					throw new Exception( "Si è verificato un errore.\nFile mancante" );
				}

				if ( String.IsNullOrEmpty( idJob ) )
				{
					throw new Exception( "Si è verificato un errore.\nManca l'identificativo del job per il quale si intende candidarsi" );
				}

				string matricola = CommonHelper.GetCurrentUsername();
				string pMatricola = "";

				if ( matricola.StartsWith( "RAI\\", StringComparison.InvariantCultureIgnoreCase ) )
				{
					matricola = matricola.Replace( "RAI\\", "" );
					pMatricola = matricola;
					if ( !( matricola.StartsWith( "P", StringComparison.InvariantCultureIgnoreCase ) ) )
					{
						pMatricola = "P" + matricola;
					}
				}
				else
				{
					pMatricola = matricola.Replace( "\\", "_" );
				}

				string ext = Path.GetExtension( this.Request.Files[0].FileName );

				string nomeFile = String.Format( "{0}_{1}{2}", idJob, pMatricola, ext );

				Files toSave = new Data.CurriculumVitae.Files();

				toSave.Id = -1;
				toSave.Matricola = matricola;
				toSave.Name = nomeFile;
				toSave.CreationDate = DateTime.Now;
				toSave.Size = this.Request.Files[0].ContentLength;
				toSave.MimeType = this.Request.Files[0].ContentType;

				byte[] buffer = new byte[this.Request.Files[0].ContentLength];
				using ( MemoryStream ms = new MemoryStream() )
				{
					int read;
					while ( ( read = this.Request.Files[0].InputStream.Read( buffer, 0, buffer.Length ) ) > 0 )
					{
						ms.Write( buffer, 0, read );
					}
					toSave.ContentType = ms.ToArray();
				}

				int iIdJob = int.Parse( idJob );
				//string basePath = this.GetStorePath();
				//string storagePath = Path.Combine( basePath, nomeFile );
				//this.Request.Files[0].SaveAs( storagePath );
				//this.SalvaCandidatura( iIdJob, nomeFile, numerojp );

				 // salvataggio dell'associativa tra file e candidatura
				this.SalvaJobPostingFiles( iIdJob, toSave );

				// aggiornamento dell'archivio con le candidature
				result = this.SalvaCandidatura( iIdJob, nomeFile, numerojp, toSave.Id );	

				return Json( new
				{
					success = true,
					errorMessage = result
				} );
			}
			catch ( Exception ex )
			{
				return Json( new
				{
					success = false,
					errorMessage = ex.Message
				} );
			}
		}
    }
}