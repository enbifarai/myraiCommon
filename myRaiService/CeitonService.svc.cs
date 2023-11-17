using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace myRaiService
{
    [ServiceLoggingBehavior()]
    public class CeitonService : ICeitonService
    {
        public UpdateWOIDresponse UpdateWorkOrderID(string woid, bool success, int errorNumber, string errorDescription, string keyString)
        {
            
            var db = new myRaiData.digiGappEntities();
             
            //autenticazione key
            if ( ! wApiUtility.KeyStringVerified(keyString) )
            {
                myRaiData.MyRai_LogAzioni azk = wApiUtility.getAzione("wrong keystring:"+keyString, "UpdateWorkOrderID Request", "UpdateWorkOrderID");
                db.MyRai_LogAzioni.Add(azk);
                db.SaveChanges();
                return new UpdateWOIDresponse() { esito = false, error = "keystring non valido"  };
            }
            
            //log richiesta
            myRaiData.MyRai_LogAzioni az = wApiUtility.getAzione(woid+"/"+success+"/"+errorNumber+"/"+errorDescription, "UpdateWorkOrderID Request", "UpdateWorkOrderID");
            db.MyRai_LogAzioni.Add(az);
            db.SaveChanges();


            //ricerca woid
            var item = db.MyRai_CeitonLog.Where(x => x.guid == woid).FirstOrDefault();
            if (item == null)
            {
                myRaiData.MyRai_LogAzioni azNotFound = wApiUtility.getAzione(woid, "UpdateWorkOrderID Response-WOID NON TROVATO", "UpdateWorkOrderID");
                db.MyRai_LogAzioni.Add(azNotFound);
                db.SaveChanges();

                return new UpdateWOIDresponse() { esito = false, error = "workorder id non trovato: " + woid };
            }
            
            //update woid
            item.data_update = DateTime.Now;
            item.esito = success;
            item.error_code = errorNumber.ToString();
            item.errore = errorDescription;
            item.ultimo_invio_da = "CEITON_WCF";

            try
            {
                db.SaveChanges();
                return new UpdateWOIDresponse() { esito = true, error = null };
            }
            catch (Exception ex)
            {
                myRaiData.MyRai_LogErrori err = new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "CEITON_WCF",
                    data = DateTime.Now,
                    error_message = woid + " - " +ex.ToString(),
                    provenienza = "UpdateWorkOrderID",
                    matricola = ""
                };
                db.MyRai_LogErrori.Add(err);
                db.SaveChanges();

                return new UpdateWOIDresponse() { esito = false, error = "DB error ("+err.Id+")" };
            }
        }
    }
    public class UpdateWOIDresponse
    {
        public bool esito { get; set; }
        public string error { get; set; }
    }
}