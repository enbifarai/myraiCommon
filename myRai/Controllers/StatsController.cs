using myRai.Models;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class StatsController : Controller
    {
        //
        // GET: /Stats/

        public ActionResult Index()
        {
            if (!UtenteHelper.IsAdmin(CommonHelper.GetCurrentUserMatricola()))
            {
                throw (new Exception("Unauthorized"));
            }
            return View();
        }

        public ActionResult getData(string tabella, string operazione, int intervallo, string datada, string dataa)
        {
            var db = new myRaiData.digiGappEntities();
            DateTime D1;
            DateTime.TryParseExact(datada, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out D1);

            DateTime D2;
            DateTime.TryParseExact(dataa, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out D2);

            var dati = db.MyRai_LogAzioni.Where(x => x.operazione == operazione && x.data >= D1 && x.data <= D2).ToList();
            List<string> ListaLabels = new List<string>();
            List<punto> ListaPunti = new List<punto>();

            while (D1 < D2)
            {
                DateTime D1PiuInt = D1.AddMinutes(intervallo);

                ListaLabels.Add(D1.ToString("dd-MM HH:mm"));
                int t = dati.Where(x => x.data >= D1 && x.data <= D1PiuInt).Count();
                punto p= new punto (){ meta = D1.ToString("ddMMyyyy HH:mm") + "/" + D1PiuInt.ToString("HH:mm ") + t.ToString(), value = t };
                ListaPunti.Add(p);
                D1 = D1.AddMinutes(30);
            }

            //string q=" DECLARE @DateTime1 DATETIME = '"+D1.ToString("yyyy/MM/dd HH:mm") +"';DECLARE @DateTime2 DATETIME = '"+ D2.ToString("yyyy/MM/dd HH:mm") + "';DECLARE @intervallo int = "+intervallo+";" +
            //         "WITH cte30MinIncrements AS(SELECT convert(DATETIME, '"+ D1.ToString("yyyy-MM-dd HH:mm") + "') as DT" +
            //        // "WITH cte30MinIncrements AS(SELECT convert(DATETIME, '2019-12-12 08:00:00') as DT" +
            //         " UNION ALL " +
            //         " SELECT DATEADD(MINUTE, @intervallo, DT) " +
            //         " FROM cte30MinIncrements WHERE DATEADD(MINUTE, @intervallo, DT) <= @DateTime2" +
            //         ") " +
            //         " SELECT " +
            //         " * , tot = (select count(*) from "+tabella+" where operazione = '"+operazione+"' and data>= cte30MinIncrements.DT and data<= DATEADD(MINUTE, @intervallo, cte30MinIncrements.DT))" +
            //         " FROM cte30MinIncrements";

           // var res= db.Database.SqlQuery<result>(q).ToList();
            //string[] labels = res.Select(x => x.DT.ToString("dd-MM HH:mm")).ToArray();
            string[] labels = ListaLabels.ToArray();

            //punto[] serie = res.Select(a => new punto { meta=a.DT.ToString("ddMMyyyy HH:mm")+"/"+ a.DT.AddMinutes(30).ToString("HH:mm ") + a.tot.ToString(), value = a.tot }).ToArray();
            punto[] serie = ListaPunti.ToArray();

            //int[] serie = res.Select(x => x.tot).ToArray();
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { labels = labels, serie=serie }
            };
        }
    }
    public class result
    {
        public DateTime DT { get; set; }
        public int tot { get; set; }
    }
    public class punto
    {
        public string meta { get; set; }
        public int  value { get; set; }
    }
}
