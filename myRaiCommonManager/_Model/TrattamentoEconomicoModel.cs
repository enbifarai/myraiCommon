using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiData.Incentivi;

namespace myRaiCommonModel
{
    public class TrattamentoEconomicoIndexModel
    {
        public List<XR_MAT_CATEGORIE> ListaCategorie { get; internal set; }
    }
    public class TrattamentoEconomicoModel
    {
        public TrattamentoEconomicoModel()
        {
            RichiesteInCaricoAme = new TEtabModel();
            RichiesteInCaricoAdAltri = new TEtabModel();
            RichiesteInCaricoANessuno = new TEtabModel();
        }
        public bool IsPreview { get; set; }
        public TEtabModel RichiesteInCaricoAme { get; set; }
        public TEtabModel RichiesteInCaricoANessuno { get; set; }
        public TEtabModel RichiesteInCaricoAdAltri { get; set; }
         
    }
    public class TEtabModel
    {
        public TEtabModel()
        {
            ListaRichieste = new List<TrattamentoEconomicoItem>();
        }
        public List<TrattamentoEconomicoItem> ListaRichieste { get; set; }
        public string index { get; set; }
    }
    public class TrattamentoEconomicoItem
    {
        public XR_MAT_RICHIESTE Richiesta { get; set; }
        public string InCarico { get; set; }
        public bool TaskAvviati { get; set; }
    }
    
    public class SearchTeModel
    {
        public string matr { get; set; }
        public string mese { get; set; }
        public string mesetask { get; set; }
        public string stato { get; set; }
        public string tipo { get; set; }
    }
    public class PopupTeModel
    {
        public XR_MAT_RICHIESTE Richiesta { get; set; }
        public List<AttributiAggiuntivi> ListaAttributiJson { get; set; }
        public bool InCaricoAMe { get; set; }

    }
    public class PopupTeAssegnaModel
    {
        public XR_MAT_RICHIESTE Richiesta { get; set; }
        public bool PossibileAssegnare { get; set; }
        public bool PuoiPrendereIncarico { get; set; }
        public bool PuoiRilasciare { get; set; }
        public List<myRaiHelper.NominativoMatricola> AssegnatariPossibili { get; set; }
        public bool InCaricoACollegaAssente { get; set; }
        public bool InCaricoAMe { get; set; }
        
    }
    
}
