using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using myRaiService;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using MyRaiServiceInterface.MyRaiServiceReference1;
using CommonManager = myRaiHelper.CommonHelper;
using Utente = myRaiHelper.UtenteHelper;

namespace myRai.Controllers
{
    public class FeriePermessiController : BaseCommonController
    {
        private StampaFeriePermessiController stampaCalendarioController = new StampaFeriePermessiController();       

        public ActionResult Index(int? anno)
        {
            if (anno != null)
            {
                if (anno < DateTime.Now.Year - 4 || anno > DateTime.Now.Year)
                {
                    return RedirectToAction("index");
                }
                SessionHelper.Set(SessionVariables.AnnoFeriePermessi, (int)anno);
            }
            else
                SessionHelper.Set(SessionVariables.AnnoFeriePermessi, null);

            FeriePermessiModel model = FeriePermessiManager.GetFeriePermessiModel();

            List<string> ecc = new List<string>( );
            ecc.Add( "FEDO" );
            ecc.Add( "FECE" );
            ecc.Add( "MRCE" );
            ecc.Add( "MNCE" );
            ecc.Add( "MFCE" );
            ecc.Add( "MRDO" );
            ecc.Add( "MRRI" );
            ecc.Add( "MR" );
            ecc.Add( "MNDO" );
            ecc.Add( "MNRI" );
            ecc.Add( "MN" );

            SessionHelper.Set( SessionVariables.GetContatoriEccezioni , FeriePermessiManager.GetContatoriEccezioni( ecc ) );

            return View(model);
        }

        public ActionResult issw(int idrichiesta)
        {
            var db = new myRaiData.digiGappEntities();
            var ric = db.MyRai_Richieste.Where(x => x.id_richiesta == idrichiesta).FirstOrDefault();
            bool esito = false;
            if (ric != null)
            {
                try
                {
                    esito = Utente.IsSmartWorker(ric.periodo_dal);
                }
                catch (Exception ex)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = esito, dataecc = ric.periodo_dal.ToString("dd/MM/yyyy"), errore=ex.ToString() }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = esito , dataecc=ric.periodo_dal.ToString("dd/MM/yyyy")}
            };
        }

        public ActionResult GetFerie()
        {
            FerieDipendente model = FeriePermessiManager.GetFerie();

            MyRaiServiceInterface.MyRaiServiceReference1.GetContatoriEccezioniResponse dati = (MyRaiServiceInterface.MyRaiServiceReference1.GetContatoriEccezioniResponse) SessionHelper.Get( SessionVariables.GetContatoriEccezioni );

            if (dati.Esito)
            {
                if (dati.ContatoriEccezioni != null && dati.ContatoriEccezioni.Any())
                {
                    var fece = dati.ContatoriEccezioni.Where( w => w.Esito && w.CodiceEccezione.Equals( "FECE" ) ).FirstOrDefault( );
                    var fedo = dati.ContatoriEccezioni.Where( w => w.Esito && w.CodiceEccezione.Equals( "FEDO" ) ).FirstOrDefault( );

                    if (fece != null)
                    {
                        model.Ceduti = float.Parse( fece.Totale );
                    }

                    if ( fedo != null )
                    {
                        model.Donati = float.Parse( fedo.Totale );
                    }

                }
            }

            return View("~/Views/FeriePermessi/subpartial/Ferie.cshtml", model);
        }

        public ActionResult GetPermessiExFest()
        {
            FerieDipendente model = FeriePermessiManager.GetPermessiExFest();
            return View("~/Views/FeriePermessi/subpartial/permessiExFest.cshtml", model);
        }

        public ActionResult GetPermessiRetr()
        {
            FerieDipendente model = FeriePermessiManager.GetPermessiRetr();
            return View("~/Views/FeriePermessi/subpartial/permessiRetr.cshtml", model);
        }

        public ActionResult GetPermessiGiornalisti()
        {
            FerieDipendente model = FeriePermessiManager.GetPermessiGiornalisti();
            return View("~/Views/FeriePermessi/subpartial/permessiGiornalisti.cshtml", model);
        }

        public ActionResult GetRiposi()
        {
            FerieDipendente model = FeriePermessiManager.GetMancatiRiposi();
            return View("~/Views/FeriePermessi/subpartial/recuperiMancatiRiposi.cshtml", model);
        }
        public ActionResult GetFestivi()
        {
            FerieDipendente model = FeriePermessiManager.GetMancatiFestivi();
            return View("~/Views/FeriePermessi/subpartial/recuperiMancatiFestivi.cshtml", model);
        }
        public ActionResult GetNonLavorati()
        {
            FerieDipendente model = FeriePermessiManager.GetMancatiNonLavorati();
            return View("~/Views/FeriePermessi/subpartial/recuperiNonLavorati.cshtml", model);
        }

        public ActionResult GetCalendario(int? AnnoRichiesto = null, int? MeseRichiesto = null, bool fromscrivania = false)
        {
            CalendarioFerie model = new CalendarioFerie();
            if (fromscrivania)
            {
                model = FeriePermessiManager.GetCalendarioSituazioneEccezioni(AnnoRichiesto, MeseRichiesto);

                try
                {
                    pianoFerie resp = ( pianoFerie )SessionHelper.Get( SessionVariables.GetPianoFerieWrapped );
                    if ( resp != null && resp.esito )
                    {
                        var ferie = resp.dipendente.ferie;

                        if ( ferie != null )
                        {
                            var giornate = ferie.giornate;

                            if ( giornate != null && giornate.Any( ) )
                            {
                                if ( model.Giornate != null &&
                                    model.Giornate.Any( ) )
                                {
                                    foreach ( var day in model.Giornate )
                                    {
                                        string dataStringa = String.Format( "{0}-{1}" , day.DataEccezione.Day.ToString( ).PadLeft( 2 , '0' ) , day.DataEccezione.Month.ToString( ).PadLeft( 2 , '0' ) );

                                        var item = giornate.Where( w => w.dataTeorica.Equals( dataStringa ) ).FirstOrDefault( );

                                        if ( item != null )
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
                catch ( Exception ex )
                {

                }
                
                return View("~/Views/FeriePermessi/subpartial/CalendarioNoFerie.cshtml", model);
            }
            else
            {
                model = FeriePermessiManager.GetCalendario(AnnoRichiesto, MeseRichiesto);
                return View("~/Views/FeriePermessi/subpartial/Calendario.cshtml", model);
            }
        }

        public ActionResult GetCalendarioAnnuale(int anno)
        {
            CalendarioFerie model = FeriePermessiManager.GetCalendarioAnnuale(anno);
            FeriePermessiControllerScope.Instance.giorniCalendario = new List<CalendarioDay>();
            FeriePermessiControllerScope.Instance.giorniCalendario = model.DaysShowed;
            FeriePermessiControllerScope.Instance.anno = anno;
            FeriePermessiControllerScope.Instance.tipiGiornata = new List<TipoPermessoFerieUsato>();
            FeriePermessiControllerScope.Instance.tipiGiornata = model.tipiGiornataSel;

            return View("CalendarioAnnuale", model);
        }

        public CalendarioFerie GetCalendarioAnnualPFmodel(int anno)
        {
            return FeriePermessiManager.GetCalendarioAnnualPFmodel(anno);
        }

        public bool RisultaUlteriormenteRipianificata(DateTime D, string matr)
        {
            var db = new myRaiData.digiGappEntities();
            var list = db.MyRai_Richieste.Where(x => x.matricola_richiesta == matr 
                        && x.MyRai_Eccezioni_Richieste.Any(z=>z.data_eccezione==D)).ToList();

            foreach (var ric in list)
            {
                if (ric.MyRai_Eccezioni_Richieste.Any(x => x.azione == "C" && x.DataRipianificazione != null 
                && x.id_stato == 20))
                {
                    return true;
                }
            }
            return false;

        }

        public ActionResult isokrip(string datascelta)
        {
            DateTime D;
            DateTime.TryParseExact(datascelta, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);
            string matr = CommonManager.GetCurrentUserMatricola();
            bool ur = RisultaUlteriormenteRipianificata(D, matr);

            

            string msg = "Il giorno " + datascelta + " è già oggetto di ripianificazione";

            var db = new myRaiData.digiGappEntities();
            var rip= db.MyRai_PianoFerieBatch.Where(x =>
                                                x.data_eccezione == D &&
                                                x.provenienza.Contains("Ripianificato") &&
                                                x.matricola == matr
                                                ).FirstOrDefault();
            if (rip != null && !ur)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = false, error =  msg}
                };
            }

            var rich = db.MyRai_Richieste.Where(x =>
                            x.matricola_richiesta == matr &&
                            x.MyRai_Eccezioni_Richieste.Any(z => z.DataRipianificazione == D)).FirstOrDefault();

            if (rich != null && !ur)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = false, error = msg }
                };
            }
            else

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = true }
                };

        }
        public ActionResult GetCalendarioAnnualePF(int anno)
        {
            CalendarioFerie model = GetCalendarioAnnualPFmodel(anno);
            return View("CalendarioAnnualePF", model);
        }

        public ActionResult GetAlertArretrati()
        {
            CalendarioFerie model = GetCalendarioAnnualPFmodel(DateTime.Now.Year);
            return View("_alertarretrati", model);
        }
        public ActionResult getpf2021()
        {
            PianoFerieExt2021Model model = new PianoFerieExt2021Model();

            WSDigigapp service = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(myRaiHelper.EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(myRaiHelper.EnumParametriSistema.AccountUtenteServizio)[1])
            };
            string matr = CommonManager.GetCurrentUserMatricola();
            pianoFerie response = ServiceWrapper.GetPianoFerieWrapped(service, matr,
            "01012021", 75, UtenteHelper.TipoDipendente());

            string[] par = CommonManager.GetParametri<string>(myRaiHelper.EnumParametriSistema.OrariGiornateNoPianoFerie);

            string orariFree = par[0] != null ? par[0] : "";
            string giornateFree = par[1] != null ? par[1] : "";

            var riposi = response.dipendente.ferie.giornate.Where(x => orariFree.Split(',').Contains(x.orarioReale) || giornateFree.Split(',').Contains(x.tipoGiornata.Trim()));
            foreach (var r in riposi)
            {
                string data = r.dataTeorica.Trim().Replace("-", "/") + "/" + 2021;
                DateTime D;
                if (DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D))
                {
                    model.DatesOff.Add(D);
                }
            }

            CalendarioFerie cal = new CalendarioFerie();
            GetStatoPianoFerie(cal, 2020);
            model.statopf = (int)cal.PianoFerieDip.StatoPianoFerie;
            var db = new myRaiData.digiGappEntities();

            model.GiorniPFDB = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matr && x.data.Year == 2021).ToList();



            //pianoFerie response2 = ServiceWrapper.GetPianoFerieWrapped(service, matr,
            //"01012020", 75, Utente.TipoDipendente());
            foreach (var item in response.dipendente.ferie.giornate)
            {
                if (String.IsNullOrWhiteSpace(item.codiceVisualizzazione)) continue;
                if ("FE,PR,PF,PX,RR,RF".Split(',').Contains( item.codiceVisualizzazione))
                    model.GappDays.Add(new GappDay() {
                        Ecc = item.codiceVisualizzazione + (item.quotaGiornata!=null?item.quotaGiornata.Trim():""),
                        Data=new DateTime(2021,
                        Convert.ToInt32( item.dataTeorica.Substring(3,2)),
                        Convert.ToInt32( item.dataTeorica.Substring(0,2))
                        )
                    });
            }
            return View(model);
        }

        public ActionResult Sconsolida(int anno)
        {
            String matricola = CommonHelper.GetCurrentUserMatricola();
            var db = new myRaiData.digiGappEntities();
            var PF = db.MyRai_PianoFerie.Where(x => x.matricola == matricola && x.anno == anno).FirstOrDefault();
            if (PF == null)
            {
                return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { result = false, error = "Piano ferie non trovato" } };
            }
            else
            {
                try
                {
                    PF.data_consolidato = null;
                    db.SaveChanges();
                    return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { result = true } };
                }
                catch (Exception ex)
                {
                    return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { result = false, error = ex.Message } };
                }
            }
        }
        public ActionResult Consolida(int anno,string cambiTurno)
        {
            String matricola = CommonManager.GetCurrentUserMatricola();
            var db = new myRaiData.digiGappEntities();
            string rep = Models.Utente.Reparto();
            if (String.IsNullOrWhiteSpace(rep) || rep.Trim() == "0" || rep.Trim() == "00") rep = null;

           
            if (!String.IsNullOrWhiteSpace(cambiTurno) && Utente.TipoDipendente()=="G")
            {
                var giaSalvati = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matricola && x.data.Year == anno).ToList();
                if (giaSalvati.Any())
                {
                    foreach (var day in giaSalvati)
                        day.data_swap_turno = null;
                }
                string[] coppie = cambiTurno.Split(',');
                foreach (var coppia in coppie)
                {
                    string data1 = coppia.Split('-')[0];
                    string data2 = coppia.Split('-')[1];
                    DateTime D1, D2;
                    bool e1= DateTime.TryParseExact(data1, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D1);
                    bool e2= DateTime.TryParseExact(data2, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D2);
                    if (e1 && e2)
                    {
                        var row =db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matricola && x.data== D1).FirstOrDefault();
                        if (row != null)
                            row.data_swap_turno = D2;
                    }
                }
                db.SaveChanges();
            }
            string SedeGappUtente = HomeManager.GetSedeGappPrecedente(CommonManager.GetCurrentUserMatricola(), DateTime.Now);
            if (SedeGappUtente==null)
                SedeGappUtente= myRai.Models.Utente.SedeGapp(DateTime.Now) + rep;
            else
                SedeGappUtente = SedeGappUtente + rep;

            var PF = db.MyRai_PianoFerie.Where(x => x.matricola == matricola && x.anno == anno).FirstOrDefault();
            if (PF != null)
            {
                PF.data_consolidato = DateTime.Now;
                PF.sedegapp = SedeGappUtente;// myRai.Models.Utente.SedeGapp(DateTime.Now) + rep;
                db.SaveChanges();
                return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { result = true } };
            }
            else
            {
                try
                {
                   

                    myRaiData.MyRai_PianoFerie p = new myRaiData.MyRai_PianoFerie()
                    {
                        anno = DateTime.Now.Year,
                        matricola = matricola,
                        data_consolidato = DateTime.Now,
                        sedegapp = SedeGappUtente// myRai.Models.Utente.SedeGapp(DateTime.Now) + rep
                    };
                    db.MyRai_PianoFerie.Add(p);
                    db.SaveChanges();
                    return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { result = true } };
                }
                catch (Exception ex)
                {
                    return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { result = false, error = ex.Message } };
                }
            }
        }

        private void GetStatoPianoFerie(CalendarioFerie model, int anno)
        {
            FeriePermessiManager.GetStatoPianoFerie(model, anno);
        }

        private void GetGiorniPianoFerieSalvati(CalendarioFerie model, int anno)
        {
            FeriePermessiManager.GetGiorniPianoFerieSalvati(model, anno);
        }

        public void GetpercentualiPF(CalendarioFerie model, int anno)
        {
            FeriePermessiManager.GetpercentualiPF(model, anno);
        }

        public ActionResult removeDayPianoFerie(string date)
        {
            System.Threading.Thread.Sleep(1000);
            string matricola = CommonHelper.GetCurrentUserMatricola();
            DateTime D;
            if (DateTime.TryParseExact(date, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D))
            {
                string esito = RimuoviGiornoPianoFerie(matricola, D);
                if (esito == null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = true }
                    };
                }
                else
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = false, error = esito }
                    };
                }
            }
            else return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = false, error = "Data non valida" }
            };


        }

        private string RimuoviGiornoPianoFerie(string matricola, DateTime d)
        {
            var db = new myRaiData.digiGappEntities();

            try
            {
                var gg = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matricola && x.data == d).ToList();

                foreach (var g in gg) db.MyRai_PianoFerieGiorni.Remove(g);

                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public ActionResult addDayPianoFerie(string date,string ecc)
        {
            System.Threading.Thread.Sleep(1000);
            DateTime D;
            if (DateTime.TryParseExact(date, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D))
            {
                string matricola = CommonHelper.GetCurrentUserMatricola();
                if (!EsisteGiornoPianoFerie(matricola, D))
                {
                    string esito = InsertGiornoPianoFerie(matricola, D,ecc);
                    if (esito == null)
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { result = true }
                        };
                    }
                    else
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { result = false, error = esito }
                        };
                    }
                }
                else return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = false, error = "Giorno già esistente" }
                };
            }
            else return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = false, error = "Data non valida" }
            };
        }

        private string InsertGiornoPianoFerie(string matricola, DateTime d, string eccezione)
        {
            var db = new myRaiData.digiGappEntities();
            try
            {
                myRaiData.MyRai_PianoFerieGiorni g = new myRaiData.MyRai_PianoFerieGiorni()
                {
                    data = d,
                    data_inserimento = DateTime.Now,
                    matricola = matricola,
                    eccezione=eccezione
                };
                db.MyRai_PianoFerieGiorni.Add(g);
                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "InsertGiornoPianoFerie"
                });
                return ex.Message;
            }
        }

        private bool EsisteGiornoPianoFerie(string matricola, DateTime d)
        {
            var db = new myRaiData.digiGappEntities();
            return (db.MyRai_PianoFerieGiorni.Any(x => x.matricola == matricola && x.data == d));
        }

        public ActionResult GetFeriePermessiGraph()
        {
            GraficiFerieModel model = FeriePermessiManager.GetGraficiFerieModel();

            if (model.PianoFerie.dipendente.ferie.visualizzaPermessiGiornalisti)
            {
                model.IsGiornalista = true;
                model.TotalePXC = model.PianoFerie.dipendente.ferie.permessiGiornalistiRecupero;
                model.TotaleMN = model.PianoFerie.dipendente.ferie.mancatiNonLavoratiSpettanti;
            }

            return View("~/Views/FeriePermessi/subpartial/FeriePermessiGraph.cshtml", model);
        }

        public ActionResult StampaPdf()
        {
            try
            {

                byte[] bytes = stampaCalendarioController.StampaPdf(FeriePermessiControllerScope.Instance.giorniCalendario, FeriePermessiControllerScope.Instance.tipiGiornata, FeriePermessiControllerScope.Instance.anno, Server.MapPath("~/assets/img/rai.png"));
                MemoryStream ms = new MemoryStream(bytes);

                return new FileStreamResult(ms, "application/pdf") { FileDownloadName = "StampaCalendarioFerie.pdf" };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region Pianifica piano ferie
        [HttpGet]
        public ActionResult PianificaFerie()
        {
            CalendarioFerie model = FeriePermessiManager.GetCalendarioAnnuale(DateTime.Now.Year);
            return View("~/Views/FeriePermessi/PianificaFerie.cshtml", model);
        }
        #endregion

        public ActionResult GetContatori()
        {
            var client = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

            string matricola = CommonHelper.GetCurrentUserMatricola();
            string tipoDip = UtenteHelper.TipoDipendente();

            DateTime fromDate = new DateTime(DateTime.Today.Year, 1, 1);
            DateTime toDate = DateTime.Today;
            //string[] eccezioni = new string[] { 1"FECE", 2"MRCE", 3"MNCE", "MFCE", 2"MRDO", 2"MRRI", 3"MNDO", 3"MNRI" };
            List<string> eccezioni = new List<string>();

            eccezioni.Add("FECE");

            eccezioni.Add("MRCE");
            eccezioni.Add("MRDO");
            eccezioni.Add("MRRI");

            if (tipoDip != "D")
            {
                eccezioni.Add("MNCE");
                eccezioni.Add("MNDO");
                eccezioni.Add("MNRI");
            }

            if (tipoDip != "G")
            {
                eccezioni.Add("MFCE");
            }

            MyRaiServiceInterface.MyRaiServiceReference1.GetContatoriEccezioniResponse result = client.GetContatoriEccezioni(matricola, fromDate, toDate, eccezioni.ToArray());

            List<EccezioniGroupCounter> groups = null;
            if (result != null && result.Esito)
            {
                groups = new List<EccezioniGroupCounter>();
                var fe = result.ContatoriEccezioni.Where(x => x.CodiceEccezione.StartsWith("FE"));
                if (fe.Any())
                {
                    var group = new EccezioniGroupCounter();
                    group.Nome = "Ferie";
                    group.Eccezioni.AddRange(fe.Select(x => new EccezioniCounter() { CodEccezione = x.CodiceEccezione, Response = x }));
                    groups.Add(group);
                }

                var mr = result.ContatoriEccezioni.Where(x => x.CodiceEccezione.StartsWith("MR"));
                if (mr != null)
                {
                    var group = new EccezioniGroupCounter();
                    group.Nome = "Mancati riposi";
                    group.Eccezioni.AddRange(mr.Select(x => new EccezioniCounter() { CodEccezione = x.CodiceEccezione, Response = x }));
                    groups.Add(group);
                }

                var mn = result.ContatoriEccezioni.Where(x => x.CodiceEccezione.StartsWith("MN"));
                if (mn != null)
                {
                    var group = new EccezioniGroupCounter();
                    group.Nome = "Mancati non lavorati";
                    group.Eccezioni.AddRange(mn.Select(x => new EccezioniCounter() { CodEccezione = x.CodiceEccezione, Response = x }));
                    groups.Add(group);
                }

                var mf = result.ContatoriEccezioni.Where(x => x.CodiceEccezione.StartsWith("MF"));
                if (mf != null)
                {
                    var group = new EccezioniGroupCounter();
                    group.Nome = "Mancati festivi";
                    group.Eccezioni.AddRange(mf.Select(x => new EccezioniCounter() { CodEccezione = x.CodiceEccezione, Response = x }));
                    groups.Add(group);
                }
            }

            return PartialView("subpartial/EccezioniCounter", groups);
        }
    }

    public class FeriePermessiControllerScope : SessionScope<FeriePermessiControllerScope>
    {
        public FeriePermessiControllerScope()
        {
            _anno = DateTime.Now.Year;
            _giorniCalendario = new List<CalendarioDay>();
        }
        private int _anno { get; set; }
        public int anno
        {
            get
            {
                return _anno;
            }
            set
            {
                _anno = value;
            }
        }
        private List<CalendarioDay> _giorniCalendario { get; set; }
        public List<CalendarioDay> giorniCalendario
        {
            get
            {
                return _giorniCalendario;
            }
            set
            {
                _giorniCalendario = value;
            }
        }
        private List<TipoPermessoFerieUsato> _tipiGiornata { get; set; }
        public List<TipoPermessoFerieUsato> tipiGiornata
        {
            get
            {
                return _tipiGiornata;
            }
            set
            {
                _tipiGiornata = value;
            }
        }
    }
    
}
