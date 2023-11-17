//using myRaiCommonHelper;
using myRaiCommonModel;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using myRaiCommonTasks;
using myRaiData;
using myRaiCommonManager;
using myRai.Business;
using myRai.DataAccess;
using Logger = myRaiHelper.Logger;
using ClosedXML.Excel;
using myRaiData.Incentivi;

namespace myRai.Controllers
{
    public class ProfiloPersonaleController : BaseCommonController
    {
        private static ProfiloPersonaleModel CreaProfiloPersonaleModel()
        {
            var flagdb = CommonHelper.GetParametri<string>(EnumParametriSistema.AbilitatiIban);
            Boolean abilitatiTutti = false;
            String listaAbilitati = flagdb[1];
            Boolean utenteabilitato = false;
            Boolean testResidenza = true;

            if (flagdb[0].Equals("1"))
            {
                abilitatiTutti = true;
            }

            if (!String.IsNullOrWhiteSpace(listaAbilitati) && !String.IsNullOrWhiteSpace(CommonHelper.GetCurrentUserMatricola()))
            {
                if (listaAbilitati.ToUpper().Split(',').Contains(myRai.Business.CommonManager.GetCurrentUserMatricola().ToUpper()))
                {
                    utenteabilitato = true;
                }
            }


            int matricola = Int32.Parse(CommonManager.GetCurrentUserMatricola());

            myRaiCommonModel.ProfiloPersonaleModel model = ProfiloPersonaleManager.GetProfiloPersonaleModel();

            if(testResidenza == true)
            {
                model.Residenza = ProfiloPersonaleManager.GetResidenzaTalentia(CommonManager.GetCurrentUserMatricola());
                model.Domicilio = ProfiloPersonaleManager.GetDomicilioTalentia(CommonManager.GetCurrentUserMatricola());
            }

            if(utenteabilitato || abilitatiTutti) { 

            model.ContiCorrente = ProfiloPersonaleManager.GetListaContoCorrenteTalentia(CommonManager.GetCurrentUserMatricola());

            }

            myRaiCommonModel.ProfiloPersonaleModel model2 = ProfiloPersonaleManager.GetListaIbanDaConv(matricola);

            if (model2.conticorrentidaconv != null)
            {

                model.conticorrentidaconv = model2.conticorrentidaconv;

            }
            else
            {
                model.conticorrentidaconv = null;
            }

            model.Cellulare = ProfiloPersonaleManager.GetCellulare(matricola);
            model.cellularedaconv = ProfiloPersonaleManager.GetListaCellDaConv(CommonManager.GetCurrentUserMatricola());

            if(testResidenza == true)
            {
                model.residenzadaconv = ProfiloPersonaleManager.GetResidenzaDaConv(CommonManager.GetCurrentUserMatricola());
                model.domiciliodaconv = ProfiloPersonaleManager.GetDomicilioDaConv(CommonManager.GetCurrentUserMatricola());
            }



            return model;
        }

        public ActionResult Index()
        {
            ProfiloPersonaleModel model = CreaProfiloPersonaleModel();

            if (TempData.Keys.Contains("RichiestaDaConvalidareFound"))
            {
                model.RichiestaDaConvalidareFound = Convert.ToBoolean(TempData["RichiestaDaConvalidareFound"]);
            }

            return View(model);
        }


        public ActionResult ProfiloUpdate(string ClickedObject, string day, string ore, string min)
        {
            string errore = ProfiloPersonaleManager.ProfiloUpdate(ClickedObject, day, ore, min);
            if (errore == null)
                return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { result = "OK" } };
            else
                return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { result = errore } };
        }

        public ActionResult NuovoIban()
        {
            ProfiloPersonaleModel model = CreaProfiloPersonaleModel();

            model.Contocorrentedb = new CMINFOANAG_EXTCopy();
            model.Contocorrentedb.DES_INTESTATARIO = CommonManager.GetNominativoPerMatricolaCognomeNome(CommonManager.GetCurrentUserMatricola());


            return View(model);
        }

        public ActionResult ConvalidaIbanDialog(string id_evento)
        {
            ProfiloPersonaleModel model = CreaProfiloPersonaleModel();

            model.codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();


            model.codice.ID_EVENTO = Int32.Parse(id_evento);



            return View(model);
        }

        public ActionResult ConvalidaCellDialog(string id_evento)
        {
            ProfiloPersonaleModel model = CreaProfiloPersonaleModel();

            model.codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();


            model.codice.ID_EVENTO = Int32.Parse(id_evento);



            return View(model);
        }

        public ActionResult ConvalidaDomicilioDialog(string id_evento)
        {
            ProfiloPersonaleModel model = CreaProfiloPersonaleModel();

            model.codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();


            model.codice.ID_EVENTO = Int32.Parse(id_evento);



            return View(model);
        }


        public ActionResult NuovoCellulare()
        {
            ProfiloPersonaleModel model = CreaProfiloPersonaleModel();

            model.Recapitidb = new CMINFOANAG_EXTCopy();
            model.Recapitidb.DES_PREFISSO = "0039";



            return View(model);
        }

        public ActionResult NuovaResidenza()
        {
            ProfiloPersonaleModel model = CreaProfiloPersonaleModel();

            model.ResDomdb = new CMINFOANAG_EXTCopy();
           



            return View(model);
        }


        public ActionResult NuovoDomicilio()
        {
            ProfiloPersonaleModel model = CreaProfiloPersonaleModel();

            model.ResDomdb = new CMINFOANAG_EXTCopy();
            
             return View(model);
        }


        public ActionResult AggiungiCellulare()
        {
            ProfiloPersonaleModel model = CreaProfiloPersonaleModel();

            model.Recapitidb = new CMINFOANAG_EXTCopy();
            model.Recapitidb.DES_PREFISSO = "0039";



            return View(model);
        }

        public ActionResult NuovoIbanStipendio()
        {
            ProfiloPersonaleModel model = CreaProfiloPersonaleModel();

            model.Contocorrentedb = new CMINFOANAG_EXTCopy();
            model.Contocorrentedb.DES_INTESTATARIO = CommonManager.GetNominativoPerMatricolaCognomeNome(CommonManager.GetCurrentUserMatricola());


            return View(model);
        }

        public ActionResult NuovoIbanTrasferte()
        {
            ProfiloPersonaleModel model = CreaProfiloPersonaleModel();

            model.Contocorrentedb = new CMINFOANAG_EXTCopy();
            model.Contocorrentedb.DES_INTESTATARIO = CommonManager.GetNominativoPerMatricolaCognomeNome(CommonManager.GetCurrentUserMatricola());


            return View(model);
        }


        public ActionResult NuovoIbanSpeseProd()
        {
            ProfiloPersonaleModel model = CreaProfiloPersonaleModel();

            model.Contocorrentedb = new CMINFOANAG_EXTCopy();
            model.Contocorrentedb.DES_INTESTATARIO = CommonManager.GetNominativoPerMatricolaCognomeNome(CommonManager.GetCurrentUserMatricola());


            return View(model);
        }


        public myRaiData.Incentivi.XR_SSV_CODICE_OTP getRecordCodiceOtp(String funzione, int idevento, int pkey)
        {

            String matricola = CommonManager.GetCurrentUserMatricola();
            myRaiData.Incentivi.XR_SSV_CODICE_OTP record_codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();

            record_codice.ID_CODICE_OTP = pkey;
            record_codice.MATRICOLA = matricola;
            record_codice.IND_UTILIZZO = "0";
            record_codice.COD_FUNZIONE = funzione;
            record_codice.DTA_SCADENZA = DateTime.Today.AddDays(7);
            record_codice.COD_TERMID = "RAIPERME";
            record_codice.COD_USER = "P" + matricola;
            record_codice.DTA_UTILIZZO = DateTime.Now;
            record_codice.TMS_TIMESTAMP = DateTime.Now;
            record_codice.ID_EVENTO = idevento;


            return record_codice;


        }

        [HttpPost]
        public ActionResult ModificaResidenzaDB(ProfiloPersonaleModel model)
        {
            var db = ProfiloPersonaleManager.GetTalentiaDB();
            string matr = CommonManager.GetCurrentUserMatricola();
            //myRaiData.Incentivi.XR_SSV_CODICE_OTP record_codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();
            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));

            model.ResDomdb.COD_CONVALIDA_CC = "0";
            model.ResDomdb.COD_CMEVENTO = "MODRES";

            model.ResDomdb.DES_INDIRRES = model.ResDomdb.DES_INDIRRES;
            model.ResDomdb.CAP_CAPRES = model.ResDomdb.CAP_CAPRES;
            model.ResDomdb.COD_CITTARES = model.ResDomdb.COD_CITTARES;
            model.ResDomdb.DTA_CAMBIO_RES = model.ResDomdb.DTA_CAMBIO_RES;




            if (model.AncheDomicilio.Equals("01"))
            {
                model.ResDomdb.IND_ASSEGNA_DOM = "S";
            }
            else
            {
                model.ResDomdb.IND_ASSEGNA_DOM = "N";
            }



            if (y != null)
            {
                model.ResDomdb.ID_PERSONA = y.ID_PERSONA;
            }

            model.ResDomdb.ID_EVENTO = db.CMINFOANAG_EXT.GeneraOid(x => x.ID_EVENTO, 9);
            //record_codice = getRecordCodiceOtp("02", model.Recapitidb.ID_EVENTO, db.XR_SSV_CODICE_OTP.GeneraPrimaryKey(6));

            //String idevento = model.Recapitidb.ID_EVENTO.ToString();




            string matricola = CommonManager.GetCurrentUserMatricola();

            myRaiData.Incentivi.CMINFOANAG_EXT record = model.ResDomdb.GetDbObject();
            record.TMS_TIMESTAMP = DateTime.Now;
            record.COD_USER = "P" + matricola;
            record.COD_TERMID = "RAIPERME";

            db.CMINFOANAG_EXT.Add(record);
            //db.XR_SSV_CODICE_OTP.Add(record_codice);
            //String idev = record_codice.ID_CODICE_OTP.ToString();





            try
            {
                if (DBHelper.Save(db, "ModificaRes"))
                {
                    GestoreMail mail = new GestoreMail();
                    List<myRaiCommonTasks.sendMail.Attachement> attachments = null;

                    string dest = CommonTasks.GetEmailPerMatricola(matricola);

                    if (String.IsNullOrWhiteSpace(dest))
                        dest = "P" + matricola + "@rai.it";

                    string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271”> La tua richiesta di modifica della residenza è stata inserita</p><p> Entro 24H riceverai il codice utile per la convalida.</br> Potrai <b> modificare/annullare </b> la richiesta effettuata in qualsiasi momento dal portale Rai Per Me nella sezione Profilo Personale.</br><i>Attenzione, si tratta di una mail generata automaticamente, si prega di non rispondere.</i></p> ";

                    var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                         "Richiesta di modifica residenza",
                         dest,
                         "raiplace.selfservice@rai.it",
                         "Richiesta di modifica residenza",
                         "",
                         corpo,
                         null,
                         null,
                         attachments);

                    //string corpo2 = "<p align=”justify”>Il codice per convalidare la tua richiesta di modifica di numero di cellulare da inserire nel portale Rai per Me nella sezione Profilo Personale – Dati Anagrafici, è il seguente:</p><p><h1 style=”text-align:center”>" + record_codice.ID_CODICE_OTP + "</h1></p>";

                    //var response2 = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                    //     "Richiesta di modifica numero di cellulare: istruzioni per la convalida",
                    //     dest,
                    //     "raiplace.selfservice@rai.it",
                    //     "Richiesta di modifica numero di cellulare",
                    //     "Convalida richiesta",
                    //     corpo2,
                    //     null,
                    //     null,
                    //     attachments, DateTime.Now.AddMinutes(5));



                    if (response != null && response.Errore != null)
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori()
                        {
                            applicativo = "Profilo personale",
                            data = DateTime.Now,
                            provenienza = "MODIFICA RESIDENZA",
                            error_message = response.Errore + " per " + dest,
                            matricola = "000000"
                        };

                        using (digiGappEntities db2 = new digiGappEntities())
                        {
                            db2.MyRai_LogErrori.Add(err);
                            db2.SaveChanges();
                        }
                    }

                    //if (response2 != null && response2.Errore != null)
                    //{
                    //    MyRai_LogErrori err = new MyRai_LogErrori()
                    //    {
                    //        applicativo = "Profilo personale",
                    //        data = DateTime.Now,
                    //        provenienza = "MAIL CONVALIDA MODIFICA NUMERO DI CELLULARE",
                    //        error_message = response.Errore + " per " + dest,
                    //        matricola = "000000"
                    //    };



                    //    using (digiGappEntities db2 = new digiGappEntities())
                    //    {
                    //        db2.MyRai_LogErrori.Add(err);
                    //        db2.SaveChanges();
                    //    }
                    //}

                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "ok" }
                    };
                }
                else
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Si è verificato un errore durante il salvataggio" }
                    };
                }

            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "ModificaCELL"
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }


        [HttpPost]
        public ActionResult ModificaDomicilioDB(ProfiloPersonaleModel model)
        {
            var db = ProfiloPersonaleManager.GetTalentiaDB();
            string matr = CommonManager.GetCurrentUserMatricola();
            myRaiData.Incentivi.XR_SSV_CODICE_OTP record_codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();
            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));

            model.ResDomdb.COD_CONVALIDA_CC = "0";
            model.ResDomdb.COD_CMEVENTO = "MODDOM";
            model.ResDomdb.DES_INDIRDOM = model.ResDomdb.DES_INDIRDOM;
            model.ResDomdb.CAP_CAPDOM = model.ResDomdb.CAP_CAPDOM;
            model.ResDomdb.COD_CITTADOM = model.ResDomdb.COD_CITTADOM;






            if (y != null)
            {
                model.ResDomdb.ID_PERSONA = y.ID_PERSONA;
            }

            model.ResDomdb.ID_EVENTO = db.CMINFOANAG_EXT.GeneraOid(x => x.ID_EVENTO, 9);
            record_codice = getRecordCodiceOtp("03", model.ResDomdb.ID_EVENTO, db.XR_SSV_CODICE_OTP.GeneraPrimaryKey(6));

            String idevento = model.ResDomdb.ID_EVENTO.ToString();




            string matricola = CommonManager.GetCurrentUserMatricola();

            myRaiData.Incentivi.CMINFOANAG_EXT record = model.ResDomdb.GetDbObject();
            record.TMS_TIMESTAMP = DateTime.Now;
            record.COD_USER = "P" + matricola;
            record.COD_TERMID = "RAIPERME";

            db.CMINFOANAG_EXT.Add(record);
            db.XR_SSV_CODICE_OTP.Add(record_codice);
            String idev = record_codice.ID_CODICE_OTP.ToString();





            try
            {
                if (DBHelper.Save(db, "ModificaDOM"))
                {
                    GestoreMail mail = new GestoreMail();
                    List<myRaiCommonTasks.sendMail.Attachement> attachments = null;

                    string dest = CommonTasks.GetEmailPerMatricola(matricola);

                    if (String.IsNullOrWhiteSpace(dest))
                        dest = "P" + matricola + "@rai.it";

                    string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271”> La tua richiesta di modifica del domicilio è stata inserita.</p><p> Tra pochi minuti riceverai il codice utile per la convalida.</br> Potrai <b> modificare/annullare </b> la richiesta effettuata in qualsiasi momento dal portale Rai Per Me nella sezione Profilo Personale.</br><i>Attenzione, si tratta di una mail generata automaticamente, si prega di non rispondere.</i></p> ";

                    var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                         "Richiesta di modifica domicilio",
                         dest,
                         "raiplace.selfservice@rai.it",
                         "Richiesta di modifica domicilio",
                         "",
                         corpo,
                         null,
                         null,
                         attachments);

                    string corpo2 = "<p align=”justify”>Il codice per convalidare la tua richiesta di modifica di domicilio da inserire nel portale Rai per Me nella sezione Profilo Personale – Dati Anagrafici, è il seguente:</p><p><h1 style=”text-align:center”>" + record_codice.ID_CODICE_OTP + "</h1></p>";

                    var response2 = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                         "Richiesta di modifica domicilio: istruzioni per la convalida",
                         dest,
                         "raiplace.selfservice@rai.it",
                         "Richiesta di modifica domicilio",
                         "Convalida richiesta",
                         corpo2,
                         null,
                         null,
                         attachments, DateTime.Now.AddMinutes(2));



                    if (response != null && response.Errore != null)
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori()
                        {
                            applicativo = "Profilo personale",
                            data = DateTime.Now,
                            provenienza = "MODIFICA DOMICILIO",
                            error_message = response.Errore + " per " + dest,
                            matricola = "000000"
                        };

                        using (digiGappEntities db2 = new digiGappEntities())
                        {
                            db2.MyRai_LogErrori.Add(err);
                            db2.SaveChanges();
                        }
                    }

                    if (response2 != null && response2.Errore != null)
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori()
                        {
                            applicativo = "Profilo personale",
                            data = DateTime.Now,
                            provenienza = "MAIL CONVALIDA MODIFICA DOMICILIO",
                            error_message = response.Errore + " per " + dest,
                            matricola = "000000"
                        };



                        using (digiGappEntities db2 = new digiGappEntities())
                        {
                            db2.MyRai_LogErrori.Add(err);
                            db2.SaveChanges();
                        }
                    }

                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "ok" }
                    };
                }
                else
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Si è verificato un errore durante il salvataggio" }
                    };
                }

            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "ModificaDomicilio"
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }

        [HttpPost]
        public ActionResult AggiungiCellulareDB(ProfiloPersonaleModel model)
        {
            var db = ProfiloPersonaleManager.GetTalentiaDB();
            string matr = CommonManager.GetCurrentUserMatricola();
            myRaiData.Incentivi.XR_SSV_CODICE_OTP record_codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();
            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));

            model.Recapitidb.COD_CONVALIDA_CC = "0";
            model.Recapitidb.COD_CMEVENTO = "INSCEL";

            model.Recapitidb.COD_IBAN = "0";
            model.Recapitidb.DES_INTESTATARIO = "-";
            model.Recapitidb.COD_UTILIZZO = "0";



            if (model.CellAziendale.Equals("02"))
            {
                model.Recapitidb.TIPO_RECAPITO = "02";
            }
            else
            {
                model.Recapitidb.TIPO_RECAPITO = "01";
            }



            if (y != null)
            {
                model.Recapitidb.ID_PERSONA = y.ID_PERSONA;
            }

            model.Recapitidb.ID_EVENTO = db.CMINFOANAG_EXT.GeneraOid(x => x.ID_EVENTO, 9);
            record_codice = getRecordCodiceOtp("02", model.Recapitidb.ID_EVENTO, db.XR_SSV_CODICE_OTP.GeneraPrimaryKey(6));

            String idevento = model.Recapitidb.ID_EVENTO.ToString();




            string matricola = CommonManager.GetCurrentUserMatricola();

            myRaiData.Incentivi.CMINFOANAG_EXT record = model.Recapitidb.GetDbObject();
            record.TMS_TIMESTAMP = DateTime.Now;
            record.COD_USER = "P" + matricola;
            record.COD_TERMID = "RAIPERME";

            db.CMINFOANAG_EXT.Add(record);
            db.XR_SSV_CODICE_OTP.Add(record_codice);
            String idev = record_codice.ID_CODICE_OTP.ToString();





            try
            {
                if (DBHelper.Save(db, "AggiungiCELL"))
                {
                    GestoreMail mail = new GestoreMail();
                    List<myRaiCommonTasks.sendMail.Attachement> attachments = null;

                    string dest = CommonTasks.GetEmailPerMatricola(matricola);

                    if (String.IsNullOrWhiteSpace(dest))
                        dest = "P" + matricola + "@rai.it";

                    string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271”> La tua richiesta di aggiunta del numero di cellulare è stata inserita</p><p> Entro 24H riceverai il codice utile per la convalida.</br> Potrai <b> modificare/annullare </b> la richiesta effettuata in qualsiasi momento dal portale Rai Per Me nella sezione Profilo Personale.</br><i>Attenzione, si tratta di una mail generata automaticamente, si prega di non rispondere.</i></p> ";

                    var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                         "Richiesta di aggiunta numero di cellulare",
                         dest,
                         "raiplace.selfservice@rai.it",
                         "Richiesta di aggiunta numero di cellulare",
                         "",
                         corpo,
                         null,
                         null,
                         attachments);

                    string corpo2 = "<p align=”justify”>Il codice per convalidare la tua richiesta di aggiunta di numero di cellulare da inserire nel portale Rai per Me nella sezione Profilo Personale – Dati Anagrafici, è il seguente:</p><p><h1 style=”text-align:center”>" + record_codice.ID_CODICE_OTP + "</h1></p>";

                    var response2 = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                         "Richiesta di aggiunta numero di cellulare: istruzioni per la convalida",
                         dest,
                         "raiplace.selfservice@rai.it",
                         "Richiesta di aggiunta numero di cellulare",
                         "Convalida richiesta",
                         corpo2,
                         null,
                         null,
                         attachments, DateTime.Now.AddMinutes(5));



                    if (response != null && response.Errore != null)
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori()
                        {
                            applicativo = "Profilo personale",
                            data = DateTime.Now,
                            provenienza = "AGGIUNTA NUMERO DI CELLULARE",
                            error_message = response.Errore + " per " + dest,
                            matricola = "000000"
                        };

                        using (digiGappEntities db2 = new digiGappEntities())
                        {
                            db2.MyRai_LogErrori.Add(err);
                            db2.SaveChanges();
                        }
                    }

                    if (response2 != null && response2.Errore != null)
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori()
                        {
                            applicativo = "Profilo personale",
                            data = DateTime.Now,
                            provenienza = "MAIL CONVALIDA AGGIUNTA NUMERO DI CELLULARE",
                            error_message = response.Errore + " per " + dest,
                            matricola = "000000"
                        };



                        using (digiGappEntities db2 = new digiGappEntities())
                        {
                            db2.MyRai_LogErrori.Add(err);
                            db2.SaveChanges();
                        }
                    }
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "ok" }
                    };
                }
                else
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Si è verificato un errore durante il salvataggio" }
                    };
                }

            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "AggiungiCELL"
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }



        [HttpPost]
        public ActionResult ModificaCellulareDB(ProfiloPersonaleModel model)
        {
            var db = ProfiloPersonaleManager.GetTalentiaDB();
            string matr = CommonManager.GetCurrentUserMatricola();
            myRaiData.Incentivi.XR_SSV_CODICE_OTP record_codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();
            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));

            model.Recapitidb.COD_CONVALIDA_CC = "0";
            model.Recapitidb.COD_CMEVENTO = "MODCEL";

            model.Recapitidb.COD_IBAN = "0";
            model.Recapitidb.DES_INTESTATARIO = "-";
            model.Recapitidb.COD_UTILIZZO = "0";
             
        

            if (model.CellAziendale.Equals("02"))
            {
                model.Recapitidb.TIPO_RECAPITO = "02";
            }
            else
            {
                model.Recapitidb.TIPO_RECAPITO = "01";
            }
            
            

            if(y!= null)
            {
                model.Recapitidb.ID_PERSONA = y.ID_PERSONA;
            }
            
            model.Recapitidb.ID_EVENTO = db.CMINFOANAG_EXT.GeneraOid(x => x.ID_EVENTO, 9);
            record_codice = getRecordCodiceOtp("02", model.Recapitidb.ID_EVENTO, db.XR_SSV_CODICE_OTP.GeneraPrimaryKey(6));

            String idevento = model.Recapitidb.ID_EVENTO.ToString();




            string matricola = CommonManager.GetCurrentUserMatricola();

            myRaiData.Incentivi.CMINFOANAG_EXT record = model.Recapitidb.GetDbObject();
            record.TMS_TIMESTAMP = DateTime.Now;
            record.COD_USER = "P" + matricola;
            record.COD_TERMID = "RAIPERME";

            db.CMINFOANAG_EXT.Add(record);
            db.XR_SSV_CODICE_OTP.Add(record_codice);
            String idev = record_codice.ID_CODICE_OTP.ToString();





            try
            {
                if (DBHelper.Save(db, "ModificaCELL"))
                {
                    GestoreMail mail = new GestoreMail();
                    List<myRaiCommonTasks.sendMail.Attachement> attachments = null;

                    string dest = CommonTasks.GetEmailPerMatricola(matricola);

                    if (String.IsNullOrWhiteSpace(dest))
                        dest = "P" + matricola + "@rai.it";

                    string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271”> La tua richiesta di modifica del numero di cellulare è stata inserita</p><p> Entro 24H riceverai il codice utile per la convalida.</br> Potrai <b> modificare/annullare </b> la richiesta effettuata in qualsiasi momento dal portale Rai Per Me nella sezione Profilo Personale.</br><i>Attenzione, si tratta di una mail generata automaticamente, si prega di non rispondere.</i></p> ";

                    var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                         "Richiesta di modifica numero di cellulare",
                         dest,
                         "raiplace.selfservice@rai.it",
                         "Richiesta di modifica numero di cellulare",
                         "",
                         corpo,
                         null,
                         null,
                         attachments);

                    string corpo2 = "<p align=”justify”>Il codice per convalidare la tua richiesta di modifica di numero di cellulare da inserire nel portale Rai per Me nella sezione Profilo Personale – Dati Anagrafici, è il seguente:</p><p><h1 style=”text-align:center”>" + record_codice.ID_CODICE_OTP + "</h1></p>";

                    var response2 = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                         "Richiesta di modifica numero di cellulare: istruzioni per la convalida",
                         dest,
                         "raiplace.selfservice@rai.it",
                         "Richiesta di modifica numero di cellulare",
                         "Convalida richiesta",
                         corpo2,
                         null,
                         null,
                         attachments, DateTime.Now.AddMinutes(5));



                    if (response != null && response.Errore != null)
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori()
                        {
                            applicativo = "Profilo personale",
                            data = DateTime.Now,
                            provenienza = "MODIFICA NUMERO DI CELLULARE",
                            error_message = response.Errore + " per " + dest,
                            matricola = "000000"
                        };

                        using (digiGappEntities db2 = new digiGappEntities())
                        {
                            db2.MyRai_LogErrori.Add(err);
                            db2.SaveChanges();
                        }
                    }

                    if (response2 != null && response2.Errore != null)
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori()
                        {
                            applicativo = "Profilo personale",
                            data = DateTime.Now,
                            provenienza = "MAIL CONVALIDA MODIFICA NUMERO DI CELLULARE",
                            error_message = response.Errore + " per " + dest,
                            matricola = "000000"
                        };



                        using (digiGappEntities db2 = new digiGappEntities())
                        {
                            db2.MyRai_LogErrori.Add(err);
                            db2.SaveChanges();
                        }
                    }

                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "ok" }
                    };
                }
                else
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Si è verificato un errore durante il salvataggio" }
                    };
                }

            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "ModificaCELL"
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }


        [HttpPost]
        public ActionResult AggiungiIbanDB(ProfiloPersonaleModel model)
        {

            var db = ProfiloPersonaleManager.GetTalentiaDB();


            myRaiData.Incentivi.XR_SSV_CODICE_OTP record_codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();



            model.Contocorrentedb.COD_IBAN = model.Contocorrentedb.COD_IBAN.ToUpper();

            model.Contocorrentedb.COD_CONVALIDA_CC = "0";
            model.Contocorrentedb.COD_CMEVENTO = "INSCC";


            model.Contocorrentedb.ID_PERSONA = Int32.Parse(CommonManager.GetCurrentUserMatricola());

            model.Contocorrentedb.ID_EVENTO = db.CMINFOANAG_EXT.GeneraOid(x => x.ID_EVENTO, 9);

            record_codice = getRecordCodiceOtp("01", model.Contocorrentedb.ID_EVENTO, db.XR_SSV_CODICE_OTP.GeneraPrimaryKey(6));




            String idevento = model.Contocorrentedb.ID_EVENTO.ToString();
            String codPaese = "";
            String codChk = "";
            String codCin = "";
            String codAbi = "";
            String codCab = "";
            String codCC = "";
            String codIban = model.Contocorrentedb.COD_IBAN.ToString();

            if (codIban.Length == 27)
            {
                codPaese = codIban.Substring(0, 2);
                codChk = codIban.Substring(2, 2);
                codCin = codIban.Substring(4, 1);
                codAbi = codIban.Substring(5, 5);
                codCab = codIban.Substring(10, 5);
                codCC = codIban.Substring(15);
            }

            model.Contocorrentedb.COD_ABI = codAbi;
            model.Contocorrentedb.COD_CAB = codCab;
            myRaiData.Incentivi.XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.FirstOrDefault(x => x.COD_ABI == codAbi && x.COD_CAB == codCab);

            if (anagBanca == null) { model.Contocorrentedb.IND_BANCA_ASSENTE = "1"; }
            else
            { model.Contocorrentedb.IND_BANCA_ASSENTE = "2"; }

            if (model.AccStip.Equals("01"))
            {
                model.Contocorrentedb.COD_UTILIZZO = "01";
            }
            else
            if (model.AntTrasf.Equals("02") && model.AntSpeseProd.Equals("03")
           )
            {
                model.Contocorrentedb.COD_UTILIZZO = "04";
            }
            else
            if (model.AntTrasf.Equals("02"))
            {
                model.Contocorrentedb.COD_UTILIZZO = "02";
            }
            else
                if (model.AntSpeseProd.Equals("03"))
            {

                model.Contocorrentedb.COD_UTILIZZO = "03";
            }


            string matricola = CommonManager.GetCurrentUserMatricola();
            myRaiData.Incentivi.CMINFOANAG_EXT record = model.Contocorrentedb.GetDbObject();
            record.TMS_TIMESTAMP = DateTime.Now;
            record.COD_USER = "P" + matricola;
            record.COD_TERMID = "RAIPERME";

            db.CMINFOANAG_EXT.Add(record);
            db.XR_SSV_CODICE_OTP.Add(record_codice);
            String idev = record_codice.ID_CODICE_OTP.ToString();







            try
            {
                db.SaveChanges();

                GestoreMail mail = new GestoreMail();
                List<myRaiCommonTasks.sendMail.Attachement> attachments = null;

                string dest = CommonTasks.GetEmailPerMatricola(matricola);


                if (String.IsNullOrWhiteSpace(dest))
                    dest = "P" + matricola + "@rai.it";


                string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271”> La tua richiesta di aggiunta di dati bancari è stata inserita</p><p> Entro 24H riceverai il codice utile per la convalida.</br> Potrai <b> modificare/annullare </b> la richiesta effettuata in qualsiasi momento dal portale Rai Per Me nella sezione Profilo Personale.</br>Si raccomanda di mantenere <b>temporaneamente</b> attivo il vecchio codice iban, in quanto non si assicura che la procedura, seppur registrata con successo, abbia effetto già per il mese corrente.</p><p style =”text-align:center”><i>Attenzione, si tratta di una mail generata automaticamente, si prega di non rispondere.</i></p> ";

                var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                     "Richiesta di aggiunta nuova tipologia di dati bancari",
                     dest,
                     "raiplace.selfservice@rai.it",
                     "Richiesta di aggiunta IBAN",
                     "",
                     corpo,
                     null,
                     null,
                     attachments);

                string corpo2 = "<p align=”justify”>Il codice per convalidare la tua richiesta di aggiunta di nuovi dati bancari da inserire nel portale Rai per Me nella sezione Profilo Personale – Dati Bancari, è il seguente:</p><p><h1 style=”text-align:center”>" + record_codice.ID_CODICE_OTP + "</h1></p>";

                var response2 = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                     "Richiesta di aggiunta nuova tipologia di dati bancari: istruzioni per la convalida",
                     dest,
                     "raiplace.selfservice@rai.it",
                     "Richiesta di aggiunta IBAN",
                     "Convalida richiesta",
                     corpo2,
                     null,
                     null,
                     attachments, DateTime.Now.AddDays(1));

                if (response != null && response.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "Aggiunta nuovo IBAN",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };



                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }

                if (response2 != null && response2.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "Mail Convalida aggiunta IBAN",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };



                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "ok" }
                };
            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "AggiungiNuovoIBAN"
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }


        [HttpPost]
        public ActionResult ModificaStipendioIbanDB(ProfiloPersonaleModel model)
        {
            var db = ProfiloPersonaleManager.GetTalentiaDB();
            //string matr = CommonManager.GetCurrentUserMatricola();
            myRaiData.Incentivi.XR_SSV_CODICE_OTP record_codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();
            //myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));

            model.Contocorrentedb.COD_IBAN = model.Contocorrentedb.COD_IBAN.ToUpper();
            model.Contocorrentedb.COD_CONVALIDA_CC = "0";
            model.Contocorrentedb.COD_CMEVENTO = "MODCC";
            model.Contocorrentedb.COD_UTILIZZO = "01";
            model.Contocorrentedb.ID_PERSONA = Int32.Parse(CommonManager.GetCurrentUserMatricola());
            //if (y != null)
            //    {
            //    model.Contocorrentedb.ID_PERSONA = y.ID_PERSONA;
            //}

            model.Contocorrentedb.ID_EVENTO = db.CMINFOANAG_EXT.GeneraOid(x => x.ID_EVENTO, 9);
            record_codice = getRecordCodiceOtp("01", model.Contocorrentedb.ID_EVENTO, db.XR_SSV_CODICE_OTP.GeneraPrimaryKey(6));

            String idevento = model.Contocorrentedb.ID_EVENTO.ToString();
            String codPaese = "";
            String codChk = "";
            String codCin = "";
            String codAbi = "";
            String codCab = "";
            String codCC = "";
            String codIban = model.Contocorrentedb.COD_IBAN.ToString();

            if (codIban.Length == 27)
            {
                codPaese = codIban.Substring(0, 2);
                codChk = codIban.Substring(2, 2);
                codCin = codIban.Substring(4, 1);
                codAbi = codIban.Substring(5, 5);
                codCab = codIban.Substring(10, 5);
                codCC = codIban.Substring(15);
            }

            model.Contocorrentedb.COD_ABI = codAbi;
            model.Contocorrentedb.COD_CAB = codCab;
            myRaiData.Incentivi.XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.FirstOrDefault(x => x.COD_ABI == codAbi && x.COD_CAB == codCab);
            if (anagBanca == null) { model.Contocorrentedb.IND_BANCA_ASSENTE = "1"; }
            else
            { model.Contocorrentedb.IND_BANCA_ASSENTE = "2"; }


            string matricola = CommonManager.GetCurrentUserMatricola();
            myRaiData.Incentivi.CMINFOANAG_EXT record = model.Contocorrentedb.GetDbObject();
            record.TMS_TIMESTAMP = DateTime.Now;
            record.COD_USER = "P" + matricola;
            record.COD_TERMID = "RAIPERME";

            db.CMINFOANAG_EXT.Add(record);
            db.XR_SSV_CODICE_OTP.Add(record_codice);
            String idev = record_codice.ID_CODICE_OTP.ToString();





            try
            {
                db.SaveChanges();
                GestoreMail mail = new GestoreMail();
                List<myRaiCommonTasks.sendMail.Attachement> attachments = null;

                string dest = CommonTasks.GetEmailPerMatricola(matricola);

                if (String.IsNullOrWhiteSpace(dest))
                    dest = "P" + matricola + "@rai.it";

                string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271”> La tua richiesta di modifica di dati bancari è stata inserita</p><p> Entro 24H riceverai il codice utile per la convalida.</br> Potrai <b> modificare/annullare </b> la richiesta effettuata in qualsiasi momento dal portale Rai Per Me nella sezione Profilo Personale.</br>Si raccomanda di mantenere <b>temporaneamente</b> attivo il vecchio codice iban, in quanto non si assicura che la procedura, seppur registrata con successo, abbia effetto già per il mese corrente.</p><p style =”text-align:center”><i>Attenzione, si tratta di una mail generata automaticamente, si prega di non rispondere.</i></p> ";

                var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                     "Richiesta di modifica dati bancari accredito stipendio",
                     dest,
                     "raiplace.selfservice@rai.it",
                     "Richiesta di modifica IBAN",
                     "",
                     corpo,
                     null,
                     null,
                     attachments);

                string corpo2 = "<p align=”justify”>Il codice per convalidare la tua richiesta di modifica dei dati bancari da inserire nel portale Rai per Me nella sezione Profilo Personale – Dati Bancari, è il seguente:</p><p><h1 style=”text-align:center”>" + record_codice.ID_CODICE_OTP + "</h1></p>";

                var response2 = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                     "Richiesta di modifica di dati bancari: istruzioni per la convalida",
                     dest,
                     "raiplace.selfservice@rai.it",
                     "Richiesta di modifica IBAN",
                     "Convalida richiesta",
                     corpo2,
                     null,
                     null,
                     attachments, DateTime.Now.AddDays(1));



                if (response != null && response.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "MODIFICA IBAN STIPENDIO",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };

                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }

                if (response2 != null && response2.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "MAIL CONVALIDA MODIFICA IBAN STIPENDIO",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };



                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "ok" }
                };
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "ModificaIBAN"
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }


        [HttpPost]
        public ActionResult ModificaTrasferteIbanDB(ProfiloPersonaleModel model)
        {
            var db = ProfiloPersonaleManager.GetTalentiaDB();

            myRaiData.Incentivi.XR_SSV_CODICE_OTP record_codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();



            model.Contocorrentedb.COD_IBAN = model.Contocorrentedb.COD_IBAN.ToUpper();
            model.Contocorrentedb.COD_CONVALIDA_CC = "0";
            model.Contocorrentedb.COD_CMEVENTO = "MODCC";
            model.Contocorrentedb.COD_UTILIZZO = "02";
            model.Contocorrentedb.ID_PERSONA = Int32.Parse(CommonManager.GetCurrentUserMatricola());
            model.Contocorrentedb.ID_EVENTO = db.CMINFOANAG_EXT.GeneraOid(x => x.ID_EVENTO, 9);
            record_codice = getRecordCodiceOtp("01", model.Contocorrentedb.ID_EVENTO, db.XR_SSV_CODICE_OTP.GeneraPrimaryKey(6));

            String idevento = model.Contocorrentedb.ID_EVENTO.ToString();
            String codPaese = "";
            String codChk = "";
            String codCin = "";
            String codAbi = "";
            String codCab = "";
            String codCC = "";
            String codIban = model.Contocorrentedb.COD_IBAN.ToString();

            if (codIban.Length == 27)
            {
                codPaese = codIban.Substring(0, 2);
                codChk = codIban.Substring(2, 2);
                codCin = codIban.Substring(4, 1);
                codAbi = codIban.Substring(5, 5);
                codCab = codIban.Substring(10, 5);
                codCC = codIban.Substring(15);
            }

            model.Contocorrentedb.COD_ABI = codAbi;
            model.Contocorrentedb.COD_CAB = codCab;
            myRaiData.Incentivi.XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.FirstOrDefault(x => x.COD_ABI == codAbi && x.COD_CAB == codCab);
            if (anagBanca == null) { model.Contocorrentedb.IND_BANCA_ASSENTE = "1"; }
            else
            { model.Contocorrentedb.IND_BANCA_ASSENTE = "2"; }


            string matricola = CommonManager.GetCurrentUserMatricola();
            myRaiData.Incentivi.CMINFOANAG_EXT record = model.Contocorrentedb.GetDbObject();
            record.TMS_TIMESTAMP = DateTime.Now;
            record.COD_USER = "P" + matricola;
            record.COD_TERMID = "RAIPERME";

            db.CMINFOANAG_EXT.Add(record);
            db.XR_SSV_CODICE_OTP.Add(record_codice);
            String idev = record_codice.ID_CODICE_OTP.ToString();


            try
            {
                db.SaveChanges();
                GestoreMail mail = new GestoreMail();
                List<myRaiCommonTasks.sendMail.Attachement> attachments = null;
                string dest = CommonTasks.GetEmailPerMatricola(matricola);



                string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271”> La tua richiesta di modifica dei dati bancari è stata inserita</p><p> Entro 24H riceverai il codice utile per la convalida.</br> Potrai <b> modificare/annullare </b> la richiesta effettuata in qualsiasi momento dal portale Rai Per Me nella sezione Profilo Personale.</br>Si raccomanda di mantenere <b>temporaneamente</b> attivo il vecchio codice iban, in quanto non si assicura che la procedura, seppur registrata con successo, abbia effetto già per il mese corrente.</p><p style =”text-align:center”><i>Attenzione, si tratta di una mail generata automaticamente, si prega di non rispondere.</i></p>";

                var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                     "Richiesta di modifica dei dati bancari anticipo trasferte",
                     dest,
                     "raiplace.selfservice@rai.it",
                     "Richiesta di modifica IBAN",
                     "",
                     corpo,
                     null,
                     null,
                     attachments);

                string corpo2 = "<p align=”justify”>Il codice per convalidare la tua richiesta di modifica dei dati bancari da inserire nel portale Rai per Me nella sezione Profilo Personale – Dati Bancari, è il seguente:</p><p><h1 style=”text-align:center” > " + record_codice.ID_CODICE_OTP + "</h1></p>";

                var response2 = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                     "Richiesta di modifica di dati bancari: istruzioni per la convalida",
                     dest,
                     "raiplace.selfservice@rai.it",
                     "Richiesta di modifica IBAN",
                     "Convalida richiesta",
                     corpo2,
                     null,
                     null,
                     attachments, DateTime.Now.AddDays(1));

                if (response != null && response.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "Modifica IBAN Trasferte",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };

                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }

                if (response2 != null && response2.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "MAIL CONVALIDA MODIFICA IBAN",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };



                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "ok" }
                };
            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "ModificaIBAN"
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }


        [HttpPost]
        public ActionResult ModificaSpeseProdIbanDB(ProfiloPersonaleModel model)
        {
            var db = ProfiloPersonaleManager.GetTalentiaDB();

            myRaiData.Incentivi.XR_SSV_CODICE_OTP record_codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();



            model.Contocorrentedb.COD_IBAN = model.Contocorrentedb.COD_IBAN.ToUpper();
            model.Contocorrentedb.COD_CONVALIDA_CC = "0";
            model.Contocorrentedb.COD_CMEVENTO = "MODCC";
            model.Contocorrentedb.COD_UTILIZZO = "03";
            model.Contocorrentedb.ID_PERSONA = Int32.Parse(CommonManager.GetCurrentUserMatricola());
            model.Contocorrentedb.ID_EVENTO = db.CMINFOANAG_EXT.GeneraOid(x => x.ID_EVENTO, 9);
            record_codice = getRecordCodiceOtp("01", model.Contocorrentedb.ID_EVENTO, db.XR_SSV_CODICE_OTP.GeneraPrimaryKey(6));

            String idevento = model.Contocorrentedb.ID_EVENTO.ToString();
            String codPaese = "";
            String codChk = "";
            String codCin = "";
            String codAbi = "";
            String codCab = "";
            String codCC = "";
            String codIban = model.Contocorrentedb.COD_IBAN.ToString();

            if (codIban.Length == 27)
            {
                codPaese = codIban.Substring(0, 2);
                codChk = codIban.Substring(2, 2);
                codCin = codIban.Substring(4, 1);
                codAbi = codIban.Substring(5, 5);
                codCab = codIban.Substring(10, 5);
                codCC = codIban.Substring(15);
            }

            model.Contocorrentedb.COD_ABI = codAbi;
            model.Contocorrentedb.COD_CAB = codCab;
            myRaiData.Incentivi.XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.FirstOrDefault(x => x.COD_ABI == codAbi && x.COD_CAB == codCab);
            if (anagBanca == null) { model.Contocorrentedb.IND_BANCA_ASSENTE = "1"; }
            else
            { model.Contocorrentedb.IND_BANCA_ASSENTE = "2"; }



            string matricola = CommonManager.GetCurrentUserMatricola();
            myRaiData.Incentivi.CMINFOANAG_EXT record = model.Contocorrentedb.GetDbObject();
            record.TMS_TIMESTAMP = DateTime.Now;
            record.COD_USER = "P" + matricola;
            record.COD_TERMID = "RAIPERME";
            db.CMINFOANAG_EXT.Add(record);
            db.XR_SSV_CODICE_OTP.Add(record_codice);
            String idev = record_codice.ID_CODICE_OTP.ToString();







            try
            {
                db.SaveChanges();
                GestoreMail mail = new GestoreMail();
                List<myRaiCommonTasks.sendMail.Attachement> attachments = null;


                string dest = CommonTasks.GetEmailPerMatricola(matricola);
                if (String.IsNullOrWhiteSpace(dest))
                    dest = "P" + matricola + "@rai.it";



                string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271”> La tua richiesta di modifica dei dati bancari è stata inserita</p><p> Entro 24H riceverai il codice utile per la convalida.</br> Potrai <b> modificare/annullare </b> la richiesta effettuata in qualsiasi momento dal portale Rai Per Me nella sezione Profilo Personale.</br>Si raccomanda di mantenere <b>temporaneamente</b> attivo il vecchio codice iban, in quanto non si assicura che la procedura, seppur registrata con successo, abbia effetto già per il mese corrente.</p><p style =”text-align:center”><i>Attenzione, si tratta di una mail generata automaticamente, si prega di non rispondere.</i></p>";

                var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                     "Richiesta di modifica dei dati bancari anticipo spese di produzione",
                     dest,
                     "raiplace.selfservice@rai.it",
                     "Richiesta di modifica IBAN",
                     "",
                     corpo,
                     null,
                     null,
                     attachments);

                string corpo2 = "<p align=”justify”>Il codice per convalidare la tua richiesta di modifica dei dati bancari da inserire nel portale Rai per Me nella sezione Profilo Personale – Dati Bancari, è il seguente:</p><p><h1 style=”text-align:center”>" + record_codice.ID_CODICE_OTP + "</h1></p>";

                var response2 = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                   "Richiesta di modifica di dati bancari: istruzioni per la convalida",
                   dest,
                   "raiplace.selfservice@rai.it",
                   "Richiesta di modifica IBAN",
                   "Convalida richiesta",
                   corpo2,
                   null,
                   null,
                   attachments, DateTime.Now.AddDays(1));


                if (response != null && response.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo Personale",
                        data = DateTime.Now,
                        provenienza = "MODIFICA IBAN SPESE PRODUZIONE",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };

                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }

                if (response2 != null && response2.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "CONVALIDA MODIFICHE IBAN SPESE PRODUZIONE",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };



                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "ok" }
                };
            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "AggiungiNuovoIBAN"
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }




        public ActionResult DeleteModIban(string idev)


        {

            int idev2 = Int32.Parse(idev);
            var db = ProfiloPersonaleManager.GetTalentiaDB();


            var daeliminare = db.CMINFOANAG_EXT.FirstOrDefault(c => c.ID_EVENTO == idev2);

            if (daeliminare != null) { 

                db.CMINFOANAG_EXT.Remove(daeliminare);
            }




            try
            {
                db.SaveChanges();



                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "ok" }
                };
            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "EliminaModificaIbanInSospeso"
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }

        public ActionResult DeleteCellulare(String prefix, String num, String tipologia)
        {
            string matr = CommonManager.GetCurrentUserMatricola();
            var db = ProfiloPersonaleManager.GetTalentiaDB();
            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));

            myRaiData.Incentivi.XR_SSV_CODICE_OTP record_codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();



            myRaiData.Incentivi.CMINFOANAG_EXT celldaeliminare = new myRaiData.Incentivi.CMINFOANAG_EXT();


            celldaeliminare.DES_PREFISSO = prefix;
            celldaeliminare.DES_CELLULARE = num;

            if (tipologia.Equals("Numero Aziendale"))
            {
                celldaeliminare.TIPO_RECAPITO = "02";
            }
            else
            if (tipologia.Equals("Numero Personale"))
            {
                celldaeliminare.TIPO_RECAPITO = "01";
            }
            else
            {

                celldaeliminare.TIPO_RECAPITO = "00";

            }




            celldaeliminare.COD_CONVALIDA_CC = "0";
            celldaeliminare.COD_CMEVENTO = "DELCEL";

            celldaeliminare.ID_PERSONA = y.ID_PERSONA;

            celldaeliminare.ID_EVENTO = db.CMINFOANAG_EXT.GeneraOid(x => x.ID_EVENTO, 9);
            record_codice = getRecordCodiceOtp("02", celldaeliminare.ID_EVENTO, db.XR_SSV_CODICE_OTP.GeneraPrimaryKey(6));

            String idevento = celldaeliminare.ID_EVENTO.ToString();




            string matricola = CommonManager.GetCurrentUserMatricola();

            celldaeliminare.TMS_TIMESTAMP = DateTime.Now;
            celldaeliminare.COD_USER = "P" + matricola;
            celldaeliminare.COD_TERMID = "RAIPERME";


            db.CMINFOANAG_EXT.Add(celldaeliminare);
            db.XR_SSV_CODICE_OTP.Add(record_codice);
            String idev = record_codice.ID_CODICE_OTP.ToString();

            try
            {
                db.SaveChanges();
                GestoreMail mail = new GestoreMail();
                List<myRaiCommonTasks.sendMail.Attachement> attachments = null;

                string dest = CommonTasks.GetEmailPerMatricola(matricola);

                if (String.IsNullOrWhiteSpace(dest))
                    dest = "P" + matricola + "@rai.it";

                string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271”> La tua richiesta di cancellazione del numero di cellulare è stata inserita</p><p> A breve riceverai il codice utile per la convalida.</br> Potrai <b> modificare/annullare </b> la richiesta effettuata in qualsiasi momento dal portale Rai Per Me nella sezione Profilo Personale.</br><i>Attenzione, si tratta di una mail generata automaticamente, si prega di non rispondere.</i></p> ";

                var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                     "Richiesta di cancellazione numero di cellulare",
                     dest,
                     "raiplace.selfservice@rai.it",
                     "Richiesta di cancellazione numero di cellulare",
                     "",
                     corpo,
                     null,
                     null,
                     attachments);

                string corpo2 = "<p align=”justify”>Il codice per convalidare la tua richiesta di cancellazione di numero di cellulare da inserire nel portale Rai per Me nella sezione Profilo Personale – Dati Anagrafici, è il seguente:</p><p><h1 style=”text-align:center”>" + record_codice.ID_CODICE_OTP + "</h1></p>";

                var response2 = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                     "Richiesta di cancellazione numero di cellulare: istruzioni per la convalida",
                     dest,
                     "raiplace.selfservice@rai.it",
                     "Richiesta di cancellazione numero di cellulare",
                     "Convalida richiesta",
                     corpo2,
                     null,
                     null,
                     attachments, DateTime.Now.AddMinutes(1));



                if (response != null && response.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "ELIMINAZIONE NUMERO DI CELLULARE",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };

                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }

                if (response2 != null && response2.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "MAIL CONVALIDA ELIMINAZIONE NUMERO DI CELLULARE",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };



                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "ok" }
                };
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "ELIMINACELLULAREDB"
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }

        public ActionResult DeleteIbanTrasf(String infoiban, String tipoiban)
        {
            var db = ProfiloPersonaleManager.GetTalentiaDB();

            myRaiData.Incentivi.XR_SSV_CODICE_OTP record_codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();



            myRaiData.Incentivi.CMINFOANAG_EXT contodaeliminare = new myRaiData.Incentivi.CMINFOANAG_EXT();
            contodaeliminare.COD_UTILIZZO = "02";

            contodaeliminare.COD_IBAN = infoiban;




            contodaeliminare.COD_CONVALIDA_CC = "0";
            contodaeliminare.COD_CMEVENTO = "DELCC";

            contodaeliminare.ID_PERSONA = Int32.Parse(CommonManager.GetCurrentUserMatricola());

            contodaeliminare.ID_EVENTO = db.CMINFOANAG_EXT.GeneraOid(x => x.ID_EVENTO, 9);
            record_codice = getRecordCodiceOtp("01", contodaeliminare.ID_EVENTO, db.XR_SSV_CODICE_OTP.GeneraPrimaryKey(6));

            String idevento = contodaeliminare.ID_EVENTO.ToString();
            String codPaese = "";
            String codChk = "";
            String codCin = "";
            String codAbi = "";
            String codCab = "";
            String codCC = "";
            String codIban = contodaeliminare.COD_IBAN.ToString();

            if (codIban.Length == 27)
            {
                codPaese = codIban.Substring(0, 2);
                codChk = codIban.Substring(2, 2);
                codCin = codIban.Substring(4, 1);
                codAbi = codIban.Substring(5, 5);
                codCab = codIban.Substring(10, 5);
                codCC = codIban.Substring(15);
            }

            contodaeliminare.COD_ABI = codAbi;
            contodaeliminare.COD_CAB = codCab;
            myRaiData.Incentivi.XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.FirstOrDefault(x => x.COD_ABI == codAbi && x.COD_CAB == codCab);

            if (anagBanca == null) { contodaeliminare.IND_BANCA_ASSENTE = "1"; }
            else
            { contodaeliminare.IND_BANCA_ASSENTE = "2"; }


            string matricola = CommonManager.GetCurrentUserMatricola();

            contodaeliminare.TMS_TIMESTAMP = DateTime.Now;
            contodaeliminare.COD_USER = "P" + matricola;
            contodaeliminare.COD_TERMID = "RAIPERME";


            db.CMINFOANAG_EXT.Add(contodaeliminare);
            db.XR_SSV_CODICE_OTP.Add(record_codice);
            String idev = record_codice.ID_CODICE_OTP.ToString();

            try
            {
                db.SaveChanges();
                GestoreMail mail = new GestoreMail();
                List<myRaiCommonTasks.sendMail.Attachement> attachments = null;

                string dest = CommonTasks.GetEmailPerMatricola(matricola);
                if (String.IsNullOrWhiteSpace(dest))
                    dest = "P" + matricola + "@rai.it";



                string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271”> La tua richiesta di chiusura di dati bancari è stata inserita</p><p> Entro 24H riceverai il codice utile per la convalida.</br> Potrai <b> modificare/annullare </b> la richiesta effettuata in qualsiasi momento dal portale Rai Per Me nella sezione Profilo Personale.</br>Si raccomanda di mantenere <b>temporaneamente</b> attivo il vecchio codice iban, in quanto non si assicura che la procedura, seppur registrata con successo, abbia effetto già per il mese corrente.</p><p style =”text-align:center”><i>Attenzione, si tratta di una mail generata automaticamente, si prega di non rispondere.</i></p> ";

                var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                     "Richiesta di chiusura dei dati bancari anticipo trasferte",
                     dest,
                     "raiplace.selfservice@rai.it",
                     "Richiesta di chiusura IBAN",
                     "",
                     corpo,
                     null,
                     null,
                     attachments);

                string corpo2 = "<p align=”justify”>Il codice per convalidare la tua richiesta di chiusura dei dati bancari da inserire nel portale Rai per Me nella sezione Profilo Personale – Dati Bancari, è il seguente:</p><p><h1 style=”text-align:center”>" + record_codice.ID_CODICE_OTP + "</h1></p>";

                var response2 = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                 "Richiesta di chiusura di dati bancari: istruzioni per la convalida",
                 dest,
                 "raiplace.selfservice@rai.it",
                 "Richiesta di chiusura IBAN",
                 "Convalida richiesta",
                 corpo2,
                 null,
                 null,
                 attachments, DateTime.Now.AddDays(1));



                if (response != null && response.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "CHIUSURA IBAN TRASFERTE",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };

                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }

                if (response2 != null && response2.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "CONVALIDA CHIUSURA IBAN TRASFERTE",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };



                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "ok" }
                };
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "AggiungiNuovoIBAN"
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }


        public ActionResult DeleteIbanSpeseProd(String infoiban, String tipoiban)
        {
            var db = ProfiloPersonaleManager.GetTalentiaDB();

            myRaiData.Incentivi.XR_SSV_CODICE_OTP record_codice = new myRaiData.Incentivi.XR_SSV_CODICE_OTP();




            myRaiData.Incentivi.CMINFOANAG_EXT contodaeliminare = new myRaiData.Incentivi.CMINFOANAG_EXT();
            contodaeliminare.COD_UTILIZZO = "03";

            contodaeliminare.COD_IBAN = infoiban;




            contodaeliminare.COD_CONVALIDA_CC = "0";
            contodaeliminare.COD_CMEVENTO = "DELCC";

            contodaeliminare.ID_PERSONA = Int32.Parse(CommonManager.GetCurrentUserMatricola());

            contodaeliminare.ID_EVENTO = db.CMINFOANAG_EXT.GeneraOid(x => x.ID_EVENTO, 9);
            record_codice = getRecordCodiceOtp("01", contodaeliminare.ID_EVENTO, db.XR_SSV_CODICE_OTP.GeneraPrimaryKey(6));

            String idevento = contodaeliminare.ID_EVENTO.ToString();
            String codPaese = "";
            String codChk = "";
            String codCin = "";
            String codAbi = "";
            String codCab = "";
            String codCC = "";
            String codIban = contodaeliminare.COD_IBAN.ToString();

            if (codIban.Length == 27)
            {
                codPaese = codIban.Substring(0, 2);
                codChk = codIban.Substring(2, 2);
                codCin = codIban.Substring(4, 1);
                codAbi = codIban.Substring(5, 5);
                codCab = codIban.Substring(10, 5);
                codCC = codIban.Substring(15);
            }

            contodaeliminare.COD_ABI = codAbi;
            contodaeliminare.COD_CAB = codCab;
            myRaiData.Incentivi.XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.FirstOrDefault(x => x.COD_ABI == codAbi && x.COD_CAB == codCab);

            if (anagBanca == null) { contodaeliminare.IND_BANCA_ASSENTE = "1"; }
            else
            { contodaeliminare.IND_BANCA_ASSENTE = "2"; }


            string matricola = CommonManager.GetCurrentUserMatricola();

            contodaeliminare.TMS_TIMESTAMP = DateTime.Now;
            contodaeliminare.COD_USER = "P" + matricola;
            contodaeliminare.COD_TERMID = "RAIPERME";

            db.CMINFOANAG_EXT.Add(contodaeliminare);
            db.XR_SSV_CODICE_OTP.Add(record_codice);
            String idev = record_codice.ID_CODICE_OTP.ToString();


            try
            {
                db.SaveChanges();
                GestoreMail mail = new GestoreMail();
                List<myRaiCommonTasks.sendMail.Attachement> attachments = null;
                string dest = CommonTasks.GetEmailPerMatricola(matricola);

                if (String.IsNullOrWhiteSpace(dest))
                    dest = "P" + matricola + "@rai.it";

                string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271”> La tua richiesta di chiusura di dati bancari è stata inserita</p><p> Entro 24H riceverai il codice utile per la convalida.</br> Potrai <b> modificare/annullare </b> la richiesta effettuata in qualsiasi momento dal portale Rai Per Me nella sezione Profilo Personale.</br>Si raccomanda di mantenere <b>temporaneamente</b> attivo il vecchio codice iban, in quanto non si assicura che la procedura, seppur registrata con successo, abbia effetto già per il mese corrente.</p><p style =”text-align:center”><i>Attenzione, si tratta di una mail generata automaticamente, si prega di non rispondere.</i></p>";

                var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                     "Richiesta di chiusura dei dati bancari anticipo spese di produzione",
                     dest,
                     "raiplace.selfservice@rai.it",
                     "Richiesta di chiusura IBAN",
                     "",
                     corpo,
                     null,
                     null,
                     attachments);

                string corpo2 = "<p align=”justify”>Il codice per convalidare la tua richiesta di chiusura dei dati bancari da inserire nel portale Rai per Me nella sezione Profilo Personale – Dati Bancari, è il seguente:</p><p><h1 style=”text-align:center”>" + record_codice.ID_CODICE_OTP + "</h1></p>";



                var response2 = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                     "Richiesta di chiusura dei dati bancari: istruzioni per la convalida",
                     dest,
                     "raiplace.selfservice@rai.it",
                     "Richiesta di chiusura IBAN",
                     "Convalida richiesta",
                     corpo2,
                     null,
                     null,
                     attachments, DateTime.Now.AddDays(1));

                if (response != null && response.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "Aggiunta nuovo IBAN",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };

                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }

                if (response2 != null && response2.Errore != null)
                {
                    MyRai_LogErrori err = new MyRai_LogErrori()
                    {
                        applicativo = "Profilo personale",
                        data = DateTime.Now,
                        provenienza = "Aggiunta nuovo IBAN",
                        error_message = response.Errore + " per " + dest,
                        matricola = "000000"
                    };



                    using (digiGappEntities db2 = new digiGappEntities())
                    {
                        db2.MyRai_LogErrori.Add(err);
                        db2.SaveChanges();
                    }
                }


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "ok" }
                };
            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "AggiungiNuovoIBAN"
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }



        //public ActionResult ConvalidaModifiche(int idevento)
        //{
        //    var db = new myRaiDataTalentia.TalentiaEntities();



        //    var verifica_codice = db.XR_SSV_CODICE_OTP.Find(idevento);

        //    if (verifica_codice == null || verifica_codice.DTA_SCADENZA.CompareTo(DateTime.Today) < 0 || !verifica_codice.COD_FUNZIONE.Equals("01") || !verifica_codice.MATRICOLA.Equals(CommonManager.GetCurrentUserMatricola()))
        //    {


        //        //ProfiloPersonaleModel model2 = CreaProfiloPersonaleModel();
        //        //model2.RichiestaDaConvalidareFound = false;

        //        //return View("Index", model2);


        //        if (TempData.Keys.Contains("RichiestaDaConvalidareFound"))
        //            TempData["RichiestaDaConvalidareFound"] = false;
        //        else
        //            TempData.Add("RichiestaDaConvalidareFound", false);

        //        return Redirect("Index");

        //    }





        //    else {

        //        var iban = db.CMINFOANAG_EXT.Find(verifica_codice.ID_EVENTO);
        //        DateTime fine = new DateTime(2999, 12, 31);

        //        //DateTime oggi = DateTime.Today;
        //        //var anno = oggi.Year;
        //        //var mese = oggi.Month;
        //        //var giorno = oggi.Day;
        //        DateTime ieri = DateTime.Today.AddDays(-1);
        //        myRaiDataTalentia.XR_DATIBANCARI nuovo1 = new myRaiDataTalentia.XR_DATIBANCARI();
        //        myRaiDataTalentia.XR_UTILCONTO nuovo2 = new myRaiDataTalentia.XR_UTILCONTO();
        //        myRaiDataTalentia.XR_UTILCONTO nuovo3 = new myRaiDataTalentia.XR_UTILCONTO();
        //        myRaiDataTalentia.XR_DATIBANCARI nuovo4 = new myRaiDataTalentia.XR_DATIBANCARI();
        //        string matr = CommonManager.GetCurrentUserMatricola();
        //        myRaiDataTalentia.ANAGPERS y = db.ANAGPERS.FirstOrDefault(x => x.COD_MATDIP.Equals(matr));

        //        if (iban == null || iban.COD_CONVALIDA_CC.Equals("1"))
        //    {

        //            //ProfiloPersonaleModel model2 = ProfiloPersonaleManager.GetProfiloPersonaleModel();
        //            //model2.ContiCorrente = ProfiloPersonaleManager.GetListaContoCorrenteTalentia(CommonManager.GetCurrentUserMatricola());
        //            //model2.RichiestaDaConvalidareFound = false;
        //            //ProfiloPersonaleModel model2 = CreaProfiloPersonaleModel();
        //            //model2.RichiestaDaConvalidareFound = false;


        //            //return View("Index", model2);

        //            if (TempData.Keys.Contains("RichiestaDaConvalidareFound"))
        //                TempData["RichiestaDaConvalidareFound"] = false;
        //            else
        //                TempData.Add("RichiestaDaConvalidareFound", false);

        //            return Redirect("Index");

        //        }


        //    else

        //    {




        //        iban.COD_CONVALIDA_CC = "1";
        //            verifica_codice.IND_UTILIZZO = "1";
        //            verifica_codice.DTA_UTILIZZO = DateTime.Now;
        //            verifica_codice.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //            verifica_codice.COD_TERMID = "RAIPERME";

        //            //Caso Modifica 

        //            if (iban.COD_CMEVENTO.Equals("MODCC"))
        //        {
        //            //Caso Modifica Stipendio
        //            if (iban.COD_UTILIZZO.Equals("01"))
        //            {
        //                var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("C")).ToList().OrderByDescending(c => c.DTA_FINE);

        //                if (datibancari != null && datibancari.Count() > 0)
        //                {
        //                    if (datibancari.First().DTA_FINE.Equals(fine))
        //                    {


        //                        var id_datibancari = datibancari.First().ID_XR_DATIBANCARI;



        //                        var util = db.XR_UTILCONTO.FirstOrDefault(x => x.ID_XR_DATIBANCARI == id_datibancari && x.COD_UTILCONTO.Equals("01"));

        //                        datibancari.First().DTA_FINE = ieri;

        //                        nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                        nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                        nuovo1.DTA_INIZIO = DateTime.Today;
        //                        nuovo1.DTA_FINE = fine;
        //                        nuovo1.COD_IBAN = iban.COD_IBAN;
        //                        nuovo1.COD_ABI = iban.COD_ABI;
        //                        nuovo1.COD_CAB = iban.COD_CAB;

        //                        nuovo1.COD_TIPOCONTO = "C";
        //                        nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

        //                        nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                        nuovo1.COD_TERMID = "RAIPERME";

        //                        nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                        nuovo1.COD_SUBTPCONTO = "ND";
        //                        nuovo1.IND_CONGELATO = "N";
        //                        nuovo1.IND_DELETE = "N";
        //                        nuovo1.IND_VINCOLATO = "N";
        //                        nuovo1.IND_CHANGED = "N";


        //                        nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                        nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                        nuovo2.COD_UTILCONTO = "01";
        //                        nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                        nuovo2.COD_TERMID = "RAIPERME";

        //                        nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                        db.XR_DATIBANCARI.Add(nuovo1);
        //                        db.XR_UTILCONTO.Add(nuovo2);


        //                    }

        //                }

        //            }
        //            else
        //                //Caso Modifica Trasferte 

        //                if (iban.COD_UTILIZZO.Equals("02"))
        //            {
        //                var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);

        //                if (datibancari != null && datibancari.Count() > 0)
        //                {
        //                    if (datibancari.First().DTA_FINE.Equals(fine))
        //                    {
        //                        var id_datibancari = datibancari.First().ID_XR_DATIBANCARI;
        //                        var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();
        //                        datibancari.First().DTA_FINE = ieri;

        //                        if (util != null && util.Count() > 0)
        //                        {
        //                            foreach (var item in util)
        //                            {
        //                                if (item.COD_UTILCONTO.Equals("02"))
        //                                {
        //                                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                                    nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                                    nuovo1.DTA_INIZIO = DateTime.Today;
        //                                    nuovo1.DTA_FINE = fine;
        //                                    nuovo1.COD_IBAN = iban.COD_IBAN;
        //                                    nuovo1.COD_ABI = iban.COD_ABI;
        //                                    nuovo1.COD_CAB = iban.COD_CAB;

        //                                    nuovo1.COD_TIPOCONTO = "R";
        //                                    nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

        //                                    nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo1.COD_TERMID = "RAIPERME";

        //                                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                                    nuovo1.COD_SUBTPCONTO = "ND";
        //                                    nuovo1.IND_CONGELATO = "N";
        //                                    nuovo1.IND_DELETE = "N";
        //                                    nuovo1.IND_VINCOLATO = "N";
        //                                    nuovo1.IND_CHANGED = "N";


        //                                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                                    nuovo2.COD_UTILCONTO = "02";
        //                                    nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo2.COD_TERMID = "RAIPERME";

        //                                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                                    db.XR_DATIBANCARI.Add(nuovo1);
        //                                    db.XR_UTILCONTO.Add(nuovo2);

        //                                }
        //                                else
        //                                if (item.COD_UTILCONTO.Equals("03"))
        //                                {
        //                                    nuovo4.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                                    nuovo4.ID_PERSONA = y.ID_PERSONA;
        //                                    nuovo4.DTA_INIZIO = DateTime.Today;
        //                                    nuovo4.DTA_FINE = fine;
        //                                    nuovo4.COD_IBAN = datibancari.First().COD_IBAN;
        //                                    nuovo4.COD_ABI = datibancari.First().COD_ABI;
        //                                    nuovo4.COD_CAB = datibancari.First().COD_CAB;

        //                                    nuovo4.COD_TIPOCONTO = "R";
        //                                    nuovo4.DES_INTESTATARIO = datibancari.First().DES_INTESTATARIO;

        //                                    nuovo4.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo4.COD_TERMID = "RAIPERME";

        //                                    nuovo4.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                                    nuovo4.COD_SUBTPCONTO = "ND";
        //                                    nuovo4.IND_CONGELATO = "N";
        //                                    nuovo4.IND_DELETE = "N";
        //                                    nuovo4.IND_VINCOLATO = "N";
        //                                    nuovo4.IND_CHANGED = "N";


        //                                    nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                                    nuovo3.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                                    nuovo3.COD_UTILCONTO = "03";
        //                                    nuovo3.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo3.COD_TERMID = "RAIPERME";

        //                                    nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                                    db.XR_DATIBANCARI.Add(nuovo4);
        //                                    db.XR_UTILCONTO.Add(nuovo3);

        //                                }


        //                            }

        //                        }


        //                    }
        //                }





        //            }
        //            else
        //                //Caso Modifica Spese di Prod
        //                if (iban.COD_UTILIZZO.Equals("03"))
        //            {
        //                var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);

        //                if (datibancari != null && datibancari.Count() > 0)
        //                {
        //                    if (datibancari.First().DTA_FINE.Equals(fine))
        //                    {
        //                        var id_datibancari = datibancari.First().ID_XR_DATIBANCARI;
        //                        var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();
        //                        datibancari.First().DTA_FINE = ieri;

        //                        if (util != null && util.Count() > 0)
        //                        {
        //                            foreach (var item in util)
        //                            {
        //                                if (item.COD_UTILCONTO.Equals("03"))
        //                                {
        //                                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                                    nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                                    nuovo1.DTA_INIZIO = DateTime.Today;
        //                                    nuovo1.DTA_FINE = fine;
        //                                    nuovo1.COD_IBAN = iban.COD_IBAN;
        //                                    nuovo1.COD_ABI = iban.COD_ABI;
        //                                    nuovo1.COD_CAB = iban.COD_CAB;

        //                                    nuovo1.COD_TIPOCONTO = "R";
        //                                    nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

        //                                    nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo1.COD_TERMID = "RAIPERME";

        //                                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                                    nuovo1.COD_SUBTPCONTO = "ND";
        //                                    nuovo1.IND_CONGELATO = "N";
        //                                    nuovo1.IND_DELETE = "N";
        //                                    nuovo1.IND_VINCOLATO = "N";
        //                                    nuovo1.IND_CHANGED = "N";


        //                                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                                    nuovo2.COD_UTILCONTO = "03";
        //                                    nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo2.COD_TERMID = "RAIPERME";

        //                                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                                    db.XR_DATIBANCARI.Add(nuovo1);
        //                                    db.XR_UTILCONTO.Add(nuovo2);

        //                                }
        //                                else
        //                                if (item.COD_UTILCONTO.Equals("02"))
        //                                {
        //                                    nuovo4.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                                    nuovo4.ID_PERSONA = y.ID_PERSONA;
        //                                    nuovo4.DTA_INIZIO = DateTime.Today;
        //                                    nuovo4.DTA_FINE = fine;
        //                                    nuovo4.COD_IBAN = datibancari.First().COD_IBAN;
        //                                    nuovo4.COD_ABI = datibancari.First().COD_ABI;
        //                                    nuovo4.COD_CAB = datibancari.First().COD_CAB;

        //                                    nuovo4.COD_TIPOCONTO = "R";
        //                                    nuovo4.DES_INTESTATARIO = datibancari.First().DES_INTESTATARIO;

        //                                    nuovo4.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo4.COD_TERMID = "RAIPERME";

        //                                    nuovo4.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                                    nuovo4.COD_SUBTPCONTO = "ND";
        //                                    nuovo4.IND_CONGELATO = "N";
        //                                    nuovo4.IND_DELETE = "N";
        //                                    nuovo4.IND_VINCOLATO = "N";
        //                                    nuovo4.IND_CHANGED = "N";


        //                                    nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                                    nuovo3.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                                    nuovo3.COD_UTILCONTO = "02";
        //                                    nuovo3.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo3.COD_TERMID = "RAIPERME";

        //                                    nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                                    db.XR_DATIBANCARI.Add(nuovo4);
        //                                    db.XR_UTILCONTO.Add(nuovo2);

        //                                }


        //                            }

        //                        }


        //                    }
        //                }





        //            }






        //        }
        //        else

        //        if (iban.COD_CMEVENTO.Equals("DELCC"))
        //        {
        //            //Caso Eliminazione Trasferte
        //            if (iban.COD_UTILIZZO.Equals("02"))
        //            {
        //                var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);

        //                if (datibancari != null && datibancari.Count() > 0)
        //                {
        //                    if (datibancari.First().DTA_FINE.Equals(fine))
        //                    {
        //                        var id_datibancari = datibancari.First().ID_XR_DATIBANCARI;
        //                        var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();
        //                        datibancari.First().DTA_FINE = ieri;

        //                        if (util != null && util.Count() > 0)
        //                        {
        //                            foreach (var item in util)
        //                            {

        //                                if (item.COD_UTILCONTO.Equals("03"))
        //                                {
        //                                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                                    nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                                    nuovo1.DTA_INIZIO = DateTime.Today;
        //                                    nuovo1.DTA_FINE = fine;
        //                                    nuovo1.COD_IBAN = datibancari.First().COD_IBAN;
        //                                    nuovo1.COD_ABI = datibancari.First().COD_ABI;
        //                                    nuovo1.COD_CAB = datibancari.First().COD_CAB;

        //                                    nuovo1.COD_TIPOCONTO = "R";
        //                                    nuovo1.DES_INTESTATARIO = datibancari.First().DES_INTESTATARIO;

        //                                    nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo1.COD_TERMID = "RAIPERME";

        //                                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                                    nuovo1.COD_SUBTPCONTO = "ND";
        //                                    nuovo1.IND_CONGELATO = "N";
        //                                    nuovo1.IND_DELETE = "N";
        //                                    nuovo1.IND_VINCOLATO = "N";
        //                                    nuovo1.IND_CHANGED = "N";


        //                                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                                    nuovo2.COD_UTILCONTO = "03";
        //                                    nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo2.COD_TERMID = "RAIPERME";

        //                                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                                    db.XR_DATIBANCARI.Add(nuovo1);
        //                                    db.XR_UTILCONTO.Add(nuovo2);

        //                                }


        //                            }

        //                        }


        //                    }
        //                }





        //            }
        //            else
        //                //Caso Elimina Spese di Prod
        //                if (iban.COD_UTILIZZO.Equals("03"))
        //            {
        //                var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);

        //                if (datibancari != null && datibancari.Count() > 0)
        //                {
        //                    if (datibancari.First().DTA_FINE.Equals(fine))
        //                    {
        //                        var id_datibancari = datibancari.First().ID_XR_DATIBANCARI;
        //                        var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();
        //                        datibancari.First().DTA_FINE = ieri;

        //                        if (util != null && util.Count() > 0)
        //                        {
        //                            foreach (var item in util)
        //                            {

        //                                if (item.COD_UTILCONTO.Equals("02"))
        //                                {
        //                                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                                    nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                                    nuovo1.DTA_INIZIO = DateTime.Today;
        //                                    nuovo1.DTA_FINE = fine;
        //                                    nuovo1.COD_IBAN = datibancari.First().COD_IBAN;
        //                                    nuovo1.COD_ABI = datibancari.First().COD_ABI;
        //                                    nuovo1.COD_CAB = datibancari.First().COD_CAB;

        //                                    nuovo1.COD_TIPOCONTO = "R";
        //                                    nuovo1.DES_INTESTATARIO = datibancari.First().DES_INTESTATARIO;

        //                                    nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo1.COD_TERMID = "RAIPERME";

        //                                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                                    nuovo1.COD_SUBTPCONTO = "ND";
        //                                    nuovo1.IND_CONGELATO = "N";
        //                                    nuovo1.IND_DELETE = "N";
        //                                    nuovo1.IND_VINCOLATO = "N";
        //                                    nuovo1.IND_CHANGED = "N";


        //                                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                                    nuovo2.COD_UTILCONTO = "02";
        //                                    nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo2.COD_TERMID = "RAIPERME";

        //                                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                                    db.XR_DATIBANCARI.Add(nuovo1);
        //                                    db.XR_UTILCONTO.Add(nuovo2);

        //                                }


        //                            }

        //                        }


        //                    }
        //                }





        //            }


        //        }
        //        else

        //          if (iban.COD_CMEVENTO.Equals("INSCC"))
        //        {
        //            //caso stipendio
        //            if (iban.COD_UTILIZZO.Equals("01"))
        //            {


        //                if (y == null)
        //                {
        //return new JsonResult
        //{
        //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //                        Data = new { result = "NO" }
        //};
        //                }
        //                else
        //                {




        //                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                    nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                    nuovo1.DTA_INIZIO = DateTime.Today;
        //                    nuovo1.DTA_FINE = fine;
        //                    nuovo1.COD_IBAN = iban.COD_IBAN;
        //                    nuovo1.COD_ABI = iban.COD_ABI;
        //                    nuovo1.COD_CAB = iban.COD_CAB;

        //                    nuovo1.COD_TIPOCONTO = "C";
        //                    nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

        //                    nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                    nuovo1.COD_TERMID = "RAIPERME";

        //                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                    nuovo1.COD_SUBTPCONTO = "ND";
        //                    nuovo1.IND_CONGELATO = "N";
        //                    nuovo1.IND_DELETE = "N";
        //                    nuovo1.IND_VINCOLATO = "N";
        //                    nuovo1.IND_CHANGED = "N";


        //                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                    nuovo2.COD_UTILCONTO = "01";
        //                    nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                    nuovo2.COD_TERMID = "RAIPERME";

        //                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                    db.XR_DATIBANCARI.Add(nuovo1);
        //                    db.XR_UTILCONTO.Add(nuovo2);


        //                }


        //            }
        //            else

        //            //Caso tutti gli anticipi
        //            if (iban.COD_UTILIZZO.Equals("04"))
        //            {
        //                var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);

        //                if (datibancari != null && datibancari.Count() > 0)
        //                {
        //                    //Ho già lo stesso IBAN per le Spese di Prod, quindi chiudo tutto e lo riapro per entrambi
        //                    if (datibancari.First().DTA_FINE.Equals(fine) && datibancari.First().COD_IBAN.Equals(iban.COD_IBAN))
        //                    {
        //                        var id_datibancari = datibancari.First().ID_XR_DATIBANCARI;
        //                        var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();
        //                        datibancari.First().DTA_FINE = ieri;

        //                        nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                        nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                        nuovo1.DTA_INIZIO = DateTime.Today;
        //                        nuovo1.DTA_FINE = fine;
        //                        nuovo1.COD_IBAN = iban.COD_IBAN;
        //                        nuovo1.COD_ABI = iban.COD_ABI;
        //                        nuovo1.COD_CAB = iban.COD_CAB;

        //                        nuovo1.COD_TIPOCONTO = "R";
        //                        nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

        //                        nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                        nuovo1.COD_TERMID = "RAIPERME";

        //                        nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                        nuovo1.COD_SUBTPCONTO = "ND";
        //                        nuovo1.IND_CONGELATO = "N";
        //                        nuovo1.IND_DELETE = "N";
        //                        nuovo1.IND_VINCOLATO = "N";
        //                        nuovo1.IND_CHANGED = "N";


        //                        nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                        nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                        nuovo2.COD_UTILCONTO = "02";
        //                        nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                        nuovo2.COD_TERMID = "RAIPERME";

        //                        nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                        nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                        nuovo3.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                        nuovo3.COD_UTILCONTO = "03";
        //                        nuovo3.TMS_TIMESTAMP = DateTime.Today;
        //                        nuovo3.COD_TERMID = "RAIPERME";

        //                        nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                        db.XR_DATIBANCARI.Add(nuovo1);
        //                        db.XR_UTILCONTO.Add(nuovo2);
        //                        db.XR_UTILCONTO.Add(nuovo3);


        //                    }
        //                    else
        //                    {

        //                        nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                        nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                        nuovo1.DTA_INIZIO = DateTime.Today;
        //                        nuovo1.DTA_FINE = fine;
        //                        nuovo1.COD_IBAN = iban.COD_IBAN;
        //                        nuovo1.COD_ABI = iban.COD_ABI;
        //                        nuovo1.COD_CAB = iban.COD_CAB;

        //                        nuovo1.COD_TIPOCONTO = "R";
        //                        nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

        //                        nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                        nuovo1.COD_TERMID = "RAIPERME";

        //                        nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                        nuovo1.COD_SUBTPCONTO = "ND";
        //                        nuovo1.IND_CONGELATO = "N";
        //                        nuovo1.IND_DELETE = "N";
        //                        nuovo1.IND_VINCOLATO = "N";
        //                        nuovo1.IND_CHANGED = "N";


        //                        nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                        nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                        nuovo2.COD_UTILCONTO = "02";
        //                        nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                        nuovo2.COD_TERMID = "RAIPERME";

        //                        nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                        nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                        nuovo3.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                        nuovo3.COD_UTILCONTO = "03";
        //                        nuovo3.TMS_TIMESTAMP = DateTime.Today;
        //                        nuovo3.COD_TERMID = "RAIPERME";

        //                        nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                        db.XR_DATIBANCARI.Add(nuovo1);
        //                        db.XR_UTILCONTO.Add(nuovo2);
        //                        db.XR_UTILCONTO.Add(nuovo3);



        //                    }
        //                }
        //                else
        //                {

        //                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                    nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                    nuovo1.DTA_INIZIO = DateTime.Today;
        //                    nuovo1.DTA_FINE = fine;
        //                    nuovo1.COD_IBAN = iban.COD_IBAN;
        //                    nuovo1.COD_ABI = iban.COD_ABI;
        //                    nuovo1.COD_CAB = iban.COD_CAB;

        //                    nuovo1.COD_TIPOCONTO = "R";
        //                    nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

        //                    nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                    nuovo1.COD_TERMID = "RAIPERME";

        //                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                    nuovo1.COD_SUBTPCONTO = "ND";
        //                    nuovo1.IND_CONGELATO = "N";
        //                    nuovo1.IND_DELETE = "N";
        //                    nuovo1.IND_VINCOLATO = "N";
        //                    nuovo1.IND_CHANGED = "N";


        //                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                    nuovo2.COD_UTILCONTO = "02";
        //                    nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                    nuovo2.COD_TERMID = "RAIPERME";

        //                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                    nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                    nuovo3.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                    nuovo3.COD_UTILCONTO = "03";
        //                    nuovo3.TMS_TIMESTAMP = DateTime.Today;
        //                    nuovo3.COD_TERMID = "RAIPERME";

        //                    nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                    db.XR_DATIBANCARI.Add(nuovo1);
        //                    db.XR_UTILCONTO.Add(nuovo2);
        //                    db.XR_UTILCONTO.Add(nuovo3);

        //                }





        //            }
        //            else
        //            //Caso Inserimento Trasferte
        //            if (iban.COD_UTILIZZO.Equals("02"))
        //            {
        //                var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);

        //                if (datibancari != null && datibancari.Count() > 0)
        //                {
        //                    //Ho già lo stesso IBAN per le Spese di Prod, quindi chiudo tutto e lo riapro per entrambi
        //                    if (datibancari.First().DTA_FINE.Equals(fine) && datibancari.First().COD_IBAN.Equals(iban.COD_IBAN))
        //                    {
        //                        var id_datibancari = datibancari.First().ID_XR_DATIBANCARI;
        //                        var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();
        //                        datibancari.First().DTA_FINE = ieri;

        //                        if (util != null && util.Count() > 0)
        //                        {
        //                            foreach (var item in util)
        //                            {

        //                                if (item.COD_UTILCONTO.Equals("03"))
        //                                {
        //                                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                                    nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                                    nuovo1.DTA_INIZIO = DateTime.Today;
        //                                    nuovo1.DTA_FINE = fine;
        //                                    nuovo1.COD_IBAN = datibancari.First().COD_IBAN;
        //                                    nuovo1.COD_ABI = datibancari.First().COD_ABI;
        //                                    nuovo1.COD_CAB = datibancari.First().COD_CAB;

        //                                    nuovo1.COD_TIPOCONTO = "R";
        //                                    nuovo1.DES_INTESTATARIO = datibancari.First().DES_INTESTATARIO;

        //                                    nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo1.COD_TERMID = "RAIPERME";

        //                                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                                    nuovo1.COD_SUBTPCONTO = "ND";
        //                                    nuovo1.IND_CONGELATO = "N";
        //                                    nuovo1.IND_DELETE = "N";
        //                                    nuovo1.IND_VINCOLATO = "N";
        //                                    nuovo1.IND_CHANGED = "N";


        //                                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                                    nuovo2.COD_UTILCONTO = "03";
        //                                    nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo2.COD_TERMID = "RAIPERME";

        //                                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                                    nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                                    nuovo3.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                                    nuovo3.COD_UTILCONTO = "02";
        //                                    nuovo3.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo3.COD_TERMID = "RAIPERME";

        //                                    nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                                    db.XR_DATIBANCARI.Add(nuovo1);
        //                                    db.XR_UTILCONTO.Add(nuovo2);
        //                                    db.XR_UTILCONTO.Add(nuovo3);

        //                                }


        //                            }

        //                        }


        //                    }
        //                    else
        //                    {

        //                        nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                        nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                        nuovo1.DTA_INIZIO = DateTime.Today;
        //                        nuovo1.DTA_FINE = fine;
        //                        nuovo1.COD_IBAN = iban.COD_IBAN;
        //                        nuovo1.COD_ABI = iban.COD_ABI;
        //                        nuovo1.COD_CAB = iban.COD_CAB;

        //                        nuovo1.COD_TIPOCONTO = "R";
        //                        nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

        //                        nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                        nuovo1.COD_TERMID = "RAIPERME";

        //                        nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                        nuovo1.COD_SUBTPCONTO = "ND";
        //                        nuovo1.IND_CONGELATO = "N";
        //                        nuovo1.IND_DELETE = "N";
        //                        nuovo1.IND_VINCOLATO = "N";
        //                        nuovo1.IND_CHANGED = "N";


        //                        nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                        nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                        nuovo2.COD_UTILCONTO = "02";
        //                        nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                        nuovo2.COD_TERMID = "RAIPERME";

        //                        nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                        db.XR_DATIBANCARI.Add(nuovo1);
        //                        db.XR_UTILCONTO.Add(nuovo2);



        //                    }
        //                }
        //                else
        //                {

        //                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                    nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                    nuovo1.DTA_INIZIO = DateTime.Today;
        //                    nuovo1.DTA_FINE = fine;
        //                    nuovo1.COD_IBAN = iban.COD_IBAN;
        //                    nuovo1.COD_ABI = iban.COD_ABI;
        //                    nuovo1.COD_CAB = iban.COD_CAB;

        //                    nuovo1.COD_TIPOCONTO = "R";
        //                    nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

        //                    nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                    nuovo1.COD_TERMID = "RAIPERME";

        //                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                    nuovo1.COD_SUBTPCONTO = "ND";
        //                    nuovo1.IND_CONGELATO = "N";
        //                    nuovo1.IND_DELETE = "N";
        //                    nuovo1.IND_VINCOLATO = "N";
        //                    nuovo1.IND_CHANGED = "N";


        //                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                    nuovo2.COD_UTILCONTO = "02";
        //                    nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                    nuovo2.COD_TERMID = "RAIPERME";

        //                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                    db.XR_DATIBANCARI.Add(nuovo1);
        //                    db.XR_UTILCONTO.Add(nuovo2);

        //                }





        //            }
        //            else
        //            //inserimento Spese Prod 
        //            if (iban.COD_UTILIZZO.Equals("03"))
        //            {
        //                var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);

        //                if (datibancari != null && datibancari.Count() > 0)
        //                {
        //                    //Ho già lo stesso IBAN per le Trasferte, quindi chiudo tutto e lo riapro per entrambi
        //                    if (datibancari.First().DTA_FINE.Equals(fine) && datibancari.First().COD_IBAN.Equals(iban.COD_IBAN))
        //                    {
        //                        var id_datibancari = datibancari.First().ID_XR_DATIBANCARI;
        //                        var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();
        //                        datibancari.First().DTA_FINE = ieri;

        //                        if (util != null && util.Count() > 0)
        //                        {
        //                            foreach (var item in util)
        //                            {

        //                                if (item.COD_UTILCONTO.Equals("02"))
        //                                {
        //                                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                                    nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                                    nuovo1.DTA_INIZIO = DateTime.Today;
        //                                    nuovo1.DTA_FINE = fine;
        //                                    nuovo1.COD_IBAN = datibancari.First().COD_IBAN;
        //                                    nuovo1.COD_ABI = datibancari.First().COD_ABI;
        //                                    nuovo1.COD_CAB = datibancari.First().COD_CAB;

        //                                    nuovo1.COD_TIPOCONTO = "R";
        //                                    nuovo1.DES_INTESTATARIO = datibancari.First().DES_INTESTATARIO;

        //                                    nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo1.COD_TERMID = "RAIPERME";

        //                                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                                    nuovo1.COD_SUBTPCONTO = "ND";
        //                                    nuovo1.IND_CONGELATO = "N";
        //                                    nuovo1.IND_DELETE = "N";
        //                                    nuovo1.IND_VINCOLATO = "N";
        //                                    nuovo1.IND_CHANGED = "N";


        //                                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                                    nuovo2.COD_UTILCONTO = "03";
        //                                    nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo2.COD_TERMID = "RAIPERME";

        //                                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                                    nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                                    nuovo3.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                                    nuovo3.COD_UTILCONTO = "02";
        //                                    nuovo3.TMS_TIMESTAMP = DateTime.Today;
        //                                    nuovo3.COD_TERMID = "RAIPERME";

        //                                    nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                                    db.XR_DATIBANCARI.Add(nuovo1);
        //                                    db.XR_UTILCONTO.Add(nuovo2);
        //                                    db.XR_UTILCONTO.Add(nuovo3);

        //                                }


        //                            }

        //                        }


        //                    }
        //                    else
        //                    {

        //                        nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                        nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                        nuovo1.DTA_INIZIO = DateTime.Today;
        //                        nuovo1.DTA_FINE = fine;
        //                        nuovo1.COD_IBAN = iban.COD_IBAN;
        //                        nuovo1.COD_ABI = iban.COD_ABI;
        //                        nuovo1.COD_CAB = iban.COD_CAB;

        //                        nuovo1.COD_TIPOCONTO = "R";
        //                        nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

        //                        nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                        nuovo1.COD_TERMID = "RAIPERME";

        //                        nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                        nuovo1.COD_SUBTPCONTO = "ND";
        //                        nuovo1.IND_CONGELATO = "N";
        //                        nuovo1.IND_DELETE = "N";
        //                        nuovo1.IND_VINCOLATO = "N";
        //                        nuovo1.IND_CHANGED = "N";


        //                        nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                        nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                        nuovo2.COD_UTILCONTO = "03";
        //                        nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                        nuovo2.COD_TERMID = "RAIPERME";

        //                        nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                        db.XR_DATIBANCARI.Add(nuovo1);
        //                        db.XR_UTILCONTO.Add(nuovo2);



        //                    }
        //                }
        //                else
        //                {

        //                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
        //                    nuovo1.ID_PERSONA = y.ID_PERSONA;
        //                    nuovo1.DTA_INIZIO = DateTime.Today;
        //                    nuovo1.DTA_FINE = fine;
        //                    nuovo1.COD_IBAN = iban.COD_IBAN;
        //                    nuovo1.COD_ABI = iban.COD_ABI;
        //                    nuovo1.COD_CAB = iban.COD_CAB;

        //                    nuovo1.COD_TIPOCONTO = "R";
        //                    nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

        //                    nuovo1.TMS_TIMESTAMP = DateTime.Today;
        //                    nuovo1.COD_TERMID = "RAIPERME";

        //                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
        //                    nuovo1.COD_SUBTPCONTO = "ND";
        //                    nuovo1.IND_CONGELATO = "N";
        //                    nuovo1.IND_DELETE = "N";
        //                    nuovo1.IND_VINCOLATO = "N";
        //                    nuovo1.IND_CHANGED = "N";


        //                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

        //                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
        //                    nuovo2.COD_UTILCONTO = "03";
        //                    nuovo2.TMS_TIMESTAMP = DateTime.Today;
        //                    nuovo2.COD_TERMID = "RAIPERME";

        //                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

        //                    db.XR_DATIBANCARI.Add(nuovo1);
        //                    db.XR_UTILCONTO.Add(nuovo2);



        //                }




        //            }



        //        }

        //    }


        //    try
        //    {
        //        db.SaveChanges();

        //        ProfiloPersonaleModel model2 = CreaProfiloPersonaleModel();
        //            model2.RichiestaConvalidata = true;

        //        GestoreMail mail = new GestoreMail();
        //        List<myRaiCommonTasks.sendMail.Attachement> attachments = null;
        //        string dest = CommonTasks.GetEmailPerMatricola(matr);

        //        if (String.IsNullOrWhiteSpace(dest))
        //            dest = "P" + matr + "@rai.it";

        //        string corpo = "La richiesta di variazione di dati bancari è stata convalidata";

        //        var response = mail.InvioMail("[CG] RaiPlace - Self Service <raiplace.selfservice@rai.it>",
        //             "Conferma convalida variazione dati bancari ",
        //             dest,
        //             "raiplace.selfservice@rai.it",
        //             "Conferma convalida IBAN",
        //             "Avviso di convalida richiesta",
        //             corpo,
        //             null,
        //             null,
        //             attachments);


        //        if (response != null && response.Errore != null)
        //        {
        //            MyRai_LogErrori err = new MyRai_LogErrori()
        //            {
        //                applicativo = "Profilo personale",
        //                data = DateTime.Now,
        //                provenienza = "Convalida variazione IBAN",
        //                error_message = response.Errore + " per " + dest,
        //                matricola = "000000"
        //            };

        //            using (digiGappEntities db2 = new digiGappEntities())
        //            {
        //                db2.MyRai_LogErrori.Add(err);
        //                db2.SaveChanges();
        //            }
        //        }



        //        return View("Index", model2);

        //    }
        //    catch (Exception ex)
        //    {
        //        Business.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
        //        {
        //            applicativo = "PORTALE",
        //            data = DateTime.Now,
        //            error_message = ex.ToString(),
        //            matricola = CommonManager.GetCurrentUserMatricola(),
        //            provenienza = "ConvalidaModificaIBAN"
        //        });
        //        return new JsonResult
        //        {
        //            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //            Data = new { result = ex.Message }
        //        };
        //    }

        //}



        //}


        public ActionResult ConvalidaModificheCodice(ProfiloPersonaleModel mod)
        {

            var db = ProfiloPersonaleManager.GetTalentiaDB();

            var cod = mod.codice.ID_CODICE_OTP;

            var verifica_codice = db.XR_SSV_CODICE_OTP.Find(cod);

            if (verifica_codice == null || verifica_codice.DTA_SCADENZA.CompareTo(DateTime.Today) < 0 || !verifica_codice.COD_FUNZIONE.Equals("01") || !verifica_codice.MATRICOLA.Equals(CommonManager.GetCurrentUserMatricola()))
            {


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "Codice non valido o scaduto" }
                };


            }
            else
            {

                String vincolo = "";

                var iban = db.CMINFOANAG_EXT.Find(verifica_codice.ID_EVENTO);
                DateTime fine = new DateTime(2999, 12, 31);


                DateTime ieri = DateTime.Today.AddDays(-1);
                DateTime oggi = DateTime.Today;
                myRaiData.Incentivi.XR_DATIBANCARI nuovo1 = new myRaiData.Incentivi.XR_DATIBANCARI();
                myRaiData.Incentivi.XR_UTILCONTO nuovo2 = new myRaiData.Incentivi.XR_UTILCONTO();
                myRaiData.Incentivi.XR_UTILCONTO nuovo3 = new myRaiData.Incentivi.XR_UTILCONTO();
                myRaiData.Incentivi.XR_DATIBANCARI nuovo4 = new myRaiData.Incentivi.XR_DATIBANCARI();
                String matr = CommonManager.GetCurrentUserMatricola();
                myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));



                if (iban == null || iban.COD_CONVALIDA_CC.Equals("1"))
                {


                    ProfiloPersonaleModel model2 = CreaProfiloPersonaleModel();
                    model2.RichiestaDaConvalidareFound = false;


                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "La richiesta da convalidare non è stata trovata" }
                    };


                }


                else

                {




                    iban.COD_CONVALIDA_CC = "1";
                    verifica_codice.IND_UTILIZZO = "1";
                    verifica_codice.DTA_UTILIZZO = DateTime.Now;
                    verifica_codice.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                    verifica_codice.COD_TERMID = "RAIPERME";

                    //Caso Modifica 

                    if (iban.COD_CMEVENTO.Equals("MODCC"))
                    {
                        //Caso Modifica Stipendio
                        if (iban.COD_UTILIZZO.Equals("01"))
                        {
                            var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("C")).ToList().OrderByDescending(c => c.DTA_FINE);
                            string id_stato;
                            if (datibancari != null && datibancari.Count() > 0)
                            {
                                if (datibancari.First().DTA_FINE.Equals(fine))
                                {
                                    id_stato = datibancari.First().COD_STATO;

                                    var id_datibancari = datibancari.First().ID_XR_DATIBANCARI;



                                    var util = db.XR_UTILCONTO.FirstOrDefault(x => x.ID_XR_DATIBANCARI == id_datibancari && x.COD_UTILCONTO.Equals("01"));

                                    var recordcorrente = datibancari.First();
                                    var utilcorrente = db.XR_UTILCONTO.Where(s => s.ID_XR_DATIBANCARI == recordcorrente.ID_XR_DATIBANCARI).ToList();

                                    if (!datibancari.First().DTA_INIZIO.Equals(oggi))
                                    {
                                    datibancari.First().DTA_FINE = ieri;
                                    }
                                 
                                    

                                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                    nuovo1.ID_PERSONA = y.ID_PERSONA;
                                    nuovo1.DTA_INIZIO = DateTime.Today;
                                    nuovo1.DTA_FINE = fine;
                                    nuovo1.COD_IBAN = iban.COD_IBAN;
                                    nuovo1.COD_ABI = iban.COD_ABI;
                                    nuovo1.COD_CAB = iban.COD_CAB;

                                    nuovo1.COD_STATO = id_stato;

                                    nuovo1.COD_TIPOCONTO = "C";
                                    nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

                                    nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                    nuovo1.COD_TERMID = "RAIPERME";

                                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                    nuovo1.COD_SUBTPCONTO = "ND";
                                    nuovo1.IND_CONGELATO = "N";
                                    nuovo1.IND_DELETE = "N";
                                    nuovo1.IND_VINCOLATO = "N";
                                    nuovo1.IND_CHANGED = "N";


                                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                    nuovo2.COD_UTILCONTO = "01";
                                    nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                    nuovo2.COD_TERMID = "RAIPERME";

                                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                    db.XR_DATIBANCARI.Add(nuovo1);
                                    db.XR_UTILCONTO.Add(nuovo2);

                                    if (recordcorrente.DTA_INIZIO.Equals(oggi))
                                    {


                                        foreach (var item in utilcorrente)
                                        {
                                            db.XR_UTILCONTO.Remove(item);
                                            db.SaveChanges();
                                        }

                                        db.XR_DATIBANCARI.Remove(recordcorrente);
                                        db.SaveChanges();

                                    }

                                    vincolo = "<p> Si ricorda che, in caso di conto <b>vincolato</b>, è propria responsabilità consegnare la modulistica necessaria all'ufficio di competenza per le coordinate bancarie dichiarate per l'accredito dello stipendio.</p>";


                                }

                            }

                        }
                        else
                            //Caso Modifica Trasferte 

                            if (iban.COD_UTILIZZO.Equals("02"))
                        {
                            var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);
                            var correnti = datibancari.First();
                            foreach (var item in datibancari)
                            {
                                if (item.DTA_FINE.Equals(fine))
                                {

                                    var esiste = db.XR_UTILCONTO.FirstOrDefault(x => x.ID_XR_DATIBANCARI == item.ID_XR_DATIBANCARI && x.COD_UTILCONTO.Equals("02"));

                                    if (esiste != null)

                                    {
                                        if (esiste.COD_UTILCONTO.Equals("02"))
                                        {
                                            correnti = item;
                                        }
                                    }

                                }
                            }

                            if (datibancari != null && datibancari.Count() > 0)
                            {
                                if (correnti.DTA_FINE.Equals(fine))
                                {
                                    var id_datibancari = correnti.ID_XR_DATIBANCARI;
                                    var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();


                                    //var correnti = datibancari.First();

                                    var utilcorrente = db.XR_UTILCONTO.Where(s => s.ID_XR_DATIBANCARI == correnti.ID_XR_DATIBANCARI).ToList();

                                    if (!datibancari.First().DTA_INIZIO.Equals(oggi))
                                    {
                                    datibancari.First().DTA_FINE = ieri;
                                    }

                                    if (util != null && util.Count() > 0)
                                    {
                                        foreach (var item in util)
                                        {
                                            if (item.COD_UTILCONTO.Equals("02"))
                                            {
                                                nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                                nuovo1.ID_PERSONA = y.ID_PERSONA;
                                                nuovo1.DTA_INIZIO = DateTime.Today;
                                                nuovo1.DTA_FINE = fine;
                                                nuovo1.COD_IBAN = iban.COD_IBAN;
                                                nuovo1.COD_ABI = iban.COD_ABI;
                                                nuovo1.COD_CAB = iban.COD_CAB;

                                                nuovo1.COD_TIPOCONTO = "R";
                                                nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

                                                nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo1.COD_TERMID = "RAIPERME";

                                                nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                                nuovo1.COD_SUBTPCONTO = "ND";
                                                nuovo1.IND_CONGELATO = "N";
                                                nuovo1.IND_DELETE = "N";
                                                nuovo1.IND_VINCOLATO = "N";
                                                nuovo1.IND_CHANGED = "N";


                                                nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                                nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                                nuovo2.COD_UTILCONTO = "02";
                                                nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo2.COD_TERMID = "RAIPERME";

                                                nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                                db.XR_DATIBANCARI.Add(nuovo1);
                                                db.XR_UTILCONTO.Add(nuovo2);

                                            }
                                            else
                                            if (item.COD_UTILCONTO.Equals("03"))
                                            {
                                                nuovo4.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                                nuovo4.ID_PERSONA = y.ID_PERSONA;
                                                nuovo4.DTA_INIZIO = correnti.DTA_INIZIO;
                                                nuovo4.DTA_FINE = fine;
                                                nuovo4.COD_IBAN = correnti.COD_IBAN;
                                                nuovo4.COD_ABI = correnti.COD_ABI;
                                                nuovo4.COD_CAB = correnti.COD_CAB;

                                                nuovo4.COD_TIPOCONTO = "R";
                                                nuovo4.DES_INTESTATARIO = correnti.DES_INTESTATARIO;

                                                nuovo4.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo4.COD_TERMID = "RAIPERME";

                                                nuovo4.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                                nuovo4.COD_SUBTPCONTO = "ND";
                                                nuovo4.IND_CONGELATO = "N";
                                                nuovo4.IND_DELETE = "N";
                                                nuovo4.IND_VINCOLATO = "N";
                                                nuovo4.IND_CHANGED = "N";


                                                nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                                nuovo3.ID_XR_DATIBANCARI = nuovo4.ID_XR_DATIBANCARI;
                                                nuovo3.COD_UTILCONTO = "03";
                                                nuovo3.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo3.COD_TERMID = "RAIPERME";

                                                nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                                db.XR_DATIBANCARI.Add(nuovo4);
                                                db.XR_UTILCONTO.Add(nuovo3);

                                            }


                                        }

                                    }


                                    if (correnti.DTA_INIZIO.Equals(oggi))
                                    {


                                        foreach (var item in utilcorrente)
                                        {
                                            db.XR_UTILCONTO.Remove(item);
                                            db.SaveChanges();
                                        }

                                        db.XR_DATIBANCARI.Remove(correnti);
                                        db.SaveChanges();

                                    }



                                }
                            }





                        }
                        else
                            //Caso Modifica Spese di Prod
                            if (iban.COD_UTILIZZO.Equals("03"))
                        {
                            var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);
                            var correnti = datibancari.First();

                            foreach (var item in datibancari)
                            {
                                if (item.DTA_FINE.Equals(fine))
                                {

                                    var esiste = db.XR_UTILCONTO.FirstOrDefault(x => x.ID_XR_DATIBANCARI == item.ID_XR_DATIBANCARI && x.COD_UTILCONTO.Equals("03"));

                                    if (esiste != null)

                                    {
                                        if (esiste.COD_UTILCONTO.Equals("03")) { 
                                        correnti = item;
                                        }
                                    }

                                }
                            }

                            if (datibancari != null && datibancari.Count() > 0)
                            {
                                if (correnti.DTA_FINE.Equals(fine))
                                {
                                    var id_datibancari = correnti.ID_XR_DATIBANCARI;
                                    var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();

                                    //var recordcorrente = datibancari.First();

                                    var utilcorrente = db.XR_UTILCONTO.Where(s => s.ID_XR_DATIBANCARI == correnti.ID_XR_DATIBANCARI).ToList();

                                    if (!correnti.DTA_INIZIO.Equals(oggi))
                                    {
                                        correnti.DTA_FINE = ieri;
                                    }


                                    if (util != null && util.Count() > 0)
                                    {
                                        foreach (var item in util)
                                        {
                                            if (item.COD_UTILCONTO.Equals("03"))
                                            {
                                                nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                                nuovo1.ID_PERSONA = y.ID_PERSONA;
                                                nuovo1.DTA_INIZIO = DateTime.Today;
                                                nuovo1.DTA_FINE = fine;
                                                nuovo1.COD_IBAN = iban.COD_IBAN;
                                                nuovo1.COD_ABI = iban.COD_ABI;
                                                nuovo1.COD_CAB = iban.COD_CAB;

                                                nuovo1.COD_TIPOCONTO = "R";
                                                nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

                                                nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo1.COD_TERMID = "RAIPERME";

                                                nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                                nuovo1.COD_SUBTPCONTO = "ND";
                                                nuovo1.IND_CONGELATO = "N";
                                                nuovo1.IND_DELETE = "N";
                                                nuovo1.IND_VINCOLATO = "N";
                                                nuovo1.IND_CHANGED = "N";


                                                nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                                nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                                nuovo2.COD_UTILCONTO = "03";
                                                nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo2.COD_TERMID = "RAIPERME";

                                                nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                                db.XR_DATIBANCARI.Add(nuovo1);
                                                db.XR_UTILCONTO.Add(nuovo2);

                                            }
                                            else
                                            if (item.COD_UTILCONTO.Equals("02"))
                                            {
                                                nuovo4.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                                nuovo4.ID_PERSONA = y.ID_PERSONA;
                                                nuovo4.DTA_INIZIO = correnti.DTA_INIZIO;
                                                nuovo4.DTA_FINE = fine;
                                                nuovo4.COD_IBAN = correnti.COD_IBAN;
                                                nuovo4.COD_ABI = correnti.COD_ABI;
                                                nuovo4.COD_CAB = correnti.COD_CAB;

                                                nuovo4.COD_TIPOCONTO = "R";
                                                nuovo4.DES_INTESTATARIO = correnti.DES_INTESTATARIO;

                                                nuovo4.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo4.COD_TERMID = "RAIPERME";

                                                nuovo4.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                                nuovo4.COD_SUBTPCONTO = "ND";
                                                nuovo4.IND_CONGELATO = "N";
                                                nuovo4.IND_DELETE = "N";
                                                nuovo4.IND_VINCOLATO = "N";
                                                nuovo4.IND_CHANGED = "N";


                                                nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                                nuovo3.ID_XR_DATIBANCARI = nuovo4.ID_XR_DATIBANCARI;
                                                nuovo3.COD_UTILCONTO = "02";
                                                nuovo3.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo3.COD_TERMID = "RAIPERME";

                                                nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                                db.XR_DATIBANCARI.Add(nuovo4);
                                                db.XR_UTILCONTO.Add(nuovo3);

                                            }


                                        }

                                    }


                                    if (correnti.DTA_INIZIO.Equals(oggi))
                                    {


                                        foreach (var item in utilcorrente)
                                        {
                                            db.XR_UTILCONTO.Remove(item);
                                            db.SaveChanges();
                                        }

                                        db.XR_DATIBANCARI.Remove(correnti);
                                        db.SaveChanges();

                                    }


                                }
                            }





                        }






                    }
                    else

                    if (iban.COD_CMEVENTO.Equals("DELCC"))
                    {
                        //Caso Eliminazione Trasferte
                        if (iban.COD_UTILIZZO.Equals("02"))
                        {
                            var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);
                            var correnti = datibancari.First();

                            foreach (var item in datibancari)
                            {
                                if (item.DTA_FINE.Equals(fine))
                                {

                                    var esiste = db.XR_UTILCONTO.FirstOrDefault(x => x.ID_XR_DATIBANCARI == item.ID_XR_DATIBANCARI && x.COD_UTILCONTO.Equals("02"));

                                    if (esiste != null)

                                    {
                                        if (esiste.COD_UTILCONTO.Equals("02"))
                                        {
                                            correnti = item;
                                        }
                                    }

                                }
                            }




                            if (datibancari != null && datibancari.Count() > 0)
                            {
                                if (correnti.DTA_FINE.Equals(fine))
                                {
                                    var id_datibancari = correnti.ID_XR_DATIBANCARI;
                                    var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();

                                    //var recordcorrente = datibancari.First();

                                    var utilcorrente = db.XR_UTILCONTO.Where(s => s.ID_XR_DATIBANCARI == correnti.ID_XR_DATIBANCARI).ToList();

                                    if (!datibancari.First().DTA_INIZIO.Equals(oggi))
                                    {
                                    datibancari.First().DTA_FINE = ieri;
                                    }

                                    if (util != null && util.Count() > 0)
                                    {
                                        foreach (var item in util)
                                        {

                                            if (item.COD_UTILCONTO.Equals("03"))
                                            {
                                                nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                                nuovo1.ID_PERSONA = y.ID_PERSONA;
                                                nuovo1.DTA_INIZIO = correnti.DTA_INIZIO;
                                                nuovo1.DTA_FINE = fine;
                                                nuovo1.COD_IBAN = correnti.COD_IBAN;
                                                nuovo1.COD_ABI = correnti.COD_ABI;
                                                nuovo1.COD_CAB = correnti.COD_CAB;

                                                nuovo1.COD_TIPOCONTO = "R";
                                                nuovo1.DES_INTESTATARIO = correnti.DES_INTESTATARIO;

                                                nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo1.COD_TERMID = "RAIPERME";

                                                nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                                nuovo1.COD_SUBTPCONTO = "ND";
                                                nuovo1.IND_CONGELATO = "N";
                                                nuovo1.IND_DELETE = "N";
                                                nuovo1.IND_VINCOLATO = "N";
                                                nuovo1.IND_CHANGED = "N";


                                                nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                                nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                                nuovo2.COD_UTILCONTO = "03";
                                                nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo2.COD_TERMID = "RAIPERME";

                                                nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                                db.XR_DATIBANCARI.Add(nuovo1);
                                                db.XR_UTILCONTO.Add(nuovo2);

                                            }


                                        }

                                    }

                                    if (correnti.DTA_INIZIO.Equals(oggi))
                                    {


                                        foreach (var item in utilcorrente)
                                        {
                                            db.XR_UTILCONTO.Remove(item);
                                            db.SaveChanges();
                                        }

                                        db.XR_DATIBANCARI.Remove(correnti);
                                        db.SaveChanges();

                                    }

                                }
                            }





                        }
                        else
                            //Caso Elimina Spese di Prod
                            if (iban.COD_UTILIZZO.Equals("03"))
                        {
                            var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);
                            var correnti = datibancari.First();

                            foreach (var item in datibancari)
                            {
                                if (item.DTA_FINE.Equals(fine))
                                {

                                    var esiste = db.XR_UTILCONTO.FirstOrDefault(x => x.ID_XR_DATIBANCARI == item.ID_XR_DATIBANCARI && x.COD_UTILCONTO.Equals("03"));

                                    if (esiste != null)

                                    {
                                        if (esiste.COD_UTILCONTO.Equals("03"))
                                        {
                                            correnti = item;
                                        }
                                    }

                                }
                            }


                            if (datibancari != null && datibancari.Count() > 0)
                            {
                                if (correnti.DTA_FINE.Equals(fine))
                                {
                                    var id_datibancari = correnti.ID_XR_DATIBANCARI;
                                    var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();


                                    //var recordcorrente = datibancari.First();

                                    var utilcorrente = db.XR_UTILCONTO.Where(s => s.ID_XR_DATIBANCARI == correnti.ID_XR_DATIBANCARI).ToList();

                                    if (!datibancari.First().DTA_INIZIO.Equals(oggi))
                                    {
                                    datibancari.First().DTA_FINE = ieri;
                                    }

                                    if (util != null && util.Count() > 0)
                                    {
                                        foreach (var item in util)
                                        {

                                            if (item.COD_UTILCONTO.Equals("02"))
                                            {
                                                nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                                nuovo1.ID_PERSONA = y.ID_PERSONA;
                                                nuovo1.DTA_INIZIO = correnti.DTA_INIZIO;
                                                nuovo1.DTA_FINE = fine;
                                                nuovo1.COD_IBAN = correnti.COD_IBAN;
                                                nuovo1.COD_ABI = correnti.COD_ABI;
                                                nuovo1.COD_CAB = correnti.COD_CAB;

                                                nuovo1.COD_TIPOCONTO = "R";
                                                nuovo1.DES_INTESTATARIO = correnti.DES_INTESTATARIO;

                                                nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo1.COD_TERMID = "RAIPERME";

                                                nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                                nuovo1.COD_SUBTPCONTO = "ND";
                                                nuovo1.IND_CONGELATO = "N";
                                                nuovo1.IND_DELETE = "N";
                                                nuovo1.IND_VINCOLATO = "N";
                                                nuovo1.IND_CHANGED = "N";


                                                nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                                nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                                nuovo2.COD_UTILCONTO = "02";
                                                nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo2.COD_TERMID = "RAIPERME";

                                                nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                                db.XR_DATIBANCARI.Add(nuovo1);
                                                db.XR_UTILCONTO.Add(nuovo2);

                                            }


                                        }

                                    }

                                    if (correnti.DTA_INIZIO.Equals(oggi))
                                    {


                                        foreach (var item in utilcorrente)
                                        {
                                            db.XR_UTILCONTO.Remove(item);
                                            db.SaveChanges();
                                        }

                                        db.XR_DATIBANCARI.Remove(correnti);
                                        db.SaveChanges();

                                    }


                                }
                            }





                        }


                    }
                    else

                      if (iban.COD_CMEVENTO.Equals("INSCC"))
                    {
                        //caso stipendio
                        if (iban.COD_UTILIZZO.Equals("01")) {

                            //verifica se il record anagrafico esiste
                            if (y == null)
                            {
                                return new JsonResult
                                {
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                                    Data = new { result = "NO" }
                                };
                            }
                            else
                            {




                                nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                nuovo1.ID_PERSONA = y.ID_PERSONA;
                                nuovo1.DTA_INIZIO = DateTime.Today;
                                nuovo1.DTA_FINE = fine;
                                nuovo1.COD_IBAN = iban.COD_IBAN;
                                nuovo1.COD_ABI = iban.COD_ABI;
                                nuovo1.COD_CAB = iban.COD_CAB;

                                nuovo1.COD_TIPOCONTO = "C";
                                nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

                                nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                nuovo1.COD_TERMID = "RAIPERME";

                                nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                nuovo1.COD_SUBTPCONTO = "ND";
                                nuovo1.IND_CONGELATO = "N";
                                nuovo1.IND_DELETE = "N";
                                nuovo1.IND_VINCOLATO = "N";
                                nuovo1.IND_CHANGED = "N";


                                nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                nuovo2.COD_UTILCONTO = "01";
                                nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                nuovo2.COD_TERMID = "RAIPERME";

                                nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                db.XR_DATIBANCARI.Add(nuovo1);
                                db.XR_UTILCONTO.Add(nuovo2);
                                vincolo = "<p> Si ricorda che, in caso di conto <b>vincolato</b>, è propria responsabilità consegnare la modulistica necessaria all'ufficio di competenza per le coordinate bancarie dichiarate per l'accredito dello stipendio.</p>";


                            }


                        }
                        else
                        //Caso tutti gli anticipi
                        if (iban.COD_UTILIZZO.Equals("04"))
                        {
                            var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);

                            if (datibancari != null && datibancari.Count() > 0)
                            {
                                //Ho già lo stesso IBAN per le Spese di Prod, quindi chiudo tutto e lo riapro per entrambi
                                if (datibancari.First().DTA_FINE.Equals(fine) && datibancari.First().COD_IBAN.Equals(iban.COD_IBAN))
                                {
                                    var id_datibancari = datibancari.First().ID_XR_DATIBANCARI;
                                    var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();
                                    datibancari.First().DTA_FINE = ieri;

                                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                    nuovo1.ID_PERSONA = y.ID_PERSONA;
                                    nuovo1.DTA_INIZIO = DateTime.Today;
                                    nuovo1.DTA_FINE = fine;
                                    nuovo1.COD_IBAN = iban.COD_IBAN;
                                    nuovo1.COD_ABI = iban.COD_ABI;
                                    nuovo1.COD_CAB = iban.COD_CAB;

                                    nuovo1.COD_TIPOCONTO = "R";
                                    nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

                                    nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                    nuovo1.COD_TERMID = "RAIPERME";

                                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                    nuovo1.COD_SUBTPCONTO = "ND";
                                    nuovo1.IND_CONGELATO = "N";
                                    nuovo1.IND_DELETE = "N";
                                    nuovo1.IND_VINCOLATO = "N";
                                    nuovo1.IND_CHANGED = "N";


                                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                    nuovo2.COD_UTILCONTO = "02";
                                    nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                    nuovo2.COD_TERMID = "RAIPERME";

                                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                    nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                    nuovo3.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                    nuovo3.COD_UTILCONTO = "03";
                                    nuovo3.TMS_TIMESTAMP = DateTime.Now;
                                    nuovo3.COD_TERMID = "RAIPERME";

                                    nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                    db.XR_DATIBANCARI.Add(nuovo1);
                                    db.XR_UTILCONTO.Add(nuovo2);
                                    db.XR_UTILCONTO.Add(nuovo3);


                                }
                                else
                                {

                                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                    nuovo1.ID_PERSONA = y.ID_PERSONA;
                                    nuovo1.DTA_INIZIO = DateTime.Today;
                                    nuovo1.DTA_FINE = fine;
                                    nuovo1.COD_IBAN = iban.COD_IBAN;
                                    nuovo1.COD_ABI = iban.COD_ABI;
                                    nuovo1.COD_CAB = iban.COD_CAB;

                                    nuovo1.COD_TIPOCONTO = "R";
                                    nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

                                    nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                    nuovo1.COD_TERMID = "RAIPERME";

                                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                    nuovo1.COD_SUBTPCONTO = "ND";
                                    nuovo1.IND_CONGELATO = "N";
                                    nuovo1.IND_DELETE = "N";
                                    nuovo1.IND_VINCOLATO = "N";
                                    nuovo1.IND_CHANGED = "N";


                                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                    nuovo2.COD_UTILCONTO = "02";
                                    nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                    nuovo2.COD_TERMID = "RAIPERME";

                                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                    nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                    nuovo3.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                    nuovo3.COD_UTILCONTO = "03";
                                    nuovo3.TMS_TIMESTAMP = DateTime.Now;
                                    nuovo3.COD_TERMID = "RAIPERME";

                                    nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                    db.XR_DATIBANCARI.Add(nuovo1);
                                    db.XR_UTILCONTO.Add(nuovo2);
                                    db.XR_UTILCONTO.Add(nuovo3);



                                }
                            }
                            else
                            {

                                nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                nuovo1.ID_PERSONA = y.ID_PERSONA;
                                nuovo1.DTA_INIZIO = DateTime.Today;
                                nuovo1.DTA_FINE = fine;
                                nuovo1.COD_IBAN = iban.COD_IBAN;
                                nuovo1.COD_ABI = iban.COD_ABI;
                                nuovo1.COD_CAB = iban.COD_CAB;

                                nuovo1.COD_TIPOCONTO = "R";
                                nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

                                nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                nuovo1.COD_TERMID = "RAIPERME";

                                nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                nuovo1.COD_SUBTPCONTO = "ND";
                                nuovo1.IND_CONGELATO = "N";
                                nuovo1.IND_DELETE = "N";
                                nuovo1.IND_VINCOLATO = "N";
                                nuovo1.IND_CHANGED = "N";


                                nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                nuovo2.COD_UTILCONTO = "02";
                                nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                nuovo2.COD_TERMID = "RAIPERME";

                                nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                nuovo3.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                nuovo3.COD_UTILCONTO = "03";
                                nuovo3.TMS_TIMESTAMP = DateTime.Now;
                                nuovo3.COD_TERMID = "RAIPERME";

                                nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                db.XR_DATIBANCARI.Add(nuovo1);
                                db.XR_UTILCONTO.Add(nuovo2);
                                db.XR_UTILCONTO.Add(nuovo3);

                            }





                        }
                        else
                        //Caso Inserimento Trasferte
                        if (iban.COD_UTILIZZO.Equals("02"))
                        {
                            var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);

                            if (datibancari != null && datibancari.Count() > 0)
                            {
                                //Ho già lo stesso IBAN per le Spese di Prod, quindi chiudo tutto e lo riapro per entrambi
                                if (datibancari.First().DTA_FINE.Equals(fine) && datibancari.First().COD_IBAN.Equals(iban.COD_IBAN))
                                {
                                    var id_datibancari = datibancari.First().ID_XR_DATIBANCARI;
                                    var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();

                                    var recordcorrente = datibancari.First();

                                    var utilcorrente = db.XR_UTILCONTO.Where(s => s.ID_XR_DATIBANCARI == recordcorrente.ID_XR_DATIBANCARI).ToList();

                                    if (!datibancari.First().DTA_INIZIO.Equals(oggi))
                                    {
                                    datibancari.First().DTA_FINE = ieri;
                                    }

                                    if (util != null && util.Count() > 0)
                                    {
                                        foreach (var item in util)
                                        {

                                            if (item.COD_UTILCONTO.Equals("03"))
                                            {
                                                nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                                nuovo1.ID_PERSONA = y.ID_PERSONA;
                                                nuovo1.DTA_INIZIO = DateTime.Today;
                                                nuovo1.DTA_FINE = fine;
                                                nuovo1.COD_IBAN = datibancari.First().COD_IBAN;
                                                nuovo1.COD_ABI = datibancari.First().COD_ABI;
                                                nuovo1.COD_CAB = datibancari.First().COD_CAB;

                                                nuovo1.COD_TIPOCONTO = "R";
                                                nuovo1.DES_INTESTATARIO = datibancari.First().DES_INTESTATARIO;

                                                nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo1.COD_TERMID = "RAIPERME";

                                                nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                                nuovo1.COD_SUBTPCONTO = "ND";
                                                nuovo1.IND_CONGELATO = "N";
                                                nuovo1.IND_DELETE = "N";
                                                nuovo1.IND_VINCOLATO = "N";
                                                nuovo1.IND_CHANGED = "N";


                                                nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                                nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                                nuovo2.COD_UTILCONTO = "03";
                                                nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo2.COD_TERMID = "RAIPERME";

                                                nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                                nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                                nuovo3.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                                nuovo3.COD_UTILCONTO = "02";
                                                nuovo3.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo3.COD_TERMID = "RAIPERME";

                                                nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                                db.XR_DATIBANCARI.Add(nuovo1);
                                                db.XR_UTILCONTO.Add(nuovo2);
                                                db.XR_UTILCONTO.Add(nuovo3);

                                            }


                                        }

                                    }


                                    if (recordcorrente.DTA_INIZIO.Equals(oggi))
                                    {


                                        foreach (var item in utilcorrente)
                                        {
                                            db.XR_UTILCONTO.Remove(item);
                                            db.SaveChanges();
                                        }

                                        db.XR_DATIBANCARI.Remove(recordcorrente);
                                        db.SaveChanges();

                                    }




                                }
                                else
                                {

                                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                    nuovo1.ID_PERSONA = y.ID_PERSONA;
                                    nuovo1.DTA_INIZIO = DateTime.Today;
                                    nuovo1.DTA_FINE = fine;
                                    nuovo1.COD_IBAN = iban.COD_IBAN;
                                    nuovo1.COD_ABI = iban.COD_ABI;
                                    nuovo1.COD_CAB = iban.COD_CAB;

                                    nuovo1.COD_TIPOCONTO = "R";
                                    nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

                                    nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                    nuovo1.COD_TERMID = "RAIPERME";

                                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                    nuovo1.COD_SUBTPCONTO = "ND";
                                    nuovo1.IND_CONGELATO = "N";
                                    nuovo1.IND_DELETE = "N";
                                    nuovo1.IND_VINCOLATO = "N";
                                    nuovo1.IND_CHANGED = "N";


                                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                    nuovo2.COD_UTILCONTO = "02";
                                    nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                    nuovo2.COD_TERMID = "RAIPERME";

                                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                    db.XR_DATIBANCARI.Add(nuovo1);
                                    db.XR_UTILCONTO.Add(nuovo2);



                                }
                            }
                            else
                            {

                                nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                nuovo1.ID_PERSONA = y.ID_PERSONA;
                                nuovo1.DTA_INIZIO = DateTime.Today;
                                nuovo1.DTA_FINE = fine;
                                nuovo1.COD_IBAN = iban.COD_IBAN;
                                nuovo1.COD_ABI = iban.COD_ABI;
                                nuovo1.COD_CAB = iban.COD_CAB;

                                nuovo1.COD_TIPOCONTO = "R";
                                nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

                                nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                nuovo1.COD_TERMID = "RAIPERME";

                                nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                nuovo1.COD_SUBTPCONTO = "ND";
                                nuovo1.IND_CONGELATO = "N";
                                nuovo1.IND_DELETE = "N";
                                nuovo1.IND_VINCOLATO = "N";
                                nuovo1.IND_CHANGED = "N";


                                nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                nuovo2.COD_UTILCONTO = "02";
                                nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                nuovo2.COD_TERMID = "RAIPERME";

                                nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                db.XR_DATIBANCARI.Add(nuovo1);
                                db.XR_UTILCONTO.Add(nuovo2);

                            }





                        }
                        else
                        //inserimento Spese Prod 
                        if (iban.COD_UTILIZZO.Equals("03"))
                        {
                            var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA && x.COD_TIPOCONTO.Equals("R")).ToList().OrderByDescending(c => c.DTA_FINE);

                            if (datibancari != null && datibancari.Count() > 0)
                            {
                                //Ho già lo stesso IBAN per le Trasferte, quindi chiudo tutto e lo riapro per entrambi
                                if (datibancari.First().DTA_FINE.Equals(fine) && datibancari.First().COD_IBAN.Equals(iban.COD_IBAN))
                                {
                                    var id_datibancari = datibancari.First().ID_XR_DATIBANCARI;
                                    var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id_datibancari).ToList();

                                    var recordcorrente = datibancari.First();

                                    var utilcorrente = db.XR_UTILCONTO.Where(s => s.ID_XR_DATIBANCARI == recordcorrente.ID_XR_DATIBANCARI).ToList();

                                    if (!datibancari.First().DTA_INIZIO.Equals(oggi))
                                    {
                                    datibancari.First().DTA_FINE = ieri;
                                    }


                                    if (util != null && util.Count() > 0)
                                    {
                                        foreach (var item in util)
                                        {

                                            if (item.COD_UTILCONTO.Equals("02"))
                                            {
                                                nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                                nuovo1.ID_PERSONA = y.ID_PERSONA;
                                                nuovo1.DTA_INIZIO = DateTime.Today;
                                                nuovo1.DTA_FINE = fine;
                                                nuovo1.COD_IBAN = datibancari.First().COD_IBAN;
                                                nuovo1.COD_ABI = datibancari.First().COD_ABI;
                                                nuovo1.COD_CAB = datibancari.First().COD_CAB;

                                                nuovo1.COD_TIPOCONTO = "R";
                                                nuovo1.DES_INTESTATARIO = datibancari.First().DES_INTESTATARIO;

                                                nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo1.COD_TERMID = "RAIPERME";

                                                nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                                nuovo1.COD_SUBTPCONTO = "ND";
                                                nuovo1.IND_CONGELATO = "N";
                                                nuovo1.IND_DELETE = "N";
                                                nuovo1.IND_VINCOLATO = "N";
                                                nuovo1.IND_CHANGED = "N";


                                                nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                                nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                                nuovo2.COD_UTILCONTO = "03";
                                                nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo2.COD_TERMID = "RAIPERME";

                                                nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                                nuovo3.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                                nuovo3.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                                nuovo3.COD_UTILCONTO = "02";
                                                nuovo3.TMS_TIMESTAMP = DateTime.Now;
                                                nuovo3.COD_TERMID = "RAIPERME";

                                                nuovo3.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                                db.XR_DATIBANCARI.Add(nuovo1);
                                                db.XR_UTILCONTO.Add(nuovo2);
                                                db.XR_UTILCONTO.Add(nuovo3);

                                            }


                                        }

                                    }



                                    if (recordcorrente.DTA_INIZIO.Equals(oggi))
                                    {


                                        foreach (var item in utilcorrente)
                                        {
                                            db.XR_UTILCONTO.Remove(item);
                                            db.SaveChanges();
                                        }

                                        db.XR_DATIBANCARI.Remove(recordcorrente);
                                        db.SaveChanges();

                                    }

                                }
                                else
                                {

                                    nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                    nuovo1.ID_PERSONA = y.ID_PERSONA;
                                    nuovo1.DTA_INIZIO = DateTime.Today;
                                    nuovo1.DTA_FINE = fine;
                                    nuovo1.COD_IBAN = iban.COD_IBAN;
                                    nuovo1.COD_ABI = iban.COD_ABI;
                                    nuovo1.COD_CAB = iban.COD_CAB;

                                    nuovo1.COD_TIPOCONTO = "R";
                                    nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

                                    nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                    nuovo1.COD_TERMID = "RAIPERME";

                                    nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                    nuovo1.COD_SUBTPCONTO = "ND";
                                    nuovo1.IND_CONGELATO = "N";
                                    nuovo1.IND_DELETE = "N";
                                    nuovo1.IND_VINCOLATO = "N";
                                    nuovo1.IND_CHANGED = "N";


                                    nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                    nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                    nuovo2.COD_UTILCONTO = "03";
                                    nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                    nuovo2.COD_TERMID = "RAIPERME";

                                    nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                    db.XR_DATIBANCARI.Add(nuovo1);
                                    db.XR_UTILCONTO.Add(nuovo2);



                                }
                            }
                            else
                            {

                                nuovo1.ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9);
                                nuovo1.ID_PERSONA = y.ID_PERSONA;
                                nuovo1.DTA_INIZIO = DateTime.Today;
                                nuovo1.DTA_FINE = fine;
                                nuovo1.COD_IBAN = iban.COD_IBAN;
                                nuovo1.COD_ABI = iban.COD_ABI;
                                nuovo1.COD_CAB = iban.COD_CAB;

                                nuovo1.COD_TIPOCONTO = "R";
                                nuovo1.DES_INTESTATARIO = iban.DES_INTESTATARIO;

                                nuovo1.TMS_TIMESTAMP = DateTime.Now;
                                nuovo1.COD_TERMID = "RAIPERME";

                                nuovo1.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                                nuovo1.COD_SUBTPCONTO = "ND";
                                nuovo1.IND_CONGELATO = "N";
                                nuovo1.IND_DELETE = "N";
                                nuovo1.IND_VINCOLATO = "N";
                                nuovo1.IND_CHANGED = "N";


                                nuovo2.ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9);

                                nuovo2.ID_XR_DATIBANCARI = nuovo1.ID_XR_DATIBANCARI;
                                nuovo2.COD_UTILCONTO = "03";
                                nuovo2.TMS_TIMESTAMP = DateTime.Now;
                                nuovo2.COD_TERMID = "RAIPERME";

                                nuovo2.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();

                                db.XR_DATIBANCARI.Add(nuovo1);
                                db.XR_UTILCONTO.Add(nuovo2);



                            }




                        }



                    }

                }


                try
                {
                    db.SaveChanges();

                    GestoreMail mail = new GestoreMail();
                    List<myRaiCommonTasks.sendMail.Attachement> attachments = null;
                    string dest = CommonTasks.GetEmailPerMatricola(matr);

                    if (String.IsNullOrWhiteSpace(dest))
                        dest = "P" + matr + "@rai.it";

                    string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271” > La richiesta di variazione dei dati bancari è stata convalidata</p>" + vincolo + " </br>";

                    var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                         "Conferma convalida variazione dati bancari",
                         dest,
                         "raiplace.selfservice@rai.it",
                         "Conferma di convalida IBAN",
                         "",
                         corpo,
                         null,
                         null,
                         attachments);


                    if (response != null && response.Errore != null)
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori()
                        {
                            applicativo = "Profilo personale",
                            data = DateTime.Now,
                            provenienza = "Convalida variazione IBAN codice",
                            error_message = response.Errore + " per " + dest,
                            matricola = "000000"
                        };

                        using (digiGappEntities db2 = new digiGappEntities())
                        {
                            db2.MyRai_LogErrori.Add(err);
                            db2.SaveChanges();
                        }
                    }



                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "ok" }
                    };

                }
                catch (Exception ex)
                {
                    Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                    {
                        applicativo = "PORTALE",
                        data = DateTime.Now,
                        error_message = ex.ToString(),
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        provenienza = "ConvalidaModificaIBAN"
                    });
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = ex.Message }
                    };
                }


            }
        }



        public ActionResult ConvalidaModificheCellCodice(ProfiloPersonaleModel mod)
        {

            var db = ProfiloPersonaleManager.GetTalentiaDB();

            var cod = mod.codice.ID_CODICE_OTP;

            var verifica_codice = db.XR_SSV_CODICE_OTP.Find(cod);
            string operazione = "";

            if (verifica_codice == null || verifica_codice.DTA_SCADENZA.CompareTo(DateTime.Today) < 0 || !verifica_codice.COD_FUNZIONE.Equals("02") || !verifica_codice.MATRICOLA.Equals(CommonManager.GetCurrentUserMatricola()))
            {


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "Codice non valido o scaduto" }
                };


            }
            else
            {

                //String vincolo = "";

                var cell = db.CMINFOANAG_EXT.Find(verifica_codice.ID_EVENTO);
                DateTime fine = new DateTime(2999, 12, 31);


                DateTime ieri = DateTime.Today.AddDays(-1);
                DateTime oggi = DateTime.Today;
                XR_RECAPITI recapito = new XR_RECAPITI();
                String matr = CommonManager.GetCurrentUserMatricola();
                myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));



                if (cell == null || cell.COD_CONVALIDA_CC.Equals("1"))
                {


                    ProfiloPersonaleModel model2 = CreaProfiloPersonaleModel();
                    model2.RichiestaDaConvalidareFound = false;


                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "La richiesta da convalidare non è stata trovata" }
                    };


                }


                else

                {




                    cell.COD_CONVALIDA_CC = "1";
                    verifica_codice.IND_UTILIZZO = "1";
                    verifica_codice.DTA_UTILIZZO = DateTime.Now;
                    verifica_codice.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                    verifica_codice.COD_TERMID = "RAIPERME";

                    //Caso Modifica 

                    if (cell.COD_CMEVENTO.Equals("MODCEL"))
                    {
                        operazione = "modifica";
 
                            var datirecapiti = db.XR_RECAPITI.Where(x => x.ID_PERSONA == y.ID_PERSONA && (x.TIPO_RECAPITO.Equals("01") || x.TIPO_RECAPITO.Equals("02"))).ToList().OrderByDescending(c => c.DTA_FINE);

                            if (datirecapiti != null && datirecapiti.Count() > 0)
                            {
                                if (datirecapiti.First().DTA_FINE.Equals(fine))
                                {


                                    //var id_datirecapiti = datirecapiti.First().ID_XR_DATIBANCARI;



                                    //var util = db.XR_UTILCONTO.FirstOrDefault(x => x.ID_XR_DATIBANCARI == id_datibancari && x.COD_UTILCONTO.Equals("01"));

                                    var recordcorrente = datirecapiti.First();
                                   // var utilcorrente = db.XR_UTILCONTO.Where(s => s.ID_XR_DATIBANCARI == recordcorrente.ID_XR_DATIBANCARI).ToList();

                                    if (!datirecapiti.First().DTA_INIZIO.Equals(oggi))
                                    {
                                        datirecapiti.First().DTA_FINE = ieri;
                                    }


                                    recapito.ID_PERSONA = y.ID_PERSONA;
                                    recapito.ID_RECAPITI = db.XR_RECAPITI.GeneraPrimaryKey(9);
                                    recapito.DTA_INIZIO = DateTime.Today;
                                recapito.COD_CITTA ="00";

                                   
                                    
                                    recapito.DTA_FINE = fine;
                                    recapito.DES_PREFISSO = cell.DES_PREFISSO;
                                    recapito.DES_CELLULARE = cell.DES_CELLULARE;
                                    recapito.TIPO_RECAPITO = cell.TIPO_RECAPITO;
                 

                       

                                    recapito.TMS_TIMESTAMP = DateTime.Now;
                                    recapito.COD_TERMID = "RAIPERME";

                                    recapito.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                         


           

                                    db.XR_RECAPITI.Add(recapito);
                             

                                    if (recordcorrente.DTA_INIZIO.Equals(oggi))
                                    {


     

                                        db.XR_RECAPITI.Remove(recordcorrente);
                                        db.SaveChanges();

                                    }

                                    //vincolo = "<p> Si ricorda che, in caso di conto <b>vincolato</b>, è propria responsabilità consegnare la modulistica necessaria all'ufficio di competenza per le coordinate bancarie dichiarate per l'accredito dello stipendio.</p>";


                                }

                            }

                        




                    }
                    else

                    if (cell.COD_CMEVENTO.Equals("DELCEL"))
                    {
                        operazione = "cancellazione";
                       
                            var datirecapiti = db.XR_RECAPITI.Where(x => x.ID_PERSONA == y.ID_PERSONA && (x.TIPO_RECAPITO.Equals("01") || x.TIPO_RECAPITO.Equals("02") || x.TIPO_RECAPITO.Equals("00"))).ToList().OrderByDescending(c => c.DTA_FINE);
                            var correnti = datirecapiti.First();


                            if (datirecapiti != null && datirecapiti.Count() > 0)
                            {
                                if (correnti.DTA_FINE.Equals(fine) && correnti.DES_CELLULARE.Equals(cell.DES_CELLULARE))
                                {
                                    
                                  

                                    //var recordcorrente = datibancari.First();

                                    

                                    if (!datirecapiti.First().DTA_INIZIO.Equals(oggi))
                                    {
                                        datirecapiti.First().DTA_FINE = ieri;
                                    }

                                

                                    if (correnti.DTA_INIZIO.Equals(oggi))
                                    {


    

                                        db.XR_RECAPITI.Remove(correnti);
                                        db.SaveChanges();

                                    }

                                }
                            }





                        
   


                    }
                    else

                      if (cell.COD_CMEVENTO.Equals("INSCEL"))
                    {
                        operazione = "aggiunta";

                            if (y == null)
                            {
                                return new JsonResult
                                {
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                                    Data = new { result = "NO" }
                                };
                            }
                            else
                            {




                                recapito.ID_RECAPITI = db.XR_RECAPITI.GeneraPrimaryKey(9);
                                recapito.ID_PERSONA = y.ID_PERSONA;
                                recapito.DTA_INIZIO = DateTime.Today;
                                recapito.DTA_FINE = fine;


                                recapito.TIPO_RECAPITO = cell.TIPO_RECAPITO;
                                recapito.DES_PREFISSO = cell.DES_PREFISSO;
                                recapito.DES_CELLULARE = cell.DES_CELLULARE;
                            recapito.COD_CITTA = "00";



                            recapito.TMS_TIMESTAMP = DateTime.Now;
                                recapito.COD_TERMID = "RAIPERME";

                                recapito.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();




                                db.XR_RECAPITI.Add(recapito);
               
                                //vincolo = "<p> Si ricorda che, in caso di conto <b>vincolato</b>, è propria responsabilità consegnare la modulistica necessaria all'ufficio di competenza per le coordinate bancarie dichiarate per l'accredito dello stipendio.</p>";


                            }


                        




                    }

                }


                try
                {
                    db.SaveChanges();

                    GestoreMail mail = new GestoreMail();
                    List<myRaiCommonTasks.sendMail.Attachement> attachments = null;
                    string dest = CommonTasks.GetEmailPerMatricola(matr);

                    if (String.IsNullOrWhiteSpace(dest))
                        dest = "P" + matr + "@rai.it";

                    string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271” > La richiesta di "+""+ operazione+""+" del numero di cellulare è stata convalidata</p>";

                    var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                         "Conferma convalida " + "" + operazione + "" + " numero di cellulare",
                         dest,
                         "raiplace.selfservice@rai.it",
                         "Conferma di convalida numero di cellulare",
                         "",
                         corpo,
                         null,
                         null,
                         attachments);


                    if (response != null && response.Errore != null)
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori()
                        {
                            applicativo = "Profilo personale",
                            data = DateTime.Now,
                            provenienza = "Convalida variazione cellulare codice",
                            error_message = response.Errore + " per " + dest,
                            matricola = "000000"
                        };

                        using (digiGappEntities db2 = new digiGappEntities())
                        {
                            db2.MyRai_LogErrori.Add(err);
                            db2.SaveChanges();
                        }
                    }



                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "ok" }
                    };

                }
                catch (Exception ex)
                {
                    Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                    {
                        applicativo = "PORTALE",
                        data = DateTime.Now,
                        error_message = ex.ToString(),
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        provenienza = "ConvalidaModificaCellulare"
                    });
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = ex.Message }
                    };
                }


            }
        }

        public ActionResult ConvalidaModificheDomicilioCodice(ProfiloPersonaleModel mod)
        {

            var db = ProfiloPersonaleManager.GetTalentiaDB();

            var cod = mod.codice.ID_CODICE_OTP;

            var verifica_codice = db.XR_SSV_CODICE_OTP.Find(cod);
            string operazione = "";

            if (verifica_codice == null || verifica_codice.DTA_SCADENZA.CompareTo(DateTime.Today) < 0 || !verifica_codice.COD_FUNZIONE.Equals("03") || !verifica_codice.MATRICOLA.Equals(CommonManager.GetCurrentUserMatricola()))
            {


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "Codice non valido o scaduto" }
                };


            }
            else
            {

                //String vincolo = "";

                var domicilio = db.CMINFOANAG_EXT.Find(verifica_codice.ID_EVENTO);
                //DateTime fine = new DateTime(2999, 12, 31);


                //DateTime ieri = DateTime.Today.AddDays(-1);
                //DateTime oggi = DateTime.Today;
                //XR_RECAPITI recapito = new XR_RECAPITI();
                String matr = CommonManager.GetCurrentUserMatricola();
                myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));




                if (domicilio == null || domicilio.COD_CONVALIDA_CC.Equals("1"))
                {


                    ProfiloPersonaleModel model2 = CreaProfiloPersonaleModel();
                    model2.RichiestaDaConvalidareFound = false;


                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "La richiesta da convalidare non è stata trovata" }
                    };


                }


                else

                {




                    domicilio.COD_CONVALIDA_CC = "1";
                    domicilio.DTA_CONVALIDA = DateTime.Now;
                    verifica_codice.IND_UTILIZZO = "1";
                    verifica_codice.DTA_UTILIZZO = domicilio.DTA_CONVALIDA.Value;// DateTime.Now;
                    verifica_codice.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();
                    verifica_codice.COD_TERMID = "RAIPERME";

                    //Caso Modifica 

                    //if (domicilio.COD_CMEVENTO.Equals("MODDOM"))
                    //{
                    //    operazione = "modifica";

                    //    //var datirecapiti = db.XR_RECAPITI.Where(x => x.ID_PERSONA == y.ID_PERSONA && (x.TIPO_RECAPITO.Equals("01") || x.TIPO_RECAPITO.Equals("02"))).ToList().OrderByDescending(c => c.DTA_FINE);

                    //    var domicilioOld = db.ANAGPERS.FirstOrDefault(x => x.ID_PERSONA == y.ID_PERSONA);

                    //    if (domicilioOld != null)
                    //    {




                    //        domicilioOld.COD_CITTADOM = domicilio.COD_CITTADOM;
                    //        domicilioOld.CAP_CAPDOM = domicilio.CAP_CAPDOM;
                    //        domicilioOld.DES_INDIRDOM = domicilio.DES_INDIRDOM;
                    //        //try
                    //        //{
                    //        //domicilio.IND_CICS_SENT = ProfiloPersonaleManager.Tracciato_Domicilio(matr, domicilioOld, null);
                    //        domicilio.IND_CICS_SENT = ProfiloPersonaleManager.Tracciato_Domicilio(matr, Models.Gestionale.TipoAnaVar.Domicilio, domicilioOld, null, domicilio.ID_EVENTO, domicilio.TMS_TIMESTAMP.Value);

                    //        //}
                    //        //catch(Exception ex)
                    //        //{
                    //        //    domicilio.IND_CICS_SENT = false;
                    //        //}






                    //    }






                    //    else
                    //    {
                    //        return new JsonResult
                    //        {
                    //            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    //            Data = new { result = "Attenzione: record anagrafico inesistente per questa matricola" }
                    //        };
                    //    }
                    //}
                    
                    //else



                    //  if (domicilio.COD_CMEVENTO.Equals("INSDOM"))
                    //{
                    //    operazione = "aggiunta";

                    //    if (y == null)
                    //    {
                    //        return new JsonResult
                    //        {
                    //            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    //            Data = new { result = "NO" }
                    //        };
                    //    }
                    //    else
                    //    {




                    //        recapito.ID_RECAPITI = db.XR_RECAPITI.GeneraPrimaryKey(9);
                    //        recapito.ID_PERSONA = y.ID_PERSONA;
                    //        recapito.DTA_INIZIO = DateTime.Today;
                    //        recapito.DTA_FINE = fine;


                    //        recapito.TIPO_RECAPITO = cell.TIPO_RECAPITO;
                    //        recapito.DES_PREFISSO = cell.DES_PREFISSO;
                    //        recapito.DES_CELLULARE = cell.DES_CELLULARE;
                    //        recapito.COD_CITTA = "00";



                    //        recapito.TMS_TIMESTAMP = DateTime.Now;
                    //        recapito.COD_TERMID = "RAIPERME";

                    //        recapito.COD_USER = "P" + CommonManager.GetCurrentUserMatricola();




                    //        db.XR_RECAPITI.Add(recapito);

                    //        vincolo = "<p> Si ricorda che, in caso di conto <b>vincolato</b>, è propria responsabilità consegnare la modulistica necessaria all'ufficio di competenza per le coordinate bancarie dichiarate per l'accredito dello stipendio.</p>";


                    //    }







                    //}

                }


                try
                {
                    db.SaveChanges();

                    GestoreMail mail = new GestoreMail();
                    List<myRaiCommonTasks.sendMail.Attachement> attachments = null;
                    string dest = CommonTasks.GetEmailPerMatricola(matr);

                    if (String.IsNullOrWhiteSpace(dest))
                        dest = "P" + matr + "@rai.it";

                    string corpo = "<p style=”font-size:22px; text-align:center; color:#88e271” > La richiesta di " + "" + operazione + "" + " del domicilio è stata convalidata</p><p>La variazione del domicilio avrà effetto dal giorno successivo a quello della convalida.</p><p>Si ricorda che, in caso di malattia, ove il domicilio di reperibilità non dovesse coincidere con quello di residenza, il lavoratore è tenuto ad indicarlo al medico, il quale provvederà ad inserirlo nella certificazione di malattia.</p><p>In caso di successivi mutamenti del domicilio di reperibilità, il lavoratore dovrà darne tempestiva comunicazione al proprio Ufficio del Personale entro le ore 11 .00 della giornata in cui intende modificare il domicilio di reperibilità.</p>";

                    var response = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                         "Conferma convalida " + "" + operazione + "" + " domicilio",
                         dest,
                         "raiplace.selfservice@rai.it",
                         "Conferma di convalida domicilio",
                         "",
                         corpo,
                         null,
                         null,
                         attachments);


                    if (response != null && response.Errore != null)
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori()
                        {
                            applicativo = "Profilo personale",
                            data = DateTime.Now,
                            provenienza = "Convalida variazione domicilio codice",
                            error_message = response.Errore + " per " + dest,
                            matricola = "000000"
                        };

                        using (digiGappEntities db2 = new digiGappEntities())
                        {
                            db2.MyRai_LogErrori.Add(err);
                            db2.SaveChanges();
                        }
                    }



                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "ok" }
                    };

                }
                catch (Exception ex)
                {
                    Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                    {
                        applicativo = "PORTALE",
                        data = DateTime.Now,
                        error_message = ex.ToString(),
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        provenienza = "ConvalidaModificaCellulare"
                    });
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = ex.Message }
                    };
                }


            }
        }


        public ActionResult CaricaNumeriFile()
        {
            List<string> matricolenonscritte = new List<string>();
            var db = ProfiloPersonaleManager.GetTalentiaDB();

            string fileName = @"\\nasrm3\PFOAC\CENNERAZZO\Numeri\prefisso-apice-spazio.xlsx";
            var workbook = new XLWorkbook(fileName);
            var ws1 = workbook.Worksheet(1);

            DateTime fine = new DateTime(2999, 12, 31);


            
            DateTime oggi = DateTime.Today;
            


            var nonEmptyDataRows = ws1.RowsUsed();

            foreach (var dataRow in nonEmptyDataRows)
            {

                var recapito = new XR_RECAPITI();

                var pmatricola = dataRow.Cell(1).Value.ToString();
                    var matricola = pmatricola.Substring(1);
                    var apicenumero = dataRow.Cell(2).Value.ToString();
                    var numero = apicenumero.Substring(6);

                myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matricola));

                
                recapito.ID_RECAPITI = db.XR_RECAPITI.GeneraPrimaryKey(9);

                if (y != null) { recapito.ID_PERSONA = y.ID_PERSONA; }
                else { matricolenonscritte.Add(matricola); }
                
                recapito.DTA_INIZIO = DateTime.Today;
                recapito.DTA_FINE = fine;


                recapito.TIPO_RECAPITO = "00";
                recapito.DES_PREFISSO = "0039";
                recapito.DES_CELLULARE = numero;
                recapito.COD_CITTA = "00";



                recapito.TMS_TIMESTAMP = DateTime.Now;
                recapito.COD_TERMID = "RAIPERME";

                recapito.COD_USER = "P" + matricola;


                db.XR_RECAPITI.Add(recapito);

                try {
                    db.SaveChanges();
                }
                catch(Exception ex)
                {
                    matricolenonscritte.Add(matricola + "-"+ ex.Message);
                }

            }

            using (System.IO.StreamWriter file =
       new System.IO.StreamWriter(@"\\nasrm3\PFOAC\CENNERAZZO\Numeri\prefisso-apice-spazio.txt"))
            {
                foreach (string line in matricolenonscritte)
                {
              
                        file.WriteLine(line);
                    

                }
            }


            try
            {
                db.SaveChanges();

                


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "ok" }
                };
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "CaricaNumeriFile"
                });

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = ex.Message }
                };
            }

        }

        public ActionResult GetAnagBancaOnline(string iban)
        {
            var db = ProfiloPersonaleManager.GetTalentiaDB();

            if (iban.Length == 27)
            {
                var codPaese = iban.Substring(0, 2);
                var codChk = iban.Substring(2, 2);
                var codCin = iban.Substring(4, 1);
                var codAbi = iban.Substring(5, 5);
                var codCab = iban.Substring(10, 5);
                var codCC = iban.Substring(15);

                myRaiData.Incentivi.XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.Include("TB_COMUNE").FirstOrDefault(x => x.COD_ABI == codAbi && x.COD_CAB == codCab);

                if (anagBanca != null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result="ok", agenzia=anagBanca.DES_RAG_SOCIALE, indirizzo=anagBanca.DES_INDIRIZZO, citta= anagBanca.TB_COMUNE!=null?anagBanca.TB_COMUNE.DES_CITTA:"" }
                    };
                }
                else
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "No" }
                    };
                }
            }
            else

            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "No" }
                };
            }






        }

        public ActionResult GetComuni(string filter, string value)
        {
            return Json(ProfiloPersonaleManager.GetComuni(filter, value), JsonRequestBehavior.AllowGet);
        }


        public static List<ListItem> getScelteTipologia(int flag)
        {
            List<ListItem> lista = new List<ListItem>();


            //ListItem listItem = new ListItem()
            //    {
            //        Value = "01",
            //        Text = "Accredito Stipendio",
            //        Selected = false
            //    };
            //    lista.Add(listItem);

            if(flag == 0) { 
                ListItem listItem2 = new ListItem()
                {
                    Value = "02",
                    Text = "Anticipi Trasferte",
                    Selected = false
                };
                lista.Add(listItem2);

                ListItem listItem3 = new ListItem()
                {
                    Value = "03",
                    Text = "Anticipi Spese di produzione",
                    Selected = false
                };
                lista.Add(listItem3);

                ListItem listItem4 = new ListItem()
                {
                    Value = "04",
                    Text = "Tutti gli anticipi",
                    Selected = false
                };
                lista.Add(listItem4);




            } else
            if (flag == 1)

            {

                ListItem listItem3 = new ListItem()
                {
                    Value = "03",
                    Text = "Anticipi Spese di produzione",
                    Selected = false
                };
                lista.Add(listItem3);

            } else
            if (flag == 2)
            {
                ListItem listItem2 = new ListItem()
                {
                    Value = "02",
                    Text = "Anticipi Trasferte",
                    Selected = false
                };
                lista.Add(listItem2);
            }




            return lista;
        }


        public ActionResult TemaUpdate(string nuovoTema)
        {
            string errore = ProfiloPersonaleManager.TemaUpdate(nuovoTema);
            if (errore == null)
                return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { result = "OK" } };
            else
                return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { result = errore } };
        }
    }
}