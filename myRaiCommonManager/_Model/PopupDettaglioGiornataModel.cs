using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class PopupDettaglioGiornata
    {
        public string Nominativo { get; set; }
        public string Matricola { get; set; }
        public string DescrizioneSedeGapp { get; set; }
        public string CodiceSedeGapp { get; set; }
        public string TipoRichiesta { get; set; } //ferie,straord,...
        public DateTime DataEccezione { get; set; }
        public DateTime? DataDalle { get; set; }
        public DateTime? DataAlle { get; set; }
        public string MotivoRichiesta { get; set; }

        public int IdRichiestaEccezione { get; set; }
        public int  IdStatoRichiesta { get; set; }
        public string ImmagineBase64 { get; set; }
        public EnumPresenzaDip InServizio { get; set; }
        
        public Boolean ShowTimbrature { get; set; }

        public String PeriodoPiuGiorni { get; set; }

        public string ApprovataDa { get; set; }

        public string EccezioneDaStornareApprovataDa { get; set; }
        public DateTime? EccezioneDaStornareDataValidazione { get; set; }

        public Dictionary<string, string> ParametriExtra { get; set; }

        public List<ParametroRichiesto> ParametriRichiesta { get; set; }

		public string NoteSegreteria { get; set; }
        public EccezioneSostitutivaInfo EccSost { get; set; }
       
    }
    public class ParametroRichiesto
    {
        public string NomeParametro { get; set; }
        public string ValoreParametro { get; set; }
    }
    public class EccezioneSostitutivaInfo
    {
        public string EccezioneSostitutivaCodice { get; set; }
        public string EccezioneSostitutivaDalle { get; set; }
        public string EccezioneSostitutivaAlle { get; set; }
        public bool EccezioneSostitutivaSWH { get; set; }
    }
}