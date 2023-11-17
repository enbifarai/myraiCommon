using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRaiDataTalentia;
using myRaiHelper;
using myRaiCommonModel.RaiAcademy;
using myRaiHelper;
using myRaiCommonModel;
using myRai.Data.CurriculumVitae;
using myRaiCommonModel.DataControllers.RaiAcademy;
using myRaiCommonManager;

namespace myRai.Controllers.RaiAcademy
{
    //Questo controller contiene le action relative alla pagina del catalogo
	public partial class RaiAcademyController : Controller
    {
        private void FiltraCorsi(RaiAcademyVM model)
        {
            var f = RaiAcademyControllerScope.Instance.Filtri;
            
            //Applico la ricerca per stringa
            if (!String.IsNullOrWhiteSpace(f.SearchString))
            {
                string tmpString = f.SearchString.ToUpper();
                var tmp = model.Corsi.Items.Where(x => x.Titolo.ToUpper().Contains(tmpString)
                                                                || x.Tematica.ToUpper().Contains(tmpString)
                                                                || x.AreaFormativa.ToUpper().Contains(tmpString)
                                                                || x.Descrizione.ToUpper().Contains(tmpString)
                                                                || x.MetodoFormativo.ToUpper().Contains(tmpString)
                                                                || x.CertificazioneOttenuta.ToUpper().Contains(tmpString)
                                                                || x.NomePortale.ToUpper().Contains(tmpString)
                                                                || x.ObiettiviEContenuti.Serve.ToUpper().Contains(tmpString)
                                                                || x.ObiettiviEContenuti.Imparerai.ToUpper().Contains(tmpString)
                                                                || x.Sede.ToUpper().Contains(tmpString)
                                                                || x.Target.ToUpper().Contains(tmpString)
                                                                || x.AltriDettagli.ArticolazioneCorso.ToUpper().Contains(tmpString)
                                                                || x.AltriDettagli.Docenti.ToUpper().Contains(tmpString)
                                                                || x.AltriDettagli.Note.ToUpper().Contains(tmpString)
                                                                || x.AltriDettagli.Requisiti.ToUpper().Contains(tmpString)
                                                                );
                    model.Corsi.Items= tmp.ToList();
            }

            //Applico l'ordinamento
            if (!String.IsNullOrWhiteSpace(f.OrderBy) && model.Corsi.Items.Count>0)
            {
                switch (f.OrderBy)
                {
                    case "Titolo del corso":
                        model.Corsi.Items = model.Corsi.Items.OrderBy(x => x.Titolo).ToList();
                        break;
                    case "Data avvio del corso":
                        model.Corsi.Items = model.Corsi.Items.OrderBy(x => x.DataInizioDisponibilita).ToList();
                        break;
                    default:
                        break;
                }
            }
        }
        
        public PartialViewResult Index(string cerca="", string area="", string tema="", string gruppo="")
        {
            FiltriCatalogo filtri = new FiltriCatalogo();
            filtri.Cerca = cerca;
            filtri.Area = area;
            filtri.Tema = tema;
            filtri.Gruppo = gruppo;

            bool raiAcademyBack = Convert.ToBoolean(System.Web.HttpContext.Current.Session["RaiAcademyBack"]);

            if (raiAcademyBack && String.IsNullOrWhiteSpace(cerca))
            {
                filtri.Cerca = RaiAcademyControllerScope.Instance.Filtri != null?RaiAcademyControllerScope.Instance.Filtri.SearchString:"";
            }

			return PartialView( "~/Views/RaiAcademy/Index.cshtml", filtri );
        }

        public PartialViewResult GetCatalogo(string cerca="", string area = "", string tema="", string gruppo="")
        {
            RaiAcademyVM model = new RaiAcademyVM();
            var result = this.dataController.GetCorsi(CommonHelper.GetCurrentUserMatricola(), false);
            //var result = this.dataController.GetCorsiCatalogo();

            if (result != null)
            {
                model.Corsi.Items = result.List;
                model.Corsi.PageSize = 9;
                model.Corsi.CurrentPage = 1;
                model.Corsi.TotalCourses = result.TotaleCorsi;
            }

            model.filtri = GetFiltri(model, area, tema, gruppo);

            if (!String.IsNullOrWhiteSpace(cerca))
            {
                model.filtri.SearchString = cerca;
                RaiAcademyControllerScope.Instance.Filtri = model.filtri;
                FiltraCorsi(model);
            }

            return PartialView("~/Views/RaiAcademy/subpartial/Catalogo.cshtml", model);
        }

        public PartialViewResult IndexTab(StatoCorsoTipoOffertaEnum stato)
        {

            RaiAcademyVM model = new RaiAcademyVM();
            var result = this.dataController.GetCorsi(CommonHelper.GetCurrentUserMatricola(), RaiAcademyControllerScope.Instance.Filtri != null
                                                    && !String.IsNullOrWhiteSpace(RaiAcademyControllerScope.Instance.Filtri.SearchString));

			if ( result != null )
			{
				model.Corsi.Items = result.List;
				model.Corsi.PageSize = 9;
				model.Corsi.CurrentPage = 1;
				model.Corsi.TotalCourses = result.TotaleCorsi;
			}

            if (RaiAcademyControllerScope.Instance.Filtri != null)
            {
                model.filtri = RaiAcademyControllerScope.Instance.Filtri;
                FiltraCorsi(model);
            }
            else
                model.filtri = GetFiltri(model);

            //model.filtri = GetFiltri(model);

            return PartialView("~/Views/RaiAcademy/ElencoCorsi.cshtml", model.Corsi);
        }

        public ActionResult DashBoard()
        {
            string selMatricola = CommonHelper.GetCurrentUserMatricola();
            Dashboard dashBoard = new Dashboard();

            TalentiaEntities db = new TalentiaEntities();
            var inAppr = db.TREQUESTS.Count(x => x.SINTESI1.COD_MATLIBROMAT == selMatricola && !x.TREQUESTS_STEP.Any(y=>y.IND_CURRFORM!=null));
            var toDo = db.CURRFORM.Count(x => x.SINTESI1.COD_MATLIBROMAT == selMatricola && x.QTA_COMPLETION < 2);
            var iniziati = db.CURRFORM.Count(x => x.SINTESI1.COD_MATLIBROMAT == selMatricola && x.QTA_COMPLETION > 1 && x.QTA_COMPLETION<100);


            dashBoard.AlertCorsiDaFare.CifraPrincipale = toDo.ToString();
            dashBoard.AlertCorsiDaFare.HrefPulsante = Url.Action("ElencoCorsiDaFare", new { showAll = true });
            dashBoard.AlertCorsiDaFare.AriaLabelSummary = "Hai " + (toDo == 1 ? "un corso" : toDo.ToString() + "corsi") + " da fare";
            dashBoard.AlertCorsiDaFare.AriaLabelPulsante = "Vai al dettaglio dei corsi da fare";

            dashBoard.AlertCorsiDaApprovare.CifraPrincipale = inAppr.ToString();
            dashBoard.AlertCorsiDaApprovare.HrefPulsante = Url.Action("InApprovazione", new { showAll = true });
            dashBoard.AlertCorsiDaApprovare.AriaLabelSummary = "Hai "+(inAppr == 1?"un corso": inAppr.ToString()+"corsi")+" da approvare";
            dashBoard.AlertCorsiDaApprovare.AriaLabelPulsante = "Vai al dettaglio dei corsi da approvare";

            dashBoard.AlertCorsiIniziati.CifraPrincipale = iniziati.ToString();
            dashBoard.AlertCorsiIniziati.HrefPulsante = Url.Action("ElencoCorsiIniziati", new { showAll = true });
            dashBoard.AlertCorsiIniziati.AriaLabelSummary = "Hai " + (iniziati == 1 ? "un corso" : iniziati.ToString() + "corsi") + " iniziati";
            dashBoard.AlertCorsiIniziati.AriaLabelPulsante = "Vai al dettaglio dei corsi iniziati";

            cv_ModelEntities cvEnt = new cv_ModelEntities();
            //int numCorsiFatti = cvEnt.V_CVCorsiRai.Count(m => m.matricola == selMatricola);
            int numCorsiFatti = RaiAcademyManager.GetCorsiFatti(selMatricola).Count();
            dashBoard.AlertCorsiFatti.CifraPrincipale = numCorsiFatti.ToString();
            dashBoard.AlertCorsiFatti.HrefPulsante = Url.Action("ElencoCorsiFatti", new { showAll = true });
            dashBoard.AlertCorsiFatti.AriaLabelSummary = "Hai fatto " + (numCorsiFatti == 1 ? "un corso" : dashBoard.AlertCorsiFatti.CifraPrincipale + " corsi");
            dashBoard.AlertCorsiFatti.AriaLabelPulsante = "Vai al dettaglio dei corsi fatti";

            dashBoard.Interessi.Add(new Link() { Testo = "giornalismo" });
            dashBoard.Interessi.Add(new Link() { Testo = "scrittura" });
            dashBoard.Interessi.Add(new Link() { Testo = "sceneggiatura" });
            dashBoard.Interessi.Add(new Link() { Testo = "web" });
            dashBoard.Interessi.Add(new Link() { Testo = "fotografia" });
            dashBoard.Interessi.Add(new Link() { Testo = "montaggio" });
            dashBoard.Interessi.Add(new Link() { Testo = "marketing" });

            return View(dashBoard);
        }

        public ActionResult search( RaiAcademyVM model)
        {
			RaiAcademyControllerScope.Instance.Filtri = new FiltriCatalogoModel();
			RaiAcademyControllerScope.Instance.Filtri = model.filtri;

			// solo per il momento
            //RaiAcademyControllerScope.Instance.Filtri = this.GetFiltri(model);

            //return RedirectToAction("index");
            return IndexTab(StatoCorsoTipoOffertaEnum.NonDefinito);
        }

        public FiltriCatalogoModel GetFiltri(RaiAcademyVM model, string areaFilter="", string temaFilter="", string gruppoFilter="")
        {
            TalentiaEntities db = new TalentiaEntities();
            
            FiltriCatalogoModel f = new FiltriCatalogoModel();

            if (RaiAcademyControllerScope.Instance.Filtri != null)
            {
                f.SearchString = RaiAcademyControllerScope.Instance.Filtri.SearchString??"";
                f.SelectedFilter = RaiAcademyControllerScope.Instance.Filtri.SelectedFilter??"";
            }

            string areaFilterStr = "";
            string temaFilterStr = "";
            if (!String.IsNullOrWhiteSpace(areaFilter))
            {
                int idArea = Convert.ToInt32(areaFilter);
                areaFilterStr = db.TB_TEMA.FirstOrDefault(x => x.ID_TEMA == idArea).DES_TEMA;
            }
            if (!String.IsNullOrWhiteSpace(temaFilter))
            {
                int idTema = Convert.ToInt32(temaFilter);
                temaFilterStr = db.TB_TPCORSO.FirstOrDefault(x => x.ID_TPCORSO == idTema).COD_TIPOCORSO;
            }   

            Sezione s = new Sezione("FILTRA PER AREE E TEMI");
            var areeTemi = model.Corsi.Items.GroupBy(x => x.AreaFormativa,
                                                     x => x.Tematica,
                                                     (key, g) => new { Area = key, Temi = g.Distinct().ToList() });
            
            foreach (var area in areeTemi)
            {
                voce voce = new voce(area.Area);
                foreach (var tema in area.Temi)
                {
                    voce sottovoce = new voce(tema);
                    sottovoce.RefField = "Tematica";
                    var statiSottovoce = model.Corsi.Items.Where(x => x.Tematica == tema).GroupBy(y => y.Stato).Select(z => z.Key);
                    foreach (var item in statiSottovoce)
                        sottovoce.StatoAttribute += (int)item + ",";
                    if (!String.IsNullOrWhiteSpace(areaFilterStr) && areaFilterStr.Equals(voce.NomeVoce, StringComparison.CurrentCultureIgnoreCase))
                        sottovoce.Impostato = String.IsNullOrWhiteSpace(temaFilterStr) || temaFilterStr.Equals(sottovoce.NomeVoce, StringComparison.CurrentCultureIgnoreCase);
                    voce.SottoVoci.Add(sottovoce);
                }
                var statiVoce = model.Corsi.Items.Where(x => x.AreaFormativa == area.Area).GroupBy(y => y.Stato).Select(z => z.Key);
                foreach (var item in statiVoce)
                    voce.StatoAttribute += (int)item + ",";
                s.Voci.Add(voce);
            }

            if (model.Corsi.Items.Any(x=>!String.IsNullOrWhiteSpace(x.Gruppo)))
            {
                string paramTemAgg = CommonHelper.GetParametro<string>(EnumParametriSistema.AcademyTematicheAggiuntive);
                var temiAgg = paramTemAgg.Split(';').Select(x=>x.Split('|')).Select(y=>new { area = y[0], gruppo = y[1]});
                

                //voce voce = new voce("TEMATICA AGGIUNTIVA");
                foreach (var item in model.Corsi.Items.Where(x=>!String.IsNullOrWhiteSpace(x.Gruppo)).Select(y=>y.Gruppo).Distinct())
	            {
                    var temaAgg = temiAgg.FirstOrDefault(x=>x.gruppo.ToUpper()==item.ToUpper());
                    if (temaAgg!=null)
                    {
                        //Cerco l'area tematica corrispondente
                        var voce = s.Voci.FirstOrDefault(x => x.NomeVoce.ToUpper() == temaAgg.area.ToUpper());
                        if (voce != null)
                        {
                            voce sottovoce = new voce(item);
                            sottovoce.Impostato = (!String.IsNullOrWhiteSpace(areaFilterStr) && areaFilterStr.Equals(voce.NomeVoce, StringComparison.CurrentCultureIgnoreCase)) || (!String.IsNullOrWhiteSpace(gruppoFilter) && gruppoFilter.Equals(sottovoce.NomeVoce, StringComparison.CurrentCultureIgnoreCase));
                            sottovoce.RefField = "Gruppo";
                            var statiSottovoce = model.Corsi.Items.Where(x => x.Gruppo == item).GroupBy(y => y.Stato).Select(z => z.Key);
                            foreach (var item2 in statiSottovoce)
                            {
                                sottovoce.StatoAttribute += (int)item2 + ",";
                                voce.StatoAttribute+= (int)item2+",";
                            }
                            voce.SottoVoci.Add(sottovoce);
                        }
                    }
                    //voce.SottoVoci.Add(sottovoce);
	            }


                //var statiVoce = model.Corsi.Items.Where(x =>!String.IsNullOrWhiteSpace(x.Gruppo)).GroupBy(y => y.Stato).Select(z => z.Key);
                //foreach (var item in statiVoce)
                //    voce.StatoAttribute += (int)item + ",";
                //s.Voci.Add(voce);
            }

            f.Sezioni.Add(s);


            s = new Sezione();
            s.NomeSezione = "MODALITA' EROGAZIONE";
            s.Voci = new List<voce>();

            var erogazioni = model.Corsi.Items.GroupBy(x => x.TipoMetodoFormativo,
                                                       x => x.MetodoFormativo,
                                                       (key, g) => new { TipoMetodo = key, Metodi = g.Distinct().ToList() });

            foreach (var erogazione in erogazioni)
            {
                voce voce = new voce();
                voce.NomeVoce = EnumHelpers.GetAmbientValue(erogazione.TipoMetodo);
                foreach (var metodo in erogazione.Metodi)
                {
                    voce SubVoce = new voce(metodo);
                    var temp = model.Corsi.Items.Where(x => x.MetodoFormativo == metodo).GroupBy(y => y.Stato).Select(z => z.Key);
                    foreach (var item in temp)
                        SubVoce.StatoAttribute += (int)item + ",";
                    SubVoce.RefField = "MetodoFormativo";
                    
                    voce.StatoAttribute += SubVoce.StatoAttribute;
                    voce.SottoVoci.Add(SubVoce);
                }
                s.Voci.Add(voce);
            }

            f.Sezioni.Add(s);


            s = new Sezione();
            s.NomeSezione = "ORDINA PER";
            s.Voci = new List<voce>();
            s.Voci.Add(new voce() { NomeVoce = "Titolo del corso", showCheckbox = false, OrderType=OrderTypeEnum.String, OrderTag="titolo" });
            s.Voci.Add(new voce() { NomeVoce = "Data avvio del corso", showCheckbox = false, OrderType = OrderTypeEnum.Number, OrderTag = "datainizio" });

            f.Sezioni.Add(s);

            //s = new Sezione();
            //s.NomeSezione = "TAGS";
            //s.Voci = new List<voce>();
            //s.Voci.Add(new voce() { NomeVoce = "news" ,IsTag =true});
            //s.Voci.Add(new voce() { NomeVoce = "comunicazione", IsTag = true });
            //s.Voci.Add(new voce() { NomeVoce = "e-leadership", IsTag = true });

            //f.Sezioni.Add(s);
            return f;
        }
        
        public ActionResult GetCourseImage2(int? idCorso,bool resized=true)
        {
            //return new FileStreamResult(RaiAcademyDataController.GetCourseImage(idCorso), "image/jpeg;");
            return new FileContentResult(RaiAcademyDataController.GetCourseImageResized(idCorso,resized), "image/jpeg;");
        }

        public ActionResult GetDefaultCourseImage()
        {
            
            return new FileStreamResult(RaiAcademyDataController.GetDefaultCourseImage(), "image/jpeg;");
        }

        public ActionResult RaiPlace()
        {
            string selMatricola = CommonHelper.GetCurrentUserMatricola();
            CatalagoRaiPlace catalogo = new CatalagoRaiPlace();

            using (TalentiaEntities db = new TalentiaEntities())
            {
                var corsi = this.dataController.EstraiCorsi(CommonHelper.GetCurrentUserMatricola(), false);

                Random rnd = null;

                //catalogo.Categorie = corsi.GroupBy(a => a.IdAreaFormativa)
                //    .Select(b => new CategoriaRaiPlace()
                //    {
                //        idCategoria = b.Key,
                //        nomeCategoria = b.First().AreaFormativa,
                //        tematiche = b.GroupBy(c => c.IdTematica)
                //            .Select(d => new TematicaRaiPlace()
                //            {
                //                idTematica = d.Key,
                //                codTematica = d.First().Tematica
                //            }).ToList(),
                //        corsi = b.ToList()
                //    }).ToList();
                var tmp = corsi.GroupBy(a => a.IdAreaFormativa);
                foreach (var cat in tmp)
                {
                    CategoriaRaiPlace newCat = new CategoriaRaiPlace();
                    newCat.idCategoria = cat.Key;
                    newCat.nomeCategoria = cat.First().AreaFormativa;
                    newCat.tematiche = cat.GroupBy(a => a.IdTematica)
                        .Select(b => new TematicaRaiPlace()
                        {
                            idTematica = b.Key,
                            codTematica = b.First().Tematica
                        }).ToList();

                    List<int> usedI = new List<int>();
                    int itemCount = cat.Count();
                    int maxI = 3;
                    rnd = new Random();
                    for (int i = 0; i < maxI; i++)
                    {
                        int index = 0;
                        do index = rnd.Next(0, itemCount);
                        while (usedI.Contains(index));
                        usedI.Add(index);
                        newCat.corsi.Add(cat.ElementAt(index));
                    }
                    catalogo.Categorie.Add(newCat);
                }
            }

            catalogo.CVPerc = CommonHelper.GetPercentualCV(selMatricola);
            catalogo.CorsiFatti = dataController.GetCorsiFatti(selMatricola);
            if (catalogo.CorsiFatti.Count > 0)
            {
                catalogo.CorsiFatti = catalogo.CorsiFatti.OrderByDescending(x => x.DataFine).ToList();
            }

            return View(catalogo);
        }

        public ActionResult TestRaiplace()
        {
            return View();
        }
    }
}