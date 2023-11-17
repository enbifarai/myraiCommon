using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRai.Business;
using myRai.Models;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;

namespace myRai.Controllers
{
    public class RicercaController : BaseCommonController
    {
        //
        // GET: /Ricerca/

        ModelDash model = new ModelDash();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RicercaEccezioni()
        {
            var db = new digiGappEntities();

            model.RicercaGen = new Ricerca();
            model.RicercaGen.ListaSedi = new List<Ricerca.Sede>();

            Abilitazioni AB = CommonManager.getAbilitazioni();
            var Lsedi=AB.ListaAbilitazioni.Where(x => x.MatrLivello1.Any(a => a.Matricola == CommonManager.GetCurrentUserPMatricola()));
            
            foreach (var s in Lsedi)
            {
                Ricerca.Sede sede = new Ricerca.Sede();

                sede.CodiceSede = s.Sede;

                if (db.MyRai_Richieste.Where(x => x.codice_sede_gapp == sede.CodiceSede && (
                  x.id_stato.Equals((int)EnumStatiRichiesta.InApprovazione) || x.id_stato.Equals((int)EnumStatiRichiesta.Approvata) || x.id_stato.Equals((int)EnumStatiRichiesta.Rifiutata))).Count() > 0 &&
                    !model.RicercaGen.ListaSedi.Any(x => x.CodiceSede.Contains(sede.CodiceSede)))
                {
                    sede.DescrizioneSede = s.DescrSede;
                    model.RicercaGen.ListaSedi.Add(sede);
                }
            }

            //foreach (var profilo in Utente.ListaProfili().ProfiliArray)
            //{
            //    foreach (System.Data.DataRow cat in profilo.DT_CategorieDatoAbilitate.Rows)
            //    {
            //        Ricerca.Sede sede = new Ricerca.Sede();

            //        sede.CodiceSede = cat["Codice_categoria_dato"].ToString();

            //        if (db.MyRai_Richieste.Where(x => x.codice_sede_gapp == sede.CodiceSede && (
            //          x.id_stato.Equals((int)EnumStatiRichiesta.InApprovazione) || x.id_stato.Equals((int)EnumStatiRichiesta.Approvata) || x.id_stato.Equals((int)EnumStatiRichiesta.Rifiutata))).Count() > 0 &&
            //            !model.RicercaGen.ListaSedi.Any(x => x.CodiceSede.Contains(sede.CodiceSede)))
            //        {
            //            sede.DescrizioneSede = cat["Descrizione_categoria_dato"].ToString();
            //            model.RicercaGen.ListaSedi.Add(sede);
            //        }

            //        //model.elencoProfilieSedi = new daApprovareModel(Utente.ListaProfili(),false, "01","", 0 );
            //    }
            //}

            model.menuSidebar = Utente.getSidebarModel();// new sidebarModel(3);
            return View("~/Views/Responsabile/RicercaEccezioni.cshtml", model);
        }

        public ActionResult RicercaEccezioniUtente()
        {
            var db = new digiGappEntities();

            model.RicercaGen = new Ricerca();
            model.RicercaGen.ListaSedi = new List<Ricerca.Sede>();
            model.RicercaGen.TipoEccezione = new List<String>();
            string matricola = CommonManager.GetCurrentUserMatricola();
            string sede = Utente.SedeGapp(DateTime.Now);


            //foreach (var profilo in Utente.ListaProfili().ProfiliArray)
            //{
            //    foreach (System.Data.DataRow cat in profilo.DT_CategorieDatoAbilitate.Rows)
            //    {
            //        Ricerca.Sede sede = new Ricerca.Sede();

            //        sede.CodiceSede = cat["Codice_categoria_dato"].ToString();

            //        if (db.MyRai_Richieste.Where(x => x.codice_sede_gapp == sede.CodiceSede && (
            //          x.id_stato.Equals((int)EnumStatiRichiesta.InApprovazione) || x.id_stato.Equals((int)EnumStatiRichiesta.Approvata) || x.id_stato.Equals((int)EnumStatiRichiesta.Rifiutata))).Count() > 0 &&
            //            !model.RicercaGen.ListaSedi.Any(x => x.CodiceSede.Contains(sede.CodiceSede)))
            //        {
            //            sede.DescrizioneSede = cat["Descrizione_categoria_dato"].ToString();
            //            model.RicercaGen.ListaSedi.Add(sede);
            //        }

            //        //model.elencoProfilieSedi = new daApprovareModel(Utente.ListaProfili(),false, "01","", 0 );
            //    }
            //}

            var asd = db.MyRai_Richieste.Where(x => x.matricola_richiesta == matricola && x.codice_sede_gapp == sede)
                 .Where(x => x.id_stato.Equals((int)EnumStatiRichiesta.InApprovazione)
                     || x.id_stato.Equals((int)EnumStatiRichiesta.Approvata)
                     || x.id_stato.Equals((int)EnumStatiRichiesta.Rifiutata)).Take(100).ToList();

            model.RicercaGen.TipoEccezione = db.MyRai_Richieste.Where(x => x.matricola_richiesta == matricola && x.codice_sede_gapp == sede)
                 .Where(x => x.id_stato.Equals((int)EnumStatiRichiesta.InApprovazione) || x.id_stato.Equals((int)EnumStatiRichiesta.Approvata) || x.id_stato.Equals((int)EnumStatiRichiesta.Rifiutata))
                //.Select(x => x.MyRai_Eccezioni_Richieste.Select(z=>z.descrizione_eccezione)).ToList();
                 .SelectMany(x => x.MyRai_Eccezioni_Richieste.Select(z => z.cod_eccezione))
                 .Distinct().ToList();


            //model.RicercaGen.TipoEccezione = db.MyRai_Eccezioni_Richieste.


            //var listent = (from r in db.MyRai_Richieste 
            //               join e in db.MyRai_Eccezioni_Richieste 
            //               on r.


            model.menuSidebar = Utente.getSidebarModel();// new sidebarModel(3);
            return View("~/Views/MieRichieste/RicercaEccezioniUtente.cshtml", model);
        }
    }
}