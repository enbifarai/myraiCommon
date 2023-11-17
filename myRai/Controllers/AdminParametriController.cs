using myRaiCommonModel;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class AdminParametriController : Controller
    {
     
        // GET: /AdminParametri/

        public ActionResult Index()
        {
            Parametri parametri = new Parametri();
            if (UtenteHelper.IsAdmin(CommonHelper.GetCurrentUserMatricola()))
            {
                parametri.GetListItem();
            }
            return View(parametri);
        }
        public ActionResult GetDettaglioParametro(long id)
        {
            Parametri parametro = new Parametri();
            List<Parametri> vm=  parametro.GetDettaglioParametro(id);
           
           
            return PartialView("GetDettaglioParametro", vm);
        }
        public ActionResult InsertCategoriaParametro(long id, string categoria, string chiave)
        {
            try
            {
                using (var db = new myRaiData.digiGappEntities())
                {
                    var data = db.MyRai_ParametriSistema.Find(id);
                    data.Categoria_Parametri = categoria;
                    db.Entry(data).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                }
            }catch(Exception ex)
            {
                throw ex;
            } 
                return View("");
        }
    }
}
