using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.raiplace
{
    public class TotalAbbonamnetiModel
    {
        public List<CampagnaAbbonamentiModel> ListCampagnaAbbonamenti;
        public MyAbbonamenti Abbonamenti;

        public bool EnabledDelete { get; set; }

    }
}