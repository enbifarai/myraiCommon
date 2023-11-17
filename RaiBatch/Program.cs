using myRai.DataAccess;
using myRaiCommonTasks;
using myRaiData.Incentivi;
using myRaiHelper;
using myRaiHelper.Task;
using RaiBatch.Auth;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using WebPush;
using static myRaiHelper.AccessoFileHelper;

namespace RaiBatch
{
    class Program
    {
        private static log4net.ILog _log;
        private static List<string> _assemblyPath;
        private static string _baseOutput;

        static void Main(string[] args)
        {
            _baseOutput = CommonHelper.GetAppSettings("Output");
            if (!Directory.Exists(_baseOutput))
                Directory.CreateDirectory(_baseOutput);

            string main = args.Count() > 0 ? args[0] : "RaiBatch";
            string func = args.Count() > 1 ? args[1] : "";

            InitAssemblyLocation();
            InitLogger(main, func);

            _log.Info("Avvio batch");

            switch (main)
            {
                case "Task":
                    EseguiTaskGenerici(func, args);
                    break;
                case "Notify":
                    ManageNotify();
                    break;
                case "getfiles":
                    getfiles(args);
                    break;
                   
                case "ImportUtehra":
                    MainClass M = new MainClass();
                    M.ImportUtehra(null, _baseOutput);
                    break;
                default:
                    try
                    {
                        string assemblyPath = main.Contains(".") ? main.Split('.')[0] : main;
                        string typeName = main;
                        ObjectHandle mainObjWrap = AppDomain.CurrentDomain.CreateInstance(assemblyPath, typeName + ".MainClass");
                        if (mainObjWrap != null)
                        {
                            dynamic mainObj = mainObjWrap.Unwrap();
                            mainObj.InitLog(_log, _baseOutput);
                            mainObj.Entry(args);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Fatal(ex);
                    }
                    break;
            }

            _log.Info("Termine batch");
        }

        private static void getfiles(string[] args)
        {
            //var db = new IncentiviEntities();
            //string query = "SELECT * FROM XR_HRIS_REGOLE_VOCI_MENU";


            //SqlCommand sqlCommand = new SqlCommand();
            //SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
            //DataSet ds = new DataSet();
            //using (SqlConnection sqlConnection = new SqlConnection(db.Database.Connection.ConnectionString))
            //{
            //    sqlCommand.Connection = sqlConnection;
            //    sqlCommand.CommandType = CommandType.Text;
            //    sqlCommand.CommandText = query;
            //    da.Fill(ds);
            //}

            //foreach (DataTable dataTable in ds.Tables)
            //{
            //    string result = Newtonsoft.Json.JsonConvert.SerializeObject(dataTable);
            //}
            
            string percorso = @"\\nasrm3\pfoac\BATCH\VALORIZZAZIONE";
            string percorso1 = @"\\nasrm3\pfoac\BATCH\VALORIZZAZIONE\BK";
            ImpersonationHelper.Impersonate("RAI", "srvruofpo", "zaq22?mk", delegate
            {
                var file = Directory.GetFiles(percorso, "*.*");
                _log.Info(percorso);
                _log.Info(String.Format("{0} file trovati", file != null ? file.Count() : 0));
                foreach (var item in file)
                {
                    _log.Info(item);
                }

                file = Directory.GetFiles(percorso1, "*.*");
                _log.Info(percorso1);
                _log.Info(String.Format("{0} file trovati", file != null ? file.Count() : 0));
                foreach (var item in file)
                {
                    _log.Info(item);
                }
            });
        }


        private static void InitLogger(string main, string func)
        {
            log4net.GlobalContext.Properties["LogMain"] = main;
            log4net.GlobalContext.Properties["LogName"] = String.Format("{0}_{1:yyyyMMdd}.log", func, DateTime.Today);

            _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        private static void InitAssemblyLocation()
        {
            _assemblyPath = new List<string>();

            string tmpLocation = System.Reflection.Assembly.GetEntryAssembly().Location;
            string currentLocation = Path.GetDirectoryName(tmpLocation);

            InsertAssemblyPath(currentLocation);
            InsertAssemblyPath(Environment.CurrentDirectory);

            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static void InsertAssemblyPath(string assemblyPath)
        {
            if (!_assemblyPath.Contains(assemblyPath))
                _assemblyPath.Insert(0, assemblyPath);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;


            assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == args.Name);
            if (assembly != null)
                return assembly;


            string assemblyPath = Path.GetDirectoryName(args.Name);
            string assemblyName = Path.GetFileName(args.Name);


            if (!String.IsNullOrWhiteSpace(assemblyPath))
                InsertAssemblyPath(assemblyPath);
            else
            {
                try
                {
                    AssemblyName assName = new AssemblyName(args.Name);
                    assemblyName = assName.Name + ".dll";
                }
                catch (Exception)
                {

                }
            }

            foreach (var item in _assemblyPath)
            {
                string assemblyFile = Path.Combine(item, assemblyName);
                try
                {
                    assembly = Assembly.LoadFile(assemblyFile);
                    if (assembly != null)
                        break;
                }
                catch (Exception ex)
                {
                    _log.Debug(String.Format("Assembly {0} non trovato", assemblyFile), ex);
                }
            }

            if (assembly == null)
                throw new DllNotFoundException();

            return assembly;
        }

        private static void EseguiTaskGenerici(string func, params string[] args)
        {
            int? idTask = null;

            if (func == "all")
            {
                //Controllo prima le schedulazioni così da trovare i task già pronti
                TaskHelper.CreateTaskFromSchedule();
            }
            else
            {
                if (args.Length < 2)
                {
                    _log.Error("Task non specificato");
                    return;
                }
                else
                    idTask = Convert.ToInt32(args[2]);
            }

            var db = new IncentiviEntities();
            List<XR_TSK_TASK> listTask = null;
            if (func == "all")
                listTask = db.XR_TSK_TASK.Where(x => x.IND_RUNNING == null && x.IND_ESITO == null).ToList();
            else
                listTask = db.XR_TSK_TASK.Where(x => x.ID == idTask.Value).ToList();

            if (!listTask.Any())
            {
                _log.Info("Nessun task generico da eseguire");
                return;
            }
            _log.Info($"Trovati {listTask.Count()} task generici da eseguire");
            var tipiTask = db.XR_TSK_TIPOLOGIE.ToList();

            foreach (var task in listTask)
            {
                bool hasParamError = false;
                if (String.IsNullOrWhiteSpace(task.COD_TIPOLOGIA))
                {
                    hasParamError = true;
                    task.IND_ESITO = false;
                    task.NOT_ERRORE = "Tipologia non indicata";
                }
                else if (!tipiTask.Any(x => x.COD_TIPOLOGIA == task.COD_TIPOLOGIA && x.COD_SOTTOTIPOLOGIA == task.COD_SOTTOTIPOLOGIA))
                {
                    hasParamError = true;
                    task.IND_ESITO = false;
                    task.NOT_ERRORE = "Tipologia inesistente";
                }
                else if (String.IsNullOrWhiteSpace(task.INPUT))
                {
                    hasParamError = true;
                    task.IND_ESITO = false;
                    task.NOT_ERRORE = "Nessun input indicato";
                }
                else
                {
                    object instTask = null;
                    try
                    {
                        Type type = Type.GetType($"myRaiHelper.Task.{task.COD_TIPOLOGIA}, myRaiHelper");
                        instTask = Newtonsoft.Json.JsonConvert.DeserializeObject(task.INPUT, type);
                    }
                    catch (Exception ex)
                    {
                        hasParamError = true;
                        task.IND_ESITO = false;
                        task.NOT_ERRORE = ex.Message;
                    }

                    try
                    {
                        if (instTask != null)
                        {
                            bool runAsync = ((BaseTask)instTask).RunAsync;

                            if (!idTask.HasValue)
                                ((BaseTask)instTask).SetStart(task);
                            if (!((BaseTask)instTask).CheckParam(out string errParam))
                            {
                                hasParamError = true;
                                task.IND_ESITO = false;
                                task.NOT_ERRORE = errParam;
                            }
                            else
                            {
                                if (!runAsync || idTask.HasValue)
                                {
                                    task.IND_ESITO = ((BaseTask)instTask).Esegui(out string output, out string errore);
                                    task.OUTPUT = output;
                                    task.NOT_ERRORE = errore;
                                }
                                else
                                {
                                    Process p = new Process();
                                    p.StartInfo.FileName = Assembly.GetEntryAssembly().Location;
                                    p.StartInfo.Arguments = "Task specific " + task.ID.ToString();
                                    p.StartInfo.UseShellExecute = true;
                                    p.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                                    p.Start();
                                    //In questo caso il processo deve essere girato in asincrono, quindi non deve fare ulteriori operazioni
                                    //che poi sono delegate al processo chiamato
                                    continue;
                                }
                            }
                            ((BaseTask)instTask).SetEnd(task);
                            ((BaseTask)instTask).ManageMail(task);
                        }
                    }
                    catch (Exception ex)
                    {
                        task.IND_ESITO = false;
                        task.NOT_ERRORE = ex.Message;
                    }
                }

                if (!task.IND_ESITO.GetValueOrDefault() && !hasParamError && task.IND_RIESEGUI)
                {
                    XR_TSK_TASK newTask = new XR_TSK_TASK()
                    {
                        ID_ORIGINALE = task.ID_ORIGINALE ?? task.ID,
                        COD_TIPOLOGIA = task.COD_TIPOLOGIA,
                        DATA_CREAZIONE = DateTime.Now,
                        MATRICOLA_CREATORE = task.MATRICOLA_CREATORE,
                        INPUT = task.INPUT
                    };
                    db.XR_TSK_TASK.Add(newTask);
                }
                db.SaveChanges();
            }
        }

        private static void ManageNotify()
        {
            var db = new IncentiviEntities();

            var list = db.XR_HRIS_NOTIFICHE.Include("XR_HRIS_NOTIFICHE_LOG").AsQueryable();
            //Prendo quelle con destinatario per matricola
            list = list.Where(x => x.DEST_MATR != null);
            
            var result = list.ToList();

            foreach (var item in list)
            {
                var elencoMatr = item.DEST_MATR.Split();
                foreach (var matr in elencoMatr.Where(x => !item.XR_HRIS_NOTIFICHE_LOG.Any(y => y.MATRICOLA == x)))
                {
                    if (SendNotification(matr, item.MESSAGE))
                    {
                        item.XR_HRIS_NOTIFICHE_LOG.Add(new myRaiData.Incentivi.XR_HRIS_NOTIFICHE_LOG()
                        {
                            MATRICOLA = matr,
                            DTA_SENT = DateTime.Now
                        });
                    }
                }
            }

            DBHelper.Save(db, "ADMIN");
        }

        private static bool SendNotification(string matricola, string testo)
        {
            bool result = false;
            var db = new IncentiviEntities();
            var subscriptions = db.XR_HRIS_NOTIFICHE_SUBSCRIPTION.Where(x => x.MATRICOLA == matricola && x.IND_ACTIVE);
            if (subscriptions.Any())
            {
                foreach (var sub in subscriptions)
                {
                    NotificationSubscription dbSub = Newtonsoft.Json.JsonConvert.DeserializeObject<NotificationSubscription>(sub.SUBSCRIPTION);
                    PushSubscription subscription = new PushSubscription()
                    {
                        Endpoint = dbSub.endpoint,
                        P256DH = dbSub.keys.p256dh,
                        Auth = dbSub.keys.auth
                    };
                    var publicKey = "BIRBAd9SBKP9dtol7fXrinpOZ_uiLD5Bf8usPuPhD9Yr97sWaReOZCCY6_0FD4-9_svp7gaZyA4Vs26cpGdIOYk";
                    var privateKey = "IstkpohQiWnRbQ5y_47SU3wEh_kOfpmCwAABZAapuOs";
                    var vapidDetails = new VapidDetails("mailto:vincenzo.bifano@rai.it", publicKey, privateKey);
                    var webPushClient = new WebPushClient();
                    webPushClient.SendNotification(subscription, testo, vapidDetails);
                }
                result = true;
            }

            return result;
        }
    }
}
