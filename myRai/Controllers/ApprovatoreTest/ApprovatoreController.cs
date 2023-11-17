using myRaiCommonManager;
using myRaiCommonModel;
using myRaiCommonModel.Approvatore;
using myRaiData;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRai.Controllers.ApprovatoreTest
{
    public class ApprovatoreController : BaseCommonController
    {
        private List<L2D_SEDE_GAPP_EXT> GetMieSedi()
        {
            string pMatricola = CommonHelper.GetCurrentUserPMatricola( );
            List<string> listaSedi = new List<string>( );
            List<L2D_SEDE_GAPP_EXT> sedi = new List<L2D_SEDE_GAPP_EXT>( );

            listaSedi = CommonHelper.GetSediL1(pMatricola ).Distinct( ).ToList( );

            using ( digiGappEntities db = new digiGappEntities( ) )
            {
                foreach (string s in listaSedi)
                {
                    string codice = "";
                    string reparto = "";
                    if (s.Length > 5)
                    {
                        codice = s.Substring( 0 , 5 );
                        reparto = s.Substring( 5 , 2 );
                    }
                    else
                    {
                        codice = s;
                        reparto = "";
                    }

                    var item = db.L2D_SEDE_GAPP.AsNoTracking( ).Where( w => w.cod_sede_gapp.Equals( codice ) ).FirstOrDefault( );

                    if (item != null)
                    {
                        L2D_SEDE_GAPP_EXT dato = new L2D_SEDE_GAPP_EXT( item );

                        RepartoLinkedServer r = new LinkedTableDataController( ).GetDettagliReparto( codice , reparto, CommonHelper.GetCurrentUserMatricola() );

                        if ( r == null )
                            dato.DescrizioneReparto = "REP." + reparto + " - DESCRIZIONE REPARTO NON TROVATA";
                        else
                            dato.DescrizioneReparto = ( r.Descr_Reparto != null ? r.Descr_Reparto.Trim( ) : r.Descr_Reparto );

                        sedi.Add( dato );
                    }
                }
            }
            return sedi;
        }

        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult GetIlMioTeam ()
        {
            List<L2D_SEDE_GAPP_EXT> sedi = new List<L2D_SEDE_GAPP_EXT>( );
            sedi = this.GetMieSedi( );

            return PartialView( "~/Views/Approvatore/IlMioTeam.cshtml" , sedi );
        }

        public PartialViewResult FiltroRicerca ( )
        {
            RicercaDaApprovareAttivita model = new RicercaDaApprovareAttivita( );
            model.ListaStati = new List<SelectListItem>( );
            model.Nominativo = "";
            model.ListaTitoli = new List<SelectListItem>( );
            model.ListaTitoliRecenti = new List<SelectListItem>( );
            return PartialView( "~/Views/Approvatore/FiltroRicerca.cshtml" , model );
        }

        public PartialViewResult CercaRichieste ( )
        {
            WidgetElencoRichieste model = new WidgetElencoRichieste( );
            model.Richieste = new List<WidgetElencoRichiesteItem>( );

            List<L2D_SEDE_GAPP_EXT> sedi = new List<L2D_SEDE_GAPP_EXT>( );
            sedi = this.GetMieSedi( );

            foreach(var s in sedi)
            {
                model.Richieste.Add( new WidgetElencoRichiesteItem( )
                {
                    CodiceSedeGapp = s.cod_sede_gapp ,
                    DescrizioneSedeGapp = s.desc_sede_gapp ,
                    Reparto = s.DescrizioneReparto,
                    TotRichiesteOrdinarie = 0 ,
                    TotRichiesteScadute = 0 ,
                    TotRichiesteUrgenti = 0
                } );
            }

            return PartialView( "~/Views/Approvatore/ElencoRichieste.cshtml" , model );
        }

        [HttpGet]
        public PartialViewResult CaricaDipendentiPerSede ( string codiceSedeGapp )
        {
            PresenzaDipendentiPerSede model = new PresenzaDipendentiPerSede( );
            model = HomeManager.GetPresenzaDipendenti_Sede( codiceSedeGapp , "" );

            return PartialView( "~/Views/Approvatore/IlMioTeamItem.cshtml", model );
        }

        [HttpGet]
        public PartialViewResult CaricaDettaglioRichieste ( string codiceSedeGapp )
        {
            ModelDash model = new ModelDash( );
            model = this.RefreshDaApprovare( );
            return PartialView( "~/Views/Approvatore/DettaglioRichieste.cshtml" , model );
        }

        private ModelDash RefreshDaApprovare ( string nominativo = "" , string sede = "" , int stato = 10, string eccezione = "" , string dataDa = "" , string dataA = "" , string livelloDip = "" , string soloUffProd = "" , bool visualizzati = false, string richiedevisti = "" )
        {
            ModelDash model = new ModelDash( );
            model = HomeManager.GetDaApprovareModel( model , false , 0 , sede , stato , nominativo , eccezione , dataDa , dataA , visualizzati , livelloDip , soloUffProd , richiedevisti == "1" );
            ModelState.Clear( );
            model = HomeManager.MarcaLivelloPerRichiedenti( model );

            if ( richiedevisti == "1" )
                model.RichiedeVisti = true;
            return model;
        }

    }
}