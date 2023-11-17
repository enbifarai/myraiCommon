using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class limitaTRmodel
    {
        public limitaTRmodel(string tabella,string div,int rows,int elementsInList)  
        {
            this.tabella=tabella;
            this.div=div;
            this.rows = rows;
            this.elementsInList = elementsInList;
        }
        public string tabella { get; set; }
        public string div { get; set; }
        public int rows { get; set; }
        public int elementsInList { get; set; }
    }
}