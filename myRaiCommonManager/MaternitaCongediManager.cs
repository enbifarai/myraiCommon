using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using myRaiCommonModel;
using myRaiCommonTasks;
using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Data.Entity;
using Newtonsoft.Json.Linq;
using myRaiCommonManager._Model;

namespace myRaiCommonManager
{
    public enum EnumPosizioniCampo
    {
        Prima_del_campo_DEFINIZIONE,
        Prima_del_campo_CRITERI_DI_INSERIMENTO,
        Prima_del_campo_TRATTAMENTO_ECONOMICO,
        Prima_del_campo_DOCUMENTAZIONE,
        Prima_del_campo_PRESUPPOSTI_E_PROCEDURE,
        Prima_del_campo_NOTE,
        Prima_del_campo_ALLEGATI,
        Prima_del_campo_FONTI_DELLA_DISCIPLINA,
        Prima_del_campo_ULTERIORI_INFORMAZIONI,
    }
    public class RisultatoEccezioni
    {
        public DateTime Data { get; set; }
        public string Codice { get; set; }
        public string Giorni { get; set; }
        public int Minuti { get; set; }
    }
    public class GetPresenzeResponse
    {
        public GetPresenzeResponse()
        {
            RisultatiEccezioniGetPresenze = new List<RisultatoEccezioniGetPresenze>();
        }
        public List<RisultatoEccezioniGetPresenze> RisultatiEccezioniGetPresenze { get; set; }
        public GetSchedaPresenzeMeseResponse ServiceResponse { get; set; }
    }
    public class RisultatoEccezioniGetPresenze
    {
        public DateTime Data { get; set; }
        public string Codice { get; set; }
        public string CodiceOrario { get; set; }
        public string Giorni { get; set; }
        public int Minuti { get; set; }
    }

    public class WeekEndComputoAF
    {
        public SettimanaComputoAF Settimana1 { get; set; }
        public SettimanaComputoAF Settimana2 { get; set; }
        public bool DaConsiderare { get; set; }
        public WeekEndComputoAF(SettimanaComputoAF _sett1, SettimanaComputoAF _sett2, bool IsInizioPeriodo)
        {

            this.Settimana1 = _sett1;
            this.Settimana2 = _sett2;

            if (Settimana1.PrimoGiornoARitrosoServizioOppureAF != null &&
                Settimana1.PrimoGiornoARitrosoServizioOppureAF.StartsWith("AF") && Settimana2.LunediAF)
            //if ( ! Settimana1.QualcheGiornoInServizio && Settimana2.LunediAF)
            {
                DaConsiderare = true;
            }
            else if (IsInizioPeriodo && Settimana1.TuttaAssenteFEML && Settimana1.PrimoGiornoARitrosoNonFerieInServizio == false
                && Settimana2.TuttaAF)
            {
                DaConsiderare = true;
            }
            else if (!IsInizioPeriodo && Settimana1.TuttaAF && Settimana2.PrimoGiornoAvantiNonFerieInServizio == false
               && Settimana2.TuttaAssenteFEML)
            {
                DaConsiderare = true;
            }
            else if (IsInizioPeriodo && Settimana1.TuttaAF && Settimana2.TuttaAssenteFEML)
            {
                DaConsiderare = true;
            }
            else if (Settimana1.TuttaAF && Settimana2.TuttaAF)
            {
                DaConsiderare = true;
            }
            else if (Settimana1.TuttaAF && Settimana2.QualcheGiornoAssenteFEML && !Settimana2.QualcheGiornoInServizio
                 && Settimana2.QualcheGiornoAF)
            {
                DaConsiderare = true;
            }
        }
    }

    public class SettimanaComputoAF
    {
        GetSchedaPresenzeMeseResponse responsePresenze { get; set; }
        GetTimbratureMeseResponse responseTimbrature { get; set; }
        public DateTime D1 { get; set; }
        public DateTime D2 { get; set; }
        public bool TuttaAF { get; set; }
        public bool LunediAF { get; set; }
        public bool VenerdiAF { get; set; }
        public bool QualcheGiornoAF { get; set; }

        public bool TuttaAssenteFEML { get; set; }
        public bool LunediAssenteFEML { get; set; }
        public bool VenerdiAssenteFEML { get; set; }
        public bool QualcheGiornoAssenteFEML { get; set; }

        public bool TuttaInServizio { get; set; }
        public bool LunediInServizio { get; set; }
        public bool VenerdiInServizio { get; set; }
        public bool QualcheGiornoInServizio { get; set; }

        public List<string> EccezioniFEML { get; set; }
        public List<string> EccezioniInServizio { get; set; }

        public bool PrimoGiornoARitrosoNonFerieInServizio { get; set; }
        public bool PrimoGiornoAvantiNonFerieInServizio { get; set; }

        public List<DateTime> GiorniAFnei30ggPrecedenti { get; set; }
        public List<DateTime> GiorniInServizioNei30ggPrecedenti { get; set; }
        public string PrimoGiornoARitrosoServizioOppureAF { get; set; }

        public SettimanaComputoAF(DateTime D1, DateTime D2, string matricola)
        {
            this.GiorniAFnei30ggPrecedenti = new List<DateTime>();
            this.GiorniInServizioNei30ggPrecedenti = new List<DateTime>();

            this.D1 = D1;
            this.D2 = D2;
            MyRaiService1Client cl = new MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            DateTime Dstart = new DateTime(D1.AddMonths(-1).Year, D1.AddMonths(-1).Month, 1);
            DateTime Dend = new DateTime(D2.AddMonths(1).Year, D2.AddMonths(1).Month, 1);
            for (DateTime D = Dstart; D <= Dend; D = D.AddMonths(1))
            {
                if (responsePresenze == null)
                    responsePresenze = cl.GetSchedaPresenzeMese(matricola, D, D.AddMonths(1).AddDays(-1));
                else
                {
                    var r = cl.GetSchedaPresenzeMese(matricola, D, D.AddMonths(1).AddDays(-1));
                    if (r != null && r.Giorni != null)
                    {
                        List<InfoPresenza> L = responsePresenze.Giorni.ToList();
                        L.AddRange(r.Giorni.ToList());
                        responsePresenze.Giorni = L.ToArray();
                    }
                }
                if (responseTimbrature == null)
                    responseTimbrature = cl.GetTimbratureMese(matricola, D, D.AddMonths(1).AddDays(-1));
                else
                {
                    var t = cl.GetTimbratureMese(matricola, D, D.AddMonths(1).AddDays(-1));
                    if (t != null && t.Giorni != null)
                    {
                        List<InfoGiornata> LI = responseTimbrature.Giorni.ToList();
                        LI.AddRange(t.Giorni);
                        responseTimbrature.Giorni = LI.ToArray();
                    }
                }
            }
            this.EccezioniFEML = MaternitaCongediManager.GetEccezioniFE_ML();
            this.EccezioniInServizio = MaternitaCongediManager.GetEccezioniInServizio();
        }

        public List<DateTime> GetDateConAF(DateTime data1, DateTime data2)
        {
            List<DateTime> GiorniAF = new List<DateTime>();
            for (DateTime D = data1; D < data2; D = D.AddDays(1))
            {
                var giorno = responsePresenze.Giorni.Where(x => x.data == D).FirstOrDefault();
                if (giorno != null && giorno.MacroAssenze != null &&
                    giorno.MacroAssenze.Any(x => x != null && (x.StartsWith("NAF") || x.StartsWith("NBF") || x.StartsWith("NCF"))))
                {
                    GiorniAF.Add(giorno.data);
                }
            }
            return GiorniAF;
        }
        public List<DateTime> GetGiorniInServizio(DateTime data1, DateTime data2)
        {
            List<DateTime> GiorniServizio = new List<DateTime>();
            for (DateTime D = data1; D < data2; D = D.AddDays(1))
            {
                var Giorno = responsePresenze.Giorni.Where(x => x.data == D).FirstOrDefault();
                var GiornoTimbrature = responseTimbrature.Giorni.Where(x => x.data == D).FirstOrDefault();
                if (Giorno != null && !Giorno.CodiceOrario.StartsWith("9") && (
                    !String.IsNullOrWhiteSpace(Giorno.Ingresso) ||
                    !String.IsNullOrWhiteSpace(Giorno.Ingresso) ||
                    Giorno.MacroAssenze.Any(x => x.StartsWith("NSW"))
                   || (Giorno.MicroAssenze != null && Giorno.MicroAssenze.Any(x => x.nome != null && x.nome.StartsWith("NSW")))
                    ))
                {
                    GiorniServizio.Add(Giorno.data);
                }
            }
            return GiorniServizio;
        }
        public string GetPrimoGiornoARitrosoServizioOppureAF(DateTime data1, DateTime data2)
        {
            for (DateTime D = data2.AddDays(-1); D >= data1; D = D.AddDays(-1))
            {
                var Giorno = responsePresenze.Giorni.Where(x => x.data == D).FirstOrDefault();
                var GiornoTimbrature = responseTimbrature.Giorni.Where(x => x.data == D).FirstOrDefault();
                if (Giorno != null && !Giorno.CodiceOrario.StartsWith("9") && (
                   !String.IsNullOrWhiteSpace(Giorno.Ingresso) ||
                   !String.IsNullOrWhiteSpace(Giorno.Ingresso) ||
                   Giorno.MacroAssenze.Any(x => x.StartsWith("NSW"))
                  || (Giorno.MicroAssenze != null && Giorno.MicroAssenze.Any(x => x.nome != null && x.nome.StartsWith("NSW")))
                   ))
                {
                    return "SERVIZIO " + D.ToString("dd/MM/yyyy");
                }
                if (Giorno != null && Giorno.MacroAssenze != null &&
                   Giorno.MacroAssenze.Any(x => x != null && (x.StartsWith("NAF") || x.StartsWith("NBF") || x.StartsWith("NCF"))))
                {
                    return "AF " + D.ToString("dd/MM/yyyy");
                }
            }
            return null;
        }
        public void Popola(XR_MAT_RICHIESTE Rich, string FormaContratto)
        {
            int AF = 0;
            int FEML = 0;
            int INSERV = 0;
            DateTime InizioPeriodo = Rich.INIZIO_GIUSTIFICATIVO != null ? Rich.INIZIO_GIUSTIFICATIVO.Value : Rich.DATA_INIZIO_MATERNITA.Value;
            DateTime Inizio30GGprecedenti = InizioPeriodo.AddDays(-30);

            this.GiorniAFnei30ggPrecedenti = GetDateConAF(Inizio30GGprecedenti, InizioPeriodo);
            this.GiorniInServizioNei30ggPrecedenti = GetGiorniInServizio(Inizio30GGprecedenti, InizioPeriodo);
            this.PrimoGiornoARitrosoServizioOppureAF = GetPrimoGiornoARitrosoServizioOppureAF(Inizio30GGprecedenti, InizioPeriodo);


            List<string> EccCongedi = new List<string>() { Rich.ECCEZIONE };
            if (Rich.ECCEZIONE != null && (Rich.ECCEZIONE.StartsWith("AF") || Rich.ECCEZIONE.StartsWith("BF")
               || Rich.ECCEZIONE.StartsWith("CF")))
            {
                EccCongedi = "AF,BF,CF".Split(',').ToList();
            }
            for (DateTime D = D1; D <= D2; D = D.AddDays(1))
            {

                var Giorno = responsePresenze.Giorni.Where(x => x.data == D).FirstOrDefault();
                if (Giorno == null)
                {

                }
                if (Giorno == null || Giorno.MacroAssenze == null || !Giorno.MacroAssenze.Any())
                    continue;
                var GiornoTimbrature = responseTimbrature.Giorni.Where(x => x.data == D).FirstOrDefault();
                if ((FormaContratto == "K" && Giorno.CodiceOrario == "97") ||
                    (!String.IsNullOrWhiteSpace(Giorno.Ingresso) || !String.IsNullOrWhiteSpace(Giorno.Uscita)) ||
                    (Giorno.data > DateTime.Today && !Giorno.MacroAssenze.Any(x => x.Length > 1))
                    )
                {
                    INSERV++;
                    this.QualcheGiornoInServizio = true;

                    if (Giorno.data.DayOfWeek == DayOfWeek.Monday)
                        this.LunediInServizio = true;
                    if (Giorno.data.DayOfWeek == DayOfWeek.Friday)
                        this.VenerdiInServizio = true;

                    continue;
                }

                foreach (string ma in Giorno.MacroAssenze)
                {

                    if (String.IsNullOrWhiteSpace(ma) || ma.Length == 1) continue;
                    string ecc = ma.Substring(1);
                    if (EccCongedi.Any(x => ecc.StartsWith(x)))
                    //if (ecc.StartsWith("AF") || ecc.StartsWith("BF") || ecc.StartsWith("CF") || ecc.StartsWith("PS"))
                    {
                        AF++;
                        this.QualcheGiornoAF = true;

                        if (Giorno.data.DayOfWeek == DayOfWeek.Monday)
                            this.LunediAF = true;
                        if (Giorno.data.DayOfWeek == DayOfWeek.Friday)
                            this.VenerdiAF = true;

                        continue;
                    }
                    else if (EccezioniFEML.Contains(ecc))
                    {
                        FEML++;
                        this.QualcheGiornoAssenteFEML = true;

                        if (Giorno.data.DayOfWeek == DayOfWeek.Monday)
                            this.LunediAssenteFEML = true;
                        if (Giorno.data.DayOfWeek == DayOfWeek.Friday)
                            this.VenerdiAssenteFEML = true;

                        continue;
                    }
                    else if (EccezioniInServizio.Contains(ecc) ||
                        (FormaContratto == "K" && Giorno.CodiceOrario == "97") ||
                        (!String.IsNullOrWhiteSpace(Giorno.Ingresso) || !String.IsNullOrWhiteSpace(Giorno.Uscita))
                        )
                    {
                        INSERV++;
                        this.QualcheGiornoInServizio = true;

                        if (Giorno.data.DayOfWeek == DayOfWeek.Monday)
                            this.LunediInServizio = true;
                        if (Giorno.data.DayOfWeek == DayOfWeek.Friday)
                            this.VenerdiInServizio = true;

                        continue;
                    }

                }

            }
            this.TuttaAF = (AF == 1 + (D2 - D1).Days);
            this.TuttaAssenteFEML = (FEML == 1 + (D2 - D1).Days);
            this.TuttaInServizio = (INSERV == 1 + (D2 - D1).Days);
            if (this.TuttaAssenteFEML)
            {
                DateTime Dlimite = new DateTime(D1.AddMonths(-1).Year, D1.AddMonths(-1).Month, 1);
                bool Found = false;
                for (DateTime Dcorrente = D1; Dcorrente > Dlimite; Dcorrente = Dcorrente.AddDays(-1))
                {
                    var Giorno = responsePresenze.Giorni.Where(x => x.data == Dcorrente).FirstOrDefault();
                    if (Giorno.CodiceOrario != null && Giorno.CodiceOrario.StartsWith("9"))
                        continue;

                    if (!String.IsNullOrWhiteSpace(Giorno.Ingresso) || !String.IsNullOrWhiteSpace(Giorno.Uscita))
                    {
                        Found = true;
                        this.PrimoGiornoARitrosoNonFerieInServizio = true;
                        break;
                    }
                    foreach (string ma in Giorno.MacroAssenze)
                    {
                        if (String.IsNullOrWhiteSpace(ma) || ma.Length == 1) continue;
                        string ecc = ma.Substring(1);
                        if (EccCongedi.Any(x => ecc.StartsWith(x)))
                        {
                            Found = true;
                            this.PrimoGiornoARitrosoNonFerieInServizio = false;
                            break;
                        }

                        if (EccezioniInServizio.Contains(ecc))
                        {
                            Found = true;
                            this.PrimoGiornoARitrosoNonFerieInServizio = true;
                            break;
                        }
                    }
                    if (Found)
                        break;
                }



                DateTime DlimiteAvanti = new DateTime(D2.AddMonths(2).Year, D2.AddMonths(2).Month, 1);
                Found = false;
                for (DateTime Dcorrente = D2; Dcorrente < DlimiteAvanti; Dcorrente = Dcorrente.AddDays(1))
                {
                    var Giorno = responsePresenze.Giorni.Where(x => x.data == Dcorrente).FirstOrDefault();
                    if (Giorno.CodiceOrario != null && Giorno.CodiceOrario.StartsWith("9"))
                        continue;

                    if (!String.IsNullOrWhiteSpace(Giorno.Ingresso) || !String.IsNullOrWhiteSpace(Giorno.Uscita))
                    {
                        Found = true;
                        this.PrimoGiornoAvantiNonFerieInServizio = true;
                        break;
                    }
                    foreach (string ma in Giorno.MacroAssenze)
                    {
                        if (Dcorrente > DateTime.Today && !Giorno.MacroAssenze.Any(x => x.Length > 1))
                        {
                            Found = true;
                            this.PrimoGiornoAvantiNonFerieInServizio = true;
                            break;
                        }
                        if (String.IsNullOrWhiteSpace(ma) || ma.Length == 1) continue;
                        string ecc = ma.Substring(1);
                        if (EccCongedi.Any(x => ecc.StartsWith(x)))
                        {
                            Found = true;
                            this.PrimoGiornoAvantiNonFerieInServizio = false;
                            break;
                        }

                        if (EccezioniInServizio.Contains(ecc))
                        {
                            Found = true;
                            this.PrimoGiornoAvantiNonFerieInServizio = true;
                            break;
                        }
                    }
                    if (Found)
                        break;
                }

            }
        }
    }
    public class WeekEnd
    {
        public DayWeekEnd Sabato { get; set; }
        public DayWeekEnd Domenica { get; set; }
    }
    public class DayWeekEnd
    {
        public DateTime Data { get; set; }
        public bool DaConteggiare { get; set; }
    }


    public class MaternitaCongediManager
    {


        public static class STR
        {
            public static float GetStr()
            {
                return 0;
            }
        }
        public enum EnumStatiRichiesta
        {
            Inviata = 10,
            InCaricoGestione = 20,
            ApprovataGestione = 30,
            InCaricoUffPers = 40,
            ApprovataUffPers = 50,
            InCaricoAmmin = 60,
            ApprovataAmmin = 70,
            Approvata = 80,
            Annullata = 90,
            AnnullataDipendente = 101,
            AnnullataGestione = 102,
            AnnullataAmministrazione = 103
        }
        public static XR_MAT_RICHIESTE GetRichiestaContiguaPrecedente(XR_MAT_RICHIESTE Rich)
        {
            //vale solo per af mt mu
            if (Rich.ECCEZIONE != "AF" && Rich.ECCEZIONE != "MT" && Rich.ECCEZIONE != "MU")
                return null;

            var db = new IncentiviEntities();
            List<XR_MAT_RICHIESTE> AltreRichieste = db.XR_MAT_RICHIESTE.Where(x =>
                            x.MATRICOLA == Rich.MATRICOLA && x.ID != Rich.ID &&
                           (x.ECCEZIONE.StartsWith("AF") || x.ECCEZIONE == "MT" || x.ECCEZIONE == "MU")).ToList();
            if (!AltreRichieste.Any()) return null;

            DateTime DataInizio = Rich.INIZIO_GIUSTIFICATIVO ?? Rich.DATA_INIZIO_MATERNITA.Value;
            //cerca richiesta con task tracciato che termina un giorno prima:
            string Dprima = DataInizio.AddDays(-1).ToString("dd/MM/yyyy");
            var RichiestaContiguaPrecedente = AltreRichieste.Where(x => x.XR_MAT_TASK_IN_CORSO.Any(z => z.NOTE != null && z.NOTE.EndsWith(Dprima))).FirstOrDefault();
            return RichiestaContiguaPrecedente;
        }
        public static void AggiornaTracciatoPagato(string meseanno, int idTask, string json)
        {
            var db = new IncentiviEntities();
            var t = db.XR_MAT_TASK_IN_CORSO.Where(x => x.ID == idTask).FirstOrDefault();
            t.MESE_ANNO_PAGATO = meseanno;
            t.MESE_ANNO_PAGATO_JSON = json;
            db.SaveChanges();
        }
        public static List<WeekEnd> GetSabDomPeriodo(DateTime D1, DateTime D2)
        {
            List<WeekEnd> L = new List<WeekEnd>();
            for (DateTime D = D1; D <= D2; D = D1.AddDays(1))
            {
                WeekEnd W = new WeekEnd();
                W.Sabato = new DayWeekEnd();
                W.Domenica = new DayWeekEnd();

                if (D.DayOfWeek == DayOfWeek.Saturday)
                    W.Sabato.Data = D;

                if (D.DayOfWeek == DayOfWeek.Sunday)
                    W.Domenica.Data = D;

                L.Add(W);
            }
            return L;
        }
        public static DateTime[] GetSettimanePrimaDopo(DateTime D, bool Prima)
        {
            DateTime D1;
            if (Prima)
            {
                if (D.DayOfWeek == DayOfWeek.Saturday) D1 = D.AddDays(-5);

                else if (D.DayOfWeek == DayOfWeek.Sunday) D1 = D.AddDays(-6);

                else return null;
            }
            else
            {
                if (D.DayOfWeek == DayOfWeek.Saturday) D1 = D.AddDays(2);

                else if (D.DayOfWeek == DayOfWeek.Sunday) D1 = D.AddDays(1);

                else return null;
            }
            DateTime D2 = D1.AddDays(4);
            return new DateTime[] { D1, D2 };
        }
        public static List<myRaiData.Incentivi.XR_VAR_DESCRITTIVE> GetDifferenze(int anno, int mese)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();

            List<myRaiData.Incentivi.XR_MAT_TASK_IN_CORSO> Tasks =
                db.XR_MAT_TASK_IN_CORSO.Where(x => x.ID_TASK == 11 && x.ANNO == anno && x.MESE == mese && x.TERMINATA == true)
                .OrderBy(x => x.INPUT).ToList();

            List<myRaiData.Incentivi.XR_VAR_DESCRITTIVE> Desc =
                db.XR_VAR_DESCRITTIVE.Where(x => x.Anno == anno && x.Mese == mese).OrderBy(x => x.Matricola).ToList();

            List<myRaiData.Incentivi.XR_VAR_DESCRITTIVE> DescDifferenza = new List<XR_VAR_DESCRITTIVE>();
            foreach (var D in Desc)
            {
                if (!IsPresentInTask(D, Tasks))
                {
                    DescDifferenza.Add(D);
                }
            }
            return DescDifferenza;
        }

        private static bool IsPresentInTask(XR_VAR_DESCRITTIVE descrittiva, List<XR_MAT_TASK_IN_CORSO> tasks)
        {
            var taskPresente = tasks.Where(x => x.INPUT.StartsWith(descrittiva.Matricola) && x.MESE == descrittiva.Mese && x.ANNO == descrittiva.Anno &&
            x.INPUT.Contains(descrittiva.Descrittiva)).FirstOrDefault();

            return taskPresente != null;
        }
        public static bool IsAbilitatoDifferenzeDescrittive()
        {
            string m = CommonHelper.GetParametro<string>(EnumParametriSistema.MatricoleAbilitateDifferenzeDescrittive);
            if (String.IsNullOrWhiteSpace(m)) return false;

            return m.Split(',').Contains(CommonHelper.GetCurrentUserMatricola());
        }
        public static bool IsAbilitatoUpload()
        {
            string m = CommonHelper.GetParametro<string>(EnumParametriSistema.MatricoleAbilitateUploadCongedi);
            if (String.IsNullOrWhiteSpace(m)) return false;

            return m.Split(',').Contains(CommonHelper.GetCurrentUserMatricola());
        }
        public static bool PeriodoRichiestaIniziaMeseAttuale(XR_MAT_RICHIESTE richiesta)
        {
            DateTime? D1 = richiesta.INIZIO_GIUSTIFICATIVO;
            if (D1 == null) D1 = richiesta.DATA_INIZIO_MATERNITA;
            return (D1.Value.Month == DateTime.Today.Month && D1.Value.Year == DateTime.Today.Year);
        }
        public static XR_MAT_TASK_DI_SERVIZIO GetTaskServizio_InserimentoEccezioni(IncentiviEntities db, string Ecc, List<DateTime> Dates, int IdRichiesta, int anno, int mese)
        {
            XR_MAT_TASK_DI_SERVIZIO TaskServizio = new XR_MAT_TASK_DI_SERVIZIO();
            TaskServizio.ID_RICHIESTA = IdRichiesta;
            TaskServizio.ID_TASK = db.XR_MAT_ELENCO_TASK.Where(x => x.NOME_TASK == "INSERIMENTO ECCEZIONI").Select(x => x.ID).FirstOrDefault();
            TaskServizio.MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola();
            TaskServizio.DATA_CREAZIONE = DateTime.Now;
            TaskServizio.ESEGUIBILE_DA_DATA = DateTime.Now;
            TaskServizio.ESEGUIBILE_FINO_A_DATA = new DateTime(9999, 12, 31);
            TaskServizio.ECCEZIONE = Ecc;
            TaskServizio.INPUT = String.Join(",", Dates.Select(x => x.ToString("dd/MM/yyyy")).ToArray());
            TaskServizio.MESE = mese;
            TaskServizio.ANNO = anno;
            TaskServizio.SISTEMA_OUTPUT = "GAPP";
            TaskServizio.PROGRESSIVO = 0;
            TaskServizio.TERMINATA = false;

            return TaskServizio;

        }
        public static string GetDescrittivaEccezione(string ecc)
        {
            var db = new myRaiData.digiGappEntities();
            string desc = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione == ecc).Select(x => x.desc_eccezione).FirstOrDefault();
            return desc;
        }
        public static XR_MAT_RICHIESTE GetRichiestaAdiacentePrecedente(XR_MAT_RICHIESTE Rich)
        {
            XR_MAT_RICHIESTE Rprec = GetRichiestaAdiacentePrecedenteInternal(Rich);
            if (Rprec == null) return null;

            int lastIdFound = 0;

            while (Rprec != null)
            {
                lastIdFound = Rprec.ID;
                Rprec = GetRichiestaAdiacentePrecedenteInternal(Rprec);
            }

            var db = new IncentiviEntities();
            var lastRich = db.XR_MAT_RICHIESTE.Where(x => x.ID == lastIdFound).FirstOrDefault();
            return lastRich;
        }
        private static XR_MAT_RICHIESTE GetRichiestaAdiacentePrecedenteInternal(XR_MAT_RICHIESTE Rich)
        {
            var db = new IncentiviEntities();
            var richiesteDip = db.XR_MAT_RICHIESTE.Where(x => x.ID != Rich.ID &&
                               x.MATRICOLA == Rich.MATRICOLA && x.CATEGORIA == Rich.CATEGORIA).ToList();

            DateTime? Dinizio = Rich.INIZIO_GIUSTIFICATIVO;
            if (Dinizio == null)
            {
                Dinizio = Rich.DATA_INIZIO_MATERNITA;
                DateTime Dfine = Dinizio.Value.AddDays(-1);

                var richPrecedente = richiesteDip.Where(x => x.DATA_FINE_MATERNITA == Dfine).FirstOrDefault();
                return richPrecedente;
            }
            else
            {
                DateTime Dfine = Dinizio.Value.AddDays(-1);
                var richPrecedente = richiesteDip.Where(x => x.FINE_GIUSTIFICATIVO == Dfine).FirstOrDefault();
                return richPrecedente;
            }
        }

        public static float ControllaSospeseCestinate(List<DettaglioGiorniPerMese> GiorniMese, int year, int month, float g26mi, XR_MAT_RICHIESTE Rich)
        {
            var meseoggetto = GiorniMese.Where(x => x.RiferimentoPrimoDelMese.Year == year && x.RiferimentoPrimoDelMese.Month == month).FirstOrDefault();
            var giorniEccezione = meseoggetto.ElencoGiorni.Where(x => x.CodiceEccezione.StartsWith(Rich.ECCEZIONE)
            && x.StatoEccez == "C1").ToList();
            float TogliSospeseTotale = 0;
            foreach (var g in giorniEccezione)
            {
                if (g.NumeroGiorniGapp > 0)
                {
                    TogliSospeseTotale += g.NumeroGiorniGapp;
                    continue;
                }
                if (g.NumeroGiorniRuoli > 0)
                {
                    TogliSospeseTotale += g.NumeroGiorniRuoli;
                }
            }

            return (float)Math.Round(g26mi - TogliSospeseTotale, 2);
        }
        public static float ControllaSe26(int year, int month, float g26mi, XR_MAT_RICHIESTE Rich)
        {
            double fattore = 1.2;
            //if (Rich.ASSENZA_LUNGA==true) fattore = 1;
            double g26miDouble = Math.Round(g26mi, 2);
            float giorniferiali = GetWeekDaysInMonth(year, month);
            if (g26miDouble >= giorniferiali * fattore) g26mi = 26;
            return g26mi;
        }
        private static List<TaskPronto> GetTaskProntiDaElaborazioneTE(int IdRichiesta, TaskModel model)
        {
            var db = new IncentiviEntities();
            var Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == IdRichiesta).FirstOrDefault();
            List<TaskPronto> ListaTaskPronti = new List<TaskPronto>();

            model.DateRiferimentoPrimoDelMeseTaskNecessari = new List<DateTime>() {
                new DateTime (DateTime.Now.Year,DateTime.Now.Month,1)
            };

            int statoUscitaPratica = 100;


            foreach (var dataprimo in model.DateRiferimentoPrimoDelMeseTaskNecessari)
            {
                foreach (var catTask in Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK
                                   .Where(x => x.STATO_PRATICA == statoUscitaPratica)
                    .OrderBy(x => x.PROGRESSIVO))
                {
                    XR_MAT_ELENCO_TASK TaskElenco = catTask.XR_MAT_ELENCO_TASK;

                    if (TaskElenco != null)
                    {
                        TaskPronto TP = new TaskPronto()
                        {
                            DataRiferimentoMeseAnno = dataprimo,
                            DicituraPeriodo = "",
                            NumeroFusioni = 0,
                            IntervalliFusi = new List<string>(),
                            PeriodoDa = new DateTime(2020, 10, 1),
                            PeriodoA = new DateTime(2020, 10, 31)
                        };

                        string testoTracciato =// "01504140011021114      11011021078                                              ";
                        MaternitaCongediManager.GetCampiTracciato(
                         (int)TaskElenco.ID_TRACCIATO_DEW,
                         (int)TaskElenco.PROGRESSIVO_TRACCIATO_DEW,
                         Richiesta,
                         model.EccezioneRisultante,
                         "",
                         "",
                         "",
                         DateTime.Now,
                         "",
                         model.Importo13ma,
                         model.Importo14ma,
                         model.ImportoPremio,
                         "",
                         "",
                         null,
                         TaskElenco
                         );
                        XR_MAT_TASK_IN_CORSO TaskInCorsoEntity = new XR_MAT_TASK_IN_CORSO()
                        {
                            ANNO = dataprimo.Year,
                            MESE = dataprimo.Month,
                            DATA_CREAZIONE = DateTime.Now,
                            INPUT = testoTracciato,
                            MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                            XR_MAT_ELENCO_TASK = TaskElenco,
                            XR_MAT_RICHIESTE = Richiesta,
                            PROGRESSIVO = (int)catTask.PROGRESSIVO,
                            ID_RICHIESTA = Richiesta.ID,
                            ID_TASK = TaskElenco.ID,
                            BLOCCATA_DATETIME = DateTime.Now,
                            BLOCCATA_DA_OPERATORE = CommonHelper.GetCurrentUserMatricola()

                        };
                        TP.TaskInCorso = TaskInCorsoEntity;
                        List<CampoContent> ListaCampi =
                            GetTracciatoEsploso((int)TP.TaskInCorso.XR_MAT_ELENCO_TASK.ID_TRACCIATO_DEW,
                            (int)TP.TaskInCorso.XR_MAT_ELENCO_TASK.PROGRESSIVO_TRACCIATO_DEW,
                                  TaskInCorsoEntity.INPUT);

                        TP.TracciatoEsplosoModel = new TaskTracciatoExpModel();
                        TP.TracciatoEsplosoModel.TracciatoEsploso = new ContenutoCampiPerMeseTask()
                        {
                            Campi = ListaCampi
                        };
                        TP.TracciatoEsplosoModel.TracciatoEsploso.TracciatoIntero = TaskInCorsoEntity.INPUT;
                        TP.TracciatoEsplosoModel.EditPermesso = TP.IdAltraPraticaGiaSuDB == 0;
                        ListaTaskPronti.Add(TP);
                    }
                }
            }
            return ListaTaskPronti;
        }
        public static string AccodaTracciatoPerInvio(string Matricola, string TestoTracciato, int IdTracciato,
            DateTime EseguibileDa, DateTime EseguibileA, int Mese, int Anno)
        {
            var db = new IncentiviEntities();
            XR_MAT_TASK_IN_CORSO TaskNew = new XR_MAT_TASK_IN_CORSO()
            {
                ANNO = Anno,
                MESE = Mese,
                DATA_CREAZIONE = DateTime.Now,
                ESEGUIBILE_DA_DATA = EseguibileDa,
                ESEGUIBILE_FINO_A_DATA = EseguibileA,
                ID_RICHIESTA = 0,
                ID_TASK = 0,
                MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                INPUT = TestoTracciato,
                PROGRESSIVO = 1
            };
            try
            {
                db.XR_MAT_TASK_IN_CORSO.Add(TaskNew);
                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
        public static string CreaTracciatoPerStornoIDTaskInCorso(int id)
        {
            var db = new IncentiviEntities();
            var taskInCorso = db.XR_MAT_TASK_IN_CORSO.Where(x => x.ID == id).FirstOrDefault();

            try
            {
                List<CampoContent> ListaCampi =
                                   GetTracciatoEsploso((int)taskInCorso.XR_MAT_ELENCO_TASK.ID_TRACCIATO_DEW,
                                   (int)taskInCorso.XR_MAT_ELENCO_TASK.PROGRESSIVO_TRACCIATO_DEW,
                                   taskInCorso.INPUT);

                ListaCampi = ModificaCampiPerStorno(ListaCampi);

                string TestoTracciato = RebuildTracciato((int)taskInCorso.XR_MAT_ELENCO_TASK.ID_TRACCIATO_DEW,
                    (int)taskInCorso.XR_MAT_ELENCO_TASK.PROGRESSIVO_TRACCIATO_DEW, ListaCampi);

                XR_MAT_TASK_IN_CORSO TaskNew = new XR_MAT_TASK_IN_CORSO()
                {
                    ANNO = taskInCorso.ANNO,
                    MESE = taskInCorso.MESE,
                    BLOCCATA_DATETIME = DateTime.Now,// taskInCorso.BLOCCATA_DATETIME,
                    BLOCCATA_DA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),// taskInCorso.BLOCCATA_DA_OPERATORE,
                    DATA_CREAZIONE = DateTime.Now,
                    DATA_ORA_CESTINATO = taskInCorso.DATA_ORA_CESTINATO,
                    ESEGUIBILE_DA_DATA = taskInCorso.ESEGUIBILE_DA_DATA,
                    ESEGUIBILE_FINO_A_DATA = new DateTime(9999, 12, 31),
                    ID_RICHIESTA = taskInCorso.ID_RICHIESTA,
                    ID_TABELLA_OUTPUT = taskInCorso.ID_TABELLA_OUTPUT,
                    ID_TASK = taskInCorso.ID_TASK,
                    MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                    INPUT = TestoTracciato,
                    PROGRESSIVO = taskInCorso.PROGRESSIVO,
                    NOTE = taskInCorso.NOTE,
                    MESE_ANNO_PAGATO = taskInCorso.MESE_ANNO_PAGATO,
                    MESE_ANNO_PAGATO_JSON = taskInCorso.MESE_ANNO_PAGATO_JSON,
                    RIMBORSO_TRACCIATO = id
                };

                db.XR_MAT_TASK_IN_CORSO.Add(TaskNew);
                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        public static List<CampoContent> ModificaCampiPerStorno(List<CampoContent> ListaCampi)
        {
            //IV DEL CODICE:2,TIPO CEDOLINO:*,RICHIAMO ANAGRAFICA:1
            string par = CommonHelper.GetParametro<string>(EnumParametriSistema.CampiStornoTracciato);
            if (String.IsNullOrWhiteSpace(par)) return ListaCampi;
            foreach (string sost in par.Split(','))
            {
                string nome = sost.Split(':')[0];
                string valore = sost.Split(':')[1];
                var campoDaModificare = ListaCampi.Where(x => x.NomeCampo == nome).FirstOrDefault();

                if (campoDaModificare != null)
                    campoDaModificare.ContenutoCampo = valore.Replace("*", " ");
            }
            var AAcompetenza = ListaCampi.Where(x => x.NomeCampo == "AA COMPETENZA").FirstOrDefault();
            var MMcompetenza = ListaCampi.Where(x => x.NomeCampo == "MM COMPETENZA").FirstOrDefault();
            if (AAcompetenza != null && MMcompetenza != null)
            {
                AAcompetenza.ContenutoCampo = DateTime.Now.ToString("yy");
                MMcompetenza.ContenutoCampo = DateTime.Now.ToString("MM");

            }
            return ListaCampi;
        }
        public static string RebuildTracciato(int idtracciato, int progressivo, List<CampoContent> ListaCampi)
        {
            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew s = new MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew();

            s.Credentials = new System.Net.NetworkCredential(
            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.CampiTracciatoResponse response =
                s.GetCampiTracciato(idtracciato, progressivo);

            TracciatoFactory TF = new TracciatoFactory();
            TracciatoGenerico Tracciato = TF.GetTracciatoClass(idtracciato, response, null);

            Tracciato.RebuildRecord(ListaCampi);
            return Tracciato.TestoTracciato;
        }
        private static List<TaskPronto> GetTaskProntiDaElaborazione(int IdRichiesta, TaskModel model)
        {
            var db = new IncentiviEntities();
            var Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == IdRichiesta).FirstOrDefault();
            List<TaskPronto> ListaTaskPronti = new List<TaskPronto>();


            DateTime? DataInizioPratica = model.Richiesta.INIZIO_GIUSTIFICATIVO;
            if (DataInizioPratica == null)
            {
                DataInizioPratica = model.Richiesta.DATA_INIZIO_MATERNITA;

            }

            foreach (var dataprimo in model.DateRiferimentoPrimoDelMeseTaskNecessari)
            {
                bool EsisteStornoInQuestoMese = false;

                double? Giorni26miDaFrontEnd = null;


                if (model.Giorni26FrontEnd != null)
                //     if (SonoDaAggiungereGiorniForzati(model.Richiesta))
                {
                    Giorni26miDaFrontEnd = model.Giorni26FrontEnd.Where(x => x.PrimoDelMese == dataprimo)
                        .Select(x => x.Giorni26mi).FirstOrDefault();
                    if (Giorni26miDaFrontEnd != null)
                    {
                        var giornimese = model.DettaglioAmmModel.ElencoGiorniPerMese.Where(x => x.RiferimentoPrimoDelMese == dataprimo)
                            .FirstOrDefault();
                        if (giornimese != null)
                        {
                            if (giornimese.ElencoGiorni.Any(x => x.IsMTfiller == true))
                            {
                                Giorni26miDaFrontEnd = null;
                            }
                        }
                    }

                }

                bool MeseHaTracciati = false;
                if (Richiesta.PIANIFICAZIONE_BASE_ORARIA == true)
                {
                    MeseHaTracciati = model.DettaglioAmmModel.ElencoGiorniPerMese
                  .Where(x => x.RiferimentoPrimoDelMese == dataprimo &&
                   x.ElencoGiorni.Any(z =>
                   z.CodiceEccezione == model.EccezioneRisultante + "M" ||
                   z.CodiceEccezione == model.EccezioneRisultante + "P" ||
                   z.CodiceEccezione == model.EccezioneRisultante + "Q")).Count() > 0;
                }
                else
                {
                    MeseHaTracciati = model.DettaglioAmmModel.ElencoGiorniPerMese
                  .Where(x => x.RiferimentoPrimoDelMese == dataprimo &&
                   x.ElencoGiorni.Any(z =>
                   z.CodiceEccezione == model.EccezioneRisultante)).Count() > 0;

                    if (!MeseHaTracciati && model.EccezioneRisultante == "MT")
                        MeseHaTracciati = model.DettaglioAmmModel.ElencoGiorniPerMese
                                            .Where(x => x.RiferimentoPrimoDelMese == dataprimo &&
                                            x.ElencoGiorni.Any(z =>
                                            z.CodiceEccezione == "MU")).Count() > 0;
                }



                ////// STORNO CEDOLINO -------------------------------------------------------------------------
                bool NecessariaModificaListoneStorni = false;

                XR_MAT_ELENCO_TASK TaskElenco = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK
                    .Where(x => x.STATO_PRATICA == 100)
                    .Select(x => x.XR_MAT_ELENCO_TASK).Where(z => z.NOME_TASK.Trim() == "STORNO CEDOLINO").FirstOrDefault();

                var list = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK.Where(x => x.STATO_PRATICA == 100).ToList();
                if (TaskElenco != null)
                {
                    XR_MAT_CATEGORIA_TASK catTask = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK.Where(x => x.STATO_PRATICA == 100
                    && x.ID_TASK == TaskElenco.ID)
                        .FirstOrDefault();

                    bool NecessarioStornoCedolino = dataprimo < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    //NecessarioStornoCedolino = true;//tbdmax
                    if (MeseHaTracciati && NecessarioStornoCedolino)
                    {
                        EsisteStornoInQuestoMese = true;

                        TaskPronto TP = new TaskPronto()
                        {
                            DataRiferimentoMeseAnno = dataprimo
                        };
                        XR_MAT_TASK_IN_CORSO taskGiaSalvatoInAltrePratiche =
                        GetTaskGiaSalvatoAltrePratiche(TaskElenco.ID, IdRichiesta, dataprimo.Month, dataprimo.Year,
                        "",
                        "");
                        if (taskGiaSalvatoInAltrePratiche != null)
                            TP.IdAltraPraticaGiaSuDB = taskGiaSalvatoInAltrePratiche.ID_RICHIESTA;






                        var listapaga = AmministrazioneManager.GetPagamenti(Richiesta.MATRICOLA, "", dataprimo);
                        var item = listapaga.elencoVoci.Where(x => x.Descrittiva != null && x.Descrittiva.ToLower()
                                  .StartsWith(Richiesta.ECCEZIONE.ToLower())).FirstOrDefault();
                        if (item != null && !String.IsNullOrWhiteSpace(item.Descrittiva) && item.Descrittiva.Contains("/"))
                        {
                            string[] seg = item.Descrittiva.Replace(Richiesta.ECCEZIONE, "").Split('/').Select(x => x.Trim()).ToArray();
                            DateTime D2;
                            if (DateTime.TryParseExact(seg[1], "dd-MM-yy", null, DateTimeStyles.None, out D2))
                            {
                                int d;
                                if (Int32.TryParse(seg[0], out d))
                                {
                                    DateTime D1 = new DateTime(D2.Year, D2.Month, d);

                                    var listGiorniTracciato = model.DettaglioAmmModel.ElencoGiorniPerMese
                                          .Where(x => x.RiferimentoPrimoDelMese == dataprimo &&
                                           x.ElencoGiorni.Any(z =>
                                           z.CodiceEccezione == Richiesta.ECCEZIONE)).ToList();
                                    foreach (var giorniTracciato in listGiorniTracciato)
                                    {
                                        if (NecessariaModificaListoneStorni) break;

                                        foreach (var g in giorniTracciato.ElencoGiorni)
                                        {
                                            if (g.DataDa != null)
                                            {
                                                if (g.DataDa >= D1 && g.DataDa <= D2)
                                                {
                                                    NecessariaModificaListoneStorni = true;
                                                    break;
                                                }
                                            }
                                            if (g.DataA != null)
                                            {
                                                if (g.DataA >= D1 && g.DataA <= D2)
                                                {
                                                    NecessariaModificaListoneStorni = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }





                        string testoTracciato = MaternitaCongediManager.GetCampiTracciato(
                          (int)TaskElenco.ID_TRACCIATO_DEW,
                          (int)TaskElenco.PROGRESSIVO_TRACCIATO_DEW,
                          Richiesta,
                          model.EccezioneRisultante,
                          dataprimo.ToString("dd/MM/yyyy"),
                          dataprimo.ToString("dd/MM/yyyy"),
                          "0",
                          DataInizioPratica.Value,
                          model.DettaglioAmmModel.ImportoFinale.ToString(),
                          "", "", ""
                          );

                        XR_MAT_TASK_IN_CORSO TaskInCorsoEntity = new XR_MAT_TASK_IN_CORSO()
                        {
                            ANNO = dataprimo.Year,
                            MESE = dataprimo.Month,
                            DATA_CREAZIONE = DateTime.Now,
                            INPUT = testoTracciato,
                            MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                            XR_MAT_ELENCO_TASK = TaskElenco,
                            XR_MAT_RICHIESTE = Richiesta,
                            PROGRESSIVO = (int)catTask.PROGRESSIVO,
                            ID_RICHIESTA = Richiesta.ID,
                            ID_TASK = TaskElenco.ID,
                            BLOCCATA_DATETIME = DateTime.Now,//sempre sospeso storno cedolino
                            BLOCCATA_DA_OPERATORE = CommonHelper.GetCurrentUserMatricola()

                        };

                        TP.TaskInCorso = TaskInCorsoEntity;
                        List<CampoContent> ListaCampi = new List<CampoContent>();
                        ListaCampi = GetTracciatoEsploso(
                                  (int)TaskElenco.ID_TRACCIATO_DEW,
                                  (int)TaskElenco.PROGRESSIVO_TRACCIATO_DEW,
                                  TaskInCorsoEntity.INPUT);
                        //var mc = ListaCampi.Where(x => x.NomeCampo == "MM COMPETENZA").FirstOrDefault();
                        //if (mc != null)
                        //{
                        //    mc.ContenutoCampo = DateTime.Now.Month.ToString().PadLeft(2, '0');
                        //}
                        TP.TracciatoEsplosoModel = new TaskTracciatoExpModel();
                        TP.TracciatoEsplosoModel.TracciatoEsploso = new ContenutoCampiPerMeseTask()
                        {
                            Campi = ListaCampi
                        };
                        TP.TracciatoEsplosoModel.TracciatoEsploso.TracciatoIntero = TaskInCorsoEntity.INPUT;
                        TP.TracciatoEsplosoModel.EditPermesso = TP.IdAltraPraticaGiaSuDB == 0;
                        ListaTaskPronti.Add(TP);

                    }
                }

                ////// MODIFICA LISTONE STORNI ----------------------------------------------------------------------
                if (NecessariaModificaListoneStorni)
                {

                    TaskElenco = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK
                   .Where(x => x.STATO_PRATICA == 100)
                   .Select(x => x.XR_MAT_ELENCO_TASK).Where(z => z.NOME_TASK.Trim() == "MODIFICA LISTONE STORNI").FirstOrDefault();
                    if (TaskElenco != null)
                    {
                        XR_MAT_CATEGORIA_TASK catTask = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK.Where(x => x.STATO_PRATICA == 100 && x.ID_TASK == TaskElenco.ID)
                       .FirstOrDefault();
                        TaskPronto TP = new TaskPronto()
                        {
                            DataRiferimentoMeseAnno = dataprimo
                        };
                        XR_MAT_TASK_IN_CORSO taskGiaSalvatoInAltrePratiche =
                       GetTaskGiaSalvatoAltrePratiche(TaskElenco.ID, IdRichiesta, dataprimo.AddMonths(1).Month, dataprimo.AddMonths(1).Year, "", "");
                        if (taskGiaSalvatoInAltrePratiche != null)
                            TP.IdAltraPraticaGiaSuDB = taskGiaSalvatoInAltrePratiche.ID_RICHIESTA;


                        XR_MAT_TASK_IN_CORSO TaskInCorsoEntity = new XR_MAT_TASK_IN_CORSO()
                        {
                            ANNO = dataprimo.Year,
                            MESE = dataprimo.Month,
                            DATA_CREAZIONE = DateTime.Now,
                            MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                            XR_MAT_ELENCO_TASK = TaskElenco,
                            XR_MAT_RICHIESTE = Richiesta,
                            PROGRESSIVO = (int)catTask.PROGRESSIVO,
                            ID_RICHIESTA = Richiesta.ID,
                            ID_TASK = TaskElenco.ID,
                            BLOCCATA_DATETIME = DateTime.Now,
                            BLOCCATA_DA_OPERATORE = CommonHelper.GetCurrentUserMatricola()

                        };
                        MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.Tracciato_richiamato[] InfoTracciati =
                            MaternitaCongediManager.GetRecordRewDaCancellare(
                                        Richiesta.MATRICOLA + dataprimo.ToString("yyMM"),
                                        "",//Richiesta.ECCEZIONE, 
                                        "POSTD9701",
                                        dataprimo);

                        if (InfoTracciati != null && InfoTracciati.Any())
                        {
                            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.Tracciato_richiamato[]
                            TracciatiDaCancellare = myRaiCommonTasks.CommonTasks.GetRecordRewDaCancellare(InfoTracciati, Richiesta, dataprimo);


                            if (TracciatiDaCancellare != null && TracciatiDaCancellare.Any())
                            {
                                TP.TracciatiRew = TracciatiDaCancellare.Distinct().OrderBy(x => x.Testo_record).ToArray();
                            }
                        }

                        TP.TaskInCorso = TaskInCorsoEntity;
                        ListaTaskPronti.Add(TP);
                    }


                }

                ////// AGGIORNAMENTO PAGAMENTO ECCEZIONI -------------------------------------------------------------------------

                TaskElenco = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK
                    .Where(x => x.STATO_PRATICA == 100)
                    .Select(x => x.XR_MAT_ELENCO_TASK).Where(z => z.NOME_TASK.Trim() == "AGGIORNAMENTO PAGAMENTO ECCEZIONI").FirstOrDefault();
                if (TaskElenco != null)
                {
                    XR_MAT_CATEGORIA_TASK catTask = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK.Where(x => x.STATO_PRATICA == 100 && x.ID_TASK == TaskElenco.ID)
                       .FirstOrDefault();

                    if (MeseHaTracciati)
                    {
                        TaskPronto TP = new TaskPronto()
                        {
                            DataRiferimentoMeseAnno = dataprimo.AddMonths(1)
                        };
                        XR_MAT_TASK_IN_CORSO taskGiaSalvatoInAltrePratiche =
                       GetTaskGiaSalvatoAltrePratiche(TaskElenco.ID, IdRichiesta, dataprimo.AddMonths(1).Month, dataprimo.AddMonths(1).Year, "", "");
                        if (taskGiaSalvatoInAltrePratiche != null)
                            TP.IdAltraPraticaGiaSuDB = taskGiaSalvatoInAltrePratiche.ID_RICHIESTA;

                        XR_MAT_TASK_IN_CORSO TaskInCorsoEntity = new XR_MAT_TASK_IN_CORSO()
                        {
                            ANNO = dataprimo.AddMonths(1).Year,
                            MESE = dataprimo.AddMonths(1).Month,
                            DATA_CREAZIONE = DateTime.Now,
                            MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                            XR_MAT_ELENCO_TASK = TaskElenco,
                            XR_MAT_RICHIESTE = Richiesta,
                            PROGRESSIVO = (int)catTask.PROGRESSIVO,
                            ID_RICHIESTA = Richiesta.ID,
                            ID_TASK = TaskElenco.ID

                        };
                        TP.TaskInCorso = TaskInCorsoEntity;
                        ListaTaskPronti.Add(TP);
                    }
                }


                ////// GESTIONE MU
                if (model.EccezioneRisultante == "MT")
                {
                    DettaglioGiorniPerMese meserif = model.DettaglioAmmModel.ElencoGiorniPerMese.Where(x => x.RiferimentoPrimoDelMese == dataprimo && x.ElencoGiorni != null && x.ElencoGiorni.Any()).FirstOrDefault();
                    //meserifnull
                    if (meserif != null && meserif.ElencoGiorni.Any(x => x.CodiceEccezione == "MU"))
                    {
                        List<DettaglioGiorniModel> PeriodiEccezione = meserif.ElencoGiorni.Where(x => x.CodiceEccezione == "MU").OrderBy(x => x.DataDa).ToList();
                        DateTime dataDa = PeriodiEccezione.Min(x => x.DataDa);
                        DateTime dataA = PeriodiEccezione.Max(x => x.DataDa);


                        string dataInizioNote = dataDa.ToString("dd/MM/yyyy");
                        string dataFineNote = dataA.ToString("dd/MM/yyyy");
                        TaskPronto TP = new TaskPronto()
                        {
                            DataRiferimentoMeseAnno = dataprimo,
                            DicituraPeriodo = dataInizioNote + " - " + dataFineNote,
                            NumeroFusioni = 0,
                            IntervalliFusi = new List<string>(),
                            PeriodoDa = dataDa,
                            PeriodoA = dataA
                        };

                        string g26 = PeriodiEccezione.Where(x => x.DataDa.DayOfWeek != DayOfWeek.Sunday).Count().ToString();

                        // vedi pdf calendario 26 per maggiori dettagli-----perche cosi ????? 13012022
                        //if (DateTime.DaysInMonth(dataA.Year, dataA.Month) == dataA.Day &&
                        //    DateTime.DaysInMonth(dataA.Year, dataA.Month) == 31)
                        //    g26 = (PeriodiEccezione.Where(x => x.DataDa.DayOfWeek != DayOfWeek.Sunday).Count() - 1).ToString();


                        if (Giorni26miDaFrontEnd != null)
                            g26 = Giorni26miDaFrontEnd.ToString();

                        if (model.DettaglioAmmModel.FormaContratto != "K" &&
                            dataDa.Day > 1 && dataA.Day == DateTime.DaysInMonth(dataDa.Year, dataDa.Month))
                        {

                            int? g26CalendarioPDF = MaternitaCongediManager.GetGiorni26ComeCalendarioPDF(model.Richiesta.MATRICOLA, dataDa);
                            if (g26CalendarioPDF != null)
                                g26 = g26CalendarioPDF.ToString();
                        }

                        string testoTracciato = MaternitaCongediManager.GetCampiTracciato(
                          892,
                          2,
                          Richiesta,
                          "MU",
                          dataDa.ToString("dd/MM/yyyy"),
                          dataA.ToString("dd/MM/yyyy"),
                          g26,
                          DataInizioPratica.Value,
                           model.DettaglioAmmModel.ImportoFinale.ToString(),
                          model.Importo13ma,
                          model.Importo14ma,
                          model.ImportoPremio,
                          null,
                          "MATERNITA MU 9000"
                          );
                        XR_MAT_TASK_IN_CORSO TaskInCorsoEntity = new XR_MAT_TASK_IN_CORSO()
                        {
                            ANNO = dataprimo.Year,
                            MESE = dataprimo.Month,
                            DATA_CREAZIONE = DateTime.Now,
                            INPUT = testoTracciato,
                            MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                            XR_MAT_ELENCO_TASK = db.XR_MAT_ELENCO_TASK.Find(8),
                            XR_MAT_RICHIESTE = Richiesta,
                            PROGRESSIVO = 40,
                            ID_RICHIESTA = Richiesta.ID,
                            ID_TASK = 8
                        };

                        TP.TaskInCorso = TaskInCorsoEntity;
                        List<CampoContent> ListaCampi = new List<CampoContent>();
                        ListaCampi = GetTracciatoEsploso(
                                  892,
                                  2,
                                  TaskInCorsoEntity.INPUT);
                        if (EsisteStornoInQuestoMese)
                        {
                            var mc = ListaCampi.Where(x => x.NomeCampo == "MM COMPETENZA").FirstOrDefault();
                            if (mc != null)
                            {
                                mc.ContenutoCampo = DateTime.Now.Month.ToString().PadLeft(2, '0');
                            }
                        }
                        TP.TracciatoEsplosoModel = new TaskTracciatoExpModel();
                        TP.TracciatoEsplosoModel.TracciatoEsploso = new ContenutoCampiPerMeseTask()
                        {
                            Campi = ListaCampi
                        };
                        TP.TracciatoEsplosoModel.TracciatoEsploso.TracciatoIntero = TaskInCorsoEntity.INPUT;
                        TP.TracciatoEsplosoModel.EditPermesso = TP.IdAltraPraticaGiaSuDB == 0;
                        ListaTaskPronti.Add(TP);

                    }
                }

                ////// ASSENZA FACOLTATIVA  AF-CF-DF-DK 9000 -------------------------------------------------------------------------
                string ecc = GetEccezioneRisultante(Richiesta);
                int statoUscitaPratica = 100;
                string nomeTask = "ASSENZA FACOLTATIVA  AF-CF-DF-DK 9000";
                if (ecc == "BF") ///BF corretto
                {
                    statoUscitaPratica = 101;
                    nomeTask = "ASSENZE BF-NI 9000";
                }
                string ProgrFamiliare = "  ";
                if (Richiesta.PIANIFICAZIONE_BASE_ORARIA == true)
                {
                    ProgrFamiliare = GetCodiceFiscaleInfo(model.Richiesta.CF_BAMBINO, model.Richiesta);
                    if (String.IsNullOrWhiteSpace(ProgrFamiliare) || ProgrFamiliare == "00")
                        ProgrFamiliare = "  ";
                }

                TaskElenco = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK
                  .Where(x => x.STATO_PRATICA == statoUscitaPratica)
                  .Select(x => x.XR_MAT_ELENCO_TASK).Where(z => z.NOME_TASK.Trim() == nomeTask).FirstOrDefault();
                if (TaskElenco != null)
                {
                    XR_MAT_CATEGORIA_TASK catTask = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK.Where(x =>
                    x.STATO_PRATICA == statoUscitaPratica && x.ID_TASK == TaskElenco.ID)
                       .FirstOrDefault();

                    DettaglioGiorniPerMese meserif = model.DettaglioAmmModel.ElencoGiorniPerMese.Where(x => x.RiferimentoPrimoDelMese == dataprimo).FirstOrDefault();
                    if (meserif == null) continue;

                    List<DettaglioGiorniModel> PeriodiEccezione = new List<DettaglioGiorniModel>();
                    if (model.Richiesta.PIANIFICAZIONE_BASE_ORARIA == true)
                    {
                        PeriodiEccezione = meserif.ElencoGiorni.Where(x =>
                        x.CodiceEccezione == model.EccezioneRisultante + "M" ||
                        x.CodiceEccezione == model.EccezioneRisultante + "P" ||
                        x.CodiceEccezione == model.EccezioneRisultante + "Q").ToList();
                    }
                    else
                    {
                        PeriodiEccezione = meserif.ElencoGiorni.Where(x => x.CodiceEccezione == model.EccezioneRisultante).ToList();
                    }

                    if (PeriodiEccezione.Count > 1)
                    {
                        if (Richiesta.PIANIFICAZIONE_BASE_ORARIA != true)
                        {
                            PeriodiEccezione = FondiPeriodiEccezione(PeriodiEccezione, Richiesta.MATRICOLA,
                                                Richiesta.XR_MAT_CATEGORIE.SKIP_FUSIONE_PERIODI, model.DettaglioAmmModel.FormaContratto);
                            meserif.ElencoGiorni.RemoveAll(x => x.CodiceEccezione == model.EccezioneRisultante);
                            meserif.ElencoGiorni.AddRange(PeriodiEccezione);
                        }

                    }
                    foreach (var periodoEccezione in PeriodiEccezione)
                    {
                        if (periodoEccezione.DataA == null) periodoEccezione.DataA = periodoEccezione.DataDa;

                        string dataInizioNote = periodoEccezione.DataDa.ToString("dd/MM/yyyy");
                        string dataFineNote = periodoEccezione.DataA.Value.ToString("dd/MM/yyyy");
                        double GiorniAggiuntiPonticelli = 0;

                        if (Richiesta.XR_MAT_CATEGORIE.SKIP_FUSIONE_PERIODI != true)
                        {
                            //   myRaiCommonModel.AmministrazioneModel.BustaPaga res1 = myRaiCommonManager.AmministrazioneManager.GetPagamenti(model.Richiesta.MATRICOLA, model.EccezioneRisultante + periodoEccezione.DataDa.ToString("dd") + "/" + periodoEccezione.DataA.Value.ToString("dd-M-yyyy"), periodoEccezione.DataDa);

                            GetSchedaPresenzeMeseResponse timbr = GetDatiPeriodo(periodoEccezione.DataDa, Richiesta.MATRICOLA);
                            DateTime? DateChanged = ControllaSabDomInizio(periodoEccezione.DataDa, periodoEccezione.DataA,  Richiesta, timbr, model.DettaglioAmmModel.FormaContratto);
                            if (DateChanged != null)
                            {
                                GiorniAggiuntiPonticelli += (periodoEccezione.DataDa - DateChanged.Value).TotalDays;
                                periodoEccezione.DataDa = DateChanged.Value;
                                dataInizioNote = periodoEccezione.DataDa.ToString("dd/MM/yyyy");

                            }


                            DateTime? DateChanged2 = ControllaSabDomFine(periodoEccezione.DataDa,periodoEccezione.DataA, Richiesta, timbr, model.DettaglioAmmModel.FormaContratto);
                            if (DateChanged2 != null)
                            {
                                GiorniAggiuntiPonticelli += (DateChanged2.Value - periodoEccezione.DataA.Value).TotalDays;
                                periodoEccezione.DataA = DateChanged2.Value;
                                dataFineNote = periodoEccezione.DataA.Value.ToString("dd/MM/yyyy");

                            }
                        }



                        TaskPronto TP = new TaskPronto()
                        {
                            DataRiferimentoMeseAnno = dataprimo,
                            DicituraPeriodo = dataInizioNote + " - " + dataFineNote,
                            NumeroFusioni = periodoEccezione.Fusioni,
                            IntervalliFusi = periodoEccezione.IntervalliFusi,
                            PeriodoDa = periodoEccezione.DataDa,
                            PeriodoA = periodoEccezione.DataA != null ? periodoEccezione.DataA.Value : periodoEccezione.DataDa
                        };

                        //   if (res1.elencoVoci.Count() > 0)
                        //       TP.DicituraPeriodo = TP.DicituraPeriodo + " PAGATO " + res1.dataCompetenza;

                        XR_MAT_TASK_IN_CORSO taskGiaSalvatoInAltrePratiche =
                      GetTaskGiaSalvatoAltrePratiche(TaskElenco.ID, IdRichiesta, dataprimo.Month, dataprimo.Year, dataInizioNote, dataFineNote);

                        if (taskGiaSalvatoInAltrePratiche != null)
                            TP.IdAltraPraticaGiaSuDB = taskGiaSalvatoInAltrePratiche.ID_RICHIESTA;

                        string gior26 = periodoEccezione.NumeroGiorniRuoli > 0 ?
                          periodoEccezione.NumeroGiorniRuoli.ToString() : periodoEccezione.NumeroGiorniGapp.ToString();
                        if (Giorni26miDaFrontEnd != null && PeriodiEccezione.Count == 1)
                            gior26 = Giorni26miDaFrontEnd.ToString();

                        //if (GiorniAggiuntiPonticelli > 0)
                        //{
                        //    if (Richiesta.ASSENZA_LUNGA != true)
                        //    {
                        //        GiorniAggiuntiPonticelli *= 1.2;
                        //    }
                        //    double diff = Convert.ToDouble(gior26) + GiorniAggiuntiPonticelli;
                        //    if (diff > 26) diff = 26;
                        //    gior26 = (diff).ToString();
                        //}
                        string testoTracciato = MaternitaCongediManager.GetCampiTracciato(
                          (int)TaskElenco.ID_TRACCIATO_DEW,
                          (int)TaskElenco.PROGRESSIVO_TRACCIATO_DEW,
                          Richiesta,
                          model.EccezioneRisultante,
                          periodoEccezione.DataDa.ToString("dd/MM/yyyy"),
                          periodoEccezione.DataA.Value.ToString("dd/MM/yyyy"),

                           gior26,
                          DataInizioPratica.Value,
                           model.DettaglioAmmModel.ImportoFinale.ToString(),
                          model.Importo13ma,
                          model.Importo14ma,
                          model.ImportoPremio,
                          ProgrFamiliare
                          );
                        XR_MAT_TASK_IN_CORSO TaskInCorsoEntity = new XR_MAT_TASK_IN_CORSO()
                        {
                            ANNO = dataprimo.Year,
                            MESE = dataprimo.Month,
                            DATA_CREAZIONE = DateTime.Now,
                            INPUT = testoTracciato,
                            MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                            XR_MAT_ELENCO_TASK = TaskElenco,
                            XR_MAT_RICHIESTE = Richiesta,
                            PROGRESSIVO = (int)catTask.PROGRESSIVO,
                            ID_RICHIESTA = Richiesta.ID,
                            ID_TASK = TaskElenco.ID

                        };

                        TP.TaskInCorso = TaskInCorsoEntity;
                        List<CampoContent> ListaCampi = new List<CampoContent>();
                        ListaCampi = GetTracciatoEsploso(
                                  (int)TaskElenco.ID_TRACCIATO_DEW,
                                  (int)TaskElenco.PROGRESSIVO_TRACCIATO_DEW,
                                  TaskInCorsoEntity.INPUT);
                        if (EsisteStornoInQuestoMese)
                        {
                            var mc = ListaCampi.Where(x => x.NomeCampo == "MM COMPETENZA").FirstOrDefault();
                            if (mc != null)
                            {
                                mc.ContenutoCampo = DateTime.Now.Month.ToString().PadLeft(2, '0');
                            }

                            var ac = ListaCampi.Where(x => x.NomeCampo == "AA COMPETENZA").FirstOrDefault();
                            if (ac != null)
                            {
                                ac.ContenutoCampo = DateTime.Now.Year.ToString().Substring(2);
                            }
                        }
                        TP.TracciatoEsplosoModel = new TaskTracciatoExpModel();
                        TP.TracciatoEsplosoModel.TracciatoEsploso = new ContenutoCampiPerMeseTask()
                        {
                            Campi = ListaCampi
                        };
                        TP.TracciatoEsplosoModel.TracciatoEsploso.TracciatoIntero = TaskInCorsoEntity.INPUT;
                        TP.TracciatoEsplosoModel.EditPermesso = TP.IdAltraPraticaGiaSuDB == 0;
                        ListaTaskPronti.Add(TP);
                    }
                }

                ////// CONGEDO PATERNO MG-MV 9000 -------------------------------------------------------------------------
                ////// MATERNITA MT 9000 -------------------------------------------------------------------------
                ////// MATERNITA MU 9000 -------------------------------------------------------------------------
                ////// ARRETRATI E RECUPERI 9000 -------------------------------------------------------------------------
                ////// DESCRITTIVA -------------------------------------------------------------------------
                ////// ASSENZA FACOLTATIVA HF 9000 -------------------------------------------------------------------------

                statoUscitaPratica = 100;
                List<string> Tracciati = new List<string>() {
                    "CONGEDO PATERNO MG-MV 9000",
                    "MATERNITA MT 9000",
                    "MATERNITA MU 9000",
                    "ARRETRATI E RECUPERI 9000",
                    "DESCRITTIVA",
                    "ASSENZA FACOLTATIVA HF 9000",
                    "HANDICAP GRAVE 9000",
                    "ASSENZE GM 9000",
                    "CONGEDO PARENT. 80% AW 9000"
                };
                foreach (string nome in Tracciati)
                {
                    nomeTask = nome;
                    TaskElenco = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK
                                 .Where(x => x.STATO_PRATICA == statoUscitaPratica)
                                 .Select(x => x.XR_MAT_ELENCO_TASK).Where(z => z.NOME_TASK.Trim() == nomeTask).FirstOrDefault();
                    if (TaskElenco != null)
                    { 

                        XR_MAT_CATEGORIA_TASK catTask = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK.Where(x =>
                        x.STATO_PRATICA == statoUscitaPratica && x.ID_TASK == TaskElenco.ID)
                           .FirstOrDefault();

                         
                        DettaglioGiorniPerMese meserif = model.DettaglioAmmModel.ElencoGiorniPerMese.Find(x => x.RiferimentoPrimoDelMese == dataprimo
                                                                                                                 && x.ElencoGiorni != null && x.ElencoGiorni.Any());

                        if (meserif == null) continue;

                        List<DettaglioGiorniModel> PeriodiEccezione = meserif.ElencoGiorni.Where(x => x.CodiceEccezione == model.EccezioneRisultante).ToList();
                        if (PeriodiEccezione.Count > 1)
                        {
                            if (Richiesta.PIANIFICAZIONE_BASE_ORARIA != true)
                            {
                                PeriodiEccezione = FondiPeriodiEccezione(PeriodiEccezione, Richiesta.MATRICOLA, Richiesta.XR_MAT_CATEGORIE.SKIP_FUSIONE_PERIODI, model.DettaglioAmmModel.FormaContratto);
                                meserif.ElencoGiorni.RemoveAll(x => x.CodiceEccezione == model.EccezioneRisultante);
                                meserif.ElencoGiorni.AddRange(PeriodiEccezione);
                            }
                        }
                        bool PrimoEseguito = false;
                        bool IsDescrittiva = false;
                        String testoDescrittiva = "";
                        //if (Richiesta.XR_MAT_CATEGORIE.SKIP_FUSIONE_PERIODI != true)
                        //{
                        //    foreach (var periodoEccezione in PeriodiEccezione)
                        //    {
                        //        if (periodoEccezione.DataA == null) periodoEccezione.DataA = periodoEccezione.DataDa;
                        //        GetSchedaPresenzeMeseResponse timbr = GetDatiPeriodo(periodoEccezione, Richiesta);
                        //        DateTime? DateChanged = ControllaSabDomInizio(periodoEccezione, Richiesta, timbr, model.DettaglioAmmModel.FormaContratto);
                        //        if (DateChanged != null)
                        //        {
                        //            periodoEccezione.DataDa = DateChanged.Value;
                        //            //dataInizioNote = periodoEccezione.DataDa.ToString("dd/MM/yyyy");
                        //        }
                        //        DateTime? DateChanged2 = ControllaSabDomFine(periodoEccezione, Richiesta, timbr, model.DettaglioAmmModel.FormaContratto);
                        //        if (DateChanged2 != null)
                        //        {
                        //            periodoEccezione.DataA = DateChanged2.Value;
                        //            //dataFineNote = periodoEccezione.DataA.Value.ToString("dd/MM/yyyy");
                        //        }
                        //    }
                        //    var listCheckSovrapposizioni = PeriodiEccezione.Where(x => x.DataDa != null).ToList();
                        //    if (listCheckSovrapposizioni.Count > 1)
                        //    {
                        //        var p1 = listCheckSovrapposizioni.First();
                        //        var p2 = listCheckSovrapposizioni[1];
                        //        if ((p1.DataA >= p2.DataDa && p1.DataA <= p2.DataA) ||
                        //            (p2.DataA >= p1.DataDa && p2.DataA <= p1.DataA))
                        //        {
                        //            if (p2.DataDa < p1.DataDa)
                        //                p1.DataDa = p2.DataDa;
                        //            if (p2.DataA > p1.DataA)
                        //                p1.DataA = p2.DataA;
                        //            p1.NumeroGiorniRuoli += p2.NumeroGiorniRuoli;
                        //            p1.NumeroGiorniGapp += p2.NumeroGiorniGapp;
                        //            PeriodiEccezione.Remove(p2);
                        //        }
                        //    }
                        //}


                        foreach (var periodoEccezione in PeriodiEccezione)
                        {
                            if (Richiesta.XR_MAT_CATEGORIE.SKIP_FUSIONE_PERIODI != true && "AF,BF,CF".Contains(Richiesta.ECCEZIONE))
                            {
                                if (periodoEccezione.DataA == null) periodoEccezione.DataA = periodoEccezione.DataDa;
                                GetSchedaPresenzeMeseResponse timbr = GetDatiPeriodo(periodoEccezione.DataDa, Richiesta.MATRICOLA);

                                DateTime? DateChanged = ControllaSabDomInizio(periodoEccezione.DataDa, periodoEccezione.DataA, Richiesta, timbr, model.DettaglioAmmModel.FormaContratto);
                                if (DateChanged != null)
                                {
                                    periodoEccezione.DataDa = DateChanged.Value;
                                    //dataInizioNote = periodoEccezione.DataDa.ToString("dd/MM/yyyy");
                                }
                                DateTime? DateChanged2 = ControllaSabDomFine(periodoEccezione.DataDa,periodoEccezione.DataA, Richiesta, timbr, model.DettaglioAmmModel.FormaContratto);
                                if (DateChanged2 != null)
                                {
                                    periodoEccezione.DataA = DateChanged2.Value;
                                    //dataFineNote = periodoEccezione.DataA.Value.ToString("dd/MM/yyyy");
                                }
                            }

                            if (TaskElenco.NOME_TASK.ToUpper().Contains("DESCRITTIVA"))
                            {
                                double totGiorni = 0;
                                if (model.Richiesta.INIZIO_GIUSTIFICATIVO == null)
                                    totGiorni = ((DateTime)model.Richiesta.DATA_FINE_MATERNITA -
                                        (DateTime)model.Richiesta.DATA_INIZIO_MATERNITA).TotalDays;
                                else
                                    totGiorni = ((DateTime)model.Richiesta.FINE_GIUSTIFICATIVO -
                                           (DateTime)model.Richiesta.INIZIO_GIUSTIFICATIVO).TotalDays;

                                double giorni = GetGiorniDaRichiestaPrecedenteAdiacente(model.Richiesta);

                                double SogliaGiorni = CommonHelper.GetParametro<double>(EnumParametriSistema.SogliaGiorniPerTracciatoDescrittiva);
                                if (totGiorni + giorni < SogliaGiorni)
                                {
                                    break;
                                }
                                if (PrimoEseguito == true) break;
                                PrimoEseguito = true;
                                IsDescrittiva = true;

                                testoDescrittiva = GetEccezioneRisultante(model.Richiesta) + " ";
                                XR_MAT_RICHIESTE RichiestaPrecedente = GetRichiestaAdiacentePrecedente(model.Richiesta);

                                if (RichiestaPrecedente == null)
                                    RichiestaPrecedente = model.Richiesta;

                                if (RichiestaPrecedente.INIZIO_GIUSTIFICATIVO == null)
                                {
                                    testoDescrittiva += RichiestaPrecedente.DATA_INIZIO_MATERNITA.Value.ToString("dd-MM-yy")
                                        + "/" + model.Richiesta.DATA_FINE_MATERNITA.Value.ToString("dd-MM-yy");
                                }
                                else
                                    testoDescrittiva += RichiestaPrecedente.INIZIO_GIUSTIFICATIVO.Value.ToString("dd-MM-yy")
                                        + "/" + model.Richiesta.FINE_GIUSTIFICATIVO.Value.ToString("dd-MM-yy");

                            }
                            if (periodoEccezione.DataA == null) periodoEccezione.DataA = periodoEccezione.DataDa;

                            string dataInizioNote = periodoEccezione.DataDa.ToString("dd/MM/yyyy");
                            string dataFineNote = periodoEccezione.DataA.Value.ToString("dd/MM/yyyy");


                            //myRaiCommonModel.AmministrazioneModel.BustaPaga res1 = myRaiCommonManager.AmministrazioneManager.GetPagamenti(model.Richiesta.MATRICOLA, model.EccezioneRisultante + periodoEccezione.DataDa.ToString("dd") + "/" + periodoEccezione.DataA.Value.ToString("dd-M-yyyy"), periodoEccezione.DataDa);
                            TaskPronto TP = new TaskPronto()
                            {
                                DataRiferimentoMeseAnno = dataprimo,
                                DicituraPeriodo = IsDescrittiva ? testoDescrittiva : dataInizioNote + " - " + dataFineNote,
                                NumeroFusioni = IsDescrittiva ? 0 : periodoEccezione.Fusioni,
                                IntervalliFusi = IsDescrittiva ? new List<string>() :
                                                periodoEccezione.IntervalliFusi,
                                PeriodoDa = periodoEccezione.DataDa,
                                PeriodoA = periodoEccezione.DataA != null ? periodoEccezione.DataA.Value : periodoEccezione.DataDa
                            };
                            XR_MAT_TASK_IN_CORSO taskGiaSalvatoInAltrePratiche =
                          GetTaskGiaSalvatoAltrePratiche(TaskElenco.ID, IdRichiesta, dataprimo.Month, dataprimo.Year, dataInizioNote, dataFineNote);

                            if (taskGiaSalvatoInAltrePratiche != null)
                                TP.IdAltraPraticaGiaSuDB = taskGiaSalvatoInAltrePratiche.ID_RICHIESTA;

                            bool PortatoAFineMese = false;

                            if (model.EccezioneRisultante == "MT" &&
                                !String.IsNullOrWhiteSpace(CommonHelper.GetParametro<string>(EnumParametriSistema.SogliaConcludiMeseMT)))
                            {
                                int daySoglia = Convert.ToInt32(CommonHelper.GetParametro<string>(EnumParametriSistema.SogliaConcludiMeseMT));

                                if (periodoEccezione.DataA.Value.Day >= daySoglia && periodoEccezione.DataA.Value.Day !=
                                    DateTime.DaysInMonth(periodoEccezione.DataA.Value.Year, periodoEccezione.DataA.Value.Month)
                                    && meserif.ElencoGiorni.Where(x => x.CodiceEccezione == "MT" && x.DataDa.Month > periodoEccezione.DataDa.Month).Count() > 0)
                                {
                                    periodoEccezione.DataA = new DateTime(periodoEccezione.DataA.Value.Year,
                                        periodoEccezione.DataA.Value.Month,
                                        DateTime.DaysInMonth(periodoEccezione.DataA.Value.Year, periodoEccezione.DataA.Value.Month));

                                    PortatoAFineMese = true;

                                    dataInizioNote = periodoEccezione.DataDa.ToString("dd/MM/yyyy");
                                    dataFineNote = periodoEccezione.DataA.Value.ToString("dd/MM/yyyy");
                                    TP.DicituraPeriodo = IsDescrittiva ? testoDescrittiva : dataInizioNote + " - " + dataFineNote;
                                }
                            }
                            string g26 = periodoEccezione.NumeroGiorniRuoli > 0 ?
                              periodoEccezione.NumeroGiorniRuoli.ToString() : periodoEccezione.NumeroGiorniGapp.ToString();
                            if ((model.EccezioneRisultante == "MT" || model.EccezioneRisultante == "HF") && meserif != null && meserif.TotaleGiorni > 0)
                            {
                                var giorniTotMese = DateTime.DaysInMonth(periodoEccezione.DataDa.Year, periodoEccezione.DataDa.Month);
                                if ((periodoEccezione.DataA.Value - periodoEccezione.DataDa).TotalDays + 1 == giorniTotMese)
                                {
                                    var max = GetGiorniLimiteDaRichiestaDB(IdRichiesta);
                                    if (meserif.TotaleGiorni > (float)max)
                                        meserif.TotaleGiorni = (float)max;

                                    g26 = meserif.TotaleGiorni.ToString();
                                }
                            }


                            if (PortatoAFineMese) g26 = GetGiorniLimiteDaRichiestaDB(IdRichiesta).ToString();// "26";
                                                                                                             //     g26 = PeriodiEccezione.Where(x => x.DataDa.DayOfWeek != DayOfWeek.Sunday).Count().ToString();

                            if (Giorni26miDaFrontEnd != null && PeriodiEccezione.Count == 1)
                                g26 = Giorni26miDaFrontEnd.ToString();


                            //formacontratto8 importante
                            string ImportoFinale = model.DettaglioAmmModel.ImportoFinale.ToString();
                            if (model.DettaglioAmmModel.FormaContratto == "8")
                            {
                                if (TipoDipendente(model.Richiesta.MATRICOLA, DateTime.Today) == "G")
                                {
                                    //ImportoFinale = "00000000"; //id tracciato 844 dew
                                    //model.Importo13ma = "0000000";
                                    //model.Importo14ma = "0000000";
                                    //model.ImportoPremio = "0000000";
                                }


                            }
                            string testoTracciato = MaternitaCongediManager.GetCampiTracciato(
                              (int)TaskElenco.ID_TRACCIATO_DEW,
                              (int)TaskElenco.PROGRESSIVO_TRACCIATO_DEW,
                              Richiesta,
                              model.EccezioneRisultante,
                              periodoEccezione.DataDa.ToString("dd/MM/yyyy"),
                              periodoEccezione.DataA.Value.ToString("dd/MM/yyyy"),
                            //periodoEccezione.NumeroGiorniRuoli > 0 ?
                            //periodoEccezione.NumeroGiorniRuoli.ToString() : periodoEccezione.NumeroGiorniGapp.ToString(),
                            g26,

                              DataInizioPratica.Value,
                              ImportoFinale,
                              model.Importo13ma,
                              model.Importo14ma,
                              model.ImportoPremio,
                              null,
                              testoDescrittiva,
                              model.DettaglioAmmModel.TotaleGiornalieroHC, null, model.DettaglioAmmModel.FormaContratto

                              );
                            XR_MAT_TASK_IN_CORSO TaskInCorsoEntity = new XR_MAT_TASK_IN_CORSO()
                            {
                                ANNO = dataprimo.Year,
                                MESE = dataprimo.Month,
                                DATA_CREAZIONE = DateTime.Now,
                                INPUT = testoTracciato,
                                MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                                XR_MAT_ELENCO_TASK = TaskElenco,
                                XR_MAT_RICHIESTE = Richiesta,
                                PROGRESSIVO = (int)catTask.PROGRESSIVO,
                                ID_RICHIESTA = Richiesta.ID,
                                ID_TASK = TaskElenco.ID

                            };

                            TP.TaskInCorso = TaskInCorsoEntity;
                            List<CampoContent> ListaCampi = new List<CampoContent>();
                            ListaCampi = GetTracciatoEsploso(
                                      (int)TaskElenco.ID_TRACCIATO_DEW,
                                      (int)TaskElenco.PROGRESSIVO_TRACCIATO_DEW,
                                      TaskInCorsoEntity.INPUT);

                            CampoContent NuovoMMcompetenza = null;
                            CampoContent NuovoAAcompetenza = null;

                            if (PeriodoEccezioneMesePresenteOPassato(periodoEccezione))
                            //if (EsisteStornoInQuestoMese)
                            {
                                CampoContent mc = ListaCampi.Where(x => x.NomeCampo == "MM COMPETENZA").FirstOrDefault();
                                if (mc != null)
                                {
                                    string oldContenuto = mc.ContenutoCampo;
                                    mc.ContenutoCampo = DateTime.Now.Month.ToString().PadLeft(2, '0');
                                    if (mc.ContenutoCampo != oldContenuto)
                                    {
                                        NuovoMMcompetenza = mc;
                                    }
                                }
                                CampoContent ac = ListaCampi.Where(x => x.NomeCampo == "AA COMPETENZA").FirstOrDefault();
                                if (ac != null)
                                {
                                    string oldCont = ac.ContenutoCampo;
                                    ac.ContenutoCampo = DateTime.Now.Year.ToString().Substring(2);
                                    if (ac.ContenutoCampo != oldCont)
                                    {
                                        NuovoAAcompetenza = ac;
                                    }
                                }
                            }
                            TP.TracciatoEsplosoModel = new TaskTracciatoExpModel();
                            TP.TracciatoEsplosoModel.TracciatoEsploso = new ContenutoCampiPerMeseTask()
                            {
                                Campi = ListaCampi
                            };
                            TP.TracciatoEsplosoModel.TracciatoEsploso.TracciatoIntero = TaskInCorsoEntity.INPUT;
                            if (NuovoMMcompetenza != null)
                            {
                                TP.TracciatoEsplosoModel.TracciatoEsploso.TracciatoIntero =
                                    AggiornaMeseCompetenzaNelTracciato(TaskInCorsoEntity.INPUT, NuovoMMcompetenza);
                            }
                            if (NuovoAAcompetenza != null)
                            {
                                TP.TracciatoEsplosoModel.TracciatoEsploso.TracciatoIntero =
                                    AggiornaAnnoCompetenzaNelTracciato(TP.TracciatoEsplosoModel.TracciatoEsploso.TracciatoIntero, NuovoAAcompetenza);
                            }
                            TP.TracciatoEsplosoModel.EditPermesso = TP.IdAltraPraticaGiaSuDB == 0;
                            ListaTaskPronti.Add(TP);
                        }
                    }
                }
                if (NecessariaModificaListoneStorni && model.Richiesta.ASSENZA_LUNGA == true)
                {
                    TaskPronto TPstorico = InserisciTracciatoAssociatoListoneStorni(Richiesta, dataprimo, DataInizioPratica,
                        model, ListaTaskPronti);
                    if (TPstorico != null)
                    {
                        ListaTaskPronti.Add(TPstorico);
                    }
                }
            }

            return ListaTaskPronti;
        }

        private static bool PeriodoEccezioneMesePresenteOPassato(DettaglioGiorniModel periodoEccezione)
        {
            DateTime MeseCorrente = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime MesePeriodo = new DateTime(periodoEccezione.DataDa.Year, periodoEccezione.DataDa.Month, 1);
            return MesePeriodo <= MeseCorrente;
        }

        private static double GetGiorniDaRichiestaPrecedenteAdiacente(XR_MAT_RICHIESTE richiesta)
        {
            DateTime? Dinizio = richiesta.INIZIO_GIUSTIFICATIVO;
            if (Dinizio == null) Dinizio = richiesta.DATA_INIZIO_MATERNITA;

            DateTime DfineAdiacente = Dinizio.Value.AddDays(-1);
            double giorni = 0;

            var db = new IncentiviEntities();
            var RicAd = db.XR_MAT_RICHIESTE.Where(x => x.MATRICOLA == richiesta.MATRICOLA && x.ECCEZIONE == richiesta.ECCEZIONE
                       && (x.FINE_GIUSTIFICATIVO == DfineAdiacente || x.DATA_FINE_MATERNITA == DfineAdiacente)
                       && x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) <= 80
                       )
                        .FirstOrDefault();
            if (RicAd != null)
            {
                if (RicAd.INIZIO_GIUSTIFICATIVO != null)
                    giorni = (RicAd.FINE_GIUSTIFICATIVO.Value - RicAd.INIZIO_GIUSTIFICATIVO.Value).TotalDays + 1;
                else
                    giorni = (RicAd.DATA_FINE_MATERNITA.Value - RicAd.DATA_INIZIO_MATERNITA.Value).TotalDays + 1;
            }
            return giorni;

        }

        private static string AggiornaMeseCompetenzaNelTracciato(string iNPUT, CampoContent nuovoMMcompetenza)
        {
            string tr = iNPUT.Substring(0, nuovoMMcompetenza.PosizioneTracciato - 1) +
                nuovoMMcompetenza.ContenutoCampo +
                 iNPUT.Substring(nuovoMMcompetenza.PosizioneTracciato + 1);
            return tr;

        }
        private static string AggiornaAnnoCompetenzaNelTracciato(string iNPUT, CampoContent nuovoAAcompetenza)
        {
            string tr = iNPUT.Substring(0, nuovoAAcompetenza.PosizioneTracciato - 1) +
                nuovoAAcompetenza.ContenutoCampo +
                 iNPUT.Substring(nuovoAAcompetenza.PosizioneTracciato + 1);
            return tr;

        }
        public static TaskPronto InserisciTracciatoAssociatoListoneStorni(XR_MAT_RICHIESTE Richiesta, DateTime dataprimo,
            DateTime? DataInizioPratica, TaskModel model, List<TaskPronto> ListaTaskPronti)
        {
            var lista = ListaTaskPronti.Where(x => x.DataRiferimentoMeseAnno == dataprimo).ToList();
            string g26MT = "0";

            string AF = "AF-CF-DF-DK-DQ-DU";
            string MG = "MG-MV";
            string NomeTracciato = "";
            TaskPronto taskRifStessoMese = null;
            XR_MAT_ELENCO_TASK TaskElenco = null;
            string InizioPeriodo = null;
            string FinePeriodo = null;

            if (Richiesta.ECCEZIONE == "MT")
            {
                NomeTracciato = "MATERNITA MT 9000 STORICO";

                var testlistDB = Richiesta.XR_MAT_TASK_IN_CORSO.Where(x =>
                     x.ANNO == dataprimo.Year && x.MESE == dataprimo.Month &&
                     x.XR_MAT_ELENCO_TASK.NOME_TASK.Trim() == "MATERNITA MT 9000").ToList();
                if (testlistDB.Any() && testlistDB.Count > 1)
                {
                    var InviatoMese = testlistDB.Where(x => x.DATA_ULTIMO_TENTATIVO != null && x.DATA_ULTIMO_TENTATIVO.Value.Year == dataprimo.Year
                     && x.DATA_ULTIMO_TENTATIVO.Value.Month == dataprimo.Month).FirstOrDefault();
                    if (InviatoMese != null)
                    {
                        string tracciatoTesto = InviatoMese.INPUT;
                        List<CampoContent> ListaCampi =
                            GetTracciatoEsploso((int)InviatoMese.XR_MAT_ELENCO_TASK.ID_TRACCIATO_DEW,
                            (int)InviatoMese.XR_MAT_ELENCO_TASK.PROGRESSIVO_TRACCIATO_DEW,
                                  tracciatoTesto);
                        g26MT = ListaCampi.Where(x => x.NomeCampo == "GIORNI RETRIBUTIVI").Select(x => x.ContenutoCampo).FirstOrDefault();
                        InizioPeriodo = InviatoMese.NOTE.Split('-')[1];
                        FinePeriodo = InviatoMese.NOTE.Split('-')[2];

                    }
                }
                else
                {
                    taskRifStessoMese = lista.Where(x => x.TaskInCorso != null &&
                                    x.TaskInCorso.XR_MAT_ELENCO_TASK.NOME_TASK.Trim() == "MATERNITA MT 9000").FirstOrDefault();
                    g26MT = taskRifStessoMese.TracciatoEsplosoModel.TracciatoEsploso.Campi
                                        .Where(x => x.NomeCampo == "GIORNI RETRIBUTIVI").Select(x => x.ContenutoCampo).FirstOrDefault();
                    InizioPeriodo = taskRifStessoMese.PeriodoDa.Value.ToString("dd/MM/yyyy");
                    FinePeriodo = taskRifStessoMese.PeriodoA.Value.ToString("dd/MM/yyyy");
                }

            }
            //else
            //    if (AF.Split('-').Any(x => Richiesta.ECCEZIONE.StartsWith(x)))
            //{
            //    NomeTracciato = "ASSENZA FACOLTATIVA AF-CF-DF-DK-DQ-DU 9000 STORICO";
            //    taskRifStessoMese = lista.Where(x => x.TaskInCorso != null && 
            //    x.TaskInCorso.XR_MAT_ELENCO_TASK.NOME_TASK.Trim() == "ASSENZA FACOLTATIVA  AF-CF-DF-DK 9000").FirstOrDefault();
            //    g26MT = taskRifStessoMese.TracciatoEsplosoModel.TracciatoEsploso.Campi.Where(x => x.NomeCampo == "GIORNI RETRIBUTIVI").Select(x => x.ContenutoCampo).FirstOrDefault();


            //}
            //else
            //    if (MG.Split('-').Any(x => Richiesta.ECCEZIONE.StartsWith(x)))
            //{
            //    NomeTracciato = "CONGEDO PATERNO MG-MV 9000 STORICO";
            //    taskRifStessoMese = lista.Where(x => x.TaskInCorso != null &&
            //    x.TaskInCorso.XR_MAT_ELENCO_TASK.NOME_TASK.Trim() == "CONGEDO PATERNO MG-MV 9000").FirstOrDefault();
            //    g26MT = taskRifStessoMese.TracciatoEsplosoModel.TracciatoEsploso.Campi.Where(x => x.NomeCampo == "GIORNI RETRIBUTIVI").Select(x => x.ContenutoCampo).FirstOrDefault();
            //}
            TaskElenco = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK
                 .Where(x => x.STATO_PRATICA == 100)
                 .Select(x => x.XR_MAT_ELENCO_TASK).Where(z => z.NOME_TASK.Trim() == NomeTracciato).FirstOrDefault();

            if (TaskElenco != null)
            {
                XR_MAT_CATEGORIA_TASK catTask = Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK.Where(x => x.STATO_PRATICA == 100
                   && x.ID_TASK == TaskElenco.ID)
                     .FirstOrDefault();

                if (catTask == null)
                    throw new Exception("XR_MAT_CATEGORIA_TASK mancante");


                TaskPronto TP = new TaskPronto()
                {
                    DataRiferimentoMeseAnno = dataprimo
                };

                string testoTracciato = MaternitaCongediManager.GetCampiTracciato(
                         (int)TaskElenco.ID_TRACCIATO_DEW,
                         (int)TaskElenco.PROGRESSIVO_TRACCIATO_DEW,
                         Richiesta,
                         Richiesta.ECCEZIONE,
                         InizioPeriodo,//taskRifStessoMese.PeriodoDa.Value.ToString("dd/MM/yyyy"),
                         FinePeriodo,//taskRifStessoMese.PeriodoA.Value.ToString("dd/MM/yyyy"),
                         g26MT,
                         DataInizioPratica.Value,
                         model.DettaglioAmmModel.ImportoFinale.ToString(),
                         model.Importo13ma,
                          model.Importo14ma,
                          model.ImportoPremio, IsTracciatoStorico: true

                         );
                XR_MAT_TASK_IN_CORSO TaskInCorsoEntity = new XR_MAT_TASK_IN_CORSO()
                {
                    ANNO = dataprimo.Year,
                    MESE = dataprimo.Month,
                    DATA_CREAZIONE = DateTime.Now,
                    INPUT = testoTracciato,
                    MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                    XR_MAT_ELENCO_TASK = TaskElenco,
                    XR_MAT_RICHIESTE = Richiesta,
                    PROGRESSIVO = (int)catTask.PROGRESSIVO,
                    ID_RICHIESTA = Richiesta.ID,
                    ID_TASK = TaskElenco.ID,
                    BLOCCATA_DATETIME = DateTime.Now,
                    BLOCCATA_DA_OPERATORE = CommonHelper.GetCurrentUserMatricola()
                };
                TP.TaskInCorso = TaskInCorsoEntity;
                List<CampoContent> ListaCampi = new List<CampoContent>();
                ListaCampi = GetTracciatoEsploso(
                          (int)TaskElenco.ID_TRACCIATO_DEW,
                          (int)TaskElenco.PROGRESSIVO_TRACCIATO_DEW,
                          TaskInCorsoEntity.INPUT);

                TP.TracciatoEsplosoModel = new TaskTracciatoExpModel();
                TP.TracciatoEsplosoModel.TracciatoEsploso = new ContenutoCampiPerMeseTask()
                {
                    Campi = ListaCampi
                };
                TP.TracciatoEsplosoModel.TracciatoEsploso.TracciatoIntero = TaskInCorsoEntity.INPUT;
                TP.TracciatoEsplosoModel.EditPermesso = TP.IdAltraPraticaGiaSuDB == 0;
                DateTime D1;
                bool e1 = DateTime.TryParseExact(InizioPeriodo, "dd/MM/yyyy", null, DateTimeStyles.None, out D1);
                DateTime D2;
                bool e2 = DateTime.TryParseExact(FinePeriodo, "dd/MM/yyyy", null, DateTimeStyles.None, out D2);
                if (e1 && e2)
                {
                    TP.PeriodoDa = D1;
                    TP.PeriodoA = D2;
                }
                return TP;
            }
            else return null;
        }

        public static bool SonoDaAggiungereGiorniForzati(XR_MAT_RICHIESTE Richiesta)
        {
            return (Richiesta.ASSENZA_LUNGA == true && Richiesta.XR_MAT_CATEGORIE.SKIP_FUSIONE_PERIODI != true);
        }
        public static MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.Tracciato_richiamato[]
            GetRecordRewDaCancellare(string matricola, string eccezione, string programma, DateTime dataprimo)
        {
            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew s = new MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew();
            s.Credentials = new System.Net.NetworkCredential(
            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            try
            {

                MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.Tracciato_richiamato[] response =
                        s.GetRecordDaCancellare(programma, eccezione, matricola, null);// dataprimo.ToString("yyyyMM"));

                if (response == null || !response.Any())
                    return null;
                else
                    return response.ToArray();
            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "HRIS",
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "GetRecordRewDaCancellare",
                    error_message = ex.ToString()
                });
                return null;
            }
        }

        //public static DateTime? ControllaSabDomFine(DettaglioGiorniModel periodoEccezione,
        public static DateTime? ControllaSabDomFine(DateTime periodoEccezione_DataDa, DateTime? periodoEccezione_DataA,
            XR_MAT_RICHIESTE Rich, GetSchedaPresenzeMeseResponse timbr, string formaContratto)
        {
            DateTime Dstart = new DateTime(periodoEccezione_DataA.Value.Year, periodoEccezione_DataA.Value.Month, 1);
            DateTime Dend = Dstart.AddMonths(1).AddDays(-1);


            var giornoSucc = timbr.Giorni.Where(x => x.data == periodoEccezione_DataA.Value.AddDays(1)).FirstOrDefault();
            if (giornoSucc == null || giornoSucc.CodiceOrario == null || !giornoSucc.CodiceOrario.StartsWith("9")
                || (giornoSucc.CodiceOrario == "97" && formaContratto == "K"))
                return null;

            DateTime? SettimanaCorrenteEnd = periodoEccezione_DataA.Value;
            DateTime? SettimanaCorrenteStart = GetGiorno9XrispettoAData(periodoEccezione_DataA.Value, timbr, false, formaContratto);
            if (SettimanaCorrenteStart != null)
                SettimanaCorrenteStart = SettimanaCorrenteStart.Value.AddDays(1);


            DateTime? SettimanaSuccessivaStart = GetGiornoDiversoDa9XrispettoAData(SettimanaCorrenteEnd.Value.AddDays(1), timbr, true, formaContratto);
            DateTime? SettimanaSuccessivaEnd = null;
            if (SettimanaSuccessivaStart != null)
            {
                SettimanaSuccessivaEnd = GetGiorno9XrispettoAData(SettimanaSuccessivaStart.Value, timbr, true, formaContratto);
                if (SettimanaSuccessivaEnd != null)
                    SettimanaSuccessivaEnd = SettimanaSuccessivaEnd.Value.AddDays(-1);
            }

            if (SettimanaCorrenteStart != null && SettimanaCorrenteEnd != null && SettimanaSuccessivaStart != null &&
               SettimanaSuccessivaEnd != null)
            {
                SettimanaComputoAF SettCorrente = new SettimanaComputoAF(SettimanaCorrenteStart.Value,
                SettimanaCorrenteEnd.Value, Rich.MATRICOLA);
                SettCorrente.Popola(Rich, formaContratto);

                SettimanaComputoAF SettSuccessiva = new SettimanaComputoAF(SettimanaSuccessivaStart.Value,
                   SettimanaSuccessivaEnd.Value, Rich.MATRICOLA);
                SettSuccessiva.Popola(Rich, formaContratto);

                WeekEndComputoAF W = new WeekEndComputoAF(SettCorrente, SettSuccessiva, false);
                if (W.DaConsiderare)
                {
                    DateTime? Dritorno = GetGiornoDiversoDa9XrispettoAData(SettimanaCorrenteEnd.Value, timbr, true, formaContratto);
                    if (Dritorno == null)
                        return null;
                    else
                    {
                        Dritorno = Dritorno.Value.AddDays(-1);
                        if (Dritorno > Dend)
                            return Dend;
                        else
                            return Dritorno;
                    }

                    //return SettimanaCorrenteEnd.Value.AddDays(1);
                }
            }


            return null;
        }

        public static bool GiornoContieneEccezioniMacro(GetSchedaPresenzeMeseResponse timbr, DateTime data)
        {
            List<string> ecc = MaternitaCongediManager.GetEccezioniFE_ML();
            var day = timbr.Giorni.Where(x => x.data == data).FirstOrDefault();
            if (day == null || day.MacroAssenze == null || !day.MacroAssenze.Any())
                return false;

            foreach (var m in day.MacroAssenze)
            {
                if (String.IsNullOrWhiteSpace(m)) continue;
                if (ecc.Contains(m.Substring(1)))
                    return true;
            }
            return false;

        }
        public static DateTime? GetGiornoDiversoDa9XrispettoAData(DateTime data, GetSchedaPresenzeMeseResponse timbr, bool Avanti,
            string FormaContratto, bool CheckIfEccezione = false)
        {
            int dir = Avanti ? 1 : -1;
            for (int i = 1; i <= 30; i++)
            {
                var giorno = timbr.Giorni.Where(x => x.data == data.AddDays(dir * i)).FirstOrDefault();
                if (giorno != null)
                {
                    if (FormaContratto == "K")
                    {
                        if (String.IsNullOrWhiteSpace(giorno.CodiceOrario) || giorno.CodiceOrario == "97" || !giorno.CodiceOrario.StartsWith("9"))
                        {
                            if (CheckIfEccezione)
                            {
                                if (GiornoContieneEccezioniMacro(timbr, giorno.data))
                                    return null;
                                else
                                    return giorno.data;
                            }
                            else
                                return giorno.data;

                        }
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(giorno.CodiceOrario) || !giorno.CodiceOrario.StartsWith("9"))
                        {
                            //if (CheckIfEccezione)
                            //{
                            //    if (GiornoContieneEccezioniMacro(timbr, giorno.data))
                            //        return null;
                            //    else
                            //        return giorno.data;
                            //}
                            //else
                            return giorno.data;
                        }
                    }
                }
            }
            return null;
        }
        public static DateTime? GetGiorno9XrispettoAData(DateTime data, GetSchedaPresenzeMeseResponse timbr, bool Avanti,
            string FormaContratto, bool CheckIfEccezione = false)
        {
            int dir = Avanti ? 1 : -1;
            for (int i = 1; i <= 30; i++)
            {
                var giorno = timbr.Giorni.Where(x => x.data == data.AddDays(dir * i)).FirstOrDefault();
                if (giorno != null)
                {
                    if (FormaContratto == "K")
                    {
                        if (!String.IsNullOrWhiteSpace(giorno.CodiceOrario) && giorno.CodiceOrario.StartsWith("9")
                            && giorno.CodiceOrario != "97")
                        {
                            if (CheckIfEccezione)
                            {
                                if (GiornoContieneEccezioniMacro(timbr, giorno.data))
                                    return null;
                                else
                                    return giorno.data;
                            }
                            else
                                return giorno.data;
                        }

                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(giorno.CodiceOrario) && giorno.CodiceOrario.StartsWith("9"))
                        {
                            if (CheckIfEccezione)
                            {
                                if (GiornoContieneEccezioniMacro(timbr, giorno.data))
                                    return null;
                                else
                                    return giorno.data;
                            }
                            else
                                return giorno.data;
                        }
                    }

                }
            }
            return null;
        }

        public static GetSchedaPresenzeMeseResponse GetDatiPeriodo(DateTime DataInizioPeriodo, string matricola)
        {
            MyRaiService1Client cl = new MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            DateTime Dstart = new DateTime(DataInizioPeriodo.Year, DataInizioPeriodo.Month, 1);
            DateTime Dend = Dstart.AddMonths(1).AddDays(-1);

            GetSchedaPresenzeMeseResponse timbr = cl.GetSchedaPresenzeMese(matricola, Dstart, Dend);

            //GetTimbratureMeseResponse timbr = cl.GetTimbratureMese(Rich.MATRICOLA, Dstart, Dend);
            var timbrPrec = cl.GetSchedaPresenzeMese(matricola, Dstart.AddMonths(-1), Dend.AddMonths(-1));
            var timbrSucc = cl.GetSchedaPresenzeMese(matricola, Dstart.AddMonths(1), Dend.AddMonths(1));
            if (timbrPrec != null && timbrPrec.Giorni != null)
            {
                var g = timbr.Giorni.ToList();
                g.AddRange(timbrPrec.Giorni.ToList());
                timbr.Giorni = g.ToArray();
            }
            if (timbrSucc != null && timbrSucc.Giorni != null)
            {
                var g = timbr.Giorni.ToList();
                g.AddRange(timbrSucc.Giorni.ToList());
                timbr.Giorni = g.ToArray();
            }
            return timbr;
        }
        //public static DateTime? ControllaSabDomInizio(DettaglioGiorniModel periodoEccezione, XR_MAT_RICHIESTE Rich,
        public static DateTime? ControllaSabDomInizio(DateTime periodoEccezione_DataDa, DateTime? periodoEccezione_DataA,  XR_MAT_RICHIESTE Rich,
            GetSchedaPresenzeMeseResponse timbr, string formaContratto)
        {
            DateTime Dstart = new DateTime(periodoEccezione_DataA.Value.Year, periodoEccezione_DataA.Value.Month, 1);
            DateTime Dend = Dstart.AddMonths(1).AddDays(-1);


            var giornoPrec = timbr.Giorni.Where(x => x.data == periodoEccezione_DataDa.AddDays(-1)).FirstOrDefault();
            if (giornoPrec == null || giornoPrec.CodiceOrario == null || !giornoPrec.CodiceOrario.StartsWith("9") ||
                (giornoPrec.CodiceOrario == "97" && formaContratto == "K"))
                return null;

            DateTime? SettimanaPrecedenteEnd = GetGiornoDiversoDa9XrispettoAData(periodoEccezione_DataDa.AddDays(-1), timbr, false, formaContratto);
            if (SettimanaPrecedenteEnd == null) // prima del 2020 il gapp non ha dati
                return null;

            DateTime? SettimanaPrecedenteStart = GetGiorno9XrispettoAData(SettimanaPrecedenteEnd.Value, timbr, false, formaContratto);
            if (SettimanaPrecedenteStart != null)
                SettimanaPrecedenteStart = SettimanaPrecedenteStart.Value.AddDays(1);

            DateTime? SettimanaCorrenteStart = periodoEccezione_DataDa;
            DateTime? SettimanaCorrenteEnd = GetGiorno9XrispettoAData(periodoEccezione_DataDa, timbr, true, formaContratto);
            if (SettimanaCorrenteEnd != null)
                SettimanaCorrenteEnd = SettimanaCorrenteEnd.Value.AddDays(-1);

            if (SettimanaPrecedenteStart != null && SettimanaPrecedenteEnd != null && SettimanaCorrenteStart != null && SettimanaCorrenteEnd != null)
            {
                SettimanaComputoAF SettCorrente = new SettimanaComputoAF(SettimanaCorrenteStart.Value,
               SettimanaCorrenteEnd.Value, Rich.MATRICOLA);

                SettCorrente.Popola(Rich, formaContratto);

                SettimanaComputoAF SettPrecedente = new SettimanaComputoAF(SettimanaPrecedenteStart.Value,
                    SettimanaPrecedenteEnd.Value, Rich.MATRICOLA);
                SettPrecedente.Popola(Rich, formaContratto);

                WeekEndComputoAF W = new WeekEndComputoAF(SettPrecedente, SettCorrente, true);
                if (W.DaConsiderare)
                {
                    DateTime? Dritorno = GetGiornoDiversoDa9XrispettoAData(SettimanaCorrenteStart.Value, timbr, false,
                        formaContratto, CheckIfEccezione: true);

                    if (Dritorno == null)
                        return null;
                    else
                    {
                        Dritorno = Dritorno.Value.AddDays(1);
                        if (Dritorno < Dstart)
                            return Dstart;
                        else
                        {
                            if (GiornoContieneEccezioniMacro(timbr, Dritorno.Value))
                            {
                                Dritorno = Dritorno.Value.AddDays(1);
                                if (Dritorno < SettimanaCorrenteStart.Value && GiornoContieneEccezioniMacro(timbr, Dritorno.Value))
                                {
                                    Dritorno = Dritorno.Value.AddDays(1);
                                }
                            }
                            return Dritorno;
                        }

                    }

                    //return SettimanaCorrenteStart.Value.AddDays(-1);
                }
            }

            return null;
        }

       
        public static MaternitaApprovazioniModel GetContentApprovazioniModel(string mese, string matr, string sede, int? tipo, int? stato)
        {
            MaternitaApprovazioniModel model = new MaternitaApprovazioniModel();
            model.IsPreview = false;
            var db = new IncentiviEntities();
            var richieste = db.XR_MAT_RICHIESTE.Where(x =>x.ECCEZIONE!="SW" &&  x.PRESA_VISIONE_RESP_GEST == null);



            List<XR_MAT_RICHIESTE> LR = new List<XR_MAT_RICHIESTE>();
            foreach (var r in richieste.OrderBy(x => x.DATA_INVIO_RICHIESTA))
            {
                if (r.ID == 5888)
                {

                }
                var statoWkf = r.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();

                if (statoWkf != null &&
                   (statoWkf.ID_STATO == (int)EnumStatiRichiesta.ApprovataGestione || statoWkf.ID_STATO == (int)EnumStatiRichiesta.ApprovataUffPers)
                    )
                {
                    LR.Add(r);
                }
            }
            model.RichiesteAggregate = GetRichiesteAggregatePerMatricola(LR);
            model.MyOffice = MaternitaCongediHelper.MaternitaCongediUffici.Gestione;
            return model;
        }

        public static void GetValoriContatoreCalcolati(int idrichiesta)
        {
            List<PeriodoArretratoDipendente> List = GetListPeriodiDipendente(idrichiesta);

        }
       
        public static List<PeriodoArretratoDipendente> GetListPeriodiDipendente(int idrichiesta)
        {
            //##contatore
            IncentiviEntities db = new IncentiviEntities();
            XR_MAT_RICHIESTE ric = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            var list = new List<PeriodoArretratoDipendente>();

            var temp1 = db.XR_MAT_ARRETRATI_DIPENDENTE
              .Where(x => x.MATRICOLA == ric.MATRICOLA && x.CODICE_FISCALE_FIGLIO == ric.CF_BAMBINO);

            if (ric.XR_MAT_CATEGORIE.CAT == "CON")
            {
                temp1 = temp1.Where(x => x.ECCEZIONE.StartsWith("MU") || x.ECCEZIONE.StartsWith("AF")
                 || x.ECCEZIONE.StartsWith("BF") || x.ECCEZIONE.StartsWith("CF"));
            }
            else
            {
                temp1 = temp1.Where(x => !x.ECCEZIONE.StartsWith("MU") && !x.ECCEZIONE.StartsWith("AF")
                && !x.ECCEZIONE.StartsWith("BF") && !x.ECCEZIONE.StartsWith("CF"));
            }
            var temp = temp1.GroupBy(x => new { x.PERIODO_RIFERIMENTO_DA, x.PERIODO_RIFERIMENTO_A }).ToList();

            foreach (var item in temp)
            {
                var tot = item.Sum(x => x.QUANTITA);
                var ecc = item.First().ECCEZIONE;
                DateTime d1 = (DateTime)item.Key.PERIODO_RIFERIMENTO_DA;
                DateTime d2 = (DateTime)item.Key.PERIODO_RIFERIMENTO_A;

                list.Add(new PeriodoArretratoDipendente()
                {
                    D1 = d1,
                    D2 = d2,
                    Eccezione = ecc,
                    Quantita = (float)tot
                });
            }
            list = list.OrderBy(x => x.D1).ToList();
            List<XR_MAT_RICHIESTE> RichInCorso = new List<XR_MAT_RICHIESTE>();
            string matr = ric.MATRICOLA;
            if (ric.XR_MAT_CATEGORIE.CAT == "CON")
            {
                RichInCorso = db.XR_MAT_RICHIESTE.Where(x =>
                                            x.ID != idrichiesta &&
                                            x.MATRICOLA == matr &&
                                            x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) <= 80
                                            &&
                                            (x.ECCEZIONE.StartsWith("MU") || x.ECCEZIONE.StartsWith("AF")
                                          || x.ECCEZIONE.StartsWith("BF") || x.ECCEZIONE.StartsWith("CF"))
                                            ).ToList();
            }
            else
            {
                RichInCorso = db.XR_MAT_RICHIESTE.Where(x =>
                                           x.ID != idrichiesta &&
                                           x.MATRICOLA == matr &&
                                           x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) <= 80
                                           &&
                                           (!x.ECCEZIONE.StartsWith("MU") && !x.ECCEZIONE.StartsWith("AF")
                                            && !x.ECCEZIONE.StartsWith("BF") && !x.ECCEZIONE.StartsWith("CF"))
                                           ).ToList();
            }
            if (RichInCorso.Any())
            {
                foreach (var r in RichInCorso)
                {

                    var PA = (new PeriodoArretratoDipendente()
                    {
                        D1 = r.INIZIO_GIUSTIFICATIVO != null ? r.INIZIO_GIUSTIFICATIVO.Value : r.DATA_INIZIO_MATERNITA.Value,
                        D2 = r.FINE_GIUSTIFICATIVO != null ? r.FINE_GIUSTIFICATIVO.Value : r.DATA_FINE_MATERNITA.Value,
                        Eccezione = r.ECCEZIONE,
                        Quantita = (float)r.NUMERO_GIORNI_GIUSTIFICATIVO,
                        IdRichiestaInCorso = r.ID
                    });

                    list.Add(PA);
                }
            }
            return list;
        }
        public static List<string> GetEccezioniInServizio()
        {
            var db = new digiGappEntities();
            string[] ini = "1".Split(',');
            var list = db.L2D_ECCEZIONE.Where(x => ini.Any(z => x.desc_cod_eccez_padre.StartsWith(z)))
                .Select(a => a.cod_eccezione.Trim()).ToList();

            return list;
        }
        public static List<string> GetEccezioniFE_ML()
        {
            var db = new digiGappEntities();
            string[] ini = "2,3,4,6,7".Split(',');
            var list = db.L2D_ECCEZIONE.Where(x => ini.Any(z => x.desc_cod_eccez_padre.StartsWith(z)))
                .Select(a => a.cod_eccezione.Trim()).ToList();

            return list;
        }


        public static List<DettaglioGiorniModel> FondiPeriodiEccezione(List<DettaglioGiorniModel> periodiEccezione, string matricola, bool? skipFusione, string formaContratto)
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            periodiEccezione = periodiEccezione.OrderBy(x => x.DataDa).ToList();

            List<DettaglioGiorniModel> PeriodiEccezioneSenzaCestinati = new List<DettaglioGiorniModel>();
            foreach (var pe in periodiEccezione)
            {
                if (pe.DataDa == pe.DataA || (pe.DataA == null && pe.DataDa != null))
                {
                    //MaternitaCongediManager.IsEccezioneCestinata()
                }
            }
            DateTime datainizio = new DateTime(periodiEccezione[0].DataDa.Year, periodiEccezione[0].DataDa.Month, 1);

            MyRaiServiceInterface.MyRaiServiceReference1.GetSchedaPresenzeMeseResponse response =
                cl.GetSchedaPresenzeMese(matricola, datainizio, datainizio.AddMonths(1).AddDays(-1));

            bool AnyFusion = true;
            while (AnyFusion)
            {
                AnyFusion = false;
                for (int i = 0; i < periodiEccezione.Count - 1; i++)
                {

                    if (
                        MaternitaCongediManager.IsEccezioneSospesa(periodiEccezione[i].StatoEccez)
                        ||
                        MaternitaCongediManager.IsEccezioneSospesa(periodiEccezione[i + 1].StatoEccez)
                         )
                        continue;

                    if (periodiEccezione[i].DataA == null) periodiEccezione[i].DataA = periodiEccezione[i].DataDa;
                    if (periodiEccezione[i + 1].DataA == null) periodiEccezione[i + 1].DataA = periodiEccezione[i + 1].DataDa;
                    if (periodiEccezione[i].DataA < periodiEccezione[i + 1].DataDa)
                    {
                        //if (periodiEccezione[i].DataA == null) periodiEccezione[i].DataA = periodiEccezione[i].DataDa;
                        //if (periodiEccezione[i + 1].DataA == null) periodiEccezione[i + 1].DataA = periodiEccezione[i + 1].DataDa;

                        bool Fondi = false;
                        DateTime Dcurrent = periodiEccezione[i].DataA.Value.AddDays(1);
                        if (Dcurrent == periodiEccezione[i + 1].DataDa)
                            Fondi = true;
                        else
                        {
                            if (skipFusione != true)
                            {
                                while (Dcurrent < periodiEccezione[i + 1].DataDa)
                                {
                                    Fondi = true;
                                    var giorno = response.Giorni.Where(x => x.data == Dcurrent).FirstOrDefault();
                                    if (!giorno.CodiceOrario.StartsWith("9") || (giorno.CodiceOrario == "97" && formaContratto == "K"))
                                    {
                                        Fondi = false;
                                        break;
                                    }
                                    Dcurrent = Dcurrent.AddDays(1);
                                }
                            }

                        }

                        if (Fondi)
                        {
                            AnyFusion = true;
                            if (skipFusione != true)
                            {
                                if (periodiEccezione[i].Fusioni == 0)
                                {
                                    periodiEccezione[i].Fusioni = 2;

                                    periodiEccezione[i].IntervalliFusi.Add(periodiEccezione[i].DataDa.ToString("dd/MM/yyyy") + " - " + periodiEccezione[i].DataA.Value.ToString("dd/MM/yyyy"));
                                    periodiEccezione[i].IntervalliFusi.Add(periodiEccezione[i + 1].DataDa.ToString("dd/MM/yyyy") + " - " + periodiEccezione[i + 1].DataA.Value.ToString("dd/MM/yyyy"));

                                }
                                else
                                {
                                    periodiEccezione[i].Fusioni++;
                                    periodiEccezione[i].IntervalliFusi.Add(periodiEccezione[i + 1].DataDa.ToString("dd/MM/yyyy") + " - " + periodiEccezione[i + 1].DataA.Value.ToString("dd/MM/yyyy"));
                                }
                            }





                            //periodiEccezione[i].NotaFusione ="fusione di " // "Il periodo proviene dalla fusione di due o più periodi ";
                            //+
                            //   periodiEccezione[i].DataDa.ToString("dd") +
                            //   (periodiEccezione[i].DataA == null || periodiEccezione[i].DataA == periodiEccezione[i].DataDa ? ""
                            //   : "-" + periodiEccezione[i].DataA.Value.ToString("dd"))

                            //   + " e del " +
                            //   periodiEccezione[i + 1].DataDa.ToString("dd") +
                            //   (periodiEccezione[i + 1].DataA == null || periodiEccezione[i + 1].DataA == periodiEccezione[i + 1].DataDa ? "" : "-" + periodiEccezione[i + 1].DataA.Value.ToString("dd"))

                            //   + " " + periodiEccezione[i].DataDa.ToString("MMMM");

                            periodiEccezione[i].DataA = periodiEccezione[i + 1].DataA;
                            periodiEccezione[i].NumeroGiorniRuoli += periodiEccezione[i + 1].NumeroGiorniRuoli;
                            periodiEccezione[i].NumeroGiorniGapp += periodiEccezione[i + 1].NumeroGiorniGapp;

                            periodiEccezione[i + 1].Soppresso = true;

                            break;
                        }
                    }
                }
                periodiEccezione = periodiEccezione.Where(x => x.Soppresso == false).ToList();
            }


            return periodiEccezione;
        }
        public static SetStatoEccezioneResponse SetStatoEccezione(string data, string eccezione, string matricola, string stato)
        {
            MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            DateTime D;
            DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);

            var RaccoltaResponse = cl.GetRuoli(matricola, new DateTime(D.Year, D.Month, 1), "   ");
            var giorno = RaccoltaResponse.Eccezioni.Where(x => x.DataDocumento == D && x.CodiceEccezione == eccezione).FirstOrDefault();
            if (giorno != null && !String.IsNullOrWhiteSpace(giorno.NumeroDocumento))
            {
                giorno.NumeroDocumento = giorno.NumeroDocumento.Trim();
                if (giorno.NumeroDocumento.Length < 6)
                    giorno.NumeroDocumento = giorno.NumeroDocumento.PadLeft(6, '0');

                SetStatoEccezioneResponse Response =
               cl.SetStatoEccezione(CommonHelper.GetCurrentUserMatricola(), D, matricola, giorno.NumeroDocumento, eccezione, stato);

                return Response;
            }
            else
                throw new Exception("Impossibile ottenere numero documento");
        }

        public static RaccoltaMesePrecedente GetRaccoltaMesePrecedente(XR_MAT_RICHIESTE richiesta)
        {
            RaccoltaMesePrecedente R = new RaccoltaMesePrecedente();

            MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            DateTime? Dinizio = richiesta.INIZIO_GIUSTIFICATIVO;
            if (Dinizio == null)
                Dinizio = richiesta.DATA_INIZIO_MATERNITA;

            Dinizio = Dinizio.Value.AddMonths(-1);
            R.Response =
                cl.GetRuoli(richiesta.MATRICOLA, new DateTime(Dinizio.Value.Year, Dinizio.Value.Month, 1),
                "   ");
            List<string> EccezioniRilevanti = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniCongedi)
         .Split(',').ToList();
            R.Response.Eccezioni = R.Response.Eccezioni.Where(x => EccezioniRilevanti.Contains(x.CodiceEccezione.Trim())).ToArray();

            R.Anno = Dinizio.Value.Year;
            R.Mese = Dinizio.Value.Month;
            foreach (var ec in R.Response.Eccezioni)
            {
                if (IsHalf(ec.CodiceEccezione))
                    R.TotaleGiorni += 0.6f;
                else if (ec.CodiceEccezione.Trim().EndsWith("Q"))
                    R.TotaleGiorni += 0.3f;
                else
                    R.TotaleGiorni += 1.2f;
            }
            R.TotaleGiorni = (float)Math.Round(R.TotaleGiorni, 2);
            return R;
        }

        private static List<TaskPronto> GetTaskProntiDaDB(int IdRichiesta)
        {
            var db = new IncentiviEntities();
            var Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == IdRichiesta).FirstOrDefault();
            List<TaskPronto> ListaTaskPronti = new List<TaskPronto>();
            foreach (var t in Richiesta.XR_MAT_TASK_IN_CORSO.OrderBy(x => x.ANNO).ThenBy(x => x.MESE).ThenBy(x => x.PROGRESSIVO))
            {
                TaskPronto TP = new TaskPronto();
                TP.DataRiferimentoMeseAnno = new DateTime(t.ANNO, t.MESE, 1);
                TP.TaskInCorso = t;
                TP.DicituraPeriodo = null;


                List<CampoContent> ListaCampi = new List<CampoContent>();

                if (t.XR_MAT_ELENCO_TASK.TIPO == "TRACCIATO" || t.XR_MAT_ELENCO_TASK.TIPO == "TRACCIATO-TE")
                {
                    ListaCampi = GetTracciatoEsploso(
                   (int)t.XR_MAT_ELENCO_TASK.ID_TRACCIATO_DEW,
                   (int)t.XR_MAT_ELENCO_TASK.PROGRESSIVO_TRACCIATO_DEW,
                   t.INPUT);

                    TP.TracciatoEsplosoModel = new TaskTracciatoExpModel();
                    TP.TracciatoEsplosoModel.TracciatoEsploso = new ContenutoCampiPerMeseTask()
                    {
                        Campi = ListaCampi
                    };
                    TP.TracciatoEsplosoModel.TracciatoEsploso.TracciatoIntero = t.INPUT;
                    if (t.TERMINATA == false)
                        TP.TracciatoEsplosoModel.EditPermesso = true;

                    if (t.XR_MAT_ELENCO_TASK.TIPO == "TRACCIATO" && t.NOTE != null)
                    {
                        var parti = t.NOTE.Split('-');
                        int t1 = Int32.Parse(parti[1].Split('/')[2]);
                        int t2 = Int32.Parse(parti[1].Split('/')[1]);
                        int t3 = Int32.Parse(parti[1].Split('/')[0]);
                        DateTime dt = new DateTime(t1, t2, t3);
                        t1 = Int32.Parse(parti[2].Split('/')[2]);
                        t2 = Int32.Parse(parti[2].Split('/')[1]);
                        t3 = Int32.Parse(parti[2].Split('/')[0]);
                        DateTime dt2 = new DateTime(t1, t2, t3);

                        myRaiCommonModel.AmministrazioneModel.BustaPaga res1 = myRaiCommonManager.AmministrazioneManager.GetPagamenti(t.XR_MAT_RICHIESTE.MATRICOLA, t.XR_MAT_RICHIESTE.ECCEZIONE + dt.ToString("dd") + "/" + dt2.ToString("dd-MM-yy"), t.ESEGUIBILE_DA_DATA);


                        if (parti.Length > 2)
                            TP.DicituraPeriodo = parti[1] + " - " + parti[2];

                        if (res1.elencoVoci.Count() > 0)
                        {
                            string data = Int32.Parse(res1.dataCompetenza.Substring(0, 2)) > 40 ? "19" + res1.dataCompetenza : "20" + res1.dataCompetenza;
                            if (res1.elencoVoci.Count() > 0)
                            {


                                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                                string jsonString = ser.Serialize(res1);
                                TP.DicituraPeriodo = TP.DicituraPeriodo + " </span> <br><br><a class='rai-label rai-label-interactive' style='padding-top:5px' data-table-collapsable-toggle='ignore' href='javascript:ShowViewCedolinoPagato(" + jsonString + ")' title='Vedi il cedolino'> PAGATO in " + DateTime.ParseExact(data + "01", "yyyyMMdd",
                                                                    System.Globalization.CultureInfo.InvariantCulture).ToString("MMMM yyyy");
                                TP.DicituraPeriodo = TP.DicituraPeriodo + "</a>";
                            }
                        }



                    }
                }
                ListaTaskPronti.Add(TP);
            }
            return ListaTaskPronti;
        }

        public static bool StraordinariCaricati(DateTime? D = null)
        {
            var db = new IncentiviEntities();
            if (D == null) D = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return db.XR_VAR_STRAORDINARI.Where(x => x.DTA_COMPETENZA == D).Any();
        }
        public static decimal GetImportoLordoTotale(string matr, string mesi)//"'2022/04', '2022/03' , '2022/02'"
        {
            var db = new IncentiviEntities();
            string query = CommonHelper.GetParametro<string>(EnumParametriSistema.QueryHRDWimportoLordo)
                            .Replace("#MATR", matr)
                            .Replace("#MESI", mesi);
            ImportoLordoHRDW im = new ImportoLordoHRDW();
            IEnumerable<ImportoLordoHRDW> quantita = db.Database.SqlQuery<ImportoLordoHRDW>(query).ToList();
            decimal tot = quantita
                //.Where(x=>x.cod_aggregato_costi=="01" || x.cod_aggregato_costi == "02")
                .Sum(x => x.totale);
            return Math.Round(tot, 2);
        }
        public static string RichiestaPagata(int idRichiesta, DateTime Dinizio, DateTime Dfine)
        {
            var db = new IncentiviEntities();
            var Rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idRichiesta).FirstOrDefault();

            myRaiCommonModel.AmministrazioneModel.BustaPaga res1 = myRaiCommonManager.AmministrazioneManager.GetPagamenti(
                Rich.MATRICOLA, Rich.ECCEZIONE + Dinizio.ToString("dd") + "/" + Dfine.ToString("dd-MM-yy"), new DateTime(Dinizio.Year, Dinizio.Month, 1));
            if (res1.elencoVoci.Count() > 0)
            {
                string data = Int32.Parse(res1.dataCompetenza.Substring(0, 2)) > 40 ? "19" + res1.dataCompetenza : "20" + res1.dataCompetenza;
                return data;
            }
            else return null;

        }
        public static int? GetGiorni26ComeCalendarioPDF(string matricola, DateTime Data)
        {
            if (Data.Day == 1) return null;

            MyRaiService1Client cl = new MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            DateTime Dstart = new DateTime(Data.Year, Data.Month, 1);
            DateTime Dend = Dstart.AddMonths(1).AddDays(-1);
            int totaleGiorniPassati = 0;

            GetSchedaPresenzeMeseResponse timbr = cl.GetSchedaPresenzeMese(matricola, Dstart, Dend);
            string[] EccezioniCongedi = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniCongedi).Split(',');
            foreach (var giorno in timbr.Giorni)
            {
                if (giorno.data == Data)
                    break;
                if (giorno.MacroAssenze != null && giorno.CodiceOrario != null && !giorno.CodiceOrario.StartsWith("9"))
                {
                    bool contieneEcc = false;
                    foreach (var ecc in EccezioniCongedi)
                    {
                        if (giorno.MacroAssenze.Contains("N" + ecc))
                        {
                            contieneEcc = true;
                        }
                    }

                }
            }
            for (DateTime D = Dstart; D < Data; D = D.AddDays(1))
            {
                if (D.DayOfWeek != DayOfWeek.Sunday)
                {
                    totaleGiorniPassati++;
                }
            }

            return 26 - totaleGiorniPassati;
        }
        public static List<TaskPronto> GetTaskPronti(int IdRichiesta, TaskModel model, bool IsFromTE)
        {
            var db = new IncentiviEntities();
            var Richiesta = model.Richiesta;// db.XR_MAT_RICHIESTE.Where(x => x.ID == IdRichiesta).FirstOrDefault();
            List<TaskPronto> ListaTaskPronti = new List<TaskPronto>();
            List<TaskPronto> ListaTaskProntiElaborati = new List<TaskPronto>();
            if (Richiesta.XR_MAT_TASK_IN_CORSO.Any())
            {
                ListaTaskPronti = GetTaskProntiDaDB(IdRichiesta).Where(x => x.TaskInCorso.TERMINATA == true).ToList();
                var ListaTaskProntiRimborsi = GetTaskProntiDaDB(IdRichiesta).Where(x => x.TaskInCorso.RIMBORSO_TRACCIATO != null
                                              && x.TaskInCorso.TERMINATA != true).ToList();
                if (ListaTaskProntiRimborsi.Any())
                {
                    foreach (var t in ListaTaskProntiRimborsi)
                    {
                        t.DicituraPeriodo = "Rimborso di " + t.TaskInCorso.XR_MAT_ELENCO_TASK.NOME_TASK + " " + t.DicituraPeriodo;
                    }
                    ListaTaskPronti.AddRange(ListaTaskProntiRimborsi);
                }
            }



            if (IsFromTE)
                ListaTaskProntiElaborati = GetTaskProntiDaElaborazioneTE(IdRichiesta, model);
            else
                ListaTaskProntiElaborati = GetTaskProntiDaElaborazione(IdRichiesta, model);

            var toremove = ListaTaskProntiElaborati.Where(x =>
                                    ListaTaskPronti.Any(b =>
                                           b.TaskInCorso.ID_TASK == x.TaskInCorso.ID_TASK &&
                                           x.TaskInCorso.ID_RICHIESTA == IdRichiesta &&
                                           x.TaskInCorso.XR_MAT_RICHIESTE.MATRICOLA == Richiesta.MATRICOLA &&
                                           b.TaskInCorso.MESE == x.TaskInCorso.MESE &&
                                           b.DicituraPeriodo == x.DicituraPeriodo &&
                                           b.TaskInCorso.ANNO == x.TaskInCorso.ANNO)).ToList();

            ListaTaskPronti.AddRange(ListaTaskProntiElaborati.Where(x => !toremove.Contains(x)));
            var listanew = ListaTaskPronti.OrderBy(x => x.DataRiferimentoMeseAnno).ToList();
            List<DateTime> ListaPrimiMese = ListaTaskPronti.Select(x => x.DataRiferimentoMeseAnno).Distinct().OrderBy(x => x).ToList();
            List<TaskPronto> LdaRimuovere = new List<TaskPronto>();

            foreach (var d in ListaPrimiMese)
            {
                var tasksMaternita = ListaTaskPronti.Where(x => x.DataRiferimentoMeseAnno == d && x.TaskInCorso != null &&
                x.TaskInCorso.ID_TASK == 3).ToList();
                if (tasksMaternita.Count == 2)
                {
                    if (tasksMaternita[0].DicituraPeriodo != null && tasksMaternita[1].DicituraPeriodo != null
                        && (tasksMaternita[0].DicituraPeriodo.StartsWith(tasksMaternita[1].DicituraPeriodo) ||
                        tasksMaternita[1].DicituraPeriodo.StartsWith(tasksMaternita[0].DicituraPeriodo))
                        )
                    {
                        LdaRimuovere.AddRange(tasksMaternita.Where(x => x.TaskInCorso.TERMINATA != true));
                    }
                }
            }
            if (LdaRimuovere.Any())
                ListaTaskPronti.RemoveAll(x => LdaRimuovere.Contains(x));
            // }
            if (Richiesta.XR_MAT_TASK_IN_CORSO.Any() && Richiesta.ECCEZIONE == "MT" && Richiesta.DATA_NASCITA_BAMBINO != null)
            {
                List<TaskPronto> ListaTaskRicreati = ListaTaskProntiElaborati;// GetTaskProntiDaElaborazione(IdRichiesta, model);
                if (ListaTaskRicreati.Count() > ListaTaskPronti.Count())
                {
                    ListaTaskRicreati = ListaTaskRicreati.OrderBy(x => x.DataRiferimentoMeseAnno).ToList();
                    foreach (var t in ListaTaskRicreati)
                    {
                        bool GiaEsiste = (ListaTaskPronti.Any(x =>
                        x.DataRiferimentoMeseAnno == t.DataRiferimentoMeseAnno
                        && x.TaskInCorso.XR_MAT_ELENCO_TASK.NOME_TASK == t.TaskInCorso.XR_MAT_ELENCO_TASK.NOME_TASK));

                        if (!GiaEsiste)
                        {
                            t.TaskInCorso.BLOCCATA_DATETIME = DateTime.Now;
                            t.TaskInCorso.BLOCCATA_DA_OPERATORE = CommonHelper.GetCurrentUserMatricola();
                            ListaTaskPronti.Add(t);
                        }

                    }
                }
            }
            return ListaTaskPronti;
        }

        public static string TipoDipendente(string matricola, DateTime D)
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.AccountUtenteServizio)[0],
                                                             CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.AccountUtenteServizio)[1]);

            MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp response = cl.recuperaUtente(matricola.PadLeft(7, '0'), D.ToString("ddMMyyyy"));
            return response.data.tipo_dipendente;

        }
        public static Boolean IsFsuper(string matricola)
        {
            var db = new IncentiviEntities();
            var elencoCat = CezanneHelper.GetFSuperCat();
            //return db.SINTESI1.Any(x => x.COD_MATLIBROMAT == matricola && (x.COD_QUALIFICA == "Q10" || x.COD_QUALIFICA == "Q12"));
            return db.SINTESI1.Any(x => x.COD_MATLIBROMAT == matricola && elencoCat.Contains(x.COD_QUALIFICA));
        }
        public static bool GiaPresenteStornoCedolinoNeiTask(string matricola, int anno, int mese, int IdRichiestaCorrente)
        {
            var db = new IncentiviEntities();
            var task = db.XR_MAT_ELENCO_TASK.Where(x => x.NOME_TASK == "STORNO CEDOLINO").FirstOrDefault();
            var giapresente = db.XR_MAT_TASK_IN_CORSO.Any(x =>
                                             x.ID_TASK == task.ID &&
                                             x.XR_MAT_RICHIESTE.MATRICOLA == matricola &&
                                             x.ANNO == anno &&
                                             x.MESE == mese && x.ID_RICHIESTA != IdRichiestaCorrente

            );
            return giapresente;
        }
        public static string GetCampoPdf(byte[] pdf)
        {
            PdfReader reader2 = new PdfReader(pdf);
            string strText = string.Empty;

            for (int page = 1; page <= reader2.NumberOfPages; page++)
            {
                ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy();
                PdfReader reader = new PdfReader(pdf);
                String s = PdfTextExtractor.GetTextFromPage(reader, page, its);

                s = System.Text.Encoding.UTF8.GetString(System.Text.ASCIIEncoding.Convert(System.Text.Encoding.Default,
                    System.Text.Encoding.UTF8, System.Text.Encoding.Default.GetBytes(s)));
                strText = strText + s;
                reader.Close();
            }

            string numprot = GetText(CommonHelper.GetParametro<string>(EnumParametriSistema.RegexNumeroProtocollo), strText);
            string cf = GetText(CommonHelper.GetParametro<string>(EnumParametriSistema.RegexCF), strText);
            string dn = GetText(CommonHelper.GetParametro<string>(EnumParametriSistema.RegexDataNascita), strText);
            string periodo = GetText(CommonHelper.GetParametro<string>(EnumParametriSistema.RegexPeriodo), strText);

            return "Num Prot " + numprot + "<br />" +
                   "CF " + cf + "<br />" +
                   "DN " + dn + "<br />" +
                   "Periodo " + periodo + "<br />"
                ;
        }
        public static string GetText(string testoRegex, string testoInput)
        {
            System.Text.RegularExpressions.Regex Rprot = new System.Text.RegularExpressions.Regex(testoRegex, System.Text.RegularExpressions.RegexOptions.Singleline);
            var Matches = Rprot.Matches(testoInput);
            List<string> Res = new List<string>();

            if (Matches != null && Matches.Count > 0 && Matches[0].Groups != null && Matches[0].Groups.Count > 1)
            {
                for (int i = 1; i < Matches[0].Groups.Count; i++)
                {
                    Res.Add(Matches[0].Groups[i].Value);
                }
                return String.Join(",", Res.ToArray()).Trim('\n').Trim('\r');
            }
            return null;
        }

        public static string cprat(int id)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            try
            {
                var Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == id).FirstOrDefault();

                if (Richiesta == null)
                    return "NON TROVATA";


                foreach (var item in Richiesta.XR_MAT_NOTE.ToList())
                {
                    db.XR_MAT_NOTE.Remove(item);
                }
                db.SaveChanges();

                foreach (var item in Richiesta.XR_WKF_OPERSTATI.ToList())
                {
                    db.XR_WKF_OPERSTATI.Remove(item);
                }
                db.SaveChanges();

                foreach (var item in Richiesta.XR_MAT_ECCEZIONI.ToList())
                {
                    db.XR_MAT_ECCEZIONI.Remove(item);
                }
                db.SaveChanges();


                foreach (var item in Richiesta.XR_MAT_VOCI_CEDOLINO.ToList())
                {
                    db.XR_MAT_VOCI_CEDOLINO.Remove(item);
                }
                db.SaveChanges();

                foreach (var pian in Richiesta.XR_MAT_PIANIFICAZIONI.ToList())
                {
                    foreach (var g in pian.XR_MAT_GIORNI_CONGEDO.ToList())
                    {
                        db.XR_MAT_GIORNI_CONGEDO.Remove(g);
                    }
                    db.XR_MAT_PIANIFICAZIONI.Remove(pian);
                }
                db.SaveChanges();

                foreach (var item in Richiesta.XR_MAT_PROMEMORIA.ToList())
                {
                    db.XR_MAT_PROMEMORIA.Remove(item);
                }
                db.SaveChanges();

                foreach (var item in Richiesta.XR_MAT_TASK_IN_CORSO.ToList())
                {
                    db.XR_MAT_TASK_IN_CORSO.Remove(item);
                }
                db.SaveChanges();
                foreach (var item in Richiesta.XR_MAT_TASK_DI_SERVIZIO.ToList())
                {
                    db.XR_MAT_TASK_DI_SERVIZIO.Remove(item);
                }
                db.SaveChanges();

                foreach (var seg in Richiesta.XR_MAT_SEGNALAZIONI.ToList())
                {
                    foreach (var item in seg.XR_MAT_SEGNALAZIONI_COMUNICAZIONI.ToList())
                    {
                        foreach (var a in item.XR_MAT_ALLEGATI.ToList())
                        {
                            db.XR_MAT_ALLEGATI.Remove(a);
                        }
                        db.XR_MAT_SEGNALAZIONI_COMUNICAZIONI.Remove(item);
                    }

                    db.XR_MAT_SEGNALAZIONI.Remove(seg);
                }
                db.SaveChanges();


                foreach (var alle in Richiesta.XR_MAT_ALLEGATI.ToList())
                {
                    foreach (var item in alle.XR_MAT_SEGNALAZIONI_COMUNICAZIONI)
                    {
                        db.XR_MAT_SEGNALAZIONI_COMUNICAZIONI.Remove(item);
                    }
                    db.XR_MAT_ALLEGATI.Remove(alle);
                }
                db.SaveChanges();

                string desc = "Eliminata richiesta id " + Richiesta.ID + " " + Richiesta.NOMINATIVO + " " + Richiesta.ECCEZIONE;

                db.XR_MAT_RICHIESTE.Remove(Richiesta);
                db.SaveChanges();

                myRaiHelper.Logger.LogAzione(new MyRai_LogAzioni()
                {
                    provenienza = "EliminaPratica",
                    descrizione_operazione = desc,
                    operazione = "EliminaPratica"
                });
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

        public static MaternitaIndexModel GetMaternitaIndexModel()
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            MaternitaIndexModel model = new MaternitaIndexModel();
            model.IsResponsabileGestione =
                MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Gestione,
                MaternitaCongediHelper.MaternitaCongediGradiAbil.ADM);

            model.Categorie = db.XR_MAT_MACROCATEGORIE.Where(x => x.ATTIVA == true).OrderBy(x => x.ORDINE).ToList();
            SediMaternitaModel sedi = new SediMaternitaModel();
            model.Sedi =
               db.XR_MAT_RICHIESTE
               .Join(db.SINTESI1,
               r => r.MATRICOLA,
               s => s.COD_MATLIBROMAT,
               (r, s) => new { Ric = r, Sin = s })
               .Select(a => new SedeModel() { CodiceSede = a.Sin.COD_SEDE, DescSede = a.Sin.DES_SEDE }).Distinct().ToList();

            // model.Sedi = db.XR_MAT_RICHIESTE.Select(x => x.SEDEGAPP).Distinct().OrderBy(x => x).ToList();
            model.Stati = db.XR_MAT_STATI.OrderBy(x => x.ID_STATO).ToList();

            model.EnabledToGestioneGeneric =
                MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Gestione,
                MaternitaCongediHelper.MaternitaCongediGradiAbil.VIS) ||
                MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Gestione,
                MaternitaCongediHelper.MaternitaCongediGradiAbil.GEST) ||
                MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Gestione,
                MaternitaCongediHelper.MaternitaCongediGradiAbil.ADM);
            return model;
        }
        public static XR_MAT_TASK_IN_CORSO GetTaskInCorsoDaDB(int IdRichiestaCurrent, int IdTask, int anno, int mese, DateTime? DataDa, DateTime? DataA)
        {
            var db = new IncentiviEntities();
            var RichiestaCurrent = db.XR_MAT_RICHIESTE.Where(x => x.ID == IdRichiestaCurrent).FirstOrDefault();
            string MatricolaDipendente = RichiestaCurrent.MATRICOLA;
            string Eccezione = GetEccezioneRisultante(RichiestaCurrent);
            var TaskDaElenco = db.XR_MAT_ELENCO_TASK.Where(x => x.ID == IdTask).FirstOrDefault();

            if (TaskDaElenco.TIPO == "TRACCIATO")
            {
                string datada = DataDa.Value.ToString("dd/MM/yyyy");
                string dataa = DataA.Value.ToString("dd/MM/yyyy");

                return db.XR_MAT_TASK_IN_CORSO.FirstOrDefault(x =>
                    x.ID_RICHIESTA == IdRichiestaCurrent &&
                    x.XR_MAT_RICHIESTE.MATRICOLA == MatricolaDipendente &&
                    x.ID_TASK == IdTask &&
                    x.ANNO == anno &&
                    x.MESE == mese &&
                    x.NOTE == Eccezione + "-" + datada + "-" + dataa
                );
            }
            else
            {
                return db.XR_MAT_TASK_IN_CORSO.FirstOrDefault(x =>
                      x.ID_RICHIESTA == IdRichiestaCurrent &&
                      x.XR_MAT_RICHIESTE.MATRICOLA == MatricolaDipendente &&
                      x.ID_TASK == IdTask &&
                      x.ANNO == anno &&
                      x.MESE == mese &&
                      x.INPUT == Eccezione
              );
            }
        }
        public static XR_MAT_TASK_IN_CORSO IsTaskPresentDB(int IdRichiestaCurrent, int IdTask, int anno, int mese, DateTime? DataDa, DateTime? DataA)
        {
            var db = new IncentiviEntities();
            var RichiestaCurrent = db.XR_MAT_RICHIESTE.Where(x => x.ID == IdRichiestaCurrent).FirstOrDefault();
            string MatricolaDipendente = RichiestaCurrent.MATRICOLA;
            string Eccezione = GetEccezioneRisultante(RichiestaCurrent);
            var TaskDaElenco = db.XR_MAT_ELENCO_TASK.Where(x => x.ID == IdTask).FirstOrDefault();

            if (TaskDaElenco.TIPO == "TRACCIATO")
            {
                string datada = DataDa.Value.ToString("dd/MM/yyyy");
                string dataa = DataA.Value.ToString("dd/MM/yyyy");

                return db.XR_MAT_TASK_IN_CORSO.Where(x =>
                    x.ID_RICHIESTA != IdRichiestaCurrent &&
                    x.XR_MAT_RICHIESTE.MATRICOLA == MatricolaDipendente &&
                    x.ID_TASK == IdTask &&
                    x.ANNO == anno &&
                    x.MESE == mese &&
                    x.NOTE == Eccezione + "-" + datada + "-" + dataa
                ).FirstOrDefault();
            }
            else
            {
                return db.XR_MAT_TASK_IN_CORSO.Where(x =>
                      x.ID_RICHIESTA != IdRichiestaCurrent &&
                      x.XR_MAT_RICHIESTE.MATRICOLA == MatricolaDipendente &&
                      x.ID_TASK == IdTask &&
                      x.ANNO == anno &&
                      x.MESE == mese &&
                      x.INPUT == Eccezione
              ).FirstOrDefault();
            }

        }

        public static string GetCodiceFiscaleInfo(string cf, XR_MAT_RICHIESTE Rich)
        {
            MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            CodiceFiscaleReponse response = null;
            try
            {
                response = cl.GetCodiceFiscaleInfo(cf, Rich.MATRICOLA);
            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new MyRai_LogErrori()
                {
                    error_message = ex.ToString(),
                    provenienza = "GetCodiceFiscaleInfo"
                });
                return null;
            }
            if (response.CFinfo == null || !response.CFinfo.Any())
                return null;
            else
                return response.CFinfo.First().ProgressivoCarichiGenitore1;
        }
        public static DateTime? GetScadenzarioPerIdUfficio(string idUfficio, int anno, int mese)
        {
            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew s = new MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew();
            s.Credentials = new System.Net.NetworkCredential(
            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.ScadenzarioMeseAnnoResponse Response =
                s.GetScadenzarioMeseAnno(idUfficio, mese, anno);
            if (Response == null || Response.scadenze == null || Response.scadenze.Length == 0)
                return null;
            else
                return Response.scadenze[0].data_scadenza;
        }



        public static List<CampoContent> GetTracciatoEsploso(int idtracciato, int progressivo, string tracciatoDaDB, bool ISFromTE = false)
        {
            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew s = new MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew();
            //s.UseDefaultCredentials = true;

            s.Credentials = new System.Net.NetworkCredential(
            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.CampiTracciatoResponse response =
                s.GetCampiTracciato(idtracciato, progressivo);

            TracciatoFactory TF = new TracciatoFactory();
            TracciatoGenerico Tracciato = TF.GetTracciatoClass(idtracciato, response, null);
            if (Tracciato != null)
            {
                return Tracciato.ExplodeTrack(tracciatoDaDB);
            }
            else
            {
                return null;
            }

        }
        public static DateTime[] GetDateEstremiTracciato(XR_MAT_TASK_IN_CORSO tracciato)
        {
            if (tracciato == null || tracciato.NOTE == null) return null;

            string[] parti = tracciato.NOTE.Split('-');
            if (parti.Length > 2)
            {
                DateTime D1;
                if (DateTime.TryParseExact(parti[1], "dd/MM/yyyy", null, DateTimeStyles.None, out D1))
                {
                    DateTime D2;
                    if (DateTime.TryParseExact(parti[2], "dd/MM/yyyy", null, DateTimeStyles.None, out D2))
                    {
                        return new DateTime[] { D1, D2 };
                    }
                }
            }
            return null;
        }
        public static List<XR_MAT_RICHIESTE> GetRichiesteCompreseInRichiestaTracciato(XR_MAT_RICHIESTE Rich)
        {
            var db = new IncentiviEntities();
            var tracciati = Rich.XR_MAT_TASK_IN_CORSO.Where(x => x.NOTE != null).ToList();
            List<XR_MAT_RICHIESTE> LR = new List<XR_MAT_RICHIESTE>();

            foreach (var tracciato in tracciati)
            {
                string[] parti = tracciato.NOTE.Split('-');
                if (parti.Length > 2)
                {
                    DateTime D1;
                    if (DateTime.TryParseExact(parti[1], "dd/MM/yyyy", null, DateTimeStyles.None, out D1))
                    {
                        DateTime D2;
                        if (DateTime.TryParseExact(parti[2], "dd/MM/yyyy", null, DateTimeStyles.None, out D2))
                        {
                            var query = db.XR_MAT_RICHIESTE.Where(x =>
                                                x.XR_WKF_OPERSTATI.OrderByDescending(z => z.ID_STATO).Select(z => z.ID_STATO).FirstOrDefault() < 80 &&
                                                x.ID != Rich.ID &&
                                                x.XR_MAT_CATEGORIE.ID == Rich.XR_MAT_CATEGORIE.ID &&
                                                x.MATRICOLA == Rich.MATRICOLA &&
                                                    ((x.INIZIO_GIUSTIFICATIVO >= D1 && x.FINE_GIUSTIFICATIVO <= D2) ||
                                                    (x.DATA_INIZIO_MATERNITA >= D1 && x.DATA_FINE_MATERNITA <= D2)
                                                    )
                                                ).ToList();
                            if (query.Any()) LR.AddRange(query);
                        }
                    }
                }
            }
            return LR;
        }
        public static List<XR_MAT_RICHIESTE> GetRichiesteCompreseInRichiesta(XR_MAT_RICHIESTE Rich)
        {
            var db = new IncentiviEntities();

            DateTime? DInizio = Rich.INIZIO_GIUSTIFICATIVO;
            DateTime Dfine;

            var query = db.XR_MAT_RICHIESTE.Where(x =>
                                                x.XR_WKF_OPERSTATI.OrderByDescending(z => z.ID_STATO).Select(z => z.ID_STATO).FirstOrDefault() < 80 &&
                                                x.ID != Rich.ID &&
                                                x.XR_MAT_CATEGORIE.ID == Rich.XR_MAT_CATEGORIE.ID &&
                                                x.MATRICOLA == Rich.MATRICOLA);

            if (DInizio == null)
            {
                DInizio = Rich.DATA_INIZIO_MATERNITA;
                Dfine = Rich.DATA_FINE_MATERNITA.Value;
                query = query.Where(x => x.DATA_INIZIO_MATERNITA >= DInizio && x.DATA_FINE_MATERNITA <= Dfine);
            }
            else
            {
                Dfine = Rich.FINE_GIUSTIFICATIVO.Value;
                query = query.Where(x => x.INIZIO_GIUSTIFICATIVO >= DInizio && x.FINE_GIUSTIFICATIVO <= Dfine);
            }
            return query.ToList();
        }
        public static decimal GetGiorniLimiteDaRichiestaDB(XR_MAT_RICHIESTE Rich)
        {
            if (Rich == null) return 26;
            if (Rich.GIORNI_DEFAULT26 == null)
                return 26;
            else
                return (decimal)Rich.GIORNI_DEFAULT26;
        }
        public static decimal GetGiorniLimiteDaRichiestaDB(int idRichiesta)
        {
            var db = new IncentiviEntities();
            var Rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idRichiesta).FirstOrDefault();

            return GetGiorniLimiteDaRichiestaDB(Rich);
        }

        public static string GetGenericoCampoFromJson(List<AttributiAggiuntivi> attributi, string nomeJson)
        {
            string valore = attributi.Where(x => x.Id == nomeJson).Select(x => x.Valore).FirstOrDefault();
            return valore;
        }
        public static string GetTipoVariazioneFromJson(List<AttributiAggiuntivi> attributi)
        {
            string tipoVariazione = attributi.Where(x => x.Id == "EccezionePerAutomatismo").Select(x => x.Valore).FirstOrDefault();
            if (tipoVariazione != null)
            {
                tipoVariazione = tipoVariazione.PadLeft(3, '0');
            }
            return tipoVariazione;
        }
        public static DateTime? GetDataDecorrenzaFromJson(List<AttributiAggiuntivi> attributi)
        {
            var dataDec = attributi.Where(x => x.Id == "dt_inline").FirstOrDefault();
            if (dataDec != null && dataDec.InLine != null && dataDec.InLine.Any())
            {
                string d = dataDec.InLine.Where(x => x.Id == "data_decorrenza").Select(x => x.Valore).FirstOrDefault();
                if (!String.IsNullOrWhiteSpace(d))
                {
                    DateTime DataDecorrenza;
                    if (DateTime.TryParseExact(d, "dd/MM/yyyy", null, DateTimeStyles.None, out DataDecorrenza))
                    {
                        return DataDecorrenza;
                    }
                }
            }
            return null;
        }
        public static string GetCampiTracciato(int idtracciato, int progressivo, XR_MAT_RICHIESTE rich,
            string eccezionerisultante, string periododa, string periodoa, string giorni26mi,
            DateTime DataInizioPratica, string importoRetrib, string Importo13ma, string Importo14ma, string ImportoPremio,
            string ProgressivoFamiliare = "  ", string descrittiva = null, double? TotaleGiornalieroHC = null, XR_MAT_ELENCO_TASK taskElenco = null,
            string FormaContratto = null, bool IsTracciatoStorico = false)
        {
            var db = new IncentiviEntities();
            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew s = new MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew();
            //s.UseDefaultCredentials = true;

            s.Credentials = new System.Net.NetworkCredential(
            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.CampiTracciatoResponse response =
                s.GetCampiTracciato(idtracciato, progressivo);

            InfoPerTracciato info = null;

            if (taskElenco != null && taskElenco.TIPO == "TRACCIATO-TE")
            {
                myRaiCommonModel.TrattamentoEconomicoInfo TE = new TrattamentoEconomicoInfo();
                info = new InfoPerTracciato(richiesta: rich, giorni26mi: giorni26mi,
                importoRetribuzione: importoRetrib, inizioPeriodoEccezione: periododa, finePeriodoEccezione: periodoa,
                eccezioneRisultante: eccezionerisultante, dataInizioPratica: DataInizioPratica, importo13ma: Importo13ma,
                importo14ma: Importo14ma, importoPremio: ImportoPremio, progressivoFamiliare: ProgressivoFamiliare,
                descrittiva: descrittiva, TE: TE);
            }
            else
            {
                DateTime D1;
                DateTime.TryParseExact(periododa, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D1);
                float giorniferiali = GetWeekDaysInMonth(D1.Year, D1.Month);

                if (!IsTracciatoStorico)//3 aggiunti alla fine
                {
                    float g26 = Convert.ToSingle(giorni26mi);

                    if (g26 >= giorniferiali * 1.2) giorni26mi = GetGiorniLimiteDaRichiestaDB(rich).ToString();// "26";
                }
                else
                {
                    var g26 = Convert.ToDouble(giorni26mi) / 100f;
                    giorni26mi = g26.ToString();
                }


                info = new InfoPerTracciato(richiesta: rich, giorni26mi: giorni26mi,
                   importoRetribuzione: importoRetrib, inizioPeriodoEccezione: periododa, finePeriodoEccezione: periodoa,
                   eccezioneRisultante: eccezionerisultante, dataInizioPratica: DataInizioPratica, importo13ma: Importo13ma,
                   importo14ma: Importo14ma, importoPremio: ImportoPremio, progressivoFamiliare: ProgressivoFamiliare,
                   descrittiva: descrittiva, FormaContratto_K: FormaContratto);

                if (TotaleGiornalieroHC != null)
                    info.TotaleGiornalieroHC = TotaleGiornalieroHC.ToString().Replace(".", ",");
            }


            TracciatoFactory TF = new TracciatoFactory();
            TracciatoGenerico Tracciato = TF.GetTracciatoClass(idtracciato, response, info);

            List<AttributiAggiuntivi> attributi = new List<AttributiAggiuntivi>();
            string GGdec = "", MMdec = "", AAdec = "";
            string tipoVariazione = "";
            if (new int[] { 114, 131, 731 }.Contains(Tracciato.Response.IdTracciato))
            {
                attributi = TrattamentoEconomicoManager.GetDatiJson(info.Richiesta);
                DateTime? DataDecorrenza = GetDataDecorrenzaFromJson(attributi);
                if (DataDecorrenza != null)
                {
                    GGdec = DataDecorrenza.Value.ToString("dd");
                    MMdec = DataDecorrenza.Value.ToString("MM");
                    AAdec = DataDecorrenza.Value.ToString("yy");
                }
                tipoVariazione = GetTipoVariazioneFromJson(attributi);
            }
            if (Tracciato.Response.IdTracciato == 114)//mobilita orizzontale
            {
                if (attributi != null)
                {
                    string sede = GetGenericoCampoFromJson(attributi, "sede");
                    string servizio = GetGenericoCampoFromJson(attributi, "servizio");
                    string sezione = GetGenericoCampoFromJson(attributi, "sezione");

                    Tracciato.Info.CampiMobilitaOrizzontale = new Dictionary<string, string>();
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("TIPO SK", "12");
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("MATRICOLA RAI", rich.MATRICOLA);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("GG DECORRENZA", GGdec);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("MM DECORRENZA", MMdec);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("AA DECORRENZA", AAdec);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("TIPO VARIAZIONE", tipoVariazione);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("SEDE RAI", sede);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("SERVIZIO RAI", servizio);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("SEZIONE RAI", sezione);
                }
            }
            if (Tracciato.Response.IdTracciato == 131)//mobilita orizzontale
            {
                if (attributi != null)
                {
                    string repartoattivita = GetGenericoCampoFromJson(attributi, "repartoattivita");

                    string GGisc = "", MMisc = "", AAisc = "";
                    string dataIscrizione = GetGenericoCampoFromJson(attributi, "data_iscrizione");
                    if (!String.IsNullOrWhiteSpace(dataIscrizione))
                    {
                        DateTime DataIscr;
                        if (DateTime.TryParseExact(dataIscrizione, "dd/MM/yyyy", null, DateTimeStyles.None, out DataIscr))
                        {
                            GGisc = DataIscr.ToString("dd");
                            MMisc = DataIscr.ToString("MM");
                            AAisc = DataIscr.ToString("yy");
                        }
                    }
                    Tracciato.Info.CampiMobilitaOrizzontale = new Dictionary<string, string>();
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("TIPO SK", "30");
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("MATRICOLA RAI", rich.MATRICOLA);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("GG DECORRENZA", GGdec);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("MM DECORRENZA", MMdec);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("AA DECORRENZA", AAdec);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("TIPO VARIAZIONE", tipoVariazione);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("REPARTO o ATTIVITA", repartoattivita);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("GG ISCRIZIONE", GGisc);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("MM ISCRIZIONE", MMisc);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("AA ISCRIZIONE", AAisc);
                }
            }
            if (Tracciato.Response.IdTracciato == 731)//mobilita orizzontale
            {
                if (attributi != null)
                {
                    string sede = GetGenericoCampoFromJson(attributi, "sede");
                    string servizio = GetGenericoCampoFromJson(attributi, "servizio");

                    XR_VARIAZIONE_TEMP Variaz = new XR_VARIAZIONE_TEMP();
                    string idVarTemp = GetGenericoCampoFromJson(attributi, "id_xr_variazione_temp");
                    if (idVarTemp == null)
                    {
                        Variaz.COD_SERVIZIO = "--";
                        Variaz.DTA_FINE_TEMP = DateTime.Today;
                        Variaz.DTA_INIZIO_TEMP = DateTime.Today;
                    }
                    else
                    {
                        int idTemp = Convert.ToInt32(idVarTemp);

                        Variaz = db.XR_VARIAZIONE_TEMP.Where(x => x.ID_XR_VARIAZIONE_TEMP == idTemp).FirstOrDefault();
                        if (Variaz == null)
                            throw (new Exception("Variazione non trovata"));
                    }


                    Tracciato.Info.CampiMobilitaOrizzontale = new Dictionary<string, string>();
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("TIPO SK", "1D");
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("MATRICOLA RAI", rich.MATRICOLA);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("GG DECORRENZA", GGdec);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("MM DECORRENZA", MMdec);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("AA DECORRENZA", AAdec);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("TIPO VARIAZIONE", tipoVariazione);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("SERVIZIO ORIGINE", Variaz.COD_SERVIZIO);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("DATA INIZIO GIORNO", Variaz.DTA_INIZIO_TEMP.ToString("dd"));
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("DATA INIZIO MESE", Variaz.DTA_INIZIO_TEMP.ToString("MM"));
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("DATA INIZIO ANNO", Variaz.DTA_INIZIO_TEMP.ToString("yy"));
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("DATA FINE giorno", Variaz.DTA_FINE_TEMP.ToString("dd"));
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("DATA FINE MESE", Variaz.DTA_FINE_TEMP.ToString("MM"));
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("DATA FINE ANNO", Variaz.DTA_FINE_TEMP.ToString("yy"));
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("SEDE DESTINAZIONE", sede);
                    Tracciato.Info.CampiMobilitaOrizzontale.Add("SERVIZIO DESTINAZIONE", servizio);
                }
            }
            if (Tracciato != null)
            {



                var anag = myRaiHelper.BatchManager.GetUserData(rich.MATRICOLA, rich.INIZIO_GIUSTIFICATIVO ?? rich.DATA_INIZIO_MATERNITA.Value);
                Tracciato.IsFormaContratto8 = anag.forma_contratto == "8";

                if (anag.tipo_dipendente == "G")
                    Tracciato.IsFormaContratto8 = false;





                Tracciato.FillRecord();
                return Tracciato.TestoTracciato;
            }
            return null;
        }
        

        public static MaternitaDettagliGestioneModel GetMaternitaDettagliGestioneModel(int idrichiesta)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            MaternitaDettagliGestioneModel model = new MaternitaDettagliGestioneModel();


            model.Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            var idstato = model.Richiesta.XR_WKF_MATCON_OPERSTATI
                //.Where(x => db.XR_MAT_CATEGORIE.Select(z => z.CAT).Distinct().Contains(x.COD_TIPO_PRATICA))
                //.Where(x => x.COD_TIPO_PRATICA == "MAT" || x.COD_TIPO_PRATICA == "CON")
                .Max(x => x.ID_STATO);

            model.EccezioniPossibili = GetEccezioniPossibili(model.Richiesta);

            if (idstato == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione)
            {
                var matr =
                    model.Richiesta.XR_WKF_MATCON_OPERSTATI.Where(x =>
                x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione
                //&& db.XR_MAT_CATEGORIE.Select(z => z.CAT).Distinct().Contains(x.COD_TIPO_PRATICA)
                // (x.COD_TIPO_PRATICA == "MAT" || x.COD_TIPO_PRATICA == "CON")
                ).Select(z => z.COD_USER).FirstOrDefault();
                if (matr == CommonHelper.GetCurrentUserMatricola())
                {
                    model.InCaricoAMe = true;
                }
            }
            if (model.Richiesta.XR_MAT_SEGNALAZIONI.Any())
            {
                List<string> matricole = new List<string>();
                foreach (var s in model.Richiesta.XR_MAT_SEGNALAZIONI)
                {
                    foreach (var c in s.XR_MAT_SEGNALAZIONI_COMUNICAZIONI)
                    {
                        matricole.Add(c.MATRICOLA_FROM);
                        matricole.Add(c.MATRICOLA_TO);
                    }
                }
                foreach (var m in matricole.Distinct())
                {
                    var resp = myRaiHelper.ServiceWrapper.EsponiAnagraficaRaicvWrapped(m);
                    if (!string.IsNullOrWhiteSpace(resp) && resp.Split(';').Length > 28)
                    {
                        InfoMatricola im = new InfoMatricola()
                        {
                            matricola = m,
                            nominativo = resp.Split(';')[1] + " " + resp.Split(';')[2],
                            posizione = resp.Split(';')[28]
                        };
                        model.ListInfoMatricola.Add(im);
                    }
                }
            }
            return model;
        }

        private static List<L2D_ECCEZIONE> GetEccezioniPossibili(XR_MAT_RICHIESTE richiesta)
        {
            List<L2D_ECCEZIONE> L = new List<L2D_ECCEZIONE>();

            if (String.IsNullOrWhiteSpace(richiesta.XR_MAT_CATEGORIE.ECCEZIONE) ||
                richiesta.XR_MAT_CATEGORIE.ECCEZIONE.Contains("-") == false)
                return L;

            List<string> Codici = richiesta.XR_MAT_CATEGORIE.ECCEZIONE.Split('-').Select(x => x.Trim()).ToList();
            var db = new myRaiData.digiGappEntities();
            L = db.L2D_ECCEZIONE.Where(x => Codici.Contains(x.cod_eccezione.Trim())).OrderBy(x => x.cod_eccezione).ToList();
            return L;
        }

        public static List<TotaleEccezione> GetTotaleEccezioniGiornalisti(string matricola, DateTime d1, DateTime d2)
        {
            List<TotaleEccezione> Response = new List<TotaleEccezione>();
            MyRaiService1Client cl = new MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            GetContatoriEccezioniResponse resp = cl.GetContatoriEccezioni(matricola, d1, d2, new string[] { "DG55", "LN20", "LN25" });
            GetContatoriEccezioniResponse resp2 = cl.GetContatoriEccezioni(matricola, d1, d2, new string[] { "PDCO", "LF80", "LF36" });
            GetContatoriEccezioniResponse resp3 = cl.GetContatoriEccezioni(matricola, d1, d2, new string[] { "LEXF", "AR20", "RPAF" });
            GetContatoriEccezioniResponse resp4 = cl.GetContatoriEccezioni(matricola, d1, d2, new string[] { "LN16", "LN50" });
            foreach (var item in resp.ContatoriEccezioni)
            {
                var te = new TotaleEccezione()
                {
                    eccezione = item.CodiceEccezione
                };
                float f;
                float.TryParse(item.Totale, out f);
                te.totali = f;
                Response.Add(te);
            }
            foreach (var item in resp2.ContatoriEccezioni)
            {
                var te = new TotaleEccezione()
                {
                    eccezione = item.CodiceEccezione
                };
                float f;
                float.TryParse(item.Totale, out f);
                te.totali = f;
                Response.Add(te);
            }
            foreach (var item in resp3.ContatoriEccezioni)
            {
                var te = new TotaleEccezione()
                {
                    eccezione = item.CodiceEccezione
                };
                float f;
                float.TryParse(item.Totale, out f);
                te.totali = f;
                Response.Add(te);
            }
            foreach (var item in resp4.ContatoriEccezioni)
            {
                var te = new TotaleEccezione()
                {
                    eccezione = item.CodiceEccezione
                };
                float f;
                float.TryParse(item.Totale, out f);
                te.totali = f;
                Response.Add(te);
            }
            return Response;
        }
        public static GetPresenzeResponse GetEccezioniByGetPresenze(string matricola, DateTime datainizio, bool baseoraria)
        {
            GetPresenzeResponse Resp = new GetPresenzeResponse();

            List<RisultatoEccezioniGetPresenze> LR = new List<RisultatoEccezioniGetPresenze>();
            MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            GetSchedaPresenzeMeseResponse response =
                cl.GetSchedaPresenzeMese(matricola, datainizio, datainizio.AddMonths(1).AddDays(-1));

            Resp.ServiceResponse = response;

            string eccezioniCongedi = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniCongedi);
            if (baseoraria)
                eccezioniCongedi += ",AFQ,AFM,AFP,BFM,BFP,BFQ,CFM,CFP,CFQ";

            string[] EccezioniRilevantiCongedi = eccezioniCongedi.Split(',');

            foreach (var giorno in response.Giorni)
            {
                foreach (var m in giorno.MacroAssenze)
                {
                    if (!String.IsNullOrWhiteSpace(m) && m.Length > 1)
                    {
                        if (EccezioniRilevantiCongedi.Contains(m.Substring(1)))
                        {
                            var r = new RisultatoEccezioniGetPresenze()
                            {
                                Codice = m.Substring(1),
                                Data = giorno.data,
                                CodiceOrario = giorno.CodiceOrario
                            };
                            LR.Add(r);
                        }
                    }
                }
            }
            Resp.RisultatiEccezioniGetPresenze = LR;
            return Resp;
        }


        public static List<RisultatoEccezioni> GetAnalisiEccezioniGapp(string matricola, DateTime datainizio, List<string> eccezioni)
        {
            List<RisultatoEccezioni> LR = new List<RisultatoEccezioni>();
            if (eccezioni == null || !eccezioni.Any()) return LR;

            MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            DateTime datafine = datainizio.AddMonths(1).AddDays(-1);
            int i = 0;


            for (i = 0; i < eccezioni.Count; i++)
            {
                GetAnalisiEccezioniResponse response =
                    cl.GetAnalisiEccezioni(matricola, datainizio, datafine, eccezioni[i], null, null);
                if (response != null && response.DettagliEccezioni != null)
                {
                    foreach (var item in response.DettagliEccezioni)
                    {
                        RisultatoEccezioni R = new RisultatoEccezioni()
                        {
                            Codice = item.eccezione,
                            Data = item.data,
                            Giorni = item.giorni,
                            Minuti = item.minuti
                        };
                        LR.Add(R);
                    }
                }

            }
            return LR;
        }
        public static string GetEccezioneRisultante(XR_MAT_RICHIESTE rich)
        {
            if (!String.IsNullOrWhiteSpace(rich.ECCEZIONE))
                return rich.ECCEZIONE;

            if (rich.XR_MAT_CATEGORIE.ECCEZIONE.Contains("AF") && rich.XR_MAT_CATEGORIE.ECCEZIONE.Contains("BF"))
            {
                if (rich.DATA_NASCITA_BAMBINO.Value.AddYears(6) < DateTime.Now)
                    return "BF";//bf
                else
                    return "AF";//af  
            }
            else
                return rich.XR_MAT_CATEGORIE.ECCEZIONE;
        }
        public static bool IsEccezioneSospesa(string StatoEccezione)
        {
            if (StatoEccezione == null)
                return false;
            else
                return StatoEccezione == CommonHelper.GetParametri<string>(EnumParametriSistema.StatoEccezioneCongedi)[0];

        }
        public static bool IsEccezioneCestinata(string StatoEccezione)
        {
            if (StatoEccezione == null)
                return false;
            else
                return StatoEccezione == CommonHelper.GetParametri<string>(EnumParametriSistema.StatoEccezioneCongedi)[1];

        }
        public static int GetWeekDaysInMonth(int year, int month)
        {
            int days = DateTime.DaysInMonth(year, month);
            List<DateTime> dates = new List<DateTime>();
            for (int i = 1; i <= days; i++)
            {
                dates.Add(new DateTime(year, month, i));
            }

            int weekDays = dates.Where(d => d.DayOfWeek != DayOfWeek.Sunday & d.DayOfWeek != DayOfWeek.Saturday).Count();
            return weekDays;
        }
        public static XR_MAT_TASK_IN_CORSO GetTaskGiaSalvatoAltrePratiche(int IdTaskElenco, int IdRichiesta, int mese, int anno,
            string periododa, string periodoa)
        {
            var db = new IncentiviEntities();
            XR_MAT_TASK_IN_CORSO taskGiaSalvato = null;
            var Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == IdRichiesta).FirstOrDefault();
            string ecc = GetEccezioneRisultante(Richiesta);

            var task = db.XR_MAT_ELENCO_TASK.Where(x => x.ID == IdTaskElenco).FirstOrDefault();

            if (task.NOME_TASK.Trim() == "STORNO CEDOLINO")
            {
                taskGiaSalvato = db.XR_MAT_TASK_IN_CORSO
               .Where(x => x.ID_TASK == IdTaskElenco &&
               x.ID_RICHIESTA != IdRichiesta &&
               x.XR_MAT_RICHIESTE.MATRICOLA == Richiesta.MATRICOLA
               && x.MESE == mese && x.ANNO == anno).FirstOrDefault();
            }
            else if (task.TIPO == "TRACCIATO")
            {
                taskGiaSalvato = db.XR_MAT_TASK_IN_CORSO
                                       .Where(x => x.ID_TASK == IdTaskElenco &&
                                          x.ID_RICHIESTA != IdRichiesta &&
                                        x.XR_MAT_RICHIESTE.MATRICOLA == Richiesta.MATRICOLA
                                       && x.MESE == mese && x.ANNO == anno
                                       && x.NOTE != null
                                       && x.NOTE.Contains(periododa) && x.NOTE.Contains(periodoa)).FirstOrDefault();
            }
            else if (task.TIPO == "SERVIZIO")
            {
                taskGiaSalvato = db.XR_MAT_TASK_IN_CORSO
            .Where(x => x.ID_TASK == IdTaskElenco &&
            x.ID_RICHIESTA != IdRichiesta &&
            x.INPUT == ecc &&
               x.XR_MAT_RICHIESTE.MATRICOLA == Richiesta.MATRICOLA
            && x.MESE == mese && x.ANNO == anno).FirstOrDefault();
            }
            if (taskGiaSalvato != null &&
                taskGiaSalvato.TERMINATA == false &&
                taskGiaSalvato.BLOCCATA_DATETIME != null &&
                taskGiaSalvato.XR_MAT_RICHIESTE.XR_WKF_MATCON_OPERSTATI.Max(x => x.ID_STATO)
                                >= (int)MaternitaCongediManager.EnumStatiRichiesta.Approvata)
            {
                taskGiaSalvato = null;
            }
            return taskGiaSalvato;
        }
        public static XR_MAT_TASK_IN_CORSO GetTaskGiaSalvatoStessaPratica(int IdTaskElenco, int IdRichiesta, int mese, int anno,
            string periododa, string periodoa)
        {
            var db = new IncentiviEntities();
            XR_MAT_TASK_IN_CORSO taskGiaSalvato = null;

            var task = db.XR_MAT_ELENCO_TASK.Where(x => x.ID == IdTaskElenco).FirstOrDefault();

            if (task.NOME_TASK.Trim() == "STORNO CEDOLINO")
            {
                taskGiaSalvato = db.XR_MAT_TASK_IN_CORSO
               .Where(x => x.ID_TASK == IdTaskElenco &&
               x.ID_RICHIESTA == IdRichiesta
               && x.MESE == mese && x.ANNO == anno).FirstOrDefault();
            }
            else if (task.TIPO == "TRACCIATO")
            {
                taskGiaSalvato = db.XR_MAT_TASK_IN_CORSO
                                       .Where(x => x.ID_TASK == IdTaskElenco &&
                                          x.ID_RICHIESTA == IdRichiesta
                                       && x.MESE == mese && x.ANNO == anno
                                       && x.NOTE != null
                                       && x.NOTE.Contains(periododa) && x.NOTE.Contains(periodoa)).FirstOrDefault();
            }
            else if (task.TIPO == "SERVIZIO")
            {
                taskGiaSalvato = db.XR_MAT_TASK_IN_CORSO
            .Where(x => x.ID_TASK == IdTaskElenco &&
            x.ID_RICHIESTA == IdRichiesta
            && x.MESE == mese && x.ANNO == anno).FirstOrDefault();
            }
            return taskGiaSalvato;
        }
        public static bool IsHC(XR_MAT_RICHIESTE Ric)
        {
            return Ric.ECCEZIONE == "HC";
        }
        public static bool IsGM(XR_MAT_RICHIESTE Ric)
        {
            return Ric.ECCEZIONE == "GM";
        }
        public static decimal GetGiorniHC_GM(string matricola)
        {
            var db = new IncentiviEntities();
            string queryHRDW = CommonHelper.GetParametro<string>(EnumParametriSistema.QueryHrdwHC_GM).Replace("#MATR", matricola);
            IEnumerable<myRaiCommonModel.Gestionale.HRDW.HRDW_HC_GM> quantita =
                    db.Database.SqlQuery<myRaiCommonModel.Gestionale.HRDW.HRDW_HC_GM>(queryHRDW).ToList();
            if (quantita.Any())
                return quantita.Sum(x => x.Quantita);
            else
                return 0;
        }
        public static List<DettaglioGiorniModel> GetTotaleGiorni(DateTime Dinizio, string matricola, XR_MAT_RICHIESTE Richiesta)
        {
            List<DettaglioGiorniModel> LD = new List<DettaglioGiorniModel>();
            List<string> EccezioniRilevanti = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniCongedi)
                .Split(',').ToList();
            List<string> EccezioniRilevantiMezzaGiornata = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniCongediMezzaGiornata)
                .Split(',').ToList();
            MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                    CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            //  cl.Endpoint.Address = new System.ServiceModel.EndpointAddress("http://svildigigappws.servizi.rai.it/api/myraiservice1.svc");


            GetRuoliResponse Response =
                cl.GetRuoli(matricola, new DateTime(Dinizio.Year, Dinizio.Month, 1),
                "   ");

            //var e1 = Response.Eccezioni.Where(x => x.DataDocumento == new DateTime(2021, 5, 10)).FirstOrDefault();
            //var e2 = Response.Eccezioni.Where(x => x.DataDocumento == new DateTime(2021, 5, 11)).FirstOrDefault();
            //e1.DataDocumento = new DateTime(2021, 5, 8);
            //e2.DataDocumento = new DateTime(2021, 5, 9);



            string[] par = (CommonHelper.GetParametri<string>(EnumParametriSistema.GiorniTestCongedi));//giornifittizi
            if (par != null && par.Length > 1 && !String.IsNullOrWhiteSpace(par[0]) && !String.IsNullOrWhiteSpace(par[1]))
            {
                //    03/2021/AF/12,13,14,15,16;04/2021/AF/1,2,3,4
                if (matricola == par[0])
                {
                    string[] gruppi = par[1].Split(';');
                    foreach (string gruppo in gruppi)
                    {
                        string[] values = gruppo.Split('/');
                        if (Dinizio.Month == Convert.ToInt32(values[0]) && Dinizio.Year == Convert.ToInt32(values[1]))
                        {
                            string ecc = values[2];
                            bool primoSospeso = true;
                            bool secondoCestinato = true;
                            foreach (var day in values[3].Split(','))
                            {
                                DettaglioGiorniModel D = new DettaglioGiorniModel();
                                if (!primoSospeso)
                                {
                                    D.StatoEccez = CommonHelper.GetParametri<string>(EnumParametriSistema.StatoEccezioneCongedi)[0];
                                    primoSospeso = true;
                                }
                                else
                                {
                                    if (!secondoCestinato)
                                    {
                                        D.StatoEccez = CommonHelper.GetParametri<string>(EnumParametriSistema.StatoEccezioneCongedi)[1];
                                        secondoCestinato = true;
                                    }
                                }
                                D.CodiceEccezione = ecc;
                                D.DataDa = new DateTime(Dinizio.Year, Dinizio.Month, Convert.ToInt32(day));
                                D.DataA = null;
                                D.DescEccezione = CommonHelper.GetDescrizioneEccezione(ecc);
                                D.NumeroGiorniRuoli = (float)120 / 100f;
                                if (IsHalf(ecc))
                                    D.NumeroGiorniRuoli = D.NumeroGiorniRuoli / 2f;
                                if (ecc.EndsWith("Q"))
                                    D.NumeroGiorniRuoli = D.NumeroGiorniRuoli / 4f;
                                LD.Add(D);
                            }
                        }
                    }

                }
            }
            //if (CommonHelper.GetParametro<bool>(EnumParametriSistema.Giorni_AF_fittizi) && matricola=="103650")
            //{
            //    if (Dinizio.Year == 2021 && Dinizio.Month == 3)
            //    {
            //        int[] giornifittizi = new int[] { 1,2,3,4,5, 6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31 };
            //        foreach (int giorno in giornifittizi)
            //        {
            //            DettaglioGiorniModel D = new DettaglioGiorniModel();
            //            D.CodiceEccezione = "AF";
            //            D.DataDa = new DateTime(2021, 3, giorno);
            //            D.DataA = null;
            //            D.DescEccezione = CommonHelper.GetDescrizioneEccezione("AF");
            //            D.NumeroGiorniRuoli = (float)120 / 100f;
            //            LD.Add(D);
            //        }
            //    }

            //}

            if (Response == null || Response.Eccezioni == null || !Response.Eccezioni.Any())
                return LD;




            int quantita = 0;
            string CodiciOrarioZero = CommonHelper.GetParametro<string>(EnumParametriSistema.CodiciOrarioValoreZeroPerCongedi);
            List<string> CodiciZero = new List<string>();
            if (!String.IsNullOrWhiteSpace(CodiciOrarioZero))
                CodiciZero = CodiciOrarioZero.Split(',').ToList();

            foreach (var ecc in Response.Eccezioni)
            {
                if (EccezioniRilevanti.Contains(ecc.CodiceEccezione.Trim()))
                {
                    DettaglioGiorniModel D = new DettaglioGiorniModel();
                    D.CodiceEccezione = ecc.CodiceEccezione.Trim();
                    D.DataDa = ecc.DataDocumento;
                    D.DataA = null;// ecc.DataDocumento;
                    D.DescEccezione = CommonHelper.GetDescrizioneEccezione(ecc.CodiceEccezione.Trim());
                    quantita = Convert.ToInt32(ecc.Quantita);
                    D.StatoEccez = ecc.StatoEccezione;

                    if (CodiciZero.Contains(ecc.CodiceOrario)) quantita = 0;
                    else quantita = 120;

                    D.NumeroGiorniRuoli = (float)quantita / 100f;
                    D.Stornato = ecc.FlagStorno == "*";
                    LD.Add(D);
                    if (ecc.FlagStorno == "*")
                    {
                        DettaglioGiorniModel Ds = new DettaglioGiorniModel();
                        Ds.CodiceEccezione = ecc.CodiceEccezione.Trim();
                        Ds.DataDa = ecc.DataDocumento;
                        Ds.DataA = null;// ecc.DataDocumento;
                        Ds.DescEccezione = CommonHelper.GetDescrizioneEccezione(ecc.CodiceEccezione.Trim());
                        quantita = Convert.ToInt32(ecc.Quantita);
                        Ds.StatoEccez = ecc.StatoEccezione;
                        Ds.Storno = true;
                        if (CodiciZero.Contains(ecc.CodiceOrario)) quantita = 0;
                        else quantita = 120;

                        Ds.NumeroGiorniRuoli = (float)quantita / -100f;

                        LD.Add(Ds);
                    }
                }
                else if (EccezioniRilevantiMezzaGiornata.Contains(ecc.CodiceEccezione.Trim()))
                {
                    DettaglioGiorniModel D = new DettaglioGiorniModel();
                    D.CodiceEccezione = ecc.CodiceEccezione.Trim();
                    D.DataDa = ecc.DataDocumento;
                    D.DataA = null;// ecc.DataDocumento;
                    D.DescEccezione = CommonHelper.GetDescrizioneEccezione(ecc.CodiceEccezione.Trim());
                    quantita = Convert.ToInt32(ecc.Quantita);
                    D.StatoEccez = ecc.StatoEccezione;

                    if (CodiciZero.Contains(ecc.CodiceOrario)) quantita = 0;
                    else quantita = 60;

                    D.NumeroGiorniRuoli = (float)quantita / 100f;

                    LD.Add(D);
                }
            }
            //  if (Richiesta.PIANIFICAZIONE_BASE_ORARIA == true) //non unire
            return LD;

            List<DettaglioGiorniModel> LDgrouped = new List<DettaglioGiorniModel>();
            var listGrouped = LD.GroupBy(x => x.CodiceEccezione).ToList();
            foreach (var eccezione in listGrouped)
            {
                foreach (var item in eccezione)
                {
                    if (!LDgrouped.Any(x => x.CodiceEccezione == item.CodiceEccezione))
                    {
                        DettaglioGiorniModel D = new DettaglioGiorniModel();
                        D.CodiceEccezione = item.CodiceEccezione.Trim();
                        D.DataDa = item.DataDa;
                        D.DataA = null;
                        D.DescEccezione = CommonHelper.GetDescrizioneEccezione(item.CodiceEccezione.Trim());
                        D.NumeroGiorniRuoli = item.NumeroGiorniRuoli;
                        D.StatoEccez = item.StatoEccez;
                        LDgrouped.Add(D);
                    }
                    else
                    {
                        var giornoPrecedente = LDgrouped.Where(x => x.CodiceEccezione == item.CodiceEccezione && (
                         (x.DataDa == item.DataDa.AddDays(-1) && x.DataA == null)
                         ||
                         (x.DataA == item.DataDa.AddDays(-1)))
                        ).FirstOrDefault();

                        if (giornoPrecedente != null
                            && !IsEccezioneSospesa(giornoPrecedente.StatoEccez)
                            && !IsEccezioneSospesa(item.StatoEccez))
                        // && (giornoPrecedente.StatoEccez ==null || giornoPrecedente.StatoEccez.StartsWith("S")==false)
                        //  && (item.StatoEccez==null || item.StatoEccez.StartsWith("S")==false))
                        {
                            giornoPrecedente.DataA = item.DataDa;
                            giornoPrecedente.NumeroGiorniRuoli += item.NumeroGiorniRuoli;
                        }
                        else
                        {
                            DettaglioGiorniModel D = new DettaglioGiorniModel();
                            D.CodiceEccezione = item.CodiceEccezione.Trim();
                            D.DataDa = item.DataDa;
                            D.DataA = null;
                            D.DescEccezione = CommonHelper.GetDescrizioneEccezione(item.CodiceEccezione.Trim());
                            D.NumeroGiorniRuoli = item.NumeroGiorniRuoli;
                            D.StatoEccez = item.StatoEccez;
                            LDgrouped.Add(D);
                        }
                    }
                }
            }

            return LDgrouped.OrderBy(x => x.DataDa).ToList();
        }

        public static MaternitaDettagliRichiestaGestioneModel GetMaternitaDettagliRichiestaGestioneModel(int idrichiesta)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            MaternitaDettagliRichiestaGestioneModel model = new MaternitaDettagliRichiestaGestioneModel();
            model.Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            return model;
        }
        public static bool IsStatePresentInWorkflow(XR_MAT_RICHIESTE Ric, int stato)
        {
            var db = new IncentiviEntities();
            var StatiWorkflow = db.XR_WKF_WORKFLOW.Where(x => x.ID_TIPOLOGIA == Ric.XR_MAT_CATEGORIE.ID_TIPOLOGIA_WORKFLOW)
                                .OrderBy(x => x.ORDINE).Select(x => x.ID_STATO).ToList();
            return StatiWorkflow.Contains(stato);
        }
        public static int GetStatoSuccessivoSecondoWorkflow(XR_MAT_RICHIESTE Ric)
        {
            var db = new IncentiviEntities();
            List<int> StatiWorkflow = db.XR_WKF_WORKFLOW.Where(x => x.ID_TIPOLOGIA == Ric.XR_MAT_CATEGORIE.ID_TIPOLOGIA_WORKFLOW)
                                .OrderBy(x => x.ORDINE).Select(x => x.ID_STATO).ToList();
            int StatoAttuale = Ric.XR_WKF_MATCON_OPERSTATI.Max(x => x.ID_STATO);
            if (!StatiWorkflow.Contains(StatoAttuale))
            {
                throw new Exception("Stato attuale non trovato nel workflow");
            }
            int? StatoSuccessivo = StatiWorkflow.SkipWhile(x => x <= StatoAttuale).FirstOrDefault();
            if (StatoSuccessivo == null)
            {
                throw new Exception("Stato successivo non trovato nel workflow");
            }
            return (int)StatoSuccessivo;
        }
        public static int GetStatoPrecedenteSecondoWorkflow(MaternitaCongediHelper.MaternitaCongediUffici uff, XR_MAT_RICHIESTE Ric, List<XR_WKF_WORKFLOW> listWkf)
        {
            var db = new IncentiviEntities();
            //var StatiWorkflow = db.XR_WKF_WORKFLOW.Where(x => x.ID_TIPOLOGIA == Ric.XR_MAT_CATEGORIE.ID_TIPOLOGIA_WORKFLOW)
            //                    .OrderBy(x => x.ORDINE).Select(x => x.ID_STATO).ToList();
            var StatiWorkflow = listWkf.Where(x => x.ID_TIPOLOGIA == Ric.XR_MAT_CATEGORIE.ID_TIPOLOGIA_WORKFLOW)
                                .OrderBy(x => x.ORDINE).Select(x => x.ID_STATO).ToList();
            int statoPrecedente = 0;

            if (uff == MaternitaCongediHelper.MaternitaCongediUffici.Gestione)
            {
                statoPrecedente = StatiWorkflow.Where(x => x < (int)EnumStatiRichiesta.InCaricoGestione)
                   .OrderByDescending(x => x)
                    .FirstOrDefault();
            }
            else if (uff == MaternitaCongediHelper.MaternitaCongediUffici.Personale)
            {
                statoPrecedente = StatiWorkflow.Where(x => x < (int)EnumStatiRichiesta.InCaricoUffPers)
                   .OrderByDescending(x => x)
                    .FirstOrDefault();
            }
            else if (uff == MaternitaCongediHelper.MaternitaCongediUffici.Amministrazione)
            {
                statoPrecedente = StatiWorkflow.Where(x => x < (int)EnumStatiRichiesta.InCaricoAmmin)
                  .OrderByDescending(x => x)
                   .FirstOrDefault();
            }
            return statoPrecedente;
        }

        public static string GetSesso(string matricola)
        {
            var db = new IncentiviEntities();
            string s = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matricola).Select(x => x.COD_SESSO).FirstOrDefault();
            return s;
        }
        public static myRaiCommonModel.MaternitaCongediModel GetMaternitaCongediModel(string mese = null,
            string matr = null, string sede = null, int? tipo = null, int? statorich = null, string mesetask = null,
            string ordine = null, string assenza = null, string listone = null, bool SoloInCaricoAme = false)
        {

            /*
            01	Amministrazione
            02	Gestione
            03	Ufficio Personale

            01VIS	Amministrazione: sola lettura	
            01GEST	Amministrazione: lettura/scrittura	P912685,P652740
            01ADM	Amministrazione: referente/responsabile	P652740, 684930
            01RES	Amminstrazione: gestione residenza retroattiva	

            02VIS	Gestione: sola lettura	P527786
            02GEST	Gestione: lettura/scrittura	P103650,P909317
            02ADM	Gestione: referente/responsabile	P103650

            03VIS	Ufficio del Personale: sola lettura	
            03GEST	Ufficio del Personale: lettura/scrittura	P451598
            03ADM	Ufficio del Personale: referente/responsabile	
            
10	Inviata
20	Presa in carico ufficio Gestione
30	Approvata ufficio Gestione
40	Presa in carico ufficio Personale
50	Approvata ufficio Personale
60	Presa in carico ufficio Amministrazione
70	Approvata ufficio Amministrazione
80	Chiusa
             */
            myRaiCommonModel.MaternitaCongediModel model = new myRaiCommonModel.MaternitaCongediModel();


            var db = new myRaiData.Incentivi.IncentiviEntities();
            // db.Configuration.LazyLoadingEnabled = false;

            //var richieste = db.XR_MAT_RICHIESTE.Include("XR_WKF_OPERSTATI").Where(x => x.ECCEZIONE != "SW" && x.XR_MAT_CATEGORIE.ECCEZIONE != null).ToList();

            bool daRicerca = false;






            var QueryRichiesteQueryable =

                db.XR_MAT_RICHIESTE
                //.Include(m => m.XR_WKF_OPERSTATI)

                .Where(x => x.ECCEZIONE != "SW" && x.XR_MAT_CATEGORIE.ECCEZIONE != null)

                .Join(db.SINTESI1,
                r => r.MATRICOLA,
                s => s.COD_MATLIBROMAT,
                (r, s) => new { Ric = r, Sin = s, Stati = r.XR_WKF_OPERSTATI });


            var rrr = QueryRichiesteQueryable.Where(x => x.Ric.ID == 8524).FirstOrDefault();
            //var test = QueryRichiesteQueryable.ToList();
            //var R= test.First();

            //DateTime d11 = DateTime.Now;
            //var stati = R.Stati .ToList();
            //DateTime d22 = DateTime.Now;
            //var mseccc = (d22 - d11).TotalMilliseconds;

            if (HttpContext.Current.Request.Cookies["maternita-cookie"] != null)
            {
                List<int> IDs = new List<int>();
                string v = HttpContext.Current.Request.Cookies["maternita-cookie"].Value;
                if (!String.IsNullOrWhiteSpace(v))
                {
                    foreach (string id in v.Split(','))
                    {
                        int idInt = 0;
                        if (int.TryParse(id, out idInt))
                        {
                            IDs.Add(idInt);
                        }
                    }
                }
                if (IDs.Any())
                {
                    if (statorich != 999)
                        QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x => !IDs.Contains(x.Ric.ID));
                    else
                        QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x => IDs.Contains(x.Ric.ID));
                }
            }
            if (statorich == 5)
            {
                QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x => x.Ric.DA_RIAVVIARE == true && x.Ric.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) < 80);
            }
            if (!String.IsNullOrWhiteSpace(mese))
            {
                DateTime D1;
                DateTime.TryParseExact("01/" + mese, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D1);
                DateTime D2 = D1.AddMonths(1);
                QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x => x.Ric.DATA_INVIO_RICHIESTA >= D1 && x.Ric.DATA_INVIO_RICHIESTA < D2);
            }
            if (assenza == "L")
            {
                daRicerca = true;
                QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x => x.Ric.ASSENZA_LUNGA == true);
            }
            if (assenza == "B")
            {
                daRicerca = true;
                QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x => x.Ric.ASSENZA_LUNGA != true);
            }
            if (!String.IsNullOrWhiteSpace(matr))
            {
                daRicerca = true;
                QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x => x.Ric.MATRICOLA == matr || x.Ric.NOMINATIVO.Contains(matr));
            }
            if (!String.IsNullOrWhiteSpace(sede))
            {
                daRicerca = true;
                QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x => x.Sin.COD_SEDE == sede);
            }
            if (tipo != null)
            {
                daRicerca = true;
                QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x => x.Ric.XR_MAT_CATEGORIE.ID_MACROCAT == tipo);
            }


            if (!String.IsNullOrWhiteSpace(mesetask))
            {
                daRicerca = true;
                DateTime D1;
                DateTime.TryParseExact("01/" + mesetask, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D1);
                DateTime D2 = D1.AddMonths(1);
                QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x => x.Ric.XR_MAT_TASK_IN_CORSO.Any(z => z.TERMINATA && z.DATA_ULTIMO_TENTATIVO >= D1
               && z.DATA_ULTIMO_TENTATIVO < D2));

            }
            if (listone == "1")
            {
                QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x => x.Ric.XR_MAT_TASK_IN_CORSO.Any(z => z.ID_TASK == 15));
            }

            List<string> Matricole = MaternitaCongediHelper.EnabledtoMatricole(QueryRichiesteQueryable.Select(x => x.Ric.MATRICOLA).Distinct().ToList());
            QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x => Matricole.Contains(x.Ric.MATRICOLA));

            var rrr2 = QueryRichiesteQueryable.Where(x => x.Ric.ID == 8524).FirstOrDefault();
            //quali vedere :
            int PreviousState = 0, InCaricoQuestoUfficioState = 0, stateA = 0, stateB = 0;
            if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Gestione
                   ))
            {
                PreviousState = (int)EnumStatiRichiesta.Inviata;
                InCaricoQuestoUfficioState = (int)EnumStatiRichiesta.InCaricoGestione;
                model.MyOffice = MaternitaCongediHelper.MaternitaCongediUffici.Gestione;
                if (statorich != null && statorich != 999)
                {
                    if (statorich == 1) { stateA = 0; stateB = 10; }
                    if (statorich == 2) { stateA = 10; stateB = 30; }
                    if (statorich == 3) { stateA = 30; stateB = 99; }
                    if (statorich == 4) { stateA = 100; stateB = 999; }
                }
            }
            if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Personale
                ))
            {
                PreviousState = (int)EnumStatiRichiesta.ApprovataGestione;
                InCaricoQuestoUfficioState = (int)EnumStatiRichiesta.InCaricoUffPers;
                model.OpenByUfficioPersonale = true;
                model.MyOffice = MaternitaCongediHelper.MaternitaCongediUffici.Personale;
                if (statorich != null && statorich != 999)
                {
                    if (statorich == 1) { stateA = 30; stateB = 40; }
                    if (statorich == 2) { stateA = 40; stateB = 60; }
                    if (statorich == 3) { stateA = 60; stateB = 99; }
                    if (statorich == 4) { stateA = 100; stateB = 999; }
                }
            }
            if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Amministrazione
               ))
            {
                PreviousState = (int)EnumStatiRichiesta.ApprovataUffPers;
                InCaricoQuestoUfficioState = (int)EnumStatiRichiesta.InCaricoAmmin;
                model.MyOffice = MaternitaCongediHelper.MaternitaCongediUffici.Amministrazione;
                model.OpenByUfficioAmministrazione = true;

                if (statorich != null && statorich != 999)
                {
                    if (statorich == 1) { stateA = 30; stateB = 60; }
                    if (statorich == 2) { stateA = 60; stateB = 80; }
                    if (statorich == 3) { stateA = 79; stateB = 99; }
                    if (statorich == 4) { stateA = 100; stateB = 999; }
                }
            }

            if (statorich != null && statorich != 5 && statorich != 999)
            {
                QueryRichiesteQueryable = QueryRichiesteQueryable.Where(x =>
                (x.Ric.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) > stateA && x.Ric.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) < stateB));

                model.RicercaPerID_STATO = statorich;
            }
            if (!QueryRichiesteQueryable.Any())
                return model;

            var QueryRichieste = QueryRichiesteQueryable.ToList();
            var ricb = QueryRichieste.Where(x => x.Ric.ID == 8524).FirstOrDefault();

            var listWkf = db.XR_WKF_WORKFLOW.ToList();

            var listIdGest = QueryRichieste.Select(x => x.Ric.ID).ToList();
            var operStati = db.XR_WKF_OPERSTATI.Where(x => listIdGest.Contains(x.ID_GESTIONE) && x.NASCOSTO == false && (x.COD_TIPO_PRATICA == "MAT" || x.COD_TIPO_PRATICA == "CON")).ToList();



            foreach (var queryItem in QueryRichieste)
            {
                if (queryItem.Ric.ID == 8524)
                {

                }
                //if (statorich == null)
                //  statorich = queryItem.Ric.XR_WKF_OPERSTATI.Max(z => z.ID_STATO);
                //if (queryItem.Ric.ID == 517)
                //{

                //}

                //var stato = queryItem.Ric.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();
                var stato = operStati.Where(x => x.ID_GESTIONE == queryItem.Ric.ID).OrderByDescending(x => x.ID_STATO).FirstOrDefault();


                if (stato == null) continue;

                if (statorich != null && statorich != 999)
                {
                    if ((stateA < (int)MaternitaCongediManager.EnumStatiRichiesta.Inviata) && (stateB > (int)MaternitaCongediManager.EnumStatiRichiesta.Inviata))
                    {
                        model.RichiesteInCaricoNessuno.Add(queryItem.Ric);
                    }
                    else
                    {

                        var statoRichiestoRow = queryItem.Stati.Where(x => x.ID_STATO == stato.ID_STATO).FirstOrDefault();
                        if (statoRichiestoRow.COD_USER == CommonHelper.GetCurrentUserMatricola())
                        {
                            model.RichiesteInCaricoAme.Add(queryItem.Ric);
                        }
                        else
                        {
                            model.RichiesteInCaricoAltri.Add(queryItem.Ric);
                        }
                    }
                }
                else
                {
                    //  if (stato.ID_STATO == PreviousState ||
                    //(model.OpenByUfficioPersonale && stato.ID_STATO == (int)EnumStatiRichiesta.ApprovataUffPers && stato.UFFPERS_PRESA_VISIONE == false))
                    //  {
                    //      model.RichiesteInCaricoNessuno.Add(queryItem.Ric);
                    //  }

                    PreviousState = GetStatoPrecedenteSecondoWorkflow(model.MyOffice, queryItem.Ric, listWkf);
                    //if (IsPN)
                    //{
                    //    if (model.MyOffice == MaternitaCongediHelper.MaternitaCongediUffici.Personale)
                    //        PreviousState = (int)EnumStatiRichiesta.Inviata;
                    //}


                    if (stato.ID_STATO == PreviousState || (PreviousState == 31 && (stato.ID_STATO == 30 || stato.ID_STATO == 31)))
                    {
                        if (model.MyOffice == MaternitaCongediHelper.MaternitaCongediUffici.Personale)
                        {
                            if (IsStatePresentInWorkflow(queryItem.Ric, (int)EnumStatiRichiesta.InCaricoUffPers))
                            {
                                model.RichiesteInCaricoNessuno.Add(queryItem.Ric);
                            }
                            else
                            {
                                if (stato.UFFPERS_PRESA_VISIONE != true)
                                {
                                    model.RichiesteInCaricoNessuno.Add(queryItem.Ric);
                                }
                            }
                        }
                        else if (model.MyOffice == MaternitaCongediHelper.MaternitaCongediUffici.Amministrazione)
                        {
                            if (queryItem.Ric.PRESA_VISIONE_RESP_GEST != null)
                            {
                                model.RichiesteInCaricoNessuno.Add(queryItem.Ric);
                            }
                        }
                        else
                        {
                            model.RichiesteInCaricoNessuno.Add(queryItem.Ric);
                        }
                    }
                    if (stato.ID_STATO == InCaricoQuestoUfficioState)
                    {

                        if (stato.COD_USER == CommonHelper.GetCurrentUserMatricola() && stato.ID_STATO == InCaricoQuestoUfficioState)
                        {
                            model.RichiesteInCaricoAme.Add(queryItem.Ric);
                        }
                        else
                        {
                            model.RichiesteInCaricoAltri.Add(queryItem.Ric);
                        }
                    }
                }


            }

            model.RichiesteAggregateInCaricoAme = GetRichiesteAggregatePerMatricola(model.RichiesteInCaricoAme);

            model.RichiesteAggregateInCaricoAme = model.RichiesteAggregateInCaricoAme
                .OrderByDescending(X => X.ListaRichiesteAggregate.Sum(z => z.XR_MAT_PROMEMORIA.Count()))
                .ThenBy(x => x.ListaRichiesteAggregate.Min(z => z.DATA_SCADENZA))
                .ToList();

            if (String.IsNullOrWhiteSpace(ordine)) ordine = "S1";

            model.OrdineRichiesto = ordine;

            if (SoloInCaricoAme == false)
            {
                model.RichiesteAggregateInCaricoAltri = GetRichiesteAggregatePerMatricola(model.RichiesteInCaricoAltri);
                model.RichiesteAggregateInCaricoNessuno = GetRichiesteAggregatePerMatricola(model.RichiesteInCaricoNessuno);
            }



            if (ordine == "N1")
            {
                model.RichiesteAggregateInCaricoAme = model.RichiesteAggregateInCaricoAme
                     .OrderBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim()).ToList();
                model.RichiesteAggregateInCaricoAltri = model.RichiesteAggregateInCaricoAltri
                     .OrderBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim()).ToList();
                model.RichiesteAggregateInCaricoNessuno = model.RichiesteAggregateInCaricoNessuno
                     .OrderBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim()).ToList();

                foreach (var item in model.RichiesteAggregateInCaricoNessuno)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                    {
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.INIZIO_GIUSTIFICATIVO).ThenBy(x => x.DATA_INIZIO_MATERNITA)
                            .ToList();
                    }
                }
                foreach (var item in model.RichiesteAggregateInCaricoAltri)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                    {
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.INIZIO_GIUSTIFICATIVO).ThenBy(x => x.DATA_INIZIO_MATERNITA)
                            .ToList();
                    }
                }
                foreach (var item in model.RichiesteAggregateInCaricoAme)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                    {
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.INIZIO_GIUSTIFICATIVO).ThenBy(x => x.DATA_INIZIO_MATERNITA)
                            .ToList();
                    }
                }
            }
            else if (ordine == "N2")
            {
                model.RichiesteAggregateInCaricoAme = model.RichiesteAggregateInCaricoAme
                      .OrderByDescending(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim()).ToList();
                model.RichiesteAggregateInCaricoAltri = model.RichiesteAggregateInCaricoAltri
                    .OrderByDescending(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim()).ToList();
                model.RichiesteAggregateInCaricoNessuno = model.RichiesteAggregateInCaricoNessuno
                     .OrderByDescending(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim()).ToList();

                foreach (var item in model.RichiesteAggregateInCaricoNessuno)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                    {
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.INIZIO_GIUSTIFICATIVO).ThenBy(x => x.DATA_INIZIO_MATERNITA)
                            .ToList();
                    }
                }
                foreach (var item in model.RichiesteAggregateInCaricoAltri)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                    {
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.INIZIO_GIUSTIFICATIVO).ThenBy(x => x.DATA_INIZIO_MATERNITA)
                            .ToList();
                    }
                }
                foreach (var item in model.RichiesteAggregateInCaricoAme)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                    {
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.INIZIO_GIUSTIFICATIVO).ThenBy(x => x.DATA_INIZIO_MATERNITA)
                            .ToList();
                    }
                }
            }

            else if (ordine == "T1")
            {
                foreach (var item in model.RichiesteAggregateInCaricoAme)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.XR_MAT_CATEGORIE.TITOLO)
                            .ThenBy(x => x.NOMINATIVO.Trim())

                            .ToList();
                }
                foreach (var item in model.RichiesteAggregateInCaricoAltri)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.XR_MAT_CATEGORIE.TITOLO)
                                    .ThenBy(x => x.NOMINATIVO.Trim()).ToList();
                }
                foreach (var item in model.RichiesteAggregateInCaricoNessuno)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.XR_MAT_CATEGORIE.TITOLO)
                                    .ThenBy(x => x.NOMINATIVO.Trim()).ToList();
                }
                model.RichiesteAggregateInCaricoAme = model.RichiesteAggregateInCaricoAme
                   .OrderBy(x => x.ListaRichiesteAggregate.First().XR_MAT_CATEGORIE.TITOLO)
                   .ThenBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim())
                   .ThenBy(x => x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO != null
                   ? x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO
                   : x.ListaRichiesteAggregate.First().DATA_INIZIO_MATERNITA)
                   .ToList();


                model.RichiesteAggregateInCaricoAltri = model.RichiesteAggregateInCaricoAltri
                   .OrderBy(x => x.ListaRichiesteAggregate.First().XR_MAT_CATEGORIE.TITOLO)
                    .ThenBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim())
                     .ThenBy(x => x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO != null
                   ? x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO
                   : x.ListaRichiesteAggregate.First().DATA_INIZIO_MATERNITA)
                    .ToList();
                model.RichiesteAggregateInCaricoNessuno = model.RichiesteAggregateInCaricoNessuno
                   .OrderBy(x => x.ListaRichiesteAggregate.First().XR_MAT_CATEGORIE.TITOLO)
                    .ThenBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim())
                     .ThenBy(x => x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO != null
                   ? x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO
                   : x.ListaRichiesteAggregate.First().DATA_INIZIO_MATERNITA)
                    .ToList();
            }

            else if (ordine == "T2")
            {
                foreach (var item in model.RichiesteAggregateInCaricoAme)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderByDescending(x => x.XR_MAT_CATEGORIE.TITOLO).ToList();
                }
                foreach (var item in model.RichiesteAggregateInCaricoAltri)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderByDescending(x => x.XR_MAT_CATEGORIE.TITOLO).ToList();
                }
                foreach (var item in model.RichiesteAggregateInCaricoNessuno)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderByDescending(x => x.XR_MAT_CATEGORIE.TITOLO).ToList();
                }
                model.RichiesteAggregateInCaricoAme = model.RichiesteAggregateInCaricoAme
                   .OrderByDescending(x => x.ListaRichiesteAggregate.First().XR_MAT_CATEGORIE.TITOLO)
                    .ThenBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim())
                     .ThenBy(x => x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO != null
                   ? x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO
                   : x.ListaRichiesteAggregate.First().DATA_INIZIO_MATERNITA)
                    .ToList();

                model.RichiesteAggregateInCaricoAltri = model.RichiesteAggregateInCaricoAltri
                                   .OrderByDescending(x => x.ListaRichiesteAggregate.First().XR_MAT_CATEGORIE.TITOLO)
                                    .ThenBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim())
                     .ThenBy(x => x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO != null
                   ? x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO
                   : x.ListaRichiesteAggregate.First().DATA_INIZIO_MATERNITA)
                    .ToList();

                model.RichiesteAggregateInCaricoNessuno = model.RichiesteAggregateInCaricoNessuno
                                   .OrderByDescending(x => x.ListaRichiesteAggregate.First().XR_MAT_CATEGORIE.TITOLO)
                                    .ThenBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim())
                     .ThenBy(x => x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO != null
                   ? x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO
                   : x.ListaRichiesteAggregate.First().DATA_INIZIO_MATERNITA)
                    .ToList();

            }
            else if (ordine == "S1")
            {
                foreach (var item in model.RichiesteAggregateInCaricoAme)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.DATA_SCADENZA).ToList();
                }
                foreach (var item in model.RichiesteAggregateInCaricoAltri)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.DATA_SCADENZA).ToList();
                }
                foreach (var item in model.RichiesteAggregateInCaricoNessuno)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.DATA_SCADENZA).ToList();
                }
                model.RichiesteAggregateInCaricoAme = model.RichiesteAggregateInCaricoAme
                   .OrderBy(x => x.ListaRichiesteAggregate.First().DATA_SCADENZA)
                   .ThenBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim())
                     .ThenBy(x => x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO != null
                   ? x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO
                   : x.ListaRichiesteAggregate.First().DATA_INIZIO_MATERNITA)
                    .ToList();
                model.RichiesteAggregateInCaricoAltri = model.RichiesteAggregateInCaricoAltri
                   .OrderBy(x => x.ListaRichiesteAggregate.First().DATA_SCADENZA)
                   .ThenBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim())
                     .ThenBy(x => x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO != null
                   ? x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO
                   : x.ListaRichiesteAggregate.First().DATA_INIZIO_MATERNITA)
                    .ToList();
                model.RichiesteAggregateInCaricoNessuno = model.RichiesteAggregateInCaricoNessuno
                   .OrderBy(x => x.ListaRichiesteAggregate.First().DATA_SCADENZA)
                   .ThenBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim())
                     .ThenBy(x => x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO != null
                   ? x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO
                   : x.ListaRichiesteAggregate.First().DATA_INIZIO_MATERNITA)
                    .ToList();
            }
            else if (ordine == "S2")
            {
                foreach (var item in model.RichiesteAggregateInCaricoAme)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderByDescending(x => x.DATA_SCADENZA).ToList();
                }
                foreach (var item in model.RichiesteAggregateInCaricoAltri)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderByDescending(x => x.DATA_SCADENZA).ToList();
                }
                foreach (var item in model.RichiesteAggregateInCaricoNessuno)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderByDescending(x => x.DATA_SCADENZA).ToList();
                }
                model.RichiesteAggregateInCaricoAme = model.RichiesteAggregateInCaricoAme
                   .OrderByDescending(x => x.ListaRichiesteAggregate.First().DATA_SCADENZA)
                 .ThenBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim())
                     .ThenBy(x => x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO != null
                   ? x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO
                   : x.ListaRichiesteAggregate.First().DATA_INIZIO_MATERNITA)
                    .ToList();
                model.RichiesteAggregateInCaricoAltri = model.RichiesteAggregateInCaricoAltri
                   .OrderByDescending(x => x.ListaRichiesteAggregate.First().DATA_SCADENZA)
                   .ThenBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim())
                     .ThenBy(x => x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO != null
                   ? x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO
                   : x.ListaRichiesteAggregate.First().DATA_INIZIO_MATERNITA)
                    .ToList();
                model.RichiesteAggregateInCaricoNessuno = model.RichiesteAggregateInCaricoNessuno
                   .OrderByDescending(x => x.ListaRichiesteAggregate.First().DATA_SCADENZA)
                 .ThenBy(x => x.ListaRichiesteAggregate.First().NOMINATIVO.Trim())
                     .ThenBy(x => x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO != null
                   ? x.ListaRichiesteAggregate.First().INIZIO_GIUSTIFICATIVO
                   : x.ListaRichiesteAggregate.First().DATA_INIZIO_MATERNITA)
                    .ToList();
            }
            else if (ordine == "A1")
            {
                foreach (var item in model.RichiesteAggregateInCaricoAme)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.XR_MAT_TASK_IN_CORSO.Any()).ToList();
                }
                foreach (var item in model.RichiesteAggregateInCaricoAltri)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.XR_MAT_TASK_IN_CORSO.Any()).ToList();
                }
                foreach (var item in model.RichiesteAggregateInCaricoNessuno)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderBy(x => x.XR_MAT_TASK_IN_CORSO.Any()).ToList();
                }
                model.RichiesteAggregateInCaricoAme = model.RichiesteAggregateInCaricoAme
                   .OrderBy(x => x.ListaRichiesteAggregate.First().XR_MAT_TASK_IN_CORSO.Any()).ToList();
                model.RichiesteAggregateInCaricoAltri = model.RichiesteAggregateInCaricoAltri
                   .OrderBy(x => x.ListaRichiesteAggregate.First().XR_MAT_TASK_IN_CORSO.Any()).ToList();
                model.RichiesteAggregateInCaricoNessuno = model.RichiesteAggregateInCaricoNessuno
                   .OrderBy(x => x.ListaRichiesteAggregate.First().XR_MAT_TASK_IN_CORSO.Any()).ToList();
            }
            else if (ordine == "A2")
            {
                foreach (var item in model.RichiesteAggregateInCaricoAme)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderByDescending(x => x.XR_MAT_TASK_IN_CORSO.Any()).ToList();
                }
                foreach (var item in model.RichiesteAggregateInCaricoAltri)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderByDescending(x => x.XR_MAT_TASK_IN_CORSO.Any()).ToList();
                }
                foreach (var item in model.RichiesteAggregateInCaricoNessuno)
                {
                    if (item.ListaRichiesteAggregate.Count > 1)
                        item.ListaRichiesteAggregate = item.ListaRichiesteAggregate
                            .OrderByDescending(x => x.XR_MAT_TASK_IN_CORSO.Any()).ToList();
                }
                model.RichiesteAggregateInCaricoAme = model.RichiesteAggregateInCaricoAme
                   .OrderByDescending(x => x.ListaRichiesteAggregate.First().XR_MAT_TASK_IN_CORSO.Any()).ToList();
                model.RichiesteAggregateInCaricoAltri = model.RichiesteAggregateInCaricoAltri
                   .OrderByDescending(x => x.ListaRichiesteAggregate.First().XR_MAT_TASK_IN_CORSO.Any()).ToList();
                model.RichiesteAggregateInCaricoNessuno = model.RichiesteAggregateInCaricoNessuno
                   .OrderByDescending(x => x.ListaRichiesteAggregate.First().XR_MAT_TASK_IN_CORSO.Any()).ToList();
            }
            if (model.RichiesteAggregateInCaricoAme.Any(x => x.ListaRichiesteAggregate.Any(z => z.DA_RIAVVIARE == true)))
            {
                var Dariavv = model.RichiesteAggregateInCaricoAme.Where(x => x.ListaRichiesteAggregate.Any(z => z.DA_RIAVVIARE == true)).ToList();
                var NonDaRiavv = model.RichiesteAggregateInCaricoAme.Where(x => !x.ListaRichiesteAggregate.Any(z => z.DA_RIAVVIARE == true)).ToList();
                model.RichiesteAggregateInCaricoAme = new List<RichiestePerMatricola>(Dariavv);
                model.RichiesteAggregateInCaricoAme.AddRange(NonDaRiavv);
            }
            if (model.RichiesteAggregateInCaricoAltri.Any(x => x.ListaRichiesteAggregate.Any(z => z.DA_RIAVVIARE == true)))
            {
                var Dariavv = model.RichiesteAggregateInCaricoAltri.Where(x => x.ListaRichiesteAggregate.Any(z => z.DA_RIAVVIARE == true)).ToList();
                var NonDaRiavv = model.RichiesteAggregateInCaricoAltri.Where(x => !x.ListaRichiesteAggregate.Any(z => z.DA_RIAVVIARE == true)).ToList();
                model.RichiesteAggregateInCaricoAltri = new List<RichiestePerMatricola>(Dariavv);
                model.RichiesteAggregateInCaricoAltri.AddRange(NonDaRiavv);
            }
            if (model.RichiesteAggregateInCaricoNessuno.Any(x => x.ListaRichiesteAggregate.Any(z => z.DA_RIAVVIARE == true)))
            {
                var Dariavv = model.RichiesteAggregateInCaricoNessuno.Where(x => x.ListaRichiesteAggregate.Any(z => z.DA_RIAVVIARE == true)).ToList();
                var NonDaRiavv = model.RichiesteAggregateInCaricoNessuno.Where(x => !x.ListaRichiesteAggregate.Any(z => z.DA_RIAVVIARE == true)).ToList();
                model.RichiesteAggregateInCaricoNessuno = new List<RichiestePerMatricola>(Dariavv);
                model.RichiesteAggregateInCaricoNessuno.AddRange(NonDaRiavv);
            }
            return model;
        }
        public static XR_MAT_RICHIESTE IsRichiestaLavorataAltrove(XR_MAT_RICHIESTE R)
        {

            var db = new IncentiviEntities();
            if (R.INIZIO_GIUSTIFICATIVO == null && R.DATA_INIZIO_MATERNITA == null) return null;
            DateTime D1 = R.INIZIO_GIUSTIFICATIVO ?? R.DATA_INIZIO_MATERNITA.Value;

            if (R.FINE_GIUSTIFICATIVO == null && R.DATA_FINE_MATERNITA == null) return null;
            DateTime D2 = R.FINE_GIUSTIFICATIVO ?? R.DATA_FINE_MATERNITA.Value;

            if (D1.Month == D2.Month && D1.Year == D2.Year)
            {
                var RichMadre = db.XR_MAT_RICHIESTE
                    .Where(x => x.MATRICOLA == R.MATRICOLA &&
                                x.ECCEZIONE == R.ECCEZIONE &&
                                x.ID != R.ID &&
                                x.XR_MAT_TASK_IN_CORSO.Any() &&
                                x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) <= 80 &&
                                (x.FINE_GIUSTIFICATIVO != null && x.FINE_GIUSTIFICATIVO.Value.Month == D2.Month
                                 ||
                                 x.DATA_FINE_MATERNITA != null && x.DATA_FINE_MATERNITA.Value.Month == D2.Month)
                            ).FirstOrDefault();

                if (RichMadre != null)
                {
                    DateTime DataEnd = RichMadre.FINE_GIUSTIFICATIVO ?? RichMadre.DATA_FINE_MATERNITA.Value;
                    DateTime DataEndUltimoDelMese = new DateTime(DataEnd.Year, DataEnd.Month, DateTime.DaysInMonth(DataEnd.Year, DataEnd.Month));

                    if (RichMadre.XR_WKF_OPERSTATI.Max(x => x.ID_STATO) >= 80)//se   è chiusa (06/03/2023 Vincenzo)
                        return null;

                    else if (DataEndUltimoDelMese < DateTime.Today)//se siamo  nel mese successivo al mese in oggetto della pratica (06/03/2023 Vincenzo)
                        return null;

                    else return RichMadre;
                }

            }
            return null;
        }
        public static IsAvviataResponse IsAvviata(int idRichiesta)
        {
            var db = new IncentiviEntities();

            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idRichiesta).FirstOrDefault();
            if (rich == null) return null;

            IsAvviataResponse Resp = new IsAvviataResponse();
            if (rich.XR_MAT_TASK_IN_CORSO.Any())
            {
                Resp.Avviata = true;
                Resp.MatricolaOperatore = rich.XR_MAT_TASK_IN_CORSO.First().MATRICOLA_OPERATORE;
                Resp.DataAvviata = rich.XR_MAT_TASK_IN_CORSO.First().DATA_CREAZIONE;
            }
            else
            {
                Resp.Avviata = false;
                Resp.MatricolaOperatore = null;
                Resp.DataAvviata = null;
            }
            return Resp;
        }
        public static bool ProvieneDaDEM(XR_MAT_RICHIESTE rich)
        {
            var db = new IncentiviEntities();
            return db.XR_DEM_DOCUMENTI.Any(x => x.Id_Richiesta == rich.ID);
        }

        public static bool IsHalf(string ecc)
        {
            return !String.IsNullOrWhiteSpace(ecc) &&
                (ecc.EndsWith("M") || ecc.EndsWith("P")) &&
                ecc.Length >= 3;
        }
        public static string ImportaArretratiMatricola(string matricola)
        {
            string query = CommonHelper.GetParametro<string>(EnumParametriSistema.QueryGiorniArretratiCongediHrdw)
                .Replace("#MATR#", matricola);
            var dbHRDW = new myRaiData.CurriculumVitae.cv_ModelEntities();

            List<CongediArretratiHRDW> res = dbHRDW.Database.SqlQuery<CongediArretratiHRDW>(query).ToList();
            string esito = res.Count() + ",";
            int inse = 0;
            var db = new IncentiviEntities();
            foreach (var day in res)
            {
                var arr = db.XR_MAT_ARRETRATI_DIPENDENTE.Where(x => x.MATRICOLA == matricola && x.DATA == day.data).FirstOrDefault();
                if (arr == null)
                {
                    decimal q = 1.0M;
                    if (MaternitaCongediManager.IsHalf(day.cod_eccezione))
                        q = 0.5M;
                    else if (day.cod_eccezione.EndsWith("Q"))
                        q = 0.25M;

                    XR_MAT_ARRETRATI_DIPENDENTE newarr = new XR_MAT_ARRETRATI_DIPENDENTE()
                    {
                        DATA = day.data,
                        ECCEZIONE = day.cod_eccezione,
                        MATRICOLA = matricola,
                        NOMINATIVO = day.nominativo,
                        QUANTITA = q
                    };
                    db.XR_MAT_ARRETRATI_DIPENDENTE.Add(newarr);
                    inse++;

                }
            }
            db.SaveChanges();
            return esito + inse;
        }


        public static List<RichiestePerMatricola> GetRichiesteAggregatePerMatricola(List<XR_MAT_RICHIESTE> Richieste)
        {
            bool AggregaSempreMatricole = true;//se ci ripensano

            List<RichiestePerMatricola> LR = new List<RichiestePerMatricola>();

            foreach (var r in Richieste)
            {
                DateRange DR = GetDateRange(r);

                if (LR.Where(x => x.Matricola == r.MATRICOLA).Any() == false)
                {
                    RichiestePerMatricola rm = new RichiestePerMatricola();
                    rm.Matricola = r.MATRICOLA;
                    if (r.INIZIO_GIUSTIFICATIVO != null)
                        rm.DataRiferimentoOrdineVisualizzazione = r.INIZIO_GIUSTIFICATIVO.Value;
                    else
                        rm.DataRiferimentoOrdineVisualizzazione = r.DATA_INIZIO_MATERNITA.Value;
                    rm.ListaRichiesteAggregate.Add(r);

                    LR.Add(rm);
                }
                else
                {
                    XR_MAT_RICHIESTE RichiestaAggregata = null;

                    foreach (var richAggr in LR.Where(x => x.Matricola == r.MATRICOLA))
                    {
                        foreach (var rich in richAggr.ListaRichiesteAggregate)
                        {
                            DateRange Drich = GetDateRange(rich);

                            bool overlaps = DR.DateStart < Drich.DateEnd && Drich.DateStart < DR.DateEnd;
                            if (AggregaSempreMatricole || (overlaps && rich.XR_MAT_CATEGORIE.ID == r.XR_MAT_CATEGORIE.ID))
                            {
                                richAggr.ListaRichiesteAggregate.Add(r);
                                RichiestaAggregata = r;
                                break;
                            }
                        }
                        if (RichiestaAggregata != null)
                        {
                            if (RichiestaAggregata.INIZIO_GIUSTIFICATIVO != null &&
                                richAggr.DataRiferimentoOrdineVisualizzazione > RichiestaAggregata.INIZIO_GIUSTIFICATIVO)
                                richAggr.DataRiferimentoOrdineVisualizzazione = RichiestaAggregata.INIZIO_GIUSTIFICATIVO.Value;
                            if (RichiestaAggregata.DATA_INIZIO_MATERNITA != null &&
                                richAggr.DataRiferimentoOrdineVisualizzazione > RichiestaAggregata.DATA_INIZIO_MATERNITA)
                                richAggr.DataRiferimentoOrdineVisualizzazione = RichiestaAggregata.DATA_INIZIO_MATERNITA.Value;

                            break;
                        }

                    }
                    if (RichiestaAggregata == null)
                    {
                        RichiestePerMatricola rm = new RichiestePerMatricola();
                        rm.Matricola = r.MATRICOLA;
                        if (r.INIZIO_GIUSTIFICATIVO != null)
                            rm.DataRiferimentoOrdineVisualizzazione = r.INIZIO_GIUSTIFICATIVO.Value;
                        else
                            rm.DataRiferimentoOrdineVisualizzazione = r.DATA_INIZIO_MATERNITA.Value;
                        rm.ListaRichiesteAggregate.Add(r);

                        LR.Add(rm);
                    }
                }
            }
            return LR.OrderBy(x => x.DataRiferimentoOrdineVisualizzazione).ToList();
        }
        public static DateRange GetDateRange(XR_MAT_RICHIESTE r)
        {
            DateRange DR = new DateRange();
            DateTime? D1 = r.INIZIO_GIUSTIFICATIVO;
            DateTime? D2 = r.FINE_GIUSTIFICATIVO;

            if (D1 == null)
            {
                D1 = r.DATA_INIZIO_MATERNITA;
                D2 = r.DATA_FINE_MATERNITA;
            }

            DateTime PrimoDelMeseInizioGiustificativo = new DateTime(D1.Value.Year, D1.Value.Month, 1);
            DateTime UltimoDelMeseFineGiustificativo = new DateTime(D2.Value.Year, D2.Value.Month, 1).AddMonths(1).AddDays(-1);
            DR.DateStart = PrimoDelMeseInizioGiustificativo;
            DR.DateEnd = UltimoDelMeseFineGiustificativo;

            return DR;
        }
        public static string GetAllegatoTipologia(string tipo)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            return db.XR_MAT_TIPOALLEGATI.Where(x => x.TIPO == tipo).Select(x => x.TITOLO).FirstOrDefault();
        }
        public static DateTime? GetScadenza(myRaiData.Incentivi.XR_MAT_RICHIESTE ric)
        {
            //ric = new IncentiviEntities().XR_MAT_RICHIESTE.Where(x => x.ID == 131).FirstOrDefault();
            if (ric.XR_MAT_CATEGORIE.ID_SCADENZARIO_PANGEA != null)
            {
                MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew s = new MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew();
                //s.UseDefaultCredentials = true;

                s.Credentials = new System.Net.NetworkCredential(
                CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

                MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.ScadenzarioMeseAnnoResponse response =
                    s.GetScadenzarioMeseAnno(
                    ric.XR_MAT_CATEGORIE.ID_SCADENZARIO_PANGEA.ToString(),
                    ric.DATA_INVIO_RICHIESTA.AddMonths(1).Month,
                    ric.DATA_INVIO_RICHIESTA.AddMonths(1).Year);

                if (response.esito && response.scadenze.Any())
                {
                    return response.scadenze.First().data_scadenza;
                }
            }
            return null;
        }

        public static string GetRewDew(int anno, int mese)
        {
            if (DateTime.Now.Year == anno && DateTime.Now.Month == mese)
                return "DEW";
            else
                return "DEW";
        }
        public static string GetUfficioperNote()
        {
            if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Amministrazione))
                return "A";
            else if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Personale))
                return "P";
            else if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Gestione))
                return "G";

            return "";
        }
        public static string SalvaNota(int idrichiesta, string testo, string visibilita, HttpPostedFileBase file, int? idnota, string nomefile)
        {
            var db = new IncentiviEntities();
            XR_MAT_NOTE nota = null;

            if (visibilita != "*")
            {
                visibilita = GetUfficioperNote();
            }
            if (idnota != null)
                nota = db.XR_MAT_NOTE.Where(x => x.ID == idnota).FirstOrDefault();
            else
                nota = new XR_MAT_NOTE();

            try
            {
                nota.MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola();
                nota.DATA_INSERIMENTO = DateTime.Now;
                nota.TESTO = testo;
                nota.VISIBILITA = visibilita;
                if (file != null)
                {
                    MemoryStream target = new MemoryStream();
                    file.InputStream.CopyTo(target);
                    nota.FILE_CONTENT = target.ToArray();
                    nota.FILE_NAME = nomefile;
                }
                if (idnota == null)
                {
                    var richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
                    nota.XR_MAT_RICHIESTE = richiesta;
                    db.XR_MAT_NOTE.Add(nota);
                }

                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string InserisciPN(XR_MAT_RICHIESTE rich)
        {
            var db = new digiGappEntities();
            if (rich == null || rich.INIZIO_GIUSTIFICATIVO == null || rich.FINE_GIUSTIFICATIVO == null)
            {
                return "Dati non validi";
            }
            DateTime D1 = rich.INIZIO_GIUSTIFICATIVO.Value;
            DateTime D2 = rich.FINE_GIUSTIFICATIVO.Value;
            int giorni = 0;
            for (DateTime Dcurrent = D1; Dcurrent <= D2; Dcurrent = Dcurrent.AddDays(1))
            {
                MyRaiServiceInterface.it.rai.servizi.digigappws.dayResponse resp =
                        HomeManager.GetEccezioni(Dcurrent.ToString("ddMMyyyy"), rich.MATRICOLA);
                if (!String.IsNullOrWhiteSpace(resp.giornata.orarioReale) &&
                    !resp.giornata.orarioReale.StartsWith("9")
                    && !resp.eccezioni.Any(x => x.cod.Trim() == "PN"))
                {
                    var anag = BatchManager.GetUserData(rich.MATRICOLA, rich.INIZIO_GIUSTIFICATIVO ?? rich.DATA_INIZIO_MATERNITA.Value);
                    myRaiData.MyRai_PianoFerieBatch P = new MyRai_PianoFerieBatch()
                    {
                        codice_eccezione = "PN",
                        provenienza = "HRIS/Congedi-DA_PIANOFERIE=FALSE-APPROVATORE_UFFPERS",
                        dalle = "",
                        alle = "",
                        importo = "",
                        data_eccezione = Dcurrent,
                        matricola = rich.MATRICOLA,
                        data_creazione_record = DateTime.Now,
                        quantita = "1",
                        sedegapp = anag.sede_gapp
                    };
                    db.MyRai_PianoFerieBatch.Add(P);
                    giorni++;
                    if (giorni == 2)
                    {
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }

                        return null;
                    }
                }
            }
            return "Impossibile inserire 2 giorni di PN nell'intervallo dato";
        }
    }



    internal class Evidenze9000
    {
        public string Matricola { get; internal set; }
        public int Anno { get; internal set; }
        public int Mese { get; internal set; }
        public string Eccezione { get; internal set; }
        public DateTime PeriodoA { get; internal set; }
        public DateTime PeriodoDa { get; internal set; }
        public string Testo { get; internal set; }
    }

    public class DateRange
    {
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
    }
}
