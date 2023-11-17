using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public class Competenze
    {
        public List<Competenza> ListaCompetenze { get; set; }
        
    }
    public class Competenza
    {
        public string DescrizioneRuolo { get; set; }
        public string CodiceRuolo { get; set; }
        public string TipoRuolo { get; set; }
        public string CodiceRequisito { get; set; }
        public bool SelectedCodiceCompetenza { get; set; }

    }
}
