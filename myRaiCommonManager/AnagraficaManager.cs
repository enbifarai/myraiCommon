using myRai.Data.CurriculumVitae;
using myRai.DataAccess;
using myRaiCommonModel;
using myRaiCommonModel.ess;
using myRaiCommonModel.RaiAcademy;
using myRaiCommonModel.Services;
using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;
using myRaiServiceHub.Autorizzazioni;
using myRaiServiceHub.it.rai.servizi.hrgb;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

using CezanneDb = myRaiData.Incentivi.IncentiviEntities;
using TalentiaDB = myRaiDataTalentia.TalentiaEntities;


namespace myRaiCommonManager
{

    public partial class AnagraficaManager
    {
        const int WKF_VAR_CONTR = 18;
        enum VarContrStati
        {
            RichiestaInserita = 1,
            RichiestaEvasa = 2,
            RichiestaCancellata = 3
        }

        public static CezanneDb GetDb()
        {
            return new CezanneDb();
            //return new CezanneDb("IncentiviEntities_Talentia");

            //if (CommonHelper.IsProduzione())
            //    return new CezanneDb("IncentiviEntities");
            //else
            //    return new CezanneDb("IncentiviEntities_Talentia");
        }

        public static CezanneDb GetDbHRDW()
        {
            return new CezanneDb();
        }

        public static CezanneDb GetDbHR_Liv2()
        {
            if (CommonHelper.IsProduzione())
                return new CezanneDb("IncentiviEntities");
            else
                return new CezanneDb("IncentiviEntities_Cezanne");
        }

        public static CezanneDb GetDbIban()
        {
            ////Il db di Cezanne di collaudo non è raggiungibile in VPN
            ////if (System.Diagnostics.Debugger.IsAttached && !CommonHelper.IsProduzione() && CommonHelper.CheckForVPNInterface())
            ////    return new CezanneDb("IncentiviEntities");
            ////else
            //    return new CezanneDb("IncentiviEntities_Cezanne");
            var flagdb = CommonHelper.GetParametri<string>(EnumParametriSistema.FlagTalentiaCezanne);
            if (flagdb[0].Equals("0"))
            {
                return new CezanneDb("IncentiviEntities_Cezanne");
            }
            else
            {
                return new CezanneDb("IncentiviEntities_Talentia");
            }
        }

        private static bool IsEnabledSez(AnagraficaModel model, SezioniAnag sezione, AbilMatrLiv auth, bool checkAbil)
        {
            bool result = false;
            //auth.LivelloAnagrafico = true;
            //auth.LivelloGestionale = true;
            //auth.LivelloRetributivo = true;
            string matricola = CommonHelper.GetCurrentUserMatricola();
            bool isLivEnabled = IsLivEnabled(sezione, auth);

            string hrisAbil = AuthHelper.HrisFuncAbil(CommonHelper.GetCurrentUserMatricola());
            if (String.IsNullOrWhiteSpace(hrisAbil))
                hrisAbil = "HRCE";

            switch (sezione)
            {
                case SezioniAnag.NonDefinito:
                    break;
                case SezioniAnag.Anagrafici:
                    if (hrisAbil == "HRCE")
                        result = model.DatiAnagrafici.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRCE", "AGG_ANAG", "EVID_ANAG"));
                    else
                        result = model.DatiAnagrafici.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRIS_PERS", "ANAGES", "ANAVIS"));
                    break;
                case SezioniAnag.Recapiti:
                    //result = model.DatiRecapiti.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRCE", "AGG_ANAG", "EVID_ANAG"));// auth.LivelloAnagrafico;
                    if (hrisAbil == "HRCE")
                        result = model.DatiRecapiti.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRCE", "AGG_ANAG", "EVID_ANAG"));
                    else
                        result = model.DatiRecapiti.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRIS_PERS", "RECGES", "RECVIS"));
                    break;
                case SezioniAnag.Residenza:
                case SezioniAnag.Domicilio:
                    //result = model.DatiResidenzaDomicilio.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRCE", "AGG_ANAG", "EVID_ANAG"));// auth.LivelloAnagrafico;
                    if (hrisAbil == "HRCE")
                        result = model.DatiResidenzaDomicilio.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRCE", "AGG_ANAG", "EVID_ANAG"));
                    else
                        result = model.DatiResidenzaDomicilio.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRIS_PERS", "RESADM", "RESGES", "RESVIS", "DOMGES", "DOMVIS"));
                    break;
                case SezioniAnag.TitoliStudio:
                    //result = model.DatiTitoliStudio.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRCE", "AGG_ANAG", "EVID_ANAG"));// auth.LivelloAnagrafico;
                    if (hrisAbil == "HRCE")
                        result = model.DatiTitoliStudio.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRCE", "AGG_ANAG", "EVID_ANAG"));
                    else
                        result = model.DatiTitoliStudio.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRIS_PERS", "STUGES", "STUVIS"));
                    break;
                case SezioniAnag.Bancari:
                    if (hrisAbil == "HRCE")
                        result = model.DatiBancari.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRCE", "DATI_BANCARI"));
                    else
                        result = model.DatiBancari.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRIS_PERS", "BNKGES", "BNKVIS"));
                    break;
                case SezioniAnag.StatoRapporto:
                    result = (model.DatiStatiRapporti.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F08")));
                    break;
                case SezioniAnag.TipoContratti:
                case SezioniAnag.Sedi:
                case SezioniAnag.Servizi:
                case SezioniAnag.Qualifiche:
                case SezioniAnag.Ruoli:
                    model.DatiContratti.IsEnabled = !checkAbil || (IsLivEnabled(SezioniAnag.TipoContratti, auth) && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F02"));
                    model.DatiSedi.IsEnabled = !checkAbil || (IsLivEnabled(SezioniAnag.Sedi, auth) && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F02"));
                    model.DatiServizi.IsEnabled = !checkAbil || (IsLivEnabled(SezioniAnag.Servizi, auth) && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F02"));
                    model.DatiQualifiche.IsEnabled = !checkAbil || (IsLivEnabled(SezioniAnag.Qualifiche, auth) && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F02"));
                    model.DatiRuoli.IsEnabled = !checkAbil || (IsLivEnabled(SezioniAnag.Ruoli, auth) && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F02"));
                    result = true;
                    break;

                //case SezioniAnag.TipoContratti:
                //    model.DatiContratti.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F02"));
                //    result = model.DatiContratti.IsEnabled;
                //    break;
                //case SezioniAnag.Sedi:
                //case SezioniAnag.Servizi:
                //case SezioniAnag.Qualifiche:
                //case SezioniAnag.Ruoli:                    
                //    model.DatiSedi.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F02"));
                //    model.DatiServizi.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F02"));
                //    model.DatiQualifiche.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F02"));
                //    model.DatiRuoli.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F02"));
                //    result = true;
                //    break;
                case SezioniAnag.Retribuzione:
                    result = !model.IsNeoMatr && (model.DatiRedditi.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F03")));
                    break;
                case SezioniAnag.Debitoria:
                    result = !model.IsNeoMatr && (model.DatiSituazioneDebitoria.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F06")));
                    break;
                case SezioniAnag.Formazione:
                    result = !model.IsNeoMatr && (model.DatiFormazione.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F16")));
                    break;
                case SezioniAnag.Struttura:
                    result = model.DatiStruttOrg.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F02"));
                    break;
                case SezioniAnag.Presenze:
                    result = !model.IsNeoMatr && (model.DatiPresenze.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F08")));
                    break;
                case SezioniAnag.Contenzioso:
                    result = !model.IsNeoMatr && (model.DatiContenzioso.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F07")));
                    break;
                case SezioniAnag.Sezioni:
                    result = model.DatiSezioni.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F02"));
                    break;
                case SezioniAnag.MieiDoc:
                    model.DematerializzazioneMieiDocumenti = new AnagraficaDematerializzazioneDocumenti();
                    result = model.DematerializzazioneMieiDocumenti.IsEnabled = !model.IsNeoMatr && (!checkAbil || isLivEnabled || AuthHelper.EnabledToAnySubFunc(matricola, "DEMA"));
                    break;
                case SezioniAnag.Cedolini:
                    result = !model.IsNeoMatr && (model.DatiCedolini.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F04")));
                    break;
                case SezioniAnag.Curricula:
                    result = !model.IsNeoMatr && (model.DatiCurriculum.IsEnabled = !checkAbil || (isLivEnabled && AuthHelper.EnabledToAnySubFunc(matricola, "HRA", "F17")));
                    break;
                case SezioniAnag.Trasferte:
                    result = model.DatiTrasferte.IsEnabled = (matricola == "103650" ? true : false);

                    break;
                case SezioniAnag.SpeseProduzione:
                    result = model.DatiSpeseProduzione.IsEnabled = (matricola == "103650" ? true : false);
                    break;
                case SezioniAnag.Documenti:
                    impostaAbilitazioneDocumenti(model, matricola);
                    result = model.DatiDocumenti.IsEnabled;
                    break;
                default:
                    result = false;
                    break;
            }

            return result;// && !model.IsNeoMatr;
        }

        public static List<CompetenzeDigitali> GetCompDigitali(string matricola)
        {
            List<CompetenzeDigitali> LD = new List<CompetenzeDigitali>();
            var cvEnt = new myRaiData.CurriculumVitae.cv_ModelEntities();

            var competenze = cvEnt.TCVCompDigit.Where(m => m.Matricola == matricola).ToList();

            if (competenze.Count > 0)
            {
                foreach (var elem in competenze.OrderByDescending(a => a.CodCompDigitLiv))
                {
                    CompetenzeDigitali frk_comepetenze = new CompetenzeDigitali();
                    LD.Add(frk_comepetenze);
                    string descCompDigit, descCompDigitLiv, descCompDigitLivLunga;

                    frk_comepetenze._codCompDigit = elem.CodCompDigit;
                    frk_comepetenze._matricola = matricola;
                    frk_comepetenze._stato = elem.Stato;
                    frk_comepetenze._tipoAgg = elem.TipoAgg;
                    frk_comepetenze._dataOraAgg = elem.DataOraAgg;
                    frk_comepetenze._codCompDigitLiv = elem.CodCompDigitLiv;
                    if (elem.CodCompDigitLiv == "")
                    {
                        descCompDigitLiv = "";
                        descCompDigitLivLunga = "";
                    }
                    else
                    {
                        descCompDigitLiv = (from compDigitLiv in cvEnt.DCompDigitLiv
                                            where (compDigitLiv.CodCompDigit == elem.CodCompDigit && compDigitLiv.CodCompDigitLiv == elem.CodCompDigitLiv)
                                            select compDigitLiv.DescCompDigitLiv).First().ToString();
                        descCompDigitLivLunga = (from compDigitLivLunga in cvEnt.DCompDigitLiv
                                                 where (compDigitLivLunga.CodCompDigit == elem.CodCompDigit && compDigitLivLunga.CodCompDigitLiv == elem.CodCompDigitLiv)
                                                 select compDigitLivLunga.DescCompDigitLivLunga).First().ToString();
                    }
                    descCompDigit = (from compDigit in cvEnt.DCompDigit
                                     where compDigit.CodCompDigit == elem.CodCompDigit
                                     select compDigit.DescCompDigit).First().ToString();
                    frk_comepetenze._descCompDigit = descCompDigit;
                    frk_comepetenze._descCompDigitLiv = descCompDigitLiv;
                    frk_comepetenze._descCompDigitLivLunga = Regex.Replace(descCompDigitLivLunga, "<.*?>", String.Empty);


                }
            }
            return LD;
        }

        public static bool AbilitatoRicercaMatCongedi(string matricola)
        {
            return MaternitaCongediHelper.EnabledToMaternitaCongedi(matricola);
        }

        public static bool EseguiMetodo(string cmd)
        {
            string[] parts = cmd.Split('.');
            string type = String.Join(".", parts.ToList().Take(parts.Length - 1).ToArray());
            string method = parts[parts.Length - 1].Split('(')[0].Trim();
            string argument = null;


            Match result = new Regex("\\(\"(.*)\"\\)").Match(cmd);

            if (result.Success) argument = result.Groups[1].Value;

            object resp = Type.GetType(type).GetMethod(method).Invoke(null, argument == null ? null : new object[] { argument });
            return (bool)resp;
        }
        public static List<RicercaDinamicaItem> GetRicercaDinamicaItems()
        {
            List<RicercaDinamicaItem> ItemListResult = new List<RicercaDinamicaItem>();

            var db = new IncentiviEntities();
            var items = db.XR_HRIS_PARAM.Where(x => x.COD_PARAM == "RicercaDinamica").ToList();
            if (!items.Any()) return ItemListResult;

            foreach (var item in items)
            {
                if (String.IsNullOrWhiteSpace(item.COD_VALUE3) ||
                    EseguiMetodo(item.COD_VALUE3.Replace("#MATR", CommonHelper.GetCurrentUserMatricola())))
                {
                    ItemListResult.Add(new RicercaDinamicaItem() { ItemText = item.COD_VALUE1, ItemCode = item.COD_VALUE2 });
                }
            }
            return ItemListResult;
        }

        private static bool IsLivEnabled(SezioniAnag sezione, AbilMatrLiv auth)
        {
            bool isEnabled = false;

            switch (sezione)
            {
                case SezioniAnag.NonDefinito:
                    break;
                case SezioniAnag.Anagrafici:
                case SezioniAnag.Residenza:
                case SezioniAnag.Domicilio:
                case SezioniAnag.TitoliStudio:
                case SezioniAnag.Bancari:
                case SezioniAnag.Recapiti:
                case SezioniAnag.TipoContratti:
                case SezioniAnag.Sedi:
                case SezioniAnag.Servizi:
                case SezioniAnag.Qualifiche:
                case SezioniAnag.Ruoli:
                case SezioniAnag.Sezioni:
                    isEnabled = auth.LivelloAnagrafico;
                    break;
                case SezioniAnag.StatoRapporto:
                //case SezioniAnag.TipoContratti:
                //case SezioniAnag.Sedi:
                //case SezioniAnag.Servizi:
                //case SezioniAnag.Qualifiche:
                //case SezioniAnag.Ruoli:
                case SezioniAnag.Formazione:
                case SezioniAnag.Struttura:
                case SezioniAnag.Presenze:
                case SezioniAnag.Contenzioso:
                //case SezioniAnag.Sezioni:
                case SezioniAnag.Curricula:
                    isEnabled = auth.LivelloGestionale;
                    break;
                case SezioniAnag.Retribuzione:
                case SezioniAnag.Debitoria:
                case SezioniAnag.Cedolini:
                    isEnabled = auth.LivelloRetributivo;
                    break;
                case SezioniAnag.MieiDoc:
                    isEnabled = auth.LivelloGestionale;
                    break;
                case SezioniAnag.Trasferte:
                    isEnabled = true;
                    break;
                case SezioniAnag.SpeseProduzione:
                    isEnabled = true;
                    break;
                case SezioniAnag.Documenti:
                    isEnabled = true;
                    break;
                default:
                    break;
            }

            return isEnabled;
        }



        private static bool HideStoricoContratti(SezioniAnag sezione, AbilMatrLiv auth)
        {
            bool isEnabled = true;

            switch (sezione)
            {
                case SezioniAnag.TipoContratti:
                    isEnabled = !auth.LivelloGestionale;
                    break;
                default:
                    break;
            }

            return isEnabled;
        }



        private static void CheckEnabledCRUD(AnagraficaModel model, AnagraficaLoader loader, SezioniAnag sezione)
        {
            BaseAnagraficaData baseAnagrafica = null;

            switch (sezione)
            {
                case SezioniAnag.NonDefinito:
                    break;
                case SezioniAnag.Anagrafici:
                    baseAnagrafica = model.DatiAnagrafici;
                    break;
                case SezioniAnag.Recapiti:
                    baseAnagrafica = model.DatiRecapiti;
                    break;
                case SezioniAnag.Residenza:
                    baseAnagrafica = model.DatiResidenzaDomicilio.Residenza;
                    break;
                case SezioniAnag.Domicilio:
                    baseAnagrafica = model.DatiResidenzaDomicilio.Domicilio;
                    break;
                case SezioniAnag.TitoliStudio:
                    baseAnagrafica = model.DatiTitoliStudio;
                    break;
                case SezioniAnag.Bancari:
                    baseAnagrafica = model.DatiBancari;
                    break;
                case SezioniAnag.TipoContratti:
                    baseAnagrafica = model.DatiContratti;
                    break;
                case SezioniAnag.StatoRapporto:
                    baseAnagrafica = model.DatiStatiRapporti;
                    break;
                case SezioniAnag.Sedi:
                    baseAnagrafica = model.DatiSedi;
                    break;
                case SezioniAnag.Servizi:
                    baseAnagrafica = model.DatiServizi;
                    break;
                case SezioniAnag.Qualifiche:
                    baseAnagrafica = model.DatiQualifiche;
                    break;
                case SezioniAnag.Ruoli:
                    baseAnagrafica = model.DatiRuoli;
                    break;
                case SezioniAnag.Retribuzione:
                    baseAnagrafica = model.DatiRedditi;
                    break;
                case SezioniAnag.Debitoria:
                    baseAnagrafica = model.DatiSituazioneDebitoria;
                    break;
                case SezioniAnag.Formazione:
                    baseAnagrafica = model.DatiFormazione;
                    break;
                case SezioniAnag.Struttura:
                    baseAnagrafica = model.DatiStruttOrg;
                    break;
                case SezioniAnag.Presenze:
                    baseAnagrafica = model.DatiPresenze;
                    break;
                case SezioniAnag.Contenzioso:
                    baseAnagrafica = model.DatiContenzioso;
                    break;
                case SezioniAnag.Sezioni:
                    baseAnagrafica = model.DatiSezioni;
                    break;
                case SezioniAnag.MieiDoc:
                    baseAnagrafica = model.DematerializzazioneMieiDocumenti;
                    break;
                case SezioniAnag.Cedolini:
                    baseAnagrafica = model.DatiCedolini;
                    break;
                case SezioniAnag.Curricula:
                    baseAnagrafica = model.DatiCurriculum;
                    break;
                case SezioniAnag.Trasferte:
                    baseAnagrafica = model.DatiTrasferte;
                    break;
                case SezioniAnag.SpeseProduzione:
                    baseAnagrafica = model.DatiSpeseProduzione;
                    break;
                case SezioniAnag.Familiari:
                    baseAnagrafica = model.DatiFamiliari;
                    break;
                case SezioniAnag.Documenti:
                    baseAnagrafica = model.DatiDocumenti;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (baseAnagrafica != null)
            {
                baseAnagrafica.CanAdd = baseAnagrafica.CanAdd && loader.EnabledAdd;
                baseAnagrafica.CanModify = baseAnagrafica.CanModify && loader.EnabledModify;
                baseAnagrafica.CanDelete = baseAnagrafica.CanDelete && loader.EnabledDelete;
            }
        }

        private static void LoadSez(AnagraficaModel model, AnagraficaLoader loader, bool checkAbil, SezioniAnag sezione, AbilMatrLiv auth, System.Action method)
        {
            if (sezione == SezioniAnag.Familiari)
            {
                sezione = SezioniAnag.Anagrafici;
                //if (CommonHelper.GetCurrentUserMatricola() == "103650")
                //{
                //    method();
                //    return;
                //}
            }

            if (loader == null || (loader.Sezioni.Contains(sezione) && !loader.SezioniAsync.Contains(sezione)))
            {
                if (IsEnabledSez(model, sezione, auth, checkAbil))
                    method();
            }
            else if (loader.SezioniAsync.Contains(sezione))
            {
                IsEnabledSez(model, sezione, auth, checkAbil);
            }
            CheckEnabledCRUD(model, loader, sezione);

        }

        public static AnagraficaModel GetAnagrafica(string matricola, AnagraficaLoader loader = null, bool checkAbil = true, string customFunc = "")
        {
            AnagraficaModel anag = new AnagraficaModel();
            anag.CustomFunc = customFunc;
            var dbCzn = GetDb();
            SINTESI1 sint = dbCzn.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
            CaricaAnagrafica(loader, anag, dbCzn, sint, checkAbil);

            return anag;
        }
        public static AnagraficaModel GetAnagrafica(string matricola, int? idPersona, AnagraficaLoader loader = null, bool checkAbil = true, string customFunc = "", bool fromAssunzioni = false)
        {
            AnagraficaModel anag = new AnagraficaModel();
            anag.CustomFunc = customFunc;
            var dbCzn = GetDb();
            SINTESI1 sint = null;

            if (!String.IsNullOrWhiteSpace(matricola))
                sint = dbCzn.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
            else
                sint = dbCzn.SINTESI1.FirstOrDefault(x => x.ID_PERSONA == idPersona.Value);

            CaricaAnagrafica(loader, anag, dbCzn, sint, checkAbil, fromAssunzioni);

            return anag;
        }

        public static List<Languages> GetLingue(string matricola)
        {
            List<Languages> Lingue = new List<Languages>();
            var cvEnt = new myRaiData.CurriculumVitae.cv_ModelEntities();


            var lingue = (cvEnt.TCVLingue.Where(m => m.Matricola == matricola).OrderBy(y => y.CodLinguaLiv)).ToList();

            foreach (var lang in lingue)
            {
                Languages frk = new Languages();
                Lingue.Add(frk);
                frk._altraLingua = lang.AltraLingua;
                frk._codLingua = lang.CodLingua;
                frk._codLinguaLiv = lang.CodLinguaLiv;
                frk._dataOraAgg = lang.DataOraAgg;
                frk._livAscolto = lang.LivAscolto;
                frk._livInterazione = lang.LivInterazione;
                frk._livLettura = lang.LivLettura;
                frk._livProdOrale = lang.LivProdOrale;
                frk._livScritto = lang.LivScritto;
                frk._matricola = lang.Matricola;
                frk._stato = lang.Stato;
                frk._tipoAgg = lang.TipoAgg;

                //Descrizione Lingua
                var tmp_lingua = cvEnt.DLingua.Where(m => m.CodLingua == lang.CodLingua).First();
                frk._descLingua = tmp_lingua.DescLingua;

                //Flag dello Stato
                frk._flagStato = tmp_lingua.FlagStato;

                //Descrizione Livello di Lingua
                var tmp_lingualiv = cvEnt.DLinguaLiv.Where(m => m.CodLinguaLiv == lang.CodLinguaLiv).First();
                frk._descLinguaLiv = tmp_lingualiv.DescLinguaLiv;

                //if (!string.IsNullOrEmpty(frk._descLingua))
                //{
                //    string key = textInfo.ToTitleCase(frk._descLingua) + "\r\n";

                //    string value = "";
                //    value += " Ascolto " + (!string.IsNullOrEmpty(frk._livAscolto) ? frk._livAscolto : "nd") + " - ";
                //    value += " Lettura " + (!string.IsNullOrEmpty(frk._livLettura) ? frk._livLettura : "nd") + " - ";
                //    value += " Interazione " + (!string.IsNullOrEmpty(frk._livInterazione) ? frk._livInterazione : "nd") + " - ";
                //    value += " Produzione orale " + (!string.IsNullOrEmpty(frk._livProdOrale) ? frk._livProdOrale : "nd") + " - ";
                //    value += " Scritto " + (!string.IsNullOrEmpty(frk._livScritto) ? frk._livScritto : "nd");

                //    blockItemInfoList.Add(new BlockItemInfo
                //    {
                //        key = key,
                //        value = value
                //    });
                //}
            }
            return Lingue;
        }

        private static void CaricaAnagrafica(AnagraficaLoader loader, AnagraficaModel anag, CezanneDb dbCzn, SINTESI1 sint, bool checkAbil = true, bool fromAssunzioni = false)
        {
            if (sint == null)
            {
                anag.CodErrorMsg = "404";
                anag.ErrorMsg = "Matricola non trovata";
                return;
            }

            if (loader == null)
                loader = new AnagraficaLoader(new SezioniAnag[] { });

            string hrisAbil = AuthHelper.HrisFuncAbil(CommonHelper.GetCurrentUserMatricola());
            if (String.IsNullOrWhiteSpace(hrisAbil))
                hrisAbil = "HRCE";

            string abilFunc = "HRCE";
            if (hrisAbil != "HRCE")
                abilFunc = "HRIS_PERS";
            bool _checkAbil = checkAbil;
            if (!String.IsNullOrWhiteSpace(anag.CustomFunc))
            {
                _checkAbil = false;
                abilFunc = anag.CustomFunc;
                loader.EnabledAdd = false;
                loader.EnabledDelete = false;
                loader.EnabledModify = false;
            }

            AbilMatrLiv auth = null;
            if (checkAbil)
                auth = AuthHelper.EnableToMatr(CommonHelper.GetCurrentUserMatricola(), sint.COD_MATLIBROMAT, abilFunc);
            else
                auth = new AbilMatrLiv() { Enabled = true, LivelloAnagrafico = true, LivelloGestionale = true, LivelloRetributivo = true };

            anag.isAbilitatoGestionale = auth.LivelloGestionale;

            if (!auth.Enabled)
            {
                anag.CodErrorMsg = "401";
                anag.ErrorMsg = "Non abilitato";
                return;
            }

            //Verifica se la matricola è neo-immatricolata
            COMPREL comprel = null;
            if (sint.ID_COMPREL != null)
            {
                comprel = dbCzn.COMPREL.Find(sint.ID_COMPREL);
                if (comprel.JOMPREL.COD_IMMATRICOLAZIONE == "N")
                {
                    anag.IsNeoMatr = true;
                    //chiamata HRDW per verificare se è in servizio
                    var resDataAss = dbCzn.Database.SqlQuery<DateTime?>("SELECT TOP 1 data_assunzione from [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] where matricola_dp='" + sint.COD_MATLIBROMAT + "'");
                    if (resDataAss != null && resDataAss.Any())
                    {
                        if (resDataAss.First().HasValue && resDataAss.First().Value <= DateTime.Today)
                            anag.IsNeoMatr = false;
                    }
                }
            }
            else
                anag.IsNeoMatr = true;

            var dbHRDW = GetDbHRDW();
            var dbTal = new TalentiaDB();
            var dbI = new IncentiviEntities();
            var param = dbI.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "AnagraficaParametri");
            if (!String.IsNullOrWhiteSpace(param.COD_VALUE1))
                loader.Parametri.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<myRaiData.Incentivi.XR_HRIS_PARAM>>(param.COD_VALUE1));

            anag.IdPersona = sint.ID_PERSONA;
            anag.Matricola = sint.COD_MATLIBROMAT;

            anag.Nome = sint.DES_NOMEPERS.TitleCase();
            anag.Cognome = sint.DES_COGNOMEPERS.TitleCase() + (!String.IsNullOrWhiteSpace(sint.DES_SECCOGNOME) ? " " + sint.DES_SECCOGNOME : "").TitleCase();

            anag.Sesso = sint.COD_SESSO;

            anag.HasPFI = dbCzn.JOBASSIGN.Any(x => x.ID_PERSONA == sint.ID_PERSONA && x.RUOLO.COD_RUOLOAGGREG == "PROFORM") || (comprel != null && (comprel.JOMPREL.DES_CAUSALEMOV ?? "").ToUpper().StartsWith("APPRENDISTA"));

            //if (!anag.IsNeoMatr)
            {
                anag.DataAssunzione = sint.DTA_ANZCONV.GetValueOrDefault();
                anag.DataCessazione = sint.DTA_FINE_CR.HasValue ? sint.DTA_FINE_CR.Value : new DateTime(2999, 12, 31);

                if (sint.ASSQUAL != null && sint.ASSQUAL.JSSQUAL != null)
                    anag.DataAnzianitaCategoria = sint.ASSQUAL.JSSQUAL.DTA_ANZCAT;

                anag.Sede = CezanneHelper.GetDes(sint.COD_SEDE, sint.DES_SEDE).TitleCase();
                anag.Servizio = CezanneHelper.GetDes(sint.COD_SERVIZIO, sint.DES_SERVIZIO).TitleCase();
                anag.CodSezione = sint.COD_UNITAORG;
                anag.Sezione = CezanneHelper.GetDes(sint.COD_UNITAORG, sint.DES_DENOMUNITAORG).TitleCase();
                anag.Qualifica = sint.COD_QUALIFICA + " - " + CezanneHelper.GetDes(sint.COD_QUALIFICA, sint.DES_QUALIFICA).TitleCase();
                anag.Ruolo = sint.COD_RUOLO + " - " + CezanneHelper.GetDes(sint.COD_RUOLO, sint.DES_RUOLO).TitleCase();
                anag.FiguraProfessionale = sint.QUALIFICA.TB_QUALSTD?.DES_QUALSTD?.UpperFirst();

                anag.CodSede = sint.COD_SEDE;
                anag.CodServizio = sint.COD_SERVIZIO;
                anag.CodQualifica = sint.COD_QUALIFICA;
                anag.CodRuolo = sint.COD_RUOLO;

                anag.TipoContratto = sint.DES_TPCNTR;

                //NC - Questa è una soluzione momentanea finchè non viene implementata
                //la tabella dei codici contabili
                TQUALIFICA db2Qual = DB2Manager.SqlQuery<TQUALIFICA>(String.Format("SELECT * FROM OPENQUERY(DB2LINK, 'SELECT * FROM " + DB2Manager.GetPrefixTable() + ".TQUALIFICA WHERE matricola=''0{0}'' and DATINI<''{1:yyyy-MM-dd}'' and DATFIN>''{1:yyyy-MM-dd}''')", anag.Matricola, DateTime.Today)).FirstOrDefault();
                if (db2Qual != null)
                {
                    anag.AssicurazioneInfortuni = db2Qual.ASS_INF;
                    anag.AssicurazioneInfortuniUpdate = db2Qual.DATAGG;
                }
                var dbDigi = new digiGappEntities();
                var cacheCode = dbDigi.MyRai_CacheFunzioni.FirstOrDefault(x => x.oggetto == anag.Matricola && x.funzione == "AssicurazioneInfortuni" && x.data_creazione > anag.AssicurazioneInfortuniUpdate);
                if (cacheCode != null)
                {
                    anag.AssicurazioneInfortuni = cacheCode.dati_serial;
                    anag.AssicurazioneInfortuniUpdate = cacheCode.data_creazione;
                }

                anag.ShowPFI = anag.HasPFI;

                CaricaPartTime(anag, dbCzn);
            }
            //else
            //{
            //    XR_IMM_IMMATRICOLAZIONI imm = dbCzn.XR_IMM_IMMATRICOLAZIONI.FirstOrDefault(x => x.ID_PERSONA == anag.IdPersona);

            //    anag.DataAssunzione = imm.DTA_INIZIO;
            //    anag.DataCessazione = imm.DTA_FINE;

            //    var sede = dbCzn.SEDE.FirstOrDefault(x => x.COD_SEDE == imm.COD_SEDE);
            //    var servizio = dbCzn.XR_TB_SERVIZIO.FirstOrDefault(x => x.COD_SERVIZIO == imm.COD_SERVIZIO);
            //    var tpcntr = dbCzn.TB_TPRLAV.FirstOrDefault(x => x.COD_TIPORLAV == imm.COD_TIPORLAV);
            //    var qual = dbCzn.QUALIFICA.FirstOrDefault(x => x.COD_QUALIFICA == imm.COD_QUALIFICA);
            //    var sez = dbCzn.UNITAORG.FirstOrDefault(x => x.COD_UNITAORG == imm.COD_SEZIONE);

            //    anag.Sede = CezanneHelper.GetDes(sede.COD_SEDE, sede.DES_SEDE).TitleCase();
            //    anag.Servizio = CezanneHelper.GetDes(servizio.COD_SERVIZIO, servizio.DES_SERVIZIO).TitleCase();
            //    anag.Qualifica = qual.COD_QUALIFICA + " - " + CezanneHelper.GetDes(qual.COD_QUALIFICA, qual.DES_QUALIFICA).TitleCase();
            //    anag.Sezione = CezanneHelper.GetDes(sez.COD_UNITAORG, sez.DES_DENOMUNITAORG).TitleCase();
            //    anag.TipoContratto = tpcntr.DES_TIPORLAV;

            //    if (!String.IsNullOrWhiteSpace(imm.COD_RUOLO))
            //    {
            //        var role = dbCzn.RUOLO.FirstOrDefault(x => x.COD_RUOLO == imm.COD_RUOLO);
            //        anag.Ruolo = role.COD_RUOLO + " - " + CezanneHelper.GetDes(role.COD_RUOLO, role.DES_RUOLO).TitleCase();
            //    }

            //    anag.ShowPFI = imm.COD_TIPORLAV == "TI" || anag.HasPFI;
            //}
            if (anag.IsNeoMatr)
            {
                XR_IMM_IMMATRICOLAZIONI imm = dbCzn.XR_IMM_IMMATRICOLAZIONI
                                                    .Where(x => x.ID_PERSONA == anag.IdPersona && x.TIPO_OPERAZIONE != "A" && x.ESITO != null && x.ESITO.Value)
                                                    .OrderByDescending(x => x.TMS_TIMESTAMP)
                                                    .FirstOrDefault();
                if (imm != null)
                {
                    var tpcntr = dbCzn.TB_TPRLAV.FirstOrDefault(x => x.COD_TIPORLAV == imm.COD_TIPORLAV);
                    anag.TipoContratto = tpcntr.DES_TIPORLAV;
                }

            }

            if (hrisAbil == "HRCE")
                anag.ShowPFI = anag.ShowPFI && (!_checkAbil || AuthHelper.EnableToMatr(CommonHelper.GetCurrentUserMatricola(), anag.Matricola, "HRCE", "APPRENDISTATO").Enabled);
            else
                anag.ShowPFI = anag.ShowPFI && (!_checkAbil || AuthHelper.EnableToMatr(CommonHelper.GetCurrentUserMatricola(), anag.Matricola, "HRIS_GEST", "PFIGES").Enabled
                                                            || AuthHelper.EnableToMatr(CommonHelper.GetCurrentUserMatricola(), anag.Matricola, "HRIS_GEST", "PFIVIS").Enabled);

            LoadDatiApprendistato(anag, dbCzn);

            //Caricamento dati da DB
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Anagrafici, auth, () => CaricaDatiAnagrafici(anag, dbCzn, sint));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Residenza, auth, () => CaricaDatiResidenza(anag, dbCzn, sint));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Domicilio, auth, () => CaricaDatiDomicilio(anag, dbCzn, sint));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Recapiti, auth, () => CaricaDatiRecapiti(anag, dbCzn, sint));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.TitoliStudio, auth, () => CaricaDatiTitoliStudio(anag, dbCzn, sint));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Bancari, auth, () => CaricaDatiBancari(anag, sint));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.TipoContratti, auth, () => CaricaDatiTipoContratti(anag, dbCzn, sint, loader));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.StatoRapporto, auth, () => CaricaDatiStatoRapporto(anag, dbTal, new SintesiModel(sint)));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Curricula, auth, () => CaricaDatiCv(anag));

            LoadSez(anag, loader, _checkAbil, SezioniAnag.Familiari, auth, () => CaricaDatiFamiliari(anag, dbCzn, sint, loader));


            //Caricamento dati contrattuali
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Qualifiche, auth, () => CaricaDatiQualifiche(anag, dbCzn, sint, loader));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Ruoli, auth, () => CaricaDatiRuoli(anag, dbCzn, sint, loader));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Sedi, auth, () => CaricaDatiSedi(anag, dbCzn, sint, loader, auth));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Servizi, auth, () => CaricaDatiServizi(anag, dbCzn, sint, loader, auth));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Sezioni, auth, () => CaricaDatiSezioni(anag, dbCzn, sint, loader, auth));

            LoadSez(anag, loader, _checkAbil, SezioniAnag.MieiDoc, auth, () => CaricaMieiDocumenti(anag));

            //Caricamento dati da servizi
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Debitoria, auth, () => CaricaDatiSituazioneDebitoria(anag, sint));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Contenzioso, auth, () => CaricaDatiContenzioso(anag));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Cedolini, auth, () => CaricaDatiCedolini(anag, loader.GetFiltro<string>("CedoCodice")));

            if (CommonHelper.IsProduzione())
            {
                //Caricamento dati da HRDW
                LoadSez(anag, loader, _checkAbil, SezioniAnag.Retribuzione, auth, () => CaricaDatiReddito(anag, dbHRDW, sint));
                LoadSez(anag, loader, _checkAbil, SezioniAnag.Struttura, auth, () => CaricaAlberoStruttOrg(anag, dbTal, sint));
                LoadSez(anag, loader, _checkAbil, SezioniAnag.Presenze, auth, () => CaricaDatiPresenze(anag, dbHRDW, sint, loader.Anno));

                //Caricamento dati DB CV
                LoadSez(anag, loader, _checkAbil, SezioniAnag.Formazione, auth, () => CaricaDatiFormazione(anag, sint));
            }

            //PROCURE: Per il momento non vincoliamo l'informazione alle abilitazioni
            if (!anag.IsNeoMatr)
                CaricaDatiProcure(anag, sint);
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Trasferte, auth, () => CaricaDatiTrasferte(anag));
            LoadSez(anag, loader, _checkAbil, SezioniAnag.SpeseProduzione, auth, () => CaricaDatiSpeseProduzione(anag));
            if (fromAssunzioni)
            {
                var xx = dbCzn.XR_IMM_IMMATRICOLAZIONI.Where(x => x.ID_PERSONA == anag.IdPersona).FirstOrDefault();
                if (xx != null && !string.IsNullOrWhiteSpace(xx.JSON_ASSUNZIONI))
                {
                    var yy = AssunzioniVM.ConverToImmatricolazioneVM(xx);
                    anag.DataAssunzione = yy.DataInizio;
                }
            }
            LoadSez(anag, loader, _checkAbil, SezioniAnag.Documenti, auth, () => CaricaDocumentiPersonalieAmministrativi(anag));
        }

        private static void CaricaDatiFamiliari(AnagraficaModel anag, CezanneDb dbCzn, SINTESI1 sint, AnagraficaLoader loader)
        {
            anag.DatiFamiliari = new AnagraficaDatiFamiliari();
            anag.DatiFamiliari.IsEnabled = true;
            var db = new IncentiviEntities();
            //anag.DatiFamiliari.RecordsCensimento = db.XR_MAT_CENSIMENTO_CF_CONGEDI_EXTRA
            //    .Where(x => x.MATRICOLA == sint.COD_MATLIBROMAT)
            //    .OrderBy(x => x.GRADO_PARENTELA)
            //    .ToList();

            //anag.DatiFamiliari.DichiarazioneFB3000= 
            //    db.XR_MAT_CENSIMENTO_CF_ESITO_DICH
            //    .Where(x => x.MATRICOLA == sint.COD_MATLIBROMAT)
            //    .Select(x => x.RISPOSTA).FirstOrDefault();
        }

        private static void CaricaDatiProcure(AnagraficaModel anag, SINTESI1 sint)
        {
            //PROCURE: Per il momento non vincoliamo l'informazione alle abilitazioni
            anag.DatiProcure.IsEnabled = true;
            anag.DatiProcure.CanAdd = false;
            anag.DatiProcure.CanDelete = false;
            anag.DatiProcure.CanModify = false;

            var db = new HRGBEntities();
            var procure = db.Procure.Where(x => x.Procuratori.Matricola == anag.Matricola && x.StatoProcura.IdStatoProcura == 3);
            foreach (var item in procure)
            {
                ProcuraModel procura = new ProcuraModel()
                {
                    Codice = item.IdProcura,
                    Descrizione = item.TipoProcura.Descrizione,
                    CodStato = item.StatoProcura.IdStatoProcura,
                    DesStato = item.StatoProcura.Descrizione
                };
                CaricaIdentityData(procura, sint);
                anag.DatiProcure.Procure.Add(procura);
            }
        }

        private static void CaricaPartTime(AnagraficaModel anag, CezanneDb db)
        {
            var query = $@" select t0.[matricola_dp]   
                             ,t2.cod_part_time
                             ,t2.desc_part_time
                             FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0   
                             INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON(t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica])   
                            INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_PART_TIME] t2 ON (t1.[SKY_PART_TIME] = t2.[sky_part_time])
                             where 
                             t1.data_inizio_validita <=DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) and t1.data_fine_validita >=DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE()))
                            and matricola_dp='{anag.Matricola}'
                             ";

            anag.CodPartTime = "";
            anag.DesPartTime = "";

            var result = db.Database.SqlQuery<AnagraficaPartTime>(query);
            if (result != null && result.Any() && result.First().cod_part_time != "$")
            {
                anag.CodPartTime = result.First().cod_part_time;
                anag.DesPartTime = result.First().desc_part_time.TitleCase();
            }

        }

        private static void CaricaDatiRecapiti(AnagraficaModel anag, CezanneDb dbCzn, SINTESI1 sint)
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
                isAbil = AuthHelper.EnabledToSubFunc(matricola, "HRIS_PERS", "RECGES", out subFunc) || AuthHelper.EnabledToSubFunc(matricola, "HRIS_PERS", "RECVIS", out subFunc);
            anag.DatiRecapiti.CanAdd = subFunc.Create;
            anag.DatiRecapiti.CanDelete = subFunc.Delete;
            anag.DatiRecapiti.CanModify = subFunc.Update;

            ANAGPERS recAnag = dbCzn.ANAGPERS.Find(sint.ID_PERSONA);

            anag.DatiRecapiti.Telefono = recAnag.DES_TELREC;
            anag.DatiRecapiti.Cellulare = recAnag.DES_CELLULARE;
            anag.DatiRecapiti.Email = recAnag.DES_EMCEMAIL;
            anag.DatiRecapiti.Fax = recAnag.DES_EMCPHONE;

            CaricaIdentityData(anag.DatiRecapiti, sint);
        }

        private static void CaricaDatiCv(AnagraficaModel anag)
        {
            myRai.Data.CurriculumVitae.cv_ModelEntities cvEnt = new myRai.Data.CurriculumVitae.cv_ModelEntities();
            anag.DatiCurriculum.DatiCompletamento = CommonHelper.GetStatCV(anag.Matricola, cvEnt);
        }

        private static void CaricaDatiTE(AnagraficaModel anag)
        {
            DateTime ini = DateTime.Today.AddMonths(-1);
            DateTime fin = DateTime.Today;

            #region StipendioMensile
            //string queryStipendioMensile = $@"SELECT 
            //                                importi.minimo,
            //                                importi.aumenti_biennali,
            //                                importi.importo_merito_ult_conc,
            //                                importi.importo_aum_merito,
            //                                importi.importo_merito_gar_contr
            //                                FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] anag
            //                                join [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] te on anag.sky_anagrafica_unica=te.sky_matricola
            //                                join [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] tempo on te.sky_mese_contabile=tempo.sky_tempo
            //                                join [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE_IMPORTI] importi on te.sky_riga_te=importi.sky_riga_te
            //                                where matricola_dp='{anag.Matricola}'
            //                                and te.flg_ultimo_record='$'";
            string queryStipendioMensile = $@"SELECT [matricola]
                                                      ,[stipendio_mensile]
                                                      ,[importo_50] as minimo
                                                      ,[impo_anz_ante_1937]
                                                      ,[impo_aum_merito]
                                                      ,[impo_merito_ult_co]
                                                      ,[impo_merito_gar_co]
                                                      ,[impo_ad_pers_assor]
                                                      ,[impo_contingenza_conglobata]
                                                      ,[impo_aum_25_anni] as aum_25_anni
                                                      ,[impo_aumen_biennal] as aumen_biennali
                                                      ,[cd_aumen_biennali] as numero_aumen_biennali
                                                  FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TREC]
                                                  where matricola='{anag.Matricola}'
                                                  and ultimo_record_$='$'";
            #endregion

            #region IndennitaMensile
            //string queryIndennitaMensile = $@"SELECT 
            //                                    dIndennita.cod_indennita,
            //                                    dIndennita.desc_indennita,
            //                                    indennita.importo_inden,
            //                                    indennita.perc_inden
            //                                    FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] anag
            //                                    join [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] te on anag.sky_anagrafica_unica=te.sky_matricola
            //                                    join [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] tempo on te.sky_mese_contabile=tempo.sky_tempo
            //                                    join [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE_IMPORTI] importi on te.sky_riga_te=importi.sky_riga_te
            //                                    join [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_INDENNITA_TE] indennita on  te.sky_riga_te=indennita.sky_riga_te
            //                                    join [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_INDENNITA] dIndennita on indennita.sky_indennita=dIndennita.sky_indennita
            //                                    where matricola_dp='{anag.Matricola}'
            //                                    and te.flg_ultimo_record='$'";
            string queryIndennitaMensile = $@"SELECT indennita.[matricola]
                                                  ,min(indennita.[data_decorr_inden]) as DataInizio
	                                              ,max(indennita.[data_decorr_inden]) as DataFine
                                                  ,indennita.[codice_tipo_inden] as cod_indennita
	                                              ,dIndennita.[desc_tipo_inden] as desc_indennita
                                                  ,max(indennita.[importo_inden]) as importo_inden
                                              FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TREC_INDENNITA] indennita
                                              join [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_DEC_TREC_INDENNITA2] dIndennita on indennita.codice_tipo_inden=dIndennita.codice_tipo_inden
                                              where matricola='{anag.Matricola}'
                                              group by matricola, indennita.codice_tipo_inden, dIndennita.[desc_tipo_inden]
                                              having min(indennita.[data_decorr_inden])<='{fin.ToString("yyyy-MM-dd")}'
                                              and '{ini.ToString("yyyy-MM-dd")}'<=max(indennita.[data_decorr_inden])
                                              order by indennita.codice_tipo_inden";
            #endregion

            #region RetribAnnuale
            string queryRetribAnnuale = $@"SELECT [matricola]
                                          ,[premio_produzione]
                                          ,[indenn_saltuaria]
                                          ,[tot_retrib_annua]
                                          ,[XIII_mensilita] as xiii_mensilita
                                          ,[XIV_mensilita] as xiv_mensilita
                                      FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TREC]
                                      where matricola='{anag.Matricola}'
                                      and ultimo_record_$='$'";
            #endregion

            #region Variabili
            string queryVariabili = $@"SELECT t0.matricola_dp as Matricola,  
                                        CAST(t2.num_anno as int) as Anno, 
                                        t3.cod_aggregato_costi,  
                                        t3.desc_aggregato_costi,  
                                        t3.cod_voce_cedolino,  
                                        t3.desc_voce_cedolino,  
                                        sum(t1.quantita_ore) as Ore,  
                                        sum(t1.quantita_giorni) as Giorni,  
                                        sum(t1.quantita_numero_prestaz) as NumeroPrestazioni,  
                                        sum(t1.importo) as Importo  
                                        FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0  
                                        INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_CEDO_CASSA] t1 ON (t1.sky_matricola = t0.sky_anagrafica_unica)  
                                        INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t1.sky_data_competenza = t2.sky_tempo)  
                                        INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_VOCE_CEDOLINO] t3 ON (t1.sky_voce_cedolino = t3.sky_voce_cedolino)  
                                        WHERE (  
                                        t0.matricola_dp ='{anag.Matricola}'  
                                        AND t3.cod_aggregato_costi IN ('02', '03', '04')  
                                        and num_anno={DateTime.Now.Year}
                                        )  
                                        GROUP BY t0.matricola_dp,  
                                        t2.num_anno,  
                                        t3.cod_aggregato_costi,  
                                        t3.desc_aggregato_costi,  
                                        t3.cod_voce_cedolino,  
                                        t3.desc_voce_cedolino  
                                        order by t2.num_anno, t3.cod_aggregato_costi, t3.cod_voce_cedolino ";
            #endregion

            var db = new CezanneDb();
            anag.DatiRedditi.DatiTE.ImportiMensili = db.Database.SqlQuery<TEImportiMensili>(queryStipendioMensile).FirstOrDefault();
            anag.DatiRedditi.DatiTE.Indennita = db.Database.SqlQuery<TEIndennita>(queryIndennitaMensile).ToList();
            anag.DatiRedditi.DatiTE.Annualita = db.Database.SqlQuery<TEAnnualita>(queryRetribAnnuale).FirstOrDefault();
            anag.DatiRedditi.DatiTE.Variabili = db.Database.SqlQuery<MaggiorazModel>(queryVariabili).ToList();
        }

        private static void CaricaDatiCedolini(AnagraficaModel anag, string codice = "")
        {
            anag.DatiCedolini.CedoliniPossibili.AddRange(AmministrazioneManager.GetElencoBustePaga(anag.Matricola));
            if (anag.DatiCedolini.CedoliniPossibili.Any())
            {
                string codSearch = codice;
                if (String.IsNullOrWhiteSpace(codSearch))
                    codSearch = anag.DatiCedolini.CedoliniPossibili.Last().Codice.Substring(0, 4);

                foreach (var element in anag.DatiCedolini.CedoliniPossibili.Where(x => x.Codice.StartsWith(codSearch)))
                {
                    myRaiCommonModel.AmministrazioneModel.BustaPaga res = myRaiCommonManager.AmministrazioneManager.GetBustaPaga(anag.Matricola, element.Codice.Substring(0, 4), element.Codice.Substring(5, 4), element.Codice.Substring(4, 1));
                    anag.DatiCedolini.BustePaga.Add(res);
                }
            }
            CaricaIdentityData(anag.DatiCedolini, anag.IdPersona, anag.Matricola);

        }

        private static void CaricaIdentityData(_IdentityData data, SINTESI1 sint)
        {
            data.IdPersona = sint.ID_PERSONA;
            data.Matricola = sint.COD_MATLIBROMAT;
        }
        private static void CaricaIdentityData(_IdentityData data, int idPersona, string codMatr)
        {
            data.IdPersona = idPersona;
            data.Matricola = codMatr;
        }

        #region CaricamentoSezioni

        private static void CaricaMieiDocumenti(AnagraficaModel anag)
        {
            List<XR_DEM_DOCUMENTI_EXT> result = null;
            string matricola = anag.Matricola;
            try
            {
                result = DematerializzazioneManager.GetMieiDocumenti(matricola);
            }
            catch (Exception ex)
            {
                result = null;
            }
            anag.DematerializzazioneMieiDocumenti = new AnagraficaDematerializzazioneDocumenti();
            anag.DematerializzazioneMieiDocumenti.Documenti = new List<XR_DEM_DOCUMENTI_EXT>();
            anag.DematerializzazioneMieiDocumenti.Documenti = result;
            anag.DematerializzazioneMieiDocumenti.Matricola = matricola;
        }

        private static void CaricaDatiAnagrafici(AnagraficaModel anag, CezanneDb db, SINTESI1 sint, ANAGPERS recAnag = null, CITTAD cittad = null)
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
                isAbil = AuthHelper.EnabledToSubFunc(matricola, "HRIS_PERS", "ANAGES", out subFunc) || AuthHelper.EnabledToSubFunc(matricola, "HRIS_PERS", "ANAVIS", out subFunc);
            anag.DatiAnagrafici.CanAdd = !anag.IsNeoMatr && subFunc.Create;
            anag.DatiAnagrafici.CanDelete = !anag.IsNeoMatr && subFunc.Delete;
            anag.DatiAnagrafici.CanModify = !anag.IsNeoMatr && subFunc.Update;

            DateTime dateMax = GetDateLimitMax();

            if (recAnag == null)
                recAnag = db.ANAGPERS.Find(sint.ID_PERSONA);

            TB_CITTAD tbCitt = null;

            TB_COMUNE com = null;
            TB_STATOCV stciv = null;
            anag.DatiAnagrafici.Nome = recAnag.DES_NOMEPERS;
            anag.DatiAnagrafici.Cognome = recAnag.DES_COGNOMEPERS;
            anag.DatiAnagrafici.SecondoCognome = recAnag.DES_SECCOGNOME;
            anag.DatiAnagrafici.CognomeAcquisito = recAnag.DES_COGNOMEACQ;
            anag.DatiAnagrafici.CodiceFiscale = recAnag.CSF_CFSPERSONA;
            anag.DatiAnagrafici.DataNascita = recAnag.DTA_NASCITAPERS;
            anag.DatiAnagrafici.Sesso = recAnag.COD_SESSO;

            anag.DatiAnagrafici.CodStatoCivile = recAnag.COD_STCIV;
            stciv = db.TB_STATOCV.FirstOrDefault(x => x.COD_STCIV == recAnag.COD_STCIV);
            if (stciv != null)
                anag.DatiAnagrafici.StatoCivile = stciv.DES_STCIV;

            if (!String.IsNullOrWhiteSpace(recAnag.COD_CITTA))
            {
                anag.DatiAnagrafici.CodLuogoNascita = recAnag.COD_CITTA;
                com = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == sint.COD_CITTANASC);
                if (com != null && com.DES_PROV_STATE != "EST")
                    anag.DatiAnagrafici.LuogoNascita = String.Format("{1} ({0}), {2}, {3}", com.COD_CITTA.TitleCase(), com.DES_CITTA.TitleCase(), com.COD_PROV_STATE, com.TB_NAZIONE.DES_NAZIONE.TitleCase());
                else
                    anag.DatiAnagrafici.LuogoNascita = com.COD_CITTA.TitleCase();
            }

            if (cittad == null)
                cittad = db.CITTAD.FirstOrDefault(x => x.ID_PERSONA == anag.IdPersona && x.DTA_INIZIO <= DateTime.Now && x.DTA_FINE > DateTime.Now && x.IND_CITTADPRIM == "Y");

            if (cittad == null)
                cittad = db.CITTAD.Where(x => x.IND_CITTADPRIM == "Y").OrderByDescending(x => x.DTA_FINE).FirstOrDefault();

            if (cittad != null)
            {
                anag.DatiAnagrafici.CodCittadinanza = cittad.COD_CITTADPERS;
                tbCitt = db.TB_CITTAD.FirstOrDefault(x => x.COD_CITTAD == cittad.COD_CITTADPERS);
                if (tbCitt != null)
                    anag.DatiAnagrafici.Cittadinanza = tbCitt.DES_CITTAD;
            }

            CaricaIdentityData(anag.DatiAnagrafici, sint);

            return;
        }

        private static void CaricaDatiResidenza(AnagraficaModel anag, CezanneDb db, SINTESI1 sint)
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
                isAbil = AuthHelper.EnabledToSubFunc(matricola, "HRIS_PERS", "RESGES", out subFunc) || AuthHelper.EnabledToSubFunc(matricola, "HRIS_PERS", "RESVIS", out subFunc);
            anag.DatiResidenzaDomicilio.Residenza.CanAdd = subFunc.Create;
            anag.DatiResidenzaDomicilio.Residenza.CanDelete = subFunc.Delete;
            anag.DatiResidenzaDomicilio.Residenza.CanModify = subFunc.Update;

            DateTime dataMax = GetDateLimitMax();

            var recRes = db.RESIDENZA.FirstOrDefault(x => x.ID_PERSONA == sint.ID_PERSONA && x.DTA_FINE == dataMax);
            if (recRes != null)
            {
                TB_COMUNE com = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == recRes.COD_CITTA);

                if (!String.IsNullOrWhiteSpace(recRes.DES_INDIRRESID))
                {
                    anag.DatiResidenzaDomicilio.Residenza.LoadFromDB(recRes);

                    //anag.DatiResidenzaDomicilio.Residenza.Decorrenza = recRes.DTA_INIZIO;
                    //anag.DatiResidenzaDomicilio.Residenza.CodCitta = recRes.COD_CITTA;
                    //anag.DatiResidenzaDomicilio.Residenza.Indirizzo = recRes.DES_INDIRRESID.TitleCase();
                    //anag.DatiResidenzaDomicilio.Residenza.Citta = String.Format("{0}, {1}", com.DES_CITTA.TitleCase(), com.COD_PROV_STATE);
                    //anag.DatiResidenzaDomicilio.Residenza.CAP = recRes.CAP_CAPRESID;
                    //anag.DatiResidenzaDomicilio.Residenza.CodStato = com.COD_SIGLANAZIONE;
                    //anag.DatiResidenzaDomicilio.Residenza.Stato = com.TB_NAZIONE.DES_NAZIONE;
                }
                anag.DatiResidenzaDomicilio.Residenza.Tipologia = IndirizzoType.Residenza;

            }
            else
            {
                anag.DatiResidenzaDomicilio.Residenza.IsNew = true;
                anag.DatiResidenzaDomicilio.Residenza.CanModify = false;
                anag.DatiResidenzaDomicilio.Residenza.CanDelete = false;
            }

            CaricaIdentityData(anag.DatiResidenzaDomicilio.Residenza, sint);
            CaricaIdentityData(anag.DatiResidenzaDomicilio, sint);

            return;
        }
        private static void CaricaDatiDomicilio(AnagraficaModel anag, CezanneDb db, SINTESI1 sint)
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
                isAbil = AuthHelper.EnabledToSubFunc(matricola, "HRIS_PERS", "DOMGES", out subFunc) || AuthHelper.EnabledToSubFunc(matricola, "HRIS_PERS", "DOMVIS", out subFunc);
            anag.DatiResidenzaDomicilio.Domicilio.CanAdd = subFunc.Create;
            anag.DatiResidenzaDomicilio.Domicilio.CanDelete = subFunc.Delete;
            anag.DatiResidenzaDomicilio.Domicilio.CanModify = subFunc.Update;

            //anag.DatiResidenzaDomicilio.Domicilio.Decorrenza = sint.DTA_INIZIO_RES.GetValueOrDefault();
            anag.DatiResidenzaDomicilio.Domicilio.Tipologia = IndirizzoType.Domicilio;

            var recAnag = db.ANAGPERS.Find(sint.ID_PERSONA);

            if (!String.IsNullOrWhiteSpace(recAnag.DES_INDIRDOM))
            {
                anag.DatiResidenzaDomicilio.Domicilio.Indirizzo = recAnag.DES_INDIRDOM.TitleCase();
                TB_COMUNE com = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == recAnag.COD_CITTADOM);
                anag.DatiResidenzaDomicilio.Domicilio.CodCitta = recAnag.COD_CITTADOM;
                anag.DatiResidenzaDomicilio.Domicilio.Citta = String.Format("{0}, {1}", com.DES_CITTA.TitleCase(), com.COD_PROV_STATE);
                anag.DatiResidenzaDomicilio.Domicilio.CAP = recAnag.CAP_CAPDOM;
                anag.DatiResidenzaDomicilio.Domicilio.CodStato = com.COD_SIGLANAZIONE;
                anag.DatiResidenzaDomicilio.Domicilio.Stato = com.TB_NAZIONE.DES_NAZIONE;
            }
            else
            {
                anag.DatiResidenzaDomicilio.Domicilio.IsNew = true;
                anag.DatiResidenzaDomicilio.Domicilio.CanDelete = false;
                anag.DatiResidenzaDomicilio.Domicilio.CanModify = false;
            }

            CaricaIdentityData(anag.DatiResidenzaDomicilio.Domicilio, sint);
            CaricaIdentityData(anag.DatiResidenzaDomicilio, sint);

            return;
        }


        //private static void CaricaDatiTitoliStudio(AnagraficaModel anag, cv_ModelEntities db)
        //{
        //    var istr = db.TCVIstruzione.Where(x => x.Matricola == anag.Matricola);
        //    var titoli = db.DTitolo.Where(x => istr.Any(y => y.CodTitolo == x.CodTitolo));
        //    var tipoTitoli = db.DTipoTitolo.Where(x => titoli.Any(y => y.CodTipoTitolo == x.CodTipoTitolo));

        //    foreach (var item in istr)
        //    {
        //        StudioModel studio = new StudioModel();
        //        studio.IsSpecializzazione = false;

        //        var titolo = titoli.FirstOrDefault(x => x.CodTitolo == item.CodTitolo);
        //        if (titolo != null)
        //        {
        //            studio.CodTitolo = titolo.CodTitolo;
        //            studio.DesTitolo = titolo.DescTitolo;

        //            var tipoTitolo = tipoTitoli.FirstOrDefault(x => x.CodTipoTitolo == titolo.CodTipoTitolo);
        //            if (tipoTitolo != null)
        //            {
        //                studio.CodTipoTitolo = tipoTitolo.CodTipoTitolo;
        //                studio.DesTipoTitolo = tipoTitolo.DescTipoTitolo;
        //            }
        //        }

        //        studio.Istituto = item.Istituto + (!String.IsNullOrWhiteSpace(item.LocalitaStudi) ? ", " + item.LocalitaStudi : "");
        //        studio.DataInizio.Set(item.AnnoInizio, "", 4);
        //        studio.DataFine.Set(item.AnnoFine, "", 4);
        //        studio.Scala = item.Scala;
        //        studio.Voto = item.Voto;
        //        studio.Lode = item.Lode == "S";
        //        studio.TitoloTesi = item.TitoloTesi;

        //        anag.DatiTitoliStudio.Studi.Add(studio);
        //    }

        //    var spec = db.TCVSpecializz.Where(x => x.Matricola == anag.Matricola);
        //    var tipiSpec = db.DTipoSpecializz.Where(x => spec.Any(y => y.TipoSpecial == x.CodTipoSpecial));


        //    foreach (var item in spec)
        //    {
        //        StudioModel studio = new StudioModel();
        //        studio.IsSpecializzazione = true;

        //        studio.CodTipoTitolo = "MA";
        //        var tipoSpec = tipiSpec.FirstOrDefault(x => x.CodTipoSpecial == item.TipoSpecial);
        //        if (tipoSpec != null)
        //            studio.DesTipoTitolo = tipoSpec.DescTipoSpecial;

        //        studio.DesTitolo = item.Titolo;

        //        studio.Istituto = item.Istituto;
        //        studio.DataInizio.Set(item.DataInizio, "", 4, 2, 2, true);
        //        studio.DataFine.Set(item.DataFine, "", 4, 2, 2, true);
        //        studio.Scala = item.Scala;
        //        studio.Voto = item.Voto;
        //        studio.Lode = item.Lode == "S";

        //        anag.DatiTitoliStudio.Studi.Add(studio);
        //    }
        //}

        private static void CaricaDatiTipoContratti(AnagraficaModel anag, CezanneDb db, SINTESI1 sint, AnagraficaLoader loader)
        {
            var contratti = db.ASSTPCONTR.Where(x => x.ID_PERSONA == sint.ID_PERSONA);
            if (loader.CarrieraInizio.HasValue)
                contratti = contratti.Where(x => x.DTA_INIZIO <= loader.CarrieraFine.Value && loader.CarrieraInizio.Value <= x.DTA_FINE);

            var tpCntr = db.TB_TPCNTR.Where(x => contratti.Any(y => y.COD_TPCNTR == x.COD_TPCNTR)).ToList();
            var evTpCntr = db.TB_EVCONTR.Where(x => contratti.Any(y => y.COD_TPCNTR == x.COD_EVCONTR)).ToList();

            if (contratti.Any())
            {
                foreach (var item in contratti.OrderByDescending(x => x.DTA_INIZIO))
                {
                    EventoModel model = new EventoModel()
                    {
                        IdEvento = item.ID_ASSTPCONTR,
                        Codice = item.COD_TPCNTR,
                        CodiceEvento = item.COD_EVCNTR,
                        DataInizio = item.DTA_INIZIO,
                        DataFine = item.DTA_FINE,
                        Tipo = TipoEvento.Contratto
                    };

                    var tmpTpCntr = tpCntr.FirstOrDefault(x => x.COD_TPCNTR == model.Codice);
                    if (tmpTpCntr != null)
                        model.Descrizione = tmpTpCntr.DES_TPCNTR;

                    var tmpEvTpCntr = evTpCntr.FirstOrDefault(x => x.COD_EVCONTR == model.CodiceEvento);
                    if (tmpEvTpCntr != null)
                        model.DescrizioneEvento = tmpEvTpCntr.DES_EVCONTR;

                    CaricaIdentityData(model, sint);

                    anag.DatiContratti.Eventi.Add(model);
                }
            }
            else if (anag.IsNeoMatr)
            {
                XR_IMM_IMMATRICOLAZIONI imm = db.XR_IMM_IMMATRICOLAZIONI.FirstOrDefault(x => x.ID_PERSONA == anag.IdPersona);

                EventoModel model = new EventoModel()
                {
                    IdEvento = 0,
                    Codice = imm.COD_TIPORLAV == "TI" ? "9" : imm.COD_TIPORLAV == "TD" ? "1" : "14",
                    CodiceEvento = "ND",
                    DataInizio = imm.DTA_INIZIO,
                    DataFine = imm.DTA_FINE,
                    Tipo = TipoEvento.Contratto
                };

                var tmpTpCntr = tpCntr.FirstOrDefault(x => x.COD_TPCNTR == model.Codice);
                if (tmpTpCntr != null)
                    model.Descrizione = tmpTpCntr.DES_TPCNTR;

                var tmpEvTpCntr = evTpCntr.FirstOrDefault(x => x.COD_EVCONTR == model.CodiceEvento);
                if (tmpEvTpCntr != null)
                    model.DescrizioneEvento = tmpEvTpCntr.DES_EVCONTR;

                CaricaIdentityData(model, sint);

                anag.DatiContratti.Eventi.Add(model);

            }

            string hrisAbil = AuthHelper.HrisFuncAbil(CommonHelper.GetCurrentUserMatricola());
            if (String.IsNullOrWhiteSpace(hrisAbil))
                hrisAbil = "HRCE";

            string abilFunc = "HRCE";
            if (hrisAbil != "HRCE")
                abilFunc = "HRIS_PERS";

            AbilMatrLiv auth = null;
            auth = AuthHelper.EnableToMatr(CommonHelper.GetCurrentUserMatricola(), sint.COD_MATLIBROMAT, abilFunc);
            if (!auth.Enabled)
            {
                anag.CodErrorMsg = "401";
                anag.ErrorMsg = "Non abilitato";
                return;
            }

            anag.DatiContratti.HideStorico = HideStoricoContratti(SezioniAnag.TipoContratti, auth);
            CaricaIdentityData(anag.DatiContratti, sint);
        }

        private static void CaricaDatiQualifiche(AnagraficaModel anag, CezanneDb db, SINTESI1 sint, AnagraficaLoader loader)
        {
            var record = db.ASSQUAL.Where(x => x.ID_PERSONA == sint.ID_PERSONA);
            if (loader.CarrieraInizio.HasValue)
                record = record.Where(x => x.DTA_INIZIO <= loader.CarrieraFine.Value && loader.CarrieraInizio.Value <= x.DTA_FINE);
            var qualifiche = db.QUALIFICA.Where(x => record.Any(y => y.COD_QUALIFICA == x.COD_QUALIFICA)).ToList();
            var evqual = db.TB_EVQUAL.Where(x => record.Any(y => y.COD_EVQUAL == x.COD_EVQUAL)).ToList();

            foreach (var item in record.OrderByDescending(x => x.DTA_INIZIO))
            {
                EventoModel model = new EventoModel()
                {
                    IdEvento = item.ID_ASSQUAL,
                    Codice = item.COD_QUALIFICA,
                    CodiceEvento = item.COD_EVQUAL,
                    DataInizio = item.DTA_INIZIO,
                    DataFine = item.DTA_FINE,
                    Tipo = TipoEvento.Qualifica
                };

                var recDes = qualifiche.FirstOrDefault(x => x.COD_QUALIFICA == model.Codice);
                if (recDes != null)
                    model.Descrizione = recDes.DES_QUALIFICA;

                var recDesEv = evqual.FirstOrDefault(x => x.COD_EVQUAL == model.CodiceEvento);
                if (recDesEv != null)
                    model.DescrizioneEvento = recDesEv.DES_EVQUAL;

                CaricaIdentityData(model, sint);

                anag.DatiQualifiche.Eventi.Add(model);
            }

            CaricaIdentityData(anag.DatiQualifiche, sint);
        }
        private static void CaricaDatiRuoli(AnagraficaModel anag, CezanneDb db, SINTESI1 sint, AnagraficaLoader loader)
        {
            var record = db.JOBASSIGN.Where(x => x.ID_PERSONA == sint.ID_PERSONA);//.OrderByDescending(x => x.DTA_INIZIO);
            if (loader.CarrieraInizio.HasValue)
                record = record.Where(x => x.DTA_INIZIO <= loader.CarrieraFine.Value && loader.CarrieraInizio.Value <= x.DTA_FINE);
            var descrizioni = db.RUOLO.Where(x => record.Any(y => y.COD_RUOLO == x.COD_RUOLO)).ToList();
            var evDescrizioni = db.TB_EVJOB.Where(x => record.Any(y => y.COD_EVJOB == x.COD_EVJOB)).ToList();

            foreach (var item in record.OrderByDescending(x => x.DTA_INIZIO))
            {
                EventoModel model = new EventoModel()
                {
                    IdEvento = item.ID_JOBASSIGN,
                    Codice = item.COD_RUOLO,
                    CodiceEvento = item.COD_EVJOB,
                    DataInizio = item.DTA_INIZIO,
                    DataFine = item.DTA_FINE,
                    Tipo = TipoEvento.Mansione
                };

                var recDes = descrizioni.FirstOrDefault(x => x.COD_RUOLO == model.Codice);
                if (recDes != null)
                    model.Descrizione = recDes.DES_RUOLO;

                var recDesEv = evDescrizioni.FirstOrDefault(x => x.COD_EVJOB == model.CodiceEvento);
                if (recDesEv != null)
                    model.DescrizioneEvento = recDesEv.DES_EVJOB;

                model.Principale = item.IND_INCPRINC == "Y";

                CaricaIdentityData(model, sint);

                anag.DatiRuoli.Eventi.Add(model);
            }

            CaricaIdentityData(anag.DatiRuoli, sint);
        }
        private static void CaricaDatiSedi(AnagraficaModel anag, CezanneDb db, SINTESI1 sint, AnagraficaLoader loader, AbilMatrLiv auth)
        {
            string matricola = CommonHelper.GetCurrentUserMatricola();
            AbilSubFunc subFunc = null;
            bool isAbil = AuthHelper.EnabledToSubFunc(matricola, "HRCE", "AGG_INQUADRAMENTO", out subFunc);

            anag.DatiSedi.CanModify = auth != null && auth.LivelloGestionale && isAbil && subFunc.Update;

            //Controllo presenza richiesta
            var reqAttive = WorkflowHelper.NotIsCurrentState((int)VarContrStati.RichiestaCancellata, (int)VarContrStati.RichiestaEvasa);
            string codTipo = TipoEvento.Sede.ToString();
            var req = db.XR_WKF_RICHIESTE.Where(x => x.ID_PERSONA == sint.ID_PERSONA && x.ID_TIPOLOGIA == WKF_VAR_CONTR && x.COD_TIPO == codTipo).FirstOrDefault(reqAttive);
            if (req != null)
            {
                anag.DatiSedi.CanModify = false;
                anag.DatiSedi.IdRichiesta = req.ID_GESTIONE;
            }

            var record = db.TRASF_SEDE.Where(x => x.ID_PERSONA == sint.ID_PERSONA);//.OrderByDescending(x => x.DTA_INIZIO);
            if (loader.CarrieraInizio.HasValue)
                record = record.Where(x => x.DTA_INIZIO <= loader.CarrieraFine.Value && loader.CarrieraInizio.Value <= x.DTA_FINE);
            var descrizioni = db.SEDE.Where(x => record.Any(y => y.COD_SEDE == x.COD_SEDE)).ToList();
            var evDescrizioni = db.TB_EVTRASF.Where(x => record.Any(y => y.COD_EVTRASF == x.COD_EVTRASF)).ToList();

            foreach (var item in record.OrderByDescending(x => x.DTA_INIZIO))
            {
                EventoModel model = new EventoModel()
                {
                    IdEvento = item.ID_TRASF_SEDE,
                    Codice = item.COD_SEDE,
                    CodiceEvento = item.COD_EVTRASF,
                    DataInizio = item.DTA_INIZIO,
                    DataFine = item.DTA_FINE,
                    Tipo = TipoEvento.Sede
                };

                var recDes = descrizioni.FirstOrDefault(x => x.COD_SEDE == model.Codice);
                if (recDes != null)
                    model.Descrizione = recDes.DES_SEDE;

                var recDesEv = evDescrizioni.FirstOrDefault(x => x.COD_EVTRASF == model.CodiceEvento);
                if (recDesEv != null)
                    model.DescrizioneEvento = recDesEv.DES_EVTRASF;

                CaricaIdentityData(model, sint);

                anag.DatiSedi.Eventi.Add(model);
            }

            CaricaIdentityData(anag.DatiSedi, sint);
        }
        private static void CaricaDatiServizi(AnagraficaModel anag, CezanneDb db, SINTESI1 sint, AnagraficaLoader loader, AbilMatrLiv auth)
        {
            string matricola = CommonHelper.GetCurrentUserMatricola();
            AbilSubFunc subFunc = null;
            bool isAbil = AuthHelper.EnabledToSubFunc(matricola, "HRCE", "AGG_INQUADRAMENTO", out subFunc);

            anag.DatiServizi.CanModify = auth != null && auth.LivelloGestionale && isAbil && subFunc.Update;

            var reqAttive = WorkflowHelper.NotIsCurrentState((int)VarContrStati.RichiestaCancellata, (int)VarContrStati.RichiestaEvasa);
            string codTipo = TipoEvento.Servizio.ToString();
            var req = db.XR_WKF_RICHIESTE.Where(x => x.ID_PERSONA == sint.ID_PERSONA && x.ID_TIPOLOGIA == WKF_VAR_CONTR && x.COD_TIPO == codTipo).FirstOrDefault(reqAttive);
            if (req != null)
            {
                anag.DatiServizi.CanModify = false;
                anag.DatiServizi.IdRichiesta = req.ID_GESTIONE;
            }

            var record = db.XR_SERVIZIO.Where(x => x.ID_PERSONA == sint.ID_PERSONA);//.OrderByDescending(x => x.DTA_INIZIO);
            if (loader.CarrieraInizio.HasValue)
                record = record.Where(x => x.DTA_INIZIO <= loader.CarrieraFine.Value && loader.CarrieraInizio.Value <= x.DTA_FINE);
            var descrizioni = db.XR_TB_SERVIZIO.Where(x => record.Any(y => y.COD_SERVIZIO == x.COD_SERVIZIO)).ToList();
            var evDescrizioni = db.XR_TB_EVSERVIZIO.Where(x => record.Any(y => y.COD_EVSERVIZIO == x.COD_EVSERVIZIO)).ToList();

            foreach (var item in record.OrderByDescending(x => x.DTA_INIZIO))
            {
                EventoModel model = new EventoModel()
                {
                    IdEvento = item.ID_XR_SERVIZIO,
                    Codice = item.COD_SERVIZIO,
                    CodiceEvento = item.COD_EVSERVIZIO,
                    DataInizio = item.DTA_INIZIO,
                    DataFine = item.DTA_FINE,
                    Tipo = TipoEvento.Servizio
                };

                var recDes = descrizioni.FirstOrDefault(x => x.COD_SERVIZIO == model.Codice);
                if (recDes != null)
                    model.Descrizione = recDes.DES_SERVIZIO;

                var recDesEv = evDescrizioni.FirstOrDefault(x => x.COD_EVSERVIZIO == model.CodiceEvento);
                if (recDesEv != null)
                    model.DescrizioneEvento = recDesEv.DES_EVSERVIZIO;

                CaricaIdentityData(model, sint);

                anag.DatiServizi.Eventi.Add(model);
            }

            CaricaIdentityData(anag.DatiServizi, sint);
        }
        private static void CaricaDatiSezioni(AnagraficaModel anag, CezanneDb db, SINTESI1 sint, AnagraficaLoader loader, AbilMatrLiv auth)
        {
            string matricola = CommonHelper.GetCurrentUserMatricola();
            AbilSubFunc subFunc = null;
            bool isAbil = AuthHelper.EnabledToSubFunc(matricola, "HRCE", "AGG_INQUADRAMENTO", out subFunc);

            anag.DatiSezioni.CanModify = auth != null && auth.LivelloGestionale && isAbil && subFunc.Update;

            var reqAttive = WorkflowHelper.NotIsCurrentState((int)VarContrStati.RichiestaCancellata, (int)VarContrStati.RichiestaEvasa);
            string codTipo = TipoEvento.Servizio.ToString();
            string codTIpoServ = TipoEvento.Servizio.ToString(); //Nel caso sia stato cambiato il servizio, varia anche la sezione
            var req = db.XR_WKF_RICHIESTE.Where(x => x.ID_PERSONA == sint.ID_PERSONA && x.ID_TIPOLOGIA == WKF_VAR_CONTR && (x.COD_TIPO == codTipo || x.COD_TIPO == codTIpoServ)).FirstOrDefault(reqAttive);
            if (req != null)
            {
                anag.DatiSezioni.CanModify = false;
                anag.DatiSezioni.IdRichiesta = req.ID_GESTIONE;
            }

            var record = db.INCARLAV.Where(x => x.ID_PERSONA == sint.ID_PERSONA);//.OrderByDescending(x => x.DTA_INIZIO);
            if (loader.CarrieraInizio.HasValue)
                record = record.Where(x => x.DTA_INIZIO <= loader.CarrieraFine.Value && loader.CarrieraInizio.Value <= x.DTA_FINE);
            var descrizioni = db.UNITAORG.Where(x => record.Any(y => y.ID_UNITAORG == x.ID_UNITAORG)).ToList();
            var evDescrizioni = db.TB_TPEVORG.Where(x => record.Any(y => y.COD_MOTIVOORGIN == x.COD_MOTIVOORG)).ToList();

            foreach (var item in record.OrderByDescending(x => x.DTA_INIZIO))
            {
                EventoModel model = new EventoModel()
                {
                    IdEvento = item.ID_INCARLAV,
                    Codice = item.UNITAORG.COD_UNITAORG,
                    CodiceEvento = item.COD_MOTIVOORGIN,
                    DataInizio = item.DTA_INIZIO,
                    DataFine = item.DTA_FINE,
                    Tipo = TipoEvento.Sezione
                };

                var recDes = descrizioni.FirstOrDefault(x => x.COD_UNITAORG == model.Codice);
                if (recDes != null)
                    model.Descrizione = recDes.DES_DENOMUNITAORG;

                var recDesEv = evDescrizioni.FirstOrDefault(x => x.COD_MOTIVOORG == model.CodiceEvento);
                if (recDesEv != null)
                    model.DescrizioneEvento = recDesEv.DES_MOTIVOORG;

                CaricaIdentityData(model, sint);

                anag.DatiSezioni.Eventi.Add(model);
            }

            CaricaIdentityData(anag.DatiSezioni, sint);
        }

        private static void CaricaAlberoStruttOrg(AnagraficaModel anag, TalentiaDB db, SINTESI1 sint)
        {
            string dateToday = DateTime.Today.ToString("yyyyMMdd");
            string matricola = anag.Matricola;

            string codice = sint.COD_UNITAORG;

            var tmpSezione = db.XR_STR_TSEZIONE.FirstOrDefault(x => x.codice_visibile == codice && string.Compare(x.data_inizio_validita, dateToday) <= 0 && string.Compare(dateToday, x.data_fine_validita) < 0);
            FindNode(db, anag.DatiStruttOrg.Sezioni, tmpSezione);

            var l = db.XR_STR_TINCARICO.Where(x => x.matricola == matricola && x.flag_resp == "1" && string.Compare(x.data_inizio_validita, dateToday) <= 0 && string.Compare(dateToday, x.data_fine_validita) < 0)
                        .Join(db.XR_STR_DINCARICO,
                                    x => x.cod_incarico,
                                    y => y.COD_INCARICO,
                                    (x, y) => new
                                    {
                                        incarico = x,
                                        dIncarico = y
                                    })
                        .Join(db.XR_STR_TSEZIONE.Where(x => string.Compare(x.data_inizio_validita, dateToday) <= 0 && string.Compare(dateToday, x.data_fine_validita) < 0),
                                    x => x.incarico.id_sezione,
                                    y => y.id,
                                    (x, y) => new
                                    {
                                        incarico = x.incarico,
                                        dIncarico = x.dIncarico,
                                        sezione = y
                                    });

            //var incarico = db.XR_STR_TINCARICO.Where(x => x.matricola == matricola && x.flag_resp == "1" && string.Compare(x.data_inizio_validita, dateToday) <= 0 && string.Compare(dateToday, x.data_fine_validita) < 0);
            anag.DatiStruttOrg.Incarichi.AddRange(l.Select(x => new IncaricoModel()
            {
                Matricola = x.incarico.matricola,
                Nominativo = x.incarico.nominativo,
                CodIncarico = x.dIncarico.COD_INCARICO,
                DesIncarico = x.dIncarico.DES_INCARICO,
                CodStruttura = x.sezione.codice_visibile,
                DesStruttura = x.sezione.descrizione_lunga
            }));

            CaricaIdentityData(anag.DatiStruttOrg, sint);
        }

        private static void FindNode(TalentiaDB db, List<SezioneModel> listSezioni, myRaiDataTalentia.XR_STR_TSEZIONE sezione)
        {
            string dateToday = DateTime.Today.ToString("yyyyMMdd");

            if (sezione != null)
            {
                var incarico = db.XR_STR_TINCARICO.Where(x => x.id_sezione == sezione.id && x.flag_resp == "1" && string.Compare(x.data_inizio_validita, dateToday) <= 0 && string.Compare(dateToday, x.data_fine_validita) < 0);

                listSezioni.Insert(0, new SezioneModel()
                {
                    Codice = sezione.codice_visibile,
                    Descrizione = sezione.descrizione_lunga,
                    Responsabili = incarico.Select(x => new IncaricoModel()
                    {
                        Matricola = x.matricola,
                        Nominativo = x.nominativo
                    }).ToList()
                });

                var nodo = db.XR_STR_TALBERO.FirstOrDefault(x => x.id == sezione.id && x.subordinato_a != x.id);
                if (nodo != null)
                    FindNode(db, listSezioni, db.XR_STR_TSEZIONE.FirstOrDefault(x => x.id == nodo.subordinato_a));
            }
        }

        private static void CaricaDatiReddito(AnagraficaModel anag, CezanneDb db, SINTESI1 sint)
        {
            try
            {
                string matricola = anag.Matricola;
                string queryRal = GetQueryRAL(matricola);
                string queryMagg = GetQueryMaggiorazioni(matricola);

                var listRal = db.Database.SqlQuery<RedditoModel>(queryRal).ToList();
                var listMagg = db.Database.SqlQuery<MaggiorazModel>(queryMagg).ToList();

                foreach (var item in listRal)
                {
                    //RedditoModel reddito = new RedditoModel()
                    //{
                    //    Anno = item.Anno,
                    //    Ral_media = item.Ral_media
                    //};
                    //CaricaIdentityData(reddito, sint);
                    CaricaIdentityData(item, sint);

                    foreach (var maggItem in listMagg.Where(x => x.Anno == item.Anno))
                    {
                        CaricaIdentityData(maggItem, sint);
                        item.Maggiorazioni.Add(maggItem);
                    }

                    anag.DatiRedditi.Redditi.Add(item);
                }
            }
            catch (Exception ex)
            {

            }

            CaricaDatiTE(anag);

            CaricaIdentityData(anag.DatiRedditi, sint);
        }
        private static string GetQueryRAL(string matricola)
        {
            string query =
                " SELECT t0.[matricola_dp] as Matricola, " +
                " 	CAST(t7.num_anno as int) as Anno, " +
                " 	avg(t8.[tot_retrib_annua]) as Ral_media " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t7 ON (t1.[SKY_MESE_CONTABILE] = t7.[sky_tempo]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE_IMPORTI] t8 ON (t8.[SKY_riga_te] = t1.[SKY_riga_te]) " +
                " WHERE t0.matricola_dp = '" + matricola + "' " +
                //" and t8.tot_retrib_annua is not null " +
                " GROUP BY t0.[matricola_dp], " +
                " 	t7.num_anno " +
                " ORDER BY t7.num_anno ";
            return query;
        }
        private static string GetQueryMaggiorazioni(string matricola)
        {
            string query =
                " SELECT t0.matricola_dp as Matricola, " +
                " 	CAST(t2.num_anno as int) as Anno, " +
                //"   t2.cod_mese as Mese, " +
                " 	t3.cod_aggregato_costi, " +
                " 	t3.desc_aggregato_costi, " +
                " 	t3.cod_voce_cedolino, " +
                " 	t3.desc_voce_cedolino, " +
                " 	sum(t1.quantita_ore) as Ore, " +
                " 	sum(t1.quantita_giorni) as Giorni, " +
                " 	sum(t1.quantita_numero_prestaz) as NumeroPrestazioni, " +
                " 	sum(t1.importo) as Importo " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_CEDO_CASSA] t1 ON (t1.sky_matricola = t0.sky_anagrafica_unica) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t1.sky_data_competenza = t2.sky_tempo) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_VOCE_CEDOLINO] t3 ON (t1.sky_voce_cedolino = t3.sky_voce_cedolino) " +
                " WHERE ( " +
                " 		t0.matricola_dp ='" + matricola + "' " +
                " 		AND t3.cod_aggregato_costi IN ('02', '03', '04') " +
                " 		) " +
                " GROUP BY t0.matricola_dp, " +
                " 	t2.num_anno, " +
                //" 	t2.cod_mese, " +
                " 	t3.cod_aggregato_costi, " +
                " 	t3.desc_aggregato_costi, " +
                " 	t3.cod_voce_cedolino, " +
                " 	t3.desc_voce_cedolino " +
                " order by t2.num_anno, t3.cod_aggregato_costi, t3.cod_voce_cedolino ";

            return query;
        }

        private static void CaricaDatiSituazioneDebitoria(AnagraficaModel anag, SINTESI1 sint)
        {
            string matricola = anag.Matricola;

            try
            {
                DateTime? data = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var resultData = SituazioneDebitoriaManager.GetSituazioneDebitoria(matricola);

                if (resultData != null && resultData.Any())
                {
                    foreach (var itm in resultData)
                    {
                        if ((itm.AnnoA == 0) || (itm.AnnoDa == 0))
                        {
                            anag.DatiSituazioneDebitoria.Dati.Add(new SitDebitModel()
                            {
                                Addebito = itm.Addebito,
                                Descrizione = itm.Descrizione,
                                ImportoRata = itm.ImportoRata,
                                ImportoRateResidue = itm.ImportoRateResidue,
                                MeseA = "",
                                MeseDa = "",
                                NumeroRate = itm.NumeroRate,
                                NumeroRateResidue = itm.NumeroRateResidue
                            });
                        }
                        else
                        {

                            DateTime? t1 = null;
                            DateTime? t2 = null;

                            if (itm.AnnoDa > 0 && itm.IntMeseDa > 0)
                            {
                                t1 = new DateTime(itm.AnnoDa, itm.IntMeseDa, 1);
                            }
                            else if (itm.AnnoDa > 0 && itm.IntMeseDa == 0)
                            {
                                t1 = DateTime.MinValue;
                                itm.MeseDa = itm.AnnoDa.ToString();
                            }

                            if (itm.AnnoA > 0 && itm.IntMeseA > 0)
                            {
                                t2 = new DateTime(itm.AnnoA, itm.IntMeseA, 1);
                            }
                            else if (itm.AnnoA > 0 && itm.IntMeseA == 0)
                            {
                                t2 = DateTime.MaxValue;
                            }

                            if (t1.HasValue &&
                                t2.HasValue &&
                                t1 <= data &&
                                data <= t2)
                            {
                                anag.DatiSituazioneDebitoria.Dati.Add(new SitDebitModel()
                                {
                                    Addebito = itm.Addebito,
                                    Descrizione = itm.Descrizione,
                                    ImportoRata = itm.ImportoRata,
                                    ImportoRateResidue = itm.ImportoRateResidue,
                                    MeseA = String.IsNullOrEmpty(itm.MeseA) ? String.Empty : itm.MeseA,
                                    MeseDa = String.IsNullOrEmpty(itm.MeseDa) ? String.Empty : itm.MeseDa,
                                    NumeroRate = itm.NumeroRate,
                                    NumeroRateResidue = itm.NumeroRateResidue
                                });
                            }
                        }
                    }
                }

                //return model;
            }
            catch (Exception ex)
            {
                //return PartialView("~/Views/Shared/TblError.cshtml", new HandleErrorInfo(ex, "SituazioneDebitoriaController", "LoadTableDebiti"));
            }

            CaricaIdentityData(anag.DatiSituazioneDebitoria, sint);
        }

        private static void CaricaDatiFormazione(AnagraficaModel anag, SINTESI1 sint)
        {
            try
            {
                string matricola = anag.Matricola;
                List<V_CVCorsiRai> result = new List<V_CVCorsiRai>();

                string statement = "exec [dbo].[Proc_CorsiDip] @pMatricola='" + matricola + "'";
                cv_ModelEntities db = new cv_ModelEntities();
                var tmp = db.Database.SqlQuery<tmpV_CVCorsiRai>(statement);
                if (tmp != null)
                    anag.DatiFormazione.CorsiFatti.AddRange(tmp.Select(x => new CorsoFormazione()
                    {
                        Codice = x.codice,
                        DataInizioDate = x.DataInizioDate,
                        DataInizio = x.DataInizio,
                        DataFine = x.DataFine,
                        TitoloCorso = x.TitoloCorso,
                        Durata = Convert.ToInt32(x.Durata),
                        Societa = x.Societa,
                        flagImage = x.flagImage
                    }));

            }
            catch (Exception ex)
            {

            }
            CaricaIdentityData(anag.DatiFormazione, sint);
        }

        private static void CaricaDatiPresenze(AnagraficaModel anag, CezanneDb db, SINTESI1 sint, int anno)
        {
            try
            {
                if (anno <= 0)
                    anno = DateTime.Today.Year;

                anag.DatiPresenze.Anno = anno;

                string matricola = anag.Matricola;
                string queryEccez = GetQueryEccezioni(matricola, anno);
                var listEcc = db.Database.SqlQuery<AnagEcc>(queryEccez).ToList();

                string queryGG = GetQueryInfoGiornate(matricola, anno);
                var listGG = db.Database.SqlQuery<AnagGiornata>(queryGG).ToList();

                foreach (var item in listEcc)
                {
                    anag.DatiPresenze.Eccezioni.Add(item);

                }

                foreach (var item in listGG)
                {
                    CaricaIdentityData(item, sint);
                    anag.DatiPresenze.Giornate.Add(item);
                }

            }
            catch (Exception ex)
            {


            }

            CaricaIdentityData(anag.DatiPresenze, sint);
        }

        private static void CaricaDatiContenzioso(AnagraficaModel anag)
        {
            AnagraficaContenzioso dati = anag.DatiContenzioso;
            try
            {
                string matricola = anag.Matricola;

                MyRaiService1Client service = new MyRaiService1Client();

                try
                {
                    ProvvedimentiCauseResponse response = service.GetProvvedimentiCause(CommonHelper.GetCurrentUserMatricola(), matricola);

                    if (response.Esito)
                    {




                        dati.CauseAperte = response.Caperte;
                        dati.ProvvedimentiAperti = response.PDaperti;
                        dati.CauseChiuse = response.Cchiuse;
                        dati.ProvvedimentiChiusi = response.PDchiusi;

                        dati.Cause = new List<myRaiCommonModel.Causa>();

                        if (response.Cause != null && response.Cause.Any())
                        {
                            int idx = 1;
                            foreach (var c in response.Cause)
                            {
                                dati.Cause.Add(new myRaiCommonModel.Causa()
                                {
                                    Id = idx,
                                    Data = c.Date,
                                    Stato = c.Stato,
                                    Anno = c.Date.GetValueOrDefault().Year,
                                    Descrizione = c.TipoMotivo
                                });
                                idx++;
                            }
                        }

                        if (response.Provvedimenti != null && response.Provvedimenti.Any())
                        {
                            int idx = 1;
                            foreach (var p in response.Provvedimenti)
                            {
                                dati.Provvedimenti.Add(new myRaiCommonModel.ProvvedimentoExt()
                                {
                                    Id = idx,
                                    Data = p.Date.GetValueOrDefault(),
                                    Descrizione = p.Testo1.Trim() + " " + p.Testo2.Trim(),
                                    Durata = p.Durata,
                                    Stato = p.Stato
                                });
                                idx++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    dati = null;
                    throw new Exception("Errore nel servizio GetProvvedimentiCause", ex);
                }

                try
                {
                    bool isHrExtraEnabled = AuthHelper.EnabledTo(CommonHelper.GetCurrentUserMatricola(), "HREXTRA");

                    Service s = new Service();
                    HrExtra_FascicoliMatricola hrgbResult = s.Get_HrExtra_FascicoliMatricola(matricola, null);

                    if (String.IsNullOrEmpty(hrgbResult.Descrizione_Errore))
                    {
                        var items = hrgbResult.DT_HrExtra_FascicoliMatricola.AsEnumerable().Select(dr => dr).ToList();

                        if (items != null && items.Any())
                        {
                            int idx = dati.Cause.Count() + 1;
                            foreach (var extra in items)
                            {
                                DateTime myData;

                                Boolean datavalida = DateTime.TryParseExact(extra.ItemArray[3].ToString(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out myData);

                                if (!datavalida)
                                    continue;

                                dati.Cause.Add(new myRaiCommonModel.Causa()
                                {
                                    Id = idx,
                                    Data = myData,
                                    Stato = extra.ItemArray[5].ToString(),
                                    Anno = myData.Year,
                                    Descrizione = isHrExtraEnabled ? extra.ItemArray[4].ToString() : "-"
                                });
                                idx++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    dati.IsEnabled = false;
                    throw new Exception("Errore nel servizio DT_HrExtra_FascicoliMatricola", ex);
                }
            }
            catch (Exception ex)
            {
                dati.IsEnabled = false;
            }

            if (dati != null && dati.Cause != null && dati.Cause.Any())
            {
                dati.Cause = dati.Cause.OrderBy(w => w.Data).ToList();
            }

            if (dati == null)
            {
                dati = new AnagraficaContenzioso();
            }
            anag.DatiContenzioso = dati;
        }

        private static void CaricaDatiTrasferte(AnagraficaModel anag, string codice = "")
        {

            anag.DatiTrasferte = GetTrasferte(anag.Matricola);
            anag.DatiTrasferte.Matricola = anag.Matricola;
            anag.DatiTrasferte.IsEnabled = true;

        }

        public static AnagraficaTrasferte GetTrasferte(string matricola, int page = 0, int size = 0, TrasferteMacroStato macroStato = TrasferteMacroStato.Aperte, string data = "")
        {
            try
            {
                DateTime data1 = new DateTime();
                DateTime data2 = new DateTime();

                if (data != "")
                {
                    DateTime.TryParseExact(data, "MMMM yyyy", null, System.Globalization.DateTimeStyles.None, out data1);
                    data2 = data1.AddMonths(1).AddDays(-1);
                }
                else
                {
                    data1 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 01);
                    data2 = DateTime.Today;
                }

                AnagraficaTrasferte model = new AnagraficaTrasferte();
                model.Matricola = matricola;
                var quanteTrasferte = int.Parse(CommonHelper.GetParametri<string>(EnumParametriSistema.GetLimitTrasferte).FirstOrDefault());

                //Trasferta t =TrasferteManager.GetTrasferteAnniPrecedenti(data1, data2, "", "", matricola, macroStato);
                var t = TrasferteManager.GetTrasferteAnag(matricola, data1, data2);

                // var t = TrasferteManager.GetTrasferte(matricola);

                if (t.Viaggi != null && t.Viaggi.Any())
                {
                    string tx = null;
                    string tconcl = null;

                    using (var db = new myRaiData.digiGappEntities())
                    {
                        var item = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("TrasferteStatiEsclusi")).FirstOrDefault();

                        if (item != null)
                        {
                            tx = item.Valore1;
                        }

                        item = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("TrasferteStatiConclusi")).FirstOrDefault();

                        if (item != null)
                        {
                            tconcl = item.Valore1;
                        }
                    }


                    var listViaggi = t.Viaggi./*Where(x=>x.DataFromDB.Year==dataq.Year && x.DataFromDB.Month==dataq.Month).*/OrderByDescending(tt => tt.DataFromDB).GroupBy(x => x.FoglioViaggio).Select(x => x.FirstOrDefault()).ToList();
                    switch (macroStato)
                    {
                        case TrasferteMacroStato.Aperte:

                            listViaggi.RemoveWhere(x => x.Stato != null && tconcl.Contains(x.Stato));
                            break;
                        case TrasferteMacroStato.Concluse:
                            listViaggi.RemoveWhere(x => x.Stato == null || !tconcl.Contains(x.Stato));
                            break;
                        default:
                            break;
                    }

                    /*  if (listViaggi.Skip(page * quanteTrasferte).Count() > quanteTrasferte)
                      {*/
                    listViaggi.Skip(size).Take(quanteTrasferte).ToList().ForEach(w =>
                    {
                        if (w.Stato != null)
                        {
                            if (!tx.Contains(w.Stato))
                            {
                                w.Rimborso = (double)TrasferteManager.CalcolaResiduoNetto(w.FoglioViaggio);
                            }
                        }
                    });
                    //    var arr = listViaggi.Skip(page * quanteTrasferte).Take(quanteTrasferte).ToArray();
                    var arr = listViaggi.Take(size + quanteTrasferte).ToArray();
                    //   var newArray = new MyRaiServiceInterface.MyRaiServiceReference1.Viaggio[quanteTrasferte];
                    //   Array.Copy(arr, newArray, arr.Length);
                    model.Trasferte = new TrasferteViewModel();
                    model.Trasferte.HasNext = arr.Length < listViaggi.GroupBy(x => x.FoglioViaggio).Count() ? true : false;

                    //                        model.Data = new Trasferta();
                    model.Trasferte.Data.Viaggi = arr;
                    model.Trasferte.Data.CompetenzaDefinizione = t.CompetenzaDefinizione;
                    model.Trasferte.Size = model.Trasferte.Data.Viaggi.Length;
                    /*   }
                       else
                       {
                           listViaggi.ToList().ForEach(w =>
                           {
                               if (w.Stato != null)
                               {
                                   if (!tx.Contains(w.Stato))
                                   {
                                       w.Rimborso = (double)TrasferteManager.CalcolaResiduoNetto(w.FoglioViaggio);
                                   }
                               }
                           });

                           var arr = listViaggi.ToArray();
                           var newArray = new MyRaiServiceInterface.MyRaiServiceReference1.Viaggio[quanteTrasferte];
                           Array.Copy(arr, newArray, arr.Length > size ? size : arr.Length);
                           model.Trasferte = new TrasferteViewModel();
                           model.Trasferte.HasNext = false;
                           model.Trasferte.Page = page;
                            model.Trasferte.Data.Viaggi = newArray;
                           model.Trasferte.Data.CompetenzaDefinizione = t.CompetenzaDefinizione;
                       }*/
                }
                else
                {
                    model.Trasferte.Data.CompetenzaDefinizione = t.CompetenzaDefinizione;
                }
                model.Trasferte.MacroStato = macroStato;
                model.Trasferte.Stati = TrasferteManager.GetStatiTrasferta();
                model.Trasferte.Page = page + 1;

                model.Matricola = matricola;
                return model;
            }
            catch (Exception e)
            {
                return null;
            }

        }
        public static DettaglioTrasfertaVM GetDettaglioTrasferta(string foglioViaggio)
        {
            try
            {
                DettaglioTrasfertaVM vm = new DettaglioTrasfertaVM();

                vm.FoglioViaggio = TrasferteManager.GetFoglioViaggio(foglioViaggio);
                vm.FViaggio = foglioViaggio;

                if (vm.FoglioViaggio != null)
                {
                    vm.StatoTrasferta = TrasferteManager.GetStatoTrasferta(vm.FoglioViaggio.STATO);
                    vm.GrandeEvento = TrasferteManager.GetGrandeEvento(vm.FoglioViaggio.COD_GRANDI_EVENTI);
                    vm.Itinerario = TrasferteManager.GetItinerario(foglioViaggio);
                    vm.NotaSpeseTrasferta = TrasferteManager.GetNotaSpeseTrasferta(foglioViaggio);
                    vm.Alberghi = TrasferteManager.GetAlberghi(foglioViaggio);
                    vm.BigliettiRai = TrasferteManager.GetBigliettoRai(foglioViaggio);
                    vm.Diaria = TrasferteManager.GetDiaria(foglioViaggio);
                    vm.ResiduoNetto = TrasferteManager.CalcolaResiduoNetto(foglioViaggio);
                }
                return vm;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static void CaricaDatiSpeseProduzione(AnagraficaModel anag, string codice = "")
        {

            anag.DatiSpeseProduzione = GetSpeseProduzione(anag.Matricola);
            anag.DatiSpeseProduzione.Matricola = anag.Matricola;
            anag.DatiSpeseProduzione.IsEnabled = true;

        }
        public static AnagraficaSpeseProduzione GetSpeseProduzione(string matricola, int page = 0, int size = 0, bool isAperte = true, string data = "")
        {
            DateTime dataq = DateTime.Today.Date;
            if (data != "")
            {
                DateTime.TryParseExact(data, "MMMM yyyy", null, System.Globalization.DateTimeStyles.None, out dataq);
            }
            SpeseDiProduzioneEntities dbContext = new SpeseDiProduzioneEntities();
            SpeseDiProduzioneServizio SpeseDiProduzioneServizio = new SpeseDiProduzioneServizio(dbContext);
            var quanteSpese = int.Parse(CommonHelper.GetParametri<string>(EnumParametriSistema.GetLimitElementForFoglioSpese).FirstOrDefault());

            var spese = SpeseDiProduzioneServizio.GetSpeseProduzione(isAperte, null, null, "", "", matricola).Where(x => x.MP_Data.ToDateTime("dd/MM/yyyy").Year == dataq.Year && x.MP_Data.ToDateTime("dd/MM/yyyy").Month == dataq.Month);
            AnagraficaSpeseProduzione model = new AnagraficaSpeseProduzione();
            model.SpeseProduzione = spese.Take(size + quanteSpese).ToList();
            model.Matricola = matricola;
            model.IsEnabled = true;
            model.statoSpese = (isAperte == true ? "aperte" : "concluse");
            model.HasNext = model.SpeseProduzione.Count() < spese.Count() ? true : false;
            model.Size = model.SpeseProduzione.Count();
            return model;
        }



        private static string GetQueryEccezioni(string matricola, int anno)
        {
            return " SELECT t0.matricola_dp as Matricola,  " +
                    " t2.cod_mese, " +
                    " CAST(t2.num_anno as int) as num_anno, " +
                    " CAST(t2.num_giorno as int) as num_giorno, " +
                    " CASE   " +
                    "     WHEN t3.cod_eccez_padre IS NULL  " +
                    "         THEN 'Varie'  " +
                    "     ELSE t3.cod_eccez_padre  " +
                    "     END AS cod_eccez_padre,  " +
                    " CASE   " +
                    "     WHEN t3.desc_cod_eccez_padre IS NULL  " +
                    "         THEN 'VARIE'  " +
                    "     ELSE t3.desc_cod_eccez_padre  " +
                    "     END AS desc_cod_eccez_padre,  " +
                    " t3.cod_eccezione,  " +
                    " t3.desc_eccezione,  " +
                    " CASE   " +
                    "     WHEN t3.unita_misura = 'G'  " +
                    "         THEN 'GG'  " +
                    "     WHEN t3.unita_misura = 'H'  " +
                    "         THEN 'Ore'  " +
                    "     WHEN t3.unita_misura = 'K'  " +
                    "         THEN 'Km'  " +
                    "     WHEN t3.unita_misura = 'N'  " +
                    "         THEN 'Q.tà'  " +
                    "     ELSE t3.unita_misura  " +
                    "     END AS unita_misura, " +
                    " t1.quantita_numero, " +
                    " t1.quantita_ore, " +
                    " t2.tipo_giorno  " +
                    " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0  " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_ECCEZIONI] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica)  " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t2.sky_tempo = t1.SKY_DATA)  " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ECCEZIONE] t3 ON (t3.sky_eccezione = t1.SKY_eccezione)  " +
                    " WHERE (  " +
                    "     t0.matricola_dp IN ('" + matricola + "')  " +
                    "     AND ( t2.num_anno = " + anno.ToString() + " ) " +
                    " 	) " +
                    " order by t2.cod_mese, t2.num_giorno ";
        }
        private static string GetQueryInfoGiornate(string matricola, int anno)
        {
            return
                " select t0.matricola_dp as Matricola " +
                " 		, CAST(t2.num_anno as int) as num_anno " +
                " 		, CAST(t2.cod_mese as int) as cod_mese " +
                " 		, CAST(t2.num_giorno as int) as num_giorno " +
                " 		, t3.ora_24 as ora_entrata " +
                " 		, t4.ora_24 as ora_uscita " +
                " 		, t1.sky_orario_previsto " +
                " 		, t1.sky_orario_reale " +
                " 		, t8.sintesi_giornata " +
                " 		, t8.sintesi2_giornata " +
                " 		, t8.sintesi3_giornata " +
                " 		, t9.ora_24 as ore_lavorate " +
                " 		, t10.ora_24 as ore_presenza " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0  " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_GIORNO_X_DIP] t1 on t0.sky_anagrafica_unica = t1.sky_matricola " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t2.sky_tempo = t1.SKY_DATA) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ORA] t3 on (t3.sky_ora = t1.sky_ora_entrata) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ORA] t4 on (t4.sky_ora = t1.sky_ora_uscita) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_STATO_GIORNATA] t8 on (t8.sky_stato_giornata=t1.sky_stato_giornata) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ORA] t9 on (t9.totale_minuti = t1.ore_lavorate) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ORA] t10 on (t10.totale_minuti = t1.ore_presenza) " +
                " where t0.matricola_dp='" + matricola + "' " +
                " and t2.num_anno=" + anno.ToString() + " " +
                " order by t2.num_anno, cod_mese, num_giorno ";
        }
        #endregion

        #region Salvataggio


        private static bool CheckAnagChanges(object entity, string oldProp, object newValue, object logEntity, string logProp)
        {
            object oldValue = entity.GetType().GetProperty(oldProp).GetValue(entity, null);

            if (oldValue == null && newValue == null)
                return false;

            bool isChanged = (oldValue == null && newValue != null) || (oldValue != null && newValue == null) || !oldValue.Equals(newValue);
            if (isChanged)
            {
                //oldValue = newValue;
                entity.GetType().GetProperty(oldProp).SetValue(entity, newValue, null);
                if (logEntity != null && !String.IsNullOrWhiteSpace(logProp))
                    logEntity.GetType().GetProperty(logProp).SetValue(logEntity, newValue, null);
            }

            return isChanged;
        }
        public static bool Save_Anagrafica(AnagraficaDatiAnag model, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            CMEVENTI evento = null;
            List<CMEVENTICURR> eventiCurr = new List<CMEVENTICURR>();
            CMINFOANAG infoAnag = new CMINFOANAG();

            var db = GetDb();
            ANAGPERS anag = db.ANAGPERS.Find(model.IdPersona);

            ANAGPERS origAnag = null;
            CITTAD origCittad = null;
            if (anag != null)
            {
                origAnag = anag.Copy();

                bool anagMod = false;
                anagMod = anagMod || CheckAnagChanges(anag, "CSF_CFSPERSONA", model.CodiceFiscale.ToUpper(), infoAnag, "CSF_CFSPERSONA");
                anagMod = anagMod || CheckAnagChanges(anag, "DES_NOMEPERS", model.Nome.ToUpper(), infoAnag, "DES_NOMEPERS");
                anagMod = anagMod || CheckAnagChanges(anag, "DES_COGNOMEPERS", model.Cognome.ToUpper(), infoAnag, "DES_COGNOMEPERS");
                anagMod = anagMod || CheckAnagChanges(anag, "DES_SECCOGNOME", !String.IsNullOrWhiteSpace(model.SecondoCognome) ? model.SecondoCognome.ToUpper() : null, null, null);
                anagMod = anagMod || CheckAnagChanges(anag, "DTA_NASCITAPERS", model.DataNascita, infoAnag, "DTA_NASCITAPERS");
                anagMod = anagMod || CheckAnagChanges(anag, "COD_CITTA", model.CodLuogoNascita, infoAnag, "COD_CITTANASC");
                anagMod = anagMod || CheckAnagChanges(anag, "COD_SESSO", model.Sesso, infoAnag, "COD_SESSO");
                anagMod = anagMod || CheckAnagChanges(anag, "COD_STCIV", model.CodStatoCivile, infoAnag, "COD_STCIV");
                anagMod = anagMod || CheckAnagChanges(anag, "DES_COGNOMEACQ", model.CognomeAcquisito, infoAnag, "DES_COGNOMEACQ");

                if (anagMod)
                {
                    eventiCurr.Add(new CMEVENTICURR()
                    {
                        ID_PERSONA = anag.ID_PERSONA,
                        COD_CURRICULUM = "ANAGPERS",
                        ID_CURRICULUM = anag.ID_PERSONA
                    });
                }

                CITTAD recCittad = db.CITTAD.FirstOrDefault(x => x.ID_PERSONA == model.IdPersona && x.DTA_INIZIO <= DateTime.Now && x.DTA_FINE > DateTime.Now);
                if (recCittad != null && recCittad.COD_CITTADPERS != model.CodCittadinanza)
                {
                    origCittad = recCittad.Copy();

                    recCittad.DTA_FINE = DateTime.Today.AddDays(-1);

                    CITTAD newRec = new CITTAD()
                    {
                        ID_PERSONA = model.IdPersona,
                        COD_CITTADPERS = model.CodCittadinanza,
                        IND_CITTADPRIM = "Y",
                        COD_CITIZENSHIP = null,
                        DTA_INIZIO = DateTime.Today,
                        DTA_FINE = GetDateLimitMax()
                    };

                    CezanneHelper.GetCampiFirma(out string cod_user, out string cod_termid, out DateTime tms_timestamp);
                    newRec.COD_USER = cod_user;
                    newRec.COD_TERMID = cod_termid;
                    newRec.TMS_TIMESTAMP = tms_timestamp;

                    infoAnag.COD_CITTAD = model.CodCittadinanza;

                    eventiCurr.Add(new CMEVENTICURR()
                    {
                        ID_PERSONA = anag.ID_PERSONA,
                        COD_CURRICULUM = "CITTAD",
                        ID_CURRICULUM = anag.ID_PERSONA
                    });

                    db.CITTAD.Add(newRec);
                }

                if (eventiCurr.Any())
                {
                    evento = new CMEVENTI()
                    {
                        ID_CMEVENTO = db.CMEVENTI.GeneraPrimaryKey(9),
                        COD_CMEVENTO = "RAI105",
                        DTA_INIZIO = DateTime.Today,
                        DTA_FRONTEND = DateTime.Today,
                        DTA_CEZANNE = DateTime.Today,
                        COD_CMSTATO = "I",
                        ID_GESTEVENTO = CommonHelper.GetCurrentIdPersona(),
                        IND_TRASMETTI = "Y"
                    };
                    foreach (var item in eventiCurr)
                        item.ID_CMEVENTO = evento.ID_CMEVENTO;
                    infoAnag.ID_CMEVENTO = evento.ID_CMEVENTO;

                    CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tmsTimestamp);
                    evento.COD_USER = codUser;
                    evento.COD_TERMID = codTermid;
                    evento.TMS_TIMESTAMP = tmsTimestamp;

                    infoAnag.COD_USER = codUser;
                    infoAnag.COD_TERMID = codTermid;
                    infoAnag.TMS_TIMESTAMP = tmsTimestamp;

                    db.CMEVENTI.Add(evento);
                    foreach (var item in eventiCurr)
                        db.CMEVENTICURR.Add(item);
                    db.CMINFOANAG.Add(infoAnag);
                }

                result = !anagMod || DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
                else if (anagMod)
                    Tracciato_Domicilio(model.Matricola, TipoAnaVar.Anagrafica, anag, recCittad, evento.ID_CMEVENTO, anag.TMS_TIMESTAMP);

                HrisHelper.LogOperazione("AggiornamentoAnagrafica", $"Aggiornamento anagrafica ID_PERS:{model.IdPersona} MATR:{model.Matricola}", result, errorMsg, null, null, model);
                if (recCittad != null && recCittad.COD_CITTADPERS != model.CodCittadinanza)
                    HrisHelper.LogOperazione("AggiornamentoCittadinanza", $"Aggiornamento cittadinanza ID_PERS:{model.IdPersona} MATR:{model.Matricola}", result, errorMsg, null, null, model);
            }
            else
            {
                errorMsg = "Persona non trovata";
            }

            return result;
        }

        public static bool Save_Recapiti(AnagraficaRecapiti model, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = GetDb();
            ANAGPERS anag = db.ANAGPERS.Find(model.IdPersona);
            if (anag != null)
            {
                bool anagMod = false;
                anag.DES_TELREC = model.Telefono;
                anag.DES_CELLULARE = model.Cellulare;
                anag.DES_EMCEMAIL = model.Email;
                anag.DES_EMCPHONE = model.Fax;

                CezanneHelper.GetCampiFirma(out string cod_user, out string cod_termid, out DateTime tms_timestamp);
                anag.COD_USER = cod_user;
                anag.COD_TERMID = cod_termid;
                anag.TMS_TIMESTAMP = tms_timestamp;

                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
            }
            else
            {
                errorMsg = "Persona non trovata";
            }

            HrisHelper.LogOperazione("AggiornamentoRecapiti", $"Aggiornamento recapiti ID_PERS:{model.IdPersona} MATR:{model.Matricola}", result, errorMsg, null, null, model);

            return result;
        }

        public static bool Save_DatiIndirizzo(CezanneDb db, IndirizzoModel model, out string errorMsg, out List<Func<bool>> postActions)
        {
            bool result = false;
            errorMsg = "";
            postActions = new List<Func<bool>>();

            DateTime dataMax = GetDateLimitMax();

            RESIDENZA oldRes = null;
            RESIDENZA newRes = null;
            JESIDENZA newJes = null;
            ANAGPERS anag = null;

            CMEVENTI evento = new CMEVENTI();
            List<CMEVENTICURR> eventiCurr = new List<CMEVENTICURR>();
            CMINFOANAG infoAnag = new CMINFOANAG();

            if (!String.IsNullOrWhiteSpace(model.Civico))
                model.Indirizzo += " " + model.Civico;

            bool saveChanges = false;
            if (db == null)
            {
                saveChanges = true;
                db = GetDb();
            }

            if (model.Tipologia == IndirizzoType.Residenza)
            {
                if (model.IsNew)
                {
                    if (!model.IgnoraUltimoRecord)
                    {
                        oldRes = db.RESIDENZA.FirstOrDefault(x => x.ID_PERSONA == model.IdPersona && x.DTA_FINE == dataMax);
                        if (oldRes != null)
                            oldRes.DTA_FINE = model.Decorrenza.AddDays(-1);
                    }

                    newRes = new RESIDENZA()
                    {
                        ID_PERSONA = model.IdPersona,
                        DTA_INIZIO = model.Decorrenza,
                        DTA_FINE = dataMax,
                        COD_CITTA = model.CodCitta,
                        DES_INDIRRESID = model.Indirizzo,
                        CAP_CAPRESID = model.CAP
                    };

                    newJes = new JESIDENZA()
                    {
                        ID_PERSONA = model.IdPersona,
                        DTA_INIZIO = model.Decorrenza,
                        DTA_CAMBIO_RES = model.G_CambioRes
                    };

                    evento.COD_CMEVENTO = "RAI012";
                }
                else
                {
                    oldRes = db.RESIDENZA.FirstOrDefault(x => x.ID_PERSONA == model.IdPersona && x.DTA_INIZIO == model.Decorrenza);
                    oldRes.COD_CITTA = model.CodCitta;
                    oldRes.DES_INDIRRESID = model.Indirizzo;
                    oldRes.CAP_CAPRESID = model.CAP;

                    evento.COD_CMEVENTO = "RAI010";
                }

                infoAnag.DES_INDIRRES = model.Indirizzo;
                infoAnag.CAP_CAPRES = model.CAP;
                infoAnag.COD_CITTARES = model.CodCitta;

                eventiCurr.Add(new CMEVENTICURR()
                {
                    ID_PERSONA = model.IdPersona,
                    COD_CURRICULUM = "RESIDENZA",
                    ID_CURRICULUM = model.IdPersona
                });
            }

            if (model.Tipologia == IndirizzoType.Domicilio || model.AssegnaDomilicio)
            {
                anag = db.ANAGPERS.Find(model.IdPersona);

                anag.COD_CITTADOM = model.CodCitta;
                anag.DES_INDIRDOM = model.Indirizzo;
                anag.CAP_CAPDOM = model.CAP;


                infoAnag.DES_INDIRDOM = model.Indirizzo;
                infoAnag.CAP_CAPDOM = model.CAP;
                infoAnag.COD_CITTADOM = model.CodCitta;

                if (!model.AssegnaDomilicio)
                {
                    evento.COD_CMEVENTO = "RAI013";
                    model.Decorrenza = DateTime.Today;
                }

                eventiCurr.Add(new CMEVENTICURR()
                {
                    ID_PERSONA = model.IdPersona,
                    COD_CURRICULUM = "ANAGPERS",
                    ID_CURRICULUM = model.IdPersona
                });
            }

            evento.ID_CMEVENTO = db.CMEVENTI.GeneraPrimaryKey(9);
            evento.DTA_INIZIO = model.Decorrenza;
            evento.DTA_FRONTEND = DateTime.Today;
            evento.DTA_CEZANNE = DateTime.Today;
            evento.COD_CMSTATO = "I";
            evento.ID_GESTEVENTO = CommonHelper.GetCurrentIdPersona();
            evento.IND_TRASMETTI = "Y";

            foreach (var item in eventiCurr)
                item.ID_CMEVENTO = evento.ID_CMEVENTO;
            infoAnag.ID_CMEVENTO = evento.ID_CMEVENTO;


            CezanneHelper.GetCampiFirma(out string cod_user, out string cod_termid, out DateTime tms_timestamp);
            if (oldRes != null)
            {
                if (!model.IsNew || oldRes.DTA_FINE > oldRes.DTA_INIZIO)
                {
                    oldRes.COD_USER = cod_user;
                    oldRes.COD_TERMID = cod_termid;
                    oldRes.TMS_TIMESTAMP = tms_timestamp;
                }
                else
                {
                    db.RESIDENZA.Remove(oldRes);
                }
            }
            if (newRes != null)
            {
                newRes.COD_USER = cod_user;
                newRes.COD_TERMID = cod_termid;
                newRes.TMS_TIMESTAMP = tms_timestamp;
                db.RESIDENZA.Add(newRes);
            }
            if (newJes != null)
            {
                db.JESIDENZA.Add(newJes);
            }
            if (anag != null)
            {
                anag.COD_USER = cod_user;
                anag.COD_TERMID = cod_termid;
                anag.TMS_TIMESTAMP = tms_timestamp;
            }

            evento.COD_USER = cod_user;
            evento.COD_TERMID = cod_termid;
            evento.TMS_TIMESTAMP = tms_timestamp;

            infoAnag.COD_USER = cod_user;
            infoAnag.COD_TERMID = cod_termid;
            infoAnag.TMS_TIMESTAMP = tms_timestamp;

            db.CMEVENTI.Add(evento);
            foreach (var item in eventiCurr)
                db.CMEVENTICURR.Add(item);
            db.CMINFOANAG.Add(infoAnag);


            result = !saveChanges || DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            if (!result)
                errorMsg = "Errore durante il salvataggio";
            else
            {
                if (saveChanges)
                {
                    bool residenza = true;
                    if (model.Tipologia == IndirizzoType.Residenza)
                        residenza = Tracciato_Residenza(evento.ID_CMEVENTO, evento.COD_CMEVENTO, model.IsNew ? newRes : oldRes);

                    bool domicilio = true;
                    if (model.Tipologia == IndirizzoType.Domicilio || (model.Tipologia == IndirizzoType.Residenza && model.AssegnaDomilicio))
                        domicilio = Tracciato_Domicilio(model.Matricola, TipoAnaVar.Domicilio, anag, null, evento.ID_CMEVENTO, model.Tipologia == IndirizzoType.Residenza || model.Decorrenza != DateTime.MinValue ? model.Decorrenza : DateTime.Today);

                    if (!residenza || !domicilio)
                        errorMsg = "ATTENZIONE: i dati sono stati salvati correttamente.\r\nTuttavia, si è verificato un problema nelle procedure di aggiornamento del CICS.\r\nContatta l'assistenza tecnica";
                }
                else
                {
                    if (model.Tipologia == IndirizzoType.Residenza)
                        postActions.Add(() => { return Tracciato_Residenza(evento.ID_CMEVENTO, evento.COD_CMEVENTO, model.IsNew ? newRes : oldRes); });

                    if (model.Tipologia == IndirizzoType.Domicilio || (model.Tipologia == IndirizzoType.Residenza && model.AssegnaDomilicio))
                        postActions.Add(() => { return Tracciato_Domicilio(model.Matricola, TipoAnaVar.Domicilio, anag, null, evento.ID_CMEVENTO, model.Tipologia == IndirizzoType.Residenza || model.Decorrenza != DateTime.MinValue ? model.Decorrenza : DateTime.Today); });
                }
            }

            HrisHelper.LogOperazione("AggiornamentoIndirizzo", $"Aggiornamento indirizzo ID_PERS:{model.IdPersona} MATR:{model.Matricola}", result, errorMsg, null, null, Newtonsoft.Json.JsonConvert.SerializeObject(model));

            return result;
        }

        public static EventoModel GetDatiContrattuali(int idPersona, TipoEvento tipo)
        {
            EventoModel model = new EventoModel();
            model.Tipo = tipo;

            var db = GetDb();
            var sint = db.SINTESI1.FirstOrDefault(x => x.ID_PERSONA == idPersona);

            switch (tipo)
            {
                case TipoEvento.Contratto:
                    break;
                case TipoEvento.Sede:
                    var sede = db.TRASF_SEDE.Where(x => x.ID_PERSONA == idPersona).OrderByDescending(x => x.DTA_FINE).FirstOrDefault();
                    model.Codice = sede.COD_SEDE;
                    model.MinDate = sede.DTA_INIZIO.AddDays(1);
                    break;
                case TipoEvento.Servizio:
                    var servizio = db.XR_SERVIZIO.Where(x => x.ID_PERSONA == idPersona).OrderByDescending(x => x.DTA_FINE).FirstOrDefault();
                    model.Codice = servizio.COD_SERVIZIO;
                    model.MinDate = servizio.DTA_INIZIO.AddDays(1);
                    var sezioneSec = db.INCARLAV.Where(x => x.ID_PERSONA == idPersona).OrderByDescending(x => x.DTA_FINE).FirstOrDefault();
                    model.CodiceSec = sezioneSec.UNITAORG.COD_UNITAORG;
                    break;
                case TipoEvento.Qualifica:
                    break;
                case TipoEvento.Mansione:
                    break;
                case TipoEvento.Stato:
                    break;
                case TipoEvento.Sezione:
                    var sezione = db.INCARLAV.Where(x => x.ID_PERSONA == idPersona).OrderByDescending(x => x.DTA_FINE).FirstOrDefault();
                    model.Codice = sezione.UNITAORG.COD_UNITAORG;
                    model.MinDate = sezione.DTA_INIZIO.AddDays(1);
                    model.CodiceRif = sint.COD_SERVIZIO;
                    break;
                default:
                    break;
            }

            model.DataInizio = DateTime.Today;
            model.DataFine = sint.DTA_FINE_CR.HasValue ? sint.DTA_FINE_CR.Value : GetDateLimitMax();
            model.MaxDate = sint.DTA_FINE_CR.HasValue ? sint.DTA_FINE_CR.Value : GetDateLimitMax();

            CaricaIdentityData(model, sint);

            return model;
        }

        public static bool Insert_RichDatiContr(EventoModel model, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = GetDb();
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);
            XR_WKF_RICHIESTE richiesta = new XR_WKF_RICHIESTE()
            {
                ID_TIPOLOGIA = WKF_VAR_CONTR,
                MATRICOLA = model.Matricola,
                ID_PERSONA = model.IdPersona,
                COD_TIPO = model.Tipo.ToString(),
                DTA_CREAZIONE = tms,
                MODELLO = SerializerHelper.SerializeXml(model),
                COD_USER = codUser,
                COD_TERMID = codTermid,
                TMS_TIMESTAMP = tms
            };
            db.XR_WKF_RICHIESTE.Add(richiesta);
            WorkflowHelper.AddStato(db, richiesta, (int)VarContrStati.RichiestaInserita);

            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
            {
                result = true;
            }
            else
            {
                result = false;
                errorMsg = "Errore durante la creazione della richiesta";
            }

            return result;
        }


        public static bool Save_DatiContrattuali(EventoModel model, bool approva, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = GetDb();
            var idPersona = model.IdPersona;
            var sint = db.SINTESI1.Find(idPersona);
            var richiesta = db.XR_WKF_RICHIESTE.FirstOrDefault(x => x.ID_GESTIONE == model.IdRichiesta);

            DateTime dtaFine = model.DataFine;
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tmsTimestamp);

            bool anyChanges = false;
            if (approva)
            {
                switch (model.Tipo)
                {
                    case TipoEvento.Contratto:
                        break;
                    case TipoEvento.Sede:
                        var lastSede = db.TRASF_SEDE.Where(x => x.ID_PERSONA == idPersona).OrderByDescending(x => x.DTA_FINE).FirstOrDefault();
                        if (model.Codice != lastSede.COD_SEDE)
                        {
                            lastSede.DTA_FINE = model.DataInizio.AddDays(-1);
                            lastSede.COD_USER = codUser;
                            lastSede.COD_TERMID = codTermid;
                            lastSede.TMS_TIMESTAMP = tmsTimestamp;

                            TRASF_SEDE newSede = new TRASF_SEDE()
                            {
                                ID_TRASF_SEDE = db.TRASF_SEDE.GeneraPrimaryKey(),
                                ID_PERSONA = model.IdPersona,
                                COD_SEDE = model.Codice,
                                COD_EVTRASF = model.CodiceEvento,
                                COD_IMPRESA = "0",
                                DTA_INIZIO = model.DataInizio,
                                DTA_FINE = dtaFine
                            };
                            newSede.COD_USER = codUser;
                            newSede.COD_TERMID = codTermid;
                            newSede.TMS_TIMESTAMP = tmsTimestamp;
                            db.TRASF_SEDE.Add(newSede);

                            anyChanges = true;
                        }
                        break;
                    case TipoEvento.Servizio:
                        var lastServizio = db.XR_SERVIZIO.Where(x => x.ID_PERSONA == idPersona).OrderByDescending(x => x.DTA_FINE).FirstOrDefault();
                        if (model.Codice != lastServizio.COD_SERVIZIO)
                        {
                            lastServizio.DTA_FINE = model.DataInizio.AddDays(-1);
                            lastServizio.COD_USER = codUser;
                            lastServizio.COD_TERMID = codTermid;
                            lastServizio.TMS_TIMESTAMP = tmsTimestamp;

                            XR_SERVIZIO newServizio = new XR_SERVIZIO()
                            {
                                ID_XR_SERVIZIO = db.XR_SERVIZIO.GeneraPrimaryKey(),
                                ID_PERSONA = model.IdPersona,
                                COD_SERVIZIO = model.Codice,
                                COD_EVSERVIZIO = model.CodiceEvento,
                                DTA_INIZIO = model.DataInizio,
                                DTA_FINE = dtaFine
                            };
                            newServizio.COD_USER = codUser;
                            newServizio.COD_TERMID = codTermid;
                            newServizio.TMS_TIMESTAMP = tmsTimestamp;
                            db.XR_SERVIZIO.Add(newServizio);

                            anyChanges = true;
                        }
                        anyChanges = anyChanges || Save_DatiContrattuali_Sezione(db, idPersona, model.CodiceSec, model.CodiceEvento, model.DataInizio, dtaFine, codUser, codTermid, tmsTimestamp, anyChanges);
                        break;
                    case TipoEvento.Qualifica:
                        break;
                    case TipoEvento.Mansione:
                        break;
                    case TipoEvento.Stato:
                        break;
                    case TipoEvento.Sezione:
                        anyChanges = Save_DatiContrattuali_Sezione(db, idPersona, model.Codice, model.CodiceEvento, model.DataInizio, dtaFine, codUser, codTermid, tmsTimestamp, anyChanges);
                        break;
                    default:
                        break;
                }

                if (!Tracciato_TEX2929(model))
                {

                }
            }
            else
            {
                anyChanges = true;
            }

            WorkflowHelper.AddStato(db, richiesta, (int)(approva ? VarContrStati.RichiestaEvasa : VarContrStati.RichiestaCancellata));

            if (anyChanges)
            {
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
            }
            else
            {
                errorMsg = "Non è stato modificato alcun dato";
            }

            return result;
        }

        private static bool Save_DatiContrattuali_Sezione(CezanneDb db, int idPersona, string codice, string codiceEvento, DateTime dtaInizio, DateTime dtaFine, string codUser, string codTermid, DateTime tmsTimestamp, bool anyChanges)
        {
            INCARLAV lastSezione = db.INCARLAV.Where(x => x.ID_PERSONA == idPersona).OrderByDescending(x => x.DTA_FINE).FirstOrDefault();
            if (codice != lastSezione.UNITAORG.COD_UNITAORG)
            {
                lastSezione.DTA_FINE = dtaInizio.AddDays(-1);
                lastSezione.COD_USER = codUser;
                lastSezione.COD_TERMID = codTermid;
                lastSezione.TMS_TIMESTAMP = tmsTimestamp;

                var unita = db.UNITAORG.FirstOrDefault(x => x.COD_UNITAORG == codice);

                INCARLAV newSezione = new INCARLAV()
                {
                    ID_INCARLAV = db.XR_SERVIZIO.GeneraPrimaryKey(),
                    ID_PERSONA = idPersona,
                    ID_UNITAORG = unita.ID_UNITAORG,
                    COD_MOTIVOORGIN = codiceEvento,
                    IND_INCPRINC = "1",
                    PRC_PERCOCCUPAZ = 100,
                    DTA_INIZIO = dtaInizio,
                    DTA_FINE = dtaFine
                };
                newSezione.COD_USER = codUser;
                newSezione.COD_TERMID = codTermid;
                newSezione.TMS_TIMESTAMP = tmsTimestamp;
                db.INCARLAV.Add(newSezione);

                anyChanges = true;
            }

            return anyChanges;
        }
        #endregion

        #region Cancellazione

        #endregion

        #region SelectList
        public static List<SelectListItem> GetComuni(string filter, string value, bool addProv = true)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var db = GetDb();

            if (!String.IsNullOrWhiteSpace(value))
                result.AddRange(db.TB_COMUNE.Where(x => x.COD_CITTA == value).ToList()
                    .Select(x => new SelectListItem()
                    {
                        Value = x.COD_CITTA,
                        Text = x.DES_CITTA.TitleCase() + (addProv ? (x.COD_PROV_STATE != "EST" ? " (" + x.COD_PROV_STATE + ")" : "") : ""),
                        Selected = true
                    }
                    ));
            else
                result.AddRange(db.TB_COMUNE.Where(x => x.DES_CITTA.ToUpper().StartsWith(filter.ToUpper())).ToList()
                    .Select(x => new SelectListItem()
                    {
                        Value = x.COD_CITTA,
                        Text = x.DES_CITTA.TitleCase() + (addProv ? (x.COD_PROV_STATE != "EST" ? " (" + x.COD_PROV_STATE + ")" : "") : "")
                    }));

            return result;
        }
        public static List<SelectListItem> GetCittadinanza(string filter, string value)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var db = GetDb();

            if (!String.IsNullOrWhiteSpace(value))
                result.AddRange(db.TB_CITTAD.Where(x => x.COD_CITTAD == value).ToList()
                .Select(x => new SelectListItem()
                {
                    Value = x.COD_CITTAD,
                    Text = x.DES_CITTAD.TitleCase(),
                    Selected = true
                }
                ));
            else
                result.AddRange(db.TB_CITTAD.Where(x => x.DES_CITTAD.ToLower().Contains(filter.ToLower())).ToList()
                    .Select(x => new SelectListItem()
                    {
                        Value = x.COD_CITTAD,
                        Text = x.DES_CITTAD.TitleCase()
                    }));

            return result;
        }
        public static List<SelectListItem> GetStatiCivile()
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var db = GetDb();
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            result.AddRange(db.TB_STATOCV.Select(x => new SelectListItem()
            {
                Value = x.COD_STCIV,
                Text = x.DES_STCIV
            }));

            return result;
        }
        public static List<SelectListItem> GetEventi(TipoEvento tipo)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();
            if (tipo != TipoEvento.Contratto)
            {
                result.Add(new SelectListItem() { Value = "", Text = "Seleziona un evento" });
            }


            switch (tipo)
            {
                case TipoEvento.Contratto:
                    //qtaoridne = 1 quando sarà effettivo result.AddRange(db.XR_TB_EVSERVIZIO.OrderBy(x => x.DES_EVSERVIZIO).ToList().Where(y=>y.QTA_ORDINE == 1).Select(x => new SelectListItem() { Value = x.COD_EVSERVIZIO, Text = x.DES_EVSERVIZIO }));
                    result.Add(new SelectListItem() { Value = "", Text = "Seleziona un tipo contratto" });
                    result.AddRange(db.XR_TB_EVSERVIZIO.OrderBy(x => x.DES_EVSERVIZIO).ToList().Select(x => new SelectListItem() { Value = x.COD_EVSERVIZIO, Text = x.DES_EVSERVIZIO + " (" + x.COD_EVSERVIZIO + ")" }));
                    break;
                case TipoEvento.Sede:
                    result.AddRange(db.TB_EVTRASF.OrderBy(x => x.DES_EVTRASF).ToList().Select(x => new SelectListItem() { Value = x.COD_EVTRASF, Text = x.DES_EVTRASF }));
                    break;
                case TipoEvento.Servizio:
                    result.AddRange(db.XR_TB_EVSERVIZIO.OrderBy(x => x.DES_EVSERVIZIO).ToList().Select(x => new SelectListItem() { Value = x.COD_EVSERVIZIO, Text = x.DES_EVSERVIZIO }));
                    break;
                case TipoEvento.Qualifica:
                    break;
                case TipoEvento.Mansione:
                    break;
                case TipoEvento.Stato:
                    break;
                case TipoEvento.Sezione:
                    result.AddRange(db.TB_TPEVORG.OrderBy(x => x.DES_MOTIVOORG).ToList().Select(x => new SelectListItem() { Value = x.COD_MOTIVOORG, Text = x.DES_MOTIVOORG }));
                    break;
                default:
                    break;
            }

            return result;
        }

        public static List<SelectListItem> GetSedi(string filter, string value, bool loadAll = false, AbilSedi abil = null, bool addCodDes = true, bool addDefault = true, bool addGroup = false, bool addCodParentesi = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<SEDE> tmp = db.SEDE.Where(x => x.DTA_INIZIO <= DateTime.Today && x.DTA_FINE >= DateTime.Today);
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = tmp.Where(x => x.COD_IMPRESA == "0" && x.COD_SEDE == value);
                else
                    tmp = tmp.Where(x => x.COD_IMPRESA == "0" && x.DES_SEDE.Contains(filter));
                //tmp = tmp.Where(x => x.COD_IMPRESA == "0" && x.DES_SEDE.StartsWith(filter));
            }

            if (abil != null && abil.HasFilter)
            {
                if (abil.SediIncluse.Any())
                    tmp = tmp.Where(x => abil.SediIncluse.Any(y => y == x.COD_SEDE || y.StartsWith(x.COD_SEDE.Substring(0, 2))));


                if (abil.SediEscluse.Any())
                    tmp = tmp.Where(x => !abil.SediEscluse.Any(y => y == x.COD_SEDE || y.StartsWith(x.COD_SEDE.Substring(0, 2))));
            }

            tmp = tmp.Where(x => x.COD_SEDE != "***");

            if (addDefault && loadAll)
                result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            if (addCodParentesi)
                result.AddRange(tmp.OrderBy(x => x.COD_SEDE).ToList().Select(x => new SelectListItem { Value = x.COD_SEDE, Text = CezanneHelper.GetDes(x.COD_SEDE, x.DES_SEDE).TitleCase() + " (" + x.COD_SEDE + ")" }));
            else
                result.AddRange(tmp.OrderBy(x => x.COD_SEDE).ToList().Select(x => new SelectListItem { Value = x.COD_SEDE, Text = (addCodDes ? x.COD_SEDE + " - " : "") + CezanneHelper.GetDes(x.COD_SEDE, x.DES_SEDE).TitleCase() }));

            return result;
        }
        public static List<SelectListItem> GetServizi(string filter, string value, bool loadAll = false, AbilDir abilDir = null, bool filterSocieta = false, AbilSocieta societa = null, string aziendaVal = "", bool addCodDes = true, bool addDefault = true, bool addCodParentesi = false, bool setSelected = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<XR_TB_SERVIZIO> tmp = db.XR_TB_SERVIZIO.Where(x => x.COD_SERVIZIO.Trim().Length == 2 && x.COD_IMPRESA != null);

            tmp = tmp.Where(x => !db.XR_TB_SERVIZIO_EXT.Any(y => y.COD_SERVIZIO.Trim() == x.COD_SERVIZIO.Trim())
                                || db.XR_TB_SERVIZIO_EXT.Any(y => y.COD_SERVIZIO.Trim() == x.COD_SERVIZIO.Trim()
                                                                    && y.DTA_INIZIO <= DateTime.Today && y.DTA_FINE >= DateTime.Today));

            if (filterSocieta)
            {
                IQueryable<CODIFYIMP> tmpSoc = db.STRGROUP
                                .Where(x => x.DTA_INIZIO <= DateTime.Today && x.DTA_FINE >= DateTime.Today)
                                .SelectMany(x => x.R_STRGROUP)
                                .OrderBy(y => y.PRG_SORT)
                                .Select(x => x.CODIFYIMP);

                if (societa != null && societa.HasFilter)
                {
                    if (societa.SocietaIncluse.Any())
                        tmpSoc = tmpSoc.Where(x => societa.SocietaIncluse.Contains(x.COD_IMPRESA));

                    if (societa.SocietaEscluse.Any())
                        tmpSoc = tmpSoc.Where(x => !societa.SocietaEscluse.Contains(x.COD_IMPRESA));
                }

                List<string> listSoc = tmpSoc.Select(x => x.COD_IMPRESA).ToList();

                tmp = tmp.Where(x => listSoc.Contains(x.COD_IMPRESA));
            }


            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = tmp.Where(x => x.COD_SERVIZIO == value);
                else
                    //tmp = tmp.Where(x => x.DES_SERVIZIO.StartsWith(filter));
                    tmp = tmp.Where(x => x.DES_SERVIZIO.Contains(filter));

                if (!String.IsNullOrWhiteSpace(aziendaVal))
                    tmp = tmp.Where(x => x.COD_IMPRESA == aziendaVal);
            }

            if (abilDir != null && abilDir.HasFilter)
            {
                if (abilDir.DirezioniIncluse.Any())
                    tmp = tmp.Where(x => abilDir.DirezioniIncluse.Contains(x.COD_SERVIZIO));

                if (abilDir.DirezioniEscluse.Any())
                    tmp = tmp.Where(x => !abilDir.DirezioniEscluse.Contains(x.COD_SERVIZIO));
            }

            if (addDefault && loadAll)
                result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            if (addCodParentesi)
                result.AddRange(tmp.ToList().Select(x => new SelectListItem { Value = x.COD_SERVIZIO, Text = CezanneHelper.GetDes(x.COD_SERVIZIO, x.DES_SERVIZIO).TitleCase() + " (" + x.COD_SERVIZIO + ")" }));
            else
                result.AddRange(tmp.ToList().Select(x => new SelectListItem { Value = x.COD_SERVIZIO, Text = (addCodDes ? x.COD_SERVIZIO + " - " : "") + CezanneHelper.GetDes(x.COD_SERVIZIO, x.DES_SERVIZIO).TitleCase() }));

            if (!String.IsNullOrWhiteSpace(value) && setSelected)
            {
                var item = result.Where(w => w.Value == value).FirstOrDefault();
                item.Selected = true;
            }

            return result;
        }
        public static List<SelectListItem> GetSezioni(string filter, string value, string codiceServizio = "", bool addCodParentesi = false, bool setSelected = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = new TalentiaDB();
            string currentDate = DateTime.Today.ToString("yyyyMMdd");

            IQueryable<myRaiDataTalentia.XR_STR_TSEZIONE> tmp = null;
            if (!string.IsNullOrEmpty(value))
            {
                tmp = db.XR_STR_TSEZIONE.Where(x => x.data_fine_validita.CompareTo(currentDate) > 0 && x.codice_visibile == value);

            }
            else
            {
                tmp = db.XR_STR_TSEZIONE.Where(x => x.data_fine_validita.CompareTo(currentDate) > 0);
                if (!String.IsNullOrWhiteSpace(codiceServizio))
                    tmp = tmp.Where(x => x.servizio == codiceServizio);
                tmp = tmp.Where(x => (x.codice_visibile.Trim() + " - " + x.descrizione_lunga).Contains(filter));
                //tmp = tmp.Where(x => (x.codice_visibile.Trim() + " - " + x.descrizione_lunga).StartsWith(filter));
            }

            result.AddRange(tmp.ToList().Select(x => new SelectListItem { Value = x.codice_visibile, Text = addCodParentesi ? x.descrizione_lunga + " (" + x.codice_visibile + ")" : x.descrizione_lunga }));
            if (!String.IsNullOrWhiteSpace(value) && setSelected)
            {
                var item = result.Where(w => w.Value == value).FirstOrDefault();
                item.Selected = true;
            }
            return result;
        }
        public static List<SelectListItem> GetTipoMinimo(string filter, string value, string codiceServizio = "")
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<XR_TB_TIPOMINIMO> tmp = null;
            tmp = db.XR_TB_TIPOMINIMO;
            if (!string.IsNullOrEmpty(value))
            {
                tmp = db.XR_TB_TIPOMINIMO.Where(x => x.COD_TIPOMINIMO == value);

            }
            if (!string.IsNullOrWhiteSpace(filter))
            {
                tmp = db.XR_TB_TIPOMINIMO.Where(x => x.DES_TIPOMINIMO.Contains(filter));
                //tmp = db.XR_TB_TIPOMINIMO.Where(x => x.DES_TIPOMINIMO.StartsWith(filter));
            }

            tmp = tmp.Where(x => x.IND_WEBVISIBLE == "Y");
            result.AddRange(tmp.OrderBy(x => x.COD_TIPOMINIMO).ToList().Select(x => new SelectListItem { Value = x.COD_TIPOMINIMO, Text = x.DES_TIPOMINIMO + " (" + x.COD_TIPOMINIMO + ")" }));

            return result;
        }
        public static List<SelectListItem> GetOrarioTipiMinimi(string tipoMinimo = "", string categoria = "")
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            //IQueryable<XR_TB_TIPOMINIMO> tmp = null;
            IQueryable<XR_TB_MINIMI> tmp = null;
            tmp = db.XR_TB_MINIMI;
            if (!string.IsNullOrEmpty(tipoMinimo) && !string.IsNullOrWhiteSpace(categoria))
            {
                tmp = db.XR_TB_MINIMI.Where(x => x.TIPOMINIMO == tipoMinimo && x.CATEGORIA == categoria);

                result.AddRange(tmp.Where(x => x.ORARIO_SETT != null && x.ORARIO_SETT != "").ToList().OrderBy(x => x.TIPOMINIMO).ToList().Select(x => new SelectListItem { Value = x.ORARIO_SETT, Text = x.ORARIO_SETT }));
            }
            return result;
        }
        public static List<SelectListItem> GetTipoMinimoByCategoria(string filter, string value, string codiceServizio = "", string categoria = "")
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            //IQueryable<XR_TB_TIPOMINIMO> tmp = null;
            IQueryable<XR_TB_MINIMI> tmp2 = null;
            //tmp = db.XR_TB_TIPOMINIMO;
            tmp2 = db.XR_TB_MINIMI;
            if (!string.IsNullOrWhiteSpace(categoria) && categoria != "-1")
            {
                tmp2 = db.XR_TB_MINIMI.Where(x => x.CATEGORIA == categoria);
            }

            //tmp = tmp.Where(x => x.IND_WEBVISIBLE == "Y");
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona tipo minimo" });
            tmp2 = tmp2.GroupBy(x => x.TIPOMINIMO).Select(x => x.FirstOrDefault());
            result.AddRange(tmp2.OrderBy(x => x.TIPOMINIMO).ToList().Select(x => new SelectListItem { Value = x.TIPOMINIMO, Text = x.DESCRIZIONE + " (" + x.TIPOMINIMO + ")" }));

            return result;
        }
        public static List<SelectListItem> GetGruppiCat()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = new TalentiaDB();
            var dbI = new IncentiviEntities();
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            result.AddRange(dbI.XR_HRIS_QUAL_FILTER.OrderBy(x => x.ORDINE.HasValue ? x.ORDINE.Value : 0).ToList()
                                .Select(x => new SelectListItem()
                                {
                                    Value = x.ID_QUAL_FILTER.ToString(),
                                    Text = x.DESCRIPTION
                                }));

            return result;
        }
        public static List<SelectListItem> GetGruppiArea()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = new TalentiaDB();
            var dbI = new IncentiviEntities();
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            result.AddRange(dbI.XR_HRIS_DIR_FILTER.ToList()
                                .Select(x => new SelectListItem()
                                {
                                    Value = x.ID_AREA_FILTER.ToString(),
                                    Text = x.COD_AREA_FILTER + " - " + x.DESCRIPTION
                                }));

            return result;
        }
        public static List<SelectListItem> GetCategorie(string filter, string value, bool loadAll = false, AbilCat abil = null, string selValue = null, bool addCodDes = true, string notQualStd = "", string qualStd = "", bool addDefault = true, bool addCodParentesi = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<QUALIFICA> tmp = null;
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.QUALIFICA.Where(x => x.COD_QUALIFICA == value);
                else
                    tmp = db.QUALIFICA.Where(x => x.DES_QUALIFICA.Contains(filter));
                //tmp = db.QUALIFICA.Where(x => x.DES_QUALIFICA.StartsWith(filter));
            }
            else
                tmp = db.QUALIFICA;

            if (!String.IsNullOrWhiteSpace(notQualStd))
                tmp = tmp.Where(x => x.COD_QUALSTD != notQualStd);

            if (!String.IsNullOrWhiteSpace(qualStd))
                tmp = tmp.Where(x => x.COD_QUALSTD == notQualStd);

            if (abil != null && abil.HasFilter)
            {
                if (abil.CategorieIncluse.Any())
                    tmp = tmp.Where(x => abil.CategorieIncluse.Any(y => x.COD_QUALIFICA.StartsWith(y)));

                if (abil.CategorieEscluse.Any())
                    tmp = tmp.Where(x => !abil.CategorieEscluse.Any(y => x.COD_QUALIFICA.StartsWith(y)));
            }

            tmp = tmp.Where(x => !x.DES_QUALIFICA.ToUpper().Contains("DESCRIZIONE ASSENTE"));

            if (addDefault && loadAll)
                result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            if (addCodParentesi)
                result.AddRange(tmp.ToList().Select(x => new SelectListItem { Value = x.COD_QUALIFICA, Text = CezanneHelper.GetDes(x.COD_QUALIFICA, x.DES_QUALIFICA).TitleCase() + " (" + x.COD_QUALIFICA + ")", Selected = (selValue ?? "") == x.COD_QUALIFICA }));
            else
                result.AddRange(tmp.ToList().Select(x => new SelectListItem { Value = x.COD_QUALIFICA, Text = (addCodDes ? x.COD_QUALIFICA + " - " : "") + CezanneHelper.GetDes(x.COD_QUALIFICA, x.DES_QUALIFICA).TitleCase(), Selected = (selValue ?? "") == x.COD_QUALIFICA }));

            return result;
        }
        public static List<SelectListItem> GetCategorieHRDW(string filter, string value, bool loadAll = false, AbilCat abil = null, string selValue = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<V_HRDW_L2D_CATEGORIA> tmp = null;
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.V_HRDW_L2D_CATEGORIA.Where(x => x.cod_categoria == value);
                else
                    tmp = db.V_HRDW_L2D_CATEGORIA.Where(x => (x.cod_categoria + " - " + x.desc_categoria).StartsWith(filter));
            }
            else
                tmp = db.V_HRDW_L2D_CATEGORIA;

            tmp = tmp.Where(x => x.data_inizio_val <= DateTime.Today && x.Data_Fine_Val >= DateTime.Today);

            if (abil != null && abil.HasFilter)
            {
                if (abil.CategorieIncluse.Any())
                    tmp = tmp.Where(x => abil.CategorieIncluse.Any(y => x.cod_categoria.StartsWith(y)));

                if (!abil.CategorieEscluse.Any())
                    tmp = tmp.Where(x => !abil.CategorieEscluse.Any(y => x.cod_categoria.StartsWith(y)));
            }

            result.AddRange(tmp.ToList().Select(x => new SelectListItem { Value = x.cod_categoria, Text = x.cod_categoria + " - " + x.desc_categoria.TitleCase(), Selected = (selValue ?? "") == x.cod_categoria }));

            return result;
        }
        public static List<SelectListItem> GetTipiContratto(string filter, string value, bool loadAll = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<TB_TPCNTR> tmp = null;
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.TB_TPCNTR.Where(x => x.COD_TPCNTR == value);
                else
                    tmp = db.TB_TPCNTR.Where(x => x.DES_TPCNTR.Contains(filter));
                //tmp = db.TB_TPCNTR.Where(x => x.DES_TPCNTR.StartsWith(filter));
            }
            else
                tmp = db.TB_TPCNTR;

            result.AddRange(tmp.ToList().Select(x => new SelectListItem { Value = x.COD_TPCNTR, Text = x.COD_TPCNTR + " - " + CezanneHelper.GetDes(x.COD_TPCNTR, x.DES_TPCNTR).TitleCase() }));

            return result;
        }
        public static List<SelectListItem> GetTipiRapportoLavorativo(string filter, string value, bool loadAll = false, bool addCodDes = true)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<TB_TPRLAV> tmp = null;
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.TB_TPRLAV.Where(x => x.COD_TIPORLAV == value);
                else
                    tmp = db.TB_TPRLAV.Where(x => x.DES_TIPORLAV.Contains(filter));
                //tmp = db.TB_TPRLAV.Where(x => x.DES_TIPORLAV.StartsWith(filter));
            }
            else
                tmp = db.TB_TPRLAV;

            tmp = tmp.Where(w => w.IND_LIBROMAT == "Y");
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            result.AddRange(tmp.OrderBy(x => x.COD_TIPORLAV).ToList().Select(x => new SelectListItem { Value = x.COD_TIPORLAV, Text = (addCodDes ? x.COD_TIPORLAV + " - " : "") + x.DES_TIPORLAV.TitleCase() }));

            return result;
        }
        public static List<SelectListItem> GetTipiContratto(string filter, string value, bool loadAll = false, bool addCodDes = true)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            result = GetEventi(TipoEvento.Contratto);
            //var db = GetDb();

            //IQueryable<TB_TPRLAV> tmp = null;
            //if (!loadAll)
            //{
            //    if (!String.IsNullOrWhiteSpace(value))
            //        tmp = db.TB_TPRLAV.Where(x => x.COD_TIPORLAV == value);
            //    else
            //        tmp = db.TB_TPRLAV.Where(x => x.DES_TIPORLAV.StartsWith(filter));
            //}
            //else
            //    tmp = db.TB_TPRLAV;

            //tmp = tmp.Where(w => w.IND_LIBROMAT == "Y");
            //result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            //result.Add(new SelectListItem() { Value = "1", Text = "Impiegati/Operai" });
            //result.Add(new SelectListItem() { Value = "2", Text = "Giornalisti" });
            //result.Add(new SelectListItem() { Value = "3", Text = "Dirigenti" });
            //result.Add(new SelectListItem() { Value = "4", Text = "Orchestrali" });
            //result.Add(new SelectListItem() { Value = "5", Text = "Medici" });
            //result.AddRange(tmp.OrderBy(x => x.COD_TIPORLAV).ToList().Select(x => new SelectListItem { Value = x.COD_TIPORLAV, Text = (addCodDes ? x.COD_TIPORLAV + " - " : "") + x.DES_TIPORLAV.TitleCase() }));

            return result;
        }
        public static List<SelectListItem> GetTipiRapportoLavorativoAssunzione(string filter, string value, bool loadAll = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<XR_TB_CAUSALEMOV> tmp = null;
            IQueryable<XR_TB_CANASSUNZ> tmpCanAssunz = null;
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.XR_TB_CAUSALEMOV.Where(x => x.COD_CAUSALEMOV == value);
                else
                    tmp = db.XR_TB_CAUSALEMOV.Where(x => x.DES_CAUSALEMOV.Contains(filter));
                //tmp = db.XR_TB_CAUSALEMOV.Where(x => x.DES_CAUSALEMOV.StartsWith(filter));
            }
            else
                tmp = db.XR_TB_CAUSALEMOV;

            //tmp = tmp.Where(w => w.QTA_ORDINE == 1);
            if (tmp.Count() > 0)
            {
                tmpCanAssunz = db.XR_TB_CANASSUNZ;
            }
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona tipologia rapporto lavorativo" });
            if (tmp.Count() > 0 && tmpCanAssunz.Count() > 0)
            {
                foreach (var item in tmp)
                {
                    foreach (var item2 in tmpCanAssunz)
                    {
                        result.Add(new SelectListItem { Value = item.COD_CAUSALEMOV + item2.COD_CANASSUNZ, Text = item.DES_CAUSALEMOV + " - " + item2.DES_CANASSUNZ.TitleCase() });
                    }
                }
            }
            //result.AddRange(tmp.OrderBy(x => x.COD_CAUSALEMOV).ToList().Select(x => new SelectListItem { Value = x.COD_CAUSALEMOV, Text = (addCodDes ? x.COD_CAUSALEMOV + " - " : "") + x.DES_CAUSALEMOV.TitleCase() }));

            return result.OrderBy(x => x.Value).ToList();
        }
        public static List<SelectListItem> GetTipoAssunzione(string filter, string value, bool loadAll = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<TB_EVENTO> tmp = null;
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.TB_EVENTO.Where(x => x.COD_EVQUAL == value);
                else
                    tmp = db.TB_EVENTO.Where(x => x.DES_EVQUAL.Contains(filter));
                //tmp = db.TB_EVENTO.Where(x => x.DES_EVQUAL.StartsWith(filter));
            }
            else
                tmp = db.TB_EVENTO;

            tmp = tmp.Where(x => x.IND_WEBVISIBLE == "Y" && x.AGGREGATO_EVENTO == "ASSUNZIONE");
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona tipo assunzione" });
            if (tmp.Count() > 0 && tmp.Count() > 0)
            {
                result.AddRange(tmp.OrderBy(x => x.COD_EVQUAL).ToList().Select(x => new SelectListItem { Value = x.COD_EVQUAL, Text = x.DES_EVQUAL.TitleCase() + " (" + x.COD_EVQUAL + ")" }));
            }

            return result;
        }
        public static List<SelectListItem> GetFormaContratto(string filter, string value, bool loadAll = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<TB_TPCNTR> tmp = null;
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.TB_TPCNTR.Where(x => x.COD_TPCNTR == value);
                else
                    tmp = db.TB_TPCNTR.Where(x => x.DES_TPCNTR.Contains(filter));
                //tmp = db.TB_TPCNTR.Where(x => x.DES_TPCNTR.StartsWith(filter));
            }
            else
                tmp = db.TB_TPCNTR;

            result.Add(new SelectListItem() { Value = "", Text = "Seleziona forma contratto" });
            if (tmp != null && tmp.Count() > 0)
            {
                result.AddRange(tmp.OrderBy(y => y.QTA_ORDINE).ToList().Select(x => new SelectListItem { Value = x.COD_TPCNTR, Text = x.DES_TPCNTR.TitleCase() + " (" + x.COD_TPCNTR + ")" }));
            }
            return result;
        }
        public static List<SelectListItem> GetCausaleAssunzione(string filter, string value, bool loadAll = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<XR_TB_CAUSALEMOV> tmp = null;
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.XR_TB_CAUSALEMOV.Where(x => x.COD_CAUSALEMOV == value);
                else
                    tmp = db.XR_TB_CAUSALEMOV.Where(x => x.DES_CAUSALEMOV.Contains(filter));
                //tmp = db.XR_TB_CAUSALEMOV.Where(x => x.DES_CAUSALEMOV.StartsWith(filter));
            }
            else
                tmp = db.XR_TB_CAUSALEMOV;
            tmp = tmp.Where(x => x.IND_WEBVISIBLE == "Y");
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona causale assunzione" });
            if (tmp != null && tmp.Count() > 0)
            {
                //tmp = tmp.Where(x => x.IND_WEBVISIBLE == "Y");
                result.AddRange(tmp.OrderBy(y => y.COD_CAUSALEMOV).ToList().Select(x => new SelectListItem { Value = x.COD_CAUSALEMOV, Text = x.DES_CAUSALEMOV.TitleCase() + " (" + x.COD_CAUSALEMOV + ")" }));
            }
            return result;
        }
        public static List<SelectListItem> GetModalitaReclutamento(string filter, string value, bool loadAll = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<XR_TB_CANASSUNZ> tmp = null;
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.XR_TB_CANASSUNZ.Where(x => x.COD_CANASSUNZ == value);
                else
                    tmp = db.XR_TB_CANASSUNZ.Where(x => x.DES_CANASSUNZ.Contains(filter));
                //tmp = db.XR_TB_CANASSUNZ.Where(x => x.DES_CANASSUNZ.StartsWith(filter));
            }
            else
                tmp = db.XR_TB_CANASSUNZ;
            tmp = tmp.Where(x => x.IND_WEBVISIBLE == "Y");
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona modalità di reclutamento" });
            if (tmp != null && tmp.Count() > 0)
            {
                result.AddRange(tmp.OrderBy(x => x.COD_CANASSUNZ).ToList().Select(x => new SelectListItem { Value = x.COD_CANASSUNZ, Text = x.DES_CANASSUNZ.TitleCase() + " (" + x.COD_CANASSUNZ + ")" }));
            }
            return result;
        }
        public static List<SelectListItem> GetAssicurazioneInfortuni(string filter, string value, bool loadAll = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<XR_TB_ASSINFORTUNI> tmp = null;
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.XR_TB_ASSINFORTUNI.Where(x => x.COD_ASSINFORTUNI == value);
                else
                    tmp = db.XR_TB_ASSINFORTUNI.Where(x => x.DES_ASSINFORTUNI.Contains(filter));
                //tmp = db.XR_TB_ASSINFORTUNI.Where(x => x.DES_ASSINFORTUNI.StartsWith(filter));
            }
            else
                tmp = db.XR_TB_ASSINFORTUNI;
            tmp = tmp.Where(x => x.IND_WEBVISIBLE == "Y");
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona assicurazione infortuni" });
            if (tmp != null && tmp.Count() > 0)
            {
                result.AddRange(tmp.OrderBy(x => x.COD_ASSINFORTUNI).ToList().Select(x => new SelectListItem { Value = x.COD_ASSINFORTUNI, Text = x.DES_ASSINFORTUNI.TitleCase() + " (" + x.COD_ASSINFORTUNI + ")" }));
            }
            return result;
        }
        public static List<SelectListItem> GetIndennita(string filter, string value, bool loadAll = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<XR_TB_INDENNITA> tmp = null;
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.XR_TB_INDENNITA.Where(x => x.COD_INDENNITA == value);
                else
                    tmp = db.XR_TB_INDENNITA.Where(x => x.DES_INDENNITA.Contains(filter));
                //tmp = db.XR_TB_INDENNITA.Where(x => x.DES_INDENNITA.StartsWith(filter));
            }
            else
                tmp = db.XR_TB_INDENNITA;
            tmp = tmp.Where(x => x.IND_WEBVISIBLE == "Y");
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona indennità" });
            if (tmp != null && tmp.Count() > 0)
            {
                result.AddRange(tmp.OrderBy(x => x.COD_INDENNITA).ToList().Select(x => new SelectListItem { Value = x.COD_INDENNITA, Text = x.DES_INDENNITA.TitleCase() + " (" + x.COD_INDENNITA + ")" }));
            }
            return result;
        }
        public static List<SelectListItem> GetStatoCivile(string filter, string value, bool loadAll = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<TB_STATOCV> tmp = null;
            if (!loadAll)
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.TB_STATOCV.Where(x => x.COD_STCIV == value);
                else
                    tmp = db.TB_STATOCV.Where(x => x.DES_STCIV.Contains(filter));
                //tmp = db.TB_STATOCV.Where(x => x.DES_STCIV.StartsWith(filter));
            }
            else
                tmp = db.TB_STATOCV;
            tmp = tmp.Where(x => x.IND_WEBVISIBLE == "Y");
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona stato civile" });
            if (tmp != null && tmp.Count() > 0)
            {
                result.AddRange(tmp.OrderBy(x => x.COD_STCIV).ToList().Select(x => new SelectListItem { Value = x.COD_STCIV, Text = x.DES_STCIV.TitleCase() }));
            }
            return result;
        }
        public static List<SelectListItem> GetInsediamento(string filter, string value, bool loadAll = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = new digiGappEntities();

            IQueryable<L2D_INSEDIAMENTO> tmp = null;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                if (!String.IsNullOrWhiteSpace(value))
                    tmp = db.L2D_INSEDIAMENTO.Where(x => x.Codsede == value);
            }
            else
                tmp = db.L2D_INSEDIAMENTO;

            result.Add(new SelectListItem() { Value = "", Text = "Seleziona insediamento" });
            if (tmp != null && tmp.Count() > 0)
            {
                result.AddRange(tmp.OrderBy(x => x.desc_insediamento).ToList().Select(x => new SelectListItem { Value = x.cod_insediamento, Text = x.desc_insediamento.TitleCase() + " (" + x.cod_insediamento + ")" }));
            }
            return result;
        }
        public static List<SelectListItem> GetMansione(string filter, string value, bool addCodParentesi = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<RUOLO> tmp = null;
            if (!String.IsNullOrWhiteSpace(value))
                tmp = db.RUOLO.Where(x => x.COD_RUOLO == value);
            else
                tmp = db.RUOLO.Where(x => x.DES_RUOLO.Contains(filter));
            //tmp = db.RUOLO.Where(x => x.DES_RUOLO.StartsWith(filter));

            //Mansioni contabili
            tmp = tmp.Where(x => db.RUOLO.Where(y => y.COD_RUOLOAGGREG == "MANC").Select(y => y.COD_RUOLO).Contains(x.COD_RUOLOAGGREG));
            //Filtro validità
            tmp = tmp.Where(x => x.DTA_INIZIO <= DateTime.Today && x.DTA_FINE >= DateTime.Today);
            if (addCodParentesi)
                result.AddRange(tmp.ToList().Select(x => new SelectListItem { Value = x.COD_RUOLO, Text = CezanneHelper.GetDes(x.COD_RUOLO, x.DES_RUOLO).TitleCase() + " (" + x.COD_RUOLO + ")" }));
            else
                result.AddRange(tmp.ToList().Select(x => new SelectListItem { Value = x.COD_RUOLO, Text = x.COD_RUOLO + " - " + CezanneHelper.GetDes(x.COD_RUOLO, x.DES_RUOLO).TitleCase() }));

            return result;
        }
        public static List<SelectListItem> GetSocieta(string filter, string value, bool loadAll = false, AbilSocieta societa = null, bool addCodDes = true, bool adddDefault = true, bool addCodParentesi = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var db = GetDb();

            IQueryable<CODIFYIMP> tmp = db.STRGROUP
                                            .Where(x => x.DTA_INIZIO <= DateTime.Today && x.DTA_FINE >= DateTime.Today)
                                            .SelectMany(x => x.R_STRGROUP)
                                            .OrderBy(y => y.PRG_SORT)
                                            .Select(x => x.CODIFYIMP);

            if (!loadAll)
            {
                if (!string.IsNullOrEmpty(value))
                    tmp = tmp.Where(x => x.COD_IMPRESA == value);
                else
                    tmp = tmp.Where(x => x.DES_RAGSOC.Contains(filter));
                //tmp = tmp.Where(x => x.DES_RAGSOC.StartsWith(filter));
            }

            if (societa != null && societa.HasFilter)
            {
                if (societa.SocietaIncluse.Any())
                    tmp = tmp.Where(x => societa.SocietaIncluse.Contains(x.COD_IMPRESA));

                if (societa.SocietaEscluse.Any())
                    tmp = tmp.Where(x => !societa.SocietaEscluse.Contains(x.COD_IMPRESA));
            }

            if (adddDefault && loadAll)
                result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            if (addCodParentesi)
                result.AddRange(tmp.ToList().Select(x => new SelectListItem { Value = x.COD_IMPRESA, Text = CezanneHelper.GetDes(x.COD_IMPRESA, x.COD_SOGGETTO).TitleCase() + " (" + x.COD_IMPRESA + ")" }));
            else
                result.AddRange(tmp.ToList().Select(x => new SelectListItem { Value = x.COD_IMPRESA, Text = (addCodDes ? x.COD_IMPRESA + " - " : "") + CezanneHelper.GetDes(x.COD_IMPRESA, x.COD_SOGGETTO).TitleCase() }));

            return result;
        }
        public static List<SelectListItem> GetSocietaFromAlbero(string filter, string value, bool loadAll = false, AbilSocieta societa = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            TalentiaDB db = new TalentiaDB();

            IQueryable<myRaiDataTalentia.XR_STR_TSEZIONE> tmp = null;
            if (!loadAll)
            {
                if (!string.IsNullOrEmpty(value))
                    tmp = db.XR_STR_TSEZIONE.Where(x => x.codice_visibile == value);
                else
                    tmp = db.XR_STR_TSEZIONE.Where(x => x.descrizione_lunga.Contains(filter));
                //tmp = db.XR_STR_TSEZIONE.Where(x => x.descrizione_lunga.StartsWith(filter));
            }
            else
                tmp = db.XR_STR_TSEZIONE.Where(x => x.codice_visibile.Trim().Length == 1);

            string date = DateTime.Now.ToString("yyyyMMdd");
            tmp = tmp.Where(x => x.id != 1 && x.data_inizio_validita.CompareTo(date) <= 0 && x.data_fine_validita.CompareTo(date) >= 0);

            if (societa != null && societa.HasFilter)
            {
                if (societa.SocietaIncluse.Any())
                    tmp = tmp.Where(x => societa.SocietaIncluse.Contains(x.codice_visibile));

                if (societa.SocietaEscluse.Any())
                    tmp = tmp.Where(x => !societa.SocietaEscluse.Contains(x.codice_visibile));
            }

            result.AddRange(tmp.ToList().Select(x => new SelectListItem { Value = x.codice_visibile, Text = x.codice_visibile + " - " + x.descrizione_lunga.TitleCase() }));

            return result;
        }

        #endregion

        #region documenti personali e ammninistrativi

        public static List<ItemDocumentoPersonale> GetDocumentiPersonali(string matricola, string codice)
        {
            myRaiServiceHub.it.rai.servizi.hrgb.Service srvHrgb = new myRaiServiceHub.it.rai.servizi.hrgb.Service();
            List<ItemDocumentoPersonale> listaDoc = new List<ItemDocumentoPersonale>();
            //srvHrgb.Credentials = System.Net.CredentialCache.DefaultCredentials;
            string logonId = CommonHelper.GetCurrentUserMatricola();
            myRaiServiceHub.it.rai.servizi.hrgb.HrDoc_Doc rit = srvHrgb.Get_HrDoc_Doc("P" + logonId, matricola, codice);
            if (rit.Cod_Errore == "0")
            {
                DataTableReader dtr = rit.DT_HrDoc_DocMatricola.CreateDataReader();
                rit.DT_HrDoc_DocMatricola.Load(dtr);
                if (dtr.HasRows)
                {
                    foreach (System.Data.DataRow row in rit.DT_HrDoc_DocMatricola.Rows)
                    {
                        listaDoc.Add(new ItemDocumentoPersonale
                        {
                            Matricola = row[0].ToString(),
                            Progressivo = row[1].ToString(),
                            Cod_Tipologia = row[2].ToString(),
                            Pagine_Tot = row[3].ToString(),
                            Data_Doc = row[4].ToString(),
                            Protocollo = row[5].ToString(),
                            Emittente = row[6].ToString(),
                            Collocazione = row[7].ToString(),
                            Operazione = row[8].ToString(),
                            Testo = row[9].ToString(),
                        });
                    }
                };
            }
            return listaDoc;
        }

        public static List<myRaiServiceHub.it.rai.servizi.hrpaga.ListaDatiDocumenti> GetDocumentiAmministrativi(string matricola, string codice, string anno)
        {
            myRaiServiceHub.it.rai.servizi.hrpaga.HrPaga hrPaga = new myRaiServiceHub.it.rai.servizi.hrpaga.HrPaga();
            hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
            myRaiServiceHub.it.rai.servizi.hrpaga.ListaDocumenti lista = hrPaga.ElencoDocumentiPersonali(codice, matricola, "");
            return lista.ListaDatiDocumenti?.Where(l => l.DataCompetenza.StartsWith(anno)).OrderBy(l => l.DataPubblicazione).ToList();
        }

        public static myRaiServiceHub.it.rai.servizi.hrpaga.Documento GetDocumentoPersonale(string id, string matricola)
        {
            myRaiServiceHub.it.rai.servizi.hrpaga.HrPaga hrPaga = new myRaiServiceHub.it.rai.servizi.hrpaga.HrPaga();
            hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
            return hrPaga.ByteDocPersonali(id, matricola, "");
        }

        private static void impostaAbilitazioneDocumenti(AnagraficaModel model, string matricola)
        {
            bool visibile = true;
            var prmHRDOC = CommonHelper.GetParametri<string>(EnumParametriSistema.HRDOC);
            if (prmHRDOC != null && prmHRDOC.Any())
                visibile = prmHRDOC[0].Contains(UtenteHelper.Matricola());
            if (visibile)
            {
                model.DatiDocumenti.AbilitatoPerDocumentiPersonali = AuthHelper.EnabledTo(matricola, "HRDOC");
                AbilFunc funzioniAbilitate = new AbilFunc();
                bool abil = AuthHelper.EnabledTo(matricola, "HRPAGA", out funzioniAbilitate);
                List<string> listaFunzioni = funzioniAbilitate.SubFuncs.Select(s => s.Key).ToList();
                model.DatiDocumenti.AbilitatoPerDocumentiAmministrativi = listaFunzioni.Contains("UAD") || listaFunzioni.Contains("UP") || listaFunzioni.Contains("MOD730");
                model.DatiDocumenti.IsEnabled = model.DatiDocumenti.AbilitatoPerDocumentiPersonali || model.DatiDocumenti.AbilitatoPerDocumentiAmministrativi;
            }
            else
            {
                model.DatiDocumenti.IsEnabled = false;
            }
        }

        private static void CaricaDocumentiPersonalieAmministrativi(AnagraficaModel anag)
        {
            string m = anag.Matricola; 
            string mLoggato = CommonHelper.GetCurrentUserMatricola();
            impostaAbilitazioneDocumenti(anag, m);

            //carica la lista dei tipi di cosumenti amministrativi
            if (anag.DatiDocumenti.AbilitatoPerDocumentiAmministrativi)
            {
                myRaiServiceHub.it.rai.servizi.hrpaga.HrPaga hrPaga = new myRaiServiceHub.it.rai.servizi.hrpaga.HrPaga();
                hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
                myRaiServiceHub.it.rai.servizi.hrpaga.ListaNavBar lista = hrPaga.ElencoMenuBar(m);
                anag.DatiDocumenti.TipiDocumentiAmministrativi = new SelectList(lista.Elementi.Select(l => new { id = l.Split('|')[0], testo = l.Split('|')[1] }).ToList(), "id", "testo");

                List<SelectListItem> listaAnni = new List<SelectListItem>();
                for (int i = 0; i < 15; i++)
                {
                    string anno = (DateTime.Today.Year - i).ToString();
                    listaAnni.Add(new SelectListItem() { Value = anno, Text = anno });
                }
                anag.DatiDocumenti.AnniDocumentiAmministrativi = new SelectList(listaAnni, "Value", "Text", DateTime.Today.Year.ToString());
            }

            //carica la lista dei tipi di documenti personali
            if (anag.DatiDocumenti.AbilitatoPerDocumentiPersonali)
            {
                myRaiServiceHub.it.rai.servizi.hrgb.Service srvHrgb = new myRaiServiceHub.it.rai.servizi.hrgb.Service();
                //srvHrgb.Credentials = System.Net.CredentialCache.DefaultCredentials;

                var listaTipiDoc = srvHrgb.Get_HrDoc_TipologiaDoc("P" + mLoggato);
                List<SelectListItem> listaTipiDocumentoPersonali = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in listaTipiDoc.DT_HrDoc_TipologiaDocMatricola.Rows)
                    listaTipiDocumentoPersonali.Add(new SelectListItem() { Value = dr[0].ToString(), Text = string.Format("{0} - {1}", dr[0], dr[1]) });
                anag.DatiDocumenti.TipiDocumentiPersonali = new SelectList(listaTipiDocumentoPersonali, "Value", "Text");
            }
        }
        #endregion

    }
}