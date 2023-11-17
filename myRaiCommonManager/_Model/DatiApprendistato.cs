using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public class DatiApprendistato : _IdentityData
    {
        public int IdJobAssign { get; set; }
        public string ProfiloFormativo { get; set; }
        public IList<SelectListItem> TipologiaApprendistati { get; set; }
        public IList<string> SelectedTipologiaApprendistato { get; set; }
        public string TipologiaApprendistato { get; set; }
        public string DurataApprendistato { get; set; }
        public List<Competenza> Competenze { get; set; }
        public PianoFormativo_Pianificato Profilo { get; set; }
        public DatiApprendistato()
        {
            TipologiaApprendistati = new List<SelectListItem>();
            SelectedTipologiaApprendistato = new List<string>();

        }
    }

}
