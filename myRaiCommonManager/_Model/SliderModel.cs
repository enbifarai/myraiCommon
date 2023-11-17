using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class SliderModel
    {
        public SliderModel (int p, string dalle, string alle,bool selectlocked=false, bool allLocked=false )
        {
            this.prog = p;
            this.dalle = dalle;
            this.alle = alle;
            int dallemin = CommonHelper.calcolaMinuti(dalle);
            int allemin = CommonHelper.calcolaMinuti(alle);
            this.max = allemin - dallemin;
            this.value = this.max;
            this.min = 0;
            this.Selectlocked = selectlocked;
            this.SelectAndSliderLocked = allLocked;
        }
        public int prog { get; set; }
        public string dalle { get; set; }
        public string alle { get; set; }
        public int min { get; set; }
        public int max { get; set; }
        public int value { get; set; }
        public bool Selectlocked { get; set; }
        public bool SelectAndSliderLocked { get; set; }
    }
}