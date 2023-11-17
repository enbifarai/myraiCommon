using myRai.DataAccess;
using myRaiData;
using myRai.Data.CurriculumVitae;
using myRaiDataTalentia;
using myRaiData.Incentivi;
using myRaiServiceHub.Autorizzazioni;
using myRaiServiceHub.it.rai.servizi.sendmail;
using MyRaiServiceInterface;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web; //GetImageWithOverride
using System.Web.Helpers;

using CommonManager = myRaiHelper.CommonHelper;
using EccezioniManager = myRaiHelper.EccezioniHelper;
using Utente = myRaiHelper.UtenteHelper;
using FestivitaManager = myRaiHelper.FestivitaHelper;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Linq.Expressions;

namespace myRaiHelper
{
    public class CommonHelper
    {
        private static MyRai_ParametriSistema GetParametro(EnumParametriSistema chiave)
        {
            MyRai_ParametriSistema p = null;
            String NomeParametro = chiave.ToString();

            switch (chiave)
            {
                case EnumParametriSistema.AccountUtenteServizio:
                case EnumParametriSistema.UrlImmagineDipendente:
                case EnumParametriSistema.MatricoleAdmin:
                case EnumParametriSistema.BypassOrariGappMatricole:
                case EnumParametriSistema.TipiDipQuadraturaSettimanale:
                case EnumParametriSistema.TipiDipQuadraturaGiornaliera:
                case EnumParametriSistema.GetLimitTrasferte:
                case EnumParametriSistema.PoliticheEccezioniAssenze:
                //Per le notifiche:
                case EnumParametriSistema.SocietaCorsiOnline:
                case EnumParametriSistema.TipiContrattoCorsiOlnlie:
                case EnumParametriSistema.NotificaCVZero:
                case EnumParametriSistema.NotificaCVLess100:
                case EnumParametriSistema.NotificaCV100:
                case EnumParametriSistema.CVEditorialiSezContabiliAbilitate:
                    if (SessionHelper.Get("RPM-PARAM-" + NomeParametro) != null)
                        p = (MyRai_ParametriSistema)SessionHelper.Get("RPM-PARAM-" + NomeParametro);
                    else
                    {
                        using (digiGappEntities db = new digiGappEntities())
                        {
                            p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == NomeParametro);
                            SessionHelper.Set("RPM-PARAM-" + NomeParametro, p);
                        }
                    }
                    break;
                case EnumParametriSistema.OrariGapp: //OrariGapp non può essere cachato
                default:
                    using (digiGappEntities db = new digiGappEntities())
                    {
                        //System.Diagnostics.Debug.WriteLine("RPM-" + NomeParametro);
                        p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == NomeParametro);
                    }
                    break;
            }

            return p;
        }

        public static string IsStornato(MyRai_Pianificazione day)
        {
            var db = new digiGappEntities();

            var row = (db.MyRai_Eccezioni_Richieste.Where(x => x.MyRai_Richieste.matricola_richiesta == day.Matricola
             && x.cod_eccezione == "SW"
             && x.data_eccezione == day.Data
             && x.azione == "C"
             && x.id_stato == 20 &&
             day.DataInserimento < x.data_validazione_primo_livello)).FirstOrDefault();
            string nota = null;
            if (row != null)
            {
                nota = "Storno approvato " +
                   (row.matricola_primo_livello != null ? "da " + row.matricola_primo_livello : "") +
                   (row.data_validazione_primo_livello != null ? " il " + row.data_validazione_primo_livello.Value.ToString("dd/MM/yyyy") : "");

            }
            return nota;
        }
        public static string IsInseribileSW(string matricola, DateTime Data)
        {
            var db = new digiGappEntities();
            var pian = db.MyRai_Pianificazione.Where(x => x.Data == Data && x.Eccezione == "SW" && x.Matricola == matricola).FirstOrDefault();
            if (pian != null)
            {
                string esitoCheckStorno = IsStornato(pian);
                if (esitoCheckStorno != null)
                {
                    return esitoCheckStorno;
                }
            }
            DateTime D1 = new DateTime(Data.Year, Data.Month, 1);
            DateTime D2 = D1.AddMonths(1).AddDays(-1);
            List<PeriodoRapporto> Periodi = new List<PeriodoRapporto>();

            //Get SW info
            bool esito = Utente.IsSmartWorker(matricola, D1, D2, out Periodi);

            if (!esito) return "Utente non risulta smartworker";
            if (!Periodi.Any()) return "Non risultano periodi di smartworking";

            var periodo = Periodi.Where(x => x.Inizio <= Data && x.Fine >= Data).FirstOrDefault();
            if (periodo == null || !periodo.Info.Any()) return "Periodo non trovato";
             
            var per = periodo.Info.Where(x => x.Inizio <= Data && x.Fine>=Data).FirstOrDefault();
            if (per == null) return "Periodo non trovato";


            var dbCzn = new IncentiviEntities();
            var RichExt = dbCzn.XR_MAT_RICHIESTE.Where(x => x.MATRICOLA == matricola && x.ECCEZIONE == "SW" &&
                x.INIZIO_GIUSTIFICATIVO <= Data && x.FINE_GIUSTIFICATIVO >= Data).FirstOrDefault();

            if (RichExt != null && RichExt.XR_WKF_OPERSTATI.Any() && RichExt.XR_WKF_OPERSTATI.Max(x => x.ID_STATO) >= 20)
            {
                return null; //se c'e una richiesta di estensione che contiene questa data ed è approvata,  è ok
            }



            if (per.NumeroGiorniPerMese == null) return "Numero giorni per mese non trovato";

            int extra = 0;
            if (per.GiorniExtra != null) extra = (int)per.GiorniExtra;

            int GiorniMese = (int)per.NumeroGiorniPerMese + extra;

            if (GiorniMese==0) return "Numero giorni per mese pari a 0";

           

            //Get SW GAPP
            int EccezioniGapp = QuanteEccezioniMese("SW", matricola, D1, D2);


            if (EccezioniGapp >= GiorniMese)
                return "Limite giorni mensili gia raggiunto";
            else
                return null; //finalmente


        }

     

        public static NetworkCredential GetUtenteServizioCredentials()
        {
            string userName = "";
            string password = "";

            if (SessionHelper.Get("AccountUtenteServizioUsername") == null)
            {
                string[] parametri = GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
                userName = parametri[0];
                password = parametri[1];
                SessionHelper.Set("AccountUtenteServizioUsername", userName);
                SessionHelper.Set("AccountUtenteServizioPassword", password);
            }
            else
            {
                userName = (string)SessionHelper.Get("AccountUtenteServizioUsername");
                password = (string)SessionHelper.Get("AccountUtenteServiziopassword");

            }

            return new NetworkCredential(userName, password, "RAI");
        }

        public static Dictionary<string, string> GetDictionaryFromJson(string json)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            dynamic data = Json.Decode(json);
            for (int i = 0; i < data.Length; i++)
            {
                dict.Add(data[i].key, data[i].value);
            }

            return dict;
        }


        #region DateUtility

        public static int calcolaMinuti(DateTime D)
        {
            int minuti = (D.Hour * 60) + D.Minute;
            return minuti;
        }

        public static int calcolaMinuti(string orarioHHMM)
        {
            int minuti = 0;
            if (orarioHHMM == null || orarioHHMM.Trim() == "" || orarioHHMM.Trim().Length < 4) return minuti;
            if (orarioHHMM.Contains("<") || orarioHHMM.Contains(">")) return minuti;

            string[] array = new string[2];

            if (orarioHHMM.IndexOf(':') > 0)
                array = orarioHHMM.Split(':');
            else
            {
                array[0] = orarioHHMM.Substring(0, 2);
                array[1] = orarioHHMM.Substring(2, 2);
            }

            minuti = Int32.Parse(array[0]) * 60 + Int32.Parse(array[1]);

            return minuti;
        }
        public static string CalcolaStringaOreMinuti(int minuti)
        {
            int h = (int)minuti / 60;
            int min = minuti - (h * 60);
            return h.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0');
        }

        public static string CalcolaQuantitaOreMinuti(string dallemin, string allemin)
        {
            int dalle = Convert.ToInt32(dallemin);
            int alle = Convert.ToInt32(allemin);
            int diff = 0;

            if (alle >= dalle)
                diff = alle - dalle;
            else
                diff = (1440 - dalle) + alle;

            int h = (int)diff / 60;
            int min = diff - (h * 60);
            return h.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0');
        }
        #endregion

        #region SedeUtility

        public static int GetMinutiCarenzaPerSede(string sede, DateTime DataRichiesta)
        {
            var db = new digiGappEntities();
            var se = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sede &&
                x.data_inizio_validita <= DataRichiesta && x.data_fine_validita >= DataRichiesta)
                .FirstOrDefault();

            if (se == null || se.minimo_car == null)
                return 0;
            else
            {
                int car = 0;
                int.TryParse(se.minimo_car, out car);
                return car;
            }
        }
        #endregion

        #region HomeHelper
        public static List<MyRai_Raggruppamenti> GetRaggruppamenti()
        {
            var db = new digiGappEntities();
            return db.MyRai_Raggruppamenti.OrderBy(x => x.IdRaggruppamento).ToList();
        }
        #endregion

        public static List<Sede> GetSediGappLivello6()
        {
            if (SessionHelper.Get("SedeLivello6") != null)
            {
                return (List<Sede>)SessionHelper.Get("SedeLivello6");
            }
            else
            {
                List<Sede> listnew = new List<Sede>();

                Abilitazioni AB = getAbilitazioni();
                foreach (var l in AB.ListaAbilitazioni.Where(x => x.MatrLivello6.Any(z => z.Matricola ==
                     GetCurrentUserPMatricola())))
                {
                    if (l.Sede.Length == 5)
                    {
                        Sede ss = new Sede()
                        {
                            CodiceSede = l.Sede,
                            DescrizioneSede = l.DescrSede,
                            RepartiSpecifici = new List<RepartoLinkedServer>() {
                                new RepartoLinkedServer(){ reparto="*" }}
                        };
                        listnew.Add(ss);
                    }
                    if (l.Sede.Length > 5)
                    {
                        string sede = l.Sede.Substring(0, 5);
                        string rep = l.Sede.Substring(5);
                        l.DescrSede = new myRaiData.digiGappEntities().L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sede)
                            .Select(x => x.desc_sede_gapp).FirstOrDefault();
                        var reparto = new LinkedTableDataController().GetDettagliReparto(sede, rep, GetCurrentUserMatricola());
                    }

                }


                List<Sede> list = new List<Sede>();

                Sedi service = new Sedi();
                CategorieDatoAbilitate response =
                    service.Get_CategoriaDato_Net("sedegapp", GetCurrentUserPMatricola(), "HRUP", "06GEST");

                if (response.DT_CategorieDatoAbilitate != null)
                {
                    foreach (DataRow item in response.DT_CategorieDatoAbilitate.Rows)
                    {
                        if (item != null && item["cod"] != null && item["cod"].ToString() != "")
                        {
                            string nomesede = item["cod"].ToString();
                            if (nomesede.Length > 5) continue;

                            var sedeCorrente = new Sede()
                            {
                                CodiceSede = nomesede,
                                DescrizioneSede = item["descrizione_categoria_dato"].ToString()
                            };
                            sedeCorrente.RepartiSpecifici = new List<RepartoLinkedServer>() {
                            new RepartoLinkedServer(){ reparto="*" }};

                            list.Add(sedeCorrente);
                        }
                    }

                    foreach (DataRow item in response.DT_CategorieDatoAbilitate.Rows)
                    {
                        if (item != null && item["cod"] != null && item["cod"].ToString() != "")
                        {
                            string nomesede = item["cod"].ToString();
                            if (nomesede.Length > 5)
                            {
                                string sede = nomesede.Substring(0, 5);
                                string rep = nomesede.Substring(5);
                                var reparto = new LinkedTableDataController().GetDettagliReparto(sede, rep, GetCurrentUserMatricola());
                                if (reparto == null)
                                {
                                    reparto = new RepartoLinkedServer() { reparto = rep, Descr_Reparto = "Descrizione non trovata" };
                                }
                                var sedeInLista = list.Where(x => x.CodiceSede == sede).FirstOrDefault();
                                if (sedeInLista != null)
                                {
                                    sedeInLista.RepartiSpecifici.Add(reparto);
                                }
                                else
                                {
                                    var sedeNuova = new Sede()
                                    {
                                        CodiceSede = sede,
                                        DescrizioneSede =
                                        new myRaiData.digiGappEntities().L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sede)
                                        .Select(x => x.desc_sede_gapp).FirstOrDefault(),
                                        //item["descrizione_categoria_dato"].ToString(),
                                        RepartiSpecifici = new List<RepartoLinkedServer>() { reparto }
                                    };
                                    list.Add(sedeNuova);
                                }
                            }
                        }
                    }
                }


                HttpContext.Current.Session["SedeLivello6"] = list;

                return (List<Sede>)HttpContext.Current.Session["SedeLivello6"];
            }
        }

        public static List<Sede> GetSediGappResponsabileList()
        {
            if (SessionHelper.Get("SedeLivello1") != null)
            {
                return (List<Sede>)SessionHelper.Get("SedeLivello1");
            }
            else
            {
                List<Sede> listnew = new List<Sede>();

                Abilitazioni AB = getAbilitazioni();
                foreach (var l in AB.ListaAbilitazioni.Where(x => x.MatrLivello1.Any(z => z.Matricola ==
                     CommonManager.GetCurrentUserPMatricola())))
                {
                    if (l.Sede.Length == 5)
                    {
                        Sede ss = new Sede()
                        {
                            CodiceSede = l.Sede,
                            DescrizioneSede = l.DescrSede,
                            RepartiSpecifici = new List<RepartoLinkedServer>() {
                                new RepartoLinkedServer(){ reparto="*" }}
                        };
                        listnew.Add(ss);
                    }
                    if (l.Sede.Length > 5)
                    {
                        string sede = l.Sede.Substring(0, 5);
                        string rep = l.Sede.Substring(5);
                        l.DescrSede = new myRaiData.digiGappEntities().L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sede)
                            .Select(x => x.desc_sede_gapp).FirstOrDefault();
                        var reparto = new LinkedTableDataController().GetDettagliReparto(sede, rep, CommonManager.GetCurrentUserMatricola());
                    }

                }


                List<Sede> list = new List<Sede>();

                Sedi service = new Sedi();
                CategorieDatoAbilitate response =
                    service.Get_CategoriaDato_Net("sedegapp", CommonManager.GetCurrentUserPMatricola(), "HRUP", "01GEST");

                if (response.DT_CategorieDatoAbilitate != null)
                {
                    foreach (DataRow item in response.DT_CategorieDatoAbilitate.Rows)
                    {
                        if (item != null && item["cod"] != null && item["cod"].ToString() != "")
                        {
                            string nomesede = item["cod"].ToString();
                            if (nomesede.Length > 5) continue;

                            var sedeCorrente = new Sede()
                            {
                                CodiceSede = nomesede,
                                DescrizioneSede = item["descrizione_categoria_dato"].ToString()
                            };
                            sedeCorrente.RepartiSpecifici = new List<RepartoLinkedServer>() {
                            new RepartoLinkedServer(){ reparto="*" }};

                            list.Add(sedeCorrente);
                        }
                    }

                    foreach (DataRow item in response.DT_CategorieDatoAbilitate.Rows)
                    {
                        if (item != null && item["cod"] != null && item["cod"].ToString() != "")
                        {
                            string nomesede = item["cod"].ToString();
                            if (nomesede.Length > 5)
                            {
                                string sede = nomesede.Substring(0, 5);
                                string rep = nomesede.Substring(5);
                                var reparto = new LinkedTableDataController().GetDettagliReparto(sede, rep, CommonManager.GetCurrentUserMatricola());
                                if (reparto == null)
                                {
                                    reparto = new RepartoLinkedServer() { reparto = rep, Descr_Reparto = "Descrizione non trovata" };
                                }
                                var sedeInLista = list.Where(x => x.CodiceSede == sede).FirstOrDefault();
                                if (sedeInLista != null)
                                {
                                    sedeInLista.RepartiSpecifici.Add(reparto);
                                }
                                else
                                {
                                    var sedeNuova = new Sede()
                                    {
                                        CodiceSede = sede,
                                        DescrizioneSede =
                                        new myRaiData.digiGappEntities().L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sede)
                                        .Select(x => x.desc_sede_gapp).FirstOrDefault(),
                                        //item["descrizione_categoria_dato"].ToString(),
                                        RepartiSpecifici = new List<RepartoLinkedServer>() { reparto }
                                    };
                                    list.Add(sedeNuova);
                                }
                            }
                        }
                    }
                }


                SessionHelper.Set("SedeLivello1", list);

                return (List<Sede>)SessionHelper.Get("SedeLivello1");
            }
        }

        public static List<Sede> GetSediGappResponsabileLiv2List()
        {
            List<Sede> list = new List<Sede>();

            Sedi service = new Sedi();
            CategorieDatoAbilitate response =
                service.Get_CategoriaDato_Net("sedegapp", CommonManager.GetCurrentUserPMatricola(), "HRUP", "02GEST");

            if (response.DT_CategorieDatoAbilitate != null && response.DT_CategorieDatoAbilitate.Rows != null)
                foreach (DataRow item in response.DT_CategorieDatoAbilitate.Rows)
                {
                    if (item != null && item["cod"] != null && item["cod"].ToString() != "")
                    {
                        list.Add(new Sede() { CodiceSede = item["cod"].ToString(), DescrizioneSede = item["descrizione_categoria_dato"].ToString() });
                    }
                }

            return list;
        }
        public static int QuanteEccezioniMese(string codiceEccezione, string matricola, DateTime D1, DateTime D2)
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            wcf1.ClientCredentials.Windows.ClientCredential = CommonManager.GetUtenteServizioCredentials();

            MyRaiServiceInterface.MyRaiServiceReference1.GetAnalisiEccezioniResponse response =
                                wcf1.GetAnalisiEccezioni(matricola,
                                                            D1,
                                                            D2,
                                                            codiceEccezione,
                                                            null,
                                                            null
                                                            );
            if (response == null || response.DettagliEccezioni == null)
                return 0;
            else
                return response.DettagliEccezioni.Count();
        }
        public static int QuanteEccezioniAnno(string codiceEccezione, string matricola)
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            wcf1.ClientCredentials.Windows.ClientCredential = CommonManager.GetUtenteServizioCredentials();

            MyRaiServiceInterface.MyRaiServiceReference1.GetAnalisiEccezioniResponse response =
                                wcf1.GetAnalisiEccezioni(matricola,
                                                            new DateTime(DateTime.Now.Year, 1, 1),
                                                            DateTime.Now,
                                                            codiceEccezione,
                                                            null,
                                                            null
                                                            );
            if (response == null || response.DettagliEccezioni == null)
                return 0;
            else
                return response.DettagliEccezioni.Count();
        }

        public static DayOfWeek GetGiornoInizialePerSede(string sede)
        {
            var db = new digiGappEntities();
            var sedegapp = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sede).FirstOrDefault();
            if (sedegapp == null || sedegapp.giorno_inizio_settimana == null || sedegapp.giorno_inizio_settimana == 1)
                return DayOfWeek.Monday;
            else
                return DayOfWeek.Sunday;
        }

        public static bool IsSemiLavorativo(DateTime date)
        {
            DateTime DataVenerdiSanto = FestivitaManager.GetPasqua(date.Year).AddDays(-2);
            return date == DataVenerdiSanto || (date.Month == 11 && date.Day == 2) || (date.Month == 12 && date.Day == 24) || (date.Month == 12 && date.Day == 31);
        }

        public static bool IsDateInControlloSNM_SMP(DateTime date)
        {
            DateTime DataVenerdiSanto = FestivitaManager.GetPasqua(date.Year).AddDays(-2);

            string dateParam = CommonManager.GetParametro<string>(EnumParametriSistema.DateConControlloSNM_SNP);// + "," + DataVenerdiSanto.ToString("dd/MM");

            foreach (string d in dateParam.Split(','))
            {
                if (String.IsNullOrWhiteSpace(d)) continue;
                DateTime dParam;
                bool dconv = DateTime.TryParseExact(d + "/" + date.Year.ToString(), "dd/MM/yyyy", null, DateTimeStyles.None, out dParam);
                if (dconv && date == dParam) return true;
            }
            return false;

        }

        public static bool SNM_SNPpresent(dayResponse resp)
        {
            return resp.eccezioni.Any(x => x.cod.Trim() == "SNM" || x.cod.Trim() == "SNP");
        }

        public static int CarenzaMin(dayResponse resp)
        {
            if (resp == null || resp.eccezioni == null) return 0;
            var c = resp.eccezioni.Where(x => x.cod == "CAR").FirstOrDefault();
            if (c == null) return 0;
            else return c.qta.ToMinutes();
        }

        public static int MaggiorPresenzaMin(dayResponse resp)
        {
            if (resp == null || resp.giornata == null || resp.giornata.maggiorPresenza == null)
                return 0;
            else
                return resp.giornata.maggiorPresenza.ToMinutes();
        }
        public static int PrevistaPresenzaMin(dayResponse resp)
        {
            if (resp == null || resp.orario == null || resp.orario.prevista_presenza == null)
                return 0;
            else
                return resp.orario.prevista_presenza.ToMinutes();
        }

        public static bool EsisteLNH5maggiorata(dayResponse resp)
        {
            try
            {
                if (resp == null || resp.eccezioni == null) return false;

                var lnh5 = resp.eccezioni.Where(x => x.cod == "LNH5").FirstOrDefault();
                if (lnh5 == null) return false;
                int minConMaggiorazione = PrevistaPresenzaMin(resp) + MaggiorPresenzaMin(resp) - CarenzaMin(resp);
                if (minConMaggiorazione > 600) minConMaggiorazione = 600;

                return MaggiorPresenzaMin(resp) > 0 && lnh5.qta.ToMinutes() == minConMaggiorazione;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "EsisteLNH5maggiorata"
                });
                return false;
            }
        }

        public static DateTime[] GetIntervalloSettimanaleSedePerGiorno(DateTime D)
        {
            DayOfWeek giornoIniziale = GetGiornoInizialePerSede(Utente.SedeGapp(D));

            DateTime datainizio = D;
            while (datainizio.DayOfWeek != giornoIniziale) datainizio = datainizio.AddDays(-1);
            DateTime datafine = datainizio.AddDays(6);
            return new DateTime[] { datainizio, datafine };
        }

        public static DateTime[] GetIntervalloSettimanaleSede()
        {
            DayOfWeek giornoIniziale = GetGiornoInizialePerSede(Utente.SedeGapp(DateTime.Now));

            DateTime datainizio = DateTime.Now;
            while (datainizio.DayOfWeek != giornoIniziale) datainizio = datainizio.AddDays(-1);
            DateTime datafine = datainizio.AddDays(6);
            return new DateTime[] { datainizio, datafine };
        }

        public static DateTime[] GetIntervalloPerResocontiLiv1(string sede)
        {
            int day = (int)DateTime.Today.DayOfWeek;
            DateTime dateStartWeek = DateTime.Today.AddDays(-7).AddDays(-day + 1);

            if (GetGiornoInizialePerSede(sede) == DayOfWeek.Sunday)
                dateStartWeek = dateStartWeek.AddDays(-1);

            DateTime dateEndWeek = dateStartWeek.AddDays(6);
            return new DateTime[] { dateStartWeek, dateEndWeek };
        }

        public static string[] GetPeriodoMieRichieste(MyRai_Richieste r)
        {
            string[] colonne = new string[2] { "", "" };

            if (r.MyRai_Eccezioni_Richieste.Count == 0) return colonne;

            //se la richiesta ha giorni diversi per periodo_dal / periodo_al
            if (r.periodo_dal != r.periodo_al)
            {
                colonne[0] = "Dal " + r.periodo_dal.ToString("dd/MM/yyyy") + " al " + r.periodo_al.ToString("dd/MM/yyyy");
            }
            else //se ha giorni uguali
            {
                MyRai_Eccezioni_Richieste ecc = r.MyRai_Eccezioni_Richieste.First();
                //se l'eccezione figlia ha dalle/alle null
                if (ecc.dalle == null && ecc.alle == null)
                {
                    colonne[0] = r.periodo_dal.ToString("d MMMM yyyy");
                }
                else if (ecc.dalle != null && ecc.alle == null)
                {
                    DateTime D1 = (DateTime)ecc.dalle;

                    colonne[0] = D1.ToString("d MMMM yyyy");
                    colonne[1] = "Dalle " + D1.ToString("HH.mm");

                }
                else //se l'eccezione figlia ha dalle/alle valorizzate
                {
                    DateTime D1 = (DateTime)ecc.dalle;
                    DateTime D2 = (DateTime)ecc.alle;
                    if (D1.Date == D2.Date) // se dalle/alle sono nella stessa giornata
                    {

                        colonne[0] = D1.ToString("d MMMM yyyy");
                        colonne[1] = "Dalle " + D1.ToString("HH.mm") + " alle " + D2.ToString("HH.mm");
                    }
                    else //se dalle/alle sono in giorni diversi
                    {

                        colonne[0] = D1.ToString("d MMMM yyyy");
                        colonne[1] = " Dalle " + D1.ToString("HH.mm") +
                                     " alle " + D2.ToString("HH.mm") + " del " + D2.ToString("dd/MM/yyyy");

                    }
                }
            }
            return colonne;

        }

        public static B2RaiPlace_RaiPlacePolicy GetPolicyEventi()
        {
            using (digiGappEntities db = new digiGappEntities())
            {

                if (Utente.EsponiAnagrafica()._dataAssunzione != null)
                {
                    if (Utente.EsponiAnagrafica()._dataAssunzione > Convert.ToDateTime(GetParametri<string>(EnumParametriSistema.MaxDatePolicy)[0]))
                    {
                        return new B2RaiPlace_RaiPlacePolicy();
                    }
                }
                string matricola = myRai.Models.Utente.EsponiAnagrafica()._matricola;
                string societa = "EVENTI";

                B2RaiPlace_RaiPlacePolicy policy = db.B2RaiPlace_RaiPlacePolicy.Where(a => a.Societa == societa).OrderByDescending(a => a.Versione).FirstOrDefault();
                if (policy == null)
                {
                    return new B2RaiPlace_RaiPlacePolicy();
                }

                B2RaiPlace_RaiPlacePolicyUtenti utente = db.B2RaiPlace_RaiPlacePolicyUtenti.Where(a => a.Matrciola == matricola && a.Fk_Id_RaiPlacePolicy == policy.Id_RaiPlacePolicy).FirstOrDefault();

                if (utente != null)
                {
                    return new B2RaiPlace_RaiPlacePolicy();
                }
                else
                {
                    policy.Testo = policy.Testo.Replace("\r\n", "");
                    return policy;
                }
            }
        }

        public static B2RaiPlace_RaiPlacePolicy GetPolicy()
        {
            if (Utente.EsponiAnagrafica()._vediPolicy == false)
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    if (Utente.EsponiAnagrafica()._dataAssunzione != null)
                    {
                        if (Utente.EsponiAnagrafica()._dataAssunzione > Convert.ToDateTime(CommonManager.GetParametri<string>(EnumParametriSistema.MaxDatePolicy)[0]))
                        {
                            Utente.EsponiAnagrafica()._vediPolicy = true;
                            return new B2RaiPlace_RaiPlacePolicy();

                        }
                    }
                    string matricola = Utente.EsponiAnagrafica()._matricola;
                    string societa = Utente.EsponiAnagrafica()._logo;


                    B2RaiPlace_RaiPlacePolicy policy = db.B2RaiPlace_RaiPlacePolicy.Where(a => a.Societa == societa).OrderByDescending(a => a.Versione).FirstOrDefault();
                    if (policy == null)
                    {
                        Utente.EsponiAnagrafica()._vediPolicy = true;
                        return new B2RaiPlace_RaiPlacePolicy();
                    }

                    B2RaiPlace_RaiPlacePolicyUtenti utente = db.B2RaiPlace_RaiPlacePolicyUtenti.Where(a => a.Matrciola == matricola && a.Fk_Id_RaiPlacePolicy == policy.Id_RaiPlacePolicy).FirstOrDefault();


                    if (utente != null)
                    {
                        Utente.EsponiAnagrafica()._vediPolicy = true;
                        return new B2RaiPlace_RaiPlacePolicy();
                    }
                    else
                    {
                        policy.Testo = policy.Testo.Replace("\r\n", "");
                        return policy;
                    }

                }
            }
            else
            {
                return new B2RaiPlace_RaiPlacePolicy();
            }

        }

        public static string GetPeriodo(MyRai_Richieste r, EnumFormatoPeriodo format)
        {
            String Periodo = "";
            if (r.MyRai_Eccezioni_Richieste.Count == 0) return "";

            //se la richiesta ha giorni diversi per periodo_dal / periodo_al
            if (r.periodo_dal != r.periodo_al)
            {
                switch (format)
                {
                    case EnumFormatoPeriodo.MieRichieste:
                        Periodo = "Dal " + r.periodo_dal.ToString("dd/MM/yyyy") + " al " + r.periodo_al.ToString("dd/MM/yyyy");
                        break;
                    case EnumFormatoPeriodo.DaApprovare2:
                        Periodo = "dal " +
                            " " + r.periodo_dal.ToString("dd/MM/yyyy") + Environment.NewLine + " a " +
                            " "
                            + r.periodo_al.ToString("dd/MM/yyyy");
                        break;
                }

            }
            else //se ha giorni uguali
            {
                MyRai_Eccezioni_Richieste ecc = r.MyRai_Eccezioni_Richieste.First();
                //se l'eccezione figlia ha dalle/alle null
                if (ecc.dalle == null && ecc.alle == null)
                {
                    switch (format)
                    {
                        case EnumFormatoPeriodo.MieRichieste:
                            Periodo = r.periodo_dal.ToString("dd/MM/yyyy");
                            break;

                        case EnumFormatoPeriodo.DaApprovare2:
                            Periodo = r.periodo_dal.ToString("dd/MM/yyyy");
                            break;
                    }
                }
                else if (ecc.dalle != null && ecc.alle == null)
                {
                    DateTime D1 = (DateTime)ecc.dalle;
                    switch (format)
                    {
                        case EnumFormatoPeriodo.MieRichieste:
                            Periodo = D1.ToString("dd/MM/yyyy") + " dalle " + D1.ToString("HH.mm");
                            break;

                        case EnumFormatoPeriodo.DaApprovare2:
                            Periodo = r.periodo_dal.ToString("dd/MM/yyyy");
                            break;
                    }
                }
                else if (ecc.dalle == null && ecc.alle != null)
                {
                    DateTime D2 = (DateTime)ecc.alle;
                    switch (format)
                    {
                        case EnumFormatoPeriodo.MieRichieste:
                            Periodo = r.periodo_al.ToString("dd/MM/yyyy");
                            break;

                        case EnumFormatoPeriodo.DaApprovare2:
                            Periodo = r.periodo_al.ToString("dd/MM/yyyy");
                            break;
                    }
                }
                else
                //se l'eccezione figlia ha dalle/alle valorizzate
                {
                    DateTime D1 = (DateTime)ecc.dalle;
                    DateTime D2 = (DateTime)ecc.alle;
                    if (D1.Date == D2.Date) // se dalle/alle sono nella stessa giornata
                    {
                        switch (format)
                        {
                            case EnumFormatoPeriodo.MieRichieste:
                                Periodo = D1.ToString("dd/MM/yyyy") + " dalle " + D1.ToString("HH.mm") + " alle " +
                                    D2.ToString("HH.mm");
                                break;
                            case EnumFormatoPeriodo.DaApprovare2:

                                Periodo = r.periodo_dal.ToString("dd/MM/yyyy");
                                break;
                        }
                    }
                    else //se dalle/alle sono in giorni diversi
                    {
                        switch (format)
                        {
                            case EnumFormatoPeriodo.MieRichieste:
                                Periodo = " Dalle " + D1.ToString("HH.mm") + " del " + D1.ToString("dd/MM/yyyy") +
                                    " alle " + D2.ToString("HH.mm") + " del " + D2.ToString("dd/MM/yyyy");
                                break;
                            case EnumFormatoPeriodo.DaApprovare2:
                                Periodo = "Da " + new CultureInfo("it-IT").DateTimeFormat.GetDayName(r.periodo_dal.DayOfWeek) +
                                    " " + r.periodo_dal.ToString("dd/MM/yyyy");
                                break;
                        }
                    }
                }

            }
            return Periodo;

        }

        public static OrarioUscitaModel GetOrarioDiUscita(string OrarioIngresso, string codiceOrario,
            string data, dayResponse resp)
        {
            OrarioUscitaModel model = new OrarioUscitaModel() { OrarioDiIngresso = OrarioIngresso };

            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = GetUtenteServizioCredentials();
            MyRaiServiceInterface.MyRaiServiceReference1.getOrarioResponse response =
                cl.getOrario(codiceOrario.PadRight(2), data, GetCurrentUserMatricola(), "BU", 75);


            int MinutiUltimaTimbratura = 0;
            var Timb = resp.timbrature.LastOrDefault();
            if (Timb != null && Timb.uscita != null && !String.IsNullOrWhiteSpace(Timb.uscita.orario))
            {
                MinutiUltimaTimbratura = calcolaMinuti(Timb.uscita.orario);
            }
            int MinutiIngresso = calcolaMinuti(OrarioIngresso);
            if (response.OrarioUscitaIniziale != response.OrarioUscitaFinale)
            {
                int MaxMinutiIngresso = Convert.ToInt32(response.OrarioEntrataFinaleMin);
                if (MinutiIngresso > MaxMinutiIngresso)
                {
                    model.IngressoRed = true;
                    model.OrarioDiUscita = response.OrarioUscitaIniziale;
                }
                else
                {
                    int diff = MaxMinutiIngresso - MinutiIngresso;
                    int MinutiUscita = calcolaMinuti(response.OrarioUscitaFinale) - diff;
                    if (MinutiUscita < Convert.ToInt32(response.OrarioUscitaInizialeMin))
                    {
                        MinutiUscita = Convert.ToInt32(response.OrarioUscitaInizialeMin);
                    }
                    model.OrarioDiUscita = CalcolaStringaOreMinuti(MinutiUscita);
                }
            }
            else
            {
                int MinutiMaxTollerati = Convert.ToInt32(response.OrarioFineTolleranzaMin);
                model.IngressoRed = MinutiIngresso > MinutiMaxTollerati;
                model.OrarioDiUscita = response.OrarioUscitaIniziale;

            }
            if (MinutiIngresso < Convert.ToInt32(response.OrarioEntrataInizialeMin))
                model.OrarioDiIngresso = response.OrarioEntrataIniziale;

            model.UscitaRed = MinutiUltimaTimbratura > 0 && MinutiUltimaTimbratura < calcolaMinuti(model.OrarioDiUscita);

            CheckIfEccezioniForIngressoRosso(model, resp);

            return model;
        }

        public static void CheckIfEccezioniForIngressoRosso(OrarioUscitaModel model, dayResponse resp)
        {
            if (model.IngressoRed && resp != null && resp.giornata != null && resp.eccezioni != null && resp.eccezioni.Count() > 0)
            {
                string eccez = CommonManager.GetParametro<string>(EnumParametriSistema.EccezioniNoOrarioRosso);
                if (!String.IsNullOrWhiteSpace(eccez))
                {
                    var E = eccez.Split(',').Where(x => resp.eccezioni.Where(aa => aa.cod != null).Select(a => a.cod.Trim()).Contains(x)).FirstOrDefault();
                    if (E != null)
                    {
                        model.IngressoRed = false;
                        model.DicituraSottoOrario = "Sei in " + E;
                    }
                }
            }
        }

        public static string GetTema(string matricola)
        {
            var db = new digiGappEntities();
            MyRai_ParametriPersonali exist = db.MyRai_ParametriPersonali.Where(a => a.matricola == matricola && a.nome_parametro == "Tema").FirstOrDefault();
            if (exist == null)
            {
                return "";
            }
            else
            {
                return exist.valore_parametro;

            }
        }

        public static MyRai_AttivitaCeiton GetAttivitaCeiton(int idRich)
        {
            return CeitonHelper.GetAttivitaCeiton(idRich);
        }

        public static MyRai_Richieste GetEccezionePadre(long id)
        {
            var db = new digiGappEntities();
            return db.MyRai_Richieste.Where(x => x.id_richiesta == id).FirstOrDefault();
        }

        public static string GetCKfoto(string matricola)
        {
            string s = DateTime.Now.ToString("ddMMyyyy") + matricola;
            int tot = 0;
            foreach (var c in s)
            {
                tot += int.Parse(c.ToString());
            }
            return tot.ToString().PadLeft(3, '0');
        }

        public static string GetDigit(string matricola)
        {
            string s = DateTime.Now.ToString("ddMMyyyy") + matricola;
            int tot = 0;
            foreach (var c in s)
            {
                if (int.TryParse(c.ToString(), out int num)) tot += num;
            }
            return tot.ToString().PadLeft(3, '0');
        }
        public static string GetUrlFotoExternal(string matricola)
        {
            return GetUrlFoto(matricola);

            string url = GetParametro<string>(EnumParametriSistema.UrlImmagineDipendente);

            string s = DateTime.Now.ToString("ddMMyyyy") + matricola;
            int tot = 0;
            foreach (var c in s)
            {
                tot += int.Parse(c.ToString());
            }
            url = url.Replace("#MATR", matricola).Replace("#CK", tot.ToString().PadLeft(3, '0'));
            return url;
        }

        public static string GetUrlFotoFromDB()
        {
            if (HttpContext.Current.Session["urlapifoto"] == null)
                 HttpContext.Current.Session["urlapifoto"] = CommonHelper.GetParametro<string>(EnumParametriSistema.UrlFotoApiControllerHRIS);

            return (string) HttpContext.Current.Session["urlapifoto"];
        }
        public static string GetUrlFoto(string matricola)
        {
            return GetUrlFotoFromDB().Replace("{matricola}", matricola).Replace("{check}", GetDigit(matricola));
            //return "/api/foto/getimage?matricola=" + matricola + "&risoluzione=3&check="+GetDigit(matricola);
            //return "/home/getimg?matr=" + matricola;
        }

        public static string GetImmagineBase64ForApp(string matricola)
        {
            string base64 = null;
            string url = GetParametro<string>(EnumParametriSistema.UrlImmagineDipendente);
            int tot = 0;
            string s = DateTime.Now.ToString("ddMMyyyy") + matricola;

            foreach (var c in s)
            {
                int a;
                if (int.TryParse(c.ToString(), out a))
                    tot += a;
            }
            url = url.Replace("#MATR", matricola).Replace("#CK", tot.ToString().PadLeft(3, '0'));
            WebClient w = new WebClient();
            w.UseDefaultCredentials = true;
            w.Credentials = GetUtenteServizioCredentials();

            try
            {
                byte[] bytes = w.DownloadData(url);
                base64 = "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {

            }
            return base64;
        }

        public static string GetImmagineBase64(string matricola)
        {
            ImageInfo im = GetImageWithOverride(matricola);
            if (im != null)
                return "data:image/" + im.ext + ";base64," + Convert.ToBase64String(im.image);

            string base64 = string.Empty;
            //return string.Empty;


            string url = GetParametro<string>(EnumParametriSistema.UrlImmagineDipendente);

            string s = DateTime.Now.ToString("ddMMyyyy") + matricola;
            int tot = 0;

            // se l'utente non è autorizzato allora deve essere rimandato alla pagina "/Home/notAuth"
            string sMat = GetCurrentUserMatricola();
            int iMat = -1;
            s = s.Trim();
            if (!int.TryParse(sMat, out iMat))
            {
                base64 = string.Empty;
            }
            else
            {
                if (Debugger.IsAttached && CommonHelper.CheckForVPNInterface())// !System.Security.Principal.WindowsIdentity.GetCurrent().Name.StartsWith("RAI"))
                {
                    return string.Empty;
                }


                foreach (var c in s)
                {
                    tot += int.Parse(c.ToString());
                }
                url = url.Replace("#MATR", matricola).Replace("#CK", tot.ToString().PadLeft(3, '0'));
                WebClient w = new WebClient();
                w.UseDefaultCredentials = true;
                w.Credentials = CommonManager.GetUtenteServizioCredentials();

                try
                {
                    byte[] bytes = w.DownloadData(url);
                    base64 = "data:image/png;base64," + Convert.ToBase64String(bytes);
                }
                catch (Exception ex)
                {
                    base64 = string.Empty;
                }
            }



            return base64;

        }
        public static bool IsThisMaxInVPN()
        {
            return (CommonManager.GetCurrentRealUsername() == "USER");
        }
        public static ImageInfo GetImageWithOverride(string matr)
        {
            try
            {
                if (SessionHelper.Get("imagepath") == null)
                    SessionHelper.Set("imagepath", CommonManager.GetParametri<string>(EnumParametriSistema.PathImmaginiFittizie));

                string[] par = (String[])SessionHelper.Get("imagepath");

                if (par == null || String.IsNullOrWhiteSpace(par[0]) || String.IsNullOrWhiteSpace(par[1]))
                    return null;
                foreach (string m in par[1].ToUpper().Split(','))
                {
                    if (m.Split('|')[0] == matr)
                    {
                        string filepath = HttpContext.Current.Server.MapPath(par[0] + (m.Split('|')[1]));
                        if (System.IO.File.Exists(filepath))
                        {
                            return new ImageInfo()
                            {
                                image = System.IO.File.ReadAllBytes(filepath),
                                ext = System.IO.Path.GetExtension(filepath).Replace(".", "")
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "GetImageWithOverride"

                });
            }
            return null;
        }

        public static T GetParametro<T>(EnumParametriSistema chiave)
        {
            String NomeParametro = chiave.ToString();
            MyRai_ParametriSistema p = GetParametro(chiave);
            if (p == null) return default(T);
            else return (T)Convert.ChangeType(p.Valore1, typeof(T));
        }

        public static T[] GetParametri<T>(EnumParametriSistema chiave)
        {
            String NomeParametro = chiave.ToString();
            MyRai_ParametriSistema p = GetParametro(chiave);
            if (p == null) return null;
            else
            {
                T[] parametri = new T[] { (T)Convert.ChangeType(p.Valore1, typeof(T)), (T)Convert.ChangeType(p.Valore2, typeof(T)) };
                return parametri;
            }
        }

        public static List<ParametriSistemaValoreJson> GetParametriJson(EnumParametriSistema chiave)
        {
            List<ParametriSistemaValoreJson> result = new List<ParametriSistemaValoreJson>();
            using (digiGappEntities db = new digiGappEntities())
            {
                String NomeParametro = chiave.ToString();
                MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == NomeParametro);
                if (p == null)
                    return null;
                else
                {
                    result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ParametriSistemaValoreJson>>(p.Valore1);
                }
            }
            return result;
        }

        public static bool CanImpersonate()
        {
            string acc = GetParametro<string>(EnumParametriSistema.AccountBatchAbilitato);
            string RealName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            return (acc == RealName);
        }

        public static string IsBatchAccess()
        {
            if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Headers["matricola_simulata_accesso_batch"] != null && CanImpersonate())
                return HttpContext.Current.Request.Headers["matricola_simulata_accesso_batch"];
            else
                return null;
        }

        public static string GetCurrentUsername()
        {
            if (SessionHelper.Get("GetCurrentUsername") == null)
            {
                string RealName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                if (String.IsNullOrWhiteSpace(RealName))
                {
                    SessionHelper.Set("GetCurrentUsername", RealName);
                    return RealName;
                }

                string chiave = EnumParametriSistema.MatricolaSimulata.ToString();

                digiGappEntities db = new digiGappEntities();
                var list = db.MyRai_ParametriSistema
                    .Where(x => x.Chiave == chiave
                                && x.Valore1 != null && x.Valore1.Trim() != ""
                                && x.Valore2 != null && x.Valore2.Trim() != "");

                foreach (MyRai_ParametriSistema item in list)
                {
                    string[] MatricoleAbilitate = item.Valore2.ToUpper().Split(',');
                    foreach (string s in MatricoleAbilitate)
                    {
                        if (RealName.ToUpper().Contains(s.Trim()))
                        {
                            SessionHelper.Set("GetCurrentUsername", item.Valore1.ToUpper());
                            return item.Valore1.ToUpper();
                        }
                    }
                }

                SessionHelper.Set("GetCurrentUsername", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            }
            else
            {
                return (string)SessionHelper.Get("GetCurrentUsername");
            }
        }

        /// <summary>
        /// Reperimento dell'utente corrente reale (non quello simulato)
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentRealUsername()
        {
            string realName;
            try
            {
                realName = (string)SessionHelper.Get("GetCurrentRealUsername");
                if (realName == null)
                {
                    realName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                    if (realName == null || realName.Split('\\').Count() <= 1)
                    { throw new InvalidOperationException("Username non valido"); }

                    realName = realName.Split('\\')[1].ToUpper();

                    SessionHelper.Set("GetCurrentRealUsername", realName);
                }
            }
            catch (Exception)
            {
                realName = null;
            }
            return realName;
        }

        public static string GetCurrentUserMatricola()
        {
            string M = GetCurrentUsername();
            if (M == null || M.Length < 5) return null;

            if (M.ToUpper().StartsWith("RAI\\P"))
                return GetCurrentUsername().Substring(5);
            else if (M.ToUpper().StartsWith("WW930"))
                return GetCurrentUsername().Substring(5);
            else
            {
                string u = GetCurrentUsername().Substring(4);
                if (Char.IsDigit(u[0]))
                {
                    return "BP" + u;
                }
                else return u;
            }
        }

        public static string GetCurrentUserMatricola7chars()
        {
            string M = GetCurrentUsername();
            if (M != null && M.Length > 5)
            {
                if (M.ToUpper().StartsWith("RAI\\P"))
                    return GetCurrentUsername().Substring(5).PadLeft(7, '0');
                else if (M.ToUpper().StartsWith("WW930\\"))
                {
                    return GetCurrentUsername().Substring(5).PadLeft(7, '0');
                }
                else
                {
                    string u = M.Substring(4);
                    //Il controllo sul primo carattere numerico gestisce
                    //le matricole Rai Pubblicità RAI\xxxxxx
                    if (Char.IsDigit(u[0]))
                    {
                        return "BP" + M.Substring(4);
                    }
                    else
                    {
                        //in questo caso dovrebbero arrivare i casi NTSIPRA
                        //e quelle diverse da RAI\Pxxxxxx e RAI\xxxxxx
                        //return GetCurrentUsername().Substring(5).PadLeft(7, '0');
                        return u;
                    }
                }
            }
            else
                return null;
        }

        public static string GetCurrentUserPMatricola()
        {
            string M = GetCurrentUsername();
            if (M != null && M.Length > 4)
                return GetCurrentUsername().Substring(4).ToUpper();
            else
                return null;
        }

        public static int GetCurrentIdPersona()
        {
            if (SessionHelper.Get("GetCurrentIdPersona") == null)
            {
                string matricola = GetCurrentUserMatricola();
                IncentiviEntities db = new IncentiviEntities();
                var sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
                int idPersona = 0;
                if (sint != null)
                    idPersona = sint.ID_PERSONA;

                SessionHelper.Set("GetCurrentIdPersona", idPersona);
                return idPersona;
            }
            else
            {
                return (int)SessionHelper.Get("GetCurrentIdPersona");
            }
        }

        public static bool MensaFruitaData(DateTime Date)
        {
            DateTime Date1 = Date.AddDays(1);
            var db = new digiGappEntities();
            string padBadge = CommonManager.GetCurrentUserMatricola().PadLeft(8, '0');

            var scontrino = db.MyRai_MensaXML.Where(x => x.TransactionDateTime >= Date && x.TransactionDateTime < Date1 &&
               x.Badge == padBadge).FirstOrDefault();

            return scontrino != null;
        }
        public static MyRai_MensaXML MensaFruitaDataMatr(DateTime Date, string matr)
        {
            DateTime Date1 = Date.AddDays(1);
            var db = new digiGappEntities();
            string padBadge = matr.PadLeft(8, '0');

            var scontrino = db.MyRai_MensaXML.Where(x => x.TransactionDateTime >= Date && x.TransactionDateTime < Date1 &&
               x.Badge == padBadge).FirstOrDefault();

            return scontrino;
        }


        public static string GetNominativoPerMatricola(string matricola)
        {
            try
            {
                string r = ServiceWrapper.EsponiAnagraficaRaicvWrapped(matricola);
                return r.Split(';')[1] + " " + r.Split(';')[2];
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GetNominativoPerMatricolaCognomeNome(string matricola)
        {
            try
            {
                //it.rai.servizi.hrgb.Service s = new it.rai.servizi.hrgb.Service( );
                string r = ServiceWrapper.EsponiAnagraficaRaicvWrapped(matricola);
                return r.Split(';')[2] + " " + r.Split(';')[1];
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<string> GetCessate(string[] matricole)
        {
            myRaiServiceHub.it.rai.servizi.hrgb.Service s = new myRaiServiceHub.it.rai.servizi.hrgb.Service();
            string r = s.EsponiAnagrafica("fineserv;" + string.Join("|", matricole) + ";;E;0");
            List<string> L = new List<string>();

            if (String.IsNullOrWhiteSpace(r)) return L;

            foreach (string segm in r.Split('|'))
            {
                if (!String.IsNullOrWhiteSpace(segm) && segm.Split(';').Length > 1)
                {
                    string data = segm.Split(';')[1];
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        DateTime D;
                        if (DateTime.TryParseExact(data, "dd/MM/yyyy", null, DateTimeStyles.None, out D))
                        {
                            if (D < DateTime.Now) L.Add(segm.Split(';')[0]);
                        }
                    }
                }
            }
            return L;
        }
        public static string GetEmailPerMatricola(string matricola)
        {
            try
            {
                string r = ServiceWrapper.EsponiAnagraficaRaicvWrapped(matricola);
                return r.Split(';')[15];
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void InviaMailDebug(string testo, InserimentoEccezioneModel model)
        {
            try
            {
                MailSender invia = new MailSender();
                Email eml = new Email();
                eml.From = GetParametro<string>(EnumParametriSistema.MailApprovazioneFrom);
                string ads = CommonManager.GetParametro<string>(EnumParametriSistema.MailDebug);
                if (String.IsNullOrWhiteSpace(ads)) return;

                eml.toList = ads.Split(',');
                eml.ContentType = "text/html";
                eml.Priority = 2;
                eml.SendWhen = DateTime.Now.AddSeconds(1);
                string serialized = "";
                try
                {
                    serialized = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                }
                catch (Exception)
                {

                }

                eml.Subject = "Errore di inserimento";
                if (model != null)
                    eml.Body = serialized + " - " + CommonManager.GetCurrentUserMatricola() + " : " + testo;
                else
                    eml.Body = CommonManager.GetCurrentUserMatricola() + " : " + testo;

                eml.Body += (IsProduzione() ? " [P] " : " [S] ");
                if (!String.IsNullOrWhiteSpace(testo) && testo.Contains("A371"))
                {
                    try
                    {
                        var dayresp = EccezioniManager.GetGiornata(model.data_da, CommonManager.GetCurrentUserMatricola());
                        if (dayresp != null && dayresp.eccezioni != null)
                        {
                            eml.Body += " ECC:" + String.Join(",", dayresp.eccezioni.Select(x => x.cod).ToArray());
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                //string[] AccountUtenteServizio = GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
                //invia.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");
                invia.Credentials = CommonManager.GetUtenteServizioCredentials();

                invia.Send(eml);
            }
            catch (Exception)
            {
            }
        }
        public static string GetEccezioneRimpiazzo(int idRichiesta)
        {
            if (idRichiesta == 0) return null;

            var db = new myRaiData.digiGappEntities();
            var ric = db.MyRai_Richieste.Where(x => x.id_richiesta == idRichiesta)
                    .FirstOrDefault();
            var st = ric.MyRai_Eccezioni_Richieste.Where(x => x.azione == "C").FirstOrDefault();
            if (st != null)
                return st.eccezione_rimpiazzo_storno;
            else
                return null;
        }
        public static string InviaMailDelega(string matricolaDelegato, string dataInizio, string dataFine,
            Boolean isRevoca = false)
        {
            if (!CommonManager.IsProduzione())
            {
                Logger.LogAzione(new MyRai_LogAzioni()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    descrizione_operazione = "Mail soppressa su sistema sviluppo (destinatario " + matricolaDelegato + ")",
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "InviaMailDelega"
                });
                return null;
            }
            string matricolaDelegante = CommonManager.GetCurrentUserMatricola();
            MailSender invia = new MailSender();
            Email eml = new Email();
            eml.From = GetParametro<string>(EnumParametriSistema.MailApprovazioneFrom);

            string mailAddressDelegato = CommonManager.GetEmailPerMatricola(matricolaDelegato);
            string mailAddressDelegante = CommonManager.GetEmailPerMatricola(matricolaDelegante);
            string nomeDelegato = CommonManager.GetNominativoPerMatricola(matricolaDelegato);

            eml.toList = new string[] { mailAddressDelegato };
            eml.ccList = new string[] { mailAddressDelegante };

            eml.ContentType = "text/html";
            eml.Priority = 2;
            eml.SendWhen = DateTime.Now.AddSeconds(1);
            string[] par = null;
            if (!isRevoca)
                par = GetParametri<string>(EnumParametriSistema.MailTemplateAvvisoDelega);
            else
                par = GetParametri<string>(EnumParametriSistema.MailTemplateRevocaDelega);

            eml.Subject = par[1];
            eml.Body = par[0]
                           .Replace("#DELEGANTE", Utente.Nominativo().Trim())
                           .Replace("#DELEGATO", nomeDelegato)
                           .Replace("#DATAINIZIO", dataInizio)
                           .Replace("#DATAFINE", dataFine);


            invia.Credentials = CommonManager.GetUtenteServizioCredentials();

            try
            {
                invia.Send(eml);
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "CommonManager.InviaEmailDelega"
                });
                return ex.ToString();
            }
        }

        public static string InviaMailGenerica(string[] mailDest, string subj, string body)
        {
            MailSender invia = new MailSender();
            Email eml = new Email();
            eml.From = GetParametro<string>(EnumParametriSistema.MailApprovazioneFrom);
            eml.toList = mailDest;
            eml.ContentType = "text/html";
            eml.Priority = 2;
            eml.SendWhen = DateTime.Now.AddSeconds(1);

            eml.Subject = subj;
            eml.Body = body;


            invia.Credentials = CommonManager.GetUtenteServizioCredentials();

            try
            {
                invia.Send(eml);
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "CommonManager.InviaMailGenerica"
                });
                return ex.ToString();
            }
        }

        public static string InviaMail(string pMatricola, string dataEccezione, string DescrEccezione, bool approvata, string subject = null)
        {
            string StopMail = CommonManager.GetParametro<string>(EnumParametriSistema.StopMail);
            if (!String.IsNullOrWhiteSpace(StopMail) && StopMail.ToLower().Trim() == "stop")
            {
                return null;
            }

            MailSender invia = new MailSender();
            Email eml = new Email();
            //invia.Url = wsSendMAIL;
            eml.From = GetParametro<string>(EnumParametriSistema.MailApprovazioneFrom);
            // eml.
            eml.toList = new string[] { pMatricola + "@rai.it" };
            eml.ContentType = "text/html";
            eml.Priority = 2;
            eml.SendWhen = DateTime.Now.AddSeconds(1);

            eml.Subject = GetParametro<string>(EnumParametriSistema.MailApprovazioneSubject);

            if (!String.IsNullOrEmpty(subject))
            {
                string _tipo = approvata ? "Richiesta approvata" : "Richiesta rifiutata";

                eml.Subject = eml.Subject.Replace("Esito Richiesta", _tipo);

                eml.Subject = String.Format("{0} {1} ", eml.Subject, subject);
            }

            eml.Body = approvata ? GetParametro<string>(EnumParametriSistema.MailApprovazioneTemplate)
                                 : GetParametro<string>(EnumParametriSistema.MailRifiutaTemplate);

            eml.Body = eml.Body.Replace("#DESCR", DescrEccezione).Replace("#DATA", dataEccezione);
            string[] AccountUtenteServizio = GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);

            invia.Credentials = CommonManager.GetUtenteServizioCredentials();

            try
            {
                invia.Send(eml);
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = pMatricola,
                    provenienza = "CommonManager.InviaEmail"
                });
                return ex.ToString();
            }
        }

        public static bool IsMazzini()
        {
            return Utente.IsMazzini();
        }

        public static string[] RewriteAddress(string[] addressList, Email eml, string[] allowed)
        {
            if (addressList == null || addressList.Length == 0) return addressList;
            for (int i = 0; i < addressList.Length; i++)
            {
                if (String.IsNullOrWhiteSpace(addressList[i])) continue;
                if (allowed.Select(x => x.Trim()).Contains(addressList[i], StringComparer.InvariantCultureIgnoreCase)) continue;

                //sovrascrivi indirizzo e annota nella mail
                eml.Body = "<p>Reinstradato da sviluppo: " + addressList[i] + "</p>" + eml.Body;
                addressList[i] = "ruo.sip.presidioopen@rai.it";
            }
            return addressList;
        }

        public static string GetTipoEccezione(string cod)
        {
            var db = new digiGappEntities();
            var e = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione.Trim() == cod.Trim()).FirstOrDefault();
            if (e != null)
                return e.flag_eccez;
            else
                return null;
        }
        public static string GetDescrizioneEccezione(string cod)
        {
            var db = new digiGappEntities();
            var e = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione.Trim() == cod.Trim()).FirstOrDefault();
            if (e != null)
                return ToTitleCase(e.desc_eccezione);
            else
                return null;
        }

        public static string GetFiguraProfessionale(string matricola)
        {
            try
            {
                string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(CommonManager.GetCurrentUserMatricola());
                string[] temp = str_temp.ToString().Split(';');
                if ((temp != null) && (temp.Count() > 16))
                {
                    SessionHelper.Set("UtenteAnagrafica", Utente.CaricaAnagrafica(temp));
                    return temp[8];
                }

            }
            catch (Exception ed)
            {

            }
            return "";
        }

        public static void GetParamCVSP(string matricola, bool noSession, out string figPro, out string serv)
        {
            figPro = String.Empty;
            serv = String.Empty;

            try
            {
                //string str_temp = wsAnag.EsponiAnagrafica("RAICV;" + CommonManager.GetCurrentUserMatricola() + ";;E;0");
                string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(matricola);
                string[] temp = str_temp.ToString().Split(';');
                if ((temp != null) && (temp.Count() > 16))
                {
                    if (!noSession) SessionHelper.Set("UtenteAnagrafica", Utente.CaricaAnagrafica(temp));
                    figPro = temp[8];
                    serv = temp[27];
                }

            }
            catch (Exception ed)
            {

            }
        }

        public static string GetSocieta(string matricola)
        {
            try
            {
                string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(CommonManager.GetCurrentUserMatricola());
                string[] temp = str_temp.ToString().Split(';');
                if ((temp != null) && (temp.Count() > 16))
                {
                    SessionHelper.Set("UtenteAnagrafica", Utente.CaricaAnagrafica(temp));
                    return temp[14];
                }

            }
            catch (Exception ed)
            {

            }
            return "";
        }

        /// <summary>
        /// Recupero del codice figura professionale senza inserire i dati in sessione
        /// </summary>
        /// <param name="matricola"></param>
        /// <returns></returns>
        public static string GetFiguraProfessionale_NoSession(string matricola)
        {
            try
            {
                //string str_temp = wsAnag.EsponiAnagrafica("RAICV;" + CommonManager.GetCurrentUserMatricola() + ";;E;0");
                string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(CommonManager.GetCurrentUserMatricola());
                string[] temp = str_temp.ToString().Split(';');
                if ((temp != null) && (temp.Count() > 16))
                {
                    return temp[8];
                }
            }
            catch (Exception ed)
            {
            }
            return "";

        }

        public static string GetSezioneContabile(string matricola, bool noSession)
        {
            try
            {
                string selectedMatricola = noSession ? matricola : CommonManager.GetCurrentUserMatricola();

                //string str_temp = wsAnag.EsponiAnagrafica("RAICV;" + selectedMatricola + ";;E;0");
                string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(selectedMatricola);
                string[] temp = str_temp.ToString().Split(';');
                if ((temp != null) && (temp.Count() > 16))
                {
                    if (!noSession) SessionHelper.Set("UtenteAnagrafica", Utente.CaricaAnagrafica(temp));
                    return temp[27];
                }

            }
            catch (Exception ed)
            {

            }
            return "";
        }

        public static string GetStruttura(string matricola)
        {
            string struttura = "";

            try
            {
                string selectedMatricola = matricola;

                //string str_temp = wsAnag.EsponiAnagrafica("RAICV;" + selectedMatricola + ";;E;0");
                string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(selectedMatricola);
                string[] temp = str_temp.ToString().Split(';');
                if ((temp != null) && (temp.Count() > 16))
                {
                    return temp[4];
                }
            }
            catch (Exception ed)
            {

            }

            return struttura;
        }

        public static string getTestoPrivacy()
        {
            string testo = "";
            try
            {
                digiGappEntities db = new digiGappEntities();
                string matricola = CommonManager.GetCurrentUserMatricola();
                string sezione = GetSezioneContabile(matricola, true);

                string chiavePar = "TestoPrivacy";
                switch (sezione.ToUpper())
                {
                    case "B2":
                        chiavePar = "TestoPrivacyB2";
                        break;
                    case "N2":
                        chiavePar = "TestoPrivacyN2";
                        break;
                    case "C2":
                        chiavePar = "TestoPrivacyC2";
                        break;
                    default:
                        chiavePar = "TestoPrivacy";
                        break;
                }

                MyRai_ParametriSistema pars = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == chiavePar);
                testo = pars != null ? pars.Valore1 : GetParametro<string>(EnumParametriSistema.TestoPrivacy);
            }
            catch (Exception)
            {
            }
            return testo;
        }

        public static DateTime ConvertToDate(string date)
        {
            DateTime Date;
            if (date != null && date.Contains("/"))
                DateTime.TryParseExact(date, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out Date);
            else
                DateTime.TryParseExact(date, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out Date);

            return Date;

        }

        public static string ToTitleCase(string str)
        {
            if (str == null) return str;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        public static MyRai_Richieste CopyFromRichieste(MyRai_Richieste Source)
        {
            var Dest = new MyRai_Richieste();
            foreach (var pS in Source.GetType().GetProperties())
            {
                if (pS.Name.ToLower() == "id_richiesta" || pS.Name.ToLower().StartsWith("myrai") || pS.Name.ToLower().StartsWith("entity")) continue;
                foreach (var pT in Dest.GetType().GetProperties())
                {
                    if (pT.Name != pS.Name) continue;
                    (pT.GetSetMethod()).Invoke(Dest, new object[] { pS.GetGetMethod().Invoke(Source, null) });
                }
            };
            return Dest;
        }

        public static MyRai_Eccezioni_Richieste CopyFrom(MyRai_Eccezioni_Richieste Source)
        {
            var Dest = new MyRai_Eccezioni_Richieste();
            foreach (var pS in Source.GetType().GetProperties())
            {
                if (pS.Name.ToLower() == "id_eccezioni_richieste" || pS.Name.ToLower().StartsWith("myrai") || pS.Name.ToLower().StartsWith("entity")) continue;
                foreach (var pT in Dest.GetType().GetProperties())
                {
                    if (pT.Name != pS.Name) continue;
                    (pT.GetSetMethod()).Invoke(Dest, new object[] { pS.GetGetMethod().Invoke(Source, null) });
                }
            };
            return Dest;
        }
        public static void InserisciEccezioneRimpiazzo(int IdEccezioneRichiesta, string eccezioneRimpiazzo)
        {
            var db = new digiGappEntities();
            MyRai_Eccezioni_Richieste eccez = db.MyRai_Eccezioni_Richieste.Where(x => x.id_eccezioni_richieste == IdEccezioneRichiesta).FirstOrDefault();

            if (eccez != null)
            {
                if (!String.IsNullOrWhiteSpace(eccezioneRimpiazzo))
                {
                    string rep = eccez.MyRai_Richieste.reparto;
                    if (String.IsNullOrWhiteSpace(rep) || rep.Trim() == "0" || rep.Trim() == "00") rep = null;
                    var InsBatchRimpiazzo = new MyRai_PianoFerieBatch()
                    {
                        alle = eccez.alle != null ? ((DateTime)eccez.alle).ToString("HH:mm") : "",
                        dalle = eccez.dalle != null ? ((DateTime)eccez.dalle).ToString("HH:mm") : "",
                        quantita = eccez.quantita == 1 ? "1" : eccez.quantita.ToString(),
                        codice_eccezione = eccezioneRimpiazzo.Trim(),
                        data_creazione_record = DateTime.Now,
                        data_eccezione = (DateTime)eccez.data_eccezione,
                        sedegapp = eccez.codice_sede_gapp + rep,
                        importo = eccez.importo == null ? "" : eccez.importo.ToString(),
                        matricola = eccez.MyRai_Richieste.matricola_richiesta
                    };
                    db.MyRai_PianoFerieBatch.Add(InsBatchRimpiazzo);
                    db.SaveChanges();
                }
            }
        }
        public static void InserisciEccezioneRipianificata(int IdEccezioneRichiesta, string matricola = null)
        {
            var db = new digiGappEntities();

            try
            {
                MyRai_Eccezioni_Richieste eccez = db.MyRai_Eccezioni_Richieste.Where(x => x.id_eccezioni_richieste == IdEccezioneRichiesta).FirstOrDefault();

                if (eccez != null && eccez.DataRipianificazione != null)
                {
                    string matr = eccez.MyRai_Richieste.matricola_richiesta;

                    //if (db.MyRai_PianoFerieBatch.Any(x => x.codice_eccezione.Trim() == eccez.cod_eccezione.Trim() && x.matricola == matr && x.data_eccezione == eccez.DataRipianificazione))
                    //{
                    //    Logger.LogErrori(new MyRai_LogErrori()
                    //    {
                    //        applicativo = "Portale",
                    //        data = DateTime.Now,
                    //        matricola = CommonManager.GetCurrentUserMatricola(),
                    //        provenienza = "InserisciEccezioneRipianificata",
                    //        error_message = "Eccezione Ripianificata già presente in MyRai_PianoFerieBatch,  idEccezRichieste " + IdEccezioneRichiesta 
                    //    });
                    //    return;
                    //}

                    string rep = eccez.MyRai_Richieste.reparto;
                    if (String.IsNullOrWhiteSpace(rep) || rep.Trim() == "0" || rep.Trim() == "00") rep = null;

                    if (eccez.DataRipianificazione != null)
                    {
                        var InsBatch = new MyRai_PianoFerieBatch()
                        {
                            alle = eccez.alle != null ? ((DateTime)eccez.alle).ToString("HH:mm") : "",
                            dalle = eccez.dalle != null ? ((DateTime)eccez.dalle).ToString("HH:mm") : "",
                            quantita = eccez.quantita == 1 ? "1" : eccez.quantita.ToString(),
                            codice_eccezione = eccez.cod_eccezione.Trim(),
                            data_creazione_record = DateTime.Now,
                            provenienza = "PianoFerie " + eccez.data_eccezione.Year + "-Ripianificato da " + eccez.data_eccezione.ToString("dd/MM/yyyy") + "-" + IdEccezioneRichiesta + "-" + GetCurrentUserMatricola() + "-" + DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            data_eccezione = (DateTime)eccez.DataRipianificazione,
                            sedegapp = eccez.codice_sede_gapp + rep,
                            importo = eccez.importo == null ? "" : eccez.importo.ToString(),
                            matricola = eccez.MyRai_Richieste.matricola_richiesta
                        };
                        db.MyRai_PianoFerieBatch.Add(InsBatch);
                    }

                    if (!String.IsNullOrWhiteSpace(eccez.eccezione_rimpiazzo_storno))
                    {
                        var InsBatchRimpiazzo = new MyRai_PianoFerieBatch()
                        {
                            alle = eccez.eccezione_rimpiazzo_alle != null ? ((DateTime)eccez.eccezione_rimpiazzo_alle).ToString("HH:mm") : "",
                            dalle = eccez.eccezione_rimpiazzo_dalle != null ? ((DateTime)eccez.eccezione_rimpiazzo_dalle).ToString("HH:mm") : "",
                            quantita = eccez.eccezione_rimpiazzo_quantita == 1 ? "1" : eccez.eccezione_rimpiazzo_quantita.ToString(),
                            codice_eccezione = eccez.eccezione_rimpiazzo_storno.Trim(),
                            data_creazione_record = DateTime.Now,

                            provenienza =
                            (eccez.DataRipianificazione != null ?
                            "PianoFerie " + eccez.data_eccezione.Year + "-Ripianificato da " + eccez.data_eccezione.ToString("dd/MM/yyyy") + "-" + IdEccezioneRichiesta + "-" + GetCurrentUserMatricola() + "-" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "-InApprovazione"
                            : "Storno SW-InApprovazione"),

                            data_eccezione = (DateTime)eccez.data_eccezione,
                            sedegapp = eccez.codice_sede_gapp + rep,
                            importo = eccez.importo == null ? "" : eccez.importo.ToString(),
                            matricola = eccez.MyRai_Richieste.matricola_richiesta
                        };


                        db.MyRai_PianoFerieBatch.Add(InsBatchRimpiazzo);

                        if (eccez.eccezione_rimpiazzo_richiedeSWH == true)
                        {
                            int minutiSWH = 0;
                            if (EccezioniManager.IsEccezioneAQuarti(eccez.eccezione_rimpiazzo_storno))
                                minutiSWH = EccezioniManager.GetTreQuartiGiornataMinuti(matr, eccez.data_eccezione.ToString("dd/MM/yyyy"));
                            else if (EccezioniManager.IsEccezione_0_50(eccez.eccezione_rimpiazzo_storno))
                                minutiSWH = EccezioniManager.GetMetaGiornataMinuti(matr, eccez.data_eccezione.ToString("dd/MM/yyyy"));

                            if (minutiSWH > 0)
                            {
                                var InsBatchSWH = new MyRai_PianoFerieBatch()
                                {
                                    alle = "",
                                    dalle = "",
                                    quantita = minutiSWH.ToHHMM(),
                                    codice_eccezione = "SWH",
                                    data_creazione_record = DateTime.Now,

                                    provenienza = "Storno SW-InApprovazione",

                                    data_eccezione = (DateTime)eccez.data_eccezione,
                                    sedegapp = eccez.codice_sede_gapp + rep,
                                    importo = eccez.importo == null ? "" : eccez.importo.ToString(),
                                    matricola = eccez.MyRai_Richieste.matricola_richiesta
                                };
                                db.MyRai_PianoFerieBatch.Add(InsBatchSWH);
                            }
                        }
                    }
                    db.SaveChanges();

                    try
                    {
                        if (eccez.DataRipianificazione != null)
                            CambiaDataPianoFerieGiorni(eccez);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogErrori(new MyRai_LogErrori()
                        {
                            applicativo = "Portale",
                            data = DateTime.Now,
                            error_message = ex.ToString(),
                            matricola = CommonManager.GetCurrentUserMatricola(),
                            provenienza = "InserisciEccezioneRipianificata"
                        });
                    }

                    if (eccez.DataRipianificazione != null)
                    {
                        Logger.LogAzione(new MyRai_LogAzioni()
                        {
                            applicativo = "Portale",
                            data = DateTime.Now,
                            matricola = (matricola == null) ? CommonManager.GetCurrentUserMatricola() : matricola,
                            provenienza = "InserisciEccezioneRipianificata",
                            operazione = "Inserimento Ripianificazione",
                            descrizione_operazione = "id eccezione_rich " + eccez.id_eccezioni_richieste + " " + eccez.cod_eccezione + " del " + eccez.data_eccezione.ToString("dd/MM/yyyy") +
                            " ripianificata per il " + ((DateTime)eccez.DataRipianificazione).ToString("dd/MM/yyyy") + " tramite tabella batch - matricola: " + matr
                        }, matricola);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    data = DateTime.Now,
                    matricola = (matricola == null) ? CommonManager.GetCurrentUserMatricola() : matricola,
                    provenienza = "InserisciEccezioneRipianificata",
                    error_message = ex.ToString()
                }, matricola);
            }
        }

        public static void CambiaDataPianoFerieGiorni(MyRai_Eccezioni_Richieste eccez)
        {
            string matricola = eccez.MyRai_Richieste.matricola_richiesta;
            DateTime OldDate = eccez.data_eccezione;
            DateTime NewDate = (DateTime)eccez.DataRipianificazione;
            var db = new digiGappEntities();
            var PianoFerieGiorniRow = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matricola && x.data == OldDate && x.eccezione == eccez.cod_eccezione).FirstOrDefault();
            if (PianoFerieGiorniRow != null)
            {
                PianoFerieGiorniRow.data = NewDate;
                db.SaveChanges();

                Logger.LogAzione(new MyRai_LogAzioni()
                {
                    applicativo = "Portale",
                    data = DateTime.Now,
                    operazione = "Ripianificazione giorni",
                    matricola = matricola,
                    provenienza = "CambiaDataPianoFerieGiorni",
                    descrizione_operazione = "Ripianificazione " + OldDate.ToString("ddMMyyyy") + " su " + NewDate.ToString("ddMMyyyy") + " - modificato PianoFerieGiorni ID " + PianoFerieGiorniRow.id
                });
            }
            else
            {
                Logger.LogAzione(new MyRai_LogAzioni()
                {
                    applicativo = "Portale",
                    data = DateTime.Now,
                    operazione = "Ripianificazione giorni",
                    matricola = matricola,
                    provenienza = "CambiaDataPianoFerieGiorni",
                    descrizione_operazione = "Ripianificazione " + OldDate.ToString("ddMMyyyy") + " su " + NewDate.ToString("ddMMyyyy") + " - PianoFerieGiorni NON TROVATO "
                });
            }
        }

        public static void Copy(object copyToObject, object copyFromObject)
        {
            foreach (PropertyInfo sourcePropertyInfo in copyFromObject.GetType().GetProperties())
            {
                PropertyInfo destPropertyInfo = copyToObject.GetType().GetProperty(sourcePropertyInfo.Name);

                if (destPropertyInfo != null)
                {
                    destPropertyInfo.SetValue(
                        copyToObject,
                        sourcePropertyInfo.GetValue(copyFromObject, null),
                        null);
                }
            }
        }

        public static List<Tuple<int, string>> GetDocumentiDipendente(string CodiceEccezione)
        {
            string matr = CommonManager.GetCurrentUserMatricola();
            var db = new digiGappEntities();
            string categorie = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == CodiceEccezione)
                .Select(x => x.IdTipologieDocumento).FirstOrDefault();

            if (!String.IsNullOrWhiteSpace(categorie))
            {
                try
                {
                    string[] cat = categorie.Split(',');
                    int[] categ = Array.ConvertAll(cat, s => int.Parse(s));
                    List<Tuple<int, string>> L = db.MyRai_DocumentiDipendente
                   .Where(x => x.matricola == matr && x.attivo == true && categ.Contains(x.id_tipologia_documento))
                   .Select(x => new { x.id, x.descrizione })
                   .AsEnumerable()
                   .Select(x => new Tuple<int, string>(x.id, x.descrizione)).ToList();
                    return L;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                List<Tuple<int, string>> L = db.MyRai_DocumentiDipendente
                   .Where(x => x.matricola == matr && x.attivo == true)
                   .Select(x => new Tuple<int, string>(x.id, x.nomefile)).ToList();
                return L;
            }
        }

        public static EccezioniRichiesteShadow CopyToShadow(MyRai_Eccezioni_Richieste Source)
        {
            var Dest = new EccezioniRichiesteShadow();
            foreach (var pS in Source.GetType().GetProperties())
            {
                if (pS.Name.ToLower() == "id_eccezioni_richieste" || pS.Name.ToLower().StartsWith("myrai") || pS.Name.ToLower().StartsWith("entity")) continue;
                foreach (var pT in Dest.GetType().GetProperties())
                {
                    if (pT.Name != pS.Name) continue;
                    (pT.GetSetMethod()).Invoke(Dest, new object[] { pS.GetGetMethod().Invoke(Source, null) });
                }
            };
            return Dest;
        }
        public static MyRai_Eccezioni_Richieste CopyFromShadow(EccezioniRichiesteShadow Source)
        {

            var Dest = new MyRai_Eccezioni_Richieste();
            foreach (var pS in Source.GetType().GetProperties())
            {
                if (pS.Name.ToLower() == "id_eccezioni_richieste" || pS.Name.ToLower().StartsWith("myrai") || pS.Name.ToLower().StartsWith("entity")) continue;
                foreach (var pT in Dest.GetType().GetProperties())
                {
                    if (pT.Name != pS.Name) continue;
                    (pT.GetSetMethod()).Invoke(Dest, new object[] { pS.GetGetMethod().Invoke(Source, null) });
                }
            };
            return Dest;
        }

        public static DateTime GetlastSunday(DateTime dateStart)
        {
            DateTime LastSunday = dateStart.AddDays(-1);

            while (LastSunday.DayOfWeek != DayOfWeek.Sunday)
            {
                LastSunday = LastSunday.AddDays(-1);
            }
            return LastSunday.Date;
        }

        public static DateTime GetlastMonday(DateTime dateStart)
        {
            DateTime LastMonday = dateStart.AddDays(-1);

            while (LastMonday.DayOfWeek != DayOfWeek.Monday)
            {
                LastMonday = LastMonday.AddDays(-1);
            }
            return LastMonday.Date;
        }

        public static object GetPropertyByName(object src, string propertyName)
        {
            if (src == null) return null;
            PropertyInfo p = null;
            try
            {
                p = src.GetType().GetProperty(propertyName);
            }
            catch
            {
            }
            if (p == null) return null;
            else return p.GetValue(src, null);
        }

        public static Boolean IsProduzione()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["digiGappEntities2"].ConnectionString.ToLower().Contains("zto");
        }

        public static string TraduciMeseDaNumLett(string MESENUM)
        {
            string uscita = "";

            Dictionary<string, string> dictionary =
        new Dictionary<string, string>();
            dictionary.Add("00", "");
            dictionary.Add("01", "Gennaio");
            dictionary.Add("02", "Febbraio");
            dictionary.Add("03", "Marzo");
            dictionary.Add("04", "Aprile");
            dictionary.Add("05", "Maggio");
            dictionary.Add("06", "Giugno");
            dictionary.Add("07", "Luglio");
            dictionary.Add("08", "Agosto");
            dictionary.Add("09", "Settembre");
            dictionary.Add("10", "Ottobre");
            dictionary.Add("11", "Novembre");
            dictionary.Add("12", "Dicembre");
            uscita = dictionary[MESENUM];
            return uscita;
        }

        public static bool EmailIsValid(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress)) return false;

            string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
               + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
               + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            Regex ValidEmailRegex = new Regex(validEmailPattern, RegexOptions.IgnoreCase);
            return ValidEmailRegex.IsMatch(emailAddress);
        }

        public static string GetProfileCode(string profileCode)
        {
            return "using System;using System.Collections.Generic;using System.Linq;using myRaiData;using MyRaiServiceInterface.it.rai.servizi;using MyRaiServiceInterface.it.rai.servizi.digigappws;" +
                   "using myRai.Business;using myRai.Models;" +
                   "namespace myRaiHelper { public class Profiler { public bool Test() { return " + profileCode + "; }}}";
        }

        public static List<MyRai_Profili> GetProfiliAbilitatiDaCodice(List<MyRai_Profili> profili)
        {
            digiGappEntities db = new digiGappEntities();
            Expression<Func<MyRai_Profili, bool>> filtroContesto = GetProfiliFiltroContesto();
            List<MyRai_Profili> profiliAbilitatiDaCodice = db.MyRai_Profili.Where(x => x.metodo_abilitazione != null && x.metodo_abilitazione.Trim() != "").Where(filtroContesto).ToList();
            if (!profiliAbilitatiDaCodice.Any()) return profili;

            string[] CodiceCompilatore = CommonManager.GetParametri<string>(EnumParametriSistema.CodiceCSharp);
            foreach (var prof in profiliAbilitatiDaCodice)
            {
                try
                {
                    if (prof.metodo_abilitazione.Trim().StartsWith("("))// se inizia con '(' è un metodo complesso 
                    {
                        if (EseguiViaCSharpCompiler(prof.metodo_abilitazione, CodiceCompilatore[1]))
                            profili.Add(prof);
                    }
                    else
                    {
                        if (EseguiViaReflection(prof.metodo_abilitazione)) // altrimenti lo esegue al volo reflection
                            profili.Add(prof);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        applicativo = "Portale",
                        data = DateTime.Now,
                        error_message = prof.metodo_abilitazione + " " + ex.ToString(),
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        provenienza = "GetProfiliAbilitatiDaCodice()"
                    });
                }
            }
            return profili;
        }

        public static List<MyRai_Profili> GetProfiliAdatti(string matr = null)
        {
            if (String.IsNullOrWhiteSpace(matr))
                matr = GetCurrentUserMatricola();

            string pMatricola = GetCurrentUserPMatricola();

            digiGappEntities db = new digiGappEntities();
            bool utenteGapp = Utente.IsAbilitatoGapp();
            List<string> gruppiAD = Utente.ADgroups().Select(x => x.ToUpper()).ToList();

            ApplicationType applicationType = GetApplicationType();

            string liv = "0";
            if (Utente.IsBoss(pMatricola)) liv = "1";
            if (Utente.IsBossLiv2(pMatricola)) liv = "2";

            Expression<Func<MyRai_Profili, bool>> filtroContesto = GetProfiliFiltroContesto();

            List<MyRai_Profili> profiliAll = db.MyRai_Profili.Where(x => x.metodo_abilitazione == null || x.metodo_abilitazione.Trim() == "")
                                                             .Where(x => !x.aggiuntivo)
                                                             .Where(filtroContesto).ToList();

            Func<MyRai_Profili, bool> FiltroMatricola = x =>
                                !String.IsNullOrWhiteSpace(x.matricola) &&
                                 x.matricola.Trim() != "*" &&
                                      x.matricola.Split(',')
                                          .Any(frag =>
                                                    !String.IsNullOrWhiteSpace(frag) &&
                                                    frag != "*" &&
                                                    (frag.Trim() == matr ||
                                                      (frag.Trim().EndsWith("*") && matr.StartsWith(frag.Trim().Replace("*", ""))))
                                                 );

            List<MyRai_Profili> profili = profiliAll.Where(FiltroMatricola)
                            .ToList();

            Func<MyRai_Profili, bool> FiltroGruppiAD = x =>
               x.matricola == "*" && x.richiede_gapp == utenteGapp &&
                   x.gruppo_AD != "*" && gruppiAD.Contains(x.gruppo_AD.ToUpper());
            if (profili.Count == 0)
            {
                profili = profiliAll.Where(FiltroGruppiAD).ToList();
            }

            Func<MyRai_Profili, bool> FiltroLivello = x =>
               x.matricola == "*" && x.gruppo_AD == "*"
                   && x.richiede_gapp == utenteGapp && x.livello == liv;
            if (profili.Count == 0)
            {
                profili = profiliAll.Where(FiltroLivello).ToList();
            }

            Func<MyRai_Profili, bool> FiltroGenerico = x =>
               x.matricola == "*" && x.gruppo_AD == "*"
                   && x.richiede_gapp == utenteGapp && x.livello == "*";
            if (profili.Count == 0)
            {
                profili = profiliAll.Where(FiltroGenerico).ToList();
            }

            //aggiungi profili extra 
            profili = GetProfiliAbilitatiDaCodice(profili);

            //aggiungo i profili aggiuntivi
            List<MyRai_Profili> profiliAgg = db.MyRai_Profili.Where(x => x.metodo_abilitazione == null || x.metodo_abilitazione.Trim() == "")
                            .Where(x => x.aggiuntivo)
                            .Where(filtroContesto)
                            .ToList()
                            .Where(x => FiltroMatricola(x) ||
                                       FiltroGruppiAD(x) ||
                                       FiltroLivello(x) ||
                                       FiltroGenerico(x)
                            ).ToList();
            profili.AddRange(profiliAgg);

            //aggiungo il profilo custom con le voci singole
            if (GetVociAggiuntive(out MyRai_Profili customProfile))
                profili.Add(customProfile);

            switch (applicationType)
            {
                case ApplicationType.RaiPerMe:
                    if (Utente.GestitoSirio())
                    {
                        MyRai_Profili profiloSirio = db.MyRai_Profili.Where(x => x.nome_profilo == "AttivitaSirio").FirstOrDefault();
                        if (profiloSirio != null) profili.Add(profiloSirio);
                    }

                    if (CeitonHelper.IsApprovatoreCeiton(matr))
                    {
                        MyRai_Profili profiloApprCeiton = db.MyRai_Profili.Where(x => x.nome_profilo == "ApprovazioneAttivita").FirstOrDefault();
                        if (profiloApprCeiton != null) profili.Add(profiloApprCeiton);
                    }

                    if (ValutazioniHelper.IsEnabled(matr))
                    {
                        MyRai_Profili valutazioni = db.MyRai_Profili.Where(x => x.nome_profilo == "Valutazioni").FirstOrDefault();
                        if (valutazioni != null) profili.Add(valutazioni);
                    }

                    if (MboHelper.IsEnabled())
                    {
                        MyRai_Profili profiloVal = db.MyRai_Profili.Where(x => x.nome_profilo == "Mbo").FirstOrDefault();
                        if (profiloVal != null) profili.Add(profiloVal);
                    }

                    MyRai_Profili profiloModuli = db.MyRai_Profili.Where(x => x.nome_profilo == "ModuliDownloader").FirstOrDefault();
                    if (profiloModuli != null) profili.Add(profiloModuli);

                    MyRai_Profili profiloAcademy = db.MyRai_Profili.Where(x => x.nome_profilo == "Rai Academy").FirstOrDefault();
                    if (profiloAcademy != null) profili.Add(profiloAcademy);

                    bool avoidForMoney = CommonManager.IsGiornoCedolino(matr) || SessionHelper.Get("RedirectBustaPaga") != null;
                    bool avoidForDoc = IsGiornoDocAmm() || SessionHelper.Get("RedirectDocAmm") != null;

                    if (IsApprovatoreProduzione(matr))
                    {
                        MyRai_Profili profiloApprovatoreProduzione = db.MyRai_Profili.Where(x => x.nome_profilo == "ApprovatoreProduzione").FirstOrDefault();
                        if (profiloApprovatoreProduzione != null)
                            profili.Add(profiloApprovatoreProduzione);
                    }

                    if (IsVistatorePerSedi(CommonManager.GetCurrentUserMatricola()).Any())
                    {
                        MyRai_Profili profiloVistoRichieste = db.MyRai_Profili.Where(x => x.nome_profilo == "VistoRichieste").FirstOrDefault();
                        if (profiloVistoRichieste != null)
                            profili.Add(profiloVistoRichieste);
                    }

                    if (!Utente.GappChiuso() && !avoidForMoney && !avoidForDoc)
                    {
                        if (Utente.GiornateAssenteIngiustificato(matr).Any() || (Utente.GetQuadratura() == Quadratura.Giornaliera && Utente.GiornateConCarenza() != null))
                        {
                            foreach (var p in profili)
                            {
                                p.MyRai_Profili_Menu = p.MyRai_Profili_Menu.Where(x => x.MyRai_Voci_Menu.Titolo != "Richiedi").ToList();

                            }
                        }
                    }

                    if (UtenteHelper.IsBoss(CommonHelper.GetCurrentUserPMatricola()))
                    {
                        string par = GetParametro<string>(EnumParametriSistema.MatricoleAblititateMaternitaCongedi);
                        if (!String.IsNullOrWhiteSpace(par) && (par == "*" || par.Split(',').Contains(GetCurrentUserMatricola())))
                        {
                            MyRai_Profili profiloMatCong = db.MyRai_Profili.Where(x => x.nome_profilo == "PianificazioniMaternita").FirstOrDefault();
                            if (profiloMatCong != null)
                                profili.Add(profiloMatCong);
                        }

                    }

                    break;
                case ApplicationType.Gestionale:

                    if (PoliticheRetributiveHelper.EnabledTo(matr))
                    {
                        if (PoliticheRetributiveHelper.EnabledToRichieste(matr)
                            || PoliticheRetributiveHelper.EnabledToAmm(matr)
                            || PoliticheRetributiveHelper.EnabledToLettere(matr))
                        {
                            MyRai_Profili profiloPolRetr = db.MyRai_Profili.Where(x => x.nome_profilo == "G_PoliticheRetributiveOnly").FirstOrDefault();
                            if (profiloPolRetr != null) profili.Add(profiloPolRetr);
                        }
                        if (PoliticheRetributiveHelper.EnabledToBudget(matr))
                        {
                            MyRai_Profili profiloPolRetr = db.MyRai_Profili.Where(x => x.nome_profilo == "G_PoliticheRetributiveBudgetOnly").FirstOrDefault();
                            if (profiloPolRetr != null) profili.Add(profiloPolRetr);
                        }
                    }

                    if (IncarichiHelper.EnabledToIncarichi(matr))
                    {
                        MyRai_Profili profiloIncarichi = db.MyRai_Profili.Where(x => x.nome_profilo == "G_Incarichi").FirstOrDefault();
                        if (profiloIncarichi != null) profili.Add(profiloIncarichi);
                    }
                    if (MaternitaCongediHelper.EnabledToMaternitaCongedi(matr))
                    {
                        MyRai_Profili profiloMaternitaCongedi = db.MyRai_Profili.Where(x => x.nome_profilo == "G_Maternita").FirstOrDefault();
                        if (profiloMaternitaCongedi != null) profili.Add(profiloMaternitaCongedi);
                    }

                    if (AuthHelper.EnabledTo(matr, "DEMA"))
                    {
                        MyRai_Profili profiloDematerializzazione = db.MyRai_Profili.Where(x => x.nome_profilo == "G_Dematerializzazione").FirstOrDefault();
                        if (profiloDematerializzazione != null)
                            profili.Add(profiloDematerializzazione);

                        // se sono autorizzato
                        var subFunc = AuthHelper.EnabledSubFunc(matr, "DEMA");

                        // se ho la funzione 02ADM - Sono un approvatore
                        if (subFunc.Contains("02ADM"))
                        {
                            MyRai_Profili profiloDematerializzazioneAppr = db.MyRai_Profili.Where(x => x.nome_profilo == "G_Dematerializzazione_Approvatore").FirstOrDefault();
                            if (profiloDematerializzazioneAppr != null)
                                profili.Add(profiloDematerializzazioneAppr);
                        }
                    }
                    break;
            }

            return profili;
        }


        public static bool GetVociAggiuntive(out MyRai_Profili customProfile)
        {
            bool result = false;
            customProfile = null;

            string matricola = CommonHelper.GetCurrentUserMatricola();

            var dbTal = new IncentiviEntities();
            var filtroContesto = GetRegoleVociMenuFiltroContesto();

            List<XR_HRIS_REGOLE_VOCI_MENU> rules = new List<XR_HRIS_REGOLE_VOCI_MENU>();
            IQueryable<XR_HRIS_REGOLE_VOCI_MENU> qryRules = dbTal.XR_HRIS_REGOLE_VOCI_MENU.Where(filtroContesto);

            //Controllo le regole *
            rules.AddRange(qryRules.Where(x => x.TIPO_REGOLA == "*"));

            //Controllo le regole DB_PARAM
            var rulesDbParam = qryRules.Where(x => x.TIPO_REGOLA == "DB_PARAM").ToList();
            if (rulesDbParam.Any())
            {
                rules.AddRange(rulesDbParam.Where(x =>
                                    (x.LST_MATR_INCL == null || x.LST_MATR_INCL.Contains(matricola))
                                    && (x.LST_MATR_EXCL == null || !x.LST_MATR_EXCL.Contains(matricola))
                                    && (x.ABIL_FUNC == null || AuthHelper.EnabledToAnySubFunc(matricola, x.ABIL_FUNC, x.ABIL_SUBFUNC != null ? x.ABIL_SUBFUNC.Split(',') : null))));
            }

            //Controllo le regole CODE
            var rulesCode = qryRules.Where(x => x.TIPO_REGOLA == "CODE" && x.CODE != null);
            if (rulesCode.Any())
            {
                string[] CodiceCompilatore = CommonManager.GetParametri<string>(EnumParametriSistema.CodiceCSharp);
                foreach (var rule in rulesCode)
                {
                    try
                    {
                        if (rule.CODE.Trim().StartsWith("("))// se inizia con '(' è un metodo complesso 
                        {
                            if (EseguiViaCSharpCompiler(rule.CODE, CodiceCompilatore[1]))
                                rules.Add(rule);
                        }
                        else
                        {
                            if (EseguiViaReflection(rule.CODE)) // altrimenti lo esegue al volo reflection
                                rules.Add(rule);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogErrori(new MyRai_LogErrori()
                        {
                            applicativo = "Portale",
                            data = DateTime.Now,
                            error_message = "Regola voce menu: " + rule.CODE + " " + ex.ToString(),
                            matricola = CommonManager.GetCurrentUserMatricola(),
                            provenienza = "GetVociAggiuntive()"
                        });
                    }
                }
            }

            if (rules.Any())
            {
                var idVoci = rules.Select(x => x.ID_VOCE_MENU);
                //get voci menu
                var db = new digiGappEntities();
                var listVoci = db.MyRai_Voci_Menu.Where(x => idVoci.Contains(x.ID)).ToList();
                if (listVoci.Any())
                {
                    result = true;
                    customProfile = new MyRai_Profili()
                    {
                        nome_profilo = "**CUSTOM_PROFILE**",
                        MyRai_Profili_Menu = listVoci.Select(x => new MyRai_Profili_Menu()
                        {
                            idmenu = x.ID,
                            MyRai_Voci_Menu = x
                        }).ToList()
                    };
                }
            }

            return result;
        }

        private static Expression<Func<MyRai_Profili, bool>> GetProfiliFiltroContesto()
        {
            string strAppType = CommonManager.GetAppSettings("AppType");
            if (String.IsNullOrWhiteSpace(strAppType))
                strAppType = "RaiPerMe";

            Expression<Func<MyRai_Profili, bool>> filtro = x => x.contesto == null || x.contesto == "" || x.contesto == "*" || x.contesto.Contains(strAppType);

            if (strAppType == "Gestionale")
                filtro = x => x.contesto.Contains(strAppType);

            return filtro;
        }

        public static Expression<Func<MyRai_HeaderMenu, bool>> GetHeaderFiltroContesto()
        {
            string strAppType = CommonHelper.GetAppSettings("AppType");
            if (String.IsNullOrWhiteSpace(strAppType))
                strAppType = "RaiPerMe";

            Expression<Func<MyRai_HeaderMenu, bool>> filtro = x => x.Contesto == "*" || x.Contesto.Contains(strAppType);

            return filtro;
        }

        private static Expression<Func<XR_HRIS_REGOLE_VOCI_MENU, bool>> GetRegoleVociMenuFiltroContesto()
        {
            string strAppType = CommonHelper.GetAppSettings("AppType");
            if (String.IsNullOrWhiteSpace(strAppType))
                strAppType = "RaiPerMe";

            Expression<Func<XR_HRIS_REGOLE_VOCI_MENU, bool>> filtro = x => x.CONTESTO == null || x.CONTESTO == "" || x.CONTESTO == "*" || x.CONTESTO.Contains(strAppType);

            return filtro;
        }

        public static ApplicationType GetApplicationType()
        {
            ApplicationType sidebarType = ApplicationType.RaiPerMe;
            if (CommonManager.GetAppSettings("AppType") == "Gestionale")
                sidebarType = ApplicationType.Gestionale;
            return sidebarType;
        }

        public static bool? getStatoVisto(int IdEccezioneRichiesta)
        {
            digiGappEntities db = new digiGappEntities();
            var ec = db.MyRai_Eccezioni_Richieste.Where(x => x.id_eccezioni_richieste == IdEccezioneRichiesta).FirstOrDefault();
            if (ec == null) return null;
            else if (ec.data_visto_validato != null) return true;
            else if (ec.data_visto_rifiutato != null) return false;
            else return null;
        }

        public static List<MyRai_Profili> GetAcademyProfiliAdatti()
        {
            digiGappEntities db = new digiGappEntities();
            string matr = GetCurrentUserMatricola().ToUpper();

            List<MyRai_Profili> profili = new List<MyRai_Profili>();

            MyRai_Profili profiloAcademy = db.MyRai_Profili.Where(x => x.nome_profilo == "Rai Academy").FirstOrDefault();
            if (profiloAcademy != null) profili.Add(profiloAcademy);

            return profili;
        }

        public static bool IsApprovatoreUfficioProduzione(string matricola)
        {
            bool result = false;
            try
            {
                // se true abilitato per L3 e L4, false solo L3
                var par = CommonManager.GetParametro<bool>(EnumParametriSistema.AbilitaApprovatoriL4L5);

                if (par)
                {
                    using (digiGappEntities db = new digiGappEntities())
                    {
                        result = db.MyRai_UffProduzioni_Approvatori.Count(w => w.MatricolaApprovatore.Equals(matricola)) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public static List<string> IsVistatorePerSedi(string matricola)
        {
            if (CommonManager.GetParametro<bool>(EnumParametriSistema.AbilitaApprovatoriL6))
                return GetSediL6();
            else
                return new List<string>();
        }

        public static bool IsApprovatoreProduzione(string matricola)
        {
            bool result = false;
            List<string> sedi = new List<string>();
            try
            {
                // se true abilitato per L3 e L4, false solo L3
                var par = CommonManager.GetParametro<bool>(EnumParametriSistema.AbilitaApprovatoriL4L5);

                if (par)
                {
                    sedi.AddRange(GetSediL4());
                    // prendere anche le sedi di livello 3, perchè anche se è abilitato il parametro comunque 
                    // gli approvatori di torino (L3) devono continuare a funzionare.
                    sedi.AddRange(GetSediL3(GetCurrentUserPMatricola()));
                    // FRANCESCO DA VALUTARE    
                    sedi.AddRange(GetSediL5());
                }
                else
                {
                    sedi = GetSediL3(GetCurrentUserPMatricola());
                }

                if (sedi != null && sedi.Any())
                {
                    result = true;
                    return result;
                }

                if (par && !result)
                {
                    using (digiGappEntities db = new digiGappEntities())
                    {
                        result = db.MyRai_UffProduzioni_Approvatori.Count(w => w.MatricolaApprovatore.Equals(matricola)) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public static bool EseguiViaCSharpCompiler(string cmd, string precode)
        {
            //return true;
            if (String.IsNullOrWhiteSpace(cmd)) return false;
            dynamic oggetto = Macinatore.Compila(
                              CommonManager.GetProfileCode(cmd),
                              "myRaiHelper.Profiler",
                              precode //assembly necessari separati da virgola
                              );

            if (oggetto == null || oggetto is System.CodeDom.Compiler.CompilerErrorCollection)
            {
                string error = "Errore per codice profilo: " + GetProfileCode(cmd) + " - ";

                if (oggetto != null)
                {
                    foreach (var e in oggetto as System.CodeDom.Compiler.CompilerErrorCollection)
                    {
                        error += e.ToString();
                    }
                }

                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    data = DateTime.Now,
                    error_message = error,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "GetProfiliAdatti"
                });
                return false;
            }
            bool res = oggetto.Test();
            return res;
        }

        public static bool EseguiViaReflection(string cmd)
        {
            if (String.IsNullOrWhiteSpace(cmd)) return false;

            try
            {
                string[] parts = cmd.Split('.');
                string type = String.Join(".", parts.ToList().Take(parts.Length - 1).ToArray());
                string method = parts[parts.Length - 1].Split('(')[0].Trim();
                string argument = null;

                Match result = new Regex("\\(\"(.*)\"\\)").Match(cmd);

                if (result.Success) argument = result.Groups[1].Value;

                object resp = Type.GetType(type).GetMethod(method).Invoke(null, argument == null ? null : new object[] { argument });
                return (bool)resp;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    data = DateTime.Now,
                    error_message = cmd + " - " + ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "ExecuteString"
                });
                return false;
            }
        }

        public static bool AggiungiSempre()
        {
            return true;
        }

        public static bool MatricolaIn(string matricola, string matricoleList)
        {
            if (String.IsNullOrWhiteSpace(matricoleList)) return false;
            return matricoleList.Split(',').Select(x => x.Trim()).Contains(matricola);
        }

        public static bool IsApprovatoreCeiton()
        {
            digiGappEntities db = new digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();
            return db.MyRai_AttivitaCeiton.Any(a => a.MatricolaResponsabile == matr);
        }

        public static sidebarModel getSidebarModel(sidebarModel model = null, bool isAcademy = false)
        {
            string matr = GetCurrentUserMatricola();
            string pmatr = GetCurrentUserPMatricola();
            if (model == null) model = new sidebarModel(pmatr, matr, 0);

            var db = new digiGappEntities();

            if (isAcademy)
                model.myProfili = GetAcademyProfiliAdatti();
            else
                model.myProfili = GetProfiliAdatti(matr);

            model.sezioni = new List<sezioniMenu>();

            foreach (var profilo in model.myProfili)
            {
                if (profilo != null && profilo.MyRai_Profili_Menu.Count() > 0)
                {
                    //prende le voci menu con IdPadre nullo
                    foreach (var item in profilo.MyRai_Profili_Menu
                                        .Where(z => z.MyRai_Voci_Menu.Id_Padre == null)
                                        .OrderBy(a => a.MyRai_Voci_Menu.progressivo)
                        )
                    {
                        model.sezioni.Add(new sezioniMenu()
                        {
                            codiceMy = item.MyRai_Voci_Menu.codiceMy,
                            customView = item.MyRai_Voci_Menu.customView,
                            nomeSezione = item.MyRai_Voci_Menu.nomeSezione,
                            Titolo = item.MyRai_Voci_Menu.Titolo,
                            vociMenu = new List<voceMenu>(),
                            ID = item.MyRai_Voci_Menu.ID,
                            DaEscludere = profilo.escludi_voci,
                            RichiedeGapp = item.MyRai_Voci_Menu.richiede_gapp,
                            progressivo = item.MyRai_Voci_Menu.progressivo
                        });
                    }
                    //se ci sono voci di menu con IdPadre valorizzato, assegnale alle sezioni sopra
                    foreach (var item in profilo.MyRai_Profili_Menu.Where(z => z.MyRai_Voci_Menu.Id_Padre != null))
                    {
                        if (model.sezioni.Where(x => x.ID == item.MyRai_Voci_Menu.Id_Padre).Count() > 0)
                        {
                            var sezPadre = model.sezioni.Where(x => x.ID == item.MyRai_Voci_Menu.Id_Padre).FirstOrDefault();

                            if (sezPadre.vociMenu == null) sezPadre.vociMenu = new List<voceMenu>();
                            sezPadre.vociMenu.Add(new voceMenu()
                            {
                                codiceMy = item.MyRai_Voci_Menu.codiceMy,
                                customView = item.MyRai_Voci_Menu.customView,
                                nomeSezione = item.MyRai_Voci_Menu.nomeSezione,
                                Titolo = item.MyRai_Voci_Menu.Titolo,
                                RichiedeGapp = item.MyRai_Voci_Menu.richiede_gapp,
                                progressivo = item.MyRai_Voci_Menu.progressivo
                            });
                            sezPadre.vociMenu = sezPadre.vociMenu.OrderBy(x => x.progressivo).ToList();
                        }
                    }
                }
            }
            model.sezioni.RemoveAll(z =>
                model.sezioni.Where(x =>
                    x.DaEscludere == true).Select(x => x.ID).ToList().Contains(z.ID));

            model.sezioni = model.sezioni.GroupBy(x => x.ID).Select(x => x.First()).ToList();

            if (!isAcademy)
            {

                if (model.sezioni.Any(x => x.Titolo == "Produzione"))
                {
                    if (!IsApprovatoreProduzione(GetCurrentUserMatricola()))
                        model.sezioni.RemoveAll(x => x.Titolo == "Produzione");
                }
            }


            return model;
        }

        public static string TransformDates(string jsonstring)
        {
            Regex R = new Regex(@"\\\/Date\((\d{10,14})\)\\\/");
            MatchCollection MC = R.Matches(jsonstring);
            for (int i = 0; i < MC.Count; i++)
            {
                string s = MC[i].Groups[1].Value;
                DateTime da = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(Convert.ToDouble(s));
                jsonstring = jsonstring.Replace(MC[i].Groups[0].Value, da.ToString("dd/MM/yyyy"));
            }
            return jsonstring;
        }

        #region Percentual Curriculum Vitae
        public static int GetPercentualCV(string matr = "")
        {
            List<string> sezioniIncomplete = null;
            List<string> sezioniMancanti = null;
            return GetPercentualCV(matr, ref sezioniMancanti, ref sezioniIncomplete);
        }

        public class GetPercentualCVResults
        {
            public GetPercentualCVResults()
            {
                this.Matricola = string.Empty;
                this.Percentuale = 0;
                this.Incomplete = string.Empty;
                this.ListaBoxIncompleti = new List<string>();
                //this.ListaBoxCompleti = new List<string>();
                //this.ListaBoxCompletiNoCoeff = new List<string>();
                this._incompleti = null;
            }

            public string Matricola { get; set; }
            public int Percentuale { get; set; }
            public int BoxValPerc { get; set; }
            public string Mancanti { get; set; }
            public string Incomplete { get; set; }
            public int NumComplete { get; set; }
            public string Complete { get; set; }
            public int NumCompleteNoCoeff { get; set; }
            public string CompleteNoCoeff { get; set; }
            public string FiguraProf { get; set; }
            public string Servizio { get; set; }
            public List<string> ListaBoxIncompleti
            {
                get
                {
                    if (String.IsNullOrEmpty(this.Incomplete))
                    {
                        return null;
                    }

                    if (_incompleti != null) return _incompleti;
                    _incompleti = new List<string>();
                    _incompleti = Incomplete.Split('|').ToList();

                    if (_incompleti != null)
                    {
                        _incompleti = _incompleti.Distinct().ToList();

                        // può capitare il caso come segue: "|TCVCertificazioni|TSVEsperProd|TCVConProf"
                        // a questo punto lo split creerebbe un elemento in posizione 0 vuoto.
                        // Si procede con la rimozione dell'elemento vuoto
                        if (String.IsNullOrEmpty(_incompleti[0]))
                        {
                            _incompleti.RemoveAt(0);
                        }
                    }

                    return _incompleti;
                }
                set
                {
                    _incompleti = value;
                }
            }
            public List<string> ListaBoxCompleti
            {
                get
                {
                    if (String.IsNullOrEmpty(this.Complete))
                    {
                        return null;
                    }

                    if (_completi != null) return _completi;
                    _completi = new List<string>();
                    _completi = Complete.Split('|').ToList();

                    if (_completi != null)
                    {
                        _completi = _completi.Distinct().ToList();

                        // può capitare il caso come segue: "|TCVCertificazioni|TSVEsperProd|TCVConProf"
                        // a questo punto lo split creerebbe un elemento in posizione 0 vuoto.
                        // Si procede con la rimozione dell'elemento vuoto
                        if (String.IsNullOrEmpty(_completi[0]))
                        {
                            _completi.RemoveAt(0);
                        }
                    }

                    return _completi;
                }
                set
                {
                    _completi = value;
                }
            }
            public List<string> ListaBoxCompletiNoCoeff
            {
                get
                {
                    if (String.IsNullOrEmpty(this.CompleteNoCoeff))
                    {
                        return null;
                    }

                    if (_completiNoCoeff != null) return _completiNoCoeff;
                    _completiNoCoeff = new List<string>();
                    _completiNoCoeff = CompleteNoCoeff.Split('|').ToList();

                    if (_completiNoCoeff != null)
                    {
                        _completiNoCoeff = _completiNoCoeff.Distinct().ToList();

                        // può capitare il caso come segue: "|TCVCertificazioni|TSVEsperProd|TCVConProf"
                        // a questo punto lo split creerebbe un elemento in posizione 0 vuoto.
                        // Si procede con la rimozione dell'elemento vuoto
                        if (String.IsNullOrEmpty(_completiNoCoeff[0]))
                        {
                            _completiNoCoeff.RemoveAt(0);
                        }
                    }

                    return _completiNoCoeff;
                }
                set
                {
                    _completiNoCoeff = value;
                }
            }
            public List<string> ListaBoxMancanti
            {
                get
                {
                    if (String.IsNullOrEmpty(this.Mancanti))
                    {
                        return null;
                    }

                    if (_mancanti != null) return _mancanti;
                    _mancanti = new List<string>();
                    _mancanti = Mancanti.Split('|').ToList();

                    if (_mancanti != null)
                    {
                        _mancanti = _mancanti.Distinct().ToList();

                        // può capitare il caso come segue: "|TCVCertificazioni|TSVEsperProd|TCVConProf"
                        // a questo punto lo split creerebbe un elemento in posizione 0 vuoto.
                        // Si procede con la rimozione dell'elemento vuoto
                        if (String.IsNullOrEmpty(_mancanti[0]))
                        {
                            _mancanti.RemoveAt(0);
                        }
                    }

                    return _mancanti;
                }
                set
                {
                    _mancanti = value;
                }
            }

            private List<String> _mancanti { get; set; }
            private List<String> _incompleti { get; set; }
            private List<String> _completi { get; set; }
            private List<String> _completiNoCoeff { get; set; }
        }

        public static int GetPercentualCV(string matr, ref List<string> sezioniMancanti, ref List<string> sezioniIncomplete)
        {
            try
            {
                myRai.Data.CurriculumVitae.cv_ModelEntities cvEnt = new myRai.Data.CurriculumVitae.cv_ModelEntities();
                GetPercentualCVResults myResult = GetStatCV(matr, cvEnt);

                int percentuale = 0;

                if (myResult != null)
                {
                    percentuale = myResult.Percentuale;
                    if (sezioniMancanti != null && myResult.ListaBoxMancanti != null)
                        sezioniMancanti.AddRange(myResult.ListaBoxMancanti);

                    if (sezioniIncomplete != null && myResult.ListaBoxIncompleti != null)
                        sezioniIncomplete.AddRange(myResult.ListaBoxIncompleti);
                }

                return percentuale;
            }
            catch (Exception ex)
            {

                return 0;
            }
        }

        public static GetPercentualCVResults GetStatCV(string matr, myRai.Data.CurriculumVitae.cv_ModelEntities cvEnt)
        {
            matr = matr.Trim();

            string statisticheSQL = String.Empty;

            using (digiGappEntities db = new digiGappEntities())
            {
                var sql = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("StoredStatistiche", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                string sezAbil = GetParametro<string>(EnumParametriSistema.CVEditorialiSezContabiliAbilitate);
                if (sql != null)
                {
                    statisticheSQL = sql.Valore1;
                    statisticheSQL = statisticheSQL.Replace("###", matr.Replace("P", ""));
                    string figpro = "";
                    string serv = "";
                    GetParamCVSP(matr.Replace("P", ""), true, out figpro, out serv);
                    if (!String.IsNullOrWhiteSpace(figpro)) statisticheSQL.Replace("#F#", figpro);
                    if (!String.IsNullOrWhiteSpace(serv)) statisticheSQL.Replace("#S#", serv);
                    //nel db le sezioni abilitate sono separate dalla virgola
                    //per sql è necessario che i valori siano separati dagli apici
                    //Il primo e ultimo apice della lista sono già inclusi nello script
                    statisticheSQL = statisticheSQL.Replace("#sezAbil", sezAbil.Replace(",", "','"));
                }
            }

            GetPercentualCVResults myTest = cvEnt.Database.SqlQuery<GetPercentualCVResults>(statisticheSQL).FirstOrDefault();
            return myTest;
        }

        public static List<TCVBox_V2> GetListaBox(cv_ModelEntities cvEnt, string matricola = "")
        {
            //freak - creare la lista da poi ciclare per creare i box
            string figurapro = "";
            string sezContabile = "";

            if (matricola != "")
            {

                //figurapro = GetFiguraProfessionale(matricola);
                figurapro = GetFiguraProfessionale_NoSession(matricola);
                sezContabile = GetSezioneContabile(matricola, true);
            }
            else
            {

                figurapro = Utente.EsponiAnagrafica()._codiceFigProf;
                sezContabile = Utente.EsponiAnagrafica()._sezContabile;
                matricola = Utente.EsponiAnagrafica()._matricola;
            }

            //risultato tabella inner join con TCVBox_Figuraprof_V2
            var innerJoinQuery =
            (from box in cvEnt.TCVBox_V2
             join boxDettaglio in cvEnt.TCVBox_Figuraprof_V2 on box.Id_box equals boxDettaglio.Id_box
             where (boxDettaglio.CodiceFiguraPro == figurapro)
             orderby boxDettaglio.Posizione
             select box).ToList(); //produces flat sequence

            //lista completa della tabella TCVBox_V2 con posizione != null
            var lista = (cvEnt.TCVBox_V2.Where(pos => pos.Posizione != null).OrderBy(d => d.Posizione)).ToList();
            int count = innerJoinQuery.Count();
            var completa = lista;


            //per quanto riguarda il profilo professionale dei programmisti registi e assistenti ai programmi 
            //rimane invariata l’area di estensione delle sezioni dedicate “Attività e competenze editoriali” e “Impegni editoriali”
            //ossia limitata ai programmisti registi e assistenti ai programmi che operano nelle Reti TV (Rai Uno, Rai Due, Rai Tre, Rai Cultura, Rai Gold, Rai Ragazzi).
            if ((figurapro == "XAA" || figurapro == "XDA") && !AbilitatoEditoriali(sezContabile)
                && !cvEnt.TSVEsperProd.Any(f => f.STATO == "C" && f.COD_ANAGRAFIA != "IR" && f.MATRICOLA == matricola)
                && !cvEnt.TCVConProf.Any(x => x.Matricola == matricola))
            {
                count = 0;
            }

            if (count > 0)
            {
                completa = innerJoinQuery; //se il count > 0 prendo come riferimento la selezione con l'inner join
            }

            return completa;
        }

        public static bool AbilitatoEditoriali(string sezContabile)
        {
            string elencoSezAbil = GetParametro<string>(EnumParametriSistema.CVEditorialiSezContabiliAbilitate);

            //Lasciamo aperta la strada nel caso si decida di abilitare tutte le sezioni
            //In questo modo possiamo intervenire subito via DB e successivamente via codice
            if (String.IsNullOrWhiteSpace(elencoSezAbil) || elencoSezAbil == "*")
                return true;

            string[] sezAbil = elencoSezAbil.Split(',');

            return sezAbil.Contains(sezContabile);
        }
        #endregion

        public static Boolean PdfAutorizzato(string matricolaRichiedente)
        {
            if (CommonManager.GetParametro<string>(EnumParametriSistema.MatricoleAutorizzatePDF).Split(',').Contains(matricolaRichiedente))
            {
                return true;
            }

            myRai.Data.CurriculumVitae.cv_ModelEntities db = new myRai.Data.CurriculumVitae.cv_ModelEntities();
            var check = db.TXLoginDB2.Join(
                db.MENU,
                a => a.PROFILO + "MP",
                b => b.Profilo,
                (a, b) =>
                    new { item1 = a, item2 = b })
                     .Where(a =>
                         a.item1.IDENTIFICATIVO == "P" + matricolaRichiedente);

            bool esito = check != null && check.Count() > 0;
            return esito;

        }

        public static Boolean IsEsentePianoFerie(string matricola)
        {
            bool esente = false;
            var db = new digiGappEntities();
            DateTime D = DateTime.Now;

            int? IdTipologiaEsenteFerie = db.MyRai_InfoDipendente_Tipologia.Where(x => x.info == "Esente Piano Ferie" && x.data_inizio <= D && (x.data_fine >= D || x.data_fine == null))
                                   .Select(x => x.id).FirstOrDefault();
            if (IdTipologiaEsenteFerie != null)
            {
                esente = db.MyRai_InfoDipendente.Any(x => x.valore != null && x.valore.ToLower() == "true" &&
                                                          x.matricola == matricola && x.id_infodipendente_tipologia == IdTipologiaEsenteFerie && D >= x.data_inizio && D <= x.data_fine);
            }
            return esente;
        }

        /// <summary>
        /// Verifica se un utente ha una determinata Tipologia assegnata
        /// </summary>
        /// <param name="tipologia"></param>
        /// <returns></returns>
        public static Boolean HasInfoDipendente(string tipologia, string matricola)
        {
            bool result = false;

            try
            {
                using (var db = new myRaiData.digiGappEntities())
                {
                    var tipologiaRow = db.MyRai_InfoDipendente_Tipologia.Where(i => i.info.Equals(tipologia, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                    if (tipologiaRow != null)
                    {
                        result = HasInfoDipendente(matricola, tipologiaRow.id, true);
                    }
                }
            }
            catch
            {
            }

            return result;
        }

        /// <summary>
        /// Verifica se un utente ha una determinata Tipologia assegnata
        /// </summary>
        /// <param name="tipologia"></param>
        /// <returns></returns>
        public static Boolean HasInfoDipendente(string matricola, int tipologia)
        {
            bool result = false;

            try
            {
                return HasInfoDipendente(matricola, tipologia, false);
            }
            catch
            {
            }

            return result;
        }
        /// <summary>
        /// Verifica se un utente ha una determinata Tipologia assegnata
        /// </summary>
        /// <param name="tipologia"></param>
        /// <param name="checkDate">Se true, controlla che la data corrente sia compresa tra la data di inizio e data fine.</param>
        /// <returns></returns>
        public static Boolean HasInfoDipendente(string matricola, int tipologia, bool checkDate = false, DateTime? D = null)
        {
            bool result = false;
            if (D == null) D = DateTime.Now;

            try
            {
                if (IsValidTipologia(matricola, tipologia))
                {
                    using (var db = new myRaiData.digiGappEntities())
                    {
                        var infoDipendente = db.MyRai_InfoDipendente.Where(i => i.id_infodipendente_tipologia == tipologia && i.matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                        if (checkDate)
                        {
                            if (infoDipendente != null)
                            {
                                DateTime min = infoDipendente.data_inizio;
                                DateTime max = infoDipendente.data_fine.HasValue ? infoDipendente.data_fine.GetValueOrDefault() : DateTime.MaxValue;

                                // DateTime currentDate = DateTime.Now;

                                if ((min <= D) && (D <= max))
                                {
                                    result = true;
                                }
                            }
                        }
                        else
                        {
                            if (infoDipendente != null)
                            {
                                result = true;
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return result;
        }

        public static Boolean IsValidTipologia(string matricola, int tipologia)
        {
            bool result = false;

            try
            {
                using (var db = new myRaiData.digiGappEntities())
                {
                    var rec = db.MyRai_InfoDipendente_Tipologia.Where(t => t.id.Equals(tipologia)).FirstOrDefault();

                    if (rec != null)
                    {
                        DateTime min = rec.data_inizio;
                        DateTime max = rec.data_fine.HasValue ? rec.data_fine.GetValueOrDefault() : DateTime.MaxValue;

                        DateTime currentDate = DateTime.Now;

                        if ((min <= currentDate) && (currentDate <= max))
                        {
                            result = true;
                        }
                    }
                }
            }
            catch
            {
            }

            return result;
        }

        /// <summary>
        /// Reperimento del valore InfoDipendente a partire dalla tipologia
        /// </summary>
        /// <param name="tipologia"></param>
        /// <returns></returns>
        public static MyRai_InfoDipendente GetInfoDipendente(string matricola, int tipologia)
        {
            MyRai_InfoDipendente result = null;

            try
            {
                using (var db = new myRaiData.digiGappEntities())
                {
                    var infoDipendente = db.MyRai_InfoDipendente.Where(i => i.id_infodipendente_tipologia == tipologia && i.matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                    if (infoDipendente != null)
                    {
                        result = new MyRai_InfoDipendente();
                        result = infoDipendente;
                    }
                }
            }
            catch
            {
            }

            return result;
        }

        public static float Tronca2Dec(float v)
        {
            if (v == (int)v)
                return v;
            else
                return (float)(((int)(v * 100)) / 100f);
        }

        public static List<string> GetSediL1(string pMatricola = "")
        {
            if (!String.IsNullOrWhiteSpace(pMatricola))
                pMatricola = GetCurrentUserMatricola();

            Abilitazioni ab = getAbilitazioni();
            return ab.ListaAbilitazioni
                    .Where(x => x.MatrLivello1.Any(z => z.Matricola == pMatricola))
                    .Select(a => a.Sede).ToList();
        }

        public static List<string> GetSediL2(string pMatricola = "")
        {
            if (!String.IsNullOrWhiteSpace(pMatricola))
                pMatricola = GetCurrentUserMatricola();

            Abilitazioni ab = CommonManager.getAbilitazioni();
            return ab.ListaAbilitazioni
                    .Where(x => x.MatrLivello2.Any(z => z.Matricola == pMatricola))
                    .Select(a => a.Sede).ToList();
        }

        public static bool AbilitatoExtPerMatricola(string matr)
        {
            //string Mrich = CommonManager.GetCurrentUserMatricola();
            // var Arich= BatchManager.GetUserData(Mrich);

            string GruppoAbilitato = GetParametro<string>(EnumParametriSistema.GruppoADceiton);

            var gruppi = Utente.ADgroups();
            bool AbilitatoAD = gruppi.Contains(GruppoAbilitato);


            //       List<string> Sedi345 = GetSediL3().Concat(GetSediL4()).Concat(GetSediL5()).ToList();
            //      if (Sedi345 == null || !Sedi345.Any()) return false;

            if (!AbilitatoAD) return false;

            var anag = BatchManager.GetUserData(matr, DateTime.Now);
            //return anag != null && !String.IsNullOrWhiteSpace(anag.sede_gapp) && Sedi345.Contains(anag.sede_gapp);

            return anag.gestito_SIRIO;
        }



        private static List<string> GetListaSediApprovatoreProduzioneInternal(string matricola, int livello = 4)
        {
            List<string> sedi = new List<string>();
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var ufficiInCuiSonoPresente = (from u in db.MyRai_UffProduzioni_Approvatori
                                                   join ap in db.MyRai_ApprovatoriProduzioni
                                                   on u.CodUfficio equals ap.MatricolaApprovatore
                                                   where u.MatricolaApprovatore == matricola &&
                                                   ap.Livello == livello
                                                   select ap.SedeGapp).ToList();

                    if (ufficiInCuiSonoPresente != null)
                    {
                        sedi.AddRange(ufficiInCuiSonoPresente.ToList());
                    }

                    var appProd = db.MyRai_ApprovatoriProduzioni.Where(w => w.MatricolaApprovatore.Equals(matricola) && w.Livello.Equals(livello)).Select(w => w.SedeGapp).ToList();

                    if (appProd != null)
                    {
                        sedi.AddRange(appProd.ToList());
                    }

                    sedi = sedi.Distinct().ToList();

                    //var listaUff = db.MyRai_UffProduzioni_Approvatori.Where( w => w.MatricolaApprovatore.Equals( matricola ) ).Select( w => w.CodUfficio ).ToList( );

                    //if (listaUff != null && listaUff.Any())
                    //{
                    //    var altreSedi = db.MyRai_Richieste.Where( w => listaUff.Contains( w.ApprovatoreSelezionato ) ).Select( w => w.codice_sede_gapp ).ToList( );
                    //    if (altreSedi != null && altreSedi.Any())
                    //    {
                    //        sedi.AddRange( altreSedi );
                    //        sedi = sedi.Distinct( ).ToList( );
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {

            }

            return sedi;
        }

        public static List<string> GetSediL3(string pMatricola)
        {
            List<string> result = new List<string>();
            Abilitazioni ab = CommonManager.getAbilitazioni();
            result = ab.ListaAbilitazioni
                    .Where(x => x.MatrLivello3.Any(z => z.Matricola == pMatricola))
                    .Select(a => a.Sede).ToList();
            return result;
        }


        public static List<string> GetSediL4()
        {
            List<string> result = new List<string>();
            string matr = GetCurrentUserPMatricola();

            var approvatoriL4Abilitato = CommonManager.GetParametro<bool>(EnumParametriSistema.AbilitaApprovatoriL4L5);
            var AbilitaSceltaApprovatore = CommonManager.GetParametro<bool>(EnumParametriSistema.AbilitaSceltaApprovatore);

            if (approvatoriL4Abilitato && AbilitaSceltaApprovatore)
            {
                // se AbilitaSceltaApprovatore è true allora è in vigore la nuova modalità, cioè quella con la quale è l'utente che fa
                // la richiesta a selezionare il suo approvatore
                result.AddRange(GetListaSediApprovatoreProduzioneInternal(CommonManager.GetCurrentUserMatricola(), 4));
            }
            else if (approvatoriL4Abilitato && !AbilitaSceltaApprovatore)
            {
                // se non è attiva la modalità AbilitaSceltaApprovatore, gli approvatori di liv4 li prende da HRGA
                Abilitazioni ab = CommonManager.getAbilitazioni();
                result = ab.ListaAbilitazioni
                        .Where(x => x.MatrLivello4.Any(z => z.Matricola == matr))
                        .Select(a => a.Sede).ToList();
            }

            return result;
        }

        public static List<string> GetSediL5()
        {
            List<string> result = new List<string>();
            string matr = CommonManager.GetCurrentUserPMatricola();

            var approvatoriL5Abilitato = CommonManager.GetParametro<bool>(EnumParametriSistema.AbilitaApprovatoriL4L5);
            var AbilitaSceltaApprovatore = CommonManager.GetParametro<bool>(EnumParametriSistema.AbilitaSceltaApprovatore);

            if (approvatoriL5Abilitato && AbilitaSceltaApprovatore)
            {
                // se AbilitaSceltaApprovatore è true allora è in vigore la nuova modalità, cioè quella con la quale è l'utente che fa
                // la richiesta a selezionare il suo approvatore
                result.AddRange(GetListaSediApprovatoreProduzioneInternal(CommonManager.GetCurrentUserMatricola(), 5));
            }
            else if (approvatoriL5Abilitato && !AbilitaSceltaApprovatore)
            {
                // se non è attiva la modalità AbilitaSceltaApprovatore, gli approvatori di liv4 li prende da HRGA
                Abilitazioni ab = CommonManager.getAbilitazioni();
                result = ab.ListaAbilitazioni
                        .Where(x => x.MatrLivello4.Any(z => z.Matricola == matr))
                        .Select(a => a.Sede).ToList();
            }

            return result;
        }

        public static List<string> GetSediL6()
        {
            string matr = CommonManager.GetCurrentUserPMatricola();
            Abilitazioni ab = CommonManager.getAbilitazioni();
            var result = ab.ListaAbilitazioni
                    .Where(x => x.MatrLivello6.Any(z => z.Matricola == matr))
                    .Select(a => a.Sede).ToList();
            //return new List<string>() { "8LC00"};
            return result;
        }

        public static List<string> GetSediLApprovatoreProduzione()
        {
            List<string> result = new List<string>();
            string matr = CommonManager.GetCurrentUserPMatricola();
            Abilitazioni ab = CommonManager.getAbilitazioni();
            result.AddRange(GetSediL3(matr));
            result.AddRange(GetSediL4());
            result.AddRange(GetSediL5());

            return result;
        }

        public static bool L1propriaSede()
        {
            string rep = "";
            if (Utente.Reparto() != null && Utente.Reparto().Trim() != "" && Utente.Reparto() != "00") rep = Utente.Reparto();

            if (Utente.IsBoss(GetCurrentUserPMatricola()))
                return GetSediL1(GetCurrentUserPMatricola()).Contains(Utente.SedeGapp(DateTime.Now) + rep);
            else
                return false;
        }

        public static Boolean L2propriaSede()
        {
            if (Utente.IsBossLiv2(GetCurrentUserPMatricola()))
                return GetSediL2(GetCurrentUserPMatricola()).Contains(Utente.SedeGapp(DateTime.Now));
            else
                return false;
        }

        public static void marca()
        {
            var db = new digiGappEntities();
            Abilitazioni AB = getAbilitazioni();
            var list = db.MyRai_Richieste.Where(x => x.id_stato == 10).ToList();
            foreach (var r in list)
            {
                var sede = AB.ListaAbilitazioni.Where(x => x.Sede == r.codice_sede_gapp).FirstOrDefault();
                if (sede != null && sede.MatrLivello1.Any(a => a.Matricola == "P" + r.matricola_richiesta))
                {
                    r.richiedente_L1 = true;
                }
                if (sede != null && sede.MatrLivello2.Any(a => a.Matricola == r.matricola_richiesta))
                {
                    r.richiedente_L2 = true;
                }
            }
            db.SaveChanges();
        }

        public static Abilitazioni getAbilitazioni()
        {
            if (SessionHelper.Get("abilitazioni") == null)
            {
                Sedi service = new Sedi();
                service.Credentials = CommonManager.GetUtenteServizioCredentials();

                CategorieDatoAbilitate response = Get_CategoriaDato_Net_Cached(0);

                Abilitazioni AB = new Abilitazioni();

                foreach (var item in response.CategorieDatoAbilitate_Array)
                {
                    AbilitazioneSede absede = new AbilitazioneSede()
                    {
                        Sede = item.Codice_categoria_dato,
                        DescrSede = item.Descrizione_categoria_dato
                    };
                    foreach (System.Data.DataRow row in item.DT_Utenti_CategorieDatoAbilitate.Rows)
                    {
                        if (row["codice_sottofunzione"].ToString() == "01GEST")
                        {
                            MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                            ms.Matricola = row["logon_id"].ToString();
                            ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                            ms.Delegante = row["Delegante"].ToString();
                            ms.Delegato = row["Delegato"].ToString();
                            absede.MatrLivello1.Add(ms);
                        }

                        if (row["codice_sottofunzione"].ToString() == "02GEST")
                        {
                            MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                            ms.Matricola = row["logon_id"].ToString();
                            ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                            ms.Delegante = row["Delegante"].ToString();
                            ms.Delegato = row["Delegato"].ToString();
                            absede.MatrLivello2.Add(ms);
                        }

                        if (row["codice_sottofunzione"].ToString() == "03GEST")
                        {
                            MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                            ms.Matricola = row["logon_id"].ToString();
                            ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                            ms.Delegante = row["Delegante"].ToString();
                            ms.Delegato = row["Delegato"].ToString();
                            absede.MatrLivello3.Add(ms);
                        }

                        if (row["codice_sottofunzione"].ToString() == "04GEST")
                        {
                            MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                            ms.Matricola = row["logon_id"].ToString();
                            ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                            ms.Delegante = row["Delegante"].ToString();
                            ms.Delegato = row["Delegato"].ToString();
                            absede.MatrLivello4.Add(ms);
                        }

                        if (row["codice_sottofunzione"].ToString() == "05GEST")
                        {
                            MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                            ms.Matricola = row["logon_id"].ToString();
                            ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                            ms.Delegante = row["Delegante"].ToString();
                            ms.Delegato = row["Delegato"].ToString();
                            absede.MatrLivello5.Add(ms);
                        }

                        if (row["codice_sottofunzione"].ToString() == "06GEST")
                        {
                            MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                            ms.Matricola = row["logon_id"].ToString();
                            ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                            ms.Delegante = row["Delegante"].ToString();
                            ms.Delegato = row["Delegato"].ToString();
                            absede.MatrLivello6.Add(ms);
                        }
                    }
                    AB.ListaAbilitazioni.Add(absede);
                }
                AB.ListaAbilitazioni = AB.ListaAbilitazioni.OrderBy(x => x.Sede).ToList();

                SessionHelper.Set("abilitazioni", AB);
                return AB;
            }
            else
                return (Abilitazioni)SessionHelper.Get("abilitazioni");

        }
        public static CategorieDatoAbilitate Get_CategoriaDato_Net_Cached_Liv00()
        {
            string[] responseDB;
            responseDB = GetParametri<string>(EnumParametriSistema.GetCategoriaDatoNetCachedL00);
            CategorieDatoAbilitate response =
                      Newtonsoft.Json.JsonConvert.DeserializeObject<CategorieDatoAbilitate>(responseDB[0]);
            return response;
        }
        public static CategorieDatoAbilitate Get_CategoriaDato_Net_Cached_Liv20()
        {
            string[] responseDB;
            responseDB = GetParametri<string>(EnumParametriSistema.GetCategoriaDatoNetCachedL20);
            CategorieDatoAbilitate response =
                      Newtonsoft.Json.JsonConvert.DeserializeObject<CategorieDatoAbilitate>(responseDB[0]);
            return response;
        }
        public static CategorieDatoAbilitate Get_CategoriaDato_Net_Cached(int Liv, bool isBatch = false, string matricola = null, bool resetcache = false)
        {
            string[] responseDB;

            if (Liv == 1)
            {
                responseDB = GetParametri<string>(EnumParametriSistema.GetCategoriaDatoNetCached);
                if (
                       resetcache ||
                       String.IsNullOrWhiteSpace(responseDB[0])
                   //|| responseDB[1] != DateTime.Today.ToString("dd/MM/yyyy")
                   )
                {
                    Sedi service = new Sedi();
                    if (resetcache) service.Timeout = 500000;
                    CategorieDatoAbilitate response = service.Get_CategoriaDato_Net_RaiPerMe("sedegapp", "", "HRUP", "01GEST");
                    if (response.Cod_Errore == "0")
                    {
                        var db = new digiGappEntities();
                        string chiave = EnumParametriSistema.GetCategoriaDatoNetCached.ToString();
                        var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).FirstOrDefault();
                        if (par != null)
                        {
                            par.Valore1 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                            par.Valore2 = DateTime.Today.ToString("dd/MM/yyyy");

                            if (isBatch)
                                DBHelper.SaveNoSession(db, matricola);
                            else
                                DBHelper.Save(db, matricola);
                        }
                    }

                    return response;
                }
                else
                {
                    CategorieDatoAbilitate response =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<CategorieDatoAbilitate>(responseDB[0]);

                    return response;
                }
            }
            else if (Liv == 2)
            {
                responseDB = GetParametri<string>(EnumParametriSistema.GetCategoriaDatoNetCachedL2);
                if (
                     resetcache || String.IsNullOrWhiteSpace(responseDB[0])
                    //  || responseDB[1] != DateTime.Today.ToString("dd/MM/yyyy")
                    )
                {
                    Sedi service = new Sedi();
                    if (resetcache) service.Timeout = 500000;
                    CategorieDatoAbilitate response =
                        service.Get_CategoriaDato_Net_RaiPerMe("sedegapp", "", "HRUP", "02GEST");
                    if (response.Cod_Errore == "0")
                    {
                        var db = new digiGappEntities();
                        string chiave = EnumParametriSistema.GetCategoriaDatoNetCachedL2.ToString();
                        var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).FirstOrDefault();
                        if (par != null)
                        {
                            par.Valore1 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                            par.Valore2 = DateTime.Today.ToString("dd/MM/yyyy");
                            if (isBatch)
                                DBHelper.SaveNoSession(db, matricola);
                            else
                                DBHelper.Save(db, matricola);
                        }
                    }

                    return response;
                }
                else
                {
                    CategorieDatoAbilitate response =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<CategorieDatoAbilitate>(responseDB[0]);

                    return response;
                }
            }
            else if (Liv == 3)
            {
                responseDB = GetParametri<string>(EnumParametriSistema.GetCategoriaDatoNetCachedL3);
                if (
                     resetcache || String.IsNullOrWhiteSpace(responseDB[0])
                    //  || responseDB[1] != DateTime.Today.ToString("dd/MM/yyyy")
                    )
                {
                    Sedi service = new Sedi();
                    if (resetcache) service.Timeout = 500000;
                    CategorieDatoAbilitate response =
                        service.Get_CategoriaDato_Net_RaiPerMe("sedegapp", "", "HRUP", "03GEST");
                    if (response.Cod_Errore == "0")
                    {
                        var db = new digiGappEntities();
                        string chiave = EnumParametriSistema.GetCategoriaDatoNetCachedL3.ToString();
                        var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).FirstOrDefault();
                        if (par != null)
                        {
                            par.Valore1 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                            par.Valore2 = DateTime.Today.ToString("dd/MM/yyyy");
                            if (isBatch)
                                DBHelper.SaveNoSession(db, matricola);
                            else
                                DBHelper.Save(db, matricola);
                        }
                    }

                    return response;
                }
                else
                {
                    CategorieDatoAbilitate response =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<CategorieDatoAbilitate>(responseDB[0]);

                    return response;
                }
            }
            else if (Liv == 4)
            {
                responseDB = GetParametri<string>(EnumParametriSistema.GetCategoriaDatoNetCachedL4);
                ;
                if (
                     resetcache || String.IsNullOrWhiteSpace(responseDB[0])
                    //   || responseDB[1] != DateTime.Today.ToString("dd/MM/yyyy")
                    )
                {
                    Sedi service = new Sedi();
                    if (resetcache)
                        service.Timeout = 500000;
                    CategorieDatoAbilitate response =
                        service.Get_CategoriaDato_Net_RaiPerMe("sedegapp", "", "HRUP", "04GEST");
                    if (response.Cod_Errore == "0")
                    {
                        var db = new digiGappEntities();
                        string chiave = EnumParametriSistema.GetCategoriaDatoNetCachedL4.ToString();
                        var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).FirstOrDefault();
                        if (par != null)
                        {
                            par.Valore1 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                            par.Valore2 = DateTime.Today.ToString("dd/MM/yyyy");
                            if (isBatch)
                                DBHelper.SaveNoSession(db, matricola);
                            else
                                DBHelper.Save(db, matricola);
                        }
                    }

                    return response;
                }
                else
                {
                    CategorieDatoAbilitate response =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<CategorieDatoAbilitate>(responseDB[0]);

                    return response;
                }
            }
            else if (Liv == 5)
            {
                responseDB = GetParametri<string>(EnumParametriSistema.GetCategoriaDatoNetCachedL5);
                if (
                     resetcache || String.IsNullOrWhiteSpace(responseDB[0])
                    //  || responseDB[1] != DateTime.Today.ToString("dd/MM/yyyy")
                    )
                {
                    Sedi service = new Sedi();
                    if (resetcache)
                        service.Timeout = 500000;
                    CategorieDatoAbilitate response =
                        service.Get_CategoriaDato_Net_RaiPerMe("sedegapp", "", "HRUP", "05GEST");
                    if (response.Cod_Errore == "0")
                    {
                        var db = new digiGappEntities();
                        string chiave = EnumParametriSistema.GetCategoriaDatoNetCachedL5.ToString();
                        var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).FirstOrDefault();
                        if (par != null)
                        {
                            par.Valore1 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                            par.Valore2 = DateTime.Today.ToString("dd/MM/yyyy");
                            if (isBatch)
                                DBHelper.SaveNoSession(db, matricola);
                            else
                                DBHelper.Save(db, matricola);
                        }
                    }

                    return response;
                }
                else
                {
                    CategorieDatoAbilitate response =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<CategorieDatoAbilitate>(responseDB[0]);

                    return response;
                }
            }
            else  //senza livello
            {
                responseDB = GetParametri<string>(EnumParametriSistema.GetCategoriaDatoNetCachedNolevel);
                if (
                      resetcache || String.IsNullOrWhiteSpace(responseDB[0])
                    //  || responseDB[1] != DateTime.Today.ToString("dd/MM/yyyy")
                    )
                {
                    Sedi service = new Sedi();
                    if (resetcache) service.Timeout = 500000;
                    CategorieDatoAbilitate response =
                        service.Get_CategoriaDato_Net_RaiPerMe("sedegapp", "", "HRUP", "01GEST|02GEST|03GEST|04GEST|05GEST|06GEST");

                    if (response.Cod_Errore == "0")
                    {
                        var db = new digiGappEntities();
                        string chiave = EnumParametriSistema.GetCategoriaDatoNetCachedNolevel.ToString();
                        var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).FirstOrDefault();
                        if (par != null)
                        {
                            par.Valore1 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                            par.Valore2 = DateTime.Today.ToString("dd/MM/yyyy");
                            if (isBatch)
                                DBHelper.SaveNoSession(db, matricola);
                            else
                                DBHelper.Save(db, matricola);
                        }
                    }

                    return response;
                }
                else
                {
                    CategorieDatoAbilitate response =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<CategorieDatoAbilitate>(responseDB[0]);

                    return response;
                }
            }
        }

        public static Abilitazioni getAbilitazioniForBatch(string matricola)
        {
            Sedi service = new Sedi();
            service.Credentials = GetUtenteServizioCredentials();

            CategorieDatoAbilitate response =
Get_CategoriaDato_Net_Cached(0, true, matricola);

            Abilitazioni AB = new Abilitazioni();

            foreach (var item in response.CategorieDatoAbilitate_Array)
            {
                AbilitazioneSede absede = new AbilitazioneSede()
                {
                    Sede = item.Codice_categoria_dato,
                    DescrSede = item.Descrizione_categoria_dato
                };

                foreach (System.Data.DataRow row in item.DT_Utenti_CategorieDatoAbilitate.Rows)
                {
                    if (row["codice_sottofunzione"].ToString() == "01GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello1.Add(ms);
                    }

                    if (row["codice_sottofunzione"].ToString() == "02GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello2.Add(ms);
                    }

                    if (row["codice_sottofunzione"].ToString() == "03GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello3.Add(ms);
                    }

                    if (row["codice_sottofunzione"].ToString() == "04GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello4.Add(ms);
                    }

                    if (row["codice_sottofunzione"].ToString() == "05GEST")
                    {
                        MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                        ms.Matricola = row["logon_id"].ToString();
                        ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                        ms.Delegante = row["Delegante"].ToString();
                        ms.Delegato = row["Delegato"].ToString();
                        absede.MatrLivello5.Add(ms);
                    }
                }
                AB.ListaAbilitazioni.Add(absede);
            }
            AB.ListaAbilitazioni = AB.ListaAbilitazioni.OrderBy(x => x.Sede).ToList();
            return AB;
        }

        public static bool ShowElencoRichiesteFormazione(string matricola)
        {
            TalentiaEntities db = new TalentiaEntities();
            var pers = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
            int tmpElenco = 0;

            try
            {
                tmpElenco = db.TREQUESTS_STEP.Where(x => x.ID_PERSONA_FROM == pers.ID_PERSONA || x.ID_PERSONA_TO == pers.ID_PERSONA)
                    .Select(a => a.TREQUESTS).Distinct()
                    .Count(y => y.ID_PERSONA != pers.ID_PERSONA);
            }
            catch (Exception ex)
            {

            }
            return tmpElenco > 0;
        }

        public static bool IsGiornoCedolino(string matricola)
        {
            //Se non è una PMatricola non è autorizzato alla visualizzazione del cedolino
            if (matricola.Length > 6)
                return false;

            int giornoCedolino = 27;
            digiGappEntities db = new digiGappEntities();
            var paramDataCed = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "GiornoCedolino");
            if (paramDataCed != null && !String.IsNullOrWhiteSpace(paramDataCed.Valore1))
            {
                if (paramDataCed.Valore1 == "FALSE")
                    return false;

                int tmp = 0;
                if (int.TryParse(paramDataCed.Valore1, out tmp)) giornoCedolino = tmp;

                if (giornoCedolino > 0 && giornoCedolino < DateTime.Today.Day)
                {
                    paramDataCed.Valore1 = "0";
                    DBHelper.Save(db, matricola);
                    return false;
                }
            }
            else
            {
                DateTime rif = new DateTime(DateTime.Today.Year, DateTime.Today.Month, giornoCedolino);
                if (rif.DayOfWeek == DayOfWeek.Saturday)
                    giornoCedolino -= 1;
                else if (rif.DayOfWeek == DayOfWeek.Sunday)
                    giornoCedolino -= 1;
            }

            bool isGiornoCedolino = DateTime.Today.Day == giornoCedolino;

            if (isGiornoCedolino)
            {
                string dateTime = DateTime.Today.ToString("dd/MM/yyyy");
                MyRai_ParametriPersonali parametriPersonali = db.MyRai_ParametriPersonali.FirstOrDefault(x => x.matricola == matricola && x.nome_parametro == "RedirectBustaPaga");
                if (parametriPersonali != null)
                {
                    if (parametriPersonali.valore_parametro == dateTime)
                        isGiornoCedolino = false;
                    else
                    {
                        parametriPersonali.valore_parametro = dateTime;
                        DBHelper.Save(db, matricola);
                    }
                }
                else
                {
                    parametriPersonali = new MyRai_ParametriPersonali();
                    parametriPersonali.matricola = matricola;
                    parametriPersonali.nome_parametro = "RedirectBustaPaga";
                    parametriPersonali.valore_parametro = dateTime;
                    db.MyRai_ParametriPersonali.Add(parametriPersonali);
                    DBHelper.Save(db, matricola);
                }
            }

            return isGiornoCedolino;
        }

        public static bool IsGiornoDocAmm()
        {
            string matricola = GetCurrentUserMatricola();

            //Se non è una PMatricola non è autorizzato alla visualizzazione del cedolino
            if (matricola.Length > 6)
                return false;

            int giornoDocAmm = 0;
            digiGappEntities db = new digiGappEntities();
            var paramDataDocAmm = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "GiornoDocAmministrativi");
            if (paramDataDocAmm != null && !String.IsNullOrWhiteSpace(paramDataDocAmm.Valore1))
            {
                if (paramDataDocAmm.Valore1 == "FALSE")
                    return false;

                int tmp = 0;
                if (int.TryParse(paramDataDocAmm.Valore1, out tmp)) giornoDocAmm = tmp;

                if (giornoDocAmm > 0 && giornoDocAmm < DateTime.Today.Day)
                {
                    paramDataDocAmm.Valore1 = "0";
                    DBHelper.Save(db, matricola);
                    return false;
                }
            }
            else
                return false;

            bool isGiornoDocAmm = DateTime.Today.Day == giornoDocAmm;

            if (isGiornoDocAmm)
            {
                string dateTime = DateTime.Today.ToString("dd/MM/yyyy");
                MyRai_ParametriPersonali parametriPersonali = db.MyRai_ParametriPersonali.FirstOrDefault(x => x.matricola == matricola && x.nome_parametro == "RedirectDocAmministrativo");
                if (parametriPersonali != null)
                {
                    if (parametriPersonali.valore_parametro == dateTime)
                        isGiornoDocAmm = false;
                    else
                    {
                        parametriPersonali.valore_parametro = dateTime;
                        DBHelper.Save(db, matricola);
                    }
                }
                else
                {
                    parametriPersonali = new MyRai_ParametriPersonali();
                    parametriPersonali.matricola = matricola;
                    parametriPersonali.nome_parametro = "RedirectDocAmministrativo";
                    parametriPersonali.valore_parametro = dateTime;
                    db.MyRai_ParametriPersonali.Add(parametriPersonali);
                    DBHelper.Save(db, matricola);
                }

                //Se capita che nello stesso giorno ci sia anche un redirect per la busta paga, vince quest'ultima. 
                //Il giro però deve rimanere ugualee, ovvero scrive anche l'avvenuto redirect sul documento.
                if (isGiornoDocAmm && db.MyRai_ParametriPersonali.Any(x => x.matricola == matricola && x.nome_parametro == "RedirectBustaPaga" && x.valore_parametro == dateTime))
                    isGiornoDocAmm = false;
            }

            return isGiornoDocAmm;
        }

        public static bool IsEsamiEnabled()
        {
            bool result = false;

            if (HttpContext.Current.Session["EsamiEnabled"] == null)
            {
                try
                {
                    string matricola = CommonManager.GetCurrentUserMatricola();
                    var db = new myRaiData.Incentivi.IncentiviEntities();
                    result = db.XR_EXAM_ABIL.Any(x => x.MATRICOLA == matricola && (x.IND_RICHIESTE || x.IND_APPUNTAMENTO || x.IND_EXPORT));
                }
                catch (Exception ex)
                {
                    result = false;
                }

                HttpContext.Current.Session["EsamiEnabled"] = result;
            }
            else
            {
                result = (bool)(HttpContext.Current.Session["EsamiEnabled"]);
            }

            return result;
        }
        public static string GetAppSettings(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }

        public static string GetFilePath(string path)
        {
            string webContext = CommonManager.GetAppSettings("WebContext");
            if (webContext == "false")
                return System.IO.Path.GetFileName(path);
            else
                return HttpContext.Current.Server.MapPath(path);
        }

        public static bool CheckForVPNInterface()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface Interface in interfaces)
                {
                    if (Interface.Description.Contains("Juniper Networks Virtual Adapter")
                      && Interface.OperationalStatus == OperationalStatus.Up)
                    {
                        IPInterfaceProperties prop = Interface.GetIPProperties();
                        if (prop != null && prop.DnsSuffix.Contains(".rai.it"))
                            return true;
                    }
                }
            }
            return false;
        }

        #region CheckAuth
        public static bool IsHrisEnabledAnySubFunc(string funzione, params string[] subFuncs)
        {
            ApplicationType applicationType = GetApplicationType();
            if (applicationType != ApplicationType.Gestionale)
                return false;

            string matricola = CommonHelper.GetCurrentUserMatricola();
            return AuthHelper.EnabledToAnySubFunc(matricola, funzione, subFuncs);
        }
        #endregion
    }
    public class Abilitazioni
    {
        public Abilitazioni()
        {
            ListaAbilitazioni = new List<AbilitazioneSede>();
        }
        public List<AbilitazioneSede> ListaAbilitazioni { get; set; }
    }
    public class AbilitazioneSede
    {
        public AbilitazioneSede()
        {
            MatrLivello1 = new List<MatrSedeAppartenenza>();
            MatrLivello2 = new List<MatrSedeAppartenenza>();
            MatrLivello3 = new List<MatrSedeAppartenenza>();
            MatrLivello4 = new List<MatrSedeAppartenenza>();
            MatrLivello5 = new List<MatrSedeAppartenenza>();
            MatrLivello6 = new List<MatrSedeAppartenenza>();
        }
        public string Sede { get; set; }
        public string DescrSede { get; set; }
        public List<MatrSedeAppartenenza> MatrLivello1 { get; set; }
        public List<MatrSedeAppartenenza> MatrLivello2 { get; set; }

        /// <summary>
        /// Livello 3 è in realtà un livello 1 con codice 03GEST. Approvatori per produzione
        /// </summary>
        public List<MatrSedeAppartenenza> MatrLivello3 { get; set; }
        public List<MatrSedeAppartenenza> MatrLivello4 { get; set; }
        public List<MatrSedeAppartenenza> MatrLivello5 { get; set; }
        public List<MatrSedeAppartenenza> MatrLivello6 { get; set; }
    }
    public class MatrSedeAppartenenza
    {
        public string Matricola { get; set; }
        public string SedeAppartenenza { get; set; }
        public string Delegante { get; set; }
        public string Delegato { get; set; }
    }
    public class ImageInfo
    {
        public byte[] image { get; set; }
        public string ext { get; set; }
    }

    public class Sede
    {
        public string CodiceSede { get; set; }
        public string DescrizioneSede { get; set; }
        public List<RepartoLinkedServer> RepartiSpecifici { get; set; }
        public IEnumerable<PeriodoPDF> PeriodiPDF { get; set; }
        public List<MyRai_PianoFerieSedi> ListaPianiFerie { get; set; }
    }

    public class PeriodoPDF
    {
        public long idPdf { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public int Versione { get; set; }
        public DateTime? Data_generazione { get; set; }
        public string codSede { get; set; }
        public string descSede { get; set; }
        public DateTime? Data_letto { get; set; }
        public string tipoPDF { get; set; }
        public string statoPDF { get; set; }
        public bool inFirma { get; set; }
    }
}
