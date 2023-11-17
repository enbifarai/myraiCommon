using myRai.Data.Interface;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace myRaiData
{
    public partial class MyRai_Eccezioni_Ammesse
    {
        public string importo_preimpostato { get; set; }
    }



    public partial class L2D_SEDE_GAPP
    {
        public string intervallo_variabile_mensa(DateTime? data)
        {
            if (!String.IsNullOrWhiteSpace(this.intervallo_mensa) && this.intervallo_mensa.ToUpper().StartsWith("MENSAVARIABILE"))
            {
                return GetIntervalloMensaVariabile(data, this.intervallo_mensa.ToUpper());//venezia pranzo
            }
            else
            {
                return this.intervallo_mensa;
            }
        }

        public string intervallo_variabile_mensa_serale(DateTime? data)
        {
            if (!String.IsNullOrWhiteSpace(this.intervallo_mensa_serale) && this.intervallo_mensa_serale.ToUpper().StartsWith("MENSAVARIABILE"))
            {
                return GetIntervalloMensaVariabile(data, this.intervallo_mensa_serale.ToUpper());//venezia sera
            }
            else
            {
                return this.intervallo_mensa_serale;
            }
        }

        public string GetIntervalloMensaVariabile(DateTime? data, string codice)
        {
            var db = new digiGappEntities();
            if (data == null) data = DateTime.Today;

            string d = ((int)((DateTime)data).DayOfWeek).ToString();
            if (((DateTime)data).DayOfWeek == DayOfWeek.Sunday) d = "7";

            var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == codice && x.Valore1.Contains(d)).FirstOrDefault();

            if (par == null || string.IsNullOrWhiteSpace(par.Valore2))
                throw new Exception("Codice Mensa variabile non trovato:" + codice);
            else
                return par.Valore2;
        }
    }

    public partial class MyRai_PianoFerieBatch
    {
        public DateTime DataProvenienzaRipianificazione { get; set; }
    }
    public partial class MyRai_Regole_SchedeEccezioni
    {
        public int versione { get; set; }
    }
    public partial class MyRai_News
    {
        public bool isNew { get; set; }
    }
    public partial class PERSEOEntities : DbContext
    {
        public PERSEOEntities(string connectionString)
         : base(connectionString)
        {
        }
    }

}

namespace myRaiData.Incentivi
{
    public partial class IncentiviEntities : DbContext
    {
        public IncentiviEntities(string connectionString)
           : base(connectionString)
        {

        }

    }

    public class CampiFirma
    {
        public string CodUser { get; set; }
        public string CodTermid { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public partial class XR_MAT_RICHIESTE
    {
        public Boolean CFinvalid { get; set; }
        public virtual ICollection<XR_WKF_OPERSTATI> XR_WKF_MATCON_OPERSTATI
        {
            get
            {
                return this.XR_WKF_OPERSTATI.Where(x =>
                x.NASCOSTO == false &&
                (x.COD_TIPO_PRATICA == "MAT" || x.COD_TIPO_PRATICA == "CON" || x.COD_TIPO_PRATICA == "TE" || x.COD_TIPO_PRATICA == "SW")).ToList();
            }
        }
        public bool InCaricoANessuno { get; set; }

        public XR_WKF_OPERSTATI _GetCurrentStatusRow()
        {
            return this.XR_WKF_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();
        }
        public int? _GetCurrentStatusValue()
        {
            return this.XR_WKF_OPERSTATI.OrderByDescending(x => x.ID_STATO).Select(x => x.ID_STATO).FirstOrDefault();
        }
        public int? _GetNextStatusValue(IncentiviEntities db)
        {
            var curr = this._GetCurrentStatusValue();

            var workflowItems = db.XR_WKF_WORKFLOW.Where(x =>
                x.ID_TIPOLOGIA == this.XR_MAT_CATEGORIE.ID_TIPOLOGIA_WORKFLOW &&
                x.ID_STATO > curr).OrderBy(x => x.ORDINE).ToList();

            if (workflowItems.Any())
                return workflowItems.First().ID_STATO;
            else
                return null;
        }
        public string _SetStatus(int value, string Matricola, string Nominativo, IncentiviEntities db, bool Save)
        {
            myRaiData.Incentivi.XR_WKF_OPERSTATI Stato = new myRaiData.Incentivi.XR_WKF_OPERSTATI()
            {
                COD_TERMID = HttpContext.Current.Request.UserHostAddress,
                COD_TIPO_PRATICA = this.XR_MAT_CATEGORIE.CAT,
                COD_USER = Matricola,
                ID_STATO = value,
                NOMINATIVO = Nominativo,
                TMS_TIMESTAMP = DateTime.Now,
                ID_TIPOLOGIA = (int)this.XR_MAT_CATEGORIE.ID_TIPOLOGIA_WORKFLOW,// db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault(),
                VALID_DTA_INI = DateTime.Now,
                ID_GESTIONE = this.ID
            };
            try
            {
                db.XR_WKF_OPERSTATI.Add(Stato);
                if (Save)
                    db.SaveChanges();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return null;
        }
    }

    public partial class XR_INC_OPERSTATI_DOC
    {
        public int ID_STATO_RIF { get; set; }
        public bool DisableDelete { get; set; }
        public bool ShowModifiedDate { get; set; }
        public string[] ElencoTag { get; set; }
        public bool IsExternal { get; set; }
        public string ExternalAction { get; set; }
        public bool ShowApproveBtn { get; set; }
        public string Chiave { get; set; }
        public bool ShowTipoRifiuto { get; set; }
        public bool FromTemplate { get; set; }
        public int? FileId { get; set; }
        public string FileInfo { get; set; }
    }

    public partial class XR_INC_DIPENDENTI
    {
        public T GetField<T>(string fieldName, T defaultValue = default(T))
        {
            return GetField(XR_INC_DIPENDENTI_FIELD, fieldName, defaultValue);
        }

        public static T GetField<T>(ICollection<XR_INC_DIPENDENTI_FIELD> fields, string fieldName, T defaultValue = default(T))
        {
            T result = defaultValue;

            if (fields != null)
            {
                XR_INC_DIPENDENTI_FIELD field = fields.FirstOrDefault(x => x.COD_FIELD == fieldName);
                if (field != null)
                {
                    Type outputType = typeof(T);
                    Type baseType = outputType.IsGenericType && outputType.GetGenericArguments() != null && outputType.GetGenericArguments().Any() ?
                    outputType.GetGenericArguments()[0] : outputType;

                    object obj = null;
                    if (baseType == typeof(int))
                        obj = field.INT_VALUE;
                    else if (baseType == typeof(DateTime))
                        obj = field.DATE_VALUE;
                    else if (baseType == typeof(decimal))
                        obj = field.DEC_VALUE;
                    else if (baseType == typeof(bool))
                        obj = field.BOOL_VALUE;
                    else
                        obj = field.STR_VALUE;

                    if (obj != null)
                        result = (T)Convert.ChangeType(obj, baseType);
                }
            }

            return result;
        }

        public void SetField<T>(string fieldName, T value)
        {
            XR_INC_DIPENDENTI_FIELD field = null;
            if (XR_INC_DIPENDENTI_FIELD != null)
                field = XR_INC_DIPENDENTI_FIELD.FirstOrDefault(x => x.COD_FIELD == fieldName);

            bool isNew = false;
            if (field == null)
            {
                isNew = true;
                field = new XR_INC_DIPENDENTI_FIELD();
            }

            field.ID_DIPENDENTE = ID_DIPENDENTE;
            field.MATRICOLA = MATRICOLA;
            field.COD_FIELD = fieldName;

            Type outputType = typeof(T);
            Type baseType = outputType.IsGenericType && outputType.GetGenericArguments() != null && outputType.GetGenericArguments().Any() ?
                                outputType.GetGenericArguments()[0] : outputType;

            if (baseType == typeof(int))
                field.INT_VALUE = (int?)(object)value;
            else if (baseType == typeof(DateTime))
                field.DATE_VALUE = (DateTime?)(object)value;
            else if (baseType == typeof(decimal))
                field.DEC_VALUE = (decimal?)(object)value;
            else if (baseType == typeof(bool))
                field.BOOL_VALUE = (bool?)(object)value;
            else
                field.STR_VALUE = value != null ? value.ToString() : null;

            if (isNew)
                XR_INC_DIPENDENTI_FIELD.Add(field);
        }
    }

    public partial class XR_TB_SERVIZIO
    {
        public XR_TB_SERVIZIO_EXT RecEsteso { get; set; }
        public List<XR_TB_SERVIZIO_EXT> Storico { get; set; }
    }

    public partial class SINTESI1 : ISintesi1
    {

    }
    public partial class XR_IMM_IMMATRICOLAZIONI : ISintesi1
    {
        public string COD_IMPRESACR { get => COD_IMPRESA; }
        public string COD_MATLIBROMAT { get => COD_MATDIP; }
        public string COD_TPCNTR { get => null; }
    }

    public partial class XR_WKF_RICHIESTE
    {
        public T GetField<T>(string fieldName, T defaultValue = default(T))
        {
            return GetField(XR_HRIS_EXTRA_FIELD, fieldName, defaultValue);
        }

        public static T GetField<T>(ICollection<XR_HRIS_EXTRA_FIELD> fields, string fieldName, T defaultValue = default(T))
        {
            T result = defaultValue;

            if (fields != null)
            {
                XR_HRIS_EXTRA_FIELD field = fields.FirstOrDefault(x => x.COD_FIELD == fieldName);
                if (field != null)
                {
                    Type outputType = typeof(T);
                    Type baseType = outputType.IsGenericType && outputType.GetGenericArguments() != null && outputType.GetGenericArguments().Any() ?
                    outputType.GetGenericArguments()[0] : outputType;

                    object obj = null;
                    if (baseType == typeof(int))
                        obj = field.INT_VALUE;
                    else if (baseType == typeof(DateTime))
                        obj = field.DATE_VALUE;
                    else if (baseType == typeof(decimal))
                        obj = field.DEC_VALUE;
                    else if (baseType == typeof(bool))
                        obj = field.BOOL_VALUE;
                    else
                        obj = field.STR_VALUE;

                    if (obj != null)
                        result = (T)Convert.ChangeType(obj, baseType);
                }
            }

            return result;
        }

        public void SetField<T>(string fieldName, T value, CampiFirma campiFirma)
        {
            SetField(fieldName, value, campiFirma.CodUser, campiFirma.CodTermid, campiFirma.Timestamp);
        }
        public void SetField<T>(string fieldName, T value, string codUser, string codTermid, DateTime tmsTimestamp)
        {
            XR_HRIS_EXTRA_FIELD field = null;
            if (XR_HRIS_EXTRA_FIELD != null)
                field = XR_HRIS_EXTRA_FIELD.FirstOrDefault(x => x.COD_FIELD == fieldName);

            bool isNew = false;
            if (field == null)
            {
                isNew = true;
                field = new XR_HRIS_EXTRA_FIELD();
            }

            field.MATRICOLA = MATRICOLA;
            field.COD_FIELD = fieldName;

            Type outputType = typeof(T);
            Type baseType = outputType.IsGenericType && outputType.GetGenericArguments() != null && outputType.GetGenericArguments().Any() ?
                                outputType.GetGenericArguments()[0] : outputType;

            if (baseType == typeof(int))
                field.INT_VALUE = (int?)(object)value;
            else if (baseType == typeof(DateTime))
                field.DATE_VALUE = (DateTime?)(object)value;
            else if (baseType == typeof(decimal))
                field.DEC_VALUE = (decimal?)(object)value;
            else if (baseType == typeof(bool))
                field.BOOL_VALUE = (bool?)(object)value;
            else
                field.STR_VALUE = value != null ? value.ToString() : null;

            field.COD_USER = codUser;
            field.COD_TERMID = codTermid;
            field.TMS_TIMESTAMP = tmsTimestamp;

            if (isNew)
                XR_HRIS_EXTRA_FIELD.Add(field);
        }

        public byte[] GetTemplate(string codTipo, string templateName)
        {
            return GetTemplate(XR_HRIS_TEMPLATE, codTipo, templateName);
        }

        public static byte[] GetTemplate(ICollection<XR_HRIS_TEMPLATE> templates, string codTipo, string templateName)
        {
            byte[] result = null;

            if (templates != null)
            {
                //Devono giò arrivare filtrati per matricola
                XR_HRIS_TEMPLATE field = templates.FirstOrDefault(x => x.COD_TIPO == codTipo && x.NME_TEMPLATE==templateName);
                if (field != null && field.TEMPLATE!=null)
                {
                    result = field.TEMPLATE;
                }
            }

            return result;
        }

        public void SetTemplate(string codTipo, string templateName, byte[] value, CampiFirma campiFirma)
        {
            XR_HRIS_TEMPLATE template = null;
            if (XR_HRIS_TEMPLATE != null)
                template = XR_HRIS_TEMPLATE.FirstOrDefault(x => x.COD_TIPO == codTipo && x.NME_TEMPLATE == templateName);

            bool isNew = false;
            if (template == null)
            {
                isNew = true;
                template = new XR_HRIS_TEMPLATE()
                {
                    ID_GESTIONE = ID_GESTIONE,
                    ID_TIPOLOGIA = ID_TIPOLOGIA,
                    COD_TIPO = codTipo,
                    NME_TEMPLATE = templateName,
                    VALID_DTA_INI = campiFirma.Timestamp
                };
            }

            template.TEMPLATE = value;
            template.COD_USER = campiFirma.CodUser;
            template.COD_TERMID = campiFirma.CodTermid;
            template.TMS_TIMESTAMP = campiFirma.Timestamp;

            if (isNew)
                XR_HRIS_TEMPLATE.Add(template);
        }
    }

    public partial class XR_SSV_CODICE_OTP
    {
        public bool Inviato { get; set; }
    }
}

