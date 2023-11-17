using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using myRai.Business;
using myRai.DataAccess;
using myRaiCommonModel;
using myRaiCommonTasks;
using myRaiData;
using myRaiData.Incentivi;

using myRaiHelper;
using myRaiHelper.Task;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using static myRaiHelper.AccessoFileHelper;

using CezanneDb = myRaiData.Incentivi.IncentiviEntities;
using TalentiaDB = myRaiDataTalentia.TalentiaEntities;

namespace myRaiCommonManager
{
    public partial class AnagraficaManager
    {
        public static DateTime GetDateLimitMax()
        {
            return new DateTime(2999, 12, 31);
        }

        private static void LoadDatiApprendistato(AnagraficaModel anag, CezanneDb dbCzn)
        {
            V_XR_APPRENDISTI recAppr = dbCzn.V_XR_APPRENDISTI.FirstOrDefault(x => x.ID_PERSONA == anag.IdPersona);
            if (recAppr != null)
            {
                anag.DatiApprendistato = new AnagraficaApprendistato()
                {
                    DtaInizio = recAppr.DTA_INIZIO,
                    DtaFine = recAppr.DTA_FINE.Value,
                    CodiceAppr = recAppr.COD_CAUSALEMOV,
                    DesAppr = recAppr.DES_CAUSALEMOV
                };
            }

            var recTutor = dbCzn.V_XR_TUTOR.Where(x => x.ID_PERSONA == anag.IdPersona);
            foreach (var item in recTutor)
            {
                anag.DatiTutoraggio.Add(new AnagraficaTutor()
                {
                    DtaInizio = item.DTA_INIZIO,
                    DtaFine = item.DTA_FINE.Value,
                    Apprendista = new AnagraficaApprendistato()
                    {
                        DtaInizio = item.DTA_INIZIO_APPR,
                        DtaFine = item.DTA_FINE_APPR.Value,
                        Matricola = item.COD_MATLIBROMAT_APPR,
                        IdPersona = item.ID_PERSONA_APPR,
                        Cognome = item.DES_COGNOMEPERS_APPR,
                        Nome = item.DES_NOMEPERS_APPR
                    }
                });
            }
        }

        #region DatiBancari
        private static void CaricaDatiBancari(AnagraficaModel anag, SINTESI1 sint)
        {
            string hrisAbil = AuthHelper.HrisFuncAbil(CommonHelper.GetCurrentUserMatricola());
            if (String.IsNullOrWhiteSpace(hrisAbil))
                hrisAbil = "HRCE";

            string matricola = CommonHelper.GetCurrentUserMatricola();
            AbilSubFunc subFunc = null;
            bool isAbil = false;
            if (hrisAbil == "HRCE")
                isAbil = AuthHelper.EnabledToSubFunc(matricola, "HRCE", "DATI_BANCARI", out subFunc);
            else
                isAbil = AuthHelper.EnabledToSubFunc(matricola, "HRIS_PERS", "BNKGES", out subFunc) || AuthHelper.EnabledToSubFunc(matricola, "HRIS_PERS", "BNKVIS", out subFunc);



            try
            {
                var db = GetDbIban();
                int intMatr = Convert.ToInt32(anag.Matricola);
                var datiBancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == sint.ID_PERSONA && x.DTA_FINE > DateTime.Now);
                var currentMod = db.CMINFOANAG_EXT.Where(x => x.ID_PERSONA == intMatr && x.COD_CONVALIDA_CC == "0");

                anag.DatiBancari.CanAdd = subFunc.Create;
                anag.DatiBancari.CanDelete = subFunc.Delete;
                anag.DatiBancari.CanModify = subFunc.Update;

                foreach (var item in datiBancari)
                {
                    foreach (var subItem in item.XR_UTILCONTO)
                    {
                        IbanModel iban = new IbanModel();
                        iban.IdDatiBancari = item.ID_XR_DATIBANCARI;

                        //XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.Include("TB_COMUNE").FirstOrDefault(x => x.COD_ABI == item.COD_ABI && x.COD_CAB == item.COD_CAB);
                        if (CezanneHelper.GetAnagBanca(item.COD_ABI, item.COD_CAB, true, out XR_ANAGBANCA anagBanca))
                        {
                            iban.IndirizzoAgenzia = (anagBanca.DES_INDIRIZZO.Trim() + " - " + (anagBanca.TB_COMUNE != null ? anagBanca.TB_COMUNE.DES_CITTA : "")).TitleCase();
                            iban.Agenzia = anagBanca.DES_RAG_SOCIALE;
                        }
                        else
                        {
                            iban.Agenzia = "-";
                            iban.IndirizzoAgenzia = "-";
                        }

                        string codUtilConto = "";
                        iban.Tipologia = IbanType.NonDefinito;
                        if (item.COD_TIPOCONTO == "C")
                        {
                            codUtilConto = "01";
                            iban.Tipologia = IbanType.AccreditoStipendio;
                        }
                        else if (item.COD_TIPOCONTO == "R")
                        {
                            codUtilConto = subItem.COD_UTILCONTO;
                            if (codUtilConto == "02")
                                iban.Tipologia = IbanType.AnticipoTrasferte;
                            else if (codUtilConto == "03")
                                iban.Tipologia = IbanType.AnticipoSpese;
                        }

                        iban.IBAN = item.COD_IBAN;
                        iban.Intestatario = item.DES_INTESTATARIO.TitleCase();

                        iban.IndCongelato = item.IND_CONGELATO == "Y";
                        iban.IndVincoli = item.IND_VINCOLATO == "Y";

                        if (item.IND_CONGELATO == "Y")
                            iban.Vincoli = "Conto congelato";
                        else if (item.IND_VINCOLATO == "Y")
                            iban.Vincoli = "Conto vincolato";

                        CaricaIdentityData(iban, sint);

                        //iban.CanModify = subFunc.Contains("01GEST") || subFunc.Contains("02GEST");
                        //if (iban.Tipologia != IbanType.AccreditoStipendio)
                        //    iban.CanDelete = subFunc.Contains("01GEST") || subFunc.Contains("02GEST");
                        iban.CanModify = anag.DatiBancari.CanModify;
                        if (iban.Tipologia != IbanType.AccreditoStipendio)
                            iban.CanDelete = anag.DatiBancari.CanDelete;

                        var mod = currentMod.FirstOrDefault(x => x.COD_UTILIZZO == codUtilConto);
                        if (mod != null)
                        {
                            iban.IdRichiestaMod = mod.ID_EVENTO;
                            iban.OperazioneRichiesta = mod.COD_CMEVENTO;
                            iban.DatiRichiesta = new IbanModel()
                            {
                                IBAN = mod.COD_IBAN,
                                Intestatario = mod.DES_INTESTATARIO,
                            };
                            anagBanca = db.XR_ANAGBANCA.Include("TB_COMUNE").FirstOrDefault(x => x.COD_ABI == mod.COD_ABI && x.COD_CAB == mod.COD_CAB);
                            if (anagBanca != null)
                            {
                                iban.DatiRichiesta.Agenzia = anagBanca.DES_RAG_SOCIALE;
                                iban.DatiRichiesta.IndirizzoAgenzia = (anagBanca.DES_INDIRIZZO.Trim() + " - " + (anagBanca.TB_COMUNE != null ? anagBanca.TB_COMUNE.DES_CITTA : "")).TitleCase();
                            }
                        }

                        anag.DatiBancari.Ibans.Add(iban);
                    }

                }

                foreach (var item in Enum.GetValues(typeof(IbanType)).OfType<IbanType>())
                {
                    if (item == IbanType.NonDefinito) continue;

                    if (item == IbanType.Anticipi)
                    {
                        if (!anag.DatiBancari.Ibans.Any(x => x.Tipologia == IbanType.AnticipoSpese && x.IdDatiBancari > 0)
                                && !anag.DatiBancari.Ibans.Any(x => x.Tipologia == IbanType.AnticipoTrasferte && x.IdDatiBancari > 0))
                            anag.DatiBancari.IbanLiberi.Add(item);
                    }
                    else if (!anag.DatiBancari.Ibans.Any(x => x.Tipologia == item))
                    {
                        IbanModel iban = new IbanModel()
                        {
                            IdDatiBancari = 0,
                            Tipologia = item,
                            CanModify = true,
                            CanDelete = false
                        };
                        DecodIbanType(item, out string codUtilizzo, out string codTipoConto);
                        var mod = currentMod.FirstOrDefault(x => x.COD_UTILIZZO == codUtilizzo);

                        //Se sto cercando una modifica per gli anticipi, verifico se è stata inserita una richiesta per entrambi gli anticipi
                        if (mod == null && (item == IbanType.AnticipoSpese || item == IbanType.AnticipoTrasferte))
                            mod = currentMod.FirstOrDefault(x => x.COD_UTILIZZO == "04");

                        if (mod != null)
                        {
                            iban.IdRichiestaMod = mod.ID_EVENTO;
                            iban.OperazioneRichiesta = mod.COD_CMEVENTO;
                            iban.DatiRichiesta = new IbanModel()
                            {
                                IBAN = mod.COD_IBAN,
                                Intestatario = mod.DES_INTESTATARIO,
                            };
                            var anagBanca = db.XR_ANAGBANCA.Include("TB_COMUNE").FirstOrDefault(x => x.COD_ABI == mod.COD_ABI && x.COD_CAB == mod.COD_CAB);
                            if (anagBanca != null)
                            {
                                iban.DatiRichiesta.Agenzia = anagBanca.DES_RAG_SOCIALE;
                                iban.DatiRichiesta.IndirizzoAgenzia = (anagBanca.DES_INDIRIZZO.Trim() + " - " + (anagBanca.TB_COMUNE != null ? anagBanca.TB_COMUNE.DES_CITTA : "")).TitleCase();
                            }
                        }

                        anag.DatiBancari.Ibans.Add(iban);

                        anag.DatiBancari.IbanLiberi.Add(item);
                    }
                }

            }
            catch (Exception ex)
            {


            }

            CaricaIdentityData(anag.DatiBancari, sint);

        }
        public static bool Save_DatiBancari(IbanModel model, out string errorMsg)
        {
            return Save_DatiBancari(null, null, model, out errorMsg);
        }
        public static bool Save_DatiBancari(CezanneDb db, CMINFOANAG_EXT recordModifica, IbanModel model, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            DateTime dataFine = GetDateLimitMax();

            if (db == null)
                db = GetDbIban();

            bool modificaDaRichiesta = recordModifica != null;
            if (modificaDaRichiesta)
            {
                string matricola = recordModifica.ID_PERSONA.ToString();
                SINTESI1 sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);


                model = new IbanModel()
                {
                    Matricola = matricola,
                    IdPersona = sint.ID_PERSONA
                };

                IbanType ibanType = DecodCodUtilizzo(recordModifica.COD_UTILIZZO);
                model.IBAN = recordModifica.COD_IBAN;
                model.Intestatario = recordModifica.DES_INTESTATARIO;
                model.Tipologia = ibanType;
                if (recordModifica.COD_CMEVENTO == "INSCC")
                    model.IdDatiBancari = 0;
                else
                {
                    var rifRecord = db.XR_DATIBANCARI.FirstOrDefault(x => x.ID_PERSONA == sint.ID_PERSONA && x.DTA_FINE == dataFine && x.XR_UTILCONTO.Any(y => y.COD_UTILCONTO == recordModifica.COD_UTILIZZO));
                    model.IdDatiBancari = rifRecord.ID_XR_DATIBANCARI;
                }
            }

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

            string codUtilizzo, codTipoConto;
            DecodIbanType(model.Tipologia, out codUtilizzo, out codTipoConto);

            string cmEvento = "";
            if (model.IdDatiBancari == 0)
                cmEvento = "INSCC";
            else
                cmEvento = "MODCC";

            if (!modificaDaRichiesta)
            {
                var anagBanca = db.XR_ANAGBANCA.FirstOrDefault(x => x.COD_ABI == codAbi && x.COD_CAB == codCab);
                recordModifica = new CMINFOANAG_EXT()
                {
                    ID_EVENTO = db.CMINFOANAG_EXT.GeneraPrimaryKey(9),
                    COD_IBAN = model.IBAN,
                    COD_ABI = codAbi,
                    COD_CAB = codCab,
                    COD_CMEVENTO = cmEvento,
                    COD_UTILIZZO = codUtilizzo,
                    ID_PERSONA = Convert.ToInt32(model.Matricola),
                    DES_INTESTATARIO = model.Intestatario,
                    IND_BANCA_ASSENTE = anagBanca == null ? "1" : "2"
                };
            }
            recordModifica.COD_CONVALIDA_CC = "1";

            XR_DATIBANCARI oldDatiBancari = null;
            XR_DATIBANCARI datiBancari = null;
            XR_DATIBANCARI replaceOther = null;
            XR_UTILCONTO utilconto = null;

            List<XR_UTILCONTO> otherUtilConto = new List<XR_UTILCONTO>();

            if (model.IdDatiBancari != 0)
            {
                oldDatiBancari = db.XR_DATIBANCARI.FirstOrDefault(x => x.ID_XR_DATIBANCARI == model.IdDatiBancari);
                oldDatiBancari.DTA_FINE = DateTime.Today.AddDays(-1);
            }

            datiBancari = new XR_DATIBANCARI()
            {
                ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(),
                ID_PERSONA = model.IdPersona,
                DTA_INIZIO = DateTime.Today,
                DTA_FINE = dataFine,
                COD_TIPOCONTO = codTipoConto,
                DES_INTESTATARIO = model.Intestatario,
                COD_IBAN = codIban,
                COD_ABI = codAbi,
                COD_CAB = codCab,
                COD_SUBTPCONTO = "ND",
                IND_CONGELATO = model.IndCongelato ? "Y" : "N",
                IND_DELETE = "N",
                IND_VINCOLATO = model.IndVincoli ? "Y" : "N",
                IND_CHANGED = "N"
            };

            if (model.Tipologia != IbanType.Anticipi)
            {
                utilconto = new XR_UTILCONTO()
                {
                    ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9),
                    ID_XR_DATIBANCARI = datiBancari.ID_XR_DATIBANCARI,
                    COD_UTILCONTO = codUtilizzo
                };
            }
            else
            {
                utilconto = new XR_UTILCONTO()
                {
                    ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9),
                    ID_XR_DATIBANCARI = datiBancari.ID_XR_DATIBANCARI,
                    COD_UTILCONTO = "02",
                };
                otherUtilConto.Add(new XR_UTILCONTO()
                {
                    ID_XR_DATIBANCARI = datiBancari.ID_XR_DATIBANCARI,
                    ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9),
                    COD_UTILCONTO = "03"
                });
            }

            if (codTipoConto == "R" && codUtilizzo != "04")
            {
                var otherIban = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == model.IdPersona && x.COD_TIPOCONTO == "R" && x.COD_IBAN == model.IBAN && x.DTA_FINE == dataFine && x.ID_XR_DATIBANCARI != model.IdDatiBancari);
                foreach (var other in otherIban)
                {
                    other.DTA_FINE = DateTime.Today.AddDays(-1);
                    foreach (var item in db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == other.ID_XR_DATIBANCARI))
                    {
                        otherUtilConto.Add(new XR_UTILCONTO()
                        {
                            ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9),
                            ID_XR_DATIBANCARI = datiBancari.ID_XR_DATIBANCARI,
                            COD_UTILCONTO = item.COD_UTILCONTO
                        });
                    }
                }

                var oldUtilConto = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == model.IdDatiBancari && x.COD_UTILCONTO != codUtilizzo);
                if (oldUtilConto.Any())
                {
                    replaceOther = new XR_DATIBANCARI()
                    {
                        ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(),
                        ID_PERSONA = oldDatiBancari.ID_PERSONA,
                        COD_IBAN = oldDatiBancari.COD_IBAN,
                        COD_ABI = oldDatiBancari.COD_ABI,
                        COD_CAB = oldDatiBancari.COD_CAB,
                        COD_SUBTPCONTO = oldDatiBancari.COD_SUBTPCONTO,
                        COD_TIPOCONTO = oldDatiBancari.COD_TIPOCONTO,
                        DES_INTESTATARIO = oldDatiBancari.DES_INTESTATARIO,
                        DTA_INIZIO = DateTime.Today,
                        DTA_FINE = dataFine,
                        IND_CONGELATO = oldDatiBancari.IND_CONGELATO,
                        IND_VINCOLATO = oldDatiBancari.IND_VINCOLATO,
                        IND_CHANGED = oldDatiBancari.IND_CHANGED,
                        IND_DELETE = oldDatiBancari.IND_DELETE,
                        COD_STATO = oldDatiBancari.COD_STATO
                    };

                    foreach (var item in oldUtilConto)
                    {
                        otherUtilConto.Add(new XR_UTILCONTO()
                        {
                            ID_XR_DATIBANCARI = datiBancari.ID_XR_DATIBANCARI,
                            ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9),
                            COD_UTILCONTO = item.COD_UTILCONTO
                        });
                    }
                }
            }


            CezanneHelper.GetCampiFirma(out string codUser, out string cod_Termid, out DateTime tms_timestamp);
            recordModifica.COD_USER = codUser;
            recordModifica.COD_TERMID = cod_Termid;
            recordModifica.TMS_TIMESTAMP = tms_timestamp;
            if (!modificaDaRichiesta)
                db.CMINFOANAG_EXT.Add(recordModifica);

            datiBancari.COD_USER = codUser;
            datiBancari.COD_TERMID = cod_Termid;
            datiBancari.TMS_TIMESTAMP = tms_timestamp;
            db.XR_DATIBANCARI.Add(datiBancari);

            utilconto.COD_USER = codUser;
            utilconto.COD_TERMID = cod_Termid;
            utilconto.TMS_TIMESTAMP = tms_timestamp;
            db.XR_UTILCONTO.Add(utilconto);

            if (replaceOther != null)
            {
                replaceOther.COD_USER = codUser;
                replaceOther.COD_TERMID = cod_Termid;
                replaceOther.TMS_TIMESTAMP = tms_timestamp;
                db.XR_DATIBANCARI.Add(datiBancari);
            }

            foreach (var item in otherUtilConto)
            {
                item.COD_USER = codUser;
                item.COD_TERMID = cod_Termid;
                item.TMS_TIMESTAMP = tms_timestamp;
                db.XR_UTILCONTO.Add(item);
            }

            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            HrisHelper.LogOperazione("AggiornamentoIBAN", $"Aggiornamento iban ID_PERS:{model.IdPersona} MATR:{model.Matricola} " + (modificaDaRichiesta ? "da richiesta " + recordModifica.ID_EVENTO.ToString() : ""), result, errorMsg, null, null, datiBancari);

            return result;
        }
        private static void DecodIbanType(IbanType ibanType, out string codUtilizzo, out string codTipoConto)
        {
            codUtilizzo = "";
            codTipoConto = "";
            switch (ibanType)
            {
                case IbanType.NonDefinito:
                    break;
                case IbanType.AccreditoStipendio:
                    codUtilizzo = "01";
                    codTipoConto = "C";
                    break;
                case IbanType.AnticipoTrasferte:
                    codUtilizzo = "02";
                    codTipoConto = "R";
                    break;
                case IbanType.AnticipoSpese:
                    codUtilizzo = "03";
                    codTipoConto = "R";
                    break;
                case IbanType.Anticipi:
                    codUtilizzo = "04";
                    codTipoConto = "R";
                    break;
                default:
                    break;
            }
        }

        public static bool Delete_DatiBancari(string m, int id, IbanType ibanType, out string errorMsg)
        {
            return Delete_DatiBancari(null, m, id, null, ibanType, out errorMsg);
        }
        public static bool Delete_DatiBancari(CezanneDb db, string m, int id, CMINFOANAG_EXT logDelete, IbanType ibanType, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            DateTime dataFine = GetDateLimitMax();

            string cod_user, cod_termid;
            DateTime tms_timestamp;

            XR_DATIBANCARI record = null;
            bool deleteFromRichiesta = logDelete != null;

            string codUtilizzo, codTipoConto;
            DecodIbanType(ibanType, out codUtilizzo, out codTipoConto);

            if (db == null)
                db = GetDbIban();

            SINTESI1 sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == m);

            if (!deleteFromRichiesta)
                record = db.XR_DATIBANCARI.FirstOrDefault(x => x.ID_XR_DATIBANCARI == id);
            else
                record = db.XR_DATIBANCARI.FirstOrDefault(x => x.ID_PERSONA == sint.ID_PERSONA && x.COD_IBAN == logDelete.COD_IBAN && x.DTA_FINE == dataFine);

            if (record != null)
            {
                var recordutilizzo = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == id);

                XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.FirstOrDefault(x => x.COD_ABI == record.COD_ABI && x.COD_CAB == record.COD_CAB);

                if (logDelete == null)
                    logDelete = new CMINFOANAG_EXT()
                    {
                        ID_EVENTO = db.CMINFOANAG_EXT.GeneraPrimaryKey(9),
                        ID_PERSONA = Convert.ToInt32(m),
                        COD_IBAN = record.COD_IBAN,
                        COD_ABI = record.COD_ABI,
                        COD_CAB = record.COD_CAB,
                        COD_CMEVENTO = "DELCC",
                        COD_UTILIZZO = codUtilizzo,
                        IND_BANCA_ASSENTE = anagBanca == null ? "1" : "2"
                    };

                logDelete.COD_CONVALIDA_CC = "1";

                record.DTA_FINE = DateTime.Today.AddDays(-1);
                if (codTipoConto == "R" && recordutilizzo.Count() > 1)
                {
                    //Nel caso degli anticipi, bisogna creare un nuovo record per il conto che rimane attivo (nel caso ci sia)
                    XR_DATIBANCARI otherAnt = new XR_DATIBANCARI()
                    {
                        ID_XR_DATIBANCARI = db.XR_DATIBANCARI.GeneraPrimaryKey(9),
                        ID_PERSONA = record.ID_PERSONA,
                        DTA_INIZIO = DateTime.Today,
                        DTA_FINE = new DateTime(2099, 12, 31),
                        COD_IBAN = record.COD_IBAN,
                        COD_ABI = record.COD_ABI,
                        COD_CAB = record.COD_CAB,
                        COD_TIPOCONTO = record.COD_TIPOCONTO,
                        DES_INTESTATARIO = record.DES_INTESTATARIO,
                        COD_SUBTPCONTO = "ND",
                        IND_CONGELATO = "N",
                        IND_DELETE = "N",
                        IND_VINCOLATO = "N",
                        IND_CHANGED = "N"
                    };
                    CezanneHelper.GetCampiFirma(out cod_user, out cod_termid, out tms_timestamp);
                    otherAnt.COD_USER = cod_user;
                    otherAnt.COD_TERMID = cod_termid;
                    otherAnt.TMS_TIMESTAMP = tms_timestamp;

                    db.XR_DATIBANCARI.Add(otherAnt);

                    foreach (var item in recordutilizzo.Where(x => x.COD_UTILCONTO != codUtilizzo))
                    {
                        XR_UTILCONTO otherUtil = new XR_UTILCONTO()
                        {
                            ID_XR_UTILCONTO = db.XR_UTILCONTO.GeneraPrimaryKey(9),
                            ID_XR_DATIBANCARI = otherAnt.ID_XR_DATIBANCARI,
                            COD_UTILCONTO = item.COD_UTILCONTO,
                            COD_USER = cod_user,
                            COD_TERMID = cod_termid,
                            TMS_TIMESTAMP = tms_timestamp
                        };
                        db.XR_UTILCONTO.Add(otherUtil);
                    }
                }

                CezanneHelper.GetCampiFirma(out cod_user, out cod_termid, out tms_timestamp);
                logDelete.COD_USER = cod_user;
                logDelete.COD_TERMID = cod_termid;
                logDelete.TMS_TIMESTAMP = tms_timestamp;

                record.COD_USER = cod_user;
                record.COD_TERMID = cod_termid;
                record.TMS_TIMESTAMP = tms_timestamp;

                if (deleteFromRichiesta)
                    db.CMINFOANAG_EXT.Add(logDelete);

                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
            }
            else
            {
                errorMsg = "Record non trovato";
            }

            HrisHelper.LogOperazione("CancellazioneIBAN", $"Cancellazione iban ID_PERS:{sint.ID_PERSONA} MATR:{sint.COD_MATLIBROMAT} " + (deleteFromRichiesta ? "da richiesta " + logDelete.ID_EVENTO.ToString() : ""), result, errorMsg, null, null, record);

            return result;
        }


        public static bool Delete_ModificaIban(int id, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = GetDbIban();

            var daEliminare = db.CMINFOANAG_EXT.FirstOrDefault(x => x.ID_EVENTO == id);
            if (daEliminare != null)
            {
                db.CMINFOANAG_EXT.Remove(daEliminare);
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
            }
            else
            {
                errorMsg = "Modifica non trovata";
            }

            HrisHelper.LogOperazione("CancellazioneModificaIban", $"Cancellazione modifica iban ID_PERS:{daEliminare.ID_PERSONA}", result, errorMsg, null, null, daEliminare);

            return result;
        }
        public static bool Reinvia_CodiceIban(int id, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = GetDbIban();
            var oldOtpCode = db.XR_SSV_CODICE_OTP.FirstOrDefault(x => x.ID_EVENTO == id && x.COD_FUNZIONE == "01");
            if (oldOtpCode != null)
            {
                XR_SSV_CODICE_OTP newCode = new XR_SSV_CODICE_OTP()
                {
                    ID_CODICE_OTP = db.XR_SSV_CODICE_OTP.GeneraPrimaryKey(),
                    MATRICOLA = oldOtpCode.MATRICOLA,
                    COD_FUNZIONE = oldOtpCode.COD_FUNZIONE,
                    DTA_UTILIZZO = DateTime.Now,
                    DTA_SCADENZA = DateTime.Today.AddDays(7),
                    ID_EVENTO = oldOtpCode.ID_EVENTO,
                    IND_UTILIZZO = "0"
                };

                CezanneHelper.GetCampiFirma(out string cod_user, out string cod_temrid, out DateTime tms_timestamp);
                db.XR_SSV_CODICE_OTP.Add(newCode);
                db.XR_SSV_CODICE_OTP.Remove(oldOtpCode);

                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (result)
                {
                    //invio della mail
                    string corpo2 = "<p align=”justify”>Il codice per convalidare la tua richiesta di modifica dei dati bancari da inserire nel portale Rai per Me nella sezione Profilo Personale – Dati Bancari, è il seguente:</p>" +
                                    "<p><h1 style=”text-align:center” > " + newCode.ID_CODICE_OTP + "</h1></p>";

                    GestoreMail mail = new GestoreMail();
                    string dest = CommonHelper.GetEmailPerMatricola(oldOtpCode.MATRICOLA);
                    var response2 = mail.InvioMail("[CG] RaiPlace - Rai per Me <raiplace.raiperme@rai.it>",
                        "Richiesta di modifica di dati bancari: istruzioni per la convalida",
                        dest,
                        "raiplace.selfservice@rai.it",
                        "Richiesta di modifica IBAN",
                        "Convalida richiesta",
                        corpo2,
                        null,
                        null,
                        null, DateTime.Now.AddDays(1));

                    if (response2 == null || !String.IsNullOrWhiteSpace(response2.Errore))
                    {
                        result = false;
                        errorMsg = "Non è stato possibile inviare il nuovo codice";
                    }
                }
                else
                {
                    errorMsg = "Errore durante l'inserimento del nuovo codice";
                }
            }
            else
            {
                errorMsg = "Vecchio codice non trovato";
            }

            return result;
        }
        public static bool Convalida_ModificaIban(string m, int id, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = GetDbIban();
            var logModifica = db.CMINFOANAG_EXT.FirstOrDefault(x => x.ID_EVENTO == id && x.COD_CONVALIDA_CC == "0");
            if (logModifica != null)
            {
                IbanType ibanType = DecodCodUtilizzo(logModifica.COD_UTILIZZO);

                if (logModifica.COD_CMEVENTO == "DELCC")
                    result = Delete_DatiBancari(db, m, 0, logModifica, ibanType, out errorMsg);
                else
                    result = Save_DatiBancari(db, logModifica, null, out errorMsg);
            }
            else
            {
                errorMsg = "Richiesta di modifica non trovata";
            }

            return result;
        }

        private static IbanType DecodCodUtilizzo(string codUtilizzo)
        {
            IbanType ibanType = IbanType.NonDefinito;
            switch (codUtilizzo)
            {
                case "01":
                    ibanType = IbanType.AccreditoStipendio;
                    break;
                case "02":
                    ibanType = IbanType.AnticipoTrasferte;
                    break;
                case "03":
                    ibanType = IbanType.AnticipoSpese;
                    break;
                case "04":
                    ibanType = IbanType.Anticipi;
                    break;
                default:
                    break;
            }

            return ibanType;
        }
        #endregion

        public static List<IndirizzoModel> StoricoDatiIndirizzo(int idPersona, IndirizzoType tipologia, bool aggiungiCorrente)
        {
            List<IndirizzoModel> storico = new List<IndirizzoModel>();

            var db = GetDb();
            DateTime dateMax = GetDateLimitMax();

            switch (tipologia)
            {
                case IndirizzoType.Residenza:
                    foreach (var item in db.RESIDENZA.Where(x => x.ID_PERSONA == idPersona && (aggiungiCorrente || x.DTA_FINE != dateMax)).OrderByDescending(x => x.DTA_INIZIO))
                    {
                        IndirizzoModel indirizzo = new IndirizzoModel()
                        {
                            Tipologia = IndirizzoType.Residenza,
                            Indirizzo = item.DES_INDIRRESID,
                            CAP = item.CAP_CAPRESID,
                            Decorrenza = item.DTA_INIZIO,
                            CodCitta = item.COD_CITTA
                        };
                        TB_COMUNE citta = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == item.COD_CITTA);
                        if (citta != null)
                        {
                            indirizzo.Citta = citta.DES_CITTA.TitleCase() + ", " + citta.DES_PROV_STATE;

                            TB_NAZIONE nazione = db.TB_NAZIONE.FirstOrDefault(x => x.COD_SIGLANAZIONE == citta.COD_SIGLANAZIONE);
                            if (nazione != null)
                                indirizzo.Stato = nazione.DES_NAZIONE.TitleCase();
                        }

                        storico.Add(indirizzo);
                    }
                    break;
                case IndirizzoType.Domicilio:
                    // REPERIMENTO DEGLI IDEVENTO CON CODICE RAI013 -- > Cambio domicilio
                    var eventi = (from e in db.CMEVENTI
                                  join eCurr in db.CMEVENTICURR
                                  on e.ID_CMEVENTO equals eCurr.ID_CMEVENTO
                                  where e.COD_CMEVENTO == "RAI013" && eCurr.ID_PERSONA == idPersona
                                  orderby e.DTA_INIZIO descending
                                  select new
                                  {
                                      ID_CMEVENTO = eCurr.ID_CMEVENTO,
                                      COD_CMEVENTO = e.COD_CMEVENTO,
                                      ID_PERSONA = eCurr.ID_PERSONA,
                                      DTA_INIZIO = e.DTA_INIZIO
                                  }).Distinct().ToList();

                    if (eventi != null && eventi.Any())
                    {
                        foreach (var e in eventi)
                        {
                            var datoAnag = db.CMINFOANAG.Where(w => w.ID_CMEVENTO.Equals(e.ID_CMEVENTO)).FirstOrDefault();
                            if (datoAnag != null)
                            {
                                IndirizzoModel indirizzo = new IndirizzoModel()
                                {
                                    Tipologia = IndirizzoType.Domicilio,
                                    Indirizzo = datoAnag.DES_INDIRDOM,
                                    CAP = datoAnag.CAP_CAPDOM,
                                    Decorrenza = e.DTA_INIZIO
                                };

                                TB_COMUNE citta = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == datoAnag.COD_CITTADOM);

                                if (citta != null)
                                {
                                    indirizzo.Citta = citta.DES_CITTA.TitleCase() + ", " + citta.DES_PROV_STATE;

                                    TB_NAZIONE nazione = db.TB_NAZIONE.FirstOrDefault(x => x.COD_SIGLANAZIONE == citta.COD_SIGLANAZIONE);
                                    if (nazione != null)
                                        indirizzo.Stato = nazione.DES_NAZIONE.TitleCase();
                                }

                                storico.Add(indirizzo);
                            }
                        }
                    }

                    break;
                default:
                    break;
            }

            return storico;
        }

        public static List<IbanModel> StoricoDatiIban(int idPersona, IbanType tipologia)
        {
            List<IbanModel> storico = new List<IbanModel>();

            var db = GetDbIban();
            DateTime dateMax = GetDateLimitMax();

            DecodIbanType(tipologia, out string codUtilizzo, out string codTipoConto);

            IQueryable<XR_DATIBANCARI> list = null;

            switch (tipologia)
            {
                case IbanType.NonDefinito:
                    break;
                case IbanType.AccreditoStipendio:
                    list = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == idPersona && x.DTA_FINE < dateMax && x.COD_TIPOCONTO == codTipoConto);
                    break;
                case IbanType.AnticipoTrasferte:
                case IbanType.AnticipoSpese:
                    list = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == idPersona && x.DTA_FINE < dateMax && x.COD_TIPOCONTO == codTipoConto && x.XR_UTILCONTO.Any(y => y.COD_UTILCONTO == codUtilizzo));
                    break;
                default:
                    break;
            }

            if (list != null)
            {
                foreach (var item in list)
                {
                    IbanModel model = new IbanModel()
                    {
                        IBAN = item.COD_IBAN,
                        Intestatario = item.DES_INTESTATARIO,
                        DataInizio = item.DTA_INIZIO,
                        DataFine = item.DTA_FINE.Value
                    };

                    XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.FirstOrDefault(x => x.COD_ABI == item.COD_ABI && x.COD_CAB == item.COD_CAB);
                    if (anagBanca != null)
                    {
                        model.Agenzia = anagBanca.DES_RAG_SOCIALE;
                        model.IndirizzoAgenzia = (anagBanca.DES_INDIRIZZO.Trim() + " - " + (anagBanca.TB_COMUNE != null ? anagBanca.TB_COMUNE.DES_CITTA : "")).TitleCase();
                    }

                    storico.Add(model);
                }
            }

            return storico;
        }

        public static List<RichiestaAnag> GetRichieste(RichiestaLoader loader = null)
        {
            if (loader == null)
                loader = new RichiestaLoader();

            List<RichiestaAnag> richieste = new List<RichiestaAnag>();

            bool hasTipFilter = loader.Tipologie.Any() && !loader.Tipologie.Contains(TipoRichiestaAnag.Tutte);

            if (!hasTipFilter || loader.Tipologie.Contains(TipoRichiestaAnag.IBAN))
            {
                string[] eventiIban = new string[] { "INSCC", "MODCC", "DELCC" };
                var db = GetDbIban();
                var currentMod = db.CMINFOANAG_EXT.Where(x => eventiIban.Contains(x.COD_CMEVENTO) && x.COD_CONVALIDA_CC == "0");
                if (!String.IsNullOrWhiteSpace(loader.Matricola))
                {
                    int.TryParse(loader.Matricola, out int intMatr);
                    currentMod = currentMod.Where(x => x.ID_PERSONA == intMatr);
                }

                foreach (var item in currentMod)
                {
                    RichiestaAnag newReq = new RichiestaAnag();
                    newReq.IdRichiesta = item.ID_EVENTO;
                    newReq.Tipologia = TipoRichiestaAnag.IBAN;
                    newReq.DataRichiesta = item.TMS_TIMESTAMP.Value;
                    if (item.COD_CMEVENTO == "INSCC")
                        newReq.Descrizione = "Inserimento IBAN";
                    else if (item.COD_CMEVENTO == "MODCC")
                        newReq.Descrizione = "Modifica IBAN";
                    else if (item.COD_CMEVENTO == "DELCC")
                        newReq.Descrizione = "Cancellazione IBAN";

                    newReq.Matricola = item.ID_PERSONA.ToString("000000");
                    var sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == newReq.Matricola);
                    newReq.IdPersona = sint.ID_PERSONA;
                    newReq.Nominativo = sint.Nominativo();

                    richieste.Add(newReq);
                }
            }
            if (!hasTipFilter || loader.Tipologie.Contains(TipoRichiestaAnag.VariazioneContrattuale))
            {
                var db = GetDb();
                var reqAttive = WorkflowHelper.NotIsCurrentState((int)VarContrStati.RichiestaCancellata, (int)VarContrStati.RichiestaEvasa);
                var listOpenReq = db.XR_WKF_RICHIESTE.Where(x => x.ID_TIPOLOGIA == WKF_VAR_CONTR).Where(reqAttive);

                if (!String.IsNullOrWhiteSpace(loader.Matricola))
                    listOpenReq = listOpenReq.Where(x => x.MATRICOLA == loader.Matricola);

                foreach (var item in listOpenReq)
                {
                    RichiestaAnag newReq = new RichiestaAnag();
                    newReq.IdRichiesta = item.ID_GESTIONE;
                    newReq.Tipologia = TipoRichiestaAnag.VariazioneContrattuale;
                    newReq.Descrizione = "Variazione " + item.COD_TIPO;
                    newReq.DataRichiesta = item.DTA_CREAZIONE;
                    CaricaIdentityData(newReq, item.ID_PERSONA, item.MATRICOLA);

                    var sint = db.SINTESI1.Find(item.ID_PERSONA);
                    if (sint != null)
                        newReq.Nominativo = sint.Nominativo();

                    richieste.Add(newReq);
                }
            }
            if (!hasTipFilter || loader.Tipologie.Contains(TipoRichiestaAnag.Dematerializzazione))
            {
                List<XR_DEM_DOCUMENTI_EXT> elenco = DematerializzazioneManager.GetDocumentiDaApprovare2();
                //if (!String.IsNullOrWhiteSpace(loader.Matricola))
                //    elenco = elenco.Where(x => x.MatricolaDestinatario == loader.Matricola).ToList();

                if (elenco != null && elenco.Any())
                {
                    foreach (var item in elenco)
                    {
                        RichiestaAnag newReq = new RichiestaAnag();
                        newReq.IdRichiesta = item.Id;
                        newReq.Tipologia = TipoRichiestaAnag.Dematerializzazione;
                        newReq.Descrizione = item.Descrizione;
                        if (item.IdPersonaDestinatario.HasValue)
                        {
                            newReq.IdPersona = item.IdPersonaDestinatario.Value;
                        }
                        newReq.Matricola = item.MatricolaDestinatario;
                        newReq.Nominativo = item.NominativoUtenteDestinatario;
                        newReq.DataScadenza = null;
                        CaricaIdentityData(newReq, item.IdPersonaDestinatario.GetValueOrDefault(), item.MatricolaDestinatario);
                        richieste.Add(newReq);
                    }
                }
            }

            return richieste;
        }

        public static RichiestaAnag GetRichiesta(string m, TipoRichiestaAnag tipoRichiesta, int idRichiesta)
        {
            RichiestaAnag richiesta = new RichiestaAnag();
            richiesta.Tipologia = tipoRichiesta;

            switch (tipoRichiesta)
            {
                case TipoRichiestaAnag.IBAN:
                    var db = GetDbIban();
                    var mod = db.CMINFOANAG_EXT.FirstOrDefault(x => x.ID_EVENTO == idRichiesta);
                    if (mod != null)
                    {
                        if (mod.COD_CONVALIDA_CC == "1")
                        {
                            richiesta.HasError = true;
                            richiesta.ErrorMsg = "Richiesta già convalidata";
                        }
                        else
                        {
                            SINTESI1 sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == m);

                            IbanModel iban = new IbanModel();
                            iban.Tipologia = DecodCodUtilizzo(mod.COD_UTILIZZO);
                            iban.IdRichiestaMod = mod.ID_EVENTO;
                            iban.OperazioneRichiesta = mod.COD_CMEVENTO;
                            iban.DatiRichiesta = new IbanModel()
                            {
                                IBAN = mod.COD_IBAN,
                                Intestatario = mod.DES_INTESTATARIO,
                            };
                            var anagBanca = db.XR_ANAGBANCA.Include("TB_COMUNE").FirstOrDefault(x => x.COD_ABI == mod.COD_ABI && x.COD_CAB == mod.COD_CAB);
                            if (anagBanca != null)
                            {
                                iban.DatiRichiesta.Agenzia = anagBanca.DES_RAG_SOCIALE;
                                iban.DatiRichiesta.IndirizzoAgenzia = (anagBanca.DES_INDIRIZZO.Trim() + " - " + (anagBanca.TB_COMUNE != null ? anagBanca.TB_COMUNE.DES_CITTA : "")).TitleCase();
                            }

                            CaricaIdentityData(iban, sint.ID_PERSONA, sint.COD_MATLIBROMAT);
                            richiesta.ObjInfo = iban;
                        }
                    }
                    else
                    {
                        richiesta.HasError = true;
                        richiesta.ErrorMsg = "Richiesta non trovata";
                    }
                    break;
                case TipoRichiestaAnag.VariazioneContrattuale:
                    var dbCzn = GetDb();
                    var dbreq = dbCzn.XR_WKF_RICHIESTE.FirstOrDefault(x => x.ID_GESTIONE == idRichiesta && x.ID_TIPOLOGIA == WKF_VAR_CONTR);
                    richiesta.IdRichiesta = dbreq.ID_GESTIONE;
                    richiesta.Descrizione = "Variazione " + dbreq.COD_TIPO;
                    richiesta.DataRichiesta = dbreq.DTA_CREAZIONE;
                    richiesta.ObjInfo = SerializerHelper.DeserializeXml(dbreq.MODELLO, typeof(EventoModel));
                    ((EventoModel)richiesta.ObjInfo).IdRichiesta = richiesta.IdRichiesta;
                    break;
                default:
                    break;
            }

            return richiesta;
        }

        public static List<GestioneAnag> GetGestioni(string m)
        {
            List<GestioneAnag> gestioni = new List<GestioneAnag>();

            var db = new CezanneDb();
            var sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == m);

            // CF 30/06/2022 --> 
            var dbTalentia = new TalentiaDB();
            var datiTfrUtente = dbTalentia.XR_MOD_DIPENDENTI
                .Where(w => w.MATRICOLA.Equals(m) && w.COD_MODULO.Equals("TFR"))
                .FirstOrDefault();
            // <--

            if (CessazioneHelper.EnabledToIncentivi(CommonHelper.GetCurrentUserMatricola()))
            {
                var pratica = sint.XR_INC_DIPENDENTI.FirstOrDefault();
                gestioni.Add(new GestioneAnag()
                {
                    IdPersona = sint.ID_PERSONA,
                    Matricola = sint.COD_MATLIBROMAT,
                    Tipologia = TipoGestioneAnag.Cessazione,
                    IdGestione = pratica != null ? pratica.ID_DIPENDENTE : 0,
                    Descrizione = pratica != null ? pratica.XR_WKF_TIPOLOGIA.DES_TIPOLOGIA : "Cessazione",
                });
            }

            if (PoliticheRetributiveHelper.EnabledTo(CommonHelper.GetCurrentUserMatricola()))
            {
                var pratiche = sint.XR_PRV_DIPENDENTI;
                if (pratiche == null || !pratiche.Any())
                {
                    gestioni.Add(new GestioneAnag()
                    {
                        IdPersona = sint.ID_PERSONA,
                        Matricola = sint.COD_MATLIBROMAT,
                        Tipologia = TipoGestioneAnag.ProvvRetr,
                        IdGestione = 0,
                        Descrizione = "Provvedimento retributivo"
                    });
                }
                else
                {
                    foreach (var pratica in pratiche)
                    {
                        gestioni.Add(new GestioneAnag()
                        {
                            IdPersona = sint.ID_PERSONA,
                            Matricola = sint.COD_MATLIBROMAT,
                            Tipologia = TipoGestioneAnag.ProvvRetr,
                            IdGestione = pratica.ID_DIPENDENTE,
                            Descrizione = pratica.XR_PRV_PROV_EFFETTIVO.DESCRIZIONE
                        });
                    }
                }
            }

            // se sono autorizzato
            var subFunc = AuthHelper.EnabledSubFunc(CommonHelper.GetCurrentUserMatricola(), "DEMA");

            // se ho la funzione 02GEST - posso accedere alla dematerializzazione
            if (subFunc.Contains("02GEST"))
            {
                gestioni.Add(new GestioneAnag()
                {
                    IdPersona = sint.ID_PERSONA,
                    Matricola = sint.COD_MATLIBROMAT,
                    Tipologia = TipoGestioneAnag.Dematerializzazione,
                    IdGestione = 0,
                    Descrizione = "Crea nuovo documento"
                });
            }

            // CF 30/06/2022 --> Link gestione scelta TFR
            // TODO Inserire eventuale condizione per far apparire o meno la gestione
            if (!CommonHelper.IsProduzione())
            {
                gestioni.Add(new GestioneAnag()
                {
                    IdPersona = sint.ID_PERSONA,
                    Matricola = sint.COD_MATLIBROMAT,
                    Tipologia = TipoGestioneAnag.DestinazioneTFR,
                    IdGestione = datiTfrUtente?.XR_MOD_DIPENDENTI1 ?? 0,
                    Descrizione = "Scelta destinazione TFR"
                });
            }
            // <--
            return gestioni;
        }

        #region DatiTitoliStudio
        private static void CaricaDatiTitoliStudio(AnagraficaModel anag, CezanneDb db, SINTESI1 sint)
        {
            string hrisAbil = AuthHelper.HrisFuncAbil(CommonHelper.GetCurrentUserMatricola());
            if (String.IsNullOrWhiteSpace(hrisAbil))
                hrisAbil = "HRCE";

            string matricola = CommonHelper.GetCurrentUserMatricola();
            AbilSubFunc subFunc = null;
            bool isAbil = false;
            if (hrisAbil == "HRCE")
                isAbil = AuthHelper.EnabledToSubFunc(matricola, "HRCE", "AGG_ANAG", out subFunc) || AuthHelper.EnabledToSubFunc(matricola, "HRCE", "EVID_ANAG", out subFunc);
            else
                isAbil = AuthHelper.EnabledToSubFunc(matricola, "HRIS_PERS", "STUGES", out subFunc) || AuthHelper.EnabledToSubFunc(matricola, "HRIS_PERS", "STUVIS", out subFunc);
            anag.DatiTitoliStudio.CanAdd = subFunc.Create;
            anag.DatiTitoliStudio.CanDelete = subFunc.Delete;
            anag.DatiTitoliStudio.CanModify = subFunc.Update;

            List<StudioModel> studiCezanne = CaricaStudiCezanne(anag, db, sint);
            List<StudioModel> studiCV = CaricaStudiCV(anag, db, sint);

            foreach (var item in studiCezanne)
            {
                StudioModel cv = studiCV.FirstOrDefault(x => x.CodTitolo == item.CodTitolo);

                if (cv != null)
                {
                    //item.Origine.Add(StudioModel.StudioOrigine.CvOnline);
                    item.StudioCV = cv;
                }
                anag.DatiTitoliStudio.Studi.Add(item);
            }
            foreach (var item in studiCV.Where(x => !studiCezanne.Any(y => y.CodTitolo == x.CodTitolo)))
            {
                anag.DatiTitoliStudio.CVOnline.Add(item);
            }

            CaricaIdentityData(anag.DatiTitoliStudio, sint);
        }

        private static List<StudioModel> CaricaStudiCezanne(AnagraficaModel anag, CezanneDb db, SINTESI1 sint)
        {
            List<StudioModel> listStudi = new List<StudioModel>();

            List<STUPERSONA> studi = HrisHelper.GetStupersona(db, sint.ID_PERSONA);
            foreach (var item in studi)
            {
                StudioModel studio = new StudioModel();
                studio.CodTitoloOld = item.TB_STUDIO.COD_STUDIO;
                studio.CodTitolo = item.TB_STUDIO.COD_STUDIO;
                studio.DesTitolo = item.TB_STUDIO.DES_STUDIO;
                studio.CodTipoTitolo = item.TB_STUDIO.COD_LIVELLOSTUDIO;
                studio.DesTipoTitolo = item.TB_STUDIO.TB_LIVSTUD.DES_LIVELLOSTUDIO;

                studio.CodIstituto = item.COD_ATENEO;
                if (studio.CodIstituto != "999" && item.TB_ATENEO != null)
                    studio.Istituto = item.TB_ATENEO.DES_ATENEO + " - " + item.TB_ATENEO.TB_COMUNE.DES_CITTA + " (" + item.TB_ATENEO.TB_COMUNE.COD_PROV_STATE + ")";
                else if (item.JTUPERSONA != null)
                    studio.Istituto = item.JTUPERSONA.DES_ISTITUTO;

                studio.Voto = item.COD_PUNTEGGIO;

                studio.Cod_TipoPunteggio = item.COD_TIPOPUNTEGGIO;
                if (item.TB_TPPUNT != null)
                    studio.Cod_PunteggioMax = item.TB_TPPUNT.COD_PUNTEGGIOMAX;

                if (item.JTUPERSONA != null)
                {
                    studio.DataInizio.Set(item.JTUPERSONA.DTA_INIZIO);
                    studio.DataInizioStr = item.JTUPERSONA.DTA_INIZIO.HasValue ? item.JTUPERSONA.DTA_INIZIO.Value.ToString("MM/yyyy") : "";
                }
                studio.DataFine.Set(item.DTA_CONSEG);
                studio.DataFineStr = item.DTA_CONSEG.ToString("MM/yyyy");

                if (item.JTUPERSONA != null)
                {
                    studio.CorsoLaurea = item.JTUPERSONA.DES_CORSO;
                    studio.Riconoscimento = item.JTUPERSONA.DES_RICONOSCIMENTO;
                }

                studio.Lode = item.COD_LIVELLOPESO == 1;

                studio.Nota = item.NOT_NOTABREVE;

                studio.CodCitta = item.COD_CITTA;
                if (!String.IsNullOrWhiteSpace(studio.CodCitta))
                    studio.DesCitta = item.TB_COMUNE.DES_CITTA.TitleCase() + " (" + item.TB_COMUNE.COD_PROV_STATE + ")";

                studio.Origine = StudioModel.StudioOrigine.Cezanne;

                AnagraficaManager.CaricaIdentityData(studio, sint);

                listStudi.Add(studio);
            }
            return listStudi;
        }
        private static List<StudioModel> CaricaStudiCV(AnagraficaModel anag, CezanneDb db, SINTESI1 sint)
        {
            List<StudioModel> listStudi = new List<StudioModel>();

            var perseo = new myRai.Data.CurriculumVitae.cv_ModelEntities();
            var istruzione = perseo.TCVIstruzione.Where(m => m.Matricola == anag.Matricola).ToList();
            var specializz = perseo.TCVSpecializz.Where(m => m.Matricola == anag.Matricola).ToList();

            foreach (var item in istruzione)
            {
                StudioModel studio = new StudioModel();
                studio.CodTitoloOld = item.CodTitolo;
                studio.CodTitolo = item.CodTitolo;

                var lvStudio = db.TB_STUDIO.FirstOrDefault(x => x.COD_STUDIO == item.CodTitolo);
                if (lvStudio != null)
                {
                    studio.DesTitolo = lvStudio.DES_STUDIO;
                    studio.CodTipoTitolo = lvStudio.COD_LIVELLOSTUDIO;
                    studio.DesTipoTitolo = lvStudio.TB_LIVSTUD.DES_LIVELLOSTUDIO;
                }
                else
                {
                    var param = new SqlParameter("@param", item.CodTitolo);
                    List<UtenteHelper.CV_DescTitoloLogo> tmp = perseo.Database.SqlQuery<UtenteHelper.CV_DescTitoloLogo>("exec sp_GETDESCTITOLO @param", param).ToList();
                    studio.DesTitolo = tmp[0].DescTitolo;
                    //studio.CodTipoTitolo = item.TB_STUDIO.COD_LIVELLOSTUDIO;
                    studio.DesTipoTitolo = tmp[0].DescTipoTitolo;
                }

                studio.CodIstituto = item.CodIstituto;
                if (studio.CodIstituto != null && studio.CodIstituto != "-1")
                    studio.Istituto = perseo.DAteneoCV.Where(x => x.Codice == item.CodIstituto).Select(x => x.Descrizione).FirstOrDefault();
                else
                    studio.Istituto = item.Istituto ?? "";

                studio.Voto = item.Voto;
                studio.Cod_PunteggioMax = item.Scala;

                studio.DataInizio.Set(item.AnnoInizio, "", 4);
                studio.DataInizioStr = item.AnnoInizio;
                studio.DataFine.Set(item.AnnoFine, "", 4);
                studio.DataFineStr = item.AnnoFine;

                studio.CorsoLaurea = item.CorsoLaurea;
                studio.Riconoscimento = "";

                studio.Lode = item.Lode == "S";

                studio.Nota = item.TitoloTesi;

                studio.DesCitta = item.LocalitaStudi;

                studio.Origine = StudioModel.StudioOrigine.CvOnline;

                AnagraficaManager.CaricaIdentityData(studio, sint);

                listStudi.Add(studio);
            }

            foreach (var item in specializz)
            {
                StudioModel studio = new StudioModel();
                studio.CodTitoloOld = item.TipoSpecial;
                studio.CodTitolo = item.TipoSpecial;

                string codTitolo = item.TipoSpecial;
                if (codTitolo == "999")
                    codTitolo = "005";

                var lvStudio = db.TB_STUDIO.FirstOrDefault(x => x.COD_STUDIO == codTitolo);
                if (lvStudio != null)
                {
                    studio.DesTitolo = lvStudio.DES_STUDIO;
                    studio.CodTipoTitolo = lvStudio.COD_LIVELLOSTUDIO;
                    studio.DesTipoTitolo = lvStudio.TB_LIVSTUD.DES_LIVELLOSTUDIO;
                }
                else
                {
                    var param = new SqlParameter("@param", item.TipoSpecial);
                    List<UtenteHelper.CV_DescTitoloLogo> tmp = perseo.Database.SqlQuery<UtenteHelper.CV_DescTitoloLogo>("exec sp_GETDESCTITOLO @param", param).ToList();
                    studio.DesTitolo = tmp[0].DescTitolo;
                    //studio.CodTipoTitolo = item.TB_STUDIO.COD_LIVELLOSTUDIO;
                    studio.DesTipoTitolo = tmp[0].DescTipoTitolo;
                }

                studio.Istituto = item.Istituto.Trim();

                studio.Voto = item.Voto;

                studio.Cod_PunteggioMax = item.Scala;

                studio.DataInizio.Set(item.DataInizio, "", 4, 2, 2, true);
                studio.DataInizioStr = item.DataInizio;
                studio.DataFine.Set(item.DataFine, "", 4, 2, 2, true);
                studio.DataFineStr = item.DataFine;

                studio.CorsoLaurea = item.Titolo.Trim();
                //studio.Riconoscimento = item.JTUPERSONA.DES_RICONOSCIMENTO;

                studio.Lode = item.Lode == "S";

                studio.DesCitta = item.LocalitaSpecial;

                studio.Origine = StudioModel.StudioOrigine.CvOnline;

                AnagraficaManager.CaricaIdentityData(studio, sint);

                listStudi.Add(studio);
            }

            return listStudi;
        }

        public static bool Save_DatiStudio(StudioModel model, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = GetDb();
            if (db.Database.Connection.Database.ToUpper() == "CZNDB")
                result = Save_DatiStudio(db, model, true, out errorMsg);
            else
            {
                var dbTal = new TalentiaDB();
                result = Save_DatiStudio(dbTal, model, true, out errorMsg);
            }

            return result;
        }
        public static bool Save_DatiStudio(CezanneDb db, StudioModel model, bool saveChanges, out string errorMsg)
        {
            bool result = false;
            errorMsg = null;

            bool isNew = false;
            var recordold = db.STUPERSONA.FirstOrDefault(x => x.ID_PERSONA == model.IdPersona && x.COD_STUDIO == model.CodTitoloOld);
            // var recordExt = db.JTUPERSONA.FirstOrDefault(x => x.ID_PERSONA == model.IdPersona && x.COD_STUDIO == model.CodTitoloOld);
            JTUPERSONA recordExt;
            STUPERSONA record;
            //var recordold = record;
            if (recordold == null || recordold.COD_STUDIO != model.CodTitolo)
            {
                if (recordold == null) isNew = true;
                record = new STUPERSONA();
                record.ID_PERSONA = model.IdPersona;
                record.COD_STUDIO = model.CodTitolo;

                recordExt = new JTUPERSONA();
                recordExt.ID_PERSONA = model.IdPersona;
                recordExt.COD_STUDIO = model.CodTitolo;
            }
            else
            {
                recordExt = recordold.JTUPERSONA;
                record = recordold;
            }
            record.COD_PUNTEGGIO = model.Voto;
            record.COD_TIPOPUNTEGGIO = model.Cod_TipoPunteggio;

            if (!String.IsNullOrWhiteSpace(model.Voto) && short.TryParse(model.Voto, out short votoNum))
                record.COD_PUNTEGGIONUM = votoNum;
            else
                record.COD_PUNTEGGIONUM = 0;

            record.COD_CITTA = model.CodCitta;
            if (model.CodTipoTitolo >= 70 && model.CodTipoTitolo <= 80)
            {
                record.COD_ATENEO = model.CodIstituto;
            }
            else
            {
                record.COD_ATENEO = null;
            }
            record.NOT_NOTABREVE = model.Nota;

            DateTime dateTime;
            try
            {
                dateTime = model.DataFineStr.PadLeft(7, '0').ToDateTime("MM/yyyy");
            }
            catch
            {
                dateTime = model.DataFineStr.ToDateTime("M/yyyy");
            }
            record.DTA_CONSEG = dateTime;

            if (model.Lode)
                record.COD_LIVELLOPESO = 1;
            else
                record.COD_LIVELLOPESO = null;

            recordExt.DES_CORSO = model.CorsoLaurea;
            if (model.DataInizioStr != null)
            {
                try
                {
                    dateTime = model.DataInizioStr.ToDateTime("MM/yyyy");
                }
                catch
                {
                    dateTime = model.DataInizioStr.ToDateTime("M/yyyy");
                }
                recordExt.DTA_INIZIO = dateTime;
            }

            recordExt.DES_RICONOSCIMENTO = model.Riconoscimento;
            recordExt.DES_ISTITUTO = model.Istituto;
            recordExt.COD_LIVELLOSTUDIO = model.CodTipoTitolo;
            recordExt.DES_LIVELLOSTUDIO = model.DesTipoTitolo;



            CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
            record.COD_USER = codUser;
            record.COD_TERMID = codTermId;
            record.TMS_TIMESTAMP = tmsTimestamp;
            record.JTUPERSONA = recordExt;
            if (isNew)
                db.STUPERSONA.Add(record);
            else
            {
                if (model.CodTitoloOld != model.CodTitolo)
                {
                    db.JTUPERSONA.Remove(recordold.JTUPERSONA);
                    db.STUPERSONA.Remove(recordold);
                    db.STUPERSONA.Add(record);
                }
            }
            if (saveChanges)
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            else result = true;
            //  result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            HrisHelper.LogOperazione("AggiornamentoTitoliStudio", (isNew ? "Aggiunta" : "Modifica") + $" titoli di studio ID_PERS:{model.IdPersona} MATR:{model.Matricola}", result, errorMsg, null, null, Newtonsoft.Json.JsonConvert.SerializeObject(model));

            return result;
        }
        public static bool Save_DatiStudio(TalentiaDB db, StudioModel model, bool saveChanges, out string errorMsg)
        {
            bool result = false;
            errorMsg = null;

            bool isNew = false;
            var record = db.STUPERSONA.FirstOrDefault(x => x.ID_PERSONA == model.IdPersona && x.COD_STUDIO == model.CodTitolo);
            if (record == null)
            {
                isNew = true;
                record = new myRaiDataTalentia.STUPERSONA();
                record.ID_PERSONA = model.IdPersona;
                record.COD_STUDIO = model.CodTitolo;
            }

            record.COD_PUNTEGGIO = model.Voto;
            record.COD_TIPOPUNTEGGIO = model.Cod_TipoPunteggio;
            record.COD_LIVELLOSTUDIO = model.CodTipoTitolo;
            record.DES_LIVELLOSTUDIO = model.DesTipoTitolo;
            record.COD_CITTA = model.CodCitta;
            if (model.CodTipoTitolo >= 70 && model.CodTipoTitolo <= 80)
            {
                record.COD_ATENEO = model.CodIstituto;
            }
            else
            {
                record.COD_ATENEO = null;
            }
            record.NOT_NOTABREVE = model.Nota;

            DateTime dateTime = model.DataFineStr.ToDateTime("MM/yyyy");
            record.DTA_CONSEG = dateTime;

            if (model.Lode)
                record.COD_LIVELLOPESO = 1;
            else
                record.COD_LIVELLOPESO = null;

            record.DES_CORSO = model.CorsoLaurea;
            record.DTA_INIZIO = model.DataInizioStr.ToDateTime("MM/yyyy");
            record.DES_RICONOSCIMENTO = model.Riconoscimento;
            record.DES_ISTITUTO = model.Istituto;
            record.IND_COMPANYFUNDED = "N";
            record.IND_VERIFIED = "N";

            CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
            record.COD_USER = codUser;
            record.COD_TERMID = codTermId;
            record.TMS_TIMESTAMP = tmsTimestamp;

            if (isNew)
                db.STUPERSONA.Add(record);
            if (saveChanges)
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "HRIS - Titoli di studio ");
            else result = true;
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            return result;
        }


        public static bool Delete_DatiStudio(int idPersona, string codStudio, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";
            var db = GetDb();

            var record = db.STUPERSONA.FirstOrDefault(x => x.ID_PERSONA == idPersona && x.COD_STUDIO == codStudio);
            var recordExt = db.JTUPERSONA.FirstOrDefault(x => x.ID_PERSONA == idPersona && x.COD_STUDIO == codStudio);
            if (record != null)
            {
                if (recordExt != null)
                    db.JTUPERSONA.Remove(recordExt);
                db.STUPERSONA.Remove(record);

                if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    result = true;
                else
                    errorMsg = "Errore durante la cancellazione";
            }
            else
            {
                errorMsg = "Titolo di studio non trovato";
            }

            HrisHelper.LogOperazione("CancellazioneTitoliStudio", $"Cancellazione titoli di studio ID_PERS:{idPersona}", result, errorMsg, null, null, record);

            return result;
        }

        public static List<TB_LIVSTUD> GetTipiStudi()
        {
            List<TB_LIVSTUD> result = new List<TB_LIVSTUD>();

            var db = GetDb();
            result.AddRange(db.TB_LIVSTUD.Where(x => x.QTA_ORDINE != 1));

            return result;
        }
        public static List<TB_STUDIO> GetStudi(int cod_livello)
        {
            List<TB_STUDIO> result = new List<TB_STUDIO>();

            var db = GetDb();

            var tmpQuery = db.TB_STUDIO.Where(x => x.IND_WEBVISIBLE == "Y").AsQueryable();//.Where(x => x.QTA_ORDINE!= 1);

            if (cod_livello > 0)
                tmpQuery = tmpQuery.Where(x => x.COD_LIVELLOSTUDIO == cod_livello);

            result.AddRange(tmpQuery.OrderBy(x => x.DES_STUDIO));

            return result;
        }
        public static List<TB_TPPUNT> GetScaleVoti()
        {
            List<TB_TPPUNT> result = new List<TB_TPPUNT>();

            var db = GetDb();
            result.AddRange(db.TB_TPPUNT);

            return result;
        }
        public static List<TB_ATENEO> GetAtenei()
        {
            List<TB_ATENEO> result = new List<TB_ATENEO>();

            var db = GetDb();
            result.AddRange(db.TB_ATENEO);

            return result;
        }
        #endregion

        public static bool Tracciato_Residenza(int idEvento, string codEvento, RESIDENZA recordInput)
        {
            string fileNameTemplate = "CMANARES_{0:yyMMdd}_{0:HHmmss}_{1}.TXT";
            string pathDest = HrisHelper.GetParametro<string>(HrisParam.PathModificaResidenza);

            /*
                INFO			ORDINE	POS	LUNGHEZZA
                -------------------------------------
                MATRICOLA		10		1	6
                SCADENZA		20		7	8
                INDIRIZZO		30		15	34
                CAP				40		49	5
                CITTA			50		54	28
                PROVINCIA		60		82	2
                SIGLA NAZIONE	70		84	3
                CODICE_REGIONE	80		87	4
                DATA_INIZIO		90		91	8
                UTENTE			100		99	8
                CODICE_CITTA	110		107	4
                DATA_ASSUNZIONE	120		111	8
                DATA_INSER		130		119	8
                ORARIO_INSER	140		127	6
             */

            var db = GetDb();
            SINTESI1 sint = db.SINTESI1.FirstOrDefault(x => x.ID_PERSONA == recordInput.ID_PERSONA);
            TB_COMUNE comune = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA.Trim() == recordInput.COD_CITTA.Trim());
            JB_COMUNE extComune = db.JB_COMUNE.FirstOrDefault(x => x.COD_CITTA.Trim() == recordInput.COD_CITTA.Trim());

            string recordLine = "";
            recordLine += CezanneHelper.AddToken(sint.COD_MATLIBROMAT, 6);
            recordLine += CezanneHelper.AddToken(recordInput.DTA_FINE.ToString("yyyyMMdd"), 8);
            recordLine += CezanneHelper.AddToken(recordInput.DES_INDIRRESID, 34);
            recordLine += CezanneHelper.AddToken(recordInput.CAP_CAPRESID, 5);
            recordLine += CezanneHelper.AddToken((codEvento == "RAI011" ? "#" : comune.DES_CITTA), 28);
            recordLine += CezanneHelper.AddToken(comune.COD_PROV_STATE == "EST" ? "  " : comune.COD_PROV_STATE, 2);
            recordLine += CezanneHelper.AddToken(comune.COD_SIGLANAZIONE, 3);
            recordLine += CezanneHelper.AddToken(extComune.COD_REGIONE, 4);
            recordLine += CezanneHelper.AddToken(recordInput.DTA_INIZIO.ToString("yyyyMMdd"), 8);
            recordLine += CezanneHelper.AddToken(recordInput.COD_USER.Replace("RAI\\P", ""), 8);
            recordLine += CezanneHelper.AddToken(comune.COD_CITTA, 4);
            recordLine += CezanneHelper.AddToken((codEvento == "RAI011" ? "#" : sint.DTA_INIZIO_CR.Value.ToString("yyyyMMdd")), 8);
            recordLine += CezanneHelper.AddToken(recordInput.TMS_TIMESTAMP.ToString("yyyyMMdd"), 8);
            recordLine += CezanneHelper.AddToken(recordInput.TMS_TIMESTAMP.ToString("HHmmss"), 6);

            string path = System.IO.Path.Combine(pathDest, String.Format(fileNameTemplate, DateTime.Now, idEvento));
            TaskHelper.AddFileWriterTask(path, recordLine, true, HrisParam.CredenzialiServerCezanne);
            HrisHelper.LogOperazione("ModificaResidenza", String.Format("ID_PERS:{2} MATR:{3}\nNome file: {0}\nContenuto:{1}", path, recordLine, sint.ID_PERSONA, sint.COD_MATLIBROMAT), true, "Task scritto correttamente");
            return true;

            //try
            //{
            //    string[] credenziali = HrisHelper.GetParametri<string>(HrisParam.CredenzialiServerCezanne);
            //    ImpersonationHelper.Impersonate(credenziali[2], credenziali[0], credenziali[1], delegate
            //    {
            //        System.IO.File.WriteAllText(path, recordLine);
            //    });
            //    HrisHelper.LogOperazione("ModificaResidenza", String.Format("ID_PERS:{2} MATR:{3}\nNome file: {0}\nContenuto:{1}", path, recordLine, sint.ID_PERSONA, sint.COD_MATLIBROMAT), true, "File scritto correttamente");
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    HrisHelper.LogOperazione("ModificaResidenza", String.Format("ID_PERS:{2} MATR:{3}\nNome file: {0}\nContenuto:{1}", path, recordLine, sint.ID_PERSONA, sint.COD_MATLIBROMAT), false, "Errore su scrittura file", null, ex);
            //    return false;
            //}
        }

        public static bool Tracciato_Domicilio(string matricola, TipoAnaVar tipoAnaVar, myRaiData.Incentivi.ANAGPERS anag, myRaiData.Incentivi.CITTAD cittad, int idEvento, DateTime dtaInizio)
        {
            var db = GetDb();
            AnaVar variazione = new AnaVar();
            variazione.Matricola = matricola;
            if (tipoAnaVar == TipoAnaVar.Anagrafica || tipoAnaVar == TipoAnaVar.Immatricolazione)
            {
                TB_COMUNE cittaNasc = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == anag.COD_CITTA);

                string matr_collab = HrisHelper.GetCodMatColl(db, anag.ID_PERSONA) ?? "";
                string cod_Casagit = HrisHelper.GetCodCasaGit(db, anag.ID_PERSONA) ?? "";

                if (cittad == null)
                    cittad = db.CITTAD.FirstOrDefault(x => x.ID_PERSONA == anag.ID_PERSONA && x.DTA_INIZIO <= DateTime.Now && x.DTA_FINE > DateTime.Now);

                variazione.Nominativo = anag.DES_COGNOMEPERS + "\\" + anag.DES_NOMEPERS + (!String.IsNullOrWhiteSpace(anag.DES_COGNOMEACQ) ? "\\" + anag.DES_COGNOMEACQ : "");
                variazione.DtaNascita = anag.DTA_NASCITAPERS;
                variazione.Sesso = anag.COD_SESSO;
                variazione.StatoCivile = anag.COD_STCIV;
                variazione.CodCittad = cittad != null && !String.IsNullOrWhiteSpace(cittad.COD_CITTADPERS) ? cittad.COD_CITTADPERS : "000";
                variazione.MatColl = matr_collab;

                variazione.CF = anag.CSF_CFSPERSONA;
                variazione.CittaNasc = cittaNasc.DES_CITTA;
                variazione.ProvNasc = cittaNasc.COD_PROV_STATE;

                variazione.Casagit = cod_Casagit;
                variazione.TitoloOnor = anag.DES_TITOLOONOR ?? "";
            }

            if (tipoAnaVar == TipoAnaVar.Domicilio || tipoAnaVar == TipoAnaVar.Immatricolazione)
            {
                TB_COMUNE cittaDom = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == anag.COD_CITTADOM);

                variazione.IndirizzoDom = anag.DES_INDIRDOM ?? "";
                variazione.CapDom = anag.CAP_CAPDOM ?? "";
                variazione.CittaDom = cittaDom != null ? cittaDom.DES_CITTA : "";
                variazione.ProvDom = cittaDom != null ? cittaDom.COD_PROV_STATE : "";
            }

            variazione.IdEvento = idEvento;
            variazione.DtaInizio = dtaInizio;

            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);
            variazione.CodUser = codUser;
            variazione.Timestamp = tms;

            return Tracciato_CMANAVAR(variazione);
        }

        public static bool Tracciato_Domicilio(string matricola, TipoAnaVar tipoAnaVar, myRaiDataTalentia.ANAGPERS anag, myRaiDataTalentia.CITTAD cittad, int idEvento, DateTime dtaInizio)
        {
            var db = new myRaiDataTalentia.TalentiaEntities();
            AnaVar variazione = new AnaVar();
            variazione.Matricola = matricola;
            if (tipoAnaVar == TipoAnaVar.Anagrafica || tipoAnaVar == TipoAnaVar.Immatricolazione)
            {
                myRaiDataTalentia.TB_COMUNE cittaNasc = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == anag.COD_CITTA);

                string matr_collab = HrisHelper.GetCodMatColl(db, anag.ID_PERSONA) ?? "";
                string cod_Casagit = HrisHelper.GetCodCasaGit(db, anag.ID_PERSONA) ?? "";

                if (cittad == null)
                    cittad = db.CITTAD.FirstOrDefault(x => x.ID_PERSONA == anag.ID_PERSONA && x.DTA_INIZIO <= DateTime.Now && x.DTA_FINE > DateTime.Now);

                variazione.Nominativo = anag.DES_COGNOMEPERS + "\\" + anag.DES_NOMEPERS + (!String.IsNullOrWhiteSpace(anag.DES_COGNOMEACQ) ? "\\" + anag.DES_COGNOMEACQ : "");
                variazione.DtaNascita = anag.DTA_NASCITAPERS;
                variazione.Sesso = anag.COD_SESSO;
                variazione.StatoCivile = anag.COD_STCIV;
                variazione.CodCittad = cittad != null && !String.IsNullOrWhiteSpace(cittad.COD_CITTADPERS) ? cittad.COD_CITTADPERS : "000";
                variazione.MatColl = matr_collab;

                variazione.CF = anag.CSF_CFSPERSONA;
                variazione.CittaNasc = cittaNasc.DES_CITTA;
                variazione.ProvNasc = cittaNasc.COD_PROV_STATE;

                variazione.Casagit = cod_Casagit;
                variazione.TitoloOnor = anag.DES_TITOLOONOR ?? "";
            }

            if (tipoAnaVar == TipoAnaVar.Domicilio || tipoAnaVar == TipoAnaVar.Immatricolazione)
            {
                myRaiDataTalentia.TB_COMUNE cittaDom = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == anag.COD_CITTADOM);

                variazione.IndirizzoDom = anag.DES_INDIRDOM ?? "";
                variazione.CapDom = anag.CAP_CAPDOM ?? "";
                variazione.CittaDom = cittaDom != null ? cittaDom.DES_CITTA : "";
                variazione.ProvDom = cittaDom != null ? cittaDom.COD_PROV_STATE : "";
            }

            variazione.IdEvento = idEvento;
            variazione.DtaInizio = dtaInizio;
            variazione.CodUser = anag.COD_USER;
            variazione.Timestamp = anag.TMS_TIMESTAMP;

            return Tracciato_CMANAVAR(variazione);
        }

        private static bool Tracciato_CMANAVAR(AnaVar variazione)
        {
            string recordLine = "";
            recordLine += CezanneHelper.AddToken(variazione.Matricola, 6);
            recordLine += CezanneHelper.AddToken(variazione.Nominativo, 30);
            recordLine += CezanneHelper.AddToken("", 19);
            recordLine += CezanneHelper.AddToken(variazione.DtaNascita.HasValue ? variazione.DtaNascita.Value.ToString("ddMMyy") : "", 6);
            recordLine += CezanneHelper.AddToken("", 12);
            recordLine += CezanneHelper.AddToken(variazione.Sesso == "M" ? "9" : variazione.Sesso == "F" ? "8" : "", 1);
            recordLine += CezanneHelper.AddToken("", 1);
            recordLine += CezanneHelper.AddToken(String.IsNullOrWhiteSpace(variazione.StatoCivile) || variazione.StatoCivile == "---" ? "" : variazione.StatoCivile, 1);
            recordLine += CezanneHelper.AddToken("", 2);
            recordLine += CezanneHelper.AddToken(variazione.ForzaCF, 1); //FORZA_CF <- select CASE ISNULL(IND_FORZCF,'N') WHEN 'N' THEN '#' ELSE '1' END FROM CMINFOANAG where  id_cmevento=§
            recordLine += CezanneHelper.AddToken("", 85);
            recordLine += CezanneHelper.AddToken(variazione.CodCittad, 3);
            recordLine += CezanneHelper.AddToken("", 6);
            recordLine += CezanneHelper.AddToken(variazione.MatColl, 6); //cod_matcoll <- da sistemi collaboratori
            recordLine += CezanneHelper.AddToken(variazione.CF, 16);
            recordLine += CezanneHelper.AddToken(variazione.CittaNasc, 30);
            recordLine += CezanneHelper.AddToken(variazione.ProvNasc == "EST" ? "#" : " " + variazione.ProvNasc + " ", 4);
            //Il carattere ° non viene gestito correttamente da CICS
            recordLine += CezanneHelper.AddToken((variazione.IndirizzoDom ?? "").Replace('°', ' '), 34);
            recordLine += CezanneHelper.AddToken(variazione.CapDom, 5);
            recordLine += CezanneHelper.AddToken(variazione.CittaDom, 28);
            recordLine += CezanneHelper.AddToken(variazione.ProvDom == "EST" ? "#" : " " + variazione.ProvDom + " ", 4);
            recordLine += CezanneHelper.AddToken(variazione.Casagit, 8);//CodCasagit <-iscrizione cassa integ. giornalisti
            recordLine += CezanneHelper.AddToken("", 56);
            recordLine += CezanneHelper.AddToken(variazione.TitoloOnor, 5);
            recordLine += CezanneHelper.AddToken(variazione.DtaInizio.ToString("yyyyMMdd"), 8); //Data inizio <-
            recordLine += CezanneHelper.AddToken(variazione.CodUser.Replace("RAI\\", ""), 8);
            recordLine += CezanneHelper.AddToken(variazione.Timestamp.ToString("yyyyMMdd"), 8);
            recordLine += CezanneHelper.AddToken(variazione.Timestamp.ToString("HHmmss"), 6);
            recordLine += CezanneHelper.AddToken("", 1);

            recordLine = recordLine.ToUpper();

            string fileNameTemplate = "CMANAVAR_{0:yyMMdd}_{0:HHmmss}_{1}.TXT";
            string pathDest = HrisHelper.GetParametro<string>(HrisParam.PathModificaAnagrafica);

            string path = System.IO.Path.Combine(pathDest, String.Format(fileNameTemplate, DateTime.Now, variazione.IdEvento));

            TaskHelper.AddFileWriterTask(path, recordLine, true, HrisParam.CredenzialiServerCezanne);
            HrisHelper.LogOperazione("ModificaAnagrafica", String.Format("MATR:{2}\nNome file: {0}\nContenuto:{1}", path, recordLine, variazione.Matricola), true, "Task scritto correttamente");
            return true;
            //try
            //{
            //    string[] credenziali = HrisHelper.GetParametri<string>(HrisParam.CredenzialiServerCezanne);
            //    ImpersonationHelper.Impersonate(credenziali[2], credenziali[0], credenziali[1], delegate
            //    {
            //        System.IO.File.WriteAllText(path, recordLine);
            //    });
            //    HrisHelper.LogOperazione("ModificaAnagrafica", String.Format("MATR:{2}\nNome file: {0}\nContenuto:{1}", path, recordLine, variazione.Matricola), true, "File scritto correttamente");
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    HrisHelper.LogOperazione("ModificaAnagrafica", String.Format("MATR:{2}\nNome file: {0}\nContenuto:{1}", path, recordLine, variazione.Matricola), false, "Errore su scrittura file", null, ex);
            //    return false;
            //}
        }
        public static bool Tracciato_TEX2929(EventoModel evento)
        {
            bool result = false;
            char filler = ' ';
            string sep = " ";

            string tracciato = "";
            //Matricola
            tracciato += evento.Matricola + sep;
            //Sede
            tracciato += CezanneHelper.AddToken(evento.Tipo == TipoEvento.Sede ? evento.Codice : "", 3, filler, sep);
            //Servizio
            tracciato += CezanneHelper.AddToken(evento.Tipo == TipoEvento.Servizio ? evento.Codice : "", 2, filler, sep);
            //Sezione
            tracciato += CezanneHelper.AddToken(evento.Tipo == TipoEvento.Sezione ? evento.Codice : evento.Tipo == TipoEvento.Servizio ? evento.CodiceSec : "", 9, filler, sep);
            //Mansione
            tracciato += CezanneHelper.AddToken(evento.Tipo == TipoEvento.Mansione ? evento.Codice : "", 4, filler, sep);
            //Categoria
            tracciato += CezanneHelper.AddToken(evento.Tipo == TipoEvento.Qualifica ? evento.Codice : "", 3, filler, sep);
            //Assicurazione infortuni
            tracciato += CezanneHelper.AddToken("", 1, filler, sep);
            //Tipo variazione TE
            tracciato += CezanneHelper.AddToken(evento.CodiceEvento, 3, filler, sep, CezanneHelper.TokenPad.Left);
            //Decorrenza
            tracciato += CezanneHelper.AddToken(evento.DataInizio.ToString("yyyyMMdd"), 8, filler, sep);
            //Nominativo
            tracciato += CezanneHelper.AddToken(evento.Nominativo, 30, filler, sep);
            //Filler finale
            tracciato += CezanneHelper.AddToken("", 1, filler);

            //@# Dove lo scrive?

            return result;
        }

        public static List<StatoRapporto_API_chain> AllineaAPI_StatoRapporto(string matricola)
        {
            var dbczn = new IncentiviEntities();
            var dbTal = new myRaiDataTalentia.TalentiaEntities();

            var StatoRapportoList = dbTal.XR_STATO_RAPPORTO.Where(x => x.MATRICOLA == matricola &&
                                                 x.DTA_INIZIO <= DateTime.Now && x.DTA_FINE >= DateTime.Now)
                                    .OrderBy(x => x.TMS_TIMESTAMP).ToList();

            var SR_PrimoInserimento = StatoRapportoList.Where(x => x.ID_STATO_RAPPORTO_ORIG == null).FirstOrDefault();
            if (SR_PrimoInserimento == null)
                return null;

            List<StatoRapporto_API_chain> LSR = new List<StatoRapporto_API_chain>();
            LSR.Add(new StatoRapporto_API_chain()
            {
                order = 1,
                TipologiaAPI = "I",
                StatoRapporto = SR_PrimoInserimento
            });



            int IDSR = SR_PrimoInserimento.ID_STATO_RAPPORTO;

            for (int i = 2; i < 100; i++)
            {
                var SR = new myRaiDataTalentia.XR_STATO_RAPPORTO();

                SR = dbTal.XR_STATO_RAPPORTO.Where(x => x.ID_STATO_RAPPORTO_ORIG == IDSR).FirstOrDefault();
                if (SR == null)
                    return LSR;
                else
                {
                    IDSR = SR.ID_STATO_RAPPORTO;
                    LSR.Add(new StatoRapporto_API_chain()
                    {
                        order = i,
                        StatoRapporto = SR
                    });
                }
            }

            if (LSR.Count > 1)
            {
                var FirstItem = LSR.Where(x => x.order == 1).FirstOrDefault();
                var LastItem = LSR.OrderByDescending(x => x.order).FirstOrDefault();
                if (LastItem.StatoRapporto.DTA_FINE < FirstItem.StatoRapporto.DTA_FINE)
                {

                }
            }

            return LSR;
        }

        public static void InserisciApiAnnulla(int idRecord)
        {
            var dbczn = new IncentiviEntities();
            XR_SW_API api = null;


            XR_SW_API row = dbczn.XR_SW_API.Where(x => x.ID_STATORAPPORTO == idRecord && x.TIPOLOGIA_API == "I").FirstOrDefault();
            if (row != null)
            {
                api = new XR_SW_API()
                {
                    DATA_CREAZIONE = DateTime.Now,
                    ID_STATORAPPORTO = idRecord,
                    MATRICOLA = row.MATRICOLA,
                    MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                    TIPOLOGIA_API = "A",
                    PERIODO_DAL = row.PERIODO_DAL,
                    PERIODO_AL = row.PERIODO_AL,
                    ID_RIFERIMENTO_SW_API = row.ID
                };
            }
            else
            {
                row = dbczn.XR_SW_API.Where(x => x.ID_STATORAPPORTO == idRecord && x.TIPOLOGIA_API == "M").FirstOrDefault();
                if (row != null)
                {
                    api = new XR_SW_API()
                    {
                        DATA_CREAZIONE = DateTime.Now,
                        ID_STATORAPPORTO = idRecord,
                        MATRICOLA = row.MATRICOLA,
                        MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                        TIPOLOGIA_API = "A",
                        PERIODO_DAL = row.PERIODO_DAL,
                        PERIODO_AL = row.PERIODO_AL,
                        ID_RIFERIMENTO_SW_API = row.ID
                    };
                }
            }

            if (api != null)
            {
                dbczn.XR_SW_API.Add(api);
                dbczn.SaveChanges();
                myRaiHelper.Logger.LogAzione(
                    new MyRai_LogAzioni()
                    {
                        operazione = "InvioAPI Annulla",
                        descrizione_operazione = "idStatoRapporto:" + idRecord + " id api :" + api.ID
                    });
            }
        }

        public static int InserisciApiNuova(myRaiDataTalentia.XR_STATO_RAPPORTO Record, string matricolaOperatore = null)
        {
            var dbczn = new IncentiviEntities();
            XR_SW_API api = new XR_SW_API()
            {
                DATA_CREAZIONE = DateTime.Now,
                ID_STATORAPPORTO = Record.ID_STATO_RAPPORTO,
                MATRICOLA = Record.MATRICOLA,
                MATRICOLA_OPERATORE = matricolaOperatore == null ? CommonHelper.GetCurrentUserMatricola() : matricolaOperatore,
                TIPOLOGIA_API = "I",
                PERIODO_DAL = Record.DTA_INIZIO,
                PERIODO_AL = Record.DTA_FINE
            };
            dbczn.XR_SW_API.Add(api);
            dbczn.SaveChanges();
            myRaiHelper.Logger.LogAzione(
                    new MyRai_LogAzioni()
                    {
                        operazione = "InvioAPI nuova",
                        descrizione_operazione = "idStatoRapporto:" + Record.ID_STATO_RAPPORTO + " id api :" + api.ID
                    });
            return api.ID;
        }
        public static void InserisciApiModifica(myRaiDataTalentia.XR_STATO_RAPPORTO Record, string matricola, string sceltaModRec)
        {
            var dbczn = new IncentiviEntities();
            //se c'e gia una API inserita
            var row = dbczn.XR_SW_API.Where(x => x.ID_STATORAPPORTO == Record.ID_STATO_RAPPORTO ||
                                            x.ID_STATORAPPORTO == Record.ID_STATO_RAPPORTO_ORIG).FirstOrDefault();

            var RowCreazione = dbczn.XR_SW_API.Where(x => x.MATRICOLA == matricola && x.TIPOLOGIA_API == "I")
                .FirstOrDefault();
            bool ModificaDiventaRecesso = false;

            if (RowCreazione != null && RowCreazione.PERIODO_AL > Record.DTA_FINE)
            {
                ModificaDiventaRecesso = true;
            }

            XR_SW_API api;
            if (row != null)
            {
                api = new XR_SW_API()
                {
                    DATA_CREAZIONE = DateTime.Now,
                    ID_STATORAPPORTO = Record.ID_STATO_RAPPORTO,
                    MATRICOLA = Record.MATRICOLA,
                    MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                    TIPOLOGIA_API = (ModificaDiventaRecesso ? "R" : "M"),
                    PERIODO_DAL = Record.DTA_INIZIO,
                    PERIODO_AL = Record.DTA_FINE,
                    ID_RIFERIMENTO_SW_API = row.ID
                };
            }
            else
            {
                api = new XR_SW_API()
                {
                    DATA_CREAZIONE = DateTime.Now,
                    ID_STATORAPPORTO = Record.ID_STATO_RAPPORTO,
                    MATRICOLA = Record.MATRICOLA,
                    MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                    TIPOLOGIA_API = "I",
                    PERIODO_DAL = Record.DTA_INIZIO,
                    PERIODO_AL = Record.DTA_FINE,
                };
                if (sceltaModRec == "R")
                {
                    var rowPrec = new myRaiDataTalentia.TalentiaEntities().XR_STATO_RAPPORTO.Where(x => x.ID_STATO_RAPPORTO == Record.ID_STATO_RAPPORTO_ORIG)
                            .FirstOrDefault();
                    if (rowPrec != null)
                        api.PERIODO_AL = rowPrec.DTA_FINE;
                }
            }
            dbczn.XR_SW_API.Add(api);
            dbczn.SaveChanges();
            if (sceltaModRec == "R")
            {
                var apiRec = new XR_SW_API()
                {
                    DATA_CREAZIONE = DateTime.Now,
                    ID_STATORAPPORTO = Record.ID_STATO_RAPPORTO,
                    MATRICOLA = Record.MATRICOLA,
                    MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola(),
                    TIPOLOGIA_API = "R",
                    PERIODO_DAL = Record.DTA_INIZIO,
                    PERIODO_AL = Record.DTA_FINE,
                    ID_RIFERIMENTO_SW_API = api.ID
                };
                dbczn.XR_SW_API.Add(apiRec);
                dbczn.SaveChanges();
            }


            myRaiHelper.Logger.LogAzione(
                    new MyRai_LogAzioni()
                    {
                        operazione = "InvioAPI nuova",
                        descrizione_operazione = "idStatoRapporto:" + Record.ID_STATO_RAPPORTO + " id api:" + api.ID
                    });
        }

        public static bool Save_StatiRapporto(EventoModel model, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new TalentiaDB();

            myRaiDataTalentia.XR_STATO_RAPPORTO Record = GestisciRecordStato(model, db, "N");

            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "Salvataggio stato rapporto"))
            {
                result = true;
            }
            else
            {
                result = false;
                errorMsg = "Errore durante il salvataggio";
            }
            if (result == true && model.InviaApi == "1")
            {

                if (model.IdEvento == 0)
                {
                    if (!EsisteAPInuovaPerMatricola(Record.MATRICOLA, Record.DTA_INIZIO, Record.DTA_FINE))
                    {
                        InserisciApiNuova(Record);
                    }
                }
                else
                {
                    InserisciApiModifica(Record, model.Matricola, model.SceltaModRec);
                }
            }


            HrisHelper.LogOperazione("AggiornamentoStatoRapporto", $"Aggiornamento stato rapporto ID_PERS:{model.IdPersona} MATR:{model.Matricola}", result, errorMsg, null, null, Newtonsoft.Json.JsonConvert.SerializeObject(model));

            return result;
        }
        public static bool EsisteAPInuovaPerMatricola(string matricola, DateTime D1, DateTime D2)
        {
            var db = new IncentiviEntities();
            var rowI = db.XR_SW_API.Where(x => x.TIPOLOGIA_API == "I" && x.MATRICOLA == matricola &&
            x.PERIODO_DAL == D1 && x.PERIODO_AL == D2).FirstOrDefault();
            if (rowI == null)
                return false;  //non esistono I
            else
            {
                var ListRowA = db.XR_SW_API.Where(x => x.TIPOLOGIA_API == "A" && x.MATRICOLA == matricola).ToList();
                if (ListRowA.Any(x => x.DATA_CREAZIONE > rowI.DATA_CREAZIONE))
                    return false; // c'e una I e ci sono Annulla successive, non esiste
                else
                    return true; // c'e una I e non ci sono Annulla successive, esiste

            }
        }

        public static bool Delete_StatoRapporto(int input, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new TalentiaDB();
            var record = db.XR_STATO_RAPPORTO.Find(input);
            if (record != null)
            {
                string dtaInizio = record.DTA_INIZIO.ToString("dd/MM/yyyy");
                string dtaFine = record.DTA_FINE.ToString("dd/MM/yyyy");

                var infos = record.XR_STATO_RAPPORTO_INFO.ToList();
                foreach (var item in infos)
                    db.XR_STATO_RAPPORTO_INFO.Remove(item);


                db.XR_STATO_RAPPORTO.Remove(record);
                //record.VALID_DTA_END = DateTime.Now;
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "Eliminazione rapporto");
                if (result && record.DTA_NOTIF_ENTE == null)
                {
                    var dbDG = new digiGappEntities();
                    string pMatr = "P" + record.MATRICOLA;
                    string key = "HRIS_AttivaSW_" + input;
                    var recImp = dbDG.MyRai_Importazioni.FirstOrDefault(x => x.Matricola == "ImportaProrogheSWDaCSV"
                                                                    && x.Tabella == "ProrogaModuloSmartWorking2020"
                                                                    && x.Parametro1 == pMatr
                                                                    && x.Parametro4 == dtaInizio
                                                                    && x.Parametro5 == dtaFine
                                                                    && x.Parametro15 == key);

                    if (recImp != null)
                    {
                        if (!String.IsNullOrWhiteSpace(recImp.Parametro13))
                            recImp.Parametro15 += " - ELIMINATO SU TALENTIA";//E' già stato inviato quindi non più possibile rimuoverlo
                        else
                            dbDG.MyRai_Importazioni.Remove(recImp);

                        bool canImp = DBHelper.Save(dbDG, CommonHelper.GetCurrentUserMatricola());
                    }
                }
                else
                    errorMsg = "Impossibile cancellare il record";

                HrisHelper.LogOperazione("AggiornamentoStatoRapporto", $"Eliminazione stato rapporto ID_PERS:{record.ID_PERSONA} MATR:{record.MATRICOLA} da {dtaInizio} a {dtaFine}", result, errorMsg, null, null,
                    Newtonsoft.Json.JsonConvert.SerializeObject(record, new Newtonsoft.Json.JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore }));
            }


            return result;
        }

        public static myRaiDataTalentia.XR_STATO_RAPPORTO GestisciRecordStato(EventoModel model, TalentiaDB db, string tipoAutomazione)
        {
            myRaiDataTalentia.XR_STATO_RAPPORTO oldRecord = null;
            myRaiDataTalentia.XR_STATO_RAPPORTO newRecord = null;

            if (model.TipologiaAccordo.Trim().ToUpper() == "DEROGA" &&
                model.SWDeroga_Opzione.Trim().ToUpper() == "OPZIONEB")
            {
                model.NumeroGiorniMax = 31;
            }

            DateTime? dataInizio = null;
            int? idOrig = null;
            string _intTipoAutomazione = tipoAutomazione;
            CezanneHelper.GetCampiFirma(out string cod_user, out string cod_termid, out DateTime tms_timestamp);
            bool addNew = true;

            string codice = model.Codice == "SW" && model.IdEvento == 0 ? (model.TipologiaAccordo == "Unilaterale" || !model.MostraProposta ? "SW" : "SW_P") : model.Codice;

            if (model.IdEvento == 0)
            {
                //oldRecord = db.XR_STATO_RAPPORTO.Where(x => x.ID_PERSONA == model.IdPersona && x.COD_STATO_RAPPORTO == codice && x.VALID_DTA_END == null).OrderByDescending(x => x.DTA_FINE).FirstOrDefault();
                //if (oldRecord != null && (oldRecord.DTA_FINE == model.DataInizio || oldRecord.DTA_FINE.AddDays(1) == model.DataInizio))
                //{
                //    oldRecord.VALID_DTA_END = DateTime.Now;

                //    dataInizio = oldRecord.DTA_INIZIO;
                //    idOrig = oldRecord.ID_STATO_RAPPORTO;

                //    oldRecord.COD_USER = cod_user;
                //    oldRecord.COD_TERMID = cod_termid;
                //    oldRecord.TMS_TIMESTAMP = tms_timestamp;
                //}
                //else
                {
                    dataInizio = model.DataInizio;
                }
            }
            else
            {
                oldRecord = db.XR_STATO_RAPPORTO.FirstOrDefault(x => x.ID_STATO_RAPPORTO == model.IdEvento);

                if (model.Codice != "SW_N" && (model.DataInizio != oldRecord.DTA_INIZIO || model.DataFine != oldRecord.DTA_FINE))
                {
                    oldRecord.VALID_DTA_END = DateTime.Now;

                    dataInizio = model.DataInizio;
                    idOrig = oldRecord.ID_STATO_RAPPORTO;
                    tipoAutomazione = "N";

                    oldRecord.COD_USER = cod_user;
                    oldRecord.COD_TERMID = cod_termid;
                    oldRecord.TMS_TIMESTAMP = tms_timestamp;


                }
                else
                {
                    addNew = false;
                }
            }

            if (model.Codice == "SW_N")
                codice = "SW_P";

            if (addNew)
            {
                newRecord = new myRaiDataTalentia.XR_STATO_RAPPORTO
                {
                    ID_STATO_RAPPORTO = db.XR_STATO_RAPPORTO.GeneraPrimaryKey(),
                    ID_PERSONA = model.IdPersona,
                    MATRICOLA = model.Matricola,
                    COD_STATO_RAPPORTO = codice,
                    DTA_INIZIO = dataInizio.Value,
                    DTA_FINE = model.DataFine,
                    COD_TIPO_ACCORDO = model.TipologiaAccordo,
                    VALID_DTA_INI = DateTime.Now,
                    IND_AUTOM = tipoAutomazione,
                    ID_STATO_RAPPORTO_ORIG = idOrig,
                    DTA_SCADENZA = model.DataScadenza,
                    DTA_NOTIF_DIP = oldRecord != null ? oldRecord.DTA_NOTIF_DIP : null,
                    DTA_INIZIO_VISUALIZZAZIONE = model.DataPresentazioneProposta,
                    FLG_FORZA_INIZIO_ACCORDO = model.BloccaDataInizio,
                    ID_RICH_RECESSO = oldRecord != null ? oldRecord.ID_RICH_RECESSO : null,
                    ID_MOD_DIPENDENTI = oldRecord != null ? oldRecord.ID_MOD_DIPENDENTI : null,
                };
                newRecord.COD_USER = cod_user;
                newRecord.COD_TERMID = cod_termid;
                newRecord.TMS_TIMESTAMP = tms_timestamp;
                newRecord.SWDEROGA_SCELTA = model.SWDeroga_Scelta;
                newRecord.SWDEROGA_OPZIONE = model.SWDeroga_Opzione;
                newRecord.LAVORATOREFRAGILE = model.LavoratoreFragile;
                newRecord.LAVORATOREFRAGILE_SCELTA = (model.LavoratoreFragile) ? model.LavoratoreFragile_Scelta : null;
                db.XR_STATO_RAPPORTO.Add(newRecord);

                if (oldRecord != null)
                {
                    //Se ho variato le date del record precedente prima porto le vecchie info
                    foreach (var item in oldRecord.XR_STATO_RAPPORTO_INFO.Where(x => x.VALID_DTA_END == null))
                    {
                        newRecord.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                        {
                            COD_TERMID = item.COD_TERMID,
                            COD_USER = item.COD_USER,
                            DTA_FINE = item.DTA_FINE,
                            DTA_INIZIO = item.DTA_INIZIO,
                            NUM_GIORNI_EXTRA = item.NUM_GIORNI_EXTRA,
                            NUM_GIORNI_MAX = item.NUM_GIORNI_MAX,
                            NUM_GIORNI_MIN = item.NUM_GIORNI_MIN,
                            TMS_TIMESTAMP = item.TMS_TIMESTAMP,
                            VALID_DTA_END = item.VALID_DTA_END,
                            VALID_DTA_INI = item.VALID_DTA_INI,
                            DTA_INVIO = item.DTA_INVIO,
                            DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA,
                            ID_RICH = item.ID_RICH,
                            IPOTESI_FRAGILI = item.IPOTESI_FRAGILI
                        });
                    }
                }
            }
            else
            {
                newRecord = oldRecord;
                newRecord.COD_STATO_RAPPORTO = codice;
                newRecord.DTA_INIZIO = model.DataInizio;
                newRecord.DTA_FINE = model.DataFine;
                newRecord.DTA_SCADENZA = model.DataScadenza;
                newRecord.DTA_INIZIO_VISUALIZZAZIONE = model.DataPresentazioneProposta;
                newRecord.FLG_FORZA_INIZIO_ACCORDO = model.BloccaDataInizio;
                newRecord.LAVORATOREFRAGILE = model.LavoratoreFragile;
                newRecord.LAVORATOREFRAGILE_SCELTA = (model.LavoratoreFragile) ? model.LavoratoreFragile_Scelta : null;
                //newRecord.SWDEROGA_SCELTA = model.SWDeroga_Scelta;
                //newRecord.SWDEROGA_OPZIONE = model.SWDeroga_Opzione;
            }

            //La definizione del numero di giorni deve essere fatta solo in fase di sottoscrizione di nuovo periodo, che sia Unilaterale o Individuale
            if (model.IdEvento == 0)
            {
                if (model.NumeroGiorniMax.HasValue)
                {
                    newRecord.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                    {
                        NUM_GIORNI_MAX = model.NumeroGiorniMax,
                        NUM_GIORNI_EXTRA = model.NumeroGiorniExtra,
                        DTA_INIZIO = newRecord.DTA_INIZIO,
                        DTA_FINE = newRecord.DTA_FINE,
                        VALID_DTA_INI = newRecord.VALID_DTA_INI.Value,
                        COD_USER = cod_user,
                        COD_TERMID = cod_termid,
                        TMS_TIMESTAMP = tms_timestamp
                    });
                }
            }
            else
            {
                //Se sono cambiate la date di inizio e fine devono essere riportate sui periodi
                if (newRecord.XR_STATO_RAPPORTO_INFO != null && newRecord.XR_STATO_RAPPORTO_INFO.Any())
                {
                    var first = newRecord.XR_STATO_RAPPORTO_INFO.Where(x => x.VALID_DTA_END == null).OrderBy(x => x.DTA_INIZIO).FirstOrDefault();
                    first.DTA_INIZIO = newRecord.DTA_INIZIO;
                    var last = newRecord.XR_STATO_RAPPORTO_INFO.Where(x => x.VALID_DTA_END == null).OrderBy(x => x.DTA_INIZIO).LastOrDefault();
                    last.DTA_FINE = newRecord.DTA_FINE;
                }
            }

            /*
            if (model.MeseRif.HasValue)
            {
                var info = newRecord.XR_STATO_RAPPORTO_INFO.Where(x => x.DTA_INIZIO < model.MeseRif && model.MeseRif <= x.DTA_FINE && x.VALID_DTA_END == null).OrderBy(x => x.DTA_INIZIO);
                if (info.Any())
                {
                    var first = info.FirstOrDefault();
                    first.DTA_FINE = model.MeseRif.Value.AddDays(-1);
                    first.COD_USER = cod_user;
                    first.COD_TERMID = cod_termid;
                    first.TMS_TIMESTAMP = tms_timestamp;
                }

                info = newRecord.XR_STATO_RAPPORTO_INFO.Where(x => x.DTA_INIZIO >= model.MeseRif && x.VALID_DTA_END == null).OrderBy(x => x.DTA_INIZIO);

                if (info.Any())
                {
                    foreach (var item in info)
                        item.VALID_DTA_END = DateTime.Now;
                }

                newRecord.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                {
                    NUM_GIORNI_MAX = model.NumeroGiorniMax,
                    NUM_GIORNI_EXTRA = model.NumeroGiorniExtra,
                    DTA_INIZIO = model.MeseRif.Value,
                    DTA_FINE = newRecord.DTA_FINE,
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = cod_user,
                    COD_TERMID = cod_termid,
                    TMS_TIMESTAMP = tms_timestamp
                });
            }
            else
            {
                foreach (var item in newRecord.XR_STATO_RAPPORTO_INFO)
                    item.VALID_DTA_END = DateTime.Now;
                if (model.NumeroGiorniMax.HasValue)
                {
                    newRecord.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                    {
                        NUM_GIORNI_MAX = model.NumeroGiorniMax,
                        NUM_GIORNI_EXTRA = model.NumeroGiorniExtra,
                        DTA_INIZIO = newRecord.DTA_INIZIO,
                        DTA_FINE = newRecord.DTA_FINE,
                        VALID_DTA_INI = newRecord.VALID_DTA_INI.Value,
                        COD_USER = cod_user,
                        COD_TERMID = cod_termid,
                        TMS_TIMESTAMP = tms_timestamp
                    });
                }
            }
            */

            if (newRecord != null)
            {
                if (newRecord.COD_STATO_RAPPORTO == "SW" && (oldRecord != null && (oldRecord.DTA_INIZIO != newRecord.DTA_INIZIO || oldRecord.DTA_FINE != newRecord.DTA_FINE)))// newRecord.COD_TIPO_ACCORDO=="Unilaterale"))
                {
                    var datiUtente = db.SINTESI1.Where(w => w.ID_PERSONA.Equals(newRecord.ID_PERSONA)).FirstOrDefault();

                    if (datiUtente != null && datiUtente.COD_IMPRESACR == "0")
                    {
                        // Link per HRDW
                        var dbHRDW = GetDbHR_Liv2();

                        string query = " select t10.[desc_serv_cont] " +
                                         " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0   " +
                                         " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON(t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica])   " +
                                         " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SERVIZIO_CONTABILE] t10 on (t1.sky_servizio_contabile=t10.sky_serv_cont) " +
                                         " where t1.[flg_ultimo_record]='$' and data_cessazione>{d'2020-01-01'} and matricola_dp = '" + datiUtente.COD_MATLIBROMAT + "'";

                        string sede = dbHRDW.Database.SqlQuery<string>(query).FirstOrDefault();

                        using (digiGappEntities digigappDB = new digiGappEntities())
                        {
                            MyRai_Importazioni toAdd = new MyRai_Importazioni();
                            toAdd.Matricola = "ImportaProrogheSWDaCSV";
                            toAdd.Tabella = "ProrogaModuloSmartWorking2020";
                            toAdd.Parametro1 = "P" + datiUtente.COD_MATLIBROMAT;
                            toAdd.Parametro2 = datiUtente.DES_COGNOMEPERS.Trim() + " " + datiUtente.DES_NOMEPERS.Trim();
                            toAdd.Parametro3 = datiUtente.DES_SERVIZIO;
                            toAdd.Parametro4 = newRecord.DTA_INIZIO.ToString("dd/MM/yyyy");
                            toAdd.Parametro5 = newRecord.DTA_FINE.ToString("dd/MM/yyyy");
                            toAdd.Parametro6 = null;
                            toAdd.Parametro8 = null;

                            if (model.IdEvento != 0)
                            {
                                toAdd.Parametro7 = "P";
                            }
                            else
                            {
                                bool exists = digigappDB.MyRai_Importazioni.Count(w => w.Parametro1.Equals(toAdd.Parametro1)) > 0;

                                if (exists)
                                {
                                    toAdd.Parametro7 = "R";
                                }
                                else
                                {
                                    toAdd.Parametro7 = "N";
                                }
                            }

                            toAdd.Parametro9 = newRecord.DTA_INIZIO.ToString("dd/MM/yyyy");
                            toAdd.Parametro10 = newRecord.DTA_FINE.ToString("dd/MM/yyyy");
                            toAdd.Parametro11 = null;
                            toAdd.Parametro12 = null;
                            toAdd.Parametro13 = null;
                            toAdd.Parametro14 = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            toAdd.Parametro15 = "HRIS_AttivaSW_" + newRecord.ID_STATO_RAPPORTO.ToString();

                            digigappDB.MyRai_Importazioni.Add(toAdd);
                            digigappDB.SaveChanges();
                        }
                    }
                }
                else if (newRecord.COD_STATO_RAPPORTO == "SW_P" && model.IdEvento == 0)
                {
                    var sintesi = db.SINTESI1.Find(newRecord.ID_PERSONA);
                    var template = HrisHelper.GetTemplate(sintesi, null, "MailSW", "SottoscrizioneAccordo");
                    if (template != null && model.TipologiaAccordo.Trim().ToUpper() != "DEROGA") // la deroga non manda mail
                    {
                        GestoreMail mail = new myRaiCommonTasks.GestoreMail();
                        string mailOggetto = HrisHelper.ReplaceToken(sintesi, newRecord, template.MAIL_OGGETTO);
                        string mailMittente = template.MAIL_MITTENTE;
                        string mailDestinario = String.Format("p{0}@rai.it", sintesi.COD_MATLIBROMAT);//CommonTasks.GetEmailPerMatricola(sintesi.COD_MATLIBROMAT);
                        string mailTesto = HrisHelper.ReplaceToken(sintesi, newRecord, template.TEMPLATE_TEXT);
                        DateTime? programmata = model.DataPresentazioneProposta;
                        var response = mail.InvioMail(mailTesto, mailOggetto, mailDestinario, mailMittente, mailMittente, null, programmata, null);
                        if (response != null && !response.Esito)
                            ;
                        else
                            newRecord.DTA_NOTIF_DIP = DateTime.Now;
                    }
                }
            }
            //      if (addNew)
            return newRecord;
            //     else
            //         return null;
        }

        public static int[] RichiesteStatiSW()
        {
            return new int[] { (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.Inviata,
                                (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.InCaricoGestione,
                                (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.ApprovataGestione,
                                (int)myRaiCommonManager.MaternitaCongediManager.EnumStatiRichiesta.AnnullataGestione};
        }

        public class TempResultItem
        {
            public string COD_MODULO { get; set; }
            public DateTime? DATA_COMPILAZIONE { get; set; }
            public string SCELTA { get; set; }
        }


        public static void CaricaDatiStatoRapporto(AnagraficaModel anag, myRaiDataTalentia.TalentiaEntities db, SintesiModel sint, IEnumerable<myRaiDataTalentia.XR_STATO_RAPPORTO> inputRapporti = null, IEnumerable<myRaiDataTalentia.XR_MOD_DIPENDENTI> listMod = null, IEnumerable<myRaiData.Incentivi.XR_MAT_RICHIESTE> listRich = null, bool orderByErrore = false)
        {
            var dbCzn = new myRaiData.Incentivi.IncentiviEntities();
            //var tmpParam = dbCzn.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "SWAggiungiRichieste");
            bool aggiungiRich = false;// tmpParam == null || tmpParam.COD_VALUE1 == "TRUE";

            anag.DatiStatiRapporti.CanAdd = true;
            anag.DatiStatiRapporti.CanModify = true;

            IEnumerable<myRaiDataTalentia.XR_STATO_RAPPORTO> statiRapporti = null;
            if (inputRapporti != null)
                statiRapporti = inputRapporti.Where(x => x.MATRICOLA == anag.Matricola).OrderByDescending(x => x.DTA_INIZIO);
            else
                statiRapporti = db.XR_STATO_RAPPORTO.Where(x => x.MATRICOLA == anag.Matricola).OrderByDescending(x => x.DTA_INIZIO);

            foreach (var item in statiRapporti)
            {
                EventoModel model = new EventoModel();

                model.IdEvento = item.ID_STATO_RAPPORTO;
                model.Codice = item.COD_STATO_RAPPORTO;
                model.Descrizione = item.XR_TB_STATO_RAPPORTO.DES_STATO_RAPPORTO;
                model.CodiceEvento = "";
                model.DataInizio = item.DTA_INIZIO;
                model.DataFine = item.DTA_FINE;
                model.TipologiaAccordo = item.COD_TIPO_ACCORDO;
                model.NotificaDipendente = item.DTA_NOTIF_DIP;
                model.NotificaEnte = item.DTA_NOTIF_ENTE;
                model.ValiditaInizio = item.VALID_DTA_INI;
                model.ValiditaFine = item.VALID_DTA_END;
                model.IdEventoPrec = item.ID_STATO_RAPPORTO_ORIG;
                model.DataScadenza = item.DTA_SCADENZA;
                model.DataPresentazioneProposta = item.DTA_INIZIO_VISUALIZZAZIONE;
                model.BloccaDataInizio = item.FLG_FORZA_INIZIO_ACCORDO.GetValueOrDefault();
                model.Tipo = TipoEvento.Stato;
                model.Modulo = item.ID_MOD_DIPENDENTI;
                model.RichiestaRecesso = item.ID_RICH_RECESSO;
                model.SWDeroga_Opzione = item.SWDEROGA_OPZIONE;
                model.SWDeroga_Scelta = item.SWDEROGA_SCELTA;
                model.DataRichiesta = item.TMS_TIMESTAMP;
                model.LavoratoreFragile = item.LAVORATOREFRAGILE.GetValueOrDefault();
                model.LavoratoreFragile_Scelta = item.LAVORATOREFRAGILE_SCELTA;
                var xx = dbCzn.XR_SW_API.Where(x => x.ID_STATORAPPORTO == item.ID_STATO_RAPPORTO).FirstOrDefault();
                if (xx != null)
                {
                    model.ErroreInvioTelematicoSw = xx.ERRORE;
                    model.PeriodoDal = xx.PERIODO_DAL;
                    model.PeriodoAl = xx.PERIODO_AL;
                }
                //if (item.ID_MOD_DIPENDENTI.HasValue)
                //{
                //    TempResultItem tempModelloFirmato = null;

                //    tempModelloFirmato = db.XR_MOD_DIPENDENTI
                //        .Where(w => w.XR_MOD_DIPENDENTI1 == item.ID_MOD_DIPENDENTI.Value)
                //        .Select(w => new TempResultItem()
                //        {
                //            COD_MODULO = w.COD_MODULO,
                //            DATA_COMPILAZIONE = w.DATA_COMPILAZIONE,
                //            SCELTA = w.SCELTA
                //        }).FirstOrDefault();

                //    if (tempModelloFirmato != null)
                //    {
                //        model.DataSottoscrizione = tempModelloFirmato.DATA_COMPILAZIONE;

                //        // modello per lavoratori fragili
                //        if (tempModelloFirmato.COD_MODULO == "SMARTW2020")
                //        {
                //            item.LAVORATOREFRAGILE = true;
                //            // se è una scelta per di un lavoratore fragile, deve
                //            // calcolare la scelta effettuata dallo stesso
                //            List<ModuloSmart2020Selezioni> selezioni = new List<ModuloSmart2020Selezioni>();
                //            selezioni = (List<ModuloSmart2020Selezioni>)Newtonsoft.Json.JsonConvert.DeserializeObject(tempModelloFirmato.SCELTA);
                //            var selezionato = selezioni.FirstOrDefault();
                //            int i_selezionato = (int)selezionato.Selezione;
                //            string valoreSelezionato = String.Empty;

                //            switch (i_selezionato)
                //            {
                //                case ((int)ModuloSmart2020SelectionEnum.Scelta50):
                //                    valoreSelezionato = "PATO";
                //                    break;

                //                case ((int)ModuloSmart2020SelectionEnum.Scelta60):
                //                    valoreSelezionato = "DISA";
                //                    break;

                //                case ((int)ModuloSmart2020SelectionEnum.Scelta1000):
                //                    valoreSelezionato = "IMMU";
                //                    break;
                //            }

                //            if (!String.IsNullOrEmpty(valoreSelezionato))
                //            {
                //                //var sel = model.ListaCasiFragilita.Where(w => w.Value == valoreSelezionato).FirstOrDefault();
                //                //sel.Selected = true;
                //                item.LAVORATOREFRAGILE_SCELTA = valoreSelezionato;
                //            }
                //        }
                //    }
                //}
                //else
                //{

                //    TempResultItem tempModelloFirmato = null;

                //    tempModelloFirmato = db.XR_MOD_DIPENDENTI
                //        .Where(w => w.COD_MODULO == "SMARTW2020"
                //                && w.MATRICOLA == model.Matricola
                //                && w.DATA_COMPILAZIONE != null)
                //        .Select(w => new TempResultItem()
                //        {
                //            SCELTA = w.SCELTA
                //        }).FirstOrDefault();

                //    if (tempModelloFirmato != null)
                //    {
                //        item.LAVORATOREFRAGILE = true;
                //        // se è una scelta per di un lavoratore fragile, deve
                //        // calcolare la scelta effettuata dallo stesso
                //        List<ModuloSmart2020Selezioni> selezioni = new List<ModuloSmart2020Selezioni>();
                //        selezioni = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ModuloSmart2020Selezioni>>(tempModelloFirmato.SCELTA);
                //        var selezionato = selezioni;
                //        int i_selezionato = (int)selezionato.FirstOrDefault().Selezione;
                //        string valoreSelezionato = String.Empty;

                //        switch (i_selezionato)
                //        {
                //            case ((int)ModuloSmart2020SelectionEnum.Scelta50):
                //                valoreSelezionato = "PATO";
                //                break;

                //            case ((int)ModuloSmart2020SelectionEnum.Scelta60):
                //                valoreSelezionato = "DISA";
                //                break;

                //            case ((int)ModuloSmart2020SelectionEnum.Scelta1000):
                //                valoreSelezionato = "IMMU";
                //                break;
                //        }

                //        if (!String.IsNullOrEmpty(valoreSelezionato))
                //        {
                //            item.LAVORATOREFRAGILE_SCELTA = valoreSelezionato;
                //        }
                //    }
                //}

                if (item.XR_STATO_RAPPORTO_INFO != null && item.XR_STATO_RAPPORTO_INFO.Any())
                {
                    var last = item.XR_STATO_RAPPORTO_INFO.FirstOrDefault(x => x.DTA_INIZIO <= DateTime.Today && (x.DTA_FINE == null || x.DTA_FINE.Value >= DateTime.Today) && x.VALID_DTA_END == null);
                    if (last == null)
                        last = item.XR_STATO_RAPPORTO_INFO.FirstOrDefault(x => (x.DTA_FINE == null || x.DTA_FINE.Value >= DateTime.Today) && x.VALID_DTA_END == null);

                    if (last != null)
                    {
                        model.NumeroGiorniMax = last.NUM_GIORNI_MAX;
                        model.NumeroGiorniExtra = last.NUM_GIORNI_EXTRA;
                        if (last.ID_RICH.HasValue)
                        {
                            var richiestaTemp = dbCzn.XR_MAT_RICHIESTE.Where(w => w.ID == last.ID_RICH.Value).FirstOrDefault();
                            if (richiestaTemp != null)
                            {
                                model.DataRichiestaGiorniAggiuntivi = richiestaTemp.DATA_INVIO_RICHIESTA;
                            }
                        }
                    }

                    model.Info.AddRange(item.XR_STATO_RAPPORTO_INFO.Where(x => x.VALID_DTA_END == null).Select(x => new EventoModelInfo() { DataInizio = x.DTA_INIZIO, DataFine = x.DTA_FINE, NumeroGiorniMax = x.NUM_GIORNI_MAX, NumeroGiorniExtra = x.NUM_GIORNI_EXTRA, Ipotesi = x.IPOTESI_FRAGILI, DataInivio = x.DTA_INVIO }));
                }

                anag.DatiStatiRapporti.Richieste = new List<XR_MAT_RICHIESTE>();
                if (aggiungiRich)
                {
                    foreach (var info in model.Info)
                    {
                        XR_MAT_RICHIESTE intrich = null;
                        if (listRich == null)
                        {
                            intrich = dbCzn.XR_MAT_RICHIESTE.Include("XR_MAT_CATEGORIE").FirstOrDefault(x => x.MATRICOLA == anag.Matricola
                                                                                                && x.ECCEZIONE == "SW"
                                                                                                && (x.CATEGORIA == 52 || x.CATEGORIA == 54)
                                                                                                && x.XR_WKF_OPERSTATI.Any(y => y.COD_TIPO_PRATICA == "SW" && y.ID_STATO == 20));
                        }
                        else
                            intrich = listRich.FirstOrDefault(x => x.MATRICOLA == anag.Matricola
                                                            && x.ECCEZIONE == "SW"
                                                            && (x.CATEGORIA == 52 || x.CATEGORIA == 54)
                                                            && x.XR_WKF_OPERSTATI.Any(y => y.COD_TIPO_PRATICA == "SW" && y.ID_STATO == 20));

                        if (intrich != null)
                            info.NumeroGiorniRich = intrich.GIORNI_APPROVATI;
                    }
                }

                model.DescrizioneEvento = "";

                if (sint != null)
                    CaricaIdentityData(model, sint.ID_PERSONA, sint.COD_MATLIBROMAT);
                else
                    CaricaIdentityData(model, anag.IdPersona, anag.Matricola);

                anag.DatiStatiRapporti.Eventi.Add(model);
            }

            if(orderByErrore && anag.DatiStatiRapporti.Eventi != null && anag.DatiStatiRapporti.Eventi.Count > 0)
                anag.DatiStatiRapporti.Eventi.OrderByDescending(x=>x.ErroreInvioTelematicoSw);



            myRaiDataTalentia.XR_MOD_DIPENDENTI recordScelta = null;
            if (listMod == null)
                recordScelta = db.XR_MOD_DIPENDENTI.FirstOrDefault(x => x.MATRICOLA == anag.Matricola && x.COD_MODULO == "SMARTW2020");
            else
                recordScelta = listMod.FirstOrDefault(x => x.MATRICOLA == anag.Matricola && x.COD_MODULO == "SMARTW2020");
            if (recordScelta != null)
            {
                anag.DatiStatiRapporti.IsAventeDiritto = true;
                anag.DatiStatiRapporti.AventeDirittoSelezione = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ModuloSmart2020Selezioni>>(recordScelta.SCELTA);
                anag.DatiStatiRapporti.DataSelezione = recordScelta.DATA_COMPILAZIONE;
            }

            anag.DatiStatiRapporti.Richieste = new List<XR_MAT_RICHIESTE>();
            List<XR_MAT_RICHIESTE> rich = null;
            if (listRich == null)
            {
                var statiSw = RichiesteStatiSW();
                rich = dbCzn.XR_MAT_RICHIESTE.Include("XR_MAT_CATEGORIE").Where(x => x.MATRICOLA == anag.Matricola && x.ECCEZIONE == "SW")
                        .Where(x => statiSw.Contains(x.XR_WKF_OPERSTATI.Where(y => y.ID_GESTIONE == x.ID && y.COD_TIPO_PRATICA == "SW").Max(y => y.ID_STATO))).ToList();
            }
            else
                rich = listRich.Where(x => x.MATRICOLA == anag.Matricola && x.ECCEZIONE == "SW").ToList();

            foreach (var item in rich)
                anag.DatiStatiRapporti.Richieste.Add(item);

            CaricaIdentityData(anag.DatiStatiRapporti, anag.IdPersona, anag.Matricola);
        }

        public static bool GetAiCode(string matricola, out string codice, out DateTime? lastUpdate)
        {
            bool result = false;
            codice = null;
            lastUpdate = null;

            TQUALIFICA db2Qual = DB2Manager.SqlQuery<TQUALIFICA>(String.Format("SELECT * FROM OPENQUERY(DB2LINK, 'SELECT * FROM " + DB2Manager.GetPrefixTable() + ".TQUALIFICA WHERE matricola=''0{0}'' and DATINI<''{1:yyyy-MM-dd}'' and DATFIN>''{1:yyyy-MM-dd}''')", matricola, DateTime.Today)).FirstOrDefault();
            if (db2Qual != null)
            {
                result = true;
                codice = db2Qual.ASS_INF;
                lastUpdate = db2Qual.DATAGG;
            }
            var dbDigi = new digiGappEntities();
            DateTime rifDate = lastUpdate.HasValue ? lastUpdate.Value : DateTime.MinValue;
            var cacheCode = dbDigi.MyRai_CacheFunzioni.FirstOrDefault(x => x.oggetto == matricola && x.funzione == "AssicurazioneInfortuni" && x.data_creazione > rifDate);
            if (cacheCode != null)
            {
                result = true;
                codice = cacheCode.dati_serial;
                lastUpdate = cacheCode.data_creazione;
            }
            return result;
        }

        public static bool UpdateSWRequest(out string output, out string errore)
        {
            bool result = false;
            output = "";
            errore = "";

            var db = new TalentiaDB();
            //db.Database.ExecuteSqlCommand(String.Format("SELECT * INTO [XR_STATO_RAPPORTO_INFO_BCK_{0:yyyyMMddHHmm}] FROM XR_STATO_RAPPORTO_INFO", DateTime.Now));

            var dbCzn = new CezanneDb();
            //Prende l'elenco di quelle accettate

            var dbtmptal = new TalentiaDB();
            var elencoGiaProc = dbtmptal.XR_STATO_RAPPORTO_INFO.Where(x => x.ID_RICH != null).Select(x => x.ID_RICH).Distinct().ToList();

            var elencoRich = dbCzn.XR_MAT_RICHIESTE.Include("XR_MAT_CATEGORIE").Where(x => x.ECCEZIONE == "SW" && !x.MATRICOLA.Contains("*")
                                                                                                    && x.XR_WKF_OPERSTATI.Any(y => y.COD_TIPO_PRATICA == "SW" && y.ID_STATO == 20));

            foreach (var richiesta in elencoRich)
            {
                if (elencoGiaProc.Contains(richiesta.ID))
                {
                    output += "Richiesta " + richiesta.ID + " già processata\r\n";
                    continue;
                }

                try
                {
                    DateTime? inizio = null;
                    DateTime? fine = null;
                    int? numGiorni = null;
                    int? numGiorniExtra = null;
                    XR_WKF_OPERSTATI appr = richiesta.XR_WKF_OPERSTATI.FirstOrDefault(x => x.ID_GESTIONE == richiesta.ID && x.ID_STATO == 20 && x.COD_TIPO_PRATICA == "SW");

                    string tmpOutput = null;
                    string tmpErrore = null;
                    //switch (richiesta.XR_MAT_CATEGORIE.SOTTO_CAT)
                    //{
                    //    case "MI14":
                    //    case "ASSD":
                    //        inizio = richiesta.DATA_INIZIO_SW;
                    //        fine = richiesta.DATA_FINE_SW;
                    //        numGiorni = null;
                    //        numGiorniExtra = richiesta.GIORNI_APPROVATI;
                    //        AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, 12, out tmpOutput, out tmpErrore);
                    //        output += tmpOutput;
                    //        errore += tmpErrore;
                    //        break;
                    //    case "PATO":
                    //    case "IMMU":
                    //        inizio = appr.VALID_DTA_INI.Date.AddDays(-(appr.VALID_DTA_INI.Day - 1));
                    //        fine = new DateTime(2022, 06, 30);
                    //        numGiorni = null;
                    //        numGiorniExtra = 12;
                    //        AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, 12, out tmpOutput, out tmpErrore);
                    //        output += tmpOutput;
                    //        errore += tmpErrore;
                    //        break;
                    //    case "DISA":
                    //        if (String.IsNullOrWhiteSpace(richiesta.XR_MAT_CATEGORIE.SOLO_TIPO_DIP) || richiesta.XR_MAT_CATEGORIE.SOLO_TIPO_DIP != "G")
                    //        {
                    //            inizio = appr.VALID_DTA_INI.Date.AddDays(-(appr.VALID_DTA_INI.Day - 1));
                    //            UtenteHelper.IsSmartWorker(richiesta.MATRICOLA, appr.VALID_DTA_INI, appr.VALID_DTA_INI, out var info);
                    //            fine = info.FirstOrDefault().Fine;
                    //            numGiorni = null;
                    //            numGiorniExtra = 12;
                    //            AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, 12, out tmpOutput, out tmpErrore);
                    //        }
                    //        else
                    //        {
                    //            inizio = richiesta.DATA_INIZIO_SW;
                    //            fine = richiesta.DATA_FINE_SW;
                    //            numGiorni = null;
                    //            numGiorniExtra = richiesta.GIORNI_APPROVATI;
                    //            AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, 12, out tmpOutput, out tmpErrore);
                    //        }

                    //        //inizio = appr.VALID_DTA_INI.Date.AddDays(-(appr.VALID_DTA_INI.Day - 1));
                    //        //UtenteHelper.IsSmartWorker(richiesta.MATRICOLA, appr.VALID_DTA_INI, appr.VALID_DTA_INI, out var info);
                    //        //fine = info.FirstOrDefault().Fine;
                    //        //numGiorni = null;
                    //        //numGiorniExtra = 12;
                    //        //AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, 12, out tmpOutput, out tmpErrore);
                    //        output += tmpOutput;
                    //        errore += tmpErrore;
                    //        break;
                    //    default:
                    //        if (richiesta.INIZIO_GIUSTIFICATIVO.Value.Month == richiesta.FINE_GIUSTIFICATIVO.Value.Month)
                    //        {
                    //            inizio = richiesta.INIZIO_GIUSTIFICATIVO.Value.AddDays(-(richiesta.INIZIO_GIUSTIFICATIVO.Value.Day - 1));
                    //            fine = inizio.Value.AddMonths(1).AddDays(-1);
                    //            numGiorni = null;
                    //            //numGiorniExtra = Convert.ToInt32(richiesta.XR_MAT_CATEGORIE.DESCRIZIONE_ECCEZIONE.Split(',')[1]);
                    //            numGiorniExtra = ContaGiorni(richiesta.INIZIO_GIUSTIFICATIVO.Value, richiesta.FINE_GIUSTIFICATIVO.Value);
                    //            AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, null, out tmpOutput, out tmpErrore);
                    //            output += tmpOutput;
                    //            errore += tmpErrore;
                    //        }
                    //        else
                    //        {
                    //            inizio = richiesta.INIZIO_GIUSTIFICATIVO.Value.AddDays(-(richiesta.INIZIO_GIUSTIFICATIVO.Value.Day - 1));
                    //            fine = inizio.Value.AddMonths(1).AddDays(-1);
                    //            numGiorni = null;
                    //            //numGiorniExtra = (fine.Value - richiesta.INIZIO_GIUSTIFICATIVO.Value).Days + 1;
                    //            numGiorniExtra = ContaGiorni(richiesta.INIZIO_GIUSTIFICATIVO.Value, fine.Value);
                    //            AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, null, out tmpOutput, out tmpErrore);
                    //            output += tmpOutput;
                    //            errore += tmpErrore;

                    //            inizio = fine.Value.AddDays(1);
                    //            fine = richiesta.FINE_GIUSTIFICATIVO.Value;
                    //            numGiorni = null;
                    //            //numGiorniExtra = (richiesta.FINE_GIUSTIFICATIVO.Value - inizio.Value).Days + 1;
                    //            numGiorniExtra = ContaGiorni(inizio.Value, richiesta.FINE_GIUSTIFICATIVO.Value);
                    //            AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, 12, out tmpOutput, out tmpErrore);
                    //            output += tmpOutput;
                    //            errore += tmpErrore;
                    //        }
                    //        break;
                    //}

                    switch (richiesta.XR_MAT_CATEGORIE.TIPO_AGGIORNAMENTO_STATO)
                    {
                        case "AGGIUNTIVI":
                            inizio = richiesta.DATA_INIZIO_SW;
                            fine = richiesta.DATA_FINE_SW;
                            numGiorni = null;
                            numGiorniExtra = richiesta.GIORNI_APPROVATI;
                            AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, 12, out tmpOutput, out tmpErrore);
                            output += tmpOutput;
                            errore += tmpErrore;
                            break;
                        case "FISSO":
                            inizio = appr.VALID_DTA_INI.Date.AddDays(-(appr.VALID_DTA_INI.Day - 1));
                            inizio = appr.VALID_DTA_INI.Date.AddDays(-(appr.VALID_DTA_INI.Day - 1));
                            if (richiesta.XR_MAT_CATEGORIE.DATA_FINE_FORZATA.HasValue)
                                fine = richiesta.XR_MAT_CATEGORIE.DATA_FINE_FORZATA.Value;
                            else
                            {
                                UtenteHelper.IsSmartWorker(richiesta.MATRICOLA, appr.VALID_DTA_INI, appr.VALID_DTA_INI, out var info);
                                fine = info.FirstOrDefault().Fine;
                            }
                            numGiorni = null;
                            numGiorniExtra = 12;
                            AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, 12, out tmpOutput, out tmpErrore);
                            output += tmpOutput;
                            errore += tmpErrore;
                            break;
                        case "PERIODO":
                            if (richiesta.INIZIO_GIUSTIFICATIVO.Value.Month == richiesta.FINE_GIUSTIFICATIVO.Value.Month)
                            {
                                inizio = richiesta.INIZIO_GIUSTIFICATIVO.Value.AddDays(-(richiesta.INIZIO_GIUSTIFICATIVO.Value.Day - 1));
                                fine = inizio.Value.AddMonths(1).AddDays(-1);
                                numGiorni = null;
                                //numGiorniExtra = Convert.ToInt32(richiesta.XR_MAT_CATEGORIE.DESCRIZIONE_ECCEZIONE.Split(',')[1]);
                                numGiorniExtra = ContaGiorni(richiesta.INIZIO_GIUSTIFICATIVO.Value, richiesta.FINE_GIUSTIFICATIVO.Value);
                                AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, null, out tmpOutput, out tmpErrore);
                                output += tmpOutput;
                                errore += tmpErrore;
                            }
                            else
                            {
                                inizio = richiesta.INIZIO_GIUSTIFICATIVO.Value.AddDays(-(richiesta.INIZIO_GIUSTIFICATIVO.Value.Day - 1));
                                fine = inizio.Value.AddMonths(1).AddDays(-1);
                                numGiorni = null;
                                //numGiorniExtra = (fine.Value - richiesta.INIZIO_GIUSTIFICATIVO.Value).Days + 1;
                                numGiorniExtra = ContaGiorni(richiesta.INIZIO_GIUSTIFICATIVO.Value, fine.Value);
                                AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, null, out tmpOutput, out tmpErrore);
                                output += tmpOutput;
                                errore += tmpErrore;

                                inizio = fine.Value.AddDays(1);
                                fine = richiesta.FINE_GIUSTIFICATIVO.Value;
                                numGiorni = null;
                                //numGiorniExtra = (richiesta.FINE_GIUSTIFICATIVO.Value - inizio.Value).Days + 1;
                                numGiorniExtra = ContaGiorni(inizio.Value, richiesta.FINE_GIUSTIFICATIVO.Value);
                                AggiornaRapportoSW(richiesta.MATRICOLA, richiesta.ID, inizio, fine, numGiorni, numGiorniExtra, 12, out tmpOutput, out tmpErrore);
                                output += tmpOutput;
                                errore += tmpErrore;
                            }
                            break;
                        default:
                            errore += "Tipologia non implementata";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    errore += String.Format("Matricola: {0} - Rich: {1} - Eccezione {2} - {3}\r\n", richiesta.MATRICOLA, richiesta.ID, ex.Message, ex.StackTrace);
                }
            }

            var tmpParam = dbCzn.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "SWAggiungiRichieste");
            tmpParam.COD_VALUE1 = "FALSE";

            DBHelper.Save(dbCzn, "Task");

            result = true;
            return result;
        }

        private static int ContaGiorni(DateTime from, DateTime to)
        {
            int totalDays = 0;

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    totalDays++;
            }

            return totalDays;
        }

        private static void CalcolaGiorni(int? recMax, int? recExtra, int? modMax, int? modExtra, int? checkTotal, out int? resMax, out int? resExtra)
        {
            resMax = recMax;
            resExtra = recExtra;

            if (modMax.HasValue)
                resMax = modMax;

            if (modExtra.HasValue)
                resExtra = recExtra.GetValueOrDefault() + modExtra.GetValueOrDefault();

            if (checkTotal.HasValue)
            {
                if (resMax.GetValueOrDefault() + resExtra.GetValueOrDefault() > checkTotal.Value)
                {
                    int diff = (resMax.GetValueOrDefault() + resExtra.GetValueOrDefault()) - checkTotal.Value;
                    if (diff > resExtra.GetValueOrDefault())
                        resExtra = 0;
                    else
                        resExtra = resExtra.GetValueOrDefault() - diff;
                }
            }
        }

        private static void AggiornaRapportoSW(string matricola, int idRichiesta, DateTime? inizio, DateTime? fine, int? numGiorni, int? numGiorniExtra, int? maxTotal, out string output, out string errore)
        {
            output = "";
            errore = "";

            var dbTal = new TalentiaDB();
            var rapporto = dbTal.XR_STATO_RAPPORTO.Include("XR_STATO_RAPPORTO_INFO").FirstOrDefault(x => x.MATRICOLA == matricola && x.COD_STATO_RAPPORTO == "SW"
                                                                                        && x.DTA_INIZIO <= fine && inizio <= x.DTA_FINE);

            int? newMax = null;
            int? newExtra = null;

            if (rapporto == null)
            {
                //rapporto non trovato
                errore += String.Format("Matricola: {0} - Rich: {1} - Non trovata\r\n", matricola, idRichiesta);
            }
            else
            {
                CezanneHelper.GetCampiFirma(out string cod_user, out string cod_termid, out DateTime tms_timestamp);
                if (rapporto.XR_STATO_RAPPORTO_INFO == null || !rapporto.XR_STATO_RAPPORTO_INFO.Any())
                {
                    //Non è possibile in quanto sono tutti con i giorni definiti
                    errore += String.Format("Matricola: {0} - Rich: {1} - Info giorni non trovate\r\n", matricola, idRichiesta);
                }
                else
                {
                    DateTime dtaInizio = inizio.Value;
                    if (inizio.Value.Month == rapporto.DTA_INIZIO.Month && inizio.Value.Year == rapporto.DTA_INIZIO.Year && inizio.Value.Day < rapporto.DTA_INIZIO.Day)
                        dtaInizio = rapporto.DTA_INIZIO;

                    bool update = true;
                    var rifPeriod = rapporto.XR_STATO_RAPPORTO_INFO.FirstOrDefault(x => x.VALID_DTA_END == null && x.DTA_INIZIO == dtaInizio && x.DTA_FINE == fine);
                    if (rifPeriod != null)
                    {
                        //Se trovo lo stesso periodo è il caso semplice, annullo il vecchio (così rimane traccia)
                        //E creo il nuovo record con le nuove date
                        rifPeriod.VALID_DTA_END = tms_timestamp;
                        CalcolaGiorni(rifPeriod.NUM_GIORNI_MAX, rifPeriod.NUM_GIORNI_EXTRA, numGiorni, numGiorniExtra, maxTotal, out newMax, out newExtra);
                        rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                        {
                            DTA_INIZIO = dtaInizio,
                            DTA_FINE = fine.Value,
                            //NUM_GIORNI_MAX = numGiorni.HasValue ? numGiorni : rifPeriod.NUM_GIORNI_MAX,
                            //NUM_GIORNI_EXTRA = numGiorniExtra.HasValue ? rifPeriod.NUM_GIORNI_EXTRA.GetValueOrDefault()+numGiorniExtra : rifPeriod.NUM_GIORNI_EXTRA,
                            NUM_GIORNI_MAX = newMax,
                            NUM_GIORNI_EXTRA = newExtra,
                            VALID_DTA_INI = tms_timestamp,
                            VALID_DTA_END = null,
                            COD_USER = cod_user,
                            COD_TERMID = cod_termid,
                            TMS_TIMESTAMP = tms_timestamp,
                            DTA_INVIO = rifPeriod.DTA_INVIO,
                            IPOTESI_FRAGILI = rifPeriod.IPOTESI_FRAGILI,
                            DTA_VISITA_MEDICA = rifPeriod.DTA_VISITA_MEDICA,
                            ID_RICH = idRichiesta,
                            NUM_GIORNI_MAX_ORIG = rifPeriod.NUM_GIORNI_MAX,
                            NUM_GIORNI_EXTRA_ORIG = rifPeriod.NUM_GIORNI_EXTRA
                        });
                    }
                    else
                    {
                        output += String.Format("Matricola: {0} - Rich: {1} - Da aggiornare {2:dd/MM/yyyy} - {3:dd/MM/yyyy}", matricola, idRichiesta, inizio, fine);
                        foreach (var item in rapporto.XR_STATO_RAPPORTO_INFO.Where(x => x.VALID_DTA_END == null).OrderBy(x => x.DTA_INIZIO))
                        {
                            output += String.Format("\t{0:dd/MM/yyyy}-{1:dd/MM/yyyy}", item.DTA_INIZIO, item.DTA_FINE);
                        }
                        output += "\r\n";

                        var periodiACavallo = rapporto.XR_STATO_RAPPORTO_INFO.Where(x => x.VALID_DTA_END == null && x.DTA_INIZIO <= fine && dtaInizio <= x.DTA_FINE).ToList();
                        foreach (var item in periodiACavallo)
                        {
                            item.VALID_DTA_END = tms_timestamp;
                            //Se il periodo è tutto contenuto, lo ricopio con i giorni approvati
                            if (item.DTA_INIZIO >= dtaInizio && item.DTA_FINE <= fine)
                            {
                                //Variazione
                                CalcolaGiorni(item.NUM_GIORNI_MAX, item.NUM_GIORNI_EXTRA, numGiorni, numGiorniExtra, maxTotal, out newMax, out newExtra);
                                rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                {
                                    DTA_INIZIO = item.DTA_INIZIO,
                                    DTA_FINE = item.DTA_FINE,
                                    //NUM_GIORNI_MAX = numGiorni.HasValue ? numGiorni : item.NUM_GIORNI_MAX,
                                    //NUM_GIORNI_EXTRA = numGiorniExtra.HasValue ? item.NUM_GIORNI_EXTRA.GetValueOrDefault() + numGiorniExtra :item.NUM_GIORNI_EXTRA,
                                    NUM_GIORNI_MAX = newMax,
                                    NUM_GIORNI_EXTRA = newExtra,
                                    VALID_DTA_INI = tms_timestamp,
                                    VALID_DTA_END = null,
                                    COD_USER = cod_user,
                                    COD_TERMID = cod_termid,
                                    TMS_TIMESTAMP = tms_timestamp,
                                    DTA_INVIO = item.DTA_INVIO,
                                    IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                    DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA,
                                    ID_RICH = idRichiesta,
                                    NUM_GIORNI_MAX_ORIG = item.NUM_GIORNI_MAX,
                                    NUM_GIORNI_EXTRA_ORIG = item.NUM_GIORNI_EXTRA
                                });
                            }
                            //Se il periodo contiene tutta la richiesta, lo spezzo
                            else if (item.DTA_INIZIO < dtaInizio && item.DTA_FINE > fine)
                            {
                                rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                {
                                    DTA_INIZIO = item.DTA_INIZIO,
                                    DTA_FINE = dtaInizio.AddDays(-1),
                                    NUM_GIORNI_MAX = item.NUM_GIORNI_MAX,
                                    NUM_GIORNI_EXTRA = item.NUM_GIORNI_EXTRA,
                                    VALID_DTA_INI = tms_timestamp,
                                    VALID_DTA_END = null,
                                    COD_USER = cod_user,
                                    COD_TERMID = cod_termid,
                                    TMS_TIMESTAMP = tms_timestamp,
                                    DTA_INVIO = item.DTA_INVIO,
                                    IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                    DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA
                                });
                                //Variazione
                                CalcolaGiorni(item.NUM_GIORNI_MAX, item.NUM_GIORNI_EXTRA, numGiorni, numGiorniExtra, maxTotal, out newMax, out newExtra);
                                rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                {
                                    DTA_INIZIO = dtaInizio,
                                    DTA_FINE = fine.Value,
                                    //NUM_GIORNI_MAX = numGiorni.HasValue ? numGiorni : item.NUM_GIORNI_MAX,
                                    //NUM_GIORNI_EXTRA = numGiorniExtra.HasValue ? item.NUM_GIORNI_EXTRA.GetValueOrDefault() + numGiorniExtra :item.NUM_GIORNI_EXTRA,
                                    NUM_GIORNI_MAX = newMax,
                                    NUM_GIORNI_EXTRA = newExtra,
                                    VALID_DTA_INI = tms_timestamp,
                                    VALID_DTA_END = null,
                                    COD_USER = cod_user,
                                    COD_TERMID = cod_termid,
                                    TMS_TIMESTAMP = tms_timestamp,
                                    DTA_INVIO = item.DTA_INVIO,
                                    IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                    DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA,
                                    ID_RICH = idRichiesta,
                                    NUM_GIORNI_MAX_ORIG = item.NUM_GIORNI_MAX,
                                    NUM_GIORNI_EXTRA_ORIG = item.NUM_GIORNI_EXTRA
                                });
                                rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                {
                                    DTA_INIZIO = fine.Value.AddDays(1),
                                    DTA_FINE = item.DTA_FINE,
                                    NUM_GIORNI_MAX = item.NUM_GIORNI_MAX,
                                    NUM_GIORNI_EXTRA = item.NUM_GIORNI_EXTRA,
                                    VALID_DTA_INI = tms_timestamp,
                                    VALID_DTA_END = null,
                                    COD_USER = cod_user,
                                    COD_TERMID = cod_termid,
                                    TMS_TIMESTAMP = tms_timestamp,
                                    DTA_INVIO = item.DTA_INVIO,
                                    IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                    DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA
                                });
                            }
                            //Se il periodo inizia prima della richiesta e finisce prima della richiesta
                            else if (item.DTA_INIZIO < dtaInizio && item.DTA_FINE < fine)
                            {
                                rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                {
                                    DTA_INIZIO = item.DTA_INIZIO,
                                    DTA_FINE = dtaInizio.AddDays(-1),
                                    NUM_GIORNI_MAX = item.NUM_GIORNI_MAX,
                                    NUM_GIORNI_EXTRA = item.NUM_GIORNI_EXTRA,
                                    VALID_DTA_INI = tms_timestamp,
                                    VALID_DTA_END = null,
                                    COD_USER = cod_user,
                                    COD_TERMID = cod_termid,
                                    TMS_TIMESTAMP = tms_timestamp,
                                    DTA_INVIO = item.DTA_INVIO,
                                    IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                    DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA
                                });
                                //Variazione
                                CalcolaGiorni(item.NUM_GIORNI_MAX, item.NUM_GIORNI_EXTRA, numGiorni, numGiorniExtra, maxTotal, out newMax, out newExtra);
                                rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                {
                                    DTA_INIZIO = dtaInizio,
                                    DTA_FINE = item.DTA_FINE,
                                    //NUM_GIORNI_MAX = numGiorni.HasValue ? numGiorni.Value : item.NUM_GIORNI_MAX,
                                    //NUM_GIORNI_EXTRA = numGiorniExtra.HasValue ? item.NUM_GIORNI_EXTRA.GetValueOrDefault() + numGiorniExtra : item.NUM_GIORNI_EXTRA,
                                    NUM_GIORNI_MAX = newMax,
                                    NUM_GIORNI_EXTRA = newExtra,
                                    VALID_DTA_INI = tms_timestamp,
                                    VALID_DTA_END = null,
                                    COD_USER = cod_user,
                                    COD_TERMID = cod_termid,
                                    TMS_TIMESTAMP = tms_timestamp,
                                    DTA_INVIO = item.DTA_INVIO,
                                    IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                    DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA,
                                    ID_RICH = idRichiesta,
                                    NUM_GIORNI_MAX_ORIG = item.NUM_GIORNI_MAX,
                                    NUM_GIORNI_EXTRA_ORIG = item.NUM_GIORNI_EXTRA
                                });
                                //Variazione
                                CalcolaGiorni(item.NUM_GIORNI_MAX, item.NUM_GIORNI_EXTRA, numGiorni, numGiorniExtra, maxTotal, out newMax, out newExtra);
                                rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                {
                                    DTA_INIZIO = item.DTA_FINE.Value.AddDays(1),
                                    DTA_FINE = fine,
                                    //NUM_GIORNI_MAX = numGiorni.HasValue ? numGiorni.Value : item.NUM_GIORNI_MAX,
                                    //NUM_GIORNI_EXTRA = numGiorniExtra.HasValue ? item.NUM_GIORNI_EXTRA.GetValueOrDefault() + numGiorniExtra : item.NUM_GIORNI_EXTRA,
                                    NUM_GIORNI_MAX = newMax,
                                    NUM_GIORNI_EXTRA = newExtra,
                                    VALID_DTA_INI = tms_timestamp,
                                    VALID_DTA_END = null,
                                    COD_USER = cod_user,
                                    COD_TERMID = cod_termid,
                                    TMS_TIMESTAMP = tms_timestamp,
                                    DTA_INVIO = item.DTA_INVIO,
                                    IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                    DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA,
                                    ID_RICH = idRichiesta,
                                    NUM_GIORNI_MAX_ORIG = item.NUM_GIORNI_MAX,
                                    NUM_GIORNI_EXTRA_ORIG = item.NUM_GIORNI_EXTRA
                                });
                            }
                            //Se il periodo inizia prima della richiesta ma finisce lo stesso giorno
                            else if (item.DTA_INIZIO < dtaInizio && item.DTA_FINE == fine)
                            {
                                rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                {
                                    DTA_INIZIO = item.DTA_INIZIO,
                                    DTA_FINE = dtaInizio.AddDays(-1),
                                    NUM_GIORNI_MAX = item.NUM_GIORNI_MAX,
                                    NUM_GIORNI_EXTRA = item.NUM_GIORNI_EXTRA,
                                    VALID_DTA_INI = tms_timestamp,
                                    VALID_DTA_END = null,
                                    COD_USER = cod_user,
                                    COD_TERMID = cod_termid,
                                    TMS_TIMESTAMP = tms_timestamp,
                                    DTA_INVIO = item.DTA_INVIO,
                                    IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                    DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA
                                });
                                //Variazione
                                CalcolaGiorni(item.NUM_GIORNI_MAX, item.NUM_GIORNI_EXTRA, numGiorni, numGiorniExtra, maxTotal, out newMax, out newExtra);
                                rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                {
                                    DTA_INIZIO = dtaInizio,
                                    DTA_FINE = fine,
                                    //NUM_GIORNI_MAX = numGiorni.HasValue ? numGiorni.Value : item.NUM_GIORNI_MAX,
                                    //NUM_GIORNI_EXTRA = numGiorniExtra.HasValue ? item.NUM_GIORNI_EXTRA.GetValueOrDefault() + numGiorniExtra : item.NUM_GIORNI_EXTRA,
                                    NUM_GIORNI_MAX = newMax,
                                    NUM_GIORNI_EXTRA = newExtra,
                                    VALID_DTA_INI = tms_timestamp,
                                    VALID_DTA_END = null,
                                    COD_USER = cod_user,
                                    COD_TERMID = cod_termid,
                                    TMS_TIMESTAMP = tms_timestamp,
                                    DTA_INVIO = item.DTA_INVIO,
                                    IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                    DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA,
                                    ID_RICH = idRichiesta,
                                    NUM_GIORNI_MAX_ORIG = item.NUM_GIORNI_MAX,
                                    NUM_GIORNI_EXTRA_ORIG = item.NUM_GIORNI_EXTRA
                                });
                            }
                            else if (item.DTA_INIZIO >= dtaInizio && item.DTA_FINE > fine)
                            {
                                //Variazione
                                CalcolaGiorni(item.NUM_GIORNI_MAX, item.NUM_GIORNI_EXTRA, numGiorni, numGiorniExtra, maxTotal, out newMax, out newExtra);
                                rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                {
                                    DTA_INIZIO = item.DTA_INIZIO,
                                    DTA_FINE = fine.Value,
                                    //NUM_GIORNI_MAX = numGiorni.HasValue ? numGiorni.Value : item.NUM_GIORNI_MAX,
                                    //NUM_GIORNI_EXTRA = numGiorniExtra.HasValue ? item.NUM_GIORNI_EXTRA.GetValueOrDefault() + numGiorniExtra : item.NUM_GIORNI_EXTRA,
                                    NUM_GIORNI_MAX = newMax,
                                    NUM_GIORNI_EXTRA = newExtra,
                                    VALID_DTA_INI = tms_timestamp,
                                    VALID_DTA_END = null,
                                    COD_USER = cod_user,
                                    COD_TERMID = cod_termid,
                                    TMS_TIMESTAMP = tms_timestamp,
                                    DTA_INVIO = item.DTA_INVIO,
                                    IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                    DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA,
                                    ID_RICH = idRichiesta,
                                    NUM_GIORNI_MAX_ORIG = item.NUM_GIORNI_MAX,
                                    NUM_GIORNI_EXTRA_ORIG = item.NUM_GIORNI_EXTRA
                                });
                                rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                {
                                    DTA_INIZIO = fine.Value.AddDays(1),
                                    DTA_FINE = item.DTA_FINE,
                                    NUM_GIORNI_MAX = item.NUM_GIORNI_MAX,
                                    NUM_GIORNI_EXTRA = item.NUM_GIORNI_EXTRA,
                                    VALID_DTA_INI = tms_timestamp,
                                    VALID_DTA_END = null,
                                    COD_USER = cod_user,
                                    COD_TERMID = cod_termid,
                                    TMS_TIMESTAMP = tms_timestamp,
                                    DTA_INVIO = item.DTA_INVIO,
                                    IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                    DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA
                                });
                            }
                            else
                            {
                                output += "--\r\n";
                                update = false;
                            }
                        }

                    }
                    if (update)
                    {
                        if (!DBHelper.Save(dbTal, "Task", "Aggiornamento richieste"))
                        {
                            errore += "Matricola: {0} - Rich: {1} - Errore nel salvataggio dei dati";
                        }
                        else
                        {
                            output += String.Format("Matricola: {0} - Rich: {1} - Aggiornato {2:dd/MM/yyyy} - {3:dd/MM/yyyy}", matricola, idRichiesta, inizio, fine);
                            foreach (var item in rapporto.XR_STATO_RAPPORTO_INFO.Where(x => x.VALID_DTA_END == null).OrderBy(x => x.DTA_INIZIO))
                            {
                                output += String.Format("\t{0:dd/MM/yyyy}-{1:dd/MM/yyyy}", item.DTA_INIZIO, item.DTA_FINE);
                            }
                            output += "\r\n";
                        }
                    }
                    else
                    {
                        errore += "Matricola: {0} - Rich: {1} - Errore nella conversione dei periodi";
                    }
                }
            }
        }

        public static bool AnnullaModificheRichiestaSW(XR_MAT_RICHIESTE rich, out string errore)
        {
            bool result = false;
            errore = null;

            try
            {
                var db = new TalentiaDB();
                var infos = db.XR_STATO_RAPPORTO_INFO.Where(x => x.ID_RICH == rich.ID && x.VALID_DTA_END == null).ToList();
                CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);
                foreach (var item in infos)
                {
                    item.VALID_DTA_END = tms;
                    db.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                    {
                        DTA_INIZIO = item.DTA_INIZIO,
                        DTA_FINE = item.DTA_FINE,
                        NUM_GIORNI_MAX = item.NUM_GIORNI_MAX_ORIG,
                        NUM_GIORNI_EXTRA = item.NUM_GIORNI_EXTRA_ORIG,
                        VALID_DTA_INI = tms,
                        VALID_DTA_END = null,
                        COD_USER = codUser,
                        COD_TERMID = codTermid,
                        TMS_TIMESTAMP = tms,
                        DTA_INVIO = item.DTA_INVIO,
                        IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                        DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA,
                        ID_RICH = null,
                        NUM_GIORNI_MAX_ORIG = null,
                        NUM_GIORNI_EXTRA_ORIG = null
                    });
                }

                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "Rifiuto richiesta");
                if (!result)
                    errore = "Errore durante il salvataggio";

                if (result)
                {
                    if (rich.XR_MAT_CATEGORIE.DESCRIZIONE_ECCEZIONE.Contains("GAPP"))
                    {
                        //In questo caso deve rimuovere le eccezioni da GAPP
                        DateTime D1 = rich.INIZIO_GIUSTIFICATIVO.Value;
                        DateTime D2 = rich.FINE_GIUSTIFICATIVO.Value;

                        MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp service =
                        new MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp()
                        {
                            Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
                        };

                        string eccezione = "SW";
                        string matricola = rich.MATRICOLA;

                        string errorEcc = "";
                        bool esitoEcc = true;

                        for (DateTime D = D1; D <= D2; D = D.AddDays(1))
                        {
                            var verifica = service.getEccezioni(rich.MATRICOLA, D.ToString("ddMMyyyy"), "BU", 75);
                            if (verifica.eccezioni != null && verifica.eccezioni.Any())
                            {
                                bool presente = verifica.eccezioni.Where(w => !String.IsNullOrEmpty(w.cod) && w.cod.Trim().Equals(eccezione)).Count() > 0;

                                if (presente)
                                {
                                    List<MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione> listaEccezioni =
                                        verifica.eccezioni.Where(w => !String.IsNullOrEmpty(w.cod) && w.cod.Trim().Equals(eccezione)).ToList();

                                    if (listaEccezioni != null && listaEccezioni.Any())
                                    {
                                        foreach (var e in listaEccezioni)
                                        {
                                            e.matricola = rich.MATRICOLA;
                                        }
                                        var resp = ServiceWrapper.validaEccezioni(service, "UFFPERS", "01", listaEccezioni.ToArray(), false, 75);

                                        if (!resp.esito)
                                        {
                                            esitoEcc = false;
                                            if (!String.IsNullOrWhiteSpace(errorEcc)) errorEcc += "\r\n";
                                            errorEcc += "Errore durante la cancellazione da GAPP per la giornata " + D.ToString("dd/MM/yyyy");
                                        }
                                    }

                                    #region AGGIORNAMENTO RECORD SU DIGIGAPP
                                    try
                                    {
                                        var dbDG = new digiGappEntities();
                                        var EccDB = dbDG.MyRai_Eccezioni_Richieste.Where(x => x.data_eccezione == D).Where(x => x.azione == "I" && x.id_stato != 60 && x.id_stato != 50 && x.id_stato != 70 && x.MyRai_Richieste.matricola_richiesta == matricola).ToList();

                                        Boolean DBtoUpdate = false;
                                        List<int> Lid = new List<int>();

                                        if (EccDB == null || !EccDB.Any())
                                        {
                                            //
                                            //Output.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " Matricola " + matricola + " non ci sono eccezioni nella giornata su digigapp");
                                            if (!String.IsNullOrWhiteSpace(errorEcc)) errorEcc += "\r\n";
                                            errorEcc += "Eccezione non trovata su DB per la giornata " + D.ToString("dd/MM/yyyy");
                                        }
                                        else
                                        {
                                            foreach (MyRai_Eccezioni_Richieste m in EccDB)
                                            {
                                                if (m.numero_documento == 0)
                                                    continue;

                                                if (!verifica.eccezioni.Select(x => x.ndoc)
                                                    .ToList()
                                                    .Contains(m.numero_documento.ToString().PadLeft(6, '0')))
                                                {
                                                    var stornoDB = dbDG.MyRai_Eccezioni_Richieste.Where(x => x.azione == "C" && x.numero_documento_riferimento == m.numero_documento && x.codice_sede_gapp == m.codice_sede_gapp && x.id_stato == (int)EnumStatiRichiesta.Approvata).FirstOrDefault();

                                                    if (m.id_stato == (int)EnumStatiRichiesta.Approvata && stornoDB != null)
                                                    {
                                                        m.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                                        m.MyRai_Richieste.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                                        MyRai_LogAzioni la = new MyRai_LogAzioni()
                                                        {
                                                            applicativo = "StornoGiorno",
                                                            data = DateTime.Now,
                                                            matricola = matricola,
                                                            provenienza = "StornoEccezioneGiorno",
                                                            operazione = "ELIMINATA SU GAPP",
                                                            descrizione_operazione = "id rich:" + m.MyRai_Richieste.id_richiesta + " id ecc_rich:" + m.id_eccezioni_richieste
                                                        };
                                                        dbDG.MyRai_LogAzioni.Add(la);
                                                        DBtoUpdate = true;
                                                        Lid.Add(m.MyRai_Richieste.id_richiesta);
                                                    }
                                                    else
                                                    {
                                                        if (m.cod_eccezione.Trim().Equals(eccezione))
                                                        {
                                                            m.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                                            m.MyRai_Richieste.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                                            MyRai_LogAzioni la = new MyRai_LogAzioni()
                                                            {
                                                                applicativo = "StornoGiorno",
                                                                data = DateTime.Now,
                                                                matricola = matricola,
                                                                provenienza = "StornoEccezioneGiorno",
                                                                operazione = "ELIMINATA SU GAPP",
                                                                descrizione_operazione = "id rich:" + m.MyRai_Richieste.id_richiesta + " id ecc_rich:" + m.id_eccezioni_richieste
                                                            };
                                                            dbDG.MyRai_LogAzioni.Add(la);
                                                            DBtoUpdate = true;
                                                            Lid.Add(m.MyRai_Richieste.id_richiesta);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (m.cod_eccezione.Trim().Equals(eccezione))
                                                    {
                                                        m.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                                        m.MyRai_Richieste.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                                        MyRai_LogAzioni la = new MyRai_LogAzioni()
                                                        {
                                                            applicativo = "StornoGiorno",
                                                            data = DateTime.Now,
                                                            matricola = matricola,
                                                            provenienza = "StornoEccezioneGiorno",
                                                            operazione = "ELIMINATA SU GAPP",
                                                            descrizione_operazione = "id rich:" + m.MyRai_Richieste.id_richiesta + " id ecc_rich:" + m.id_eccezioni_richieste
                                                        };
                                                        dbDG.MyRai_LogAzioni.Add(la);
                                                        DBtoUpdate = true;
                                                        Lid.Add(m.MyRai_Richieste.id_richiesta);
                                                    }
                                                }
                                            }
                                            if (DBtoUpdate)
                                            {
                                                db.SaveChanges();
                                                //Output.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " Matricola " + matricola + " record aggiornati su digigapp");
                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        esitoEcc = false;
                                        if (!String.IsNullOrWhiteSpace(errorEcc)) errorEcc += "\r\n";
                                        errorEcc += "Errore durante la cancellazione da DB per la giornata " + D.ToString("dd/MM/yyyy");
                                    }
                                    #endregion
                                }

                            }
                        }

                        if (!esitoEcc)
                        {
                            result = false;
                            errore = errorEcc;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errore = ex.Message;
            }


            return result;
        }
    }
}
