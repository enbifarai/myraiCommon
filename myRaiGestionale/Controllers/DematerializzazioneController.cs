using ImageMagick;
using iTextSharp.text;
using iTextSharp.text.pdf;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData;
using myRaiData.Incentivi;
using myRaiGestionale.it.rai.servizi.raiconnectcoll;
using myRaiHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using myRai.Business;
using Giornata = MyRaiServiceInterface.it.rai.servizi.digigappws.Giornata;
using System.Text;
using System.Data.Objects;
using myRaiGestionale.Helpers;

namespace myRaiGestionale.Controllers
{
    public class DematerializzazioneController : BaseCommonController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SessionHelper.Set("GEST_SECTION", "GESTIONE");
            base.OnActionExecuting(filterContext);
        }

        public JsonResult IsRichiestaGiaPresente(string matricolaDestinatario,
                                                string tipologiaDocumentale,//VSRUO
                                                string tipodoc,//MAT
                                                string tipoWKF,//"DEMDOC_VSRUO_MAT"
                                                string attrs)
        {
            int idTipoDocumento = 0;
            int idtipoWorkFlow = 0;

            tipodoc = tipodoc.Trim().ToUpper();
            tipologiaDocumentale = tipologiaDocumentale.Trim().ToUpper();
            tipoWKF = tipoWKF.Trim().ToUpper();

            try
            {
                List<AttributiAggiuntivi> objScelte = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(attrs);
                IncentiviEntities db = new IncentiviEntities();

                int idvariazioneContabile = 0;

                try
                {
                    idvariazioneContabile = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Codice.ToUpper().Equals("VCON")).FirstOrDefault().Id;
                }
                catch (Exception ex)
                {

                }

                // recupero id tipo documento
                XR_DEM_TIPI_DOCUMENTO tipoDocumento = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Codice.ToUpper().Equals(tipodoc)).FirstOrDefault();

                if (tipoDocumento != null)
                {
                    idTipoDocumento = tipoDocumento.Id;
                }

                XR_WKF_TIPOLOGIA wkfTipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.ToUpper().Equals(tipoWKF)).FirstOrDefault();

                if (wkfTipologia != null)
                {
                    idtipoWorkFlow = wkfTipologia.ID_TIPOLOGIA;
                }

                // prende tutti i documenti aperti che rispondono ai criteri di base, senza considerare il json
                var richieste = db.XR_DEM_DOCUMENTI.Where(w => w.Id_Tipo_Doc.Equals(idTipoDocumento) &&
                                                            w.Id_WKF_Tipologia.Equals(idtipoWorkFlow) &&
                                                            w.Cod_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
                                                            w.MatricolaDestinatario.Equals(matricolaDestinatario) &&
                                                            w.Id_Stato != (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore &&
                                                            w.Id_Stato != (int)StatiDematerializzazioneDocumenti.RifiutatoFirma &&
                                                            w.Id_Stato != (int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente &&
                                                            w.Id_Stato != (int)StatiDematerializzazioneDocumenti.PraticaCancellata).ToList();

                // a questo punto abbiamo tutti i documenti che sono ancora aperti e che rispondono ai criteri passati
                // per ogni elemento va visto se nel json sono presenti gli stessi dati che si sta cercando di inserire
                if (richieste != null && richieste.Any())
                {
                    XR_DEM_DOCUMENTI _tempDoc = new XR_DEM_DOCUMENTI();
                    _tempDoc.Id = -1;
                    _tempDoc.MatricolaDestinatario = matricolaDestinatario;
                    _tempDoc.CustomDataJSON = attrs;
                    _tempDoc.Id_Tipo_Doc = idTipoDocumento;
                    _tempDoc.Id_WKF_Tipologia = idtipoWorkFlow;
                    _tempDoc.Cod_Tipologia_Documentale = tipologiaDocumentale;

                    List<XR_MAT_RICHIESTE> lista_XR_MAT_RICHIESTE_Temp = new List<XR_MAT_RICHIESTE>();
                    lista_XR_MAT_RICHIESTE_Temp = PreparaOggettoXR_MAT_RICHIESTE2(_tempDoc);

                    foreach (var r in richieste)
                    {
                        if (r.Id_Tipo_Doc == idvariazioneContabile)
                        {
                            // al momento se il documento è di tipo Variazione contabile non viene controllato
                            continue;
                        }

                        if (String.IsNullOrEmpty(r.CustomDataJSON) || r.CustomDataJSON == "[]")
                        {
                            continue;
                        }

                        List<XR_MAT_RICHIESTE> listaR = new List<XR_MAT_RICHIESTE>();
                        listaR = PreparaOggettoXR_MAT_RICHIESTE2(r);

                        if (listaR == null || !listaR.Any())
                        {
                            throw new Exception("Errore nella generazione delle eccezioni da inserire in GAPP");
                        }

                        foreach (var XR_R in lista_XR_MAT_RICHIESTE_Temp)
                        {
                            bool equal = listaR.Count(w =>
                                                        w.MATRICOLA == XR_R.MATRICOLA &&
                                                        w.ECCEZIONE == XR_R.ECCEZIONE &&
                                                        w.DATA_INIZIO_MATERNITA == XR_R.DATA_INIZIO_MATERNITA &&
                                                        w.DATA_FINE_MATERNITA == XR_R.DATA_FINE_MATERNITA &&
                                                        w.DATA_NASCITA_BAMBINO == XR_R.DATA_NASCITA_BAMBINO &&
                                                        w.DATA_PRESUNTA_PARTO == XR_R.DATA_PRESUNTA_PARTO &&
                                                        w.INIZIO_GIUSTIFICATIVO == XR_R.INIZIO_GIUSTIFICATIVO &&
                                                        w.FINE_GIUSTIFICATIVO == XR_R.FINE_GIUSTIFICATIVO) > 0;

                            if (equal)
                            {
                                return Json(equal, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        #region APPROVATORE DEMATERIALIZZAZIONE

        public ActionResult GetContent(bool approvazioneEnabled = false)
        {
            DematerializzazioneDocumentiVM model = new DematerializzazioneDocumentiVM();
            model.IsPreview = false;
            model.ApprovazioneEnabled = approvazioneEnabled;
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>();
            model.Documenti = DematerializzazioneManager.GetDocumentiDaApprovare2();
            model.Matricola = CommonHelper.GetCurrentUserMatricola();
            return View("~/Views/Dematerializzazione/subpartial/Content.cshtml", model);
        }

        public ActionResult Approvatore()
        {
            AnagraficaModel anagraficaModel = null;
            DematerializzazioneDocumentiVM model = new DematerializzazioneDocumentiVM();

            SezioniAnag sezione = SezioniAnag.NonDefinito;
            anagraficaModel = AnagraficaManager.GetAnagrafica(UtenteHelper.Matricola(), new AnagraficaLoader(sezione));

            if (anagraficaModel != null)
            {
                model.IdPersona = anagraficaModel.IdPersona;
                model.Matricola = anagraficaModel.Matricola;

                if (anagraficaModel.Matricola == null)
                {
                    model.Matricola = CommonHelper.GetCurrentUserMatricola();
                }
            }

            model.IsPreview = true;
            return View("~/Views/Dematerializzazione/Approvatore.cshtml", model);
        }

        #endregion

        #region VISIONATORE

        [ObsoleteAttribute("This method is obsolete. Call NewMethod instead.", true)]
        public ActionResult Visionatore()
        {
            AnagraficaModel anagraficaModel = null;
            DematerializzazioneDocumentiVM model = new DematerializzazioneDocumentiVM();

            SezioniAnag sezione = SezioniAnag.NonDefinito;
            anagraficaModel = AnagraficaManager.GetAnagrafica(UtenteHelper.Matricola(), new AnagraficaLoader(sezione));

            if (anagraficaModel != null)
            {
                model.IdPersona = anagraficaModel.IdPersona;
                model.Matricola = anagraficaModel.Matricola;

                if (anagraficaModel.Matricola == null)
                {
                    model.Matricola = CommonHelper.GetCurrentUserMatricola();
                }
            }

            model.IsPreview = true;
            return View("~/Views/Dematerializzazione/Visionatore.cshtml", model);
        }

        public ActionResult GetContentVisionatore()
        {
            DematerializzazioneDocumentiVM model = new DematerializzazioneDocumentiVM();
            model.IsPreview = false;
            model.ApprovazioneEnabled = false;
            model.PrendiInCaricoEnabled = false;
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>();
            model.Documenti = DematerializzazioneManager.GetDocumentiDaVisionare();
            model.Matricola = CommonHelper.GetCurrentUserMatricola();
            return View("~/Views/Dematerializzazione/subpartial/ContentVisionatore.cshtml", model);
        }

        #endregion

        #region METODI NUOVI
        /// <summary>
        /// Metodo per l'aggiornamento del campo DaSiglare in merito agli allegati
        /// di un documento
        public void UpdateDaSiglareAllegato(int idAllegato, bool value)
        {
            try
            {
                var db = AnagraficaManager.GetDb();
                var item = db.XR_ALLEGATI.Where(x => x.Id.Equals(idAllegato)).FirstOrDefault();
                item.DaSiglare = value;

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ScriviLogDEM(0, 0, "UpdateDaSiglareAllegato", "Errore in fase di Update del campo DaSiglare sulla tabella XR_Allegati");
            }
        }

        /// <summary>
        /// Metodo per l'upload dei file temporanei durante la creazione
        /// di un documento
        /// </summary>
        /// <param name="file"></param>
        /// <param name="filePrincipale"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadTempDocument(HttpPostedFileBase file, bool filePrincipale = true, int tipologiaFile = 1)
        {
            string fileName = "";
            int length = 0;
            List<int> idAllegati = new List<int>();
            int result = 0;

            try
            {
                var db = AnagraficaManager.GetDb();

                if (Request.Files != null && Request.Files.Count > 0)
                {
                    for (int idx = 0; idx < Request.Files.Count; idx++)
                    {
                        var file2 = Request.Files[idx];
                        if (file2.ContentLength > 0)
                        {
                            fileName = Path.GetFileName(file2.FileName);

                            byte[] data;
                            using (Stream inputStream = file2.InputStream)
                            {
                                MemoryStream memoryStream = inputStream as MemoryStream;
                                if (memoryStream == null)
                                {
                                    memoryStream = new MemoryStream();
                                    inputStream.CopyTo(memoryStream);
                                }
                                data = memoryStream.ToArray();
                            }
                            length = data.Length;
                            string est = Path.GetExtension(fileName);
                            string tipoFile = MimeTypeMap.GetMimeType(est);
                            XR_ALLEGATI allegato = new XR_ALLEGATI()
                            {
                                NomeFile = fileName,
                                MimeType = tipoFile,
                                Length = length,
                                ContentByte = data,
                                IsPrincipal = filePrincipale,
                                TipoFile = tipologiaFile
                            };

                            db.XR_ALLEGATI.Add(allegato);
                            db.SaveChanges();

                            result = allegato.Id;

                            if (!filePrincipale)
                            {
                                idAllegati.Add(allegato.Id);
                            }

                            if (result == 0)
                            {
                                throw new Exception("Errore nel salvataggio dei dati");
                            }
                        }
                    }

                    if (!filePrincipale)
                    {
                        string ids = String.Join(",", idAllegati.ToList());
                        return Json(new { success = true, responseText = ids }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = true, responseText = result.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    throw new Exception("Errore nel caricamento del file");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = "Errore nel caricamento dei files" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Metodo per disegnare una riga nella tabella
        /// contenente gli allegati temporanei durante
        /// la creazione di un documento
        /// </summary>
        /// <param name="idAllegato"></param>
        /// <param name="tipologiaDocuemnto"></param> Indica la tipologia del documento 1 -> Allegato,  2 -> Documenti a supporto
        /// <returns></returns>
        [HttpGet]
        public ActionResult DrawTRFile(string idAllegato, bool isPrincipal, int tipologiaDocumento, int tipologiaFile = 1)
        {
            Dematerializzazione_TRFileUploadVM model = new Dematerializzazione_TRFileUploadVM();
            model.Allegati = new List<XR_ALLEGATI>();
            model.Principale = isPrincipal;
            model.TipologiaDocumento = tipologiaDocumento;

            var db = AnagraficaManager.GetDb();
            List<int> ids = new List<int>();

            if (isPrincipal)
            {
                int id = int.Parse(idAllegato);
                ids.Add(id);
            }
            else
            {
                List<string> stringhe = new List<string>();
                stringhe.AddRange(idAllegato.Split(',').ToList());
                if (stringhe != null && stringhe.Any())
                {
                    foreach (var s in stringhe)
                    {
                        int id = int.Parse(s);
                        ids.Add(id);
                    }
                }
            }

            if (ids != null && ids.Any())
            {
                foreach (var id in ids)
                {
                    try
                    {
                        var a = db.XR_ALLEGATI.Where(w => w.Id.Equals(id) && w.TipoFile == tipologiaFile).FirstOrDefault();
                        if (a == null)
                        {
                            throw new Exception("Errore impossibile reperire il file");
                        }
                        model.Allegati.Add(a);
                    }
                    catch (Exception ex)
                    {
                        model = null;
                    }
                }
            }


            return View("~/Views/Dematerializzazione/subpartial/_TRFileUpload.cshtml", model);
        }

        /// <summary>
        /// Reperimento dell'oggetto XR_ALLEGATI a partire dal suo id
        /// </summary>
        /// <param name="idAllegato"></param>
        /// <returns></returns>
        public ActionResult GetAllegatoTemporaneo(int idAllegato)
        {
            XR_ALLEGATI allegato = new XR_ALLEGATI();
            var db = AnagraficaManager.GetDb();

            try
            {
                allegato = db.XR_ALLEGATI.Where(w => w.Id.Equals(idAllegato)).FirstOrDefault();
                if (allegato == null)
                {
                    throw new Exception("Errore impossibile reperire il file");
                }
            }
            catch (Exception ex)
            {
                allegato = null;
            }
            return View("~/Views/Dematerializzazione/subpartial/_Allegato_Viewer.cshtml", allegato);
        }
    
        public ActionResult GetTempMail(int idTemplate, int idDoc)
        {
            DematerializzazioneBozza bozza = new DematerializzazioneBozza();
            var db = AnagraficaManager.GetDb();

            try
            {
                bozza = InternalGetBozza(idTemplate, idDoc);
                if (bozza == null)
                {
                    throw new Exception("Errore impossibile reperire il file");
                }
            }
            catch (Exception ex)
            {
                bozza = null;
            }
            return View("~/Views/Dematerializzazione/subpartial/_Template_Viewer.cshtml", bozza);
        }

        /// <summary>
        /// Metodo per il reperimento del file da visualizzare 
        /// nella modale di anteprima, durante la creazione di un
        /// documento
        /// </summary>
        /// <param name="idAllegato"></param>
        /// <returns></returns>
        public ActionResult GetAllegato(int idAllegato)
        {
            XR_ALLEGATI allegato = new XR_ALLEGATI();
            var db = AnagraficaManager.GetDb();

            try
            {
                allegato = db.XR_ALLEGATI.Where(w => w.Id.Equals(idAllegato)).FirstOrDefault();
                if (allegato == null)
                {
                    throw new Exception("Errore impossibile reperire il file");
                }

                byte[] byteArray = null;

                if (allegato.ContentBytePDF != null)
                {
                    byteArray = allegato.ContentBytePDF;
                }
                else
                {
                    byteArray = allegato.ContentByte;
                }

                string nomefile = allegato.NomeFile;
                nomefile = nomefile.Replace(",", "_");
                MemoryStream pdfStream = new MemoryStream();
                pdfStream.Write(byteArray, 0, byteArray.Length);
                pdfStream.Position = 0;

                //var cd = new System.Net.Mime.ContentDisposition
                //{
                //    FileName = nomefile,
                //    Inline = true ,
                //};

                Response.AddHeader("Content-Disposition", "inline; filename=" + nomefile + ".pdf");
                //return File( byteArray , "application/pdf" );

                return new FileStreamResult(pdfStream, "application/pdf");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Metodo per la cancellazione di un allegato
        /// </summary>
        /// <param name="idAllegato"></param>
        /// <returns></returns>
        public ActionResult EliminaAllegato(int idAllegato)
        {
            dynamic showMessageString = string.Empty;
            XR_ALLEGATI allegato = new XR_ALLEGATI();
            var db = AnagraficaManager.GetDb();

            try
            {
                allegato = db.XR_ALLEGATI.Where(w => w.Id.Equals(idAllegato)).FirstOrDefault();
                if (allegato == null)
                {
                    throw new Exception("Errore impossibile reperire il file");
                }

                db.XR_ALLEGATI.Remove(allegato);

                // verifica se l'allegato è associato ad una versione
                // se si, deve eliminare tutti i record ad esso collegato
                var allegatiVersioni = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => w.IdAllegato.Equals(idAllegato)).ToList();

                if (allegatiVersioni != null && allegatiVersioni.Any())
                {
                    List<int> idVersioni = new List<int>();
                    idVersioni.AddRange(allegatiVersioni.Select(w => w.IdVersione).ToList());
                    db.XR_DEM_ALLEGATI_VERSIONI.RemoveWhere(w => w.IdAllegato.Equals(idAllegato));
                    db.XR_DEM_VERSIONI_DOCUMENTO.RemoveWhere(w => idVersioni.Contains(w.Id));
                }

                db.SaveChanges();

                return Json(showMessageString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404, "Errore durante la rimozione dell'allegato");
            }
        }

        /// <summary>
        /// Inserimento di un documento da parte dell'ufficio di gestione
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <note>METODO PER L'INSERIMENTO DI UNA NUOVA PRATICA 
        /// NON BADARE AL NOME CHE COL TEMPO HA PERSO LA SPECIFICITA' VERSO I SOLI DIPENDENTI</note>
        [HttpPost]
        [Obsolete("Rimpiazzato da InsertPratica")]
        public ActionResult InsertDocumentoVSDip(InsRicModel model)
        {
            RichiestaDoc richiesta = new RichiestaDoc();
            List<int> idAllegati = new List<int>();
            if (!String.IsNullOrEmpty(model.Allegati))
            {
                List<string> temp = model.Allegati.Split(',').ToList();
                foreach (var t in temp)
                {
                    idAllegati.Add(int.Parse(t));
                }
            }

            if (model.IdAllegatoPrincipale > 0)
            {
                bool presenteStessoAllegatoPrincipale = idAllegati.Contains(model.IdAllegatoPrincipale);
                if (!presenteStessoAllegatoPrincipale)
                {
                    //elimina il file perchè è stato sostituito
                }
            }

            try
            {
                if (model != null)
                {
                    XR_WKF_TIPOLOGIA WKF_Tipologia = null;
                    // calcolo della tipologia
                    if (String.IsNullOrEmpty(model.CustomAttrs) || model.CustomAttrs == "[]")
                    {
                        WKF_Tipologia = Get_WKF_Tipologia(model.TipologiaWKF);
                    }
                    else
                    {
                        // se i customAttrs sono valorizzati va calcolata la tipologia in base ad eventuali sotto
                        // categorie determinate dall'eccezione selezionata e salvata tra i custom attrs esempio
                        // DEMDOC_VSRUO_CPM, può anche avere una sotto categoria DEMDOC_VSRUO_CPM_ON
                        WKF_Tipologia = Get_WKF_Tipologia(model.TipologiaWKF, model.CustomAttrs);
                    }

                    if (WKF_Tipologia == null)
                    {
                        throw new Exception("Workflow non trovato");
                    }

                    int stato = DematerializzazioneManager.GetNextIdStato(0, WKF_Tipologia.ID_TIPOLOGIA);

                    if (stato == 0)
                    {
                        throw new Exception("Errore nel reperimento dello stato successivo del workflow");
                    }

                    XR_DEM_TIPI_DOCUMENTO _tempXR_DEM_TIPI_DOCUMENTO = Get_XR_DEM_TIPI_DOCUMENTO(model.TipologiaDocumento);

                    if (_tempXR_DEM_TIPI_DOCUMENTO == null)
                    {
                        throw new Exception("Tipo documento non trovato");
                    }

                    model.Descrizione = _tempXR_DEM_TIPI_DOCUMENTO.Descrizione;

                    if (!String.IsNullOrEmpty(model.MatricolaApprovatore) &&
                        (model.MatricolaApprovatore == "-1" || model.MatricolaApprovatore == "undefined"))
                    {
                        model.MatricolaApprovatore = null;
                    }

                    if (!String.IsNullOrEmpty(model.IncaricatoFirma) &&
                        (model.IncaricatoFirma == "-1" || model.IncaricatoFirma == "undefined"))
                    {
                        model.IncaricatoFirma = null;
                    }

                    model.IdPersona = CommonHelper.GetCurrentIdPersona();
                    model.Matricola = UtenteHelper.Matricola();

                    int? idPersonaDestinatario = null;

                    if (!String.IsNullOrEmpty(model.MatricolaDestinatario))
                    {
                        idPersonaDestinatario = CezanneHelper.GetIdPersona(model.MatricolaDestinatario);
                    }

                    List<AttributiAggiuntivi> objModuloValorizzato = null;
                    if (!String.IsNullOrEmpty(model.CustomAttrs) && model.CustomAttrs != "[]")
                    {
                        objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(model.CustomAttrs);
                    }

                    if (stato == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInCreazione && !String.IsNullOrEmpty(model.CustomAttrs) && model.CustomAttrs != "[]")
                    {
                        var findRiferimentoRichiestaMaternita = objModuloValorizzato.Where(w => w.DBRefAttribute == "OGGETTO").FirstOrDefault();
                        if (findRiferimentoRichiestaMaternita != null)
                        {
                            model.Descrizione = findRiferimentoRichiestaMaternita.Valore;
                            stato = DematerializzazioneManager.GetNextIdStato((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInCreazione, WKF_Tipologia.ID_TIPOLOGIA);

                            if (stato == 0)
                            {
                                throw new Exception("Errore nel reperimento dello stato successivo del workflow");
                            }
                        }
                    }

                    if (_tempXR_DEM_TIPI_DOCUMENTO.Codice == "VCON" && objModuloValorizzato != null)
                    {
                        #region CREAZIONE DATI JSON

                        XR_DEM_TIPIDOC_COMPORTAMENTO comportamento = null;
                        IncentiviEntities db = new IncentiviEntities();
                        int id_Tipo_Doc = 0;
                        string cod_tipo_doc = "";
                        string descrizione = "";
                        const string COD_TIPOLOGIA_DOC = "VSRUO";

                        id_Tipo_Doc = _tempXR_DEM_TIPI_DOCUMENTO.Id;
                        cod_tipo_doc = _tempXR_DEM_TIPI_DOCUMENTO.Codice.Trim();
                        descrizione = _tempXR_DEM_TIPI_DOCUMENTO.Descrizione.Trim();

                        comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(
                            w => w.Codice_Tipologia_Documentale.Equals(COD_TIPOLOGIA_DOC) &&
                            w.Codice_Tipo_Documento.Equals(cod_tipo_doc)).FirstOrDefault();

                        if (comportamento == null)
                        {
                            throw new Exception("Comportamento non trovato");
                        }

                        List<AttributiAggiuntivi> objBASE = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(comportamento.CustomDataJSON);
                        List<AttributiAggiuntivi> objFinale = new List<AttributiAggiuntivi>();

                        foreach (var o in objModuloValorizzato)
                        {
                            AttributiAggiuntivi obj = objBASE.Where(w => w.Id == o.Id).FirstOrDefault();
                            o.TagSINTESI1 = obj.TagSINTESI1;

                            var tipo = o.Tipo;
                            string tag = o.TagSINTESI1;

                            if (!String.IsNullOrEmpty(tag))
                            {
                                switch (tag)
                                {
                                    case "DES_SEDE":
                                        o.Title = DematerializzazioneManager.GetDescrizioneSede(o.Valore);
                                        break;
                                    case "DES_SERVIZIO":
                                        o.Title = DematerializzazioneManager.GetDescrizioneServizio(o.Valore);
                                        break;
                                    case "COD_UNITAORG": // SEZIONE
                                        o.Title = DematerializzazioneManager.GetDescrizioneSezione_UnitaOrganizzativa(o.Valore);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            if (!String.IsNullOrEmpty(o.Id))
                            {
                                switch (o.Id.ToUpper())
                                {
                                    case "INDENNITA":
                                        o.Title = DematerializzazioneManager.GetDescrizioneIndennita(o.Valore);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            o.ValoreInModifica = o.Valore;
                            objFinale.Add(o);
                        }

                        // DEVE CALCOLARE IL GIUSTO CODICE EVENTO IN BASE AI DATI INSERITI
                        // RECUPERO DELLE INFO UTILI AI FINI DEL CALCOLO DELLA TIPOLOGIA DI RICHIESTA
                        var findItem = objModuloValorizzato.Where(w => w.Id.ToUpper() == "ECCEZIONEPERAUTOMATISMO").FirstOrDefault();

                        if (findItem != null)
                        {
                            DateTime dataDefinitiva = new DateTime(2999, 12, 31);

                            //// verifica che tipo di eccezione è stata registrata
                            //if (findItem.Valore == "3R" || findItem.Valore == "3T")
                            //{
                            List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

                            if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                            {
                                foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
                                {
                                    listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
                                }
                            }

                            var tipoVariazione = objModuloValorizzato.Where(w => w.Id.ToUpper() == "TIPOVARIAZIONE").FirstOrDefault();
                            var data_decorrenza = listaPiatta.Where(w => w.Id.ToUpper() == "DATA_DECORRENZA").FirstOrDefault();
                            var data_scadenza = listaPiatta.Where(w => w.Id.ToUpper() == "DATA_SCADENZA").FirstOrDefault();
                            var automatismoUpdate = objModuloValorizzato.Where(w => w.Id.ToUpper() == "ECCEZIONEPERAUTOMATISMO").FirstOrDefault();
                            var selezionataUpdate = objModuloValorizzato.Where(w => w.Id.ToUpper() == "ECCEZIONESELEZIONATANASCOSTA").FirstOrDefault();

                            if (tipoVariazione == null)
                            {
                                throw new Exception("Tipo variazione non trovata");
                            }

                            if (data_decorrenza == null || String.IsNullOrWhiteSpace(data_decorrenza.Valore))
                            {
                                throw new Exception("Errore nel reperimento della data di decorrenza");
                            }

                            if (data_scadenza == null)
                            {
                                throw new Exception("Errore nel reperimento della data di scadenza");
                            }

                            if (automatismoUpdate == null || String.IsNullOrWhiteSpace(automatismoUpdate.Valore))
                            {
                                throw new Exception("Errore nel reperimento della tipologia di assegnazione");
                            }

                            if (selezionataUpdate == null || String.IsNullOrWhiteSpace(selezionataUpdate.Valore))
                            {
                                throw new Exception("Errore nel reperimento della tipologia di assegnazione");
                            }

                            DateTime dtInizioSelezionata;
                            DateTime? dtFineSelezionata = null;

                            bool conversioneData = DateTime.TryParse(data_decorrenza.Valore, out dtInizioSelezionata);

                            if (!conversioneData)
                            {
                                throw new Exception("Errore in conversione data");
                            }

                            if (String.IsNullOrWhiteSpace(data_scadenza.Valore))
                            {
                                // se non è selezionata allora è definitiva
                                dtFineSelezionata = dataDefinitiva;
                            }
                            else
                            {
                                DateTime _tmpDate;
                                conversioneData = DateTime.TryParse(data_scadenza.Valore, out _tmpDate);
                                if (!conversioneData)
                                {
                                    throw new Exception("Errore in conversione data");
                                }
                                dtFineSelezionata = _tmpDate;
                            }

                            /*
                                30	ASSEGNAZIONE
                                3R	ASSEGNAZIONE TEMPORANEA
                                3S	FINE ASSEGNAZIONE TEMPORANEA (anche anticipata)

                                3W	TRASFERIMENTO DEFINITIVO
                                3T	TRASFERIMENTO TEMPORANEO
                                3U	FINE TRASFERIMENTO TEMPORANEO (anche anticipata)
                                34	TRASFERIMENTO A DOMANDA

                                3K	PROROGA ASSEGNAZIONE
                                3Y	PROROGA TRASFERIMENTO

                                31	DISTACCO
                                3X	FINE DISTACCO

                                3Z	VARIAZIONE SEZIONE
                             */

                            List<string> codEventi = new List<string>();
                            codEventi.Add("3R");    //ASSEGNAZIONE TEMPORANEA
                            codEventi.Add("3T");    //TRASFERIMENTO TEMPORANEO
                            codEventi.Add("3K");    //PROROGA ASSEGNAZIONE
                            codEventi.Add("3Y");    //PROROGA TRASFERIMENTO
                            var ultimaAssegnazione = GetVariazione(idPersonaDestinatario.Value, codEventi);

                            if (ultimaAssegnazione != null)
                            {
                                string nuovoCodice = GetNuovoCodiceVariazione_TEMPORANEAPRESENTE(idPersonaDestinatario.Value, dtFineSelezionata.GetValueOrDefault());
                                automatismoUpdate.Valore = nuovoCodice;
                                automatismoUpdate.ValoreInModifica = nuovoCodice;
                                selezionataUpdate.Valore = nuovoCodice;
                                selezionataUpdate.ValoreInModifica = nuovoCodice;
                            }

                            tipoVariazione.Valore = DematerializzazioneManager.GetDescrizioneEvento(automatismoUpdate.Valore, true);
                            //}
                        }

                        string JSON = JsonConvert.SerializeObject(objFinale);
                        model.CustomAttrs = JSON;
                        #endregion
                    }

                    richiesta.Documento = new XR_DEM_DOCUMENTI()
                    {
                        Descrizione = model.Descrizione,
                        Note = model.Note,
                        Id_Stato = stato,
                        Id_Tipo_Doc = _tempXR_DEM_TIPI_DOCUMENTO.Id,
                        MatricolaCreatore = model.Matricola,
                        IdPersonaCreatore = model.IdPersona,
                        MatricolaDestinatario = model.MatricolaDestinatario,
                        Id_WKF_Tipologia = WKF_Tipologia.ID_TIPOLOGIA,
                        Cod_Tipologia_Documentale = model.TipologiaDocumentale,
                        MatricolaApprovatore = model.MatricolaApprovatore,
                        MatricolaFirma = model.IncaricatoFirma,
                        MatricolaIncaricato = model.MatricolaIncaricato,
                        Id = model.IdDocumento,
                        IdPersonaDestinatario = idPersonaDestinatario,
                        CustomDataJSON = model.CustomAttrs
                    };

                    int result = InsertDocumentoVSDipendenteInternal(richiesta, idAllegati);

                    if (result == 0)
                    {
                        throw new Exception("Errore nel salvataggio dei dati");
                    }

                    return Json(new { success = true, responseText = result.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = "Errore nell'inserimento della richiesta" }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        /// <summary>
        /// Imposta il documento come attivo
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="idPersona"></param>
        /// <param name="idDoc"></param>
        /// <returns></returns>
        public ActionResult SetCompleted(string matricola, int idPersona, int idDoc)
        {
            dynamic showMessageString = string.Empty;

            SezioniAnag sezione = SezioniAnag.NonDefinito;
            var anagrafica = AnagraficaManager.GetAnagrafica(matricola, new AnagraficaLoader(sezione));

            try
            {
                var db = AnagraficaManager.GetDb();
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (item != null)
                {
                    item.PraticaAttiva = true;

                    db.SaveChanges();
                }

                showMessageString = new
                {
                    param1 = 200,
                    param2 = "OK"
                };

                return Json(showMessageString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404, "Errore nel salvataggio dei dati.");
            }
        }

        /// <summary>
        /// Download del file principale associato ad un documento
        /// </summary>
        /// <param name="idDoc"></param>
        /// <returns></returns>
        public ActionResult ScaricaPDF(int idDoc)
        {
            try
            {
                RichiestaDoc documento = GetDocumentData(idDoc);

                string nomeFile = "prova.pdf";
                Stream stream = null;

                var data = documento.Allegati.Where(w => w.IsPrincipal).FirstOrDefault();
                if (data != null)
                {
                    nomeFile = data.NomeFile;
                    stream = new MemoryStream(data.ContentByte);
                }
                else
                {
                    throw new Exception("Impossibile trovare il file desiderato");
                }

                return new FileStreamResult(stream, "application/pdf") { FileDownloadName = nomeFile };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Download del file con id = idAllegato
        /// </summary>
        /// <param name="idAllegato"></param>
        /// <returns></returns>
        public ActionResult ScaricaPDFASupporto(int idAllegato)
        {
            try
            {
                var db = AnagraficaManager.GetDb();
                var item = db.XR_ALLEGATI.Where(w => w.Id.Equals(idAllegato)).FirstOrDefault();
                string nomeFile = "prova.pdf";
                Stream stream = null;

                if (item != null)
                {
                    nomeFile = item.NomeFile;
                    stream = new MemoryStream(item.ContentByte);
                }
                else
                {
                    throw new Exception("Impossibile trovare il file desiderato");
                }

                return new FileStreamResult(stream, "application/pdf") { FileDownloadName = nomeFile };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Render dei dati di riepilogo del documento che si sta creando
        /// Questo metodo viene chiamato per popolare il div del tab3
        /// del wizard di creazione documento
        /// </summary>
        /// <param name="idDoc">Identificativo del documento</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetDataPerRiepilogo(int idDoc)
        {
            IncentiviEntities db = new IncentiviEntities();
            var documento = GetDocumentData(idDoc);
            RiepilogoVM model = new RiepilogoVM();
            model.Documento = documento;
            model.NominativoUtenteDestinatario = DematerializzazioneManager.GetNominativoByMatricola(documento.Documento.MatricolaDestinatario);
            model.NominativoUtenteIncaricatoFirma = DematerializzazioneManager.GetNominativoByMatricola(documento.Documento.MatricolaFirma);
            model.NominativoUtenteCreatore = DematerializzazioneManager.GetNominativoByMatricola(documento.Documento.MatricolaCreatore);
            model.NominativoUtenteApprovatore = DematerializzazioneManager.GetNominativoByMatricola(documento.Documento.MatricolaApprovatore);
            model.NominativoUtenteIncaricato = DematerializzazioneManager.GetNominativoByMatricola(documento.Documento.MatricolaIncaricato);
            model.NominativoUtenteVistatore = new List<string>();

            string matricola = UtenteHelper.Matricola();
            string tipologiaDocumentale = (string)SessionHelper.Get(matricola + "tipologiaDocumentale");
            string tipologiaDocumento = (string)SessionHelper.Get(matricola + "tipologiaDocumento");
            var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

            if (comportamento == null)
            {
                throw new Exception("Comportamento non trovato");
            }

            /*
             * Se da comportamento è previsto un vistatore, ma sulla tabella non vi è alcuna matricola.
             * cerca tra tutti i vistatori quelli che hanno diritti sulla pratica
             */
            if (comportamento.Visionatore && String.IsNullOrEmpty(documento.Documento.MatricolaVisualizzatore))
            {
                List<NominativoMatricola> items = null;
                string tipologia = String.Format("{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());
                items = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

                if (items != null && items.Any())
                {
                    foreach (var i in items)
                    {
                        string _tempNominativo = "";
                        var r = AuthHelper.EnableToMatr(i.Matricola, matricola, "DEMA", "01VIST");
                        if (r.Enabled)
                        {
                            _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                        }
                        // se non c'è matricola, allora è una tipologia come ad esempio 
                        // APPUNTO, che non ha matricola dipendente, ma ha comunque un vistatore
                        var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                        .Where(w => w.MATRICOLA == i.Matricola && w.IND_ATTIVO)
                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01VIST")
                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                        .FirstOrDefault();

                        if (_myAbil != null)
                        {
                            #region POSSO VISTATORE 
                            //Calcolo degli elementi che posso firmare
                            List<string> possoVistare = new List<string>();

                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                            {
                                possoVistare.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                            }

                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                                && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                            {
                                possoVistare.Add(_myAbil.TIP_DOC_INCLUSI);
                            }

                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                            {
                                possoVistare.Add(tipologia);
                            }
                            #endregion

                            #region NON POSSO VISTATORE 
                            //Calcolo degli elementi che non posso vistare
                            List<string> nonPossoVistare = new List<string>();

                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                            {
                                nonPossoVistare.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                            }

                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                                && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                            {
                                nonPossoVistare.Add(_myAbil.TIP_DOC_ESCLUSI);
                            }

                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                            {
                                // se però possoVistare contiene tipologia allora non va aggiunta
                                if (!possoVistare.Contains(tipologia))
                                {
                                    nonPossoVistare.Add(tipologia);
                                }
                            }
                            #endregion

                            #region CASO LIMITE TUTTI E DUE *
                            if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                                !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                    && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                                    && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                            {
                                possoVistare = new List<string>();
                                nonPossoVistare = new List<string>();
                            }
                            #endregion

                            List<string> tipologieAbilitate = possoVistare.Except(nonPossoVistare).ToList();

                            if (tipologieAbilitate != null &&
                                tipologieAbilitate.Any())
                            {
                                if (tipologieAbilitate.Contains(tipologia))
                                {
                                    _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim())) + "<br/>";
                                }
                                else
                                {
                                    _tempNominativo = "";
                                }
                            }
                        }

                        if (!String.IsNullOrEmpty(_tempNominativo))
                        {
                            model.NominativoUtenteVistatore.Add(_tempNominativo);
                        }
                    }
                }
            }
            else if (comportamento.Visionatore && !String.IsNullOrEmpty(documento.Documento.MatricolaVisualizzatore) &&
                documento.Documento.MatricolaVisualizzatore.Contains(','))
            {
                foreach (var i in documento.Documento.MatricolaVisualizzatore.Split(','))
                {
                    model.NominativoUtenteVistatore.Add(DematerializzazioneManager.GetNominativoByMatricola(i));
                }
            }
            else if (comportamento.Visionatore && !String.IsNullOrEmpty(documento.Documento.MatricolaVisualizzatore) &&
                !documento.Documento.MatricolaVisualizzatore.Contains(','))
            {
                model.NominativoUtenteVistatore.Add(DematerializzazioneManager.GetNominativoByMatricola(documento.Documento.MatricolaVisualizzatore));
            }

            if (!String.IsNullOrEmpty(documento.Documento.CustomDataJSON))
            {
                // se ci sono dati custom nel documento esaminato
                // carica il json di default che si trova sulla tabella dei comportamenti

                List<AttributiAggiuntivi> objJsonComportamento = null;
                objJsonComportamento = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(comportamento.CustomDataJSON);

                // per gli elementi di tipo checkbox essendo una struttura ibrida tra una input e una select, il 
                // sistema non riesce a recuperare il selectlistitem quindi va preso dall'elemento di default in
                // comportamento e riversato nell'attributo in model.attributi

                List<AttributiAggiuntivi> objD = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(documento.Documento.CustomDataJSON);
                model.Attributi = objD;

                bool esisteAlmenoUnCheckBox = model.Attributi.Where(w => w.Tipo == TipologiaAttributoEnum.Check).Count() > 0;

                if (esisteAlmenoUnCheckBox)
                {
                    foreach (var check in model.Attributi.Where(w => w.Tipo == TipologiaAttributoEnum.Check).ToList())
                    {
                        var _tempComportamento = objJsonComportamento.FirstOrDefault(w => w.Id == check.Id.Replace("check_", ""));
                        if (_tempComportamento != null)
                        {
                            check.SelectListItems = _tempComportamento.SelectListItems;
                            if (check.SelectListItems != null)
                            {
                                var firstsel = check.SelectListItems.FirstOrDefault();
                                if (firstsel != null)
                                {
                                    firstsel.Selected = check.Checked;
                                }
                            }
                        }
                    }
                }
            }

            model.Note = GetNoteDocumento(idDoc);

            return View("~/Views/Dematerializzazione/subpartial/_tab3Riepilogo.cshtml", model);
        }

        [HttpGet]
        public ActionResult GetDettaglioRichiesta(string m, int id, int idDoc, bool approvatoreEnabled = false, bool presaInCaricoEnabled = false, bool presaInVisioneEnabled = false)
        {
            IncentiviEntities db = new IncentiviEntities();
            var item = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();
            DateTime ora = DateTime.Now;
            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM();
            model.IsOperatore = DematerializzazioneManager.IsOperatore();
            model.IsApprovatore = DematerializzazioneManager.IsApprovatore();
            model.IsSegreteria = DematerializzazioneManager.IsSegreteria();
            model.ReportInserimentiEccezioniInGapp = new ReportMyRaiPianoFerieBatch();

            if (item == null)
            {
                throw new Exception("Impossibile reperire il documento");
            }

            if (String.IsNullOrEmpty(m))
            {
                m = UtenteHelper.Matricola();
            }

            if (id == 0)
            {
                id = CommonHelper.GetCurrentIdPersona();
            }

            if (item.DataPresaInModifica.HasValue)
            {
                if (item.MatricolaPresaInModifica != m)
                {
                    if ((ora - item.DataPresaInModifica.Value).TotalMinutes <= 15)
                    {
                        model.MessaggioPraticaBloccata = true;
                        string nominativo = DematerializzazioneManager.GetNominativoByMatricola(item.MatricolaPresaInModifica);
                        model.Msg1 = "Pratica in modifica da " + nominativo;
                        model.Msg2 = nominativo + " potrebbe star modificando questa pratica. Attendi che venga rilasciata per lavorarla";
                    }
                    else if ((ora - item.DataPresaInModifica.Value).TotalMinutes > 15)
                    {
                        model.MessaggioPraticaBloccata = false;
                        model.Msg1 = "";
                        model.Msg2 = "";
                    }
                }
            }

            model.Richiesta = GetDocumentData(idDoc);
            model.ApprovazioneEnabled = approvatoreEnabled;
            model.PresaInCaricoEnabled = presaInCaricoEnabled;
            model.PresaInVisioneEnabled = presaInVisioneEnabled;
            model.Tabs = new List<Dematerializzazione_Tab_Dettaglio_Pratica>();
            model.ProssimoStato = DematerializzazioneManager.GetNextIdStato(model.Richiesta.Documento.Id_Stato, model.Richiesta.Documento.Id_WKF_Tipologia, true);

            if (model.Richiesta != null)
            {
                if (model.Richiesta.Documento.Id_Stato < (int)StatiDematerializzazioneDocumenti.Accettato)
                {
                    model.AbilitaModifica = true;
                }

                string tipologiaDocumentale = model.Richiesta.Documento.Cod_Tipologia_Documentale;
                int id_tipo_doc = model.Richiesta.Documento.Id_Tipo_Doc;
                string tipologiaDocumento = "";

                var dem_TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(id_tipo_doc)).FirstOrDefault();

                if (dem_TIPI_DOCUMENTO != null)
                {
                    tipologiaDocumento = dem_TIPI_DOCUMENTO.Codice;
                }

                model.Matricola = m;
                model.IdPersona = id;

                model.NominativoUtenteApprovatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaApprovatore);
                model.NominativoUtenteCreatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaCreatore);
                model.NominativoUtenteDestinatario = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaDestinatario);
                model.NominativoUtenteFirma = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaFirma);
                model.NominativoUtenteIncaricato = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaIncaricato);

                model.NominativoUtenteVistatore = new List<string>();

                var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                if (comportamento == null)
                {
                    throw new Exception("Comportamento non trovato");
                }

                if (!String.IsNullOrEmpty(model.Richiesta.Documento.MatricolaVisualizzatore))
                {
                    if (model.Richiesta.Documento.MatricolaVisualizzatore.Contains(','))
                    {
                        foreach (var i in model.Richiesta.Documento.MatricolaVisualizzatore.Split(',').ToList())
                        {
                            model.NominativoUtenteVistatore.Add(DematerializzazioneManager.GetNominativoByMatricola(i));
                        }
                    }
                    else
                    {
                        model.NominativoUtenteVisionatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaVisualizzatore);
                    }
                }
                else
                {
                    if (comportamento.Visionatore)
                    {
                        string matricola = UtenteHelper.Matricola();
                        List<NominativoMatricola> items = null;
                        string tipologia = String.Format("{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());
                        items = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

                        if (items != null && items.Any())
                        {
                            foreach (var i in items)
                            {
                                string _tempNominativo = "";
                                var r = AuthHelper.EnableToMatr(i.Matricola, item.MatricolaCreatore, "DEMA", "01VIST");
                                if (r.Enabled)
                                {
                                    _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                                }
                                // se non c'è matricola, allora è una tipologia come ad esempio 
                                // APPUNTO, che non ha matricola dipendente, ma ha comunque un vistatore
                                var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                                .Where(w => w.MATRICOLA == i.Matricola && w.IND_ATTIVO)
                                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01VIST")
                                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                                .FirstOrDefault();

                                if (_myAbil != null)
                                {
                                    #region POSSO VISTATORE 
                                    //Calcolo degli elementi che posso firmare
                                    List<string> possoVistare = new List<string>();

                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                        && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                                    {
                                        possoVistare.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                                    }

                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                        && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                                        && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                                    {
                                        possoVistare.Add(_myAbil.TIP_DOC_INCLUSI);
                                    }

                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                        && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                                    {
                                        possoVistare.Add(tipologia);
                                    }
                                    #endregion

                                    #region NON POSSO VISTATORE 
                                    //Calcolo degli elementi che non posso vistare
                                    List<string> nonPossoVistare = new List<string>();

                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                        && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                                    {
                                        nonPossoVistare.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                                    }

                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                        && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                                        && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                    {
                                        nonPossoVistare.Add(_myAbil.TIP_DOC_ESCLUSI);
                                    }

                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                        && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                    {
                                        // se però possoVistare contiene tipologia allora non va aggiunta
                                        if (!possoVistare.Contains(tipologia))
                                        {
                                            nonPossoVistare.Add(tipologia);
                                        }
                                    }
                                    #endregion

                                    #region CASO LIMITE TUTTI E DUE *
                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                                        !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                            && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                                            && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                    {
                                        possoVistare = new List<string>();
                                        nonPossoVistare = new List<string>();
                                    }
                                    #endregion

                                    List<string> tipologieAbilitate = possoVistare.Except(nonPossoVistare).ToList();

                                    if (tipologieAbilitate != null &&
                                        tipologieAbilitate.Any())
                                    {
                                        if (tipologieAbilitate.Contains(tipologia))
                                        {
                                            _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim())) + "<br/>";
                                        }
                                        else
                                        {
                                            _tempNominativo = "";
                                        }
                                    }
                                }

                                if (!String.IsNullOrEmpty(_tempNominativo))
                                {
                                    model.NominativoUtenteVistatore.Add(_tempNominativo);
                                }
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(model.Richiesta.Documento.CustomDataJSON))
                {
                    string codEccezione = "";
                    List<AttributiAggiuntivi> objD = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(model.Richiesta.Documento.CustomDataJSON);

                    var tt = objD.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                    if (tt != null)
                    {
                        // esempio codEccezione = MT
                        codEccezione = tt.Valore;
                    }

                    if (String.IsNullOrEmpty(codEccezione))
                    {
                        tt = objD.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                        if (tt != null)
                        {
                            codEccezione = tt.Valore;
                        }
                    }

                    if (!String.IsNullOrEmpty(codEccezione))
                    {
                        string matricoleContatore = CommonManager.GetParametro<string>(EnumParametriSistema.DematerializzazioneEccezioniPerContatore);

                        if (!String.IsNullOrEmpty(matricoleContatore))
                        {
                            // esempio HC, GM
                            if (matricoleContatore.Split(',').Contains(codEccezione))
                            {
                                // se l'eccezione è inclusa tra quelle per le quali va mostrato il contatore
                                // allora per prima cosa controlliamo che sulla tabella XR_MAT_ARRETRATI_DIPENDENTE
                                // ci sia tale eccezione, se si significa che è già stata importata da HRDW
                                var verificaHC = db.XR_MAT_ARRETRATI_DIPENDENTE.Where(w => w.MATRICOLA.Equals(model.Richiesta.Documento.MatricolaDestinatario) &&
                                                                                    w.ECCEZIONE.Equals("HC")).OrderBy(x => x.DATA).ToList();

                                var verificaGM = db.XR_MAT_ARRETRATI_DIPENDENTE.Where(w => w.MATRICOLA.Equals(model.Richiesta.Documento.MatricolaDestinatario) &&
                                                    w.ECCEZIONE.Equals("GM")).OrderBy(x => x.DATA).ToList();

                                if (verificaHC == null || !verificaHC.Any() || verificaGM == null || !verificaGM.Any())
                                {
                                    // se vuoto allora deve importare l'informazione da HRDW
                                    string r = MaternitaCongediManager.ImportaArretratiMatricola(model.Richiesta.Documento.MatricolaDestinatario);


                                    verificaHC = db.XR_MAT_ARRETRATI_DIPENDENTE.Where(w => w.MATRICOLA.Equals(model.Richiesta.Documento.MatricolaDestinatario) &&
                                                                                        w.ECCEZIONE.Equals("HC")).OrderBy(x => x.DATA).ToList();

                                    verificaGM = db.XR_MAT_ARRETRATI_DIPENDENTE.Where(w => w.MATRICOLA.Equals(model.Richiesta.Documento.MatricolaDestinatario) &&
                                                        w.ECCEZIONE.Equals("GM")).OrderBy(x => x.DATA).ToList();
                                }

                                if ((verificaHC != null && verificaHC.Any()) || (verificaGM != null && verificaGM.Any()))
                                {
                                    int ordine = objD.Count();
                                    ordine += 1;

                                    decimal sumHC = verificaHC.Sum(w => w.QUANTITA);
                                    decimal sumGM = verificaGM.Sum(w => w.QUANTITA);

                                    decimal totale = sumHC + sumGM;

                                    objD.Add(new AttributiAggiuntivi()
                                    {
                                        Azioni = null,
                                        Checked = false,
                                        DBRefAttribute = null,
                                        Gruppo = null,
                                        Id = "ContatoreArretrati",
                                        Label = "Giorni fruiti ad oggi (GM + HC)",
                                        MaxLength = 0,
                                        MinLength = 0,
                                        Nome = null,
                                        OnChange = null,
                                        OnClick = null,
                                        OnSelect = null,
                                        Ordinamento = ordine,
                                        Required = false,
                                        SelectListItems = null,
                                        TagHRDW = null,
                                        Testo = null,
                                        Tipo = TipologiaAttributoEnum.Testo,
                                        Title = null,
                                        UrlLoadData = null,
                                        Valore = totale.ToString("0.00"),
                                        ValoreInModifica = null,
                                        Visible = true
                                    });
                                }
                            }
                        }
                    }

                    model.Attributi = objD;
                }

                model.IsDuplicable = true;

                if (tipologiaDocumento == "ASS")
                {
                    model.AbilitaModifica = false;
                    model.NascondiConcludiPratica = true;
                    model.NascondiEliminaPratica = true;
                    model.IsDuplicable = false;
                }

                if (tipologiaDocumento == "VCON")
                {
                    model.AbilitaModifica = false;
                    model.NascondiConcludiPratica = true;
                    model.NascondiEliminaPratica = true;
                    model.IsDuplicable = false;
                    model.NascondiRiprendi = true;

                    if (model.PresaInCaricoEnabled &&
                        model.Richiesta.Documento.MatricolaIncaricato == UtenteHelper.Matricola())
                    {
                        model.NascondiConcludiPratica = false;
                    }
                }

                if (!String.IsNullOrEmpty(comportamento.ModelloJsonAlternativo) &&
                    comportamento.ModelloJsonAlternativo != "[]" &&
                    DematerializzazioneManager.IsSegreteria())
                {
                    model.IsDuplicable = false;
                    model.AbilitaModifica = true;
                }

                if (model.Richiesta.Documento.Id_Stato == (int)StatiDematerializzazioneDocumenti.PraticaConclusa)
                {
                    model.AbilitaModifica = false;
                    model.NascondiConcludiPratica = true;
                    model.NascondiEliminaPratica = true;
                    model.IsDuplicable = false;
                    model.NascondiRiprendi = true;
                }

                // verifica se ci sono log da mostrare per il documento in corso
                // per prima cosa verifica se il documento è in uno stato successivo ad approvato
                // in quanto solo dopo quello stato abbiamo le azioni di firma che producono la log in questione
                if (model.Richiesta.Documento.Id_Stato >= (int)StatiDematerializzazioneDocumenti.Accettato)
                {
                    bool existsLog = db.XR_DEM_LOG.Where(w => w.IdDocumento == model.Richiesta.Documento.Id &&
                                    (
                                        w.IdStato == (int)XR_DEM_LOG_STATI_ENUM.ErroreInInvioMail ||
                                        w.IdStato == (int)XR_DEM_LOG_STATI_ENUM.MailInviata ||
                                        w.IdStato == (int)XR_DEM_LOG_STATI_ENUM.DocumentoProtocollatoEFirmatoCorrettamente ||
                                        w.IdStato == (int)XR_DEM_LOG_STATI_ENUM.DocumentoInviatoAlDipendente
                                    )).Count() > 0;

                    model.AbilitaPannelloLog = existsLog;

                    if (existsLog)
                    {
                        model.EventiLog = new List<EventoLog>();
                        var items = db.XR_DEM_LOG.Where(w => w.IdDocumento == model.Richiesta.Documento.Id &&
                                    (
                                        w.IdStato == (int)XR_DEM_LOG_STATI_ENUM.ErroreInInvioMail ||
                                        w.IdStato == (int)XR_DEM_LOG_STATI_ENUM.MailInviata ||
                                        w.IdStato == (int)XR_DEM_LOG_STATI_ENUM.DocumentoProtocollatoEFirmatoCorrettamente ||
                                        w.IdStato == (int)XR_DEM_LOG_STATI_ENUM.DocumentoInviatoAlDipendente
                                    )).ToList().OrderBy(w => w.DataOperazione).ToList();

                        foreach (var i in items)
                        {
                            XR_DEM_LOG_STATI_ENUM en = (XR_DEM_LOG_STATI_ENUM)i.IdStato;
                            EventoLog e = new EventoLog()
                            {
                                DataEvento = i.DataOperazione,
                                Descrizione = en.GetAmbientValue()
                            };

                            model.EventiLog.Add(e);
                        }
                    }
                }

                // se il documento è legato ad una XR_MAT_RICHIESTA
                // allora mostrerà i dati aggiuntivi dello stato della XR_MAT_RICHIESTA
                if (model.Richiesta.Documento.Id_Richiesta.HasValue && !String.IsNullOrEmpty(model.Richiesta.Documento.CustomDataJSON) &&
                    model.Richiesta.Documento.CustomDataJSON != "[]")
                {
                    model.Dettaglio_Mat_Richieste = GetDatiAggiuntivi_Mat_Richieste(model.Richiesta.Documento.Id, model.Richiesta.Documento.Id_Richiesta.Value, model.Richiesta.Documento.CustomDataJSON);
                }

                int id_WKF_Tipologia = model.Richiesta.Documento.Id_WKF_Tipologia;
                int idStato = model.Richiesta.Documento.Id_Stato;
                var itemWKF = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(id_WKF_Tipologia) && w.ID_STATO.Equals(idStato)).FirstOrDefault();

                if (itemWKF != null)
                {
                    // bottone aggiungi info visibile
                    model.AbilitaAggiuntaInfoJson = !String.IsNullOrEmpty(itemWKF.CUSTOMDATAJSON);

                    if (model.AbilitaAggiuntaInfoJson)
                    {
                        List<AttributiAggiuntivi> objJson = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(itemWKF.CUSTOMDATAJSON);
                        model.AttributiAggiuntiWKF = objJson;

                        model.AttributiAggiuntiWKF.RemoveAll(w => model.Attributi.Select(x => x.Id).Contains(w.Id));

                        if (!model.AttributiAggiuntiWKF.Any())
                        {
                            // nasconde il bottone aggiungi info, in quanto 
                            // i campi aggiuntivi sono stati già valorizzati 
                            // in precedenza, infatti la lista coi campi aggiuntivi
                            // è vuota.
                            model.AbilitaAggiuntaInfoJson = false;
                            model.AbilitaModificaInfoJson = true;
                        }
                    }
                }

                model.ReportInserimentiEccezioniInGapp = null;
                model.Tabs = GetTabsPratica(idDoc);
                model.Note = GetNoteDocumento(idDoc);

                if (!String.IsNullOrEmpty(model.Richiesta.Documento.CustomDataJSON))
                {
                    // se ci sono dati custom nel documento esaminato
                    // carica il json di default che si trova sulla tabella dei comportamenti

                    List<AttributiAggiuntivi> objJsonComportamento = null;
                    objJsonComportamento = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(comportamento.CustomDataJSON);

                    // per gli elementi di tipo checkbox essendo una struttura ibrida tra una input e una select, il 
                    // sistema non riesce a recuperare il selectlistitem quindi va preso dall'elemento di default in
                    // comportamento e riversato nell'attributo in model.attributi

                    List<AttributiAggiuntivi> objD = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(model.Richiesta.Documento.CustomDataJSON);
                    model.Attributi = objD;

                    bool esisteAlmenoUnCheckBox = model.Attributi.Where(w => w.Tipo == TipologiaAttributoEnum.Check).Count() > 0;

                    if (esisteAlmenoUnCheckBox)
                    {
                        foreach (var check in model.Attributi.Where(w => w.Tipo == TipologiaAttributoEnum.Check).ToList())
                        {
                            var _tempComportamento = objJsonComportamento.FirstOrDefault(w => w.Id == check.Id.Replace("check_", ""));
                            if (_tempComportamento != null)
                            {
                                check.SelectListItems = _tempComportamento.SelectListItems;
                                if (check.SelectListItems != null)
                                {
                                    var firstsel = check.SelectListItems.FirstOrDefault();
                                    if (firstsel != null)
                                    {
                                        firstsel.Selected = check.Checked;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (model.PresaInVisioneEnabled)
            {
                // se deve mostrare il bottone per il visto, allora deve controllare che non sia già stato 
                // vistato l'intero documento
                if (model.Richiesta.Documento.DataVisto.HasValue)
                {
                    model.BottoneVistaDisabilitato = true;
                    model.InfoBottoneVistaDisabilitato = $"Vistato il {model.Richiesta.Documento.DataVisto.Value.ToString("dd/MM/yyyy HH:mm:ss")}";
                }

                // bisogna controllare nella tabella [XR_WKF_WORKFLOW_DYNAMIC_STEPS] se per la matricola
                // corrente esiste un record come vistatore e lo stato del visto
                var steps = db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Where(w => w.ID_DOCUMENTO == idDoc && w.ATTIVO && w.ID_STATO_PARTENZA == model.Richiesta.Documento.Id_Stato).ToList();
                if (steps != null && steps.Any())
                {
                    foreach (var s in steps)
                    {
                        // per ogni elemento prende il json e verifica se tra le matricole 
                        // vistatori c'è la matricola corrente
                        var _stringToJson = s.JSON_INPUT;
                        if (!String.IsNullOrEmpty(_stringToJson))
                        {
                            XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item _json = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item();
                            _json = JsonConvert.DeserializeObject<XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item>(_stringToJson);

                            if (_json != null && !String.IsNullOrEmpty(_json.MatricolaVistatore))
                            {
                                if (_json.MatricolaVistatore == m)
                                {
                                    if (_json.Data.HasValue && _json.Vistato)
                                    {
                                        model.PresaInVisioneEnabled = false;
                                        model.BottoneVistaDisabilitato = true;
                                        model.InfoBottoneVistaDisabilitato = $"Vistato il {_json.Data.Value.ToString("dd/MM/yyyy HH:mm:ss")}";
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return View("~/Views/Dematerializzazione/Modal_DettaglioRichiesta.cshtml", model);
        }

        private List<Dematerializzazione_Tab_Dettaglio_Pratica> GetTabsPratica(int idDoc)
        {
            List<Dematerializzazione_Tab_Dettaglio_Pratica> result = new List<Dematerializzazione_Tab_Dettaglio_Pratica>();

            IncentiviEntities db = new IncentiviEntities();

            var doc = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

            if (doc == null)
            {
                throw new Exception("Impossibile reperire la pratica");
            }

            int id_WKF_Tipologia = doc.Id_WKF_Tipologia;

            var tipologiaWKF = db.XR_WKF_TIPOLOGIA.Where(w => w.ID_TIPOLOGIA.Equals(id_WKF_Tipologia)).FirstOrDefault();

            if (tipologiaWKF == null)
            {
                throw new Exception("Impossibile reperire la tipologia di workflow associato alla pratica");
            }

            string menu = tipologiaWKF.CONFIG_TABS;

            if (!String.IsNullOrEmpty(menu) && menu != "[]")
            {
                List<Dematerializzazione_Config_Tab> objD = JsonConvert.DeserializeObject<List<Dematerializzazione_Config_Tab>>(menu);

                if (objD == null || !objD.Any())
                {
                    throw new Exception("Errore nel caricamento dello stato della pratica");
                }

                int count = 0;
                if (doc.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza)
                {
                    foreach (var t in objD)
                    {
                        Dematerializzazione_Tab_Dettaglio_Pratica m = new Dematerializzazione_Tab_Dettaglio_Pratica();
                        m.Title = t.NomeTab;
                        m.Descrizione = t.NomeTab;

                        if (count == 0)
                        {
                            m.Attivo = true;
                            m.Completato = false;
                        }

                        if (t.Tag == "UFFDEST")
                        {
                            if (doc.Id_Richiesta.HasValue)
                            {
                                m.SottoTitolo = "Amministrazione";
                            }
                            else
                            {
                                m.SottoTitolo = GetDescrizioneAbilitazione(doc);
                            }
                        }

                        result.Add(m);
                        count++;
                    }
                }
                else if (doc.Id_Stato == (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore)
                {
                    // prende il workflow nello stato in cui si trova la pratica
                    var current_workflow_Rifiutato = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(doc.Id_WKF_Tipologia) && w.ID_STATO.Equals((int)StatiDematerializzazioneDocumenti.RifiutoApprovatore)).FirstOrDefault();

                    int ordine_rifiutato = 0;
                    ordine_rifiutato = current_workflow_Rifiutato.ORDINE;

                    foreach (var t in objD)
                    {
                        Dematerializzazione_Tab_Dettaglio_Pratica m = new Dematerializzazione_Tab_Dettaglio_Pratica();
                        m.Title = t.NomeTab;
                        m.Descrizione = t.NomeTab;

                        try
                        {
                            m.SottoTitolo = DematerializzazioneManager.GetPropValue(doc, t.NomeAttributo_XR_DEM_DOCUMENTI).ToString();
                        }
                        catch (Exception ex)
                        {
                            m.SottoTitolo = string.Empty;
                        }

                        List<int> ords = t.OrdineWKF.Split(',').ToList().Select(int.Parse).ToList();
                        int min = ords.Min();
                        int max = ords.Max();

                        if (ordine_rifiutato < min)
                        {
                            m.Attivo = false;
                            m.Completato = false;
                            m.Fallito = false;
                        }
                        else if (ords.Contains(ordine_rifiutato))
                        {
                            m.Attivo = false;
                            m.Completato = false;
                            m.Fallito = true;
                            m.SottoTitolo = doc.DataRifiuto.GetValueOrDefault().ToString("dd/MM/yyyy HH:mm:ss");
                        }
                        else if (max < ordine_rifiutato)
                        {
                            m.Attivo = false;
                            m.Completato = true;
                            m.Fallito = false;
                        }

                        if (t.Tag == "UFFDEST")
                        {
                            if (doc.Id_Richiesta.HasValue)
                            {
                                m.SottoTitolo = "Amministrazione";
                            }
                            else
                            {
                                m.SottoTitolo = GetDescrizioneAbilitazione(doc);
                            }
                        }

                        result.Add(m);
                    }
                }
                else if (doc.Id_Stato == (int)StatiDematerializzazioneDocumenti.PraticaCancellata)
                {
                    foreach (var t in objD)
                    {
                        Dematerializzazione_Tab_Dettaglio_Pratica m = new Dematerializzazione_Tab_Dettaglio_Pratica();
                        m.Title = t.NomeTab;
                        m.Descrizione = t.NomeTab;
                        XR_DEM_WKF_OPERSTATI e = new XR_DEM_WKF_OPERSTATI();
                        var canc = db.XR_DEM_WKF_OPERSTATI.Where(w => w.ID_GESTIONE.Equals(doc.Id) &&
                                                w.ID_STATO.Equals(doc.Id_Stato) &&
                                                w.ID_TIPOLOGIA.Equals(doc.Id_WKF_Tipologia) &&
                                                w.COD_TIPO_PRATICA == "DEM").ToList();

                        if (canc == null)
                        {
                            throw new Exception("Impossibile reperire la data di eliminazione pratica");
                        }
                        else
                        {
                            canc = canc.OrderByDescending(w => w.ID_OPERSTATI).ToList();
                            e = canc.FirstOrDefault();
                        }

                        m.SottoTitolo = e.TMS_TIMESTAMP.ToString("dd/MM/yyyy");

                        if (count == (objD.Count - 1))
                        {
                            m.Attivo = false;
                            m.Completato = false;
                            m.Fallito = true;
                        }

                        if (t.Tag == "UFFDEST")
                        {
                            if (doc.Id_Richiesta.HasValue)
                            {
                                m.SottoTitolo = "Amministrazione";
                            }
                            else
                            {
                                m.SottoTitolo = GetDescrizioneAbilitazione(doc);
                            }
                        }

                        result.Add(m);
                        count++;
                    }
                }
                else
                {
                    XR_WKF_WORKFLOW current_workflow = null;

                    // prende il workflow nello stato in cui si trova la pratica
                    if (doc.Id_Stato == (int)StatiDematerializzazioneDocumenti.Visionato)
                    {
                        current_workflow = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(doc.Id_WKF_Tipologia) && w.ID_STATO.Equals((int)StatiDematerializzazioneDocumenti.Visionato)).FirstOrDefault();
                        if (current_workflow == null)
                        {
                            // se non trova nulla è possibile che ci sia il vistatore, 
                            // ma non sia impostato dal workflow, quindi in questo caso 
                            // cercherà il primo stato possibile del workflow
                            current_workflow = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(doc.Id_WKF_Tipologia)).OrderBy(w => w.ORDINE).ThenBy(w => w.ID_STATO).FirstOrDefault();
                        }
                    }
                    else if (doc.Id_Stato == (int)StatiDematerializzazioneDocumenti.Protocollato)
                    {
                        current_workflow = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(doc.Id_WKF_Tipologia) && w.ID_STATO.Equals((int)StatiDematerializzazioneDocumenti.Accettato)).FirstOrDefault();
                    }
                    else
                    {
                        current_workflow = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(doc.Id_WKF_Tipologia) && w.ID_STATO.Equals(doc.Id_Stato)).FirstOrDefault();
                    }

                    if (current_workflow == null)
                    {
                        throw new Exception("Nessun workflow trovato per la pratica esaminata");
                    }

                    int ordine = 0;
                    ordine = current_workflow.ORDINE;

                    foreach (var t in objD)
                    {
                        Dematerializzazione_Tab_Dettaglio_Pratica m = new Dematerializzazione_Tab_Dettaglio_Pratica();
                        m.Title = t.NomeTab;
                        m.Descrizione = t.NomeTab;
                        m.SottoTitolo = string.Empty;

                        List<int> ords = t.OrdineWKF.Split(',').ToList().Select(int.Parse).ToList();
                        int min = ords.Min();
                        int max = ords.Max();

                        if (ordine < min)
                        {
                            m.Attivo = true;
                        }
                        else if (ords.Contains(ordine))
                        {
                            m.Attivo = false;
                            m.Completato = true;

                            try
                            {
                                if (String.IsNullOrEmpty(t.NomeAttributo_XR_DEM_DOCUMENTI) && !String.IsNullOrEmpty(t.NomeAttributo_XR_DEM_WKF_OPERSTATI))
                                {
                                    int findStato = 0;
                                    bool convertito = int.TryParse(t.Stato_In_XR_DEM_WKF_OPERSTATI, out findStato);
                                    if (convertito)
                                    {
                                        XR_DEM_WKF_OPERSTATI xr_stati = db.XR_DEM_WKF_OPERSTATI.Where(w => w.ID_GESTIONE == doc.Id &&
                                        w.ID_STATO == findStato).OrderByDescending(w => w.TMS_TIMESTAMP).FirstOrDefault();

                                        m.SottoTitolo = DematerializzazioneManager.GetPropValue(xr_stati, t.NomeAttributo_XR_DEM_WKF_OPERSTATI).ToString();
                                    }
                                }
                                else
                                {
                                    m.SottoTitolo = DematerializzazioneManager.GetPropValue(doc, t.NomeAttributo_XR_DEM_DOCUMENTI).ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                m.SottoTitolo = string.Empty;
                            }
                        }
                        else if (max < ordine)
                        {
                            m.Attivo = false;
                            m.Completato = true;
                            try
                            {
                                if (String.IsNullOrEmpty(t.NomeAttributo_XR_DEM_DOCUMENTI) && !String.IsNullOrEmpty(t.NomeAttributo_XR_DEM_WKF_OPERSTATI))
                                {
                                    int findStato = 0;
                                    bool convertito = int.TryParse(t.Stato_In_XR_DEM_WKF_OPERSTATI, out findStato);
                                    if (convertito)
                                    {
                                        XR_DEM_WKF_OPERSTATI xr_stati = db.XR_DEM_WKF_OPERSTATI.Where(w => w.ID_GESTIONE == doc.Id &&
                                        w.ID_STATO == findStato).OrderByDescending(w => w.TMS_TIMESTAMP).FirstOrDefault();

                                        m.SottoTitolo = DematerializzazioneManager.GetPropValue(xr_stati, t.NomeAttributo_XR_DEM_WKF_OPERSTATI).ToString();
                                    }
                                }
                                else
                                {
                                    m.SottoTitolo = DematerializzazioneManager.GetPropValue(doc, t.NomeAttributo_XR_DEM_DOCUMENTI).ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                m.SottoTitolo = string.Empty;
                            }
                        }

                        if (t.Tag == "UFFDEST")
                        {
                            if (doc.Id_Richiesta.HasValue)
                            {
                                m.SottoTitolo = "Amministrazione";
                            }
                            else
                            {
                                m.SottoTitolo = GetDescrizioneAbilitazione(doc);
                            }
                        }

                        result.Add(m);
                    }
                }
            }

            return result;
        }

        private string GetDescrizioneAbilitazione(XR_DEM_DOCUMENTI doc)
        {
            string result = "";
            IncentiviEntities db = new IncentiviEntities();
            try
            {
                var current_workflow_position = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(doc.Id_WKF_Tipologia) && w.ID_STATO.Equals(doc.Id_Stato)).FirstOrDefault();
                if (current_workflow_position != null)
                {
                    string dest = current_workflow_position.DESTINATARIO;
                    if (!String.IsNullOrEmpty(dest))
                    {
                        myRaiData.Incentivi.IncentiviEntities dbTal = new myRaiData.Incentivi.IncentiviEntities();

                        myRaiData.Incentivi.XR_HRIS_ABIL_FUNZIONE funzione = dbTal.XR_HRIS_ABIL_FUNZIONE.Where(w => w.COD_FUNZIONE.Equals("DEMA")).FirstOrDefault();

                        if (funzione != null)
                        {
                            int id_funzione = funzione.ID_FUNZIONE;

                            var itemDesc = dbTal.XR_HRIS_ABIL_SUBFUNZIONE.Where(w => w.ID_FUNZIONE.Equals(id_funzione) && w.COD_SUBFUNZIONE.Equals(dest)).FirstOrDefault();
                            if (itemDesc != null)
                            {
                                result = itemDesc.NOT_UFFICIO;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = "";
            }

            return result;
        }

        /// <summary>
        /// Metodo per la cancellazione di una pratica
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="idPersona"></param>
        /// <param name="idDoc"></param>
        /// <returns></returns>
        public ActionResult EliminaPratica(string matricola, int idPersona, int idDoc)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                showMessageString = new
                {
                    param1 = 200,
                    param2 = "OK"
                };

                var db = AnagraficaManager.GetDb();
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (item == null)
                {
                    throw new Exception("Impossibile reperire il documento");
                }

                // se i documenti sono stati inviati a maternità e congedi, bisogna controllare se
                // la pratica è stata già avviata oppure no
                if (item.Id_Richiesta.HasValue && item.Id_Richiesta.GetValueOrDefault() > 0)
                {
                    int idRichiesta = item.Id_Richiesta.GetValueOrDefault();
                    int idStatoPraticaBK = item.Id_Stato;
                    var myRisp = MaternitaCongediManager.IsAvviata(item.Id_Richiesta.GetValueOrDefault());
                    if (myRisp != null && myRisp.Avviata)
                    {
                        throw new Exception("Impossibile eliminare la pratica in quanto risulta già avviata");
                    }
                    else
                    {
                        item.Id_Richiesta = null;
                        item.Id_Stato = (int)StatiDematerializzazioneDocumenti.PraticaCancellata;
                        item.PraticaAttiva = false;
                        db.SaveChanges();
                        string esito = MaternitaCongediManager.cprat(idRichiesta);

                        if (esito != null)
                        {
                            item.Id_Richiesta = idRichiesta;
                            item.Id_Stato = idStatoPraticaBK;
                            item.PraticaAttiva = true;
                            db.SaveChanges();
                            throw new Exception(esito);
                        }
                    }
                }
                else
                {
                    item.Id_Stato = (int)StatiDematerializzazioneDocumenti.PraticaCancellata;
                    item.PraticaAttiva = false;
                    db.SaveChanges();
                }

                DateTime ora = DateTime.Now;

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = Request.UserHostAddress,
                    COD_USER = UtenteHelper.Matricola(),
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = item.Id,
                    ID_PERSONA = idPersona,
                    ID_STATO = item.Id_Stato,
                    ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = DateTime.Now,
                    TMS_TIMESTAMP = DateTime.Now
                });

                db.SaveChanges();

                return Json(showMessageString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404, "Errore non è stato possibile cancellare il documento");
            }
        }

        [HttpGet]
        public ActionResult IsReadyPDF(int idDoc)
        {
            bool result = false;
            string error = "";
            try
            {
                var db = AnagraficaManager.GetDb();
                var item = db.XR_ALLEGATI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();
                string mimeTypePDF = MimeTypeMap.GetMimeType("pdf");
                if (!String.IsNullOrEmpty(mimeTypePDF))
                {
                    mimeTypePDF = mimeTypePDF.ToUpper();
                }
                if (item != null)
                {
                    if (item.MimeType.ToUpper() == mimeTypePDF)
                    {
                        result = true;
                    }
                    else
                    {
                        // se non è un pdf, ma è stato convertito in pdf allora 
                        // il file sarà disponibile nella colonna ContentBytePDF
                        if (item.ContentBytePDF != null)
                        {
                            result = true;
                        }
                    }
                }
                else
                {
                    throw new Exception("Impossibile trovare il file desiderato");
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = result,
                    message = error,
                    error = !String.IsNullOrEmpty(error)
                }
            };
        }

        /// <summary>
        /// Imposta le coordinate del protocollo e della data
        /// che verranno impressi sul documento principale 
        /// all'atto della protocollazione del documento
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="idPersona"></param>
        /// <param name="idDoc">Identificativo univoco dell'allegato</param>
        /// <param name="posLeft"></param>
        /// <param name="posTop"></param>
        /// <param name="posLeftData"></param>
        /// <param name="posTopData"></param>
        /// <returns></returns>
        public ActionResult SetPosizioneProtocollo(string matricola, int idPersona, int idAllegato, float posLeft, float posTop, float posLeftData, float posTopData, float posLeftFirma, float posTopFirma, int numeroPaginaFirma = 1, int numeroPaginaProtocollo = 1, int? idDocumento = null)
        {
            dynamic showMessageString = string.Empty;

            //SezioniAnag sezione = SezioniAnag.NonDefinito;
            //var anagrafica = AnagraficaManager.GetAnagrafica(matricola , new AnagraficaLoader(sezione));

            try
            {
                var db = AnagraficaManager.GetDb();
                var item = db.XR_ALLEGATI.Where(w => w.Id.Equals(idAllegato)).FirstOrDefault();
                if (item != null)
                {
                    // true vuol dire che sono tutti -1 allora verifica se per quel
                    // tipo di documento è definita una posizione fissa per il protocollo
                    bool noProtocollo = (posLeft == -1 && posLeftData == -1 && posTop == -1 && posTopData == -1);

                    if (noProtocollo)
                    {
                        string tipologiaDocumentale = "";
                        string tipologiaDocumento = "";

                        tipologiaDocumentale = (string)SessionHelper.Get(matricola + "tipologiaDocumentale");
                        tipologiaDocumento = (string)SessionHelper.Get(matricola + "tipologiaDocumento");

                        var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                        if (comportamento == null)
                        {
                            throw new Exception("Comportamento non trovato");
                        }

                        if (!String.IsNullOrEmpty(comportamento.PosizioneProtocollo))
                        {
                            List<PosizioneProtocolloOBJ> objD = JsonConvert.DeserializeObject<List<PosizioneProtocolloOBJ>>(comportamento.PosizioneProtocollo);

                            var protPos = objD.Where(w => w.Oggetto.Equals("Protocollo")).FirstOrDefault();
                            if (protPos != null)
                            {
                                posLeft = protPos.PosizioneLeft;
                                posTop = protPos.PosizioneTop;
                            }

                            var protPosData = objD.Where(w => w.Oggetto.Equals("Data")).FirstOrDefault();
                            if (protPosData != null)
                            {
                                posLeftData = protPosData.PosizioneLeft;
                                posTopData = protPosData.PosizioneTop;
                            }

                            var protPosFirma = objD.Where(w => w.Oggetto.Equals("Firma")).FirstOrDefault();
                            if (protPosFirma != null)
                            {
                                posLeftFirma = protPosFirma.PosizioneLeft;
                                posTopFirma = protPosFirma.PosizioneTop;
                                numeroPaginaFirma = protPosFirma.NumeroPagina;
                            }

                            noProtocollo = false;
                        }
                    }

                    if (!noProtocollo)
                    {
                        List<PosizioneProtocolloOBJ> obj = new List<PosizioneProtocolloOBJ>();

                        obj.Add(new PosizioneProtocolloOBJ()
                        {
                            Oggetto = "Protocollo",
                            PosizioneLeft = posLeft,
                            PosizioneTop = posTop,
                            NumeroPagina = numeroPaginaProtocollo
                        });

                        obj.Add(new PosizioneProtocolloOBJ()
                        {
                            Oggetto = "Data",
                            PosizioneLeft = posLeftData,
                            PosizioneTop = posTopData,
                            NumeroPagina = numeroPaginaProtocollo
                        });

                        obj.Add(new PosizioneProtocolloOBJ()
                        {
                            Oggetto = "Firma",
                            PosizioneLeft = posLeftFirma,
                            PosizioneTop = posTopFirma,
                            NumeroPagina = numeroPaginaFirma
                        });

                        var jsonString = JsonConvert.SerializeObject(obj);
                        item.PosizioneProtocollo = jsonString;

                        if (idDocumento.HasValue && idDocumento.GetValueOrDefault() > 0)
                        {
                            int IDWKF_TIPOLOGIA = 0;
                            int iddocumento = idDocumento.GetValueOrDefault();
                            var documento = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(iddocumento)).FirstOrDefault();
                            if (documento != null)
                            {
                                IDWKF_TIPOLOGIA = documento.Id_WKF_Tipologia;
                            }

                            db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                            {
                                COD_TERMID = Request.UserHostAddress,
                                COD_USER = UtenteHelper.Matricola(),
                                DTA_OPERAZIONE = 0,
                                COD_TIPO_PRATICA = "DEM",
                                ID_GESTIONE = idDocumento.GetValueOrDefault(),
                                ID_PERSONA = idPersona,
                                ID_STATO = (int)StatiDematerializzazioneDocumenti.ModificaPosizioneProtocollo,
                                ID_TIPOLOGIA = IDWKF_TIPOLOGIA,
                                NOMINATIVO = UtenteHelper.Nominativo(),
                                VALID_DTA_INI = DateTime.Now,
                                TMS_TIMESTAMP = DateTime.Now
                            });
                        }

                    }

                    db.SaveChanges();
                }

                showMessageString = new
                {
                    param1 = 200,
                    param2 = "OK"
                };

                return Json(showMessageString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404, "Errore nel salvataggio della posizione del protocollo.");
            }
        }

        #endregion

        public ActionResult GetPDFByIdAllegato(int idAllegato)
        {
            try
            {
                var db = AnagraficaManager.GetDb();
                var item = db.XR_ALLEGATI.Where(w => w.Id.Equals(idAllegato)).FirstOrDefault();

                byte[] byteArray = item.ContentByte;
                string nomefile = item.NomeFile;

                MemoryStream pdfStream = new MemoryStream();
                pdfStream.Write(byteArray, 0, byteArray.Length);
                pdfStream.Position = 0;

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = "documento",
                    Inline = true,
                };

                Response.AddHeader("Content-Disposition", "inline; filename=" + nomefile + ".pdf");
                return File(byteArray, "application/pdf");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult GetModal_Doc_Viewer(string m, int id, int idDoc)
        {
            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM();
            model.Richiesta = GetDocumentData(idDoc);
            model.ApprovazioneEnabled = false;
            model.PresaInCaricoEnabled = false;

            if (model.Richiesta != null)
            {
                model.Matricola = m;
                model.IdPersona = id;

                model.NominativoUtenteApprovatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaApprovatore);
                model.NominativoUtenteCreatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaCreatore);
                model.NominativoUtenteDestinatario = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaDestinatario);
                model.NominativoUtenteFirma = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaFirma);
                model.NominativoUtenteIncaricato = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaIncaricato);
            }

            return View("~/Views/Dematerializzazione/Modal_Doc_Viewer.cshtml", model);
        }

        public ActionResult Modal_Viewer(string m, int id, int idDoc)
        {
            RichiestaDoc documento = GetDocumentData(idDoc);
            documento.IdPersona = id;
            documento.Matricola = m;

            if (documento != null)
            {
                // SISTEMARE
                //if ( documento.Allegato != null )
                //{
                //    // il pdf va convertito in png
                //    byte[] convertito = ConvertPDFTOPNG( documento.Allegato.ContentByte );
                //    if ( convertito != null && convertito.Length > 0 )
                //    {
                //        documento.PNGBase64 = Convert.ToBase64String( convertito );
                //    }
                //}
            }

            return View("Modal_Viewer", documento);
        }

        #region private
        /// <summary>
        /// Salvataggio dati della richiesta
        /// </summary>
        /// <param name="richiesta"></param>
        /// <note>Metodo interno di inserimento nuova pratica, non vincolato al solo 
        /// dipendente come lascerebbe intendere il nome del metodo</note>
        /// 
        private int InsertDocumentoVSDipendenteInternal(RichiestaDoc richiesta, List<int> allegati, bool isBozza = false)
        {
            int result = 0;
            try
            {
                var db = AnagraficaManager.GetDb();
                List<XR_DEM_VERSIONI_DOCUMENTO> versioni = new List<XR_DEM_VERSIONI_DOCUMENTO>();

                XR_DEM_DOCUMENTI doc = null;

                if (richiesta.Documento.Id > 0)
                {
                    doc = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(richiesta.Documento.Id)).FirstOrDefault();

                    if (doc == null)
                    {
                        throw new Exception("Errore nel reperimento del documento");
                    }

                    //doc.Note = richiesta.Documento.Note;
                    doc.IdArea = richiesta.Documento.IdArea;
                    doc.MatricolaApprovatore = richiesta.Documento.MatricolaApprovatore;
                    doc.MatricolaFirma = richiesta.Documento.MatricolaFirma;
                    doc.MatricolaIncaricato = richiesta.Documento.MatricolaIncaricato;
                    doc.CustomDataJSON = richiesta.Documento.CustomDataJSON;
                    doc.Id_WKF_Tipologia = richiesta.Documento.Id_WKF_Tipologia;
                    if (doc.Id_Stato < richiesta.Documento.Id_Stato)
                    {
                        doc.Id_Stato = richiesta.Documento.Id_Stato;
                    }
                    if (isBozza)
                    {
                        doc.Id_Stato = (int)StatiDematerializzazioneDocumenti.Bozza;
                    }
                }
                else
                {
                    doc = new XR_DEM_DOCUMENTI()
                    {
                        Descrizione = richiesta.Documento.Descrizione,
                        DataCreazione = DateTime.Now,
                        Id_Stato = richiesta.Documento.Id_Stato,
                        Cod_Tipologia_Documentale = richiesta.Documento.Cod_Tipologia_Documentale,
                        Id_WKF_Tipologia = richiesta.Documento.Id_WKF_Tipologia,
                        MatricolaCreatore = richiesta.Documento.MatricolaCreatore,
                        IdPersonaCreatore = richiesta.Documento.IdPersonaCreatore,
                        MatricolaDestinatario = richiesta.Documento.MatricolaDestinatario,
                        IdPersonaDestinatario = richiesta.Documento.IdPersonaDestinatario,
                        //Note = richiesta.Documento.Note ,
                        Note = null,
                        IdArea = richiesta.Documento.IdArea,
                        Id_Tipo_Doc = richiesta.Documento.Id_Tipo_Doc,
                        MatricolaApprovatore = richiesta.Documento.MatricolaApprovatore,
                        MatricolaFirma = richiesta.Documento.MatricolaFirma,
                        MatricolaIncaricato = richiesta.Documento.MatricolaIncaricato,
                        CustomDataJSON = richiesta.Documento.CustomDataJSON
                    };
                }

                int tempId = 0;
                foreach (var a in allegati)
                {
                    bool exist = db.XR_DEM_ALLEGATI_VERSIONI.Count(w => w.IdAllegato.Equals(a)) > 0;

                    // Se non esiste allora deve creare l'associazione
                    if (!exist)
                    {
                        tempId--;
                        XR_DEM_VERSIONI_DOCUMENTO version = new XR_DEM_VERSIONI_DOCUMENTO()
                        {
                            N_Versione = 1,
                            DataUltimaModifica = DateTime.Now,
                            Id_Documento = doc.Id,
                            Id = tempId
                        };

                        db.XR_DEM_VERSIONI_DOCUMENTO.Add(version);

                        XR_DEM_ALLEGATI_VERSIONI associativa = new XR_DEM_ALLEGATI_VERSIONI()
                        {
                            IdAllegato = a,
                            IdVersione = version.Id
                        };
                        db.XR_DEM_ALLEGATI_VERSIONI.Add(associativa);
                    }
                }

                // modificato: non serve più perchè calcolato nel metodo padre
                //doc.Id_WKF_Tipologia = GetTipologiaWorkFlow(doc);

                if (doc.Id == 0)
                {
                    db.XR_DEM_DOCUMENTI.Add(doc);
                    db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                    {
                        COD_TERMID = Request.UserHostAddress,
                        COD_USER = richiesta.Documento.MatricolaCreatore,
                        DTA_OPERAZIONE = 0,
                        COD_TIPO_PRATICA = "DEM",
                        ID_GESTIONE = doc.Id,
                        ID_PERSONA = richiesta.Documento.IdPersonaCreatore,
                        ID_STATO = doc.Id_Stato,
                        ID_TIPOLOGIA = doc.Id_WKF_Tipologia,
                        NOMINATIVO = UtenteHelper.Nominativo(),
                        VALID_DTA_INI = DateTime.Now,
                        TMS_TIMESTAMP = DateTime.Now
                    });
                }
                else
                {
                    db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                    {
                        COD_TERMID = Request.UserHostAddress,
                        COD_USER = richiesta.Documento.MatricolaCreatore,
                        DTA_OPERAZIONE = 0,
                        COD_TIPO_PRATICA = "DEM",
                        ID_GESTIONE = doc.Id,
                        ID_PERSONA = richiesta.Documento.IdPersonaCreatore,
                        ID_STATO = (int)StatiDematerializzazioneDocumenti.DocumentoModificato,
                        ID_TIPOLOGIA = doc.Id_WKF_Tipologia,
                        NOMINATIVO = UtenteHelper.Nominativo(),
                        VALID_DTA_INI = DateTime.Now,
                        TMS_TIMESTAMP = DateTime.Now
                    });
                }

                db.SaveChanges();
                result = doc.Id;

                CezanneHelper.GetCampiFirma(out var campiFirma);
                string mioNome = DematerializzazioneManager.GetNominativoByMatricola(richiesta.Documento.MatricolaCreatore);
                if (!String.IsNullOrEmpty(richiesta.Documento.Note))
                {
                    // se è stata inserita una nota, allora va creata una riga nella tabella 
                    // XR_DEM_NOTE
                    XR_DEM_NOTE nuovaNota = new XR_DEM_NOTE()
                    {
                        DATAINSERIMENTO = campiFirma.Timestamp,
                        TESTONOTA = richiesta.Documento.Note,
                        TIPONOTA = (int)XR_DEM_TIPI_NOTE_ENUM.NOTAOPERATORE,
                        IDDOCUMENTO = result,
                        MATRICOLA = richiesta.Documento.MatricolaCreatore,
                        NOMINATIVO = mioNome
                    };

                    db.XR_DEM_NOTE.Add(nuovaNota);
                    db.SaveChanges();
                }

                // VCON
                // se è una variazione contabile deve creare la lettera per il dipendente
                if (doc.Id_Tipo_Doc == 35)
                {
                    DateTime dataInizio = DateTime.MinValue;
                    DateTime dataFine = DateTime.MinValue;
                    string codiceEvento = "";
                    List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();
                    List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                    var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                    if (tt != null)
                    {
                        codiceEvento = tt.Valore;
                    }

                    if (String.IsNullOrEmpty(codiceEvento))
                    {
                        tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                        if (tt != null)
                        {
                            codiceEvento = tt.Valore;
                        }
                    }

                    if (String.IsNullOrEmpty(codiceEvento))
                    {
                        throw new Exception("Impossibile proseguire tipo di variazione mancante");
                    }

                    if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                    {
                        foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
                        {
                            listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
                        }
                    }
                    else
                    {
                        listaPiatta.AddRange(objModuloValorizzato.ToList());
                    }

                    // adesso in lista piatta c'è o il contenuto di GetListaPiatta oppure l'oggetto standard senza elementi inline
                    var dtInizio = listaPiatta.Where(w => w.DBRefAttribute == "DATA_DECORRENZA").FirstOrDefault();
                    if (dtInizio != null)
                    {
                        DateTime temp;
                        string dt = dtInizio.Valore;
                        if (!String.IsNullOrEmpty(dt))
                        {
                            if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                throw new Exception("Errore in conversione DATA_DECORRENZA");
                            dataInizio = temp;
                        }
                    }
                    else
                    {
                        throw new Exception("Errore DATA_DECORRENZA non trovata");
                    }

                    var dtFine = listaPiatta.Where(w => w.DBRefAttribute == "DATA_SCADENZA").FirstOrDefault();
                    if (dtFine != null)
                    {
                        DateTime temp;
                        string dt = dtFine.Valore;
                        if (!String.IsNullOrEmpty(dt))
                        {
                            if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                throw new Exception("Errore in conversione DATA_SCADENZA");
                            dataFine = temp;
                        }
                    }
                    else
                    {
                        // #PRESTARE ATTENZIONE potrebbe non essere corretto
                        dataFine = new DateTime(2999, 12, 31);
                        //throw new Exception("Errore DATA_SCADENZA non trovata");
                    }

                    byte[] lettera = DematerializzazioneManager.GeneraPdfLetteraVariazione(doc, codiceEvento, dataInizio, dataFine);
                    string nominativo = DematerializzazioneManager.GetNominativoByMatricola(doc.MatricolaDestinatario);
                    nominativo = nominativo.Trim();
                    nominativo = nominativo.Replace(" ", "_");
                    nominativo = RemoveDiacritics(nominativo);
                    string myTypeFileName = doc.Descrizione.Trim().Replace(" ", "_");
                    string nomeLettera = String.Format("Lettera_{0}_{1}_{2}_{3}.pdf", myTypeFileName, codiceEvento, doc.MatricolaDestinatario, nominativo);

                    #region Crea allegati

                    int length = lettera.Length;
                    string est = Path.GetExtension(nomeLettera);
                    string tipoFile = MimeTypeMap.GetMimeType(est);
                    string jsonStringProtocollo = null;

                    List<PosizioneProtocolloOBJ> objProt = new List<PosizioneProtocolloOBJ>();

                    float protL = 83.5f;
                    float protTop = 112.0f;
                    int pagProt = 1;

                    objProt.Add(new PosizioneProtocolloOBJ()
                    {
                        Oggetto = "Protocollo",
                        PosizioneLeft = protL,
                        PosizioneTop = protTop,
                        NumeroPagina = pagProt
                    });

                    float dataLeft = 123.5f;
                    float dataTop = 158.5f;
                    int dataPagina = 1;

                    objProt.Add(new PosizioneProtocolloOBJ()
                    {
                        Oggetto = "Data",
                        PosizioneLeft = dataLeft,
                        PosizioneTop = dataTop,
                        NumeroPagina = dataPagina
                    });

                    float firmaLeft = 332.5f;
                    float firmaTop = 453.0f;
                    int firmaPagina = 1;

                    objProt.Add(new PosizioneProtocolloOBJ()
                    {
                        Oggetto = "Firma",
                        PosizioneLeft = firmaLeft,
                        PosizioneTop = firmaTop,
                        NumeroPagina = firmaPagina
                    });

                    jsonStringProtocollo = JsonConvert.SerializeObject(objProt);

                    XR_ALLEGATI allegato = new XR_ALLEGATI()
                    {
                        NomeFile = nomeLettera,
                        MimeType = tipoFile,
                        Length = length,
                        ContentByte = lettera,
                        IsPrincipal = true,
                        PosizioneProtocollo = jsonStringProtocollo
                    };

                    db.XR_ALLEGATI.Add(allegato);

                    tempId = -1;

                    XR_DEM_VERSIONI_DOCUMENTO version = new XR_DEM_VERSIONI_DOCUMENTO()
                    {
                        N_Versione = 1,
                        DataUltimaModifica = DateTime.Now,
                        Id_Documento = doc.Id,
                        Id = tempId
                    };

                    db.XR_DEM_VERSIONI_DOCUMENTO.Add(version);

                    XR_DEM_ALLEGATI_VERSIONI associativa = new XR_DEM_ALLEGATI_VERSIONI()
                    {
                        IdAllegato = allegato.Id,
                        IdVersione = version.Id
                    };
                    db.XR_DEM_ALLEGATI_VERSIONI.Add(associativa);

                    db.SaveChanges();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// In alcuni casi ad esempio per i documenti con dati custom
        /// il valore dell'eccezione selezionata potrebbe modificare la 
        /// tipologia di workflow da seguire
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private int GetTipologiaWorkFlow(XR_DEM_DOCUMENTI doc)
        {
            int result = doc.Id_WKF_Tipologia;
            var db = AnagraficaManager.GetDb();

            try
            {

                XR_WKF_TIPOLOGIA WKF_Tipologia = Get_WKF_Tipologia(doc.Id_WKF_Tipologia);
                if (WKF_Tipologia == null)
                {
                    throw new Exception("Workflow non trovato");
                }

                // prende il nome della tipologia corrente
                // ad esempio DEMDOC_VSRUO_CPM
                string nomeTipo = WKF_Tipologia.COD_TIPOLOGIA;
                nomeTipo += "_";

                // verifica se esistono tipologie di workflow che hanno come nome che inizia per [nomeTipo_]*
                bool exists = db.XR_WKF_TIPOLOGIA.Count(w => w.COD_TIPOLOGIA.Contains(nomeTipo)) > 0;

                // se ce ne sono allora verifica che il documento abbia il codice eccezione selezionato 
                // e che esista un XR_WKF_TIPOLOGIA con quel nome
                if (exists)
                {
                    string codEccezione = "";
                    if (!String.IsNullOrEmpty(doc.CustomDataJSON) && doc.CustomDataJSON != "[]")
                    {
                        List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                        if (tt != null)
                        {
                            // esempio codEccezione = ON
                            codEccezione = tt.Valore;
                        }
                        else
                        {
                            // in alcuni casi l'eccezione non viene salvata nel campo EccezionePerAutomatismo, ma nel campo EccezioneSelezionataNascosta
                            tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                            if (tt != null)
                            {
                                codEccezione = tt.Valore;
                            }
                        }

                        if (!String.IsNullOrEmpty(codEccezione))
                        {
                            // combina il contenuto di nometipo con il codice eccezione per ottenere 
                            // qualcosa tipo DEMDOC_VSRUO_CPM_ON
                            nomeTipo += codEccezione;

                            // verifica se esiste la tipologia contenuta in nomeTipo es: DEMDOC_VSRUO_CPM_ON
                            var wkfTipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeTipo)).FirstOrDefault();

                            if (wkfTipologia != null)
                            {
                                result = wkfTipologia.ID_TIPOLOGIA;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idDocument"></param>
        /// <returns></returns>
        private RichiestaDoc GetDocumentData(int idDocument)
        {
            RichiestaDoc result = null;

            try
            {
                var db = AnagraficaManager.GetDb();
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDocument)).FirstOrDefault();

                if (item != null)
                {
                    result = new RichiestaDoc();
                    result.Documento = new XR_DEM_DOCUMENTI();
                    result.Documento = item;
                    result.Allegati = new List<XR_ALLEGATI>();

                    var versioni = item.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id_Documento.Equals(item.Id) && !w.Deleted).OrderByDescending(w => w.Id).ThenByDescending(x => x.N_Versione).ToList();

                    if (versioni != null && versioni.Any())
                    {
                        List<int> idVersioni = new List<int>();
                        idVersioni.AddRange(versioni.Select(w => w.Id).ToList());

                        // prende tutti gli idversione e prende tutti gli idallegati associati agli idversione
                        var AllVers = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => idVersioni.Contains(w.IdVersione)).OrderByDescending(w => w.IdAllegato).ToList();

                        if (AllVers != null && AllVers.Any())
                        {
                            List<int> idsAllegati = new List<int>();
                            idsAllegati.AddRange(AllVers.Select(w => w.IdAllegato).ToList());
                            if (idsAllegati != null && idsAllegati.Any())
                            {
                                var allegati = db.XR_ALLEGATI.Where(w => idsAllegati.Contains(w.Id)).OrderByDescending(w => w.Id).ToList();
                                if (allegati != null && allegati.Any())
                                {
                                    // presi tutti gli allegati associati al documento
                                    // bisogna scartare quelli che hanno più versioni e
                                    // va presa solo la versione più recente
                                    // purtroppo per i vecchi documenti per vedere se c'è 
                                    // una versione differente dello stesso file, bisogna
                                    // controllare il nome file e idallegato
                                    // stesso nome file idallegato maggiore sarà la versione più recente
                                    // dell'allegato
                                    foreach (var a in allegati)
                                    {
                                        // prende l'allegato con lo stesso nome ma con id più grande
                                        int idMax = allegati.Where(w => w.NomeFile == a.NomeFile).Max(x => x.Id);

                                        // se già presente nell'elenco allora l'elemento è stato già analizzato
                                        bool giaEsiste = result.Allegati.Count(w => w.Id == idMax) == 1;

                                        // se non esiste lo aggiunge all'elenco finale
                                        if (!giaEsiste)
                                        {
                                            var inserire = allegati.Where(w => w.Id == idMax).FirstOrDefault();
                                            result.Allegati.Add(inserire);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        private List<XR_ALLEGATI> GetAllegati(int idDocument)
        {
            List<XR_ALLEGATI> result = new List<XR_ALLEGATI>();

            try
            {
                var db = AnagraficaManager.GetDb();
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDocument)).FirstOrDefault();

                if (item != null)
                {
                    var versioni = item.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id_Documento.Equals(item.Id) && !w.Deleted).OrderByDescending(w => w.Id).ThenByDescending(x => x.N_Versione).ToList();

                    if (versioni != null && versioni.Any())
                    {
                        List<int> idVersioni = new List<int>();
                        idVersioni.AddRange(versioni.Select(w => w.Id).ToList());

                        // prende tutti gli idversione e prende tutti gli idallegati associati agli idversione
                        var AllVers = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => idVersioni.Contains(w.IdVersione)).OrderByDescending(w => w.IdAllegato).ToList();

                        if (AllVers != null && AllVers.Any())
                        {
                            List<int> idsAllegati = new List<int>();
                            idsAllegati.AddRange(AllVers.Select(w => w.IdAllegato).ToList());
                            if (idsAllegati != null && idsAllegati.Any())
                            {
                                var all = db.XR_ALLEGATI.Where(w => idsAllegati.Contains(w.Id)).OrderByDescending(w => w.Id).ToList();
                                if (all != null && all.Any())
                                {
                                    // presi tutti gli allegati associati al documento
                                    // bisogna scartare quelli che hanno più versioni e
                                    // va presa solo la versione più recente
                                    // purtroppo per i vecchi documenti per vedere se c'è 
                                    // una versione differente dello stesso file, bisogna
                                    // controllare il nome file e idallegato
                                    // stesso nome file idallegato maggiore sarà la versione più recente
                                    // dell'allegato
                                    foreach (var a in all)
                                    {
                                        // prende l'allegato con lo stesso nome ma con id più grande
                                        int idMax = all.Where(w => w.NomeFile == a.NomeFile).Max(x => x.Id);

                                        // se già presente nell'elenco allora l'elemento è stato già analizzato
                                        bool giaEsiste = result.Count(w => w.Id == idMax) == 1;

                                        // se non esiste lo aggiunge all'elenco finale
                                        if (!giaEsiste)
                                        {
                                            var inserire = all.Where(w => w.Id == idMax).FirstOrDefault();
                                            result.Add(inserire);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        #endregion

        public ActionResult PrendiInCarico(string matricola, int idPersona, int idDoc)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                showMessageString = new
                {
                    param1 = 200,
                    param2 = "OK"
                };

                var db = AnagraficaManager.GetDb();
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (item == null)
                {
                    throw new Exception("Impossibile trovare il documento richiesto");
                }

                DateTime ora = DateTime.Now;
                int nuovoStato = DematerializzazioneManager.GetNextIdStato(item.Id_Stato, item.Id_WKF_Tipologia, true);

                item.Id_Stato = nuovoStato;
                item.MatricolaIncaricato = matricola;
                item.DataPresaInCarico = DateTime.Now;

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = Request.UserHostAddress,
                    COD_USER = UtenteHelper.Matricola(),
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = item.Id,
                    ID_PERSONA = idPersona,
                    ID_STATO = item.Id_Stato,
                    ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = DateTime.Now,
                    TMS_TIMESTAMP = DateTime.Now
                });

                db.SaveChanges();

                return Json(showMessageString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404, "Errore nella presa in carico del documento");
            }
        }

        public ActionResult AnnullaPrendiInCarico(string matricola, int idPersona, int idDoc)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                showMessageString = new
                {
                    param1 = 200,
                    param2 = "OK"
                };

                var db = AnagraficaManager.GetDb();
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (item == null)
                {
                    throw new Exception("Impossibile trovare il documento richiesto");
                }

                DateTime ora = DateTime.Now;
                int nuovoStato = (int)StatiDematerializzazioneDocumenti.Accettato;

                item.Id_Stato = nuovoStato;
                item.MatricolaIncaricato = null;
                item.DataPresaInCarico = null;

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = Request.UserHostAddress,
                    COD_USER = UtenteHelper.Matricola(),
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = item.Id,
                    ID_PERSONA = idPersona,
                    ID_STATO = item.Id_Stato,
                    ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = DateTime.Now,
                    TMS_TIMESTAMP = DateTime.Now
                });

                db.SaveChanges();

                return Json(showMessageString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404, "Errore nella presa in carico del documento");
            }
        }

        public ActionResult ConcludiPratica(string matricola, int idPersona, int idDoc, string nota)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                showMessageString = new
                {
                    param1 = 200,
                    param2 = "OK"
                };

                var db = AnagraficaManager.GetDb();
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (item == null)
                {
                    throw new Exception("Impossibile trovare il documento richiesto");
                }

                DateTime ora = DateTime.Now;
                int prossimoStato = DematerializzazioneManager.GetNextIdStato(item.Id_Stato, item.Id_WKF_Tipologia, true);

                if (prossimoStato == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInvioDocumentoAlDipendente)
                {
                    item.DataInvioNotifica = ora;
                    db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                    {
                        COD_TERMID = Request.UserHostAddress,
                        COD_USER = UtenteHelper.Matricola(),
                        DTA_OPERAZIONE = 0,
                        COD_TIPO_PRATICA = "DEM",
                        ID_GESTIONE = item.Id,
                        ID_PERSONA = idPersona,
                        ID_STATO = (int)StatiDematerializzazioneDocumenti.InviatoAlDipendente,
                        ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                        NOMINATIVO = UtenteHelper.Nominativo(),
                        VALID_DTA_INI = ora,
                        TMS_TIMESTAMP = ora
                    });

                    ScriviLogDEM(item.Id,
                                    (int)XR_DEM_LOG_STATI_ENUM.DocumentoInviatoAlDipendente,
                                    "ConcludiPratica",
                                    "Documento inviato al dipendente");
                }

                int nuovoStato = (int)StatiDematerializzazioneDocumenti.PraticaConclusa;
                item.Id_Stato = nuovoStato;
                item.DataChiusuraPratica = ora;

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = Request.UserHostAddress,
                    COD_USER = UtenteHelper.Matricola(),
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = item.Id,
                    ID_PERSONA = idPersona,
                    ID_STATO = item.Id_Stato,
                    ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = ora,
                    TMS_TIMESTAMP = ora
                });

                db.SaveChanges();

                return Json(showMessageString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404, "Errore nella presa in carico del documento");
            }
        }

        private AzioneAutomaticaResult EseguiAzioneAutomatica(int nuovoStato, XR_DEM_DOCUMENTI doc)
        {
            AzioneAutomaticaResult result = new AzioneAutomaticaResult();
            result.Esito = true;
            result.IdRichiesta = 0;
            result.NuovoStato = nuovoStato;

            try
            {
                string codEccezione = "";
                if (!String.IsNullOrEmpty(doc.CustomDataJSON) && doc.CustomDataJSON != "[]")
                {
                    List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                    var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                    if (tt != null)
                    {
                        codEccezione = tt.Valore;
                    }

                    if (!String.IsNullOrEmpty(codEccezione))
                    {
                        result = EseguiAzioneAutomaticaInternal(nuovoStato, doc);
                    }
                    else
                    {
                        // non deve eseguire l'azione automatica classica verso Maternità ma deve calcolare il prossimo stato della pratica
                        result.NuovoStato = DematerializzazioneManager.GetNextIdStato(nuovoStato, doc.Id_WKF_Tipologia);

                        // deve caricare le eccezioni in myrai_pianoferiebatch
                        var recuperoEccezione = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                        if (recuperoEccezione != null)
                        {
                            codEccezione = recuperoEccezione.Valore;
                        }

                        if (!String.IsNullOrEmpty(codEccezione))
                        {
                            IncentiviEntities db = new IncentiviEntities();
                            digiGappEntities dbDigiGapp = new digiGappEntities();

                            var eccezioniDaInserireConAzioneAutomatica = dbDigiGapp.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "EccezioniDaInserireConAzioneAutomatica");

                            if (eccezioniDaInserireConAzioneAutomatica == null)
                            {
                                throw new Exception("Eccezioni consentite non trovate");
                            }

                            List<XR_MAT_RICHIESTE> listaR = new List<XR_MAT_RICHIESTE>();
                            listaR = PreparaOggettoXR_MAT_RICHIESTE2(doc);
                            //XR_MAT_RICHIESTE R = PreparaOggettoXR_MAT_RICHIESTE(doc);

                            if (listaR == null || !listaR.Any())
                            {
                                throw new Exception("Errore nella generazione delle eccezioni da inserire in GAPP");
                            }

                            foreach (var R in listaR)
                            {
                                R.ECCEZIONE = codEccezione;
                                R.ID = 0;

                                if (R.PIANIFICAZIONE_BASE_ORARIA.HasValue && !R.PIANIFICAZIONE_BASE_ORARIA.GetValueOrDefault())
                                {
                                    R.PIANIFICAZIONE_BASE_ORARIA = null;
                                }

                                if (R.ECCEZIONE != "MT" && R.DATA_INIZIO_MATERNITA.HasValue)
                                {
                                    R.INIZIO_GIUSTIFICATIVO = R.DATA_INIZIO_MATERNITA;
                                    R.DATA_INIZIO_MATERNITA = null;
                                }

                                if (R.ECCEZIONE != "MT" && R.DATA_FINE_MATERNITA.HasValue)
                                {
                                    R.FINE_GIUSTIFICATIVO = R.DATA_FINE_MATERNITA;
                                    R.DATA_FINE_MATERNITA = null;
                                }

                                var anag = BatchManager.GetUserData(R.MATRICOLA, R.INIZIO_GIUSTIFICATIVO ?? R.DATA_INIZIO_MATERNITA.Value);

                                // se l'eccezione è una di quelle per le quali vanno fatti gli inserimenti in gapp
                                // R.PIANIFICAZIONE_BASE_ORARIA == null significa che è una eccezioni per intera giornata
                                if (!String.IsNullOrEmpty(eccezioniDaInserireConAzioneAutomatica.Valore1) &&
                                    eccezioniDaInserireConAzioneAutomatica.Valore1.Split(',').Contains(R.ECCEZIONE) &&
                                    R.PIANIFICAZIONE_BASE_ORARIA == null)
                                {
                                    DateTime inizioGiustificativo = R.INIZIO_GIUSTIFICATIVO.HasValue ? R.INIZIO_GIUSTIFICATIVO.GetValueOrDefault() : R.DATA_INIZIO_MATERNITA.GetValueOrDefault();
                                    DateTime fineGiustificativo = R.FINE_GIUSTIFICATIVO.HasValue ? R.FINE_GIUSTIFICATIVO.GetValueOrDefault() : R.DATA_FINE_MATERNITA.GetValueOrDefault();

                                    List<Giornata> giorni = new List<Giornata>();
                                    // per il periodo Inizio - Fine giustificativo deve inserire una voce nella tabella myrai_pianoFerieBatch
                                    // in questo modo il batch prenderà in carico le richieste di inserimento eccezione 
                                    var pf = FeriePermessiManager.GetPianoFerieAnno(inizioGiustificativo.Year, true, R.MATRICOLA, false);

                                    if (pf == null)
                                    {
                                        throw new Exception("Errore nel servizio GetPianoFerieAnno");
                                    }

                                    if (pf.esito)
                                    {
                                        if (pf.dipendente != null && pf.dipendente.ferie != null
                                            && pf.dipendente.ferie.giornate != null && pf.dipendente.ferie.giornate.Any())
                                        {
                                            giorni.AddRange(pf.dipendente.ferie.giornate.ToList());

                                            foreach (var g in giorni)
                                            {
                                                string sGiorno = g.dataTeorica.Substring(0, 2);
                                                string sMese = g.dataTeorica.Substring(3, 2);
                                                int anno = inizioGiustificativo.Year;
                                                int giorno = int.Parse(sGiorno);
                                                int mese = int.Parse(sMese);
                                                DateTime nuovaData = DateTime.MinValue;
                                                try
                                                {
                                                    nuovaData = new DateTime(anno, mese, giorno);
                                                }
                                                catch (Exception ex)
                                                {

                                                }

                                                g.data = nuovaData;
                                            }

                                            if (inizioGiustificativo.Year != fineGiustificativo.Year)
                                            {
                                                // se son diversi vuol dire che è un permesso a cavallo di 2 anni esempio un permesso che inizia a dicembre 2021 e termina a gennaio 2022
                                                pf = FeriePermessiManager.GetPianoFerieAnno(fineGiustificativo.Year, true, R.MATRICOLA, false);
                                                if (pf.esito)
                                                {
                                                    if (pf.dipendente != null && pf.dipendente.ferie != null
                                                        && pf.dipendente.ferie.giornate != null && pf.dipendente.ferie.giornate.Any())
                                                    {
                                                        foreach (var g in pf.dipendente.ferie.giornate.ToList())
                                                        {
                                                            string sGiorno = g.dataTeorica.Substring(0, 2);
                                                            string sMese = g.dataTeorica.Substring(3, 2);
                                                            int anno = fineGiustificativo.Year;
                                                            int giorno = int.Parse(sGiorno);
                                                            int mese = int.Parse(sMese);

                                                            DateTime nuovaData = DateTime.MinValue;
                                                            try
                                                            {
                                                                nuovaData = new DateTime(anno, mese, giorno);
                                                            }
                                                            catch (Exception ex)
                                                            {

                                                            }

                                                            g.data = nuovaData;
                                                            giorni.Add(g);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        // a questo punto in giorni ci saranno tutte le giornate degli anni interessati
                                        // prende solo le giornate incluse nel range della richiesta
                                        string eccezioniAccettano9596 = eccezioniDaInserireConAzioneAutomatica.Valore2;

                                        if (!String.IsNullOrEmpty(eccezioniAccettano9596) &&
                                            eccezioniAccettano9596.Split(',').Contains(R.ECCEZIONE))
                                        {
                                            giorni = giorni.Where(w => w.data >= inizioGiustificativo && w.data <= fineGiustificativo).ToList();
                                        }
                                        else
                                        {
                                            // in questo caso l'eccezione va inserita solo per le giornate il cui orario reale sia diverso da 
                                            // 95 e 96
                                            giorni = giorni.Where(w => w.data >= inizioGiustificativo && w.data <= fineGiustificativo &&
                                                                !String.IsNullOrWhiteSpace(w.orarioReale) &&
                                                                !w.orarioReale.StartsWith("9")).ToList();
                                        }

                                        foreach (var g in giorni)
                                        {
                                            string provenienza = String.Format("HRIS/Dematerializzazione-DA_PIANOFERIE=FALSE-APPROVATORE_UFFPERS-ID_DOC={0}", doc.Id);
                                            MyRai_PianoFerieBatch p = new MyRai_PianoFerieBatch()
                                            {
                                                codice_eccezione = R.ECCEZIONE.ToUpper(),
                                                provenienza = provenienza,
                                                dalle = "",
                                                alle = "",
                                                importo = "",
                                                data_eccezione = g.data,
                                                matricola = doc.MatricolaDestinatario,
                                                data_creazione_record = DateTime.Now,
                                                quantita = "1",
                                                sedegapp = anag.sede_gapp
                                            };
                                            dbDigiGapp.MyRai_PianoFerieBatch.Add(p);
                                        }
                                        dbDigiGapp.SaveChanges();
                                    }
                                    else
                                    {
                                        throw new Exception("Errore nel servizio GetPianoFerieAnno");
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Impossibile reperire l'eccezione selezionata (campo non valorizzato)");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.NuovoStato = 0;
                result.DescrizioneErrore = ex.Message;
            }

            return result;
        }

        private XR_MAT_RICHIESTE PreparaOggettoXR_MAT_RICHIESTE(XR_DEM_DOCUMENTI doc)
        {
            XR_MAT_RICHIESTE toSend = new XR_MAT_RICHIESTE();

            if (!String.IsNullOrEmpty(doc.CustomDataJSON) && doc.CustomDataJSON != "[]")
            {
                List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                DateTime temp;

                string nominativo = DematerializzazioneManager.GetNominativoByMatricola(doc.MatricolaDestinatario);
                List<XR_ALLEGATI> allegati = GetAllegati(doc.Id);
                string idRifMaternita = "";

                var findRiferimentoRichiestaMaternita = objModuloValorizzato.Where(w => w.Id == "RiferimentoRichiestaMaternita").FirstOrDefault();
                if (findRiferimentoRichiestaMaternita != null)
                {
                    idRifMaternita = findRiferimentoRichiestaMaternita.Valore;
                }

                try
                {
                    // per ogni attributo nell'oggetto XR_MAT_RICHIESTE cerco 
                    // il rispettivo elemento in objModuloValorizzato
                    // c'è un campo in quell'oggetto che definisce a quale attributo di XR_MAT_RICHIESTE
                    // fa riferimento quel valore.
                    foreach (PropertyInfo prop in typeof(XR_MAT_RICHIESTE).GetProperties())
                    {
                        var DBRefAttribute = objModuloValorizzato.Where(w => w.DBRefAttribute == prop.Name).FirstOrDefault();
                        if (DBRefAttribute != null)
                        {
                            // verifica la tipologia dell'elemento selezionato
                            var tipo = DBRefAttribute.Tipo;

                            if (tipo == TipologiaAttributoEnum.Testo || tipo == TipologiaAttributoEnum.FixedHiddenValue)
                            {
                                PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                                if (null != propToSet && propToSet.CanWrite)
                                {
                                    propToSet.SetValue(toSend, DBRefAttribute.Valore, null);
                                }
                            }

                            if (tipo == TipologiaAttributoEnum.Data)
                            {
                                DateTime? _data = null;
                                string dt = DBRefAttribute.Valore;
                                if (!String.IsNullOrEmpty(dt))
                                {
                                    if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                        throw new Exception("Errore in conversione " + prop.Name);
                                    _data = temp;
                                }

                                PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                                if (null != propToSet && propToSet.CanWrite)
                                {
                                    propToSet.SetValue(toSend, _data, null);
                                }
                            }

                            if (tipo == TipologiaAttributoEnum.Check || tipo == TipologiaAttributoEnum.Radio)
                            {
                                string _tempId = DBRefAttribute.Id;
                                string _gruopId = DBRefAttribute.Gruppo;
                                string valoreSelezionato = "";

                                var tt = objModuloValorizzato.Where(w => w.Gruppo == _gruopId && w.Checked).FirstOrDefault();
                                if (tt != null)
                                {
                                    valoreSelezionato = tt.Valore;
                                }

                                PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);

                                if (!String.IsNullOrEmpty(valoreSelezionato) &&
                                    ((valoreSelezionato.ToUpper() == "TRUE") || (valoreSelezionato.ToUpper() == "FALSE"))
                                    )
                                {
                                    var tipi = prop.PropertyType.GetGenericArguments();
                                    foreach (var t in tipi)
                                    {
                                        if (t.Name == "bool" || t.Name == "Boolean")
                                        {
                                            bool valore = Boolean.Parse(valoreSelezionato);

                                            if (null != propToSet && propToSet.CanWrite)
                                            {
                                                propToSet.SetValue(toSend, valore, null);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (null != propToSet && propToSet.CanWrite)
                                    {
                                        var propertyType = prop.PropertyType;
                                        if (System.Nullable.GetUnderlyingType(propertyType) != null && String.IsNullOrEmpty(valoreSelezionato))
                                        {
                                            // It's a nullable type
                                            propToSet.SetValue(toSend, null, null);
                                        }
                                        else
                                        {
                                            // It's not a nullable type
                                            propToSet.SetValue(toSend, valoreSelezionato, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                toSend.MATRICOLA = doc.MatricolaDestinatario;
                toSend.NOMINATIVO = nominativo;
                toSend.CUSTOM_JSON = doc.CustomDataJSON;

                if ((String.IsNullOrEmpty(idRifMaternita)) || idRifMaternita == "0")
                {
                    toSend.ID = 0;
                }
                else
                {
                    int riferimento = 0;
                    bool convertito = int.TryParse(idRifMaternita, out riferimento);
                    if (convertito)
                    {
                        toSend.ID = riferimento;
                    }
                }
            }

            return toSend;
        }

        private AzioneAutomaticaResult EseguiAzioneAutomaticaInternal(int nuovoStato, XR_DEM_DOCUMENTI doc)
        {
            AzioneAutomaticaResult result = new AzioneAutomaticaResult();

            result.NuovoStato = nuovoStato;
            result.IdRichiesta = 0;
            List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

            if (!String.IsNullOrEmpty(doc.CustomDataJSON) && doc.CustomDataJSON != "[]")
            {
                List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                DateTime temp;
                XR_MAT_RICHIESTE toSend = new XR_MAT_RICHIESTE();
                string stato = "50";
                string nominativo = DematerializzazioneManager.GetNominativoByMatricola(doc.MatricolaDestinatario);
                List<XR_ALLEGATI> allegati = GetAllegati(doc.Id);
                string idRifMaternita = "";

                var findRiferimentoRichiestaMaternita = objModuloValorizzato.Where(w => w.Id == "RiferimentoRichiestaMaternita").FirstOrDefault();
                if (findRiferimentoRichiestaMaternita != null)
                {
                    idRifMaternita = findRiferimentoRichiestaMaternita.Valore;
                }

                try
                {
                    int max = 0;

                    if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                    {
                        foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
                        {
                            listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
                        }

                        if (listaPiatta != null && listaPiatta.Any())
                        {
                            var ragg = listaPiatta.GroupBy(w => w.DBRefAttribute);
                            max = ragg.Max(w => w.Count());
                        }
                    }

                    // per ogni attributo nell'oggetto XR_MAT_RICHIESTE cerco 
                    // il rispettivo elemento in objModuloValorizzato
                    // c'è un campo in quell'oggetto che definisce a quale attributo di XR_MAT_RICHIESTE
                    // fa riferimento quel valore.
                    foreach (PropertyInfo prop in typeof(XR_MAT_RICHIESTE).GetProperties())
                    {
                        var DBRefAttribute = objModuloValorizzato.Where(w => w.DBRefAttribute == prop.Name).FirstOrDefault();
                        if (DBRefAttribute != null)
                        {
                            // verifica la tipologia dell'elemento selezionato
                            var tipo = DBRefAttribute.Tipo;

                            if (tipo == TipologiaAttributoEnum.Testo || tipo == TipologiaAttributoEnum.FixedHiddenValue)
                            {
                                PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                                if (null != propToSet && propToSet.CanWrite)
                                {
                                    propToSet.SetValue(toSend, DBRefAttribute.Valore, null);
                                }
                            }

                            if (tipo == TipologiaAttributoEnum.Data)
                            {
                                DateTime? _data = null;
                                string dt = DBRefAttribute.Valore;
                                if (!String.IsNullOrEmpty(dt))
                                {
                                    if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                        throw new Exception("Errore in conversione " + prop.Name);
                                    _data = temp;
                                }

                                PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                                if (null != propToSet && propToSet.CanWrite)
                                {
                                    propToSet.SetValue(toSend, _data, null);
                                }
                            }

                            if (tipo == TipologiaAttributoEnum.Check || tipo == TipologiaAttributoEnum.Radio)
                            {
                                string _tempId = DBRefAttribute.Id;
                                string _gruopId = DBRefAttribute.Gruppo;
                                string valoreSelezionato = "";

                                var tt = objModuloValorizzato.Where(w => w.Gruppo == _gruopId && w.Checked).FirstOrDefault();
                                if (tt != null)
                                {
                                    valoreSelezionato = tt.Valore;
                                }

                                PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);

                                if (!String.IsNullOrEmpty(valoreSelezionato) &&
                                    ((valoreSelezionato.ToUpper() == "TRUE") || (valoreSelezionato.ToUpper() == "FALSE"))
                                    )
                                {
                                    var tipi = prop.PropertyType.GetGenericArguments();
                                    foreach (var t in tipi)
                                    {
                                        if (t.Name == "bool" || t.Name == "Boolean")
                                        {
                                            bool valore = Boolean.Parse(valoreSelezionato);

                                            if (null != propToSet && propToSet.CanWrite)
                                            {
                                                propToSet.SetValue(toSend, valore, null);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (null != propToSet && propToSet.CanWrite)
                                    {
                                        var propertyType = prop.PropertyType;
                                        if (System.Nullable.GetUnderlyingType(propertyType) != null && String.IsNullOrEmpty(valoreSelezionato))
                                        {
                                            // It's a nullable type
                                            propToSet.SetValue(toSend, null, null);
                                        }
                                        else
                                        {
                                            // It's not a nullable type
                                            propToSet.SetValue(toSend, valoreSelezionato, null);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // aggiunto per i casi GM che ora hanno le date come nei casi ON, cioè in linea
                    if (listaPiatta != null && listaPiatta.Any())
                    {
                        foreach (PropertyInfo prop in typeof(XR_MAT_RICHIESTE).GetProperties())
                        {
                            var DBRefAttribute = listaPiatta.Where(w => w.DBRefAttribute == prop.Name).FirstOrDefault();

                            if (DBRefAttribute != null)
                            {
                                // verifica la tipologia dell'elemento selezionato
                                var tipo = DBRefAttribute.Tipo;

                                if (tipo == TipologiaAttributoEnum.Testo || tipo == TipologiaAttributoEnum.FixedHiddenValue)
                                {
                                    PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                                    if (null != propToSet && propToSet.CanWrite)
                                    {
                                        propToSet.SetValue(toSend, DBRefAttribute.Valore, null);
                                    }
                                }

                                if (tipo == TipologiaAttributoEnum.Data)
                                {
                                    DateTime? _data = null;
                                    string dt = DBRefAttribute.Valore;
                                    if (!String.IsNullOrEmpty(dt))
                                    {
                                        if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                            throw new Exception("Errore in conversione " + prop.Name);
                                        _data = temp;
                                    }

                                    PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                                    if (null != propToSet && propToSet.CanWrite)
                                    {
                                        propToSet.SetValue(toSend, _data, null);
                                    }
                                }

                                if (tipo == TipologiaAttributoEnum.Check || tipo == TipologiaAttributoEnum.Radio)
                                {
                                    string _tempId = DBRefAttribute.Id;
                                    string _gruopId = DBRefAttribute.Gruppo;
                                    string valoreSelezionato = "";

                                    var tt = listaPiatta.Where(w => w.Gruppo == _gruopId && w.Checked).FirstOrDefault();
                                    if (tt != null)
                                    {
                                        valoreSelezionato = tt.Valore;
                                    }

                                    PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);

                                    if (!String.IsNullOrEmpty(valoreSelezionato) &&
                                        ((valoreSelezionato.ToUpper() == "TRUE") || (valoreSelezionato.ToUpper() == "FALSE"))
                                        )
                                    {
                                        var tipi = prop.PropertyType.GetGenericArguments();
                                        foreach (var t in tipi)
                                        {
                                            if (t.Name == "bool" || t.Name == "Boolean")
                                            {
                                                bool valore = Boolean.Parse(valoreSelezionato);

                                                if (null != propToSet && propToSet.CanWrite)
                                                {
                                                    propToSet.SetValue(toSend, valore, null);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (null != propToSet && propToSet.CanWrite)
                                        {
                                            var propertyType = prop.PropertyType;
                                            if (System.Nullable.GetUnderlyingType(propertyType) != null && String.IsNullOrEmpty(valoreSelezionato))
                                            {
                                                // It's a nullable type
                                                propToSet.SetValue(toSend, null, null);
                                            }
                                            else
                                            {
                                                // It's not a nullable type
                                                propToSet.SetValue(toSend, valoreSelezionato, null);
                                            }
                                        }
                                    }
                                }

                                listaPiatta.Remove(DBRefAttribute);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                toSend.MATRICOLA = doc.MatricolaDestinatario;
                toSend.NOMINATIVO = nominativo;
                toSend.CUSTOM_JSON = doc.CustomDataJSON;

                if ((String.IsNullOrEmpty(idRifMaternita)) || idRifMaternita == "0")
                {
                    toSend.ID = 0;
                }
                else
                {
                    int riferimento = 0;
                    bool convertito = int.TryParse(idRifMaternita, out riferimento);
                    if (convertito)
                    {
                        toSend.ID = riferimento;
                    }
                }

                result = ImportaRowExcel(toSend, stato, doc.Id, allegati);

                if (result != null && !result.Esito && !String.IsNullOrEmpty(result.DescrizioneErrore))
                {
                    throw new Exception(result.DescrizioneErrore);
                }

                List<string> eccezioniBaseOrariaDaInserire = new List<string>();
                eccezioniBaseOrariaDaInserire.Add("AF");
                eccezioniBaseOrariaDaInserire.Add("BF");
                eccezioniBaseOrariaDaInserire.Add("CF");

                // se l'eccezione è una di quelle gestite con base oraria
                // e fa parte del gruppo contenuto ineccezioniBaseOrariaDaInserire
                // allora vanno popolate due tabelle XR_MAT_PIANIFICAZIONI e
                // XR_MAT_GIORNI_CONGEDO per gestire gli inserimenti lato
                // raiperme
                if (toSend.PIANIFICAZIONE_BASE_ORARIA.HasValue &&
                    toSend.PIANIFICAZIONE_BASE_ORARIA.Value &&
                    eccezioniBaseOrariaDaInserire.Contains(toSend.ECCEZIONE))
                {
                    List<AttributiAggiuntivi> listaGiorni = new List<AttributiAggiuntivi>();

                    if (objModuloValorizzato != null &&
                        objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                    {
                        foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null &&
                                w.InLine.Any() &&
                                w.Id.StartsWith("dt_inline_giornate")).ToList())
                        {
                            listaGiorni.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
                        }

                        if (listaGiorni != null && listaGiorni.Any())
                        {
                            string nominativoApprovatore = DematerializzazioneManager.GetNominativoByMatricola(doc.MatricolaApprovatore);
                            AzioneResult testInserimentoGiorni = InserimentoGiorniPerEccezioneBaseOraria(result.IdRichiesta, doc.MatricolaApprovatore, nominativoApprovatore, doc.DataApprovazione.GetValueOrDefault(), listaGiorni);

                            if (testInserimentoGiorni != null && !testInserimentoGiorni.Esito &&
                                !String.IsNullOrEmpty(testInserimentoGiorni.DescrizioneErrore))
                            {
                                // azzera il record su maternità
                                throw new Exception(testInserimentoGiorni.DescrizioneErrore);
                            }
                        }
                    }
                }

                if (result.NuovoStato != (int)StatiDematerializzazioneDocumenti.PresaInCarico)
                {
                    result.NuovoStato = DematerializzazioneManager.GetNextIdStato(nuovoStato, doc.Id_WKF_Tipologia);
                }
            }

            return result;
        }

        private AzioneResult InserimentoGiorniPerEccezioneBaseOraria(int idRichiesta, string matricolaApprovatore, string nominativoApprovatore, DateTime dataApprovazione, List<AttributiAggiuntivi> listaGiorni)
        {
            AzioneResult result = new AzioneResult();
            result.Esito = false;

            try
            {
                Dictionary<string, string> _tempInfo = new Dictionary<string, string>();

                if (listaGiorni != null && listaGiorni.Any())
                {
                    for (int idx = 0; idx < listaGiorni.Count; idx = idx + 2)
                    {
                        string dt = listaGiorni[idx].Valore;
                        string tipo = listaGiorni[idx + 1].Valore;
                        _tempInfo.Add(dt, tipo);
                    }
                }

                if (_tempInfo.Count == 0)
                {
                    throw new Exception("Non è stato possibile reperire le giornate da inserire");
                }

                IncentiviEntities db = new IncentiviEntities();
                XR_MAT_PIANIFICAZIONI pianificazione = new XR_MAT_PIANIFICAZIONI()
                {
                    ID = -1,
                    ID_RICHIESTA = idRichiesta,
                    DATA_APPROVAZIONE = dataApprovazione,
                    DATA_INSERIMENTO = DateTime.Now,
                    MATRICOLA_APPROVATORE = matricolaApprovatore,
                    NOMINATIVO_APPROVATORE = nominativoApprovatore
                };

                db.XR_MAT_PIANIFICAZIONI.Add(pianificazione);
                int _myId = -1;
                foreach (var giorno in _tempInfo)
                {
                    DateTime data;
                    DateTime.TryParseExact(giorno.Key, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out data);
                    string fruizione = giorno.Value;

                    XR_MAT_GIORNI_CONGEDO congedoGiorno = new XR_MAT_GIORNI_CONGEDO()
                    {
                        ID = _myId,
                        ID_PIANIFICAZIONE = pianificazione.ID,
                        DATA = data,
                        FRUIZIONE = (fruizione == "P") ? "M" : fruizione,
                        ECCEZIONI_PRESENTI = null,
                        NOTE = null,
                        TIMESTAMP = DateTime.Now,
                        FRUIZIONE_TURNO = (fruizione == "Q") ? null : (fruizione == "P") ? "F" : "I",
                        FRUIZIONE_DECIMAL = (fruizione == "P" || fruizione == "M") ? 0.5m : 0.25m
                    };

                    db.XR_MAT_GIORNI_CONGEDO.Add(congedoGiorno);
                    _myId--;
                }

                db.SaveChanges();

            }
            catch (Exception ex)
            {
                result.DescrizioneErrore = ex.Message;
            }

            return result;
        }

        public AzioneAutomaticaResult AggiornaRowExcel(XR_MAT_RICHIESTE rich, string stato = "50", int idDoc = 0, List<XR_ALLEGATI> allegati = null)
        {
            AzioneAutomaticaResult result = new AzioneAutomaticaResult();
            result.NuovoStato = 0;
            result.IdRichiesta = 0;
            IncentiviEntities db = new IncentiviEntities();
            XR_MAT_RICHIESTE R = new XR_MAT_RICHIESTE();

            try
            {
                R = db.XR_MAT_RICHIESTE.Where(w => w.ID.Equals(rich.ID)).FirstOrDefault();

                if (R == null)
                {
                    throw new Exception("Impossibile reperire la richieta da aggiornare");
                }

                string nota = "";

                if (R.ECCEZIONE == "MT")
                {
                    if (R.CF_BAMBINO != rich.CF_BAMBINO)
                    {
                        R.CF_BAMBINO = rich.CF_BAMBINO;
                        db.SaveChanges();

                        // crea nota
                        MaternitaCongediController tempController = new MaternitaCongediController();

                        var AggiornaNascita = tempController.SalvaDataNascita(R.ID, rich.DATA_NASCITA_BAMBINO.GetValueOrDefault().ToString("dd/MM/yyyy"));

                        if (((dynamic)AggiornaNascita).Data.esito == false)
                        {
                            throw new Exception(((dynamic)AggiornaNascita).errore);
                        }
                        nota = "Inserita data nascita";
                    }

                    if (allegati != null && allegati.Any())
                    {
                        nota = nota + " ed allegati";

                        foreach (var a in allegati)
                        {
                            XR_MAT_ALLEGATI A = new XR_MAT_ALLEGATI();
                            #region recupero info file                
                            string fileName = a.NomeFile;
                            byte[] data = null;

                            if (a.ContentBytePDF != null)
                            {
                                data = a.ContentBytePDF;
                            }
                            else
                            {
                                data = a.ContentByte;
                            }
                            #endregion

                            A.NOMEFILE = fileName;
                            A.MATRICOLA = R.MATRICOLA;
                            A.UPLOAD_DA_AMMIN = false;
                            A.TIPOLOGIA = "DEM";
                            A.ID_RICHIESTA = R.ID;
                            A.ID_STATO = 20;
                            A.BYTECONTENT = data;
                            A.DATA_CONSOLIDATO = DateTime.Now;
                            A.DATA_INVIATO = DateTime.Now;
                            db.XR_MAT_ALLEGATI.Add(A);
                        }
                    }

                    string esito = MaternitaCongediManager.SalvaNota(rich.ID, nota, "*", null, null, null);

                    if (!String.IsNullOrEmpty(esito))
                    {
                        throw new Exception(esito);
                    }

                    db.SaveChanges();
                }

                result.Esito = true;
                result.DescrizioneErrore = null;
                result.IdRichiesta = R.ID;
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.DescrizioneErrore = ex.Message;
            }

            return result;
        }

        public AzioneAutomaticaResult ImportaRowExcel(XR_MAT_RICHIESTE rich, string stato = "50", int idDoc = 0, List<XR_ALLEGATI> allegati = null)
        {
            AzioneAutomaticaResult result = new AzioneAutomaticaResult();
            result.NuovoStato = 0;
            result.IdRichiesta = 0;
            bool richiestaCreata = false;
            try
            {
                XR_MAT_RICHIESTE R = new XR_MAT_RICHIESTE();
                IncentiviEntities db = new IncentiviEntities();
                digiGappEntities dbDigiGapp = new digiGappEntities();

                var eccezioniDaInserireConAzioneAutomatica = dbDigiGapp.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "EccezioniDaInserireConAzioneAutomatica");

                if (eccezioniDaInserireConAzioneAutomatica == null)
                {
                    throw new Exception("Eccezioni consentite non trovate");
                }

                if (rich.ID <= 0)
                {
                    R = rich;
                }
                else
                {
                    result = AggiornaRowExcel(rich, stato, idDoc, allegati);

                    if (result.Esito)
                    {
                        var myRisp = MaternitaCongediManager.IsAvviata(rich.ID);

                        var toUp = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                        if (toUp == null)
                        {
                            throw new Exception("Documento non trovato");
                        }

                        if (myRisp != null && myRisp.Avviata)
                        {
                            int nuovoStato = (int)StatiDematerializzazioneDocumenti.PresaInCarico;
                            toUp.Id_Stato = nuovoStato;
                            toUp.DataPresaInCarico = myRisp.DataAvviata.GetValueOrDefault();
                            toUp.MatricolaIncaricato = myRisp.MatricolaOperatore;
                            result.NuovoStato = nuovoStato;
                        }
                        toUp.Id_Richiesta = rich.ID;
                        db.SaveChanges();
                    }

                    return result;
                }

                if (R.ECCEZIONE != "PSC")
                {
                    R.CUSTOM_JSON = null;
                }
                R.IMPORTATA_DATETIME = DateTime.Now;
                R.IMPORTATA_MATRICOLA = null;
                R.DATA_INVIO_RICHIESTA = DateTime.Now;

                MyRaiServiceInterface.MyRaiServiceReference1.Utente anag = null;

                if (!R.INIZIO_GIUSTIFICATIVO.HasValue && !R.DATA_INIZIO_MATERNITA.HasValue)
                {
                    anag = BatchManager.GetUserData(R.MATRICOLA, null);
                }
                else
                {
                    anag = BatchManager.GetUserData(R.MATRICOLA, R.INIZIO_GIUSTIFICATIVO ?? R.DATA_INIZIO_MATERNITA.Value);
                }

                R.SEDEGAPP = anag.sede_gapp;
                R.REPARTO = anag.CodiceReparto;
                if (String.IsNullOrWhiteSpace(R.NOMINATIVO))
                {
                    result.Esito = false;
                    result.DescrizioneErrore = "Campo NOMINATIVO non trovato";
                    return result;
                }

                if (R.PIANIFICAZIONE_BASE_ORARIA.HasValue && !R.PIANIFICAZIONE_BASE_ORARIA.GetValueOrDefault())
                {
                    R.PIANIFICAZIONE_BASE_ORARIA = null;
                }

                if (R.ECCEZIONE != "MT" && R.DATA_INIZIO_MATERNITA.HasValue)
                {
                    R.INIZIO_GIUSTIFICATIVO = R.DATA_INIZIO_MATERNITA;
                    R.DATA_INIZIO_MATERNITA = null;
                }

                if (R.ECCEZIONE != "MT" && R.DATA_FINE_MATERNITA.HasValue)
                {
                    R.FINE_GIUSTIFICATIVO = R.DATA_FINE_MATERNITA;
                    R.DATA_FINE_MATERNITA = null;
                }

                if (R.ECCEZIONE == "MT")
                    R.ASSENZA_LUNGA = (R.DATA_FINE_MATERNITA.Value - R.DATA_INIZIO_MATERNITA.Value)
                        .TotalDays > 13;
                else
                    R.ASSENZA_LUNGA = (R.FINE_GIUSTIFICATIVO.Value - R.INIZIO_GIUSTIFICATIVO.Value)
                        .TotalDays > 13;

                // se esiste già una richiesta con quelle date e quell'eccezione che non sia in stato superiore a 80
                if (db.XR_MAT_RICHIESTE.Any(x => x.MATRICOLA == R.MATRICOLA && x.INIZIO_GIUSTIFICATIVO == R.INIZIO_GIUSTIFICATIVO
                && x.FINE_GIUSTIFICATIVO == R.FINE_GIUSTIFICATIVO &&
                (x.ECCEZIONE != null && x.ECCEZIONE.Equals(R.ECCEZIONE)) &&
                !x.XR_WKF_OPERSTATI.Any(z => z.ID_STATO > 80)))
                {

                    var prendiRichiesta = db.XR_MAT_RICHIESTE.Where(x => x.MATRICOLA == R.MATRICOLA && x.INIZIO_GIUSTIFICATIVO == R.INIZIO_GIUSTIFICATIVO
                && x.FINE_GIUSTIFICATIVO == R.FINE_GIUSTIFICATIVO &&
                (x.ECCEZIONE != null && x.ECCEZIONE.Equals(R.ECCEZIONE)) &&
                !x.XR_WKF_OPERSTATI.Any(z => z.ID_STATO > 80)).ToList();

                    if (prendiRichiesta != null && prendiRichiesta.Any())
                    {
                        var myItem = prendiRichiesta.OrderByDescending(w => w.ID).FirstOrDefault();

                        InserisciGiornateInPianoFerieBatch(eccezioniDaInserireConAzioneAutomatica, myItem, rich.MATRICOLA, anag.sede_gapp);

                        // Controlla se la pratica è stata avviata, da chi e quando
                        var myRisp = MaternitaCongediManager.IsAvviata(myItem.ID);

                        var toUp = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                        if (toUp == null)
                        {
                            throw new Exception("Documento non trovato");
                        }

                        if (myRisp != null && myRisp.Avviata)
                        {
                            int nuovoStato = (int)StatiDematerializzazioneDocumenti.PresaInCarico;
                            toUp.Id_Stato = nuovoStato;
                            toUp.DataPresaInCarico = myRisp.DataAvviata.GetValueOrDefault();
                            toUp.MatricolaIncaricato = myRisp.MatricolaOperatore;
                        }
                        toUp.Id_Richiesta = myItem.ID;
                        db.SaveChanges();

                        result.Esito = true;
                        result.DescrizioneErrore = null;
                        result.IdRichiesta = myItem.ID;

                        return result;
                    }
                    else
                    {
                        result.Esito = false;
                        result.DescrizioneErrore = "Periodo già presente per questa matricola";
                        return result;
                    }
                }

                if (R.ECCEZIONE == "MT")
                    R.NUMERO_GIORNI_GIUSTIFICATIVO = (int)(R.DATA_FINE_MATERNITA.Value - R.DATA_INIZIO_MATERNITA.Value).TotalDays;
                else if (R.ECCEZIONE == "PN")
                    R.NUMERO_GIORNI_GIUSTIFICATIVO = 2;
                else
                    R.NUMERO_GIORNI_GIUSTIFICATIVO = (int)(R.FINE_GIUSTIFICATIVO.Value - R.INIZIO_GIUSTIFICATIVO.Value).TotalDays;

                XR_MAT_CATEGORIE cat = null;

                if (!String.IsNullOrWhiteSpace(R.ECCEZIONE))
                {
                    cat = db.XR_MAT_CATEGORIE.Where(x => x.CODICE_DEMATERIALIZZAZIONE.Contains(R.ECCEZIONE)).FirstOrDefault();
                    if (cat != null)
                    {
                        R.ECCEZIONE = R.ECCEZIONE;
                        R.XR_MAT_CATEGORIE = cat;
                    }
                    else
                    {
                        result.Esito = false;
                        result.DescrizioneErrore = "Tipologia di congedo non trovata";
                        return result;
                    }
                }
                else
                {
                    result.Esito = false;
                    result.DescrizioneErrore = "Tipologia di congedo non trovata";
                    return result;
                }

                R.DATA_SCADENZA = MaternitaCongediManager.GetScadenza(R);

                /*
                 * se è una domanda di maternità (MT) ed esiste già una domanda di maternità per questa matricola con la stessa data presunto parto, 
                 * il cui stato non sia terminato/chiuso. Allora, nel caso in cui la data di inizio del nuovo giustificativo fosse contigua alla
                 * data di fine del giustificativo già esistente, verrà esteso il periodo della richiesta già esistente, portando la data fine
                 * alla data fine del nuovo giustificativo
                 */
                if (R.ECCEZIONE == "MT")
                {
                    // verifica se esiste già una richiesta con quei parametri
                    var richiestaMAT = db.XR_MAT_RICHIESTE.Where(w => w.MATRICOLA.Equals(R.MATRICOLA) &&
                                        w.ECCEZIONE.Equals(R.ECCEZIONE) &&
                                        w.DATA_NASCITA_BAMBINO == null &&
                                        w.DATA_PRESUNTA_PARTO != null &&
                                        R.DATA_PRESUNTA_PARTO != null &&
                                        w.DATA_PRESUNTA_PARTO == R.DATA_PRESUNTA_PARTO).OrderByDescending(w => w.ID).FirstOrDefault();

                    if (richiestaMAT != null)
                    {
                        // se la pratica è in stato da 80 in poi è una pratica chiusa/annullata o comunque da non considerare
                        var ok = db.XR_WKF_OPERSTATI.Count(w => w.COD_TIPO_PRATICA.Equals("MAT") && w.ID_GESTIONE.Equals(richiestaMAT.ID) && w.ID_STATO >= 80) == 0;

                        // se la pratica non è stata chiusa o annullata
                        if (ok)
                        {
                            // se la data presunta parto è contigua al periodo allora continua a funzionare come sempre
                            // vale anche se la nuova data racchiude la precedente.
                            DateTime? presunto = richiestaMAT.DATA_PRESUNTA_PARTO;

                            if (presunto.HasValue)
                            {
                                DateTime inizio = richiestaMAT.DATA_INIZIO_MATERNITA.GetValueOrDefault();
                                DateTime fine = richiestaMAT.DATA_FINE_MATERNITA.GetValueOrDefault();
                                DateTime finePiuUno = fine.AddDays(1);
                                DateTime nuovoInizio = R.DATA_INIZIO_MATERNITA.GetValueOrDefault();
                                DateTime nuovaFine = R.DATA_FINE_MATERNITA.GetValueOrDefault();

                                if (nuovoInizio == finePiuUno || (nuovoInizio <= fine && fine <= nuovaFine))
                                {
                                    // estende la richiesta con la nuova data fine
                                    richiestaMAT.DATA_FINE_MATERNITA = nuovaFine;
                                    R.DA_RIAVVIARE = true;
                                    db.SaveChanges();
                                    R = richiestaMAT;
                                }
                                else
                                {
                                    // non è contiguo
                                    db.XR_MAT_RICHIESTE.Add(R);
                                }
                            }
                            else
                            {
                                // non è contiguo
                                db.XR_MAT_RICHIESTE.Add(R);
                            }
                        }
                        else
                        {
                            // non è contiguo
                            db.XR_MAT_RICHIESTE.Add(R);
                        }
                    }
                    else
                    {
                        // non è contiguo
                        db.XR_MAT_RICHIESTE.Add(R);
                    }
                }

                ///////////////////////////////   STATI WKF /////////////////////////////
                int StatoPratica = Convert.ToInt32(stato);

                List<int> Stati = db.XR_MAT_STATI.Where(x => x.ID_STATO <= StatoPratica)
                                        .Select(x => x.ID_STATO).OrderBy(x => x).ToList();

                string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ip))
                {
                    ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }

                foreach (int s in Stati)
                {
                    XR_WKF_OPERSTATI ST = new XR_WKF_OPERSTATI()
                    {
                        COD_TERMID = ip,
                        COD_USER = CommonHelper.GetCurrentUserMatricola(),
                        VALID_DTA_INI = DateTime.Now,
                        TMS_TIMESTAMP = DateTime.Now,
                        NOMINATIVO = UtenteHelper.Nominativo(),
                        ID_STATO = s,
                        ID_TIPOLOGIA = 3,
                        XR_MAT_RICHIESTE = R,
                        COD_TIPO_PRATICA = cat.CAT //<----------------------------------------------

                    };
                    if (s == 30)//appr uffgest
                    {
                        R.PRESA_VISIONE_RESP_GEST = DateTime.Now;
                        R.PRESA_VISIONE_RESP_MATR = CommonHelper.GetCurrentRealUsername();
                    }
                    if (s == 50) // appr uffpers
                    {
                        ST.UFFPERS_PRESA_VISIONE = true;
                    }

                    db.XR_WKF_OPERSTATI.Add(ST);

                    R.XR_WKF_OPERSTATI.Add(ST);
                }

                if (allegati != null && allegati.Any())
                {
                    foreach (var a in allegati)
                    {
                        XR_MAT_ALLEGATI A = new XR_MAT_ALLEGATI();
                        #region recupero info file                
                        string fileName = a.NomeFile;
                        byte[] data = null;

                        if (a.ContentBytePDF != null)
                        {
                            data = a.ContentBytePDF;
                        }
                        else
                        {
                            data = a.ContentByte;
                        }
                        #endregion

                        A.NOMEFILE = fileName;
                        A.MATRICOLA = R.MATRICOLA;
                        A.UPLOAD_DA_AMMIN = false;
                        A.TIPOLOGIA = "DEM";
                        A.ID_RICHIESTA = R.ID;
                        A.ID_STATO = 20;
                        A.BYTECONTENT = data;
                        A.DATA_CONSOLIDATO = DateTime.Now;
                        A.DATA_INVIATO = DateTime.Now;
                        db.XR_MAT_ALLEGATI.Add(A);
                    }
                }
                db.SaveChanges();
                richiestaCreata = true;

                // se l'eccezione è una di quelle per le quali vanno fatti gli inserimenti in gapp
                // R.PIANIFICAZIONE_BASE_ORARIA == null significa che è una eccezioni per intera giornata
                InserisciGiornateInPianoFerieBatch(eccezioniDaInserireConAzioneAutomatica, R, rich.MATRICOLA, anag.sede_gapp);

                result.Esito = true;
                result.DescrizioneErrore = null;
                result.IdRichiesta = R.ID;
                return result;
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.DescrizioneErrore = "Tipologia di congedo non trovata";

                if (richiestaCreata)
                {
                    string esito = MaternitaCongediManager.cprat(rich.ID);
                }

                return result;
            }
        }

        public ActionResult InserisciGiornateInPianoFerieBatch()
        {
            string esito = null;
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                return RedirectToAction("Index");
            }

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                digiGappEntities dbDigiGapp = new digiGappEntities();

                string codEccezione = "";

                XR_DEM_DOCUMENTI doc = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(254)).FirstOrDefault();

                if (doc == null)
                {
                    throw new Exception("Documento non trovato");
                }

                List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);

                // deve caricare le eccezioni in myrai_pianoferiebatch
                var recuperoEccezione = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                if (recuperoEccezione != null)
                {
                    codEccezione = recuperoEccezione.Valore;
                }

                if (!String.IsNullOrEmpty(codEccezione))
                {
                    var eccezioniDaInserireConAzioneAutomatica = dbDigiGapp.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "EccezioniDaInserireConAzioneAutomatica");

                    if (eccezioniDaInserireConAzioneAutomatica == null)
                    {
                        throw new Exception("Eccezioni consentite non trovate");
                    }

                    List<XR_MAT_RICHIESTE> listaR = new List<XR_MAT_RICHIESTE>();
                    listaR = PreparaOggettoXR_MAT_RICHIESTE2(doc);

                    if (listaR == null || !listaR.Any())
                    {
                        throw new Exception("Errore nella generazione delle eccezioni da inserire in GAPP");
                    }

                    foreach (var R in listaR)
                    {
                        R.ECCEZIONE = codEccezione;
                        R.ID = 0;

                        if (R.PIANIFICAZIONE_BASE_ORARIA.HasValue && !R.PIANIFICAZIONE_BASE_ORARIA.GetValueOrDefault())
                        {
                            R.PIANIFICAZIONE_BASE_ORARIA = null;
                        }

                        if (R.ECCEZIONE != "MT" && R.DATA_INIZIO_MATERNITA.HasValue)
                        {
                            R.INIZIO_GIUSTIFICATIVO = R.DATA_INIZIO_MATERNITA;
                            R.DATA_INIZIO_MATERNITA = null;
                        }

                        if (R.ECCEZIONE != "MT" && R.DATA_FINE_MATERNITA.HasValue)
                        {
                            R.FINE_GIUSTIFICATIVO = R.DATA_FINE_MATERNITA;
                            R.DATA_FINE_MATERNITA = null;
                        }

                        var anag = BatchManager.GetUserData(R.MATRICOLA, R.INIZIO_GIUSTIFICATIVO ?? R.DATA_INIZIO_MATERNITA.Value);

                        // se l'eccezione è una di quelle per le quali vanno fatti gli inserimenti in gapp
                        // R.PIANIFICAZIONE_BASE_ORARIA == null significa che è una eccezioni per intera giornata
                        if (!String.IsNullOrEmpty(eccezioniDaInserireConAzioneAutomatica.Valore1) &&
                            eccezioniDaInserireConAzioneAutomatica.Valore1.Split(',').Contains(R.ECCEZIONE) &&
                            R.PIANIFICAZIONE_BASE_ORARIA == null)
                        {
                            DateTime inizioGiustificativo = R.INIZIO_GIUSTIFICATIVO.HasValue ? R.INIZIO_GIUSTIFICATIVO.GetValueOrDefault() : R.DATA_INIZIO_MATERNITA.GetValueOrDefault();
                            DateTime fineGiustificativo = R.FINE_GIUSTIFICATIVO.HasValue ? R.FINE_GIUSTIFICATIVO.GetValueOrDefault() : R.DATA_FINE_MATERNITA.GetValueOrDefault();

                            List<Giornata> giorni = new List<Giornata>();
                            // per il periodo Inizio - Fine giustificativo deve inserire una voce nella tabella myrai_pianoFerieBatch
                            // in questo modo il batch prenderà in carico le richieste di inserimento eccezione 
                            var pf = FeriePermessiManager.GetPianoFerieAnno(inizioGiustificativo.Year, true, R.MATRICOLA, false);
                            if (pf.esito)
                            {
                                if (pf.dipendente != null && pf.dipendente.ferie != null
                                    && pf.dipendente.ferie.giornate != null && pf.dipendente.ferie.giornate.Any())
                                {
                                    giorni.AddRange(pf.dipendente.ferie.giornate.ToList());

                                    foreach (var g in giorni)
                                    {
                                        string sGiorno = g.dataTeorica.Substring(0, 2);
                                        string sMese = g.dataTeorica.Substring(3, 2);
                                        int anno = inizioGiustificativo.Year;
                                        int giorno = int.Parse(sGiorno);
                                        int mese = int.Parse(sMese);
                                        DateTime nuovaData = DateTime.MinValue;
                                        try
                                        {
                                            nuovaData = new DateTime(anno, mese, giorno);
                                        }
                                        catch (Exception ex)
                                        {

                                        }

                                        g.data = nuovaData;
                                    }

                                    if (inizioGiustificativo.Year != fineGiustificativo.Year)
                                    {
                                        // se son diversi vuol dire che è un permesso a cavallo di 2 anni esempio un permesso che inizia a dicembre 2021 e termina a gennaio 2022
                                        pf = FeriePermessiManager.GetPianoFerieAnno(fineGiustificativo.Year, true, R.MATRICOLA, false);
                                        if (pf.esito)
                                        {
                                            if (pf.dipendente != null && pf.dipendente.ferie != null
                                                && pf.dipendente.ferie.giornate != null && pf.dipendente.ferie.giornate.Any())
                                            {
                                                foreach (var g in pf.dipendente.ferie.giornate.ToList())
                                                {
                                                    string sGiorno = g.dataTeorica.Substring(0, 2);
                                                    string sMese = g.dataTeorica.Substring(3, 2);
                                                    int anno = fineGiustificativo.Year;
                                                    int giorno = int.Parse(sGiorno);
                                                    int mese = int.Parse(sMese);

                                                    DateTime nuovaData = DateTime.MinValue;
                                                    try
                                                    {
                                                        nuovaData = new DateTime(anno, mese, giorno);
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }

                                                    g.data = nuovaData;
                                                    giorni.Add(g);
                                                }
                                            }
                                        }
                                    }
                                }
                                // a questo punto in giorni ci saranno tutte le giornate degli anni interessati
                                // prende solo le giornate incluse nel range della richiesta
                                string eccezioniAccettano9596 = eccezioniDaInserireConAzioneAutomatica.Valore2;

                                if (!String.IsNullOrEmpty(eccezioniAccettano9596) &&
                                    eccezioniAccettano9596.Split(',').Contains(R.ECCEZIONE))
                                {
                                    giorni = giorni.Where(w => w.data >= inizioGiustificativo && w.data <= fineGiustificativo).ToList();
                                }
                                else
                                {
                                    // in questo caso l'eccezione va inserita solo per le giornate il cui orario reale sia diverso da 
                                    // 95 e 96
                                    giorni = giorni.Where(w => w.data >= inizioGiustificativo && w.data <= fineGiustificativo &&
                                                        !String.IsNullOrWhiteSpace(w.orarioReale) &&
                                                        !w.orarioReale.StartsWith("9")).ToList();
                                }

                                foreach (var g in giorni)
                                {
                                    string ecc = R.ECCEZIONE.ToUpper();
                                    string prov = "HRIS/Dematerializzazione-DA_PIANOFERIE=FALSE-APPROVATORE_UFFPERS-ID_DOC=" + doc.Id;
                                    var exist = dbDigiGapp.MyRai_PianoFerieBatch.Count(w => w.matricola.Equals(doc.MatricolaDestinatario) &&
                                                                                        w.codice_eccezione.Equals(ecc) &&
                                                                                        w.data_eccezione == g.data &&
                                                                                        w.provenienza.Equals(prov)) > 0;

                                    if (!exist)
                                    {
                                        MyRai_PianoFerieBatch p = new MyRai_PianoFerieBatch()
                                        {
                                            codice_eccezione = ecc,
                                            provenienza = prov,
                                            dalle = "",
                                            alle = "",
                                            importo = "",
                                            data_eccezione = g.data,
                                            matricola = doc.MatricolaDestinatario,
                                            data_creazione_record = DateTime.Now,
                                            quantita = "1",
                                            sedegapp = anag.sede_gapp
                                        };
                                        dbDigiGapp.MyRai_PianoFerieBatch.Add(p);
                                    }
                                }
                                dbDigiGapp.SaveChanges();
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Impossibile reperire l'eccezione selezionata");
                }

            }
            catch (Exception e)
            {
                esito = e.Message;
            }

            if (esito == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = esito }
                };
            }
        }

        private void InserisciGiornateInPianoFerieBatch(MyRai_ParametriSistema eccezioniDaInserireConAzioneAutomatica, XR_MAT_RICHIESTE R, string matricola, string sedeGapp)
        {
            digiGappEntities dbDigiGapp = new digiGappEntities();

            // se l'eccezione è una di quelle per le quali vanno fatti gli inserimenti in gapp
            // R.PIANIFICAZIONE_BASE_ORARIA == null significa che è una eccezioni per intera giornata
            if (!String.IsNullOrEmpty(eccezioniDaInserireConAzioneAutomatica.Valore1) &&
                eccezioniDaInserireConAzioneAutomatica.Valore1.Split(',').Contains(R.ECCEZIONE) &&
                R.PIANIFICAZIONE_BASE_ORARIA == null)
            {
                DateTime inizioGiustificativo = R.INIZIO_GIUSTIFICATIVO.HasValue ? R.INIZIO_GIUSTIFICATIVO.GetValueOrDefault() : R.DATA_INIZIO_MATERNITA.GetValueOrDefault();
                DateTime fineGiustificativo = R.FINE_GIUSTIFICATIVO.HasValue ? R.FINE_GIUSTIFICATIVO.GetValueOrDefault() : R.DATA_FINE_MATERNITA.GetValueOrDefault();

                List<Giornata> giorni = new List<Giornata>();
                // per il periodo Inizio - Fine giustificativo deve inserire una voce nella tabella myrai_pianoFerieBatch
                // in questo modo il batch prenderà in carico le richieste di inserimento eccezione 
                var pf = FeriePermessiManager.GetPianoFerieAnno(inizioGiustificativo.Year, true, R.MATRICOLA, false);
                if (pf.esito)
                {
                    if (pf.dipendente != null && pf.dipendente.ferie != null
                        && pf.dipendente.ferie.giornate != null && pf.dipendente.ferie.giornate.Any())
                    {
                        giorni.AddRange(pf.dipendente.ferie.giornate.ToList());

                        foreach (var g in giorni)
                        {
                            string sGiorno = g.dataTeorica.Substring(0, 2);
                            string sMese = g.dataTeorica.Substring(3, 2);
                            int anno = inizioGiustificativo.Year;
                            int giorno = int.Parse(sGiorno);
                            int mese = int.Parse(sMese);
                            DateTime nuovaData = DateTime.MinValue;
                            try
                            {
                                nuovaData = new DateTime(anno, mese, giorno);
                            }
                            catch (Exception ex)
                            {

                            }

                            g.data = nuovaData;
                        }

                        if (inizioGiustificativo.Year != fineGiustificativo.Year)
                        {
                            // se son diversi vuol dire che è un permesso a cavallo di 2 anni esempio un permesso che inizia a dicembre 2021 e termina a gennaio 2022
                            pf = FeriePermessiManager.GetPianoFerieAnno(fineGiustificativo.Year, true, R.MATRICOLA, false);
                            if (pf.esito)
                            {
                                if (pf.dipendente != null && pf.dipendente.ferie != null
                                    && pf.dipendente.ferie.giornate != null && pf.dipendente.ferie.giornate.Any())
                                {
                                    foreach (var g in pf.dipendente.ferie.giornate.ToList())
                                    {
                                        string sGiorno = g.dataTeorica.Substring(0, 2);
                                        string sMese = g.dataTeorica.Substring(3, 2);
                                        int anno = fineGiustificativo.Year;
                                        int giorno = int.Parse(sGiorno);
                                        int mese = int.Parse(sMese);

                                        DateTime nuovaData = DateTime.MinValue;
                                        try
                                        {
                                            nuovaData = new DateTime(anno, mese, giorno);
                                        }
                                        catch (Exception ex)
                                        {

                                        }

                                        g.data = nuovaData;
                                        giorni.Add(g);
                                    }
                                }
                            }
                        }
                    }
                    // a questo punto in giorni ci saranno tutte le giornate degli anni interessati
                    // prende solo le giornate incluse nel range della richiesta
                    string eccezioniAccettano9596 = eccezioniDaInserireConAzioneAutomatica.Valore2;

                    if (!String.IsNullOrEmpty(eccezioniAccettano9596) &&
                        eccezioniAccettano9596.Split(',').Contains(R.ECCEZIONE))
                    {
                        giorni = giorni.Where(w => w.data >= inizioGiustificativo && w.data <= fineGiustificativo).ToList();
                    }
                    else
                    {
                        // in questo caso l'eccezione va inserita solo per le giornate il cui orario reale sia diverso da 
                        // 95 e 96
                        giorni = giorni.Where(w => w.data >= inizioGiustificativo && w.data <= fineGiustificativo &&
                                            !String.IsNullOrWhiteSpace(w.orarioReale) &&
                                            !w.orarioReale.StartsWith("9")).ToList();
                    }

                    foreach (var g in giorni)
                    {
                        string ecc = R.ECCEZIONE.ToUpper();
                        string prov = "HRIS/Dematerializzazione-DA_PIANOFERIE=FALSE-APPROVATORE_UFFPERS-ID_MAT_RICHIESTA=" + R.ID;
                        var exist = dbDigiGapp.MyRai_PianoFerieBatch.Count(w => w.matricola.Equals(matricola) &&
                                                                            w.codice_eccezione.Equals(ecc) &&
                                                                            w.data_eccezione == g.data &&
                                                                            w.provenienza.Equals(prov)) > 0;

                        if (!exist)
                        {
                            MyRai_PianoFerieBatch p = new MyRai_PianoFerieBatch()
                            {
                                codice_eccezione = ecc,
                                provenienza = prov,
                                dalle = "",
                                alle = "",
                                importo = "",
                                data_eccezione = g.data,
                                matricola = matricola,
                                data_creazione_record = DateTime.Now,
                                quantita = "1",
                                sedegapp = sedeGapp
                            };
                            dbDigiGapp.MyRai_PianoFerieBatch.Add(p);
                        }
                    }

                    dbDigiGapp.SaveChanges();
                }
            }
            else if (!String.IsNullOrEmpty(eccezioniDaInserireConAzioneAutomatica.Valore1) &&
    eccezioniDaInserireConAzioneAutomatica.Valore1.Split(',').Contains(R.ECCEZIONE) &&
    R.PIANIFICAZIONE_BASE_ORARIA != null)
            {
                // al momento non vengono inserite le eccezioni su base oraria
                // per le eccezioni di tipo AF, BF, CF che possono essere definite come
                // base oraria Q, M o P, il sistema memorizza nel json il dato
                // sarà poi raiperme a verificare che per una determinata giornata 
                // l'utente abbia la possibilità o meno di prendere un AFQ, AFM, AFP ...
            }
        }

        private void InserisciAllegatoMatRichieste(int idRichiesta, string matricola, List<XR_ALLEGATI> allegati, string tipologia = "DEM")
        {
            var db = new IncentiviEntities();

            foreach (var a in allegati)
            {
                XR_MAT_ALLEGATI A = new XR_MAT_ALLEGATI();
                #region recupero info file                
                string fileName = a.NomeFile;
                byte[] data = null;

                if (a.ContentBytePDF != null)
                {
                    data = a.ContentBytePDF;
                }
                else
                {
                    data = a.ContentByte;
                }
                #endregion

                A.NOMEFILE = fileName;
                A.MATRICOLA = matricola;
                A.UPLOAD_DA_AMMIN = false;
                A.TIPOLOGIA = tipologia;
                A.ID_RICHIESTA = idRichiesta;
                A.ID_STATO = 20;
                A.BYTECONTENT = data;
                A.DATA_CONSOLIDATO = DateTime.Now;
                A.DATA_INVIATO = DateTime.Now;
                db.XR_MAT_ALLEGATI.Add(A);
            }
            db.SaveChanges();
        }

        public ActionResult GetPDF(int idDoc)
        {
            try
            {
                RichiestaDoc documento = GetDocumentData(idDoc);
                //byte[] byteArray = documento.Allegato.ContentByte;
                //string nomefile = documento.Allegato.NomeFile;

                // SISTEMARE
                byte[] byteArray = null;
                string nomefile = "";

                MemoryStream pdfStream = new MemoryStream();
                pdfStream.Write(byteArray, 0, byteArray.Length);
                pdfStream.Position = 0;

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = "documento",
                    Inline = true,
                };

                Response.AddHeader("Content-Disposition", "inline; filename=" + nomefile + ".pdf");
                return File(byteArray, "application/pdf");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Metodo richiamato dagli uffici per leggere le richieste a loro inviate
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            AnagraficaModel anagraficaModel = null;
            DematerializzazioneDocumentiVM model = new DematerializzazioneDocumentiVM();
            model.Filtri = new Dematerializzazione_FiltriApprovatore();
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>();
            model.DocumentiDaPrendereInCarico = DematerializzazioneManager.GetDocumentiDaPrendereInCarico();
            model.DocumentiInCaricoAdAltri = DematerializzazioneManager.GetDocumentiInCaricoAltri(UtenteHelper.Matricola());
            model.DocumentiInCaricoAMe = DematerializzazioneManager.GetDocumentiInCaricoAMe(UtenteHelper.Matricola());
            model.IsPreview = false;
            model.PrendiInCaricoEnabled = true;
            model.Matricola = CommonHelper.GetCurrentUserMatricola();

            SezioniAnag sezione = SezioniAnag.NonDefinito;
            anagraficaModel = AnagraficaManager.GetAnagrafica(UtenteHelper.Matricola(), new AnagraficaLoader(sezione));

            if (anagraficaModel != null)
            {
                model.IdPersona = anagraficaModel.IdPersona;
                model.Matricola = anagraficaModel.Matricola;
            }

            model.IsPreview = true;
            return View(model);
        }

        /// <summary>
        /// Render della tabella contenente i documenti verso i settori interni RUO
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetDocumentiSettoriInterni()
        {
            AnagraficaModel anagraficaModel = null;
            DematerializzazioneDocumentiVM model = new DematerializzazioneDocumentiVM();
            model.Filtri = new Dematerializzazione_FiltriApprovatore();
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>();
            model.DocumentiDaPrendereInCarico = DematerializzazioneManager.GetDocumentiDaPrendereInCarico();
            model.DocumentiInCaricoAdAltri = DematerializzazioneManager.GetDocumentiInCaricoAltri(UtenteHelper.Matricola());
            model.DocumentiInCaricoAMe = DematerializzazioneManager.GetDocumentiInCaricoAMe(UtenteHelper.Matricola());
            model.IsPreview = false;
            model.PrendiInCaricoEnabled = true;
            model.Matricola = CommonHelper.GetCurrentUserMatricola();

            SezioniAnag sezione = SezioniAnag.NonDefinito;
            anagraficaModel = AnagraficaManager.GetAnagrafica(UtenteHelper.Matricola(), new AnagraficaLoader(sezione));

            if (anagraficaModel != null)
            {
                model.IdPersona = anagraficaModel.IdPersona;
                model.Matricola = anagraficaModel.Matricola;
            }

            model.IsPreview = true;
            return View("~/Views/Dematerializzazione/subpartial/_indexInternal.cshtml", model);
        }

        [HttpGet]
        public ActionResult GetViewDettaglio(string m, int id, int idDoc, bool approvazioneEnabled, bool presaInCaricoEnabled)
        {
            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM();
            model.Richiesta = GetDocumentData(idDoc);
            model.ApprovazioneEnabled = approvazioneEnabled;
            model.PresaInCaricoEnabled = presaInCaricoEnabled;
            string matricola = UtenteHelper.Matricola();
            if (model.Richiesta != null)
            {
                model.Matricola = m;
                model.IdPersona = id;

                model.NominativoUtenteApprovatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaApprovatore);
                model.NominativoUtenteCreatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaCreatore);
                model.NominativoUtenteDestinatario = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaDestinatario);
                model.NominativoUtenteFirma = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaFirma);
                model.NominativoUtenteIncaricato = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaIncaricato);
                model.NominativoUtenteVistatore = new List<string>();

                if (!String.IsNullOrEmpty(model.Richiesta.Documento.MatricolaVisualizzatore))
                {
                    model.NominativoUtenteVisionatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaVisualizzatore);
                }
                else
                {
                    string tipologiaDocumentale = model.Richiesta.Documento.Cod_Tipologia_Documentale;
                    int id_tipo_doc = model.Richiesta.Documento.Id_Tipo_Doc;
                    string tipologiaDocumento = "";
                    IncentiviEntities db = new IncentiviEntities();
                    var dem_TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(id_tipo_doc)).FirstOrDefault();

                    if (dem_TIPI_DOCUMENTO != null)
                    {
                        tipologiaDocumento = dem_TIPI_DOCUMENTO.Codice;
                    }

                    var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                    if (comportamento == null)
                    {
                        throw new Exception("Comportamento non trovato");
                    }

                    if (comportamento.Visionatore)
                    {
                        List<NominativoMatricola> items = null;

                        string tipologia = String.Format("{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());
                        items = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

                        if (items != null && items.Any())
                        {
                            foreach (var i in items)
                            {
                                string _tempNominativo = "";
                                var r = AuthHelper.EnableToMatr(i.Matricola, matricola, "DEMA", "01VIST");
                                if (r.Enabled)
                                {
                                    _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                                }
                                // se non c'è matricola, allora è una tipologia come ad esempio 
                                // APPUNTO, che non ha matricola dipendente, ma ha comunque un vistatore
                                var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                                .Where(w => w.MATRICOLA == i.Matricola && w.IND_ATTIVO)
                                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01VIST")
                                .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                                .FirstOrDefault();

                                if (_myAbil != null)
                                {
                                    #region POSSO VISTATORE 
                                    //Calcolo degli elementi che posso firmare
                                    List<string> possoVistare = new List<string>();

                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                        && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                                    {
                                        possoVistare.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                                    }

                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                        && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                                        && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                                    {
                                        possoVistare.Add(_myAbil.TIP_DOC_INCLUSI);
                                    }

                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                        && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                                    {
                                        possoVistare.Add(tipologia);
                                    }
                                    #endregion

                                    #region NON POSSO VISTATORE 
                                    //Calcolo degli elementi che non posso vistare
                                    List<string> nonPossoVistare = new List<string>();

                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                        && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                                    {
                                        nonPossoVistare.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                                    }

                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                        && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                                        && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                    {
                                        nonPossoVistare.Add(_myAbil.TIP_DOC_ESCLUSI);
                                    }

                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                        && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                    {
                                        // se però possoVistare contiene tipologia allora non va aggiunta
                                        if (!possoVistare.Contains(tipologia))
                                        {
                                            nonPossoVistare.Add(tipologia);
                                        }
                                    }
                                    #endregion

                                    #region CASO LIMITE TUTTI E DUE *
                                    if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                                        !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                            && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                                            && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                    {
                                        possoVistare = new List<string>();
                                        nonPossoVistare = new List<string>();
                                    }
                                    #endregion

                                    List<string> tipologieAbilitate = possoVistare.Except(nonPossoVistare).ToList();

                                    if (tipologieAbilitate != null &&
                                        tipologieAbilitate.Any())
                                    {
                                        if (tipologieAbilitate.Contains(tipologia))
                                        {
                                            _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim())) + "<br/>";
                                        }
                                        else
                                        {
                                            _tempNominativo = "";
                                        }
                                    }
                                }

                                if (!String.IsNullOrEmpty(_tempNominativo))
                                {
                                    model.NominativoUtenteVistatore.Add(_tempNominativo);
                                }
                            }
                        }
                    }
                }
            }

            return View("~/Views/Dematerializzazione/Modal_Dettaglio.cshtml", model);
        }

        [HttpGet]
        public ActionResult Modal_Viewer_ReadOnly(string m, int id, int idDoc)
        {
            RichiestaDoc documento = GetDocumentData(idDoc);
            documento.IdPersona = id;
            documento.Matricola = m;

            if (documento != null)
            {
                // SISTEMARE
                //if ( documento.Allegato != null )
                //{
                //    // il pdf va convertito in png
                //    byte[] convertito = ConvertPDFTOPNG( documento.Allegato.ContentByte );
                //    if ( convertito != null && convertito.Length > 0 )
                //    {
                //        documento.PNGBase64 = Convert.ToBase64String( convertito );
                //    }
                //}
            }

            return View("~/Views/Dematerializzazione/subpartial/Modal_FileViewer.cshtml", documento);
        }

        public JsonResult SetScelteFase1(string tipologiaDocumentale, string tipologiaDocumento, string destinatario)
        {
            try
            {
                string matricola = UtenteHelper.Matricola();

                #region REGISTRA LA SCELTA FATTA
                if (String.IsNullOrEmpty(tipologiaDocumentale) ||
                    String.IsNullOrWhiteSpace(tipologiaDocumento))
                {
                    throw new Exception("Errore nei dati di input");
                }

                SessionHelper.Set(matricola + "tipologiaDocumentale", tipologiaDocumentale);
                SessionHelper.Set(matricola + "tipologiaDocumento", tipologiaDocumento);
                SessionHelper.Set(matricola + "MATRICOLA_DESTINATARIO", destinatario);
                #endregion

                #region CALCOLA IL COMPORTAMENTO
                var db = AnagraficaManager.GetDb();
                XR_WKF_TIPOLOGIA WKF_TIPOLOGIA = null;

                WKF_TIPOLOGIA = Get_XR_WKF_TIPOLOGIA(tipologiaDocumentale, tipologiaDocumento);

                if (WKF_TIPOLOGIA == null)
                {
                    // se anche in questo caso è null allora c'è un errore
                    throw new Exception("XR_WKF_TIPOLOGIA non trovata");
                }

                SessionHelper.Set(matricola + "WKF_TIPOLOGIA", WKF_TIPOLOGIA.ID_TIPOLOGIA);

                var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
                w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                if (comportamento == null)
                {
                    throw new Exception("Comportamento non trovato");
                }

                string custom = comportamento.CustomDataJSON;
                string customView = comportamento.CustomView;
                bool isCustomType = (!String.IsNullOrEmpty(custom) || !String.IsNullOrEmpty(customView));
                string nominativoUtenteVistatore = "";

                if (comportamento.Visionatore)
                {
                    string tipologia = String.Format("{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());
                    List<NominativoMatricola> items = null;
                    items = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

                    if (items != null && items.Any())
                    {
                        foreach (var i in items)
                        {
                            string _tempNominativo = "";
                            if (!String.IsNullOrEmpty(destinatario))
                            {
                                var r = AuthHelper.EnableToMatr(i.Matricola, matricola, "DEMA", "01VIST");
                                if (r.Enabled)
                                {
                                    _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim())) + "<br/>";
                                }
                            }
                            // se non c'è matricola, allora è una tipologia come ad esempio 
                            // APPUNTO, che non ha matricola dipendente, ma ha comunque un vistatore
                            var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                            .Where(w => w.MATRICOLA == i.Matricola && w.IND_ATTIVO)
                            .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                            .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01VIST")
                            .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                            .FirstOrDefault();

                            if (_myAbil != null)
                            {
                                #region POSSO VISTATORE 
                                //Calcolo degli elementi che posso firmare
                                List<string> possoVistare = new List<string>();

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                    && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                                {
                                    possoVistare.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                                }

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                    && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                                    && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                                {
                                    possoVistare.Add(_myAbil.TIP_DOC_INCLUSI);
                                }

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                    && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                                {
                                    possoVistare.Add(tipologia);
                                }
                                #endregion

                                #region NON POSSO VISTATORE 
                                //Calcolo degli elementi che non posso vistare
                                List<string> nonPossoVistare = new List<string>();

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                    && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                                {
                                    nonPossoVistare.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                                }

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                    && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                                    && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                {
                                    nonPossoVistare.Add(_myAbil.TIP_DOC_ESCLUSI);
                                }

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                    && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                {
                                    // se però possoVistare contiene tipologia allora non va aggiunta
                                    if (!possoVistare.Contains(tipologia))
                                    {
                                        nonPossoVistare.Add(tipologia);
                                    }
                                }
                                #endregion

                                #region CASO LIMITE TUTTI E DUE *
                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                                    !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                        && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                                        && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                {
                                    possoVistare = new List<string>();
                                    nonPossoVistare = new List<string>();
                                }
                                #endregion

                                List<string> tipologieAbilitate = possoVistare.Except(nonPossoVistare).ToList();

                                if (tipologieAbilitate != null &&
                                    tipologieAbilitate.Any())
                                {
                                    if (tipologieAbilitate.Contains(tipologia))
                                    {
                                        _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim())) + "<br/>";
                                    }
                                    else
                                    {
                                        _tempNominativo = "";
                                    }
                                }
                            }

                            if (!String.IsNullOrEmpty(_tempNominativo))
                            {
                                nominativoUtenteVistatore += _tempNominativo;
                            }
                        }
                    }
                }


                var item = new
                {
                    Id = comportamento.Id,
                    Codice_Tipologia_Documentale = comportamento.Codice_Tipologia_Documentale,
                    Codice_Tipo_Documento = comportamento.Codice_Tipo_Documento,
                    MacroAggregato = comportamento.MacroAggregato,
                    ConsentiRifiuto = comportamento.ConsentiRifiuto,
                    ApprovazioneObbligatoria = comportamento.ApprovazioneObbligatoria,
                    ApprovatoreVisibile = comportamento.ApprovatoreVisibile,
                    FirmaObbligatoria = comportamento.FirmaObbligatoria,
                    FirmaVisibile = comportamento.FirmaVisibile,
                    PosizionaProtocollo = comportamento.PosizionaProtocollo,
                    WKF_TIPOLOGIA = WKF_TIPOLOGIA.COD_TIPOLOGIA,
                    TipologiaDocumentale = tipologiaDocumentale,
                    TipologiaDocumento = tipologiaDocumento,
                    FileObbligatorio = comportamento.FileObbligatorio,
                    IsCustomType = isCustomType,
                    FileAggiuntivoObbligatorio = comportamento.FileAggiuntivoObbligatorio,
                    MatricolaDestinatarioVisibile = comportamento.MatricolaDestinatarioVisibile,
                    NominativoUtenteVistatore = nominativoUtenteVistatore,
                    PannelloAllegati = comportamento.PannelloAllegati
                };

                return Json(item, JsonRequestBehavior.AllowGet);
                #endregion
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public JsonResult GetElencoApprovatoriJSON(string tipologiaDocumentale = null, string tipologiaDocumento = null, string matr = null, int? idPratica = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            IncentiviEntities db = new IncentiviEntities();
            XR_WKF_TIPOLOGIA WKF_TIPOLOGIA = null;
            List<string> tipologieAbilitate = null;

            string matricola = CommonHelper.GetCurrentUserMatricola();
            matr = matricola;

            if (String.IsNullOrEmpty(tipologiaDocumentale))
            {
                tipologiaDocumentale = (string)SessionHelper.Get(matricola + "tipologiaDocumentale");
            }

            if (String.IsNullOrEmpty(tipologiaDocumento))
            {
                tipologiaDocumento = (string)SessionHelper.Get(matricola + "tipologiaDocumento");
            }

            if (String.IsNullOrEmpty(tipologiaDocumentale) ||
                String.IsNullOrEmpty(tipologiaDocumento))
            {
                result.Add(new SelectListItem()
                {
                    Value = "-1",
                    Text = "Seleziona un approvatore"
                });
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            WKF_TIPOLOGIA = Get_XR_WKF_TIPOLOGIA(tipologiaDocumentale, tipologiaDocumento);

            if (WKF_TIPOLOGIA == null)
            {
                // se anche in questo caso è null allora c'è un errore
                throw new Exception("XR_WKF_TIPOLOGIA non trovata");
            }

            var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
                w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

            if (comportamento == null)
            {
                throw new Exception("Comportamento non trovato");
            }

            if (!comportamento.ApprovazioneObbligatoria)
            {
                result.Add(new SelectListItem()
                {
                    Value = "0",
                    Text = "Nessun approvatore"
                });
            }

            // verifica se nel workflow c'è un elemento abbia 01APPR come destinatario
            var destinatario = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(WKF_TIPOLOGIA.ID_TIPOLOGIA) &&
            (w.DESTINATARIO.Contains("01APPR") || w.DESTINATARIO.Contains("01ADM"))).FirstOrDefault();

            if (
                destinatario == null ||
                (destinatario != null && String.IsNullOrEmpty(destinatario.DESTINATARIO))
                )
            {
                if (comportamento.ApprovazioneObbligatoria)
                {
                    result.Add(new SelectListItem()
                    {
                        Value = "0",
                        Text = "Nessun approvatore"
                    });
                }
            }
            else
            {
                List<NominativoMatricola> items = AuthHelper.GetAllEnabledAs("DEMA", "01APPR", true);

                if (items != null && items.Any())
                {
                    foreach (var i in items)
                    {
                        var r = AuthHelper.EnableToMatr(i.Matricola, matr, "DEMA", "01APPR");
                        if (r.Enabled && r.Visibilita == AbilMatrLiv.VisibilitaEnum.Filtrata)
                        {
                            List<string> nonPosso = new List<string>();

                            // se non c'è matricola, allora è una tipologia come ad esempio 
                            // APPUNTO, che non ha matricola dipendente, ma ha comunque un vistatore
                            var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                                            .Where(w => w.MATRICOLA == i.Matricola && w.IND_ATTIVO)
                                            .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                                            .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01APPR")
                                            .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                                            .FirstOrDefault();

                            if (_myAbil != null)
                            {
                                #region POSSO SEGRETERIA 
                                //Calcolo degli elementi che posso lavorare
                                List<string> posso = new List<string>();

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                    && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                                {
                                    posso.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                                }

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                    && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                                    && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                                {
                                    posso.Add(_myAbil.TIP_DOC_INCLUSI);
                                }

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                    && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                                {
                                    posso.Add("*");
                                }
                                #endregion

                                #region NON POSSO SEGRETERIA 
                                //Calcolo degli elementi che non posso lavorare

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                    && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                                {
                                    nonPosso.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                                }

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                    && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                                    && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                {
                                    nonPosso.Add(_myAbil.TIP_DOC_ESCLUSI);
                                }

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                    && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                {
                                    nonPosso.Add("*");
                                }
                                #endregion

                                #region CASO LIMITE TUTTI E DUE *
                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                                    !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                        && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                                        && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                {
                                    posso = new List<string>();
                                    nonPosso = new List<string>();
                                }
                                #endregion

                                tipologieAbilitate = posso.Except(nonPosso).ToList();

                                if (tipologieAbilitate.Contains("*"))
                                {
                                    // null non applicherà il filtro sulla tipologia perchè abilitato a tutte
                                    tipologieAbilitate = null;
                                }
                            }

                            string _temp = String.Format("{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());

                            if (tipologieAbilitate != null && tipologieAbilitate.Any())
                            {
                                if (tipologieAbilitate.Contains(_temp))
                                {
                                    result.Add(new SelectListItem()
                                    {
                                        Value = i.Matricola,
                                        Text = i.Cognome + " " + i.Nome
                                    });
                                }
                            }
                            else if (nonPosso != null && nonPosso.Any())
                            {
                                if (!nonPosso.Contains(_temp))
                                {
                                    result.Add(new SelectListItem()
                                    {
                                        Value = i.Matricola,
                                        Text = i.Cognome + " " + i.Nome
                                    });
                                }
                            }
                            else
                            {
                                result.Add(new SelectListItem()
                                {
                                    Value = i.Matricola,
                                    Text = i.Cognome + " " + i.Nome
                                });
                            }
                        }
                    }
                }
            }

            if (idPratica.HasValue && idPratica > 0)
            {
                if (result == null)
                {
                    result = new List<SelectListItem>();
                }
                var doc = db.XR_DEM_DOCUMENTI.Where(w => w.Id == idPratica).FirstOrDefault();
                string matricolaApp = doc.MatricolaApprovatore;
                if (!String.IsNullOrEmpty(matricolaApp))
                {
                    bool esisteApprovatoreInLista = result.Count(w => w.Value == matricolaApp) > 0;
                    if (esisteApprovatoreInLista)
                    {
                        var seleziona = result.Where(w => w.Value == matricolaApp).FirstOrDefault();
                        seleziona.Selected = true;
                    }
                    else
                    {
                        // se non è presente in lista, va aggiunto visto che comunque è stato
                        // assegnato al documento.
                        // Potrebbe non essere presente il lista perchè l'utente corrente non 
                        // ha la matricola associata a quell'approvatore e quindi non ha visibilità 
                        // su di esso.
                        result.Add(new SelectListItem()
                        {
                            Value = matricolaApp,
                            Text = DematerializzazioneManager.GetNominativoByMatricola(matricolaApp),
                            Selected = true
                        });
                    }
                }
            }

            if (result != null && result.Any())
            {
                result = result.DistinctBy(w => w.Text).ToList();
                foreach (var r in result)
                {
                    r.Text = CommonHelper.ToTitleCase(r.Text);
                }
                if (result.Count == 1)
                {
                    result[0].Selected = true;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDettaglioRichiestaApprovatore(string m, int id, int idDoc)
        {
            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM();
            model.Richiesta = GetDocumentData(idDoc);
            model.ApprovazioneEnabled = true;
            model.PresaInCaricoEnabled = false;
            model.NascondiEliminaPratica = true;
            model.NascondiConcludiPratica = true;

            if (String.IsNullOrEmpty(m))
            {
                m = UtenteHelper.Matricola();
            }

            if (id == 0)
            {
                id = CommonHelper.GetCurrentIdPersona();
            }

            if (model.Richiesta != null)
            {
                model.Matricola = m;
                model.IdPersona = id;

                model.NominativoUtenteApprovatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaApprovatore);
                model.NominativoUtenteCreatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaCreatore);
                model.NominativoUtenteDestinatario = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaDestinatario);
                model.NominativoUtenteFirma = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaFirma);
                model.NominativoUtenteIncaricato = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaIncaricato);
                model.NominativoUtenteVistatore = new List<string>();

                if (!String.IsNullOrEmpty(model.Richiesta.Documento.MatricolaVisualizzatore))
                {
                    model.NominativoUtenteVisionatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaVisualizzatore);
                }
                else
                {
                    IncentiviEntities db = new IncentiviEntities();
                    string tipologiaDocumentale = model.Richiesta.Documento.Cod_Tipologia_Documentale;
                    int id_tipo_doc = model.Richiesta.Documento.Id_Tipo_Doc;
                    string tipologiaDocumento = "";
                    var dem_TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(id_tipo_doc)).FirstOrDefault();

                    if (dem_TIPI_DOCUMENTO != null)
                    {
                        tipologiaDocumento = dem_TIPI_DOCUMENTO.Codice;
                    }

                    var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                    if (comportamento == null)
                    {
                        throw new Exception("Comportamento non trovato");
                    }

                    if (comportamento.Visionatore)
                    {
                        List<NominativoMatricola> items = null;

                        items = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

                        if (items != null && items.Any())
                        {
                            foreach (var i in items)
                            {
                                var r = AuthHelper.EnableToMatr(i.Matricola, model.Richiesta.Documento.MatricolaCreatore, "DEMA", "01VIST");
                                if (r.Enabled)
                                {
                                    model.NominativoUtenteVistatore.Add(CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                                }
                            }
                        }
                    }
                }
            }

            return View("~/Views/Dematerializzazione/Modal_DettaglioRichiesta.cshtml", model);
        }

        #region private

        private XR_WKF_TIPOLOGIA Get_XR_WKF_TIPOLOGIA(string tipologiaDocumentale, string tipologiaDocumento)
        {
            XR_WKF_TIPOLOGIA result = null;
            var db = AnagraficaManager.GetDb();

            try
            {
                string tipologia = String.Format("DEMDOC_{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());

                result = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(tipologia)).FirstOrDefault();

                if (result == null)
                {
                    // se è null allora la tipologia è formata dalla sola DEMDOC_TipologiaDocumentale senza 
                    // il tipo documento
                    tipologia = String.Format("DEMDOC_{0}", tipologiaDocumentale.Trim());

                    result = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(tipologia)).FirstOrDefault();
                }

                if (result == null)
                {
                    // se anche in questo caso è null allora c'è un errore
                    throw new Exception("XR_WKF_TIPOLOGIA non trovata");
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        private byte[] ApplicaProtocollo(string protocollo, float left, float top,
                                            float? dataPosLeft, float? dataPosTop, byte[] originePDF)
        {
            byte[] result = null;

            try
            {
                using (var reader = new PdfReader(originePDF))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Document document = null;
                        document = new Document(reader.GetPageSizeWithRotation(1));

                        var writer = PdfWriter.GetInstance(document, ms);
                        float width = document.PageSize.Width;
                        float height = document.PageSize.Height;

                        document.Open();

                        for (var i = 1; i <= reader.NumberOfPages; i++)
                        {
                            document.NewPage();

                            var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            var importedPage = writer.GetImportedPage(reader, i);

                            var contentByte = writer.DirectContent;

                            var rotation = reader.GetPageRotation(i);

                            switch (rotation)
                            {
                                case 90:
                                    contentByte.AddTemplate(importedPage, 0, -1, 1, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                                    break;
                                // TODO case 180
                                case 270:
                                    contentByte.AddTemplate(importedPage, 0, 1, -1, 0, reader.GetPageSizeWithRotation(i).Width, 0);
                                    break;
                                default:
                                    contentByte.AddTemplate(importedPage, 0, 0);
                                    break;
                            }

                            contentByte.BeginText();
                            contentByte.SetFontAndSize(baseFont, 12);

                            if ((595 - left) < 142)
                            {
                                // il margine sinistro va oltre il documento
                                left = 595 - 142;
                            }

                            if (i == 1)
                            {
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, protocollo, left, 842 - top - 12, 0);

                                //if (dataPosLeft.HasValue && dataPosTop.HasValue)
                                //{
                                //    DateTime adesso = DateTime.Now;
                                //    contentByte.ShowTextAligned( PdfContentByte.ALIGN_LEFT ,
                                //        adesso.ToString( "dd/MM/yyyy" ) ,
                                //        dataPosLeft.GetValueOrDefault( ) ,
                                //        842 - dataPosTop.GetValueOrDefault( ) - 12 ,
                                //        0 );
                                //}
                            }

                            contentByte.EndText();
                        }

                        document.Close();
                        writer.Close();
                        result = ms.ToArray();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private byte[] ConvertPDFTOPNG(byte[] origine, int pagina = 1)
        {
            byte[] result = null;

            try
            {
                string ghostScriptPath = "";
                string tempJPEGFileName = "";
                string tempPDFFileName = "";
                string realName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                // path dove trovare il file exe
                if (!CommonHelper.IsProduzione() && (realName.Contains("\\Francesco") || realName.Contains("a863492")))
                {
                    ghostScriptPath = @"c:/ghostScript/gswin64c.exe";
                    tempJPEGFileName = @"c:/GhostScriptTempFiles/" + string.Format(@"{0}.jpg", Guid.NewGuid());
                    tempPDFFileName = @"c:/GhostScriptTempFiles/" + string.Format(@"{0}.pdf", Guid.NewGuid());
                }
                else
                {
                    ghostScriptPath = Server.MapPath(@"~/ghostScript/gswin64c.exe");
                    tempJPEGFileName = Server.MapPath(@"~/GhostScriptTempFiles/") + string.Format(@"{0}.jpg", Guid.NewGuid());
                    tempPDFFileName = Server.MapPath(@"~/GhostScriptTempFiles/") + string.Format(@"{0}.pdf", Guid.NewGuid());
                }

                System.IO.File.WriteAllBytes(tempPDFFileName, origine);

                if (System.IO.File.Exists(tempPDFFileName))
                {
                    // argomenti
                    String ars = " -q -dQUIET -dSAFER -dBATCH -dNOPAUSE -dNOPROMPT -dMaxBitmap=500000000 -dAlignToPixels=0 -dGridFitTT=2 -sDEVICE=jpeg -dJPEGQ=100 -r200 -dFirstPage=" + pagina.ToString() + " -dLastPage=" + pagina.ToString() + " -sOutputFile=" + tempJPEGFileName + " " + tempPDFFileName;

                    try
                    {
                        Process proc = new Process();
                        proc.StartInfo.FileName = ghostScriptPath;
                        proc.StartInfo.Arguments = ars;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        proc.Start();
                        proc.WaitForExit();
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.LogErrori(new MyRai_LogErrori()
                        {
                            applicativo = "PORTALE",
                            data = DateTime.Now,
                            matricola = CommonHelper.GetCurrentUserMatricola(),
                            provenienza = "DematerializzazioneController.ConvertPDFTOPNG",
                            error_message = ex.ToString()
                        });
                    }
                    catch (Win32Exception ex)
                    {
                        Logger.LogErrori(new MyRai_LogErrori()
                        {
                            applicativo = "PORTALE",
                            data = DateTime.Now,
                            matricola = CommonHelper.GetCurrentUserMatricola(),
                            provenienza = "DematerializzazioneController.ConvertPDFTOPNG",
                            error_message = ex.ToString()
                        });
                    }
                }

                if (System.IO.File.Exists(tempJPEGFileName))
                {
                    result = System.IO.File.ReadAllBytes(tempJPEGFileName);
                }

                if (System.IO.File.Exists(tempPDFFileName))
                {
                    System.IO.File.Delete(tempPDFFileName);
                }

                if (System.IO.File.Exists(tempJPEGFileName))
                {
                    System.IO.File.Delete(tempJPEGFileName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "DematerializzazioneController.ConvertPDFTOPNG",
                    error_message = ex.ToString()
                });
            }

            return result;
        }

        //private List<XR_RICHIESTE> GetRichieste ( DEM_TIPI_DOCUMENTO_ENUM? tipoDoc = null )
        //{
        //    List<XR_RICHIESTE> richieste = new List<XR_RICHIESTE>( );
        //    var db = AnagraficaManager.GetDb( );
        //    try
        //    {
        //        if ( tipoDoc != null )
        //        {
        //            richieste = db.XR_RICHIESTE.Where( w => w.Id_Tipologia.Equals( ( int ) tipoDoc ) ).ToList( );
        //        }
        //        else
        //        {
        //            richieste = db.XR_RICHIESTE.ToList( );
        //        }
        //    }
        //    catch ( Exception ex )
        //    {
        //        richieste = null;
        //    }
        //    return richieste;
        //}

        private static string SerializeObject<T>(T oggetto) where T : class
        {
            string result;
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(oggetto.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            using (Utf8StringWriter sw = new Utf8StringWriter())
            {
                using (var wri = XmlWriter.Create(sw, settings))
                {
                    serializer.Serialize(wri, oggetto, emptyNamespaces);
                    result = sw.ToString();
                }
            }

            return result;
        }

        #endregion

        #region static
        public static List<SelectListItem> GetTipologieDocumentali(string codice = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            result.Add(new SelectListItem()
            {
                Value = "-1",
                Text = "Seleziona un valore"
            });

            var db = AnagraficaManager.GetDb();
            string matricolaUtenteCorrente = UtenteHelper.Matricola();

            result.AddRange(db.XR_DEM_TIPOLOGIE_DOCUMENTALI.Where(w => w.Attivo &&
            (
                (w.MatricoleAbilitate.Contains(matricolaUtenteCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaUtenteCorrente))) ||
                (w.MatricoleAbilitate.Contains(matricolaUtenteCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaUtenteCorrente))) ||
                (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
            )).Select(x => new SelectListItem()
            {
                Value = x.Codice,
                Text = x.Descrizione,
                Selected = (codice == null ? false : (x.Codice == codice))
            }));


            //result.AddRange(db.XR_DEM_TIPOLOGIE_DOCUMENTALI.Where(w => w.Attivo &&
            //(
            //    (w.MatricoleAbilitate.Contains(matricolaUtenteCorrente) && w.MatricoleDisabilitate.Contains("*")) ||
            //    (w.MatricoleAbilitate.Contains(matricolaUtenteCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
            //    (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
            //    (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate)  && !w.MatricoleDisabilitate.Contains(matricolaUtenteCorrente))
            //)).Select(x => new SelectListItem()
            //{
            //    Value = x.Codice,
            //    Text = x.Descrizione,
            //    Selected = (codice == null ? false : (x.Codice == codice))
            //}));

            return result;
        }

        public static List<SelectListItem> GetTipologieDematerializzazioni(string codice = null, string tipologiaDoc = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var db = AnagraficaManager.GetDb();
            string matricolaUtenteCorrente = UtenteHelper.Matricola();

            result.Add(new SelectListItem()
            {
                Value = "-1",
                Text = "Seleziona un valore"
            });

            if (!String.IsNullOrEmpty(tipologiaDoc))
            {
                var comp = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDoc) &&
                    (
                        (w.MatricoleAbilitate.Contains(matricolaUtenteCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaUtenteCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaUtenteCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaUtenteCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                    )
                ).ToList();

                //var comp = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDoc) &&
                //    (
                //        (w.MatricoleAbilitate.Contains(matricolaUtenteCorrente) && w.MatricoleDisabilitate.Contains("*")) ||
                //        (w.MatricoleAbilitate.Contains(matricolaUtenteCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                //        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                //        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaUtenteCorrente))
                //    )
                //).ToList();

                if (comp != null && comp.Any())
                {
                    var listaCodiciTipoDocumento = comp.Select(w => w.Codice_Tipo_Documento).ToList();

                    result.AddRange(db.XR_DEM_TIPI_DOCUMENTO.Where(w => listaCodiciTipoDocumento.Contains(w.Codice)).Select(x => new SelectListItem()
                    {
                        Value = x.Codice,
                        Text = x.Descrizione,
                        Selected = (codice == null ? false : (x.Codice == codice))
                    }));
                }
            }
            return result;
        }

        [HttpGet]
        public JsonResult GetTipologieDematerializzazioniJSON(string codice = null, string tipologiaDoc = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var db = AnagraficaManager.GetDb();
            string matricolaUtenteCorrente = UtenteHelper.Matricola();

            result.Add(new SelectListItem()
            {
                Value = "-1",
                Text = "Seleziona un valore"
            });

            if (!String.IsNullOrEmpty(tipologiaDoc))
            {

                var comp = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDoc) &&
                    (
                        (w.MatricoleAbilitate.Contains(matricolaUtenteCorrente) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaUtenteCorrente))) ||
                        (w.MatricoleAbilitate.Contains(matricolaUtenteCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaUtenteCorrente))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                    )
                ).ToList();


                //var comp = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDoc) &&
                //    (
                //        (w.MatricoleAbilitate.Contains(matricolaUtenteCorrente) && w.MatricoleDisabilitate.Contains("*")) ||
                //        (w.MatricoleAbilitate.Contains(matricolaUtenteCorrente) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                //        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                //        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matricolaUtenteCorrente))
                //    )
                //).ToList();

                if (comp != null && comp.Any())
                {
                    var listaCodiciTipoDocumento = comp.Select(w => w.Codice_Tipo_Documento).ToList();

                    result.AddRange(db.XR_DEM_TIPI_DOCUMENTO.Where(w => listaCodiciTipoDocumento.Contains(w.Codice)).Select(x => new SelectListItem()
                    {
                        Value = x.Codice,
                        Text = x.Descrizione,
                        Selected = (codice == null ? false : (x.Codice == codice))
                    }));
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public static List<SelectListItem> GetIncaricatiFirma(string matricolaIncaricatoFirma = null)
        {
            IncentiviEntities db = new IncentiviEntities();
            List<SelectListItem> result = new List<SelectListItem>();
            string tipologia = String.Empty;
            string matricola = CommonHelper.GetCurrentUserMatricola();
            string tipologiaDocumentale = (string)SessionHelper.Get(matricola + "tipologiaDocumentale");
            string tipologiaDocumento = (string)SessionHelper.Get(matricola + "tipologiaDocumento");

            if (String.IsNullOrEmpty(tipologiaDocumentale) ||
                String.IsNullOrEmpty(tipologiaDocumento))
            {
                result.Add(new SelectListItem()
                {
                    Value = "-1",
                    Text = "Seleziona un incaricato alla firma"
                });
                return result;
            }

            // prende tutti gli incaricati alla firma
            List<NominativoMatricola> incaricatiFirma = AuthHelper.GetAllEnabledAs("DEMA", "01FIRM");

            // per ognuno di loro verifica che sia abilitato al tipo documento che si sta creando
            if (incaricatiFirma != null && incaricatiFirma.Any())
            {
                tipologia = String.Format("{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());

                foreach (var i in incaricatiFirma)
                {
                    var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                        //.Include("XR_HRIS_ABIL_FUNZIONE")
                        .Where(w => w.MATRICOLA == i.Matricola && w.IND_ATTIVO)
                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01FIRM")
                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                        .FirstOrDefault();

                    if (_myAbil != null)
                    {
                        #region POSSO FIRMARE
                        //Calcolo degli elementi che posso firmare
                        List<string> possoFirmare = new List<string>();

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                            && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                        {
                            possoFirmare.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                            && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                            && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                        {
                            possoFirmare.Add(_myAbil.TIP_DOC_INCLUSI);
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                            && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                        {
                            possoFirmare.Add(tipologia);
                        }
                        #endregion

                        #region NON POSSO FIRMARE
                        //Calcolo degli elementi che non posso firmare
                        List<string> nonPossoFirmare = new List<string>();

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                        {
                            nonPossoFirmare.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                            && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                        {
                            nonPossoFirmare.Add(_myAbil.TIP_DOC_ESCLUSI);
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                        {
                            // se però possoFirmare contiene tipologia allora non va aggiunta
                            if (!possoFirmare.Contains(tipologia))
                            {
                                nonPossoFirmare.Add(tipologia);
                            }
                        }
                        #endregion

                        #region CASO LIMITE TUTTI E DUE *
                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                            !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                                && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                        {
                            possoFirmare = new List<string>();
                            nonPossoFirmare = new List<string>();
                        }
                        #endregion

                        List<string> tipologieAbilitate = possoFirmare.Except(nonPossoFirmare).ToList();

                        if (tipologieAbilitate != null &&
                            tipologieAbilitate.Any())
                        {
                            if (tipologieAbilitate.Contains(tipologia))
                            {
                                string nominativo = CommonHelper.ToTitleCase(String.Format("{0} {1}", i.Cognome.Trim(), i.Nome.Trim()));

                                result.Add(new SelectListItem()
                                {
                                    Value = i.Matricola,
                                    Text = nominativo,
                                    Selected = (matricolaIncaricatoFirma == null ? false : matricolaIncaricatoFirma == i.Matricola)
                                });
                            }
                        }
                    }
                }
            }

            return result;
        }

        [HttpGet]
        public JsonResult GetIncaricatiFirmaJSON(string matricolaIncaricatoFirma = null)
        {
            IncentiviEntities db = new IncentiviEntities();
            List<SelectListItem> result = new List<SelectListItem>();
            string tipologia = String.Empty;
            string matricola = CommonHelper.GetCurrentUserMatricola();
            string tipologiaDocumentale = (string)SessionHelper.Get(matricola + "tipologiaDocumentale");
            string tipologiaDocumento = (string)SessionHelper.Get(matricola + "tipologiaDocumento");

            if (String.IsNullOrEmpty(tipologiaDocumentale) ||
                String.IsNullOrEmpty(tipologiaDocumento))
            {
                result.Add(new SelectListItem()
                {
                    Value = "-1",
                    Text = "Seleziona un incaricato alla firma"
                });
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            // prende tutti gli incaricati alla firma
            List<NominativoMatricola> incaricatiFirma = AuthHelper.GetAllEnabledAs("DEMA", "01FIRM");

            // per ognuno di loro verifica che sia abilitato al tipo documento che si sta creando
            if (incaricatiFirma != null && incaricatiFirma.Any())
            {
                tipologia = String.Format("{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());

                foreach (var i in incaricatiFirma)
                {
                    var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                        //.Include("XR_HRIS_ABIL_FUNZIONE")
                        .Where(w => w.MATRICOLA == i.Matricola && w.IND_ATTIVO)
                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01FIRM")
                        .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                        .FirstOrDefault();

                    if (_myAbil != null)
                    {
                        #region POSSO FIRMARE
                        //Calcolo degli elementi che posso firmare
                        List<string> possoFirmare = new List<string>();

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                            && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                        {
                            possoFirmare.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                            && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                            && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                        {
                            possoFirmare.Add(_myAbil.TIP_DOC_INCLUSI);
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                            && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                        {
                            possoFirmare.Add(tipologia);
                        }
                        #endregion

                        #region NON POSSO FIRMARE
                        //Calcolo degli elementi che non posso firmare
                        List<string> nonPossoFirmare = new List<string>();

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                        {
                            nonPossoFirmare.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                            && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                        {
                            nonPossoFirmare.Add(_myAbil.TIP_DOC_ESCLUSI);
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                        {
                            // se però possoFirmare contiene tipologia allora non va aggiunta
                            if (!possoFirmare.Contains(tipologia))
                            {
                                nonPossoFirmare.Add(tipologia);
                            }
                        }
                        #endregion

                        #region CASO LIMITE TUTTI E DUE *
                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                            !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                                && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                        {
                            possoFirmare = new List<string>();
                            nonPossoFirmare = new List<string>();
                        }
                        #endregion

                        List<string> tipologieAbilitate = possoFirmare.Except(nonPossoFirmare).ToList();

                        if (tipologieAbilitate != null &&
                            tipologieAbilitate.Any())
                        {
                            if (tipologieAbilitate.Contains(tipologia))
                            {
                                string nominativo = CommonHelper.ToTitleCase(String.Format("{0} {1}", i.Cognome.Trim(), i.Nome.Trim()));

                                result.Add(new SelectListItem()
                                {
                                    Value = i.Matricola,
                                    Text = nominativo,
                                    Selected = (matricolaIncaricatoFirma == null ? false : matricolaIncaricatoFirma == i.Matricola)
                                });
                            }
                        }
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public static List<SelectListItem> GetElencoApprovatori(string matricolaApprovatore = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = AnagraficaManager.GetDb();
            XR_WKF_TIPOLOGIA WKF_TIPOLOGIA = null;
            List<string> tipologieAbilitate = null;

            string matricola = CommonHelper.GetCurrentUserMatricola();

            string tipologiaDocumentale = (string)SessionHelper.Get(matricola + "tipologiaDocumentale");
            string tipologiaDocumento = (string)SessionHelper.Get(matricola + "tipologiaDocumento");

            if (String.IsNullOrEmpty(tipologiaDocumentale) ||
                String.IsNullOrEmpty(tipologiaDocumento))
            {
                result.Add(new SelectListItem()
                {
                    Value = "-1",
                    Text = "Seleziona un approvatore"
                });
                return result;
            }

            string tipologia = String.Format("DEMDOC_{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());

            WKF_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(tipologia)).FirstOrDefault();

            if (WKF_TIPOLOGIA == null)
            {
                // se è null allora la tipologia è formata dalla sola DEMDOC_TipologiaDocumentale senza 
                // il tipo documento
                tipologia = String.Format("DEMDOC_{0}", tipologiaDocumentale.Trim());

                WKF_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(tipologia)).FirstOrDefault();
            }

            if (WKF_TIPOLOGIA == null)
            {
                // se anche in questo caso è null allora c'è un errore
                throw new Exception("XR_WKF_TIPOLOGIA non trovata");
            }

            // recupera il [XR_DEM_TIPI_DOCUMENTO] così da ottenere le info aggiuntive
            // quali firmaobbligatoria ed approvatoreobbligatorio

            var dem_tipi_documento = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Codice.Equals(tipologiaDocumento)).FirstOrDefault();

            if (dem_tipi_documento == null)
            {
                throw new Exception("Tipo documento non trovato");
            }

            if (!dem_tipi_documento.ApprovazioneObbligatoria)
            {
                result.Add(new SelectListItem()
                {
                    Value = "-1",
                    Text = "Nessun approvatore"
                });
            }

            // reperimento del destinatario del workflow
            //var destinatario = db.XR_WKF_WORKFLOW.Where( w => w.ID_TIPOLOGIA.Equals( WKF_TIPOLOGIA.ID_TIPOLOGIA ) &&
            // w.ID_STATO == ( int ) StatiDematerializzazioneDocumenti.ProntoVisione ).FirstOrDefault( );

            // verifica se nel workflow c'è un elemento abbia 01APPR come destinatario
            var destinatario = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(WKF_TIPOLOGIA.ID_TIPOLOGIA) &&
            (w.DESTINATARIO.Contains("01APPR") || w.DESTINATARIO.Contains("01ADM"))).FirstOrDefault();

            if (
                destinatario == null ||
                (destinatario != null && String.IsNullOrEmpty(destinatario.DESTINATARIO))
                )
            {
                if (!dem_tipi_documento.ApprovazioneObbligatoria)
                {
                    result.Add(new SelectListItem()
                    {
                        Value = "0",
                        Text = "Nessun approvatore"
                    });
                }
            }
            else
            {
                List<NominativoMatricola> items = AuthHelper.GetAllEnabledAs("DEMA", "01APPR", true);

                if (items != null && items.Any())
                {
                    foreach (var i in items)
                    {
                        List<string> nonPosso = new List<string>();

                        var r = AuthHelper.EnableToMatr(i.Matricola, matricola, "DEMA", "01APPR");
                        if (r.Enabled && r.Visibilita == AbilMatrLiv.VisibilitaEnum.Filtrata)
                        {
                            // se non c'è matricola, allora è una tipologia come ad esempio 
                            // APPUNTO, che non ha matricola dipendente, ma ha comunque un vistatore
                            var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                                            .Where(w => w.MATRICOLA == i.Matricola && w.IND_ATTIVO)
                                            .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                                            .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01APPR")
                                            .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                                            .FirstOrDefault();

                            if (_myAbil != null)
                            {
                                #region POSSO SEGRETERIA 
                                //Calcolo degli elementi che posso lavorare
                                List<string> posso = new List<string>();

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                    && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                                {
                                    posso.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                                }

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                    && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                                    && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                                {
                                    posso.Add(_myAbil.TIP_DOC_INCLUSI);
                                }

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                                    && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                                {
                                    posso.Add("*");
                                }
                                #endregion

                                #region NON POSSO SEGRETERIA 
                                //Calcolo degli elementi che non posso lavorare

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                    && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                                {
                                    nonPosso.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                                }

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                    && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                                    && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                {
                                    nonPosso.Add(_myAbil.TIP_DOC_ESCLUSI);
                                }

                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                    && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                {
                                    nonPosso.Add("*");
                                }
                                #endregion

                                #region CASO LIMITE TUTTI E DUE *
                                if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                                    !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                        && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                                        && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                                {
                                    posso = new List<string>();
                                    nonPosso = new List<string>();
                                }
                                #endregion

                                tipologieAbilitate = posso.Except(nonPosso).ToList();

                                if (tipologieAbilitate.Contains("*"))
                                {
                                    // null non applicherà il filtro sulla tipologia perchè abilitato a tutte
                                    tipologieAbilitate = null;
                                }
                            }

                            string _temp = String.Format("{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());

                            if (tipologieAbilitate != null && tipologieAbilitate.Any())
                            {
                                if (tipologieAbilitate.Contains(_temp))
                                {
                                    result.Add(new SelectListItem()
                                    {
                                        Value = i.Matricola,
                                        Text = i.Cognome + " " + i.Nome
                                    });
                                }
                            }
                            else if (nonPosso != null && nonPosso.Any())
                            {
                                if (!nonPosso.Contains(_temp))
                                {
                                    result.Add(new SelectListItem()
                                    {
                                        Value = i.Matricola,
                                        Text = i.Cognome + " " + i.Nome
                                    });
                                }
                            }
                            else
                            {
                                result.Add(new SelectListItem()
                                {
                                    Value = i.Matricola,
                                    Text = i.Cognome + " " + i.Nome
                                });
                            }
                        }
                    }
                }
            }

            //if ( destinatario != null )
            //{
            //    List<NominativoMatricola> items = new List<NominativoMatricola>();

            //    if (destinatario.DESTINATARIO.Contains(","))
            //    {
            //        List<string> abil = new List<string>();
            //        abil = destinatario.DESTINATARIO.Split(',').ToList();

            //        foreach(var a in abil)
            //        {
            //            items.AddRange(AuthHelper.GetAllEnabledAs("DEMA", a, true));
            //        }
            //    }
            //    else
            //    {
            //        items = AuthHelper.GetAllEnabledAs("DEMA", destinatario.DESTINATARIO, true);
            //    }

            //    if (items != null && items.Any())
            //    {
            //        foreach (var i in items)
            //        {
            //            var r = AuthHelper.EnableToMatr(i.Matricola, matricola, "DEMA", "01APPR");
            //            if (r.Enabled && r.Visibilita == AbilMatrLiv.VisibilitaEnum.Filtrata)
            //            {
            //                result.Add(new SelectListItem()
            //                {
            //                    Value = i.Matricola,
            //                    Text = i.Cognome + " " + i.Nome,
            //                    Selected = (matricolaApprovatore == null ? false : matricolaApprovatore == r.Matricola)
            //                });
            //            }
            //        }
            //    }
            //}

            if (!String.IsNullOrEmpty(matricolaApprovatore))
            {
                if (result == null)
                {
                    result = new List<SelectListItem>();
                }
                bool esisteApprovatoreInLista = result.Count(w => w.Value == matricolaApprovatore) > 0;
                if (esisteApprovatoreInLista)
                {
                    var seleziona = result.Where(w => w.Value == matricolaApprovatore).FirstOrDefault();
                    seleziona.Selected = true;
                }
                else
                {
                    // se non è presente in lista, va aggiunto visto che comunque è stato
                    // assegnato al documento.
                    // Potrebbe non essere presente il lista perchè l'utente corrente non 
                    // ha la matricola associata a quell'approvatore e quindi non ha visibilità 
                    // su di esso.
                    result.Add(new SelectListItem()
                    {
                        Value = matricolaApprovatore,
                        Text = DematerializzazioneManager.GetNominativoByMatricola(matricolaApprovatore),
                        Selected = true
                    });
                }
            }

            if (result != null && result.Any())
            {
                foreach (var r in result)
                {
                    r.Text = CommonHelper.ToTitleCase(r.Text);
                }

                if (result.Count == 1)
                {
                    result[0].Selected = true;
                }
            }

            return result;
        }

        public static List<SelectListItem> GetNumeroPagineDocumento(int idAllegato, int nPagina = 1)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            if (idAllegato == 0)
            {
                result.Add(new SelectListItem()
                {
                    Value = "1",
                    Text = "1",
                    Selected = true
                });
            }
            else
            {
                var db = AnagraficaManager.GetDb();
                var item = db.XR_ALLEGATI.Where(w => w.Id.Equals(idAllegato)).FirstOrDefault();
                string mimeTypePDF = MimeTypeMap.GetMimeType("pdf");
                byte[] bArray = null;
                if (!String.IsNullOrEmpty(mimeTypePDF))
                {
                    mimeTypePDF = mimeTypePDF.ToUpper();
                }
                if (item != null)
                {
                    if (item.MimeType.ToUpper() == mimeTypePDF)
                    {
                        bArray = item.ContentByte;
                    }
                    else
                    {
                        // se non è un pdf, ma è stato convertito in pdf allora 
                        // il file sarà disponibile nella colonna ContentBytePDF
                        if (item.ContentBytePDF != null)
                        {
                            bArray = item.ContentBytePDF;
                        }
                    }

                    if (bArray != null)
                    {
                        PdfReader pdfReader = new PdfReader(bArray);
                        int numberOfPages = pdfReader.NumberOfPages;

                        for (int nPag = 1; nPag <= numberOfPages; nPag++)
                        {
                            result.Add(new SelectListItem()
                            {
                                Value = nPag.ToString(),
                                Text = nPag.ToString(),
                                Selected = (nPag == nPagina ? true : false)
                            });
                        }
                    }
                    else
                    {
                        throw new Exception("Errore nel reperimento del file");
                    }
                }
                else
                {
                    throw new Exception("Impossibile trovare il file desiderato");
                }
            }

            return result;
        }

        #endregion

        public ActionResult GetElencoRichieste()
        {
            DematerializzazioneDocumentiVM model = new DematerializzazioneDocumentiVM();
            model.IsPreview = false;
            model.ApprovazioneEnabled = false;
            model.PrendiInCaricoEnabled = true;
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>();
            model.Documenti = DematerializzazioneManager.GetDocumentiDaPrendereInCarico();
            return View("~/Views/Dematerializzazione/subpartial/Content.cshtml", model);
        }

        /// <summary>
        /// Restituisce la prima pagina del pdf principale
        /// in formato Jpeg
        /// </summary>
        /// <param name="idDoc">Identificativo XR_DEM_DOCUMENTI</param>
        /// <returns></returns>

        [HttpGet]
        public String GetFileInJpeg(int idAllegato, int pagina = 1)
        {
            if (pagina == -1000)
            {
                pagina = 1;
            }
            byte[] convertito = null;
            string base64 = "";
            var db = AnagraficaManager.GetDb();

            var item = db.XR_ALLEGATI.Where(w => w.Id.Equals(idAllegato)).FirstOrDefault();
            string mimeTypePDF = MimeTypeMap.GetMimeType("pdf");
            if (!String.IsNullOrEmpty(mimeTypePDF))
            {
                mimeTypePDF = mimeTypePDF.ToUpper();
            }
            if (item != null)
            {
                if (item.MimeType.ToUpper() == mimeTypePDF)
                {
                    // il pdf va convertito in png
                    convertito = ConvertPDFTOPNG(item.ContentByte, pagina);
                }
                else
                {
                    // se non è un pdf, ma è stato convertito in pdf allora 
                    // il file sarà disponibile nella colonna ContentBytePDF
                    if (item.ContentBytePDF != null)
                    {
                        convertito = ConvertPDFTOPNG(item.ContentBytePDF, pagina);
                    }
                }
            }
            else
            {
                throw new Exception("Impossibile trovare il file desiderato");
            }

            if (convertito != null && convertito.Length > 0)
            {
                base64 = Convert.ToBase64String(convertito);
            }

            string mimeType = "image/jpeg";
            return string.Format("data:{0};base64,{1}", mimeType, base64);
        }

        #region MODIFICA DOCUMENTO

        [HttpGet]
        public ActionResult GetDocumentoPerModifica(int idDoc)
        {
            try
            {
                List<AttributiAggiuntivi> objModuloBase = null;
                List<AttributiAggiuntivi> objModuloValorizzato = null;
                DatiAggiuntivi rispostaDIP = null;
                //List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();
                string tipologiaDocumentale = "";
                string tipologiaDocumento = "";
                string matricola = CommonHelper.GetCurrentUserMatricola();
                SessionHelper.Set(matricola + "tipologiaDocumentale", "");
                SessionHelper.Set(matricola + "tipologiaDocumento", "");

                Dem_ModificaDocumentoVM model = new Dem_ModificaDocumentoVM();

                model.Richiesta = GetDocumentData(idDoc);
                model.Matricola = model.Richiesta.Matricola;
                model.IdPersona = model.Richiesta.IdPersona;
                model.IdDocumento = model.Richiesta.Documento.Id;

                XR_WKF_TIPOLOGIA WKF_Tipologia = Get_WKF_Tipologia(model.Richiesta.Documento.Id_WKF_Tipologia);
                if (WKF_Tipologia == null)
                {
                    throw new Exception("Tipologia WorkFlow non trovato");
                }

                model.TipologiaWKF = WKF_Tipologia.COD_TIPOLOGIA;
                // modifica modello
                //model.TipologiaDocumentale = (DEM_TIPOLOGIE_DOCUMENTALI_ENUM)Enum.Parse(typeof(DEM_TIPOLOGIE_DOCUMENTALI_ENUM) , model.Richiesta.Documento.Cod_Tipologia_Documentale);

                // modifica modello
                model.TipologiaDocumentale = model.Richiesta.Documento.Cod_Tipologia_Documentale;

                // modifica modello
                XR_DEM_TIPI_DOCUMENTO _tempXR_DEM_TIPI_DOCUMENTO = Get_XR_DEM_TIPI_DOCUMENTO(model.Richiesta.Documento.Id_Tipo_Doc);

                if (_tempXR_DEM_TIPI_DOCUMENTO == null)
                {
                    throw new Exception("Tipo documento non trovato");
                }

                model.TipologiaDocumento = _tempXR_DEM_TIPI_DOCUMENTO.Codice;
                model.ListaTemplates = DematerializzazioneManager.GetTemplateModels();

                #region REGISTRA LA SCELTA FATTA
                SessionHelper.Set(matricola + "tipologiaDocumentale", model.TipologiaDocumentale.Trim());
                SessionHelper.Set(matricola + "tipologiaDocumento", model.TipologiaDocumento.Trim());
                SessionHelper.Set(matricola + "WKF_TIPOLOGIA", model.Richiesta.Documento.Id_WKF_Tipologia);
                #endregion

                tipologiaDocumentale = model.TipologiaDocumentale;
                tipologiaDocumento = model.TipologiaDocumento;

                #region CALCOLA IL COMPORTAMENTO
                var db = AnagraficaManager.GetDb();
                var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
                w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                if (comportamento == null)
                {
                    throw new Exception("Comportamento non trovato");
                }

                model.Comportamento = new XR_DEM_TIPIDOC_COMPORTAMENTO();
                model.Comportamento = comportamento;
                model.WKF_TIPOLOGIA = WKF_Tipologia.COD_TIPOLOGIA;
                model.AbilitaPannelloAllegati = comportamento.PannelloAllegati;

                if (!String.IsNullOrEmpty(model.Richiesta.Documento.CustomDataJSON) && model.Richiesta.Documento.CustomDataJSON != "[]")
                {
                    objModuloBase = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(comportamento.CustomDataJSON);
                    objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(model.Richiesta.Documento.CustomDataJSON);

                    using (var sediDB = new PERSEOEntities())
                    {
                        string queryDIP = "SELECT t0.[matricola_dp] as Matricola " +
                                            ",t12.[cod_sede] + ' ' + t12.[desc_sede] as SedeGapp " +
                                            ",substring(t1.[CODICINI], 3, 1) as AssicurazioneIinfortuni " +
                                            ",t2.[cod_mansione] + ' ' + t2.[desc_mansione] as Mansione " +
                                            ",t0.[sezione] as Sezione " +
                                            ",t0.[cod_serv_cont] + t0.[cod_serv_inquadram] + ' ' + t10.[desc_breve] as Servizio " +
                                            ",t11.[cod_categoria] + ' / ' + t0.[tipo_minimo] + ' ' + t3.desc_livello as Categoria " +
                                            ",t0.forma_contratto as FormaContratto " +
                                            ",t1.[DATA_ANZIANITA_CATEGORIA] as AnzianitaCategoria " +
                                            "FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                                            "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON(t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                                            "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_MANSIONE] " +
                                            "        t2 on(t2.[sky_mansione] = t1.[sky_mansione]) " +
                                            "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] " +
                                            "        t3 ON(t3.[sky_livello_categ] = t1.[sky_livello_categ]) " +
                                            "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SERVIZIO_CONTABILE] " +
                                            "        t10 on(t1.sky_servizio_contabile= t10.sky_serv_cont) " +
                                            "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA] " +
                                            "        t11 on(t1.sky_categoria = t11.sky_categoria) " +
                                            "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SEDE] " +
                                            "        t12 on(t1.sky_sede = t12.sky_sede) " +
                                            "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_INSEDIAMENTO] " +
                                            "        t13 on(t0.cod_insediamento_ubicazione = t13.cod_insediamento) " +
                                            "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CONTRATTO_UNICO] " +
                                            "        t14 ON(t1.[SKY_CONTRATTO] = t14.[SKY_CONTRATTO]) " +
                                            "where " +
                                            "t0.matricola_dp='##MATRICOLA##' and t1.[flg_ultimo_record]='$' ";

                        queryDIP = queryDIP.Replace("##MATRICOLA##", matricola);

                        rispostaDIP = sediDB.Database.SqlQuery<DatiAggiuntivi>(queryDIP).FirstOrDefault();
                    }
                }

                #endregion
                model.NominativoUtenteVistatore = new List<string>();
                if (!String.IsNullOrEmpty(model.Richiesta.Documento.MatricolaVisualizzatore))
                {
                    model.NominativoUtenteVistatore.Add(DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaVisualizzatore));
                }

                if (model.Richiesta.Documento.Id_Stato < (int)StatiDematerializzazioneDocumenti.Accettato &&
                    String.IsNullOrEmpty(model.Richiesta.Documento.MatricolaVisualizzatore))
                {
                    if (comportamento.Visionatore)
                    {
                        List<NominativoMatricola> items = null;

                        items = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

                        if (items != null && items.Any())
                        {
                            foreach (var i in items)
                            {
                                var r = AuthHelper.EnableToMatr(i.Matricola, model.Richiesta.Documento.MatricolaCreatore, "DEMA", "01VIST");
                                if (r.Enabled)
                                {
                                    model.NominativoUtenteVistatore.Add(CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                                }
                            }
                        }
                    }
                }

                // SE NELLA TABELLA XR_DEM_TIPIDOC_COMPORTAMENTO IL CAMPO ModelloJsonAlternativo 
                // E' VALORIZZATO ALLORA L'OGGETTO CONTENUTO IN model.ModelDatiAggiuntivi lista 
                // di Attributi VA MERGIATO CON L'OGGETTO JSON DI ModelloJsonAlternativo
                // QUESTO PERCHè ALCUNI CAMPI POTREBBERO ESSERE IN SOLA LETTURA IN FASE DI MODIFICA
                // OPPURE SONO PREVISTI CAMPI AGGIUNTIVI
                if (DematerializzazioneManager.IsSegreteria())
                {
                    model.IsSegreteria = true;
                    // IMPORTANTE QUESTO PEZZO VA RIVISTO PERCHè AL MOMENETO è 
                    // VINCOLATO ALLA SOLA MODIFICA DA PARTE DELLA SEGRETERIA 
                    // E NON VA BENE COSì
                    if (comportamento.ModelloJsonAlternativo != null &&
                        comportamento.ModelloJsonAlternativo != "[]")
                    {
                        model.SkipSalvataggioCompleto = true;
                        List<AttributiAggiuntivi> objAlternativo = null;
                        objAlternativo = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(comportamento.ModelloJsonAlternativo);

                        // prima di tutto per gli elementi già esistenti ne sovrascrive attributi
                        // come readonly, visible
                        foreach (var a in objModuloBase)
                        {
                            var alt = objAlternativo.Where(w => w.Id == a.Id).FirstOrDefault();
                            if (alt != null)
                            {
                                a.HideInReadOnly = alt.HideInReadOnly;
                                a.ReadOnly = alt.ReadOnly;
                                a.Required = alt.Required;
                            }
                        }

                        // una volta aggiornati gli elementi, controlla se ci sono 
                        // attributi da aggiungere alla lista finale
                        if (objModuloBase.Count != objAlternativo.Count)
                        {
                            objAlternativo = objAlternativo.OrderBy(w => w.Ordinamento).ToList();
                            // objAlternativo è disegnato partendo da [CustomDataJSON] 
                            // più eventuali altri campi, quindi se il count degli elementi 
                            // dovesse essere diverso significa che ci sono campi in più 
                            // da aggiungere, quelli non modificabili sono definiti come
                            // readonly, ma ci saranno sempre nell'elenco
                            List<AttributiAggiuntivi> _final = new List<AttributiAggiuntivi>();
                            foreach (var _alt in objAlternativo)
                            {
                                var _base = objModuloBase.Where(w => w.Id == _alt.Id).FirstOrDefault();

                                if (_base != null)
                                {
                                    _final.Add(_base);
                                }
                                else
                                {
                                    _final.Add(_alt);
                                }
                            }

                            _final = _final.OrderBy(w => w.Ordinamento).ToList();
                            objModuloBase = _final;
                        }
                    }
                }

                if (objModuloBase != null && objModuloBase.Any())
                {
                    objModuloBase = PopolaDatiModulo(objModuloBase, rispostaDIP, objModuloValorizzato, idDoc);
                }

                model.ModelDatiAggiuntivi = new DematerializzazioneCustomDataView()
                {
                    Attributi = objModuloBase
                };

                try
                {
                    model.ModelDatiAggiuntivi.CodiceProtocollatoreInModifica = objModuloBase.Where(w => w.Id == "CODICEPROTOCOLLATORE").FirstOrDefault().ValoreInModifica;
                }
                catch (Exception)
                {
                }

                foreach (var elementoDaPrecaricare in objModuloBase.Where(w => !String.IsNullOrEmpty(w.DBRefAttribute)).ToList())
                {
                    if (elementoDaPrecaricare.DBRefAttribute.Equals("UFFICIO_DESTINATARIO") && elementoDaPrecaricare.Tipo == TipologiaAttributoEnum.SelectMultiSelezione)
                    {
                        model.ModelDatiAggiuntivi.emailDirezione = new List<TBEmailDirezioni>();
                        model.ModelDatiAggiuntivi.emailDirezione = DematerializzazioneManager.GetEmailDirezione();
                    }

                    if (elementoDaPrecaricare.DBRefAttribute.Equals("CODICE_PROTOCOLLATORE"))
                    {
                        List<XR_HRIS_PROTOCOLLI> protocolli = new List<XR_HRIS_PROTOCOLLI>();
                        List<SelectListItem> result = new List<SelectListItem>();
                        model.CodiceProtocollatore = new List<string>();

                        protocolli = GetProtocolloPerMatricola(matricola);

                        if (protocolli != null)
                        {
                            if (model.ModelDatiAggiuntivi.CodiceProtocollatoreInModifica != null)
                            {
                                model.ModelDatiAggiuntivi.CodiceProtocollatoreInModificaTesto = protocolli.FirstOrDefault(x => x.CodiceProtocollo == model.ModelDatiAggiuntivi.CodiceProtocollatoreInModifica).CodificaEsterna;
                            }

                            foreach (var item in protocolli)
                            {
                                result.Add(new SelectListItem()
                                {
                                    Text = item.Struttura,
                                    Value = item.CodificaEsterna,
                                    Selected = model.ModelDatiAggiuntivi.CodiceProtocollatoreInModificaTesto == item.CodificaEsterna ? true : false
                                });
                                model.CodiceProtocollatore.Add(item.CodiceProtocollo);
                            }

                            if (result != null)
                            {
                                model.ModelDatiAggiuntivi.ElementiLstProtocolli = result;
                            }
                        }

                        //Lo elimino cosi non carica la versione vecchia del protocollo
                        objModuloBase.Remove(elementoDaPrecaricare);
                    }
                }

                return View("~/Views/Dematerializzazione/Modal_ModificaDocumento.cshtml", model);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private List<AttributiAggiuntivi> PopolaDatiModulo(List<AttributiAggiuntivi> objModuloBase, DatiAggiuntivi rispostaDIP, List<AttributiAggiuntivi> objModuloValorizzato, int idDoc = 0)
        {
            List<AttributiAggiuntivi> result = new List<AttributiAggiuntivi>();

            foreach (var attr in objModuloBase)
            {
                if (!String.IsNullOrEmpty(attr.TagHRDW))
                {
                    var val = rispostaDIP.GetType().GetProperty(attr.TagHRDW).GetValue(rispostaDIP, null);

                    if (val != null)
                    {
                        attr.Valore = val.ToString();
                    }
                }

                // CARICA LA COMBO MULTISELECT
                // SE PreLoadSelectListItems è VALORIZZATO ALLORA CONTERRà L'INDIRIZZO
                // DEL METODO DA CHIAMARE PER OTTENERE LA LISTA SELECTLISTITEMS DA CARICARE 
                // NELL'ATTRIBUTO SELECTLISTITEMS DELL'OGGETTO IN ESAME
                if (!String.IsNullOrEmpty(attr.PreLoadSelectListItems))
                {
                    string matricolaDestinatario = UtenteHelper.Matricola();
                    Type thisType = this.GetType();
                    MethodInfo theMethod = thisType.GetMethod(attr.PreLoadSelectListItems);
                    int idpersona = 0;
                    List<object> parametri = new List<object>();

                    if (!String.IsNullOrEmpty(matricolaDestinatario))
                    {
                        idpersona = CezanneHelper.GetIdPersona(matricolaDestinatario);
                    }

                    parametri.Add(idpersona);

                    List<SelectListItem> lista = (List<SelectListItem>)theMethod.Invoke(this, parametri.ToArray());
                    if (lista != null && lista.Any())
                    {
                        attr.SelectListItems = lista;
                    }
                }

                // SE PreLoadData è VALORIZZATO ALLORA CONTERRà L'INDIRIZZO
                // DEL METODO DA CHIAMARE PER OTTENERE IL TESTO DA CARICARE 
                // NELL'ATTRIBUTO VALUE DELL'OGGETTO IN ESAME
                if (!String.IsNullOrEmpty(attr.PreLoadData))
                {
                    string matricolaDestinatario = String.Empty;

                    if (idDoc > 0)
                    {
                        using (IncentiviEntities db = new IncentiviEntities())
                        {
                            var _myDoc = db.XR_DEM_DOCUMENTI.Where(w => w.Id == idDoc).FirstOrDefault();
                            if (_myDoc == null)
                            {
                                throw new Exception("Documento non trovato");
                            }
                            matricolaDestinatario = _myDoc.MatricolaDestinatario;
                        }
                    }

                    Type thisType = this.GetType();
                    MethodInfo theMethod = thisType.GetMethod(attr.PreLoadData);
                    int idpersona = 0;
                    List<object> parametri = new List<object>();

                    if (!String.IsNullOrEmpty(matricolaDestinatario))
                    {
                        idpersona = CezanneHelper.GetIdPersona(matricolaDestinatario);
                    }

                    parametri.Add(idpersona);

                    string _myValue = (string)theMethod.Invoke(this, parametri.ToArray());
                    if (!String.IsNullOrEmpty(_myValue))
                    {
                        attr.Valore = _myValue;
                    }
                }

                // se è una SelectListItems allora bisogna cercare il campo 
                // 		"gruppo": scelta-[id]
                if (attr.SelectListItems != null && attr.SelectListItems.Any())
                {
                    string gruppo = "scelta-" + attr.Id;
                    if (objModuloValorizzato != null && objModuloValorizzato.Any())
                    {
                        var find = objModuloValorizzato.Where(w => w.Gruppo != null && w.Gruppo.Equals(gruppo)).ToList();

                        if (find == null || !find.Any())
                        {
                            // se non lo trova tramite il tag GRUPPO, allora cerca per DBRefAttribute
                            find = objModuloValorizzato.Where(w => w.DBRefAttribute == attr.DBRefAttribute).ToList();
                        }

                        // in questa lista va cercato l'elemento col campo checked a true
                        if (find != null && find.Any())
                        {
                            var elementoChecked = find.Where(w => w.Checked).FirstOrDefault();
                            if (elementoChecked != null)
                            {
                                var it = attr.SelectListItems.Where(w => w.Value.Equals(elementoChecked.Valore)).FirstOrDefault();
                                if (it != null)
                                {
                                    it.Selected = true;
                                }
                                if (!String.IsNullOrEmpty(elementoChecked.Label))
                                {
                                    attr.Label = elementoChecked.Label;
                                }
                            }
                        }
                    }
                }

                var itemValorizzato = objModuloValorizzato.Where(w => w.Id.Equals(attr.Id)).FirstOrDefault();

                if (itemValorizzato != null)
                {
                    if (attr.Tipo == TipologiaAttributoEnum.Switch)
                    {
                        attr.SelectListItems.ForEach(w =>
                        {
                            w.Selected = false;
                        });
                        var toSet = attr.SelectListItems.Where(w => w.Text == itemValorizzato.Valore).FirstOrDefault();
                        toSet.Selected = true;
                        attr.ValoreInModifica = itemValorizzato.Valore;
                        attr.Checked = itemValorizzato.Checked;
                    }
                    else if (attr.Tipo == TipologiaAttributoEnum.SelectMultiSelezioneLibera && !String.IsNullOrEmpty(itemValorizzato.Valore))
                    {
                        // se l'elemento è di tipo selectmultilibera, il valore è contenuto nel campo
                        // valore come stringa separata da virgola e non nel campo selectlistitems
                        // questa stringa però deve diventare una selectlistItems

                        attr.SelectListItems = new List<SelectListItem>();
                        foreach (var s in itemValorizzato.Valore.Split(',').ToList())
                        {
                            SelectListItem si = new SelectListItem();
                            si.Selected = true;
                            si.Text = s;
                            si.Value = s;
                            attr.SelectListItems.Add(si);
                        }
                    }
                    else if (!String.IsNullOrEmpty(itemValorizzato.Valore))
                    {
                        attr.ValoreInModifica = itemValorizzato.Valore;
                        attr.Checked = itemValorizzato.Checked;
                        attr.HideInReadOnly = itemValorizzato.HideInReadOnly;
                    }

                    // se ha figli vanno controllati anche i figli
                    if (attr.InLine != null && attr.InLine.Any())
                    {
                        attr.InLine = PopolaDatiModulo(attr.InLine, rispostaDIP, objModuloValorizzato, idDoc);
                    }
                }
                else if (objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                {
                    foreach (var item in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
                    {
                        itemValorizzato = FindValorizzatoInObject(attr.Id, item);
                        if (itemValorizzato != null)
                        {
                            if (!String.IsNullOrEmpty(itemValorizzato.Valore))
                            {
                                attr.ValoreInModifica = itemValorizzato.Valore;
                                attr.Checked = itemValorizzato.Checked;
                                attr.HideInReadOnly = itemValorizzato.HideInReadOnly;

                                if (attr.SelectListItems != null && attr.SelectListItems.Any())
                                {
                                    var toSelect = attr.SelectListItems.Where(w => w.Value.Equals(attr.ValoreInModifica)).FirstOrDefault();
                                    if (toSelect != null)
                                    {
                                        toSelect.Selected = true;
                                    }

                                }
                            }

                            // se ha figli vanno controllati anche i figli
                            if (attr.InLine != null && attr.InLine.Any())
                            {
                                attr.InLine = PopolaDatiModulo(attr.InLine, rispostaDIP, objModuloValorizzato, idDoc);
                            }
                        }
                    }
                }

                result.Add(attr);

                /*
                 * Verifica se ci sono altri oggetti valorizzati che hanno come id, un id composto
                 * da [id]_GUID...
                 */
                string startGuid = attr.Id + "_GUID";
                var itemsValorizzati = objModuloValorizzato.Where(w => w.Id.Contains(startGuid)).ToList();

                if (itemsValorizzati != null && itemsValorizzati.Any())
                {
                    foreach (var toClone in itemsValorizzati)
                    {
                        var toInsert = ClonaElementoCustom(attr, toClone);
                        result.Add(toInsert);
                    }
                }
            }

            return result;
        }

        private static AttributiAggiuntivi FindValorizzatoInObject(string id, AttributiAggiuntivi objModuloValorizzato)
        {
            AttributiAggiuntivi result = null;
            var itemValorizzato = (objModuloValorizzato.Id.Equals(id) ? objModuloValorizzato : null);
            if (itemValorizzato != null)
            {
                result = new AttributiAggiuntivi();
                result = itemValorizzato;
            }
            else if (objModuloValorizzato.InLine != null && objModuloValorizzato.InLine.Any())
            {
                foreach (var item in objModuloValorizzato.InLine.ToList())
                {
                    itemValorizzato = FindValorizzatoInObject(id, item);
                    if (itemValorizzato != null)
                    {
                        result = new AttributiAggiuntivi();
                        result = itemValorizzato;
                    }
                }
            }
            else if (objModuloValorizzato.Buttons != null && objModuloValorizzato.Buttons.Any())
            {
                foreach (var item in objModuloValorizzato.Buttons.ToList())
                {
                    itemValorizzato = FindValorizzatoInObject(id, item);
                    if (itemValorizzato != null)
                    {
                        result = new AttributiAggiuntivi();
                        result = itemValorizzato;
                    }
                }
            }
            return result;
        }

        private AttributiAggiuntivi ClonaElementoCustom(AttributiAggiuntivi source, AttributiAggiuntivi data)
        {
            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(source);
                AttributiAggiuntivi elementoDaClonare = Newtonsoft.Json.JsonConvert.DeserializeObject<AttributiAggiuntivi>(json);

                elementoDaClonare.Id = data.Id;
                elementoDaClonare.Valore = data.Valore;
                elementoDaClonare.ValoreInModifica = data.Valore;
                elementoDaClonare.Checked = data.Checked;

                //dt_inline_GUIDdf361dbb7cb64a58a5ed792c884b2d68
                int pos = data.Id.IndexOf("_GUID");
                //dt_inline
                string idPadre = data.Id.Substring(0, pos);
                string guid = data.Id.Substring(pos);
                //df361dbb7cb64a58a5ed792c884b2d68
                guid = guid.Replace("_GUID", "");

                if (elementoDaClonare.InLine != null && elementoDaClonare.InLine.Any())
                {
                    RiscriviID(elementoDaClonare, guid, idPadre);
                    List<AttributiAggiuntivi> elementiInLine = new List<AttributiAggiuntivi>();

                    foreach (var l in elementoDaClonare.InLine)
                    {
                        AttributiAggiuntivi valorizzato = FindValorizzatoInObject(l.Id, data);
                        if (valorizzato != null)
                        {
                            elementiInLine.Add(ClonaElementoCustom(l, valorizzato));
                        }
                        else
                        {
                            elementiInLine.Add(l);
                        }
                    }

                    elementoDaClonare.InLine.Clear();
                    elementoDaClonare.InLine.AddRange(elementiInLine.ToList());
                }

                if (elementoDaClonare.Buttons != null && elementoDaClonare.Buttons.Any())
                {
                    RiscriviID(elementoDaClonare, guid, idPadre);
                    List<AttributiAggiuntivi> elementiInLine = new List<AttributiAggiuntivi>();

                    foreach (var l in elementoDaClonare.Buttons)
                    {
                        AttributiAggiuntivi valorizzato = FindValorizzatoInObject(l.Id, data);
                        if (valorizzato != null)
                        {
                            elementiInLine.Add(ClonaElementoCustom(l, valorizzato));
                        }
                        else
                        {
                            elementiInLine.Add(l);
                        }
                    }

                    elementoDaClonare.Buttons.Clear();
                    elementoDaClonare.Buttons.AddRange(elementiInLine.ToList());
                }

                if (elementoDaClonare.SelectListItems != null && elementoDaClonare.SelectListItems.Any())
                {
                    var toSelect = elementoDaClonare.SelectListItems.Where(w => w.Value.Equals(elementoDaClonare.ValoreInModifica)).FirstOrDefault();
                    if (toSelect != null)
                    {
                        toSelect.Selected = true;
                    }
                }

                return elementoDaClonare;
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        //private static List<AttributiAggiuntivi> GetListaPiatta(AttributiAggiuntivi initialFolder)
        //{
        //    var folders = new List<AttributiAggiuntivi>();

        //    if (initialFolder.InLine == null)
        //    {
        //        folders.Add(initialFolder);
        //    }

        //    if (initialFolder.InLine != null && initialFolder.InLine.Any())
        //    {
        //        foreach (var f in initialFolder.InLine.ToList())
        //        {
        //            folders.AddRange(GetListaPiatta(f));
        //        }
        //    }

        //    return folders;
        //}

        public ActionResult SetPresoInModifica(int idDoc)
        {
            string error = "";
            string msg = "";
            var db = AnagraficaManager.GetDb();
            try
            {
                string matricola = UtenteHelper.Matricola();
                XR_DEM_DOCUMENTI item = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (item == null)
                {
                    throw new Exception("Impossibile reperire il documento");
                }

                DateTime ora = DateTime.Now;

                if (item.DataPresaInModifica.HasValue)
                {
                    // se la data di ultima presa in modifica è inferiore a
                    // 15 minuti, allora il sistema avviserà che probabilmente 
                    // c'è un altro operatore che ha aperto in modifica il documento
                    if ((ora - item.DataPresaInModifica.Value).TotalMinutes < 15 &&
                        item.MatricolaPresaInModifica != matricola)
                    {
                        string nominativo = DematerializzazioneManager.GetNominativoByMatricola(item.MatricolaPresaInModifica);
                        msg = "Il documento è stato aperto in modifica da " + nominativo.Trim() + " alle ore " + item.DataPresaInModifica.GetValueOrDefault().ToString("HH:mm") + ". Si desidera procedere con l'apertura in modifica del documento?";
                    }
                    else if ((ora - item.DataPresaInModifica.Value).TotalMinutes >= 15)
                    {
                        // se son passati più di 15 minuti dall'ultima apertura in modifica, 
                        // probabilmente il browser è stato chiuso senza salvare le modifiche
                        item.MatricolaPresaInModifica = matricola;
                        item.DataPresaInModifica = DateTime.Now;
                        db.SaveChanges();
                    }
                }
                else
                {
                    item.MatricolaPresaInModifica = matricola;
                    item.DataPresaInModifica = ora;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                msg = "";
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    message = msg,
                    error = error
                }
            };
        }

        public ActionResult AnnullaPresoInModifica()
        {
            dynamic showMessageString = string.Empty;
            try
            {
                showMessageString = new
                {
                    param1 = 200,
                    param2 = "OK"
                };

                string matricola = UtenteHelper.Matricola();

                var db = AnagraficaManager.GetDb();
                var items = db.XR_DEM_DOCUMENTI.Where(w => w.MatricolaPresaInModifica == matricola).ToList();

                if (items != null && items.Any())
                {
                    foreach (var i in items)
                    {
                        i.MatricolaPresaInModifica = null;
                        i.DataPresaInModifica = null;
                    }
                    db.SaveChanges();
                }

                return Json(showMessageString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404, "Errore " + ex.Message);
            }
        }

        public ActionResult DownloadAllegato(int idAllegato)
        {
            XR_ALLEGATI allegato = new XR_ALLEGATI();
            var db = AnagraficaManager.GetDb();

            try
            {
                allegato = db.XR_ALLEGATI.Where(w => w.Id.Equals(idAllegato)).FirstOrDefault();
                if (allegato == null)
                {
                    throw new Exception("Errore impossibile reperire il file");
                }
                byte[] byteArray = allegato.ContentByte;
                string nomefile = allegato.NomeFile;

                MemoryStream pdfStream = new MemoryStream();
                pdfStream.Write(byteArray, 0, byteArray.Length);
                pdfStream.Position = 0;

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = "documento",
                    Inline = true,
                };

                Response.AddHeader("Content-Disposition", "inline; filename=" + nomefile);
                return File(byteArray, allegato.MimeType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Obsolete("Sostituito dalla chiamata UpdatePratica", true)]
        [HttpPost]
        public ActionResult UpdateDocumentoVSDip(UpdateDocumento toupload)
        {
            var db = AnagraficaManager.GetDb();
            RichiestaDoc richiesta = new RichiestaDoc();
            List<int> idAllegati = new List<int>();
            List<int> idAllegatiEliminati = new List<int>();
            if (!String.IsNullOrEmpty(toupload.Allegati))
            {
                List<string> temp = toupload.Allegati.Split(',').ToList();
                foreach (var t in temp)
                {
                    idAllegati.Add(int.Parse(t));
                }
            }

            if (toupload.IdAllegatoPrincipale != toupload.IdAllegatoPrincipaleOLD)
            {
                idAllegati.Add(toupload.IdAllegatoPrincipale);
            }

            if (!String.IsNullOrEmpty(toupload.AllegatiEliminati))
            {
                List<string> temp = toupload.AllegatiEliminati.Split(',').ToList();
                foreach (var t in temp)
                {
                    idAllegatiEliminati.Add(int.Parse(t));
                }
            }

            try
            {
                if (toupload != null)
                {
                    if (!String.IsNullOrEmpty(toupload.MatricolaApprovatore) &&
                        toupload.MatricolaApprovatore == "-1")
                    {
                        toupload.MatricolaApprovatore = null;
                    }

                    if (!String.IsNullOrEmpty(toupload.MatricolaFirma) &&
                        toupload.MatricolaFirma == "-1")
                    {
                        toupload.MatricolaFirma = null;
                    }

                    if (toupload.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza)
                    {
                        toupload.Id_Stato = (int)StatiDematerializzazioneDocumenti.ProntoVisione;
                    }

                    XR_WKF_TIPOLOGIA WKF_Tipologia = null;
                    // calcolo della tipologia
                    if (String.IsNullOrEmpty(toupload.CustomAttrs) || toupload.CustomAttrs == "[]")
                    {
                        WKF_Tipologia = Get_WKF_Tipologia(toupload.Id_WKF_Tipologia);
                    }
                    else
                    {
                        // se i customAttrs sono valorizzati va calcolata la tipologia in base ad eventuali sotto
                        // categorie determinate dall'eccezione selezionata e salvata tra i custom attrs esempio
                        // DEMDOC_VSRUO_CPM, può anche avere una sotto categoria DEMDOC_VSRUO_CPM_ON
                        WKF_Tipologia = Get_WKF_Tipologia(toupload.Id_WKF_Tipologia, toupload.CustomAttrs);


                        // verifica un eventuale cambio di tipologia, ad esempio se un PN che come tipologia ha
                        // DEMDOC_VSRUO_PPP_PN, viene modificato in un GM deve cambiare tipologia WKF in DEMDOC_VSRUO_PPP
                        var _tempWKF = Get_WKF_Tipologia(toupload.Cod_Tipologia_Documentale, toupload.Id_Tipo_Doc, toupload.CustomAttrs);

                        if (_tempWKF != null)
                        {
                            if (_tempWKF.ID_TIPOLOGIA != WKF_Tipologia.ID_TIPOLOGIA)
                            {
                                WKF_Tipologia = _tempWKF;
                            }
                        }
                        else
                        {
                            throw new Exception("Workflow non trovato");
                        }
                    }

                    if (WKF_Tipologia == null)
                    {
                        throw new Exception("Workflow non trovato");
                    }

                    richiesta.Documento = new XR_DEM_DOCUMENTI()
                    {
                        Descrizione = toupload.Descrizione,
                        Note = toupload.Note,
                        Id_Stato = toupload.Id_Stato,
                        Id_Tipo_Doc = toupload.Id_Tipo_Doc,
                        MatricolaCreatore = toupload.MatricolaCreatore,
                        IdPersonaCreatore = toupload.IdPersonaCreatore,
                        MatricolaDestinatario = toupload.MatricolaDestinatario,
                        Id_WKF_Tipologia = WKF_Tipologia.ID_TIPOLOGIA,
                        Cod_Tipologia_Documentale = toupload.Cod_Tipologia_Documentale,
                        MatricolaApprovatore = toupload.MatricolaApprovatore,
                        MatricolaFirma = toupload.MatricolaFirma,
                        Id = toupload.Id,
                        CustomDataJSON = toupload.CustomAttrs
                    };

                    int result = InsertDocumentoVSDipendenteInternal(richiesta, idAllegati);

                    if (result == 0)
                    {
                        throw new Exception("Errore nel salvataggio dei dati");
                    }

                    if (toupload.IdAllegatoPrincipale != toupload.IdAllegatoPrincipaleOLD)
                    {
                        // se è diverso è cambiato il file principale ed il vecchio va rimosso
                        bool eliminato = EliminaAllegatoById(toupload.IdAllegatoPrincipaleOLD);
                        if (!eliminato)
                        {
                            throw new Exception("Impossibile eliminare l'allegato");
                        }
                    }

                    if (idAllegatiEliminati != null && idAllegatiEliminati.Any())
                    {
                        foreach (var a in idAllegatiEliminati)
                        {
                            bool eliminato = EliminaAllegatoById(a);
                            if (!eliminato)
                            {
                                throw new Exception("Impossibile eliminare l'allegato");
                            }
                        }
                    }

                    return Json(new { success = true, responseText = result.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = "Errore nell'inserimento della richiesta" }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        private bool EliminaAllegatoById(int idAllegato)
        {
            bool result = false;
            var db = AnagraficaManager.GetDb();
            try
            {
                var fileAllegato = db.XR_ALLEGATI.Where(w => w.Id == idAllegato).FirstOrDefault();
                if (fileAllegato == null)
                {
                    throw new Exception("Allegato non trovato");
                }
                db.XR_ALLEGATI.Remove(fileAllegato);

                // verifica se l'allegato è associato ad una versione
                // se si, deve eliminare tutti i record ad esso collegato
                var allegatiVersioni = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => w.IdAllegato.Equals(idAllegato)).ToList();

                if (allegatiVersioni != null && allegatiVersioni.Any())
                {
                    List<int> idVersioni = new List<int>();
                    idVersioni.AddRange(allegatiVersioni.Select(w => w.IdVersione).ToList());
                    db.XR_DEM_ALLEGATI_VERSIONI.RemoveWhere(w => w.IdAllegato.Equals(idAllegato));
                    db.XR_DEM_VERSIONI_DOCUMENTO.RemoveWhere(w => idVersioni.Contains(w.Id));
                }

                db.SaveChanges();

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
        #endregion

        #region OPERATORI
        [ObsoleteAttribute("This method is obsolete. Call NewMethod instead.", true)]
        public ActionResult Operatore()
        {
            AnagraficaModel anagraficaModel = null;
            DematerializzazioneDocumentiVM model = new DematerializzazioneDocumentiVM();

            SezioniAnag sezione = SezioniAnag.NonDefinito;
            anagraficaModel = AnagraficaManager.GetAnagrafica(UtenteHelper.Matricola(), new AnagraficaLoader(sezione));

            if (anagraficaModel != null)
            {
                model.IdPersona = anagraficaModel.IdPersona;
                model.Matricola = anagraficaModel.Matricola;

                if (anagraficaModel.Matricola == null)
                {
                    model.Matricola = CommonHelper.GetCurrentUserMatricola();
                }
            }

            model.IsPreview = true;
            return View("~/Views/Dematerializzazione/Operatore.cshtml", model);
        }

        #endregion

        [HttpPost]
        public ActionResult Modal_InserimentoDocDem(string m, int id = 0, bool ricercaLibera = false)
        {
            InsRicModel richiesta = new InsRicModel();
            if (!string.IsNullOrEmpty(m))
            {
                richiesta.IdPersona = id;
                richiesta.Matricola = m;
                richiesta.MatricolaDestinatario = m;
                richiesta.IdPersonaDestinatario = id;
            }

            richiesta.ListaTemplates = DematerializzazioneManager.GetTemplateModels();

            if (String.IsNullOrEmpty(richiesta.Matricola))
            {
                richiesta.Matricola = UtenteHelper.Matricola();
            }

            if (richiesta.IdPersona == 0)
            {
                int? idPersona = null;

                idPersona = CezanneHelper.GetIdPersona(richiesta.Matricola);
                richiesta.IdPersona = idPersona.GetValueOrDefault();
            }

            richiesta.MatricolaDestinatario = richiesta.Matricola;
            richiesta.IdPersonaDestinatario = richiesta.IdPersona;

            if (ricercaLibera)
            {
                richiesta.MatricolaDestinatario = "";
                richiesta.IdPersonaDestinatario = 0;
            }
            string matricola = CommonHelper.GetCurrentUserMatricola();
            SessionHelper.Set(matricola + "tipologiaDocumentale", "");
            SessionHelper.Set(matricola + "tipologiaDocumento", "");
            richiesta.RicercaLibera = ricercaLibera;
            return View("subpartial/Modal_InserimentoDocDem", richiesta);
        }

        public ActionResult GetDestinatario(string filter, string value)
        {
            return Json(DematerializzazioneManager.GetDestinatario(filter, value), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SalvaComeBozza(InsRicModel model)
        {
            RichiestaDoc richiesta = new RichiestaDoc();
            List<int> idAllegati = new List<int>();
            if (!String.IsNullOrEmpty(model.Allegati))
            {
                List<string> temp = model.Allegati.Split(',').ToList();
                foreach (var t in temp)
                {
                    idAllegati.Add(int.Parse(t));
                }
            }

            if (model.IdAllegatoPrincipale > 0)
            {
                bool presenteStessoAllegatoPrincipale = idAllegati.Contains(model.IdAllegatoPrincipale);
                if (!presenteStessoAllegatoPrincipale)
                {
                    //elimina il file perchè è stato sostituito
                }
            }

            //---- Recupero l'idArea altrimenti la bozza non sarà più visibile all'operatore
            int idArea = 0;
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    if (!String.IsNullOrWhiteSpace(model.Matricola))
                    {
                        string direzione = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == model.Matricola)
                                            .Select(x => x.COD_SERVIZIO).FirstOrDefault();
                        if (!String.IsNullOrWhiteSpace(direzione))
                        {
                            idArea = db.XR_HRIS_DIR_FILTER
                                .Where(x => x.DIR_INCLUDED != null && x.DIR_INCLUDED.Contains(direzione.Trim()))
                                .Select(x => x.ID_AREA_FILTER).FirstOrDefault(); //male che va rimane null
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Impossibile recuperare l'IdArea");
            }

            try
            {
                if (model != null)
                {
                    int stato = (int)StatiDematerializzazioneDocumenti.Bozza;

                    if (String.IsNullOrEmpty(model.Matricola))
                    {
                        model.Matricola = UtenteHelper.Matricola();
                        int? idPersona = null;

                        idPersona = CezanneHelper.GetIdPersona(model.Matricola);
                        model.IdPersona = idPersona.GetValueOrDefault();
                    }

                    XR_DEM_TIPI_DOCUMENTO _tempXR_DEM_TIPI_DOCUMENTO = Get_XR_DEM_TIPI_DOCUMENTO(model.TipologiaDocumento);

                    if (_tempXR_DEM_TIPI_DOCUMENTO == null)
                    {
                        throw new Exception("Tipo documento non trovato");
                    }

                    model.Descrizione = _tempXR_DEM_TIPI_DOCUMENTO.Descrizione;

                    if (!String.IsNullOrEmpty(model.MatricolaApprovatore) &&
                        (model.MatricolaApprovatore == "-1" || model.MatricolaApprovatore == "undefined"))
                    {
                        model.MatricolaApprovatore = null;
                    }

                    if (!String.IsNullOrEmpty(model.IncaricatoFirma) &&
                        (model.IncaricatoFirma == "-1" || model.IncaricatoFirma == "undefined"))
                    {
                        model.IncaricatoFirma = null;
                    }

                    XR_WKF_TIPOLOGIA WKF_Tipologia = null;
                    // calcolo della tipologia
                    if (String.IsNullOrEmpty(model.CustomAttrs) || model.CustomAttrs == "[]")
                    {
                        WKF_Tipologia = Get_WKF_Tipologia(model.TipologiaWKF);
                    }
                    else
                    {
                        // se i customAttrs sono valorizzati va calcolata la tipologia in base ad eventuali sotto
                        // categorie determinate dall'eccezione selezionata e salvata tra i custom attrs esempio
                        // DEMDOC_VSRUO_CPM, può anche avere una sotto categoria DEMDOC_VSRUO_CPM_ON
                        WKF_Tipologia = Get_WKF_Tipologia(model.TipologiaWKF, model.CustomAttrs);
                    }

                    if (WKF_Tipologia == null)
                    {
                        throw new Exception("Workflow non trovato");
                    }

                    if (!String.IsNullOrEmpty(model.MatricolaDestinatario) && model.IdPersonaDestinatario == 0)
                    {
                        int idPersonaDestinatario = CezanneHelper.GetIdPersona(model.MatricolaDestinatario);
                        model.IdPersonaDestinatario = idPersonaDestinatario;
                    }

                    richiesta.Documento = new XR_DEM_DOCUMENTI()
                    {
                        Descrizione = model.Descrizione,
                        Note = model.Note,
                        Id_Stato = stato,
                        Id_Tipo_Doc = _tempXR_DEM_TIPI_DOCUMENTO.Id,
                        MatricolaCreatore = model.Matricola,
                        IdPersonaCreatore = model.IdPersona,
                        MatricolaDestinatario = model.MatricolaDestinatario,
                        Id_WKF_Tipologia = WKF_Tipologia.ID_TIPOLOGIA,
                        Cod_Tipologia_Documentale = model.TipologiaDocumentale,
                        MatricolaApprovatore = model.MatricolaApprovatore,
                        MatricolaFirma = model.IncaricatoFirma,
                        MatricolaIncaricato = model.MatricolaIncaricato,
                        Id = model.IdDocumento,
                        IdPersonaDestinatario = model.IdPersonaDestinatario,
                        CustomDataJSON = model.CustomAttrs,
                        IdArea = idArea
                    };

                    int result = InsertDocumentoVSDipendenteInternal(richiesta, idAllegati, true);

                    if (result == 0)
                    {
                        throw new Exception("Errore nel salvataggio dei dati");
                    }

                    return Json(new { success = true, responseText = result.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = "Errore nell'inserimento della richiesta" }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        [HttpGet]
        public JsonResult GetNumeroPagineDocumentoJSON(int idAllegato, int nPagina = 1)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            result.Add(new SelectListItem()
            {
                Value = "-1000",
                Text = "Nessuna",
                Selected = false
            });

            if (idAllegato == 0)
            {
                result.Add(new SelectListItem()
                {
                    Value = "1",
                    Text = "1",
                    Selected = true
                });
            }
            else
            {
                var db = AnagraficaManager.GetDb();
                var item = db.XR_ALLEGATI.Where(w => w.Id.Equals(idAllegato)).FirstOrDefault();
                string mimeTypePDF = MimeTypeMap.GetMimeType("pdf");
                byte[] bArray = null;
                if (!String.IsNullOrEmpty(mimeTypePDF))
                {
                    mimeTypePDF = mimeTypePDF.ToUpper();
                }
                if (item != null)
                {
                    if (item.MimeType.ToUpper() == mimeTypePDF)
                    {
                        bArray = item.ContentByte;
                    }
                    else
                    {
                        // se non è un pdf, ma è stato convertito in pdf allora 
                        // il file sarà disponibile nella colonna ContentBytePDF
                        if (item.ContentBytePDF != null)
                        {
                            bArray = item.ContentBytePDF;
                        }
                    }

                    if (bArray != null)
                    {
                        PdfReader pdfReader = new PdfReader(bArray);
                        int numberOfPages = pdfReader.NumberOfPages;

                        for (int nPag = 1; nPag <= numberOfPages; nPag++)
                        {
                            result.Add(new SelectListItem()
                            {
                                Value = nPag.ToString(),
                                Text = nPag.ToString(),
                                Selected = (nPag == nPagina ? true : false)
                            });
                        }
                    }
                    else
                    {
                        throw new Exception("Errore nel reperimento del file");
                    }
                }
                else
                {
                    throw new Exception("Impossibile trovare il file desiderato");
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ScaricaTemplate(int idTemplate)
        {
            try
            {
                var db = AnagraficaManager.GetDb();
                var item = db.XR_TEMPLATES.Where(w => w.Id.Equals(idTemplate)).FirstOrDefault();
                string nomeFile = "";
                Stream stream = null;

                if (item != null)
                {
                    nomeFile = item.NomeFile;
                    stream = new MemoryStream(item.ContentByte);
                }
                else
                {
                    throw new Exception("Impossibile trovare il file desiderato");
                }

                return new FileStreamResult(stream, item.MimeType) { FileDownloadName = nomeFile };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult IsCustomType()
        {
            dynamic result = new
            {
                esito = true,
                error = "",
                customType = ""
            };

            try
            {
                var db = AnagraficaManager.GetDb();
                string matricola = UtenteHelper.Matricola();
                string tipologiaDocumentale = (string)SessionHelper.Get(matricola + "tipologiaDocumentale");
                string tipologiaDocumento = (string)SessionHelper.Get(matricola + "tipologiaDocumento");

                var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
    w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                if (comportamento == null)
                {
                    throw new Exception("Comportamento non trovato");

                }

                if (!String.IsNullOrEmpty(comportamento.CustomDataJSON))
                {
                    result = new
                    {
                        esito = true,
                        error = "",
                        customType = "CUSTOMDATA"
                    };
                }
                else if (!String.IsNullOrEmpty(comportamento.CustomView))
                {
                    result = new
                    {
                        esito = true,
                        error = "",
                        customType = "CUSTOMVIEW"
                    };

                }
                else
                {
                    result = new
                    {
                        esito = true,
                        error = "",
                        customType = "NONE"
                    };
                }

            }
            catch (Exception ex)
            {
                result.esito = false;
                result.error = ex.Message;
                result.customType = null;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CaricaCustomData(string matricola, bool alternativo = false, List<DEM_RicalcoloElementiInVistaResponse_Item> elementiDefault = null)
        {
            DematerializzazioneCustomDataView model = new DematerializzazioneCustomDataView();
            try
            {
                string matSession = UtenteHelper.Matricola();

                var db = AnagraficaManager.GetDb();

                if (String.IsNullOrEmpty(matricola))
                {
                    matricola = UtenteHelper.Matricola();
                }

                string tipologiaDocumentale = (string)SessionHelper.Get(matSession + "tipologiaDocumentale");
                string tipologiaDocumento = (string)SessionHelper.Get(matSession + "tipologiaDocumento");
                string matricolaDestinatario = (string)SessionHelper.Get(matSession + "MATRICOLA_DESTINATARIO");

                var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
    w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                if (comportamento == null)
                {
                    throw new Exception("Comportamento non trovato");
                }

                List<AttributiAggiuntivi> objD = null;
                objD = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(comportamento.CustomDataJSON);
                model.Attributi = objD;

                if (model.Attributi != null && model.Attributi.Any() &&
                    model.Attributi.Count(w => !String.IsNullOrEmpty(w.Valore)) > 0)
                {
                    foreach (var attr in model.Attributi.Where(w => !String.IsNullOrEmpty(w.Valore)).ToList())
                    {
                        attr.ValoreInModifica = attr.Valore;
                    }
                }

                if (model.Attributi != null && model.Attributi.Any() &&
                    model.Attributi.Count(w => !String.IsNullOrEmpty(w.TagSINTESI1)) > 0)
                {
                    SINTESI1 sintesi = null;
                    using (IncentiviEntities sediDB = new IncentiviEntities())
                    {
                        sintesi = db.SINTESI1.Where(w => w.COD_MATLIBROMAT == matricola).FirstOrDefault();
                        if (sintesi == null)
                        {
                            throw new Exception("Utente non trovato in anagrafica");
                        }
                    }

                    foreach (var attr in model.Attributi.Where(w => !String.IsNullOrEmpty(w.TagSINTESI1)).ToList())
                    {
                        if (!String.IsNullOrEmpty(attr.TagSINTESI1))
                        {
                            var val = sintesi.GetType().GetProperty(attr.TagSINTESI1).GetValue(sintesi, null);

                            if (val != null)
                            {
                                attr.Valore = val.ToString();
                            }
                        }
                    }
                }

                if (alternativo && elementiDefault != null && elementiDefault.Any())
                {
                    var selezione = model.Attributi.Where(w => w.Id == "tipovariazione").FirstOrDefault();
                    if (selezione != null)
                    {
                        model.SceltaViewAlternativa = selezione.Valore;
                    }

                    // carica eventuali dati di default
                    foreach (var e in elementiDefault)
                    {
                        AttributiAggiuntivi daImpostare = model.Attributi.Where(w => w.Id == e.NomeElemento).FirstOrDefault();
                        if (daImpostare == null)
                        {
                            if (model.Attributi != null && model.Attributi.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                            {
                                foreach (var obj in model.Attributi.Where(w => w.InLine != null && w.InLine.Any()).ToList())
                                {
                                    var itemsInListaPiatta = DematerializzazioneManager.GetListaPiatta(obj);

                                    if (itemsInListaPiatta != null && itemsInListaPiatta.Any())
                                    {
                                        daImpostare = itemsInListaPiatta.Where(w => w.Id == e.NomeElemento).FirstOrDefault();
                                    }
                                }
                            }
                        }

                        if (daImpostare != null)
                        {
                            if (e.NomeElemento.ToUpper() == "NOTE")
                            {
                                daImpostare.Valore = "";
                                daImpostare.ValoreInModifica = "";
                            }
                            else
                            {
                                switch (e.NomeElemento.ToUpper())
                                {
                                    case "TIPOVARIAZIONE":
                                        daImpostare.Required = true;
                                        break;
                                    case "DATA_DECORRENZA":
                                        daImpostare.Valore = e.ValoreDefault;
                                        daImpostare.ReadOnly = true;
                                        daImpostare.Required = false;
                                        break;
                                    case "SEDE":
                                        daImpostare.Valore = DematerializzazioneManager.GetDescrizioneSede(e.ValoreDefault);
                                        daImpostare.ReadOnly = !e.Modificabile;
                                        daImpostare.Required = e.CampoObbligatorio;
                                        break;
                                    case "SERVIZIO":
                                        daImpostare.Valore = DematerializzazioneManager.GetDescrizioneServizio(e.ValoreDefault);
                                        daImpostare.ReadOnly = !e.Modificabile;
                                        daImpostare.Required = e.CampoObbligatorio;
                                        break;
                                    case "SEZIONE":
                                        daImpostare.Valore = DematerializzazioneManager.GetDescrizioneSezione_UnitaOrganizzativa(e.ValoreDefault);
                                        daImpostare.ReadOnly = !e.Modificabile;
                                        daImpostare.Required = e.CampoObbligatorio;
                                        break;
                                    default:
                                        daImpostare.Valore = e.ValoreDefault;
                                        daImpostare.Required = false;
                                        break;
                                }
                                daImpostare.ValoreInModifica = e.ValoreDefault;
                                if (e.Selezionato)
                                {
                                    daImpostare.SelectListItems.Where(w => w.Value == e.ValoreDefault).FirstOrDefault().Selected = true;
                                }
                            }
                        }
                    }
                }

                // CARICA LA COMBO MULTISELECT
                // SE PreLoadSelectListItems è VALORIZZATO ALLORA CONTERRà L'INDIRIZZO
                // DEL METODO DA CHIAMARE PER OTTENERE LA LISTA SELECTLISTITEMS DA CARICARE 
                // NELL'ATTRIBUTO SELECTLISTITEMS DELL'OGGETTO IN ESAME
                if (model.Attributi != null &&
                    model.Attributi.Any() &&
                    model.Attributi.Count(w => !String.IsNullOrEmpty(w.PreLoadSelectListItems)) > 0)
                {
                    foreach (var selectDaPrecaricare in model.Attributi.Where(w => !String.IsNullOrEmpty(w.PreLoadSelectListItems)).ToList())
                    {
                        Type thisType = this.GetType();
                        MethodInfo theMethod = thisType.GetMethod(selectDaPrecaricare.PreLoadSelectListItems);
                        int idpersona = 0;
                        List<object> parametri = new List<object>();

                        if (!String.IsNullOrEmpty(matricolaDestinatario))
                        {
                            idpersona = CezanneHelper.GetIdPersona(matricolaDestinatario);
                        }

                        parametri.Add(idpersona);

                        List<SelectListItem> lista = (List<SelectListItem>)theMethod.Invoke(this, parametri.ToArray());
                        if (lista != null && lista.Any())
                        {
                            selectDaPrecaricare.SelectListItems = lista;
                        }
                    }
                }


                // CARICA LA COMBO MULTISELECT
                // SE PreLoadData è VALORIZZATO ALLORA CONTERRà L'INDIRIZZO
                // DEL METODO DA CHIAMARE PER OTTENERE IL TESTO DA CARICARE 
                // NELL'ATTRIBUTO VALUE DELL'OGGETTO IN ESAME
                if (model.Attributi != null &&
                    model.Attributi.Any() &&
                    model.Attributi.Count(w => !String.IsNullOrEmpty(w.PreLoadData)) > 0)
                {
                    foreach (var elementoDaPrecaricare in model.Attributi.Where(w => !String.IsNullOrEmpty(w.PreLoadData)).ToList())
                    {
                        Type thisType = this.GetType();
                        MethodInfo theMethod = thisType.GetMethod(elementoDaPrecaricare.PreLoadData);
                        int idpersona = 0;
                        List<object> parametri = new List<object>();

                        if (!String.IsNullOrEmpty(matricolaDestinatario))
                        {
                            idpersona = CezanneHelper.GetIdPersona(matricolaDestinatario);
                        }

                        parametri.Add(idpersona);

                        string _myValue = (string)theMethod.Invoke(this, parametri.ToArray());
                        if (!String.IsNullOrEmpty(_myValue))
                        {
                            elementoDaPrecaricare.Valore = _myValue;
                            elementoDaPrecaricare.ValoreInModifica = _myValue;
                        }
                    }
                }

                if (model.Attributi != null &&
                   model.Attributi.Any() &&
                   model.Attributi.Count(w => !String.IsNullOrEmpty(w.DBRefAttribute)) > 0)
                {
                    foreach (var elementoDaPrecaricare in model.Attributi.Where(w => !String.IsNullOrEmpty(w.DBRefAttribute)).ToList())
                    {
                        if (elementoDaPrecaricare.DBRefAttribute.Equals("UFFICIO_DESTINATARIO") && elementoDaPrecaricare.Tipo == TipologiaAttributoEnum.SelectMultiSelezione)
                        {
                            model.emailDirezione = DematerializzazioneManager.GetEmailDirezione();
                        }

                        if (elementoDaPrecaricare.DBRefAttribute.Equals("CODICE_PROTOCOLLATORE"))
                        {
                            List<XR_HRIS_PROTOCOLLI> protocolli = new List<XR_HRIS_PROTOCOLLI>();
                            List<SelectListItem> result = new List<SelectListItem>();
                            model.CodiceProtocollatore = new List<string>();

                            protocolli = GetProtocolloPerMatricola(matSession);

                            if (protocolli != null)
                            {
                                foreach (var item in protocolli)
                                {
                                    result.Add(new SelectListItem()
                                    {
                                        Text = item.Struttura,
                                        Value = item.CodificaEsterna,
                                        Selected = false
                                    });
                                    model.CodiceProtocollatore.Add(item.CodiceProtocollo);
                                }

                                if (result != null)
                                {
                                    model.ElementiLstProtocolli = result;
                                }
                            }

                            //Lo elimino cosi non carica la versione vecchia del protocollo
                            model.Attributi.Remove(elementoDaPrecaricare);
                        }
                    }
                }


                return View("~/Views/Dematerializzazione/subpartial/_tabCustomData.cshtml", model);
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        #region Caricamento combo profili custom
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="value"></param>
        /// <param name="parametro">matricola sulla quale effettuare il filtro</param>
        /// <returns></returns>
        //public ActionResult GetSediSelezionabili(string filter, string value, string parametro = null)
        //{
        //    if (String.IsNullOrEmpty(parametro))
        //    {
        //        return Json(DematerializzazioneManager.GetSedi(filter.ToUpper(), value, addCodDes: false), JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        return Json(DematerializzazioneManager.GetSedi(filter.ToUpper(), value, false, null, false, true, false, parametro, true), JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult GetSediSelezionabili(string filter, string value, bool setSelected = false)
        {
            return Json(DematerializzazioneManager.GetSedi(filter.ToUpper(), value, addCodDes: false, setSelected: setSelected), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetServiziSelezionabili(string filter, string value, bool setSelected = false)
        {
            return Json(AnagraficaManager.GetServizi(filter.ToUpper(), value, addCodDes: false, setSelected: setSelected), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="value"></param>
        /// <param name="parametro">codiceServizio</param>
        /// <returns></returns>
        //public ActionResult GetSezioniSelezionabili(string filter, string value, string parametro = null)
        //{
        //    return Json(AnagraficaManager.GetSezioni(filter.ToUpper(), value, parametro), JsonRequestBehavior.AllowGet);
        //}

        public ActionResult GetSezioniSelezionabili(string filter, string value, string parametro = null, bool setSelected = false)
        {
            return Json(AnagraficaManager.GetSezioni(filter.ToUpper(), value, parametro, setSelected: setSelected), JsonRequestBehavior.AllowGet);
        }


        //public ActionResult GetIndennitaSelezionabili(string filter, string value, string parametro = null)
        //{
        //    return Json(AnagraficaManager.GetIndennita(filter, value), JsonRequestBehavior.AllowGet);
        //}

        /// <summary>
        /// Caricamento dell'elenco delle indennità possibili e associate ad un dipendente
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> GetIndennitaSelezionabili(int idpersona)
        {
            DateTime oggi = DateTime.Today;
            List<string> codIndennitaAssegnate = new List<string>();
            List<SelectListItem> result = new List<SelectListItem>();
            IncentiviEntities db = new IncentiviEntities();

            // reperimento delle indennità associate all'utente
            var myIndennita = db.XR_INDENNITA.Where(w => w.ID_PERSONA == idpersona &&
                                                    EntityFunctions.TruncateTime(w.DTA_FINE) >= oggi).ToList();

            IQueryable<XR_TB_INDENNITA> tmp = null;
            tmp = db.XR_TB_INDENNITA;

            if (myIndennita != null && myIndennita.Any())
            {
                codIndennitaAssegnate.AddRange(myIndennita.Select(w => w.COD_INDENNITA).ToList());
            }

            if (tmp != null && tmp.Count() > 0)
            {
                result.AddRange(tmp.OrderBy(x => x.COD_INDENNITA).ToList().Select(x => new SelectListItem
                {
                    Value = x.COD_INDENNITA,
                    Text = x.DES_INDENNITA.TitleCase() + " (" + x.COD_INDENNITA + ")",
                    Selected = codIndennitaAssegnate.Contains(x.COD_INDENNITA) ? true : false
                }));
            }

            return result;
        }

        #endregion

        public ActionResult GetCodiceSedeGappDaHRDW(string filter, string value)
        {
            return Json(DematerializzazioneManager.GetCodiceSedeGappDaHRDW(filter, value), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCategoriaDaHRDW(string filter, string value)
        {
            return Json(DematerializzazioneManager.GetCategoriaDaHRDW(filter, value), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCodiceServizioDaHRDW(string filter, string value)
        {
            return Json(DematerializzazioneManager.GetCodiceServizioDaHRDW(filter, value), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCodiceSezioneDaHRDW(string filter, string value)
        {
            return Json(DematerializzazioneManager.GetCodiceSezioneDaHRDW(filter, value), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMansioneDaHRDW(string filter, string value)
        {
            return Json(DematerializzazioneManager.GetMansioneDaHRDW(filter, value), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AttivaDateSuSelezioneEccezione(string matricola, string eccezione, int id_doc_dem = 0)
        {
            try
            {
                // verifica in base all'eccezione selezionata se vanno abilitate alcune delle date 
                // che di partenza vengono nascoste nella schermata custom
                // ad esempio per l'eccezione MT verifica se va mostrata la data presunta parto oppure
                // la data di nascita del bambino
                IncentiviEntities db = new IncentiviEntities();

                if (String.IsNullOrEmpty(matricola) || String.IsNullOrEmpty(eccezione))
                {
                    throw new Exception("Errore nel reperimento della richiesta MT");
                }

                //var item = db.XR_MAT_RICHIESTE.Where(w => w.MATRICOLA.Equals(matricola) &&
                //                                        w.ECCEZIONE.Equals(eccezione) &&
                //                                        w.DATA_NASCITA_BAMBINO == null &&
                //                                        w.DATA_PRESUNTA_PARTO != null).OrderByDescending(w => w.ID).FirstOrDefault();

                var item = db.XR_MAT_RICHIESTE.Where(w => w.MATRICOLA.Equals(matricola) &&
                                                        w.ECCEZIONE.Equals(eccezione) &&
                                                        w.DATA_PRESUNTA_PARTO != null).OrderByDescending(w => w.ID).FirstOrDefault();

                if (item != null)
                {
                    // se la pratica è in stato da 80 in poi è una pratica chiusa/annullata o comunque da non considerare
                    var ok = db.XR_WKF_OPERSTATI.Count(w => w.COD_TIPO_PRATICA.Equals("MAT") && w.ID_GESTIONE.Equals(item.ID) && w.ID_STATO >= 80) == 0;

                    if (ok)
                    {
                        DateTime? presuntoParto = null;
                        DateTime? parto = null;

                        if (id_doc_dem > 0)
                        {
                            // se è in modifica, cerca il documento in XR_DEM_DOCUMENTI e prende i valori correnti nel json
                            var doc = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(id_doc_dem)).FirstOrDefault();

                            if (doc == null)
                            {
                                throw new Exception("Documento non trovato");
                            }

                            if (!String.IsNullOrEmpty(doc.CustomDataJSON) && doc.CustomDataJSON != "[]")
                            {
                                List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                                DateTime temp;

                                var DBRefAttribute = objModuloValorizzato.Where(w => w.DBRefAttribute == "DATA_PRESUNTA_PARTO").FirstOrDefault();
                                if (DBRefAttribute != null)
                                {
                                    DateTime? _data = null;
                                    string dt = DBRefAttribute.Valore;
                                    if (!String.IsNullOrEmpty(dt))
                                    {
                                        if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                            throw new Exception("Errore in conversione DATA_PRESUNTA_PARTO");
                                        _data = temp;
                                    }
                                    if (_data.HasValue)
                                    {
                                        presuntoParto = _data.GetValueOrDefault();
                                    }
                                }

                                DBRefAttribute = objModuloValorizzato.Where(w => w.DBRefAttribute == "DATA_NASCITA_BAMBINO").FirstOrDefault();
                                if (DBRefAttribute != null)
                                {
                                    DateTime? _data = null;
                                    string dt = DBRefAttribute.Valore;
                                    if (!String.IsNullOrEmpty(dt))
                                    {
                                        if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                            throw new Exception("Errore in conversione DATA_NASCITA_BAMBINO");
                                        _data = temp;
                                    }
                                    if (_data.HasValue)
                                    {
                                        parto = _data.GetValueOrDefault();
                                    }
                                }
                            }

                            if (!item.DATA_PRESUNTA_PARTO.HasValue)
                            {
                                item.DATA_PRESUNTA_PARTO = presuntoParto;
                            }

                            if (!item.DATA_NASCITA_BAMBINO.HasValue)
                            {
                                item.DATA_NASCITA_BAMBINO = parto;
                            }
                        }

                        // se la data presunta parto è contigua al periodo allora continua a funzionare come sempre
                        DateTime? presunto = item.DATA_PRESUNTA_PARTO;

                        if (presunto.HasValue)
                        {
                            DateTime inizio = item.DATA_INIZIO_MATERNITA.GetValueOrDefault();
                            DateTime fine = item.DATA_FINE_MATERNITA.GetValueOrDefault();
                            DateTime finePiuUno = fine.AddDays(1);

                            if (presunto >= inizio && presunto <= finePiuUno)
                            {
                                // è contiguo
                                var result = new
                                {
                                    presuntoParto = (item.DATA_PRESUNTA_PARTO.HasValue ? item.DATA_PRESUNTA_PARTO.GetValueOrDefault().ToString("dd/MM/yyyy") : null),
                                    parto = (item.DATA_NASCITA_BAMBINO.HasValue ? item.DATA_NASCITA_BAMBINO.GetValueOrDefault().ToString("dd/MM/yyyy") : null),
                                    codiceFiscale = (String.IsNullOrEmpty(item.CF_BAMBINO) ? item.CF_BAMBINO : null),
                                    idMatRichiesta = item.ID
                                };

                                return Json(result, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(String.Empty, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            var result = new
                            {
                                presuntoParto = (item.DATA_PRESUNTA_PARTO.HasValue ? item.DATA_PRESUNTA_PARTO.GetValueOrDefault().ToString("dd/MM/yyyy") : null),
                                parto = (item.DATA_NASCITA_BAMBINO.HasValue ? item.DATA_NASCITA_BAMBINO.GetValueOrDefault().ToString("dd/MM/yyyy") : null),
                                codiceFiscale = (String.IsNullOrEmpty(item.CF_BAMBINO) ? item.CF_BAMBINO : null),
                                idMatRichiesta = item.ID
                            };

                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(String.Empty, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(String.Empty, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGiorniPN(string datanascita)
        {
            DateTime D;
            string per = "";
            if (DateTime.TryParseExact(datanascita, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D))
            {
                var Response = GetDatiPeriodo(D, UtenteHelper.Matricola());
                DateTime Dmax = Response.Giorni.Max(x => x.data);
                List<DateTime> ListaLavorativi = new List<DateTime>();
                for (DateTime Dcurrent = D; Dcurrent <= Dmax; Dcurrent = Dcurrent.AddDays(1))
                {
                    var g = Response.Giorni.Where(x => x.data == Dcurrent).FirstOrDefault();
                    if (g != null)
                    {
                        if (!String.IsNullOrWhiteSpace(g.CodiceOrario) && !g.CodiceOrario.StartsWith("9"))
                        {
                            ListaLavorativi.Add(Dcurrent);
                            if (ListaLavorativi.Count == 3)
                            {
                                break;
                            }
                        }
                    }
                }
                per = String.Join(",", ListaLavorativi.Select(x => x.ToString("dd/MM/yyyy")).ToArray());
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true, result = per }
            };
        }

        public MyRaiServiceInterface.MyRaiServiceReference1.GetSchedaPresenzeMeseResponse GetDatiPeriodo(DateTime Dnascita, string matricola)
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            DateTime Dstart = new DateTime(Dnascita.Year, Dnascita.Month, 1);
            DateTime Dend = Dstart.AddMonths(1).AddDays(-1);

            var timbr = cl.GetSchedaPresenzeMese(matricola, Dstart, Dend);

            //GetTimbratureMeseResponse timbr = cl.GetTimbratureMese(Rich.MATRICOLA, Dstart, Dend);
            var timbrPrec = cl.GetSchedaPresenzeMese(matricola, Dstart.AddMonths(-1), Dend.AddMonths(-1));
            var timbrSucc = cl.GetSchedaPresenzeMese(matricola, Dstart.AddMonths(1), Dend.AddMonths(1));
            if (timbrPrec != null && timbrPrec.Giorni != null)
            {
                var g = timbr.Giorni.ToList();
                g.AddRange(timbrPrec.Giorni.ToList());
                timbr.Giorni = g.ToArray();
            }
            if (timbrSucc != null && timbrSucc.Giorni != null)
            {
                var g = timbr.Giorni.ToList();
                g.AddRange(timbrSucc.Giorni.ToList());
                timbr.Giorni = g.ToArray();
            }
            return timbr;
        }

        /// <summary>
        /// Metodo che si occupa di impostare il documento nello stati di 
        /// "pratica conclusa".
        /// Questo metodo può essere chiamato dall'esterno e restituisce un
        /// oggetto di tipo AzioneResult, formato da esito(true/false) ed
        /// eventualmente DescrizioneErrore se esito è false
        /// </summary>
        /// <param name="idRichiesta"></param>
        /// <param name="nota"></param>
        /// <returns></returns>
        public static AzioneResult ConcludiPratica(ref IncentiviEntities db, int idRichiesta, string nota = null)
        {
            AzioneResult result = new AzioneResult();
            result.Esito = false;
            try
            {
                var item = db.XR_DEM_DOCUMENTI.Where(w => w.Id_Richiesta != null && w.Id_Richiesta.Value.Equals(idRichiesta)).FirstOrDefault();

                if (item == null)
                {
                    throw new Exception("Impossibile trovare il documento richiesto");
                }

                DateTime ora = DateTime.Now;
                int nuovoStato = (int)StatiDematerializzazioneDocumenti.PraticaConclusa;

                item.Id_Stato = nuovoStato;
                item.DataChiusuraPratica = ora;

                string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ip))
                {
                    ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = ip,
                    COD_USER = UtenteHelper.Matricola(),
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = item.Id,
                    ID_PERSONA = CommonHelper.GetCurrentIdPersona(),
                    ID_STATO = item.Id_Stato,
                    ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = DateTime.Now,
                    TMS_TIMESTAMP = DateTime.Now
                });

                db.SaveChanges();
                result.Esito = true;
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.DescrizioneErrore = ex.Message;
            }

            return result;
        }

        public static AzioneResult PrendiInCaricoPratica(ref IncentiviEntities db, int idRichiesta, string matricolaIncaricato)
        {
            AzioneResult result = new AzioneResult();
            result.Esito = false;
            try
            {
                var item = db.XR_DEM_DOCUMENTI.Where(w => w.Id_Richiesta != null && w.Id_Richiesta.Value.Equals(idRichiesta)).FirstOrDefault();

                if (item == null)
                {
                    throw new Exception("Impossibile trovare il documento richiesto");
                }

                DateTime ora = DateTime.Now;
                int nuovoStato = (int)StatiDematerializzazioneDocumenti.PresaInCarico;

                item.Id_Stato = nuovoStato;
                item.DataPresaInCarico = DateTime.Now;
                item.MatricolaIncaricato = matricolaIncaricato;

                string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ip))
                {
                    ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = ip,
                    COD_USER = UtenteHelper.Matricola(),
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = item.Id,
                    ID_PERSONA = CommonHelper.GetCurrentIdPersona(),
                    ID_STATO = item.Id_Stato,
                    ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = DateTime.Now,
                    TMS_TIMESTAMP = DateTime.Now
                });

                db.SaveChanges();
                result.Esito = true;
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.DescrizioneErrore = ex.Message;
            }

            return result;
        }

        public Dematerializzazione_Dettaglio_Mat_Richieste GetDatiAggiuntivi_Mat_Richieste(int idDoc, int idRichiesta, string json)
        {
            Dematerializzazione_Dettaglio_Mat_Richieste result = null;

            try
            {
                // prima di tutto deve capire con quale eccezione lavora così da identificare i 
                // campi da considerare
                string codEccezione = "";
                List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(json);
                var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                if (tt != null)
                {
                    codEccezione = tt.Valore;
                }

                if (String.IsNullOrEmpty(codEccezione))
                {
                    throw new Exception("Errore nel reperimento dell'eccezione");
                }

                switch (codEccezione)
                {
                    case "PN":
                        result = GetDatiAggiuntivi_Mat_Richieste_PN(idRichiesta, ref result);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        private Dematerializzazione_Dettaglio_Mat_Richieste GetDatiAggiuntivi_Mat_Richieste_PN(int idRichiesta, ref Dematerializzazione_Dettaglio_Mat_Richieste result)
        {
            result = new Dematerializzazione_Dettaglio_Mat_Richieste();
            result.Titolo = "Permessi indennizzati";
            result.Items = new List<Dematerializzazione_Dettaglio_Mat_Richieste_Item>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                var item = db.XR_MAT_RICHIESTE.Where(w => w.ID.Equals(idRichiesta)).FirstOrDefault();
                if (item != null)
                {
                    var approvato = item.XR_WKF_OPERSTATI.Where(w => w.ID_STATO.Equals(50) && w.ID_STATO <= 80).FirstOrDefault();
                    if (approvato != null)
                    {
                        int dd = item.NUMERO_GIORNI_GIUSTIFICATIVO.GetValueOrDefault();
                        string tx = "";
                        if (dd > 1 || dd == 0)
                        {
                            tx = String.Format("{0} giorni", dd);
                        }
                        else
                        {
                            tx = String.Format("{0} giorno", dd);
                        }

                        result.Items.Add(new Dematerializzazione_Dettaglio_Mat_Richieste_Item()
                        {
                            Label = "PN",
                            Text = tx
                        });
                    }
                    else
                    {
                        result = null;
                    }

                    var last = item.XR_WKF_OPERSTATI.Last();

                    if (last != null)
                    {
                        if (!String.IsNullOrEmpty(last.COD_USER) && !String.IsNullOrEmpty(last.NOMINATIVO))
                        {
                            result.Items.Add(new Dematerializzazione_Dettaglio_Mat_Richieste_Item()
                            {
                                Label = "IN CARICO A",
                                Text = last.NOMINATIVO.Trim()
                            });
                        }
                    }

                    // prende le date
                    DateTime dt1 = item.INIZIO_GIUSTIFICATIVO.GetValueOrDefault();
                    DateTime dt2 = item.FINE_GIUSTIFICATIVO.GetValueOrDefault();
                    string rispostaRichiestaPagata = "";
                    string tempRispostaRichiestaPagata = "";

                    var dateSpan = DateTimeSpan.CompareDates(dt1, dt2);

                    int addMonths = 0;
                    int years = dateSpan.Years;
                    int months = dateSpan.Months;
                    int days = dateSpan.Days;
                    DateTime dtStart = dt1;
                    DateTime dtEnd = dt1.AddMonths(1);
                    dtEnd = new DateTime(dtEnd.Year, dtEnd.Month, 1);
                    dtEnd = dtEnd.AddDays(-1);

                    if (years > 0)
                    {
                        addMonths = years * 12;
                    }

                    months += addMonths;

                    if (months == 0)
                    {
                        tempRispostaRichiestaPagata = MaternitaCongediManager.RichiestaPagata(idRichiesta, dt1, dt2);
                        tempRispostaRichiestaPagata = "202110";
                        if (!String.IsNullOrEmpty(tempRispostaRichiestaPagata))
                        {
                            string sub = tempRispostaRichiestaPagata.Substring(4, 2);
                            int mesePagamento = int.Parse(sub);
                            string fullMonthName = new DateTime(dtStart.Year, mesePagamento, 1).ToString("MMMM", CultureInfo.CreateSpecificCulture("it"));
                            rispostaRichiestaPagata += " " + String.Format("PAGATO in {0}", fullMonthName);
                        }
                    }
                    else
                    {
                        dtStart = dt1;
                        for (int i = 1; i <= months; i++)
                        {
                            dtEnd = dt1.AddMonths(1);
                            dtEnd = new DateTime(dtEnd.Year, dtEnd.Month, 1);
                            dtEnd = dtEnd.AddDays(-1);

                            tempRispostaRichiestaPagata = MaternitaCongediManager.RichiestaPagata(idRichiesta, dtStart, dtEnd);
                            if (!String.IsNullOrEmpty(tempRispostaRichiestaPagata))
                            {
                                string sub = tempRispostaRichiestaPagata.Substring(4, 2);
                                int mesePagamento = int.Parse(sub);
                                string fullMonthName = new DateTime(dtStart.Year, mesePagamento, 1).ToString("MMMM", CultureInfo.CreateSpecificCulture("it"));
                                rispostaRichiestaPagata += " " + String.Format("PAGATO in {0}", fullMonthName);
                            }

                            dtStart = dtEnd.AddMonths(1);
                        }

                        if (days > 0)
                        {
                            dtStart = dtEnd;
                            dtEnd = dt2;
                            tempRispostaRichiestaPagata = MaternitaCongediManager.RichiestaPagata(idRichiesta, dtStart, dtEnd);
                            if (!String.IsNullOrEmpty(tempRispostaRichiestaPagata))
                            {
                                string sub = tempRispostaRichiestaPagata.Substring(4, 2);
                                int mesePagamento = int.Parse(sub);
                                string fullMonthName = new DateTime(dtStart.Year, mesePagamento, 1).ToString("MMMM", CultureInfo.CreateSpecificCulture("it"));
                                rispostaRichiestaPagata += " " + String.Format("PAGATO in {0}", fullMonthName);
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(rispostaRichiestaPagata))
                    {
                        result.Items.Add(new Dematerializzazione_Dettaglio_Mat_Richieste_Item()
                        {
                            Label = "STATO",
                            Text = rispostaRichiestaPagata
                        });
                    }
                }
                else
                {
                    result = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Il metodo aggiunge ai dati custom presenti, nuovi elementi presi dal wkf
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="idPersona"></param>
        /// <param name="idDoc"></param>
        /// <param name="customAttrs"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateDocumento_DatiAggiuntiviWKF(string matricola, int idPersona, int idDoc, string customAttrs)
        {
            if (idPersona == 0)
            {
                idPersona = CommonHelper.GetCurrentIdPersona();
            }

            List<AttributiAggiuntivi> objSaved = new List<AttributiAggiuntivi>();
            List<AttributiAggiuntivi> objToAdd = new List<AttributiAggiuntivi>();

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                    if (item == null)
                    {
                        throw new Exception("Documento non trovato");
                    }

                    if (!String.IsNullOrEmpty(item.CustomDataJSON))
                    {
                        // prende i dati custom già salvati e li mette in una lista
                        objSaved = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(item.CustomDataJSON);
                    }

                    // i dati da salvare li trasforma a sua volta in una lista sempre di AttributiAggiuntivi
                    objToAdd = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(customAttrs);

                    // a questo punto fa il merge delle due liste
                    List<AttributiAggiuntivi> newList = new List<AttributiAggiuntivi>();
                    newList.AddRange(objSaved.ToList());

                    newList.RemoveAll(w => objToAdd.Select(x => x.Id).Contains(w.Id));

                    newList.AddRange(objToAdd.ToList());

                    // Serializza la lista trasformandola in stringa e la salva nel campo CustomDataJSON del documento
                    var jsonString = JsonConvert.SerializeObject(newList);
                    item.CustomDataJSON = jsonString;

                    db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                    {
                        COD_TERMID = Request.UserHostAddress,
                        COD_USER = matricola,
                        DTA_OPERAZIONE = 0,
                        COD_TIPO_PRATICA = "DEM",
                        ID_GESTIONE = idDoc,
                        ID_PERSONA = idPersona,
                        ID_STATO = (int)StatiDematerializzazioneDocumenti.AggiuntaDatiJson,
                        ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                        NOMINATIVO = UtenteHelper.Nominativo(),
                        VALID_DTA_INI = DateTime.Now,
                        TMS_TIMESTAMP = DateTime.Now
                    });

                    db.SaveChanges();
                }

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        result = "OK",
                        infoAggiuntive = ""
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
                        result = "KO",
                        infoAggiuntive = "Si è verificato un errore: " + ex.Message
                    }
                };
            }
        }

        /// <summary>
        /// Metodo utilizzato per aggiornare il box col dettaglio degli attributi custom
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="idPersona"></param>
        /// <param name="idDoc"></param>
        /// <param name="approvatoreEnabled"></param>
        /// <param name="presaInCaricoEnabled"></param>
        /// <param name="presaInVisioneEnabled"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult RefreshGetDettaglioRichiesta(string matricola, int idPersona, int idDoc, bool approvatoreEnabled = false, bool presaInCaricoEnabled = false, bool presaInVisioneEnabled = false)
        {
            var db = AnagraficaManager.GetDb();
            var item = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();
            DateTime ora = DateTime.Now;
            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM();
            model.IsOperatore = DematerializzazioneManager.IsOperatore();

            if (item == null)
            {
                throw new Exception("Impossibile reperire il documento");
            }

            if (String.IsNullOrEmpty(matricola))
            {
                matricola = UtenteHelper.Matricola();
            }

            if (idPersona == 0)
            {
                idPersona = CommonHelper.GetCurrentIdPersona();
            }

            if (item.DataPresaInModifica.HasValue)
            {
                if (item.MatricolaPresaInModifica != matricola)
                {
                    if ((ora - item.DataPresaInModifica.Value).TotalMinutes <= 15)
                    {
                        model.MessaggioPraticaBloccata = true;
                        string nominativo = DematerializzazioneManager.GetNominativoByMatricola(item.MatricolaPresaInModifica);
                        model.Msg1 = "Pratica in modifica da " + nominativo;
                        model.Msg2 = nominativo + " potrebbe star modificando questa pratica. Attendi che venga rilasciata per lavorarla";
                    }
                    else if ((ora - item.DataPresaInModifica.Value).TotalMinutes > 15)
                    {
                        model.MessaggioPraticaBloccata = false;
                        model.Msg1 = "";
                        model.Msg2 = "";
                    }
                }
            }

            model.Richiesta = GetDocumentData(idDoc);
            model.ApprovazioneEnabled = approvatoreEnabled;
            model.PresaInCaricoEnabled = presaInCaricoEnabled;
            model.PresaInVisioneEnabled = presaInVisioneEnabled;

            if (model.Richiesta != null)
            {
                if (model.Richiesta.Documento.Id_Stato < (int)StatiDematerializzazioneDocumenti.Accettato)
                {
                    model.AbilitaModifica = true;
                }
                model.Matricola = matricola;
                model.IdPersona = idPersona;

                string tipologiaDocumentale = model.Richiesta.Documento.Cod_Tipologia_Documentale;
                int id_tipo_doc = model.Richiesta.Documento.Id_Tipo_Doc;
                string tipologiaDocumento = "";

                var dem_TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(id_tipo_doc)).FirstOrDefault();

                if (dem_TIPI_DOCUMENTO != null)
                {
                    tipologiaDocumento = dem_TIPI_DOCUMENTO.Codice;
                }

                if (tipologiaDocumento == "ASS")
                {
                    model.AbilitaModifica = false;
                }

                model.NominativoUtenteApprovatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaApprovatore);
                model.NominativoUtenteCreatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaCreatore);
                model.NominativoUtenteDestinatario = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaDestinatario);
                model.NominativoUtenteFirma = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaFirma);
                model.NominativoUtenteIncaricato = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaIncaricato);

                if (!String.IsNullOrEmpty(model.Richiesta.Documento.MatricolaVisualizzatore))
                {
                    model.NominativoUtenteVisionatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaVisualizzatore);
                }

                if (!String.IsNullOrEmpty(model.Richiesta.Documento.CustomDataJSON))
                {
                    List<AttributiAggiuntivi> objD = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(model.Richiesta.Documento.CustomDataJSON);
                    model.Attributi = objD;
                }

                // se il documento è legato ad una XR_MAT_RICHIESTA
                // allora mostrerà i dati aggiuntivi dello stato della XR_MAT_RICHIESTA
                if (model.Richiesta.Documento.Id_Richiesta.HasValue && !String.IsNullOrEmpty(model.Richiesta.Documento.CustomDataJSON) &&
                    model.Richiesta.Documento.CustomDataJSON != "[]")
                {
                    model.Dettaglio_Mat_Richieste = GetDatiAggiuntivi_Mat_Richieste(model.Richiesta.Documento.Id, model.Richiesta.Documento.Id_Richiesta.Value, model.Richiesta.Documento.CustomDataJSON);
                }

                int id_WKF_Tipologia = model.Richiesta.Documento.Id_WKF_Tipologia;
                int idStato = model.Richiesta.Documento.Id_Stato;
                var itemWKF = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(id_WKF_Tipologia) && w.ID_STATO.Equals(idStato)).FirstOrDefault();

                if (itemWKF != null)
                {
                    model.AbilitaAggiuntaInfoJson = !String.IsNullOrEmpty(itemWKF.CUSTOMDATAJSON);

                    if (model.AbilitaAggiuntaInfoJson)
                    {
                        List<AttributiAggiuntivi> objJson = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(itemWKF.CUSTOMDATAJSON);
                        model.AttributiAggiuntiWKF = objJson;

                        model.AttributiAggiuntiWKF.RemoveAll(w => model.Attributi.Select(x => x.Id).Contains(w.Id));
                    }

                    if (!model.AttributiAggiuntiWKF.Any())
                    {
                        // nasconde il bottone aggiungi info, in quanto 
                        // i campi aggiuntivi sono stati già valorizzati 
                        // in precedenza, infatti la lista coi campi aggiuntivi
                        // è vuota.
                        model.AbilitaAggiuntaInfoJson = false;
                        model.AbilitaModificaInfoJson = true;
                    }
                }
            }

            return View("~/Views/Dematerializzazione/subpartial/_ContenitoreAttributiAggiuntivi.cshtml", model);
        }

        /// <summary>
        /// abilita la modifica dei soli elementi custom previsti nello stato in cui si trova il documento
        /// l'elenco degli item modificabili viene preso dal workflow
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="idPersona"></param>
        /// <param name="idDoc"></param>
        /// <param name="approvatoreEnabled"></param>
        /// <param name="presaInCaricoEnabled"></param>
        /// <param name="presaInVisioneEnabled"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ModificaDatiAggiuntiviDettaglioRichiesta(string matricola, int idPersona, int idDoc, bool approvatoreEnabled = false, bool presaInCaricoEnabled = false, bool presaInVisioneEnabled = false)
        {
            var db = AnagraficaManager.GetDb();
            var item = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();
            DateTime ora = DateTime.Now;
            DettaglioDematerializzazioneVM model = new DettaglioDematerializzazioneVM();
            model.IsOperatore = DematerializzazioneManager.IsOperatore();

            if (item == null)
            {
                throw new Exception("Impossibile reperire il documento");
            }

            if (String.IsNullOrEmpty(matricola))
            {
                matricola = UtenteHelper.Matricola();
            }

            if (idPersona == 0)
            {
                idPersona = CommonHelper.GetCurrentIdPersona();
            }

            if (item.DataPresaInModifica.HasValue)
            {
                if (item.MatricolaPresaInModifica != matricola)
                {
                    if ((ora - item.DataPresaInModifica.Value).TotalMinutes <= 15)
                    {
                        model.MessaggioPraticaBloccata = true;
                        string nominativo = DematerializzazioneManager.GetNominativoByMatricola(item.MatricolaPresaInModifica);
                        model.Msg1 = "Pratica in modifica da " + nominativo;
                        model.Msg2 = nominativo + " potrebbe star modificando questa pratica. Attendi che venga rilasciata per lavorarla";
                    }
                    else if ((ora - item.DataPresaInModifica.Value).TotalMinutes > 15)
                    {
                        model.MessaggioPraticaBloccata = false;
                        model.Msg1 = "";
                        model.Msg2 = "";
                    }
                }
            }

            model.Richiesta = GetDocumentData(idDoc);
            model.ApprovazioneEnabled = approvatoreEnabled;
            model.PresaInCaricoEnabled = presaInCaricoEnabled;
            model.PresaInVisioneEnabled = presaInVisioneEnabled;

            if (model.Richiesta != null)
            {
                if (model.Richiesta.Documento.Id_Stato < (int)StatiDematerializzazioneDocumenti.Accettato)
                {
                    model.AbilitaModifica = true;
                }
                model.Matricola = matricola;
                model.IdPersona = idPersona;

                string tipologiaDocumentale = model.Richiesta.Documento.Cod_Tipologia_Documentale;
                int id_tipo_doc = model.Richiesta.Documento.Id_Tipo_Doc;
                string tipologiaDocumento = "";

                var dem_TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(id_tipo_doc)).FirstOrDefault();

                if (dem_TIPI_DOCUMENTO != null)
                {
                    tipologiaDocumento = dem_TIPI_DOCUMENTO.Codice;
                }

                if (tipologiaDocumento == "ASS")
                {
                    model.AbilitaModifica = false;
                }

                model.NominativoUtenteApprovatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaApprovatore);
                model.NominativoUtenteCreatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaCreatore);
                model.NominativoUtenteDestinatario = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaDestinatario);
                model.NominativoUtenteFirma = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaFirma);
                model.NominativoUtenteIncaricato = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaIncaricato);

                if (!String.IsNullOrEmpty(model.Richiesta.Documento.MatricolaVisualizzatore))
                {
                    model.NominativoUtenteVisionatore = DematerializzazioneManager.GetNominativoByMatricola(model.Richiesta.Documento.MatricolaVisualizzatore);
                }

                if (!String.IsNullOrEmpty(model.Richiesta.Documento.CustomDataJSON))
                {
                    List<AttributiAggiuntivi> objD = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(model.Richiesta.Documento.CustomDataJSON);
                    model.Attributi = objD;
                }

                // se il documento è legato ad una XR_MAT_RICHIESTA
                // allora mostrerà i dati aggiuntivi dello stato della XR_MAT_RICHIESTA
                if (model.Richiesta.Documento.Id_Richiesta.HasValue && !String.IsNullOrEmpty(model.Richiesta.Documento.CustomDataJSON) &&
                    model.Richiesta.Documento.CustomDataJSON != "[]")
                {
                    model.Dettaglio_Mat_Richieste = GetDatiAggiuntivi_Mat_Richieste(model.Richiesta.Documento.Id, model.Richiesta.Documento.Id_Richiesta.Value, model.Richiesta.Documento.CustomDataJSON);
                }

                int id_WKF_Tipologia = model.Richiesta.Documento.Id_WKF_Tipologia;
                int idStato = model.Richiesta.Documento.Id_Stato;
                var itemWKF = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(id_WKF_Tipologia) && w.ID_STATO.Equals(idStato)).FirstOrDefault();

                if (itemWKF != null)
                {
                    model.AbilitaAggiuntaInfoJson = !String.IsNullOrEmpty(itemWKF.CUSTOMDATAJSON);

                    if (model.AbilitaAggiuntaInfoJson)
                    {
                        model.AbilitaModificaInfoJson = true;
                        List<AttributiAggiuntivi> objJson = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(itemWKF.CUSTOMDATAJSON);
                        model.AttributiAggiuntiWKF = objJson;

                        var toModify = model.Attributi.Where(w => model.AttributiAggiuntiWKF.Select(x => x.Id).Contains(w.Id)).ToList();

                        if (toModify != null && toModify.Any())
                        {
                            foreach (var t in toModify)
                            {
                                var f = model.AttributiAggiuntiWKF.Where(w => w.Id.Equals(t.Id)).FirstOrDefault();

                                if (f != null)
                                {
                                    f.ValoreInModifica = t.Valore;
                                }
                            }
                        }

                        model.Attributi.RemoveAll(w => model.AttributiAggiuntiWKF.Select(x => x.Id).Contains(w.Id));
                    }

                    model.AbilitaAggiuntaInfoJson = false;
                    model.AbilitaModificaInfoJson = true;
                }
            }

            return View("~/Views/Dematerializzazione/subpartial/_ContenitoreAttributiAggiuntivi.cshtml", model);
        }

        /// <summary>
        /// Metodo per la duplicazione di un documento.
        /// Crea un nuovo documento in stato bozza, con gli stessi allegati dell'originale e 
        /// con lo stesso eventuale oggetto json valorizzato.
        /// </summary>
        /// <param name="idDoc"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DuplicaPratica(int idDoc)
        {
            Dematerializzazione_EsitoAjax response = new Dematerializzazione_EsitoAjax();
            RichiestaDoc richiesta = new RichiestaDoc();
            XR_DEM_DOCUMENTI doc = null;
            IncentiviEntities db = new IncentiviEntities();

            try
            {
                // reperimento del documento da clonare
                richiesta = GetDocumentData(idDoc);

                doc = new XR_DEM_DOCUMENTI()
                {
                    Id = -1,
                    Descrizione = richiesta.Documento.Descrizione,
                    DataCreazione = DateTime.Now,
                    Id_Stato = (int)StatiDematerializzazioneDocumenti.Bozza,
                    Cod_Tipologia_Documentale = richiesta.Documento.Cod_Tipologia_Documentale,
                    Id_WKF_Tipologia = richiesta.Documento.Id_WKF_Tipologia,
                    MatricolaCreatore = UtenteHelper.Matricola(),
                    IdPersonaCreatore = CommonHelper.GetCurrentIdPersona(),
                    MatricolaDestinatario = richiesta.Documento.MatricolaDestinatario,
                    IdPersonaDestinatario = richiesta.Documento.IdPersonaDestinatario,
                    Note = richiesta.Documento.Note,
                    Id_Tipo_Doc = richiesta.Documento.Id_Tipo_Doc,
                    MatricolaApprovatore = richiesta.Documento.MatricolaApprovatore,
                    MatricolaFirma = null,
                    MatricolaIncaricato = null,
                    CustomDataJSON = richiesta.Documento.CustomDataJSON,
                    IdArea = richiesta.Documento.IdArea
                };

                int tempId = 0;
                foreach (var a in richiesta.Allegati)
                {
                    tempId--;

                    XR_DEM_VERSIONI_DOCUMENTO version = new XR_DEM_VERSIONI_DOCUMENTO()
                    {
                        N_Versione = 1,
                        DataUltimaModifica = DateTime.Now,
                        Id_Documento = doc.Id,
                        Id = tempId
                    };

                    db.XR_DEM_VERSIONI_DOCUMENTO.Add(version);

                    XR_ALLEGATI allegato = new XR_ALLEGATI()
                    {
                        ContentByte = a.ContentByte,
                        ContentBytePDF = a.ContentBytePDF,
                        Id = tempId,
                        IsPrincipal = a.IsPrincipal,
                        Length = a.Length,
                        MimeType = a.MimeType,
                        NomeFile = a.NomeFile,
                        PosizioneProtocollo = a.PosizioneProtocollo
                    };

                    XR_DEM_ALLEGATI_VERSIONI associativa = new XR_DEM_ALLEGATI_VERSIONI()
                    {
                        IdAllegato = allegato.Id,
                        IdVersione = version.Id
                    };

                    db.XR_ALLEGATI.Add(allegato);
                    db.XR_DEM_ALLEGATI_VERSIONI.Add(associativa);
                }

                db.XR_DEM_DOCUMENTI.Add(doc);
                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = Request.UserHostAddress,
                    COD_USER = UtenteHelper.Matricola(),
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = doc.Id,
                    ID_PERSONA = CommonHelper.GetCurrentIdPersona(),
                    ID_STATO = doc.Id_Stato,
                    ID_TIPOLOGIA = doc.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = DateTime.Now,
                    TMS_TIMESTAMP = DateTime.Now
                });

                db.SaveChanges();
                response.Esito = true;
                response.ErrorMessage = String.Empty;
                response.Id = doc.Id;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.ErrorMessage = ex.Message;
                response.Id = 0;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #region IN PARTENZA

        public ActionResult InPartenza()
        {
            DematerializzazioneDocumentiInPartenzaVM model = new DematerializzazioneDocumentiInPartenzaVM();
            model.IsApprovatore = DematerializzazioneManager.IsApprovatore();
            model.IsOperatore = DematerializzazioneManager.IsOperatore();
            model.IsVisionatore = DematerializzazioneManager.IsVisionatore();
            model.IsSegreteria = DematerializzazioneManager.IsSegreteria();
            model.IdPersona = CommonHelper.GetCurrentIdPersona();
            model.Matricola = CommonHelper.GetCurrentUserMatricola();
            model.IsPreview = true;
            return View("~/Views/Dematerializzazione/InPartenza.cshtml", model);
        }

        public ActionResult GetContentInternal(bool approvazioneEnabled = false, string nominativo = null, string oggetto = null, string matricola = null, string id_Tipo_Doc = null, DateTime? datadal = null)
        {
            DateTime defaultDate = new DateTime(1900, 1, 1);
            DematerializzazioneDocumentiVM model = new DematerializzazioneDocumentiVM();
            model.IsPreview = false;
            model.ApprovazioneEnabled = approvazioneEnabled;
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>();
            model.Documenti = DematerializzazioneManager.GetDocumentiDaApprovare2(nominativo, oggetto, matricola, id_Tipo_Doc);
            model.Matricola = CommonHelper.GetCurrentUserMatricola();

            //Se sei Approvatore e utilizzi la ricerca allora carico anche il pregresso
            model.IsApprovatore = DematerializzazioneManager.IsApprovatore();
            if (model.IsApprovatore && (!string.IsNullOrEmpty(nominativo) || !string.IsNullOrEmpty(matricola) || !string.IsNullOrEmpty(id_Tipo_Doc) || (datadal.HasValue && datadal != defaultDate)))
            {
                List<XR_DEM_DOCUMENTI_EXT> lstDoc = new List<XR_DEM_DOCUMENTI_EXT>();
                lstDoc = DematerializzazioneManager.GetDocumentiFiltrati(TIPI_UTENTI.Approvatore, nominativo, matricola, id_Tipo_Doc, datadal);
                if (lstDoc != null)
                {
                    model.Documenti.AddRange(lstDoc);
                    model.Documenti = model.Documenti.DistinctBy(w => w.Id).ToList();
                }
            }

            //Ordinamento documenti in base alla DataDiScadenza
            DematerializzazioneHelper demaHelper = new DematerializzazioneHelper();
            model = demaHelper.OrdinamentoListaDocumentiManager(model);

            if (model.Documenti != null && model.Documenti.Any())
                model.NascondiCheckBox = model.Documenti.Count(w => w.Id_Stato < 40) == 0;
            return View("~/Views/Dematerializzazione/subpartial/ContentInternal.cshtml", model);
        }




        public ActionResult GetContentOperatore(string nominativo = null, string matricola = null, string oggetto = null, string id_Tipo_Doc = null, DateTime? datadal = null)
        {
            DateTime defaultDate = new DateTime(1900, 1, 1);
            DematerializzazioneDocumentiVM model = new DematerializzazioneDocumentiVM();
            model.IsPreview = false;
            model.ApprovazioneEnabled = false;
            model.PrendiInCaricoEnabled = false;
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>();
            model.IsApprovatore = DematerializzazioneManager.IsApprovatore();
            model.IsVisionatore = DematerializzazioneManager.IsVisionatore();
            model.IsOperatore = DematerializzazioneManager.IsOperatore();
            model.IsSegreteria = DematerializzazioneManager.IsSegreteria();

            if (model.IsOperatore)
            {
                model.Documenti = DematerializzazioneManager.GetDocumentiPerOperatori(nominativo, matricola, oggetto, id_Tipo_Doc, datadal);
            }

            if (model.IsSegreteria)
            {
                var docs = DematerializzazioneManager.GetDocumentiPerSegreteria(nominativo, matricola, oggetto, id_Tipo_Doc, datadal);
                docs = docs.Where(k => k.Id_Stato >= (int)StatiDematerializzazioneDocumenti.Accettato).ToList();
                if (model.Documenti == null || !model.Documenti.Any())
                {
                    model.Documenti = docs;
                }
                else
                {
                    model.Documenti.AddRange(docs.Except(model.Documenti).ToList());
                }
            }

            if (model.IsVisionatore)
            {
                List<XR_DEM_DOCUMENTI_EXT> _temp = new List<XR_DEM_DOCUMENTI_EXT>();
                var _lista1 = DematerializzazioneManager.GetDocumentiDaVisionare(nominativo, matricola, oggetto, id_Tipo_Doc, datadal);

                if (_lista1 != null && _lista1.Any())
                {
                    _temp.AddRange(_lista1);
                }

                var _lista2 = DematerializzazioneManager.GetDocumentiDaVisionare2(nominativo, matricola, oggetto, id_Tipo_Doc, datadal);
                if (_lista2 != null && _lista2.Any())
                {
                    _temp.AddRange(_lista2);
                }

                _temp = _temp.DistinctBy(w => w.Id).ToList();

                model.DocumentiDaVisionare = _temp;
            }

            //Se sei Visionatore e utilizzi la ricerca allora carico tutto
            model.IsVisionatore = DematerializzazioneManager.IsVisionatore();
            if (model.IsVisionatore && (!string.IsNullOrEmpty(nominativo) || !string.IsNullOrEmpty(matricola) || !string.IsNullOrEmpty(id_Tipo_Doc) || (datadal.HasValue && datadal != defaultDate)))
            {
                model.DocumentiDaVisionare.AddRange(DematerializzazioneManager.GetDocumentiFiltrati(TIPI_UTENTI.Vistatore, nominativo, matricola, id_Tipo_Doc, datadal).ToList());
                model.DocumentiDaVisionare = model.DocumentiDaVisionare.DistinctBy(w => w.Id).ToList();
            }

            model.Matricola = CommonHelper.GetCurrentUserMatricola();
            //Ordinamento documenti in base alla DataDiScadenza
            DematerializzazioneHelper demaHelper = new DematerializzazioneHelper();
            model = demaHelper.OrdinamentoListaDocumentiManager(model);

            return View("~/Views/Dematerializzazione/subpartial/ContentOperatore.cshtml", model);
        }

        public static List<SelectListItem> GetFiltroTipologieDematerializzazioni(int? id = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            result.Add(new SelectListItem()
            {
                Value = "-1",
                Text = "Seleziona un valore"
            });

            using (IncentiviEntities db = new IncentiviEntities())
            {
                // prima carica le tipologie per le quali la matricola ha visibilità
                List<string> TIPOLOGIE_DOCUMENTALI_VISIBILI = new List<string>();
                string matr = UtenteHelper.Matricola().ToString();
                var isSegreteria = DematerializzazioneManager.IsSegreteria();

                if (isSegreteria)
                {
                    var tipiDocInclu = db.XR_HRIS_ABIL.AsNoTracking().Where(k => k.MATRICOLA.Equals(matr) && k.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE.Equals("01SEGR")).Select(x => x.TIP_DOC_INCLUSI).FirstOrDefault();
                    var tipidocumento = db.XR_DEM_TIPI_DOCUMENTO.AsNoTracking().ToList();
                    var obj = new List<Object>();
                    List<ListaTipiDocRaggruppati> lst = new List<ListaTipiDocRaggruppati>();


                    foreach (var item in tipiDocInclu.Split(',').ToList())
                    {
                        obj.Add(tipidocumento.Where(k => k.Codice == item).Select(x => new { x.Id, x.Descrizione }).FirstOrDefault());
                        ListaTipiDocRaggruppati doc = new ListaTipiDocRaggruppati();

                        var descrizione = tipidocumento.Where(k => k.Codice == item).Select(x => x.Descrizione).FirstOrDefault();
                        var check = lst.Where(k => k.NomeTipologiaDocumentale.Equals(descrizione)).FirstOrDefault();
                        if (check == null)
                        {
                            doc.NomeTipologiaDocumentale = descrizione;
                            doc.ListaId.Add(tipidocumento.Where(k => k.Codice == item).Select(x => x.Id).FirstOrDefault());
                            lst.Add(doc);
                        }
                        else
                        {
                            check.ListaId.Add(tipidocumento.Where(k => k.Codice == item).Select(x => x.Id).FirstOrDefault());
                        }

                    }

                    foreach (var item in lst)
                    {
                        result.Add(new SelectListItem()
                        {
                            Value = string.Join(", ", item.ListaId),
                            Text = item.NomeTipologiaDocumentale
                        });
                    }


                }
                else
                {
                    var _tempTip = db.XR_DEM_TIPOLOGIE_DOCUMENTALI.Where(w => w.Attivo && (
                        (w.MatricoleAbilitate.Contains(matr) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matr))) ||
                        (w.MatricoleAbilitate.Contains(matr) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                        (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matr))) ||
                        (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                    )).ToList();

                    if (_tempTip != null && _tempTip.Any())
                    {
                        TIPOLOGIE_DOCUMENTALI_VISIBILI.AddRange(_tempTip.Select(w => w.Codice).ToList());
                    }
                    else
                    {
                        throw new Exception("L'Utente non ha visibilità su alcuna tipologia");
                    }

                    List<string> TIPI_DOCUMENTO_VISIBILI = new List<string>();
                    List<int> ID_TIPO_DOC_VISIBILI = new List<int>();

                    // reperisco tutti i XR_DEM_TIPIDOC_COMPORTAMENTO visibili alla matricola corrente
                    var _tempTIPIDOC_COMPORTAMENTO = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => (
                            (w.MatricoleAbilitate.Contains(matr) && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matr))) ||
                            (w.MatricoleAbilitate.Contains(matr) && String.IsNullOrEmpty(w.MatricoleDisabilitate)) ||
                            (w.MatricoleAbilitate.Contains("*") && (!String.IsNullOrEmpty(w.MatricoleDisabilitate) && !w.MatricoleDisabilitate.Contains(matr))) ||
                            (w.MatricoleAbilitate.Contains("*") && String.IsNullOrEmpty(w.MatricoleDisabilitate))
                    )).ToList();

                    if (_tempTIPIDOC_COMPORTAMENTO != null && _tempTIPIDOC_COMPORTAMENTO.Any())
                    {
                        TIPI_DOCUMENTO_VISIBILI.AddRange(_tempTIPIDOC_COMPORTAMENTO.Select(w => w.Codice_Tipo_Documento).ToList());

                        // a questo punto vanno recuperati gli dei tipi doc visibili
                        ID_TIPO_DOC_VISIBILI = db.XR_DEM_TIPI_DOCUMENTO.Where(w => TIPI_DOCUMENTO_VISIBILI.Contains(w.Codice)).Select(w => w.Id).ToList();
                    }
                    else
                    {
                        throw new Exception("L'Utente non ha visibilità su alcuna tipologia");
                    }


                    var items = db.XR_DEM_DOCUMENTI.Where(w => TIPOLOGIE_DOCUMENTALI_VISIBILI.Contains(w.Cod_Tipologia_Documentale) && ID_TIPO_DOC_VISIBILI.Contains(w.Id_Tipo_Doc)).Select(k => k.XR_DEM_TIPI_DOCUMENTO).ToList();

                    //var items = db.XR_DEM_DOCUMENTI.Select(w => w.Id_Tipo_Doc).Distinct().ToList();

                    //if (items != null && items.Any())
                    //{
                    //var lista = db.XR_DEM_TIPI_DOCUMENTO.Where(w => items.Contains(w.Id)).ToList();

                    if (items != null && items.Any())
                    {
                        items = items.DistinctBy(x => x.Descrizione).ToList();

                        result.AddRange(items.Select(x => new SelectListItem()
                        {
                            Value = x.Id.ToString(),
                            Text = x.Descrizione,
                            Selected = (id == null ? false : (x.Id == id))
                        }));
                    }
                    //}         
                }
            }
            return result;
        }

        public static List<SelectListItem> GetStatiRichiesta(string id = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var db = AnagraficaManager.GetDb();

            result.Add(new SelectListItem()
            {
                Value = "-1",
                Text = "Seleziona un valore"
            });

            List<string> tipoUtente = new List<string>();

            if (DematerializzazioneManager.IsOperatore())
            {
                tipoUtente.Add("Operatore");
            }

            if (DematerializzazioneManager.IsVisionatore())
            {
                tipoUtente.Add("Visionatore");
            }

            if (DematerializzazioneManager.IsApprovatore())
            {
                tipoUtente.Add("Approvatore");
            }

            foreach (StatiDematerializzazioneDocumenti e in Enum.GetValues(typeof(StatiDematerializzazioneDocumenti)))
            {
                if (!String.IsNullOrEmpty(e.GetVisibilitaValue()))
                {
                    bool b = tipoUtente.Any(s => e.GetVisibilitaValue().Contains(s));

                    if (b)
                    {
                        result.Add(new SelectListItem()
                        {
                            Value = ((int)e).ToString(),
                            Text = e.GetAmbientValue(),
                            Selected = (id == null ? false : id == ((int)e).ToString())
                        });
                    }
                }
            }

            return result;
        }

        [HttpGet]
        public ActionResult PortaPraticaStatoPrecedente(int idDoc)
        {
            Dematerializzazione_EsitoAjax response = new Dematerializzazione_EsitoAjax();
            IncentiviEntities db = new IncentiviEntities();
            XR_DEM_DOCUMENTI doc = null;
            int statoPrecedente = 0;
            int idRich = 0;

            try
            {
                // reperimento del documento da clonare
                doc = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();
                statoPrecedente = doc.Id_Stato;

                if (doc == null)
                {
                    throw new Exception("Documento non trovato");
                }

                idRich = doc.Id_Richiesta.GetValueOrDefault();

                Dematerializzazione_EsitoAjax puoModificare = VerificaPossibilitaDiRipristinoStatoPratica(idDoc, idRich);

                if (!puoModificare.Esito)
                {
                    throw new Exception(puoModificare.ErrorMessage);
                }

                doc.Id_Stato = DematerializzazioneManager.GetPrevIdStato(doc.Id_Stato, doc.Id_WKF_Tipologia, true);

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = Request.UserHostAddress,
                    COD_USER = UtenteHelper.Matricola(),
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = doc.Id,
                    ID_PERSONA = CommonHelper.GetCurrentIdPersona(),
                    ID_STATO = doc.Id_Stato,
                    ID_TIPOLOGIA = doc.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = DateTime.Now,
                    TMS_TIMESTAMP = DateTime.Now
                });

                doc.Id_Richiesta = null;
                db.SaveChanges();

                string codEcc = "";
                string provenienza = "";

                if (idRich > 0)
                {
                    try
                    {
                        var richiesta = db.XR_MAT_RICHIESTE.Where(w => w.ID.Equals(idRich)).FirstOrDefault();

                        if (richiesta == null)
                        {
                            throw new Exception("Richiesta non trovata");
                        }

                        digiGappEntities dbGapp = new digiGappEntities();
                        codEcc = richiesta.ECCEZIONE.Trim();
                        provenienza = "ID_MAT_RICHIESTA=" + idRich;
                        var toRemove = dbGapp.MyRai_PianoFerieBatch.Where(w => w.codice_eccezione.Equals(codEcc) &&
                                                                            w.provenienza.Contains(provenienza) &&
                                                                            w.id_richiesta_db == null).Select(w => w.id).ToList();

                        string esito = MaternitaCongediManager.cprat(idRich);

                        if (!String.IsNullOrEmpty(esito))
                        {
                            throw new Exception(esito);
                        }

                        if (toRemove != null && toRemove.Any())
                        {
                            dbGapp.MyRai_PianoFerieBatch.RemoveWhere(w => toRemove.Contains(w.id));
                            dbGapp.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        doc.Id_Stato = statoPrecedente;
                        doc.Id_Richiesta = idRich;

                        db.SaveChanges();

                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    digiGappEntities dbGapp = new digiGappEntities();
                    List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);

                    var tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                    if (tt != null)
                    {
                        codEcc = tt.Valore;
                    }
                    if (String.IsNullOrEmpty(codEcc))
                    {
                        throw new Exception("Eccezione non trovata");
                    }

                    provenienza = "ID_DOC=" + idDoc;
                    var toRemove = dbGapp.MyRai_PianoFerieBatch.Where(w => w.codice_eccezione.Equals(codEcc) &&
                                                                        w.provenienza.Contains(provenienza) &&
                                                                        w.id_richiesta_db == null).Select(w => w.id).ToList();

                    if (toRemove != null && toRemove.Any())
                    {
                        dbGapp.MyRai_PianoFerieBatch.RemoveWhere(w => toRemove.Contains(w.id));
                        dbGapp.SaveChanges();
                    }
                }

                response.Esito = true;
                response.ErrorMessage = String.Empty;
                response.Id = doc.Id;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.ErrorMessage = ex.Message;
                response.Id = 0;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private Dematerializzazione_EsitoAjax VerificaPossibilitaDiRipristinoStatoPratica(int idDoc, int idRich)
        {
            Dematerializzazione_EsitoAjax response = new Dematerializzazione_EsitoAjax();

            if (idRich == 0)
            {
                response.Esito = true;
                return response;
            }

            digiGappEntities dbGapp = new digiGappEntities();
            IncentiviEntities db = new IncentiviEntities();

            try
            {
                string codEcc = "";
                string provenienza = "";

                if (idRich != 0)
                {
                    provenienza = "ID_MAT_RICHIESTA=" + idRich;
                    var richiesta = db.XR_MAT_RICHIESTE.Where(w => w.ID.Equals(idRich)).FirstOrDefault();

                    if (richiesta == null)
                    {
                        throw new Exception("Richiesta non trovata");
                    }
                    codEcc = richiesta.ECCEZIONE.Trim();
                }
                else
                {
                    provenienza = "ID_DOC=" + idDoc;
                    var d = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                    if (d == null)
                    {
                        throw new Exception("Pratica non trovata");
                    }

                    List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(d.CustomDataJSON);
                    var tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                    if (tt != null)
                    {
                        codEcc = tt.Valore;
                    }
                    if (String.IsNullOrEmpty(codEcc))
                    {
                        throw new Exception("Eccezione non trovata");
                    }
                }

                response.Esito = dbGapp.MyRai_PianoFerieBatch.Count(w => w.codice_eccezione.Equals(codEcc) &&
                                                                    w.provenienza.Contains(provenienza) &&
                                                                    w.id_richiesta_db != null && w.id_richiesta_db > 0) == 0;

                if (!response.Esito)
                {
                    response.ErrorMessage = "L'eccezione è stata già inserita in gapp, non è possibile portare indietro la pratica.";
                }
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        private class ListaTipiDocRaggruppati
        {
            public string NomeTipologiaDocumentale { get; set; }
            public List<int> ListaId { get; set; }

            public ListaTipiDocRaggruppati()
            {
                ListaId = new List<int>();
            }
        }
        public class Risultati
        {
            public string Matricola { get; set; }
            public string Nominativo { get; set; }
            public string DataDal { get; set; }
            public string DataAl { get; set; }
            public int IdRichiesta { get; set; }
            public string Eccezione { get; set; }
            public int Giorni { get; set; }
        }


        //public ActionResult CaricaGiaApprovatiSenzaRichiesta()
        //{
        //    int pratiche = 0;
        //    List<Risultati> risultati = new List<Risultati>();
        //    try
        //    {
        //        List<XR_DEM_DOCUMENTI> docs = new List<XR_DEM_DOCUMENTI>();
        //        IncentiviEntities db = new IncentiviEntities();
        //        digiGappEntities dbDigiGapp = new digiGappEntities();
        //        docs = db.XR_DEM_DOCUMENTI.Where(w => (w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Accettato ||
        //        w.Id_Stato == (int)StatiDematerializzazioneDocumenti.PresaInCarico) &&
        //        w.Id_Richiesta == null && w.CustomDataJSON != null).ToList();

        //        XR_MAT_RICHIESTE R = new XR_MAT_RICHIESTE();

        //        var eccezioniDaInserireConAzioneAutomatica = dbDigiGapp.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "EccezioniDaInserireConAzioneAutomatica");

        //        if (eccezioniDaInserireConAzioneAutomatica == null)
        //        {
        //            throw new Exception("Eccezioni consentite non trovate");
        //        }

        //        if (docs != null && docs.Any())
        //        {
        //            pratiche = docs.Count();
        //            foreach (var d in docs)
        //            {
        //                int myID = d.Id;
        //                string formaStringa = String.Format("HRIS/Dematerializzazione-DA_PIANOFERIE=FALSE-APPROVATORE_UFFPERS-ID_DOC={0}", myID);
        //                bool giaInserito = dbDigiGapp.MyRai_PianoFerieBatch.Count(w => w.provenienza.Contains(formaStringa)) > 0;

        //                if (giaInserito)
        //                {
        //                    continue;
        //                }
        //                else
        //                {
        //                    string codEccezione = "";
        //                    List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(d.CustomDataJSON);
        //                    var tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
        //                    if (tt != null)
        //                    {
        //                        codEccezione = tt.Valore;
        //                    }

        //                    R = PreparaOggettoXR_MAT_RICHIESTE(d);

        //                    R.ECCEZIONE = codEccezione;
        //                    R.ID = 0;

        //                    if (R.PIANIFICAZIONE_BASE_ORARIA.HasValue && !R.PIANIFICAZIONE_BASE_ORARIA.GetValueOrDefault())
        //                    {
        //                        R.PIANIFICAZIONE_BASE_ORARIA = null;
        //                    }

        //                    if (R.ECCEZIONE != "MT" && R.DATA_INIZIO_MATERNITA.HasValue)
        //                    {
        //                        R.INIZIO_GIUSTIFICATIVO = R.DATA_INIZIO_MATERNITA;
        //                        R.DATA_INIZIO_MATERNITA = null;
        //                    }

        //                    if (R.ECCEZIONE != "MT" && R.DATA_FINE_MATERNITA.HasValue)
        //                    {
        //                        R.FINE_GIUSTIFICATIVO = R.DATA_FINE_MATERNITA;
        //                        R.DATA_FINE_MATERNITA = null;
        //                    }

        //                    if (R != null)
        //                    {
        //                        var anag = BatchManager.GetUserData(R.MATRICOLA);
        //                        // se l'eccezione è una di quelle per le quali vanno fatti gli inserimenti in gapp
        //                        // R.PIANIFICAZIONE_BASE_ORARIA == null significa che è una eccezioni per intera giornata
        //                        if (!String.IsNullOrEmpty(eccezioniDaInserireConAzioneAutomatica.Valore1) &&
        //                            eccezioniDaInserireConAzioneAutomatica.Valore1.Split(',').Contains(R.ECCEZIONE) &&
        //                            R.PIANIFICAZIONE_BASE_ORARIA == null)
        //                        {
        //                            DateTime inizioGiustificativo = R.INIZIO_GIUSTIFICATIVO.HasValue ? R.INIZIO_GIUSTIFICATIVO.GetValueOrDefault() : R.DATA_INIZIO_MATERNITA.GetValueOrDefault();
        //                            DateTime fineGiustificativo = R.FINE_GIUSTIFICATIVO.HasValue ? R.FINE_GIUSTIFICATIVO.GetValueOrDefault() : R.DATA_FINE_MATERNITA.GetValueOrDefault();

        //                            List<Giornata> giorni = new List<Giornata>();
        //                            // per il periodo Inizio - Fine giustificativo deve inserire una voce nella tabella myrai_pianoFerieBatch
        //                            // in questo modo il batch prenderà in carico le richieste di inserimento eccezione 
        //                            var pf = FeriePermessiManager.GetPianoFerieAnno(inizioGiustificativo.Year, true, R.MATRICOLA, false);
        //                            if (pf.esito)
        //                            {
        //                                if (pf.dipendente != null && pf.dipendente.ferie != null
        //                                    && pf.dipendente.ferie.giornate != null && pf.dipendente.ferie.giornate.Any())
        //                                {
        //                                    giorni.AddRange(pf.dipendente.ferie.giornate.ToList());

        //                                    foreach (var g in giorni)
        //                                    {
        //                                        string sGiorno = g.dataTeorica.Substring(0, 2);
        //                                        string sMese = g.dataTeorica.Substring(3, 2);
        //                                        int anno = inizioGiustificativo.Year;
        //                                        int giorno = int.Parse(sGiorno);
        //                                        int mese = int.Parse(sMese);
        //                                        DateTime nuovaData = DateTime.MinValue;
        //                                        try
        //                                        {
        //                                            nuovaData = new DateTime(anno, mese, giorno);
        //                                        }
        //                                        catch (Exception ex)
        //                                        {

        //                                        }

        //                                        g.data = nuovaData;
        //                                    }

        //                                    if (inizioGiustificativo.Year != fineGiustificativo.Year)
        //                                    {
        //                                        // se son diversi vuol dire che è un permesso a cavallo di 2 anni esempio un permesso che inizia a dicembre 2021 e termina a gennaio 2022
        //                                        pf = FeriePermessiManager.GetPianoFerieAnno(fineGiustificativo.Year, true, R.MATRICOLA, false);
        //                                        if (pf.esito)
        //                                        {
        //                                            if (pf.dipendente != null && pf.dipendente.ferie != null
        //                                                && pf.dipendente.ferie.giornate != null && pf.dipendente.ferie.giornate.Any())
        //                                            {
        //                                                foreach (var g in pf.dipendente.ferie.giornate.ToList())
        //                                                {
        //                                                    string sGiorno = g.dataTeorica.Substring(0, 2);
        //                                                    string sMese = g.dataTeorica.Substring(3, 2);
        //                                                    int anno = fineGiustificativo.Year;
        //                                                    int giorno = int.Parse(sGiorno);
        //                                                    int mese = int.Parse(sMese);

        //                                                    DateTime nuovaData = DateTime.MinValue;
        //                                                    try
        //                                                    {
        //                                                        nuovaData = new DateTime(anno, mese, giorno);
        //                                                    }
        //                                                    catch (Exception ex)
        //                                                    {

        //                                                    }

        //                                                    g.data = nuovaData;
        //                                                    giorni.Add(g);
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                                // a questo punto in giorni ci saranno tutte le giornate degli anni interessati
        //                                // prende solo le giornate incluse nel range della richiesta
        //                                string eccezioniAccettano9596 = eccezioniDaInserireConAzioneAutomatica.Valore2;

        //                                if (!String.IsNullOrEmpty(eccezioniAccettano9596) &&
        //                                    eccezioniAccettano9596.Split(',').Contains(R.ECCEZIONE))
        //                                {
        //                                    giorni = giorni.Where(w => w.data >= inizioGiustificativo && w.data <= fineGiustificativo).ToList();
        //                                }
        //                                else
        //                                {
        //                                    // in questo caso l'eccezione va inserita solo per le giornate il cui orario reale sia diverso da 
        //                                    // 95 e 96
        //                                    giorni = giorni.Where(w => w.data >= inizioGiustificativo && w.data <= fineGiustificativo &&
        //                                                        !String.IsNullOrWhiteSpace(w.orarioReale) &&
        //                                                        !w.orarioReale.StartsWith("9")).ToList();
        //                                }

        //                                int days = (int)(giorni.Last().data - giorni.First().data).TotalDays;

        //                                risultati.Add(new Risultati()
        //                                {
        //                                    Matricola = d.MatricolaDestinatario,
        //                                    Nominativo = anag.nominativo,
        //                                    IdRichiesta = R.ID,
        //                                    Eccezione = R.ECCEZIONE,
        //                                    Giorni = giorni.Count,
        //                                    DataDal = giorni.First().data.ToString("dd/MM/yyyy"),
        //                                    DataAl = giorni.Last().data.ToString("dd/MM/yyyy")
        //                                });

        //                                foreach (var g in giorni)
        //                                {
        //                                    MyRai_PianoFerieBatch p = new MyRai_PianoFerieBatch()
        //                                    {
        //                                        codice_eccezione = R.ECCEZIONE.ToUpper(),
        //                                        provenienza = "HRIS/Dematerializzazione-DA_PIANOFERIE=FALSE-APPROVATORE_UFFPERS-ID_DOC=" + d.Id,
        //                                        dalle = "",
        //                                        alle = "",
        //                                        importo = "",
        //                                        data_eccezione = g.data,
        //                                        matricola = R.MATRICOLA,
        //                                        data_creazione_record = DateTime.Now,
        //                                        quantita = "1",
        //                                        sedegapp = anag.sede_gapp
        //                                    };
        //                                    dbDigiGapp.MyRai_PianoFerieBatch.Add(p);
        //                                }

        //                                dbDigiGapp.SaveChanges();
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Nessun documento trovato");
        //        }
        //        int giorniTot = 0;

        //        giorniTot = risultati.Sum(w => w.Giorni);

        //        return new JsonResult
        //        {
        //            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //            Data = new { esito = true, Pratiche = pratiche, GiorniTotali = giorniTot, Riepilogo = risultati }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new JsonResult
        //        {
        //            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //            Data = new { esito = false, descrizione = ex.Message }
        //        };
        //    }
        //}

        //public ActionResult CaricaGiaApprovatiConRichiesta()
        //{
        //    int pratiche = 0;
        //    List<Risultati> risultati = new List<Risultati>();
        //    try
        //    {
        //        List<XR_DEM_DOCUMENTI> docs = new List<XR_DEM_DOCUMENTI>();
        //        IncentiviEntities db = new IncentiviEntities();
        //        digiGappEntities dbDigiGapp = new digiGappEntities();
        //        docs = db.XR_DEM_DOCUMENTI.Where(w => (w.Id_Stato == (int)StatiDematerializzazioneDocumenti.Accettato ||
        //        w.Id_Stato == (int)StatiDematerializzazioneDocumenti.PresaInCarico) &&
        //        w.Id_Richiesta != null).ToList();

        //        XR_MAT_RICHIESTE R = new XR_MAT_RICHIESTE();

        //        var eccezioniDaInserireConAzioneAutomatica = dbDigiGapp.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "EccezioniDaInserireConAzioneAutomatica");

        //        if (eccezioniDaInserireConAzioneAutomatica == null)
        //        {
        //            throw new Exception("Eccezioni consentite non trovate");
        //        }

        //        if (docs != null && docs.Any())
        //        {
        //            pratiche = docs.Count();
        //            foreach (var d in docs)
        //            {
        //                int myrichiestaID = d.Id_Richiesta.Value;
        //                string formaStringa = String.Format("HRIS/Dematerializzazione-DA_PIANOFERIE=FALSE-APPROVATORE_UFFPERS-ID_MAT_RICHIESTA={0}", myrichiestaID);
        //                bool giaInserito = dbDigiGapp.MyRai_PianoFerieBatch.Count(w => w.provenienza.Contains(formaStringa)) > 0;

        //                if (giaInserito)
        //                {
        //                    continue;
        //                }
        //                else
        //                {
        //                    R = db.XR_MAT_RICHIESTE.Where(w => w.ID.Equals(d.Id_Richiesta.Value)).FirstOrDefault();

        //                    if (R != null)
        //                    {
        //                        var anag = BatchManager.GetUserData(R.MATRICOLA);
        //                        // se l'eccezione è una di quelle per le quali vanno fatti gli inserimenti in gapp
        //                        // R.PIANIFICAZIONE_BASE_ORARIA == null significa che è una eccezioni per intera giornata
        //                        if (!String.IsNullOrEmpty(eccezioniDaInserireConAzioneAutomatica.Valore1) &&
        //                            eccezioniDaInserireConAzioneAutomatica.Valore1.Split(',').Contains(R.ECCEZIONE) &&
        //                            R.PIANIFICAZIONE_BASE_ORARIA == null)
        //                        {
        //                            DateTime inizioGiustificativo = R.INIZIO_GIUSTIFICATIVO.HasValue ? R.INIZIO_GIUSTIFICATIVO.GetValueOrDefault() : R.DATA_INIZIO_MATERNITA.GetValueOrDefault();
        //                            DateTime fineGiustificativo = R.FINE_GIUSTIFICATIVO.HasValue ? R.FINE_GIUSTIFICATIVO.GetValueOrDefault() : R.DATA_FINE_MATERNITA.GetValueOrDefault();

        //                            List<Giornata> giorni = new List<Giornata>();
        //                            // per il periodo Inizio - Fine giustificativo deve inserire una voce nella tabella myrai_pianoFerieBatch
        //                            // in questo modo il batch prenderà in carico le richieste di inserimento eccezione 
        //                            var pf = FeriePermessiManager.GetPianoFerieAnno(inizioGiustificativo.Year, true, R.MATRICOLA, false);
        //                            if (pf.esito)
        //                            {
        //                                if (pf.dipendente != null && pf.dipendente.ferie != null
        //                                    && pf.dipendente.ferie.giornate != null && pf.dipendente.ferie.giornate.Any())
        //                                {
        //                                    giorni.AddRange(pf.dipendente.ferie.giornate.ToList());

        //                                    foreach (var g in giorni)
        //                                    {
        //                                        string sGiorno = g.dataTeorica.Substring(0, 2);
        //                                        string sMese = g.dataTeorica.Substring(3, 2);
        //                                        int anno = inizioGiustificativo.Year;
        //                                        int giorno = int.Parse(sGiorno);
        //                                        int mese = int.Parse(sMese);
        //                                        DateTime nuovaData = DateTime.MinValue;
        //                                        try
        //                                        {
        //                                            nuovaData = new DateTime(anno, mese, giorno);
        //                                        }
        //                                        catch (Exception ex)
        //                                        {

        //                                        }

        //                                        g.data = nuovaData;
        //                                    }

        //                                    if (inizioGiustificativo.Year != fineGiustificativo.Year)
        //                                    {
        //                                        // se son diversi vuol dire che è un permesso a cavallo di 2 anni esempio un permesso che inizia a dicembre 2021 e termina a gennaio 2022
        //                                        pf = FeriePermessiManager.GetPianoFerieAnno(fineGiustificativo.Year, true, R.MATRICOLA, false);
        //                                        if (pf.esito)
        //                                        {
        //                                            if (pf.dipendente != null && pf.dipendente.ferie != null
        //                                                && pf.dipendente.ferie.giornate != null && pf.dipendente.ferie.giornate.Any())
        //                                            {
        //                                                foreach (var g in pf.dipendente.ferie.giornate.ToList())
        //                                                {
        //                                                    string sGiorno = g.dataTeorica.Substring(0, 2);
        //                                                    string sMese = g.dataTeorica.Substring(3, 2);
        //                                                    int anno = fineGiustificativo.Year;
        //                                                    int giorno = int.Parse(sGiorno);
        //                                                    int mese = int.Parse(sMese);

        //                                                    DateTime nuovaData = DateTime.MinValue;
        //                                                    try
        //                                                    {
        //                                                        nuovaData = new DateTime(anno, mese, giorno);
        //                                                    }
        //                                                    catch (Exception ex)
        //                                                    {

        //                                                    }

        //                                                    g.data = nuovaData;
        //                                                    giorni.Add(g);
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                                // a questo punto in giorni ci saranno tutte le giornate degli anni interessati
        //                                // prende solo le giornate incluse nel range della richiesta
        //                                string eccezioniAccettano9596 = eccezioniDaInserireConAzioneAutomatica.Valore2;

        //                                if (!String.IsNullOrEmpty(eccezioniAccettano9596) &&
        //                                    eccezioniAccettano9596.Split(',').Contains(R.ECCEZIONE))
        //                                {
        //                                    giorni = giorni.Where(w => w.data >= inizioGiustificativo && w.data <= fineGiustificativo).ToList();
        //                                }
        //                                else
        //                                {
        //                                    // in questo caso l'eccezione va inserita solo per le giornate il cui orario reale sia diverso da 
        //                                    // 95 e 96
        //                                    giorni = giorni.Where(w => w.data >= inizioGiustificativo && w.data <= fineGiustificativo &&
        //                                                        !String.IsNullOrWhiteSpace(w.orarioReale) &&
        //                                                        !w.orarioReale.StartsWith("9")).ToList();
        //                                }

        //                                int days = (int)(giorni.Last().data - giorni.First().data).TotalDays;

        //                                risultati.Add(new Risultati()
        //                                {
        //                                    Matricola = d.MatricolaDestinatario,
        //                                    Nominativo = anag.nominativo,
        //                                    IdRichiesta = R.ID,
        //                                    Eccezione = R.ECCEZIONE,
        //                                    Giorni = giorni.Count,
        //                                    DataDal = giorni.First().data.ToString("dd/MM/yyyy"),
        //                                    DataAl = giorni.Last().data.ToString("dd/MM/yyyy")
        //                                });

        //                                foreach (var g in giorni)
        //                                {
        //                                    MyRai_PianoFerieBatch p = new MyRai_PianoFerieBatch()
        //                                    {
        //                                        codice_eccezione = R.ECCEZIONE.ToUpper(),
        //                                        provenienza = "HRIS/Dematerializzazione-DA_PIANOFERIE=FALSE-APPROVATORE_UFFPERS-ID_MAT_RICHIESTA=" + R.ID,
        //                                        dalle = "",
        //                                        alle = "",
        //                                        importo = "",
        //                                        data_eccezione = g.data,
        //                                        matricola = R.MATRICOLA,
        //                                        data_creazione_record = DateTime.Now,
        //                                        quantita = "1",
        //                                        sedegapp = anag.sede_gapp
        //                                    };
        //                                    dbDigiGapp.MyRai_PianoFerieBatch.Add(p);
        //                                }

        //                                dbDigiGapp.SaveChanges();
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Nessun documento trovato");
        //        }
        //        int giorniTot = 0;

        //        giorniTot = risultati.Sum(w => w.Giorni);

        //        return new JsonResult
        //        {
        //            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //            Data = new { esito = true, Pratiche = pratiche, GiorniTotali = giorniTot, Riepilogo = risultati }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new JsonResult
        //        {
        //            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //            Data = new { esito = false, descrizione = ex.Message }
        //        };
        //    }
        //}
        #endregion

        private ReportMyRaiPianoFerieBatch GetStatoInserimenti(int idDoc)
        {
            ReportMyRaiPianoFerieBatch result = new ReportMyRaiPianoFerieBatch();
            digiGappEntities dbDigiGapp = new digiGappEntities();
            IncentiviEntities db = new IncentiviEntities();
            result.StatoInserimento = StatoInserimentoMyRaiPianoFerieBatchEnum.DaCaricareInTabella;
            result.Giorni = new List<EsitoInserimentoMyRaiPianoFerieBatch>();
            result.Esito = true;
            result.ErrorMessage = String.Empty;
            result.Id = idDoc;

            try
            {
                var eccezioniDaInserireConAzioneAutomatica = dbDigiGapp.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "EccezioniDaInserireConAzioneAutomatica");

                if (eccezioniDaInserireConAzioneAutomatica == null)
                {
                    throw new Exception("Eccezioni consentite non trovate");
                }
                int idRich = 0;

                var item = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (item == null)
                {
                    throw new Exception("Pratica non trovata");
                }

                if (item.Id_Richiesta.HasValue)
                {
                    idRich = item.Id_Richiesta.GetValueOrDefault();
                }
                else
                {
                    throw new Exception("Impossibile reperire l'identificativo della richiesta");
                }

                var richiestaMat = db.XR_MAT_RICHIESTE.Where(w => w.ID.Equals(idRich)).FirstOrDefault();

                if (richiestaMat == null)
                {
                    throw new Exception("Pratica non trovata");
                }

                string eccezione = richiestaMat.ECCEZIONE;
                string stringaDaCercareSuPianoFerieBatch = String.Format("HRIS/Dematerializzazione-DA_PIANOFERIE=FALSE-ID_MAT_RICHIESTA={0}", idRich);

                var giorni = dbDigiGapp.MyRai_PianoFerieBatch.Where(w => w.provenienza.Contains(stringaDaCercareSuPianoFerieBatch)).ToList();

                if (giorni == null || !giorni.Any())
                {
                    result.Esito = true;
                    result.StatoInserimento = StatoInserimentoMyRaiPianoFerieBatchEnum.DaCaricareInTabella;
                }
                else
                {
                    bool almenoUnoAncoraDaInserire = giorni.Count(w => w.id_richiesta_db == null && w.data_ultimo_tentativo == null) == 0;
                    bool almenoUnoLavorato = giorni.Count(w => w.data_ultimo_tentativo != null) > 0;
                    bool tuttiLavorati = giorni.Count(w => w.data_ultimo_tentativo != null) == giorni.Count();

                    if (tuttiLavorati)
                    {
                        result.Esito = true;
                        result.StatoInserimento = StatoInserimentoMyRaiPianoFerieBatchEnum.Lavorati;
                    }
                    else if (almenoUnoAncoraDaInserire && almenoUnoLavorato)
                    {
                        result.Esito = true;
                        result.StatoInserimento = StatoInserimentoMyRaiPianoFerieBatchEnum.InLavorazione;
                    }
                    else if (almenoUnoAncoraDaInserire && !almenoUnoLavorato)
                    {
                        result.Esito = true;
                        result.StatoInserimento = StatoInserimentoMyRaiPianoFerieBatchEnum.DaLavorare;
                    }

                    foreach (var g in giorni)
                    {
                        result.Giorni.Add(new EsitoInserimentoMyRaiPianoFerieBatch()
                        {
                            Id = g.id,
                            IdRichiestaDB = g.id_richiesta_db,
                            NumeroDocumento = g.ndoc_gapp,
                            DataUltimoTentativo = g.data_ultimo_tentativo,
                            Errore = g.error
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.ErrorMessage = ex.Message;
                result.Giorni = new List<EsitoInserimentoMyRaiPianoFerieBatch>();
            }

            return result;
        }

        [HttpGet]
        public ActionResult ClonaElementoCustom(string matricola, string idToClone)
        {
            DematerializzazioneCustomDataView model = new DematerializzazioneCustomDataView();
            try
            {
                string idParent = idToClone;
                if (idToClone.StartsWith("row_"))
                {
                    idToClone = idToClone.Replace("row_", "");
                }

                string matSession = UtenteHelper.Matricola();

                var db = AnagraficaManager.GetDb();

                if (String.IsNullOrEmpty(matricola))
                {
                    matricola = UtenteHelper.Matricola();
                }

                string tipologiaDocumentale = (string)SessionHelper.Get(matSession + "tipologiaDocumentale");
                string tipologiaDocumento = (string)SessionHelper.Get(matSession + "tipologiaDocumento");

                var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
    w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                if (comportamento == null)
                {
                    throw new Exception("Comportamento non trovato");
                }

                List<AttributiAggiuntivi> objD = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(comportamento.CustomDataJSON);
                model.Attributi = objD;

                DatiAggiuntivi rispostaDIP = null;

                using (var sediDB = new PERSEOEntities())
                {
                    string queryDIP = "SELECT t0.[matricola_dp] as Matricola " +
                                        ",t12.[cod_sede] + ' ' + t12.[desc_sede] as SedeGapp " +
                                        ",substring(t1.[CODICINI], 3, 1) as AssicurazioneIinfortuni " +
                                        ",t2.[cod_mansione] + ' ' + t2.[desc_mansione] as Mansione " +
                                        ",t0.[sezione] as Sezione " +
                                        ",t0.[cod_serv_cont] + t0.[cod_serv_inquadram] + ' ' + t10.[desc_breve] as Servizio " +
                                        ",t11.[cod_categoria] + ' / ' + t0.[tipo_minimo] + ' ' + t3.desc_livello as Categoria " +
                                        ",t0.forma_contratto as FormaContratto " +
                                        ",t1.[DATA_ANZIANITA_CATEGORIA] as AnzianitaCategoria " +
                                        //"--,t2.[desc_mansione] " +
                                        //"--,t3.[tipo_minimo] " +
                                        //"--,t10.[cod_serv_cont] " +
                                        //"--,t10.[desc_serv_cont] " +
                                        //"--,t10.[societa] " +
                                        //"--,t11.[desc_macro_categoria] " +
                                        //"--,t11.[cod_categoria] " +
                                        //"--,t11.[desc_categoria] " +
                                        //"--,t3.desc_livello " +
                                        //"--,t11.[desc_liv_professionale] " +
                                        //"--,t11.[ccl] " +
                                        //"--,t12.[desc_aggregato_sede] " +
                                        //"--,t12.[cod_sede] " +
                                        //"--,t12.[desc_sede] " +
                                        //"--,t13.cod_insediamento " +
                                        //"--,t13.desc_insediamento " +
                                        //"--,t0.sezione " +
                                        //"--,t0.forma_contratto " +
                                        //"--,t14.[DATA_ANZ_CATEGORIA] " +
                                        "FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                                        "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON(t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                                        "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_MANSIONE] " +
                                        "        t2 on(t2.[sky_mansione] = t1.[sky_mansione]) " +
                                        "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] " +
                                        "        t3 ON(t3.[sky_livello_categ] = t1.[sky_livello_categ]) " +
                                        "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SERVIZIO_CONTABILE] " +
                                        "        t10 on(t1.sky_servizio_contabile= t10.sky_serv_cont) " +
                                        "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA] " +
                                        "        t11 on(t1.sky_categoria = t11.sky_categoria) " +
                                        "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SEDE] " +
                                        "        t12 on(t1.sky_sede = t12.sky_sede) " +
                                        "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_INSEDIAMENTO] " +
                                        "        t13 on(t0.cod_insediamento_ubicazione = t13.cod_insediamento) " +
                                        "INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CONTRATTO_UNICO] " +
                                        "        t14 ON(t1.[SKY_CONTRATTO] = t14.[SKY_CONTRATTO]) " +
                                        "where " +
                                        "t0.matricola_dp='##MATRICOLA##' and t1.[flg_ultimo_record]='$' ";

                    queryDIP = queryDIP.Replace("##MATRICOLA##", matricola);

                    rispostaDIP = sediDB.Database.SqlQuery<DatiAggiuntivi>(queryDIP).FirstOrDefault();
                }

                foreach (var attr in model.Attributi)
                {
                    if (!String.IsNullOrEmpty(attr.TagHRDW))
                    {
                        var val = rispostaDIP.GetType().GetProperty(attr.TagHRDW).GetValue(rispostaDIP, null);

                        if (val != null)
                        {
                            attr.Valore = val.ToString();
                        }
                    }
                }

                if (idToClone.Contains("_GUID"))
                {
                    int pos = idToClone.IndexOf("_GUID");
                    if (pos > -1)
                    {
                        idToClone = idToClone.Substring(0, pos);
                    }
                }

                AttributiAggiuntivi elementoDaClonare = new AttributiAggiuntivi();
                elementoDaClonare = model.Attributi.Where(w => w.Id.Equals(idToClone)).FirstOrDefault();

                if (elementoDaClonare != null)
                {
                    string guid = Guid.NewGuid().ToString("N");
                    string newID = String.Format("{0}_GUID{1}", idToClone, guid);
                    elementoDaClonare.Id = newID;

                    if (elementoDaClonare.InLine != null && elementoDaClonare.InLine.Any())
                    {
                        RiscriviID(elementoDaClonare, guid, idToClone);
                    }

                    if (elementoDaClonare.Buttons != null && elementoDaClonare.Buttons.Any())
                    {
                        RiscriviID(elementoDaClonare, guid, idParent);
                    }

                    model.Attributi.Clear();
                    elementoDaClonare.Visible = true;
                    model.Attributi.Add(elementoDaClonare);
                }

                return View("~/Views/Dematerializzazione/subpartial/_tabCustomDataClone.cshtml", elementoDaClonare);
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        private static void RiscriviID(AttributiAggiuntivi attributo, string guid, string idParent)
        {
            if (!attributo.Visible && attributo.Id.StartsWith("btn_rimuovi"))
            {
                attributo.Visible = true;
            }

            if (attributo.DataAttributeElements != null && attributo.DataAttributeElements.Any())
            {
                var puntamentiDaAggiornare = attributo.DataAttributeElements.Where(w => w.Nome.Equals("rowparent") && w.Valore.Equals(idParent)).ToList();

                if (puntamentiDaAggiornare != null && puntamentiDaAggiornare.Any())
                {
                    foreach (var x in puntamentiDaAggiornare)
                    {
                        x.Valore = String.Format("{0}_GUID{1}", idParent, guid);
                    }
                }
            }

            if (attributo.InLine != null && attributo.InLine.Any())
            {
                foreach (var e in attributo.InLine)
                {
                    e.Id = String.Format("{0}_GUID{1}", e.Id, guid);

                    if (e.InLine != null && e.InLine.Any())
                    {
                        RiscriviID(e, guid, idParent);
                    }

                    if (!e.Visible && e.Id.StartsWith("btn_rimuovi"))
                    {
                        e.Visible = true;
                    }

                    if (e.DataAttributeElements != null && e.DataAttributeElements.Any())
                    {
                        var puntamentiDaAggiornare = e.DataAttributeElements.Where(w => w.Nome.Equals("rowparent") && w.Valore.Equals(idParent)).ToList();

                        if (puntamentiDaAggiornare != null && puntamentiDaAggiornare.Any())
                        {
                            foreach (var x in puntamentiDaAggiornare)
                            {
                                x.Valore = String.Format("{0}_GUID{1}", idParent, guid);
                            }
                        }
                    }
                }
            }

            if (attributo.Buttons != null && attributo.Buttons.Any())
            {
                if (idParent.Contains("_GUID"))
                {
                    int pos = idParent.IndexOf("_GUID");
                    if (pos > -1)
                    {
                        idParent = idParent.Substring(0, pos);
                    }
                }

                // aggiunto per gestire il nuovo attributo Buttons, che lavora similmente all'elemento InLine, ma con una sola profondità
                foreach (var e in attributo.Buttons)
                {
                    e.Id = String.Format("{0}_GUID{1}", e.Id, guid);

                    if (!e.Visible && e.Id.StartsWith("btn_rimuovi"))
                    {
                        e.Visible = true;
                    }

                    if (e.DataAttributeElements != null && e.DataAttributeElements.Any())
                    {
                        if (idParent.StartsWith("row_"))
                        {
                            idParent = idParent.Replace("row_", "");
                        }
                        var puntamentiDaAggiornare = e.DataAttributeElements.Where(w => w.Nome.Equals("rowparent") && w.Valore.Equals("row_" + idParent)).ToList();

                        if (puntamentiDaAggiornare != null && puntamentiDaAggiornare.Any())
                        {
                            foreach (var x in puntamentiDaAggiornare)
                            {
                                x.Valore = String.Format("row_{0}_GUID{1}", idParent, guid);
                            }
                        }
                    }
                }
            }
        }

        private List<XR_MAT_RICHIESTE> PreparaOggettoXR_MAT_RICHIESTE2(XR_DEM_DOCUMENTI doc)
        {
            List<XR_MAT_RICHIESTE> result = new List<XR_MAT_RICHIESTE>();
            List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

            if (!String.IsNullOrEmpty(doc.CustomDataJSON) && doc.CustomDataJSON != "[]")
            {
                List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                DateTime temp;

                string nominativo = DematerializzazioneManager.GetNominativoByMatricola(doc.MatricolaDestinatario);
                List<XR_ALLEGATI> allegati = GetAllegati(doc.Id);
                string idRifMaternita = "";

                var findRiferimentoRichiestaMaternita = objModuloValorizzato.Where(w => w.Id == "RiferimentoRichiestaMaternita").FirstOrDefault();
                if (findRiferimentoRichiestaMaternita != null)
                {
                    idRifMaternita = findRiferimentoRichiestaMaternita.Valore;
                }

                try
                {
                    int max = 0;
                    int count = 0;

                    if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                    {
                        foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
                        {
                            listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
                        }

                        if (listaPiatta != null && listaPiatta.Any())
                        {
                            var ragg = listaPiatta.GroupBy(w => w.DBRefAttribute);
                            max = ragg.Max(w => w.Count());
                        }
                    }

                    do
                    {
                        XR_MAT_RICHIESTE toSend = new XR_MAT_RICHIESTE();

                        // per ogni attributo nell'oggetto XR_MAT_RICHIESTE cerco 
                        // il rispettivo elemento in objModuloValorizzato
                        // c'è un campo in quell'oggetto che definisce a quale attributo di XR_MAT_RICHIESTE
                        // fa riferimento quel valore.
                        foreach (PropertyInfo prop in typeof(XR_MAT_RICHIESTE).GetProperties())
                        {
                            var DBRefAttribute = objModuloValorizzato.Where(w => w.DBRefAttribute == prop.Name).FirstOrDefault();

                            if (DBRefAttribute != null)
                            {
                                // verifica la tipologia dell'elemento selezionato
                                var tipo = DBRefAttribute.Tipo;

                                if (tipo == TipologiaAttributoEnum.Testo || tipo == TipologiaAttributoEnum.FixedHiddenValue)
                                {
                                    PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                                    if (null != propToSet && propToSet.CanWrite)
                                    {
                                        propToSet.SetValue(toSend, DBRefAttribute.Valore, null);
                                    }
                                }

                                if (tipo == TipologiaAttributoEnum.Data)
                                {
                                    DateTime? _data = null;
                                    string dt = DBRefAttribute.Valore;
                                    if (!String.IsNullOrEmpty(dt))
                                    {
                                        if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                            throw new Exception("Errore in conversione " + prop.Name);
                                        _data = temp;
                                    }

                                    PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                                    if (null != propToSet && propToSet.CanWrite)
                                    {
                                        propToSet.SetValue(toSend, _data, null);
                                    }
                                }

                                if (tipo == TipologiaAttributoEnum.Check || tipo == TipologiaAttributoEnum.Radio)
                                {
                                    string _tempId = DBRefAttribute.Id;
                                    string _gruopId = DBRefAttribute.Gruppo;
                                    string valoreSelezionato = "";

                                    var tt = objModuloValorizzato.Where(w => w.Gruppo == _gruopId && w.Checked).FirstOrDefault();
                                    if (tt != null)
                                    {
                                        valoreSelezionato = tt.Valore;
                                    }

                                    PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);

                                    if (!String.IsNullOrEmpty(valoreSelezionato) &&
                                        ((valoreSelezionato.ToUpper() == "TRUE") || (valoreSelezionato.ToUpper() == "FALSE"))
                                        )
                                    {
                                        var tipi = prop.PropertyType.GetGenericArguments();
                                        foreach (var t in tipi)
                                        {
                                            if (t.Name == "bool" || t.Name == "Boolean")
                                            {
                                                bool valore = Boolean.Parse(valoreSelezionato);

                                                if (null != propToSet && propToSet.CanWrite)
                                                {
                                                    propToSet.SetValue(toSend, valore, null);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (null != propToSet && propToSet.CanWrite)
                                        {
                                            var propertyType = prop.PropertyType;
                                            if (System.Nullable.GetUnderlyingType(propertyType) != null && String.IsNullOrEmpty(valoreSelezionato))
                                            {
                                                // It's a nullable type
                                                propToSet.SetValue(toSend, null, null);
                                            }
                                            else
                                            {
                                                // It's not a nullable type
                                                propToSet.SetValue(toSend, valoreSelezionato, null);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        toSend.MATRICOLA = doc.MatricolaDestinatario;
                        toSend.NOMINATIVO = nominativo;
                        toSend.CUSTOM_JSON = doc.CustomDataJSON;

                        if ((String.IsNullOrEmpty(idRifMaternita)) || idRifMaternita == "0")
                        {
                            toSend.ID = 0;
                        }
                        else
                        {
                            int riferimento = 0;
                            bool convertito = int.TryParse(idRifMaternita, out riferimento);
                            if (convertito)
                            {
                                toSend.ID = riferimento;
                            }
                        }

                        if (listaPiatta != null && listaPiatta.Any())
                        {
                            foreach (PropertyInfo prop in typeof(XR_MAT_RICHIESTE).GetProperties())
                            {
                                var DBRefAttribute = listaPiatta.Where(w => w.DBRefAttribute == prop.Name).FirstOrDefault();

                                if (DBRefAttribute != null)
                                {
                                    // verifica la tipologia dell'elemento selezionato
                                    var tipo = DBRefAttribute.Tipo;

                                    if (tipo == TipologiaAttributoEnum.Testo || tipo == TipologiaAttributoEnum.FixedHiddenValue)
                                    {
                                        PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                                        if (null != propToSet && propToSet.CanWrite)
                                        {
                                            propToSet.SetValue(toSend, DBRefAttribute.Valore, null);
                                        }
                                    }

                                    if (tipo == TipologiaAttributoEnum.Data)
                                    {
                                        DateTime? _data = null;
                                        string dt = DBRefAttribute.Valore;
                                        if (!String.IsNullOrEmpty(dt))
                                        {
                                            if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                                throw new Exception("Errore in conversione " + prop.Name);
                                            _data = temp;
                                        }

                                        PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                                        if (null != propToSet && propToSet.CanWrite)
                                        {
                                            propToSet.SetValue(toSend, _data, null);
                                        }
                                    }

                                    if (tipo == TipologiaAttributoEnum.Check || tipo == TipologiaAttributoEnum.Radio)
                                    {
                                        string _tempId = DBRefAttribute.Id;
                                        string _gruopId = DBRefAttribute.Gruppo;
                                        string valoreSelezionato = "";

                                        var tt = listaPiatta.Where(w => w.Gruppo == _gruopId && w.Checked).FirstOrDefault();
                                        if (tt != null)
                                        {
                                            valoreSelezionato = tt.Valore;
                                        }

                                        PropertyInfo propToSet = toSend.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);

                                        if (!String.IsNullOrEmpty(valoreSelezionato) &&
                                            ((valoreSelezionato.ToUpper() == "TRUE") || (valoreSelezionato.ToUpper() == "FALSE"))
                                            )
                                        {
                                            var tipi = prop.PropertyType.GetGenericArguments();
                                            foreach (var t in tipi)
                                            {
                                                if (t.Name == "bool" || t.Name == "Boolean")
                                                {
                                                    bool valore = Boolean.Parse(valoreSelezionato);

                                                    if (null != propToSet && propToSet.CanWrite)
                                                    {
                                                        propToSet.SetValue(toSend, valore, null);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (null != propToSet && propToSet.CanWrite)
                                            {
                                                var propertyType = prop.PropertyType;
                                                if (System.Nullable.GetUnderlyingType(propertyType) != null && String.IsNullOrEmpty(valoreSelezionato))
                                                {
                                                    // It's a nullable type
                                                    propToSet.SetValue(toSend, null, null);
                                                }
                                                else
                                                {
                                                    // It's not a nullable type
                                                    propToSet.SetValue(toSend, valoreSelezionato, null);
                                                }
                                            }
                                        }
                                    }

                                    listaPiatta.Remove(DBRefAttribute);
                                }
                            }
                        }

                        result.Add(toSend);
                        count++;
                    } while (count < max);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return result;
        }

        private static AttributiAggiuntivi FindValorizzatoInObjectByDBRefAttribute(string name, AttributiAggiuntivi objModuloValorizzato)
        {
            AttributiAggiuntivi result = null;
            var itemValorizzato = (objModuloValorizzato.DBRefAttribute.Equals(name) ? objModuloValorizzato : null);
            if (itemValorizzato != null)
            {
                result = new AttributiAggiuntivi();
                result = itemValorizzato;
            }
            else if (objModuloValorizzato.InLine != null && objModuloValorizzato.InLine.Any())
            {
                foreach (var item in objModuloValorizzato.InLine.ToList())
                {
                    itemValorizzato = FindValorizzatoInObjectByDBRefAttribute(name, item);
                    if (itemValorizzato != null)
                    {
                        result = new AttributiAggiuntivi();
                        result = itemValorizzato;
                    }
                }
            }
            return result;
        }

        public ActionResult ScaricaExport()
        {
            try
            {
                FileContentResult result = DematerializzazioneManager.ExportSituazioneDocumentale();

                if (result != null)
                {
                    return result;
                }
                else
                {
                    throw new Exception("Impossibile trovare il file desiderato");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Metodo per filtrare gli elementi nella pagina InArrivo
        /// </summary>
        /// <param name="mese"></param>
        /// <param name="utente">Può contenere la matricola della persona per la quale si intende filtrare oppure il nominativo</param>
        /// <param name="sede"></param>
        /// <param name="tipologia"></param>
        /// <param name="statoRichiesta"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult FiltaDocumentiInArrivo(string mese = null, string utente = null, string sede = null, string tipologia = null, string statoRichiesta = null)
        {
            AnagraficaModel anagraficaModel = null;
            DematerializzazioneDocumentiVM model = new DematerializzazioneDocumentiVM();
            model.Filtri = new Dematerializzazione_FiltriApprovatore();
            model.Documenti = new List<XR_DEM_DOCUMENTI_EXT>();

            if (mese == "")
            {
                mese = null;
            }

            if (utente == "")
            {
                utente = null;
            }

            if (sede == "")
            {
                sede = null;
            }

            if (tipologia == "")
            {
                tipologia = null;
            }

            if (statoRichiesta == "")
            {
                statoRichiesta = null;
            }

            model.DocumentiDaPrendereInCarico = DematerializzazioneManager.GetDocumentiDaPrendereInCarico(mese, utente, sede, tipologia, statoRichiesta);
            model.DocumentiInCaricoAdAltri = DematerializzazioneManager.GetDocumentiInCaricoAltri(UtenteHelper.Matricola());
            model.DocumentiInCaricoAMe = DematerializzazioneManager.GetDocumentiInCaricoAMe(UtenteHelper.Matricola());
            model.IsPreview = false;
            model.PrendiInCaricoEnabled = true;
            model.Matricola = CommonHelper.GetCurrentUserMatricola();

            SezioniAnag sezione = SezioniAnag.NonDefinito;
            anagraficaModel = AnagraficaManager.GetAnagrafica(UtenteHelper.Matricola(), new AnagraficaLoader(sezione));

            if (anagraficaModel != null)
            {
                model.IdPersona = anagraficaModel.IdPersona;
                model.Matricola = anagraficaModel.Matricola;
            }

            model.IsPreview = true;
            return View("~/Views/Dematerializzazione/subpartial/_indexInternal.cshtml", model);
        }

        public static List<SelectListItem> GetSedi(string filter, string value)
        {
            return AnagraficaManager.GetSedi("", "", true, AuthHelper.EnabledSedi(CommonHelper.GetCurrentUserMatricola(), "DEMA"), true, true);
        }

        private XR_WKF_TIPOLOGIA Get_WKF_Tipologia(string codiceWKF_Tipologia)
        {
            XR_WKF_TIPOLOGIA result = null;

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                var item = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(codiceWKF_Tipologia)).FirstOrDefault();
                if (item != null)
                {
                    result = item;
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private XR_WKF_TIPOLOGIA Get_WKF_Tipologia(int idWKF)
        {
            XR_WKF_TIPOLOGIA result = null;

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                var item = db.XR_WKF_TIPOLOGIA.Where(w => w.ID_TIPOLOGIA.Equals(idWKF)).FirstOrDefault();
                if (item != null)
                {
                    result = item;
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        /// <summary>
        /// In alcuni casi ad esempio per i documenti con dati custom
        /// il valore dell'eccezione selezionata potrebbe modificare la 
        /// tipologia di workflow da seguire
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private XR_WKF_TIPOLOGIA Get_WKF_Tipologia(XR_DEM_DOCUMENTI doc)
        {
            XR_WKF_TIPOLOGIA result = null;
            var db = AnagraficaManager.GetDb();

            try
            {

                XR_WKF_TIPOLOGIA WKF_Tipologia = Get_WKF_Tipologia(doc.Id_WKF_Tipologia);
                if (WKF_Tipologia == null)
                {
                    throw new Exception("Workflow non trovato");
                }

                result = WKF_Tipologia;

                // prende il nome della tipologia corrente
                // ad esempio DEMDOC_VSRUO_CPM
                string nomeTipo = WKF_Tipologia.COD_TIPOLOGIA;
                nomeTipo += "_";

                // verifica se esistono tipologie di workflow che hanno come nome che inizia per [nomeTipo_]*
                bool exists = db.XR_WKF_TIPOLOGIA.Count(w => w.COD_TIPOLOGIA.Contains(nomeTipo)) > 0;

                // se ce ne sono allora verifica che il documento abbia il codice eccezione selezionato 
                // e che esista un XR_WKF_TIPOLOGIA con quel nome
                if (exists)
                {
                    string codEccezione = "";
                    if (!String.IsNullOrEmpty(doc.CustomDataJSON) && doc.CustomDataJSON != "[]")
                    {
                        List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                        if (tt != null)
                        {
                            // esempio codEccezione = ON
                            codEccezione = tt.Valore;
                        }

                        if (String.IsNullOrEmpty(codEccezione))
                        {
                            // in alcuni casi l'eccezione non viene salvata nel campo EccezionePerAutomatismo, ma nel campo EccezioneSelezionataNascosta
                            tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                            if (tt != null)
                            {
                                codEccezione = tt.Valore;
                            }
                        }

                        if (!String.IsNullOrEmpty(codEccezione))
                        {
                            // combina il contenuto di nometipo con il codice eccezione per ottenere 
                            // qualcosa tipo DEMDOC_VSRUO_CPM_ON
                            nomeTipo += codEccezione;

                            // verifica se esiste la tipologia contenuta in nomeTipo es: DEMDOC_VSRUO_CPM_ON
                            var wkfTipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeTipo)).FirstOrDefault();

                            if (wkfTipologia != null)
                            {
                                result = wkfTipologia;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private XR_WKF_TIPOLOGIA Get_WKF_Tipologia(int idWKF, string CustomDataJSON)
        {
            XR_WKF_TIPOLOGIA result = null;
            var db = AnagraficaManager.GetDb();

            try
            {

                XR_WKF_TIPOLOGIA WKF_Tipologia = Get_WKF_Tipologia(idWKF);
                if (WKF_Tipologia == null)
                {
                    throw new Exception("Workflow non trovato");
                }

                result = WKF_Tipologia;

                // prende il nome della tipologia corrente
                // ad esempio DEMDOC_VSRUO_CPM
                string nomeTipo = WKF_Tipologia.COD_TIPOLOGIA;
                nomeTipo += "_";

                // verifica se esistono tipologie di workflow che hanno come nome che inizia per [nomeTipo_]*
                bool exists = db.XR_WKF_TIPOLOGIA.Count(w => w.COD_TIPOLOGIA.Contains(nomeTipo)) > 0;

                // se ce ne sono allora verifica che il documento abbia il codice eccezione selezionato 
                // e che esista un XR_WKF_TIPOLOGIA con quel nome
                if (exists)
                {
                    string codEccezione = "";
                    if (!String.IsNullOrEmpty(CustomDataJSON) && CustomDataJSON != "[]")
                    {
                        List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                        if (tt != null)
                        {
                            // esempio codEccezione = ON
                            codEccezione = tt.Valore;
                        }

                        if (String.IsNullOrEmpty(codEccezione))
                        {
                            // in alcuni casi l'eccezione non viene salvata nel campo EccezionePerAutomatismo, ma nel campo EccezioneSelezionataNascosta
                            tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                            if (tt != null)
                            {
                                codEccezione = tt.Valore;
                            }
                        }

                        if (!String.IsNullOrEmpty(codEccezione))
                        {
                            // combina il contenuto di nometipo con il codice eccezione per ottenere 
                            // qualcosa tipo DEMDOC_VSRUO_CPM_ON
                            nomeTipo += codEccezione;

                            // verifica se esiste la tipologia contenuta in nomeTipo es: DEMDOC_VSRUO_CPM_ON
                            var wkfTipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeTipo)).FirstOrDefault();

                            if (wkfTipologia != null)
                            {
                                result = wkfTipologia;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private XR_WKF_TIPOLOGIA Get_WKF_Tipologia(string codiceWKF_Tipologia, string CustomDataJSON)
        {
            XR_WKF_TIPOLOGIA result = null;
            var db = AnagraficaManager.GetDb();

            try
            {

                XR_WKF_TIPOLOGIA WKF_Tipologia = Get_WKF_Tipologia(codiceWKF_Tipologia);
                if (WKF_Tipologia == null)
                {
                    throw new Exception("Workflow non trovato");
                }

                result = WKF_Tipologia;

                // prende il nome della tipologia corrente
                // ad esempio DEMDOC_VSRUO_CPM
                string nomeTipo = WKF_Tipologia.COD_TIPOLOGIA;
                nomeTipo += "_";

                // verifica se esistono tipologie di workflow che hanno come nome che inizia per [nomeTipo_]*
                bool exists = db.XR_WKF_TIPOLOGIA.Count(w => w.COD_TIPOLOGIA.Contains(nomeTipo)) > 0;

                // se ce ne sono allora verifica che il documento abbia il codice eccezione selezionato 
                // e che esista un XR_WKF_TIPOLOGIA con quel nome
                if (exists)
                {
                    string codEccezione = "";
                    if (!String.IsNullOrEmpty(CustomDataJSON) && CustomDataJSON != "[]")
                    {
                        List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                        if (tt != null)
                        {
                            // esempio codEccezione = ON
                            codEccezione = tt.Valore;
                        }

                        if (String.IsNullOrEmpty(codEccezione))
                        {
                            // in alcuni casi l'eccezione non viene salvata nel campo EccezionePerAutomatismo, ma nel campo EccezioneSelezionataNascosta
                            tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                            if (tt != null)
                            {
                                codEccezione = tt.Valore;
                            }
                        }

                        if (!String.IsNullOrEmpty(codEccezione))
                        {
                            // combina il contenuto di nometipo con il codice eccezione per ottenere 
                            // qualcosa tipo DEMDOC_VSRUO_CPM_ON
                            nomeTipo += codEccezione;

                            // verifica se esiste la tipologia contenuta in nomeTipo es: DEMDOC_VSRUO_CPM_ON
                            var wkfTipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeTipo)).FirstOrDefault();

                            if (wkfTipologia != null)
                            {
                                result = wkfTipologia;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private XR_WKF_TIPOLOGIA Get_WKF_Tipologia(string tipologiaDocumentale, string codiceWKF_Tipologia, string CustomDataJSON)
        {
            XR_WKF_TIPOLOGIA result = null;
            var db = AnagraficaManager.GetDb();

            try
            {

                XR_WKF_TIPOLOGIA WKF_Tipologia = Get_XR_WKF_TIPOLOGIA(tipologiaDocumentale, codiceWKF_Tipologia);
                if (WKF_Tipologia == null)
                {
                    throw new Exception("Workflow non trovato");
                }

                result = WKF_Tipologia;

                // prende il nome della tipologia corrente
                // ad esempio DEMDOC_VSRUO_CPM
                string nomeTipo = WKF_Tipologia.COD_TIPOLOGIA;
                nomeTipo += "_";

                // verifica se esistono tipologie di workflow che hanno come nome che inizia per [nomeTipo_]*
                bool exists = db.XR_WKF_TIPOLOGIA.Count(w => w.COD_TIPOLOGIA.Contains(nomeTipo)) > 0;

                // se ce ne sono allora verifica che il documento abbia il codice eccezione selezionato 
                // e che esista un XR_WKF_TIPOLOGIA con quel nome
                if (exists)
                {
                    string codEccezione = "";
                    if (!String.IsNullOrEmpty(CustomDataJSON) && CustomDataJSON != "[]")
                    {
                        List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                        if (tt != null)
                        {
                            // esempio codEccezione = ON
                            codEccezione = tt.Valore;
                        }

                        if (String.IsNullOrEmpty(codEccezione))
                        {
                            // in alcuni casi l'eccezione non viene salvata nel campo EccezionePerAutomatismo, ma nel campo EccezioneSelezionataNascosta
                            tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                            if (tt != null)
                            {
                                codEccezione = tt.Valore;
                            }
                        }

                        if (!String.IsNullOrEmpty(codEccezione))
                        {
                            // combina il contenuto di nometipo con il codice eccezione per ottenere 
                            // qualcosa tipo DEMDOC_VSRUO_CPM_ON
                            nomeTipo += codEccezione;

                            // verifica se esiste la tipologia contenuta in nomeTipo es: DEMDOC_VSRUO_CPM_ON
                            var wkfTipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeTipo)).FirstOrDefault();

                            if (wkfTipologia != null)
                            {
                                result = wkfTipologia;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private XR_WKF_TIPOLOGIA Get_WKF_Tipologia(string tipologiaDocumentale, int idTipoDoc, string CustomDataJSON)
        {
            XR_WKF_TIPOLOGIA result = null;
            IncentiviEntities db = new IncentiviEntities();

            try
            {
                XR_DEM_TIPI_DOCUMENTO tipoDoc = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(idTipoDoc)).FirstOrDefault();

                if (tipoDoc == null)
                {
                    throw new Exception("Tipo documento non trovato");
                }

                string nomeWKF = String.Format("DEMDOC_{0}_{1}", tipologiaDocumentale.Trim(), tipoDoc.Codice.Trim());

                XR_WKF_TIPOLOGIA WKF_Tipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeWKF)).FirstOrDefault();
                if (WKF_Tipologia == null)
                {
                    throw new Exception("Workflow non trovato");
                }

                result = WKF_Tipologia;

                // prende il nome della tipologia corrente
                // ad esempio DEMDOC_VSRUO_CPM
                string nomeTipo = WKF_Tipologia.COD_TIPOLOGIA;
                nomeTipo += "_";

                // verifica se esistono tipologie di workflow che hanno come nome che inizia per [nomeTipo_]*
                bool exists = db.XR_WKF_TIPOLOGIA.Count(w => w.COD_TIPOLOGIA.Contains(nomeTipo)) > 0;

                // se ce ne sono allora verifica che il documento abbia il codice eccezione selezionato 
                // e che esista un XR_WKF_TIPOLOGIA con quel nome
                if (exists)
                {
                    string codEccezione = "";
                    if (!String.IsNullOrEmpty(CustomDataJSON) && CustomDataJSON != "[]")
                    {
                        List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                        if (tt != null)
                        {
                            // esempio codEccezione = ON
                            codEccezione = tt.Valore;
                        }

                        if (String.IsNullOrEmpty(codEccezione))
                        {
                            // in alcuni casi l'eccezione non viene salvata nel campo EccezionePerAutomatismo, ma nel campo EccezioneSelezionataNascosta
                            tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                            if (tt != null)
                            {
                                codEccezione = tt.Valore;
                            }
                        }

                        if (!String.IsNullOrEmpty(codEccezione))
                        {
                            // combina il contenuto di nometipo con il codice eccezione per ottenere 
                            // qualcosa tipo DEMDOC_VSRUO_CPM_ON
                            nomeTipo += codEccezione;

                            // verifica se esiste la tipologia contenuta in nomeTipo es: DEMDOC_VSRUO_CPM_ON
                            var wkfTipologia = db.XR_WKF_TIPOLOGIA.Where(w => w.COD_TIPOLOGIA.Equals(nomeTipo)).FirstOrDefault();

                            if (wkfTipologia != null)
                            {
                                result = wkfTipologia;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private XR_DEM_TIPI_DOCUMENTO Get_XR_DEM_TIPI_DOCUMENTO(string code)
        {
            XR_DEM_TIPI_DOCUMENTO result = null;

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                var item = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Codice.Equals(code)).FirstOrDefault();
                if (item != null)
                {
                    result = item;
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private XR_DEM_TIPI_DOCUMENTO Get_XR_DEM_TIPI_DOCUMENTO(int id)
        {
            XR_DEM_TIPI_DOCUMENTO result = null;

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                var item = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(id)).FirstOrDefault();
                if (item != null)
                {
                    result = item;
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private AzioneAutomaticaResult EseguiAzioneAutomaticaContabile(XR_DEM_DOCUMENTI doc, ref IncentiviEntities db)
        {
            AzioneAutomaticaResult result = new AzioneAutomaticaResult();
            result.Esito = true;
            result.IdRichiesta = 0;
            result.NuovoStato = doc.Id_Stato;

            try
            {
                string codEccezione = null;
                if (!String.IsNullOrEmpty(doc.CustomDataJSON) && doc.CustomDataJSON != "[]")
                {
                    List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                    var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                    if (tt != null)
                    {
                        codEccezione = tt.Valore;
                    }

                    if (String.IsNullOrEmpty(codEccezione))
                    {
                        tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                        if (tt != null)
                        {
                            codEccezione = tt.Valore;
                        }
                    }

                    if (String.IsNullOrEmpty(codEccezione))
                    {
                        throw new Exception("Impossibile proseguire tipo di variazione mancante");
                    }

                    // a questo punto in base al tipo variazione chiama dei sotto metodi che restituiscono un AzioneAutomaticaResult

                    /*
                    "text": "Assegnazione temporanea",
                    "value": "03R"
                    "text": "Fine assegnazione temporanea",
                    "value": "03S"
                    "text": "Fine assegnazione anticipata",
                    "value": "03S_A"
                    "text": "Proroga assegnazione temporanea",
                    "value": "PAT"
                    "text": "Nuova assegnazione temporanea",
                    "value": "03R"
                    "text": "Assegnazione definitiva",
                    "value": "030"
                    "text": "Trasferimento temporaneo",
                    "value": "03T"
                    "text": "Fine trasferimento temporaneo",
                    "value": "03U"
                    "text": "Distacco",
                    "value": "031"
                    "text": "Cambio sezione",
                    "value": "03Z"
                    "text": "Trasferimento definitivo",
                    "value": "005"
                    "text": "Trasferimento a domanda",
                    "value": "034"
                    */

                    if (codEccezione.StartsWith("0"))
                    {
                        codEccezione = codEccezione.Substring(1);
                    }

                    result = EseguiAzioneAutomaticaContabile_Internal(doc, codEccezione, objModuloValorizzato, ref db);

                    if (result.Esito)
                    {
                        result.NuovoStato = DematerializzazioneManager.GetNextIdStato(doc.Id_Stato, doc.Id_WKF_Tipologia);
                    }
                    else
                    {
                        throw new Exception(result.DescrizioneErrore);
                    }
                }
                else
                {
                    throw new Exception("Impossibile proseguire dati aggiuntivi mancanti");
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.NuovoStato = 0;
                result.DescrizioneErrore = ex.Message;
            }
            return result;
        }

        private AzioneAutomaticaResult EseguiAzioneAutomaticaContabile_Internal(XR_DEM_DOCUMENTI doc, string codice_evento, List<AttributiAggiuntivi> objModuloValorizzato, ref IncentiviEntities db)
        {
            AzioneAutomaticaResult result = new AzioneAutomaticaResult();
            result.Esito = true;
            int idPersona = doc.IdPersonaDestinatario.GetValueOrDefault();

            try
            {
                List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

                /*
                 * l'assegnazione temporanea scrive su XR_Servizio, la sede
                 * può cambiare, ma nella stessa città
                 * Per prima cosa prende i dati dal JSON
                 * Deve cercare il campo SERVIZIO ed il campo SEDE
                 */
                string servizio = null;
                string sede = null;
                string sezione = null;
                string nota = null;
                int idUOG = 0;
                DateTime dataInizio = DateTime.MinValue;
                DateTime dataFine = DateTime.MinValue;
                SINTESI1 datiUtente = null;

                datiUtente = db.SINTESI1.Where(w => w.ID_PERSONA.Equals(idPersona)).FirstOrDefault();

                if (datiUtente == null)
                {
                    throw new Exception("Utente non presente in anagrafica.");
                }

                var _servizioObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SERVIZIO").FirstOrDefault();
                if (_servizioObj != null)
                {
                    servizio = _servizioObj.Valore;
                }

                var _sedeObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SEDE").FirstOrDefault();
                if (_sedeObj != null)
                {
                    sede = _sedeObj.Valore;
                }

                var _sezioneObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SEZIONE").FirstOrDefault();
                if (_sezioneObj != null)
                {
                    sezione = _sezioneObj.Valore;
                }

                if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                {
                    foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
                    {
                        listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
                    }
                }
                else
                {
                    listaPiatta.AddRange(objModuloValorizzato.ToList());
                }

                // adesso in lista piatta c'è o il contenuto di GetListaPiatta oppure l'oggetto standard senza elementi inline
                var dtInizio = listaPiatta.Where(w => w.DBRefAttribute == "DATA_DECORRENZA").FirstOrDefault();
                if (dtInizio != null)
                {
                    DateTime temp;
                    string dt = dtInizio.Valore;
                    if (!String.IsNullOrEmpty(dt))
                    {
                        if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                            throw new Exception("Errore in conversione DATA_DECORRENZA");
                        dataInizio = temp;
                    }
                }
                else
                {
                    throw new Exception("Errore DATA_DECORRENZA non trovata");
                }

                var dtFine = listaPiatta.Where(w => w.DBRefAttribute == "DATA_SCADENZA").FirstOrDefault();
                if (dtFine != null)
                {
                    DateTime temp;
                    string dt = dtFine.Valore;
                    if (!String.IsNullOrEmpty(dt))
                    {
                        if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                            throw new Exception("Errore in conversione DATA_SCADENZA");
                        dataFine = temp;
                    }
                }
                else
                {
                    // #PRESTARE ATTENZIONE potrebbe non essere corretto
                    dataFine = new DateTime(2999, 12, 31);
                    //throw new Exception("Errore DATA_SCADENZA non trovata");
                }

                var notaObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "NOTE").FirstOrDefault();
                if (notaObj != null)
                {
                    nota = notaObj.Valore;
                }

                if (String.IsNullOrEmpty(nota))
                {
                    nota = null;
                }

                if (dataInizio == DateTime.MinValue || dataFine == DateTime.MinValue)
                {
                    throw new Exception("Errore, le date non sono corrette");
                }

                #region DA SPOSTARE DOPO LA FIRMA
                //DateTime nuovaDataFine = new DateTime(2999, 12, 31);

                //if (!String.IsNullOrEmpty(servizio))
                //{
                //    // chiude l'ultimo record di XR_SERVIZIO
                //    XR_SERVIZIO aggiornatoServ = AggiornaUltimo_XR_SERVIZIO(codice_evento, servizio, idPersona, dataInizio, dataFine, ref db);

                //    // crea un nuovo record in XR_SERVIZIO con data fine 31/12/2999
                //    XR_SERVIZIO newItem = Crea_XR_SERVIZIO(codice_evento, servizio, idPersona, dataInizio, nuovaDataFine, ref db);
                //}

                //XR_VARIAZIONE_TEMP chiudiUltimo_XR_VARIAZIONE_TEMP = AggiornaUltimo_XR_VARIAZIONE_TEMP(idPersona, dataInizio, ref db);

                //if (!String.IsNullOrEmpty(sezione))
                //{
                //    var uog = db.UNITAORG.Where(w => w.COD_UNITAORG.Equals(sezione)).FirstOrDefault();
                //    if (uog != null && uog.ID_UNITAORG > 0)
                //    {
                //        idUOG = uog.ID_UNITAORG;

                //        INCARLAV incarlav = Modifica_INCARLAV(codice_evento, sede, idPersona,
                //                                                dataInizio, dataFine,
                //                                                nuovaDataFine, idUOG, nota, ref db);
                //    }
                //}

                //XR_VARIAZIONE_TEMP assegna_Temp = Crea_XR_ASSEGNA_TEMP(codice_evento, servizio, sede, idPersona, dataInizio, dataFine, nuovaDataFine, nota, idUOG, ref db);

                //if (!String.IsNullOrEmpty(sede))
                //{
                //    // se è cambiata la sede ma sempre nella stessa città/provincia allora NON
                //    // deve creare il record su trasf_sede
                //    // su trasf_sede devono essere registrati solo i cambi sede in altre città.
                //    bool creaNuovoRecord = false;

                //    string sedeCorrente = DematerializzazioneManager.GetSedeByMatricola(null, idPersona);

                //    if (String.IsNullOrEmpty(sedeCorrente))
                //    {
                //        throw new Exception("Sede corrente non trovata");
                //    }

                //    if (sede.Trim() != sedeCorrente.Trim())
                //    {
                //        // reperimento della città
                //        string query = "SELECT [DESC_AGGREGATO_SEDE] FROM [LINK_HRIS_HRLIV2].[HR_LIV2].[dbo].[L2D_SEDE] WHERE cod_sede = '##CODICE##'";

                //        query = query.Replace("##CODICE##", sedeCorrente);
                //        string cittaCorrente = db.Database.SqlQuery<string>(query).FirstOrDefault();

                //        if (String.IsNullOrEmpty(cittaCorrente))
                //        {
                //            throw new Exception("Città corrente non trovata");
                //        }

                //        query = "SELECT [DESC_AGGREGATO_SEDE] FROM [LINK_HRIS_HRLIV2].[HR_LIV2].[dbo].[L2D_SEDE] WHERE cod_sede = '##CODICE##'";

                //        query = query.Replace("##CODICE##", sede);
                //        string nuovaCitta = db.Database.SqlQuery<string>(query).FirstOrDefault();

                //        if (String.IsNullOrEmpty(nuovaCitta))
                //        {
                //            throw new Exception("Nessuna città corrisponde al codice sede selezionato");
                //        }

                //        if (nuovaCitta != cittaCorrente)
                //        {
                //            // se è una nuova città allora è un trasferimento
                //            creaNuovoRecord = true;
                //        }
                //    }

                //    if (creaNuovoRecord)
                //    {
                //        TRASF_SEDE chiudiUltimo_Trasf_sede = AggiornaUltimo_Trasf_Sede(codice_evento, sede, idPersona, dataInizio, dataFine, nota, ref db);
                //        TRASF_SEDE nuovoTrasf_Sede = Crea_Trasf_Sede(codice_evento, sede, idPersona, dataInizio, dataFine, nuovaDataFine, nota, ref db);
                //    }
                //}
                #endregion
                byte[] lettera = DematerializzazioneManager.GeneraPdfLetteraVariazione(doc, codice_evento, dataInizio, dataFine);
                string nominativo = DematerializzazioneManager.GetNominativoByMatricola(doc.MatricolaDestinatario);
                nominativo = nominativo.Trim();
                nominativo = nominativo.Replace(" ", "_");
                nominativo = RemoveDiacritics(nominativo);
                string nomeLettera = String.Format("Lettera_Variazione_{0}_{1}_{2}.pdf", codice_evento, doc.MatricolaDestinatario, nominativo);

                #region Crea allegati

                int length = lettera.Length;
                string est = Path.GetExtension(nomeLettera);
                string tipoFile = MimeTypeMap.GetMimeType(est);
                string jsonStringProtocollo = null;

                XR_ALLEGATI allegato = new XR_ALLEGATI()
                {
                    NomeFile = nomeLettera,
                    MimeType = tipoFile,
                    Length = length,
                    ContentByte = lettera,
                    IsPrincipal = false,
                    PosizioneProtocollo = jsonStringProtocollo
                };

                db.XR_ALLEGATI.Add(allegato);

                int tempId = -1;

                XR_DEM_VERSIONI_DOCUMENTO version = new XR_DEM_VERSIONI_DOCUMENTO()
                {
                    N_Versione = 1,
                    DataUltimaModifica = DateTime.Now,
                    Id_Documento = doc.Id,
                    Id = tempId
                };

                db.XR_DEM_VERSIONI_DOCUMENTO.Add(version);

                XR_DEM_ALLEGATI_VERSIONI associativa = new XR_DEM_ALLEGATI_VERSIONI()
                {
                    IdAllegato = allegato.Id,
                    IdVersione = version.Id
                };
                db.XR_DEM_ALLEGATI_VERSIONI.Add(associativa);

                #endregion
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.NuovoStato = 0;
                result.DescrizioneErrore = ex.Message;
            }
            return result;
        }

        //private AzioneAutomaticaResult EseguiAzioneAutomaticaContabile_03R(int idPersona, List<AttributiAggiuntivi> objModuloValorizzato, ref IncentiviEntities db)
        //{
        //    AzioneAutomaticaResult result = new AzioneAutomaticaResult();
        //    result.Esito = true;
        //    string _codEvento = "3R";
        //    // il codice evento diventa 3T se c'è anche un cambio sede e la sede è in un'altra città
        //    try
        //    {
        //        List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

        //        /*
        //         * l'assegnazione temporanea scrive su XR_Servizio, la sede
        //         * può cambiare, ma nella stessa città
        //         * Per prima cosa prende i dati dal JSON
        //         * Deve cercare il campo SERVIZIO ed il campo SEDE
        //         */
        //        string servizio = null;
        //        string sede = null;
        //        string sezione = null;
        //        string nota = null;
        //        int idUOG = 0;
        //        DateTime dataInizio = DateTime.MinValue;
        //        DateTime dataFine = DateTime.MinValue;

        //        var _servizioObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SERVIZIO").FirstOrDefault();
        //        if (_servizioObj != null)
        //        {
        //            servizio = _servizioObj.Valore;
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_DECORRENZA non trovata");
        //        }

        //        var _sedeObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SEDE").FirstOrDefault();
        //        if (_sedeObj != null)
        //        {
        //            sede = _sedeObj.Valore;
        //        }

        //        var _sezioneObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SEZIONE").FirstOrDefault();
        //        if (_sezioneObj != null)
        //        {
        //            sezione = _sezioneObj.Valore;
        //        }

        //        // Se la sede è valorizzata, verifica che sia nella stessa città di quella attuale
        //        if (!String.IsNullOrEmpty(sede))
        //        {
        //            string sedeCorrente = DematerializzazioneManager.GetSedeByMatricola(null, idPersona);

        //            if (String.IsNullOrEmpty(sedeCorrente))
        //            {
        //                throw new Exception("Sede corrente non trovata");
        //            }

        //            if (sede.Trim() != sedeCorrente.Trim())
        //            {
        //                // reperimento della città
        //                string query = "SELECT [DESC_AGGREGATO_SEDE] FROM [LINK_HRIS_HRLIV2].[HR_LIV2].[dbo].[L2D_SEDE] WHERE cod_sede = '##CODICE##'";

        //                query = query.Replace("##CODICE##", sedeCorrente);
        //                string cittaCorrente = db.Database.SqlQuery<string>(query).FirstOrDefault();

        //                if (String.IsNullOrEmpty(cittaCorrente))
        //                {
        //                    throw new Exception("Città corrente non trovata");
        //                }

        //                query = "SELECT [DESC_AGGREGATO_SEDE] FROM [LINK_HRIS_HRLIV2].[HR_LIV2].[dbo].[L2D_SEDE] WHERE cod_sede = '##CODICE##'";

        //                query = query.Replace("##CODICE##", sede);
        //                string nuovaCitta = db.Database.SqlQuery<string>(query).FirstOrDefault();

        //                if (String.IsNullOrEmpty(nuovaCitta))
        //                {
        //                    throw new Exception("Nessuna città corrisponde al codice sede selezionato");
        //                }

        //                if (nuovaCitta != cittaCorrente)
        //                {
        //                    // se è una nuova città allora è un trasferimento
        //                    _codEvento = "3T";
        //                }
        //            }
        //        }

        //        if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
        //        {
        //            foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
        //            {
        //                listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
        //            }                    
        //        }
        //        else
        //        {
        //            listaPiatta.AddRange(objModuloValorizzato.ToList());
        //        }

        //        // adesso in lista piatta c'è o il contenuto di GetListaPiatta oppure l'oggetto standard senza elementi inline
        //        var dtInizio = listaPiatta.Where(w => w.DBRefAttribute == "DATA_DECORRENZA").FirstOrDefault();
        //        if (dtInizio != null)
        //        {
        //            DateTime temp;
        //            string dt = dtInizio.Valore;
        //            if (!String.IsNullOrEmpty(dt))
        //            {
        //                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
        //                    throw new Exception("Errore in conversione DATA_DECORRENZA");
        //                dataInizio = temp;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_DECORRENZA non trovata");
        //        }

        //        var dtFine = listaPiatta.Where(w => w.DBRefAttribute == "DATA_SCADENZA").FirstOrDefault();
        //        if (dtFine != null)
        //        {
        //            DateTime temp;
        //            string dt = dtFine.Valore;
        //            if (!String.IsNullOrEmpty(dt))
        //            {
        //                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
        //                    throw new Exception("Errore in conversione DATA_SCADENZA");
        //                dataFine = temp;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_SCADENZA non trovata");
        //        }

        //        var notaObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "NOTE").FirstOrDefault();
        //        if (notaObj != null)
        //        {
        //            nota = notaObj.Valore;
        //        }

        //        if (String.IsNullOrEmpty(nota))
        //        {
        //            nota = null;
        //        }

        //        if (dataInizio == DateTime.MinValue || dataFine == DateTime.MinValue)
        //        {
        //            throw new Exception("Errore, le date non sono corrette");
        //        }

        //        /*
        //         * Una volta raccolte le info si passa al salvataggio dei dati sul db
        //         */

        //        /*
        //         * Aggiornamento XR_SERVIZIO
        //         */

        //        XR_SERVIZIO aggiornatoServ = AggiornaUltimo_XR_SERVIZIO(_codEvento, servizio, idPersona, dataInizio, dataFine, ref db);

        //        DateTime nuovaDataFine = new DateTime(2999, 12, 31);

        //        XR_SERVIZIO newItem = Crea_XR_SERVIZIO(_codEvento, servizio, idPersona, dataInizio, nuovaDataFine, ref db);

        //        if (!String.IsNullOrEmpty(sede) && _codEvento == "3T")
        //        {
        //            TRASF_SEDE aggiornatoTrasf = AggiornaUltimo_Trasf_Sede(_codEvento, sede, idPersona, dataInizio, dataFine, nota, ref db);
        //            /*
        //             * Aggiornamento TRASF_SEDE
        //             */
        //            TRASF_SEDE trasf_sede = Crea_Trasf_Sede(_codEvento, sede, idPersona, dataInizio, dataFine, nuovaDataFine, nota, ref db);
        //        }

        //        if (!String.IsNullOrEmpty(sezione))
        //        {
        //            var uog = db.UNITAORG.Where(w => w.COD_UNITAORG.Equals(sezione)).FirstOrDefault();
        //            if (uog != null && uog.ID_UNITAORG > 0)
        //            {
        //                idUOG = uog.ID_UNITAORG;
        //            }
        //        }

        //        XR_VARIAZIONE_TEMP assegna_Temp = Crea_XR_ASSEGNA_TEMP(_codEvento, servizio, sede, idPersona, dataInizio, dataFine, nuovaDataFine, nota, idUOG, ref db);                
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Esito = false;
        //        result.NuovoStato = 0;
        //        result.DescrizioneErrore = ex.Message;
        //    }
        //    return result;
        //}

        //private AzioneAutomaticaResult EseguiAzioneAutomaticaContabile_03S(int idPersona, List<AttributiAggiuntivi> objModuloValorizzato, ref IncentiviEntities db)
        //{
        //    AzioneAutomaticaResult result = new AzioneAutomaticaResult();
        //    result.Esito = true;
        //    string _codEvento = "3S";

        //    try
        //    {
        //        List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

        //        /*
        //         * l'assegnazione temporanea scrive su XR_Servizio, la sede
        //         * può cambiare, ma nella stessa città
        //         * Per prima cosa prende i dati dal JSON
        //         * Deve cercare il campo SERVIZIO ed il campo SEDE
        //         */
        //        string servizio = null;
        //        string sede = null;
        //        string nota = null;
        //        DateTime dataInizio = DateTime.MinValue;
        //        DateTime dataFine = DateTime.MaxValue;

        //        var _servizioObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SERVIZIO").FirstOrDefault();
        //        if (_servizioObj != null)
        //        {
        //            servizio = _servizioObj.Valore;
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_DECORRENZA non trovata");
        //        }

        //        var _sedeObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SEDE").FirstOrDefault();
        //        if (_sedeObj != null)
        //        {
        //            sede = _sedeObj.Valore;
        //        }

        //        if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
        //        {
        //            foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
        //            {
        //                listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
        //            }
        //        }
        //        else
        //        {
        //            listaPiatta.AddRange(objModuloValorizzato.ToList());
        //        }

        //        // adesso in lista piatta c'è o il contenuto di GetListaPiatta oppure l'oggetto standard senza elementi inline
        //        var dtInizio = listaPiatta.Where(w => w.DBRefAttribute == "DATA_DECORRENZA").FirstOrDefault();
        //        if (dtInizio != null)
        //        {
        //            DateTime temp;
        //            string dt = dtInizio.Valore;
        //            if (!String.IsNullOrEmpty(dt))
        //            {
        //                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
        //                    throw new Exception("Errore in conversione DATA_DECORRENZA");
        //                dataInizio = temp;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_DECORRENZA non trovata");
        //        }

        //        var dtFine = listaPiatta.Where(w => w.DBRefAttribute == "DATA_SCADENZA").FirstOrDefault();
        //        if (dtFine != null)
        //        {
        //            DateTime temp;
        //            string dt = dtFine.Valore;
        //            if (!String.IsNullOrEmpty(dt))
        //            {
        //                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
        //                    throw new Exception("Errore in conversione DATA_SCADENZA");
        //                dataFine = temp;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_SCADENZA non trovata");
        //        }

        //        var notaObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "NOTE").FirstOrDefault();
        //        if (notaObj != null)
        //        {
        //            nota = notaObj.Valore;
        //        }

        //        if (dataInizio == DateTime.MinValue)
        //        {
        //            throw new Exception("Errore, le date non sono corrette");
        //        }

        //        /*
        //         * Una volta raccolte le info si passa al salvataggio dei dati sul db
        //         */

        //        /*
        //         * Aggiornamento XR_SERVIZIO
        //         */
        //        XR_SERVIZIO newItem = Crea_XR_SERVIZIO(_codEvento, servizio, idPersona, dataInizio, dataFine, ref db);

        //        if (!String.IsNullOrEmpty(sede))
        //        {
        //            /*
        //             * Aggiornamento TRASF_SEDE
        //             */
        //            DateTime dtFineNuova = new DateTime(2999, 12, 31); 
        //            TRASF_SEDE trasf_sede = Crea_Trasf_Sede(_codEvento, sede, idPersona, dataInizio, dataFine, dtFineNuova, nota, ref db);
        //        }                
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Esito = false;
        //        result.NuovoStato = 0;
        //        result.DescrizioneErrore = ex.Message;
        //    }
        //    return result;
        //}

        //private AzioneAutomaticaResult EseguiAzioneAutomaticaContabile_PAT(int idPersona, List<AttributiAggiuntivi> objModuloValorizzato, ref IncentiviEntities db)
        //{
        //    AzioneAutomaticaResult result = new AzioneAutomaticaResult();
        //    result.Esito = true;
        //    string _codEvento = "03R";

        //    try
        //    {
        //        List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

        //        /*
        //         * l'assegnazione temporanea scrive su XR_Servizio, la sede
        //         * può cambiare, ma nella stessa città
        //         * Per prima cosa prende i dati dal JSON
        //         * Deve cercare il campo SERVIZIO ed il campo SEDE
        //         */
        //        string servizio = null;
        //        string sede = null;
        //        string nota = null;
        //        DateTime dataInizio = DateTime.MinValue;
        //        DateTime dataFine = DateTime.MinValue;

        //        var _servizioObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SERVIZIO").FirstOrDefault();
        //        if (_servizioObj != null)
        //        {
        //            servizio = _servizioObj.Valore;
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_DECORRENZA non trovata");
        //        }

        //        var _sedeObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SEDE").FirstOrDefault();
        //        if (_sedeObj != null)
        //        {
        //            sede = _sedeObj.Valore;
        //        }

        //        if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
        //        {
        //            foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
        //            {
        //                listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
        //            }
        //        }
        //        else
        //        {
        //            listaPiatta.AddRange(objModuloValorizzato.ToList());
        //        }

        //        // adesso in lista piatta c'è o il contenuto di GetListaPiatta oppure l'oggetto standard senza elementi inline
        //        var dtInizio = listaPiatta.Where(w => w.DBRefAttribute == "DATA_DECORRENZA").FirstOrDefault();
        //        if (dtInizio != null)
        //        {
        //            DateTime temp;
        //            string dt = dtInizio.Valore;
        //            if (!String.IsNullOrEmpty(dt))
        //            {
        //                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
        //                    throw new Exception("Errore in conversione DATA_DECORRENZA");
        //                dataInizio = temp;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_DECORRENZA non trovata");
        //        }

        //        var dtFine = listaPiatta.Where(w => w.DBRefAttribute == "DATA_SCADENZA").FirstOrDefault();
        //        if (dtFine != null)
        //        {
        //            DateTime temp;
        //            string dt = dtFine.Valore;
        //            if (!String.IsNullOrEmpty(dt))
        //            {
        //                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
        //                    throw new Exception("Errore in conversione DATA_SCADENZA");
        //                dataFine = temp;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_SCADENZA non trovata");
        //        }

        //        var notaObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "NOTE").FirstOrDefault();
        //        if (notaObj != null)
        //        {
        //            nota = notaObj.Valore;
        //        }

        //        if (dataInizio == DateTime.MinValue || dataFine == DateTime.MinValue)
        //        {
        //            throw new Exception("Errore, le date non sono corrette");
        //        }
        //        DateTime newDataFine = new DateTime(2999, 12, 31);
        //        // da rivedere
        //        //XR_ASSEGNA_TEMP assegna_Temp = Crea_XR_ASSEGNA_TEMP(_codEvento, servizio, sede, idPersona, dataInizio, dataFine, newDataFine, nota, 0, ref db);

        //    }
        //    catch (Exception ex)
        //    {
        //        result.Esito = false;
        //        result.NuovoStato = 0;
        //        result.DescrizioneErrore = ex.Message;
        //    }
        //    return result;
        //}

        //private AzioneAutomaticaResult EseguiAzioneAutomaticaContabile_03U(int idPersona, List<AttributiAggiuntivi> objModuloValorizzato, ref IncentiviEntities db)
        //{
        //    AzioneAutomaticaResult result = new AzioneAutomaticaResult();
        //    result.Esito = true;
        //    string _codEvento = "3U";            
        //    try
        //    {
        //        List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

        //        string servizio = null;
        //        string sede = null;
        //        string nota = null;
        //        DateTime dataInizio = DateTime.MinValue;
        //        DateTime dataFine = DateTime.MinValue;

        //        // sede, servizio e sezione non sono obbligatori

        //        var _servizioObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SERVIZIO").FirstOrDefault();
        //        if (_servizioObj != null)
        //        {
        //            servizio = _servizioObj.Valore;
        //        }
        //        // da rivedere
        //        //var _ordinatoXR_ASSEGNA_TEMP = db.XR_ASSEGNA_TEMP.Where(w => w.ID_PERSONA.Equals(idPersona)).OrderBy(w => w.DTA_FINE).ToList();
        //        //var ultimo3T = _ordinatoXR_ASSEGNA_TEMP.Where(w => w.COD_EVASSTEMP.Equals("3T")).LastOrDefault();
        //        //sede = ultimo3T.COD_SEDE;

        //        //if (String.IsNullOrEmpty(servizio))
        //        //{
        //        //    servizio = ultimo3T.COD_SERVIZIO;
        //        //}

        //        var _sedeObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SEDE").FirstOrDefault();
        //        if (_sedeObj != null)
        //        {
        //            sede = _sedeObj.Valore;
        //        }

        //        if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
        //        {
        //            foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
        //            {
        //                listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
        //            }
        //        }
        //        else
        //        {
        //            listaPiatta.AddRange(objModuloValorizzato.ToList());
        //        }

        //        // adesso in lista piatta c'è o il contenuto di GetListaPiatta oppure l'oggetto standard senza elementi inline
        //        var dtInizio = listaPiatta.Where(w => w.DBRefAttribute == "DATA_DECORRENZA").FirstOrDefault();
        //        if (dtInizio != null)
        //        {
        //            DateTime temp;
        //            string dt = dtInizio.Valore;
        //            if (!String.IsNullOrEmpty(dt))
        //            {
        //                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
        //                    throw new Exception("Errore in conversione DATA_DECORRENZA");
        //                dataInizio = temp;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_DECORRENZA non trovata");
        //        }

        //        var dtFine = listaPiatta.Where(w => w.DBRefAttribute == "DATA_SCADENZA").FirstOrDefault();
        //        if (dtFine != null)
        //        {
        //            DateTime temp;
        //            string dt = dtFine.Valore;
        //            if (!String.IsNullOrEmpty(dt))
        //            {
        //                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
        //                    throw new Exception("Errore in conversione DATA_SCADENZA");
        //                dataFine = temp;
        //            }
        //            else
        //            {
        //                dataFine = dataInizio;
        //            }
        //        }
        //        else
        //        {
        //            dataFine = dataInizio;
        //        }

        //        dataFine = new DateTime(2999, 12, 31);

        //        var notaObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "NOTE").FirstOrDefault();
        //        if (notaObj != null)
        //        {
        //            nota = notaObj.Valore;
        //        }

        //        if (String.IsNullOrEmpty(nota))
        //        {
        //            nota = null;
        //        }

        //        if (dataInizio == DateTime.MinValue || dataFine == DateTime.MinValue)
        //        {
        //            throw new Exception("Errore, le date non sono corrette");
        //        }

        //        /*
        //         * Una volta raccolte le info si passa al salvataggio dei dati sul db
        //         */

        //        /*
        //         * Aggiornamento XR_SERVIZIO
        //         */
        //        XR_SERVIZIO newItem = Crea_XR_SERVIZIO(_codEvento, servizio, idPersona, dataInizio, dataFine, ref db);
        //        DateTime newDataFine = new DateTime(2999, 12, 31);
        //        TRASF_SEDE trasf_sede = Crea_Trasf_Sede(_codEvento, sede, idPersona, dataInizio, dataFine, newDataFine, nota, ref db);
        //        //if (!String.IsNullOrEmpty(sede) && nuovaCitta != cittaCorrente)
        //        //{
        //        //    /*
        //        //     * Aggiornamento TRASF_SEDE
        //        //     */
        //        //    TRASF_SEDE trasf_sede = Crea_Trasf_Sede(_codEvento, sede, idPersona, dataInizio, dataFine, nota, ref db);
        //        //}

        //        // da rivedere
        //        //XR_ASSEGNA_TEMP assegna_Temp = Crea_XR_ASSEGNA_TEMP(_codEvento, servizio, sede, idPersona, dataInizio, dataFine, newDataFine, nota, 0, ref db);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Esito = false;
        //        result.NuovoStato = 0;
        //        result.DescrizioneErrore = ex.Message;
        //    }
        //    return result;
        //}

        //private AzioneAutomaticaResult EseguiAzioneAutomaticaContabile_030(int idPersona, List<AttributiAggiuntivi> objModuloValorizzato, ref IncentiviEntities db)
        //{
        //    AzioneAutomaticaResult result = new AzioneAutomaticaResult();
        //    result.Esito = true;
        //    string _codEvento = "30";
        //    // il codice evento diventa 3T se c'è anche un cambio sede e la sede è in un'altra città
        //    try
        //    {
        //        List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

        //        /*
        //         * l'assegnazione temporanea scrive su XR_Servizio, la sede
        //         * può cambiare, ma nella stessa città
        //         * Per prima cosa prende i dati dal JSON
        //         * Deve cercare il campo SERVIZIO ed il campo SEDE
        //         */
        //        string servizio = null;
        //        string sede = null;
        //        string sezione = null;
        //        string nota = null;
        //        int idUOG = 0;
        //        DateTime dataInizio = DateTime.MinValue;
        //        DateTime dataFine = DateTime.MinValue;

        //        var _servizioObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SERVIZIO").FirstOrDefault();
        //        if (_servizioObj != null)
        //        {
        //            servizio = _servizioObj.Valore;
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_DECORRENZA non trovata");
        //        }

        //        var _sedeObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SEDE").FirstOrDefault();
        //        if (_sedeObj != null)
        //        {
        //            sede = _sedeObj.Valore;
        //        }

        //        var _sezioneObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SEZIONE").FirstOrDefault();
        //        if (_sezioneObj != null)
        //        {
        //            sezione = _sezioneObj.Valore;
        //        }

        //        // Se la sede è valorizzata, verifica che sia nella stessa città di quella attuale
        //        if (!String.IsNullOrEmpty(sede))
        //        {
        //            string sedeCorrente = DematerializzazioneManager.GetSedeByMatricola(null, idPersona);

        //            if (String.IsNullOrEmpty(sedeCorrente))
        //            {
        //                throw new Exception("Sede corrente non trovata");
        //            }

        //            if (sede.Trim() != sedeCorrente.Trim())
        //            {
        //                // reperimento della città
        //                string query = "SELECT [DESC_AGGREGATO_SEDE] FROM [LINK_HRIS_HRLIV2].[HR_LIV2].[dbo].[L2D_SEDE] WHERE cod_sede = '##CODICE##'";

        //                query = query.Replace("##CODICE##", sedeCorrente);
        //                string cittaCorrente = db.Database.SqlQuery<string>(query).FirstOrDefault();

        //                if (String.IsNullOrEmpty(cittaCorrente))
        //                {
        //                    throw new Exception("Città corrente non trovata");
        //                }

        //                query = "SELECT [DESC_AGGREGATO_SEDE] FROM [LINK_HRIS_HRLIV2].[HR_LIV2].[dbo].[L2D_SEDE] WHERE cod_sede = '##CODICE##'";

        //                query = query.Replace("##CODICE##", sede);
        //                string nuovaCitta = db.Database.SqlQuery<string>(query).FirstOrDefault();

        //                if (String.IsNullOrEmpty(nuovaCitta))
        //                {
        //                    throw new Exception("Nessuna città corrisponde al codice sede selezionato");
        //                }

        //                if (nuovaCitta != cittaCorrente)
        //                {
        //                    // se è una nuova città allora è un trasferimento
        //                    _codEvento = "3T";
        //                }
        //            }
        //        }

        //        if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
        //        {
        //            foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
        //            {
        //                listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
        //            }
        //        }
        //        else
        //        {
        //            listaPiatta.AddRange(objModuloValorizzato.ToList());
        //        }

        //        // adesso in lista piatta c'è o il contenuto di GetListaPiatta oppure l'oggetto standard senza elementi inline
        //        var dtInizio = listaPiatta.Where(w => w.DBRefAttribute == "DATA_DECORRENZA").FirstOrDefault();
        //        if (dtInizio != null)
        //        {
        //            DateTime temp;
        //            string dt = dtInizio.Valore;
        //            if (!String.IsNullOrEmpty(dt))
        //            {
        //                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
        //                    throw new Exception("Errore in conversione DATA_DECORRENZA");
        //                dataInizio = temp;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_DECORRENZA non trovata");
        //        }

        //        var dtFine = listaPiatta.Where(w => w.DBRefAttribute == "DATA_SCADENZA").FirstOrDefault();
        //        if (dtFine != null)
        //        {
        //            DateTime temp;
        //            string dt = dtFine.Valore;
        //            if (!String.IsNullOrEmpty(dt))
        //            {
        //                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
        //                    throw new Exception("Errore in conversione DATA_SCADENZA");
        //                dataFine = temp;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_SCADENZA non trovata");
        //        }

        //        var notaObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "NOTE").FirstOrDefault();
        //        if (notaObj != null)
        //        {
        //            nota = notaObj.Valore;
        //        }

        //        if (String.IsNullOrEmpty(nota))
        //        {
        //            nota = null;
        //        }

        //        if (dataInizio == DateTime.MinValue || dataFine == DateTime.MinValue)
        //        {
        //            throw new Exception("Errore, le date non sono corrette");
        //        }

        //        /*
        //         * Una volta raccolte le info si passa al salvataggio dei dati sul db
        //         */

        //        /*
        //         * Aggiornamento XR_SERVIZIO
        //         */

        //        XR_SERVIZIO aggiornatoServ = AggiornaUltimo_XR_SERVIZIO(_codEvento, servizio, idPersona, dataInizio, dataFine, ref db);

        //        DateTime nuovaDataFine = new DateTime(2999, 12, 31);

        //        XR_SERVIZIO newItem = Crea_XR_SERVIZIO(_codEvento, servizio, idPersona, dataInizio, nuovaDataFine, ref db);

        //        if (!String.IsNullOrEmpty(sede) && _codEvento == "3T")
        //        {
        //            TRASF_SEDE aggiornatoTrasf = AggiornaUltimo_Trasf_Sede(_codEvento, sede, idPersona, dataInizio, dataFine, nota, ref db);
        //            /*
        //             * Aggiornamento TRASF_SEDE
        //             */
        //            TRASF_SEDE trasf_sede = Crea_Trasf_Sede(_codEvento, sede, idPersona, dataInizio, dataFine, nuovaDataFine, nota, ref db);
        //        }

        //        if (!String.IsNullOrEmpty(sezione))
        //        {
        //            var uog = db.UNITAORG.Where(w => w.COD_UNITAORG.Equals(sezione)).FirstOrDefault();
        //            if (uog != null && uog.ID_UNITAORG > 0)
        //            {
        //                idUOG = uog.ID_UNITAORG;
        //            }
        //        }

        //        XR_VARIAZIONE_TEMP assegna_Temp = Crea_XR_ASSEGNA_TEMP(_codEvento, servizio, sede, idPersona, dataInizio, dataFine, nuovaDataFine, nota, idUOG, ref db);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Esito = false;
        //        result.NuovoStato = 0;
        //        result.DescrizioneErrore = ex.Message;
        //    }
        //    return result;
        //}

        //private AzioneAutomaticaResult EseguiAzioneAutomaticaContabile_050(int idPersona, List<AttributiAggiuntivi> objModuloValorizzato, ref IncentiviEntities db)
        //{
        //    AzioneAutomaticaResult result = new AzioneAutomaticaResult();
        //    result.Esito = true;
        //    string _codEvento = "05";
        //    try
        //    {
        //        List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

        //        string servizio = null;
        //        string sede = null;
        //        string sezione = null;
        //        string nota = null;
        //        int idUOG = 0;
        //        DateTime dataInizio = DateTime.MinValue;
        //        DateTime dataFine = DateTime.MinValue;

        //        var _servizioObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SERVIZIO").FirstOrDefault();
        //        if (_servizioObj != null)
        //        {
        //            servizio = _servizioObj.Valore;
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_DECORRENZA non trovata");
        //        }

        //        var _sedeObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SEDE").FirstOrDefault();
        //        if (_sedeObj != null)
        //        {
        //            sede = _sedeObj.Valore;
        //        }

        //        var _sezioneObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "SEZIONE").FirstOrDefault();
        //        if (_sezioneObj != null)
        //        {
        //            sezione = _sezioneObj.Valore;
        //        }

        //        if (String.IsNullOrEmpty(sede))
        //        {
        //            throw new Exception("Sede non trovata");
        //        }

        //        if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
        //        {
        //            foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
        //            {
        //                listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
        //            }
        //        }
        //        else
        //        {
        //            listaPiatta.AddRange(objModuloValorizzato.ToList());
        //        }

        //        // adesso in lista piatta c'è o il contenuto di GetListaPiatta oppure l'oggetto standard senza elementi inline
        //        var dtInizio = listaPiatta.Where(w => w.DBRefAttribute == "DATA_DECORRENZA").FirstOrDefault();
        //        if (dtInizio != null)
        //        {
        //            DateTime temp;
        //            string dt = dtInizio.Valore;
        //            if (!String.IsNullOrEmpty(dt))
        //            {
        //                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
        //                    throw new Exception("Errore in conversione DATA_DECORRENZA");
        //                dataInizio = temp;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_DECORRENZA non trovata");
        //        }

        //        var dtFine = listaPiatta.Where(w => w.DBRefAttribute == "DATA_SCADENZA").FirstOrDefault();
        //        if (dtFine != null)
        //        {
        //            DateTime temp;
        //            string dt = dtFine.Valore;
        //            if (!String.IsNullOrEmpty(dt))
        //            {
        //                if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
        //                    throw new Exception("Errore in conversione DATA_SCADENZA");
        //                dataFine = temp;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Errore DATA_SCADENZA non trovata");
        //        }
        //        dataFine = new DateTime(2999, 12, 31);

        //        var notaObj = objModuloValorizzato.Where(w => w.DBRefAttribute == "NOTE").FirstOrDefault();
        //        if (notaObj != null)
        //        {
        //            nota = notaObj.Valore;
        //        }

        //        if (String.IsNullOrEmpty(nota))
        //        {
        //            nota = null;
        //        }

        //        if (dataInizio == DateTime.MinValue || dataFine == DateTime.MinValue)
        //        {
        //            throw new Exception("Errore, le date non sono corrette");
        //        }

        //        XR_SERVIZIO aggiornatoServ = AggiornaUltimo_XR_SERVIZIO(_codEvento, servizio, idPersona, dataInizio, dataFine, ref db);

        //        DateTime nuovaDataFine = new DateTime(2999, 12, 31);

        //        XR_SERVIZIO newItem = Crea_XR_SERVIZIO(_codEvento, servizio, idPersona, dataInizio, nuovaDataFine, ref db);

        //        TRASF_SEDE aggiornatoTrasf = AggiornaUltimo_Trasf_Sede(_codEvento, sede, idPersona, dataInizio, dataFine, nota, ref db);

        //        TRASF_SEDE trasf_sede = Crea_Trasf_Sede(_codEvento, sede, idPersona, dataInizio, dataFine, nuovaDataFine, nota, ref db);

        //        if (!String.IsNullOrEmpty(sezione))
        //        {
        //            var uog = db.UNITAORG.Where(w => w.COD_UNITAORG.Equals(sezione)).FirstOrDefault();
        //            if (uog != null && uog.ID_UNITAORG > 0)
        //            {
        //                idUOG = uog.ID_UNITAORG;
        //            }
        //        }

        //        INCARLAV incarlav = Modifica_INCARLAV(_codEvento, sede, idPersona, 
        //                                                dataInizio, dataFine,
        //                                                nuovaDataFine, idUOG, nota, ref db);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Esito = false;
        //        result.NuovoStato = 0;
        //        result.DescrizioneErrore = ex.Message;
        //    }
        //    return result;
        //}

        private XR_SERVIZIO Crea_XR_SERVIZIO(string cod_evServizio, string servizio, int idPersona, DateTime dta_inizio, DateTime dta_fine, ref IncentiviEntities db)
        {
            // verifica per quella persona e in quella data inizio se ci sono altri record, 
            // per poter calcolare il progressivo
            int presenze = db.XR_SERVIZIO.Count(w => w.ID_PERSONA.Equals(idPersona) && w.DTA_INIZIO == dta_inizio);

            XR_SERVIZIO newItem = new XR_SERVIZIO();
            newItem.ID_XR_SERVIZIO = db.XR_SERVIZIO.GeneraPrimaryKey();
            newItem.COD_EVSERVIZIO = cod_evServizio;
            newItem.ID_PERSONA = idPersona;
            newItem.DTA_INIZIO = dta_inizio;
            newItem.DTA_FINE = dta_fine;
            newItem.COD_USER = UtenteHelper.Matricola();
            newItem.COD_TERMID = Request.UserHostAddress;
            newItem.TMS_TIMESTAMP = DateTime.Now;
            newItem.COD_SERVIZIO = servizio;
            newItem.PROG_EVENTO = presenze;

            db.XR_SERVIZIO.Add(newItem);

            return newItem;
        }

        private TRASF_SEDE Crea_Trasf_Sede(string cod_evServizio, string cod_sede, int idPersona, DateTime dta_inizio, DateTime dta_fine, DateTime newDta_fine, string nota, ref IncentiviEntities db)
        {
            TRASF_SEDE newItem = new TRASF_SEDE();
            newItem.ID_TRASF_SEDE = db.TRASF_SEDE.GeneraPrimaryKey();
            newItem.COD_IMPRESA = "0";
            newItem.COD_EVTRASF = cod_evServizio;
            newItem.ID_PERSONA = idPersona;
            newItem.DTA_INIZIO = dta_inizio;
            newItem.DTA_FINE = newDta_fine;
            newItem.COD_USER = UtenteHelper.Matricola();
            newItem.COD_TERMID = Request.UserHostAddress;
            newItem.TMS_TIMESTAMP = DateTime.Now;
            newItem.NOT_NOTA = nota;
            newItem.COD_SEDE = cod_sede;

            db.TRASF_SEDE.Add(newItem);

            return newItem;
        }

        private XR_VARIAZIONE_TEMP Crea_XR_ASSEGNA_TEMP(string codEvento, string servizio, string cod_sede, int idPersona, DateTime dta_inizio, DateTime dta_fine, DateTime newDta_fine, string nota, int idUnitaOrg, ref IncentiviEntities db)
        {
            // verifica per quella persona e in quella data inizio se ci sono altri record, 
            // per poter calcolare il progressivo
            int presenze = db.XR_VARIAZIONE_TEMP.Count(w => w.ID_PERSONA.Equals(idPersona) && w.DTA_INIZIO == dta_inizio);

            int old_ID_UNITAORG = 0;
            string old_COD_SERVIZIO = "";
            string old_COD_SEDE = "";

            var datiSintesi = db.SINTESI1.Where(w => w.ID_PERSONA.Equals(idPersona)).FirstOrDefault();

            if (datiSintesi == null)
            {
                throw new Exception("Dati anagrafici non trovati");
            }

            old_ID_UNITAORG = datiSintesi.ID_UNITAORG.GetValueOrDefault();
            old_COD_SERVIZIO = datiSintesi.COD_SERVIZIO;
            old_COD_SEDE = datiSintesi.COD_SEDE;

            if (String.IsNullOrEmpty(servizio))
            {
                servizio = old_COD_SERVIZIO;
            }

            if (String.IsNullOrEmpty(cod_sede))
            {
                cod_sede = old_COD_SEDE;
            }

            if (idUnitaOrg == 0)
            {
                idUnitaOrg = old_ID_UNITAORG;
            }

            XR_VARIAZIONE_TEMP newItem = new XR_VARIAZIONE_TEMP();
            newItem.ID_XR_VARIAZIONE_TEMP = db.XR_VARIAZIONE_TEMP.GeneraPrimaryKey();
            newItem.ID_PERSONA = idPersona;
            newItem.COD_EVVARTEMP = codEvento;
            newItem.DTA_INIZIO = dta_inizio;
            newItem.DTA_FINE = newDta_fine;
            newItem.DTA_INIZIO_TEMP = dta_inizio;
            newItem.DTA_FINE_TEMP = dta_fine;
            newItem.ID_UNITAORG_TEMP = idUnitaOrg;
            newItem.COD_SERVIZIO_TEMP = servizio;
            newItem.COD_SEDE_TEMP = cod_sede;
            newItem.ID_UNITAORG = old_ID_UNITAORG;
            newItem.COD_SERVIZIO = old_COD_SERVIZIO;
            newItem.COD_SEDE = old_COD_SEDE;
            newItem.COD_USER = UtenteHelper.Matricola();
            newItem.COD_TERMID = Request.UserHostAddress;
            newItem.TMS_TIMESTAMP = DateTime.Now;
            newItem.NOT_VARIAZIONE = nota;
            newItem.PROG_EVENTO = presenze;


            db.XR_VARIAZIONE_TEMP.Add(newItem);

            return newItem;
        }

        private INCARLAV Modifica_INCARLAV(string cod_evServizio, string cod_sede, int idPersona, DateTime dta_inizio, DateTime dta_fine, DateTime nuovaFine, int id_unitOrg, string nota, ref IncentiviEntities db)
        {
            INCARLAV newItem = null;
            // aggiorna la data fine ultimo record
            try
            {
                var last = db.INCARLAV.Where(w => w.ID_PERSONA == idPersona).OrderByDescending(w => w.DTA_FINE).FirstOrDefault();
                if (last != null)
                {
                    if (last.DTA_INIZIO == dta_inizio)
                    {
                        last.DTA_FINE = dta_inizio;
                    }
                    else
                    {
                        last.DTA_FINE = dta_inizio.AddDays(-1);
                    }
                }

                // crea il nuovo record
                newItem = new INCARLAV();
                newItem.ID_INCARLAV = db.INCARLAV.GeneraPrimaryKey();
                newItem.ID_PERSONA = idPersona;
                newItem.DTA_INIZIO = dta_inizio;
                newItem.ID_UNITAORG = id_unitOrg;
                newItem.ID_IPOPOSITION = null;
                newItem.ID_IPOPERSON = null;
                newItem.ID_POSTOORG = null;
                newItem.COD_MOTIVOORGIN = cod_evServizio;
                newItem.IND_INCPRINC = "Y";
                newItem.DTA_FINE = nuovaFine;
                newItem.PRC_PERCOCCUPAZ = 100.000m;
                newItem.NOT_NOTA = nota;
                newItem.COD_USER = UtenteHelper.Matricola();
                newItem.COD_TERMID = Request.UserHostAddress;
                newItem.TMS_TIMESTAMP = DateTime.Now;

                db.INCARLAV.Add(newItem);

                return newItem;

            }
            catch (Exception ex)
            {

            }

            return newItem;
        }

        private XR_SERVIZIO AggiornaUltimo_XR_SERVIZIO(string cod_evServizio, string servizio, int idPersona, DateTime dta_inizio, DateTime dta_fine, ref IncentiviEntities db)
        {
            XR_SERVIZIO last = null;
            var ordinato = db.XR_SERVIZIO.Where(w => w.ID_PERSONA.Equals(idPersona)).OrderBy(w => w.DTA_FINE).ToList();

            if (ordinato != null && ordinato.Any())
            {
                last = ordinato.LastOrDefault();
                if (last.DTA_FINE > dta_inizio)
                {
                    if (last.DTA_INIZIO == dta_inizio)
                    {
                        last.DTA_FINE = dta_inizio;
                    }
                    else
                    {
                        last.DTA_FINE = dta_inizio.AddDays(-1);
                    }
                }
            }

            return last;
        }

        private TRASF_SEDE AggiornaUltimo_Trasf_Sede(string cod_evServizio, string cod_sede, int idPersona, DateTime dta_inizio, DateTime dta_fine, string nota, ref IncentiviEntities db)
        {
            TRASF_SEDE last = null;
            var ordinato = db.TRASF_SEDE.Where(w => w.ID_PERSONA.Equals(idPersona)).OrderBy(w => w.DTA_FINE).ToList();

            if (ordinato != null && ordinato.Any())
            {
                last = ordinato.LastOrDefault();

                if (last.DTA_INIZIO == dta_inizio)
                {
                    last.DTA_FINE = dta_inizio;
                }
                else
                {
                    last.DTA_FINE = dta_inizio.AddDays(-1);
                }
            }

            return last;
        }

        private XR_VARIAZIONE_TEMP AggiornaUltimo_XR_VARIAZIONE_TEMP(int idPersona, DateTime dta_inizio, ref IncentiviEntities db)
        {
            XR_VARIAZIONE_TEMP last = null;
            var ordinato = db.XR_VARIAZIONE_TEMP.Where(w => w.ID_PERSONA.Equals(idPersona)).OrderBy(w => w.DTA_FINE).ToList();

            if (ordinato != null && ordinato.Any())
            {
                last = ordinato.LastOrDefault();

                if (last.DTA_INIZIO == dta_inizio)
                {
                    last.DTA_FINE = dta_inizio;
                }
                else
                {
                    last.DTA_FINE = dta_inizio.AddDays(-1);
                }
            }

            return last;
        }

        #region AZIONE AUTOMATICA ASSUNZIONE

        private AzioneAutomaticaResult EseguiAzioneAutomaticaAssunzione(XR_DEM_DOCUMENTI doc, ref IncentiviEntities db)
        {
            AzioneAutomaticaResult result = new AzioneAutomaticaResult();
            result.Esito = true;
            result.IdRichiesta = 0;
            result.NuovoStato = doc.Id_Stato;

            try
            {
                int idEvento = 0;
                bool esito = false;

                if (!String.IsNullOrEmpty(doc.CustomDataJSON) && doc.CustomDataJSON != "[]")
                {
                    List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                    var tt = objModuloValorizzato.Where(w => w.Id == "IdEvento").FirstOrDefault();
                    if (tt != null)
                    {
                        esito = int.TryParse(tt.Valore, out idEvento);
                    }

                    if (esito)
                    {
                        result = EseguiAzioneAutomaticaAssunzione_Internal(idEvento, doc, ref db);

                        if (result.Esito)
                        {
                            result.NuovoStato = DematerializzazioneManager.GetNextIdStato(doc.Id_Stato, doc.Id_WKF_Tipologia);
                        }
                        else
                        {
                            throw new Exception(result.DescrizioneErrore);
                        }
                    }
                    else
                    {
                        throw new Exception("Impossibile proseguire informazione evento mancante");
                    }
                }
                else
                {
                    throw new Exception("Impossibile proseguire dati aggiuntivi mancanti");
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.NuovoStato = 0;
                result.DescrizioneErrore = ex.Message;
            }
            return result;
        }

        private AzioneAutomaticaResult EseguiAzioneAutomaticaAssunzione_Internal(int idEvento, XR_DEM_DOCUMENTI doc, ref IncentiviEntities db)
        {
            AzioneAutomaticaResult result = new AzioneAutomaticaResult();
            result.Esito = true;

            try
            {
                var assunzione = db.XR_IMM_IMMATRICOLAZIONI.Where(w => w.ID_EVENTO == idEvento &&
                                                                w.ID_PERSONA == doc.IdPersonaDestinatario).FirstOrDefault();

                if (assunzione != null && !String.IsNullOrEmpty(assunzione.JSON_ASSUNZIONI))
                {
                    AssunzioniVM objAssunzione = JsonConvert.DeserializeObject<AssunzioniVM>(assunzione.JSON_ASSUNZIONI);
                    objAssunzione.DataApprovazione = doc.DataApprovazione;
                    objAssunzione.Avanzamento = 85;

                    string toSave = JsonConvert.SerializeObject(objAssunzione);
                    assunzione.JSON_ASSUNZIONI = toSave;
                }
                else
                {
                    throw new Exception("Impossibile trovare i dati di assunzione");
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.NuovoStato = 0;
                result.DescrizioneErrore = ex.Message;
            }
            return result;
        }

        #endregion

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static void ScriviLogDEM(int idDocumento,
            int idStato,
            string operazione,
            string descrizione)
        {
            try
            {
                IncentiviEntities db = new IncentiviEntities();
                XR_DEM_LOG log = new XR_DEM_LOG();
                log.IdDocumento = idDocumento;
                log.DataOperazione = DateTime.Now;
                log.Operazione = operazione;
                log.Descrizione = descrizione;
                log.Matricola = CommonManager.GetCurrentUserMatricola();
                log.IdStato = idStato;
                db.XR_DEM_LOG.Add(log);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipoDocumento"></param>
        /// <param name="tipologiaDocumentale"></param>
        /// <param name="selezione"></param>
        /// <param name="matricolaEsaminata"></param>
        /// <returns></returns>
        public JsonResult RicalcoloElementiDaAbilitare(string tipoDocumento,    //VSRUO
                                                string tipologiaDocumentale,    //VCON
                                                string selezione,               //TRASFERIMENTO
                                                string matricolaEsaminata)
        {
            /*
                30	ASSEGNAZIONE
                3R	ASSEGNAZIONE TEMPORANEA
                3S	FINE ASSEGNAZIONE TEMPORANEA (anche anticipata)

                3W	TRASFERIMENTO DEFINITIVO
                3T	TRASFERIMENTO TEMPORANEO
                3U	FINE TRASFERIMENTO TEMPORANEO (anche anticipata)
                34	TRASFERIMENTO A DOMANDA

                3K	PROROGA ASSEGNAZIONE
                3Y	PROROGA TRASFERIMENTO

                31	DISTACCO
                3X	FINE DISTACCO

                3Z	VARIAZIONE SEZIONE
             */

            DEM_RicalcoloElementiDaAbilitareResponse result = new DEM_RicalcoloElementiDaAbilitareResponse();
            result.Elementi = new List<DEM_RicalcoloElementiInVistaResponse_Item>();
            result.Messaggio = null;

            List<string> elementiInView = new List<string>();
            elementiInView.Add("tipovariazione");
            elementiInView.Add("data_decorrenza");
            elementiInView.Add("data_scadenza");
            elementiInView.Add("sede");
            elementiInView.Add("servizio");
            elementiInView.Add("sezione");
            elementiInView.Add("trasferimentoadomanda");
            elementiInView.Add("note");
            elementiInView.Add("EccezionePerAutomatismo");
            elementiInView.Add("EccezioneSelezionataNascosta");

            foreach (var e in elementiInView)
            {
                result.Elementi.Add(new DEM_RicalcoloElementiInVistaResponse_Item()
                {
                    NomeElemento = e,
                    Visibile = true,
                    Modificabile = true
                });
            }

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                // reperimento dell'idpersona per la matricola interessata
                int idPersona = CezanneHelper.GetIdPersona(matricolaEsaminata);

                var tipovariazione = result.Elementi.First(w => w.NomeElemento == "tipovariazione");
                var trasferimentoadomanda = result.Elementi.First(w => w.NomeElemento == "trasferimentoadomanda");
                var eccezionePerAutomatismo = result.Elementi.First(w => w.NomeElemento == "EccezionePerAutomatismo");
                var eccezioneSelezionataNascosta = result.Elementi.First(w => w.NomeElemento == "EccezioneSelezionataNascosta");
                var sede = result.Elementi.First(w => w.NomeElemento == "sede");
                var servizio = result.Elementi.First(w => w.NomeElemento == "servizio");
                var sezione = result.Elementi.First(w => w.NomeElemento == "sezione");
                List<string> codEventi = new List<string>();
                XR_VARIAZIONE_TEMP ultimaAssegnazione = null;
                string codiceEvento = String.Empty;

                switch (selezione)
                {
                    case "ASSEGNAZIONE":
                        codiceEvento = "3R";
                        break;
                    case "TRASFERIMENTO":
                        codiceEvento = "3T";
                        break;
                    case "CAMBIOSEZIONE":
                        codiceEvento = "3Z";
                        break;
                }

                tipovariazione.Selezionato = true;
                tipovariazione.ValoreDefault = selezione;
                tipovariazione.CampoObbligatorio = true;
                tipovariazione.Visibile = true;

                // In qualsiasi caso vanno impostati questi dati sia
                // se l'ultima assegnazione non è una 30, 3R o 3S
                // oppure non ci sono assegnazioni                        
                trasferimentoadomanda.Selezionato = false;
                trasferimentoadomanda.ValoreDefault = "";
                trasferimentoadomanda.CampoObbligatorio = false;
                trasferimentoadomanda.Visibile = false;

                // imposta come 3R come assegnazione temporanea, poi sarà il salvataggio 
                // che in base alla data fine calcolerà se è una temporanea o definitiva                        
                eccezionePerAutomatismo.Visibile = false;
                eccezionePerAutomatismo.Selezionato = false;
                eccezionePerAutomatismo.CampoObbligatorio = false;
                eccezionePerAutomatismo.ValoreDefault = codiceEvento;

                eccezioneSelezionataNascosta.Visibile = false;
                eccezioneSelezionataNascosta.Selezionato = false;
                eccezioneSelezionataNascosta.CampoObbligatorio = false;
                eccezioneSelezionataNascosta.ValoreDefault = codiceEvento;

                sede.Selezionato = false;
                sede.CampoObbligatorio = (selezione == "CAMBIOSEZIONE" ? false : true);
                sede.Modificabile = true;
                sede.Visibile = true;

                servizio.Selezionato = false;
                servizio.CampoObbligatorio = (selezione == "ASSEGNAZIONE" ? true : false);
                servizio.Modificabile = false;
                servizio.Visibile = true;

                sezione.Selezionato = false;
                sezione.CampoObbligatorio = (selezione == "CAMBIOSEZIONE" ? true : false);
                sezione.Visibile = true;
                sezione.Modificabile = false;

                codEventi.Add("3R");    //ASSEGNAZIONE TEMPORANEA
                codEventi.Add("3T");    //TRASFERIMENTO TEMPORANEO
                codEventi.Add("3K");    //PROROGA ASSEGNAZIONE
                codEventi.Add("3Y");    //PROROGA TRASFERIMENTO
                ultimaAssegnazione = GetVariazione(idPersona, codEventi);

                // SE ESISTE UNA ASSEGNAZIONE TEMPORANEA ATTIVA O UN
                // TRASFERIMENTO TEMPORANEO ATTIVO
                if (ultimaAssegnazione != null)
                {
                    // in questo caso sarà possibile modificare soltanto la data di scadenza
                    result.RicaricaVista = true;
                    if (ultimaAssegnazione.COD_EVVARTEMP == "3T" || ultimaAssegnazione.COD_EVVARTEMP == "3Y")
                    {
                        result.Messaggio = "E\' già presente un trasferimento temporaneo, sarà possibile modificare soltanto la data di scadenza di tale trasferimento.";
                        eccezionePerAutomatismo.ValoreDefault = "3T";
                        tipovariazione.ValoreDefault = "TRASFERIMENTO";
                    }
                    else
                    {
                        result.Messaggio = "E\' già presente una assegnazione temporanea, sarà possibile modificare soltanto la data di scadenza di tale assegnazione.";
                        eccezionePerAutomatismo.ValoreDefault = "3R";
                    }

                    var data_decorrenza = result.Elementi.First(w => w.NomeElemento == "data_decorrenza");
                    data_decorrenza.Selezionato = false;
                    data_decorrenza.ValoreDefault = ultimaAssegnazione.DTA_INIZIO_TEMP.ToString("dd/MM/yyyy");
                    data_decorrenza.CampoObbligatorio = true;
                    data_decorrenza.Modificabile = false;

                    var data_scadenza = result.Elementi.First(w => w.NomeElemento == "data_scadenza");
                    data_scadenza.Selezionato = false;
                    if (ultimaAssegnazione.DTA_FINE_TEMP.ToString("dd/MM/yyyy") == "31/12/2999")
                    {
                        // se è 31/12/2999 è come mettere null
                        data_scadenza.ValoreDefault = "";
                    }
                    else
                    {
                        // altrimenti imposta la data presente sul db
                        data_scadenza.ValoreDefault = ultimaAssegnazione.DTA_FINE_TEMP.ToString("dd/MM/yyyy");
                    }
                    data_scadenza.CampoObbligatorio = false;

                    sede.ValoreDefault = ultimaAssegnazione.COD_SEDE_TEMP;
                    servizio.ValoreDefault = ultimaAssegnazione.COD_SERVIZIO_TEMP;

                    if (ultimaAssegnazione.ID_UNITAORG_TEMP.HasValue && ultimaAssegnazione.ID_UNITAORG_TEMP.Value > 0)
                    {
                        var _temp = db.UNITAORG.Where(w => w.ID_UNITAORG == ultimaAssegnazione.ID_UNITAORG_TEMP.Value).FirstOrDefault();

                        if (_temp != null)
                        {
                            sezione.ValoreDefault = _temp.COD_UNITAORG;
                        }
                    }
                }
                else
                {
                    // se non ci sono Assegnazioni temporanee attive
                    result.RicaricaVista = false;

                    var data_decorrenza = result.Elementi.First(w => w.NomeElemento == "data_decorrenza");
                    data_decorrenza.Selezionato = false;
                    data_decorrenza.ValoreDefault = DateTime.Today.ToString("dd/MM/yyyy");
                    data_decorrenza.CampoObbligatorio = true;
                    data_decorrenza.Modificabile = true;

                    var data_scadenza = result.Elementi.First(w => w.NomeElemento == "data_scadenza");
                    data_scadenza.Selezionato = false;
                    data_scadenza.ValoreDefault = "31/12/2999";
                    data_scadenza.CampoObbligatorio = false;
                    data_scadenza.Modificabile = false;
                    data_scadenza.Visibile = true;

                    string _codSede = "";
                    string _descSede = "";
                    string _codServizio = "";
                    string _descServizio = "";
                    string _codSezione = "";
                    string _descSezione = "";

                    bool risultato = DematerializzazioneManager.DecodificaDatiJson(
                        matricolaEsaminata,
                        null,
                        ref _codSede,
                        ref _descSede,
                        ref _codServizio,
                        ref _descServizio,
                        ref _codSezione,
                        ref _descSezione);

                    sede.ValoreDefault = _codSede;
                    servizio.ValoreDefault = _codServizio;
                    sezione.ValoreDefault = _codSezione;
                }


                //switch (selezione)
                //{
                //    case "ASSEGNAZIONE":
                //    case "TRASFERIMENTO":
                //    case "CAMBIOSEZIONE":
                //        tipovariazione.Selezionato = true;
                //        tipovariazione.ValoreDefault = selezione;
                //        tipovariazione.CampoObbligatorio = true;
                //        tipovariazione.Visibile = true;

                //        // In qualsiasi caso vanno impostati questi dati sia
                //        // se l'ultima assegnazione non è una 30, 3R o 3S
                //        // oppure non ci sono assegnazioni                        
                //        trasferimentoadomanda.Selezionato = false;
                //        trasferimentoadomanda.ValoreDefault = "";
                //        trasferimentoadomanda.CampoObbligatorio = false;
                //        trasferimentoadomanda.Visibile = false;

                //        // imposta come 3R come assegnazione temporanea, poi sarà il salvataggio 
                //        // che in base alla data fine calcolerà se è una temporanea o definitiva                        
                //        eccezionePerAutomatismo.Visibile = false;
                //        eccezionePerAutomatismo.Selezionato = false;
                //        eccezionePerAutomatismo.CampoObbligatorio = false;
                //        eccezionePerAutomatismo.ValoreDefault = (selezione =="ASSEGNAZIONE" ? "3R" : "3T");

                //        eccezioneSelezionataNascosta.Visibile = false;
                //        eccezioneSelezionataNascosta.Selezionato = false;
                //        eccezioneSelezionataNascosta.CampoObbligatorio = false;
                //        eccezioneSelezionataNascosta.ValoreDefault = (selezione == "ASSEGNAZIONE" ? "3R" : "3T");

                //        sede.Selezionato = false;
                //        sede.CampoObbligatorio = true;
                //        sede.Modificabile = false;
                //        sede.Visibile = true;

                //        servizio.Selezionato = false;
                //        servizio.CampoObbligatorio = (selezione == "ASSEGNAZIONE" ? true : false);
                //        servizio.Modificabile = false;
                //        servizio.Visibile = true;

                //        sezione.Selezionato = false;
                //        sezione.CampoObbligatorio = false;
                //        sezione.Visibile = true;
                //        sezione.Modificabile = false;

                //        codEventi.Add("3R");    //ASSEGNAZIONE TEMPORANEA
                //        codEventi.Add("3T");    //TRASFERIMENTO TEMPORANEO
                //        codEventi.Add("3K");    //PROROGA ASSEGNAZIONE
                //        codEventi.Add("3Y");    //PROROGA TRASFERIMENTO
                //        ultimaAssegnazione = GetVariazione(idPersona, codEventi);

                //        // SE ESISTE UNA ASSEGNAZIONE TEMPORANEA ATTIVA O UN
                //        // TRASFERIMENTO TEMPORANEO ATTIVO
                //        if (ultimaAssegnazione != null)
                //        {
                //            // in questo caso sarà possibile modificare soltanto la data di scadenza
                //            result.RicaricaVista = true;
                //            if (ultimaAssegnazione.COD_EVVARTEMP == "3T" || ultimaAssegnazione.COD_EVVARTEMP == "3Y")
                //            {
                //                result.Messaggio = "E\' già presente un trasferimento temporaneo, sarà possibile modificare soltanto la data di scadenza di tale trasferimento.";
                //                eccezionePerAutomatismo.ValoreDefault = "3T";
                //                tipovariazione.ValoreDefault = "TRASFERIMENTO";
                //            }
                //            else
                //            {
                //                result.Messaggio = "E\' già presente una assegnazione temporanea, sarà possibile modificare soltanto la data di scadenza di tale assegnazione.";
                //                eccezionePerAutomatismo.ValoreDefault = "3R";
                //            }

                //            var data_decorrenza = result.Elementi.First(w => w.NomeElemento == "data_decorrenza");
                //            data_decorrenza.Selezionato = false;
                //            data_decorrenza.ValoreDefault = ultimaAssegnazione.DTA_INIZIO_TEMP.ToString("dd/MM/yyyy");
                //            data_decorrenza.CampoObbligatorio = true;
                //            data_decorrenza.Modificabile = false;

                //            var data_scadenza = result.Elementi.First(w => w.NomeElemento == "data_scadenza");
                //            data_scadenza.Selezionato = false;
                //            if (ultimaAssegnazione.DTA_FINE_TEMP.ToString("dd/MM/yyyy") == "31/12/2999")
                //            {
                //                // se è 31/12/2999 è come mettere null
                //                data_scadenza.ValoreDefault = "";
                //            }
                //            else
                //            {
                //                // altrimenti imposta la data presente sul db
                //                data_scadenza.ValoreDefault = ultimaAssegnazione.DTA_FINE_TEMP.ToString("dd/MM/yyyy");
                //            }
                //            data_scadenza.CampoObbligatorio = false;

                //            sede.ValoreDefault = ultimaAssegnazione.COD_SEDE_TEMP;
                //            servizio.ValoreDefault = ultimaAssegnazione.COD_SERVIZIO_TEMP;

                //            if (ultimaAssegnazione.ID_UNITAORG_TEMP.HasValue && ultimaAssegnazione.ID_UNITAORG_TEMP.Value > 0)
                //            {
                //                var _temp = db.UNITAORG.Where(w => w.ID_UNITAORG == ultimaAssegnazione.ID_UNITAORG_TEMP.Value).FirstOrDefault();

                //                if (_temp != null)
                //                {
                //                    sezione.ValoreDefault = _temp.COD_UNITAORG;
                //                }
                //            }
                //        }
                //        else
                //        {
                //            // se non ci sono Assegnazioni temporanee attive
                //            result.RicaricaVista = false;

                //            var data_decorrenza = result.Elementi.First(w => w.NomeElemento == "data_decorrenza");
                //            data_decorrenza.Selezionato = false;
                //            data_decorrenza.ValoreDefault = DateTime.Today.ToString("dd/MM/yyyy");
                //            data_decorrenza.CampoObbligatorio = true;
                //            data_decorrenza.Modificabile = true;

                //            string _codSede = "";
                //            string _descSede = "";
                //            string _codServizio = "";
                //            string _descServizio = "";
                //            string _codSezione = "";
                //            string _descSezione = "";

                //            bool risultato = DematerializzazioneManager.DecodificaDatiJson(
                //                matricolaEsaminata,
                //                null,
                //                ref _codSede,
                //                ref _descSede,
                //                ref _codServizio,
                //                ref _descServizio,
                //                ref _codSezione,
                //                ref _descSezione);

                //            sede.ValoreDefault = _codSede;
                //            servizio.ValoreDefault = _codServizio;
                //            sezione.ValoreDefault = _codSezione;
                //        }

                //        break;

                //}

            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private XR_VARIAZIONE_TEMP GetVariazione(int idPersona, List<string> codiceEventiFiltro)
        {
            XR_VARIAZIONE_TEMP result = null;

            try
            {
                IncentiviEntities db = new IncentiviEntities();

                // per prima cosa deve controllare se ci sono altre assegnazioni già aperte per questo dipendente
                var variazioni = db.XR_VARIAZIONE_TEMP.Where(w => w.ID_PERSONA == idPersona)
                    .ToList()
                    .OrderByDescending(w => w.TMS_TIMESTAMP)
                    .ToList();

                if (variazioni != null && variazioni.Any())
                {
                    DateTime dataPerTestFine = new DateTime(2999, 12, 31);
                    DateTime dataPerTestGiornoCorrente = DateTime.Now;
                    // prima di tutto verifica se ci sono assegnazioni temporanee attive
                    var ultimaAssegnazione = variazioni.OrderBy(w => w.TMS_TIMESTAMP).Last(w =>
                                                                    codiceEventiFiltro.Contains(w.COD_EVVARTEMP)
                                                                    &&
                                                                    w.DTA_FINE_TEMP.Date != dataPerTestFine &&
                                                                    w.DTA_FINE_TEMP.Date > dataPerTestGiornoCorrente.Date
                                                                    );

                    if (ultimaAssegnazione != null)
                    {
                        result = ultimaAssegnazione;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        /// <summary>
        /// Calcola il codice variazione nel caso in partenza ci fosse un codice relativo ad una assunzione
        /// </summary>
        /// <param name="idPersona"></param>
        /// <param name="dataScadenza"></param>
        /// <returns></returns>
        private string GetNuovoCodiceVariazione_TEMPORANEAPRESENTE(int idPersona, DateTime dataScadenza)
        {
            /*
                30	ASSEGNAZIONE
                3R	ASSEGNAZIONE TEMPORANEA
                3S	FINE ASSEGNAZIONE TEMPORANEA (anche anticipata)

                3W	TRASFERIMENTO DEFINITIVO
                3T	TRASFERIMENTO TEMPORANEO
                3U	FINE TRASFERIMENTO TEMPORANEO (anche anticipata)
                34	TRASFERIMENTO A DOMANDA

                3K	PROROGA ASSEGNAZIONE
                3Y	PROROGA TRASFERIMENTO

                31	DISTACCO
                3X	FINE DISTACCO

                3Z	VARIAZIONE SEZIONE
             */
            string result = string.Empty;
            DateTime definitiva = new DateTime(2999, 12, 31);

            try
            {
                List<string> codEventi = new List<string>();
                codEventi.Add("3R");    //ASSEGNAZIONE TEMPORANEA
                codEventi.Add("3T");    //TRASFERIMENTO TEMPORANEO
                codEventi.Add("3K");    //PROROGA ASSEGNAZIONE
                codEventi.Add("3Y");    //PROROGA TRASFERIMENTO
                XR_VARIAZIONE_TEMP ultimaAssegnazione = GetVariazione(idPersona, codEventi);

                if (ultimaAssegnazione != null)
                {
                    if (dataScadenza.Date == ultimaAssegnazione.DTA_FINE_TEMP.Date)
                    {
                        if (ultimaAssegnazione.COD_EVVARTEMP == "3T" ||
                            ultimaAssegnazione.COD_EVVARTEMP == "3Y")
                        {
                            // se uguale, è un termine assegnazione
                            result = "3U"; // FINE TRASFERIMENTO TEMPORANEO
                        }
                        else
                        {
                            // se uguale, è un termine assegnazione
                            result = "3S"; // FINE ASSEGNAZIONE TEMPORANEA
                        }
                    }
                    else if (dataScadenza.Date < ultimaAssegnazione.DTA_FINE_TEMP.Date)
                    {
                        // termine anticipato
                        if (ultimaAssegnazione.COD_EVVARTEMP == "3T" ||
                            ultimaAssegnazione.COD_EVVARTEMP == "3Y")
                        {
                            // se uguale è un termine assegnazione
                            result = "3U"; // FINE TRASFERIMENTO TEMPORANEO
                        }
                        else
                        {
                            // se uguale è un termine assegnazione
                            result = "3S"; // FINE ASSEGNAZIONE TEMPORANEA
                        }
                    }
                    else if (dataScadenza.Date > ultimaAssegnazione.DTA_FINE_TEMP.Date)
                    {
                        // proroga
                        if (ultimaAssegnazione.COD_EVVARTEMP == "3T" ||
                            ultimaAssegnazione.COD_EVVARTEMP == "3Y")
                        {
                            result = "3Y"; // ulteriore proroga Trasferimento
                        }
                        else
                        {
                            result = "3K"; // Ulteriore proroga Assegnazione
                        }
                    }
                }
                else
                {
                    // se non ci sono assegnazioni allora può essere una nuova assegnazione temporanea
                    // oppure una nuova assegnazione definitiva
                    if (dataScadenza.Date == definitiva.Date)
                    {
                        //30  ASSEGNAZIONE
                        result = "30";
                    }
                    else
                    {
                        //3R ASSEGNAZIONE TEMPORANEA
                        result = "3R";
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        /// <summary>
        /// Metodo chiamato quando la segreteria decide di far 
        /// proseguire una pratica e mandarla in approvazione
        /// </summary>
        /// <param name="idDoc">identificativo univoco del documento su xr_dem_documenti</param>
        /// <param name="nota">Eventuale nota da applicare</param>
        /// <returns></returns>
        public ActionResult ConfermatoDaSegreteria(int idDoc, string nota)
        {
            string txEsito = "";
            try
            {
                IncentiviEntities db = new IncentiviEntities();
                string matricola = CommonHelper.GetCurrentUserMatricola();
                string mioNome = DematerializzazioneManager.GetNominativoByMatricola(matricola);
                int idPersona = CommonHelper.GetCurrentIdPersona();
                CezanneHelper.GetCampiFirma(out var campiFirma);

                var item = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (item != null)
                {
                    txEsito = DematerializzazioneManager.GetNominativoByMatricola(item.MatricolaDestinatario);
                    txEsito += " - " + item.Descrizione.Trim();

                    if (!String.IsNullOrEmpty(nota))
                    {
                        // se è stata inserita una nota, allora va creata una riga nella tabella 
                        // XR_DEM_NOTE
                        XR_DEM_NOTE nuovaNota = new XR_DEM_NOTE()
                        {
                            DATAINSERIMENTO = campiFirma.Timestamp,
                            TESTONOTA = nota,
                            TIPONOTA = (int)XR_DEM_TIPI_NOTE_ENUM.NOTASEGRETERIA,
                            IDDOCUMENTO = idDoc,
                            MATRICOLA = matricola,
                            NOMINATIVO = mioNome
                        };

                        db.XR_DEM_NOTE.Add(nuovaNota);
                    }
                }

                DateTime ora = DateTime.Now;
                item.Id_Stato = (int)StatiDematerializzazioneDocumenti.VisionatoSegreteria;

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = campiFirma.CodTermid,
                    COD_USER = campiFirma.CodUser,
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = item.Id,
                    ID_PERSONA = idPersona,
                    ID_STATO = item.Id_Stato,
                    ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = ora,
                    TMS_TIMESTAMP = campiFirma.Timestamp
                });

                string tipologiaDocumentale = item.Cod_Tipologia_Documentale.Trim();
                int idTipoDoc = item.Id_Tipo_Doc;
                var tipoDoc = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id == idTipoDoc).FirstOrDefault();
                string tipologiaDocumento = tipoDoc.Codice.Trim();

                var comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(tipologiaDocumentale) &&
    w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                if (comportamento == null)
                {
                    throw new Exception("Comportamento non trovato");
                }

                //if (comportamento.AbilitaSigla)
                //{
                //    AzioneResult applicaSiglaRequest = ApplicaSigla(matricola, idDoc);
                //    if (!applicaSiglaRequest.Esito)
                //    {
                //        throw new Exception(applicaSiglaRequest.DescrizioneErrore);
                //    }
                //}

                db.SaveChanges();

                txEsito += " esito: OK";
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        result = "OK",
                        infoAggiuntive = txEsito
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
                        result = "KO",
                        infoAggiuntive = txEsito + " esito: KO " + ex.Message
                    }
                };

            }
        }

        private List<NotaVM> GetNoteDocumento(int idDocumento)
        {
            List<XR_DEM_NOTE> note = new List<XR_DEM_NOTE>();
            List<NotaVM> result = new List<NotaVM>();

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                note = db.XR_DEM_NOTE.Where(w => w.IDDOCUMENTO == idDocumento).ToList();
                var doc = db.XR_DEM_DOCUMENTI.Where(w => w.Id == idDocumento).FirstOrDefault();

                string comm = String.Empty;
                string nominativoUtenteApprovatore = DematerializzazioneManager.GetNominativoByMatricola(doc.MatricolaApprovatore);
                string nominativoUtenteCreatore = DematerializzazioneManager.GetNominativoByMatricola(doc.MatricolaCreatore);
                string nominativoUtenteFirma = DematerializzazioneManager.GetNominativoByMatricola(doc.MatricolaFirma);

                if (note != null && note.Any())
                {
                    foreach (var n in note)
                    {
                        NotaVM nuovo = new NotaVM()
                        {
                            Data = n.DATAINSERIMENTO,
                            Matricola = n.MATRICOLA,
                            Nominativo = n.NOMINATIVO,
                            Testo = n.TESTONOTA,
                            Commento = "",
                            TipoNota = n.TIPONOTA
                        };

                        if (n.TIPONOTA == (int)XR_DEM_TIPI_NOTE_ENUM.NOTAOPERATORE)
                        {
                            nuovo.Commento = "Ha creato il documento";
                        }

                        if (n.TIPONOTA == (int)XR_DEM_TIPI_NOTE_ENUM.NOTAAPPROVATORE)
                        {
                            if (doc.DataApprovazione.HasValue)
                            {
                                nuovo.Commento = "Ha approvato il documento";
                            }
                            else if (doc.DataRifiuto.HasValue)
                            {
                                nuovo.Commento = "Ha rifiutato il documento";
                            }
                        }

                        if (n.TIPONOTA == (int)XR_DEM_TIPI_NOTE_ENUM.NOTAFIRMA)
                        {
                            if (doc.DataFirma.HasValue)
                            {
                                nuovo.Commento = "Ha firmato il documento";
                            }
                            else if (doc.DataFirma.HasValue)
                            {
                                nuovo.Commento = "Ha rifiutato il documento";
                            }
                        }

                        if (n.TIPONOTA == (int)XR_DEM_TIPI_NOTE_ENUM.NOTASEGRETERIA)
                        {
                            // cerca nella operstati per il documento con id iddocumento e il timestamp uguale
                            // a quello della nota con evento 32 quindi passato avanti oppure 20 (tornato indietro)
                            var elemento = db.XR_DEM_WKF_OPERSTATI.Where(w => w.ID_GESTIONE == idDocumento &&
                            w.TMS_TIMESTAMP == n.DATAINSERIMENTO &&
                            (w.ID_STATO == (int)StatiDematerializzazioneDocumenti.VisionatoSegreteria ||
                            w.ID_STATO == (int)StatiDematerializzazioneDocumenti.ProntoVisione)).FirstOrDefault();

                            if (elemento != null)
                            {
                                if (elemento.ID_STATO == (int)StatiDematerializzazioneDocumenti.VisionatoSegreteria)
                                {
                                    nuovo.Commento = "Ha visionato il documento";
                                }
                                else if (elemento.ID_STATO == (int)StatiDematerializzazioneDocumenti.ProntoVisione)
                                {
                                    nuovo.Commento = "Ha portato indietro il documento";
                                }
                            }
                        }

                        result.Add(nuovo);
                    }
                }

                if (!String.IsNullOrEmpty(doc.Note))
                {
                    result.Add(new NotaVM()
                    {
                        Commento = "Ha creato il documento",
                        Data = doc.DataCreazione,
                        Matricola = doc.MatricolaCreatore,
                        Nominativo = nominativoUtenteCreatore,
                        Testo = doc.Note
                    });
                }

                comm = "";
                if (doc.DataApprovazione.HasValue)
                {
                    comm = "Ha approvato il documento";
                }
                else if (doc.DataRifiuto.HasValue)
                {
                    comm = "Ha rifiutato il documento";
                }

                if (!String.IsNullOrEmpty(doc.NotaApprovatore))
                {
                    result.Add(new NotaVM()
                    {
                        Commento = comm,
                        Data = doc.DataApprovazione.HasValue ? doc.DataApprovazione.GetValueOrDefault() : doc.DataRifiuto.GetValueOrDefault(),
                        Matricola = doc.MatricolaApprovatore,
                        Nominativo = nominativoUtenteApprovatore,
                        Testo = doc.NotaApprovatore
                    });
                }

                comm = "";
                if (doc.DataFirma.HasValue)
                {
                    comm = "Ha firmato il documento";
                }
                else if (doc.DataFirma.HasValue)
                {
                    comm = "Ha rifiutato il documento";
                }

                if (!String.IsNullOrEmpty(doc.NotaFirma))
                {
                    result.Add(new NotaVM()
                    {
                        Commento = comm,
                        Data = doc.DataFirma.GetValueOrDefault(),
                        Matricola = doc.MatricolaFirma,
                        Nominativo = nominativoUtenteFirma,
                        Testo = doc.NotaFirma
                    });
                }
            }
            catch (Exception ex)
            {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// Metodo chiamato quando la segreteria decide di  
        /// portare allo stato precedente una pratica
        /// </summary>
        /// <param name="idDoc">identificativo univoco del documento su xr_dem_documenti</param>
        /// <param name="nota">nota da applicare</param>
        /// <returns></returns>
        public ActionResult RimandaVsOperatore(int idDoc, string nota)
        {
            string txEsito = "";
            try
            {
                IncentiviEntities db = new IncentiviEntities();
                string matricola = CommonHelper.GetCurrentUserMatricola();
                string mioNome = DematerializzazioneManager.GetNominativoByMatricola(matricola);
                int idPersona = CommonHelper.GetCurrentIdPersona();
                CezanneHelper.GetCampiFirma(out var campiFirma);

                var item = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (item != null)
                {
                    txEsito = DematerializzazioneManager.GetNominativoByMatricola(item.MatricolaDestinatario);
                    txEsito += " - " + item.Descrizione.Trim();

                    if (!String.IsNullOrEmpty(nota))
                    {
                        // se è stata inserita una nota, allora va creata una riga nella tabella 
                        // XR_DEM_NOTE
                        XR_DEM_NOTE nuovaNota = new XR_DEM_NOTE()
                        {
                            DATAINSERIMENTO = campiFirma.Timestamp,
                            TESTONOTA = nota,
                            TIPONOTA = (int)XR_DEM_TIPI_NOTE_ENUM.NOTASEGRETERIA,
                            IDDOCUMENTO = idDoc,
                            MATRICOLA = matricola,
                            NOMINATIVO = mioNome
                        };

                        db.XR_DEM_NOTE.Add(nuovaNota);
                    }
                }

                DateTime ora = DateTime.Now;
                item.Id_Stato = (int)StatiDematerializzazioneDocumenti.ProntoVisione;

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = campiFirma.CodTermid,
                    COD_USER = campiFirma.CodUser,
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = item.Id,
                    ID_PERSONA = idPersona,
                    ID_STATO = item.Id_Stato,
                    ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = ora,
                    TMS_TIMESTAMP = campiFirma.Timestamp
                });

                db.SaveChanges();

                txEsito += " esito: OK";
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        result = "OK",
                        infoAggiuntive = txEsito
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
                        result = "KO",
                        infoAggiuntive = txEsito + " esito: KO " + ex.Message
                    }
                };

            }
        }

        [HttpPost]
        public ActionResult UpdatePratica(UpdateDocumento toupload)
        {
            IncentiviEntities db = new IncentiviEntities();
            RichiestaDoc richiesta = new RichiestaDoc();
            List<int> idAllegati = new List<int>();
            List<int> idAllegatiEliminati = new List<int>();
            if (!String.IsNullOrEmpty(toupload.Allegati))
            {
                List<string> temp = toupload.Allegati.Split(',').ToList();
                foreach (var t in temp)
                {
                    idAllegati.Add(int.Parse(t));
                }
            }

            if (toupload.IdAllegatoPrincipale != toupload.IdAllegatoPrincipaleOLD)
            {
                idAllegati.Add(toupload.IdAllegatoPrincipale);
            }

            if (!String.IsNullOrEmpty(toupload.AllegatiEliminati))
            {
                List<string> temp = toupload.AllegatiEliminati.Split(',').ToList();
                foreach (var t in temp)
                {
                    idAllegatiEliminati.Add(int.Parse(t));
                }
            }

            try
            {
                if (toupload != null)
                {
                    if (!String.IsNullOrEmpty(toupload.MatricolaApprovatore) &&
                        toupload.MatricolaApprovatore == "-1")
                    {
                        toupload.MatricolaApprovatore = null;
                    }

                    if (!String.IsNullOrEmpty(toupload.MatricolaFirma) &&
                        toupload.MatricolaFirma == "-1")
                    {
                        toupload.MatricolaFirma = null;
                    }

                    if (toupload.Id_Stato == (int)StatiDematerializzazioneDocumenti.Bozza)
                    {
                        toupload.Id_Stato = (int)StatiDematerializzazioneDocumenti.ProntoVisione;
                    }

                    XR_WKF_TIPOLOGIA WKF_Tipologia = null;
                    // calcolo della tipologia
                    if (String.IsNullOrEmpty(toupload.CustomAttrs) || toupload.CustomAttrs == "[]")
                    {
                        WKF_Tipologia = Get_WKF_Tipologia(toupload.Id_WKF_Tipologia);
                    }
                    else
                    {
                        // se i customAttrs sono valorizzati va calcolata la tipologia in base ad eventuali sotto
                        // categorie determinate dall'eccezione selezionata e salvata tra i custom attrs esempio
                        // DEMDOC_VSRUO_CPM, può anche avere una sotto categoria DEMDOC_VSRUO_CPM_ON
                        WKF_Tipologia = Get_WKF_Tipologia(toupload.Id_WKF_Tipologia, toupload.CustomAttrs);


                        // verifica un eventuale cambio di tipologia, ad esempio se un PN che come tipologia ha
                        // DEMDOC_VSRUO_PPP_PN, viene modificato in un GM deve cambiare tipologia WKF in DEMDOC_VSRUO_PPP
                        var _tempWKF = Get_WKF_Tipologia(toupload.Cod_Tipologia_Documentale, toupload.Id_Tipo_Doc, toupload.CustomAttrs);

                        if (_tempWKF != null)
                        {
                            if (_tempWKF.ID_TIPOLOGIA != WKF_Tipologia.ID_TIPOLOGIA)
                            {
                                WKF_Tipologia = _tempWKF;
                            }
                        }
                        else
                        {
                            throw new Exception("Workflow non trovato");
                        }
                    }

                    if (WKF_Tipologia == null)
                    {
                        throw new Exception("Workflow non trovato");
                    }

                    int idArea = 0;
                    try
                    {
                        using (IncentiviEntities dbx = new IncentiviEntities())
                        {
                            if (!String.IsNullOrWhiteSpace(toupload.MatricolaCreatore))
                            {
                                string direzione = dbx.SINTESI1.Where(x => x.COD_MATLIBROMAT == toupload.MatricolaCreatore)
                                                    .Select(x => x.COD_SERVIZIO).FirstOrDefault();
                                if (!String.IsNullOrWhiteSpace(direzione))
                                {
                                    idArea = db.XR_HRIS_DIR_FILTER
                                        .Where(x => x.DIR_INCLUDED != null && x.DIR_INCLUDED.Contains(direzione.Trim()))
                                        .Select(x => x.ID_AREA_FILTER).FirstOrDefault(); //male che va rimane null
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw new Exception("Impossibile recuperare l'IdArea");
                    }

                    richiesta.Documento = new XR_DEM_DOCUMENTI()
                    {
                        Descrizione = toupload.Descrizione,
                        Note = toupload.Note,
                        Id_Stato = toupload.Id_Stato,
                        Id_Tipo_Doc = toupload.Id_Tipo_Doc,
                        MatricolaCreatore = toupload.MatricolaCreatore,
                        IdPersonaCreatore = toupload.IdPersonaCreatore,
                        MatricolaDestinatario = toupload.MatricolaDestinatario,
                        Id_WKF_Tipologia = WKF_Tipologia.ID_TIPOLOGIA,
                        Cod_Tipologia_Documentale = toupload.Cod_Tipologia_Documentale,
                        MatricolaApprovatore = toupload.MatricolaApprovatore,
                        MatricolaFirma = toupload.MatricolaFirma,
                        Id = toupload.Id,
                        CustomDataJSON = toupload.CustomAttrs,
                        IdArea = idArea
                    };

                    int result = SavePraticaInternal(richiesta, idAllegati);

                    if (result == 0)
                    {
                        throw new Exception("Errore nel salvataggio dei dati");
                    }

                    if (toupload.IdAllegatoPrincipale != toupload.IdAllegatoPrincipaleOLD)
                    {
                        // se è diverso è cambiato il file principale ed il vecchio va rimosso
                        bool eliminato = EliminaAllegatoById(toupload.IdAllegatoPrincipaleOLD);
                        if (!eliminato)
                        {
                            throw new Exception("Impossibile eliminare l'allegato");
                        }
                    }

                    if (idAllegatiEliminati != null && idAllegatiEliminati.Any())
                    {
                        foreach (var a in idAllegatiEliminati)
                        {
                            bool eliminato = EliminaAllegatoById(a);
                            if (!eliminato)
                            {
                                throw new Exception("Impossibile eliminare l'allegato");
                            }
                        }
                    }

                    return Json(new { success = true, responseText = result.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = "Errore nell'inserimento della richiesta" }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        private int SavePraticaInternal(RichiestaDoc richiesta, List<int> allegati, bool isBozza = false)
        {
            int result = 0;
            try
            {
                string currentPMatricola = CommonHelper.GetCurrentUserPMatricola();
                IncentiviEntities db = new IncentiviEntities();
                List<XR_DEM_VERSIONI_DOCUMENTO> versioni = new List<XR_DEM_VERSIONI_DOCUMENTO>();

                XR_DEM_DOCUMENTI doc = null;

                if (richiesta.Documento.Id > 0)
                {
                    doc = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(richiesta.Documento.Id)).FirstOrDefault();

                    if (doc == null)
                    {
                        throw new Exception("Errore nel reperimento del documento");
                    }

                    //doc.Note = richiesta.Documento.Note;
                    doc.IdArea = richiesta.Documento.IdArea;
                    doc.MatricolaApprovatore = richiesta.Documento.MatricolaApprovatore;
                    doc.MatricolaFirma = richiesta.Documento.MatricolaFirma;
                    doc.MatricolaIncaricato = richiesta.Documento.MatricolaIncaricato;
                    doc.CustomDataJSON = richiesta.Documento.CustomDataJSON;
                    doc.Id_WKF_Tipologia = richiesta.Documento.Id_WKF_Tipologia;
                    if (doc.Id_Stato < richiesta.Documento.Id_Stato)
                    {
                        doc.Id_Stato = richiesta.Documento.Id_Stato;
                    }
                    if (isBozza)
                    {
                        doc.Id_Stato = (int)StatiDematerializzazioneDocumenti.Bozza;
                    }
                }
                else
                {
                    doc = new XR_DEM_DOCUMENTI()
                    {
                        Descrizione = richiesta.Documento.Descrizione,
                        DataCreazione = DateTime.Now,
                        Id_Stato = richiesta.Documento.Id_Stato,
                        Cod_Tipologia_Documentale = richiesta.Documento.Cod_Tipologia_Documentale,
                        Id_WKF_Tipologia = richiesta.Documento.Id_WKF_Tipologia,
                        MatricolaCreatore = richiesta.Documento.MatricolaCreatore,
                        IdPersonaCreatore = richiesta.Documento.IdPersonaCreatore,
                        MatricolaDestinatario = richiesta.Documento.MatricolaDestinatario,
                        IdPersonaDestinatario = richiesta.Documento.IdPersonaDestinatario,
                        //Note = richiesta.Documento.Note ,
                        Note = null,
                        IdArea = richiesta.Documento.IdArea,
                        Id_Tipo_Doc = richiesta.Documento.Id_Tipo_Doc,
                        MatricolaApprovatore = richiesta.Documento.MatricolaApprovatore,
                        MatricolaFirma = richiesta.Documento.MatricolaFirma,
                        MatricolaIncaricato = richiesta.Documento.MatricolaIncaricato,
                        CustomDataJSON = richiesta.Documento.CustomDataJSON
                    };
                }

                int tempId = 0;
                foreach (var a in allegati)
                {
                    bool exist = db.XR_DEM_ALLEGATI_VERSIONI.Count(w => w.IdAllegato.Equals(a)) > 0;

                    // Se non esiste allora deve creare l'associazione
                    if (!exist)
                    {
                        tempId--;
                        XR_DEM_VERSIONI_DOCUMENTO version = new XR_DEM_VERSIONI_DOCUMENTO()
                        {
                            N_Versione = 1,
                            DataUltimaModifica = DateTime.Now,
                            Id_Documento = doc.Id,
                            Id = tempId
                        };

                        db.XR_DEM_VERSIONI_DOCUMENTO.Add(version);

                        XR_DEM_ALLEGATI_VERSIONI associativa = new XR_DEM_ALLEGATI_VERSIONI()
                        {
                            IdAllegato = a,
                            IdVersione = version.Id
                        };
                        db.XR_DEM_ALLEGATI_VERSIONI.Add(associativa);
                    }
                }

                if (doc.Id == 0)
                {
                    db.XR_DEM_DOCUMENTI.Add(doc);
                    db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                    {
                        COD_TERMID = Request.UserHostAddress,
                        COD_USER = richiesta.Documento.MatricolaCreatore,
                        DTA_OPERAZIONE = 0,
                        COD_TIPO_PRATICA = "DEM",
                        ID_GESTIONE = doc.Id,
                        ID_PERSONA = richiesta.Documento.IdPersonaCreatore,
                        ID_STATO = doc.Id_Stato,
                        ID_TIPOLOGIA = doc.Id_WKF_Tipologia,
                        NOMINATIVO = UtenteHelper.Nominativo(),
                        VALID_DTA_INI = DateTime.Now,
                        TMS_TIMESTAMP = DateTime.Now
                    });
                }
                else
                {
                    db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                    {
                        COD_TERMID = Request.UserHostAddress,
                        COD_USER = richiesta.Documento.MatricolaCreatore,
                        DTA_OPERAZIONE = 0,
                        COD_TIPO_PRATICA = "DEM",
                        ID_GESTIONE = doc.Id,
                        ID_PERSONA = richiesta.Documento.IdPersonaCreatore,
                        ID_STATO = (int)StatiDematerializzazioneDocumenti.DocumentoModificato,
                        ID_TIPOLOGIA = doc.Id_WKF_Tipologia,
                        NOMINATIVO = UtenteHelper.Nominativo(),
                        VALID_DTA_INI = DateTime.Now,
                        TMS_TIMESTAMP = DateTime.Now
                    });
                }

                if (!String.IsNullOrEmpty(doc.CustomDataJSON)
                    && doc.CustomDataJSON != "[]"
                    && DematerializzazioneManager.IsSegreteria())
                {
                    List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                    var tt = objModuloValorizzato.Where(w => w.DBRefAttribute == "NUMERO_PROTOCOLLO").FirstOrDefault();
                    if (tt != null)
                    {
                        if (!String.IsNullOrEmpty(tt.Valore))
                        {
                            doc.NumeroProtocollo = tt.Valore.Trim();
                        }
                    }

                    tt = objModuloValorizzato.Where(w => w.DBRefAttribute == "DATA_PROTOCOLLO").FirstOrDefault();
                    if (tt != null)
                    {
                        if (!String.IsNullOrEmpty(tt.Valore))
                        {
                            DateTime temp;
                            string dt = tt.Valore;
                            if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                throw new Exception("Errore in conversione DATA_PROTOCOLLO");
                            doc.DataProtocollo = temp;
                        }
                    }

                    if (!String.IsNullOrEmpty(doc.NumeroProtocollo))
                    {
                        // a questo punto va cercato il protocollo col numero doc.NumeroProtocollo
                        string codiceProtocollatore = "42936";
                        /*
                         * La ricerca permette di effettuare l’estrazione di un elenco di protocolli 
                         * creati attraverso l’utilizzo dei Web Services.
                         * Su tale ricerca, oltre ad essere filtrata per gli eventuali criteri di 
                         * ricerca applicati (parametro “filtro_ricerca” del metodo), viene applicato 
                         * un filtro di sicurezza che estrae solo i protocolli creati con il ruolo 
                         * di archivista specificato nel parametro “id_ruolo” del metodo.
                         * I criteri di filtro utilizzabili sono quelli descritti nel paragrafo 
                         * “Metodo Esegui Ricerca”.
                         */
                        string id_ricerca = "1502";
                        string top = "1";
                        FiltriRicerca filtri = new FiltriRicerca();
                        filtri.NumeroProtocollo = doc.NumeroProtocollo;

                        string user = "srv_raiconnect_ruo";
                        string pwd = "6E6asOXO";
                        rai_ruo_ws client = new rai_ruo_ws();
                        client.Credentials = new System.Net.NetworkCredential(user, pwd);

                        var _myTemp = db.XR_HRIS_PROTOCOLLI.Where(w => w.CodDema != null).ToList();
                        string _Cod_Tipologia_Documentale = doc.Cod_Tipologia_Documentale.Trim();
                        var item_XR_HRIS_PROTOCOLLI = _myTemp.Where(w => w.CodDema.Split(',').ToList().Contains(_Cod_Tipologia_Documentale)).FirstOrDefault();

                        if (item_XR_HRIS_PROTOCOLLI != null)
                        {
                            codiceProtocollatore = item_XR_HRIS_PROTOCOLLI.CodiceProtocollo;
                        }

                        var cercaCodiceProtocollatore = objModuloValorizzato.Where(w => w.DBRefAttribute == "CODICE_PROTOCOLLATORE").FirstOrDefault();
                        if (cercaCodiceProtocollatore != null)
                        {
                            if (!String.IsNullOrEmpty(cercaCodiceProtocollatore.Valore))
                            {
                                doc.NumeroProtocollo = cercaCodiceProtocollatore.Valore.Trim();
                            }
                        }



                        string serFiltri = SerializerHelper.SerializeObject(filtri);
                        var cercaProt = client.eseguiRicerca(codiceProtocollatore, currentPMatricola, id_ricerca, top, serFiltri);
                        EseguiRicerca outputServizio = new EseguiRicerca();

                        var p = SerializerHelper.DeserializeXml(cercaProt, outputServizio.GetType());
                        outputServizio = (EseguiRicerca)p;

                        if (outputServizio.Errore != null && outputServizio.Errore.IdErrore == "0")
                        {
                            if (outputServizio.Recordset != null
                                && outputServizio.Recordset.Record != null)
                            {
                                if (!String.IsNullOrEmpty(outputServizio.Recordset.Record.IdDocumento))
                                {
                                    doc.Prot_Id_Documento = outputServizio.Recordset.Record.IdDocumento;
                                }
                            }
                        }
                        else
                        {
                            throw new Exception(outputServizio.Errore.Text);
                        }
                    }
                }

                if (!String.IsNullOrEmpty(doc.CustomDataJSON) && doc.CustomDataJSON != "[]")
                {
                    string codiceProtocollatore = "";
                    List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                    var tt = objModuloValorizzato.Where(w => w.Id == "CODICEPROTOCOLLATORE").FirstOrDefault();
                    if (tt != null)
                    {
                        codiceProtocollatore = tt.Valore.Trim();
                        doc.CodiceProtocollatore = codiceProtocollatore;
                    }
                }

                db.SaveChanges();
                result = doc.Id;

                CezanneHelper.GetCampiFirma(out var campiFirma);
                string mioNome = DematerializzazioneManager.GetNominativoByMatricola(richiesta.Documento.MatricolaCreatore);
                if (!String.IsNullOrEmpty(richiesta.Documento.Note))
                {
                    // se è stata inserita una nota, allora va creata una riga nella tabella 
                    // XR_DEM_NOTE
                    XR_DEM_NOTE nuovaNota = new XR_DEM_NOTE()
                    {
                        DATAINSERIMENTO = campiFirma.Timestamp,
                        TESTONOTA = richiesta.Documento.Note,
                        TIPONOTA = (int)XR_DEM_TIPI_NOTE_ENUM.NOTAOPERATORE,
                        IDDOCUMENTO = result,
                        MATRICOLA = richiesta.Documento.MatricolaCreatore,
                        NOMINATIVO = mioNome
                    };

                    db.XR_DEM_NOTE.Add(nuovaNota);
                    db.SaveChanges();
                }

                // VCON
                // se è una variazione contabile deve creare la lettera per il dipendente
                if (doc.Id_Tipo_Doc == 35)
                {
                    DateTime dataInizio = DateTime.MinValue;
                    DateTime dataFine = DateTime.MinValue;
                    string codiceEvento = "";
                    List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();
                    List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                    var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                    if (tt != null)
                    {
                        codiceEvento = tt.Valore;
                    }

                    if (String.IsNullOrEmpty(codiceEvento))
                    {
                        tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                        if (tt != null)
                        {
                            codiceEvento = tt.Valore;
                        }
                    }

                    if (String.IsNullOrEmpty(codiceEvento))
                    {
                        throw new Exception("Impossibile proseguire tipo di variazione mancante");
                    }

                    if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                    {
                        foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
                        {
                            listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
                        }
                    }
                    else
                    {
                        listaPiatta.AddRange(objModuloValorizzato.ToList());
                    }

                    // adesso in lista piatta c'è o il contenuto di GetListaPiatta oppure l'oggetto standard senza elementi inline
                    var dtInizio = listaPiatta.Where(w => w.DBRefAttribute == "INIZIO_GIUSTIFICATIVO").FirstOrDefault();
                    if (dtInizio != null)
                    {
                        DateTime temp;
                        string dt = dtInizio.Valore;
                        if (!String.IsNullOrEmpty(dt))
                        {
                            if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                throw new Exception("Errore in conversione INIZIO_GIUSTIFICATIVO");
                            dataInizio = temp;
                        }
                    }
                    else
                    {
                        throw new Exception("Errore DATA_DECORRENZA non trovata");
                    }

                    var dtFine = listaPiatta.Where(w => w.DBRefAttribute == "FINE_GIUSTIFICATIVO").FirstOrDefault();
                    if (dtFine != null)
                    {
                        DateTime temp;
                        string dt = dtFine.Valore;
                        if (!String.IsNullOrEmpty(dt))
                        {
                            if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                throw new Exception("Errore in conversione FINE_GIUSTIFICATIVO");
                            dataFine = temp;
                        }
                    }
                    else
                    {
                        // #PRESTARE ATTENZIONE potrebbe non essere corretto
                        dataFine = new DateTime(2999, 12, 31);
                        //throw new Exception("Errore DATA_SCADENZA non trovata");
                    }

                    byte[] lettera = DematerializzazioneManager.GeneraPdfLetteraVariazione(doc, codiceEvento, dataInizio, dataFine);
                    string nominativo = DematerializzazioneManager.GetNominativoByMatricola(doc.MatricolaDestinatario);
                    nominativo = nominativo.Trim();
                    nominativo = nominativo.Replace(" ", "_");
                    nominativo = RemoveDiacritics(nominativo);
                    string myTypeFileName = doc.Descrizione.Trim().Replace(" ", "_");
                    string nomeLettera = String.Format("Lettera_{0}_{1}_{2}_{3}.pdf", myTypeFileName, codiceEvento, doc.MatricolaDestinatario, nominativo);

                    #region Crea allegati

                    int length = lettera.Length;
                    string est = Path.GetExtension(nomeLettera);
                    string tipoFile = MimeTypeMap.GetMimeType(est);
                    string jsonStringProtocollo = null;

                    List<PosizioneProtocolloOBJ> objProt = new List<PosizioneProtocolloOBJ>();

                    float protL = 83.5f;
                    float protTop = 112.0f;
                    int pagProt = 1;

                    objProt.Add(new PosizioneProtocolloOBJ()
                    {
                        Oggetto = "Protocollo",
                        PosizioneLeft = protL,
                        PosizioneTop = protTop,
                        NumeroPagina = pagProt
                    });

                    float dataLeft = 123.5f;
                    float dataTop = 158.5f;
                    int dataPagina = 1;

                    objProt.Add(new PosizioneProtocolloOBJ()
                    {
                        Oggetto = "Data",
                        PosizioneLeft = dataLeft,
                        PosizioneTop = dataTop,
                        NumeroPagina = dataPagina
                    });

                    float firmaLeft = 332.5f;
                    float firmaTop = 453.0f;
                    int firmaPagina = 1;

                    objProt.Add(new PosizioneProtocolloOBJ()
                    {
                        Oggetto = "Firma",
                        PosizioneLeft = firmaLeft,
                        PosizioneTop = firmaTop,
                        NumeroPagina = firmaPagina
                    });

                    jsonStringProtocollo = JsonConvert.SerializeObject(objProt);

                    XR_ALLEGATI allegato = new XR_ALLEGATI()
                    {
                        NomeFile = nomeLettera,
                        MimeType = tipoFile,
                        Length = length,
                        ContentByte = lettera,
                        IsPrincipal = true,
                        PosizioneProtocollo = jsonStringProtocollo
                    };

                    db.XR_ALLEGATI.Add(allegato);

                    tempId = -1;

                    XR_DEM_VERSIONI_DOCUMENTO version = new XR_DEM_VERSIONI_DOCUMENTO()
                    {
                        N_Versione = 1,
                        DataUltimaModifica = DateTime.Now,
                        Id_Documento = doc.Id,
                        Id = tempId
                    };

                    db.XR_DEM_VERSIONI_DOCUMENTO.Add(version);

                    XR_DEM_ALLEGATI_VERSIONI associativa = new XR_DEM_ALLEGATI_VERSIONI()
                    {
                        IdAllegato = allegato.Id,
                        IdVersione = version.Id
                    };
                    db.XR_DEM_ALLEGATI_VERSIONI.Add(associativa);

                    db.SaveChanges();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        public static List<SelectListItem> GetVistatoriSelezionabili(int idpersona)
        {
            IncentiviEntities db = new IncentiviEntities();
            string matricola = CommonHelper.GetCurrentUserMatricola();
            List<NominativoMatricola> items = null;
            List<SelectListItem> result = new List<SelectListItem>();

            string tipologiaDocumentale = (string)SessionHelper.Get(matricola + "tipologiaDocumentale");
            string tipologiaDocumento = (string)SessionHelper.Get(matricola + "tipologiaDocumento");
            string tipologia = String.Format("{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());

            items = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

            if (items != null && items.Any())
            {
                foreach (var i in items)
                {
                    string _tempNominativo = "";
                    var r = AuthHelper.EnableToMatr(i.Matricola, matricola, "DEMA", "01VIST");
                    if (r.Enabled)
                    {
                        _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                    }
                    // se non c'è matricola, allora è una tipologia come ad esempio 
                    // APPUNTO, che non ha matricola dipendente, ma ha comunque un vistatore
                    var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                    .Where(w => w.MATRICOLA == i.Matricola && w.IND_ATTIVO)
                    .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                    .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01VIST")
                    .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                    .FirstOrDefault();

                    if (_myAbil != null)
                    {
                        bool includi = false;

                        if (!String.IsNullOrEmpty(_myAbil.MATR_ESCLUSE)
                            && (_myAbil.MATR_ESCLUSE.Contains(matricola) || _myAbil.MATR_ESCLUSE.Contains("*")))
                        {
                            // questo vistatore non ha visibilità sulla matricola corrente
                            includi = false;
                        }

                        if (!String.IsNullOrEmpty(_myAbil.MATR_INCLUSE)
                            && (_myAbil.MATR_INCLUSE.Contains(matricola) || _myAbil.MATR_INCLUSE.Contains("*")))
                        {
                            includi = true;
                        }

                        if (!includi)
                        {
                            continue;
                        }

                        #region POSSO VISTATORE 
                        //Calcolo degli elementi che posso firmare
                        List<string> possoVistare = new List<string>();

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                            && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                        {
                            possoVistare.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                            && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                            && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                        {
                            possoVistare.Add(_myAbil.TIP_DOC_INCLUSI);
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                            && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                        {
                            possoVistare.Add(tipologia);
                        }
                        #endregion

                        #region NON POSSO VISTATORE 
                        //Calcolo degli elementi che non posso vistare
                        List<string> nonPossoVistare = new List<string>();

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                        {
                            nonPossoVistare.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                            && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                        {
                            nonPossoVistare.Add(_myAbil.TIP_DOC_ESCLUSI);
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                        {
                            // se però possoVistare contiene tipologia allora non va aggiunta
                            if (!possoVistare.Contains(tipologia))
                            {
                                nonPossoVistare.Add(tipologia);
                            }
                        }
                        #endregion

                        #region CASO LIMITE TUTTI E DUE *
                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                            !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                                && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                        {
                            possoVistare = new List<string>();
                            nonPossoVistare = new List<string>();
                        }
                        #endregion

                        List<string> tipologieAbilitate = possoVistare.Except(nonPossoVistare).ToList();

                        if (tipologieAbilitate != null &&
                            tipologieAbilitate.Any())
                        {
                            if (tipologieAbilitate.Contains(tipologia))
                            {
                                _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                            }
                            else
                            {
                                _tempNominativo = "";
                            }
                        }
                        else if (nonPossoVistare != null && nonPossoVistare.Any())
                        {
                            if (!nonPossoVistare.Contains(tipologia))
                            {
                                _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(_tempNominativo))
                    {
                        result.Add(new SelectListItem()
                        {
                            Text = _tempNominativo,
                            Value = i.Matricola,
                            Selected = false
                        });
                    }
                }
            }

            // se c'è un solo elemento lo imposta come selezionato
            if (result != null && result.Any() && result.Count == 1)
            {
                result[0].Selected = true;
            }

            return result;
        }

        [HttpPost]
        public ActionResult InsertPratica(InsRicModel model)
        {
            RichiestaDoc richiesta = new RichiestaDoc();
            XR_DEM_TIPIDOC_COMPORTAMENTO comportamento = null;
            List<int> idAllegati = new List<int>();

            if (!String.IsNullOrEmpty(model.Allegati))
            {
                List<string> temp = model.Allegati.Split(',').ToList();
                foreach (var t in temp)
                {
                    idAllegati.Add(int.Parse(t));
                }
            }

            if (model.IdAllegatoPrincipale > 0)
            {
                bool presenteStessoAllegatoPrincipale = idAllegati.Contains(model.IdAllegatoPrincipale);
                if (!presenteStessoAllegatoPrincipale)
                {
                    //elimina il file perchè è stato sostituito
                }
            }

            try
            {
                if (model != null)
                {
                    IncentiviEntities db = new IncentiviEntities();


                    XR_WKF_TIPOLOGIA WKF_Tipologia = null;
                    // calcolo della tipologia
                    if (String.IsNullOrEmpty(model.CustomAttrs) || model.CustomAttrs == "[]")
                    {
                        WKF_Tipologia = Get_WKF_Tipologia(model.TipologiaWKF);
                    }
                    else
                    {
                        // se i customAttrs sono valorizzati va calcolata la tipologia in base ad eventuali sotto
                        // categorie determinate dall'eccezione selezionata e salvata tra i custom attrs esempio
                        // DEMDOC_VSRUO_CPM, può anche avere una sotto categoria DEMDOC_VSRUO_CPM_ON
                        WKF_Tipologia = Get_WKF_Tipologia(model.TipologiaWKF, model.CustomAttrs);
                    }

                    if (WKF_Tipologia == null)
                    {
                        throw new Exception("Workflow non trovato");
                    }

                    int stato = DematerializzazioneManager.GetNextIdStato(0, WKF_Tipologia.ID_TIPOLOGIA);

                    if (stato == 0)
                    {
                        throw new Exception("Errore nel reperimento dello stato successivo del workflow");
                    }

                    XR_DEM_TIPI_DOCUMENTO _tempXR_DEM_TIPI_DOCUMENTO = Get_XR_DEM_TIPI_DOCUMENTO(model.TipologiaDocumento);

                    if (_tempXR_DEM_TIPI_DOCUMENTO == null)
                    {
                        throw new Exception("Tipo documento non trovato");
                    }

                    model.Descrizione = _tempXR_DEM_TIPI_DOCUMENTO.Descrizione;

                    if (!String.IsNullOrEmpty(model.MatricolaApprovatore) &&
                        (model.MatricolaApprovatore == "-1" || model.MatricolaApprovatore == "undefined"))
                    {
                        model.MatricolaApprovatore = null;
                    }

                    if (!String.IsNullOrEmpty(model.IncaricatoFirma) &&
                        (model.IncaricatoFirma == "-1" || model.IncaricatoFirma == "undefined"))
                    {
                        model.IncaricatoFirma = null;
                    }

                    model.IdPersona = CommonHelper.GetCurrentIdPersona();
                    model.Matricola = UtenteHelper.Matricola();

                    int? idPersonaDestinatario = null;

                    if (!String.IsNullOrEmpty(model.MatricolaDestinatario))
                    {
                        idPersonaDestinatario = CezanneHelper.GetIdPersona(model.MatricolaDestinatario);
                    }

                    List<AttributiAggiuntivi> objModuloValorizzato = null;
                    if (!String.IsNullOrEmpty(model.CustomAttrs) && model.CustomAttrs != "[]")
                    {
                        objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(model.CustomAttrs);

                    }

                    if (stato == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInCreazione && !String.IsNullOrEmpty(model.CustomAttrs) && model.CustomAttrs != "[]")
                    {
                        var findRiferimentoRichiestaMaternita = objModuloValorizzato.Where(w => w.DBRefAttribute == "OGGETTO").FirstOrDefault();
                        if (findRiferimentoRichiestaMaternita != null)
                        {
                            model.Descrizione = findRiferimentoRichiestaMaternita.Valore;
                            stato = DematerializzazioneManager.GetNextIdStato((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInCreazione, WKF_Tipologia.ID_TIPOLOGIA);

                            if (stato == 0)
                            {
                                throw new Exception("Errore nel reperimento dello stato successivo del workflow");
                            }
                        }
                    }

                    if (_tempXR_DEM_TIPI_DOCUMENTO.Codice == "VCON" && objModuloValorizzato != null)
                    {
                        #region CREAZIONE DATI JSON

                        int id_Tipo_Doc = 0;
                        string cod_tipo_doc = "";
                        string descrizione = "";
                        const string COD_TIPOLOGIA_DOC = "VSRUO";

                        id_Tipo_Doc = _tempXR_DEM_TIPI_DOCUMENTO.Id;
                        cod_tipo_doc = _tempXR_DEM_TIPI_DOCUMENTO.Codice.Trim();
                        descrizione = _tempXR_DEM_TIPI_DOCUMENTO.Descrizione.Trim();

                        comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(
                            w => w.Codice_Tipologia_Documentale.Equals(COD_TIPOLOGIA_DOC) &&
                            w.Codice_Tipo_Documento.Equals(cod_tipo_doc)).FirstOrDefault();

                        if (comportamento == null)
                        {
                            throw new Exception("Comportamento non trovato");
                        }

                        List<AttributiAggiuntivi> objBASE = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(comportamento.CustomDataJSON);
                        List<AttributiAggiuntivi> objFinale = new List<AttributiAggiuntivi>();

                        foreach (var o in objModuloValorizzato)
                        {
                            AttributiAggiuntivi obj = objBASE.Where(w => w.Id == o.Id).FirstOrDefault();
                            o.TagSINTESI1 = obj.TagSINTESI1;

                            var tipo = o.Tipo;
                            string tag = o.TagSINTESI1;

                            if (!String.IsNullOrEmpty(tag))
                            {
                                switch (tag)
                                {
                                    case "DES_SEDE":
                                        o.Title = DematerializzazioneManager.GetDescrizioneSede(o.Valore);
                                        break;
                                    case "DES_SERVIZIO":
                                        o.Title = DematerializzazioneManager.GetDescrizioneServizio(o.Valore);
                                        break;
                                    case "COD_UNITAORG": // SEZIONE
                                        o.Title = DematerializzazioneManager.GetDescrizioneSezione_UnitaOrganizzativa(o.Valore);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            if (!String.IsNullOrEmpty(o.Id))
                            {
                                switch (o.Id.ToUpper())
                                {
                                    case "INDENNITA":
                                        o.Title = DematerializzazioneManager.GetDescrizioneIndennita(o.Valore);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            o.ValoreInModifica = o.Valore;
                            objFinale.Add(o);
                        }

                        // DEVE CALCOLARE IL GIUSTO CODICE EVENTO IN BASE AI DATI INSERITI
                        // RECUPERO DELLE INFO UTILI AI FINI DEL CALCOLO DELLA TIPOLOGIA DI RICHIESTA
                        var findItem = objModuloValorizzato.Where(w => w.Id.ToUpper() == "ECCEZIONEPERAUTOMATISMO").FirstOrDefault();

                        if (findItem != null)
                        {
                            DateTime dataDefinitiva = new DateTime(2999, 12, 31);

                            //// verifica che tipo di eccezione è stata registrata
                            //if (findItem.Valore == "3R" || findItem.Valore == "3T")
                            //{
                            List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();

                            if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                            {
                                foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
                                {
                                    listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
                                }
                            }

                            var tipoVariazione = objModuloValorizzato.Where(w => w.Id.ToUpper() == "TIPOVARIAZIONE").FirstOrDefault();
                            var data_decorrenza = listaPiatta.Where(w => w.Id.ToUpper() == "DATA_DECORRENZA").FirstOrDefault();
                            var data_scadenza = listaPiatta.Where(w => w.Id.ToUpper() == "DATA_SCADENZA").FirstOrDefault();
                            var automatismoUpdate = objModuloValorizzato.Where(w => w.Id.ToUpper() == "ECCEZIONEPERAUTOMATISMO").FirstOrDefault();
                            var selezionataUpdate = objModuloValorizzato.Where(w => w.Id.ToUpper() == "ECCEZIONESELEZIONATANASCOSTA").FirstOrDefault();

                            if (tipoVariazione == null)
                            {
                                throw new Exception("Tipo variazione non trovata");
                            }

                            if (data_decorrenza == null || String.IsNullOrWhiteSpace(data_decorrenza.Valore))
                            {
                                throw new Exception("Errore nel reperimento della data di decorrenza");
                            }

                            if (data_scadenza == null)
                            {
                                throw new Exception("Errore nel reperimento della data di scadenza");
                            }

                            if (automatismoUpdate == null || String.IsNullOrWhiteSpace(automatismoUpdate.Valore))
                            {
                                throw new Exception("Errore nel reperimento della tipologia di assegnazione");
                            }

                            if (selezionataUpdate == null || String.IsNullOrWhiteSpace(selezionataUpdate.Valore))
                            {
                                throw new Exception("Errore nel reperimento della tipologia di assegnazione");
                            }

                            DateTime dtInizioSelezionata;
                            DateTime? dtFineSelezionata = null;

                            bool conversioneData = DateTime.TryParse(data_decorrenza.Valore, out dtInizioSelezionata);

                            if (!conversioneData)
                            {
                                throw new Exception("Errore in conversione data");
                            }

                            if (String.IsNullOrWhiteSpace(data_scadenza.Valore))
                            {
                                // se non è selezionata allora è definitiva
                                dtFineSelezionata = dataDefinitiva;
                            }
                            else
                            {
                                DateTime _tmpDate;
                                conversioneData = DateTime.TryParse(data_scadenza.Valore, out _tmpDate);
                                if (!conversioneData)
                                {
                                    throw new Exception("Errore in conversione data");
                                }
                                dtFineSelezionata = _tmpDate;
                            }

                            /*
                                30	ASSEGNAZIONE
                                3R	ASSEGNAZIONE TEMPORANEA
                                3S	FINE ASSEGNAZIONE TEMPORANEA (anche anticipata)

                                3W	TRASFERIMENTO DEFINITIVO
                                3T	TRASFERIMENTO TEMPORANEO
                                3U	FINE TRASFERIMENTO TEMPORANEO (anche anticipata)
                                34	TRASFERIMENTO A DOMANDA

                                3K	PROROGA ASSEGNAZIONE
                                3Y	PROROGA TRASFERIMENTO

                                31	DISTACCO
                                3X	FINE DISTACCO

                                3Z	VARIAZIONE SEZIONE
                             */

                            List<string> codEventi = new List<string>();
                            codEventi.Add("3R");    //ASSEGNAZIONE TEMPORANEA
                            codEventi.Add("3T");    //TRASFERIMENTO TEMPORANEO
                            codEventi.Add("3K");    //PROROGA ASSEGNAZIONE
                            codEventi.Add("3Y");    //PROROGA TRASFERIMENTO
                            var ultimaAssegnazione = GetVariazione(idPersonaDestinatario.Value, codEventi);

                            if (ultimaAssegnazione != null)
                            {
                                string nuovoCodice = GetNuovoCodiceVariazione_TEMPORANEAPRESENTE(idPersonaDestinatario.Value, dtFineSelezionata.GetValueOrDefault());
                                automatismoUpdate.Valore = nuovoCodice;
                                automatismoUpdate.ValoreInModifica = nuovoCodice;
                                selezionataUpdate.Valore = nuovoCodice;
                                selezionataUpdate.ValoreInModifica = nuovoCodice;
                            }

                            tipoVariazione.Valore = DematerializzazioneManager.GetDescrizioneEvento(automatismoUpdate.Valore, true);
                            //}
                        }

                        string JSON = JsonConvert.SerializeObject(objFinale);
                        model.CustomAttrs = JSON;
                        #endregion
                    }

                    richiesta.Documento = new XR_DEM_DOCUMENTI()
                    {
                        Descrizione = model.Descrizione,
                        Note = model.Note,
                        Id_Stato = stato,
                        Id_Tipo_Doc = _tempXR_DEM_TIPI_DOCUMENTO.Id,
                        MatricolaCreatore = model.Matricola,
                        IdPersonaCreatore = model.IdPersona,
                        MatricolaDestinatario = model.MatricolaDestinatario,
                        Id_WKF_Tipologia = WKF_Tipologia.ID_TIPOLOGIA,
                        Cod_Tipologia_Documentale = model.TipologiaDocumentale,
                        MatricolaApprovatore = model.MatricolaApprovatore,
                        MatricolaFirma = model.IncaricatoFirma,
                        MatricolaIncaricato = model.MatricolaIncaricato,
                        Id = model.IdDocumento,
                        IdPersonaDestinatario = idPersonaDestinatario,
                        CustomDataJSON = model.CustomAttrs,
                        MatricolaVisualizzatore = model.MatricolaVisionatore
                    };

                    int result = InsertPraticaInternal(richiesta, idAllegati);

                    if (result == 0)
                    {
                        throw new Exception("Errore nel salvataggio dei dati");
                    }

                    int idWKFVIS = 0;
                    int idStatoFinaleVis = (int)StatiDematerializzazioneDocumenti.Visionato;

                    #region Calcolo Comportamento
                    string tipologiaDocumento = "";
                    var dem_TIPI_DOCUMENTO = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id.Equals(richiesta.Documento.Id_Tipo_Doc)).FirstOrDefault();

                    if (dem_TIPI_DOCUMENTO != null)
                    {
                        tipologiaDocumento = dem_TIPI_DOCUMENTO.Codice;
                    }

                    comportamento = db.XR_DEM_TIPIDOC_COMPORTAMENTO.Where(w => w.Codice_Tipologia_Documentale.Equals(richiesta.Documento.Cod_Tipologia_Documentale) &&
w.Codice_Tipo_Documento.Equals(tipologiaDocumento)).FirstOrDefault();

                    if (comportamento == null)
                    {
                        throw new Exception("Comportamento non trovato");
                    }
                    #endregion

                    XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item_SiglaDetails sigla = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item_SiglaDetails();

                    if (comportamento.AbilitaSigla)
                    {
                        sigla.DaApplicare = true;
                    }

                    try
                    {
                        var dynamicsStep = db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Where(x => x.ID_DOCUMENTO.Equals(result)).FirstOrDefault();

                        if (dynamicsStep == null)
                        {
                            // se c'è la matricola vistatore allora deve creare in XR_WKF_WORKFLOW_DYNAMIC_STEPS
                            // dei record per quanti sono i vistatori selezionati dall'operatore in fase di creazione
                            // della pratica. Lo stato di visto è lo stato 30, quindi il sistema deve calcolare
                            // quale stato viene prima dello stato 30 nel workflow principale in XR_WKF_WORKFLOW
                            if (!String.IsNullOrEmpty(model.MatricolaVisionatore))
                            {
                                XR_WKF_WORKFLOW elemento = GetXR_WKF_WORKFLOWPrecedente(WKF_Tipologia.ID_TIPOLOGIA, (int)StatiDematerializzazioneDocumenti.Visionato);
                                if (elemento == null)
                                {
                                    throw new Exception("Non è stato trovato uno stato iniziale");
                                }
                                idWKFVIS = elemento.ID_WORKFLOW;

                                int ordine = 1;

                                // Qui vengono creati gli steps per i vistatori
                                foreach (var v in model.MatricolaVisionatore.Split(',').ToList())
                                {
                                    XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item _myJson = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item()
                                    {
                                        MatricolaVistatore = v,
                                        Vistato = false,
                                        Data = null,
                                        Sigla = sigla
                                    };

                                    XR_WKF_WORKFLOW_DYNAMIC_STEPS newStep = new XR_WKF_WORKFLOW_DYNAMIC_STEPS()
                                    {
                                        ID_WORKFLOW = elemento.ID_WORKFLOW,
                                        ATTIVO = true,
                                        DATACREAZIONE = DateTime.Now,
                                        DATAESECUZIONE = null,
                                        ESEGUITO = false,
                                        ID_DOCUMENTO = result,
                                        ID_STATO_PARTENZA = elemento.ID_STATO,
                                        ID_STATO_ARRIVO = (int)StatiDematerializzazioneDocumenti.Visionato,
                                        JSON_INPUT = JsonConvert.SerializeObject(_myJson),
                                        JSON_OUTPUT = null,
                                        ORDINE = ordine
                                    };
                                    db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Add(newStep);
                                    ordine++;
                                }
                            }
                            else
                            {
                                // **** Questa porzione di Else non serve, poichè la modifica non viene riportata sul db **** 06.09.2023
                                /*
                                 * Se non ci sono matricole vistatori, verifica se il vistatore è previsto
                                 * da XR_DEM_TIPIDOC_COMPORTAMENTO e non dal workflow, se così fosse                         
                                 */
                                if (!String.IsNullOrEmpty(richiesta.Documento.MatricolaDestinatario))
                                {
                                    if (comportamento.Visionatore)
                                    {
                                        List<NominativoMatricola> vistatori = null;
                                        vistatori = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);
                                        foreach (var i in vistatori)
                                        {
                                            var r = AuthHelper.EnableToMatr(i.Matricola, richiesta.Documento.MatricolaCreatore, "DEMA", "01VIST");
                                            if (r.Enabled)
                                            {
                                                if (String.IsNullOrEmpty(richiesta.Documento.MatricolaVisualizzatore))
                                                {
                                                    richiesta.Documento.MatricolaVisualizzatore = i.Matricola;
                                                }
                                                else
                                                {
                                                    richiesta.Documento.MatricolaVisualizzatore += "," + i.Matricola;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            // se c'è la matricola approvatore allora deve creare in XR_WKF_WORKFLOW_DYNAMIC_STEPS
                            // dei record per quanti sono gli approvatori selezionati dall'operatore in fase di creazione
                            // della pratica. Lo stato di approvato è lo stato 40, quindi il sistema deve calcolare
                            // quale stato viene prima dello stato 40 nel workflow principale in XR_WKF_WORKFLOW
                            if (!String.IsNullOrEmpty(model.MatricolaApprovatore))
                            {
                                XR_WKF_WORKFLOW elemento = GetXR_WKF_WORKFLOWPrecedente(WKF_Tipologia.ID_TIPOLOGIA, (int)StatiDematerializzazioneDocumenti.Accettato);
                                if (elemento == null)
                                {
                                    throw new Exception("Non è stato trovato uno stato iniziale");
                                }

                                int id_stato_partenza = elemento.ID_STATO;

                                if (elemento.ID_WORKFLOW == idWKFVIS)
                                {
                                    /*
                                     * se sono uguali allora è un workflow dove il vistatore non è esplicito
                                     * cioè non c'è un vero record nel workflow con lo stato a 30, ma è dovuto
                                     * al fatto che è stato configurato col vistatore obbligatorio ma non previsto
                                     * dal workflow stesso.
                                     * Questo significa che l'id_stato di partenza sarà 30 e non quello di elemento.ID_STATO
                                     */
                                    id_stato_partenza = idStatoFinaleVis;
                                }

                                int ordine = 1;

                                // Qui vengono creati gli steps per gli approvatori
                                foreach (var v in model.MatricolaApprovatore.Split(',').ToList())
                                {
                                    // se false e data null allora non ha fatto nessuna azione, 
                                    // se invece data è valorizzata e approvato è true allora è stato approvato e la data
                                    // indica la data di approvazione, mentre se è false, vuol dire che il documento è 
                                    // stato rifiutato e la data è la data del rifiuto

                                    XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item _myJson = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item()
                                    {
                                        MatricolaApprovatore = v,
                                        Approvato = false,
                                        Data = null,
                                        Sigla = sigla
                                    };

                                    XR_WKF_WORKFLOW_DYNAMIC_STEPS newStep = new XR_WKF_WORKFLOW_DYNAMIC_STEPS()
                                    {
                                        ID_WORKFLOW = elemento.ID_WORKFLOW,
                                        ATTIVO = true,
                                        DATACREAZIONE = DateTime.Now,
                                        DATAESECUZIONE = null,
                                        ESEGUITO = false,
                                        ID_DOCUMENTO = result,
                                        ID_STATO_PARTENZA = id_stato_partenza,
                                        ID_STATO_ARRIVO = (int)StatiDematerializzazioneDocumenti.Accettato,
                                        JSON_INPUT = JsonConvert.SerializeObject(_myJson),
                                        JSON_OUTPUT = null,
                                        ORDINE = ordine
                                    };
                                    db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Add(newStep);
                                    ordine++;
                                }
                            }

                            // se c'è la matricola incaricato firma allora deve creare in XR_WKF_WORKFLOW_DYNAMIC_STEPS
                            // dei record per quanti sono gli incaricati alla firma selezionati dall'operatore 
                            // in fase di creazione della pratica. Lo stato di firmato è lo stato 50, 
                            // quindi il sistema deve calcolare quale stato viene prima dello stato 50 
                            // nel workflow principale in XR_WKF_WORKFLOW
                            if (!String.IsNullOrEmpty(model.IncaricatoFirma))
                            {
                                XR_WKF_WORKFLOW elemento = GetXR_WKF_WORKFLOWPrecedente(WKF_Tipologia.ID_TIPOLOGIA, (int)StatiDematerializzazioneDocumenti.FirmatoDigitalmente);
                                if (elemento == null)
                                {
                                    throw new Exception("Non è stato trovato uno stato iniziale");
                                }

                                int ordine = 1;

                                // Qui vengono creati gli steps per gli incaricati alla firma
                                foreach (var v in model.IncaricatoFirma.Split(',').ToList())
                                {
                                    // se false e data null allora non ha fatto nessuna azione, 
                                    // se invece data è valorizzata e Firmato è true allora è stato Firmato e la data
                                    // indica la data di Firmato, mentre se è false, vuol dire che il documento è 
                                    // stato rifiutato e la data è la data del rifiuto

                                    XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item _myJson = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item()
                                    {
                                        MatricolaFirma = v,
                                        Firmato = false,
                                        Data = null
                                    };

                                    XR_WKF_WORKFLOW_DYNAMIC_STEPS newStep = new XR_WKF_WORKFLOW_DYNAMIC_STEPS()
                                    {
                                        ID_WORKFLOW = elemento.ID_WORKFLOW,
                                        ATTIVO = true,
                                        DATACREAZIONE = DateTime.Now,
                                        DATAESECUZIONE = null,
                                        ESEGUITO = false,
                                        ID_DOCUMENTO = result,
                                        ID_STATO_PARTENZA = elemento.ID_STATO,
                                        ID_STATO_ARRIVO = (int)StatiDematerializzazioneDocumenti.FirmatoDigitalmente,
                                        JSON_INPUT = JsonConvert.SerializeObject(_myJson),
                                        JSON_OUTPUT = null,
                                        ORDINE = ordine
                                    };
                                    db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Add(newStep);
                                    ordine++;
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                    }

                    db.SaveChanges();
                    return Json(new { success = true, responseText = result.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = "Errore nell'inserimento della richiesta" }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        private int InsertPraticaInternal(RichiestaDoc richiesta, List<int> allegati, bool isBozza = false)
        {
            int result = 0;
            try
            {
                IncentiviEntities db = new IncentiviEntities();
                List<XR_DEM_VERSIONI_DOCUMENTO> versioni = new List<XR_DEM_VERSIONI_DOCUMENTO>();

                XR_DEM_DOCUMENTI doc = null;

                if (richiesta.Documento.Id > 0)
                {
                    doc = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(richiesta.Documento.Id)).FirstOrDefault();

                    if (doc == null)
                    {
                        throw new Exception("Errore nel reperimento del documento");
                    }

                    doc.MatricolaApprovatore = richiesta.Documento.MatricolaApprovatore;
                    doc.MatricolaFirma = richiesta.Documento.MatricolaFirma;
                    doc.MatricolaIncaricato = richiesta.Documento.MatricolaIncaricato;
                    doc.CustomDataJSON = richiesta.Documento.CustomDataJSON;
                    doc.Id_WKF_Tipologia = richiesta.Documento.Id_WKF_Tipologia;
                    doc.IdArea = richiesta.Documento.IdArea;
                    if (doc.Id_Stato < richiesta.Documento.Id_Stato)
                    {
                        doc.Id_Stato = richiesta.Documento.Id_Stato;
                    }
                    if (isBozza)
                    {
                        doc.Id_Stato = (int)StatiDematerializzazioneDocumenti.Bozza;
                    }
                }
                else
                {
                    doc = new XR_DEM_DOCUMENTI()
                    {
                        Descrizione = richiesta.Documento.Descrizione,
                        DataCreazione = DateTime.Now,
                        Id_Stato = richiesta.Documento.Id_Stato,
                        Cod_Tipologia_Documentale = richiesta.Documento.Cod_Tipologia_Documentale,
                        Id_WKF_Tipologia = richiesta.Documento.Id_WKF_Tipologia,
                        MatricolaCreatore = richiesta.Documento.MatricolaCreatore,
                        IdPersonaCreatore = richiesta.Documento.IdPersonaCreatore,
                        MatricolaDestinatario = richiesta.Documento.MatricolaDestinatario,
                        IdPersonaDestinatario = richiesta.Documento.IdPersonaDestinatario,
                        Note = null,
                        Id_Tipo_Doc = richiesta.Documento.Id_Tipo_Doc,
                        MatricolaApprovatore = richiesta.Documento.MatricolaApprovatore,
                        MatricolaFirma = richiesta.Documento.MatricolaFirma,
                        MatricolaIncaricato = richiesta.Documento.MatricolaIncaricato,
                        CustomDataJSON = richiesta.Documento.CustomDataJSON,
                        MatricolaVisualizzatore = richiesta.Documento.MatricolaVisualizzatore,
                        IdArea = richiesta.Documento.IdArea
                    };
                }

                int tempId = 0;
                foreach (var a in allegati)
                {
                    bool exist = db.XR_DEM_ALLEGATI_VERSIONI.Count(w => w.IdAllegato.Equals(a)) > 0;

                    // Se non esiste allora deve creare l'associazione
                    if (!exist)
                    {
                        tempId--;
                        XR_DEM_VERSIONI_DOCUMENTO version = new XR_DEM_VERSIONI_DOCUMENTO()
                        {
                            N_Versione = 1,
                            DataUltimaModifica = DateTime.Now,
                            Id_Documento = doc.Id,
                            Id = tempId
                        };

                        db.XR_DEM_VERSIONI_DOCUMENTO.Add(version);

                        XR_DEM_ALLEGATI_VERSIONI associativa = new XR_DEM_ALLEGATI_VERSIONI()
                        {
                            IdAllegato = a,
                            IdVersione = version.Id
                        };
                        db.XR_DEM_ALLEGATI_VERSIONI.Add(associativa);
                    }
                }

                if (doc.Id == 0)
                {
                    db.XR_DEM_DOCUMENTI.Add(doc);
                    db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                    {
                        COD_TERMID = Request.UserHostAddress,
                        COD_USER = richiesta.Documento.MatricolaCreatore,
                        DTA_OPERAZIONE = 0,
                        COD_TIPO_PRATICA = "DEM",
                        ID_GESTIONE = doc.Id,
                        ID_PERSONA = richiesta.Documento.IdPersonaCreatore,
                        ID_STATO = doc.Id_Stato,
                        ID_TIPOLOGIA = doc.Id_WKF_Tipologia,
                        NOMINATIVO = UtenteHelper.Nominativo(),
                        VALID_DTA_INI = DateTime.Now,
                        TMS_TIMESTAMP = DateTime.Now
                    });
                }
                else
                {
                    db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                    {
                        COD_TERMID = Request.UserHostAddress,
                        COD_USER = richiesta.Documento.MatricolaCreatore,
                        DTA_OPERAZIONE = 0,
                        COD_TIPO_PRATICA = "DEM",
                        ID_GESTIONE = doc.Id,
                        ID_PERSONA = richiesta.Documento.IdPersonaCreatore,
                        ID_STATO = (int)StatiDematerializzazioneDocumenti.DocumentoModificato,
                        ID_TIPOLOGIA = doc.Id_WKF_Tipologia,
                        NOMINATIVO = UtenteHelper.Nominativo(),
                        VALID_DTA_INI = DateTime.Now,
                        TMS_TIMESTAMP = DateTime.Now
                    });
                }

                if (!String.IsNullOrEmpty(doc.CustomDataJSON) && doc.CustomDataJSON != "[]")
                {
                    string codiceProtocollatore = "";
                    List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                    var tt = objModuloValorizzato.Where(w => w.Id == "CODICEPROTOCOLLATORE").FirstOrDefault();
                    if (tt != null)
                    {
                        codiceProtocollatore = tt.Valore.Trim();
                        doc.CodiceProtocollatore = codiceProtocollatore;
                    }

                    foreach (var item in objModuloValorizzato)
                    {
                        if (item.Id == "codicefiscalebambino")
                        {
                            item.Valore = item.Valore.Trim();
                        }
                    }

                    doc.CustomDataJSON = JsonConvert.SerializeObject(objModuloValorizzato);
                }

                try
                {
                    if (!string.IsNullOrEmpty(doc.CodiceProtocollatore))
                    {
                        doc.IdArea = db.XR_HRIS_PROTOCOLLI.Where(x => x.CodiceProtocollo == doc.CodiceProtocollatore && x.StrutturaAttiva).Select(k => k.IdArea).FirstOrDefault();
                    }
                    else if (!String.IsNullOrWhiteSpace(doc.MatricolaDestinatario))
                    {
                        string direzione = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == doc.MatricolaDestinatario)
                                            .Select(x => x.COD_SERVIZIO).FirstOrDefault();
                        if (!String.IsNullOrWhiteSpace(direzione))
                        {
                            doc.IdArea = db.XR_HRIS_DIR_FILTER
                                .Where(x => x.DIR_INCLUDED != null && x.DIR_INCLUDED.Contains(direzione.Trim()))
                                .Select(x => x.ID_AREA_FILTER).FirstOrDefault(); //male che va rimane null
                        }
                    }

                }
                catch (Exception)
                {
                    throw new Exception("Impossibile recuperare l'IdArea");
                }

                db.SaveChanges();
                result = doc.Id;

                CezanneHelper.GetCampiFirma(out var campiFirma);
                string mioNome = DematerializzazioneManager.GetNominativoByMatricola(richiesta.Documento.MatricolaCreatore);
                if (!String.IsNullOrEmpty(richiesta.Documento.Note))
                {
                    /*
                     * se il documento è ancora non attivo e si tenta di inserire una nota, allora deve verificare che
                     * non ci siano già delle note presenti, se così fosse le dovrà rimuovere, in quanto in inserimento
                     * è possibile avere una sola nota.
                     */

                    bool giaPresenti = db.XR_DEM_NOTE.Count(w => w.IDDOCUMENTO == result) > 0;
                    if (giaPresenti)
                    {
                        db.XR_DEM_NOTE.RemoveWhere(w => w.IDDOCUMENTO == result);
                    }

                    // se è stata inserita una nota, allora va creata una riga nella tabella 
                    // XR_DEM_NOTE
                    XR_DEM_NOTE nuovaNota = new XR_DEM_NOTE()
                    {
                        DATAINSERIMENTO = campiFirma.Timestamp,
                        TESTONOTA = richiesta.Documento.Note,
                        TIPONOTA = (int)XR_DEM_TIPI_NOTE_ENUM.NOTAOPERATORE,
                        IDDOCUMENTO = result,
                        MATRICOLA = richiesta.Documento.MatricolaCreatore,
                        NOMINATIVO = mioNome
                    };

                    db.XR_DEM_NOTE.Add(nuovaNota);
                    db.SaveChanges();
                }

                // VCON
                // se è una variazione contabile deve creare la lettera per il dipendente
                if (doc.Id_Tipo_Doc == 35)
                {
                    DateTime dataInizio = DateTime.MinValue;
                    DateTime dataFine = DateTime.MinValue;
                    string codiceEvento = "";
                    List<AttributiAggiuntivi> listaPiatta = new List<AttributiAggiuntivi>();
                    List<AttributiAggiuntivi> objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(doc.CustomDataJSON);
                    var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                    if (tt != null)
                    {
                        codiceEvento = tt.Valore;
                    }

                    if (String.IsNullOrEmpty(codiceEvento))
                    {
                        tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                        if (tt != null)
                        {
                            codiceEvento = tt.Valore;
                        }
                    }

                    if (String.IsNullOrEmpty(codiceEvento))
                    {
                        throw new Exception("Impossibile proseguire tipo di variazione mancante");
                    }

                    if (objModuloValorizzato != null && objModuloValorizzato.Count(w => w.InLine != null && w.InLine.Any()) > 0)
                    {
                        foreach (var obj in objModuloValorizzato.Where(w => w.InLine != null && w.InLine.Any()).ToList())
                        {
                            listaPiatta.AddRange(DematerializzazioneManager.GetListaPiatta(obj));
                        }
                    }
                    else
                    {
                        listaPiatta.AddRange(objModuloValorizzato.ToList());
                    }

                    // adesso in lista piatta c'è o il contenuto di GetListaPiatta oppure l'oggetto standard senza elementi inline
                    var dtInizio = listaPiatta.Where(w => w.DBRefAttribute == "INIZIO_GIUSTIFICATIVO").FirstOrDefault();
                    if (dtInizio != null)
                    {
                        DateTime temp;
                        string dt = dtInizio.Valore;
                        if (!String.IsNullOrEmpty(dt))
                        {
                            if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                throw new Exception("Errore in conversione INIZIO_GIUSTIFICATIVO");
                            dataInizio = temp;
                        }
                    }
                    else
                    {
                        throw new Exception("Errore DATA_DECORRENZA non trovata");
                    }

                    var dtFine = listaPiatta.Where(w => w.DBRefAttribute == "FINE_GIUSTIFICATIVO").FirstOrDefault();
                    if (dtFine != null)
                    {
                        DateTime temp;
                        string dt = dtFine.Valore;
                        if (!String.IsNullOrEmpty(dt))
                        {
                            if (!DateTime.TryParseExact(dt, "dd/MM/yyyy", null, DateTimeStyles.None, out temp))
                                throw new Exception("Errore in conversione FINE_GIUSTIFICATIVO");
                            dataFine = temp;
                        }
                    }
                    else
                    {
                        // #PRESTARE ATTENZIONE potrebbe non essere corretto
                        dataFine = new DateTime(2999, 12, 31);
                        //throw new Exception("Errore DATA_SCADENZA non trovata");
                    }

                    byte[] lettera = DematerializzazioneManager.GeneraPdfLetteraVariazione(doc, codiceEvento, dataInizio, dataFine);
                    string nominativo = DematerializzazioneManager.GetNominativoByMatricola(doc.MatricolaDestinatario);
                    nominativo = nominativo.Trim();
                    nominativo = nominativo.Replace(" ", "_");
                    nominativo = RemoveDiacritics(nominativo);
                    string myTypeFileName = doc.Descrizione.Trim().Replace(" ", "_");
                    string nomeLettera = String.Format("Lettera_{0}_{1}_{2}_{3}.pdf", myTypeFileName, codiceEvento, doc.MatricolaDestinatario, nominativo);

                    #region Crea allegati

                    int length = lettera.Length;
                    string est = Path.GetExtension(nomeLettera);
                    string tipoFile = MimeTypeMap.GetMimeType(est);
                    string jsonStringProtocollo = null;

                    List<PosizioneProtocolloOBJ> objProt = new List<PosizioneProtocolloOBJ>();

                    float protL = 83.5f;
                    float protTop = 112.0f;
                    int pagProt = 1;

                    objProt.Add(new PosizioneProtocolloOBJ()
                    {
                        Oggetto = "Protocollo",
                        PosizioneLeft = protL,
                        PosizioneTop = protTop,
                        NumeroPagina = pagProt
                    });

                    float dataLeft = 123.5f;
                    float dataTop = 158.5f;
                    int dataPagina = 1;

                    objProt.Add(new PosizioneProtocolloOBJ()
                    {
                        Oggetto = "Data",
                        PosizioneLeft = dataLeft,
                        PosizioneTop = dataTop,
                        NumeroPagina = dataPagina
                    });

                    float firmaLeft = 332.5f;
                    float firmaTop = 453.0f;
                    int firmaPagina = 1;

                    objProt.Add(new PosizioneProtocolloOBJ()
                    {
                        Oggetto = "Firma",
                        PosizioneLeft = firmaLeft,
                        PosizioneTop = firmaTop,
                        NumeroPagina = firmaPagina
                    });

                    jsonStringProtocollo = JsonConvert.SerializeObject(objProt);

                    XR_ALLEGATI allegato = new XR_ALLEGATI()
                    {
                        NomeFile = nomeLettera,
                        MimeType = tipoFile,
                        Length = length,
                        ContentByte = lettera,
                        IsPrincipal = true,
                        PosizioneProtocollo = jsonStringProtocollo
                    };

                    db.XR_ALLEGATI.Add(allegato);

                    tempId = -1;

                    XR_DEM_VERSIONI_DOCUMENTO version = new XR_DEM_VERSIONI_DOCUMENTO()
                    {
                        N_Versione = 1,
                        DataUltimaModifica = DateTime.Now,
                        Id_Documento = doc.Id,
                        Id = tempId
                    };

                    db.XR_DEM_VERSIONI_DOCUMENTO.Add(version);

                    XR_DEM_ALLEGATI_VERSIONI associativa = new XR_DEM_ALLEGATI_VERSIONI()
                    {
                        IdAllegato = allegato.Id,
                        IdVersione = version.Id
                    };
                    db.XR_DEM_ALLEGATI_VERSIONI.Add(associativa);

                    db.SaveChanges();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private XR_WKF_WORKFLOW GetXR_WKF_WORKFLOWPrecedente(int idTipologia, int statoCercato)
        {
            XR_WKF_WORKFLOW result = null;
            IncentiviEntities db = new IncentiviEntities();

            // cerca stato precedente al 30
            var stati = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA.Equals(idTipologia))
                .OrderBy(w => w.ORDINE)
                .ThenBy(w => w.ID_STATO).ToList();

            if (stati == null)
            {
                throw new Exception("Errore nel reperimento del workflow");
            }

            int ordine = stati.Where(w => w.ID_STATO == statoCercato).Select(w => w.ORDINE).FirstOrDefault();

            IEnumerable<XR_WKF_WORKFLOW> _myItem;
            if (ordine > 1)
            {
                _myItem = stati.Where(w => w.ORDINE == (ordine - 1));
            }
            else
            {
                _myItem = stati.Where(w => w.ORDINE == ordine);
            }

            // da questo elenco vanno esclusi tutti gli stati come rifiutato o azioniautomatiche
            _myItem = _myItem.Where(w => !w.ID_STATO.Equals((int)StatiDematerializzazioneDocumenti.RifiutoApprovatore));
            _myItem = _myItem.Where(w => !w.ID_STATO.Equals((int)StatiDematerializzazioneDocumenti.RifiutatoFirma));
            _myItem = _myItem.Where(w => !w.ID_STATO.Equals((int)StatiDematerializzazioneDocumenti.RifiutatoDalDipendente));
            _myItem = _myItem.Where(w => !w.ID_STATO.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomatica));
            _myItem = _myItem.Where(w => !w.ID_STATO.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaAssunzionePreApprovazione));
            _myItem = _myItem.Where(w => !w.ID_STATO.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaAssunzionePreFirma));
            _myItem = _myItem.Where(w => !w.ID_STATO.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaContabile));
            _myItem = _myItem.Where(w => !w.ID_STATO.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInCreazione));
            _myItem = _myItem.Where(w => !w.ID_STATO.Equals((int)StatiDematerializzazioneDocumenti.AzioneAutomaticaInvioDocumentoAlDipendente));

            result = _myItem.FirstOrDefault();
            if (result == null)
            {
                result = stati.OrderBy(w => w.ORDINE).FirstOrDefault();
            }
            return result;
        }

        public ActionResult DocumentoMarkAsVisualizzato(string matricola, int idPersona, int idDoc)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                bool tuttiVisti = true;
                DateTime ora = DateTime.Now;
                CezanneHelper.GetCampiFirma(out CampiFirma campiFirma);
                string matricolaCorrente = UtenteHelper.Matricola();

                if (idPersona == 0)
                {
                    idPersona = CommonHelper.GetCurrentIdPersona();
                }

                showMessageString = new
                {
                    param1 = 200,
                    param2 = "OK"
                };

                //var db = AnagraficaManager.GetDb( );
                IncentiviEntities db = new IncentiviEntities();
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (item == null)
                {
                    throw new Exception("Richiesta non trovata");
                }

                // prima di tutto prende l'id del workflow corrente, in base allo stato del documento
                var wkf = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA == item.Id_WKF_Tipologia && w.ID_STATO == item.Id_Stato).FirstOrDefault();
                if (wkf == null)
                {
                    throw new Exception("Impossibile reperire il workflow del documento");
                }

                var _wkfSteps = db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Where(w => w.ID_DOCUMENTO == item.Id && w.ATTIVO && w.ID_WORKFLOW == wkf.ID_WORKFLOW).ToList();

                if (_wkfSteps != null && _wkfSteps.Any())
                {
                    foreach (var w in _wkfSteps)
                    {
                        // per ogni elemento prende il json e verifica se tra le matricole 
                        // vistatori c'è la matricola corrente
                        var _stringToJson = w.JSON_INPUT;
                        if (!String.IsNullOrEmpty(_stringToJson))
                        {
                            XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item _json = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item();
                            _json = JsonConvert.DeserializeObject<XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item>(_stringToJson);

                            if (_json != null && !String.IsNullOrEmpty(_json.MatricolaVistatore))
                            {
                                if ((_json.MatricolaVistatore == matricolaCorrente) ||
                                    ISUtenteDelegato(_json.MatricolaVistatore))
                                {
                                    // se lo trova verifica che l'utente non abbia già vistato la pratica
                                    // se così fosse, allora il sistema sarebbe entrato in questa modalità erroneamente
                                    // in quanto una volta vistata la pratica dovrebbe semplicemente dire che
                                    // è stata già vistata, ma il bottone visto dovrebbe essere disabilitato
                                    if (_json.Data.HasValue)
                                    {
                                        throw new Exception("Pratica già vistata in data " + _json.Data.Value.ToString("dd/MM/yyyy HH:mm:ss"));
                                    }
                                    else
                                    {
                                        // se non risulta valorizzata la data, allora l'utente corrente 
                                        // non ha ancora vistato la pratica
                                        // impostiamo i dati che ci interessano e sovrascriviamo il record ed il 
                                        // campo json stesso
                                        _json.Vistato = true;
                                        _json.Data = ora;

                                        if (_json.Sigla != null && _json.Sigla.DaApplicare)
                                        {
                                            AzioneResult applicaSiglaRequest = ApplicaSigla(matricolaCorrente, idDoc);
                                            if (applicaSiglaRequest.Esito)
                                            {
                                                _json.Sigla = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item_SiglaDetails()
                                                {
                                                    DaApplicare = true,
                                                    Applicata = true,
                                                    DataApplicazione = DateTime.Now
                                                };
                                            }
                                        }

                                        w.ESEGUITO = true;
                                        w.DATAESECUZIONE = ora;
                                        string _newJson = JsonConvert.SerializeObject(_json);
                                        w.JSON_INPUT = _newJson;
                                        w.JSON_OUTPUT = _newJson;
                                    }
                                }
                                else
                                {
                                    // se è un vistatore, 
                                    // se eseguito è false oppure non c'è la data esecuzione
                                    // allora marca a false tuttiVisti
                                    // questa variabile servirà più giù perchè se true allora
                                    // deve impostare il documento a visto, perchè tutti i vistatori
                                    // hanno messo la propria sigla.
                                    if (!w.ESEGUITO || !w.DATAESECUZIONE.HasValue)
                                    {
                                        tuttiVisti = false;
                                    }
                                }
                            }
                        }
                    }
                }

                if (tuttiVisti)
                {
                    int nuovoStato = (int)StatiDematerializzazioneDocumenti.Visionato;
                    item.DataLettura = ora;
                    item.DataVisto = ora;
                    item.Id_Stato = nuovoStato;

                    if (_wkfSteps == null || !_wkfSteps.Any())
                    {
                        item.MatricolaVisualizzatore = matricolaCorrente;
                    }
                }

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = campiFirma.CodTermid,
                    COD_USER = campiFirma.CodUser,
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = item.Id,
                    ID_PERSONA = idPersona,
                    ID_STATO = (int)StatiDematerializzazioneDocumenti.Visionato,
                    ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = DateTime.Now,
                    TMS_TIMESTAMP = campiFirma.Timestamp
                });

                db.SaveChanges();

                return Json(showMessageString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404, ex.Message);
            }
        }

        private bool ISUtenteDelegato(string matricolaDelegante)
        {
            bool result = false;

            string matricolaCorrente = UtenteHelper.Matricola();
            DateTime oggi = DateTime.Today;

            /*
             * SELECT *
              FROM [CZNDB].[dbo].[XR_HRIS_DELEGHE]
              where matricola_delegante = '911935'
              and matricola_delegato = '108160'
              and data_inizio <= GEtDATE()
              and data_fine >= GETDATE()
              and attiva = 1
              and esercitata = 1
             */

            using (IncentiviEntities db = new IncentiviEntities())
            {
                result = db.XR_HRIS_DELEGHE.Count(w => w.MATRICOLA_DELEGANTE == matricolaDelegante
                            && w.MATRICOLA_DELEGATO == matricolaCorrente
                            && w.DATA_INIZIO <= oggi
                            && w.DATA_FINE >= oggi
                            && w.ATTIVA
                            && w.ESERCITATA) > 0;
            }

            return result;
        }

        private AzioneResult ApplicaSigla(string matricolaCaller, int idDocumento)
        {
            AzioneResult result = new AzioneResult()
            {
                DescrizioneErrore = null,
                Esito = false
            };

            IncentiviEntities db = new IncentiviEntities();

            try
            {
                byte[] sigla = null;

                // per prima cosa deve recuperare tutti gli allegati del documento
                var allegati = GetAllegati(idDocumento);

                // filtra tutti i file allegati al documento
                // scartando il documento principale e i documenti a supporto
                var daSiglare = allegati.Where(w => w.IsPrincipal).OrderByDescending(w => w.Id).FirstOrDefault();

                if (daSiglare != null)
                {
                    List<PosizioneProtocolloOBJ> obj = JsonConvert.DeserializeObject<List<PosizioneProtocolloOBJ>>(daSiglare.PosizioneProtocollo);
                    List<PosizioneProtocolloOBJ> firmaPos = obj.Where(w => w.Oggetto.Equals("Firma")).ToList();
                    int firmaPosLeft = 16;
                    int firmaPosTop = 760;
                    int pagina = -1;
                    if (firmaPos != null && firmaPos.Any())
                    {
                        for (var idxFirma = 0; idxFirma < firmaPos.Count; idxFirma++)
                        {
                            firmaPosLeft = (int)firmaPos[idxFirma].PosizioneLeft;
                            firmaPosTop = (int)firmaPos[idxFirma].PosizioneTop;
                            pagina = (int)firmaPos[idxFirma].NumeroPagina;
                            if (pagina != -1000)
                            {
                                var d = daSiglare;
                                byte[] nuovoFile = null;
                                PosizioneProtocolloOBJ posizioneSigla = new PosizioneProtocolloOBJ();

                                // verifica se c'è la sigla per la matricola in esame
                                var siglaItem = db.XR_HRIS_FIRME.Where(w => w.Matricola == matricolaCaller && w.Tipologia == "SIG").FirstOrDefault();
                                if (siglaItem != null)
                                {
                                    posizioneSigla = new PosizioneProtocolloOBJ()
                                    {
                                        PosizioneLeft = 545,
                                        PosizioneTop = 0
                                    };

                                    sigla = siglaItem.ContentByte;

                                    if (!String.IsNullOrEmpty(d.PosizioneUltimaSigla))
                                    {
                                        posizioneSigla = JsonConvert.DeserializeObject<PosizioneProtocolloOBJ>(d.PosizioneUltimaSigla);

                                        if ((posizioneSigla.PosizioneLeft - 30) <= 120)
                                        {
                                            // 120 è il margine ipotizzato nel caso di un documento con intestazione
                                            // in basso a sinistra. Nel caso sia arrivato quel margine allora la posizione top
                                            // deve aumentare per andare alla riga sopra e ripartirà da Left 545
                                            posizioneSigla.PosizioneTop += 30;
                                            posizioneSigla.PosizioneLeft = 530;
                                        }
                                        else
                                        {
                                            // sposta la sigla a sinistra
                                            posizioneSigla.PosizioneLeft -= 60;
                                        }
                                    }
                                    else
                                    {
                                        posizioneSigla.PosizioneLeft -= 30;
                                    }

                                    nuovoFile = ApplicaSiglaAlFile(d.ContentByte, sigla, posizioneSigla.PosizioneLeft, posizioneSigla.PosizioneTop, pagina);
                                }
                                else
                                {
                                    posizioneSigla = new PosizioneProtocolloOBJ()
                                    {
                                        PosizioneLeft = 545,
                                        PosizioneTop = 30
                                    };
                                    string sign = String.Empty;
                                    var ut = db.SINTESI1.Where(w => w.COD_MATLIBROMAT == matricolaCaller).FirstOrDefault();
                                    sign = ut.DES_NOMEPERS.Substring(0, 1).ToUpper() + ut.DES_COGNOMEPERS.Substring(0, 1).ToUpper();

                                    if (!String.IsNullOrEmpty(d.PosizioneUltimaSigla))
                                    {
                                        posizioneSigla = JsonConvert.DeserializeObject<PosizioneProtocolloOBJ>(d.PosizioneUltimaSigla);

                                        if ((posizioneSigla.PosizioneLeft - 50) <= 120)
                                        {
                                            // 120 è il margine ipotizzato nel caso di un documento con intestazione
                                            // in basso a sinistra. Nel caso sia arrivato quel margine allora la posizione top
                                            // deve aumentare per andare alla riga sopra e ripartirà da Left 545
                                            posizioneSigla.PosizioneTop += 40;
                                            posizioneSigla.PosizioneLeft = 545;
                                        }
                                        else
                                        {
                                            // sposta la sigla a sinistra
                                            posizioneSigla.PosizioneLeft -= 50;
                                        }
                                    }

                                    nuovoFile = ApplicaSiglaAlFile(d.ContentByte, sign, posizioneSigla.PosizioneLeft, posizioneSigla.PosizioneTop, pagina);
                                }

                                if (nuovoFile != null)
                                {
                                    var toUpdate = db.XR_ALLEGATI.Where(w => w.Id == d.Id).FirstOrDefault();
                                    toUpdate.ContentByte = nuovoFile;
                                    string pos = JsonConvert.SerializeObject(posizioneSigla);
                                    toUpdate.PosizioneUltimaSigla = pos;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }

                result.Esito = true;
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.DescrizioneErrore = ex.Message;
            }

            return result;
        }

        private byte[] ApplicaSiglaAlFile(byte[] allegato, byte[] sigla, float posizioneX, float posizioneY, int pagina)
        {
            IncentiviEntities db = new IncentiviEntities();
            byte[] nuovoAllegato = null;

            try
            {
                using (var reader = new PdfReader(allegato))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Document document = null;
                        document = new Document(reader.GetPageSizeWithRotation(1));

                        var writer = PdfWriter.GetInstance(document, ms);
                        float width = document.PageSize.Width;
                        float height = document.PageSize.Height;

                        document.Open();

                        for (var i = 1; i <= reader.NumberOfPages; i++)
                        {
                            document.NewPage();

                            var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            var importedPage = writer.GetImportedPage(reader, i);

                            PdfContentByte contentByte = writer.DirectContent;

                            var rotation = reader.GetPageRotation(i);

                            switch (rotation)
                            {
                                case 90:
                                    contentByte.AddTemplate(importedPage, 0, -1, 1, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                                    break;
                                // TODO case 180
                                case 270:
                                    contentByte.AddTemplate(importedPage, 0, 1, -1, 0, reader.GetPageSizeWithRotation(i).Width, 0);
                                    break;
                                default:
                                    contentByte.AddTemplate(importedPage, 0, 0);
                                    break;
                            }

                            if (i == pagina)
                            {
                                iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(sigla);
                                png.ScalePercent(17.5f);
                                png.SetAbsolutePosition(posizioneX, posizioneY);
                                contentByte.AddImage(png);
                                contentByte.BeginText();
                                contentByte.SetFontAndSize(baseFont, 12);
                                contentByte.EndText();
                            }
                        }

                        document.Close();
                        writer.Close();
                        nuovoAllegato = ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                nuovoAllegato = null;
            }

            return nuovoAllegato;
        }

        private byte[] ApplicaSiglaAlFile(byte[] allegato, string sigla, float posizioneX, float posizioneY, int pagina)
        {
            IncentiviEntities db = new IncentiviEntities();
            byte[] nuovoAllegato = null;

            const int fontIntestazione = 12;
            const int fontCorpo = 11;
            const int fontNote = 9;

            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
            Font myFontIntestazione = new Font(bf, fontIntestazione, Font.NORMAL);
            Font myFontIntestazioneBold = new Font(bf, fontIntestazione, Font.BOLD);
            Font myFontCorpo = new Font(bf, fontCorpo, Font.NORMAL);
            Font myFontCorpoBold = new Font(bf, fontCorpo, Font.BOLD);
            Font myFontNote = new Font(bf, fontNote, Font.NORMAL);
            Font myFontCorpoSottolineato = new Font(bf, fontCorpo, Font.UNDERLINE);
            Font myFontCorpoItalic = new Font(bf, fontCorpo, Font.ITALIC);
            Font myFontCorpoItalicBold = new Font(bf, fontCorpo, Font.BOLDITALIC);

            try
            {
                using (var reader = new PdfReader(allegato))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Document document = null;
                        document = new Document(reader.GetPageSizeWithRotation(1));

                        var writer = PdfWriter.GetInstance(document, ms);
                        float width = document.PageSize.Width;
                        float height = document.PageSize.Height;

                        document.Open();

                        for (var i = 1; i <= reader.NumberOfPages; i++)
                        {
                            document.NewPage();

                            var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            var importedPage = writer.GetImportedPage(reader, i);

                            PdfContentByte contentByte = writer.DirectContent;

                            var rotation = reader.GetPageRotation(i);

                            switch (rotation)
                            {
                                case 90:
                                    contentByte.AddTemplate(importedPage, 0, -1, 1, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                                    break;
                                // TODO case 180
                                case 270:
                                    contentByte.AddTemplate(importedPage, 0, 1, -1, 0, reader.GetPageSizeWithRotation(i).Width, 0);
                                    break;
                                default:
                                    contentByte.AddTemplate(importedPage, 0, 0);
                                    break;
                            }

                            if (i == pagina)
                            {
                                contentByte.BeginText();
                                contentByte.SetFontAndSize(baseFont, 12);
                                contentByte.ShowTextAligned(1, sigla, posizioneX, posizioneY, 0);
                                contentByte.EndText();
                            }
                        }

                        document.Close();
                        writer.Close();
                        nuovoAllegato = ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                nuovoAllegato = null;
            }

            return nuovoAllegato;
        }

        public ActionResult ApprovaDocumento(string matricola, int idPersona, int idDoc, string nota)
        {
            bool tuttiApprovati = true;
            string txEsito = "";
            DateTime ora = DateTime.Now;
            string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();

            try
            {
                CezanneHelper.GetCampiFirma(out var campiFirma);
                IncentiviEntities db = new IncentiviEntities();

                if (idPersona == 0)
                {
                    idPersona = CommonHelper.GetCurrentIdPersona();
                }

                var item = db.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (item != null)
                {
                    txEsito = DematerializzazioneManager.GetNominativoByMatricola(item.MatricolaDestinatario); ;
                    txEsito += " - " + item.Descrizione.Trim();

                    if (!String.IsNullOrEmpty(nota))
                    {
                        string mioNome = DematerializzazioneManager.GetNominativoByMatricola(CommonHelper.GetCurrentUserMatricola());

                        // se è stata inserita una nota, allora va creata una riga nella tabella 
                        // XR_DEM_NOTE
                        XR_DEM_NOTE nuovaNota = new XR_DEM_NOTE()
                        {
                            DATAINSERIMENTO = campiFirma.Timestamp,
                            TESTONOTA = nota,
                            TIPONOTA = (int)XR_DEM_TIPI_NOTE_ENUM.NOTAAPPROVATORE,
                            IDDOCUMENTO = idDoc,
                            MATRICOLA = CommonHelper.GetCurrentUserMatricola(),
                            NOMINATIVO = mioNome
                        };

                        db.XR_DEM_NOTE.Add(nuovaNota);
                    }
                }

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = campiFirma.CodTermid,
                    COD_USER = campiFirma.CodUser,
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = item.Id,
                    ID_PERSONA = idPersona,
                    ID_STATO = item.Id_Stato,
                    ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = DateTime.Now,
                    TMS_TIMESTAMP = DateTime.Now
                });

                // NOTA IN QUESTO MOMENTO NON è GESTITA LA MULTI APPROVAZIONE 
                // QUINDI LO STATO PASSERà AD APPROVATO IN AUTOMATICO
                // NEL CASO CI FOSSERO PIù RECORD NELLA TABELLA XR_WKF_WORKFLOW_DYNAMIC_STEPS
                // ALLORA ANDRà GESTITO COME IL VISTATORE, SOLO QUANDO TUTTI GLI APPROVATORI
                // AVRANNO APPROVATO LA PRATICA QUESTA POTRà PASSARE ALLO STATO SUCCESSIVO
                var wkf = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA == item.Id_WKF_Tipologia && w.ID_STATO == item.Id_Stato).FirstOrDefault();
                List<XR_WKF_WORKFLOW_DYNAMIC_STEPS> _wkfSteps = new List<XR_WKF_WORKFLOW_DYNAMIC_STEPS>();

                if (wkf == null)
                {
                    /*
                     * Se non trova nulla può essere dovuto al fatto che lo stato in cui si 
                     * trova il documento non sia dichiarato nel workflow, questo capita ad 
                     * esempio per lo stato vistato, perchè alcune tipologie non hanno nel
                     * workflow questo stato, ma viene abilitato attraverso la tabella dei
                     * comportamenti.
                     */
                    _wkfSteps = db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Where(w => w.ID_DOCUMENTO == item.Id && w.ATTIVO).ToList();
                }
                else
                {
                    _wkfSteps = db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Where(w => w.ID_DOCUMENTO == item.Id && w.ATTIVO && w.ID_WORKFLOW == wkf.ID_WORKFLOW).ToList();
                }


                if (_wkfSteps != null && _wkfSteps.Any())
                {
                    foreach (var w in _wkfSteps)
                    {
                        // per ogni elemento prende il json e verifica se tra le matricole approvatore
                        // c'è la matricola corrente
                        var _stringToJson = w.JSON_INPUT;
                        if (!String.IsNullOrEmpty(_stringToJson))
                        {
                            XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item _json = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item();
                            _json = JsonConvert.DeserializeObject<XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item>(_stringToJson);

                            if (_json != null && !String.IsNullOrEmpty(_json.MatricolaApprovatore))
                            {
                                if ((_json.MatricolaApprovatore == matricolaCorrente) ||
                                    ISUtenteDelegato(_json.MatricolaApprovatore))
                                {
                                    // se lo trova verifica che l'utente non abbia già approvato la pratica
                                    // se così fosse, allora il sistema sarebbe entrato in questa modalità erroneamente
                                    // in quanto una volta approvata la pratica dovrebbe semplicemente dire che
                                    // è stata già approvata, ma il bottone approva dovrebbe essere disabilitato
                                    if (_json.Data.HasValue)
                                    {
                                        throw new Exception("Pratica già approvata in data " + _json.Data.Value.ToString("dd/MM/yyyy HH:mm:ss"));
                                    }
                                    else
                                    {
                                        // non ha ancora approvato la pratica
                                        // impostiamo i dati che ci interessano e sovrascriviamo il record ed il 
                                        // campo json stesso
                                        _json.Approvato = true;
                                        _json.Data = ora;
                                        if (_json.Sigla != null && _json.Sigla.DaApplicare)
                                        {
                                            AzioneResult applicaSiglaRequest = ApplicaSigla(matricolaCorrente, idDoc);
                                            if (applicaSiglaRequest.Esito)
                                            {
                                                _json.Sigla = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item_SiglaDetails()
                                                {
                                                    DaApplicare = true,
                                                    Applicata = true,
                                                    DataApplicazione = DateTime.Now
                                                };
                                            }
                                        }
                                        w.ESEGUITO = true;
                                        w.DATAESECUZIONE = ora;
                                        string _newJson = JsonConvert.SerializeObject(_json);
                                        w.JSON_INPUT = _newJson;
                                        w.JSON_OUTPUT = _newJson;
                                    }
                                }
                                else
                                {
                                    // se è un approvatore, 
                                    // se eseguito è false oppure non c'è la data esecuzione
                                    // allora marca a false tuttiApprovati
                                    // questa variabile servirà più giù, perchè se true allora
                                    // deve impostare il documento ad approvato, perchè tutti gli approvatori
                                    // hanno messo la propria sigla.
                                    if (!w.ESEGUITO || !w.DATAESECUZIONE.HasValue)
                                    {
                                        tuttiApprovati = false;
                                    }
                                    else if (w.ESEGUITO && w.DATAESECUZIONE.HasValue)
                                    {
                                        // deve verificare se non sia stato rifiutato
                                        if (!_json.Approvato)
                                        {
                                            tuttiApprovati = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (tuttiApprovati)
                {
                    item.DataApprovazione = ora;
                    int nuovoStato = DematerializzazioneManager.GetNextIdStato(item.Id_Stato, item.Id_WKF_Tipologia);
                    if (nuovoStato == (int)StatiDematerializzazioneDocumenti.AzioneAutomatica)
                    {
                        var esAutomatica = EseguiAzioneAutomatica(nuovoStato, item);

                        if (esAutomatica.Esito)
                        {
                            nuovoStato = esAutomatica.NuovoStato;
                            if (esAutomatica.IdRichiesta > 0)
                            {
                                item.Id_Richiesta = esAutomatica.IdRichiesta;
                            }
                        }
                        else
                        {
                            throw new Exception(esAutomatica.DescrizioneErrore);
                        }

                        if (nuovoStato == 0)
                        {
                            if (!String.IsNullOrEmpty(esAutomatica.DescrizioneErrore))
                            {
                                throw new Exception(esAutomatica.DescrizioneErrore);
                            }
                            else
                            {
                                throw new Exception("Errore durante l'esecuzione dell'azione automatica");
                            }
                        }
                    }
                    else if (nuovoStato == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaContabile)
                    {
                        if (!item.IdPersonaDestinatario.HasValue)
                        {
                            item.IdPersonaDestinatario = CezanneHelper.GetIdPersona(item.MatricolaDestinatario);
                        }
                        item.Id_Stato = nuovoStato;

                        var esAutomatica = EseguiAzioneAutomaticaContabile(item, ref db);

                        if (esAutomatica.Esito)
                        {
                            nuovoStato = esAutomatica.NuovoStato;
                        }
                        else
                        {
                            throw new Exception(esAutomatica.DescrizioneErrore);
                        }
                    }
                    else if (nuovoStato == (int)StatiDematerializzazioneDocumenti.AzioneAutomaticaAssunzionePreApprovazione)
                    {
                        item.Id_Stato = nuovoStato;

                        var esAutomatica = EseguiAzioneAutomaticaAssunzione(item, ref db);

                        if (esAutomatica.Esito)
                        {
                            nuovoStato = esAutomatica.NuovoStato;
                        }
                        else
                        {
                            throw new Exception(esAutomatica.DescrizioneErrore);
                        }
                    }
                    item.Id_Stato = nuovoStato;
                }
                db.SaveChanges();

                txEsito += " esito: OK";
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        result = "OK",
                        infoAggiuntive = txEsito
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
                        result = "KO",
                        infoAggiuntive = txEsito + " esito: KO " + ex.Message
                    }
                };

            }
        }

        public ActionResult RifiutaDocumento(string matricola, int idPersona, int idDoc, string motivo)
        {
            string txEsito = "";
            DateTime ora = DateTime.Now;
            string matricolaCorrente = CommonHelper.GetCurrentUserMatricola();
            try
            {
                IncentiviEntities db = new IncentiviEntities();
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (!String.IsNullOrEmpty(motivo))
                {
                    string mioNome = DematerializzazioneManager.GetNominativoByMatricola(CommonHelper.GetCurrentUserMatricola());
                    //item.NotaApprovatore = nota;
                    CezanneHelper.GetCampiFirma(out var campiFirma);

                    // se è stata inserita una nota, allora va creata una riga nella tabella 
                    // XR_DEM_NOTE
                    XR_DEM_NOTE nuovaNota = new XR_DEM_NOTE()
                    {
                        DATAINSERIMENTO = campiFirma.Timestamp,
                        TESTONOTA = motivo,
                        TIPONOTA = (int)XR_DEM_TIPI_NOTE_ENUM.NOTAAPPROVATORE,
                        IDDOCUMENTO = idDoc,
                        MATRICOLA = CommonHelper.GetCurrentUserMatricola(),
                        NOMINATIVO = mioNome
                    };

                    db.XR_DEM_NOTE.Add(nuovaNota);
                }

                txEsito = DematerializzazioneManager.GetNominativoByMatricola(item.MatricolaDestinatario);
                txEsito += " - " + item.Descrizione.Trim();

                db.XR_DEM_WKF_OPERSTATI.Add(new XR_DEM_WKF_OPERSTATI()
                {
                    COD_TERMID = Request.UserHostAddress,
                    COD_USER = UtenteHelper.Matricola(),
                    DTA_OPERAZIONE = 0,
                    COD_TIPO_PRATICA = "DEM",
                    ID_GESTIONE = item.Id,
                    ID_PERSONA = CommonHelper.GetCurrentIdPersona(),
                    ID_STATO = item.Id_Stato,
                    ID_TIPOLOGIA = item.Id_WKF_Tipologia,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    VALID_DTA_INI = DateTime.Now,
                    TMS_TIMESTAMP = ora
                });


                // NOTA IN QUESTO MOMENTO NON è GESTITA LA MULTI APPROVAZIONE 
                // QUINDI LO STATO PASSERà AD APPROVATO IN AUTOMATICO
                // NEL CASO CI FOSSERO PIù RECORD NELLA TABELLA XR_WKF_WORKFLOW_DYNAMIC_STEPS
                // ALLORA ANDRà GESTITO COME IL VISTATORE, SOLO QUANDO TUTTI GLI APPROVATORI
                // AVRANNO APPROVATO LA PRATICA QUESTA POTRà PASSARE ALLO STATO SUCCESSIVO
                var wkf = db.XR_WKF_WORKFLOW.Where(w => w.ID_TIPOLOGIA == item.Id_WKF_Tipologia && w.ID_STATO == item.Id_Stato).FirstOrDefault();
                if (wkf == null)
                {
                    throw new Exception("Impossibile reperire il workflow del documento");
                }

                var _wkfSteps = db.XR_WKF_WORKFLOW_DYNAMIC_STEPS.Where(w => w.ID_DOCUMENTO == item.Id && w.ATTIVO && w.ID_WORKFLOW == wkf.ID_WORKFLOW).ToList();

                if (_wkfSteps != null && _wkfSteps.Any())
                {
                    foreach (var w in _wkfSteps)
                    {
                        // per ogni elemento prende il json e verifica se tra le matricole approvatore
                        // c'è la matricola corrente
                        var _stringToJson = w.JSON_INPUT;
                        if (!String.IsNullOrEmpty(_stringToJson))
                        {
                            XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item _json = new XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item();
                            _json = JsonConvert.DeserializeObject<XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item>(_stringToJson);

                            if (_json != null && !String.IsNullOrEmpty(_json.MatricolaApprovatore))
                            {
                                if (_json.MatricolaApprovatore == matricolaCorrente)
                                {
                                    // se lo trova verifica che l'utente non abbia già approvato la pratica
                                    // se così fosse, allora il sistema sarebbe entrato in questa modalità erroneamente
                                    // in quanto una volta approvata la pratica dovrebbe semplicemente dire che
                                    // è stata già approvata, ma il bottone approva dovrebbe essere disabilitato
                                    if (_json.Data.HasValue)
                                    {
                                        throw new Exception("Pratica già approvata in data " + _json.Data.Value.ToString("dd/MM/yyyy HH:mm:ss"));
                                    }
                                    else
                                    {
                                        // non ha ancora approvato la pratica
                                        // impostiamo i dati che ci interessano e sovrascriviamo il record ed il 
                                        // campo json stesso
                                        _json.Approvato = false;
                                        _json.Data = ora;
                                        w.ESEGUITO = true;
                                        w.DATAESECUZIONE = ora;
                                        string _newJson = JsonConvert.SerializeObject(_json);
                                        w.JSON_INPUT = _newJson;
                                        w.JSON_OUTPUT = _newJson;
                                    }
                                }
                            }
                        }
                    }
                }

                item.Id_Stato = (int)StatiDematerializzazioneDocumenti.RifiutoApprovatore;
                item.DataApprovazione = ora;
                db.SaveChanges();

                txEsito += " esito: OK";

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        result = "OK",
                        infoAggiuntive = txEsito
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
                        result = "KO",
                        infoAggiuntive = txEsito + " esito: KO " + ex.Message
                    }
                };
            }
        }

        [HttpGet]
        public JsonResult GetVistatoriSelezionabiliJSON()
        {
            IncentiviEntities db = new IncentiviEntities();
            string matricola = CommonHelper.GetCurrentUserMatricola();
            List<NominativoMatricola> items = null;
            List<SelectListItem> result = new List<SelectListItem>();

            string tipologiaDocumentale = (string)SessionHelper.Get(matricola + "tipologiaDocumentale");
            string tipologiaDocumento = (string)SessionHelper.Get(matricola + "tipologiaDocumento");
            string tipologia = String.Format("{0}_{1}", tipologiaDocumentale.Trim(), tipologiaDocumento.Trim());

            items = AuthHelper.GetAllEnabledAs("DEMA", "01VIST", true);

            if (items != null && items.Any())
            {
                foreach (var i in items)
                {
                    string _tempNominativo = "";
                    var r = AuthHelper.EnableToMatr(i.Matricola, matricola, "DEMA", "01VIST");
                    if (r.Enabled)
                    {
                        _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                    }
                    // se non c'è matricola, allora è una tipologia come ad esempio 
                    // APPUNTO, che non ha matricola dipendente, ma ha comunque un vistatore
                    var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                    .Where(w => w.MATRICOLA == i.Matricola && w.IND_ATTIVO)
                    .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                    .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01VIST")
                    .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                    .FirstOrDefault();

                    if (_myAbil != null)
                    {
                        bool includi = false;

                        if (!String.IsNullOrEmpty(_myAbil.MATR_ESCLUSE)
                            && (_myAbil.MATR_ESCLUSE.Contains(matricola) || _myAbil.MATR_ESCLUSE.Contains("*")))
                        {
                            // questo vistatore non ha visibilità sulla matricola corrente
                            includi = false;
                        }

                        if (!String.IsNullOrEmpty(_myAbil.MATR_INCLUSE)
                            && (_myAbil.MATR_INCLUSE.Contains(matricola) || _myAbil.MATR_INCLUSE.Contains("*")))
                        {
                            includi = true;
                        }

                        if (!includi)
                        {
                            continue;
                        }

                        #region POSSO VISTATORE 
                        //Calcolo degli elementi che posso firmare
                        List<string> possoVistare = new List<string>();

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                            && _myAbil.TIP_DOC_INCLUSI.Contains(","))
                        {
                            possoVistare.AddRange(_myAbil.TIP_DOC_INCLUSI.Split(',').ToList());
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                            && !_myAbil.TIP_DOC_INCLUSI.Contains(",")
                            && !_myAbil.TIP_DOC_INCLUSI.Contains("*"))
                        {
                            possoVistare.Add(_myAbil.TIP_DOC_INCLUSI);
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI)
                            && _myAbil.TIP_DOC_INCLUSI.Contains("*"))
                        {
                            possoVistare.Add(tipologia);
                        }
                        #endregion

                        #region NON POSSO VISTATORE 
                        //Calcolo degli elementi che non posso vistare
                        List<string> nonPossoVistare = new List<string>();

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && _myAbil.TIP_DOC_ESCLUSI.Contains(","))
                        {
                            nonPossoVistare.AddRange(_myAbil.TIP_DOC_ESCLUSI.Split(',').ToList());
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && !_myAbil.TIP_DOC_ESCLUSI.Contains(",")
                            && !_myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                        {
                            nonPossoVistare.Add(_myAbil.TIP_DOC_ESCLUSI);
                        }

                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                            && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                        {
                            // se però possoVistare contiene tipologia allora non va aggiunta
                            if (!possoVistare.Contains(tipologia))
                            {
                                nonPossoVistare.Add(tipologia);
                            }
                        }
                        #endregion

                        #region CASO LIMITE TUTTI E DUE *
                        if (!String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_INCLUSI) &&
                            !String.IsNullOrWhiteSpace(_myAbil.TIP_DOC_ESCLUSI)
                                && _myAbil.TIP_DOC_INCLUSI.Contains("*")
                                && _myAbil.TIP_DOC_ESCLUSI.Contains("*"))
                        {
                            possoVistare = new List<string>();
                            nonPossoVistare = new List<string>();
                        }
                        #endregion

                        List<string> tipologieAbilitate = possoVistare.Except(nonPossoVistare).ToList();

                        if (tipologieAbilitate != null &&
                            tipologieAbilitate.Any())
                        {
                            if (tipologieAbilitate.Contains(tipologia))
                            {
                                _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                            }
                            else
                            {
                                _tempNominativo = "";
                            }
                        }
                        else if (nonPossoVistare != null && nonPossoVistare.Any())
                        {
                            if (!nonPossoVistare.Contains(tipologia))
                            {
                                _tempNominativo = (CommonHelper.ToTitleCase(i.Cognome.Trim()) + " " + CommonHelper.ToTitleCase(i.Nome.Trim()));
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(_tempNominativo))
                    {
                        result.Add(new SelectListItem()
                        {
                            Text = _tempNominativo,
                            Value = i.Matricola,
                            Selected = false
                        });
                    }
                }
            }

            // se c'è un solo elemento lo imposta come selezionato
            if (result != null && result.Any() && result.Count == 1)
            {
                result[0].Selected = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RenderViewMultiSelezione()
        {
            // purtroppo è necessario perchè la combo multiselezione non funziona se aggiornata
            // con la chiamata RaiSelectExtLoadAsyncData, quindi è stato necessario renderizzare
            // da capo tutta lo il div che la contiene
            MultiSelezioneVM model = new MultiSelezioneVM();
            model.ID = "Vistatore";
            model.Elementi = GetVistatoriSelezionabili(0);
            model.Placeholder = "Seleziona i valori";
            model.LabelText = "Vistatore";
            return View("~/Views/Dematerializzazione/subpartial/_multiSelezione.cshtml", model);
        }

        [HttpGet]
        public static List<SelectListItem> GetCodiceProtocollatorePerMatricola(int idPersona)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            IncentiviEntities db = new IncentiviEntities();
            string matricolaUtenteCorrente = UtenteHelper.Matricola();
            List<XR_HRIS_DIR_FILTER> aree = new List<XR_HRIS_DIR_FILTER>();
            List<int> ids = new List<int>();

            var abilitazioni = (from f in db.XR_HRIS_ABIL_FUNZIONE
                                join sf in db.XR_HRIS_ABIL_SUBFUNZIONE
                                on f.ID_FUNZIONE equals sf.ID_FUNZIONE
                                join ab in db.XR_HRIS_ABIL
                                on sf.ID_SUBFUNZ equals ab.ID_SUBFUNZ
                                where f.COD_FUNZIONE == "DEMA"
                                && ab.GR_AREA != null
                                && ab.MATRICOLA == matricolaUtenteCorrente
                                select ab).ToList();

            if (abilitazioni != null && abilitazioni.Any())
            {
                // prende tutti gli idarea e valorizza la combo
                foreach (var a in abilitazioni)
                {
                    string tx = a.GR_AREA;
                    List<string> ar = new List<string>();
                    ar = tx.Split(',').ToList();
                    foreach (var arr in ar)
                    {
                        int _tempID = 0;
                        bool converti = int.TryParse(arr, out _tempID);
                        if (converti && _tempID > 0)
                        {
                            ids.Add(_tempID);
                        }
                    }
                    ids = ids.Distinct().ToList();
                }
            }

            if (ids != null && ids.Any())
            {
                aree = db.XR_HRIS_DIR_FILTER.Where(w => ids.Contains(w.ID_AREA_FILTER)).ToList();
            }
            else
            {
                aree = db.XR_HRIS_DIR_FILTER.ToList();
            }

            foreach (var area in aree)
            {
                result.Add(new SelectListItem()
                {
                    Value = area.COD_AREA_FILTER,
                    Text = area.DESCRIPTION,
                    Selected = (aree.Count == 1
                                ? true
                                : false)
                });
            }

            return result;
        }

        [HttpGet]
        public JsonResult GetCodiceProtocollatorePerMatricolaJSON(string valore)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            IncentiviEntities db = new IncentiviEntities();
            string matricolaUtenteCorrente = UtenteHelper.Matricola();
            List<XR_HRIS_DIR_FILTER> aree = new List<XR_HRIS_DIR_FILTER>();
            List<int> ids = new List<int>();

            var abilitazioni = (from f in db.XR_HRIS_ABIL_FUNZIONE
                                join sf in db.XR_HRIS_ABIL_SUBFUNZIONE
                                on f.ID_FUNZIONE equals sf.ID_FUNZIONE
                                join ab in db.XR_HRIS_ABIL
                                on sf.ID_SUBFUNZ equals ab.ID_SUBFUNZ
                                where f.COD_FUNZIONE == "DEMA"
                                && ab.GR_AREA != null
                                && ab.MATRICOLA == matricolaUtenteCorrente
                                select ab).ToList();

            if (abilitazioni != null && abilitazioni.Any())
            {
                // prende tutti gli idarea e valorizza la combo
                foreach (var a in abilitazioni)
                {
                    string tx = a.GR_AREA;
                    List<string> ar = new List<string>();
                    ar = tx.Split(',').ToList();
                    foreach (var arr in ar)
                    {
                        int _tempID = 0;
                        bool converti = int.TryParse(arr, out _tempID);
                        if (converti && _tempID > 0)
                        {
                            ids.Add(_tempID);
                        }
                    }
                    ids = ids.Distinct().ToList();
                }
            }

            if (ids != null && ids.Any())
            {
                aree = db.XR_HRIS_DIR_FILTER.Where(w => ids.Contains(w.ID_AREA_FILTER)).ToList();
            }
            else
            {
                aree = db.XR_HRIS_DIR_FILTER.ToList();
            }

            foreach (var area in aree)
            {
                result.Add(new SelectListItem()
                {
                    Value = area.COD_AREA_FILTER,
                    Text = area.DESCRIPTION,
                    Selected = (aree.Count == 1
                                ? true
                                : !String.IsNullOrEmpty(valore)
                                        ? area.COD_AREA_FILTER == valore
                                            ? true
                                            : false
                                        : false)
                });
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public static List<SelectListItem> GetDestinatariPerIDPersona(int idPersona)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            IncentiviEntities db = new IncentiviEntities();

            var sint = db.SINTESI1.Where(w => w.ID_PERSONA == idPersona).FirstOrDefault();
            if (sint == null)
            {
                throw new Exception("Errore nel reperimento dei dati del dipendente");
            }

            string matricolaDestinatario = sint.COD_MATLIBROMAT.Trim();
            string matricolaUtenteCorrente = UtenteHelper.Matricola();
            string emailDipendente = CommonHelper.GetEmailPerMatricola(matricolaDestinatario);

            if (String.IsNullOrWhiteSpace(emailDipendente))
                emailDipendente = "P" + matricolaDestinatario + "@rai.it";

            result.Add(new SelectListItem()
            {
                Value = emailDipendente,
                Text = emailDipendente
            });

            string cod_servizio = sint.COD_SERVIZIO;
            if (String.IsNullOrWhiteSpace(cod_servizio))
            {
                throw new Exception($"Impossibile reperire il codice servizio per la persona con id {idPersona}");
            }

            cod_servizio = cod_servizio.Trim();
            var datiMail = db.XR_TB_EMAIL_DIREZIONI.Where(w => w.Codice_Servizio == cod_servizio).FirstOrDefault();

            if (datiMail == null)
            {
                throw new Exception($"Impossibile reperire l'indirizzo email della direzione {cod_servizio}");
            }

            string emailDirezione = String.Format("{0} <{1}>", datiMail.Alias.Trim(), datiMail.Email.Trim());

            result.Add(new SelectListItem()
            {
                Value = datiMail.Email.Trim(),
                Text = emailDirezione
            });

            result.Add(new SelectListItem()
            {
                Value = "StaffAmministratoreDelegato@rai.it",
                Text = "[CC] Staff Amministratore Delegato <StaffAmministratoreDelegato@rai.it>"
            });

            return result;
        }

        [HttpGet]
        public static List<SelectListItem> GetDestinatariPerIDPersona_EmailDipendente(int idPersona)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            IncentiviEntities db = new IncentiviEntities();

            var sint = db.SINTESI1.Where(w => w.ID_PERSONA == idPersona).FirstOrDefault();
            if (sint == null)
            {
                throw new Exception("Errore nel reperimento dei dati del dipendente");
            }

            string matricolaDestinatario = sint.COD_MATLIBROMAT.Trim();
            string matricolaUtenteCorrente = UtenteHelper.Matricola();
            string emailDipendente = CommonHelper.GetEmailPerMatricola(matricolaDestinatario);

            if (String.IsNullOrWhiteSpace(emailDipendente))
                emailDipendente = "P" + matricolaDestinatario + "@rai.it";

            result.Add(new SelectListItem()
            {
                Value = emailDipendente,
                Text = emailDipendente
            });

            return result;
        }

        [HttpGet]
        public static List<SelectListItem> GetDestinatariPerIDPersona_NoStaffAD(int idPersona)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            IncentiviEntities db = new IncentiviEntities();

            var sint = db.SINTESI1.Where(w => w.ID_PERSONA == idPersona).FirstOrDefault();
            if (sint == null)
            {
                throw new Exception("Errore nel reperimento dei dati del dipendente");
            }

            string matricolaDestinatario = sint.COD_MATLIBROMAT.Trim();
            string matricolaUtenteCorrente = UtenteHelper.Matricola();
            string emailDipendente = CommonHelper.GetEmailPerMatricola(matricolaDestinatario);

            if (String.IsNullOrWhiteSpace(emailDipendente))
                emailDipendente = "P" + matricolaDestinatario + "@rai.it";

            result.Add(new SelectListItem()
            {
                Value = emailDipendente,
                Text = emailDipendente
            });

            string cod_servizio = sint.COD_SERVIZIO;
            if (String.IsNullOrWhiteSpace(cod_servizio))
            {
                throw new Exception($"Impossibile reperire il codice servizio per la persona con id {idPersona}");
            }

            cod_servizio = cod_servizio.Trim();
            var datiMail = db.XR_TB_EMAIL_DIREZIONI.Where(w => w.Codice_Servizio == cod_servizio).FirstOrDefault();

            if (datiMail == null)
            {
                throw new Exception($"Impossibile reperire l'indirizzo email della direzione {cod_servizio}");
            }

            string emailDirezione = String.Format("{0} <{1}>", datiMail.Alias.Trim(), datiMail.Email.Trim());

            result.Add(new SelectListItem()
            {
                Value = datiMail.Email.Trim(),
                Text = emailDirezione
            });

            return result;
        }

        [HttpGet]
        public static string GetOggettoPerIDPersona(int idPersona)
        {
            string result = null;
            IncentiviEntities db = new IncentiviEntities();

            var sint = db.SINTESI1.Where(w => w.ID_PERSONA == idPersona).FirstOrDefault();
            if (sint == null)
            {
                throw new Exception("Errore nel reperimento dei dati del dipendente");
            }

            //cognome nome attività extra aziendali
            result = String.Format("{0} {1} attività extra aziendali", sint.DES_COGNOMEPERS.Trim(), sint.DES_NOMEPERS.Trim());

            return result;
        }

        public static List<XR_HRIS_PROTOCOLLI> GetProtocolloPerMatricola(string matricola)
        {
            List<XR_HRIS_PROTOCOLLI> result = new List<XR_HRIS_PROTOCOLLI>();
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var _myAbil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE")
                    .Where(w => w.MATRICOLA == matricola && w.IND_ATTIVO)
                    .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO)
                    .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "02GEST")
                    .Where(w => w.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA")
                    .FirstOrDefault();

                    if (_myAbil != null)
                    {
                        var listArea = _myAbil?.GR_AREA?.Split(',').Select(int.Parse).ToList();
                        if (listArea != null)
                        {
                            result = db.XR_HRIS_PROTOCOLLI.Where(x => listArea.Contains((int)x.IdArea)).ToList();
                        }
                        else
                        {
                            result = db.XR_HRIS_PROTOCOLLI.ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //ScriviLogDEM()
                throw new Exception(ex.Message);
            }


            return result;
        }

        public ActionResult MailDem_Viewer(int id)
        {
            //DematerializzazioneBozza bozza = InternalGetBozza(idTemplate, idDoc);
            //TemplateInfo bozza = null;

            XR_ALLEGATI allegato = new XR_ALLEGATI();
            var db = AnagraficaManager.GetDb();

            try
            {
                allegato = db.XR_ALLEGATI.Where(w => w.Id.Equals(14046)).FirstOrDefault();
                if (allegato == null)
                {
                    throw new Exception("Errore impossibile reperire il file");
                }
            }
            catch (Exception ex)
            {
                allegato = null;
            }
            return View("~/Views/Dematerializzazione/subpartial/_Allegato_Viewer.cshtml", allegato);
            //return View("~/Views/Dematerializzazione/subpartial/Modal_BozzaMail.cshtml", allegato);
        }
        private static DematerializzazioneBozza InternalGetBozza(int idTemplate, int idDoc)
        {
            DematerializzazioneBozza bozza = new DematerializzazioneBozza();
            string tipo = "proposta";
            bozza.TipologiaBozza = "proposta";
            var db = new IncentiviEntities();
            //var dip = db.XR_INC_DIPENDENTI.Find(idDip);

            if (tipo == "proposta")
            {
                string filtroTemplate = "";
                //if (dip.NOT_TIP_ACCERT == "Quota100" || dip.NOT_TIP_ACCERT == "Quota102")
                //{
                //    if (dip.NOT_TIP_SCELTA == "Quota100" || dip.NOT_TIP_SCELTA == "Quota102")
                //        filtroTemplate = "BozzaA";
                //    else
                //        filtroTemplate = "BozzaC";
                //}
                //else
                //{
                //    if (dip.SINTESI1.COD_QUALIFICA.StartsWith("M") || dip.SINTESI1.COD_QUALIFICA.StartsWith("A7"))
                //    {
                //        if (dip.NUM_MENS_AGG_DEC.GetValueOrDefault() > 0)
                //            filtroTemplate = "BozzaB_Agg";
                //        else
                //            filtroTemplate = "BozzaB_NoAgg";
                //    }
                //    else
                //    {
                //        if ((dip.NOT_TIP_SCELTA == "Quota100" || dip.NOT_TIP_SCELTA == "Quota102") && dip.IND_INVALIDITA.GetValueOrDefault() == 1)
                //            filtroTemplate = "BozzaD";
                //        else
                //            filtroTemplate = "BozzaB";
                //    }
                //}

                var template = HrisHelper.GetTemplateById(idTemplate);
                //if (template.ID_DIPENDENTE.HasValue)
                bozza.HtmlTextLastMod = String.Format("Ultima modifica di {0} il {1:dd/MM/yyyy} alle {1:HH:mm}", CezanneHelper.GetNominativoByMatricola(template.COD_USER), template.TMS_TIMESTAMP);

                //Richiamo la funzione per prendere la descrizione del template standard
                //var templSt = CessazioneHelper.GetTemplate(db, "Mail", idDip, filtroTemplate, false);
                //if (templSt != null)
                bozza.TemplateBozza = template.NME_TEMPLATE;

                bozza.IdDocumento = idDoc;
                //bozza.TipoVertenze = dip.IND_TIPO_VERTENZE;
                bozza.Codice = template.COD_TIPO;
                bozza.HtmlText = template.TEMPLATE_TEXT;

                string[] paramsToken = HrisHelper.GetParametri<string>(HrisParam.IncentiviMensilitaAggiuntive);
                //string token = dip.NUM_MENS_AGG_DEC.GetValueOrDefault() == 0 ? paramsToken[1] : paramsToken[0];

                string[] paramsStraToken = HrisHelper.GetParametri<string>(HrisParam.IncentiviTokenStra);
                //string tokenStra = !String.IsNullOrWhiteSpace(dip.CAUSE_VERTENZE) ? paramsStraToken[0] : "";

                string dataFnl930 = HrisHelper.GetParametro<string>(HrisParam.IncentiviEsecuzioneFNL930);
                DateTime dataExec;
                DateTime.TryParseExact(dataFnl930, "dd/MM/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out dataExec);

                //bozza.HtmlText = bozza.HtmlText
                //                    .Replace("__DATA_CESSAZIONE__", dip.DATA_CESSAZIONE.Value.ToString("dd/MM/yyyy"))
                //                    .Replace("__INCENTIVO_NUM__", dip.INCENTIVO_LORDO_IP.GetValueOrDefault().ToString("N2"))
                //                    .Replace("__INCENTIVO_LETT__", dip.INCENTIVO_LORDO_IP.GetValueOrDefault().AmountToReadableString())
                //                    .Replace("__UNA_TANTUM_NUM__", dip.UNA_TANTUM_LORDA_IP.GetValueOrDefault().ToString("N2"))
                //                    .Replace("__UNA_TANTUM_LETT__", dip.UNA_TANTUM_LORDA_IP.GetValueOrDefault().AmountToReadableString())
                //                    .Replace("__TFR_LORDO_NUM__", dip.TFR_LORDO_AZ_IP.GetValueOrDefault().ToString())
                //                    .Replace("__TFR_LORDO_LETT__", dip.TFR_LORDO_AZ_IP.GetValueOrDefault().AmountToReadableString())
                //                    .Replace("__TFR_LORDO_AZ__", dip.TFR_LORDO_AZ_IP.GetValueOrDefault().ToString("N2"))
                //                    .Replace("__TFR_NETTO__", dip.TFR_NETTO.GetValueOrDefault().ToString("N2"))
                //                    .Replace("__ALIQ_TFR__", dip.ALIQ_TFR.GetValueOrDefault().ToString("N2"))
                //                    .Replace("__NUM_MENS_PRINC__", dip.NUM_MENS_PRINC_DEC.GetValueOrDefault().ToString())
                //                    .Replace("__TOKEN_MENS_AGG__", token)
                //                    .Replace("__NUM_MENS_AGG__", dip.NUM_MENS_AGG_DEC.GetValueOrDefault().ToString())
                //                    .Replace("__TOKEN_STRA__", tokenStra)
                //                    .Replace("__DATA_AGG_TFR__", dip.DATA_TFR.HasValue ? dip.DATA_TFR.Value.ToString("dd/MM/yyyy") : dataExec.ToString("dd/MM/yyyy"));

                //var templateAll = CessazioneHelper.GetTemplate(db, "PropostaPDF", idDip, "", true);
                //if (templateAll != null)
                //{
                //    bozza.HasPDFTemplate = true;
                //    if (templateAll.COD_USER != "ADMIN")
                //        bozza.TemplateLastMod = String.Format("Ultima modifica di {0} il {1:dd/MM/yyyy} alle {1:HH:mm}", CezanneHelper.GetNominativoByMatricola(templateAll.COD_USER), templateAll.TMS_TIMESTAMP);
                //    else
                //        bozza.TemplateLastMod = String.Format("Proposta generata automaticamente il {0:dd/MM/yyyy} alle {0:HH:mm}", templateAll.TMS_TIMESTAMP);
                //}
                //else
                //{
                //    bozza.HasPDFTemplate = false;
                //}

                //if (dip.DATA_INVIO_PROP.HasValue)
                //{
                bozza.IsViewMode = false;
                //bozza.InfoInvio = dip.NOT_INVIO_PROP;
                //}

                bozza.IndirizziCC = "";
                //var extraCC = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == "CC" && x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO)).Select(x => x.VALORE);
                //if (extraCC != null && extraCC.Any())
                //    bozza.IndirizziCC = String.Join(";", extraCC);

                //var tmpExtraCC = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == "CC").ToList();

                //if (tmpExtraCC != null && tmpExtraCC.Any())
                //{

                //    tmpExtraCC = tmpExtraCC.Where(x => (x.COD_SERVIZIO == null || x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO))
                //                                    && (x.COD_SERVIZIO_ESCL == null || !x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO))
                //                                    && (x.SEDI_INCLUSE == null || x.SEDI_INCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + "*") || x.SEDI_INCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + dip.SINTESI1.COD_SERVIZIO))
                //                                    && (x.SEDI_ESCLUSE == null || (!x.SEDI_ESCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + "*") && !x.SEDI_ESCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + dip.SINTESI1.COD_SERVIZIO)))
                //                                    ).ToList();

                //    var extraCC = tmpExtraCC.Select(x => x.VALORE);
                //    if (extraCC != null && extraCC.Any())
                //    {
                //        foreach (var item in extraCC)
                //        {
                //            bozza.IndirizziCC += (!String.IsNullOrWhiteSpace(bozza.IndirizziCC) ? ";" : "") + item;
                //        }
                //    }
                //}

                //bozza.AbilitaInvio = true;
                bozza.AbilitaGestione = true;
            }
            //else
            //{
            //    var filtroTemplate = "BozzaBase";

            //    var template = CessazioneHelper.GetTemplate(db, "MailVerbale", idDip, filtroTemplate, true);
            //    if (template.ID_DIPENDENTE.HasValue)
            //        bozza.HtmlTextLastMod = String.Format("Ultima modifica di {0} il {1:dd/MM/yyyy} alle {1:HH:MM}", CezanneHelper.GetNominativoByMatricola(template.COD_USER), template.TMS_TIMESTAMP);

            //    //Richiamo la funzione per prendere la descrizione del template standard
            //    var templSt = CessazioneHelper.GetTemplate(db, "MailVerbale", idDip, filtroTemplate, false);
            //    if (templSt != null)
            //        bozza.TemplateBozza = templSt.DES_TEMPLATE;

            //    bozza.IdDipendente = idDip;
            //    bozza.Codice = template.NME_TEMPLATE;
            //    bozza.HtmlText = template.TEMPLATE_TEXT;

            //    string tokenIban = "";
            //    if (dip.IND_PROPRIO_IBAN != "B")
            //        tokenIban = HrisHelper.GetParametro<string>(HrisParam.IncentiviMailVerbaleTokenIban);

            //    string tokenQIO = "";
            //    var paramTokenQIO = HrisHelper.GetParametro(HrisParam.IncentiviMailVerbaleTokenQIO);
            //    var qualTokenQIOExcl = paramTokenQIO.COD_VALUE2.Split(',');
            //    //if (!dip.SINTESI1.COD_QUALIFICA.StartsWith("M7") && !dip.SINTESI1.COD_QUALIFICA.StartsWith("A7") && !dip.SINTESI1.COD_QUALIFICA.StartsWith("A01"))
            //    if (dip.ID_SIGLASIND == null && !qualTokenQIOExcl.Any(x => dip.SINTESI1.COD_QUALIFICA.StartsWith(x)))
            //    {
            //        if (!String.IsNullOrWhiteSpace(tokenIban))
            //            tokenQIO = paramTokenQIO.COD_VALUE1;
            //        else
            //            tokenQIO = paramTokenQIO.COD_VALUE3;
            //    }

            //    string tokenExtraSind = "";
            //    using (var dbTal = new myRaiDataTalentia.TalentiaEntities())
            //    {
            //        var extraSind = db.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "IncentiviExtraSind" && x.COD_VALUE1 == dip.SEDE.ToUpper().Trim());
            //        if (extraSind != null)
            //            tokenExtraSind = ", " + extraSind.COD_VALUE2;
            //    }

            //    string tokenITL = "";
            //    //if (dip.IND_ITL.GetValueOrDefault())
            //    tokenITL = HrisHelper.GetParametro<string>(HrisParam.IncentiviMailVerbaleTokenItl);

            //    string tokenRoma = "";
            //    if (dip.SEDE.ToUpper().Contains("ROMA"))
            //        tokenRoma = HrisHelper.GetParametro<string>(HrisParam.IncentiviMailVerbaleTokenRoma);

            //    string dataFnl930 = HrisHelper.GetParametro<string>(HrisParam.IncentiviEsecuzioneFNL930);
            //    DateTime dataExec;
            //    DateTime.TryParseExact(dataFnl930, "dd/MM/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out dataExec);

            //    bozza.HtmlText = bozza.HtmlText
            //                        .Replace("__NOMINATIVO__", dip.SINTESI1.DES_NOMEPERS.TitleCase() + " " + dip.SINTESI1.DES_COGNOMEPERS.TitleCase())
            //                        .Replace("__TOKEN_IBAN__", tokenIban)
            //                        .Replace("__TOKEN_QIO__", tokenQIO)
            //                        .Replace("__EXTRA_SIND__", tokenExtraSind)
            //                        .Replace("__TOKEN_ITL__", tokenITL)
            //                        .Replace("__TOKEN_ROMA__", tokenRoma);

            //    if (dip.NUM_BOZZA_GIORNI.HasValue)
            //        bozza.HtmlText = bozza.HtmlText.Replace("__NUM_GIORNI_LIMITE__", dip.NUM_BOZZA_GIORNI.Value.ToString());

            //    if (dip.DATA_APPUNTAMENTO.HasValue)
            //    {
            //        bozza.HtmlText = bozza.HtmlText
            //                            .Replace("__GIORNO_APPUNTAMENTO__", dip.DATA_APPUNTAMENTO.Value.ToString("dd/MM/yyyy"))
            //                            .Replace("__ORA_APPUNTAMENTO__", dip.DATA_APPUNTAMENTO.Value.ToString("HH:mm"));
            //    }

            //    if (!String.IsNullOrWhiteSpace(dip.NOT_LUOGO_APPUNTAMENTO))
            //        bozza.HtmlText = bozza.HtmlText
            //                            .Replace("__LUOGO_APPUNTAMENTO__", dip.NOT_LUOGO_APPUNTAMENTO);

            //    var templateAll = CessazioneHelper.GetTemplate(db, "VerbalePDF", idDip, "", true);
            //    if (templateAll != null)
            //    {
            //        bozza.HasPDFTemplate = true;
            //        if (templateAll.COD_USER != "ADMIN")
            //            bozza.TemplateLastMod = String.Format("Ultima modifica di {0} il {1:dd/MM/yyyy} alle {1:HH:mm}", CezanneHelper.GetNominativoByMatricola(templateAll.COD_USER), templateAll.TMS_TIMESTAMP);
            //        else
            //            bozza.TemplateLastMod = String.Format("Proposta generata automaticamente il {0:dd/MM/yyyy} alle {0:HH:mm}", templateAll.TMS_TIMESTAMP);
            //    }
            //    else
            //    {
            //        bozza.HasPDFTemplate = false;
            //    }

            //    if (dip.DATA_BOZZA_INVIO.HasValue)
            //    {
            //        bozza.IsViewMode = true;
            //        bozza.InfoInvio = dip.NOT_BOZZA_INVIO;
            //    }

            //    bozza.IndirizziCC = "";
            //    //var extraCC = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == "CC Verbale" && x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO)).Select(x => x.VALORE);
            //    //if (extraCC != null && extraCC.Any())
            //    //    bozza.IndirizziCC = String.Join(";", extraCC);

            //    var tmpExtraCC = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == "CC Verbale").ToList();
            //    if (tmpExtraCC != null && tmpExtraCC.Any())
            //    {

            //        tmpExtraCC = tmpExtraCC.Where(x => (x.COD_SERVIZIO == null || x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO))
            //                                        && (x.COD_SERVIZIO_ESCL == null || !x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO))
            //                                        && (x.SEDI_INCLUSE == null || x.SEDI_INCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + "*") || x.SEDI_INCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + dip.SINTESI1.COD_SERVIZIO))
            //                                        && (x.SEDI_ESCLUSE == null || (!x.SEDI_ESCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + "*") && !x.SEDI_ESCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + dip.SINTESI1.COD_SERVIZIO)))
            //                                        ).ToList();

            //        var extraCC = tmpExtraCC.Select(x => x.VALORE);
            //        if (extraCC != null && extraCC.Any())
            //        {
            //            foreach (var item in extraCC)
            //            {
            //                bozza.IndirizziCC += (!String.IsNullOrWhiteSpace(bozza.IndirizziCC) ? ";" : "") + item;
            //            }
            //        }
            //    }

            //    bozza.HasCronologia = db.XR_INC_TEMPLATE.Any(x => x.COD_TIPO == "VerbaleDOC" && x.ID_DIPENDENTE == idDip);

            //    if (dip.SEDE.ToUpper() == "ROMA")
            //    {
            //        bozza.AbilitaInvio = true;
            //        bozza.AbilitaGestione = true;
            //    }
            //    else
            //    {
            //        bozza.AbilitaInvio = dip.IND_SBLOCCA_PRATICA.GetValueOrDefault() == 1;
            //        string matricola = CommonHelper.GetCurrentUserMatricola();
            //        if (CessazioneHelper.EnabledToAnySubFunc(matricola, "ADM", "GEST"))
            //            bozza.AbilitaGestione = true;
            //        else
            //            bozza.AbilitaGestione = AuthHelper.EnabledToSubFunc(matricola, CessazioneHelper.INCENTIVI_INC_EXTRA, "CONTENZIOSO");
            //    }
            //}

            return bozza;
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save_BozzaMail(DematerializzazioneBozza bozza)
        {
            //List<XR_HRIS_PARAM> listParam = HrisHelper.GetParametriJson<XR_HRIS_PARAM>(HrisParam.IncentiviParametri);
            var db = new IncentiviEntities();
            //var dip = db.XR_INC_DIPENDENTI.Find(bozza.IdDipendente);

            string codTipo = "";
            //if (bozza.TipologiaBozza == "proposta")
            codTipo = "TemplateInvioMail";
            //else
            //    codTipo = "MailVerbale";
            XR_HRIS_TEMPLATE template = new XR_HRIS_TEMPLATE();

            var old = db.XR_HRIS_TEMPLATE.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)).FirstOrDefault(x => x.COD_TIPO == codTipo && x.NME_TEMPLATE == bozza.TemplateBozza);
            if (old != null && old.TEMPLATE_TEXT != bozza.HtmlText)
                old.VALID_DTA_END = DateTime.Now;

            if (old == null || old.TEMPLATE_TEXT != bozza.HtmlText)
            {
                template.NME_TEMPLATE = bozza.TemplateBozza;
                template.DES_TEMPLATE = bozza.Codice;
                template.TEMPLATE_TEXT = bozza.HtmlText;
                template.COD_TIPO = codTipo;


                CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime timestamp);
                template.VALID_DTA_INI = timestamp;
                template.COD_USER = codUser;
                template.COD_TERMID = codTermid;
                template.TMS_TIMESTAMP = timestamp;

                db.XR_HRIS_TEMPLATE.Add(template);
            }

            //if (bozza.TipologiaBozza == "verbale")
            //    dip.IND_SBLOCCA_PRATICA = _sbloccaPratica;

            var result = db.SaveChanges();

            return Content(!string.IsNullOrWhiteSpace(template.ID_TEMPLATE.ToString()) ? template.ID_TEMPLATE.ToString() : old.ID_TEMPLATE.ToString());
            //if (result)
            //{
            //if (_invioMail)
            //{
            //    GestoreMail mail = new GestoreMail();
            //    List<Attachement> attach = new List<Attachement>();

            //    string tipoDoc = "";
            //    string nomeAllegato = "";
            //    HrisParam paramAbilita = HrisParam.IncentiviAbilitaMail;
            //    string codCampoCC = "";
            //    string destinatario = "";
            //    if (bozza.TipologiaBozza == "proposta")
            //    {
            //        tipoDoc = "PropostaPDF";
            //        nomeAllegato = "Proposta";
            //        paramAbilita = HrisParam.IncentiviAbilitaMail;
            //        codCampoCC = "CC";
            //        destinatario = CessazioneHelper.GetMailDip(dip); //CommonHelper.GetEmailPerMatricola(dip.MATRICOLA);
            //    }
            //    else
            //    {
            //        tipoDoc = "VerbalePDF";
            //        nomeAllegato = "Bozza verbale";
            //        paramAbilita = HrisParam.IncentiviAbilitaMailVerbale;
            //        codCampoCC = "CC Verbale";
            //        destinatario = CessazioneHelper.GetMailDip(dip); //dip.MAIL;
            //    }

            //    if (bozza.TipologiaBozza == "proposta")
            //    {
            //        if (_includiProposta)
            //        {
            //            var template = CessazioneHelper.GetTemplate(db, tipoDoc, dip.ID_DIPENDENTE, "", true);
            //            if (template != null)
            //            {
            //                attach.Add(new Attachement()
            //                {
            //                    AttachementName = nomeAllegato + " " + dip.SINTESI1.DES_COGNOMEPERS.TitleCase() + ".pdf",
            //                    AttachementValue = template.TEMPLATE,
            //                    AttachementType = "application/pdf"
            //                });
            //            }
            //        }

            //        var listAll = CessazioneHelper.GetListaAllegati(db, "MailAllegatiExtra", dip.ID_DIPENDENTE, dip.SINTESI1.DES_CITTASEDE, dip.SINTESI1.COD_QUALIFICA);
            //        if (listAll != null && listAll.Any())
            //        {
            //            attach.AddRange(listAll.Select(x => new Attachement()
            //            {
            //                AttachementName = x.NME_TEMPLATE,
            //                AttachementValue = x.TEMPLATE,
            //                AttachementType = x.CONTENT_TYPE
            //            }));
            //        }
            //    }
            //    else
            //    {
            //        var template = CessazioneHelper.GetTemplate(db, tipoDoc, dip.ID_DIPENDENTE, "", true);
            //        if (template != null)
            //        {
            //            attach.Add(new Attachement()
            //            {
            //                AttachementName = nomeAllegato + " " + dip.SINTESI1.DES_COGNOMEPERS.TitleCase() + ".pdf",
            //                AttachementValue = template.TEMPLATE,
            //                AttachementType = "application/pdf"
            //            });
            //        }

            //        var ms = InternalProspetto(db, dip, 0, false);
            //        attach.Add(new Attachement()
            //        {
            //            AttachementName = "Prospetto " + dip.SINTESI1.Nominativo().ToUpper() + ".pdf",
            //            AttachementValue = ms.ToArray(),
            //            AttachementType = "application/pdf"
            //        });


            //        var listAll = CessazioneHelper.GetListaAllegati(db, "MailVerbaleAllegatiExtra", dip.ID_DIPENDENTE, dip.SINTESI1.DES_CITTASEDE, dip.SINTESI1.COD_QUALIFICA);
            //        if (listAll != null && listAll.Any())
            //        {
            //            attach.AddRange(listAll.Select(x => new Attachement()
            //            {
            //                AttachementName = x.NME_TEMPLATE,
            //                AttachementValue = x.TEMPLATE,
            //                AttachementType = x.CONTENT_TYPE
            //            }));
            //        }
            //    }

            //    string currentMatr = CommonHelper.GetCurrentUserMatricola();

            //    myRaiData.Incentivi.XR_HRIS_PARAM parametro = HrisHelper.GetParametro(paramAbilita);
            //    string mittente = "";
            //    if (bozza.TipologiaBozza == "verbale")
            //    {

            //        var mailParam = db.XR_INC_PARAM_MAIL.FirstOrDefault(x => x.COD_CAMPO == "FROM_VERBALE" && x.COD_MATRICOLA.Contains(currentMatr));
            //        if (mailParam != null)
            //            mittente = mailParam.VALORE;
            //        else
            //        {
            //            var dbTal = new myRaiDataTalentia.TalentiaEntities();
            //            var paramRifContenzioso = db.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "IncentiviRifContenzioso");
            //            if (paramRifContenzioso != null && !String.IsNullOrWhiteSpace(paramRifContenzioso.COD_VALUE1))
            //            {
            //                AbilSubFunc abilSubFunc = null;
            //                var filtroContenzioso = AuthHelper.EnabledToSubFunc(paramRifContenzioso.COD_VALUE1, CessazioneHelper.INCENTIVI_INC_EXTRA, "VERBALI", out abilSubFunc)
            //                            || AuthHelper.EnabledToSubFunc(paramRifContenzioso.COD_VALUE1, CessazioneHelper.INCENTIVI_INC_EXTRA, "CONTENZIOSO", out abilSubFunc);

            //                if (filtroContenzioso)
            //                {
            //                    var abilCat = AuthHelper.EnabledCategory(paramRifContenzioso.COD_VALUE1, CessazioneHelper.INCENTIVI_INC_EXTRA, abilSubFunc.Nome);
            //                    //if (!String.IsNullOrWhiteSpace(dip.CAUSE_VERTENZE) || (!String.IsNullOrWhiteSpace(abilSubFunc.CategorieAbilitate..CAT_INCLUSE) && filtroContenzioso.CAT_INCLUSE.Split(',').Any(x => dip.SINTESI1.COD_QUALIFICA.StartsWith(x))))
            //                    //if (!String.IsNullOrWhiteSpace(dip.CAUSE_VERTENZE) || (abilCat.HasFilter && abilCat.CategorieIncluse.Any(x => dip.SINTESI1.COD_QUALIFICA.StartsWith(x))))
            //                    if (CessazioneHelper.IsConteziosoDip(dip) || (abilCat.HasFilter && abilCat.CategorieIncluse.Any(x => dip.SINTESI1.COD_QUALIFICA.StartsWith(x))))
            //                    {
            //                        var mailCont = db.XR_INC_PARAM_MAIL.FirstOrDefault(x => x.COD_CAMPO == "FROM_VERBALE" && x.COD_GRUPPO == abilSubFunc.Nome);
            //                        if (mailCont != null)
            //                            mittente = mailCont.VALORE;
            //                    }
            //                }
            //            }

            //        }
            //    }

            //    if (String.IsNullOrWhiteSpace(mittente))
            //        mittente = parametro.COD_VALUE2;

            //    string cc = mittente;
            //    var tmpExtraCC = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == codCampoCC).ToList();

            //    if (tmpExtraCC != null && tmpExtraCC.Any())
            //    {

            //        tmpExtraCC = tmpExtraCC.Where(x => (x.COD_SERVIZIO == null || x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO))
            //                                        && (x.COD_SERVIZIO_ESCL == null || !x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO))
            //                                        && (x.SEDI_INCLUSE == null || x.SEDI_INCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + "*") || x.SEDI_INCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + dip.SINTESI1.COD_SERVIZIO))
            //                                        && (x.SEDI_ESCLUSE == null || (!x.SEDI_ESCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + "*") && !x.SEDI_ESCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + dip.SINTESI1.COD_SERVIZIO)))
            //                                        ).ToList();

            //        var extraCC = tmpExtraCC.Select(x => x.VALORE);
            //        if (extraCC != null && extraCC.Any())
            //        {
            //            foreach (var item in extraCC)
            //            {
            //                cc += ";" + item;
            //            }
            //        }
            //    }

            //    string oggetto = parametro.COD_VALUE3;
            //    oggetto = oggetto.Replace("__COGNOME__", dip.SINTESI1.DES_COGNOMEPERS.TitleCase())
            //                     .Replace("__NOME__", dip.SINTESI1.DES_NOMEPERS.TitleCase());

            //    if (bozza.TipologiaBozza == "verbale")
            //        cc = CommonHelper.GetEmailPerMatricola(currentMatr) + (!String.IsNullOrWhiteSpace(cc) ? ";" + cc : "");

            //    var response = mail.InvioMail(bozza.HtmlText, oggetto, destinatario, cc, mittente, attach);
            //    if (response != null && response.Errore != null)
            //    {
            //        return Content("Errore durante l'invio della mail");
            //    }

            //    if (bozza.TipologiaBozza == "proposta")
            //    {
            //        dip.DATA_INVIO_PROP = DateTime.Today;
            //        dip.NOT_INVIO_PROP = String.Format("Mail inviata da {0} il {1:dd/MM/yyyy}", CezanneHelper.GetNominativoByMatricola(CommonHelper.GetCurrentUserMatricola()), dip.DATA_INVIO_PROP.Value);
            //        if (!_includiProposta)
            //            dip.NOT_INVIO_PROP += " - proposta non allegata";


            //        var param = listParam.FirstOrDefault(x => x.COD_PARAM == "LimiteProposta");
            //        dip.SetField("LimiteProposta", dip.DATA_INVIO_PROP.Value.Date.AddDays(Convert.ToDouble(param.COD_VALUE1)));
            //    }
            //    else
            //    {
            //        dip.DATA_BOZZA_INVIO = DateTime.Today;
            //        dip.NOT_BOZZA_INVIO = String.Format("Mail inviata da {0} il {1:dd/MM/yyyy}", CezanneHelper.GetNominativoByMatricola(CommonHelper.GetCurrentUserMatricola()), dip.DATA_BOZZA_INVIO.Value);
            //    }

            //    if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
            //    {
            //        return Content("L'invio della mail è andato a buon fine, tuttavia non è stata aggiornato il dato di invio");
            //    }

            //}

            //}
            //else
            //    return Content("Errore durante il salvataggio");
        }

    }
}