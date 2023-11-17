using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class OnBoardingModel
    {
        public List<myRaiData.MyRai_OnBoarding> ItemList { get; set; }
        public myRaiData.MyRai_OnBoarding ItemSelected { get; set; }
        public int idNextItem { get; set; }
        public int idPrevItem { get; set; }
        
    }
}