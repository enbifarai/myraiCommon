using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiData;

namespace myRaiCommonDatacontrollers
{
	public class Logger
	{
		public static Boolean LogAzione ( MyRai_LogAzioni azione )
		{
			using ( var db = new digiGappEntities() )
			{
				azione.data = DateTime.Now;
				azione.applicativo = "Portale";
				azione.provenienza = GetServerName() + azione.provenienza;
				if ( azione.provenienza != null && azione.provenienza.Length > 100 )
					azione.provenienza = azione.provenienza.Substring( 0, 99 );

				db.MyRai_LogAzioni.Add( azione );

				try
				{
					db.SaveChanges();
					return true;
				}
				catch ( Exception ex )
				{
					return false;
				}
			}
		}

		public static string GetServerName ()
		{
			try
			{
				return System.Environment.MachineName + "-";
			}
			catch ( Exception ex )
			{
				return "e-";
			}
		}

		public static Boolean LogErrori ( MyRai_LogErrori errore )
		{
			using ( var db = new digiGappEntities() )
			{
				errore.data = DateTime.Now;
				errore.applicativo = "Portale";
				errore.provenienza = GetServerName() + errore.provenienza;
				if ( errore.provenienza != null && errore.provenienza.Length > 100 )
					errore.provenienza = errore.provenienza.Substring( 0, 99 );

				db.MyRai_LogErrori.Add( errore );

				try
				{
					db.SaveChanges();

					return true;
				}
				catch ( Exception ex )
				{
					return false;
				}
			}

		}
	}
}