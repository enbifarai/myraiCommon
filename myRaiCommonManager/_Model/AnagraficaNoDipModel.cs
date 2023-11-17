using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    #region ClassiDB2
    [AttributeUsage(AttributeTargets.Property)]
    public class Db2Field : Attribute
    {
        public Db2Field()
        {

        }
    }
    public abstract class Db2BaseClass
    {
        public abstract string Key();
        public abstract string UpdateQuery();
        public abstract string InsertQuery();

    }
    public class TNDI_DATI_INDIVIDUALI : Db2BaseClass
    {
        [Db2Field]
        public string Matricola { get; set; }
        [Db2Field]
        public DateTime Datini { get; set; }
        [Db2Field]
        public DateTime Datfin { get; set; }
        [Db2Field]
        public string Codice_fiscale { get; set; }
        [Db2Field]
        public string Cognome { get; set; }
        [Db2Field]
        public string Nome { get; set; }
        [Db2Field]
        public string Sesso { get; set; }
        [Db2Field]
        public DateTime Data_nascita { get; set; }
        [Db2Field]
        public string Cod_luogo_nascita { get; set; }

        public override string Key()
        {
            return String.Format("Matricola='{0}' and Datini='{1:yyyy-MM-dd}'", Matricola, Datini);
        }

        public override string UpdateQuery()
        {
            string query = "";
            query += String.Format("Datfin = '{0:yyyy-MM-dd}' ,", Datfin);
            query += String.Format("Codice_fiscale = '{0}' ,", Codice_fiscale);
            query += String.Format("Cognome = '{0}' ,", Cognome.Replace("'", "''"));
            query += String.Format("Nome = '{0}' ,", Nome.Replace("'", "''"));
            query += String.Format("Sesso = '{0}' ,", Sesso);
            query += String.Format("Data_nascita = '{0:yyyy-MM-dd}' ,", Data_nascita);
            query += String.Format("Cod_luogo_nascita = '{0}' ", Cod_luogo_nascita);
            return query;
        }

        public override string InsertQuery()
        {
            string query = "";
            query += String.Format("'{0}' ,", Matricola);
            query += String.Format("'{0:yyyy-MM-dd}' ,", Datini);
            query += String.Format("'{0:yyyy-MM-dd}' ,", Datfin);
            query += String.Format("'{0}' ,", Codice_fiscale);
            query += String.Format("'{0}' ,", Cognome.Replace("'", "''"));
            query += String.Format("'{0}' ,", Nome.Replace("'", "''"));
            query += String.Format("'{0}' ,", Sesso);
            query += String.Format("'{0:yyyy-MM-dd}' ,", Data_nascita);
            query += String.Format("'{0}' ", Cod_luogo_nascita);
            return query;
        }

    }
    public class TNDI_IBAN : Db2BaseClass
    {
        [Db2Field]
        public string Matricola { get; set; }
        [Db2Field]
        public DateTime Datini { get; set; }
        [Db2Field]
        public DateTime Datfin { get; set; }
        [Db2Field]
        public string IBAN { get; set; }

        public override string Key()
        {
            return String.Format("Matricola='{0}' and Datini='{1:yyyy-MM-dd}'", Matricola, Datini);
        }

        public override string UpdateQuery()
        {
            string query = "";
            query += String.Format("Datfin = '{0:yyyy-MM-dd}' ,", Datfin);
            query += String.Format("IBAN = '{0}' ", IBAN);
            return query;
        }

        public override string InsertQuery()
        {
            string query = "";
            query += String.Format("'{0}' ,", Matricola);
            query += String.Format("'{0:yyyy-MM-dd}' ,", Datini);
            query += String.Format("'{0:yyyy-MM-dd}' ,", Datfin);
            query += String.Format("'{0}' ", IBAN);
            return query;
        }
    }
    public class TNDI_RESIDENZA_DOMICILIO : Db2BaseClass
    {
        [Db2Field]
        public string Matricola { get; set; }
        [Db2Field]
        public DateTime Datini { get; set; }
        [Db2Field]
        public DateTime Datfin { get; set; }
        [Db2Field]
        public string Res_Cod_Comune { get; set; }
        [Db2Field]
        public string Res_Indirizzo { get; set; }
        [Db2Field]
        public string Res_CAP { get; set; }
        [Db2Field]
        public string Res_Cod_Nazione { get; set; }
        [Db2Field]
        public string Dom_Cod_Comune { get; set; }
        [Db2Field]
        public string Dom_Indirizzo { get; set; }
        [Db2Field]
        public string Dom_CAP { get; set; }
        [Db2Field]
        public string Dom_Cod_Nazione { get; set; }

        public override string Key()
        {
            return String.Format("Matricola='{0}' and Datini='{1:yyyy-MM-dd}'", Matricola, Datini);
        }

        public override string UpdateQuery()
        {
            string query = "";
            query += String.Format("Datfin = '{0:yyyy-MM-dd}' ,", Datfin);
            query += String.Format("Res_Cod_Comune = '{0}' ,", Res_Cod_Comune);
            query += String.Format("Res_Indirizzo = '{0}' ,", Res_Indirizzo.Replace("'", "''"));
            query += String.Format("Res_CAP = '{0}' ,", Res_CAP);
            query += String.Format("Res_Cod_Nazione = '{0}' ,", Res_Cod_Nazione);
            query += String.Format("Dom_Cod_Comune = '{0}' ,", Dom_Cod_Comune);
            query += String.Format("Dom_Indirizzo = '{0}' ,", Dom_Indirizzo.Replace("'", "''"));
            query += String.Format("Dom_CAP = '{0}' ,", Dom_CAP);
            query += String.Format("Dom_Cod_Nazione = '{0}' ", Dom_Cod_Nazione);
            return query;
        }

        public override string InsertQuery()
        {
            string query = "";
            query += String.Format("'{0}' ,", Matricola);
            query += String.Format("'{0:yyyy-MM-dd}' ,", Datini);
            query += String.Format("'{0:yyyy-MM-dd}' ,", Datfin);
            query += String.Format("'{0}' ,", Res_Cod_Comune);
            query += String.Format("'{0}' ,", Res_Indirizzo.Replace("'", "''"));
            query += String.Format("'{0}' ,", Res_CAP);
            query += String.Format("'{0}' ,", Res_Cod_Nazione);
            query += String.Format("'{0}' ,", Dom_Cod_Comune);
            query += String.Format("'{0}' ,", Dom_Indirizzo.Replace("'", "''"));
            query += String.Format("'{0}' ,", Dom_CAP);
            query += String.Format("'{0}' ", Dom_Cod_Nazione);
            return query;
        }
    }
    public class TNDI_TIPO_RAPPORTO : Db2BaseClass
    {
        [Db2Field]
        public string Matricola { get; set; }
        [Db2Field]
        public string Societa { get; set; }
        [Db2Field]
        public string Tipologia { get; set; }
        [Db2Field]
        public DateTime Datini { get; set; }
        [Db2Field]
        public DateTime Datfin { get; set; }
        [Db2Field]
        public string Matr_dip_collegata { get; set; }
        [Db2Field]
        public string Num_Erede { get; set; }
        [Db2Field]
        public decimal Importo_reddito { get; set; }
        [Db2Field]
        public int Anno_riferimento { get; set; }
        public override string Key()
        {
            return String.Format("Matricola='{0}' and Datini='{1:yyyy-MM-dd}' and Societa='{2}' and Tipologia='{3}'", Matricola, Datini, Societa, Tipologia);
        }
        public override string UpdateQuery()
        {
            string query = "";
            query += String.Format("Datfin = '{0:yyyy-MM-dd}' ,", Datfin);
            query += String.Format("Matr_dip_collegata = '{0}' ,", Matr_dip_collegata);
            query += String.Format("Num_erede = '{0}' , ", Num_Erede);
            query += String.Format("Importo_reddito = {0} , ", Importo_reddito.ToString(new System.Globalization.NumberFormatInfo() { NumberDecimalSeparator = "." }));
            query += String.Format("Anno_riferimento = {0}  ", Anno_riferimento);
            return query;
        }

        public override string InsertQuery()
        {
            string query = "";
            query += String.Format("'{0}' ,", Matricola);
            query += String.Format("'{0}' ,", Societa);
            query += String.Format("'{0}' ,", Tipologia);
            query += String.Format("'{0:yyyy-MM-dd}' ,", Datini);
            query += String.Format("'{0:yyyy-MM-dd}' ,", Datfin);
            query += String.Format("'{0}' ,", Matr_dip_collegata);
            query += String.Format("'{0}' , ", Num_Erede);
            query += String.Format("{0} , ", Importo_reddito.ToString(new System.Globalization.NumberFormatInfo() { NumberDecimalSeparator = "." }));
            query += String.Format("{0}  ", Anno_riferimento);
            return query;
        }
    }
    public class TQUALIFICA : Db2BaseClass
    {
        public string MATRICOLA { get; set; }
        //public decimal ID_CONTRATTO { get; set; }
        public DateTime DATINI { get; set; }
        //public decimal? NPROGR { get; set; }
        public DateTime DATFIN { get; set; }
        //public string CATEGORIA { get; set; }
        //public string SERVIZIO { get; set; }
        //public string SEDE_CONTABILE { get; set; }
        //public string FIG_PROFESSIONALE { get; set; }
        //public string MANSIONE { get; set; }
        //public string CLASSE_RETRIBUTIVA { get; set; }
        //public string TIPO_MINIMO { get; set; }
        //public decimal TIPO_TRASFERIMENTO { get; set; }
        //public string CAUS_TRASFERIMENTO { get; set; }
        //public string COD_PART_TIME { get; set; }
        //public string COD_ORARIO_RIDOTTO { get; set; }
        //public decimal? GG_PRESTAZ_RIDOTTE { get; set; }
        //public decimal? ORE_SETTIMANALI { get; set; }
        //public string TIPO_ORARIO { get; set; }
        public string ASS_INF { get; set; }
        //public string IST_ASS { get; set; }
        //public string F_IND_SPEC_TD { get; set; }
        //public string F_INCENTIVATO { get; set; }
        //public string F_INCENTIVATO_PT { get; set; }
        //public DateTime? DATA_INCENTIV_PT { get; set; }
        public string UTENTE { get; set; }
        public DateTime? DATAGG { get; set; }
        public string TIPAGG { get; set; }


        public override string InsertQuery()
        {
            throw new NotImplementedException();
        }

        public override string Key()
        {
            throw new NotImplementedException();
        }

        public override string UpdateQuery()
        {
            throw new NotImplementedException();
        }
    }
    public class PrevisioneConIndennita
    {
        public HRDW_TREC_PREVISIONALE Previsione { get; set; }
        public List<HRDW_INDTE_PREVISIONALE> VariazioniIndennita { get; set; }
    }
    public class HRDW_TREC_PREVISIONALE : Db2BaseClass
    {
        public string MATRICOLA { get; set; }
        public string DATA_DECORRENZA { get; set; }
        public string TIPO_VARIAZIONE { get; set; }
        public string DATA_SCADENZA { get; set; }
        //public string ULTIMO_RECORD_$ { get; set; }
        public string DATA_COMPETENZA { get; set; }
        public string DATA_STAMPA { get; set; }
        public string CATEGORIA_ASSUNZ { get; set; }
        public string SEDE { get; set; }
        public string SERVIZIO1 { get; set; }
        public string SERVIZIO2 { get; set; }
        public string SEZIONE { get; set; }
        public string REPARTO_ATTIVITA { get; set; }
        public string CATEGORIA { get; set; }
        public string TIPO_MINIMO { get; set; }
        public string CLASSE_RETRIBUZ { get; set; }
        public string DESCR_CATEG { get; set; }
        public string ORARIO_SETTIMANALE { get; set; }
        public string NUM_GG_PARTIME_VER { get; set; }
        public string NUM_HH_PARTIME_VER { get; set; }
        public string MANSIONE { get; set; }
        public string ANZ_SERVIZIO { get; set; }
        public string ANZ_UTILE_AUM_BIEN { get; set; }
        public string ANZ_PER_XXV { get; set; }
        public string ANZ_UTILE_FERIE { get; set; }
        public string ANZ_UTILE_FONDO { get; set; }
        public string MATRICOLA_ENPALS { get; set; }
        public string DATA_CESSAZIONE { get; set; }
        public string DATA_ANZ_CATEGORIA { get; set; }
        public string NUMERO_PROTOCOLLO { get; set; }
        public string RETRIB_MENS_PER_12 { get; set; }
        public string FORFAIT_PER_12 { get; set; }
        public string PREMIO_PRODUZIONE { get; set; }
        public string INDENN_SALTUARIA { get; set; }
        public string TOT_RETRIB_ANNUA { get; set; }
        public string FORMA_CONTRATTO { get; set; }
        public string TIPO_RECL { get; set; }
        public string PERNOTTAMENTO { get; set; }
        public string CL_STRAORDIN { get; set; }
        public string CODICINI { get; set; }
        public string CONTING_CONGLOBATA { get; set; }
        public string EX_FC2 { get; set; }
        public string STIPENDIO_MENSILE { get; set; }
        public string XIII_MENSILITA { get; set; }
        public string XIV_MENSILITA { get; set; }
        public string DESTINAZ_FUNZION { get; set; }
        public string UORG { get; set; }
        public string MATRIC_SPETTACOLO { get; set; }
        public string NUM_PUNTATE { get; set; }
        public string DURATA_MESI { get; set; }
        public string DURATA_GIORNI { get; set; }
        public string INIZIO_PERIODO { get; set; }
        public string FINE_PERIODO { get; set; }
        public string DATA_RICONOSCIMEN { get; set; }
        public string IMPORTO { get; set; }
        public string CENTRO_DI_COSTO { get; set; }
        public string SEDE_1 { get; set; }
        public string SOT_COD_50 { get; set; }
        public string IMPORTO_50 { get; set; }
        public string IMPO_ANZ_ANTE_1937 { get; set; }
        public string IMPO_ANZ_37_52 { get; set; }
        public string IMPO_SUPERMINIMO { get; set; }
        public string CD_AUM_MERITO { get; set; }
        public string IMPO_AUM_MERITO { get; set; }
        public string CD_MERITO_ULT_CONC { get; set; }
        public string IMPO_MERITO_ULT_CO { get; set; }
        public string CD_MERITO_GAR_CONT { get; set; }
        public string IMPO_MERITO_GAR_CO { get; set; }
        public string IMPO_DIF_CAT_SUPER { get; set; }
        public string IMPO_AD_PERS_ASSOR { get; set; }
        public string CAT_RETROC_CLASSE { get; set; }
        public string TM_RETROC_CLASSE { get; set; }
        public string IMPO_RETROC_CLASSE { get; set; }
        public string CD_VARIABILI { get; set; }
        public string IMPORTO_VARIABILI { get; set; }
        public string TOT_IMP_AUM_ANZCAT { get; set; }
        public string TOT_IMP_CODICE_IND { get; set; }
        public string A_DISPOZIZIONE { get; set; }
        public string CODICE_INDENNITA { get; set; }
        public string TOT_GG_PVT { get; set; }
        public string IMPO_AUM_25_ANNI { get; set; }
        public string CD_AUMEN_BIENNALI { get; set; }
        public string IMPO_AUMEN_BIENNAL { get; set; }
        public string CONTATORE_RECORD { get; set; }
        public string NUMERO_PROTOCOL_2 { get; set; }
        public string A_DISP_NUM_10 { get; set; }
        public string A_DISP_NUM_5 { get; set; }
        public string A_DISP_ALF_3 { get; set; }
        public int ROWID_HRDW_TREC_PREV { get; set; }
        public string FLAG { get; set; }
        public string COSTO_GIORNALIERO_IFRS { get; set; }
        public string FILLER_01 { get; set; }
        public string FILLER_02 { get; set; }
        public string FILLER_03 { get; set; }
        public string FILLER_04 { get; set; }
        public string FILLER_05 { get; set; }
        public string FILLER_06 { get; set; }
        public string FILLER_07 { get; set; }
        public string FILLER_08 { get; set; }
        public string FILLER_09 { get; set; }

        public override string InsertQuery()
        {
            throw new NotImplementedException();
        }

        public override string Key()
        {
            throw new NotImplementedException();
        }

        public override string UpdateQuery()
        {
            throw new NotImplementedException();
        }
    }

    public class HRDW_INDTE_PREVISIONALE : Db2BaseClass
    {
        public string MATRICOLA_INDTE { get; set; }
        public string DATA_DECORR_INDTE { get; set; }
        public string CTR_INDTE { get; set; }
        public string PERC_INDEN_INDTE { get; set; }
        public string COD_INDENN_INDTE { get; set; }
        public string SC_INDENN_INDTE { get; set; }
        public string IMPO_INDENN_INDTE { get; set; }
        public int ROWID_HRDW_IND_PREV { get; set; }
        public string FLAG { get; set; }
        public override string InsertQuery()
        {
            throw new NotImplementedException();
        }

        public override string Key()
        {
            throw new NotImplementedException();
        }

        public override string UpdateQuery()
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    public class DB2Manager
    {
        const string _LINK_DB_NAME = "DB2LINK";

        public static string GetPrefixTable()
        {
            string dbName = "PROVA";
            if (CommonHelper.IsProduzione())
                dbName = "PROD";

            return dbName;
        }

        private static string Fields<T>(bool withCast)
        {
            string result = "*";

            var props = typeof(T).GetProperties();
            if (props != null && props.Any(x => Attribute.IsDefined(x, typeof(Db2Field))))
            {
                if (!withCast)
                    result = String.Join(",", props.Where(x => Attribute.IsDefined(x, typeof(Db2Field))).Select(x => x.Name));
                else
                {
                    List<string> names = new List<string>();
                    foreach (var item in props.Where(x => Attribute.IsDefined(x, typeof(Db2Field))))
                    {
                        Type propType = item.PropertyType;
                        Type baseType = propType.IsGenericType && propType.GetGenericArguments() != null && propType.GetGenericArguments().Any() ? propType.GetGenericArguments()[0] : propType;

                        if (baseType == typeof(int))
                            names.Add("convert(int, " + item.Name + ") as " + item.Name);
                        else
                            names.Add(item.Name);
                    }
                    result = String.Join(",", names);
                }
            }

            return result;
        }

        private static string GetSelectStatement<T>(string filter = "")
        {
            string dbName = GetPrefixTable();

            string tableName = typeof(T).Name;
            string fields = Fields<T>(false);

            string query = String.Format("SELECT {2} FROM {0}.{1}", dbName, tableName, fields);
            if (!String.IsNullOrWhiteSpace(filter))
                query += " WHERE " + filter.Replace("'", "''");

            return query;
        }

        public static List<T> SqlQuery<T>(string query)
        {
            List<T> result = new List<T>();

            IncentiviEntities db = new IncentiviEntities();
            try
            {
                var tmp = db.Database.SqlQuery<T>(query);
                if (tmp != null)
                    result.AddRange(tmp);
            }
            catch (Exception ex)
            {
                HrisHelper.LogOperazione("Exception", query, false, "DB2 SqlQuery: Errore durante l'esecuzione della query", null, ex);
            }

            return result;
        }

        public static T GetEntity<T>(string filter = "") where T : class
        {
            return GetEntities<T>(filter).FirstOrDefault();
        }
        public static List<T> GetEntities<T>(string filter = "") where T : class
        {
            List<T> result = new List<T>();

            string query = GetSelectStatement<T>();
            string sqlSqueryFields = Fields<T>(true);

            IncentiviEntities db = new IncentiviEntities();
            string cmd = String.Format("SELECT {2} FROM OPENQUERY({0}, '{1}')", _LINK_DB_NAME, query, sqlSqueryFields);
            if (!String.IsNullOrWhiteSpace(filter))
                cmd += " WHERE " + filter;

            try
            {
                var tmp = db.Database.SqlQuery<T>(cmd);
                if (tmp != null)
                    result.AddRange(tmp.ToList());
            }
            catch (Exception ex)
            {
                HrisHelper.LogOperazione("Exception", cmd, false, "DB2 GetEntities: Errore durante l'esecuzione della query", null, ex);
            }

            return result;
        }


        public static bool UpdateEntity<T>(T entity) where T : Db2BaseClass
        {
            bool result = false;

            string query = GetSelectStatement<T>(entity.Key());
            IncentiviEntities db = new IncentiviEntities();
            string cmd = String.Format("UPDATE OPENQUERY({0}, '{1}') SET {2}", _LINK_DB_NAME, query, entity.UpdateQuery());
            try
            {
                int tmp = db.Database.ExecuteSqlCommand(cmd);
                result = tmp > 0;
            }
            catch (Exception ex)
            {
                HrisHelper.LogOperazione("Exception", cmd, false, "DB2 Update: Errore durante l'esecuzione della query", null, ex);
            }

            return result;
        }

        public static bool InsertEntity<T>(T entity) where T : Db2BaseClass
        {
            bool result = false;

            string query = GetSelectStatement<T>();
            string cmd = String.Format("INSERT OPENQUERY({0}, '{1}') VALUES ({2})", _LINK_DB_NAME, query, entity.InsertQuery());
            IncentiviEntities db = new IncentiviEntities();
            try
            {
                int tmp = db.Database.ExecuteSqlCommand(cmd);
                result = tmp > 0;
            }
            catch (Exception ex)
            {
                HrisHelper.LogOperazione("Exception", cmd, false, "DB2 Insert: Errore durante l'esecuzione della query", null, ex);
            }

            return result;
        }


        public static bool DeleteEntity<T>(T entity) where T : Db2BaseClass
        {
            bool result = false;

            string query = GetSelectStatement<T>(entity.Key());
            IncentiviEntities db = new IncentiviEntities();
            string cmd = String.Format("DELETE OPENQUERY({0}, '{1}')", _LINK_DB_NAME, query);
            try
            {
                int tmp = db.Database.ExecuteSqlCommand(cmd);
                result = tmp > 0;
            }
            catch (Exception ex)
            {
                HrisHelper.LogOperazione("Exception", cmd, false, "DB2 Update: Errore durante l'esecuzione della query", null, ex);
            }

            return result;
        }
    }

    public class AnagNoDipRapporti : BaseAnagraficaData
    {
        public AnagNoDipRapporti() : base()
        {
            Elenco = new List<AnagNoDipRapportoModel>();
        }
        public List<AnagNoDipRapportoModel> Elenco { get; set; }
    }
    public class AnagNoDipRapportoModel : _IdentityData
    {
        public AnagNoDipRapportoModel()
        {
            IsNew = true;
            RappDataInizio = DateTime.Today;
            RappDataFine = RappDataInizio.AddDays(1);
            RappAnnoRiferimento = RappDataInizio.Year;
        }
        public AnagNoDipRapportoModel(XR_NDI_TIPO_SOGGETTO dbRec)
        {
            IsNew = false;
            IdPersona = dbRec.ID_PERSONA;
            Codice = OrigCodice = dbRec.COD_TIPO_SOGGETTO;
            Descrizione = dbRec.XR_NDI_TB_TIPO_RAPPORTO.DES_TIPOLOGIA;
            Societa = OrigSocieta = dbRec.COD_IMPRESA;
            RappDataInizio = OrigRappDataInizio = dbRec.DTA_INIZIO;
            RappDataFine = dbRec.DTA_FINE;
            MatricolaCollegata = dbRec.COD_MAT_COLLEGATA;
            NumErede = dbRec.NUM_EREDE;
            ImportoReddito = dbRec.IMPORTO_REDDITO.GetValueOrDefault();
            //Finchè non viene gestita la storicità delle società (non solo a livello di struttura ma anche nei dati)
            //non è possibile usare CODIFYIMP. 
            //Pertanto su XR_HRIS_PARAM c'è un parametro per gestire l'attuale codifica
            //DesSocieta = dbRec.CODIFYIMP.DES_RAGSOC;
            var paramSoc = HrisHelper.GetParametriJson<HrisMapSocieta>(HrisParam.DecodSocietaStrOrg);
            DesSocieta = paramSoc.FirstOrDefault(x => x.Cezanne == dbRec.COD_IMPRESA && x.Stato == 1).Descrizione;
            RappAnnoRiferimento = dbRec.ANNO_RIFERIMENTO.Value;
        }
        public int IdAnag { get; set; }
        public int Id { get; set; }
        public string Codice { get; set; }
        public string OrigCodice { get; set; }
        public string Descrizione { get; set; }
        public DateTime RappDataInizio { get; set; }
        public DateTime OrigRappDataInizio { get; set; }
        public DateTime RappDataFine { get; set; }
        public string Societa { get; set; }
        public string OrigSocieta { get; set; }
        public string DesSocieta { get; set; }
        public string NumErede { get; set; }
        public decimal ImportoReddito { get; set; }
        public string MatricolaCollegata { get; set; }
        public string NominativoMatrColl { get; set; }
        public int? RappAnnoRiferimento { get; set; }
        public bool IsNew { get; set; }
    }

    public class AnagNoDipIndirizzi : BaseAnagraficaData
    {
        public AnagNoDipIndirizzi() : base()
        {
            Elenco = new List<AnagNoDipIndirizziModel>();
        }
        public List<AnagNoDipIndirizziModel> Elenco { get; set; }
    }
    public class AnagNoDipIndirizziModel : _IdentityData
    {
        public AnagNoDipIndirizziModel() : base()
        {
            Residenza = new IndirizzoModel();
            Domicilio = new IndirizzoModel();
            IndDataInizio = DateTime.Today;
            IndDataFine = new DateTime(9999, 12, 31);
            IsNew = true;
        }
        public AnagNoDipIndirizziModel(XR_NDI_RESIDENZA_DOMICILIO dbRec) : base()
        {
            IndDataInizio = dbRec.DTA_INIZIO;
            IndDataFine = dbRec.DTA_FINE;

            Residenza = new IndirizzoModel()
            {
                Tipologia = IndirizzoType.Residenza,
                IdPersona = dbRec.ID_PERSONA,
                Decorrenza = dbRec.DTA_INIZIO,
                DataFine = dbRec.DTA_FINE,
                CodCitta = dbRec.COD_CITTARES,
                Indirizzo = dbRec.DES_INDIRRES,
                Citta = dbRec.TB_COMUNE_RESIDENZA.COD_SIGLANAZIONE == "ITA" ? String.Format("{0}, {1}", dbRec.TB_COMUNE_RESIDENZA.DES_CITTA, dbRec.TB_COMUNE_RESIDENZA.COD_PROV_STATE) : dbRec.TB_COMUNE_RESIDENZA.DES_CITTA,
                CAP = dbRec.CAP_CAPRES,
                CodStato = dbRec.TB_COMUNE_RESIDENZA.COD_SIGLANAZIONE,
                Stato = dbRec.TB_COMUNE_RESIDENZA.TB_NAZIONE.DES_NAZIONE
            };

            Domicilio = new IndirizzoModel()
            {
                Tipologia = IndirizzoType.Domicilio,
                IdPersona = dbRec.ID_PERSONA,
                Decorrenza = dbRec.DTA_INIZIO,
                DataFine = dbRec.DTA_FINE,
                CodCitta = dbRec.COD_CITTADOM,
                Indirizzo = dbRec.DES_INDIRDOM,
                Citta = dbRec.TB_COMUNE_DOMICILIO.COD_SIGLANAZIONE == "ITA" ? String.Format("{0}, {1}", dbRec.TB_COMUNE_DOMICILIO.DES_CITTA, dbRec.TB_COMUNE_DOMICILIO.COD_PROV_STATE) : dbRec.TB_COMUNE_DOMICILIO.DES_CITTA,
                CAP = dbRec.CAP_CAPDOM,
                CodStato = dbRec.TB_COMUNE_DOMICILIO.COD_SIGLANAZIONE,
                Stato = dbRec.TB_COMUNE_DOMICILIO.TB_NAZIONE.DES_NAZIONE
            };

            LastModifiedTime = dbRec.TMS_TIMESTAMP;
            IsNew = false;
        }
        public int IdAnag { get; set; }
        public DateTime IndDataInizio { get; set; }
        public DateTime IndDataFine { get; set; }
        public bool IsNew { get; set; }
        public IndirizzoModel Residenza { get; set; }
        public IndirizzoModel Domicilio { get; set; }
    }

    public class AnagNoDipIban : BaseAnagraficaData
    {
        public AnagNoDipIban() : base()
        {
            Elenco = new List<AnagNoDipIbanModel>();
        }

        public List<AnagNoDipIbanModel> Elenco { get; set; }
    }
    public class AnagNoDipIbanModel : IbanModel
    {
        public AnagNoDipIbanModel() : base()
        {
            this.Tipologia = IbanType.NonDefinito;
        }
        public AnagNoDipIbanModel(XR_DATIBANCARI dbRec) : base(dbRec)
        {
            Tipologia = IbanType.NonDefinito;
        }

        public int IdAnag { get; set; }
    }

    public class AnagraficaNoDipModel : BaseAnagraficaData
    {
        public AnagraficaNoDipModel()
        {
            DatiAnagrafici = new AnagraficaDatiAnag()
            {
                DataNascita = DateTime.Today
            };
            DatiBancari = new AnagNoDipIban();
            ResidenzaDomicilio = new AnagNoDipIndirizzi();
            Rapporti = new AnagNoDipRapporti();
            AnagDataInizio = new DateTime(DateTime.Today.Year, 1, 1);
            AnagDataFine = new DateTime(9999, 12, 31);
            Storico = new List<AnagraficaNoDipModel>();
        }

        public AnagraficaNoDipModel(XR_NDI_ANAG dbRec)
        {
            AnagDataInizio = DateTime.Today;
            AnagDataFine = new DateTime(9999, 12, 31);

            IdAnag = dbRec.ID_ANAG;
            IdPersona = dbRec.ID_PERSONA;
            Matricola = dbRec.MATRICOLA;

            OrigDataInizio = dbRec.DTA_INIZIO;
            OrigDataFine = dbRec.DTA_FINE;

            AnagDataInizio = dbRec.DTA_INIZIO;
            AnagDataFine = dbRec.DTA_FINE;

            AnagNewDataInizio = null;

            IndForzaCF = dbRec.IND_MANCF;
            IsDipendente = dbRec.IND_DIPENDENTE;

            DatiAnagrafici = new AnagraficaDatiAnag(dbRec);
            DatiAnagrafici.Matricola = dbRec.MATRICOLA;

            DatiBancari = new AnagNoDipIban();
            if (dbRec.ANAGPERS.XR_DATIBANCARI != null)
            {
                foreach (var dbIban in dbRec.ANAGPERS.XR_DATIBANCARI.Where(x => x.COD_TIPOCONTO == "N"))
                {
                    var iban = new AnagNoDipIbanModel(dbIban);
                    iban.IdAnag = IdAnag;
                    DatiBancari.Elenco.Add(iban);
                }
            }

            ResidenzaDomicilio = new AnagNoDipIndirizzi();
            if (dbRec.ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO != null)
            {
                foreach (var res in dbRec.ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO)
                {
                    var resDom = new AnagNoDipIndirizziModel(res);
                    resDom.IdAnag = IdAnag;
                    ResidenzaDomicilio.Elenco.Add(resDom);
                }
            }

            Rapporti = new AnagNoDipRapporti();
            if (dbRec.ANAGPERS.XR_NDI_TIPO_SOGGETTO != null)
            {
                foreach (var recRapp in dbRec.ANAGPERS.XR_NDI_TIPO_SOGGETTO)
                {
                    var rapp = new AnagNoDipRapportoModel(recRapp);
                    rapp.IdAnag = IdAnag;
                    Rapporti.Elenco.Add(rapp);
                }
            }

            Storico = new List<AnagraficaNoDipModel>();
        }

        public void SetAbil(bool create, bool read, bool update, bool delete)
        {
            if (IdPersonaInCarico != 0 && IdPersonaInCarico != CommonHelper.GetCurrentIdPersona())
            {
                create = false;
                update = false;
                delete = false;
            }

            CanAdd = create;
            CanDelete = delete;
            CanModify = update;

            DatiBancari.CanAdd = CanAdd;
            DatiBancari.CanDelete = CanDelete;
            DatiBancari.CanModify = CanModify;

            ResidenzaDomicilio.CanAdd = CanAdd;
            ResidenzaDomicilio.CanDelete = CanDelete;
            ResidenzaDomicilio.CanModify = CanModify;

            Rapporti.CanAdd = CanAdd;
            Rapporti.CanDelete = CanDelete;
            Rapporti.CanModify = CanModify;
        }

        public int IdAnag { get; set; }
        public bool IndForzaCF { get; set; }
        public DateTime OrigDataInizio { get; set; }
        public DateTime OrigDataFine { get; set; }
        public DateTime AnagDataInizio { get; set; }
        public DateTime AnagDataFine { get; set; }
        public DateTime? AnagNewDataInizio { get; set; }
        public bool IsDipendente { get; set; }
        public AnagraficaDatiAnag DatiAnagrafici { get; set; }
        public AnagNoDipIndirizzi ResidenzaDomicilio { get; set; }
        public AnagNoDipRapporti Rapporti { get; set; }
        public AnagNoDipIban DatiBancari { get; set; }

        public List<AnagraficaNoDipModel> Storico { get; set; }
        public int IdPersonaInCarico { get; internal set; }
    }

    public class AnagNoDipRicerca
    {
        public bool HasFilter { get; set; }
        public string Cognome { get; set; }
        public string Nome { get; set; }
        public string CodiceFiscale { get; set; }
        public string Matricola { get; set; }
        public string AnnoRapporto { get; set; }
        public string MatrCollegata { get; set; }
        public string TipoRapporto { get; set; }
        public int? AnnoRiferimento { get; set; }
    }
}
