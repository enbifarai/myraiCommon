using myRaiCommonManager;
using myRaiCommonModel;
using myRaiCommonTasks.it.rai.servizi.svildigigappws;
using myRaiData;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class ApprovazioneProduzioneController : BaseCommonController
    {
        private class StatiRichiesta
        {
            public string Value { get; set; }
            public string Text { get; set; }
            public bool Selected { get; set; }
        }

        ModelDash pr = new ModelDash ( );
        daApprovareModel daApprov;
        WSDigigapp datiBack = new WSDigigapp ( );
        WSDigigapp datiBack_ws1 = new WSDigigapp ( );

        public ActionResult Index ( int? idscelta )
        {
            string userName = CommonHelper.GetCurrentUsername ( );

            datiBack.Credentials = new System.Net.NetworkCredential ( CommonHelper.GetParametri<string> ( EnumParametriSistema.AccountUtenteServizio )[0] , CommonHelper.GetParametri<string> ( EnumParametriSistema.AccountUtenteServizio )[1] );
            datiBack_ws1.Credentials = new System.Net.NetworkCredential ( CommonHelper.GetParametri<string> ( EnumParametriSistema.AccountUtenteServizio )[0] , CommonHelper.GetParametri<string> ( EnumParametriSistema.AccountUtenteServizio )[1] );
            Autorizzazioni.Sedi SEDI = new Autorizzazioni.Sedi ( );
            SEDI.Credentials = new System.Net.NetworkCredential ( CommonHelper.GetParametri<string> ( EnumParametriSistema.AccountUtenteServizio )[0] , CommonHelper.GetParametri<string> ( EnumParametriSistema.AccountUtenteServizio )[1] );

            pr.menuSidebar = UtenteHelper.getSidebarModel();
            pr.MieRichieste = new List<MiaRichiesta> ( );
            pr.digiGAPP = true;
            pr.Raggruppamenti = CommonHelper.GetRaggruppamenti ( );

            pr.ValidazioneGenericaEccezioni = CommonHelper.GetParametro<string> ( EnumParametriSistema.ValidazioneGenericaEccezioni );
            pr.SceltePercorso = HomeManager.GetSceltepercorsoModel ( "PR" );
            pr.JsInitialFunction = HomeManager.GetJSfunzioneIniziale ( idscelta );

            var db = new digiGappEntities ( );
            List<SelectListItem> list = new List<SelectListItem> ( );

            pr.RicercaModel = new RicercaDaApprovare ( );

            var response = CommonHelper.GetSediGappResponsabileList ( );

            if ( response != null && response.Any ( ) )
            {
                response.ForEach ( i =>
                {
                    list.Add ( new SelectListItem ( )
                    {
                        Value = i.CodiceSede ,
                        Text = i.DescrizioneSede ,
                        Selected = false
                    } );
                } );
            }

            pr.RicercaModelAttivita = new RicercaDaApprovareAttivita ( );
            pr.RicercaModelAttivita.ListaTitoli = new List<SelectListItem> ( );
            pr.RicercaModelAttivita.ListaStati = this.GetListaStati( );
            pr.RicercaModelAttivita.ListaTitoliRecenti = this.GetAttivitaCeitonRecenti( );
            pr.RicercaModelAttivita.ListaTitoli = this.GetAttivitaCeiton( 10 );
            pr.RicercaModelAttivita.ListaEccezioniCeiton = this.GetListaEccezioni( 10 );

            var titoli = pr.RicercaModelAttivita.ListaTitoli.Select( w => w.Text ).ToList( );

            if (titoli != null &&
                titoli.Any())
            {
                pr.RicercaModelAttivita.ListaTitoliRecenti = pr.RicercaModelAttivita.ListaTitoliRecenti.Where( w => titoli.Contains( w.Text ) ).ToList( );
            }

            return View ( pr );
        }

        public ActionResult GetRichiesteAttProduzione ( )
        {
            string nominativo = Request.QueryString["nome"];
            string sede = Request.QueryString["sede"];
            int stato = 0;
            string eccezione = Request.QueryString["eccezione"];
            string dataDa = Request.QueryString["data_da"];
            string dataA = Request.QueryString["data_a"];
            string livelloDip = Request.QueryString["livelloDip"];
            string titolo = Request.QueryString["titolo"];

            bool? visualizzati = null;
            try
            {
                if ( Request.QueryString["stato"] == "" )
                { stato = 10; }
                else
                { stato = Convert.ToInt32 ( Request.QueryString["stato"] ); }
            }
            catch ( Exception )
            {

            }

            try
            {
                if ( Request.QueryString["visualizzati"] != "" && !Request.QueryString["visualizzati"].ToUpper ( ).Equals ( "UNDEFINED" ) )
                {
                    int _myTemp = Convert.ToInt32 ( Request.QueryString["visualizzati"] );

                    if ( _myTemp == 1 )
                    {
                        visualizzati = true;
                    }
                    else if ( _myTemp == 0 )
                    {
                        visualizzati = false;
                    }
                    else
                    {
                        visualizzati = null;
                    }
                }
            }
            catch ( Exception )
            {
            }

            if (!String.IsNullOrEmpty(sede) && sede.ToUpper().Equals("UNDEFINED"))
            {
                sede = "";
            }

            if ( !String.IsNullOrEmpty ( livelloDip ) && livelloDip.ToUpper ( ).Equals ( "UNDEFINED" ) )
            {
                livelloDip = "";
            }

            if ( !String.IsNullOrEmpty ( eccezione ) && eccezione.ToUpper ( ).Equals ( "UNDEFINED" ) )
            {
                eccezione = "";
            }

            if ( Request.QueryString["visualizzati"] != "" && Request.QueryString["visualizzati"].ToUpper ( ).Equals ( "UNDEFINED" ) )
            {
                visualizzati = null;
            }

            if ( !String.IsNullOrEmpty(titolo) && !titolo.ToUpper ( ).Equals ( "UNDEFINED" ) )
            {
                this.SetAttivitaCeiton ( titolo );
            }

            if ( !String.IsNullOrEmpty( titolo ) && titolo.ToUpper( ).Equals( "UNDEFINED" ) )
            {
                titolo = "";
            }

            ModelDash model = HomeManager.GetDaApprovareProduzioneModel ( pr , false , 0 , sede , stato , nominativo , eccezione , dataDa , dataA , visualizzati , livelloDip, titolo );
            ModelState.Clear ( );
            model = HomeManager.MarcaLivelloPerRichiedenti ( model );

            if ( ApprovatoreProduzioneCUSTOMEnabled( ) )
                return View( "~/Views/ApprovazioneProduzione/subpartial/da_approvareCustom.cshtml" , model );
            else
                return View( "~/Views/ApprovazioneProduzione/subpartial/da_approvare.cshtml" , model );
        }

        private bool ApprovatoreProduzioneCUSTOMEnabled ( )
        {
            bool result = false;
            try
            {
                //var par = CommonHelper.GetParametro<bool>( EnumParametriSistema.AbilitaSceltaApprovatore );
                //result = par;

                var L3 = CommonHelper.GetSediL3(CommonHelper.GetCurrentUserPMatricola() );
                var L4 = CommonHelper.GetSediL4( );
                var L5 = CommonHelper.GetSediL5( );

                if ((L3 != null && L3.Any())
                    && (( L4 != null && L4.Any( ) ) || ( L5 != null && L5.Any( ) )))
                {
                    result = true;
                }
                else if ( ( L3 == null || !L3.Any( ) )
                            && ( ( L4 != null && L4.Any( ) ) || ( L5 != null && L5.Any( ) ) ) )
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            catch ( Exception ex )
            {
                result = false;
            }
            return result;
        }

        public ActionResult GetRichiesteAttProduzioneFlat ( )
        {
            string nominativo = Request.QueryString["nome"];
            string sede = Request.QueryString["sede"];
            int stato = 0;
            string eccezione = Request.QueryString["eccezione"];
            string dataDa = Request.QueryString["data_da"];
            string dataA = Request.QueryString["data_a"];
            string livelloDip = Request.QueryString["livelloDip"];
            string titolo = Request.QueryString["titolo"];

            bool? visualizzati = null;
            try
            {
                if ( Request.QueryString["stato"] == "" )
                { stato = 10; }
                else
                {
                    stato = Convert.ToInt32( Request.QueryString["stato"] );
                }
            }
            catch ( Exception )
            {

            }

            try
            {
                if ( Request.QueryString["visualizzati"] != "" && !Request.QueryString["visualizzati"].ToUpper( ).Equals( "UNDEFINED" ) )
                {
                    int _myTemp = Convert.ToInt32( Request.QueryString["visualizzati"] );

                    if ( _myTemp == 1 )
                    {
                        visualizzati = true;
                    }
                    else if ( _myTemp == 0 )
                    {
                        visualizzati = false;
                    }
                    else
                    {
                        visualizzati = null;
                    }
                }
            }
            catch ( Exception )
            {
            }

            if ( !String.IsNullOrEmpty( sede ) && sede.ToUpper( ).Equals( "UNDEFINED" ) )
            {
                sede = "";
            }

            if ( !String.IsNullOrEmpty( livelloDip ) && livelloDip.ToUpper( ).Equals( "UNDEFINED" ) )
            {
                livelloDip = "";
            }

            if ( !String.IsNullOrEmpty( eccezione ) && eccezione.ToUpper( ).Equals( "UNDEFINED" ) )
            {
                eccezione = "";
            }

            if ( Request.QueryString["visualizzati"] != "" && Request.QueryString["visualizzati"].ToUpper( ).Equals( "UNDEFINED" ) )
            {
                visualizzati = null;
            }

            if ( !String.IsNullOrEmpty( titolo ) && !titolo.ToUpper( ).Equals( "UNDEFINED" ) )
            {
                this.SetAttivitaCeiton( titolo );
            }

            if ( !String.IsNullOrEmpty( titolo ) && titolo.ToUpper( ).Equals( "UNDEFINED" ) )
            {
                titolo = "";
            }

            ModelDash model = HomeManager.GetDaApprovareProduzioneModel( pr , false , 0 , sede , stato , nominativo , eccezione , dataDa , dataA , visualizzati , livelloDip , titolo );
            ModelState.Clear( );
            model = HomeManager.MarcaLivelloPerRichiedenti( model );

            model.RicercaModelAttivita.ListaStati = this.GetListaStati( stato );

            if ( ApprovatoreProduzioneCUSTOMEnabled( ) )
                return View( "~/Views/ApprovazioneProduzione/subpartial/da_approvareCustom.cshtml" , model );
            else
            return View( "~/Views/ApprovazioneProduzione/subpartial/da_approvare.cshtml" , model );
        }

        public ActionResult LoadSelect(int idStato )
        {
            RicercaDaApprovareAttivita model = new RicercaDaApprovareAttivita( );
            model.ListaTitoliRecenti = this.GetAttivitaCeitonRecenti( );
            model.ListaTitoli = this.GetAttivitaCeiton( idStato );
            model.ListaEccezioniCeiton = this.GetListaEccezioni( idStato );

            var titoli = model.ListaTitoli.Select( w => w.Text ).ToList( );

            if ( titoli != null &&
                titoli.Any( ) )
            {
                model.ListaTitoliRecenti = model.ListaTitoliRecenti.Where( w => titoli.Contains( w.Text ) ).ToList( );
            }

            return View( "~/Views/ApprovazioneProduzione/subpartial/_selectAttivita.cshtml" , model );
        }

        #region Private

        private List<SelectListItem> GetListaStati ( int idStato = 10 )
        {
            List<SelectListItem> list = new List<SelectListItem>( );

            list.Add( new SelectListItem( )
            {
                Value = "10" ,
                Text = "In approvazione" ,
                Selected = (idStato == 10)
            } );
            list.Add( new SelectListItem( )
            {
                Value = "20" ,
                Text = "Approvato" ,
                Selected = ( idStato == 20 )
            } );
            list.Add( new SelectListItem( )
            {
                Value = "50" ,
                Text = "Rifiutato" ,
                Selected = ( idStato == 50 )
            } );
            list.Add( new SelectListItem( )
            {
                Value = "60" ,
                Text = "Cancellato" ,
                Selected = ( idStato == 60 )
            } );
            list.Add( new SelectListItem( )
            {
                Value = "70" ,
                Text = "Eliminato" ,
                Selected = ( idStato == 70 )
            } );

            return list;
        }

        private List<SelectListItem> GetListaEccezioni ( int idStato )
        {
            List<SelectListItem> list = new List<SelectListItem>( );
            List<string> listaSedi = new List<string>( );
            List<string> listaSedi4 = new List<string>( );
            List<string> listaEccezioniScartate = new List<string>( );
            List<string> listaSedi5 = new List<string>( );

            try
            {
                string matricola = CommonHelper.GetCurrentUserMatricola( );

                //listaSedi = CommonManager.GetSediL3( ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );

                listaSedi = CommonHelper.GetSediLApprovatoreProduzione( ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );
                listaSedi4 = CommonHelper.GetSediL4( ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );
                listaSedi5 = CommonHelper.GetSediL5( ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );

                string par = CommonHelper.GetParametro<string>( EnumParametriSistema.EccezioniEscluseL4 );
                if ( !String.IsNullOrEmpty( par ) )
                {
                    listaEccezioniScartate = par.Split( ',' ).ToList( );
                }

                if ( listaSedi != null && listaSedi.Any( ) )
                {
                    foreach ( var sedeGapp in listaSedi )
                    {
                        using ( digiGappEntities db = new digiGappEntities( ) )
                        {
                            var results = ( from ecc in db.MyRai_Eccezioni_Richieste
                                            join ric in db.MyRai_Richieste
                                            on ecc.id_richiesta equals ric.id_richiesta
                                            join amm in db.MyRai_Eccezioni_Ammesse
                                            on ecc.cod_eccezione equals amm.cod_eccezione
                                            where
                                                ric.id_Attivita_ceiton != null &&
                                                ecc.id_stato == idStato &&
                                                (
                                                  ( listaSedi.Count > 0 ? listaSedi.Contains( ric.codice_sede_gapp ) : true ) ||
                                                  ( listaSedi4.Count > 0 ? listaSedi4.Contains( ric.codice_sede_gapp ) && !listaEccezioniScartate.Contains( ecc.cod_eccezione ) : true ) ||
                                                  ( listaSedi5.Count > 0 ? listaSedi5.Contains( ric.codice_sede_gapp ) && listaEccezioniScartate.Contains( ecc.cod_eccezione ) : true )
                                                )
                                            select new
                                            {
                                                CodiceEccezione = ecc.cod_eccezione ,
                                                DescrizioneEccezione = amm.desc_eccezione
                                            } ).Distinct( ).ToList( );

                            if ( results != null )
                            {
                                results.ToList( ).ForEach( q =>
                                {
                                    list.Add( new SelectListItem( )
                                    {
                                        Value = q.CodiceEccezione ,
                                        Text = q.CodiceEccezione + "-" + q.DescrizioneEccezione ,
                                        Selected = false
                                    } );
                                } );
                            }
                        }
                    }
                }
                else
                {
                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        var results = ( from ecc in db.MyRai_Eccezioni_Richieste
                                        join ric in db.MyRai_Richieste
                                        on ecc.id_richiesta equals ric.id_richiesta
                                        join amm in db.MyRai_Eccezioni_Ammesse
                                        on ecc.cod_eccezione equals amm.cod_eccezione
                                        join attivita in db.MyRai_AttivitaCeiton
                                        on ric.id_Attivita_ceiton equals attivita.id
                                        join Approvatori in db.MyRai_ApprovatoreProduzione
                                        on attivita.Titolo equals Approvatori.Titolo
                                        where ric.id_Attivita_ceiton != null &&
                                            ecc.id_stato == idStato &&
                                            Approvatori.MatricolaApprovatore == matricola
                                        select new
                                        {
                                            CodiceEccezione = ecc.cod_eccezione ,
                                            DescrizioneEccezione = amm.desc_eccezione
                                        } ).Distinct( ).ToList( );

                        if ( results != null )
                        {
                            results.ToList( ).ForEach( q =>
                            {
                                list.Add( new SelectListItem( )
                                {
                                    Value = q.CodiceEccezione ,
                                    Text = q.CodiceEccezione + "-" + q.DescrizioneEccezione ,
                                    Selected = false
                                } );
                            } );
                        }
                    }
                }
            }
            catch
            {
                list = new List<SelectListItem>( );
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    var results = db.MyRai_Eccezioni_Ammesse.ToList( );

                    if ( results != null )
                    {
                        results.ToList( ).ForEach( q =>
                        {
                            list.Add( new SelectListItem( )
                            {
                                Value = q.cod_eccezione ,
                                Text = q.cod_eccezione + "-" + q.desc_eccezione ,
                                Selected = false
                            } );
                        } );
                    }
                }
            }

            return list;
        }

        private void SetAttivitaCeiton(string titolo)
        {
            try
            {
                string matricola = CommonHelper.GetCurrentUserMatricola( );

                using ( digiGappEntities db = new digiGappEntities ( ) )
                {
                    // reperimento del record con id attività ceiton pari a idAttivita
                    var suggerimento = db.MyRai_SuggerimentiApprovatoreProduzione.Where( w => w.AttivitaCeiton.Equals( titolo ) ).FirstOrDefault( );

                    if ( suggerimento == null )
                    {
                        // aggiunge il record in suggerimenti
                        db.MyRai_SuggerimentiApprovatoreProduzione.Add( new MyRai_SuggerimentiApprovatoreProduzione( )
                        {
                            Contatore = 1 ,
                            Matricola = matricola ,
                            AttivitaCeiton = titolo,
                            DataUltimoAggiornamento = DateTime.Now
                        } );
                    }
                    else
                    {
                        // aggiunge una unità al contatore,
                        int contatore = suggerimento.Contatore;
                        contatore++;
                        suggerimento.Contatore = contatore;

                        // aggiorna la data di ultimo aggiornamento
                        suggerimento.DataUltimoAggiornamento = DateTime.Now;
                    }

                    db.SaveChanges ( );

                }
            }
            catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// Restituisce l'elenco delle attività Ceiton ordinate
        /// in base alla frequenza di utilizzo
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetAttivitaCeiton ( int idStato )
        {
            List<SelectListItem> list = new List<SelectListItem> ( );
            List<string> listaSedi = new List<string>( );

            try
            {
                string matricola = CommonHelper.GetCurrentUserMatricola( );

                listaSedi = CommonHelper.GetSediL3( CommonHelper.GetCurrentUserPMatricola( ) ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );

                if ( listaSedi != null && listaSedi.Any( ) )
                {
                using ( digiGappEntities db = new digiGappEntities ( ) )
                {

                    var temp = ( from attivita in db.MyRai_AttivitaCeiton
                                    join richieste in db.MyRai_Richieste
                                    on attivita.id equals richieste.id_Attivita_ceiton
                                    where richieste.id_stato == idStato &&
                                     ( listaSedi.Count > 0 ? listaSedi.Contains( richieste.codice_sede_gapp ) : true )
                                    select attivita.Titolo ).Distinct().ToList( );

                    if (temp != null && temp.Any())
                    {
                        foreach ( var itm in temp )
                        {
                            list.Add( new SelectListItem( )
                            {
                                Value = itm ,
                                Text = itm ,
                                Selected = false
                            } );
                        }
                    }
                }
            }
                else
                {
                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        var temp = db.MyRai_ApprovatoreProduzione.Where( w => w.MatricolaApprovatore.Equals( matricola ) ).ToList( );

                        if ( temp != null && temp.Any( ) )
                        {
                            var titoli = temp.Select( w => w.Titolo ).Distinct( );

                            foreach ( var itm in titoli )
                            {
                                list.Add( new SelectListItem( )
                                {
                                    Value = itm ,
                                    Text = itm ,
                                    Selected = false
                                } );
                            }
                        }
                    }
                }
            }
            catch
            {
                list = new List<SelectListItem>( );
            }

            return list;
        }

        private List<SelectListItem> GetAttivitaCeitonRecenti ( )
        {
            List<SelectListItem> list = new List<SelectListItem>( );

            string matricola = CommonHelper.GetCurrentUserMatricola( );

            using ( digiGappEntities db = new digiGappEntities( ) )
            {
                try
                {
                    List<string> titoli = new List<string>( );
                    var temp1 = db.MyRai_SuggerimentiApprovatoreProduzione.Where( w => w.Matricola.Equals( matricola ) ).Distinct( ).ToList( );

                    if ( temp1 != null && temp1.Any( ) )
                    {
                        var temp2 = temp1.OrderByDescending( w => w.Contatore ).ThenByDescending( w => w.DataUltimoAggiornamento ).ToList( );

                        if ( temp2 != null && temp2.Any( ) )
                        {
                            titoli = temp2.Select( w => w.AttivitaCeiton ).ToList( );

                            if ( titoli != null && titoli.Any( ) )
                            {
                                foreach ( var i in titoli )
                                {
                                    list.Add( new SelectListItem( )
                                    {
                                        Value = i ,
                                        Text = i ,
                                        Selected = false
                                    } );
                                }
                            }

                        }
                    }
                }
                catch
                {
                    list = new List<SelectListItem>( );
                }
            }

            return list;
        }

        #endregion

    }
}