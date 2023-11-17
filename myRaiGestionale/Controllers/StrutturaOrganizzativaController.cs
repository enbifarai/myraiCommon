using myRaiCommonManager;
using myRaiCommonManager.Cessazione;
using myRaiCommonModel.Gestionale;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using ClosedXML.Excel;
using myRaiDataTalentia;
using myRaiData;
using Newtonsoft.Json;
using System.Globalization;

namespace myRaiGestionale.Controllers
{
    public class StrutturaOrganizzativaController : BaseCommonController
    {
        public StrutturaOrganizzativaController()
        {

        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!IncarichiHelper.EnabledToIncarichi(CommonHelper.GetCurrentUserMatricola()))
            {
                if (filterContext.ActionDescriptor.ActionName != null &&
                    filterContext.ActionDescriptor.ActionName.ToLower() == "getincarichiallext")
                {
                    return;
                }
                filterContext.Result = new RedirectResult("/Home/notAuth");
                return;
            }

            SessionHelper.Set("GEST_SECTION", "ORGANIZZAZIONE");

            HttpCookie c = HttpContext.Request.Cookies["db_orig"];
            if (c != null)
                IncarichiManager.ChangeDB(c.Value);
            else
                IncarichiManager.ChangeDB("P");

            base.OnActionExecuting(filterContext);
        }

        [HttpPost]
        public ActionResult SaveProcesso(SaveProcessoModel model)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            DateTime D = new DateTime(9999, 12, 31);




            if (String.IsNullOrWhiteSpace(model.codiceprocesso) || model.codiceprocesso.Trim().EndsWith("."))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, error = "Codice processo non valido." }
                };
            }
            int livelloPadre = GetLivello(model.idprocessopadre);

            if (model.idprocesso == 0)
            {
                if (db.XR_STR_PROCESSI.Any(x => x.codice_processo == model.codiceprocesso))
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, error = "Codice processo già esistente." }
                    };
                }
            }
            else
            {
                if (db.XR_STR_PROCESSI.Any(x => x.codice_processo == model.codiceprocesso && x.id != model.idprocesso))
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, error = "Codice processo già esistente." }
                    };
                }
            }
            if (String.IsNullOrWhiteSpace(model.codiceprocesso) || String.IsNullOrWhiteSpace(model.nomeprocesso))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, error = "Mancano dati obbligatori" }
                };
            }
            if (model.Documenti != null && model.Documenti.Any())
            {
                if (model.Documenti.Any(x => String.IsNullOrWhiteSpace(x.Nome)))
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, error = "Non è stato specificato il nome file di un documento." }
                    };
                }
            }
            if (livelloPadre > 0 && (model.Documenti == null || !model.Documenti.Any(x => x.Tipo == TipoDocumento.ProcessIdentity)))
            {
                //return new JsonResult
                //{
                //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                //    Data = new { esito = false, error = "Il documento Process Identity Card è obbligatorio per tutti i processi." }
                //};
            }
            if (model.idprocesso == 0)
            {
                if (livelloPadre == 1 && (model.Documenti == null || !model.Documenti.Any(x => x.Tipo == TipoDocumento.ProcessMap)))
                {
                    //return new JsonResult
                    //{
                    //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    //    Data = new { esito = false, error = "Il documento Process Map è obbligatorio per i processi di secondo livello." }
                    //};
                }
            }
            else
            {
                int livello = GetLivello(model.idprocesso);
                if (livello == 2 && (model.Documenti == null || !model.Documenti.Any(x => x.Tipo == TipoDocumento.ProcessMap)))
                {
                    //return new JsonResult
                    //{
                    //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    //    Data = new { esito = false, error = "Il documento Process Map è obbligatorio per i processi di secondo livello." }
                    //};
                }
            }

            if (livelloPadre > 0 && model.idprocesso != 0 && !HaFigli(model.idprocesso) &&
                (model.Documenti == null || !model.Documenti.Any(x => x.Tipo == TipoDocumento.FlowChart))
                )
            {
                //return new JsonResult
                //{
                //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                //    Data = new { esito = false, error = "Il documento Folwchart è obbligatorio per i processi che non hanno figli." }
                //};
            }
            if (model.idprocesso == 0)
            {
                int idmax = db.XR_STR_PROCESSI.Select(x => x.id).OrderByDescending(z => z).FirstOrDefault();
                myRaiDataTalentia.XR_STR_PROCESSI p = new myRaiDataTalentia.XR_STR_PROCESSI();
                p.id = idmax + 1;
                p.nome_processo = model.nomeprocesso;
                p.descrizione_processo = model.descrizioneprocesso;
                p.codice_processo = model.codiceprocesso;
                p.data_inizio_validita = DateTime.Today;
                p.data_fine_validita = new DateTime(9999, 12, 31);
                db.XR_STR_PROCESSI.Add(p);

                myRaiDataTalentia.XR_STR_TALBERO_GEN a = new myRaiDataTalentia.XR_STR_TALBERO_GEN();
                a.tipo_oggetto = "P";
                a.id = idmax + 1;
                a.subordinato_a = model.idprocessopadre;
                db.XR_STR_TALBERO_GEN.Add(a);

                if (model.Documenti != null && model.Documenti.Any())
                {
                    SalvaDocumenti(p.id, model, db);
                }

                if (model.sistemaIT != null && model.sistemaIT.Any())
                {
                    foreach (int idSistema in model.sistemaIT)
                    {
                        myRaiDataTalentia.XR_STR_PROCESSO_SISTEMA_IT ps = new myRaiDataTalentia.XR_STR_PROCESSO_SISTEMA_IT();
                        ps.id_processo = p.id;
                        ps.id_sistema = idSistema;
                        ps.data_inizio_validita = DateTime.Today;
                        ps.data_fine_validita = new DateTime(9999, 12, 31);
                        db.XR_STR_PROCESSO_SISTEMA_IT.Add(ps);
                    }
                }
                if (!String.IsNullOrWhiteSpace(model.customOwner))
                {
                    int? idCustomOwner = IsExistingOrAdd(model.customOwner);
                    if (idCustomOwner != null)
                    {
                        myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE pd = new myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE();
                        pd.id_processo = p.id;
                        pd.id_sezione = (int)idCustomOwner;
                        pd.data_inizio_validita = DateTime.Today;
                        pd.data_fine_validita = new DateTime(9999, 12, 31);
                        pd.tipo_collegamento = "CustOwn";//custom owner - non nelle sezioni
                        db.XR_STR_PROCESSO_SEZIONE.Add(pd);
                    }
                }
                if (model.dirOwner != null && model.dirOwner.Any())
                {
                    foreach (int idDirezione in model.dirOwner)
                    {
                        myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE pd = new myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE();
                        pd.id_processo = p.id;
                        pd.id_sezione = idDirezione;
                        pd.data_inizio_validita = DateTime.Today;
                        pd.data_fine_validita = new DateTime(9999, 12, 31);
                        pd.tipo_collegamento = "O";
                        db.XR_STR_PROCESSO_SEZIONE.Add(pd);
                    }
                }
                if (model.dirCoinvolte != null && model.dirCoinvolte.Any())
                {
                    foreach (int idDirezione in model.dirCoinvolte)
                    {
                        myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE pd = new myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE();
                        pd.id_processo = p.id;
                        pd.id_sezione = idDirezione;
                        pd.data_inizio_validita = DateTime.Today;
                        pd.data_fine_validita = new DateTime(9999, 12, 31);
                        pd.tipo_collegamento = "C";
                        db.XR_STR_PROCESSO_SEZIONE.Add(pd);
                    }
                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                    {
                        applicativo = "Portale",
                        data = DateTime.Now,
                        error_message = ex.ToString(),
                        provenienza = "SaveProcesso",
                        matricola = CommonHelper.GetCurrentUserMatricola()
                    });
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, error = "Errore salvataggio dati : " + ex.Message }
                    };
                }
            }
            else
            {
                var procAttuale = db.XR_STR_PROCESSI.Where(x => x.id == model.idprocesso && x.data_fine_validita == D).FirstOrDefault();

                if (model.tipomodifica == "M")
                {
                    if (db.XR_STR_PROCESSI.Any(x => x.data_inizio_validita == DateTime.Today && x.id == model.idprocesso))
                        model.tipomodifica = "R";
                }
                if (model.tipomodifica == "R")
                {
                    //stesso record
                    procAttuale.nome_processo = model.nomeprocesso;
                    procAttuale.descrizione_processo = model.descrizioneprocesso;
                    procAttuale.codice_processo = model.codiceprocesso;
                }
                else
                {
                    //nuova istanza stesso id

                    procAttuale.data_fine_validita = DateTime.Today;

                    myRaiDataTalentia.XR_STR_PROCESSI procNew = new myRaiDataTalentia.XR_STR_PROCESSI();
                    procNew.id = procAttuale.id;
                    procNew.nome_processo = model.nomeprocesso;
                    procNew.descrizione_processo = model.descrizioneprocesso;
                    procNew.codice_processo = model.codiceprocesso;
                    procNew.data_inizio_validita = DateTime.Today;
                    procNew.data_fine_validita = new DateTime(9999, 12, 31);
                    db.XR_STR_PROCESSI.Add(procNew);
                }

                if (model.Documenti != null && model.Documenti.Any())
                {
                    SalvaDocumenti(procAttuale.id, model, db);
                }

                if (model.DirezioniOwnerCollegateOld == null) model.DirezioniOwnerCollegateOld = "";
                if (model.SistemiCollegatiOld == null) model.SistemiCollegatiOld = "";
                if (model.DirezioniCoinvolteCollegateOld == null) model.DirezioniCoinvolteCollegateOld = "";

                List<int> oldDirezioniOwner = new List<int>();
                if (!String.IsNullOrWhiteSpace(model.DirezioniOwnerCollegateOld))
                {
                    oldDirezioniOwner = model.DirezioniOwnerCollegateOld.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                }
                foreach (int idDirezioneOld in oldDirezioniOwner)
                {
                    if (model.dirOwner == null || !model.dirOwner.Contains(idDirezioneOld))
                    {
                        var RelToDelete =
                             db.XR_STR_PROCESSO_SEZIONE.Where(x => x.tipo_collegamento == "O" && x.id_processo == model.idprocesso &&
                             x.id_sezione == idDirezioneOld &&
                             x.data_fine_validita == new DateTime(9999, 12, 31)).FirstOrDefault();

                        if (RelToDelete != null)
                            RelToDelete.data_fine_validita = DateTime.Today;
                    }
                }
                if (model.dirOwner != null)
                {
                    foreach (int idDirezioneNow in model.dirOwner)
                    {
                        if (!oldDirezioniOwner.Contains(idDirezioneNow))
                        {
                            myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE ps = new myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE();
                            ps.id_processo = model.idprocesso;
                            ps.id_sezione = idDirezioneNow;
                            ps.data_inizio_validita = DateTime.Today;
                            ps.tipo_collegamento = "O";
                            ps.data_fine_validita = new DateTime(9999, 12, 31);
                            db.XR_STR_PROCESSO_SEZIONE.Add(ps);
                        }
                    }
                }

                List<int> OldDirezioniCoinvolte = new List<int>();
                if (!String.IsNullOrWhiteSpace(model.DirezioniCoinvolteCollegateOld))
                {
                    OldDirezioniCoinvolte = model.DirezioniCoinvolteCollegateOld.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                }
                foreach (int idDirezioneOld in OldDirezioniCoinvolte)
                {
                    if (model.dirCoinvolte == null || !model.dirCoinvolte.Contains(idDirezioneOld))
                    {
                        var RelToDelete =
                             db.XR_STR_PROCESSO_SEZIONE.Where(x => x.tipo_collegamento == "C" && x.id_processo == model.idprocesso &&
                             x.id_sezione == idDirezioneOld &&
                             x.data_fine_validita == new DateTime(9999, 12, 31)).FirstOrDefault();

                        if (RelToDelete != null)
                            RelToDelete.data_fine_validita = DateTime.Today;
                    }
                }
                if (model.dirCoinvolte != null)
                {
                    foreach (int idDirezioneNow in model.dirCoinvolte)
                    {
                        if (!OldDirezioniCoinvolte.Contains(idDirezioneNow))
                        {
                            myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE ps = new myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE();
                            ps.id_processo = model.idprocesso;
                            ps.id_sezione = idDirezioneNow;
                            ps.data_inizio_validita = DateTime.Today;
                            ps.tipo_collegamento = "C";
                            ps.data_fine_validita = new DateTime(9999, 12, 31);
                            db.XR_STR_PROCESSO_SEZIONE.Add(ps);
                        }
                    }
                }
                if (!String.IsNullOrWhiteSpace(model.customOwner))
                {
                    int? idCustomOwner = IsExistingOrAdd(model.customOwner);
                    if (idCustomOwner != null)
                    {
                        myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE pd = db.XR_STR_PROCESSO_SEZIONE.Where(x =>
                        x.tipo_collegamento == "CustOwn"
                         && x.id_processo == model.idprocesso).FirstOrDefault();
                        if (pd != null)
                            pd.id_sezione = (int)idCustomOwner;
                        else
                        {
                            myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE psez = new myRaiDataTalentia.XR_STR_PROCESSO_SEZIONE();
                            psez.id_processo = model.idprocesso;
                            psez.id_sezione = (int)idCustomOwner;
                            psez.data_inizio_validita = DateTime.Today;
                            psez.data_fine_validita = new DateTime(9999, 12, 31);
                            psez.tipo_collegamento = "CustOwn";//custom owner - non nelle sezioni
                            db.XR_STR_PROCESSO_SEZIONE.Add(psez);
                        }

                    }
                }
                else
                {
                    var pdlist = db.XR_STR_PROCESSO_SEZIONE.Where(x => x.tipo_collegamento == "CustOwn"
                                      && x.id_processo == model.idprocesso).ToList();
                    if (pdlist.Any())
                    {
                        foreach (var item in pdlist)
                        {
                            db.XR_STR_PROCESSO_SEZIONE.Remove(item);
                        }
                    }
                }




                List<int> oldSistemi = new List<int>();
                if (!String.IsNullOrWhiteSpace(model.SistemiCollegatiOld))
                {
                    oldSistemi = model.SistemiCollegatiOld.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                }
                foreach (int idSistemaOld in oldSistemi)
                {
                    if (model.sistemaIT == null || !model.sistemaIT.Contains(idSistemaOld))
                    {
                        var RelToDelete =
                             db.XR_STR_PROCESSO_SISTEMA_IT.Where(x => x.id_processo == model.idprocesso && x.id_sistema == idSistemaOld &&
                             x.data_fine_validita == new DateTime(9999, 12, 31)).FirstOrDefault();
                        if (RelToDelete != null)
                            RelToDelete.data_fine_validita = DateTime.Today;
                    }
                }
                if (model.sistemaIT != null)
                {
                    foreach (int idSistemaNow in model.sistemaIT)
                    {
                        if (!oldSistemi.Contains(idSistemaNow))
                        {
                            myRaiDataTalentia.XR_STR_PROCESSO_SISTEMA_IT ps = new myRaiDataTalentia.XR_STR_PROCESSO_SISTEMA_IT();
                            ps.id_processo = model.idprocesso;
                            ps.id_sistema = idSistemaNow;
                            ps.data_inizio_validita = DateTime.Today;
                            ps.data_fine_validita = new DateTime(9999, 12, 31);
                            db.XR_STR_PROCESSO_SISTEMA_IT.Add(ps);
                        }
                    }
                }

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                    {
                        applicativo = "Portale",
                        data = DateTime.Now,
                        error_message = ex.ToString(),
                        provenienza = "SaveProcesso",
                        matricola = CommonHelper.GetCurrentUserMatricola()
                    });
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, error = "Errore salvataggio dati : " + ex.Message }
                    };
                }
            }


            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }

        private int? IsExistingOrAdd(string customOwner)
        {
            if (String.IsNullOrWhiteSpace(customOwner)) throw new Exception("custom owner empty");

            var db = IncarichiManager.GetIncarichiDBContext();
            string cod = myRaiCommonManager.StrutturaOrganizzativaManager.HRIS_ExtraField.CustomProcessOwner.ToString();
            var extraField = db.XR_HRIS_EXTRA_FIELD.Where(x => x.COD_FIELD == cod
                             && x.STR_VALUE == customOwner.Trim()).FirstOrDefault();

            if (extraField != null)
                return extraField.ID_FIELD;
            else
            {
                myRaiDataTalentia.XR_HRIS_EXTRA_FIELD H = new XR_HRIS_EXTRA_FIELD()
                {
                    STR_VALUE = customOwner,
                    COD_FIELD = myRaiCommonManager.StrutturaOrganizzativaManager.HRIS_ExtraField.CustomProcessOwner.ToString(),
                    COD_TERMID = Request.UserHostAddress,
                    COD_USER = CommonHelper.GetCurrentUserMatricola(),
                    ID_TIPOLOGIA = 0,
                    ID_GESTIONE = 0,
                    MATRICOLA = CommonHelper.GetCurrentUserMatricola(),
                    TMS_TIMESTAMP = DateTime.Now
                };
                db.XR_HRIS_EXTRA_FIELD.Add(H);
                db.SaveChanges();
                return H.ID_FIELD;
            }

        }

        private void SalvaDocumenti(int id, SaveProcessoModel model, TalentiaEntities db)
        {
            foreach (var doc in model.Documenti)
            {
                if (doc.Action == "N")
                {
                    myRaiDataTalentia.XR_STR_ALLEGATI_PROCESSI ap = new XR_STR_ALLEGATI_PROCESSI();
                    MemoryStream target = new MemoryStream();
                    doc.Filecontent.InputStream.CopyTo(target);
                    ap.bytecontent = target.ToArray();
                    ap.categoria = doc.Tipo.ToString();
                    ap.nome_file = doc.Nome;
                    ap.descrizione = doc.Desc;
                    ap.id_processo = id;
                    ap.data_inizio_validita = DateTime.Today;
                    ap.data_fine_validita = new DateTime(9999, 12, 31);

                    db.XR_STR_ALLEGATI_PROCESSI.Add(ap);
                }
                if (doc.Action == "D")
                {
                    myRaiDataTalentia.XR_STR_ALLEGATI_PROCESSI ap = db.XR_STR_ALLEGATI_PROCESSI.Where(x => x.id == doc.id).FirstOrDefault();
                    if (ap != null)
                        ap.data_fine_validita = DateTime.Today;
                }
                if (doc.Action == "M")
                {
                    myRaiDataTalentia.XR_STR_ALLEGATI_PROCESSI ap = db.XR_STR_ALLEGATI_PROCESSI.Where(x => x.id == doc.id).FirstOrDefault();
                    if (ap != null)
                    {
                        ap.nome_file = doc.Nome;
                        ap.descrizione = doc.Desc;
                    }
                }
            }
        }

        private bool SomethingChanged(SaveProcessoModel model)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            DateTime D = new DateTime(9999, 12, 31);
            var processoDB = db.XR_STR_PROCESSI.Where(x => x.id == model.idprocesso && x.data_fine_validita == D).FirstOrDefault();
            bool changed =
                processoDB.codice_processo != model.codiceprocesso ||
                processoDB.nome_processo != model.nomeprocesso ||
                processoDB.descrizione_processo != model.descrizioneprocesso;

            return changed;
        }

        public ActionResult GetProcesso(int idprocesso, string data)
        {
            DateTime D;
            DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);
            var db = IncarichiManager.GetIncarichiDBContext();
            var proc = db.XR_STR_PROCESSI.Where(x => x.id == idprocesso && x.data_inizio_validita <= D && x.data_fine_validita > D).FirstOrDefault();
            DettaglioProcessoModel model = new DettaglioProcessoModel();
            model.Processo = proc;
            model.IsStartingProcess = db.XR_STR_TALBERO_GEN.Any(x => x.tipo_oggetto == "P" && x.subordinato_a == idprocesso && x.id == idprocesso);

            var idsist = db.XR_STR_PROCESSO_SISTEMA_IT.Where(x => x.id_processo == idprocesso && x.data_inizio_validita <= D && x.data_fine_validita > D)
                .Select(x => x.id_sistema)
                .ToList();
            model.SistemiCollegati = db.XR_STR_SISTEMI_IT.Where(x => idsist.Contains(x.id)).ToList();

            var idDir = db.XR_STR_PROCESSO_SEZIONE.Where(x => x.id_processo == idprocesso && x.tipo_collegamento == "O"
                      && x.data_inizio_validita <= D && x.data_fine_validita > D)
              .Select(x => x.id_sezione)
              .ToList();
            model.DirezioniOwnerCollegati = db.XR_STR_TSEZIONE.Where(x => idDir.Contains(x.id)).ToList();

            var idCustomOwnerCollegati = db.XR_STR_PROCESSO_SEZIONE.Where(x => x.id_processo == idprocesso && x.tipo_collegamento == "CustOwn"
                      && x.data_inizio_validita <= D && x.data_fine_validita > D)
              .Select(x => x.id_sezione)
              .ToList();
            string cod = myRaiCommonManager.StrutturaOrganizzativaManager.HRIS_ExtraField.CustomProcessOwner.ToString();
            model.CustomOwnerCollegati = db.XR_HRIS_EXTRA_FIELD.Where(x => idCustomOwnerCollegati.Contains(x.ID_FIELD))
                .Select(x => x.STR_VALUE).ToList();

            var idDirCoinv = db.XR_STR_PROCESSO_SEZIONE.Where(x => x.id_processo == idprocesso && x.tipo_collegamento == "C"
                     && x.data_inizio_validita <= D && x.data_fine_validita > D)
             .Select(x => x.id_sezione)
             .ToList();
            model.DirezioniCoinvolteCollegati = db.XR_STR_TSEZIONE.Where(x => idDirCoinv.Contains(x.id)).ToList();

            model.Allegati = db.XR_STR_ALLEGATI_PROCESSI.Where(x => x.id_processo == idprocesso && x.data_inizio_validita <= D
            && x.data_fine_validita > D).ToList();

            return View("_dettaglioProcesso", model);
        }
        public ActionResult GetDoc(int id)
        {
            var db = IncarichiManager.GetIncarichiDBContext();

            var doc = db.XR_STR_ALLEGATI_PROCESSI.Where(x => x.id == id).FirstOrDefault();

            if (doc == null) return null;


            byte[] byteArray = doc.bytecontent;
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;
            string app = "application/pdf";
            string ext = System.IO.Path.GetExtension(doc.nome_file).ToLower();
            switch (ext)
            {
                case ".docx":
                    app = "application/vnd.openxmlformats-officedocument.wordprocessing";
                    break;
                case ".xlsx":
                    app = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".pptx":
                    app = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    break;
            }

            Response.AppendHeader("content-disposition", "inline; filename=" + doc.nome_file);
            return new FileStreamResult(pdfStream, app);
        }
        public ActionResult CloseProcesso(int idprocesso)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            DateTime Dnow = DateTime.Today;
            var processoDB = db.XR_STR_PROCESSI.Where(x => x.id == idprocesso && x.data_fine_validita > Dnow &&
            x.data_inizio_validita <= Dnow).FirstOrDefault();
            if (processoDB != null)
            {
                processoDB.data_fine_validita = Dnow;
                try
                {
                    db.SaveChanges();
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = true }
                    };
                }
                catch (Exception ex)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, error = "Errore salvataggio dati" }
                    };
                }
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, error = "Errore dati nel DB" }
                };
            }
        }
        public int GetLivello(int idProcesso)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            int livello = 0;

            var ItemAlbero = db.XR_STR_TALBERO_GEN.Where(x => x.id == idProcesso && x.tipo_oggetto == "P").FirstOrDefault();
            while (ItemAlbero.subordinato_a != ItemAlbero.id)
            {
                livello++;
                int idProc = ItemAlbero.subordinato_a;
                ItemAlbero = db.XR_STR_TALBERO_GEN.Where(x => x.id == idProc && x.tipo_oggetto == "P").FirstOrDefault();
            }
            return livello;

        }
        public int GetLivelloSezione(int idSezione)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            int livello = 0;

            var ItemAlbero = db.XR_STR_TALBERO.Where(x => x.id == idSezione).FirstOrDefault();
            if (ItemAlbero == null) return 0;

            while (ItemAlbero.subordinato_a != ItemAlbero.id)
            {
                livello++;
                int idProc = ItemAlbero.subordinato_a;
                ItemAlbero = db.XR_STR_TALBERO.Where(x => x.id == idProc).FirstOrDefault();
            }
            return livello;

        }
        public bool HaFigli(int idProcesso)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            var ItemAlbero = db.XR_STR_TALBERO_GEN.Where(x => x.subordinato_a == idProcesso && x.tipo_oggetto == "P").FirstOrDefault();
            return ItemAlbero != null;
        }

        public GestioneProcessoModel GetGestioneProcessoModel(int idprocesso, string data)
        {
            GestioneProcessoModel model = new GestioneProcessoModel();
            DateTime D;
            DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);
            var db = IncarichiManager.GetIncarichiDBContext();
            model.ProcessoCorrente = db.XR_STR_PROCESSI.Where(x => x.id == idprocesso && x.data_inizio_validita <= D && x.data_fine_validita > D).FirstOrDefault();
            var treeItem = db.XR_STR_TALBERO_GEN.Where(x => x.tipo_oggetto == "P" && x.id == idprocesso).FirstOrDefault();
            if (treeItem != null)
            {
                model.ProcessoPadre = db.XR_STR_PROCESSI.Where(x => x.id == treeItem.subordinato_a && x.data_inizio_validita <= D && x.data_fine_validita >= D).FirstOrDefault();
            }
            model.Sistemi = db.XR_STR_SISTEMI_IT.OrderBy(x => x.nome_sistema).ToList();
            var idsist = db.XR_STR_PROCESSO_SISTEMA_IT.Where(x => x.id_processo == idprocesso && x.data_inizio_validita <= D && x.data_fine_validita > D)
                .Select(x => x.id_sistema)
                .ToList();
            model.SistemiCollegati = db.XR_STR_SISTEMI_IT.Where(x => idsist.Contains(x.id)).ToList();
            model.SistemiCollegatiOld = String.Join(",", model.SistemiCollegati.Select(x => x.id).ToArray());

            model = PopolaDirezioniOwner(idprocesso, model, db, D);
            model = PopolaDirezioniCoinvolte(idprocesso, model, db, D);
            model = GetAllegatiProcesso(idprocesso, model, db, D);
            var ProcessiSezioniCustomOwner = db.XR_STR_PROCESSO_SEZIONE.Where(x => x.tipo_collegamento == "CustOwn" && x.id_processo == idprocesso
              && x.data_inizio_validita <= D && x.data_fine_validita >= D).Select(x=>x.id_sezione).ToList();
            if (ProcessiSezioniCustomOwner.Any())
            {
                model.CustomOwner = db.XR_HRIS_EXTRA_FIELD.Where(x => ProcessiSezioniCustomOwner.Contains(x.ID_FIELD)).ToList();
            }
            return model;
        }
        public ActionResult ModProcesso(int idprocesso, string data)
        {
            var model = GetGestioneProcessoModel(idprocesso, data);
            return View("_gestProcesso", model);
        }
        public GestioneProcessoModel GetAllegatiProcesso(int idprocesso, GestioneProcessoModel model, TalentiaEntities db, DateTime D)
        {
            model.Allegati = db.XR_STR_ALLEGATI_PROCESSI.Where(x => x.id_processo == idprocesso && x.data_inizio_validita <= D &&
              x.data_fine_validita > D).ToList();
            return model;
        }
        public GestioneProcessoModel PopolaDirezioniOwner(int idprocesso, GestioneProcessoModel model,
            myRaiDataTalentia.TalentiaEntities db, DateTime D)
        {
            model.DirezioniOwner = db.XR_STR_TSEZIONE.Where(x => x.codice_visibile.Trim().Length == 2
                               && x.data_fine_validita == "99991231" && x.pubblicato == true
                               )
                               .OrderBy(x => x.descrizione_lunga)
                               .ToList();
            var idDir = db.XR_STR_PROCESSO_SEZIONE.Where(x => x.id_processo == idprocesso && x.tipo_collegamento == "O"
                        && x.data_inizio_validita <= D && x.data_fine_validita > D)
                .Select(x => x.id_sezione)
                .ToList();
            model.DirezioniOwnerCollegate = db.XR_STR_TSEZIONE.Where(x => idDir.Contains(x.id)).ToList();
            model.DirezioniOwnerCollegateOld = String.Join(",", model.DirezioniOwnerCollegate.Select(x => x.id).ToArray());
            return model;
        }
        public GestioneProcessoModel PopolaDirezioniCoinvolte(int idprocesso, GestioneProcessoModel model,
           myRaiDataTalentia.TalentiaEntities db, DateTime D)
        {
            model.DirezioniCoinvolte = db.XR_STR_TSEZIONE.Where(x => x.codice_visibile.Trim().Length == 2
                               && x.data_fine_validita == "99991231"
                               )
                               .OrderBy(x => x.descrizione_lunga)
                               .ToList();
            var idDir = db.XR_STR_PROCESSO_SEZIONE.Where(x => x.id_processo == idprocesso && x.tipo_collegamento == "C"
                        && x.data_inizio_validita <= D && x.data_fine_validita > D)
                .Select(x => x.id_sezione)
                .ToList();
            model.DirezioniCoinvolteCollegate = db.XR_STR_TSEZIONE.Where(x => idDir.Contains(x.id)).ToList();
            model.DirezioniCoinvolteCollegateOld = String.Join(",", model.DirezioniCoinvolteCollegate.Select(x => x.id).ToArray());
            return model;
        }
        /*   url: '/StrutturaOrganizzativa/addSistema',
                type: "POST",
                data: {sistema:nome},*/
        public ActionResult addSistema(string sistema)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            if (db.XR_STR_SISTEMI_IT.Any(x => x.nome_sistema == sistema))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, error = "Sistema già esistente" }
                };
            }
            var s = new XR_STR_SISTEMI_IT()
            {
                nome_sistema = sistema,
            };
            db.XR_STR_SISTEMI_IT.Add(s);
            db.SaveChanges();
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true, error = s.id }
            };
        }
        public ActionResult AddProcesso(int idprocesso, string data)
        {
            GestioneProcessoModel model = new GestioneProcessoModel();
            DateTime D;
            DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);
            var db = IncarichiManager.GetIncarichiDBContext();
            model.ProcessoPadre = db.XR_STR_PROCESSI.Where(x => x.id == idprocesso && x.data_inizio_validita <= D && x.data_fine_validita >= D).FirstOrDefault();
            model.Sistemi = db.XR_STR_SISTEMI_IT.OrderBy(x => x.nome_sistema).ToList();
            model.SistemiCollegati = new List<myRaiDataTalentia.XR_STR_SISTEMI_IT>();
            model.SistemiCollegatiOld = String.Join(",", model.SistemiCollegati.Select(x => x.id).ToArray());

            model = PopolaDirezioniOwner(idprocesso, model, db, D);
            model = PopolaDirezioniCoinvolte(idprocesso, model, db, D);
            model.Allegati = new List<XR_STR_ALLEGATI_PROCESSI>();
            return View("_gestProcesso", model);
        }
        public ActionResult getBoxAllegato(string tipo)
        {
            BoxAllegatoModel model = new BoxAllegatoModel() { tipo = tipo };
            return View("~/Views/strutturaorganizzativa/_boxallegato.cshtml", model);
        }
        public GenericTreeModel GetTreeProcessiModel(string data)
        {
            GenericTreeModel model = new GenericTreeModel();
            model.DBorigine = IncarichiManager.GetDB();// "P";
            model.DataStruttura = data;
            var db = IncarichiManager.GetIncarichiDBContext();

            DateTime D;
            DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);

            List<TreeItem> items = db.XR_STR_TALBERO_GEN.Where(x => x.tipo_oggetto == "P")
                              .Join(db.XR_STR_PROCESSI.OrderBy(x => x.codice_processo).Where(x => x.data_inizio_validita <= D && x.data_fine_validita > D),

                              alb => alb.id,
                              pr => pr.id,
                              (alb, pr) => new TreeItem
                              {
                                  ItemProcesso = pr,
                                  ItemAlbero = alb
                              })
                              .OrderBy(z => z.ItemProcesso.codice_processo)
                              .ToList();

            model.TreeItems = items;
            return model;
        }
        public ActionResult cancellaSistema(int id)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            var s = db.XR_STR_SISTEMI_IT.Where(x => x.id == id).FirstOrDefault();
            if (s == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, error = "Dati non trovati" }
                };
            }
            if (db.XR_STR_PROCESSO_SISTEMA_IT.Any(x => x.id_sistema == id))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, error = "Il sistema ha relazioni con uno o piu processi, impossibile eliminarlo." }
                };
            }
            try
            {
                db.XR_STR_SISTEMI_IT.Remove(s);
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, error = ex.Message }
                };
            }

        }
        public ActionResult getsistemi(int idprocesso, string data)
        {
            GestioneProcessoModel model = GetGestioneProcessoModel(idprocesso, data);
            return View(model);
        }
        public ActionResult _sistemi()
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            var GestioneSistemi = new GestioneSistemiModel();
            GestioneSistemi.Sistemi = db.XR_STR_SISTEMI_IT.OrderBy(x => x.nome_sistema).ToList();
            return View(GestioneSistemi);
        }
        public ActionResult Processi()
        {
            GenericTreeModel model = GetTreeProcessiModel(DateTime.Today.ToString("dd/MM/yyyy"));

            DateTime D = new DateTime(9999, 12, 31);
            var db = IncarichiManager.GetIncarichiDBContext();
            var inizialeAlbero = db.XR_STR_TALBERO_GEN.Where(x => x.tipo_oggetto == "P" && x.subordinato_a == x.id).FirstOrDefault();
            model.SelectedProcess = new DettaglioProcessoModel();

            model.SelectedProcess.Processo = db.XR_STR_PROCESSI.Where(x => x.id == inizialeAlbero.id && x.data_fine_validita == D).FirstOrDefault();
            model.SelectedProcess.IsStartingProcess = true;
            model.GestioneSistemi = new GestioneSistemiModel();


            model.GestioneSistemi.Sistemi = db.XR_STR_SISTEMI_IT.OrderBy(x => x.nome_sistema).ToList();
            // model.SelectedItem = items.Where(x => x.ItemAlbero.id == x.ItemAlbero.subordinato_a && x.ItemAlbero.tipo_oggetto=="P").FirstOrDefault();

            return View(model);
        }

        public ActionResult RespSaturi(int sezione, int idinc, string codsezione = null)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            if (!String.IsNullOrWhiteSpace(codsezione))
            {
                //viene da anagrafica
                var sezDaAnag = db.XR_STR_TSEZIONE.Where(x => x.codice_visibile == codsezione && x.data_fine_validita == "99991231").FirstOrDefault();
                if (sezDaAnag != null)
                {
                    sezione = sezDaAnag.id;
                }
            }
            int max = db.XR_STR_TSEZIONE.Where(x => x.id == sezione && x.data_fine_validita == "99991231")
                .Select(x => x.max_responsabili).FirstOrDefault();
            if (max == 1)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = "OK" }
                };
            }
            var responsabili = db.XR_STR_TINCARICO.Where(x => x.data_fine_validita == "99991231"
                                && x.id_sezione == sezione
                                && x.id_incarico != idinc
                                && x.flag_resp == "1").ToList();
            if (responsabili.Count() < max)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = "OK" }
                };
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    esito = "",
                    respnow = responsabili.Select(x => new { id = x.id_incarico, nome = x.nominativo }).ToArray()
                }
            };
        }
        public ActionResult Index()
        {
            var model = new myRaiCommonModel.Gestionale.IncarichiTreeModel(IncarichiManager.GetDB());


            model.GetModel(IncarichiManager.GetIncarichiDBContext());

            return View(model);
        }

        [HttpPost]
        public ActionResult Riversa()
        {
            string esito = IncarichiManager.Riversa();

            myRaiHelper.Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
            {
                operazione = "Incarichi: Copia Ipotesi -> Produzione",
                descrizione_operazione = esito
            });

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { error = (esito == null ? "" : esito) }
            };
        }

        [HttpPost]
        public ActionResult changedb(string orig)
        {
            string esito = IncarichiManager.ChangeDB(orig);
            Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
            {
                operazione = "Switch DB Incarichi",
                descrizione_operazione = "Switch su DB " + orig
            });

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { error = (esito == null ? "" : esito) }
            };
        }
        [HttpPost]
        public ActionResult moveSezione(int idsezione, int idSezionePadre)
        {
            string esito = IncarichiManager.MoveSezione(idsezione, idSezionePadre);
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { error = (esito == null ? "" : esito) }
            };
        }

        public ActionResult GetTreeProcessi(string data)
        {
            var model = GetTreeProcessiModel(data);
            return View("_treegen", model);
        }

        public ActionResult GetTree(string data)
        {
            var model = new myRaiCommonModel.Gestionale.IncarichiTreeModel(IncarichiManager.GetDB());
            model.GetModel(IncarichiManager.GetIncarichiDBContext(), data);
            return View("_tree", model);
        }
        public ActionResult GetDettaglioByNomeStruttura(string nomeSezione)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            var sez = db.XR_STR_TSEZIONE.Where(x => x.codice_visibile == nomeSezione &&
            x.data_fine_validita == "99991231").FirstOrDefault();

            var model = new myRaiCommonModel.Gestionale.IncarichiTreeModel(IncarichiManager.GetDB());
            model.FromAnagrafica = true;
            model.GetDettaglio(sez.id.ToString(), DateTime.Now.ToString("dd/MM/yyyy"),
                IncarichiManager.GetIncarichiDBContext());

            int? sezSuperiore = db.XR_STR_TALBERO.Where(x => x.id == sez.id)
                .Select(x => x.subordinato_a)
                .FirstOrDefault();
            if (sezSuperiore != null)
            {
                var incarichi = db.XR_STR_TINCARICO.Where(x => x.id_sezione == sezSuperiore && x.data_fine_validita == "99991231")
                    .OrderBy(x => x.cod_incarico)
                    .ToList();
                if (incarichi.Any())
                {

                    model.FromAnagraficaRiportaA = incarichi.First().nominativo;
                    if (model.FromAnagraficaRiportaA != null) model.FromAnagraficaRiportaA = model.FromAnagraficaRiportaA.Trim();

                    string cod = incarichi.First().cod_incarico;
                    string desc = db.XR_STR_DINCARICO.Where(x => x.COD_INCARICO == cod)
                        .Select(x => x.DES_INCARICO).FirstOrDefault();
                    //if (desc != null)
                    //   model.FromAnagraficaRiportaA += " (" + desc + ")";
                }
            }
            return View("_dettaglio", model);
        }

        public ActionResult GetDettaglio(string idsezione, string data, int livello)
        {
            var model = new myRaiCommonModel.Gestionale.IncarichiTreeModel(IncarichiManager.GetDB());
            model.GetDettaglio(idsezione, data, IncarichiManager.GetIncarichiDBContext());
            model.Dettaglio.LivelloNodo = livello;

            string matricola = CommonHelper.GetCurrentUserMatricola();

            model.Dettaglio.CanModify = StrOrgCanModify(matricola, model.Dettaglio.CodiceSezione ?? "A", model.Dettaglio.CodServizio, "", data);

            return View("_dettaglio", model);
        }

        private static bool StrOrgCanModify(string matricola, string sezione, string servizio, string idSezione, string data = "")
        {
            bool result = false;
            if (AuthHelper.EnabledToSubFunc(matricola, "STRORG", "AGG_STRORG"))
            {
                if (String.IsNullOrWhiteSpace(servizio) && String.IsNullOrWhiteSpace(sezione) && !String.IsNullOrWhiteSpace(idSezione))
                {
                    var model = new myRaiCommonModel.Gestionale.IncarichiTreeModel(IncarichiManager.GetDB());
                    model.GetDettaglio(idSezione, data, IncarichiManager.GetIncarichiDBContext());
                    servizio = model.Dettaglio.CodServizio;
                    sezione = model.Dettaglio.CodiceSezione ?? "A";
                }

                if (!String.IsNullOrWhiteSpace(servizio))
                {
                    string serCurrent = servizio.Trim();
                    AbilDir abilDir = AuthHelper.EnabledDirection(matricola, "STRORG", "AGG_STRORG");
                    result = !abilDir.HasFilter
                                        || ((abilDir.DirezioniIncluse == null || !abilDir.DirezioniIncluse.Any() || abilDir.DirezioniIncluse.Contains(serCurrent))
                                            && (abilDir.DirezioniEscluse == null || !abilDir.DirezioniEscluse.Any() || !abilDir.DirezioniEscluse.Contains(serCurrent)));
                }
                else if (!String.IsNullOrWhiteSpace(sezione) || sezione.Trim().Length == 1)
                {
                    string socCurrent = sezione.Trim();

                    AbilSocieta abilSoc = AuthHelper.EnabledSocieta(matricola, "STRORG", "AGG_STRORG");
                    result = !abilSoc.HasFilter
                                        || ((abilSoc.SocietaSezIncluse == null || !abilSoc.SocietaSezIncluse.Any() || abilSoc.SocietaSezIncluse.Contains(socCurrent.Trim()))
                                            && (abilSoc.SocietaSezEscluse == null || !abilSoc.SocietaSezEscluse.Any() || !abilSoc.SocietaSezEscluse.Contains(socCurrent.Trim())));
                }
            }
            return result;
        }

        public ActionResult GetDipendenti(string sezione, string data, string sede)
        {
            var model = new myRaiCommonModel.Gestionale.IncarichiTreeModel(IncarichiManager.GetDB());
            model.Dettaglio.Dipendenti = model.GetDipendenti(sezione, sede, data, IncarichiManager.GetIncarichiDBContext());
            foreach (var d in model.Dettaglio.Dipendenti)
            {
                // d.Nominativo = d.Nominativo.Trim().TitleCase();
                // d.DTA_INIZIO_CR = model.GetDateFrom_yyMMdd(d.Data_assunzione).ToString("dd/MM/yyyy");
            }
            return View("_dipendenti", model);
        }
        public ActionResult GetDipendentiFromAnagrafica(string sezione)
        {
            string data = DateTime.Now.ToString("dd/MM/yyyy");
            var model = new myRaiCommonModel.Gestionale.IncarichiTreeModel(IncarichiManager.GetDB());
            model.Dettaglio.Dipendenti = model.GetDipendenti(sezione, "0-0", data, IncarichiManager.GetIncarichiDBContext());
            foreach (var d in model.Dettaglio.Dipendenti)
            {
                // d.Nominativo = d.Nominativo.Trim().TitleCase();
                // d.DTA_INIZIO_CR = model.GetDateFrom_yyMMdd(d.Data_assunzione).ToString("dd/MM/yyyy");
            }

            return View("_dipendenti", model);
        }

        public ActionResult GetDettaglioIncarico(int idincarico)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            var model = new myRaiCommonModel.Gestionale.GestioneIncaricoModel();
            model.IncarichiAll = IncarichiManager.getIncarichiAll();

            model.Incarico = db.XR_STR_TINCARICO.Where(x => x.id_incarico == idincarico).FirstOrDefault();
            return View("_gestIncarichi", model);
        }

        public ActionResult GetDettaglioIncaricoNew(int idsezione, string matricola, string cognome, string nome,
            bool fromAnagrafica = false)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            var model = new myRaiCommonModel.Gestionale.GestioneIncaricoModel();
            model.FromAnagrafica = fromAnagrafica;

            model.IncarichiAll = IncarichiManager.getIncarichiAll();
            model.Incarico = new myRaiDataTalentia.XR_STR_TINCARICO()
            {
                matricola = matricola,
                nominativo = cognome + " " + nome,
                data_inizio_validita = DateTime.Now.ToString("yyyyMMdd"),
                data_fine_validita = new DateTime(9999, 12, 31).ToString("yyyyMMdd"),
                id_sezione = idsezione,
                flag_resp = "1"
            };
            return View("_gestIncarichi", model);

        }

        public IncarichiTreeModel GetModelIncarichi(int idsezione, string data)
        {
            IncarichiTreeModel model = new myRaiCommonModel.Gestionale.IncarichiTreeModel(IncarichiManager.GetDB());
            model.GetModel(IncarichiManager.GetIncarichiDBContext());

            foreach (var a in model.AlberoItems.Where(x => x.subordinato_a == idsezione)
                .OrderBy(x => x.num_ordina)
                .ToList())
            {
                a.FirstShowing = true;
            }
            model.AlberoItems.RemoveWhere(x => x.pubblicato == false);

            DateTime D;
            DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);
            int d = Convert.ToInt32(D.ToString("yyyyMMdd"));

            var db = IncarichiManager.GetIncarichiDBContext();
            var ListSezioni2Car = db.XR_STR_TSEZIONE.Where(x => x.codice_visibile != null && x.codice_visibile.Trim().Length == 2)
                .ToList();
            //altrimenti non supportato
            var idListSezioni2Car = ListSezioni2Car.Where(x => Convert.ToInt32(x.data_inizio_validita) <= d && Convert.ToInt32(x.data_fine_validita) >= d)
                                    .Select(x => x.id)
                                    .ToList();
            List<string> CodIncarichiViceDir =
                db.XR_STR_DINCARICO.Where(x => x.DES_INCARICO != null && x.DES_INCARICO.Contains("vice direttore"))
                .Select(x => x.COD_INCARICO).ToList();

            var ListIncarichi = db.XR_STR_TINCARICO.Where(x => x.flag_resp == "1" ||
            (idListSezioni2Car.Contains(x.id_sezione) && (CodIncarichiViceDir.Contains(x.cod_incarico) || x.incarico_personalizzato.Contains("vice direttore")))
            ).OrderByDescending(x => x.flag_resp).ToList();

            var DescIncarichi = db.XR_STR_DINCARICO.ToList();

            foreach (var item in model.AlberoItems)
            {
                var inca = ListIncarichi.Where(x => Convert.ToInt32(x.data_inizio_validita) <= d
                && Convert.ToInt32(x.data_fine_validita) >= d && x.id_sezione == item.id).ToList();

                if (inca.Any())
                {
                    foreach (var i in inca)
                    {
                        string desc = DescIncarichi.Where(x => x.COD_INCARICO == i.cod_incarico)
                            .Select(x => x.DES_INCARICO)
                            .FirstOrDefault();
                        if (i.cod_incarico == "999999" && !String.IsNullOrWhiteSpace(i.incarico_personalizzato))
                            desc = i.incarico_personalizzato;

                        var inc = new IncaricoResponsabile()
                        {
                            Incarico = desc,
                            Responsabile = i.nominativo != null ? i.nominativo.Trim() : i.nominativo,
                            Matr = i.matricola
                        };
                        DateTime Din;
                        if (DateTime.TryParseExact(i.data_inizio_validita, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out Din))
                        {
                            inc.DataInizioIncarico = Din;
                        }
                        string CognomeNome = IncarichiManager.GetNomeCognomeCamel(i.matricola);
                        if (!String.IsNullOrWhiteSpace(CognomeNome))
                        {
                            inc.Responsabile = CognomeNome;
                        }

                        item.Incarichi.Add(inc);
                    }
                }

            }
            //var sezi = db.XR_STR_TSEZIONE.Where(x => x.id == idsezione).FirstOrDefault();
            //if (sezi != null)
            //{
            //    model.Dettaglio.DescSezione = sezi.descrizione_lunga;
            //    model.Dettaglio.MissionSezione = sezi.mission;
            //}

            return model;
        }
        public ActionResult GetIncarichiAll(int idsezione, string data, bool forceUpdateSession = false)
        {
            var model = GetModelIncarichi(idsezione, data);
            OggettoPerSessioneOrganizzazione sessionObj = new OggettoPerSessioneOrganizzazione();
            
            if (forceUpdateSession)
            {
                string key = "GetIncarichiAllExt";
                sessionObj = new OggettoPerSessioneOrganizzazione();
                sessionObj.Obj = model;
                sessionObj.Data = DateTime.Now;
                ScriviSessioneSulDB(sessionObj, key);
                SessionHelper.Set(key, sessionObj);
            }

            return View("_tableincall", model);
        }
        public ActionResult GetIncarichiAllExt(int idsezione, string data)
        {
            OggettoPerSessioneOrganizzazione sessionObj = new OggettoPerSessioneOrganizzazione();
            IncarichiTreeModel model = null;
            string key = "GetIncarichiAllExt";
            try
            {
                var cercaInSessione = SessionHelper.Get(key);
                // cerca in sessione
                if (cercaInSessione != null)
                {
                    sessionObj = (OggettoPerSessioneOrganizzazione)cercaInSessione;
                }
                else
                // se non ci sono dati in sessione, cerca sul db
                {
                    sessionObj = CercaSessioneSulDB(key);                    
                }

                if (sessionObj != null)
                {
                    DateTime ora = DateTime.Now;
                    TimeSpan span = ora.Subtract(sessionObj.Data);
                    if (span.TotalMinutes <= 15)
                    {
                        model = sessionObj.Obj;
                        SessionHelper.Set(key, sessionObj);
                    }
                }
            }
            catch (Exception ex)
            {
                model = null;
            }

            if (model == null)
            {
                model = GetModelIncarichi(idsezione, data);
                sessionObj = new OggettoPerSessioneOrganizzazione();
                sessionObj.Obj = model;
                sessionObj.Data = DateTime.Now;
                ScriviSessioneSulDB(sessionObj, key);
                SessionHelper.Set(key, sessionObj);
            }

            return View("_tableincallExt", model);
        }

        private void ScriviSessioneSulDB(OggettoPerSessioneOrganizzazione dati, string chiave)
        {
            try
            {
                digiGappEntities db = new digiGappEntities();
                string dat = JsonConvert.SerializeObject(dati);
                MyRai_ParametriSistema par = new MyRai_ParametriSistema();
                par.Chiave = chiave;                
                par.Valore1 = dat;

                var toUpdate = db.MyRai_ParametriSistema.Where(w => w.Chiave.Equals(chiave)).FirstOrDefault();
                if (toUpdate != null)
                {
                    toUpdate.Valore1 = par.Valore1;
                }
                else
                {
                    db.MyRai_ParametriSistema.Add(par);
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }

        private OggettoPerSessioneOrganizzazione CercaSessioneSulDB(string chiave)
        {
            OggettoPerSessioneOrganizzazione sessionObj = null;
            try
            {
                digiGappEntities db = new digiGappEntities();
                var sessionePar = db.MyRai_ParametriSistema.Where(w => w.Chiave.Equals(chiave)).FirstOrDefault();

                if (sessionePar != null && !String.IsNullOrEmpty(sessionePar.Valore1))
                {
                    OggettoPerSessioneOrganizzazione obj = JsonConvert.DeserializeObject<OggettoPerSessioneOrganizzazione>(sessionePar.Valore1);

                    sessionObj = new OggettoPerSessioneOrganizzazione();
                    sessionObj = obj;
                }
            }
            catch (Exception ex)
            {
                sessionObj = null;
            }

            return sessionObj;
        }

        public JsonResult UltimoAggiornamentoSessione()
        {
            AggiornaSessioneResponse response = new AggiornaSessioneResponse();
            response.Esito = true;
            try
            {
                OggettoPerSessioneOrganizzazione sessionObj = new OggettoPerSessioneOrganizzazione();
                string key = "GetIncarichiAllExt";

                sessionObj = CercaSessioneSulDB(key);
                if (sessionObj != null)
                {
                    response.Data = sessionObj.Data;
                }
                else
                {
                    throw new Exception("Non sono presenti dati");
                }
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportIncarichi(int idsezione, string data)
        {

            MemoryStream ms = GetXlsx(idsezione, data);

            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            { FileDownloadName = "Incarichi_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx" }; ;
        }
        public static string FirstCharToUpper(string name)
        {
            if (String.IsNullOrEmpty(name))
                return name;
            return name.First().ToString().ToUpper() + String.Join("", name.Skip(1));
        }

        public int StampaRow(TAlberoSezioneModel item,
            List<myRaiDataTalentia.XR_TB_STR_TIPOLOGIA> tipologie, int rowCounter, IXLWorksheet worksheet,
            List<Dip> dati, IncarichiTreeModel model
            )
        {
            myRaiDataTalentia.XR_TB_STR_TIPOLOGIA tipologia =
                   tipologie.Where(x => x.COD_TIPOLOGIA == item.tipologia).FirstOrDefault();
            if (tipologia == null || tipologia.DES_TIPOLOGIA == null || tipologia.DES_TIPOLOGIA.ToLower() == "altro")
                return rowCounter;

            rowCounter++;
            worksheet.Cell(rowCounter, 1).Value = item.codice_visibile;
            worksheet.Cell(rowCounter, 2).Value = tipologia.DES_TIPOLOGIA;
            worksheet.Cell(rowCounter, 3).Value = item.descrizione_lunga;
            if (item.Incarichi.Any())
            {
                for (int i = 0; i < item.Incarichi.Count; i++)
                {

                    var dip = dati.Where(x => x.matr == item.Incarichi[i].Matr).FirstOrDefault();

                    if (dip != null)
                    {
                        if (i > 0) rowCounter++;
                        worksheet.Cell(rowCounter, 4).Value = FirstCharToUpper(dip.nome.ToLower()) + " " + dip.cogn.ToUpper();
                        worksheet.Cell(rowCounter, 5).Value = item.Incarichi[i].Incarico;
                        worksheet.Cell(rowCounter, 6).Value = dip.qual;
                        worksheet.Cell(rowCounter, 7).Value = dip.sess;
                    }
                }
            }
            foreach (var child in model.AlberoItems.Where(x => x.subordinato_a == item.id).OrderBy(x => x.num_ordina))
            {
                rowCounter = StampaRow(child, tipologie, rowCounter, worksheet, dati, model);
            }
            return rowCounter;
        }
        public class Dip
        {
            public string matr { get; set; }
            public string nome { get; set; }
            public string cogn { get; set; }
            public string qual { get; set; }
            public string sess { get; set; }
        }
        public MemoryStream GetXlsx(int idsezione, string data)
        {
            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Incarichi");

            worksheet.Cell(1, 1).Value = "Codice";
            worksheet.Cell(1, 2).Value = "Tipologia";
            worksheet.Cell(1, 3).Value = "Denominazione";
            worksheet.Cell(1, 4).Value = "Titolare";
            worksheet.Cell(1, 5).Value = "Incarico";
            worksheet.Cell(1, 6).Value = "Profilo";
            worksheet.Cell(1, 7).Value = "Sesso";

            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 2).Style.Font.Bold = true;
            worksheet.Cell(1, 3).Style.Font.Bold = true;
            worksheet.Cell(1, 4).Style.Font.Bold = true;
            worksheet.Cell(1, 5).Style.Font.Bold = true;
            worksheet.Cell(1, 6).Style.Font.Bold = true;
            worksheet.Cell(1, 7).Style.Font.Bold = true;

            var model = GetModelIncarichi(idsezione, data);
            var db = IncarichiManager.GetIncarichiDBContext();

            var dati = db.SINTESI1.Select(x => new Dip()
            {
                cogn = x.DES_COGNOMEPERS,
                nome = x.DES_NOMEPERS,
                matr = x.COD_MATLIBROMAT,
                qual = x.DES_QUALIFICA,
                sess = x.COD_SESSO
            }).ToList();

            var tipologie = db.XR_TB_STR_TIPOLOGIA.ToList();

            int rowCounter = 2;
            foreach (var item in model.AlberoItems.Where(x => x.FirstShowing == true).OrderBy(x => x.num_ordina))
            {
                rowCounter = StampaRow(item, tipologie, rowCounter, worksheet, dati, model);
            }


            worksheet.Columns().AdjustToContents();

            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return ms;
        }

        public ActionResult GetIncarichi(int idsezione, string data)
        {
            var model = new myRaiCommonModel.Gestionale.IncarichiTreeModel(IncarichiManager.GetDB());
            var db = IncarichiManager.GetIncarichiDBContext();
            DateTime D;
            DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);
            int d = Convert.ToInt32(D.ToString("yyyyMMdd"));

            model.IncarichiSezione = db.XR_STR_TINCARICO
              .AsEnumerable()
              .Where(x => Convert.ToInt32(x.data_inizio_validita) <= d && Convert.ToInt32(x.data_fine_validita) >= d && x.id_sezione == idsezione).ToList();
            model.Dettaglio.IdSezione = idsezione;
            model.Dettaglio.CanModify = StrOrgCanModify(CommonHelper.GetCurrentUserMatricola(), null, null, idsezione.ToString(), data);
            foreach (var i in model.IncarichiSezione)
            {
                string nc = IncarichiManager.GetNomeCognomeCamel(i.matricola);
                if (!String.IsNullOrWhiteSpace(nc))
                    i.nominativo = nc;
            }
            return View("_incarichi", model);
        }
        public ActionResult GestisciSezione(int idsezione, string data, string azione)
        {
            GestioneSezioneModel model = new GestioneSezioneModel();
            int liv = GetLivelloSezione(idsezione);

            model.GetSezione(idsezione, data, azione, IncarichiManager.GetIncarichiDBContext(), liv);
            model.IsVisualizzazione = !StrOrgCanModify(CommonHelper.GetCurrentUserMatricola(), null, null, idsezione.ToString(), data);

            return View("_gestSezioni", model);
        }


        [HttpPost]
        public ActionResult EliminaIncarico(int idincarico)
        {
            string esito = IncarichiManager.EliminaIncarico(idincarico);
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { error = (esito == null ? "" : esito) }
            };
        }
        [HttpPost]
        public ActionResult GetDataIncarico(int idsezione, string incarico, string matricola)
        {

            var db = IncarichiManager.GetIncarichiDBContext();
            string DataInizioNuovoIncarico = DateTime.Now.ToString("dd/MM/yyyy");

            var incaAttivo = db.XR_STR_TINCARICO.Where(x => x.id_sezione == idsezione
             && x.cod_incarico == incarico
             && x.data_fine_validita == "99991231").FirstOrDefault();

            if (incaAttivo != null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true, info = DataInizioNuovoIncarico }
                };
            }

            var incaChiuso = db.XR_STR_TINCARICO.Where(x => x.id_sezione == idsezione
             && x.cod_incarico == incarico
             && x.data_fine_validita != "99991231").OrderByDescending(x => x.id_incarico).FirstOrDefault();
            if (incaChiuso != null)
            {
                DateTime D;
                if (DateTime.TryParseExact(incaChiuso.data_fine_validita, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out D))
                {
                    DataInizioNuovoIncarico = D.AddDays(1).ToString("dd/MM/yyyy");
                    DateTime? Dass = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matricola).Select(x => x.DTA_INIZIO_CR).FirstOrDefault();
                    if (Dass != null && Dass.Value > D.AddDays(1))
                    {
                        DataInizioNuovoIncarico = Dass.Value.ToString("dd/MM/yyyy");
                    }
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true, info = DataInizioNuovoIncarico }
            };
        }

        /*
           url: '/StrutturaOrganizzativa/getdataincarico',
            type: "POST",
            data: {  idsezione:idsezione, incarico:incarico },
             */
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveSezione(GestioneSezioneModel model, string tipoSalvataggio)
        {
            if (model.Sezione.max_responsabili <= 0) model.Sezione.max_responsabili = 1;

            //R stessa validita
            //M nuova
            string esito = IncarichiManager.SaveSezione(model, tipoSalvataggio);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { error = (esito == null ? "" : esito) }
            };
        }

        [HttpPost]
        public ActionResult RicercaDip(RicercaAnagrafica model)
        {
            model.HasFilter = true;
            var elenco = IncarichiManager.GetElencoAnagrafiche(model);

            return View("_anagrafiche_sel", elenco);

        }

        [HttpPost]
        public ActionResult GetMySection(string matr)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            var sint = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matr).FirstOrDefault();
            if (sint != null && !String.IsNullOrWhiteSpace(sint.COD_UNITAORG))
            {

                int d = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));

                var sez = db.XR_STR_TSEZIONE.Where(x => x.codice_visibile.Trim() == sint.COD_UNITAORG.Trim())
                    .AsEnumerable()
                    .Where(x => Convert.ToInt32(x.data_inizio_validita) <= d && Convert.ToInt32(x.data_fine_validita) >= d)
                    .FirstOrDefault();

                if (sez == null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { success = false, error = "Matricola non trovata nella struttura" }
                    };
                }
                List<string> parentChain = new List<string>();
                parentChain.Add(sez.id.ToString());

                if (sez != null)
                {
                    int idParent = sez.id;
                    while (idParent != 1)
                    {
                        var alb = db.XR_STR_TALBERO.Where(x => x.id == idParent).FirstOrDefault();
                        if (alb == null)
                            break;

                        idParent = alb.subordinato_a;
                        parentChain.Add(idParent.ToString());
                    }
                    parentChain.Reverse();

                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { success = true, cod = String.Join(",", parentChain.ToArray()) }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { success = false, error = "Matricola non trovata nella struttura" }
            };
        }

        [HttpPost]
        public ActionResult SaveOrder(string sez, string data)
        {
            string esito = IncarichiManager.SaveOrder(sez, data);
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { error = (esito == null ? "" : esito) }
            };
        }

        [HttpPost]
        public ActionResult SaveIncarico(GestioneIncaricoModel model)
        {
            string esito = IncarichiManager.SaveIncarico(model);
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    error = (esito == null ? "" : esito),
                    fromAnag = (!String.IsNullOrWhiteSpace(model.CodSezioneFromAnagrafica) ? "1" : "")
                }
            };
        }

        public ActionResult SelezioneDipendenti()
        {
            return View("SelezioneDipendenti");
        }
    }
}