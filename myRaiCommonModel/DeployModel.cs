using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class DeployModel
    {
        public DeployModel()
        {
            ListaFileDaPubblicare = new List<myRaiData.MyRai_Pubblicazioni>();
        }
        public List<myRaiData.MyRai_Pubblicazioni> ListaFileDaPubblicare { get; set; }
        public bool IsRollBack { get; set; }
    }
}