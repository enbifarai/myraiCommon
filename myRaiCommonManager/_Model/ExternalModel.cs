using myRaiCommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRai.Models
{
    public class ExternalModel
    {
        public InfoGiornataModel InfoGiornata { get; set; }

        public ModelDash InfoTimbrature { get; set; }

        public ModelDash InfoSegnalazioni { get; set; }
    }
}