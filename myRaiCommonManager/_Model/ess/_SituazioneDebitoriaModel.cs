using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.ess
{
    public class _SituazioneDebitoriaModel
    {

        public List<Debito> situazioneDebitoria = new List<Debito>();
        public _SituazioneDebitoriaModel()
        {

            myRaiServiceHub.it.rai.servizi.wiahrss.ezService ezS = new myRaiServiceHub.it.rai.servizi.wiahrss.ezService();
            ezS.Credentials = new System.Net.NetworkCredential();
            string strAppoFunzione = "", strFunzione = ezS.ElencoSituazioneDebitoria();
            string strACK = strFunzione.Substring(0, 10);
            int iSpazio = (Convert.ToInt16(strFunzione.Substring(5, 5)) + 10) - strFunzione.Length;
            if (iSpazio > 0)
            {
                strAppoFunzione = (strAppoFunzione + strFunzione.Substring(10)).PadRight(iSpazio);
            }
            else
            {
                strAppoFunzione = (strAppoFunzione + strFunzione.Substring(10, Convert.ToInt16(strACK.Substring(5, 5))));
            }
            string strFunzioneDeb = strAppoFunzione.Substring(360);
            string appoanno = "", appomese = "";
            int iNum = strFunzioneDeb.Length / 40;
            strFunzioneDeb = strFunzioneDeb + " ";

            for (int i = 1; i <= iNum; i++)
            {
                Debito appoDebito = new Debito();
                appoDebito.descrittiva = strFunzioneDeb.Substring(0, 20);
                appoDebito.addebito = strFunzioneDeb.Substring(23, 11);
                if ((strFunzioneDeb.Substring(36, 2) != "00") || (strFunzioneDeb.Substring(36, 2) != ""))
                {
                    appomese = strFunzioneDeb.Substring(36, 2);
                }

                if (Convert.ToInt16(strFunzioneDeb.Substring(39, 2)) > 40)
                {
                    appoanno = "19" + strFunzioneDeb.Substring(39, 2);
                }
                else
                {
                    appoanno = "20" + strFunzioneDeb.Substring(39, 2);
                }
                appoDebito.datainizio = appoanno + appomese;
                appoDebito.periodo = appomese + "." + appoanno + " - ";
                if ((strFunzioneDeb.Substring(43, 2) != "00") || (strFunzioneDeb.Substring(43, 2) != ""))
                {
                    appomese = strFunzioneDeb.Substring(43, 2);
                }

                if (Convert.ToInt16(strFunzioneDeb.Substring(46, 2)) > 40)
                {
                    appoanno = "19" + strFunzioneDeb.Substring(46, 2);
                }
                else
                {
                    appoanno = "20" + strFunzioneDeb.Substring(46, 2);
                }
                appoDebito.datafine = appoanno + appomese;

                appoDebito.periodo = appoDebito.periodo + appomese + "." + appoanno ;

                appoDebito.numeroratetotali = strFunzioneDeb.Substring(50, 3);
                appoDebito.importorata = strFunzioneDeb.Substring(54, 10);
                appoDebito.numerorateresidue = strFunzioneDeb.Substring(66, 3);
                appoDebito.importoresiduo = strFunzioneDeb.Substring(69, 11);
              
                appoDebito.numeroratepagate = (Convert.ToInt16(appoDebito.numeroratetotali) - Convert.ToInt16(appoDebito.numerorateresidue)).ToString();
                appoDebito.percentuale = ((Convert.ToInt16(appoDebito.numeroratepagate) * 100) / Convert.ToInt16(appoDebito.numeroratetotali)).ToString();

                situazioneDebitoria.Add(appoDebito);
                if (strFunzioneDeb.Length > 90)
                {
                    strFunzioneDeb = strFunzioneDeb.Substring(80);
                }
                else
                { break; }
            }
        }

        public struct Debito
        {
            public string descrittiva;
            public string addebito;
            public string datainizio;
            public string datafine;
            public string numeroratetotali;
            public string importorata;
            public string numerorateresidue;
            public string importoresiduo;
            public string numeroratepagate;
            public string percentuale;
            public string periodo;
        }
    }
}