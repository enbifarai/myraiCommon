using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public class RichiesteController : BaseCommonController
    {
        //
        // GET: /Richieste/

        public ActionResult Index()
        {
            return View(new RichiestaLoader());
        }

        public ActionResult ElencoRichieste(RichiestaLoader loader=null)
        {
            RichContainer model = new RichContainer()
            {
                Loader = loader ?? new RichiestaLoader(),
                Richieste = AnagraficaManager.GetRichieste(loader)
            };

            return View(model);
        }

        public ActionResult Modal_Richiesta(string m, TipoRichiestaAnag tipo, int id)
        {
            switch (tipo)
            {
                case TipoRichiestaAnag.IBAN:
                    var richiesta = AnagraficaManager.GetRichiesta(m, tipo, id);
                    return View("~/Views/Anagrafica/subpartial/Modal_richiesta.cshtml", richiesta);
                    break;
                case TipoRichiestaAnag.VariazioneContrattuale:
                    var varContr = AnagraficaManager.GetRichiesta(m, tipo, id);
                    return View("~/Views/Anagrafica/subpartial/Modal_DatiContrattuali.cshtml", varContr.ObjInfo);
                case TipoRichiestaAnag.Congedo:
                    MaternitaDettagliGestioneModel model = myRaiCommonManager.MaternitaCongediManager.GetMaternitaDettagliGestioneModel(id);
                    return View("~/Views/maternitaCongedi/PopupVisGestContent", model);
                    break;
            }

            return Http404();
        }
    }
}
