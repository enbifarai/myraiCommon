using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiCommonTasks.sendMail;
using myRaiData;

namespace myRaiCommonTasks
{
    public class NotificheMatricola
    {
        public string matricola { get; set; }
        public List<MyRai_Notifiche> notifiche { get; set; }
    }

    public class Notifiche
    {

        #region DIPENDENTE SOLLECITO MENSILE
        public static List<MyRai_Notifiche> GetSollecitoMensileDipendente()
        {
            var db = new digiGappEntities();
            string tipoDestinatario = EnumTipoDestinatarioNotifica.ESS_DIP.ToString();

            List<MyRai_Notifiche> LMensiliDipendente = new List<MyRai_Notifiche>();
            int giornoMese = DateTime.Now.Day;

            var invioRow = db.MyRai_InvioNotifiche.Where(x =>
                x.TipoDestinatario == tipoDestinatario &&
                x.TipoEvento == "CH" &&
                x.InvioAttivo == true).FirstOrDefault();

            if (IsInvioMensileNOW(invioRow))
            {
                List<string> SediGapp = myRaiCommonTasks.CommonTasks.GetSediGappPDF();

				MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client client = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

				client.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential
					( myRaiCommonTasks.CommonTasks.GetParametri<string>( myRaiCommonTasks.CommonTasks.EnumParametriSistema.AccountUtenteServizio )[0],
					myRaiCommonTasks.CommonTasks.GetParametri<string>( myRaiCommonTasks.CommonTasks.EnumParametriSistema.AccountUtenteServizio )[1] );

                string[] AccountESSWEB = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.UtentePerConvalida);

                //tutte le matricole per sede gapp 
                List<string> ListMatricole = new List<string>();
                foreach (string sede in SediGapp)
                {
                    var response = client.presenzeGiornaliere(AccountESSWEB[0], sede, DateTime.Today.ToString("ddMMyyyy"));
                    ListMatricole.AddRange(response.dati.Select(a => a.matricola).ToList());
                }
                string mailSollecitoDipendente = myRaiCommonTasks.CommonTasks.GetParametri<string>
                    (myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateSollecitoDipendente)[0];
                string mesePrecedente = DateTime.Today.AddMonths(-1).ToString("MMMM yyyy");
                mailSollecitoDipendente = mailSollecitoDipendente.Replace("#MESE", mesePrecedente);

                InviaNotifiche(ListMatricole, mailSollecitoDipendente);

            }

            return null;
        }

        public static Boolean IsInvioMensileNOW(MyRai_InvioNotifiche item)
        {
            if (item == null || item.OraMinuti == null || (item.GiornoDelMese == null && item.GiornoChiusura1 == null))
                return false;

            if (item.UltimoInvio != null && ((DateTime)item.UltimoInvio).Month == DateTime.Now.Month)
                return false;


            if (item.GiornoDelMese != null && item.GiornoDelMese == DateTime.Now.Day)
            {
                int oraminDB = Convert.ToInt32(item.OraMinuti);
                int oraminNOW = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
                return (oraminNOW >= oraminDB);
            }

            if (item.GiornoChiusura1 != null)
            {
                string mese = DateTime.Now.AddMonths(-1).Month.ToString().PadLeft(2, '0');
                string anno = DateTime.Now.AddMonths(-1).Year.ToString();
                string[] utenteConv = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.UtentePerConvalida);
                if (myRaiCommonTasks.CommonTasks.IsDataChiusura1(mese, anno, utenteConv[0], 75, (int)item.GiornoChiusura1))
                {
                    int oraminDB = Convert.ToInt32(item.OraMinuti);
                    int oraminNOW = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
                    return (oraminNOW >= oraminDB);
                }
            }
            return false;
        }
        #endregion




        #region LIVELLO 1 GIORNALIERE E SETTIMANALI
        public static void SendNotificheLivello1_GS()
        {
            //si occupa delle notifiche per chi ha impostato Giornaliere o Settimanali - NON ISTANTANEE

            var db = new digiGappEntities();
            int[] ore = myRaiCommonTasks.CommonTasks.GetParametri<int>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.NotificheRangeOre);

            if (DateTime.Now.Hour < ore[0] || DateTime.Now.Hour > ore[1])
            {
                Logger.Log("Orario fuori range, uscita task");
                return;
            }
       
            List<NotificheMatricola> LNotifiche = GetNotifiche(EnumTipoDestinatarioNotifica.ESS_LIV1,
                                                                EnumCategoriaEccezione.InsRichiesta,
                                                                EnumTipoEventoNotifica.INSR);


            List<NotificheMatricola> LnotStorni = GetNotifiche(EnumTipoDestinatarioNotifica.ESS_LIV1,
                                                                 EnumCategoriaEccezione.InsStorno,
                                                                 EnumTipoEventoNotifica.INSS);

            LNotifiche.AddRange(LnotStorni);


            List<NotificheMatricola> LnotScadute = GetNotifiche(EnumTipoDestinatarioNotifica.ESS_LIV1,
                                                               EnumCategoriaEccezione.MarcaturaScaduta,
                                                               EnumTipoEventoNotifica.SCAD);

            LNotifiche.AddRange(LnotScadute);


            List<NotificheMatricola> LnotUrgenti = GetNotifiche(EnumTipoDestinatarioNotifica.ESS_LIV1,
                                                              EnumCategoriaEccezione.MarcaturaUrgente,
                                                              EnumTipoEventoNotifica.URG);
            LNotifiche.AddRange(LnotUrgenti);

            if (LNotifiche.Count == 0)
            {
                Logger.Log("Nessuna notifica da inviare");
                return;
            }




            var NotificheDaInviarePerMatr = LNotifiche.GroupBy(
                 item => item.matricola,
                 item => item.notifiche,
                    (key, g) => new
                    {
                        matricola = key,
                        notifiche = g.SelectMany(n => n).ToList()
                    }).ToList();

            string[] MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>
                    (myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateNotificheBossL1);
            string MailTemplate = MailParams[0];
            string MailSubject = MailParams[1];

            foreach (var item in NotificheDaInviarePerMatr)
            {
                string testoTotale = "";
                string MailDestinatario = "";
                foreach (var notifica in item.notifiche)
                {
                    if (String.IsNullOrWhiteSpace(notifica.descrizione)) continue;

                    MailDestinatario = notifica.email_destinatario;
                    testoTotale += notifica.descrizione + " - il " + notifica.data_inserita.ToString("dd/MM/yyyy HH.mm") + "<br />";
                }


                bool success = InviaSingolaMail(MailTemplate.Replace("#NOTIFICHE", testoTotale), MailSubject, MailDestinatario);
                if (success)
                {
                    item.notifiche.ForEach(x => x.data_inviata = DateTime.Now);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Errore salvataggio DB SendNotificheLivello1_GS " + ex.ToString());
                    }
                }
            }
        }

        public static List<NotificheMatricola> GetNotifiche(EnumTipoDestinatarioNotifica tipodestinatario,
                                                             EnumCategoriaEccezione categoria,
                                                             EnumTipoEventoNotifica prefissoCategoria,
                                                             string prefissoCategoriaForzato=null   
                                                                )
        {
            //EnumTipoDestinatarioNotifica : ESS_DIP ESS_LIV1
            //EnumCategoriaEccezione : InsRichiesta, ApprovazioneEccezione
            //EnumTipoEventoNotifica : INSR, APPR

            string tipoNotifica4char = prefissoCategoria.ToString();
            if (prefissoCategoriaForzato != null)
            {
                tipoNotifica4char = prefissoCategoriaForzato;
            }

            string tipoNotifica = categoria.ToString();
            List<NotificheMatricola> LNotifiche = new List<NotificheMatricola>();

            var db = new digiGappEntities();
            string oreMinutiRiferimento = DateTime.Now.Hour.ToString().PadLeft(2, '0') + "00";//ora fissata a :00
            DateTime DataLimite = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);

            var ListNotifichePerMatricola = db.MyRai_Notifiche.Where(x => x.data_inviata == null && x.data_letta == null
               && x.categoria == tipoNotifica && x.data_inserita < DataLimite)
               .GroupBy(
                 p => p.matricola_destinatario,
                 p => p,
                (key, g) => new
                {
                    matricola = key,
                    notifiche = g.OrderBy(x =>
                                   x.data_inserita)
                });

            foreach (var item in ListNotifichePerMatricola)
            {
                if (IsToSendNow(item.matricola, tipodestinatario.ToString(), tipoNotifica4char, oreMinutiRiferimento))
                {
                    LNotifiche.Add(new NotificheMatricola() { matricola = item.matricola, notifiche = item.notifiche.ToList() });
                }
            }
            return LNotifiche;

        }
        #endregion




        #region DIPENDENTE GIORNALIERE E SETTIMANALI
        public static void SendNotificheDipendenteGS()
        {
            //si occupa delle notifiche per chi ha impostato Giornaliere o Settimanali - NON ISTANTANEE

            var db = new digiGappEntities();
            int[] ore = myRaiCommonTasks.CommonTasks.GetParametri<int>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.NotificheRangeOre);

            if (DateTime.Now.Hour < ore[0] || DateTime.Now.Hour > ore[1])
            {
                Logger.Log("Orario fuori range generico, uscita task");
                return;
            }


            //se gira alle 08.23 deve comunque prendere le notifiche fino alle 08.00, per evitare di mandarne altre appena arrivate
            string oreMinutiRiferimento = DateTime.Now.Hour.ToString().PadLeft(2, '0') + "00";//ora fissata a :00
            DateTime DataLimite = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);

            string tipoDestinatario = EnumTipoDestinatarioNotifica.ESS_DIP.ToString();
            string tipoNotificaAPPR = EnumCategoriaEccezione.ApprovazioneEccezione.ToString();
            string tipoNotificaRIF = EnumCategoriaEccezione.RifiutoEccezione.ToString();
            string tipoNotificaSCAD = EnumCategoriaEccezione.MarcaturaScaduta.ToString();

            List<NotificheMatricola> LNotifiche = new List<NotificheMatricola>();


            //legge notifiche APPROVAZIONI
            var ListApprovazioniPerMatricola = db.MyRai_Notifiche.Where(x => x.data_inviata == null && x.data_letta == null
                && x.categoria == tipoNotificaAPPR && x.data_inserita < DataLimite)
                .GroupBy(
                  p => p.matricola_destinatario,
                  p => p,
                 (key, g) => new
                 {
                     matricola = key,
                     notifiche = g.OrderBy(x =>
                                    x.data_inserita)
                 });

            //verifica se per ogni matricola è da inviare e aggiunge in LNotifiche
            foreach (var item in ListApprovazioniPerMatricola)
            {
                if (IsToSendNow(item.matricola, tipoDestinatario, "APPR", oreMinutiRiferimento))
                {
                    LNotifiche.Add(new NotificheMatricola() { matricola = item.matricola, notifiche = item.notifiche.ToList() });
                }
            }


            //legge notifiche RIFIUTI
            var ListRifiutiPerMatricola = db.MyRai_Notifiche.Where(x => x.data_inviata == null && x.data_letta == null
                && x.categoria == tipoNotificaRIF && x.data_inserita < DataLimite)
                .GroupBy(
                  p => p.matricola_destinatario,
                  p => p,
                 (key, g) => new
                 {
                     matricola = key,
                     notifiche = g.OrderBy(x =>
                                    x.data_inserita)
                 });
            foreach (var item in ListRifiutiPerMatricola)
            {
                if (IsToSendNow(item.matricola, tipoDestinatario, "RIF", oreMinutiRiferimento))
                {
                    LNotifiche.Add(new NotificheMatricola() { matricola = item.matricola, notifiche = item.notifiche.ToList() });
                }
            }


            //legge notifiche SCADUTE
            var ListScadutePerMatricola = db.MyRai_Notifiche.Where(x => x.data_inviata == null && x.data_letta == null
                && x.categoria == tipoNotificaSCAD && x.data_inserita < DataLimite)
                .GroupBy(
                  p => p.matricola_destinatario,
                  p => p,
                 (key, g) => new
                 {
                     matricola = key,
                     notifiche = g.OrderBy(x =>
                                    x.data_inserita)
                 });
            foreach (var item in ListScadutePerMatricola)
            {
                if (IsToSendNow(item.matricola, tipoDestinatario, "SCAD", oreMinutiRiferimento))
                {
                    LNotifiche.Add(new NotificheMatricola() { matricola = item.matricola, notifiche = item.notifiche.ToList() });
                }
            }

            var NotificheDaInviarePerMatr = LNotifiche.GroupBy(
                 item => item.matricola,
                 item => item.notifiche,
                    (key, g) => new
                    {
                        matricola = key,
                        notifiche = g.SelectMany(n => n).ToList()
                    }).ToList();



            string[] MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateNotificheDipendente);
            string MailTemplate = MailParams[0];
            string MailSubject = MailParams[1];

            foreach (var item in NotificheDaInviarePerMatr)
            {
                string testoTotale = "";
                string MailDestinatario = "";
                foreach (var notifica in item.notifiche)
                {
                    MailDestinatario = notifica.email_destinatario;

                    if (notifica.categoria == EnumCategoriaEccezione.ApprovazioneEccezione.ToString()
                        || notifica.categoria == EnumCategoriaEccezione.RifiutoEccezione.ToString())
                    {
                        MyRai_Richieste r = db.MyRai_Richieste.Where(x => x.id_richiesta == notifica.id_riferimento).FirstOrDefault();
                        if (r != null)
                        {
                            MyRai_Eccezioni_Richieste er = r.MyRai_Eccezioni_Richieste.FirstOrDefault();
                            if (er != null && er.nominativo_primo_livello != null)
                            {
                                notifica.descrizione += " - da " + er.nominativo_primo_livello.Trim();
                                if (er.data_validazione_primo_livello != null)
                                {
                                    notifica.descrizione += " il " + ((DateTime)er.data_validazione_primo_livello).ToString("dd/MM/yyyy HH.mm");
                                }
                            }
                        }
                    }
                    testoTotale += notifica.descrizione + "<br />";
                }
                bool success = InviaSingolaMail(MailTemplate.Replace("#NOTIFICHE", testoTotale), MailSubject, MailDestinatario);
                if (success)
                {
                    item.notifiche.ForEach(x => x.data_inviata = DateTime.Now);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Errore salvataggio DB SendNotificheDipendente " + ex.ToString());
                    }
                }
            }
        }

        public static Boolean IsToSendNow(string matricola, string tipodestinatario, string tipoevento, string oreMinutiRiferimento)
        {
           // if (DateTime.Now.ToString("dd/MM/yyyy") == "03/07/2018") return true;

            using (digiGappEntities db = new digiGappEntities())
            {
                var ImpostazioneNotifica = GetImpostazioneNotifiche(matricola, tipodestinatario, tipoevento);

              

                if (ImpostazioneNotifica == null ||
                    ImpostazioneNotifica.TipoInvio == "N" ||
                    ImpostazioneNotifica.TipoInvio == "I" ||
                    ImpostazioneNotifica.OraMinuti != oreMinutiRiferimento) return false;


                if (ImpostazioneNotifica.TipoInvio == "G")
                    return (Convert.ToInt32(DateTime.Now.ToString("HHmm")) >= Convert.ToInt32(ImpostazioneNotifica.OraMinuti == null ? "0000" : ImpostazioneNotifica.OraMinuti));
                else
                {
                    if (ImpostazioneNotifica.GiornoDellaSettimana == null) ImpostazioneNotifica.GiornoDellaSettimana = 1;
                    return (ImpostazioneNotifica.GiornoDellaSettimana == (int)DateTime.Today.DayOfWeek &&
                        Convert.ToInt32(DateTime.Now.ToString("HHmm")) >= Convert.ToInt32(ImpostazioneNotifica.OraMinuti == null ? "0000" : ImpostazioneNotifica.OraMinuti));
                }
            }
        }

        public static MyRai_InvioNotifiche GetImpostazioneNotifiche(string matricola, string tipodestinatario, string tipoevento)
        {
            var db = new digiGappEntities();
            var ImpostazioneNotifica = db.MyRai_InvioNotifiche.Where(x =>
                x.InvioAttivo == true &&
                x.TipoDestinatario == tipodestinatario &&
                x.TipoEvento == tipoevento &&
                (x.Matricola == matricola || x.Matricola == "*"))
                .OrderByDescending(x => x.Matricola).FirstOrDefault();

            return ImpostazioneNotifica;
        }

        public static Boolean InviaSingolaMail(string body, string subj, string dest, bool ccRaiPlace=true)
        {
            MailSender invia = new MailSender();

            string from = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailApprovazioneFrom)[0];
            string[] AccountUtenteServizio = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.AccountUtenteServizio);
            invia.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");

            Email eml = new Email();

            eml.From = from;

			//dest = "RUO.SIP.PRESIDIOOPEN@rai.it";

            eml.toList = dest.Split(',');// new string[] { dest };
            
            if (ccRaiPlace)
                eml.ccList = new string[] {"raiplace.selfservice@rai.it" };

            eml.ContentType = "text/html";
            eml.Priority = 2;
            eml.SendWhen = DateTime.Now.AddSeconds(1);
            eml.Subject = subj;
            eml.Body = body;
            try
            {
                invia.Send(eml);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Errore invio email da [InviaSingolaMail] a " + dest + " - " + ex.ToString());
                return false;
            }
        }

        public static void InviaNotifiche(List<string> Lmatricole, string mailBody)
        {
            MailSender invia = new MailSender();

            string subj = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateSollecitoDipendente)[1];
            string from = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailApprovazioneFrom)[0];
            string[] AccountUtenteServizio = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.AccountUtenteServizio);
            invia.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");

            foreach (string matricola in Lmatricole)
            {
                Email eml = new Email();

                eml.From = from;

                eml.toList = new string[] { "P" + matricola + "@rai.it" };
                eml.ContentType = "text/html";
                eml.Priority = 2;
                eml.SendWhen = DateTime.Now.AddSeconds(1);
                eml.Subject = subj;
                eml.Body = mailBody;
                try
                {
                    //invia.Send(eml);
                }
                catch (Exception ex)
                {
                    Logger.Log("Errore invio email a " + eml.toList[0] + " - " + ex.ToString());
                }
            }
        }
        
        #endregion




       



    }
}