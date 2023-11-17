using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace myRaiData
{

    #region XML PER HRDOC
    [Serializable]
    [XmlRoot(ElementName = "COLLOCAZIONE")]
    public class COLLOCAZIONE
    {
        [XmlAttribute(AttributeName = "valore")]
        public string Valore { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "PROTOCOLLO")]
    public class PROTOCOLLO
    {
        [XmlAttribute(AttributeName = "valore")]
        public string Valore { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "EMITTENTE")]
    public class EMITTENTE
    {
        [XmlAttribute(AttributeName = "valore")]
        public string Valore { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "PAROLA_CHIAVE")]
    public class PAROLA_CHIAVE
    {
        [XmlAttribute(AttributeName = "valore")]
        public string Valore { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "TESTO")]
    public class TESTO
    {
        [XmlAttribute(AttributeName = "valore")]
        public string Valore { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "DATADOC")]
    public class DATADOC
    {
        [XmlAttribute(AttributeName = "valore")]
        public string Valore { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "CERTIFICATO")]
    public class CERTIFICATO
    {
        public CERTIFICATO()
        {
            this.COLLOCAZIONE = new COLLOCAZIONE();
            this.PROTOCOLLO = new PROTOCOLLO();
            this.EMITTENTE = new EMITTENTE();
            this.PAROLA_CHIAVE = new PAROLA_CHIAVE();
            this.TESTO = new TESTO();
            this.DATADOC = new DATADOC();
        }
        [XmlElement(ElementName = "COLLOCAZIONE")]
        public COLLOCAZIONE COLLOCAZIONE { get; set; }
        [XmlElement(ElementName = "PROTOCOLLO")]
        public PROTOCOLLO PROTOCOLLO { get; set; }
        [XmlElement(ElementName = "EMITTENTE")]
        public EMITTENTE EMITTENTE { get; set; }
        [XmlElement(ElementName = "PAROLA_CHIAVE")]
        public PAROLA_CHIAVE PAROLA_CHIAVE { get; set; }
        [XmlElement(ElementName = "TESTO")]
        public TESTO TESTO { get; set; }
        [XmlElement(ElementName = "DATADOC")]
        public DATADOC DATADOC { get; set; }
    }

    #endregion
}

