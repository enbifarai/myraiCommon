using System.IO;
using System;
using System.Web.Mvc;
using myRaiHelper;
using myRaiDataTalentia;
using System.Linq;
using System.Web;
using System.Net;

namespace myRai.Controllers
{
    public class FileController : Controller
    {
        public ActionResult Get(string path, string matricola, string fileName, string contentType = null)
        {
            string currentMatricola = CommonHelper.GetCurrentUserMatricola();
            if (matricola != currentMatricola)
            {
                return View("~/Views/Shared/NonAbilitatoError2.cshtml");
            }

            var tmp = "~";
            string rootPath = Server.MapPath(tmp) + "\\" + path;
            string filePath = Path.Combine(rootPath, matricola, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return View("~/Views/Shared/404.cshtml");
            }

            if (contentType != null)
                return File(filePath, contentType);
            else
                return File(filePath, "application/unknown");
        }

        /// <summary>
        /// Reperimento di un file
        /// </summary>
        /// <param name="filePath">path compreso di matricola e nome file. Es: Server.MapPath( ~ ) + \Media\000000\Penguins.jpg </param>
        /// <param name="matricola">Matricola dell'utente chiamante</param>
        /// <param name="contentType">ContentType del file richiesto</param>
        /// <returns></returns>
        public ActionResult Get(string filePath, string matricola, string contentType = null)
        {
            string currentMatricola = CommonHelper.GetCurrentUserMatricola();
            if (matricola != currentMatricola)
            {
                return View("~/Views/Shared/NonAbilitatoError2.cshtml");
            }

            if (!System.IO.File.Exists(filePath))
            {
                return View("~/Views/Shared/404.cshtml");
            }

            if (contentType != null)
                return File(filePath, contentType);
            else
                return File(filePath, "application/unknown");
        }

        /// <summary>
        /// Reperimento di un file
        /// </summary>
        /// <param name="filePath">path compreso di matricola e nome file. Es: Server.MapPath( ~ ) + \Media\000000\Penguins.jpg </param>
        /// <param name="matricola">Matricola dell'utente chiamante</param>
        /// <param name="contentType">ContentType del file richiesto</param>
        /// <param name="isAuthorized">Funzione per verifica abilitazione al contenuto</param>
        /// <returns></returns>
        public ActionResult Get(string filePath, string matricola, Func<string, bool> isAuthorized, string contentType = null)
        {
            string currentMatricola = CommonHelper.GetCurrentUserMatricola();
            if (matricola != currentMatricola && !isAuthorized(currentMatricola))
            {
                return View("~/Views/Shared/NonAbilitatoError2.cshtml");
            }

            if (!System.IO.File.Exists(filePath))
            {
                return View("~/Views/Shared/404.cshtml");
            }

            if (contentType != null)
                return File(filePath, contentType);
            else
                return File(filePath, "application/unknown");
        }

        public ActionResult GetFile(string matricola, string functionName, string fileName)
        {
            string currentMatricola = CommonHelper.GetCurrentUserMatricola();
            if (matricola != currentMatricola)
                return new HttpUnauthorizedResult();

            var result = FileHelper.DownloadFile(matricola, functionName, fileName);
            if (result.Esito)
                return File(result.File.Content, result.File.ContentType);
            else if (result.ErrorType==FileOperError.FileNotFound)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, result.Errore);
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, result.Errore);
        }

        public ActionResult UploadFile(string matricola, string functionName, HttpPostedFileBase postedFile)
        {
            string currentMatricola = CommonHelper.GetCurrentUserMatricola();
            if (matricola != currentMatricola)
                return new HttpUnauthorizedResult();

            var result = FileHelper.UploadFile(matricola, functionName, new FileOperLog(CommonHelper.GetCurrentUserPMatricola(), System.Web.HttpContext.Current.Request.UserHostAddress), postedFile);
            if (result.Esito)
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, result.Errore);
        }
    }
}