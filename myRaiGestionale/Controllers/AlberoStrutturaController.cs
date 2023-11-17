using System.Web.Mvc;
using myRaiCommonModel;
using myRaiCommonManager;
using myRaiHelper;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using myRaiCommonManager;

namespace myRaiGestionale.Controllers
{
    public class AlberoStrutturaController : BaseCommonController
    {
        public ActionResult Index(int? idRoot)
        {
            return View("Index");
        }
        public ActionResult ShowAlberoExt(int idRoot)
        {
            return View("ShowAlberoExt", idRoot);
        }

        public JsonResult ShowAlberoByModal(int? idRoot, bool viewEmployee=false)
        {
            NodeModel vm = new NodeModel();
            OrganizzazioneManager organizzazione = new OrganizzazioneManager();
            string jsonNodes;
            var nodeTree = organizzazione.getAlbero(idRoot, IncarichiManager.GetIncarichiDBContext(), viewEmployee);
            jsonNodes = JsonConvert.SerializeObject(nodeTree);

            return Json(new { jsonNodes = nodeTree }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateStrutturaGrafo(NodeModel model)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            try
            {
                var updateGrafo = db.XR_STR_TALBERO.First(x => x.id == model.id);
                if (updateGrafo != null)
                {
                    updateGrafo.subordinato_a = model.pid;
                    updateGrafo.id = model.id;
                    db.Entry(updateGrafo);
                    db.SaveChanges();
                }
                else
                {
                    return new EmptyResult();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new EmptyResult();
        }

    }

}
