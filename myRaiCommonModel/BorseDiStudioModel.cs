using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class Index_BorseDiStudio
    {
        public StoricoRichieste_BorseDiStudio Storico { get; set; }
        public RiepilogoRichieste_BorseDiStudio Riepilogo { get; set; }

        public Index_BorseDiStudio()
        {
            Storico = new StoricoRichieste_BorseDiStudio();
            Riepilogo = new RiepilogoRichieste_BorseDiStudio();
        }
    }

    public class StoricoRichieste_BorseDiStudio
    {
        public List<Richiesta_BorseDiStudio> RichiesteBorseDiStudio { get; set; }
    }

    public class Richiesta_BorseDiStudio
    {
        public string Nominativo { get; set; }
        public string CodFiscale { get; set; }
        public string DescrStatus { get; set; }
        public string CodeDocumento { get; set; }
        public int? AnnoScolastico { get; set; }
        public int RankIstituto { get; set; }
        public int RankClasseFreq { get; set; }
        public string CodeIstituto { get; set; }
        public string DescrIstituto { get; set; }
        public DateTime? DataRichiesta { get; set; }
        public DateTime? DataApprovata { get; set; }
        public DateTime? DataContabile { get; set; }
        public DateTime? DataDiStampa { get; set; }
        public Decimal? Importo { get; set; }
        public bool HaAllegatoTipo1 { get; set; }
        public bool HaAllegatoTipo2 { get; set; }
    }

    public class RiepilogoRichieste_BorseDiStudio
    {
        public bool PuoEffettureNuovaRichiesta { get; set; }
        public int? NumeroRichieste { get; set; }

        public RiepilogoRichieste_BorseDiStudio()
        {
            PuoEffettureNuovaRichiesta = false;
            NumeroRichieste = null;
        }
    }

    public class NuovaRichiesta_BorseDiStudio
    {
        [Required(ErrorMessage = "Selezione obbligatoria")]
        [Range(2017, 2100, ErrorMessage = "Selezione non valida")]
        public string AnnoScolasticoRichiesta { get; set; }

        [Required(ErrorMessage = "Selezione obbligatoria")]
        [StringLength(1, ErrorMessage = "Selezione non valida")]
        public string IstitutoRichiesta { get; set; }

        [Required(ErrorMessage = "Selezione obbligatoria")]
        [StringLength(16, ErrorMessage = "Selezione non valida")]
        public string CodFiscaleRichiedente { get; set; }

        public string NumeroTelefono { get; set; }
                
        public List<string> AnniScolasticiSelezionabili { get; set; }
        public List<Familiare_BorseDiStudio> FamiliariSelezionabili { get; set; }
        public List<string> IstitutiSelezionabili { get; set; }
        
        public NuovaRichiesta_BorseDiStudio()
        {
            AnniScolasticiSelezionabili = new List<string>();
            FamiliariSelezionabili = new List<Familiare_BorseDiStudio>();
            IstitutiSelezionabili = new List<string>();
        }
    }

    public class Familiare_BorseDiStudio
    {
        public string Nominativo { get; set; }
        public string CodFiscale { get; set; }
        public int PercentualeACarico { get; set; }
        public int Progressivo { get; set; }
        public DateTime DataNascita { get; set; }
    }
}