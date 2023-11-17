using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.DocFirmaModels
{
    public class DocumentiDaFirmareModel
    {
        public DocumentiDaFirmareModel()
        {
            Sedi = new List<InfoSedeModel>();
        }

        public List<InfoSedeModel> Sedi { get; set; }
        public MeseAnnoModel Mese { get; set; }
        public GiornateModel Giornate { get; set; }

        public string JsInitialFunction { get; set; }
    }

    public class InfoSedeModel
    {
        public string NomeSede { get; set; }
        public string CodiceSede { get; set; }
        public int TotaliSede { get; set; }
        public Boolean Selezionata { get; set; }
    }
    public class MeseAnnoModel
    {
        public int Anno { get; set; }
        public int Mese { get; set; }
        public string Etichetta { get; set; }
        public string AllowNext { get; set; }
        public string AllowPrevious { get; set; }
    }
    public class GiornateModel
    {
        public GiornataContentModel [] Giornate { get; set; }
    }
    public class GiornataContentModel
    {
        public DateTime data { get; set; }
        
    }
}