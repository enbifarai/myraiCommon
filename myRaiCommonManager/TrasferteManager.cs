using System;
using System.Collections.Generic;
using System.Linq;
using myRai.Data.CurriculumVitae;
using myRaiHelper;
using myRaiCommonModel.ess;
using MyRaiServiceInterface.MyRaiServiceReference1;
using myRaiData;
using System.Globalization;


namespace myRaiCommonManager
{
    public class TrasferteManager
    {
        /// <summary>
        /// Calcola il rimborso per tutti gli elementi nell'array 
        /// aventi nel campo rimborso il valore 0, se esiste la relativa nota spese
        /// </summary>
        /// <param name="viaggi"></param>
        public static List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> CalcolaRimborso(List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> viaggi)
        {
            try
            {
                if (viaggi != null && viaggi.Any())
                {
                    string elencoFogli = "'" + String.Join("','", viaggi.Select(x => x.FoglioViaggio)) + "'";
                    List<DiariaTrasferta> diarie = GetDiarie(elencoFogli);
                    List<NotaSpeseTrasferta> noteSpese = GetNoteSpeseTrasfertaMulti(elencoFogli);

                    foreach (var v in viaggi)
                    {
                        try
                        {
                            if (v.Rimborso <= 0.0)
                            {
                                List<DiariaTrasferta> diaria = diarie.Where(x => x.NUM_FOG == v.FoglioViaggio).ToList();

                                NotaSpeseTrasferta nota = noteSpese.FirstOrDefault(x => x.NUM_FOG == v.FoglioViaggio);

                                decimal altreSpese = 0;

                                if (nota != null)
                                {
                                    altreSpese = nota.TOT_ALTRE_SPESE_DOC;
                                }

                                v.Rimborso = (double)(TotalPDL(diaria) + TotalForfait(diaria) + altreSpese);
                            }
                        }
                        catch (Exception ex)
                        {
                            v.Rimborso = 0.0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - CalcolaRimborso"
                });
            }

            return viaggi;
        }

        public static decimal CalcolaResiduoNetto(string foglioViaggio)
        {
            decimal result = 0;
            try
            {
                List<DiariaTrasferta> diaria = GetDiaria(foglioViaggio);

                NotaSpeseTrasferta nota = GetNotaSpeseTrasferta(foglioViaggio);

                decimal altreSpese = 0;

                if (nota != null)
                {
                    // spese viaggio doc
                    altreSpese = nota.TOT_TAXI_BUS + nota.TOT_PEDAGGI + nota.TOT_CARBUR + nota.TOT_NOLO + nota.TOT_BIGLIETTI_DOC_NO_RAI;

                    // spesa per conto RAI
                    altreSpese += nota.TOT_PER_CONTO;

                    // voce altre spese
                    altreSpese += nota.TOT_ALTRE_SPESE_DOC + nota.TOT_ALTRE_SPESE_NO_DOC;
                }

                result = TotalPDL(diaria) + TotalForfait(diaria) + altreSpese;

            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - CalcolaResiduoNetto"
                });

                result = 0;
            }

            return result;
        }

        public static decimal TotalPDL(List<DiariaTrasferta> diaria)
        {
            try
            {
                decimal col = diaria.Sum(d => d.COLAZ_PDL);
                decimal pasti = diaria.Sum(d => d.PASTO_PDL_1) + diaria.Sum(d => d.PASTO_PDL_2);
                decimal pernottamenti = diaria.Sum(d => d.PERNOTTO_PDL);

                return (col + pasti + pernottamenti);
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - TotalPDL"
                });
                return 0;
            }
        }

        public static decimal TotalForfait(List<DiariaTrasferta> diaria)
        {
            try
            {
                decimal col = diaria.Sum(d => d.COLAZ_FOR);
                decimal pasti = diaria.Sum(d => d.PASTO_FOR_1) + diaria.Sum(d => d.PASTO_FOR_2);
                decimal pernottamenti = diaria.Sum(d => d.PERNOTTO_FOR);
                decimal spese = diaria.Sum(d => d.PICCOLE_SPESE);
                decimal indennita = diaria.Sum(d => d.INDENNITA_TRASFERTA);

                return (col + pasti + pernottamenti + spese + indennita);
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - TotalForfait"
                });
                return 0;
            }
        }

        /// <summary>
        /// Reperimento dell'elenco delle trasferte dal db
        /// </summary>
        /// <returns></returns>
        public static List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> GetTrasferteFromDB(string matricola = null)
        {
            List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> viaggi = null;
            try
            {
                DateTime curr = DateTime.Now;
                curr = curr.AddYears(-1);
                int anno = curr.Year;

                if (matricola == null)
                {
                    matricola = CommonHelper.GetCurrentUserMatricola();
                }

                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [NUM_FOG] as FoglioViaggio									" +
                                    ",[DATA_PARTENZA] as DataFromDB										" +
                                    ",[DATA_ARRIVO] as DataArrivoFromDB   							" +
                                    ",[SCOPO] as Descrizione											" +
                                    ",[ANTICIPI] as AnticipoFromDB										" +
                                    ",0.0 as RimborsoFromDB												" +
                                    ",[ITINERARIO] as Note	     										" +
                                    ",[STATO] as STATO													" +
                                    ",[SPESE_PREV] as SpesaPrevistaFromDB								" +
                                    ",[AUTORIZZATA_DA] as AutorizzatoDa									" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_FOGLIO_VIAGGIO]	" +
                                    "WHERE matricola_dp = '##MATRICOLA##'								" +
                                    "AND YEAR(data_elaborazione) >= '##ANNO##'									" +
                                    "ORDER BY [NUM_FOG] ASC ";

                    query = query.Replace("##MATRICOLA##", matricola);
                    query = query.Replace("##ANNO##", anno.ToString());
                    viaggi = db.Database.SqlQuery<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>(query).ToList();

                    if (viaggi != null && viaggi.Any())
                    {
                        viaggi.ForEach(v =>
                       {
                           v.Data = v.DataFromDB.ToString("dd/MM/yyyy");
                           v.Anticipo = (double)v.AnticipoFromDB;
                           v.Rimborso = (double)v.RimborsoFromDB;
                           v.SpesaPrevista = (double)v.SpesaPrevistaFromDB;
                       });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetTrasferteFromDB"
                });
                viaggi = null;
            }

            return viaggi;
        }

        public static List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> GetTrasferteFromDB(DateTime dt1, DateTime dt2, string foglioViaggio, string scopo, string matricola = null)
        {
            List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> viaggi = null;
            try
            {
                if (!String.IsNullOrWhiteSpace(matricola))
                    matricola = UtenteHelper.EsponiAnagrafica()._matricola;

                using (var db = new cv_ModelEntities())
                {
                    string query = "";

                    if (dt1.Equals(DateTime.MinValue))
                    {
                        // La data minima accettata da sql server è : 1753 - 01 - 01 00:00:00.000
                        dt1 = new DateTime(1753, 1, 1, 0, 0, 0);
                    }

                    if (!String.IsNullOrEmpty(foglioViaggio))
                    {
                        query = "SELECT [NUM_FOG] as FoglioViaggio									" +
                                    ",[DATA_PARTENZA] as DataFromDB										" +
                                    ",[DATA_ARRIVO] as DataArrivoFromDB      							" +
                                    ",[SCOPO] as Descrizione											" +
                                    ",[ANTICIPI] as AnticipoFromDB										" +
                                    ",0.0 as RimborsoFromDB												" +
                                    ",'' as Note														" +
                                    ",[STATO] as STATO													" +
                                    ",[SPESE_PREV] as SpesaPrevistaFromDB								" +
                                    ",[AUTORIZZATA_DA] as AutorizzatoDa									" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_FOGLIO_VIAGGIO]	" +
                                    "WHERE matricola_dp = '##MATRICOLA##'								" +
                                    "AND NUM_FOG = '##NUM_FOG##'                                        ";
                    }
                    else
                    {
                        query = "SELECT [NUM_FOG] as FoglioViaggio									" +
                                ",[DATA_PARTENZA] as DataFromDB										" +
                                ",[DATA_ARRIVO] as DataArrivoFromDB      							" +
                                ",[SCOPO] as Descrizione											" +
                                ",[ANTICIPI] as AnticipoFromDB										" +
                                ",0.0 as RimborsoFromDB												" +
                                ",'' as Note														" +
                                ",[STATO] as STATO													" +
                                ",[SPESE_PREV] as SpesaPrevistaFromDB								" +
                                ",[AUTORIZZATA_DA] as AutorizzatoDa									" +
                                "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_FOGLIO_VIAGGIO]	" +
                                "WHERE matricola_dp = '##MATRICOLA##'								" +
                                "AND DATA_PARTENZA >= '##DATAPARTENZA##'                            " +
                                "AND DATA_ARRIVO <= '##DATAARRIVO##' ";

                        if (!String.IsNullOrEmpty(scopo))
                        {
                            query = query + " AND SCOPO LIKE '%##SCOPO##%' ";
                        }
                    }

                    query = query.Replace("##MATRICOLA##", matricola);
                    query = query.Replace("##DATAPARTENZA##", dt1.ToString("yyyy-MM-dd 00:00:00.000"));
                    query = query.Replace("##DATAARRIVO##", dt2.ToString("yyyy-MM-dd 00:00:00.000"));
                    query = query.Replace("##NUM_FOG##", foglioViaggio);
                    query = query.Replace("##SCOPO##", scopo);
                    viaggi = db.Database.SqlQuery<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>(query).ToList();

                    if (viaggi != null && viaggi.Any())
                    {
                        viaggi.ForEach(v =>
                        {
                            v.Data = v.DataFromDB.ToString("dd/MM/yyyy");
                            v.Anticipo = (double)v.AnticipoFromDB;
                            v.Rimborso = (double)v.RimborsoFromDB;
                            v.SpesaPrevista = (double)v.SpesaPrevistaFromDB;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetTrasferteFromDB"
                });
                viaggi = null;
            }

            return viaggi;
        }

        public static List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> GetTrasferteFromDBAnag(DateTime dt1, DateTime dt2, string foglioViaggio, string scopo, string matricola = null)
        {
            List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> viaggi = null;
            try
            {
                if (!String.IsNullOrWhiteSpace(matricola))
                    matricola = UtenteHelper.EsponiAnagrafica()._matricola;

                using (var db = new cv_ModelEntities())
                {
                    string query = "";

                    if (dt1.Equals(DateTime.MinValue))
                    {
                        // La data minima accettata da sql server è : 1753 - 01 - 01 00:00:00.000
                        dt1 = new DateTime(1753, 1, 1, 0, 0, 0);
                    }

                    if (!String.IsNullOrEmpty(foglioViaggio))
                    {
                        query = "SELECT [NUM_FOG] as FoglioViaggio									" +
                                    ",[DATA_PARTENZA] as DataFromDB										" +
                                    ",[DATA_ARRIVO] as DataArrivoFromDB      							" +
                                    ",[SCOPO] as Descrizione											" +
                                    ",[ANTICIPI] as AnticipoFromDB										" +
                                    ",0.0 as RimborsoFromDB												" +
                                    ",'' as Note														" +
                                    ",[STATO] as STATO													" +
                                    ",[SPESE_PREV] as SpesaPrevistaFromDB								" +
                                    ",[AUTORIZZATA_DA] as AutorizzatoDa									" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_FOGLIO_VIAGGIO]	" +
                                    "WHERE matricola_dp = '##MATRICOLA##'								" +
                                    "AND NUM_FOG = '##NUM_FOG##'                                        ";
                    }
                    else
                    {
                        query = "SELECT [NUM_FOG] as FoglioViaggio									" +
                                ",[DATA_PARTENZA] as DataFromDB										" +
                                ",[DATA_ARRIVO] as DataArrivoFromDB      							" +
                                ",[SCOPO] as Descrizione											" +
                                ",[ANTICIPI] as AnticipoFromDB										" +
                                ",0.0 as RimborsoFromDB												" +
                                ",'' as Note														" +
                                ",[STATO] as STATO													" +
                                ",[SPESE_PREV] as SpesaPrevistaFromDB								" +
                                ",[AUTORIZZATA_DA] as AutorizzatoDa									" +
                                "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_FOGLIO_VIAGGIO]	" +
                                "WHERE matricola_dp = '##MATRICOLA##'								" +
                                "AND DATA_PARTENZA <= '##DATAARRIVO##'                            " +
                                "AND '##DATAPARTENZA##' <= DATA_ARRIVO ";

                        if (!String.IsNullOrEmpty(scopo))
                        {
                            query = query + " AND SCOPO LIKE '%##SCOPO##%' ";
                        }
                    }

                    query = query.Replace("##MATRICOLA##", matricola);
                    query = query.Replace("##DATAPARTENZA##", dt1.ToString("yyyy-MM-dd 00:00:00.000"));
                    query = query.Replace("##DATAARRIVO##", dt2.ToString("yyyy-MM-dd 00:00:00.000"));
                    query = query.Replace("##NUM_FOG##", foglioViaggio);
                    query = query.Replace("##SCOPO##", scopo);
                    viaggi = db.Database.SqlQuery<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>(query).ToList();

                    if (viaggi != null && viaggi.Any())
                    {
                        viaggi.ForEach(v =>
                        {
                            v.Data = v.DataFromDB.ToString("dd/MM/yyyy");
                            v.Anticipo = (double)v.AnticipoFromDB;
                            v.Rimborso = (double)v.RimborsoFromDB;
                            v.SpesaPrevista = (double)v.SpesaPrevistaFromDB;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetTrasferteFromDB"
                });
                viaggi = null;
            }

            return viaggi;
        }


        /// <summary>
        /// Reperimento dei dati di un foglio viaggio
        /// </summary>
        /// <param name="foglioViaggio"></param>
        /// <returns></returns>
        public static FoglioViaggio GetFoglioViaggio(string foglioViaggio, string matricola = null)
        {
            //foglioViaggio = "3872236";
            FoglioViaggio f = null;
            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    matricola = CommonHelper.GetCurrentUserMatricola();
                }

                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [NUM_FOG],														" +
                                    "[MATRICOLA_DP],														" +
                                    "[NOME],																" +
                                    "[COD_SEDE],															" +
                                    "[COD_SERV_CONT],														" +
                                    "[COD_CATEGORIA],														" +
                                    "[COD_SEZIONE],															" +
                                    "[DATA_PARTENZA],														" +
                                    "[DATA_ARRIVO],															" +
                                    "[COD_UORG],															" +
                                    "[MATRIC_SPETTACOLO],													" +
                                    "[COD_DF],																" +
                                    "[ITINERARIO],															" +
                                    "[SCOPO],																" +
                                    "[NUM_GIORNI],															" +
                                    "[ANTICIPI],															" +
                                    "[STATO],																" +
                                    "[COD_MANSIONE],														" +
                                    "[SPESE_PREV],															" +
                                    "[COD_UORG_EMISSIONE],													" +
                                    "[ESTERO],																" +
                                    "[FLG_ISUD],															" +
                                    "[COD_GRANDI_EVENTI],													" +
                                    "[DATA_ELABORAZIONE],													" +
                                    "[MESE_LIQUIDAZIONE],													" +
                                    "[CODICE_UTENTE],														" +
                                    "[DATA_NOTA_SPESE],														" +
                                    "[AUTORIZZATA_DA]														" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_FOGLIO_VIAGGIO]		" +
                                    "WHERE [NUM_FOG] = '##NUMEROFOGLIO##'									";

                    //query = query.Replace( "##MATRICOLA##", matricola );
                    query = query.Replace("##NUMEROFOGLIO##", foglioViaggio);
                    f = new FoglioViaggio();
                    f = db.Database.SqlQuery<FoglioViaggio>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetFoglioViaggio"
                });
                f = null;
            }

            return f;
        }
        public static List<FoglioViaggio> GetFogliViaggioMulti(string foglioViaggio, string matricola = null)
        {
            //foglioViaggio = "3872236";
            List<FoglioViaggio> f = null;
            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    matricola = CommonHelper.GetCurrentUserMatricola();
                }

                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [NUM_FOG],														" +
                                    "[MATRICOLA_DP],														" +
                                    "[NOME],																" +
                                    "[COD_SEDE],															" +
                                    "[COD_SERV_CONT],														" +
                                    "[COD_CATEGORIA],														" +
                                    "[COD_SEZIONE],															" +
                                    "[DATA_PARTENZA],														" +
                                    "[DATA_ARRIVO],															" +
                                    "[COD_UORG],															" +
                                    "[MATRIC_SPETTACOLO],													" +
                                    "[COD_DF],																" +
                                    "[ITINERARIO],															" +
                                    "[SCOPO],																" +
                                    "[NUM_GIORNI],															" +
                                    "[ANTICIPI],															" +
                                    "[STATO],																" +
                                    "[COD_MANSIONE],														" +
                                    "[SPESE_PREV],															" +
                                    "[COD_UORG_EMISSIONE],													" +
                                    "[ESTERO],																" +
                                    "[FLG_ISUD],															" +
                                    "[COD_GRANDI_EVENTI],													" +
                                    "[DATA_ELABORAZIONE],													" +
                                    "[MESE_LIQUIDAZIONE],													" +
                                    "[CODICE_UTENTE],														" +
                                    "[DATA_NOTA_SPESE],														" +
                                    "[AUTORIZZATA_DA]														" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_FOGLIO_VIAGGIO]		" +
                                    "WHERE [NUM_FOG] IN (##NUMEROFOGLIO##)									";

                    //query = query.Replace( "##MATRICOLA##", matricola );
                    query = query.Replace("##NUMEROFOGLIO##", foglioViaggio);
                    f = db.Database.SqlQuery<FoglioViaggio>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetFoglioViaggio"
                });
                f = null;
            }

            return f;
        }

        public static StatoTrasferta GetStatoTrasferta(string codiceStato)
        {
            StatoTrasferta s = null;
            try
            {
                string matricola = UtenteHelper.EsponiAnagrafica()._matricola;

                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [cod_stato],												" +
                                    "[desc_stato]													" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_DEC_TRAS_STATO]	" +
                                    "WHERE [cod_stato] = '##STATO##'								";

                    query = query.Replace("##STATO##", codiceStato);
                    s = new StatoTrasferta();
                    s = db.Database.SqlQuery<StatoTrasferta>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetStatoTrasferta"
                });
                s = null;
            }

            return s;
        }

        public static List<StatoTrasferta> GetStatiTrasferta()
        {
            List<StatoTrasferta> s = null;
            try
            {
                string matricola = myRai.Models.Utente.EsponiAnagrafica()._matricola;

                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [cod_stato],												" +
                                    "[desc_stato]													" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_DEC_TRAS_STATO]	";

                    s = new List<StatoTrasferta>();
                    s.AddRange(db.Database.SqlQuery<StatoTrasferta>(query));
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetStatoTrasferta"
                });
                s = null;
            }

            return s;
        }

        public static GrandeEvento GetGrandeEvento(string codiceEvento)
        {
            GrandeEvento e = null;
            try
            {
                string matricola = UtenteHelper.EsponiAnagrafica()._matricola;

                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [cod_grandi_eventi],												" +
                                    "[desc_grandi_eventi],													" +
                                    "[data_inizio_manifestazione],											" +
                                    "[data_fine_manifestazione],											" +
                                    //"[testo]																" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_DEC_TRAS_GRANDI_EVENTI]	" +
                                    "WHERE [cod_grandi_eventi] = '##EVENTO##'								";

                    query = query.Replace("##EVENTO##", codiceEvento);
                    e = new GrandeEvento();
                    e = db.Database.SqlQuery<GrandeEvento>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetGrandeEvento"
                });
                e = null;
            }

            return e;
        }

        public static List<ItinerarioTrasferta> GetItinerario(string foglioViaggio, string matricola = null)
        {
            List<ItinerarioTrasferta> _itinerario = null;
            List<ItinerarioTrasferta> itinerario = new List<ItinerarioTrasferta>();
            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    matricola = CommonHelper.GetCurrentUserMatricola();
                }

                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [NUM_FOG], 												 " +
                                    " [NUM_TRAS],													 " +
                                    " [NUM_RIGA],													 " +
                                    " [DESTINAZIONE],												 " +
                                    " [PROVINCIA],													 " +
                                    " [CAP],														 " +
                                    " [DATA_ARRIVO],												 " +
                                    " [COD_MEZZO_TRASP],											 " +
                                    " [IMPORTO_BIGLIETTI_DIP],										 " +
                                    " [IMPORTO_BIGLIETTI_RAI],										 " +
                                    " [FLG_ISUD],													 " +
                                    " [DATA_ELABORAZIONE]											 " +
                                    " FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_ITINERARIO]  " +
                                    " WHERE NUM_FOG = '##FOGLIO##'									 " +
                                    " ORDER BY [NUM_TRAS] ASC, [NUM_RIGA] ASC						 ";

                    query = query.Replace("##FOGLIO##", foglioViaggio);
                    _itinerario = new List<ItinerarioTrasferta>();
                    _itinerario = db.Database.SqlQuery<ItinerarioTrasferta>(query).ToList();
                }

                if (_itinerario != null && _itinerario.Any())
                {
                    int posizione = 1;
                    foreach (ItinerarioTrasferta i in _itinerario)
                    {
                        if (i.COD_MEZZO_TRASP != "AS" && i.COD_MEZZO_TRASP != "AU")
                        {
                            itinerario.Add(GetItinerarioExt(matricola, foglioViaggio, posizione.ToString(), i.DATA_ARRIVO, i));
                            posizione++;
                        }
                    }

                    if (itinerario == null || !itinerario.Any())
                    {
                        itinerario = new List<ItinerarioTrasferta>();

                        itinerario.AddRange(GetItinerarioFromImportedData(matricola, foglioViaggio).Distinct().ToList());
                    }
                }
                else
                {
                    itinerario = new List<ItinerarioTrasferta>();

                    itinerario.AddRange(GetItinerarioFromImportedData(matricola, foglioViaggio).Distinct().ToList());
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetItinerario"
                });
                itinerario = null;
            }

            return itinerario;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="foglioViaggio"></param>
        /// <param name="dataArrivo"></param>
        /// <param name="dataPartenza"></param>
        /// <returns></returns>
        public static ItinerarioTrasferta GetItinerarioExt(string matricola, string foglioViaggio, string posizione, DateTime dataArrivo, ItinerarioTrasferta it)
        {
            ItinerarioTrasferta result = new ItinerarioTrasferta();
            result = it;
            try
            {

                matricola = matricola.PadLeft(8, '0');
                foglioViaggio = foglioViaggio.PadLeft(10, '0');
                string _dataArrivo = dataArrivo.ToString("ddMMyyyy");

                using (digiGappEntities db = new digiGappEntities())
                {
                    var item = db.MyRai_Trasferte_ProgViaggio.Where(w => w.PROCID.Equals(matricola) &&
                                                                    w.PRONUMPRAT.Equals(foglioViaggio) &&
                                                                    w.PRODNUMPOS.Equals(posizione) &&
                                                                    w.PRODDATAOUT.Equals(_dataArrivo)
                                                                    ).ToList();

                    if (item != null && item.Any())
                    {
                        var itm = item.OrderBy(o1 => o1.PRODNUMPOS).OrderBy(o2 => o2.PRODNUMVERS).FirstOrDefault();

                        result.CodicePrenotazione = itm.PRODCODPREN;
                        result.Origine = itm.PRODORIGINE;
                        result.DestCitta = itm.PRODDESTCITTA;
                        result.CodiceMezzo = itm.PRODVOLONUM;

                        try
                        {
                            string data1 = itm.PRODDATAIN;
                            string ora1 = itm.PRODORAIN;

                            int gg = int.Parse(data1.Substring(0, 2));
                            int mm = int.Parse(data1.Substring(2, 2));
                            int yyyy = int.Parse(data1.Substring(4, 4));

                            int hh = int.Parse(ora1.Substring(0, 2));
                            int min = int.Parse(ora1.Substring(2, 2));
                            int sec = int.Parse(ora1.Substring(4, 2));

                            result.DataPartenza = new DateTime(yyyy, mm, gg, hh, min, sec);

                            string data2 = itm.PRODDATAOUT;
                            string ora2 = itm.PRODORAOUT;

                            gg = int.Parse(data2.Substring(0, 2));
                            mm = int.Parse(data2.Substring(2, 2));
                            yyyy = int.Parse(data2.Substring(4, 4));

                            hh = int.Parse(ora2.Substring(0, 2));
                            min = int.Parse(ora2.Substring(2, 2));
                            sec = int.Parse(ora2.Substring(4, 2));

                            result.DataArrivoFull = new DateTime(yyyy, mm, gg, hh, min, sec);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetItinerarioExt"
                });
                result = new ItinerarioTrasferta();
            }

            return result;
        }

        public static NotaSpeseTrasferta GetNotaSpeseTrasferta(string foglioViaggio)
        {
            //foglioViaggio = "3872236";
            NotaSpeseTrasferta nota = new NotaSpeseTrasferta();
            try
            {
                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [NUM_FOG],												" +
                                    "[DATA_INIZIO],													" +
                                    "[DATA_FINE],													" +
                                    "[TOT_BIGLIETTI_DOC_NO_RAI],									" +
                                    "[TOT_BIGLIETTI_DOC_RAI],										" +
                                    "[TOT_TAXI_BUS],												" +
                                    "[TOT_PEDAGGI],													" +
                                    "[TOT_ALTRE_SPESE_DOC],											" +
                                    "[TOT_CARBUR],													" +
                                    "[TOT_PER_CONTO],												" +
                                    "[TOT_BIGLIETTI_SMARRITI],										" +
                                    "[TOT_ALTRE_SPESE_NO_DOC],										" +
                                    "[TOT_NOLO],													" +
                                    "[KM_AUTO],														" +
                                    "[TOT_FORFAIT],													" +
                                    "[TOT_PDL],														" +
                                    "[NUM_GIORNI],													" +
                                    "[FLG_ISUD],													" +
                                    "[TOT_COSTO_AZIENDALE],											" +
                                    "[DATA_ELABORAZIONE]											" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_NOTA_SPESE]	" +
                                    "WHERE NUM_FOG = '##FOGLIO##'									";

                    query = query.Replace("##FOGLIO##", foglioViaggio);
                    nota = new NotaSpeseTrasferta();
                    nota = db.Database.SqlQuery<NotaSpeseTrasferta>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetNotaSpeseTrasferta"
                });
                nota = new NotaSpeseTrasferta();
            }

            return nota;
        }

        public static List<NotaSpeseTrasferta> GetNoteSpeseTrasfertaMulti(string foglioViaggio)
        {
            //foglioViaggio = "3872236";
            List<NotaSpeseTrasferta> note = new List<NotaSpeseTrasferta>();
            try
            {
                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [NUM_FOG],												" +
                                    "[DATA_INIZIO],													" +
                                    "[DATA_FINE],													" +
                                    "[TOT_BIGLIETTI_DOC_NO_RAI],									" +
                                    "[TOT_BIGLIETTI_DOC_RAI],										" +
                                    "[TOT_TAXI_BUS],												" +
                                    "[TOT_PEDAGGI],													" +
                                    "[TOT_ALTRE_SPESE_DOC],											" +
                                    "[TOT_CARBUR],													" +
                                    "[TOT_PER_CONTO],												" +
                                    "[TOT_BIGLIETTI_SMARRITI],										" +
                                    "[TOT_ALTRE_SPESE_NO_DOC],										" +
                                    "[TOT_NOLO],													" +
                                    "[KM_AUTO],														" +
                                    "[TOT_FORFAIT],													" +
                                    "[TOT_PDL],														" +
                                    "[NUM_GIORNI],													" +
                                    "[FLG_ISUD],													" +
                                    "[TOT_COSTO_AZIENDALE],											" +
                                    "[DATA_ELABORAZIONE]											" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_NOTA_SPESE]	" +
                                    "WHERE NUM_FOG IN (##FOGLIO##)									";

                    query = query.Replace("##FOGLIO##", foglioViaggio);
                    note = db.Database.SqlQuery<NotaSpeseTrasferta>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetNotaSpeseTrasferta"
                });
                note = new List<NotaSpeseTrasferta>();
            }

            return note;
        }

        public static List<AlbergoTrasferta> GetAlberghi(string foglioViaggio)
        {
            //foglioViaggio = "3872236";
            List<AlbergoTrasferta> alberghi = null;
            try
            {
                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [foglio_viaggio],											" +
                                    "[progressivo],														" +
                                    "[stato],															" +
                                    "[gg_prenotati],													" +
                                    "[data],															" +
                                    "[costo_effettivo],													" +
                                    "[tipo],															" +
                                    "[costo_teorico],													" +
                                    "[utente],															" +
                                    "[data_log],														" +
                                    "[utente_canc],														" +
                                    "[data_canc],														" +
                                    "[citta],															" +
                                    "[denominazione],													" +
                                    "[data_decorrenza],													" +
                                    "[multi],															" +
                                    "[prov],															" +
                                    "[telefono],														" +
                                    "[fax],																" +
                                    "[categ],															" +
                                    "[costo_giornaliero],												" +
                                    "[costo_dus],														" +
                                    "[cod_albergo],														" +
                                    "[indirizzo],														" +
                                    "[cap],																" +
                                    "[nom_rich],														" +
                                    "[tel_rich],														" +
                                    "[automatico],														" +
                                    "[autoriz],															" +
                                    "[FLG_ISUD],														" +
                                    "[DATA_ELABORAZIONE]												" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_ALBERGHI]		" +
                                    "WHERE foglio_viaggio = '##FOGLIO##'								" +
                                    "ORDER BY progressivo ASC											";

                    query = query.Replace("##FOGLIO##", foglioViaggio);
                    alberghi = new List<AlbergoTrasferta>();
                    alberghi = db.Database.SqlQuery<AlbergoTrasferta>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetAlberghi"
                });

                alberghi = null;
            }

            return alberghi;
        }

        public static List<BigliettoRaiTrasferta> GetBigliettoRai(string foglioViaggio)
        {
            //foglioViaggio = "3872236";
            List<BigliettoRaiTrasferta> biglietti = null;
            try
            {
                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [Numero_foglio_viaggio],											" +
                                    "[Numero_biglietto],													" +
                                    "[Data_partenza_viaggio],												" +
                                    "[Progressivo],															" +
                                    "[Numero_foglio_viaggio_new],											" +
                                    "[Matricola_DP],														" +
                                    "[Itinerario],															" +
                                    "[Data_emissione_biglietto],											" +
                                    "[Mezzo],																" +
                                    "[Codice_vettore],														" +
                                    "[Flag_estero],															" +
                                    "[Codice_tariffa],														" +
                                    "[Data_fattura],														" +
                                    "[Numero_fattura],														" +
                                    "[Importo_fattura],														" +
                                    "[Importo_rimborso],													" +
                                    "[Data_acquisizione],													" +
                                    "[Data_elaborazione]													" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_BIGLIETTI_RAI]		" +
                                    "WHERE [Numero_foglio_viaggio] = '##FOGLIO##'							" +
                                    "ORDER BY Numero_biglietto ASC											";

                    query = query.Replace("##FOGLIO##", foglioViaggio);
                    biglietti = new List<BigliettoRaiTrasferta>();
                    biglietti = db.Database.SqlQuery<BigliettoRaiTrasferta>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetBigliettoRai"
                });

                biglietti = null;
            }

            return biglietti;
        }

        public static List<DiariaTrasferta> GetDiaria(string foglioViaggio)
        {
            //foglioViaggio = "3872236";
            List<DiariaTrasferta> diaria = null;
            try
            {
                using (var db = new cv_ModelEntities())
                {

                    string query = "SELECT [NUM_FOG]												" +
                                    ",[NUM_RIGA]													" +
                                    ",[DATA]														" +
                                    ",[PASTO_PDL_1]													" +
                                    ",[PASTO_FOR_1]													" +
                                    ",[PASTO_PDL_2]													" +
                                    ",[PASTO_FOR_2]													" +
                                    ",[PERNOTTO_PDL]												" +
                                    ",[PERNOTTO_FOR]												" +
                                    ",[COLAZ_PDL]													" +
                                    ",[COLAZ_FOR]													" +
                                    ",[PICCOLE_SPESE]												" +
                                    ",[INDENNITA_TRASFERTA]											" +
                                    ",[PERNOTTO_DESC]												" +
                                    ",[PERNOTTO_CAP]												" +
                                    ",[FLG_ISUD]													" +
                                    ",[FRUIZIONE_PRANZO]											" +
                                    ",[FRUIZIONE_CENA]												" +
                                    ",[FRUIZIONE_PERNOTTO]											" +
                                    ",[FRUIZIONE_COLAZ]												" +
                                    ",[DIRITTO_PRANZO]												" +
                                    ",[DIRITTO_CENA]												" +
                                    ",[DIRITTO_PERNOTTO]											" +
                                    ",[DIRITTO_COLAZ]												" +
                                    ",[DATA_ELABORAZIONE]											" +
                                    ",[PDL_SOGGETTO]												" +
                                    ",[FORFAIT_SOGGETTO]											" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_DIARIA]		" +
                                    "WHERE NUM_FOG = '##FOGLIO##'									" +
                                    "ORDER BY [NUM_RIGA] ASC										";

                    query = query.Replace("##FOGLIO##", foglioViaggio);
                    diaria = new List<DiariaTrasferta>();
                    diaria = db.Database.SqlQuery<DiariaTrasferta>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetDiaria"
                });
                diaria = null;
            }

            return diaria;
        }

        public static List<DiariaTrasferta> GetDiarie(string foglioViaggio)
        {
            //foglioViaggio = "3872236";
            List<DiariaTrasferta> diaria = null;
            try
            {
                using (var db = new cv_ModelEntities())
                {

                    string query = "SELECT [NUM_FOG]												" +
                                    ",[NUM_RIGA]													" +
                                    ",[DATA]														" +
                                    ",[PASTO_PDL_1]													" +
                                    ",[PASTO_FOR_1]													" +
                                    ",[PASTO_PDL_2]													" +
                                    ",[PASTO_FOR_2]													" +
                                    ",[PERNOTTO_PDL]												" +
                                    ",[PERNOTTO_FOR]												" +
                                    ",[COLAZ_PDL]													" +
                                    ",[COLAZ_FOR]													" +
                                    ",[PICCOLE_SPESE]												" +
                                    ",[INDENNITA_TRASFERTA]											" +
                                    ",[PERNOTTO_DESC]												" +
                                    ",[PERNOTTO_CAP]												" +
                                    ",[FLG_ISUD]													" +
                                    ",[FRUIZIONE_PRANZO]											" +
                                    ",[FRUIZIONE_CENA]												" +
                                    ",[FRUIZIONE_PERNOTTO]											" +
                                    ",[FRUIZIONE_COLAZ]												" +
                                    ",[DIRITTO_PRANZO]												" +
                                    ",[DIRITTO_CENA]												" +
                                    ",[DIRITTO_PERNOTTO]											" +
                                    ",[DIRITTO_COLAZ]												" +
                                    ",[DATA_ELABORAZIONE]											" +
                                    ",[PDL_SOGGETTO]												" +
                                    ",[FORFAIT_SOGGETTO]											" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_DIARIA]		" +
                                    "WHERE NUM_FOG IN (##FOGLIO##)									" +
                                    "ORDER BY [NUM_RIGA] ASC										";

                    query = query.Replace("##FOGLIO##", foglioViaggio);
                    diaria = new List<DiariaTrasferta>();
                    diaria = db.Database.SqlQuery<DiariaTrasferta>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetDiaria"
                });
                diaria = null;
            }

            return diaria;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="foglioViaggio"></param>
        /// <param name="dataArrivo"></param>
        /// <param name="dataPartenza"></param>
        /// <returns></returns>
        private static List<ItinerarioTrasferta> GetItinerarioFromImportedData(string matricola, string foglioViaggio)
        {
            List<ItinerarioTrasferta> result = new List<ItinerarioTrasferta>();

            try
            {

                matricola = matricola.PadLeft(8, '0');
                foglioViaggio = foglioViaggio.PadLeft(10, '0');

                using (digiGappEntities db = new digiGappEntities())
                {
                    var item = db.MyRai_Trasferte_ProgViaggio.Where(w => w.PROCID.Equals(matricola) &&
                                                                    w.PRONUMPRAT.Equals(foglioViaggio)
                                                                    ).ToList();


                    if (item != null && item.Any())
                    {
                        var itms = item.OrderBy(o1 => o1.PRODNUMPOS).OrderBy(o2 => o2.PRODNUMVERS).ToList();

                        int min = (from minItms in itms
                                   where !String.IsNullOrEmpty(minItms.PRODNUMPOS)
                                   select Convert.ToInt32(minItms.PRODNUMPOS)).Min();

                        int max = (from maxItms in itms
                                   where !String.IsNullOrEmpty(maxItms.PRODNUMPOS)
                                   select Convert.ToInt32(maxItms.PRODNUMPOS)).Max();

                        for (int idx = min; idx <= max; idx++)
                        {
                            string pos = idx.ToString();
                            try
                            {
                                ItinerarioTrasferta it = new ItinerarioTrasferta();
                                var itm = itms.Where(w => w.PRODNUMPOS.Equals(pos)).ToList().OrderBy(w => w.PRODNUMVERS).FirstOrDefault();

                                it.CodicePrenotazione = itm.PRODCODPREN;
                                it.Origine = itm.PRODORIGINE;
                                it.DestCitta = itm.PRODDESTCITTA;
                                it.CodiceMezzo = itm.PRODVOLONUM;

                                try
                                {
                                    string data1 = itm.PRODDATAIN;
                                    string ora1 = itm.PRODORAIN;

                                    int gg = int.Parse(data1.Substring(0, 2));
                                    int mm = int.Parse(data1.Substring(2, 2));
                                    int yyyy = int.Parse(data1.Substring(4, 4));

                                    int hh = int.Parse(ora1.Substring(0, 2));
                                    int minuti = int.Parse(ora1.Substring(2, 2));
                                    int sec = int.Parse(ora1.Substring(4, 2));

                                    it.DataPartenza = new DateTime(yyyy, mm, gg, hh, minuti, sec);

                                    string data2 = itm.PRODDATAOUT;
                                    string ora2 = itm.PRODORAOUT;

                                    gg = int.Parse(data2.Substring(0, 2));
                                    mm = int.Parse(data2.Substring(2, 2));
                                    yyyy = int.Parse(data2.Substring(4, 4));

                                    hh = int.Parse(ora2.Substring(0, 2));
                                    minuti = int.Parse(ora2.Substring(2, 2));
                                    sec = int.Parse(ora2.Substring(4, 2));

                                    it.DataArrivoFull = new DateTime(yyyy, mm, gg, hh, minuti, sec);

                                    result.Add(it);
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                            catch (Exception ex)
                            {
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetItinerarioFromImportedData"
                });
                result = new List<ItinerarioTrasferta>();
            }

            return result;
        }

        /// <summary>
        /// Reperimento delle trasferte di una determinata matricola a partire dalla data "giornataDal"
        /// </summary>
        /// <param name="matricola">matricola dell'utente per il quale cercare le trasferte</param>
        /// <param name="giornataDal">data dal quale partire per la ricerca</param>
        /// <returns>Restituisce null in caso di errore.
        /// Lista vuota se non ci sono trasferte.
        /// Lista di date se ci sono trasferte</returns>
        public static List<DettaglioTrasfertaVM> GetTrasferte(DateTime giornataDal, string matricola = null)
        {
            List<DettaglioTrasfertaVM> result = new List<DettaglioTrasfertaVM>();
            List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> viaggi = new List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>();
            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    matricola = CommonHelper.GetCurrentUserMatricola();
                }

                var pp = GetTrasferte(matricola);

                if (pp.Viaggi != null && pp.Viaggi.Any())
                {
                    pp.Viaggi.ToList().ForEach(w =>
                    {
                        if (w.DataFromDB.Date >= giornataDal.Date)
                        {
                            viaggi.Add(w);
                        }
                    });
                }

                if (viaggi != null && viaggi.Any())
                {
                    foreach (var v in viaggi)
                    {
                        string foglioViaggio = v.FoglioViaggio;
                        DettaglioTrasfertaVM dt = new DettaglioTrasfertaVM();

                        dt.FoglioViaggio = GetFoglioViaggio(foglioViaggio, matricola);
                        dt.FViaggio = foglioViaggio;

                        if (dt.FoglioViaggio != null)
                        {
                            dt.StatoTrasferta = GetStatoTrasferta(dt.FoglioViaggio.STATO);
                            dt.GrandeEvento = GetGrandeEvento(dt.FoglioViaggio.COD_GRANDI_EVENTI);
                            dt.Itinerario = GetItinerario(foglioViaggio, matricola);
                            dt.NotaSpeseTrasferta = GetNotaSpeseTrasferta(foglioViaggio);
                            dt.Alberghi = GetAlberghi(foglioViaggio);
                            dt.BigliettiRai = GetBigliettoRai(foglioViaggio);
                            dt.Diaria = GetDiaria(foglioViaggio);
                            dt.ResiduoNetto = CalcolaResiduoNetto(foglioViaggio);
                        }
                        result.Add(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetTrasferte"
                });

                result = null;
            }

            return result;
        }

        /// <summary>
        /// Reperimento delle trasferte per l'utente corrente
        /// </summary>
        /// <returns></returns>
        public static Trasferta GetTrasferte(string matricola = null)
        {
            MyRaiService1Client service = new MyRaiService1Client();

            try
            {
                Trasferta t = new Trasferta();

                if (matricola == null)
                {
                    matricola = CommonHelper.GetCurrentUserMatricola();
                }

                TrasferteResponse serviceResponse = new TrasferteResponse();

                serviceResponse = service.GetTrasferte(matricola);

                if (!serviceResponse.Esito)
                {
                    if (serviceResponse.ServiceResponse.Equals("ACK91", StringComparison.InvariantCultureIgnoreCase))
                    {
                        serviceResponse.Errore = "DATI MOMENTANEAMENTE NON DISPONIBILI";
                    }

                    serviceResponse.Esito = true;
                    serviceResponse.Trasferte = new Trasferta();
                }

                List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> lst = new List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>();

                // se ci sono viaggi, va reperita la descrizione per ognuno di esso perchè non viene
                // restituita da CICS
                if (serviceResponse.Trasferte.Viaggi != null && serviceResponse.Trasferte.Viaggi.Any())
                {
                    foreach (var v in serviceResponse.Trasferte.Viaggi)
                    {
                        var fv = GetFoglioViaggio(v.FoglioViaggio, matricola);

                        if (fv != null)
                        {
                            v.Descrizione = fv.SCOPO;
                            v.Stato = fv.STATO;
                        }
                    }

                    lst = serviceResponse.Trasferte.Viaggi.OfType<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>().ToList();
                }

                var vgg = GetTrasferteFromDB(matricola);

                if (vgg != null && vgg.Any())
                {
                    if (serviceResponse.Trasferte == null)
                        serviceResponse.Trasferte = new Trasferta();

                    var newArray = new MyRaiServiceInterface.MyRaiServiceReference1.Viaggio[vgg.Count()];

                    Array.Copy(vgg.ToArray(), newArray, vgg.Count());

                    // rimuove tutti gli elementi da vgg che sono già contenuti in lst
                    vgg.RemoveAll(item => lst.Exists(i => i.FoglioViaggio.Equals(item.FoglioViaggio)));

                    lst.AddRange(vgg);

                    if (lst != null && lst.Any())
                    {
                        lst.ToList().ForEach(w =>
                        {
                            if (w.DataArrivoFromDB.Equals(DateTime.MinValue))
                            {
                                // cerca in newArray il foglioviaggio
                                // e prende il valore di DataArrivoFromDB

                                var itemX = newArray.Where(r => r.FoglioViaggio.Equals(w.FoglioViaggio)).FirstOrDefault();
                                if (itemX != null)
                                {
                                    w.DataArrivoFromDB = itemX.DataArrivoFromDB;
                                }

                            }
                        });
                    }
                }

                if (lst != null && lst.Any())
                {
                    lst.ForEach(i =>
                    {
                        if (i.DataFromDB.Equals(DateTime.MinValue))
                        {
                            if (i.Data.Length == 8)
                            {
                                string sgg = i.Data.Substring(0, 2);
                                string smm = i.Data.Substring(3, 2);
                                string saa = i.Data.Substring(6, 2);

                                int gg = int.Parse(sgg);
                                int mm = int.Parse(smm);
                                int aa = int.Parse(saa);
                                aa += 2000;

                                i.DataFromDB = new DateTime(aa, mm, gg);
                            }
                            else
                            {
                                i.DataFromDB = i.Data.ToDateTime(format: "ddMMyyyy");
                            }
                        }

                        switch (i.Stato)
                        {
                            case "A":
                            case "B":
                                i.Note = string.Empty;
                                break;
                            case "C":
                            case "D":
                                i.Note = "Foglio di viaggio emesso";
                                break;
                            case "G":
                            case "H":
                            case "I":
                            case "L":
                            case "M":
                            case "N":
                            case "P":
                            case "Q":
                            case "R":
                            case "S":
                                i.Note = "Nota spese in elaborazione";
                                break;
                            case "T":
                            case "U":
                            case "V":
                            case "W":
                            case "X":
                                i.Note = "Nota spese completata";
                                break;
                            case "E":
                            case "F":
                            case "Y":
                                i.Note = "Foglio di viaggio annullato";
                                break;
                            default:
                                break;
                        }
                    });
                    lst = CalcolaRimborso(lst);
                    serviceResponse.Trasferte.Viaggi = lst.OrderByDescending(d => d.DataFromDB).ToArray();
                }
                else
                {
                    serviceResponse.Trasferte.Viaggi = lst.ToArray();
                }
                return serviceResponse.Trasferte;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetTrasferte"
                });

                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Restituisce le trasferte programmate per il giorno "giornataDal"
        /// </summary>
        /// <param name="giornataDal">Giorno per il quale si intende ottenere l'elenco delle trasferte programmate</param>
        /// <param name="matricola">Matricola dell'utente per il quale ottenere l'elenco delle trasferte programmate, se null prende l'utente corrente</param>
        /// <returns></returns>
        public static List<DettaglioTrasfertaVM> GetTrasferteForDay(DateTime giornataDal, string matricola = null)
        {
            List<DettaglioTrasfertaVM> result = new List<DettaglioTrasfertaVM>();
            List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> viaggi = new List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>();
            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    matricola = CommonHelper.GetCurrentUserMatricola();
                }

                var pp = GetTrasferte(matricola);

                if (pp.Viaggi != null && pp.Viaggi.Any())
                {
                    pp.Viaggi.ToList().ForEach(w =>
                  {
                      if (w.DataFromDB.Date == giornataDal.Date)
                      {
                          viaggi.Add(w);
                      }
                  });
                }

                if (viaggi != null && viaggi.Any())
                {
                    foreach (var v in viaggi)
                    {
                        string foglioViaggio = v.FoglioViaggio;
                        DettaglioTrasfertaVM dt = new DettaglioTrasfertaVM();

                        dt.FoglioViaggio = GetFoglioViaggio(foglioViaggio, matricola);
                        dt.FViaggio = foglioViaggio;

                        if (dt.FoglioViaggio != null)
                        {
                            dt.StatoTrasferta = GetStatoTrasferta(dt.FoglioViaggio.STATO);
                            dt.GrandeEvento = GetGrandeEvento(dt.FoglioViaggio.COD_GRANDI_EVENTI);
                            dt.Itinerario = GetItinerario(foglioViaggio, matricola);
                            dt.NotaSpeseTrasferta = GetNotaSpeseTrasferta(foglioViaggio);
                            dt.Alberghi = GetAlberghi(foglioViaggio);
                            dt.BigliettiRai = GetBigliettoRai(foglioViaggio);
                            dt.Diaria = GetDiaria(foglioViaggio);
                            dt.ResiduoNetto = CalcolaResiduoNetto(foglioViaggio);
                        }
                        result.Add(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - HasTrasferta"
                });

                result = null;
            }

            return result;
        }

        public class ObjParamsAnniPrecedenti
        {
            public DateTime DataLimite { get; set; }
            public int Giorni { get; set; }
        }

        /// <summary>
        /// Reperimento della data limite
        /// </summary>
        /// <param name="matricola"></param>
        /// <returns></returns>
        private static ObjParamsAnniPrecedenti GetDataLimiteAnniPrecedenti(string matricola = null)
        {
            ObjParamsAnniPrecedenti result = null;

            string cultureString = "it-IT";
            DateTime curr = DateTime.Now;
            int anno = curr.Year;

            if (matricola == null)
            {
                matricola = CommonHelper.GetCurrentUserMatricola();
            }

            // reperimento della data limite su parametri di sistema
            try
            {

                using (digiGappEntities db = new digiGappEntities())
                {
                    // reperimento dei parametri
                    var parametri = db.MyRai_ParametriSistema.Where(w => w.Chiave.Equals("LimiteTrasferteAnniPrecedenti")).FirstOrDefault();

                    if (parametri != null)
                    {
                        result = new ObjParamsAnniPrecedenti();

                        if (!String.IsNullOrEmpty(parametri.Valore1))
                        {
                            string par1 = parametri.Valore1.Trim();

                            if (!String.IsNullOrEmpty(par1))
                            {
                                DateTime temp = DateTime.ParseExact(par1, "dd/MM/yyyy", CultureInfo.GetCultureInfo(cultureString));

                                result.DataLimite = temp;
                            }
                        }

                        if (!String.IsNullOrEmpty(parametri.Valore2))
                        {
                            string par2 = parametri.Valore2.Trim();

                            if (!String.IsNullOrEmpty(par2))
                            {
                                int gg = int.Parse(par2);
                                gg *= -1;
                                result.Giorni = gg;
                            }
                        }
                    }
                    else
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "TrasferteManager - GetDataLimiteAnniPrecedenti"
                });
            }

            return result;
        }

        /// <summary>
        /// Reperimento delle trasferte la cui dataArrivo sia successiva alla data impostata sul db
        /// </summary>
        /// <param name="matricola"></param>
        /// <returns></returns>
        public static List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> GetTrasferteFromDBAnniPrecedenti(string matricola = null)
        {
            List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> viaggi = null;

            try
            {
                if (matricola == null)
                {
                    matricola = CommonHelper.GetCurrentUserMatricola();
                }

                // reperimento della data limite su parametri di sistema
                var par = GetDataLimiteAnniPrecedenti(matricola);

                if (par == null)
                {
                    return viaggi;
                }

                DateTime dtTemp = DateTime.Now;
                dtTemp = dtTemp.AddDays(par.Giorni);
                string dataLimite = par.DataLimite.ToString("yyyy-MM-dd 00:00:00.000");
                string dataLimite2 = dtTemp.ToString("yyyy-MM-dd 00:00:00.000");

                using (var db = new cv_ModelEntities())
                {
                    string query = "SELECT [NUM_FOG] as FoglioViaggio									" +
                                    ",[DATA_PARTENZA] as DataFromDB										" +
                                    ",[DATA_ARRIVO] as DataArrivoFromDB   	    						" +
                                    ",[SCOPO] as Descrizione											" +
                                    ",[ANTICIPI] as AnticipoFromDB										" +
                                    ",0.0 as RimborsoFromDB												" +
                                    ",'' as Note														" +
                                    ",[STATO] as STATO													" +
                                    ",[SPESE_PREV] as SpesaPrevistaFromDB								" +
                                    ",[AUTORIZZATA_DA] as AutorizzatoDa									" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_FOGLIO_VIAGGIO]	" +
                                    "WHERE matricola_dp = '##MATRICOLA##'								" +
                                    "AND [STATO] in ('D')                                               " +
                                    "AND DATA_ARRIVO >= '##DATALIMITE##'        						" +
                                    "AND DATA_ARRIVO <= '##DATALIMITE2##'        						" +
                                    "ORDER BY [NUM_FOG] ASC ";

                    query = query.Replace("##MATRICOLA##", matricola);
                    query = query.Replace("##DATALIMITE##", dataLimite);
                    query = query.Replace("##DATALIMITE2##", dataLimite2);
                    viaggi = db.Database.SqlQuery<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>(query).ToList();

                    if (viaggi != null && viaggi.Any())
                    {
                        viaggi.ForEach(v =>
                       {
                           v.Data = v.DataFromDB.ToString("dd/MM/yyyy");
                           v.Anticipo = (double)v.AnticipoFromDB;
                           v.Rimborso = (double)v.RimborsoFromDB;
                           v.SpesaPrevista = (double)v.SpesaPrevistaFromDB;
                       });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "TrasferteManager - GetTrasferteFromDBAnniPrecedenti"
                });
                viaggi = null;
            }

            return viaggi;
        }

        public static List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> GetTrasferteFromDBAnniPrecedenti(DateTime dt1, DateTime dt2, string foglioViaggio, string scopo, string matricola = null, TrasferteMacroStato st = TrasferteMacroStato.NonSpecificato, string tconcl = "")
        {
            List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> viaggi = null;

            if (matricola == null)
            {
                matricola = CommonHelper.GetCurrentUserMatricola();
            }

            // reperimento della data limite su parametri di sistema
            var par = GetDataLimiteAnniPrecedenti(matricola);

            if (par == null)
            {
                return viaggi;
            }

            DateTime dtTemp = DateTime.Now;
            dtTemp = dtTemp.AddDays(par.Giorni);
            string dataLimite = par.DataLimite.ToString("yyyy-MM-dd 00:00:00.000");
            string dataLimite2 = dtTemp.ToString("yyyy-MM-dd 00:00:00.000");

            try
            {
                using (var db = new cv_ModelEntities())
                {
                    string query = "";

                    if (dt1.Equals(DateTime.MinValue))
                    {
                        // La data minima accettata da sql server è : 1753 - 01 - 01 00:00:00.000
                        dt1 = new DateTime(1753, 1, 1, 0, 0, 0);
                    }

                    if (!String.IsNullOrWhiteSpace(foglioViaggio))
                    {
                        query = "SELECT [NUM_FOG] as FoglioViaggio									" +
                                    ",[DATA_PARTENZA] as DataFromDB										" +
                                    ",[DATA_ARRIVO] as DataArrivoFromDB      							" +
                                    ",[SCOPO] as Descrizione											" +
                                    ",[ANTICIPI] as AnticipoFromDB										" +
                                    ",0.0 as RimborsoFromDB												" +
                                    ",'' as Note														" +
                                    ",[STATO] as STATO													" +
                                    ",[SPESE_PREV] as SpesaPrevistaFromDB								" +
                                    ",[AUTORIZZATA_DA] as AutorizzatoDa									" +
                                    "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_FOGLIO_VIAGGIO]	" +
                                    "WHERE matricola_dp = '##MATRICOLA##'								" +
                                   "AND [STATO] in ('D')                                               " +
                                    "AND DATA_ARRIVO >= '##DATALIMITE##'        						" +
                                    "AND DATA_ARRIVO <= '##DATALIMITE2##'        						" +
                                    "AND NUM_FOG = '##NUM_FOG##'                                        ";
                    }
                    else
                    {
                        query = "SELECT [NUM_FOG] as FoglioViaggio									" +
                                ",[DATA_PARTENZA] as DataFromDB										" +
                                ",[DATA_ARRIVO] as DataArrivoFromDB      							" +
                                ",[SCOPO] as Descrizione											" +
                                ",[ANTICIPI] as AnticipoFromDB										" +
                                ",0.0 as RimborsoFromDB												" +
                                ",'' as Note														" +
                                ",[STATO] as STATO													" +
                                ",[SPESE_PREV] as SpesaPrevistaFromDB								" +
                                ",[AUTORIZZATA_DA] as AutorizzatoDa									" +
                                "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_FOGLIO_VIAGGIO]	" +
                                "WHERE matricola_dp = '##MATRICOLA##'								" +
                                //    "AND [STATO] in ('D')                                               " +
                                "AND DATA_ARRIVO >= '##DATALIMITE##'        						" +
                                "AND DATA_ARRIVO <= '##DATALIMITE2##'        						" +
                                "AND DATA_PARTENZA >= '##DATAPARTENZA##'                            " +
                                "AND DATA_ARRIVO <= '##DATAARRIVO##' ";
                        switch (st)
                        {
                            case TrasferteMacroStato.Aperte:
                                query = query + "AND [STATO] not in ('" + tconcl.Replace(",", @"','") + "')";
                                break;
                            case TrasferteMacroStato.Concluse:
                                query = query + "AND [STATO] in ('" + tconcl.Replace(",", @"','") + "')";
                                break;
                            default:
                                query = query + "AND [STATO] in ('D')                                               ";
                                break;
                        }



                        if (!String.IsNullOrWhiteSpace(scopo))
                        {
                            query = query + " AND SCOPO LIKE '%##SCOPO##%' ";
                        }
                    }

                    query = query.Replace("##MATRICOLA##", matricola);
                    query = query.Replace("##DATAPARTENZA##", dt1.ToString("yyyy-MM-dd 00:00:00.000"));
                    query = query.Replace("##DATAARRIVO##", dt2.ToString("yyyy-MM-dd 00:00:00.000"));
                    query = query.Replace("##NUM_FOG##", foglioViaggio);
                    query = query.Replace("##SCOPO##", scopo);
                    query = query.Replace("##DATALIMITE##", dataLimite);
                    query = query.Replace("##DATALIMITE2##", dataLimite2);
                    viaggi = db.Database.SqlQuery<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>(query).ToList();

                    if (viaggi != null && viaggi.Any())
                    {
                        viaggi.ForEach(v =>
                       {
                           v.Data = v.DataFromDB.ToString("dd/MM/yyyy");
                           v.Anticipo = (double)v.AnticipoFromDB;
                           v.Rimborso = (double)v.RimborsoFromDB;
                           v.SpesaPrevista = (double)v.SpesaPrevistaFromDB;
                       });
                    }
                }
            }

            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetTrasferteFromDBAnniPrecedenti"
                });
                viaggi = null;
            }

            return viaggi;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matricola"></param>
        /// <returns></returns>
        public static Trasferta GetTrasferteAnniPrecedenti(string matricola = null)
        {
            MyRaiService1Client service = new MyRaiService1Client();
            Trasferta t = new Trasferta();
            TrasferteResponse serviceResponse = new TrasferteResponse();

            try
            {
                if (matricola == null)
                {
                    matricola = CommonHelper.GetCurrentUserMatricola();
                }

                // reperimento della data limite su parametri di sistema
                var par = GetDataLimiteAnniPrecedenti(matricola);

                if (par == null)
                {
                    return t;
                }

                DateTime dtTemp = DateTime.Now;
                dtTemp = dtTemp.AddDays(par.Giorni);
                string dataLimite = par.DataLimite.ToString("yyyy-MM-dd 00:00:00.000");
                string dataLimite2 = dtTemp.ToString("yyyy-MM-dd 00:00:00.000");
                DateTime dtLimite1 = par.DataLimite;
                DateTime dtLimite2 = dtTemp;

                serviceResponse = service.GetTrasferte(matricola);

                if (!serviceResponse.Esito)
                {
                    if (serviceResponse.ServiceResponse.Equals("ACK91", StringComparison.InvariantCultureIgnoreCase))
                    {
                        serviceResponse.Errore = "DATI MOMENTANEAMENTE NON DISPONIBILI";
                    }

                    serviceResponse.Esito = true;
                    serviceResponse.Trasferte = new Trasferta();
                }

                List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> lst = new List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>();

                // se ci sono viaggi, va reperita la descrizione per ognuno di esso perchè non viene
                // restituita da CICS
                if (serviceResponse.Trasferte.Viaggi != null && serviceResponse.Trasferte.Viaggi.Any())
                {
                    var elencoViaggi = "'" + String.Join("','", serviceResponse.Trasferte.Viaggi.Select(x => x.FoglioViaggio)) + "'";
                    var fogliViaggi = GetFogliViaggioMulti(elencoViaggi, matricola);

                    foreach (var v in serviceResponse.Trasferte.Viaggi)
                    {
                        var fv = fogliViaggi.FirstOrDefault(x => x.NUM_FOG == v.FoglioViaggio);

                        if (fv != null && fv.DATA_ARRIVO >= dtLimite1 && fv.DATA_ARRIVO <= dtLimite2)
                        {
                            v.Descrizione = fv.SCOPO;
                            v.Stato = fv.STATO;
                            v.Note = fv.ITINERARIO;
                        }
                        else
                        {
                            v.FoglioViaggio = null;
                        }
                    }

                    if (serviceResponse.Trasferte.Viaggi.Where(w => w.FoglioViaggio != null).Count() > 0)
                    {
                        lst = serviceResponse.Trasferte.Viaggi.Where(w => w.FoglioViaggio != null).OfType<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>().ToList();
                    }
                }

                var vgg = GetTrasferteFromDBAnniPrecedenti(matricola);

                if (vgg != null && vgg.Any())
                {
                    if (serviceResponse.Trasferte == null)
                        serviceResponse.Trasferte = new Trasferta();

                    var newArray = new MyRaiServiceInterface.MyRaiServiceReference1.Viaggio[vgg.Count()];

                    Array.Copy(vgg.ToArray(), newArray, vgg.Count());

                    // rimuove tutti gli elementi da vgg che sono già contenuti in lst
                    vgg.RemoveAll(item => lst.Exists(i => i.FoglioViaggio.Equals(item.FoglioViaggio)));

                    lst.AddRange(vgg);

                    if (lst != null && lst.Any())
                    {
                        lst.ToList().ForEach(w =>
                      {
                          if (w.DataArrivoFromDB.Equals(DateTime.MinValue))
                          {
                              // cerca in newArray il foglioviaggio
                              // e prende il valore di DataArrivoFromDB

                              var itemX = newArray.Where(r => r.FoglioViaggio.Equals(w.FoglioViaggio)).FirstOrDefault();
                              if (itemX != null)
                              {
                                  w.DataArrivoFromDB = itemX.DataArrivoFromDB;
                              }
                          }
                      });
                    }
                }

                if (lst != null && lst.Any())
                {
                    lst.RemoveAll(w => !w.Stato.Equals("D"));
                }

                if (lst != null && lst.Any())
                {
                    lst.ForEach(i =>
                   {
                       if (i.DataFromDB.Equals(DateTime.MinValue))
                       {
                           if (i.Data.Length == 8)
                           {
                               string sgg = i.Data.Substring(0, 2);
                               string smm = i.Data.Substring(3, 2);
                               string saa = i.Data.Substring(6, 2);

                               int gg = int.Parse(sgg);
                               int mm = int.Parse(smm);
                               int aa = int.Parse(saa);
                               aa += 2000;

                               i.DataFromDB = new DateTime(aa, mm, gg);
                           }
                           else
                           {
                               i.DataFromDB = i.Data.ToDateTime(format: "ddMMyyyy");
                           }
                       }

                       //switch ( i.Stato )
                       //{
                       //    case "A":
                       //    case "B":
                       //        i.Note = string.Empty;
                       //        break;
                       //    case "C":
                       //    case "D":
                       //        i.Note = "Foglio di viaggio emesso";
                       //        break;
                       //    case "G":
                       //    case "H":
                       //    case "I":
                       //    case "L":
                       //    case "M":
                       //    case "N":
                       //    case "P":
                       //    case "Q":
                       //    case "R":
                       //    case "S":
                       //        i.Note = "Nota spese in elaborazione";
                       //        break;
                       //    case "T":
                       //    case "U":
                       //    case "V":
                       //    case "W":
                       //    case "X":
                       //        i.Note = "Nota spese completata";
                       //        break;
                       //    case "E":
                       //    case "F":
                       //    case "Y":
                       //        i.Note = "Foglio di viaggio annullato";
                       //        break;
                       //    default:
                       //        break;
                       //}
                   });
                    lst = TrasferteManager.CalcolaRimborso(lst);
                    serviceResponse.Trasferte.Viaggi = lst.OrderByDescending(d => d.DataFromDB).ToArray();
                }
                else
                {
                    serviceResponse.Trasferte.Viaggi = lst.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "TrasferteManager - GetTrasferteAnniPrecedenti"
                });
            }

            return serviceResponse.Trasferte;
        }

        public static Trasferta GetTrasferteAnniPrecedenti(DateTime dt1, DateTime dt2, string foglioViaggio, string scopo, string matricola = null, TrasferteMacroStato st = TrasferteMacroStato.NonSpecificato)
        {
            MyRaiService1Client service = new MyRaiService1Client();
            Trasferta t = new Trasferta();
            TrasferteResponse serviceResponse = new TrasferteResponse();

            try
            {
                if (matricola == null)
                {
                    matricola = CommonHelper.GetCurrentUserMatricola();
                }

                // reperimento della data limite su parametri di sistema
                var par = GetDataLimiteAnniPrecedenti(matricola);

                if (par == null)
                {
                    return t;
                }

                DateTime dtTemp = DateTime.Now;
                dtTemp = dtTemp.AddDays(par.Giorni);
                string dataLimite = par.DataLimite.ToString("yyyy-MM-dd 00:00:00.000");
                string dataLimite2 = dtTemp.ToString("yyyy-MM-dd 00:00:00.000");
                DateTime dtLimite1 = par.DataLimite;
                DateTime dtLimite2 = dtTemp;

                serviceResponse = service.GetTrasferte(matricola);

                if (!serviceResponse.Esito)
                {
                    if (serviceResponse.ServiceResponse.Equals("ACK91", StringComparison.InvariantCultureIgnoreCase))
                    {
                        serviceResponse.Errore = "DATI MOMENTANEAMENTE NON DISPONIBILI";
                    }

                    serviceResponse.Esito = true;
                    serviceResponse.Trasferte = new Trasferta();
                }

                List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> lst = new List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>();

                // se ci sono viaggi, va reperita la descrizione per ognuno di esso perchè non viene
                // restituita da CICS
                string tconcl = null;
                using (var db = new myRaiData.digiGappEntities())
                {
                    var item = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("TrasferteStatiConclusi")).FirstOrDefault();

                    if (item != null)
                    {
                        tconcl = item.Valore1;
                    }
                }


                if (serviceResponse.Trasferte.Viaggi != null && serviceResponse.Trasferte.Viaggi.Any())
                {
                    var elencoViaggi = "'" + String.Join("','", serviceResponse.Trasferte.Viaggi.Select(x => x.FoglioViaggio)) + "'";
                    var fogliViaggi = GetFogliViaggioMulti(elencoViaggi, matricola);
                    foreach (var v in serviceResponse.Trasferte.Viaggi)
                    {
                        var fv = fogliViaggi.FirstOrDefault(x => x.NUM_FOG == v.FoglioViaggio);

                        if (fv != null && fv.DATA_ARRIVO >= dtLimite1 && fv.DATA_ARRIVO <= dtLimite2)
                        {
                            v.Descrizione = fv.SCOPO;
                            v.Stato = fv.STATO;
                            v.Note = fv.ITINERARIO;
                        }
                        else
                        {
                            v.FoglioViaggio = null;
                        }
                    }

                    if (serviceResponse.Trasferte.Viaggi.Where(w => w.FoglioViaggio != null).Count() > 0)
                    {
                        lst = serviceResponse.Trasferte.Viaggi.Where(w => w.FoglioViaggio != null).OfType<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>().ToList();
                    }
                }

                if (lst != null && lst.Any())
                {
                    if (st == TrasferteMacroStato.NonSpecificato)
                    {
                        lst.RemoveAll(w => !w.Stato.Equals("D"));
                    }
                    else
                    {


                        switch (st)
                        {
                            case TrasferteMacroStato.Aperte:

                                lst.RemoveWhere(x => x.Stato != null && tconcl.Contains(x.Stato));
                                break;
                            case TrasferteMacroStato.Concluse:
                                lst.RemoveWhere(x => x.Stato == null || !tconcl.Contains(x.Stato));
                                break;
                            default:
                                break;

                        }
                    }
                }
                if (lst != null && lst.Any())
                {
                    List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> tempLST = new List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>();
                    DateTime tempDate = DateTime.Now;
                    foreach (var lstItem in lst)
                    {
                        if (!String.IsNullOrEmpty(foglioViaggio))
                        {
                            if (lstItem.FoglioViaggio.Equals(foglioViaggio))
                            {
                                tempLST.Add(lstItem);
                            }
                        }
                        else
                        {
                            if (lstItem.Data.Length == 8)
                            {
                                string sgg = lstItem.Data.Substring(0, 2);
                                string smm = lstItem.Data.Substring(3, 2);
                                string saa = lstItem.Data.Substring(6, 2);

                                int gg = int.Parse(sgg);
                                int mm = int.Parse(smm);
                                int aa = int.Parse(saa);
                                aa += 2000;

                                tempDate = new DateTime(aa, mm, gg);
                            }
                            else if (lstItem.Data.Length == 10)
                            {
                                string sgg = lstItem.Data.Substring(0, 2);
                                string smm = lstItem.Data.Substring(3, 2);
                                string saa = lstItem.Data.Substring(6, 4);

                                int gg = int.Parse(sgg);
                                int mm = int.Parse(smm);
                                int aa = int.Parse(saa);

                                tempDate = new DateTime(aa, mm, gg);
                            }

                            if (tempDate.Date >= dt1.Date &&
                                tempDate.Date <= dt2.Date)
                            {
                                tempLST.Add(lstItem);
                            }
                        }
                    }

                    if (tempLST != null &&
                        tempLST.Any() &&
                        !String.IsNullOrEmpty(scopo) &&
                        String.IsNullOrEmpty(foglioViaggio))
                    {
                        tempLST.RemoveAll(w => !w.Descrizione.Contains(scopo));
                    }

                    lst.Clear();
                    lst.AddRange(tempLST.ToList());
                }

                var vgg = TrasferteManager.GetTrasferteFromDBAnniPrecedenti(dt1, dt2, foglioViaggio, scopo, matricola, st, tconcl);

                if (vgg != null && vgg.Any())
                {
                    if (serviceResponse.Trasferte == null)
                        serviceResponse.Trasferte = new Trasferta();

                    var newArray = new MyRaiServiceInterface.MyRaiServiceReference1.Viaggio[vgg.Count()];

                    Array.Copy(vgg.ToArray(), newArray, vgg.Count());

                    // rimuove tutti gli elementi da vgg che sono già contenuti in lst
                    vgg.RemoveAll(item => lst.Exists(i => i.FoglioViaggio.Equals(item.FoglioViaggio)));

                    lst.AddRange(vgg);

                    if (lst != null && lst.Any())
                    {
                        lst.ToList().ForEach(w =>
                      {
                          if (w.DataArrivoFromDB.Equals(DateTime.MinValue))
                          {
                              // cerca in newArray il foglioviaggio
                              // e prende il valore di DataArrivoFromDB
                              var itemX = newArray.Where(r => r.FoglioViaggio.Equals(w.FoglioViaggio)).FirstOrDefault();
                              if (itemX != null)
                              {
                                  w.DataArrivoFromDB = itemX.DataArrivoFromDB;
                              }
                          }
                      });
                    }
                }

                if (lst != null && lst.Any())
                {
                    lst.ForEach(i =>
                   {
                       if (i.DataFromDB.Equals(DateTime.MinValue))
                       {
                           if (i.Data.Length == 8)
                           {
                               string sgg = i.Data.Substring(0, 2);
                               string smm = i.Data.Substring(3, 2);
                               string saa = i.Data.Substring(6, 2);

                               int gg = int.Parse(sgg);
                               int mm = int.Parse(smm);
                               int aa = int.Parse(saa);
                               aa += 2000;

                               i.DataFromDB = new DateTime(aa, mm, gg);
                           }
                           else
                           {
                               i.DataFromDB = i.Data.ToDateTime(format: "ddMMyyyy");
                           }
                       }

                       //switch ( i.Stato )
                       //{
                       //    case "A":
                       //    case "B":
                       //        i.Note = string.Empty;
                       //        break;
                       //    case "C":
                       //    case "D":
                       //        i.Note = "Foglio di viaggio emesso";
                       //        break;
                       //    case "G":
                       //    case "H":
                       //    case "I":
                       //    case "L":
                       //    case "M":
                       //    case "N":
                       //    case "P":
                       //    case "Q":
                       //    case "R":
                       //    case "S":
                       //        i.Note = "Nota spese in elaborazione";
                       //        break;
                       //    case "T":
                       //    case "U":
                       //    case "V":
                       //    case "W":
                       //    case "X":
                       //        i.Note = "Nota spese completata";
                       //        break;
                       //    case "E":
                       //    case "F":
                       //    case "Y":
                       //        i.Note = "Foglio di viaggio annullato";
                       //        break;
                       //    default:
                       //        break;
                       //}
                   });
                    lst = TrasferteManager.CalcolaRimborso(lst);
                    serviceResponse.Trasferte.Viaggi = lst.OrderByDescending(d => d.DataFromDB).ToArray();
                }
                else
                {
                    serviceResponse.Trasferte.Viaggi = lst.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "TrasferteManager - GetTrasferteAnniPrecedenti"
                });
            }

            return serviceResponse.Trasferte;
        }

        public static Trasferta GetTrasferteAnag(string matricola, DateTime dt1, DateTime dt2)
        {
            MyRaiService1Client service = new MyRaiService1Client();

            try
            {
                Trasferta t = new Trasferta();
                TrasferteResponse serviceResponse = new TrasferteResponse();

                serviceResponse = service.GetTrasferte(matricola);

                if (!serviceResponse.Esito)
                {
                    if (serviceResponse.ServiceResponse.Equals("ACK91", StringComparison.InvariantCultureIgnoreCase))
                    {
                        serviceResponse.Errore = "DATI MOMENTANEAMENTE NON DISPONIBILI";
                    }

                    serviceResponse.Esito = true;
                    serviceResponse.Trasferte = new Trasferta();
                }

                List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> lst = new List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>();

                // se ci sono viaggi, va reperita la descrizione per ognuno di esso perchè non viene
                // restituita da CICS
                if (serviceResponse.Trasferte.Viaggi != null && serviceResponse.Trasferte.Viaggi.Any())
                {
                    var elencoViaggi = "'" + String.Join("','", serviceResponse.Trasferte.Viaggi.Select(x => x.FoglioViaggio)) + "'";
                    var fogliViaggi = GetFogliViaggioMulti(elencoViaggi, matricola);

                    foreach (var v in serviceResponse.Trasferte.Viaggi)
                    {
                        var fv = fogliViaggi.FirstOrDefault(x => x.NUM_FOG == v.FoglioViaggio);

                        if (fv != null)
                        {
                            v.Descrizione = fv.SCOPO;
                            v.Stato = fv.STATO;
                            v.Note = fv.ITINERARIO;
                        }
                    }

                    lst = serviceResponse.Trasferte.Viaggi.OfType<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>().ToList();
                }

                if (lst != null && lst.Any())
                {
                    List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> tempLST = new List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>();
                    DateTime tempDate = DateTime.Now;
                    foreach (var lstItem in lst)
                    {

                        if (lstItem.Data.Length == 8)
                        {
                            string sgg = lstItem.Data.Substring(0, 2);
                            string smm = lstItem.Data.Substring(3, 2);
                            string saa = lstItem.Data.Substring(6, 2);

                            int gg = int.Parse(sgg);
                            int mm = int.Parse(smm);
                            int aa = int.Parse(saa);
                            aa += 2000;

                            tempDate = new DateTime(aa, mm, gg);
                        }
                        else if (lstItem.Data.Length == 10)
                        {
                            string sgg = lstItem.Data.Substring(0, 2);
                            string smm = lstItem.Data.Substring(3, 2);
                            string saa = lstItem.Data.Substring(6, 4);

                            int gg = int.Parse(sgg);
                            int mm = int.Parse(smm);
                            int aa = int.Parse(saa);

                            tempDate = new DateTime(aa, mm, gg);
                        }

                        if (tempDate.Date >= dt1.Date &&
                            tempDate.Date <= dt2.Date)
                        {
                            tempLST.Add(lstItem);
                        }

                    }

                    lst.Clear();
                    lst.AddRange(tempLST.ToList());
                }

                var vgg = GetTrasferteFromDBAnag(dt1, dt2, null, null, matricola);

                if (vgg != null && vgg.Any())
                {
                    if (serviceResponse.Trasferte == null)
                        serviceResponse.Trasferte = new Trasferta();

                    var newArray = new MyRaiServiceInterface.MyRaiServiceReference1.Viaggio[vgg.Count()];

                    Array.Copy(vgg.ToArray(), newArray, vgg.Count());

                    // rimuove tutti gli elementi da vgg che sono già contenuti in lst
                    vgg.RemoveAll(item => lst.Exists(i => i.FoglioViaggio.Equals(item.FoglioViaggio)));

                    lst.AddRange(vgg);

                    if (lst != null && lst.Any())
                    {
                        lst.ToList().ForEach(w =>
                        {
                            if (w.DataArrivoFromDB.Equals(DateTime.MinValue))
                            {
                                // cerca in newArray il foglioviaggio
                                // e prende il valore di DataArrivoFromDB

                                var itemX = newArray.Where(r => r.FoglioViaggio.Equals(w.FoglioViaggio)).FirstOrDefault();
                                if (itemX != null)
                                {
                                    w.DataArrivoFromDB = itemX.DataArrivoFromDB;
                                }

                            }
                        });
                    }
                }

                if (lst != null && lst.Any())
                {
                    lst.ForEach(i =>
                    {
                        if (i.DataFromDB.Equals(DateTime.MinValue))
                        {
                            if (i.Data.Length == 8)
                            {
                                string sgg = i.Data.Substring(0, 2);
                                string smm = i.Data.Substring(3, 2);
                                string saa = i.Data.Substring(6, 2);

                                int gg = int.Parse(sgg);
                                int mm = int.Parse(smm);
                                int aa = int.Parse(saa);
                                aa += 2000;

                                i.DataFromDB = new DateTime(aa, mm, gg);
                            }
                            else
                            {
                                i.DataFromDB = i.Data.ToDateTime(format: "ddMMyyyy");
                            }
                        }

                        //switch (i.Stato)
                        //{
                        //    case "A":
                        //    case "B":
                        //        i.Note = string.Empty;
                        //        break;
                        //    case "C":
                        //    case "D":
                        //        i.Note = "Foglio di viaggio emesso";
                        //        break;
                        //    case "G":
                        //    case "H":
                        //    case "I":
                        //    case "L":
                        //    case "M":
                        //    case "N":
                        //    case "P":
                        //    case "Q":
                        //    case "R":
                        //    case "S":
                        //        i.Note = "Nota spese in elaborazione";
                        //        break;
                        //    case "T":
                        //    case "U":
                        //    case "V":
                        //    case "W":
                        //    case "X":
                        //        i.Note = "Nota spese completata";
                        //        break;
                        //    case "E":
                        //    case "F":
                        //    case "Y":
                        //        i.Note = "Foglio di viaggio annullato";
                        //        break;
                        //    default:
                        //        break;
                        //}
                    });
                    lst = CalcolaRimborso(lst);
                    serviceResponse.Trasferte.Viaggi = lst.OrderByDescending(d => d.DataFromDB).ToArray();
                }
                else
                {
                    serviceResponse.Trasferte.Viaggi = lst.ToArray();
                }
                return serviceResponse.Trasferte;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteManager - GetTrasferte"
                });

                throw new Exception(ex.Message);
            }
        }























    }
}
