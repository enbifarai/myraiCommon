using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public class MappaturaController : Controller
    {
        //
        // GET: /Mappatura/


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Elenco_gruppi()
        {
            var elenco = MappaturaManager.GetElencoGruppi();

            return View(elenco);
        }

        public ActionResult Dettaglio_gruppo(string codicegruppo)
        {
            var tmp = MappaturaManager.GetElencoGruppi(codicegruppo, false);
            MPGGruppo gruppo = null;
            if (tmp != null)
                gruppo = tmp.First();

            return View(gruppo);
        }

        public ActionResult ModalAggiuntaDipendenti(string codicegruppo)
        {
            var tmp = MappaturaManager.GetElencoGruppi(codicegruppo, true);
            MPGGruppo gruppo = null;
            if (tmp != null)
                gruppo = tmp.First();

            return View("Associazione_gruppo", gruppo);
        }

        public ActionResult Elenco_Stanze_NoGruppo()
        {
            var elenco = MappaturaManager.GetStanzeNotAssigned();

            return View(elenco);
        }

        public ActionResult AssociaDipendente(string matricola, string codicegruppo)
        {
            string result = "";

            if (!MappaturaManager.AssociaDipendente(matricola, codicegruppo, out result))
                return Content(result);

            return Content("OK");
        }
        public ActionResult AssociaDipendenti(string elencoMatr, string codicegruppo)
        {
            string result = "";

            if (!MappaturaManager.AssociaDipendenti(elencoMatr, codicegruppo, out result))
                return Content(result);

            return Content("OK");
        }

        public ActionResult DisassociaDipendente(string matricola, string codicegruppo)
        {
            string result = "";

            if (!MappaturaManager.DisassociaDipendente(matricola, codicegruppo, out result))
                return Content(result);

            return Content("OK");
        }
        public ActionResult DisassociaDipendenti(string elencoMatr, string codicegruppo)
        {
            string result = "";

            if (!MappaturaManager.DisassociaDipendenti(elencoMatr, codicegruppo, out result))
                return Content(result);

            return Content("OK");
        }

        public ActionResult EsportaAppuntamenti(string cs, string df, string dt, string nst, string fo = "excel")
        {
            string CodiceSede = cs;
            string DataFrom = df;
            string DataTo = dt;
            string NumeroStanza = nst;
            string formato = fo;

            if (MappaturaManager.EsportaAppuntamenti(CodiceSede, DataFrom, DataTo, NumeroStanza, formato, out byte[] output, out string errorMsg))
            {
                string fileName = "Prenotazioni";
                string contentType = "";
                if (formato.ToLower() == "excel")
                {
                    fileName += ".xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
                else if (formato.ToLower() == "pdf")
                {
                    fileName += ".pdf";
                    contentType = "application/pdf";
                }

                return new FileStreamResult(new MemoryStream(output), contentType) { FileDownloadName = fileName };
            }
            else
                return Content(errorMsg);
        }
    }
}
