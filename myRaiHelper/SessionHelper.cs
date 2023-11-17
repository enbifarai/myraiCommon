using myRaiData;
using myRaiServiceHub.Autorizzazioni;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace myRaiHelper
{
    public static class SessionHelper
    {
        private static bool _alreadyInitialized = false;

        private static bool _webContext = true;
        private static Dictionary<string, object> _sessionStore = null;

        private static void Initialize()
        {
            if (!_alreadyInitialized)
            {
                var appSettings = System.Configuration.ConfigurationManager.AppSettings;

                if (appSettings["WebContext"] == null || appSettings["WebContext"] == "true")
                {
                    _webContext = true;
                }
                else
                {
                    _webContext = false;
                }
                _alreadyInitialized = true;
            }
        }

        public static object Get(string key)
        {
            Initialize();

            if (_webContext)
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                    return null;
                else
                    return HttpContext.Current.Session[key];
            }
            else
            {
                if (_sessionStore == null || !_sessionStore.ContainsKey(key))
                    return null;
                else
                    return _sessionStore[key];
            }
        }

        public static T Get<T>(string key, Func<T> defaultGetter)
        {
            Initialize();

            if (_webContext)
            {
                if (HttpContext.Current.Session[key] == null)
                    return defaultGetter();
                else
                {
                    bool IsNullable(Type type) => Nullable.GetUnderlyingType(type) != null;
                    if (IsNullable(typeof(T)))
                        return (T)HttpContext.Current.Session[key];
                    else
                        return (T)Convert.ChangeType(HttpContext.Current.Session[key], typeof(T));
                }
            }
            else
            {
                if (_sessionStore == null || !_sessionStore.ContainsKey(key))
                    return defaultGetter();
                else
                    return (T)Convert.ChangeType(_sessionStore[key], typeof(T));
            }
        }
        public static T Get<T>(string key, T defaultValue)
        {
            Initialize();

            if (_webContext)
            {
                if (HttpContext.Current.Session[key] == null)
                {
                    Set(key, defaultValue);
                    return defaultValue;
                }
                else
                {
                    bool IsNullable(Type type) => Nullable.GetUnderlyingType(type) != null;
                    if (IsNullable(typeof(T)))
                        return (T)HttpContext.Current.Session[key];
                    else
                        return (T)Convert.ChangeType(HttpContext.Current.Session[key], typeof(T));
                }
            }
            else
            {
                if (_sessionStore == null || !_sessionStore.ContainsKey(key))
                {
                    Set(key, defaultValue);
                    return defaultValue;
                }
                else
                    return (T)Convert.ChangeType(_sessionStore[key], typeof(T));
            }
        }

        public static void Set(string key, object value)
        {
            Initialize();

            if (_webContext)
            {
                try
                {
                    HttpContext.Current.Session[key] = value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(key + " - " + ex.Message);
                }

            }
            else
            {
                if (_sessionStore == null) _sessionStore = new Dictionary<string, object>();
                if (!_sessionStore.ContainsKey(key))
                    _sessionStore.Add(key, value);
                else
                    _sessionStore[key] = value;
            }
        }

        public static void Set(SessionVariables SessionName, object SessionValue)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return;
            else
                Set(SessionName.ToString(), SessionValue);
        }
        public static object Get(SessionVariables SessionName)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return null;
            else
                return Get(SessionName.ToString());
        }

        public static void SetEccezioniPossibili(List<myRaiData.MyRai_Eccezioni_Ammesse> e, string matricola, DateTime dataecc)
        {
            EccPoss ep = new EccPoss()
            {
                timeStamp = DateTime.Now,
                matricola = matricola,
                dataEccezioni = dataecc,
                EccezioniPossibili = e
            };
            HttpContext.Current.Session["EccezioniPossibili"] = ep;
        }
        public static List<myRaiData.MyRai_Eccezioni_Ammesse> GetEccezioniPossibili(string matricola, DateTime dataecc)
        {
            if (HttpContext.Current.Session["EccezioniPossibili"] == null)
                return null;

            EccPoss ep = (EccPoss)HttpContext.Current.Session["EccezioniPossibili"];
            if ((ep.matricola == matricola || ep.matricola == "0" + matricola) &&
                ep.timeStamp.AddSeconds(15) > DateTime.Now
                && dataecc == ep.dataEccezioni)
            {
                return ep.EccezioniPossibili;
            }
            else
                return null;
        }
        public static void SessionStart()
        {
            string[] valori = null;

            try
            {
                valori = CommonHelper.GetParametri<string>(EnumParametriSistema.OrariGapp);
            }
            catch (Exception ex)
            {

            }

            //string appType = CommonHelper.GetAppSettings("AppType");
            var appType = CommonHelper.GetApplicationType();

            //Anagrafica Utente
            myRaiServiceHub.it.rai.servizi.hrgb.Service wsAnag = new myRaiServiceHub.it.rai.servizi.hrgb.Service();

            try
            {

                wsAnag.Credentials = CommonHelper.GetUtenteServizioCredentials();

                string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(CommonHelper.GetCurrentUserMatricola());
                string[] temp = str_temp.ToString().Split(';');

                if ((temp != null) && (temp.Count() > 16))
                {
                    HttpContext.Current.Session["UtenteAnagrafica"] = UtenteHelper.CaricaAnagrafica(temp);
                }
                else
                {
                    //se il servizio risponde una stringa nulla o non valida, recuper i dati da DigiGapp
                    MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                    wcf1.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();

                    MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp resp =
                        wcf1.GetRecuperaUtente(CommonHelper.GetCurrentUserMatricola7chars(), DateTime.Today.ToString("ddMMyyyy"));

                    if (resp.data != null)
                    {
                        UtenteHelper.Anagrafica utente = new UtenteHelper.Anagrafica();
                        //carico l'oggetto utente con il resp
                        if (String.IsNullOrWhiteSpace(resp.data.Cognome) && String.IsNullOrWhiteSpace(resp.data.Nome))
                            utente._cognome = resp.data.nominativo;
                        else
                        {
                            utente._cognome = resp.data.Cognome;
                            utente._nome = resp.data.Nome;
                        }
                        utente._matricola = resp.data.matricola;
                        utente._codiceFigProf = resp.data.categoria;
                        utente._dataNascita = null;
                        utente._dataAssunzione = null;
                        HttpContext.Current.Session["UtenteAnagrafica"] = utente;
                    }
                }
            }
            catch (Exception exc)
            {
                //se il servizio risponde una stringa nulla o non valida, recuper i dati da DigiGapp
                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                wcf1.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();

                MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp resp =
                    wcf1.GetRecuperaUtente(CommonHelper.GetCurrentUserMatricola7chars(), DateTime.Today.ToString("ddMMyyyy"));
                //resp.data = null; //freak - forzatura
                if (resp.data != null)
                {
                    UtenteHelper.Anagrafica utente = new UtenteHelper.Anagrafica();
                    //carico l'oggetto utente con il resp
                    utente._cognome = resp.data.nominativo;
                    utente._matricola = resp.data.matricola;
                    utente._codiceFigProf = resp.data.categoria;
                    utente._dataNascita = null;
                    utente._dataAssunzione = null;
                    HttpContext.Current.Session["UtenteAnagrafica"] = utente;
                }
            }

            if ((Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString().PadLeft(2, '0')) > Convert.ToInt32(valori[0]))
                && (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString().PadLeft(2, '0')) < Convert.ToInt32(valori[1])))
            {
                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                wcf1.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();

                MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp resp =
                                wcf1.GetRecuperaUtente(CommonHelper.GetCurrentUserMatricola7chars(), DateTime.Today.ToString("ddMMyyyy"));

                Sedi SEDI = new Sedi();

                SEDI.Credentials = CommonHelper.GetUtenteServizioCredentials();

                if (resp.data == null)
                {
                    using (digiGappEntities db = new digiGappEntities())
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori();
                        err.matricola = CommonHelper.GetCurrentUserMatricola();
                        err.data = DateTime.Now;
                        err.error_message = "Errore servizio wApiUtilitydipendente_resp";
                        err.applicativo = "Servizio";
                        err.provenienza = new StackFrame(1, true).GetMethod().Name;
                        Logger.LogErrori(err);
                    }
                    Exception ex = new Exception("Errore servizio wApiUtilitydipendente_resp");
                    throw ex;
                }

                string matricola = CommonHelper.GetCurrentUserMatricola();
                string pmatricola = CommonHelper.GetCurrentUserPMatricola();

                if (appType == ApplicationType.RaiPerMe)
                {
                    string strAuth = SEDI.autorizzazioni(pmatricola + ";HRUP;;;;E;0");
                    string[] autorizzazioni = strAuth.Split(';');

                    resp.data.isBoss = (autorizzazioni != null && autorizzazioni.Length > 3 &&
                                        autorizzazioni[0] == "01" && autorizzazioni[1] == "01" &&
                                        autorizzazioni[3] != null && autorizzazioni[3].ToUpper().Contains("01GEST"));
                }

                CommonHelper.GetCurrentIdPersona();


                if (HttpContext.Current.Session["FotoUtente"] == null)
                {
                    HttpContext.Current.Session["FotoUtente"] = CommonHelper.GetImmagineBase64(matricola);
                }

                HttpContext.Current.Session["Utente"] = resp;

                if (appType == ApplicationType.RaiPerMe)
                    HttpContext.Current.Session["DatePerEvidenze"] = UtenteHelper.GetDateBackPerEvidenze();
            }

            if (UtenteHelper.GestitoSirio())
                CeitonHelper.GetCeitonWeekPlan(CommonHelper.GetCurrentUserPMatricola(), CommonHelper.GetCurrentUserMatricola());

            if (appType==ApplicationType.Gestionale)
                AuthHelper.EnabledFuncs(CommonHelper.GetCurrentUserMatricola());
        }
    }
}
