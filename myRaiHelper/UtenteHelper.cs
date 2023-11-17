using myRaiData;
using myRaiServiceHub.Autorizzazioni;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using Utente = myRaiHelper.UtenteHelper;
using CommonManager = myRaiHelper.CommonHelper;
using System.Diagnostics;
using myRaiDataTalentia;
using myRaiData.Incentivi;

namespace myRaiHelper
{
    public class UtenteHelper
    {
        public static myRaiData.Incentivi.SINTESI1 SintesiInfo(string matricola)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();

            return db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matricola).FirstOrDefault();
        }

        public static WeekPlan GetCeitonWeekPlan()
        {
            return CeitonHelper.GetCeitonWeekPlan();
        }

        public static bool AbilitatoAdAssistente()
        {
            var db = new digiGappEntities();
            string matr = CommonManager.GetParametro<string>(EnumParametriSistema.AssistenteMatricole);
            if (!String.IsNullOrWhiteSpace(matr))
            {
                return matr.Split(',').Contains(CommonManager.GetCurrentUserMatricola());
            }
            string tipiDip = CommonManager.GetParametro<string>(EnumParametriSistema.AssistenteTipiDip);
            if (!String.IsNullOrWhiteSpace(tipiDip))
            {
                return tipiDip.Contains(Utente.TipoDipendente());
            }
            else
                return false;

        }

        public static bool IsSedeGappTerritoriale()
        {
            string par = CommonManager.GetParametro<string>(EnumParametriSistema.SediGappTerritoriali);
            if (String.IsNullOrWhiteSpace(par))
                return false;
            else
                return par.Split(',').Contains(Utente.SedeGapp());
        }

        public static bool AssistenteAbilitato()
        {
            var db = new digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();
            return db.MyRai_Date_Assistenti.Any(x => x.matricola == matr && x.attivo == true);
        }

        public static Boolean HaPatenteB()
        {
            var infoDipendente = CommonManager.GetInfoDipendente(CommonManager.GetCurrentUserMatricola(), 14);
            return (infoDipendente != null && infoDipendente.valore != null && infoDipendente.valore.Trim().ToLower() == "true" && infoDipendente.data_inizio < DateTime.Now && (infoDipendente.data_fine == null || infoDipendente.data_fine > DateTime.Now));
        }
        public static Boolean HaPatenteC()
        {
            var infoDipendente = CommonManager.GetInfoDipendente(CommonManager.GetCurrentUserMatricola(), 10);
            return (infoDipendente != null && infoDipendente.valore != null && infoDipendente.valore.Trim().ToLower() == "true" && infoDipendente.data_inizio < DateTime.Now && (infoDipendente.data_fine == null || infoDipendente.data_fine > DateTime.Now));
        }

        public static Boolean IsTurnista()
        {
            var db = new digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();
            var info = db.MyRai_InfoDipendente_Tipologia.Where(x => x.info == "Turnista").FirstOrDefault();
            return db.MyRai_InfoDipendente.Any(x =>
            x.id_infodipendente_tipologia == info.id &&
            x.valore == "true" &&
            x.matricola == matr &&
            x.data_inizio < DateTime.Now &&
            (x.data_fine == null || x.data_fine > DateTime.Now));
        }

        public static Boolean IsTester(string matricola = "")
        {
            if (String.IsNullOrWhiteSpace(matricola))
                matricola = CommonManager.GetCurrentUserMatricola();

            string matr = CommonManager.GetParametro<string>(EnumParametriSistema.MatricoleTester);
            //if (CommonManager.GetCurrentRealUsername() == "EAM0708") return true;

            if (String.IsNullOrWhiteSpace(matr)) return false;
            if (matr == "*") return true;

            string[] MatricoleTester = matr.Split(',').Select(a => a.Trim()).ToArray();
            return MatricoleTester.Contains(matricola);
        }

        public static Boolean IsAdmin(string matricola = "")
        {
            if (String.IsNullOrWhiteSpace(matricola))
                matricola = CommonManager.GetCurrentUserMatricola();

            string matr = CommonManager.GetParametro<string>(EnumParametriSistema.MatricoleAdmin);
            if (String.IsNullOrWhiteSpace(matr)) return false;

            string[] MatricoleAdministrator = matr.Split(',').Select(a => a.Trim()).ToArray();

            return MatricoleAdministrator.Contains(matricola)
                || (
                    System.Security.Principal.WindowsIdentity.GetCurrent() != null
                    && System.Security.Principal.WindowsIdentity.GetCurrent().Name != null
                    && System.Security.Principal.WindowsIdentity.GetCurrent().Name.Contains("\\")
                    && MatricoleAdministrator.Contains(
                      System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1]
                    )
                );
        }

        public static Quadratura? GetQuadratura()
        {
            string tipoDipendente = TipoDipendente();

            string tipi = CommonManager.GetParametro<String>(EnumParametriSistema.TipiDipQuadraturaSettimanale);
            if (tipi != null && tipoDipendente != null && tipi.ToUpper().Contains(tipoDipendente.ToUpper()))
                return Quadratura.Settimanale;

            string tipiGiorn = CommonManager.GetParametro<String>(EnumParametriSistema.TipiDipQuadraturaGiornaliera);
            if (tipiGiorn != null && tipoDipendente != null && tipiGiorn.ToUpper().Contains(tipoDipendente.ToUpper()))
                return Quadratura.Giornaliera;

            return null;

        }

        private static GetAnalisiEccezioniResponse GetAnalisiEccAllaDataAnnoPassato(DateTime date)
        {
            MyRaiService1Client wcf1 = new MyRaiService1Client();
            wcf1.ClientCredentials.Windows.ClientCredential = CommonManager.GetUtenteServizioCredentials();
            GetAnalisiEccezioniResponse response = wcf1.GetAnalisiEccezioni(CommonManager.GetCurrentUserMatricola(),
                                                                                new DateTime(date.Year, 1, 1),
                                                                                date,
                                                                                "POH",
                                                                                "ROH",
                                                                                null
                                                                                );
            return response;
        }

        private static GetAnalisiEccezioniResponse GetAnalisiEcc()
        {
            try
            {
                MyRaiService1Client wcf1 = new MyRaiService1Client();
                wcf1.ClientCredentials.Windows.ClientCredential = CommonManager.GetUtenteServizioCredentials();
                int? anno = (int?)SessionHelper.Get(SessionVariables.AnnoFeriePermessi);
                if (anno == null)
                {
                    GetAnalisiEccezioniResponse response = wcf1.GetAnalisiEccezioni(CommonManager.GetCurrentUserMatricola(),
                                                                                new DateTime(DateTime.Now.Year, 1, 1),
                                                                                DateTime.Now,
                                                                                "POH",
                                                                                "ROH",
                                                                                null
                                                                                );


                    if (response != null && response.DettagliEccezioni != null)
                    {
                        foreach (var d in response.DettagliEccezioni)
                        {
                            d.data = new DateTime(DateTime.Now.Year, d.data.Month, d.data.Day);
                        }
                    }
                    return response;
                }
                else
                {
                    GetAnalisiEccezioniResponse response = wcf1.GetAnalisiEccezioni(CommonManager.GetCurrentUserMatricola(),
                                                                                new DateTime((int)anno, 1, 1),
                                                                                new DateTime((int)anno, 12, 31),
                                                                                //DateTime.Now,
                                                                                "POH",
                                                                                "ROH",
                                                                                null
                                                                                );

                    if (response != null && response.DettagliEccezioni != null)
                    {
                        foreach (var d in response.DettagliEccezioni)
                        {
                            d.data = new DateTime((int)anno, d.data.Month, d.data.Day);
                        }
                    }
                    return response;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "Utente.GetAnalisiEcc()",
                    error_message = ex.ToString()
                });
                return null;
            }
        }
        public static GetAnalisiEccezioniResponse GetAnalisiEccezioni(Boolean ForceRefresh = false)
        {
            return GetAnalisiEcc();
        }
        public static int GetPOH(Boolean ForceRefresh = false, DateTime? FinoAdata = null)
        {
            GetAnalisiEccezioniResponse r = null;
            if (FinoAdata != null && ((DateTime)FinoAdata).Year != DateTime.Now.Year)
            {
                r = GetAnalisiEccAllaDataAnnoPassato((DateTime)FinoAdata);
            }
            else
            {
                r = GetAnalisiEcc();
            }

            int sottraiMinuti = 0;
            if (FinoAdata != null)
            {
                var successive = r.DettagliEccezioni.Where(x => x.data > FinoAdata && x.eccezione == "POH")
                    .Select(x => x.minuti).ToList();
                if (successive != null && successive.Count > 0)
                {
                    sottraiMinuti = successive.Sum();
                }
            }
            if (r != null && r.AnalisiEccezione.Length > 0 && r.AnalisiEccezione[0].totale != null)
                return Convert.ToInt32(r.AnalisiEccezione[0].totale) - sottraiMinuti;
            else
                return 0;
        }

        public static int GetROH(Boolean ForceRefresh = false, DateTime? FinoAdata = null)
        {
            GetAnalisiEccezioniResponse r = null;
            if (FinoAdata != null && ((DateTime)FinoAdata).Year != DateTime.Now.Year)
            {
                r = GetAnalisiEccAllaDataAnnoPassato((DateTime)FinoAdata);
            }
            else
            {
                r = GetAnalisiEcc();
            }

            int sottraiMinuti = 0;
            if (FinoAdata != null)
            {
                var successive = r.DettagliEccezioni.Where(x => x.data > FinoAdata && x.eccezione == "ROH")
                    .Select(x => x.minuti).ToList();
                if (successive != null && successive.Count > 0)
                {
                    sottraiMinuti = successive.Sum();
                }
            }
            if (r != null && r.AnalisiEccezione.Length > 1 && r.AnalisiEccezione[1].totale != null)
                return Convert.ToInt32(r.AnalisiEccezione[1].totale) - sottraiMinuti;
            else return 0;
        }

        public static void GetPOHandROH(Boolean ForceRefresh, DateTime? FinoAdata, out int poh, out int roh)
        {
            poh = 0;
            roh = 0;

            GetAnalisiEccezioniResponse r = null;
            if (FinoAdata != null && ((DateTime)FinoAdata).Year != DateTime.Now.Year)
            {
                r = GetAnalisiEccAllaDataAnnoPassato((DateTime)FinoAdata);
            }
            else
            {
                r = GetAnalisiEcc();
            }

            if (r != null)
            {
                int sottraiMinutiPoh = 0;
                int sottraiMinutiRoh = 0;
                if (FinoAdata != null)
                {
                    sottraiMinutiPoh = r.DettagliEccezioni.Where(x => x.data > FinoAdata && x.eccezione == "POH").Sum(y => y.minuti);
                    sottraiMinutiRoh = r.DettagliEccezioni.Where(x => x.data > FinoAdata && x.eccezione == "ROH").Sum(y => y.minuti);
                }

                if (r.AnalisiEccezione.Length > 0 && r.AnalisiEccezione[0].totale != null)
                    poh = Convert.ToInt32(r.AnalisiEccezione[0].totale) - sottraiMinutiPoh;

                if (r.AnalisiEccezione.Length > 1 && r.AnalisiEccezione[1].totale != null)
                    roh = Convert.ToInt32(r.AnalisiEccezione[1].totale) - sottraiMinutiRoh;
            }
        }

        public static List<DateTime> GetPOHdays(Boolean ForceRefresh = false)
        {
            GetAnalisiEccezioniResponse r = GetAnalisiEcc();
            if (r == null || r.DettagliEccezioni == null) return new List<DateTime>();
            return r.DettagliEccezioni.Where(x => x.eccezione == "POH").Select(x => x.data).ToList();
        }

        public static List<DateTime> GetROHdays(Boolean ForceRefresh = false)
        {
            GetAnalisiEccezioniResponse r = GetAnalisiEcc();
            if (r == null || r.DettagliEccezioni == null) return new List<DateTime>();
            return r.DettagliEccezioni.Where(x => x.eccezione == "ROH").Select(x => x.data).ToList();
        }

        public static bool GappChiuso()
        {
            string m = CommonManager.GetParametro<string>(EnumParametriSistema.BypassOrariGappMatricole);

            if (!String.IsNullOrWhiteSpace(m) && m.Split(',').Contains(CommonManager.GetCurrentUserMatricola()))
            {
                return false;
            }

            DateTime D = DateTime.Now;

            string[] valori = CommonManager.GetParametri<string>(EnumParametriSistema.OrariGapp);
            if ((Convert.ToInt32(D.Hour.ToString() + D.Minute.ToString().PadLeft(2, '0')) > Convert.ToInt32(valori[0]))
           && (Convert.ToInt32(D.Hour.ToString() + D.Minute.ToString().PadLeft(2, '0')) < Convert.ToInt32(valori[1])))
            {
                return IsAnagraficaFromCICS() == false;
            }
            else
            {
                return true;
            }
        }

        public static List<string> getAuthorizedControllers()
        {
            if (SessionHelper.Get("authorizedControllers") == null)
            {
                sidebarModel s = getSidebarModel();

                List<string> Lcontrollers = s.sezioni.Where(z =>
                            !String.IsNullOrWhiteSpace(z.customView)).Select(
                                    item =>
                                           item.customView.Trim(new Char[] { ' ', '/' }).Contains("/")
                                              ? item.customView.Trim(new Char[] { ' ', '/' }).Split('/')[0].ToLower()
                                                    : item.customView.Trim(new Char[] { ' ', '/' }).ToLower()
                                          ).ToList();
                SessionHelper.Set("authorizedControllers", Lcontrollers);
            }

            return SessionHelper.Get("authorizedControllers") as List<string>;
        }

        public static sidebarModel getSidebarModel(Boolean ForceRefresh = false)
        {
            if (ForceRefresh || SessionHelper.Get("sidebar") == null)
            {
                if (ForceRefresh) SessionHelper.Set("authorizedControllers", null);

                sidebarModel model = CommonManager.getSidebarModel();
                SessionHelper.Set("sidebar", model);
            }
            return SessionHelper.Get("sidebar") as sidebarModel;
        }

        public static sidebarModel getAcademySidebarModel(Boolean ForceRefresh = false)
        {
            if (ForceRefresh || SessionHelper.Get("sidebarAcademy") == null)
            {
                if (ForceRefresh) SessionHelper.Set("authorizedControllers", null);

                sidebarModel model = CommonManager.getSidebarModel(isAcademy: true);
                SessionHelper.Set("sidebarAcademy", model);
            }
            return SessionHelper.Get("sidebarAcademy") as sidebarModel;
        }

        public static string SedeGapp()
        {
            return SedeGapp(DateTime.Now);
        }
        public static string SedeGapp(DateTime data)
        {
            if (Utente.DataInizioValidita() != null && data < Utente.DataInizioValidita())
            {
                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                wcf1.ClientCredentials.Windows.ClientCredential = CommonManager.GetUtenteServizioCredentials();

                var resp = wcf1.GetRecuperaUtente(CommonManager.GetCurrentUserMatricola7chars(), data.ToString("ddMMyyyy"));
                return resp.data.sede_gapp;
            }
            else
            {
                if (SessionHelper.Get("Utente") != null)
                {
                    wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                    if (r.data != null)
                        return r.data.sede_gapp;
                    else
                        throw new Exception("Sede gapp null");
                }
                else
                {
                    RefreshUserSession();
                    if (SessionHelper.Get("Utente") != null)
                    {
                        wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                        if (r.data != null)
                            return r.data.sede_gapp;
                        else
                            throw new Exception("Sede gapp null");
                    }
                    else
                        throw new Exception("Sede gapp null");
                }
            }
        }

        public static DateTime? DataInizioValidita()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                if (r.data != null)
                {
                    DateTime d;
                    if (DateTime.TryParseExact(r.data.data_inizio_validita, "yyyyMMdd", null,
                                System.Globalization.DateTimeStyles.None, out d))
                        return d;
                    else
                        return null;
                }
                else return null;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                if (r.data != null)
                {
                    DateTime d;
                    if (DateTime.TryParseExact(r.data.data_inizio_validita, "yyyyMMdd", null,
                                System.Globalization.DateTimeStyles.None, out d))
                        return d;
                    else
                        return null;
                }
                else return null;
            }
        }

        public static string FotoUtente()
        {
            return CommonHelper.GetUrlFoto(CommonHelper.GetCurrentUserMatricola());
            return "/home/getimg?matr=" + CommonHelper.GetCurrentUserMatricola();

            if (SessionHelper.Get("FotoUtente") != null)
            {
                return SessionHelper.Get("FotoUtente").ToString();
            }
            else
            {
                RefreshFotoUtente();
                return SessionHelper.Get("FotoUtente").ToString();
            }
        }

        public static DateTime GetDateBackPerEvidenzeDateTime()
        {
            DateTime D;
            DateTime.TryParseExact(GetDateBackPerEvidenze(), "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D);
            return D;
        }

        public static DateTime GetDataChiusuraSogliaStatica()
        {
            string parChiusura = CommonManager.GetParametro<string>(EnumParametriSistema.GiorniRiaperturaDopoSecondoGiornoC);
            int giorniAggiunti = 0;
            DateTime Dnow = DateTime.Now;

            if (!String.IsNullOrEmpty(parChiusura) && parChiusura.Trim() != "0" && int.TryParse(parChiusura, out giorniAggiunti))
            {
                DateTime? dc2 = myRaiCommonTasks.CommonTasks.GetDataChiusura2(Dnow.AddMonths(-1).Month.ToString(), Dnow.AddMonths(-1).Year.ToString(), CommonManager.GetCurrentUserMatricola(), 80);
                if (dc2 != null && Dnow > ((DateTime)dc2).AddDays(1))
                {
                    return ((DateTime)dc2).AddDays(giorniAggiunti);
                }
            }

            string[] par = CommonManager.GetParametri<string>(EnumParametriSistema.ChiusuraMese);

            var first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var last = first.AddMonths(1).AddDays(-1);

            List<DateTime> LD = new List<DateTime>();

            for (var i = first; i <= last; i = i.AddDays(1))
            {
                if (i.DayOfWeek != DayOfWeek.Saturday && i.DayOfWeek != DayOfWeek.Sunday)
                {
                    LD.Add(i);
                }
            }
            int dayClose = Convert.ToInt32(par[0]);
            int h = Convert.ToInt32(par[1].Substring(0, 2));
            int m = Convert.ToInt32(par[1].Substring(2, 2));

            DateTime Dsoglia = LD[dayClose - 1].AddHours(h).AddMinutes(m);
            return Dsoglia;
        }

        public static string GetDateBackPerEvidenze()
        {
            if (SessionHelper.Get("DatePerEvidenze") == null)
            {
                DateTime D;
                if (DateTime.Now > GetDataChiusuraSogliaStatica())
                {
                    D = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }
                else
                {
                    D = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                }
                SessionHelper.Set("DatePerEvidenze", D.ToString("ddMMyyyy"));
            }
            return SessionHelper.Get("DatePerEvidenze").ToString();
        }

        public static List<DateTime> GiornateConCarenza()
        {
            if (Utente.GetQuadratura() != Quadratura.Giornaliera) return null;

            string DateLimitBack = GetDateBackPerEvidenze();
            WSDigigapp datiBack_ws1 = new WSDigigapp();
            datiBack_ws1.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
;
            var getListaEvidenze = SessionHelper.Get(SessionVariables.ListaEvidenzeScrivania);
            monthResponseEccezione resp = new monthResponseEccezione();

            if (getListaEvidenze != null)
            {
                var sessionData = (SessionListaEvidenzeModel)getListaEvidenze;
                DateTime dtNow = DateTime.Now;

                TimeSpan diff = dtNow - sessionData.UltimaScrittura;

                if (diff.TotalSeconds <= 30)
                {
                    resp = sessionData.ListaEvidenze;
                }
                else
                {
                    resp = ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, Utente.Matricola(), DateLimitBack, DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);
                    //resp = datiBack_ws1.ListaEvidenze( Utente.Matricola() , DateLimitBack , DateTime.Now.AddDays( -1 ).ToString( "ddMMyyyy" ) , 70 );

                    SessionListaEvidenzeModel sessionModel = new SessionListaEvidenzeModel()
                    {
                        UltimaScrittura = DateTime.Now,
                        ListaEvidenze = resp
                    };

                    // set della sessione per impostare i dati della chiamata ListaEvidenze
                    SessionHelper.Set(SessionVariables.ListaEvidenzeScrivania, sessionModel);
                }
            }
            else
            {
                resp = ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, Utente.Matricola(), DateLimitBack, DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);
                // resp = datiBack_ws1.ListaEvidenze( Utente.Matricola( ) , DateLimitBack , DateTime.Now.AddDays( -1 ).ToString( "ddMMyyyy" ) , 70 );
                SessionListaEvidenzeModel sessionModel = new SessionListaEvidenzeModel()
                {
                    UltimaScrittura = DateTime.Now,
                    ListaEvidenze = resp
                };

                // set della sessione per impostare i dati della chiamata ListaEvidenze
                SessionHelper.Set(SessionVariables.ListaEvidenzeScrivania, sessionModel);

            }

            if (resp != null && resp.data != null && resp.data.giornate != null)
            {
                IEnumerable<Evidenza> evid = resp.data.giornate.Where(a => a.TipoEcc == TipoEccezione.Carenza);
                if (evid != null && evid.Any())
                    return evid.Select(x => x.data).OrderBy(x => x).ToList();
            }
            return null;
        }

        public static List<DateTime> GiornateConMaggiorPresenza()
        {
            List<DateTime> LD = new List<DateTime>();
            if (Utente.GetQuadratura() != Quadratura.Giornaliera) return LD;

            string DateLimitBack = GetDateBackPerEvidenze();
            WSDigigapp datiBack_ws1 = new WSDigigapp();
            datiBack_ws1.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
;
            var getListaEvidenze = SessionHelper.Get(SessionVariables.ListaEvidenzeScrivania);
            monthResponseEccezione resp = new monthResponseEccezione();

            if (getListaEvidenze != null)
            {
                var sessionData = (SessionListaEvidenzeModel)getListaEvidenze;
                DateTime dtNow = DateTime.Now;

                TimeSpan diff = dtNow - sessionData.UltimaScrittura;

                if (diff.TotalSeconds <= 30)
                {
                    resp = sessionData.ListaEvidenze;
                }
                else
                {
                    resp = ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, CommonManager.GetCurrentUserMatricola(), DateLimitBack, DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);
                    //resp = datiBack_ws1.ListaEvidenze( Utente.Matricola() , DateLimitBack , DateTime.Now.AddDays( -1 ).ToString( "ddMMyyyy" ) , 70 );

                    SessionListaEvidenzeModel sessionModel = new SessionListaEvidenzeModel()
                    {
                        UltimaScrittura = DateTime.Now,
                        ListaEvidenze = resp
                    };

                    // set della sessione per impostare i dati della chiamata ListaEvidenze
                    SessionHelper.Set(SessionVariables.ListaEvidenzeScrivania, sessionModel);
                }
            }
            else
            {
                resp = ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, CommonManager.GetCurrentUserMatricola(), DateLimitBack, DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);
                // resp = datiBack_ws1.ListaEvidenze( Utente.Matricola( ) , DateLimitBack , DateTime.Now.AddDays( -1 ).ToString( "ddMMyyyy" ) , 70 );
                SessionListaEvidenzeModel sessionModel = new SessionListaEvidenzeModel()
                {
                    UltimaScrittura = DateTime.Now,
                    ListaEvidenze = resp
                };

                // set della sessione per impostare i dati della chiamata ListaEvidenze
                SessionHelper.Set(SessionVariables.ListaEvidenzeScrivania, sessionModel);

            }

            if (resp != null && resp.data != null && resp.data.giornate != null)
            {
                IEnumerable<Evidenza> evid = resp.data.giornate.Where(a => a.TipoEcc == TipoEccezione.Carenza);
                if (evid != null && evid.Any())
                    return evid.Select(x => x.data).OrderBy(x => x).ToList();
            }
            return null;
        }


        public static List<DateTime> GiornateAssenteIngiustificato(string currentMatricola, Boolean ForceRefresh = false)
        {
            if (ForceRefresh || SessionHelper.Get("GiornateAssenteIngiustificato") == null)
            {
                string DateLimitBack = GetDateBackPerEvidenze();
                WSDigigapp datiBack_ws1 = new WSDigigapp();
                datiBack_ws1.Credentials = CommonManager.GetUtenteServizioCredentials();

                var getListaEvidenze = SessionHelper.Get(SessionVariables.ListaEvidenzeScrivania);
                monthResponseEccezione resp = new monthResponseEccezione();

                if (getListaEvidenze != null)
                {
                    var sessionData = (SessionListaEvidenzeModel)getListaEvidenze;
                    DateTime dtNow = DateTime.Now;

                    TimeSpan diff = dtNow - sessionData.UltimaScrittura;

                    if (diff.TotalSeconds <= 30)
                    {
                        resp = sessionData.ListaEvidenze;
                    }
                    else
                    {
                        resp = ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, currentMatricola, DateLimitBack, DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);

                        SessionListaEvidenzeModel sessionModel = new SessionListaEvidenzeModel()
                        {
                            UltimaScrittura = DateTime.Now,
                            ListaEvidenze = resp
                        };

                        // set della sessione per impostare i dati della chiamata ListaEvidenze
                        SessionHelper.Set(SessionVariables.ListaEvidenzeScrivania, sessionModel);
                    }
                }
                else
                {
                    resp = ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, currentMatricola, DateLimitBack, DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);

                    SessionListaEvidenzeModel sessionModel = new SessionListaEvidenzeModel()
                    {
                        UltimaScrittura = DateTime.Now,
                        ListaEvidenze = resp
                    };

                    // set della sessione per impostare i dati della chiamata ListaEvidenze
                    SessionHelper.Set(SessionVariables.ListaEvidenzeScrivania, sessionModel);

                }

                if (resp != null && resp.data != null)
                {
                    IEnumerable<Evidenza> evid = resp.data.giornate.Where(a =>
                       a.TipoEcc == TipoEccezione.AssenzaIngiustificata
                       && (a.timbrature == null || a.timbrature.Count() == 0)
                        );
                    if (evid == null || evid.Count() == 0)
                        SessionHelper.Set("GiornateAssenteIngiustificato", new List<DateTime>());
                    else
                        SessionHelper.Set("GiornateAssenteIngiustificato", evid.Select(x => x.data).OrderBy(x => x).ToList());
                }
                else
                    SessionHelper.Set("GiornateAssenteIngiustificato", new List<DateTime>());
            }
            return (List<DateTime>)SessionHelper.Get("GiornateAssenteIngiustificato");
        }

        public static bool FlagEvidenze()
        {
            if (!GappChiuso())
            {
                if (SessionHelper.Get("FlagEvidenze") == null)
                {
                    WSDigigapp datiBack_ws1 = new WSDigigapp();
                    datiBack_ws1.Credentials = CommonManager.GetUtenteServizioCredentials();
                    var resp = ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, CommonManager.GetCurrentUserMatricola(), "01042017", DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);

                    if (resp != null && resp.data != null && resp.data.giornate.Where(a => a.TipoEcc == TipoEccezione.AssenzaIngiustificata).Count() > 0)
                    {
                        SessionHelper.Set("FlagEvidenze", true);
                    }
                    else
                    {
                        SessionHelper.Set("FlagEvidenze", false);
                    }
                }
                return (bool)SessionHelper.Get("FlagEvidenze");
            }
            return false;
        }

        public static bool exart12inequipeex33percSpecified()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                MyRaiServiceInterface.MyRaiServiceReference1.Utente u = new MyRaiServiceInterface.MyRaiServiceReference1.Utente() { };
                MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp r = (MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.ex_art12_in_equipe_ex_33_perc;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.ex_art12_in_equipe_ex_33_perc;
            }
        }

        public static List<string> ADgroups()
        {
            if (SessionHelper.Get("ADgroups") == null)
            {
                List<string> grADpersonali = new List<string>();

                string[] parametri = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);


                if (Debugger.IsAttached && CommonHelper.CheckForVPNInterface())
                {
                    //
                }
                else
                {
                    try
                    {
                        using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "RAI", null, ContextOptions.Negotiate, parametri[0], parametri[1]))
                        {
                            UserPrincipal user = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, CommonManager.GetCurrentUserPMatricola());
                            DirectoryEntry de = (user.GetUnderlyingObject() as DirectoryEntry);

                            //  string GP = user.GetGroups().Where(a => a.Name == "UT_UTENTI_DIGIGAPP").FirstOrDefault().Name;
                            grADpersonali = user.GetGroups().Select(F => F.Name).ToList();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                SessionHelper.Set("ADgroups", grADpersonali);
            }

            return SessionHelper.Get("ADgroups") as List<string>;
        }

        public static myRaiDataTalentia.XR_STATO_RAPPORTO GetPeriodoSW(string matr)
        {
            if (matr.Length == 7) matr = matr.Substring(1);
            myRaiDataTalentia.TalentiaEntities db = new myRaiDataTalentia.TalentiaEntities();
            //var item = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matr).FirstOrDefault();
            //if (item == null) return null;

            DateTime D = DateTime.Today;
            var periodo = db.XR_STATO_RAPPORTO.Where(x => x.COD_STATO_RAPPORTO == "SW"
                            && x.DTA_INIZIO <= D && x.DTA_FINE >= D
                            && x.MATRICOLA == matr).FirstOrDefault();
            //&& x.ID_PERSONA == item.ID_PERSONA).FirstOrDefault();
            return periodo;
        }

        public static bool IsSmartWorker(DateTime D, string matricola = null)
        {
            string matr = "";

            myRaiDataTalentia.TalentiaEntities db = new myRaiDataTalentia.TalentiaEntities();

            if (!String.IsNullOrEmpty(matricola))
            {
                matr = matricola;
            }
            else
            {
                matr = CommonManager.GetCurrentUserMatricola();
            }

            string matricoleSW = CommonManager.GetParametro<string>(EnumParametriSistema.MatricoleSW);
            if (matricoleSW == "*" || matricoleSW.Split(',').Contains(matr))
            {
                //var item = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matr).FirstOrDefault();
                //if (item == null) return false;

                D = D.Date;

                //var periodi = db.XR_STATO_RAPPORTO.Where(x => x.COD_STATO_RAPPORTO == "SW" && x.ID_PERSONA == item.ID_PERSONA).ToList();
                var periodi = db.XR_STATO_RAPPORTO.Where(x => x.COD_STATO_RAPPORTO == "SW" && x.MATRICOLA == matr).ToList();
                if (periodi.Any())
                {
                    foreach (var p in periodi)
                    {
                        if (D >= p.DTA_INIZIO && D <= p.DTA_FINE)
                            return true;
                    }
                }
            }
            return false;
        }

        public static bool IsSmartWorker(DateTime inizioPeriodo, DateTime finePeriodo)
        {
            myRaiDataTalentia.TalentiaEntities db = new myRaiDataTalentia.TalentiaEntities();
            string matr = CommonManager.GetCurrentUserMatricola();

            string matricoleSW = CommonManager.GetParametro<string>(EnumParametriSistema.MatricoleSW);
            if (matricoleSW == "*" || matricoleSW.Split(',').Contains(matr))
            {
                //var item = db.SINTESI1.Where( x => x.COD_MATLIBROMAT == matr ).FirstOrDefault( );
                //if ( item == null )
                //    return false;

                inizioPeriodo = inizioPeriodo.Date;
                finePeriodo = finePeriodo.Date;
                finePeriodo = new DateTime(2020, 10, 30);

                //var periodi = db.XR_STATO_RAPPORTO.Where( x => x.COD_STATO_RAPPORTO == "SW" && x.ID_PERSONA == item.ID_PERSONA ).ToList( );
                var periodi = db.XR_STATO_RAPPORTO.Where(x => x.COD_STATO_RAPPORTO == "SW" && x.MATRICOLA == matr).ToList();
                if (periodi.Any())
                {
                    foreach (var p in periodi)
                    {
                        bool overlap = inizioPeriodo <= p.DTA_FINE && p.DTA_INIZIO <= finePeriodo;
                        if (overlap)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool HasSmartWorkingProvvisorio(string matricola, out DateTime? dataInizio, out DateTime? dataFine, out int? numGiorni)
        {
            bool hasSmartWorking = false;
            dataInizio = null;
            dataFine = null;
            numGiorni = null;

            if (String.IsNullOrWhiteSpace(matricola))
                matricola = CommonHelper.GetCurrentUserMatricola();

            myRaiDataTalentia.TalentiaEntities db = new myRaiDataTalentia.TalentiaEntities();

            var record = db.XR_STATO_RAPPORTO.Include("XR_STATO_RAPPORTO_INFO").FirstOrDefault(x => x.MATRICOLA == matricola && x.COD_STATO_RAPPORTO == "SW_P" && x.VALID_DTA_END == null && x.DTA_INIZIO > DateTime.Today);
            if (record!=null)
            {
                hasSmartWorking = true;
                dataInizio = record.DTA_INIZIO;
                dataFine = record.DTA_FINE;
                numGiorni = record.XR_STATO_RAPPORTO_INFO.Select(x => x.NUM_GIORNI_MAX).FirstOrDefault();
            }

            return hasSmartWorking;
        }

        public static bool IsSmartWorker(string matricola, DateTime inizioPeriodo, DateTime finePeriodo, out List<PeriodoRapporto> info)
        {
            bool isSmartWorker = false;
            info = null;

            if (String.IsNullOrWhiteSpace(matricola))
                matricola = CommonManager.GetCurrentUserMatricola();

            myRaiDataTalentia.TalentiaEntities db = new myRaiDataTalentia.TalentiaEntities();

            DateTime lDate = inizioPeriodo.Date;
            DateTime rDate = finePeriodo.Date;
            var periodi = db.XR_STATO_RAPPORTO.Include("XR_STATO_RAPPORTO_INFO").Where(x => x.MATRICOLA == matricola && x.COD_STATO_RAPPORTO == "SW" && x.DTA_INIZIO <= finePeriodo && inizioPeriodo <= x.DTA_FINE && x.VALID_DTA_END == null);
            isSmartWorker = periodi.Any();
            if (isSmartWorker)
            {
                var dbCzn = new IncentiviEntities();
                var tmpParam = dbCzn.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "SWAggiungiRichieste");
                bool aggiungiRich = tmpParam == null || tmpParam.COD_VALUE1 == "TRUE";

                info = new List<PeriodoRapporto>();
                foreach (var periodo in periodi)
                {
                    var rapp = new PeriodoRapporto()
                    {
                        Codice = periodo.COD_STATO_RAPPORTO,
                        Inizio = periodo.DTA_INIZIO,
                        Fine = periodo.DTA_FINE,
                        Cod_User = periodo.COD_USER
                    };

                    DateTime rif = rapp.Inizio;
                    while (rif < rapp.Fine)
                    {
                        DateTime endRif = rif.AddMonths(1).AddDays(-rif.Day);
                        if (endRif > rapp.Fine)
                            endRif = rapp.Fine;

                        var infoP = periodo.XR_STATO_RAPPORTO_INFO.FirstOrDefault(x => x.VALID_DTA_END == null && rif <= x.DTA_FINE && x.DTA_INIZIO <= endRif);
                        var rich = !aggiungiRich ? null : dbCzn.XR_MAT_RICHIESTE.FirstOrDefault(x => x.MATRICOLA == matricola && x.DATA_INIZIO_SW <= endRif && rif <= x.DATA_FINE_SW && x.ECCEZIONE == "SW" && (x.CATEGORIA == 52 || x.CATEGORIA == 54) && x.XR_WKF_OPERSTATI.Any(y => y.ID_STATO == 20) && x.GIORNI_APPROVATI != null);
                        if (infoP != null)
                        {
                            rapp.Info.Add(new PeriodoRapportoInfo()
                            {
                                Inizio = rif,
                                Fine = endRif,
                                NumeroGiorniPerMese = infoP.NUM_GIORNI_MAX.GetValueOrDefault() + infoP.NUM_GIORNI_EXTRA.GetValueOrDefault() + (rich != null ? rich.GIORNI_APPROVATI.GetValueOrDefault() : 0),
                                GiorniBase = infoP.NUM_GIORNI_MAX,
                                GiorniExtra = infoP.NUM_GIORNI_EXTRA + (rich != null ? rich.GIORNI_APPROVATI.GetValueOrDefault() : 0)
                            });
                        }
                        else
                        {
                            rapp.Info.Add(new PeriodoRapportoInfo()
                            {
                                Inizio = rif,
                                Fine = endRif,
                                NumeroGiorniPerMese = null,
                                GiorniBase = null,
                                GiorniExtra = null
                            });
                        }

                        rif = endRif.AddDays(1);
                    }

                    info.Add(rapp);
                }
            }

            return isSmartWorker;
        }

        public static bool IsAbilitatoGapp()
        {

            if (SessionHelper.Get("IsAbilitatoGapp") == null)
            {
                string matricoleForzate = CommonManager.GetParametro<string>(EnumParametriSistema.ForzaAbilitazioneGapp);
                if (matricoleForzate != null && matricoleForzate.Split(',').Contains(CommonManager.GetCurrentUserMatricola()))
                {
                    SessionHelper.Set("IsAbilitatoGapp", true);
                    return true;
                }
                //Autorizzazioni.Sedi service = new Autorizzazioni.Sedi();
                CategorieDatoAbilitate response = CommonManager.Get_CategoriaDato_Net_Cached(1);

                //service.Get_CategoriaDato_Net( "sedegapp", "", "HRUP", "01GEST" );

                List<string> list = new List<string>();
                foreach (DataRow item in response.DT_CategorieDatoAbilitate.Rows)
                {
                    if (item != null && item["cod"] != null && item["cod"].ToString() != "")
                    {
                        list.Add(item["cod"].ToString().Trim());
                    }
                }
                string rep = Utente.Reparto();
                Boolean Abilitato = false;
                if (rep != null && rep.Trim() != "" && rep.Trim() != "0" && rep.Trim() != "00")
                    Abilitato = list.Contains(Utente.SedeGapp(DateTime.Now)) || list.Contains(Utente.SedeGapp(DateTime.Now) + rep);
                else
                    Abilitato = list.Contains(Utente.SedeGapp(DateTime.Now));

                SessionHelper.Set("IsAbilitatoGapp", Abilitato);

            }
            return (bool)SessionHelper.Get("IsAbilitatoGapp");
        }
        public static string Nominativo()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.nominativo.Replace("\\", " ");
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.nominativo.Replace("\\", " ");
            }
        }
        public static string Reparto()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.CodiceReparto;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.CodiceReparto;
            }
        }

        public static string GetNomeProprio()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.NomeProprio;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.NomeProprio;
            }
        }

        public static Boolean IsAnagraficaFromCICS()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.SourceAnagrafica == AnagSource.FromCICS;
            }
            else
            {
                RefreshUserSession();

                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.SourceAnagrafica == AnagSource.FromCICS;
            }
        }

        public static string Matricola()
        {

            return CommonManager.GetCurrentUserMatricola().Trim();
        }

        public static String SedePuoRichiedere(string ecc)
        {

            string s = Utente.SedeGapp(DateTime.Now);
            var db = new digiGappEntities();
            var sede = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == s).FirstOrDefault();
            if (sede == null || sede.eccezioni_specifiche == null || sede.eccezioni_specifiche.Trim() == "")
            {
                return null;
            }

            string[] ecSpec = sede.eccezioni_specifiche.Split(',');

            if (ecSpec.Contains(ecc))
                return "";

            var item = ecSpec.Where(x => x.StartsWith(ecc + ":")).FirstOrDefault();
            if (item != null)
                return item.Split(':')[1];

            return null;
        }


        public static bool GestitoSirio()
        {

            string sediGestiteSirio = CommonManager.GetParametro<string>(EnumParametriSistema.SediEffettivamenteGestiteSirio);
            if (String.IsNullOrWhiteSpace(sediGestiteSirio)) return false;

            if (String.IsNullOrWhiteSpace(Utente.SedeGapp(DateTime.Now))) return false;

            if (!sediGestiteSirio.ToUpper().Split(',').Contains(Utente.SedeGapp(DateTime.Now).ToUpper()))
                return false;

            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.gestito_SIRIO;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.gestito_SIRIO;
            }
        }

        public static bool SelezioneAttivitaCeitonObbligatoria()
        {
            string sediConObbligoSelezioneIdAttivita = CommonManager.GetParametro<string>(EnumParametriSistema.SediConObbligoSelezioneIdAttivita);

            if (String.IsNullOrWhiteSpace(sediConObbligoSelezioneIdAttivita))
                return false;

            if (String.IsNullOrWhiteSpace(Utente.SedeGapp(DateTime.Now)))
                return false;

            if (!sediConObbligoSelezioneIdAttivita.ToUpper().Split(',').Contains(Utente.SedeGapp(DateTime.Now).ToUpper()))
                return false;

            return true;
        }

        public static bool IndennitaSirio()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.indennita_SIRIO;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.indennita_SIRIO;
            }
        }

        public static bool ExArt12inequipeex33per()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.ex_art12_in_equipe_ex_33_perc;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.ex_art12_in_equipe_ex_33_perc;
            }
        }

        public static bool DomandaQualificaSpecified()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.domanda_qualifica;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.domanda_qualifica;
            }
        }

        public static bool DomandaQualifica()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.domanda_qualifica;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.domanda_qualifica;
            }
        }

        public static bool DirittoReperibilitaSpecified()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.diritto_reperibilita;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.diritto_reperibilita;
            }
        }

        public static bool DirittoReperibilita()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.diritto_reperibilita;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.diritto_reperibilita;
            }
        }

        public static bool DiProduzioneSpecified()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.di_produzione;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.di_produzione;
            }
        }

        public static bool DiProduzione()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.di_produzione;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.di_produzione;
            }
        }

        public static string DataNascita()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.data_nascita;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.data_nascita;
            }
        }

        public static string DataFineValidita()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.data_fine_validita;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.data_fine_validita;
            }
        }

        public static string DataInizioRapportoLavorativo()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.data_inizio_rapporto_lavorativo;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.data_inizio_rapporto_lavorativo;
            }
        }
        public static DateTime? Data_Fine_Rapporto_lavorativo()
        {
            string d = DataFineRapportoLavorativo();
            if (String.IsNullOrWhiteSpace(d)) return null;
            DateTime D;
            if (DateTime.TryParseExact(d, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out D))
                return D;
            else
                return null;
        }

        public static string DataFineRapportoLavorativo()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.data_fine_rapporto_lavorativo;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.data_fine_rapporto_lavorativo;
            }
        }

        public static string CodiceInail()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.codice_inail;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.codice_inail;
            }
        }

        public static string Errore()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.errore;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.errore;
            }
        }

        public static bool Esito()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.esito;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.esito;
            }
        }

        public static bool EsitoSpecified()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.esito;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.esito;
            }
        }

        public static string Categoria()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.categoria;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.categoria;
            }
        }

        //public static Boolean IsLivello1PropriaSede ()
        //{
        //    if ( HttpContext.Current.Session["liv1ps"] == null )
        //        HttpContext.Current.Session["liv1ps"] = CommonManager.IsMatricolaLiv1_2_PerSede(
        //            CommonManager.GetCurrentUserMatricola(), "1", Utente.SedeGapp() );

        //    return ( bool )HttpContext.Current.Session["liv1ps"];
        //}
        //public static Boolean IsLivello2PropriaSede ()
        //{
        //    if ( HttpContext.Current.Session["liv2ps"] == null )
        //        HttpContext.Current.Session["liv2ps"] = CommonManager.IsMatricolaLiv1_2_PerSede(
        //            CommonManager.GetCurrentUserMatricola(), "2", Utente.SedeGapp() );

        //    return ( bool )HttpContext.Current.Session["liv2ps"];
        //}

        public static Boolean IsBoss(string pMatricola)
        {
            List<string> s = CommonManager.GetSediL1(pMatricola);
            return (s != null && s.Count > 0);
            //var AB = CommonManager.getAbilitazioni();
            //return AB.ListaAbilitazioni.Any(a => a.MatrLivello1.Select(w=>w.Matricola).Contains(CommonManager.GetCurrentUserPMatricola()));

            //if ( HttpContext.Current.Session["Utente"] != null )
            //{
            //    wApiUtilitydipendente_resp r = ( wApiUtilitydipendente_resp )HttpContext.Current.Session["Utente"];
            //    return r.data.isBoss;
            //}
            //else
            //{
            //    RefreshUserSession();
            //    if ( HttpContext.Current.Session["Utente"] != null )
            //    {
            //        wApiUtilitydipendente_resp r = ( wApiUtilitydipendente_resp )HttpContext.Current.Session["Utente"];
            //        return r.data.isBoss;
            //    }
            //    else return false;
            //}
        }

        public static Boolean IsBossLiv2(string pMatricola)
        {
            List<string> s = CommonManager.GetSediL2(pMatricola);
            return (s != null && s.Count > 0);

            //if ( ListaProfili().ProfiliArray != null )
            //    foreach ( var profilo in ListaProfili().ProfiliArray )
            //    {
            //        string codSottofunzione = profilo.DT_ProfiliFunzioni.Rows[0]["Codice_sottofunzione"].ToString();
            //        if ( codSottofunzione.Substring( 0, 2 ) == "02" && profilo.DT_CategorieDatoAbilitate.Rows.Count > 0 )
            //            return true;
            //    }
            //return false;
        }

        public static string[] SediGappAccessoFirma(string pMatricola)
        {
            return CommonManager.GetSediL2(pMatricola).ToArray();

            //List<string> ls = new List<string>();
            //if ( ListaProfili().ProfiliArray != null )
            //    foreach ( myRai.Autorizzazioni.Profilo profilo in ListaProfili().ProfiliArray )
            //    {
            //        if ( profilo.DT_ProfiliFunzioni.Rows.Count > 0 )
            //        {
            //            if ( "02" == profilo.DT_ProfiliFunzioni.Rows[0]["Codice_sottofunzione"].ToString().Substring( 0, 2 ) )
            //            {
            //                foreach ( System.Data.DataRow riga in profilo.DT_CategorieDatoAbilitate.Rows )
            //                {
            //                    ls.Add( riga["Codice_categoria_dato"].ToString() );
            //                }
            //            }
            //        }
            //    }
            //return ls.ToArray();
        }

        public static string FormaContratto()
        {
            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.forma_contratto;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.forma_contratto;
            }
        }

        public static string TipoDipendente()
        {
            var db = new digiGappEntities();
            string s = EnumParametriSistema.SovrascritturaTipoDipendente.ToString();
            string u = CommonManager.GetCurrentUsername();
            var tipo = db.MyRai_ParametriSistema.Where(x => x.Chiave == s && x.Valore1 == u).FirstOrDefault();
            if (tipo != null) return tipo.Valore2;


            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                if (r.data != null && r.data.tipo_dipendente != null && r.data.tipo_dipendente.Trim() == "H")
                    r.data.tipo_dipendente = "G";

                return r.data.tipo_dipendente;

            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");

                if (r.data != null && r.data.tipo_dipendente != null && r.data.tipo_dipendente.Trim() == "H")
                    r.data.tipo_dipendente = "G";

                return r.data.tipo_dipendente;
            }
        }

        public static Boolean GiornalistaDelleReti()
        {

            if (SessionHelper.Get("Utente") != null)
            {
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.giornalisti_delle_reti;
            }
            else
            {
                RefreshUserSession();
                wApiUtilitydipendente_resp r = (wApiUtilitydipendente_resp)SessionHelper.Get("Utente");
                return r.data.giornalisti_delle_reti;
            }
        }



        //metodo statico che restituisce l'oggetto Anagrafica
        public static Anagrafica EsponiAnagrafica()
        {
            if (SessionHelper.Get("UtenteAnagrafica") != null)
            {
                return (Anagrafica)SessionHelper.Get("UtenteAnagrafica");
            }
            else
            {
                RefreshUtenteAnagrafica();
                return (Anagrafica)SessionHelper.Get("UtenteAnagrafica");
            }
        }

        public static Anagrafica CaricaAnagrafica(string[] resp)
        {
            if ((resp != null) && (resp.Count() > 1))
            {
                Anagrafica utente = new Anagrafica();

                utente._cognome = resp[2];
                utente._nome = resp[1];
                utente._foto = CommonManager.GetImmagineBase64(resp[0]);
                utente._comuneNascita = resp[12];
                utente._contratto = resp[6];
                utente._figProfessionale = resp[8];
                utente._qualifica = resp[10];
                utente._dataNascita = (resp[11]) != null ? CommonManager.ConvertToDate(resp[11]) : Convert.ToDateTime(null);
                utente._comuneNascita = resp[12];
                utente._provinciaNascita = resp[24];
                utente._matricola = resp[0];
                utente._logo = resp[14]; //freak - nella function dbo.GERARSEZIONE il primo elemento è la società - resp[4]
                utente._dataAssunzione = (resp[3]) != null ? CommonManager.ConvertToDate(resp[3]) : Convert.ToDateTime(null);//CommonManager.ConvertToDate(resp[3]);
                                                                                                                             //campi aggiuntivi
                utente._codiceFigProf = resp[7];
                utente._codiceContratto = resp[5];
                utente._codiceQualifica = resp[9];
                utente._sedeApppartenenza = resp[17];
                utente._genere = resp[18];
                utente._cf = resp[19];
                utente._indirizzoresidenza = resp[20];
                utente._capresidenza = resp[21];
                utente._comuneresidenza = resp[22];
                utente._provinciaresidenza = resp[23];
                utente._nazionalita = resp[13];
                utente._categoria = resp[25];

                utente.SedeContabile = resp[29];
                utente.SedeContabileDescrizione = resp[30];

                using (var ctx = new myRai.Data.CurriculumVitae.cv_ModelEntities())
                {
                    var param = new SqlParameter("@param", resp[4]);

                    try
                    {
                        var tmp = ctx.Database.SqlQuery<string>("exec sp_GERARSEZIONE @param", param).ToList();
                        utente._inquadramento = tmp[0].ToString();// "RUO;Amministrazione;Sistemi del personale";//freak - nella function dbo.GERARSEZIONE dal secondo elemento è la sezione - resp[4]

                    }
                    catch (Exception)
                    {

                        // throw;
                    }
                }
                utente._statoNascita = resp[13];//"(IT)";//freak - da implementare - resp[13]
                utente._email = resp[15];
                utente._telefono = resp[16];
                utente._sezContabile = resp[27];

                return utente;
            }
            else
            {
                return null;
            }
        }

        public static Boolean IsMazzini()
        {
            return IsMazziniInternal(Matricola());
        }

        public static bool IsTorinoCavalli()
        {
            bool result = false;

            using (var sediDB = new PERSEOEntities())
            {
                string output = "";
                string query = "SELECT a.[cod_insediamento_ubicazione]" +
                                "FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] a " +
                                "where a.matricola_dp = '#MATRICOLA#' ";

                query = query.Replace("#MATRICOLA#", CommonManager.GetCurrentUserMatricola());

                output = sediDB.Database.SqlQuery<string>(query).FirstOrDefault();

                if (output != null)
                {
                    string output2 = "";
                    query = "SELECT [desc_insediamento] " +
                            "FROM[LINK_HRIS_HRLIV2].[HR_LIV2].[dbo].[L2D_INSEDIAMENTO]" +
                            "where (desc_insediamento like '%cavalli%')" +
                            "AND [cod_insediamento] = '#COD#' " +
                            "AND [Indirizzo] is not null";

                    query = query.Replace("#COD#", output);
                    output2 = sediDB.Database.SqlQuery<string>(query).FirstOrDefault();

                    if (!String.IsNullOrEmpty(output2))
                    {
                        result = true;
                    }
                }
            }

            return result;
        }
        private static bool IsMazziniInternal(string matricola)
        {
            bool result = false;

            using (var sediDB = new PERSEOEntities())
            {
                string output = "";
                string query = "SELECT a.[cod_insediamento_ubicazione]" +
                                "FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] a " +
                                "where a.matricola_dp = '#MATRICOLA#' ";

                query = query.Replace("#MATRICOLA#", matricola);

                output = sediDB.Database.SqlQuery<string>(query).FirstOrDefault();

                if (output != null)
                {
                    string output2 = "";
                    query = "SELECT [desc_insediamento] " +
                            "FROM[LINK_HRIS_HRLIV2].[HR_LIV2].[dbo].[L2D_INSEDIAMENTO]" +
                            //"where ((desc_insediamento like 'RM -%mazzini%') OR (desc_insediamento like 'RM - PASUBIO 2/4') OR (desc_insediamento like 'RM - V. COL DI LANA'))" +
                            "where (desc_insediamento like 'RM -%mazzini%')" +
                            "AND [cod_insediamento] = '#COD#' " +
                            "AND [Indirizzo] is not null";

                    query = query.Replace("#COD#", output);
                    output2 = sediDB.Database.SqlQuery<string>(query).FirstOrDefault();

                    if (!String.IsNullOrEmpty(output2))
                    {
                        result = true;
                    }
                }
            }

            return result;
        }




        #region Private Methods
        static Boolean GetLevel1Boss()
        {
            Sedi SEDI = new Sedi();
            SEDI.Credentials = CommonManager.GetUtenteServizioCredentials();
            string pmatricola = CommonManager.GetCurrentUserPMatricola();
            string strAuth = SEDI.autorizzazioni(pmatricola + ";HRUP;;;;E;0");
            string[] autorizzazioni = strAuth.Split(';');
            bool BossL1 = (autorizzazioni != null && autorizzazioni.Length > 3 &&
                                 autorizzazioni[0] == "01" && autorizzazioni[1] == "01" &&
                                 autorizzazioni[3] != null && autorizzazioni[3].ToUpper().Contains("01GEST"));
            return BossL1;
        }
        static void RefreshUserSession()
        {
            string[] valori = CommonManager.GetParametri<string>(EnumParametriSistema.OrariGapp);

            if ((Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString().PadLeft(2, '0')) > Convert.ToInt32(valori[0]))
            && (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString().PadLeft(2, '0')) < Convert.ToInt32(valori[1])))
            {
                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                wcf1.ClientCredentials.Windows.ClientCredential = CommonManager.GetUtenteServizioCredentials();

                try
                {
                    var resp = wcf1.GetRecuperaUtente(CommonManager.GetCurrentUserMatricola7chars(), DateTime.Today.ToString("ddMMyyyy"));
                    if (resp.data != null)
                        resp.data.isBoss = GetLevel1Boss();

                    SessionHelper.Set("Utente", resp);
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                SessionHelper.Set("Utente", new wApiUtilitydipendente_resp
                {
                    data = new MyRaiServiceInterface.MyRaiServiceReference1.Utente
                    {
                        matricola = CommonManager.GetCurrentUserMatricola()

                    }
                });
            }
        }
        static void RefreshFotoUtente()
        {
            SessionHelper.Set("FotoUtente", CommonManager.GetImmagineBase64(CommonManager.GetCurrentUserMatricola()));
        }
        static void RefreshUtenteAnagrafica()
        {
            try
            {
                string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(CommonManager.GetCurrentUserMatricola());

                string[] temp = str_temp.ToString().Split(';');

                if ((temp != null) && (temp.Count() > 16))
                {
                    SessionHelper.Set("UtenteAnagrafica", CaricaAnagrafica(temp));
                }
                else
                {
                    MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                    wcf1.ClientCredentials.Windows.ClientCredential = CommonManager.GetUtenteServizioCredentials();
                    MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp resp =
                       wcf1.GetRecuperaUtente(CommonManager.GetCurrentUserMatricola7chars(), DateTime.Today.ToString("ddMMyyyy"));

                    if (resp.data != null)
                    {
                        Anagrafica utente = new Anagrafica();
                        //carico l'oggetto utente con il resp
                        utente._cognome = resp.data.nominativo;
                        utente._matricola = resp.data.matricola;
                        utente._codiceFigProf = resp.data.categoria;
                        utente._dataNascita = null;
                        utente._dataAssunzione = null;
                        SessionHelper.Set("UtenteAnagrafica", utente);
                    }
                }
            }
            catch (Exception exc)
            {
                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                wcf1.ClientCredentials.Windows.ClientCredential = CommonManager.GetUtenteServizioCredentials();

                MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp resp =
                    wcf1.GetRecuperaUtente(CommonManager.GetCurrentUserMatricola7chars(), DateTime.Today.ToString("ddMMyyyy"));

                if (resp.data != null)
                {
                    Utente.Anagrafica utente = new Utente.Anagrafica();
                    //carico l'oggetto utente con il resp
                    utente._cognome = resp.data.nominativo;
                    utente._matricola = resp.data.matricola;
                    utente._codiceFigProf = resp.data.categoria;
                    utente._dataNascita = null;
                    utente._dataAssunzione = null;
                    SessionHelper.Set("UtenteAnagrafica", utente);
                }
            }
        }

        #endregion


        #region Class

        public class Anagrafica
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
            public string _provinciaNascita { get; set; }
            public string _statoNascita { get; set; }
            public string _inquadramento { get; set; }
            public string _logo { get; set; }
            //campi aggiuntivi
            public string _codiceFigProf { get; set; }
            public string _codiceContratto { get; set; }
            public string _codiceQualifica { get; set; }
            public string _email { get; set; }
            public string _telefono { get; set; }
            public string _sedeApppartenenza { get; set; }
            public string _genere { get; set; }
            public string _cf { get; set; }
            public string _indirizzoresidenza { get; set; }
            public string _capresidenza { get; set; }
            public string _comuneresidenza { get; set; }
            public string _provinciaresidenza { get; set; }
            public string _nazionalita { get; set; }
            public string _sezContabile { get; set; }
            public bool _vediPolicy { get; set; }
            public string _categoria { get; set; }
            public string SedeContabile { get; set; }
            public string SedeContabileDescrizione { get; set; }

            public Anagrafica()
            {
                this._matricola = "";
                this._cognome = "";
                this._nome = "";
                this._foto = "";
                this._contratto = "";
                this._dataAssunzione = Convert.ToDateTime(null);
                this._figProfessionale = "";
                this._qualifica = "";
                this._dataNascita = Convert.ToDateTime(null);
                this._comuneNascita = "";
                this._provinciaNascita = "";
                this._statoNascita = "";
                this._inquadramento = "";
                this._logo = "";
                //campi aggiuntivi
                this._codiceFigProf = "";
                this._codiceContratto = "";
                this._codiceQualifica = "";
                this._email = "";
                this._telefono = "";
                this._sedeApppartenenza = "";
                this._genere = "";
                this._cf = "";
                this._indirizzoresidenza = "";
                this._capresidenza = "";
                this._comuneresidenza = "";
                this._provinciaresidenza = "";
                this._nazionalita = "";
                this._sezContabile = "";
                this._vediPolicy = false;
                this._categoria = "";
            }
        }

        public class CV_DescTitoloLogo
        {
            public string DescTipoTitolo { get; set; }
            public string DescTitolo { get; set; }
            public string Logo { get; set; }
        }

        public class cv_dataAggiornamento
        {
            public string matricola { get; set; }
            public DateTime dataUltimoAggiornamento { get; set; }
        }

        #endregion



        private static GetAnalisiEccezioniResponse GetAnalisiEccAR20()
        {
            try
            {
                int anno = DateTime.Now.Year;

                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

                GetAnalisiEccezioniResponse response = wcf1.GetAnalisiEccezioni(CommonManager.GetCurrentUserMatricola(),
                                                            new DateTime((int)anno, 8, 16),
                                                            new DateTime((int)anno, 12, 31),
                                                            "AR20",
                                                            null,
                                                            null
                                                            );

                if (response != null && response.DettagliEccezioni != null)
                {
                    foreach (var d in response.DettagliEccezioni)
                    {
                        d.data = new DateTime((int)anno, d.data.Month, d.data.Day);
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "Utente.GetAnalisiEcc()",
                    error_message = ex.ToString()
                });
                return null;
            }
        }

        public static List<DateTime> GetAR20Days()
        {
            SessionHelper.Set("AnalisiEccezioni", GetAnalisiEccAR20());
            GetAnalisiEccezioniResponse r = SessionHelper.Get("AnalisiEccezioni") as GetAnalisiEccezioniResponse;

            if (r == null || r.DettagliEccezioni == null)
                return new List<DateTime>();

            return r.DettagliEccezioni.Where(x => x.eccezione == "AR20").Select(x => x.data).ToList();
        }

        public static bool IsDirigente()
        {
            bool result = false;

            try
            {
                string tipologia = TipoDipendente();

                string cod_Categoria = Categoria();

                if (cod_Categoria.Trim().ToUpper().StartsWith("A01") || tipologia == "D")
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Restituisce true se il dipendente è un Dirigente o Dirigente Giornalista.
        /// False altrimenti
        /// </summary>
        /// <param name="matricola"></param>
        /// <returns></returns>
        public static bool IsDirigente_DirigenteGiornalista()
        {
            bool result = false;

            try
            {
                string cod_Categoria = Categoria();

                if (cod_Categoria.Trim().ToUpper().StartsWith("A7") || cod_Categoria.Trim().ToUpper().StartsWith("A01"))
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Il metodo restituisce TRUE se l'utente corrente
        /// è un Giornalista o un Dirigente giornalista.
        /// FALSE altrimenti
        /// </summary>
        /// <returns></returns>
        public static bool IsDirigenteGiornalista_OR_Giornalista()
        {
            bool result = false;

            try
            {
                string tipologia = TipoDipendente();

                string cod_Categoria = Categoria();

                if (cod_Categoria.Trim().ToUpper().StartsWith("A7") || tipologia == "G" || cod_Categoria.Trim().ToUpper().StartsWith("MXX"))
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

    }

    public class SessionListaEvidenzeModel
    {
        public DateTime UltimaScrittura { get; set; }

        public monthResponseEccezione ListaEvidenze { get; set; }
    }

    public class PeriodoRapporto
    {
        public PeriodoRapporto()
        {
            Info = new List<PeriodoRapportoInfo>();
        }
        public int IdRecord { get; set; }
        public int? Id_XR_MOD_DIPENDENTI { get; set; }
        public string Codice { get; set; }
        public string Cod_User { get; set; }
        public DateTime Inizio { get; set; }
        public DateTime Fine { get; set; }
        public DateTime? Scadenza { get; set; }
        public DateTime? InizioVisualizzazione { get; set; }
        public bool BloccaDataInizio { get; set; }
        public List<PeriodoRapportoInfo> Info { get; set; }
    }
    public class PeriodoRapportoInfo
    {
        public DateTime Inizio { get; set; }
        public DateTime Fine { get; set; }
        public int? NumeroGiorniPerMese { get; set; }
        public int? GiorniBase { get; set; }
        public int? GiorniExtra { get; set; }
    }
}