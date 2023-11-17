using myRai.DataAccess;
using myRaiData;
//using myRaiDataTalentia;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace myRaiCommonManager
{
    public class AbilitazioniPers
    {
        public string Matricola { get; set; }
        public List<XR_HRIS_ABIL> Abilitazioni { get; set; }
    }

    public class RegolaVoceMenu
    {
        public XR_HRIS_REGOLE_VOCI_MENU Regola { get; set; }
        public MyRai_Voci_Menu VoceMenu { get; set; }
    }

    public class AbilModello : XR_HRIS_ABIL_MODELLO
    {
        public string[] ARY_GR_CATEGORIE { get; set; }
        public string[] ARY_GR_AREA { get; set; }
        public string[] ARY_DIR_INCLUSE { get; set; }
        public string[] ARY_DIR_ESCLUSE { get; set; }
        public string[] ARY_CAT_INCLUSE { get; set; }
        public string[] ARY_CAT_ESCLUSE { get; set; }
        public string[] ARY_SEDI_INCLUSE { get; set; }
        public string[] ARY_SEDI_ESCLUSE { get; set; }
        public string[] ARY_TIP_INCLUSE { get; set; }
        public string[] ARY_TIP_ESCLUSE { get; set; }
        public string[] ARY_SOC_INCLUSE { get; set; }
        public string[] ARY_SOC_ESCLUSE { get; set; }
        public string[] ARY_CNT_INCLUSI { get; set; }
        public string[] ARY_CNT_ESCLUSI { get; set; }
    }

    public class AbilAbilitazione : XR_HRIS_ABIL
    {
        public string[] ARY_MODELLI { get; set; }
        public string[] ARY_GR_CATEGORIE { get; set; }
        public string[] ARY_GR_AREA { get; set; }
        public string[] ARY_DIR_INCLUSE { get; set; }
        public string[] ARY_DIR_ESCLUSE { get; set; }
        public string[] ARY_CAT_INCLUSE { get; set; }
        public string[] ARY_CAT_ESCLUSE { get; set; }
        public string[] ARY_SEDI_INCLUSE { get; set; }
        public string[] ARY_SEDI_ESCLUSE { get; set; }
        public string[] ARY_TIP_INCLUSE { get; set; }
        public string[] ARY_TIP_ESCLUSE { get; set; }
        public string[] ARY_SOC_INCLUSE { get; set; }
        public string[] ARY_SOC_ESCLUSE { get; set; }
        public string[] ARY_CNT_INCLUSI { get; set; }
        public string[] ARY_CNT_ESCLUSI { get; set; }
    }

    public class AbilitazioniManager
    {
        public static List<XR_HRIS_ABIL_PROFILO> GetProfili()
        {
            IncentiviEntities db = new IncentiviEntities();
            List<XR_HRIS_ABIL_PROFILO> list = db.XR_HRIS_ABIL_PROFILO.ToList();
            return list;
        }
        public static XR_HRIS_ABIL_PROFILO GetProfilo(int idProfilo)
        {
            IncentiviEntities db = new IncentiviEntities();

            XR_HRIS_ABIL_PROFILO model = null;
            if (idProfilo != 0)
                model = db.XR_HRIS_ABIL_PROFILO.Find(idProfilo);
            else
                model = new XR_HRIS_ABIL_PROFILO() { IND_ATTIVO = true };

            return model;
        }
        public static bool SaveProfilo(XR_HRIS_ABIL_PROFILO model, out int idProfilo, out string errorMsg)
        {
            bool result = false;
            idProfilo = 0;
            errorMsg = "";

            IncentiviEntities db = new IncentiviEntities();
            XR_HRIS_ABIL_PROFILO dbrec = null;
            if (model.ID_PROFILO == 0)
                dbrec = new XR_HRIS_ABIL_PROFILO();
            else
                dbrec = db.XR_HRIS_ABIL_PROFILO.Find(model.ID_PROFILO);

            dbrec.COD_PROFILO = model.COD_PROFILO;
            dbrec.DES_PROFILO = model.DES_PROFILO;
            dbrec.IND_ATTIVO = model.IND_ATTIVO;

            if (model.ID_PROFILO == 0)
                db.XR_HRIS_ABIL_PROFILO.Add(dbrec);

            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "ADD PROFILO Abilitazioni");
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            idProfilo = dbrec.ID_PROFILO;

            return result;
        }
        public static bool SaveProfiloAssoc(int idProfilo, List<int> subFunc, List<int> profili, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            IncentiviEntities db = new IncentiviEntities();
            XR_HRIS_ABIL_PROFILO profilo = db.XR_HRIS_ABIL_PROFILO.Find(idProfilo);

            foreach (var item in subFunc.Where(x => !db.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Any(y => y.ID_PROFILO == idProfilo && y.ID_PROFILO_SUB == null && y.ID_SUBFUNZ == x)))
            {
                XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI newAssoc = new XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI()
                {
                    ID_PROFILO = idProfilo,
                    ID_PROFILO_SUB = null,
                    ID_SUBFUNZ = item,
                    IND_ATTIVO = true
                };
                db.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Add(newAssoc);
            }
            foreach (var item in db.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Where(x => x.ID_PROFILO == idProfilo && x.ID_PROFILO_SUB == null && !subFunc.Any(y => y == x.ID_SUBFUNZ)))
            {
                db.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Remove(item);
            }

            foreach (var item in profili.Where(x => !db.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Any(y => y.ID_PROFILO == idProfilo && y.ID_SUBFUNZ == null && y.ID_PROFILO_SUB == x)))
            {
                XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI newAssoc = new XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI()
                {
                    ID_PROFILO = idProfilo,
                    ID_PROFILO_SUB = item,
                    ID_SUBFUNZ = null,
                    IND_ATTIVO = true
                };
                db.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Add(newAssoc);
            }
            foreach (var item in db.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Where(x => x.ID_PROFILO == idProfilo && x.ID_SUBFUNZ == null && !profili.Any(y => y == x.ID_PROFILO_SUB)))
            {
                db.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Remove(item);
            }

            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "ADD PROFILO Assoc");
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            return result;
        }



        public static List<XR_HRIS_ABIL_FUNZIONE> GetFunzioni()
        {
            IncentiviEntities db = new IncentiviEntities();

            List<XR_HRIS_ABIL_FUNZIONE> list = db.XR_HRIS_ABIL_FUNZIONE.ToList();

            return list;
        }
        public static XR_HRIS_ABIL_FUNZIONE GetFunzione(int idFunz)
        {
            IncentiviEntities db = new IncentiviEntities();

            XR_HRIS_ABIL_FUNZIONE model = null;
            if (idFunz != 0)
                model = db.XR_HRIS_ABIL_FUNZIONE.Find(idFunz);
            else
                model = new XR_HRIS_ABIL_FUNZIONE() { IND_ATTIVO = true };

            return model;
        }
        public static bool SaveFunzione(XR_HRIS_ABIL_FUNZIONE model, out int idFunz, out string errorMsg)
        {
            bool result = false;
            idFunz = 0;
            errorMsg = "";

            IncentiviEntities db = new IncentiviEntities();
            XR_HRIS_ABIL_FUNZIONE dbrec = null;
            if (model.ID_FUNZIONE == 0)
                dbrec = new XR_HRIS_ABIL_FUNZIONE();
            else
                dbrec = db.XR_HRIS_ABIL_FUNZIONE.Find(model.ID_FUNZIONE);

            dbrec.COD_FUNZIONE = model.COD_FUNZIONE;
            dbrec.DES_FUNZIONE = model.DES_FUNZIONE;
            dbrec.NMB_PROVENIENZA = model.NMB_PROVENIENZA;
            dbrec.IND_ABIL_INTEGRATION = model.IND_ABIL_INTEGRATION;
            dbrec.IND_FILTER_INTEGRATION = model.IND_FILTER_INTEGRATION;
            dbrec.IND_ATTIVO = model.IND_ATTIVO;

            if (model.ID_FUNZIONE == 0)
                db.XR_HRIS_ABIL_FUNZIONE.Add(dbrec);

            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "ADD FUNZ Abilitazioni");
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            idFunz = dbrec.ID_FUNZIONE;

            return result;
        }

        public static XR_HRIS_ABIL_SUBFUNZIONE GetSottofunzione(int idFunz, int idSubFunz)
        {
            IncentiviEntities db = new IncentiviEntities();

            XR_HRIS_ABIL_SUBFUNZIONE model = null;
            if (idSubFunz != 0)
                model = db.XR_HRIS_ABIL_SUBFUNZIONE.Find(idSubFunz);
            else
            {
                model = new XR_HRIS_ABIL_SUBFUNZIONE();
                model.ID_FUNZIONE = idFunz;
                model.XR_HRIS_ABIL_FUNZIONE = db.XR_HRIS_ABIL_FUNZIONE.Find(idFunz);
                model.IND_ATTIVO = true;
                model.IND_CREATE = true;
                model.IND_READ = true;
                model.IND_UPDATE = true;
                model.IND_DELETE = true;
            }

            return model;
        }

        public static bool SaveSottofunzione(XR_HRIS_ABIL_SUBFUNZIONE model, out int idSubFunz, out string errorMsg)
        {
            bool result = false;
            idSubFunz = 0;
            errorMsg = "";

            IncentiviEntities db = new IncentiviEntities();
            XR_HRIS_ABIL_SUBFUNZIONE dbrec = null;
            if (model.ID_SUBFUNZ == 0)
                dbrec = new XR_HRIS_ABIL_SUBFUNZIONE();
            else
                dbrec = db.XR_HRIS_ABIL_SUBFUNZIONE.Find(model.ID_SUBFUNZ);

            dbrec.COD_SUBFUNZIONE = model.COD_SUBFUNZIONE;
            dbrec.DES_SUBFUNZIONE = model.DES_SUBFUNZIONE;
            dbrec.IND_CREATE = model.IND_CREATE;
            dbrec.IND_READ = model.IND_READ;
            dbrec.IND_UPDATE = model.IND_UPDATE;
            dbrec.IND_DELETE = model.IND_DELETE;
            dbrec.IND_ATTIVO = model.IND_ATTIVO;
            dbrec.IND_NOFILTERS = model.IND_NOFILTERS;
            dbrec.NOT_UFFICIO = model.NOT_UFFICIO;

            if (model.ID_SUBFUNZ == 0)
            {
                dbrec.XR_HRIS_ABIL_FUNZIONE = db.XR_HRIS_ABIL_FUNZIONE.Find(model.ID_FUNZIONE);
                db.XR_HRIS_ABIL_SUBFUNZIONE.Add(dbrec);
            }

            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "ADD SUB Abilitazioni");
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            idSubFunz = dbrec.ID_SUBFUNZ;

            return result;
        }

        public static bool DeleteSottofunzione(int idSubFunz, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            IncentiviEntities db = new IncentiviEntities();
            var subFunz = db.XR_HRIS_ABIL_SUBFUNZIONE.Find(idSubFunz);
            db.XR_HRIS_ABIL_SUBFUNZIONE.Remove(subFunz);
            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "REMOVE SUB Abilitazioni");
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            return result;
        }

        public static AbilitazioniPers GetPersAbil(string matricola)
        {
            IncentiviEntities db = new IncentiviEntities();
            AbilitazioniPers model = new AbilitazioniPers();
            model.Matricola = matricola;
            model.Abilitazioni = db.XR_HRIS_ABIL.Where(x => x.MATRICOLA == matricola).ToList();

            return model;
        }

        public static AbilAbilitazione GetAbilPers(int idAbil, int? idSubfunz, int? idProfilo, int? idModello, string matricola)
        {
            IncentiviEntities db = new IncentiviEntities();

            XR_HRIS_ABIL dbModel = null;
            if (idAbil != 0)
                dbModel = db.XR_HRIS_ABIL.Find(idAbil);
            else
            {
                dbModel = new XR_HRIS_ABIL();
                dbModel.ID_SUBFUNZ = idSubfunz;
                dbModel.ID_PROFILO = idProfilo;
                //dbModel.XR_HRIS_ABIL_SUBFUNZIONE = db.XR_HRIS_ABIL_SUBFUNZIONE.Find(idSubfunz);
                dbModel.IND_ATTIVO = true;
                dbModel.MATRICOLA = matricola;
            }

            AbilAbilitazione model = dbModel.CopyProperty<XR_HRIS_ABIL, AbilAbilitazione>();
            if (db.XR_HRIS_ABIL_ASSOC_MODELLO != null && db.XR_HRIS_ABIL_ASSOC_MODELLO.Any())
                model.ARY_MODELLI = model.XR_HRIS_ABIL_ASSOC_MODELLO.Select(x => x.ID_MODELLO.ToString()).ToArray();
            else if (model.ID_MODELLO.HasValue)
                model.ARY_MODELLI = new string[] { model.ID_MODELLO.Value.ToString() };

            if (idModello != null)
            {
                if (model.ARY_MODELLI != null)
                {
                    if (!model.ARY_MODELLI.Contains(idModello.Value.ToString()))
                        model.ARY_MODELLI = model.ARY_MODELLI.Concat(new string[] { idModello.Value.ToString() }).ToArray();
                }
                else
                    model.ARY_MODELLI = new string[] { idModello.Value.ToString() };
            }

            model.ARY_GR_CATEGORIE = String.IsNullOrWhiteSpace(model.GR_CATEGORIE) ? new string[] { } : model.GR_CATEGORIE.Split(',');
            model.ARY_GR_AREA = String.IsNullOrWhiteSpace(model.GR_AREA) ? new string[] { } : model.GR_AREA.Split(',');
            model.ARY_DIR_INCLUSE = String.IsNullOrWhiteSpace(model.DIR_INCLUSE) ? new string[] { } : model.DIR_INCLUSE.Split(',');
            model.ARY_DIR_ESCLUSE = String.IsNullOrWhiteSpace(model.DIR_ESCLUSE) ? new string[] { } : model.DIR_ESCLUSE.Split(',');
            model.ARY_CAT_INCLUSE = String.IsNullOrWhiteSpace(model.CAT_INCLUSE) ? new string[] { } : model.CAT_INCLUSE.Split(',');
            model.ARY_CAT_ESCLUSE = String.IsNullOrWhiteSpace(model.CAT_ESCLUSE) ? new string[] { } : model.CAT_ESCLUSE.Split(',');
            //model.ARY_SEDI_INCLUSE = String.IsNullOrWhiteSpace(model.SEDI_INCLUSE) ? new string[] { } : model.SEDI_INCLUSE.Split(',');
            //model.ARY_SEDI_ESCLUSE = String.IsNullOrWhiteSpace(model.SEDI_ESCLUSE) ? new string[] { } : model.SEDI_ESCLUSE.Split(',');
            model.ARY_TIP_INCLUSE = String.IsNullOrWhiteSpace(model.TIP_INCLUSE) ? new string[] { } : model.TIP_INCLUSE.Split(',');
            model.ARY_TIP_ESCLUSE = String.IsNullOrWhiteSpace(model.TIP_ESCLUSE) ? new string[] { } : model.TIP_ESCLUSE.Split(',');
            model.ARY_SOC_INCLUSE = String.IsNullOrWhiteSpace(model.SOC_INCLUSE) ? new string[] { } : model.SOC_INCLUSE.Split(',');
            model.ARY_SOC_ESCLUSE = String.IsNullOrWhiteSpace(model.SOC_ESCLUSE) ? new string[] { } : model.SOC_ESCLUSE.Split(',');
            model.ARY_CNT_INCLUSI = String.IsNullOrWhiteSpace(model.CONTR_INCLUSI) ? new string[] { } : model.CONTR_INCLUSI.Split(',');
            model.ARY_CNT_ESCLUSI = String.IsNullOrWhiteSpace(model.CONTR_ESCLUSI) ? new string[] { } : model.CONTR_ESCLUSI.Split(',');

            return model;
        }
        public static bool SaveAbilPers(AbilAbilitazione model, out int idAbil, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";
            idAbil = 0;

            var db = new IncentiviEntities();
            XR_HRIS_ABIL dbRec = null;
            if (model.ID_ABIL == 0)
                dbRec = new XR_HRIS_ABIL();
            else
                dbRec = db.XR_HRIS_ABIL.Find(model.ID_ABIL);

            dbRec.GR_CATEGORIE = model.ARY_GR_CATEGORIE != null ? String.Join(",", model.ARY_GR_CATEGORIE) : null;
            dbRec.GR_AREA = model.ARY_GR_AREA != null ? String.Join(",", model.ARY_GR_AREA) : null;
            dbRec.CAT_ESCLUSE = model.ARY_CAT_ESCLUSE != null ? String.Join(",", model.ARY_CAT_ESCLUSE) : null;
            dbRec.CAT_INCLUSE = model.ARY_CAT_INCLUSE != null ? String.Join(",", model.ARY_CAT_INCLUSE) : null;
            dbRec.DIR_ESCLUSE = model.ARY_DIR_ESCLUSE != null ? String.Join(",", model.ARY_DIR_ESCLUSE) : null;
            dbRec.DIR_INCLUSE = model.ARY_DIR_INCLUSE != null ? String.Join(",", model.ARY_DIR_INCLUSE) : null;
            //dbRec.SEDI_ESCLUSE = model.ARY_SEDI_ESCLUSE != null ? String.Join(",", model.ARY_SEDI_ESCLUSE) : null;
            //dbRec.SEDI_INCLUSE = model.ARY_SEDI_INCLUSE != null ? String.Join(",", model.ARY_SEDI_INCLUSE) : null;
            dbRec.SEDI_ESCLUSE = model.SEDI_ESCLUSE;
            dbRec.SEDI_INCLUSE = model.SEDI_INCLUSE;
            dbRec.TIP_ESCLUSE = model.ARY_TIP_ESCLUSE != null ? String.Join(",", model.ARY_TIP_ESCLUSE) : null;
            dbRec.TIP_INCLUSE = model.ARY_TIP_INCLUSE != null ? String.Join(",", model.ARY_TIP_INCLUSE) : null;
            dbRec.SOC_ESCLUSE = model.ARY_SOC_ESCLUSE != null ? String.Join(",", model.ARY_SOC_ESCLUSE) : null;
            dbRec.SOC_INCLUSE = model.ARY_SOC_INCLUSE != null ? String.Join(",", model.ARY_SOC_INCLUSE) : null;
            dbRec.CONTR_INCLUSI = model.ARY_CNT_INCLUSI != null ? String.Join(",", model.ARY_CNT_INCLUSI) : null;
            dbRec.CONTR_ESCLUSI = model.ARY_CNT_ESCLUSI != null ? String.Join(",", model.ARY_CNT_ESCLUSI) : null;
            dbRec.MATR_INCLUSE = model.MATR_INCLUSE;
            dbRec.MATR_ESCLUSE = model.MATR_ESCLUSE;
            dbRec.TIP_INCLUSE = model.TIP_INCLUSE;
            dbRec.TIP_ESCLUSE = model.TIP_ESCLUSE;
            dbRec.COD_CONDITION = model.COD_CONDITION;
            dbRec.DTA_INIZIO = model.DTA_INIZIO;
            dbRec.DTA_FINE = model.DTA_FINE;
            dbRec.MATRICOLA_DELEGANTE = model.MATRICOLA_DELEGANTE;
            dbRec.ID_DELEGA = model.ID_DELEGA;
            dbRec.TIP_DOC_INCLUSI = model.TIP_DOC_INCLUSI;
            dbRec.TIP_DOC_ESCLUSI = model.TIP_DOC_ESCLUSI;

            int[] modelIds = model.ARY_MODELLI != null ? model.ARY_MODELLI.Select(x => Convert.ToInt32(x)).ToArray() : new int[] { };
            foreach (var item in modelIds.Where(x => !dbRec.XR_HRIS_ABIL_ASSOC_MODELLO.Any(y => y.ID_MODELLO == x)))
            {
                dbRec.XR_HRIS_ABIL_ASSOC_MODELLO.Add(new XR_HRIS_ABIL_ASSOC_MODELLO()
                {
                    ID_MODELLO = item
                });
            }
            var list = dbRec.XR_HRIS_ABIL_ASSOC_MODELLO.Where(x => !modelIds.Contains(x.ID_MODELLO)).ToList();
            foreach (var item in list)
                db.XR_HRIS_ABIL_ASSOC_MODELLO.Remove(item);

            dbRec.ID_MODELLO = null;
            dbRec.IND_ATTIVO = model.IND_ATTIVO;

            if (model.ID_ABIL == 0)
            {
                dbRec.MATRICOLA = model.MATRICOLA;
                if (model.ID_SUBFUNZ.HasValue)
                    dbRec.XR_HRIS_ABIL_SUBFUNZIONE = db.XR_HRIS_ABIL_SUBFUNZIONE.Find(model.ID_SUBFUNZ);
                else if (model.ID_PROFILO.HasValue)
                    dbRec.XR_HRIS_ABIL_PROFILO = db.XR_HRIS_ABIL_PROFILO.Find(model.ID_PROFILO);
                db.XR_HRIS_ABIL.Add(dbRec);
            }

            if (model.ID_PROFILO == null && model.ID_SUBFUNZ == null && !modelIds.Any())
                db.XR_HRIS_ABIL.Remove(dbRec);

            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "ADD ABIL PERS Abilitazioni");
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            if (dbRec != null)
                idAbil = dbRec.ID_ABIL;

            return result;
        }

        public static bool DeleteAbilitazione(int idAbil, int? idModello, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            var abil = db.XR_HRIS_ABIL.Find(idAbil);
            if (abil != null)
            {
                if (!idModello.HasValue)
                    db.XR_HRIS_ABIL.Remove(abil);
                else if (abil.XR_HRIS_ABIL_ASSOC_MODELLO != null)
                {
                    var abilMod = abil.XR_HRIS_ABIL_ASSOC_MODELLO.FirstOrDefault(x => x.ID_MODELLO == idModello);
                    if (abilMod != null)
                        db.XR_HRIS_ABIL_ASSOC_MODELLO.Remove(abilMod);

                    if (!abil.XR_HRIS_ABIL_ASSOC_MODELLO.Any(x => x.ID_MODELLO != idModello))
                        db.XR_HRIS_ABIL.Remove(abil);
                }

                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "DEL Abilitazioni");
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
            }
            else
                errorMsg = "Abilitazione non trovata";

            return result;
        }


        public static List<RegolaVoceMenu> GetRegole()
        {
            IncentiviEntities db = new IncentiviEntities();

            List<RegolaVoceMenu> list = db.XR_HRIS_REGOLE_VOCI_MENU.Select(x => new RegolaVoceMenu() { Regola = x }).ToList();
            using (var dbDigi = new digiGappEntities())
            {
                foreach (var item in list)
                {
                    item.VoceMenu = dbDigi.MyRai_Voci_Menu.Find(item.Regola.ID_VOCE_MENU);
                }
            }

            return list;
        }
        public static RegolaVoceMenu GetRegola(int idRegola)
        {
            IncentiviEntities db = new IncentiviEntities();

            RegolaVoceMenu model = null;
            if (idRegola != 0)
                model = new RegolaVoceMenu() { Regola = db.XR_HRIS_REGOLE_VOCI_MENU.Find(idRegola) };
            else
                model = new RegolaVoceMenu() { Regola = new XR_HRIS_REGOLE_VOCI_MENU() };

            return model;
        }
        public static bool SaveRegola(RegolaVoceMenu model, out int idRegola, out string errorMsg)
        {
            bool result = false;
            idRegola = 0;
            errorMsg = "";

            IncentiviEntities db = new IncentiviEntities();
            XR_HRIS_REGOLE_VOCI_MENU dbrec = null;
            if (model.Regola.ID_REGOLA == 0)
                dbrec = new XR_HRIS_REGOLE_VOCI_MENU();
            else
                dbrec = db.XR_HRIS_REGOLE_VOCI_MENU.Find(model.Regola.ID_REGOLA);

            dbrec.ID_VOCE_MENU = model.Regola.ID_VOCE_MENU;
            dbrec.CONTESTO = model.Regola.CONTESTO;
            dbrec.TIPO_REGOLA = model.Regola.TIPO_REGOLA;
            dbrec.LST_MATR_EXCL = model.Regola.LST_MATR_EXCL;
            dbrec.LST_MATR_INCL = model.Regola.LST_MATR_INCL;
            dbrec.ABIL_FUNC = model.Regola.ABIL_FUNC;
            dbrec.ABIL_SUBFUNC = model.Regola.ABIL_SUBFUNC;
            dbrec.CODE = model.Regola.CODE;

            if (model.Regola.ID_REGOLA == 0)
                db.XR_HRIS_REGOLE_VOCI_MENU.Add(dbrec);

            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "ADD RULE Abilitazioni");
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            idRegola = dbrec.ID_REGOLA;

            return result;
        }
        public static bool DeleteRegola(int idRegola, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            var db = new IncentiviEntities();
            var abil = db.XR_HRIS_REGOLE_VOCI_MENU.Find(idRegola);
            if (abil != null)
            {
                db.XR_HRIS_REGOLE_VOCI_MENU.Remove(abil);
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "DEL Abilitazioni");
                if (!result)
                    errorMsg = "Errore durante il salvataggio";
            }
            else
                errorMsg = "Regola non trovata";

            return result;
        }
        public static List<SelectListItem> GetVociMenu()
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var db = new digiGappEntities();
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            result.AddRange(db.MyRai_Voci_Menu
                                .Select(x => new { Value = x.ID, Text = x.Titolo + " - " + x.codiceMy + " - " + x.customView })
                                .ToList()
                                .Select(x => new SelectListItem() { Value = x.Value.ToString(), Text = x.Value.ToString() + " - " + x.Text }));

            return result;
        }

        public static List<XR_HRIS_ABIL_MODELLO> GetModelli()
        {
            IncentiviEntities db = new IncentiviEntities();
            List<XR_HRIS_ABIL_MODELLO> list = db.XR_HRIS_ABIL_MODELLO.ToList();
            return list;
        }
        public static AbilModello GetModello(int idModello)
        {
            IncentiviEntities db = new IncentiviEntities();

            XR_HRIS_ABIL_MODELLO dbModel = null;
            if (idModello != 0)
                dbModel = db.XR_HRIS_ABIL_MODELLO.Find(idModello);
            else
                dbModel = new XR_HRIS_ABIL_MODELLO();

            AbilModello model = dbModel.CopyProperty<XR_HRIS_ABIL_MODELLO, AbilModello>();
            model.ARY_GR_CATEGORIE = String.IsNullOrWhiteSpace(model.GR_CATEGORIE) ? new string[] { } : model.GR_CATEGORIE.Split(',');
            model.ARY_GR_AREA = String.IsNullOrWhiteSpace(model.GR_AREA) ? new string[] { } : model.GR_AREA.Split(',');
            model.ARY_DIR_INCLUSE = String.IsNullOrWhiteSpace(model.DIR_INCLUSE) ? new string[] { } : model.DIR_INCLUSE.Split(',');
            model.ARY_DIR_ESCLUSE = String.IsNullOrWhiteSpace(model.DIR_ESCLUSE) ? new string[] { } : model.DIR_ESCLUSE.Split(',');
            model.ARY_CAT_INCLUSE = String.IsNullOrWhiteSpace(model.CAT_INCLUSE) ? new string[] { } : model.CAT_INCLUSE.Split(',');
            model.ARY_CAT_ESCLUSE = String.IsNullOrWhiteSpace(model.CAT_ESCLUSE) ? new string[] { } : model.CAT_ESCLUSE.Split(',');
            //model.ARY_SEDI_INCLUSE = String.IsNullOrWhiteSpace(model.SEDI_INCLUSE) ? new string[] { } : model.SEDI_INCLUSE.Split(',');
            //model.ARY_SEDI_ESCLUSE = String.IsNullOrWhiteSpace(model.SEDI_ESCLUSE) ? new string[] { } : model.SEDI_ESCLUSE.Split(',');
            model.ARY_TIP_INCLUSE = String.IsNullOrWhiteSpace(model.TIP_INCLUSE) ? new string[] { } : model.TIP_INCLUSE.Split(',');
            model.ARY_TIP_ESCLUSE = String.IsNullOrWhiteSpace(model.TIP_ESCLUSE) ? new string[] { } : model.TIP_ESCLUSE.Split(',');
            model.ARY_SOC_INCLUSE = String.IsNullOrWhiteSpace(model.SOC_INCLUSE) ? new string[] { } : model.SOC_INCLUSE.Split(',');
            model.ARY_SOC_ESCLUSE = String.IsNullOrWhiteSpace(model.SOC_ESCLUSE) ? new string[] { } : model.SOC_ESCLUSE.Split(',');
            model.ARY_CNT_INCLUSI = String.IsNullOrWhiteSpace(model.CONTR_INCLUSI) ? new string[] { } : model.CONTR_INCLUSI.Split(',');
            model.ARY_CNT_ESCLUSI = String.IsNullOrWhiteSpace(model.CONTR_ESCLUSI) ? new string[] { } : model.CONTR_ESCLUSI.Split(',');

            if (idModello != 0)
            {
                List<XR_HRIS_ABIL> abil = new List<XR_HRIS_ABIL>();
                abil.AddRange(model.XR_HRIS_ABIL);
                if (model.XR_HRIS_ABIL_ASSOC_MODELLO != null && model.XR_HRIS_ABIL_ASSOC_MODELLO.Any(x => x.XR_HRIS_ABIL != null))// && x.XR_HRIS_ABIL.ID_SUBFUNZ == null && x.XR_HRIS_ABIL.ID_PROFILO == null))
                    abil.AddRange(model.XR_HRIS_ABIL_ASSOC_MODELLO.Where(x => x.XR_HRIS_ABIL != null /*&& x.XR_HRIS_ABIL.ID_SUBFUNZ == null && x.XR_HRIS_ABIL.ID_PROFILO == null*/).Select(x => x.XR_HRIS_ABIL));

                foreach (var item in abil)
                    item.ID_MODELLO = idModello;

                model.XR_HRIS_ABIL = abil;
            }

            return model;
        }
        public static bool SaveModello(AbilModello model, out int idModello, out string errorMsg)
        {
            bool result = false;
            idModello = 0;
            errorMsg = "";

            IncentiviEntities db = new IncentiviEntities();
            XR_HRIS_ABIL_MODELLO dbrec = null;
            if (model.ID_MODELLO == 0)
                dbrec = new XR_HRIS_ABIL_MODELLO();
            else
                dbrec = db.XR_HRIS_ABIL_MODELLO.Find(model.ID_MODELLO);

            dbrec.CODICE = model.CODICE;
            dbrec.GR_CATEGORIE = model.ARY_GR_CATEGORIE != null ? String.Join(",", model.ARY_GR_CATEGORIE) : null;
            dbrec.GR_AREA = model.ARY_GR_AREA != null ? String.Join(",", model.ARY_GR_AREA) : null;
            dbrec.CAT_ESCLUSE = model.ARY_CAT_ESCLUSE != null ? String.Join(",", model.ARY_CAT_ESCLUSE) : null;
            dbrec.CAT_INCLUSE = model.ARY_CAT_INCLUSE != null ? String.Join(",", model.ARY_CAT_INCLUSE) : null;
            dbrec.DIR_ESCLUSE = model.ARY_DIR_ESCLUSE != null ? String.Join(",", model.ARY_DIR_ESCLUSE) : null;
            dbrec.DIR_INCLUSE = model.ARY_DIR_INCLUSE != null ? String.Join(",", model.ARY_DIR_INCLUSE) : null;
            //dbrec.SEDI_ESCLUSE = model.ARY_SEDI_ESCLUSE != null ? String.Join(",", model.ARY_SEDI_ESCLUSE) : null;
            //dbrec.SEDI_INCLUSE = model.ARY_SEDI_INCLUSE != null ? String.Join(",", model.ARY_SEDI_INCLUSE) : null;
            dbrec.SEDI_ESCLUSE = model.SEDI_ESCLUSE;
            dbrec.SEDI_INCLUSE = model.SEDI_INCLUSE;
            dbrec.TIP_ESCLUSE = model.ARY_TIP_ESCLUSE != null ? String.Join(",", model.ARY_TIP_ESCLUSE) : null;
            dbrec.TIP_INCLUSE = model.ARY_TIP_INCLUSE != null ? String.Join(",", model.ARY_TIP_INCLUSE) : null;
            dbrec.SOC_ESCLUSE = model.ARY_SOC_ESCLUSE != null ? String.Join(",", model.ARY_SOC_ESCLUSE) : null;
            dbrec.SOC_INCLUSE = model.ARY_SOC_INCLUSE != null ? String.Join(",", model.ARY_SOC_INCLUSE) : null;
            dbrec.CONTR_INCLUSI = model.ARY_CNT_INCLUSI != null ? String.Join(",", model.ARY_CNT_INCLUSI) : null;
            dbrec.CONTR_ESCLUSI = model.ARY_CNT_ESCLUSI != null ? String.Join(",", model.ARY_CNT_ESCLUSI) : null;
            dbrec.MATR_INCLUSE = model.MATR_INCLUSE;
            dbrec.MATR_ESCLUSE = model.MATR_ESCLUSE;
            dbrec.TIP_INCLUSE = model.TIP_INCLUSE;
            dbrec.TIP_ESCLUSE = model.TIP_ESCLUSE;
            dbrec.IND_ATTIVO = model.IND_ATTIVO;

            if (model.ID_MODELLO == 0)
                db.XR_HRIS_ABIL_MODELLO.Add(dbrec);

            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "ADD RULE Abilitazioni");
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            idModello = dbrec.ID_MODELLO;

            return result;
        }

        public static List<SelectListItem> GetListModelli()
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var db = new IncentiviEntities();
            result.Add(new SelectListItem() { Value = null, Text = "Nessun profilo" });
            result.AddRange(db.XR_HRIS_ABIL_MODELLO
                                .Select(x => new { Value = x.ID_MODELLO, Text = x.CODICE })
                                .ToList()
                                .Select(x => new SelectListItem() { Value = x.Value.ToString(), Text = x.Value.ToString() + " - " + x.Text }));

            return result;
        }
    }
}
