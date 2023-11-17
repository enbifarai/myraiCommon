using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using myRai.Models;
using myRaiData;
using myRaiHelper;
using myRaiServiceHub;
using myRaiServiceHub.it.rai.intranet.hrga;



namespace myRai.Business
{
    public class SessionManager
    {
        
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

            //Anagrafica Utente
            myRaiServiceHub.it.rai.servizi.hrgb.Service wsAnag = new myRaiServiceHub.it.rai.servizi.hrgb.Service();

            try
            {

                wsAnag.Credentials = CommonHelper.GetUtenteServizioCredentials();

                string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(CommonHelper.GetCurrentUserMatricola());
                string[] temp = str_temp.ToString().Split(';');

				if ((temp != null) && (temp.Count() > 16))
                {
                    HttpContext.Current.Session["UtenteAnagrafica"] = Utente.CaricaAnagrafica(temp);
                }
                else
                {
                    //se il servizio risponde una stringa nulla o non valida, recuper i dati da DigiGapp
                    MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                    wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

                    MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp resp =
                        wcf1.GetRecuperaUtente(myRai.Business.CommonHelper.GetCurrentUserMatricola7chars( ), DateTime.Today.ToString("ddMMyyyy"));

                    if (resp.data != null)
                    {
                        Utente.Anagrafica utente = new Utente.Anagrafica();
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
                wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

                MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp resp =
                    wcf1.GetRecuperaUtente(myRai.Business.CommonHelper.GetCurrentUserMatricola7chars( ), DateTime.Today.ToString("ddMMyyyy"));
                //resp.data = null; //freak - forzatura
                if (resp.data != null)
                {
                    Utente.Anagrafica utente = new Utente.Anagrafica();
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
                wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

                MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitydipendente_resp resp =
                                wcf1.GetRecuperaUtente(myRai.Business.CommonHelper.GetCurrentUserMatricola7chars(), DateTime.Today.ToString("ddMMyyyy"));

                Sedi SEDI = new Sedi();

                SEDI.Credentials = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

                if (resp.data == null)
                {
                    using (digiGappEntities db = new digiGappEntities())
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori();
                        err.matricola = Utente.Matricola();
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
                string strAuth = SEDI.autorizzazioni(pmatricola + ";HRUP;;;;E;0");
                string[] autorizzazioni = strAuth.Split(';');

                resp.data.isBoss = (autorizzazioni != null && autorizzazioni.Length > 3 &&
                                    autorizzazioni[0] == "01" && autorizzazioni[1] == "01" &&
                                    autorizzazioni[3] != null && autorizzazioni[3].ToUpper().Contains("01GEST"));


                Sedi SEDI2 = new Sedi();
                SEDI2.Credentials = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

				if ( HttpContext.Current.Session["FotoUtente"] == null )
				{
					HttpContext.Current.Session["FotoUtente"] = CommonManager.GetImmagineBase64( matricola );
				}

                HttpContext.Current.Session["Utente"] = resp;

                HttpContext.Current.Session["DatePerEvidenze"] = Utente.GetDateBackPerEvidenze();
               // HttpContext.Current.Session["UtenteListaProfili"] = SEDI2.Get_ProfiliAssociati_Net(CommonHelper.GetCurrentUserPMatricola(), "HRUP");
            }

            if (Utente.GestitoSirio()) 
                Utente.GetCeitonWeekPlan();
        }
    }
}