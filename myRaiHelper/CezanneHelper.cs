using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace myRaiHelper
{
    public class CezanneHelper
    {
        public static string[] GetFSuperCat()
        {
            return new string[] { "Q10", "Q12", "Q14" };
        }
        public static bool IsDipTipo(string matricola, string tipo)
        {
            bool result = false;

            var db = new IncentiviEntities();

            switch (tipo)
            {
                case "FSUPER":
                    var catFSuper = GetFSuperCat();
                    result = db.SINTESI1.Any(x => x.COD_MATLIBROMAT == matricola && catFSuper.Contains(x.COD_QUALIFICA));
                    break;
                default:
                    break;
            }

            return result;
        }

        public static bool GetAnagBanca(string codAbi, string codCab, bool loadInfoCitta, out XR_ANAGBANCA anagBanca)
        {
            bool result = false;
            anagBanca = null;

            using (IncentiviEntities db = new IncentiviEntities())
            {
                if (loadInfoCitta)
                    anagBanca = db.XR_ANAGBANCA.Include("TB_COMUNE").FirstOrDefault(x => x.COD_ABI == codAbi && x.COD_CAB == codCab);
                else
                    anagBanca = db.XR_ANAGBANCA.FirstOrDefault(x => x.COD_ABI == codAbi && x.COD_CAB == codCab);
            }

            result = anagBanca != null;

            return result;
        }
        public static string GetNomeBanca(string codAbi, string codCab)
        {
            if (GetAnagBanca(codAbi, codCab, false, out XR_ANAGBANCA anagBanca))
                return anagBanca.DES_RAG_SOCIALE.Trim();
            else
                return "-non trovata-";
        }
        public static int GetIdPersona(string matricola)
        {
            int idPersona = 0;
            if (String.IsNullOrEmpty(matricola))
            {
                return idPersona;
            }
            using (IncentiviEntities db = new IncentiviEntities())
            {
                INC_V_XR_XANAGRA pers = db.V_XR_XANAGRA.FirstOrDefault(x => x.MATRICOLA == matricola);
                idPersona = pers.ID_PERSONA;
            }
            return idPersona;
        }
        public static string GetNominativoByMatricola(string matricola = "")
        {
            string nominativo = "";

            if (String.IsNullOrWhiteSpace(matricola))
            {
                return nominativo;
            }

            if (matricola.ToUpper().StartsWith("P"))
                matricola = matricola.Substring(1);
            else if (matricola.ToUpper().StartsWith("RAI\\P"))
                matricola = matricola.Substring(5);

            var dict = SessionHelper.Get("_Matricola_GetNominativoByMatricola", () => new Dictionary<string, string>());

            if (!dict.TryGetValue(matricola, out nominativo))
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var pers = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
                    if (pers != null)
                        nominativo = pers.Nominativo();
                }

                dict.Add(matricola, nominativo);
                SessionHelper.Set("_Matricola_GetNominativoByMatricola", dict);
            }

            return nominativo;
        }
        public static string GetMatricola(int idPersona)
        {
            string matricola = "";
            using (IncentiviEntities db = new IncentiviEntities())
            {
                INC_V_XR_XANAGRA pers = db.V_XR_XANAGRA.FirstOrDefault(x => x.ID_PERSONA == idPersona);
                matricola = pers.MATRICOLA;
            }
            return matricola;
        }

        public static string GetDes(string codice, string descrizione)
        {
            string des = "";

            if (!String.IsNullOrWhiteSpace(codice) && !(String.IsNullOrWhiteSpace(descrizione)))
            {
                des = descrizione;
                if (des.StartsWith(codice.Trim()))
                    des = des.Substring(codice.Trim().Length + 3);
            }

            return des;
        }

        public static void GetCampiFirma(out CampiFirma campiFirma)
        {
            campiFirma = new CampiFirma();
            GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);
            campiFirma.CodUser = codUser;
            campiFirma.CodTermid = codTermid;
            campiFirma.Timestamp = tms;
        }

        public static void GetCampiFirma(out string cod_user, out string cod_termid, out DateTime tms_timestamp)
        {
            string webContext = CommonHelper.GetAppSettings("WebContext");
            if (webContext == "false")
            {
                cod_user = "ADMIN";
                cod_termid = "BATCHSESSION";
            }
            else
            {
                cod_user = CommonHelper.GetCurrentUsername();
                cod_termid = System.Web.HttpContext.Current.Request.UserHostAddress;
            }
            tms_timestamp = DateTime.Now;
        }

        public enum TokenPad
        {
            Left,
            Right
        }
        public static string AddToken(string dato, int? len, char filler = ' ', string sep = "", TokenPad padDir = TokenPad.Right)
        {
            string result = "";

            dato = dato ?? "";

            if (len.HasValue)
            {
                if (padDir == TokenPad.Right)
                    result = dato.PadRight(len.Value, filler).Substring(0, len.Value) + sep;
                else
                    result = dato.PadLeft(len.Value, filler).Substring(0, len.Value) + sep;
            }
            else
                result = dato + sep;

            return result;
        }

        public static Type GetTypeByName(string name)
        {
            Type retType = null;

            retType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).FirstOrDefault(x => x.FullName == name);

            return retType;
        }
    }
}
