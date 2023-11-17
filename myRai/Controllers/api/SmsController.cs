using myRai.Business;
using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace myRai.Controllers.api
{
    public class SmsController : ApiController
    {
        public BaseSMSResponse ReceivedSMS(string PHONE, string smsTEXT, string smsDATE)
        {
            bool incoming = true;
            bool esito = true;
            BaseSMSResponse result = ManageSMSApi(PHONE, smsTEXT, smsDATE, incoming, esito);

            return result;
        }
        
        public BaseSMSResponse SentSMS(string PHONE, string smsTEXT, string smsDATE)
        {
            bool incoming = false;
            bool esito = true;
            BaseSMSResponse result = ManageSMSApi(PHONE, smsTEXT, smsDATE, incoming, esito);
            return result;
        }

        public BaseSMSResponse FailedSMS(string PHONE, string smsTEXT, string smsDATE)
        {
            bool incoming = false;
            bool esito = false;
            BaseSMSResponse result = ManageSMSApi(PHONE, smsTEXT, smsDATE, incoming, esito);
            return result;
        }

        private static BaseSMSResponse ManageSMSApi(string PHONE, string smsTEXT, string smsDATE, bool incoming, bool esito)
        {
            BaseSMSResponse result = new BaseSMSResponse();
            result.Esito = false;

            //Nel caso sia un failed, gestiamo anche l'assenza del mittente
            if (esito && String.IsNullOrWhiteSpace(PHONE))
            {
                result.Message = "Numero di telefono non indicato";
            }
            else if (String.IsNullOrWhiteSpace(smsDATE))
            {
                result.Message = "Data sms non indicata";
            }
            else
            {
                var db = new IncentiviEntities();
                XR_SMS_INBOX rec = new XR_SMS_INBOX()
                {
                    COD_PHONENUMBER = PHONE,
                    NOT_TEXT = smsTEXT,
                    IND_ESITO = esito,
                    IND_INCOMING = incoming,
                    COD_DTA_DATE = smsDATE
                };
                db.XR_SMS_INBOX.Add(rec);

                try
                {
                    db.SaveChanges();
                    result.Esito = true;
                }
                catch (Exception ex)
                {
                    result.Esito = false;
                    result.Message = ex.Message;
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        applicativo = "SmsApi",
                        data = DateTime.Now,
                        error_message = ex.ToString(),
                        matricola = "API",
                        provenienza = !esito ? "SmsFailed" : incoming ? "SmsReceived" : "SmsSent"
                    }, "API");
                }
            }

            return result;
        }
    }

    public class BaseSMSResponse
    {
        public bool Esito { get; set; }
        public string Message { get; set; }
    }
}

