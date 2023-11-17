using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class GuidaController : Controller
    {
        
        public ActionResult Index(string viewhelp)
       {
            ViewData["viewhelp"] = viewhelp;
            return PartialView("_Index");
                       
        }

    }
}
