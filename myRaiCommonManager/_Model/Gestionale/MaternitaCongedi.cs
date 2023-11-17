using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel.Gestionale
{
    public class MaternitaCongedi
    {
        public class TrattamentoEconomico1
        {
            public string Matricola { get; set; }
            public string num_anno { get; set; }
            public int cod_mese { get; set; }
            public decimal? tot_retrib_annua { get; set; }
            public decimal? retrib_mensile { get; set; }
            public decimal? xiii_mensilita { get; set; }
            public decimal? xiv_mensilita { get; set; }
            public decimal? minimo { get; set; }
            public decimal? premio_produzione { get; set; }
            public decimal? superminimo { get; set; }
			public decimal? superminimo_acc_13032018 { get; set; }
            public string forma_contratto { get; set; }
        }
        public class TrattamentoEconomico2
        {
            public string Matricola { get; set; }
            public string num_anno { get; set; }
            public int cod_mese { get; set; }
            public string cod_indennita { get; set; }
            public string desc_indennita { get; set; }
            public decimal? importo_inden { get; set; }
            public decimal? perc_inden { get; set; }

        }
    }
}
