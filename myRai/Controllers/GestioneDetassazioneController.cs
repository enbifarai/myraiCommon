using myRaiCommonManager;
using myRaiCommonModel.Detassazione;
using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class GestioneDetassazioneController : BaseCommonController
    {
        #region Views Gestionale Detassazione

        /// <summary>
        /// Caricamento della pagina principale
        /// </summary>
        /// <returns></returns>
        public ActionResult Index ( )
        {
            this.InitializeViewBagModelli( null );
            this.InitializeViewBagStati( 1 );
            return View( );
        }

        /// <summary>
        /// Caricamento dell'elenco dei dipendenti in base ai filtri passati
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="nominativo"></param>
        /// <param name="modello"></param>
        /// <param name="stato"></param>
        /// <returns></returns>
        public ActionResult ListaDipendenti ( int pagina = 1 , string matricola = null , string nominativo = "" , int? modello = null , int? stato = null )
        {
            DetassazioneListaDipendentiVM model = new DetassazioneListaDipendentiVM( );

            model = ListaDipendentiInternal( pagina , matricola , nominativo , modello , stato );

            if (String.IsNullOrEmpty(matricola) && String.IsNullOrEmpty( nominativo ) &&
                !modello.HasValue && stato.HasValue && stato.GetValueOrDefault() == 1)
            {
                model.DescrizioneTab = "Ultimi modelli compilati";
            }
            else if ( String.IsNullOrEmpty( matricola ) && String.IsNullOrEmpty( nominativo ) &&
                        !modello.HasValue && stato.HasValue && stato.GetValueOrDefault( ) == 0 )
            {
                model.DescrizioneTab = "Ultimi modelli non compilati";
            }
            else if ( String.IsNullOrEmpty( matricola ) && String.IsNullOrEmpty( nominativo ) &&
            !modello.HasValue && !stato.HasValue )
            {
                model.DescrizioneTab = "Ultimi modelli";
            }
            else
            {
                string msg = "";
                msg += ( !String.IsNullOrEmpty( matricola ) ) ? 
                            "Matricola <b>" + matricola + "</b>" : 
                            "";
                msg += ( !String.IsNullOrEmpty( nominativo ) ) ?
                        ( msg.Length > 0 ) ? ", <b>" + nominativo.ToUpper() + "</b>" : "<b>" + nominativo.ToUpper() + "</b>" :
                        "";
                msg += ( modello.HasValue ) ?
                        ( msg.Length > 0 ) ? 
                            ", <b>" + String.Format( "{0}C" , modello.GetValueOrDefault( ) ) + "</b>" : 
                            "<b>" + String.Format( "{0}C" , modello.GetValueOrDefault( ) ) + "</b>" :
                        "";
                msg += ( stato.HasValue ) ?
                        ( msg.Length > 0 ) ? 
                            ", <b>" + ( ( stato.GetValueOrDefault( ) == 1 ) ? "Compilato" : "Non compilato" ) + "</b>" : 
                            "<b>" + ( ( stato.GetValueOrDefault( ) == 1 ) ? "Compilato" : "Non compilato" ) + "</b>" :
                        "";
                model.DescrizioneTab = String.Format( "Hai cercato: {0}" , msg );
            }

            // verifica se l'utente corrente è abilitato al reset dei dati
            model.UtenteAbilitatoModifica = AbilitatoReset();

            return View( "~/Views/GestioneDetassazione/subpartial/_listaDipendenti.cshtml" , model );
        }

        /// <summary>
        /// Caricamento dei riepiloghi
        /// </summary>
        /// <returns></returns>
        public ActionResult ReportDetassazione ( )
        {
            DetassazioneGraficiModel model = new DetassazioneGraficiModel( );

            using ( HRPADBEntities db = new HRPADBEntities( ) )
            {
                var tot1C = db.T_DetaxNew.Count( w => w.Anno_T_DetaxNew == DateTime.Now.Year && w.CodiceDetassazione_T_DetaxNew.Equals( "DETAX" ) && w.ModelloAssegnato_T_DetaxNew == "1C" );

                var tot2C = db.T_DetaxNew.Count( w => w.Anno_T_DetaxNew == DateTime.Now.Year && w.CodiceDetassazione_T_DetaxNew.Equals( "DETAX" ) && w.ModelloAssegnato_T_DetaxNew == "2C" );

                var compl1C = db.T_DetaxNew.Count( w => w.Anno_T_DetaxNew == DateTime.Now.Year && w.CodiceDetassazione_T_DetaxNew.Equals( "DETAX" ) && w.ModelloAssegnato_T_DetaxNew == "1C" && w.Modello_T_DetaxNew != null && w.Modello_T_DetaxNew.Equals( "1C" ) );

                var compl2C = db.T_DetaxNew.Count( w => w.Anno_T_DetaxNew == DateTime.Now.Year && w.CodiceDetassazione_T_DetaxNew.Equals( "DETAX" ) && w.ModelloAssegnato_T_DetaxNew == "2C" && w.Modello_T_DetaxNew != null && w.Modello_T_DetaxNew.Equals( "2C" ) );

                model.Tot1C = tot1C;
                model.Tot2C = tot2C;
                model.TotCompilato1C = compl1C;
                model.TotCompilato2C = compl2C;

                model.Percentuale1C = ( double ) compl1C / ( double ) tot1C * 100;
                model.Percentuale2C = ( double ) compl2C / ( double ) tot2C * 100;
            }

            return View( "~/Views/GestioneDetassazione/subpartial/_reportDetassazione.cshtml" , model );
        }

        public ActionResult GetPDFViewer ( int anno , string matricola , string codiceDetassazione = "DETAX" )
        {
            DetassazioneModel model = new DetassazioneModel( );

            try
            {
                using ( HRPADBEntities db = new HRPADBEntities( ) )
                {
                    var item = db.T_DetaxNew.Where( w => w.Anno_T_DetaxNew.Equals( anno ) && w.Matricola_T_DetaxNew.Equals( matricola ) && w.CodiceDetassazione_T_DetaxNew.Equals( codiceDetassazione ) ).FirstOrDefault( );

                    if ( item != null )
                    {
                        model.Matricola = matricola;
                        model.Nominativo = item.Nominativo_T_DetaxNew;
                        model.Codice = item.Modello_T_DetaxNew;
                        model.CodiceDetassazione = codiceDetassazione;
                        model.CodiceFiscale = string.Empty;
                        model.DataDiNascita = DateTime.MinValue;
                        model.LuogoDiNascita = string.Empty;
                        model.Sesso = string.Empty;
                        model.Anno = anno;
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }

            return View( "~/Views/GestioneDetassazione/subpartial/_visualizzaPDF.cshtml" , model );
        }

        public ActionResult GetPDF ( string matricola , int anno , string codiceDetassazione )
        {
            byte[] result = null;
            try
            {
                using ( HRPADBEntities db = new HRPADBEntities( ) )
                {
                    var item = db.T_DetaxNew.Where( w => w.Anno_T_DetaxNew.Equals( anno ) && w.Matricola_T_DetaxNew.Equals( matricola ) && w.CodiceDetassazione_T_DetaxNew.Equals( codiceDetassazione ) ).FirstOrDefault( );

                    if ( item != null )
                    {
                        result = item.PDF_T_DetaxNew;
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }

            if ( result == null )
            {
                return null;
            }
            return new FileContentResult( result , "application/pdf" );
        }

        public ActionResult ResetData( int anno , string matricola , DateTime dataOperazione, string codiceDetassazione = "DETAX")
        {
            string result = "";
            bool canContinue = false;
            try
            {
                // verifica se il dato è vecchio
                // se il dato non ha subito variazioni
                // effettua il reset
                T_DetaxNew record = null;
                using ( HRPADBEntities db = new HRPADBEntities( ) )
                {
                    record = db.T_DetaxNew.Where( w => w.Matricola_T_DetaxNew.Equals( matricola ) && w.Anno_T_DetaxNew.Equals( anno ) && w.CodiceDetassazione_T_DetaxNew.Equals( codiceDetassazione ) ).FirstOrDefault( );

                    if (record != null)
                    {
                        // verifica se è stata modificata la data operazione
                        if ( record.Data_T_DetaxNew.HasValue &&
                            record.Data_T_DetaxNew.Equals(dataOperazione))
                        {
                            result = "OK";
                            canContinue = true;
                        }
                        else
                        {
                            result = "OLDDATA";
                        }
                    }
                }

                if( canContinue )
                {
                    // rimuove dal db
                    using ( HRPADBEntities db = new HRPADBEntities( ) )
                    {
                        var item = db.T_DetaxNew.Where( w => w.Matricola_T_DetaxNew.Equals( matricola ) && w.Anno_T_DetaxNew.Equals( anno ) && w.CodiceDetassazione_T_DetaxNew.Equals( codiceDetassazione ) ).FirstOrDefault( );

                        if ( item != null )
                        {
                            item.Data_T_DetaxNew = null;
                            item.Modello_T_DetaxNew = null;
                            item.Scelta_T_DetaxNew = null;
                            item.PDF_T_DetaxNew = null;
                            db.SaveChanges( );
                        }
                    }

                    // chiamata CICS
                    MyRaiService1Client servizioWCF = new MyRaiService1Client( );
                    var risposta = servizioWCF.ResetModuloDetassazione( "P" + matricola , matricola , record.Applicazione_T_DetaxNew );

                    if (!risposta.Esito)
                    {
                        // ripristina i dati
                        using ( HRPADBEntities db = new HRPADBEntities( ) )
                        {
                            var item = db.T_DetaxNew.Where( w => w.Matricola_T_DetaxNew.Equals( matricola ) && w.Anno_T_DetaxNew.Equals( anno ) && w.CodiceDetassazione_T_DetaxNew.Equals( codiceDetassazione ) ).FirstOrDefault( );

                            if ( item != null )
                            {
                                item.Data_T_DetaxNew = record.Data_T_DetaxNew;
                                item.Modello_T_DetaxNew = record.Modello_T_DetaxNew;
                                item.Scelta_T_DetaxNew = record.Scelta_T_DetaxNew;
                                item.PDF_T_DetaxNew = record.PDF_T_DetaxNew;
                                db.SaveChanges( );
                            }
                        }

                        throw new Exception( risposta.Errore );
                    }
                }

            }
            catch(Exception ex)
            {
                result = ex.Message;
            }

            return Content( result );
        }
        #endregion

        #region Private Gestionale

        private bool AbilitatoReset()
        {
            bool result = false;
            try
            {
                string abilitati = CommonHelper.GetParametro<string>( EnumParametriSistema.DetaxAbilitatiRipristinoDati );

                List<string> ab = new List<string>( );
                ab = abilitati.Split( ',' ).ToList( );

                if (ab != null && ab.Any())
                {
                    return ab.Contains( CommonHelper.GetCurrentUserMatricola( ) );
                }
            }
            catch(Exception ex)
            {
                result = false;
            }
            return result;
        }

        private void InitializeViewBagModelli ( int? selectedItem = null )
        {
            var modelli = new List<SelectListItem>( );

            modelli.Add( new SelectListItem( )
            {
                Selected = false ,
                Text = "Modello 1C" ,
                Value = "1"
            } );

            modelli.Add( new SelectListItem( )
            {
                Selected = false,
                Text = "Modello 2C" ,
                Value = "2"
            } );

            ViewBag.Modello = new SelectList( modelli , "Value" , "Text" , ( selectedItem ?? -1 ) );
        }

        private void InitializeViewBagStati ( int? selectedItem = null )
        {
            var stati = new List<SelectListItem>( );

            stati.Add( new SelectListItem( )
            {
                Selected = false ,
                Text = "Compilato" ,
                Value = "1"
            } );

            stati.Add( new SelectListItem( )
            {
                Selected = false ,
                Text = "Non compilato" ,
                Value = "0"
            } );

            ViewBag.Stato = new SelectList( stati , "Value" , "Text" , ( selectedItem ?? -1 ) );
        }

        private List<DetassazioneUser> GetListaDipendenti ( int limit = 50, string matricola = null , string nominativo = "" , int? modello = null , int? stato = null )
        {
            List<DetassazioneUser> risultato = new List<DetassazioneUser>( );
            
            try
            {
                risultato = this.GetListaDipendentiDB( limit , matricola , nominativo , modello, stato );
            }
            catch ( Exception ex )
            {
                risultato = new List<DetassazioneUser>( );
            }

            return risultato;
        }

        private List<DetassazioneUser> GetListaDipendentiDB ( int limit = 50 , string matricola = null , string nominativo = "" , int? modello = null , int? stato = null )
        {
            List<DetassazioneUser> risultato = new List<DetassazioneUser>( );

            try
            {
                List<T_DetaxNew> list = new List<T_DetaxNew>( );
                using ( HRPADBEntities db = new HRPADBEntities( ) )
                {
                    var detax = Enumerable.Empty<T_DetaxNew>( ).AsQueryable( );

                    detax = db.T_DetaxNew.Where( w => w.Anno_T_DetaxNew.Equals( DateTime.Now.Year ) );

                    if (!String.IsNullOrEmpty(matricola))
                    {
                        detax = detax.Where( w => w.Matricola_T_DetaxNew.Equals( matricola ) );
                    }

                    if ( modello.HasValue )
                    {
                        string mod = String.Format( "{0}C" , modello.GetValueOrDefault( ) );
                        detax = detax.Where( w => w.ModelloAssegnato_T_DetaxNew.Equals( mod ) );
                    }

                    if ( !String.IsNullOrEmpty( nominativo ) )
                    {
                        detax = detax.Where( w => w.Nominativo_T_DetaxNew.Contains( nominativo ) );
                    }

                    if (stato.HasValue)
                    {
                        if ( stato.GetValueOrDefault( ).Equals( 0 ) )
                        {
                            detax = detax.Where( w => w.Modello_T_DetaxNew == null );
                        }
                        if ( stato.GetValueOrDefault( ).Equals( 1 ) )
                        {
                            detax = detax.Where( w => w.Modello_T_DetaxNew != null );

                            if ( modello.HasValue )
                            {
                                string mod = String.Format( "{0}C" , modello.GetValueOrDefault( ) );
                                detax = detax.Where( w => w.Modello_T_DetaxNew.Equals( mod ) );
                            }
                        }
                    }

                    try
                    {
                        list = detax.OrderByDescending( w => w.Data_T_DetaxNew ).Take( limit ).ToList( );
                    }
                    catch ( Exception ex )
                    {
                        list = detax.OrderByDescending( w => w.Data_T_DetaxNew ).ToList( );
                    }
                }

                if (list != null && list.Any())
                {
                    foreach ( var item in list )
                    {
                        risultato.Add( new DetassazioneUser( )
                        {
                            Matricola = item.Matricola_T_DetaxNew ,
                            Completato = !String.IsNullOrEmpty( item.Modello_T_DetaxNew ) ,
                            DataCompletamento = item.Data_T_DetaxNew ,
                            Nominativo = item.Nominativo_T_DetaxNew ,
                            TipoModello = !String.IsNullOrEmpty( item.Modello_T_DetaxNew ) ? item.Modello_T_DetaxNew : item.ModelloAssegnato_T_DetaxNew
                        } );
                    }
                }
            }
            catch ( Exception ex )
            {
                risultato = new List<DetassazioneUser>( );
            }

            return risultato;
        }

        [Obsolete("Adesso i dati sono tutti sul db, non c'è bisogno della chiamata a CICS")]
        private List<DetassazioneUser> GetListaDipendentiCICS ( int limit = 20 , string matricola = null , string nominativo = "" , int? modello = null , int? stato = null )
        {
            List<DetassazioneUser> risultato = new List<DetassazioneUser>( );

            try
            {
                List<T_DetaxNew> list = new List<T_DetaxNew>( );
                using ( HRPADBEntities db = new HRPADBEntities( ) )
                {
                    list = db.T_DetaxNew.Where( w => w.Anno_T_DetaxNew.Equals( DateTime.Now.Year ) ).OrderByDescending( w => w.Data_T_DetaxNew ).Take( limit ).ToList( );
                }

                foreach ( var item in list )
                {
                    risultato.Add( new DetassazioneUser( )
                    {
                        Matricola = item.Matricola_T_DetaxNew ,
                        Completato = true ,
                        DataCompletamento = item.Data_T_DetaxNew ,
                        Nominativo = BatchManager.GetUserData( item.Matricola_T_DetaxNew ).nominativo ,
                        TipoModello = item.Modello_T_DetaxNew
                    } );
                }
            }
            catch ( Exception ex )
            {
                risultato = new List<DetassazioneUser>( );
            }

            return risultato;
        }

        private DetassazioneListaDipendentiVM ListaDipendentiInternal( int pagina = 1 , string matricola = null , string nominativo = "" , int? modello = null , int? stato = null )
        {
            this.InitializeViewBagModelli( modello );
            this.InitializeViewBagStati( stato );

            DetassazioneListaDipendentiVM model = new DetassazioneListaDipendentiVM( );
            List<DetassazioneUser> dati = new List<DetassazioneUser>( );

            int[] param = CommonHelper.GetParametri<int>( EnumParametriSistema.DetaxLimit );

            model.Paginatore = new DetassazionePaginatore( );
            model.Paginatore.ElementiPerPagina = 20;
            if ( param != null )
            {
                model.Paginatore.ElementiPerPagina = param[0];
            }

            int limit = 1000;

            if ( param != null )
            {
                limit = param[1];
            }

            if (pagina == 1)
            {
                dati = GetListaDipendenti( limit , matricola , nominativo , modello , stato );
                model.Dipendenti = dati.Take( model.Paginatore.ElementiPerPagina ).ToList( );
                //SessionManager.Set( SessionVariables.DetassazioneListaDipendenti , dati );
            }
            else
            {
                //dati = ( List<DetassazioneUser> ) SessionManager.Get( SessionVariables.DetassazioneListaDipendenti );
                dati = GetListaDipendenti( limit , matricola , nominativo , modello , stato );
                try
                {
                    model.Dipendenti = dati.Skip( ( pagina - 1 ) * model.Paginatore.ElementiPerPagina ).Take( model.Paginatore.ElementiPerPagina ).ToList( );
                }
                catch ( Exception ex )
                {
                    model.Dipendenti = dati.Skip( ( pagina - 1 ) * model.Paginatore.ElementiPerPagina ).ToList( );
                }
            }

            model.Paginatore.PaginaCorrente = pagina;
            model.Badge = dati.Count;
            model.Filtri = new DetassazioneRicercaModel( )
            {
                Matricola = matricola ,
                Nominativo = nominativo ,
                Modello = modello ,
                Stato = stato,
                Pagina = pagina
            };

            if ( ( dati.Count % model.Paginatore.ElementiPerPagina ) == 0 )
            {
                model.Paginatore.Pagine = ( dati.Count / model.Paginatore.ElementiPerPagina );
            }
            else
            {
                model.Paginatore.Pagine = ( dati.Count / model.Paginatore.ElementiPerPagina ) + 1;
            }
            Pager p = new Pager( model.Badge , pagina , model.Paginatore.ElementiPerPagina );

            model.Paginatore.PaginaMin = p.StartPage;
            model.Paginatore.PaginaMax = p.EndPage;

            //model.Paginatore.PaginaMin = pager.StartIndex;
            //model.Paginatore.PaginaMax = model.Paginatore.Pagine <= 5 ? model.Paginatore.Pagine : model.Paginatore.PaginaMin + 5;

            return model;
        }

        #endregion
    }
}