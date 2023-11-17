using myRai.DataAccess;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class FormAdminController : BaseCommonController
    {
        public ActionResult Index()
        {
            string matr = CommonHelper.GetCurrentUserMatricola();
            var db = new myRaiData.digiGappEntities();

            //List<myRaiData.MyRai_FormPrimario> model = db.MyRai_FormPrimario
            //    //.Where (x=>x.creato_da==matr)
            //    .OrderByDescending(x => x.id).ToList();
            return View();
        }

        public ActionResult listaforms(string titolo = "", int tipologia = -1)
        {
            string matr = CommonHelper.GetCurrentUserMatricola();
            var db = new myRaiData.digiGappEntities();

            var query = db.MyRai_FormPrimario
                .Include("MyRai_FormTipologiaForm")
                .Include("MyRai_FormSecondario")
                .Where(x => true);
            if (!String.IsNullOrWhiteSpace(titolo))
                query = query.Where(x => x.titolo.Contains(titolo));

            if (tipologia > 0)
                query = query.Where(x => x.id_tipologia == tipologia);


            List<myRaiData.MyRai_FormPrimario> model = query.OrderByDescending(x => x.id).ToList();

            return View(model);
        }

        public static SelectList GetTipologie()
        {
            var db = new myRaiData.digiGappEntities();
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Selected = true,
                Text = "Tutte",
                Value = "-1"
            });

            foreach (var item in db.MyRai_FormTipologiaForm)
            {
                list.Add(new SelectListItem()
                {
                    Selected = false,
                    Text = item.tipologia,
                    Value = item.id.ToString()
                });
            }
            return new SelectList(list, "Value", "Text");
        }

        public ActionResult getFormPrimario(int id)
        {
            if (id == 0)
                return View("popupFormPrimario", new FormPrimario() {  
                    attivo=true,
                    invia_mail_dopo_comp=true 
                });
            else
            {
                var fp = new FormPrimario();
                var db = new digiGappEntities();
                var dbFormPrimario = db.MyRai_FormPrimario.Where(x => x.id == id).FirstOrDefault();
                fp.anonimo = dbFormPrimario.anonimo;
                fp.barra_avanzamento = dbFormPrimario.barra_avanzamento;
                fp.data_fine_validita = dbFormPrimario.data_fine_validita.ToString("dd/MM/yyyy HH:mm");
                fp.data_inizio_validita = dbFormPrimario.data_inizio_validita.ToString("dd/MM/yyyy HH:mm");
                fp.descrizione = dbFormPrimario.descrizione;
                fp.id = dbFormPrimario.id;
                fp.id_tipologia = dbFormPrimario.id_tipologia;
                fp.messaggio_feedback = dbFormPrimario.messaggio_feedback;
                fp.precompilati_ammessi = dbFormPrimario.precompilati_ammessi;
                fp.titolo = dbFormPrimario.titolo;
                fp.attivo = dbFormPrimario.attivo;
                fp.invia_mail_dopo_comp = dbFormPrimario.invia_mail_comp;
                fp.mail_oggetto = dbFormPrimario.mail_oggetto;
                fp.mail_corpo = dbFormPrimario.mail_corpo;
                fp.vedi_statistiche_dopo_comp = dbFormPrimario.vedi_stats;
                fp.filtro_gruppo = dbFormPrimario.filtro_gruppo;
                fp.filtro_matricola = dbFormPrimario.filtro_matricola;
                fp.azione_fine_validita = dbFormPrimario.azione_fine_validita;
                fp.messaggio_fine_validita = dbFormPrimario.messaggio_fine_validita;

                return View("popupFormPrimario", fp);
            }
        }

        public ActionResult getFormSecondario(int id, int idPrimario)
        {
            var db = new digiGappEntities();
            var primario = db.MyRai_FormPrimario.Where(x => x.id == idPrimario).FirstOrDefault();
            var fp = new FormSecondario();
            if (id == 0)
            {
                fp.id_form_primario = primario.id;
                fp.titolo_form_primario = primario.titolo;
                fp.attivo = true;
                fp.Progressivo = primario.MyRai_FormSecondario.Count() + 1;
                return View("popupFormSecondario", fp);
            }
            else
            {
                var dbForm = db.MyRai_FormSecondario.Where(x => x.id == id).FirstOrDefault();
                fp.descrizione = dbForm.descrizione;
                fp.titolo_form_primario = dbForm.MyRai_FormPrimario.titolo;
                fp.titolo = dbForm.titolo;
                fp.attivo = dbForm.attivo;
                fp.Progressivo = dbForm.progressivo;
                return View("popupFormSecondario", fp);
            }
        }

        public ActionResult getFormQuestion(int id, int idSecondario)
        {
            var db = new digiGappEntities();
            var secondario = db.MyRai_FormSecondario.Where(x => x.id == idSecondario).FirstOrDefault();
            var fd = new FormDomanda();
            fd.DomandeMasterDisponibili_list = getDomandePossibili(idSecondario,id);
            if (id == 0)
            {
                fd.id_form_secondario = secondario.id;
                fd.titolo_form_primario = secondario.MyRai_FormPrimario.titolo;
                fd.titolo_form_secondario = secondario.titolo;
                fd.attiva = true;
                fd.obbligatoria = true;
                fd.Progressivo = secondario.MyRai_FormDomande.Where(d=>d.id_domanda_parent==null).Count() + 1;
                return View("popupFormQuestion", fd);
            }
            else
            {
                var dbFormDomanda = db.MyRai_FormDomande.Where(x => x.id == id).FirstOrDefault();
                fd.attiva = dbFormDomanda.attiva;
                fd.descrizione = dbFormDomanda.descrizione;
                fd.id_domanda_parent = dbFormDomanda.id_domanda_parent;
                fd.id_form_secondario = dbFormDomanda.id_form_secondario;
                fd.id_tipologia = dbFormDomanda.id_tipologia;
                fd.obbligatoria = dbFormDomanda.obbligatoria;
                fd.titolo = dbFormDomanda.titolo;
                fd.PrevedeSceltaRisposta = dbFormDomanda.MyRai_FormTipologieDomande.scelta_risposte == true ? true : false;
                fd.PermettiAltro = dbFormDomanda.permetti_altro;
                fd.titolo_form_primario = dbFormDomanda.MyRai_FormSecondario.MyRai_FormPrimario.titolo;
                fd.titolo_form_secondario = dbFormDomanda.MyRai_FormSecondario.titolo;
                fd.Progressivo = dbFormDomanda.progressivo;
                fd.id = dbFormDomanda.id;
                fd.max_scelte = dbFormDomanda.max_scelte;

                if (dbFormDomanda.MyRai_FormRispostePossibili !=null && dbFormDomanda.MyRai_FormRispostePossibili.Count >0)
                {
                    fd.risposte = dbFormDomanda.MyRai_FormRispostePossibili.Select(x => x.item_risposta).ToArray();
                }
              
                return View("popupFormQuestion", fd);
            }
        }

        public ActionResult FormPrimarioDelete(int id)
        {
            var db = new digiGappEntities();
            var fp = db.MyRai_FormPrimario.Where(x => x.id == id).FirstOrDefault();
            if (fp == null) return Content("Errore dati DB");
            else
            {
                fp.attivo = false;
                if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                    return Content( "OK" );
                else
                    return Content( "Errore salvataggio DB" );
            }
        }

        public ActionResult FormSecondarioDelete(int id)
        {
            var db = new myRaiData.digiGappEntities();
            var fp = db.MyRai_FormSecondario.Where(x => x.id == id).FirstOrDefault();
            if (fp == null) return Content("Errore dati DB");
            else
            {
                fp.attivo = false;
                if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                    return Content( "OK" );
                else
                    return Content( "Errore salvataggio DB" );
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult FormSecondarioSave(FormSecondario model)
        {
            var db = new digiGappEntities();
            Boolean IsNew = model.id == 0;

            MyRai_FormSecondario fs = null;

            if (IsNew) fs = new MyRai_FormSecondario();
            else
            {
                fs = db.MyRai_FormSecondario.Where(x => x.id == model.id).FirstOrDefault();
                if (fs == null) return Content("Errore dati nel DB");
            }
            fs.titolo = model.titolo;
            fs.descrizione = model.descrizione;
            fs.attivo = model.attivo;
            fs.progressivo = model.Progressivo;
            fs.MyRai_FormPrimario = db.MyRai_FormPrimario.Where(x => x.id == model.id_form_primario).FirstOrDefault();

            bool addQuestionHotel = false;
            if (IsNew)
            {
                if (fs.MyRai_FormPrimario.MyRai_FormTipologiaForm.tipologia== "Hotel" && 
                        fs.progressivo == 1 && 
                            db.MyRai_FormSecondario.Count(x => x.id_form_primario == model.id_form_primario && x.attivo) == 0)
                    addQuestionHotel = true;

                db.MyRai_FormSecondario.Add(fs);
            }
            bool success = DBHelper.Save(db , CommonHelper.GetCurrentUserMatricola( ) );
            if (success)
            {
                if (addQuestionHotel)
                {
                    FormDomanda fd = null;

                    fd = new FormDomanda();
                    fd.titolo = "Indica l'hotel che desideri recensire";
                    fd.descrizione = "Seleziona l'hotel in base alla regione, alla provincia e alla città";
                    fd.attiva = true;
                    fd.id_tipologia = (int)EnumTipologiaDomanda.ShortText;
                    fd.obbligatoria = true;
                    fd.Progressivo = 1;
                    fd.PermettiAltro = false;
                    fd.id_form_secondario = fs.id;
                    fd.max_scelte = 999999;

                    FormQuestionSave(fd);
                }
                return Content("OK");
            }
            else
                return Content("Salvataggio DB fallito");
        }

        public ActionResult FormQuestionDelete(int id)
        {
            var db = new myRaiData.digiGappEntities();
            var q = db.MyRai_FormDomande.Where(x => x.id == id).FirstOrDefault();
            if (q != null)
            {
                q.attiva = false;
                if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                    return Content( "OK" );
            }
            return Content("Errore Dati DB");
        }
        private bool ClearRisposte(int idDomanda)
        {
          var db = new myRaiData.digiGappEntities();
            var d= db.MyRai_FormDomande.Find(idDomanda);
            if (d != null)
            {
                if (d.MyRai_FormRispostePossibili != null && d.MyRai_FormRispostePossibili.Count > 0)
                {
                    int[] idrisp = d.MyRai_FormRispostePossibili.Select(x => x.id).ToArray();
                    foreach (var id in idrisp)
                    {
                        var risp = db.MyRai_FormRispostePossibili.Find(id);
                        if (risp != null)
                            db.MyRai_FormRispostePossibili.Remove(risp);
                    }
                    return DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) );
                }
                else 
                    return true;
            }
            else
                return false;

        }

        public SelectList getDomandePossibili(int id_sec, int id_domanda)
        {
            var db = new myRaiData.digiGappEntities();
            var list2 = new List<SelectListItem>();
            var form_sec = db.MyRai_FormSecondario.Find(id_sec);

            foreach (var item in form_sec.MyRai_FormDomande.Where(x => x.id_tipologia == 8 && x.id !=id_domanda))
            {
                list2.Add(new SelectListItem()
                {
                    Selected = false,
                    Text = item.titolo,
                    Value = item.id.ToString()
                });
            }
            return new SelectList(list2, "Value", "Text");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult FormQuestionSave(FormDomanda model)
        {
            var db = new digiGappEntities();
            Boolean IsNew = model.id == 0;
            if (IsNew && db.MyRai_FormDomande.Any(x => x.titolo == model.titolo
                && x.id_form_secondario ==model.id_form_secondario
                && (x.id_domanda_parent==null && model.id_domanda_parent==null || x.id_domanda_parent == model.id_domanda_parent)
                )) return Content("Titolo già esistente");

            MyRai_FormDomande fd = null;
            if (IsNew) fd = new MyRai_FormDomande();
            else
            {
                fd = db.MyRai_FormDomande.Where(x => x.id == model.id).FirstOrDefault();
                if (fd == null) return Content("Errore dati nel DB");
            }
            fd.titolo = model.titolo;
            fd.descrizione = model.descrizione;
            fd.attiva = model.attiva;
            fd.id_tipologia = model.id_tipologia;
            fd.obbligatoria = model.obbligatoria;
            fd.progressivo = model.Progressivo;
            fd.permetti_altro = model.PermettiAltro;
            fd.MyRai_FormSecondario = db.MyRai_FormSecondario.Where(x => x.id == model.id_form_secondario).FirstOrDefault();
            fd.id_domanda_parent = model.id_domanda_parent;
            fd.max_scelte = model.max_scelte;

            if (IsNew)
                db.MyRai_FormDomande.Add(fd);
            else
            {
                bool esito=ClearRisposte(fd.id);
                if (!esito) 
                    return Content("Si è verificato un errore DB. Potrebbero già essere presenti risposte a questa domanda");
            }

            if (
                model.id_tipologia == 2 ||
                model.id_tipologia == 3 || 
                model.id_tipologia == 4 || 
                model.id_tipologia == 7 ||
                model.id_tipologia == 9 
                )
            {
                if (model.risposte != null && model.risposte.Length > 0)
                {
                    foreach (string risposta in model.risposte)
                    {
                        if (! String.IsNullOrWhiteSpace(risposta))
                        {
                            MyRai_FormRispostePossibili fr = new MyRai_FormRispostePossibili();
                            fr.item_risposta = risposta;
                           
                            db.MyRai_FormRispostePossibili.Add(fr);
                            fd.MyRai_FormRispostePossibili.Add(fr);
                        }
                    }
                }
            }
            if (model.id_tipologia == 10 ) //rating 1-6
            {
                for (int i = 1; i < 7; i++)
                {
                    MyRai_FormRispostePossibili fr = new MyRai_FormRispostePossibili();
                    fr.item_risposta = i.ToString();

                    db.MyRai_FormRispostePossibili.Add(fr);
                    fd.MyRai_FormRispostePossibili.Add(fr);
                }
            }
            bool success = DBHelper.Save(db , CommonHelper.GetCurrentUserMatricola( ) );
            if (success)
                return Content("OK");
            else
                return Content("Salvataggio DB fallito");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult FormPrimarioSave(FormPrimario model)
        {
            var db = new digiGappEntities();
            Boolean IsNew = model.id == 0;

            if (IsNew && db.MyRai_FormPrimario.Any(x => x.titolo == model.titolo)) return Content("Titolo form già esistente");

            DateTime di;
            DateTime.TryParseExact(model.data_inizio_validita, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out di);

            DateTime df;
            DateTime.TryParseExact(model.data_fine_validita, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out df);

            if (df < di) return Content("Date errate");

            MyRai_FormPrimario fp = null;

            if (IsNew) fp = new myRaiData.MyRai_FormPrimario();
            else
            {
                fp = db.MyRai_FormPrimario.Where(x => x.id == model.id).FirstOrDefault();
                if (fp == null) return Content("Errore dati nel DB");
            }

            fp.anonimo = model.anonimo;
            fp.barra_avanzamento = model.barra_avanzamento;
            fp.creato_da = CommonHelper.GetCurrentUserMatricola();
            fp.data_fine_validita = df;
            fp.data_inizio_validita = di;
            fp.descrizione = model.descrizione;
            fp.id_tipologia = model.id_tipologia;
            fp.messaggio_feedback = model.messaggio_feedback;
            fp.precompilati_ammessi = model.precompilati_ammessi;
            fp.titolo = model.titolo;
            fp.attivo = model.attivo;
            fp.invia_mail_comp = model.invia_mail_dopo_comp;
            fp.vedi_stats = model.vedi_statistiche_dopo_comp;
            fp.filtro_gruppo = model.filtro_gruppo;
            fp.filtro_matricola = model.filtro_matricola;
            fp.azione_fine_validita = model.azione_fine_validita;
            fp.messaggio_fine_validita = model.messaggio_fine_validita;
            fp.mail_oggetto = model.mail_oggetto;
            if (fp.mail_corpo == "<p><br></p>")
                fp.mail_corpo = null;
            else
                fp.mail_corpo = model.mail_corpo;

            if (IsNew) db.MyRai_FormPrimario.Add(fp);

            bool success = DBHelper.Save(db , CommonHelper.GetCurrentUserMatricola( ) );
            if (success)
                return Content("OK");
            else
                return Content("Salvataggio DB fallito");
        }

        public ActionResult FormPrimarioCopy(int idFormPrimario)
        {
            var db = new digiGappEntities();

            MyRai_FormPrimario oldFp = db.MyRai_FormPrimario.FirstOrDefault(x => x.id == idFormPrimario);

            MyRai_FormPrimario newFp = new MyRai_FormPrimario();
            
            newFp.anonimo = oldFp.anonimo;
            newFp.barra_avanzamento = oldFp.barra_avanzamento;
            newFp.creato_da = CommonHelper.GetCurrentUserMatricola();
            newFp.data_fine_validita = oldFp.data_fine_validita;
            newFp.data_inizio_validita = oldFp.data_inizio_validita;
            newFp.descrizione = oldFp.descrizione;
            newFp.id_tipologia = oldFp.id_tipologia;
            newFp.messaggio_feedback = oldFp.messaggio_feedback;
            newFp.precompilati_ammessi = oldFp.precompilati_ammessi;
            newFp.titolo = oldFp.titolo;
            newFp.attivo = oldFp.attivo;
            newFp.invia_mail_comp = oldFp.invia_mail_comp;
            newFp.mail_oggetto = oldFp.mail_oggetto;
            newFp.mail_corpo = oldFp.mail_corpo;
            newFp.vedi_stats = oldFp.vedi_stats;
            newFp.filtro_gruppo = oldFp.filtro_gruppo;
            newFp.filtro_matricola = oldFp.filtro_matricola;
            newFp.azione_fine_validita = oldFp.azione_fine_validita;
            newFp.messaggio_fine_validita = oldFp.messaggio_fine_validita;

            foreach ( MyRai_FormSecondario oldFs in oldFp.MyRai_FormSecondario)
            {
                MyRai_FormSecondario newFs = new MyRai_FormSecondario();
                newFs.titolo = oldFs.titolo;
                newFs.descrizione = oldFs.descrizione;
                newFs.attivo = oldFs.attivo;
                newFs.progressivo = oldFs.progressivo;
                newFs.MyRai_FormPrimario = newFp;

                Dictionary<int, MyRai_FormDomande> dictOldNew = new Dictionary<int, MyRai_FormDomande>();

                foreach (MyRai_FormDomande oldFd in oldFs.MyRai_FormDomande)
                {
                    MyRai_FormDomande newFd = new MyRai_FormDomande();
                    dictOldNew.Add(oldFd.id, newFd);

                    newFd.titolo = oldFd.titolo;
                    newFd.descrizione = oldFd.descrizione;
                    newFd.attiva = oldFd.attiva;
                    newFd.id_tipologia = oldFd.id_tipologia;
                    newFd.obbligatoria = oldFd.obbligatoria;
                    newFd.progressivo = oldFd.progressivo;
                    newFd.permetti_altro = oldFd.permetti_altro;
                    newFd.MyRai_FormSecondario = newFs;
                    if (oldFd.id_domanda_parent.HasValue) //?
                        newFd.MyRai_FormDomande2 = dictOldNew[oldFd.id_domanda_parent.Value];
                    newFd.max_scelte = oldFd.max_scelte;

                    foreach (MyRai_FormRispostePossibili oldRp in oldFd.MyRai_FormRispostePossibili)
                    {
                        MyRai_FormRispostePossibili newRp = new MyRai_FormRispostePossibili();
                        newRp.item_risposta = oldRp.item_risposta;
                        newFd.MyRai_FormRispostePossibili.Add(newRp);
                    }

                    newFs.MyRai_FormDomande.Add(newFd);
                }

                newFp.MyRai_FormSecondario.Add(newFs);
            }

            db.MyRai_FormPrimario.Add(newFp);
            if (!DBHelper.Save(db, "Duplicazione questionario"))
            {
                return Content("Errore salvataggio DB");
            }

            return Content("OK");
        }
    }
}
