using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRai.Business;
using myRai.Models;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;

namespace myRai.Controllers.raiplace
{
    public class EventiController : BaseCommonController
    {
        ModelDash pr = new ModelDash();

        public ActionResult Index()
        {
            var db = new digiGappEntities();
            pr.Events = new EventsModel();
            pr.Programs = new ProgramsModel();
            pr.menuSidebar = UtenteHelper.getSidebarModel();// new sidebarModel(3);

            return View("~/Views/RaiPlace/Eventi/Index.cshtml", pr);
        }

        public ActionResult RefreshProgrammi()
        {
            string titolo = Request.QueryString["titolo"];
            string luogo = Request.QueryString["luogo"];
            Int32 dataDa, dataA;

            if (Request.QueryString["data_da"] == "") dataDa = 0; else dataDa = Convert.ToInt32(Request.QueryString["data_da"]);
            if (Request.QueryString["data_a"] == "") dataA = 0; else dataA = Convert.ToInt32(Request.QueryString["data_a"]);

            ModelDash model = HomeManager.GetListaProgrammi(pr, titolo, luogo, dataDa, dataA);
            ModelState.Clear();

            return View("~/Views/tabelle/subpartial/listaprogrammi.cshtml", model);
        }

        public ActionResult RefreshEventi()
        {
            var db = new digiGappEntities();
            string titolo = Request.QueryString["titolo"];
            string luogo = Request.QueryString["luogo"];
            DateTime? dataDa, dataA;
            if (Request.QueryString["data_da"] == "") dataDa = null; else dataDa = DateTime.Parse(Request.QueryString["data_da"]);
            if (Request.QueryString["data_a"] == "") dataA = null; else dataA = DateTime.Parse(Request.QueryString["data_a"]);

            ModelDash model = HomeManager.GetListaEventi(pr, titolo, luogo, dataDa, dataA);
            ModelState.Clear();

            try
            {
                return View("~/Views/tabelle/subpartial/listaeventi.cshtml", model);
            }
            catch (Exception ex)
            {
                return null;
                
            }
        }

        public ActionResult DettaglioProgramma(int id)
        {
            var db = new digiGappEntities();

            pr.menuSidebar = UtenteHelper.getSidebarModel();// new sidebarModel(3);
            pr.ListaProgrammi = db.B2RaiPlace_Eventi_Programma.Where(x => x.id == id).ToList();

            return View("index", pr);
        }

        public JsonResult SelectProgramma(int id)
        {
            digiGappEntities db = new digiGappEntities();

            try
            {
                ProgramsModel model = new ProgramsModel();
                var programmi = model.elencoProgrammi.Where(a => a.id == id).Select(x => new
                                {
                                    titolo = x.titolo,
                                    luogo = x.luogo,
                                    numerototale = x.numero_medio,
                                    numeromassimo = x.numero_atteso,
                                    matricole_abilitate = x.B2RaiPlace_Eventi_Utenti_Abilitati.Count > 0 ? x.B2RaiPlace_Eventi_Utenti_Abilitati.Select(i => i.Matricola).Aggregate((i, j) => i + ";" + j) : "",
                                    sedi_abilitate = x.B2RaiPlace_Eventi_Sede.Count > 0 ? x.B2RaiPlace_Eventi_Sede.Select(i => i.sede_gapp).Aggregate((i, j) => i + ";" + j) : "",
                                    nota_email = x.testo_mail
                                }).ToList();

                return Json(programmi, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return null;

            }
        }

        public ActionResult Evento()
        {
            var db = new digiGappEntities();

            pr.menuSidebar = UtenteHelper.getSidebarModel();// new sidebarModel(3);
            pr.Events = new EventsModel();
            pr.Programs = new ProgramsModel();
            pr.Programs.listaProgrammi = new SelectList(db.B2RaiPlace_Eventi_Programma.OrderByDescending(a => a.id).ToList(), "id", "titolo");

            return View("~/Views/RaiPlace/Eventi/Evento/Index.cshtml", pr);
        }

        public ActionResult cancellaEvento(Int32 idEvento)
        {
            var db = new digiGappEntities();
            var prenotazione = db.B2RaiPlace_Eventi_Prenotazione.Where(x => x.idEvento == idEvento).FirstOrDefault();

            if (prenotazione != null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "l'evento non può essere cancellato, presenta delle prenotazioni." }
                };
            }
            else
            {
                var evento = db.B2RaiPlace_Eventi_Evento.Where(x => x.id == idEvento).FirstOrDefault();
                if (evento == null)
                {
                    Exception ex = new Exception();
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "errore: " + ex.Message }
                    };
                }
                else
                {
                    for (int i = evento.B2RaiPlace_Eventi_Campi.Count() - 1; i >= 0; i--)
                    {
                        db.B2RaiPlace_Eventi_Campi.Remove(evento.B2RaiPlace_Eventi_Campi.ElementAt(i));
                    }

                    if (evento.B2RaiPlace_Eventi_Sede != null && evento.B2RaiPlace_Eventi_Sede.Any())
                    {
                        for (int i = evento.B2RaiPlace_Eventi_Sede.Count() - 1; i >= 0; i--)
                        {
                            db.B2RaiPlace_Eventi_Sede.Remove(evento.B2RaiPlace_Eventi_Sede.ElementAt(i));
                        }
                    }

                    if (evento.B2RaiPlace_Eventi_Utenti_Abilitati != null && evento.B2RaiPlace_Eventi_Utenti_Abilitati.Any())
                    {
                        for (int i = evento.B2RaiPlace_Eventi_Utenti_Abilitati.Count() - 1; i >= 0; i--)
                        {
                            db.B2RaiPlace_Eventi_Utenti_Abilitati.Remove(evento.B2RaiPlace_Eventi_Utenti_Abilitati.ElementAt(i));
                        }
                    }

                    db.B2RaiPlace_Eventi_Evento.Remove(evento);

                    if (DataAccess.DBHelper.Save(db, CommonManager.GetCurrentUserMatricola()))
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "OK" }
                    };
                    else
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { result = "Si è verificato un errore durante il salvataggio." }
                        };

                }
            }
        }

        public ActionResult cancellaProgramma(Int32 idProgramma)
        {
            var db = new digiGappEntities();
            var evento = db.B2RaiPlace_Eventi_Evento.Where(x => x.id_programma == idProgramma).FirstOrDefault();

            if (evento != null)
            {
                var prenotazione = db.B2RaiPlace_Eventi_Prenotazione.Where(x => x.idEvento == evento.id).FirstOrDefault();
                if (prenotazione != null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Il programma non può essere cancellato, un'evento o più eventi collegati presentano delle prenotazioni." }
                    };
                }

                {
                    var programma = db.B2RaiPlace_Eventi_Programma.Where(x => x.id == idProgramma).FirstOrDefault();
                    if (programma == null)
                    {
                        Exception ex = new Exception();
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { result = "errore: " + ex.Message }
                        };
                    }
                    else
                    {
                        db.B2RaiPlace_Eventi_Programma.Remove(programma);
                        db.SaveChanges();
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { result = "OK" }
                        };

                    }
                }
            }
            else
            {
                var programma = db.B2RaiPlace_Eventi_Programma.Where(x => x.id == idProgramma).FirstOrDefault();
                var prenotazione = db.B2RaiPlace_Eventi_Prenotazione.Where(x => x.idProgramma == idProgramma).FirstOrDefault();
                if (prenotazione == null)
                {
                    db.B2RaiPlace_Eventi_Programma.Remove(programma);
                    db.SaveChanges();
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "OK" }
                    };
                }
                else
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Il programma non può essere cancellato, un'evento o più eventi collegati presentano delle prenotazioni." }
                    };
                }
            }
        }

        #region action richiamati dentro myrai.js
        public ActionResult _listaEvento(int idEvento)
        {
            var db = new digiGappEntities();
            pr.Programs = new ProgramsModel();
            pr.Programs.listaProgrammi = new SelectList(db.B2RaiPlace_Eventi_Programma.OrderByDescending(a => a.id).ToList(), "id", "titolo");

            pr.Events = new EventsModel();
            pr.Events.listaSedi = new SelectList(db.L2D_SEDE_GAPP, "cod_sede_gapp", "desc_sede_gapp");

            pr.menuSidebar = Utente.getSidebarModel();// new sidebarModel(3);
            pr.ListaEventi = db.B2RaiPlace_Eventi_Evento.Where(x => x.id == idEvento).ToList();
            return View("~/Views/RaiPlace/Eventi/Evento/_modificaevento.cshtml", pr);
        }

        public ActionResult _listaProgramma(int idProgramma)
        {
            var db = new digiGappEntities();
            pr.Programs = new ProgramsModel();

            pr.ListaProgrammi = db.B2RaiPlace_Eventi_Programma.Where(x => x.id == idProgramma).ToList();
            pr.Programs.listaSedi = new SelectList(db.L2D_SEDE_GAPP, "cod_sede_gapp", "desc_sede_gapp");
            pr.menuSidebar = Utente.getSidebarModel();// new sidebarModel(3);

            return View("~/Views/RaiPlace/Eventi/Programma/_modificaprogramma.cshtml", pr);
        }

        public ActionResult _nuovoEvento()
        {
            var db = new digiGappEntities();
            pr.Programs = new ProgramsModel();
            pr.Programs.listaProgrammi = new SelectList(db.B2RaiPlace_Eventi_Programma.OrderByDescending(a => a.id).ToList(), "id", "titolo");

            pr.Events = new EventsModel();
            pr.Events.listaSedi = new SelectList(db.L2D_SEDE_GAPP, "cod_sede_gapp", "desc_sede_gapp");
            pr.menuSidebar = Utente.getSidebarModel();// new sidebarModel(3);

            return View("~/Views/RaiPlace/Eventi/Evento/_nuovoevento.cshtml", pr);
        }

        public ActionResult _nuovoProgramma()
        {
            var db = new digiGappEntities();
            pr.Programs = new ProgramsModel();
            pr.Programs.listaSedi = new SelectList(db.L2D_SEDE_GAPP, "cod_sede_gapp", "desc_sede_gapp");
            pr.menuSidebar = Utente.getSidebarModel();// new sidebarModel(3);

            return View("~/Views/RaiPlace/Eventi/Programma/_nuovoprogramma.cshtml", pr);
        }

        public ActionResult _inserimentoEvento(string titoloIns = "", DateTime? dataInizio = null, string orarioInizio = "", string sedecontabile = "",
                                                string luogoIns = "",
                                               Int32 numeroTotale = 0, Int32 numeroMassimo = 0, DateTime? dataApertura = null,
                                               string orarioApertura = "", DateTime? dataChiusura = null,
                                               string orarioChiusura = "", Int32 programma = 0, string sedi = "", string matricole = "",
                                                string noteEmail = "", string checkInsediamento = "off", string ticket = "off",
                                                string check_vis_email = "off", string check_obl_email = "off", string check_vis_datnas = "off",
                                                string check_obl_datnas = "off", string check_vis_citnas = "off", string check_obl_citnas = "off",
                                                string check_vis_gen = "off", string check_obl_gen = "off", string check_vis_tel = "off",
                                                string check_obl_tel = "off", string check_vis_doc = "off", string check_obl_doc = "off",
                                                string check_vis_gra = "off", string check_obl_gra = "off", int limiteEta = 0,
                                                string check_vis_nota = "off", string check_obl_nota = "off"
                                                )
        {
            var db = new digiGappEntities();
            B2RaiPlace_Eventi_Evento insert = new B2RaiPlace_Eventi_Evento();
            B2RaiPlace_Eventi_Campi insertCampi = new B2RaiPlace_Eventi_Campi();
            try
            {
                if (titoloIns == "")
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un titolo" }
                    };
                }
                if (dataInizio == null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire una data evento valida" }
                    };
                }

                if (dataApertura == null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire una data apertura prenotazione valida" }
                    };
                }

                if (dataChiusura == null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire una data chiusura prenotazione valida" }
                    };
                }

                if (luogoIns == "")
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un luogo" }
                    };
                }

                if (numeroTotale == 0)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un numero valido di posti totali per l'evento" }
                    };
                }

                if (numeroMassimo == 0)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un numero valido di posti prenotabili per l'evento" }
                    };
                }
                if (numeroMassimo > numeroTotale)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un numero di posti prenotabili inferiore ai posti totali" }
                    };
                }
                insert.testo_mail = noteEmail;

                insert.sede_contabile = sedecontabile;
                insert.titolo = titoloIns;
                insert.data_inizio = DateTime.Parse(dataInizio.Value.ToShortDateString() + " " + orarioInizio + ":00");
                insert.data_fine = DateTime.Parse(dataInizio.Value.ToShortDateString() + " " + orarioInizio + ":00");
                insert.data_inizio_prenotazione = Convert.ToDateTime(dataApertura.Value.ToShortDateString() + " " + orarioApertura + ":00");
                insert.data_fine_prenotazione = Convert.ToDateTime(dataChiusura.Value.ToShortDateString() + " " + orarioChiusura + ":00");
                if (checkInsediamento == "on")
                {
                    insert.vedi_insediamento = true;
                }
                else
                {
                    insert.vedi_insediamento = false;
                }
                insert.luogo = luogoIns;
                if (insert.data_inizio_prenotazione >= insert.data_inizio)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "La data di prenotazione deve essere inferiore alla data inizio dell'evento" }
                    };
                }
                if (insert.data_fine_prenotazione >= insert.data_inizio)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "La data di fine prenotazione deve essere inferiore alla data inizio dell'evento" }
                    };
                }
                if (insert.data_inizio_prenotazione >= insert.data_fine_prenotazione)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "La data di inizio prenotazione deve essere inferiore alla data fine di prenotazione" }
                    };
                }

                insert.numero_totale = numeroTotale;
                insert.numero_massimo = numeroMassimo;

                if (limiteEta < 0)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un limite di età valido" }
                    };
                }
                insert.limite_eta = limiteEta;
                
                if (programma != 0)
                    insert.id_programma = programma;
                if (sedi != String.Empty)
                {
                    foreach (string sede in sedi.Split(';').ToList())
                    {
                        if (sede != String.Empty)
                        {
                            B2RaiPlace_Eventi_Sede evento_sede = new B2RaiPlace_Eventi_Sede();
                            evento_sede.sede_gapp = sede;
                            insert.B2RaiPlace_Eventi_Sede.Add(evento_sede);
                        }
                    }
                }
                if (matricole != String.Empty)
                {
                    foreach (string matricola in matricole.Split(';').ToList())
                    {
                        B2RaiPlace_Eventi_Utenti_Abilitati evento_matricola = new B2RaiPlace_Eventi_Utenti_Abilitati();
                        evento_matricola.Matricola = matricola;
                        insert.B2RaiPlace_Eventi_Utenti_Abilitati.Add(evento_matricola);
                    }
                }
                if (ticket == "on")
                {
                    insert.ticket = true;
                }
                else
                {
                    insert.ticket = false;
                }


                db.B2RaiPlace_Eventi_Evento.Add(insert);
                db.SaveChanges();

                insertCampi.Descrizione = "email";
                insertCampi.id_evento = insert.id;
                if (check_vis_email == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_email == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }
                db.B2RaiPlace_Eventi_Campi.Add(insertCampi);
                db.SaveChanges();

                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "genere";
                insertCampi.id_evento = insert.id;
                if (check_vis_gen == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_gen == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }
                db.B2RaiPlace_Eventi_Campi.Add(insertCampi);
                db.SaveChanges();

                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "telefono";
                insertCampi.id_evento = insert.id;
                if (check_vis_tel == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_tel == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }
                db.B2RaiPlace_Eventi_Campi.Add(insertCampi);
                db.SaveChanges();

                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "doc";
                insertCampi.id_evento = insert.id;
                if (check_vis_doc == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_doc == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }
                db.B2RaiPlace_Eventi_Campi.Add(insertCampi);
                db.SaveChanges();

                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "grado";
                insertCampi.id_evento = insert.id;
                if (check_vis_gra == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_gra == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }
                db.B2RaiPlace_Eventi_Campi.Add(insertCampi);
                db.SaveChanges();

                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "citta";
                insertCampi.id_evento = insert.id;
                if (check_vis_citnas == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_citnas == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }

                db.B2RaiPlace_Eventi_Campi.Add(insertCampi);
                db.SaveChanges();

                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "datanascita";
                insertCampi.id_evento = insert.id;

                if (check_vis_datnas == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_datnas == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }
                db.B2RaiPlace_Eventi_Campi.Add(insertCampi);
                db.SaveChanges();


                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "nota";
                insertCampi.id_evento = insert.id;

                if (check_vis_nota == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_nota == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }


                db.B2RaiPlace_Eventi_Campi.Add(insertCampi);
                db.SaveChanges();

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "OK" }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "errore: " + ex.Message }
                };
            }
        }

        public ActionResult _inserimentoProgramma(string titolo = "", Int32 anno = 0, string luogo = "", string admin = "",
                                                  Int32 numeroMedio = 0, Int32 numeroMassimo = 0, Int32 numeroAtteso = 0, string sedi = "", string matricole = "",
                                                    string testo_mail = "")
        {
            var db = new digiGappEntities();
            B2RaiPlace_Eventi_Programma insert = new B2RaiPlace_Eventi_Programma();

            try
            {


                if (titolo == "")
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un titolo" }
                    };
                }
                if (anno == 0)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un'anno valido" }
                    };
                }

                if (admin == "")
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un amministratore" }
                    };
                }

                if (luogo == "")
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un luogo" }
                    };
                }

                if (numeroMedio == 0)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire la capienza totale" }
                    };
                }

                if (numeroMassimo == 0)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire il limite per evento" }
                    };
                }

                if (numeroAtteso == 0)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire il limite per programma" }
                    };
                }

                insert.titolo = titolo;
                insert.anno = anno;
                insert.amministrazione = admin;
                insert.luogo = luogo;
                insert.testo_mail = testo_mail;

                insert.numero_atteso = numeroAtteso;
                insert.numero_massimo = numeroMassimo;
                insert.numero_medio = numeroMedio;
                if (sedi != String.Empty)
                {
                    foreach (string sede in sedi.Split(';').ToList())
                    {
                        if (sede != String.Empty)
                        {
                            B2RaiPlace_Eventi_Sede evento_sede = new B2RaiPlace_Eventi_Sede();
                            evento_sede.sede_gapp = sede;
                            insert.B2RaiPlace_Eventi_Sede.Add(evento_sede);
                        }
                    }
                }
                if (matricole != String.Empty)
                {
                    foreach (string matricola in matricole.Split(';').ToList())
                    {
                        B2RaiPlace_Eventi_Utenti_Abilitati evento_matricola = new B2RaiPlace_Eventi_Utenti_Abilitati();
                        evento_matricola.Matricola = matricola;
                        insert.B2RaiPlace_Eventi_Utenti_Abilitati.Add(evento_matricola);
                    }
                }

                db.B2RaiPlace_Eventi_Programma.Add(insert);
                db.SaveChanges();

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "OK" }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "errore: " + ex.Message }
                };
            }
        }

        public ActionResult _modificaProgramma(Int32 idProgramma = 0, string titolo = "", Int32 anno = 0, string luogo = "", string admin = "",
                                                  Int32 numeroMedio = 0, Int32 numeroMassimo = 0, Int32 numeroAtteso = 0, string sedi = "", string matricole = "",
                                                string testo_mail = "")
        {
            var db = new digiGappEntities();
            B2RaiPlace_Eventi_Programma insert = db.B2RaiPlace_Eventi_Programma.Where(x => x.id == idProgramma).FirstOrDefault();
            if (insert == null)
            {
                // Exception ex = new Exception();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "Errore - elemento non trovato" }
                };
            }
            else
            {
                try
                {


                    if (titolo == "")
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { result = "Inserire un titolo" }
                        };
                    }

                    if (anno == 0)
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { result = "Inserire un'anno valido" }
                        };
                    }

                    if (admin == "")
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { result = "Inserire un amministratore" }
                        };
                    }

                    if (luogo == "")
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { result = "Inserire un luogo" }
                        };
                    }

                    if (numeroMedio == 0)
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { result = "Inserire un numero valido di posti totali per il programma" }
                        };
                    }

                    if (numeroMassimo == 0)
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { result = "Inserire un numero atteso di posti per il programma" }
                        };
                    }

                    if (numeroAtteso == 0)
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { result = "Inserire un numero valido di posti totali per il programma" }
                        };
                    }

                    insert.titolo = titolo;
                    insert.amministrazione = admin;
                    insert.anno = anno;
                    insert.luogo = luogo;
                    insert.testo_mail = testo_mail;

                    insert.numero_atteso = numeroAtteso;
                    insert.numero_massimo = numeroMassimo;
                    insert.numero_medio = numeroMedio;

                    List<B2RaiPlace_Eventi_Sede> listadel = new List<B2RaiPlace_Eventi_Sede>(insert.B2RaiPlace_Eventi_Sede);
                    foreach (B2RaiPlace_Eventi_Sede sededel in listadel)
                    {
                        insert.B2RaiPlace_Eventi_Sede.Remove(sededel);
                    }

                    if (sedi != String.Empty)
                    {
                        foreach (string sede in sedi.Split(';').ToList())
                        {
                            if (sede != String.Empty)
                            {
                                B2RaiPlace_Eventi_Sede evento_sede = new B2RaiPlace_Eventi_Sede();
                                evento_sede.sede_gapp = sede;
                                insert.B2RaiPlace_Eventi_Sede.Add(evento_sede);
                            }
                        }
                    }
                    List<B2RaiPlace_Eventi_Utenti_Abilitati> listamatrdel = new List<B2RaiPlace_Eventi_Utenti_Abilitati>(insert.B2RaiPlace_Eventi_Utenti_Abilitati);
                    foreach (B2RaiPlace_Eventi_Utenti_Abilitati sededel in listamatrdel)
                    {
                        insert.B2RaiPlace_Eventi_Utenti_Abilitati.Remove(sededel);
                    }

                    if (matricole != String.Empty)
                    {
                        foreach (string matricola in matricole.Split(';').ToList())
                        {
                            B2RaiPlace_Eventi_Utenti_Abilitati evento_matricola = new B2RaiPlace_Eventi_Utenti_Abilitati();
                            evento_matricola.Matricola = matricola;
                            insert.B2RaiPlace_Eventi_Utenti_Abilitati.Add(evento_matricola);
                        }
                    }

                    db.SaveChanges();

                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "OK" }
                    };
                }
                catch (Exception ex)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "errore: " + ex.Message }
                    };
                }
            }
        }

        public ActionResult _modificaEvento(Int32 idEvento = 0, string titolo = "", DateTime? dataInizio = null, string orarioInizio = "", string sedecontabile="",
                                                 string luogo = "",
                                                Int32 numeroTotale = 0, Int32 numeroMassimo = 0, DateTime? dataApertura = null,
                                                string orarioApertura = "", DateTime? dataChiusura = null,
                                                string orarioChiusura = "", Int32 programmaupd = 0, string sediupd = "", string matricoleupd = "", 
                                                string noteEmail = "", string checkInsediamento = "off", string ticket = "off",
                                                string check_vis_email = "off", string check_obl_email = "off", string check_vis_datnas = "off",
                                                string check_obl_datnas = "off", string check_vis_citnas = "off", string check_obl_citnas = "off",
                                                string check_vis_gen = "off", string check_obl_gen = "off", string check_vis_tel = "off",
                                                string check_obl_tel = "off", string check_vis_doc = "off", string check_obl_doc = "off",
                                                string check_vis_gra = "off", string check_obl_gra = "off", int limiteEta = 0,
                                                string check_vis_nota = "off", string check_obl_nota = "off")
        {
            var db = new digiGappEntities();
            try
            {
                var insert = db.B2RaiPlace_Eventi_Evento.Where(x => x.id == idEvento).FirstOrDefault();
                B2RaiPlace_Eventi_Campi insertCampi = new B2RaiPlace_Eventi_Campi();
                if (titolo == "")
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un titolo" }
                    };
                }
                if (dataInizio == null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire una data evento valida" }
                    };
                }

                if (dataApertura == null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire una data apertura prenotazione valida" }
                    };
                }

                if (dataChiusura == null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire una data chiusura prenotazione valida" }
                    };
                }

                if (luogo == "")
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un luogo" }
                    };
                }

                if (numeroTotale == 0)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un numero valido di posti totali per l'evento" }
                    };
                }

                if (numeroMassimo == 0)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un numero valido di posti prenotabili per l'evento" }
                    };
                }
                if (numeroMassimo > numeroTotale)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un numero di posti prenotabili inferiore ai posti totali" }
                    };
                }
                insert.testo_mail = noteEmail;


                insert.titolo = titolo;
                insert.sede_contabile = sedecontabile;
                insert.data_inizio = DateTime.Parse(dataInizio.Value.ToShortDateString() + " " + orarioInizio + ":00");
                insert.data_fine = DateTime.Parse(dataInizio.Value.ToShortDateString() + " " + orarioInizio + ":00");
                insert.data_inizio_prenotazione = Convert.ToDateTime(dataApertura.Value.ToShortDateString() + " " + orarioApertura + ":00");
                insert.data_fine_prenotazione = Convert.ToDateTime(dataChiusura.Value.ToShortDateString() + " " + orarioChiusura + ":00");
                insert.luogo = luogo;

                if (insert.data_inizio_prenotazione >= insert.data_inizio)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "La data di prenotazione deve essere inferiore alla data inizio dell'evento" }
                    };
                }
                if (insert.data_fine_prenotazione >= insert.data_inizio)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "La data di fine prenotazione deve essere inferiore alla data inizio dell'evento" }
                    };
                }
                if (insert.data_inizio_prenotazione >= insert.data_fine_prenotazione)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "La data di inizio prenotazione deve essere inferiore alla data fine di prenotazione" }
                    };
                }
                if (checkInsediamento == "on")
                {
                    insert.vedi_insediamento = true;
                }
                else
                {
                    insert.vedi_insediamento = false;
                }
                insert.numero_totale = numeroTotale;
                insert.numero_massimo = numeroMassimo;
                if (limiteEta < 0)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "Inserire un limite di età valido" }
                    };
                }
                insert.limite_eta = limiteEta;


                if (programmaupd != 0)
                    insert.id_programma = programmaupd;


                List<B2RaiPlace_Eventi_Sede> listadel = new List<B2RaiPlace_Eventi_Sede>(insert.B2RaiPlace_Eventi_Sede);
                foreach (B2RaiPlace_Eventi_Sede sededel in listadel)
                {
                    insert.B2RaiPlace_Eventi_Sede.Remove(sededel);
                }

                if (sediupd != String.Empty)
                {
                    foreach (string sede in sediupd.Split(';').ToList())
                    {
                        if (sede != String.Empty)
                        {
                            B2RaiPlace_Eventi_Sede evento_sede = new B2RaiPlace_Eventi_Sede();
                            evento_sede.sede_gapp = sede;
                            insert.B2RaiPlace_Eventi_Sede.Add(evento_sede);
                        }
                    }
                }
                List<B2RaiPlace_Eventi_Utenti_Abilitati> listamatrdel = new List<B2RaiPlace_Eventi_Utenti_Abilitati>(insert.B2RaiPlace_Eventi_Utenti_Abilitati);
                foreach (B2RaiPlace_Eventi_Utenti_Abilitati sededel in listamatrdel)
                {
                    insert.B2RaiPlace_Eventi_Utenti_Abilitati.Remove(sededel);
                }

                if (matricoleupd != String.Empty)
                {
                    foreach (string matricola in matricoleupd.Split(';').ToList())
                    {
                        B2RaiPlace_Eventi_Utenti_Abilitati evento_matricola = new B2RaiPlace_Eventi_Utenti_Abilitati();
                        evento_matricola.Matricola = matricola;
                        insert.B2RaiPlace_Eventi_Utenti_Abilitati.Add(evento_matricola);
                    }
                }

                
                if (ticket == "on")
                {
                    insert.ticket = true;
                }
                else
                {
                    insert.ticket = false;
                }

                db.SaveChanges();

                List<B2RaiPlace_Eventi_Campi> listacampi = new List<B2RaiPlace_Eventi_Campi>(insert.B2RaiPlace_Eventi_Campi);
                foreach (B2RaiPlace_Eventi_Campi campo in listacampi)
                {
                    db.B2RaiPlace_Eventi_Campi.Remove(campo);
                }

                insert.B2RaiPlace_Eventi_Campi = new List<B2RaiPlace_Eventi_Campi>();



                insertCampi.Descrizione = "email";
                insertCampi.id_evento = insert.id;
                if (check_vis_email == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_email == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }
                insert.B2RaiPlace_Eventi_Campi.Add(insertCampi);


                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "genere";
                insertCampi.id_evento = insert.id;
                if (check_vis_gen == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_gen == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }
                insert.B2RaiPlace_Eventi_Campi.Add(insertCampi);


                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "telefono";
                insertCampi.id_evento = insert.id;
                if (check_vis_tel == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_tel == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }
                insert.B2RaiPlace_Eventi_Campi.Add(insertCampi);


                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "doc";
                insertCampi.id_evento = insert.id;
                if (check_vis_doc == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_doc == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }
                insert.B2RaiPlace_Eventi_Campi.Add(insertCampi);


                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "grado";
                insertCampi.id_evento = insert.id;
                if (check_vis_gra == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_gra == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }
                insert.B2RaiPlace_Eventi_Campi.Add(insertCampi);


                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "citta";
                insertCampi.id_evento = insert.id;
                if (check_vis_citnas == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_citnas == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }

                insert.B2RaiPlace_Eventi_Campi.Add(insertCampi);


                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "datanascita";
                insertCampi.id_evento = insert.id;

                if (check_vis_datnas == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_datnas == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }

                insert.B2RaiPlace_Eventi_Campi.Add(insertCampi);


                insertCampi = new B2RaiPlace_Eventi_Campi();
                insertCampi.Descrizione = "nota";
                insertCampi.id_evento = insert.id;

                if (check_vis_nota == "on")
                {
                    insertCampi.Visibile = true;
                }
                else
                {
                    insertCampi.Visibile = false;
                }
                if (check_obl_nota == "on")
                {
                    insertCampi.Obbligatorio = true;
                }
                else
                {
                    insertCampi.Obbligatorio = false;
                }


                insert.B2RaiPlace_Eventi_Campi.Add(insertCampi);
                db.SaveChanges();


                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "OK" }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "errore: " + ex.Message }
                };
            }
        }


        #endregion
    }
}
