using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace myRaiService
{
    // NOTA: è possibile utilizzare il comando "Rinomina" del menu "Refactoring" per modificare il nome di interfaccia "ICeitonService" nel codice e nel file di configurazione contemporaneamente.
    [ServiceContract]
    public interface ICeitonService
    {
        [OperationContract]
        UpdateWOIDresponse UpdateWorkOrderID(string woid, bool success, int errorNumber, string errorDescription, string keyString);
      
    }
     
}
