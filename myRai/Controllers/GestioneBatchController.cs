using myRai.Business;
using myRaiCommonModel.GestioneBatch;
using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRaiHelper;

namespace myRai.Controllers
{
    public class GestioneBatchController : BaseCommonController
    {
        [HttpGet]
        public ActionResult Index ( )
        {
            GestioneBatchVM model = new GestioneBatchVM( );
            model.ReportOnDemandItems = new List<ReportOnDemandItem>( );

            bool avvioForzato = false;
            bool giaInEsecuzione = false;
            bool attivo = CommonManager.GetParametro<bool>( EnumParametriSistema.EsportazionePresenzeDipendenti );

            // Creazione del modello
            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    MyRai_ParametriSistema parAvvioForzato = db.MyRai_ParametriSistema.Where( w => w.Chiave.Equals( "EsportazionePresenzeDipendentiAvvioForzato" ) ).FirstOrDefault( );

                    if ( parAvvioForzato != null )
                    {
                        Boolean.TryParse( parAvvioForzato.Valore1 , out avvioForzato );
                        Boolean.TryParse( parAvvioForzato.Valore2 , out giaInEsecuzione );
                    }
                }

                // creazione delle varie righe del modello
                model.ReportOnDemandItems.Add( new ReportOnDemandItem( )
                {
                    Titolo = "Report presenze" ,
                    Descrizione = "Riporta le presenze di tutti gli insediamenti aziendali" ,
                    Attivo = attivo ,
                    AvvioImmediato = avvioForzato ,
                    GiaInEsecuzione = giaInEsecuzione ,
                } );

            }
            catch ( Exception ex )
            {
                model.ReportOnDemandItems = new List<ReportOnDemandItem>( );
            }

            // conteggio delle elaborazioni in corso
            int elab = model.ReportOnDemandItems.Count( w => w.AvvioImmediato && w.GiaInEsecuzione );

            model.ElaborazioniInCorso = elab;

            return View( model );
        }

        [HttpPost]
        public ActionResult AvviaBatchPresenze ( )
        {
            ReportOnDemandJsonResult result = new ReportOnDemandJsonResult( );

            result.Error = null;
            result.Result = false;
            result.ElaborazioniInCorso = 0;

            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    var par = db.MyRai_ParametriSistema.Where( w => w.Chiave.Equals( "EsportazionePresenzeDipendentiAvvioForzato" ) ).FirstOrDefault( );
                    if ( par != null )
                    {
                        par.Valore1 = "True";
                    }
                    db.SaveChanges( );
                    result.Result = true;
                    result.ElaborazioniInCorso = 1;
                }
            }
            catch ( Exception ex )
            {
                result.Result = false;
                result.Error = ex.Message;
                result.ElaborazioniInCorso = 0;
            }
            return Json( result );
        }

    }
}
