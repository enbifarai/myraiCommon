using myRai.DataControllers;
using myRai.Models;
using myRaiData;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using TimbratureCore;
using static myRai.Controllers.ajaxController;
using myRaiHelper;
using myRaiCommonManager;
using myRaiCommonModel;

namespace myRai.Business
{
    public class WizardFsManager
    {
        public enum StatiWizardFS
        {
            Visualizzato,
            CambioOrarioRichiesto,
            CambioOrarioEseguito,
            EccezioniInserite,
            NonVisualizzare
        }

        public static bool CeitonObblEcc(string cod, DateTime D)
        {
            if (UtenteHelper.GestitoSirio())
            {
                var db = new digiGappEntities();
                var etr = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == cod).FirstOrDefault();
                return (etr != null && (etr.RichiedeAttivitaCeiton || (CeitonManager.ActivityAvailableToday(D) && EccezioniManager.IsEccezioneInGruppo2_3(cod))));
            }
            else return false;
        }
        public static int GetCaratteriObbEcc(string cod)
        {
            var db = new digiGappEntities();
            var cobb = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == cod).Select(x => x.CaratteriMotivoRichiesta).FirstOrDefault();
            if (cobb != null)
                return (int)cobb;
            else
                return 0;
        }

        public static String  CambiaStatoFS(string stato, DateTime data, string hmGapp, string hmInserito, string spostamento)
        {
            var db = new digiGappEntities();
            string matr = CommonHelper.GetCurrentUserMatricola();

            if (stato == StatiWizardFS.CambioOrarioRichiesto.ToString())
            {
                int idstato = db.MyRai_StatiWizardFS.Where(x => x.stato == stato).Select(x => x.id).FirstOrDefault();
                try
                {
                    var wiz = db.MyRai_WizardFS.Where(x => x.matricola == matr && x.data == data).FirstOrDefault();
                    if (wiz == null)
                    {
                        MyRai_WizardFS w = new MyRai_WizardFS()
                        {
                            data = data,
                            data_ultimo_aggiornamento = DateTime.Now,
                            matricola = matr,
                            Spostamento = spostamento,
                            orario_alla_richiesta = hmGapp.Replace(":", ""),
                            orario_richiesto = hmInserito.Replace(":", ""),
                            id_stato = idstato
                        };
                        db.MyRai_WizardFS.Add(w);
                    }
                    else
                    {
                        wiz.data_ultimo_aggiornamento = DateTime.Now;
                        wiz.id_stato = idstato;
                        wiz.orario_alla_richiesta = hmGapp.Replace(":", "");
                        wiz.orario_richiesto = hmInserito.Replace(":", "");
                    }

                    db.SaveChanges();
                    return  "OK";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

            return "Stato non contemplato";
        }
        public static SupportoFSmodel GetSupportoFSmodel(DateTime date, MyRaiServiceInterface.it.rai.servizi.digigappws.dayResponse resp,string matr)
        {
            var db = new digiGappEntities();
            
            SupportoFSmodel model = new SupportoFSmodel();
            model.AbilitatoGAVE = UtenteHelper.HaPatenteB() || UtenteHelper.HaPatenteC();
            model.AbilitatoGAVU = UtenteHelper.HaPatenteB() || UtenteHelper.HaPatenteC();
            model.AbilitatoGAPC = UtenteHelper.HaPatenteC();

            var wiz = db.MyRai_WizardFS.Where(x => x.matricola == matr && x.data == date).FirstOrDefault();

            if (wiz != null && wiz.id_stato == 2)
            {
                string dalleOrario = wiz.orario_alla_richiesta.Split('/')[0];
                string alleOrario = wiz.orario_alla_richiesta.Split('/')[1];

                string dalleOrarioOra = resp.orario.hhmm_entrata_48.Replace(":", "");
                string alleOrarioOra = resp.orario.hhmm_uscita_48.Replace(":", "");

                if (dalleOrario != dalleOrarioOra)
                {
                    wiz.id_stato = 3;
                    wiz.data_ultimo_aggiornamento = DateTime.Now;
                    db.SaveChanges();

                }
            }

            model.WizardFS_db = wiz;
            return model;
        }
        public static MyRaiServiceInterface.it.rai.servizi.digigappws.Timbrature CreaTimbratura(string hhmm1, string hhmm2)
        {
            MyRaiServiceInterface.it.rai.servizi.digigappws.Timbrature T = new MyRaiServiceInterface.it.rai.servizi.digigappws.Timbrature();
            T.entrata = new MyRaiServiceInterface.it.rai.servizi.digigappws.Timbratura();
            T.entrata.orario = hhmm1;
            T.uscita = new MyRaiServiceInterface.it.rai.servizi.digigappws.Timbratura();
            T.uscita.orario = hhmm2;
            return T;
        }
        public static ProposteAutomaticheModel ElaboraEccezioni(string data, string oraFrom, string oraTo, string spostamento, int smapTestaMinuti)
        {
            DateTime D;
            DateTime.TryParseExact(data, "dd/MM/yyyy", null, DateTimeStyles.None, out D);
                

            WSDigigappDataController service = new WSDigigappDataController();

            MyRaiServiceInterface.it.rai.servizi.digigappws.dayResponse resp = service.GetEccezioni( CommonHelper.GetCurrentUserMatricola( ), CommonHelper.GetCurrentUserMatricola(), D.ToString("ddMMyyyy"), "BU", 70);
            var db = new digiGappEntities();

            ProposteAutomaticheModel PAmodel = new ProposteAutomaticheModel();
            List<MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione> LE = new List<MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione>();

            if (smapTestaMinuti > 0)
            {
                LE.Add(new MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione()
                {
                    cod = "SMAP",
                    dalle=oraFrom,
                    alle=oraTo,
                    qta = smapTestaMinuti.ToHHMM(),
                    data = D.ToString("ddMMyyyy"),
                    descrittiva_lunga = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione == "SMAP").Select(x => x.desc_eccezione).FirstOrDefault(),
                    CaratteriObbligatoriNota = WizardFsManager.GetCaratteriObbEcc("SMAP"),
                    RichiedeAttivitaCeiton = WizardFsManager.CeitonObblEcc("SMAP", D)
                });
            }
            if (spostamento != null && spostamento.ToLower() == "guida")
            {
                if (UtenteHelper.HaPatenteB() || UtenteHelper.HaPatenteC())
                {
                    LE.Add(new MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione()
                    {
                        cod = "GAVE",
                        qta = "1",
                        data = D.ToString("ddMMyyyy"),
                        descrittiva_lunga = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione == "GAVE").Select(x => x.desc_eccezione).FirstOrDefault(),
                        CaratteriObbligatoriNota = WizardFsManager.GetCaratteriObbEcc("GAVE"),
                        RichiedeAttivitaCeiton = WizardFsManager.CeitonObblEcc("GAVE", D)
                    });

                    LE.Add(new MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione()
                    {
                        cod = "GAVU",
                        qta = "1",
                        data = D.ToString("ddMMyyyy"),
                        descrittiva_lunga = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione == "GAVU").Select(x => x.desc_eccezione).FirstOrDefault(),
                        CaratteriObbligatoriNota = WizardFsManager.GetCaratteriObbEcc("GAVU"),
                        RichiedeAttivitaCeiton = WizardFsManager.CeitonObblEcc("GAVU", D)
                    });
                }

                if ( UtenteHelper.HaPatenteC())
                {
                    LE.Add(new MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione()
                    {
                        cod = "GAPC",
                        qta = "1",
                        data = D.ToString("ddMMyyyy"),
                        descrittiva_lunga = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione == "GAPC").Select(x => x.desc_eccezione).FirstOrDefault(),
                        CaratteriObbligatoriNota = WizardFsManager.GetCaratteriObbEcc("GAPC"),
                        RichiedeAttivitaCeiton = WizardFsManager.CeitonObblEcc("GAPC", D)
                    });
                }

            }
            if (spostamento != null && spostamento.ToLower() == "mezzo" && "LT".Contains( UtenteHelper.TipoDipendente()))
            {
                string matr = CommonHelper.GetCurrentUserMatricola();
                //List<Models.ess.DettaglioTrasfertaVM> trasf = TrasferteManager.GetTrasferteForDay(D, CommonHelper.GetCurrentUserMatricola());
                var bigl= db.MyRai_Trasferte_ProgViaggio.Where(x => x.PROCID == "00" + matr && x.PRODDATAIN == data.Replace("/", "")).ToList();
                
                if (bigl.Any())
                {
                    foreach (var b in bigl)
                    {
                        if (!string.IsNullOrWhiteSpace(b.PRODORAIN))
                        {
                            int minInBiglietto = b.PRODORAIN.Substring(0, 4).Insert(2, ":").ToMinutes();
                            int minInizioOrario = resp.orario.hhmm_entrata_48.ToMinutes();
                            if (minInizioOrario < minInBiglietto)
                            {
                                LE.Add(new MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione()
                                {
                                    cod = "ORV",
                                    dalle= minInizioOrario.ToHHMM(),
                                    alle= minInBiglietto.ToHHMM (),
                                    qta = (minInBiglietto - minInizioOrario).ToHHMM() ,
                                    data = D.ToString("ddMMyyyy"),
                                    descrittiva_lunga = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione == "ORV").Select(x => x.desc_eccezione).FirstOrDefault(),
                                    CaratteriObbligatoriNota = WizardFsManager.GetCaratteriObbEcc("ORV"),
                                    RichiedeAttivitaCeiton = WizardFsManager.CeitonObblEcc("ORV", D)
                                });
                                break;
                            }
                        }
                    }
                }
            }
            if (oraTo.ToMinutes() > resp.orario.hhmm_uscita_48.ToMinutes())
            {
                MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione e = new MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione()
                {
                    cod = "SMAP",
                    dalle = resp.orario.hhmm_uscita_48.Trim(),
                    alle = oraTo.Trim(),
                    qta = (oraTo.ToMinutes() - resp.orario.hhmm_uscita_48.ToMinutes()).ToHHMM(),
                    data = D.ToString("ddMMyyyy"),
                    descrittiva_lunga = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione == "SMAP").Select(x => x.desc_eccezione).FirstOrDefault()
                };
                if ( UtenteHelper.GestitoSirio())
                {
                    var etr = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == e.cod).FirstOrDefault();
                    e.RichiedeAttivitaCeiton = etr != null &&
                        (etr.RichiedeAttivitaCeiton || (CeitonManager.ActivityAvailableToday(D) && EccezioniManager.IsEccezioneInGruppo2_3(e.cod)));
                }
                var cobb = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == e.cod).Select(x => x.CaratteriMotivoRichiesta).FirstOrDefault();
                if (cobb != null)
                    e.CaratteriObbligatoriNota = (int)cobb;

                LE.Add(e);
            }

            List<MyRai_Eccezioni_Ammesse> EccezioniAmmesse = EccezioniManager.GetListaEccezioniPossibili( CommonHelper.GetCurrentUserMatricola(), D).OrderBy(x => x.cod_eccezione).ToList();
            List<string> Lmagg = new List<string>();
            foreach (var ec in EccezioniAmmesse)
            {
                if (DayAnalysisFactory.DayAnalysisExists(ec.cod_eccezione))
                {
                    Lmagg.Add(ec.cod_eccezione);
                }
            }
            if (Lmagg.Any())
            {
                var smap = LE.Where(x => x.cod == "SMAP").FirstOrDefault();
                if (smap != null)
                {
                    var list = resp.eccezioni.ToList();
                    list.Add(smap);
                    resp.eccezioni = list.ToArray();
                }

                resp.eccezioni = resp.eccezioni.ToList().Where(x => x.cod.Trim() != "FS").ToArray();

                resp.timbrature = new List<MyRaiServiceInterface.it.rai.servizi.digigappws.Timbrature>() { CreaTimbratura (oraFrom,oraTo )}.ToArray();

                foreach (string codice in Lmagg)
                {
                    
                    DayAnalysisBase da = DayAnalysisFactory.GetDayAnalysisClass(codice.ToUpper(), resp, UtenteHelper.GetQuadratura() == Quadratura.Giornaliera);
                    var quan = da.GetEccezioneQuantita();
                    if (quan.QuantitaMinuti > 0)
                    {
                        LE.Add(new MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione()
                        {
                            cod = codice,
                            qta = quan.QuantitaMinutiHHMM,
                            data = D.ToString("ddMMyyyy"),
                            descrittiva_lunga = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione == codice).Select(x => x.desc_eccezione).FirstOrDefault(),
                            CaratteriObbligatoriNota = WizardFsManager.GetCaratteriObbEcc(codice),
                            RichiedeAttivitaCeiton = WizardFsManager.CeitonObblEcc(codice, D)
                        });
                    }
                }
            }

            PAmodel.EccezioniProposte = LE.ToArray();
            return PAmodel;
        }
    }
}