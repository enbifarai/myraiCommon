using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using System;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class SediController : Controller
    {
        public ActionResult Index(string searchName, string searchDesc, string searchCal)
        {
            if ( !UtenteHelper.IsAdmin( CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                throw new Exception( "Non autorizzato" );
            }
            SediGappModel model = SediGappManager.GetSediGappModel(searchName, searchDesc, searchCal);
            return View(model);
        }

        public ActionResult getsede(string sede)
        {
            if (!UtenteHelper.IsAdmin( CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                throw new Exception("Non autorizzato");
            }
            var sedeHrdw = SediGappManager.GetSedeHrdw(sede);

            if (sedeHrdw == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "Sede non trovata" }
                };
            }

            try
            {
                SediGappManager.AddSede(sedeHrdw);
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

        public ActionResult CheckConv(string datacheck, string sede)
        {
            DateTime d;
            if (!DateTime.TryParseExact(datacheck, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out d))
            {
                return new JsonResult
                   {
                       JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                       Data = new { result = "Data non valida" }
                   };
            }
            string[] utenteConv = CommonHelper.GetParametri<string>(EnumParametriSistema.UtentePerConvalida);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = myRaiCommonTasks.CommonTasks.VerificaGiornoConvalidato(d, utenteConv[0], utenteConv[1], sede)
                }
            };

        }

        public ActionResult GetSedeDB(int id)
        {
            SedeGappModel model = new SedeGappModel();
            model.sedeDB = new L2D_SEDE_GAPP_Model();

            var db = new myRaiData.digiGappEntities();
            var sDB = db.L2D_SEDE_GAPP.Find(id);
            if (sDB != null)
            {
                CommonHelper.Copy(model.sedeDB, sDB);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult UpdateSede(SedeGappModel model)
        {
            var db = new myRaiData.digiGappEntities();
            var sede = db.L2D_SEDE_GAPP.Find(model.sedeDB.id);

            sede.CalendarioDiSede = model.sedeDB.CalendarioDiSede;
            sede.Data_Patrono = model.sedeDB.Data_Patrono;
            sede.giorno_inizio_settimana = model.sedeDB.giorno_inizio_settimana;
            sede.importo_rimborso = model.sedeDB.importo_rimborso;
            sede.intervallo_mensa = model.sedeDB.intervallo_mensa;
            sede.intervallo_mensa_serale = model.sedeDB.intervallo_mensa_serale;
            sede.mensa_disponibile = model.sedeDB.mensa_disponibile == null ? "" : model.sedeDB.mensa_disponibile;
            sede.minimo_car = model.sedeDB.minimo_car;
            sede.periodo_mensa = model.sedeDB.periodo_mensa;
            sede.RMTR_intervallo = model.sedeDB.RMTR_intervallo;

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
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "UpdateSede"
                });
                return new JsonResult
               {
                   JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                   Data = new { result = ex.Message }
               };
            }

        }
    }


}
