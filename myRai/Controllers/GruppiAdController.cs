using myRai.Business;
using myRai.Models;
using myRaiCommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class GruppiAdController : Controller
    {
        //
        // GET: /GruppiAd/

        public ActionResult Index()
        {
            return View(new UserAdSearch());
        }

        public ActionResult Elenco_GruppiAd()
        {
            var model = GruppiAdManager.GetGruppiAd();
            
            return View("subpartial/Elenco_GruppiAd", model);
        }

        public ActionResult Dettaglio_GruppiAd(string name)
        {
            var model = GruppiAdManager.GetGruppoAd(name);

            return View("subpartial/Dettaglio_GruppiAd", model);
        }

        public ActionResult RicercaDipendenti(UserAdSearch model)
        {
            return View("subpartial/Elenco_Dip", GruppiAdManager.GetListDip(model));
        }

        public ActionResult AssociaDipendenti(string nomeGruppo, string elencoMatr)
        {
            if (GruppiAdManager.AggiungiDipendenti(nomeGruppo, elencoMatr, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }
        public ActionResult DiassociaDipendenti(string nomeGruppo, string elencoMatr)
        {
            var result = GruppiAdManager.RimuoviDipendenti(nomeGruppo, elencoMatr, out string errorMsg);

            return Content("OK");
        }
    }
}
