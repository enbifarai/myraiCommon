using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class ApprovazioneAttivitaController :BaseCommonController
    {
        public ActionResult Index()
        {
           ApprovazioniAttivitaModel model = new ApprovazioniAttivitaModel() { IsPreview=true };
           return View(model);
        }

        public ActionResult GetDaApprAtt(string nome, string data_da,string data_a, string stato,string titolo,
            string eccezione)
        {
            ApprovazioniAttivitaModel model = CeitonManager.GetApprovazioneAttivitaModel(nome,data_da,data_a,stato,titolo,eccezione);
            model.IsPreview = false;
            return View("~/Views/approvazioneAttivita/subpartial/da_approvare.cshtml" , model);
        }
    }
}