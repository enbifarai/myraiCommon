using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimbratureCore
{
    public class AnalisiDettaglioGiornata
    {

    }

     
    public class DettaglioMinuti
    {
        public int ProgressivoMinuto { get; set; }
        public string HHMM { get; set; }
        public string tipo { get; set; }
        public string Copertura { get; set; }
    }
    public class Fascia
    {
        public string fascia { get; set; }
        public int minuti { get; set; }
    }
    public class EccezioneQuantita
    {
        public string Eccezione { get; set; }
        public int QuantitaMinuti { get; set; }
        public string QuantitaMinutiHHMM { get; set; }
        public string Dalle { get; set; }
        public string Alle { get; set; }
        public List<DettaglioMinuti> DettMinuti { get; set; }
      
    }
    public class Lacuna
    {
        public string dalleHHMM { get; set; }
        public string alleHHMM { get; set; }
        public int dalleMin { get; set; }
        public int alleMin { get; set; }
        public int durata { get; set; }
        
    }
    public class AnalisiGiornata
    {
        public AnalisiGiornata()
        {
            DettaglioMinutiGiornata = new List<DettaglioMinuti>();
            Fasce = new List<Fascia>();
            EccezioniCalcolate = new List<EccezioneQuantita>();
            Lacune = new List<Lacuna>();
        }
        public int MinutiPrimoIngresso { get; set; }
        public int MinutiFSH { get; set; }
        public int MinutiSEH { get; set; }

        public int MinutiURH { get; set; }
        public int MinutiTotali { get; set; }
        public string MinutiTotaliHHMM { get; set; }

        public string SMAPdalle { get; set; }
        public string SMAPalle { get; set; }
        public int MinutiSMAP { get; set; }
        public Boolean SMAPcoda { get; set; }

        public int MinutiIntervalloScalati { get; set; }
        public bool FS { get; set; }
        public bool SE { get; set; }

        public List<Fascia> Fasce { get; set; }
        public List<EccezioneQuantita> EccezioniCalcolate { get; set; }
        public List<Lacuna> Lacune { get; set; }
        public List<DettaglioMinuti> DettaglioMinutiGiornata { get; set; }



    }
}
