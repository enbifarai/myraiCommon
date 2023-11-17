using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public class TeaserModel
    {
        public string Url { get; set; }
    }
    public class FormazioneController : Controller //BaseCommonController
    {
        //
        // GET: /Formazione/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetTeaser(string video="")
        {
            if (String.IsNullOrWhiteSpace(video))
                video = "PresentazionePiattaforma";

            var db = new myRaiDataTalentia.TalentiaEntities();
            var param = db.XR_ACA_PARAM.FirstOrDefault(x =>x.COD_PARAM== "UrlTeaser" && x.COD_VALUE1 == video);
            if (param==null)
                return View("~/Views/Shared/404.cshtml");

            TeaserModel model = new TeaserModel();
            model.Url = param.COD_VALUE2;

            return View(model);
        }
    }
}
