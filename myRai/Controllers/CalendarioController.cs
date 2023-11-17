using System.Web.Mvc;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;

namespace myRai.Controllers
{
    public class CalendarioController : BaseCommonController
    {
        ModelDash pr = new ModelDash();

        WSDigigapp datiBack = new WSDigigapp();
        WSDigigapp datiBack_ws1 = new WSDigigapp();
        
        public ActionResult annuale()
        {
            string[] valori = CommonHelper.GetParametri<string>(EnumParametriSistema.OrariGapp);
            string userName = CommonHelper.GetCurrentUsername();

            datiBack.Credentials =new System.Net.NetworkCredential( CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            datiBack_ws1.Credentials =new System.Net.NetworkCredential( CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            pr.menuSidebar = UtenteHelper.getSidebarModel();// new sidebarModel(3);
            pr.Raggruppamenti = CommonHelper.GetRaggruppamenti();

            pr.dettaglioGiornata = HomeManager.GetTimbratureTodayModel();
            pr.digiGAPP = false;
            return View();
        }

        public ActionResult mensile()
        {
            return View();
        }

        public ActionResult pianoferie()
        {
            return View();
        }
    }
}