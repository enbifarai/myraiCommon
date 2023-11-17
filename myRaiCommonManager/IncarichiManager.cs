using myRaiCommonDatacontrollers;
using myRaiCommonModel.Gestionale;
using myRaiData;
using myRaiData.Incentivi;
using myRaiDataTalentia;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace myRaiCommonManager
{
    public class IncarichiManager
    {
        public static string ChangeDB(string orig)
        {
            HttpContext.Current.Session["DB"] = orig;
            return null;
        }
        public static string GetDB()
        {
            if (HttpContext.Current.Session["DB"] == null)
            {
                HttpContext.Current.Session["DB"] = "P";
            }

            return HttpContext.Current.Session["DB"].ToString();
        }
        public static myRaiDataTalentia.TalentiaEntities GetIncarichiDBContext()
        {
            string DBorigine = GetDB();
            if (DBorigine == "P")
                return new myRaiDataTalentia.TalentiaEntities("TalentiaEntities");
            else
                return new myRaiDataTalentia.TalentiaEntities("TalentiaEntities_IP");
        }

        public static string GetCategoria(string cat)
        {
            var db = GetIncarichiDBContext();
            var c = db.QUALIFICA.Where(x => x.COD_QUALIFICA.Trim() == cat.Trim()).FirstOrDefault();
            if (c == null) return null;
            else return c.DES_QUALIFICA.Trim();
        }

        public static string Riversa()
        {
            try
            {
                return CopyIpotesiproduzione();
            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new MyRai_LogErrori() { error_message = ex.ToString() });
                return "Errore nella replica dei dati";
            }

        }

        public static List<myRaiDataTalentia.XR_STR_DINCARICO> getIncarichiAll()
        {
            var db = GetIncarichiDBContext();
            return db.XR_STR_DINCARICO.OrderBy(x => x.DES_INCARICO).ToList();
        }

        public static void AddPeso(GestioneSezioneModel model)
        {
            string matr = myRaiHelper.CommonHelper.GetCurrentUserMatricola();
            var db = GetIncarichiDBContext();
            XR_STR_PESO_SEZIONE peso = new XR_STR_PESO_SEZIONE()
            {
                data_inizio_validita = DateTime.Now.ToString("yyyyMMdd"),
                data_fine_validita = "99991231",
                grade = model.grade,
                punteggio = model.punteggio,
                id_sezione = model.Sezione.id,
                tms_timestamp = DateTime.Now,
                cod_user = matr
            };
            db.XR_STR_PESO_SEZIONE.Add(peso);
            db.SaveChanges();
        }

        public static string GetNomeCognomeCamel(string matricola)
        {
            var db = IncarichiManager.GetIncarichiDBContext();
            var sint = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matricola).FirstOrDefault();
            if (sint != null && !String.IsNullOrWhiteSpace(sint.DES_COGNOMEPERS) && !String.IsNullOrWhiteSpace(sint.DES_NOMEPERS))
            {
                return IncarichiManager.PrimaMaiuscola(sint.DES_NOMEPERS) + " " + IncarichiManager.PrimaMaiuscola(sint.DES_COGNOMEPERS);
            }
            else
                return null;
        }
        public static string InvertiSedeServizioCodice(string par)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(par) || !par.Contains("-")) return par;
                par = par.Trim();
                int pos = par.IndexOf("-");
                if (pos <= 0) return par;
                string inv = par.Substring(pos + 1).Trim() + " (" + par.Substring(0, pos - 1) + ")";
                return inv;
            }
            catch 
            {
                return par;
            }
        }
        public static string PrimaMaiuscola(string v)
        {
            if (String.IsNullOrWhiteSpace(v)) return v;
            v = v.Trim();
            try
            {
                string[] parti = v.Split(' ');
                for (int i = 0; i < parti.Length; i++)
                {
                    string p = parti[i].Substring(0, 1).ToUpper() + parti[i].Substring(1).ToLower();
                    p = p.Trim();
                    if (p.Length == 1)
                        p = p.ToLower();
                    else
                    {
                        if (p.Contains("-") && !p.EndsWith("-") && !p.StartsWith("-"))
                        {
                            string newp = "";
                            for (int k = 0; k < p.Length; k++)
                            {
                                if (p[k] == '-')
                                {
                                    newp += "-" + p[k + 1].ToString().ToUpper();
                                    k++;
                                }
                                else newp += p[k];
                            }
                            p = newp;
                        }
                    }

                    parti[i] = p;
                }
                return String.Join(" ", parti);
            }
            catch
            {
                return v;
            }
        }
        public static void ChiudiSezioneEIncarichi(int idsezione, DateTime DataChiusura)
        {
            TalentiaEntities db = IncarichiManager.GetIncarichiDBContext();
            var sez = db.XR_STR_TSEZIONE.Where(x => x.id == idsezione && x.data_fine_validita=="99991231").FirstOrDefault();
            if (sez != null)
            {
                sez.data_fine_validita = DataChiusura.ToString("yyyyMMdd");
                sez.data_fine_contabile = DataChiusura.AddMonths(1).ToString("yyyyMMdd");

                var listIncarichi = db.XR_STR_TINCARICO.Where(x => x.id_sezione == idsezione && x.data_fine_validita == "99991231").ToList();
                if (listIncarichi.Any())
                {
                    foreach (var i in listIncarichi)
                    {
                        i.data_fine_validita = DataChiusura.ToString("yyyyMMdd");
                    }
                }
                db.SaveChanges();
            }
        }
        public static List<int> GetSezioniFiglie(int idsezione)
        {
            TalentiaEntities db = IncarichiManager.GetIncarichiDBContext();
            List<int> IDFiglie = new List<int>();
            IDFiglie = GetFiglie(db, IDFiglie, idsezione);

            return IDFiglie;
        }

        public static List<int> GetFiglie(TalentiaEntities db, List<int> IDFiglie, int IDPadre)
        {
            var figli = db.XR_STR_TALBERO.Where(x => x.subordinato_a == IDPadre).ToList();
            foreach (var sezioneFiglia in figli)
            {
                IDFiglie.Add(sezioneFiglia.id);
                IDFiglie = GetFiglie(db, IDFiglie, sezioneFiglia.id);
            }
            return IDFiglie;
        }
        public static string SaveSezione(GestioneSezioneModel model, string tipoSalvataggio)
        {
            DateTime DE = default(DateTime);
            bool UsaDataEffettiva = false;
            if (!String.IsNullOrWhiteSpace(model.DataEffettiva))
            {
                UsaDataEffettiva = DateTime.TryParseExact(model.DataEffettiva, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DE);
            }

            var db = GetIncarichiDBContext();

            DateTime D;
            if (!DateTime.TryParseExact(model.Sezione.data_inizio_validita, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D))
                return "Data non valida";

            if (model.Sezione.codice_visibile.Trim().Length == 2)
            {
                if (String.IsNullOrWhiteSpace(model.flag_prodotto))
                {
                    return "Impostare il Flag Prodotto/Supporto";
                }
            }
            string d = D.ToString("yyyyMMdd");


            if (model.punteggio != null || model.grade != null)
            {
                if (tipoSalvataggio == "N")
                {
                    AddPeso(model);
                }
                else
                {
                    XR_STR_PESO_SEZIONE pesoEsistente = db.XR_STR_PESO_SEZIONE.Where(x => x.id_sezione == model.Sezione.id && x.data_fine_validita == "99991231")
                        .FirstOrDefault();
                    if (pesoEsistente == null)
                    {
                        AddPeso(model);
                    }
                    else
                    {
                        if (model.grade != pesoEsistente.grade || model.punteggio != pesoEsistente.punteggio)
                        {
                            pesoEsistente.data_fine_validita = DateTime.Now.ToString("yyyyMMdd");
                            db.SaveChanges();
                            AddPeso(model);
                        }

                    }
                }

            }

            if (tipoSalvataggio == "N")
            {
                string codiceNuovaSezione = model.Sezione.codice_visibile.Trim();
                if (db.XR_STR_TSEZIONE.Any(x => x.codice_visibile == codiceNuovaSezione))
                    return "Codice sezione già in uso";

                if (codiceNuovaSezione.Trim().Length > 9)
                    return "Codice sezione troppo esteso (max 9 caratteri)";

                try
                {
                    decimal? MaxOrdine = db.XR_STR_TALBERO
                           .Where(x => x.subordinato_a == model.IdSezionePadre)
                           .Join(db.XR_STR_TSEZIONE.Where(x => x.data_fine_validita == "99991231"),
                                     alb => alb.id,
                                     s => s.id,
                                     (alb, s) => new
                                     {
                                         ordine = s.num_ordina
                                     }).ToList().Max(x => x.ordine);
                    if (MaxOrdine == null) MaxOrdine = 0;

                    int idmax = db.XR_STR_TSEZIONE.Select(x => x.id).OrderByDescending(x => x).FirstOrDefault();

                    myRaiDataTalentia.XR_STR_TSEZIONE NuovaSez = new myRaiDataTalentia.XR_STR_TSEZIONE()
                    {
                        id = idmax + 1,
                        area = model.Sezione.area,
                        codice_visibile = codiceNuovaSezione,
                        descrizione_breve = model.Sezione.descrizione_breve,
                        descrizione_lunga = model.Sezione.descrizione_lunga,
                        indirizzo = model.Sezione.indirizzo,
                        sede_contabile = model.Sezione.sede_contabile.Trim(),
                        servizio = model.Sezione.servizio.Trim(),
                        mission = model.Sezione.mission,
                        num_ordina = MaxOrdine + 1,
                        data_inizio_validita = IncarichiHelper.ConvertDateFromSlashedDDMMYYYY_to_YYYYMMDD(model.Sezione.data_inizio_validita),
                        data_fine_validita = "99991231",
                        livello = model.Sezione.livello,
                        tipo = model.Sezione.tipo,
                        pubblicato = model.Sezione.pubblicato,
                        flag_prodotto = model.flag_prodotto,
                        max_responsabili = model.Sezione.max_responsabili
                    };
                    NuovaSez.data_convalida = NuovaSez.data_inizio_validita;
                    NuovaSez.data_formalizza = NuovaSez.data_inizio_validita;
                    if (UsaDataEffettiva)
                    {
                        NuovaSez.data_inizio_validita = DE.ToString("yyyyMMdd");
                        NuovaSez.data_convalida = DE.ToString("yyyyMMdd");
                        NuovaSez.data_formalizza = DE.ToString("yyyyMMdd");
                    }
                    db.XR_STR_TSEZIONE.Add(NuovaSez);



                    db.SaveChanges();
                    myRaiDataTalentia.XR_STR_TALBERO T = new myRaiDataTalentia.XR_STR_TALBERO() { id = NuovaSez.id, subordinato_a = model.IdSezionePadre, tipo_schema = "ge" };
                    db.XR_STR_TALBERO.Add(T);
                    db.SaveChanges();



                }
                catch (Exception ex)
                {
                    myRaiHelper.Logger.LogErrori(new MyRai_LogErrori() { error_message = ex.ToString() });
                    return "Errore salvataggio dati";

                }
                return null;
            }
            var sez = db.XR_STR_TSEZIONE.Where(x => x.id == model.Sezione.id && x.data_inizio_validita == d).FirstOrDefault();
            if (sez == null)
                return "Errore DB, dati non trovati";



            if (tipoSalvataggio == "D")
            {
                if (!String.IsNullOrWhiteSpace(sez.data_fine_contabile))
                {
                    return "Impossibile eliminare una sezione già contabilizzata";
                }
                else
                {
                    if (db.XR_STR_TALBERO.Any(x => x.subordinato_a == model.Sezione.id))
                        return "La sezione ha elementi in cascata e non puo essere cancellata";
                    else
                    {
                        db.XR_STR_TSEZIONE.RemoveWhere(x => x.id == model.Sezione.id);
                        db.XR_STR_TALBERO.RemoveWhere(x => x.id == model.Sezione.id);
                        var listIncarichi = db.XR_STR_TINCARICO.Where(x => x.id_sezione == model.Sezione.id && x.data_fine_validita == "99991231").ToList();
                        if (listIncarichi.Any())
                        {
                            foreach (var i in listIncarichi)
                            {
                                db.XR_STR_TINCARICO.Remove(i);
                            }
                        }
                    }
                }
            }
            else if (tipoSalvataggio == "C")
            {

                if (UsaDataEffettiva)
                {
                    sez.data_fine_validita = DE.ToString("yyyyMMdd");
                    sez.data_fine_contabile = DE.AddMonths(1).ToString("yyyyMMdd");
                }
                else
                {
                    sez.data_fine_validita = DateTime.Now.ToString("yyyyMMdd");
                    sez.data_fine_contabile = DateTime.Now.AddMonths(1).ToString("yyyyMMdd");
                }

                var listIncarichi = db.XR_STR_TINCARICO.Where(x => x.id_sezione == model.Sezione.id && x.data_fine_validita == "99991231").ToList();
                if (listIncarichi.Any())
                {
                    foreach (var i in listIncarichi)
                    {
                        i.data_fine_validita = DateTime.Today.ToString("yyyyMMdd");
                    }
                }
                List<int> SezFiglie = GetSezioniFiglie(model.Sezione.id);
                if (SezFiglie != null && SezFiglie.Any())
                {
                    foreach (int id in SezFiglie)
                    {
                        if (UsaDataEffettiva)
                            ChiudiSezioneEIncarichi(id, DE);
                        else
                            ChiudiSezioneEIncarichi(id, DateTime.Now);
                    }
                }
            }
            else if (tipoSalvataggio == "R")
            {
                if (sez.area != model.Sezione.area)
                    sez.area = model.Sezione.area;
                if (sez.descrizione_breve != model.Sezione.descrizione_breve)
                    sez.descrizione_breve = model.Sezione.descrizione_breve;
                if (sez.descrizione_lunga != model.Sezione.descrizione_lunga)
                    sez.descrizione_lunga = model.Sezione.descrizione_lunga;
                if (sez.indirizzo != model.Sezione.indirizzo)
                    sez.indirizzo = model.Sezione.indirizzo;
                if (sez.mission != model.Sezione.mission)
                    sez.mission = model.Sezione.mission;
                if (sez.sede_contabile != model.Sezione.sede_contabile)
                    sez.sede_contabile = model.Sezione.sede_contabile.Trim();
                if (sez.servizio != model.Sezione.servizio)
                    sez.servizio = model.Sezione.servizio.Trim();
                if (sez.livello != model.Sezione.livello)
                    sez.livello = model.Sezione.livello.Trim();
                if (sez.tipo != model.Sezione.tipo)
                    sez.tipo = model.Sezione.tipo.Trim();
                if (sez.pubblicato != model.Sezione.pubblicato)
                    sez.pubblicato = model.Sezione.pubblicato;
                if (sez.max_responsabili != model.Sezione.max_responsabili)
                    sez.max_responsabili = model.Sezione.max_responsabili;
            }
            else
            {
                // tipoSalvataggio == "M"
                if (UsaDataEffettiva)
                    sez.data_fine_validita = DE.AddDays(-1).ToString("yyyyMMdd");
                else
                    sez.data_fine_validita = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");


                var NewSezione = new myRaiDataTalentia.XR_STR_TSEZIONE()
                {
                    flag_prodotto = model.flag_prodotto,
                    area = model.Sezione.area,
                    attivita = sez.attivita,
                    codice_visibile = sez.codice_visibile,
                    data_convalida = sez.data_convalida,
                    data_fine_contabile = sez.data_fine_contabile,
                    data_fine_validita = "99991231",
                    data_formalizza = sez.data_formalizza,
                    data_inizio_contabile = sez.data_inizio_contabile,
                    data_inizio_validita = UsaDataEffettiva ? DE.ToString("yyyyMMdd") : DateTime.Now.ToString("yyyyMMdd"),
                    descrizione_breve = model.Sezione.descrizione_breve,
                    descrizione_lunga = model.Sezione.descrizione_lunga,
                    id = sez.id,
                    indirizzo = model.Sezione.indirizzo,
                    mission = model.Sezione.mission,
                    num_ordina = sez.num_ordina,
                    sede_contabile = model.Sezione.sede_contabile.Trim(),
                    servizio = model.Sezione.servizio.Trim(),
                    tel_internazionale = sez.tel_internazionale,
                    tipo = model.Sezione.tipo,
                    pubblicato = model.Sezione.pubblicato,
                    max_responsabili = model.Sezione.max_responsabili
                };

                db.XR_STR_TSEZIONE.Add(NewSezione);

            }
            try
            {
                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "SaveSezione"
                });
                return "Errore salvataggio dati";
            }

        }

        public static string MoveSezione(int idsezione, int idSezionePadre)
        {
            try
            {
                var db = GetIncarichiDBContext();
                var alb = db.XR_STR_TALBERO.Where(x => x.id == idsezione).FirstOrDefault();
                if (alb != null)
                {
                    alb.subordinato_a = idSezionePadre;
                    db.SaveChanges();
                }
                return null;
            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new MyRai_LogErrori() { error_message = ex.ToString() });
                return "Errore salvataggio dati";
            }
        }

        public static string GetMansione(string mans)
        {
            var db = GetIncarichiDBContext();
            var c = db.RUOLO.Where(x => x.COD_RUOLO.Trim() == mans.Trim()).FirstOrDefault();
            if (c == null) return null;
            else return c.DES_RUOLO.Trim();
        }
        public static string EliminaIncarico(int idincarico)
        {
            var db = GetIncarichiDBContext();
            var incarico = db.XR_STR_TINCARICO.Where(x => x.id_incarico == idincarico).FirstOrDefault();
            if (incarico == null)
                return "Errore DB, dati non trovati";
            else
            {
                try
                {
                    db.XR_STR_TINCARICO.Remove(incarico);
                    db.SaveChanges();
                    return null;
                }
                catch (Exception ex)
                {
                    myRaiHelper.Logger.LogErrori(new MyRai_LogErrori() { error_message = ex.ToString() });
                    return "Errore salvataggio dati";
                }
            }
        }
        public static string GetDescrizioneIncarico(string codiceIncarico)
        {
            var db = GetIncarichiDBContext(); ;
            return db.XR_STR_DINCARICO.Where(x => x.COD_INCARICO == codiceIncarico).Select(x => x.DES_INCARICO.Trim()).FirstOrDefault();

        }

        public static void EliminaResp(GestioneIncaricoModel model)
        {
            var db = GetIncarichiDBContext();
            XR_STR_TSEZIONE sez = null;
            if (String.IsNullOrWhiteSpace(model.CodSezioneFromAnagrafica))
            {
                sez=db.XR_STR_TSEZIONE.Where(x => x.id == model.Incarico.id_sezione && x.data_fine_validita == "99991231").FirstOrDefault();
            }
            else
            {
                sez=db.XR_STR_TSEZIONE.Where(x => x.codice_visibile==model.CodSezioneFromAnagrafica && x.data_fine_validita == "99991231").FirstOrDefault();
            }
            if (sez != null && sez.max_responsabili == 1)
            {
                var inc = db.XR_STR_TINCARICO.Where(x => x.id_sezione == model.Incarico.id_sezione && x.data_fine_validita == "99991231").ToList();
                if (inc.Any())
                {
                    foreach (var i in inc)
                    {
                        if (i.flag_resp == "1")
                        {
                            i.flag_resp = "0";
                        }
                    }
                    db.SaveChanges();
                }
            }
            if (sez.max_responsabili > 1)
            {
                if (HttpContext.Current.Request["respcanc"] != null)
                {
                    int id;
                    if (int.TryParse(HttpContext.Current.Request["respcanc"].ToString(), out id))
                    {
                        var inc = db.XR_STR_TINCARICO.Where(x => x.id_incarico == id).FirstOrDefault();
                        if (inc != null)
                        {
                            inc.flag_resp = "0";
                            db.SaveChanges();
                        }
                    }
                }
            }

        }
        public static string SaveIncarico(GestioneIncaricoModel model)
        {
            string flagResp = model.Incarico.flag_resp == null ? "0" : "1";
            if (flagResp == "1")
                EliminaResp(model);

            var db = GetIncarichiDBContext();
            if (!String.IsNullOrWhiteSpace(model.CodSezioneFromAnagrafica))
            {
                var sezioneDaAnag = db.XR_STR_TSEZIONE.Where(x => x.codice_visibile == model.CodSezioneFromAnagrafica && x.data_fine_validita == "99991231")
                    .FirstOrDefault();
                if (sezioneDaAnag == null)
                {
                    return "Impossibile trovare sezione valida";
                }
                else
                    model.Incarico.id_sezione = sezioneDaAnag.id;
            }
            string i = IncarichiHelper.ConvertDateFromSlashedDDMMYYYY_to_YYYYMMDD(model.Incarico.data_inizio_validita);
            int data_in = Convert.ToInt32(i);
            var stessiIncarichiEsistenti = db.XR_STR_TINCARICO.Where(x =>
                                              x.id_sezione == model.Incarico.id_sezione
                                           && x.cod_incarico == model.Incarico.cod_incarico
                                           && x.id_incarico != model.Incarico.id_incarico
                                           ).ToList();

            int Sovrapposti = 0;
            foreach (var incaricoEsistente in stessiIncarichiEsistenti)
            {
                int d1 = Convert.ToInt32(incaricoEsistente.data_inizio_validita);
                int d2 = Convert.ToInt32(incaricoEsistente.data_fine_validita);
                if (data_in >= d1 && data_in <= d2)
                {
                    Sovrapposti++;
                }
            }
            if (Sovrapposti > 0)
            {
                var sez = db.XR_STR_TSEZIONE.Where(x => x.id == model.Incarico.id_sezione && x.data_fine_validita == "99991231")
                    .FirstOrDefault();

                if (sez.max_responsabili > 1)
                {
                    if (IsMultiResponsabile(model.Incarico.cod_incarico, db) == false)
                    {
                        return "E' stato raggiunto il numero massimo di incarichi per questa sezione";
                    }
                    else
                    {
                        if (Sovrapposti >= sez.max_responsabili)
                        {
                            return "E' stato raggiunto il numero massimo di incarichi per questa sezione";
                        }
                    }
                }
            }

            var primaSez = db.XR_STR_TSEZIONE.Where(x => x.id == model.Incarico.id_sezione).OrderBy(x => x.data_inizio_validita).FirstOrDefault();
            if (primaSez != null)
            {
                int datainizio;
                if (int.TryParse(primaSez.data_inizio_validita, out datainizio))
                {
                    string inizio = IncarichiHelper.ConvertDateFromSlashedDDMMYYYY_to_YYYYMMDD(model.Incarico.data_inizio_validita);
                    if (Convert.ToInt32(inizio) < datainizio)
                    {
                        return "La data inizio incarico non può essere precedente alla data iniziale della sezione (" + datainizio + ")";
                    }
                }
            }


            if (model.Incarico.id_incarico == 0)
            {
                try
                {
                    myRaiDataTalentia.XR_STR_TINCARICO newInc = new myRaiDataTalentia.XR_STR_TINCARICO()
                    {
                        id_sezione = model.Incarico.id_sezione,
                        matricola = model.Incarico.matricola,
                        data_inizio_validita = IncarichiHelper.ConvertDateFromSlashedDDMMYYYY_to_YYYYMMDD(model.Incarico.data_inizio_validita),
                        data_fine_validita = IncarichiHelper.ConvertDateFromSlashedDDMMYYYY_to_YYYYMMDD(model.Incarico.data_fine_validita),
                        cod_incarico = model.Incarico.cod_incarico,
                        flag_resp = model.Incarico.flag_resp == null ? "0" : "1",
                        nominativo = model.Incarico.nominativo,
                        note_incarico = model.Incarico.note_incarico,
                        tms_timestamp = DateTime.Now,
                         incarico_personalizzato=model.Incarico.incarico_personalizzato
                    };
                    db.XR_STR_TINCARICO.Add(newInc);

                    var sez = db.XR_STR_TSEZIONE.Where(x => x.id == model.Incarico.id_sezione && x.data_fine_validita == "99991231")
                    .FirstOrDefault();
                    //if (sez.max_responsabili == 1)
                    //{
                    //    var oldIncarico = db.XR_STR_TINCARICO.Where(x =>
                    //                  x.cod_incarico == model.Incarico.cod_incarico
                    //                  && x.id_sezione == model.Incarico.id_sezione
                    //                  && x.data_fine_validita == "99991231").FirstOrDefault();
                    //    if (oldIncarico != null)
                    //    {
                    //        DateTime D;
                    //        if (DateTime.TryParseExact(model.Incarico.data_inizio_validita, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D))
                    //        {
                    //            oldIncarico.data_fine_validita = D.AddDays(-1).ToString("yyyyMMdd");
                    //        }
                    //    }
                    //}

                    db.SaveChanges();
                    return null;
                }
                catch (Exception ex)
                {
                    myRaiHelper.Logger.LogErrori(new MyRai_LogErrori() { error_message = ex.ToString() });
                    return "Errore salvataggio dati";
                }
            }
            var inc = db.XR_STR_TINCARICO.Where(x => x.id_incarico == model.Incarico.id_incarico).FirstOrDefault();
            if (inc == null)
                return "Errore DB, dati non trovati";

            try
            {
                inc.note_incarico = model.Incarico.note_incarico;
                inc.cod_incarico = model.Incarico.cod_incarico;
                inc.data_inizio_validita = IncarichiHelper.ConvertDateFromSlashedDDMMYYYY_to_YYYYMMDD(model.Incarico.data_inizio_validita);
                inc.data_fine_validita = IncarichiHelper.ConvertDateFromSlashedDDMMYYYY_to_YYYYMMDD(model.Incarico.data_fine_validita);
                inc.flag_resp = model.Incarico.flag_resp == null ? "0" : "1";
                inc.tms_timestamp = DateTime.Now;
                inc.incarico_personalizzato = model.Incarico.incarico_personalizzato;
                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new MyRai_LogErrori() { error_message = ex.ToString() });
                return "Errore salvataggio dati";
            }
        }

        private static bool IsMultiResponsabile(string cod_incarico, myRaiDataTalentia.TalentiaEntities db)
        {
            var t = db.XR_STR_DINCARICO.Where(x => x.COD_INCARICO == cod_incarico).FirstOrDefault();
            if (t != null && t.IND_INCMULTIPLI == "Y")
                return true;
            else
                return false;
        }

        public static string SaveOrder(string sez, string data)
        {
            try
            {
                DateTime D;
                DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);

                int[] ids = Array.ConvertAll<string, int>(sez.Split(','), x => Convert.ToInt32(x));
                var db = GetIncarichiDBContext();
                int d = Convert.ToInt32(D.ToString("yyyyMMdd"));

                db.XR_STR_TSEZIONE
               .Where(x => ids.Contains(x.id))
               .AsEnumerable()
               .Where(x => Convert.ToInt32(x.data_inizio_validita) <= d && Convert.ToInt32(x.data_fine_validita) >= d)
               .OrderBy(item => ids.ToList().IndexOf(item.id))
               .ToList()
               .ForEach(x => x.num_ordina = ids.ToList().IndexOf(x.id));

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new MyRai_LogErrori() { error_message = ex.ToString() });
                return "Errore salvataggio dati";
            }

            return null;

        }


        public static string CopyIpotesiproduzione()
        {
            var dbIp = new myRaiDataTalentia.TalentiaEntities("TalentiaEntities_IP");
            var dbProd = new myRaiDataTalentia.TalentiaEntities("TalentiaEntities");

            using (SqlConnection con = new SqlConnection(dbProd.Database.Connection.ConnectionString))
            {
                con.Open();
                using (SqlTransaction tr = con.BeginTransaction(IsolationLevel.Serializable))
                {
                    string esito = CopyTable<myRaiDataTalentia.XR_STR_TALBERO>(dbIp.XR_STR_TALBERO.ToList(), con, tr);
                    if (esito != null)
                    {
                        tr.Rollback();
                        return esito;
                    }

                    esito = CopyTable<myRaiDataTalentia.XR_STR_TSEZIONE>(dbIp.XR_STR_TSEZIONE.ToList(), con, tr);
                    if (esito != null)
                    {
                        tr.Rollback();
                        return esito;
                    }

                    esito = CopyTable<myRaiDataTalentia.XR_STR_TINCARICO>(dbIp.XR_STR_TINCARICO.ToList(), con, tr);
                    if (esito != null)
                    {
                        tr.Rollback();
                        return esito;
                    }

                    tr.Commit();
                }
            }
            return null;
        }

        public static string CopyTable<T>(List<T> datiSource, SqlConnection con, SqlTransaction tr)
        {
            try
            {
                string TabellaDestinazione = typeof(T).Name;

                var dt = new DataTable();
                foreach (var pi in typeof(T).GetProperties())
                {
                    dt.Columns.Add(new DataColumn(pi.Name, Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType));
                }

                foreach (var item in datiSource)
                {
                    var row = dt.NewRow();

                    foreach (var i in item.GetType().GetProperties())
                    {
                        row[i.Name] = i.GetValue(item, null);
                    }
                    dt.Rows.Add(row);
                }


                SqlCommand comm = con.CreateCommand();
                comm.CommandText = "truncate table " + TabellaDestinazione;
                comm.Transaction = tr;
                comm.ExecuteNonQuery();

                using (SqlBulkCopy sbc = new SqlBulkCopy(con, SqlBulkCopyOptions.KeepIdentity, tr))
                {
                    sbc.DestinationTableName = TabellaDestinazione;
                    sbc.BatchSize = 100000;
                    sbc.WriteToServer(dt);
                }

            }
            catch (Exception ex)
            {
                myRaiHelper.Logger.LogErrori(new MyRai_LogErrori() { error_message = ex.ToString() });


                return "Errore salvataggio dati";
            }
            return null;
        }

        public static IncarichiElencoAnagrafiche GetElencoAnagrafiche(RicercaAnagrafica model)
        {
            IncarichiElencoAnagrafiche elencoAnagrafiche = new IncarichiElencoAnagrafiche();
            myRaiDataTalentia.TalentiaEntities db = new myRaiDataTalentia.TalentiaEntities();

            if (model.HasFilter)
            {
                IQueryable<myRaiDataTalentia.SINTESI1> tmp = db.SINTESI1;

                tmp = tmp.Where(x => x.DTA_FINE_CR != null && x.DTA_FINE_CR > DateTime.Now);

                if (!String.IsNullOrWhiteSpace(model.Matricola))
                    tmp = tmp.Where(x => model.Matricola.Contains(x.COD_MATLIBROMAT));

                if (!String.IsNullOrWhiteSpace(model.Cognome))
                    tmp = tmp.Where(x => x.DES_COGNOMEPERS.StartsWith(model.Cognome.ToUpper()));

                if (!String.IsNullOrWhiteSpace(model.Nome))
                    tmp = tmp.Where(x => x.DES_NOMEPERS.StartsWith(model.Nome.ToUpper()));

                if (!String.IsNullOrWhiteSpace(model.Servizio))
                    tmp = tmp.Where(x => x.COD_SERVIZIO == model.Servizio);

                tmp = tmp.OrderBy(y => y.DES_COGNOMEPERS).ThenBy(z => z.DES_NOMEPERS);

                elencoAnagrafiche.anagrafiche = tmp.ToList();
            }
            elencoAnagrafiche.TreeSearch = model.TreeSearch;
            return elencoAnagrafiche;
        }
    }
}

