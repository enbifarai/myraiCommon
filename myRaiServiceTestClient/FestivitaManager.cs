using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiServiceTestClient
{
    public class FestivitaManager
    {
        #region Public

        /// <summary>
        /// Verifica se una particolare data è un giorno festivo
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sedeGapp">Codice sede gapp di appartenenza</param>
        /// <returns></returns>
        public static bool IsFestivo ( DateTime data , string sedeGapp )
        {
            bool result = false;

            try
            {
                // Epifania
                if ( data.Day == 6 && data.Month == 1 )
                {
                    result = true;
                }

                // Liberazione
                if ( data.Day == 25 && data.Month == 4 )
                {
                    result = true;
                }

                // Festa della Repubblica
                if ( data.Day == 2 && data.Month == 6 )
                {
                    result = true;
                }

                // 1 Novembre Tutti i santi
                if ( data.Day == 1 && data.Month == 11 )
                {
                    result = true;
                }

                // Immacolata concezione
                if ( data.Day == 8 && data.Month == 12 )
                {
                    result = true;
                }

                // Santo Stefano
                if ( data.Day == 26 && data.Month == 12 )
                {
                    result = true;
                }

                DateTime pasqua = GetPasqua( data.Year );

                // Lunedì dell'angelo
                if ( data.Day == ( pasqua.Day + 1 ) && data.Month == pasqua.Month )
                {
                    result = true;
                }

                // PATRONO
                DateTime patrono = GetPatrono( sedeGapp );

                if ( patrono != DateTime.MinValue )
                {
                    if ( data.Day == patrono.Day && data.Month == patrono.Month )
                    {
                        result = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }

            return result;
        }

        /// <summary>
        /// Verifica se una particolare data è un giorno super festivo
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsSuperFestivo ( DateTime data )
        {
            bool result = false;

            try
            {
                // Epifania
                if ( data.Day == 1 && data.Month == 1 )
                {
                    result = true;
                }

                // 1 Maggio Festa dei lavoratori
                if ( data.Day == 1 && data.Month == 5 )
                {
                    result = true;
                }

                // 15 Agosto
                if ( data.Day == 15 && data.Month == 8 )
                {
                    result = true;
                }

                // Natale
                if ( data.Day == 25 && data.Month == 12 )
                {
                    result = true;
                }

                //// Santa Pasqua
                //if ( IsPasqua( data ) )
                //{
                //	result = true;
                //}
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }

            return result;
        }

        /// <summary>
        /// Verifica se una particolare data è un giorno ex festivo
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sedeGapp">Codice sede gapp di appartenenza</param>
        /// <returns></returns>
        public static bool IsExFestivo ( DateTime data , string sedeGapp )
        {
            bool result = false;

            try
            {
                // San Giuseppe
                if ( data.Day == 19 && data.Month == 3 )
                {
                    result = true;
                }

                // Unità Nazionale
                if ( data.Day == 4 && data.Month == 11 )
                {
                    result = true;
                }

                // Ascensione
                if ( IsAscensionDay( data ) )
                {
                    result = true;
                }

                // CorpusDomini
                if ( IsCorpusDomini( data ) )
                {
                    result = true;
                }

                var sede = GetSedeGappDetails( sedeGapp );

                if ( sede != null )
                {
                    if ( sede.CalendarioDiSede != null && sede.CalendarioDiSede.Equals( "ROMA" , StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // San Giovanni
                        if ( data.Day == 24 && data.Month == 6 )
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        if ( data.Day == 29 && data.Month == 6 )
                        {
                            result = true;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
            return result;
        }

        /// <summary>
        /// Restituisce l'elenco di tutte le festività per l'anno considerato
        /// </summary>
        /// <param name="anno">anno per il quale si intende ottenere l'elenco delle festività</param>
        /// <returns></returns>
        public static List<DateTime> GetFestivita ( int anno )
        {
            List<DateTime> result = new List<DateTime>( );

            try
            {
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
            return result;
        }

        /// <summary>
        /// Restituisce l'elenco di tutte le super festività per l'anno considerato
        /// </summary>
        /// <param name="anno">anno per il quale si intende ottenere l'elenco delle super festività</param>
        /// <returns></returns>
        public static List<DateTime> GetSuperFestivita ( int anno )
        {
            List<DateTime> result = new List<DateTime>( );

            try
            {
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
            return result;
        }

        /// <summary>
        /// Restituisce l'elenco di tutte le ex festività per l'anno considerato
        /// </summary>
        /// <param name="anno">anno per il quale si intende ottenere l'elenco delle ex festività</param>
        /// <returns></returns>
        public static List<DateTime> GetExFestivita ( int anno )
        {
            List<DateTime> result = new List<DateTime>( );

            try
            {
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
            return result;
        }

        /// <summary>
        /// Calcolo della pasqua
        /// </summary>
        /// <param name="year">Anno</param>
        /// <returns>Data completa in cui ricade la pasqua</returns>
        public static DateTime GetPasqua ( int year )
        {
            int month = 0;
            int day = 0;

            int g = year % 19;
            int c = year / 100;
            int h = h = ( c - ( int ) ( c / 4 ) - ( int ) ( ( 8 * c + 13 ) / 25 )
                                                + 19 * g + 15 ) % 30;
            int i = h - ( int ) ( h / 28 ) * ( 1 - ( int ) ( h / 28 ) *
                        ( int ) ( 29 / ( h + 1 ) ) * ( int ) ( ( 21 - g ) / 11 ) );

            day = i - ( ( year + ( int ) ( year / 4 ) +
                          i + 2 - c + ( int ) ( c / 4 ) ) % 7 ) + 28;
            month = 3;

            if ( day > 31 )
            {
                month++;
                day -= 31;
            }

            return new DateTime( year , month , day );
        }

        #endregion

        #region Private

        /// <summary>
        /// Verifica se le due date sono uguali prendendo escludendo dal confronto il Time
        /// </summary>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        private static bool IsDTEquals ( DateTime dt1 , DateTime dt2 )
        {
            bool result = false;

            try
            {
                if ( dt1.Date.Equals( dt2.Date ) )
                {
                    result = true;
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
            return result;
        }

        /// <summary>
        /// Verifica se in una determinata data ricade la festività pasquale
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsPasqua ( DateTime data )
        {
            bool result = false;

            try
            {
                int anno = data.Year;

                DateTime pasqua = GetPasqua( anno );

                if ( IsDTEquals( data , pasqua ) )
                {
                    result = true;
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
            return result;
        }

        /// <summary>
        /// Verifica se in una particolare data ricade la festività
        /// del Corpus Domini
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsCorpusDomini ( DateTime data )
        {
            bool result = false;

            try
            {
                int anno = data.Year;

                DateTime corpusDomini = GetCorpusDomini( anno );

                if ( IsDTEquals( data , corpusDomini ) )
                {
                    result = true;
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
            return result;
        }

        /// <summary>
        /// Verifica il giorno dell'ascensione ricade in una determinata data passata
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsAscensionDay ( DateTime data )
        {
            bool result = false;

            try
            {
                int anno = data.Year;

                DateTime ascensionDay = GetAscensionDay( anno );

                if ( IsDTEquals( data , ascensionDay ) )
                {
                    result = true;
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
            return result;
        }

        /// <summary>
        /// Giovedì dopo la sesta domenica dopo Pasqua; in alcuni Paesi, la domenica successiva
        /// </summary>
        /// <param name="anno"></param>
        /// <returns></returns>
        private static DateTime GetAscensionDay ( int anno )
        {
            return GetPasqua( anno ).AddDays( 39 );
        }

        /// <summary>
        /// Metodo che restituisce la data in cui ricade il Corpus Domini.
        /// Il Corpus Domini ricorre il Giovedì successivo alla SS Trinità (Prima Domenica dopo la Pentecoste)
        /// </summary>
        /// <param name="anno"></param>
        /// <returns></returns>
        private static DateTime GetCorpusDomini ( int anno )
        {
            return GetSSTrinita( anno ).AddDays( 4 );
        }

        /// <summary>
        /// Restituisce la data in cui ricade il Giorno della Pentecoste a partire dall'anno passato.
        /// La Pentecoste ricate 50 gg dopo la Pasqua
        /// </summary>
        /// <param name="anno"></param>
        /// <returns></returns>
        private static DateTime GetPentecoste ( int anno )
        {
            return GetPasqua( anno ).AddDays( 49 );
        }

        /// <summary>
        /// Restituisce la data in cui ricade il giorno della SS. Trinità.
        /// Prima domenica dopo la Pentecoste
        /// </summary>
        /// <param name="anno"></param>
        /// <returns></returns>
        private static DateTime GetSSTrinita ( int anno )
        {
            return GetPentecoste( anno ).AddDays( 7 );
        }

        /// <summary>
        /// Reperimento della data di decorrenza della festa patronale della
        /// sede gapp
        /// </summary>
        /// <returns></returns>
        private static DateTime GetPatrono ( string sede )
        {
            DateTime result = DateTime.MinValue;

            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    var row = db.L2D_SEDE_GAPP.Where( c => c.cod_sede_gapp.Equals( sede , StringComparison.InvariantCultureIgnoreCase ) ).FirstOrDefault( );

                    if ( row != null && row.Data_Patrono.HasValue )
                    {
                        result = row.Data_Patrono.Value;
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }

            return result;
        }



        private static L2D_SEDE_GAPP GetSedeGappDetails ( string sede )
        {
            L2D_SEDE_GAPP result = null;

            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    var row = db.L2D_SEDE_GAPP.Where( c => c.cod_sede_gapp.Equals( sede , StringComparison.InvariantCultureIgnoreCase ) ).FirstOrDefault( );

                    if ( row != null )
                    {
                        result = new L2D_SEDE_GAPP( );
                        result = row;
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }

            return result;
        }
        #endregion
    }
}
