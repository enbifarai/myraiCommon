using System;
using System.Linq;
using myRai.Data.CurriculumVitae;

namespace myRai.DataControllers
{
	public class FileDataController
	{
		/// <summary>
		/// Metodo per la rimozione di un file
		/// </summary>
		/// <param name="id"></param>
		public void DeleteFile ( int id )
		{
			try
			{
				using ( var db = new cv_ModelEntities() )
				{
					var toRemove = db.Files.Where( f => f.Id.Equals( id ) ).FirstOrDefault();

					if ( toRemove != null )
					{
						db.Files.Remove( toRemove );
					}
					else
					{
						throw new Exception( "Errore nel reperimento del file da rimuovere" );
					}

					db.SaveChanges();
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

		/// <summary>
		/// Metodo per il salvataggio di un file
		/// </summary>
		/// <param name="toSave"></param>
		/// <returns></returns>
		public myRai.Data.CurriculumVitae.Files SaveFile ( myRai.Data.CurriculumVitae.Files toSave )
		{
			try
			{
				if ( toSave.Id <= 0 )
				{
					toSave = this.InsertFile( toSave );
				}
				else
				{
					toSave = this.UpdateFile( toSave );
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}

			return toSave;
		}

		/// <summary>
		/// Inserimento di un nuovo file
		/// </summary>
		/// <param name="toSave"></param>
		/// <returns></returns>
		private myRai.Data.CurriculumVitae.Files InsertFile ( myRai.Data.CurriculumVitae.Files toSave )
		{
			try
			{
				using ( var db = new cv_ModelEntities() )
				{
					db.Files.Add( toSave );

					db.SaveChanges();
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}

			return toSave;
		}

		/// <summary>
		/// Aggiornamento dei dati relativi ad un file esistente
		/// </summary>
		/// <param name="toSave"></param>
		/// <returns></returns>
		private myRai.Data.CurriculumVitae.Files UpdateFile ( myRai.Data.CurriculumVitae.Files toSave )
		{
			try
			{
				using ( var db = new cv_ModelEntities() )
				{
					var toUpdate = db.Files.Where( f => f.Id.Equals( toSave.Id ) ).FirstOrDefault();

					if ( toUpdate != null )
					{
						toUpdate = toSave;
					}
					else
					{
						throw new Exception( "Errore nel reperimento del file da aggiornare" );
					}
					
					db.SaveChanges();
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}

			return toSave;
		}
	}
}