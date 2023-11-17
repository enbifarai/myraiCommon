using myRaiHelper.GenericRepository;
using System;
using System.Linq;
using System.Web.Mvc;
using myRaiCommonModel;
using myRaiData.Incentivi;
using System.Collections.Generic;
using myRaiHelper;
using ComunicaCics;
using myRaiGestionale.RepositoryServices;
using myRaiCommonManager;
using System.Net;
using myRaiGestionale.Extensions;
using myRaiGestionale.Exceptions;
using myRaiGestionale.Services;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;
using myRaiCommonTasks.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using static myRaiCommonManager.Widget_Da_Fare_Manager;
using myRaiData;

namespace myRaiGestionale.Controllers
{
    public class OnBoardingController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SessionHelper.Set("GEST_SECTION", "GESTIONE");
            base.OnActionExecuting(filterContext);
        }

        #region GET

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        //public ActionResult LoadRicercaImmatricolazione(DateTime? dal, DateTime? al, string matricola, string nome, string cognome)
        public ActionResult RicercaDocumenti(string stato, DateTime? dal, DateTime? al, string matricola, string nominativo)
        {
            List<AssunzioniVM> listaImmFilter = new List<AssunzioniVM>();
            try
            {
                var db = new IncentiviEntities();
                AssunzioneServizio assunzioneServizio = new AssunzioneServizio(db);
                var imm = assunzioneServizio.GetAssunzioni(stato).OrderByDescending(o => o.DataCreazione);
                if (dal.HasValue && dal.Value != DateTime.MinValue)
                {
                    imm = imm.Where(x => x.DataCreazione >= dal.Value).OrderByDescending(o => o.DataCreazione);
                }
                if (al.HasValue && al.Value != DateTime.MinValue)
                {
                    imm = imm.Where(x => x.DataCreazione <= dal.Value).OrderByDescending(o => o.DataCreazione);
                }
                if (!string.IsNullOrWhiteSpace(matricola))
                {
                    imm = imm.Where(x => x.Matricola == matricola).OrderByDescending(o => o.DataCreazione);
                }
                if (!string.IsNullOrWhiteSpace(nominativo))
                {
                    imm = imm.Where(x => x.Cognome.StartsWith(nominativo)).OrderByDescending(o => o.DataCreazione);
                }
                foreach (var item in imm)
                {
                    var descrizioneServizio = (db.XR_TB_SERVIZIO.FirstOrDefault(x => x.COD_SERVIZIO == item.SelectedServizio) ?? new XR_TB_SERVIZIO()).DES_SERVIZIO;
                    var descrizioneCategoria = (db.QUALIFICA.FirstOrDefault(w => w.COD_QUALIFICA == item.SelectedCategoria) ?? new QUALIFICA()).DES_QUALIFICA;
                    item.Servizio = descrizioneServizio;
                    item.Categoria = descrizioneCategoria;
                    item.SelectedTipoAssunzione = "ASSUNZIONE A TEMPO INDETERMINATO";
                    listaImmFilter.Add(item);
                }
            }
            catch (Exception ex)
            {
                string msgError = ex.ToString();
                listaImmFilter = new List<AssunzioniVM>();

            }
            return PartialView("_subpartial/Elenco_Immatricolazioni_Per_Assunzione", listaImmFilter);
        }
        #endregion

    }
}
