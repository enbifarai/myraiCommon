using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyRaiServiceInterface.MyRaiServiceReference1;
using MyRaiServiceInterface;
using myRaiHelper;
using myRai.Business;

namespace myRaiCommonManager
{
    public class ApprovazioniManager
    {
        public static GetDipendentiResponse GetDatiDipendentiAssenti(string matricola, string sedegapp, DateTime date1, DateTime date2)
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client client = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            client.ClientCredentials.Windows.ClientCredential =CommonHelper.GetUtenteServizioCredentials();
            GetDipendentiResponse response = client.getDipendentiPeriodo(matricola, sedegapp, date1, date2);
            if (response.Success == false) throw new Exception("GetdipendentiAssenti " + response.Error);
            
            var db = new myRaiData.digiGappEntities();
            var eccs = db.MyRai_Eccezioni_Ammesse.ToList();
            
            List<DataDip> LD = response.datadip.ToList();
            LD.RemoveAll(x => x.matricola == matricola);
            response.datadip = LD.ToArray();

            foreach (var d in response.datadip)
            {
                d.UrlFoto = CommonHelper.GetUrlFoto(d.matricola);
                for (int i = 0; i < d.eccezioni.Length; i++)
                {
                    var e = eccs.Where(x => x.cod_eccezione.Trim().ToUpper() == d.eccezioni[i].Trim().ToUpper())
                        .Select(x => x.desc_eccezione).FirstOrDefault();
                    if (e != null) d.eccezioni[i] = e;
                }
            }
            foreach (var d in response.datadipPresenti)
            {
                d.UrlFoto = CommonManager.GetUrlFoto(d.matricola);

            }

            return response;
        }
    }
    
    
}