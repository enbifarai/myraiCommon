using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace myRaiGestionale.Controllers
{
    public class NotificheController : Controller
    {
        //
        // GET: /Notifiche/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetPushPublicKey()
        {
            return Content("BIRBAd9SBKP9dtol7fXrinpOZ_uiLD5Bf8usPuPhD9Yr97sWaReOZCCY6_0FD4-9_svp7gaZyA4Vs26cpGdIOYk");
        }

        public ActionResult RegisterSubscription(NotificationSubscription subscription)
        {
            var db = new IncentiviEntities();
            db.XR_HRIS_NOTIFICHE_SUBSCRIPTION.Add(new XR_HRIS_NOTIFICHE_SUBSCRIPTION()
            {
                MATRICOLA = CommonHelper.GetCurrentUserMatricola(),
                IND_ACTIVE = true,
                SUBSCRIPTION = Newtonsoft.Json.JsonConvert.SerializeObject(subscription)
            });
            db.SaveChanges();

            return Content("OK");
        }

        public ActionResult RegisterUnSubscription(NotificationSubscription subscription)
        {
            var db = new IncentiviEntities();
            var sub = Newtonsoft.Json.JsonConvert.SerializeObject(subscription);
            var matricola = CommonHelper.GetCurrentUserMatricola();

            var dbSub = db.XR_HRIS_NOTIFICHE_SUBSCRIPTION.FirstOrDefault(x => x.MATRICOLA == matricola && x.SUBSCRIPTION == sub);
            if (dbSub != null)
                dbSub.IND_ACTIVE = false;
            db.SaveChanges();

            return Content("OK");
        }

        public ActionResult Test()
        {
            string result = "";
            try
            {
                var applicationID = "IstkpohQiWnRbQ5y_47SU3wEh_kOfpmCwAABZAapuOs";
                var senderId = "339310353624";

                var db = new IncentiviEntities();
                var matricola = CommonHelper.GetCurrentUserMatricola();
                var sub = db.XR_HRIS_NOTIFICHE_SUBSCRIPTION.FirstOrDefault(x => x.MATRICOLA == matricola);
                NotificationSubscription notSub = Newtonsoft.Json.JsonConvert.DeserializeObject<NotificationSubscription>(sub.SUBSCRIPTION);


                var request = new HttpRequestMessage(HttpMethod.Post, notSub.endpoint);
                request.Headers.Add("", "");

                request.Content = new StringContent("Questa è una notifica di prova");


            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Content(result);
        }
    }
}
