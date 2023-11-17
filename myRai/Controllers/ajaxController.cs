using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using myRai.DataAccess;
using myRaiData;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System.Globalization;
using System.Data.Objects;
using TimbratureCore;
using myRaiCommonDatacontrollers.DataControllers;
using myRaiCommonManager;
using myRaiHelper;
using myRaiHelper;
using myRaiCommonModel;
using myRaiCommonModel.ess;
using ServiceWrapper = myRaiHelper.ServiceWrapper;
using myRaiCommonModel.cvModels;
using myRai.Business;
using System.Reflection;
using CommonManager = myRaiHelper.CommonHelper;
using Utente = myRaiHelper.UtenteHelper;
using EccezioniManager = myRaiCommonManager.EccezioniManager;

namespace myRai.Controllers
{
    public partial class CV_OnlineController : BaseCommonController
    {
        public string a = "";
    }

    public class EccezioneCopertura
    {
        public string nome { get; set; }
        public string durata { get; set; }
        public string intv { get; set; }
        public string nota { get; set; }
        public string eccezione_collegata { get; set; }
        public string parametro_passato { get; set; }
        public string parametro_ricevuto { get; set; }
        public string valore_parametro { get; set; }
    }

    public class ajaxController : BaseCommonController
    {
        private NoteRichiesteDataController _noteRichiesteDataController { get; set; }

        public LinkedTableDataController linkedTableDataController = new LinkedTableDataController( );

        bool a = true;
        public ajaxController ( )
        {
            SessionManager.Set(SessionVariables.AnnoFeriePermessi, null);
            this._noteRichiesteDataController = new NoteRichiesteDataController( );
        }
        public ActionResult azzeraMP ( int minfine , string data )
        {

            WSDigigapp service = new WSDigigapp( )
            {
                Credentials = new System.Net.NetworkCredential(
                  CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] ,
                  CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] )
            };

            dayResponse resp = service.getEccezioni( CommonManager.GetCurrentUserMatricola( ) , data.Replace( "/" , "" ) , "BU" , 80 );
            if ( resp.eccezioni != null && resp.eccezioni.Any( ) )
            {
                foreach ( var e in resp.eccezioni )
                {
                    if ( DayAnalysisFactory.DayAnalysisExists( e.cod.Trim( ) ) )
                    {
                        var DA = DayAnalysisFactory.GetDayAnalysisClass( e.cod.Trim( ) , resp , Utente.GetQuadratura( ) == Quadratura.Giornaliera , null , true );//smap 1 min
                        var Q = DA.GetEccezioneQuantita( );
                        int minGapp = e.qta.ToMinutes( );
                        int minSmap1min = Q.QuantitaMinuti;
                        if ( minGapp != minSmap1min )
                        {
                            return new JsonResult
                            {
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                                Data = new { result = "Questo inserimento avrebbe conseguenze sulla eccezione " + e.cod + ", già richiesta, la cui quantità non sarebbe piu corretta. Eseguire questa operazione prima di inserire maggiorazioni." }
                            };
                        }
                    }
                }
            }


            return Inserimento( new InserimentoEccezioneModel( )
            {
                dalle = minfine.ToHHMM( ) ,
                alle = ( minfine + 1 ).ToHHMM( ) ,
                cod_eccezione = "SMAP" ,
                data_da = data ,
                data_a = data ,
                nota_richiesta = "Cancellazione MP"
            } );
        }
        public ActionResult RifiutaEccezione(int id, string nota, bool visto = false)
        {
            return Valida_Rifiuta(id, false, nota, visto);
        }

        public ActionResult GetMinutiFineTurno ( string dataOggi )
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );
            WSDigigappDataController service = new WSDigigappDataController( );
            DateTime data;
            DateTime.TryParseExact( dataOggi , "dd/MM/yyyy" , null , DateTimeStyles.None , out data );

            data = data.AddDays( -1 );
            var resp = service.GetEccezioni(matricola, matricola, data.ToString("ddMMyyyy"), "BU", 70);
            if ( resp.orario.cod_orario.StartsWith( "9" ) )
            {
                data = data.AddDays( -1 );
                resp = service.GetEccezioni(matricola, matricola, data.ToString("ddMMyyyy"), "BU", 70);
                if ( resp.orario.cod_orario.StartsWith( "9" ) )
                {
                    return Content( "-1" );
                }
            }
            return Content( resp.orario.uscita_iniziale );


        }

        public ActionResult ValidaEccezione(int id, string nota, bool visto = false)
        {
            //return null;

            return Valida_Rifiuta(id, true, nota, visto);
        }
        public ActionResult getnome ( string matricola )
        {
            string n = CommonManager.GetNominativoPerMatricola( matricola.Trim( 'P' ) );
            return Content( n );
        }
        public ActionResult img ( string matr , string ck )
        {

            WebClient w = new WebClient( );
            //w.Credentials = new System.Net.NetworkCredential(
            //    CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] ,
            //    CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] );
            w.Credentials = CommonManager.GetUtenteServizioCredentials();

            byte[] b = w.DownloadData("http://hrapp.servizi.rai.it/viewFace/view.aspx?m=#MATR&w=200&hr=200&sqr=1&ck=#CK"
                .Replace( "#MATR" , matr )
                .Replace( "#CK" , ck )
                );
            return File( b , "image/png" );

        }

        public ActionResult getInfoRichiesta ( int IdRichiestaPadre )
        {
            var db = new myRaiData.digiGappEntities( );
            try
            {
                var rich = db.MyRai_Richieste.Where( x => x.id_richiesta == IdRichiestaPadre ).FirstOrDefault( );
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new
                    {
                        result = "OK" ,
                        dal = rich.periodo_dal.ToString( "ddMMyyyy" ) ,
                        al = rich.periodo_al.ToString( "ddMMyyyy" )
                    }
                };
            }
            catch ( Exception ex )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new
                    {
                        result = ex.Message ,
                    }
                };
            }
        }
        public ActionResult getRichiestaMultipla ( int IdRichiestapadre , int IDEccRich )
        {

            var db = new myRaiData.digiGappEntities( );
            var rich = db.MyRai_Richieste.Where( x => x.id_richiesta == IdRichiestapadre ).FirstOrDefault( );
            if ( rich != null )
            {
                if ( rich.periodo_dal == rich.periodo_al )
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                        Data = new { result = "OK" , newID = rich.id_richiesta , giorni = 1 }
                    };
                }
                else
                {
                    List<MyRai_Richieste> NuoveRich = EccezioniManager.RendiSingoliGiorni( IdRichiestapadre );

                    if ( NuoveRich != null )
                    {
                        if ( IDEccRich == -1 )
                        {
                            return new JsonResult
                            {
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                                Data = new { result = "OK" }
                            };
                        }
                        foreach ( var r in NuoveRich )
                        {
                            if ( r.MyRai_Eccezioni_Richieste.Any( x => x.id_eccezioni_richieste == IDEccRich ) )
                                return new JsonResult
                                {
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                                    Data = new { result = "OK" , newID = r.id_richiesta , giorni = NuoveRich.Count( ) , dal = rich.periodo_dal.ToString( "ddMMyyyy" ) , al = rich.periodo_al.ToString( "ddMMyyyy" ) }
                                };
                        }
                    }
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = "Richiesta non trovata" }
            };
        }



        private ActionResult Valida_Rifiuta(int IdEccezioneRichiesta, bool valida, string NotaRifiuto_o_Approvazione = null, bool Visto = false)
        {

            string Errore = null;
            if (Visto == false)
                Errore = EccezioniManager.Valida_Rifiuta(IdEccezioneRichiesta, valida, NotaRifiuto_o_Approvazione);
            else
            {
                var db = new digiGappEntities();
                MyRai_Eccezioni_Richieste eccez = db.MyRai_Eccezioni_Richieste.Where(x => x.id_eccezioni_richieste == IdEccezioneRichiesta).FirstOrDefault();
                if (eccez == null)
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Errore DB, impossibile recuperare dati della richiesta" }
                    };
                if (eccez.data_visto_rifiutato != null || eccez.data_visto_validato != null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Visto già presente" }
                    };
                }

                if (eccez.id_stato == 10)
                {
                    Errore = EccezioniManager.Valida_Rifiuta(IdEccezioneRichiesta, valida, NotaRifiuto_o_Approvazione);
                    if (Errore == null)
                        EccezioniManager.AggiornaDatiVisto(IdEccezioneRichiesta, valida, NotaRifiuto_o_Approvazione);
                }
                else if (eccez.id_stato == 20)
                {
                    if (valida)
                        EccezioniManager.AggiornaDatiVisto(IdEccezioneRichiesta, valida, NotaRifiuto_o_Approvazione);
                    else
                    {
                        Errore = EccezioniManager.Valida_Rifiuta(IdEccezioneRichiesta, valida, NotaRifiuto_o_Approvazione);
                        if (Errore == null)
                            EccezioniManager.AggiornaDatiVisto(IdEccezioneRichiesta, valida, NotaRifiuto_o_Approvazione);
                    }
                }
                else
        {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Stato richiesta non ammesso" }
                    };
                }
            }


            //string Errore = null; System.Threading.Thread.Sleep(1000);
            //if (IdEccezioneRichiesta.ToString().Contains("140")) Errore = "Errore simulato server !";
            if ( Errore != null )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = Errore }
                };
            }
            else
            {
                // Modificata in data 10/08/2018
                // Prima le notifiche relative ad una richiesta approvata o rifiutata, venivano
                // marcate come lette. Adesso vengono rimosse definitivamente

                //this.NotificheSetDataLettura( IdEccezioneRichiesta );
                this.RemoveNotifiche( IdEccezioneRichiesta );

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "OK" }
                };
            }
        }

        /// <summary>
        /// Imposta come lette le notifiche associate ad una richiesta eccezione
        /// </summary>
        /// <param name="idEccezioneRichiesta"></param>
        private void NotificheSetDataLettura ( int idEccezioneRichiesta )
        {
            try
            {
                using ( var db = new myRaiData.digiGappEntities( ) )
                {
                    var eccezione = db.MyRai_Eccezioni_Richieste.Where( x => x.id_eccezioni_richieste.Equals( idEccezioneRichiesta ) ).FirstOrDefault( );

                    if ( eccezione != null )
                    {
                        int idRichiesta = eccezione.id_richiesta;
                        string categoriaInsRichiesta = EnumCategoriaNotifica.InsRichiesta.GetDescription( );
                        string MarcaturaScaduta = EnumCategoriaNotifica.MarcaturaScaduta.GetDescription( );
                        string MarcaturaUrgente = EnumCategoriaNotifica.MarcaturaUrgente.GetDescription( );

                        List<MyRai_Notifiche> notifiche = db.MyRai_Notifiche.Where( n => n.id_riferimento.Equals( idRichiesta ) && ( n.categoria.Equals( categoriaInsRichiesta ) || n.categoria.Equals( MarcaturaScaduta ) || n.categoria.Equals( MarcaturaUrgente ) ) ).ToList( );

                        if ( notifiche != null && notifiche.Any( ) )
                        {
                            notifiche.ForEach( n =>
                             {
                                 n.data_letta = DateTime.Now;
                             } );

                            db.SaveChanges( );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
            }
        }


        private void RemoveNotifiche ( int idEccezioneRichiesta )
        {
            try
            {
                using ( var db = new myRaiData.digiGappEntities( ) )
                {
                    var eccezione = db.MyRai_Eccezioni_Richieste.Where( x => x.id_eccezioni_richieste.Equals( idEccezioneRichiesta ) ).FirstOrDefault( );

                    if ( eccezione != null )
                    {
                        int idRichiesta = eccezione.id_richiesta;
                        string categoriaInsRichiesta = EnumCategoriaNotifica.InsRichiesta.GetDescription( );
                        string MarcaturaScaduta = EnumCategoriaNotifica.MarcaturaScaduta.GetDescription( );
                        string MarcaturaUrgente = EnumCategoriaNotifica.MarcaturaUrgente.GetDescription( );

                        List<MyRai_Notifiche> notifiche = db.MyRai_Notifiche.Where( n => n.id_riferimento.Equals( idRichiesta ) && ( n.categoria.Equals( categoriaInsRichiesta ) || n.categoria.Equals( MarcaturaScaduta ) || n.categoria.Equals( MarcaturaUrgente ) ) ).ToList( );

                        if ( notifiche != null && notifiche.Any( ) )
                        {
                            notifiche.ForEach( n =>
                             {
                                 db.MyRai_Notifiche.Remove( n );
                             } );

                            db.SaveChanges( );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
            }
        }

        public ActionResult DettaglioDaApprovare ( )
        {
            return View( "~/Views/richieste/_dettaglioDaApprovare.cshtml" );
        }

        public ActionResult PrendiVisione ( int IdPolicy )
        {
            var db = new digiGappEntities( );
            B2RaiPlace_RaiPlacePolicyUtenti utente = new B2RaiPlace_RaiPlacePolicyUtenti( );
            utente.DataConferma = DateTime.Now;
            utente.Matrciola = CommonManager.GetCurrentUserMatricola( );
            utente.Fk_Id_RaiPlacePolicy = IdPolicy;
            utente.nome = myRai.Models.Utente.EsponiAnagrafica()._nome;
            utente.cognome = myRai.Models.Utente.EsponiAnagrafica()._cognome;
            utente.societa = myRai.Models.Utente.EsponiAnagrafica()._logo;
            if ( System.Web.HttpContext.Current.Request.UserAgent.Length > 300 )
            {
                utente.user_agent = System.Web.HttpContext.Current.Request.UserAgent.Substring( 0 , 299 );
            }
            else
            {
                utente.user_agent = System.Web.HttpContext.Current.Request.UserAgent;
            }

            utente.ip = System.Web.HttpContext.Current.Request.UserHostAddress;

            db.B2RaiPlace_RaiPlacePolicyUtenti.Add( utente );

            if ( !DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) ) )
            {

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Errore aggiornamento DB" }
                };
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = "OK" }
            };
        }
        public ActionResult CancellaRichiestaEccezioneFittizia ( int IdRichiestaPadre , string NotaCancellazione )
        {
            var db = new digiGappEntities( );
            var richPadre = db.MyRai_Richieste.Where( x => x.id_richiesta == IdRichiestaPadre ).FirstOrDefault( );
            foreach ( var child in richPadre.MyRai_Eccezioni_Richieste.ToList( ) )
            {
                child.id_stato = ( int ) EnumStatiRichiesta.Cancellata;
            }
            richPadre.id_stato = ( int ) EnumStatiRichiesta.Cancellata;
            db.SaveChanges( );
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = "OK" }
            };
        }
        public Boolean IsDataUtilizzabileRipianificazione ( DateTime DataRip )
        {
            var resp = EccezioniManager.GetGiornata( DataRip.ToString( "ddMMyyyy" ) , CommonManager.GetCurrentUserMatricola( ) );

            if ( resp.eccezioni == null || !resp.eccezioni.Any( ) )
                return true;
            else
                return !( resp.eccezioni.Any( x => x.cod.StartsWith( "FE" ) ) ||
                           resp.eccezioni.Any( x => x.cod.StartsWith( "PR" ) ) ||
                           resp.eccezioni.Any( x => x.cod.StartsWith( "PF" ) ) ||
                           resp.eccezioni.Any( x => x.cod.StartsWith( "PG" ) ) ||
                           resp.eccezioni.Any( x => x.cod.StartsWith( "PX" ) ) );
        }
        public ActionResult CancellaRichiesta(int IdRichiestaPadre, string NotaCancellazione, string DataRipian, string eccRimpiazzo = null, string dalle = null, bool swh = false)
        {
            var db = new digiGappEntities( );
            DateTime DataRipianificazione;
            Boolean IsRipianificazione = DateTime.TryParseExact( DataRipian , "dd/MM/yyyy" , null , DateTimeStyles.None , out DataRipianificazione );

            if ( IsRipianificazione && !IsDataUtilizzabileRipianificazione( DataRipianificazione ) )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "La giornata del " + DataRipian + " ha eccezioni incompatibili con la ripianificazione" }
                };
            }

            if ( IsRipianificazione )
                NotaCancellazione += " (Ripianificazione su " + DataRipianificazione.ToString( "dd/MM/yyyy" ) + ")";
            MyRai_Eccezioni_Richieste EccRichAutoStorno = null;

            Boolean RichiestoInserimentoNotificaStorno = false;

            var richPadre = db.MyRai_Richieste.Where( x => x.id_richiesta == IdRichiestaPadre ).FirstOrDefault( );
            if ( richPadre == null )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Richiesta non individuata nel DB" }
                };
            }

            if ( EccezioniManager.IsEccezioneFittiziaNoCorrispondenzaGapp( richPadre.MyRai_Eccezioni_Richieste.Select( x => x.cod_eccezione ).FirstOrDefault( ) ) )
            {
                return CancellaRichiestaEccezioneFittizia( IdRichiestaPadre , NotaCancellazione );
            }


            //se lo stato era inSegreteria, metti in stato cancellata
            if ( richPadre.id_stato == ( int ) EnumStatiRichiesta.InseritoSegreteria )
            {
                richPadre.id_stato = ( int ) EnumStatiRichiesta.Cancellata;
                foreach ( var ec in richPadre.MyRai_Eccezioni_Richieste )
                    ec.id_stato = ( int ) EnumStatiRichiesta.Cancellata;

                if ( !DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) ) )
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                        Data = new { result = "Errore aggiornamento DB" }
                    };
                }

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "OK" }
                };
            }
            List<MyRai_Eccezioni_Richieste> LM = new List<MyRai_Eccezioni_Richieste>( );
            //prende richiesta figlia piu recente :
            var lastRichiestaFiglia = richPadre.MyRai_Eccezioni_Richieste.OrderByDescending( x => x.data_creazione ).FirstOrDefault( );
            if ( lastRichiestaFiglia == null )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Errore recupero dati figlia DB" }
                };
            }

            Boolean IsInserimento = ( lastRichiestaFiglia.azione == "I" );
            Boolean IsStorno = ( lastRichiestaFiglia.azione == "C" );
            DateTime dataDa = lastRichiestaFiglia.data_eccezione;
            string matricola = richPadre.matricola_richiesta;

            MyRai_LogAzioni azioneDB = new MyRai_LogAzioni( )
            {
                provenienza = "ajaxcontroller/CancellaRichiesta" ,
                operazione = "Cancellazione IdRichiesta:" + IdRichiestaPadre
            };

            WSDigigappDataController service = new WSDigigappDataController( );
            service.ClearAll( );

            //CANCELLAZIONE RICHIESTA IN APPROVAZIONE                                                          
            if ( richPadre.id_stato == ( int ) EnumStatiRichiesta.InApprovazione && IsInserimento )
            {
                var richFiglia = richPadre.MyRai_Eccezioni_Richieste
                                           .Where( x => x.id_stato == lastRichiestaFiglia.id_stato ).FirstOrDefault( );


                //cancella richiesta esistente tramite storno
                string ErroreRifiuta = EccezioniManager.Valida_Rifiuta( richFiglia.id_eccezioni_richieste ,
                    false ,  //valida
                    null ,   //nota rifiuto o approvazione
                    false ,  //aggiorna stato db
                    false , //non inviare email
                    true ); //CancellazioneRichiesta
                if ( ErroreRifiuta != null )
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                        Data = new { result = ErroreRifiuta }
                    };
                }
                //cambia stato su DB -> cancellata
                foreach ( var rfiglia in richPadre.MyRai_Eccezioni_Richieste )
                    rfiglia.id_stato = ( int ) EnumStatiRichiesta.Cancellata;

                //cambia stato su DB -> cancellata
                richPadre.id_stato = ( int ) EnumStatiRichiesta.Cancellata;
                RichiestoInserimentoNotificaStorno = true;

                //elimina row CoperturaCarenze if any
                var cop = db.MyRai_CoperturaCarenze.Where( x => x.id_richiesta == richPadre.id_richiesta ).FirstOrDefault( );
                if ( cop != null )
                {
                    db.MyRai_CoperturaCarenze.Remove( cop );
                }
            }
            //CANCELLAZIONE STORNO IN APPROVAZIONE                                                             
            else if ( richPadre.id_stato == ( int ) EnumStatiRichiesta.InApprovazione && IsStorno )
            {
                //todo cics : -
                foreach ( var richFiglia in richPadre.MyRai_Eccezioni_Richieste
                                            .Where( x => x.id_stato == lastRichiestaFiglia.id_stato ) )
                {
                    LM.Add( richFiglia );
                }
                LM.ForEach( e => db.MyRai_Eccezioni_Richieste.Remove( e ) );
                richPadre.id_stato = ( int ) EnumStatiRichiesta.Approvata;


            }
            //CANCELLAZIONE RICHIESTA APPROVATA                                                              
            else if ( richPadre.id_stato == ( int ) EnumStatiRichiesta.Approvata && IsInserimento )
            {
                foreach ( var richFiglia in richPadre.MyRai_Eccezioni_Richieste
                                          .Where( x => x.id_stato == lastRichiestaFiglia.id_stato ) )
                {
                    MyRai_Eccezioni_Richieste EccNew = CommonManager.CopyFrom( richFiglia );
                    EccNew.azione = "C";
                    EccNew.id_stato = ( int ) EnumStatiRichiesta.InApprovazione;
                    EccNew.data_creazione = DateTime.Now;
                    EccNew.motivo_richiesta = NotaCancellazione;
                    EccNew.numero_documento_riferimento = richFiglia.numero_documento;
                    EccNew.matricola_primo_livello = null;
                    EccNew.data_validazione_primo_livello = null;
                    EccNew.nominativo_primo_livello = null;
                    EccNew.numero_documento = null;

                    if ( IsRipianificazione )
                    {
                        EccNew.DataRipianificazione = DataRipianificazione;
                        if (!String.IsNullOrWhiteSpace(eccRimpiazzo))
                        {
                            EccNew.eccezione_rimpiazzo_storno = eccRimpiazzo;
                        }

                    }
                    else if (!String.IsNullOrWhiteSpace(eccRimpiazzo))
                    {
                        EccNew.eccezione_rimpiazzo_storno = eccRimpiazzo; //da storno sw
                        if (EccezioniManager.IsEccezioneAQuarti(eccRimpiazzo))
                        {
                            int minutiQ = EccezioniManager.GetQuartoDiGiornataInMinuti(EccNew.data_eccezione.ToString("dd/MM/yyyy"));

                            EccNew.eccezione_rimpiazzo_dalle = EccNew.data_eccezione.AddMinutes(dalle.ToMinutes());
                            EccNew.eccezione_rimpiazzo_alle = ((DateTime)EccNew.eccezione_rimpiazzo_dalle).AddMinutes(minutiQ);
                            EccNew.eccezione_rimpiazzo_quantita = (decimal)0.25;
                            EccNew.eccezione_rimpiazzo_richiedeSWH = swh;
                        }
                        else if (EccezioniManager.IsEccezione_0_50(eccRimpiazzo))
                        {
                            EccNew.eccezione_rimpiazzo_quantita = (decimal)0.25;
                            EccNew.eccezione_rimpiazzo_richiedeSWH = swh;
                        }
                    }

                    LM.Add( EccNew );

                    if ( EccRichAutoStorno == null )
                    {
                        if ( !string.IsNullOrWhiteSpace( richFiglia.cod_eccezione ) &&
                            EccezioniManager.InserimentoEccezioneGiaApprovata( richFiglia.cod_eccezione ) )
                        {
                            EccRichAutoStorno = EccNew;
                        }
                    }
                    var RichSuApprovazioniMassive = db.MyRai_ApprovazioneMassiva
                        .Where(x => x.IdRichiesta == EccNew.id_richiesta).FirstOrDefault();
                    if (RichSuApprovazioniMassive != null)
                        RichSuApprovazioniMassive.Status = 5;

                }
                LM.ForEach( e => db.MyRai_Eccezioni_Richieste.Add( e ) );
                richPadre.id_stato = ( int ) EnumStatiRichiesta.InApprovazione;
                RichiestoInserimentoNotificaStorno = true;

            }
            //CANCELLAZIONE STORNO APPROVATO                                                                   
            else if ( richPadre.id_stato == ( int ) EnumStatiRichiesta.Approvata && IsStorno )
            {
                // non ammesso (gia intercettato dalla view)
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Cancellazione non ammessa" }
                };
            }


            if ( !DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) ) )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Errore aggiornamento DB" }
                };
            }

            RimuoviVisionato( matricola , dataDa );

            //se richiesto ins notifica:
            if ( RichiestoInserimentoNotificaStorno )
            {
                NotificheManager.NotificaPerInserimentoRichiesta_o_Storno( richPadre.id_richiesta , EnumCategoriaNotifica.InsStorno );
            }

            if ( EccRichAutoStorno != null )
            {
                string r = EccezioniManager.Valida_Rifiuta( ( int ) EccRichAutoStorno.id_eccezioni_richieste , true , NotaCancellazione );
                if ( !String.IsNullOrWhiteSpace( r ) )
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                        Data = new { result = r }
                    };
                }
            }

            if (richPadre.MyRai_Eccezioni_Richieste.Any(x => x.cod_eccezione.Trim() == "SW"))
            {
                EccezioniManager.AggiornaDatiDiSessione();
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = "OK" }
            };
        }


        /// <summary>
        /// Rimuove il visionato se presente, nella giornata passata
        /// </summary>
        /// <param name="matricolaUtente"></param>
        /// <param name="dataRichiesta"></param>
        public static void RimuoviVisionato ( string matricolaUtente , DateTime dataRichiesta )
        {
            var db = new digiGappEntities( );
            try
            {
                try
                {
                    DateTime start = new DateTime( dataRichiesta.Year , dataRichiesta.Month , dataRichiesta.Day , 0 , 0 , 0 );
                    DateTime stop = new DateTime( dataRichiesta.Year , dataRichiesta.Month , dataRichiesta.Day , 23 , 59 , 59 );

                    var records = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricolaUtente ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).ToList( );

                    if ( records != null && records.Any( ) )
                    {
                        records.ForEach( r =>
                         {
                             r.Visualizzato = false;
                         } );

                        DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) );
                    }
                }
                catch ( Exception ex )
                {
                }

            }
            catch ( Exception ex )
            {
                CommonManager.InviaMailDebug( String.Format( "Errore in RimuoviVisionato per l'utente: {0} nella data: {1}. \nErrore:{2}" , matricolaUtente , dataRichiesta.ToString( "dd/MM/yyyy" ) , ex.ToString( ) ) , null );

                Logger.LogErrori( new MyRai_LogErrori( )
                {
                    error_message = ex.ToString( ) ,
                    provenienza = "RimuoviVisionato"
                } );
            }
        }




        public ActionResult GetEccezioni(int idragg, string date, string RimuoviDaGiornata = null)
        {

            DateTime Date;
            Boolean datavalida = DateTime.TryParseExact( date , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out Date );


            var db = new digiGappEntities( );
            WSDigigapp service = new WSDigigapp( )
            {
                Credentials = new System.Net.NetworkCredential( CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] , CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] )
            };
            dayResponse respToCut = null;
            if (!String.IsNullOrWhiteSpace(RimuoviDaGiornata))
            {
                respToCut = service.getEccezioni(CommonManager.GetCurrentUserMatricola(), date.Replace("/", ""), "BU", 80);
                if (respToCut.eccezioni != null && respToCut.eccezioni.Any())
                {
                    Session["EccezioniPossibili"] = null;
                    string[] Elimina = RimuoviDaGiornata.Split(',');
                    respToCut.eccezioni = respToCut.eccezioni.Where(x => !Elimina.Contains(x.cod.Trim())).ToArray();
                }
            }
            var ListEccezioniAmmesse =
                EccezioniManager.GetListaEccezioniPossibili(CommonManager.GetCurrentUserMatricola7chars(), Date, respToCut);

            var query = ListEccezioniAmmesse.Where(x => idragg == 0 || x.id_raggruppamento == idragg).ToList();



            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client( );
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential( CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] , CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] );

            // MyRaiServiceInterface.MyRaiServiceReference1.GetFerieResponse ResponseWcf = cl.GetFerie(CommonManager.GetCurrentUserMatricola(), "");

            string dataPianoFerie = DateTime.Now.ToString( "ddMMyyyy" );
            if ( Date.Year != DateTime.Now.Year )
                dataPianoFerie = "0101" + Date.Year;

            pianoFerie resp = ServiceWrapper.GetPianoFerieWrapped(service, CommonManager.GetCurrentUserMatricola7chars(), dataPianoFerie, 70, Utente.TipoDipendente());

            //  Ferie ferieFromWcf_convertiteToAsmxResponse = EccezioniManager.ConvertiWcfToAsmxFerie(ResponseWcf);

            if ( Date < DateTime.Now )
            {
                TimeSpan ts = DateTime.Now - Date;
                query = query.Where( x => x.OreInPassato > ts.TotalHours ).ToList( );
            }
            else
            {
                TimeSpan ts = Date - DateTime.Now;
                query = query.Where( x => x.OreInFuturo > ts.TotalHours ).ToList( );
            }

            if ( Date > DateTime.Now && CommonManager.IsDateInControlloSNM_SMP( Date ) ) // 4 giorni famosi
            {
                var respge = service.getEccezioni( CommonManager.GetCurrentUserMatricola( ) , date.Replace( "/" , "" ) , "BU" , 80 );
                if ( !CommonManager.SNM_SNPpresent( respge ) )
                {
                    query.RemoveAll( x => new string[] { "PRM" , "PRP" , "PFM" , "PFP" }.Contains( x.cod_eccezione.Trim( ) ) );
                }
            }

            var q = query.OrderBy( x => x.cod_eccezione )
                    .Select( ecc => new
                    {
                        desc = ecc.cod_eccezione + " - " + ecc.desc_eccezione ,
                        cod = ecc.cod_eccezione ,
                        controlli = ecc.controlli_specifici ,
                        partial = ( ecc.PartialView == null ? "" : ecc.PartialView ) ,
                        chars = ecc.CaratteriMotivoRichiesta == null ? 0 : ecc.CaratteriMotivoRichiesta ,
                        idragg = ecc.id_raggruppamento ,
                        desclunga = ecc.descrizione_eccezione ,
                        importo_preimpostato = ecc.importo_preimpostato ,
                        richiede_attivita_ceiton = Utente.GestitoSirio( ) && ( ecc.RichiedeAttivitaCeiton ||
                                                                            Utente.SelezioneAttivitaCeitonObbligatoria() ||
                                                                            (CeitonManager.ActivityAvailableToday(Date) &&
                                                                            EccezioniManager.IsEccezioneInGruppo2_3(ecc.cod_eccezione)))
                    } );

            if (resp != null && resp.dipendente.spgDip == "0")
            {
                q = q.Where(x => x.cod.StartsWith("PG") == false);
            }
            return Json( new
            {
                result = q ,
                //ferie =ferieFromWcf_convertiteToAsmxResponse//resp.dipendente.ferie
                ferie = resp != null ? resp.dipendente.ferie : null,
                pgq = resp != null ? resp.dipendente.spgDip : null
            } , JsonRequestBehavior.AllowGet );
        }

        public ActionResult getEccezioniAttivita ( string idecc )
        {
            idecc = idecc.Trim( ',' );
            int[] ids = Array.ConvertAll( idecc.Split( ',' ) , a => Convert.ToInt32( a ) );
            var db = new digiGappEntities( );
            var eccezioni_ric = db.MyRai_Eccezioni_Richieste.Where( x => ids.Contains( x.id_eccezioni_richieste ) ).ToList( );
            eccezioni_ric = eccezioni_ric.OrderBy( x => ids.ToList( ).IndexOf( x.id_eccezioni_richieste ) ).ToList( );

            return View( "~/Views/approvazioneAttivita/subpartial/trEcc_conferma.cshtml" , eccezioni_ric );
        }

      
        [HttpPost]
        public ActionResult saveProposteAuto ( PropostaAutoToSave[] proposte )
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );
            if ( proposte != null && proposte.Count( ) > 0 )
            {
                DateTime data;
                Boolean datavalida = DateTime.TryParseExact( proposte[0].d , "ddMMyyyy" , null , System.Globalization.DateTimeStyles.None , out data );
                if ( !datavalida )
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                        Data = new { result = "Errore nella validazione della data" }
                    };
                }
                var db = new myRaiData.digiGappEntities( );
                foreach ( var p in proposte )
                {
                    var eccAmmessa = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == p.cod ).FirstOrDefault( );
                    if ( eccAmmessa == null || ( HomeManager.GetCeitonAttivitaObbligatoriaPerSede( ) && Utente.GestitoSirio( ) && eccAmmessa.RichiedeAttivitaCeiton && String.IsNullOrWhiteSpace( p.idAttivitaCeiton ) ) )
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                            Data = new { result = "Eccezione non valida/Attivita Ceiton mancante" }
                        };
                    }
                }

                foreach ( var p in proposte )
                {
                    if ( p.index >= 0 )//queste provengono come prop automatiche da CICS
                    {

                        //InserimentoEccezioneModel model = new InserimentoEccezioneModel();
                        string dalleMin = "";
                        string alleMin = "";

                        InserimentoEccezioneModel model = new InserimentoEccezioneModel( );

                        model.dalle = p.dalle;
                        model.alle = p.alle;
                        model.cod_eccezione = p.cod;
                        model.data_da = data.ToString( "dd/MM/yyyy" );
                        model.data_a = model.data_da;
                        model.nota_richiesta = p.nota;
                        model.IsFromProposteAuto = true;
                        //08052018
                        model.quantita = p.quantita;
                        //
                        model.idAttivitaCeitonDaPropAutomatiche = p.idAttivitaCeiton;
                        model.MatricolaApprovatoreProduzione = p.MatricolaApprovatoreProduzione;
                        model.DaProposteAuto = true;

                        var eccdb = db.L2D_ECCEZIONE.Where( x => x.cod_eccezione == model.cod_eccezione ).FirstOrDefault( );
                        if ( eccdb != null && eccdb.unita_misura == "H" && !String.IsNullOrWhiteSpace( model.quantita )
                            && model.quantita != "0" && model.quantita != "00:00" )
                        {
                            model.DontChangeQuantita = true;
                        }

                        if ( !String.IsNullOrWhiteSpace( p.dalle ) )
                            dalleMin = EccezioniManager.calcolaMinuti(p.dalle).ToString().PadLeft(4, '0');
                        if ( !String.IsNullOrWhiteSpace( p.alle ) )
                            alleMin = EccezioniManager.calcolaMinuti(p.alle).ToString().PadLeft(4, '0');

                        if ( !String.IsNullOrWhiteSpace( dalleMin ) && !String.IsNullOrWhiteSpace( alleMin ) )
                        {
                            if ( dalleMin != "0000" || alleMin != "0000" )
                                model.quantita = EccezioniManager.CalcolaQuantitaOreMinuti(dalleMin, alleMin);
                        }
                        if ( model.quantita == null )
                            model.quantita = p.quantita;

                        EsitoInserimentoEccezione esitoInserimento = EccezioniManager.Inserimento( model );

                        if ( esitoInserimento.response != "OK" )
                        {
                            Utente.GetPOH( true );
                            return new JsonResult
                            {
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                                Data = new { result = esitoInserimento.response }
                            };
                        }
                    }
                    if ( p.index == -1 )//queste provengono come prop automatiche da PRONTUARIO
                    {
                        InserimentoEccezioneModel model = new InserimentoEccezioneModel( );

                        model.cod_eccezione = p.cod;
                        model.data_da = data.ToString( "dd/MM/yyyy" );
                        model.data_a = model.data_da;
                        model.nota_richiesta = p.nota;

                        EsitoInserimentoEccezione esitoInserimento = EccezioniManager.Inserimento( model );

                        if ( esitoInserimento.response != "OK" )
                        {
                            Utente.GetPOH( true );
                            return new JsonResult
                            {
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                                Data = new { result = esitoInserimento.response }
                            };
                        }
                    }
                }
                Utente.GetPOH( true );
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "OK" }
                };
            }
            else
            {
                Utente.GetPOH( true );
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Nessuna eccezione proposta da caricare" }
                };
            }
        }

        public ActionResult CheckData ( )
        {
            string s = BackupManager.CheckSavedData(CommonManager.GetCurrentUserMatricola());
            return Content( s );
        }

        public static InserimentoEccezioneModel GetModelEccezioneCollegata ( InserimentoEccezioneModel model ,
            HttpRequestBase request )
        {
            if ( request["eccezione_collegata"] == null )
                return null;
            string EccezioneCollegata = request["eccezione_collegata"].ToString( );
            string par = request["parametro_passato"].ToString( );
            string propEccezioneCollegata = request["parametro_ricevuto"].ToString( );
            string valorePropEccezioneCollegata = request["ep-" + par].ToString( );
            InserimentoEccezioneModel modelEccezioneCollegata = new InserimentoEccezioneModel( );
            foreach ( var prop in modelEccezioneCollegata.GetType( ).GetProperties( ) )
            {
                if ( prop.Name.ToLower( ) == propEccezioneCollegata.ToLower( ) )
                {
                    prop.SetValue( modelEccezioneCollegata , valorePropEccezioneCollegata , null );
                }
            }
            modelEccezioneCollegata.data_a = model.data_a;
            modelEccezioneCollegata.data_da = model.data_da;
            modelEccezioneCollegata.cod_eccezione = EccezioneCollegata;
            modelEccezioneCollegata.quantita = "1";
            modelEccezioneCollegata.DontChangeQuantita = true;

            return modelEccezioneCollegata;
        }

        public string CheckQuantitaAmmessa ( InserimentoEccezioneModel model )
        {
            if (
                String.IsNullOrWhiteSpace( model.cod_eccezione ) ||
                model.cod_eccezione.Trim( ) == "URH" ||
                model.cod_eccezione.Trim( ) == "UMH" ||
                !EccezioniManager.IsOraria( model.cod_eccezione ) ||
                Utente.GetQuadratura( ) == Quadratura.Giornaliera
                )
                return null;


            WSDigigapp service = new WSDigigapp( )
            {
                Credentials = new System.Net.NetworkCredential(
                    CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] ,
                    CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] )
            };

            //proposteResponse resp = service.getProposteAutomatiche(CommonManager.GetCurrentUserMatricola(),model.data_da.Replace("/","") ,"BU", 70);
            proposteResponse resp = ServiceWrapper.GetProposteAutomaticheWrapper(CommonManager.GetCurrentUserMatricola(), model.data_da.Replace("/", ""));

            if ( resp == null || resp.eccezioni == null || !resp.eccezioni.Any( x => x.cod.Trim( ) == model.cod_eccezione.Trim( ) ) )
                return null;

            dayResponse g = service.getEccezioni( CommonManager.GetCurrentUserMatricola( ) , model.data_da.Replace( "/" , "" ) , "BU" , 70 );
            if ( g.eccezioni == null || g.eccezioni.Count( ) == 0 || !g.eccezioni.Any( x => x.cod.Trim( ) == "CAR" ) )
                return null;

            if ( !g.eccezioni.Any( x => x.cod == "MR" || x.cod == "MF" || x.cod == "MFS" ) )
                return null;

            var eccCarenza = g.eccezioni.Where( x => x.cod.Trim( ) == "CAR" ).FirstOrDefault( );
            if ( String.IsNullOrWhiteSpace( eccCarenza.qta ) || eccCarenza.qta.Trim( ) == "00:00" || eccCarenza.qta.Trim( ) == "0" )
                return null;

            resp = TimbratureCore.TimbratureManager.ScalaCarenzaDaPa( resp , g );

            var propAutomatica = resp.eccezioni.Where( x => x.cod.Trim( ) == model.cod_eccezione.Trim( ) ).FirstOrDefault( );
            if ( propAutomatica == null )
                return null;

            if ( model.quantita.ToMinutes( ) > propAutomatica.qta.ToMinutes( ) )
                return "La quantità richiesta non è ammessa";
            else
                return null;

        }

        public Boolean IsEccezioneSbloccata ( InserimentoEccezioneModel model , Eccezione PropAuto )
        {
            var db = new digiGappEntities( );
            string matr = CommonManager.GetCurrentUserMatricola( );

            DateTime dataDa;
            bool d1 = DateTime.TryParseExact( model.data_da , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dataDa );
            DateTime dataA;
            bool d2 = DateTime.TryParseExact( model.data_a , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dataA );

            var sbloccata = db.MyRai_Eccezioni_Sbloccate.Where( x => x.matricola == matr && x.data == dataDa && x.data == dataA && x.eccezione == model.cod_eccezione ).FirstOrDefault( );
            if ( sbloccata == null )
                return false;
            else
            {
                sbloccata.quantita_proposta = PropAuto.qta;
                sbloccata.quantita_richiesta_dip = model.quantita;
                db.SaveChanges( );
                return true;
            }
        }


        public string CheckSeMaggioreDiPropostaAuto ( InserimentoEccezioneModel model )
        {

            if ( String.IsNullOrWhiteSpace( model.cod_eccezione ) || !EccezioniManager.IsOraria( model.cod_eccezione ) )
                return null;

            WSDigigapp service = new WSDigigapp( )
            {
                Credentials = new System.Net.NetworkCredential(
                    CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] ,
                    CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] )
            };
            int MinutiInserimento = model.quantita.ToMinutes( );

            dayResponse respge = null;

            if ( model != null && !String.IsNullOrWhiteSpace( model.cod_eccezione ) && model.cod_eccezione.Trim( ) == "ROH" )
            {
                respge = service.getEccezioni( CommonManager.GetCurrentUserMatricola( ) , model.data_da.Replace( "/" , "" ) , "BU" , 80 );

                var pa = GetProposteAutoModel( respge.giornata.data , respge );
                if ( pa != null && pa.EccezioniProposte != null && pa.EccezioniProposte.Any( ) )
                {
                    var roh = pa.EccezioniProposte.Where( x => x.cod == "ROH" ).FirstOrDefault( );
                    int qtaRohDigitato = model.alle.ToMinutes( ) - model.dalle.ToMinutes( );
                    if ( qtaRohDigitato == 0 || ( roh != null && qtaRohDigitato > roh.qta.ToMinutes( ) ) )
                    {
                        return "Intervallo richiesto non valido.";
                    }
                }
            }
            //proposteResponse resp = service.getProposteAutomatiche(CommonManager.GetCurrentUserMatricola(), model.data_da.Replace("/", ""), "BU", 70);
            proposteResponse resp = ServiceWrapper.GetProposteAutomaticheWrapper(CommonManager.GetCurrentUserMatricola(), model.data_da.Replace("/", ""));
            if ( resp == null || resp.eccezioni == null || !resp.eccezioni.Any( x => x.cod.Trim( ) == model.cod_eccezione.Trim( ) ) )
            {
                if ( respge == null )
                    respge = service.getEccezioni( CommonManager.GetCurrentUserMatricola( ) , model.data_da.Replace( "/" , "" ) , "BU" , 80 );

                if ( IsEccezioneDaRic( model.cod_eccezione ) )
                {
                    int MinutiRic = GetMinutiRicalcolo( model , respge );

                    if ( MinutiInserimento > MinutiRic )
                    {
                        Logger.LogAzione( new MyRai_LogAzioni( )
                        {
                            applicativo = "PORTALE" ,
                            data = DateTime.Now ,
                            matricola = CommonManager.GetCurrentUserMatricola( ) ,
                            provenienza = "CheckSeMaggioreDiPropostaAuto" ,
                            operazione = "Verifica Quantita" ,
                            descrizione_operazione = CommonManager.GetCurrentUserMatricola( ) + " " + model.data_da + " " + model.cod_eccezione
                            + " Richiesti min:" + MinutiInserimento + " Ricalcolo min:" + MinutiRic
                        } );
                        return "La quantità è maggiore di quella calcolata come spettante.";
                    }
                }

                return null;
            }

            var PropAuto = resp.eccezioni.Where( x => x.cod.Trim( ) == model.cod_eccezione.Trim( ) ).FirstOrDefault( );
            if ( PropAuto == null )
                return null;

            int MinutiProposta = PropAuto.qta.ToMinutes( );

            if ( MinutiInserimento == 0 || MinutiProposta == 0 )
                return null;

            //quantita sbloccata myrai_eccezioni_sbloccate
            if ( IsEccezioneSbloccata( model , PropAuto ) )
                return null;


            if ( MinutiInserimento > MinutiProposta )
            {
                Logger.LogAzione( new MyRai_LogAzioni( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    matricola = CommonManager.GetCurrentUserMatricola( ) ,
                    provenienza = "CheckSeMaggioreDiPropostaAuto" ,
                    operazione = "Verifica Quantita PA" ,
                    descrizione_operazione = CommonManager.GetCurrentUserMatricola( ) + " " + model.data_da + " " + model.cod_eccezione
                               + " Richiesti min:" + MinutiInserimento + " Proposta min:" + MinutiProposta
                } );
                return "La quantità richiesta non è ammessa.";
            }
            else
                return null;
        }

        public int GetMinutiRicalcolo ( InserimentoEccezioneModel model , dayResponse resp )
        {
            if ( DayAnalysisFactory.DayAnalysisExists( model.cod_eccezione ) )
            {
                var DA = DayAnalysisFactory.GetDayAnalysisClass(model.cod_eccezione, resp, myRai.Models.Utente.GetQuadratura() == Quadratura.Giornaliera);
                if ( DA != null )
                {
                    var QuantitaRicalcolata = DA.GetEccezioneQuantita( );
                    if ( QuantitaRicalcolata != null )
                        return QuantitaRicalcolata.QuantitaMinuti;
                }
            }
            return 0;
        }

        public Boolean IsEccezioneDaRic ( string ecc )
        {
            string eccezioniRic = CommonManager.GetParametro<string>( EnumParametriSistema.ProposteAutoRicalcoloQuantita );
            return eccezioniRic != null && eccezioniRic.Split( ',' ).Contains( ecc );
        }

        public ActionResult Inserimento ( InserimentoEccezioneModel model )
        {
            string matricolaApprovatoreProduzione = null;

            try
            {
                matricolaApprovatoreProduzione = HttpContext.Request["ep-ApprovatoreSelezionato"].ToString( );
            }
            catch ( Exception ex )
            {

            }

            if ( !String.IsNullOrEmpty( matricolaApprovatoreProduzione ) &&
                matricolaApprovatoreProduzione.Trim( ).Length != 6 )
            {
                Logger.LogErrori( new MyRai_LogErrori( ) { error_message = "Approvatore produzione errato: " + matricolaApprovatoreProduzione } );

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Errore selezione approvatore (" + matricolaApprovatoreProduzione + ")" }
                };
            }

            model.MatricolaApprovatoreProduzione = matricolaApprovatoreProduzione;

            if ( model.cod_eccezione == "RKMF" )
                model.importo = CommonManager.GetParametro<string>( EnumParametriSistema.ImportoRKMF );

            if ( model.cod_eccezione == "RVIV" )
                model.importo = CommonManager.GetParametro<string>( EnumParametriSistema.ImportoRVIV );

            string EsitoCheckMaggPA = CheckSeMaggioreDiPropostaAuto( model );
            if ( EsitoCheckMaggPA != null )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = EsitoCheckMaggPA }
                };
            }

            string EsitoCheck = CheckQuantitaAmmessa( model );
            if ( EsitoCheck != null )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = EsitoCheck }
                };
            }

            InserimentoEccezioneModel modelColl = GetModelEccezioneCollegata( model , HttpContext.Request );
            if ( modelColl != null )
            {
                if ( modelColl.cod_eccezione != null &&
                    modelColl.cod_eccezione.Trim( ).ToUpper( ) == "RICO" &&
                    String.IsNullOrWhiteSpace( modelColl.importo ) )
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                        Data = new { result = "Codice ristorante non pervenuto nei dati" }
                    };
                }
            }


            EsitoInserimentoEccezione esito = EccezioniManager.Inserimento( model );

            if ( esito.response == "OK" )
            {
                if (model.cambioturni == "1")
                {
                    DateTime D1, D2;
                    DateTime.TryParseExact(model.data_da, "dd/MM/yyyy", null, DateTimeStyles.None, out D1);
                    DateTime.TryParseExact(model.data_a, "dd/MM/yyyy", null, DateTimeStyles.None, out D2);

                    string esitoCambio = EccezioniManager.ControllaFerie(CommonManager.GetCurrentUserMatricola(), D1, D2, Utente.SedeGapp(), Utente.Reparto(),
                        Utente.GestitoSirio(), Utente.Nominativo());

                    if (esitoCambio != null)
                    {
                        Logger.LogErrori(new MyRai_LogErrori()
                        {
                            applicativo = "PORTALE",
                            data = DateTime.Now,
                            error_message = esitoCambio,
                            matricola = CommonManager.GetCurrentUserMatricola(),
                            provenienza = "Inserimento"
                        });
                    }
                }
                if ( modelColl != null )
                {
                    esito = EccezioniManager.Inserimento( modelColl );
                }

                if (model.cod_eccezione.Trim() == "UME")
                {
                    if (!EccezioniManager.IsGiaPresente("RPAF", model.data_da))
                    {
                        DateTime Drpaf;
                        if (DateTime.TryParseExact(model.data_da, "dd/MM/yyyy", null, DateTimeStyles.None, out Drpaf))
                        {
                            var ListEccezioniAmmesse = EccezioniManager.GetListaEccezioniPossibili(
                                CommonManager.GetCurrentUserMatricola7chars(), Drpaf);
                            if (ListEccezioniAmmesse.Any(x => x.cod_eccezione.Trim() == "RPAF"))
                    {
                        modelColl = new InserimentoEccezioneModel();
                        modelColl.cod_eccezione = "RPAF";
                        modelColl.data_da = model.data_da;
                        modelColl.data_a = model.data_a;
                        modelColl.quantita = "1";
                        esito = EccezioniManager.Inserimento(modelColl);
                            }
                        }
                    }
                }
            }

            //refresh valori sessione per POH ROH :
            Utente.GetPOH( true );

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = esito.response }
            };
        }

        public ActionResult GetDettaglioRichiesta ( int IdEccezioneRichiesta )
        {
            PopupDettaglioGiornata model = HomeManager.GetPopupDettaglioGiornataModel( IdEccezioneRichiesta );
            return View( "~/Views/notifiche/dettaglio.cshtml" , model );
        }

        public ActionResult GetTimbratureAjaxView ( string date , bool nocalendar = true , string matr = null , string sessionId = null )
        {
            DateTime Date;
            Boolean datavalida = DateTime.TryParseExact( date , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out Date );

            if ( matr != null && matr.Length == 7 )
                matr = matr.Substring( 1 );

            ModelDash model = new ModelDash( );
            model.dettaglioGiornata = HomeManager.GetTimbratureTodayModel( Date , matr , false , sessionId );
            model.dettaglioGiornata.HideRefresh = nocalendar;
            return View( "~/Views/home/_timbraturetoday.cshtml" , model );
        }
        public ActionResult getInfoSpostamentoRiposi(string datada, string dataa)
        {
            string matr = CommonManager.GetCurrentUserMatricola();
            string MatricolePermesse = CommonManager.GetParametro<string>(EnumParametriSistema.CambioTurniMatricola);
           
            if (MatricolePermesse != null && ( MatricolePermesse == "*" || MatricolePermesse.Split(',').Contains(matr)))
            {
                DateTime D1, D2;
                DateTime.TryParseExact(datada, "dd/MM/yyyy", null, DateTimeStyles.None, out D1);
                DateTime.TryParseExact(dataa, "dd/MM/yyyy", null, DateTimeStyles.None, out D2);
                int dayWeek = (int)D1.DayOfWeek;
                if (dayWeek == 0) dayWeek = 7;
                if (dayWeek == 1)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false }
                    };
                }
                List<DateTime> GiorniRiposo = EccezioniManager.GetNonLavoratoRiposoInPeriodo(CommonManager.GetCurrentUserMatricola(), D1, D2);
                if (GiorniRiposo.Any())
                {
                    string giorniPrevisti = "";
                    if (dayWeek == 2)
                    {
                        giorniPrevisti = D1.AddDays(-1).ToString("dd/MM/yyyy");
                    }
                    if (dayWeek > 2)
                    {
                        giorniPrevisti = D1.AddDays(-(dayWeek - 1)).ToString("dd/MM/yyyy");
                        if (GiorniRiposo.Count > 1)
                            giorniPrevisti += " - " + D1.AddDays(-(dayWeek - 1) + 1).ToString("dd/MM/yyyy");
                    }
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = true, giorni = giorniPrevisti }
                    };
                }
                else
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false }
                    };
            }
            else
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false }
                };

        }
        public ActionResult GetInfoGiornataAjaxView ( string date , int? ideccezionerichiesta = null , string matr = null , string sessionId = null , bool NascondiMessaggio = false )
        {
            if ( ideccezionerichiesta == 0 )
                ideccezionerichiesta = null;

            DateTime Date;
            Boolean datavalida = DateTime.TryParseExact( date , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out Date );

            DateTime Date1 = Date.AddDays( 1 );
            if ( Date1.Year < 1901 )
                return Content( "" );

            InfoGiornataModel model = new InfoGiornataModel( );
            model.NascondiMessaggio = NascondiMessaggio;

            model.ApertaDaResp = ( matr != null && matr != CommonManager.GetCurrentUserMatricola( ) && matr.Substring( 1 ) != CommonManager.GetCurrentUserMatricola( ) );

            if ( Session["DatePerEvidenze"] != null )
            {
                string dd = Session["DatePerEvidenze"].ToString( );
                DateTime Dchi;
                DateTime.TryParseExact( dd , "ddMMyyyy" , null , DateTimeStyles.None , out Dchi );

                model.GiornataChiusa = ( Date < Dchi );

            }

            model.giornoSettimana = Date.ToString( "dddd" ).ToUpper( );

            model.TipoQuadratura = Utente.GetQuadratura( ).ToString( );

            int poh = 0;
            int roh = 0;
            Utente.GetPOHandROH( false , Date , out poh , out roh );
            //model.POHdaRecuperare = (Utente.GetPOH(false, Date) - Utente.GetROH(false, Date)).ToString();
            model.POHdaRecuperare = ( poh - roh ).ToString( );
            model.CeitonAttivitaObbligatoriaPerSede = HomeManager.GetCeitonAttivitaObbligatoriaPerSede( );
            model.EccezioniRichiedentiCeiton = HomeManager.GetEccezioniRichiedentiCeiton( );

            model.BonusCarenza = EccezioniManager.GetMinutiCarenzaPerSede(Utente.SedeGapp(Date), Date);
            if ( Utente.GetQuadratura( ) == Quadratura.Settimanale )
            {
                DateTime[] da = CommonManager.GetIntervalloSettimanaleSedePerGiorno( Date );
                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client( );
                wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential( CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] , CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] );

                var dett = new DettaglioSettimanaleModel(
                           wcf1.GetPresenzeSettimanaliProtected( CommonManager.GetCurrentUserMatricola( ) ,
                           da[0].ToString( "ddMMyyyy" ) , da[1].ToString( "ddMMyyyy" ) ,
                           Utente.DataInizioValidita( ) , da[0] ) );
                model.BilancioPositivoPerQuadraturaSettimanale = ( dett.DeltaTotale != null &&
                                                                  dett.DeltaTotale.Contains( "+" ) &&
                                                                  dett.DeltaTotale.Replace( "+" , "" ).Trim( ).ToMinutes( ) > 0 );
            }

            model.MaxDurataURH = EccezioniManager.GetMaxDurataURH( Date );
            string[] interv = EccezioniManager.GetIntervalliMensa( Date );
            if ( interv != null )
            {
                model.intervallo_mensa = interv[0];
                model.intervallo_mensa_serale = interv[1];

                if ( model.intervallo_mensa != null && model.intervallo_mensa.Contains( "/" ) )
                {
                    model.intervallo_mensa_pranzo_minuti_from = model.intervallo_mensa.Split( '/' )[0].ToMinutes( );
                    model.intervallo_mensa_pranzo_minuti_to = model.intervallo_mensa.Split( '/' )[1].ToMinutes( );
                }
                if ( model.intervallo_mensa_serale != null && model.intervallo_mensa_serale.Contains( "/" ) )
                {
                    model.intervallo_mensa_serale_minuti_from = model.intervallo_mensa_serale.Split( '/' )[0].ToMinutes( );
                    model.intervallo_mensa_serale_minuti_to = model.intervallo_mensa_serale.Split( '/' )[1].ToMinutes( );
                }
            }

            var db = new digiGappEntities( );
            string miasede = Utente.SedeGapp( Date );
            model.MensaDisponibilePC = db.L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp == miasede )
                .Select( a => a.mensa_disponibile ).FirstOrDefault( );

            string badge = "";
            if ( ideccezionerichiesta == null && String.IsNullOrEmpty( matr ) )
            {
                badge = "00" + CommonManager.GetCurrentUserMatricola( );
            }
            else if ( ( ideccezionerichiesta == null ) && !String.IsNullOrEmpty( matr ) )
            {
                if ( matr.Length == 7 )
                    matr = matr.Substring( 1 );
                badge = matr;
            }
            else
            {
                try
                {
                    int idEcc = ideccezionerichiesta.GetValueOrDefault( );
                    var item = db.MyRai_Eccezioni_Richieste.Where( e => e.id_eccezioni_richieste.Equals( idEcc ) ).FirstOrDefault( );

                    if ( item != null )
                    {
                        var query =
                            from eccezione in db.MyRai_Eccezioni_Richieste
                            join richiesta in db.MyRai_Richieste on eccezione.id_richiesta equals richiesta.id_richiesta
                            where eccezione.id_eccezioni_richieste == idEcc
                            select new { Eccezione = eccezione , Richieste = richiesta };

                        if ( query != null )
                        {
                            badge = query.FirstOrDefault( ).Richieste.matricola_richiesta;

                            if ( matr == null )
                            {
                                if ( CommonManager.GetCurrentUserMatricola( ) != badge )
                                {
                                    model.ApertaDaResp = true;
                                }
                            }

                            matr = badge;
                        }
                    }
                }
                catch ( Exception ex )
                {
                }
            }

            string padBadge = badge.PadLeft( 8 , '0' );

            var scontrino = db.MyRai_MensaXML.Where( x => x.TransactionDateTime >= Date && x.TransactionDateTime < Date1 &&
                 x.Badge == padBadge ).FirstOrDefault( );
            if ( scontrino != null )
            {
                string[] linee = scontrino.XMLorig.Split( new string[] { "<Line>" } , StringSplitOptions.None )
                    .Where( item => item != "</Line>" ).ToArray( );
                model.MensaDataOra = scontrino.TransactionDateTime;
                if ( linee != null && linee.Length > 2 )
                {
                    model.MensaSede = ( linee[2] != null ? linee[2].Replace( "</Line>" , "" ) : "" );
                    if ( linee.Length > 3 && !String.IsNullOrWhiteSpace( linee[3] ) )
                        model.MensaSede += " - " + linee[3].Substring( linee[3].IndexOf( '-' ) + 1 ).Replace( "</Line>" , "" );
                }

            }
            dayResponse d = HomeManager.GetTimbratureTodayModel( Date , matr , false , sessionId );
            model.HaSmap1min = d.eccezioni != null && d.eccezioni.Any( ) && d.eccezioni.Any( x => x.cod != null && x.cod.Trim( ) == "SMAP" && x.dalle.ToMinutes( ) == x.alle.ToMinutes( ) - 1 );

            if ( d != null && d.timbrature != null && d.timbrature.Count( ) > 0 )
            {
                if ( d.timbrature.First( ).entrata != null && d.timbrature.First( ).entrata.orario != null )
                    model.PrimaTimbratura = d.timbrature.First( ).entrata.orario.Trim( );
                if ( d.timbrature.Last( ).uscita != null && d.timbrature.Last( ).uscita.orario != null )
                    model.UltimaTimbratura = d.timbrature.Last( ).uscita.orario.Trim( );
            }
            WSDigigapp service = new WSDigigapp( )
            {
                Credentials = new System.Net.NetworkCredential( CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] , CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] )
            };

            try
            {
                if ( d.giornata != null )
                {
                    model.codOrarioPrevisto = d.giornata.orarioTeorico;
                    model.descOrarioPrevisto = service.getDescrizionecodOrario( d.giornata.orarioTeorico );
                    model.descOrarioReale = d.giornata.descOrarioReale;
                    model.codOrarioReale = d.giornata.orarioReale;
                    model.MaggiorPresenza = d.giornata.maggiorPresenza;
                    model.DataGiornata = d.giornata.data;
                }
                if ( d.orario != null )
                {
                    model.orarioIngressoTurno = d.orario.hhmm_entrata_48;
                    model.orarioUscitaTurno = d.orario.hhmm_uscita_48;
                    model.minutiMensa = d.orario.intervallo_mensa;
                }

                model.PresenzaMensa = "";
                if ( d.eccezioni != null )
                {
                    var car = d.eccezioni.Where( x => x.cod != null && x.cod.Trim( ) == "CAR" ).FirstOrDefault( );
                    if ( car != null )
                    {
                        model.Carenza = car.qta;
                        model.CarenzaMinuti = model.Carenza.ToMinutes( );
                    }
                }

                var orar = db.L2D_ORARIO.Where( c => c.cod_orario == d.giornata.orarioReale ).FirstOrDefault( );
                if ( orar != null )
                    model.PrevistaPresenza = orar.prevista_presenza == null ? "0" : ( ( int ) orar.prevista_presenza ).ToString( );

                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client( );
                cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential( CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] , CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] );

                MyRaiServiceInterface.MyRaiServiceReference1.getOrarioResponse response =
                    cl.getOrario( d.giornata.orarioReale , date.Replace( "/" , "" ) , CommonManager.GetCurrentUserMatricola( ) , "BU" , 75 );
                model.OrarioReale = response;

                if ( d.giornata.orarioReale == d.giornata.orarioTeorico )
                {
                    model.OrarioPrevisto = response;
                }
                else
                {
                    MyRaiServiceInterface.MyRaiServiceReference1.getOrarioResponse response2 =
                       cl.getOrario( d.giornata.orarioTeorico , date.Replace( "/" , "" ) , CommonManager.GetCurrentUserMatricola( ) , "BU" , 75 );
                    model.OrarioPrevisto = response2;
                }

                model.RICO = d.eccezioni != null && d.eccezioni.Any( x => x.cod == "RICO" );
                //model.RICO = true;

                if ( model.RICO )
                {
                    string codice = d.eccezioni.Where( x => x.cod == "RICO" ).Select( x => x.importo ).FirstOrDefault( );

                    if ( !String.IsNullOrEmpty( codice ) )
                    {
                        CultureInfo culture = new CultureInfo( "it-IT" );
                        TextInfo textInfo = culture.TextInfo;

                        RistoranteConvenzionato ristorante = linkedTableDataController.GetRistoranteConvenzionato( codice );

                        if ( ristorante != null )
                        {
                            model.RistoranteConvensionato = textInfo.ToTitleCase( ristorante.Nominativo.ToLower( ) );
                        }
                    }
                }

                if ( String.IsNullOrWhiteSpace( matr ) || matr == CommonManager.GetCurrentUserMatricola( ) )
                    model.TipoDip = Utente.TipoDipendente( );
                else
                    model.TipoDip = GetUtenteByMatricola( matr );

                try
                {
                    if ( !model.ApertaDaResp )
                    {
                    var temp = this._noteRichiesteDataController.GetNoteRichieste( CommonManager.GetCurrentUserMatricola( ) , model.DataGiornata );

                    if ( temp != null && temp.Any( ) )
                    {
                        model.Note = new List<MyRai_Note_Richieste_EXT>( );
                        model.Note = ( from w in temp
                                       select new MyRai_Note_Richieste_EXT( )
                                       {
                                           DataCreazione = w.DataCreazione ,
                                           DataGiornata = w.DataGiornata ,
                                           DataLettura = w.DataLettura ,
                                           DataUltimaModifica = w.DataUltimaModifica ,
                                           DescrizioneMittente = w.DescrizioneMittente ,
                                           DescrizioneVisualizzatore = w.DescrizioneVisualizzatore ,
                                           Destinatario = w.Destinatario ,
                                           Id = w.Id ,
                                           Immagine = CommonManager.GetImmagineBase64( w.Mittente ) ,
                                           Messaggio = w.Messaggio ,
                                           Mittente = w.Mittente ,
                                           SedeGapp = w.SedeGapp ,
                                           Visualizzatore = w.Visualizzatore
                                       } ).ToList( );
                    }

                    model.MatricolaUtente = CommonManager.GetCurrentUserMatricola( );
                    //model.NomeUtente = CommonManager.GetNominativoPerMatricola(model.MatricolaUtente);
                    model.NomeUtente = Utente.Nominativo( );

                    if ( model.Note != null && model.Note.Any( ) )
                    {
                        model.Note.ForEach( w =>
                         {
                             if ( !w.DataLettura.HasValue && w.Destinatario.Equals( model.MatricolaUtente ) )
                             {
                                 this._noteRichiesteDataController.SetLettura( w.Id , model.MatricolaUtente , model.NomeUtente );
                             }
                         } );
                    }
                }
                    else
                    {
                        model.Note = new List<MyRai_Note_Richieste_EXT>( );
                    }
                }
                catch ( Exception ex )
                {
                    model.Note = new List<MyRai_Note_Richieste_EXT>( );
                }

            }
            catch ( Exception ed )
            { }
            return PartialView( "~/Views/home/infogiornata.cshtml" , model );
        }

        /// <summary>
        /// Reperimento di un utente a partire dalla matricola
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        private string GetUtenteByMatricola ( string term )
        {
            it.rai.servizi.hrgb.Service s = new it.rai.servizi.hrgb.Service( );
            // reperimento della sede dell'utente corrente
            var r = s.EsponiAnagrafica_Net("COMINT", myRai.Models.Utente.EsponiAnagrafica()._matricola);

            if ( r.DT_Anagrafica.Rows != null )
            {
                foreach ( System.Data.DataRow item in r.DT_Anagrafica.Rows )
                {
                    return item[21].ToString( );
                }
            }
            return null;
        }

        public ActionResult GetSegnalazioniAjaxView ( string date , string matricola = null , string abilitaApprovazione = null , int? ideccezionerichiesta = null , bool hideCurrentDataRow = false , string sessionId = null , bool NascondiCestino = false )
        {
            if ( !String.IsNullOrEmpty( matricola ) )
            {
                matricola = matricola.PadLeft( 6 , '0' );
            }
            else if ( ideccezionerichiesta.HasValue )
            {
                try
                {
                    int idEcc = ideccezionerichiesta.GetValueOrDefault( );

                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        var item = db.MyRai_Eccezioni_Richieste.Where( e => e.id_eccezioni_richieste.Equals( idEcc ) ).FirstOrDefault( );

                        if ( item != null )
                        {
                            var query =
                                from eccezione in db.MyRai_Eccezioni_Richieste
                                join richiesta in db.MyRai_Richieste on eccezione.id_richiesta equals richiesta.id_richiesta
                                where eccezione.id_eccezioni_richieste == idEcc
                                select new { Eccezione = eccezione , Richieste = richiesta };

                            if ( query != null )
                            {
                                matricola = query.FirstOrDefault( ).Richieste.matricola_richiesta;
                            }
                        }

                    }
                }
                catch ( Exception ex )
                {
                }
            }
            else
            {
                matricola = CommonManager.GetCurrentUserMatricola( );
                matricola = matricola.PadLeft( 6 , '0' );
            }

            if ( matricola != null && matricola.Length == 7 )
                matricola = matricola.Substring( 1 );

            DateTime Date;
            if ( date != null && date.Contains( "/" ) )
                DateTime.TryParseExact( date , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out Date );
            else
                DateTime.TryParseExact( date , "ddMMyyyy" , null , System.Globalization.DateTimeStyles.None , out Date );

            ModelDash model = new ModelDash( );
            model.NascondiCestino = NascondiCestino;

            model.dettaglioGiornata = HomeManager.GetTimbratureTodayModel( Date , matricola , true , sessionId );
            if ( model.dettaglioGiornata != null )
            {
                model.dettaglioGiornata.HideRefresh = true;
                if ( !String.IsNullOrEmpty( abilitaApprovazione ) )
                {
                    model.AbilitaApprovazione = ( abilitaApprovazione.Equals( "1" ) ? true : false );

                    if ( model.AbilitaApprovazione && hideCurrentDataRow )
                    {
                        int idEcc = ideccezionerichiesta.GetValueOrDefault( );

                        model.dettaglioGiornata.eccezioni = model.dettaglioGiornata.eccezioni.Where( e => !e.IdEccezioneRichiesta.Equals( idEcc ) ).ToArray( );
                    }
                }
            }

            if ( model.dettaglioGiornata != null && model.dettaglioGiornata.eccezioni != null && model.dettaglioGiornata.eccezioni.Any( ) )
            {
                foreach ( var item in model.dettaglioGiornata.eccezioni )
                {
                    if (item.IdStato == (int)EnumStatiRichiesta.Approvata)
                    {
                        if ( String.IsNullOrEmpty( item.matricolaPrimoLivello ) )
                        {
                            using ( digiGappEntities db = new digiGappEntities( ) )
                            {
                                var ecc = db.MyRai_Eccezioni_Richieste.Where( w => w.id_eccezioni_richieste.Equals( item.IdEccezioneRichiesta ) ).FirstOrDefault( );
                                if ( ecc != null )
                                {
                                    item.matricolaPrimoLivello = ecc.matricola_primo_livello;
                                }
                            }
                        }

                        if ( !String.IsNullOrEmpty( item.matricolaPrimoLivello ) )
                        {
                            var app = BatchManager.GetUserData( item.matricolaPrimoLivello );
                            if ( app != null )
                            {
                                item.DescrizioneApprovatorePrimoLivello = app.nominativo;
                            }
                        }
                    }
                }
            }

            return View( "~/Views/home/_segnalazioni.cshtml" , model );
        }

        [HttpPost]
        public ActionResult InserimentoDaTimbr ( EccezioneCopertura[] model , string dataecc )
        {
            string esito = "";
            var db = new myRaiData.digiGappEntities( );
            for ( int i = 0 ; i < model.Length ; i++ )
            {

                InserimentoEccezioneModel ModelInserimento = new InserimentoEccezioneModel( )
                {
                    data_a = dataecc ,
                    data_da = dataecc ,
                    dalle = "" ,
                    alle = "" ,
                    cod_eccezione = model[i].nome ,
                    quantita = model[i].durata ,
                    nota_richiesta = model[i].nota
                };


                if ( model[i].intv.Count( x => x == ',' ) == 1 )
                {
                    ModelInserimento.SetDalleAlle( model[i].intv );
                }


                DateTime D;
                bool datavalida = DateTime.TryParseExact( dataecc , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out D );
                int du = 0;
                int.TryParse( model[i].durata , out du );
                if ( du == 0 || datavalida == false || string.IsNullOrWhiteSpace( model[i].nome ) )
                {
                    esito += "Errore per eccezione " + model[i].nome + ": " + "parametri eccezione non validi " + "<br />";
                    continue;
                }
                ModelInserimento.quantita = Convert.ToInt32( ModelInserimento.quantita ).ToHHMM( );
                int IdRichiestaPadre = 0;

                if ( !new List<string>( ) {
                   CommonManager.GetParametro<string>(EnumParametriSistema.EccezioneFittiziaSpostamento),
                   CommonManager.GetParametro<string>(EnumParametriSistema.EccezioneFittiziaIgnora),
                }.Contains( model[i].nome ) )
                {
                    EsitoInserimentoEccezione r = EccezioniManager.Inserimento( ModelInserimento );
                    if ( r.response != "OK" )
                    {
                        esito += "Errore per eccezione " + model[i].nome + ": " + r + "<br />";
                        continue;
                    }
                    else
                        IdRichiestaPadre = r.id_richiesta_padre;

                    if ( !String.IsNullOrWhiteSpace( model[i].eccezione_collegata ) )
                    {
                        InserimentoEccezioneModel modelCollegata = new InserimentoEccezioneModel( )
                        {
                            data_da = ModelInserimento.data_da ,
                            data_a = ModelInserimento.data_a ,
                            dalle = "" ,
                            alle = "" ,
                            quantita = "1" ,
                            cod_eccezione = model[i].eccezione_collegata ,
                            nota_richiesta = model[i].nota ,
                            DontChangeQuantita = true
                        };
                        foreach ( var prop in modelCollegata.GetType( ).GetProperties( ) )
                        {
                            if ( prop.Name.ToLower( ) == model[i].parametro_ricevuto.ToLower( ) )
                            {
                                prop.SetValue( modelCollegata , model[i].valore_parametro , null );
                            }
                        }
                        EsitoInserimentoEccezione r2 = EccezioniManager.Inserimento( modelCollegata );
                        if ( r2.response != "OK" )
                        {
                            esito += "Errore per eccezione " + modelCollegata.cod_eccezione + ": " + r2 + "<br />";
                        }
                    }
                }


                DateTime Decc;
                DateTime.TryParseExact( ModelInserimento.data_da , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None ,
                    out Decc );
                string[] interv = model[i].intv.Trim( ).Split( ',' ).Select( x => x.Trim( ) ).ToArray( );

                foreach ( string intervallo in interv )
                {
                    if ( String.IsNullOrWhiteSpace( intervallo ) && !intervallo.Contains( "/" ) )
                        continue;

                    myRaiData.MyRai_CoperturaCarenze cop = new MyRai_CoperturaCarenze( )
                    {
                        id_richiesta = IdRichiestaPadre ,
                        cod_eccezione = ModelInserimento.cod_eccezione ,
                        matricola = CommonManager.GetCurrentUserMatricola( ) ,
                        data_inserimento = DateTime.Now ,
                        note = ModelInserimento.nota_richiesta ,
                        dalle = intervallo.Split( '/' )[0].Trim( ) ,
                        alle = intervallo.Split( '/' )[1].Trim( ) ,
                        data_eccezione = Decc ,
                        minuti_totali = Convert.ToInt32( model[i].durata ) ,
                        minuti_intervallo = EccezioniManager.calcolaMinuti(intervallo.Split('/')[1]) -
                                            EccezioniManager.calcolaMinuti(intervallo.Split('/')[0])
                    };
                    db.MyRai_CoperturaCarenze.Add( cop );
                }
                DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) );

            }
            //refresh valori sessione per POH ROH :
            Utente.GetPOH( true );
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = esito == "" ? "OK" : esito }
            };
        }

        public ActionResult GetSliderView ( int prog , string dalle , string alle )
        {
            return View( "_slider" , new SliderModel( prog , dalle , alle ) );
        }

        public Boolean EsistonoProposteAutomatiche ( DateTime Date , dayResponse resp )
        {
            ProposteAutomaticheModel PAmodel = GetProposteAutoModel( Date , resp );
            return PAmodel != null;
        }

        public ActionResult GetGiornateProposteAssistente ( )
        {
            string matr = CommonManager.GetCurrentUserMatricola( );
            var db = new digiGappEntities( );
            var assis = db.MyRai_Date_Assistenti.Where( x => x.matricola == matr && x.attivo == true ).FirstOrDefault( );
            List<string> Ldate = new List<string>( );

            if ( assis != null && assis.giornata_ultimo_check < DateTime.Now.Date.AddDays( -1 ) )
            {
                for ( int i = 1 ; i < 1000 ; i++ )
                {
                    if ( ( ( DateTime ) assis.giornata_ultimo_check ).AddDays( i ) < DateTime.Now.Date )
                    {
                        Ldate.Add( ( ( DateTime ) assis.giornata_ultimo_check ).AddDays( i ).ToString( "dd/MM/yyyy" ) );
                    }
                    else
                        break;
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new
                {
                    result = true ,
                    model = Ldate.ToArray( )
                }
            };
        }

        [HttpPost]
        public ActionResult AbilitaAssistente ( )
        {

            var db = new digiGappEntities( );
            string matr = CommonManager.GetCurrentUserMatricola( );
            var r = db.MyRai_Richieste.Where( x => x.matricola_richiesta == matr ).OrderByDescending( x => x.data_richiesta ).FirstOrDefault( );

            DateTime D = DateTime.Now.AddDays( -8 ).Date;
            if ( r != null )
                D = r.data_richiesta.Date;
            var assis = db.MyRai_Date_Assistenti.Where( x => x.matricola == matr ).FirstOrDefault( );
            if ( assis != null )
            {
                assis.attivo = true;
                assis.giornata_ultimo_check = D;
                assis.data = DateTime.Now;
            }
            else
            {
                MyRai_Date_Assistenti newass = new MyRai_Date_Assistenti( )
                {
                    attivo = true ,
                    data = DateTime.Now ,
                    matricola = matr ,
                    giornata_ultimo_check = D
                };
                db.MyRai_Date_Assistenti.Add( newass );
            }
            try
            {
                db.SaveChanges( );
            }
            catch ( Exception ex )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = ex.Message }
                };
            }


            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = "OK" }
            };


        }

        public ActionResult AggiornaDataAssistente ( string data )
        {
            DateTime D;
            if ( DateTime.TryParseExact( data , "dd/MM/yyyy" , null , DateTimeStyles.None , out D ) )
            {
                var db = new digiGappEntities( );
                string matr = CommonManager.GetCurrentUserMatricola( );

                var assis = db.MyRai_Date_Assistenti.Where( x => x.matricola == matr ).FirstOrDefault( );
                assis.data = DateTime.Now;
                assis.giornata_ultimo_check = D;
                db.SaveChanges( );


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "OK" }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Data errata : " + data }
                };
            }


        }
        public ProposteAutomaticheModel GetProposteAssistenteModel ( string date )
        {
            string t = Utente.TipoDipendente( );

            date = date.Replace( "/" , "" );
            WSDigigappDataController service = new WSDigigappDataController( );
            var resp = service.GetEccezioni( CommonManager.GetCurrentUserMatricola( ) , CommonManager.GetCurrentUserMatricola( ) , date , "BU" , 70 );
            DateTime D;
            DateTime.TryParseExact( date , "ddMMyyyy" , null , DateTimeStyles.None , out D );
            ProposteAutomaticheModel PAmodel = GetProposteAutoModel( D , resp );
            if ( PAmodel != null )
            {
                string ec = CommonManager.GetParametro<string>( EnumParametriSistema.AssistenteEccezioni );
                if ( String.IsNullOrWhiteSpace( ec ) )
                    PAmodel.EccezioniProposte = new List<Eccezione>( ).ToArray( );
                else
                    PAmodel.EccezioniProposte = PAmodel.EccezioniProposte.Where( x => ec.Split( ',' ).Contains( x.cod.Trim( ) ) ).ToArray( );
            }
            if (CommonManager.GetCurrentUserMatricola() == "103650")
            {
                if (PAmodel == null || PAmodel.EccezioniProposte != null || PAmodel.EccezioniProposte.Count() == 0)
                {
                    if (PAmodel == null) PAmodel = new ProposteAutomaticheModel();

                    List<Eccezione> L = new List<Eccezione>();
                    L.Add(new Eccezione()
                    {
                        alle = "",
                        dalle = "",
                        cod = "FO",
                        descrittiva_lunga = "FORMAZIONE",
                        qta = "1",
                        CaratteriObbligatoriNota = 5,
                        data = date,
                    });
                    L.Add(new Eccezione()
                    {
                        dalle = "18:00",
                        alle = "19:30",
                        cod = "SMAP",
                        descrittiva_lunga = "STRAORDINARIO IN MAP",
                        qta = "01:30",
                        CaratteriObbligatoriNota = 5,
                        data = date,
                    });
                    PAmodel.EccezioniProposte = L.ToArray();
                }
            }
            return PAmodel;
        }

        /// <summary>
        /// /ajax/getpropostemobile?data_da=15032020&data_a=16032020
        /// </summary>
        /// <param name="data_da"></param>
        /// <param name="data_a"></param>
        /// <returns></returns>
        public ActionResult GetProposteMobile(string data_da, string data_a)
        {
            data_da = data_da.Replace("/", "");
            data_a = data_a.Replace("/", "");
            DateTime D1;
            DateTime.TryParseExact(data_da, "ddMMyyyy", null, DateTimeStyles.None, out D1);
            DateTime D2;
            DateTime.TryParseExact(data_a, "ddMMyyyy", null, DateTimeStyles.None, out D2);


            ProposteAutoMob p = new ProposteAutoMob();

            DateTime Dcurr = D1;
            while (Dcurr <= D2)
            {
                ProposteAutomaticheModel PAmodel = GetProposteAssistenteModel(Dcurr.ToString("dd/MM/yyyy"));
                if (PAmodel != null && PAmodel.EccezioniProposte != null && PAmodel.EccezioniProposte.Any())
                {
                    GiornataMob gio = new GiornataMob();
                    gio.data = Dcurr.ToString("ddMMyyyy");
                    gio.eccezioni = new List<EccezioneMob>();
                    foreach (var ecprop in PAmodel.EccezioniProposte)
                    {
                        gio.eccezioni.Add(new EccezioneMob()
                        {
                            alle = ecprop.alle,
                            dalle = ecprop.dalle,
                            desc = ecprop.descrittiva_lunga,
                            caratteri = ecprop.CaratteriObbligatoriNota,
                            cod = ecprop.cod,
                            qta = ecprop.qta
                        });
                    }
                    p.giornate.Add(gio);
                }

                Dcurr = Dcurr.AddDays(1);
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = p.giornate
            };
        }
        [HttpPost]
        public ActionResult SaveProposteMobile(string prop, string matr)
        {
            Logger.LogAzione(new MyRai_LogAzioni()
            {
                applicativo = "Mobile",
                data = DateTime.Now,
                provenienza = "SaveProposteMobile",
                descrizione_operazione = prop,
                operazione = "SaveProposteMobile"
            }, matr);

            if (String.IsNullOrWhiteSpace(prop))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "Nulla da salvare" }
                };
            }
            try
            {
                PropostaAutoToSave[] Proposte = Newtonsoft.Json.JsonConvert.DeserializeObject<PropostaAutoToSave[]>(prop);

                if (Proposte == null || !Proposte.Any())
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Errore nella deserializzazione delle proposte" }
                    };
                }
                return saveProposteAuto(Proposte);

                string ser = Newtonsoft.Json.JsonConvert.SerializeObject(Proposte);
                Logger.LogAzione(new MyRai_LogAzioni()
                {
                    applicativo = "Mobile",
                    data = DateTime.Now,
                    provenienza = "SaveProposteMobile",
                    descrizione_operazione = ser,
                    operazione = "SaveProposteMobile ser"
                }, matr);
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "OK" }
                };

            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Mobile",
                    data = DateTime.Now,
                    error_message = prop + " Exc:" + ex.ToString(),
                    provenienza = "SaveProposteMobile"
                }, matr);
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }
        }
        public ActionResult GetProposteAssistente ( string date )
        {
            ProposteAutomaticheModel PAmodel = GetProposteAssistenteModel( date );

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = true , model = PAmodel }
            };
        }


        public ActionResult ElaboraEccezioni ( string data , string oraFrom , string oraTo , string spostamento , int smapTestaMinuti )
        {
            ProposteAutomaticheModel PAmodel = WizardFsManager.ElaboraEccezioni( data , oraFrom , oraTo , spostamento , smapTestaMinuti );
            return View( "~/Views/richieste/_proposteAutomatiche.cshtml" , PAmodel );
        }


        public ActionResult CambiaStatoFS ( string stato , DateTime data , string hmGapp , string hmInserito , string spostamento )
        {
            return Content( WizardFsManager.CambiaStatoFS( stato , data , hmGapp , hmInserito , spostamento ) );
        }


        public ActionResult GetProposteAjaxView ( string date , Boolean? forceProposte , string sessionId = null )
        {
            string matr = CommonManager.GetCurrentUserMatricola( );



            DateTime Date;
            Boolean datavalida = DateTime.TryParseExact( date , "dd/MM/yyyy" , null ,
                                                    System.Globalization.DateTimeStyles.None , out Date );

            WSDigigappDataController service = new WSDigigappDataController( );
            dayResponse resp = new dayResponse( );

            if ( !String.IsNullOrWhiteSpace( sessionId ) && Session[sessionId] != null )
            {
                resp = ( dayResponse ) Session[sessionId];
            }
            else
                resp = service.GetEccezioni( matr, matr , Date.ToString( "ddMMyyyy" ) , "BU" , 70 );



            /* WIZARDFS -------------------------------------------*/
            string MatrAb = CommonManager.GetParametro<String>( EnumParametriSistema.AbilitaWizardFS );
            if (!String.IsNullOrWhiteSpace(MatrAb) && MatrAb.Split(',').Contains(matr))
            {
                if ( Date < DateTime.Now &&
                    resp.eccezioni.Any( x => x.cod.Trim( ) == "FS" ) &&
                   !resp.eccezioni.Any( x => new string[] { "GAVE" , "GAVU" , "GAPC" , "SMAP" , "ORV" }.Contains( x.cod.Trim( ) ) ) &&
                    Utente.IsTurnista( ) )
                {
                    SupportoFSmodel SFSmodel = WizardFsManager.GetSupportoFSmodel( Date , resp , matr );
                    return View( "~/Views/richieste/_wizardFS.cshtml" , SFSmodel );
                }
            }

            /* ----------------------------------------------------- */



            PannelloCarenzeResponse pann = GetPannelloCarenzeModel( Date , date , resp );
            if ( pann.model != null && forceProposte != true )
                return View( "~/Views/richieste/_intervalliTimbrature.cshtml" , pann.model );

            if ( pann.ProposteAutoAmmesse == false && forceProposte != true && Utente.GetQuadratura( ) == Quadratura.Giornaliera )
                return Content( "" );

            ProposteAutomaticheModel PAmodel = GetProposteAutoModel( Date , resp );
            if ( PAmodel == null )
                return Content( "" );
            else
            {
                PAmodel.ShowPannelloCarenzeButton = ( pann.model != null );
                if (Utente.TipoDipendente()=="G" &&  Utente.GiornalistaDelleReti() && PAmodel.EccezioniProposte != null 
                    && PAmodel.EccezioniProposte.Any())
                {
                    string eccGiornReti = myRaiCommonTasks.CommonTasks.GetParametro<string>(
                        myRaiCommonTasks.CommonTasks.EnumParametriSistema.GdelleRetiMotivoObbligatorio);
                    if (!String.IsNullOrWhiteSpace(eccGiornReti))
                    {
                        string[] eccMotivoObb = eccGiornReti.Split(',');
                        foreach (var item in PAmodel.EccezioniProposte)
                        {
                            if (eccMotivoObb.Contains(item.cod.Trim()))
                                item.CaratteriObbligatoriNota = 10;
                        }
                    }
                }
                return View( "~/Views/richieste/_proposteAutomatiche.cshtml" , PAmodel );
            }

        }

        public PannelloCarenzeResponse GetPannelloCarenzeModel ( DateTime Date , string date , dayResponse resp )
        {
            PannelloCarenzeResponse response = new PannelloCarenzeResponse( );
            response.ProposteAutoAmmesse = true;
            //Boolean ProposteAutomaticheAmmesse = true;
            if (resp==null || resp.eccezioni == null )
            {
                return response;
            }

            var car = resp.eccezioni.Where( x => x.cod != null && x.cod.Trim( ) == "CAR" ).FirstOrDefault( );
            if ( car != null && !String.IsNullOrWhiteSpace( car.qta ) && car.qta != "0" && car.qta != "00:00" )
            {
                int carGiornataMinuti = EccezioniManager.calcolaMinuti(car.qta);
                int franchigia = EccezioniManager.GetMinutiCarenzaPerSede(Utente.SedeGapp(Date), Date);

                if (carGiornataMinuti > franchigia)// && Utente.GetQuadratura() == Quadratura.Giornaliera)
                {
                    response.ProposteAutoAmmesse = false;

                    if (Utente.IsTester())
                    {
                        CarenzaTimbratureModel mo = new CarenzaTimbratureModel(CommonManager.GetCurrentUserMatricola(), CommonManager.GetCurrentUserMatricola(),
                                 Date, resp);
                        if ( mo.EccezioniPerCopertura != null && mo.EccezioniPerCopertura.Count > 0 )
                        {

                            //___________________________________________________________________________________________
                            TimbratureCore.CarenzaTimbrature frammenti =
                                                                        TimbratureCore.TimbratureManager.getCarenzaTimbrature( CommonManager.GetCurrentUserMatricola( ) ,
                                                                        resp , date , Utente.SedeGapp( Date ) );
                            //___________________________________________________________________________________________


                            int minutiCoperti = 0;
                            if ( frammenti != null )
                            {
                                var frammentiCoperti = frammenti.Intervalli.Where( a => a.CopertaDa ==
                                      CommonManager.GetParametro<string>( EnumParametriSistema.EccezioneFittiziaIgnora )
                                    ).ToList( );
                                if ( frammentiCoperti.Count > 0 )
                                    minutiCoperti = frammentiCoperti.Sum( x => x.MinutiTotali );
                            }

                            if ( frammenti == null || carGiornataMinuti - minutiCoperti < franchigia )
                            {
                                response.model = null;
                                return response;
                            }
                            else
                            {
                                mo.giornata = resp;
                                mo.carenze = frammenti;
                                mo.CarenzaGiornataMinuti = carGiornataMinuti;
                                if ( Utente.GetQuadratura( ) == Quadratura.Settimanale )
                                    mo.ShowProposteAutomaticheButton = EsistonoProposteAutomatiche( Date , resp );
                                response.model = mo;// View("~/Views/richieste/_intervalliTimbrature.cshtml", mo);
                                return response;
                            }
                        }
                    }
                }
            }
            return response;
        }

        public ProposteAutomaticheModel GetProposteAutoModel ( DateTime Date , dayResponse resp , Boolean IsFromAssistente = false )
        {

            ProposteAutomaticheModel model = new ProposteAutomaticheModel( );
            proposteResponse PAresp = ServiceWrapper.GetProposteAutomaticheWrapper(CommonManager.GetCurrentUserMatricola(), Date.ToString("ddMMyyyy"), resp);

            int poh = 0;
            int roh = 0;
            Utente.GetPOHandROH( false , Date , out poh , out roh );

            //string orarioUscitaProiezione = null;
            //if (resp.timbrature != null && resp.timbrature.Any())
            //{
            //    var t = resp.timbrature.First();
            //    var orausc= CommonManager.GetOrarioDiUscita(t.entrata.orario, resp.orario.cod_orario, Date.ToString("ddMMyyyy"), resp);
            //    if (orausc!=null)
            //        orarioUscitaProiezione = orausc.OrarioDiUscita;
            //}
            model.EccezioniProposte = TimbratureCore.TimbratureManager.GetProposteAutomatiche(
                CommonManager.GetCurrentUserMatricola( ) , Date , Utente.GetQuadratura( ) == Quadratura.Giornaliera ,
                     poh ,
                     roh ,
                     Utente.IsTester(),
                     CommonManager.GetParametro<String>( EnumParametriSistema.SoppressePOH ).Trim( ).ToUpper( ).Split( ',' ) ,
                     Utente.TipoDipendente( ) ,
                     PAresp,resp
                //,
                // orarioUscitaProiezione
                );



            RewriteLNH5( resp , model );
            EccezioniMinimoMetaTurno( resp , model );

            var db = new myRaiData.digiGappEntities( );
            foreach ( var e in model.EccezioniProposte )
            {
                if ( Utente.GestitoSirio( ) )
                {
                    var etr = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == e.cod ).FirstOrDefault( );
                    e.RichiedeAttivitaCeiton = etr != null &&
                        ( etr.RichiedeAttivitaCeiton || ( CeitonManager.ActivityAvailableToday( Date ) && EccezioniManager.IsEccezioneInGruppo2_3( e.cod ) ) );
                }
            }
            //***************** ??VB forse si può risparmiare una chiamata.


            model.MacroEccezioniProposte = ProntuarioManager.OpzioniProposte( resp );

            //if (IsFromAssistente && ! resp.eccezioni.Any (x=>x.cod.Trim()=="UMH"))
            //{
            //    Boolean UMHcandidato = TimbratureManager.ApplicabileUMH(CommonManager.GetCurrentUserMatricola());
            //    if (UMHcandidato)
            //    {
            //        bool QuadraturaGiornaliera = Utente.GetQuadratura() == Quadratura.Giornaliera;

            //        TimbratureCore.TimbratureInfo Ti = new TimbratureCore.TimbratureInfo(CommonManager.GetCurrentUserMatricola(), Date);

            //        List<Eccezione> Lumh = TimbratureCore.TimbratureManager.GetProposteAutoUMH(CommonManager.GetCurrentUserMatricola(), Date, Ti,
            //       rangeMensa,
            //       QuadraturaGiornaliera
            //       );
            //    }
            //}

            List<MyRai_Eccezioni_Ammesse> Epossibili = new List<MyRai_Eccezioni_Ammesse>();

            //PROPOSTE AUTO GIORNALISTI//////////////////////////////////////////
            List<Eccezione> propGiornalisti = new List<Eccezione>( );
            if ( Utente.TipoDipendente( ) == "G" &&
                    ( resp.eccezioni == null || ( !resp.eccezioni.Any( x => x.cod.StartsWith( "FS" ) ) && !resp.eccezioni.Any( x => x.cod.StartsWith( "SE" ) ) ) )
                )
            {
                Epossibili = EccezioniManager.GetListaEccezioniPossibili(
                 CommonManager.GetCurrentUserMatricola(), Date);

                propGiornalisti = TimbratureCore.TimbratureManager.GetProposteAutomaticheGiornalisti(
                                      CommonManager.GetCurrentUserMatricola( ) ,
                                      Date ,
                                      Epossibili, resp, Utente.GiornalistaDelleReti());
                if ( Epossibili.Any( x => x.cod_eccezione == "PDCO" ) )
                {
                    if ( TimbratureCore.TimbratureManager.ApplicabilePDCOoPDRA( CommonManager.GetCurrentUserMatricola( ) , "PDCO" ) )
                    {
                        MyRai_Eccezioni_Opzioni_Proposte ep = db.MyRai_Eccezioni_Opzioni_Proposte.Where( x => x.cod_eccezione == "PDCO" ).FirstOrDefault( );
                        if ( ep != null )
                        {
                            if ( model.MacroEccezioniProposte == null )
                                model.MacroEccezioniProposte = new List<OpzioneProposta>( );
                            OpzioneProposta op = new OpzioneProposta( ) { testo = ep.testo_opzione };
                            L2D_ECCEZIONE EccFrom = db.L2D_ECCEZIONE.Where( a => a.cod_eccezione == "PDCO" ).FirstOrDefault( );
                            Eccezione datiEccTo = new Eccezione( );
                            datiEccTo.cod = "PDCO";
                            datiEccTo.qta = "1";
                            datiEccTo.data = Date.ToString( "ddMMyyyy" );
                            datiEccTo.descrittiva_lunga = EccFrom.desc_eccezione;

                            CommonManager.Copy( EccFrom , datiEccTo );
                            var cobb = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == "PDCO" ).Select( x => x.CaratteriMotivoRichiesta ).FirstOrDefault( );
                            if ( cobb != null )
                                datiEccTo.CaratteriObbligatoriNota = ( int ) cobb;

                            op.eccezioniProposte = new List<Eccezione>( ) { datiEccTo };

                            model.MacroEccezioniProposte.Add( op );
                        }
                    }
                }
                if ( Epossibili.Any( x => x.cod_eccezione == "PDRA" ) )
                {
                    if ( TimbratureCore.TimbratureManager.ApplicabilePDCOoPDRA( CommonManager.GetCurrentUserMatricola( ) , "PDRA" ) )
                    {
                        MyRai_Eccezioni_Opzioni_Proposte ep = db.MyRai_Eccezioni_Opzioni_Proposte.Where( x => x.cod_eccezione == "PDRA" ).FirstOrDefault( );
                        if ( ep != null )
                        {
                            if ( model.MacroEccezioniProposte == null )
                                model.MacroEccezioniProposte = new List<OpzioneProposta>( );
                            OpzioneProposta op = new OpzioneProposta( ) { testo = ep.testo_opzione };
                            L2D_ECCEZIONE EccFrom = db.L2D_ECCEZIONE.Where( a => a.cod_eccezione == "PDRA" ).FirstOrDefault( );
                            Eccezione datiEccTo = new Eccezione( );
                            datiEccTo.cod = "PDRA";
                            datiEccTo.qta = "1";
                            datiEccTo.data = Date.ToString( "ddMMyyyy" );
                            datiEccTo.descrittiva_lunga = EccFrom.desc_eccezione;
                            CommonManager.Copy( EccFrom , datiEccTo );
                            var cobb = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == "PDRA" ).Select( x => x.CaratteriMotivoRichiesta ).FirstOrDefault( );
                            if ( cobb != null )
                                datiEccTo.CaratteriObbligatoriNota = ( int ) cobb;
                            op.eccezioniProposte = new List<Eccezione>( ) { datiEccTo };
                            model.MacroEccezioniProposte.Add( op );
                        }

                    }
                }
            }
            ////////////////////////////////////////////////////////////////////
            //********************

            if (Utente.TipoDipendente() == "G" && Epossibili.Any(x => x.cod_eccezione == "DG55") &&
                (model.EccezioniProposte == null || !model.EccezioniProposte.Any(x => x.cod.Trim() == "DG55")))
            {
                Eccezione eDG55 = new Eccezione()
                {
                    cod = "DG55",
                    qta = "1",
                    descrittiva_lunga = db.L2D_ECCEZIONE.Where(a => a.cod_eccezione == "DG55").Select(x => x.desc_eccezione).FirstOrDefault(),
                    data = Date.ToString("ddMMyyyy")
                };
                var cobb = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "DG55").Select(x => x.CaratteriMotivoRichiesta).FirstOrDefault();
                if (cobb != null)
                    eDG55.CaratteriObbligatoriNota = (int)cobb;

                if (model.EccezioniProposte == null)
                    model.EccezioniProposte = new Eccezione[] { eDG55 };
                else
                {
                    var Lecc = model.EccezioniProposte.ToList();
                    Lecc.Add(eDG55);
                    model.EccezioniProposte = Lecc.ToArray();
                }
            }

            if (model.EccezioniProposte == null || !model.EccezioniProposte.Any(x => x.cod.Trim() == "RMTR"))
            {
                Eccezione eRMTR = new Eccezione()
                {
                    cod = "RMTR",
                    qta = "1",
                    descrittiva_lunga = db.L2D_ECCEZIONE.Where(a => a.cod_eccezione == "RMTR").Select(x => x.desc_eccezione).FirstOrDefault(),
                    data = Date.ToString("ddMMyyyy")
                };
                var cobb = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "RMTR").Select(x => x.CaratteriMotivoRichiesta).FirstOrDefault();
                if (cobb != null)
                    eRMTR.CaratteriObbligatoriNota = (int)cobb;

                if (model.EccezioniProposte == null)
                    model.EccezioniProposte = new Eccezione[] { eRMTR };
                else
                {
                    var Lecc = model.EccezioniProposte.ToList();
                    Lecc.Add(eRMTR);
                    model.EccezioniProposte = Lecc.ToArray();
                }
            }

            if ( ( model.EccezioniProposte == null || model.EccezioniProposte.Length == 0 ) &&
                 ( propGiornalisti == null || propGiornalisti.Count == 0 ) &&
                 ( model.MacroEccezioniProposte == null || model.MacroEccezioniProposte.Count == 0 ) )
                return null;
            else
            {
                model.EccezioniProposte = EccezioniManager.VerificaProposteConMacinatore( model.EccezioniProposte , resp );
                if ( propGiornalisti != null && propGiornalisti.Count > 0 )
                {
                    var list = model.EccezioniProposte.ToList( );
                    list.AddRange( propGiornalisti.ToList( ) );
                    model.EccezioniProposte = list.ToArray( );
                }
                if ( ( model.EccezioniProposte == null || model.EccezioniProposte.Length == 0 ) &&
                ( model.MacroEccezioniProposte == null || model.MacroEccezioniProposte.Count == 0 ) )
                {
                    return null;
                }
                if ( model.EccezioniProposte != null && model.EccezioniProposte.Any( ) )
                {
                    var ammesse = EccezioniManager.GetListaEccezioniPossibili( CommonManager.GetCurrentUserMatricola( ) , Date , resp );
                    var list = model.EccezioniProposte.ToList( );
                    list.RemoveAll( x => ammesse.Select( a => a.cod_eccezione.Trim( ) ).Contains( x.cod.Trim( ) ) == false );
                    model.EccezioniProposte = list.ToArray( );
                }
                if (model.EccezioniProposte != null && model.EccezioniProposte.Any())
                {
                    foreach (var e in model.EccezioniProposte)
                    {
                        if (Utente.GestitoSirio())
                        {
                            var etr = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == e.cod).FirstOrDefault();
                            e.RichiedeAttivitaCeiton = etr != null &&
                                (etr.RichiedeAttivitaCeiton || (CeitonManager.ActivityAvailableToday(Date) && EccezioniManager.IsEccezioneInGruppo2_3(e.cod)));
                        }
                    }
                }


                if ( ( model.EccezioniProposte == null || model.EccezioniProposte.Length == 0 ) &&
                  ( model.MacroEccezioniProposte == null || model.MacroEccezioniProposte.Count == 0 ) )
                {
                    return null;
                }
                else
                return model;
            }
        }

        private void EccezioniMinimoMetaTurno ( dayResponse resp , ProposteAutomaticheModel model )
        {
            if ( resp.orario == null || String.IsNullOrWhiteSpace( resp.orario.prevista_presenza ) )
                return;
            if ( resp.eccezioni == null || resp.eccezioni.Count( ) == 0 )
                return;



            string[] EccezioniMinimoMetaTurno = CommonManager.GetParametri<string>( EnumParametriSistema.MinimoMetaTurnoSeMR_MF_MN );
            if ( String.IsNullOrWhiteSpace( EccezioniMinimoMetaTurno[0] ) || String.IsNullOrWhiteSpace( EccezioniMinimoMetaTurno[1] ) )
                return;
            String[] eccezioniNecessarie = EccezioniMinimoMetaTurno[0].Split( ',' );

            if ( !resp.eccezioni.Any( x => eccezioniNecessarie.Contains( x.cod.Trim( ) ) ) )
                return;

            string[] ecc = EccezioniMinimoMetaTurno[1].Split( ',' );

            IEnumerable<Eccezione> eccMetaTurno = model.EccezioniProposte.Where( x => ecc.Contains( x.cod ) );
            if ( !eccMetaTurno.Any( ) )
                return;

            int MetaTurno = Convert.ToInt32( resp.orario.prevista_presenza );
            foreach ( var e in eccMetaTurno )
            {
                if ( !String.IsNullOrWhiteSpace( e.qta ) && e.qta.ToMinutes( ) < MetaTurno )
                {
                    e.qta = MetaTurno.ToHHMM( );
                }
            }
            return;
        }

        private static void RewriteLNH5 ( dayResponse resp , ProposteAutomaticheModel model )
        {
            Eccezione LNH5 = model.EccezioniProposte.Where( x => x.cod == "LNH5" ).FirstOrDefault( );
            if ( LNH5 != null && resp.orario != null && TimbratureCore.TimbratureManager.TurnoMaggioreDiSogliaNotturno( resp.orario.hhmm_entrata_48 , resp.orario.hhmm_uscita_48 ) )
            {
                int quantita;
                if ( int.TryParse( resp.orario.prevista_presenza , out quantita ) )
                {
                    var car = resp.eccezioni.Where( x => x.cod.Trim( ) == "CAR" ).FirstOrDefault( );
                    if ( car != null )
                        quantita -= car.qta.ToMinutes( );

                    if ( resp.giornata.maggiorPresenza.ToMinutes( ) > 0 )
                    {
                        quantita += resp.giornata.maggiorPresenza.ToMinutes( );
                        int m = TimbratureCore.TimbratureManager.GetMinutiPrimaInizioTurno( resp );
                        if ( m > 0 )
                            quantita -= m;
                    }

                    if ( quantita > 600 )
                        quantita = 600;

                    LNH5.qta = quantita.ToHHMM( );
                    if ( resp.eccezioni.Any( x => x.cod != "LNH5" ) )
                    {
                        string EccezioniDaEliminare = myRaiCommonTasks.CommonTasks.GetParametro<string>( myRaiCommonTasks.CommonTasks.EnumParametriSistema.EliminaSeOltreSogliaNotturno );
                        if ( !String.IsNullOrWhiteSpace( EccezioniDaEliminare ) )
                        {
                            var L = model.EccezioniProposte.ToList( );
                            L.RemoveAll( x => EccezioniDaEliminare.Split( ',' ).Contains( x.cod ) );
                            model.EccezioniProposte = L.ToArray( );
                        }
                    }
                }
            }
        }

        public ActionResult GetRaggruppamentiDisponibili ( string date , string sessionId = null )
        {
            DateTime Date;
            DateTime.TryParseExact( date , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out Date );

            var db = new digiGappEntities( );
            dayResponse d = null;
            if ( !String.IsNullOrWhiteSpace( sessionId ) && Session[sessionId] != null )
                d = ( dayResponse ) Session[sessionId];

            var ListEccezioniAmmesse = EccezioniManager.GetListaEccezioniPossibili( CommonManager.GetCurrentUserMatricola7chars( ) , Date, d );
            if ( Date < DateTime.Now )
            {
                TimeSpan ts = DateTime.Now - Date;
                ListEccezioniAmmesse = ListEccezioniAmmesse.Where( x => x.OreInPassato > ts.TotalHours ).ToList( );
            }
            else
            {
                TimeSpan ts = Date - DateTime.Now;
                ListEccezioniAmmesse = ListEccezioniAmmesse.Where( x => x.OreInFuturo > ts.TotalHours ).ToList( );
            }

            //pianoFerie resp = service.getPianoFerie(CommonManager.GetCurrentUserMatricola7chars(), DateTime.Now.ToString("ddMMyyyy"), 70);
            var query = ListEccezioniAmmesse.Select( x => x.id_raggruppamento ).Distinct( ).ToArray( );
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = query }
            };
        }

        public ActionResult GetJSvalidazione ( string cod )
        {
            var db = new digiGappEntities( );
            var ecc = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == cod ).FirstOrDefault( );
            if ( ecc == null )
                return null;
            else
            {

                return Content( ecc.FunzioneJS );
            }
        }

        public ActionResult GetTimbratureSW_Evidenze()
        {
            List<DataID> LD = new List<DataID>();

            var evidenze = SessionManager.Get(SessionVariables.ListaEvidenzeScrivania);
            if (evidenze != null)
            {
                var sessionData = (SessionListaEvidenzeModel)evidenze;
                var date = sessionData.ListaEvidenze.data.giornate.Where(x => x.TipoEcc == TipoEccezione.TimbratureInSW)
                    .Select(x => x.data).ToList();

                if (date.Any())
                {
                    var db = new digiGappEntities();
                    foreach (var d in date)
                    {
                        DataID D = new DataID();
                        D.data = d.ToString("dd/MM/yyyy");
                        string matr = CommonManager.GetCurrentUserMatricola();
                        var rich = db.MyRai_Richieste.Where(x => x.matricola_richiesta == matr &&
                        x.MyRai_Eccezioni_Richieste.Any(z =>
                                                       z.data_eccezione == d &&
                                                       z.cod_eccezione == "SW")).FirstOrDefault();

                        //var rich = db.MyRai_Richieste.Where(x => x.MyRai_Eccezioni_Richieste.Any(z => 
                        //z.data_eccezione == d &&  z.cod_eccezione == "SW")).FirstOrDefault();
                        if (rich != null)
                        {
                            D.id = rich.id_richiesta;
                        }
                        LD.Add(D);
                    }
                }
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = LD.ToArray() }
            };
        }
        public ActionResult GetAssenzeIngiustificate ( )
        {
            List<DateTime> GiorniAssenze = Utente.GiornateAssenteIngiustificato(CommonManager.GetCurrentUserMatricola(), true);
            List<DateTime> GiorniCarenze = Utente.GiornateConCarenza( );
            if ( GiorniCarenze != null )
                GiorniAssenze.AddRange( GiorniCarenze );


            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = GiorniAssenze.Select( x => x.Date.ToString( "dd/MM/yyyy" ) ) }
            };
        }
        public ActionResult GetMaggiorPresenza ( )
        {
            List<DateTime> GiorniMaggPres = Utente.GiornateConMaggiorPresenza( );

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = GiorniMaggPres.Select( x => x.Date.ToString( "dd/MM/yyyy" ) ) }
            };
        }

        // Restituisce true/false se assente o no in quella data                                       
        public ActionResult IsAssenteIngiustificato ( string date )
        {
            var GiorniAssenze = Utente.GiornateAssenteIngiustificato(CommonManager.GetCurrentUserMatricola());
            DateTime D = CommonManager.ConvertToDate( date );
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = GiorniAssenze.Contains( D ) }
            };
        }

        //Restituisce prossima data con assenza ingiustificata o null se non ce ne sono
        public ActionResult NextAssenteIngiustificato ( string date )
        {
            DateTime D = CommonManager.ConvertToDate( date );
            DateTime? nextAssente = Utente.GiornateAssenteIngiustificato(CommonManager.GetCurrentUserMatricola()).Where(x => x > D).OrderBy(x => x).FirstOrDefault();
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = ( nextAssente == default( DateTime ) ? null : ( ( DateTime ) nextAssente ).ToString( "ddMMyyyy" ) ) }
            };
        }

        //Restituisce precedente data con assenza ingiustificata o null se non ce ne sono
        public ActionResult PreviousAssenteIngiustificato ( string date )
        {
            DateTime D = CommonManager.ConvertToDate( date );
            DateTime? previousAssente = Utente.GiornateAssenteIngiustificato(CommonManager.GetCurrentUserMatricola()).Where(x => x < D).OrderBy(x => x).FirstOrDefault();
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = ( previousAssente == default( DateTime ) ? null : ( ( DateTime ) previousAssente ).ToString( "ddMMyyyy" ) ) }
            };
        }

        //  url: "ajax/getPartial?nomePartial="+partial,
        public ActionResult GetPartial ( string nomePartial )
        {
            try
            {
                return View( "~/Views/ExtraParams/" + nomePartial );
            }
            catch ( Exception ex )
            {
                return Content( "" );
            }
        }

        public ActionResult getPartialAttivitaCeiton ( string cod , string data )
        {
            DateTime D;
            bool esito = DateTime.TryParseExact( data , "dd/MM/yyyy" , null , DateTimeStyles.None , out D );
            WeekPlan model = myRaiHelper.Sirio.Helper.GetWeekPlanCached(
                CommonManager.GetCurrentUserPMatricola( ) ,
                CommonManager.GetCurrentUserMatricola( ) ,
                D );

            model.Cod_Eccezione = cod;
            return View( "~/Views/ExtraParams/attivitaCeiton.cshtml" , model );

        }

        public ActionResult RicercaGenerale ( string nominativo , string sede , string stato , string dal , string al )
        {
            if ( string.IsNullOrEmpty( stato ) )
                stato = "0";
            int intstato = Convert.ToInt32( stato );

            ModelDash model = new ModelDash( );
            model.elencoProfilieSedi = new daApprovareModel( CommonManager.GetCurrentUserPMatricola( ) , "01" , nominativo , sede , intstato , dal , al );
            return View( "~/Views/Responsabile/_ListaRicercaEccezioni.cshtml" , model );
        }

        public ActionResult RicercaGeneraleUtente ( string stato , string tipoEcc , string dal , string al )
        {
            if ( string.IsNullOrEmpty( stato ) )
                stato = "0";
            int intstato = Convert.ToInt32( stato );

            ModelDash model = new ModelDash( );
            model.elencoProfilieSedi = new daApprovareModel( CommonManager.GetCurrentUserPMatricola( ) , CommonManager.GetCurrentUserMatricola( ) , "01" , intstato , tipoEcc , dal , al );
            return View( "~/Views/MieRichieste/_ListaRicercaEccezioniUtente.cshtml" , model );
        }

        public ActionResult GetGiornataChiusaView ( )
        {
            return View( "~/Views/Richieste/giornataChiusa.cshtml" );
        }

        public ActionResult GetGiornataChiusaOrarioView ( )
        {
            return View( "~/Views/Richieste/giornataChiusaOrario.cshtml" );
        }

        [HttpPost]
        public ActionResult Nascondi ( string data )
        {
            DateTime d;
            DateTime.TryParseExact( data , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out d );
            string matr = CommonManager.GetCurrentUserMatricola( );
            var db = new digiGappEntities( );
            var gq = db.MyRai_GiornateQuadrate.Where( x => x.matricola == matr && x.data == d ).FirstOrDefault( );
            if ( gq != null )
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "OK" }
                };

            else
            {
                try
                {
                    db.MyRai_GiornateQuadrate.Add( new MyRai_GiornateQuadrate( )
                    {
                        data = d ,
                        data_inserita = DateTime.Now ,
                        matricola = matr
                    } );
                    db.SaveChanges( );
                }
                catch ( Exception ex )
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                        Data = new { result = ex.Message }
                    };
                }
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "OK" }
                };
            }

        }

        public ActionResult getmenuCV ( string testo )
        {
            List<MenuModel> model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MenuModel>>( testo );
            return View( "~/Views/cv_online/partials/_cv_menu.cshtml" , model );
            //return null;
        }


        /// <summary>
        /// Inserimento di una nota per la segreteria
        /// </summary>
        /// <param name="data"></param>
        /// <param name="nota"></param>
        /// <returns></returns>
        public ActionResult AggiungiNotaEccezione ( DateTime data , string nota )
        {
            string result = "";
            try
            {
                // reperimento della matricola
                string matricola = CommonManager.GetCurrentUserMatricola( );

                string nomeUtente = CommonManager.GetNominativoPerMatricolaCognomeNome(matricola);

                string sede = Utente.SedeGapp( data );

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    var exists = db.MyRai_Note_Richieste.Where( w => w.Mittente.Equals( matricola ) &&
                                                    EntityFunctions.TruncateTime( w.DataGiornata ) == EntityFunctions.TruncateTime( data ) ).ToList( );

                    // esiste già una nota per quella data
                    if ( exists != null && exists.Any( ) )
                    {
                        // la nota non è stata ancora letta
                        if ( !exists.LastOrDefault( ).DataLettura.HasValue )
                        {
                            throw new Exception( "Impossibile inserire una nuova nota finchè non verrà letta la precedente" );
                        }
                    }
                }

                var opt = _noteRichiesteDataController.InserisciNotaRichiesta( matricola , nomeUtente , nota , data , sede );

                if ( opt != null && opt.Id > 0 )
                {
                    result = "OK";
                    DispositiviManager.SendPushMessaggioSegreteria(Utente.SedeGapp());
                }
                else
                {
                    throw new Exception( "Si è verificato un errore nell'inserimento della nota." );
                }
            }
            catch ( Exception ex )
            {
                result = ex.Message;
            }
            return Content( result );
        }

        /// <summary>
        /// Metodo per la modifica di una nota
        /// </summary>
        /// <param name="idNota"></param>
        /// <param name="nota"></param>
        /// <returns></returns>
        public ActionResult ModificaNotaRichiesta ( int idNota , string nota )
        {
            string result = "";
            try
            {
                // reperimento della matricola
                string matricola = CommonManager.GetCurrentUserMatricola( );

                var opt = _noteRichiesteDataController.ModificaNotaRichiesta( idNota , matricola , nota );

                if ( opt != null && opt.Id > 0 )
                {
                    result = "OK";
                }
                else
                {
                    throw new Exception( "Si è verificato un errore. Impossibile modificare la nota." );
                }
            }
            catch ( Exception ex )
            {
                result = "Si è verificato un errore. Impossibile modificare la nota.";
            }
            return Content( result );
        }

        /// <summary>
        /// Metodo per la cancellazione di una nota
        /// </summary>
        /// <param name="idNota"></param>
        /// <returns></returns>
        public ActionResult EliminaNotaRichiesta ( int idNota )
        {
            string result = "";
            try
            {
                result = "OK";

                // reperimento della matricola
                string matricola = CommonManager.GetCurrentUserMatricola( );

                _noteRichiesteDataController.EliminaNotaRichiesta( idNota , matricola );
            }
            catch ( Exception ex )
            {
                result = "Si è verificato un errore. Impossibile eliminare la nota.";
            }
            return Content( result );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ActionResult GetNoteRichiesta ( DateTime data )
        {
            List<MyRai_Note_Richieste_EXT> note = new List<MyRai_Note_Richieste_EXT>( );
            try
            {
                // reperimento della matricola
                string matricola = CommonManager.GetCurrentUserMatricola( );

                var tempNote = this._noteRichiesteDataController.GetNoteRichieste( matricola , data );

                if ( tempNote != null && tempNote.Any( ) )
                {
                    foreach ( var n in tempNote )
                    {
                        string img = CommonManager.GetImmagineBase64( n.Mittente );
                        MyRai_Note_Richieste_EXT nota = new MyRai_Note_Richieste_EXT( );

                        nota.Id = n.Id;
                        nota.Mittente = n.Mittente;
                        nota.Destinatario = n.Destinatario;
                        nota.Visualizzatore = n.Visualizzatore;
                        nota.DescrizioneVisualizzatore = n.DescrizioneVisualizzatore;
                        nota.Messaggio = n.Messaggio;
                        nota.DataCreazione = n.DataCreazione;
                        nota.DataUltimaModifica = n.DataUltimaModifica;
                        nota.DataLettura = n.DataLettura;
                        nota.DataGiornata = n.DataGiornata;
                        nota.DescrizioneMittente = n.DescrizioneMittente;
                        nota.SedeGapp = n.SedeGapp;
                        nota.Immagine = img;

                        note.Add( nota );
                    }
                }
            }
            catch ( Exception ex )
            {
                note = new List<MyRai_Note_Richieste_EXT>( );
            }
            InfoGiornataModel model = new InfoGiornataModel( );

            model.Note = note;
            model.MatricolaUtente = CommonManager.GetCurrentUserMatricola( );
            return View( "~/Views/Home/subpartial/_noteGiornata.cshtml" , model );
        }

        [HttpGet]
        public ActionResult getProssimeTrasferte ( string date , string matricola )
        {
            DateTime _data = DateTime.Now;

            if ( !String.IsNullOrEmpty( matricola ) )
            {
                if ( matricola.Length > 6 )
                {
                    matricola = matricola.Substring( 1 , 6 );
                }
            }

            List<DettaglioTrasfertaVM> model = TrasferteManager.GetTrasferte( _data , matricola );

            return View( "~/Views/Home/subpartial/_prossimeTrasferte.cshtml" , model );
        }

        [HttpGet]
        public ActionResult getProssimaTrasferta ( string date , string matricola )
        {
            DateTime _data = DateTime.Now;

            if ( !String.IsNullOrEmpty( matricola ) )
            {
                if ( matricola.Length > 6 )
                {
                    matricola = matricola.Substring( 1 , 6 );
                }
            }

            List<DettaglioTrasfertaVM> model = TrasferteManager.GetTrasferte( _data , matricola );

            string result = "";
            string title = "";
            string prossimaTrasferta = "";

            if ( model != null &&
                model.Any( ) )
            {
                var primo = model.OrderBy( w => w.FoglioViaggio.DATA_PARTENZA ).FirstOrDefault( );
                prossimaTrasferta = String.Format( "dal {0} al {1} " , primo.FoglioViaggio.DATA_PARTENZA.ToString( "dd MMMM yyyy" ) , primo.FoglioViaggio.DATA_ARRIVO.ToString( "dd MMMM yyyy" ) );

                if ( !String.IsNullOrEmpty( primo.FoglioViaggio.SCOPO ) )
                {
                    prossimaTrasferta += "<br>";
                    prossimaTrasferta += String.Format( "{0}" , primo.FoglioViaggio.SCOPO );
                }

                if ( primo.Itinerario != null && primo.Itinerario.Any( ) )
                {
                    var first = primo.Itinerario.FirstOrDefault( );
                    if ( first != null )
                    {
                        title += String.Format( "Data ora partenza {0}" , first.DataArrivoFull.Value.ToString( "dd MMMM yyyy HH:mm:ss" ) );
                        title += "<br>";
                        title += String.Format( "Destinazione {0} {1}" , first.DestCitta , first.DESTINAZIONE );
                        title += "<br>";
                    }

                    var last = primo.Itinerario.LastOrDefault( );
                    if ( last != first )
                    {
                        title += String.Format( "Data ora rientro {0}" , last.DataArrivoFull.Value.ToString( "dd MMMM yyyy HH:mm:ss" ) );
                        title += "<br>";
                        title += String.Format( "Destinazione {0} {1}" , last.DestCitta , last.DESTINAZIONE );
                        title += "<br>";
                    }
                }
            }

            result = "<div><b style =\"font-size: 11px;\" data-toggle=\"tooltip\" data-html=\"true\" title=\"" + title + "\"> " + prossimaTrasferta + "</b></div>";

            return Content( result );
        }

        [HttpGet]
        public ActionResult GetTransitiSfasatiView ( string giorno )
        {
            WSDigigapp digigappWS = new WSDigigapp( );

            string matricola = CommonManager.GetCurrentUserMatricola( ).PadLeft( 8 , '0' );

            //matricola = "00006650";
            //giorno = "14012019";

            var response = digigappWS.GetTimbratureOspiti( matricola , matricola , giorno );
            List<TimOspitiElement> model = new List<TimOspitiElement>( );

            if ( response.Esito )
            {
                if ( response.Risultato != null && response.Risultato.Any( ) )
                {
                    model = response.Risultato.ToList( );
                }
            }
            return View( "~/Views/Home/subpartial/_modalTransitiSfasati.cshtml" , model );
        }

        [HttpPost]
        public ActionResult salvaordine ( string seq )
        {
            var db = new digiGappEntities( );
            try
            {
                string[] parti = seq.Split( ',' );
                foreach ( string parte in parti )
                {
                    if ( !string.IsNullOrWhiteSpace( parte ) )
                    {
                        int id = 0;
                        if ( !int.TryParse( parte.Split( ':' )[0] , out id ) )
                        {
                            return Content( "Si è verificato un errore imprevisto nei dati" );
                        }

                        int prog = 0;
                        if ( !int.TryParse( parte.Split( ':' )[1] , out prog ) )
                        {
                            return Content( "Valore non corretto nel campo Progressivo" );
                        }

                        var allegato = db.MyRai_Regole_Allegati.Where( x => x.id == id ).FirstOrDefault( );
                        if ( allegato != null )
                            allegato.progressivo = prog;
                    }
                }
                db.SaveChanges( );
                return Content( "OK" );
            }
            catch ( Exception ex )
            {
                return Content( ex.Message );
            }
        }

        [HttpPost]
        public ActionResult IsSedeTorino ( )
        {
            bool result = false;
            Abilitazioni ab = CommonManager.getAbilitazioni( );
            var lista = ab.ListaAbilitazioni
                    .Where( x => x.MatrLivello3 != null && x.MatrLivello3.Any( ) )
                    .Select( a => a.Sede ).ToList( );

            if ( lista != null && lista.Any( ) )
            {
                result = lista.Contains( Utente.SedeGapp( ).Substring( 0 , 5 ) );
            }

            if (result)
            {
                string sede = Utente.SedeGapp( ).Substring( 0 , 5 );
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    var esiste = db.MyRai_ApprovatoriProduzioni.Count( w => w.SedeGapp.Equals( sede ) ) > 0;

                    if (esiste)
                    {
                        // deve essere abilitata la selezione dell'approvatore
                        result = false;
                    }
                }
            }
            return Json( result , JsonRequestBehavior.AllowGet );
        }

        [HttpPost]
        public ActionResult GetElencoApprovatoriPerAttivita ( int idAttivita , string dataRichiesta = null, string titolo = null )
        {
            string matricolaUtente = CommonManager.GetCurrentUserMatricola( );
            string sedeGapp = Utente.SedeGapp( );
            List<ApprovatoreAttivita> elenco = new List<ApprovatoreAttivita>( );
            string approvatore = "";
            List<MyRai_ApprovatoriProduzioni> lista = new List<MyRai_ApprovatoriProduzioni>( );
            DateTime Date;
            Boolean datavalida = DateTime.TryParseExact( dataRichiesta , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out Date );

            if (datavalida)
            {
                try
                {
                    WSDigigapp service = new WSDigigapp( )
                    {
                        Credentials = new System.Net.NetworkCredential(
                  CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] ,
                  CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] )
                    };

                    dayResponse resp = service.getEccezioni( CommonManager.GetCurrentUserMatricola( ) , dataRichiesta.Replace( "/" , "" ) , "BU" , 70 );

                    if ( resp.esito )
                    {
                        if ( resp.giornata != null )
                        {
                            string mySede = resp.giornata.sedeGapp.Trim( );
                            if ( mySede.Length > 5 )
                            {
                                mySede = mySede.Substring( 0 , 5 );
                            }
                            sedeGapp = mySede;
                        }
                    }
                }
                catch(Exception ex)
                {
                    sedeGapp = Utente.SedeGapp( );
                }
            }

            using ( digiGappEntities db = new digiGappEntities( ) )
            {
                bool par = CommonManager.GetParametro<bool>( EnumParametriSistema.ModalitaTabellaApprovatoriProduzioneAttivo );

                if ( par )
                {
                    string sIdAttivita = idAttivita.ToString( );
                    // prendo il titolo dell'attività
                    var att = db.MyRai_AttivitaCeiton.Where( w => w.idCeiton.Equals( sIdAttivita ) ).FirstOrDefault( );

                    if ( att == null )
                    {
                        // se abbiamo il titolo filtra per titolo
                        if (!String.IsNullOrEmpty( titolo ) )
                        {
                            // scarta gli uffici di produzione
                            lista = ( from app in db.MyRai_ApprovatoriProduzioni
                                      join att2 in db.MyRai_AttivitaApprovatori
                                      on app.MatricolaApprovatore equals att2.Matricola
                                      where !app.MatricolaApprovatore.Equals( matricolaUtente )
                                      && att2.Titolo == titolo
                                      && att2.Data == Date
                                      && !app.Fixed
                                      && app.SedeGapp.Equals( sedeGapp )
                                      select app ).Distinct( ).ToList( );
                        }
                        else
                        {
                            // altrimenti prende tutti tranne gli uffici di produzione
                            lista = db.MyRai_ApprovatoriProduzioni.Where( w => w.SedeGapp.Equals( sedeGapp ) && !w.MatricolaApprovatore.Equals( matricolaUtente ) && !w.Fixed ).Distinct( ).ToList( );
                        }                        
                    }
                    else
                    {
                        // scarta gli uffici di produzione
                        lista = ( from app in db.MyRai_ApprovatoriProduzioni
                                  join att2 in db.MyRai_AttivitaApprovatori
                                  on app.MatricolaApprovatore equals att2.Matricola
                                  where !app.MatricolaApprovatore.Equals( matricolaUtente )
                                  && att2.Titolo == att.Titolo
                                  && att2.Data == Date
                                  && !app.Fixed
                                  && app.SedeGapp.Equals( sedeGapp )
                                  select app ).Distinct( ).ToList( );
                    }

                    // se non trova nemmeno un approvatore allora li prende tutto
                    // questo perchè può capitare che una attività non è stata pianificata
                    if ( lista == null || !lista.Any( ) )
                    {
                        lista = db.MyRai_ApprovatoriProduzioni.Where( w => w.SedeGapp.Equals( sedeGapp ) && !w.MatricolaApprovatore.Equals( matricolaUtente ) ).Distinct( ).ToList( );
                    }
                    else
                    {
                        // Se la lista è valorizzata allora la deve completare aggiungendo gli uffici di produzione
                        lista.AddRange( db.MyRai_ApprovatoriProduzioni.Where( w => w.SedeGapp.Equals( sedeGapp ) && w.Fixed ).Distinct( ).ToList( ) );
                    }
                }
                else
                {
                    // vecchia modalità
                    // visualizza tutti gli approvatori
                    lista = db.MyRai_ApprovatoriProduzioni.Where( w => w.SedeGapp.Equals( sedeGapp ) && !w.MatricolaApprovatore.Equals( matricolaUtente ) ).Distinct( ).ToList( );
                }

                if ( lista != null && lista.Any( ) )
                {
                    lista = lista.OrderBy( w => w.Nominativo ).ToList( );
                    foreach ( var e in lista )
                    {
                        elenco.Add( new ApprovatoreAttivita( )
                        {
                            Matricola = e.MatricolaApprovatore ,
                            Nominativo = e.Nominativo ,
                            Selected = false
                        } );
                    }
                }

                var ric = db.MyRai_Richieste.Where( w => w.matricola_richiesta.Equals( matricolaUtente ) &&
                                ( w.periodo_dal <= Date && Date <= w.periodo_al ) &&
                                    w.id_stato == 10 &&
                                    w.id_Attivita_ceiton != null &&
                                    w.id_Attivita_ceiton.Value == idAttivita ).OrderByDescending( w => w.id_richiesta ).FirstOrDefault( );

                if ( ric != null )
                {
                    approvatore = ric.ApprovatoreSelezionato;
                    if ( !String.IsNullOrEmpty( approvatore ) )
                    {
                        try
                        {
                            elenco.Where( w => w.Matricola.Equals( approvatore ) ).ToList( ).ForEach( w =>
                            {
                                w.Selected = true;
                            } );
                        }
                        catch ( Exception ex )
                        {

                        }
                    }
                }

                if ( elenco != null && elenco.Any( ) )
                {
                    elenco = elenco.OrderBy( w => w.Nominativo ).ToList( );
                }
            }
            return Json( elenco , JsonRequestBehavior.AllowGet );
        }



        [HttpGet]
        public ActionResult GetElencoApprovatori()
        {
            string matricolaUtente = CommonManager.GetCurrentUserMatricola();
            string sedeGapp = Utente.SedeGapp();
            List<ApprovatoreAttivita> elenco = new List<ApprovatoreAttivita>();
            List<MyRai_ApprovatoriProduzioni> lista = new List<MyRai_ApprovatoriProduzioni>();

            using (digiGappEntities db = new digiGappEntities())
            {
                lista = db.MyRai_ApprovatoriProduzioni.Where(w => !w.MatricolaApprovatore.Equals(matricolaUtente)).DistinctBy(w => w.Nominativo).ToList().OrderBy(w => w.Nominativo).ThenBy(w => w.Fixed).ToList();

                if (lista != null && lista.Any())
                {
                    lista = lista.OrderBy(w => w.Nominativo).ToList();
                    foreach (var e in lista)
                    {
                        elenco.Add(new ApprovatoreAttivita()
                        {
                            Matricola = e.MatricolaApprovatore,
                            Nominativo = e.Nominativo,
                            Selected = false
                        });
                    }
                }
            }
            return Json(elenco, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SmistaRichiesta(int idRichiestaEccezione, string nuovoResponsabile)
        {
            bool esito = false;
            string vecchioResponsabile = "";

            try
            {

                using (digiGappEntities db = new digiGappEntities())
                {
                    var item = db.MyRai_Eccezioni_Richieste.Where(w => w.id_eccezioni_richieste.Equals(idRichiestaEccezione)).FirstOrDefault();

                    if (item != null)
                    {
                        var richiesta = db.MyRai_Richieste.Where(w => w.id_richiesta.Equals(item.id_richiesta)).FirstOrDefault();
                        if (richiesta != null)
                        {
                            vecchioResponsabile = richiesta.ApprovatoreSelezionato;
                            richiesta.ApprovatoreSelezionato = nuovoResponsabile;
                            db.SaveChanges();
                            esito = true;

                            string descrizioneVecchioApprovatore = "";
                            string descrizioneNuovoApprovatore = "";

                            if (!String.IsNullOrEmpty(vecchioResponsabile))
                            {
                                var appOld = db.MyRai_ApprovatoriProduzioni.Where(w => w.MatricolaApprovatore.Equals(vecchioResponsabile)).FirstOrDefault();

                                descrizioneVecchioApprovatore = appOld.Nominativo;
                            }

                            var appNew = db.MyRai_ApprovatoriProduzioni.Where(w => w.MatricolaApprovatore.Equals(nuovoResponsabile)).FirstOrDefault();
                            descrizioneNuovoApprovatore = appNew.Nominativo;

                            InvioMail(richiesta.matricola_richiesta, descrizioneVecchioApprovatore, descrizioneNuovoApprovatore, richiesta.data_richiesta, richiesta.periodo_dal, richiesta.periodo_al, item.cod_eccezione);
                        }
                        else
                        {
                            throw new Exception("Impossibile trovare la richiesta da smistare");
                        }
                    }
                    else
                    {
                        throw new Exception("Impossibile trovare la richiesta da smistare");
                    }
                }

                using (digiGappEntities db = new digiGappEntities())
                {
                    MyRai_LogAzioni azione = new MyRai_LogAzioni()
                    {
                        applicativo = "PORTALE",
                        data = DateTime.Now,
                        descrizione_operazione = String.Format("Parametri: idRichiestaEccezione = {0}, nuovoResponsabile = {1}, vecchioResponsabile = {2}.", idRichiestaEccezione, nuovoResponsabile, vecchioResponsabile),
                        operazione = "SmistaRichiesta",
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        provenienza = "ajax/SmistaRichiesta"
                    };
                    try
                    {
                        db.MyRai_LogAzioni.Add(azione);
                        db.SaveChanges();
                    }
                    catch (Exception ex2)
                    { }
                }

            }
            catch (Exception ex)
            {
                esito = false;
                using (digiGappEntities db = new digiGappEntities())
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "PORTALE",
                        data = DateTime.Now,
                        error_message = String.Format("Parametri: idRichiestaEccezione = {0}, nuovoResponsabile = {1}. Errore: {2}", idRichiestaEccezione, nuovoResponsabile, ex.ToString()),
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        provenienza = "SmistaRichiesta"
                    };
                    try
                    {
                        db.MyRai_LogErrori.Add(err);
                        db.SaveChanges();
                    }
                    catch (Exception ex2)
                    { }
                }
            }

            return Json(esito, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrDestinatario"></param>
        /// <param name="utenteApprovatoreOLD"></param>
        /// <param name="utenteApprovatoreNEW"></param>
        /// <param name="dataRichiesta"></param>
        /// <param name="periodoDal"></param>
        /// <param name="periodoAL"></param>
        /// <param name="codiceEccezione"></param>
        private static void InvioMail(string matrDestinatario, string utenteApprovatoreOLD, string utenteApprovatoreNEW, DateTime dataRichiesta, DateTime periodoDal, DateTime periodoAL, string codiceEccezione)
        {
            try
            {
                string[] MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailNotificaSmistamentoRichiesta);

                string[] MailBody = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateNotificaSmistamentoRichiesta);

                string body = "";
                string MailSubject = MailParams[0];
                string from = MailParams[1];
                string dest = myRaiCommonTasks.CommonTasks.GetEmailPerMatricola(matrDestinatario.TrimStart('P'));

                if (periodoDal.Equals(periodoAL))
                {
                    // se periodoDAL e periodoAL sono uguali:
                    // la tua richiesta del #DATA# di #ECCEZIONE# per il #PERIODODAL# è stata assegnata ad un nuovo responsabile. #RESPONSABILE# si occuperà di elaborare tale richiesta.

                    body = MailBody[0];
                }
                else
                {
                    body = MailBody[1];
                    // se periodoDAL e periodoAL non sono uguali:
                    // la tua richiesta del #DATA# di #ECCEZIONE# per il periodo #PERIODODAL# - #PERIODOAL# è stata assegnata ad un nuovo responsabile. #RESPONSABILE# si occuperà di elaborare tale richiesta.
                }

                body = body.Replace("#ECCEZIONE#", codiceEccezione);
                body = body.Replace("#DATA#", dataRichiesta.ToString("dd/MM/yyyy"));
                body = body.Replace("#PERIODODAL#", periodoDal.ToString("dd/MM/yyyy"));
                body = body.Replace("#PERIODOAL#", periodoAL.ToString("dd/MM/yyyy"));
                body = body.Replace("#RESPONSABILE#", utenteApprovatoreNEW);

                myRaiCommonTasks.GestoreMail mail = new myRaiCommonTasks.GestoreMail();

                var response = mail.InvioMail(body, MailSubject, dest, "raiplace.selfservice@rai.it", from, null, null);

                if (response != null && !String.IsNullOrEmpty(response.Errore))
                {
                    myRaiCommonDatacontrollers.Logger.LogErrori(new MyRai_LogErrori()
                    {
                        applicativo = "Portale",
                        data = DateTime.Now,
                        provenienza = "AjaxController - InvioMail",
                        error_message = response.Errore + " per " + dest,
                        feedback = null,
                        matricola = matrDestinatario
                    });
                }
            }
            catch (Exception ex)
            {
                myRaiCommonDatacontrollers.Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    data = DateTime.Now,
                    provenienza = "AjaxController - InvioMail",
                    error_message = ex.ToString(),
                    feedback = null,
                    matricola = matrDestinatario
                });
            }
        }
    }

    public class PannelloCarenzeResponse
    {
        public Boolean ProposteAutoAmmesse { get; set; }
        public CarenzaTimbratureModel model { get; set; }
    }

    public class ApprovatoreAttivita
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public bool Selected { get; set; }
    }
    public class DataID
    {
        public string data { get; set; }
        public int id { get; set; }
    }
}