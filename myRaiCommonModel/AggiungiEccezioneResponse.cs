using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class AggiungiEccezioneResponse
    {
        public string NumDoc { get; set; }
        public string Error { get; set; }
        public updateResponse response { get; set; }
    }
}