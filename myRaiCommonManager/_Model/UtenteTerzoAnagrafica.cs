using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyRaiServiceInterface.MyRaiServiceReference1;

namespace myRaiCommonModel
{
    public class UtenteTerzoAnagrafica
    {
        public string _matricola { get; set; }
        public string _cognome { get; set; }
        public string _nome { get; set; }
        public string _foto { get; set; }
        public string _contratto { get; set; }
        public DateTime? _dataAssunzione { get; set; }
        public string _figProfessionale { get; set; }
        public string _qualifica { get; set; }
        public DateTime? _dataNascita { get; set; }
        public string _comuneNascita { get; set; }
        public string _statoNascita { get; set; }
        public string _inquadramento { get; set; }
        public string _logo { get; set; }
        //campi aggiuntivi
        public string _codiceFigProf { get; set; }
        public string _codiceContratto { get; set; }
        public string _codiceQualifica { get; set; }
        public string _email { get; set; }
        public string _telefono { get; set; }        
    }
}