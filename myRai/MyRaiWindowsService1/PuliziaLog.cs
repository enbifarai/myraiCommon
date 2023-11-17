using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiData;
using MyRaiWindowsService1.Model;

namespace MyRaiWindowsService1
{
	public class PuliziaLog
	{
		public PuliziaLog ()
		{
		}

		//public void Start (TipoLogEnum? tipolog = null)
		//{
		//	Logger.Log( "Avvio pulizia Log." );

		//	bool result = true;

		//	try
		//	{
		//		using ( digiGappEntities db = new digiGappEntities() )
		//		{
		//			// se la tipologia è null allora verranno eseguite tutte le stored procedure di 
		//			// pulizia dei log
		//			if ( tipolog == null )
		//			{
		//				Logger.Log( "Pulizia di tutti i log" );
						
		//				result = ( this.PulisciLogErrori() && 
		//							this.PulisciLogApi() &&
		//							this.PulisciLogAzioni() && 
		//							this.PulisciLogDB() );

		//				if ( !result )
		//				{
		//					Logger.Log( "Si è verificato un errore durante la pulizia di uno o più log" );
		//				}
		//			}
		//			else if ( tipolog.Value == TipoLogEnum.LogErrori )
		//			{
		//				result = ( this.PulisciLogErrori() );
		//			}
		//			else if ( tipolog.Value == TipoLogEnum.LogApi )
		//			{
		//				result = ( this.PulisciLogApi() );
		//			}
		//			else if ( tipolog.Value == TipoLogEnum.LogDB )
		//			{
		//				result = ( this.PulisciLogDB() );
		//			}
		//			else if ( tipolog.Value == TipoLogEnum.LogAzioni )
		//			{
		//				result = ( this.PulisciLogAzioni() );
		//			}
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		Logger.Log( "Si è verificato un errore durante la pulizia di uno o più log" );
		//	}

		//	Logger.Log( "Fine pulizia Log." );
		//}

		///// <summary>
		///// Metodo che si occupa di lanciare la stored procedure che 
		///// rimuove i vecchi log di errore
		///// </summary>
		///// <returns>True se l'esecuzione della stored procedure non ha riscontrato nessun errore
		///// False altrimenti</returns>
		//private bool PulisciLogErrori ()
		//{
		//	Logger.Log( "Avvio della pulizia dei soli log di errore" );
		//	bool result = true;
		//	try
		//	{
		//		using ( digiGappEntities db = new digiGappEntities() )
		//		{
		//			db.Clear_MyRai_LogErrori();
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		result = false;
		//	}

		//	if ( !result )
		//	{
		//		Logger.Log( "Si è verificato un errore durante la pulizia dei log di errore" );
		//	}

		//	return result;
		//}

		///// <summary>
		///// Metodo che si occupa di lanciare la stored procedure che 
		///// rimuove i vecchi log relativi alle API
		///// </summary>
		///// <returns>True se l'esecuzione della stored procedure non ha riscontrato nessun errore
		///// False altrimenti</returns>
		//private bool PulisciLogApi ()
		//{
		//	Logger.Log( "Avvio pulizia dei soli log relativi alle API" );
		//	bool result = true;
		//	try
		//	{
		//		using ( digiGappEntities db = new digiGappEntities() )
		//		{
		//			db.Clear_MyRai_LogAPI();
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		result = false;
		//	}

		//	if ( !result )
		//	{
		//		Logger.Log( "Si è verificato un errore durante la pulizia dei log relativi alle API" );
		//	}

		//	return result;
		//}

		///// <summary>
		///// Metodo che si occupa di lanciare la stored procedure che 
		///// rimuove i vecchi log relativi al DB
		///// </summary>
		///// <returns>True se l'esecuzione della stored procedure non ha riscontrato nessun errore
		///// False altrimenti</returns>
		//private bool PulisciLogDB ()
		//{
		//	Logger.Log( "Avvio pulizia dei soli log relativi al DB" );
		//	bool result = true;
		//	try
		//	{
		//		using ( digiGappEntities db = new digiGappEntities() )
		//		{
		//			db.Clear_MyRai_LogDB();
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		result = false;
		//	}

		//	if ( !result )
		//	{
		//		Logger.Log( "Si è verificato un errore durante la pulizia dei log relativi al DB" );
		//	}

		//	return result;
		//}

		///// <summary>
		///// Metodo che si occupa di lanciare la stored procedure che 
		///// rimuove i vecchi log relativi alle azioni
		///// </summary>
		///// <returns>True se l'esecuzione della stored procedure non ha riscontrato nessun errore
		///// False altrimenti</returns>
		//private bool PulisciLogAzioni ()
		//{
		//	Logger.Log( "Avvio della pulizia dei soli log relativi alle azioni" );
		//	bool result = true;
		//	try
		//	{
		//		using ( digiGappEntities db = new digiGappEntities() )
		//		{
		//			db.Clear_MyRai_LogAzioni();
		//		}
		//	}
		//	catch ( Exception ex )
		//	{
		//		result = false;
		//	}

		//	if ( !result )
		//	{
		//		Logger.Log( "Si è verificato un errore durante la pulizia dei log relativi alle azioni" );
		//	}

		//	return result;
		//}

	}
}