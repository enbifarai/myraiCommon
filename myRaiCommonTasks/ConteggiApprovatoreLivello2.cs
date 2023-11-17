using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiData;

namespace myRaiCommonTasks
{
	public class ConteggiApprovatoreLivello2
	{
		/// <summary>
		/// Calcolo dei pdf settimanali per l'approvatore di secondo livello
		/// </summary>
		/// <param name="pMatricola">PMatricola dell'utente per il quale si intende 
		/// calcolare il numero di pdf ancora non lavorati</param>
		/// <returns>Numero di pdf settimanali ancora non elaborati dall'utente </returns>
		public static int GetConteggioPDFSettimanali ( string pMatricola )
		{
			int result = 0;
			try
			{
				Abilitazioni abilitazioni = CommonTasks.getAbilitazioni();

				if ( abilitazioni != null &&
					abilitazioni.ListaAbilitazioni != null &&
					abilitazioni.ListaAbilitazioni.Any() )
				{
					var sedi = abilitazioni.ListaAbilitazioni.Where( w => w.MatrLivello2.Equals( pMatricola ) ).ToList();

					if ( sedi != null && sedi.Any() )
					{
						sedi.ForEach( s =>
						{
							result += GetPDFCount( s.Sede );
						} );
					}
				}
			}
			catch ( Exception ex )
			{
				result = 0;
			}

			return result;
		}

		private static int GetPDFCount ( string sede )
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
	}
}
