using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonManager
{
    public class Calendario
    {
        public Calendario()
        {
            ShowPreviousButton = true;
            ShowNextButton = true;
        }

        public int Mese { get; set; }
        public int Anno { get; set; }
        public string MeseCorrente { get; set; }
        public Boolean ShowPreviousButton { get; set; }
        public Boolean ShowNextButton { get; set; }
        public int MeseNext { get; set; }
        public int AnnoNext { get; set; }
        public int MesePrev { get; set; }
        public int AnnoPrev { get; set; }

        public List<CalendarioGiorno> DaysShowed { get; set; }

        public void NormalizzaMese(int? mese, int? anno)
        {
            int meseScelto = mese ?? DateTime.Now.Month;
            int annoScelto = anno ?? DateTime.Now.Year;

            DateTime primo = new DateTime(annoScelto, meseScelto, 1);
            DateTime mese1 = new DateTime(annoScelto, meseScelto, 1);

            Mese = primo.Month;
            Anno = primo.Year;
            MeseCorrente = primo.ToString("MMMM yyyy");
            MeseNext = primo.AddMonths(1).Month;
            AnnoNext = primo.AddMonths(1).Year;
            MesePrev = primo.AddMonths(-1).Month;
            AnnoPrev = primo.AddMonths(-1).Year;

            while (primo.DayOfWeek != DayOfWeek.Monday)
            {
                primo = primo.AddDays(-1);
            }
            DateTime ultimo = mese1.AddMonths(1).AddDays(-1);
            while (ultimo.DayOfWeek != DayOfWeek.Sunday)
            {
                ultimo = ultimo.AddDays(1);
            }
            DaysShowed = new List<CalendarioGiorno>();
            while (primo <= ultimo)
            {
                DaysShowed.Add(new CalendarioGiorno() { giorno = primo, isCurrentMonth = primo.Month == Mese });
                primo = primo.AddDays(1);
            }
        }
    }

    public class CalendarioGiorno
    {
        public DateTime giorno { get; set; }
        public Boolean isCurrentMonth { get; set; }
        public int? Frazione { get; set; }
        public string Tooltip { get; set; }
        public AppuntamentoStato Stato { get; set; }
    }

    public class Agenda
    {
        public Agenda()
        {
            Appuntamenti = new List<Appuntamento>();
            AnnoCorrente = DateTime.Today.Year;
            MeseCorrente = DateTime.Today.Month;
            MeseCorrenteString = DateTime.Today.ToString("MMMM");
            FrecciaAvanti = true;
            FrecciaIndietro = true;
        }
        public int AnnoCorrente { get; set; }
        public bool FrecciaAvanti { get; set; }
        public bool FrecciaIndietro { get; set; }
        public int MeseCorrente { get; set; }
        public string MeseCorrenteString { get; set; }
        public int MeseNext { get; set; }
        public int AnnoNext { get; set; }
        public int MesePrev { get; set; }
        public int AnnoPrev { get; set; }
        public List<Appuntamento> Appuntamenti { get; set; }

        public void NormalizzaMese(int? annoRichiesto, int? meseRichiesto)
        {
            MeseCorrente = meseRichiesto ?? DateTime.Now.Month;
            AnnoCorrente = annoRichiesto ?? DateTime.Now.Year;

            DateTime data = new DateTime(AnnoCorrente, MeseCorrente, 1);

            DateTime dataMod = data.AddMonths(-1);
            MesePrev = dataMod.Month;
            AnnoPrev = dataMod.Year;

            dataMod = data.AddMonths(1);
            MeseNext = dataMod.Month;
            AnnoNext = dataMod.Year;

            MeseCorrenteString = CommonHelper.TraduciMeseDaNumLett(String.Format("{0:00}", MeseCorrente));
        }
    }
    public class Appuntamento
    {
        public DateTime Giorno { get; set; }
        public string Orario { get; set; }
        public string Testo { get; set; }
        public string Sede { get; set; }
        public AppuntamentoStato Stato { get; set; }
    }
    public enum AppuntamentoStato
    {
        Approvato, DaApprovare
    }
}