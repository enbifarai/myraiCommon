using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Net;
using System.Data;
using System.IO;
using myRaiCommonModel.cvModels;
using myRai.Data.CurriculumVitae;
using myRaiHelper;

namespace myRai.Controllers
{
    public class CurriculumVitaeController : BaseCommonController
    {
        cvModel cvBox = new cvModel( );

        public ActionResult Index ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            var lista = this.GetListaBox( cvEnt );
            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }
            return View( cvBox );
        }

        public ActionResult Privacy ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            return View( cvBox );
        }

        #region Controller Languages
        public ActionResult languages ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.lingue = new List<cvModel.Languages>( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            var lista = this.GetListaBox( cvEnt );

            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }

            //carico il modello CvModel.studies
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            List<TCVLingue> lingue = ( cvEnt.TCVLingue.Where( m => m.Matricola == matricola ).OrderBy( y => y.CodLinguaLiv ) ).ToList( );

            foreach ( TCVLingue lang in lingue )
            {
                cvModel.Languages frk = new cvModel.Languages( CommonHelper.GetCurrentUserMatricola( ) );

                frk._altraLingua = lang.AltraLingua;
                frk._codLingua = lang.CodLingua;
                frk._codLinguaLiv = lang.CodLinguaLiv;
                frk._dataOraAgg = lang.DataOraAgg;
                frk._livAscolto = lang.LivAscolto;
                frk._livInterazione = lang.LivInterazione;
                frk._livLettura = lang.LivLettura;
                frk._livProdOrale = lang.LivProdOrale;
                frk._livScritto = lang.LivScritto;
                frk._matricola = lang.Matricola;
                frk._stato = lang.Stato;
                frk._tipoAgg = lang.TipoAgg;
                //Descrizione Lingua
                DLingua tmp_lingua = cvEnt.DLingua.Where( m => m.CodLingua == lang.CodLingua ).First( );
                frk._descLingua = tmp_lingua.DescLingua;
                //Flag dello Stato
                frk._flagStato = tmp_lingua.FlagStato;
                //Descrizione Livello di Lingua
                DLinguaLiv tmp_lingualiv = cvEnt.DLinguaLiv.Where( m => m.CodLinguaLiv == lang.CodLinguaLiv ).First( );
                frk._descLinguaLiv = tmp_lingualiv.DescLinguaLiv;

                cvBox.lingue.Add( frk );
            }

            //---------------------------------
            ViewBag.idMenu = 20;

            return View( "~/Views/CurriculumVitae/languages/Index.cshtml" , cvBox );
        }

        [HttpPost]
        public ActionResult InsertLanguage ( cvModel.Languages lingue )
        {
            string result;

            if ( ModelState.IsValid )
            {
                string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
                cv_ModelEntities cvEnt = new cv_ModelEntities( );
                TCVLingue language = new TCVLingue( );

                //riempimento oggetto Language ->TCVLingue
                language.Matricola = matricola;
                language.AltraLingua = "";
                language.CodLingua = lingue._codLingua;
                language.CodLinguaLiv = lingue._codLinguaLiv;
                language.DataOraAgg = DateTime.Now;
                language.LivAscolto = lingue._livAscolto;
                language.LivInterazione = lingue._livInterazione;
                language.LivLettura = lingue._livLettura;
                language.LivProdOrale = lingue._livProdOrale;
                language.LivScritto = lingue._livScritto;
                language.Note = lingue._note;
                language.Stato = "S";
                language.TipoAgg = "I";

                try
                {
                    cvEnt.TCVLingue.Add( language );
                    cvEnt.SaveChanges( );
                    result = "ok";
                }
                catch ( Exception )
                {
                    result = "error";
                }
            }
            else
            {
                result = "invalid";
            }

            return Content( result );
        }

        [HttpGet]
        public ActionResult DeleteLanguages ( string matricola , string codLingua )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );

            if ( matricola == null && codLingua == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }

            TCVLingue lingua = cvEnt.TCVLingue.Find( matricola , codLingua );
            if ( lingua == null )
            {
                return HttpNotFound( );
            }

            try
            {
                cvEnt.TCVLingue.Remove( lingua );
                cvEnt.SaveChanges( );
            }
            catch ( Exception exc )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }

            ViewBag.idMenu = 20;
            return RedirectToAction( "languages" );
        }

        //action per la creazione della view di modifica per Languages
        [HttpPost]
        public ActionResult Create_ViewLanguages ( cvModel.Languages language )
        {
            string matricola = language._matricola;
            string codLingua = language._codLingua;

            if ( matricola == null || codLingua == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            return View( "~/Views/CurriculumVitae/languages/ViewLanguage.cshtml" , language );
        }

        [HttpPost]
        public ActionResult ModificaLanguage ( cvModel.Languages language )
        {
            string result;

            if ( ModelState.IsValid )
            {
                string matricola = language._matricola;
                string codLingua = language._codLingua;

                cv_ModelEntities cvEnt = new cv_ModelEntities( );
                TCVLingue lingua = cvEnt.TCVLingue.Find( matricola , codLingua );

                lingua.AltraLingua = "";
                lingua.CodLingua = codLingua;
                lingua.CodLinguaLiv = language._codLinguaLiv;
                lingua.DataOraAgg = DateTime.Now;
                lingua.LivAscolto = language._livAscolto;
                lingua.LivInterazione = language._livInterazione;
                lingua.LivLettura = language._livLettura;
                lingua.LivProdOrale = language._livProdOrale;
                lingua.LivScritto = language._livScritto;
                lingua.Matricola = matricola;
                lingua.Note = language._note;
                lingua.Stato = "S";
                lingua.TipoAgg = "A";

                try
                {
                    cvEnt.SaveChanges( );
                    result = "ok";
                }
                catch ( Exception exc )
                {
                    result = "error";
                }
            }
            else
            {
                result = "invalid";
            }

            return Content( result );
        }

        #endregion


        #region Controller Experiences
        public ActionResult experiences ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.experencies = new List<cvModel.Experiences>( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            var lista = this.GetListaBox( cvEnt );
            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }

            //carico il modello CvModel.experiences
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            string codFigProf = UtenteHelper.EsponiAnagrafica( )._codiceFigProf;

            var esperienze = cvEnt.TCVEsperExRai.Where( x => x.Matricola == matricola ).OrderByDescending( z => z.DataInizio ).ToList( );

            foreach ( TCVEsperExRai esp in esperienze )
            {
                cvModel.Experiences frk_esp = new cvModel.Experiences( CommonHelper.GetCurrentUserMatricola( ) );
                frk_esp._areaAtt = esp.AreaAtt;
                frk_esp._codContinente = esp.CodContinente;
                if ( esp.CodContinente != null )
                {
                    frk_esp._descContinente = cvEnt.DContinente.Where( x => x.CodContinente == esp.CodContinente ).First( ).DesContinente.ToString( );
                }
                else
                {
                    frk_esp._descContinente = "";
                }
                frk_esp._codRedazione = esp.CodRedazione;
                if ( esp.CodRedazione != null )
                {
                    frk_esp._descRedazione = cvEnt.DRedazione.Where( x => x.CodRedazione == esp.CodRedazione ).First( ).DesRedazione.ToString( );
                }
                else
                {
                    frk_esp._descRedazione = "";
                }
                frk_esp._codiceFiguraProf = esp.CodiceFiguraProf;
                frk_esp._codDirezione = esp.CodDirezione;
                if ( esp.CodDirezione != null )
                {
                    frk_esp._direzione = cvEnt.VDServizio.Where( x => x.Codice == esp.CodDirezione ).First( ).Descrizione.ToString( );
                }
                else
                {
                    frk_esp._direzione = "";
                }
                frk_esp._codSocieta = esp.CodSocieta;
                if ( ( esp.CodSocieta != null ) && ( esp.CodSocieta != "" ) )
                {
                    frk_esp._societa = cvEnt.VDSocieta.Where( x => x.Codice == esp.CodSocieta ).First( ).Descrizione.ToString( );
                }
                else
                {
                    frk_esp._societa = esp.Societa;
                }
                frk_esp._stato = esp.Stato;
                frk_esp._dataFine = esp.DataFine;
                frk_esp._dataInizio = esp.DataInizio;
                frk_esp._dataOraAgg = esp.DataOraAgg;
                frk_esp._descrizioneEsp = esp.DescrizioneEsp;
                frk_esp._flagEspEstero = esp.FlagEspEstero;
                frk_esp._flagEspRai = esp.FlagEspRai;
                if ( ( codFigProf == "MAA" ) || ( codFigProf == "MBA" ) )
                {
                    frk_esp._isGiornalista = "1";
                }
                else
                {
                    frk_esp._isGiornalista = "0";
                }
                frk_esp._localitaEsp = esp.LocalitaEsp;
                frk_esp._matricola = esp.Matricola;
                frk_esp._nazione = esp.Nazione;
                frk_esp._prog = esp.Prog;
                frk_esp._tipoAgg = esp.TipoAgg;
                frk_esp._titoloEspQual = esp.TitoloEspQual;
                frk_esp._ultRuolo = esp.UltRuolo;
                frk_esp._budgetGest = esp.CodBudgetGest;
                frk_esp._risorseGest = esp.CodRisorseGest;
                frk_esp._procura = esp.CodProcura;

                cvBox.experencies.Add( frk_esp );
            }
            //---------------------------------
            ViewBag.idMenu = 24;
            return View( "~/Views/CurriculumVitae/experiences/Index.cshtml" , cvBox );
        }

        [HttpPost]
        public ActionResult Create_DettaglioRai ( cvModel.Experiences experiences )
        {
            return View( "~/Views/CurriculumVitae/experiences/DettaglioRai.cshtml" , experiences );
        }

        [HttpPost]
        public ActionResult Create_DettaglioExtraRai ( cvModel.Experiences experiences )
        {
            return View( "~/Views/CurriculumVitae/experiences/DettaglioExtraRai.cshtml" , experiences );
        }

        [HttpPost]
        public ActionResult InsertExperiences ( cvModel.Experiences experiences )
        {
            string result = "";
            string[] data_tmp;
            string matricola;
            int prog;

            matricola = UtenteHelper.EsponiAnagrafica( )._matricola;

            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            TCVEsperExRai esper = new TCVEsperExRai( );

            if ( ( experiences._dataInizio == "" ) || ( experiences._dataFine == "" ) || ( experiences._dataFine == null ) || ( experiences._dataInizio == null ) )
            {
                result = "error-data";
                return Content( result );
            }

            esper.AreaAtt = experiences._areaAtt;
            esper.CodContinente = experiences._codContinente;
            esper.CodDirezione = experiences._codDirezione;
            esper.CodiceFiguraProf = experiences._codiceFiguraProf;
            esper.CodRedazione = experiences._codRedazione;
            esper.CodSocieta = experiences._codSocieta;
            data_tmp = experiences._dataInizio.Split( '/' );
            esper.DataInizio = data_tmp[2] + data_tmp[1] + data_tmp[0];
            data_tmp = experiences._dataFine.Split( '/' );
            esper.DataFine = data_tmp[2] + data_tmp[1] + data_tmp[0];
            esper.DataOraAgg = DateTime.Now;
            esper.DescrizioneEsp = experiences._descrizioneEsp;
            esper.Direzione = experiences._direzione;
            if ( ( experiences._nazione != "ITALIA" ) && ( experiences._nazione != "" ) )
            {
                esper.FlagEspEstero = "1";
            }
            else
            {
                esper.FlagEspEstero = "0";
            }
            esper.FlagEspRai = experiences._flagEspRai;
            esper.LocalitaEsp = experiences._localitaEsp;
            esper.Matricola = matricola;
            esper.Nazione = experiences._nazione;
            esper.Societa = experiences._societa;
            esper.Stato = "S";
            esper.TipoAgg = "I";
            esper.TitoloEspQual = experiences._titoloEspQual;
            esper.UltRuolo = experiences._ultRuolo;
            esper.CodBudgetGest = experiences._budgetGest;
            esper.CodRisorseGest = experiences._risorseGest;
            esper.CodProcura = experiences._procura;

            //calcolo del prog
            var tmp = cvEnt.TCVEsperExRai.Where( x => x.Matricola == matricola );
            if ( tmp.Count( ) == 0 )
            {
                prog = 1;
            }
            else
            {
                var nro_prog = ( cvEnt.TCVEsperExRai.Where( x => x.Matricola == matricola ) ).Max( x => x.Prog );
                prog = Convert.ToInt32( nro_prog ) + 1;
            }
            esper.Prog = prog;
            try
            {
                cvEnt.TCVEsperExRai.Add( esper );
                cvEnt.SaveChanges( );
                result = "ok";
            }
            catch ( Exception exc )
            {
                result = exc.Message;
            }

            return Content( result );
        }

        [HttpPost]
        public ActionResult Create_ModificaDettaglioRai ( cvModel.Experiences experiences )
        {
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            int prog = experiences._prog;

            if ( matricola == null || prog == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            return View( "~/Views/CurriculumVitae/experiences/ModificaDettaglioRai.cshtml" , experiences );
        }

        [HttpPost]
        public ActionResult Create_ModificaDettaglioExtraRai ( cvModel.Experiences experiences )
        {
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            int prog = experiences._prog;

            if ( matricola == null || prog == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            return View( "~/Views/CurriculumVitae/experiences/ModificaDettaglioExtraRai.cshtml" , experiences );
        }

        [HttpGet]
        public ActionResult DeleteExperiences ( string matricola , int prog )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );

            if ( ( matricola == null ) && ( prog < 0 ) )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            TCVEsperExRai master = cvEnt.TCVEsperExRai.Find( matricola , prog );
            if ( master == null )
            {
                return HttpNotFound( );
            }
            try
            {
                cvEnt.TCVEsperExRai.Remove( master );
                cvEnt.SaveChanges( );
            }
            catch ( Exception exc )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            //FREAK - RIEMPIMENTO DEL MODELLO CVBOX E DELLE LISTE SPECIALIZZAZIONI E ISTRUZIONE
            //---------------------------------
            ViewBag.idMenu = 19;
            return RedirectToAction( "experiences" );
            //FINEE
        }

        [HttpPost]
        public ActionResult ModificaExperiences ( cvModel.Experiences experiences )
        {
            string result = "";
            string matricola = experiences._matricola;
            int prog = experiences._prog;

            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            TCVEsperExRai esper = new TCVEsperExRai( );

            esper = cvEnt.TCVEsperExRai.Find( matricola , prog );

            if ( esper == null )
            {
                return HttpNotFound( );
            }

            esper.AreaAtt = experiences._areaAtt;
            esper.CodContinente = experiences._codContinente;
            esper.CodDirezione = experiences._codDirezione;
            esper.CodiceFiguraProf = experiences._codiceFiguraProf;
            esper.CodRedazione = experiences._codRedazione;
            esper.CodSocieta = experiences._codSocieta;
            esper.DataOraAgg = DateTime.Now;
            esper.DescrizioneEsp = experiences._descrizioneEsp;
            esper.Direzione = experiences._direzione;
            if ( ( experiences._nazione != "ITALIA" ) && ( experiences._nazione != "" ) )
            {
                esper.FlagEspEstero = "1";
            }
            else
            {
                esper.FlagEspEstero = "0";
            }
            esper.FlagEspRai = experiences._flagEspRai;
            esper.LocalitaEsp = experiences._localitaEsp;
            esper.Matricola = matricola;
            esper.Nazione = experiences._nazione;
            esper.Societa = experiences._societa;
            esper.Stato = "S";
            esper.TipoAgg = "A";
            esper.TitoloEspQual = experiences._titoloEspQual;
            esper.UltRuolo = experiences._ultRuolo;
            esper.CodBudgetGest = experiences._budgetGest;
            esper.CodRisorseGest = experiences._risorseGest;
            esper.CodProcura = experiences._procura;

            try
            {
                cvEnt.SaveChanges( );
                result = "ok";
            }
            catch ( Exception ex )
            {
                result = ex.Message;
            }

            return Content( result );
        }

        #endregion

        #region Controller Studies
        public ActionResult studies ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.curricula = new List<cvModel.Studies>( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            var lista = this.GetListaBox( cvEnt );

            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }

            //carico il modello CvModel.studies
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            var istruzione = ( cvEnt.TCVIstruzione.Where( m => m.Matricola == matricola ) ).ToList( );
            var specializz = ( cvEnt.TCVSpecializz.Where( m => m.Matricola == matricola ) ).ToList( );

            foreach ( TCVIstruzione istr in istruzione )
            {
                cvModel.Studies frk = new cvModel.Studies( CommonHelper.GetCurrentUserMatricola( ) );
                //freak - riempire i campi
                using ( var ctx = new cv_ModelEntities( ) )
                {
                    var param = new SqlParameter( "@param" , istr.CodTitolo );
                    List<UtenteHelper.CV_DescTitoloLogo> tmp = ctx.Database.SqlQuery<UtenteHelper.CV_DescTitoloLogo>( "exec sp_GETDESCTITOLO @param" , param ).ToList( );

                    frk._descTipoTitolo = tmp[0].DescTipoTitolo; //tmp.GetValue(0,0).ToString();
                    frk._descTitolo = tmp[0].DescTitolo;//tmp.GetValue(0, 1).ToString();
                    frk._logo = tmp[0].Logo;//tmp.GetValue(0, 2).ToString();
                    frk._codNazione = istr.CodNazione;

                    try
                    {
                        var sql2 = ctx.Database.SqlQuery<string>( "SELECT DES_NAZIONE FROM DNAZIONE WHERE COD_SIGLANAZIONE = '" + frk._codNazione + "'" ).ToList( );
                        frk._descNazione = sql2[0];
                    }
                    catch
                    {
                        frk._descNazione = "";
                    }
                }
                frk._codTitolo = istr.CodTitolo;
                frk._codTipoTitolo = istr.CodTipoTitolo;
                frk._corsoLaurea = istr.CorsoLaurea;
                frk._dataFine = istr.AnnoFine;
                frk._dataInizio = istr.AnnoInizio;
                frk._dataoraAgg = ( DateTime ) istr.DataOraAgg;
                frk._durata = istr.Durata;
                frk._indirizzoStudi = ( istr.IndirizzoStudi != null ) ? istr.IndirizzoStudi : "";
                frk._istituto = ( istr.Istituto != null ) ? istr.Istituto : "";
                frk._localitaStudi = ( istr.LocalitaStudi != null ) ? istr.LocalitaStudi : "";
                frk._lode = ( istr.Lode == null ) ? ' ' : Convert.ToChar( istr.Lode );
                frk._matricola = istr.Matricola;
                frk._prog = -1;
                frk._scala = istr.Scala;
                frk._stato = Convert.ToChar( istr.Stato );
                frk._tipoAgg = Convert.ToChar( istr.TipoAgg );
                frk._titoloSpecializ = null;
                frk._titoloTesi = istr.TitoloTesi;
                frk._voto = istr.Voto;
                frk._riconoscimento = "";
                frk._tableTarget = "I";
                cvBox.curricula.Add( frk );
            }

            foreach ( TCVSpecializz spec in specializz )
            {
                cvModel.Studies frk = new cvModel.Studies( CommonHelper.GetCurrentUserMatricola( ) );
                //freak - riempire i campi
                using ( var ctx = new cv_ModelEntities( ) )
                {
                    var param = new SqlParameter( "@param" , spec.TipoSpecial );
                    List<UtenteHelper.CV_DescTitoloLogo> tmp = ctx.Database.SqlQuery<UtenteHelper.CV_DescTitoloLogo>( "exec sp_GETDESCTITOLO @param" , param ).ToList( );
                    frk._descTipoTitolo = tmp[0].DescTipoTitolo; //tmp.GetValue(0,0).ToString();
                    frk._descTitolo = tmp[0].DescTitolo;//tmp.GetValue(0, 1).ToString();
                    frk._logo = tmp[0].Logo;//tmp.GetValue(0, 2).ToString();
                    //recupero descNazione tramite codNazione
                    // freak - imposto un valore a frk._codNazione
                    frk._codNazione = spec.CodNazione;
                    try
                    {
                        var sql2 = ctx.Database.SqlQuery<string>( "SELECT DES_NAZIONE FROM DNAZIONE WHERE COD_SIGLANAZIONE = '" + frk._codNazione + "'" ).ToList( );
                        frk._descNazione = sql2[0];
                    }
                    catch
                    {
                        frk._descNazione = "";
                    }
                }
                frk._codTitolo = spec.TipoSpecial;
                frk._corsoLaurea = "";// spec.Titolo; //freak - da controllare con Massimo Tesoro
                frk._dataFine = spec.DataFine.Substring( 6 , 2 ) + "/" + spec.DataFine.Substring( 4 , 2 ) + "/" + spec.DataFine.Substring( 0 , 4 );
                frk._dataInizio = ( spec.DataInizio.Length == 8 ) ? spec.DataInizio.Substring( 6 , 2 ) + "/" + spec.DataInizio.Substring( 4 , 2 ) + "/" + spec.DataInizio.Substring( 0 , 4 ) : "";
                frk._dataoraAgg = ( DateTime ) spec.DataOraAgg;
                frk._durata = spec.Durata;
                frk._indirizzoStudi = ( spec.IndirizzoSpecial != null ) ? spec.IndirizzoSpecial : "";
                frk._istituto = ( spec.Istituto != null ) ? spec.Istituto : "";
                frk._localitaStudi = ( spec.LocalitaSpecial != null ) ? spec.LocalitaSpecial : "";
                frk._lode = ( spec.Lode == null ) ? ' ' : Convert.ToChar( spec.Lode );
                frk._matricola = spec.Matricola;
                frk._prog = spec.Prog;
                frk._scala = spec.Scala;
                frk._stato = Convert.ToChar( spec.Stato );
                frk._tipoAgg = Convert.ToChar( spec.TipoAgg );
                frk._titoloSpecializ = spec.Titolo;
                frk._titoloTesi = ""; //freak - da controllare con Massimo Tesoro
                frk._voto = spec.Voto;
                //frk._riconoscimento = (spec._riconoscimento != null) ? spec._riconoscimento : "";
                frk._tableTarget = "S";
                cvBox.curricula.Add( frk );
            }
            //---------------------------------
            ViewBag.idMenu = 17;
            return View( "~/Views/CurriculumVitae/studies/Index.cshtml" , cvBox );
        }

        //inserimento di un nuovo Currivulum
        [HttpPost]
        public ActionResult InsertCV ( cvModel.Studies curricula )
        {
            // ************************rimempimento del modello cvBox per la view Studies************************
            /*cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.curricula = new List<Models.cvModels.cvModel.Studies>();
            cvBox.listaBox = new List<myRai.Models.cvModels.cvModel.Box>();
            cvBox.menuSidebar = new sidebarModel(5);
            string matricola = Utente.EsponiAnagrafica()._matricola;*/
            bool isScala, isDataFine;
            string result = "";
            isScala = false;
            isDataFine = false;

            if ( ModelState.IsValid )
            {
                //inserimento nel DB --> controllo della variabile _tableTarget: "I"->TCVIstruzione ; "S"->TCVSpecializz
                string table;
                string matricola;

                cv_ModelEntities cvEnt = new cv_ModelEntities( );

                matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
                table = curricula._tableTarget;

                switch ( table )
                {
                    case "I":
                        //inserimento in TCVIstruzione
                        TCVIstruzione istruzione = new TCVIstruzione( );

                        istruzione.Matricola = matricola;
                        istruzione.CodTipoTitolo = curricula._codTipoTitolo;
                        istruzione.CodTitolo = curricula._codTitolo;
                        istruzione.AnnoInizio = curricula._dataInizio;
                        istruzione.AnnoFine = curricula._dataFine;
                        istruzione.Scala = curricula._scala;
                        istruzione.Voto = curricula._voto;
                        istruzione.Lode = ( curricula._lode == 'S' ) ? "S" : "N";
                        istruzione.Durata = "";
                        istruzione.Istituto = curricula._istituto;
                        istruzione.CorsoLaurea = curricula._corsoLaurea;
                        istruzione.TitoloTesi = curricula._titoloTesi;
                        istruzione.Stato = "S";
                        istruzione.TipoAgg = "I";
                        istruzione.DataOraAgg = DateTime.Now;
                        istruzione.LocalitaStudi = curricula._localitaStudi;
                        istruzione.CodNazione = curricula._codNazione;
                        istruzione.IndirizzoStudi = curricula._indirizzoStudi;
                        istruzione.Riconoscimento = curricula._riconoscimento;
                        try
                        {
                            cvEnt.TCVIstruzione.Add( istruzione );
                            cvEnt.SaveChanges( );
                            result = "ok";
                        }
                        catch ( Exception exc )
                        {
                            result = exc.Message;
                        }

                        break;
                    case "S":
                        //inseirmento in TCVSpecializz
                        TCVSpecializz specializz = new TCVSpecializz( );
                        string[] data_tmp;
                        string data_inizio, data_fine;
                        int prog;

                        data_tmp = curricula._dataInizio.Split( '/' );
                        if ( data_tmp.Count( ) == 3 )
                        {
                            data_inizio = data_tmp[2] + data_tmp[1] + data_tmp[0];
                        }
                        else
                        {
                            data_inizio = "";
                        }

                        data_tmp = curricula._dataFine.Split( '/' );
                        if ( data_tmp.Count( ) == 3 )
                        {
                            data_fine = data_tmp[2] + data_tmp[1] + data_tmp[0];
                        }
                        else
                        {
                            result = "data";
                            return Content( result );
                        }
                        //var tmp = (cvEnt.TCVSpecializz.Where(x => x.Matricola == matricola )).Max(x => x.Prog);
                        var tmp = cvEnt.TCVSpecializz.Where( x => x.Matricola == matricola );
                        if ( tmp.Count( ) == 0 )
                        {
                            prog = 1;
                        }
                        else
                        {
                            var nro_prog = ( cvEnt.TCVSpecializz.Where( x => x.Matricola == matricola ) ).Max( x => x.Prog );
                            prog = Convert.ToInt32( nro_prog ) + 1;
                        }

                        //rieempimento di specializz
                        specializz.DataInizio = data_inizio;
                        specializz.DataFine = data_fine;
                        specializz.Stato = "S";
                        specializz.TipoAgg = "I";
                        specializz.Matricola = matricola;
                        specializz.Durata = "";
                        specializz.TipoSpecial = curricula._codTitolo;
                        specializz.Titolo = curricula._titoloSpecializ;
                        specializz.Istituto = curricula._istituto;
                        specializz.Voto = curricula._voto;
                        specializz.Scala = curricula._scala;
                        specializz.Note = curricula._note;
                        specializz.Lode = ( curricula._lode == 'S' ) ? "S" : "N";
                        specializz.LocalitaSpecial = curricula._localitaStudi;
                        specializz.CodNazione = curricula._codNazione;
                        specializz.IndirizzoSpecial = curricula._indirizzoStudi;
                        specializz.Riconoscimento = curricula._riconoscimento;
                        specializz.DataOraAgg = DateTime.Now;
                        specializz.Prog = prog;

                        //inserisco
                        try
                        {
                            cvEnt.TCVSpecializz.Add( specializz );
                            cvEnt.SaveChanges( );
                            result = "ok";
                        }
                        catch ( Exception exc )
                        {
                            result = exc.Message;
                        }
                        break;
                    default:
                        //mando messaggio: Errore di inserimento
                        result = "error";
                        break;
                }
            }
            else
            {
                foreach ( var modelState in ModelState.Values )
                {
                    foreach ( var elem in modelState.Errors )
                    {
                        switch ( elem.ErrorMessage )
                        {
                            case "scala":
                                isScala = true;
                                break;
                            case "datafine":
                                isDataFine = true;
                                break;
                            default:
                                break;
                        }
                    }
                }
                if ( isScala )
                {
                    result += "scala";
                }
                if ( isDataFine )
                {
                    result += "datafine";
                }
            }
            return Content( result );
        }

        private List<TCVBox_V2> GetListaBox ( cv_ModelEntities cvEnt )
        {
            //freak - creare la lista da poi ciclare per creare i box
            string figurapro;
            figurapro = UtenteHelper.EsponiAnagrafica( )._codiceFigProf;
            //risultato tabella inner join con TCVBox_Figuraprof_V2
            var innerJoinQuery =
            ( from box in cvEnt.TCVBox_V2
              join boxDettaglio in cvEnt.TCVBox_Figuraprof_V2 on box.Id_box equals boxDettaglio.Id_box
              where ( boxDettaglio.CodiceFiguraPro == figurapro )
              orderby boxDettaglio.Posizione
              select box ).ToList( ); //produces flat sequence

            //lista completa della tabella TCVBox_V2 con posizione != null
            var lista = ( cvEnt.TCVBox_V2.Where( pos => pos.Posizione != null ).OrderBy( d => d.Posizione ) ).ToList( );
            int count = innerJoinQuery.Count( );
            var completa = lista;
            /*
             :            Figura_pro in ('XAA', 'XDA', 'Q0A')
                          & Servizio in ('01', '02', '03', '35', '63', '65', '17', '33', '61', '62')

             */
            if ( figurapro == "XAA" || figurapro == "XDA" || figurapro == "Q0A" )
            {
                string m = CommonHelper.GetCurrentUserMatricola( );
                var dip = cvEnt.TDipendenti.Where( x => x.Matricola == m && x.Flag_ultimo_record == "$" ).FirstOrDefault( );
                if ( dip != null && dip.Servizio != null && dip.Servizio.Length >= 2 )
                {
                    string serv = dip.Servizio.Substring( 0 , 2 );
                    if ( !new string[] { "01" , "02" , "03" , "35" , "63" , "65" , "17" , "33" , "61" , "62" }.Contains( serv ) )
                    {
                        count = 0;
                    }

                }
            }

            if ( count > 0 )
            {
                completa = innerJoinQuery; //se il count > 0 prendo come riferimento la selezione con l'inner join
            }
            return completa;
        }

        [HttpGet]
        public ActionResult DeleteStudiesMaster ( string matricola , int prog )
        {
            //devo trovare l'elemento nella tabella TCVSpecializz

            cv_ModelEntities cvEnt = new cv_ModelEntities( );

            if ( ( matricola == null ) && ( prog < 0 ) )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            TCVSpecializz master = cvEnt.TCVSpecializz.Find( matricola , prog );
            if ( master == null )
            {
                return HttpNotFound( );
            }
            try
            {
                cvEnt.TCVSpecializz.Remove( master );
                cvEnt.SaveChanges( );
            }
            catch ( Exception exc )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            //FREAK - RIEMPIMENTO DEL MODELLO CVBOX E DELLE LISTE SPECIALIZZAZIONI E ISTRUZIONE
            //---------------------------------
            ViewBag.idMenu = 17;
            return RedirectToAction( "studies" );
            //FINEE
        }

        [HttpGet]
        public ActionResult DeleteStudiesDiplomaLaurea ( string matricola , string codTitolo )
        {
            //devo trovare l'elemento nella tabella TCVIstruzioni

            cv_ModelEntities cvEnt = new cv_ModelEntities( );

            if ( ( matricola == null ) && ( codTitolo == null ) )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            TCVIstruzione laurea_diploma = cvEnt.TCVIstruzione.Find( matricola , codTitolo );
            if ( laurea_diploma == null )
            {
                return HttpNotFound( );
            }
            try
            {
                cvEnt.TCVIstruzione.Remove( laurea_diploma );
                cvEnt.SaveChanges( );
            }
            catch ( Exception exc )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadGateway );
            }

            //FREAK - RIEMPIMENTO DEL MODELLO CVBOX E DELLE LISTE SPECIALIZZAZIONI E ISTRUZIONE
            //---------------------------------
            ViewBag.idMenu = 17;

            return RedirectToAction( "studies" );
            //FINEE
        }

        //Controller per Modifica del CV Studies (Laurea; Master; Diploma)
        public ActionResult ModificaStudies ( cvModel.Studies curricula )
        {
            string logo;
            string matricola, codTitolo;
            string data_inizio, data_fine, frk_tmp;
            int prog;


            logo = curricula._logo;
            matricola = curricula._matricola;
            codTitolo = curricula._codTitolo;
            prog = curricula._prog;
            if ( logo == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }

            switch ( logo )
            {
                case "Diploma":

                    return View( "~/Views/CurriculumVitae/studies/ViewDiploma.cshtml" , curricula );
                case "Laurea":
                    if ( ( matricola == null ) || ( codTitolo == null ) )
                    {
                        return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
                    }

                    return View( "~/Views/CurriculumVitae/studies/ViewLaurea.cshtml" , curricula );
                case "Master":
                    frk_tmp = curricula._dataInizio;
                    if ( frk_tmp == null )
                    {
                        data_inizio = "";
                    }
                    else
                    {
                        data_inizio = frk_tmp;
                    }
                    frk_tmp = curricula._dataFine;
                    if ( frk_tmp == null )
                    {
                        return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
                    }
                    else
                    {
                        data_fine = frk_tmp;
                    }

                    curricula._dataInizio = data_inizio;
                    curricula._dataFine = data_fine;

                    return View( "~/Views/CurriculumVitae/studies/ViewMaster.cshtml" , curricula );
                default:
                    return HttpNotFound( );
            }
        }

        //modifica del Currivulum
        [HttpPost]
        public ActionResult ModificaCV ( cvModel.Studies curricula )
        {
            // ************************rimempimento del modello cvBox per la view Studies************************
            /*cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.curricula = new List<Models.cvModels.cvModel.Studies>();
            cvBox.listaBox = new List<myRai.Models.cvModels.cvModel.Box>();
            cvBox.menuSidebar = new sidebarModel(5);
            string matricola = Utente.EsponiAnagrafica()._matricola;*/
            bool isScala, isDataFine;
            string result = "";
            isScala = false;
            isDataFine = false;

            if ( ModelState.IsValid )
            {
                //inserimento nel DB --> controllo della variabile _tableTarget: "I"->TCVIstruzione ; "S"->TCVSpecializz
                string table;
                string matricola;
                string codTitolo;
                cv_ModelEntities cvEnt = new cv_ModelEntities( );

                matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
                codTitolo = curricula._codTitolo;
                table = curricula._tableTarget;

                switch ( table )
                {
                    case "I":
                        //inserimento in TCVIstruzione
                        //TCVIstruzione istruzione = new TCVIstruzione();
                        TCVIstruzione istruzione = cvEnt.TCVIstruzione.Find( matricola , codTitolo );

                        istruzione.Matricola = matricola;
                        istruzione.CodTipoTitolo = curricula._codTipoTitolo;
                        istruzione.CodTitolo = curricula._codTitolo;
                        istruzione.AnnoInizio = curricula._dataInizio;
                        istruzione.AnnoFine = curricula._dataFine;
                        istruzione.Scala = curricula._scala;
                        istruzione.Voto = curricula._voto;
                        istruzione.Lode = ( curricula._lode == 'S' ) ? "S" : "N";
                        istruzione.Durata = "";
                        istruzione.Istituto = curricula._istituto;
                        istruzione.CorsoLaurea = curricula._corsoLaurea;
                        istruzione.TitoloTesi = curricula._titoloTesi;
                        istruzione.Stato = "S";
                        istruzione.TipoAgg = "A";
                        istruzione.DataOraAgg = DateTime.Now;
                        istruzione.LocalitaStudi = curricula._localitaStudi;
                        istruzione.CodNazione = curricula._codNazione;
                        istruzione.IndirizzoStudi = curricula._indirizzoStudi;
                        istruzione.Riconoscimento = curricula._riconoscimento;
                        try
                        {
                            //cvEnt.TCVIstruzione.(istruzione);
                            cvEnt.SaveChanges( );
                            result = "ok";
                        }
                        catch ( Exception exc )
                        {
                            result = exc.Message;
                        }
                        break;
                    case "S":
                        //inseirmento in TCVSpecializz
                        //TCVSpecializz specializz = new TCVSpecializz();
                        string[] data_tmp;
                        string data_inizio, data_fine;
                        int prog;

                        prog = curricula._prog;
                        TCVSpecializz specializz = cvEnt.TCVSpecializz.Find( matricola , prog );
                        data_tmp = curricula._dataInizio.Split( '/' );
                        if ( data_tmp.Count( ) == 3 )
                        {
                            data_inizio = data_tmp[2] + data_tmp[1] + data_tmp[0];
                        }
                        else
                        {
                            data_inizio = "";
                        }

                        data_tmp = curricula._dataFine.Split( '/' );
                        if ( data_tmp.Count( ) == 3 )
                        {
                            data_fine = data_tmp[2] + data_tmp[1] + data_tmp[0];
                        }
                        else
                        {
                            result = "data";
                            return Content( result );
                        }

                        //rieempimento di specializz
                        specializz.DataInizio = data_inizio;
                        specializz.DataFine = data_fine;
                        specializz.Stato = "S";
                        specializz.TipoAgg = "A";
                        specializz.Matricola = matricola;
                        specializz.Durata = "";
                        specializz.TipoSpecial = curricula._codTitolo;
                        specializz.Titolo = curricula._titoloSpecializ;
                        specializz.Istituto = curricula._istituto;
                        specializz.Voto = curricula._voto;
                        specializz.Scala = curricula._scala;
                        specializz.Note = curricula._note;
                        specializz.Lode = ( curricula._lode == 'S' ) ? "S" : "N";
                        specializz.LocalitaSpecial = curricula._localitaStudi;
                        specializz.CodNazione = curricula._codNazione;
                        specializz.IndirizzoSpecial = curricula._indirizzoStudi;
                        specializz.Riconoscimento = curricula._riconoscimento;
                        specializz.DataOraAgg = DateTime.Now;
                        specializz.Prog = prog;

                        //inserisco
                        try
                        {
                            //cvEnt.TCVSpecializz.Add(specializz);
                            cvEnt.SaveChanges( );
                            result = "ok";
                        }
                        catch ( Exception exc )
                        {
                            result = exc.Message;
                        }
                        break;
                    default:
                        //mando messaggio: Errore di inserimento
                        result = "error";
                        break;
                }
            }
            else
            {
                foreach ( var modelState in ModelState.Values )
                {
                    foreach ( var elem in modelState.Errors )
                    {
                        switch ( elem.ErrorMessage )
                        {
                            case "scala":
                                isScala = true;
                                break;
                            case "datafine":
                                isDataFine = true;
                                break;
                            default:
                                break;
                        }
                    }
                }
                if ( isScala )
                {
                    result += "scala";
                }
                if ( isDataFine )
                {
                    result += "datafine";
                }
            }
            return Content( result );
        }
        #endregion

        #region Controller AreeInteresse
        public ActionResult AreeInteresse ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.areeInteresse = new List<cvModel.AreeInteresse>( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            var lista = this.GetListaBox( cvEnt );
            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }

            //carico il modello CvModel.areeInteresse
            string descAreaOrg, descServizio, descTipoDispo, descAreaGeo;
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            List<TCVAreaIntAz> interesse = cvEnt.TCVAreaIntAz.Where( m => m.Matricola == matricola ).ToList( );

            foreach ( var area in interesse )
            {
                cvModel.AreeInteresse frk = new cvModel.AreeInteresse( CommonHelper.GetCurrentUserMatricola( ) );

                frk._areeIntDispo = area.AreeIntDispo;
                frk._codAreaGeo = area.CodAreaGeo;
                frk._codAreaOrg = area.CodAreaOrg;
                frk._codServizio = area.CodServizio;
                frk._codTipoDispo = area.CodTipoDispo;
                frk._dataOraAgg = area.DataOraAgg;
                frk._flagEsteroDispo = area.FlagEsteroDispo;
                frk._matricola = area.Matricola;
                frk._profIntDispo = area.ProfIntDispo;
                frk._prog = area.Prog;
                frk._stato = area.Stato;
                frk._tipoAgg = area.TipoAgg;

                var frk_geogio = cvEnt.DAreaGeoGio.Where( m => m.CodAreaGeoGio == area.CodAreaGeo ).ToList( );
                if ( frk_geogio.Count > 0 )
                {
                    descAreaGeo = frk_geogio.First( ).DesAreaGeoGio;
                }
                else
                {
                    descAreaGeo = null;
                }

                var frk_org = cvEnt.DAreaOrg.Where( m => m.CodAreaOrg == area.CodAreaOrg ).ToList( );
                if ( frk_org.Count > 0 )
                {
                    descAreaOrg = frk_org.First( ).DesAreaOrg;
                }
                else
                {
                    descAreaOrg = null;
                }

                var frk_tipodisto = cvEnt.DTipoDispo.Where( m => m.CodTipoDispo == area.CodTipoDispo ).ToList( );
                if ( frk_tipodisto.Count > 0 )
                {
                    descTipoDispo = frk_tipodisto.First( ).DescTipoDispo;
                }
                else
                {
                    descTipoDispo = null;
                }

                var frk_servizio = cvEnt.VDServizio.Where( m => m.Codice == area.CodServizio ).ToList( );
                if ( frk_servizio.Count > 0 )
                {
                    descServizio = frk_servizio.First( ).Descrizione;
                }
                else
                {
                    descServizio = null;
                }
                frk._descAreaGeo = descAreaGeo;
                frk._descAreaOrg = descAreaOrg;
                frk._descServizio = descServizio;
                frk._descTipoDispo = descTipoDispo;

                cvBox.areeInteresse.Add( frk );
            }
            //---------------------------------
            ViewBag.idMenu = 22;
            return View( "~/Views/CurriculumVitae/AreeInteresse/Index.cshtml" , cvBox );
        }

        [HttpPost]
        public ActionResult InsertAreeInteresse ( cvModel.AreeInteresse areeInteresse )
        {
            string result = "";

            if ( ModelState.IsValid )
            {
                string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
                int prog;
                cv_ModelEntities cvEnt = new cv_ModelEntities( );
                TCVAreaIntAz interesse = new TCVAreaIntAz( );

                interesse.AreeIntDispo = areeInteresse._areeIntDispo;
                interesse.CodAreaGeo = areeInteresse._codAreaGeo;
                interesse.CodAreaOrg = areeInteresse._codAreaOrg;
                interesse.CodServizio = areeInteresse._codServizio.Trim( );
                interesse.CodTipoDispo = areeInteresse._codTipoDispo;
                interesse.DataOraAgg = DateTime.Now;
                interesse.FlagEsteroDispo = ( areeInteresse._flagEsteroDispo == "S" ) ? "S" : "";
                interesse.Matricola = matricola;
                interesse.ProfIntDispo = areeInteresse._profIntDispo;
                interesse.Stato = "S";
                interesse.TipoAgg = "I";

                var tmp = cvEnt.TCVAreaIntAz.Where( x => x.Matricola == matricola );
                if ( tmp.Count( ) == 0 )
                {
                    prog = 1;
                }
                else
                {
                    var nro_prog = ( cvEnt.TCVAreaIntAz.Where( x => x.Matricola == matricola ) ).Max( x => x.Prog );
                    prog = Convert.ToInt32( nro_prog ) + 1;
                }
                interesse.Prog = prog;

                try
                {
                    cvEnt.TCVAreaIntAz.Add( interesse );
                    cvEnt.SaveChanges( );
                    result = "ok";
                }
                catch ( Exception ex )
                {
                    result = "error";
                }
            }
            else
            {
                result = "invalid";
            }

            return Content( result );
        }

        [HttpGet]
        public ActionResult DeleteAreeInteresse ( string matricola , int prog )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );

            if ( matricola == null || prog == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            TCVAreaIntAz interesse = new TCVAreaIntAz( );
            interesse = cvEnt.TCVAreaIntAz.Find( matricola , prog );
            if ( interesse == null )
            {
                return HttpNotFound( );
            }
            try
            {
                cvEnt.TCVAreaIntAz.Remove( interesse );
                cvEnt.SaveChanges( );
            }
            catch ( Exception ex )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            ViewBag.idMenu = 22;
            return RedirectToAction( "AreeInteresse" );
        }

        [HttpPost]
        public ActionResult Create_ViewAreeInteresse ( cvModel.AreeInteresse interesse )
        {
            string matricola = interesse._matricola;
            int prog = interesse._prog;

            if ( matricola == null || prog == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            return View( "~/Views/CurriculumVitae/AreeInteresse/ViewAreeInteresse.cshtml" , interesse );
        }


        [HttpPost]
        public ActionResult ModificaAreeInteresse ( cvModel.AreeInteresse interesse )
        {
            string result = "";
            //implementare
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            TCVAreaIntAz frk_interesse = new TCVAreaIntAz( );

            string matricola;
            int prog;

            matricola = interesse._matricola;
            prog = interesse._prog;

            frk_interesse = cvEnt.TCVAreaIntAz.Find( matricola , prog );
            if ( frk_interesse == null )
            {
                return HttpNotFound( );
            }

            if ( ModelState.IsValid )
            {
                frk_interesse.Matricola = matricola;
                frk_interesse.Prog = prog;
                frk_interesse.AreeIntDispo = interesse._areeIntDispo;
                frk_interesse.CodAreaGeo = interesse._codAreaGeo;
                frk_interesse.CodAreaOrg = interesse._codAreaOrg;
                frk_interesse.CodServizio = interesse._codServizio.Trim( );
                frk_interesse.CodTipoDispo = interesse._codTipoDispo;
                frk_interesse.DataOraAgg = DateTime.Now;
                frk_interesse.FlagEsteroDispo = ( interesse._flagEsteroDispo == "S" ) ? "S" : "";
                frk_interesse.ProfIntDispo = interesse._profIntDispo;
                frk_interesse.Stato = "S";
                frk_interesse.TipoAgg = "A";

                try
                {
                    cvEnt.SaveChanges( );
                    result = "ok";
                }
                catch ( Exception ex )
                {
                    result = "error";
                }
            }
            else
            {
                result = "invalid";
            }
            return Content( result );
        }
        #endregion

        #region Controller CompetenzeDigitali

        public ActionResult CompetenzeDigitali ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.competenzeDigitali = new List<cvModel.CompetenzeDigitali>( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            List<string> codici = new List<string> { "010" , "020" , "030" , "040" , "050" };

            var lista = this.GetListaBox( cvEnt );

            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }

            //carico il modello CvModel.competenzeDigitali
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;

            if ( cvEnt.TCVCompDigit.Where( m => m.Matricola == matricola ).ToList( ).Count == 0 )
            {
                //inserisco i primi record di default
                foreach ( var elem in codici )
                {
                    TCVCompDigit comp = new TCVCompDigit( );
                    comp.Matricola = matricola;
                    comp.CodCompDigit = elem;
                    comp.CodCompDigitLiv = "";
                    comp.DataOraAgg = DateTime.Now;
                    comp.Stato = "S";
                    comp.TipoAgg = "I";

                    try
                    {
                        cvEnt.TCVCompDigit.Add( comp );
                        cvEnt.SaveChanges( );
                    }
                    catch ( Exception exc )
                    {
                        return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                    }
                }
            }
            List<TCVCompDigit> competenze = cvEnt.TCVCompDigit.Where( m => m.Matricola == matricola ).ToList( );

            foreach ( var elem in competenze )
            {
                cvModel.CompetenzeDigitali frk_comepetenze = new cvModel.CompetenzeDigitali( );
                string descCompDigit, descCompDigitLiv, descCompDigitLivLunga;
                string codCompDigit, codCompDigitLiv;

                frk_comepetenze._codCompDigit = elem.CodCompDigit;
                frk_comepetenze._matricola = matricola;
                frk_comepetenze._stato = elem.Stato;
                frk_comepetenze._tipoAgg = elem.TipoAgg;
                frk_comepetenze._dataOraAgg = elem.DataOraAgg;
                frk_comepetenze._codCompDigitLiv = elem.CodCompDigitLiv;
                if ( elem.CodCompDigitLiv == "" )
                {
                    descCompDigitLiv = "";
                    descCompDigitLivLunga = "";
                }
                else
                {
                    descCompDigitLiv = ( from compDigitLiv in cvEnt.DCompDigitLiv
                                         where ( compDigitLiv.CodCompDigit == elem.CodCompDigit && compDigitLiv.CodCompDigitLiv == elem.CodCompDigitLiv )
                                         select compDigitLiv.DescCompDigitLiv ).First( ).ToString( );
                    descCompDigitLivLunga = ( from compDigitLivLunga in cvEnt.DCompDigitLiv
                                              where ( compDigitLivLunga.CodCompDigit == elem.CodCompDigit && compDigitLivLunga.CodCompDigitLiv == elem.CodCompDigitLiv )
                                              select compDigitLivLunga.DescCompDigitLivLunga ).First( ).ToString( );
                }
                descCompDigit = ( from compDigit in cvEnt.DCompDigit
                                  where compDigit.CodCompDigit == elem.CodCompDigit
                                  select compDigit.DescCompDigit ).First( ).ToString( );
                frk_comepetenze._descCompDigit = descCompDigit;
                frk_comepetenze._descCompDigitLiv = descCompDigitLiv;
                frk_comepetenze._descCompDigitLivLunga = descCompDigitLivLunga;

                //---------------------------------------
                cvBox.competenzeDigitali.Add( frk_comepetenze );
            }
            //--------------------------------------------
            ViewBag.idMenu = 39;
            return View( "~/Views/CurriculumVitae/CompetenzeDigitali/Index.cshtml" , cvBox );
        }

        [HttpPost]
        public ActionResult InsertCompetenzeDigitali ( string[] compDigit , string[] compDigitLiv )
        {
            string result;
            string matricola;
            cv_ModelEntities cvEnt = new cv_ModelEntities( );

            matricola = UtenteHelper.EsponiAnagrafica( )._matricola;

            if ( ModelState.IsValid )
            {
                for ( var i = 0 ; i < compDigit.Count( ) ; i++ )
                {
                    string codCompDigit, codCompDigitLiv;
                    codCompDigit = compDigit[i];
                    codCompDigitLiv = compDigitLiv[i];
                    TCVCompDigit frk_competenze = new TCVCompDigit( );
                    frk_competenze = cvEnt.TCVCompDigit.Find( matricola , codCompDigit );
                    frk_competenze.CodCompDigitLiv = codCompDigitLiv;
                    frk_competenze.DataOraAgg = DateTime.Now;
                    frk_competenze.Stato = "S";
                    frk_competenze.TipoAgg = "A";

                    try
                    {
                        cvEnt.SaveChanges( );
                    }
                    catch ( Exception exc )
                    {
                        return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                    }
                }
                result = "ok";
            }
            else
            {
                result = "invalid";
            }
            return Content( result );
        }

        #endregion

        #region Controller Formazione

        public ActionResult Formazione ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.formazione = new List<cvModel.Formazione>( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );

            var lista = this.GetListaBox( cvEnt );
            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            List<TCVFormExRai> formaz = new List<TCVFormExRai>( );

            formaz = cvEnt.TCVFormExRai.Where( x => x.Matricola == matricola ).ToList( );
            foreach ( var elem in formaz )
            {
                cvModel.Formazione frk_formazione = new cvModel.Formazione( CommonHelper.GetCurrentUserMatricola( ) );

                frk_formazione._anno = elem.Anno;
                frk_formazione._codNazione = elem.CodNazione;
                frk_formazione._corso = elem.Corso;
                frk_formazione._dataOraAgg = elem.DataOraAgg;
                frk_formazione._descNazione = ( from naz in cvEnt.DNazione
                                                where naz.COD_SIGLANAZIONE == elem.CodNazione
                                                select naz.DES_NAZIONE ).ToString( );
                frk_formazione._durata = elem.Durata;
                frk_formazione._localitaCorso = elem.LocalitaCorso;
                frk_formazione._matricola = elem.Matricola;
                frk_formazione._note = elem.Note;
                frk_formazione._presso = elem.Presso;
                frk_formazione._prog = elem.Prog;
                frk_formazione._stato = elem.Stato;
                frk_formazione._tipoAgg = elem.TipoAgg;

                cvBox.formazione.Add( frk_formazione );
            }

            //carico il modello CvModel.formazione

            //------------------------------------

            ViewBag.idMenu = 19;
            return View( "~/Views/CurriculumVitae/Formazione/Index.cshtml" , cvBox );
        }

        [HttpPost]
        public ActionResult InsertFormazione ( cvModel.Formazione frmz )
        {
            string result = "";
            int prog;

            if ( ModelState.IsValid )
            {
                string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
                cv_ModelEntities cvEnt = new cv_ModelEntities( );
                TCVFormExRai formazione = new TCVFormExRai( );

                formazione.Anno = frmz._anno;
                formazione.CodNazione = frmz._codNazione;
                formazione.Corso = frmz._corso;
                formazione.DataOraAgg = DateTime.Now;
                formazione.Durata = frmz._durata;
                formazione.LocalitaCorso = frmz._localitaCorso;
                formazione.Matricola = matricola;
                formazione.Note = frmz._note;
                formazione.Presso = frmz._presso;
                formazione.Stato = "S";
                formazione.TipoAgg = "I";

                //freak - Gestione de campo Prog
                var tmp = cvEnt.TCVFormExRai.Where( x => x.Matricola == matricola );
                if ( tmp.Count( ) == 0 )
                {
                    prog = 1;
                }
                else
                {
                    var nro_prog = ( cvEnt.TCVFormExRai.Where( x => x.Matricola == matricola ) ).Max( x => x.Prog );
                    prog = Convert.ToInt32( nro_prog ) + 1;
                }
                formazione.Prog = prog;
                //-----------------------------

                try
                {
                    cvEnt.TCVFormExRai.Add( formazione );
                    cvEnt.SaveChanges( );
                    result = "ok";
                }
                catch ( Exception ex )
                {
                    result = ex.Message;
                }
            }
            else
            {
                result = "invalid";
            }

            return Content( result );
        }

        [HttpGet]
        public ActionResult DeleteFormazione ( string matricola , int prog )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );

            if ( ( matricola == null ) && ( prog < 0 ) )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            TCVFormExRai master = cvEnt.TCVFormExRai.Find( matricola , prog );
            if ( master == null )
            {
                return HttpNotFound( );
            }
            try
            {
                cvEnt.TCVFormExRai.Remove( master );
                cvEnt.SaveChanges( );
            }
            catch ( Exception exc )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            //FREAK - RIEMPIMENTO DEL MODELLO CVBOX E DELLE LISTE SPECIALIZZAZIONI E ISTRUZIONE
            //---------------------------------
            ViewBag.idMenu = 19;
            return RedirectToAction( "Formazione" );
            //FINEE
        }

        [HttpPost]
        public ActionResult Create_ViewFormazione ( cvModel.Formazione formazione )
        {
            string matricola = formazione._matricola;
            int prog = formazione._prog;

            if ( matricola == null || prog == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            return View( "~/Views/CurriculumVitae/Formazione/ViewFormazione.cshtml" , formazione );
        }

        [HttpPost]
        public ActionResult ModificaFormazione ( cvModel.Formazione frmz )
        {
            string result = "";

            if ( ModelState.IsValid )
            {
                string matricola = frmz._matricola;
                int prog = frmz._prog;

                cv_ModelEntities cvEnt = new cv_ModelEntities( );
                TCVFormExRai formazione = new TCVFormExRai( );

                formazione = cvEnt.TCVFormExRai.Find( matricola , prog );

                if ( formazione == null )
                {
                    return HttpNotFound( );
                }

                formazione.Anno = frmz._anno;
                formazione.CodNazione = frmz._codNazione;
                formazione.Corso = frmz._corso;
                formazione.DataOraAgg = DateTime.Now;
                formazione.Durata = frmz._durata;
                formazione.LocalitaCorso = frmz._localitaCorso;
                formazione.Note = frmz._note;
                formazione.Presso = frmz._presso;
                formazione.Stato = "S";
                formazione.TipoAgg = "A";

                try
                {
                    cvEnt.SaveChanges( );
                    result = "ok";
                }
                catch ( Exception ex )
                {
                    result = ex.Message;
                }
            }
            else
            {
                result = "invalid";
            }

            return Content( result );
        }


        #endregion

        #region Controller ConoscenzeInformatiche

        public ActionResult knowledges ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.conoscenzeInformatiche = new List<cvModel.ConoscenzeInformatiche>( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            var lista = this.GetListaBox( cvEnt );
            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }

            //carico il modello CvModel.conoscenzeInformatiche
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            List<TCVConInfo> TCVConInfo = new List<TCVConInfo>( );

            TCVConInfo = cvEnt.TCVConInfo.Where( x => x.Matricola == matricola ).ToList( );
            var allList = from conInfo in cvEnt.DConInfo
                          join gruppoConInfo in cvEnt.DGruppoConInfo on conInfo.CodGruppoConInfo equals gruppoConInfo.CodGruppoConInfo
                          select new
                          {
                              CodConInfo = conInfo.CodConInfo ,
                              DescInfo = conInfo.DescConInfo ,
                              CodGruppoConInfo = conInfo.CodGruppoConInfo ,
                              CodPosizione = conInfo.CodPosizione ,
                              DescGruppoConInfo = gruppoConInfo.DescGruppoConInfo
                          };


            foreach ( var elem in allList )
            {
                cvModel.ConoscenzeInformatiche know = new cvModel.ConoscenzeInformatiche( );
                know._codConInfo = elem.CodConInfo;
                know._codGruppoConInfo = elem.CodGruppoConInfo;
                know._codPosizione = elem.CodPosizione;
                know._descConInfo = elem.DescInfo;
                know._descGruppoConInfo = elem.DescGruppoConInfo;

                var tmp = TCVConInfo.Where( x => x.CodConInfo == elem.CodConInfo );
                if ( elem.CodGruppoConInfo != "99" )
                {
                    if ( tmp.Count( ) > 0 )
                    {
                        know._selectedConInfo = true;
                        know._altraConInfo = tmp.First( ).AltraConInfo;
                        know._codConInfoLiv = tmp.First( ).CodConInfoLiv;
                        know._note = tmp.First( ).Note;
                        know._matricola = tmp.First( ).Matricola;
                        know._prog = tmp.First( ).Prog;
                    }
                    else
                    {
                        know._selectedConInfo = false;
                        know._altraConInfo = "";
                        know._codConInfoLiv = "";
                        know._note = "";
                        know._matricola = "";
                        know._prog = 0;
                    }
                }
                cvBox.conoscenzeInformatiche.Add( know );
            }

            var tmp_lista9999 = TCVConInfo.Where( x => x.CodConInfo == "9999" ).ToList( );
            foreach ( var elem_2 in tmp_lista9999 )
            {
                cvModel.ConoscenzeInformatiche know = new cvModel.ConoscenzeInformatiche( );

                know._altraConInfo = elem_2.AltraConInfo;
                know._codConInfo = elem_2.CodConInfo;
                know._codConInfoLiv = elem_2.CodConInfoLiv;
                know._codGruppoConInfo = "99";
                know._dataOraAgg = elem_2.DataOraAgg;
                know._matricola = elem_2.Matricola;
                know._note = elem_2.Note;
                know._prog = elem_2.Prog;
                know._selectedConInfo = true;
                know._stato = elem_2.Stato;
                know._tipoAgg = elem_2.TipoAgg;

                cvBox.conoscenzeInformatiche.Add( know );
            }

            ViewBag.idMenu = 21;
            return View( "~/Views/CurriculumVitae/knowledges/Index.cshtml" , cvBox );
        }

        [HttpPost]
        public ActionResult EditConoscenzeInformatiche ( cvModel.ConoscenzeInformatiche[] know )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            List<TCVConInfo> delete_list = cvEnt.TCVConInfo.Where( x => x.Matricola == matricola ).ToList( );
            foreach ( TCVConInfo delete in delete_list )
            {
                try
                {
                    cvEnt.TCVConInfo.Remove( delete );
                    cvEnt.SaveChanges( );
                }
                catch ( Exception ex )
                {
                    return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                }
            }

            var lista_know = know.Where( x => x._selectedConInfo == true ).ToList( );

            foreach ( var item_coninfo in lista_know.Where( x => x._codConInfo != "9999" ) )
            {
                TCVConInfo ConInfo = new TCVConInfo( );

                ConInfo.AltraConInfo = item_coninfo._altraConInfo;
                ConInfo.CodConInfo = item_coninfo._codConInfo;
                ConInfo.CodConInfoLiv = item_coninfo._codConInfoLiv;
                ConInfo.DataOraAgg = DateTime.Now;
                ConInfo.Matricola = matricola;
                ConInfo.Note = item_coninfo._note;
                ConInfo.Prog = 1;
                ConInfo.Stato = "S";
                ConInfo.TipoAgg = "I";

                try
                {
                    cvEnt.TCVConInfo.Add( ConInfo );
                    cvEnt.SaveChanges( );
                }
                catch ( Exception ex )
                {
                    return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                }
            }

            int prog = 0;
            foreach ( var item_infoextra in lista_know.Where( x => x._codConInfo == "9999" ) )
            {
                TCVConInfo ConInfo = new TCVConInfo( );
                prog++;
                ConInfo.AltraConInfo = item_infoextra._altraConInfo;
                ConInfo.CodConInfo = item_infoextra._codConInfo;
                ConInfo.CodConInfoLiv = item_infoextra._codConInfoLiv;
                ConInfo.DataOraAgg = DateTime.Now;
                ConInfo.Matricola = matricola;
                ConInfo.Note = item_infoextra._note;
                ConInfo.Prog = prog;
                ConInfo.Stato = "S";
                ConInfo.TipoAgg = "I";

                try
                {
                    cvEnt.TCVConInfo.Add( ConInfo );
                    cvEnt.SaveChanges( );
                }
                catch ( Exception ex )
                {
                    return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                }
            }
            return RedirectToAction( "knowledges" );
        }

        #endregion

        #region AltreInfo

        public ActionResult AltreInfo ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.altreInformazioni = new cvModel.AltreInfo( CommonHelper.GetCurrentUserMatricola( ) );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            var lista = this.GetListaBox( cvEnt );
            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }

            //carico il modello CvModel.studies
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;

            List<TCVAltreInfPat> listaPatenti = cvEnt.TCVAltreInfPat.Where( x => x.Matricola == matricola ).ToList( );
            TCVAltreInf altreInformazioni = cvEnt.TCVAltreInf.Where( x => x.Matricola == matricola ).First( );

            if ( altreInformazioni != null )
            {
                cvModel.AltreInfo frk_altreInfo = new cvModel.AltreInfo( CommonHelper.GetCurrentUserMatricola( ) );

                /*
                 * 
                 * FREAK - INSERIRE LA PARTE RELATIVA AI DATI DI DOMICILIO E RESIDENZA, PRESI DA O UN SERVIZIO 
                 * O DA UNA STORED PROCEDURE
                 * 
                 * */
                frk_altreInfo._dataOraAgg = altreInformazioni.DataOraAgg;
                frk_altreInfo._email = altreInformazioni.EMail;
                frk_altreInfo._matricola = matricola;
                frk_altreInfo._note = altreInformazioni.Note;
                frk_altreInfo._numTel1 = altreInformazioni.NumTel1;
                frk_altreInfo._numTel2 = altreInformazioni.NumTel2;
                frk_altreInfo._sitoWeb = altreInformazioni.Sitoweb;
                frk_altreInfo._stato = altreInformazioni.Stato;
                frk_altreInfo._tipoAgg = altreInformazioni.TipoAgg;
                frk_altreInfo._tipoTel1 = altreInformazioni.TipoTel1;
                frk_altreInfo._tipoTel2 = altreInformazioni.TipoTel2;

                frk_altreInfo._tipoPatente = new List<DTipoPatente>( );

                if ( listaPatenti.Count > 0 )
                {
                    foreach ( var elem in listaPatenti )
                    {
                        DTipoPatente item = new DTipoPatente( );
                        item = cvEnt.DTipoPatente.Where( x => x.CodTipoPatente == elem.CodTipoPatente ).First( );
                        frk_altreInfo._tipoPatente.Add( item );
                    }
                }
                else
                {
                    frk_altreInfo._tipoPatente = null;
                }
                cvBox.altreInformazioni = frk_altreInfo;
            }
            //---------------------------------
            ViewBag.idMenu = 18;
            return View( "~/Views/CurriculumVitae/AltreInfo/Index.cshtml" , cvBox );
        }

        public ActionResult InsertAltreInfo ( cvModel.AltreInfo altreInfo , string[] tipoPatente )
        {
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;

            cv_ModelEntities cvEnt = new cv_ModelEntities( );

            //FREAK - INSERIMENTO O MODIFICA DEL RECORD NELLA TABELLA TCVALTREINF E TCVALTREINFPAT
            TCVAltreInf elemAltreInfo = cvEnt.TCVAltreInf.Find( matricola );

            if ( elemAltreInfo != null )
            {
                //Devo fare un Update
                elemAltreInfo.DataOraAgg = DateTime.Now;
                elemAltreInfo.EMail = altreInfo._email;
                elemAltreInfo.Note = altreInfo._note;
                elemAltreInfo.NumTel1 = altreInfo._numTel1;
                elemAltreInfo.NumTel2 = altreInfo._numTel2;
                elemAltreInfo.Sitoweb = altreInfo._sitoWeb;
                elemAltreInfo.Stato = "S";
                elemAltreInfo.TipoAgg = "I";
                elemAltreInfo.TipoTel1 = altreInfo._tipoTel1;
                elemAltreInfo.TipoTel2 = altreInfo._tipoTel2;

                try
                {
                    cvEnt.SaveChanges( );
                }
                catch ( Exception ex )
                {
                    return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                }
                //inserisco i dati nella tabella TCVAltreInfPat
                //cancello prima tutti gli elementi
                List<TCVAltreInfPat> listaPatenti = cvEnt.TCVAltreInfPat.Where( x => x.Matricola == matricola ).ToList( );
                try
                {
                    foreach ( var item in listaPatenti )
                    {

                        cvEnt.TCVAltreInfPat.Remove( item );
                    }
                    cvEnt.SaveChanges( );
                }
                catch ( Exception ex )
                {
                    return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                }
                //inserisco i nuovi dati
                foreach ( var codPatenti in tipoPatente )
                {
                    TCVAltreInfPat elem_insert = new TCVAltreInfPat( );
                    elem_insert.CodTipoPatente = codPatenti;
                    elem_insert.Matricola = matricola;

                    try
                    {
                        cvEnt.TCVAltreInfPat.Add( elem_insert );
                        cvEnt.SaveChanges( );
                    }
                    catch ( Exception ex )
                    {
                        return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                    }
                }

            }
            else
            {
                //devo fare un Insert
                TCVAltreInf insAltreInfo = new TCVAltreInf( );
                insAltreInfo.DataOraAgg = DateTime.Now;
                insAltreInfo.Matricola = matricola;
                insAltreInfo.EMail = altreInfo._email;
                insAltreInfo.Note = altreInfo._note;
                insAltreInfo.NumTel1 = altreInfo._numTel1;
                insAltreInfo.NumTel2 = altreInfo._numTel2;
                insAltreInfo.Sitoweb = altreInfo._sitoWeb;
                insAltreInfo.Stato = "S";
                insAltreInfo.TipoAgg = "A";
                insAltreInfo.TipoTel1 = altreInfo._tipoTel1;
                insAltreInfo.TipoTel2 = altreInfo._tipoTel2;

                try
                {
                    cvEnt.TCVAltreInf.Add( elemAltreInfo );
                    cvEnt.SaveChanges( );
                }
                catch ( Exception ex )
                {
                    return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                }
                //inserisco i nuovi dati
                try
                {
                    foreach ( var codPatenti in tipoPatente )
                    {
                        TCVAltreInfPat elem_insert = new TCVAltreInfPat( );
                        elem_insert.CodTipoPatente = codPatenti;
                        elem_insert.Matricola = matricola;

                        cvEnt.TCVAltreInfPat.Add( elem_insert );
                        cvEnt.SaveChanges( );
                    }
                }
                catch ( Exception ex )
                {
                    return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                }
            }
            return RedirectToAction( "AltreInfo" );
        }

        #endregion

        #region ImpegniEditoriali RAI

        public ActionResult ImpegniRAI ( )
        {
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;

            //FREAK - da cancellare dopo il test
            matricola = "069894";

            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.impegniEditoriali = new List<cvModel.ImpegniRAI>( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            var lista = this.GetListaBox( cvEnt );
            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }
            //---------------------------------

            //carico la lista impegniEditoriali 
            int prog = 0;
            List<TCVEsperProd> esperProd = cvEnt.TCVEsperProd.Where( x => x.MATRICOLA == matricola ).ToList( );
            foreach ( var elem in esperProd )
            {
                string tmp_ruolo;
                cvModel.ImpegniRAI impegniRai = new cvModel.ImpegniRAI( );
                prog++;
                impegniRai._desTitoloDefinit = elem.DES_TITOLO_DEFINIT;
                impegniRai._idEsperienze = elem.ID_ESPERIENZE;
                impegniRai._matricola = matricola;
                impegniRai._matricolaSpett = elem.COD_MATRICOLA;
                impegniRai._progDaStampare = prog;
                tmp_ruolo = cvEnt.DConProf.Where( x => x.CodConProf == elem.COD_RUOLO ).First( ).DescConProf;
                //gestione date
                impegniRai._ruolo = tmp_ruolo;
                impegniRai._dtDataInizio = elem.INIZIO_PERIODO_ESP.Substring( 6 , 2 ) + "/" + elem.INIZIO_PERIODO_ESP.Substring( 4 , 2 ) + "/" + elem.INIZIO_PERIODO_ESP.Substring( 0 , 4 );
                impegniRai._dtDataFine = elem.FINE_PERIODO_ESP.Substring( 6 , 2 ) + "/" + elem.FINE_PERIODO_ESP.Substring( 4 , 2 ) + "/" + elem.FINE_PERIODO_ESP.Substring( 0 , 4 );

                cvBox.impegniEditoriali.Add( impegniRai );
            }

            ViewBag.idMenu = 41;
            return View( "~/Views/CurriculumVitae/ImpegniRAI/Index.cshtml" , cvBox );
        }

        #endregion

        #region Controller CompetenzeRAI

        public ActionResult CompetenzeRAI ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.competenzeRai = new List<cvModel.CompetenzeRAI>( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            var lista = this.GetListaBox( cvEnt );
            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }

            //carico il modello CvModel.competenzeRai
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            //prelevo la figura professionale
            string figura_profesisonale = UtenteHelper.EsponiAnagrafica( )._codiceFigProf;
            //freak - dopo cancellare. E' per test
            figura_profesisonale = "XAA";
            //-----------------------------------
            List<TCVConProf> tcvConProf = cvEnt.TCVConProf.Where( x => x.Matricola == matricola ).ToList( );
            List<DConProf> dConProf = cvEnt
                .DConProf.Where( x => x.FiguraProfessionale == figura_profesisonale
                     && x.Stato != "1"
                ).ToList( );

            foreach ( var elem in dConProf )
            {
                cvModel.CompetenzeRAI frk_compRai = new cvModel.CompetenzeRAI( );

                frk_compRai._codConProf = elem.CodConProf;
                frk_compRai._descConProf = elem.DescConProf;
                frk_compRai._figuraProfessionale = elem.FiguraProfessionale;
                frk_compRai._matricola = matricola;
                frk_compRai.descrittiva_lunga = elem.DescConProfLunga;

                var tmp = tcvConProf.Where( x => x.CodConProf == elem.CodConProf );
                if ( tmp.Count( ) > 0 )
                {
                    frk_compRai._dataOraAgg = tmp.First( ).DataOraAgg;
                    frk_compRai._flagExtraRai = tmp.First( ).FlagExtraRai;
                    frk_compRai._flagPrincipale = tmp.First( ).FlagPrincipale;
                    frk_compRai._flagSecondario = tmp.First( ).FlagSecondario;
                    frk_compRai._stato = tmp.First( ).Stato;
                    frk_compRai._tipoAgg = tmp.First( ).TipoAgg;

                    if ( frk_compRai._flagPrincipale == "1" )
                    {
                        frk_compRai._flagChoice = "P";
                    }
                    else if ( frk_compRai._flagSecondario == "1" )
                    {
                        frk_compRai._flagChoice = "S";
                    }
                    else
                    {
                        frk_compRai._flagChoice = "";
                    }
                }
                else
                {
                    frk_compRai._dataOraAgg = null;
                    frk_compRai._flagChoice = "";
                    frk_compRai._flagExtraRai = "";
                    frk_compRai._flagPrincipale = "";
                    frk_compRai._flagSecondario = "";
                    frk_compRai._stato = "";
                    frk_compRai._tipoAgg = "";
                }

                cvBox.competenzeRai.Add( frk_compRai );
            }
            //---------------------------------
            ViewBag.idMenu = 42;
            return View( "~/Views/CurriculumVitae/CompetenzeRAI/Index.cshtml" , cvBox );
        }

        [HttpPost]
        public ActionResult EditCompetenzeRai ( cvModel.CompetenzeRAI[] compRai )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;

            //cancello tutti gli elementi in TCVConProf
            List<TCVConProf> delete_list = cvEnt.TCVConProf.Where( x => x.Matricola == matricola ).ToList( );
            foreach ( TCVConProf delete in delete_list )
            {
                try
                {
                    cvEnt.TCVConProf.Remove( delete );
                    cvEnt.SaveChanges( );
                }
                catch ( Exception ex )
                {
                    return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                }
            }
            //filtro la lista solo con gli elementi selezionati
            var lista_check = compRai.Where( x => x._codConProf != null ).ToList( );
            foreach ( var elem in lista_check )
            {
                string extraRai, principale, secondario;
                TCVConProf conProf = new TCVConProf( );

                conProf.AltraConProf = "";
                conProf.CodConProf = elem._codConProf;
                conProf.CodConProfLiv = "";
                conProf.DataOraAgg = DateTime.Now;
                if ( elem._flagExtraRai == "1" )
                {
                    extraRai = "1";
                }
                else
                {
                    extraRai = "0";
                }

                switch ( elem._flagChoice )
                {
                    case "P":
                        principale = "1";
                        secondario = "0";
                        break;
                    case "S":
                        principale = "0";
                        secondario = "1";
                        break;
                    default:
                        principale = "0";
                        secondario = "0";
                        break;
                }
                conProf.FlagExtraRai = extraRai;
                conProf.FlagPrincipale = principale;
                conProf.FlagSecondario = secondario;
                conProf.Matricola = matricola;
                conProf.Prog = 1;
                conProf.Stato = "S";
                conProf.TipoAgg = "I";

                try
                {
                    cvEnt.TCVConProf.Add( conProf );
                    cvEnt.SaveChanges( );
                }
                catch ( Exception ex )
                {
                    return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                }
            }
            return RedirectToAction( "CompetenzeRAI" );
        }

        #endregion

        #region Controller CompetenzeSpecialistiche

        public ActionResult CompetenzeSpecialistiche ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.competenzeSpecialistiche = new List<cvModel.CompetenzeSpecialistiche>( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            var lista = this.GetListaBox( cvEnt );
            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }

            //carico il modello CvModel.studies
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            string figura_professionale = UtenteHelper.EsponiAnagrafica( )._codiceFigProf;
            //freak - dopo cancellare. E' per test
            figura_professionale = "MBA";
            //-----------------------------------
            List<DConProf> DConProf = cvEnt.DConProf.Where( x => x.FiguraProfessionale == figura_professionale ).ToList( );
            List<TCVConProf> TCVConProf = cvEnt.TCVConProf.Where( x => x.Matricola == matricola ).ToList( );

            foreach ( var elem in DConProf )
            {
                string tmp_descConProf;
                cvModel.CompetenzeSpecialistiche frk_compSpec = new cvModel.CompetenzeSpecialistiche( );

                var tmp_tcvConProf = TCVConProf.Where( x => x.CodConProf == elem.CodConProf );

                frk_compSpec._codConProf = elem.CodConProf;
                frk_compSpec._codConProfAggr = elem.CodConProfAggr;
                frk_compSpec._dataOraAgg = null;
                frk_compSpec._descConProf = elem.DescConProf;
                frk_compSpec._descConProfLunga = elem.DescConProfLunga;
                frk_compSpec._figuraProfessionale = figura_professionale;
                //setto il flag _isSelected
                if ( tmp_tcvConProf.Count( ) > 0 )
                {
                    frk_compSpec._isSelected = true;
                    frk_compSpec._codConProfLiv = tmp_tcvConProf.First( ).CodConProfLiv;
                    frk_compSpec._flagPrincipale = tmp_tcvConProf.First( ).FlagPrincipale;
                }
                else
                {
                    frk_compSpec._isSelected = false;
                    frk_compSpec._codConProfLiv = null;
                }
                //setto il flag _isTitle
                if ( ( elem.DescConProf.Contains( "skill" ) ) )
                {
                    frk_compSpec._isTitle = true;
                }
                else
                {
                    frk_compSpec._isTitle = false;
                }
                frk_compSpec._matricola = matricola;
                frk_compSpec._posizione = Convert.ToInt32( elem.Posizione );
                frk_compSpec._prog = 1;
                frk_compSpec._stato = "";
                frk_compSpec._tipoAgg = "";

                cvBox.competenzeSpecialistiche.Add( frk_compSpec );
            }
            //---------------------------------
            ViewBag.idMenu = 43;
            return View( "~/Views/CurriculumVitae/CompetenzeSpecialistiche/Index.cshtml" , cvBox );
        }

        public ActionResult EditCompetenzeSpecialistiche ( cvModel.CompetenzeSpecialistiche[] compSpec , string[] flagPrinc )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;

            //cancello tutti gli elementi in TCVConProf
            List<TCVConProf> delete_list = cvEnt.TCVConProf.Where( x => x.Matricola == matricola ).ToList( );
            foreach ( TCVConProf delete in delete_list )
            {
                try
                {
                    cvEnt.TCVConProf.Remove( delete );
                    cvEnt.SaveChanges( );
                }
                catch ( Exception ex )
                {
                    return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                }
            }

            List<cvModel.CompetenzeSpecialistiche> listaTot = compSpec.Where( x => x._codConProfLiv != null || flagPrinc.Contains( x._codConProf ) ).ToList( );
            foreach ( var item in listaTot )
            {
                TCVConProf conProf = new TCVConProf( );

                conProf.CodConProf = item._codConProf;
                conProf.CodConProfLiv = item._codConProfLiv;
                conProf.DataOraAgg = DateTime.Now;
                conProf.Matricola = matricola;
                conProf.Prog = 1;
                conProf.Stato = "S";
                conProf.TipoAgg = "I";

                if ( flagPrinc.Contains( item._codConProf ) )
                {
                    conProf.FlagPrincipale = "1";
                }
                else
                {
                    conProf.FlagPrincipale = "0";
                }

                try
                {
                    cvEnt.TCVConProf.Add( conProf );
                    cvEnt.SaveChanges( );
                }
                catch ( Exception ex )
                {
                    return new HttpStatusCodeResult( HttpStatusCode.InternalServerError );
                }
            }
            return RedirectToAction( "CompetenzeSpecialistiche" );
        }

        #endregion

        #region Controller Certificazioni
        public ActionResult Certificazioni ( )
        {
            cvBox.certificazioni = new List<cvModel.Certificazioni>( );
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            var matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            var certificazioni = cvEnt.TCVCertifica.Where( c => c.Matricola == matricola );
            var descalb = cvEnt.DAlboProf.ToList( );
            foreach ( var certif in certificazioni )
            {
                string dataini = null;
                string datafin = null;
                string datapub = null;
                string databrev = null;
                string dataalbo = null;
                if ( certif.MeseIni != null )
                {
                    var mese = new DateTime( Convert.ToInt16( certif.AnnoIni ) , Convert.ToInt16( certif.MeseIni ) , 01 ).ToString( "MMMM" );
                    dataini = mese.Substring( 0 , 1 ).ToUpper( ) + mese.Substring( 1 ) + " " + certif.AnnoIni;
                }
                if ( certif.MeseFin != null )
                {
                    var mese = new DateTime( Convert.ToInt16( certif.AnnoFin ) , Convert.ToInt16( certif.MeseFin ) , 01 ).ToString( "MMMM" );
                    datafin = mese.Substring( 0 , 1 ).ToUpper( ) + mese.Substring( 1 ) + " " + certif.AnnoFin;
                }
                if ( certif.DataPubblica != null )
                {
                    datapub = new DateTime( Convert.ToInt16( certif.DataPubblica.Substring( 0 , 4 ) ) , Convert.ToInt16( certif.DataPubblica.Substring( 4 , 2 ) ) , Convert.ToInt16( certif.DataPubblica.Substring( 6 , 2 ) ) ).ToString( "dd/MM/yyyy" );
                }
                if ( certif.DataBrevetto != null )
                {
                    databrev = new DateTime( Convert.ToInt16( certif.DataBrevetto.Substring( 0 , 4 ) ) , Convert.ToInt16( certif.DataBrevetto.Substring( 4 , 2 ) ) , Convert.ToInt16( certif.DataBrevetto.Substring( 6 , 2 ) ) ).ToString( "dd/MM/yyyy" );
                }
                if ( certif.DataAlboProf != null )
                {
                    dataalbo = new DateTime( Convert.ToInt16( certif.DataAlboProf.Substring( 0 , 4 ) ) , Convert.ToInt16( certif.DataAlboProf.Substring( 4 , 2 ) ) , Convert.ToInt16( certif.DataAlboProf.Substring( 6 , 2 ) ) ).ToString( "dd/MM/yyyy" );
                }
                cvModel.Certificazioni cer = new cvModel.Certificazioni( CommonHelper.GetCurrentUserMatricola() )
                {
                    _annoFin = certif.AnnoFin ,
                    _annoIni = certif.AnnoIni ,
                    _autCertifica = certif.AutCertifica ,
                    _codAlboProf = certif.CodAlboProf ,
                    _dataAlboProf = dataalbo ,
                    _dataBrevetto = databrev ,
                    _dataOraAgg = certif.DataOraAgg ,
                    _dataPubblica = datapub ,
                    _descAlboProf = descalb.Where( x => x.CodAlboProf == certif.CodAlboProf ).Select( x => x.DescAlboProf ).FirstOrDefault( ) ,
                    _editorePubblica = certif.EditorePubblica ,
                    _flagRegBrevetto = certif.FlagRegBrevetto ,
                    _inventore = certif.Inventore ,
                    _matricola = certif.Matricola ,
                    _meseFin = certif.MeseFin ,
                    _meseIni = certif.MeseIni ,
                    _dataIni = dataini ,
                    _dataFin = datafin ,
                    _nomeCertifica = certif.NomeCertifica ,
                    _noteAlboProf = certif.NoteAlboProf ,
                    _noteBrevetto = certif.NoteBrevetto ,
                    _notePubblica = certif.NotePubblica ,
                    _numBrevetto = certif.NumBrevetto ,
                    _numLicenza = certif.NumLicenza ,
                    _pressoAlboProf = certif.PressoAlboProf ,
                    _prog = certif.Prog ,
                    _tipo = certif.Tipo ,
                    _tipoAgg = certif.TipoAgg ,
                    _tipoBrevetto = certif.TipoBrevetto ,
                    _titoloPubblica = certif.TitoloPubblica ,
                    _uffBrevetto = certif.UffBrevetto ,
                    _urlBrevetto = certif.UrlBrevetto ,
                    _urlCertifica = certif.UrlCertifica ,
                    _urlPubblica = certif.UrlPubblica
                };

                cvBox.certificazioni.Add( cer );
            }

            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            var lista = this.GetListaBox( cvEnt );
            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }
            //---------------------------------
            ViewBag.idMenu = 40;
            return View( "~/Views/CurriculumVitae/Certificazioni/Index.cshtml" , cvBox );
        }

        [HttpPost]
        public ActionResult Create_DettaglioCertificazione ( cvModel.Certificazioni certificazione )
        {
            return View( "~/Views/CurriculumVitae/Certificazioni/DettaglioCertificazione.cshtml" , certificazione );
        }

        [HttpPost]
        public ActionResult Create_DettaglioPubblicazione ( cvModel.Certificazioni certificazione )
        {
            return View( "~/Views/CurriculumVitae/Certificazioni/DettaglioPubblicazione.cshtml" , certificazione );
        }

        [HttpPost]
        public ActionResult Create_DettaglioBrevetto ( cvModel.Certificazioni certificazione )
        {
            return View( "~/Views/CurriculumVitae/Certificazioni/DettaglioBrevetto.cshtml" , certificazione );
        }

        [HttpPost]
        public ActionResult Create_DettaglioAlbo ( cvModel.Certificazioni certificazione )
        {
            return View( "~/Views/CurriculumVitae/Certificazioni/DettaglioAlbo.cshtml" , certificazione );
        }

        [HttpPost]
        public ActionResult InsertCertificazioni ( cvModel.Certificazioni certificazione )
        {
            string result = "";

            var ctr = this.VerificaObbligatori( certificazione );

            if ( ctr != null )
            {

                return Content( ctr );
            }

            string matricola;
            int prog;

            matricola = UtenteHelper.EsponiAnagrafica( )._matricola;

            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            TCVCertifica certdb = new TCVCertifica( );
            certdb.AnnoFin = certificazione._annoFin;
            certdb.AnnoIni = certificazione._annoIni;
            certdb.AutCertifica = certificazione._autCertifica;
            certdb.CodAlboProf = certificazione._codAlboProf;
            if ( certificazione._dataAlboProf != null )
            {
                certdb.DataAlboProf = certificazione._dataAlboProf.Substring( 6 , 4 ) + certificazione._dataAlboProf.Substring( 3 , 2 ) + certificazione._dataAlboProf.Substring( 0 , 2 );
            }
            if ( certificazione._dataBrevetto != null )
            {
                certdb.DataBrevetto = certificazione._dataBrevetto.Substring( 6 , 4 ) + certificazione._dataBrevetto.Substring( 3 , 2 ) + certificazione._dataBrevetto.Substring( 0 , 2 );
            }
            if ( certificazione._dataPubblica != null )
            {
                certdb.DataPubblica = certificazione._dataPubblica.Substring( 6 , 4 ) + certificazione._dataPubblica.Substring( 3 , 2 ) + certificazione._dataPubblica.Substring( 0 , 2 );
            }
            certdb.EditorePubblica = certificazione._editorePubblica;
            certdb.FlagRegBrevetto = certificazione._flagRegBrevetto;
            certdb.Inventore = certificazione._inventore;
            certdb.Matricola = matricola;
            certdb.MeseFin = certificazione._meseFin;
            certdb.MeseIni = certificazione._meseIni;
            certdb.NomeCertifica = certificazione._nomeCertifica;
            certdb.NoteAlboProf = certificazione._noteAlboProf;
            certdb.NoteBrevetto = certificazione._noteBrevetto;
            certdb.NotePubblica = certificazione._notePubblica;
            certdb.NumBrevetto = certificazione._numBrevetto;
            certdb.NumLicenza = certificazione._numLicenza;
            certdb.PressoAlboProf = certificazione._pressoAlboProf;
            certdb.TipoBrevetto = certificazione._tipoBrevetto;
            certdb.TitoloPubblica = certificazione._titoloPubblica;
            certdb.UffBrevetto = certificazione._uffBrevetto;
            certdb.UrlBrevetto = certificazione._urlBrevetto;
            certdb.UrlCertifica = certificazione._urlCertifica;
            certdb.UrlPubblica = certificazione._urlPubblica;
            certdb.DataOraAgg = DateTime.Now;
            certdb.TipoAgg = "I";
            certdb.Tipo = certificazione._tipo;
            //    //calcolo del prog
            var tmp = cvEnt.TCVCertifica.Where( x => x.Matricola == matricola );
            if ( tmp.Count( ) == 0 )
            {
                prog = 1;
            }
            else
            {
                var nro_prog = ( cvEnt.TCVCertifica.Where( x => x.Matricola == matricola ) ).Max( x => x.Prog );
                prog = Convert.ToInt32( nro_prog ) + 1;
            }
            certdb.Prog = prog;

            try
            {
                cvEnt.TCVCertifica.Add( certdb );
                cvEnt.SaveChanges( );
                result = "ok";
            }
            catch ( Exception exc )
            {
                result = exc.Message;
            }

            return Content( result );
        }

        [HttpGet]
        public ActionResult DeleteCertificazione ( string matricola , int prog )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );

            if ( ( matricola == null ) && ( prog < 0 ) )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            TCVCertifica cert = cvEnt.TCVCertifica.Find( matricola , prog );
            if ( cert == null )
            {
                return HttpNotFound( );
            }
            try
            {
                cvEnt.TCVCertifica.Remove( cert );
                cvEnt.SaveChanges( );
            }
            catch ( Exception )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            //FREAK - RIEMPIMENTO DEL MODELLO CVBOX E DELLE LISTE SPECIALIZZAZIONI E ISTRUZIONE
            //---------------------------------
            ViewBag.idMenu = 40;
            return RedirectToAction( "certificazioni" );
            //FINEE
        }

        [HttpPost]
        public ActionResult Create_ModificaDettaglioCertificazione ( cvModel.Certificazioni certificazione )
        {
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            int prog = certificazione._prog;

            if ( matricola == null || prog == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            switch ( certificazione._tipo )
            {
                case "1":
                    return View( "~/Views/CurriculumVitae/Certificazioni/ModificaDettaglioCertificazione.cshtml" , certificazione );

                case "2":
                    return View( "~/Views/CurriculumVitae/Certificazioni/ModificaDettaglioPubblicazione.cshtml" , certificazione );
                case "3":
                    return View( "~/Views/CurriculumVitae/Certificazioni/ModificaDettaglioBrevetto.cshtml" , certificazione );
                case "4":
                    return View( "~/Views/CurriculumVitae/Certificazioni/ModificaDettaglioAlbo.cshtml" , certificazione );
            }
            return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
        }

        [HttpPost]
        public ActionResult ModificaCertificazioni ( cvModel.Certificazioni certificazione )
        {
            string result = "";
            var ctr = this.VerificaObbligatori( certificazione );

            if ( ctr != null )
            {

                return Content( ctr );
            }
            string matricola = certificazione._matricola;
            int prog = certificazione._prog;

            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            TCVCertifica certdb = new TCVCertifica( );

            certdb = cvEnt.TCVCertifica.Find( matricola , prog );

            if ( certdb == null )
            {
                return HttpNotFound( );
            }
            certdb.AnnoFin = certificazione._annoFin;
            certdb.AnnoIni = certificazione._annoIni;
            certdb.AutCertifica = certificazione._autCertifica;
            certdb.CodAlboProf = certificazione._codAlboProf;
            if ( certificazione._dataAlboProf != null )
            {
                certdb.DataAlboProf = certificazione._dataAlboProf.Substring( 6 , 4 ) + certificazione._dataAlboProf.Substring( 3 , 2 ) + certificazione._dataAlboProf.Substring( 0 , 2 );
            }
            else
            {
                certdb.DataAlboProf = null;
            }
            if ( certificazione._dataBrevetto != null )
            {
                certdb.DataBrevetto = certificazione._dataBrevetto.Substring( 6 , 4 ) + certificazione._dataBrevetto.Substring( 3 , 2 ) + certificazione._dataBrevetto.Substring( 0 , 2 );
            }
            else
            {
                certdb.DataBrevetto = null;
            }
            if ( certificazione._dataPubblica != null )
            {
                certdb.DataPubblica = certificazione._dataPubblica.Substring( 6 , 4 ) + certificazione._dataPubblica.Substring( 3 , 2 ) + certificazione._dataPubblica.Substring( 0 , 2 );
            }
            else
            {
                certdb.DataPubblica = null;
            }
            certdb.EditorePubblica = certificazione._editorePubblica;
            certdb.FlagRegBrevetto = certificazione._flagRegBrevetto;
            certdb.Inventore = certificazione._inventore;
            certdb.Matricola = matricola;
            certdb.MeseFin = certificazione._meseFin;
            certdb.MeseIni = certificazione._meseIni;
            certdb.NomeCertifica = certificazione._nomeCertifica;
            certdb.NoteAlboProf = certificazione._noteAlboProf;
            certdb.NoteBrevetto = certificazione._noteBrevetto;
            certdb.NotePubblica = certificazione._notePubblica;
            certdb.NumBrevetto = certificazione._numBrevetto;
            certdb.NumLicenza = certificazione._numLicenza;
            certdb.PressoAlboProf = certificazione._pressoAlboProf;
            certdb.TipoBrevetto = certificazione._tipoBrevetto;
            certdb.TitoloPubblica = certificazione._titoloPubblica;
            certdb.UffBrevetto = certificazione._uffBrevetto;
            certdb.UrlBrevetto = certificazione._urlBrevetto;
            certdb.UrlCertifica = certificazione._urlCertifica;
            certdb.UrlPubblica = certificazione._urlPubblica;
            certdb.DataOraAgg = DateTime.Now;
            certdb.TipoAgg = "A";

            try
            {
                cvEnt.SaveChanges( );
                result = "ok";
            }
            catch ( Exception ex )
            {
                result = ex.Message;
            }

            return Content( result );
        }

        private string VerificaObbligatori ( cvModel.Certificazioni certificazione )
        {
            string error = null;
            if ( certificazione._tipo == "1" )
            {
                if ( string.IsNullOrWhiteSpace( certificazione._nomeCertifica ) || string.IsNullOrEmpty( certificazione._nomeCertifica ) )
                {
                    error += "campo nome certificazione obbligatorio;";
                }

                if ( string.IsNullOrWhiteSpace( certificazione._dataIni ) || string.IsNullOrEmpty( certificazione._dataIni ) )
                {
                    error += "campo data da obbligatorio;";
                }
            }
            if ( certificazione._tipo == "2" )
            {
                if ( string.IsNullOrWhiteSpace( certificazione._titoloPubblica ) || string.IsNullOrEmpty( certificazione._titoloPubblica ) )
                {
                    error += "campo titolo pubblicazione obbligatorio;";
                }
                if ( string.IsNullOrWhiteSpace( certificazione._dataPubblica ) || string.IsNullOrEmpty( certificazione._dataPubblica ) )
                {
                    error += "campo data pubblicazione;";
                }

            }
            if ( certificazione._tipo == "3" )
            {
                if ( string.IsNullOrWhiteSpace( certificazione._tipoBrevetto ) || string.IsNullOrEmpty( certificazione._tipoBrevetto ) )
                {
                    error += "campo titolo brevetto obbligatorio;";
                }
                if ( string.IsNullOrWhiteSpace( certificazione._flagRegBrevetto ) || string.IsNullOrEmpty( certificazione._flagRegBrevetto ) )
                {
                    error += "selezionare lo stato del brevetto;";
                }
                if ( string.IsNullOrWhiteSpace( certificazione._dataBrevetto ) || string.IsNullOrEmpty( certificazione._dataBrevetto ) )
                {
                    error += "campo data concessione obbligatorio;";
                }
            }
            if ( certificazione._tipo == "4" )
            {
                if ( string.IsNullOrWhiteSpace( certificazione._codAlboProf ) || string.IsNullOrEmpty( certificazione._codAlboProf ) )
                {
                    error += "selezionare albo professionale;";
                }
                if ( string.IsNullOrWhiteSpace( certificazione._dataAlboProf ) || string.IsNullOrEmpty( certificazione._dataAlboProf ) )
                {
                    error += "campo data iscrizione obbligatorio;";
                }
            }
            if ( !ModelState.IsValid )
            {

                var errori = ModelState.Values.Where( E => E.Errors.Count > 0 ).SelectMany( E => E.Errors ).Select( E => E.ErrorMessage ).ToList( );

                foreach ( var err in errori )
                {
                    error += err + ";";

                }
            }
            return error;
        }
        #endregion

        #region Controller Allegati
        public ActionResult Allegati ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvBox.allegati = new List<cvModel.Allegati>( );
            cvBox.listaBox = new List<cvModel.Box>( );
            cvBox.menuSidebar = new sidebarModel( CommonHelper.GetCurrentUserMatricola( ) , CommonHelper.GetCurrentUserPMatricola( ) , 5 );
            var lista = this.GetListaBox( cvEnt );
            foreach ( TCVBox_V2 boxSingolo in lista )
            {
                cvModel.Box appo = new cvModel.Box( );
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._idMenu = boxSingolo.Id_box;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                cvBox.listaBox.Add( appo );
            }

            //carico il modello CvModel.allegati
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            var allegati = cvEnt.TCVAllegato.Where( x => x.Matricola == matricola ).ToList( );

            foreach ( TCVAllegato all in allegati )
            {
                cvModel.Allegati frk_all = new cvModel.Allegati( );

                frk_all._contentType = all.ContentType;
                frk_all._dataOraAgg = all.DataOraAgg;
                frk_all._id = all.Id;
                frk_all._idBox = all.Id_box;
                frk_all._matricola = matricola;
                frk_all._name = all.Name;
                frk_all._pathName = all.Path_name;
                frk_all._size = all.Size;
                frk_all._stato = all.Stato;
                frk_all._tipoAgg = all.TipoAgg;

                cvBox.allegati.Add( frk_all );
            }
            //---------------------------------
            ViewBag.idMenu = 23;
            return View( "~/Views/CurriculumVitae/Allegati/Index.cshtml" , cvBox );
        }

        public ActionResult InsertAllegati ( cvModel.Allegati allegato , HttpPostedFileBase _fileUpload )
        {
            string matricola;
            string rootPath;
            string basePath;
            string pathComplete;
            string pathForAllegato;

            matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
            var tmp = "~";
            rootPath = Server.MapPath( tmp ) + "\\Media";
            if ( !Directory.Exists( rootPath ) )
            {
                Directory.CreateDirectory( rootPath );
            }
            basePath = rootPath + "\\" + matricola + "\\";
            if ( !Directory.Exists( basePath ) )
            {
                Directory.CreateDirectory( basePath );
            }
            //salvataggio del file in allegato o setto il pathName
            if ( ( _fileUpload != null ) && ( allegato._pathName == null ) )
            {
                pathComplete = basePath + _fileUpload.FileName;
                pathForAllegato = "\\Media\\" + matricola + "\\" + _fileUpload.FileName;
                try
                {
                    _fileUpload.SaveAs( pathComplete );
                }
                catch ( Exception exc )
                {
                    return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
                }
            }
            else
            {
                pathForAllegato = allegato._pathName;
            }
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            TCVAllegato tmp_all = new TCVAllegato( );

            if ( ( _fileUpload != null ) && ( allegato._pathName == null ) )
            {
                if ( _fileUpload.ContentType.Length >= 50 )
                {
                    tmp_all.ContentType = _fileUpload.ContentType.Substring( 0 , 50 );
                }
                else
                {
                    tmp_all.ContentType = _fileUpload.ContentType;
                }
                tmp_all.Size = _fileUpload.ContentLength;
            }
            else
            {
                tmp_all.ContentType = "website";
                tmp_all.Size = 0;
            }
            tmp_all.DataOraAgg = DateTime.Now;
            tmp_all.Id_box = null;
            tmp_all.Matricola = matricola;
            tmp_all.Name = allegato._name;
            tmp_all.Path_name = pathForAllegato;
            tmp_all.Stato = "S";
            tmp_all.TipoAgg = "I";

            try
            {
                cvEnt.TCVAllegato.Add( tmp_all );
                cvEnt.SaveChanges( );
            }
            catch ( Exception exc )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }

            ViewBag.idMenu = 23;
            return RedirectToAction( "Allegati" );
        }

        [HttpGet]
        public ActionResult DeleteAllegati ( string matricola , int id , string pathName )
        {
            string basepath;
            cv_ModelEntities cvEnt = new cv_ModelEntities( );

            if ( ( matricola == null ) && ( id < 0 ) )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            TCVAllegato allegato = cvEnt.TCVAllegato.Find( id );
            if ( allegato == null )
            {
                return HttpNotFound( );
            }
            try
            {
                //cancellaione del file allegato
                //var tmp = Request.AppRelativeCurrentExecutionFilePath;
                var tmp = "~";
                basepath = Server.MapPath( tmp ) + pathName;
                if ( System.IO.File.Exists( basepath ) )
                {
                    System.IO.File.Delete( basepath );
                }
                cvEnt.TCVAllegato.Remove( allegato );
                cvEnt.SaveChanges( );
            }
            catch ( Exception exc )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            //FREAK - RIEMPIMENTO DEL MODELLO CVBOX E DELLE LISTE SPECIALIZZAZIONI E ISTRUZIONE
            //---------------------------------
            ViewBag.idMenu = 23;
            return RedirectToAction( "Allegati" );
            //FINEE

        }
        #endregion
    }
}