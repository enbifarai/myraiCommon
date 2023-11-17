using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using myRai.Zip;
using myRaiCommonModel.AdminJobPosting;
using myRaiData;
using myRai.Data.CurriculumVitae;

namespace myRai.Controllers
{
    public class AdminJobPostingController : Controller
    {

		/// <summary>
		/// Visualizza l'elenco dei jobPosting
		/// </summary>
		/// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

		/// <summary>
		/// Visualizza l'elenco dei jobPosting
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public PartialViewResult LoadJobs ()
		{
			try
			{
				List<tblJobPosting> model = this.GetJobs();

				return PartialView( "~/Views/AdminJobPosting/subpartial/TblJobsPosting.cshtml", model );
			}
			catch ( Exception ex )
			{
				return PartialView( "~/Views/Shared/TblError.cshtml", new HandleErrorInfo( ex, "AdminJobPostingController", "LoadJobs" ) );
			}
		}

		/// <summary>
		/// Visualizzazione della pagina di dettaglio del job
		/// </summary>
		/// <param name="idJob"></param>
		/// <returns></returns>
		public ActionResult DettaglioJob ( int idJob )
		{
			AdminDettaglioJobPostingVM model = new AdminDettaglioJobPostingVM();

			model.JobPosting = this.GetJob( idJob );
			model.Files = this.GetJobPostingFiles( idJob );

			return View( "~/Views/AdminJobPosting/DettaglioJob.cshtml", model );
		}


		public ActionResult ShowCV ( int idFile )
		{
			try
			{
				Files cvFile = new Files();
				MemoryStream ms = new MemoryStream();

				using ( var db = new cv_ModelEntities() )
				{
					cvFile = db.Files.Where( f => f.Id.Equals( idFile ) ).FirstOrDefault();

					if ( cvFile != null )
					{
						ms = new MemoryStream( cvFile.ContentType ); 
					}
				}
				if ( cvFile != null )
				{
					return new FileContentResult( ms.ToArray(), cvFile.MimeType );
				}
				else
				{
					return null;
				}				
			}
			catch ( Exception ex )
			{
				return null;
			}
		}


		public FileResult Download ( int idFile )
		{
			try
			{
				Files cvFile = new Files();
				MemoryStream ms = new MemoryStream();

				using ( var db = new cv_ModelEntities() )
				{
					cvFile = db.Files.Where( f => f.Id.Equals( idFile ) ).FirstOrDefault();

					if ( cvFile != null )
					{
						ms = new MemoryStream( cvFile.ContentType );
					}
				}
				if ( cvFile != null )
				{
					return File( cvFile.ContentType, cvFile.MimeType, cvFile.Name );
				}
				else
				{
					return null;
				}
			}
			catch ( Exception ex )
			{
				return null;
			}
		}

		public FileResult DownloadZip ( int idJob )
		{
			try
			{
				SharpZipLibCompressionDataController controller = new SharpZipLibCompressionDataController();

				List<CompressionFileItem> filesToCompress = new List<CompressionFileItem>();

				byte[] b = null;

				using ( var db = new cv_ModelEntities() )
				{
					var files = db.JobPostingFiles.Where( j => j.IdJobPosting.Equals( idJob ) ).ToList();

					if ( files != null )
					{
						foreach ( var f in files )
						{
							filesToCompress.Add( new CompressionFileItem()
							{
								FileName = f.Files.Name,
								Content = f.Files.ContentType
							} );
						}
					}
				}

				if ( filesToCompress.Any() )
				{
					b = controller.Compress( filesToCompress, new CompressionOptions()
					{
						CompressionLevel = 3
					} );
				}

				string fileName = String.Format( "{0}.zip", idJob );

				return File( b, "application/zip", fileName );
			}
			catch ( Exception ex )
			{
				return null;
			}
		}

		private tblJobPosting GetJob (int idJob)
		{
			tblJobPosting result = new tblJobPosting();
			try
			{
				using ( var db = new CorsiJobEntities() )
				{
					result = db.tblJobPosting.Where( j => j.ID.Equals( idJob ) ).FirstOrDefault();
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
			return result;
		}


		/// <summary>
		/// Reperimento dell'elenco dei jobs in ordinati per scadenza DESC
		/// </summary>
		/// <returns></returns>
		private List<tblJobPosting> GetJobs ()
		{
			List<tblJobPosting> result = new List<tblJobPosting>();
			try
			{
				using ( var db = new CorsiJobEntities() )
				{
					result = db.tblJobPosting.OrderByDescending( j => j.scadenza ).ToList();
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
			return result;
		}

		/// <summary>
		/// Reperimento dell'elenco dei files per il job selezionato
		/// </summary>
		/// <param name="idJob"></param>
		/// <returns></returns>
		private List<JobPostingFilesExt> GetJobPostingFiles ( int idJob )
		{
			List<JobPostingFilesExt> result = new List<JobPostingFilesExt>();
			try
			{
				using ( var db = new cv_ModelEntities() )
				{
					var files = db.JobPostingFiles.Where( j => j.IdJobPosting.Equals( idJob ) ).ToList();

					if ( files != null )
					{
						files.ForEach( f =>
						{					
							var user = db.TDipendenti.Where( d => d.Matricola.Equals( f.Files.Matricola, StringComparison.InvariantCultureIgnoreCase ) ).FirstOrDefault();

							string nominativo = String.Empty;

							if ( user != null )
							{
								nominativo = user.Nominativo;
							}

							result.Add( new JobPostingFilesExt()
							{
								Id = f.Id,
								IdJobPosting = f.IdJobPosting,
								Files = f.Files,
								Nominativo = nominativo
							} );
						} );
					}
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
			return result;
		}

    }
}