using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using myRai.Data.CurriculumVitae;
using myRai.DataAccess;
using myRaiData;
using myRaiHelper;

namespace myRaiCommonModel
{
    public class NotifichePopupModel
    {
        private const string DEFAULT_HEADER = "Svil_Header";

        public NotifichePopupModel(string matr)
        {
            string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(matr);

            string[] temp = str_temp.ToString().Split(';');
            string codiceContratto = "";
            string societa = "";
            //temp = null; //freak - forzatura
            if ((temp != null) && (temp.Count() > 16))
            {
                codiceContratto = temp[5];
                societa = temp[14];
            }

            using (var db = new myRaiData.digiGappEntities())
            {
                Notifiche = db.MyRai_Notifiche.Where(x =>
                      x.matricola_destinatario == matr &&
                      x.data_letta == null)
                      .OrderByDescending(x => x.data_inserita).ToList();
                NotificheTotali = Notifiche.Where(x => x.tipo == null || x.tipo == 2).Count();
                NotificheTotaliTipo1 = Notifiche.Where(x => x.tipo == 1).Count();
                ParametriNotifiche = db.MyRai_ParametriSistema.Where(a => a.Chiave.Contains("IndirizzoComnit")).ToList();

                this.HeaderMenu = new List<myRaiData.MyRai_HeaderMenu>();

                var filtroContesto = CommonHelper.GetHeaderFiltroContesto();

                try
                {
                    this.HeaderMenu = db.MyRai_HeaderMenu.Where(filtroContesto).OrderBy(m => m.IdParent).OrderBy(m2 => m2.Posizione).ToList();
                }
                catch (Exception ex)
                {
                    this.HeaderMenu = new List<myRaiData.MyRai_HeaderMenu>();
                }

            }
            try
            {
                myRaiServiceHub.it.rai.servizi.comunica.Comunica hrComunica = new myRaiServiceHub.it.rai.servizi.comunica.Comunica();
                hrComunica.Credentials = System.Net.CredentialCache.DefaultCredentials;
                myRaiServiceHub.it.rai.servizi.comunica.ListaNotificheDocumenti result = hrComunica.ElencoNotificheDocumenti("41ZMWEJWSDJD16DPWX8S28ZZVTLOF56P", matr, "WS_COMINT");
                if (result.Esito == 0)
                {
                    if (result.Notifiche.Split('|')[0] == "SI")
                    {
                        myRaiData.MyRai_Notifiche item = new myRaiData.MyRai_Notifiche();
                        item.id = 0;
                        item.descrizione = "<a href='/BustaPaga'>Hai cedolini non letti</a>";
                        Notifiche.Add(item);
                        NotificheTotali += 1;

                    }
                    if (result.Notifiche.Split('|')[1] == "SI")
                    {
                        myRaiData.MyRai_Notifiche item = new myRaiData.MyRai_Notifiche();
                        item.id = 0;
                        item.descrizione = "<a href='/DocumentiAmministrativi'>Hai documenti amministrativi non letti</a>";
                        Notifiche.Add(item);
                        NotificheTotali += 1;

                    }
                }
                using (var db = new myRaiData.CorsiJobEntities())
                {
                    // var statiNotValid = new string[] { "'\'" + CommonHelper.GetParametro<string>(EnumParametriSistema.IdCorsiNotValid).Replace(",", "'\','\'") + "'\'" };
                    string codiceCorso = CommonHelper.GetParametro<string>(EnumParametriSistema.CorsoOnline).ToString();
                    var listcorsi = db.tbCorsiCodice.Select(a => new { a.codice_tbCorsiCodice, a.titolo_tbCorsiCodice }).Where(a => a.codice_tbCorsiCodice == codiceCorso).Distinct().ToList();
                    //string str_temp = wsAnag.EsponiAnagrafica("RAICV;" + CommonHelper.GetCurrentUserMatricola() + ";;E;0");


                    if (temp.Length >= 26 && temp[25].Substring(0, 1) != "A" && temp[25].Substring(0, 3) != "M71")
                    {
                        if (CommonHelper.GetParametro<string>(EnumParametriSistema.TipiContrattoCorsiOlnlie).Contains(codiceContratto) && CommonHelper.GetParametro<string>(EnumParametriSistema.SocietaCorsiOnline).Contains(societa))
                        {


                            foreach (var corso in listcorsi)
                            {

                                myRaiData.tblPartecipantiOnline partecipanti = db.tblPartecipantiOnline.Where(a => a.matricola == "P" + matr && a.codice == corso.codice_tbCorsiCodice).FirstOrDefault();

                                myRaiData.MyRai_Notifiche item = new myRaiData.MyRai_Notifiche();
                                if (partecipanti == null)
                                {
                                    item.id = 0;
                                    item.tipo = 1;
                                    item.descrizione = CommonHelper.GetParametro<string>(EnumParametriSistema.IndirizzoComnitNotFound).Replace("#corso", corso.codice_tbCorsiCodice).Replace("#titolo", corso.titolo_tbCorsiCodice);
                                    Notifiche.Add(item);
                                    NotificheTotaliTipo1 += 1;
                                }
                                else
                                {
                                    myRaiData.MyRai_ParametriSistema par = ParametriNotifiche.Where(a => a.Chiave == "IndirizzoComnit" + partecipanti.stato).FirstOrDefault();
                                    if (par != null)
                                    {
                                        item.id = 0;
                                        item.tipo = Convert.ToInt32(par.Valore2);
                                        string dataCorso = "";
                                        if (!string.IsNullOrEmpty(partecipanti.dataesito_test))
                                            dataCorso = partecipanti.dataesito_test.Substring(6, 2) + "/" + partecipanti.dataesito_test.Substring(4, 2) + "/" + partecipanti.dataesito_test.Substring(0, 4);

                                        item.descrizione = par.Valore1.Replace("#corso", corso.codice_tbCorsiCodice).Replace("#titolo", corso.titolo_tbCorsiCodice).Replace("#data", dataCorso);
                                        if (Convert.ToInt32(par.Valore2) == 1)
                                        {
                                            NotificheTotaliTipo1 += 1;
                                            //             NotificheTotali += 1;
                                            Notifiche.Add(item);

                                        }
                                        else
                                        {
                                            if (DateTime.Now == Convert.ToDateTime(dataCorso))
                                            {
                                                NotificheTotali += 1;
                                                Notifiche.Add(item);
                                            }

                                        }
                                    }
                                }
                            }
                            //&& statiNotValid.Contains(a.stato_tbCorsiPartecipanti)
                            /*if  && !statiNotValid.Contains(a.stato_tbCorsiPartecipanti)).Count() == 0)
                            {
                                myRaiData.tbCorsiCodice corsoCodice = db.tbCorsiCodice.Where(x => x.codice_tbCorsiCodice == corso).FirstOrDefault();
                                myRaiData.MyRai_Notifiche item= new myRaiData.MyRai_Notifiche();
                                item.id = 0;
                                item.tipo = 1;
                                item.descrizione = corsoCodice.titolo_tbCorsiCodice;
                                Notifiche.Add(item);
                                NotificheTotaliTipo1 += 1;
                                NotificheTotali += 1;
                            }
                            */
                        }

                    }
                }

                //notifiche completamento CV
                using (cv_ModelEntities dbCV = new cv_ModelEntities())
                {
                    var db = new myRaiData.digiGappEntities();
                    string matrCV = matr;



                    myRai.Data.CurriculumVitae.TCVLogin login = dbCV.TCVLogin.FirstOrDefault(x => x.Matricola == matrCV);
                    if (login != null)
                    {
                        int percCV = CommonHelper.GetPercentualCV(matrCV);
                        if (percCV == 100)
                        {
                            if (!db.MyRai_Notifiche.Any(x => x.matricola_destinatario == matrCV && x.categoria == "CV Online"))
                            {
                                MyRai_Notifiche notifica100 = new MyRai_Notifiche();
                                notifica100.categoria = "CV Online";
                                notifica100.tipo = 2;
                                notifica100.matricola_destinatario = matrCV;
                                notifica100.descrizione = "Complimenti per aver compilato il 100% del CV!";
                                notifica100.descrizione += "\r\n" + CommonHelper.GetParametro<string>(EnumParametriSistema.NotificaCV100);
                                notifica100.inserita_da = "Portale";
                                notifica100.data_inserita = DateTime.Now;
                                db.MyRai_Notifiche.Add(notifica100);
                                myRai.DataAccess.DBHelper.Save(db, matrCV);
                                Notifiche.Add(notifica100);
                                NotificheTotali += 1;
                            }
                        }
                        else
                        {
                            //se presente la notifica del 100 la tolgo
                            MyRai_Notifiche notifica100 = db.MyRai_Notifiche.FirstOrDefault(x => x.matricola_destinatario == matrCV && x.categoria == "CV Online");
                            if (notifica100 != null)
                            {
                                db.MyRai_Notifiche.Remove(notifica100);
                                DBHelper.Save(db, matrCV);
                                if (notifica100.data_letta == null)
                                {
                                    notifica100 = Notifiche.FirstOrDefault(x => x.matricola_destinatario == matrCV && x.categoria == "CV Online");
                                    Notifiche.Remove(notifica100);
                                    NotificheTotali -= 1;
                                }
                            }

                            //ha compilato almeno una parte del nuovo cv
                            myRaiData.MyRai_Notifiche notificaCV = new myRaiData.MyRai_Notifiche();
                            notificaCV.categoria = "CV Online";
                            notificaCV.tipo = 2;
                            notificaCV.id = 0;
                            notificaCV.descrizione = "<a href='/cv_online'>Completa la compilazione del CV</a>";
                            notificaCV.descrizione += CommonHelper.GetParametro<string>(EnumParametriSistema.NotificaCVLess100).Replace("\r\n", "<br/>");
                            Notifiche.Add(notificaCV);
                            NotificheTotali += 1;
                        }
                    }
                    else
                    {
                        //se presente la notifica del 100 la tolgo
                        MyRai_Notifiche notifica100 = db.MyRai_Notifiche.FirstOrDefault(x => x.matricola_destinatario == matrCV && x.categoria == "CV Online");
                        if (notifica100 != null)
                        {
                            db.MyRai_Notifiche.Remove(notifica100);
                            DBHelper.Save(db, matr);
                            if (notifica100.data_letta == null)
                            {
                                notifica100 = Notifiche.FirstOrDefault(x => x.matricola_destinatario == matrCV && x.categoria == "CV Online");
                                Notifiche.Remove(notifica100);
                                NotificheTotali -= 1;
                            }
                        }

                        //Non ha mai compilato il CV
                        myRaiData.MyRai_Notifiche notificaCV = new myRaiData.MyRai_Notifiche();
                        notificaCV.categoria = "CV Online";
                        notificaCV.tipo = 2;
                        notificaCV.id = 0;
                        notificaCV.descrizione = "<a href='/cv_online'>Compila il nuovo CV</a>";
                        notificaCV.descrizione += CommonHelper.GetParametro<string>(EnumParametriSistema.NotificaCVZero).Replace("\r\n", "<br/>");
                        Notifiche.Add(notificaCV);
                        NotificheTotali += 1;
                    }

                }
            }
            catch (Exception ex)
            {
            }
            Notifiche = Notifiche.OrderBy(a => a.id).ToList();

            if (Notifiche.Any(x => x.categoria == "CV Online" && x.id > 0))
            {
                int index = Notifiche.FindIndex(x => x.id > 0);
                if (index >= 0)
                {
                    MyRai_Notifiche c = Notifiche.FirstOrDefault(x => x.categoria == "CV Online");
                    Notifiche.Remove(c);
                    Notifiche.Insert(index, c);
                }
            }

        }
        public List<myRaiData.MyRai_Notifiche> Notifiche { get; set; }

        public int NotificheTotali { get; set; }
        public int NotificheTotaliTipo1 { get; set; }
        public int tipoNotifiche { get; set; }
        public List<myRaiData.MyRai_HeaderMenu> HeaderMenu { get; set; }
        public List<myRaiData.MyRai_ParametriSistema> ParametriNotifiche { get; set; }

        /// <summary>
        /// Nome dell'header da visualizzare
        /// per sviluppo verrà renderizzato l'header di RaiPlace,
        /// mentre per produzione finchè non verrà lanciato RaiPlace avremo
        /// l'header vecchio stile
        /// </summary>
        public string HeaderName
        {
            get
            {
                if (!String.IsNullOrEmpty(this._headerName))
                {
                    return this._headerName;
                }
                else if (SessionHelper.Get("HeaderName") == null)
                {
                    using (var db = new myRaiData.digiGappEntities())
                    {
                        var headerData = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("HeaderName", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                        if (headerData != null)
                        {
                            SessionHelper.Set("HeaderName", headerData.Valore1);
                            this._headerName = headerData.Valore1;
                            return this._headerName;
                        }
                        else
                        {
                            SessionHelper.Set("HeaderName", DEFAULT_HEADER);
                            this._headerName = DEFAULT_HEADER;
                            return this._headerName;
                        }
                    }
                }
                else
                {
                    this._headerName = HttpContext.Current.Session["HeaderName"].ToString();
                    return this._headerName;
                }
            }
        }

        private string _headerName { get; set; }
    }
}