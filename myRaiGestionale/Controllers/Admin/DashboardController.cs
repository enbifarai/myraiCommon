using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public class HrisLogFilter
    {
        public string Operazione { get; set; }
        public string Esito { get; set; }
        public string Matricola { get; set; }
        public string Parametri { get; set; }
        public string Errore { get; set; }
        public string NotaEsito { get; set; }
        public string DataDa { get; set; }
        public string DataA { get; set; }
    }
    public class HrisSegnFilter
    {
        public string Ambito { get; set; }
        public string Tipologia { get; set; }
        public string MatricolaIns { get; set; }
        public string Testo { get; set; }
        public string MatrIncarico { get; set; }
        public string MatrEsito { get; set; }
        public string Esito { get; set; }
        public string NotaEsito { get; set; }
        public string DataDa { get; set; }
        public string DataA { get; set; }
    }
    public class HrisTaskFilter
    {
        public string Tipologia { get; set; }
        public string Sottotipologia { get; set; }
        public string MatricolaCreatore { get; set; }
        public string Esito { get; set; }
        public string DataDa { get; set; }
        public string DataA { get; set; }
        public bool EscludiSchedulati { get; set; }
    }

    public class HrisScheduleFilter
    {
        public string Tipologia { get; set; }
        public string Sottotipologia { get; set; }
        public string Nome { get; set; }
    }

    public class DashboardController : BaseCommonController
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Elenco_Log(HrisLogFilter filter = null)
        {
            filter = filter ?? new HrisLogFilter();

            IncentiviEntities db = new IncentiviEntities();
            IQueryable<XR_HRIS_LOG> query = db.XR_HRIS_LOG.AsQueryable();

            if (!String.IsNullOrWhiteSpace(filter.Operazione))
                query = query.Where(x => x.DES_OPERAZIONE == filter.Operazione);

            if (!String.IsNullOrWhiteSpace(filter.Esito))
            {
                bool indEsito = filter.Esito.ToUpper() == "TRUE";
                query = query.Where(x => x.IND_ESITO == indEsito);
            }

            if (!String.IsNullOrWhiteSpace(filter.Matricola))
                query = query.Where(x => filter.Matricola.Contains(x.COD_MATRICOLA));

            if (!String.IsNullOrWhiteSpace(filter.Parametri))
                query = query.Where(x => x.NOT_PARAMETRI.Contains(filter.Parametri));

            if (!String.IsNullOrWhiteSpace(filter.Errore))
                query = query.Where(x => x.NOT_ERRORE.Contains(filter.Errore));

            if (!String.IsNullOrWhiteSpace(filter.NotaEsito))
                query = query.Where(x => x.NOT_ESITO.Contains(filter.NotaEsito));

            if (!String.IsNullOrWhiteSpace(filter.DataDa))
            {
                DateTime dtDataDa = filter.DataDa.ToDateTime("dd/MM/yyyy");
                query = query.Where(x => x.TMS_TIMESTAMP != null && x.TMS_TIMESTAMP >= dtDataDa);
            }

            if (!String.IsNullOrWhiteSpace(filter.DataA))
            {
                DateTime dtDataA = filter.DataA.ToDateTime("dd/MM/yyyy");
                query = query.Where(x => x.TMS_TIMESTAMP != null && x.TMS_TIMESTAMP <= dtDataA);
            }

            List<XR_HRIS_LOG> model = new List<XR_HRIS_LOG>();
            model.AddRange(query);

            return View(model);
        }

        [HttpPost]
        public ActionResult Detail_Log(int idLog)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_HRIS_LOG log = db.XR_HRIS_LOG.Find(idLog);
            return View("Detail_Log", log);
        }

        [HttpPost]
        public ActionResult Elenco_Segn(HrisSegnFilter filter = null)
        {
            filter = filter ?? new HrisSegnFilter();

            IncentiviEntities db = new IncentiviEntities();
            IQueryable<XR_HRIS_SEGNALAZIONE> query = db.XR_HRIS_SEGNALAZIONE.AsQueryable();

            if (!String.IsNullOrWhiteSpace(filter.Ambito))
                query = query.Where(x => x.DES_AMBITO == filter.Ambito);

            if (!String.IsNullOrWhiteSpace(filter.Esito))
            {
                int.TryParse(filter.Esito, out int esito);
                query = query.Where(x => x.IND_ESITO!=null && x.IND_ESITO.Value == esito);
            }

            if (!String.IsNullOrWhiteSpace(filter.MatricolaIns))
                query = query.Where(x => filter.MatricolaIns.Contains(x.MATR_INSERIMENTO));

            if (!String.IsNullOrWhiteSpace(filter.MatrIncarico))
                query = query.Where(x => filter.MatrIncarico.Contains(x.MATR_INCARICO));

            if (!String.IsNullOrWhiteSpace(filter.MatrEsito))
                query = query.Where(x => filter.MatrEsito.Contains(x.MATR_ESITO));

            if (!String.IsNullOrWhiteSpace(filter.NotaEsito))
                query = query.Where(x => x.NOT_ESITO.Contains(filter.NotaEsito));

            if (!String.IsNullOrWhiteSpace(filter.DataDa))
            {
                DateTime dtDataDa = filter.DataDa.ToDateTime("dd/MM/yyyy");
                query = query.Where(x => x.DTA_INSERIMENTO >= dtDataDa);
            }

            if (!String.IsNullOrWhiteSpace(filter.DataA))
            {
                DateTime dtDataA = filter.DataA.ToDateTime("dd/MM/yyyy");
                query = query.Where(x => x.DTA_INSERIMENTO <= dtDataA);
            }

            List<XR_HRIS_SEGNALAZIONE> model = new List<XR_HRIS_SEGNALAZIONE>();
            model.AddRange(query);

            return View(model);
        }

        [HttpPost]
        public ActionResult Detail_Segn(int idSegn)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_HRIS_SEGNALAZIONE segn = db.XR_HRIS_SEGNALAZIONE.Find(idSegn);
            return View("Detail_Segn", segn);
        }

        [HttpPost]
        public ActionResult Esito_Segn(int idSegn, int indEsito, string nota)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_HRIS_SEGNALAZIONE segn = db.XR_HRIS_SEGNALAZIONE.Find(idSegn);
            segn.IND_ESITO = indEsito;
            segn.NOT_ESITO = nota;
            segn.MATR_ESITO = CommonHelper.GetCurrentUserMatricola();
            segn.DTA_ESITO = DateTime.Now;

            db.SaveChanges();

            return View("Detail_Segn", segn);
        }

        [HttpPost]
        public ActionResult Elenco_Task(HrisTaskFilter filter = null)
        {
            filter = filter ?? new HrisTaskFilter();

            IncentiviEntities db = new IncentiviEntities();
            IQueryable<XR_TSK_TASK> query = db.XR_TSK_TASK.AsQueryable();

            if (!String.IsNullOrWhiteSpace(filter.Tipologia))
                query = query.Where(x => x.COD_TIPOLOGIA == filter.Tipologia);

            if (!String.IsNullOrWhiteSpace(filter.Sottotipologia))
                query = query.Where(x => x.COD_SOTTOTIPOLOGIA == filter.Sottotipologia);

            if (!String.IsNullOrWhiteSpace(filter.Esito))
            {
                bool indEsito = filter.Esito.ToUpper() == "TRUE";
                query = query.Where(x => x.IND_ESITO == indEsito);
            }

            if (!String.IsNullOrWhiteSpace(filter.MatricolaCreatore))
                query = query.Where(x => filter.MatricolaCreatore.Contains(x.MATRICOLA_CREATORE));

            if (!String.IsNullOrWhiteSpace(filter.DataDa))
            {
                DateTime dtDataDa = filter.DataDa.ToDateTime("dd/MM/yyyy");
                query = query.Where(x => x.DATA_CREAZIONE != null && x.DATA_CREAZIONE >= dtDataDa);
            }

            if (!String.IsNullOrWhiteSpace(filter.DataA))
            {
                DateTime dtDataA = filter.DataA.ToDateTime("dd/MM/yyyy");
                query = query.Where(x => x.DATA_CREAZIONE != null && x.DATA_CREAZIONE <= dtDataA);
            }

            if (filter.EscludiSchedulati)
                query = query.Where(x => x.SCHEDULE_PARENT == null);

            List<XR_TSK_TASK> model = new List<XR_TSK_TASK>();
            model.AddRange(query);

            return View(model);
        }

        [HttpPost]
        public ActionResult Detail_Task(int idTask,  string tipologia, string sottoTipologia=null)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_TSK_TASK task = null;
            if (idTask == 0)
            {
                string tmpSotto = String.IsNullOrWhiteSpace(sottoTipologia) ? null : sottoTipologia;
                XR_TSK_TIPOLOGIE defaultTipo = db.XR_TSK_TIPOLOGIE.ToList().FirstOrDefault(x => x.COD_TIPOLOGIA == tipologia && x.COD_SOTTOTIPOLOGIA == tmpSotto);
                task = new XR_TSK_TASK()
                {
                    COD_TIPOLOGIA = tipologia,
                    COD_SOTTOTIPOLOGIA = sottoTipologia,
                    INPUT = defaultTipo.DEFAULT_INPUT,
                    IND_RIESEGUI = defaultTipo.IND_RIESEGUI.GetValueOrDefault()
                };
            }
            else
                task = db.XR_TSK_TASK.Find(idTask);
            return View("Detail_Task", task);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save_Task(XR_TSK_TASK model)
        {
            IncentiviEntities db = new IncentiviEntities();
            model.DATA_CREAZIONE = DateTime.Now;
            model.MATRICOLA_CREATORE = CommonHelper.GetCurrentUserMatricola();
            db.XR_TSK_TASK.Add(model);
            db.SaveChanges();

            return Content("OK");
        }

        [HttpPost]
        public ActionResult Elenco_Schedule(HrisScheduleFilter filter = null)
        {
            filter = filter ?? new HrisScheduleFilter();

            IncentiviEntities db = new IncentiviEntities();
            IQueryable<XR_TSK_SCHEDULER> query = db.XR_TSK_SCHEDULER.AsQueryable();

            if (!String.IsNullOrWhiteSpace(filter.Tipologia))
                query = query.Where(x => x.COD_TIPOLOGIA == filter.Tipologia);

            if (!String.IsNullOrWhiteSpace(filter.Sottotipologia))
                query = query.Where(x => x.COD_SOTTOTIPOLOGIA == filter.Sottotipologia);

            if (!String.IsNullOrWhiteSpace(filter.Nome))
                query = query.Where(x => x.COD_NAME.ToUpper().Contains(filter.Nome.ToUpper()));
            
            List<XR_TSK_SCHEDULER> model = new List<XR_TSK_SCHEDULER>();
            model.AddRange(query.OrderBy(x=>x.COD_NAME));

            return View(model);
        }

        [HttpPost]
        public ActionResult Detail_Schedule(int idSchedule, string tipologia, string sottoTipologia = null)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_TSK_SCHEDULER schedule = null;
            if (idSchedule == 0)
            {
                string tmpSotto = String.IsNullOrWhiteSpace(sottoTipologia) ? null : sottoTipologia;
                XR_TSK_TIPOLOGIE defaultTipo = db.XR_TSK_TIPOLOGIE.ToList().FirstOrDefault(x => x.COD_TIPOLOGIA == tipologia && x.COD_SOTTOTIPOLOGIA == tmpSotto);
                schedule = new XR_TSK_SCHEDULER()
                {
                    COD_TIPOLOGIA = tipologia,
                    COD_SOTTOTIPOLOGIA = sottoTipologia,
                    INPUT = defaultTipo.DEFAULT_INPUT,
                    IND_ACTIVE = true
                };
            }
            else
                schedule = db.XR_TSK_SCHEDULER.Find(idSchedule);
            return View("Detail_Schedule", schedule);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save_Schedule(XR_TSK_SCHEDULER model)
        {
            IncentiviEntities db = new IncentiviEntities();
            if (model.ID==0)
                db.XR_TSK_SCHEDULER.Add(model);
            else
            {
                var rec = db.XR_TSK_SCHEDULER.Find(model.ID);
                rec.COD_NAME = model.COD_NAME;
                rec.COD_SOTTOTIPOLOGIA = model.COD_SOTTOTIPOLOGIA;
                rec.COD_TIPOLOGIA = model.COD_TIPOLOGIA;
                rec.DAILY_RECURRENCE = model.DAILY_RECURRENCE;
                rec.DTA_EXPIRE = model.DTA_EXPIRE;
                rec.DTA_START = model.DTA_START;
                rec.IND_ACTIVE = model.IND_ACTIVE;
                rec.INPUT = model.INPUT;
                rec.MONTHLY_DAYS = model.MONTHLY_DAYS;
                rec.MONTHLY_MONTHS = model.MONTHLY_MONTHS;
                rec.MONTHLY_WEEKDAYS = model.MONTHLY_WEEKDAYS;
                rec.REPEAT_DURATION = model.REPEAT_DURATION;
                rec.REPEAT_RECURRENCE = model.REPEAT_RECURRENCE;
                rec.SCHEDULE_TYPE = model.SCHEDULE_TYPE;
                rec.WEEKLY_RECURRENCE = model.WEEKLY_RECURRENCE;
                rec.WEEKLY_WEEKDAYS = model.WEEKLY_WEEKDAYS;
            }
            db.SaveChanges();

            return Content("OK");
        }


        public ActionResult Test()
        {
            var dbCV = new myRaiData.PERSEOEntities();
            var dbCzn = new myRaiData.Incentivi.IncentiviEntities();

            var listOld = dbCV.DServizio.ToList();
            List<myRaiData.Incentivi.XR_TB_SERVIZIO_EXT> newList = new List<myRaiData.Incentivi.XR_TB_SERVIZIO_EXT>();

            foreach (var item in listOld)
            {
                myRaiData.Incentivi.XR_TB_SERVIZIO_EXT newServ = new myRaiData.Incentivi.XR_TB_SERVIZIO_EXT()
                {
                    COD_SERVIZIO = item.Codice,
                    DES_ESTESA = item.DescrLunga,
                    DTA_INIZIO = DateTime.ParseExact(item.Data_scadenza, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None),
                    DTA_FINE = DateTime.ParseExact(item.Data_scadenza, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None),
                    COD_USER = "ADMIN",
                    COD_TERMID  = "BATCHSESSION",
                    TMS_TIMESTAMP = DateTime.Now
                };
                newList.Add(newServ);
            }

            newList = newList.OrderBy(x => x.COD_SERVIZIO).ThenBy(x => x.DTA_INIZIO).ToList();

            foreach (var item in newList.GroupBy(x=>x.COD_SERVIZIO))
            {
                item.First().DTA_INIZIO = new DateTime(1900, 1, 1);
                if (item.Count()>1)
                {
                    for (int i = 1; i < item.Count(); i++)
                    {
                        item.ElementAt(i).DTA_INIZIO = item.ElementAt(i - 1).DTA_FINE.AddDays(1);
                    }
                }
            }

            foreach (var item in newList)
                dbCzn.XR_TB_SERVIZIO_EXT.Add(item);

            dbCzn.SaveChanges();

            return Content("OK");
        }
    }
}
