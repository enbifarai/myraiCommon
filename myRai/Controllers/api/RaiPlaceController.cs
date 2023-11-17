using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using myRaiData;
using System.Web.Http.Filters;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using myRai.DataAccess;
using myRaiHelper;
using myRaiCommonManager;
using myRaiCommonModel;
using myRai.Data.CurriculumVitae;
using myRai.Business;

namespace myRai.Controllers.api
{
    public class PasswordRichiesta : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var db = new digiGappEntities();

            var AccessiApi = db.MyRai_AccessoAPI.ToList();
            string action = actionContext.ActionDescriptor.ActionName.ToLower();

            var keyString = actionContext.ControllerContext.Request.Headers.Where(x => x.Key.ToLower() == "keystring").FirstOrDefault();

            if (keyString.Key == null)
            {
                var Authorized = AccessiApi
                        .Where(x => String.IsNullOrWhiteSpace(x.KeyString)
                                     &&
                                     (String.IsNullOrWhiteSpace(x.ActionPermesse) || x.ActionPermesse.ToLower().Contains(action))
                                     &&
                                    (String.IsNullOrWhiteSpace(x.ActionNegate) || !x.ActionNegate.Split(',').Contains(action)))
                        .ToList().Count > 0;
                LogAPIcall(action, null, Authorized, actionContext);

                if (Authorized)
                {
                    base.OnActionExecuting(actionContext);
                }
                else
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("NON AUTORIZZATO") };
                }
            }
            else
            {
                string keyStringValue = keyString.Value.FirstOrDefault();
                if (keyStringValue != null)
                {
                    var Authorized = AccessiApi
                      .Where(x => x.KeyString.ToLower() == keyStringValue.ToLower()
                                   &&
                                   (String.IsNullOrWhiteSpace(x.ActionPermesse) || x.ActionPermesse.ToLower().Contains(action))
                                   &&
                                  (String.IsNullOrWhiteSpace(x.ActionNegate) || !x.ActionNegate.Split(',').Contains(action)))
                      .ToList().Count > 0;
                    LogAPIcall(action, keyStringValue, Authorized, actionContext);
                    if (Authorized)
                    {
                        base.OnActionExecuting(actionContext);
                    }
                    else
                    {
                        actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("NON AUTORIZZATO") };
                    }
                    return;
                }
                else
                {
                    LogAPIcall(action, keyStringValue, false, actionContext);
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("NON AUTORIZZATO") };
                }
            }
        }

        public void LogAPIcall(string action, string keyString, Boolean success, System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var db = new digiGappEntities();
            try
            {
                MyRai_LogAPI a = new MyRai_LogAPI()
                {
                    API = action,
                    Data = DateTime.Now,
                    KeyString = keyString,
                    Parametri = System.Web.HttpContext.Current.Request.Url.ToString(),
                    IP = System.Web.HttpContext.Current.Request.UserHostAddress,
                    UserAgent = System.Web.HttpContext.Current.Request.UserAgent,
                    Authorized = success
                };
                db.MyRai_LogAPI.Add(a);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "API",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    provenienza = "LogAPIcall"
                });
            }
        }
    }

    [PasswordRichiesta]
    public class RaiPlaceController : ApiController
    {
        Boolean esito = false;
        string strMex = "";

        #region C.R.U.D Evento

        public Evento getEvento(Int32 idEvento)
        {

            //+++ DA RIVEDERE TUTTI I DATI RESTITUITI

            var db = new digiGappEntities();
            string programmaTitolo = "Nessun programma associato a quest'evento"; ;
            List<matricole> _mat = new List<matricole>();
            List<sedi> _sedi = new List<sedi>();

            int idProgramma = 0;

            var item = db.B2RaiPlace_Eventi_Evento.Where(x => x.id == idEvento).FirstOrDefault();

            if (item == null)
            {
                strMex = "Errore";
            }
            else
            {
                esito = true;
                strMex = "OK";
                var programs = db.B2RaiPlace_Eventi_Programma.Where(x => x.id == item.id_programma).FirstOrDefault();
                if (programs != null) { programmaTitolo = programs.titolo; idProgramma = programs.id; }

                List<B2RaiPlace_Eventi_Sede> listSedi = db.B2RaiPlace_Eventi_Sede.Where(x => x.id_evento == item.id && x.id_programma == item.id_programma).ToList();
                //List<B2RaiPlace_Eventi_Utenti_Abilitati> listMatricole = db.B2RaiPlace_Eventi_Utenti_Abilitati.Where(x => x.Id_Evento == item.id && x.Id_Programma == item.id_programma).ToList();
                List<B2RaiPlace_Eventi_Utenti_Abilitati> listMatricole = db.B2RaiPlace_Eventi_Utenti_Abilitati.Where(x => x.Id_Evento == item.id ).ToList();

                if (listMatricole.Count > 0)
                {
                    foreach (var itemMat in listMatricole)
                    {
                        _mat.Add(new matricole { matricola = itemMat.Matricola, livello = itemMat.Livello });
                    }
                }

                if (listSedi.Count > 0)
                {

                    foreach (var itemSede in listSedi)
                    {
                        _sedi.Add(new sedi { sedeGapp = itemSede.sede_gapp });
                    }
                }


            }
            Evento lst = new Evento();

            lst.resultListaEvento = new ListEvento();

            lst.esito = esito;
            lst.message = strMex;

            if (esito == true)
            {
                lst.resultListaEvento.idEvento = item.id;
                lst.resultListaEvento.titolo = item.titolo;
                lst.resultListaEvento.dataInizio = item.data_inizio;
                lst.resultListaEvento.dataFine = item.data_fine;
                lst.resultListaEvento.luogo = item.luogo;
                lst.resultListaEvento.dataInizioPrenotazione = item.data_inizio_prenotazione;
                lst.resultListaEvento.dataFinePrenotazione = item.data_fine_prenotazione;
                lst.resultListaEvento.numeroMassimo = item.numero_massimo;
                lst.resultListaEvento.numeroTotale = item.numero_totale;
                lst.resultListaEvento.idProgramma = idProgramma;
                lst.resultListaEvento.programma = programmaTitolo;
                lst.resultListaEvento.listaMatricoleAbilitate = _mat;
                lst.resultListaEvento.listaSediAbilitate = _sedi;
            }

            return lst;
        }

        public string postEvento([FromBody] ListEvento parametriEvento)
        {
            var db = new digiGappEntities();

            B2RaiPlace_Eventi_Evento insert = new B2RaiPlace_Eventi_Evento();

            if (parametriEvento.titolo == "" || parametriEvento.titolo == null) return "Inserire Titolo";

            if (parametriEvento.dataInizio == null) return "Inserire Data Inizio Evento";
            if (parametriEvento.dataFine == null) return "Inserire Data Fine Evento";
            if (parametriEvento.dataInizioPrenotazione == null) return "Inserire data inizio prenotazione";
            if (parametriEvento.dataFinePrenotazione == null) return "Inserire data fine prenotazione";
            if (parametriEvento.luogo == "" || parametriEvento.luogo == null) return "Inserire Luogo";
            if (parametriEvento.numeroTotale == 0 || parametriEvento.numeroTotale == null) return "Inserire numero totale posti";
            if (parametriEvento.numeroMassimo == 0 || parametriEvento.numeroMassimo == null) return "Inserire numero massimo posti";
            if (parametriEvento.idProgramma == 0) return "Inserire ID del programma associato all'evento";

            insert.titolo = parametriEvento.titolo;
            insert.data_inizio = parametriEvento.dataInizio;
            insert.data_fine = parametriEvento.dataFine;
            insert.data_inizio_prenotazione = parametriEvento.dataInizioPrenotazione;
            insert.data_fine_prenotazione = parametriEvento.dataFinePrenotazione;
            insert.luogo = parametriEvento.luogo;

            insert.numero_totale = parametriEvento.numeroTotale;
            insert.numero_massimo = parametriEvento.numeroMassimo;
            insert.id_programma = parametriEvento.idProgramma;

            db.B2RaiPlace_Eventi_Evento.Add(insert);
            db.SaveChanges();

            return "Inserimento evento avvenuto con successo";
        }

        public string deleteEvento(Int32 idEvento, Int32 idProgramma)
        {
            var db = new digiGappEntities();
            var item = db.B2RaiPlace_Eventi_Evento.Where(x => x.id == idEvento && x.id_programma == idProgramma).FirstOrDefault();
            var prenotazione = db.B2RaiPlace_Eventi_Prenotazione.Where(x => x.idEvento == idEvento && x.idProgramma == idProgramma).FirstOrDefault();
            if (prenotazione != null)
            {
                return "Impossibile cancellare l'evento in quanto ci sono prenotazioni collegate all'evento, cancellare prima le prenotazioni e poi procedere alla cancellazione dell'evento";
            }
            else
            {
                if (item != null)
                {
                    db.B2RaiPlace_Eventi_Evento.Remove(item);
                    db.SaveChanges();
                    return "Cancellazione evento avvenuta con successo";
                }
                else
                { return "Impossibile cancellare l'evento, non esiste"; }
            }

        }

        public string putEvento(Int32 idEvento, Int32 idProgramma, [FromBody] ListEvento parametriEvento)
        {
            var db = new digiGappEntities();
            var item = db.B2RaiPlace_Eventi_Evento.Where(x => x.id == idEvento && x.id_programma == idProgramma).FirstOrDefault();
            if (item != null)
            {
                item.id_programma = idProgramma;
                item.luogo = parametriEvento.luogo;
                item.numero_totale = parametriEvento.numeroTotale;
                item.numero_massimo = parametriEvento.numeroMassimo;
                item.titolo = parametriEvento.titolo;
                item.data_inizio = parametriEvento.dataInizio;
                item.data_fine = parametriEvento.dataFine;
                item.data_inizio_prenotazione = parametriEvento.dataInizioPrenotazione;
                item.data_fine_prenotazione = parametriEvento.dataFinePrenotazione;

                db.SaveChanges();
                return "Modifica dell'evento avvenuta con successo";
            }
            else
            {
                { return "Impossibile modificare l'evento, non esiste"; }
            }

        }

        #endregion

        #region C.R.U.D Prenotazione Evento

        public Prenotazione getPrenotazione(Int32 matricola)
        {
            var db = new digiGappEntities();
            var listPrenotazioni = db.B2RaiPlace_Eventi_Prenotazione.Where(x => x.matricola == matricola).ToList();
            List<listPrenotazione> _list = new List<listPrenotazione>();

            listPrenotazione lp = new listPrenotazione();
            Prenotazione lst = new Prenotazione();

            if (listPrenotazioni.Count > 0)
            {
                lst.esito = true;
                lst.message = "OK";
                foreach (var item in listPrenotazioni)
                {
                    var evento = db.B2RaiPlace_Eventi_Evento.Where(x => x.id == item.idEvento).FirstOrDefault();
                    var programma = db.B2RaiPlace_Eventi_Programma.Where(x => x.id == item.idProgramma).FirstOrDefault();
                    lp.idEvento = item.idEvento;
                    lp.idProgramma = item.idProgramma;
                    lp.matricola = item.matricola;
                    lp.titoloProgramma = programma.titolo;
                    lp.titoloEvento = evento.titolo;
                    lp.posti = item.posti;
                    _list.Add(lp);

                }
                lst.result = _list;
            }
            else
            {
                lst.esito = esito;
                lst.message = "Errore";
            }

            return lst;
        }

        public string getPrenotazione_Posti(Int32 idEvento)
        {
            var db = new digiGappEntities();
            var item = db.B2RaiPlace_Eventi_Evento.Where(x => x.id == idEvento).FirstOrDefault();
            if (item == null) { return "nessuna evento presente"; }
            else { return "il numero di posti disponibili per l'evento è: " + item.numero_totale; }
        }

        public string deletePrenotazione(Int32 idPrenotazione)
        {
            var db = new digiGappEntities();
            var item = db.B2RaiPlace_Eventi_Prenotazione.Where(x => x.id == idPrenotazione).FirstOrDefault();
            if (item != null)
            {
                db.B2RaiPlace_Eventi_Prenotazione.Remove(item);
                db.SaveChanges();
                return "Cancellazione prenotazione avvenuta con successo";
            }
            else
            { return "Impossibile cancellare la prenotazione: non esiste"; }
        }

        public string putPrenotazione(Int32 idPrenotazione, [FromBody] listPrenotazione parametriPrenotazione)
        {
            var db = new digiGappEntities();
            var item = db.B2RaiPlace_Eventi_Prenotazione.Where(x => x.id == idPrenotazione).FirstOrDefault();
            if (item != null)
            {
                item.matricola = parametriPrenotazione.matricola;
                item.posti = parametriPrenotazione.posti;
                db.SaveChanges();
                return "Modifica prenotazione avvenuta con successo";
            }
            else
            {
                { return "Impossibile modificare la prenotazione: non esiste"; }
            }

        }

        public listPrenotazione postPrenotazione([FromBody] listPrenotazione parametriPrenotazione)
        {
            var db = new digiGappEntities();

            B2RaiPlace_Eventi_Prenotazione insert = new B2RaiPlace_Eventi_Prenotazione();
            listPrenotazione _msg = new listPrenotazione();

            if (parametriPrenotazione.matricola == 0) { _msg.message = "Inserire matricola"; return _msg; }
            if (parametriPrenotazione.idEvento == 0) { _msg.message = "Inserire id evento collegato alla prenotazione"; return _msg; }
            if (parametriPrenotazione.idProgramma == 0) { _msg.message = "Inserire id programma collegato all'evento della prenotazione"; return _msg; }
            if (parametriPrenotazione.posti == 0) { _msg.message = "Inserire numero di posti da prenotare maggiore di 0"; return _msg; }

            insert.matricola = parametriPrenotazione.matricola;
            insert.idEvento = parametriPrenotazione.idEvento;
            insert.idProgramma = parametriPrenotazione.idProgramma;
            insert.posti = parametriPrenotazione.posti;

            var evento = db.B2RaiPlace_Eventi_Evento.Where(x => x.id == parametriPrenotazione.idEvento && x.id_programma == parametriPrenotazione.idProgramma).FirstOrDefault();
            var programma = db.B2RaiPlace_Eventi_Programma.Where(x => x.id == parametriPrenotazione.idProgramma).FirstOrDefault();

            if (evento == null) { _msg.message = "Inserire un ID evento valido"; return _msg; }
            if (programma == null) { _msg.message = "Inserire un ID programma valido"; return _msg; }

            if (parametriPrenotazione.posti <= evento.numero_massimo)
            {
                db.B2RaiPlace_Eventi_Prenotazione.Add(insert);

                int? postiMax = evento.numero_totale;
                evento.numero_totale = Convert.ToInt32(postiMax - parametriPrenotazione.posti);

                db.SaveChanges();
            }
            else
            { _msg.message = "Numero posti prenotabili maggiore della dispinibilità dell'evento, massimo posti prenotabili: " + evento.numero_massimo; return _msg; }

            _msg.idProgramma = parametriPrenotazione.idProgramma;
            _msg.idEvento = parametriPrenotazione.idEvento;
            _msg.matricola = parametriPrenotazione.matricola;
            _msg.posti = parametriPrenotazione.posti;
            _msg.titoloEvento = evento.titolo;
            _msg.titoloProgramma = programma.titolo;
            _msg.message = "Inserimento prenotazione avvenuta con successo";
            return _msg;
        }

        #endregion

        public NotificheResponse getNotifiche(string m)
        {
            try
            {
                string homeUrl = Request.RequestUri.AbsoluteUri.ToLower().
                 Split(new string[] { "api" }, StringSplitOptions.None)[0];

                m = m.ToUpper().TrimStart('P');
                var notifiche = new myRaiData.digiGappEntities().MyRai_Notifiche
                                       .Where(x => x.matricola_destinatario == m && x.data_letta == null)
                                       .OrderByDescending(x => x.data_inserita).ToList();

                NotificheResponse resp = new NotificheResponse()
                {
                    success = true,
                    UrlNotificheTipo1 = Request.RequestUri.AbsoluteUri.ToLower().
                        Split(new string[] { "api" }, StringSplitOptions.None)[0] +
                        "notifiche?tipo=1",
                    UrlNotificheTipo2 = Request.RequestUri.AbsoluteUri.ToLower().
                        Split(new string[] { "api" }, StringSplitOptions.None)[0] +
                        "notifiche?tipo=2",
                    NotificheAPI = notifiche.Select(z => new NotificaAPI()
                    {
                        descrittiva = z.data_inserita != null ? z.data_inserita.ToString("dd MMMM yyyy") : "",
                        tipo = z.tipo != null ? z.tipo == 1 ? "1" : "2" : "2",
                        titolo = z.descrizione,

                        href = z.tipo == 1 ? Request.RequestUri.AbsoluteUri.ToLower().
                        Split(new string[] { "api" }, StringSplitOptions.None)[0] +
                        "notifiche?tipo=1" : Request.RequestUri.AbsoluteUri.ToLower().
                        Split(new string[] { "api" }, StringSplitOptions.None)[0] +
                        "notifiche?tipo=2",
                        id = z.id

                    }).ToList()
                };

                it.rai.servizi.comunica.Comunica hrComunica = new it.rai.servizi.comunica.Comunica();
                hrComunica.Credentials = System.Net.CredentialCache.DefaultCredentials;
                it.rai.servizi.comunica.ListaNotificheDocumenti result = hrComunica.ElencoNotificheDocumenti("41ZMWEJWSDJD16DPWX8S28ZZVTLOF56P", m, "WS_COMINT");
                if (result.Esito == 0)
                {
                    if (result.Notifiche.Split('|')[0] == "SI")
                    {
                        NotificaAPI item = new NotificaAPI();
                        item.titolo = "Hai cedolini non letti";
                        item.href = homeUrl+"BustaPaga/?notifiche=true";
                        item.tipo = "2";
                        item.id = 0;
                        item.descrittiva = "";
                        resp.NotificheAPI.Add(item);
                    }

                    if (result.Notifiche.Split('|')[1] == "SI")
                    {
                        NotificaAPI item = new NotificaAPI();
                        item.titolo = "Hai documenti amministrativi non letti";
                        item.href = homeUrl + "DocumentiAmministrativi/?notifiche=true";
                        item.tipo = "2";
                        item.id = 0;
                        item.descrittiva = "";
                        resp.NotificheAPI.Add(item);
                    }
                }

                var notificheCorsi = NotificheManager.GetNotificheCorsi(m, 0, true);
                foreach (var item in notificheCorsi)
                {
                    NotificaAPI notifica = new NotificaAPI();
                    notifica.tipo = item.notifica.tipo.Value.ToString();
                    notifica.id = 0;
                    notifica.titolo = item.notifica.descrizione;
                    notifica.href = item.Dettaglio.AnchorHref;
                    notifica.descrittiva = item.Dettaglio.Title;
                    resp.NotificheAPI.Add(notifica);
                }

                using (myRai.Data.CurriculumVitae.cv_ModelEntities dbCV = new Data.CurriculumVitae.cv_ModelEntities())
                {
                    myRai.Data.CurriculumVitae.TCVLogin login = dbCV.TCVLogin.FirstOrDefault(x => x.Matricola == m);
                    digiGappEntities db = new digiGappEntities();

                    //notifiche completamento CV
                    if (login != null)
                    {
                        int percCV = CommonManager.GetPercentualCV(m);
                        if (percCV == 100)
                        {
                            if (!db.MyRai_Notifiche.Any(x => x.matricola_destinatario == m && x.categoria == "CV Online"))
                            {
                                MyRai_Notifiche notifica100 = new MyRai_Notifiche();
                                notifica100.categoria = "CV Online";
                                notifica100.tipo = 2;
                                notifica100.matricola_destinatario = m;
                                notifica100.descrizione = "Complimenti per aver compilato il 100% del CV";
                                notifica100.descrizione += "\r\n" + CommonManager.GetParametro<string>(EnumParametriSistema.NotificaCV100);
                                notifica100.inserita_da = "Portale";
                                notifica100.data_inserita = DateTime.Now;
                                db.MyRai_Notifiche.Add(notifica100);
                                DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

                                //ha compilato almeno una parte del nuovo cv
                                NotificaAPI notificaCV = new NotificaAPI();
                                notificaCV.titolo = notifica100.categoria;
                                notificaCV.tipo = "2";
                                notificaCV.id = 0;
                                notificaCV.href = Request.RequestUri.AbsoluteUri.ToLower().Split(new string[] { "api" }, StringSplitOptions.None)[0] + "notifiche?tipo=1";
                                notificaCV.descrittiva = notifica100.descrizione;
                                resp.NotificheAPI.Add(notificaCV);
                            }
                        }
                        else
                        {
                            //se presente la notifica del 100 la tolgo
                            MyRai_Notifiche notifica100 = db.MyRai_Notifiche.FirstOrDefault(x => x.matricola_destinatario == m && x.categoria == "CV Online");
                            if (notifica100 != null)
                            {
                                NotificaAPI noti100 = resp.NotificheAPI.FirstOrDefault(x => x.titolo == "CV Online");
                                resp.NotificheAPI.Remove(noti100);
                                db.MyRai_Notifiche.Remove(notifica100);
                                DBHelper.Save(db , CommonHelper.GetCurrentUserMatricola( ) );
                            }

                            //ha compilato almeno una parte del nuovo cv
                            NotificaAPI notificaCV = new NotificaAPI();
                            notificaCV.titolo = "CV Online";
                            notificaCV.tipo = "2";
                            notificaCV.id = 0;
                            notificaCV.href = Request.RequestUri.AbsoluteUri.ToLower().Split(new string[] { "api" }, StringSplitOptions.None)[0] + "notifiche?tipo=1";
                            notificaCV.titolo = "Completa la compilazione del CV";
                            notificaCV.descrittiva = CommonManager.GetParametro<string>(EnumParametriSistema.NotificaCVLess100);
                            resp.NotificheAPI.Add(notificaCV);
                        }
                    }
                    else
                    {
                        //se presente la notifica del 100 la tolgo
                        MyRai_Notifiche notifica100 = db.MyRai_Notifiche.FirstOrDefault(x => x.matricola_destinatario == m && x.categoria == "CV Online");
                        if (notifica100 != null)
                        {
                            NotificaAPI noti100 = resp.NotificheAPI.FirstOrDefault(x => x.titolo == "CV Online");
                            resp.NotificheAPI.Remove(noti100);   
                            db.MyRai_Notifiche.Remove(notifica100);
                            DBHelper.Save(db , CommonHelper.GetCurrentUserMatricola( ) );
                        }

                        //Non ha mai compilato il CV
                        NotificaAPI notificaCV = new NotificaAPI();
                        notificaCV.titolo = "CV Online";
                        notificaCV.tipo = "2";
                        notificaCV.id = 0;
                        notificaCV.href = Request.RequestUri.AbsoluteUri.ToLower().Split(new string[] { "api" }, StringSplitOptions.None)[0] + "cv_online";
                        notificaCV.titolo = "Compila il nuovo CV";
                        notificaCV.descrittiva = CommonManager.GetParametro<string>(EnumParametriSistema.NotificaCVZero);
                        resp.NotificheAPI.Add(notificaCV);
                    }
                }

                resp.NotificheAPI = resp.NotificheAPI.OrderBy(a => a.id).ToList();

                if (resp.NotificheAPI.Any(x => x.titolo == "CV Online" && x.id>0))
                {
                    int index = resp.NotificheAPI.FindIndex(x => x.id > 0);
                    if (index >= 0)
                    {
                        NotificaAPI c = resp.NotificheAPI.FirstOrDefault(x => x.titolo == "CV Online");
                        resp.NotificheAPI.Remove(c);
                        resp.NotificheAPI.Insert(index, c);
                    }
                }

                return resp;
            }
            catch (Exception ex)
            {
                return new NotificheResponse() { error = ex.Message, success = false };
            }
        }

        public InfoResponse getInfoMatr(string m)
        {
            InfoResponse IR = new InfoResponse();
            IR.formatoVersione = "2";
            try
            {
                m = m.ToUpper().TrimStart('P');

                IR.cvResponse = getCV(m);
                IR.ferieResponse = getFeriePermessi(m);

                Dictionary<string, string> Labels = new Dictionary<string, string>();
                Labels.Add("FE", "Ferie residue");
                Labels.Add("PR", "Permessi retribuiti residui");
                Labels.Add("PF", "Permessi ex festività residui");
                Labels.Add("PX", "Permessi giornalisti residui");

                IR.ferieData = new FerieResponseNewWrapper()
                {
                    error = IR.ferieResponse.error,
                    success = IR.ferieResponse.success,
                    url = IR.ferieResponse.url,
                    title = "Ferie"
                };
                List<FerieResponseNew> LF = new List<FerieResponseNew>();
                foreach (var item in IR.ferieResponse.eccezioni)
                {
                    FerieResponseNew fn = new FerieResponseNew();
                    fn.codice = item.codice;
                    fn.label = Labels.Where(x => x.Key == item.codice).Select(x => x.Value).FirstOrDefault();
                    fn.qty = item.Residue;
                    List<segmento> LS = new List<segmento>();
                    LS.Add(new segmento() { label = "Anni precedenti", qty = item.AnniPrec });
                    LS.Add(new segmento() { label = "Fruit" + (item.codice == "FE" ? "e" : "i"), qty = item.Fruite });
                    LS.Add(new segmento() { label = "Spettanti", qty = item.Spettanti });
                    fn.segmenti = LS.ToArray();

                    LF.Add(fn);
                }
                IR.ferieData.blocchi = LF.ToArray();

                it.rai.intranet.hrpaganew.Comunica serviceRequest = new it.rai.intranet.hrpaganew.Comunica();

                serviceRequest.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

                var serviceResponse = serviceRequest.ElencoUltimaBusta("41ZMWEJWSDJD16DPWX8S28ZZVTLOF56P",
                                                        m,
                                                        "WS_COMINT");

                if (serviceResponse.Esito == 0)
                {
                    string data = serviceResponse.Notifiche;

                    int anno = int.Parse(data.Substring(0, 4));
                    int mese = int.Parse(data.Substring(4, 2));

                    DateTime t = new DateTime(anno, mese, 1);

                    string upper = t.ToString("MMMM").Substring(0, 1).ToUpper();

                    string sMese = upper + t.ToString("MMMM").Substring(1, t.ToString("MMMM").Length - 1);

                    IR.cedResponse = new CedolinoResponse()
                    {
                        DataCompetenza = data,
                        DataCompetenza_Desc = String.Format("{0} {1}", sMese, t.Year.ToString()),
                        DataContabilizzazione = data,
                        DataContabilizzazione_Desc = String.Format("{0} {1}", sMese, t.Year.ToString()),
                        success = true,
                        title = "Busta paga",
                        url = Request.RequestUri.AbsoluteUri.ToLower().
                                   Split(new string[] { "api" }, StringSplitOptions.None)[0] +
                                    "bustapaga"
                    };
                    IR.cedResponse.text = "E' disponibile la tua busta paga di <b>" + IR.cedResponse.DataCompetenza_Desc + "</b>";
                }
                else
                {
                    throw new Exception(serviceResponse.StringaErrore);
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    provenienza = "API getInfoMatr " + m,
                    error_message = ex.ToString()
                });

                IR.cedResponse = new CedolinoResponse()
                {
                    DataCompetenza = "--------",
                    DataCompetenza_Desc = "--------",
                    DataContabilizzazione = "--------",
                    DataContabilizzazione_Desc = "--------",
                    success = false,
                    url = Request.RequestUri.AbsoluteUri.ToLower().
                               Split(new string[] { "api" }, StringSplitOptions.None)[0] +
                                "bustapaga"
                };
            }

            return IR;
        }

        public EventoResponse getDispoEv(int idev)
        {
            var db = new digiGappEntities();

            try
            {
                B2RaiPlace_Eventi_Evento evento = db.B2RaiPlace_Eventi_Evento
                                                   .Where(x => x.id == idev).FirstOrDefault();

                if (evento == null)
                    return new EventoResponse() { disponibili = 0, error = "Evento non esistente", success = false };

                int disp = (int)evento.numero_totale - evento.B2RaiPlace_Eventi_Anagrafica.Count();
                return new EventoResponse() { disponibili = disp, success = true };
            }
            catch (Exception ex)
            {
                return new EventoResponse() { error = ex.Message, success = false };
            }
        }

        public FerieResponse getFeriePermessi(string matricola)
        {
            try
            {
                pianoFerie response = FeriePermessiManager.GetPianoFerieAnno(DateTime.Now.Year, false, matricola,true);
                FerieDipendente datiPR = FeriePermessiManager.GetFeriePermessiGeneric(response,
                    EnumPrefixFeriePermessi.permessi,
                    EnumCodiceVisualizzazioneFeriePermessi.PR);

                FerieDipendente datiPF = FeriePermessiManager.GetFeriePermessiGeneric(response,
                    EnumPrefixFeriePermessi.exFestivita,
                  EnumCodiceVisualizzazioneFeriePermessi.PF);

                FerieDipendente datiFE = FeriePermessiManager.GetFeriePermessiGeneric(response,
                    EnumPrefixFeriePermessi.ferie,
                  EnumCodiceVisualizzazioneFeriePermessi.FE);

                FerieDipendente datiPX = FeriePermessiManager.GetFeriePermessiGeneric(response,
                    EnumPrefixFeriePermessi.permessiGiornalisti,
                        EnumCodiceVisualizzazioneFeriePermessi.PX);

                FerieResponse f = new FerieResponse()
                {
                    error = null,
                    success = true,
                    url = Request.RequestUri.AbsoluteUri.ToLower().
                             Split(new string[] { "api" }, StringSplitOptions.None)[0] +
                              "feriepermessi"
                };
                f.eccezioni = new List<Ecc>();

                var db = new myRaiData.digiGappEntities();
                var listaEcc = db.L2D_ECCEZIONE.ToList();

                if (datiFE.Spettanti > 0)
                    f.eccezioni.Add(new Ecc()
                    {
                        codice = "FE",
                        AnniPrec = (float)Math.Round(datiFE.AnnoPrec, 2),
                        Fruite = (float)Math.Round(datiFE.Usufruite, 2),
                        Residue = (float)Math.Round(datiFE.Residue, 2),
                        Spettanti = (float)Math.Round(datiFE.Spettanti, 2),
                        Descrittiva = listaEcc.Where(x => x.cod_eccezione.Trim().ToUpper() == "FE")
                            .Select(x => x.desc_eccezione).FirstOrDefault()
                    });

                if (datiPR.Spettanti > 0)
                    f.eccezioni.Add(new Ecc()
                    {
                        codice = "PR",
                        AnniPrec = (float)Math.Round(datiPR.AnnoPrec, 2),
                        Fruite = (float)Math.Round(datiPR.Usufruite, 2),
                        Residue = (float)Math.Round(datiPR.Residue, 2),
                        Spettanti = (float)Math.Round(datiPR.Spettanti, 2),
                        Descrittiva = listaEcc.Where(x => x.cod_eccezione.Trim().ToUpper() == "PR")
                            .Select(x => x.desc_eccezione).FirstOrDefault()
                    });
                if (datiPF.Spettanti > 0)
                    f.eccezioni.Add(new Ecc()
                    {
                        codice = "PF",
                        AnniPrec = (float)Math.Round(datiPF.AnnoPrec, 2),
                        Fruite = (float)Math.Round(datiPF.Usufruite, 2),
                        Residue = (float)Math.Round(datiPF.Residue, 2),
                        Spettanti = (float)Math.Round(datiPF.Spettanti, 2),
                        Descrittiva = listaEcc.Where(x => x.cod_eccezione.Trim().ToUpper() == "PF")
                            .Select(x => x.desc_eccezione).FirstOrDefault()
                    });

                if (datiPX.Spettanti > 0)
                    f.eccezioni.Add(new Ecc()
                    {
                        codice = "PX",
                        AnniPrec = (float)Math.Round(datiPX.AnnoPrec, 2),
                        Fruite = (float)Math.Round(datiPX.Usufruite, 2),
                        Residue = (float)Math.Round(datiPX.Residue, 2),
                        Spettanti = (float)Math.Round(datiPX.Spettanti, 2),
                        Descrittiva = listaEcc.Where(x => x.cod_eccezione.Trim().ToUpper() == "PX")
                         .Select(x => x.desc_eccezione).FirstOrDefault()
                    });
                return f;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "API",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    provenienza = "getFeriePermessi"

                });
                return new FerieResponse() { error = ex.Message, success = false };
            }
        }

        public CVresponse getCV(string matricola)
        {
            try
            {
                int perc = CommonManager.GetPercentualCV(matricola);
                return new CVresponse()
                {
                    CVperc = perc.ToString(),
                    CvpercDiff = (100 - perc).ToString(),
                    error = null,
                    success = true,
                    text = "Completa il " + (100 - perc).ToString() + "% che ti manca",
                    title = "Curriculum Vitae",
                    url = Request.RequestUri.AbsoluteUri.ToLower().
                    Split(new string[] { "api" }, StringSplitOptions.None)[0] +
                     "cv_online"
                };
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "API",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    provenienza = "getCV"

                });
                return new CVresponse() { CVperc = "", error = ex.Message, success = false };
            }
        }

        public BustaPagaRedirectResponse putBustaPagaRedirect(int? dayOfMonth)
        {
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    MyRai_ParametriSistema param = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "GiornoCedolino");
                    if (param == null)
                    {
                        param = new MyRai_ParametriSistema()
                        {
                            Chiave = "GiornoCedolino",
                            Valore1 = dayOfMonth.ToString(),
                            Valore2 = null
                        };
                    }
                    else
                        param.Valore1 = dayOfMonth.ToString();

                    db.SaveChanges();
                }

                return new BustaPagaRedirectResponse()
                {
                    error = null,
                    success = true
                };
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "API",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    provenienza = "setBustaPagaRedirect"

                });
                return new BustaPagaRedirectResponse()
                {
                    error = ex.ToString(),
                    success = false
                };
            }
        }
        public BustaPagaRedirectResponse getBustaPagaRedirect()
        {
            try
            {
                bool flagSuccess = false;
                string errorMsg = null;
                int? giornoRedirect = null;

                using (digiGappEntities db = new digiGappEntities())
                {
                    MyRai_ParametriSistema param = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "GiornoCedolino");
                    if (param == null)
                    {
                        flagSuccess = false;
                        errorMsg = "Parametro non trovato";
                    }
                    else
                    {
                        flagSuccess = true;
                        giornoRedirect = Convert.ToInt32(param.Valore1);
                    }    
                }

                return new BustaPagaRedirectResponse()
                {
                    error = errorMsg,
                    success = flagSuccess,
                    giornoRedirect = giornoRedirect
                };
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "API",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    provenienza = "getBustaPagaRedirect"

                });
                return new BustaPagaRedirectResponse()
                {
                    error = ex.ToString(),
                    success = false
                };
            }
        }

        public BustaPagaRedirectResponse putDocAmministrativoRedirect(int? dayOfMonth)
        {
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    MyRai_ParametriSistema param = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "GiornoDocAmministrativi");
                    if (param == null)
                    {
                        param = new MyRai_ParametriSistema()
                        {
                            Chiave = "GiornoDocAmministrativi",
                            Valore1 = dayOfMonth.ToString(),
                            Valore2 = null
                        };
                    }
                    else
                        param.Valore1 = dayOfMonth.ToString();

                    db.SaveChanges();
                }

                return new BustaPagaRedirectResponse()
                {
                    error = null,
                    success = true
                };
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "API",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    provenienza = "setDocAmministrativiRedirect"

                });
                return new BustaPagaRedirectResponse()
                {
                    error = ex.ToString(),
                    success = false
                };
            }
        }
        public BustaPagaRedirectResponse getDocAmministrativoRedirect()
        {
            try
            {
                bool flagSuccess = false;
                string errorMsg = null;
                int? giornoRedirect = null;

                using (digiGappEntities db = new digiGappEntities())
                {
                    MyRai_ParametriSistema param = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "GiornoDocAmministrativi");
                    if (param == null)
                    {
                        flagSuccess = false;
                        errorMsg = "Parametro non trovato";
                    }
                    else
                    {
                        flagSuccess = true;
                        giornoRedirect = Convert.ToInt32(param.Valore1);
                    }
                }

                return new BustaPagaRedirectResponse()
                {
                    error = errorMsg,
                    success = flagSuccess,
                    giornoRedirect = giornoRedirect
                };
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "API",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    provenienza = "getDocAmministrativiRedirect"

                });
                return new BustaPagaRedirectResponse()
                {
                    error = ex.ToString(),
                    success = false
                };
            }
        }
    }

    public class BustaPagaRedirectResponse
    {
        public bool success { get; set; }
        public string error { get; set; }
        public int? giornoRedirect { get; set; }
    }

    public class EventoResponse
    {
        public int disponibili { get; set; }
        public bool success { get; set; }
        public string error { get; set; }
    }

    public class FerieResponseNewWrapper
    {
        public bool success { get; set; }
        public string error { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public FerieResponseNew[] blocchi { get; set; }
    }

    public class FerieResponseNew
    {
        public string codice { get; set; }
        public string label { get; set; }
        public float qty { get; set; }

        public segmento[] segmenti { get; set; }
    }

    public class segmento
    {
        public float qty { get; set; }
        public string label { get; set; }
    }

    public class FerieResponse
    {
        public bool success { get; set; }
        public string error { get; set; }
        public List<Ecc> eccezioni { get; set; }
        public string url { get; set; }
    }
    public class Ecc
    {
        public string codice { get; set; }
        public float Spettanti { get; set; }
        public float Fruite { get; set; }
        public float Residue { get; set; }
        public float AnniPrec { get; set; }
        public string Descrittiva { get; set; }
    }

    public class CVresponse
    {
        public bool success { get; set; }
        public string error { get; set; }
        public string CVperc { get; set; }
        public string CvpercDiff { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public string url { get; set; }
    }
    public class CedolinoResponse
    {
        public string DataCompetenza { get; set; }
        public string DataCompetenza_Desc { get; set; }
        public string DataContabilizzazione { get; set; }
        public string DataContabilizzazione_Desc { get; set; }
        public bool success { get; set; }
        public string error { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string text { get; set; }
    }

    public class Evento
    {
        public Boolean esito { get; set; }
        public string message { get; set; }
        public ListEvento resultListaEvento { get; set; }
    }

    public class Prenotazione
    {
        public Boolean esito { get; set; }
        public string message { get; set; }
        public List<listPrenotazione> result { get; set; }
    }

    public class InfoResponse
    {
        public string formatoVersione { get; set; }
        public CVresponse cvResponse { get; set; }
        public FerieResponse ferieResponse { get; set; }
        public FerieResponseNewWrapper ferieData { get; set; }
        public CedolinoResponse cedResponse { get; set; }
    }
    public class NotificheResponse
    {
        public NotificheResponse()
        {
            NotificheAPI = new List<NotificaAPI>();
        }
        public bool success { get; set; }
        public string error { get; set; }
        public string UrlNotificheTipo1 { get; set; }
        public string UrlNotificheTipo2 { get; set; }
        public List<NotificaAPI> NotificheAPI { get; set; }
    }

    public class NotificaAPI
    {
        public string tipo { get; set; }
        public string titolo { get; set; }
        public string descrittiva { get; set; }
        public string href { get; set; }
        public int id;
    }

    public class listPrenotazione
    {
        public Int32 idEvento { get; set; }
        public Int32 idProgramma { get; set; }
        public Int32 matricola { get; set; }
        public Int32 posti { get; set; }
        public string titoloEvento { get; set; }
        public string titoloProgramma { get; set; }
        public string message { get; set; }
    }

    public class ListEvento
    {
        public string titolo { get; set; }
        public string luogo { get; set; }
        public string programma { get; set; }
        public DateTime? dataInizio { get; set; }
        public DateTime? dataFine { get; set; }
        public DateTime? dataInizioPrenotazione { get; set; }
        public DateTime? dataFinePrenotazione { get; set; }
        public Int32? numeroMassimo { get; set; }
        public Int32? numeroTotale { get; set; }
        public Int32 idProgramma { get; set; }
        public Int32 idEvento { get; set; }
        public List<matricole> listaMatricoleAbilitate { get; set; }
        public List<sedi> listaSediAbilitate { get; set; }
        public string message { get; set; }
    }

    public class matricole
    {
        public string matricola { get; set; }
        public Int32? livello { get; set; }
    }

    public class sedi
    {
        public String sedeGapp { get; set; }
    }
}