using myRaiCommonManager;
using myRaiHelper;
using System;
using System.Web.Http;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRai.Controllers.api
{
    //Controllo Accesso API +++++++++++++++++++++++++++++++++++++++++
    [PasswordRichiesta]
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    public class BatchController : ApiController
    {
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.HttpPost]
        public string InsEcc( [FromBody]InserimentoEccezioneModel Model = null)
        {
            try
            {
                Model.IsFromBatch = true;
                
                EsitoInserimentoEccezione esito = EccezioniManager.Inserimento(Model);
                return esito.response;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}