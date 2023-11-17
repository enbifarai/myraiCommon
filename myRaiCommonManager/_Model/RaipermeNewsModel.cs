using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class RaipermeNewsModel
    {
        public RaipermeNewsModel()
        {
            NewsItems = new List<myRaiData.MyRai_News>();
        }
        public List<myRaiData.MyRai_News> NewsItems { get; set; }

    }
}