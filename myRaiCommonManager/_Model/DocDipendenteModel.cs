using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public class DocDipendenteModel
    {
        public DocDipendenteModel()
        {
            this.Documenti = new List<DocDipendente>();
        }
        public List<DocDipendente> Documenti { get; set; }
        public string err { get; set; }
        public SelectList listaTipologie { get; set; }
        public int idTipologia { get; set; }
    }
    public class DocDipendente
    {
        public string nomefile { get; set; }
        public string descrizione { get; set; }
        public DateTime datainserito { get; set; }
        public int id { get; set; }
        public string tipologia { get; set; }
        public myRaiData.MyRai_DocumentiDipendente docDB { get; set; }
    }

}