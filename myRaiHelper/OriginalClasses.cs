using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRai.Business
{
    public class CommonManager : myRaiHelper.CommonHelper //myRaiHelper.OriginalClasses.CommonManager
    {
        public static MyRai_InfoDipendente GetInfoDipendente(int tipologia)
        {
            return CommonHelper.GetInfoDipendente(CommonHelper.GetCurrentUserMatricola(), tipologia);
        }

        public static bool ShowElencoRichiesteFormazione()
        {
            return CommonHelper.ShowElencoRichiesteFormazione(CommonHelper.GetCurrentUserMatricola());
        }

        public static Boolean HasInfoDipendente(string tipologia)
        {
            return CommonHelper.HasInfoDipendente(tipologia, CommonHelper.GetCurrentUserMatricola());
        }

        public static Boolean HasInfoDipendente(int tipologia)
        {
            return CommonHelper.HasInfoDipendente(CommonHelper.GetCurrentUserMatricola(), tipologia);
        }

        public static Boolean HasInfoDipendente(int tipologia, bool checkDate = false, DateTime? D = null)
        {
            return CommonHelper.HasInfoDipendente(CommonHelper.GetCurrentUserMatricola(), tipologia, checkDate, D);
        }

        public static string GetIntervalloRMTRinsediamento(dayResponse R, string Orario_Importo = "O")
        {
            return EccezioniHelper.GetIntervalloRMTRinsediamento(R, Orario_Importo);
        }
    }

    public class EccezioniManager : EccezioniHelper
    {
        public static int calcolaMinuti(DateTime D)
        {
            return CommonHelper.calcolaMinuti(D);
        }

        public static int calcolaMinuti(string orarioHHMM)
        {
            return CommonHelper.calcolaMinuti(orarioHHMM);
        }
    }

    public class Utility
    {
        public static Dictionary<string, string> GetDictionaryFromJson(string json) => CommonHelper.GetDictionaryFromJson(json);
        public static List<Sede> GetSediGappLivello6() => CommonHelper.GetSediGappLivello6();
        public static List<Sede> GetSediGappResponsabileList() => CommonHelper.GetSediGappResponsabileList();
        public static List<Sede> GetSediGappResponsabileLiv2List() => CommonHelper.GetSediGappResponsabileLiv2List();
    }

    public class SessionManager
    {
        public static void Set(SessionVariables SessionName, object SessionValue) => SessionHelper.Set(SessionName, SessionValue);
        public static object Get(SessionVariables SessionName) => SessionHelper.Get(SessionName);

        public static void Set(string key, object value) => SessionHelper.Set(key, value);
        public static object Get(string key) => SessionHelper.Get(key);
    }

    public class FestivitaManager : myRaiHelper.FestivitaHelper
    {

    }
}

namespace myRai.Models
{
    public class Utente : UtenteHelper
    {
        public static bool IsBoss()
        {
            return UtenteHelper.IsBoss(CommonHelper.GetCurrentUserPMatricola());
        }
        public static bool IsBossLiv2()
        {
            return UtenteHelper.IsBossLiv2(CommonHelper.GetCurrentUserPMatricola());
        }
    }
}