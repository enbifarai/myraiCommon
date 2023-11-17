using myRaiData;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;





namespace TimbratureCore
{
   
    public  class TimbratureInfo
    {
        public string username { get; set; }
        public string password { get; set; }
        public dayResponse giornata { get; set; }
        public getOrarioResponse orario { get; set; }
        public List<myRaiData.MyRai_Eccezioni_Ammesse> LEccAmmesse { get; set; }

        public  TimbratureInfo(string matricola, DateTime data, dayResponse respG=null)
        {
            var db = new digiGappEntities();
            var row = db.MyRai_ParametriSistema.Where(x => x.Chiave == "AccountUtenteServizio").FirstOrDefault();
            if (row != null)
            {
                this.username = row.Valore1;
                this.password = row.Valore2;
            }
            this.LEccAmmesse = db.MyRai_Eccezioni_Ammesse.ToList();
            WSDigigapp service = new WSDigigapp() { Credentials = new System.Net.NetworkCredential(this.username, this.password) };
            if (respG != null)
                this.giornata = respG;
            else
            this.giornata = service.getEccezioni(matricola, data.ToString("ddMMyyyy"), "BU", 75);

            MyRaiService1Client WcfClient = new MyRaiService1Client();
            WcfClient.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(this.username, this.password);
            this.orario = WcfClient.getOrario(giornata.orario.cod_orario, data.ToString("ddMMyyyy"), matricola, "BU", 75);

        }
        public  int GetBonusCarenza(string sedegapp)
        {
            var db = new digiGappEntities();
            var sede= db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sedegapp).FirstOrDefault();
            if (sede == null) return 0;
            else return sede.minimo_car.ToMinutes();
        }
        public  int GetMinutiIntervalloMensa()
        {
            return this.giornata.orario.intervallo_mensa.ToMinutes();
        }

        public  int GetQuartoDiGiornataInMinuti(getOrarioResponse orario)
        {
            int d = (Convert.ToInt32(orario.OrarioPresenzaPrevistaMin)) / 4;
            return d;
        }
        public  bool PrqDisponibile(string matricola)
        {
             MyRaiService1Client WcfClient = new MyRaiService1Client();
             WcfClient.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(username, password);
             GetFerieResponse response= WcfClient.GetFerie(matricola, DateTime.Now.Year.ToString());
             var PR = response.datiDipendente.Where(x => x.codiceEccezione == "PR").FirstOrDefault();
             return (PR.residui > 0.25);
        }
        /// <summary>
        /// Verifica se il dipendente ha timbratura in ingresso in ritardo rispetto all'orario consentito 
        /// compreso di flessibilità
        /// </summary>
        /// <param name="data"></param>
        /// <param name="matricola"></param>
        /// <param name="giornata"></param>
        /// <param name="orario"></param>
        /// <returns></returns>
        public  GiornataInfo GetGiornataInfo(DateTime data, string matricola)
        {
            GiornataInfo g = new GiornataInfo();
             
            if (giornata == null || giornata.timbrature == null || giornata.timbrature.Length == 0 ||
                giornata.timbrature[0].entrata == null || String.IsNullOrWhiteSpace(giornata.timbrature[0].entrata.orario))
            {
                g.errore = "Dati timbrature mancanti";
                return g;
            }

            try
            {
                int MinutiIngresso = giornata.timbrature[0].entrata.orario.ToMinutes();
                int MinutiMaxTollerati = Convert.ToInt32(orario.OrarioFineTolleranzaMin);
                g.OrarioBaseMinimoIngresso = orario.OrarioEntrataIniziale;
                    g.IngressoInRitardo = MinutiIngresso > MinutiMaxTollerati;
                    g.OrarioIngressoEffettuatoMinuti = MinutiIngresso;
                    g.OrarioIngressoEffettuato = giornata.timbrature[0].entrata.orario;
                    g.OrarioIngressoMaxTolleratoMinuti = MinutiMaxTollerati;
                    g.OrarioIngressoMaxTollerato = orario.OrarioFineTolleranza;

                //uscita
                    int MinutiUltimaTimbratura = 0;
                    var Timb = giornata.timbrature.LastOrDefault();
                    if (Timb != null && Timb.uscita != null && !String.IsNullOrWhiteSpace(Timb.uscita.orario))
                    {
                        MinutiUltimaTimbratura = Timb.uscita.orario.ToMinutes();

                        g.OrarioUscitaEffettuato = Timb.uscita.orario;
                        g.OrarioUscitaEffettuatoMinuti = Timb.uscita.orario.ToMinutes();

                        if (MinutiIngresso > MinutiMaxTollerati)
                        {
                           
                            g.OrarioUscitaDovuto = orario.OrarioUscitaIniziale;
                            g.OrarioUscitaDovutoMinuti = orario.OrarioUscitaIniziale.ToMinutes();
                            g.UscitaAnticipata = Timb.uscita.orario.ToMinutes() < orario.OrarioUscitaIniziale.ToMinutes();

                        }
                        else
                        {
                            int diff = MinutiMaxTollerati - MinutiIngresso;
                            int MinutiUscita = Convert.ToInt32(orario.OrarioUscitaFinaleMin) - diff;
                            if (MinutiUscita < Convert.ToInt32(orario.OrarioUscitaInizialeMin))
                            {
                                MinutiUscita = Convert.ToInt32(orario.OrarioUscitaInizialeMin);
                                g.OrarioUscitaDovutoMinuti = MinutiUscita;
                                g.OrarioUscitaDovuto = orario.OrarioUscitaIniziale;
                            }
                            else
                            {
                                g.OrarioUscitaDovutoMinuti = MinutiUscita;
                                g.OrarioUscitaDovuto = orario.OrarioUscitaFinale;
                            }

                            g.UscitaAnticipata = MinutiUltimaTimbratura < MinutiUscita;
                        }

                    }
                    else
                        g.errore = "Dati timbrature mancanti";

                    g.PRQdisponibile = PrqDisponibile(matricola);
                    g.MinutiQuartoDiGiornata = GetQuartoDiGiornataInMinuti(orario);
                    return g;
            }
            catch (Exception ex)
            {
                return new GiornataInfo() { IngressoInRitardo = null, errore = ex.Message };
            }
        }

        
        public  CarenzeInterneInfo GetCarenzeInfo(DateTime data, string matricola, string InizioMensaHHMM,
            string FineMensaHHMM)
        {
            CarenzeInterneInfo carenze = new CarenzeInterneInfo();
            carenze.FruitoMensa = FruitoMensa(data, matricola);
            carenze.Intervalli = new List<Intervallo>();
            carenze.MinutiBonusMensa = GetBonusCarenza(this.giornata.giornata.sedeGapp);

            if (this.giornata == null || this.giornata.timbrature == null || this.giornata.timbrature.Length == 0 )
            {
                return new CarenzeInterneInfo() { errore = "Dati timbrature mancanti", FruitoMensa = null, Intervalli = null };
            }
            carenze.MinutiIntervalloMensa = this.giornata.orario.intervallo_mensa.ToMinutes();

            for (int i=0;i<giornata.timbrature.Length-1 ;i++)
            {
                if (giornata.timbrature[i].uscita == null) continue;

                Intervallo Interv = new Intervallo();
                Interv.Inizio = giornata.timbrature[i].uscita.orario;
                Interv.InizioMin = giornata.timbrature[i].uscita.orario.ToMinutes();
                Interv.SedeDiUscita = giornata.timbrature[i].uscita.insediamento;

                Interv.Fine = giornata.timbrature[i + 1].entrata.orario;
                Interv.FineMin = giornata.timbrature[i + 1].entrata.orario.ToMinutes();
                Interv.SedeDiRientro = giornata.timbrature[i + 1].entrata.insediamento;

                Interv.TotaleMinuti = Interv.FineMin - Interv.InizioMin;

                int InizioMensaMin = InizioMensaHHMM.ToMinutes();
                int FineMensaMin = FineMensaHHMM.ToMinutes();
                Interv.InizioDurantePausaMensa = (Interv.InizioMin >= InizioMensaMin && Interv.InizioMin <= FineMensaMin);
                Interv.FineDurantePausaMensa = (Interv.FineMin >= InizioMensaMin && Interv.FineMin <= FineMensaMin);

                Interv.RitornoStessaSede = giornata.timbrature[i].uscita.insediamento == giornata.timbrature[i + 1].entrata.insediamento;

                carenze.Intervalli.Add(Interv);
            }

            return carenze;
        }
     
        private  DateTime? FruitoMensa(DateTime data, string matricola)
        {
            digiGappEntities db = new digiGappEntities();
            string badge=matricola.PadLeft (8,'0');
            DateTime d2 = data.AddDays(1);
            var rowMensa= db.MyRai_MensaXML.Where(x => x.Badge == badge 
                                                    && x.TransactionDateTime >= data 
                                                    && x.TransactionDateTime < d2)
                          .FirstOrDefault();

            if (rowMensa == null)
                return null;
            else
                return rowMensa.TransactionDateTime;
        }


     
    }

    public class GiornataInfo
    {
        public Boolean? UscitaAnticipata { get; set; }
        public Boolean? IngressoInRitardo { get; set; }
        public string OrarioBaseMinimoIngresso { get; set; }
        public int OrarioIngressoMaxTolleratoMinuti { get; set; }
        public string OrarioIngressoMaxTollerato { get; set; }
        public int OrarioIngressoEffettuatoMinuti { get; set; }
        public string OrarioIngressoEffettuato { get; set; }

        public string OrarioUscitaEffettuato { get; set; }
        public int OrarioUscitaEffettuatoMinuti { get; set; }
        public string OrarioUscitaDovuto { get; set; }
        public int OrarioUscitaDovutoMinuti { get; set; }

        public bool PRQdisponibile { get; set; }
        public int MinutiQuartoDiGiornata { get; set; }
        public string errore { get; set; }
    }

    public class CarenzeInterneInfo
    {
        public DateTime? FruitoMensa{get;set;}
        public int MinutiIntervalloMensa { get; set; }
        public int MinutiBonusMensa { get; set; }
        public List<Intervallo> Intervalli { get; set; }
        public string errore { get; set; }
    }

    public class Intervallo
    {
        public string Inizio { get; set; }
        public int InizioMin { get; set; }
        public string SedeDiUscita { get; set; }

        public string Fine { get; set; }
        public int FineMin { get; set; }
        public string SedeDiRientro { get; set; }

        public Boolean RitornoStessaSede { get; set; }

        public int TotaleMinuti { get; set; }
        public Boolean InizioDurantePausaMensa { get; set; }
        public Boolean FineDurantePausaMensa { get; set; }
    } 

 

   
}

      public static class ExtensionMethods
    {
        public static int ToMinutes(this string value)
        {
            if (value == null || value.Trim() == "" || value.Trim().Length < 4 || value.Contains("-")) return 0;


            string[] array = new string[2];

            if (value.IndexOf(':') > 0)
                array = value.Split(':');
            else
            {
                array[0] = value.Substring(0, 2);
                array[1] = value.Substring(2, 2);
            }
            return Int32.Parse(array[0]) * 60 + Int32.Parse(array[1]);
        }
        public static string ToHHMM(this int minuti)
        {
            int h = (int)minuti / 60;
            int min = minuti - (h * 60);
            return h.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0');
        }
    }