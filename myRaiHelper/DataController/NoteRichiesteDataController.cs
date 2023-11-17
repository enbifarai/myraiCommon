using System;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Data.Objects;
using System.Data.Entity;
using System.Linq;
using System.Web;
using myRaiData;
using myRaiCommonTasks;

namespace myRaiCommonDatacontrollers.DataControllers
{
	/// <summary>
	/// DataController che si occupa della gestione della tabella MyRai_Note_Eccezioni
	/// Prevede azioni di inserimento, modifica e cancellazione di una nota per la segreteria
	/// collegata ad una determinata eccezione
	/// </summary>
	public class NoteRichiesteDataController
	{
		/// <summary>
		/// Metodo per l'inserimento di una nota legata ad una determinata eccezione
		/// </summary>
		/// <param name="matricola">Matricola dell'utente che effettua l'operazione</param>
		/// <param name="nota">Messaggio da inserire</param>
		/// <param name="giornata">Data per la quale è riferita la nota</param>
		/// <returns></returns>
        public MyRai_Note_Richieste InserisciNotaRichiesta ( string matricola , string nomeUtente , string nota , DateTime giornata , string sedeGapp , string destinatario = "Segreteria" , string tipoMittente = null )

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
                        Destinatario = destinatario ,
						Messaggio = nota,
						Mittente = matricola,
						DescrizioneMittente = nomeUtente,
						DataGiornata = giornata,
                        Visualizzatore = null ,
                        SedeGapp = sedeGapp
					};

					var exists = db.MyRai_Note_Richieste.Where( w => w.Mittente.Equals(matricola) &&
																	EntityFunctions.TruncateTime( w.DataGiornata ) == EntityFunctions.TruncateTime( giornata ) ).FirstOrDefault();

					// esiste già una nota per quella data
					if ( exists != null )
					{
                        // se la nota non è stata ancora letta ed il destinatario è la segreteria
                        if ( !exists.DataLettura.HasValue && (!String.IsNullOrEmpty( destinatario ) && destinatario.ToUpper( ).Equals( "SEGRETERIA" )) )
						{
							throw new Exception( "Impossibile inserire una nuova nota finchè non verrà letta la precedente" );
						}
					}

					db.MyRai_Note_Richieste.Add( toInsert );

					db.SaveChanges();
				}

                // se è stata inserita una nota dalla segreteria (destinatario != "SEGRETERIA", 
                // allora l'utente riceverà una mail con il messaggio a lui destinato
                if ( !destinatario.ToUpper( ).Equals( "SEGRETERIA" ) )
                {
                    InviaMailNotaDallaSegreteria( destinatario , nota , giornata , tipoMittente );
                }
            }
			catch ( Exception ex )
			{
                Logger.LogErrori( new MyRai_LogErrori( )
				{
					applicativo = "PORTALE",
					data = DateTime.Now,
                    error_message = String.Format( "InserisciNotaRichiesta - dati input - giornata: {0}, matricola: {1}, nota: {2} \n Motivo errore {3}" , giornata.ToString( "dd/MM/yyyy" ) , matricola , nota , ex.Message ) ,
                    provenienza = "NoteRichiesteDataController - InserisciNotaRichiesta" ,
					matricola = matricola
				} );

                //Logger.LogAzione( new MyRai_LogAzioni( )
                //{
                //    applicativo = "PORTALE" ,
                //    data = DateTime.Now ,
                //    descrizione_operazione = String.Format( "InserisciNotaRichiesta - dati input - giornata: {0}, matricola: {1}, nota: {2} \n Motivo errore {3}" , giornata.ToString( "dd/MM/yyyy" ) , matricola , nota , ex.Message ) ,
                //    provenienza = "NoteRichiesteDataController" ,
                //    operazione = "InserisciNotaRichiesta" ,
                //    matricola = matricola
                //} );
                throw new Exception( ex.Message );
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
        public MyRai_Note_Richieste ModificaNotaRichiesta ( int idNota , string matricola , string nota , string tipoMittente = null )
		{
			MyRai_Note_Richieste toUpdate = null;
            string destinatario = "";
            DateTime giornata = DateTime.Now;
            string messaggioOLD = "";

			try
			{
				DateTime data = DateTime.Now;

				using ( digiGappEntities db = new digiGappEntities() )
				{
					toUpdate = db.MyRai_Note_Richieste.Where( w => w.Id.Equals( idNota ) ).FirstOrDefault();

					if ( toUpdate != null )
					{
                        destinatario = toUpdate.Destinatario;
                        giornata = toUpdate.DataGiornata;
                        messaggioOLD = toUpdate.Messaggio;

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

                // Se il mittente è la segreteria ed il messaggio è stato modificato
                // allora l'utente riceverà una mail con il messaggio a lui destinato
                if ( !destinatario.ToUpper( ).Equals( "SEGRETERIA" ) &&
                    !String.IsNullOrEmpty( messaggioOLD ) &&
                    !String.IsNullOrEmpty( nota ) &&
                    !messaggioOLD.Trim( ).Equals( nota.Trim( ) ) )
                {
                    InviaMailNotaDallaSegreteria( destinatario , nota , giornata , tipoMittente );
                }
            }
			catch ( Exception ex )
			{
                //Logger.LogAzione( new MyRai_LogAzioni( )
                //{
                //    applicativo = "PORTALE" ,
                //    data = DateTime.Now ,
                //    descrizione_operazione = String.Format( "ModificaNotaRichiesta - dati input - idNota: {0}, matricola: {1}, nota: {2} \n Motivo errore {3}" , idNota , matricola , nota , ex.Message ) ,
                //    provenienza = "NoteRichiesteDataController" ,
                //    operazione = "ModificaNotaRichiesta" ,
                //    matricola = matricola
                //} );

                Logger.LogErrori( new MyRai_LogErrori( )
				{
					applicativo = "PORTALE",
					data = DateTime.Now,
                    error_message = String.Format( "ModificaNotaRichiesta - dati input - idNota: {0}, matricola: {1}, nota: {2} \n Motivo errore {3}" , idNota , matricola , nota , ex.Message ) ,
                    provenienza = "NoteRichiesteDataController - ModificaNotaRichiesta" ,
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

                throw new Exception( ex.Message );
            }
		}

		/// <summary>
        /// Reperimento delle note di cui l'utente con la matricola "matricola" è creatore o destinatario, 
        /// per la giornata passata
		/// </summary>
		/// <param name="matricola">Matricola dell'utente che effettua l'operazione</param>
		/// <param name="giornata">Data per la quale recuperare le note</param>
		/// <returns></returns>
		public List<MyRai_Note_Richieste> GetNoteRichieste ( string matricola, DateTime giornata )
		{
            List<MyRai_Note_Richieste> note = new List<MyRai_Note_Richieste>();
            List<MyRai_Note_Richieste> result = new List<MyRai_Note_Richieste>( );

			try
			{
				using ( digiGappEntities db = new digiGappEntities() )
				{
                    List<MyRai_Note_Richieste> l1 = db.MyRai_Note_Richieste.Where( w => w.Mittente.Equals( matricola ) &&
                                                        EntityFunctions.TruncateTime( w.DataGiornata ) == EntityFunctions.TruncateTime( giornata ) && w.Destinatario.Equals( "Segreteria" ) ).ToList( );

                    List<MyRai_Note_Richieste> l2 = db.MyRai_Note_Richieste.Where( w => w.Destinatario.Equals( matricola ) &&
									EntityFunctions.TruncateTime( w.DataGiornata ) == EntityFunctions.TruncateTime( giornata ) ).ToList();

                    if ( l1 != null && l1.Any( ) )
                    {
                        note.AddRange( l1.ToList( ) );
                    }

                    if ( l2 != null && l2.Any( ) )
                    {
                        note.AddRange( l2.ToList( ) );
                    }

                    if ( note != null && note.Any( ) )
                    {
                        var _temp = note.OrderBy( w => w.DataCreazione ).ToList( );
                        result.AddRange( _temp.ToList( ) );
                    }
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

            return result;
		}

		/// <summary>
		/// Metodo per impostare come letta, la nota associata all'eccezione
		/// </summary>
		/// <param name="idNota"></param>
		/// <param name="matricola"></param>
		/// <param name="nominativo"></param>
        public void SetLettura ( int idNota , string matricola , string nominativo , bool letta = true )
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
                        if ( letta )
                        {
                            toUpdate.DataLettura = time;
                            toUpdate.DataUltimaModifica = time;
                        }
                        else
                        {
                            toUpdate.DataLettura = null;
                            toUpdate.DataUltimaModifica = time;
                        }

                        toUpdate.Visualizzatore = matricola;
                        toUpdate.DescrizioneVisualizzatore = nominativo;
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

        /// <summary>
        /// Reperimento della nota con identificativo pari a idnota
        /// </summary>
        /// <param name="idNota">identificativo univoco della nota</param>
        /// <param name="matricola">Matricola dell'utente che effettua l'operazione</param>
        /// <returns></returns>
        public MyRai_Note_Richieste GetNotaRichiesta ( int idNota , string matricola )
        {
            MyRai_Note_Richieste nota = new MyRai_Note_Richieste( );

            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    nota = db.MyRai_Note_Richieste.Where( w => w.Id.Equals( idNota ) ).FirstOrDefault( );
                }
            }
            catch ( Exception ex )
            {
                Logger.LogAzione( new MyRai_LogAzioni( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    descrizione_operazione = String.Format( "GetNotaRichiesta - dati input - idnota: {0}, matricola richiedente: {1}\n Motivo errore {2}" , idNota , matricola , ex.Message ) ,
                    provenienza = "NoteRichiesteDataController" ,
                    operazione = "GetNotaRichiesta" ,
                    matricola = matricola
                } );
            }

            return nota;
        }

        /// <summary>
        /// Reperimento delle note associate alle richieste per SedeGapp
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="sedi"></param>
        /// <param name="destinatario"></param>
        /// <returns>Elenco delle sole note non lette</returns>
        public List<MyRai_Note_Richieste> GetNoteRichiestePerSedeGapp ( string matricola , List<string> sedi , string destinatario = "segreteria" )
        {
            List<MyRai_Note_Richieste> note = new List<MyRai_Note_Richieste>( );

            try
            {
                if ( sedi != null && sedi.Any( ) )
                {
                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        foreach ( var s in sedi )
                        {
                            note.AddRange( db.MyRai_Note_Richieste.Where( w => w.SedeGapp.Equals( s ) &&
                                w.Destinatario.Equals( destinatario ) &&
                                !w.DataLettura.HasValue ).ToList( ) );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                string joined = string.Join( "," , sedi );

                Logger.LogAzione( new MyRai_LogAzioni( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    descrizione_operazione = String.Format( "GetNoteRichiestePerSedeGapp - dati input - matricola: {0} sede: {1}\n Motivo errore {2}" , matricola , joined , ex.Message ) ,
                    provenienza = "NoteRichiesteDataController" ,
                    operazione = "GetNoteRichiestePerSedeGapp" ,
                    matricola = matricola
                } );
            }

            return note;
        }

        public List<MyRai_Note_Richieste> GetNoteRichiestePerSedeGappFiltrata ( string matricola , List<string> sedi , bool onlyNonLette = true , string destinatario = "segreteria" )
        {
            List<MyRai_Note_Richieste> note = new List<MyRai_Note_Richieste>( );

            try
            {

                if ( onlyNonLette )
                {
                    return GetNoteRichiestePerSedeGapp( matricola , sedi , destinatario );
                }
                else
                {
                    if ( sedi != null && sedi.Any( ) )
                    {
                        using ( digiGappEntities db = new digiGappEntities( ) )
                        {
                            foreach ( var s in sedi )
                            {
                                note.AddRange( db.MyRai_Note_Richieste.Where( w => w.SedeGapp.Equals( s ) &&
                                                                              w.Destinatario.Equals( destinatario ) ).ToList( ) );
                            }
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                string joined = string.Join( "," , sedi );

                Logger.LogAzione( new MyRai_LogAzioni( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    descrizione_operazione = String.Format( "GetNoteRichiestePerSedeGapp - dati input - matricola: {0} sede: {1}\n Motivo errore {2}" , matricola , joined , ex.Message ) ,
                    provenienza = "NoteRichiesteDataController" ,
                    operazione = "GetNoteRichiestePerSedeGapp" ,
                    matricola = matricola
                } );
            }

            return note;
        }

        /// <summary>
        /// Reperimento delle note associate alle richieste per SedeGapp
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="giornata"></param>
        /// <param name="destinatario"></param>
        /// <param name="nonLetti"></param>
        /// <returns></returns>
        public List<MyRai_Note_Richieste> GetNoteRichiestePerGiornata ( string matricola , DateTime giornata , string destinatario = "segreteria" , bool nonLetti = false )
        {
            List<MyRai_Note_Richieste> note = new List<MyRai_Note_Richieste>( );

            try
            {
                // reperimento delle note per la giornata indicata
                note = this.GetNoteRichieste( matricola , giornata );

                if ( note != null && note.Any( ) )
                {
                    // se nonLetti == True allora prenderà
                    // soltanto le note che non sono state ancora lette
                    if ( nonLetti )
                    {
                        note = note.Where( w => !w.DataLettura.HasValue ).ToList( );
                    }
                }
            }
            catch ( Exception ex )
            {
                Logger.LogAzione( new MyRai_LogAzioni( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    descrizione_operazione = String.Format( "GetNoteRichiestePerSedeGapp - dati input - data: {0} \n Motivo errore {1}" , giornata.ToString( "dd/MM/yyyy" ) , ex.Message ) ,
                    provenienza = "NoteRichiesteDataController" ,
                    operazione = "GetNoteRichiestePerSedeGapp" ,
                    matricola = matricola
                } );
            }

            return note;
        }

        private void InviaMailNotaDallaSegreteria ( string matricola , string messaggio , DateTime data , string tipoMittente = null )
        {
            string body = "";
            string oggetto = "";
            string dest = "";
            try
            {
                //dest = CommonTasks.GetEmailPerMatricola( matricola.TrimStart( 'P' ) );
                //dest = "RUO.SIP.PRESIDIOOPEN@rai.it";

                dest = String.Format( "P{0}@rai.it" , matricola.TrimStart( 'P' ) );

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault( x => x.Chiave.Equals( "MailTemplateNotaDallaSegreteria" ) );
                    if ( p != null )
                    {
                        body = p.Valore1;
                        oggetto = p.Valore2;
                    }
                }

                if ( !String.IsNullOrEmpty( body ) && !String.IsNullOrEmpty( body ) && !String.IsNullOrEmpty( dest ) )
                {
                    body = body.Replace( "#GIORNO" , data.ToString( "dd/MM/yyyy" ) );
                    body = body.Replace( "#MESSAGGIO" , messaggio );

                    if ( !String.IsNullOrEmpty( tipoMittente ) )
                    {
                        if ( tipoMittente.ToUpper( ).Equals( "SEGRETERIA" ) )
                        {
                            tipoMittente = "dalla segreteria";
                            oggetto = "Messaggio dalla segreteria";
                        }
                        else
                        {
                            tipoMittente = "dall'ufficio del personale";
                            oggetto = "Messaggio dall'ufficio del personale";
                        }

                        body = body.Replace( "##MITTENTE##" , tipoMittente );
                    }
                    else
                    {
                        body = body.Replace( "##MITTENTE##" , "dalla segreteria" );
                    }

                    GestoreMail mail = new GestoreMail( );

                    var response = mail.InvioMail( body , oggetto , dest , "raiplace.selfservice@rai.it" , CommonTasks.EnumParametriSistema.MailTemplateNotaDallaSegreteriaFrom );

                    if ( response != null && response.Errore != null )
                    {
                        using ( digiGappEntities db = new digiGappEntities( ) )
                        {
                            myRaiData.MyRai_LogErrori err = new MyRai_LogErrori( )
                            {
                                applicativo = "ServizioWCF" ,
                                data = DateTime.Now ,
                                provenienza = "InviaMailNotaDallaSegreteria" ,
                                error_message = response.Errore + " per " + dest
                            };
                            db.MyRai_LogErrori.Add( err );
                            try
                            {
                                db.SaveChanges( );
                            }
                            catch ( Exception ex )
                            {
                                Logger.LogErrori( new MyRai_LogErrori( )
                                {
                                    applicativo = "ServizioWCF" ,
                                    data = DateTime.Now ,
                                    provenienza = "InviaMailNotaDallaSegreteria" ,
                                    error_message = response.Errore + " per " + dest
                                } );
                            }
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                Logger.LogErrori( new MyRai_LogErrori( )
                {
                    applicativo = "ServizioWCF" ,
                    data = DateTime.Now ,
                    provenienza = "InviaMailNotaDallaSegreteria" ,
                    error_message = ex.Message + " per " + dest
                } );
            }
        }

    }
}