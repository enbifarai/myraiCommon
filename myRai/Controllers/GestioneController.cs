using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRai.DataAccess;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiCommonModel.Controllo;
using myRaiData;
using myRaiHelper;

namespace myRai.Controllers
{
    public class GestioneController : BaseCommonController
    {
        public ActionResult Index ( )
        {
            return View( );
        }

        public ActionResult Controllo ( )
        {
            Report model = new Report( );
            return View( "~/Views/Gestione/Controllo/index.cshtml" , model );
        }

        [HttpPost]
        public ActionResult testcodice ( string matricola , string data , string codeccezione , string codice )
        {
            string esito = EccezioniManager.TestCodice( CommonHelper.GetCurrentUserMatricola( ) , matricola , data , codeccezione , codice );
            return Content( esito );
        }

        [HttpPost]
        public ActionResult savecodice ( string codeccezione , string codice )
        {
            var db = new digiGappEntities( );
            var ec = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == codeccezione ).FirstOrDefault( );
            if ( ec != null )
            {
                ec.CodiceCsharp = codice;
                if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                    return Content( "OK" );
                else
                    return Content( "Errore salvataggio DB" );
            }
            return Content( "Non trovata" );
        }

        public ActionResult Save ( GestioneEccezioniAmmesseModel model )
        {
            if ( !ModelState.IsValid )
            {
                return RedirectToAction( "ammesse" , new { idEcc = model.EccezioneAmmessa.id } );
            }

            var db = new digiGappEntities( );

            MyRai_Eccezioni_Ammesse e = db.MyRai_Eccezioni_Ammesse.Where( x => x.id == model.EccezioneAmmessa.id ).FirstOrDefault( );
            e.CaratteriMotivoRichiesta = model.EccezioneAmmessa.CaratteriMotivoRichiesta;
            e.Categoria = model.EccezioneAmmessa.Categoria;
            e.cod_eccezione = model.EccezioneAmmessa.cod_eccezione;
            e.controlli_specifici = model.EccezioneAmmessa.controlli_specifici;
            e.data_fine_validita = model.EccezioneAmmessa.data_fine_validita;
            e.data_inizio_validita = model.EccezioneAmmessa.data_inizio_validita;
            e.desc_eccezione = model.EccezioneAmmessa.desc_eccezione;
            e.flag_attivo = model.EccezioneAmmessa.flag_attivo;
            e.FunzioneJS = model.EccezioneAmmessa.FunzioneJS;
            e.id_raggruppamento = model.EccezioneAmmessa.id_raggruppamento;
            e.OrarioEffettivo = model.EccezioneAmmessa.OrarioEffettivo;
            e.OrarioPrevisto = model.EccezioneAmmessa.OrarioPrevisto;
            e.OreInFuturo = model.EccezioneAmmessa.OreInFuturo;
            e.OreInPassato = model.EccezioneAmmessa.OreInPassato;
            e.PartialView = model.EccezioneAmmessa.PartialView;
            e.periodo = model.EccezioneAmmessa.periodo;
            e.Prontuario = model.EccezioneAmmessa.Prontuario;
            e.StatoGiornata = model.EccezioneAmmessa.StatoGiornata;
            e.TipiDipendente = model.EccezioneAmmessa.TipiDipendente;
            e.TipiDipendenteEsclusi = model.EccezioneAmmessa.TipiDipendenteEsclusi;
            e.tipo_controllo = model.EccezioneAmmessa.tipo_controllo;
            e.TipoGiornata = model.EccezioneAmmessa.TipoGiornata;
            e.ValoriParamExtraJSON = model.EccezioneAmmessa.ValoriParamExtraJSON;
            e.descrizione_eccezione = model.EccezioneAmmessa.descrizione_eccezione;
            e.id_workflow = model.EccezioneAmmessa.id_workflow;
            e.CodiceCsharp = model.EccezioneAmmessa.CodiceCsharp;

            DBHelper.Save( db, CommonHelper.GetCurrentUserMatricola( ) );

            return RedirectToAction( "ammesse" , new { idEcc = model.EccezioneAmmessa.id } );
        }

        public ActionResult Ammesse ( int? idEcc , Boolean? SoloConValidazioneJS )
        {
            if ( !UtenteHelper.IsAdmin( CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                return RedirectToAction( "/home/notauth" );
            }

            Boolean filtrovalJS = true;
            if ( SoloConValidazioneJS == null )
                filtrovalJS = false;
            else
                filtrovalJS = ( bool ) SoloConValidazioneJS;

            GestioneEccezioniAmmesseModel model = new GestioneEccezioniAmmesseModel( );
            model.ListaEccezioni = new digiGappEntities( ).MyRai_Eccezioni_Ammesse
                .Where( x => ( SoloConValidazioneJS == true && x.FunzioneJS != null && x.FunzioneJS != "" ) || ( SoloConValidazioneJS == null || SoloConValidazioneJS == false ) )
                .OrderBy( x => x.cod_eccezione )
                .Select( x => new EccezioneShort( )
                {
                    DescEccezione = x.cod_eccezione + "-" + x.desc_eccezione ,
                    id = x.id ,
                    selected = ( idEcc != null && idEcc == x.id ) ,
                    validazSelected = filtrovalJS
                } ).ToList( );

            if ( idEcc == null )
                model.EccezioneAmmessa = Copy( new digiGappEntities( ).MyRai_Eccezioni_Ammesse.OrderBy( x => x.cod_eccezione ).First( ) );
            else
                model.EccezioneAmmessa = Copy( new digiGappEntities( ).MyRai_Eccezioni_Ammesse.Where( x => x.id == idEcc ).FirstOrDefault( ) );

            model.StatoGiornataList = new SelectList(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Text = "", Value = ""},
                                    new SelectListItem { Text = "ASSING", Value ="ASSING"},
                                } , "Value" , "Text" );
            model.idEcc = model.EccezioneAmmessa.id;

            var db = new digiGappEntities( );
            model.RaggruppamentiList = new SelectList( db.MyRai_Raggruppamenti , "IdRaggruppamento" , "Descrizione" );
            model.WorkflowsList = new SelectList( db.MyRai_Workflows , "id" , "descrizione" );

            model.SidebarModel = UtenteHelper.getSidebarModel();// new sidebarModel(3);

            return View( model );
        }

        public ActionResult SavePar ( GestioneParametriModel model )
        {
            var db = new digiGappEntities( );
            MyRai_ParametriSistema p = db.MyRai_ParametriSistema.Where( x => x.Id == model.ParametroId ).FirstOrDefault( );
            p.Valore1 = model.ParametroValore1;
            p.Valore2 = model.ParametroValore2;

            db.SaveChanges( );

            return RedirectToAction( "parametri" , new { idPar = model.ParametroId } );
        }

        public ActionResult Parametri ( long? idPar )
        {
            if ( !UtenteHelper.IsAdmin( CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                return RedirectToAction( "/home/notauth" );
            }

            GestioneParametriModel model = new GestioneParametriModel( );
            model.SidebarModel = UtenteHelper.getSidebarModel();// new sidebarModel(3);
            model.ListaParametri = Getparametri( );

            model = GetDettaglioPar( idPar , model , model.ListaParametri[0] );
            return View( model );
        }

        public GestioneParametriModel GetDettaglioPar ( long? idPar , GestioneParametriModel model , ParametroDiSistema p )
        {
            var db = new digiGappEntities( );
            if ( idPar == null )
                idPar = p.id;
            var par = db.MyRai_ParametriSistema.Where( x => x.Id == idPar ).FirstOrDefault( );
            model.ParametroId = par.Id;
            model.ParametroChiave = par.Chiave;
            model.ParametroValore1 = par.Valore1;
            model.ParametroValore2 = par.Valore2;
            return model;
        }

        public List<ParametroDiSistema> Getparametri ( )
        {
            var db = new digiGappEntities( );
            List<ParametroDiSistema> L =
                 db.MyRai_ParametriSistema.OrderBy( x => x.Chiave ).Select( x => new ParametroDiSistema( )
                 {
                     id = x.Id ,
                     nomeparametro = x.Chiave
                 } ).OrderBy( x => x.nomeparametro ).ToList( );

            return L;
        }

        public EccezioneAmmessaModel Copy ( MyRai_Eccezioni_Ammesse m )
        {
            EccezioneAmmessaModel e = new EccezioneAmmessaModel( )
            {
                CaratteriMotivoRichiesta = m.CaratteriMotivoRichiesta ,
                Categoria = m.Categoria ,
                cod_eccezione = m.cod_eccezione ,
                controlli_specifici = m.controlli_specifici ,
                data_fine_validita = m.data_fine_validita ,
                data_inizio_validita = m.data_inizio_validita ,
                desc_eccezione = m.desc_eccezione ,
                flag_attivo = m.flag_attivo == null || m.flag_attivo == false ? false : true ,
                FunzioneJS = m.FunzioneJS ,
                id = m.id ,
                id_raggruppamento = m.id_raggruppamento ,
                OrarioEffettivo = m.OrarioEffettivo ,
                OrarioPrevisto = m.OrarioPrevisto ,
                OreInFuturo = m.OreInFuturo ,
                OreInPassato = m.OreInPassato ,
                PartialView = m.PartialView ,
                periodo = m.periodo ,
                Prontuario = m.Prontuario ,
                StatoGiornata = m.StatoGiornata ,
                TipiDipendente = m.TipiDipendente ,
                TipiDipendenteEsclusi = m.TipiDipendenteEsclusi ,
                tipo_controllo = m.tipo_controllo ,
                TipoGiornata = m.TipoGiornata ,
                ValoriParamExtraJSON = m.ValoriParamExtraJSON ,
                descrizione_eccezione = m.descrizione_eccezione ,
                id_workflow = m.id_workflow ,
                CodiceCsharp = m.CodiceCsharp
            };
            return e;
        }
    }
}