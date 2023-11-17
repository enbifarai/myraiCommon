using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;

namespace myRai.Controllers
{
    public class VistoController : BaseCommonController
    {
        public ActionResult Index(int? idscelta)
        {
            ModelDash pr = new ModelDash();
            daApprovareModel daApprov;
            WSDigigapp datiBack = new WSDigigapp();
            var datiBack_ws1 = new WSDigigapp();


            string userName = CommonHelper.GetCurrentUsername();

            datiBack.Credentials = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            datiBack_ws1.Credentials = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            Autorizzazioni.Sedi SEDI = new Autorizzazioni.Sedi();
            SEDI.Credentials = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            pr.menuSidebar = UtenteHelper.getSidebarModel();// new sidebarModel(3);
            pr.MieRichieste = new List<MiaRichiesta>();
            pr.digiGAPP = true;
            //pr.dettaglioGiornata = HomeManager.GetTimbratureTodayModel();
            pr.Raggruppamenti = HomeManager.GetRaggruppamenti();

            pr.ValidazioneGenericaEccezioni = CommonHelper.GetParametro<string>(EnumParametriSistema.ValidazioneGenericaEccezioni);
            pr.SceltePercorso = HomeManager.GetSceltepercorsoModel("PR");
            pr.JsInitialFunction = HomeManager.GetJSfunzioneIniziale(idscelta);

            var db = new digiGappEntities();
            List<SelectListItem> list = new List<SelectListItem>();

            pr.RicercaModel = new RicercaDaApprovare() ;

            var response = CommonHelper.GetSediGappLivello6();//  GetSediGappResponsabileList();

            if (response != null && response.Any())
            {
                response.ForEach(i =>
                {
                    list.Add(new SelectListItem()
                    {
                        Value = i.CodiceSede,
                        Text = i.DescrizioneSede,
                        Selected = false
                    });
                });
            }

            pr.RicercaModel.listaSedi = list;

            /////////////////
            List<SelectListItem> listOpzVisti = new List<SelectListItem>();
            listOpzVisti.Add(new SelectListItem() {
                 Value="VP",
                 Text ="Visti positivi"
            });
            listOpzVisti.Add(new SelectListItem()
            {
                Value = "VN",
                Text = "Visti negativi"
            });
            pr.RicercaModel.listaOpzioniRicercaVisti = listOpzVisti;
            //////////////////////
            ///

            List<StatiRichiesta> listaIdsStati = new List<StatiRichiesta>() {
                new StatiRichiesta (){ Selected=false, Text="In Approvazione", Value="10" },
                new StatiRichiesta (){ Selected=false, Text="Approvato", Value="20" },
            };
            List<StatiRichiesta> listaEccezioni = new List<StatiRichiesta>();

            if (list != null && list.Any())
            {
                list.ForEach(l =>
                {
                    listaEccezioni.AddRange(this.GetListaEccezioni(l.Value));
                });
            }

            List<StatiRichiesta> listVisualizzato = new List<StatiRichiesta>();
            listVisualizzato.AddRange(this.GetListaVisualizzato());

            pr.RicercaModel.ListaidStato = new SelectList(listaIdsStati.GroupBy(x => x.Value).Select(x => x.FirstOrDefault()), "Value", "Text");

            pr.RicercaModel.ListaEccezione = new SelectList(listaEccezioni.GroupBy(x => x.Value).Select(x => x.FirstOrDefault()), "Value", "Text");

            pr.RicercaModel.ListVisualizzato = new SelectList(listVisualizzato, "Value", "Text");

            pr.RicercaModel.ListLivelloDip = new SelectList(new List<SelectListItem>(){
            new SelectListItem() { Text = "Tutti i livelli",  Value = ""},
            new SelectListItem() { Text = "Solo i Livello 1", Value = "01"}}, "Value", "Text");

          
            pr.RichiedeVisti = true;

            return View("index", pr);
        }
        private List<StatiRichiesta> GetListaVisualizzato()
        {
            List<StatiRichiesta> list = new List<StatiRichiesta>();

            list.Add(new StatiRichiesta()
            {
                Value = "-1",
                Text = "Tutti",
                Selected = false
            });

            list.Add(new StatiRichiesta()
            {
                Value = "1",
                Text = "Visualizzato",
                Selected = false
            });

            list.Add(new StatiRichiesta()
            {
                Value = "0",
                Text = "Non visualizzato",
                Selected = false
            });

            return list;
        }
        private List<StatiRichiesta> GetListaEccezioni(string sedeGapp)
        {
            List<StatiRichiesta> list = new List<StatiRichiesta>();

            // prendo tutte le richieste 
            using (digiGappEntities db = new digiGappEntities())
            {
                try
                {
                    var results = db.sp_GetListaEccezioni(sedeGapp);

                    if (results != null)
                    {
                        results.ToList().ForEach(q =>
                        {
                            list.Add(new StatiRichiesta()
                            {
                                Value = q.CodiceEccezione,
                                Text = q.CodiceEccezione + "-" + q.DescrizioneEccezione,
                                Selected = false
                            });
                        });
                    }
                }
                catch
                {
                    var results = db.MyRai_Eccezioni_Ammesse.ToList();

                    if (results != null)
                    {
                        results.ToList().ForEach(q =>
                        {
                            list.Add(new StatiRichiesta()
                            {
                                Value = q.cod_eccezione,
                                Text = q.cod_eccezione + "-" + q.desc_eccezione,
                                Selected = false
                            });
                        });
                    }
                }
            }

            return list;
        }
        private List<StatiRichiesta> GetListaStati(string sedeGapp)
        {
            List<StatiRichiesta> list = new List<StatiRichiesta>();

            var db = new digiGappEntities();

            var query = (from stati in db.MyRai_Stati
                         join listRichieste in db.MyRai_Richieste
                             on stati.id_stato equals listRichieste.id_stato
                         where listRichieste.codice_sede_gapp.Equals(sedeGapp) &&
                             stati.id_stato > 10
                         orderby stati.id_stato
                         select new { stato = stati }).Distinct();

            if (query != null)
            {
                query.ToList().ForEach(q =>
                {
                    if (!list.Any(x => x.Value.Equals(q.stato.id_stato.ToString())))
                    {
                        list.Add(new StatiRichiesta()
                        {
                            Value = q.stato.id_stato.ToString(),
                            Text = q.stato.descrizione_stato,
                            Selected = false
                        });
                    }
                });
            }

            return list;
        }
        private class StatiRichiesta
        {
            public string Value { get; set; }
            public string Text { get; set; }
            public bool Selected { get; set; }
        }
    }
}
