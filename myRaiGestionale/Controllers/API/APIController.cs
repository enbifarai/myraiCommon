using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers.API
{
    public class FotoController : ApiController
    {
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetImage(string matricola, string check, myRaiCommonTasks.CommonTasks.enumPhotoFormat risoluzione)
        {
            if (string.IsNullOrWhiteSpace(matricola) || string.IsNullOrWhiteSpace(check) || check.Length != 3 || check != CheckDigits(matricola))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(myRaiCommonTasks.CommonTasks.GetFoto(matricola, risoluzione));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                return response;
            }
        }

        private string CheckDigits(string input)
        {
            int t = 0;
            foreach (char c in DateTime.Now.ToString("yyyyMMdd") + input)
            {
                if (int.TryParse(c.ToString(), out int num)) t += num;
            }

            return t.ToString().PadLeft(3, '0');
        }

    }
}
