using myRai.DataAccess;
using myRaiCommonTasks;
using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace myRaiHelper.Task
{
    public class TaskLogMail
    {
        public bool Enabled { get; set; }
        public string Mittente { get; set; }
        public string Destinatari { get; set; }
        public string DestinatariCC { get; set; }
        public string DestinatariCCN { get; set; }
        public string Oggetto { get; set; }
    }

    public class BaseTask
    {
        public string Note { get; set; }
        public TaskLogMail Mail { get; set; }
        public bool Impersonate { get; set; }
        public HrisParam? ImpersonateHrisParam { get; set; }
        public bool RunAsync { get; set; }

        public virtual void SetStart(XR_TSK_TASK input)
        {
            using (var db = new IncentiviEntities())
            {
                var task = db.XR_TSK_TASK.Find(input.ID);
                input.DATA_ESECUZIONE = task.DATA_ESECUZIONE = DateTime.Now;
                input.IND_RUNNING = task.IND_RUNNING = true;
                DBHelper.Save(db, "EsecutoreTask");
            }
        }
        public virtual void SetEnd(XR_TSK_TASK input)
        {
            using (var db = new IncentiviEntities())
            {
                var task = db.XR_TSK_TASK.Find(input.ID);
                input.DATA_ESECUZIONE_FINE = task.DATA_ESECUZIONE_FINE = DateTime.Now;
                input.IND_RUNNING = task.IND_RUNNING = false;
                DBHelper.Save(db, "EsecutoreTask");
            }
        }

        public virtual void ManageMail(XR_TSK_TASK task)
        {
            if (Mail != null && Mail.Enabled)
            {
                GestoreMail mail = new GestoreMail();
                string testo = String.Format(@"Esecuzione task {0}<br/>
                                                    Data/ora esecuzione: {1:dd/MM/yyyy HH:mm}<br/>
                                                    Data/ora fine esecuzione: {2:dd/MM/yyyy HH:mm}<br />
                                                    Esito: {3}<br/>
                                                    Errore: {4}<br/>
                                                    Output: {5}
                                                    ",
                                            task.COD_TIPOLOGIA + (!String.IsNullOrWhiteSpace(task.COD_SOTTOTIPOLOGIA) ? "/" + task.COD_SOTTOTIPOLOGIA : ""),
                                            task.DATA_ESECUZIONE,
                                            task.DATA_ESECUZIONE_FINE,
                                            task.IND_ESITO.GetValueOrDefault() ? "Eseguito con successo" : "Errore",
                                            task.NOT_ERRORE ?? "-",
                                            task.OUTPUT ?? "-");

                mail.InvioMail(testo, Mail.Oggetto, Mail.Destinatari, Mail.DestinatariCC, Mail.Mittente, null, Mail.DestinatariCCN);
            }
        }
        public virtual bool CheckParam(out string errore)
        {
            throw new NotImplementedException();
        }

        public virtual bool Esegui(out string output, out string errore)
        {
            throw new NotImplementedException();
        }
    }
    public class TaskHelper
    {
    

        public static bool NomeFunzione(out string output, out string errorMsg)
        {
            bool result = true;
            errorMsg = "";
            output = "ciao";

            return result;
        }

        public static void CreateTaskFromSchedule()
        {
            using (var db = new IncentiviEntities())
            {
                DateTime now = DateTime.Now;

                var query = db.XR_TSK_SCHEDULER.AsQueryable();

                //Verifico che siano attivi
                query = query.Where(x => x.IND_ACTIVE);

                //Verifico che siano compresi tra DTA_START e DTA_EXPIRE
                query = query.Where(x => x.DTA_START < now && (x.DTA_EXPIRE == null || x.DTA_EXPIRE > now));

                var listSchedule = query.ToList();
                bool anyChange = false;
                foreach (var sch in listSchedule)
                {
                    bool addTask = false;
                    XR_TSK_TASK last = null;
                    if (sch.XR_TSK_TASK == null || !sch.XR_TSK_TASK.Any())
                        addTask = true;
                    else
                    {
                        last = sch.XR_TSK_TASK.OrderByDescending(x => x.DATA_CREAZIONE).FirstOrDefault();

                        switch (sch.SCHEDULE_TYPE)
                        {
                            case "ONCE":
                                if (sch.REPEAT_RECURRENCE.HasValue)
                                {
                                    DateTime? limit = null;
                                    if (sch.REPEAT_DURATION.HasValue)
                                    {
                                        var first = sch.XR_TSK_TASK.OrderBy(x => x.DATA_CREAZIONE).FirstOrDefault();
                                        limit = first.DATA_CREAZIONE.AddMinutes(sch.REPEAT_DURATION.Value);
                                    }

                                    DateTime newExec = last.DATA_CREAZIONE.AddMinutes(sch.REPEAT_RECURRENCE.Value);
                                    if (sch.REPEAT_DURATION.HasValue && newExec > limit)
                                    {
                                        anyChange = true;
                                        sch.IND_ACTIVE = false;
                                    }
                                    else
                                        addTask = newExec < now;
                                }
                                else
                                {
                                    anyChange = true;
                                    sch.IND_ACTIVE = false; //Cosi evitiamo che venga estratto di nuovo
                                }
                                break;
                            case "DAILY":
                                addTask = last.DATA_CREAZIONE.AddDays(sch.DAILY_RECURRENCE.Value) < now;
                                break;
                            case "WEEKLY":
                                addTask = last.DATA_CREAZIONE.AddDays(sch.WEEKLY_RECURRENCE.Value * 7) < now;
                                break;
                            case "MONTHLY":
                                if (!String.IsNullOrWhiteSpace(sch.MONTHLY_MONTHS) && !String.IsNullOrWhiteSpace(sch.MONTHLY_DAYS))
                                {
                                    //controlla che sia il giorno corretto
                                    bool isThisMonth = sch.MONTHLY_MONTHS.Split(',').Contains(now.Month.ToString());
                                    bool isThisDay = sch.MONTHLY_DAYS.Split(',').Contains(now.Day.ToString()) 
                                                        || (sch.MONTHLY_DAYS.Contains("LAST") && now.Day==DateTime.DaysInMonth(now.Year, now.Month));
                                    if (isThisMonth && isThisDay)
                                    {
                                        addTask = last.DATA_CREAZIONE.Date < now.Date;
                                    }

                                    //controlla l'orario di esecuzione
                                    if (addTask)
                                    {
                                        if (last.DATA_CREAZIONE.Hour != now.Hour || last.DATA_CREAZIONE.Minute != now.Minute)
                                            addTask = false;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }


                    if (addTask)
                    {
                        anyChange = true;
                        sch.XR_TSK_TASK.Add(new XR_TSK_TASK()
                        {
                            COD_TIPOLOGIA = sch.COD_TIPOLOGIA,
                            COD_SOTTOTIPOLOGIA = sch.COD_SOTTOTIPOLOGIA,
                            DATA_CREAZIONE = now.AddSeconds(-now.Second).AddMilliseconds(-now.Millisecond),
                            DATA_ESECUZIONE = null,
                            DATA_ESECUZIONE_FINE = null,
                            DATA_PROGRAMMATA = null,
                            IND_ESITO = null,
                            IND_RIESEGUI = false,
                            INPUT = sch.INPUT,
                            MATRICOLA_CREATORE = "SCHEDULER",
                            IND_RUNNING = null,
                            NOT_ERRORE = null,
                            OUTPUT = null
                        });
                    }
                }

                if (anyChange)
                    DBHelper.Save(db, "SCHEDULER");
            }
        }
        private static bool GetTipologiaData(string codTipologia, string sottoTipologia, out XR_TSK_TIPOLOGIE tipologia, out string errore)
        {
            bool result = false;
            errore = null;
            tipologia = null;

            var db = new IncentiviEntities();
            tipologia = db.XR_TSK_TIPOLOGIE.FirstOrDefault(x => x.COD_TIPOLOGIA == codTipologia && x.COD_SOTTOTIPOLOGIA == sottoTipologia);
            if (tipologia == null)
                errore = "Tipologia non trovata";
            else
            {
                bool any = tipologia.IND_RECORDESCLUSIVO.GetValueOrDefault() && CheckExistingTask(db, codTipologia, sottoTipologia, out int lastReq);

                if (any)
                    errore = "Esecuzione già richiesta";
                else
                {
                    result = true;
                }
            }

            if (!result)
                tipologia = null;

            return result;
        }

        public static XR_TSK_TASK UltimaEsecuzione(IncentiviEntities db, string codTipologia, string sottoTipologia)
        {
            XR_TSK_TASK result = null;

            if (db == null)
                db = new IncentiviEntities();

            result = db.XR_TSK_TASK.Where(x => x.COD_TIPOLOGIA == codTipologia && x.COD_SOTTOTIPOLOGIA == sottoTipologia && x.IND_ESITO.HasValue)
                        .OrderByDescending(x => x.DATA_ESECUZIONE)
                        .FirstOrDefault();

            return result;
        }

        public static bool CheckExistingTask(IncentiviEntities db, string codTipologia, string sottoTipologia, out int lastReq)
        {
            lastReq = 0;
            if (db == null)
                db = new IncentiviEntities();
            lastReq = db.XR_TSK_TASK.Where(x => x.COD_TIPOLOGIA == codTipologia && x.COD_SOTTOTIPOLOGIA == sottoTipologia && x.IND_ESITO == null && (x.IND_RUNNING == null || !x.IND_RUNNING.Value)).Select(x => x.ID).FirstOrDefault();
            return lastReq > 0;
        }

        public static void AddFileWriterTask(string path, string textContent, bool impersonate, HrisParam? impersonateParam = null, string note = null, TaskLogMail mail = null)
        {
            FileWriter fileWriter = new FileWriter()
            {
                Path = path,
                TextContent = textContent,
                Impersonate = true,
                ImpersonateHrisParam = HrisParam.CredenzialiServerCezanne
            };
            fileWriter.Note = note;
            if (mail != null)
                fileWriter.Mail = mail;

            XR_TSK_TASK task = new XR_TSK_TASK()
            {
                DATA_CREAZIONE = DateTime.Now,
                MATRICOLA_CREATORE = CommonHelper.GetCurrentUserMatricola(),
                IND_RIESEGUI = false,
                COD_TIPOLOGIA = "FileWriter",
                INPUT = Newtonsoft.Json.JsonConvert.SerializeObject(fileWriter)
            };

            using (var db = new IncentiviEntities())
            {
                db.XR_TSK_TASK.Add(task);
                DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            }
        }

        public static bool AddBatchRunnerTask(string sottotipologia, out string errore, string path = "", string arguments = null, string note = null, TaskLogMail mail = null)
        {
            bool result = false;
            if (GetTipologiaData("BatchRunner", sottotipologia, out var tipologia, out errore))
            {
                BatchRunner fileWriter = null;
                if (!String.IsNullOrWhiteSpace(tipologia.DEFAULT_INPUT))
                {
                    try
                    {
                        fileWriter = Newtonsoft.Json.JsonConvert.DeserializeObject<BatchRunner>(tipologia.DEFAULT_INPUT);
                    }
                    catch (Exception)
                    {
                        fileWriter = new BatchRunner();
                    }
                }
                else
                    fileWriter = new BatchRunner();

                if (!String.IsNullOrWhiteSpace(path))
                    fileWriter.Path = path;

                fileWriter.Arguments = fileWriter.Arguments ?? "";
                if (!String.IsNullOrWhiteSpace(arguments))
                    fileWriter.Arguments += " " + arguments;

                fileWriter.Note = note;

                if (mail != null)
                    fileWriter.Mail = mail;

                XR_TSK_TASK task = new XR_TSK_TASK()
                {
                    DATA_CREAZIONE = DateTime.Now,
                    MATRICOLA_CREATORE = CommonHelper.GetCurrentUserMatricola(),
                    IND_RIESEGUI = tipologia.IND_RIESEGUI.GetValueOrDefault(),
                    COD_TIPOLOGIA = "BatchRunner",
                    COD_SOTTOTIPOLOGIA = sottotipologia,
                    INPUT = Newtonsoft.Json.JsonConvert.SerializeObject(fileWriter),
                };

                using (var db = new IncentiviEntities())
                {
                    db.XR_TSK_TASK.Add(task);
                    DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                }
                result = true;
            }

            return result;
        }

        public static void AddFtpManagerTask(FtpManager param)
        {
            XR_TSK_TASK task = new XR_TSK_TASK()
            {
                DATA_CREAZIONE = DateTime.Now,
                MATRICOLA_CREATORE = CommonHelper.GetCurrentUserMatricola(),
                IND_RIESEGUI = false,
                COD_TIPOLOGIA = "FtpManager",
                INPUT = Newtonsoft.Json.JsonConvert.SerializeObject(param)
            };

            using (var db = new IncentiviEntities())
            {
                db.XR_TSK_TASK.Add(task);
                DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            }
        }
    }
}
