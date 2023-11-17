using myRai.Controllers.RaiAcademy;
using myRai.DataAccess;
using myRaiCommonModel;
using myRaiCommonModel.RaiAcademy;
using myRaiCommonModel.raiplace;
using myRaiDataTalentia;
using myRai.Data.CurriculumVitae;
using myRaiHelper;
using myRaiServiceHub.it.rai.servizi.sendmail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using myRaiCommonModel.DataControllers.RaiAcademy;

namespace myRai.Controllers
{
    public class FormazioneRichiesteModel
    {
        public FormazioneRichiesteModel()
        {
            MieRichieste = new List<TREQUESTS>();
            AltreRichieste = new List<TREQUESTS>();
        }

        public int IdPersona { get; set; }
        public List<TREQUESTS> MieRichieste { get; set; }
        public List<TREQUESTS> AltreRichieste { get; set; }
    }
    public class RichiestaModel
    {
        public TREQUESTS Richiesta { get; set; }
        public Corso Corso { get; set; }
    }
    public class RicercaRichiesteModel
    { 
        public string BoxDest { get; set; }

        public bool HasFilter { get; set; }

        public string Matricola { get; set; }
        public string Cognome { get; set; }
        public string Nome { get; set; }
        public string ResultView { get; set; }
        public int IdCorso { get; set; }
    }
    public class RichiestaInline
    {
        public int IdRichiesta { get; set; }
        public int IdRichiestaStep { get; set; }

        public int Oper { get; set; }
    }
    
    public class ApprovazioneFormazioneController : BaseCommonController
    {
        private static string _urlFotoParam = "";
        public static string GetUrlFoto(string matricola)
        {
            if (_urlFotoParam == "")
                _urlFotoParam = CommonHelper.GetParametro<string>(EnumParametriSistema.UrlImmagineDipendente);

            string url = _urlFotoParam;

            string s = DateTime.Now.ToString("ddMMyyyy") + matricola;
            int tot = 0;
            foreach (var c in s)
            {
                tot += int.Parse(c.ToString());
            }
            url = url.Replace("#MATR", matricola).Replace("#CK", tot.ToString().PadLeft(3, '0'));
            return url;
        }

        public static bool HasRichieste(string matricola)
        {
            TalentiaEntities db = new TalentiaEntities();
            SINTESI1 pers = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
            return false;
        }

        public ActionResult Index()
        {
            return View();
        }

        #region GestioneRichieste
        [HttpGet]
        public ActionResult ElencoRichieste()
        {
            return RicercaRichieste(new RicercaRichiesteModel());
        }

        [HttpPost]
        public ActionResult RicercaRichieste(RicercaRichiesteModel model)
        {
            TalentiaEntities db = new TalentiaEntities();

            FormazioneRichiesteModel formRichieste = new FormazioneRichiesteModel();

            string matricola = CommonHelper.GetCurrentUserMatricola();

            var pers = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
            formRichieste.IdPersona = pers.ID_PERSONA;

            var tmpElenco = db.TREQUESTS_STEP.Where(x => x.ID_PERSONA_FROM == pers.ID_PERSONA || x.ID_PERSONA_TO == pers.ID_PERSONA)
                .Select(a => a.TREQUESTS).Distinct()
                .Where(y => y.ID_PERSONA != pers.ID_PERSONA);

            if (model.HasFilter)
            {
                if (!String.IsNullOrWhiteSpace(model.Matricola))
                    tmpElenco = tmpElenco.Where(x => x.SINTESI1.COD_MATLIBROMAT == model.Matricola);

                if (!String.IsNullOrWhiteSpace(model.Cognome))
                    tmpElenco = tmpElenco.Where(x => x.SINTESI1.DES_COGNOMEPERS.StartsWith(model.Cognome));

                if (!String.IsNullOrWhiteSpace(model.Nome))
                    tmpElenco = tmpElenco.Where(x => x.SINTESI1.DES_NOMEPERS.StartsWith(model.Nome));

                if (model.IdCorso > 0)
                    tmpElenco = tmpElenco.Where(x => x.ID_CORSO == model.IdCorso);
            }

            var list = tmpElenco.ToList();

            formRichieste.MieRichieste.AddRange(list.Where(y => !y.TREQUESTS_STEP.Any(z => z.IND_CURRFORM != null) && y.TREQUESTS_STEP.OrderBy(x=>x.NUM_PROGR).Last().ID_PERSONA_TO==pers.ID_PERSONA));
            formRichieste.AltreRichieste.AddRange(list.Where(y => y.TREQUESTS_STEP.Any(z => z.IND_CURRFORM != null) || y.TREQUESTS_STEP.OrderBy(x => x.NUM_PROGR).Last().ID_PERSONA_TO != pers.ID_PERSONA));

            return PartialView("subpartial/ElencoRichieste", formRichieste);
        }

        public ActionResult DettaglioRichiesta(int idRichiesta)
        {
            TalentiaEntities db = new TalentiaEntities();
            RaiAcademyDataController data = new RaiAcademyDataController();

            RichiestaModel rich = new RichiestaModel();
            rich.Richiesta = db.TREQUESTS.FirstOrDefault(x => x.ID_TREQUESTS == idRichiesta);
            rich.Corso = data.EstraiCorsi(CommonHelper.GetCurrentUserMatricola(), true, rich.Richiesta.ID_CORSO.Value, true, false, StatoPubblicazione.NonDefinito).First();

            return PartialView("subpartial/DettaglioRichiesta", rich);
        }

        [HttpPost]
        public ActionResult RifiutaRichiesta(int idRichiestaStep, string motivo)
        {
            string content = "";

            TalentiaEntities db = new TalentiaEntities();
            var reqStep = db.TREQUESTS_STEP.FirstOrDefault(x => x.ID_TREQUESTS_STEP == idRichiestaStep);
            if (reqStep!=null)
            {
                DateTime current = DateTime.Now;

                reqStep.IND_APPROVED = false;
                reqStep.DTA_APPROVED = current;
                reqStep.IND_CURRFORM = false;
                reqStep.NOT_APPROVED = motivo;
                reqStep.COD_USER = CommonHelper.GetCurrentUserMatricola();
                reqStep.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                reqStep.TMS_TIMESTAMP=current;

                reqStep.TREQUESTS.IND_MANAPPROVED = "N";
                reqStep.TREQUESTS.NOT_MANAPPROVED = motivo;
                reqStep.TREQUESTS.COD_USER = reqStep.COD_USER;
                reqStep.TREQUESTS.COD_TERMID = reqStep.COD_TERMID;
                reqStep.TREQUESTS.TMS_TIMESTAMP = reqStep.TMS_TIMESTAMP;

                if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "Approvazione formazione" ) )
                {
                    string matr = CommonHelper.GetCurrentUserMatricola();

                    List<string> elencoMatricole = reqStep.TREQUESTS.TREQUESTS_STEP
                        .Select(x => x.SINTESI1_TO.COD_MATLIBROMAT).Distinct().Where(y => y != matr).ToList();

                    //mail con dest. utente orgiinale e cc con intermediari
                    MailSender invia = new MailSender();
                    Email eml = new Email();
                    eml.From = CommonHelper.GetEmailPerMatricola(matr);
                    eml.toList = new string[] { CommonHelper.GetEmailPerMatricola(reqStep.TREQUESTS.SINTESI1.COD_MATLIBROMAT) };
                    eml.ccList = elencoMatricole.Select(a => CommonHelper.GetEmailPerMatricola(a)).ToArray();

                    eml.ContentType = "text/html";
                    eml.Priority = 2;
                    eml.SendWhen = DateTime.Now.AddSeconds(1);

                    content = "OK";
                }
                else
                    content = "Errore durante il salvataggio";
            }
            else
            {
                content = "Messaggio inesistente";
            }

            return Content(content);
        }

        [HttpPost]
        public ActionResult ApprovaRichiesta(int idRichiestaStep, string motivo, string dest)
        {
            string content = "";

            TalentiaEntities db = new TalentiaEntities();
            var reqStep = db.TREQUESTS_STEP.FirstOrDefault(x => x.ID_TREQUESTS_STEP == idRichiestaStep);
            if (reqStep != null)
            {
                DateTime current = DateTime.Now;

                reqStep.IND_APPROVED = true;
                reqStep.DTA_APPROVED = current;
                reqStep.NOT_APPROVED = motivo;
                reqStep.COD_USER = CommonHelper.GetCurrentUserMatricola();
                reqStep.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                reqStep.TMS_TIMESTAMP = current;

                TREQUESTS_STEP newStep = new TREQUESTS_STEP();
                newStep.ID_TREQUESTS_STEP = db.TREQUESTS_STEP.GeneraOid(x => x.ID_TREQUESTS_STEP);
                newStep.ID_TREQUESTS = reqStep.ID_TREQUESTS;
                newStep.ID_EDIZIONE = reqStep.ID_EDIZIONE;
                newStep.NUM_PROGR = reqStep.NUM_PROGR+1;
                newStep.DTA_RICHIESTA = current;
                newStep.ID_PERSONA_FROM = reqStep.ID_PERSONA_TO;
                newStep.ID_PERSONA_TO = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == dest).ID_PERSONA;
                newStep.COD_USER = reqStep.COD_USER;
                newStep.COD_TERMID = reqStep.COD_TERMID;
                newStep.TMS_TIMESTAMP = reqStep.TMS_TIMESTAMP;
                reqStep.TREQUESTS.TREQUESTS_STEP.Add(newStep);

                if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "Approvazione formazione") )
                {
                    string matr = CommonHelper.GetCurrentUserMatricola();

                    //Email per nuova ID_PERSONA_TO
                    MailSender invia = new MailSender();
                    Email eml = new Email();
                    eml.From = CommonHelper.GetEmailPerMatricola(matr);
                    eml.toList = new string[] { CommonHelper.GetEmailPerMatricola(dest) };

                    eml.ContentType = "text/html";
                    eml.Priority = 2;
                    eml.SendWhen = DateTime.Now.AddSeconds(1);

                    content = "OK";
                }
                else
                    content = "Errore durante il salvataggio";
            }
            else
            {
                content = "Messaggio inesistente";
            }
            return Content(content);
        }
        
        public ActionResult GestisciInline(int oper, int idRichiesta, int idRichiestaStep)
        {
            RichiestaInline rich = new RichiestaInline();
            rich.Oper = oper;
            rich.IdRichiesta = idRichiesta;
            rich.IdRichiestaStep = idRichiestaStep;

            return PartialView("subpartial/GestioneInline", rich);
        }

        #endregion

        [HttpPost]
        public ActionResult GetCognomiRisorsa()
        {
            string matricola = CommonHelper.GetCurrentUserMatricola();
            int idRich = Convert.ToInt32(Request.Params["idRich"]);
            string cog = Request.Params["cog"];
            List<AnagSelect> lista = new List<AnagSelect>();

            List<string> elencoMatrRichiesta = new List<string>();
            using (TalentiaEntities db = new TalentiaEntities())
            {
                var richiesta = db.TREQUESTS.FirstOrDefault(x => x.ID_TREQUESTS == idRich);
                elencoMatrRichiesta.Add(richiesta.SINTESI1.COD_MATLIBROMAT);
                elencoMatrRichiesta.AddRange(richiesta.TREQUESTS_STEP.Select(x => x.SINTESI1_TO.COD_MATLIBROMAT));
            }

            // Se la parola cercata è uguale alla precedente
            // Oppure la vecchia parola cercata è parte della nuova + 1 char (es. vecchia "BIF" nuova "BIFA")
            // e la lista è già stata popolata per la vecchia parola ("BIF"), allora verranno filtrati i 
            // risultati già ottenuti senza dover chiamare nuovamente il servizio

            if (ApprovazioneFormazioneControllerScope.Instance.IdRichiesta == idRich &&
                cog.Equals(ApprovazioneFormazioneControllerScope.Instance.FiltroRicercaUtenti, StringComparison.InvariantCultureIgnoreCase) &&
                (ApprovazioneFormazioneControllerScope.Instance.AnagraficaUtenti != null &&
                ApprovazioneFormazioneControllerScope.Instance.AnagraficaUtenti.Any()))
            {
                ApprovazioneFormazioneControllerScope.Instance.AnagraficaUtenti.ForEach(r =>
                {
                    if (r.Matricola != matricola && !elencoMatrRichiesta.Contains(r.Matricola))
                    {
                        AnagSelect anag = new AnagSelect()
                        {
                            id = r.Matricola,
                            text = r.Cognome + "/" + r.Nome + "/" + r.Direzione,
                            matricola = r.Matricola
                        };

                        lista.Add(anag);
                    }
                });
            }
            else if (ApprovazioneFormazioneControllerScope.Instance.IdRichiesta == idRich &&
                        cog.StartsWith(ApprovazioneFormazioneControllerScope.Instance.FiltroRicercaUtenti) &&
                        (ApprovazioneFormazioneControllerScope.Instance.AnagraficaUtenti != null &&
                          ApprovazioneFormazioneControllerScope.Instance.AnagraficaUtenti.Any()) &&
                        cog.Length > ApprovazioneFormazioneControllerScope.Instance.FiltroRicercaUtenti.Length)
            {
                List<FormazioneAnagraficaUtente> subList = ApprovazioneFormazioneControllerScope.Instance.AnagraficaUtenti.Where(i => i.Nominativo.ToUpper().Contains(cog.ToUpper())).ToList();

                if (subList != null && subList.Any())
                {
                    ApprovazioneFormazioneControllerScope.Instance.AnagraficaUtenti = new List<FormazioneAnagraficaUtente>();

                    subList.ForEach(r =>
                    {
                        if (r.Matricola != matricola && !elencoMatrRichiesta.Contains(r.Matricola))
                        {
                            ApprovazioneFormazioneControllerScope.Instance.AnagraficaUtenti.Add(new FormazioneAnagraficaUtente()
                            {
                                Matricola = r.Matricola,
                                Nome = r.Nome,
                                Cognome = r.Cognome,
                                Direzione = r.Direzione,
                                Nominativo = r.Nominativo
                            });

                            AnagSelect anag = new AnagSelect()
                            {
                                id = r.Matricola,
                                text = r.Cognome + "/" + r.Nome + "/" + r.Direzione,
                                matricola = r.Matricola
                            };

                            lista.Add(anag);
                        }
                    });
                }
            }
            else
            {
                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    var rows = db.sp_RicercaUtenti(cog).ToList();

                    if (rows != null && rows.Any())
                    {
                        ApprovazioneFormazioneControllerScope.Instance.AnagraficaUtenti = new List<FormazioneAnagraficaUtente>();

                        rows.ForEach(r =>
                        {
                            if (r.Matricola != matricola && !elencoMatrRichiesta.Contains(r.Matricola) &&
                                (!(String.IsNullOrEmpty(r.Cognome) ||
                                String.IsNullOrEmpty(r.Nome) ||
                                String.IsNullOrEmpty(r.Sezione) ||
                                String.IsNullOrEmpty(r.Descrizione_Servizio)))
                                )
                            {
                                AnagSelect anag = new AnagSelect()
                                {
                                    id = r.Matricola,
                                    text = r.Cognome + "/" + r.Nome + "/" + r.Descrizione_Servizio,
                                    matricola = r.Matricola
                                };

                                lista.Add(anag);
                                ApprovazioneFormazioneControllerScope.Instance.AnagraficaUtenti.Add(new FormazioneAnagraficaUtente()
                                {
                                    Matricola = r.Matricola,
                                    Nome = r.Nome,
                                    Cognome = r.Cognome,
                                    Direzione = r.Descrizione_Sezione,
                                    Nominativo = String.Format("{0} {1}", r.Cognome, r.Nome)
                                });
                            }
                        });
                    }
                }
            }

            ApprovazioneFormazioneControllerScope.Instance.IdRichiesta = idRich;
            ApprovazioneFormazioneControllerScope.Instance.FiltroRicercaUtenti = cog;

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = lista }
            };
        }

        public static List<ListItem> getCorsiList()
        {
            List<ListItem> list = new List<ListItem>();
            list.Add(new ListItem() { Value = "0", Text = "Seleziona un corso", Selected = true });

            TalentiaEntities db = new TalentiaEntities();

            string matricola = CommonHelper.GetCurrentUserMatricola();
            var pers = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);

            list.AddRange(db.TREQUESTS_STEP.Where(x => x.ID_PERSONA_FROM == pers.ID_PERSONA || x.ID_PERSONA_TO == pers.ID_PERSONA)
                .Select(a => a.TREQUESTS).Distinct()
                .Select(b => b.CORSO).Distinct().ToList()
                .Select(c => new ListItem()
                {
                    Value = c.ID_CORSO.ToString(),
                    Text = c.COD_CORSO
                }));

            return list;
        }
    }

    public class ApprovazioneFormazioneControllerScope : SessionScope<ApprovazioneFormazioneControllerScope>
    {
        public ApprovazioneFormazioneControllerScope()
        {
            this.IdRichiesta = 0;
            this.FiltroRicercaUtenti = String.Empty;
            this.AnagraficaUtenti = new List<FormazioneAnagraficaUtente>();
        }

        /// <summary>
        /// Identificativo univoco della richiesta selezionata
        /// </summary>
        public int IdRichiesta
        {
            get;
            set;
        }

        /// <summary>
        /// Filtri utilizzati in ricerca
        /// </summary>
        public FiltriCatalogoModel Filtri
        {
            get;
            set;
        }

        public int PageSize
        {
            get;
            set;
        }

        public string FiltroRicercaUtenti { get; set; }
        public List<FormazioneAnagraficaUtente> AnagraficaUtenti { get; set; }
    }

    /// <summary>
    /// Definizione dell'oggetto che rappresenta i dati di output del servizio di ricerca utenti
    /// </summary>
    public class FormazioneAnagraficaUtente
    {
        public FormazioneAnagraficaUtente()
        {
        }

        public string Cognome { get; set; }
        public string Nome { get; set; }
        public string Matricola { get; set; }
        public string Direzione { get; set; }
        public string Nominativo
        {
            get
            {
                return String.Format("{0} {1}", this.Cognome, this.Nome);
            }
            set
            {
            }
        }
    }
}