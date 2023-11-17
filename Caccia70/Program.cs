using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caccia70
{
    class Program
    {
        static string IsGiornalista(string matr)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            var g = db.SINTESI1.Any(x => x.COD_MATLIBROMAT == matr && x.DES_QUALIFICA.StartsWith("M"));
            return (g ? "G" : "");
        }
        static void Main(string[] args)
        {
            var db = new myRaiData.digiGappEntities();
            DateTime D = new DateTime(2022, 1,17);
            var Modifiche70 = db.MyRai_LogDB.Where(x => x.Data >= D && 
            (x.Modifiche.Trim()== "id_stato:20-->70," || x.Modifiche.Trim() == "id_stato:10-->70,")
            )
                .OrderBy(x => x.Data)
                .ToList();

            Modifiche70 = Modifiche70.Where(x => x.NomeTabella == "MyRai_Eccezioni_Richieste").ToList();

            it.rai.intranet.digigappwshis2016vip.WSDigigapp Service = new it.rai.intranet.digigappwshis2016vip.WSDigigapp();
            Service.Credentials = new System.Net.NetworkCredential("SRVRUOFPO", "zaq22?mk", "RAI");
            myRaiCommonTasks.CommonTasks.Log(" /DataUpd-70/DataInserita/IDeccezioneRichiesta/Matr/Giornalista/Sede/Data/Eccezione/Ndoc/GAPPstessoNDOC/StornoGAPP/StornoRPM/GAPPaltroNDOC/AltroNDOC/StatoGetEccBlank/StatoGetEccClassic/Errato");

            int counter = 0;
            foreach (var Mod in Modifiche70)
            {
                counter++;
                if (counter > 10000)
                {
                    break;
                }
                Console.WriteLine(counter);
                var EccRic = db.MyRai_Eccezioni_Richieste.Where(x => x.id_eccezioni_richieste == Mod.IdTabella).FirstOrDefault();
                if (EccRic == null)
                {
                    myRaiCommonTasks.CommonTasks.Log(Mod.IdTabella + " ; " + " ; non trovata nel DB");
                    continue;
                }

                var dayResponse = Service.getEccezioni(EccRic.MyRai_Richieste.matricola_richiesta,
                                    EccRic.data_eccezione.ToString("ddMMyyyy"),
                                    "",
                                    80);
                var dayResponseClassic = Service.getEccezioni(EccRic.MyRai_Richieste.matricola_richiesta,
                                    EccRic.data_eccezione.ToString("ddMMyyyy"),
                                    "BU",
                                    80);
                 
                string Linea ="/"+Mod.Data.ToString("ddMMyyyy HH.mm.ss.fff")+"/"+EccRic.data_creazione.ToString("ddMMyyyy HH.mm.ss.fff")+
                    "/" +Mod.IdTabella + "/"+ EccRic.MyRai_Richieste.matricola_richiesta+
                    "/"+IsGiornalista(EccRic.MyRai_Richieste.matricola_richiesta) +
                    "/"+EccRic.codice_sede_gapp + "/" + EccRic.data_eccezione.ToString("ddMMyyyy") +"/"+ EccRic.cod_eccezione.Trim() + "/" + EccRic.numero_documento + "/";

                bool PresenteGAPPstessoNDOC = true;

                if (dayResponse.eccezioni == null)
                {
                    PresenteGAPPstessoNDOC = false;
                }

                if (dayResponse.eccezioni==null || !dayResponse.eccezioni.Any(x => x.cod.Trim() == EccRic.cod_eccezione.Trim() &&
                                                   Convert.ToInt32(x.ndoc) == EccRic.numero_documento))
                {
                    PresenteGAPPstessoNDOC = false;
                }

                bool StornataGAPP = dayResponse.eccezioni!= null && dayResponse.eccezioni.Any(x => x.cod.Trim() == EccRic.cod_eccezione.Trim() &&
                                                     x.flg_storno!=null && x.flg_storno.Trim() == "*");

                bool StornataRaiPerMe = EccRic.MyRai_Richieste.MyRai_Eccezioni_Richieste.Any(x => x.azione == "C" &&
                                       x.numero_documento_riferimento == EccRic.numero_documento && x.id_stato==20);

                it.rai.intranet.digigappwshis2016vip.Eccezione div = null;
                if (dayResponse.eccezioni != null)
                {
                    div = dayResponse.eccezioni.Where(x => x.cod.Trim() == EccRic.cod_eccezione.Trim() &&
                                                     Convert.ToInt32(x.ndoc) != EccRic.numero_documento)
                                                     .FirstOrDefault();
                }
                   
                bool PresenteGappDiversoNDOC =( div != null);
                string DiversoNDOC = "";
                if (PresenteGappDiversoNDOC)
                {
                    DiversoNDOC = div.ndoc;
                }
                string statusBlank = "";
                if (dayResponse.eccezioni != null)
                {
                    foreach (var e in dayResponse.eccezioni.Where(x => x.cod.Trim() == EccRic.cod_eccezione.Trim()))
                    {
                        statusBlank += e.cod.Trim() + "," + e.ndoc + "," + e.flg_storno + " - ";
                    }
                }
              
                string statusClassic = "";
                bool Errato = false;
                if (dayResponseClassic.eccezioni != null)
                {
                    foreach (var e in dayResponseClassic.eccezioni.Where(x => x.cod.Trim() == EccRic.cod_eccezione.Trim()))
                    {
                        statusClassic += e.cod.Trim() + "," + e.ndoc + "," + e.flg_storno + " - ";
                        if (Convert.ToInt32(e.ndoc) == EccRic.numero_documento) Errato = true;
                    }
                }
                

                myRaiCommonTasks.CommonTasks.Log(Linea + PresenteGAPPstessoNDOC + "/" + StornataGAPP + "/" + StornataRaiPerMe + "/" +
                    PresenteGappDiversoNDOC + "/" + DiversoNDOC+"/"+ statusBlank + "/"+statusClassic + "/" + Errato);

            }

        }
    }
}
