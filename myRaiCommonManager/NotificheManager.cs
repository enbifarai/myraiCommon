
using System;
using System.Collections.Generic;
using System.Linq;
using myRai.Business;
using myRai.DataAccess;
using myRai.Models;
using myRaiCommonModel;
using myRaiData;
using myRaiDataTalentia;
using myRaiHelper;

namespace myRaiCommonManager
{

    public class NotificheManager
    {

        public static Boolean InserisciNotifica(string desc, string categoria, string matricolaDest, string inserita_da, long idRiferimento, string matricola = null, int? tipo=null)
        {

            if (desc.StartsWith("Inserimento") && matricolaDest == inserita_da) return true;

            myRaiData.MyRai_Notifiche n = new myRaiData.MyRai_Notifiche()
            {
                categoria = categoria,
                data_inserita = DateTime.Now,
                descrizione = desc,
                inserita_da = inserita_da,
                matricola_destinatario = matricolaDest,
                id_riferimento = idRiferimento,
                email_destinatario = CommonHelper.GetEmailPerMatricola(matricolaDest),
                tipo=tipo
            };

            digiGappEntities db = new digiGappEntities();
            db.MyRai_Notifiche.Add(n);

            // se matricola è valorizzata allora l'operazione è chiamata
            // da un batch che non ha l'oggetto utente valorizzato e la matricola
            // gli verrà passata dall'esterno
            if ( matricola != null)
            {
                return DBHelper.SaveNoSession ( db , matricola );
            }
            else
            {
                return DBHelper.Save ( db, CommonHelper.GetCurrentUserMatricola());
            }
        }
        public static Boolean ModificaNotifica(string desc, string categoria, string matricolaDest, string inserita_da, long idRiferimento)
        {
            digiGappEntities db = new digiGappEntities();
            MyRai_Notifiche notifica = db.MyRai_Notifiche.Where(a => a.id_riferimento == idRiferimento && a.matricola_destinatario == matricolaDest).FirstOrDefault();
            notifica.categoria = categoria;
            notifica.data_inserita = DateTime.Now;
            notifica.descrizione = desc;
            notifica.inserita_da = inserita_da;
            notifica.data_letta = null;
            notifica.matricola_destinatario = matricolaDest;
            notifica.id_riferimento = idRiferimento;
            notifica.email_destinatario = CommonHelper.GetEmailPerMatricola(matricolaDest);

            return DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
        }

        public static void NotificaPerInserimentoRichiesta_o_Storno(long IdRichiestaPadre, EnumCategoriaNotifica tipo)
        {
            digiGappEntities db = new digiGappEntities();
            try
            {
                
                MyRai_Richieste Richiesta = db.MyRai_Richieste.Where(x => x.id_richiesta == IdRichiestaPadre).FirstOrDefault();
                if (Richiesta == null) return;

                MyRai_Eccezioni_Richieste eccRich = Richiesta.MyRai_Eccezioni_Richieste.FirstOrDefault();
                if (eccRich == null) return;




                List<string> matricoleLiv1 = new List<string>();
                if (!String.IsNullOrWhiteSpace(Utente.Reparto()) && Utente.Reparto() != "00")
                {
                    matricoleLiv1 = DaFirmareManager.GetMatricolaLivelloPerSede(Utente.SedeGapp(Richiesta.periodo_dal) + Utente.Reparto(), 1);
                }
                if (matricoleLiv1 ==null || matricoleLiv1.Count ==0)
                    matricoleLiv1 = DaFirmareManager.GetMatricolaLivelloPerSede(Utente.SedeGapp(Richiesta.periodo_dal), 1);


                if (matricoleLiv1 != null && matricoleLiv1.Count > 0)
                {
                    foreach (string matricolaLiv1 in matricoleLiv1)
                    {
                        string ml1=matricolaLiv1;
                        if (ml1!=null &&  ml1.ToUpper().StartsWith("P")) ml1 = ml1.Substring(1);
                        
                        myRaiData.MyRai_Notifiche n = new myRaiData.MyRai_Notifiche();
                        n.categoria = tipo.ToString();
                        n.data_inserita = DateTime.Now;
                        n.id_riferimento = IdRichiestaPadre;

                        n.matricola_destinatario = ml1;
                        n.email_destinatario = CommonManager.GetEmailPerMatricola(ml1);
                        if (String.IsNullOrWhiteSpace(n.email_destinatario)) n.email_destinatario = "P" + ml1 + "@rai.it";
                        n.inserita_da = CommonManager.GetCurrentUserMatricola();

                        string codiceEcc = eccRich.cod_eccezione;
                        n.descrizione = "Inserimento " + (tipo == EnumCategoriaNotifica.InsRichiesta ? "richiesta " : "storno ")
                            + codiceEcc + " per il " + Richiesta.periodo_dal.ToString("dd/MM/yyyy") +
                            (Richiesta.periodo_dal != Richiesta.periodo_al
                            ? "-" + Richiesta.periodo_al.ToString("dd/MM/yyyy")
                            : "") +
                            " da " + Utente.Nominativo();

                        if (n.matricola_destinatario != n.inserita_da)
                        {
                        db.MyRai_Notifiche.Add(n);
						}

                    }
                    DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
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
                    provenienza = "NotificaPerInserimentoRichiesta_o_Storno"
                });
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


        public static List<NotificaPlus> GetNotificheCorsi(string matricola, int tipo = 0, bool addDetails = false, string homeUrl = "")
        {
            List<NotificaPlus> result = new List<NotificaPlus>();

            bool checkOldcorsoFGS = false;

            if (CommonManager.IsProduzione())
            {
                var dbTal = new TalentiaEntities();
                XR_ACA_PARAM paramNotifiche = dbTal.XR_ACA_PARAM.FirstOrDefault(x => x.COD_PARAM == "AbilitaNotifiche");

                if (paramNotifiche != null && paramNotifiche.COD_VALUE1 == "TRUE" && !String.IsNullOrWhiteSpace(paramNotifiche.COD_VALUE2))
                {
                    XR_ACA_PARAM paramUrl = dbTal.XR_ACA_PARAM.FirstOrDefault(x => x.COD_PARAM == "LinkCorso");

                    var sint = dbTal.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
                    if (sint != null)
                    {
                        var elencoCorsi = paramNotifiche.COD_VALUE2.Split(',');
                        foreach (var codCorso in elencoCorsi)
                        {
                            int idCorso = 0;
                            if (!Int32.TryParse(codCorso, out idCorso))
                                continue;

                            //Corso FGS Sicurezza - Corrispondente al corso 04141
                            if (idCorso == 132917648)
                            {
                                var verifyOldTest = dbTal.Database.SqlQuery<string>("SELECT [matricola] FROM[HCMDB9].[dbo].[V_XR_CORSO_FGS_TEST] where matricola='" + matricola + "'");
                                if (verifyOldTest != null && verifyOldTest.Any())
                                {
                                    checkOldcorsoFGS = true;
                                    continue;
                                }
                            }

                            int idEdizione = dbTal.EDIZIONE.Where(x => x.ID_CORSO == idCorso && x.DTA_INIZIO <= DateTime.Today && x.DTA_FINE >= DateTime.Today).Select(x => x.ID_EDIZIONE).FirstOrDefault();

                            NotificaPlus notificaPlus = null;
                            var currform = dbTal.CURRFORM.FirstOrDefault(x => x.ID_PERSONA == sint.ID_PERSONA && x.ID_EDIZIONE == idEdizione);
                            if (currform != null)
                            {
                                string urlCorso = homeUrl + paramUrl.COD_VALUE1.Replace("#idCorso", codCorso);

                                string chiaveParametro = "";
                                int tipoNot = 1;
                                if (currform.QTA_COMPLETION == 0)
                                {
                                    tipoNot = 1;
                                    chiaveParametro = "Notifica0";
                                }
                                else if (currform.QTA_COMPLETION < 100)
                                {
                                    tipoNot = 1;
                                    chiaveParametro = "NotificaLess100";
                                }
                                else if (currform.IND_PARTICIPANT == "N")
                                {
                                    tipoNot = 1;
                                    chiaveParametro = "NotificaTest";
                                }
                                else
                                {
                                    //la notifica di completamento viene inserita dal batch
                                    continue;
                                    //tipoNot = 2;
                                    //chiaveParametro = "Notifica100";
                                }

                                var notifica = dbTal.XR_ACA_PARAM.FirstOrDefault(x => x.COD_PARAM == chiaveParametro && x.COD_VALUE1.Contains(codCorso));
                                if (notifica == null)
                                    notifica = dbTal.XR_ACA_PARAM.FirstOrDefault(x => x.COD_PARAM == chiaveParametro && x.COD_VALUE1 == "*");

                                if (notifica != null && !String.IsNullOrWhiteSpace(notifica.COD_VALUE2))
                                {
                                    MyRai_Notifiche item = new MyRai_Notifiche();
                                    item.id = 0;
                                    item.tipo = tipoNot;
                                    item.categoria = "Corsi Obbligatori";

                                    item.descrizione = notifica.COD_VALUE2.Replace("#linkCorso", urlCorso).Replace("#titoloCorso", currform.EDIZIONE.CORSO.COD_CORSO);
                                    notificaPlus = new NotificaPlus() { notifica = item, richiesta = null };

                                }

                                if (notificaPlus != null && (tipo == 0 || tipo == notificaPlus.notifica.tipo))
                                {
                                    if (addDetails)
                                    {
                                        notificaPlus.Dettaglio = new NotificaDettaglio()
                                        {
                                            Title = currform.EDIZIONE.CORSO.COD_CORSO,
                                            AnchorHref = urlCorso
                                        };
                                    }
                                    result.Add(notificaPlus);
                                }
                            }
                        }
                    }
                }
                else
                    checkOldcorsoFGS = true;
            }

            if (checkOldcorsoFGS)
            {
                CorsiJobEntities dbCorsi = new CorsiJobEntities();
                digiGappEntities db = new digiGappEntities();

                string codiceCorso = CommonManager.GetParametro<string>(EnumParametriSistema.CorsoOnline).ToString();
                var listcorsi = dbCorsi.tbCorsiCodice.Where(a => a.codice_tbCorsiCodice == codiceCorso).Select(a => new { a.codice_tbCorsiCodice, a.titolo_tbCorsiCodice }).Distinct().ToList();
                myRaiServiceHub.it.rai.servizi.hrgb.Service wsAnag = new myRaiServiceHub.it.rai.servizi.hrgb.Service();
                //string str_temp = wsAnag.EsponiAnagrafica("RAICV;" + CommonManager.GetCurrentUserMatricola() + ";;E;0");
                string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(matricola);
                string[] temp = str_temp.ToString().Split(';');
                string codiceContratto = "";
                string societa = "";
                string qualifica = "";
                //temp = null; //freak - forzatura
                if ((temp != null) && (temp.Count() > 16))
                {
                    codiceContratto = temp[5];
                    societa = temp[14];
                    qualifica = temp[25];
                }

                if (temp != null && !qualifica.StartsWith("A") && !qualifica.StartsWith("M71"))//  temp[25].Substring(0, 1) != "A" && temp[25].Substring(0, 3) != "M71")
                {
                    if (CommonManager.GetParametro<string>(EnumParametriSistema.TipiContrattoCorsiOlnlie).Contains(codiceContratto)
                        && CommonManager.GetParametro<string>(EnumParametriSistema.SocietaCorsiOnline).Contains(societa))
                    {
                        foreach (var corso in listcorsi)
                        {
                            tblPartecipantiOnline partecipanti = dbCorsi.tblPartecipantiOnline.Where(a => a.matricola == "P" + matricola && a.codice == corso.codice_tbCorsiCodice).FirstOrDefault();
                            MyRai_Notifiche item = new myRaiData.MyRai_Notifiche();

                            NotificaPlus notificaPlus = null;

                            if (partecipanti == null)
                            {
                                item.id = 0;
                                item.tipo = 1;
                                item.categoria = "Corsi Obbligatori";

                                string parametro = CommonManager.GetParametro<string>(EnumParametriSistema.IndirizzoComnitNotFound);
                                if (!String.IsNullOrWhiteSpace(parametro))
                                {
                                    item.descrizione = parametro.Replace("#corso", corso.codice_tbCorsiCodice).Replace("#titolo", corso.titolo_tbCorsiCodice);
                                    notificaPlus = new NotificaPlus() { notifica = item, richiesta = null };
                                }
                            }
                            else
                            {
                                myRaiData.MyRai_ParametriSistema par = db.MyRai_ParametriSistema.Where(a => a.Chiave == "IndirizzoComnit" + partecipanti.stato).FirstOrDefault();
                                if (par != null)
                                {
                                    item.id = 0;
                                    item.tipo = Convert.ToInt32(par.Valore2);
                                    item.categoria = "Corsi Obbligatori";
                                    string dataCorso = "";
                                    if (!string.IsNullOrEmpty(partecipanti.dataesito_test))
                                        dataCorso = partecipanti.dataesito_test.Substring(6, 2) + "/" + partecipanti.dataesito_test.Substring(4, 2) + "/" + partecipanti.dataesito_test.Substring(0, 4);

                                    item.descrizione = par.Valore1.Replace("#corso", corso.codice_tbCorsiCodice).Replace("#titolo", corso.titolo_tbCorsiCodice).Replace("#data", dataCorso);
                                    if (Convert.ToInt32(par.Valore2) == 1)
                                        notificaPlus = new NotificaPlus() { notifica = item, richiesta = null };
                                    else
                                    {
                                        if (DateTime.Now.ToShortDateString() == dataCorso)
                                            notificaPlus = new NotificaPlus() { notifica = item, richiesta = null };

                                    }
                                }
                            }

                            if (notificaPlus != null && (tipo == 0 || tipo == notificaPlus.notifica.tipo))
                            {
                                if (addDetails)
                                {
                                    notificaPlus.Dettaglio = new NotificaDettaglio()
                                    {
                                        Title = corso.titolo_tbCorsiCodice,
                                        AnchorHref = notificaPlus.notifica.descrizione.Substring(notificaPlus.notifica.descrizione.IndexOf("href") + 6, notificaPlus.notifica.descrizione.IndexOf("target") - 11)
                                    };
                                }
                                result.Add(notificaPlus);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}