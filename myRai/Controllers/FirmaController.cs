using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRai.DataAccess;
using System.IO;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using myRaiCommonModel;
using myRaiCommonManager;
using myRaiHelper;
using myRaiCommonModel.DocFirmaModels;
using myRaiData;
using CommonManager = myRaiHelper.CommonHelper;
using Utente = myRaiHelper.UtenteHelper;
using myRaiData.Incentivi;

namespace myRai.Controllers
{
    public class FirmaController : BaseCommonController
    {
        ModelDash pr = new ModelDash();
        daApprovareModel daApprov;
        WSDigigapp datiBack = new WSDigigapp();
        WSDigigapp datiBack_ws1 = new WSDigigapp();

        public ActionResult TestFirma(string pin)
        {
            try
            {
                firmaDigitale.remoteSignature r = new firmaDigitale.remoteSignature();
                firmaDigitale.RemoteSignatureCredentials cred = new firmaDigitale.RemoteSignatureCredentials();
                cred.userid = DaFirmareManager.CheckMatricoleParticolari(CommonManager.GetCurrentUserPMatricola());// "P103650";

                cred.password = "Password01"; //test: Password01
                cred.extAuth = pin; //test: 033193

                firmaDigitale.SignatureFlags d = new firmaDigitale.SignatureFlags();

                String sessionToken = "";
                try
                {
                    sessionToken = r.openSignatureSession(cred);
                }
                catch (Exception ex)
                {
                    return Content(ex.ToString());
                }
                var db = new myRaiData.digiGappEntities();
                var p = db.DIGIRESP_Archivio_PDF.First();
                var response = r.signPDF(cred, p.pdf,
                                           false, "SHA256", null, d, "FirmaDigitale", -1, 16, 760, 250, 20,
                                           CommonManager.GetCurrentUserPMatricola(), "FirmaDigitale", p.sede_gapp, "ddMMyyyy",
                                           "Firmato da " + Utente.Nominativo(), 10);

                r.closeSignatureSession(cred, sessionToken);

                MemoryStream pdfStream = new MemoryStream();
                pdfStream.Write(response, 0, response.Length);
                pdfStream.Position = 0;

                Response.AppendHeader("content-disposition", "inline; filename=" + p.sede_gapp + ".pdf");
                return new FileStreamResult(pdfStream, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        public ActionResult GetDafirmareApp()
        {
            List<Sede> model = DaFirmareManager.GetDaFirmareModel();
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = model,
                MaxJsonLength = int.MaxValue
            };
        }

        public ActionResult Index(int? idscelta)
        {

           

            string NomeAction = this.ControllerContext.RouteData.Values["action"].ToString();
            string NomeController = this.ControllerContext.RouteData.Values["controller"].ToString();

            DaFirmareModel model = new DaFirmareModel();
            model.ListaAlias = DaFirmareManager.GetAlias( );
            model.Sedi = DaFirmareManager.GetDaFirmareModel();

            model.RicercaPdf = DaFirmareManager.GetRicercaPdfModel();

            var db = new myRaiData.digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();

            model.TotaliDaFirmare = DaFirmareManager.GetTotaliDaFirmareModel(model.Sedi);

            model.menuSidebar = Utente.getSidebarModel();// new sidebarModel(3);
            //wcf1.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            string userName = CommonManager.GetCurrentUsername();

            datiBack.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            datiBack_ws1.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            Autorizzazioni.Sedi SEDI = new Autorizzazioni.Sedi();
            SEDI.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);


            pr.menuSidebar = Utente.getSidebarModel();// new sidebarModel(3);
            pr.MieRichieste = new List<MiaRichiesta>();
            pr.digiGAPP = true;
            pr.dettaglioGiornata = HomeManager.GetTimbratureTodayModel();
            pr.Raggruppamenti = CommonManager.GetRaggruppamenti();

            pr.ValidazioneGenericaEccezioni = CommonManager.GetParametro<string>(EnumParametriSistema.ValidazioneGenericaEccezioni);
            pr.SceltePercorso = HomeManager.GetSceltepercorsoModel("PR");
            pr.JsInitialFunction = HomeManager.GetJSfunzioneIniziale(idscelta);


            model.modeldash = new ModelDash();
            model.modeldash.dettaglioGiornata = HomeManager.GetTimbratureTodayModel();
            if (Request.QueryString["fromapp"] == "true")
            {
                model.menuSidebar = null;
                model.modeldash = null;
                model.IdNelCarrello = db.MyRai_Carrello.Where(x => x.matricola == matr).Select(x => x.id_archivio_pdf).ToList();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = model,
                    MaxJsonLength = int.MaxValue
                };
            }
            else
                return View("index2", model);

        }
        public ActionResult GetDaEsaminare(string data1, string data2, string sede, string stato)
        {
            DaFirmareModel model = new DaFirmareModel();
            model.Sedi = DaFirmareManager.GetDaFirmareModel(data1, data2, sede, stato);
            List<string> Lr = new List<string>();
            if (!string.IsNullOrWhiteSpace(data1)) Lr.Add("DAL " + data1);
            if (!string.IsNullOrWhiteSpace(data2)) Lr.Add("AL " + data2);
            if (!string.IsNullOrWhiteSpace(sede)) Lr.Add("SEDE " + sede);
            if (!string.IsNullOrWhiteSpace(stato)) Lr.Add("STATO " + stato);

            ViewBag.ricercati = String.Join(",", Lr.ToArray());

            return View("../Responsabile/da_firmare", model.Sedi);
        }
        public ActionResult GetDaEsaminarePF(string data1, string data2, string sede, string stato)
        {
            DaFirmareModel model = new DaFirmareModel();
            model.Sedi = DaFirmareManager.GetDaFirmareModel(data1, data2, sede, stato);
            List<string> Lr = new List<string>();
            if (!string.IsNullOrWhiteSpace(data1)) Lr.Add("DAL " + data1);
            if (!string.IsNullOrWhiteSpace(data2)) Lr.Add("AL " + data2);
            if (!string.IsNullOrWhiteSpace(sede)) Lr.Add("SEDE " + sede);
            if (!string.IsNullOrWhiteSpace(stato)) Lr.Add("STATO " + stato);

            ViewBag.ricercati = String.Join(",", Lr.ToArray());

            return View("../Responsabile/da_firmarePF", model.Sedi);
        }
        public ActionResult GetCarrello()
        {
            TotaliDaFirmareModel model = DaFirmareManager.GetTotaliDaFirmareModel(null);
            return View("dafirmare", model);
        }

        public ActionResult AggiungiCarrello(int idPdf)
        {
            string matricola = CommonManager.GetCurrentUserMatricola();
            Tuple<int, string> T = DaFirmareManager.AggiungiCarrello(idPdf, matricola);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = T.Item2,
                    items = T.Item1
                }
            };
        }

        public ActionResult QuantiInCarrello()
        {
            var db = new myRaiData.digiGappEntities();
            string  matr = CommonManager.GetCurrentUserMatricola();
            var n = db.MyRai_CarrelloGenerico.Where(x => x.matricola == matr).Count();
            var q = db.MyRai_Carrello.Where(x => x.matricola == matr).Count();
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    car1 = n,
                    car2 = q
                }
            };
        }

        public ActionResult CheckRemovibile(int idPdf)
        {
            string resp = DaFirmareManager.CheckRemovibile(idPdf);
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = resp
                }
            };
        }

        public ActionResult CheckConsecutivo(int idPdf)
        {
            bool CanAdd = DaFirmareManager.CheckConsecutivo(idPdf);
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = CanAdd
                }
            };

        }

        public ActionResult RemoveDaCarrello(int idPdf)
        {
            string matricola = CommonManager.GetCurrentUserMatricola();
            Tuple<int, string> T = DaFirmareManager.CancellaDaCarrello(idPdf, matricola);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = T.Item2,
                    items = T.Item1
                }
            };
        }
        public ActionResult IsInCarrello(int idPdf)
        {
            string matr = CommonManager.GetCurrentUserMatricola();
            var db = new myRaiData.digiGappEntities();
            Boolean GiaCarrello = db.MyRai_Carrello.Any(x => x.matricola == matr && x.id_archivio_pdf == idPdf);
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = GiaCarrello
                }
            };


        }

        public ActionResult IsInCarrelloPF(int idpdf)
        {
            var db = new myRaiData.digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();
            var car = db.MyRai_CarrelloGenerico.Where(x => x.id_documento == idpdf && x.tabella == "MyRai_PianoFerieSedi" && x.matricola == matr).FirstOrDefault();
            if (car != null)
            {
                return new JsonResult  { JsonRequestBehavior = JsonRequestBehavior.AllowGet,  Data = new  {  result = true       }   };
            }
            else
                return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { result = false } };

        }

        [HttpPost]
        public ActionResult DelCarrelloPF(int idpdf)
        {
            try
            {
                var db = new myRaiData.digiGappEntities();
                string matr = CommonManager.GetCurrentUserMatricola();
                var car = db.MyRai_CarrelloGenerico.Where(x => x.id_documento == idpdf && x.tabella == "MyRai_PianoFerieSedi" && x.matricola==matr).FirstOrDefault();
                if (car != null)
                {
                    db.MyRai_CarrelloGenerico.Remove(car);
                    db.SaveChanges();
                }
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        result = true
                    }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        result = false,
                        error = ex.Message
                    }
                };
            }

        }


        [HttpPost]
        public ActionResult AddCarrelloPF(int idpdf)
        {
            var db = new myRaiData.digiGappEntities();
            var pdf = db.MyRai_PianoFerieSedi.Where(x => x.id == idpdf).FirstOrDefault();
            if (pdf == null)
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        result = false,
                        error = "Pdf non trovato"
                    }
                };
            string matr = CommonManager.GetCurrentUserMatricola();
            if (db.MyRai_CarrelloGenerico.Any(x => x.id_documento == idpdf && x.tabella == "MyRai_PianoFerieSedi" && x.matricola==matr))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        result = true
                    }
                };
            }
            try
            {
                myRaiData.MyRai_CarrelloGenerico car = new myRaiData.MyRai_CarrelloGenerico();
                car.data_creazione = DateTime.Now;
                car.id_documento = idpdf;
                car.matricola = CommonManager.GetCurrentUserMatricola();
                car.tabella = "MyRai_PianoFerieSedi";
                db.MyRai_CarrelloGenerico.Add(car);
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        result = true
                    }
                };
            }
            catch (Exception ex)
            {

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        result = false,
                        error = ex.Message
                    }
                };
            }
        }

        public ActionResult GetPdfPianoFerie(int idPdf)
        {
            var db = new myRaiData.digiGappEntities();
            var pdf = db.MyRai_PianoFerieSedi.Where(x => x.id == idPdf).FirstOrDefault();
            
            byte[] b = pdf.pdf;

            string s = Convert.ToBase64String(b);
            string matr = CommonManager.GetCurrentUserMatricola();

            Boolean GiaCarrello = db.MyRai_CarrelloGenerico.Any(x => x.matricola == matr && x.id_documento == idPdf && x.tabella=="MyRai_PianoFerieSedi");

            PDFmodel model = new PDFmodel()
            {
                datainizio = "datainizio",
                datafine = "datafine",
                codSede = "sede",
                idPdf = idPdf,
                PdfCountInCarrello = "(n)",
                PdfCarrelloDisabledAttribute = GiaCarrello == true ? "c" : ""
            };

            return View("../DocumentiDaFirmare/_pdfviewerPF", model);
        }

        public ActionResult GetPdf(int idPdf, string datainizio, string datafine, string codsede)
        {
            var db = new myRaiData.digiGappEntities();
            var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.ID == idPdf).FirstOrDefault();

            byte[] b = pdf.pdf;

            string s = Convert.ToBase64String(b);
            string matr = CommonManager.GetCurrentUserMatricola();
            int c = db.MyRai_Carrello.Where(x => x.matricola == matr).Count();

            Boolean GiaCarrello = db.MyRai_Carrello.Any(x => x.matricola == matr && x.id_archivio_pdf == idPdf);

            PDFmodel model = new PDFmodel()
            {
                datainizio = datainizio,
                datafine = datafine,
                codSede = codsede,
                idPdf = idPdf,
                PdfCountInCarrello = c == 0 ? "" : "(" + c.ToString() + ")",
                PdfCarrelloDisabledAttribute = GiaCarrello == true ? "disabled='disabled'" : ""
            };

            return View("../DocumentiDaFirmare/_pdfviewer", model);
        }

        public ActionResult GetPdfBinary(int idPdf)
        {
            if ( !Utente.IsBossLiv2( CommonManager.GetCurrentUserMatricola( ) ) )
                throw new Exception( "NON AUTORIZZATO" );

            var db = new myRaiData.digiGappEntities();
            var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.ID == idPdf).FirstOrDefault();

            if (pdf == null) return null;

            pdf.MyRai_ArchivioPDF_viewers.Add(new myRaiData.MyRai_ArchivioPDF_viewers()
            {
                data = DateTime.Now,
                ip = HttpContext.Request.UserHostAddress,
                matricola = CommonManager.GetCurrentUserMatricola(),
                useragent = HttpContext.Request.UserAgent
            });
            DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) );

            byte[] byteArray = pdf.pdf;
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;

            Response.AppendHeader("content-disposition", "inline; filename=" + pdf.sede_gapp + ".pdf");
            return new FileStreamResult(pdfStream, "application/pdf");
        }
        public ActionResult GetPdfBinaryPF(int idPdf)
        {
            if ( !Utente.IsBossLiv2( CommonManager.GetCurrentUserMatricola( ) ) )
                throw new Exception( "NON AUTORIZZATO" );

            var db = new myRaiData.digiGappEntities();
            var pdf = db.MyRai_PianoFerieSedi.Where(x => x.id == idPdf).FirstOrDefault();

            if (pdf == null) return null;

            byte[] byteArray = pdf.pdf;
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;

            Response.AppendHeader("content-disposition", "inline; filename=" + pdf.sedegapp + ".pdf");
            return new FileStreamResult(pdfStream, "application/pdf");
        }

        [HttpPost]
        public ActionResult Firma( string pin , string pwd , string pmatr , string nom )
        {
            var db = new digiGappEntities( );
            string Impers = null;
            if ( !String.IsNullOrWhiteSpace( pmatr ) )
            {
                Impers = "Matr imp: " + pmatr + " - Nom:" + nom;
            }

            MyRai_LogAzioni a = new MyRai_LogAzioni( )
            {
                applicativo = "PORTALE" ,
                data = DateTime.Now ,
                matricola = CommonManager.GetCurrentUserMatricola( ) ,
                operazione = "FirmaDoc-Request" ,
                provenienza = "FirmaController.Firma" ,
                descrizione_operazione = Impers
            };

            db.MyRai_LogAzioni.Add( a );
            db.SaveChanges( );




            FirmaDocumentiResponse response = DaFirmareManager.FirmaDocumenti( pin , pwd , pmatr , nom );



            string DocsInErr = "";
            if (response.DocsInErrore != null && response.DocsInErrore .Any() )
            {
            foreach ( DocInErrore d in response.DocsInErrore )
            {
                    if (d == null) continue;
                DocsInErr += d.sedegapp + ": " + d.data_inizio + "/" + d.data_fine + ":" + d.esito + ", \r\n";
            }
            }
           

            MyRai_LogAzioni az = new MyRai_LogAzioni( )
            {
                applicativo = "PORTALE" ,
                data = DateTime.Now ,
                matricola = CommonManager.GetCurrentUserMatricola( ) ,
                operazione = "FirmaDoc-Response" ,
                provenienza = "FirmaController.Firma" ,
                descrizione_operazione =(response!=null?response.esito:"" ) + DocsInErr
            };
            db.MyRai_LogAzioni.Add( az );
            db.SaveChanges( );

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new
                {
                    result = response.esito ,
                    firmati = response.FirmatiOk ,
                    incarrello = response.InCarrello ,
                    isautherror = response.IsAuthError ,
                    erroridocs =(response.esito !="OK" ? response.esito :"" )+ DocsInErr
                }
            };
        }

        #region Dematerializzazione
        private List<XR_DEM_DOCUMENTI_EXT> LoadDocumentiDematerializzati ( )
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );
            DateTime dataLimite = DateTime.Now;
            List<XR_DEM_DOCUMENTI> documenti = new List<XR_DEM_DOCUMENTI>( );
            List<XR_DEM_DOCUMENTI_EXT> result = new List<XR_DEM_DOCUMENTI_EXT>( );
            try
            {
                using ( IncentiviEntities db = new IncentiviEntities( ) )
                {
                    documenti = db.XR_DEM_DOCUMENTI.Where( w =>
                        w.PraticaAttiva &&
                        w.MatricolaDestinatario == matricola &&
                        w.Id_Stato == ( int ) StatiDematerializzazioneDocumenti.InviatoAlDipendente &&
                        ( w.DataInvioNotifica != null && w.DataInvioNotifica <= dataLimite ) ).ToList( );
                }

                if (documenti != null && documenti.Any())
                {
                    foreach(var d in documenti)
                    {
                        CommonManager.GetNominativoPerMatricola( d.MatricolaDestinatario );
                    }
                }
            }
            catch ( Exception ex )
            {
                result = null;
            }

            return result;
        }

        #endregion

    }
}
