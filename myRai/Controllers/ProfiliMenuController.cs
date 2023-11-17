using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRai.DataAccess;
using myRaiCommonModel;
using myRaiHelper;

namespace myRai.Controllers
{
    public class ProfiliMenuController : BaseCommonController
    {
        public ActionResult Index()
        {
            return View(new myRaiData.digiGappEntities().MyRai_Profili.ToList());
        }

        public ActionResult datiProfilo(int id)
        {
            var db = new myRaiData.digiGappEntities();
            if (id != 0)
            {
                var p = db.MyRai_Profili.Where(x => x.id == id).FirstOrDefault();
                return View("datiProfilo", p);
            }
            else
                return View("datiProfilo", new myRaiData.MyRai_Profili()
                {
                    gruppo_AD = "*",
                    livello = "*"
                   ,
                    matricola = "*"
                });

        }
        
		public ActionResult datiMenu(int id)
        {
            var db = new myRaiData.digiGappEntities();
            var p = db.MyRai_Profili.Where(x => x.id == id).FirstOrDefault();
            ProfiloDatiMenuModel model = new ProfiloDatiMenuModel();
            foreach (var item in db.MyRai_Voci_Menu
                // .Where(x => x.customView != null && x.customView.Trim() != "")
                .OrderBy(z => z.customView)
                .ToList())
            {
                model.vociMenu.Add(new ProfiloVoceMenu()
                {
                    voceMenu = item,
                    Selected = p == null ? false : p.MyRai_Profili_Menu.Select(x => x .idmenu).Contains(item.ID)
                });
            }
            var mm= model.vociMenu.Where(x => x.voceMenu.Id_Padre != null).ToList();
            return View("datimenu", model);
        }
        
		public ActionResult delProfilo(int id)
        {
            var db = new myRaiData.digiGappEntities();
            var p = db.MyRai_Profili.Where(x => x.id == id).FirstOrDefault();
            if (p != null)
            {
                int[] ids = p.MyRai_Profili_Menu.Select(x => x.id).ToArray();

                foreach (var idp in ids)
                {
                    db.MyRai_Profili_Menu.Remove(db.MyRai_Profili_Menu.Find(idp));
                }

                db.MyRai_Profili.Remove(p);
                if (DBHelper.Save(db , CommonHelper.GetCurrentUserMatricola( ) ) )
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "OK" }
                    };
                }
                else
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Errore aggiornamento DB" }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = "Profilo non individuato" }
            };
        }
        
		public ActionResult modProfilo(myRaiData.MyRai_Profili model)
        {
            if (model.nome_profilo.Trim() == "") return Content("Nome profilo mancante");
            if (model.livello.Trim() != "*" && model.livello.Trim() != "1" && model.livello.Trim() != "2") return Content("Livello errato");
            if (model.matricola.Trim() != "" && ! model.matricola.Contains( "*") && model.matricola.Trim().Length < 6) return Content("Lunghezza matricola errata");

            var db = new myRaiData.digiGappEntities();

            if (model.id == 0 && db.MyRai_Profili.Any(x => x.nome_profilo.Trim() == model.nome_profilo.Trim()))
                return Content("Nome profilo già esistente");

            myRaiData.MyRai_Profili prof;
            if (model.id != 0)
                prof = db.MyRai_Profili.Where(x => x.id == model.id).FirstOrDefault();
            else
            {
                prof = new myRaiData.MyRai_Profili();
                db.MyRai_Profili.Add(prof);
            }

            if (prof != null)
            {
                prof.escludi_voci = model.escludi_voci;
                prof.gruppo_AD = String.IsNullOrWhiteSpace(model.gruppo_AD) ? "*" : model.gruppo_AD;
                prof.livello = String.IsNullOrWhiteSpace(model.livello) ? "*" : model.livello;
                prof.matricola = String.IsNullOrWhiteSpace(model.matricola) ? "*" : model.matricola;
                prof.nome_profilo = model.nome_profilo;
                prof.richiede_gapp = model.richiede_gapp;
                int[] ids = prof.MyRai_Profili_Menu.Select(x => x.id).ToArray();

                foreach (int id in ids)
                    if (id!=0) db.MyRai_Profili_Menu.Remove(db.MyRai_Profili_Menu.Find(id));

                if (HttpContext.Request.Form["vocemenu"] != null && HttpContext.Request.Form["vocemenu"].ToString() != "")
                {
                    string[] v = HttpContext.Request.Form["vocemenu"].Split(',');
                    foreach (string idmenu in v)
                    {
                        int idmen = Convert.ToInt32(idmenu);
                        prof.MyRai_Profili_Menu.Add(new myRaiData.MyRai_Profili_Menu()
                        {
                            MyRai_Profili = prof,
                            MyRai_Voci_Menu = db.MyRai_Voci_Menu.Find(idmen), 
                            idmenu = idmen,
                            idprofilo = prof.id
                        });
                    }
                }
                if (DBHelper.Save(db , CommonHelper.GetCurrentUserMatricola( ) ) )
                {
                    return Content("OK");
                }
                else
                    return Content("Errore salvataggio DB");

            }
            return Content("Errore DB");
        }
    }
}