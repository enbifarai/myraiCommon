using ClosedXML.Excel;
using myRai.Business;
using myRai.Models;
using myRaiData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;


namespace myRai.Controllers
{
    public class CeitonGraph
    {
        public CeitonGraph()
        {

        }
        public CeitonGraph(DateTime date, int richiesteTot, int richiesteErrori)
        {
            Date = date;
            RichiesteTOT = richiesteTot;
            RichiesteErrori = richiesteErrori;
            if (RichiesteTOT>0 && RichiesteErrori>0)
            {
                Rapporto = Math.Round((double)RichiesteErrori / (double)RichiesteTOT * 100,2);
            }
        }
        public DateTime Date { get; set; }
        public string Label { get; set; }
        public int RichiesteTOT { get; set; }
        public int RichiesteErrori { get; set; }
        public double Rapporto { get; set; }
    }

    public class CeitonErrorTable
    {
        public DateTime Date { get; set; }

        public List<wsGappCeiton_Richieste> Richieste { get; set; }

        public List<MyRai_CeitonLog> RichiesteRPM { get; set; }
    }

    public class CeitonErrorTypeTable
    {
        public DateTime DateDa { get; set; }

        public DateTime DateA { get; set; }

        public List<wsGappCeiton_Richieste> Richieste { get; set; }
    }


    public class CeitonTechController : Controller
    {
        //
        // GET: /CeitonTech/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RichiestePerGiornoChart(string dataDa, string dataAl)
        {
            List<CeitonGraph> richieste = new List<CeitonGraph>();

            DateTime fromDate = DateTime.Today.AddDays(-15);
            DateTime toDate = DateTime.Today.AddDays(1);

            if (!String.IsNullOrWhiteSpace(dataDa))
            {
                fromDate = DateTime.ParseExact(dataDa, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                toDate = DateTime.ParseExact(dataAl, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1);
            }

            HRGBEntities db = new HRGBEntities();
            var query = db.wsGappCeiton_Richieste
                .Where(x => x.Data_Operazione >= fromDate && x.Data_Operazione < toDate)
                .Select(a=>new { data = a.Data_Operazione, esito = a.Esito }).ToList()
                .GroupBy(y => y.data.Value.Date);

            for (DateTime i = fromDate; i < toDate; i=i.AddDays(1))
            {
                var elem = query.FirstOrDefault(x => x.Key == i);
                richieste.Add(new CeitonGraph()
                {
                    Label = i.ToString("dd-MM-yyyy"),
                    RichiesteTOT = elem!=null?elem.Count():0,
                    RichiesteErrori = elem!=null?elem.Count(x=>!x.esito.GetValueOrDefault()):0
                });
            }
            
            return PartialView("subpartial/RichiestePerGiornoChart", richieste);
        }

        public ActionResult RichiestePerGiornoExport(string dataDa, string dataAl)
        {
            DateTime fromDate = DateTime.ParseExact(dataDa, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime toDate = DateTime.ParseExact(dataAl, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1);

            HRGBEntities db = new HRGBEntities();
            var richieste = db.wsGappCeiton_Richieste
                .Where(x => x.Data_Operazione >= fromDate && x.Data_Operazione < toDate).ToList();

            var query = richieste
                .Where(x=>!x.Esito.Value)
                .OrderBy(x=>x.Data_Operazione)
                .Select(a => new
                {
                    a.ID,
                    a.WorkOrderID,
                    a.Matricola,
                    a.Origine,
                    a.Destinazione,
                    a.Data_Operazione,
                    a.Data_Riferimento,
                    a.Desc_Messaggio_Errore_Flusso,
                    a.Messaggio_Soap
                })
                .ToList();

            XLWorkbook workbook = new XLWorkbook();
            var sheetRpt = workbook.Worksheets.Add("Report");
            sheetRpt.Cell(1, 1).SetValue("Data da:");
            sheetRpt.Cell(1, 2).SetValue(fromDate.ToString("dd/MM/yyyy"));
            sheetRpt.Cell(1, 3).SetValue("Data a:");
            sheetRpt.Cell(1, 4).SetValue(toDate.AddDays(-1).ToString("dd/MM/yyyy"));

            var qryRpt = richieste.OrderBy(x => x.Data_Operazione.Value.Date).GroupBy(x => x.Data_Operazione.Value.Date)
                .Select(y => new CeitonGraph(y.Key, y.Count(), y.Count(z=>!z.Esito.Value))).ToList();

            DateTime rif = fromDate;
            while (rif<=toDate)
            {
                if (!qryRpt.Any(x => x.Date == rif))
                    qryRpt.Add(new CeitonGraph(rif, 0, 0));
                rif = rif.AddDays(1);
            }
            qryRpt = qryRpt.OrderBy(x => x.Date).ToList();

            var tableRpt = sheetRpt.Cell(3, 1).InsertTable(qryRpt, "ReportTable", true);
            sheetRpt.Columns().AdjustToContents();


            var worksheet = workbook.Worksheets.Add("Elenco errori");

            worksheet.Cell(1, 1).SetValue("Data da:");
            worksheet.Cell(1, 2).SetValue(fromDate.ToString("dd/MM/yyyy"));
            worksheet.Cell(1, 3).SetValue("Data a:");
            worksheet.Cell(1, 4).SetValue(toDate.AddDays(-1).ToString("dd/MM/yyyy"));

            var table = worksheet.Cell(3, 1).InsertTable(query, "ErrorsTable", true);
            worksheet.Columns().AdjustToContents();

            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            string nomeFile = String.Format("Errori {0:yyyyMMdd} - {1:yyyyMMdd}", fromDate, toDate.AddDays(-1));
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }

        public ActionResult RichiestePerOraChart(string data)
        {
            List<CeitonGraph> richieste = new List<CeitonGraph>();

            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Now;
            DateTime rifDate = DateTime.Now.AddHours(1);

            if (!String.IsNullOrWhiteSpace(data))
            {
                fromDate = DateTime.ParseExact(data, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                if (fromDate!=DateTime.Today)
                { 
                    rifDate = fromDate.AddDays(1);
                    toDate = rifDate.AddHours(-1);
                }
            }

            HRGBEntities db = new HRGBEntities();
            var query = db.wsGappCeiton_Richieste
                .Where(x => x.Data_Operazione >= fromDate && x.Data_Operazione < rifDate)
                .Select(a => new { data = a.Data_Operazione, esito = a.Esito }).ToList()
                .GroupBy(y => y.data.Value.Hour);

            for (int i = 0; i <= toDate.Hour; i++)
            {
                var elem = query.FirstOrDefault(x => x.Key == i);
                richieste.Add(new CeitonGraph()
                {
                    Date = fromDate.Date,
                    Label = String.Format("{0}:00 - {1}:00",i, i+1),
                    RichiesteTOT = elem != null ? elem.Count() : 0,
                    RichiesteErrori = elem != null ? elem.Count(x => !x.esito.GetValueOrDefault()) : 0
                });
            }

            return PartialView("subpartial/RichiestePerOraChart", richieste);
        }

        public ActionResult ErroriPerGiornataTable(string data, string matr, string dataRif, string errore, string conErrore)
        {
            CeitonErrorTable model = new CeitonErrorTable();

            model.Richieste = new List<wsGappCeiton_Richieste>();

            DateTime fromDate = DateTime.Today;
            if (!String.IsNullOrWhiteSpace(data))
                fromDate = DateTime.ParseExact(data, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime toDate = fromDate.AddDays(1);

            DateTime rifDate = new DateTime();
            if (!String.IsNullOrWhiteSpace(dataRif))
                rifDate = DateTime.ParseExact(dataRif, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            HRGBEntities db = new HRGBEntities();
            
            var query = db.wsGappCeiton_Richieste.Include("wsGappCeiton_Operazioni").Where(x=>true);

            if (String.IsNullOrWhiteSpace(conErrore) || conErrore == "true")
                query = query.Where(x => x.Esito==null || !x.Esito.Value);
         
            if (!String.IsNullOrWhiteSpace(matr))
                query = query.Where(x => x.Matricola.Substring(1, 6) == matr);

            if (!String.IsNullOrWhiteSpace(dataRif))
                query = query.Where(x => x.Data_Riferimento != null && x.Data_Riferimento.Value == rifDate);

            if (!String.IsNullOrWhiteSpace(errore))
            {
                string tmpErrore = errore.ToUpper();
                query = query.Where(x => x.Desc_Messaggio_Errore_Flusso.Contains(tmpErrore));
            }

            if (!String.IsNullOrWhiteSpace(data) || (String.IsNullOrWhiteSpace(matr) && String.IsNullOrWhiteSpace(dataRif) && String.IsNullOrWhiteSpace(errore)))
                query = query.Where(x => x.Data_Operazione >= fromDate && x.Data_Operazione < toDate);

            var tmp = query.ToList();

            if (tmp.Count()>0)
                model.Richieste.AddRange(tmp);
            
            model.Date = fromDate;

            return PartialView("subpartial/ErroriTable", model);
        }

        public ActionResult ErroriPerGiornataTableRPM(string data, string matr, string dataRif, string errore, string conErrore)
        {
          CeitonErrorTable model = new CeitonErrorTable();

            DateTime fromDate = DateTime.Today;
            if (!String.IsNullOrWhiteSpace(data))
                fromDate = DateTime.ParseExact(data, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime toDate = fromDate.AddDays(1);

            DateTime rifDate = new DateTime();
            if (!String.IsNullOrWhiteSpace(dataRif))
                rifDate = DateTime.ParseExact(dataRif, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            digiGappEntities dbRPM = new digiGappEntities();

            model.RichiesteRPM = new List<MyRai_CeitonLog>();

            var queryRPM = dbRPM.MyRai_CeitonLog.Where(x => true);

            if (String.IsNullOrWhiteSpace(conErrore) || conErrore == "true")
                queryRPM = queryRPM.Where(x => !x.esito);

            if (!String.IsNullOrWhiteSpace(matr))
                queryRPM = queryRPM.Where(x => x.matricola == matr);

            if (!String.IsNullOrWhiteSpace(dataRif))
                queryRPM = queryRPM.Where(x => x.data == rifDate);

            if (!String.IsNullOrWhiteSpace(errore))
                queryRPM = queryRPM.Where(x => x.errore == errore);

            if (!String.IsNullOrWhiteSpace(data) || (String.IsNullOrWhiteSpace(matr) && String.IsNullOrWhiteSpace(dataRif) && String.IsNullOrWhiteSpace(errore)))
                queryRPM = queryRPM.Where(x => x.data_ultimo_invio >= fromDate && x.data_ultimo_invio < toDate);

            var tmpRPM = queryRPM.ToList();

            if (tmpRPM.Count() > 0)
                model.RichiesteRPM.AddRange(tmpRPM);

            model.Date = fromDate;

            return PartialView("subpartial/ErroriTableRPM", model);
        }

        public ActionResult ErroriRichiestePerTipo(string dataDa, string dataAl)
        {
            CeitonErrorTypeTable model = new CeitonErrorTypeTable();
            model.Richieste = new List<wsGappCeiton_Richieste>();

            DateTime fromDate = DateTime.Today.AddDays(-15);
            DateTime toDate = DateTime.Today.AddDays(1);

            if (!String.IsNullOrWhiteSpace(dataDa))
            {
                fromDate = DateTime.ParseExact(dataDa, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                toDate = DateTime.ParseExact(dataAl, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1);
            }

            model.DateDa = fromDate;
            model.DateA = toDate.AddDays(-1);

            HRGBEntities db = new HRGBEntities();
            var query = db.wsGappCeiton_Richieste
                .Where(x => !x.Esito.Value &&  x.Data_Operazione >= fromDate && x.Data_Operazione < toDate)
                //.GroupBy(a => a.Desc_Messaggio_Errore_Flusso)
                .ToList();

            if (query != null)
                model.Richieste.AddRange(query);

            return PartialView("subpartial/ErroriTipoTable", model);
        }

        public ActionResult ExportError(string datadaet, string dataalet)
        {
            DateTime fromDate = DateTime.Today.AddDays(-15);
            DateTime toDate = DateTime.Today.AddDays(1);

            if (!String.IsNullOrWhiteSpace(datadaet))
            {
                fromDate = DateTime.ParseExact(datadaet, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                toDate = DateTime.ParseExact(dataalet, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1);
            }

            
            HRGBEntities db = new HRGBEntities();
            var query = db.wsGappCeiton_Richieste
                .Where(x => !x.Esito.Value && x.Data_Operazione >= fromDate && x.Data_Operazione < toDate)
                .Select(a=> new
                {
                    a.ID,
                    a.WorkOrderID,
                    a.Matricola,
                    a.Origine,
                    a.Destinazione,
                    a.Data_Operazione,
                    a.Data_Riferimento,
                    a.Desc_Messaggio_Errore_Flusso,
                    a.Messaggio_Soap
                })
                .ToList();

            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Elenco errori");

            worksheet.Cell(1, 1).SetValue("Data da:");
            worksheet.Cell(1, 2).SetValue(fromDate.ToString("dd/MM/yyyy"));
            worksheet.Cell(1, 3).SetValue("Data a:");
            worksheet.Cell(1, 4).SetValue(toDate.AddDays(-1).ToString("dd/MM/yyyy"));

            var table = worksheet.Cell(3, 1).InsertTable(query, "ErrorsTable", true);
            worksheet.Columns().AdjustToContents();
            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            
            string nomeFile = String.Format("Errori {0:yyyyMMdd} - {1:yyyyMMdd}", fromDate, toDate.AddDays(-1));
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }
    }
}
