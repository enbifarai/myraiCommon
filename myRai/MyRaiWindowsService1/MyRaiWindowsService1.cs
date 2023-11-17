using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using myRaiData;
using MyRaiWindowsService1.it.rai.servizi.HRGA;
using MyRaiWindowsService1.it.rai.servizi.svildigigappws;



namespace MyRaiWindowsService1
{
    // installazione:
    // c:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil myraiwindowsservice1.exe

    public partial class MyRaiWindowsService1 : ServiceBase
    {
       
        System.Timers.Timer TimerUrgentiScadute;
        System.Timers.Timer TimerNotificheUtenteLivello1;

		System.Timers.Timer TimerGeneraPDFeccezioni;
		System.Timers.Timer TimerMainSendMail;
		System.Timers.Timer TimerMainPresenzePdf;
		System.Timers.Timer TimerMainConvalidaPdf;
		System.Timers.Timer TimerGiornalieroCheckMeseChiuso;
		System.Timers.Timer TimerPDFinFirma;
		System.Timers.Timer TimerNotifiche;
		
		
		System.Timers.Timer TimerPuliziaLog;

        public MyRaiWindowsService1()
        {
            InitializeComponent();
        }

        public void MyStart()
        {
            OnStart(null);
        }

        protected override void OnStop()
        {
            Logger.Log("Service stopped");

        }

        protected override void OnStart(string[] args)
        {
            
            Logger.Log("---------------------------------------------------------------------------------");
            Logger.Log("Service started");
            Logger.Log("---------------------------------------------------------------------------------");
            //  TimerGiornalieroCheckMeseChiuso_Elapsed( null, null );

            //TimerGeneraPDFeccezioni_Elapsed(null, null);
            //  return;

            //  TimerPdfDaFirmareLiv2_Elapsed(null, null);
            //  return;

            //   TimerGiornalieroCheckMeseChiuso_Elapsed(null, null);

		 


			//invia mail pdf
	 TimerPDFinFirma_Elapsed(null, null);
      return;


            //pdf del giovedi


            //TimerPuliziaLog = new System.Timers.Timer();
            //TimerPuliziaLog.Interval = 86400000; // 1 giorno
            //TimerPuliziaLog.Elapsed += TimerPuliziaLog_Elapsed;
            //TimerPuliziaLog.Enabled = true;
            //TimerPuliziaLog.Start();
            //TimerPuliziaLog_Elapsed( null, null );

			
	 // //      TimerPDFinFirma_Elapsed(null, null);
	 ////       return;

//	   TimerGeneraPDFeccezioni_Elapsed(null, null);
//	    return;


	 //	 //  NotificheDestinateL1.InviaRichiesteInserite();

			////Urgenti e scadute:
			//TimerUrgentiScadute = new System.Timers.Timer();
			//TimerUrgentiScadute.Interval = 30 * 60 * 1000;
			//TimerUrgentiScadute.Elapsed += TimerUrgentiScadute_Elapsed;
			//TimerUrgentiScadute.Enabled = true;
			//TimerUrgentiScadute.Start();
			//TimerUrgentiScadute_Elapsed( null, null );

         

            /////////////////////////////////////////////////////
            // MIGRATO IN ALTO CIO CHE E' ATTIVO IN PRODUZIONE //
            /////////////////////////////////////////////////////

         //   NotificheDestinateL1.InviaRichiesteInserite();
         //   return;

            
            ///////////////////////////////////////////////////////////////////////////////
            //Generazione PDF Eccezioni del giovedi
            ///////////////////////////////////////////////////////////////////////////////
			//TimerGeneraPDFeccezioni = new System.Timers.Timer();
			//TimerGeneraPDFeccezioni.Interval = 10 * 60 * 1000;
			//TimerGeneraPDFeccezioni.Elapsed += TimerGeneraPDFeccezioni_Elapsed;
			//TimerGeneraPDFeccezioni.Enabled = true;
			//TimerGeneraPDFeccezioni.Start();
			//TimerGeneraPDFeccezioni_Elapsed( null, null );
			//return;

            ///////////////////////////////////////////////////////////////////////////////
            //Invia mail per PDF generati da firmare
            ///////////////////////////////////////////////////////////////////////////////
            //TimerPDFinFirma = new System.Timers.Timer();
            //TimerPDFinFirma.Interval = 60 * 60 * 1000;
            //TimerPDFinFirma.Elapsed += TimerPDFinFirma_Elapsed;
            //TimerPDFinFirma.Enabled = true;
            //TimerPDFinFirma.Start();
		 //TimerPDFinFirma_Elapsed(null, null);
        //return;


            ///////////////////////////////////////////////////////////////////////////////
            //Chiusure di fine mese ( 1mo e 2do giorno )   
            ///////////////////////////////////////////////////////////////////////////////
			//TimerGiornalieroCheckMeseChiuso = new System.Timers.Timer();
			//TimerGiornalieroCheckMeseChiuso.Interval = 5 * 60 * 1000;
			//TimerGiornalieroCheckMeseChiuso.Elapsed += TimerGiornalieroCheckMeseChiuso_Elapsed;
			//TimerGiornalieroCheckMeseChiuso.Enabled = true;
			//TimerGiornalieroCheckMeseChiuso.Start();
			//TimerGiornalieroCheckMeseChiuso_Elapsed( null, null );
			//return;


           
            //Notifiche.GetSollecitoMensileDipendente();

            //CheckMailDocDaFirmare();

            //EseguiOnTimerPDFeccezioni();

            //int intervalloSec = 0;
            //TimerMain = new System.Timers.Timer();
            //TimerMainUrgentiScadute = new System.Timers.Timer();
            //TimerMainSendMail = new System.Timers.Timer();
            //TimerMainPresenzePdf = new System.Timers.Timer();
            //TimerMainConvalidaPdf = new System.Timers.Timer();
            //TimerGiornalieroCheckMeseChiuso = new System.Timers.Timer();

            //intervalloSec = GetParametro<int>(EnumParametriSistema.IntervalloBatchSecondi);
            //TimerMain.Interval = intervalloSec * 1000;
            //TimerMain.Elapsed += TimerMain_Elapsed;
            //TimerMain.Enabled = true;
            //TimerMain.Start();

            //TimerGiornalieroCheckMeseChiuso.Interval = 60 * 60 * 1000;
            //TimerGiornalieroCheckMeseChiuso.Elapsed += TimerGiornalieroCheckMeseChiuso_Elapsed;
            //TimerGiornalieroCheckMeseChiuso.Enabled = true;
            //TimerGiornalieroCheckMeseChiuso.Start();

            //TimerPDFinFirma.Interval = 5 * 60 * 1000;
            //TimerPDFinFirma.Elapsed += TimerPDFinFirma_Elapsed; 
            //TimerPDFinFirma.Enabled = true;
            //TimerPDFinFirma.Start();
        }

		#region TimerPuliziaLog
		/// <summary>
		/// Metodo che si occupa di richiamare l'azione per la pulizia dei log
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void TimerPuliziaLog_Elapsed ( object sender, System.Timers.ElapsedEventArgs e )
		{
			TimerPuliziaLog.Stop();
			TimerPuliziaLog.Enabled = false;

			try
			{
				Logger.Log( "Inizio TimerPuliziaLog_Elapsed" );

				PuliziaLog services = new PuliziaLog();
				//services.Start(null);

				Logger.Log( "Fine TimerPuliziaLog_Elapsed" );
			}
			catch ( Exception ex )
			{
				Logger.Log( ex.ToString() );
				LogErrore( ex.ToString(), "TimerPuliziaLog_Elapsed" );
			}

			TimerPuliziaLog.Start();
			TimerPuliziaLog.Enabled = true;
		}

		#endregion

        void TimerGeneraPDFeccezioni_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //  TimerGeneraPDFeccezioni.Stop();
            //  TimerGeneraPDFeccezioni.Enabled = false;

            try
            {
                EseguiOnTimerPDFeccezioni();
            }
            catch (Exception ex)
            {
                LogErrore(ex.ToString(), "EseguiOnTimer");
            }

            //  TimerGeneraPDFeccezioni.Start();
            // TimerGeneraPDFeccezioni.Enabled = true;
        }

        void TImerNotifiche_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimerNotifiche.Stop();
            TimerNotifiche.Enabled = false;

            try
            {
                Notifiche.SendNotificheDipendenteGS();
            }
            catch (Exception ex)
            {
                LogErrore(ex.ToString(), "TimerNotifiche");
            }

            TimerNotifiche.Enabled = true;
            TimerNotifiche.Start();
        }

        void TimerPDFinFirma_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //  TimerPDFinFirma.Stop();
            //  TimerPDFinFirma.Enabled = false;

            try
            {
                CheckMailDocDaFirmare();
            }
            catch (Exception ex)
            {

                LogErrore(ex.ToString(), "TimerPDFinFirma");
            }

            // TimerPDFinFirma.Enabled = true;
            // TimerPDFinFirma.Start();
        }

        void TimerGiornalieroCheckMeseChiuso_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //  TimerGiornalieroCheckMeseChiuso.Stop();
            // TimerGiornalieroCheckMeseChiuso.Enabled=false;

            try
            {
                EseguiOnTimerGiornaliero();
            }
            catch (Exception ex)
            {

                LogErrore(ex.ToString(), "EseguiOnTimerGiornaliero");
            }

            //  TimerGiornalieroCheckMeseChiuso.Enabled = true;
            // TimerGiornalieroCheckMeseChiuso.Start();
        }

        void TimerUrgentiScadute_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimerUrgentiScadute.Stop();
            TimerUrgentiScadute.Enabled = false;

            try
            {
                Logger.Log("Inizio marcatura urgenti scadute");
                MarcaturaUrgentiScadute.MarcaturaRichiesteUrgentiScadute();
                Logger.Log("Fine marcatura urgenti scadute");
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                LogErrore(ex.ToString(), "MarcaturaRichiesteUrgentiScadute");
            }

            TimerUrgentiScadute.Start();
            TimerUrgentiScadute.Enabled = true;
        }

        void TimerMainSendMail_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimerMainSendMail.Stop();
            TimerMainSendMail.Enabled = false;

            try
            {
                EseguiInvioEmail();
            }
            catch (Exception ex)
            {
                LogErrore(ex.ToString(), "EseguiInvioEmail");
            }

            TimerMainSendMail.Start();
            TimerMainSendMail.Enabled = true;
        }

        void TimerMainPresenzePdf_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimerMainPresenzePdf.Stop();
            TimerMainPresenzePdf.Enabled = false;

            try
            {
                EseguiOnTimerPresenzePdf();
            }
            catch (Exception ex)
            {
                LogErrore(ex.ToString(), "EseguiOnTimerPresenzePdf");
            }

            TimerMainPresenzePdf.Start();
            TimerMainPresenzePdf.Enabled = true;
        }

        void TimerMainConvalidaPdf_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimerMainConvalidaPdf.Stop();
            TimerMainConvalidaPdf.Enabled = false;

            try
            {
                EseguiOnTimerConvalidaPdf();
            }
            catch (Exception ex)
            {
                LogErrore(ex.ToString(), "EseguiOnTimerConvalidaPdf");
            }

            TimerMainConvalidaPdf.Start();
            TimerMainConvalidaPdf.Enabled = true;
        }

        #region qui viene richiamato il service SENDMAIL
		public void EseguiInvioEmail()
        {

            //DIPENDENTI - invii istantanei:

            //List<MyRai_Notifiche> LApprRich = Notifiche.GetNotificheIstantaneeDipendente(EnumTipoEventoNotifica.APPR,
            //     EnumCategoriaEccezione.ApprovazioneEccezione);

            //List<MyRai_Notifiche> LRifiutiRich = Notifiche.GetNotificheIstantaneeDipendente(EnumTipoEventoNotifica.RIF,
            //   EnumCategoriaEccezione.RifiutoEccezione);

            //List<MyRai_Notifiche> LScadute = Notifiche.GetNotificheIstantaneeDipendente(EnumTipoEventoNotifica.SCAD,
            //  EnumCategoriaEccezione.MarcaturaScaduta);

            //LApprRich.AddRange(LRifiutiRich);
            //LApprRich.AddRange(LScadute);


            //var gruppi = LApprRich.GroupBy(
            //     p => p.matricola_destinatario,
            //     p => p,
            //    (key, g) => new { matricola = key, notifiche = g.OrderBy (x=>x.data_inserita) .ToList() });


            //DIPENDENTI - invii GIORNALIERI - G





            //if (ItsTimeToGoEmail())
            //{
            //    var db = new digiGappEntities();
            //    var service = new it.rai.servizi.HRGA.Sedi();

            //    var listEmail = db.MyRai_Notifiche.Where(x => x.data_letta == null && x.data_inviata == null && x.email_destinatario != null)
            //                                      .OrderBy(x => x.matricola_destinatario);

            //    var array01 = service.Get_CategoriaDato_Net("sedegapp", "", "hrup", "01gest").CategorieDatoAbilitate_Array[0].DT_Utenti_CategorieDatoAbilitate;

            //    var array02 = service.Get_CategoriaDato_Net("sedegapp", "", "hrup", "02gest").CategorieDatoAbilitate_Array[0].DT_Utenti_CategorieDatoAbilitate;

            //    List<string> ListaMatricoleResp01 = new List<string>();
            //    List<string> ListaMatricoleResp02 = new List<string>();
            //    Boolean check = false;

            //    for (int x = 0; x < array01.Rows.Count; x++)
            //    {
            //        ListaMatricoleResp01.Add(array01.Rows[x].ItemArray[0].ToString());
            //    }

            //    for (int x = 0; x < array02.Rows.Count; x++)
            //    {
            //        ListaMatricoleResp02.Add(array02.Rows[x].ItemArray[0].ToString());
            //    }

            //    for (int x = 0; x < ListaMatricoleResp02.Count; x++)
            //    {
            //        for (int i = 0; i < ListaMatricoleResp01.Count; i++)
            //        {
            //            if (ListaMatricoleResp02[x] != ListaMatricoleResp01[i])
            //            {
            //                check = false;
            //            }
            //            else
            //            { 
            //                check = true;
            //                break;
            //            }
            //        }
            //        if(check== false) ListaMatricoleResp01.Add(ListaMatricoleResp02[x]);

            //    }



            //    String corpoEmail = "", matPrec = "";
            //    int count = 0;

            //    sendMail.MailSender send = new sendMail.MailSender();
            //    sendMail.Email mail = new sendMail.Email();
            //    mail.From = GetParametro<string>(EnumParametriSistema.MailApprovazioneFrom);
            //    mail.ContentType = "text/html";
            //    mail.Priority = 2;
            //    mail.Subject = GetParametro<string>(EnumParametriSistema.MailRichiesteSubject);
            //    string[] AccountUtenteServizio = GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
            //    foreach (var item in listEmail)
            //    {
            //        if (matPrec != item.matricola_destinatario)
            //        {
            //            String matPrecTemp = matPrec;
            //            if (corpoEmail != "")
            //            {
            //                mail.toList = new string[] { ListaMatricoleResp01[count] + "@rai.it" };
            //                mail.Body = GetParametro<string>(EnumParametriSistema.MailTemplateUrgentiScadute);
            //                mail.Body = mail.Body.Replace("#DESCR", corpoEmail);
            //                mail.SendWhen = DateTime.Now.AddSeconds(1);
            //                send.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");
            //                count = count + 1;
            //                try
            //                {
            //                    send.Send(mail);
            //                }
            //                catch (Exception ex)
            //                {
            //                    LogErrore(ex.ToString(), "SENDMAIL");
            //                }

            //                corpoEmail = "";
            //                db = new digiGappEntities();
            //                foreach (var item1 in listEmail)
            //                {
            //                    var update = db.MyRai_Notifiche.Where(x => x.data_letta == null && x.data_inviata == null && x.email_destinatario != null && x.matricola_destinatario == item1.matricola_destinatario).FirstOrDefault();
            //                    update.data_inviata = DateTime.Now;
            //                    db.SaveChanges();
            //                }


            //            }
            //            matPrec = item.matricola_destinatario;
            //            corpoEmail = corpoEmail + item.descrizione + "<br>";
            //        }
            //        else
            //        { corpoEmail = corpoEmail + item.descrizione + "<br>"; }

            //    }
            //}
        }

        Boolean ItsTimeToGoEmail()
        {
            int ExecHours = Common.GetParametro<int>(EnumParametriSistema.EsecuzioneInvioEmail);
            int NowHours = Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString().PadLeft(2, '0'));
            return (NowHours == ExecHours);
        }

        #endregion

        #region qui viene richiamato il service  MarcaturaRichiesteUrgentiScadute



        #endregion

        #region qui viene richiamato il service  PRESENZE PDF

        void EseguiOnTimerPresenzePdf()
        {
            if (ItsTimeToGoPDF())
            {
                //check sedi Gapp abilitate
                List<string> ListaSediGapp = GetSediGappPDF();

                if (ListaSediGapp == null || ListaSediGapp.Count == 0) return;

                DateTime Sunday = GetlastSunday(DateTime.Now).Date;
                DateTime Monday = GetlastMonday(Sunday).Date;


                foreach (string sedeGapp in ListaSediGapp)
                {
                    try
                    {
                        Boolean result = ProcessaPresenzePdf(sedeGapp, Monday, Sunday);
                        if (result == true)
                        {
                            // InvioEmailConvalide(result);
                            AggiungiNotificaSedeGapp(sedeGapp, Monday, Sunday, 0);
                        }

                    }
                    catch (Exception ex)
                    {
                        LogErrore(ex.ToString(), "CheckPresenzePdf");
                    }
                }


            }
        }

        public Boolean ProcessaPresenzePdf(string sede, DateTime dateStart, DateTime dateEnd)
        {
            WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();
            service.Credentials = System.Net.CredentialCache.DefaultCredentials;

            try
            {
                presenzeResponse resp = service.getPresenze("103650", "*", dateStart.ToString("ddMMyyyy"),
                                                                          dateEnd.ToString("ddMMyyyy"), sede, 75);
                if (resp.esito != true)
                {
                    LogErrore(resp.errore + resp.raw, "GetRiepilogo");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Errore DB ProcessaPresenzePdf : " + ex.ToString());
            }
            return false;
        }

        #endregion

        void EseguiOnTimerConvalidaPdf()
        {
            if (ItsTimeToGoPDF())
            {
                //check sedi Gapp abilitate
                List<string> ListaSediGapp = GetSediGappPDF();
                string[] sedeGapp = ListaSediGapp.ToArray();

                if (ListaSediGapp == null || ListaSediGapp.Count == 0) return;

                DateTime Sunday = GetlastSunday(DateTime.Now).Date;
                DateTime Monday = GetlastMonday(Sunday).Date;

                try
                {
                    string result = ProcessaConvalidaPdf(sedeGapp, Monday, Sunday);
                    if (result != "")
                    {
                        InvioEmailConvalide(result);
                        AggiungiNotificaSedeGapp(sedeGapp, Monday, Sunday, -1);

                    }
                    else
                    {
                        AggiungiNotificaSedeGapp(sedeGapp, Monday, Sunday, 0);
                    }
                }
                catch (Exception ex)
                {
                    LogErrore(ex.ToString(), "CheckConvalidaePdf");
                }

            }
        }

        public String ProcessaConvalidaPdf(string[] sede, DateTime dateStart, DateTime dateEnd)
        {
            WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();
            service.Credentials = new System.Net.NetworkCredential(Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            var db = new digiGappEntities();

            Periodo[] scad = new Periodo[] { };
            riepilogoSedeGappResponse resp = new riepilogoSedeGappResponse();

            string dataTempStart = dateStart.ToString("ddMMyyyy");
            string dataTempEnd = dateEnd.ToString("ddMMyyyy");
            DateTime dataTemp = new DateTime();
            Boolean checkConvalida = false, checkConvalidaDate = false;
            String DescSediGapp = "", mex = "";
            int idResponse = 0;

            try
            {
                dataTemp = dateStart;
                for (int z = 0; z < 7; z++)
                {
                    scad = service.getScadenzario("103650", dataTemp.ToString("ddMMyyyy"), dataTemp.ToString("ddMMyyyy"), "75", sede, 75);

                    if (scad[z].giornate[z].statoTotale == "C")
                    {
                        DescSediGapp = GetDescSediGappPDF(scad[z].sedeGapp.ToString());
                        checkConvalida = true;
                        checkConvalidaDate = true;
                        dataTempStart = dataTemp.ToString("ddMMyyyy");
                        dataTempEnd = dataTemp.ToString("ddMMyyyy");
                        mex = scad[z].sedeGapp + " - " + DescSediGapp + " con data da: " + dataTemp.ToShortDateString() + " a: " + dataTemp.ToShortDateString() + " ";
                    }
                    else
                    { checkConvalida = false; }

                    if (checkConvalida == true)
                    {
                        for (int i = 0; i < sede.Count(); i++)
                        {
                            resp = service.getRiepilogoReadOnly("103650", "Bifano Vincenzo", dataTempStart, dataTempEnd,
                                                                sede[i], true, 75);
                            idResponse = resp.ID;
                            var update = db.DIGIRESP_Archivio_PDF.Where(x => x.ID == idResponse).FirstOrDefault();
                            update.matricola_stampa = "batch";
                            update.stato_pdf = EnumStatiPDF.Convalidato;
                            db.SaveChanges();

                            if (resp.esito != true)
                            {
                                LogErrore(resp.errore + resp.raw, "ProcessaConvalidePdf");
                            }
                        }
                    }
                    dataTemp = dataTemp.AddDays(1);
                }

                if (checkConvalidaDate == true)
                {
                    dataTempEnd = dateEnd.ToString("ddMMyyyy");
                }
                for (int i = 0; i < sede.Count(); i++)
                {
                    resp = service.getStampa("103650", "*", dataTempStart, dataTempEnd, sede[i], true, 75);
                    if (resp.esito != true)
                    {
                        LogErrore(resp.errore + resp.raw, "ProcessaPresenzePdf");
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Log("Errore DB ProcessaPresenzePdf : " + ex.ToString());
            }
            return mex;
        }

        public void CheckMailDocDaFirmare()
        {

            var db = new digiGappEntities();
            var list = db.MyRai_Notifiche.Where(x => x.categoria == "PDF firma" && x.data_inviata == null).ToList();
            Logger.Log("Notifiche PDF firma:" + list.Count);
            if (list.Count == 0) return;

            var gruppi = list.GroupBy(
                p => p.matricola_destinatario,
                p => p,
               (key, g) => new { matricola = key, notifiche = g.OrderBy(x => x.data_inserita).ToList() });

            string[] template = Common.GetParametri<string>(EnumParametriSistema.MailTemplatePDFinFirma);


            foreach (var g in gruppi)
            {
                try
                {
                    Logger.Log(g.matricola + ": " + g.notifiche.Count + " notifiche");

                    string messaggio = "";
                    string maildest = "";
                    foreach (var n in g.notifiche)
                    {
                        messaggio += "<br />" + n.descrizione;
                        maildest = n.email_destinatario;
                    }

                    string mailbody = template[0].Replace("#DOCS", messaggio);
                    string mailsubj = template[1];
                    if (String.IsNullOrWhiteSpace(maildest)) maildest = "P" + g.matricola + "@rai.it";

                    Logger.Log("Invio mail a " + maildest);

                    InvioEmail(mailbody + "<p>Sara inviato a :" + maildest + "</p>", mailsubj, "ruo.sip.presidioopen@rai.it", null);

                    string errore = InvioEmail(mailbody, mailsubj, maildest, "raiplace.selfservice@rai.it");
                    if (errore == null)
                    {
                        Logger.Log("Invio ok");
                        foreach (var n in g.notifiche) n.data_inviata = DateTime.Now;
                        db.SaveChanges();
                        Logger.Log("DB update ok");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.ToString());
                    LogErrore(ex.ToString(), "CheckMailDocDaFirmare ");
                }
            }
        }

        public string InvioEmail(String body, string subj, string dest, string copy)
        {
            var db = new digiGappEntities();

            string eMail = System.Configuration.ConfigurationManager.AppSettings["DefaultMail"];

            if (!String.IsNullOrEmpty(eMail))
            {
                subj = eMail;
            }

            sendMail.MailSender send = new sendMail.MailSender();
            sendMail.Email mail = new sendMail.Email();
            mail.From = Common.GetParametro<string>(EnumParametriSistema.MailApprovazioneFrom);
            mail.ContentType = "text/html";
            mail.Priority = 2;
            mail.Subject = subj;
            string[] AccountUtenteServizio = Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
            string[] MailTemplateScadenzario = Common.GetParametri<string>(EnumParametriSistema.MailTemplateScadenzario);
            mail.toList = new string[] { 
                dest
            };
            if (!String.IsNullOrWhiteSpace(copy))
            {
                mail.ccList = new string[] { copy };
            }

            mail.Body = body;
            mail.SendWhen = DateTime.Now.AddSeconds(1);
            send.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");
            try
            {
                send.Send(mail);
                return null;
            }
            catch (Exception ex)
            {
                LogErrore(ex.ToString(), "SENDMAIL");
                return ex.Message;
            }
        }

        public void InvioEmailConvalide(String messaggio)
        {
            var db = new digiGappEntities();
            sendMail.MailSender send = new sendMail.MailSender();
            sendMail.Email mail = new sendMail.Email();
            mail.From = Common.GetParametro<string>(EnumParametriSistema.MailApprovazioneFrom);
            mail.ContentType = "text/html";
            mail.Priority = 2;
            mail.Subject = Common.GetParametro<string>(EnumParametriSistema.MailApprovazioneSubject);
            string[] AccountUtenteServizio = Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
            string[] MailTemplateScadenzario = Common.GetParametri<string>(EnumParametriSistema.MailTemplateScadenzario);
            mail.toList = new string[] { "vincenzo.bifano@rai.it" };
            mail.Body = MailTemplateScadenzario[0];
            mail.Body = mail.Body.Replace("#DESCR", messaggio + MailTemplateScadenzario[1]);
            mail.SendWhen = DateTime.Now.AddSeconds(1);
            send.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");
            try
            {
                send.Send(mail);
            }
            catch (Exception ex)
            {
                LogErrore(ex.ToString(), "SENDMAIL");
            }
        }

        #region qui viene richiamato il service eseguiOnTimerSedeGapp

        public String GetDescSediGappPDF(String sedeGapp)
        {
            var service = new it.rai.servizi.HRGA.Sedi();
            service.Credentials = new System.Net.NetworkCredential(Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            try
            {
                CategorieDatoAbilitate response = service.Get_CategoriaDato_Net("sedegapp", "", "hrup", "02GEST");
                if (response != null && !String.IsNullOrWhiteSpace(response.Descrizione_Errore))
                {
                    Logger.Log("GetSediGapp Errore servizio: " + response.Descrizione_Errore);
                    return null;
                }

                if (response != null
                    && response.DT_CategorieDatoAbilitate != null
                    && response.DT_CategorieDatoAbilitate.Rows.Count > 0)
                {

                    var descSede = response.DT_CategorieDatoAbilitate.Select(@"cod='" + sedeGapp + "'");
                    return descSede[0].ItemArray[1].ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("GetSediGapp Error: " + ex.ToString());
            }

            return null;
        }

        public List<string> GetSediGappPDF()
        {
            var service = new it.rai.servizi.HRGA.Sedi();
            service.Credentials = new System.Net.NetworkCredential(Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            try
            {
                CategorieDatoAbilitate response = service.Get_CategoriaDato_Net("sedegapp", "", "hrup", "02GEST");
                if (response != null && !String.IsNullOrWhiteSpace(response.Descrizione_Errore))
                {
                    Logger.Log("GetSediGapp Errore servizio: " + response.Descrizione_Errore);
                    return null;
                }

                if (response != null
                    && response.DT_CategorieDatoAbilitate != null
                    && response.DT_CategorieDatoAbilitate.Rows.Count > 0)
                {
                    List<string> sedi = response.DT_CategorieDatoAbilitate
                                        .AsEnumerable()
                                        .Select(p => p.Field<string>("Cod")
                                        .Trim())
                                        .Distinct()
                                        .OrderBy(cod => cod)
                                        .ToList();
                    return sedi;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("GetSediGapp Error: " + ex.ToString());
            }

            return null;
        }

        public Boolean? IsSomethingChanged(string sedegapp, DateTime DateStart, DateTime DateEnd)
        {
            var db = new digiGappEntities();
            string stato = EnumStatiPDF.Stampato;

            var pdfMaxVersion = db.DIGIRESP_Archivio_PDF.Where(x =>
                 x.sede_gapp == sedegapp
              && x.stato_pdf == stato
              && x.data_inizio == DateStart
              && x.data_fine == DateEnd
              ).OrderByDescending(x => x.numero_versione).FirstOrDefault();

            if (pdfMaxVersion == null) return null;

            WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();
            service.Credentials = new System.Net.NetworkCredential(Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            riepilogoSedeGappResponse resp = service.getRiepilogoReadOnly("103650", "Bifano Vincenzo",
                       pdfMaxVersion.data_inizio.ToString("ddMMyyyy"),
                       pdfMaxVersion.data_fine.ToString("ddMMyyyy"),
                       sedegapp,
                       false,
                       75);
            string eccezioniSerial = Serialize(resp.eccezioni);
            return (pdfMaxVersion.contenuto_eccezioni != eccezioniSerial);
        }

        public Boolean CheckSedeGappEccezioni(string sedeGapp, DateTime CurrentDateStart, DateTime CurrentDateEnd)
        {
            var db = new digiGappEntities();
            string stato = EnumStatiPDF.Stampato;

            var ListaOldPeriodi = db.DIGIRESP_Archivio_PDF.Where(x =>
                   x.sede_gapp == sedeGapp
                && x.stato_pdf == stato
                && x.data_inizio != CurrentDateStart
                && x.data_fine != CurrentDateEnd
                )
                .GroupBy(
                    key => new { da = key.data_inizio, a = key.data_fine, },
                    v => v,
                     (key, v) => v.OrderByDescending(x => x.numero_versione).FirstOrDefault())
                .OrderByDescending(x => x.data_inizio).ThenByDescending(x => x.data_fine)
                .ToList();

            if (ListaOldPeriodi == null) return false;

            WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();
            service.Credentials = new System.Net.NetworkCredential(Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            foreach (var item in ListaOldPeriodi)
            {
                riepilogoSedeGappResponse resp = service.getRiepilogoReadOnly("103650", "Bifano Vincenzo",
                           item.data_inizio.ToString("ddMMyyyy"),
                           item.data_fine.ToString("ddMMyyyy"),
                           sedeGapp,
                           false,
                           75);
                string eccezioniSerial = Serialize(resp.eccezioni);
                if (item.contenuto_eccezioni != eccezioniSerial)
                {
                    riepilogoSedeGappResponse resp2 = service.getRiepilogoReadOnly("103650", "Bifano Vincenzo",
                               item.data_inizio.ToString("ddMMyyyy"),
                               item.data_fine.ToString("ddMMyyyy"),
                               sedeGapp,
                               true,
                               75);
                    string ecSerial = Serialize(resp2.eccezioni);
                    DIGIRESP_Archivio_PDF pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.ID == resp2.ID).FirstOrDefault();
                    if (pdf != null)
                    {
                        pdf.numero_versione = item.numero_versione + 1;
                        pdf.contenuto_eccezioni = ecSerial;
                        db.SaveChanges();
                    }
                }
            }
            return true;
        }

        public Boolean SedeProcessata(string sede, DateTime datestart, DateTime dateend)
        {
            var db = new digiGappEntities();

            var row = db.DIGIRESP_Archivio_PDF.Where(x => x.sede_gapp == sede && x.data_inizio == datestart && x.data_fine == dateend).FirstOrDefault();
            return (row != null);

        }

        public int ProcessaSedeGapp(string sede, DateTime dateStart, DateTime dateEnd, string TipoDiStampa = "sc")
        {
            var db = new digiGappEntities();

            //bool? isSomethingChanged = IsSomethingChanged(sede, dateStart, dateEnd);
            //if (isSomethingChanged == null) { 
            //    Console.WriteLine("PDF non creato precedentemente, creazione in progress"); 
            //}
            //if (isSomethingChanged == false)
            //{
            //    Console.WriteLine("Contenuto eccezioni INVARIATO - File NON RICREATO");
            //    return -1;
            //}
            //if (isSomethingChanged == true)
            //{
            //    Console.WriteLine("Contenuto eccezioni VARIATO !!!! Creazione in progress");
            //}

            WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();
            service.Credentials = new System.Net.NetworkCredential(Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            try
            {
                riepilogoSedeGappResponse resp = new riepilogoSedeGappResponse();
                if (TipoDiStampa == EnumTipoDiStampa.ro.ToString())
                {
                    resp = service.getRiepilogoReadOnly("103650", "Bifano Vincenzo",
                                  dateStart.ToString("ddMMyyyy"),
                                  dateEnd.ToString("ddMMyyyy"),
                                  sede,
                                  true,
                                  75);
                }
                if (TipoDiStampa == EnumTipoDiStampa.ss.ToString())
                {
                    resp = service.getStampa("103650", "Bifano Vincenzo",
                                  dateStart.ToString("ddMMyyyy"),
                                  dateEnd.ToString("ddMMyyyy"),
                                  sede,
                                  true,
                                  75);
                }
                if (TipoDiStampa == EnumTipoDiStampa.sc.ToString())
                {
                    resp = service.getRiepilogo("103650", "Bifano Vincenzo",
                                  dateStart.ToString("ddMMyyyy"),
                                  dateEnd.ToString("ddMMyyyy"),
                                  sede,
                                  75);
                }
                if (resp.esito != true)
                {
                    LogErrore(resp.errore + resp.raw, "GetRiepilogo");
                    return -1;
                }
                string eccezioniSerial = Serialize(resp.eccezioni);



                var pdfrow = db.DIGIRESP_Archivio_PDF.Where(x => x.ID == resp.ID).FirstOrDefault();

                if (pdfrow != null)
                {
                    pdfrow.contenuto_eccezioni = eccezioniSerial;
                    db.SaveChanges();
                    return resp.ID;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Errore DB ProcessaSedeGapp : " + ex.ToString());
            }
            return -1;
        }

        Boolean ItsTimeToGoPDF()
        {
            int[] ExecutionDay = Common.GetParametri<int>(EnumParametriSistema.GiornoEsecuzioneBatchPDF);
            int dayToday = (int)DateTime.Now.DayOfWeek;
            int oramin = Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString().PadLeft(2, '0'));
            return (ExecutionDay[0] <= dayToday && oramin >= ExecutionDay[1]);
        }

        public void EseguiOnTimerGiornaliero()
        {
            myRaiCommonTasks.CommonTasks.Task_VerificaChiusureMese();
        }

        public void EseguiOnTimerPDFeccezioni()
        {

            if (ItsTimeToGoPDF())
            {
                string response=  myRaiCommonTasks.CommonTasks.Task_GeneraPdfEccezioni();
                //if (response != null)
                //{
                //    string[] par = myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateReportGenPDF);
                //    Notifiche.InviaSingolaMail(par[0].Replace("#TEXT", response), par[1],
                //        "vincenzo.bifano@rai.it,ruo.sip.presidioopen@rai.it,", false);
                //}

            }
        }

        public void AggiungiNotificaSedeGapp(string[] sede, DateTime dateStart, DateTime dateEnd, int idPdf)
        {
            var db = new digiGappEntities();
            var tt = Common.GetMatricolaLivelloPerSede(sede, 2);

            String matricola = "";
            for (int i = 0; i < tt.Count; i++)
            {
                if (tt[i].ToString().Substring(0, 1) == "P")
                { matricola = tt[i].Substring(1, tt[i].Length - 1); }
                else
                { matricola = tt[i].ToString(); }

                myRaiData.MyRai_Notifiche n = new MyRai_Notifiche()
                {
                    categoria = "PDF firma",
                    data_inserita = DateTime.Now,
                    descrizione = "Nuovo doc in firma :" + dateStart.ToString("ddMM") + "/" + dateEnd.ToString("ddMM"),
                    inserita_da = "ServizioWindows",
                    id_riferimento = idPdf,
                    matricola_destinatario = matricola
                };
                db.MyRai_Notifiche.Add(n);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Log("AggiungiNotificaSedeGapp errore:" + ex.ToString());
                }
            }


        }

        public void AggiungiNotificaSedeGapp(string sede, DateTime dateStart, DateTime dateEnd, int idPdf)
        {
            var db = new digiGappEntities();
            var tt = Common.GetMatricolaLivelloPerSede(sede, 2);

            String matricola = "";
            for (int i = 0; i < tt.Count; i++)
            {
                if (tt[i].ToString().Substring(0, 1) == "P")
                { matricola = tt[i].Substring(1, tt[i].Length - 1); }
                else
                { matricola = tt[i].ToString(); }

                myRaiData.MyRai_Notifiche n = new MyRai_Notifiche()
                {
                    categoria = "PDF firma",
                    data_inserita = DateTime.Now,
                    descrizione = "Nuovo doc in firma :" + dateStart.ToString("ddMM") + "/" + dateEnd.ToString("ddMM"),
                    inserita_da = "ServizioWindows",
                    id_riferimento = idPdf,
                    matricola_destinatario = matricola
                };
                db.MyRai_Notifiche.Add(n);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Log("AggiungiNotificaSedeGapp errore:" + ex.ToString());
                }
            }
        }

        public static List<String> GetMatricolaLivelloPerSede(string sedegapp, int livelloResponsabile_1_2)
        {
            Autorizzazioni.Sedi service = new Autorizzazioni.Sedi();
            service.Credentials = new System.Net.NetworkCredential(myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.AccountUtenteServizio)[0],
                myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.AccountUtenteServizio)[1]);

            Autorizzazioni.CategorieDatoAbilitate response = service.Get_CategoriaDato_Net("sedegapp", "", "HRUP", "0" + livelloResponsabile_1_2.ToString() + "GEST");
            var item = response.CategorieDatoAbilitate_Array
                .Where(sede => sede.Codice_categoria_dato.Trim().ToUpper() == sedegapp.Trim().ToUpper())
                       .FirstOrDefault();
            if (item == null) return null;

            List<string> Matricole = item.DT_Utenti_CategorieDatoAbilitate
                                         .AsEnumerable()
                                         .Select(p => (String)p.ItemArray[0]).ToList();
            return Matricole;
        }

        public void StartupSede(string sede, string TipoDiStampa)
        {
            if (TipoDiStampa != "ro" && TipoDiStampa != "ss" && TipoDiStampa != "sc")
            {
                Console.WriteLine("Tipo di stampa errato (ro,ss,sc)");
                return;
            }
            WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();
            service.Credentials = new System.Net.NetworkCredential(Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            DateTime d1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            Periodo[] periodo = service.getScadenzarioSingolaSede("103650", d1.ToString("ddMMyyyy"), DateTime.Now.ToString("ddMMyyyy"), "75", sede, 75);
            DateTime LastDayConvalidato = d1.AddDays(-1);

            foreach (var item in periodo[0].giornate)
            {
                if (item.statoTotale == "L" || item.statoTotale == "C")
                {
                    LastDayConvalidato = item.data;
                }
            }

            DateTime StartingDate = LastDayConvalidato.AddDays(1);
            DateTime DomenicaScorsa = GetlastSunday(DateTime.Now);

            List<DateTime> Domeniche = new List<DateTime>();


            while (StartingDate <= DomenicaScorsa)
            {
                if (StartingDate.DayOfWeek == DayOfWeek.Sunday) Domeniche.Add(StartingDate);

                StartingDate = StartingDate.AddDays(1);
            }
            List<PDFperiod> PeriodiPdf = new List<PDFperiod>();

            foreach (DateTime D in Domeniche)
            {
                PDFperiod p = new PDFperiod();
                p.DateEnd = D;
                p.DateStart = D.AddDays(-6);
                if (p.DateStart < LastDayConvalidato.AddDays(1)) p.DateStart = LastDayConvalidato.AddDays(1);
                PeriodiPdf.Add(p);
            }


            Console.WriteLine("\r\n");
            foreach (var per in PeriodiPdf)
            {
                Console.WriteLine(per.DateStart.ToString("dd/MM/yyyy") + "..." + per.DateEnd.ToString("dd/MM/yyyy") + "\r\n");
            }

            string conn = System.Configuration.ConfigurationManager.ConnectionStrings["digiGappEntities2"].ToString().ToUpper();


            Console.WriteLine("Confermi ? (yes) " + (conn.Contains("ZTO") ? " **** SEI SU PRODUZIONE ***************" : " **SVILUPPO**"));

            string resp = Console.ReadLine();
            if (resp.ToLower() != "yes") return;



            foreach (var pdf in PeriodiPdf)
            {
                int idpdf = ProcessaSedeGapp(sede, pdf.DateStart, pdf.DateEnd, TipoDiStampa);
                Console.WriteLine(idpdf);
            }
        }

        public string GetEmailPerMatricola(string matricola)
        {
            try
            {
                EsponiAnagrafica.Service s = new EsponiAnagrafica.Service();
                s.Credentials = new System.Net.NetworkCredential(Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], Common.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

                string r = s.EsponiAnagrafica("raicv;" + matricola + ";;E;0");
                return r.Split(';')[15];
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #region classi comuni

        public static void LogErrore(string errore, string provenienza)
        {
            try
            {
                if (Environment.UserInteractive) Console.WriteLine(errore);
                MyRai_LogErrori err = new MyRai_LogErrori()
                {
                    applicativo = "BATCH",
                    data = DateTime.Now,
                    error_message = errore,
                    matricola = "000000",
                    provenienza = provenienza
                };
                using (var db = new digiGappEntities())
                {
                    db.MyRai_LogErrori.Add(err);
                    db.SaveChanges();
                }
                Logger.Log("Errore da " + provenienza + ": " + errore);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        public DateTime GetlastSunday(DateTime dateStart)
        {
            DateTime LastSunday = dateStart.AddDays(-1);

            while (LastSunday.DayOfWeek != DayOfWeek.Sunday)
            {
                LastSunday = LastSunday.AddDays(-1);
            }
            return LastSunday;
        }

        public DateTime GetlastMonday(DateTime dateStart)
        {
            DateTime LastMonday = dateStart.AddDays(-1);

            while (LastMonday.DayOfWeek != DayOfWeek.Monday)
            {
                LastMonday = LastMonday.AddDays(-1);
            }
            return LastMonday;
        }

        public string Serialize(Eccezione[] v)
        {
            var json = new JavaScriptSerializer().Serialize(v);
            return json;
        }

        #endregion
    }

    public static class EnumStatiPDF
    {
        public static string Stampato { get { return "S_OK"; } }
        public static string Convalidato { get { return "C_OK"; } }
    }
    public class PDFperiod
    {
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
    }
    public class SollecitoApprovazione
    {
        public string sedeGapp { get; set; }
        public int RichiesteCount { get; set; }
        public IEnumerable<MyRai_Richieste> Richieste { get; set; }
        public List<string> MatricoleRespLiv1 { get; set; }
    }
}