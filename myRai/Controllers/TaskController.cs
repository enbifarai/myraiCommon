using myRaiHelper;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class TaskController : Controller
    {
        public ActionResult Index ( )
        {
            if ( !UtenteHelper.IsAdmin( CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                return RedirectToAction( "notauth" , "Home" );
            }
            return View( );
        }

        public ActionResult GeneraPdf ( )
        {
            string response = myRaiCommonTasks.CommonTasks.Task_GeneraPdfEccezioni( );
            return Content( response );
        }
    }
}