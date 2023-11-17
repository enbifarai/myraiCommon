using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using myRai.DataAccess;
using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiHelper;

namespace myRaiCommonManager
{
    public class AnagraficaNoDipManager
    {
        public static bool EnabledToSubFunc(string matricola, string sottofunzione)
        {
            return EnabledToSubFunc(matricola, sottofunzione, out var subFunc);
        }
        public static bool EnabledToSubFunc(string matricola, string sottofunzione, out AbilSubFunc subFunc)
        {
            string funzione = HrisHelper.GetParametro<string>(HrisParam.NDIAbilFunc);
            if (String.IsNullOrWhiteSpace(funzione))
                funzione = "ANAGNODIP";

            string _intSubFunc = sottofunzione;
            if (funzione=="ANAGNODIP")
            {
                switch (sottofunzione)
                {
                    case "NDIVIS":
                        _intSubFunc = "VIS";
                        break;
                    case "NDIGES":
                        _intSubFunc = "GES";
                        break;
                    case "NDIADM":
                        _intSubFunc = "ADM";
                        break;
                    default:
                        break;
                }
            }

            return AuthHelper.EnabledToSubFunc(matricola, funzione, _intSubFunc, out subFunc);
        }

        public static void LoadFromDB2()
        {
            bool forDebug = false;

            if (forDebug)
            {
                List<TNDI_DATI_INDIVIDUALI> anagrafiche = DB2Manager.GetEntities<TNDI_DATI_INDIVIDUALI>("MATRICOLA='Z000155'");

                foreach (var item in anagrafiche)
                {
                    using (IncentiviEntities db = new IncentiviEntities())
                    {
                        CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime timestamp);
                        ANAGPERS dbAnag = db.ANAGPERS.FirstOrDefault(x => x.CSF_CFSPERSONA == item.Codice_fiscale.Trim());
                        if (dbAnag == null)
                            dbAnag = new ANAGPERS()
                            {
                                ID_PERSONA = db.ANAGPERS.GeneraPrimaryKey(9),
                                DES_COGNOMEPERS = item.Cognome.Trim(),
                                DES_NOMEPERS = item.Nome.Trim(),
                                CSF_CFSPERSONA = item.Codice_fiscale.Trim(),
                                COD_SESSO = item.Sesso.Trim(),
                                DTA_NASCITAPERS = item.Data_nascita,
                                COD_CITTA = item.Cod_luogo_nascita.Trim(),
                                COD_PLANNING = "A",
                                IND_DOMICILIO = "N",
                                IND_PATENTE = "N",
                                COD_STCIV = "---",
                                IND_RECAPITO = "N",
                            };
                        XR_NDI_ANAG dbAnagNoDip = new XR_NDI_ANAG()
                        {
                            ID_ANAG = Convert.ToInt32(item.Matricola.Replace("Z", "")),
                            DTA_INIZIO = item.Datini,
                            DTA_FINE = item.Datfin,
                            IND_DIPENDENTE = false,
                        };

                        dbAnagNoDip.COD_USER = codUser;
                        dbAnagNoDip.COD_TERMID = codTermid;
                        dbAnagNoDip.TMS_TIMESTAMP = timestamp;

                        dbAnag.COD_USER = codUser;
                        dbAnag.COD_TERMID = codTermid;
                        dbAnag.TMS_TIMESTAMP = timestamp;

                        dbAnagNoDip.ID_PERSONA = dbAnag.ID_PERSONA;
                        dbAnagNoDip.ANAGPERS = dbAnag;

                        db.XR_NDI_ANAG.Add(dbAnagNoDip);

                        //DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

                        var listRapp = DB2Manager.GetEntities<TNDI_TIPO_RAPPORTO>("Matricola='" + item.Matricola + "'");
                        foreach (var rapp in listRapp)
                        {
                            XR_NDI_TIPO_SOGGETTO dbRec = new XR_NDI_TIPO_SOGGETTO();
                            dbRec.ID_PERSONA = dbAnag.ID_PERSONA;
                            dbRec.DTA_INIZIO = rapp.Datini;
                            dbRec.DTA_FINE = rapp.Datfin;
                            dbRec.COD_TIPO_SOGGETTO = rapp.Tipologia;
                            dbRec.COD_IMPRESA = rapp.Societa.Trim() == "RAI" ? "0" : rapp.Societa.Trim().Substring(rapp.Societa.Trim().Length - 1);
                            dbRec.COD_MAT_COLLEGATA = rapp.Matr_dip_collegata;

                            CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
                            dbRec.COD_USER = codUser;
                            dbRec.COD_TERMID = codTermid;
                            dbRec.TMS_TIMESTAMP = timestamp;

                            db.XR_NDI_TIPO_SOGGETTO.Add(dbRec);
                        }

                        var listResDom = DB2Manager.GetEntities<TNDI_RESIDENZA_DOMICILIO>("Matricola='" + item.Matricola + "'");
                        foreach (var resDom in listResDom)
                        {
                            XR_NDI_RESIDENZA_DOMICILIO newRes = new XR_NDI_RESIDENZA_DOMICILIO();
                            newRes.ID_PERSONA = dbAnag.ID_PERSONA;
                            newRes.DTA_INIZIO = resDom.Datini;
                            newRes.DTA_FINE = resDom.Datfin;
                            newRes.COD_CITTARES = resDom.Res_Cod_Comune.Trim();
                            newRes.DES_INDIRRES = resDom.Res_Indirizzo.Trim();
                            newRes.CAP_CAPRES = resDom.Res_CAP.Trim();
                            newRes.COD_CITTADOM = resDom.Dom_Cod_Comune.Trim();
                            newRes.DES_INDIRDOM = resDom.Dom_Indirizzo.Trim();
                            newRes.CAP_CAPDOM = resDom.Dom_CAP.Trim();

                            CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
                            newRes.COD_USER = codUser;
                            newRes.COD_TERMID = codTermid;
                            newRes.TMS_TIMESTAMP = timestamp;

                            db.XR_NDI_RESIDENZA_DOMICILIO.Add(newRes);
                        }

                        var listIban = DB2Manager.GetEntities<TNDI_IBAN>("Matricola='" + item.Matricola + "'");
                        foreach (var iban in listIban)
                        {
                            XR_DATIBANCARI datiBancari = null;

                            string codIban = iban.IBAN.ToUpper();
                            String codPaese = "";
                            String codChk = "";
                            String codCin = "";
                            String codAbi = "";
                            String codCab = "";
                            String codCC = "";

                            codPaese = codIban.Substring(0, 2);
                            codChk = codIban.Substring(2, 2);
                            codCin = codIban.Substring(4, 1);
                            codAbi = codIban.Substring(5, 5);
                            codCab = codIban.Substring(10, 5);
                            codCC = codIban.Substring(15);

                            datiBancari = new XR_DATIBANCARI()
                            {
                                ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(),
                                ID_PERSONA = dbAnag.ID_PERSONA,
                                DTA_INIZIO = iban.Datini,
                                DTA_FINE = iban.Datfin,
                                COD_TIPOCONTO = "N",
                                DES_INTESTATARIO = dbAnag.DES_COGNOMEPERS + " " + dbAnag.DES_NOMEPERS,
                                COD_IBAN = codIban,
                                COD_ABI = codAbi,
                                COD_CAB = codCab,
                                COD_SUBTPCONTO = "ND",
                                IND_CONGELATO = "N",
                                IND_DELETE = "N",
                                IND_VINCOLATO = "N",
                                IND_CHANGED = "N",
                            };

                            CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
                            datiBancari.COD_USER = codUser;
                            datiBancari.COD_TERMID = codTermid;
                            datiBancari.TMS_TIMESTAMP = timestamp;
                            db.XR_DATIBANCARI.Add(datiBancari);
                        }

                        DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                    }
                }
            }
        }

        public static AnagraficaNoDipModel GetAnagrafica(int idAnag, DateTime? dtaInizio)
        {
            AnagraficaNoDipModel model = new AnagraficaNoDipModel();

            string matricola = CommonHelper.GetCurrentUserMatricola();
            AbilSubFunc subFunc = null;
            bool isAbil = AnagraficaNoDipManager.EnabledToSubFunc(matricola, "NDIADM", out subFunc)
                || AnagraficaNoDipManager.EnabledToSubFunc(matricola, "NDIGEST", out subFunc)
                || AnagraficaNoDipManager.EnabledToSubFunc(matricola, "NDIVIS", out subFunc);

            int inCaricoIdPersona = 0;

            if (idAnag > 0)
            {
                var db = new IncentiviEntities();
                var list = db.XR_NDI_ANAG.Where(x => x.ID_ANAG == idAnag).OrderBy(x => x.DTA_INIZIO).ToList();
                var dbRec = list.FirstOrDefault(x => x.DTA_INIZIO == dtaInizio);
                model = new AnagraficaNoDipModel(dbRec);

                model.Storico.AddRange(list.Select(x => new AnagraficaNoDipModel(x)));

                string abilLock = HrisHelper.GetParametro<string>(HrisParam.AbilLockAnagNoDip);
                if (!String.IsNullOrWhiteSpace(abilLock) && abilLock == "TRUE")
                {
                    var recInCarico = db.XR_WKF_OPERSTATI_GENERIC.FirstOrDefault(x => x.ID_TIPOLOGIA == 31 && x.ID_GESTIONE == idAnag && x.ID_STATO == 0);
                    inCaricoIdPersona = recInCarico != null ? recInCarico.ID_PERSONA : 0;
                    if (recInCarico == null && (subFunc.Create || subFunc.Update || subFunc.Delete))
                    {
                        recInCarico = new XR_WKF_OPERSTATI_GENERIC()
                        {
                            ID_TIPOLOGIA = 31,
                            COD_TIPO_PRATICA = "ANAGNODIP",
                            ID_GESTIONE = idAnag,
                            ID_STATO = 0,
                            ID_PERSONA = CommonHelper.GetCurrentIdPersona(),
                            DTA_OPERAZIONE = 0,
                            VALID_DTA_INI = DateTime.Now,
                            NOMINATIVO = CommonHelper.GetNominativoPerMatricola(matricola)
                        };
                        CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);
                        recInCarico.COD_USER = codUser;
                        recInCarico.COD_TERMID = codTermid;
                        recInCarico.TMS_TIMESTAMP = tms;
                        db.XR_WKF_OPERSTATI_GENERIC.Add(recInCarico);
                        DBHelper.Save(db, matricola);

                        inCaricoIdPersona = CommonHelper.GetCurrentIdPersona();
                    }
                }
            }

            model.IdPersonaInCarico = inCaricoIdPersona;
            model.SetAbil(subFunc.Create, subFunc.Read, subFunc.Update, subFunc.Delete);

            return model;
        }
        public static List<AnagraficaNoDipModel> GetAnagrafiche(AnagNoDipRicerca ricerca)
        {
            RilasciaAnagrafica();

            List<AnagraficaNoDipModel> result = new List<AnagraficaNoDipModel>();

            var db = new IncentiviEntities();
            var list = db.XR_NDI_ANAG
                        .Include("ANAGPERS")
                        .Include("ANAGPERS.TB_COMUNE")
                        .Include("ANAGPERS.XR_NDI_TIPO_SOGGETTO")
                        .Include("ANAGPERS.XR_NDI_TIPO_SOGGETTO.XR_NDI_TB_TIPO_RAPPORTO")
                        .Include("ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO")
                        .Include("ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO.TB_COMUNE_RESIDENZA")
                        .Include("ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO.TB_COMUNE_RESIDENZA.TB_NAZIONE")
                        .Include("ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO.TB_COMUNE_DOMICILIO")
                        .Include("ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO.TB_COMUNE_DOMICILIO.TB_NAZIONE")
                        .Include("ANAGPERS.XR_DATIBANCARI")
                        .AsQueryable();

            if (ricerca.HasFilter)
            {
                if (!String.IsNullOrWhiteSpace(ricerca.Matricola))
                    list = list.Where(x => x.MATRICOLA.StartsWith(ricerca.Matricola));

                if (!String.IsNullOrWhiteSpace(ricerca.Cognome))
                    list = list.Where(x => x.DES_COGNOMEPERS.StartsWith(ricerca.Cognome));

                if (!String.IsNullOrWhiteSpace(ricerca.Nome))
                    list = list.Where(x => x.DES_NOMEPERS.StartsWith(ricerca.Nome));

                if (!String.IsNullOrWhiteSpace(ricerca.CodiceFiscale))
                    list = list.Where(x => x.CSF_CFSPERSONA.StartsWith(ricerca.CodiceFiscale));

                if (!String.IsNullOrWhiteSpace(ricerca.AnnoRapporto))
                {
                    int anno = Convert.ToInt32(ricerca.AnnoRapporto);
                    DateTime ini = new DateTime(anno, 1, 1);
                    DateTime fin = new DateTime(anno, 12, 31);
                    list = list.Where(x => x.ANAGPERS.XR_NDI_TIPO_SOGGETTO.Any(y => ini <= y.DTA_FINE && y.DTA_INIZIO <= fin));
                }

                if (!String.IsNullOrWhiteSpace(ricerca.TipoRapporto))
                    list = list.Where(x => x.ANAGPERS.XR_NDI_TIPO_SOGGETTO.Any(y => y.COD_TIPO_SOGGETTO == ricerca.TipoRapporto));

                if (!String.IsNullOrWhiteSpace(ricerca.MatrCollegata))
                    list = list.Where(x => x.ANAGPERS.XR_NDI_TIPO_SOGGETTO.Any(y => y.COD_MAT_COLLEGATA == ricerca.MatrCollegata));

                if (ricerca.AnnoRiferimento.HasValue)
                    list = list.Where(x => x.ANAGPERS.XR_NDI_TIPO_SOGGETTO.Any(y => y.ANNO_RIFERIMENTO == ricerca.AnnoRiferimento));
            }

            foreach (var dbRec in list.OrderBy(x => x.ID_ANAG).ThenBy(x => x.DTA_INIZIO))
            {
                AnagraficaNoDipModel model = new AnagraficaNoDipModel(dbRec);
                result.Add(model);
            }

            return result;
        }
        public static void RilasciaAnagrafica(int? idAnag = null, bool force = false)
        {
            string abilLock = HrisHelper.GetParametro<string>(HrisParam.AbilLockAnagNoDip);
            if (!String.IsNullOrWhiteSpace(abilLock) && abilLock == "TRUE")
            {
                int idPersona = CommonHelper.GetCurrentIdPersona();
                var db = new IncentiviEntities();
                if (idAnag.HasValue)
                {
                    var tmp = db.XR_WKF_OPERSTATI_GENERIC.Where(x => x.ID_TIPOLOGIA == 31 && x.ID_GESTIONE == idAnag.Value && x.ID_STATO == 0);
                    if (!force || !AnagraficaNoDipManager.EnabledToSubFunc(CommonHelper.GetCurrentUserMatricola(), "NDIADM"))
                        tmp = tmp.Where(x => x.ID_PERSONA == idPersona);

                    var recInCarico = tmp.FirstOrDefault();
                    if (recInCarico != null)
                    {
                        db.XR_WKF_OPERSTATI_GENERIC.Remove(recInCarico);
                        DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                    }
                }
                else
                {
                    var list = db.XR_WKF_OPERSTATI_GENERIC.Where(x => x.ID_TIPOLOGIA == 31 && x.ID_STATO == 0 && x.ID_PERSONA == idPersona);
                    if (list.Any())
                    {
                        foreach (var item in list.ToList())
                        {
                            db.XR_WKF_OPERSTATI_GENERIC.Remove(item);
                        }
                        DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                    }
                }
            }
        }
        public static List<Tuple<int, string, string>> GetLock()
        {
            var result = new List<Tuple<int, string, string>>();

            var db = new IncentiviEntities();
            var tmp = db.XR_WKF_OPERSTATI_GENERIC.Where(x => x.ID_TIPOLOGIA == 31 && x.ID_STATO == 0);
            foreach (var item in tmp)
            {
                var rec = db.XR_NDI_ANAG.Where(x => x.ID_ANAG == item.ID_GESTIONE).Select(x => x.MATRICOLA).FirstOrDefault();
                result.Add(new Tuple<int, string, string>(item.ID_GESTIONE, rec, item.NOMINATIVO.TitleCase()));
            }

            return result;
        }

        public static bool Save_Anagrafica(AnagraficaNoDipModel model, out int idAnag, out string matricola, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";
            matricola = "";
            idAnag = 0;

            if (!Regex.IsMatch(model.DatiAnagrafici.Cognome, @"^[A-Z0-9\.\s'\/\\-]+$"))
            {
                errorMsg = "Il cognome contiene dei caratteri non ammessi.";
                return result;
            }

            if (!Regex.IsMatch(model.DatiAnagrafici.Nome, @"^[A-Z0-9\.\s'\/\\-]+$"))
            {
                errorMsg = "Il nome contiene dei caratteri non ammessi.";
                return result;
            }

            var db = new IncentiviEntities();
            XR_NDI_ANAG dbAnagNoDip = null;
            ANAGPERS dbAnag = null;

            if (model.IdAnag == 0)
            {
                dbAnagNoDip = new XR_NDI_ANAG();
                dbAnag = new ANAGPERS();

                if (db.XR_NDI_ANAG.Any(x => x.CSF_CFSPERSONA == model.DatiAnagrafici.CodiceFiscale))
                {
                    errorMsg = "Anagrafica già presente";
                    return result;
                }
            }
            else
            {
                dbAnagNoDip = db.XR_NDI_ANAG.Find(model.IdAnag, model.OrigDataInizio);
                dbAnag = dbAnagNoDip.ANAGPERS;
            }

            if (model.IdAnag > 0)
            {
                bool sovrapposto = db.XR_NDI_ANAG.Any(x => x.ID_ANAG == model.IdAnag && x.DTA_INIZIO != model.OrigDataInizio && x.DTA_INIZIO <= model.AnagDataFine && model.AnagDataInizio <= x.DTA_FINE);
                if (sovrapposto)
                {
                    errorMsg = "Il periodo si sovrappone a uno esistente";
                    return result;
                }
            }

            bool keyModified = model.IdAnag != 0 && dbAnagNoDip.DTA_INIZIO != model.AnagDataInizio;
            if (keyModified)
            {
                db.XR_NDI_ANAG.Remove(dbAnagNoDip);
                dbAnagNoDip = new XR_NDI_ANAG()
                {
                    ID_ANAG = model.IdAnag,
                    ID_PERSONA = model.IdPersona,
                    IND_DIPENDENTE = model.IsDipendente
                };
            }

            dbAnagNoDip.DES_COGNOMEPERS = model.DatiAnagrafici.Cognome;
            dbAnagNoDip.DES_NOMEPERS = model.DatiAnagrafici.Nome;
            dbAnagNoDip.CSF_CFSPERSONA = model.DatiAnagrafici.CodiceFiscale;
            dbAnagNoDip.COD_SESSO = model.DatiAnagrafici.Sesso;
            dbAnagNoDip.DTA_NASCITAPERS = model.DatiAnagrafici.DataNascita;
            dbAnagNoDip.COD_CITTA = model.DatiAnagrafici.CodLuogoNascita;
            dbAnagNoDip.IND_MANCF = model.IndForzaCF;
            dbAnagNoDip.DTA_INIZIO = model.AnagDataInizio;
            DateTime oldFine = dbAnagNoDip.DTA_FINE;
            dbAnagNoDip.DTA_FINE = model.AnagDataFine;
            if (model.IdAnag == 0)
                dbAnagNoDip.IND_DIPENDENTE = model.IsDipendente;

            bool notMarkAsDip = false;
            bool isDip = false;

            if (!model.IsDipendente)
            {
                //fa un controllo se esiste già l'anagrafica come dipendente
                dbAnag = db.ANAGPERS.FirstOrDefault(x => x.CSF_CFSPERSONA == model.DatiAnagrafici.CodiceFiscale);
                if (dbAnag == null)
                {
                    //In questo caso è un nuovo inserimento
                    dbAnag = new ANAGPERS();
                    dbAnag.DES_COGNOMEPERS = model.DatiAnagrafici.Cognome;
                    dbAnag.DES_NOMEPERS = model.DatiAnagrafici.Nome;
                    dbAnag.CSF_CFSPERSONA = model.DatiAnagrafici.CodiceFiscale;
                    dbAnag.COD_SESSO = model.DatiAnagrafici.Sesso;
                    dbAnag.DTA_NASCITAPERS = model.DatiAnagrafici.DataNascita;
                    dbAnag.COD_CITTA = model.DatiAnagrafici.CodLuogoNascita;
                    dbAnag.COD_PLANNING = "A";
                    dbAnag.IND_DOMICILIO = "N";
                    dbAnag.IND_PATENTE = "N";
                    dbAnag.COD_STCIV = "---";
                    dbAnag.IND_RECAPITO = "N";
                    dbAnag.ID_PERSONA = db.ANAGPERS.GeneraPrimaryKey(9);
                }
                else if (dbAnag.COMPREL != null && dbAnag.COMPREL.Any(x => x.COD_MATLIBROMAT != null))
                    //Questo è il record di un dipendente, l'aggiornamento influisce solo sulle XR_NDI_*
                    //Capito in questo ramo se in fase di creazione non ho selezionato il check di un dipendente
                    //tuttavia ne ho inserito i dati
                    //Quindi crea l'associazione per CF, ma non ne deve modificare assolutamente i dati
                    isDip = true;
                else
                {
                    var lastRec = db.XR_NDI_ANAG.Where(x => x.ID_ANAG == model.IdAnag).Select(x => x.DTA_INIZIO).Max();
                    if (lastRec == model.AnagDataInizio)
                    {
                        //esiste l'anagrafica, non è un dipendente, perciò lo aggiorno
                        dbAnag.DES_COGNOMEPERS = model.DatiAnagrafici.Cognome;
                        dbAnag.DES_NOMEPERS = model.DatiAnagrafici.Nome;
                        dbAnag.CSF_CFSPERSONA = model.DatiAnagrafici.CodiceFiscale;
                        dbAnag.COD_SESSO = model.DatiAnagrafici.Sesso;
                        dbAnag.DTA_NASCITAPERS = model.DatiAnagrafici.DataNascita;
                        dbAnag.COD_CITTA = model.DatiAnagrafici.CodLuogoNascita;
                    }
                }
            }
            else
            {
                dbAnag = db.ANAGPERS.Find(model.IdPersona);
                isDip = true;
            }

            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime timestamp);
            dbAnagNoDip.COD_USER = codUser;
            dbAnagNoDip.COD_TERMID = codTermid;
            dbAnagNoDip.TMS_TIMESTAMP = timestamp;

            if (!isDip)
            {
                dbAnag.COD_USER = codUser;
                dbAnag.COD_TERMID = codTermid;
                dbAnag.TMS_TIMESTAMP = timestamp;
            }

            if (keyModified)
                db.XR_NDI_ANAG.Add(dbAnagNoDip);

            TNDI_DATI_INDIVIDUALI origIndiv = null;
            TNDI_DATI_INDIVIDUALI datiIndiv = null;
            if (model.IdAnag == 0)
            {
                List<int> ids = DB2Manager.SqlQuery<int>("select top 1 convert(int, REPLACE(MATRICOLA,'Z','')) from OPENQUERY(DB2LINK, 'select MATRICOLA from " + DB2Manager.GetPrefixTable() + ".TNDI_DATI_INDIVIDUALI') order by MATRICOLA DESC");
                int id = ids.FirstOrDefault();


                dbAnagNoDip.ID_ANAG = id + 1;
                dbAnagNoDip.ANAGPERS = dbAnag;
                db.XR_NDI_ANAG.Add(dbAnagNoDip);
            }
            else
                datiIndiv = DB2Manager.GetEntity<TNDI_DATI_INDIVIDUALI>("MATRICOLA='" + model.DatiAnagrafici.Matricola + "' and DATINI='" + model.OrigDataInizio.ToString("yyyy-MM-dd") + "'");

            origIndiv = datiIndiv.Copy();

            bool isNew = datiIndiv == null || origIndiv.Datini != model.AnagDataInizio;
            if (isNew)
                datiIndiv = new TNDI_DATI_INDIVIDUALI();

            datiIndiv.Matricola = datiIndiv.Matricola ?? "Z" + (dbAnagNoDip.ID_ANAG.ToString()).PadLeft(6, '0');
            datiIndiv.Cognome = model.DatiAnagrafici.Cognome;
            datiIndiv.Nome = model.DatiAnagrafici.Nome;
            datiIndiv.Codice_fiscale = model.DatiAnagrafici.CodiceFiscale;
            datiIndiv.Sesso = model.DatiAnagrafici.Sesso;
            datiIndiv.Data_nascita = model.DatiAnagrafici.DataNascita;
            datiIndiv.Cod_luogo_nascita = model.DatiAnagrafici.CodLuogoNascita;
            datiIndiv.Datini = model.AnagDataInizio;
            datiIndiv.Datfin = model.AnagDataFine;


            XR_NDI_ANAG newAnagNoDip = null;
            TNDI_DATI_INDIVIDUALI newDatiIndiv = null;
            if (model.AnagNewDataInizio.HasValue)
            {
                //In questo caso ho chiesto di definire una nuova posizione, quindi bisogna inserire un nuovo record
                newAnagNoDip = dbAnagNoDip.Copy();
                newAnagNoDip.DTA_INIZIO = model.AnagNewDataInizio.Value;
                newAnagNoDip.DTA_FINE = oldFine;
                db.XR_NDI_ANAG.Add(newAnagNoDip);

                newDatiIndiv = new TNDI_DATI_INDIVIDUALI();
                newDatiIndiv.Matricola = newAnagNoDip.MATRICOLA;
                newDatiIndiv.Cognome = newAnagNoDip.DES_COGNOMEPERS;
                newDatiIndiv.Nome = newAnagNoDip.DES_NOMEPERS;
                newDatiIndiv.Codice_fiscale = newAnagNoDip.CSF_CFSPERSONA;
                newDatiIndiv.Sesso = newAnagNoDip.COD_SESSO;
                newDatiIndiv.Data_nascita = newAnagNoDip.DTA_NASCITAPERS;
                newDatiIndiv.Cod_luogo_nascita = newAnagNoDip.COD_CITTA;
                newDatiIndiv.Datini = newAnagNoDip.DTA_INIZIO;
                newDatiIndiv.Datfin = newAnagNoDip.DTA_FINE;
            }

            //Salvataggio su DB2
            if (isNew || keyModified)
                result = DB2Manager.InsertEntity(datiIndiv);
            else
                result = DB2Manager.UpdateEntity(datiIndiv);

            if (result && keyModified)
            {
                result = DB2Manager.DeleteEntity(origIndiv);
                if (!result)
                {
                    if (!DB2Manager.DeleteEntity(datiIndiv))
                    {
                        string message = String.Format($"Cancellazione record con vecchia data inizio su DB2 non riuscita. Cancelllazione record nuova data inizio non riuscito");
                        message += "\nOrig:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(origIndiv);
                        message += "\nNuovi dati:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(datiIndiv);
                        HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                    }
                }
            }

            if (result && model.AnagNewDataInizio.HasValue)
            {
                result = DB2Manager.InsertEntity(newDatiIndiv);
                if (!result)
                {
                    if (!DB2Manager.UpdateEntity(origIndiv))
                    {
                        string message = String.Format($"Inserimento nuova posizione su DB2 non riuscita. Ripristino vecchia posizione non riuscita");
                        if (!isNew)
                            message += "\nOrig:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(origIndiv);
                        message += "\nNuovi dati:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(datiIndiv);
                        HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                    }
                }
            }

            if (result)
            {
                if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                {
                    result = true;
                    if (notMarkAsDip)
                        errorMsg = String.Format($"{datiIndiv.Cognome} {datiIndiv.Nome} risulta essere un dipendente, pertanto sono stati importati i dati anagrafici esistenti");
                }
                else
                {
                    errorMsg = "Errore durante il salvataggio";

                    bool restore = false;
                    if (isNew)
                        restore = DB2Manager.DeleteEntity(datiIndiv);
                    else
                        restore = DB2Manager.UpdateEntity(origIndiv);

                    if (!restore)
                    {
                        string message = String.Format($"{(isNew ? "Inserimento" : "Modifica")} dati anagrafici su SQL non riuscito. Ripristino dati DB2 non riuscito.");
                        if (!isNew)
                            message += "\nOrig:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(origIndiv);
                        message += "\nNuovi dati:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(datiIndiv);
                        HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                    }
                    else if (!DB2Manager.DeleteEntity(newDatiIndiv))
                    {
                        string message = String.Format($"{(isNew ? "Inserimento" : "Modifica")} dati anagrafici su SQL non riuscito. Cancellazione nuova posizione DB2 non riuscito.");
                        if (!isNew)
                            message += "\nOrig:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(origIndiv);
                        message += "\nNuovi dati:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(datiIndiv);
                        HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                    }
                }
            }
            else
                errorMsg = "Errore durante il salvataggio su DB2";

            idAnag = dbAnagNoDip.ID_ANAG;
            matricola = dbAnagNoDip.MATRICOLA;

            return result;
        }
        public static bool Delete_Anagrafica(int idAnag, DateTime dtaInizio, out string errorMsg)
        {
            bool result = false;
            errorMsg = null;

            var db = new IncentiviEntities();
            var list = db.XR_NDI_ANAG.Where(x => x.ID_ANAG == idAnag);
            var anag = list.FirstOrDefault(x => x.DTA_INIZIO == dtaInizio);
            var anagPers = anag.ANAGPERS;

            //Se è l'ultimo record, bisogna pulire tutti i dati associati
            if (list.Count() == 1)
            {
                //Iban
                if (anag.ANAGPERS.XR_DATIBANCARI != null)
                {
                    var listIban = anag.ANAGPERS.XR_DATIBANCARI.Where(x => x.COD_TIPOCONTO == "N").ToList();
                    foreach (var item in listIban)
                    {
                        if (!Delete_Iban(anag.ID_ANAG, item.ID_XR_DATIBANCARI, out errorMsg))
                            return false;
                    }
                }

                //Indirizzi
                if (anag.ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO != null)
                {
                    var listInd = anag.ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO.ToList();
                    foreach (var item in listInd)
                    {
                        if (!Delete_Indirizzo(anag.ID_ANAG, item.DTA_INIZIO, out errorMsg))
                            return false;
                    }
                }

                //Rapporto
                if (anag.ANAGPERS.XR_NDI_TIPO_SOGGETTO != null)
                {
                    var listRapp = anag.ANAGPERS.XR_NDI_TIPO_SOGGETTO.ToList();
                    foreach (var item in listRapp)
                    {
                        if (!Delete_Rapporto(anag.ID_ANAG, item.COD_TIPO_SOGGETTO, item.COD_IMPRESA, item.DTA_INIZIO, out errorMsg))
                            return false;
                    }
                }
            }

            TNDI_DATI_INDIVIDUALI datiIndiv = DB2Manager.GetEntity<TNDI_DATI_INDIVIDUALI>("MATRICOLA='" + anag.MATRICOLA + "' and DATINI='" + anag.DTA_INIZIO.ToString("yyyy-MM-dd") + "'");
            if (datiIndiv != null)
            {
                if (!DB2Manager.DeleteEntity(datiIndiv))
                {
                    errorMsg = "Errore durante la cancellazione DB2";
                    return false;
                }
            }


            //Controllo la comprel per maggior sicurezza
            bool updateAnagpers = anagPers.COMPREL == null || !anagPers.COMPREL.Any();
            if (list.Count() == 1 && updateAnagpers)
                db.ANAGPERS.Remove(anagPers);
            else if (updateAnagpers)
            {
                var lastAnag = list.Where(x => x.DTA_INIZIO != dtaInizio).OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();
                anagPers.DES_COGNOMEPERS = lastAnag.DES_COGNOMEPERS;
                anagPers.DES_NOMEPERS = lastAnag.DES_NOMEPERS;
                anagPers.CSF_CFSPERSONA = lastAnag.CSF_CFSPERSONA;
                anagPers.COD_SESSO = lastAnag.COD_SESSO;
                anagPers.DTA_NASCITAPERS = lastAnag.DTA_NASCITAPERS;
                anagPers.COD_CITTA = lastAnag.COD_CITTA;
            }
            
            db.XR_NDI_ANAG.Remove(anag);
            
            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            if (!result)
            {
                errorMsg = "Errore durante il salvataggio";

                if (datiIndiv != null && !DB2Manager.InsertEntity(datiIndiv))
                {
                    string message = String.Format($"Cancellazione dati anagrafici su SQL non riuscito. Ripristino dati DB2 non riuscito.");
                    message += "\nDa ripristinare su DB2:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(datiIndiv);
                    HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                }
            }


            return result;
        }

        public static AnagNoDipRapportoModel GetRapporto(int idAnag, string codTipoSogg, string codImpresa, DateTime? dataInizio)
        {
            AnagNoDipRapportoModel model = new AnagNoDipRapportoModel();

            IncentiviEntities db = new IncentiviEntities();
            XR_NDI_ANAG anag = db.XR_NDI_ANAG.Where(x => x.ID_ANAG == idAnag).OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();

            if (!String.IsNullOrWhiteSpace(codTipoSogg) && !String.IsNullOrWhiteSpace(codImpresa) && dataInizio.HasValue)
            {
                XR_NDI_TIPO_SOGGETTO rapporto = db.XR_NDI_TIPO_SOGGETTO.Find(anag.ID_PERSONA, codTipoSogg, codImpresa, dataInizio.Value);
                model = new AnagNoDipRapportoModel(rapporto);
            }
            else
            {
                model.IdPersona = anag.ID_PERSONA;
                model.RappDataInizio = DateTime.Today;
                model.RappDataFine = model.RappDataInizio.AddDays(1);
            }
            model.Matricola = anag.MATRICOLA;

            model.IdAnag = idAnag;

            return model;
        }

        public static AnagNoDipRapporti GetRapporti(int idAnag)
        {
            AnagNoDipRapporti model = new AnagNoDipRapporti();

            IncentiviEntities db = new IncentiviEntities();
            XR_NDI_ANAG anag = db.XR_NDI_ANAG.FirstOrDefault(x => x.ID_ANAG == idAnag);

            foreach (var item in anag.ANAGPERS.XR_NDI_TIPO_SOGGETTO)
            {
                model.Elenco.Add(new AnagNoDipRapportoModel(item));
            }

            return model;
        }

        public static bool Save_Rapporto(AnagNoDipRapportoModel model, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();

            if (model.Codice == "ER")
            {
                bool existErede = db.XR_NDI_TIPO_SOGGETTO.Any(x => x.ID_PERSONA != model.IdPersona
                                                                    && x.COD_TIPO_SOGGETTO == model.Codice
                                                                    && x.COD_MAT_COLLEGATA == model.MatricolaCollegata
                                                                    && x.NUM_EREDE == model.NumErede
                                                                    && x.DTA_INIZIO == model.RappDataInizio
                                                                    && x.DTA_FINE == model.RappDataFine);

                if (existErede)
                {
                    errorMsg = "Esiste già un erede " + model.NumErede + " collegato alla matricola " + model.MatricolaCollegata;
                    return result;
                }
            }

            XR_NDI_TIPO_SOGGETTO dbRec, oldDBRec = null;
            if (model.IsNew)
            {
                dbRec = new XR_NDI_TIPO_SOGGETTO();
                dbRec.ID_PERSONA = model.IdPersona;
                dbRec.DTA_INIZIO = model.RappDataInizio;
                dbRec.COD_TIPO_SOGGETTO = model.Codice;
                dbRec.COD_IMPRESA = model.Societa;
            }
            else
                dbRec = db.XR_NDI_TIPO_SOGGETTO.FirstOrDefault(x => x.ID_PERSONA == model.IdPersona && x.COD_TIPO_SOGGETTO == model.OrigCodice && x.DTA_INIZIO == model.OrigRappDataInizio && x.COD_IMPRESA == model.OrigSocieta);

            bool keyModified = false;
            if (!model.IsNew && (dbRec.COD_TIPO_SOGGETTO != model.Codice || dbRec.DTA_INIZIO != model.RappDataInizio || dbRec.COD_IMPRESA != model.Societa))
            {
                keyModified = true;
                db.XR_NDI_TIPO_SOGGETTO.Remove(dbRec);
                dbRec = new XR_NDI_TIPO_SOGGETTO();
                dbRec.ID_PERSONA = model.IdPersona;
                dbRec.DTA_INIZIO = model.RappDataInizio;
                dbRec.COD_TIPO_SOGGETTO = model.Codice;
                dbRec.COD_IMPRESA = model.Societa;
            }

            dbRec.DTA_FINE = model.RappDataFine;

            XR_NDI_TB_TIPO_RAPPORTO tipoRapporto = db.XR_NDI_TB_TIPO_RAPPORTO.Find(model.Codice);
            if (tipoRapporto.IND_MATR_COLLEGATA)
                dbRec.COD_MAT_COLLEGATA = model.MatricolaCollegata;
            else
                dbRec.COD_MAT_COLLEGATA = "";

            if (dbRec.COD_TIPO_SOGGETTO == "ER")
                dbRec.NUM_EREDE = model.NumErede;
            else
                dbRec.NUM_EREDE = "";

            if (tipoRapporto.IND_IMPORTO_REDDITO)
                dbRec.IMPORTO_REDDITO = model.ImportoReddito;
            else
                dbRec.IMPORTO_REDDITO = 0;

            dbRec.ANNO_RIFERIMENTO = model.RappAnnoRiferimento;

            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tmsTimestamp);
            dbRec.COD_USER = codUser;
            dbRec.COD_TERMID = codTermid;
            dbRec.TMS_TIMESTAMP = tmsTimestamp;

            if (model.IsNew || keyModified)
                db.XR_NDI_TIPO_SOGGETTO.Add(dbRec);

            //Su XR_HRIS_PARAM c'è un parametro per gestire la decodifica Cezanne-AnagNoDip_su_DB2
            List<HrisMapSocieta> paramSoc = HrisHelper.GetParametriJson<HrisMapSocieta>(HrisParam.DecodSocietaStrOrg);
            string societa = paramSoc.FirstOrDefault(x => x.Cezanne == model.Societa && x.Stato == 1).NoDip;
            string origSocieta = model.IsNew ? "" : paramSoc.FirstOrDefault(x => x.Cezanne == model.OrigSocieta && x.Stato == 1).NoDip;

            //Salvataggio su DB2
            TNDI_TIPO_RAPPORTO rapporto = null;
            if (!model.IsNew)
                rapporto = DB2Manager.GetEntity<TNDI_TIPO_RAPPORTO>(String.Format("MATRICOLA='{0}' and TIPOLOGIA='{1}' and DATINI='{2:yyyy-MM-dd}' and Societa='{3}'", model.Matricola, model.OrigCodice, model.OrigRappDataInizio, origSocieta));

            TNDI_TIPO_RAPPORTO origRappo = rapporto.Copy();
            bool isNew = rapporto == null;
            if (isNew || keyModified)
                rapporto = new TNDI_TIPO_RAPPORTO();

            rapporto.Matricola = model.Matricola;
            rapporto.Societa = societa;
            rapporto.Tipologia = model.Codice;
            rapporto.Datini = model.RappDataInizio;
            rapporto.Datfin = model.RappDataFine;
            if (tipoRapporto.IND_MATR_COLLEGATA)
                rapporto.Matr_dip_collegata = !String.IsNullOrWhiteSpace(model.MatricolaCollegata) ? model.MatricolaCollegata.PadLeft(7, '0') : "";
            else
                rapporto.Matr_dip_collegata = "";

            if (tipoRapporto.COD_TIPOLOGIA == "ER")
                rapporto.Num_Erede = model.NumErede;
            else
                rapporto.Num_Erede = "";

            if (tipoRapporto.IND_IMPORTO_REDDITO)
                rapporto.Importo_reddito = model.ImportoReddito;
            else
                rapporto.Importo_reddito = 0;

            rapporto.Anno_riferimento = model.RappAnnoRiferimento.GetValueOrDefault();


            if (isNew || keyModified)
                result = DB2Manager.InsertEntity(rapporto);
            else
                result = DB2Manager.UpdateEntity(rapporto);

            if (result)
            {
                if (keyModified)
                {
                    result = DB2Manager.DeleteEntity(origRappo);
                    if (!result)
                    {
                        errorMsg = "Errore durante l'aggiornamento su DB2";

                        if (DB2Manager.DeleteEntity(rapporto))
                        {
                            string message = String.Format($"Cancellazione record originale rapporto lavorativo su SQL non riuscito. Ripristino dati DB2 non riuscito.");
                            HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                        }
                    }
                }

                if (result)
                {
                    result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                    if (!result)
                    {
                        errorMsg = "Errore durante il salvataggio";

                        bool restore = false;
                        if (isNew || keyModified)
                            restore = DB2Manager.DeleteEntity(rapporto);
                        else
                            restore = DB2Manager.UpdateEntity(origRappo);

                        if (!restore)
                        {
                            string message = String.Format($"{(isNew ? "Inserimento" : "Modifica")} dati rapporto lavorativo su SQL non riuscito. Ripristino dati DB2 non riuscito.");
                            if (!isNew)
                                message += "\nOrig:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(origRappo);
                            message += "\nNuovi dati:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(rapporto);

                            HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                        }
                        else if (keyModified)
                        {
                            if (!DB2Manager.InsertEntity(origRappo))
                            {
                                string message = String.Format($"{(isNew ? "Inserimento" : "Modifica")} dati rapporto lavorativo su SQL non riuscito. Ripristino dati DB2 non riuscito.");
                                if (!isNew)
                                    message += "\nOrig:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(origRappo);
                                message += "\nNuovi dati:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(rapporto);

                                HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                            }
                        }
                    }
                }
            }
            else errorMsg = "Errore durante il salvataggio su DB2";

            return result;
        }
        public static bool Delete_Rapporto(int idAnag, string codTipoSogg, string codImpresa, DateTime dataInizio, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            IncentiviEntities db = new IncentiviEntities();
            XR_NDI_ANAG anag = db.XR_NDI_ANAG.Where(x => x.ID_ANAG == idAnag).OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();
            XR_NDI_TIPO_SOGGETTO rapporto = db.XR_NDI_TIPO_SOGGETTO.Find(anag.ID_PERSONA, codTipoSogg, codImpresa, dataInizio);
            if (rapporto != null)
            {
                TNDI_TIPO_RAPPORTO db2Rapporto = null;
                List<HrisMapSocieta> paramSoc = HrisHelper.GetParametriJson<HrisMapSocieta>(HrisParam.DecodSocietaStrOrg);
                string societa = paramSoc.FirstOrDefault(x => x.Cezanne == codImpresa && x.Stato == 1).NoDip;
                db2Rapporto = DB2Manager.GetEntity<TNDI_TIPO_RAPPORTO>(String.Format("MATRICOLA='{0}' and TIPOLOGIA='{1}' and DATINI='{2:yyyy-MM-dd}' and Societa='{3}'", anag.MATRICOLA, codTipoSogg, dataInizio, societa));

                if (db2Rapporto != null)
                {
                    if (!DB2Manager.DeleteEntity(db2Rapporto))
                    {
                        errorMsg = "Errore durante la cancellazione del rapporto DB2";
                        return false;
                    }
                }

                db.XR_NDI_TIPO_SOGGETTO.Remove(rapporto);
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                {
                    if (!DB2Manager.InsertEntity(db2Rapporto))
                    {
                        string message = String.Format($"Cancellazione dati rapporto lavorativo su SQL non riuscito. Ripristino dati DB2 non riuscito.");
                        message += "\nDa ripristinare su DB2:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(db2Rapporto);
                        HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                    }
                }
            }
            else
                errorMsg = "Rapporto non trovato";

            return result;
        }


        public static AnagNoDipIbanModel GetIban(int idAnag, int idIban)
        {
            AnagNoDipIbanModel model = new AnagNoDipIbanModel();
            var db = new IncentiviEntities();
            var anag = db.XR_NDI_ANAG.Where(x => x.ID_ANAG == idAnag).OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();

            if (idIban > 0)
            {
                XR_DATIBANCARI dbRec = db.XR_DATIBANCARI.Find(idIban);
                model = new AnagNoDipIbanModel(dbRec);
            }
            else
            {
                model.IdPersona = anag.ID_PERSONA;
                model.DataInizio = DateTime.Today;
                model.DataFine = AnagraficaManager.GetDateLimitMax();
            }

            model.IdAnag = idAnag;
            model.Matricola = anag.MATRICOLA;

            return model;
        }
        public static List<AnagNoDipIbanModel> GetElencoIban(int idAnag)
        {
            List<AnagNoDipIbanModel> model = new List<AnagNoDipIbanModel>();

            var db = new IncentiviEntities();
            var anag = db.XR_NDI_ANAG.Where(x => x.ID_ANAG == idAnag).OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();
            if (anag.ANAGPERS.XR_DATIBANCARI != null)
            {
                foreach (var item in anag.ANAGPERS.XR_DATIBANCARI.Where(x => x.COD_TIPOCONTO == "N"))
                {
                    var tmp = new AnagNoDipIbanModel(item);
                    tmp.IdPersona = anag.ID_PERSONA;
                    tmp.IdAnag = idAnag;
                    model.Add(tmp);
                }
            }

            return model;
        }
        public static bool Save_Iban(AnagNoDipIbanModel model, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            XR_DATIBANCARI oldDatiBancari = null;
            XR_DATIBANCARI datiBancari = null;

            TNDI_IBAN oldDB2Iban = null;
            TNDI_IBAN newDB2Iban = null;

            string codIban = model.IBAN.ToUpper();
            String codPaese = "";
            String codChk = "";
            String codCin = "";
            String codAbi = "";
            String codCab = "";
            String codCC = "";

            codPaese = codIban.Substring(0, 2);
            codChk = codIban.Substring(2, 2);
            codCin = codIban.Substring(4, 1);
            codAbi = codIban.Substring(5, 5);
            codCab = codIban.Substring(10, 5);
            codCC = codIban.Substring(15);

            if (model.IdDatiBancari != 0)
            {
                oldDatiBancari = db.XR_DATIBANCARI.FirstOrDefault(x => x.ID_XR_DATIBANCARI == model.IdDatiBancari);
                oldDB2Iban = DB2Manager.GetEntity<TNDI_IBAN>(String.Format("Matricola='{0}' and Datini = '{1:yyyy-MM-dd}'", model.Matricola, model.DataInizio));
            }
            else
            {
                oldDatiBancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == model.IdPersona).OrderByDescending(x => x.DTA_INIZIO).ThenByDescending(x => x.DTA_FINE).FirstOrDefault();
                oldDB2Iban = DB2Manager.GetEntities<TNDI_IBAN>(String.Format("Matricola='{0}'", model.Matricola)).OrderByDescending(x => x.Datini).ThenByDescending(x => x.Datfin).FirstOrDefault();
            }

            TNDI_IBAN origDB2Iban = oldDB2Iban.Copy();

            //Aggiungere controllo 
            if (oldDatiBancari != null)
                oldDatiBancari.DTA_FINE = model.DataInizio.AddDays(-1);
            if (oldDB2Iban != null)
                oldDB2Iban.Datfin = model.DataInizio.AddDays(-1);

            datiBancari = new XR_DATIBANCARI()
            {
                ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(),
                ID_PERSONA = model.IdPersona,
                DTA_INIZIO = model.DataInizio,
                DTA_FINE = model.DataFine,
                COD_TIPOCONTO = "N",
                DES_INTESTATARIO = model.Intestatario,
                COD_IBAN = codIban,
                COD_ABI = codAbi,
                COD_CAB = codCab,
                COD_SUBTPCONTO = "ND",
                IND_CONGELATO = "N",
                IND_DELETE = "N",
                IND_VINCOLATO = "N",
                IND_CHANGED = "N",
            };
            newDB2Iban = new TNDI_IBAN()
            {
                Matricola = model.Matricola,
                Datini = model.DataInizio,
                Datfin = model.DataFine,
                IBAN = model.IBAN
            };

            CezanneHelper.GetCampiFirma(out string codUser, out string cod_Termid, out DateTime tms_timestamp);
            datiBancari.COD_USER = codUser;
            datiBancari.COD_TERMID = cod_Termid;
            datiBancari.TMS_TIMESTAMP = tms_timestamp;
            db.XR_DATIBANCARI.Add(datiBancari);

            if (oldDB2Iban != null)
                result = DB2Manager.UpdateEntity(oldDB2Iban);

            if (!result && oldDB2Iban != null)
                errorMsg = "Errore durante l'aggiornamento DB2";
            else
                result = DB2Manager.InsertEntity(newDB2Iban);

            if (!result)
            {
                errorMsg = "Errore durante il salvataggio su DB2";

                //Restre vecchio record
                if (origDB2Iban != null)
                {
                    bool restore = DB2Manager.UpdateEntity(origDB2Iban);
                    if (!restore)
                    {
                        string message = String.Format($"Inserimento nuovo IBAN su DB2 non riuscito. Ripristino dati DB2 non riuscito.\nOrig:\n{Newtonsoft.Json.JsonConvert.SerializeObject(origDB2Iban)}\nNuovo:\n{Newtonsoft.Json.JsonConvert.SerializeObject(oldDB2Iban)}");
                        HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                    }
                }
            }
            else
            {
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                {
                    errorMsg = "Errore durante il salvataggio";


                    if (!DB2Manager.DeleteEntity(newDB2Iban))
                    {
                        string message = String.Format($"Inserimento dati IBAN su SQL non riuscito. Ripristino dati DB2 non riuscito. Dati: {Newtonsoft.Json.JsonConvert.SerializeObject(newDB2Iban)}");
                        HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                    }

                    if (origDB2Iban != null && !DB2Manager.UpdateEntity(origDB2Iban))
                    {
                        string message = String.Format($"Inserimento dati IBAN su SQL non riuscito. Ripristino vecchio IBAN DB2 non riuscito.\nOrig:\n{Newtonsoft.Json.JsonConvert.SerializeObject(origDB2Iban)}\nNuovo:\n{Newtonsoft.Json.JsonConvert.SerializeObject(oldDB2Iban)}");
                        HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                    }

                }
            }

            return result;
        }
        public static bool Delete_Iban(int idAnag, int idIban, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            var anag = db.XR_NDI_ANAG.Where(x => x.ID_ANAG == idAnag).OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();
            XR_DATIBANCARI dbRec = db.XR_DATIBANCARI.Find(idIban);
            if (dbRec != null)
            {
                TNDI_IBAN db2Iban = DB2Manager.GetEntity<TNDI_IBAN>(String.Format("Matricola='{0}' and Datini = '{1:yyyy-MM-dd}'", anag.MATRICOLA, dbRec.DTA_INIZIO));
                if (db2Iban != null)
                {
                    if (!DB2Manager.DeleteEntity(db2Iban))
                    {
                        errorMsg = "Errore durante la cancellazione dell'IBAN DB2";
                        return false;
                    }
                }

                db.XR_DATIBANCARI.Remove(dbRec);
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                {
                    if (!DB2Manager.InsertEntity(db2Iban))
                    {
                        string message = String.Format($"Cancellazione dati iban su SQL non riuscito. Ripristino dati DB2 non riuscito.");
                        message += "\nDa ripristinare su DB2:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(db2Iban);
                        HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                    }
                }
            }

            return result;
        }

        public static AnagNoDipIndirizziModel GetIndirizzo(int idAnag, DateTime? dataInizio)
        {
            AnagNoDipIndirizziModel model = new AnagNoDipIndirizziModel();
            model.Residenza = new IndirizzoModel() { Tipologia = IndirizzoType.Residenza };
            model.Domicilio = new IndirizzoModel() { Tipologia = IndirizzoType.Domicilio };

            var db = new IncentiviEntities();
            var anag = db.XR_NDI_ANAG.Where(x => x.ID_ANAG == idAnag).OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();
            if (dataInizio.HasValue)
            {
                XR_NDI_RESIDENZA_DOMICILIO dbRec = db.XR_NDI_RESIDENZA_DOMICILIO.Find(anag.ID_PERSONA, dataInizio.Value);
                model = new AnagNoDipIndirizziModel(dbRec);
            }
            else
            {
                model.IndDataInizio = anag.DTA_INIZIO;
                model.IndDataFine = AnagraficaManager.GetDateLimitMax();
            }

            model.IdPersona = anag.ID_PERSONA;
            model.IdAnag = idAnag;
            model.Matricola = anag.MATRICOLA;

            return model;
        }
        public static List<AnagNoDipIndirizziModel> GetIndirizzi(int idAnag)
        {
            List<AnagNoDipIndirizziModel> model = new List<AnagNoDipIndirizziModel>();

            var db = new IncentiviEntities();
            var anag = db.XR_NDI_ANAG.Where(x => x.ID_ANAG == idAnag).OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();
            if (anag.ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO != null)
            {
                foreach (var item in anag.ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO)
                {
                    var tmp = new AnagNoDipIndirizziModel(item);
                    tmp.IdPersona = anag.ID_PERSONA;
                    tmp.IdAnag = idAnag;
                    model.Add(tmp);
                }
            }

            return model;
        }
        public static bool Save_Indirizzo(AnagNoDipIndirizziModel model, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();

            DateTime dataMax = AnagraficaManager.GetDateLimitMax();

            XR_NDI_RESIDENZA_DOMICILIO oldRes = null;
            XR_NDI_RESIDENZA_DOMICILIO newRes = null;

            TNDI_RESIDENZA_DOMICILIO origOldDB2Res = null;
            TNDI_RESIDENZA_DOMICILIO origNewDB2Res = null;
            TNDI_RESIDENZA_DOMICILIO oldDB2Res = null;
            TNDI_RESIDENZA_DOMICILIO newDB2Res = null;

            if (model.IsNew)
            {
                oldRes = db.XR_NDI_RESIDENZA_DOMICILIO.FirstOrDefault(x => x.ID_PERSONA == model.IdPersona && x.DTA_FINE == dataMax);
                if (oldRes != null)
                    oldRes.DTA_FINE = model.IndDataInizio.AddDays(-1);

                newRes = new XR_NDI_RESIDENZA_DOMICILIO();

                oldDB2Res = DB2Manager.GetEntity<TNDI_RESIDENZA_DOMICILIO>(String.Format("Matricola='{0}' and Datfin='{1:yyyy-MM-dd}'", model.Matricola, dataMax));
                if (oldDB2Res != null)
                    oldDB2Res.Datfin = model.IndDataInizio.AddDays(-1);

                newDB2Res = new TNDI_RESIDENZA_DOMICILIO();
            }
            else
            {
                newRes = db.XR_NDI_RESIDENZA_DOMICILIO.FirstOrDefault(x => x.ID_PERSONA == model.IdPersona && x.DTA_INIZIO == model.IndDataInizio);
                newDB2Res = DB2Manager.GetEntity<TNDI_RESIDENZA_DOMICILIO>(String.Format("Matricola='{0}' and Datini='{1:yyyy-MM-dd}'", model.Matricola, model.IndDataInizio));
            }

            origOldDB2Res = oldDB2Res.Copy();
            origNewDB2Res = newDB2Res.Copy();

            newRes.ID_PERSONA = model.IdPersona;
            newRes.DTA_INIZIO = model.IndDataInizio;
            newRes.DTA_FINE = dataMax;
            newRes.COD_CITTARES = model.Residenza.CodCitta;
            newRes.DES_INDIRRES = model.Residenza.Indirizzo;
            newRes.CAP_CAPRES = model.Residenza.CAP;
            newRes.COD_CITTADOM = model.Domicilio.CodCitta;
            newRes.DES_INDIRDOM = model.Domicilio.Indirizzo;
            newRes.CAP_CAPDOM = model.Domicilio.CAP;

            newDB2Res.Matricola = model.Matricola;
            newDB2Res.Datini = model.IndDataInizio;
            newDB2Res.Datfin = dataMax;
            newDB2Res.Res_Cod_Comune = model.Residenza.CodCitta;
            newDB2Res.Res_Indirizzo = model.Residenza.Indirizzo;
            newDB2Res.Res_CAP = model.Residenza.CAP;
            //newDB2Res.Res_Cod_Nazione = model.Residenza.cod
            newDB2Res.Dom_Cod_Comune = model.Domicilio.CodCitta;
            newDB2Res.Dom_Indirizzo = model.Domicilio.Indirizzo;
            newDB2Res.Dom_CAP = model.Domicilio.CAP;
            //newDB2Res.Dom_Cod_Nazione = model.Domicilio.Naz;

            CezanneHelper.GetCampiFirma(out string codUser, out string cod_Termid, out DateTime tms_timestamp);
            newRes.COD_USER = codUser;
            newRes.COD_TERMID = cod_Termid;
            newRes.TMS_TIMESTAMP = tms_timestamp;

            if (model.IsNew)
                db.XR_NDI_RESIDENZA_DOMICILIO.Add(newRes);

            if (oldDB2Res != null)
            {
                result = DB2Manager.UpdateEntity(oldDB2Res);
                if (!result)
                {
                    errorMsg = "Errore durante l'aggiornamento del record precedente su DB2";
                    return result;
                }
            }

            if (model.IsNew)
                result = DB2Manager.InsertEntity(newDB2Res);
            else
                result = DB2Manager.UpdateEntity(newDB2Res);

            if (!result)
            {
                errorMsg = "Errore durante l'aggiornamento di DB2";
                if (origOldDB2Res != null && !DB2Manager.UpdateEntity(origOldDB2Res))
                {
                    string message = String.Format($"Modifica dati indirizzo su DB2 (per aggiunta nuovo indirizzo) non riuscito. Ripristino dati DB2 non riuscito.\nOrig:\n{Newtonsoft.Json.JsonConvert.SerializeObject(origOldDB2Res)}\nNuovo:\n{Newtonsoft.Json.JsonConvert.SerializeObject(oldDB2Res)}");
                    HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                }
            }
            else
            {
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                {
                    errorMsg = "Errore durante il salvataggio";

                    if (origOldDB2Res != null && !DB2Manager.UpdateEntity(origOldDB2Res))
                    {
                        string message = String.Format($"Modifica dati indirizzo su DB2 (per aggiunta nuovo indirizzo) non riuscito. Ripristino dati DB2 non riuscito.\nOrig:\n{Newtonsoft.Json.JsonConvert.SerializeObject(origOldDB2Res)}\nNuovo:\n{Newtonsoft.Json.JsonConvert.SerializeObject(oldDB2Res)}");
                        HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                    }

                    bool restore = false;
                    if (model.IsNew)
                        restore = DB2Manager.DeleteEntity(newDB2Res);
                    else
                        restore = DB2Manager.UpdateEntity(origNewDB2Res);

                    if (!restore)
                    {
                        string message = String.Format($"{(model.IsNew ? "Inserimento" : "Modifica")} dati anagrafici su SQL non riuscito. Ripristino dati DB2 non riuscito.");
                        if (!model.IsNew)
                            message += "\nOrig:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(origNewDB2Res);
                        message += "\nNuovi dati:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(newDB2Res);
                        HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                    }
                }
            }

            return result;
        }
        public static bool Delete_Indirizzo(int idAnag, DateTime dataInizio, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            var anag = db.XR_NDI_ANAG.Where(x => x.ID_ANAG == idAnag).OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();
            XR_NDI_RESIDENZA_DOMICILIO dbRec = db.XR_NDI_RESIDENZA_DOMICILIO.Find(anag.ID_PERSONA, dataInizio);
            if (dbRec != null)
            {
                TNDI_RESIDENZA_DOMICILIO db2Res = DB2Manager.GetEntity<TNDI_RESIDENZA_DOMICILIO>(String.Format("Matricola='{0}' and Datini='{1:yyyy-MM-dd}'", anag.MATRICOLA, dataInizio));
                if (db2Res != null)
                {
                    if (!DB2Manager.DeleteEntity(db2Res))
                    {
                        errorMsg = "Errore durante la cancellazione dell'indirizzo DB2";
                        return false;
                    }
                }

                db.XR_NDI_RESIDENZA_DOMICILIO.Remove(dbRec);
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                {
                    if (!DB2Manager.InsertEntity(db2Res))
                    {
                        string message = String.Format($"Cancellazione dati indirizzo su SQL non riuscito. Ripristino dati DB2 non riuscito.");
                        message += "\nDa ripristinare su DB2:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(db2Res);
                        HrisHelper.AddSegnalazione("Ripristino dati", "HRIS: Anagrafica non dipendenti", message);
                    }
                }
            }
            else
                errorMsg = "Indirizzo non trovato";

            return result;
        }
    }
}
