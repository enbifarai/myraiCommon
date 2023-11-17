using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRai.DataAccess;
using myRaiData;
using MyRaiServiceInterface.MyRaiServiceReference1;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using myRaiCommonManager;
using ServiceWrapper = myRaiHelper.ServiceWrapper;
using myRaiCommonModel;
using myRaiCommonModel.ess;
using System.ServiceModel;
using System.Text;
using System.Net;
using myRaiDataTalentia;
using myRaiCommonModel.DashboardResponsabile;
using myRai.Business;
using myRai.Controllers;

namespace myRai.Models
{
    public class ScrivaniaController : BaseCommonController
    {
        ModelDash pr = new ModelDash();
        daApprovareModel daApprov;
        WSDigigapp datiBack = new WSDigigapp();
        WSDigigapp datiBack_ws1 = new WSDigigapp();
        MyRaiService1Client wcf1 = new MyRaiService1Client();

        private void InitSessionScopeController()
        {
            try
            {
                if (!ScrivaniaControllerScope.Instance.DataChiusura1.HasValue)
                {
                    DateTime curr = DateTime.Now;
                    string mese = DateTime.Now.AddMonths(-1).Month.ToString().PadLeft(2, '0');
                    string anno = DateTime.Now.AddMonths(-1).Year.ToString();
                    string[] utenteConv = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.UtentePerConvalida);

                    ScrivaniaControllerScope.Instance.DataChiusura1 = myRaiCommonTasks.CommonTasks.GetDataChiusura1(mese, anno, utenteConv[0], 75, 0);
                }

                string dateBack = Utente.GetDateBackPerEvidenze();
                var getListaEvidenze = SessionHelper.Get(SessionVariables.ListaEvidenzeScrivania);
                monthResponseEccezione resp = new monthResponseEccezione();

                if (getListaEvidenze != null)
                {
                    var sessionData = (myRaiHelper.SessionListaEvidenzeModel)getListaEvidenze;
                    DateTime dtNow = DateTime.Now;

                    TimeSpan diff = dtNow - sessionData.UltimaScrittura;

                    if (diff.TotalSeconds <= 60)
                    {
                        resp = sessionData.ListaEvidenze;
                    }
                    else
                    {
                        resp = ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, CommonManager.GetCurrentUserMatricola(), dateBack, DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);
                    }
                }
                else
                {
                    resp = ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, CommonManager.GetCurrentUserMatricola(), dateBack, DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);
                }

                myRaiHelper.SessionListaEvidenzeModel sessionModel = new myRaiHelper.SessionListaEvidenzeModel()
                {
                    UltimaScrittura = DateTime.Now,
                    ListaEvidenze = resp
                };

                // set della sessione per impostare i dati della chiamata ListaEvidenze
                SessionHelper.Set(SessionVariables.ListaEvidenzeScrivania, sessionModel);

                List<string> ecc = new List<string>();
                ecc.Add("FEDO");
                ecc.Add("FECE");

                SessionHelper.Set(SessionVariables.GetContatoriEccezioni, FeriePermessiManager.GetContatoriEccezioni(ecc));
            }
            catch (Exception ex)
            {

            }
        }

        public string SkipScrivania()
        {
            var db = new digiGappEntities();
            string m = CommonManager.GetCurrentRealUsername();
            var q = db.MyRai_ParametriSistema.Where(x => x.Chiave == "QuickStart" && (x.Valore1 == m || x.Valore1 == "*")).FirstOrDefault();
            if (q == null || String.IsNullOrWhiteSpace(q.Valore2))
                return null;
            else
                return q.Valore2;
        }

        public ActionResult Index(string ver)
        {
            InitSessionScopeController();
            SessionManager.Set(SessionVariables.AnnoFeriePermessi, null);

            string s = SkipScrivania();
            if (s != null)
                return RedirectToAction("/", s);

            if (ver == null)
            {
                ver = "3";
            }
            //if (ver != "3")
            //{
            //    string[] valori = CommonManager.GetParametri<string>(EnumParametriSistema.OrariGapp);

            //    wcf1.ClientCredentials.Windows.ClientCredential =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );

            //    string userName = CommonManager.GetCurrentUsername();

            //    datiBack.Credentials =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            //    datiBack_ws1.Credentials =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );

            //    pr.digiGAPP = false;

            //    pr.SceltePercorso = HomeManager.GetSceltepercorsoModel();

            //    if (Utente.IsBoss())
            //        pr.TotaleEccezioniDaApprovare = ScrivaniaManager.GetTotaleEccezioniDaApprovare().Item1;
            //    if (!Utente.GappChiuso())
            //    {
            //        pr = ScrivaniaManager.GetTotaliEvidenze(pr, true);
            //        pr.dettaglioGiornata = HomeManager.GetTimbratureTodayModel();
            //        pr.dettaglioSettimanaleModel = new Models.DettaglioSettimanaleModel(
            //           wcf1.GetPresenzeSettimanaliProtected(CommonManager.GetCurrentUserMatricola(), "03042017", "09042017",Utente.DataInizioValidita(),Convert.ToDateTime("03/04/2017")));
            //    }
            //}

            pr.SceltePercorso = HomeManager.GetSceltepercorsoModel("PR");
            pr.Raggruppamenti = CommonManager.GetRaggruppamenti();

            pr.menuSidebar = Utente.getSidebarModel();// new sidebarModel(3);

            if (pr.menuSidebar.sezioni.Count == 0)
            {
                return View("~/Views/Shared/NonAbilitatoError.cshtml");
            }

            pr.SezioniVisibili = HomeManager.GetSezioniVisibili(CommonManager.GetCurrentUserMatricola());
            SectionDayModel model = new SectionDayModel() { };
            pr.MessaggioHome = ScrivaniaManager.SetMessaggioHome();
            return View("~/Views/Scrivania/Index" + ver + ".cshtml", pr);
        }

        public void TimeStamper(Dictionary<string, DateTime> times)
        {
            string desc = "";
            int counter = 0;
            foreach (var item in times)
            {
                if (counter == 0)
                    desc += item.Key + ":0\n";
                else
                    desc += item.Key + ":" + (times.ElementAt(counter).Value - times.ElementAt(counter - 1).Value).TotalMilliseconds.ToString() + "\n";
                counter++;
            }

            Logger.LogAzione(new MyRai_LogAzioni()
            {
                applicativo = "Portale",
                data = DateTime.Now,
                descrizione_operazione = desc,
                matricola = CommonManager.GetCurrentUserMatricola(),
                operazione = "Timestamper",
                provenienza = "Index_section1"
            });
        }

        public ActionResult Ajax_GetWidgetDaApprovare()
        {
            AlertModel model = GetWidgetDaApprovare(null);

            if (model != null)
            {
                model.WaitLabel = "Controllo resoconti...";
                model.IntestazioneWidget = "Attività di Ruolo";
                return View("~/Views/_RaiDesign/Widget_home.cshtml", model);
            }
            else
                return null;
        }

        public ActionResult Ajax_GetResocontiDaApprovare()
        {
            List<Sede> sedi = Utility.GetSediGappResponsabileList();
            int TotalPdfPresenzeDaVisionare = 0;
            foreach (Sede s in sedi)
            {
                TotalPdfPresenzeDaVisionare += ResocontiManager.QuantiPdfDaGenerare(CommonManager.GetCurrentUserMatricola(), s.CodiceSede).Count();
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { totale = TotalPdfPresenzeDaVisionare }
            };
        }

        AlertModel GetWidgetDaApprovare(List<MyRai_Sezioni_Visibili> sezionivisibili)
        {
            if ((Utente.IsBoss() ||
                Utente.IsBossLiv2() ||
                CommonManager.IsApprovatoreProduzione(CommonManager.GetCurrentUserMatricola())) && sezionivisibili == null)
            {
                sezionivisibili = HomeManager.GetSezioniVisibili(CommonManager.GetCurrentUserMatricola());
            }

            //se utente è boss, alert per COSE DA APPROVARE
            if (Utente.IsBoss())
            {
                Tuple<int, int> T = ScrivaniaManager.GetTotaleEccezioniDaApprovare();

                // List<Sede> sedi = Utility.GetSediGappResponsabileList();

                int TotalPdfPresenzeDaVisionare = 0;

                // foreach (Sede s in sedi)
                // {
                //    TotalPdfPresenzeDaVisionare += ResocontiManager.QuantiPdfDaGenerare(s.CodiceSede).Count();
                //}


                string dataIntro = "Queste sono le richieste da approvare. Al momento hai " +
                     (TotalPdfPresenzeDaVisionare + T.Item1).ToString() + " richieste, di cui " + T.Item2
                     + " scadute";
                return new AlertModel()
                {
                    CifraPrincipale = (TotalPdfPresenzeDaVisionare + T.Item1).ToString(),
                    ClasseIcona = "icons icon-like",
                    ColoreClasseIcona = "cda",
                    HrefPulsante = "/approvazione",
                    TestoPulsante = "Vedi",
                    Titolo = "In approvazione \n(1° liv)",
                    TraParentesi = "(" + T.Item2 + " richieste scadute)",
                    Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "CDA").Count() > 0,
                    TipoAlert = 2,
                    intro_datastep = "3",
                    intro_dataintro = dataIntro,
                    AriaLabelSummary = "Hai " + (TotalPdfPresenzeDaVisionare + T.Item1).ToString() + " richieste da approvare, di cui " + T.Item2
                     + " scadute",
                    AriaLabelPulsante = "Vai alle richieste"
                };
            }
            else if (Utente.IsBossLiv2())
            {
                var db = new myRaiData.digiGappEntities();
                List<string> sedi = CommonManager.GetSediL2();


                int inApp = db.MyRai_Richieste.Where(a => sedi.Contains(a.codice_sede_gapp) && a.richiedente_L1 && a.id_stato == 10).Count();
                int inAppScadute = db.MyRai_Richieste.Where(a => sedi.Contains(a.codice_sede_gapp) && a.richiedente_L1 && a.id_stato == 10 && a.scaduta).Count();
                string intro = "Queste sono le richieste da approvare. Al momento hai " + inApp.ToString() + " richieste, di cui " + inAppScadute + " scadute";
                return new AlertModel()
                {
                    CifraPrincipale = inApp.ToString(),
                    ClasseIcona = "icons icon-like",
                    ColoreClasseIcona = "cda",
                    HrefPulsante = "/approvazione",
                    TestoPulsante = "Vedi",
                    Titolo = "In approvazione (1° liv)",
                    TraParentesi = "(" + inAppScadute + " richieste scadute)",
                    Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "CDA").Count() > 0,
                    TipoAlert = 2,
                    intro_datastep = "3",
                    intro_dataintro = intro,
                    AriaLabelSummary = "Hai " + inApp.ToString() + " richieste da approvare, di cui " + inAppScadute + " scadute",
                    AriaLabelPulsante = "Vai alle richieste"
                };
            }
            //else if (CommonManager.IsApprovatoreProduzione(CommonManager.GetCurrentUserMatricola()))
            //{
            //    int inApp = 0;
            //    int inAppScadute = 0;
            //    string intro = "";
            //    using ( digiGappEntities db = new digiGappEntities( ) )
            //    {
            //        string query = " SELECT DISTINCT * from MyRai_Richieste                                                          " +
            //                        " WHERE id_stato = 10 AND                                                                                            " +
            //                        " id_Attivita_ceiton in ( SELECT [id] FROM[digiGapp].[dbo].[MyRai_AttivitaCeiton] WHERE Titolo in ( SELECT [Titolo]  " +
            //                        " FROM [digiGapp].[dbo].[MyRai_ApprovatoreProduzione] WHERE MatricolaApprovatore = '##MATRICOLA##'))                 ";

            //        query = query.Replace( "##MATRICOLA##" , CommonManager.GetCurrentUserMatricola( ) );

            //        List< MyRai_Richieste> queryResult = db.Database.SqlQuery<MyRai_Richieste>( query ).ToList( );

            //        if (queryResult != null &&
            //            queryResult.Any())
            //        {
            //            inApp = queryResult.Count( );
            //            inAppScadute = queryResult.Count( w => w.scaduta );
            //            intro = "Queste sono le richieste da approvare. Al momento hai " + inApp.ToString( ) + " richieste, di cui " + inAppScadute + " scadute";
            //        }
            //    }

            //    return new AlertModel( )
            //    {
            //        CifraPrincipale = inApp.ToString( ) ,
            //        ClasseIcona = "icons icon-like" ,
            //        ColoreClasseIcona = "cda" ,
            //        HrefPulsante = "/ApprovazioneProduzione" ,
            //        TestoPulsante = "VEDI" ,
            //        Titolo = "In approvazione (1° liv)" ,
            //        TraParentesi = "(" + inAppScadute + " richieste scadute)" ,
            //        Visibile = sezionivisibili.Where( x => x.Sigla_Sezione.TrimEnd( ) == "CDA" ).Count( ) > 0 ,
            //        TipoAlert = 2 ,
            //        intro_datastep = "3" ,
            //        intro_dataintro = intro ,
            //        AriaLabelSummary = "Hai " + inApp.ToString( ) + " richieste da approvare, di cui " + inAppScadute + " scadute" ,
            //        AriaLabelPulsante = "Vai alle richieste"
            //    };
            //}
            return null;
        }

        public ActionResult Ajax_GetWidgetGiornateInEvidenza()
        {
            AlertModel model = GetWidgetGiornateInEvidenza(null);
            if (model != null)
            {
                model.IntestazioneWidget = "I TUOI DATI";
            }
            if (model != null)
                return View("~/Views/_RaiDesign/Widget_home.cshtml", model);
            else
                return null;
        }

        AlertModel GetWidgetGiornateInEvidenza(List<MyRai_Sezioni_Visibili> sezionivisibili)
        {
            if (Utente.TipoDipendente() == "D" || !Utente.IsAbilitatoGapp())
                return null;
            else
            {
                if (sezionivisibili == null)
                    sezionivisibili = HomeManager.GetSezioniVisibili(CommonManager.GetCurrentUserMatricola());
                // in evidenza DA GIUSTIFICARE                   
                ModelDash m = new ModelDash();
                m = ScrivaniaManager.GetTotaliEvidenze(m, true);


                if (m.TotaleEvidenzeDaGiustificareSoloAssIng != 0)
                {
                    string dataIntro = "Queste sono le tue giornate in evidenza. Al momento hai un totale di " +
                          m.TotaleEvidenzeDaGiustificare.ToString() + " giornate, di cui " + m.TotaleEvidenzeDaGiustificareSoloAssIng +
                          " sono assenze ingiustificate.";
                    var d = m.listaEvidenze.data.giornate.Where(x => x.TipoEcc == TipoEccezione.AssenzaIngiustificata || x.TipoEcc == TipoEccezione.Incongruenza)
                        .OrderBy(a => a.data).FirstOrDefault();

                    return new AlertModel()
                    {
                        CifraPrincipale = m.TotaleEvidenzeDaGiustificare.ToString(),
                        ClasseIcona = "icons icon-fire",
                        ColoreClasseIcona = "gioev",
                        HrefPulsante = "JavaScript:ShowPopupInizialeGoDate('" + CommonManager.GetParametro<string>(EnumParametriSistema.MessaggioAssenteIngiustificato) + "',false,'" + d.data.ToString("dd/MM/yyyy") + "')",
                        TestoPulsante = "Gestisci",
                        Titolo = "Giornate in Evidenza",
                        TraParentesi = "(" + (m.TotaleEvidenzeDaGiustificareSoloAssIng +
                                                m.TotaleEvidenzeDaGiustificareSWTIM +
                                                m.TotaleEvidenzeDaGiustificareCarenze) + " da giustificare)",
                        Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "GIOEV").Count() > 0,
                        TipoAlert = 1,
                        intro_datastep = "1",
                        intro_dataintro = dataIntro,
                        AriaLabelSummary = "Hai " + m.TotaleEvidenzeDaGiustificare.ToString() + " giornate in evidenza",
                        AriaLabelPulsante = "Gestisci le tue giornate in evidenza"
                    };
                }
                else
                {
                    if (m.TotaleEvidenzeDaGiustificare != 0)
                    {
                        string dataIntro = "Queste sono le tue giornate in evidenza. Al momento hai un totale di " +
                          m.TotaleEvidenzeDaGiustificare.ToString() + " giornate, di cui " + m.TotaleEvidenzeDaGiustificareSoloAssIng +
                          " sono assenze ingiustificate.";
                        return new AlertModel()
                        {
                            CifraPrincipale = m.TotaleEvidenzeDaGiustificare.ToString(),
                            ClasseIcona = "icons icon-fire",
                            ColoreClasseIcona = "gioev",
                            HrefPulsante = "/Home",
                            TestoPulsante = "Vedi",
                            Titolo = "Giornate in Evidenza",
                            TraParentesi = "(" + (m.TotaleEvidenzeDaGiustificareSoloAssIng + m.TotaleEvidenzeDaGiustificareSWTIM +
                                                m.TotaleEvidenzeDaGiustificareCarenze) + " da giustificare)",
                            Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "GIOEV").Count() > 0,
                            TipoAlert = 1,
                            intro_datastep = "1",
                            intro_dataintro = dataIntro,
                            AriaLabelSummary = "Hai " + m.TotaleEvidenzeDaGiustificare.ToString() + " giornate in evidenza",
                            AriaLabelPulsante = "Controlla le tue giornate in evidenza"
                        };
                    }
                    else
                    {
                        string dataIntro = "Queste sono le tue giornate in evidenza. Al momento hai un totale di " +
                      m.TotaleEvidenzeDaGiustificare.ToString() + " giornate, di cui " + m.TotaleEvidenzeDaGiustificareSoloAssIng +
                      " sono assenze ingiustificate.";
                        return new AlertModel()
                        {
                            CifraPrincipale = m.TotaleEvidenzeDaGiustificare.ToString(),
                            ClasseIcona = "icons icon-emotsmile",
                            ColoreClasseIcona = "gioev",
                            HrefPulsante = m.TotaleEvidenzeDaGiustificareSoloAssIng == 0 ? "javascript:ShowPopup('',0)" : "JavaScript:ShowPopupIniziale('" + CommonManager.GetParametro<string>(EnumParametriSistema.MessaggioAssenteIngiustificato) + "')",
                            TestoPulsante = "Effettua una richiesta",
                            Titolo = "Giornate in Evidenza",
                            TraParentesi = "(" + (m.TotaleEvidenzeDaGiustificareSoloAssIng + m.TotaleEvidenzeDaGiustificareSWTIM +
                                                m.TotaleEvidenzeDaGiustificareCarenze) + " da giustificare)",
                            Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "GIOEV").Count() > 0,
                            TipoAlert = 1,
                            intro_datastep = "1",
                            intro_dataintro = dataIntro,
                            AriaLabelSummary = "Hai " + m.TotaleEvidenzeDaGiustificare.ToString() + " giornate in evidenza",
                            AriaLabelPulsante = "Effettua una richiesta"
                        };
                    }
                }
            }
        }

        public ActionResult Ajax_GetWidgetDaFirmare()
        {
            AlertModel model = GetWidgetDaFirmare(null);
            if (model != null)
            {
                model.IntestazioneWidget = "ATTIVITA' DI RUOLO";
            }
            if (model != null)
                return View("~/Views/_RaiDesign/Widget_home.cshtml", model);
            else
                return null;
        }

        AlertModel GetWidgetDaFirmare(List<MyRai_Sezioni_Visibili> sezionivisibili)
        {
            if (!Utente.IsBossLiv2())
                return null;
            else
            {
                if (sezionivisibili == null)
                    sezionivisibili = HomeManager.GetSezioniVisibili(CommonManager.GetCurrentUserMatricola());

                DaFirmareModel dfmodel = new DaFirmareModel();
                dfmodel.Sedi = DaFirmareManager.GetDaFirmareModel();


                TotaliDaFirmareModel TotaliDaFirmare = DaFirmareManager.GetTotaliDaFirmareModel(dfmodel.Sedi);
                dfmodel.PianiFerieDisponibiliPerFirma = DaFirmareManager.GetTotalePianiFerieDisponibiliFirma(dfmodel.Sedi);

                DateTime Dstart, Dend;
                if ((int)DateTime.Now.DayOfWeek < CommonManager.GetParametro<int>(EnumParametriSistema.GiornoEsecuzioneBatchPDF))
                {
                    Dend = CommonManager.GetlastSunday(DateTime.Now.AddDays(-7).Date);
                    Dstart = CommonManager.GetlastMonday(Dend).Date;
                }
                else
                {
                    Dend = CommonManager.GetlastSunday(DateTime.Now).Date;
                    Dstart = CommonManager.GetlastMonday(Dend).Date;
                }



                int Inweek = dfmodel.Sedi.Where(x => x.PeriodiPDF != null).SelectMany(se => se.PeriodiPDF).Where(d => d.DateStart == Dstart && d.DateEnd == Dend).Count();
                string dataIntro = "Questi sono i PDF delle eccezioni e delle presenze che hai da esaminare. Ci sono "
                     + dfmodel.Sedi.Where(x => x.PeriodiPDF != null).SelectMany(se => se.PeriodiPDF).Count().ToString() + " documenti, di cui " +
                     TotaliDaFirmare.Totale.ToString() + " da firmare.";
                return new AlertModel()
                {
                    CifraPrincipale = (dfmodel.PianiFerieDisponibiliPerFirma + dfmodel.Sedi.Where(x => x.PeriodiPDF != null).SelectMany(se => se.PeriodiPDF).Count()).ToString(),
                    ClasseIcona = "icons icon-note",
                    ColoreClasseIcona = "cdf",
                    HrefPulsante = "/Firma",
                    TestoPulsante = "Vedi",
                    Titolo = "In firma (2° liv)",
                    TraParentesi = "(" + TotaliDaFirmare.Totale.ToString() + " esaminate)",
                    //  TraParentesi = "(" + Inweek.ToString() + " tra " + Dstart.ToString("dd-MM") + "/" + Dend.ToString("dd-MM") + ")",
                    Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "CDF").Count() > 0,
                    TipoAlert = 2,
                    intro_datastep = "4",
                    intro_dataintro = dataIntro,
                    AriaLabelSummary = "Hai " + dfmodel.Sedi.Where(x => x.PeriodiPDF != null).SelectMany(se => se.PeriodiPDF).Count().ToString() + " PDF delle eccezioni da esaminare, di cui " + TotaliDaFirmare.Totale.ToString() + " da firmare.",
                    AriaLabelPulsante = "Vedi le eccezioni"
                };
            }
        }

        public ActionResult Ajax_GetWidgetQuadratura()
        {
            AlertModel model = GetWidgetQuadratura(null);
            if (model != null)
            {
                model.IntestazioneWidget = "I TUOI DATI";
            }
            if (model != null)
                return View("~/Views/_RaiDesign/Widget_home.cshtml", model);
            else
                return null;
        }

        AlertModel GetWidgetQuadratura(List<MyRai_Sezioni_Visibili> sezionivisibili)
        {
            if (sezionivisibili == null)
                sezionivisibili = HomeManager.GetSezioniVisibili(CommonManager.GetCurrentUserMatricola());
            Boolean abil = Utente.IsAbilitatoGapp();

            // se utente è a Quadratura Settimanale
            if (Utente.GetQuadratura() == Quadratura.Settimanale &&
                (Utente.IsAbilitatoGapp() || myRaiCommonManager.EccezioniManager.GetMinutiCarenzaPerSede(Utente.SedeGapp(DateTime.Now), DateTime.Now) <= 0))
            {
                // DateTime datainizio = Convert.ToDateTime("03/04/2017");
                // DateTime datafine = Convert.ToDateTime("09/04/2017");

                DateTime[] Da = CommonManager.GetIntervalloSettimanaleSede();


                DateTime datainizio = Da[0];
                DateTime datafine = Da[1];
                wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

                //DettaglioSettimanaleModel dsettModel = new Models.DettaglioSettimanaleModel(
                //    wcf1.PresenzeSettimanali(CommonManager.GetCurrentUserMatricola(), datainizio.ToString("ddMMyyyy"), 
                //    datainizio.AddDays(6).ToString("ddMMyyyy")));
                //DettaglioSettimanaleModel dsettModel = new Models.DettaglioSettimanaleModel(
                //   wcf1.GetPresenzeSettimanaliProtected(CommonManager.GetCurrentUserMatricola(), datainizio.ToString("ddMMyyyy"),
                //   datainizio.AddDays(6).ToString("ddMMyyyy"), Utente.DataInizioValidita(), datainizio));
                DettaglioSettimanaleModel dsettModel = new DettaglioSettimanaleModel(
                   wcf1.GetPresenzeSettimanaliProtected(CommonManager.GetCurrentUserMatricola(), datainizio.ToString("ddMMyyyy"),
                   DateTime.Today.ToString("ddMMyyyy"), Utente.DataInizioValidita(), datainizio));



                string dataIntro = "Questo è il tuo riepilogo settimanale. Nella settimana tra il "
                      + datainizio.ToString("dd-MM") + " e il " + datafine.ToString("dd-MM") + " hai una differenza di "
                      + dsettModel.DeltaTotale + " minuti.";
                return new AlertModel()
                {
                    CifraPrincipale = dsettModel.DeltaTotale,
                    ClasseIcona = "icons icon-user-following",
                    ColoreClasseIcona = "prse",
                    HrefPulsante = abil ? "/Home" : "/Timbrature",
                    //TestoPulsante = abil ? "Vedi" : "Riepilogo" ,
                    TestoPulsante = "Vedi",
                    Titolo = "Riepilogo settimanale",
                    TraParentesi = "(" + datainizio.ToString("dd-MM") + "/" + datafine.ToString("dd-MM") + ")",
                    Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "PRSE").Count() > 0,
                    TipoAlert = 1,
                    intro_datastep = "2",
                    intro_dataintro = dataIntro,
                    AriaLabelSummary = "Nella settimana tra il "
                    + datainizio.ToString("dd-MM") + " e il " + datafine.ToString("dd-MM") + " hai una differenza di "
                    + dsettModel.DeltaTotale + " minuti.",
                    AriaLabelPulsante = "Vedi il riepilogo settimanale"
                };
            }
            //se utente è a quadratura giornaliera
            if (Utente.GetQuadratura() == Quadratura.Giornaliera)
            {
                int poh = 0;
                int roh = 0;
                Utente.GetPOHandROH(false, null, out poh, out roh);
                //int bilancio = Utente.GetROH() - Utente.GetPOH();
                int bilancio = roh - poh;

                int hrs = Math.Abs(bilancio) / 60;
                int min = Math.Abs(bilancio) - (60 * hrs);
                int POHmese = Utente.GetPOHdays().Where(x => x.Date.Month == DateTime.Now.Month).Count();
                int maxPOHmese = CommonManager.GetParametro<int>(EnumParametriSistema.POHperMese);

                string dataIntro = "Questo è il tuo bilancio tra i permessi POH e i recuperi ROH. In questo momento " +
                      " il tuo bilancio è di " +
                      (bilancio > 0 ? "+ " : bilancio == 0 ? " " : "- ") + hrs.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0')
                      + " minuti, e nel mese corrente hai usufruito di " + POHmese + " POH";
                return new AlertModel()
                {
                    CifraPrincipale = (bilancio > 0 ? "+ " : bilancio == 0 ? " " : "- ") + hrs.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0'),
                    ClasseIcona = bilancio >= 0 ? "icons icon-user-following" : "icons icon-clock",
                    ColoreClasseIcona = "prse",
                    HrefPulsante = abil ? "/Home" : "/feriepermessi",
                    //TestoPulsante = abil ? "Gestisci" : "Riepilogo" ,
                    TestoPulsante = "Vedi",
                    Titolo = "Permessi Orari",
                    TraParentesi = "(" + POHmese + " nel mese corr.)",
                    TestoRosso = (POHmese > maxPOHmese ? "+" + (POHmese - maxPOHmese).ToString() + " extra" : ""),
                    Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "BPOH").Count() > 0,
                    TipoAlert = 1,
                    intro_datastep = "2",
                    intro_dataintro = dataIntro,
                    AriaLabelSummary = "Il tuo bilancio tra i permessi P O H e i recuperi R O H è di " +
                    (bilancio > 0 ? "+ " : bilancio == 0 ? " " : "- ") + hrs.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0'),
                    AriaLabelPulsante = abil ? "Gestisci i permessi orari" : "Vedi il riepilogo dei permessi orari"
                };
            }
            return null;
        }

        public ActionResult Ajax_GetWidgetApprovatoreProduzione()
        {
            AlertModel model = GetWidgetApprovatoreProduzione(null);
            if (model != null)
            {
                model.IntestazioneWidget = "Attività di Ruolo";
                return View("~/Views/_RaiDesign/Widget_home.cshtml", model);
                //return View( "~/Views/scrivania/subpartial/alert.cshtml" , model );
            }
            else
                return null;
        }

        AlertModel GetWidgetApprovatoreProduzione(List<MyRai_Sezioni_Visibili> sezionivisibili)
        {
            string matricola = CommonManager.GetCurrentUserMatricola();

            List<string> result = new List<string>();
            //result.AddRange( CommonManager.GetSediL3( ) );
            result.AddRange(CommonManager.GetSediL4());
            //result.AddRange( CommonManager.GetSediL5( ) );

            if (result == null || !result.Any())
            {
                return null;
            }

            if (sezionivisibili == null)
                sezionivisibili = HomeManager.GetSezioniVisibili(matricola);

            int inApp = 0;
            int inAppScadute = 0;

            using (digiGappEntities db = new digiGappEntities())
            {
                DateTime oggi = DateTime.Now;
                List<string> listaEccezioniScartate = new List<string>();

                string par = CommonManager.GetParametro<string>(EnumParametriSistema.EccezioniEscluseL4);
                if (!String.IsNullOrEmpty(par))
                {
                    listaEccezioniScartate = par.Split(',').ToList();
                }

                //var totale = db.MyRai_Richieste.Where( x => x.id_stato == 10 && x.ApprovatoreSelezionato == matricola && x.id_Attivita_ceiton != null ).ToList( );

                var totale = (from richieste in db.MyRai_Richieste
                              join ecc in db.MyRai_Eccezioni_Richieste
                              on richieste.id_richiesta equals ecc.id_richiesta
                              where richieste.id_stato == 10
                              && richieste.matricola_richiesta != matricola
                              && richieste.ApprovatoreSelezionato == matricola
                              && richieste.id_Attivita_ceiton != null
                              && !listaEccezioniScartate.Contains(ecc.cod_eccezione)
                              select richieste).ToList();

                inApp = totale.Count();
                inAppScadute = totale.Count(w => w.scaduta);

                // deve prendere tutte le richieste dove in approvatore selezionato è presente un ufficio di produzione a me assegnato
                List<string> uffici = new List<string>();
                var uffApp = db.MyRai_UffProduzioni_Approvatori.Where(w => w.MatricolaApprovatore.Equals(matricola)).ToList();
                if (uffApp != null && uffApp.Any())
                {
                    uffici.AddRange(uffApp.Select(w => w.CodUfficio).ToList());

                    totale = (from richieste in db.MyRai_Richieste
                              join ecc in db.MyRai_Eccezioni_Richieste
                              on richieste.id_richiesta equals ecc.id_richiesta
                              join uffApp2 in db.MyRai_UffProduzioni_Approvatori
                              on richieste.ApprovatoreSelezionato equals uffApp2.CodUfficio
                              where richieste.id_stato == 10
                              && richieste.matricola_richiesta != matricola
                              && richieste.id_Attivita_ceiton != null
                              && !listaEccezioniScartate.Contains(ecc.cod_eccezione)
                              && uffApp2.MatricolaApprovatore == matricola
                              select richieste).ToList();

                    //totale = db.MyRai_Richieste.Where( x => x.id_stato == 10 && uffici.Contains( x.ApprovatoreSelezionato ) && x.id_Attivita_ceiton != null ).ToList( );
                    inApp += totale.Count();
                    inAppScadute += totale.Count(w => w.scaduta);
                }
            }

            string intro = "Queste sono le richieste da approvare. Al momento hai " + inApp.ToString() + " richieste, di cui " + inAppScadute + " scadute";
            return new AlertModel()
            {
                CifraPrincipale = inApp.ToString(),
                ClasseIcona = "icons icon-like",
                ColoreClasseIcona = "cda",
                HrefPulsante = "/ApprovazioneProduzione",
                TestoPulsante = "Vedi",
                Titolo = "In approvazione",
                TraParentesi = "(" + inAppScadute + " richieste scadute)",
                Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "CDA").Count() > 0,
                TipoAlert = 2,
                intro_datastep = "3",
                intro_dataintro = intro,
                AriaLabelSummary = "Hai " + inApp.ToString() + " richieste da approvare, di cui " + inAppScadute + " scadute",
                AriaLabelPulsante = "Vai alle richieste"
            };

        }


        public ActionResult Index_section1()
        {
            try
            {
                wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
                SectionAlertModel model = new SectionAlertModel() { Alerts = new List<AlertModel>() };
                List<MyRai_Sezioni_Visibili> sezionivisibili = HomeManager.GetSezioniVisibili(CommonManager.GetCurrentUserMatricola());

                AlertModel WidgetDaApprovare = GetWidgetDaApprovare(sezionivisibili);
                if (WidgetDaApprovare != null)
                    model.Alerts.Add(WidgetDaApprovare);

                AlertModel WidgetGiornateInEvidenza = GetWidgetGiornateInEvidenza(sezionivisibili);
                if (WidgetGiornateInEvidenza != null)
                    model.Alerts.Add(WidgetGiornateInEvidenza);

                AlertModel WidgetDaFirmare = GetWidgetDaFirmare(sezionivisibili);
                if (WidgetDaFirmare != null)
                    model.Alerts.Add(WidgetDaFirmare);

                AlertModel WidgetQuadratura = GetWidgetQuadratura(sezionivisibili);
                if (WidgetQuadratura != null)
                    model.Alerts.Add(WidgetQuadratura);

                AlertModel WidgetApprovatoreProduzione = GetWidgetApprovatoreProduzione(sezionivisibili);
                if (WidgetApprovatoreProduzione != null)
                    model.Alerts.Add(WidgetApprovatoreProduzione);

                return View(model);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        public ActionResult Index_section2()
        {
            SectionMetersModel model = new SectionMetersModel();
            model.CalendarioFerieModel = new FeriePermessiController().GetCalendarioAnnualPFmodel(DateTime.Now.Year);
            WSDigigapp service = new WSDigigapp();
            service.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            pianoFerie resp = ServiceWrapper.GetPianoFerieWrapped(service, CommonManager.GetCurrentUserMatricola7chars(),
                DateTime.Now.ToString("0101yyyy"), 70, Utente.TipoDipendente());
            //DateTime.Now.ToString( "ddMMyyyy" ) , 70 , Utente.TipoDipendente( ) );
            model.ferie = resp.dipendente.ferie;
            model.spettanzapg = resp.dipendente.spgDip;
            SessionManager.Set(SessionVariables.GetPianoFerieWrapped, resp);

            ModelDash m = new ModelDash();
            if (myRai.Models.Utente.IsAbilitatoGapp())
            {
                m = ScrivaniaManager.GetTotaliEvidenze(m, true);

                string ai = CommonManager.GetParametro<string>(EnumParametriSistema.IgnoraAssenzeIngiustificatePerMatricole);

                model.HaAssenzeIngiustificate = (m.TotaleEvidenzeDaGiustificareSWTIM > 0 || m.TotaleEvidenzeDaGiustificareSoloAssIng > 0)
                    && !(ai != null && ai.ToUpper().Contains(CommonManager.GetCurrentUserMatricola().ToUpper()));

                model.HaGiorniCarenza = Utente.GiornateConCarenza() != null;
            }

            model.pianoF = resp;

            if (model.pianoF.dipendente.ferie != null && model.pianoF.dipendente.ferie.visualizzaPermessiGiornalisti)
            {
                model.IsGiornalista = true;
                model.TotalePXC = model.pianoF.dipendente.ferie.permessiGiornalistiRecupero;
                model.TotaleMN = model.pianoF.dipendente.ferie.mancatiNonLavoratiSpettanti;
                model.MNAnnoPrecedente = model.pianoF.dipendente.ferie.mancatiNonLavoratiAnniPrecedenti;
                if (model.MNAnnoPrecedente > 0)
                {
                    DateTime inizioA = new DateTime(DateTime.Now.Year, 1, 1);
                    DateTime scadenza = inizioA.AddDays(45);
                    model.ScadenzaMNAnnoPrecedente = scadenza;
                }

                // se la matricola è autorizzata allora visualizza i pxc
                string matricoleForzarePXC = "";
                string matricoleForzareMN = "";
                using (digiGappEntities dbDG = new digiGappEntities())
                {
                    var parametriPXC = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "MatricoleForzarePXC");
                    if (parametriPXC != null)
                    {
                        matricoleForzarePXC = parametriPXC.Valore1;
                    }

                    var parametriMN = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "MatricoleForzareMN");
                    if (parametriMN != null)
                    {
                        matricoleForzareMN = parametriMN.Valore1;
                    }
                }

                if (matricoleForzarePXC.Contains(Utente.Matricola()) || matricoleForzarePXC.Contains("*"))
                {
                    model.PXCVisibili = true;
                }

                if (matricoleForzareMN.Contains(Utente.Matricola()) || matricoleForzareMN.Contains("*"))
                {
                    model.MNVisibili = true;
                }
            }

            GetContatoriEccezioniResponse dati = (GetContatoriEccezioniResponse)SessionManager.Get(SessionVariables.GetContatoriEccezioni);

            if (dati != null && dati.Esito)
            {
                if (dati.ContatoriEccezioni != null && dati.ContatoriEccezioni.Any())
                {
                    var fece = dati.ContatoriEccezioni.Where(w => w.Esito && w.CodiceEccezione.Equals("FECE")).FirstOrDefault();
                    var fedo = dati.ContatoriEccezioni.Where(w => w.Esito && w.CodiceEccezione.Equals("FEDO")).FirstOrDefault();
                    var mrce = dati.ContatoriEccezioni.Where(w => w.Esito && w.CodiceEccezione.Equals("MRCE")).FirstOrDefault();
                    var mrdo = dati.ContatoriEccezioni.Where(w => w.Esito && w.CodiceEccezione.Equals("MRDO")).FirstOrDefault();

                    if (fece != null)
                    {
                        model.FeCedute = float.Parse(fece.Totale);
                    }

                    if (fedo != null)
                    {
                        model.FeDonate = float.Parse(fedo.Totale);
                    }

                    if (mrce != null)
                    {
                        model.MRCeduti = float.Parse(mrce.Totale);
                    }

                    if (mrdo != null)
                    {
                        model.MRDonati = float.Parse(mrdo.Totale);
                    }

                }
            }

            #region REPERIMENTO DATI SCOSTAMENTO FERIE

            bool monitoraggioVisibile = false;
            var matricoleMonitoraggioFerie = CommonManager.GetParametro<string>(EnumParametriSistema.MatricoleMonitoraggioFerie);

            if (!String.IsNullOrEmpty(matricoleMonitoraggioFerie))
            {
                if (matricoleMonitoraggioFerie.Contains(CommonManager.GetCurrentUserMatricola()) ||
                    matricoleMonitoraggioFerie.Contains("*"))
                {
                    monitoraggioVisibile = true;
                }
                else
                {
                    monitoraggioVisibile = false;
                }
            }
            else
            {
                monitoraggioVisibile = false;
            }

            if (monitoraggioVisibile)
            {
                DipendenteScostamento myData = new DipendenteScostamento();
                string sede = "";
                string reparto = "";

                sede = Utente.SedeGapp();
                reparto = Utente.Reparto();

                if (!(String.IsNullOrWhiteSpace(reparto) || reparto.Trim() == "0" || reparto.Trim() == "00"))
                {
                    sede = sede + reparto;
                }

                StatoFerieObj datiFerie = GetModelFromCache(sede);

                if (datiFerie != null)
                {
                    var dip = datiFerie.Dipendenti.Where(w => w.Matricola.Equals(CommonManager.GetCurrentUserMatricola())).FirstOrDefault();

                    if (dip != null)
                    {
                        myData.GiorniEffettivi = dip.GiorniEffettivi;
                        myData.GiorniPianificati = dip.GiorniPianificati;
                        myData.GiorniPianificatiAdOggi = dip.GiorniPianificatiAdOggi;
                        myData.Scostamento = dip.Scostamento;
                        myData.Percentuale = dip.Percentuale;

                        model.DatiScostamento = new DipendenteScostamento();
                        model.DatiScostamento = myData;
                    }
                }
            }

            #endregion

            return View(model);
        }

        private StatoFerieObj GetModelFromCache(string sede)
        {
            StatoFerieObj objDeserialized = null;

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    sede = sede.Replace("-", "");
                    string tag = "StatoFerie" + sede;
                    var exists = db.MyRai_CacheFunzioni.Where(w => w.oggetto.Equals(tag)).FirstOrDefault();

                    if (exists != null)
                    {
                        objDeserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<StatoFerieObj>(exists.dati_serial);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return objDeserialized;
        }

        public ActionResult Index_section3(bool GappClosed)
        {
            SectionDayModel model = new SectionDayModel() { GappClosed = GappClosed };
            string matr = CommonManager.GetCurrentUserMatricola();
            try
            {
                model.CalendarioModel = FeriePermessiManager.GetCalendarioSituazioneEccezioni(null, null);

                try
                {
                    pianoFerie resp = (pianoFerie)SessionManager.Get(SessionVariables.GetPianoFerieWrapped);
                    if (resp != null && resp.esito)
                    {
                        var ferie = resp.dipendente.ferie;

                        if (ferie != null)
                        {
                            var giornate = ferie.giornate;

                            if (giornate != null && giornate.Any())
                            {
                                if (model.CalendarioModel != null &&
                                    model.CalendarioModel.Giornate != null &&
                                    model.CalendarioModel.Giornate.Any())
                                {
                                    foreach (var day in model.CalendarioModel.Giornate)
                                    {
                                        string dataStringa = String.Format("{0}-{1}", day.DataEccezione.Day.ToString().PadLeft(2, '0'), day.DataEccezione.Month.ToString().PadLeft(2, '0'));

                                        var item = giornate.Where(w => w.dataTeorica.Equals(dataStringa)).FirstOrDefault();

                                        if (item != null)
                                        {
                                            string codiceOrario = item.orarioReale;
                                            day.CodiceOrario = codiceOrario;
                                        }
                                    }
                                }
                            }
                        }


                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    provenienza = "Index_section3",
                    error_message = ex.ToString()
                });
                model.CalendarioModel = null;
            }

            List<MyRai_Sezioni_Visibili> sezionivisibili = HomeManager.GetSezioniVisibili(CommonManager.GetCurrentUserMatricola());

            bool schermataFlat = CommonManager.GetParametro<bool>(EnumParametriSistema.AbilitaSchermataFlat);

            if (!GappClosed)
            {
                WSDigigappDataController service = new WSDigigappDataController();

                dayResponse resp = service.GetEccezioni(CommonManager.GetCurrentUserMatricola(), matr, DateTime.Now.ToString("ddMMyyyy"), "BU", 70);

                if (resp != null && resp.timbrature != null &&
                    resp.timbrature.Count() > 0 &&
                    resp.timbrature[0].entrata != null &&
                    resp.timbrature[0].entrata.orario != null &&
                    resp.giornata != null
                    && resp.giornata.orarioReale != null)
                {
                    string codiceOrario = resp.giornata.orarioReale;
                    string ingresso = resp.timbrature[0].entrata.orario;
                    MyRaiServiceInterface.OrarioUscitaModel ou = CommonManager.GetOrarioDiUscita(ingresso, codiceOrario,
                                            DateTime.Now.ToString("ddMMyyyy"), resp);

                    resp.OrarioInBaseAingresso = ou;
                }

                model.DayResponse = resp;
            }

            model.Notifiche = new myRaiData.digiGappEntities().MyRai_Notifiche
                              .Where(x => x.matricola_destinatario == matr && x.data_letta == null)
                              .OrderByDescending(x => x.data_inserita).ToList();

            model.NotificaVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "NOTIF").Count() > 0;
            model.AttivitaVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "ATTSET").Count() > 0;
            model.CoseDaFareVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "DAFARE").Count() > 0;
            model.BustaPagaVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "BUSTA").Count() > 0;
            model.OrarioVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "OGGI").Count() > 0;
            model.TimbraturaVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "TIMB").Count() > 0;
            model.ScelteVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "SCELTE").Count() > 0;
            model.WeekPlanVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "APLAN").Count() > 0;

            if (model.WeekPlanVisibile && myRai.Models.Utente.GestitoSirio())
            {
                string attMatr = CommonManager.GetCurrentUserPMatricola();
                DateTime rifDate = DateTime.Today;

                //model.WeekPlan = myRai.Models.Sirio.Helper.GetWeekPlan( attMatr , rifDate.AddDays( -3 ) , rifDate.AddDays( 3 ) );
                model.WeekPlan = myRaiHelper.Sirio.Helper.GetWeekPlan(attMatr, rifDate, rifDate.AddDays(6));

                if (model.WeekPlan != null && model.WeekPlan.Days.Any())
                {
                    model.WeekPlan.Days.ForEach(d =>
                   {
                       if (d.Activities != null && d.Activities.Any())
                       {
                           d.Activities.RemoveAll(w => w.idAttivita.Equals("000000"));
                       }
                   });
                }

                if (model.WeekPlan.Days != null && model.WeekPlan.Days.Any() && model.WeekPlan.Days.Count() < 7)
                {
                    for (int idx = 0; idx < 7; idx++)
                    {
                        //DateTime dtTest = rifDate.AddDays( -3 ).AddDays( idx );
                        DateTime dtTest = rifDate.AddDays(idx);
                        bool exists = model.WeekPlan.Days.Count(w => w.Date.Date == dtTest.Date) > 0;
                        if (!exists)
                        {
                            model.WeekPlan.Days.Add(new myRaiHelper.DayPlan()
                            {
                                Date = dtTest,
                                Activities = new List<DayActivity>()
                            });
                        }
                    }
                }
            }
            var db = new digiGappEntities();
            int y = DateTime.Now.Year;

            string MatricolePianoFerie = CommonManager.GetParametro<string>(EnumParametriSistema.MatricoleVisibilitaPianoFerie);
            var def = db.MyRai_PianoFerieDefinizioni.Where(x => x.anno == DateTime.Now.Year).FirstOrDefault();
            var un = CommonManager.GetCurrentRealUsername();
            model.PianoFerieVisible = Utente.IsAbilitatoGapp() &&
                def != null && DateTime.Now >= def.data_apertura &&
                DateTime.Now <= def.data_chiusura && (MatricolePianoFerie == "*" || MatricolePianoFerie.Contains(matr));

            if (model.PianoFerieVisible)
            {
                string datasoglia = CommonManager.GetParametro<string>(EnumParametriSistema.DataSogliaPartenzaFase3);
                if (!String.IsNullOrWhiteSpace(datasoglia))
                {
                    DateTime Da;
                    if (DateTime.TryParseExact(datasoglia, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out Da))
                    {
                        string sedeutente = Utente.SedeGapp();
                        var sedeGapp = db.L2D_SEDE_GAPP.FirstOrDefault(x => x.cod_sede_gapp == sedeutente);
                        if (sedeGapp != null && sedeGapp.partenza_fase_3 != null
                                    && sedeGapp.partenza_fase_3.Value >= Da)

                            model.PianoFerieVisible = false;
                    }

                }

            }

            model.MyPianoFerie = db.MyRai_PianoFerie.Where(x => x.matricola == matr && x.anno == y).FirstOrDefault();

            string rep = Utente.Reparto();
            if (String.IsNullOrWhiteSpace(rep) || rep.Trim() == "0" || rep.Trim() == "00")
                rep = null;
            string codSede = Utente.SedeGapp(DateTime.Now);
            string mysede = Utente.SedeGapp(DateTime.Now) + rep;

            model.MyPianoFerieSede = db.MyRai_PianoFerieSedi.Where(x => x.sedegapp == mysede && x.anno == y).OrderByDescending(x => x.numero_versione).FirstOrDefault();

            model.AbilitatoGestionePianoFerie = true;
            if (model.MyPianoFerieSede != null && model.MyPianoFerieSede.data_firma != null)
            {
                model.AbilitatoGestionePianoFerie = false;
            }

            DateTime D = DateTime.Now;
            int? IdTipologiaEsenteFerie = db.MyRai_InfoDipendente_Tipologia.Where(x => x.info == "Esente Piano Ferie" && x.data_inizio <= D && (x.data_fine >= D || x.data_fine == null))
                                    .Select(x => x.id).FirstOrDefault();

            if (IdTipologiaEsenteFerie != null)
            {

                model.EsentatoPianoFerie = db.MyRai_InfoDipendente.Any(x => x.valore != null && x.valore.ToLower() == "true" &&
                                                         x.matricola == matr && x.id_infodipendente_tipologia == IdTipologiaEsenteFerie && D >= x.data_inizio && D <= x.data_fine);
            }

            // conteggio trasferte da rendicontare
            model.TrasfertePrecedenti = new myRaiCommonModel.ess.TrasferteDaRendicontareVM();

            if (!schermataFlat)
            {
                Trasferta trasferteDaRendicontare = null;

                if (Session["TrasferteDaRendicontare"] == null)
                {
                    Session["TrasferteDaRendicontare"] = TrasferteManager.GetTrasferteAnniPrecedenti(CommonManager.GetCurrentUserMatricola());
                }
                trasferteDaRendicontare = (Trasferta)Session["TrasferteDaRendicontare"];

                if (trasferteDaRendicontare != null &&
                    trasferteDaRendicontare.Viaggi != null &&
                    trasferteDaRendicontare.Viaggi.Any())
                {
                    var fogliV = (from v in trasferteDaRendicontare.Viaggi select v.FoglioViaggio).ToList();
                    int tot = 0;
                    if (fogliV != null && fogliV.Any())
                    {
                        tot = fogliV.Distinct().Count();
                    }
                    model.TrasfertePrecedenti.Conteggio = tot;
                    string messaggio = tot > 1 ? "Hai " + tot + " fogli viaggio da rendicontare" : "Hai " + tot + " foglio viaggio da rendicontare";
                    messaggio = tot == 0 ? "Hai " + tot + " fogli viaggio da rendicontare" : messaggio;
                    model.TrasfertePrecedenti.Messaggio = messaggio;
                    model.TrasfertePrecedenti.Url = Url.Action("Index", "Trasferte", new { daRendicontare = 1 });
                }
                else
                {
                    string messaggio = "Hai 0 fogli viaggio da rendicontare";
                    model.TrasfertePrecedenti.Messaggio = messaggio;
                    model.TrasfertePrecedenti.Url = Url.Action("Index", "Trasferte", new { daRendicontare = 1 });
                }
            }

            model.DetassazioneVM = new DetassazioneVM();

            // se l'utente è Giornalista, Orchestrale o Dirigente
            // non ha diritto alla richiesta di detassazione
            if (Utente.TipoDipendente().Equals("G") ||
                Utente.TipoDipendente().Equals("O") ||
                Utente.TipoDipendente().Equals("D"))
            {
                model.DetassazioneVM.HaDiritto = false;
            }
            else
            {
                string[] valori = CommonManager.GetParametri<string>(EnumParametriSistema.BoxDetassazione);
                string[] messaggi = CommonManager.GetParametri<string>(EnumParametriSistema.BoxDetassazioneMessaggi);

                if (valori != null &&
                    valori.Any() &&
                    messaggi != null &&
                    messaggi.Any())
                {
                    //string msg = valori[0];
                    string dtStart = valori[0];
                    string dtEnd = valori[1];
                    DateTime dataInizio = DateTime.Now;
                    DateTime dataFine = DateTime.Now;
                    DateTime.TryParseExact(dtStart, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataInizio);
                    DateTime.TryParseExact(dtEnd, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataFine);
                    string msg1 = messaggi[0];
                    string msg2 = messaggi[1];

                    if (dataInizio <= DateTime.Now &&
                        dataFine >= DateTime.Now)
                    {
                        using (HRPADBEntities dbDetax = new HRPADBEntities())
                        {
                            string mtr = CommonManager.GetCurrentUserMatricola();
                            var exists = dbDetax.T_DetaxNew.Where(w => w.Matricola_T_DetaxNew.Equals(mtr)).FirstOrDefault();

                            if (exists != null)
                            {
                                // se già valorizzati allora, scelta già effettuata
                                if (exists.Data_T_DetaxNew.HasValue && !String.IsNullOrEmpty(exists.Modello_T_DetaxNew) &&
                                    !exists.Modello_T_DetaxNew.Equals("ND"))
                                {
                                    model.DetassazioneVM.HaDiritto = false;
                                    model.DetassazioneVM.GiaScelto = true;
                                    model.DetassazioneVM.CodiceModello = exists.Modello_T_DetaxNew;
                                    model.DetassazioneVM.Messaggio = msg2;
                                    model.DetassazioneVM.Anno = DateTime.Now.Year;
                                    model.DetassazioneVM.CodiceDetassazione = "DETAX";
                                }
                                else if (!String.IsNullOrEmpty(exists.Modello_T_DetaxNew) &&
                                           exists.Modello_T_DetaxNew.Equals("ND"))
                                {
                                    model.DetassazioneVM.HaDiritto = false;
                                    model.DetassazioneVM.GiaScelto = false;
                                }
                                else
                                {
                                    model.DetassazioneVM.HaDiritto = true;
                                    model.DetassazioneVM.GiaScelto = false;
                                    model.DetassazioneVM.CodiceModello = exists.ModelloAssegnato_T_DetaxNew;

                                    if (exists.ModelloAssegnato_T_DetaxNew.Equals("1C"))
                                    {
                                        msg1 = "Rinuncia tassazione agevolata";
                                    }

                                    model.DetassazioneVM.Messaggio = msg1;
                                    model.DetassazioneVM.Anno = DateTime.Now.Year;
                                    model.DetassazioneVM.CodiceDetassazione = "DETAX";
                                }
                            }
                            else
                            {
                                model.DetassazioneVM.HaDiritto = false;
                            }
                        }
                    }
                }
                else
                {
                    model.DetassazioneVM.HaDiritto = false;
                }
            }

            #region Bonus 100 EURO

            model.Bonus100VM = GetWidgetBonus100Data();

            #endregion

            #region Modulo SmartWorking

            model.SmartWorkingWidget = GetWidgetSW2020Data();

            #endregion

            #region Modulo ProrogaSW
            //model.ProrogaSWWidget = null;
            model.ProrogaSWWidget = GetProrogaWidgetData();

            #endregion

            #region BOX MODULO RINUNCIA

            if (Utente.Categoria().ToUpper().StartsWith("A7") || Utente.Categoria().ToUpper().StartsWith("A01"))
            {
                model.RinunciaWidget = new WidgetModuloBox()
                {
                    WidgetId = "WdgRinuncia2020",
                    Anno = DateTime.Now.Year,
                    GiaScelto = false,
                    HaDiritto = false,
                    Titolo = "Modulo rinuncia al trattamento integrativo e ulteriore detrazione",
                    Scelta = "",
                    Bottoni = null
                };
            }
            else
            {
                model.RinunciaWidget = GetRinuncia2020WidgetData();
            }

            #endregion

            #region MODULO INCENTIVAZIONE GENNAIO 2021


            // se è una matricola forzata allora salta tutto e visualizza il box
            string Incentivazione012021WidgetMatricoleForzate = "";
            using (digiGappEntities dbDG = new digiGappEntities())
            {
                var parametro3 = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "Incentivazione012021WidgetMatricoleForzate");
                if (parametro3 != null)
                {
                    Incentivazione012021WidgetMatricoleForzate = parametro3.Valore1;
                }
            }

            if (Incentivazione012021WidgetMatricoleForzate.Contains(Utente.Matricola()))
            {
                model.Incentivazione012021Widget = GetIncentivazione012021WidgetData();
            }
            else if (Utente.TipoDipendente().Equals("O") || Utente.TipoDipendente().Equals("D"))
            {
                // se è un orchestrale o dirigente il widget non va mostrato
                model.Incentivazione012021Widget = new WidgetModuloBox();
                model.Incentivazione012021Widget.HaDiritto = false;
            }
            else
            {
                // altrimenti passa ai controlli sull'età
                // calcolo età utente                
                var today = DateTime.Today;

                // calcola la data massima selezionabile dal widget
                // 3 mesi successivi alla fine del mese corrente
                DateTime prossimi3Mesi = new DateTime(today.Year, today.Month, 1);
                prossimi3Mesi = prossimi3Mesi.AddMonths(4);
                prossimi3Mesi = prossimi3Mesi.AddDays(-1);

                var age = prossimi3Mesi.Year - Utente.EsponiAnagrafica()._dataNascita.GetValueOrDefault().Year;

                // se è nato in un mese e giorno successivo ad oggi sottrae un anno perchè ancora non ha compiuto gli anni
                if (Utente.EsponiAnagrafica()._dataNascita.GetValueOrDefault().Date > prossimi3Mesi.AddYears(-age))
                    age--;

                if (age == 60)
                {
                    // bisogna verificare se compirà gli anni entro i 3 mesi
                    DateTime toCompare = new DateTime(DateTime.Now.Year, Utente.EsponiAnagrafica()._dataNascita.GetValueOrDefault().Month, Utente.EsponiAnagrafica()._dataNascita.GetValueOrDefault().Day);

                    if (Utente.EsponiAnagrafica()._dataNascita.GetValueOrDefault().Month <= prossimi3Mesi.Month &&
                        (toCompare.Date > today.Date && (DateTime.Now.Year - Utente.EsponiAnagrafica()._dataNascita.GetValueOrDefault().Year > 60)))
                    {
                        age++;
                    }
                }

                // se non ha almeno 61 anni allora il widget non va mostrato
                if (age < 61)
                {
                    model.Incentivazione012021Widget = new WidgetModuloBox();
                    model.Incentivazione012021Widget.HaDiritto = false;
                }
                else
                {
                    model.Incentivazione012021Widget = GetIncentivazione012021WidgetData();
                }
            }

            #endregion

            if (CommonManager.IsProduzione() || System.Diagnostics.Debugger.IsAttached)
            {
                string matrAmmesse = "";
                using (digiGappEntities dbDG = new digiGappEntities())
                {
                    var parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "ValutazioniMatrAmmesse");
                    if (parametro != null)
                        matrAmmesse = parametro.Valore1;
                }

                if (String.IsNullOrWhiteSpace(matrAmmesse) || matrAmmesse.Split(',').Contains(CommonManager.GetCurrentUserMatricola()))
                    model.ValutatoreEsterno = ValutazioniManager.GetEvaluatorFromPerson(CommonManager.GetCurrentUserMatricola());
            }


            return View(model);
        }

        public ActionResult GetIncentivazione012021Widget()
        {
            WidgetModuloBox model = GetIncentivazione012021WidgetData();
            return View("~/Views/Scrivania/subpartial/boxModulo.cshtml", model);
        }

        public ActionResult GetWidgetBonus100()
        {
            Bonus100EVM model = GetWidgetBonus100Data();
            return View("~/Views/Scrivania/subpartial/boxBonus100.cshtml", model);
        }

        public ActionResult GetWidgetProrogaSW()
        {
            WidgetModuloBox model = GetProrogaWidgetData();
            return View("~/Views/Scrivania/subpartial/boxModulo.cshtml", model);
        }

        public ActionResult GetWidgetSW2020()
        {
            WidgetModuloBox model = GetWidgetSW2020Data();
            return View("~/Views/Scrivania/subpartial/boxModulo.cshtml", model);
        }

        public ActionResult GetWidgetRinuncia2020()
        {
            WidgetModuloBox model = GetRinuncia2020WidgetData();
            return View("~/Views/Scrivania/subpartial/boxModulo.cshtml", model);
        }

        private WidgetModuloBox GetProrogaWidgetData()
        {
            WidgetModuloBox model = new WidgetModuloBox();

            #region Modulo PROROGA SmartWorking
            try
            {
                string dtStart = "";
                string dtEnd = "";

                using (digiGappEntities dbDG = new digiGappEntities())
                {
                    var parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "ProrogaModuloSmartWorking2020");
                    if (parametro != null)
                    {
                        dtStart = parametro.Valore1;
                        dtEnd = parametro.Valore2;
                    }
                }

                DateTime dataInizio = DateTime.Now;
                DateTime dataFine = DateTime.Now;
                DateTime.TryParseExact(dtStart, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataInizio);
                DateTime.TryParseExact(dtEnd, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataFine);

                bool giaScelto = false;
                string sceltaSmart2020 = "";
                DateTime? dataCompilazioneSW2020 = null;
                DateTime? dataLettura = null;
                List<WidgetModuloBox_Azione> btns = new List<WidgetModuloBox_Azione>();
                bool prorogaVisibile = false;
                string matr7 = CommonManager.GetCurrentUserPMatricola();
                prorogaVisibile = Utente.IsSmartWorker(dataInizio, dataFine);

                bool skipControlli = false;

                using (digiGappEntities db = new digiGappEntities())
                {
                    var parametro = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "ProrogaModuloSmartWorking2020ForzaVisibilita");
                    if (parametro != null)
                    {
                        if (parametro.Valore1.Contains(matr7))
                        {
                            skipControlli = true;
                        }
                    }
                }

                if ((prorogaVisibile && (dataInizio <= DateTime.Now && dataFine >= DateTime.Now)) || skipControlli)
                {
                    // verifica se l'utente ha già effettuato la scelta
                    string mt = CommonManager.GetCurrentUserMatricola();
                    using (TalentiaEntities dbTalentia = new TalentiaEntities())
                    {
                        // verifica se la matricola è presente su Sintesi1
                        bool matricolaPresente = dbTalentia.SINTESI1.Count(w => w.COD_MATLIBROMAT.Equals(mt) && w.COD_IMPRESACR == "0") > 0;
                        if (!matricolaPresente)
                        {
                            throw new Exception("Utente non presente in elenco");
                        }

                        bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count(w => w.MATRICOLA.Equals(mt) && w.COD_MODULO.Equals("PROROGASMARTW2020") && w.SCELTA != "") > 0;

                        if (!exist)
                        {
                            giaScelto = false;
                        }
                        else
                        {
                            var item = dbTalentia.XR_MOD_DIPENDENTI.Where(w => w.MATRICOLA.Equals(mt) && w.COD_MODULO.Equals("PROROGASMARTW2020")).FirstOrDefault();

                            if (item != null)
                            {
                                DateTime? dtComp = item.DATA_COMPILAZIONE;
                                dataCompilazioneSW2020 = dtComp;
                                dataLettura = item.DATA_LETTURA;
                                giaScelto = true;
                                sceltaSmart2020 = item.SCELTA;
                            }
                        }
                    }

                    if (giaScelto)
                    {
                        btns.Add(new WidgetModuloBox_Azione()
                        {
                            TestoBottone = "Visualizza",
                            UrlBottone = Url.Action("VisualizzaProrogaModuloSmartWorking", "Moduli", new { codiceModulo = "PROROGASMARTW2020" })
                        });

                        model = new WidgetModuloBox()
                        {
                            WidgetId = "WdgProrogaSmartWorking",
                            Anno = DateTime.Now.Year,
                            GiaScelto = true,
                            HaDiritto = true,
                            Titolo = "Modulo accordo smart working",
                            Scelta = sceltaSmart2020,
                            DataCompilazione = dataCompilazioneSW2020,
                            DataLettura = dataLettura,
                            Sottotitolo = "Modulo compilato in data " + dataCompilazioneSW2020.GetValueOrDefault().ToString("dd/MM/yyyy"),
                            Bottoni = btns
                        };
                    }
                    else
                    {
                        btns.Add(new WidgetModuloBox_Azione()
                        {
                            TestoBottone = "Compila",
                            UrlBottone = Url.Action("CompilaProrogaModuloSmartWorking", "Moduli", new { codiceModulo = "PROROGASMARTW2020" })
                        });

                        model = new WidgetModuloBox()
                        {
                            WidgetId = "WdgProrogaSmartWorking",
                            Anno = DateTime.Now.Year,
                            GiaScelto = giaScelto,
                            HaDiritto = true,
                            Titolo = "Modulo accordo smart working",
                            Scelta = sceltaSmart2020,
                            DataCompilazione = null,
                            DataLettura = dataLettura,
                            Bottoni = btns
                        };
                    }
                }
                else
                {
                    model = new WidgetModuloBox()
                    {
                        WidgetId = "WdgProrogaSmartWorking",
                        Anno = DateTime.Now.Year,
                        GiaScelto = false,
                        HaDiritto = false,
                        Titolo = "Modulo accordo smart working",
                        Scelta = "",
                        DataCompilazione = null,
                        Bottoni = null
                    };
                }
            }
            catch (Exception ex)
            {
                myRaiCommonTasks.CommonTasks.LogErrore(ex.Message, "Scrivania");
                model = new WidgetModuloBox()
                {
                    WidgetId = "WdgProrogaSmartWorking",
                    Anno = DateTime.Now.Year,
                    GiaScelto = false,
                    HaDiritto = false,
                    Titolo = "Modulo accordo smart working",
                    Scelta = "",
                    DataCompilazione = null,
                    Bottoni = null
                };
            }

            #endregion

            return model;
        }

        private WidgetModuloBox GetIncentivazione012021WidgetData()
        {
            WidgetModuloBox model = new WidgetModuloBox();

            try
            {
                string dtStart = "";
                string dtEnd = "";
                string tipoDipEscluso = "";
                string Incentivazione012021WidgetMatricoleForzate = "";

                using (digiGappEntities dbDG = new digiGappEntities())
                {
                    var parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "Incentivazione012021Widget");
                    if (parametro != null)
                    {
                        dtStart = parametro.Valore1;
                        dtEnd = parametro.Valore2;
                    }

                    var parametro2 = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "Incentivazione012021WidgetTipiDipEsclusi");
                    if (parametro2 != null)
                    {
                        tipoDipEscluso = parametro2.Valore1;
                    }

                    var parametro3 = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "Incentivazione012021WidgetMatricoleForzate");
                    if (parametro3 != null)
                    {
                        Incentivazione012021WidgetMatricoleForzate = parametro3.Valore1;
                    }
                }

                DateTime dataInizio = DateTime.Now;
                DateTime dataFine = DateTime.Now;
                DateTime.TryParseExact(dtStart, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataInizio);
                DateTime.TryParseExact(dtEnd, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataFine);

                bool giaScelto = false;
                string scelta = "";
                DateTime? dataCompilazioneModulo = null;
                List<WidgetModuloBox_Azione> btns = new List<WidgetModuloBox_Azione>();

                string tipoDip = Utente.TipoDipendente();

                if (Incentivazione012021WidgetMatricoleForzate.Contains(Utente.Matricola()) ||
                    (dataInizio <= DateTime.Now && dataFine >= DateTime.Now && !tipoDipEscluso.Contains(tipoDip)))
                {
                    // verifica se l'utente ha già effettuato la scelta
                    string mt = CommonManager.GetCurrentUserMatricola();
                    using (TalentiaEntities dbTalentia = new TalentiaEntities())
                    {
                        // verifica se la matricola è presente su Sintesi1
                        bool matricolaPresente = dbTalentia.SINTESI1.Count(w => w.COD_MATLIBROMAT.Equals(mt) && w.COD_IMPRESACR == "0") > 0;
                        if (!matricolaPresente)
                        {
                            throw new Exception("Utente non presente in elenco");
                        }

                        bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count(w => w.MATRICOLA.Equals(mt) && w.COD_MODULO.Equals("INCENTIVAZIONE012021")) > 0;

                        if (!exist)
                        {
                            giaScelto = false;
                        }
                        else
                        {
                            var items = dbTalentia.XR_MOD_DIPENDENTI.Where(w => w.MATRICOLA.Equals(mt) && w.COD_MODULO.Equals("INCENTIVAZIONE012021")).ToList();

                            if (items != null && items.Any())
                            {
                                DateTime? dtComp = items.First().DATA_COMPILAZIONE;
                                dataCompilazioneModulo = dtComp;
                                giaScelto = true;
                            }
                        }
                    }

                    if (giaScelto)
                    {
                        btns.Add(new WidgetModuloBox_Azione()
                        {
                            TestoBottone = "Visualizza",
                            UrlBottone = Url.Action("VisualizzaModuloIncentivazione012021", "Moduli", new { codiceModulo = "INCENTIVAZIONE012021" })
                        });

                        model = new WidgetModuloBox()
                        {
                            WidgetId = "WdgIncentivazione012021",
                            Anno = DateTime.Now.Year,
                            GiaScelto = true,
                            HaDiritto = true,
                            Titolo = "Modulo incentivazione",
                            Scelta = scelta,
                            DataCompilazione = dataCompilazioneModulo,
                            Sottotitolo = "Modulo compilato in data " + dataCompilazioneModulo.GetValueOrDefault().ToString("dd/MM/yyyy"),
                            Bottoni = btns
                        };
                    }
                    else
                    {
                        btns.Add(new WidgetModuloBox_Azione()
                        {
                            TestoBottone = "Compila",
                            UrlBottone = Url.Action("CompilaIncentivazione012021", "Moduli", new { codiceModulo = "INCENTIVAZIONE012021" })
                        });

                        model = new WidgetModuloBox()
                        {
                            WidgetId = "WdgIncentivazione012021",
                            Anno = DateTime.Now.Year,
                            GiaScelto = giaScelto,
                            HaDiritto = true,
                            Titolo = "Modulo incentivazione",
                            Scelta = scelta,
                            DataCompilazione = null,
                            Bottoni = btns
                        };
                    }
                }
                else
                {
                    model = new WidgetModuloBox()
                    {
                        WidgetId = "WdgIncentivazione012021",
                        Anno = DateTime.Now.Year,
                        GiaScelto = false,
                        HaDiritto = false,
                        Titolo = "Modulo incentivazione",
                        Scelta = "",
                        DataCompilazione = null,
                        Bottoni = null
                    };
                }
            }
            catch (Exception ex)
            {
                myRaiCommonTasks.CommonTasks.LogErrore(ex.Message, "Scrivania");
                model = new WidgetModuloBox()
                {
                    WidgetId = "WdgIncentivazione012021",
                    Anno = DateTime.Now.Year,
                    GiaScelto = false,
                    HaDiritto = false,
                    Titolo = "Modulo incentivazione",
                    Scelta = "",
                    DataCompilazione = null,
                    Bottoni = null
                };
            }

            return model;
        }

        /// <summary>
        /// Recupera su DB2 i dati relativi al widget Modulo di Rinuncia Trattamento integrativo e Ulteriore detrazione
        /// </summary>
        /// <returns></returns>
        private WidgetModuloBox GetRinuncia2020WidgetData()
        {
            WidgetModuloBox model = new WidgetModuloBox();
            List<WidgetModuloBox_Azione> btns = new List<WidgetModuloBox_Azione>();
            int dataRif = DateTime.Now.Year;
            try
            {
                string dtStart = "";
                string dtEnd = "";
                string matr7 = CommonManager.GetCurrentUserPMatricola();
                bool skipControlli = false;

                List<ParametriSistemaValoreJson> parametriDB = parametriDB = CommonManager.GetParametriJson(EnumParametriSistema.ModuloRinuncia2020Params);

                if (parametriDB != null && parametriDB.Any())
                {
                    var parametro = parametriDB.Where(w => w.Attributo == "ModuloRinuncia2020").FirstOrDefault();

                    if (parametro != null)
                    {
                        dtStart = parametro.Valore1;
                        dtEnd = parametro.Valore2;
                    }

                    parametro = parametriDB.Where(w => w.Attributo == "ModuloRinuncia2020ForzaVisibilita").FirstOrDefault();
                    if (parametro != null)
                    {
                        if (parametro.Valore1.Contains(matr7))
                        {
                            skipControlli = true;
                        }
                    }

                    var moduloRinuncia2020AnnoRiferimento = parametriDB.Where(w => w.Attributo == "ModuloRinuncia2020AnnoRiferimento").FirstOrDefault();

                    if (moduloRinuncia2020AnnoRiferimento != null)
                    {
                        if (!String.IsNullOrEmpty(moduloRinuncia2020AnnoRiferimento.Valore1))
                        {
                            dataRif = int.Parse(moduloRinuncia2020AnnoRiferimento.Valore1);
                        }
                    }
                }
                else
                {
                    using (digiGappEntities dbDG = new digiGappEntities())
                    {
                        var parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "ModuloRinuncia2020");
                        if (parametro != null)
                        {
                            dtStart = parametro.Valore1;
                            dtEnd = parametro.Valore2;
                        }

                        parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "ModuloRinuncia2020ForzaVisibilita");
                        if (parametro != null)
                        {
                            if (parametro.Valore1.Contains(matr7))
                            {
                                skipControlli = true;
                            }
                        }
                    }
                }

                DateTime dataInizio = DateTime.Now;
                DateTime dataFine = DateTime.Now;
                DateTime.TryParseExact(dtStart, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataInizio);
                DateTime.TryParseExact(dtEnd, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataFine);

                DateTime? dataCompilazione = null;

                if ((dataInizio <= DateTime.Now && dataFine >= DateTime.Now) || skipControlli)
                {
                    string urlServizio = System.Configuration.ConfigurationManager.AppSettings["URLwiahrss"];

                    Uri u = new Uri(urlServizio);

                    BasicHttpBinding binding = new BasicHttpBinding();
                    EndpointAddress address = new EndpointAddress(u);
                    binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                    binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                    binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

                    binding.MessageEncoding = WSMessageEncoding.Text;
                    binding.TextEncoding = Encoding.UTF8;
                    binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;

                    var _client = new myRaiServiceHub.ServiceBonus100.ezServiceSoapClient(binding, address);

                    if (_client.ClientCredentials != null)
                    {
                        _client.ClientCredentials.Windows.ClientCredential = new NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
                        _client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Delegation;
                    }
                    _client.Open();

                    var requestInterceptor = new InspectorBehavior();
                    _client.Endpoint.Behaviors.Add(requestInterceptor);

                    var clientResponse = _client.TRINUNCIA_BONUS(CommonManager.GetCurrentUserMatricola7chars());

                    if (clientResponse != null && clientResponse.Rows != null && clientResponse.Rows.Count > 0)
                    {
                        var r = clientResponse.Rows[0];

                        string matricola = r.ItemArray[0].ToString();
                        string annoRiferimento = r.ItemArray[1].ToString();
                        string risposta = r.ItemArray[2].ToString();
                        string dataAggiornamento = r.ItemArray[3].ToString();

                        if (!String.IsNullOrEmpty(dataAggiornamento))
                        {
                            dataAggiornamento = dataAggiornamento.Trim();
                            if (dataAggiornamento.Equals("01/01/0001 00:00:00"))
                            {
                                dataAggiornamento = "";
                            }

                            if (!string.IsNullOrEmpty(dataAggiornamento))
                            {
                                DateTime tmp;
                                DateTime.TryParseExact(dataAggiornamento, "dd/MM/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out tmp);
                                dataCompilazione = tmp;
                            }
                        }

                        if (!String.IsNullOrEmpty(annoRiferimento))
                        {
                            annoRiferimento = annoRiferimento.Trim();
                            if (annoRiferimento.Equals("") || annoRiferimento.Equals("0001"))
                            {
                                annoRiferimento = "";
                            }

                            if (!string.IsNullOrEmpty(annoRiferimento))
                            {
                                dataRif = int.Parse(annoRiferimento);
                            }
                        }

                        if (!String.IsNullOrEmpty(risposta))
                        {
                            bool risp = risposta.Trim().ToUpper() == "1";

                            btns.Add(new WidgetModuloBox_Azione()
                            {
                                TestoBottone = "Visualizza",
                                UrlBottone = Url.Action("VisualizzaModuloRinuncia2020", "Moduli", new { codiceModulo = "RINUNCIA2020" })
                            });

                            model = new WidgetModuloBox()
                            {
                                WidgetId = "WdgRinuncia2020",
                                Anno = dataRif,
                                GiaScelto = risp,
                                HaDiritto = true,
                                Titolo = "Modulo rinuncia al trattamento integrativo e ulteriore detrazione",
                                Sottotitolo = "Modulo compilato in data " + dataCompilazione.GetValueOrDefault().ToString("dd/MM/yyyy"),
                                Scelta = "",
                                DataCompilazione = dataCompilazione,
                                Bottoni = btns
                            };
                        }
                    }
                    else
                    {
                        // se non trova dati ??!!?
                        btns.Add(new WidgetModuloBox_Azione()
                        {
                            TestoBottone = "Compila",
                            UrlBottone = Url.Action("VisualizzaModuloRinuncia2020", "Moduli", new { codiceModulo = "RINUNCIA2020" })
                        });

                        model = new WidgetModuloBox()
                        {
                            WidgetId = "WdgRinuncia2020",
                            Anno = dataRif,
                            GiaScelto = false,
                            HaDiritto = true,
                            Titolo = "Modulo rinuncia al trattamento integrativo e ulteriore detrazione",
                            Scelta = "",
                            DataCompilazione = null,
                            Bottoni = btns
                        };
                    }

                    string requestXML = requestInterceptor.LastRequestXML;
                    string responseXML = requestInterceptor.LastResponseXML;

                    _client.Close();

                    model.Sottotitolo2 = "Per ulteriori informazioni <a href=\"http://www.raiplace.rai.it/comunicazione-int/trattamento-integrativo-e-ulteriore-detrazione-fiscale-a-favore-dei-lavoratori-dipendenti-d-l-5-2-2020-n-3-convertito-con-legge-del-2-4-2020-n-21/\" target=\"_blank\">clicca qui</a>";

                    if (parametriDB != null && parametriDB.Any())
                    {
                        var parametro = parametriDB.Where(w => w.Attributo == "ModuloRinuncia2020Sottotitolo2").FirstOrDefault();

                        if (parametro != null && !String.IsNullOrEmpty(parametro.Valore1))
                        {
                            model.Sottotitolo2 = parametro.Valore1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                myRaiCommonTasks.CommonTasks.LogErrore(ex.Message, "Scrivania");
                model = new WidgetModuloBox()
                {
                    WidgetId = "WdgRinuncia2020",
                    Anno = dataRif,
                    GiaScelto = false,
                    HaDiritto = false,
                    Titolo = "Modulo rinuncia al trattamento integrativo e ulteriore detrazione",
                    Scelta = "",
                    DataCompilazione = null,
                    Bottoni = null
                };
            }

            return model;
        }

        private WidgetModuloBox GetWidgetSW2020Data()
        {
            WidgetModuloBox model = new WidgetModuloBox();

            #region Modulo SmartWorking
            try
            {
                string dtStart = "";
                string dtEnd = "";
                string tipoDipEscluso = "";
                string ModuloSmartWorking2020MatricoleForzate = "";

                using (digiGappEntities dbDG = new digiGappEntities())
                {
                    var parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "ModuloSmartWorking2020");
                    if (parametro != null)
                    {
                        dtStart = parametro.Valore1;
                        dtEnd = parametro.Valore2;
                    }

                    var parametro2 = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "ModuloSmartWorking2020TipiDipEsclusi");
                    if (parametro2 != null)
                    {
                        tipoDipEscluso = parametro2.Valore1;
                    }

                    var parametro3 = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "ModuloSmartWorking2020MatricoleForzate");
                    if (parametro3 != null)
                    {
                        ModuloSmartWorking2020MatricoleForzate = parametro3.Valore1;
                    }
                }

                DateTime dataInizio = DateTime.Now;
                DateTime dataFine = DateTime.Now;
                DateTime.TryParseExact(dtStart, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataInizio);
                DateTime.TryParseExact(dtEnd, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataFine);

                bool giaScelto = false;
                string sceltaSmart2020 = "";
                DateTime? dataCompilazioneSW2020 = null;
                List<WidgetModuloBox_Azione> btns = new List<WidgetModuloBox_Azione>();

                string tipoDip = Utente.TipoDipendente();

                if (ModuloSmartWorking2020MatricoleForzate.Contains(Utente.Matricola()) ||
                    (dataInizio <= DateTime.Now && dataFine >= DateTime.Now && !tipoDipEscluso.Contains(tipoDip)))
                {
                    // verifica se l'utente ha già effettuato la scelta
                    string mt = CommonManager.GetCurrentUserMatricola();
                    using (TalentiaEntities dbTalentia = new TalentiaEntities())
                    {
                        // verifica se la matricola è presente su Sintesi1
                        bool matricolaPresente = dbTalentia.SINTESI1.Count(w => w.COD_MATLIBROMAT.Equals(mt) && w.COD_IMPRESACR == "0") > 0;
                        if (!matricolaPresente)
                        {
                            throw new Exception("Utente non presente in elenco");
                        }

                        bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count(w => w.MATRICOLA.Equals(mt) && w.COD_MODULO.Equals("SMARTW2020")) > 0;

                        if (!exist)
                        {
                            giaScelto = false;
                        }
                        else
                        {
                            var item = dbTalentia.XR_MOD_DIPENDENTI.Where(w => w.MATRICOLA.Equals(mt) && w.COD_MODULO.Equals("SMARTW2020")).FirstOrDefault();

                            if (item != null)
                            {
                                // se esiste controlla che tra le scelte fatte le date inserite non siano
                                // antecedenti ad oggi, altrimenti storicizza la selezione, in modo da
                                // consentire una nuova scelta all'utente.

                                List<ModuloSmart2020Selezioni> selezioni = new List<ModuloSmart2020Selezioni>();
                                selezioni = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ModuloSmart2020Selezioni>>(item.SCELTA);
                                DateTime dt1 = DateTime.MinValue;
                                DateTime dt2 = DateTime.MinValue;

                                if (selezioni == null)
                                {
                                    throw new Exception("Errore nel reperimento dei dati");
                                }

                                var dateDAL = selezioni.Where(w => w.DataSelezionataDal != null).ToList();
                                var dateAL = selezioni.Where(w => w.DataSelezionataAl != null).ToList();

                                if (dateDAL != null && dateDAL.Any())
                                {
                                    dt1 = dateDAL.Max(w => w.DataSelezionataDal.Value);
                                }

                                if (dateAL != null && dateAL.Any())
                                {
                                    dt2 = dateAL.Max(w => w.DataSelezionataAl.Value);
                                }

                                if (dateDAL != null && dateDAL.Any())
                                {
                                    DateTime dtMax = new DateTime(Math.Max(dt1.Ticks, dt2.Ticks));

                                    if (dtMax.Date > DateTime.Now.Date)
                                    {
                                        DateTime? dtComp = item.DATA_COMPILAZIONE;
                                        dataCompilazioneSW2020 = dtComp;
                                        giaScelto = true;
                                    }
                                    else
                                    {
                                        // storicizza
                                        item.MATRICOLA = item.MATRICOLA + "*";
                                        item.NOT_NOTA = "Informazione storicizzata. Data limite raggiunta. Storicizzato il " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                                        dbTalentia.SaveChanges();
                                        giaScelto = false;
                                    }
                                }
                                else
                                {
                                    DateTime? dtComp = item.DATA_COMPILAZIONE;
                                    dataCompilazioneSW2020 = dtComp;
                                    giaScelto = true;
                                }
                            }
                        }
                    }

                    if (giaScelto)
                    {
                        btns.Add(new WidgetModuloBox_Azione()
                        {
                            TestoBottone = "Visualizza",
                            UrlBottone = Url.Action("VisualizzaModuloSmartWorking", "Moduli", new { codiceModulo = "SMARTW2020" })
                        });

                        model = new WidgetModuloBox()
                        {
                            WidgetId = "WdgSmartWorking",
                            Anno = DateTime.Now.Year,
                            GiaScelto = true,
                            HaDiritto = true,
                            Titolo = "Esercizio diritto smart working",
                            Scelta = sceltaSmart2020,
                            DataCompilazione = dataCompilazioneSW2020,
                            Sottotitolo = "Modulo compilato in data " + dataCompilazioneSW2020.GetValueOrDefault().ToString("dd/MM/yyyy"),
                            Bottoni = btns
                        };
                    }
                    else
                    {
                        btns.Add(new WidgetModuloBox_Azione()
                        {
                            TestoBottone = "Compila",
                            UrlBottone = Url.Action("CompilaModuloSmartWorking", "Moduli", new { codiceModulo = "SMARTW2020" })
                        });

                        model = new WidgetModuloBox()
                        {
                            WidgetId = "WdgSmartWorking",
                            Anno = DateTime.Now.Year,
                            GiaScelto = giaScelto,
                            HaDiritto = true,
                            Titolo = "Esercizio diritto smart working",
                            Scelta = sceltaSmart2020,
                            DataCompilazione = null,
                            Bottoni = btns
                        };
                    }
                }
                else
                {
                    model = new WidgetModuloBox()
                    {
                        WidgetId = "WdgSmartWorking",
                        Anno = DateTime.Now.Year,
                        GiaScelto = false,
                        HaDiritto = false,
                        Titolo = "Esercizio diritto smart working",
                        Scelta = "",
                        DataCompilazione = null,
                        Bottoni = null
                    };
                }
            }
            catch (Exception ex)
            {
                myRaiCommonTasks.CommonTasks.LogErrore(ex.Message, "Scrivania");
                model = new WidgetModuloBox()
                {
                    WidgetId = "WdgSmartWorking",
                    Anno = DateTime.Now.Year,
                    GiaScelto = false,
                    HaDiritto = false,
                    Titolo = "Esercizio diritto smart working",
                    Scelta = "",
                    DataCompilazione = null,
                    Bottoni = null
                };
            }

            #endregion

            return model;
        }

        private Bonus100EVM GetWidgetBonus100Data()
        {
            Bonus100EVM model = new Bonus100EVM();

            #region Bonus 100 EURO

            bool haDirittoAlBonus100 = false;
            bool giaCompilato = false;
            string scelta = "";
            DateTime dataCompilazione = DateTime.Now;

            try
            {
                string dtStart = "";
                string dtEnd = "";

                using (digiGappEntities dbDG = new digiGappEntities())
                {
                    var parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "Bonus100DateRange");
                    if (parametro != null)
                    {
                        dtStart = parametro.Valore1;
                        dtEnd = parametro.Valore2;
                    }
                }

                DateTime dataInizio = DateTime.Now;
                DateTime dataFine = DateTime.Now;
                DateTime.TryParseExact(dtStart, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataInizio);
                DateTime.TryParseExact(dtEnd, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataFine);

                if (dataInizio <= DateTime.Now && dataFine >= DateTime.Now)
                {
                    string urlServizio = System.Configuration.ConfigurationManager.AppSettings["URLwiahrss"];

                    Uri u = new Uri(urlServizio);

                    BasicHttpBinding binding = new BasicHttpBinding();
                    EndpointAddress address = new EndpointAddress(u);
                    binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                    binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                    binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

                    binding.MessageEncoding = WSMessageEncoding.Text;
                    binding.TextEncoding = Encoding.UTF8;
                    binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;

                    var _client = new myRaiServiceHub.ServiceBonus100.ezServiceSoapClient(binding, address);

                    if (_client.ClientCredentials != null)
                    {
                        _client.ClientCredentials.Windows.ClientCredential = new NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
                        _client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Delegation;
                    }
                    _client.Open();

                    var requestInterceptor = new InspectorBehavior();
                    _client.Endpoint.Behaviors.Add(requestInterceptor);

                    var clientResponse = _client.TBONUS_100EURO(CommonManager.GetCurrentUserMatricola7chars());

                    if (clientResponse != null && clientResponse.Rows != null && clientResponse.Rows.Count > 0)
                    {
                        var r = clientResponse.Rows[0];

                        string SOGLIA_40_60 = r.ItemArray[13].ToString();
                        string diritto = r.ItemArray[14].ToString();
                        string dichiarazione = r.ItemArray[15].ToString();
                        string risposta = r.ItemArray[16].ToString();
                        string dtRisposta = r.ItemArray[17].ToString();
                        string BONUS_SPETTANTE = r.ItemArray[18].ToString();

                        float bonusSpettante = 0;

                        if (!String.IsNullOrEmpty(BONUS_SPETTANTE))
                        {
                            bonusSpettante = float.Parse(BONUS_SPETTANTE);
                        }

                        if (!String.IsNullOrEmpty(diritto))
                        {
                            diritto = diritto.Trim();
                        }

                        if (!String.IsNullOrEmpty(dichiarazione))
                        {
                            dichiarazione = dichiarazione.Trim();
                        }

                        if (!String.IsNullOrEmpty(risposta))
                        {
                            risposta = risposta.Trim();
                            if (risposta.Equals("\0"))
                            {
                                risposta = "";
                            }
                        }

                        if (!String.IsNullOrEmpty(dtRisposta))
                        {
                            dtRisposta = dtRisposta.Trim();
                            if (dtRisposta.Equals("01/01/0001 00:00:00"))
                            {
                                dtRisposta = "";
                            }
                        }

                        //DIRITTO = ‘S’
                        //SOGLIA_40_60 = ‘40’
                        //BONUS_SPETTANTE > 0
                        //DICHIARAZIONE = ‘S’


                        if (!diritto.Equals("S") ||
                            !dichiarazione.Equals("S"))
                        {
                            // vuol dire che l'utente non deve scegliere
                            haDirittoAlBonus100 = false;
                        }
                        else if (!String.IsNullOrEmpty(risposta) && !String.IsNullOrEmpty(dtRisposta))
                        {
                            // vuol dire che l'utente ha già scelto
                            haDirittoAlBonus100 = true;
                            scelta = risposta;
                            giaCompilato = true;
                            DateTime.TryParseExact(dtRisposta, "dd/MM/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out dataCompilazione);
                        }
                        else if (!String.IsNullOrEmpty(SOGLIA_40_60) && SOGLIA_40_60 != "40")
                        {
                            // non ha diritto
                            haDirittoAlBonus100 = false;
                        }
                        else if (!String.IsNullOrEmpty(BONUS_SPETTANTE) && !(bonusSpettante > 0))
                        {
                            // non ha diritto
                            haDirittoAlBonus100 = false;
                        }
                        else
                        {
                            // vuol dire che l'utente deve scegliere
                            haDirittoAlBonus100 = true;
                        }

                        if (!String.IsNullOrEmpty(risposta) && !String.IsNullOrEmpty(dtRisposta))
                        {
                            // vuol dire che l'utente ha già scelto
                            haDirittoAlBonus100 = true;
                            scelta = risposta;
                            giaCompilato = true;
                        }
                    }
                    else
                    {
                        // se non trova i dati dell'utente non deve mostrare il widget
                        haDirittoAlBonus100 = false;
                    }

                    string requestXML = requestInterceptor.LastRequestXML;
                    string responseXML = requestInterceptor.LastResponseXML;

                    _client.Close();
                }
            }
            catch (Exception ex)
            {
                haDirittoAlBonus100 = false;
            }

            if (haDirittoAlBonus100)
            {
                model = new Bonus100EVM()
                {
                    Anno = DateTime.Now.Year,
                    GiaScelto = giaCompilato,
                    HaDiritto = true,
                    Messaggio = "Premio ai lavoratori dipendenti D.L. 18/20",
                    Scelta = scelta,
                    DataCompilazione = dataCompilazione
                };
            }
            else
            {
                model = new Bonus100EVM()
                {
                    Anno = DateTime.Now.Year,
                    GiaScelto = giaCompilato,
                    HaDiritto = false,
                    Messaggio = "Premio ai lavoratori dipendenti D.L. 18/20",
                    Scelta = scelta,
                    DataCompilazione = dataCompilazione
                };
            }

            #endregion

            return model;
        }

        private string GetSceltaEnumByDescription(string sceltaDescription)
        {
            // sceltaDescription es: LavoratoreDisabile

            ModuloSmart2020SelectionEnum myEnum = (ModuloSmart2020SelectionEnum)Enum.Parse(typeof(ModuloSmart2020SelectionEnum), sceltaDescription);
            int val = (int)myEnum;

            return val.ToString();
        }

        private string GetSceltaEnumByDescriptionOLD(string sceltaDescription)
        {
            // sceltaDescription es: LavoratoreDisabile

            ModuloSmart2020SelectionEnumOLD myEnum = (ModuloSmart2020SelectionEnumOLD)Enum.Parse(typeof(ModuloSmart2020SelectionEnumOLD), sceltaDescription);
            int val = (int)myEnum;

            return val.ToString();
        }

        public ActionResult Index_section4(bool GappClosed)
        {
            SituazioniEAnagraficaModel model = new SituazioniEAnagraficaModel() { GappClosed = GappClosed };
            model.Boxes = new List<BoxSituazioneModel>();
            List<MyRai_Sezioni_Visibili> sezionivisibili = HomeManager.GetSezioniVisibili(CommonManager.GetCurrentUserMatricola());
            model.Visibile = false;
            model.BustaPagaVisibile = false;
            bool schermataFlat = CommonManager.GetParametro<bool>(EnumParametriSistema.AbilitaSchermataFlat);

            if (!schermataFlat)
            {
                model.Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "ANAG").Count() > 0;
                model.BustaPagaVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "BUSTA").Count() > 0;
                model.Boxes.Add(new BoxSituazioneModel()
                {
                    CifraPrincipale = "€89.90",
                    AllaData = "al 24/05",
                    HrefPulsante = "/home",
                    TestoPulsante = "VEDI SPESE",
                    Titolo = "Nota spese - Trasferte",
                    ClasseIcona = "icons icon-briefcase",
                    ColoreIcona = "text-primary",
                    Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "TRASF").Count() > 0
                });
                model.Boxes.Add(new BoxSituazioneModel()
                {
                    CifraPrincipale = "€109.90",
                    AllaData = "al 20/05",
                    HrefPulsante = "/home",
                    TestoPulsante = "VEDI TUTTO",
                    Titolo = "Situazione Debitoria",
                    ClasseIcona = "icons icon-wallet",
                    ColoreIcona = "text-primary",
                    Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "DEB").Count() > 0
                });

                model.Boxes.Add(new BoxSituazioneModel()
                {
                    CifraPrincipale = "€99.00",
                    AllaData = "al 31/05",
                    HrefPulsante = "/home",
                    TestoPulsante = "Vedi tutto",
                    Titolo = "Situazione ?",
                    ClasseIcona = "icons icon-wallet",
                    ColoreIcona = "text-primary",
                    Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "SIT?").Count() > 0
                });
            }

            return View(model);
        }

        public ActionResult GetBoxDetassazione()
        {
            DetassazioneVM model = new DetassazioneVM();

            // se l'utente è Giornalista, Orchestrale o Dirigente
            // non ha diritto alla richiesta di detassazione
            if (Utente.TipoDipendente().Equals("G") ||
                Utente.TipoDipendente().Equals("O") ||
                Utente.TipoDipendente().Equals("D"))
            {
                model.HaDiritto = false;
            }
            else
            {
                string[] valori = CommonManager.GetParametri<string>(EnumParametriSistema.BoxDetassazione);
                string[] messaggi = CommonManager.GetParametri<string>(EnumParametriSistema.BoxDetassazioneMessaggi);

                if (valori != null &&
                    valori.Any() &&
                    messaggi != null &&
                    messaggi.Any())
                {
                    //string msg = valori[0];
                    string dtStart = valori[0];
                    string dtEnd = valori[1];
                    DateTime dataInizio = DateTime.Now;
                    DateTime dataFine = DateTime.Now;
                    DateTime.TryParseExact(dtStart, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataInizio);
                    DateTime.TryParseExact(dtEnd, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataFine);
                    string msg1 = messaggi[0];
                    string msg2 = messaggi[1];

                    if (dataInizio <= DateTime.Now &&
                        dataFine >= DateTime.Now)
                    {
                        using (HRPADBEntities dbDetax = new HRPADBEntities())
                        {
                            string mtr = CommonManager.GetCurrentUserMatricola();
                            var exists = dbDetax.T_DetaxNew.Where(w => w.Matricola_T_DetaxNew.Equals(mtr)).FirstOrDefault();

                            if (exists != null)
                            {
                                // se già valorizzati allora, scelta già effettuata
                                if (exists.Data_T_DetaxNew.HasValue && !String.IsNullOrEmpty(exists.Modello_T_DetaxNew) &&
                                    !exists.Modello_T_DetaxNew.Equals("ND"))
                                {
                                    model.HaDiritto = false;
                                    model.GiaScelto = true;
                                    model.CodiceModello = exists.Modello_T_DetaxNew;
                                    model.Messaggio = msg2;
                                    model.Anno = DateTime.Now.Year;
                                    model.CodiceDetassazione = "DETAX";
                                }
                                else if (!String.IsNullOrEmpty(exists.Modello_T_DetaxNew) &&
                                           exists.Modello_T_DetaxNew.Equals("ND"))
                                {
                                    model.HaDiritto = false;
                                    model.GiaScelto = false;
                                }
                                else
                                {
                                    model.HaDiritto = true;
                                    model.GiaScelto = false;
                                    model.CodiceModello = exists.ModelloAssegnato_T_DetaxNew;

                                    if (exists.ModelloAssegnato_T_DetaxNew.Equals("1C"))
                                    {
                                        msg1 = "Rinuncia tassazione agevolata";
                                    }

                                    model.Messaggio = msg1;
                                    model.Anno = DateTime.Now.Year;
                                    model.CodiceDetassazione = "DETAX";
                                }
                            }
                            else
                            {
                                model.HaDiritto = false;
                            }
                        }
                    }
                }
                else
                {
                    model.HaDiritto = false;
                }
            }

            return View("~/Views/Scrivania/subpartial/boxDetassazione.cshtml", model);
        }

        public ActionResult CancellaNotifica(long id)
        {
            var db = new myRaiData.digiGappEntities();
            var notifica = db.MyRai_Notifiche.Where(x => x.id == id).FirstOrDefault();
            if (notifica != null)
            {
                notifica.data_letta = DateTime.Now;
                if (DBHelper.Save(db, CommonManager.GetCurrentUserMatricola()))
                {
                    NotifichePopupModel m = new NotifichePopupModel(CommonManager.GetCurrentUserMatricola());

                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new
                        {
                            result = "OK",
                            totalNow = m.NotificheTotali,
                            totalNow1 = m.NotificheTotaliTipo1
                        }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = "Errore dati DB"
                }
            };
        }

        public ActionResult CancellaNotificheAll(string matricola, int tipo)
        {
            var db = new myRaiData.digiGappEntities();
            var notifiche = db.MyRai_Notifiche.Where(x => x.matricola_destinatario == matricola);
            if (tipo == 1)
                notifiche = notifiche.Where(x => x.tipo == 1);
            else
                notifiche = notifiche.Where(x => x.tipo == 2 || x.tipo == null);

            if (notifiche != null)
            {
                foreach (var item in notifiche)
                {
                    item.data_letta = DateTime.Now;
                }
                if (DBHelper.Save(db, CommonManager.GetCurrentUserMatricola()))
                {
                    NotifichePopupModel m = new NotifichePopupModel(CommonManager.GetCurrentUserMatricola());
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new
                        {
                            result = "OK",
                            totalNow = m.NotificheTotali,
                            totalNow1 = m.NotificheTotaliTipo1
                        }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = "Nessuna notifica"
                }
            };
        }

        public ActionResult GetNotifiche(int tipo)
        {
            NotifichePopupModel model = new NotifichePopupModel(CommonManager.GetCurrentUserMatricola());
            model.tipoNotifiche = tipo;

            if (tipo == 1)
            {
                model.Notifiche = model.Notifiche.Where(x => x.tipo == 1).ToList();
                model.NotificheTotali = model.Notifiche.Count();
                model.Notifiche = model.Notifiche.Take(3).ToList();
            }
            if (tipo == 2)
            {
                model.Notifiche = model.Notifiche.Where(x => x.tipo == 2 || x.tipo == null).ToList();
                model.NotificheTotali = model.Notifiche.Count();
                model.Notifiche = model.Notifiche.Take(3).ToList();
            }
            return View("~/Views/Shared/header2_notifiche.cshtml", model);
        }

        public ActionResult GetNews()
        {
            RaipermeNewsModel model = ScrivaniaManager.GetRaipermeNewsModel("Main news");
            if (model.NewsItems.Count == 0)
                return Content("NONEWS");
            else
                return View(model);
        }

        public ActionResult GetNewsReleaseNote()
        {
            RaipermeNewsModel model = ScrivaniaManager.GetRaipermeNewsModel("Release notes");
            if (model.NewsItems.Count == 0)
                return View(model);
            else
                return View(model);
        }
    }

    public class ScrivaniaControllerScope : SessionScope<ScrivaniaControllerScope>
    {
        public DateTime? DataChiusura1 { get; set; }
    }
}