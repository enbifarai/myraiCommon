using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using myRaiData;
using System.Collections.Generic;
using MyRaiWindowsService1.sendMail;

namespace MyRaiWindowsService1
{
    public class Common
    {
        public static T GetParametro<T>(EnumParametriSistema chiave)
        {
            var db = new digiGappEntities();

            String NomeParametro = chiave.ToString();
            MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == NomeParametro);
            if (p == null) return default(T);
            else return (T)Convert.ChangeType(p.Valore1, typeof(T));

        }

        public static T[] GetParametri<T>(EnumParametriSistema chiave)
        {
            using (digiGappEntities db = new digiGappEntities())
            {
                String NomeParametro = chiave.ToString();
                MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == NomeParametro);
                if (p == null) return null;
                else
                {
                    T[] parametri = new T[] { (T)Convert.ChangeType(p.Valore1, typeof(T)), (T)Convert.ChangeType(p.Valore2, typeof(T)) };
                    return parametri;
                }
            }
        }
        public static List<String> GetMatricolaLivelloPerSede(string sedegapp, int livelloResponsabile_1_2)
        {
            Autorizzazioni.Sedi service = new Autorizzazioni.Sedi();
            service.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

            Autorizzazioni.CategorieDatoAbilitate response = service.Get_CategoriaDato_Net("sedegapp", "", "HRUP", "0" + livelloResponsabile_1_2.ToString() + "GEST");
            var item = response.CategorieDatoAbilitate_Array
                .Where(sede => sede.Codice_categoria_dato.Trim().ToUpper() == sedegapp.Trim().ToUpper())
                       .FirstOrDefault();
            if (item == null) return null;

            List<string> Matricole = item.DT_Utenti_CategorieDatoAbilitate
                                         .AsEnumerable()
                                         .Select(p => (String)p.ItemArray[0]).ToList();
            return Matricole;
        }
        public static List<String> GetMatricolaLivelloPerSede(string[] sedegapp, int livelloResponsabile_1_2)
        {
            Autorizzazioni.Sedi service = new Autorizzazioni.Sedi();
            service.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

            Autorizzazioni.CategorieDatoAbilitate response = service.Get_CategoriaDato_Net("sedegapp", "", "HRUP", "0" + livelloResponsabile_1_2.ToString() + "GEST");
            List<string> Matricole = new List<string>();

            for (int x = 0; x < sedegapp.Count(); x++)
            {
                var item = response.CategorieDatoAbilitate_Array
                    .Where(sede => sede.Codice_categoria_dato.Trim().ToUpper() == sedegapp[x].Trim().ToUpper())
                           .FirstOrDefault();
                if (item == null) return null;

                Matricole = item.DT_Utenti_CategorieDatoAbilitate
                                             .AsEnumerable()
                                             .Select(p => (String)p.ItemArray[0]).ToList();

            }

            return Matricole;
        }
        public static String GetEmailMatricola(string matricola)
        {
            EsponiAnagrafica.Service service = new EsponiAnagrafica.Service();
            return service.EsponiAnagrafica("raicv;" + matricola + ";;E;0");
		}

		public static string[] RewriteAddress ( string emailAddress, string[] addressList, Email eml, string[] allowed )
		{
			if ( String.IsNullOrEmpty( emailAddress ) )
			{
				emailAddress = "ruo.sip.presidioopen@rai.it";
			}

			if ( addressList == null || addressList.Length == 0 ) return addressList;
			for ( int i = 0; i < addressList.Length; i++ )
			{
				if ( String.IsNullOrWhiteSpace( addressList[i] ) ) continue;
				if ( allowed.Select( x => x.Trim() ).Contains( addressList[i], StringComparer.InvariantCultureIgnoreCase ) ) continue;

				//sovrascrivi indirizzo e annota nella mail
				eml.Body = "<p>Reinstradato da sviluppo: " + addressList[i] + "</p>" + eml.Body;
				addressList[i] = emailAddress;
			}
			return addressList;
		}
    }
}
