using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiHelper
{
    public class MaternitaCongediHelper
    {
        public enum MaternitaCongediUffici
        {
            Amministrazione=1,
            Gestione=2,
            Personale=3
            
        }
        public struct StatiVisibili
        {
            public int codice { get; set; }
            public string nomeStato { get; set; }

        }

        public enum  MaternitaCongediGradiAbil
        {
            VIS,
            GEST,
            ADM
        }

        public static bool EnabledToMaternitaCongedi(string matricola)
        {
            return myRaiHelper.AuthHelper.EnabledTo(matricola, "HRIS");
            //return (matricola == "451598" || matricola == "103650");
        }

        public static bool EnabledToMaternitaCongediUfficioConDirittiScrittura(MaternitaCongediUffici uff, string matricola =null)
        {
            if (String.IsNullOrWhiteSpace(matricola)) matricola = myRaiHelper.CommonHelper.GetCurrentUserMatricola();
            bool enabled =
               AuthHelper.EnabledToSubFunc(matricola, "HRIS", "0" + (int)uff + "GEST")
               ||
               AuthHelper.EnabledToSubFunc(matricola, "HRIS", "0" + (int)uff + "ADM");

            return enabled;
        }
        public static bool EnabledToMaternitaCongediDetail( MaternitaCongediUffici uff, MaternitaCongediGradiAbil abil, string matricola=null)
        {
            if (String.IsNullOrWhiteSpace(matricola)) matricola = myRaiHelper.CommonHelper.GetCurrentUserMatricola();
            return myRaiHelper.AuthHelper.EnabledToSubFunc(matricola, "HRIS", "0"+(int)uff + abil);
        }
        public static bool EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediUffici uff, string matricola = null)
        {
            if (String.IsNullOrWhiteSpace(matricola)) matricola = myRaiHelper.CommonHelper.GetCurrentUserMatricola();
            bool enabled =
                AuthHelper.EnabledToSubFunc(matricola, "HRIS", "0" + (int)uff + "VIS")
                ||
                AuthHelper.EnabledToSubFunc(matricola, "HRIS", "0" + (int)uff + "GEST")
                ||
                AuthHelper.EnabledToSubFunc(matricola, "HRIS", "0" + (int)uff + "ADM");
            return enabled;
        }
        public static List<string> EnabledtoMatricole(List<string> Matricole)
        {
            return Matricole;
        }
        public static List<string> GetEccezioniCongedi()
        {
            string s = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniCongedi);
            if (s != null)
                return s.Split(',').ToList();
            else
                return new List<string>();
        }
        public static bool RichiestaAttualmenteDaPrendereInCaricoMioUfficio(myRaiData.Incentivi.XR_MAT_RICHIESTE R)
        {
            bool esito =
                (EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediUffici.Gestione) &&
                 R.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).Select(x => x.ID_STATO).FirstOrDefault() == 10)
                 ||
                 (EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediUffici.Personale) &&
                 R.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).Select(x => x.ID_STATO).FirstOrDefault() == 30)
                  ||
                 (EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediUffici.Amministrazione) &&
                 R.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).Select(x => x.ID_STATO).FirstOrDefault() == 50);
            return esito;
        }
        public static bool RichiestaAttualmentePresaInCaricoMioUfficio(myRaiData.Incentivi.XR_MAT_RICHIESTE R)
        {
            bool esito =
                (EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediUffici.Gestione) &&
                 R.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).Select(x => x.ID_STATO).FirstOrDefault() == 20)
                 ||
                 (EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediUffici.Personale) &&
                 R.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).Select(x => x.ID_STATO).FirstOrDefault() == 40)
                  ||
                 (EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediUffici.Amministrazione) &&
                 R.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).Select(x => x.ID_STATO).FirstOrDefault() == 60);
            return esito;
        }
        public static bool RichiestaAttualmenteInCaricoAme(myRaiData.Incentivi.XR_MAT_RICHIESTE R)
        {
            var statoNow = R.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();
            if (statoNow.ID_STATO != 20 && statoNow.ID_STATO != 40 && statoNow.ID_STATO != 60)
                return false;
            else
                return statoNow.COD_USER == CommonHelper.GetCurrentUserMatricola();
        }
        public static bool RichiestaInCaricoAmeAnyTime(myRaiData.Incentivi.XR_MAT_RICHIESTE R)
        {
            string matr = CommonHelper.GetCurrentUserMatricola();
            return R.XR_WKF_MATCON_OPERSTATI.Any(x => (x.ID_STATO == 20 || x.ID_STATO == 40 || x.ID_STATO == 60) && x.COD_USER == matr);
        }
        public static bool RichiestaAttiva(myRaiData.Incentivi.XR_MAT_RICHIESTE R)
        {
            var statoNow = R.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();
            return statoNow.ID_STATO <= 80;
        }
    }
}
