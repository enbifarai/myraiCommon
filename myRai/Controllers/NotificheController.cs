using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRaiCommonModel;
using myRaiData;
using myRai.Data.CurriculumVitae;
using myRaiHelper;
using myRaiCommonManager;

namespace myRai.Controllers
{
    public class NotificheController : BaseCommonController
    {
        public ActionResult Index(int? tipo)
        {
            if (tipo == null) tipo = 2;
            return View( new MieNotificheModel( ) { tipo = tipo } );
        }
        
		public ActionResult RefreshMieNotifiche(int tipo)
        {
            MieNotificheModel model = new MieNotificheModel();
            var db = new myRaiData.digiGappEntities();
            string matr = CommonHelper.GetCurrentUserMatricola();

            var Lnotifiche = db.MyRai_Notifiche
                              .Where(x => x.matricola_destinatario == matr && x.data_letta == null);

			if ( Lnotifiche != null && Lnotifiche.Any() )
			{
				Lnotifiche.ToList().ForEach( n =>
				{
					n.descrizione = n.descrizione.Replace( "\\", " " );
				} );
			}

            if (tipo == 1)
                Lnotifiche = Lnotifiche.Where(x => x.tipo == 1).OrderByDescending(x => x.data_inserita);
            else
                Lnotifiche = Lnotifiche.Where(x => x.tipo == 2 || x.tipo == null).OrderByDescending(x => x.data_inserita);


            model.Notifiche = new List<NotificaPlus>();

            foreach (var item in Lnotifiche)
            {
                MyRai_Richieste rich = null;
                if (item.categoria == "ApprovazioneEccezione" || item.categoria == "RifiutoEccezione" ||
					item.categoria.Equals( "InsRichiesta" ) || item.categoria.Equals( "InsStorno" ) ||
					item.categoria.Equals( "MarcaturaUrgente" ) || item.categoria.Equals( "MarcaturaScaduta" ) )
                {
                    rich = db.MyRai_Richieste.Where(x => x.id_richiesta == item.id_riferimento).FirstOrDefault();
                }
                model.Notifiche.Add(new NotificaPlus() { notifica = item, richiesta = rich });
            }

            try
            {
                it.rai.servizi.comunica.Comunica hrComunica = new it.rai.servizi.comunica.Comunica();
                hrComunica.Credentials = System.Net.CredentialCache.DefaultCredentials;
                it.rai.servizi.comunica.ListaNotificheDocumenti result = hrComunica.ElencoNotificheDocumenti("41ZMWEJWSDJD16DPWX8S28ZZVTLOF56P", UtenteHelper.EsponiAnagrafica()._matricola, "WS_COMINT");
                if (result.Esito == 0)
                {
                    if (result.Notifiche.Split('|')[0] == "SI")
                    {
                        myRaiData.MyRai_Notifiche item = new myRaiData.MyRai_Notifiche();
                        item.id = 0;

                        item.descrizione = "<a href='/BustaPaga'>Hai cedolini non letti</a>";
                        model.Notifiche.Add(new NotificaPlus() { notifica = item, richiesta = null });

                    }
                    if (result.Notifiche.Split('|')[1] == "SI")
                    {
                        myRaiData.MyRai_Notifiche item = new myRaiData.MyRai_Notifiche();
                        item.id = 0;
                        item.descrizione = "<a href='/DocumentiAmministrativi'>Hai documenti amministrativi non letti</a>";
                        model.Notifiche.Add(new NotificaPlus() { notifica = item, richiesta = null });
                    }
                }

                var notificheCorsi = NotificheManager.GetNotificheCorsi(matr, tipo);
                foreach (var item in notificheCorsi)
                    model.Notifiche.Add(item);


                if (tipo == 2)
                {
                    using (cv_ModelEntities dbCV = new cv_ModelEntities())
                    {
                        TCVLogin login = dbCV.TCVLogin.FirstOrDefault(x => x.Matricola == matr);
                        if (login != null)
                        {
                            int percCv = CommonHelper.GetPercentualCV(matr);
                            if (percCv < 100)
                            {
                                //ha compilato almeno una parte del nuovo cv
                                MyRai_Notifiche notificaCV = new MyRai_Notifiche();
                                notificaCV.categoria = "CV Online";
                                notificaCV.tipo = 2;
                                notificaCV.id = 0;
                                notificaCV.descrizione = "Completa la compilazione del CV";
                                string note = CommonHelper.GetParametro<string>(EnumParametriSistema.NotificaCVLess100);
                                model.Notifiche.Add(new NotificaPlus()
                                    {
                                        notifica = notificaCV,
                                        richiesta = null,
                                        ShowDetail = true,
                                        Dettaglio = new NotificaDettaglio()
                                        {
                                            Title = notificaCV.descrizione,
                                            Note = note,
                                            ShowButton = true,
                                            AnchorHref = "/cv_online",
                                            AnchorText = "Accedi al CV online"
                                        }
                                    });
                            }
                        }
                        else
                        {
                            MyRai_Notifiche notificaCV = new MyRai_Notifiche();
                            notificaCV.categoria = "CV Online";
                            notificaCV.tipo = 2;
                            notificaCV.id = 0;
                            notificaCV.descrizione = "Compila il nuovo CV";
                            string note = CommonHelper.GetParametro<string>(EnumParametriSistema.NotificaCVZero);
                            model.Notifiche.Add(new NotificaPlus()
                            {
                                notifica = notificaCV,
                                richiesta = null,
                                ShowDetail = true,
                                Dettaglio = new NotificaDettaglio()
                                {
                                    Title = notificaCV.descrizione,
                                    Note = note,
                                    ShowButton = true,
                                    AnchorHref = "/cv_online",
                                    AnchorText = "Accedi al CV online"
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }

            model.Notifiche = model.Notifiche.OrderBy(a => a.notifica.id).ToList();
            if (model.Notifiche.Any(x => x.notifica.categoria == "CV Online" && x.notifica.id>0))
            {
                int index = model.Notifiche.FindIndex(x => x.notifica.id > 0);
                if (index >= 0)
                {
                    NotificaPlus c = model.Notifiche.FirstOrDefault(x => x.notifica.categoria == "CV Online");
                    model.Notifiche.Remove(c);
                    model.Notifiche.Insert(index, c);
                }
            }

            return View("../Tabelle/subpartial/lemienotifiche", model);
        }

        public ActionResult RefreshMieNotifiche1()
        {
            return RefreshMieNotifiche(1);
        }

        public ActionResult RefreshMieNotifiche2()
        {
            return RefreshMieNotifiche(2);
        }

        [HttpPost]
        public ActionResult DettaglioNotifica(NotificaDettaglio notifica)
        {
            return PartialView("~/Views/Notifiche/dettaglioNotifica.cshtml", notifica);
        }
    }
}