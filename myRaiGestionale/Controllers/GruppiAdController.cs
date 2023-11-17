using myRai.Business;
using myRaiCommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public class GruppiAdController : Controller
    {
        //
        // GET: /GruppiAd/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Elenco_Gruppi()
        {
            List<GruppoAd> model = GruppiAdManager.GetGruppiAd();
            return View(model);
        }

        public ActionResult Elenco_Membri(string name)
        {
            GruppoAd model = GruppiAdManager.GetGruppoAd(name);
            return View(model);
        }

        public ActionResult Modal_Gruppo()
        {
            return View();
        }

        public ActionResult Modal_Mail()
        {
            return View();
        }
    }
}
