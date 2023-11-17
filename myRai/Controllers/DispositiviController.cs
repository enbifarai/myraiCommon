using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class DispositiviController: BaseCommonController
    {
        

        public ActionResult Index()
        {
            DispositiviModel model = GetDispositiviModel();
            return View(model);
        }


        [HttpPost]
        public ActionResult SendPush(int id)
        {
            string PushResult = "";
            var db = new myRaiData.digiGappEntities();
            var mobile=db.MyRai_MobileRegistration.Where(x => x.id == id).FirstOrDefault();
            if (mobile != null)
            {
                string apikey = CommonHelper.GetParametro<string>(EnumParametriSistema.PushAPIkey);
                if (! String.IsNullOrWhiteSpace(apikey))
                {
                    PushResult =DispositiviManager. SendPushNotification(apikey, mobile.token, "RaiPerMe", "Questa e' una notifica di prova dal sistema RaiPerme");
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = "OK", pushresult=PushResult}
            };
        }
        [HttpPost]
        public ActionResult disable(int id)
        {
            var db = new myRaiData.digiGappEntities();
            var mobile = db.MyRai_MobileRegistration.Where(x => x.id == id).FirstOrDefault();
            if (mobile != null)
            {
                mobile.abilitato = false;
                db.SaveChanges();
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = "OK"  }
            };
        }
        [HttpPost]
        public ActionResult enable(int id)
        {
            var db = new myRaiData.digiGappEntities();
            var mobile = db.MyRai_MobileRegistration.Where(x => x.id == id).FirstOrDefault();
            if (mobile != null)
            {
                mobile.abilitato = true;
                db.SaveChanges();
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = "OK" }
            };
        }

        public DispositiviModel GetDispositiviModel()
        {
            var db = new myRaiData.digiGappEntities();
            string pmatr = CommonHelper.GetCurrentUserPMatricola();
            DispositiviModel model = new DispositiviModel();

            model.Dispositivi = db.MyRai_MobileRegistration.Where(x => x.pmatricola == pmatr).OrderByDescending(x => x.last_access).ToList();
            return model;
        }
    }
   
}
