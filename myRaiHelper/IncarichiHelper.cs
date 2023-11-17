using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiHelper
{
    public class IncarichiHelper
    {
        public static bool EnabledToIncarichi(string matr)
        {
            //Per gestire i permessi di lettura/scrittura e filtri sulle società siamo passati alle abilitazioni di HRIS disponibili su /abilitazioni
            return AuthHelper.EnabledTo(matr, "STRORG");

            //string[] par= CommonHelper.GetParametri<String>(EnumParametriSistema.MatricoleIncarichiVisMod);

            //return par != null &&
            //    (
            //    (par[0] != null && par[0].Split(',').Contains(matr))
            //    ||
            //    (par[1] != null && par[1].Split(',').Contains(matr))
            //    ) ;
        }

        public static bool EnabledToIncarichiModifica(string matr)
        {
            string[] par = CommonHelper.GetParametri<String>(EnumParametriSistema.MatricoleIncarichiVisMod);
            return (par[1] != null && par[1].Split(',').Contains(matr));
        }
        public static DateTime GetDateFrom_yyMMdd(string data)
        {
            DateTime D;
            DateTime.TryParseExact(data, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out D);
            return D;
        }
        public static string ConvertDateFromSlashedDDMMYYYY_to_YYYYMMDD(string data)
        {
            DateTime D;
            DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);
            return D.ToString("yyyyMMdd");
        }
    }
}