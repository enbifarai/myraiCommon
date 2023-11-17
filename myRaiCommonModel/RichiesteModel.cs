using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class RichContainer
    {
        public RichiestaLoader Loader { get; set; }
        public List<RichiestaAnag> Richieste { get; set; }
    }

    public class RichiestaLoader
    {
        public RichiestaLoader()
        {
            Tipologie = new List<TipoRichiestaAnag>();
        }
        public List<TipoRichiestaAnag> Tipologie { get; set; }
        public string Matricola { get; set; }

        public string Nominativo { get; set; }
    }

    public class RichiestaAnag : _IdentityData
    {
        public int IdRichiesta { get; set; }
        public TipoRichiestaAnag Tipologia { get; set; }
        public DateTime DataRichiesta { get; set; }
        public string Descrizione { get; set; }
        public string Note { get; set; }
        public object ObjInfo { get; set; }
        public DateTime? DataMemo { get; set; }
        public DateTime? DataScadenza { get; set; }
        public bool HasError { get; set; }
        public string ErrorMsg { get; set; }
        public DEM_TIPI_DOCUMENTO_ENUM TipologiaDoc { get; set; }
    }
}
