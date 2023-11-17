using myRai.Business;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class NewsController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if ( !UtenteHelper.IsAdmin( CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                filterContext.Result = new RedirectResult( "/Home/notauth" );
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index()
        {
            NewsEditorModel model = NewsManager.GetNewsEditorModel();
            return View(model);
        }
        public static List<SelectListItem> GetCategorie(string categoria)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = "Release notes" , Value= "Release notes" });
            items.Add(new SelectListItem() { Text = "Main news", Value = "Main news" });
            switch (categoria)
            {
                case "categorie":
                    items.Select(x => new SelectListItem { Value = x.Text, Text = x.Text });
                    break;
                default:
                    break;
            }

            return items;
        }
        public ActionResult AddNews()
        {
            MyRai_News_Shadow model = new MyRai_News_Shadow();
            model.contenuto = CommonManager.GetParametro<String>(EnumParametriSistema.NewsTemplate);

            return View(model);
        }

        public ActionResult DelNews(int id)
        {
            var db = new myRaiData.digiGappEntities();
            var n = db.MyRai_News.Find(id);

            if (n==null)
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "Errore DB" }
                };

            n.MyRai_News_Lette.Clear();
            db.MyRai_News.Remove(n);
            try
            {
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "ok" }
                };
            }
            catch (Exception ex)
            {
                 return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }
        }

        public ActionResult ModNews(int id)
        {
            var db = new myRaiData.digiGappEntities();
            var n = db.MyRai_News.Find(id);
            MyRai_News_Shadow model = new MyRai_News_Shadow()
            {
                contenuto = n.contenuto,
                destinatari_any = n.destinatari_any,
                priorita = n.priorita,
                destinatari_L1 = n.destinatari_L1,
                destinatari_L2 = n.destinatari_L2,
                destinatari_matricole = n.destinatari_matricole,
                destinatari_sedigapp = n.destinatari_sedigapp,
                destinatari_tipodip = n.destinatari_tipodip,
                controllo_aggiuntivo = n.controllo_aggiuntivo,
                validita_inizio = n.validita_inizio!=null?n.validita_inizio.Value.ToString("dd/MM/yyyy"):null,
                validita_fine = n.validita_fine!=null?n.validita_fine.Value.ToString("dd/MM/yyyy"):null,
                id = n.id,
                categoria = n.categoria,
                titolo = n.titolo,

            };

            return View("addnews",model);
        }

        [ValidateInput(false)]
        public ActionResult SaveNews(MyRai_News_Shadow model)
        {
            var db = new myRaiData.digiGappEntities();
            if (model.tipodest == 4 && String.IsNullOrWhiteSpace(model.destinatari_sedigapp))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "Sedi gapp non indicate" }
                };
            }
            if (model.tipodest == 5 && String.IsNullOrWhiteSpace(model.destinatari_matricole))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "Matricole non indicate" }
                };
            }
            if (model.tipodest == 6 && String.IsNullOrWhiteSpace(model.destinatari_tipodip))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "Tipi dipendente non indicati" }
                };
            }
            myRaiData.MyRai_News n = new myRaiData.MyRai_News();

            if (model.id != 0)
            {
                n = db.MyRai_News.Find(model.id);
                if (n == null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Errore DB, news non trovata" }
                    };
                }

            }
            else
                db.MyRai_News.Add(n);


            n.contenuto = model.contenuto;
            n.data_info = DateTime.Now;
            n.priorita = model.priorita;
            n.destinatari_any = (model.tipodest == 1);
            n.destinatari_L1 = model.tipodest == 2;
            n.destinatari_L2 = model.tipodest == 3;
            n.categoria = model.categoria;
            n.titolo = model.titolo;

            if (model.tipodest == 4)
                n.destinatari_sedigapp = string.IsNullOrWhiteSpace(model.destinatari_sedigapp) ? null : model.destinatari_sedigapp;
            else
                n.destinatari_sedigapp = null;

            if (model.tipodest == 5)
                n.destinatari_matricole = string.IsNullOrWhiteSpace(model.destinatari_matricole) ? null : model.destinatari_matricole;
            else
                n.destinatari_matricole = null;

            if (model.tipodest == 6)
                n.destinatari_tipodip = string.IsNullOrWhiteSpace(model.destinatari_tipodip) ? null : model.destinatari_tipodip;
            else
                n.destinatari_tipodip = null;

            n.controllo_aggiuntivo = string.IsNullOrWhiteSpace(model.controllo_aggiuntivo) ? null : model.controllo_aggiuntivo;
            
            if (string.IsNullOrWhiteSpace(model.validita_inizio))
                n.validita_inizio = null;
            else
            {
                string[] dateSplit = model.validita_inizio.Split('/');
                n.validita_inizio = new DateTime(Convert.ToInt32(dateSplit[2]), Convert.ToInt32(dateSplit[1]), Convert.ToInt32(dateSplit[0]));
            }

            if (string.IsNullOrWhiteSpace(model.validita_fine))
                n.validita_fine = null;
            else
            {
                string[] dateSplit = model.validita_fine.Split('/');
                n.validita_fine = new DateTime(Convert.ToInt32(dateSplit[2]), Convert.ToInt32(dateSplit[1]), Convert.ToInt32(dateSplit[0]));
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = "ok" }
            };
        }
    }
}