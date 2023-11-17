using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonManager._Model
{
    public class Congedo
    {/*(<ID_CONGEDO, INTEGER,>
           ,<MATRICOLA, char,>
           ,<DATA_ECCEZIONE, DECIMAL,>
           ,<ECCEZIONE, char,>
           ,<CODICE_FISCALE, char,>
           ,<DATA_NASCITA, DECIMAL,>
           ,<DATA_INSERIMENTO, DECIMAL,>
           ,<DATA_INIZIO, DECIMAL,>
           ,<DATA_FINE, DECIMAL,>
           ,<CTR_NON_TRASFERIBILE, DECIMAL,>
           ,<CTR_TRASFERIBILE, DECIMAL,>
           ,<CODICE_INPS, char,>
           ,<DATA_LOG, DECIMAL,>)*/
        public int Id { get; set; }
        public string Matricola { get; set; }
        public string CF { get; set; }
        public DateTime Data_eccezione { get; set; }
        public string Eccezione { get; set; }
        public DateTime? Data_nascita { get; set; }
        public DateTime Data_inserimento { get; set; }
        public DateTime? Data_inizio { get; set; }
        public DateTime? Data_fine { get; set; }
        public decimal Ctr_non_trasferibile { get; set; }
        public decimal Ctr_trasferibile { get; set; }
        public string Codice_inps { get; set; }
        public DateTime Data_log { get; set; }
        public int? IdHRIS { get; set; }
        public int? IdSELF { get; set; }
    }
    public class EsitoCongedoDB2
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public int Rows { get; set; }
        public string Query { get; set; }
        public string InsDel { get; set; }
        public int IDinserito { get; set; }
    }
}
