using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace myRaiCommonModel.raiplace
{
    public class Omaggio
    {
        public int Id { get; set; }
        public bool UtenteEsterno { get; set; }
        public B2RaiPlace_RegistroOmaggi_Anagrafica Utente { get; set; }
        public string Data_Ricezione { get; set; }
        public int Tipo_id { get; set; }
        public SelectListItem Tipo { get; set; }
        public int Motivo_id { get; set; }
        public SelectListItem Motivo { get; set; }
        public string Mittente { get; set; }
        public string Descrizione { get; set; }
        public string Valore { get; set; }
        public bool Flag_UfficioSpedizioni { get; set; }
        public string UfficioSpedizioni { get; set; }
        public string Ente_Beneficiario { get; set; }
        public string Note { get; set; }
        public bool Flag_validita { get; set; }
        public DateTime Data_Inserimento { get; set; }
        public string Mail_Responsabile { get; set; }
        public bool Flag_Accetto { get; set; }
        public string Note_Accetto { get; set; }
        public string Note_Altro { get; set; }
        public bool VediTutti { get; set; }
    }
}