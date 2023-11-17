using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{

    public class ProfiloDatiMenuModel
    {
        public ProfiloDatiMenuModel()
        {
            this.vociMenu = new List<ProfiloVoceMenu>();
        }
        public List<ProfiloVoceMenu> vociMenu { get; set; }
    }
    public class ProfiloVoceMenu
    {
        public myRaiData.MyRai_Voci_Menu voceMenu { get; set; }
        public bool Selected { get; set; }
    }

}