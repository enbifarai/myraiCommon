using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRaiData;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using myRaiHelper;
using myRaiCommonModel;
using myRaiCommonManager;
using myRai.Business;

namespace myRai.Controllers
{
    public class ApprovazioneController : myRaiHelper.BaseCommonController
    {
        ModelDash pr = new ModelDash();
        daApprovareModel daApprov;
        WSDigigapp datiBack = new WSDigigapp();
        WSDigigapp datiBack_ws1 = new WSDigigapp();

        public ActionResult Index(int? idscelta)
        {
            string userName = CommonHelper.GetCurrentUsername();
       
            datiBack.Credentials =new System.Net.NetworkCredential( CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            datiBack_ws1.Credentials =new System.Net.NetworkCredential( CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            Autorizzazioni.Sedi SEDI = new Autorizzazioni.Sedi();
            SEDI.Credentials =new System.Net.NetworkCredential( CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );

            pr.menuSidebar = UtenteHelper.getSidebarModel();// new sidebarModel(3);
            pr.MieRichieste = new List<MiaRichiesta>();
            pr.digiGAPP = true;
            pr.Raggruppamenti = CommonHelper.GetRaggruppamenti();

            pr.ValidazioneGenericaEccezioni = CommonHelper.GetParametro<string>(EnumParametriSistema.ValidazioneGenericaEccezioni);
            pr.SceltePercorso = HomeManager.GetSceltepercorsoModel("PR");
            pr.JsInitialFunction = HomeManager.GetJSfunzioneIniziale(idscelta);

            var db = new digiGappEntities();
            List<SelectListItem> list = new List<SelectListItem>();

			pr.RicercaModel = new RicercaDaApprovare();

			var response = CommonHelper.GetSediGappResponsabileList();

			if ( response != null && response.Any() )
			{
				response.ForEach( i =>
				{
					list.Add( new SelectListItem()
					{
						Value = i.CodiceSede,
						Text = i.DescrizioneSede,
						Selected = false
					} );
				} );
			}

            pr.RicercaModel.listaSedi = list;

			List<StatiRichiesta> listaIdsStati = new List<StatiRichiesta>();
			List<StatiRichiesta> listaEccezioni = new List<StatiRichiesta>();

			if ( list != null && list.Any() )
			{
				list.ForEach( l =>
				{
					listaIdsStati.AddRange(this.GetListaStati( l.Value ));
					listaEccezioni.AddRange( this.GetListaEccezioni( l.Value ) );
				} );				
			}

			List<StatiRichiesta> listVisualizzato = new List<StatiRichiesta>();
			listVisualizzato.AddRange( this.GetListaVisualizzato() );

			pr.RicercaModel.ListaidStato = new SelectList( listaIdsStati.GroupBy( x => x.Value ).Select( x => x.FirstOrDefault() ), "Value", "Text" );

			pr.RicercaModel.ListaEccezione = new SelectList( listaEccezioni.GroupBy( x => x.Value ).Select( x => x.FirstOrDefault() ), "Value", "Text" );

			pr.RicercaModel.ListVisualizzato = new SelectList( listVisualizzato, "Value", "Text" );

            pr.RicercaModel.ListLivelloDip = new SelectList(new List<SelectListItem>(){
            new SelectListItem() { Text = "Tutti i livelli",  Value = ""},
            new SelectListItem() { Text = "Solo i Livello 1", Value = "01"}},"Value","Text");
            
            var matricoleMonitoraggioFerie = CommonManager.GetParametro<string>( EnumParametriSistema.MatricoleMonitoraggioFerie );

            if ( !String.IsNullOrEmpty( matricoleMonitoraggioFerie ) )
            {
                if ( matricoleMonitoraggioFerie.Contains( CommonManager.GetCurrentUserMatricola( ) ) ||
                    matricoleMonitoraggioFerie.Contains( "*" ) )
                {
                    pr.WidgetStatoFerie = new myRaiCommonModel.DashboardResponsabile.WidgetVM( );
                    pr.WidgetStatoFerie.WidgetId = "statoFerie";
                    pr.WidgetStatoFerie.Title = "Monitoraggio ferie";
                    pr.WidgetStatoFerie.ActionName = "AnteprimaStatoFerie";
                }
                else
                {
                    pr.WidgetStatoFerie = null;
                }
            }
            else
            {
                pr.WidgetStatoFerie = null;
                //pr.WidgetStatoFerie = new Models.DashboardResponsabile.WidgetVM( );
                //pr.WidgetStatoFerie.WidgetId = "statoFerie";
                //pr.WidgetStatoFerie.Title = "Monitoraggio ferie";
                //pr.WidgetStatoFerie.ActionName = "AnteprimaStatoFerie";
            }

            return View("index2", pr);
        }

        public ActionResult FiltroRicerca(string nominativo, string stato, string eccezione, string dataFrom, string dataTo)
        {
            RicercaDaApprovare model = new RicercaDaApprovare();

            return View(model);
        }

        public ActionResult Ricerca()
        {
            RicercaDaApprovare model = new RicercaDaApprovare();

            var db = new digiGappEntities();

            return View(model);
        }

        public ActionResult InServ(string m)
        {
            if (string.IsNullOrWhiteSpace(m)) return Content("false");

            var b =HomeManager.IsInServizio(m);
            return Content(b.ToString().ToLower());
        }

		/// <summary>
		/// Reperimento del file pdf convalidato o firmato per la sede richiesta
		/// nell'intervallo temporale passato
		/// </summary>
		/// <param name="sede"></param>
		/// <param name="dataDa"></param>
		/// <param name="dataA"></param>
		/// <returns></returns>
		public ActionResult GetPDFPerSede ( string sede, string dataDa, string dataA )
		{
			try
			{
				DateTime da = dataDa.ToDateTime( format: "ddMMyyyy" );
				DateTime a = dataA.ToDateTime( format: "ddMMyyyy" );

				byte[] content = null;

				using ( digiGappEntities db = new digiGappEntities() )
				{
					var rec = db.DIGIRESP_Archivio_PDF.Where( r => r.sede_gapp.Equals( sede ) && r.data_inizio.Equals( da ) && r.data_fine.Equals( a ) ).ToList();

					if ( rec != null && rec.Any() )
					{
						content = rec.First().pdf;
					}
				}

				return new FileContentResult( content.ToArray(), "application/pdf" );
			}
			catch ( Exception ex )
			{
				return null;
			}
		}

		private List<StatiRichiesta> GetListaStati ( string sedeGapp )
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

			if ( query != null )
			{
				query.ToList().ForEach( q =>
				{
					if ( !list.Any( x => x.Value.Equals( q.stato.id_stato.ToString() ) ) )
					{
						list.Add( new StatiRichiesta()
						{
							Value = q.stato.id_stato.ToString(),
							Text = q.stato.descrizione_stato,
							Selected = false
						} );
					}
				} );
			}

			return list;
		}

		private List<StatiRichiesta> GetListaEccezioni ( string sedeGapp )
		{
			List<StatiRichiesta> list = new List<StatiRichiesta>();

			// prendo tutte le richieste 
			using (digiGappEntities db = new digiGappEntities())
			{
				try
				{
					var results = db.sp_GetListaEccezioni( sedeGapp );

					if ( results != null )
					{
						results.ToList().ForEach( q =>
						{
							list.Add( new StatiRichiesta()
							{
								Value = q.CodiceEccezione,
								Text = q.CodiceEccezione + "-" + q.DescrizioneEccezione,
								Selected = false
							} );
						} );
					}
				}
				catch
				{
					var results = db.MyRai_Eccezioni_Ammesse.ToList();

					if ( results != null )
					{
						results.ToList().ForEach( q =>
						{
							list.Add( new StatiRichiesta()
							{
								Value = q.cod_eccezione,
								Text = q.cod_eccezione + "-" + q.desc_eccezione,
								Selected = false
							} );
						} );
					}
				}
			}

			return list;
		}

		private List<StatiRichiesta> GetListaVisualizzato ( )
		{
			List<StatiRichiesta> list = new List<StatiRichiesta>();

			list.Add( new StatiRichiesta()
			{
				Value = "-1",
				Text = "Tutti",
				Selected = false
			} );

			list.Add( new StatiRichiesta()
			{
				Value = "1",
				Text = "Visualizzato",
				Selected = false
			} );

			list.Add( new StatiRichiesta()
			{
				Value = "0",
				Text = "Non visualizzato",
				Selected = false
			} );

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