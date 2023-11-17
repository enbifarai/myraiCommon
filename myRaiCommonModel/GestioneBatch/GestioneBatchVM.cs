using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.GestioneBatch
{
    public class GestioneBatchVM
    {
        public List<ReportOnDemandItem> ReportOnDemandItems { get; set; }

        [DisplayAttribute( Name = "Elaborazioni in corso...." )]
        public int ElaborazioniInCorso { get; set; }
    }

    public class ReportOnDemandItem
    {
        [DisplayAttribute( Name = "Titolo" )]
        public string Titolo { get; set; }

        [DisplayAttribute( Name = "Descrizione" )]
        public string Descrizione { get; set; }

        [DisplayAttribute( Name = "Stato attivazione" )]
        public bool Attivo { get; set; }

        [DisplayAttribute( Name = "In esecuzione" )]
        public bool GiaInEsecuzione { get; set; }

        [DisplayAttribute( Name = "Avvio immediato" )]
        public bool AvvioImmediato { get; set; }
    }


    public class ReportOnDemandJsonResult
    {
        public bool Result { get; set; }
        public string Error { get; set; }
        public int ElaborazioniInCorso { get; set; }
    }
}