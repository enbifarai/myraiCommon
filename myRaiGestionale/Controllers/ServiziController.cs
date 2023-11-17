using myRai.DataAccess;
using myRaiCommonManager;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public class ServiziController : BaseCommonController
    {
        //
        // GET: /Servizi/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Elenco_Servizi(string ricCodice="", string ricDes="", string ricDesExt="")
        {
            IncentiviEntities db = new IncentiviEntities();

            IQueryable<CODIFYIMP> tmpSoc = db.STRGROUP
                .Where(x => x.DTA_INIZIO <= DateTime.Today && x.DTA_FINE >= DateTime.Today)
                .SelectMany(x => x.R_STRGROUP)
                .OrderBy(y => y.PRG_SORT)
                .Select(x => x.CODIFYIMP);

            List<string> listSoc = tmpSoc.Select(x => x.COD_IMPRESA).ToList();
            
            var qry = db.XR_TB_SERVIZIO.Where(x=> listSoc.Contains(x.COD_IMPRESA) && x.COD_SERVIZIO.Trim().Length==2)
                        .GroupJoin(db.XR_TB_SERVIZIO_EXT,
                                    x => x.COD_SERVIZIO.Trim(),
                                    y => y.COD_SERVIZIO.Trim(),
                                    (x, y) => new { Serv = x, Ext = y });

            if (!String.IsNullOrWhiteSpace(ricCodice))
                qry = qry.Where(x => x.Serv.COD_SERVIZIO.Trim().ToUpper() == ricCodice.Trim().ToUpper());

            if (!String.IsNullOrWhiteSpace(ricDes))
                qry = qry.Where(x => x.Serv.DES_SERVIZIO.Substring(5).ToUpper().StartsWith(ricDes.ToUpper()));

            List<XR_TB_SERVIZIO> servizi = new List<XR_TB_SERVIZIO>();
            foreach (var item in qry)
            {
                XR_TB_SERVIZIO serv = item.Serv;
                if (item.Ext!=null && item.Ext.Any())
                    serv.RecEsteso = item.Ext.OrderByDescending(a => a.DTA_FINE).FirstOrDefault();
                servizi.Add(serv);
            }

            if (!String.IsNullOrWhiteSpace(ricDesExt))
                servizi = servizi.Where(x => x.RecEsteso!=null && (x.RecEsteso.DES_ESTESA??"").ToUpper().StartsWith(ricDesExt.ToUpper())).ToList();

            servizi = servizi.Where(x => x.RecEsteso == null || (x.RecEsteso.DTA_INIZIO <= DateTime.Today && x.RecEsteso.DTA_FINE >= DateTime.Today)).ToList();

            return View(servizi);
        }

        [HttpPost]
        public ActionResult Modal_Servizio(string codice)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_TB_SERVIZIO servizio = db.XR_TB_SERVIZIO.FirstOrDefault(x => x.COD_SERVIZIO.Trim() == codice.Trim());
            servizio.Storico = new List<XR_TB_SERVIZIO_EXT>();
            servizio.Storico.AddRange(db.XR_TB_SERVIZIO_EXT.Where(x => x.COD_SERVIZIO.Trim() == servizio.COD_SERVIZIO.Trim()));
            if (servizio.Storico.Any())
                servizio.RecEsteso = servizio.Storico.OrderByDescending(x => x.DTA_FINE).FirstOrDefault();
            else
                servizio.RecEsteso = new XR_TB_SERVIZIO_EXT() { DES_ESTESA = CezanneHelper.GetDes(servizio.COD_SERVIZIO, servizio.DES_SERVIZIO).TitleCase() };

            return View(servizio);
        }

        [HttpPost]
        public ActionResult Save_Servizio(XR_TB_SERVIZIO model)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_TB_SERVIZIO servizio = db.XR_TB_SERVIZIO.FirstOrDefault(x => x.COD_SERVIZIO.Trim() == model.COD_SERVIZIO.Trim());
            XR_TB_SERVIZIO_EXT ext = db.XR_TB_SERVIZIO_EXT.Where(x => x.COD_SERVIZIO.Trim() == model.COD_SERVIZIO.Trim()).OrderByDescending(x => x.DTA_FINE).FirstOrDefault();
            if (ext != null)
            {
                ext.DTA_FINE = DateTime.Today.AddDays(-1);
                if (ext.DTA_FINE <= ext.DTA_INIZIO)
                    db.XR_TB_SERVIZIO_EXT.Remove(ext);
            }

            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);

            XR_TB_SERVIZIO_EXT newExt = new XR_TB_SERVIZIO_EXT()
            {
                COD_SERVIZIO = model.COD_SERVIZIO.Trim(),
                DTA_INIZIO = DateTime.Today,
                DTA_FINE = AnagraficaManager.GetDateLimitMax(),
                DES_ESTESA = model.RecEsteso.DES_ESTESA,
                COD_USER = codUser,
                COD_TERMID = codTermid,
                TMS_TIMESTAMP = tms
            };
            db.XR_TB_SERVIZIO_EXT.Add(newExt);

            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("OK");
            else
                return Content("Errore durante il salvataggio");
        }
    }
}
