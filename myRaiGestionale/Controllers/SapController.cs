using myRaiCommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public class SapController : Controller
    {
        //
        // GET: /Tech/

        public ActionResult Index()
        {
            string matricole = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.SapTestMatricole);
            if (String.IsNullOrWhiteSpace(matricole) || ! matricole .Split (',').Contains(myRaiHelper.CommonHelper.GetCurrentUserMatricola()))
            {
                return RedirectToAction("notauth", "home");
            }
            SapTestModel model = new SapTestModel();
            string[] urls = myRaiHelper.CommonHelper.GetParametri<string>(myRaiHelper.EnumParametriSistema.SapTestUrl);
            model.url1 = urls[0];
            model.url2 = urls[1];
            return View(model);
        }

    }
}
