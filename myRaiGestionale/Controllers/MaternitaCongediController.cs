using ClosedXML.Excel;
using myRaiCommonManager;
using myRaiCommonManager._Model;
using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    /*
     ADM	Amministratore	P103650,P909317
01VIS	Amministrazione: sola lettura	213525
01GEST	Amministrazione: lettura/scrittura	P912685,P652740
01ADM	Amministrazione: referente/responsabile	P652740
01RES	Amminstrazione: gestione residenza retroattiva	
02VIS	Gestione: sola lettura	
02GEST	Gestione: lettura/scrittura	P103650,P909317
02ADM	Gestione: refente/responsabile	P103650
03VIS	Ufficio del Personale: sola lettura	
03GEST	Ufficio del Personale: lettura/scrittura	P451598
03ADM	Ufficio del Personale: referente/responsabile	
04VIS	Formazione: sola lettura	
04GEST	Formazione: lettura/scrittura	
04ADM	Formazione: referente/responsabile	
         */
    public class MaternitaCongediController: Controller
    {
        public MaternitaCongediController()

        {

        }
        public ActionResult Index(int? idrichiesta, int? dar)
        {


            MaternitaIndexModel model = new MaternitaIndexModel();
            model = myRaiCommonManager.MaternitaCongediManager.GetMaternitaIndexModel();
            model.MostraSoloDaRiavviare = (dar == 1);

            if (idrichiesta != null)
            {
                var db = new IncentiviEntities();
                var Rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
                if (Rich != null)
                {
                    if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Gestione))
                    {
                        model.JSstringApriRichiesta = "VisualizzaGestione(" + idrichiesta + ")";
                    }
                    if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Personale))
                    {
                        model.JSstringApriRichiesta = "VisualizzaUffPersonale(" + idrichiesta + ")";
                    }
                    if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Amministrazione))
                    {
                        model.JSstringApriRichiesta = "VisualizzaAmm('" + Rich.XR_MAT_CATEGORIE.TITOLO + "'," + idrichiesta + ")";
                    }
                }
            }
            return View(model);
        }

        public ActionResult EliminaForzaPratica(int id)
        {
            try
            {
                var db = new IncentiviEntities();
                var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == id).FirstOrDefault();
                rich.FORZA_ECCEZIONE_PRATICA = false;
                EliminaEccezioniSalvate(id, db);
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }
        }
        public ActionResult ForzaPratica(int id)
        {
            try
            {
                var db = new IncentiviEntities();
                var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == id).FirstOrDefault();
                rich.FORZA_ECCEZIONE_PRATICA = true;
                EliminaEccezioniSalvate(id, db);
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }
        }
        public void EliminaEccezioniSalvate(int idRichiesta, IncentiviEntities db)
        {
            var eccezioni = db.XR_MAT_ECCEZIONI.Where(x => x.ID_RICHIESTA == idRichiesta).ToList();
            if (!eccezioni.Any()) return;

            foreach (var e in eccezioni)
            {
                db.XR_MAT_ECCEZIONI.Remove(e);
            }
        }
        public ActionResult SetStatoEccezioneRipristina(string data, string eccezione, int idrichiesta)
        {
            var db = new IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            string stato = CommonHelper.GetParametro<string>(EnumParametriSistema.StatoEccezioneRipristinaCongedi);

            MyRaiServiceInterface.MyRaiServiceReference1.SetStatoEccezioneResponse Response = null;

            try
            {
                Response = MaternitaCongediManager.SetStatoEccezione(data, eccezione, rich.MATRICOLA, stato);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }

            if (Response.esito == true)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = Response.error }
                };
            }
        }
        public ActionResult SetStatoEccezioneEliminata(string data, string eccezione, int idrichiesta)
        {
            var db = new IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();

            string stato = CommonHelper.GetParametri<string>(EnumParametriSistema.StatoEccezioneCongedi)[1];
            MyRaiServiceInterface.MyRaiServiceReference1.SetStatoEccezioneResponse Response = null;

            try
            {
                Response = MaternitaCongediManager.SetStatoEccezione(data, eccezione, rich.MATRICOLA, stato);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }

            if (Response.esito == true)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = Response.error }
                };
            }
        }
        public ActionResult SetStatoEccezioneSospesa(string data, string eccezione, int idrichiesta)
        {
            var db = new IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();

            string stato = CommonHelper.GetParametri<string>(EnumParametriSistema.StatoEccezioneCongedi)[0];
            MyRaiServiceInterface.MyRaiServiceReference1.SetStatoEccezioneResponse Response = null;

            try
            {
                Response = MaternitaCongediManager.SetStatoEccezione(data, eccezione, rich.MATRICOLA, stato);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }

            if (Response.esito == true)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = Response.error }
                };
            }
        }

        public ActionResult cprat(int id)
        {

            if (CommonHelper.GetParametro<string>(EnumParametriSistema.MatricoleCancellazionePratiche).Split(',')
                .Contains(CommonHelper.GetCurrentUserMatricola()) == false)
            {
                return Content("NON AUTORIZZATO");
            }

            string esito = MaternitaCongediManager.cprat(id);

            if (esito == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = esito }
                };
            }


        }
        public ActionResult UpdateDB2(int id, string set)
        {
            if (!UtenteHelper.IsAdmin())
                throw new Exception("NON AUTORIZZATO");
            var esito = UpdateDB2Congedi(id,set);
            string es = Newtonsoft.Json.JsonConvert.SerializeObject(esito);
            return Content(es);
        }
        public ActionResult Deldb2(int id)
        {
            if (!UtenteHelper.IsAdmin())
                throw new Exception("NON AUTORIZZATO");

            var esito = DeleteFromDB2congedi(id);
            string es = Newtonsoft.Json.JsonConvert.SerializeObject(esito);
            return Content(es);

        }
        public ActionResult AnnullaApprovazioneRespGestione(int id)
        {
            if (!UtenteHelper.IsAdmin())
                throw new Exception("NON AUTORIZZATO");

            var db = new myRaiData.Incentivi.IncentiviEntities();
            var ric = db.XR_MAT_RICHIESTE.Where(x => x.ID == id).FirstOrDefault();
            ric.PRESA_VISIONE_RESP_GEST = null;
            ric.PRESA_VISIONE_RESP_MATR = null;
            foreach (var ts in ric.XR_MAT_TASK_DI_SERVIZIO
                .Where(x => x.XR_MAT_ELENCO_TASK.NOME_TASK == "INSERIMENTO ECCEZIONI").ToList())
                db.XR_MAT_TASK_DI_SERVIZIO.Remove(ts);

            DateTime D1 = ric.INIZIO_GIUSTIFICATIVO != null ? ric.INIZIO_GIUSTIFICATIVO.Value : ric.DATA_INIZIO_MATERNITA.Value;
            DateTime D2 = ric.FINE_GIUSTIFICATIVO != null ? ric.FINE_GIUSTIFICATIVO.Value : ric.DATA_FINE_MATERNITA.Value;


            var ArrDip = db.XR_MAT_ARRETRATI_DIPENDENTE.Where(x => x.MATRICOLA == ric.MATRICOLA &&
                            x.ECCEZIONE.StartsWith(ric.ECCEZIONE)
                            && x.DATA >= D1 && x.DATA <= D2).ToList();
            foreach (var a in ArrDip)
            {
                db.XR_MAT_ARRETRATI_DIPENDENTE.Remove(a);
                DeleteFromDB2congedi(a.ID);
            }

            db.SaveChanges();


            return Content("OK");
        }
        public ActionResult ApprovaResponsabileGestione(int idRichiesta)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            var ric = db.XR_MAT_RICHIESTE.Where(x => x.ID == idRichiesta).FirstOrDefault();
            if (ric == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Errore dati DB" }
                };
            }
            try
            {
                ric.PRESA_VISIONE_RESP_GEST = DateTime.Now;
                ric.PRESA_VISIONE_RESP_MATR = CommonHelper.GetCurrentUserMatricola();
                string ecc = ric.ECCEZIONE;
                //if (ecc == "MG")
                //{
                //    ric.PERMESSO_FRUIBILE = true;
                //    db.SaveChanges();

                //    return new JsonResult
                //    {
                //        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                //        Data = new { esito = true }
                //    };
                //}
                if (ric.XR_MAT_CATEGORIE.CAT == "CON")
                {
                    if (!ric.XR_MAT_PIANIFICAZIONI.Any() ||
                        !ric.XR_MAT_PIANIFICAZIONI.OrderByDescending(x => x.DATA_INSERIMENTO).FirstOrDefault().XR_MAT_GIORNI_CONGEDO.Any()
                        )
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { esito = false, errore = "Pianificazione o date eccezioni non presenti" }
                        };
                    }

                    var pian = ric.XR_MAT_PIANIFICAZIONI.OrderByDescending(x => x.DATA_INSERIMENTO).FirstOrDefault();

                    if (ric.PIANIFICAZIONE_BASE_ORARIA == true)
                    {
                        var tot = pian.XR_MAT_GIORNI_CONGEDO.Sum(x => x.FRUIZIONE_DECIMAL);
                        if (tot != ric.NUMERO_GIORNI_GIUSTIFICATIVO)
                        {
                            return new JsonResult
                            {
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                                Data = new { esito = false, errore = "Numero di giorni pianificati totali non coincidenti" }
                            };
                        }
                        foreach (var GiornoCongedo in pian.XR_MAT_GIORNI_CONGEDO)
                        {
                            ecc = ric.ECCEZIONE;
                            if (GiornoCongedo.FRUIZIONE != "I")
                            {
                                if (GiornoCongedo.FRUIZIONE_DECIMAL == (decimal)0.50)
                                    ecc += (GiornoCongedo.FRUIZIONE_TURNO == "I" ? "M" : "P");
                                else if (GiornoCongedo.FRUIZIONE_DECIMAL == (decimal)0.25)
                                    ecc += "Q";
                            }

                            var TS = MaternitaCongediManager.GetTaskServizio_InserimentoEccezioni(
                            db,
                            ecc,
                            new List<DateTime> { GiornoCongedo.DATA },
                            idRichiesta,
                            GiornoCongedo.DATA.Year,
                            GiornoCongedo.DATA.Month);

                            db.XR_MAT_TASK_DI_SERVIZIO.Add(TS);
                        }
                    }
                    else //non base oraria
                    {
                        var TS = MaternitaCongediManager.GetTaskServizio_InserimentoEccezioni(
                        db,
                        ecc,
                        pian.XR_MAT_GIORNI_CONGEDO.Select(x => x.DATA).OrderBy(z => z).ToList(),
                        idRichiesta,
                        pian.XR_MAT_GIORNI_CONGEDO.OrderBy(x => x.DATA).Select(z => z.DATA.Year).FirstOrDefault(),
                        pian.XR_MAT_GIORNI_CONGEDO.OrderBy(x => x.DATA).Select(z => z.DATA.Month).FirstOrDefault());

                        db.XR_MAT_TASK_DI_SERVIZIO.Add(TS);
                    }
                }
                else
                {
                    //MAT
                    List<DateTime> ListaDate = new List<DateTime>();
                    DateTime Dcurrent = ric.DATA_INIZIO_MATERNITA.Value;
                    ListaDate.Add(Dcurrent);
                    Dcurrent = Dcurrent.AddDays(1);

                    while (Dcurrent <= ric.DATA_FINE_MATERNITA)
                    {
                        ListaDate.Add(Dcurrent);
                        Dcurrent = Dcurrent.AddDays(1);
                    }

                    var TS = MaternitaCongediManager.GetTaskServizio_InserimentoEccezioni(
                               db,
                               ecc,
                               ListaDate,
                               idRichiesta,
                               ListaDate.OrderBy(x => x).Select(z => z.Year).FirstOrDefault(),
                               ListaDate.OrderBy(x => x).Select(z => z.Month).FirstOrDefault());

                    db.XR_MAT_TASK_DI_SERVIZIO.Add(TS);
                }

                //aggiungi in XR_MAT_ARRETRATI_DIPENDENTE-------------------------------------------------
                EsitoInserimentoArretrati EsitoArretrati = AggiungiSuArretratiDipendente(idRichiesta, db);
                if (!String.IsNullOrWhiteSpace(EsitoArretrati.Error))
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = EsitoArretrati.Error }
                    };
                }
                if (EsitoArretrati.ListArr == null || !EsitoArretrati.ListArr.Any())
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = "Nessuna eccezione da aggiungere" }
                    };
                }

                db.SaveChanges();// punto unico salvataggio (servono gli ID per la tabella DB")

                //aggiungi in DB2 -------------------------------------------------------------------------------
                EsitoInserimentoCongedi CongediInseriti = InserisciInDb2DaArretratiDip(EsitoArretrati.ListArr, idRichiesta);
                if (CongediInseriti.Error != null)
                {
                    AnnullaApprovazioneRespGestione(idRichiesta);
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = CongediInseriti.Error }
                    };
                }
                return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { esito = true } };
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    provenienza = "ApprovaRespGest",
                    error_message = ex.ToString()
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }


        }
        public EsitoCongedoDB2 UpdateDB2Congedi(int IDcong, string SetString)
        {
            EsitoCongedoDB2 esito = new EsitoCongedoDB2()
            {
                InsDel = "U"
            };
            string prefix = CommonHelper.IsProduzione() ? "DB2R.PROD" : "DB2P.PROVA";
            var db = new IncentiviEntities();
            try
            {
                string sql = $"UPDATE OPENQUERY (DB2LINK, ' SELECT * from {prefix}.EMEN_TB_CONG_PAR where ID_CONGEDO={IDcong} ')"+
                             $" SET " + SetString;
                esito.Query = sql;

                esito.Rows = db.Database.ExecuteSqlCommand(sql);
                esito.Success = esito.Rows == 1;
            }
            catch (Exception ex)
            {
                esito.Success = false;
                esito.Error = ex.Message;
            }
            return esito;
        }
        public EsitoCongedoDB2 DeleteFromDB2congedi(int IDcong)
        {
            EsitoCongedoDB2 esito = new EsitoCongedoDB2()
            {
                InsDel = "D"
            };

            string prefix = CommonHelper.IsProduzione() ? "DB2R.PROD" : "DB2P.PROVA";

            var db = new IncentiviEntities();
            try
            {
                string sql = $"DELETE OPENQUERY (DB2LINK, ' SELECT * from {prefix}.EMEN_TB_CONG_PAR where ID_CONGEDO={IDcong} ')";
                esito.Query = sql;

                esito.Rows = db.Database.ExecuteSqlCommand(sql);
                esito.Success = esito.Rows == 1;
            }
            catch (Exception ex)
            {
                esito.Success = false;
                esito.Error = ex.Message;
            }
            return esito;

        }

        public bool EsisteRecordDB2(string matricola, DateTime? dataint, string eccezione)
        {
            string prefix = CommonHelper.IsProduzione() ? "DB2R.PROD" : "DB2PPROVA";
            var db = new IncentiviEntities();
            string sql = $"SELECT count(*) from  OPENQUERY(DB2LINK, 'SELECT *  FROM {prefix}.EMEN_TB_CONG_PAR "+
                         $"WHERE MATRICOLA=''{matricola}'' AND DATA_ECCEZIONE=''{dataint.Value.ToString("yyyy-MM-dd")}'' AND ECCEZIONE=''{eccezione}'' ')";
            var esito = db.Database.SqlQuery<int>(sql).FirstOrDefault();
            return esito > 0;
           // return true;
        }
        public ActionResult testinsert()
        {
            Congedo co = new Congedo() {
                 CF= "BRGBNC20H59H501T",
                  Codice_inps="TST",
                   Ctr_non_trasferibile=10.5M,
                    Ctr_trasferibile=100,
                     Data_eccezione= new DateTime (2023,5,1),
                      Data_inizio= new DateTime(2023, 5, 1),
                      Data_fine= new DateTime(2023, 5, 1),
                       Data_inserimento= DateTime .Now,
                        Data_log=DateTime .Now,
                         Data_nascita=new DateTime (2022,1,1),
                          Eccezione="AF",
                           Matricola="853623"
                     
            };
            myRaiCommonManager.NuoviCongedi. InsertToDB2congedi(co,"");
            return Content("OK");
        }
        //public EsitoCongedoDB2 InsertToDB2congedi(Congedo cong)
        //{
        //    EsitoCongedoDB2 esito = new EsitoCongedoDB2()
        //    {
        //        InsDel = "I"
        //    };

        //    string format = "yyyy-MM-dd";
        //    string prefix = CommonHelper.IsProduzione() ? "DB2R.PROD" : "DB2P.PROVA";
        //    var db = new IncentiviEntities();
        //    try
        //    {
              
        //        string datanascita = cong.Data_nascita != null ? "'"+cong.Data_nascita.Value.ToString(format)+"'" : "NULL";
        //        string datainizio = cong.Data_inizio != null ? "'"+cong.Data_inizio.Value.ToString(format)+ "'" : "NULL";
        //        string datafine = cong.Data_fine != null ? "'"+cong.Data_fine.Value.ToString(format)+ "'" : "NULL";
        //        string dataeccezione = cong.Data_eccezione != null ? "'"+cong.Data_eccezione.Value.ToString(format)+ "'" : "NULL";
        //        string datainserimento = cong.Data_inserimento != null ? "'"+cong.Data_inserimento.ToString(format)+ "'" : "NULL";
        //        string datalog= cong.Data_log != null ? "'"+cong.Data_log.ToString(format)+ "'" : "NULL";

        //        string sql = @"INSERT OPENQUERY(DB2LINK, 
        //    'SELECT ID_CONGEDO,
        //            MATRICOLA,
        //            DATA_ECCEZIONE,
        //            ECCEZIONE,
        //            CODICE_FISCALE,
        //            DATA_NASCITA,
        //            DATA_INSERIMENTO,
        //            DATA_INIZIO,
        //            DATA_FINE,
        //            CTR_TRASFERIBILE,
        //            CTR_NON_TRASFERIBILE,
        //            CODICE_INPS,
        //            DATA_LOG
        //            FROM " + prefix + ".EMEN_TB_CONG_PAR') " +
        //                    $" VALUES ( {cong.Id},'{cong.Matricola}',{dataeccezione},'{cong.Eccezione}','{cong.CF}',{datanascita}," +
        //                    $" {datainserimento},{datainizio},{datafine},{(cong.Ctr_trasferibile).ToString().Replace(",", ".")},{(cong.Ctr_non_trasferibile).ToString().Replace(",", ".")}," +
        //                    $"'{cong.Codice_inps}',{datalog})";

        //        esito.Query = sql;

        //        esito.Rows = db.Database.ExecuteSqlCommand(sql);
        //        esito.Success = esito.Rows == 1;
        //        if (esito.Success == false) esito.Error = "0 rows";
        //    }
        //    catch (Exception ex)
        //    {
        //        esito.Success = false;
        //        esito.Error = ex.Message;
        //    }
        //    return esito;

        //}

        public EsitoInserimentoCongedi InserisciInDb2DaArretratiDip(List<XR_MAT_ARRETRATI_DIPENDENTE> list, int idrich)
        {
            EsitoInserimentoCongedi Esito = new EsitoInserimentoCongedi();
            var db = new IncentiviEntities();
            var ric = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrich).FirstOrDefault();
            List<Congedo> LC = new List<Congedo>();
            foreach (var item in list)
            {
                Congedo C = new Congedo()
                {
                    CF = item.CODICE_FISCALE_FIGLIO,
                    Data_eccezione = item.DATA,
                    Data_fine = item.PERIODO_RIFERIMENTO_A.Value,
                    Data_inizio = item.PERIODO_RIFERIMENTO_DA,
                    Data_inserimento = DateTime.Now,
                    Data_log = DateTime.Now,
                    Data_nascita = ric.DATA_NASCITA_BAMBINO ,
                    Eccezione = item.ECCEZIONE,
                    Matricola = ric.MATRICOLA,
                    Id = item.ID
                };
                ContatoreCongedi datiINPS = NuoviCongedi.GetCodiceUniEmens(ric.MATRICOLA, ric.CF_BAMBINO, item.ECCEZIONE, item.DATA,ric.DATA_NASCITA_BAMBINO );
                C.Codice_inps = datiINPS.CodiceUNIEMENS.ToString();
                C.Ctr_non_trasferibile = datiINPS.GiorniTotaliDipendente >= 90 ? 90 : datiINPS.GiorniTotaliDipendente;
                C.Ctr_trasferibile = datiINPS.GiorniTotaliDipendente > 90 ? datiINPS.GiorniTotaliDipendente - 90 : 0;
                LC.Add(C);
            }
            foreach (var cong in LC)
            {
                try
                {
                    if (!EsisteRecordDB2(cong.Matricola, cong.Data_eccezione, cong.Eccezione))
                    {
                        EsitoCongedoDB2 esito = myRaiCommonManager.NuoviCongedi. InsertToDB2congedi(cong,"");
                        if (esito.Success == false)
                        {
                            Esito.Error = esito.Error;
                            return Esito;
                        }
                        Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
                        {
                            operazione = "InsCongediDB2",
                            descrizione_operazione = $"rich:{ric.ID} CF:{cong.CF},dataecc:{cong.Data_eccezione}, esito:{esito.Success}, errore:{esito.Error}"
                        });
                    }
                    
                }
                catch (Exception ex)
                {
                    Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                    {
                        provenienza = "InsCongediDB2",
                        error_message = $"rich:{ric.ID}, dataecc:{cong.Data_eccezione} " + ex.ToString()
                    });
                    Esito.Error = ex.Message;
                    return Esito;
                }
            }
            Esito.Congedi = LC;
            return Esito;
        }
        public EsitoInserimentoArretrati AggiungiSuArretratiDipendente(int idRich, IncentiviEntities db)
        {
            EsitoInserimentoArretrati Esito = new EsitoInserimentoArretrati();

            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idRich).FirstOrDefault();
            if (rich.PIANIFICAZIONE_BASE_ORARIA == true && !rich.XR_MAT_PIANIFICAZIONI.Any())
            {
                Esito.Error = "Nessuna pianificazione trovata";
                return Esito;
            }
            var pian = rich.XR_MAT_PIANIFICAZIONI.OrderByDescending(x => x.DATA_INSERIMENTO).FirstOrDefault();
            if (rich.PIANIFICAZIONE_BASE_ORARIA == true && !pian.XR_MAT_GIORNI_CONGEDO.Any())
            {
                Esito.Error = "Nessuna giornata trovata nella pianificazione";
                return Esito;
            }
            List<XR_MAT_ARRETRATI_DIPENDENTE> ListAdded = new List<XR_MAT_ARRETRATI_DIPENDENTE>();


            DateTime D1 = rich.INIZIO_GIUSTIFICATIVO != null ? rich.INIZIO_GIUSTIFICATIVO.Value : rich.DATA_INIZIO_MATERNITA.Value;
            DateTime D2 = rich.FINE_GIUSTIFICATIVO != null ? rich.FINE_GIUSTIFICATIVO.Value : rich.DATA_FINE_MATERNITA.Value;

            for (DateTime Dcurrent = D1; Dcurrent <= D2; Dcurrent = Dcurrent.AddDays(1))
            {
                XR_MAT_ARRETRATI_DIPENDENTE arr = new XR_MAT_ARRETRATI_DIPENDENTE();
                if (rich.PIANIFICAZIONE_BASE_ORARIA == true)
                {
                    var giornoCongedo = pian.XR_MAT_GIORNI_CONGEDO.Where(x => x.DATA == Dcurrent).FirstOrDefault();
                    if (giornoCongedo != null)
                    {
                        string EccDaInserire = rich.ECCEZIONE;
                        if (giornoCongedo.FRUIZIONE_DECIMAL == (decimal)0.50 && giornoCongedo.FRUIZIONE_TURNO == "I")
                            EccDaInserire += "M";
                        else if (giornoCongedo.FRUIZIONE_DECIMAL == (decimal)0.50 && giornoCongedo.FRUIZIONE_TURNO == "F")
                            EccDaInserire += "P";
                        else if (giornoCongedo.FRUIZIONE_DECIMAL == (decimal)0.25)
                            EccDaInserire += "Q";

                        arr = new XR_MAT_ARRETRATI_DIPENDENTE()
                        {
                            CODICE_FISCALE_FIGLIO = rich.CF_BAMBINO,
                            DATA = Dcurrent,
                            DATA_INSERIMENTO = DateTime.Now,
                            ECCEZIONE = EccDaInserire,
                            MATRICOLA = rich.MATRICOLA,
                            PERIODO_RIFERIMENTO_DA = D1,
                            PERIODO_RIFERIMENTO_A = D2,
                            NOMINATIVO = rich.NOMINATIVO,
                            QUANTITA = giornoCongedo.FRUIZIONE_DECIMAL,
                            PROTOCOLLO_INPS = rich.PROTOCOLLO_INPS
                        };
                    }
                    else
                        continue;

                }
                else
                {
                    arr = new XR_MAT_ARRETRATI_DIPENDENTE()
                    {
                        CODICE_FISCALE_FIGLIO = rich.CF_BAMBINO,
                        DATA = Dcurrent,
                        DATA_INSERIMENTO = DateTime.Now,
                        ECCEZIONE = rich.ECCEZIONE,
                        MATRICOLA = rich.MATRICOLA,
                        PERIODO_RIFERIMENTO_DA = D1,
                        PERIODO_RIFERIMENTO_A = D2,
                        NOMINATIVO = rich.NOMINATIVO,
                        QUANTITA = 1,
                        PROTOCOLLO_INPS = rich.PROTOCOLLO_INPS
                    };
                }

                if ( !db.XR_MAT_ARRETRATI_DIPENDENTE.Any(x => x.MATRICOLA == rich.MATRICOLA && x.DATA == Dcurrent
                        && x.ECCEZIONE == rich.ECCEZIONE))
                {
                    ListAdded.Add(arr);
                    db.XR_MAT_ARRETRATI_DIPENDENTE.Add(arr);
                }
                else
                {
                    Esito.Error = "Data/eccezione gia presente negli arretrati:" + Dcurrent.ToString("dd/MM/yyyy") +
                        " " + rich.ECCEZIONE;
                    return Esito;
                }
            }
            Esito.ListArr = ListAdded;
            return Esito;
        }
        public ActionResult getDoc(int idAllegato)
        {
            var db = new myRaiData.digiGappEntities();
            var alle = db.MyRai_Regole_Allegati.Find(idAllegato);

            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(alle.documento, 0, alle.documento.Length);
            pdfStream.Position = 0;
            string est = System.IO.Path.GetExtension(alle.real_filename).Replace(".", "");
            switch (est.ToLower())
            {
                case "pdf":
                    return new FileStreamResult(pdfStream, "application/pdf") { FileDownloadName = alle.real_filename };
                    break;

                case "xls":
                case "xlsx":
                    return new FileStreamResult(pdfStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = alle.real_filename };
                    break;

                case "doc":
                case "docx":
                    return new FileStreamResult(pdfStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document") { FileDownloadName = alle.real_filename };
                    break;

            }

            return new FileStreamResult(pdfStream, "application/" + System.IO.Path.GetExtension(alle.real_filename).Replace(".", "")) { FileDownloadName = alle.real_filename };
        }

        public ActionResult getEccezioneSchedaPopup(string codice)
        {
            var db = new myRaiData.digiGappEntities();
            var scheda = db.MyRai_Regole_SchedeEccezioni.ValidToday().Where(x => x.codice == codice).FirstOrDefault();

            if (!String.IsNullOrWhiteSpace(scheda.definizione)) scheda.definizione = myRaiHelper.HtmlTrim.Trim(scheda.definizione);
            if (!String.IsNullOrWhiteSpace(scheda.trattamento_economico)) scheda.trattamento_economico = myRaiHelper.HtmlTrim.Trim(scheda.trattamento_economico);
            if (!String.IsNullOrWhiteSpace(scheda.presupposti_procedure)) scheda.presupposti_procedure = myRaiHelper.HtmlTrim.Trim(scheda.presupposti_procedure);
            if (!String.IsNullOrWhiteSpace(scheda.presupposti_documentazione)) scheda.presupposti_documentazione = myRaiHelper.HtmlTrim.Trim(scheda.presupposti_documentazione);
            if (!String.IsNullOrWhiteSpace(scheda.criteri_inserimento)) scheda.criteri_inserimento = myRaiHelper.HtmlTrim.Trim(scheda.criteri_inserimento);

            foreach (var d in scheda.MyRai_Regole_CampiDinamici)
            {
                d.valore = myRaiHelper.HtmlTrim.Trim(d.valore);
            }

            scheda.versione = db.MyRai_Regole_SchedeEccezioni.Where(x => x.codice == codice).Count();

            if (scheda != null)
                return View("_dettaglioEccezione", scheda);
            else
                return Content("NON TROVATA");
        }
        public List<XR_MAT_RICHIESTE> RichiesteStessoMese(int idrichiesta)
        {
            var db = new IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            bool stessoMese = false;
            DateTime? D1 = null;
            DateTime? D2 = null;
            List<XR_MAT_RICHIESTE> listStessoMese = new List<XR_MAT_RICHIESTE>();

            if (rich.INIZIO_GIUSTIFICATIVO == null)
            {
                stessoMese = (rich.DATA_INIZIO_MATERNITA.Value.Month == rich.DATA_FINE_MATERNITA.Value.Month
                    && rich.DATA_INIZIO_MATERNITA.Value.Year == rich.DATA_FINE_MATERNITA.Value.Year);
                if (stessoMese)
                {
                    D1 = new DateTime(rich.DATA_INIZIO_MATERNITA.Value.Year, rich.DATA_FINE_MATERNITA.Value.Month, 1);
                    D2 = D1.Value.AddMonths(1).AddDays(-1);

                    listStessoMese = db.XR_MAT_RICHIESTE.Where(x => x.MATRICOLA == rich.MATRICOLA
                                    && x.ID != idrichiesta
                                    && x.ECCEZIONE == rich.ECCEZIONE
                                    && x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) < 80
                                    && x.DATA_INIZIO_MATERNITA >= D1 && x.DATA_FINE_MATERNITA <= D2).ToList();
                }
            }
            else
            {
                D1 = new DateTime(rich.FINE_GIUSTIFICATIVO.Value.Year, rich.FINE_GIUSTIFICATIVO.Value.Month, 1);
                D2 = D1.Value.AddMonths(1).AddDays(-1);
                listStessoMese = db.XR_MAT_RICHIESTE.Where(x => x.MATRICOLA == rich.MATRICOLA
                               && x.ID != idrichiesta
                               && x.ECCEZIONE == rich.ECCEZIONE
                               && x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) < 80
                               && x.INIZIO_GIUSTIFICATIVO >= D1 && x.FINE_GIUSTIFICATIVO <= D2).ToList();

            }
            DateTime? MaxEstensioneDataTracciati = null;
            foreach (var t in rich.XR_MAT_TASK_IN_CORSO.Where(x => x.NOTE != null && x.NOTE.Contains("-")))
            {
                string[] seg = t.NOTE.Split('-');
                if (seg.Length < 3) continue;

                string dataEnd = seg[2];
                DateTime D;
                if (DateTime.TryParseExact(dataEnd, "dd/MM/yyyy", null, DateTimeStyles.None, out D))
                {
                    if (MaxEstensioneDataTracciati == null || MaxEstensioneDataTracciati < D)
                        MaxEstensioneDataTracciati = D;
                }
            }
            if (MaxEstensioneDataTracciati != null)
            {
                listStessoMese.RemoveAll(x =>
                (x.DATA_FINE_MATERNITA != null && x.DATA_FINE_MATERNITA.Value > MaxEstensioneDataTracciati)
                ||
                (x.FINE_GIUSTIFICATIVO != null && x.FINE_GIUSTIFICATIVO.Value > MaxEstensioneDataTracciati));
            }
            return listStessoMese;
        }
        public ActionResult PrendiInCaricoAmmCheckStessoMese(int idrichiesta)
        {
            List<XR_MAT_RICHIESTE> List = RichiesteStessoMese(idrichiesta);
            List = List.Where(x => x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) <
            (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin).ToList();

            string Esito = "";
            if (List.Any())
            {
                Esito = "<br /><br />";
                foreach (var ric in List)
                {
                    string per = "";
                    if (ric.INIZIO_GIUSTIFICATIVO != null)
                        per = ric.INIZIO_GIUSTIFICATIVO.Value.ToString("dd/MM/yyyy") + "-" + ric.FINE_GIUSTIFICATIVO.Value.ToString("dd/MM/yyyy");
                    else if (ric.DATA_INIZIO_MATERNITA != null)
                        per = ric.DATA_INIZIO_MATERNITA.Value.ToString("dd/MM/yyyy") + "-" + ric.DATA_FINE_MATERNITA.Value.ToString("dd/MM/yyyy");

                    Esito += ric.ECCEZIONE + " " + per + "<br />";
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { data = Esito }
            };
        }
        public ActionResult ConcludiPraticaCheckStessoMese(int idrichiesta)
        {
            List<XR_MAT_RICHIESTE> List = RichiesteStessoMese(idrichiesta);

            string Esito = "";
            if (List.Any())
            {
                foreach (var ric in List)
                {
                    string per = "";
                    if (ric.INIZIO_GIUSTIFICATIVO != null)
                        per = ric.INIZIO_GIUSTIFICATIVO.Value.ToString("dd/MM/yyyy") + "-" + ric.FINE_GIUSTIFICATIVO.Value.ToString("dd/MM/yyyy");
                    else if (ric.DATA_INIZIO_MATERNITA != null)
                        per = ric.DATA_INIZIO_MATERNITA.Value.ToString("dd/MM/yyyy") + "-" + ric.DATA_FINE_MATERNITA.Value.ToString("dd/MM/yyyy");

                    Esito += ric.ECCEZIONE + " " + per + ", ";
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { data = Esito }
            };
        }
        public ActionResult ConcludiPratica(int idrichiesta, bool AncheComprese)
        {
            try
            {
                string nome = UtenteHelper.Nominativo();
                var db = new IncentiviEntities();

                var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();

                myRaiData.Incentivi.XR_WKF_OPERSTATI stato = new myRaiData.Incentivi.XR_WKF_OPERSTATI();
                stato.ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault();
                stato.COD_USER = CommonHelper.GetCurrentUserMatricola();
                stato.ID_STATO = (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.Approvata;
                stato.TMS_TIMESTAMP = DateTime.Now;
                stato.VALID_DTA_INI = DateTime.Now;
                stato.COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT;
                stato.COD_TERMID = Request.UserHostAddress;
                stato.NOMINATIVO = nome;
                stato.XR_MAT_RICHIESTE = rich;
                db.XR_WKF_OPERSTATI.Add(stato);

                if (MaternitaCongediManager.ProvieneDaDEM(rich))
                //if (db.XR_DEM_DOCUMENTI.Any(x => x.Id_Richiesta == idrichiesta))
                {
                    var esit = DematerializzazioneController.ConcludiPratica(ref db, idrichiesta);
                    if (esit != null && esit.Esito == false)
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { esito = false, errore = esit.DescrizioneErrore }
                        };
                    }
                }

                List<XR_MAT_RICHIESTE> Lcomprese = new List<XR_MAT_RICHIESTE>();
                if (AncheComprese)
                {
                    Lcomprese = RichiesteStessoMese(idrichiesta);

                    if (Lcomprese.Any())
                    {
                        foreach (var R in Lcomprese)
                        {
                            myRaiData.Incentivi.XR_WKF_OPERSTATI statoRichiesta = new myRaiData.Incentivi.XR_WKF_OPERSTATI();
                            statoRichiesta.ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault();
                            statoRichiesta.COD_USER = CommonHelper.GetCurrentUserMatricola();
                            statoRichiesta.ID_STATO = (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.Approvata;
                            statoRichiesta.TMS_TIMESTAMP = DateTime.Now;
                            statoRichiesta.VALID_DTA_INI = DateTime.Now;
                            statoRichiesta.COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT;
                            statoRichiesta.COD_TERMID = Request.UserHostAddress;
                            statoRichiesta.NOMINATIVO = nome;
                            statoRichiesta.ID_GESTIONE = R.ID;
                            db.XR_WKF_OPERSTATI.Add(statoRichiesta);
                        }
                    }
                }

                if (AncheComprese && Lcomprese.Any())
                {
                    foreach (var R in Lcomprese)
                    {
                        if (MaternitaCongediManager.ProvieneDaDEM(R))
                        //  if (db.XR_DEM_DOCUMENTI.Any(x => x.Id_Richiesta == R.ID))
                        {
                            var es = DematerializzazioneController.ConcludiPratica(ref db, R.ID);
                            if (es != null && es.Esito == false)
                            {
                                return new JsonResult
                                {
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                                    Data = new { esito = false, errore = es.DescrizioneErrore }
                                };
                            }
                        }
                    }
                }
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };

            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }

        }

        public ActionResult ModificaEccezione(int idrichiesta, string eccezione)
        {
            var db = new IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            rich.ECCEZIONE = eccezione;
            try
            {
                db.SaveChanges();
                return new JsonResult
                {

                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true, desc = MaternitaCongediManager.GetDescrittivaEccezione(eccezione) }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {

                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }

        }
        public ActionResult GetVisualizzazioneGestione(int idrichiesta, bool FromApprovatoreGestione = false)
        {
            MaternitaDettagliGestioneModel model = myRaiCommonManager.MaternitaCongediManager.GetMaternitaDettagliGestioneModel(idrichiesta);
            model.FromApprovatoreGestione = FromApprovatoreGestione;
            return View("PopupVisGestContent", model);
        }
        public ActionResult GetVisualizzazioneDettaglioRichiesta(int idrichiesta)
        {
            MaternitaDettagliRichiestaGestioneModel model = myRaiCommonManager.MaternitaCongediManager.GetMaternitaDettagliRichiestaGestioneModel(idrichiesta);
            return View("popupvisGestDettagliRichiesta", model);
        }
        [HttpPost]
        public ActionResult GetScadenzeAjax(string dati)
        {
            Scadenze[] scadenze = Newtonsoft.Json.JsonConvert.DeserializeObject<Scadenze[]>(dati);
            var grouped = scadenze.GroupBy(x => new { x.meseinvio, x.annoinvio });
            List<RisultatiScadenze> LR = new List<RisultatiScadenze>();
            List<ScadenzeToChange> ScadenzeToChange = new List<ScadenzeToChange>();
            foreach (var item in grouped)
            {
                if (!LR.Any(x => x.mese == item.Key.meseinvio && x.anno == item.Key.annoinvio))
                {
                    DateTime? DataScadenza = GetScadenza(item.Key.meseinvio, item.Key.annoinvio, 17);
                    LR.Add(new RisultatiScadenze()
                    {
                        anno = item.Key.annoinvio,
                        mese = item.Key.meseinvio,
                        datascadenza = DataScadenza
                    });
                }
                DateTime D = new DateTime(item.Key.annoinvio, item.Key.meseinvio, 1);
                int mesenext = D.AddMonths(1).Month;
                int annonext = D.AddMonths(1).Year;
                if (!LR.Any(x => x.mese == item.Key.meseinvio && x.anno == item.Key.annoinvio))
                {
                    DateTime? DataScadenza = GetScadenza(mesenext, annonext, 17);
                    LR.Add(new RisultatiScadenze()
                    {
                        anno = annonext,
                        mese = mesenext,
                        datascadenza = DataScadenza
                    });
                }
            }
            var db = new IncentiviEntities();
            bool save = false;
            foreach (var item in grouped)
            {
                var ListaScad = scadenze.Where(x => x.meseinvio == item.Key.meseinvio && x.annoinvio == item.Key.annoinvio).ToList();
                foreach (var s in ListaScad)
                {
                    XR_MAT_RICHIESTE r = db.XR_MAT_RICHIESTE.Where(x => x.ID == s.idrich).FirstOrDefault();
                    if (r == null) continue;
                    if (r.ID == 417)
                    {

                    }
                    var scadRegistrata = LR.Where(x => x.mese == item.Key.meseinvio && x.anno == item.Key.annoinvio).FirstOrDefault();
                    if (scadRegistrata == null) continue;
                    if (scadRegistrata.datascadenza < r.DATA_INVIO_RICHIESTA)
                    {
                        DateTime D = new DateTime(item.Key.annoinvio, item.Key.meseinvio, 1);
                        int mesenext = D.AddMonths(1).Month;
                        int annonext = D.AddMonths(1).Year;
                        scadRegistrata = LR.Where(x => x.mese == mesenext && x.anno == annonext).FirstOrDefault();
                        if (scadRegistrata == null) continue;
                        if (scadRegistrata.datascadenza > r.DATA_INVIO_RICHIESTA)
                        {
                            if (scadRegistrata.datascadenza != null &&
                                scadRegistrata.datascadenza != r.DATA_SCADENZA)
                            {
                                ScadenzeToChange.Add(new Controllers.ScadenzeToChange()
                                {
                                    dataScadenza = scadRegistrata.datascadenza.Value.ToString("dd/MM/yyyy"),
                                    idrich = s.idrich
                                });
                                r.DATA_SCADENZA = scadRegistrata.datascadenza.Value;
                                save = true;
                            }
                        }
                    }
                    else
                    {
                        if (scadRegistrata.datascadenza != null &&
                                scadRegistrata.datascadenza != r.DATA_SCADENZA)
                        {
                            ScadenzeToChange.Add(new Controllers.ScadenzeToChange()
                            {
                                dataScadenza = scadRegistrata.datascadenza.Value.ToString("dd/MM/yyyy"),
                                idrich = s.idrich
                            });
                            r.DATA_SCADENZA = scadRegistrata.datascadenza.Value;
                            save = true;
                        }
                    }
                }
            }
            if (save) db.SaveChanges();
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { dati = ScadenzeToChange }
            };
        }
        public static DateTime? GetScadenza(int mese, int anno, int idScadenzario)
        {

            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew s = new MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew();
            s.Credentials = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.ScadenzarioMeseAnnoResponse response =
                s.GetScadenzarioMeseAnno(
                idScadenzario.ToString(),
                mese,
                anno);

            if (response.esito && response.scadenze.Any())
            {
                DateTime D = response.scadenze.First().data_scadenza;
                return D;
            }
            return null;
        }
        public ActionResult PrendiInCaricoMazzo(string ids)
        {
            foreach (string s in ids.Split(','))
            {
                int id = Convert.ToInt32(s);
                var resp = PrendiInCaricoAmministrazione(id);
                if (((dynamic)resp).Data.esito == false)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = ((dynamic)resp).Data.errore }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }

        public ActionResult GetValoriContatore(int idrichiesta)
        {
            return new JsonResult
            {
                //data.mesi,data.giorni,data.mesiconiuge,data.giorniconiuge,data.sesso);
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { mesi = 2, giorni = 5, mesiconiuge = 0, giorniconiuge = 0, sesso = "M" }
            };
        }
        public ActionResult GetContentApprovazioni(string mese = null, string matr = null, string sede = null, int? tipo = null, int? stato = null)
        {
            MaternitaApprovazioniModel model = MaternitaCongediManager.GetContentApprovazioniModel(mese, matr, sede, tipo, stato);
            return View("content_approvazioni", model);
        }
        public ActionResult GetContent(string ordine = "N1", string mese = null, string matr = null, string sede = null, int? tipo = null, int? stato = null, string mesetask = null, string assenza = null, string listone = null)
        {
            MaternitaCongediModel model = myRaiCommonManager.MaternitaCongediManager.GetMaternitaCongediModel(mese, matr, sede, tipo, stato, mesetask, ordine, assenza, listone, true);
            model.FromRicerca = (!String.IsNullOrWhiteSpace(mese) || !String.IsNullOrWhiteSpace(matr) || !String.IsNullOrWhiteSpace(sede) ||
                    !String.IsNullOrWhiteSpace(mesetask) || tipo != null || stato != null || !String.IsNullOrWhiteSpace(assenza));

            //if (Environment.MachineName == "LAPTOP-NUTUJ4I3")
            //{
            //    model.RichiesteAggregateInCaricoAme = model.RichiesteAggregateInCaricoAme.Take(3).ToList();
            //    model.RichiesteAggregateInCaricoAltri = model.RichiesteAggregateInCaricoAltri.Take(3).ToList();
            //    model.RichiesteAggregateInCaricoNessuno = model.RichiesteAggregateInCaricoNessuno.Take(3).ToList();
            //}
            return View("content", model);
        }
        public ActionResult GetContent2(string ordine = "N1", string mese = null, string matr = null, string sede = null, int? tipo = null, int? stato = null, string mesetask = null, string assenza = null, string listone = null)
        {
            MaternitaCongediModel model = myRaiCommonManager.MaternitaCongediManager.GetMaternitaCongediModel(mese, matr, sede, tipo, stato, mesetask, ordine, assenza, listone, false);
            model.FromRicerca = (!String.IsNullOrWhiteSpace(mese) || !String.IsNullOrWhiteSpace(matr) || !String.IsNullOrWhiteSpace(sede) ||
                    !String.IsNullOrWhiteSpace(mesetask) || tipo != null || stato != null || !String.IsNullOrWhiteSpace(assenza));

            //if (Environment.MachineName == "LAPTOP-NUTUJ4I3")
            //{
            //    model.RichiesteAggregateInCaricoAme = model.RichiesteAggregateInCaricoAme.Take(3).ToList();
            //    model.RichiesteAggregateInCaricoAltri = model.RichiesteAggregateInCaricoAltri.Take(3).ToList();
            //    model.RichiesteAggregateInCaricoNessuno = model.RichiesteAggregateInCaricoNessuno.Take(3).ToList();
            //}
            return View("content", model);
        }
        //public ActionResult GetCalendarioContent(int idRichiesta, bool isAppr)
        //{

        //    MatPianificazioneModel model = new MatPianificazioneModel();
        //    model.isFromApprovazione = isAppr;

        //    var db = new IncentiviEntities();


        //    model.Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idRichiesta).FirstOrDefault();
        //    model.pf = FeriePermessiManager.GetPianoFerieAnno(((DateTime)model.Richiesta.INIZIO_GIUSTIFICATIVO).Year);

        //    model.DataInizialeCalendario = new DateTime(
        //        ((DateTime)model.Richiesta.INIZIO_GIUSTIFICATIVO).AddMonths(-3).Year,
        //        ((DateTime)model.Richiesta.INIZIO_GIUSTIFICATIVO).AddMonths(-3).Month,
        //        1);
        //    model.DataFinaleCalendario = new DateTime(
        //        ((DateTime)model.Richiesta.FINE_GIUSTIFICATIVO).AddMonths(3).Year,
        //        ((DateTime)model.Richiesta.FINE_GIUSTIFICATIVO).AddMonths(3).Month,
        //        1);

        //    return View("PopupViewPianificazioneContent", model);
        //}
        public enum PosizioneExcel
        {
            MatricolaIndex = 2,
            NominativoIndex = 3,
            DalIndex = 4,
            AlIndex = 5,
            DataNascitaIndex = 6,
            DataPresuntaPartoIndex = 7,
            CFIndex = 8,
            Eccezione = 9
        }
        public ActionResult UploadExcel(HttpPostedFileBase file, string stato)
        {

            var workbook = new XLWorkbook(file.InputStream);
            String ErroriImportazione = "";
            foreach (var w in workbook.Worksheets)
            {

                IXLRow row = w.Row(1);
                if (row.Cell((int)PosizioneExcel.MatricolaIndex) != null)
                {
                    if (row.Cell((int)PosizioneExcel.MatricolaIndex).Value.ToString().ToUpper() != "MATRICOLA")
                    {
                        continue;
                    }
                }
                for (int i = 2; i < 1000; i++)
                {
                    row = w.Row(i);
                    string Matricola = row.Cell((int)PosizioneExcel.MatricolaIndex).Value.ToString();
                    if (String.IsNullOrWhiteSpace(Matricola))
                    {
                        break;
                    }
                    try
                    {
                        string esito = ImportaRowExcel(row, w.Name, stato);
                        if (esito != null)
                        {
                            ErroriImportazione += "Errore Foglio " + w.Name + " Riga " + i + " - " + esito + "\r\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        ErroriImportazione += "Errore Foglio " + w.Name + " Riga " + i + " - " + ex.Message + "\r\n";
                    }
                }
            }
            if (String.IsNullOrWhiteSpace(ErroriImportazione))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ErroriImportazione }
                };
            }


        }

        public bool ModificaTracciato(XR_MAT_RICHIESTE Rich, DateTime DataFineMaternitaPrecedente,
            int SogliaPortaFineMese, bool ProsegueUlterioreMese)
        {
            bool BackView = false;
            int TotDays = DateTime.DaysInMonth(DataFineMaternitaPrecedente.Year, DataFineMaternitaPrecedente.Month);
            int EffettivoGiornoFineTracciato = Rich.DATA_FINE_MATERNITA.Value.Day;

            if (ProsegueUlterioreMese)
            {
                EffettivoGiornoFineTracciato = TotDays;
            }
            var TaskDaModificare = Rich.XR_MAT_TASK_IN_CORSO.Where(x =>
                x.ANNO == DataFineMaternitaPrecedente.Year &&
                x.MESE == DataFineMaternitaPrecedente.Month &&
                x.XR_MAT_ELENCO_TASK.NOME_TASK.Trim() == "MATERNITA MT 9000").FirstOrDefault();
            if (TaskDaModificare != null && TaskDaModificare.TERMINATA != true)
            {
                List<CampoContent> ContenutoCampi =
                  MaternitaCongediManager.GetTracciatoEsploso(
                      (int)TaskDaModificare.XR_MAT_ELENCO_TASK.ID_TRACCIATO_DEW,
                      (int)TaskDaModificare.XR_MAT_ELENCO_TASK.PROGRESSIVO_TRACCIATO_DEW,
                       TaskDaModificare.INPUT);
                var campo_GG_RET = ContenutoCampi.Where(x => x.NomeCampo == "GG RETRIB AL").FirstOrDefault();
                if (campo_GG_RET != null)
                {
                    if (campo_GG_RET.ContenutoCampo != TotDays.ToString())
                    {
                        string NuovoValoreGG = EffettivoGiornoFineTracciato.ToString().PadLeft(2, '0');
                        TaskDaModificare.INPUT = TaskDaModificare.INPUT
                               .Remove(campo_GG_RET.PosizioneTracciato - 1, 2)
                               .Insert(campo_GG_RET.PosizioneTracciato - 1, NuovoValoreGG.ToString());

                        var campo30mi = ContenutoCampi.Where(x => x.NomeCampo.Trim() == "GIORNI 30MI").FirstOrDefault();
                        TaskDaModificare.INPUT = TaskDaModificare.INPUT
                           .Remove(campo30mi.PosizioneTracciato - 1, 2)
                           .Insert(campo30mi.PosizioneTracciato - 1, NuovoValoreGG.ToString());

                        DateTime DateStart = new DateTime(Rich.DATA_FINE_MATERNITA.Value.Year, Rich.DATA_FINE_MATERNITA.Value.Month, 1);
                        DateTime DateEnd = new DateTime(Rich.DATA_FINE_MATERNITA.Value.Year, Rich.DATA_FINE_MATERNITA.Value.Month, EffettivoGiornoFineTracciato);
                        int ggNoDom = GetGiorniNoDom(DateStart, DateEnd);
                        var campoGG_RET = ContenutoCampi.Where(x => x.NomeCampo.Trim() == "GIORNI RETRIBUTIVI").FirstOrDefault();
                        TaskDaModificare.INPUT = TaskDaModificare.INPUT
                           .Remove(campoGG_RET.PosizioneTracciato - 1, 2)
                           .Insert(campoGG_RET.PosizioneTracciato - 1, ggNoDom.ToString().PadLeft(3, '0') + "00");
                        var campoGG_MENSA = ContenutoCampi.Where(x => x.NomeCampo.Trim() == "GIORNI MENSA").FirstOrDefault();
                        TaskDaModificare.INPUT = TaskDaModificare.INPUT
                           .Remove(campoGG_MENSA.PosizioneTracciato - 1, 2)
                           .Insert(campoGG_MENSA.PosizioneTracciato - 1, ggNoDom.ToString().PadLeft(3, '0') + "00");

                        TaskDaModificare.BLOCCATA_DATETIME = DateTime.Now;
                        TaskDaModificare.BLOCCATA_DA_OPERATORE = CommonHelper.GetCurrentUserMatricola();
                        BackView = true;
                    }
                }
            }
            if (ProsegueUlterioreMese) return true;
            else return BackView;
        }
        [HttpPost]
        public ActionResult SalvaDataPresunta(int idrich, string data)
        {
            DateTime D;
            if (DateTime.TryParseExact(data, "dd/MM/yyyy", null, DateTimeStyles.None, out D) == false)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Data non valida" }
                };
            }
            var db = new IncentiviEntities();
            var Rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrich).FirstOrDefault();
            if (Rich == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Richiesta non trovata" }
                };
            }
            Rich.DATA_PRESUNTA_PARTO = D;
            if (Rich.XR_MAT_TASK_IN_CORSO.Any())
            {
                Rich.DA_RIAVVIARE = true;
            }
            bool ReloadView = false;
            bool BackView = false;
            try
            {
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true, reload = ReloadView, back = BackView }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }
        }

        [HttpPost]
        public ActionResult SalvaDataNascita(int idrich, string data)
        {
            DateTime D;
            if (DateTime.TryParseExact(data, "dd/MM/yyyy", null, DateTimeStyles.None, out D) == false)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Data non valida" }
                };
            }
            var db = new IncentiviEntities();
            var Rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrich).FirstOrDefault();
            if (Rich == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Richiesta non trovata" }
                };
            }
            Rich.DATA_NASCITA_BAMBINO = D;
            Rich.DATA_PARTO = D;
            if (Rich.XR_MAT_TASK_IN_CORSO.Any())
            {
                Rich.DA_RIAVVIARE = true;
            }

            bool ReloadView = false;
            bool BackView = false;
            if (Rich.DATA_PRESUNTA_PARTO != null)
            {
                if (Rich.DATA_NASCITA_BAMBINO > Rich.DATA_PRESUNTA_PARTO.Value)
                {
                    int SogliaPortaFineMese = CommonHelper.GetParametro<int>(EnumParametriSistema.SogliaConcludiMeseMT);

                    int daysDifference = (Rich.DATA_NASCITA_BAMBINO.Value - Rich.DATA_PRESUNTA_PARTO.Value).Days;
                    DateTime DataFineMaternitaPrecedente = Rich.DATA_FINE_MATERNITA.Value;

                    Rich.DATA_FINE_MATERNITA = Rich.DATA_FINE_MATERNITA.Value.AddDays(daysDifference);

                }
            }
            try
            {
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true, reload = ReloadView, back = BackView }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }

        }
        public int GetGiorniNoDom(DateTime from, DateTime to)
        {
            var totalDays = 0;
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Sunday)
                    totalDays++;
            }

            return totalDays;
        }
        public string ImportaRowExcel(IXLRow row, string WorksheetName, string stato)
        {

            var db = new myRaiData.Incentivi.IncentiviEntities();
            XR_MAT_RICHIESTE R = new XR_MAT_RICHIESTE();

            R.IMPORTATA_DATETIME = DateTime.Now;
            R.IMPORTATA_MATRICOLA = CommonHelper.GetCurrentUserMatricola();
            R.DATA_INVIO_RICHIESTA = DateTime.Now;

            R.MATRICOLA = GetCella(row, (int)PosizioneExcel.MatricolaIndex);
            if (String.IsNullOrWhiteSpace(R.MATRICOLA)) return "Campo MATRICOLA non trovato";
            if (R.MATRICOLA != null && R.MATRICOLA.ToUpper().Contains("P"))
                R.MATRICOLA = R.MATRICOLA.Replace("P", "");

            if (R.MATRICOLA.Length < 6) R.MATRICOLA = R.MATRICOLA.PadLeft(6, '0');


            R.NOMINATIVO = GetCella(row, (int)PosizioneExcel.NominativoIndex);
            if (String.IsNullOrWhiteSpace(R.NOMINATIVO)) return "Campo NOMINATIVO non trovato";
            string Eccezione = GetCella(row, (int)PosizioneExcel.Eccezione);
            if (Eccezione != null)
                Eccezione = Eccezione.Trim().ToUpper();

            string Dal = GetCella(row, (int)PosizioneExcel.DalIndex);
            if (String.IsNullOrWhiteSpace(Dal)) return "Campo DAL non trovato";
            if (Dal.Length > 10) Dal = Dal.Substring(0, 10);
            DateTime D1;
            if (!DateTime.TryParseExact(Dal, "dd/MM/yyyy", null, DateTimeStyles.None, out D1))
                return "Campo DAL non valido";
            else
            {
                if (Eccezione == "MT")
                    R.DATA_INIZIO_MATERNITA = D1;
                else
                    R.INIZIO_GIUSTIFICATIVO = D1;
            }


            string Al = GetCella(row, (int)PosizioneExcel.AlIndex);
            if (String.IsNullOrWhiteSpace(Dal)) return "Campo AL non trovato";
            if (Al.Length > 10) Al = Al.Substring(0, 10);
            DateTime D2;
            if (!DateTime.TryParseExact(Al, "dd/MM/yyyy", null, DateTimeStyles.None, out D2))
                return "Campo AL non valido";
            else
            {
                if (Eccezione == "MT")
                    R.DATA_FINE_MATERNITA = D2;
                else
                    R.FINE_GIUSTIFICATIVO = D2;
            }

            if (D2 < D1)
            {
                return "Periodo non corretto";
            }

            var anag = BatchManager.GetUserData(R.MATRICOLA, R.INIZIO_GIUSTIFICATIVO ?? R.DATA_INIZIO_MATERNITA.Value);
            R.SEDEGAPP = anag.sede_gapp;
            R.REPARTO = anag.CodiceReparto;

            //+++VB
            if (Eccezione == "MT")
                R.ASSENZA_LUNGA = (R.DATA_FINE_MATERNITA.Value - R.DATA_INIZIO_MATERNITA.Value)
                   .TotalDays > 13;
            else
                R.ASSENZA_LUNGA = (R.FINE_GIUSTIFICATIVO.Value - R.INIZIO_GIUSTIFICATIVO.Value)
                   .TotalDays > 13;


            if (MaternitaCongediManager.TipoDipendente(R.MATRICOLA, R.INIZIO_GIUSTIFICATIVO ?? R.DATA_INIZIO_MATERNITA.Value) == "O")
            {
                R.ASSENZA_LUNGA = true;
            }

            if (db.XR_MAT_RICHIESTE.Any(x => x.MATRICOLA == R.MATRICOLA && x.INIZIO_GIUSTIFICATIVO == D1
            && x.FINE_GIUSTIFICATIVO == D2 && !x.XR_WKF_OPERSTATI.Any(z => z.ID_STATO > 80)))
            {
                return "Periodo già presente per questa matricola";
            }
            string DataNascita = GetCella(row, (int)PosizioneExcel.DataNascitaIndex);
            if (!String.IsNullOrWhiteSpace(DataNascita))
            {
                if (DataNascita.Length > 10) DataNascita = DataNascita.Substring(0, 10);
                DateTime DN;
                if (!DateTime.TryParseExact(DataNascita, "dd/MM/yyyy", null, DateTimeStyles.None, out DN))
                    return "Campo DATANASCITA non valido";
                else
                    R.DATA_NASCITA_BAMBINO = DN;
            }

            string DataPresuntaParto = GetCella(row, (int)PosizioneExcel.DataPresuntaPartoIndex);
            if (!String.IsNullOrWhiteSpace(DataPresuntaParto))
            {
                if (DataPresuntaParto.Length > 10) DataPresuntaParto = DataPresuntaParto.Substring(0, 10);
                DateTime DPP;
                if (!DateTime.TryParseExact(DataPresuntaParto, "dd/MM/yyyy", null, DateTimeStyles.None, out DPP))
                    return "Campo DATAPRESUNTAPARTO non valido";
                else
                    R.DATA_PRESUNTA_PARTO = DPP;
            }

            string CF = GetCella(row, (int)PosizioneExcel.CFIndex);
            if (!String.IsNullOrWhiteSpace(CF))
            {
                R.CF_BAMBINO = CF;
            }

            XR_MAT_CATEGORIE cat = null;

            if (!String.IsNullOrWhiteSpace(Eccezione))
            {
                if (Eccezione.Trim().ToUpper() == "MU")
                {
                    cat = db.XR_MAT_CATEGORIE.Where(x => x.ECCEZIONE == "MU").FirstOrDefault();
                }
                else
                {
                    cat = db.XR_MAT_CATEGORIE.Where(x => x.ECCEZIONE.Contains(Eccezione)).FirstOrDefault();
                }

                if (cat != null)
                {
                    R.ECCEZIONE = Eccezione;
                    R.XR_MAT_CATEGORIE = cat;
                }
                else
                    return "Tipologia di congedo non trovata";
            }
            else
                return "Tipologia di congedo non trovata";

            R.DATA_SCADENZA = MaternitaCongediManager.GetScadenza(R);

            if (R.DATA_PARTO == null && R.DATA_NASCITA_BAMBINO != null)
                R.DATA_PARTO = R.DATA_NASCITA_BAMBINO;

            if (R.DATA_NASCITA_BAMBINO == null && R.DATA_PARTO != null)
                R.DATA_NASCITA_BAMBINO = R.DATA_PARTO;







            db.XR_MAT_RICHIESTE.Add(R);


            ///////////////////////////////   STATI WKF /////////////////////////////
            int StatoPratica = Convert.ToInt32(stato);

            List<int> Stati = db.XR_MAT_STATI.Where(x => x.ID_STATO <= StatoPratica)
                                   .Select(x => x.ID_STATO).OrderBy(x => x).ToList();
            foreach (int s in Stati)
            {
                XR_WKF_OPERSTATI ST = new XR_WKF_OPERSTATI()
                {
                    COD_TERMID = Request.UserHostAddress,
                    COD_USER = CommonHelper.GetCurrentUserMatricola(),
                    VALID_DTA_INI = DateTime.Now,
                    TMS_TIMESTAMP = DateTime.Now,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    ID_STATO = s,
                    ID_TIPOLOGIA = 3,
                    XR_MAT_RICHIESTE = R,
                    COD_TIPO_PRATICA = cat.CAT //<----------------------------------------------

                };
                if (s == 30)//appr uffgest
                {
                    R.PRESA_VISIONE_RESP_GEST = DateTime.Now;
                    R.PRESA_VISIONE_RESP_MATR = CommonHelper.GetCurrentRealUsername();
                }
                if (s == 50) // appr uffpers
                {
                    ST.UFFPERS_PRESA_VISIONE = true;
                }

                db.XR_WKF_OPERSTATI.Add(ST);

                R.XR_WKF_OPERSTATI.Add(ST);
            }


            // se ce ne sono stessa eccezione stessa matricola stesso mese ----------------------------------------
            // segnala sulla principale DA RIAVVIARE
            XR_MAT_RICHIESTE PraticaGiaAvviata = null;

            if (R.FINE_GIUSTIFICATIVO != null)
            {
                PraticaGiaAvviata = db.XR_MAT_RICHIESTE.Where(x =>
                                   x.XR_MAT_TASK_IN_CORSO.Any() &&
                                   x.MATRICOLA == R.MATRICOLA && R.ECCEZIONE == x.ECCEZIONE &&
                                   x.FINE_GIUSTIFICATIVO != null &&
                                   x.FINE_GIUSTIFICATIVO.Value.Year == R.FINE_GIUSTIFICATIVO.Value.Year &&
                                   x.FINE_GIUSTIFICATIVO.Value.Month == R.FINE_GIUSTIFICATIVO.Value.Month)
                                   .FirstOrDefault();
            }
            else
            {
                PraticaGiaAvviata = db.XR_MAT_RICHIESTE.Where(x =>
                                   x.XR_MAT_TASK_IN_CORSO.Any() &&
                                   x.MATRICOLA == R.MATRICOLA && R.ECCEZIONE == x.ECCEZIONE &&
                                   x.DATA_FINE_MATERNITA != null &&
                                   x.DATA_FINE_MATERNITA.Value.Year == R.DATA_FINE_MATERNITA.Value.Year &&
                                   x.DATA_FINE_MATERNITA.Value.Month == R.DATA_FINE_MATERNITA.Value.Month)
                                   .FirstOrDefault();
            }
            if (PraticaGiaAvviata != null)
            {
                DateTime DataEnd = PraticaGiaAvviata.FINE_GIUSTIFICATIVO ?? PraticaGiaAvviata.DATA_FINE_MATERNITA.Value;
                DateTime DataEndUltimoDelMese = new DateTime(DataEnd.Year, DataEnd.Month, DateTime.DaysInMonth(DataEnd.Year, DataEnd.Month));

                if (PraticaGiaAvviata.XR_WKF_OPERSTATI.Max(x => x.ID_STATO) < 80//se NON è chiusa (06/03/2023 Vincenzo)
                    &&
                    (
                    DataEndUltimoDelMese >= DateTime.Today //se siamo ancora nel mese in oggetto della pratica (06/03/2023 Vincenzo)
                    ))
                {
                    PraticaGiaAvviata.DA_RIAVVIARE = true;
                }
            }


            ///
            ///insert db :
            ///
            db.SaveChanges();

            /////////////
            return null;
        }
        public string GetCella(IXLRow row, int index)
        {
            if (row.Cell(index) != null && row.Cell(index).Value != null &&
                !String.IsNullOrWhiteSpace(row.Cell(index).Value.ToString()))
            {
                return row.Cell(index).Value.ToString();
            }
            else
            {
                return null;
            }
        }
        public ActionResult getImportiCedolinoMese(string meseanno, int idrichiesta)
        {
            int mese = Convert.ToInt32(meseanno.Split('/')[0]);
            int anno = Convert.ToInt32(meseanno.Split('/')[1]);

            DettaglioAmministrazioneModel model = GetDettaglioAmministrazioneModel(idrichiesta, mese, anno);
            return null;
        }

        public DettaglioAmministrazioneModel GetDettaglioAmministrazioneModel(int idrichiesta, int? mese = null, int? anno = null)
        {
            DettaglioAmministrazioneModel model = new DettaglioAmministrazioneModel();
            var db = new IncentiviEntities();
            model.Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            var anag = myRaiHelper.BatchManager.GetUserData(model.Richiesta.MATRICOLA, model.Richiesta.INIZIO_GIUSTIFICATIVO ?? model.Richiesta.DATA_INIZIO_MATERNITA.Value);
            string FormaContrattoDaAnag = anag.forma_contratto;

            DateTime? datachiusura = myRaiCommonTasks.CommonTasks.GetDataChiusura2(DateTime.Now.Month.ToString().PadLeft(2, '0'),
                DateTime.Now.Year.ToString(), CommonHelper.GetCurrentUserMatricola(), 80);

            model.DataAttualeOltreChiusura = (datachiusura != null && datachiusura.Value < DateTime.Now);

            model.OperazioniAvviate = model.Richiesta.XR_MAT_TASK_IN_CORSO.Any();

            model.MesiAnnoCompetenza = GetMesiAnnoCompetenza(model.Richiesta);
            model.EccezioneRisultante = MaternitaCongediManager.GetEccezioneRisultante(model.Richiesta);

            List<XR_MAT_ECCEZIONI> eccezioniSalvateDB = new List<XR_MAT_ECCEZIONI>();
            List<MyRaiServiceInterface.MyRaiServiceReference1.InfoPresenza> TuttiGiorni = new List<MyRaiServiceInterface.MyRaiServiceReference1.InfoPresenza>();

            foreach (DateTime DinizioMese in model.MesiAnnoCompetenza)
            {
                int GiorniTotaliQuestoMese = DateTime.DaysInMonth(DinizioMese.Year, DinizioMese.Month);

                DettaglioGiorniPerMese DM = new DettaglioGiorniPerMese();
                model.ElencoGiorniPerMese.Add(DM);
                DM.RiferimentoPrimoDelMese = DinizioMese;

                DateTime DataFineMese = DinizioMese.AddMonths(1).AddDays(-1);

                eccezioniSalvateDB = db.XR_MAT_ECCEZIONI.Where(x => x.ID_RICHIESTA == idrichiesta
                && x.DATA_INIZIO >= DinizioMese && x.DATA_INIZIO <= DataFineMese
                ).ToList();
                bool EccezioniDaDB = false;

                if (eccezioniSalvateDB.Any())
                {
                    EccezioniDaDB = true;

                    if (model.Richiesta.ECCEZIONE == "MT")
                    {
                        DateTime Dmin = eccezioniSalvateDB.Min(x => x.DATA_INIZIO);
                        DateTime Dmax = eccezioniSalvateDB.Max(x => x.DATA_INIZIO);
                        if ((Dmax - Dmin).TotalDays + 1 > eccezioniSalvateDB.Count)
                        {
                            foreach (var item in eccezioniSalvateDB)
                            {
                                if (item.QUANTITA == (decimal)1.2)
                                {
                                    item.QUANTITA = 1;
                                }

                            }
                            for (DateTime D = Dmin; D <= Dmax; D = D.AddDays(1))
                            {
                                if (!eccezioniSalvateDB.Any(x => x.DATA_INIZIO == D))
                                {
                                    if (D < model.Richiesta.DATA_INIZIO_MATERNITA.Value) continue;

                                    //MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp serviceWS =
                                    //    new MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp()
                                    //    {
                                    //        Credentials = CommonHelper.GetUtenteServizioCredentials()
                                    //    };


                                    //var resp = serviceWS.getEccezioni(model.Richiesta.MATRICOLA, D.ToString("ddMMyyyy"), "BU", 70);
                                    //if (resp.orario != null && resp.orario.cod_orario != null && !resp.orario.cod_orario.StartsWith("9"))
                                    //{
                                    eccezioniSalvateDB.Add(new XR_MAT_ECCEZIONI()
                                    {
                                        DATA_INIZIO = D,
                                        DATA_FINE = null,
                                        DATA_INSERIMENTO = DateTime.Now,
                                        ECCEZIONE = "MT",
                                        ID_RICHIESTA = model.Richiesta.ID,
                                        MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                                        QUANTITA = (D.DayOfWeek == DayOfWeek.Sunday ? 0 : 1),
                                        QUANTITA_ORIGINALE = (D.DayOfWeek == DayOfWeek.Sunday ? 0 : 1),
                                        STATO_ECCEZIONE = null
                                    });
                                    // }

                                }
                            }
                        }
                    }
                    foreach (var item in eccezioniSalvateDB)
                    {
                        DM.ElencoGiorni.Add(new DettaglioGiorniModel()
                        {
                            CodiceEccezione = item.ECCEZIONE,
                            DescEccezione = CommonHelper.GetDescrizioneEccezione(item.ECCEZIONE),
                            DataDa = item.DATA_INIZIO,
                            DataA = item.DATA_FINE,
                            NumeroGiorniRuoli = (float)item.QUANTITA,
                            NumeroGiorniGapp = (float)item.QUANTITA_ORIGINALE,
                            StatoEccez = item.STATO_ECCEZIONE
                        });
                    }
                }
                else
                {
                    DM.ElencoGiorni = MaternitaCongediManager.GetTotaleGiorni(DinizioMese, model.Richiesta.MATRICOLA, model.Richiesta);

                    if (MaternitaCongediManager.GetEccezioneRisultante(model.Richiesta) == "MT")
                    {
                        if (DM.ElencoGiorni.Any(x => x.DataDa.Day == 1 || x.DataDa.Day == GiorniTotaliQuestoMese))
                        {
                            foreach (var item in DM.ElencoGiorni)
                            {
                                if (item.NumeroGiorniRuoli == 1.2f) item.NumeroGiorniRuoli = 1f;
                                if (item.DataDa.DayOfWeek == DayOfWeek.Saturday) item.NumeroGiorniRuoli = 1f;
                                if (item.DataDa.DayOfWeek == DayOfWeek.Sunday) item.NumeroGiorniRuoli = 0;
                            }
                            while (DM.ElencoGiorni.Sum(x => x.NumeroGiorniRuoli) > 26f)
                            {
                                var last = DM.ElencoGiorni.Where(x => x.NumeroGiorniRuoli == 1f).LastOrDefault();
                                if (last == null)
                                    break;
                                else
                                    last.NumeroGiorniRuoli = 0;
                            }
                        }
                    }
                }
                bool skipElaborazione = EccezioniDaDB && model.Richiesta.FORZA_ECCEZIONE_PRATICA;

                if (!skipElaborazione)
                {
                    List<string> ListaEccez = DM.ElencoGiorni.Select(x => x.CodiceEccezione).Distinct().ToList();

                    List<RisultatoEccezioni> risultato =
                        MaternitaCongediManager.GetAnalisiEccezioniGapp(model.Richiesta.MATRICOLA, DinizioMese, ListaEccez);

                    var group = DM.ElencoGiorni.GroupBy(x => x.CodiceEccezione).ToList();
                    foreach (var itemGroup in group)
                    {
                        string codice = itemGroup.Key;
                        foreach (var item in itemGroup.ToList())
                        {
                            if (!risultato.Any(x => x.Data == item.DataDa && x.Codice == item.CodiceEccezione))
                            {
                                item.NonPresenteSuGetAnalisiEccezioni = true;
                            }
                        }
                    }
                    DM.TotaleGiorni = DM.ElencoGiorni.Sum(x => x.NumeroGiorniRuoli);

                    //List<RisultatoEccezioniGetPresenze> RisultatiSchedaPres =
                    GetPresenzeResponse Res =
                   MaternitaCongediManager.GetEccezioniByGetPresenze(model.Richiesta.MATRICOLA, DinizioMese, model.Richiesta.PIANIFICAZIONE_BASE_ORARIA == true);


                    List<RisultatoEccezioniGetPresenze> RisultatiSchedaPres = Res.RisultatiEccezioniGetPresenze;
                    if (Res != null && Res.ServiceResponse != null && Res.ServiceResponse.Giorni != null)
                    {
                        TuttiGiorni.AddRange(Res.ServiceResponse.Giorni.ToList());
                    }
                    List<RisultatoEccezioniGetPresenze> ListaNonPresentiSuGetRuol = new List<RisultatoEccezioniGetPresenze>();
                    List<RisultatoEccezioniGetPresenze> ListaPresentiAncheSuGetRuol = new List<RisultatoEccezioniGetPresenze>();
                    foreach (var item in RisultatiSchedaPres)
                    {
                        if (!DM.ElencoGiorni.Any(x => x.CodiceEccezione == item.Codice &&
                     (x.DataA == null && x.DataDa == item.Data) || (x.DataA != null && x.DataDa <= item.Data && x.DataA >= item.Data)))
                        {
                            ListaNonPresentiSuGetRuol.Add(item);
                        }
                        else
                        {
                            ListaPresentiAncheSuGetRuol.Add(item);
                        }
                    }

                    if (ListaNonPresentiSuGetRuol.Any())
                    {
                        string[] codiciOrarioZeroQuantita = CommonHelper.GetParametro<string>(EnumParametriSistema.CodiciOrarioValoreZeroPerCongedi).Split(',');
                        float quantita = 1.2f;
                        if (MaternitaCongediManager.GetEccezioneRisultante(model.Richiesta) == "MT")
                        {
                            var PrimoItem = ListaNonPresentiSuGetRuol.Where(x => x.Codice == "MT" && x.Data.Day == 1).FirstOrDefault();
                            var UltimoItem = ListaNonPresentiSuGetRuol.Where(x => x.Codice == "MT" && x.Data.Day == GiorniTotaliQuestoMese).FirstOrDefault();
                            if (PrimoItem != null || UltimoItem != null)
                            {
                                quantita = 1.0f;
                            }
                        }
                        foreach (var g in ListaNonPresentiSuGetRuol)
                        {
                            if (!model.Richiesta.XR_MAT_CATEGORIE.ECCEZIONE.Contains(g.Codice)) continue;

                            DettaglioGiorniModel d = new DettaglioGiorniModel()
                            {
                                CodiceEccezione = g.Codice,
                                DataDa = g.Data,
                                DataA = null,
                                DescEccezione = CommonHelper.GetDescrizioneEccezione(g.Codice),
                                NonPresenteSuGetRuoli = true
                            };
                            if (g.Codice.EndsWith("Q"))
                                d.NumeroGiorniGapp = 0.30f;
                            else if (MaternitaCongediManager.IsHalf(g.Codice))
                                d.NumeroGiorniGapp = 0.60f;
                            else
                            {
                                if (quantita == 1.2f && codiciOrarioZeroQuantita.Contains(g.CodiceOrario))
                                    d.NumeroGiorniGapp = 0;
                                else
                                {
                                    if (quantita == 1.2f)
                                        d.NumeroGiorniGapp = quantita;
                                    else if (quantita == 1.0f)
                                    {
                                        if (g.Data.DayOfWeek == DayOfWeek.Saturday)
                                            d.NumeroGiorniGapp = quantita;
                                        else if (g.Data.DayOfWeek == DayOfWeek.Sunday)
                                            d.NumeroGiorniGapp = 0;
                                        else
                                            d.NumeroGiorniGapp = quantita;
                                    }
                                }
                            }



                            DM.TotaleGiorni += d.NumeroGiorniGapp;
                            DM.ElencoGiorni.Add(d);
                        }

                    }

                }
            }

            //if (MaternitaCongediManager.SonoDaAggiungereGiorniForzati(model.Richiesta))
            //{

            if (FormaContrattoDaAnag != null && FormaContrattoDaAnag.Trim() != "K"
                || (FormaContrattoDaAnag.Trim() == "K" && (model.Richiesta.GIORNI_DEFAULT26 == null ||
                model.Richiesta.GIORNI_DEFAULT26 == 26)))
            {
                DateTime Dlimite1 = model.Richiesta.INIZIO_GIUSTIFICATIVO ?? model.Richiesta.DATA_INIZIO_MATERNITA.Value;
                DateTime Dlimite2 = model.Richiesta.FINE_GIUSTIFICATIVO ?? model.Richiesta.DATA_FINE_MATERNITA.Value;
                string NonAggiungereSe90 = CommonHelper.GetParametro<string>(EnumParametriSistema.NonAggiungereSe90);
                foreach (var ricontrollo in model.ElencoGiorniPerMese)
                {
                    foreach (var g in ricontrollo.ElencoGiorni)
                    {
                        g.CodiceOrario = TuttiGiorni.Where(x => x.data == g.DataDa).Select(x => x.CodiceOrario).FirstOrDefault();
                    }
                    string desc = CommonHelper.GetDescrizioneEccezione(model.Richiesta.ECCEZIONE);
                    DateTime D = ricontrollo.RiferimentoPrimoDelMese;
                    DateTime D2 = D.AddMonths(1).AddDays(-1);
                    for (DateTime Dcurrent = D; Dcurrent <= D2; Dcurrent = Dcurrent.AddDays(1))
                    {
                        if (Dcurrent < Dlimite1 || Dcurrent > Dlimite2) continue;
                        //test-afbf ///////////////////////////////////////////////////
                        if (model.Richiesta.FORZA_ECCEZIONE_PRATICA)
                        {
                            var d = ricontrollo.ElencoGiorni.Where(x => x.DataDa == Dcurrent).FirstOrDefault();
                            if (d != null && d.CodiceEccezione != model.Richiesta.ECCEZIONE)
                            {
                                d.CodiceEccezione = model.Richiesta.ECCEZIONE;
                                d.AggiuntoForzato = true;
                            }
                        }
                        //////////////////////////////////////////////////////////////
                        if (!ricontrollo.ElencoGiorni.Any(x => x.DataDa == Dcurrent))
                        {
                            bool Aggiungi = true;

                            if (NonAggiungereSe90.Split(',').Contains(model.Richiesta.ECCEZIONE))
                            {
                                string turno = TuttiGiorni.Where(x => x.data == Dcurrent).Select(x => x.CodiceOrario).FirstOrDefault();
                                if (turno != null && turno.StartsWith("9"))
                                    Aggiungi = false;

                            }
                            if (Aggiungi)
                            {
                                DettaglioGiorniModel Dg = new DettaglioGiorniModel()
                                {
                                    CodiceEccezione = model.Richiesta.ECCEZIONE,
                                    DataDa = Dcurrent,
                                    DataA = Dcurrent,
                                    DescEccezione = desc,
                                    Fusioni = 0,
                                    IntervalliFusi = new List<string>(),
                                    NonPresenteSuGetAnalisiEccezioni = true,
                                    NumeroGiorniRuoli = 01,
                                    AggiuntoForzato = true,
                                    CodiceOrario = TuttiGiorni.Where(x => x.data == Dcurrent).Select(x => x.CodiceOrario).FirstOrDefault()
                                };
                                ricontrollo.ElencoGiorni.Add(Dg);
                            }

                        }

                    }
                    ricontrollo.ElencoGiorni = ricontrollo.ElencoGiorni.OrderBy(x => x.DataDa).ToList();
                }
            }

            //}





            if (eccezioniSalvateDB != null && eccezioniSalvateDB.Any())
            {
                foreach (var item in eccezioniSalvateDB)
                {
                    model.ElencoGiorni = new List<DettaglioGiorniModel>();
                    model.ElencoGiorni.Add(new DettaglioGiorniModel()
                    {
                        CodiceEccezione = item.ECCEZIONE,
                        DescEccezione = CommonHelper.GetDescrizioneEccezione(item.ECCEZIONE),
                        DataDa = item.DATA_INIZIO,
                        DataA = item.DATA_FINE,
                        NumeroGiorniRuoli = (float)item.QUANTITA,
                        NumeroGiorniGapp = (float)item.QUANTITA_ORIGINALE
                    });
                }
            }
            else
            {
                DateTime? Dstart = model.Richiesta.INIZIO_GIUSTIFICATIVO;
                if (Dstart == null) Dstart = model.Richiesta.DATA_INIZIO_MATERNITA;
                model.ElencoGiorni = MaternitaCongediManager.GetTotaleGiorni((DateTime)Dstart, model.Richiesta.MATRICOLA, model.Richiesta);

            }
            DateTime? D1 = model.Richiesta.INIZIO_GIUSTIFICATIVO;
            if (D1 == null) D1 = model.Richiesta.DATA_INIZIO_MATERNITA;

            model.MeseCompetenza = D1.Value.ToString("MM");
            model.AnnoCompetenza = D1.Value.ToString("yyyy");
            model.MeseAnnoCompetenza = (D1.Value.ToString("MMMM yyyy").Substring(0, 1).ToUpper() +
                D1.Value.ToString("MMMM yyyy").Substring(1));



            List<string> ListaEccezioni = model.ElencoGiorni.Select(x => x.CodiceEccezione).Distinct().ToList();
            DateTime Dinizio = new DateTime(Convert.ToInt32(model.AnnoCompetenza), Convert.ToInt32(model.MeseCompetenza), 1);

            List<RisultatoEccezioni> ris =
                MaternitaCongediManager.GetAnalisiEccezioniGapp(model.Richiesta.MATRICOLA, Dinizio, ListaEccezioni);


            var grouped = model.ElencoGiorni.GroupBy(x => x.CodiceEccezione).ToList();
            foreach (var itemGroup in grouped)
            {
                string codice = itemGroup.Key;
                foreach (var item in itemGroup.ToList())
                {
                    if (!ris.Any(x => x.Data == item.DataDa && x.Codice == item.CodiceEccezione))
                    {
                        item.NonPresenteSuGetAnalisiEccezioni = true;
                    }
                }
            }
            model.TotaleGiorni = model.ElencoGiorni.Sum(x => x.NumeroGiorniRuoli);

            GetPresenzeResponse Re =
            //List<RisultatoEccezioniGetPresenze> RisultatiSchedaPresenze =
                MaternitaCongediManager.GetEccezioniByGetPresenze(model.Richiesta.MATRICOLA, Dinizio, model.Richiesta.PIANIFICAZIONE_BASE_ORARIA == true);

            List<RisultatoEccezioniGetPresenze> RisultatiSchedaPresenze = Re.RisultatiEccezioniGetPresenze;

            List<RisultatoEccezioniGetPresenze> ListaNonPresentiSuGetRuoli = new List<RisultatoEccezioniGetPresenze>();
            foreach (var item in RisultatiSchedaPresenze)
            {
                if (!model.ElencoGiorni.Any(x => x.CodiceEccezione == item.Codice &&
             (x.DataA == null && x.DataDa == item.Data) || (x.DataA != null && x.DataDa <= item.Data && x.DataA >= item.Data)))
                {
                    ListaNonPresentiSuGetRuoli.Add(item);
                }
            }
            if (ListaNonPresentiSuGetRuoli.Any())
            {
                string[] codiciOrarioZeroQuantita = CommonHelper.GetParametro<string>(EnumParametriSistema.CodiciOrarioValoreZeroPerCongedi).Split(',');

                foreach (var g in ListaNonPresentiSuGetRuoli)
                {
                    DettaglioGiorniModel d = new DettaglioGiorniModel()
                    {
                        CodiceEccezione = g.Codice,
                        DataDa = g.Data,
                        DataA = g.Data,
                        DescEccezione = CommonHelper.GetDescrizioneEccezione(g.Codice),
                        NonPresenteSuGetRuoli = true
                    };
                    if (codiciOrarioZeroQuantita.Contains(g.CodiceOrario))
                        d.NumeroGiorniGapp = 0;
                    else
                        d.NumeroGiorniGapp = 1.2f;

                    model.TotaleGiorni += d.NumeroGiorniGapp;
                    model.ElencoGiorni.Add(d);
                }

            }



            var stato = model.Richiesta.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();
            model.InCaricoAMe = stato.COD_USER == CommonHelper.GetCurrentUserMatricola() && stato.ID_STATO ==
                (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin;




            ////////////////////////////////////

            if (mese == null)
            {
                XR_MAT_RICHIESTE RichiestaAgganciataPrecedente = MaternitaCongediManager.GetRichiestaAdiacentePrecedente(model.Richiesta);
                if (RichiestaAgganciataPrecedente != null)
                {
                    DateTime? DataInizioPeriodo = model.Richiesta.INIZIO_GIUSTIFICATIVO;
                    if (DataInizioPeriodo == null)
                        DataInizioPeriodo = model.Richiesta.DATA_INIZIO_MATERNITA;

                    DateTime? DataInizioPeriodoRichAgganciata = RichiestaAgganciataPrecedente.INIZIO_GIUSTIFICATIVO;
                    if (DataInizioPeriodoRichAgganciata == null)
                        DataInizioPeriodoRichAgganciata = RichiestaAgganciataPrecedente.DATA_INIZIO_MATERNITA;

                    if (DataInizioPeriodoRichAgganciata.Value.Month != DataInizioPeriodo.Value.Month)
                    {
                        mese = DataInizioPeriodoRichAgganciata.Value.AddMonths(-1).Month;
                        anno = DataInizioPeriodoRichAgganciata.Value.AddMonths(-1).Year;
                    }
                }
            }
            //////////////////////////////////
            DettaglioCedolinoModel CedModel = GetCedolinoModel(idrichiesta, true, mese, anno);

            model.FormaContratto = CedModel.FormaContratto;
            model.DettagliSTR = CedModel.DettagliSTR;

            model.MesePerCalcoloCedolino = CedModel.MesePerCalcolo;
            model.AnnoPerCalcoloCedolino = CedModel.AnnoPerCalcolo;
            //if (model.FormaContratto == "9")// tipo K
            //{

            //    DateTime Dfinale = new DateTime(Convert.ToInt32(model.AnnoPerCalcoloCedolino), Convert.ToInt32(model.MesePerCalcoloCedolino), 1);
            //    DateTime Diniziale = Dfinale.AddMonths(-11);
            //    List<string> MesiAnno = new List<string>();
            //    Decimal Totale175 = 0;
            //    Decimal Totale178 = 0;
            //    Decimal Totale179 = 0;
            //    for (DateTime Dcurrent = Diniziale; Dcurrent <= Dfinale; Dcurrent = Dcurrent.AddMonths(1))
            //    {
            //        MesiAnno.Add(" '" + Dcurrent.Year + "/" + Dcurrent.Month.ToString().PadLeft(2, '0') + "' ");
            //        string queryIndennita = GetQueryVariabiliTE2(model.Richiesta.MATRICOLA, Dcurrent.Year, Dcurrent.Month);
            //        IEnumerable<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico2> varsTE2 =
            //              db.Database.SqlQuery<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico2>
            //              (queryIndennita).ToList();
            //        var row175 = varsTE2.Where(x => x.cod_indennita == "175").FirstOrDefault();
            //        if (row175 != null && row175.importo_inden != 0)
            //        {
            //            Totale175 += (decimal)row175.importo_inden;
            //        }
            //        var row178 = varsTE2.Where(x => x.cod_indennita == "178").FirstOrDefault();
            //        if (row178 != null && row178.importo_inden != 0)
            //        {
            //            Totale178 += (decimal)row178.importo_inden;
            //        }
            //        var row179 = varsTE2.Where(x => x.cod_indennita == "179").FirstOrDefault();
            //        if (row179 != null && row179.importo_inden != 0)
            //        {
            //            Totale179 += (decimal)row179.importo_inden;
            //        }
            //    }
            //    string MesiAnnoConcat = String.Join(",", MesiAnno);
            //    Decimal TotaleLordo = MaternitaCongediManager.GetImportoLordoTotale(model.Richiesta.MATRICOLA, MesiAnnoConcat);
            //    TotaleLordo = TotaleLordo - Totale175 - Totale178 - Totale179;

            //    CedModel.LordoMedioMensile = Math.Round(TotaleLordo / 12, 2);
            //    CedModel.Medio175Mensile = Math.Round(Totale175 / 12, 2);
            //    CedModel.Medio178Mensile = Math.Round(Totale178 / 12, 2);
            //    CedModel.Medio179Mensile = Math.Round(Totale179 / 12, 2);

            //    CedModel.ListaItemCedolino = new List<DettaglioCedolinoItemModel>();
            //    CedModel.ListaItemCedolino.Add(new DettaglioCedolinoItemModel()
            //    {
            //        CellaCalcoloDaExcel="A9",
            //         CalcolatoModel=null,
            //         Etichetta="Lordo medio mensile",
            //         NameAttuale="Lordo_medio_mensile",
            //         NameHRDW="Lordo_medio_mensile_old",
            //         TipiDipendenteSpettanti="*",
            //          ValoreAttuale=(float)CedModel.LordoMedioMensile,
            //          ValoreHRDW=(float)CedModel.LordoMedioMensile
            //    });
            //}


            if (CedModel.TipoDipendente == "G")
            {
                if (CedModel.VociGeneriche.ImportoTotale > 0)
                    model.ImportoFinale = CedModel.VociGeneriche.ImportoTotale;
                else
                    model.ImportoFinale =
                          CedModel.VociGeneriche.stipendio +
                          CedModel.VociGeneriche.ind_contingenza +
                          CedModel.VociGeneriche.ind_mensa +
                           CedModel.VociGeneriche.ind_110 +
                           CedModel.VociGeneriche.ind_120 +
                           CedModel.VociGeneriche.ind_123 +
                           CedModel.VociGeneriche.ind_129 +
                           CedModel.VociGeneriche.ind_130 +
                           CedModel.VociGeneriche.ind_114 +
                           CedModel.VociGeneriche.ind_119 +
                           CedModel.VociGeneriche.ind_12B +
                           CedModel.VociGeneriche.ind_13B +
                           CedModel.VociGeneriche.ind_14E +
                           CedModel.VociGeneriche.ind_14F +
                           CedModel.VociGeneriche.minimo +
                           CedModel.VociGeneriche.superminimo +
                           CedModel.VociGeneriche.DG55 +
                           CedModel.VociGeneriche.LN20 +
                           CedModel.VociGeneriche.LN25 +
                           CedModel.VociGeneriche.PDCO +
                           CedModel.VociGeneriche.LF80 +
                           CedModel.VociGeneriche.LF36 +
                           CedModel.VociGeneriche.AR20 +
                           CedModel.VociGeneriche.RPAF;
            }
            else
            {
                if (CedModel.VociGeneriche.ImportoTotale > 0)
                    model.ImportoFinale = CedModel.VociGeneriche.ImportoTotale;
                else
                    model.ImportoFinale =
                        CedModel.VociGeneriche.stipendio +
                          CedModel.VociGeneriche.edr +
                          CedModel.VociGeneriche.elemento_distinto +
                          CedModel.VociGeneriche.ind_106 +
                          CedModel.VociGeneriche.ind_108 +
                          CedModel.VociGeneriche.ind_109 +
                          CedModel.VociGeneriche.ind_111 +
                          CedModel.VociGeneriche.ind_116 +
                          CedModel.VociGeneriche.ind_11A +
                          CedModel.VociGeneriche.ind_11e +
                          CedModel.VociGeneriche.ind_11C +
                          CedModel.VociGeneriche.ind_131 +
                          CedModel.VociGeneriche.ind_133 +
                          CedModel.VociGeneriche.ind_141 +
                          CedModel.VociGeneriche.ind_146 +
                          CedModel.VociGeneriche.ind_147 +
                          CedModel.VociGeneriche.ind_159 +
                          CedModel.VociGeneriche.ind_altre +
                          CedModel.VociGeneriche.ind_contingenza +
                          CedModel.VociGeneriche.ind_mensa +
                          CedModel.VociGeneriche.str_precedente +
                          CedModel.VociGeneriche.ind_102 +
                          CedModel.VociGeneriche.ind_149
                          ;
            }


            return model;
        }

        public DettaglioAmministrazioneModel MarcaCompresoTotalmente(DettaglioAmministrazioneModel model)
        {
            foreach (var item in model.ElencoGiorniPerMese)
            {
                DateTime D1 = item.RiferimentoPrimoDelMese;
                DateTime D2 = item.RiferimentoPrimoDelMese.AddMonths(1).AddDays(-1);

                DateTime Inizio = model.Richiesta.INIZIO_GIUSTIFICATIVO ?? model.Richiesta.DATA_INIZIO_MATERNITA.Value;
                DateTime Fine = model.Richiesta.FINE_GIUSTIFICATIVO ?? model.Richiesta.DATA_FINE_MATERNITA.Value;

                if (Inizio <= D1 && Fine >= D2)
                {
                    item.CompresoTotalmente = true;
                }
            }
            return model;
        }
        public ActionResult GetDettaglioAmm(int idrichiesta, int? mese = null, int? anno = null)
        {
            DettaglioAmministrazioneModel model = GetDettaglioAmministrazioneModel(idrichiesta, mese, anno);

            if (MaternitaCongediManager.PeriodoRichiestaIniziaMeseAttuale(model.Richiesta))
            {
                model.RaccoltaMesePrec = MaternitaCongediManager.GetRaccoltaMesePrecedente(model.Richiesta);
            }

            model = MarcaCompresoTotalmente(model);


            return View("_dettaglioAmm", model);
        }
        public ActionResult ResetErrore(int id)
        {
            var db = new IncentiviEntities();
            var taskincorso = db.XR_MAT_TASK_IN_CORSO.Where(x => x.ID == id).FirstOrDefault();
            if (taskincorso != null)
            {
                taskincorso.ERRORE_BATCH = null;
                taskincorso.ERRORE_RESETTABILE = false;
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            else
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Non trovato" }
                };
        }
        public ActionResult SospendiPratica(int idrich, int sospesa)
        {
            var db = new IncentiviEntities();
            try
            {
                var ric = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrich).FirstOrDefault();
                if (sospesa == 1)
                {
                    ric.PRATICA_SOSPESA_DATETIME = DateTime.Now;
                    ric.PRATICA_SOSPESA_MATR = CommonHelper.GetCurrentUserMatricola();
                }
                else
                {
                    ric.PRATICA_SOSPESA_DATETIME = null;
                    ric.PRATICA_SOSPESA_MATR = null;
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }
        private List<DateTime> GetMesiAnnoCompetenza(XR_MAT_RICHIESTE richiesta)
        {
            DateTime? Dinizio = richiesta.INIZIO_GIUSTIFICATIVO;
            DateTime? Dfine = richiesta.FINE_GIUSTIFICATIVO;

            List<DateTime> LD = new List<DateTime>();

            if (Dinizio == null)
            {
                Dinizio = richiesta.DATA_INIZIO_MATERNITA;
                Dfine = richiesta.DATA_FINE_MATERNITA;

                //+++VB AUMENTARE I PERIODI IN BASE A MATERNITA'



            }

            DateTime CurrentDate = (DateTime)Dinizio;
            while (true)
            {
                if (CurrentDate > Dfine) break;

                LD.Add(new DateTime(CurrentDate.Year, CurrentDate.Month, 1));
                CurrentDate = new DateTime(CurrentDate.AddMonths(1).Year, CurrentDate.AddMonths(1).Month, 1);

            }
            return LD;
        }

        public ActionResult GetAlertField()
        {
            return View("_alertField");
        }
        public ActionResult Trovak()
        {
            var db = new IncentiviEntities();
            var list = db.XR_MAT_RICHIESTE.Where(x =>
            x.ECCEZIONE != "SW" &&
            x.XR_MAT_TASK_IN_CORSO.Any() &&
            x.XR_WKF_OPERSTATI.Any() &&
            x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) <= 80)
            .OrderBy(x => x.MATRICOLA).ToList();



            List<string> Matricole = list.Select(x => x.MATRICOLA).Distinct().OrderBy(x => x).ToList();
            List<string> ListaK = new List<string>();
            foreach (var m in Matricole)
            {
                string q = GetQueryVariabiliTE1(m, 2022, 9);
                IEnumerable<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico1> varsTE1 =
                    db.Database.SqlQuery<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico1>
                    (q).ToList();
                string formacontratto = varsTE1.Select(x => x.forma_contratto).FirstOrDefault();
                if (formacontratto == "K")
                {
                    ListaK.Add(m);
                }
            }
            string html = "";
            foreach (string mat in ListaK)
            {
                string nom = db.XR_MAT_RICHIESTE.Where(x => x.MATRICOLA == mat).Select(x => x.NOMINATIVO).FirstOrDefault();
                html += mat + " " + nom + "<br />";
            }
            return Content(html);

            foreach (var ric in list)
            {
                myRaiCommonModel.DettaglioCedolinoModel model = GetCedolinoModel(ric.ID, false);
                if (model.FormaContratto == "K")
                {
                    html += ric.NOMINATIVO + " " + ric.ECCEZIONE + " " +
                        (ric.INIZIO_GIUSTIFICATIVO != null
                        ? ric.INIZIO_GIUSTIFICATIVO.Value.ToString("dd/MM/yyyy") + "-" + ric.FINE_GIUSTIFICATIVO.Value.ToString("dd/MM/yyyy")
                        : ric.DATA_INIZIO_MATERNITA.Value.ToString("dd/MM/yyyy") + "-" + ric.DATA_FINE_MATERNITA.Value.ToString("dd/MM/yyyy")
                        )
                        + "<BR />";
                }
            }
            return Content("html");

        }
        public myRaiCommonModel.DettaglioCedolinoModel GetCedolinoModel(int idrichiesta, bool modifica, int? mese = null, int? anno = null)
        {
            myRaiCommonModel.DettaglioCedolinoModel model = new DettaglioCedolinoModel(idrichiesta);
            model.ModificaAbilitata = modifica;
            var anag = myRaiHelper.BatchManager.GetUserData(model.Richiesta.MATRICOLA, model.Richiesta.INIZIO_GIUSTIFICATIVO ?? model.Richiesta.DATA_INIZIO_MATERNITA.Value);
            //model.TipoDipendente = "I";  
            model.TipoDipendente = anag.tipo_dipendente;

            var db = new IncentiviEntities();


            //voci stipendio/////////////////////////////////////////////////////////
            DateTime? D1 = model.Richiesta.INIZIO_GIUSTIFICATIVO;
            if (D1 == null) D1 = model.Richiesta.DATA_INIZIO_MATERNITA;

            var RichiestaPrecedenteContigua = MaternitaCongediManager.GetRichiestaContiguaPrecedente(model.Richiesta);
            if (RichiestaPrecedenteContigua != null)
            {
                D1 = RichiestaPrecedenteContigua.INIZIO_GIUSTIFICATIVO ?? RichiestaPrecedenteContigua.DATA_INIZIO_MATERNITA.Value;

                var RichUlteriorePrecedente = MaternitaCongediManager.GetRichiestaContiguaPrecedente(RichiestaPrecedenteContigua);
                if (RichUlteriorePrecedente != null)
                {
                    D1 = RichUlteriorePrecedente.INIZIO_GIUSTIFICATIVO ?? RichUlteriorePrecedente.DATA_INIZIO_MATERNITA.Value;
                }
            }

            int annoQuery = ((DateTime)D1).AddMonths(-1).Year;
            int meseQuery = ((DateTime)D1).AddMonths(-1).Month;

            if (mese != null) meseQuery = (int)mese;
            if (anno != null) annoQuery = (int)anno;

            model.MesePerCalcolo = meseQuery.ToString().PadLeft(2, '0');
            model.AnnoPerCalcolo = annoQuery.ToString();

            string q = GetQueryVariabiliTE1(model.Richiesta.MATRICOLA, annoQuery, meseQuery);

            IEnumerable<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico1> varsTE1 =
                    db.Database.SqlQuery<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico1>
                    (q).ToList();

            q = GetQueryVariabiliTE2(model.Richiesta.MATRICOLA, annoQuery, meseQuery);

            IEnumerable<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico2> varsTE2 =
                   db.Database.SqlQuery<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico2>
                   (q).ToList();
            ////////////////////////////////////////////////////////////////////
            ///

            List<XR_MAT_VOCI_CEDOLINO> vociSovrascritteSuDB = new List<XR_MAT_VOCI_CEDOLINO>();
            if (mese == null)
            {
                vociSovrascritteSuDB = db.XR_MAT_VOCI_CEDOLINO.Where(x => x.ID_RICHIESTA == idrichiesta).ToList();
                if (vociSovrascritteSuDB.Any())
                {
                    model.MesePerCalcolo = vociSovrascritteSuDB.First().MESE_RIFERIMENTO_VALORI.ToString().PadLeft(2, '0');
                    model.AnnoPerCalcolo = vociSovrascritteSuDB.First().ANNO_RIFERIMENTO_VALORI.ToString();
                }
            }


            model.VociGeneriche = new CedolinoGenerico();
            model.VociGeneriche.idrichiesta = idrichiesta;

            model.FormaContratto = varsTE1.Select(x => x.forma_contratto).FirstOrDefault();
            //stipendio - TUTTI
            decimal? value = varsTE1.Select(x => x.retrib_mensile).FirstOrDefault();
            //vars.Where(x => x.cod_voce_cedolino == "101").Select(x => x.Importo).FirstOrDefault();
            if (value == null) value = 0;
            model.VociGeneriche.stipendio_old = (float)value;
            var voce = vociSovrascritteSuDB.Where(x => x.VOCE_CEDOLINO == "stipendio").FirstOrDefault();
            if (voce != null)
                model.VociGeneriche.stipendio = (float)voce.VALORE;
            else
                model.VociGeneriche.stipendio = (float)value;

            //ind_contingenza - TUTTI
            value = varsTE2.Where(x => x.cod_indennita == "104").Select(x => x.importo_inden).FirstOrDefault();
            if (value == null) value = 0;
            //vars.Where(x => x.cod_voce_cedolino == "104").Select(x => x.Importo).FirstOrDefault();
            model.VociGeneriche.ind_contingenza_old = (float)value;
            voce = vociSovrascritteSuDB.Where(x => x.VOCE_CEDOLINO == "ind_contingenza").FirstOrDefault();
            if (voce != null)
                model.VociGeneriche.ind_contingenza = (float)voce.VALORE;
            else
                model.VociGeneriche.ind_contingenza = (float)value;

            //ind_mensa - TUTTI
            value = varsTE2.Where(x => x.cod_indennita == "105").Select(x => x.importo_inden).FirstOrDefault();
            if (value == null) value = 0;
            //vars.Where(x => x.cod_voce_cedolino == "105").Select(x => x.Importo).FirstOrDefault();
            model.VociGeneriche.ind_mensa_old = (float)value;
            voce = vociSovrascritteSuDB.Where(x => x.VOCE_CEDOLINO == "ind_mensa").FirstOrDefault();
            if (voce != null)
                model.VociGeneriche.ind_mensa = (float)voce.VALORE;
            else
                model.VociGeneriche.ind_mensa = (float)value;

            //elemento_distinto - NON G
            if (model.TipoDipendente != "G")
            {
                value = varsTE2.Where(x => x.cod_indennita == "103").Select(x => x.importo_inden).FirstOrDefault();
                if (value == null) value = -1;
                //vars.Where(x => x.cod_voce_cedolino == "103").Select(x => x.Importo).FirstOrDefault();
                model.VociGeneriche.elemento_distinto_old = (float)value;
                voce = vociSovrascritteSuDB.Where(x => x.VOCE_CEDOLINO == "elemento_distinto").FirstOrDefault();
                if (voce != null)
                    model.VociGeneriche.elemento_distinto = (float)voce.VALORE;
                else
                    model.VociGeneriche.elemento_distinto = (float)value;

                //edr - NON G
                value = varsTE2.Where(x => x.cod_indennita == "14G").Select(x => x.importo_inden).FirstOrDefault();
                if (value == null) value = -1;
                //vars.Where(x => x.cod_voce_cedolino == "14G").Select(x => x.Importo).FirstOrDefault();
                model.VociGeneriche.edr_old = (float)value;
                voce = vociSovrascritteSuDB.Where(x => x.VOCE_CEDOLINO == "edr").FirstOrDefault();
                if (voce != null)
                    model.VociGeneriche.edr = (float)voce.VALORE;
                else
                    model.VociGeneriche.edr = (float)value;

            }
            else
            {
                model.VociGeneriche.elemento_distinto_old = -1;
                model.VociGeneriche.edr_old = -1;
            }





            //ind_altre
            value = 0;// vars.Where(x => x.cod_voce_cedolino == "14G").Select(x => x.Importo).FirstOrDefault();
                      //model.VociGeneriche.ind_altre_old = (float)value;
            voce = vociSovrascritteSuDB.Where(x => x.VOCE_CEDOLINO == "ind_altre").FirstOrDefault();
            if (voce != null)
                model.VociGeneriche.ind_altre = (float)voce.VALORE;
            else
                model.VociGeneriche.ind_altre = (float)value;

            //minimo (SOLO G)
            if (model.TipoDipendente == "G")
            {
                value = varsTE1.Select(x => x.minimo).FirstOrDefault();
                //vars.Where(x => x.cod_voce_cedolino == "9999999").Select(x => x.Importo).FirstOrDefault();
                if (value == 0) value = -1;
                if (value == null) value = 0;

                model.VociGeneriche.minimo_old = (float)value;
                voce = vociSovrascritteSuDB.Where(x => x.VOCE_CEDOLINO == "minimo").FirstOrDefault();
                if (voce != null)
                    model.VociGeneriche.minimo = (float)voce.VALORE;
                else
                    model.VociGeneriche.minimo = (float)value;
            }
            else
            {
                model.VociGeneriche.minimo_old = -1;
            }

            if (model.TipoDipendente == "G")
            {
                //superminimo (SOLO G)
                decimal? sup = varsTE1.Select(x => x.superminimo + x.superminimo_acc_13032018).FirstOrDefault();
                if (sup != null)
                {
                    model.VociGeneriche.superminimo = (float)sup;
                    model.VociGeneriche.superminimo_old = (float)sup;
                }
                else
                    model.VociGeneriche.superminimo_old = -1;
                //value = vars.Where(x => x.cod_voce_cedolino == "88888888").Select(x => x.Importo).FirstOrDefault();
                //if (value == 0) value = -1;
                //model.VociGeneriche.superminimo_old = (float)value;
                //voce = vociSovrascritteSuDB.Where(x => x.VOCE_CEDOLINO == "superminimo").FirstOrDefault();
                //if (voce != null)
                //    model.VociGeneriche.superminimo = (float)voce.VALORE;
                //else
                //    model.VociGeneriche.superminimo = (float)value;
            }
            else
            {
                model.VociGeneriche.superminimo_old = -1;
            }

            if (model.TipoDipendente == "G")
            {
                //redazionale (premio produzione per i non G)
                value = varsTE1.Select(x => x.xiv_mensilita).FirstOrDefault();
            }
            else
            {
                //redazionale (premio produzione per i non G)
                value = varsTE1.Select(x => x.premio_produzione).FirstOrDefault();
            }



            if (value == null) value = 0;
            //vars.Where(x => x.cod_voce_cedolino == "105").Select(x => x.Importo).FirstOrDefault();
            model.VociGeneriche.redazionale_old = (float)value;
            voce = vociSovrascritteSuDB.Where(x => x.VOCE_CEDOLINO == "redazionale").FirstOrDefault();
            if (voce != null)
                model.VociGeneriche.redazionale = (float)voce.VALORE;
            else
                model.VociGeneriche.redazionale = (float)value;



            //indennita specifiche
            PropertyInfo[] properties = typeof(myRaiCommonModel.CedolinoGenerico).GetProperties();
            foreach (PropertyInfo prop_ind_old in properties.Where(x => x.Name.ToLower().StartsWith("ind")
                                                            && x.Name.ToLower().EndsWith("old")))
            {
                if (prop_ind_old.Name.ToLower() == "ind_mensa_old" ||
                    prop_ind_old.Name.ToLower() == "ind_contingenza_old" ||
                     prop_ind_old.Name.ToLower() == "ind_altre_old"
                    ) continue;

                string codiceHRDW = prop_ind_old.Name.ToLower().Replace("ind_", "").Replace("_old", "");

                var voceHRDW = varsTE2.Where(x => x.cod_indennita == codiceHRDW).FirstOrDefault();
                //vars.Where(x => x.cod_voce_cedolino == codiceHRDW).FirstOrDefault();
                if (voceHRDW != null)
                    value = voceHRDW.importo_inden;
                else
                    value = -1; //non ha questa indennita

                prop_ind_old.SetValue(model.VociGeneriche, (float)value, null);
                if (value == -1) continue;


                voce = vociSovrascritteSuDB.Where(x => x.VOCE_CEDOLINO == "ind_" + codiceHRDW).FirstOrDefault();
                PropertyInfo prop_ind = typeof(myRaiCommonModel.CedolinoGenerico).GetProperty("ind_" + codiceHRDW);
                if (voce != null)
                {
                    prop_ind.SetValue(model.VociGeneriche, (float)voce.VALORE, null);
                }
                else
                {
                    prop_ind.SetValue(model.VociGeneriche, (float)value, null);
                }
            }
            if (model.TipoDipendente != "O")
            {
                model.VociGeneriche.ind_102 = 0;
                model.VociGeneriche.ind_102_old = -1;
                model.VociGeneriche.ind_149 = 0;
                model.VociGeneriche.ind_149_old = -1;
            }

            var ind13r = varsTE2.Where(x => x.cod_indennita == "13R").FirstOrDefault();
            if (ind13r != null)
            {
                model.VociGeneriche.ind_13R = (float)ind13r.importo_inden;
                model.VociGeneriche.ind_13R_old = (float)ind13r.importo_inden;
            }
            var ind14e = varsTE2.Where(x => x.cod_indennita == "14E").FirstOrDefault();
            if (ind14e != null)
            {
                model.VociGeneriche.ind_14E = (float)ind14e.importo_inden;
                model.VociGeneriche.ind_14E_old = (float)ind14e.importo_inden;
            }
            var ind14f = varsTE2.Where(x => x.cod_indennita == "14F").FirstOrDefault();
            if (ind14f != null)
            {
                model.VociGeneriche.ind_14F = (float)ind14f.importo_inden;
                model.VociGeneriche.ind_14F_old = (float)ind14f.importo_inden;
            }
            var ind11c = varsTE2.Where(x => x.cod_indennita == "11C").FirstOrDefault();
            if (ind11c != null)
            {
                model.VociGeneriche.ind_11C = (float)ind11c.importo_inden;
                model.VociGeneriche.ind_11C_old = (float)ind11c.importo_inden;
            }
            //testdelete

            //model.VociGeneriche.ind_126 = (float)10;
            // model.VociGeneriche.ind_126_old = (float)10;




            if (MaternitaCongediManager.IsFsuper(model.Richiesta.MATRICOLA) && model.Richiesta.ECCEZIONE == "MG")
            {
                var ind11a = varsTE2.Where(x => x.cod_indennita == "11A").FirstOrDefault();
                if (ind11a != null)
                {
                    model.VociGeneriche.ind_11A = (float)ind11a.importo_inden;
                    model.VociGeneriche.ind_11A_old = (float)ind11a.importo_inden;
                }
            }


            var ind11e = varsTE2.Where(x => x.cod_indennita == "11E").FirstOrDefault();
            if (ind11e != null)
            {
                model.VociGeneriche.ind_11e = (float)ind11e.importo_inden;
                model.VociGeneriche.ind_11e_old = (float)ind11e.importo_inden;
            }

            var ind12B = varsTE2.Where(x => x.cod_indennita == "12B").FirstOrDefault();
            if (model.TipoDipendente == "G" && ind12B != null)
            {
                model.VociGeneriche.ind_12B = (float)ind12B.importo_inden;
                model.VociGeneriche.ind_12B_old = (float)ind12B.importo_inden;
            }
            if (model.TipoDipendente == "G")
            {
                DateTime d1 = new DateTime(annoQuery, meseQuery, 1);
                DateTime d2 = d1.AddMonths(1).AddDays(-1);
                model.QuantitaEccezioni = MaternitaCongediManager.GetTotaleEccezioniGiornalisti(model.Richiesta.MATRICOLA, d1, d2);
            }
            //if (MaternitaCongediManager.IsHC(model.Richiesta))
            //{
            //    model.VociGeneriche.ind_106 = 0;
            //    model.VociGeneriche.ind_106_old = -1;

            //    model.VociGeneriche.ind_altre = 0;
            //}
            if (MaternitaCongediManager.GetEccezioneRisultante(model.Richiesta) == "AF" ||
                MaternitaCongediManager.GetEccezioneRisultante(model.Richiesta) == "MU")
            {
                model.VociGeneriche.mens_13ma = 0;
                model.VociGeneriche.mens_14ma = 0;
                model.VociGeneriche.redazionale = 0;
                model.VociGeneriche.redazionale_old = 0;
            }
            model.ListaItemCedolino = GetItemsCedolino(model, vociSovrascritteSuDB, mese, anno);
            foreach (var item in model.ListaItemCedolino)
            {
                if (model.Richiesta.XR_MAT_TASK_IN_CORSO.Any())
                    item.ModificaAbilitata = false;
            }
            return model;
        }
        public ActionResult RimborsaTracciato(int id)
        {
            string esito = MaternitaCongediManager.CreaTracciatoPerStornoIDTaskInCorso(id);
            if (esito == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = esito }
                };
            }
        }

        public ActionResult getTasksRecap(int idrichiesta)
        {
            var db = new IncentiviEntities();

            TaskModel model = new TaskModel(idrichiesta);
            model.ContenutoCampi = new List<ContenutoCampiPerTask>();
            model.PuoConcluderePratica = true;
            bool IsRichiestaTE = (model.Richiesta.XR_MAT_TASK_IN_CORSO.Any(x => x.XR_MAT_ELENCO_TASK.TIPO == "TRACCIATO-TE"));

            if (!IsRichiestaTE)
                model.RichiesteCompreseInQuestoPeriodo = MaternitaCongediManager.GetRichiesteCompreseInRichiesta(model.Richiesta);
            else
                model.RichiesteCompreseInQuestoPeriodo = new List<XR_MAT_RICHIESTE>();

            foreach (var taskInCorso in model.Richiesta.XR_MAT_TASK_IN_CORSO)
            {

                var taskElenco = db.XR_MAT_ELENCO_TASK.Where(x => x.ID == taskInCorso.ID_TASK).FirstOrDefault();
                if (taskElenco.NOME_TASK.ToUpper() != "DESCRITTIVA" && taskElenco.OBBLIGATORIO_PER_CONCLUSIONE == true && taskInCorso.TERMINATA == false)
                {
                    if (taskInCorso.ERRORE_BATCH == null && taskInCorso.BLOCCATA_DATETIME == null)
                    {
                        model.PuoConcluderePratica = false;
                    }

                }
                List<CampoContent> ContenutoCampi = new List<CampoContent>();

                if (taskElenco.TIPO == "TRACCIATO" || (IsRichiestaTE && taskElenco.TIPO == "TRACCIATO-TE"))
                {
                    ContenutoCampi =
                    MaternitaCongediManager.GetTracciatoEsploso((int)taskElenco.ID_TRACCIATO_DEW, (int)taskElenco.PROGRESSIVO_TRACCIATO_DEW,
                    taskInCorso.INPUT);
                }

                model.ContenutoCampi.Add(new ContenutoCampiPerTask()
                {
                    ID_task = taskInCorso.ID,
                    Contenuti = ContenutoCampi
                });
            }

            return View("_tasksRecap", model);
        }
        public ActionResult SospendiTracciato(int id, bool sospendi)
        {
            var db = new IncentiviEntities();
            var task = db.XR_MAT_TASK_IN_CORSO.Where(x => x.ID == id).FirstOrDefault();
            if (sospendi)
            {
                task.BLOCCATA_DA_OPERATORE = CommonHelper.GetCurrentUserMatricola();
                task.BLOCCATA_DATETIME = DateTime.Now;
            }
            else
            {
                task.BLOCCATA_DA_OPERATORE = null;
                task.BLOCCATA_DATETIME = null;
            }
            db.SaveChanges();

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }

        //public List<DettaglioGiorniPerMese> TogliSospesi(List<DettaglioGiorniPerMese> ElencoGiorniPermese, XR_MAT_RICHIESTE richiesta,
        //    List<string> Leccezioni)
        //{
        //    MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl =
        //        new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
        //    cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
        //            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
        //            CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

        //    foreach (var elenco in ElencoGiorniPermese)
        //    {
        //        DateTime Dinizio = elenco.RiferimentoPrimoDelMese;
        //        var Response = cl.GetRuoli(richiesta.MATRICOLA, new DateTime(Dinizio.Year, Dinizio.Month, 1), "   ");
        //        List<DateTime> DateSospese = new List<DateTime>();
        //        foreach (var data in elenco.ElencoGiorni)
        //        {
        //            var stato = Response.Eccezioni.Where(x => x.DataDocumento == data.DataDa &&
        //            Leccezioni.Contains(data.CodiceEccezione))
        //            .Select(x => x.StatoEccezione).FirstOrDefault();
        //            if (stato == CommonHelper.GetParametri<string>(EnumParametriSistema.StatoEccezioneCongedi)[0])
        //            {
        //                DateSospese.Add(data.DataDa);
        //            }
        //        }
        //        if (DateSospese.Any())
        //        {
        //            elenco.ElencoGiorni.RemoveAll(x => DateSospese.Contains(x.DataDa));
        //        }
        //    }
        //    return ElencoGiorniPermese;
        //}


        public ActionResult getTasks(int idrichiesta, int incongruenze, string giornicedolino, string Importo13ma,
            string Importo14ma, string ImportoPremio, string ImportoTotale, string TotaleGiornaliero, string giorniforzati,
            string meseanno = null,
            bool IsFromTE = false, string g26mesi = "", string datiTE = null)
        {
            var db = new IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            DatiTE[] DatiFromTEview = null;
            if (!String.IsNullOrWhiteSpace(datiTE))
            {
                DatiFromTEview = Newtonsoft.Json.JsonConvert.DeserializeObject<DatiTE[]>(datiTE);
            }


            int? meseSpecifico = null;
            int? annoSpecifico = null;

            if (meseanno != null)
            {
                meseSpecifico = Convert.ToInt32(meseanno.Split('/')[0]);
                annoSpecifico = Convert.ToInt32(meseanno.Split('/')[1]);

            }
            DettaglioAmministrazioneModel AmmModel = new DettaglioAmministrazioneModel();
            if (!IsFromTE)
            {
                AmmModel = GetDettaglioAmministrazioneModel(idrichiesta, meseSpecifico, annoSpecifico);
            }

            List<Mesi26fromFrontEnd> M26 = new List<Mesi26fromFrontEnd>();
            if (!String.IsNullOrWhiteSpace(g26mesi))
            {//10/2021:26/26|11/2021:26/26|12/2021:26/26
                DateTime D1 = rich.INIZIO_GIUSTIFICATIVO ?? rich.DATA_INIZIO_MATERNITA.Value;
                DateTime D2 = rich.FINE_GIUSTIFICATIVO ?? rich.DATA_FINE_MATERNITA.Value;

                string[] p = g26mesi.Split('|');
                foreach (string s in p)
                {
                    bool ModificatoDaCalendarioPDF = false;
                    string mesanno = s.Split(':')[0];
                    int anno = Convert.ToInt32(mesanno.Split('/')[1]);
                    int mese = Convert.ToInt32(mesanno.Split('/')[0]);
                    DateTime D = new DateTime(anno, mese, 1);
                    string g26 = s.Split(':')[1];
                    g26 = g26.Split('/')[0];
                    double val = Convert.ToDouble(g26.Replace(".", ","));
                    if (rich.ASSENZA_LUNGA == true && AmmModel.FormaContratto != "K" &&
                        D1.Year == anno && D1.Month == mese && D1.Day > 1 &&
                        D2 >= new DateTime(D1.Year, D1.Month, DateTime.DaysInMonth(D1.Year, D1.Month)))
                    {
                        int? Calcolo26mi = MaternitaCongediManager.GetGiorni26ComeCalendarioPDF(rich.MATRICOLA, D1);
                        if (Calcolo26mi != null)
                        {
                            val = (double)Calcolo26mi;
                            ModificatoDaCalendarioPDF = true;
                        }
                    }
                    M26.Add(new Mesi26fromFrontEnd() { PrimoDelMese = D, Giorni26mi = val, ModificatoDaCalendarioPDF = ModificatoDaCalendarioPDF });
                }
            }

            //////////////////////TRATT ECONOMICO ////////////////////////////
            if (IsFromTE)
            {
                TaskModel tmodel = new TaskModel(idrichiesta) { IsTrattamentoEconomico = true };
                tmodel.Giorni26FrontEnd = M26;
                tmodel.ListaTaskPronti = MaternitaCongediManager.GetTaskPronti(idrichiesta, tmodel, IsFromTE);

                return View("_tasksPronti", tmodel);
            }
            ///////////////////////////////////////////////






            if (MaternitaCongediManager.IsHC(rich))
            {
                AmmModel.TotaleGiornalieroHC = Convert.ToDouble(TotaleGiornaliero);
            }

            string EccezioneRisultante = MaternitaCongediManager.GetEccezioneRisultante(rich);

            if (EccezioneRisultante == "MT")
            {

                //DateTime dataNascitaPE =
                //    rich.DATA_PARTO == null ? rich.DATA_PRESUNTA_PARTO.Value :
                //    rich.DATA_PRESUNTA_PARTO != null && rich.DATA_PARTO < rich.DATA_PRESUNTA_PARTO.Value ? rich.DATA_PRESUNTA_PARTO.Value :
                //    rich.DATA_PARTO.Value
                //   ;

                DateTime dataNascitaPE;
                DateTime dataFine;
                if (rich.DATA_PARTO == null)
                {
                    dataNascitaPE = rich.DATA_PRESUNTA_PARTO.Value;
                    dataFine = dataNascitaPE.AddMonths(3);
                }
                else
                {
                    if (rich.DATA_PRESUNTA_PARTO > rich.DATA_PARTO)//se nasce prima
                    {
                        dataNascitaPE = rich.DATA_PARTO.Value;// rich.DATA_PRESUNTA_PARTO.Value;
                        if (rich.TIPO_FLEX_MATERNITA == 1)//giovannini
                        {
                            dataFine = rich.DATA_PARTO.Value.AddMonths(3);
                        }
                        else
                        {
                            dataFine = rich.DATA_PARTO.Value.AddMonths(3)
                           .AddDays((rich.DATA_PRESUNTA_PARTO.Value - rich.DATA_PARTO.Value).TotalDays);
                        }

                    }
                    else
                    {
                        dataNascitaPE = rich.DATA_PARTO.Value;
                        dataFine = dataNascitaPE.AddMonths(3);
                    }
                }



                //if (rich.DATA_INIZIO_MATERNITA.Value == rich.DATA_PRESUNTA_PARTO.Value.AddMonths(-2))
                if (rich.DATA_PRESUNTA_PARTO != null && (rich.DATA_INIZIO_MATERNITA.Value > rich.DATA_PRESUNTA_PARTO.Value.AddMonths(-2)) && (rich.DATA_INIZIO_MATERNITA.Value <= rich.DATA_PRESUNTA_PARTO.Value))
                    dataFine = dataFine
                         .AddDays((rich.DATA_INIZIO_MATERNITA.Value - rich.DATA_PRESUNTA_PARTO.Value.AddMonths(-2)).TotalDays);

                if (rich.DATA_INIZIO_MATERNITA == rich.DATA_PARTO && rich.DATA_PARTO != null && rich.DATA_PARTO == rich.DATA_PRESUNTA_PARTO)
                {
                    dataFine = rich.DATA_PARTO.Value.AddMonths(5);
                }


                // else if (rich.DATA_INIZIO_MATERNITA.Value == rich.DATA_PRESUNTA_PARTO.Value.AddDays(-30) || rich.DATA_INIZIO_MATERNITA.Value == rich.DATA_PRESUNTA_PARTO.Value.AddMonths(-1))
                //     dataFine = dataFine.AddDays((rich.DATA_INIZIO_MATERNITA.Value - rich.DATA_INIZIO_MATERNITA.Value.AddMonths(-1)).TotalDays);
                // else if ((rich.DATA_INIZIO_MATERNITA.Value > rich.DATA_PRESUNTA_PARTO.Value.AddMonths(-1)) && (rich.DATA_INIZIO_MATERNITA.Value <= rich.DATA_PRESUNTA_PARTO.Value))
                //      dataFine = dataFine
                //        .AddDays((rich.DATA_INIZIO_MATERNITA.Value - rich.DATA_PRESUNTA_PARTO.Value.AddMonths(-1)).TotalDays);


                // else if (rich.DATA_INIZIO_MATERNITA.Value == rich.DATA_PRESUNTA_PARTO.Value)
                //     dataFine = dataFine.AddMonths(2);//.AddDays(1);

                if (rich.DATA_PARTO != null)
                {

                }

                //Aggiungo MT dall'ultima eccezione fino a data fine
                var idx = AmmModel.ElencoGiorniPerMese.Count() - 1;
                var lastMonth = AmmModel.ElencoGiorniPerMese[idx];
                var lastEccez = lastMonth.ElencoGiorni.LastOrDefault(x => x.CodiceEccezione == EccezioneRisultante);
                // lastEccez = null;
                while (lastEccez == null)
                {
                    idx--;
                    lastMonth = AmmModel.ElencoGiorniPerMese[idx];
                    lastEccez = lastMonth.ElencoGiorni.LastOrDefault(x => x.CodiceEccezione == EccezioneRisultante);
                }

                var dataRif = lastEccez.DataDa.AddDays(1);

                if (rich.TIPO_FLEX_MATERNITA == 1 && dataRif > dataFine)
                    dataRif = dataFine.AddDays(1);


                string codEcc = "MT";
                string desEcc = "Maternita";

                while (dataRif <= dataFine.AddMonths(1))
                {
                    if (lastMonth.RiferimentoPrimoDelMese.Month != dataRif.Month)
                    {
                        if (codEcc == "MU" && DateTime.DaysInMonth(lastMonth.RiferimentoPrimoDelMese.Year, lastMonth.RiferimentoPrimoDelMese.Month) == dataRif.AddDays(-1).Day)
                        {

                            lastMonth.TotaleGiorni = lastMonth.ElencoGiorni.Where(x => x.DataDa.DayOfWeek != DayOfWeek.Sunday).Count() - 1;
                        }

                        lastMonth = new DettaglioGiorniPerMese()
                        {
                            RiferimentoPrimoDelMese = dataRif,
                            ElencoGiorni = new List<DettaglioGiorniModel>(),
                            TotaleGiorni = 0
                        };
                        AmmModel.ElencoGiorniPerMese.Add(lastMonth);
                    }

                    if (dataRif == dataFine.AddDays(1))
                    {

                        codEcc = "MU";
                        desEcc = "Quarto mese RAI";
                    }

                    lastMonth.ElencoGiorni.Add(new DettaglioGiorniModel()
                    {
                        CodiceEccezione = codEcc,
                        DescEccezione = desEcc,
                        DataDa = dataRif,
                        DataA = null,
                        NumeroGiorniRuoli = dataRif.DayOfWeek != DayOfWeek.Sunday ? 1f : 0,
                        NumeroGiorniGapp = dataRif.DayOfWeek != DayOfWeek.Sunday ? 1f : 0,
                        StatoEccez = null,
                        IsMTfiller = true
                    });

                    //if (codEcc=="MT" && dataRif.DayOfWeek != DayOfWeek.Sunday)
                    if (dataRif.DayOfWeek != DayOfWeek.Sunday)
                        lastMonth.TotaleGiorni++;

                    dataRif = dataRif.AddDays(1);
                }
            }


            //toglisosp
            AmmModel.ElencoGiorniPerMese.ForEach(x => x.ElencoGiorni.RemoveAll(z => z.StatoEccez ==
             CommonHelper.GetParametri<string>(EnumParametriSistema.StatoEccezioneCongedi)[0]));

            //toglicestinate
            AmmModel.ElencoGiorniPerMese.ForEach(x => x.ElencoGiorni.RemoveAll(z => z.StatoEccez ==
             CommonHelper.GetParametri<string>(EnumParametriSistema.StatoEccezioneCongedi)[1]));

            List<DettaglioGiorniPerMese> mesi = new List<DettaglioGiorniPerMese>();
            if (rich.PIANIFICAZIONE_BASE_ORARIA == true)
                mesi = AmmModel.ElencoGiorniPerMese.Where(x => x.ElencoGiorni.Any(z =>
                    z.CodiceEccezione == EccezioneRisultante + "M"
                 || z.CodiceEccezione == EccezioneRisultante + "P"
                 || z.CodiceEccezione == EccezioneRisultante + "Q")).ToList();
            else
                mesi = AmmModel.ElencoGiorniPerMese.Where(x => x.ElencoGiorni.Any(z => z.CodiceEccezione == EccezioneRisultante)).ToList();


            string giorni = giornicedolino.Split('/')[0];
            float g = Convert.ToSingle(giorni);

            TaskModel model = new TaskModel(idrichiesta);

            model.GiorniForzatiCSV = giorniforzati;
            model.Giorni26FrontEnd = M26;
            model.Importo13ma = Importo13ma;
            model.Importo14ma = Importo14ma;
            model.ImportoPremio = ImportoPremio;

            model.DateRiferimentoPrimoDelMeseTaskNecessari = mesi.Select(x => x.RiferimentoPrimoDelMese).ToList();
            if (model.DateRiferimentoPrimoDelMeseTaskNecessari.Any())
            {
                model.DateRiferimentoPrimoDelMeseTaskNecessari.Add(model.DateRiferimentoPrimoDelMeseTaskNecessari.Last().AddMonths(1));
            }

            model.StatoFinalePratica = GetStatoDiUscita(incongruenze, g);
            model.DettaglioAmmModel = AmmModel;
            model.EccezioneRisultante = EccezioneRisultante;

            DateTime? DataInizioPratica = model.Richiesta.INIZIO_GIUSTIFICATIVO;
            if (DataInizioPratica == null) DataInizioPratica = model.Richiesta.DATA_INIZIO_MATERNITA;
            model.TracciatiEsplosi = new List<ContenutoCampiPerMeseTask>();

            ///////////// NUOVO TASKPRONTI ///////////////////////////////////////////////////////

            if (!String.IsNullOrWhiteSpace(ImportoTotale))
            {
                model.DettaglioAmmModel.ImportoFinale = Convert.ToSingle(ImportoTotale);
            }

            foreach (var e in model.DettaglioAmmModel.ElencoGiorniPerMese)
            {
                e.TotaleGiorni = (float)Math.Round(e.TotaleGiorni, 2);
            }
            model.ListaTaskPronti = MaternitaCongediManager.GetTaskPronti(idrichiesta, model, IsFromTE);
            //if (model.ListaTaskPronti.Any())
            //var lista = model.ListaTaskPronti.Where(x => x.TaskInCorso.ID_TASK == 15).ToList();

            //model = GestisciEccezioniCestinate(model);
            return View("_tasksPronti", model);


            //////////////////////////////////////////////////////////////////
            foreach (var item in model.Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK.Where(x =>
                        x.STATO_PRATICA == model.StatoFinalePratica))
            {
                var taskCurrent = db.XR_MAT_ELENCO_TASK.Where(x => x.ID == item.ID_TASK).FirstOrDefault();
                if (taskCurrent.TIPO == "SERVIZIO")
                    continue;

                bool SingolaIstanza = (taskCurrent != null && taskCurrent.NOME_TASK.Trim() == "STORNO CEDOLINO");

                foreach (var mese in model.DateRiferimentoPrimoDelMeseTaskNecessari)
                {

                    DettaglioGiorniPerMese meserif = model.DettaglioAmmModel.ElencoGiorniPerMese.Where(x => x.RiferimentoPrimoDelMese == mese).FirstOrDefault();
                    if (meserif == null) continue;

                    List<DettaglioGiorniModel> PeriodiEccezione = meserif.ElencoGiorni.Where(x => x.CodiceEccezione == model.EccezioneRisultante).ToList();
                    if (PeriodiEccezione.Count > 1)
                    {
                        PeriodiEccezione = MaternitaCongediManager.FondiPeriodiEccezione(PeriodiEccezione, rich.MATRICOLA, rich.XR_MAT_CATEGORIE.SKIP_FUSIONE_PERIODI, model.DettaglioAmmModel.FormaContratto);
                        meserif.ElencoGiorni.RemoveAll(x => x.CodiceEccezione == model.EccezioneRisultante);
                        meserif.ElencoGiorni.AddRange(PeriodiEccezione);
                    }
                    foreach (var periodoEccezione in PeriodiEccezione)
                    {
                        string testoTracciato;
                        if (periodoEccezione.DataA == null) periodoEccezione.DataA = periodoEccezione.DataDa;

                        string dataInizioNote = periodoEccezione.DataDa.ToString("dd/MM/yyyy");
                        string dataFineNote = periodoEccezione.DataA.Value.ToString("dd/MM/yyyy");

                        XR_MAT_TASK_IN_CORSO trGiaSalvato = null;
                        if (SingolaIstanza)
                        {
                            trGiaSalvato = rich.XR_MAT_TASK_IN_CORSO.Where(x => x.MESE == mese.Month &&
                           x.ANNO == mese.Year && x.ID_TASK == item.ID_TASK).FirstOrDefault();
                        }
                        else
                        {
                            trGiaSalvato = rich.XR_MAT_TASK_IN_CORSO.Where(x => x.MESE == mese.Month &&
                          x.ANNO == mese.Year && x.ID_TASK == item.ID_TASK && x.NOTE != null && x.NOTE.Contains(dataInizioNote)
                          && x.NOTE.Contains(dataFineNote)).FirstOrDefault();
                        }


                        if (trGiaSalvato != null)
                        {
                            testoTracciato = trGiaSalvato.INPUT;
                        }
                        else
                        {
                            testoTracciato = MaternitaCongediManager.GetCampiTracciato(
                          (int)item.XR_MAT_ELENCO_TASK.ID_TRACCIATO_DEW,
                          (int)item.XR_MAT_ELENCO_TASK.PROGRESSIVO_TRACCIATO_DEW,
                          rich,
                          model.EccezioneRisultante,
                          periodoEccezione.DataDa.ToString("dd/MM/yyyy"),
                          periodoEccezione.DataA.Value.ToString("dd/MM/yyyy"),
                          periodoEccezione.NumeroGiorniRuoli > 0 ?
                          periodoEccezione.NumeroGiorniRuoli.ToString() : periodoEccezione.NumeroGiorniGapp.ToString(),
                          DataInizioPratica.Value,
                           model.DettaglioAmmModel.ImportoFinale.ToString(),
                           Importo13ma, Importo14ma, ImportoPremio
                          );
                        }


                        List<CampoContent> ContenutoCampi = new List<CampoContent>();
                        if (!string.IsNullOrWhiteSpace(testoTracciato))
                        {
                            ContenutoCampi =
                              MaternitaCongediManager.GetTracciatoEsploso(
                                  (int)item.XR_MAT_ELENCO_TASK.ID_TRACCIATO_DEW,
                                  (int)item.XR_MAT_ELENCO_TASK.PROGRESSIVO_TRACCIATO_DEW,
                                   testoTracciato);
                        }

                        model.TracciatiEsplosi.Add(new ContenutoCampiPerMeseTask()
                        {
                            Campi = ContenutoCampi,
                            anno = mese.Year,
                            mese = mese.Month,
                            DataRiferimentoPrimoMese = mese,
                            PeriodoDa = periodoEccezione.DataDa,
                            PeriodoA = periodoEccezione.DataA.Value,
                            IdElencoTask = item.ID_TASK,
                            TracciatoIntero = testoTracciato

                        });

                    }
                }
            }

            foreach (DateTime Dmese in model.DateRiferimentoPrimoDelMeseTaskNecessari)
            {
                TaskDaVisualizzare Tdv = new TaskDaVisualizzare()
                {
                    DataPrimoDelMese = Dmese
                };
                bool MeseHaTracciati = model.DettaglioAmmModel.ElencoGiorniPerMese
                    .Where(x => x.RiferimentoPrimoDelMese == Dmese && x.ElencoGiorni.Any(z => z.CodiceEccezione == EccezioneRisultante)).Count() > 0;

                //var m = model.DettaglioAmmModel.ElencoGiorniPerMese
                //    .Where(x => x.RiferimentoPrimoDelMese == Dmese && x.ElencoGiorni.Any(z=>z.CodiceEccezione==EccezioneRisultante)).ToList();

                foreach (var item in model.Richiesta.XR_MAT_CATEGORIE.XR_MAT_CATEGORIA_TASK
                    .Where(x => x.STATO_PRATICA == model.StatoFinalePratica).OrderBy(x => x.PROGRESSIVO))
                {
                    if (item.XR_MAT_ELENCO_TASK.NOME_TASK.Trim() == "STORNO CEDOLINO" && MeseHaTracciati)
                    {
                        bool NecessarioStornoCedolino = Dmese < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        bool giaNeiTask = MaternitaCongediManager.GiaPresenteStornoCedolinoNeiTask(model.Richiesta.MATRICOLA,
                            Dmese.Year, Dmese.Month, model.Richiesta.ID);

                        if (NecessarioStornoCedolino && !giaNeiTask)
                        {
                            TaskDaVisualizzareInfo info = new TaskDaVisualizzareInfo();
                            info.Anno = Dmese.Year;
                            info.Mese = Dmese.Month;
                            info.IdTask = item.ID_TASK;
                            info.Nome = item.XR_MAT_ELENCO_TASK.NOME_TASK;
                            info.Tipo = "TRACCIATO";

                            Tdv.Info.Add(info);
                        }
                    }
                    if (!MeseHaTracciati && item.XR_MAT_ELENCO_TASK.TIPO == "TRACCIATO")
                    {
                        continue;
                    }
                    if (item.XR_MAT_ELENCO_TASK.TIPO == "SERVIZIO")
                    {
                        var t = myRaiCommonManager.MaternitaCongediManager.IsTaskPresentDB(model.Richiesta.ID,
                       item.ID_TASK, Dmese.Year, Dmese.Month, null, null
                       );
                        if (t != null)
                        {
                            Tdv.IdRichiestaCheGiaVisualizzaTask = t.ID_RICHIESTA;
                            continue;
                        }
                        if (model.DateRiferimentoPrimoDelMeseTaskNecessari.IndexOf(Dmese) > 0)
                        {
                            TaskDaVisualizzareInfo info = new TaskDaVisualizzareInfo();
                            info.Anno = Dmese.Year;
                            info.Mese = Dmese.Month;
                            info.IdTask = item.ID_TASK;
                            info.Nome = item.XR_MAT_ELENCO_TASK.NOME_TASK;
                            info.Tipo = "SERVIZIO";

                            Tdv.Info.Add(info);
                        }
                    }
                }
                model.TaskDaVisualizzareModel.Add(Tdv);
            }

            return View("_tasks", model);


        }

        //private TaskModel GestisciEccezioniCestinate(TaskModel model)
        //{
        //    foreach (var e in model.DettaglioAmmModel.ElencoGiorniPerMese)
        //    {
        //        foreach (var el in e.ElencoGiorni)
        //        {
        //            DateTime D1 = el.DataDa;
        //            for (DateTime Dcurrent = D1; Dcurrent <= el.DataA; Dcurrent = Dcurrent.AddDays(1))
        //            {

        //            }
        //        }


        //    }
        //    return model;
        //}

        public int GetStatoDiUscita(int incongruenze, float giornicedolino)
        {
            //if (incongruenze == 0 && giornicedolino <= 26)
            return 100;
            //else
            //   return 0;
        }
        [HttpPost]
        public ActionResult postfileFromAmm(HttpPostedFileBase file, string tipo, string nome, int cat, int id)
        {
            var db = new IncentiviEntities();
            XR_MAT_ALLEGATI A = new XR_MAT_ALLEGATI();

            try
            {
                A.NOMEFILE = nome;
                A.MATRICOLA = CommonHelper.GetCurrentUserMatricola();
                A.UPLOAD_DA_AMMIN = true;
                A.TIPOLOGIA = tipo;
                A.ID_RICHIESTA = id;
                A.ID_STATO = 20;

                MemoryStream target = new MemoryStream();
                file.InputStream.CopyTo(target);
                A.BYTECONTENT = target.ToArray();
                A.DATA_CONSOLIDATO = DateTime.Now;
                A.DATA_INVIATO = DateTime.Now;
                db.XR_MAT_ALLEGATI.Add(A);
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            catch (Exception ex)
            {

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }

        }
        public ActionResult CancellaAllegato(int idallegato)
        {
            var db = new IncentiviEntities();
            var A = db.XR_MAT_ALLEGATI.Where(x => x.ID == idallegato).FirstOrDefault();
            if (A != null)
            {
                try
                {
                    db.XR_MAT_ALLEGATI.Remove(A);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = ex.Message }
                    };
                }
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "allegato non trovato" }
                };
            }
        }

        public List<DettaglioCedolinoItemModel> GetItemsCedolino(DettaglioCedolinoModel Model, List<XR_MAT_VOCI_CEDOLINO> VociCedolinoSalvate, int? mese = null, int? anno = null)
        {
            var ListaItemCedolino = new List<DettaglioCedolinoItemModel>();
            ListaItemCedolino.Add(new DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A9",
                Etichetta = "Stipendio",
                NameAttuale = "stipendio",
                NameHRDW = "stipendio_old",
                ValoreAttuale = Model.VociGeneriche.stipendio,
                ValoreHRDW = Model.VociGeneriche.stipendio_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "*"
            });
            ListaItemCedolino.Add(new DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A10",
                Etichetta = "Indennità di contingenza",
                NameAttuale = "ind_contingenza",
                NameHRDW = "ind_contingenza_old",
                ValoreAttuale = Model.VociGeneriche.ind_contingenza,
                ValoreHRDW = Model.VociGeneriche.ind_contingenza_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "*"
            });

            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A11",
                Etichetta = "Indennità di mensa",
                NameAttuale = "ind_mensa",
                NameHRDW = "ind_mensa_old",
                ValoreAttuale = Model.VociGeneriche.ind_mensa,
                ValoreHRDW = Model.VociGeneriche.ind_mensa_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "*"
            });


            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A12",
                Etichetta = "Indennità 110",
                NameAttuale = "ind_110",
                NameHRDW = "ind_110_old",
                ValoreAttuale = Model.VociGeneriche.ind_110,
                ValoreHRDW = Model.VociGeneriche.ind_110_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });

            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A13",
                Etichetta = "Indennità 120",
                NameAttuale = "ind_120",
                NameHRDW = "ind_120_old",
                ValoreAttuale = Model.VociGeneriche.ind_120,
                ValoreHRDW = Model.VociGeneriche.ind_120_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });

            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A14",
                Etichetta = "Indennità 123",
                NameAttuale = "ind_123",
                NameHRDW = "ind_123_old",
                ValoreAttuale = Model.VociGeneriche.ind_123,
                ValoreHRDW = Model.VociGeneriche.ind_123_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A15",
                Etichetta = "Indennità 129",
                NameAttuale = "ind_129",
                NameHRDW = "ind_129_old",
                ValoreAttuale = Model.VociGeneriche.ind_129,
                ValoreHRDW = Model.VociGeneriche.ind_129_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A16",
                Etichetta = "Indennità 130",
                NameAttuale = "ind_130",
                NameHRDW = "ind_130_old",
                ValoreAttuale = Model.VociGeneriche.ind_130,
                ValoreHRDW = Model.VociGeneriche.ind_130_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A17",
                Etichetta = "Indennità 114",
                NameAttuale = "ind_114",
                NameHRDW = "ind_114_old",
                ValoreAttuale = Model.VociGeneriche.ind_114,
                ValoreHRDW = Model.VociGeneriche.ind_114_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A18",
                Etichetta = "Indennità 119",
                NameAttuale = "ind_119",
                NameHRDW = "ind_119_old",
                ValoreAttuale = Model.VociGeneriche.ind_119,
                ValoreHRDW = Model.VociGeneriche.ind_119_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A19",
                Etichetta = "Indennità 12B",
                NameAttuale = "ind_12B",
                NameHRDW = "ind_12B_old",
                ValoreAttuale = Model.VociGeneriche.ind_12B,
                ValoreHRDW = Model.VociGeneriche.ind_12B_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A20",
                Etichetta = "Indennità 13B",
                NameAttuale = "ind_13B",
                NameHRDW = "ind_13B_old",
                ValoreAttuale = Model.VociGeneriche.ind_13B,
                ValoreHRDW = Model.VociGeneriche.ind_13B_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A21",
                Etichetta = "Indennità 14E",
                NameAttuale = "ind_14E",
                NameHRDW = "ind_14E_old",
                ValoreAttuale = Model.VociGeneriche.ind_14E,
                ValoreHRDW = Model.VociGeneriche.ind_14E_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A22",
                Etichetta = "Indennità 14F",
                NameAttuale = "ind_14F",
                NameHRDW = "ind_14F_old",
                ValoreAttuale = Model.VociGeneriche.ind_14F,
                ValoreHRDW = Model.VociGeneriche.ind_14F_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A23",
                Etichetta = "Minimo",
                NameAttuale = "minimo",
                NameHRDW = "minimo_old",
                ValoreAttuale = Model.VociGeneriche.minimo,
                ValoreHRDW = Model.VociGeneriche.minimo_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A24",
                Etichetta = "Superminimo",
                NameAttuale = "superminimo",
                NameHRDW = "superminimo_old",
                ValoreAttuale = Model.VociGeneriche.superminimo,
                ValoreHRDW = Model.VociGeneriche.superminimo_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });


            ////////////////// ultime inserite 21/02/2022////////////////////////////////////////////////
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A113",
                Etichetta = "Indennità 113",
                NameAttuale = "ind_113",
                NameHRDW = "ind_113_old",
                ValoreAttuale = Model.VociGeneriche.ind_113,
                ValoreHRDW = Model.VociGeneriche.ind_113_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A115",
                Etichetta = "Indennità 115",
                NameAttuale = "ind_115",
                NameHRDW = "ind_115_old",
                ValoreAttuale = Model.VociGeneriche.ind_115,
                ValoreHRDW = Model.VociGeneriche.ind_115_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A11B",
                Etichetta = "Indennità 11B",
                NameAttuale = "ind_11B",
                NameHRDW = "ind_11B_old",
                ValoreAttuale = Model.VociGeneriche.ind_11B,
                ValoreHRDW = Model.VociGeneriche.ind_11B_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A11E",
                Etichetta = "Indennità 11E",
                NameAttuale = "ind_11E",
                NameHRDW = "ind_11E_old",
                ValoreAttuale = Model.VociGeneriche.ind_11E,
                ValoreHRDW = Model.VociGeneriche.ind_11E_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A11F",
                Etichetta = "Indennità 11F",
                NameAttuale = "ind_11F",
                NameHRDW = "ind_11F_old",
                ValoreAttuale = Model.VociGeneriche.ind_11F,
                ValoreHRDW = Model.VociGeneriche.ind_11F_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A11G",
                Etichetta = "Indennità 11G",
                NameAttuale = "ind_11G",
                NameHRDW = "ind_11G_old",
                ValoreAttuale = Model.VociGeneriche.ind_11G,
                ValoreHRDW = Model.VociGeneriche.ind_11G_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A124",
                Etichetta = "Indennità 124",
                NameAttuale = "ind_124",
                NameHRDW = "ind_124_old",
                ValoreAttuale = Model.VociGeneriche.ind_124,
                ValoreHRDW = Model.VociGeneriche.ind_124_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A125",
                Etichetta = "Indennità 125",
                NameAttuale = "ind_125",
                NameHRDW = "ind_125_old",
                ValoreAttuale = Model.VociGeneriche.ind_125,
                ValoreHRDW = Model.VociGeneriche.ind_125_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A126",
                Etichetta = "Indennità 126",
                NameAttuale = "ind_126",
                NameHRDW = "ind_126_old",
                ValoreAttuale = Model.VociGeneriche.ind_126,
                ValoreHRDW = Model.VociGeneriche.ind_126_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A127",
                Etichetta = "Indennità 127",
                NameAttuale = "ind_127",
                NameHRDW = "ind_127_old",
                ValoreAttuale = Model.VociGeneriche.ind_127,
                ValoreHRDW = Model.VociGeneriche.ind_127_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A128",
                Etichetta = "Indennità 128",
                NameAttuale = "ind_128",
                NameHRDW = "ind_128_old",
                ValoreAttuale = Model.VociGeneriche.ind_128,
                ValoreHRDW = Model.VociGeneriche.ind_128_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A138",
                Etichetta = "Indennità 138",
                NameAttuale = "ind_138",
                NameHRDW = "ind_138_old",
                ValoreAttuale = Model.VociGeneriche.ind_138,
                ValoreHRDW = Model.VociGeneriche.ind_138_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A13A",
                Etichetta = "Indennità 13A",
                NameAttuale = "ind_13A",
                NameHRDW = "ind_13A_old",
                ValoreAttuale = Model.VociGeneriche.ind_13A,
                ValoreHRDW = Model.VociGeneriche.ind_13A_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A13E",
                Etichetta = "Indennità 13E",
                NameAttuale = "ind_13E",
                NameHRDW = "ind_13E_old",
                ValoreAttuale = Model.VociGeneriche.ind_13E,
                ValoreHRDW = Model.VociGeneriche.ind_13E_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A13P",
                Etichetta = "Indennità 13P",
                NameAttuale = "ind_13P",
                NameHRDW = "ind_13P_old",
                ValoreAttuale = Model.VociGeneriche.ind_13P,
                ValoreHRDW = Model.VociGeneriche.ind_13P_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A13R",
                Etichetta = "Indennità 13R",
                NameAttuale = "ind_13R",
                NameHRDW = "ind_13R_old",
                ValoreAttuale = Model.VociGeneriche.ind_13R,
                ValoreHRDW = Model.VociGeneriche.ind_13R_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A142",
                Etichetta = "Indennità 142",
                NameAttuale = "ind_142",
                NameHRDW = "ind_142_old",
                ValoreAttuale = Model.VociGeneriche.ind_142,
                ValoreHRDW = Model.VociGeneriche.ind_142_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A145",
                Etichetta = "Indennità 145",
                NameAttuale = "ind_145",
                NameHRDW = "ind_145_old",
                ValoreAttuale = Model.VociGeneriche.ind_145,
                ValoreHRDW = Model.VociGeneriche.ind_145_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A14A",
                Etichetta = "Indennità 14A",
                NameAttuale = "ind_14A",
                NameHRDW = "ind_14A_old",
                ValoreAttuale = Model.VociGeneriche.ind_14A,
                ValoreHRDW = Model.VociGeneriche.ind_14A_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A150",
                Etichetta = "Indennità 150",
                NameAttuale = "ind_150",
                NameHRDW = "ind_150_old",
                ValoreAttuale = Model.VociGeneriche.ind_150,
                ValoreHRDW = Model.VociGeneriche.ind_150_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A152",
                Etichetta = "Indennità 152",
                NameAttuale = "ind_152",
                NameHRDW = "ind_152_old",
                ValoreAttuale = Model.VociGeneriche.ind_152,
                ValoreHRDW = Model.VociGeneriche.ind_152_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A153",
                Etichetta = "Indennità 153",
                NameAttuale = "ind_153",
                NameHRDW = "ind_153_old",
                ValoreAttuale = Model.VociGeneriche.ind_153,
                ValoreHRDW = Model.VociGeneriche.ind_153_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A157",
                Etichetta = "Indennità 157",
                NameAttuale = "ind_157",
                NameHRDW = "ind_157_old",
                ValoreAttuale = Model.VociGeneriche.ind_157,
                ValoreHRDW = Model.VociGeneriche.ind_157_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });

            if (Model.VociGeneriche.ind_157 > 0 || Model.VociGeneriche.ind_153 > 0 || Model.VociGeneriche.ind_152 > 0 ||
                Model.VociGeneriche.ind_150 > 0 || Model.VociGeneriche.ind_14A > 0 || Model.VociGeneriche.ind_145 > 0 ||
                Model.VociGeneriche.ind_142 > 0 || Model.VociGeneriche.ind_13R > 0 || Model.VociGeneriche.ind_13P > 0 ||
                Model.VociGeneriche.ind_13A > 0 || Model.VociGeneriche.ind_13E > 0 || Model.VociGeneriche.ind_128 > 0 ||
                Model.VociGeneriche.ind_138 > 0 || Model.VociGeneriche.ind_124 > 0 || Model.VociGeneriche.ind_125 > 0 ||
                Model.VociGeneriche.ind_126 > 0 || Model.VociGeneriche.ind_127 > 0 || Model.VociGeneriche.ind_11E > 0 ||
                Model.VociGeneriche.ind_11F > 0 || Model.VociGeneriche.ind_11G > 0 || Model.VociGeneriche.ind_113 > 0 ||
                Model.VociGeneriche.ind_115 > 0 || Model.VociGeneriche.ind_11B > 0
                )
            {

            }
            /////////////////////////////////////////////////////////////////////////////////
            if (Model.TipoDipendente == "G")
            {
                Model.VociGeneriche.DG55 = CalcolaValoreEccezione("DG55", Model);
                Model.VociGeneriche.LN20 = CalcolaValoreEccezione("LN20", Model);
                Model.VociGeneriche.LN16 = CalcolaValoreEccezione("LN16", Model);
                Model.VociGeneriche.LN25 = CalcolaValoreEccezione("LN25", Model);
                Model.VociGeneriche.LN50 = CalcolaValoreEccezione("LN50", Model);
                Model.VociGeneriche.PDCO = CalcolaValoreEccezione("PDCO", Model);
                Model.VociGeneriche.LF80 = CalcolaValoreEccezione("LF80", Model);
                Model.VociGeneriche.LF36 = CalcolaValoreEccezione("LF36", Model);
                Model.VociGeneriche.LEXF = CalcolaValoreEccezione("LEXF", Model);
                Model.VociGeneriche.AR20 = CalcolaValoreEccezione("AR20", Model);
                Model.VociGeneriche.RPAF = CalcolaValoreEccezione("RPAF", Model);

                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "G",
                    CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "DG55",
                        NameAttuale = "DG55",
                        ValoreAttuale = Model.VociGeneriche.DG55,
                        id_label = "dg55",
                        id_hidden = "dg55hid",
                        GiorniEccezione = Model.QuantitaEccezioni.Where(x => x.eccezione == "DG55").Select(x => x.totali).FirstOrDefault()
                    }
                });
                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "G",
                    CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "LN16",
                        NameAttuale = "LN16",
                        ValoreAttuale = Model.VociGeneriche.LN16,
                        id_label = "ln16",
                        id_hidden = "ln16hid",
                        GiorniEccezione = Model.QuantitaEccezioni.Where(x => x.eccezione == "LN16").Select(x => x.totali).FirstOrDefault()

                    }
                });

                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "G",
                    CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "LN20",
                        NameAttuale = "LN20",
                        ValoreAttuale = Model.VociGeneriche.LN20,
                        id_label = "ln20",
                        id_hidden = "ln20hid",
                        GiorniEccezione = Model.QuantitaEccezioni.Where(x => x.eccezione == "LN20").Select(x => x.totali).FirstOrDefault()

                    }
                });
                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "G",
                    CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "LN25",
                        NameAttuale = "LN25",
                        ValoreAttuale = Model.VociGeneriche.LN25,
                        id_label = "ln25",
                        id_hidden = "ln25hid",
                        GiorniEccezione = Model.QuantitaEccezioni.Where(x => x.eccezione == "LN25").Select(x => x.totali).FirstOrDefault()

                    }
                });
                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "G",
                    CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "LN50",
                        NameAttuale = "LN50",
                        ValoreAttuale = Model.VociGeneriche.LN50,
                        id_label = "ln50",
                        id_hidden = "ln50hid",
                        GiorniEccezione = Model.QuantitaEccezioni.Where(x => x.eccezione == "LN50").Select(x => x.totali).FirstOrDefault()

                    }
                });
                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "G",
                    CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "PDCO",
                        NameAttuale = "PDCO",
                        ValoreAttuale = Model.VociGeneriche.PDCO,
                        id_label = "pdco",
                        id_hidden = "pdcohid",
                        GiorniEccezione = Model.QuantitaEccezioni.Where(x => x.eccezione == "PDCO").Select(x => x.totali).FirstOrDefault()

                    }
                });
                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "G",
                    CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "LF80",
                        NameAttuale = "LF80",
                        ValoreAttuale = Model.VociGeneriche.LF80,
                        id_label = "lf80",
                        id_hidden = "lf80hid",
                        GiorniEccezione = Model.QuantitaEccezioni.Where(x => x.eccezione == "LF80").Select(x => x.totali).FirstOrDefault()

                    }
                });

                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "G",
                    CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "LF36",
                        NameAttuale = "LF36",
                        ValoreAttuale = Model.VociGeneriche.LF36,
                        id_label = "lf36",
                        id_hidden = "lf36hid",
                        GiorniEccezione = Model.QuantitaEccezioni.Where(x => x.eccezione == "LF36").Select(x => x.totali).FirstOrDefault()

                    }
                });
                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "G",
                    CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "LEXF",
                        NameAttuale = "LEXF",
                        ValoreAttuale = Model.VociGeneriche.LEXF,
                        id_label = "lexf",
                        id_hidden = "lexfhid",
                        GiorniEccezione = Model.QuantitaEccezioni.Where(x => x.eccezione == "LEXF").Select(x => x.totali).FirstOrDefault()

                    }
                });
                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "G",
                    CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "AR20",
                        NameAttuale = "AR20",
                        ValoreAttuale = Model.VociGeneriche.AR20,
                        id_label = "ar20",
                        id_hidden = "ar20hid",
                        GiorniEccezione = Model.QuantitaEccezioni.Where(x => x.eccezione == "AR20").Select(x => x.totali).FirstOrDefault()

                    }
                });
                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "G",
                    CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                    {
                        Etichetta = "RPAF",
                        NameAttuale = "RPAF",
                        ValoreAttuale = Model.VociGeneriche.RPAF,
                        id_label = "rpaf",
                        id_hidden = "rpafhid",
                        GiorniEccezione = Model.QuantitaEccezioni.Where(x => x.eccezione == "RPAF").Select(x => x.totali).FirstOrDefault()

                    }
                });

            }




            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A12",
                Etichetta = "Elemento distinto",
                NameAttuale = "elemento_distinto",
                NameHRDW = "elemento_distinto_old",
                ValoreAttuale = Model.VociGeneriche.elemento_distinto,
                ValoreHRDW = Model.VociGeneriche.elemento_distinto_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });

            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A13",
                Etichetta = "E.D.R. ACCORDO 7.2.2013",
                NameAttuale = "edr",
                NameHRDW = "edr_old",
                ValoreAttuale = Model.VociGeneriche.edr,
                ValoreHRDW = Model.VociGeneriche.edr_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A14",
                Etichetta = "Indennità 11E/11C",
                NameAttuale = "ind_11e",
                NameHRDW = "ind_11e_old",
                ValoreAttuale = Model.VociGeneriche.ind_11e,
                ValoreHRDW = Model.VociGeneriche.ind_11e_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A14",
                Etichetta = "Indennità 11C",
                NameAttuale = "ind_11c",
                NameHRDW = "ind_11c_old",
                ValoreAttuale = Model.VociGeneriche.ind_11C,
                ValoreHRDW = Model.VociGeneriche.ind_11C_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A15",
                Etichetta = "Indennità 106",
                NameAttuale = "ind_106",
                NameHRDW = "ind_106_old",
                ValoreAttuale = Model.VociGeneriche.ind_106,
                ValoreHRDW = Model.VociGeneriche.ind_106_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A16",
                Etichetta = "Indennità 111",
                NameAttuale = "ind_111",
                NameHRDW = "ind_111_old",
                ValoreAttuale = Model.VociGeneriche.ind_111,
                ValoreHRDW = Model.VociGeneriche.ind_111_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A17",
                Etichetta = "Indennità 131",
                NameAttuale = "ind_131",
                NameHRDW = "ind_131_old",
                ValoreAttuale = Model.VociGeneriche.ind_131,
                ValoreHRDW = Model.VociGeneriche.ind_131_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A18",
                Etichetta = "Indennità 116",
                NameAttuale = "ind_116",
                NameHRDW = "ind_116_old",
                ValoreAttuale = Model.VociGeneriche.ind_116,
                ValoreHRDW = Model.VociGeneriche.ind_116_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A19",
                Etichetta = "Indennità 11A/11B",
                NameAttuale = "ind_11A",
                NameHRDW = "ind_11A_old",
                ValoreAttuale = Model.VociGeneriche.ind_11A,
                ValoreHRDW = Model.VociGeneriche.ind_11A_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A20",
                Etichetta = "Indennità 108",
                NameAttuale = "ind_108",
                NameHRDW = "ind_108_old",
                ValoreAttuale = Model.VociGeneriche.ind_108,
                ValoreHRDW = Model.VociGeneriche.ind_108_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A21",
                Etichetta = "Indennità 109",
                NameAttuale = "ind_109",
                NameHRDW = "ind_109_old",
                ValoreAttuale = Model.VociGeneriche.ind_109,
                ValoreHRDW = Model.VociGeneriche.ind_109_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A22",
                Etichetta = "Indennità 133",
                NameAttuale = "ind_133",
                NameHRDW = "ind_133_old",
                ValoreAttuale = Model.VociGeneriche.ind_133,
                ValoreHRDW = Model.VociGeneriche.ind_133_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A23",
                Etichetta = "Indennità 146",
                NameAttuale = "ind_146",
                NameHRDW = "ind_146_old",
                ValoreAttuale = Model.VociGeneriche.ind_146,
                ValoreHRDW = Model.VociGeneriche.ind_146_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A24",
                Etichetta = "Indennità 147",
                NameAttuale = "ind_147",
                NameHRDW = "ind_147_old",
                ValoreAttuale = Model.VociGeneriche.ind_147,
                ValoreHRDW = Model.VociGeneriche.ind_147_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A25",
                Etichetta = "Indennità 159",
                NameAttuale = "ind_159",
                NameHRDW = "ind_159_old",
                ValoreAttuale = Model.VociGeneriche.ind_159,
                ValoreHRDW = Model.VociGeneriche.ind_159_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A26",
                Etichetta = "Indennità 141",
                NameAttuale = "ind_141",
                NameHRDW = "ind_141_old",
                ValoreAttuale = Model.VociGeneriche.ind_141,
                ValoreHRDW = Model.VociGeneriche.ind_141_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A102",
                Etichetta = "Indennità 102",
                NameAttuale = "ind_102",
                NameHRDW = "ind_102_old",
                ValoreAttuale = Model.VociGeneriche.ind_102,
                ValoreHRDW = Model.VociGeneriche.ind_102_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                CellaCalcoloDaExcel = "A149",
                Etichetta = "Indennità 149",
                NameAttuale = "ind_149",
                NameHRDW = "ind_149_old",
                ValoreAttuale = Model.VociGeneriche.ind_149,
                ValoreHRDW = Model.VociGeneriche.ind_149_old,
                ModificaAbilitata = Model.ModificaAbilitata,
                TipiDipendenteSpettanti = "!G"
            });
            if (!MaternitaCongediManager.IsHC(Model.Richiesta))
            {
                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    CellaCalcoloDaExcel = "A27",
                    Etichetta = "Varie",
                    NameAttuale = "ind_altre",
                    NameHRDW = null,
                    ValoreAttuale = Model.VociGeneriche.ind_altre,
                    ValoreHRDW = 0,
                    ModificaAbilitata = Model.ModificaAbilitata,
                    TipiDipendenteSpettanti = "!G"
                });
            }




            if (Model.TipoDipendente != "G" && !MaternitaCongediManager.IsHC(Model.Richiesta))
            {
                decimal importo = 0;

                var voceStr = VociCedolinoSalvate.Where(x => x.VOCE_CEDOLINO == "str_precedente").FirstOrDefault();
                if (voceStr == null)
                    importo = GetImportoStraordinario(Model, mese, anno);
                else
                    importo = voceStr.VALORE;
                Model.VociGeneriche.str_precedente = (float)importo;

                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "!G",
                    CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                    {
                        Etichetta = "Straordinari mese precedente",
                        NameAttuale = "str_precedente",
                        ValoreAttuale = (float)importo,
                        id_label = "str",
                        id_hidden = "strhid",
                        DettagliSTR = Model.DettagliSTR
                    }
                });
            }

            //ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            //{
            //    CellaCalcoloDaExcel = "A28",
            //    Etichetta = "Straordinari mese precedente",
            //    NameAttuale = "str_precedente",
            //    NameHRDW = null,
            //    ValoreAttuale = Model.VociGeneriche.str_precedente,
            //    ValoreHRDW = 0,
            //    ModificaAbilitata = Model.ModificaAbilitata,
            //    TipiDipendenteSpettanti = "!G"
            //});

            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                TipiDipendenteSpettanti = "*",
                CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                {

                    Etichetta = "13ma mensilità",
                    NameAttuale = "mens_13ma",
                    ValoreAttuale = Model.VociGeneriche.mens_13ma,
                    id_label = "13ma",
                    id_hidden = "13mahid",

                }
            });

            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                TipiDipendenteSpettanti = "*",
                CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                {

                    Etichetta = "14ma mensilità o RED",
                    NameAttuale = "mens_14ma",
                    ValoreAttuale = Model.VociGeneriche.mens_14ma,
                    id_label = "14ma",
                    id_hidden = "14mahid"
                }
            });

            ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
            {
                TipiDipendenteSpettanti = "!G",
                CalcolatoModel = new myRaiCommonModel.DettaglioCedolinoItemCalcolatoModel()
                {

                    Etichetta = "Premio produzione",
                    NameAttuale = "premio_prod",
                    ValoreAttuale = Model.VociGeneriche.premio_prod,
                    id_label = "premio",
                    id_hidden = "premiohid"
                }
            });
            if (!MaternitaCongediManager.IsHC(Model.Richiesta))
            {
                ListaItemCedolino.Add(new myRaiCommonModel.DettaglioCedolinoItemModel()
                {
                    CellaCalcoloDaExcel = Model.TipoDipendente == "G" ? "A41" : "A33",
                    Etichetta = "Redazionale",
                    NameAttuale = "redazionale",
                    NameHRDW = "redazionale_old",
                    ValoreAttuale = Model.VociGeneriche.redazionale,
                    ValoreHRDW = Model.VociGeneriche.redazionale_old,
                    ModificaAbilitata = Model.ModificaAbilitata,
                    TipiDipendenteSpettanti = "*"
                });
            }
            else
            {
                var p = ListaItemCedolino.Where(x => x.CalcolatoModel != null && x.CalcolatoModel.NameAttuale == "premio_prod").FirstOrDefault();
                if (p != null)
                {
                    p.CalcolatoModel.ValoreAttuale = (float)Math.Round(Convert.ToDecimal(Model.VociGeneriche.redazionale / 12), 2);
                }
            }


            //ListaItemCedolino.Add();


            return ListaItemCedolino;
        }



        /*  url: '/MaternitaCongedi/ModCampoTracciato',
                type: "POST",
                data: { id: id, pos:pos, testo:newText },*/

        [HttpPost]
        public ActionResult ModCampoTracciato(int id, int pos, string testo)
        {
            var db = new IncentiviEntities();
            var row = db.XR_MAT_TASK_IN_CORSO.Where(x => x.ID == id).FirstOrDefault();
            string tracciato = row.INPUT;
            tracciato = tracciato.Remove(pos - 1, testo.Length).Insert(pos - 1, testo);
            row.INPUT = tracciato;

            db.SaveChanges();
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }
        public ActionResult GetGestionePeriodi(int idrich)
        {
            myRaiCommonModel.DettaglioGestionePeriodiModel model = new DettaglioGestionePeriodiModel();
            var db = new IncentiviEntities();
            model.Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrich).FirstOrDefault();
            var list = db.XR_MAT_ARRETRATI_DIPENDENTE.Where(x => x.MATRICOLA == model.Richiesta.MATRICOLA).OrderBy(x => x.DATA).ToList();
            if (list.Any() == false)
            {
                string r = MaternitaCongediManager.ImportaArretratiMatricola(model.Richiesta.MATRICOLA);

            }
            list = db.XR_MAT_ARRETRATI_DIPENDENTE.Where(x => x.MATRICOLA == model.Richiesta.MATRICOLA).OrderBy(x => x.DATA).ToList();

            if (list.Any() == false)
                return null;


            model.Periodi = new List<DettaglioGestionePeriodoItem>();
            var RichiesteMatricola = db.XR_MAT_RICHIESTE.Where(x => x.MATRICOLA == model.Richiesta.MATRICOLA).ToList();

            foreach (var item in list)
            {
                DettaglioGestionePeriodoItem dett = new DettaglioGestionePeriodoItem();
                dett.CF = item.CODICE_FISCALE_FIGLIO;
                dett.Data = item.DATA;
                dett.Eccezione = item.ECCEZIONE;
                dett.INPS = item.PROTOCOLLO_INPS;
                dett.PeriodoFine = (item.PERIODO_RIFERIMENTO_A != null ? item.PERIODO_RIFERIMENTO_A.Value.ToString("dd/MM/yyyy") : "");
                dett.PeriodoInizio = (item.PERIODO_RIFERIMENTO_DA != null ? item.PERIODO_RIFERIMENTO_DA.Value.ToString("dd/MM/yyyy") : "");
                dett.id = item.ID;
                model.Periodi.Add(dett);

                bool savedb = false;
                if (String.IsNullOrWhiteSpace(dett.PeriodoInizio) || String.IsNullOrWhiteSpace(dett.PeriodoFine)
                    || String.IsNullOrWhiteSpace(dett.CF) || String.IsNullOrWhiteSpace(dett.INPS))
                {
                    foreach (var r in RichiesteMatricola)
                    {
                        DateTime D1 = r.INIZIO_GIUSTIFICATIVO ?? r.DATA_INIZIO_MATERNITA.Value;
                        DateTime D2 = r.FINE_GIUSTIFICATIVO ?? r.DATA_FINE_MATERNITA.Value;
                        if (D1 != default(DateTime) && D2 != default(DateTime))
                        {
                            savedb = true;
                            if (item.DATA >= D1 && item.DATA <= D2)
                            {
                                if (String.IsNullOrWhiteSpace(dett.PeriodoInizio))
                                {
                                    item.PERIODO_RIFERIMENTO_DA = D1;
                                    dett.PeriodoInizio = D1.ToString("dd/MM/yyyy");
                                }
                                if (String.IsNullOrWhiteSpace(dett.PeriodoFine))
                                {
                                    item.PERIODO_RIFERIMENTO_A = D2;
                                    dett.PeriodoFine = D2.ToString("dd/MM/yyyy");
                                }

                                if (String.IsNullOrWhiteSpace(dett.CF))
                                {
                                    item.CODICE_FISCALE_FIGLIO = r.CF_BAMBINO;
                                    dett.CF = r.CF_BAMBINO;
                                }
                                if (String.IsNullOrWhiteSpace(dett.INPS))
                                {
                                    item.PROTOCOLLO_INPS = r.PROTOCOLLO_INPS;
                                    dett.INPS = r.PROTOCOLLO_INPS;
                                }
                            }
                        }

                    }
                }
                db.SaveChanges();
            }
            return View("_dettagliPeriodiGestione", model);
        }
        [HttpPost]
        public ActionResult SavePeriods(SavePeriodModel[] Model)
        {
            var db = new IncentiviEntities();
            foreach (var item in Model)
            {
                bool changed = false;

                var DBitem = db.XR_MAT_ARRETRATI_DIPENDENTE.Where(x => x.ID == item.id).FirstOrDefault();

                if (item.cf != DBitem.CODICE_FISCALE_FIGLIO)
                {
                    changed = true;
                    DBitem.CODICE_FISCALE_FIGLIO = item.cf;
                }
                string inpsDB = null;
                if (DBitem.PROTOCOLLO_INPS != null)
                    inpsDB = DBitem.PROTOCOLLO_INPS.Trim(new char[] { '\r', '\n' });

                if (item.inps != inpsDB)
                {
                    changed = true;
                    DBitem.PROTOCOLLO_INPS = item.inps;
                }

                DateTime D1;
                if (DateTime.TryParseExact(item.p1, "dd/MM/yyyy", null, DateTimeStyles.None, out D1))
                {
                    if (DBitem.PERIODO_RIFERIMENTO_DA != D1)
                    {
                        changed = true;
                        DBitem.PERIODO_RIFERIMENTO_DA = D1;
                    }
                }
                else if (String.IsNullOrWhiteSpace(item.p1) && DBitem.PERIODO_RIFERIMENTO_DA != null)
                {
                    DBitem.PERIODO_RIFERIMENTO_DA = null;
                    changed = true;
                }
                DateTime D2;
                if (DateTime.TryParseExact(item.p2, "dd/MM/yyyy", null, DateTimeStyles.None, out D2))
                {
                    if (DBitem.PERIODO_RIFERIMENTO_A != D2)
                    {
                        changed = true;
                        DBitem.PERIODO_RIFERIMENTO_A = D2;
                    }
                }
                else if (String.IsNullOrWhiteSpace(item.p2) && DBitem.PERIODO_RIFERIMENTO_A != null)
                {
                    DBitem.PERIODO_RIFERIMENTO_A = null;
                    changed = true;
                }
                if (changed)
                {
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { esito = false, errore = "ID " + item.id.ToString() + "-" + ex.Message }
                        };
                    }
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }
        public List<myRaiCommonModel.Gestionale.HRDW.HRDWdettagliSTR> GetDettagliStraordinario(string matricola, int mese, int anno)
        {
            var db = new IncentiviEntities();
            string query = myRaiHelper.CommonHelper.GetParametro<string>(EnumParametriSistema.QueryDettaglioStrHRDW)
                .Replace("#MATR", matricola).Replace("#ANNO", anno.ToString()).Replace("#MESE", mese.ToString());
            List<myRaiCommonModel.Gestionale.HRDW.HRDWdettagliSTR> DettagliSTR =
                    db.Database.SqlQuery<myRaiCommonModel.Gestionale.HRDW.HRDWdettagliSTR>(query).ToList();
            return DettagliSTR;

        }
        public decimal GetImportoStraordinario(DettaglioCedolinoModel Model, int? mese = null, int? anno = null)
        {
            DateTime D1 = Model.Richiesta.INIZIO_GIUSTIFICATIVO ?? Model.Richiesta.DATA_INIZIO_MATERNITA.Value;

            DateTime DataPrimoDelMeseInizioCongedo = new DateTime(
                   D1.Year,
                   D1.Month,
                   1);
            DateTime DataPrimoDelMesePrecedente = DataPrimoDelMeseInizioCongedo.AddMonths(-1);
            DateTime DataPerSTRdaHRDW = DataPrimoDelMeseInizioCongedo;

            if (mese != null && mese != DataPrimoDelMeseInizioCongedo.Month)
            {
                DataPrimoDelMeseInizioCongedo = new DateTime((int)anno, (int)mese, 1);
                DataPrimoDelMesePrecedente = DataPrimoDelMeseInizioCongedo.AddMonths(-1);
                DataPerSTRdaHRDW = new DateTime((int)anno, (int)mese, 1).AddMonths(1);
            }
            var db = new IncentiviEntities();
            decimal importo = 0;
            string codicistr = CommonHelper.GetParametro<string>(EnumParametriSistema.CodiciStraordinario);
            if (!String.IsNullOrWhiteSpace(codicistr))
            {
                string q = GetQueryVariabili(Model.Richiesta.MATRICOLA, DataPerSTRdaHRDW.Year, DataPerSTRdaHRDW.Month);
                IEnumerable<myRaiCommonModel.Gestionale.HRDW.HRDWVariabili> varsPrimaQuery =
                    db.Database.SqlQuery<myRaiCommonModel.Gestionale.HRDW.HRDWVariabili>(q).ToList();

                varsPrimaQuery = varsPrimaQuery.Where(x => x.Anno == DataPerSTRdaHRDW.Year &&
                x.Mese == DataPerSTRdaHRDW.Month).ToList();

                importo = varsPrimaQuery.Where(x => codicistr.Split(',').Contains(x.cod_voce_cedolino)).ToList()
                                        .Sum(x => x.Importo);
                if (importo > 0)
                {
                    Model.DettagliSTR = GetDettagliStraordinario(Model.Richiesta.MATRICOLA, DataPerSTRdaHRDW.Month, DataPerSTRdaHRDW.Year);
                }
                DateTime Dcomp;
                if (mese == null)
                {
                    Dcomp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }
                else
                {
                    Dcomp = new DateTime((int)anno, (int)mese, 1).AddMonths(1);
                }
                DateTime Dp = DataPerSTRdaHRDW.AddMonths(-1);

                //modifica 11/2/2022////////////////////////////////////
                var importoSoloTabella = db.XR_VAR_STRAORDINARI.Where(x => x.MATRICOLA == Model.Richiesta.MATRICOLA &&
                            x.DTA_RIFERIMENTO == Dp).ToList().Sum(z => z.IMP_IMPORTO);
                Model.DettagliSTR = new List<myRaiCommonModel.Gestionale.HRDW.HRDWdettagliSTR>();

                return importoSoloTabella;
                ///////////////////////////////////////////////////////

                var importoUlteriore = db.XR_VAR_STRAORDINARI.Where(x => x.MATRICOLA == Model.Richiesta.MATRICOLA &&
                             x.DTA_COMPETENZA == Dcomp &&
                             x.DTA_RIFERIMENTO == Dp).ToList().Sum(z => z.IMP_IMPORTO);
                if (importoUlteriore > 0)
                {
                    Model.DettagliSTR.Add(new myRaiCommonModel.Gestionale.HRDW.HRDWdettagliSTR()
                    {
                        Importo = importoUlteriore,
                        Anno = Dcomp.Year,
                        Mese_Competenza = Dcomp.Month,
                        desc_aggregato_costi = "Provvisorio",
                        desc_voce_cedolino = "Arretrati",
                        IV_Cedolino = 0,
                        Ore = 0,
                        FromArretratiSuTabella = true
                    });

                    importo += importoUlteriore;
                }


            }
            if (importo == 0)
            {
                DateTime Dcomp;
                DateTime Drif;
                if (mese == null)
                {
                    //mese = Model.Richiesta.INIZIO_GIUSTIFICATIVO.Value.AddMonths(-1).Month;
                    //anno = Model.Richiesta.INIZIO_GIUSTIFICATIVO.Value.AddMonths(-1).Year;
                    mese = D1.Month;
                    anno = D1.Year;
                    Dcomp = new DateTime((int)anno, (int)mese, 1);
                }
                else
                {
                    Dcomp = new DateTime((int)anno, (int)mese, 1).AddMonths(1);
                }
                Drif = Dcomp.AddMonths(-1);

                importo = db.XR_VAR_STRAORDINARI.Where(x => x.MATRICOLA == Model.Richiesta.MATRICOLA &&
                            //x.DTA_COMPETENZA == Dcomp &&
                            x.DTA_RIFERIMENTO == Drif).ToList().Sum(x => x.IMP_IMPORTO);



                //x.DTA_COMPETENZA == DataPrimoDelMeseInizioCongedo &&
                //            x.DTA_RIFERIMENTO == DataPrimoDelMesePrecedente).ToList().Sum(x => x.IMP_IMPORTO);
            }
            return importo;
        }
        public ActionResult GetDifferenzeDescrittive()
        {

            int anno = DateTime.Now.Year;
            int mese = DateTime.Now.Month;
            List<XR_VAR_DESCRITTIVE> lista = MaternitaCongediManager.GetDifferenze(anno, mese);
            List<string> Rows = new List<string>();

            string ElencoRigheCSV = "";
            if (lista.Any())
            {
                foreach (var item in lista)
                {
                    ElencoRigheCSV += item.RigaCSV + "\r\n";
                }
            }

            MemoryStream stringInMemoryStream = new MemoryStream(System.Text.ASCIIEncoding.Default.GetBytes(ElencoRigheCSV));

            stringInMemoryStream.Position = 0;
            return new FileStreamResult(stringInMemoryStream, "application/txt") { FileDownloadName = "Descrittive.csv" };

        }
        public ActionResult GetPeriodiConiuge(int idrichiesta)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_MAT_RICHIESTE ric = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            var model = new List<PeriodoArretratoDipendente>();

            var listDB = db.XR_MAT_ARRETRATI_CONIUGE.Where(x => x.MATRICOLA_DIPENDENTE == ric.MATRICOLA &&
                       x.CODICE_FISCALE_FIGLIO == ric.CF_BAMBINO && x.CANCELLATO == false).ToList();

            foreach (var item in listDB)
            {
                var a = new PeriodoArretratoDipendente();
                if (item.PERIODO_DA == null)
                    a.D1 = default(DateTime);
                else
                    a.D1 = item.PERIODO_DA.Value;

                if (item.PERIODO_A == null)
                    a.D2 = default(DateTime);
                else
                    a.D2 = item.PERIODO_A.Value;

                a.Quantita = (float)item.QUANTITA;
                model.Add(a);
            }
            //model.Clear();
            return View("_periodiConiuge", model);
        }


        public ActionResult GetPeriodiDipendente(int idrichiesta)
        {

            var list = MaternitaCongediManager.GetListPeriodiDipendente(idrichiesta);
            return View("_periodiDipendente", list);

        }
        public ActionResult GetNota(int idnota)
        {
            var db = new IncentiviEntities();
            var nota = db.XR_MAT_NOTE.Where(x => x.ID == idnota).FirstOrDefault();
            myRaiCommonModel.SingolaNotaModel model = new SingolaNotaModel();
            model.nota = nota;
            return View("_notamod", model);
        }
        [HttpPost]
        public ActionResult CancellaNota(int idnota)
        {
            var db = new IncentiviEntities();
            try
            {
                var nota = db.XR_MAT_NOTE.Where(x => x.ID == idnota).FirstOrDefault();
                db.XR_MAT_NOTE.Remove(nota);
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }
        }

        [HttpPost]
        public ActionResult salvanota(int idrichiesta, string testo, string visibilita, HttpPostedFileBase file, int? idnota, string nomefile)
        {
            string esito = MaternitaCongediManager.SalvaNota(idrichiesta, testo, visibilita, file, idnota, nomefile);

            if (esito == null)
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            else
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = esito }
                };
        }
        private float CalcolaValoreEccezione(string eccezione, DettaglioCedolinoModel model)
        {
            float Risultato = 0;
            float giorni = model.QuantitaEccezioni.Where(x => x.eccezione == eccezione).Select(x => x.totali).FirstOrDefault();

            if (giorni == 0) return 0;
            if (eccezione == "DG55")
            {
                Risultato =
                      (((model.VociGeneriche.stipendio +
                      model.VociGeneriche.ind_contingenza +
                      model.VociGeneriche.ind_mensa +
                      model.VociGeneriche.ind_110 +
                      model.VociGeneriche.ind_120 +
                      model.VociGeneriche.ind_123 +
                      model.VociGeneriche.ind_129 +
                      model.VociGeneriche.ind_130 +
                      model.VociGeneriche.ind_114 +
                      model.VociGeneriche.ind_119)
                      / 26f)
                      * 0.55f)
                      * giorni;
            }
            if (eccezione == "LN16")
            {
                Risultato =
                      (((
                      model.VociGeneriche.ind_contingenza +
                      model.VociGeneriche.minimo +
                      model.VociGeneriche.superminimo
                      )
                      / 26f)
                      * 0.16f)
                      * giorni;
            }
            if (eccezione == "LN20")
            {
                Risultato =
                      (((
                      model.VociGeneriche.ind_contingenza +
                      model.VociGeneriche.minimo +
                      model.VociGeneriche.superminimo
                      )
                      / 26f)
                      * 0.2f)
                      * giorni;
            }
            if (eccezione == "LN25")
            {
                Risultato =
                      (((
                      model.VociGeneriche.ind_contingenza +
                      model.VociGeneriche.minimo +
                      model.VociGeneriche.superminimo
                      )
                      / 26f)
                      * 0.25f)
                      * giorni;
            }
            if (eccezione == "LN50")
            {
                Risultato =
                      (((
                      model.VociGeneriche.ind_contingenza +
                      model.VociGeneriche.minimo +
                      model.VociGeneriche.superminimo
                      )
                      / 26f)
                      * 0.50f)
                      * giorni;
            }
            if (eccezione == "PDCO")
            {
                Risultato =
                      (((
                      model.VociGeneriche.minimo +
                      model.VociGeneriche.superminimo
                      )
                      / 26f)
                      * 0.25f)
                      * giorni;
            }

            if (eccezione == "LF80")
            {
                Risultato =
                      (((model.VociGeneriche.stipendio +
                      model.VociGeneriche.ind_contingenza +
                      model.VociGeneriche.ind_mensa +
                      model.VociGeneriche.ind_110 +
                      model.VociGeneriche.ind_120 +
                      model.VociGeneriche.ind_123 +
                      model.VociGeneriche.ind_129 +
                      model.VociGeneriche.ind_130 +
                      model.VociGeneriche.ind_114 +
                      model.VociGeneriche.ind_12B)
                      / 26f)
                      * 1.8f)
                      * giorni;
            }
            if (eccezione == "LF36")
            {
                Risultato =
                      (((model.VociGeneriche.stipendio +
                      model.VociGeneriche.ind_contingenza +
                      model.VociGeneriche.ind_mensa +
                      model.VociGeneriche.ind_110 +
                      model.VociGeneriche.ind_120 +
                      model.VociGeneriche.ind_123 +
                      model.VociGeneriche.ind_129 +
                      model.VociGeneriche.ind_130 +
                      model.VociGeneriche.ind_114 +
                      model.VociGeneriche.ind_12B)
                      / 26f)
                      * 3.6f)
                      * giorni;
            }
            if (eccezione == "LEXF")
            {
                Risultato =
                      ((model.VociGeneriche.stipendio +
                      model.VociGeneriche.ind_contingenza +
                      model.VociGeneriche.ind_mensa +
                      model.VociGeneriche.ind_110 +
                      model.VociGeneriche.ind_120 +
                      model.VociGeneriche.ind_123 +
                      model.VociGeneriche.ind_129 +
                      model.VociGeneriche.ind_130 +
                      model.VociGeneriche.ind_114 +
                      model.VociGeneriche.ind_119)
                      / 26f)
                      * giorni;
            }

            if (eccezione == "AR20")
            {
                Risultato =
                      ((model.VociGeneriche.stipendio +
                      model.VociGeneriche.ind_contingenza +
                      model.VociGeneriche.ind_mensa +
                      model.VociGeneriche.ind_110 +
                      model.VociGeneriche.ind_120 +
                      model.VociGeneriche.ind_123 +
                      model.VociGeneriche.ind_129 +
                      model.VociGeneriche.ind_130 +
                      model.VociGeneriche.ind_114 +
                      model.VociGeneriche.ind_119 +
                      model.VociGeneriche.ind_12B +
                      model.VociGeneriche.ind_13B
                      )
                      / 26f)
                      * giorni;
            }
            if (eccezione == "RPAF")
            {
                string s = CommonHelper.GetParametro<string>(EnumParametriSistema.ImportoRPAF);
                Risultato = giorni * float.Parse(s.Replace(".", ","));
            }


            return Risultato;
        }


        public ActionResult GetDettaglioCedolinoCambioMese(int idrichiesta, bool modifica, int mese, int anno)
        {
            var db = new IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();

            var stato = rich.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();
            modifica = stato.COD_USER == CommonHelper.GetCurrentUserMatricola() && stato.ID_STATO ==
                (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin;

            //myRaiCommonModel.DettaglioCedolinoModel model = GetCedolinoModel(idrichiesta, modifica, mese, anno);
            myRaiCommonModel.DettaglioCedolinoModel model = GetCedolinoModel(idrichiesta, modifica, mese, anno);



            return View("PopupVisAmmCedolinoContent", model);
        }

        public ActionResult GetDettaglioCedolinoAmm(int idrichiesta, bool modifica, int mese, int anno)
        {
            var db = new IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();

            var stato = rich.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();
            modifica = stato.COD_USER == CommonHelper.GetCurrentUserMatricola() && stato.ID_STATO ==
                (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin;

            //myRaiCommonModel.DettaglioCedolinoModel model = GetCedolinoModel(idrichiesta, modifica, mese, anno);
            myRaiCommonModel.DettaglioCedolinoModel model = GetCedolinoModel(idrichiesta, modifica);

            if (model.FormaContratto == "K")// tipo K
            {

                DateTime Dfinale = new DateTime(Convert.ToInt32(model.AnnoPerCalcolo), Convert.ToInt32(model.MesePerCalcolo), 1);
                DateTime Diniziale = Dfinale.AddMonths(-11);
                List<string> MesiAnno = new List<string>();
                Decimal Totale175 = 0;
                Decimal Totale178 = 0;
                Decimal Totale179 = 0;
                for (DateTime Dcurrent = Diniziale; Dcurrent <= Dfinale; Dcurrent = Dcurrent.AddMonths(1))
                {
                    MesiAnno.Add(" '" + Dcurrent.Year + "/" + Dcurrent.Month.ToString().PadLeft(2, '0') + "' ");

                    //string q = GetQueryVariabiliTE1(model.Richiesta.MATRICOLA, Dcurrent.Year, Dcurrent.Month);

                    //IEnumerable<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico1> varsTE1 =
                    //        db.Database.SqlQuery<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico1>
                    //        (q).ToList();
                    //var row175 = varsTE1.Select(x => x.xiii_mensilita).FirstOrDefault(); ;
                    //var row178 = varsTE1.Select(x => x.xiv_mensilita).FirstOrDefault(); ;
                    //var row179 = varsTE1.Select(x => x.premio_produzione).FirstOrDefault(); ;
                    //if (row175 != null)
                    //{
                    //    Totale175 += (decimal)row175;
                    //}
                    //if (row178 != null)
                    //{
                    //    Totale178 += (decimal)row178;
                    //}
                    //if (row179 != null)
                    //{
                    //    Totale179 += (decimal)row179;
                    //}
                    string queryIndennita = GetQueryVariabiliTE2(model.Richiesta.MATRICOLA, Dcurrent.Year, Dcurrent.Month);
                    IEnumerable<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico2> varsTE2 =
                          db.Database.SqlQuery<myRaiCommonModel.Gestionale.MaternitaCongedi.TrattamentoEconomico2>
                          (queryIndennita).ToList();


                    var row175 = varsTE2.Where(x => x.cod_indennita == "175").FirstOrDefault();
                    if (row175 != null && row175.importo_inden != null)
                    {
                        Totale175 += (decimal)row175.importo_inden;
                    }
                    var row178 = varsTE2.Where(x => x.cod_indennita == "178").FirstOrDefault();
                    if (row178 != null && row178.importo_inden != null)
                    {
                        Totale178 += (decimal)row178.importo_inden;
                    }
                    var row179 = varsTE2.Where(x => x.cod_indennita == "179").FirstOrDefault();
                    if (row179 != null && row179.importo_inden != null)
                    {
                        Totale179 += (decimal)row179.importo_inden;
                    }
                }
                string MesiAnnoConcat = String.Join(",", MesiAnno);
                Decimal TotaleLordo = MaternitaCongediManager.GetImportoLordoTotale(model.Richiesta.MATRICOLA, MesiAnnoConcat);
                TotaleLordo = TotaleLordo - Totale175 - Totale178 - Totale179;

                model.LordoMedioMensile = Math.Round(TotaleLordo / 12, 2);
                model.Medio175Mensile = Math.Round(Totale175 / 12, 2);
                model.Medio178Mensile = Math.Round(Totale178 / 12, 2);
                model.Medio179Mensile = Math.Round(Totale179 / 12, 2);

                model.ListaItemCedolino = new List<DettaglioCedolinoItemModel>();
                model.ListaItemCedolino.Add(new DettaglioCedolinoItemModel()
                {
                    CellaCalcoloDaExcel = "A9",
                    CalcolatoModel = null,
                    Etichetta = "Lordo medio mensile",
                    NameAttuale = "Lordo_medio_mensile",
                    NameHRDW = "Lordo_medio_mensile_old",
                    TipiDipendenteSpettanti = "*",
                    ValoreAttuale = (float)model.LordoMedioMensile,
                    ValoreHRDW = (float)model.LordoMedioMensile
                });
                model.ListaItemCedolino.Add(new DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "*",
                    CalcolatoModel = new DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "13ma mensilità",
                        NameAttuale = "mens_13ma",
                        id_hidden = "13mahid",
                        id_label = "13ma",
                        ValoreAttuale = (float)model.Medio175Mensile,
                        IsFromFormaContrattoK = true

                    }
                });
                model.ListaItemCedolino.Add(new DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "*",
                    CalcolatoModel = new DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "14ma mensilità",
                        NameAttuale = "mens_14ma",
                        id_hidden = "14mahid",
                        id_label = "14ma",
                        ValoreAttuale = (float)model.Medio178Mensile,
                        IsFromFormaContrattoK = true
                    }
                });

                model.ListaItemCedolino.Add(new DettaglioCedolinoItemModel()
                {
                    TipiDipendenteSpettanti = "*",
                    CalcolatoModel = new DettaglioCedolinoItemCalcolatoModel()
                    {

                        Etichetta = "Premio produzione",
                        NameAttuale = "premio_prod",
                        id_hidden = "premiohid",
                        id_label = "premio",
                        ValoreAttuale = (float)model.Medio179Mensile,
                        IsFromFormaContrattoK = true
                    }
                });
            }

            return View("PopupVisAmmCedolinoContent", model);

        }
        public ActionResult GetDettaglioRichiestaAmmBox(int idrichiesta)
        {
            var db = new IncentiviEntities();
            DettagliRichiestaAmmModel model = new DettagliRichiestaAmmModel();
            model.InfoDematerializzazione = DematerializzazioneManager.GetDocumentoByIdRichiesta(idrichiesta);

            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            model.Richiesta = rich;
            var statoInCarico = model.Richiesta.XR_WKF_OPERSTATI.Where(x => x.ID_STATO == 60 && x.NASCOSTO == false).FirstOrDefault();
            if (statoInCarico == null)
                model.InCarico = "-";
            else
            {
                if (!String.IsNullOrWhiteSpace(statoInCarico.COD_USER))
                {
                    var nom = CommonHelper.GetNominativoPerMatricola(statoInCarico.COD_USER);
                    model.InCarico = statoInCarico.COD_USER + " - " + nom;
                    model.InCaricoAMe = statoInCarico.COD_USER == CommonHelper.GetCurrentUserMatricola();
                }
            }

            return View("_dettagliRichiestaAmm", model);
        }
        public ActionResult GetDettaglioRichiestaAmmBoxPeriodi(int idrichiesta)
        {
            var db = new IncentiviEntities();
            DettagliRichiestaAmmModel model = new DettagliRichiestaAmmModel();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            model.Richiesta = rich;
            model.IsFromPeriodiPopup = true;
            return View("_dettagliRichiestaAmm", model);
        }


        public ActionResult GetDettaglioCedolinoPagato(myRaiCommonModel.AmministrazioneModel.BustaPaga json)
        {


            return View("_dettagliCedolinoPagato", json);
        }



        public void SovrascriviTask(TaskSavingModel[] model, IncentiviEntities db)
        {
            DateTime Dnow = DateTime.Now;

            foreach (var item in model)
            {
                var taskInCorso = db.XR_MAT_TASK_IN_CORSO.Where(x => x.ID == item.idtaskincorso).FirstOrDefault();
                if (taskInCorso.TERMINATA)
                    continue;

                if (item.attivo == 1)
                {
                    taskInCorso.BLOCCATA_DATETIME = null;
                    taskInCorso.BLOCCATA_DA_OPERATORE = null;
                }
                else
                {
                    taskInCorso.BLOCCATA_DATETIME = Dnow;
                    taskInCorso.BLOCCATA_DA_OPERATORE = CommonHelper.GetCurrentUserMatricola();
                }

                if (taskInCorso.XR_MAT_ELENCO_TASK.TIPO == "TRACCIATO")
                {
                    taskInCorso.INPUT = item.tracciatointero;
                }
            }
        }
        public void ScriviNuoviTask(TaskSavingModel[] model, IncentiviEntities db)
        {
            DateTime Dnow = DateTime.Now;

            foreach (var item in model)
            {
                XR_MAT_ELENCO_TASK task = db.XR_MAT_ELENCO_TASK.Where(x => x.ID == item.idtask).FirstOrDefault();
                DateTime? D = null;
                if (item.attivo == 0) D = Dnow;
                string m = null;
                if (item.attivo == 0) m = CommonHelper.GetCurrentUserMatricola();

                if (task.TIPO == "TRACCIATO-TE")
                {
                    DateTime DEseguibileDa = new DateTime(item.anno, item.mese, 1);
                    XR_MAT_TASK_IN_CORSO newTask = new XR_MAT_TASK_IN_CORSO()
                    {
                        BLOCCATA_DATETIME = D,
                        BLOCCATA_DA_OPERATORE = m,
                        DATA_CREAZIONE = Dnow,
                        ESEGUIBILE_DA_DATA = DEseguibileDa,
                        ESEGUIBILE_FINO_A_DATA = new DateTime(9999, 12, 31),
                        ID_RICHIESTA = item.idrichiesta,
                        ID_TASK = item.idtask,
                        MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                        PROGRESSIVO = item.progressivotask,
                        MESE = item.mese,
                        ANNO = item.anno,
                        INPUT = item.tracciatointero
                    };
                    newTask.SISTEMA_OUTPUT = "DEW";
                    if (String.IsNullOrWhiteSpace(item.tracciatointero))
                    {
                        newTask.ERRORE_BATCH = "Non è stato possibile compilare il tracciato";
                        newTask.DATA_ULTIMO_TENTATIVO = DateTime.Now;
                    }

                    db.XR_MAT_TASK_IN_CORSO.Add(newTask);
                }
                else if (task.TIPO == "TRACCIATO")
                {
                    //DateTime DEseguibileDa = new DateTime(item.anno, item.mese, 1);
                    string IDscadenzario;
                    if (task.NOME_TASK.Contains("DESCRITTIVA"))
                        IDscadenzario = CommonHelper.GetParametro<string>(EnumParametriSistema.IDscadenzarioInvioDescrittive);
                    else
                        IDscadenzario = CommonHelper.GetParametro<string>(EnumParametriSistema.IDscadenzarioInvioTracciatiDEW);

                    DateTime? DEseguibileDa = MaternitaCongediManager.GetScadenzarioPerIdUfficio(IDscadenzario, item.anno, item.mese);

                    XR_MAT_TASK_IN_CORSO newTask = new XR_MAT_TASK_IN_CORSO()
                    {
                        BLOCCATA_DATETIME = D,
                        BLOCCATA_DA_OPERATORE = m,
                        DATA_CREAZIONE = Dnow,
                        ESEGUIBILE_FINO_A_DATA = new DateTime(9999, 12, 31),
                        ID_RICHIESTA = item.idrichiesta,
                        ID_TASK = item.idtask,
                        MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                        PROGRESSIVO = item.progressivotask,
                        MESE = item.mese,
                        ANNO = item.anno,
                        INPUT = item.tracciatointero
                    };
                    if (DEseguibileDa != null)
                    {
                        newTask.ESEGUIBILE_DA_DATA = DEseguibileDa.Value;
                    }
                    else
                    {
                        newTask.ESEGUIBILE_DA_DATA = new DateTime(9999, 12, 31);
                        newTask.ERRORE_BATCH = "Non è stato possibile recuperare la data dallo scadenzario DEW";
                        newTask.DATA_ULTIMO_TENTATIVO = DateTime.Now;
                    }
                    if (task.NOME_TASK.Contains("DESCRITTIVA"))
                    {
                        newTask.ESEGUIBILE_FINO_A_DATA = new DateTime(newTask.ESEGUIBILE_DA_DATA.Year, newTask.ESEGUIBILE_DA_DATA.Month,
                            DateTime.DaysInMonth(newTask.ESEGUIBILE_DA_DATA.Year, newTask.ESEGUIBILE_DA_DATA.Month));
                    }

                    if (!String.IsNullOrWhiteSpace(item.eccezionerisultante))
                    {
                        if (task.NOME_TASK.Trim() != "STORNO CEDOLINO"
                            && !task.NOME_TASK.Trim().Contains("DESCRITTIVA"))
                            newTask.NOTE = item.eccezionerisultante + "-" + item.periododa + "-" + item.periodoa;
                        if (item.eccezionerisultante == "MT" && item.tracciatointero.Contains("MU"))
                        {
                            newTask.NOTE = "MU-" + item.periododa + "-" + item.periodoa;
                        }

                    }

                    newTask.SISTEMA_OUTPUT = MaternitaCongediManager.GetRewDew(item.anno, item.mese);
                    if (String.IsNullOrWhiteSpace(item.tracciatointero))
                    {
                        newTask.ERRORE_BATCH = "Non è stato possibile compilare il tracciato";
                        newTask.DATA_ULTIMO_TENTATIVO = DateTime.Now;
                    }

                    db.XR_MAT_TASK_IN_CORSO.Add(newTask);
                }
                if (task.TIPO == "SERVIZIO")
                {
                    if (task.NOME_TASK == "MODIFICA LISTONE STORNI")
                    {
                        DateTime? DataScadenzario = MaternitaCongediManager.GetScadenzarioPerIdUfficio("16", item.anno, item.mese);
                        if (DataScadenzario == null)
                        {
                            Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                            {
                                applicativo = "HRIS",
                                data = DateTime.Now,
                                matricola = CommonHelper.GetCurrentUserMatricola(),
                                provenienza = "ScriviNuoviTask",
                                error_message = "Impossibile ricavare data per esecuzione task MODIFICA LISTONE STORNI"
                            });
                            throw new Exception("Impossibile ricavare data per esecuzione task MODIFICA LISTONE STORNI");
                        }
                        else
                        {
                            XR_MAT_TASK_IN_CORSO newTask = new XR_MAT_TASK_IN_CORSO()
                            {
                                BLOCCATA_DATETIME = D,
                                BLOCCATA_DA_OPERATORE = m,
                                DATA_CREAZIONE = Dnow,
                                ESEGUIBILE_DA_DATA = DataScadenzario.Value.AddDays(1),
                                ESEGUIBILE_FINO_A_DATA = new DateTime(9999, 12, 31),
                                ID_RICHIESTA = item.idrichiesta,
                                ID_TASK = item.idtask,
                                MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                                PROGRESSIVO = item.progressivotask,
                                MESE = item.mese,
                                ANNO = item.anno,
                                INPUT = item.eccezionerisultante
                            };
                            db.XR_MAT_TASK_IN_CORSO.Add(newTask);
                        }

                    }
                    if (task.NOME_TASK == "AGGIORNAMENTO PAGAMENTO ECCEZIONI")
                    {
                        DateTime Dstart = new DateTime(item.anno, item.mese, 28);

                        XR_MAT_TASK_IN_CORSO newTask = new XR_MAT_TASK_IN_CORSO()
                        {
                            BLOCCATA_DATETIME = D,
                            BLOCCATA_DA_OPERATORE = m,
                            DATA_CREAZIONE = Dnow,
                            ESEGUIBILE_DA_DATA = Dstart,
                            ESEGUIBILE_FINO_A_DATA = new DateTime(9999, 12, 31),
                            ID_RICHIESTA = item.idrichiesta,
                            ID_TASK = item.idtask,
                            MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                            PROGRESSIVO = item.progressivotask,
                            MESE = item.mese,
                            ANNO = item.anno,
                            INPUT = item.eccezionerisultante
                        };
                        db.XR_MAT_TASK_IN_CORSO.Add(newTask);
                    }
                }
            }
        }
        [HttpPost]
        public ActionResult SalvaTask(myRaiCommonModel.TaskSavingModel[] model)
        {
            var db = new IncentiviEntities();

            int idxDb = model.First().idrichiesta;
            db.XR_MAT_TASK_IN_CORSO.RemoveWhere(g => g.TERMINATA == false && g.ID_RICHIESTA == idxDb);
            var Rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idxDb).FirstOrDefault();
            if (Rich != null) Rich.DA_RIAVVIARE = false;

            Rich.DATA_AVVIATA = DateTime.Now;

            db.SaveChanges();


            var TaskDaSovrascrivere = model.Where(x => x.idaltrapratica == 0 && x.idtaskincorso != 0).ToArray();
            var TaskNuovi = model.Where(x => x.idaltrapratica == 0 && x.idtaskincorso == 0).ToArray();
            if (TaskDaSovrascrivere.Any())
            {
                SovrascriviTask(TaskDaSovrascrivere, db);
            }
            if (TaskNuovi.Any())
            {
                ScriviNuoviTask(TaskNuovi, db);
            }
            try
            {
                if (TaskDaSovrascrivere.Any() || TaskNuovi.Any())
                {
                    if (MaternitaCongediManager.ProvieneDaDEM(Rich))
                    {
                        var es = DematerializzazioneController.PrendiInCaricoPratica(ref db, Rich.ID, CommonHelper.GetCurrentUserMatricola());
                        if (es != null && es.Esito == false)
                        {
                            return new JsonResult
                            {
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                                Data = new { esito = false, errore = es.DescrizioneErrore }
                            };
                        }
                    }

                    db.SaveChanges();
                }
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }

            DateTime Dnow = DateTime.Now;

            XR_MAT_RICHIESTE rich = null;
            foreach (var item in model)
            {
                XR_MAT_ELENCO_TASK task = db.XR_MAT_ELENCO_TASK.Where(x => x.ID == item.idtask).FirstOrDefault();
                if (task == null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = "Task non trovato" }
                    };
                }
                rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == item.idrichiesta).FirstOrDefault();
                if (rich == null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = "Richiesta non trovata" }
                    };
                }
                DateTime? D = null;
                if (item.attivo == 0) D = Dnow;
                string m = null;
                if (item.attivo == 0) m = CommonHelper.GetCurrentUserMatricola();

                XR_MAT_TASK_IN_CORSO taskGiaSalvato = null;

                if (task.NOME_TASK.Trim() == "STORNO CEDOLINO")
                {
                    taskGiaSalvato = db.XR_MAT_TASK_IN_CORSO
                   .Where(x => x.ID_TASK == item.idtask && x.ID_RICHIESTA == item.idrichiesta
                   && x.MESE == item.mese && x.ANNO == item.anno).FirstOrDefault();
                }
                else if (task.TIPO == "TRACCIATO")
                {
                    taskGiaSalvato = db.XR_MAT_TASK_IN_CORSO
                                           .Where(x => x.ID_TASK == item.idtask && x.ID_RICHIESTA == item.idrichiesta
                                           && x.MESE == item.mese && x.ANNO == item.anno
                                           && x.NOTE != null
                                           && x.NOTE.Contains(item.periododa) && x.NOTE.Contains(item.periodoa)).FirstOrDefault();
                }
                else if (task.TIPO == "SERVIZIO")
                {
                    taskGiaSalvato = db.XR_MAT_TASK_IN_CORSO
                .Where(x => x.ID_TASK == item.idtask && x.ID_RICHIESTA == item.idrichiesta
                && x.MESE == item.mese && x.ANNO == item.anno).FirstOrDefault();
                }



                if (taskGiaSalvato != null)
                {
                    if (taskGiaSalvato.TERMINATA)
                        continue;
                    if (task.TIPO == "TRACCIATO")
                        taskGiaSalvato.INPUT = item.tracciatointero;
                    else
                        taskGiaSalvato.INPUT = MaternitaCongediManager.GetEccezioneRisultante(rich);

                    if (item.attivo == 1 && taskGiaSalvato.BLOCCATA_DATETIME != null)
                    {
                        taskGiaSalvato.BLOCCATA_DATETIME = null;
                        taskGiaSalvato.BLOCCATA_DA_OPERATORE = null;
                    }
                    else if (item.attivo == 0 && taskGiaSalvato.BLOCCATA_DATETIME == null)
                    {
                        taskGiaSalvato.BLOCCATA_DATETIME = Dnow;
                        taskGiaSalvato.BLOCCATA_DA_OPERATORE = CommonHelper.GetCurrentUserMatricola(); ;
                    }
                }
                else
                {
                    if (task.TIPO == "TRACCIATO")
                    {
                        DateTime DEseguibileDa = new DateTime(item.anno, item.mese, 1);
                        XR_MAT_TASK_IN_CORSO newTask = new XR_MAT_TASK_IN_CORSO()
                        {
                            BLOCCATA_DATETIME = D,
                            BLOCCATA_DA_OPERATORE = m,
                            DATA_CREAZIONE = Dnow,
                            ESEGUIBILE_DA_DATA = DEseguibileDa,
                            ESEGUIBILE_FINO_A_DATA = new DateTime(9999, 12, 31),
                            ID_RICHIESTA = item.idrichiesta,
                            ID_TASK = item.idtask,
                            MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                            PROGRESSIVO = item.progressivotask,
                            MESE = item.mese,
                            ANNO = item.anno,
                            INPUT = item.tracciatointero
                        };

                        if (!String.IsNullOrWhiteSpace(item.eccezionerisultante))
                        {
                            newTask.NOTE = item.eccezionerisultante + "-" + item.periododa + "-" + item.periodoa;
                        }

                        newTask.SISTEMA_OUTPUT = MaternitaCongediManager.GetRewDew(item.anno, item.mese);
                        if (String.IsNullOrWhiteSpace(item.tracciatointero))
                        {
                            newTask.ERRORE_BATCH = "Non è stato possibile compilare il tracciato";
                            newTask.DATA_ULTIMO_TENTATIVO = DateTime.Now;
                        }

                        db.XR_MAT_TASK_IN_CORSO.Add(newTask);
                    }
                    if (task.TIPO == "SERVIZIO")
                    {
                        if (task.NOME_TASK == "AGGIORNAMENTO PAGAMENTO ECCEZIONI")
                        {
                            DateTime Dstart = new DateTime(item.anno, item.mese, 28);

                            XR_MAT_TASK_IN_CORSO newTask = new XR_MAT_TASK_IN_CORSO()
                            {
                                BLOCCATA_DATETIME = D,
                                BLOCCATA_DA_OPERATORE = m,
                                DATA_CREAZIONE = Dnow,
                                ESEGUIBILE_DA_DATA = Dstart,
                                ESEGUIBILE_FINO_A_DATA = new DateTime(9999, 12, 31),
                                ID_RICHIESTA = item.idrichiesta,
                                ID_TASK = item.idtask,
                                MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                                PROGRESSIVO = item.progressivotask,
                                MESE = item.mese,
                                ANNO = item.anno,
                                INPUT = item.eccezionerisultante
                            };
                            db.XR_MAT_TASK_IN_CORSO.Add(newTask);
                        }


                    }


                    //var t = db.XR_MAT_ELENCO_TASK.Where(x => x.ID == item.idtask).FirstOrDefault();

                    //DateTime? DinizioPratica = rich.INIZIO_GIUSTIFICATIVO;
                    //if (DinizioPratica == null) DinizioPratica = rich.DATA_INIZIO_MATERNITA;


                    //string testoTracciato = MaternitaCongediManager.GetCampiTracciato(
                    //    (int)t.ID_TRACCIATO_DEW, (int)t.PROGRESSIVO_TRACCIATO_DEW, rich, item.eccezionerisultante,
                    //    item.periododa, item.periodoa, item.giorni26mi,
                    //    DinizioPratica.Value, item.importofinale
                    //    );
                    //newTask.INPUT = testoTracciato;

                }
            }

            //if ( ! rich.XR_WKF_MATCON_OPERSTATI
            //    .Any(x => x.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.ApprovataAmmin))
            //{
            //    XR_WKF_OPERSTATI stato = new XR_WKF_OPERSTATI();
            //    stato.ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault();
            //    stato.COD_USER = CommonHelper.GetCurrentUserMatricola();
            //    stato.ID_STATO = (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.ApprovataAmmin;
            //    stato.TMS_TIMESTAMP = Dnow;
            //    stato.VALID_DTA_INI = Dnow;
            //    stato.COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT;
            //    stato.COD_TERMID = Request.UserHostAddress;
            //    stato.NOMINATIVO = UtenteHelper.Nominativo();
            //    stato.XR_MAT_RICHIESTE = rich;

            //    db.XR_WKF_OPERSTATI.Add(stato);
            //}


            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }
        [HttpPost]
        public ActionResult SalvaEccezioniCongedi(myRaiCommonModel.EccezioneCongedi[] model)
        {
            var db = new IncentiviEntities();
            if (model != null && model.Any())
            {
                int idrichiesta = model.Select(x => x.idrichiesta).FirstOrDefault();

                try
                {
                    DateTime Din;
                    DateTime.TryParseExact(model[0].meserif, "dd/MM/yyyy", null, DateTimeStyles.None, out Din);
                    DateTime Dfine = Din.AddMonths(1).AddDays(-1);

                    var list = db.XR_MAT_ECCEZIONI.Where(x => x.ID_RICHIESTA == idrichiesta
                    && x.DATA_INIZIO >= Din && (x.DATA_FINE <= Dfine | x.DATA_FINE == null))
                    .Select(x => x.ID).ToList();

                    foreach (var id in list)
                    {
                        var e = db.XR_MAT_ECCEZIONI.Where(x => x.ID == id).FirstOrDefault();
                        db.XR_MAT_ECCEZIONI.Remove(e);
                    }


                    foreach (var item in model)
                    {
                        XR_MAT_ECCEZIONI eccdb = new XR_MAT_ECCEZIONI()
                        {
                            DATA_INSERIMENTO = DateTime.Now,
                            ECCEZIONE = item.eccezione,
                            ID_RICHIESTA = idrichiesta,
                            MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                            QUANTITA = (decimal)item.giorni,
                            QUANTITA_ORIGINALE = (decimal)item.giorni_old,
                            STATO_ECCEZIONE = item.statoeccezione

                        };
                        DateTime D1;
                        if (DateTime.TryParseExact(item.datada, "dd/MM/yyyy", null, DateTimeStyles.None, out D1))
                            eccdb.DATA_INIZIO = D1;

                        DateTime D2;
                        if (DateTime.TryParseExact(item.dataa, "dd/MM/yyyy", null, DateTimeStyles.None, out D2))
                            eccdb.DATA_FINE = D2;

                        db.XR_MAT_ECCEZIONI.Add(eccdb);
                    }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = ex.Message }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }
        public ActionResult ModificaCedolino(myRaiCommonModel.CedolinoGenerico model)
        {
            int mese = Convert.ToInt32(model.meseannocedolino.Split('/')[0]);
            int anno = Convert.ToInt32(model.meseannocedolino.Split('/')[1]);
            PropertyInfo[] properties = typeof(myRaiCommonModel.CedolinoGenerico).GetProperties();
            var db = new IncentiviEntities();
            var richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == model.idrichiesta).FirstOrDefault();
            var anag = myRaiHelper.BatchManager.GetUserData(richiesta.MATRICOLA, richiesta.INIZIO_GIUSTIFICATIVO ?? richiesta.DATA_INIZIO_MATERNITA.Value);
            var oldList = db.XR_MAT_VOCI_CEDOLINO.Where(x => x.ID_RICHIESTA == model.idrichiesta).Select(x => x.ID).ToList();
            foreach (int id in oldList)
            {
                var r = db.XR_MAT_VOCI_CEDOLINO.Where(x => x.ID == id).FirstOrDefault();
                db.XR_MAT_VOCI_CEDOLINO.Remove(r);
            }
            db.SaveChanges();

            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "meseannocedolino" || property.Name.EndsWith("old") || property.Name.ToLower().StartsWith("id")) continue;

                if (HttpContext.Request[property.Name] == null) continue;

                XR_MAT_VOCI_CEDOLINO voce = new XR_MAT_VOCI_CEDOLINO();
                voce.ID_RICHIESTA = model.idrichiesta;
                voce.MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola();

                voce.TIPO_DIPENDENTE = anag.tipo_dipendente != null ? anag.tipo_dipendente : " ";

                voce.DATA_INSERIMENTO = DateTime.Now;
                voce.VOCE_CEDOLINO = property.Name;
                float value = (float)model.GetType().GetProperty(property.Name).GetValue(model, null);


                if (model.GetType().GetProperty(property.Name + "_old") != null)
                {
                    float value_old = (float)model.GetType().GetProperty(property.Name + "_old").GetValue(model, null);
                    if (value_old == -1) value_old = 0;
                    voce.VALORE_ORIGINALE = (decimal)value_old;
                }
                voce.VALORE = (decimal)value;
                voce.XR_MAT_RICHIESTE = richiesta;
                voce.MESE_RIFERIMENTO_VALORI = mese;
                voce.ANNO_RIFERIMENTO_VALORI = anno;
                db.XR_MAT_VOCI_CEDOLINO.Add(voce);
            }
            db.SaveChanges();

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }
        public ActionResult ModificaCedolinoG(myRaiCommonModel.CedolinoGiornalisti model)
        {
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }

        public AssegnazioneAmmModel GetAssegnazioneAmmModel(int idrichiesta)
        {
            var db = new IncentiviEntities();
            string MiaMatricola = CommonHelper.GetCurrentUserMatricola();
            AssegnazioneAmmModel model = new AssegnazioneAmmModel();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            model.Richiesta = rich;

            if (rich.XR_MAT_CATEGORIE.CAT == "SW")
            {
                model.PossibileAssegnare = false;
                model.PuoiRilasciare = false;
                model.PuoiPrendereIncarico = false;
                return model;
            }

            var StatoAttuale = model.Richiesta.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();

            if (StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.Inviata ||
                StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione)
            {
                model.FunzioneJSPrendiInCarico = "PrendiInCarico";
                model.FunzioneJSRilascia = "Rilascia";
                model.FunzioneJSAssegna = "AssegnaRichiesta";
                model.SonoADM =
                MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Gestione,
                MaternitaCongediHelper.MaternitaCongediGradiAbil.ADM);

                model.PossibileAssegnare = MaternitaCongediHelper.RichiestaAttualmenteInCaricoAme(rich)
                                           || model.SonoADM;

                string funzioneHrga = "02GEST";
                var giaInCaricoMatricole = rich.XR_WKF_MATCON_OPERSTATI.Where(x =>
                x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers ||
                x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin ||
                x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione).Select(x => x.COD_USER).ToList();

                model.AssegnatariPossibili = myRaiHelper.AuthHelper.GetAllEnabledAs("HRIS", funzioneHrga)
                                            .Where(x => x.Matricola != CommonHelper.GetCurrentUserMatricola()
                                            && !giaInCaricoMatricole.Contains(x.Matricola)
                                            ).ToList();

                model.InCaricoACollegaAssente = false;
                if (StatoAttuale.COD_USER != MiaMatricola &&
                   StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione)
                {
                    MyRaiServiceInterface.it.rai.servizi.digigappws.dayResponse resp =
                        HomeManager.GetEccezioni(DateTime.Now.ToString("ddMMyyyy"), StatoAttuale.COD_USER);

                    if (!resp.eccezioni.Any(x => x.cod.Trim() == "SW") && !resp.timbrature.Any())
                    {
                        model.InCaricoACollegaAssente = true;
                    }

                }
                bool EnabledToWR = MaternitaCongediHelper.EnabledToMaternitaCongediUfficioConDirittiScrittura(MaternitaCongediHelper.MaternitaCongediUffici.Gestione);
                model.PuoiPrendereIncarico =
                (EnabledToWR && StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione
                && model.InCaricoACollegaAssente)
                ||
                (EnabledToWR && StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.Inviata)
                ||
                (StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione
                && model.SonoADM && StatoAttuale.COD_USER != myRaiHelper.CommonHelper.GetCurrentUserMatricola())
                ;

                model.PuoiRilasciare = StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione &&
                      StatoAttuale.COD_USER == CommonHelper.GetCurrentUserMatricola();
            }
            else if (StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.ApprovataGestione ||
                StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers)
            {
                model.SonoADM =
               MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Personale,
               MaternitaCongediHelper.MaternitaCongediGradiAbil.ADM);

                model.PossibileAssegnare = MaternitaCongediHelper.RichiestaAttualmenteInCaricoAme(rich)
                                           || model.SonoADM;

                string funzioneHrga = "03GEST";
                var giaInCaricoMatricole = rich.XR_WKF_MATCON_OPERSTATI.Where(x =>
                x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers ||
                x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin ||
                x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione).Select(x => x.COD_USER).ToList();

                model.AssegnatariPossibili = myRaiHelper.AuthHelper.GetAllEnabledAs("HRIS", funzioneHrga)
                                            .Where(x => x.Matricola != CommonHelper.GetCurrentUserMatricola()
                                            && !giaInCaricoMatricole.Contains(x.Matricola)
                                            ).ToList();

                model.InCaricoACollegaAssente = false;
                if (StatoAttuale.COD_USER != MiaMatricola &&
                   StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers)
                {
                    MyRaiServiceInterface.it.rai.servizi.digigappws.dayResponse resp =
                        HomeManager.GetEccezioni(DateTime.Now.ToString("ddMMyyyy"), StatoAttuale.COD_USER);

                    if (!resp.eccezioni.Any(x => x.cod.Trim() == "SW") && !resp.timbrature.Any())
                    {
                        model.InCaricoACollegaAssente = true;
                    }

                }
                bool EnabledToWR = MaternitaCongediHelper.EnabledToMaternitaCongediUfficioConDirittiScrittura(MaternitaCongediHelper.MaternitaCongediUffici.Personale);

                model.PuoiPrendereIncarico = false
                //(EnabledToWR && StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers
                //&& model.InCaricoACollegaAssente)
                //||
                //(EnabledToWR && StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.ApprovataGestione)
                //||
                //(StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers
                //&& model.SonoADM && StatoAttuale.COD_USER != myRaiHelper.CommonHelper.GetCurrentUserMatricola())
                ;

                model.PuoiRilasciare = false;
                //StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers &&
                //  StatoAttuale.COD_USER == CommonHelper.GetCurrentUserMatricola();
            }
            else if (StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers ||
                StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin)
            {
                model.FunzioneJSPrendiInCarico = "PrendiInCaricoAmm";
                model.FunzioneJSRilascia = "RilasciaAmm";
                model.FunzioneJSAssegna = "AssegnaRichiestaAmm";
                model.SonoADM =
               MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Amministrazione,
               MaternitaCongediHelper.MaternitaCongediGradiAbil.ADM);

                model.PossibileAssegnare = MaternitaCongediHelper.RichiestaAttualmenteInCaricoAme(rich)
                                           || model.SonoADM;

                string funzioneHrga = "01GEST";
                var giaInCaricoMatricole = rich.XR_WKF_MATCON_OPERSTATI.Where(x =>
                x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers ||
                x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin ||
                x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione).Select(x => x.COD_USER).ToList();

                model.AssegnatariPossibili = myRaiHelper.AuthHelper.GetAllEnabledAs("HRIS", funzioneHrga)
                                            .Where(x => x.Matricola != CommonHelper.GetCurrentUserMatricola()
                                            && !giaInCaricoMatricole.Contains(x.Matricola)
                                            ).ToList();

                model.InCaricoACollegaAssente = false;
                if (StatoAttuale.COD_USER != MiaMatricola &&
                   StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin)
                {
                    MyRaiServiceInterface.it.rai.servizi.digigappws.dayResponse resp =
                        HomeManager.GetEccezioni(DateTime.Now.ToString("ddMMyyyy"), StatoAttuale.COD_USER);

                    if (!resp.eccezioni.Any(x => x.cod.Trim() == "SW") && !resp.timbrature.Any())
                    {
                        model.InCaricoACollegaAssente = true;
                    }

                }
                bool EnabledToWR = MaternitaCongediHelper.EnabledToMaternitaCongediUfficioConDirittiScrittura(MaternitaCongediHelper.MaternitaCongediUffici.Amministrazione);
                model.PuoiPrendereIncarico =
                (EnabledToWR && StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin
                && model.InCaricoACollegaAssente)
                ||
                (EnabledToWR && StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers)
                ||
                (StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin
                && model.SonoADM && StatoAttuale.COD_USER != myRaiHelper.CommonHelper.GetCurrentUserMatricola())
                ;

                model.PuoiRilasciare = StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin &&
                      StatoAttuale.COD_USER == CommonHelper.GetCurrentUserMatricola();
            }

            return model;






            bool SonoADM =
        (StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.Inviata ||
        StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione)
        &&
        MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Gestione,
         MaternitaCongediHelper.MaternitaCongediGradiAbil.ADM);
            if (!SonoADM)
                SonoADM =
                (StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.ApprovataGestione ||
                StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers)
                &&
                MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Personale,
                 MaternitaCongediHelper.MaternitaCongediGradiAbil.ADM);
            if (!SonoADM)
                SonoADM =
                (StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers ||
                StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin)
                &&
                MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Amministrazione,
                 MaternitaCongediHelper.MaternitaCongediGradiAbil.ADM);

            model.SonoADM = SonoADM;

            model.PossibileAssegnare = MaternitaCongediHelper.RichiestaAttualmenteInCaricoAme(rich)
                                       || SonoADM;

            string funzione = null;
            MaternitaCongediHelper.MaternitaCongediUffici? UfficioCorrente = null;


            if (StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin ||
                StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers)
            {
                funzione = "01GEST";
                UfficioCorrente = MaternitaCongediHelper.MaternitaCongediUffici.Amministrazione;
            }
            else if (StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione ||
                StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.Inviata
                )
            {
                funzione = "02GEST";
                UfficioCorrente = MaternitaCongediHelper.MaternitaCongediUffici.Gestione;
            }
            else if (StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers ||
                StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.ApprovataGestione
                )
            {
                funzione = "03GEST";
                UfficioCorrente = MaternitaCongediHelper.MaternitaCongediUffici.Gestione;
            }



            var giaInCarico = rich.XR_WKF_MATCON_OPERSTATI.Where(x =>
             x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers ||
             x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin ||
             x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione).Select(x => x.COD_USER).ToList();

            model.AssegnatariPossibili = myRaiHelper.AuthHelper.GetAllEnabledAs("HRIS", funzione)
                                        .Where(x => x.Matricola != CommonHelper.GetCurrentUserMatricola()
                                        && !giaInCarico.Contains(x.Matricola)
                                        ).ToList();
            model.InCaricoACollegaAssente = false;
            if (StatoAttuale.COD_USER != MiaMatricola &&
               (StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin ||
               StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione ||
               StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers
               )
                )
            {
                MyRaiServiceInterface.it.rai.servizi.digigappws.dayResponse resp =
                    HomeManager.GetEccezioni(DateTime.Now.ToString("ddMMyyyy"), StatoAttuale.COD_USER);

                if (!resp.eccezioni.Any(x => x.cod.Trim() == "SW") && !resp.timbrature.Any())
                {
                    model.InCaricoACollegaAssente = true;
                }

            }

            bool EnabledToWrite = false;
            if (UfficioCorrente == null)
            {
                model.PuoiPrendereIncarico = false;
                model.PuoiRilasciare = false;
                return model;
            }
            else
            {
                EnabledToWrite = MaternitaCongediHelper.EnabledToMaternitaCongediUfficioConDirittiScrittura(UfficioCorrente.Value);
            }


            model.PuoiPrendereIncarico =
                (EnabledToWrite && StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin
                && model.InCaricoACollegaAssente)
                ||
                (EnabledToWrite && StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers)
                ||
                (StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin
                && model.SonoADM && StatoAttuale.COD_USER != myRaiHelper.CommonHelper.GetCurrentUserMatricola())
                ;

            model.PuoiRilasciare = StatoAttuale.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin &&
                  StatoAttuale.COD_USER == CommonHelper.GetCurrentUserMatricola();

            return model;
        }

        public ActionResult GetAssegnazioneAmmBox(int idrichiesta)
        {

            var model = GetAssegnazioneAmmModel(idrichiesta);
            return View("_assegnazioneAmm", model);
        }
        public ActionResult GetAnnullamentoAmmBox(int idrichiesta)
        {
            var model = new AnnullamentoAmmModel(idrichiesta);
            return View("_annullamentoAmm", model);
        }

        public ActionResult
            GetNoteBox(int idrichiesta)
        {
            NotaModel model = new NotaModel(idrichiesta);

            var db = new IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            string matr = CommonHelper.GetCurrentUserMatricola();

            //var StatoAttuale = rich.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();

            model.InCaricoAMe = MaternitaCongediHelper.RichiestaInCaricoAmeAnyTime(rich);

            //rich.XR_WKF_MATCON_OPERSTATI.Any(x => x.COD_USER == matr && 
            //                (x.ID_STATO== (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin ||
            //                x.ID_STATO== (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione ||
            //                x.ID_STATO== (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers)
            //                );

            //bool caricoame = StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin
            //                      &&
            //            StatoAttuale.COD_USER == CommonHelper.GetCurrentUserMatricola();
            string visibilita = MaternitaCongediManager.GetUfficioperNote();

            //if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Amministrazione))
            //    visibilita = "A";
            //else if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Personale))
            //    visibilita = "P";
            //else if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Gestione))
            //    visibilita = "G";




            model.MioUfficioPerNote = visibilita;
            return View("_nota", model);

        }
        /*  url: "/maternitacongedi/cambiagiorni",
                type: 'POST',

                dataType: "json",
                data: { idrichiesta: idrich, giorni: giorni },*/
        public ActionResult cambiagiorni(int idrichiesta, string giorni)
        {
            var db = new IncentiviEntities();
            decimal giornidec = Convert.ToDecimal(giorni.Replace(".", ","));
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            if (rich != null)
            {
                rich.GIORNI_DEFAULT26 = giornidec;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = ex.Message }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }
        private static string GetQueryVariabiliTE1(string matricola, int anno, int mese)
        {
            string query = CommonHelper.GetParametro<string>(EnumParametriSistema.MaternitaCongediTE1)
            //         string query =
            //      @"SELECT anag.[matricola_dp] as Matricola, 
            //anag.forma_contratto,
            //anag.tipo_minimo,	
            //         tempo.num_anno,
            //         tempo.cod_mese,
            //         importi.tot_retrib_annua,
            //         importi.retrib_mensile,
            //         importi.xiii_mensilita,
            //         importi.xiv_mensilita,
            //         importi.minimo,
            //         importi.premio_produzione,
            //importi.superminimo,
            //importi.superminimo_acc_13032018
            //         FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA]
            //         anag
            //         INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE]
            //         te ON(te.[SKY_MATRICOLA] = anag.[sky_anagrafica_unica])
            //         INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO]
            //         tempo ON(te.[SKY_MESE_CONTABILE] = tempo.[sky_tempo])
            //         INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE_IMPORTI]
            //         importi ON(importi.[SKY_riga_te] = te.[SKY_riga_te])
            //         WHERE anag.matricola_dp = '#MATR' 
            //         and num_anno = #ANNO
            //         and cod_mese = #MESE"
            .Replace("#MATR", matricola).Replace("#ANNO", anno.ToString()).Replace("#MESE", mese.ToString());
            return query;
        }
        public static string GetQueryVariabiliTE2(string matricola, int anno, int mese)
        {
            string query = CommonHelper.GetParametro<string>(EnumParametriSistema.MaternitaCongediTE2)
            //string query =
            //    @"SELECT anag.[matricola_dp] as Matricola, 
            //tempo.num_anno,
            //tempo.cod_mese,
            //ind.cod_indennita,
            //ind.desc_indennita,
            //indTe.importo_inden,
            //indTe.perc_inden
            //FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA]
            //anag
            //INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE]
            //te ON(te.[SKY_MATRICOLA] = anag.[sky_anagrafica_unica])
            //INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO]
            //tempo ON(te.[SKY_MESE_CONTABILE] = tempo.[sky_tempo])
            //INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_INDENNITA_TE]
            //indTe on(te.sky_riga_te= indTe.sky_riga_te)
            //INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_INDENNITA]
            //ind on(indTe.sky_indennita= ind.sky_indennita)
            //WHERE anag.matricola_dp = '#MATR' 
            //and num_anno = #ANNO
            //and cod_mese = #MESE"
            .Replace("#MATR", matricola).Replace("#ANNO", anno.ToString()).Replace("#MESE", mese.ToString());
            return query;
        }

        private static string GetQueryVariabili(string matricola, int anno, int mese)
        {
            string query = myRaiHelper.CommonHelper.GetParametro<string>(EnumParametriSistema.QueryStrHRDWCongedi)
                .Replace("#MATR", matricola).Replace("#ANNO", anno.ToString()).Replace("#MESE", mese.ToString());
            //" SELECT t0.matricola_dp, " +
            //" 	CAST(t2.num_anno as int) as Anno, " +
            //"   t2.cod_mese as Mese, " +
            //" 	t3.cod_aggregato_costi, " +
            //" 	t3.desc_aggregato_costi, " +
            //" 	t3.cod_voce_cedolino, " +
            //" 	t3.desc_voce_cedolino, " +
            //" 	sum(t1.quantita_ore) as Ore, " +
            //" 	sum(t1.quantita_giorni) as Giorni, " +
            //" 	sum(t1.quantita_numero_prestaz) as NumeroPrestazioni, " +
            //" 	sum(t1.importo) as Importo " +
            //" FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
            //" INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_CEDO_CASSA] t1 ON (t1.sky_matricola = t0.sky_anagrafica_unica) " +
            //" INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t1.sky_data_competenza = t2.sky_tempo) " +
            //" INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_VOCE_CEDOLINO] t3 ON (t1.sky_voce_cedolino = t3.sky_voce_cedolino) " +
            //" WHERE ( " +
            //" 		t0.matricola_dp ='" + matricola + "' and t1.sky_voce_cedolino_iv=1 " +
            //" 		) " +
            //" GROUP BY t0.matricola_dp, " +
            //" 	t2.num_anno, " +
            //" 	t2.cod_mese, " +
            //" 	t3.cod_aggregato_costi, " +
            //" 	t3.desc_aggregato_costi, " +
            //" 	t3.cod_voce_cedolino, " +
            //" 	t3.desc_voce_cedolino " +
            //" order by t2.num_anno, t3.cod_aggregato_costi, t3.cod_voce_cedolino ";
            return query;
        }
        public ActionResult PrendiVisioneUffPersonale(int idrichiesta)
        {
            var db = new IncentiviEntities();
            string matr = CommonHelper.GetCurrentUserMatricola();

            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            if (rich == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Richiesta non trovata" }
                };
            }
            string nome = UtenteHelper.Nominativo();
            DateTime Dnow = DateTime.Now;
            try
            {
                if (MaternitaCongediManager.IsStatePresentInWorkflow(rich,
                    (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers))
                {
                    myRaiData.Incentivi.XR_WKF_OPERSTATI stato = new myRaiData.Incentivi.XR_WKF_OPERSTATI();
                    stato.ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault();
                    stato.COD_USER = matr;
                    stato.ID_STATO = (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers;
                    stato.TMS_TIMESTAMP = Dnow;
                    stato.VALID_DTA_INI = Dnow;
                    stato.COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT;
                    stato.COD_TERMID = Request.UserHostAddress;
                    stato.NOMINATIVO = nome;
                    stato.XR_MAT_RICHIESTE = rich;
                    db.XR_WKF_OPERSTATI.Add(stato);

                    myRaiData.Incentivi.XR_WKF_OPERSTATI stato2 = new myRaiData.Incentivi.XR_WKF_OPERSTATI();
                    stato2.ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault();
                    stato2.COD_USER = matr;
                    stato2.ID_STATO = (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers;
                    stato2.TMS_TIMESTAMP = Dnow;
                    stato2.VALID_DTA_INI = Dnow;
                    stato2.COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT;
                    stato2.COD_TERMID = Request.UserHostAddress;
                    stato2.NOMINATIVO = nome;
                    stato2.XR_MAT_RICHIESTE = rich;
                    db.XR_WKF_OPERSTATI.Add(stato2);
                }
                else
                {
                    int statoSuccessivo = MaternitaCongediManager.GetStatoSuccessivoSecondoWorkflow(rich);
                    if (statoSuccessivo == (int)MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers)//MG
                    {
                        rich.PERMESSO_FRUIBILE = true;
                        rich.PRESA_VISIONE_RESP_GEST = DateTime.Now;
                        rich.PRESA_VISIONE_RESP_MATR = CommonHelper.GetCurrentUserMatricola();
                        int[] st = new int[] {
                            (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers,
                            (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers };

                        foreach (int s in st)
                        {
                            myRaiData.Incentivi.XR_WKF_OPERSTATI stato = new myRaiData.Incentivi.XR_WKF_OPERSTATI();
                            stato.ID_TIPOLOGIA = (int)rich.XR_MAT_CATEGORIE.ID_TIPOLOGIA_WORKFLOW;// db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault();
                            stato.COD_USER = matr;
                            stato.ID_STATO = s;
                            stato.TMS_TIMESTAMP = Dnow;
                            stato.VALID_DTA_INI = Dnow;
                            stato.COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT;
                            stato.COD_TERMID = Request.UserHostAddress;
                            stato.NOMINATIVO = nome;
                            stato.XR_MAT_RICHIESTE = rich;
                            db.XR_WKF_OPERSTATI.Add(stato);
                        }
                    }
                    else if (statoSuccessivo == (int)MaternitaCongediManager.EnumStatiRichiesta.Approvata)//PN
                    {

                        string esito = MaternitaCongediManager.InserisciPN(rich);
                        if (esito != null)
                        {
                            return new JsonResult
                            {
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                                Data = new { esito = false, errore = esito }
                            };
                        }
                        rich.PERMESSO_FRUIBILE = true;

                        int[] st = new int[] {
                            (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers,
                            (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers,
                            (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.Approvata };
                        foreach (int s in st)
                        {
                            myRaiData.Incentivi.XR_WKF_OPERSTATI stato = new myRaiData.Incentivi.XR_WKF_OPERSTATI();
                            stato.ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault();
                            stato.COD_USER = matr;
                            stato.ID_STATO = s;
                            stato.TMS_TIMESTAMP = Dnow;
                            stato.VALID_DTA_INI = Dnow;
                            stato.COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT;
                            stato.COD_TERMID = Request.UserHostAddress;
                            stato.NOMINATIVO = nome;
                            stato.XR_MAT_RICHIESTE = rich;
                            db.XR_WKF_OPERSTATI.Add(stato);
                        }
                    }
                    else
                    {
                        var statoprec = rich.XR_WKF_MATCON_OPERSTATI.Where(x =>
                   x.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.ApprovataGestione).FirstOrDefault();

                        statoprec.UFFPERS_PRESA_VISIONE = true;
                    }


                }

                //var statoFittizioIncarico = rich.XR_WKF_MATCON_OPERSTATI.Where(x =>
                //x.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers).FirstOrDefault();

                //var statoFittizioApprovato = rich.XR_WKF_MATCON_OPERSTATI.Where(x =>
                //x.ID_STATO == (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers).FirstOrDefault();

                //if (statoFittizioIncarico != null && statoFittizioApprovato != null)
                //{
                //    statoFittizioIncarico.COD_USER = matr;
                //    statoFittizioApprovato.COD_USER = matr;

                //    statoFittizioIncarico.TMS_TIMESTAMP = Dnow;
                //    statoFittizioApprovato.TMS_TIMESTAMP = Dnow;

                //    statoFittizioIncarico.VALID_DTA_INI = Dnow;
                //    statoFittizioApprovato.VALID_DTA_INI = Dnow;

                //    statoFittizioIncarico.COD_TERMID = Request.UserHostAddress;
                //    statoFittizioApprovato.COD_TERMID = Request.UserHostAddress;

                //    statoFittizioIncarico.NOMINATIVO = nome;
                //    statoFittizioApprovato.NOMINATIVO = nome;

                //    statoFittizioIncarico.UFFPERS_PRESA_VISIONE = true;
                //    statoFittizioApprovato.UFFPERS_PRESA_VISIONE = true;
                //}
                //else
                //{
                //    myRaiData.Incentivi.XR_WKF_OPERSTATI stato = new myRaiData.Incentivi.XR_WKF_OPERSTATI();
                //    stato.ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault();
                //    stato.COD_USER = matr;
                //    stato.ID_STATO = (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers;
                //    stato.TMS_TIMESTAMP = Dnow;
                //    stato.VALID_DTA_INI = Dnow;
                //    stato.COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT;
                //    stato.COD_TERMID = Request.UserHostAddress;
                //    stato.NOMINATIVO = nome;
                //    stato.XR_MAT_RICHIESTE = rich;
                //    db.XR_WKF_OPERSTATI.Add(stato);

                //    myRaiData.Incentivi.XR_WKF_OPERSTATI stato2 = new myRaiData.Incentivi.XR_WKF_OPERSTATI();
                //    stato2.ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault();
                //    stato2.COD_USER = matr;
                //    stato2.ID_STATO = (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers;
                //    stato2.TMS_TIMESTAMP = Dnow;
                //    stato2.VALID_DTA_INI = Dnow;
                //    stato2.COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT;
                //    stato2.COD_TERMID = Request.UserHostAddress;
                //    stato2.NOMINATIVO = nome;
                //    stato2.XR_MAT_RICHIESTE = rich;
                //    db.XR_WKF_OPERSTATI.Add(stato2);
                //}



                db.SaveChanges();

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }

        }
        public ActionResult GetPianificazioneContent(int idRichiesta)
        {
            MatPianificazioneModel model = new MatPianificazioneModel();

            var db = new IncentiviEntities();

            model.Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idRichiesta).FirstOrDefault();
            model.pf = FeriePermessiManager.GetPianoFerieAnno(((DateTime)model.Richiesta.INIZIO_GIUSTIFICATIVO).Year);

            model.DataInizialeCalendario = new DateTime(
                ((DateTime)model.Richiesta.INIZIO_GIUSTIFICATIVO).AddMonths(-3).Year,
                ((DateTime)model.Richiesta.INIZIO_GIUSTIFICATIVO).AddMonths(-3).Month,
                1);
            model.DataFinaleCalendario = new DateTime(
                ((DateTime)model.Richiesta.FINE_GIUSTIFICATIVO).AddMonths(3).Year,
                ((DateTime)model.Richiesta.FINE_GIUSTIFICATIVO).AddMonths(3).Month,
                1);

            return View("PopupVisGestPianificazioneContent", model);
        }
        public ActionResult GetCalendarioContent(int idRichiesta, string d1 = null, string d2 = null, string ecc = null)
        {
            MatCalendarioTracciatiModel model = new MatCalendarioTracciatiModel();
            if (!String.IsNullOrWhiteSpace(d1) && !String.IsNullOrWhiteSpace(d2)
                && !String.IsNullOrWhiteSpace(ecc))
            {
                model.IsFromDem = true;
            }

            var db = new IncentiviEntities();
            DateTime D1;
            DateTime D2;
            string matricola;
            if (!model.IsFromDem)
            {
                model.Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idRichiesta).FirstOrDefault();
                D1 = model.Richiesta.INIZIO_GIUSTIFICATIVO ?? model.Richiesta.DATA_INIZIO_MATERNITA.Value;
                D2 = model.Richiesta.FINE_GIUSTIFICATIVO ?? model.Richiesta.DATA_FINE_MATERNITA.Value;
                matricola = model.Richiesta.MATRICOLA;
            }
            else
            {

                model.EccezioneFromDEM = ecc;
                model.IDdocumentoDEM = idRichiesta;
                DateTime.TryParseExact(d1, "dd/MM/yyyy", null, DateTimeStyles.None, out D1);
                DateTime.TryParseExact(d2, "dd/MM/yyyy", null, DateTimeStyles.None, out D2);
                model.InizioPeriodoDEM = D1;
                model.FinePeriodoDEM = D2;
                matricola = db.XR_DEM_DOCUMENTI.Where(x => x.Id == idRichiesta).Select(x => x.MatricolaDestinatario).FirstOrDefault();
                var sint = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matricola).FirstOrDefault();
                if (sint != null)
                    model.NominativoFromDEM = sint.DES_COGNOMEPERS + " " + sint.DES_NOMEPERS;

                var dbR = new myRaiData.digiGappEntities();
                model.ListaPianoFerieBatch = dbR.MyRai_PianoFerieBatch
                                            .Where(x => x.matricola == matricola &&
                                                      x.codice_eccezione == ecc &&
                                                      x.data_eccezione >= D1 &&
                                                      x.data_eccezione <= D2
                                                      ).ToList();
            }


            model.pf = FeriePermessiManager.GetPianoFerieAnno(D1.Year, true, matricola);
            foreach (var g in model.pf.dipendente.ferie.giornate)
            {
                if (!g.dataTeorica.Contains(D1.Year.ToString()))
                {
                    g.dataTeorica += "-" + D1.Year.ToString();
                }
                else
                {

                }

            }
            if (D2.Year > D1.Year)
            {
                var Ferie = FeriePermessiManager.GetPianoFerieAnno(D2.Year, true, matricola);
                if (Ferie != null && Ferie.dipendente != null && Ferie.dipendente.ferie != null && Ferie.dipendente.ferie.giornate != null)
                {
                    foreach (var g in Ferie.dipendente.ferie.giornate)
                    {
                        g.dataTeorica += "-" + D2.Year.ToString();
                    }
                    var L = model.pf.dipendente.ferie.giornate.ToList();
                    L.AddRange(Ferie.dipendente.ferie.giornate.ToList());
                    model.pf.dipendente.ferie.giornate = L.ToArray();
                }
            }
            model.DataInizialeCalendario = new DateTime(
                D1.AddMonths(-1).Year,
                D1.AddMonths(-1).Month,
                1);
            model.DataFinaleCalendario = new DateTime(
                D2.AddMonths(1).Year,
                D2.AddMonths(1).Month,
                1);

            return View("PopupVisGestCalendarioTracciatiContent", model);
        }
        public XR_WKF_OPERSTATI GetNuovoStato(XR_WKF_OPERSTATI oldStato, string nuovoOperatore)
        {
            XR_WKF_OPERSTATI newStato = new XR_WKF_OPERSTATI()
            {
                COD_TERMID = oldStato.COD_TERMID,
                COD_TIPO_PRATICA = oldStato.COD_TIPO_PRATICA,
                COD_USER = nuovoOperatore,
                DTA_OPERAZIONE = oldStato.DTA_OPERAZIONE,
                ID_GESTIONE = oldStato.ID_GESTIONE,
                ID_PERSONA = oldStato.ID_PERSONA,
                ID_STATO = oldStato.ID_STATO,
                ID_TIPOLOGIA = oldStato.ID_TIPOLOGIA,
                NASCOSTO = false,
                NOMINATIVO = CommonHelper.GetNominativoPerMatricola(nuovoOperatore),
                TMS_TIMESTAMP = DateTime.Now,
                VALID_DTA_INI = DateTime.Now,
                VALID_DTA_END = oldStato.VALID_DTA_END,
                XR_MAT_RICHIESTE = oldStato.XR_MAT_RICHIESTE
            };
            return newStato;
        }
        public ActionResult Assegna(string operatore, int idRichiesta)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idRichiesta).FirstOrDefault();

            var stato = rich.XR_WKF_MATCON_OPERSTATI
                .OrderByDescending(x => x.ID_STATO)
                .FirstOrDefault();

            if (stato.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers)
            {
                //se ADM assegna una non ancora in carico
                return PrendiInCaricoAmministrazione(idRichiesta, matr: operatore);
            }
            if (stato.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.Inviata)
            {
                //se ADM assegna una non ancora in carico
                return PrendiInCaricoGestione(idRichiesta, operatore);
            }
            try
            {
                stato.NASCOSTO = true;

                db.XR_WKF_OPERSTATI.Add(GetNuovoStato(stato, operatore));

                //stato.COD_USER = operatore;
                //stato.TMS_TIMESTAMP = DateTime.Now;
                //stato.VALID_DTA_INI = DateTime.Now;
                //stato.NOMINATIVO = CommonHelper.GetNominativoPerMatricola(operatore);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }
            Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "Portale",
                data = DateTime.Now,
                matricola = CommonHelper.GetCurrentUserMatricola(),
                provenienza = "MaternitaCongedi/Assegna",
                operazione = "MaternitaCongedi/Assegna",
                descrizione_operazione = "Richiesta id " + idRichiesta + " assegnata a " + operatore
            });
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };

        }
        public ActionResult RilasciaAmministrazione(int idrichiesta)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            string matr = CommonHelper.GetCurrentUserMatricola();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            if (rich != null)
            {
                try
                {
                    var list = db.XR_WKF_OPERSTATI.Where(x => x.ID_GESTIONE == rich.ID
                                && x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin
                                ).Select(x => x.ID_OPERSTATI).ToList();

                    foreach (int id in list)
                    {
                        var s = db.XR_WKF_OPERSTATI.Where(x => x.ID_OPERSTATI == id).FirstOrDefault();
                        db.XR_WKF_OPERSTATI.Remove(s);
                    }
                    db.SaveChanges();
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = true }
                    };
                }
                catch (Exception ex)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = ex.Message }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = false, errore = "Richiesta non trovata" }
            };
        }
        public ActionResult RilasciaGestione(int idrichiesta)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            string matr = CommonHelper.GetCurrentUserMatricola();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            if (rich != null)
            {
                try
                {

                    var list = db.XR_WKF_OPERSTATI.Where(x => x.ID_GESTIONE == rich.ID
                                //&& (x.COD_TIPO_PRATICA == "MAT" || x.COD_TIPO_PRATICA == "CON")
                                && db.XR_MAT_CATEGORIE.Select(z => z.CAT).Distinct().Contains(x.COD_TIPO_PRATICA)
                                && x.ID_STATO > (int)MaternitaCongediManager.EnumStatiRichiesta.Inviata
                                ).Select(x => x.ID_OPERSTATI).ToList();


                    foreach (int id in list)
                    {
                        var s = db.XR_WKF_OPERSTATI.Where(x => x.ID_OPERSTATI == id).FirstOrDefault();
                        db.XR_WKF_OPERSTATI.Remove(s);
                    }

                    db.SaveChanges();
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = true }
                    };
                }
                catch (Exception ex)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = ex.Message }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = false, errore = "Richiesta non trovata" }
            };
        }
        public ActionResult PrendiInCaricoGestione(int idrichiesta, string matr = null)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            if (matr == null) matr = CommonHelper.GetCurrentUserMatricola();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            if (rich != null)
            {
                if (rich.XR_WKF_MATCON_OPERSTATI.Any(x => x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione))
                {
                    //se ADM ha preso in carico
                    var tbd = rich.XR_WKF_OPERSTATI.Where(x => x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione)
                        .Select(x => x.ID_OPERSTATI).ToList();
                    foreach (int id in tbd)
                    {
                        var operstato = db.XR_WKF_OPERSTATI.Where(x => x.ID_OPERSTATI == id).FirstOrDefault();
                        if (operstato != null)
                            operstato.NASCOSTO = true;
                    }
                }
                myRaiData.Incentivi.XR_WKF_OPERSTATI stato = new myRaiData.Incentivi.XR_WKF_OPERSTATI();
                try
                {
                    string tipologia = "MATCON_GENERICO";
                    if (rich.XR_MAT_CATEGORIE.CAT == "SW")
                        tipologia = "SW_EXT";

                    stato.ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == tipologia).Select(x => x.ID_TIPOLOGIA).FirstOrDefault();
                    stato.COD_USER = matr;
                    stato.ID_STATO = (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione;
                    stato.TMS_TIMESTAMP = DateTime.Now;
                    stato.VALID_DTA_INI = DateTime.Now;
                    stato.COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT;
                    stato.COD_TERMID = Request.UserHostAddress;
                    stato.NOMINATIVO = UtenteHelper.Nominativo();

                    stato.XR_MAT_RICHIESTE = rich;
                    db.XR_WKF_OPERSTATI.Add(stato);
                    db.SaveChanges();

                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = true }
                    };
                }
                catch (Exception ex)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = ex.Message }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = false, errore = "Richiesta non trovata" }
            };
        }
        public ActionResult PrendiInCaricoAmministrazioneTE(int idrichiesta, string matr = null)
        {
            return PrendiInCaricoAmministrazione(idrichiesta, false, matr);
        }
        public ActionResult PrendiInCaricoAmministrazione(int idrichiesta, bool comprese = false, string matr = null)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            if (matr == null) matr = CommonHelper.GetCurrentUserMatricola();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            if (rich != null)
            {
                if (rich.XR_WKF_MATCON_OPERSTATI.Any(x => x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin))
                {
                    //se ADM ha preso in carico
                    var tbd = rich.XR_WKF_OPERSTATI.Where(x => x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin)
                        .Select(x => x.ID_OPERSTATI).ToList();
                    foreach (int id in tbd)
                    {
                        var operstato = db.XR_WKF_OPERSTATI.Where(x => x.ID_OPERSTATI == id).FirstOrDefault();
                        if (operstato != null)
                            operstato.NASCOSTO = true;
                    }
                }

                try
                {



                    if (!rich.XR_WKF_OPERSTATI.Any() ||
                          rich.XR_WKF_OPERSTATI.Max(x => x.ID_STATO < (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin))
                    {
                        myRaiData.Incentivi.XR_WKF_OPERSTATI stato = new myRaiData.Incentivi.XR_WKF_OPERSTATI();
                        stato.ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault();
                        stato.COD_USER = matr;
                        stato.ID_STATO = (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin;
                        stato.TMS_TIMESTAMP = DateTime.Now;
                        stato.VALID_DTA_INI = DateTime.Now;
                        stato.COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT;
                        stato.COD_TERMID = Request.UserHostAddress;
                        if (matr == CommonHelper.GetCurrentUserMatricola())
                            stato.NOMINATIVO = UtenteHelper.Nominativo();
                        else
                            stato.NOMINATIVO = BatchManager.GetUserData(matr, rich.INIZIO_GIUSTIFICATIVO ?? rich.DATA_INIZIO_MATERNITA.Value).nominativo;
                        stato.XR_MAT_RICHIESTE = rich;
                        db.XR_WKF_OPERSTATI.Add(stato);
                        db.SaveChanges();
                    }


                    if (comprese)
                    {
                        var ListaRichieste = RichiesteStessoMese(idrichiesta);
                        ListaRichieste = ListaRichieste.Where(x => x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) <
                        (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin).ToList();
                        foreach (var richiesta in ListaRichieste)
                        {
                            if (richiesta.XR_WKF_OPERSTATI.Max(x => x.ID_STATO < (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin))
                            {
                                myRaiData.Incentivi.XR_WKF_OPERSTATI statoNew = new myRaiData.Incentivi.XR_WKF_OPERSTATI();
                                statoNew.ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault();
                                statoNew.COD_USER = matr;
                                statoNew.ID_STATO = (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin;
                                statoNew.TMS_TIMESTAMP = DateTime.Now;
                                statoNew.VALID_DTA_INI = DateTime.Now;
                                statoNew.COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT;
                                statoNew.COD_TERMID = Request.UserHostAddress;
                                if (matr == CommonHelper.GetCurrentUserMatricola())
                                    statoNew.NOMINATIVO = UtenteHelper.Nominativo();
                                else
                                    statoNew.NOMINATIVO = BatchManager.GetUserData(matr, richiesta.INIZIO_GIUSTIFICATIVO ?? rich.DATA_INIZIO_MATERNITA.Value).nominativo;
                                statoNew.ID_GESTIONE = richiesta.ID;
                                db.XR_WKF_OPERSTATI.Add(statoNew);
                                db.SaveChanges();
                            }
                        }
                    }
                    db.SaveChanges();

                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = true }
                    };
                }
                catch (Exception ex)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = ex.Message }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = false, errore = "Richiesta non trovata" }
            };
        }
        public ActionResult RespingiGestione(int idrichiesta, string nota, string allegatiOK, string allegatiNOK)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            List<int> ListAllegatiOK = new List<int>();
            if (!String.IsNullOrWhiteSpace(allegatiOK))
                ListAllegatiOK = allegatiOK.Split(',').Select(x => Convert.ToInt32(x)).ToList();

            List<int> ListAllegatiNOK = new List<int>();
            if (!String.IsNullOrWhiteSpace(allegatiNOK))
                ListAllegatiNOK = allegatiNOK.Split(',').Select(x => Convert.ToInt32(x)).ToList();

            XR_MAT_SEGNALAZIONI S = new XR_MAT_SEGNALAZIONI()
            {
                APERTA_DA = CommonHelper.GetCurrentUserMatricola(),
                DATA_APERTURA = DateTime.Now,
                RISOLTA = false,
                XR_MAT_RICHIESTE = rich
            };
            db.XR_MAT_SEGNALAZIONI.Add(S);
            XR_MAT_SEGNALAZIONI_COMUNICAZIONI SC = new XR_MAT_SEGNALAZIONI_COMUNICAZIONI()
            {
                MATRICOLA_FROM = CommonHelper.GetCurrentUserMatricola(),
                MATRICOLA_TO = rich.MATRICOLA,
                NOTA = nota,
                TIMESTAMP = DateTime.Now,
                XR_MAT_SEGNALAZIONI = S
            };
            db.XR_MAT_SEGNALAZIONI_COMUNICAZIONI.Add(SC);
            SC.XR_MAT_ALLEGATI = db.XR_MAT_ALLEGATI.Where(x => ListAllegatiNOK.Contains(x.ID)).ToList();

            foreach (var a in db.XR_MAT_ALLEGATI.Where(x => ListAllegatiOK.Contains(x.ID)))
                a.ID_STATO = 20;
            foreach (var a in db.XR_MAT_ALLEGATI.Where(x => ListAllegatiNOK.Contains(x.ID)))
                a.ID_STATO = 50;

            db.SaveChanges();

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }


        [HttpPost]
        public ActionResult AnnullaPratica(string testo
            , int idrichiesta, string datablocco, string nomefile, HttpPostedFileBase file
            )
        {
            DateTime D;
            if (!DateTime.TryParseExact(datablocco, "dd/MM/yyyy", null, DateTimeStyles.None, out D))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Data non valida" }
                };
            }
            var db = new myRaiData.Incentivi.IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            if (rich.FINE_GIUSTIFICATIVO != null)
            {
                if (D < rich.INIZIO_GIUSTIFICATIVO.Value || D >= rich.FINE_GIUSTIFICATIVO.Value)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = "Data non valida" }
                    };
                }
            }
            if (rich.DATA_FINE_MATERNITA != null)
            {
                if (D < rich.DATA_INIZIO_MATERNITA.Value || D >= rich.DATA_FINE_MATERNITA.Value)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = "Data non valida" }
                    };
                }
            }
            string esito = MaternitaCongediManager.SalvaNota(idrichiesta, testo, "*", file, null, nomefile);
            if (esito != null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = esito }
                };
            }


            if (rich.FINE_GIUSTIFICATIVO != null)
            {

                rich.FINE_GIUSTIFICATIVO = D;
            }

            else
            {
                rich.DATA_FINE_MATERNITA = D;
            }


            foreach (var task in rich.XR_MAT_TASK_IN_CORSO)
            {
                if (task.TERMINATA == true)
                    continue;

                DateTime Dtask = new DateTime(task.ANNO, task.MESE, 1);
                if (Dtask.AddMonths(1) >= D)
                {
                    task.BLOCCATA_DATETIME = DateTime.Now;
                    task.BLOCCATA_DA_OPERATORE = CommonHelper.GetCurrentUserMatricola();
                }
            }
            try
            {
                myRaiData.Incentivi.XR_WKF_OPERSTATI newStato = new XR_WKF_OPERSTATI()
                {
                    COD_TERMID = Request.UserHostAddress,
                    COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT,
                    COD_USER = CommonHelper.GetCurrentUserMatricola(),
                    ID_STATO = (int)MaternitaCongediManager.EnumStatiRichiesta.AnnullataAmministrazione,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    TMS_TIMESTAMP = DateTime.Now,
                    VALID_DTA_INI = DateTime.Now,
                    XR_MAT_RICHIESTE = rich,
                    ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == "MATCON_GENERICO").Select(x => x.ID_TIPOLOGIA).FirstOrDefault()
                };
                db.XR_WKF_OPERSTATI.Add(newStato);
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }


        }
        public ActionResult AnnullaRichiesta(int idrichiesta, string nota = null)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            try
            {
                string tipologia = "MATCON_GENERICO";
                if (rich.XR_MAT_CATEGORIE.CAT == "SW")
                    tipologia = "SW_EXT";

                myRaiData.Incentivi.XR_WKF_OPERSTATI newStato = new XR_WKF_OPERSTATI()
                {
                    COD_TERMID = Request.UserHostAddress,
                    COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT,
                    COD_USER = CommonHelper.GetCurrentUserMatricola(),
                    ID_STATO = (int)MaternitaCongediManager.EnumStatiRichiesta.AnnullataGestione,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    TMS_TIMESTAMP = DateTime.Now,
                    VALID_DTA_INI = DateTime.Now,
                    XR_MAT_RICHIESTE = rich,
                    ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == tipologia).Select(x => x.ID_TIPOLOGIA).FirstOrDefault()
                };
                db.XR_WKF_OPERSTATI.Add(newStato);


                XR_MAT_NOTE dbNota = new XR_MAT_NOTE();
                dbNota.MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola();
                dbNota.DATA_INSERIMENTO = DateTime.Now;
                dbNota.TESTO = nota;
                dbNota.VISIBILITA = "*";
                dbNota.XR_MAT_RICHIESTE = rich;
                db.XR_MAT_NOTE.Add(dbNota);

                db.SaveChanges();

                if (rich.XR_MAT_CATEGORIE.CAT == "SW")
                {
                    bool result = AnagraficaManager.AnnullaModificheRichiestaSW(rich, out string errorMsg);
                    myRaiHelper.HrisHelper.LogOperazione("Smartworking", String.Format("Annullamento richiesta: {0} - {1}", rich.MATRICOLA, rich.ID), result, errorMsg);
                }

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }
        }
        private void AggiungiStatiFittiziUfficioPersonale(XR_WKF_OPERSTATI stato, IncentiviEntities db, XR_MAT_RICHIESTE rich)
        {
            myRaiData.Incentivi.XR_WKF_OPERSTATI StatoNew1 = new myRaiData.Incentivi.XR_WKF_OPERSTATI()
            {
                COD_TERMID = stato.COD_TERMID,
                COD_TIPO_PRATICA = stato.COD_TIPO_PRATICA,
                COD_USER = stato.COD_USER,
                ID_STATO = (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers,
                NOMINATIVO = stato.NOMINATIVO,
                TMS_TIMESTAMP = stato.TMS_TIMESTAMP,
                ID_TIPOLOGIA = stato.ID_TIPOLOGIA,
                VALID_DTA_INI = stato.VALID_DTA_INI,
                XR_MAT_RICHIESTE = rich,
                UFFPERS_PRESA_VISIONE = false
            };
            db.XR_WKF_OPERSTATI.Add(StatoNew1);

            myRaiData.Incentivi.XR_WKF_OPERSTATI StatoNew2 = new myRaiData.Incentivi.XR_WKF_OPERSTATI()
            {
                COD_TERMID = stato.COD_TERMID,
                COD_TIPO_PRATICA = stato.COD_TIPO_PRATICA,
                COD_USER = stato.COD_USER,
                ID_STATO = (int)MaternitaCongediManager.EnumStatiRichiesta.ApprovataUffPers,
                NOMINATIVO = stato.NOMINATIVO,
                TMS_TIMESTAMP = stato.TMS_TIMESTAMP,
                ID_TIPOLOGIA = stato.ID_TIPOLOGIA,
                VALID_DTA_INI = stato.VALID_DTA_INI,
                XR_MAT_RICHIESTE = rich,
                UFFPERS_PRESA_VISIONE = false
            };
            db.XR_WKF_OPERSTATI.Add(StatoNew2);
        }
        public ActionResult ApprovaGestione(int idrichiesta)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            var rich = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            if (rich == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Richiesta non trovata" }
                };
            }
            try
            {
                //var statiWorkflow = db.XR_WKF_WORKFLOW.Where(x => x.ID_TIPOLOGIA == rich.XR_MAT_CATEGORIE.ID_TIPOLOGIA_WORKFLOW)
                //    .OrderBy(x => x.ORDINE).ToList();
                //int statoAttuale = rich.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).Select(x => x.ID_STATO).FirstOrDefault();
                //int statoNext = statiWorkflow.SkipWhile(x => x.ID_STATO <= statoAttuale).Select(x => x.ID_STATO).FirstOrDefault();

                string tipologia = "MATCON_GENERICO";
                if (rich.XR_MAT_CATEGORIE.CAT == "SW")
                    tipologia = "SW_EXT";

                myRaiData.Incentivi.XR_WKF_OPERSTATI Stato = new myRaiData.Incentivi.XR_WKF_OPERSTATI()
                {
                    COD_TERMID = Request.UserHostAddress,
                    COD_TIPO_PRATICA = rich.XR_MAT_CATEGORIE.CAT,
                    COD_USER = CommonHelper.GetCurrentUserMatricola(),
                    ID_STATO = (int)MaternitaCongediManager.EnumStatiRichiesta.ApprovataGestione,
                    NOMINATIVO = UtenteHelper.Nominativo(),
                    TMS_TIMESTAMP = DateTime.Now,
                    ID_TIPOLOGIA = db.XR_WKF_TIPOLOGIA.Where(x => x.COD_TIPOLOGIA == tipologia).Select(x => x.ID_TIPOLOGIA).FirstOrDefault(),
                    VALID_DTA_INI = DateTime.Now,
                    XR_MAT_RICHIESTE = rich
                };
                db.XR_WKF_OPERSTATI.Add(Stato);

                if (rich.ECCEZIONE == "PN")
                {
                    rich.PERMESSO_FRUIBILE = true;
                }


                foreach (var a in rich.XR_MAT_ALLEGATI)
                {
                    a.ID_STATO = 20;
                }

                if (rich.XR_MAT_SEGNALAZIONI.Any())
                {
                    foreach (var s in rich.XR_MAT_SEGNALAZIONI)
                        s.RISOLTA = true;
                }
                if (rich.XR_MAT_CATEGORIE.UFFPERS_PARALLELO || rich.ECCEZIONE == "MG")
                {
                    AggiungiStatiFittiziUfficioPersonale(Stato, db, rich); //per presa visione parallela di uffpers
                }
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }
        }



        public ActionResult InviaMessaggioSegnalazione(int idSegnalazione, string nota, string idAllegatiRifiutati)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            var S = db.XR_MAT_SEGNALAZIONI.Where(x => x.ID == idSegnalazione).FirstOrDefault();
            if (S == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Segnalazione non trovata" }
                };
            }
            try
            {

                XR_MAT_SEGNALAZIONI_COMUNICAZIONI SC = new XR_MAT_SEGNALAZIONI_COMUNICAZIONI();
                SC.MATRICOLA_FROM = CommonHelper.GetCurrentUserMatricola();
                SC.MATRICOLA_TO = S.XR_MAT_RICHIESTE.MATRICOLA;
                SC.NOTA = nota;
                SC.TIMESTAMP = DateTime.Now;
                SC.XR_MAT_SEGNALAZIONI = S;

                db.XR_MAT_SEGNALAZIONI_COMUNICAZIONI.Add(SC);
                foreach (var al in S.XR_MAT_RICHIESTE.XR_MAT_ALLEGATI.Where(x => x.ID_STATO == 10))
                {
                    al.ID_STATO = 20;
                }

                if (!String.IsNullOrWhiteSpace(idAllegatiRifiutati))
                {
                    string[] ids = idAllegatiRifiutati.Split(',');
                    foreach (var id in ids)
                    {
                        int idn = Convert.ToInt32(id);
                        var a = db.XR_MAT_ALLEGATI.Where(x => x.ID == idn).FirstOrDefault();
                        if (a != null)
                        {
                            a.ID_STATO = 50;
                            SC.XR_MAT_ALLEGATI.Add(a);
                        }
                    }
                }
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true, idrichiesta = S.XR_MAT_RICHIESTE.ID }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }
        }

        public ActionResult GetUffPersonaleInfoBox(int idrichiesta)
        {
            UffPersBoxModel model = new UffPersBoxModel();
            var db = new myRaiData.Incentivi.IncentiviEntities();
            model.Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            model.isUffPers =
                MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Personale);
            model.EccezioniInserite = model.Richiesta.XR_MAT_CATEGORIE.ECCEZIONE;

            model.EccezioniInserite = MaternitaCongediManager.GetEccezioneRisultante(model.Richiesta);
            //if (model.EccezioniInserite.Contains("BF"))
            //{
            //    model.EccezioniInserite = "AF";
            //    if (model.Richiesta.DATA_NASCITA_BAMBINO.Value.AddYears(6) < DateTime.Now)
            //        model.EccezioniInserite = "BF";
            //}
            return View("_UffPersonaleInfoBox", model);
        }

        public ActionResult GetVisualizzazioneAnnullamento(int idrichiesta)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            AnnullamentoBoxModel model = new AnnullamentoBoxModel()
            {
                Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault()
            };
            return View("PopupvisGestAnnulla", model);
        }

        public ActionResult GetPromemoriaBox(int idrichiesta)
        {
            myRaiCommonModel.PromemoriaModel model = new PromemoriaModel();
            var db = new IncentiviEntities();
            model.Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();

            var stato = model.Richiesta.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();
            if (stato.COD_USER == CommonHelper.GetCurrentUserMatricola())
            {
                if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Gestione))
                {
                    model.UfficioGPA = "G";
                    model.InCaricoAMe = stato.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione;
                }
                if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Personale))
                {
                    model.UfficioGPA = "P";
                    model.InCaricoAMe = stato.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers;
                }
                if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Amministrazione))
                {
                    model.UfficioGPA = "A";
                    model.InCaricoAMe = stato.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin;
                }
            }
            else
                model.InCaricoAMe = false;

            if (MaternitaCongediHelper.EnabledToMaternitaCongediUfficioAnyRole(MaternitaCongediHelper.MaternitaCongediUffici.Personale))
            {
                model.UfficioGPA = "P";
                model.InCaricoAMe = stato.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoUffPers;
            }

            return View("_promemoria", model);
        }
        public ActionResult GetVisualizzazioneAssegnazione(int idrichiesta)
        {
            var model1 = GetAssegnazioneAmmModel(idrichiesta);
            return View("_assegnazioneAmm", model1);

            var db = new myRaiData.Incentivi.IncentiviEntities();
            var model = new AssegnazioneBoxModel();
            model.Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            var StatoAttuale = model.Richiesta.XR_WKF_MATCON_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();

            model.PossibileAssegnare = StatoAttuale.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione
                                       &&
                                       StatoAttuale.COD_USER == CommonHelper.GetCurrentUserMatricola();
            model.AssegnatariPossibili = myRaiHelper.AuthHelper.GetAllEnabledAs("HRIS", "02GEST")
                .Where(x => x.Matricola != CommonHelper.GetCurrentUserMatricola()).ToList();

            return View("PopupvisGestAssegnazione", model);
        }
        public ActionResult GetPdfNota(int id)
        {
            var db = new IncentiviEntities();
            var pdf = db.XR_MAT_NOTE.Where(x => x.ID == id).FirstOrDefault();
            byte[] byteArray = pdf.FILE_CONTENT;
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;

            Response.AppendHeader("content-disposition", "inline; filename=" + "test.pdf");
            return new FileStreamResult(pdfStream, "application/pdf");
        }
        public ActionResult getInfoPdf(int idallegato)
        {
            var db = new IncentiviEntities();
            var pdf = db.XR_MAT_ALLEGATI.Where(x => x.ID == idallegato).FirstOrDefault();
            if (pdf == null)
                return Content("id non trovato");

            string contenuto = MaternitaCongediManager.GetCampoPdf(pdf.BYTECONTENT);
            return Content(contenuto);
        }
        public ActionResult GetAllegatiVisualizzazione(int idRichiesta, bool InCaricoAMe)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            string matr = CommonHelper.GetCurrentUserMatricola();
            AllegatiReviewModel model = new AllegatiReviewModel() { InCaricoAMe = InCaricoAMe };
            var ric = db.XR_MAT_RICHIESTE.Where(x => x.ID == idRichiesta).FirstOrDefault();
            model.Categoria = ric.XR_MAT_CATEGORIE.CAT;

            if (ric.XR_MAT_CATEGORIE.CAT != "SW")
            {
                model.Allegati = db.XR_MAT_ALLEGATI.Where(x => x.MATRICOLA == ric.MATRICOLA && x.DATA_CONSOLIDATO != null &&
                x.ID_RICHIESTA == idRichiesta).ToList();
            }
            else
            {
                model.Allegati = db.XR_MAT_ALLEGATI.Where(x => x.MATRICOLA == ric.MATRICOLA &&
                    x.ID_RICHIESTA == idRichiesta).ToList();
            }

            model.TipoAllegati = db.XR_MAT_TIPOALLEGATI.ToList();
            model.StatoRichiesta = ric.XR_WKF_MATCON_OPERSTATI.Max(x => x.ID_STATO);
            return View("_allegatoReview", model);
        }
        public ActionResult GetPDF(int id)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            string matr = CommonHelper.GetCurrentUserMatricola();
            var pdf = db.XR_MAT_ALLEGATI.Where(x => x.ID == id).FirstOrDefault();

            byte[] byteArray = pdf.BYTECONTENT;
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;

            Response.AppendHeader("content-disposition", "inline; filename=" + "test.pdf");
            return new FileStreamResult(pdfStream, "application/pdf");

        }
        public ActionResult GetPDFnote(int id)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            string matr = CommonHelper.GetCurrentUserMatricola();
            var pdf = db.XR_MAT_NOTE.Where(x => x.ID == id).FirstOrDefault();

            byte[] byteArray = pdf.FILE_CONTENT;
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;

            Response.AppendHeader("content-disposition", "inline; filename=" + pdf.FILE_NAME);
            return new FileStreamResult(pdfStream, "application/pdf");

        }
        public ActionResult CambiaPromemoria(int idrichiesta, string data)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            var r = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
            if (r != null)
            {
                DateTime D;
                if (DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D))
                {
                    if (r.XR_MAT_PROMEMORIA.Any())
                    {
                        r.XR_MAT_PROMEMORIA.FirstOrDefault().DATA = D;
                    }
                    else
                    {
                        XR_MAT_PROMEMORIA p = new XR_MAT_PROMEMORIA();
                        p.DATA = D;
                        p.DATA_INSERITO = DateTime.Now;
                        p.MATRICOLA = CommonHelper.GetCurrentUserMatricola();
                        p.ID_RICHIESTA = r.ID;
                        db.XR_MAT_PROMEMORIA.Add(p);

                    }
                    db.SaveChanges();
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = true }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = false, errore = "Impossibile cambiare il proomemoria" }
            };
        }


        //public ActionResult cpratFULL()
        //{
        //    IncentiviEntities db = new IncentiviEntities();
        //    var lista = db.XR_MAT_RICHIESTE.ToList();
        //    string esito = null;
        //    if (lista != null && lista.Any())
        //    {
        //        foreach (var l in lista)
        //        {
        //            string tempEsito = MaternitaCongediManager.cprat(l.ID);

        //            if (!String.IsNullOrEmpty(tempEsito))
        //            {
        //                if (esito == null)
        //                    esito = "";
        //                esito += tempEsito + "\n\r";
        //            }
        //        }
        //    }

        //    if (esito == null)
        //    {
        //        return new JsonResult
        //        {
        //            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //            Data = new { esito = true }
        //        };
        //    }
        //    else
        //    {
        //        return new JsonResult
        //        {
        //            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //            Data = new { esito = false, errore = esito }
        //        };
        //    }
        //}
    }
    public class EsitoInserimentoCongedi
    {
        public List<Congedo> Congedi { get; set; } = new List<Congedo>();
        public String Error { get; set; }
    }
    public class EsitoInserimentoArretrati
    {
        public List<XR_MAT_ARRETRATI_DIPENDENTE> ListArr { get; set; }
        public string Error { get; set; }
    }
    internal class Scadenze
    {
        public int idrich { get; set; }
        public int meseinvio { get; set; }
        public int annoinvio { get; set; }
    }
    public class RisultatiScadenze
    {
        public int mese { get; set; }
        public int anno { get; set; }
        public DateTime? datascadenza { get; set; }
    }
    public class ScadenzeToChange
    {
        public int idrich { get; set; }
        public string dataScadenza { get; set; }
    }
    public static class ExtensionMethods
    {
        //Estensione per LINQ con generics T per tutte le tabelle che prevedono data_inizio e data_fine validita
        public static IEnumerable<T> ValidToday<T>(this IEnumerable<T> source)
        {
            if (source == null) return null;
            if (typeof(T).GetProperty("data_inizio_validita") == null
                || typeof(T).GetProperty("data_fine_validita") == null)
                return source;

            ParameterExpression t = Expression.Parameter(typeof(T), "t");
            Expression comparison = Expression.LessThanOrEqual(
                Expression.Property(t, "data_inizio_validita"),
                    Expression.Constant(DateTime.Now));
            Expression prop2 = Expression.Property(t, "data_fine_validita");
            Expression comparison2 = Expression.Equal(
                Expression.Property(t, "data_fine_validita"),
                    Expression.Constant(null));
            Expression prop3 = Expression.Property(t, "data_fine_validita");
            var converted = Expression.Convert(Expression.Property(t, "data_fine_validita"), typeof(DateTime));
            Expression comparison3 = Expression.GreaterThan(
                Expression.Convert(
                    Expression.Property(t, "data_fine_validita"), typeof(DateTime)), Expression.Constant(DateTime.Now));

            Expression datafine = Expression.OrElse(comparison2, comparison3);
            Expression final = Expression.And(comparison, datafine);

            var e = source.Where(Expression.Lambda<Func<T, bool>>(final, t).Compile());
            return e;
        }


    }
}
