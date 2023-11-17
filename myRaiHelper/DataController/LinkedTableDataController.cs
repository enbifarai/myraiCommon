using System;
using System.Collections.Generic;
using System.Linq;
using myRai.Data.CurriculumVitae;

namespace myRaiHelper
{
    public class RistoranteConvenzionato
    {
        public string Codice { get; set; }
        public string Nominativo { get; set; }
        public string Indirizzo { get; set; }
        public string Citta { get; set; }
        public string Cap { get; set; }
        public string Provincia { get; set; }
        public string Codice_fiscale { get; set; }
        public string Partita_iva { get; set; }
        public DateTime? Data_inizio_conv { get; set; }
        public DateTime? Data_fine_conv { get; set; }
        public string Protocollo_sap { get; set; }
        public string Note { get; set; }
        public string Sede_regionale { get; set; }
    }
    public class RepartoLinkedServer
    {
        public string cod_reparto { get; set; }
        public string reparto { get; set; }
        public string Descr_Reparto { get; set; }
    }

    public class LinkedTableDataController
    {
        /// <summary>
        /// Reperimento del ristorante convenzionato a partire dal codice
        /// </summary>
        /// <param name="codice"></param>
        /// <returns></returns>
        public RistoranteConvenzionato GetRistoranteConvenzionato(string codice)
        {
            RistoranteConvenzionato result = new RistoranteConvenzionato();
            try
            {
                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [codice] as Codice, " +
                                  "[nominativo] as Nominativo, " +
                                  "[indirizzo] as Indirizzo, " +
                                  "[citta] as Citta, " +
                                  "[cap] as Cap, " +
                                  "[provincia] as Provincia, " +
                                  "[codice_fiscale] as Codice_fiscale, " +
                                  "[partita_iva] as Partita_iva, " +
                                  "[data_inizio_conv] as Data_inizio_conv," +
                                  "[data_fine_conv] as Data_fine_conv, " +
                                  "[protocollo_sap] as Protocollo_sap, " +
                                  "[note] as Note " +
                                  "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_DEC_GAPP_RIST_CONV] " +
                                  "WHERE [codice] = '##CODICE##' " +
                                  "ORDER BY Nominativo";

                    query = query.Replace("##CODICE##", codice.Replace(".", ",").PadLeft(7, '0'));

                    result = db.Database.SqlQuery<RistoranteConvenzionato>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Reperimento dei ristoranti convenzionati a partire dal codice del Calendario Sede
        /// </summary>
        /// <param name="codice">Codice del calendario sede</param>
        /// <returns></returns>
        public List<RistoranteConvenzionato> GetRistorantiConvenzionati(string codice)
        {
            List<RistoranteConvenzionato> result = new List<RistoranteConvenzionato>();
            try
            {
                using (var db = new cv_ModelEntities())
                {
                    if (codice.Equals("ROMA", StringComparison.InvariantCultureIgnoreCase))
                    {
                        codice = "RM";
                    }

                    string query = "SELECT [codice] as Codice, " +
                                  "[nominativo] as Nominativo, " +
                                  "[indirizzo] as Indirizzo, " +
                                  "[citta] as Citta, " +
                                  "[cap] as Cap, " +
                                  "[provincia] as Provincia, " +
                                  "[codice_fiscale] as Codice_fiscale, " +
                                  "[partita_iva] as Partita_iva, " +
                                  "[data_inizio_conv] as Data_inizio_conv," +
                                  "[data_fine_conv] as Data_fine_conv, " +
                                  "[protocollo_sap] as Protocollo_sap, " +
                                  "[note] as Note " +
                                  "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_DEC_GAPP_RIST_CONV] " +
                                  "WHERE [provincia] = '##CODICE##' " +
                                  "AND LEFT([nominativo], 2) <> '**' " +
                                  "ORDER BY Nominativo";

                    query = query.Replace("##CODICE##", codice);

                    result = db.Database.SqlQuery<RistoranteConvenzionato>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }
        public RepartoLinkedServer GetDettagliReparto(string sedeGapp, string reparto, string matricola=null)
        {
            if (String.IsNullOrWhiteSpace(sedeGapp) || sedeGapp.Length < 2) return null;

            try
            {
                var db = new cv_ModelEntities();
                string cod_reparto = sedeGapp.Substring(0, 2);
                string query = "select * FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_DEC_GAPP_Reparto] " +
                    " where cod_reparto='" + cod_reparto + "' and reparto='" + reparto + "'";

                RepartoLinkedServer result = db.Database.SqlQuery<RepartoLinkedServer>(query).FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                if (String.IsNullOrWhiteSpace(matricola))
                    matricola = CommonHelper.GetCurrentUserMatricola();

                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola ,
                    provenienza = "GetDettagliReparto"
                });
                return null;
            }
        }

        public Dictionary<string , string> GetDettagliReparti ( string sedeGapp , string matricola )
        {
            Dictionary<string , string> output = new Dictionary<string , string>( );

            if ( String.IsNullOrWhiteSpace( sedeGapp ) || sedeGapp.Length < 2 )
                return null;

            try
            {
                var db = new cv_ModelEntities( );
                string cod_reparto = sedeGapp.Substring( 0 , 2 );
                string query = "select * FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_DEC_GAPP_Reparto] " +
                    " where cod_reparto='" + cod_reparto + "'";

                var result = db.Database.SqlQuery<RepartoLinkedServer>( query );
                foreach ( var reparto in result )
                {
                    output.Add( reparto.reparto , reparto.Descr_Reparto );
                }
                if ( !output.ContainsKey( "00" ) )
                    output.Add( "00" , "REPARTO 00" );

            }
            catch ( Exception ex )
            {
                Logger.LogErrori( new myRaiData.MyRai_LogErrori( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    error_message = ex.ToString( ) ,
                    matricola = matricola ,
                    provenienza = "GetDettagliReparto"
                } );
                return null;
            }

            return output;
        }
    }
}