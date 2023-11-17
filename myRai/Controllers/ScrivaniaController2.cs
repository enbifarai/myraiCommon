using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRai.Business;
using myRai.DataAccess;
using myRai.Controllers;

using myRaiData;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using myRai.DataControllers;

namespace myRai.Models
{
    public partial class ScrivaniaBisController : BaseCommonController
    {
        myRai.Models.ModelDash pr = new Models.ModelDash();
        myRai.Models.daApprovareModel daApprov;
        WSDigigapp datiBack = new WSDigigapp();
        digigappWS_ws1.wAPI_WS1 datiBack_ws1 = new digigappWS_ws1.wAPI_WS1();

        MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

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
            }
            catch (Exception ex)
            {

            }
        }


        public ActionResult Index_section1()
        {
            try
            {
                wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
                SectionAlertModel model = new SectionAlertModel() { Alerts = new List<AlertModel>() };
                List<MyRai_Sezioni_Visibili> sezionivisibili = HomeManager.GetSezioniVisibili(CommonManager.GetCurrentUserMatricola());

                //se utente è boss, alert per COSE DA APPROVARE
                if (Utente.IsBoss())
                {
                    Tuple<int, int> T = ScrivaniaManager.GetTotaleEccezioniDaApprovare();
                    List<Sede> sedi = Utility.GetSediGappResponsabileList();
                    int TotalPdfPresenzeDaVisionare = 0;
                    foreach (Sede s in sedi)
                    {
                        TotalPdfPresenzeDaVisionare += ResocontiManager.QuantiPdfDaGenerare(s.CodiceSede).Count();
                    }

                    string dataIntro = "Queste sono le richieste da approvare. Al momento hai " +
                         (TotalPdfPresenzeDaVisionare + T.Item1).ToString() + " richieste, di cui " + T.Item2
                         + " scadute";
                    model.Alerts.Add(new AlertModel()
                    {
                        CifraPrincipale = (TotalPdfPresenzeDaVisionare + T.Item1).ToString(),
                        ClasseIcona = "icons icon-like",
                        ColoreClasseIcona = "cda",
                        HrefPulsante = "/approvazione",
                        TestoPulsante = "VEDI",
                        Titolo = "In approvazione (1° liv)",
                        TraParentesi = "(" + T.Item2 + " richieste scadute)",
                        Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "CDA").Count() > 0,
                        TipoAlert = 2,
                        intro_datastep = "3",
                        intro_dataintro = dataIntro,
                        AriaLabelSummary = "Hai " + (TotalPdfPresenzeDaVisionare + T.Item1).ToString() + " richieste da approvare, di cui " + T.Item2
                         + " scadute",
                        AriaLabelPulsante = "Vai alle richieste"
                    });
                }
                else
                {
                    if (Utente.IsBossLiv2())
                    {
                        var db = new myRaiData.digiGappEntities();
                        List<string> sedi = CommonManager.GetSediL2();


                        int inApp = db.MyRai_Richieste.Where(a => sedi.Contains(a.codice_sede_gapp) && a.richiedente_L1 && a.id_stato == 10).Count();
                        int inAppScadute = db.MyRai_Richieste.Where(a => sedi.Contains(a.codice_sede_gapp) && a.richiedente_L1 && a.id_stato == 10 && a.scaduta).Count();
                        string intro = "Queste sono le richieste da approvare. Al momento hai " + inApp.ToString() + " richieste, di cui " + inAppScadute + " scadute";
                        model.Alerts.Add(new AlertModel()
                        {
                            CifraPrincipale = inApp.ToString(),
                            ClasseIcona = "icons icon-like",
                            ColoreClasseIcona = "cda",
                            HrefPulsante = "/approvazione",
                            TestoPulsante = "VEDI",
                            Titolo = "In approvazione (1° liv)",
                            TraParentesi = "(" + inAppScadute + " richieste scadute)",
                            Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "CDA").Count() > 0,
                            TipoAlert = 2,
                            intro_datastep = "3",
                            intro_dataintro = intro,
                            AriaLabelSummary = "Hai " + inApp.ToString() + " richieste da approvare, di cui " + inAppScadute + " scadute",
                            AriaLabelPulsante = "Vai alle richieste"
                        });

                    }
                }

                if (Utente.TipoDipendente() != "D")
                {

                    // in evidenza DA GIUSTIFICARE                   
                    ModelDash m = new ModelDash();
                    m = ScrivaniaManager.GetTotaliEvidenze(m);

                    if (m.TotaleEvidenzeDaGiustificareSoloAssIng != 0)
                    {
                        string dataIntro = "Queste sono le tue giornate in evidenza. Al momento hai un totale di " +
                              m.TotaleEvidenzeDaGiustificare.ToString() + " giornate, di cui " + m.TotaleEvidenzeDaGiustificareSoloAssIng +
                              " sono assenze ingiustificate.";
                        var d = m.listaEvidenze.data.giornate.Where(x => x.TipoEcc == digigappWS_ws1.TipoEccezione.AssenzaIngiustificata)
                            .OrderBy(a => a.data).FirstOrDefault();

                        model.Alerts.Add(new AlertModel()
                        {
                            CifraPrincipale = m.TotaleEvidenzeDaGiustificare.ToString(),
                            ClasseIcona = "icons icon-fire",
                            ColoreClasseIcona = "gioev",
                            HrefPulsante = "JavaScript:ShowPopupInizialeGoDate('" + CommonManager.GetParametro<string>(EnumParametriSistema.MessaggioAssenteIngiustificato) + "',false,'" + d.data.ToString("dd/MM/yyyy") + "')",
                            TestoPulsante = "GESTISCI",
                            Titolo = "Giornate in Evidenza",
                            TraParentesi = "(" + m.TotaleEvidenzeDaGiustificareSoloAssIng + " da giustificare)",
                            Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "GIOEV").Count() > 0,
                            TipoAlert = 1,
                            intro_datastep = "1",
                            intro_dataintro = dataIntro,
                            AriaLabelSummary = "Hai " + m.TotaleEvidenzeDaGiustificare.ToString() + " giornate in evidenza",
                            AriaLabelPulsante = "Gestisci le tue giornate in evidenza"
                        });
                    }
                    else
                    {
                        if (m.TotaleEvidenzeDaGiustificare != 0)
                        {
                            string dataIntro = "Queste sono le tue giornate in evidenza. Al momento hai un totale di " +
                              m.TotaleEvidenzeDaGiustificare.ToString() + " giornate, di cui " + m.TotaleEvidenzeDaGiustificareSoloAssIng +
                              " sono assenze ingiustificate.";
                            model.Alerts.Add(new AlertModel()
                            {
                                CifraPrincipale = m.TotaleEvidenzeDaGiustificare.ToString(),
                                ClasseIcona = "icons icon-fire",
                                ColoreClasseIcona = "gioev",
                                HrefPulsante = "/Home",
                                TestoPulsante = "VEDI",
                                Titolo = "Giornate in Evidenza",
                                TraParentesi = "(" + m.TotaleEvidenzeDaGiustificareSoloAssIng + " da giustificare)",
                                Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "GIOEV").Count() > 0,
                                TipoAlert = 1,
                                intro_datastep = "1",
                                intro_dataintro = dataIntro,
                                AriaLabelSummary = "Hai " + m.TotaleEvidenzeDaGiustificare.ToString() + " giornate in evidenza",
                                AriaLabelPulsante = "Controlla le tue giornate in evidenza"
                            });
                        }
                        else
                        {
                            string dataIntro = "Queste sono le tue giornate in evidenza. Al momento hai un totale di " +
                          m.TotaleEvidenzeDaGiustificare.ToString() + " giornate, di cui " + m.TotaleEvidenzeDaGiustificareSoloAssIng +
                          " sono assenze ingiustificate.";
                            model.Alerts.Add(new AlertModel()
                            {
                                CifraPrincipale = m.TotaleEvidenzeDaGiustificare.ToString(),
                                ClasseIcona = "icons icon-emotsmile",
                                ColoreClasseIcona = "gioev",
                                HrefPulsante = m.TotaleEvidenzeDaGiustificareSoloAssIng == 0 ? "javascript:ShowPopup('',0)" : "JavaScript:ShowPopupIniziale('" + CommonManager.GetParametro<string>(EnumParametriSistema.MessaggioAssenteIngiustificato) + "')",
                                TestoPulsante = "EFFETTUA UNA RICHIESTA",
                                Titolo = "Giornate in Evidenza",
                                TraParentesi = "(" + m.TotaleEvidenzeDaGiustificareSoloAssIng + " da giustificare)",
                                Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "GIOEV").Count() > 0,
                                TipoAlert = 1,
                                intro_datastep = "1",
                                intro_dataintro = dataIntro,
                                AriaLabelSummary = "Hai " + m.TotaleEvidenzeDaGiustificare.ToString() + " giornate in evidenza",
                                AriaLabelPulsante = "Effettua una richiesta"
                            });
                        }
                    }
                }

                //se utente è boss liv 2, DA FIRMARE
                if (Utente.IsBossLiv2())
                {
                    DaFirmareModel dfmodel = new DaFirmareModel();
                    dfmodel.Sedi = DaFirmareManager.GetDaFirmareModel();

                    TotaliDaFirmareModel TotaliDaFirmare = DaFirmareManager.GetTotaliDaFirmareModel(dfmodel.Sedi);

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
                    int Inweek = dfmodel.Sedi.SelectMany(se => se.PeriodiPDF).Where(d => d.DateStart == Dstart && d.DateEnd == Dend).Count();
                    string dataIntro = "Questi sono i PDF delle eccezioni e delle presenze che hai da esaminare. Ci sono "
                         + dfmodel.Sedi.SelectMany(se => se.PeriodiPDF).Count().ToString() + " documenti, di cui " +
                         TotaliDaFirmare.Totale.ToString() + " da firmare.";
                    model.Alerts.Add(new AlertModel()
                    {
                        CifraPrincipale = dfmodel.Sedi.SelectMany(se => se.PeriodiPDF).Count().ToString(),
                        ClasseIcona = "icons icon-note",
                        ColoreClasseIcona = "cdf",
                        HrefPulsante = "/Firma",
                        TestoPulsante = "VEDI",
                        Titolo = "In firma (2° liv)",
                        TraParentesi = "(" + TotaliDaFirmare.Totale.ToString() + " esaminate)",
                        //  TraParentesi = "(" + Inweek.ToString() + " tra " + Dstart.ToString("dd-MM") + "/" + Dend.ToString("dd-MM") + ")",
                        Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "CDF").Count() > 0,
                        TipoAlert = 2,
                        intro_datastep = "4",
                        intro_dataintro = dataIntro,
                        AriaLabelSummary = "Hai " + dfmodel.Sedi.SelectMany(se => se.PeriodiPDF).Count().ToString() + " PDF delle eccezioni da esaminare, di cui " + TotaliDaFirmare.Totale.ToString() + " da firmare.",
                        AriaLabelPulsante = "Vedi le eccezioni"
                    });
                }
                Boolean abil = Utente.IsAbilitatoGapp();

                // se utente è a Quadratura Settimanale
                if (Utente.GetQuadratura() == Quadratura.Settimanale &&
                    EccezioniManager.GetMinutiCarenzaPerSede(Utente.SedeGapp(), DateTime.Now) <= 0)
                {
                    // DateTime datainizio = Convert.ToDateTime("03/04/2017");
                    // DateTime datafine = Convert.ToDateTime("09/04/2017");

                    DateTime[] Da = CommonManager.GetIntervalloSettimanaleSede();

                    DateTime datainizio = Da[0];
                    DateTime datafine = Da[1];
                    //DettaglioSettimanaleModel dsettModel = new Models.DettaglioSettimanaleModel(
                    //    wcf1.PresenzeSettimanali(CommonManager.GetCurrentUserMatricola(), datainizio.ToString("ddMMyyyy"), 
                    //    datainizio.AddDays(6).ToString("ddMMyyyy")));
                    DettaglioSettimanaleModel dsettModel = new Models.DettaglioSettimanaleModel(
                       wcf1.GetPresenzeSettimanaliProtected(CommonManager.GetCurrentUserMatricola(), datainizio.ToString("ddMMyyyy"),
                       datainizio.AddDays(6).ToString("ddMMyyyy"), Utente.DataInizioValidita(), datainizio));

                    string dataIntro = "Questo è il tuo riepilogo settimanale. Nella settimana tra il "
                          + datainizio.ToString("dd-MM") + " e il " + datafine.ToString("dd-MM") + " hai una differenza di "
                          + dsettModel.DeltaTotale + " minuti.";
                    model.Alerts.Add(new AlertModel()
                    {
                        CifraPrincipale = dsettModel.DeltaTotale,
                        ClasseIcona = "icons icon-user-following",
                        ColoreClasseIcona = "prse",
                        HrefPulsante = abil ? "/Home" : "/Timbrature",
                        TestoPulsante = abil ? "VEDI TUTTE" : "RIEPILOGO",
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
                    });
                }
                //se utente è a quadratura giornaliera
                if (Utente.GetQuadratura() == Quadratura.Giornaliera)
                {

                    int bilancio = Utente.GetROH() - Utente.GetPOH();
                    int hrs = Math.Abs(bilancio) / 60;
                    int min = Math.Abs(bilancio) - (60 * hrs);
                    int POHmese = Utente.GetPOHdays().Where(x => x.Date.Month == DateTime.Now.Month).Count();
                    int maxPOHmese = CommonManager.GetParametro<int>(EnumParametriSistema.POHperMese);

                    string dataIntro = "Questo è il tuo bilancio tra i permessi POH e i recuperi ROH. In questo momento " +
                          " il tuo bilancio è di " +
                          (bilancio > 0 ? "+ " : bilancio == 0 ? " " : "- ") + hrs.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0')
                          + " minuti, e nel mese corrente hai usufruito di " + POHmese + " POH";
                    model.Alerts.Add(new AlertModel()
                    {
                        CifraPrincipale = (bilancio > 0 ? "+ " : bilancio == 0 ? " " : "- ") + hrs.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0'),
                        ClasseIcona = bilancio >= 0 ? "icons icon-user-following" : "icons icon-clock",
                        ColoreClasseIcona = "prse",
                        HrefPulsante = abil ? "/Home" : "/feriepermessi",
                        TestoPulsante = abil ? "GESTISCI" : "RIEPILOGO",
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
                    });
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
    }
  
}