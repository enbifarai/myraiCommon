using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using myRaiData;
using myRaiHelper;

namespace myRai.DataControllers
{
    /// <summary>
    /// DataController che si occupa della gestione della tabella MyRai_Note_Eccezioni
    /// Prevede azioni di inserimento, modifica e cancellazione di una nota per la segreteria
    /// collegata ad una determinata eccezione
    /// </summary>
    public class NoteRichiesteDataController_
	{
		/// <summary>
		/// Metodo per l'inserimento di una nota legata ad una determinata eccezione
		/// </summary>
		/// <param name="matricola">Matricola dell'utente che effettua l'operazione</param>
		/// <param name="nota">Messaggio da inserire</param>
		/// <param name="giornata">Data per la quale è riferita la nota</param>
		/// <returns></returns>
		public MyRai_Note_Richieste InserisciNotaRichiesta ( string matricola, string nomeUtente, string nota, DateTime giornata )
		{
			MyRai_Note_Richieste toInsert = null;

			try
			{
				DateTime data = DateTime.Now;

				using ( digiGappEntities db = new digiGappEntities() )
				{
					toInsert = new MyRai_Note_Richieste()
					{
						DataCreazione = data,
						DataLettura = null,
						DataUltimaModifica = data,
						DescrizioneVisualizzatore = null,
						Destinatario = "Segreteria",
						Messaggio = nota,
						Mittente = matricola,
						DescrizioneMittente = nomeUtente,
						DataGiornata = giornata,
						Visualizzatore = null
					};

					var exists = db.MyRai_Note_Richieste.Where( w => w.Mittente.Equals(matricola) &&
																	EntityFunctions.TruncateTime( w.DataGiornata ) == EntityFunctions.TruncateTime( giornata ) ).FirstOrDefault();

					// esiste già una nota per quella data
					if ( exists != null )
					{
						// la nota non è stata ancora letta
						if ( !exists.DataLettura.HasValue )
						{
							throw new Exception( "Impossibile inserire una nuova nota finchè non verrà letta la precedente" );
						}
					}

					db.MyRai_Note_Richieste.Add( toInsert );

					db.SaveChanges();
				}
			}
			catch ( Exception ex )
			{
				Logger.LogAzione( new MyRai_LogAzioni()
				{
					applicativo = "PORTALE",
					data = DateTime.Now,
					descrizione_operazione = String.Format( "InserisciNotaRichiesta - dati input - giornata: {0}, matricola: {1}, nota: {2} \n Motivo errore {3}", giornata.ToString( "dd/MM/yyyy" ), matricola, nota, ex.Message ),
					provenienza = "NoteRichiesteDataController",
					operazione = "InserisciNotaRichiesta",
					matricola = matricola
				} );
			}

			return toInsert;
		}

		/// <summary>
		/// Modifica di una nota esistente
		/// </summary>
		/// <param name="idNota">Identificativo univoco della nota</param>
		/// <param name="matricola">Matricola dell'utente che effettua l'operazione</param>
		/// <param name="nota">Messaggio da modificare</param>
		/// <returns></returns>
		public MyRai_Note_Richieste ModificaNotaRichiesta ( int idNota, string matricola, string nota )
		{
			MyRai_Note_Richieste toUpdate = null;

			try
			{
				DateTime data = DateTime.Now;

				using ( digiGappEntities db = new digiGappEntities() )
				{
					toUpdate = db.MyRai_Note_Richieste.Where( w => w.Id.Equals( idNota ) ).FirstOrDefault();

					if ( toUpdate != null )
					{
						// Se la nota è stata letta non è possibile cancellarla
						if ( toUpdate.DataLettura.HasValue )
						{
							throw new Exception( "Si è verificato un errore. Impossibile modificare la nota già letta" );
						}
						else
						{
							if ( !toUpdate.Mittente.Equals( matricola ) )
							{
								throw new Exception( "Si è verificato un errore. Non si dispone dei diritti necessari per eseguire tale operazione." );
							}

							toUpdate.Messaggio = nota;
							toUpdate.DataUltimaModifica = DateTime.Now;
						}
					}
					else
					{
						throw new Exception( "Si è verificato un errore, nota non trovata." );
					}

					db.SaveChanges();
				}
			}
			catch ( Exception ex )
			{
				Logger.LogAzione( new MyRai_LogAzioni()
				{
					applicativo = "PORTALE",
					data = DateTime.Now,
					descrizione_operazione = String.Format( "ModificaNotaRichiesta - dati input - idNota: {0}, matricola: {1}, nota: {2} \n Motivo errore {3}", idNota, matricola, nota, ex.Message ),
					provenienza = "NoteRichiesteDataController",
					operazione = "ModificaNotaRichiesta",
					matricola = matricola
				} );
			}

			return toUpdate;
		}

		/// <summary>
		/// Cancellazione di una nota
		/// </summary>
		/// <param name="idNota">Identificativo univoco della nota</param>
		/// <param name="matricola">Matricola dell'utente che effettua l'operazione</param>
		/// <returns></returns>
		public void EliminaNotaRichiesta ( int idNota, string matricola )
		{
			MyRai_Note_Richieste toRemove = null;

			try
			{
				using ( digiGappEntities db = new digiGappEntities() )
				{
					toRemove = db.MyRai_Note_Richieste.Where( w => w.Id.Equals( idNota ) ).FirstOrDefault();

					if ( toRemove != null )
					{
						// Se la nota è stata letta non è possibile cancellarla
						if ( toRemove.DataLettura.HasValue )
						{
							throw new Exception( "Si è verificato un errore. Impossibile rimuovere la nota già letta." );
						}
						else
						{
							if ( !toRemove.Mittente.Equals( matricola ) )
							{
								throw new Exception( "Si è verificato un errore. Non si dispone dei diritti necessari per eseguire tale operazione." );
							}
							db.MyRai_Note_Richieste.Remove( toRemove );
						}
					}
					else
					{
						throw new Exception( "Si è verificato un errore, nota non trovata." );
					}

					db.SaveChanges();
				}
			}
			catch ( Exception ex )
			{
				Logger.LogAzione( new MyRai_LogAzioni()
				{
					applicativo = "PORTALE",
					data = DateTime.Now,
					descrizione_operazione = String.Format( "EliminaNotaRichiesta - dati input - idNota: {0}, matricola: {1} \n Motivo errore {2}", idNota, matricola, ex.Message ),
					provenienza = "NoteRichiesteDataController",
					operazione = "EliminaNotaRichiesta",
					matricola = matricola
				} );
			}
		}

		/// <summary>
		/// Reperimento delle note legate ad una determinata giornata
		/// </summary>
		/// <param name="matricola">Matricola dell'utente che effettua l'operazione</param>
		/// <param name="giornata">Data per la quale recuperare le note</param>
		/// <returns></returns>
		public List<MyRai_Note_Richieste> GetNoteRichieste ( string matricola, DateTime giornata )
		{
			List<MyRai_Note_Richieste> note = new List<MyRai_Note_Richieste>();

			try
			{
				using ( digiGappEntities db = new digiGappEntities() )
				{
					note = db.MyRai_Note_Richieste.Where( w => w.Mittente.Equals( matricola ) &&
														EntityFunctions.TruncateTime( w.DataGiornata ) == EntityFunctions.TruncateTime( giornata ) ).ToList();
				}
			}
			catch ( Exception ex )
			{
				Logger.LogAzione( new MyRai_LogAzioni()
				{
					applicativo = "PORTALE",
					data = DateTime.Now,
					descrizione_operazione = String.Format( "GetNoteRichieste - dati input - data: {0} \n Motivo errore {1}", giornata.ToString( "dd/MM/yyyy" ), ex.Message ),
					provenienza = "NoteRichiesteDataController",
					operazione = "GetNoteRichieste",
					matricola = matricola
				} );
			}

			return note;
		}

		/// <summary>
		/// Metodo per impostare come letta, la nota associata all'eccezione
		/// </summary>
		/// <param name="idNota"></param>
		/// <param name="matricola"></param>
		/// <param name="nominativo"></param>
		public void SetLettura ( int idNota, string matricola, string nominativo )
		{
			MyRai_Note_Richieste toUpdate = null;

			DateTime time = DateTime.Now;
			try
			{
				using ( digiGappEntities db = new digiGappEntities() )
				{
					toUpdate = db.MyRai_Note_Richieste.Where( w => w.Id.Equals( idNota ) ).FirstOrDefault();

					if ( toUpdate != null )
					{
						// Se la nota è stata letta non è possibile cancellarla
						if ( toUpdate.DataLettura.HasValue )
						{
							throw new Exception( "Si è verificato un errore. Impossibile modificare la nota già letta" );
						}
						else
						{
							toUpdate.DataLettura = time;
							toUpdate.DataUltimaModifica = time;
							toUpdate.Visualizzatore = matricola;
							toUpdate.DescrizioneVisualizzatore = nominativo;
						}
					}
					else
					{
						throw new Exception( "Si è verificato un errore, nota non trovata." );
					}

					db.SaveChanges();
				}
			}
			catch ( Exception ex )
			{
				Logger.LogAzione( new MyRai_LogAzioni()
				{
					applicativo = "PORTALE",
					data = DateTime.Now,
					descrizione_operazione = String.Format( "SetLettura - dati input - idNota: {0}, matricola: {1} \n Motivo errore {2}", idNota, matricola, ex.Message ),
					provenienza = "NoteRichiesteDataController",
					operazione = "SetLettura",
					matricola = matricola
				} );
			}
		}
	}
}