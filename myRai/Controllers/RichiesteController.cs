using System.Collections.Generic;
using System.Web.Mvc;
using myRai.Business;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using MyRaiServiceInterface.MyRaiServiceReference1;

namespace myRai.Controllers
{
    public class RichiesteController : BaseCommonController
    {
        ModelDash pr = new ModelDash();
        daApprovareModel daApprov;
        WSDigigapp datiBack = new WSDigigapp();
        WSDigigapp datiBack_ws1 = new WSDigigapp();
        MyRaiService1Client wcf1 = new MyRaiService1Client();

        public ActionResult Index()
        {
            SessionManager.Set(SessionVariables.AnnoFeriePermessi, null);
            wcf1.ClientCredentials.Windows.ClientCredential =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            //  wcf1.Credentials  =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            //  it.rai.servizi.hrpaga.HrPaga wsHrPaga = new it.rai.servizi.hrpaga.HrPaga();
            //  wsHrPaga.Credentials =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            //  it.rai.servizi.hrpaga.ListaDocumenti lista = wsHrPaga.ElencoDocumenti("01", "");

            //  Response.Write(lista.StringaErrore);

            //  digigappws_wcf1.wApiUtilitydipendente_resp resp = wcf1.recuperaUtente("103650", "01032017");
            //  resp.data.
            string userName = CommonManager.GetCurrentUsername();// System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            //  Response.Write(userName);
            datiBack.Credentials =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            datiBack_ws1.Credentials =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            Autorizzazioni.Sedi SEDI = new Autorizzazioni.Sedi();
            SEDI.Credentials =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );

            pr.menuSidebar = myRai.Models.Utente.getSidebarModel();// new sidebarModel(3);
            pr.MieRichieste = new List<MiaRichiesta>();
            pr.digiGAPP = true;
            //pr.dettaglioGiornata = HomeManager.GetTimbratureTodayModel();// new digigppws.dayResponse() { HideRefresh = false };
            //pr.dettaglioGiornata = new digigappws.dayResponse() { HideRefresh=false };
            pr.dettaglioGiornata = HomeManager.GetTimbratureTodayModel();
            pr.Raggruppamenti = HomeManager.GetRaggruppamenti();

            pr.ValidazioneGenericaEccezioni = CommonManager.GetParametro<string>(EnumParametriSistema.ValidazioneGenericaEccezioni);
            pr.SceltePercorso = HomeManager.GetSceltepercorsoModel("PR");

			//pr.statiModel = new StatiModel() { ListaStati = HomeManager.GetListaStati() };
            //pr.eccezioniModel = new EccezioniModel() {  ListaEccezioni =HomeManager.GetListaEccezioni()};
			//pr.presenzaDipendenti = HomeManager.GetPresenzaDipendenti();

			pr.statiModel = new StatiModel() { ListaStati = HomeManager.GetListaStati( CommonManager.GetCurrentUserMatricola() ) };
			pr.eccezioniModel = new EccezioniModel() { ListaEccezioni = HomeManager.GetListaEccezioni( CommonManager.GetCurrentUserMatricola() ) };

			return View(pr);
        }
    }
}