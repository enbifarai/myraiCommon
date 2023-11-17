using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRai.DataAccess;
using myRaiCommonModel;
using myRaiHelper;

namespace myRai.Controllers
{
    public enum EnumTipologiaDomanda
    {
        Si_No = 1,
        Risposta_singola_Lista_RadioButton = 2,
        Risposta_Singola_Lista_Tendina = 3,
        Risposta_Multipla_CheckBox = 4,
        ShortText = 5,
        LongText = 6,
        Precompilato = 7,
        MasterPerMatrixRating = 8,
        SlavePerMatrixRating = 9,
        SlavePerRating_6 = 10

    }

    public class FormUserController : BaseCommonController
    {
        public ActionResult Index ( )
        {
            string matricola = CommonHelper.GetCurrentUserMatricola();
            DateTime today = DateTime.Now;

            myRaiData.digiGappEntities db = new myRaiData.digiGappEntities();
            List<myRaiData.MyRai_FormPrimario> formPrimari = new List<myRaiData.MyRai_FormPrimario>();

            formPrimari.AddRange(db.MyRai_FormPrimario.Where(x => x.attivo && x.data_inizio_validita <= today && today < x.data_fine_validita && (x.filtro_matricola == null || x.filtro_matricola.Contains(matricola)) && !x.MyRai_FormCompletati.Any(y=>y.matricola==matricola)));
            
            return View(formPrimari.Where(x => x.data_fine_validita.Subtract(today).TotalDays < 5));
        }

        public ActionResult ElencoQuestionari()
        {
            string matricola = CommonHelper.GetCurrentUserMatricola();
            DateTime today = DateTime.Now;

            myRaiData.digiGappEntities db = new myRaiData.digiGappEntities();
            List<myRaiData.MyRai_FormPrimario> formPrimari = new List<myRaiData.MyRai_FormPrimario>();

            formPrimari.AddRange(db.MyRai_FormPrimario.Where(x => x.attivo && x.data_inizio_validita<=today && today<x.data_fine_validita && (x.filtro_matricola == null || x.filtro_matricola.Contains(matricola))));

            return View("ElencoQuestionari", formPrimari);
        }

        public ActionResult Fill ( int idform , string idHotel = null )
        {
            var db = new myRaiData.digiGappEntities( );
            myRaiData.MyRai_FormPrimario model = db.MyRai_FormPrimario.Find( idform );

            ViewBag.Hotel = new HotelModel() { regioni_list = HotelManager.getRegioni() };
            if ( idHotel == null )
                ViewData.Add( "isSingle" , false );
            else
            {
                ViewData.Add( "isSingle" , true );
                HotelSingoloModel singleHotel = new HotelSingoloModel( );
                singleHotel.hotel = HotelManager.dbh.Hotel.First( x => x.Id_Hotel == idHotel );
                if ( singleHotel.hotel == null )
                    return Content( "Albergo non trovato" );
                ViewData.Add( "SingleHotel" , singleHotel );
            }

            return View( model );
        }

        private DateTime? CheckUnicitaFormPrimario ( int id )
        {
            var db = new myRaiData.digiGappEntities( );
            string matr = CommonHelper.GetCurrentUserMatricola( );
            var datacomp = db.MyRai_FormCompletati.Where( x => x.id_form_primario == id && x.matricola == matr )
                .Select( x => x.data_completamento )
                .FirstOrDefault( );

            return datacomp;
        }
        private string ValidaDomanda ( myRaiData.MyRai_FormDomande dom )
        {
            if ( !dom.obbligatoria || dom.MyRai_FormDomande1.Count( ) > 0 )
                return null;

            if (
                ( HttpContext.Request.Form["dom-id-" + dom.id] == null ||
                 String.IsNullOrWhiteSpace( HttpContext.Request.Form["dom-id-" + dom.id].ToString( ) ) )
                &&
                ( HttpContext.Request.Form["dom-id-" + dom.id + "-altro"] == null ||
                 String.IsNullOrWhiteSpace( HttpContext.Request.Form["dom-id-" + dom.id + "-altro"].ToString( ) ) )
                )
                return "La domanda '" + dom.titolo + "' prevede una risposta obbligatoria che non è stata trovata";
            else
                return null;
        }
        private bool DomandaChiusa ( myRaiData.MyRai_FormDomande dom )
        {
            return ( new int[]{ (int)EnumTipologiaDomanda.Risposta_Multipla_CheckBox,
                               (int)EnumTipologiaDomanda.Risposta_singola_Lista_RadioButton,
                               (int)EnumTipologiaDomanda.Risposta_Singola_Lista_Tendina,
                               (int)EnumTipologiaDomanda.SlavePerMatrixRating,
                               (int)EnumTipologiaDomanda.SlavePerRating_6
                                }
                    .Contains( dom.id_tipologia ) );
        }
        private string GetRispMatricola ( bool isAnonimo , string guid , bool isHotel , string idHotel )
        {
            string matricola = "";
            if ( isAnonimo )
                matricola = guid;
            else
                matricola = CommonHelper.GetCurrentUserMatricola( ) + ( isHotel ? "-" + idHotel : "" );
            return matricola;
        }

        [HttpPost]
        public ActionResult save ( int id_formprimario )
        {
            var db = new myRaiData.digiGappEntities( );
            myRaiData.MyRai_FormPrimario fp = db.MyRai_FormPrimario.Find( id_formprimario );
            if ( fp == null )
                return Content( "Questionario non individuato per salvataggio dati" );

            if ( CommonHelper.IsProduzione( ) )
            {
                if ( fp.MyRai_FormTipologiaForm.tipologia != "Hotel" )
                {
                    DateTime? dc = CheckUnicitaFormPrimario( id_formprimario );
                    if ( dc != default( DateTime ) )
                        return Content( "Hai già completato il questionario il " + ( ( DateTime ) dc ).ToString( "dd/MM/yyyy HH.mm" ) );
                }
            }

            string guid = Guid.NewGuid( ).ToString( );

            bool isHotel = fp.MyRai_FormTipologiaForm.tipologia == "Hotel";
            int idDomHotel = 0;
            string idHotel = "";
            if ( isHotel )
            {
                //Nel caso di questionari di tipo albergo, la domanda relativa all'albergo è sempre la prima della prima sezione
                idDomHotel = fp.MyRai_FormSecondario.First( x => x.progressivo == 1 )
                                .MyRai_FormDomande.First( x => x.progressivo == 1 ).id;
            }

            foreach ( var secondario in fp.MyRai_FormSecondario.Where( x => x.attivo ) )
            {
                foreach ( var dom in secondario.MyRai_FormDomande.Where( x => x.attiva ) )
                {
                    string val = ValidaDomanda( dom );
                    if ( !String.IsNullOrWhiteSpace( val ) )
                        return Content( val );

                    //var doma = new DomandaSwitcher(dom).GetDomandaSpecifica();

                    if ( HttpContext.Request.Form["dom-id-" + dom.id] != null )
                    {
                        string risp = HttpContext.Request.Form["dom-id-" + dom.id].ToString( );

                        if ( isHotel && dom.id == idDomHotel )
                            idHotel = risp;

                        if ( DomandaChiusa( dom ) )
                        {
                            foreach ( string r in risp.Split( ',' ) )
                            {
                                if ( String.IsNullOrWhiteSpace( r ) )
                                    continue;
                                string[] segm = r.Split( '-' );
                                int idRispostaPossibile;
                                if ( segm.Length < 3 || !int.TryParse( segm[2] , out idRispostaPossibile ) )
                                    continue;
                                myRaiData.MyRai_FormRisposteDate rispData = new myRaiData.MyRai_FormRisposteDate( )
                                {
                                    MyRai_FormRispostePossibili = db.MyRai_FormRispostePossibili.Find( idRispostaPossibile ) ,
                                    MyRai_FormDomande = dom ,
                                    matricola = GetRispMatricola( fp.anonimo , guid , isHotel , idHotel ) ,
                                    data = DateTime.Now
                                };
                                db.MyRai_FormRisposteDate.Add( rispData );
                            }
                            if ( HttpContext.Request.Form["dom-id-" + dom.id + "-altro"] != null )
                            {
                                string rispAltro = HttpContext.Request.Form["dom-id-" + dom.id + "-altro"].ToString( );
                                if ( !String.IsNullOrWhiteSpace( rispAltro ) )
                                {
                                    myRaiData.MyRai_FormRisposteDate rispData = new myRaiData.MyRai_FormRisposteDate( )
                                    {
                                        MyRai_FormDomande = dom ,
                                        matricola = GetRispMatricola( fp.anonimo , guid , isHotel , idHotel ) ,
                                        data = DateTime.Now ,
                                        risposta_text = rispAltro
                                    };
                                    db.MyRai_FormRisposteDate.Add( rispData );
                                }
                            }
                        }
                        else
                        {
                            myRaiData.MyRai_FormRisposteDate rispData = new myRaiData.MyRai_FormRisposteDate( );
                            rispData.MyRai_FormDomande = dom;
                            rispData.matricola = GetRispMatricola( fp.anonimo , guid , isHotel , idHotel );
                            rispData.data = DateTime.Now;
                            rispData.risposta_text = risp;

                            db.MyRai_FormRisposteDate.Add( rispData );
                        }
                    }
                }
            }
            myRaiData.MyRai_FormCompletati fc = new myRaiData.MyRai_FormCompletati( )
            {
                data_completamento = DateTime.Now ,
                MyRai_FormPrimario = fp ,
                matricola = CommonHelper.GetCurrentUserMatricola( ) ,
                valore1 = isHotel ? idHotel : null
            };
            db.MyRai_FormCompletati.Add( fc );
            if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                if ( fp.invia_mail_comp )
                    SendMailNotificaQuestionario( fp );
                return Content( "OK" );
            }
            else
                return Content( "Errore salvataggio DB" );
        }
        private void SendMailNotificaQuestionario ( myRaiData.MyRai_FormPrimario fp )
        {
            try
            {
                string mailDestinatario = CommonHelper.GetEmailPerMatricola(fp.creato_da);
                myRaiData.digiGappEntities db = new myRaiData.digiGappEntities();
                var paramFormOggetto = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "MailNotificaQuestionarioOggetto");
                var paramFormCorpo = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "MailNotificaQuestionarioCorpo");

                string mailOggetto = paramFormOggetto.Valore1;
                string mailTemplate = paramFormCorpo.Valore1;
                string mailCorpo = paramFormCorpo.Valore2;

                //string mailCorpo = CommonManager.GetParametro<string>(EnumParametriSistema.MailNotificaQuestionario);

                if (!String.IsNullOrWhiteSpace(fp.mail_oggetto))
                    mailOggetto = fp.mail_oggetto;

                if (!String.IsNullOrWhiteSpace(fp.mail_corpo))
                    mailCorpo = fp.mail_corpo;

                string mailBody = mailTemplate
                      .Replace("#MAIL_OGGETTO", mailOggetto)
                      .Replace("#MAIL_CORPO", mailCorpo)
                      .Replace("#QUESTIONARIO", fp.titolo)
                      .Replace("#NOMINATIVO", UtenteHelper.Nominativo());

                CommonHelper.InviaMailGenerica(new string[] { mailDestinatario },
                                                mailOggetto,
                                                mailBody);
            }
            catch ( Exception ex )
            {
            }
        }
    }
}