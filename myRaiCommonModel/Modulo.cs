using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public enum TipoElemento
    {
        Testo = 0,
        CheckBox = 1,
        Radio = 2
    }

    public class Modulo_Elemento
    {
        public TipoElemento Tipo { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
        public string Text { get; set; }
        public string Tooltip { get; set; }
        public string GroupName { get; set; }
        public string Style { get; set; }
    }

    public enum ModuloSmart2020SelectionEnumOLD
    {
        [Description( "Lavoratore disabile" )]
        [AmbientValue( "lavoratore disabile in condizione di gravita' accertata, ai sensi dell'art. 3, comma 3, L. n. 104/1992;" )]
        LavoratoreDisabile = 1,
        [Description( "Familiare disabile" )]
        [AmbientValue( "lavoratore che abbia nel proprio \"nucleo familiare\" un soggetto disabile in condizione di gravita' accertata, ai sensi dell'art. 3, comma 3, L. n. 104/1992;" )]
        FamiliareDisabile = 2,
        [Description( "Lavoratore immunodepresso" )]
        [AmbientValue( "lavoratore immunodepresso;" )]
        LavoratoreImmunodepresso = 3,
        [Description( "Familiare immunodepresso" )]
        [AmbientValue( "lavoratore che convive con \"familiare\"  immunodepresso;" )]
        FamiliareImmunodepresso = 4,
        [Description( "Genitore" )]
        [AmbientValue( "genitore di figlio di eta' non superiore a 14 anni (fino a 13 anni e 364 giorni). A tal fine, dichiaro che l'altro genitore non e' beneficiario di strumenti a sostegno del reddito per sospensione o cessazione di attivita' (es. Cassa integrazione e tutele assimilate, indennita' di disoccupazione, DIS/COLL, ecc.), disoccupato od in condizione di non occupazione (es. casalinga/o)." )]
        Genitore = 5
    }

    public enum ModuloSmart2020SelectionEnum
    {
        [Description("lavoratore in condizione di grave disabilita', accertata ai sensi dell'art. 3, comma 3, L. n. 104/1992;")]
        Scelta7 = 7,

        [Description("lavoratore in possesso di una certificazione, rilasciata dai competenti organi medico – legali della ASL di riferimento, attestante una situazione di rischio derivante da immunodepressione, esiti da patologie oncologiche o dallo svolgimento di relative terapie salvavita;")]
        Scelta8 = 8,

        [Description("genitore di figlio di eta' non superiore a 16 anni(fino a 15 anni e 364 giorni), interessato da una sospensione dell'attivita' didattica in presenza dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta9 = 9,
        [Description("disposizioni governative;")]
        Scelta9_1 = 91,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta9_2 = 92,
        [Description("determinazione della scuola.")]
        Scelta9_3 = 93,
        [Description("che l'altro genitore non svolge, nello stesso periodo, la propria attivita' lavorativa in regime di smart working, salvo che lo smart working sia stato richiesto in relazione ad una situazione di fragilita' del genitore medesimo o per assistere altro figlio in condizione di disabilita', accertata ai sensi dell'art. 3, commi 1 o 3, della legge n. 104/1992 ovvero con disturbi specifici dell'apprendimento(DSA) ovvero con bisogni educativi speciali (BES);")]
        Scelta9_4 = 94,
        [Description("di essere genitore solo.")]
        Scelta9_6 = 96,

        [Description("genitore di figlio di eta' non superiore a 16 anni(fino a 15 anni e 364 giorni), interessato da una sospensione dell'attivita' educativa in presenza dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta10 = 100,
        [Description("disposizioni governative;")]
        Scelta10_1 = 101,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta10_2 = 102,
        [Description("determinazione della scuola.")]
        Scelta10_3 = 103,
        [Description("che l'altro genitore non svolge, nello stesso periodo, la propria attivita' lavorativa in regime di smart working, salvo che lo smart working sia stato richiesto in relazione ad una situazione di fragilita' del genitore medesimo o per assistere altro figlio in condizione di disabilita', accertata ai sensi dell'art. 3, commi 1 o 3, della legge n. 104/1992 ovvero con disturbi specifici dell'apprendimento(DSA) ovvero con bisogni educativi speciali (BES);")]
        Scelta10_4 = 104,
        [Description("di essere genitore solo.")]
        Scelta10_6 = 106,

        [Description("genitore di figlio di eta' non superiore a 16 anni (fino a 15 anni e 364 giorni), sottoposto alla misura precauzionale della quarantena, disposta dalla ASL territorialmente competente ovvero risultante dalla certificazione rilasciata dal pediatra/dal medico di medicina generale, per il periodo dal #DATADAL# al #DATAAL# .")]
        Scelta11 = 110,
        [Description("che l'altro genitore non svolge, nello stesso periodo, la propria attivita' lavorativa in regime di smart working, salvo che lo smart working sia stato richiesto in relazione ad una situazione di fragilita' del genitore medesimo o per assistere altro figlio in condizione di disabilita', accertata ai sensi dell'art. 3, commi 1 o 3, della legge n. 104/1992 ovvero con disturbi specifici dell'apprendimento(DSA) ovvero con bisogni educativi speciali (BES);")]
        Scelta11_1 = 111,
        [Description("di essere genitore solo.")]
        Scelta11_3 = 113,

        [Description("genitore di figlio di eta' non superiore a 16 anni (fino a 15 anni e 364 giorni), affetto da infezione da COVID–19, accertata in data #DATADAL# ")]
        Scelta12 = 120,
        [Description("che l'altro genitore non svolge, nello stesso periodo, la propria attivita' lavorativa in regime di smart working, salvo che lo smart working sia stato richiesto in relazione ad una situazione di fragilita' del genitore medesimo o per assistere altro figlio in condizione di disabilita', accertata ai sensi dell'art. 3, commi 1 o 3, della legge n. 104/1992 ovvero con disturbi specifici dell'apprendimento(DSA) ovvero con bisogni educativi speciali (BES);")]
        Scelta12_1 = 121,
        [Description("di essere genitore solo.")]
        Scelta12_3 = 123,

        [Description("genitore di figlio in condizione di disabilita' non grave, accertata ai sensi dell'art. 3 comma 1, della legge n 104/1992 interessato da una sospensione dell'attivita' didattica in presenza dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta13 = 130,
        [Description("disposizioni governative;")]
        Scelta13_1 = 131,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta13_2 = 132,
        [Description("determinazione della scuola.")]
        Scelta13_3 = 133,

        [Description("genitore di figlio in condizione di disabilita' non grave, accertata ai sensi dell'art. 3 comma 1, della legge n 104/1992 interessato da una sospensione dell'attivita' educativa in presenza dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta14 = 140,
        [Description("disposizioni governative;")]
        Scelta14_1 = 141,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta14_2 = 142,
        [Description("determinazione della scuola.")]
        Scelta14_3 = 143,


        [Description("genitore di figlio in condizione di disabilita' non grave, accertata ai sensi dell'art. 3 comma 1, della legge n 104/1992 interessato dalla chiusura del centro diurno a carattere assistenziale frequentato dal proprio figlio, prevista dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta15 = 150,
        [Description("disposizioni governative;")]
        Scelta15_1 = 151,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta15_2 = 152,
        [Description("determinazione della scuola.")]
        Scelta15_3 = 153,

        [Description("genitore di figlio in condizione di disabilita' non grave, accertata ai sensi dell'art. 3 comma 1, della legge n 104/1992 interessato dalla misura precauzionale della quarantena, disposta dalla ASL territoriale competente ovvero risultante dalla certificazione rilasciata dal pediatra/medico di medicina generale, per il periodo dal #DATADAL# al #DATAAL# .")]
        Scelta16 = 160,

        [Description("genitore di figlio in condizione di disabilita' non grave, accertata ai sensi dell'art. 3 comma 1, della legge n 104/1992 interessato dalla infezione da COVID–19, accertata in data #DATADAL#")]
        Scelta17 = 170,

        [Description("genitore di figlio in condizione di grave disabilita', accertata ai sensi dell'art. 3 comma 3, della legge n 104/1992 interessato da una sospensione dell'attivita' didattica in presenza dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta18 = 180,
        [Description("disposizioni governative;")]
        Scelta18_1 = 181,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta18_2 = 182,
        [Description("determinazione della scuola.")]
        Scelta18_3 = 183,

        [Description("genitore di figlio in condizione di grave disabilita', accertata ai sensi dell'art. 3 comma 3, della legge n 104/1992 interessato da una sospensione dell'attivita' educativa in presenza dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta19 = 190,
        [Description("disposizioni governative;")]
        Scelta19_1 = 191,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta19_2 = 192,
        [Description("determinazione della scuola.")]
        Scelta19_3 = 193,

        [Description("genitore di figlio in condizione di grave disabilita', accertata ai sensi dell'art. 3 comma 3, della legge n 104/1992 interessato dalla chiusura del centro diurno a carattere assistenziale frequentato dal proprio figlio, prevista dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta20 = 200,
        [Description("disposizioni governative;")]
        Scelta20_1 = 201,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta20_2 = 202,
        [Description("determinazione della scuola.")]
        Scelta20_3 = 203,

        [Description("genitore di figlio in condizione di grave disabilita', accertata ai sensi dell'art. 3 comma 3, della legge n 104/1992 interessato dalla misura precauzionale della quarantena, disposta dalla ASL territoriale competente ovvero risultante dalla certificazione rilasciata dal pediatra/medico di medicina generale, per il periodo dal #DATADAL# al #DATAAL# .")]
        Scelta21 = 210,

        [Description("genitore di figlio in condizione di grave disabilita', accertata ai sensi dell'art. 3 comma 3, della legge n 104/1992 interessato dalla infezione da COVID–19, accertata in data #DATADAL# .")]
        Scelta22 = 220,

        [Description("genitore di figlio con disturbi specifici dell'apprendimento ai sensi della legge n. 170/2010 (DSA) interessato da una sospensione dell'attivita' didattica in presenza dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta23 = 230,
        [Description("disposizioni governative;")]
        Scelta23_1 = 231,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta23_2 = 232,
        [Description("determinazione della scuola.")]
        Scelta23_3 = 233,

        [Description("genitore di figlio con disturbi specifici dell'apprendimento ai sensi della legge n. 170/2010 (DSA) interessato dalla chiusura del centro diurno a carattere assistenziale frequentato dal proprio figlio, prevista dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta24 = 240,
        [Description("disposizioni governative;")]
        Scelta24_1 = 241,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta24_2 = 242,
        [Description("determinazione della scuola.")]
        Scelta24_3 = 243,

        [Description("genitore di figlio con disturbi specifici dell'apprendimento ai sensi della legge n. 170/2010 (DSA) interessato dalla misura precauzionale della quarantena, disposta dalla ASL territoriale competente ovvero risultante dalla certificazione rilasciata dal pediatra/medico di medicina generale, per il periodo dal #DATADAL# al #DATAAL# .")]
        Scelta25 = 250,

        [Description("genitore di figlio con disturbi specifici dell'apprendimento ai sensi della legge n. 170/2010 (DSA) interessato dalla infezione da COVID–19, accertata in data #DATADAL# .")]
        Scelta26 = 260,

        [Description("genitore di figlio con bisogni educativi speciali (BES) secondo la direttiva del Ministero dell'Istruzione, dell'Universita' e della Ricerca (MIUR) del 27 dicembre 2012 interessato da una sospensione dell'attivita' didattica in presenza dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta27 = 270,
        [Description("disposizioni governative;")]
        Scelta27_1 = 271,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta27_2 = 272,
        [Description("determinazione della scuola.")]
        Scelta27_3 = 273,

        [Description("genitore di figlio con bisogni educativi speciali (BES) secondo la direttiva del Ministero dell'Istruzione, dell'Universita' e della Ricerca (MIUR) del 27 dicembre 2012 interessato dalla chiusura del centro diurno a carattere assistenziale frequentato dal proprio figlio, prevista dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta28 = 280,
        [Description("disposizioni governative;")]
        Scelta28_1 = 281,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta28_2 = 282,
        [Description("determinazione della scuola.")]
        Scelta28_3 = 283,

        [Description("genitore di figlio con bisogni educativi speciali (BES) secondo la direttiva del Ministero dell'Istruzione, dell'Universita' e della Ricerca (MIUR) del 27 dicembre 2012 interessato dalla misura precauzionale della quarantena, disposta dalla ASL territoriale competente ovvero risultante dalla certificazione rilasciata dal pediatra/medico di medicina generale, per il periodo dal #DATADAL# al #DATAAL#.")]
        Scelta29 = 290,

        [Description("genitore di figlio con bisogni educativi speciali (BES) secondo la direttiva del Ministero dell'Istruzione, dell'Universita' e della Ricerca (MIUR) del 27 dicembre 2012 interessato dalla infezione da COVID–19, accertata in data #DATADAL#.")]
        Scelta30 = 300,

        [Description("genitore di figlio in condizione di grave disabilita', accertata ai sensi dell'art. 3, comma 3, della legge n. 104/199243. Dichiaro, altresi', che nel nucleo familiare l'altro genitore non e' un \"non lavoratore\".")]
        Scelta31 = 310,

        [Description("genitore di figlio con bisogni educativi speciali (BES) secondo la direttiva del Ministero dell'Istruzione, dell'Universita' e della Ricerca (MIUR) del 27 dicembre 2012. Dichiaro altresi', che nel nucleo familiare l'altro genitore non e' un \"non lavoratore\".")]
        Scelta32 = 320,

        [Description("genitore di figlio con disturbi specifici dell'apprendimento ai sensi della legge n. 170/2010 (DSA) interessato da una sospensione dell'attivita' educativa in presenza dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta33 = 330,
        [Description("disposizioni governative;")]
        Scelta33_1 = 331,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta33_2 = 332,
        [Description("determinazione della scuola.")]
        Scelta33_3 = 333,

        [Description("genitore di figlio con bisogni educativi speciali (BES) secondo la direttiva del Ministero dell'Istruzione, dell'Universita' e della Ricerca (MIUR) del 27 dicembre 2012 interessato da una sospensione dell'attivita' educativa in presenza dal #DATADAL# al #DATAAL#, per effetto di: ")]
        Scelta34 = 340,
        [Description("disposizioni governative;")]
        Scelta34_1 = 341,
        [Description("provvedimento di carattere regionale o locale, quali le ordinanze dei Presidenti di Regione e dei Sindaci;")]
        Scelta34_2 = 342,
        [Description("determinazione della scuola.")]
        Scelta34_3 = 343
    }

    public class ModuloVM
    {
        public string WidgetId { get; set; }

        public string Nominativo { get; set; }

        public string Matricola { get; set; }

        public String Sesso { get; set; }

        public string CodiceModulo { get; set; }

        public int Anno { get; set; }

        public bool Scelta1 { get; set; }

        public bool Scelta2 { get; set; }

        public bool Scelta3 { get; set; }

        public bool Scelta4 { get; set; }

        public bool Scelta5 { get; set; }

        public bool Scelta6 { get; set; }

        public bool Scelta7 { get; set; }

        public bool Scelta8 { get; set; }

        public bool Scelta7_1 { get; set; }

        public bool Scelta7_2 { get; set; }

        public bool Scelta7_3 { get; set; }

        public bool Scelta7_4 { get; set; }

        public bool Scelta7_5 { get; set; }

        public bool Scelta7_6 { get; set; }

        public bool Scelta7_7 { get; set; }

        public bool Scelta7_8 { get; set; }

        public DateTime Scelta7_Dal { get; set; }

        public DateTime Scelta7_Al { get; set; }

        public bool Scelta8_1 { get; set; }

        public bool Scelta8_2 { get; set; }

        public bool Scelta8_3 { get; set; }

        public bool Scelta8_4 { get; set; }

        public bool Scelta8_5 { get; set; }

        public DateTime Scelta8_Dal { get; set; }

        public DateTime Scelta8_Al { get; set; }

        public bool GiaScelto { get; set; }

        public DateTime? DataCompilazione { get; set; }

        public DateTime? DataLettura { get; set; }

        public DateTime? DataNascita { get; set; }

        public string LuogoNascita { get; set; }

        public string CodiceFiscale { get; set; }

        public Incentivazione012021VM IncentivazioneWidgetData { get; set; }

        #region AGGIUNTO IL 19/03/2021

        public bool Scelta9 { get; set; }

        public bool Scelta9_1 { get; set; }

        public bool Scelta9_2 { get; set; }

        public bool Scelta9_3 { get; set; }

        public bool Scelta9_4 { get; set; }

        public bool Scelta9_5 { get; set; }

        public bool Scelta9_6 { get; set; }

        public DateTime Scelta9_Dal { get; set; }

        public DateTime Scelta9_Al { get; set; }

        public bool Scelta10 { get; set; }

        public bool Scelta10_1 { get; set; }

        public bool Scelta10_2 { get; set; }

        public bool Scelta10_3 { get; set; }

        public bool Scelta10_4 { get; set; }

        public bool Scelta10_6 { get; set; }

        public DateTime Scelta10_Dal { get; set; }

        public DateTime Scelta10_Al { get; set; }

        public bool Scelta11 { get; set; }

        public bool Scelta11_1 { get; set; }

        public bool Scelta11_2 { get; set; }

        public bool Scelta11_3 { get; set; }

        public DateTime Scelta11_Dal { get; set; }
        public DateTime Scelta11_Al { get; set; }
        public bool Scelta12 { get; set; }
        public bool Scelta12_1 { get; set; }
        public bool Scelta12_3 { get; set; }
        public DateTime Scelta12_Dal { get; set; }
        #endregion


        #region AGGIUNTO IL 28/05/2021

        public bool Scelta13 { get; set; }
        public bool Scelta13_1 { get; set; }
        public bool Scelta13_2 { get; set; }
        public bool Scelta13_3 { get; set; }
        public DateTime Scelta13_Dal { get; set; }
        public DateTime Scelta13_Al { get; set; }
        public bool Scelta14 { get; set; }
        public bool Scelta14_1 { get; set; }
        public bool Scelta14_2 { get; set; }
        public bool Scelta14_3 { get; set; }
        public DateTime Scelta14_Dal { get; set; }
        public DateTime Scelta14_Al { get; set; }
        public bool Scelta15 { get; set; }
        public bool Scelta15_1 { get; set; }
        public bool Scelta15_2 { get; set; }
        public bool Scelta15_3 { get; set; }
        public DateTime Scelta15_Dal { get; set; }
        public DateTime Scelta15_Al { get; set; }
        public bool Scelta16 { get; set; }
        public DateTime Scelta16_Dal { get; set; }
        public DateTime Scelta16_Al { get; set; }
        public bool Scelta17 { get; set; }
        public DateTime Scelta17_Dal { get; set; }
        public bool Scelta18 { get; set; }
        public bool Scelta18_1 { get; set; }
        public bool Scelta18_2 { get; set; }
        public bool Scelta18_3 { get; set; }
        public DateTime Scelta18_Dal { get; set; }
        public DateTime Scelta18_Al { get; set; }
        public bool Scelta19 { get; set; }
        public bool Scelta19_1 { get; set; }
        public bool Scelta19_2 { get; set; }
        public bool Scelta19_3 { get; set; }
        public DateTime Scelta19_Dal { get; set; }
        public DateTime Scelta19_Al { get; set; }
        public bool Scelta20 { get; set; }
        public bool Scelta20_1 { get; set; }
        public bool Scelta20_2 { get; set; }
        public bool Scelta20_3 { get; set; }
        public DateTime Scelta20_Dal { get; set; }
        public DateTime Scelta20_Al { get; set; }
        public bool Scelta21 { get; set; }
        public DateTime Scelta21_Dal { get; set; }
        public DateTime Scelta21_Al { get; set; }
        public bool Scelta22 { get; set; }
        public DateTime Scelta22_Dal { get; set; }
        public bool Scelta23 { get; set; }
        public bool Scelta23_1 { get; set; }
        public bool Scelta23_2 { get; set; }
        public bool Scelta23_3 { get; set; }
        public DateTime Scelta23_Dal { get; set; }
        public DateTime Scelta23_Al { get; set; }
        public bool Scelta24 { get; set; }
        public bool Scelta24_1 { get; set; }
        public bool Scelta24_2 { get; set; }
        public bool Scelta24_3 { get; set; }
        public DateTime Scelta24_Dal { get; set; }
        public DateTime Scelta24_Al { get; set; }
        public bool Scelta25 { get; set; }
        public DateTime Scelta25_Dal { get; set; }
        public DateTime Scelta25_Al { get; set; }
        public bool Scelta26 { get; set; }
        public DateTime Scelta26_Dal { get; set; }
        public DateTime Scelta26_Al { get; set; }
        public bool Scelta27 { get; set; }
        public bool Scelta27_1 { get; set; }
        public bool Scelta27_2 { get; set; }
        public bool Scelta27_3 { get; set; }
        public DateTime Scelta27_Dal { get; set; }
        public DateTime Scelta27_Al { get; set; }
        public bool Scelta28 { get; set; }
        public bool Scelta28_1 { get; set; }
        public bool Scelta28_2 { get; set; }
        public bool Scelta28_3 { get; set; }
        public DateTime Scelta28_Dal { get; set; }
        public DateTime Scelta28_Al { get; set; }
        public bool Scelta29 { get; set; }
        public DateTime Scelta29_Dal { get; set; }
        public DateTime Scelta29_Al { get; set; }
        public bool Scelta30 { get; set; }
        public DateTime Scelta30_Dal { get; set; }
        public bool Scelta31 { get; set; }
        public bool Scelta32 { get; set; }
        public bool Scelta33 { get; set; }
        public bool Scelta33_1 { get; set; }
        public bool Scelta33_2 { get; set; }
        public bool Scelta33_3 { get; set; }
        public DateTime Scelta33_Dal { get; set; }
        public DateTime Scelta33_Al { get; set; }
        public bool Scelta34 { get; set; }
        public bool Scelta34_1 { get; set; }
        public bool Scelta34_2 { get; set; }
        public bool Scelta34_3 { get; set; }
        public DateTime Scelta34_Dal { get; set; }
        public DateTime Scelta34_Al { get; set; }
        #endregion



        public bool BtnAnnullaSceltaEnabled { get; set; }
        public string BtnAnnullaSceltaText { get; set; }
        public string BtnAnnullaSceltaTitleMessage { get; set; }
        public string BtnAnnullaSceltaConfirmMessage { get; set; }

    }

    public class ModuloSmart2020Selezioni
    {
        public ModuloSmart2020Selezioni()
        {
            this.DataSelezionataDal = null;
            this.DataSelezionataAl = null;
        }

        public ModuloSmart2020SelectionEnum Selezione { get; set; }
        public DateTime? DataSelezionataDal { get; set; }
        public DateTime? DataSelezionataAl { get; set; }
    }

    public class Incentivazione012021VM
    {
        public string Cognome { get; set; }

        public string Nome { get; set; }

        public string EtichettaProtocollo { get; set; }

        public string Azienda { get; set; }

        public string DataSelezionata { get; set; }

        public bool IsGiornalista { get; set; }

        public bool Scelta_Inc_1 { get; set; }

        public bool Scelta_Inc_2 { get; set; }

        public bool Scelta_Inc_2_1 { get; set; }

        public bool Scelta_Inc_2_2 { get; set; }

        public bool Scelta_Inc_2_3 { get; set; }

        public DateTime Scelta_Inc_Data { get; set; }

        public bool NonHa60Anni { get; set; }

        public bool Compie61Anni { get; set; }

        public bool Compie62Anni { get; set; }

        public List<SelectListItem> Date { get; set; }
    }

    public enum ModuloIncentivazione012021Enum
    {
        [AmbientValue("di aver conseguito o di conseguire nel corso del 2021 i requisiti per la pensione anticipata “in quota 100”")]
        Quota100 = 1,
        [AmbientValue("di non avere/non poter conseguire i requisiti per la pensione anticipata in “quota 100”, ma di conseguire quale primo trattamento pensionistico utile")]
        NoQuota100 = 2,
        [AmbientValue("la pensione di vecchiaia")]
        PensioneVecchiaia = 21,
        [AmbientValue("la pensione anticipata")]
        PensioneAnticipata = 22,
        [AmbientValue("la pensione di anzianità")]
        PensioneAnzianita = 23
    }

    public enum ModuloIncentivazione012021SimulazioneEnum
    {
        None = 0,
        All = 1,
        Dip61 = 2,
        Dip62 = 3,
        Gior61 = 4,
        Gior62 = 5
    }
}
