using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class GiornataInfoMobile
    {
        public GiornataInfoMobile()
        {
            this.timbrature = new List<timbraturaMobile>();
            this.eccezioni = new List<eccezioneMobile>();
            this.note = new List<NotaSegreteriaMobile>();
        }
        public string orarioreale { get; set; }
        public string orarioprevisto { get; set; }
        public string orariorealedesc { get; set; }
        public string orarioprevistodesc { get; set; }
        public string intervallomensa { get; set; }
        public string carenza { get; set; }
        public string mp { get; set; }
        public string mensafruita { get; set; }
        public List<timbraturaMobile> timbrature { get; set; }
        public List<eccezioneMobile> eccezioni { get; set; }
        public string MensaSede { get; set; }

        public List<NotaSegreteriaMobile> note { get; set; }

    }
    public class eccezioneMobile
    {
        public string codiceEccezione { get; set; }
        public string descEccezione { get; set; }
        public string quantita { get; set; }
        public string dalle { get; set; }
        public string alle { get; set; }
    }
    public class timbraturaMobile
    {
        public string orarioIn { get; set; }
        public string insediamentoIn { get; set; }
        public string insediamentoDescIn { get; set; }
        public string orarioOut { get; set; }
        public string insediamentoOut { get; set; }
        public string insediamentoDescOut { get; set; }
    }
    public class NotaSegreteriaMobile
    {
        public string datacreazione { get; set; }
        public string datagiornata { get; set; }
        public string matricolaMittente { get; set; }
        public string nominativoMittente { get; set; }
        public string messaggio { get; set; }
        public string sede { get; set; }
    }

    public class MobileResponse
    {
        public MobileResponse()
        {
            this.esito = true;
        }
        public bool esito { get; set; }
        public string error { get; set; }

        public string AnagraficaCSV { get; set; }
        public string ImageBase64 { get; set; }
        public bool IsSeg { get; set; }
        public bool IsUffPers { get; set; }
    }
    public class NoteSegreteriaResponse
    {
        public NoteSegreteriaResponse()
        {
            this.note = new List<NotaSegreteria>();
        }
        public bool esito { get; set; }
        public string error { get; set; }
        public List<NotaSegreteria> note { get; set; }
        public class NotaSegreteria
        {
            public string datacreazione { get; set; }
            public string datagiornata { get; set; }
            public string matricola { get; set; }
            public string nominativo { get; set; }
            public string messaggio { get; set; }
            public string sede { get; set; }
            public string datalettura { get; set; }
            public int id { get; set; }
            public string destinatario { get; set; }
            public bool miodioggi { get; set; }
            public int idnotainiziale { get; set; }
        }
    }
    public class RegistrationInfo
    {
        public bool HasFingerPrint;
        public string phonenbr { get; set; }
        public string deviceID { get; set; }
        public string devicemodel { get; set; }
        public string manufacturer { get; set; }
        public string deviceName { get; set; }
        public string version { get; set; }
        public string platform { get; set; }
        public string idiom { get; set; }
        public string deviceType { get; set; }
        public string token { get; set; }

        public string pmatricola { get; set; }

        public bool needAnagrafica { get; set; }

    }
}
