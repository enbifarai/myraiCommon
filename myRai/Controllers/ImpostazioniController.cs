using myRaiCommonManager;
using myRaiCommonModel;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class ImpostazioniController : Controller
    {
        //
        // GET: /Impostazioni/

        public ActionResult Index()
        {
            ProfiloPersonaleModel model = ProfiloPersonaleManager.GetProfiloPersonaleModel();
            return View(model);
        }

    }
}
