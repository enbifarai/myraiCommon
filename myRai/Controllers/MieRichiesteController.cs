using myRaiCommonModel;
using myRaiHelper;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class MieRichiesteController : Controller
    {
        public ActionResult Index()
        {
            MieRichiesteTotaleModel model = new MieRichiesteTotaleModel();
            model.SidebarModel = UtenteHelper.getSidebarModel();// new sidebarModel(3);

            return View(model);
        }
    }
}