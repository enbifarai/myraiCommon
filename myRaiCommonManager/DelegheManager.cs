using System;
using System.Collections.Generic;
using System.Linq;
using myRaiCommonModel;
using myRaiData;
using System.Data;
using System.Web.Script.Serialization;
using myRaiHelper;
using myRaiServiceHub.Autorizzazioni;
using myRaiData.Incentivi;
using System.Web.Mvc;

namespace myRaiCommonManager
{
    public class DelegheManager
    {
        public static DelegheModel GetDelegheModel()
        {
            List<string> L; 
            DelegheModel model = new DelegheModel();
            Sedi Hrga = new Sedi();
            Hrga.Credentials = CommonHelper.GetUtenteServizioCredentials();

            myRaiServiceHub.Autorizzazioni.Delega deleghe = Hrga.Get_Delega_Net(CommonHelper.GetCurrentUserPMatricola(), "HRUP", "", "", "", "");
            if (deleghe != null && deleghe.DT_Deleghe != null)
                model.ListaDeleghe = deleghe.DT_Deleghe
                                  .AsEnumerable()
                                  .Select(row => new myRaiCommonModel.Delega()
                                  {
                                      Delega_da = Convert.ToDateTime(row.Field<string>("DataInizioValidità")),
                                      Delega_a = Convert.ToDateTime(row.Field<string>("DataScadenzaValidità")),
                                      MatricolaDelegato = row.Field<string>("Matricola"),
                                      Eliminato = (row.Field<int>("Eliminato") == 0 ? false : true),
                                      Funzione = row.Field<string>("Codice_funzione"),
                                      NominativoDelegato = CommonHelper.GetNominativoPerMatricola(row.Field<string>("Matricola"))
                                  })
                                  .ToList();
            return model;
        }
        //public static Delega HaDelegheAttive(string da, string a)
        //{
        //    DateTime date1;
        //    DateTime.TryParseExact(da, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out date1);

        //    DateTime date2;
        //    DateTime.TryParseExact(a, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out date2);


        //    Autorizzazioni.Sedi Hrga = new Autorizzazioni.Sedi();
        //    Hrga.Credentials = CommonHelper.GetUtenteServizioCredentials();

        //    Autorizzazioni.Delega deleghe = Hrga.Get_Delega_Net(CommonHelper.GetCurrentUserPMatricola(), "HRUP", "", "");
        //    if (deleghe != null && deleghe.DT_Deleghe != null)
        //    {
        //        List<Delega>  list= deleghe.DT_Deleghe
        //                              .AsEnumerable()
        //                              .Select(row => new myRaiCommonModel.Delega()
        //                              {
        //                                  Delega_da =Convert.ToDateTime( row.Field<string>("DataInizioValidità")),
        //                                  Delega_a = Convert.ToDateTime(row.Field<string>("DataScadenzaValidità")),
        //                                  MatricolaDelegato = row.Field<string>("Matricola"),
        //                                  Eliminato = (row.Field<int>("Eliminato") == 0 ? false : true),
        //                                  Funzione = row.Field<string>("Codice_funzione"),
        //                                  NominativoDelegato = CommonHelper.GetNominativoPerMatricola(row.Field<string>("Matricola"))
        //                              })
        //                              .ToList();
        //        if (list != null && list.Count > 0)
        //        {
        //            return list.FirstOrDefault(del => (date1 >= del.Delega_da && date1 <= del.Delega_a) || (date2 >= del.Delega_da && date2 <= del.Delega_a));
        //        }
        //    }
        //    return null;
        //}

        public static string SetDelega(PresenzaDipendenti model)
        {
            try
            {
                //Delega d = HaDelegheAttive(model.dataInizioDelega, model.dataFineDelega);
                //if (d != null) 
                //    return "Richiesta in sovrapposizione con delega " + d.Delega_da.ToString("dd/MM/yyyy") + "-" + d.Delega_a.ToString("dd/MM/yyyy") + " a favore di " + d.NominativoDelegato;
                 

                Sedi Hrga = new Sedi();
                Hrga.Credentials = CommonHelper.GetUtenteServizioCredentials();
                foreach (var list in model.ListaDipendenti)
                {
                    foreach (var dip in list.ListaDipendentiPerSede)
                    {
                        if (dip.SelezionatoPerDelega)
                        {
                            Logger.LogAzione(new MyRai_LogAzioni()
                            {
                                applicativo = "PORTALE",
                                data = DateTime.Now,
                                descrizione_operazione = "SetDelega " + CommonHelper.GetCurrentUserPMatricola() + " HRUP " + dip.matricola +
                                model.dataInizioDelega + " " + model.dataFineDelega,
                                matricola = CommonHelper.GetCurrentUserMatricola(),
                                provenienza = "SetDelega",
                                operazione = "SetDelega Request"
                            });

                            myRaiServiceHub.Autorizzazioni.Delega esito = Hrga.Set_Delega_Net(CommonHelper.GetCurrentUserPMatricola(), "HRUP", dip.matricola.Substring(1),
                                model.dataInizioDelega, model.dataFineDelega);

                            string ser = "";
                            try
                            {
                                JavaScriptSerializer j = new JavaScriptSerializer();
                                ser = j.Serialize(esito);
                            }
                            catch
                            {
                            }

                            Logger.LogAzione(new MyRai_LogAzioni()
                            {
                                applicativo = "PORTALE",
                                data = DateTime.Now,
                                descrizione_operazione = ser,
                                matricola = CommonHelper.GetCurrentUserMatricola(),
                                provenienza = "SetDelega",
                                operazione = "SetDelega Response"
                            });
                            if (esito.Cod_Errore != "0")
                                return esito.Descrizione_Errore;

                            NotificheManager.InserisciNotifica("Sei stato delegato da " + UtenteHelper.Nominativo().Trim() +
                               (model.dataInizioDelega == model.dataFineDelega ? " per il giorno " + model.dataInizioDelega
                                : " per il periodo " + model.dataInizioDelega + "-" + model.dataFineDelega), "Delega",
                                dip.matricola.Substring(1),
                                CommonHelper.GetCurrentUserMatricola(),
                                0);
                            CommonHelper.InviaMailDelega(dip.matricola.Substring(1),
                                model.dataInizioDelega, model.dataFineDelega);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "SetDelega"
                });
                return ex.Message;
            }

            return "OK";
        }
        public static string EliminaDelega(string da, string a, string matr)
        {
            try
            {
                Sedi Hrga = new Sedi();
                Hrga.Credentials = CommonHelper.GetUtenteServizioCredentials();

                Logger.LogAzione(new MyRai_LogAzioni()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    descrizione_operazione = "EliminaDelega " + CommonHelper.GetCurrentUserPMatricola() + " HRUP " + matr + " da:" + da + " a:" + a,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "EliminaDelega",
                    operazione = "EliminaDelega Request"
                });

                myRaiServiceHub.Autorizzazioni.Delega esito = Hrga.Del_Delega_Net(CommonHelper.GetCurrentUserPMatricola(), "HRUP", matr, da, a, "");
                string ser = "";
                try
                {
                    JavaScriptSerializer j = new JavaScriptSerializer();
                    ser = j.Serialize(esito);
                }
                catch
                {
                }
                Logger.LogAzione(new MyRai_LogAzioni()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    descrizione_operazione = ser,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "EliminaDelega",
                    operazione = "EliminaDelega Response"
                });
                if (esito.Cod_Errore == "0")
                {
                    CommonHelper.InviaMailDelega(matr, da, a, true);//true:isRevoca
                    return "OK";
                }
                else
                    return esito.Descrizione_Errore;

            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "EliminaDelega"
                });
                return ex.Message;
            }
        }

        /// <summary>
        /// Reperimento delle deleghe che una determinata matricola ha concesso
        /// </summary>
        /// <param name="matricola"></param>
        /// <returns></returns>
        public static List<AbilitazioniPersExt> GetDelegheConcesse(string matricola, List<int> idSottoFunzioni = null)
        {
            List<AbilitazioniPersExt> result = null;

            if (String.IsNullOrEmpty(matricola))
            {
                matricola = CommonHelper.GetCurrentUserMatricola();
            }

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var deleghe = db.XR_HRIS_DELEGHE.Where(w => w.MATRICOLA_DELEGANTE.Equals(matricola) && w.ATTIVA).OrderBy(w => w.DATA_INIZIO).ThenBy(w => w.DATA_FINE).ThenBy(w => w.MATRICOLA_DELEGATO).ToList();
                    if (deleghe != null && deleghe.Any())
                    {
                        result = new List<AbilitazioniPersExt>();
                        foreach(var d in deleghe)
                        {
                            List<XR_HRIS_ABIL> abilitazioni = new List<XR_HRIS_ABIL>();

                            if (idSottoFunzioni == null)
                            {
                                abilitazioni = db.XR_HRIS_ABIL.Where(w => w.ID_DELEGA != null && w.ID_DELEGA == d.ID).ToList();
                            }
                            else
                            {
                                abilitazioni = db.XR_HRIS_ABIL.Where(w => w.ID_DELEGA != null && w.ID_DELEGA == d.ID &&
                                                                        w.ID_SUBFUNZ != null && idSottoFunzioni.Contains(w.ID_SUBFUNZ.Value)).ToList();
                            }
                            
                            result.Add(new AbilitazioniPersExt()
                            {
                                IdDelega = d.ID,
                                InEsercizio = d.ESERCITATA,
                                Descrizione = d.DESCRIZIONE,
                                Matricola = d.MATRICOLA_DELEGATO,
                                Nominativo = CezanneHelper.GetNominativoByMatricola(d.MATRICOLA_DELEGATO),
                                Abilitazioni = abilitazioni
                            });
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "GetDelegheConcesse"
                });
            }

            return result;

        }

        /// <summary>
        /// Reperimento delle deleghe ricevute da altri utenti
        /// </summary>
        /// <param name="matricola"></param>
        /// <returns></returns>
        public static List<AbilitazioniPersExt> GetDelegheRicevute(string matricola, List<int> idSottoFunzioni = null)
        {
            List<AbilitazioniPersExt> result = null;

            if (String.IsNullOrEmpty(matricola))
            {
                matricola = CommonHelper.GetCurrentUserMatricola();
            }

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var deleghe = db.XR_HRIS_DELEGHE.Where(w => w.MATRICOLA_DELEGATO.Equals(matricola) && w.ATTIVA).OrderBy(w => w.DATA_INIZIO).ThenBy(w => w.DATA_FINE).ThenBy(w => w.MATRICOLA_DELEGANTE).ToList();
                    if (deleghe != null && deleghe.Any())
                    {
                        result = new List<AbilitazioniPersExt>();
                        foreach (var d in deleghe)
                        {
                            List<XR_HRIS_ABIL> abilitazioni = new List<XR_HRIS_ABIL>();

                            if (idSottoFunzioni == null)
                            {
                                abilitazioni = db.XR_HRIS_ABIL.Where(w => w.ID_DELEGA != null && w.ID_DELEGA == d.ID).ToList();
                            }
                            else
                            {
                                abilitazioni = db.XR_HRIS_ABIL.Where(w => w.ID_DELEGA != null && w.ID_DELEGA == d.ID &&
                                                                        w.ID_SUBFUNZ != null && idSottoFunzioni.Contains(w.ID_SUBFUNZ.Value)).ToList();
                            }
                            
                            result.Add(new AbilitazioniPersExt()
                            {
                                IdDelega = d.ID,
                                Descrizione = d.DESCRIZIONE,
                                InEsercizio = d.ESERCITATA,
                                Matricola = d.MATRICOLA_DELEGATO,
                                Nominativo = CezanneHelper.GetNominativoByMatricola(d.MATRICOLA_DELEGANTE),
                                Abilitazioni = abilitazioni
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "GetDelegheRicevute"
                });
            }

            return result;

        }

        public static object GetDestinatario(string filter, string value)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = AnagraficaManager.GetDb();

            string abilKey = "DEMA";
            string currentMatr = CommonHelper.GetCurrentUserMatricola();
            //var enabledCat = AuthHelper.EnabledCategory(currentMatr, abilKey);
            //var enabledSer = AuthHelper.EnabledDirection(currentMatr, abilKey);

            DateTime oggi = DateTime.Now;
            var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.Where(x => x.COD_MATLIBROMAT != null && x.DTA_FINE_CR != null && x.DTA_FINE_CR.Value >= DateTime.Today), currentMatr, null, abilKey);
            //var tmpSint = db.SINTESI1.Where(x => x.COD_MATLIBROMAT != null && x.DTA_FINE_CR != null && x.DTA_FINE_CR.Value >= DateTime.Today);
            //if (enabledCat.HasFilter)
            //    tmpSint = tmpSint.Where(x => (enabledCat.CategorieIncluse.Count() == 0 || enabledCat.CategorieIncluse.Any(y => x.COD_QUALIFICA.StartsWith(y)))
            //                                && (enabledCat.CategorieEscluse.Count() == 0 || !enabledCat.CategorieEscluse.Any(y => x.COD_QUALIFICA.StartsWith(y))));

            //if (enabledSer.HasFilter)
            //    tmpSint = tmpSint.Where(x => enabledSer.DirezioniIncluse.Contains(x.COD_SERVIZIO));

            tmpSint = tmpSint.OrderBy(x => x.COD_MATLIBROMAT);

            var tmp = tmpSint.Select(s => new CercaDipendentiItem()
            {
                MATRICOLA = s.COD_MATLIBROMAT,
                NOME = s.DES_NOMEPERS,
                COGNOME = s.DES_COGNOMEPERS,
                SECONDO_COGNOME = s.DES_SECCOGNOME,
                DATA_ASSUNZIONE = s.DTA_INIZIO_CR,
                CONTRATTO = s.DES_TPCNTR,
                ID_PERSONA = s.ID_PERSONA,
                COD_SEDE = s.COD_SEDE,
                DES_SEDE = s.DES_SEDE,
                COD_SERVIZIO = s.COD_SERVIZIO,
                DES_SERVIZIO = s.DES_SERVIZIO,
                SmartWorker = false
            });

            if (tmp != null && tmp.Any())
            {
                foreach (var t in tmp)
                {
                    t.NOME = CommonHelper.ToTitleCase(t.NOME);
                    t.COGNOME = CommonHelper.ToTitleCase(t.COGNOME);
                    t.SECONDO_COGNOME = CommonHelper.ToTitleCase(t.SECONDO_COGNOME);
                }
            }

            if (!string.IsNullOrEmpty(filter))
            {
                var _filteredList = tmp.Where(w => !String.IsNullOrEmpty(w.MATRICOLA) && w.MATRICOLA.Contains(filter)).ToList();



                result.AddRange(_filteredList.ToList().Select(x => new SelectListItem { Value = x.MATRICOLA, Text = "<div class='rai-profile-widget'><div class='rai-profile-info'><span class='rai-font-md-bold'>" + x.MATRICOLA + " - " + x.COGNOME + " " + x.NOME + "</span><br><span class='rai-font-sm'>" + x.DES_SERVIZIO + "</span></div></div>" }));
            }

            if (!string.IsNullOrEmpty(filter))
            {
                var _filteredList = tmp.Where(w => (w.NOME + " " + w.COGNOME).ToUpper().Contains(filter.ToUpper())
                                        || (w.COGNOME + " " + w.NOME).ToUpper().Contains(filter.ToUpper())).ToList();

                result.AddRange(_filteredList.ToList().Select(x => new SelectListItem { Value = x.MATRICOLA, Text = "<div class='rai-profile-widget'><div class='rai-profile-info'><span class='rai-font-md-bold'>" + x.MATRICOLA + " - " + x.COGNOME + " " + x.NOME + "</span><br><span class='rai-font-sm'>" + x.DES_SERVIZIO + "</span></div></div>" }));
            }
            return result;
        }

        public static DelegheResult CreaDelega(DelegaModelVM delega)
        {
            DelegheResult result = new DelegheResult();
            DateTime dataCreazione = DateTime.Now;

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    XR_HRIS_DELEGHE newDelega = new XR_HRIS_DELEGHE()
                    {
                        ATTIVA = true,
                        DATA_CREAZIONE = dataCreazione,
                        DATA_FINE = delega.DataFineDelega,
                        DATA_INIZIO = delega.DataInizioDelega,
                        DESCRIZIONE = delega.NomeDelega,
                        ESERCITATA = true,
                        ID = -1,
                        MATRICOLA_DELEGANTE = delega.MatricolaDelegante,
                        MATRICOLA_DELEGATO = delega.MatricolaDelegato
                    };

                    db.XR_HRIS_DELEGHE.Add(newDelega);

                    db.SaveChanges();

                    foreach (var a in delega.Abilitazioni)
                    {
                        XR_HRIS_ABIL newAbil = new XR_HRIS_ABIL();
                        newAbil.CAT_ESCLUSE = a.CAT_ESCLUSE;
                        newAbil.CAT_INCLUSE = a.CAT_INCLUSE;
                        newAbil.COD_CONDITION = a.COD_CONDITION;
                        newAbil.COD_FUNZIONE = a.COD_FUNZIONE;
                        newAbil.COD_SUBFUNZIONE = a.COD_SUBFUNZIONE;
                        newAbil.CONTR_ESCLUSI = a.CONTR_ESCLUSI;
                        newAbil.CONTR_INCLUSI = a.CONTR_INCLUSI;
                        newAbil.DIR_ESCLUSE = a.DIR_ESCLUSE;
                        newAbil.DIR_INCLUSE = a.DIR_INCLUSE;
                        newAbil.DTA_FINE = delega.DataFineDelega;
                        newAbil.DTA_INIZIO = delega.DataInizioDelega;
                        newAbil.GR_AREA = a.GR_AREA;
                        newAbil.GR_CATEGORIE = a.GR_CATEGORIE;
                        newAbil.ID_MODELLO = a.ID_MODELLO;
                        newAbil.ID_PROFILO = a.ID_PROFILO;
                        newAbil.ID_SUBFUNZ = a.ID_SUBFUNZ;
                        newAbil.IND_ATTIVO = true;
                        newAbil.MATRICOLA = delega.MatricolaDelegato;
                        newAbil.MATRICOLA_DELEGANTE = delega.MatricolaDelegante;
                        newAbil.MATR_ESCLUSE = a.MATR_ESCLUSE;
                        newAbil.MATR_INCLUSE = a.MATR_INCLUSE;
                        newAbil.NOT_NOTE = a.NOT_NOTE;
                        newAbil.SEDI_ESCLUSE = a.SEDI_ESCLUSE;
                        newAbil.SEDI_INCLUSE = a.SEDI_INCLUSE;
                        newAbil.SOC_ESCLUSE = a.SOC_ESCLUSE;
                        newAbil.SOC_INCLUSE = a.SOC_INCLUSE;
                        newAbil.TIP_ESCLUSE = a.TIP_ESCLUSE;
                        newAbil.TIP_INCLUSE = a.TIP_INCLUSE;
                        newAbil.ID_DELEGA = newDelega.ID;
                        newAbil.ID_ABIL = -1;

                        db.XR_HRIS_ABIL.Add(newAbil);

                        db.SaveChanges();

                        // verifica se per questa abilitazione esiste un associazione sulla tabella 
                        // [XR_HRIS_ABIL_ASSOC_MODELLO], in questo caso l'abilitazione ha un modello
                        // e va clonato anche l'associazione tra il modello e la nuova abilitazione

                        var modelli = db.XR_HRIS_ABIL_ASSOC_MODELLO.Where(w => w.ID_ABIL.Equals(a.ID_ABIL)).ToList();
                        if (modelli != null && modelli.Any())
                        {
                            foreach (var m in modelli)
                            {
                                XR_HRIS_ABIL_ASSOC_MODELLO mod = new XR_HRIS_ABIL_ASSOC_MODELLO()
                                {
                                    ID_ABIL = newAbil.ID_ABIL,
                                    ID_MODELLO = m.ID_MODELLO,
                                    ID_ABIL_MODELLO = -1
                                };

                                db.XR_HRIS_ABIL_ASSOC_MODELLO.Add(mod);
                                db.SaveChanges();
                            }
                        }
                    }

                    result.Esito = true;
                }
            }
            catch(Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }
            
            return result;
        }

        public static DelegheResult GetDelega(int idDelega)
        {
            DelegheResult result = new DelegheResult();
            DelegaModelVM model = new DelegaModelVM();

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var item = db.XR_HRIS_DELEGHE.Where(w => w.ID.Equals(idDelega)).FirstOrDefault();

                    if (item == null)
                    {
                        throw new Exception("Delega non trovata");
                    }

                    result.Esito = true;

                    model.NomeDelega = item.DESCRIZIONE;
                    model.DataCreazioneDelega = item.DATA_CREAZIONE;
                    model.DataFineDelega = item.DATA_FINE;
                    model.DataInizioDelega = item.DATA_INIZIO;
                    model.MatricolaDelegante = item.MATRICOLA_DELEGANTE;
                    model.MatricolaDelegato = item.MATRICOLA_DELEGATO;
                    model.NominativoDelegante = CezanneHelper.GetNominativoByMatricola(model.MatricolaDelegante);
                    model.NominativoDelegato = CezanneHelper.GetNominativoByMatricola(model.MatricolaDelegato);

                    List<XR_HRIS_ABIL> abil = db.XR_HRIS_ABIL.Include("XR_HRIS_ABIL_SUBFUNZIONE").Include("XR_HRIS_ABIL_ASSOC_MODELLO").Include("XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE").Where(w => w.ID_DELEGA != null && w.ID_DELEGA == item.ID).ToList();
                    model.Abilitazioni = abil ?? throw new Exception("Abilitazioni non trovate");

                    result.Obj = model;
                }
            }
            catch(Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }

            return result;
        }

        public static DelegheResult DeleteDelega(int idDelega)
        {
            DelegheResult result = new DelegheResult();

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var item = db.XR_HRIS_DELEGHE.Where(w => w.ID.Equals(idDelega)).FirstOrDefault();

                    if (item == null)
                    {
                        throw new Exception("Delega non trovata");
                    }

                    result.Esito = true;
                    item.ATTIVA = false;
                    item.ESERCITATA = false;

                    List<XR_HRIS_ABIL> abil = db.XR_HRIS_ABIL.Where(w => w.ID_DELEGA != null && w.ID_DELEGA == item.ID).ToList();

                    if (abil == null)
                    {
                        throw new Exception("Abilitazioni non trovate");
                    }

                    abil.ForEach(w =>
                    {
                        w.IND_ATTIVO = false;
                    });

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }

            return result;
        }

        public static DelegheResult Esercita(int idDelega, bool attiva = true)
        {
            DelegheResult result = new DelegheResult();

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var item = db.XR_HRIS_DELEGHE.Where(w => w.ID.Equals(idDelega)).FirstOrDefault();

                    if (item == null)
                    {
                        throw new Exception("Delega non trovata");
                    }

                    result.Esito = true;                    
                    item.ESERCITATA = attiva;

                    List<XR_HRIS_ABIL> abil = db.XR_HRIS_ABIL.Where(w => w.ID_DELEGA != null && w.ID_DELEGA == item.ID).ToList();

                    if (abil == null)
                    {
                        throw new Exception("Abilitazioni non trovate");
                    }

                    abil.ForEach(w =>
                    {
                        w.IND_ATTIVO = attiva;
                    });

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }

            return result;
        }

        public static int GetNumeroDelegheConcesse(string matricola)
        {
            int result = 0;

            if (String.IsNullOrEmpty(matricola))
            {
                matricola = CommonHelper.GetCurrentUserMatricola();
            }

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    result = db.XR_HRIS_DELEGHE.Count(w => w.MATRICOLA_DELEGANTE.Equals(matricola) && w.ATTIVA);
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "GetNumeroDelegheConcesse"
                });
            }

            return result;

        }

        public static DelegheResult AlmenoUnaDelega(string cod_Funzione = null)
        {
            DelegheResult result = new DelegheResult();
            result.Obj = false;
            IncentiviEntities db = new IncentiviEntities();
            string matricola = UtenteHelper.Matricola();

            try
            {
                if (!String.IsNullOrEmpty(cod_Funzione))
                {
                    var funzioni = db.XR_HRIS_ABIL_FUNZIONE.Where(w => cod_Funzione == w.COD_FUNZIONE).ToList();

                    if (funzioni != null && funzioni.Any())
                    {
                        List<int> idFunzioni = new List<int>();
                        idFunzioni.AddRange(funzioni.Select(w => w.ID_FUNZIONE).ToList());

                        var sottoFunzioni = db.XR_HRIS_ABIL_SUBFUNZIONE.Where(w => idFunzioni.Contains(w.ID_FUNZIONE)).ToList();

                        if (sottoFunzioni != null && sottoFunzioni.Any())
                        {
                            List<int> idSottoFunzioni = new List<int>();
                            idSottoFunzioni.AddRange(sottoFunzioni.Select(w => w.ID_SUBFUNZ).ToList());

                            bool exists = db.XR_HRIS_ABIL.Count(w => w.MATRICOLA.Equals(matricola) && w.ID_SUBFUNZ != null && idSottoFunzioni.Contains(w.ID_SUBFUNZ.Value) && w.IND_ATTIVO && w.ID_DELEGA != null) > 0;

                            result.Obj = exists;
                            result.Esito = true;
                        }
                    }
                }
                else
                {
                    bool exists = db.XR_HRIS_ABIL.Count(w => w.MATRICOLA.Equals(matricola) && w.IND_ATTIVO && w.ID_DELEGA != null) > 0;
                }
            }
            catch(Exception ex)
            {
                result.Errore = ex.Message;
                result.Obj = false;
            }

            return result;
        }

        public static DelegheResult RecuperaDelegheRicevute(string cod_Funzione = null)
        {
            DelegheResult result = new DelegheResult();
            result.Obj = false;
            IncentiviEntities db = new IncentiviEntities();
            string matricola = UtenteHelper.Matricola();

            try
            {
                if (!String.IsNullOrEmpty(cod_Funzione))
                {
                    var funzioni = db.XR_HRIS_ABIL_FUNZIONE.Where(w => cod_Funzione == w.COD_FUNZIONE).ToList();

                    if (funzioni != null && funzioni.Any())
                    {
                        List<int> idFunzioni = new List<int>();
                        idFunzioni.AddRange(funzioni.Select(w => w.ID_FUNZIONE).ToList());

                        var sottoFunzioni = db.XR_HRIS_ABIL_SUBFUNZIONE.Where(w => idFunzioni.Contains(w.ID_FUNZIONE)).ToList();

                        if (sottoFunzioni != null && sottoFunzioni.Any())
                        {
                            List<int> idSottoFunzioni = new List<int>();
                            idSottoFunzioni.AddRange(sottoFunzioni.Select(w => w.ID_SUBFUNZ).ToList());

                            var abil = db.XR_HRIS_ABIL.Where(w => w.MATRICOLA.Equals(matricola) && w.ID_SUBFUNZ != null && idSottoFunzioni.Contains(w.ID_SUBFUNZ.Value) && w.IND_ATTIVO && w.ID_DELEGA != null).ToList();

                            if (abil != null && abil.Any())
                            {
                                List<int> idDeleghe = new List<int>();
                                idDeleghe.AddRange(abil.Select(w => w.ID_DELEGA.Value).ToList());

                                var d = db.XR_HRIS_DELEGHE.Where(w => idDeleghe.Contains(w.ID)).ToList();
                                if (d != null && d.Any())
                                {
                                   

                                    result.Obj = d;
                                    result.Esito = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    var abil = db.XR_HRIS_ABIL.Where(w => w.MATRICOLA.Equals(matricola) && w.ID_SUBFUNZ != null && w.IND_ATTIVO && w.ID_DELEGA != null).ToList();

                    if (abil != null && abil.Any())
                    {
                        List<int> idDeleghe = new List<int>();
                        idDeleghe.AddRange(abil.Select(w => w.ID_DELEGA.Value).ToList());

                        var d = db.XR_HRIS_DELEGHE.Where(w => idDeleghe.Contains(w.ID)).ToList();
                        if (d != null && d.Any())
                        {
                            result.Obj = d;
                            result.Esito = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errore = ex.Message;
                result.Obj = false;
            }

            return result;
        }
    }
}