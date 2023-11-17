using myRai.Business;
using myRai.Models;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class DematerializzazioneController : Controller
    {
        #region RAI PER ME - Dipendente (Destinatario)

        public ActionResult Index ( )
        {
            DematerializzazioneModel model = new DematerializzazioneModel( );
            model.menuSidebar = Utente.getSidebarModel( );
            return View( model );
        }

        public ActionResult CaricaTabellaDocumenti ()
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );

            CaricaTabellaDocumentiVM model = new CaricaTabellaDocumentiVM( );
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>( );
            model.Matricola = matricola;
            model.Documenti = DematerializzazioneManager.GetDocumentiInviatiAMe( matricola );
            return View( "~/Views/Dematerializzazione/subpartial/elencoDocumenti.cshtml" , model );
        }

        public ActionResult CaricaFiltri()
        {
            Dematerializzazione_FiltroTipologia model = new Dematerializzazione_FiltroTipologia( );
            return View( "~/Views/Dematerializzazione/subpartial/filtri.cshtml" , model );
        }

        public ActionResult DownloadAllegato ( int idAllegato )
        {
            XR_ALLEGATI allegato = new XR_ALLEGATI( );

            try
            {
                allegato = DematerializzazioneManager.GetAllegato( idAllegato );
                if ( allegato == null )
                {
                    throw new Exception( "Impossibile trovare il file desiderato" );
                }
                string nomeFile = "prova.pdf";
                Stream stream = null;
                nomeFile = allegato.NomeFile;
                stream = new MemoryStream( allegato.ContentByte );

                return new FileStreamResult( stream , "application/pdf" ) { FileDownloadName = nomeFile };
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult GetDettaglioRichiesta ( int idDoc )
        {
            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM( );
            model.Richiesta = GetDocumentData( idDoc );

            if ( model.Richiesta != null )
            {
                model.NominativoUtenteApprovatore = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaApprovatore );
                model.NominativoUtenteCreatore = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaCreatore );
                model.NominativoUtenteDestinatario = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaDestinatario );
                model.NominativoUtenteFirma = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaFirma );
                model.NominativoUtenteIncaricato = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaIncaricato );
            }

            return View( "~/Views/Dematerializzazione/Modal_DettaglioRichiesta.cshtml" , model );
        }

        private RichiestaDoc GetDocumentData ( int idDocument )
        {
            RichiestaDoc result = null;

            try
            {
                IncentiviEntities db = new IncentiviEntities( );
                var item = db.XR_DEM_DOCUMENTI.Include( "XR_DEM_VERSIONI_DOCUMENTO" ).Where( w => w.Id.Equals( idDocument ) ).FirstOrDefault( );

                if (item != null)
                {
                    result = new RichiestaDoc();
                    result.Documento = new XR_DEM_DOCUMENTI();
                    result.Documento = item;
                    result.Allegati = new List<XR_ALLEGATI>();

                    var versioni = item.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id_Documento.Equals(item.Id) && !w.Deleted).OrderByDescending(w => w.Id).ThenByDescending(x => x.N_Versione).ToList();

                    if (versioni != null && versioni.Any())
                    {
                        List<int> idVersioni = new List<int>();
                        idVersioni.AddRange(versioni.Select(w => w.Id).ToList());

                        // prende tutti gli idversione e prende tutti gli idallegati associati agli idversione
                        var AllVers = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => idVersioni.Contains(w.IdVersione)).OrderByDescending(w => w.IdAllegato).ToList();

                        if (AllVers != null && AllVers.Any())
                        {
                            List<int> idsAllegati = new List<int>();
                            idsAllegati.AddRange(AllVers.Select(w => w.IdAllegato).ToList());
                            if (idsAllegati != null && idsAllegati.Any())
                            {
                                var all = db.XR_ALLEGATI.Where(w => idsAllegati.Contains(w.Id)).OrderByDescending(w => w.Id).ToList();
                                if (all != null && all.Any())
                                {
                                    // presi tutti gli allegati associati al documento
                                    // bisogna scartare quelli che hanno più versioni e
                                    // va presa solo la versione più recente
                                    // purtroppo per i vecchi documenti per vedere se c'è 
                                    // una versione differente dello stesso file, bisogna
                                    // controllare il nome file e idallegato
                                    // stesso nome file idallegato maggiore sarà la versione più recente
                                    // dell'allegato
                                    foreach (var a in all)
                                    {
                                        // prende l'allegato con lo stesso nome ma con id più grande
                                        int idMax = all.Where(w => w.NomeFile == a.NomeFile).Max(x => x.Id);

                                        // se già presente nell'elenco allora l'elemento è stato già analizzato
                                        bool giaEsiste = result.Allegati.Count(w => w.Id == idMax) == 1;

                                        // se non esiste lo aggiunge all'elenco finale
                                        if (!giaEsiste)
                                        {
                                            var inserire = result.Allegati.Where(w => w.Id == idMax).FirstOrDefault();
                                            result.Allegati.Add(inserire);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //if ( item != null )
                //{
                //    result = new RichiestaDoc( );
                //    result.Documento = new XR_DEM_DOCUMENTI( );
                //    result.Documento = item;
                //    result.Allegati = new List<XR_ALLEGATI>( );

                //    var versioni = item.XR_DEM_VERSIONI_DOCUMENTO.Where( w => w.Id_Documento.Equals( item.Id ) ).OrderByDescending( w => w.Id ).ToList( );

                //    if ( versioni != null && versioni.Any( ) )
                //    {
                //        foreach ( var v in versioni )
                //        {
                //            var AllVers = db.XR_DEM_ALLEGATI_VERSIONI.Where( w => w.IdVersione == v.Id ).FirstOrDefault( );

                //            if ( AllVers != null )
                //            {
                //                var allegato = db.XR_ALLEGATI.Where( w => w.Id.Equals( AllVers.IdAllegato ) ).FirstOrDefault( );

                //                if ( allegato != null )
                //                {
                //                    result.Allegati.Add( allegato );
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch ( Exception ex )
            {
                result = null;
            }

            return result;
        }

        public ActionResult GetFilePerIframe ( int idDoc )
        {
            try
            {
                RichiestaDoc documento = GetDocumentData( idDoc );
                XR_ALLEGATI principale = new XR_ALLEGATI( );
                principale = documento.Allegati.FirstOrDefault( w => w.IsPrincipal );
                byte[] byteArray = principale.ContentByte;
                string nomefile = principale.NomeFile;

                MemoryStream pdfStream = new MemoryStream( );
                pdfStream.Write( byteArray , 0 , byteArray.Length );
                pdfStream.Position = 0;

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = "documento" ,
                    Inline = true ,
                };

                Response.AddHeader( "Content-Disposition" , "inline; filename=" + nomefile + ".pdf" );
                return File( byteArray , "application/pdf" );
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        public ActionResult FiltraDocumenti(DateTime? datadal = null, int tipologia = -1 )
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );

            CaricaTabellaDocumentiVM model = new CaricaTabellaDocumentiVM( );
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>( );
            model.Matricola = matricola;
            model.Documenti = DematerializzazioneManager.GetDocumentiInviatiAMe( matricola, datadal , tipologia );
            return View( "~/Views/Dematerializzazione/subpartial/elencoDocumenti.cshtml" , model );
        }
        #endregion

        #region STATIC
        public static List<SelectListItem> GetTipologieDematerializzazioni ( )
        {
            List<SelectListItem> result = new List<SelectListItem>( );
            IncentiviEntities db = new IncentiviEntities( );
            string matricola = CommonManager.GetCurrentUserMatricola( );

            result.Add( new SelectListItem( )
            {
                Value = "-1" ,
                Text = "Tutti"
            } );

            var items = db.XR_DEM_DOCUMENTI.Where( w => w.PraticaAttiva &&
                w.Id_Stato == ( int ) StatiDematerializzazioneDocumenti.InviatoAlDipendente &&
                w.MatricolaDestinatario.Equals( matricola ) ).ToList( );

            if (items != null && items.Any())
            {
                var titoli = items.DistinctBy( w => w.Descrizione ).ToList( ).OrderBy( w => w.Descrizione ).ToList();

                result.AddRange( titoli.Select( x => new SelectListItem( )
                {
                    Value = x.Id_Tipo_Doc.ToString( ) ,
                    Text = x.Descrizione
                } ) );
            }

            return result;
        }

        public static List<SelectListItem> GetStatiRichiesta ()
        {
            List<SelectListItem> result = new List<SelectListItem>( );

            IncentiviEntities db = new IncentiviEntities( );

            result.Add(new SelectListItem( )
            {
                Value = "-1" ,
                Text = "Seleziona un valore"
            });

            var dati = db.XR_DEM_STATI.ToList( );
            if (dati != null && dati.Any( ))
            {
                foreach (var d in dati)
                {
                    result.Add(new SelectListItem( )
                    {
                        Value = d.ID_STATO.ToString( ) ,
                        Text = d.DESCRIZIONE
                    });
                }
            }

            return result;
        }
        #endregion

        #region RAI PER ME - (Firmatario)
        public ActionResult Firma ( )
        {
            DematerializzazioneInterfaceControllerScope.Instance.Obj = new DematerializzazioneOggettoPerNavigazione( );
            DematerializzazioneModel model = new DematerializzazioneModel( );
            model.menuSidebar = Utente.getSidebarModel( );
            return View( model );
        }

        public ActionResult CaricaDati ( )
        {
            dynamic showMessageString = string.Empty;

            string matricola = CommonManager.GetCurrentUserMatricola( );

            try
            {
                bool result = this.CaricaDatiInternal( );

                if ( !result )
                {
                    throw new Exception( "Errore durante il caricamento in cache." );
                }

                return Json( showMessageString , JsonRequestBehavior.AllowGet );
            }
            catch (Exception ex )
            {
                return new HttpStatusCodeResult( 404 , ex.Message );
            }
        }

        private bool CaricaDatiInternal()
        {
            bool result = true;

            try
            {
                string matricola = CommonManager.GetCurrentUserMatricola( );

                DematerializzazioneInterfaceControllerScope.Instance.Obj = new DematerializzazioneOggettoPerNavigazione( );

                DematerializzazioneInterfaceControllerScope.Instance.Obj.Matricola = matricola;
                DematerializzazioneInterfaceControllerScope.Instance.Obj.IdDocumentoCorrente = 0;
                DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare = new List<XR_DEM_DOCUMENTI_EXT>( );
                DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati = new List<XR_DEM_DOCUMENTI_EXT>( );
                DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma = new List<XR_DEM_DOCUMENTI_EXT>( );

                DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare = DematerializzazioneManager.GetDocumentiDaLavorare( matricola );
                DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati = DematerializzazioneManager.GetDocumentiRifiutatiInFirma(matricola);
                DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma = DematerializzazioneManager.GetDocumentiInFirma( matricola );

                if ( DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare != null &&
                    DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare.Any( ) )
                {
                    DematerializzazioneInterfaceControllerScope.Instance.Obj.PosizioneCorrente = 1;
                    DematerializzazioneInterfaceControllerScope.Instance.Obj.IdDocumentoCorrente = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare.FirstOrDefault( ).Id;
                }
                else
                {
                    DematerializzazioneInterfaceControllerScope.Instance.Obj.PosizioneCorrente = 0;
                    DematerializzazioneInterfaceControllerScope.Instance.Obj.IdDocumentoCorrente = 0;
                }
            }
            catch(Exception ex)
            {
                result = false;
            }
            return result;
        }

        public ActionResult CaricaTabellaDocumentiDaLavorare ( )
        {
            if (DematerializzazioneInterfaceControllerScope.Instance.Obj == null)
            {
                DematerializzazioneInterfaceControllerScope.Instance.Obj = new DematerializzazioneOggettoPerNavigazione( );

                bool result = this.CaricaDatiInternal( );

                if (!result)
                {
                    throw new Exception("Errore durante il caricamento in cache.");
                }
            }

            string matricola = CommonManager.GetCurrentUserMatricola( );

            CaricaTabellaDocumentiVM model = new CaricaTabellaDocumentiVM( );
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>( );
            model.DocumentiDaLavorare = new List<XR_DEM_DOCUMENTI_EXT>( );
            model.DocumentiRifiutati = new List<XR_DEM_DOCUMENTI_EXT>( );
            model.Matricola = matricola;
            model.DocumentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare;
            model.DocumentiRifiutati = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati;
            return View( "~/Views/Dematerializzazione/subpartial/elencoDocumentiInFirma.cshtml" , model );
        }

        public ActionResult RefreshTabellaDocumentiDaLavorare()
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );

            CaricaTabellaDocumentiVM model = new CaricaTabellaDocumentiVM( );
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>( );
            model.DocumentiDaLavorare = new List<XR_DEM_DOCUMENTI_EXT>( );
            model.DocumentiRifiutati = new List<XR_DEM_DOCUMENTI_EXT>( );
            model.Matricola = matricola;
            model.DocumentiDaLavorare = DematerializzazioneManager.GetDocumentiDaLavorare(matricola);
            model.DocumentiRifiutati = DematerializzazioneManager.GetDocumentiRifiutatiInFirma(matricola);
            return View("~/Views/Dematerializzazione/subpartial/elencoDocumentiInFirma.cshtml" , model);
        }

        public ActionResult CaricaFiltriInFirma()
        {
            Dematerializzazione_FiltroFirmaVM model = new Dematerializzazione_FiltroFirmaVM( );
            return View( "~/Views/Dematerializzazione/subpartial/filtriFirma.cshtml" , model );
        }

        public ActionResult CaricaDocumentiInFirma ()
        {
            if (DematerializzazioneInterfaceControllerScope.Instance.Obj == null)
            {
                DematerializzazioneInterfaceControllerScope.Instance.Obj = new DematerializzazioneOggettoPerNavigazione( );

                DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma = new List<XR_DEM_DOCUMENTI_EXT>( );
                DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare = new List<XR_DEM_DOCUMENTI_EXT>( );
                DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati = new List<XR_DEM_DOCUMENTI_EXT>( );
                DematerializzazioneInterfaceControllerScope.Instance.Obj.PosizioneCorrente = 1;

                bool result = this.CaricaDatiInternal( );

                if (!result)
                {
                    throw new Exception("Errore durante il caricamento in cache.");
                }
            }

            Dematerializzazione_DocumentiInFirmaVM model = new Dematerializzazione_DocumentiInFirmaVM( );
            model.ConteggioDocumentiInFirma = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma.Count( );
            return View("~/Views/Dematerializzazione/subpartial/carrello.cshtml" , model);
        }

        [HttpGet]
        public ActionResult GetDettaglioRichiestaInFirma ( int idDoc, bool forceReset = true  )
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );

            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM( );
            model.Richiesta = GetDocumentData( idDoc );
            model.Navigazione = new DematerializzazioneOggettoPerNavigazione( );

            if ( DematerializzazioneInterfaceControllerScope.Instance.Obj == null || forceReset )
            {
                DematerializzazioneInterfaceControllerScope.Instance.Obj = new DematerializzazioneOggettoPerNavigazione( );

                bool result = this.CaricaDatiInternal( );

                if ( !result )
                {
                    throw new Exception( "Errore durante il caricamento in cache." );
                }
            }

            model.Navigazione.DocumentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare;
            model.Navigazione.DocumentiRifiutati = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati;
            model.Navigazione.DocumentiInFirma = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma;

            model.Navigazione.IdDocumentoCorrente = idDoc;
            var doc = model.Navigazione.DocumentiDaLavorare.Where( w => w.Id == idDoc ).FirstOrDefault( );
            model.Navigazione.PosizioneCorrente = model.Navigazione.DocumentiDaLavorare.IndexOf( doc ) + 1;
            //model.Navigazione.HasNext = true;
            //model.Navigazione.HasPrev = true;
            //if (model.Navigazione.PosizioneCorrente == model.Navigazione.DocumentiDaLavorare.Count())
            //{
            //    model.Navigazione.HasNext = false;
            //}

            //if (model.Navigazione.PosizioneCorrente == 1)
            //{
            //    model.Navigazione.HasPrev = true;
            //}

            if ( model.Richiesta != null )
            {
                model.NominativoUtenteApprovatore = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaApprovatore );
                model.NominativoUtenteCreatore = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaCreatore );
                model.NominativoUtenteDestinatario = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaDestinatario );
                model.NominativoUtenteFirma = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaFirma );
                model.NominativoUtenteIncaricato = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaIncaricato );
            }

            if (model.Navigazione.DocumentiInFirma != null && model.Navigazione.DocumentiInFirma.Any())
            {
                model.DocumentoInFirma = model.Navigazione.DocumentiInFirma.Count(w => w.Id == idDoc) > 0;
            }
            return View( "~/Views/Dematerializzazione/Modal_WizardFirma.cshtml" , model );
        }

        #region NAVIGAZIONE NEL WIZARD
        [HttpGet]
        public ActionResult GetDettaglioRichiestaInFirmaPrev ( int idDoc)
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );

            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM( );

            if ( DematerializzazioneInterfaceControllerScope.Instance.Obj == null )
            {
                DematerializzazioneInterfaceControllerScope.Instance.Obj = new DematerializzazioneOggettoPerNavigazione( );

                bool result = this.CaricaDatiInternal( );

                if ( !result )
                {
                    throw new Exception( "Errore durante il caricamento in cache." );
                }
            }
            model.Navigazione = new DematerializzazioneOggettoPerNavigazione( );

            model.Navigazione.DocumentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare;
            model.Navigazione.DocumentiRifiutati = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati;
            model.Navigazione.DocumentiInFirma = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma;
            
            var doc = model.Navigazione.DocumentiDaLavorare.Where( w => w.Id == idDoc ).FirstOrDefault( );
            
            int pos = model.Navigazione.DocumentiDaLavorare.IndexOf( doc ) + 1;
            var newCurr = model.Navigazione.DocumentiDaLavorare[pos - 2];
            model.Navigazione.IdDocumentoCorrente = newCurr.Id;
            model.Navigazione.PosizioneCorrente = pos - 1;

            model.Richiesta = GetDocumentData( newCurr.Id );

            if ( model.Richiesta != null )
            {
                model.NominativoUtenteApprovatore = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaApprovatore );
                model.NominativoUtenteCreatore = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaCreatore );
                model.NominativoUtenteDestinatario = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaDestinatario );
                model.NominativoUtenteFirma = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaFirma );
                model.NominativoUtenteIncaricato = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaIncaricato );
            }

            if (model.Navigazione.DocumentiInFirma != null && model.Navigazione.DocumentiInFirma.Any( ))
            {
                model.DocumentoInFirma = model.Navigazione.DocumentiInFirma.Count(w => w.Id == newCurr.Id) > 0;
            }

            model.Navigazione.DocumentiDaLavorare = DematerializzazioneManager.GetDocumentiDaLavorare(matricola);

            return View( "~/Views/Dematerializzazione/Modal_WizardFirma.cshtml" , model );
        }

        [HttpGet]
        public ActionResult GetDettaglioRichiestaInFirmaNext ( int idDoc, bool canRestartIndex = false )
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );

            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM( );

            if ( DematerializzazioneInterfaceControllerScope.Instance.Obj == null )
            {
                DematerializzazioneInterfaceControllerScope.Instance.Obj = new DematerializzazioneOggettoPerNavigazione( );

                bool result = this.CaricaDatiInternal( );

                if ( !result )
                {
                    throw new Exception( "Errore durante il caricamento in cache." );
                }
            }
            model.Navigazione = new DematerializzazioneOggettoPerNavigazione( );

            model.Navigazione.DocumentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare;
            model.Navigazione.DocumentiRifiutati = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati;
            model.Navigazione.DocumentiInFirma = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma;

            var doc = model.Navigazione.DocumentiDaLavorare.Where( w => w.Id == idDoc ).FirstOrDefault( );

            int pos = model.Navigazione.DocumentiDaLavorare.IndexOf( doc ) + 1;
            
            if (canRestartIndex)
            {
                // se era l'ultimo documento
                if (pos == model.Navigazione.DocumentiDaLavorare.Count( ))
                {
                    pos = 0;
                }
            }

            XR_DEM_DOCUMENTI_EXT newCurr = model.Navigazione.DocumentiDaLavorare[pos];
            model.Navigazione.IdDocumentoCorrente = newCurr.Id;
            model.Navigazione.PosizioneCorrente = pos + 1;

            model.Richiesta = GetDocumentData( newCurr.Id );

            if ( model.Richiesta != null )
            {
                model.NominativoUtenteApprovatore = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaApprovatore );
                model.NominativoUtenteCreatore = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaCreatore );
                model.NominativoUtenteDestinatario = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaDestinatario );
                model.NominativoUtenteFirma = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaFirma );
                model.NominativoUtenteIncaricato = DematerializzazioneManager.GetNominativoByMatricola( model.Richiesta.Documento.MatricolaIncaricato );
            }

            if (model.Navigazione.DocumentiInFirma != null && model.Navigazione.DocumentiInFirma.Any( ))
            {
                model.DocumentoInFirma = model.Navigazione.DocumentiInFirma.Count(w => w.Id == newCurr.Id) > 0;
            }

            return View( "~/Views/Dematerializzazione/Modal_WizardFirma.cshtml" , model );
        }

        #endregion

        #region BOTTONI AZIONE NEL WIZARD
        [HttpGet]
        public ActionResult SaltaDocumento(int idDoc)
        {
            dynamic showMessageString = string.Empty;

            string matricola = CommonManager.GetCurrentUserMatricola( );
            bool lastItem = false;
            try
            {
                DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM( );

                if ( DematerializzazioneInterfaceControllerScope.Instance.Obj == null )
                {
                    DematerializzazioneInterfaceControllerScope.Instance.Obj = new DematerializzazioneOggettoPerNavigazione( );

                    bool result = this.CaricaDatiInternal( );

                    if ( !result )
                    {
                        throw new Exception( "Errore durante il caricamento in cache." );
                    }
                }
                model.Navigazione = new DematerializzazioneOggettoPerNavigazione( );

                var item = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare.Where( w => w.Id == idDoc ).FirstOrDefault( );

                int pos = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare.IndexOf( item ) + 1;

                // true se è l'ultimo elemento della lista
                lastItem = pos == DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare.Count( );

                DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare.Where( w => w.Id != idDoc ).ToList( );

                model.Navigazione.DocumentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare;
                model.Navigazione.DocumentiRifiutati = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati;
                model.Navigazione.DocumentiInFirma = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma;
                int newId = 0;
                if ( lastItem )
                {
                    newId = 0;
                }
                else
                {
                    // se non è l'ultimo elemento, calcola la posizione dell'elemento precedente,
                    // in questo modo la navigazione all'elemento successivo che verrà richiamata
                    // a seguito di questo metodo navigherà all'elemento corretto della nuova lista
                    newId = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare[pos - 1].Id;

                }
                return Json( new { success = true , Id = newId } , JsonRequestBehavior.AllowGet );
            }
            catch ( Exception ex )
            {
                return Json( new { success = false , message = ex.Message } , JsonRequestBehavior.AllowGet );
            }
        }

        public ActionResult RifiutaDocumento (int idDoc , string motivo)
        {
            dynamic showMessageString = string.Empty;
            string matricola = CommonManager.GetCurrentUserMatricola( );
            string nominativo = CommonManager.GetNominativoPerMatricola(matricola);

            try
            {
                IncentiviEntities db = new IncentiviEntities( );
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDoc)).FirstOrDefault( );

                item.Id_Stato = ( int ) StatiDematerializzazioneDocumenti.RifiutatoFirma;
                item.NotaFirma = motivo;
                item.DataRifiuto = DateTime.Now;

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI( )
                {
                    COD_TERMID = Request.UserHostAddress ,
                    COD_USER = matricola ,
                    DTA_OPERAZIONE = 0 ,
                    COD_TIPO_PRATICA = "DEM" ,
                    ID_GESTIONE = item.Id ,
                    ID_PERSONA = 0 ,
                    ID_STATO = item.Id_Stato ,
                    ID_TIPOLOGIA = item.Id_WKF_Tipologia ,
                    NOMINATIVO = nominativo ,
                    VALID_DTA_INI = DateTime.Now ,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.SaveChanges( );

                return Json(showMessageString , JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404 , "Errore durante il rifiuto del documento");
            }
        }

        public ActionResult MettiInFirma (int idDoc , string motivo)
        {
            dynamic showMessageString = string.Empty;
            string matricola = CommonManager.GetCurrentUserMatricola( );
            string nominativo = CommonManager.GetNominativoPerMatricola(matricola);

            try
            {
                //var documentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare;
                //var doc = documentiDaLavorare.Where(w => w.Id == idDoc).FirstOrDefault();

                IncentiviEntities db = new IncentiviEntities( );
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDoc)).FirstOrDefault( );

                item.NotaFirma = motivo;

                //db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI( )
                //{
                //    COD_TERMID = Request.UserHostAddress ,
                //    COD_USER = matricola ,
                //    DTA_OPERAZIONE = 0 ,
                //    COD_TIPO_PRATICA = "DEM" ,
                //    ID_GESTIONE = item.Id ,
                //    ID_PERSONA = 0 ,
                //    ID_STATO = item.Id_Stato ,
                //    ID_TIPOLOGIA = item.Id_WKF_Tipologia ,
                //    NOMINATIVO = nominativo ,
                //    VALID_DTA_INI = DateTime.Now ,
                //    TMS_TIMESTAMP = DateTime.Now
                //});
                db.SaveChanges( );

                using (digiGappEntities dbe = new digiGappEntities( ))
                {
                    MyRai_CarrelloGenerico newItem = new MyRai_CarrelloGenerico( )
                    {
                        id_documento = item.Id ,
                        data_creazione = DateTime.Now ,
                        tabella = "XR_DEM_DOCUMENTI" ,
                        matricola = matricola
                    };

                    dbe.MyRai_CarrelloGenerico.Add(newItem);

                    dbe.SaveChanges( );
                }

                var documentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare;
                var doc = documentiDaLavorare.Where(w => w.Id == idDoc).FirstOrDefault();                
                int pos = documentiDaLavorare.IndexOf(doc) + 1;

                if (pos == documentiDaLavorare.Count())
                {
                    showMessageString = 0;
                }
                else
                {
                    var itemsRimasti = DematerializzazioneManager.GetDocumentiDaLavorare(matricola);
                    if (itemsRimasti == null || !itemsRimasti.Any())
                    {
                        showMessageString = 0;
                    }
                    else
                    {
                        showMessageString = idDoc;
                    }
                }

                DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma = DematerializzazioneManager.GetDocumentiInFirma(matricola);

                return Json(showMessageString , JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404 , "Errore durante il rifiuto del documento");
            }
        }

        [HttpGet]
        public ActionResult RimuoviDaCarrello(int idDoc ,string motivo)
        {
            dynamic showMessageString = string.Empty;

            string matricola = CommonManager.GetCurrentUserMatricola( );
            string nominativo = CommonManager.GetNominativoPerMatricola(matricola);

            try
            {
                IncentiviEntities db = new IncentiviEntities( );
                var element = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDoc)).FirstOrDefault( );

                if (element == null)
                {
                    throw new Exception("Documento non trovato");
                }

                if (!String.IsNullOrEmpty(motivo))
                {
                    element.NotaFirma = motivo;
                }

                db.SaveChanges( );

                using (digiGappEntities dbe = new digiGappEntities( ))
                {
                    var toRemove = dbe.MyRai_CarrelloGenerico.Where(w =>
                    w.tabella.Equals("XR_DEM_DOCUMENTI") &&
                    w.id_documento.Equals(idDoc)).FirstOrDefault( );

                    dbe.MyRai_CarrelloGenerico.Remove(toRemove);

                    dbe.SaveChanges( );
                }

                DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM( );

                DematerializzazioneInterfaceControllerScope.Instance.Obj = new DematerializzazioneOggettoPerNavigazione( );

                bool result = this.CaricaDatiInternal( );

                if (!result)
                {
                    throw new Exception("Errore durante il caricamento in cache.");
                }
                model.Navigazione = new DematerializzazioneOggettoPerNavigazione( );

                var item = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare.Where(w => w.Id == idDoc).FirstOrDefault( );
                int pos = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare.IndexOf(item) + 1;

                DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare.Where(w => w.Id != idDoc).ToList( );

                model.Navigazione.DocumentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare;
                model.Navigazione.DocumentiRifiutati = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati;
                model.Navigazione.DocumentiInFirma = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma;

                return Json(new { success = true , Id = idDoc } , JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false , message = ex.Message } , JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        public ActionResult VisualizzatoreAllegato ( int idAllegato )
        {
            IncentiviEntities db = new IncentiviEntities( );
            XR_ALLEGATI allegato = new XR_ALLEGATI( );
            
            try
            {
                allegato = db.XR_ALLEGATI.Where( w => w.Id.Equals( idAllegato ) ).FirstOrDefault( );
                if ( allegato == null )
                {
                    throw new Exception( "Errore impossibile reperire il file" );
                }
            }
            catch ( Exception ex )
            {
                throw ex;
            }
            return View( "~/Views/Dematerializzazione/subpartial/_visualizzatoreAllegato.cshtml" , allegato );
        }

        public ActionResult GetAllegatoPerIframe ( int idAllegato )
        {
            IncentiviEntities db = new IncentiviEntities( );
            XR_ALLEGATI allegato = new XR_ALLEGATI( );

            try
            {
                allegato = db.XR_ALLEGATI.Where( w => w.Id.Equals( idAllegato ) ).FirstOrDefault( );
                if ( allegato == null )
                {
                    throw new Exception( "Errore impossibile reperire il file" );
                }

                byte[] byteArray = allegato.ContentByte;
                string nomefile = allegato.NomeFile;

                MemoryStream pdfStream = new MemoryStream( );
                pdfStream.Write( byteArray , 0 , byteArray.Length );
                pdfStream.Position = 0;

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = nomefile ,
                    Inline = true ,
                };

                Response.AddHeader( "Content-Disposition" , "inline; filename=" + nomefile + ".pdf" );
                return File( byteArray , "application/pdf" );
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        /// <summary>
        /// Restituisce il bytearray dell'allegato principale
        /// da visualizzare all'interno di un Iframe
        /// 
        /// </summary>
        /// <param name="idDocumento"></param>
        /// <returns></returns>
        public ActionResult GetAllegatoPerIframeByIdDoc ( int idDocumento )
        {
            IncentiviEntities db = new IncentiviEntities( );

            XR_ALLEGATI allegato = new XR_ALLEGATI( );

            try
            {
                var item = GetDocumentData( idDocumento );

                if ( item == null || item.Allegati == null || !item.Allegati.Any( ) )
                {
                    throw new Exception( "Errore nel reperimento del documento" );
                }

                allegato = item.Allegati.LastOrDefault( w => w.IsPrincipal );
                if ( allegato == null )
                {
                    throw new Exception( "Errore impossibile reperire il file" );
                }

                byte[] byteArray = null;

                if (allegato.ContentBytePDF != null )
                {
                    byteArray = allegato.ContentBytePDF;
                }
                else
                {
                    byteArray = allegato.ContentByte;
                }

                string nomefile = allegato.NomeFile;

                MemoryStream pdfStream = new MemoryStream( );
                pdfStream.Write( byteArray , 0 , byteArray.Length );
                pdfStream.Position = 0;

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = nomefile ,
                    Inline = true ,
                };

                Response.AddHeader( "Content-Disposition" , "inline; filename=" + nomefile + ".pdf" );
                return File( byteArray , "application/pdf" );
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        public ActionResult FiltraDocumentiInFirma ( DateTime? datadal = null , string nominativo = null, int tipologia = -1 )
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );

            CaricaTabellaDocumentiVM model = new CaricaTabellaDocumentiVM( );
            model.Matricola = matricola;
            model.DocumentiDaLavorare = DematerializzazioneManager.GetDocumentiDaLavorare( matricola , datadal , tipologia , nominativo );
            model.DocumentiRifiutati = DematerializzazioneManager.GetDocumentiRifiutatiInFirma(matricola , datadal , tipologia , nominativo);
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>( );            
            return View( "~/Views/Dematerializzazione/subpartial/elencoDocumentiInFirma.cshtml" , model );
        }

        #region MODALE FIRMA DOCUMENTI
        
        [HttpGet]
        public ActionResult GetModalFirmaDocs()
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );

            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM( );
            model.Navigazione = new DematerializzazioneOggettoPerNavigazione( );

            if (DematerializzazioneInterfaceControllerScope.Instance.Obj == null)
            {
                DematerializzazioneInterfaceControllerScope.Instance.Obj = new DematerializzazioneOggettoPerNavigazione( );

                bool result = this.CaricaDatiInternal( );

                if (!result)
                {
                    throw new Exception("Errore durante il caricamento in cache.");
                }
            }

            model.Navigazione.DocumentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare;
            model.Navigazione.DocumentiRifiutati = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati;
            model.Navigazione.DocumentiInFirma = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma;

            return View("~/Views/Dematerializzazione/Modal_FirmaDocs.cshtml" , model);
        }

        [HttpPost]
        public ActionResult FirmaDocumentiNelCarrello ( string otp , string pwd , string pmatr , string nom )
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );
            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM( );

            //test: Password01
            //test: 033193
            var db = new myRaiData.digiGappEntities( );
            string Impers = null;
            if ( !String.IsNullOrWhiteSpace( pmatr ) )
            {
                Impers = "Matr imp: " + pmatr + " - Nom:" + nom;
            }

            myRaiData.MyRai_LogAzioni a = new myRaiData.MyRai_LogAzioni( )
            {
                applicativo = "PORTALE" ,
                data = DateTime.Now ,
                matricola = CommonManager.GetCurrentUserMatricola( ) ,
                operazione = "FirmaDocumentiNelCarrello-Request" ,
                provenienza = "Dematerializzazione.FirmaDocumentiNelCarrello" ,
                descrizione_operazione = Impers
            };

            db.MyRai_LogAzioni.Add( a );
            db.SaveChanges( );

            FirmaDocumentiResponse response = DaFirmareManager.DEM_FirmaDocumenti( otp , pwd , pmatr , nom );

            string DocsInErr = "";
            if ( response.DocsInErrore != null && response.DocsInErrore.Any( ) )
            {
                foreach ( DocInErrore d in response.DocsInErrore )
                {
                    if ( d == null )
                        continue;
                    DocsInErr += d.sedegapp + ": " + d.data_inizio + "/" + d.data_fine + ":" + d.esito + ", \r\n";
                }
            }

            myRaiData.MyRai_LogAzioni az = new myRaiData.MyRai_LogAzioni( )
            {
                applicativo = "PORTALE" ,
                data = DateTime.Now ,
                matricola = CommonManager.GetCurrentUserMatricola( ) ,
                operazione = "FirmaDocumentiNelCarrello-Request" ,
                provenienza = "Dematerializzazione.FirmaDocumentiNelCarrello" ,
                descrizione_operazione = ( response != null ? response.esito : "" ) + DocsInErr
            };
            db.MyRai_LogAzioni.Add( az );
            db.SaveChanges( );

            model.Navigazione = new DematerializzazioneOggettoPerNavigazione( );

            bool result = this.CaricaDatiInternal( );

            if ( !result )
            {
                throw new Exception( "Errore durante il caricamento in cache." );
            }

            model.Navigazione.DocumentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare;
            model.Navigazione.DocumentiRifiutati = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati;
            model.Navigazione.DocumentiInFirma = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma;

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new
                {
                    result = response.esito ,
                    firmati = response.FirmatiOk ,
                    incarrello = model.Navigazione.DocumentiInFirma.Count() ,
                    isautherror = response.IsAuthError ,
                    erroridocs = ( response.esito != "OK" ? response.esito : "" ) + DocsInErr,
                    infoAggiuntive = response.InfoAggiuntive
                }
            };
        }

        public ActionResult LoadElencoInFirma()
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );

            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM( );
            model.Navigazione = new DematerializzazioneOggettoPerNavigazione( );
            DematerializzazioneInterfaceControllerScope.Instance.Obj = new DematerializzazioneOggettoPerNavigazione( );

            bool result = this.CaricaDatiInternal( );

            if (!result)
            {
                throw new Exception("Errore durante il caricamento in cache.");
            }

            model.Navigazione.DocumentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare;
            model.Navigazione.DocumentiRifiutati = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati;
            model.Navigazione.DocumentiInFirma = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma;

            return View("~/Views/Dematerializzazione/subpartial/_tbl_elenco_in_firma.cshtml" , model);
        }














        [HttpPost]
        public ActionResult FirmaDocumentiNelCarrelloREST(string otp, string pwd, string pmatr, string nom)
        {
            string matricola = CommonManager.GetCurrentUserMatricola();
            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM();

            //test: Password01
            //test: 033193
            var db = new myRaiData.digiGappEntities();
            string Impers = null;
            if (!String.IsNullOrWhiteSpace(pmatr))
            {
                Impers = "Matr imp: " + pmatr + " - Nom:" + nom;
            }

            myRaiData.MyRai_LogAzioni a = new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = CommonManager.GetCurrentUserMatricola(),
                operazione = "FirmaDocumentiNelCarrello-Request",
                provenienza = "Dematerializzazione.FirmaDocumentiNelCarrello",
                descrizione_operazione = Impers
            };

            db.MyRai_LogAzioni.Add(a);
            db.SaveChanges();

            FirmaDocumentiResponse response = DaFirmareManager.TestFirma(otp, pwd, pmatr, nom);

            string DocsInErr = "";
            if (response.DocsInErrore != null && response.DocsInErrore.Any())
            {
                foreach (DocInErrore d in response.DocsInErrore)
                {
                    if (d == null)
                        continue;
                    DocsInErr += d.sedegapp + ": " + d.data_inizio + "/" + d.data_fine + ":" + d.esito + ", \r\n";
                }
            }

            myRaiData.MyRai_LogAzioni az = new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = CommonManager.GetCurrentUserMatricola(),
                operazione = "FirmaDocumentiNelCarrello-Request",
                provenienza = "Dematerializzazione.FirmaDocumentiNelCarrello",
                descrizione_operazione = (response != null ? response.esito : "") + DocsInErr
            };
            db.MyRai_LogAzioni.Add(az);
            db.SaveChanges();

            model.Navigazione = new DematerializzazioneOggettoPerNavigazione();

            bool result = this.CaricaDatiInternal();

            if (!result)
            {
                throw new Exception("Errore durante il caricamento in cache.");
            }

            model.Navigazione.DocumentiDaLavorare = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiDaLavorare;
            model.Navigazione.DocumentiRifiutati = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiRifiutati;
            model.Navigazione.DocumentiInFirma = DematerializzazioneInterfaceControllerScope.Instance.Obj.DocumentiInFirma;

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = response.esito,
                    firmati = response.FirmatiOk,
                    incarrello = model.Navigazione.DocumentiInFirma.Count(),
                    isautherror = response.IsAuthError,
                    erroridocs = (response.esito != "OK" ? response.esito : "") + DocsInErr,
                    infoAggiuntive = response.InfoAggiuntive
                }
            };
        }







        #endregion

        #endregion

        public class DematerializzazioneInterfaceControllerScope : SessionScope<DematerializzazioneInterfaceControllerScope>
        {
            public DematerializzazioneInterfaceControllerScope ( )
            {
                this._DematerializzazioneOggettoPerNavigazione = new DematerializzazioneOggettoPerNavigazione( );
            }

            public DematerializzazioneOggettoPerNavigazione Obj
            {
                get
                {
                    return this._DematerializzazioneOggettoPerNavigazione;
                }
                set
                {
                    this._DematerializzazioneOggettoPerNavigazione = value;
                }
            }

            private DematerializzazioneOggettoPerNavigazione _DematerializzazioneOggettoPerNavigazione = null;
        }
    }
}