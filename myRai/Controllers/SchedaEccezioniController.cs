using myRai.DataAccess;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class SchedaEccezioniController : BaseCommonController
    {
        public ActionResult getCampoDin()
        {
            return View();
        }
        public ActionResult Index()
        {
            SchedeEccezioniModel model = SchedaEccezioniManager.getIndexModel();
            return View(model);
        }
        public ActionResult getEccezioneSchedaPopup(string codice)
        {
            var db = new digiGappEntities();
            var scheda=db.MyRai_Regole_SchedeEccezioni.ValidToday().Where(x => x.codice == codice).FirstOrDefault();

            if (!String.IsNullOrWhiteSpace (scheda.definizione)) scheda.definizione = HtmlTrim.Trim(scheda.definizione);
            if (!String.IsNullOrWhiteSpace (scheda.trattamento_economico)) scheda.trattamento_economico = HtmlTrim.Trim(scheda.trattamento_economico);
            if (!String.IsNullOrWhiteSpace(scheda.presupposti_procedure)) scheda.presupposti_procedure = HtmlTrim.Trim(scheda.presupposti_procedure);
            if (!String.IsNullOrWhiteSpace(scheda.presupposti_documentazione)) scheda.presupposti_documentazione = HtmlTrim.Trim(scheda.presupposti_documentazione);
            if (!String.IsNullOrWhiteSpace(scheda.criteri_inserimento)) scheda.criteri_inserimento = HtmlTrim.Trim(scheda.criteri_inserimento);

            foreach (var d in scheda.MyRai_Regole_CampiDinamici)
            {
                d.valore = HtmlTrim.Trim(d.valore);
            }

            scheda.versione= db.MyRai_Regole_SchedeEccezioni.Where(x => x.codice == codice).Count();

            if (scheda != null)
                return View("_dettaglioEccezione", scheda);
            else
                return Content("NON TROVATA");
        }
        public ActionResult EccezioneContent(int? id)
        {
            if (id == null)
            {
                PopupEccezioneModel model = new PopupEccezioneModel() { IsNew = true, IdEccezione = null };
                return View(model);
            }
            else
            {
                PopupEccezioneModel model = SchedaEccezioniManager. getEccezioneForUpdate((int)id);
                return View(model);
            }

        }
        

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SalvaEccezione(PopupEccezioneModel model)
        {
            var db = new digiGappEntities();
            var scheda = db.MyRai_Regole_SchedeEccezioni.ValidToday().Where(x => x.codice.Trim() == model.CodiceEccezione.Trim()).FirstOrDefault();
             model.Pubblicata = Request.Form["Pubblicata"] == "1";

            try
            {
                if (scheda == null)
                {
                    string esito = InserisciScheda(model);
                    return Content(esito);
                }
                else
                {
                    string esito = AggiornaScheda(model);
                    return Content(esito);
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    error_message = ex.ToString(),
                    provenienza = "SchedaEccezioniController.SalvaEccezione"
                });
                return Content(ex.Message + (ex.InnerException != null ? ex.InnerException.Message : ""));
            }
        }

        public string AggiornaScheda(PopupEccezioneModel model)
        {
            var db = new digiGappEntities();
            var scheda = db.MyRai_Regole_SchedeEccezioni.ValidToday().Where(x => x.codice.Trim() == model.CodiceEccezione.Trim()).FirstOrDefault();
            if (SchedaEccezioniManager.SchedaEccezioneChanged(model, scheda))
            {
                scheda = SchedaEccezioniManager.SalvaSchedaAggiornata(model, scheda, db);
                foreach (var alle in db.MyRai_Regole_Allegati.Where(x => x.id_scheda_eccezione == model.IdEccezione))
                {
                    alle.MyRai_Regole_SchedeEccezioni = scheda;
                }
            }

            SchedaEccezioniManager.UpdateDipendenzaUtenti(model, scheda, db);
            SchedaEccezioniManager.UpdateDipendenzaTematica(model, scheda, db);
            SchedaEccezioniManager.UpdateDipendenzaDestinatari(model, scheda, db);
            SchedaEccezioniManager.UpdateDipendenzaCampiDinamici(model, scheda, db);
            SchedaEccezioniManager.UpdateDipendenzaFonti(model, scheda, db);

            if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                return "OK" + scheda.id.ToString( );
            else
                return "Errore salvataggio DB";
        }

        public string InserisciScheda(PopupEccezioneModel model)
        {
            var db = new digiGappEntities();
            myRaiData.MyRai_Regole_SchedeEccezioni scheda = new MyRai_Regole_SchedeEccezioni();
            scheda.codice = model.CodiceEccezione;
            scheda.descrittiva = model.DescrittivaEccezione;
            scheda.descrittiva_libera = model.DescrittivaLibera;
            scheda.Pubblicato = model.Pubblicata;
            scheda.id_tipo_assenza = model.TipoAssenza;

            scheda.criteri_inserimento = model.CriteriInserimento;
            scheda.data_inizio_validita = DateTime.Now;
            scheda.data_modifica = DateTime.Now;
            scheda.data_fine_validita = null;

            scheda.definizione = model.Definizione;
            scheda.note = model.Note;
            scheda.presupposti_documentazione = model.Presupposti;
            scheda.trattamento_economico = model.TrattamentoEconomico;
            scheda.id_tipo_documentazione = model.TipoDocumentazione;
            scheda.EccezioniCollegate = model.EccezioniCollegate;

            //aggiungi tematiche nella relazione MyRai_Regole_SchedeEccezioni_Tematiche
            foreach (int idTematica in model.tematiche)
            {
                var tematicaDB = db.MyRai_Regole_Tematiche.Where(x => x.id == idTematica).FirstOrDefault();
                if (tematicaDB != null)
                {
                    myRaiData.MyRai_Regole_SchedeEccezioni_Tematiche tem = new MyRai_Regole_SchedeEccezioni_Tematiche();
                    tem.id_tematica = tematicaDB.id;
                    tem.data_inizio_validita = DateTime.Now;
                    tem.data_fine_validita = null;
                    db.MyRai_Regole_SchedeEccezioni_Tematiche.Add(tem);
                    scheda.MyRai_Regole_SchedeEccezioni_Tematiche.Add(tem);
                }
            }

            //aggiungi utenti nella relazione MyRai_Regole_SchedeEccezioni_Utenti
            foreach (int idUtente in model.utenti)
            {
                var utenteDB = db.MyRai_Regole_Utenti.Where(x => x.id == idUtente).FirstOrDefault();
                if (utenteDB != null)
                {
                    myRaiData.MyRai_Regole_SchedeEccezioni_Utenti utente = new MyRai_Regole_SchedeEccezioni_Utenti();
                    utente.id_utente = utenteDB.id;
                    utente.data_inizio_validita = DateTime.Now;
                    utente.data_fine_validita = null;
                    db.MyRai_Regole_SchedeEccezioni_Utenti.Add(utente);
                    scheda.MyRai_Regole_SchedeEccezioni_Utenti.Add(utente);
                }
            }

            //aggiungi destinatari nella relazione MyRai_Regole_SchedeEccezioni_Destinatari
            foreach (int idDestinatario in model.destinatari)
            {
                var destDB = db.MyRai_Regole_Destinatari.Where(x => x.id == idDestinatario).FirstOrDefault();
                if (destDB != null)
                {
                    myRaiData.MyRai_Regole_SchedeEccezioni_Destinatari dest = new MyRai_Regole_SchedeEccezioni_Destinatari();
                    dest.id_destinatario = destDB.id;
                    dest.data_inizio_validita = DateTime.Now;
                    dest.data_fine_validita = null;
                    db.MyRai_Regole_SchedeEccezioni_Destinatari.Add(dest);
                    scheda.MyRai_Regole_SchedeEccezioni_Destinatari.Add(dest);
                }
            }

            if (model.fonti != null && model.fonti.Length > 0 && !String.IsNullOrWhiteSpace(model.fonti[0]))
            {
                for (int i = 0; i < model.fonti.Length; i++)
                {
                    string fonte = model.fonti[i];
                    myRaiData.MyRai_Regole_SchedeEccezioni_Fonti f = new MyRai_Regole_SchedeEccezioni_Fonti();
                    f.data_inizio_validita = DateTime.Now;
                    f.data_fine_validita = null;
                    f.descrizione = fonte;
                    f.url = model.urlfonti[i];
                    db.MyRai_Regole_SchedeEccezioni_Fonti.Add(f);
                    scheda.MyRai_Regole_SchedeEccezioni_Fonti.Add(f);
                }
            }
            if (model.campodinamico != null && model.campodinamico.Length > 0 && !String.IsNullOrWhiteSpace(model.campodinamico[0]))
            {
                for (int i = 0; i < model.campodinamico.Length; i++)
                {
                    string campo = model.campodinamico[i];
                    if (String.IsNullOrWhiteSpace(campo)) continue;

                    string valore = model.valoredinamico[i];

                    myRaiData.MyRai_Regole_CampiDinamici c = new MyRai_Regole_CampiDinamici();
                    c.data_inizio_validita = DateTime.Now;
                    c.data_fine_validita = null;
                    c.chiave = campo;
                    c.valore = valore;
                    if ( String.IsNullOrWhiteSpace (model.posizionedinamico[i]))
                        c.Posizione = null;
                    else
                        c.Posizione = Convert.ToInt32(model.posizionedinamico[i]);

                    db.MyRai_Regole_CampiDinamici.Add(c);
                    scheda.MyRai_Regole_CampiDinamici.Add(c);
                }
            }

            db.MyRai_Regole_SchedeEccezioni.Add(scheda);

            if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                return "OK" + scheda.id.ToString( );
            else
                return "Errore salvataggio DB";
        }


        public ActionResult getDettagliEccezione(string codice)
        {
            var db = new digiGappEntities();
            var ec = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione.Trim() == codice.Trim()).FirstOrDefault();
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { desc = ec.desc_eccezione, unit = ec.unita_misura }
            };
        }
        public ActionResult getPartialAllegato(int indice)
        {
            AllegatoEccezione a = new AllegatoEccezione() { indice = indice };
            return View("_AllegatiEcc", a);
        }
        public ActionResult DelAllegati(int id)
        {
            var db = new digiGappEntities();
            var alle = db.MyRai_Regole_Allegati.Find(id);
            if (alle != null)
            {
                alle.data_fine_validita = DateTime.Now;

                if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola( ) ) )
                    return Content("OK");
            }
            return Content("Errore cancellazione DB");
        }
        public ActionResult AddAllegati(HttpPostedFileBase[] _fileUpload, string[] nomefile, int idecc)
        {
            if (_fileUpload == null) return Content("OK");

            var db = new digiGappEntities();
            for (int i = 0; i < _fileUpload.Length; i++)
            {
                var file = _fileUpload[i];
                myRaiData.MyRai_Regole_Allegati al = new MyRai_Regole_Allegati();
                al.real_filename = file.FileName;
                al.data_fine_validita = null;
                al.data_inizio_validita = DateTime.Now;
                al.nomefile = nomefile[i];
                MemoryStream target = new MemoryStream();
                file.InputStream.CopyTo(target);
                al.documento = target.ToArray();
                al.id_scheda_eccezione = idecc;
                db.MyRai_Regole_Allegati.Add(al);
            }
            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola( ) ) )
                return Content("OK");
            else
                return Content("Errore salvataggio DB Allegati");

        }
        public ActionResult getDoc(int idAllegato)
        {
            var db = new digiGappEntities();
            var alle = db.MyRai_Regole_Allegati.Find(idAllegato);

            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(alle.documento, 0, alle.documento.Length);
            pdfStream.Position = 0;
            string est = System.IO.Path.GetExtension(alle.real_filename).Replace(".", "");
            switch (est.ToLower())
            {
                case "pdf":
                    return new FileStreamResult(pdfStream, "application/pdf") { FileDownloadName = alle.real_filename };
                    break;

                case "xls":
                case "xlsx":
                    return new FileStreamResult(pdfStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = alle.real_filename };
                    break;

                case "doc":
                case "docx":
                    return new FileStreamResult(pdfStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document") { FileDownloadName = alle.real_filename };
                    break;

            }
             
            return new FileStreamResult(pdfStream, "application/" + System.IO.Path.GetExtension(alle.real_filename).Replace(".", "")) { FileDownloadName=alle.real_filename };
        }

    }
     

    public static class ExtensionMethods
    {
        //Estensione per LINQ con generics T per tutte le tabelle che prevedono data_inizio e data_fine validita
        public static IEnumerable<T> ValidToday<T>(this IEnumerable<T> source)
        {
            if (source == null) return null;
            if ( typeof(T).GetProperty("data_inizio_validita") == null 
                || typeof(T).GetProperty("data_fine_validita") == null)
                return source;
             
            ParameterExpression t = Expression.Parameter(typeof(T), "t");
            Expression comparison = Expression.LessThanOrEqual(
                Expression.Property(t, "data_inizio_validita"), 
                    Expression.Constant(DateTime.Now));
            Expression prop2 = Expression.Property(t, "data_fine_validita");
            Expression comparison2 = Expression.Equal(
                Expression.Property(t, "data_fine_validita"), 
                    Expression.Constant(null));
            Expression prop3 = Expression.Property(t, "data_fine_validita");
            var converted = Expression.Convert(Expression.Property(t, "data_fine_validita"), typeof(DateTime));
            Expression comparison3 = Expression.GreaterThan(
                Expression.Convert(
                    Expression.Property(t, "data_fine_validita"), typeof(DateTime)), Expression.Constant(DateTime.Now));

            Expression datafine = Expression.OrElse(comparison2, comparison3);
            Expression final = Expression.And(comparison, datafine);

            var e = source.Where(Expression.Lambda<Func<T, bool>>(final, t).Compile());
            return e;
        }
        
       
    }

}
