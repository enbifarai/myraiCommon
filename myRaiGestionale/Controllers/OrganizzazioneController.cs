using myRaiCommonModel.Gestionale;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public class OrganizzazioneController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetIncarichiAllExt(int idsezione, string data)
        {
            IncarichiTreeModel model = null;
            string key = "GetIncarichiAllExt";
            OggettoPerSessioneOrganizzazione sessionObj = new OggettoPerSessioneOrganizzazione();

            try
            {
                var cercaInSessione = SessionHelper.Get(key);
                if (cercaInSessione != null)
                {
                    sessionObj = (OggettoPerSessioneOrganizzazione)cercaInSessione;
                    DateTime ora = DateTime.Today;
                    TimeSpan span = ora.Subtract(sessionObj.Data);
                    if (span.TotalMinutes <= 15)
                    {
                        model = sessionObj.Obj;
                    }
                }
            }
            catch (Exception ex)
            {
                model = null;
            }

            if (model == null)
            {
                StrutturaOrganizzativaController s = new StrutturaOrganizzativaController();

                model = s.GetModelIncarichi(idsezione, data);

                sessionObj.Obj = model;
                sessionObj.Data = DateTime.Today;

                SessionHelper.Set(key, sessionObj);
            }

            return View("~/Views/strutturaorganizzativa/_tableincallExt", model);
        }
    }
}
