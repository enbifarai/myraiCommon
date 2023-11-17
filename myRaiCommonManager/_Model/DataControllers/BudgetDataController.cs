using myRaiCommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using myRaiData.Incentivi;
using System.Linq.Expressions;
using myRaiHelper;
using myRaiCommonModel.Gestionale.HRDW;
using System.Data.Entity.Infrastructure;
using myRaiCommonModel.Gestionale;

namespace myRai.DataControllers
{
    public class BudgetDataController
    {
        public BudgetDataController()
        {
        }

        private static IQueryable<XR_PRV_DIPENDENTI> BudgetGetDipendenti(IncentiviEntities db, List<int> idCampagnas, IEnumerable<int> direzioni, int? anno)
        {
            IQueryable<XR_PRV_DIPENDENTI> uss;
            var anyProvEff = PoliticheRetributiveHelper.AnyDipProvEff();
            uss = db.XR_PRV_DIPENDENTI.Include("XR_PRV_DIPENDENTI_VARIAZIONI")
                    .Where(x => idCampagnas.Contains(x.ID_CAMPAGNA.Value))
                    .Where(x => direzioni.Contains(x.ID_DIREZIONE))
                    .Where(anyProvEff);

            if (anno.HasValue && anno.GetValueOrDefault() != DateTime.Now.Year)
            {
                uss = uss.Where(x => x.DECORRENZA.Value.Year == anno.Value);
            }
            else if (anno.HasValue && anno.GetValueOrDefault() == DateTime.Now.Year)
            {
                uss = uss.Where(x => (x.DECORRENZA.Value.Year == anno.Value || x.DECORRENZA == null));
            }
            else
            {
            }

            uss = uss.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

            return uss;
        }

        /// <summary>
        /// Reperimento delle info riguardanti le aree coinvolte nella campagna
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<DettaglioCampagna> GetDettaglioCampagna(string currentMatricola, int id, int? idArea = null, int? anno = null)
        {
            List<DettaglioCampagna> result = new List<DettaglioCampagna>();
            try
            {
                using (var db = new IncentiviEntities())
                {
                    var rslt = (from budget in db.XR_PRV_CAMPAGNA_BUDGET
                                join area in db.XR_PRV_AREA
                                on budget.ID_AREA equals area.ID_AREA
                                where budget.ID_CAMPAGNA == id
                                select new
                                {
                                    budget = budget,
                                    area = area
                                });


                    if (rslt != null && rslt.Any())
                    {
                        foreach (var r in rslt)
                        {
                            if (idArea.HasValue && r.area.ID_AREA != idArea.Value)
                                continue;

                            decimal costo = 0;
                            var direzioni = this.GetDirezioniByIdArea(currentMatricola, r.area.ID_AREA, id, anno);

                            if (direzioni != null && direzioni.Any())
                            {
                                costo = this.GetCostoDirezioni(direzioni.Select(x => x.ID_DIREZIONE).ToList(), new List<int> { id }, anno);
                            }
                            else
                            {
                                continue;
                            }
                            result.Add(new DettaglioCampagna()
                            {
                                Budget = r.budget.BUDGET,
                                IdArea = r.area.ID_AREA,
                                NomeArea = r.area.NOME,
                                BudgetSpeso = costo
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public List<DettaglioCampagna> GetDettaglioCampagna(string currentMatricola, List<int> ids, int? idArea = null, int? anno = null)
        {
            bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
            bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);

            List<DettaglioCampagna> result = new List<DettaglioCampagna>();
            try
            {
                using (var db = new IncentiviEntities())
                {
                    int idAreaRS = 0;
                    List<PRV_DIREZIONE> dirAreaRS = null;
                    if (enableQIO == enableRS)
                    {
                        idAreaRS = db.XR_PRV_AREA.FirstOrDefault(x => x.LV_ABIL.Contains(PoliticheRetributiveHelper.BUDGETRS_HRGA_SOTTO_FUNC)).ID_AREA;
                        dirAreaRS = this.GetDirezioniByIdArea(currentMatricola, idAreaRS, ids);
                    }

                    var rslt = (from budget in db.XR_PRV_CAMPAGNA_BUDGET
                                join area in db.XR_PRV_AREA
                                on budget.ID_AREA equals area.ID_AREA
                                where ids.Contains(budget.ID_CAMPAGNA)
                                select new
                                {
                                    budget = budget,
                                    area = area
                                });//.ToList();


                    if (rslt != null && rslt.Any())
                    {
                        foreach (var r in rslt.GroupBy(x => x.area.ID_AREA))
                        {
                            if (idArea.HasValue && r.Key != idArea.Value)
                                continue;

                            if (enableQIO == enableRS && r.Key == idAreaRS)
                                continue;

                            var direzioni = this.GetDirezioniByIdArea(currentMatricola, r.Key, ids);
                            List<int> idDirezioni = new List<int>();
                            idDirezioni.AddRange(direzioni.Select(x => x.ID_DIREZIONE));
                            if (enableQIO == enableRS)
                                idDirezioni.AddRange(dirAreaRS.Where(x => direzioni.Any(y => y.CODICE == x.CODICE)).Select(x => x.ID_DIREZIONE));

                            if (idDirezioni.Count() == 0) continue;

                            result.Add(new DettaglioCampagna()
                            {
                                Budget = r.Sum(x => x.budget.BUDGET),
                                IdArea = r.Key,
                                NomeArea = r.First().area.NOME,
                                BudgetSpeso = this.GetCostoDirezioni(idDirezioni, ids, anno)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public InfoCampagna GetCampagna(string currentMatricola, int id, int? idArea = null, int? anno = null)
        {
            InfoCampagna result = new InfoCampagna();
            try
            {
                using (var db = new IncentiviEntities())
                {
                    var funcFilter = PoliticheRetributiveHelper.FuncFilterCampagna();

                    if (id != 0)
                    {
                        var rslt = db.XR_PRV_CAMPAGNA.Where(w => w.ID_CAMPAGNA.Equals(id)).Where(funcFilter).FirstOrDefault();

                        if (rslt != null)
                        {
                            result.Id = rslt.ID_CAMPAGNA;
                            result.NomeCampagna = rslt.NOME;
                            result.DettaglioCampagna = new List<DettaglioCampagna>();
                            result.DettaglioCampagna = GetDettaglioCampagna(currentMatricola, rslt.ID_CAMPAGNA, idArea, anno);
                        }
                        else
                        {
                            var rslt2 = db.XR_PRV_CAMPAGNA.Where(w => w.DTA_FINE != null).Where(funcFilter).ToList();
                            if (rslt2 != null)
                            {
                                var item = rslt2.OrderByDescending(w => w.DTA_FINE).FirstOrDefault();
                                result.Id = item.ID_CAMPAGNA;
                                result.NomeCampagna = item.NOME;
                                result.DettaglioCampagna = new List<DettaglioCampagna>();
                                result.DettaglioCampagna = GetDettaglioCampagna(currentMatricola, item.ID_CAMPAGNA, idArea, anno);

                            }
                        }
                    }
                    else
                    {
                        result.Id = 0;
                        result.NomeCampagna = "Totale";
                        result.DettaglioCampagna = new List<DettaglioCampagna>();

                        var rlst = db.XR_PRV_CAMPAGNA.Where(funcFilter).Where(x => x.DTA_FINE == null || x.DTA_FINE.Value > DateTime.Today).Where(x => x.ID_CAMPAGNA > 2).Where(w => !w.ESCLUDI_DA_TOTALI);

                        result.CampagneContenute.AddRange(rlst.Select(x => x.ID_CAMPAGNA));
                        result.DettaglioCampagna = GetDettaglioCampagna(currentMatricola, result.CampagneContenute, idArea, anno);
                    }

                    if (result != null)
                    {
                        if (result.Id == 0)
                        {
                            var dts = db.XR_PRV_CAMPAGNA_DECORRENZA.Select(w => w.DT_DECORRENZA).Distinct().ToList();
                            if (dts != null && dts.Any())
                            {
                                result.Decorrenze = new List<DateTime>();
                                result.Decorrenze.AddRange(dts.ToList());
                            }
                        }
                        else
                        {
                            var dts = db.XR_PRV_CAMPAGNA_DECORRENZA.Where(w => w.ID_CAMPAGNA.Equals(id)).Select(w => w.DT_DECORRENZA).Distinct().ToList();
                            if (dts != null && dts.Any())
                            {
                                result.Decorrenze = new List<DateTime>();
                                result.Decorrenze.AddRange(dts.ToList());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public List<int> GetCampagneContenute(int id)
        {
            List<int> result = new List<int>();
            if (id == 0)
            {
                using (var db = new IncentiviEntities())
                {
                    var funcFilter = PoliticheRetributiveHelper.FuncFilterCampagna();
                    var rlst = db.XR_PRV_CAMPAGNA.Where(funcFilter).Where(x => x.ID_CAMPAGNA > 2).Where(x => x.DTA_FINE == null || x.DTA_FINE.Value > DateTime.Today).Where(w => !w.ESCLUDI_DA_TOTALI);

                    foreach (var subItem in rlst)
                    {
                        result.Add(subItem.ID_CAMPAGNA);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Reperimento dei dati della campagna corrente
        /// </summary>
        /// <returns></returns>
        public InfoCampagna GetCurrentCampagna(string currentMatricola, int? anno = null)
        {
            InfoCampagna result = new InfoCampagna();
            try
            {
                using (var db = new IncentiviEntities())
                {
                    var funcFilter = PoliticheRetributiveHelper.FuncFilterCampagna();

                    var rslt = db.XR_PRV_CAMPAGNA.Where(w => w.DTA_FINE == null).Where(w => !w.ESCLUDI_DA_TOTALI).Where(funcFilter).FirstOrDefault();

                    if (rslt != null)
                    {
                        result.Id = rslt.ID_CAMPAGNA;
                        result.NomeCampagna = rslt.NOME;
                        result.DettaglioCampagna = new List<DettaglioCampagna>();
                        result.DettaglioCampagna = GetDettaglioCampagna(currentMatricola, rslt.ID_CAMPAGNA, null, anno);
                    }
                    else
                    {
                        var rslt2 = db.XR_PRV_CAMPAGNA.Where(w => w.DTA_FINE != null).Where(funcFilter).ToList();
                        if (rslt2 != null)
                        {
                            var item = rslt2.OrderByDescending(w => w.DTA_FINE).FirstOrDefault();
                            result.Id = item.ID_CAMPAGNA;
                            result.NomeCampagna = item.NOME;
                            result.DettaglioCampagna = new List<DettaglioCampagna>();
                            result.DettaglioCampagna = GetDettaglioCampagna(currentMatricola, item.ID_CAMPAGNA, null, anno);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public List<InfoCampagna> GetCampagne(bool getDettaglioCampagna = false, bool loadClosed = false)
        {
            List<InfoCampagna> result = new List<InfoCampagna>();
            try
            {
                using (var db = new IncentiviEntities())
                {
                    var funcFilter = PoliticheRetributiveHelper.FuncFilterCampagna();
                    var tmp = db.XR_PRV_CAMPAGNA.Where(funcFilter);
                    //if (!loadClosed)
                    //    tmp = tmp.Where(x => x.DTA_FINE == null || x.DTA_FINE.Value > DateTime.Today);
                    var rslt = tmp.ToList();

                    if (rslt != null && rslt.Any())
                    {
                        foreach (var c in rslt)
                        {
                            InfoCampagna res = new InfoCampagna();
                            res.Id = c.ID_CAMPAGNA;
                            res.NomeCampagna = c.NOME;
                            res.DettaglioCampagna = new List<DettaglioCampagna>();
                            res.DataInizio = c.DTA_INIZIO;
                            res.DataFine = c.DTA_FINE;

                            if (res.Id == 2)
                                res.NomeCampagna += " - Risorse chiave";

                            if (getDettaglioCampagna)
                            {
                                res.DettaglioCampagna = GetDettaglioCampagna(CommonHelper.GetCurrentUserMatricola(), c.ID_CAMPAGNA);
                                var decorrenze = GetDecorrenzeCampagna(c.ID_CAMPAGNA);
                                if (decorrenze != null && decorrenze.Any())
                                {
                                    res.Decorrenze.AddRange(decorrenze.OrderBy(w => w.DT_DECORRENZA).Distinct().Select(w => w.DT_DECORRENZA));
                                }
                            }
                            result.Add(res);
                        }

                        InfoCampagna all = new InfoCampagna();
                        all.Id = 0;
                        all.NomeCampagna = "Totale";
                        all.DettaglioCampagna = new List<DettaglioCampagna>();
                        result.Add(all);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Reperimento delle direzioni per una particolare area
        /// </summary>
        /// <param name="idArea"></param>
        /// <returns></returns>
        public List<PRV_DIREZIONE> GetDirezioniByIdArea(string currentMatricola, int idArea, int idCamp, int? anno = null)
        {
            List<PRV_DIREZIONE> result = new List<PRV_DIREZIONE>();

            try
            {
                using (var db = new IncentiviEntities())
                {
                    result = (from dir in db.XR_PRV_DIREZIONE
                              join campDir in db.XR_PRV_CAMPAGNA_DIREZIONE
                              on dir.ID_DIREZIONE equals campDir.ID_DIREZIONE
                              where dir.ID_AREA == idArea && campDir.ID_CAMPAGNA == idCamp
                              select new PRV_DIREZIONE()
                              {
                                  ID_AREA = idArea,
                                  ID_DIREZIONE = dir.ID_DIREZIONE,
                                  CODICE = dir.CODICE,
                                  NOME = dir.NOME,
                                  ORGANICO = campDir.ORGANICO,
                                  ORGANICO_FEMMINILE = campDir.ORGANICO_F,
                                  ORGANICO_MASCHILE = campDir.ORGANICO_M,
                                  BUDGET = campDir.BUDGET,
                                  Ordine = dir.ORDINE,
                                  ORGANICO_AD = campDir.ORGANICO_AD,
                                  ORGANICO_MASCHILE_AD = campDir.ORGANICO_M_AD,
                                  ORGANICO_FEMMINILE_AD = campDir.ORGANICO_F_AD,
                                  ORGANICO_CONTABILE = campDir.ORGANICO_INTERESSATO                                  
                              }).ToList();

                    IQueryable<XR_PRV_DIPENDENTI> dip = null;
                    if (anno.HasValue && anno.GetValueOrDefault() != DateTime.Now.Year)
                    {
                        dip = db.XR_PRV_DIPENDENTI.Where(w => w.ID_CAMPAGNA != null);
                        dip = dip.Where(w => w.ID_CAMPAGNA == idCamp);
                        dip = dip.Where(w => w.DECORRENZA != null);
                        dip = dip.Where(w => w.DECORRENZA.Value.Year == anno.Value);
                    }
                    else if (anno.HasValue && anno.GetValueOrDefault() == DateTime.Now.Year)
                    {
                        dip = db.XR_PRV_DIPENDENTI.Where(w => w.ID_CAMPAGNA != null);
                        dip = dip.Where(w => w.ID_CAMPAGNA == idCamp);
                        dip = dip.Where(w => (w.DECORRENZA != null && w.DECORRENZA.Value.Year == anno.Value) || w.DECORRENZA == null);
                    }

                    if (dip != null)
                        dip = dip.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

                    List<int> enableDir = null;
                    bool applyFilters = PoliticheRetributiveHelper.GetEnabledDirezioni(db, currentMatricola, out enableDir);

                    if (result != null && result.Any() && applyFilters && enableDir != null && enableDir.Any())
                    {
                        result.RemoveAll(w => !enableDir.Contains(w.ID_DIREZIONE));
                    }

                    if (dip != null && dip.Any())
                    {
                        List<int> idDirezioni = new List<int>();
                        idDirezioni = dip.Select(w => w.ID_DIREZIONE).Distinct().ToList();
                        result.RemoveAll(w => !idDirezioni.Contains(w.ID_DIREZIONE));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public List<PRV_DIREZIONE> GetDirezioniByIdAreaTOTALI(int idArea, List<int> idCamp, int? anno = null)
        {
            List<PRV_DIREZIONE> result = new List<PRV_DIREZIONE>();

            try
            {
                using (var db = new IncentiviEntities())
                {
                    List<PRV_DIREZIONE> tmp = new List<PRV_DIREZIONE>();

                    tmp = (from dir in db.XR_PRV_DIREZIONE
                           join campDir in db.XR_PRV_CAMPAGNA_DIREZIONE
                           on dir.ID_DIREZIONE equals campDir.ID_DIREZIONE
                           where dir.ID_AREA == idArea && idCamp.Contains(campDir.ID_CAMPAGNA)
                           select new PRV_DIREZIONE()
                           {
                               ID_AREA = idArea,
                               ID_DIREZIONE = dir.ID_DIREZIONE,
                               CODICE = dir.CODICE,
                               NOME = dir.NOME,
                               ORGANICO = campDir.ORGANICO,
                               ORGANICO_FEMMINILE = campDir.ORGANICO_F,
                               ORGANICO_MASCHILE = campDir.ORGANICO_M,
                               BUDGET = campDir.BUDGET,
                               BUDGET_PERIODO = campDir.BUDGET_PERIODO.Value,
                               Ordine = dir.ORDINE,
                               ORGANICO_AD = campDir.ORGANICO_AD,
                               ORGANICO_MASCHILE_AD = campDir.ORGANICO_M_AD,
                               ORGANICO_FEMMINILE_AD = campDir.ORGANICO_F_AD,
                               ORGANICO_CONTABILE = campDir.ORGANICO_INTERESSATO
                           }).ToList();

                    if (idCamp.Count > 1)
                    {

                        bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
                        bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);

                        if (enableQIO == enableRS)
                        {
                            tmp.AddRange((from dir in db.XR_PRV_DIREZIONE
                                          join campDir in db.XR_PRV_CAMPAGNA_DIREZIONE
                                          on dir.ID_DIREZIONE equals campDir.ID_DIREZIONE
                                          where dir.ID_AREA != idArea &&
                                          tmp.Any(z => z.CODICE.Equals(dir.CODICE)) &&
                                            idCamp.Contains(campDir.ID_CAMPAGNA)
                                          select new PRV_DIREZIONE()
                                          {
                                              ID_AREA = idArea,
                                              ID_DIREZIONE = dir.ID_DIREZIONE,
                                              CODICE = dir.CODICE,
                                              NOME = dir.NOME,
                                              ORGANICO = campDir.ORGANICO,
                                              ORGANICO_FEMMINILE = campDir.ORGANICO_F,
                                              ORGANICO_MASCHILE = campDir.ORGANICO_M,
                                              BUDGET = campDir.BUDGET,
                                              BUDGET_PERIODO = campDir.BUDGET_PERIODO.Value,
                                              Ordine = dir.ORDINE,
                                              ORGANICO_AD = campDir.ORGANICO_AD,
                                              ORGANICO_MASCHILE_AD = campDir.ORGANICO_M_AD,
                                              ORGANICO_FEMMINILE_AD = campDir.ORGANICO_F_AD,
                                              ORGANICO_CONTABILE = campDir.ORGANICO_INTERESSATO
                                          }));
                        }
                    }

                    List<XR_PRV_DIPENDENTI> dip = null;
                    if (anno.HasValue && anno.GetValueOrDefault() != DateTime.Now.Year)
                    {
                        dip = new List<XR_PRV_DIPENDENTI>();
                        dip = db.XR_PRV_DIPENDENTI.Where(w => w.ID_CAMPAGNA != null).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();
                        dip = dip.Where(w => idCamp.Contains(w.ID_CAMPAGNA.Value) && w.DECORRENZA != null).ToList();
                        dip = dip.Where(w => w.DECORRENZA.Value.Year == anno.Value).ToList();
                    }
                    else if (anno.HasValue && anno.GetValueOrDefault() == DateTime.Now.Year)
                    {
                        dip = new List<XR_PRV_DIPENDENTI>();
                        dip = db.XR_PRV_DIPENDENTI.Where(w => w.ID_CAMPAGNA != null).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();
                        dip = dip.Where(w => idCamp.Contains(w.ID_CAMPAGNA.Value)).ToList();
                        dip.RemoveAll(w => w.DECORRENZA.HasValue && w.DECORRENZA.Value.Year != anno.Value);
                    }

                    string matricola = CommonHelper.GetCurrentUserMatricola();
                    List<int> enableDir = null;
                    bool applyFilters = PoliticheRetributiveHelper.GetEnabledDirezioni(db, CommonHelper.GetCurrentUserMatricola(), out enableDir);

                    if (tmp != null && tmp.Any() && enableDir != null && enableDir.Any())
                    {
                        tmp.RemoveAll(w => !enableDir.Contains(w.ID_DIREZIONE));
                    }

                    if (dip != null && dip.Any())
                    {
                        List<int> idDirezioni = new List<int>();
                        idDirezioni = dip.Select(w => w.ID_DIREZIONE).Distinct().ToList();
                        tmp.RemoveAll(w => !idDirezioni.Contains(w.ID_DIREZIONE));
                    }

                    foreach (var groupDir in tmp.GroupBy(x => x.CODICE))
                    {
                        PRV_DIREZIONE dir = new PRV_DIREZIONE()
                        {
                            ID_AREA = idArea,
                            ID_DIREZIONE = groupDir.First().ID_DIREZIONE,
                            CODICE = groupDir.Key,
                            NOME = groupDir.First().NOME,
                            ORGANICO = groupDir.Sum(x => x.ORGANICO),
                            ORGANICO_MASCHILE = groupDir.Sum(x => x.ORGANICO_MASCHILE),
                            ORGANICO_FEMMINILE = groupDir.Sum(x => x.ORGANICO_FEMMINILE),
                            BUDGET = groupDir.Sum(x => x.BUDGET),
                            BUDGET_PERIODO = groupDir.Sum(x => x.BUDGET_PERIODO),
                            Ordine = groupDir.First().Ordine,
                            ORGANICO_AD = groupDir.First().ORGANICO_AD,
                            ORGANICO_MASCHILE_AD = groupDir.First().ORGANICO_MASCHILE_AD,
                            ORGANICO_FEMMINILE_AD = groupDir.First().ORGANICO_FEMMINILE_AD,
                            //ORGANICO_CONTABILE = groupDir.Sum(x=>x.ORGANICO_CONTABILE)
                            ORGANICO_CONTABILE = groupDir.First().ORGANICO_CONTABILE
                        };
                        result.Add(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public List<PRV_DIREZIONE> GetDirezioniByIdArea(string currentMatricola, int idArea, List<int> idCamp, int? anno = null)
        {
            List<PRV_DIREZIONE> result = new List<PRV_DIREZIONE>();

            try
            {
                using (var db = new IncentiviEntities())
                {
                    List<PRV_DIREZIONE> tmp = new List<PRV_DIREZIONE>();

                    tmp = (from dir in db.XR_PRV_DIREZIONE
                           join campDir in db.XR_PRV_CAMPAGNA_DIREZIONE
                           on dir.ID_DIREZIONE equals campDir.ID_DIREZIONE
                           where dir.ID_AREA == idArea && idCamp.Contains(campDir.ID_CAMPAGNA)
                           select new PRV_DIREZIONE()
                           {
                               ID_AREA = idArea,
                               ID_DIREZIONE = dir.ID_DIREZIONE,
                               CODICE = dir.CODICE,
                               NOME = dir.NOME,
                               ORGANICO = campDir.ORGANICO,
                               ORGANICO_FEMMINILE = campDir.ORGANICO_F,
                               ORGANICO_MASCHILE = campDir.ORGANICO_M,
                               BUDGET = campDir.BUDGET,
                               BUDGET_PERIODO = campDir.BUDGET_PERIODO.Value,
                               Ordine = dir.ORDINE,
                               ORGANICO_AD = campDir.ORGANICO_AD,
                               ORGANICO_MASCHILE_AD = campDir.ORGANICO_M_AD,
                               ORGANICO_FEMMINILE_AD = campDir.ORGANICO_F_AD,
                               ORGANICO_CONTABILE = campDir.ORGANICO_INTERESSATO
                           }).ToList();
                    List<XR_PRV_DIPENDENTI> dip = null;
                    if (anno.HasValue && anno.GetValueOrDefault() != DateTime.Now.Year)
                    {
                        dip = new List<XR_PRV_DIPENDENTI>();
                        dip = db.XR_PRV_DIPENDENTI.Where(w => w.ID_CAMPAGNA != null).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();
                        dip = dip.Where(w => idCamp.Contains(w.ID_CAMPAGNA.Value) && w.DECORRENZA != null).ToList();
                        dip = dip.Where(w => w.DECORRENZA.Value.Year == anno.Value).ToList();
                    }
                    else if (anno.HasValue && anno.GetValueOrDefault() == DateTime.Now.Year)
                    {
                        dip = new List<XR_PRV_DIPENDENTI>();
                        dip = db.XR_PRV_DIPENDENTI.Where(w => w.ID_CAMPAGNA != null).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();
                        dip = dip.Where(w => idCamp.Contains(w.ID_CAMPAGNA.Value)).ToList();
                        dip.RemoveAll(w => w.DECORRENZA.HasValue && w.DECORRENZA.Value.Year != anno.Value);
                    }

                    List<int> enableDir = null;
                    bool applyFilters = PoliticheRetributiveHelper.GetEnabledDirezioni(db, currentMatricola, out enableDir);

                    if (tmp != null && tmp.Any() && enableDir != null && enableDir.Any())
                    {
                        tmp.RemoveAll(w => !enableDir.Contains(w.ID_DIREZIONE));
                    }

                    if (dip != null && dip.Any())
                    {
                        List<int> idDirezioni = new List<int>();
                        idDirezioni = dip.Select(w => w.ID_DIREZIONE).Distinct().ToList();
                        tmp.RemoveAll(w => !idDirezioni.Contains(w.ID_DIREZIONE));
                    }

                    foreach (var groupDir in tmp.GroupBy(x => x.ID_DIREZIONE))
                    {
                        PRV_DIREZIONE dir = new PRV_DIREZIONE()
                        {
                            ID_AREA = idArea,
                            ID_DIREZIONE = groupDir.Key,
                            CODICE = groupDir.First().CODICE,
                            NOME = groupDir.First().NOME,
                            ORGANICO = groupDir.Sum(x => x.ORGANICO),
                            ORGANICO_MASCHILE = groupDir.Sum(x => x.ORGANICO_MASCHILE),
                            ORGANICO_FEMMINILE = groupDir.Sum(x => x.ORGANICO_FEMMINILE),
                            BUDGET = groupDir.Sum(x => x.BUDGET),
                            BUDGET_PERIODO = groupDir.Sum(x => x.BUDGET_PERIODO),
                            Ordine = groupDir.First().Ordine,
                            ORGANICO_AD = groupDir.First().ORGANICO_AD,
                            ORGANICO_MASCHILE_AD = groupDir.First().ORGANICO_MASCHILE_AD,
                            ORGANICO_FEMMINILE_AD = groupDir.First().ORGANICO_FEMMINILE_AD,
                            //ORGANICO_CONTABILE = groupDir.Sum(x=>x.ORGANICO_CONTABILE)
                            ORGANICO_CONTABILE = groupDir.First().ORGANICO_CONTABILE
                        };
                        result.Add(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Reperimento del numero di provvedimenti per la direzione e l'anno passati
        /// </summary>
        /// <param name="idDirezione"></param>
        /// <param name="anno"></param>
        /// <returns></returns>
        public int GetProvvedimentiByDirezione(int idDirezione, int anno, int? provvedimento = null)
        {
            int result = 0;
            List<XR_PRV_DIREZIONE_PROV> rstl = new List<XR_PRV_DIREZIONE_PROV>();

            try
            {
                using (var db = new IncentiviEntities())
                {
                    if (provvedimento == null)
                    {
                        rstl = db.XR_PRV_DIREZIONE_PROV.Where(w => w.ID_DIREZIONE.Equals(idDirezione) && w.ANNO.Equals(anno)).ToList();
                    }
                    else
                    {
                        int prov = provvedimento.GetValueOrDefault();
                        rstl = db.XR_PRV_DIREZIONE_PROV.Where(w => w.ID_DIREZIONE.Equals(idDirezione) && w.ANNO.Equals(anno) && w.ID_PROV.Equals(prov)).ToList();
                    }

                    if (rstl != null && rstl.Any())
                    {
                        result = rstl.Sum(w => w.NM_PROV);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public int GetProvvByDirezione(int idCampagna, string codDirezione, int? provvedimento)
        {
            int result = 0;

            using (IncentiviEntities db = new IncentiviEntities())
            {
                if (provvedimento.HasValue)
                {
                    var isThisProv = PoliticheRetributiveHelper.AnyOfProv(provvedimento.Value);
                    result = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna && x.SINTESI1.COD_SERVIZIO == codDirezione).Where(isThisProv).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).Count();
                }
                else
                {
                    result = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna && x.SINTESI1.COD_SERVIZIO == codDirezione).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).Count();
                }
            }

            return result;
        }

        public int GetSumProvvByDirezione(int idCampagna, int idDirezione, int? provvedimento, DateTime? decorrenza = null, bool senzaDecorrenza = false)
        {
            int result = 0;

            using (IncentiviEntities db = new IncentiviEntities())
            {
                if (provvedimento.HasValue)
                {
                    var isThisProv = PoliticheRetributiveHelper.AnyOfProv(provvedimento.Value);
                    if (decorrenza.HasValue)
                    {
                        var conDecorrenza = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna &&
                                                                x.ID_DIREZIONE.Equals(idDirezione) &&
                                                                x.DECORRENZA != null)
                                                                .Where(isThisProv)
                                                                .Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();

                        if (conDecorrenza != null && conDecorrenza.Any())
                        {
                            result = conDecorrenza.Count(w => w.DECORRENZA.Value.Date == decorrenza.Value.Date);
                        }
                    }
                    else
                    {

                        if (senzaDecorrenza)
                        {
                            var filtrati = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna &&
                                                                        x.ID_DIREZIONE.Equals(idDirezione) &&
                                                                        x.DECORRENZA == null)
                                                                        .Where(isThisProv)
                                                                        .Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();

                            if (filtrati != null && filtrati.Any())
                            {
                                result = filtrati.Count();
                            }
                        }
                        else
                        {
                            result = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna && x.ID_DIREZIONE.Equals(idDirezione)).Where(isThisProv).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).Count();
                        }
                    }
                }
                else
                {
                    if (decorrenza.HasValue)
                    {
                        var conDecorrenza = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna &&
                                                                        x.ID_DIREZIONE.Equals(idDirezione) &&
                                                                        x.DECORRENZA != null).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();

                        if (conDecorrenza != null && conDecorrenza.Any())
                        {
                            result = conDecorrenza.Count(w => w.DECORRENZA.Value.Date == decorrenza.Value.Date);
                        }
                    }
                    else
                    {
                        if (senzaDecorrenza)
                        {
                            var filtrati = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna &&
                                                x.ID_DIREZIONE.Equals(idDirezione) &&
                                                x.DECORRENZA == null).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();

                            if (filtrati != null && filtrati.Any())
                            {
                                result = filtrati.Count();
                            }
                        }
                        else
                        {
                            result = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna && x.ID_DIREZIONE.Equals(idDirezione)).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).Count();
                        }
                    }
                }
            }

            return result;
        }

        public int GetSumProvvByDirezione(int idCampagna, int idDirezione, Expression<Func<XR_PRV_DIPENDENTI, bool>> provvSelector, DateTime? decorrenza = null, bool senzaDecorrenza = false, int? annoDecorrenza = null)
        {
            int result = 0;

            using (IncentiviEntities db = new IncentiviEntities())
            {
                IQueryable<XR_PRV_DIPENDENTI> elencoDip = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna && x.ID_DIREZIONE.Equals(idDirezione));

                elencoDip = elencoDip.Where(provvSelector).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

                if (annoDecorrenza.HasValue && !senzaDecorrenza)
                {
                    elencoDip = elencoDip.Where(w => w.DECORRENZA != null);
                    elencoDip = elencoDip.Where(w => w.DECORRENZA.Value.Year == annoDecorrenza.Value);
                }
                else if (annoDecorrenza.HasValue && senzaDecorrenza)
                {
                    elencoDip = elencoDip.Where(w => (w.DECORRENZA != null && w.DECORRENZA.Value.Year == annoDecorrenza.Value) || w.DECORRENZA == null);
                }
                else if (decorrenza.HasValue)
                    elencoDip = elencoDip.Where(w => w.DECORRENZA.Value == decorrenza.Value);
                else if (senzaDecorrenza)
                    elencoDip = elencoDip.Where(x => x.DECORRENZA == null);
                result = elencoDip.Count();
            }

            return result;
        }


        public List<ReportProvDir> GetDistrProvvByDirezione(int idCampagna, IEnumerable<int> direzioni, DateTime? decorrenza = null, bool senzaDecorrenza = false, int? annoDecorrenza = null)
        {
            List<ReportProvDir> result = new List<ReportProvDir>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                var notNessuno = PoliticheRetributiveHelper.NotAnyOfProv();

                IQueryable<XR_PRV_DIPENDENTI> elencoDip = db.XR_PRV_DIPENDENTI
                    .Where(x => x.ID_CAMPAGNA == idCampagna && direzioni.Contains(x.ID_DIREZIONE))
                    .Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa))
                    .Where(notNessuno)
                    ;

                if (annoDecorrenza.HasValue && !senzaDecorrenza)
                {
                    elencoDip = elencoDip.Where(w => w.DECORRENZA != null);
                    elencoDip = elencoDip.Where(w => w.DECORRENZA.Value.Year == annoDecorrenza.Value);
                }
                else if (annoDecorrenza.HasValue && senzaDecorrenza)
                {
                    elencoDip = elencoDip.Where(w => (w.DECORRENZA != null && w.DECORRENZA.Value.Year == annoDecorrenza.Value) || w.DECORRENZA == null);
                }
                else if (decorrenza.HasValue)
                    elencoDip = elencoDip.Where(w => w.DECORRENZA.Value == decorrenza.Value);
                else if (senzaDecorrenza)
                    elencoDip = elencoDip.Where(x => x.DECORRENZA == null);

                var provSelector = PoliticheRetributiveHelper.GetDipProvEff();

                result = elencoDip.Select(provSelector).Where(x => x != null)
                    .GroupBy(x => new { x.XR_PRV_DIPENDENTI.ID_DIREZIONE, x.XR_PRV_PROV.SIGLA })
                    .Select(x=>new { x.Key.ID_DIREZIONE, x.Key.SIGLA, NumElem = x.Count()})
                    .ToList()
                    .GroupBy(x=>x.ID_DIREZIONE)
                    .Select(x => new ReportProvDir
                    {
                        IdDir = x.Key,
                        Report = x.ToDictionary(z => z.SIGLA, z => z.NumElem)
                    }).ToList();
                   
                    
            }

            return result;
        }

        public int GetSumProvvByDirezione(List<int> idCampagna, int idDirezione, Expression<Func<XR_PRV_DIPENDENTI, bool>> provvSelector, DateTime? decorrenza = null, bool senzaDecorrenza = false, int? annoDecorrenza = null)
        {
            int result = 0;

            using (IncentiviEntities db = new IncentiviEntities())
            {
                var elencoDip = db.XR_PRV_DIPENDENTI.Where(x => idCampagna.Contains(x.ID_CAMPAGNA.Value) && x.ID_DIREZIONE.Equals(idDirezione));
                elencoDip = elencoDip.Where(provvSelector);
                if (decorrenza.HasValue)
                    elencoDip = elencoDip.Where(w => w.DECORRENZA.Value == decorrenza.Value);
                else if (senzaDecorrenza)
                    elencoDip = elencoDip.Where(x => x.DECORRENZA == null);

                if (annoDecorrenza.HasValue)
                {
                    elencoDip = elencoDip.Where(w => w.DECORRENZA != null);
                    elencoDip = elencoDip.Where(w => w.DECORRENZA.Value.Year == annoDecorrenza.Value);
                }

                elencoDip = elencoDip.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

                result = elencoDip.Count();
            }

            return result;
        }


        public int GetSumProvvConsolidatiByDirezione(int idCampagna, int idDirezione, int? provvedimento, DateTime? decorrenza = null, bool senzaDecorrenza = false)
        {
            int conteggio = 0;
            List<XR_PRV_DIPENDENTI> result = new List<XR_PRV_DIPENDENTI>();
            List<int> dipendentiConsolidati = new List<int>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                if (provvedimento.HasValue)
                {
                    var isThisProv = PoliticheRetributiveHelper.AnyOfProv(provvedimento.Value);
                    if (decorrenza.HasValue)
                    {
                        var conDecorrenza = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna &&
                                                                x.ID_DIREZIONE.Equals(idDirezione) &&
                                                                x.DECORRENZA != null)
                                                                .Where(isThisProv)
                                                                .Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();

                        if (conDecorrenza != null && conDecorrenza.Any())
                        {
                            result = conDecorrenza.Where(w => w.DECORRENZA.Value.Date == decorrenza.Value.Date).ToList();
                        }
                    }
                    else
                    {

                        if (senzaDecorrenza)
                        {
                            result = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna &&
                                                                        x.ID_DIREZIONE.Equals(idDirezione) &&
                                                                        x.DECORRENZA == null).Where(isThisProv).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();
                        }
                        else
                        {
                            result = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna &&
                                                                x.ID_DIREZIONE.Equals(idDirezione))
                                                                .Where(isThisProv).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();
                        }
                    }
                }
                else
                {
                    if (decorrenza.HasValue)
                    {
                        result = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna &&
                                                                        x.ID_DIREZIONE.Equals(idDirezione) &&
                                                                        x.DECORRENZA != null).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();
                    }
                    else
                    {
                        if (senzaDecorrenza)
                        {
                            result = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna &&
                                                x.ID_DIREZIONE.Equals(idDirezione) &&
                                                x.DECORRENZA == null).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();
                        }
                        else
                        {
                            result = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna && x.ID_DIREZIONE.Equals(idDirezione)).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();
                        }
                    }
                }
                DateTime current = DateTime.Now;

                dipendentiConsolidati = db.XR_PRV_OPERSTATI.Where(w => w.ID_STATO == (int)OperStatiEnum.Consolidato && w.DATA.Year == current.Year).Select(y => y.ID_DIPENDENTE).ToList();
            }

            if (dipendentiConsolidati != null && dipendentiConsolidati.Any())
            {
                return result.Count(w => dipendentiConsolidati.Contains(w.ID_DIPENDENTE));
            }
            else
            {
                return 0;
            }
        }

        public int GetSumProvvConsolidatiByDirezione(int idCampagna, int idDirezione, Expression<Func<XR_PRV_DIPENDENTI, bool>> provvSelector, DateTime? decorrenza = null, bool senzaDecorrenza = false)
        {
            int conteggio = 0;
            List<XR_PRV_DIPENDENTI> result = new List<XR_PRV_DIPENDENTI>();
            List<int> dipendentiConsolidati = new List<int>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                var elencoDip = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna && x.ID_DIREZIONE.Equals(idDirezione));
                elencoDip = elencoDip.Where(provvSelector);
                if (decorrenza.HasValue)
                    elencoDip = elencoDip.Where(x => x.DECORRENZA.Value == decorrenza.Value);
                else if (senzaDecorrenza)
                    elencoDip = elencoDip.Where(x => x.DECORRENZA == null);

                elencoDip = elencoDip.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

                conteggio = elencoDip.Count(x => x.XR_PRV_OPERSTATI.Where(y => y.DATA_FINE_VALIDITA == null).Max(z => z.ID_STATO) == (int)OperStatiEnum.Consolidato);
            }

            return conteggio;
        }

        /// <summary>
        /// Reperimento del numero di provvedimenti che rispondono ai filtri impostati
        /// </summary>
        /// <param name="idCampagna">Identificativo della campagna alla quale appartengono i dipendenti</param>
        /// <param name="provvSelector">Provvedimento/i da cercare</param>
        /// <param name="senzaDecorrenza">Se true cerca anche i dipendenti con decorrenza null</param>
        /// <param name="annoDecorrenza">se impostato filtra per anno</param>
        /// <returns></returns>
        public int GetSumProvvedimenti(int idCampagna, Expression<Func<XR_PRV_DIPENDENTI, bool>> provvSelector, bool senzaDecorrenza = false, int? annoDecorrenza = null)
        {
            int result = 0;

            using (IncentiviEntities db = new IncentiviEntities())
            {
                IQueryable<XR_PRV_DIPENDENTI> elencoDip = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna);

                elencoDip = elencoDip.Where(provvSelector);

                if (annoDecorrenza.HasValue && !senzaDecorrenza)
                {
                    elencoDip = elencoDip.Where(w => w.DECORRENZA != null && w.DECORRENZA.Value.Year == annoDecorrenza.Value);
                }
                else if (annoDecorrenza.HasValue && senzaDecorrenza)
                {
                    elencoDip = elencoDip.Where(w => w.DECORRENZA == null || (w.DECORRENZA != null && w.DECORRENZA.Value.Year == annoDecorrenza.Value));
                }
                else if (!annoDecorrenza.HasValue && senzaDecorrenza)
                {
                    elencoDip = elencoDip.Where(w => w.DECORRENZA == null);
                }

                elencoDip = elencoDip.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

                result = elencoDip.Count();
            }

            return result;
        }



        /// <summary>
        /// Calcola il costo annuo di una campagna
        /// </summary>
        /// <param name="idCampagna">Identificativo univoco della campagna</param>
        /// <param name="senzaDecorrenza">Se true, considera anche i dipendenti senza data di decorrenza</param>
        /// <param name="annoDecorrenza">Filtro sull'anno di decorrenza, se null prende tutti</param>
        /// <returns></returns>
        public decimal GetCostoAnnoCampagna(int idCampagna, bool senzaDecorrenza = false, int? annoDecorrenza = null)
        {
            decimal result = 0;
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    // prende i dipendenti di quella campagna
                    IQueryable<XR_PRV_DIPENDENTI> elencoDip = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna);

                    // prende i dipendenti con un provvedimento
                    var anyProv = PoliticheRetributiveHelper.AnyOfProv();
                    elencoDip = elencoDip.Where(anyProv);

                    if (annoDecorrenza.HasValue && !senzaDecorrenza)
                    {
                        elencoDip = elencoDip.Where(w => w.DECORRENZA != null && w.DECORRENZA.Value.Year == annoDecorrenza.Value);
                    }
                    else if (annoDecorrenza.HasValue && senzaDecorrenza)
                    {
                        elencoDip = elencoDip.Where(w => w.DECORRENZA == null || (w.DECORRENZA != null && w.DECORRENZA.Value.Year == annoDecorrenza.Value));
                    }
                    else if (!annoDecorrenza.HasValue && senzaDecorrenza)
                    {
                        elencoDip = elencoDip.Where(w => w.DECORRENZA == null);
                    }

                    elencoDip = elencoDip.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

                    if (elencoDip != null && elencoDip.Any())
                    {
                        var anyDipProvEff = PoliticheRetributiveHelper.AnyDipProvEff();
                        elencoDip = elencoDip.Where(anyDipProvEff);
                    }

                    if (elencoDip != null && elencoDip.Any())
                    {
                        var funcProvEff = PoliticheRetributiveHelper.GetDipProvEff();
                        result = elencoDip.Select(funcProvEff).Sum(a => a.COSTO_ANNUO);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }




        /// <summary>
        /// Reperimento dei provvedimenti selezionabili
        /// </summary>
        /// <returns></returns>
        public List<XR_PRV_PROV> GetProvvedimenti()
        {
            List<XR_PRV_PROV> result = new List<XR_PRV_PROV>();
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    result = db.XR_PRV_PROV.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public XR_PRV_PROV GetProvvedimento(int idProvvedimento)
        {
            XR_PRV_PROV result = new XR_PRV_PROV();
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    result = db.XR_PRV_PROV.Where(w => w.ID_PROV.Equals(idProvvedimento)).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public XR_PRV_DIREZIONE GetDirezione(int id)
        {
            XR_PRV_DIREZIONE result = new XR_PRV_DIREZIONE();
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    //result = db.XR_PRV_DIREZIONE.Where(w => w.ID_DIREZIONE.Equals(id)).FirstOrDefault();
                    result = db.XR_PRV_DIREZIONE.Find(id);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public XR_PRV_AREA GetArea(int id)
        {
            XR_PRV_AREA result = new XR_PRV_AREA();
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    result = db.XR_PRV_AREA.Where(w => w.ID_AREA.Equals(id)).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public decimal GetBudgetAreaByDirezione(int idDirezione, int idCamp)
        {
            return GetBudgetAreaByDirezione(idDirezione, new List<int> { idCamp });
        }

        public decimal GetBudgetAreaByDirezione(int idDirezione, List<int> idCamps)
        {
            decimal result = 0;
            try
            {
                XR_PRV_DIREZIONE direzione = this.GetDirezione(idDirezione);

                if (direzione != null)
                {
                    XR_PRV_AREA area = this.GetArea(direzione.ID_AREA);
                    if (area != null)
                    {
                        using (IncentiviEntities db = new IncentiviEntities())
                        {
                            var bud = db.XR_PRV_CAMPAGNA_BUDGET.Where(w => idCamps.Contains(w.ID_CAMPAGNA) && w.ID_AREA.Equals(area.ID_AREA));
                            if (bud != null && bud.Count() > 0)
                            {
                                result = bud.Sum(x => x.BUDGET);
                            }

                            bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
                            bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);
                            if (enableQIO == enableRS && idCamps.Count() > 1)
                            {
                                var listCodici = db.XR_PRV_AREA.FirstOrDefault(x => x.ID_AREA == direzione.ID_AREA).XR_PRV_DIREZIONE.Select(x => x.CODICE);
                                result += db.XR_PRV_CAMPAGNA_DIREZIONE.Where(x => x.XR_PRV_CAMPAGNA.LV_ABIL == PoliticheRetributiveHelper.BUDGETRS_HRGA_SOTTO_FUNC
                                 && idCamps.Contains(x.ID_CAMPAGNA)
                                 && listCodici.Contains(x.XR_PRV_DIREZIONE.CODICE)).Sum(x => x.BUDGET);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public decimal GetBudgetDirezione(int idDirezione, int idCamp)
        {
            return GetBudgetDirezione(idDirezione, new List<int> { idCamp });
        }
        public decimal GetBudgetDirezione(int idDirezione, List<int> idCamps)
        {
            decimal result = 0;

            try
            {
                using (var db = new IncentiviEntities())
                {
                    bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
                    bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);
                    int idDirezioneRS = 0;
                    if (enableQIO == enableRS && idCamps.Count() > 1)
                    {
                        XR_PRV_DIREZIONE direzione = this.GetDirezione(idDirezione);
                        var dirRS = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.ID_DIREZIONE != direzione.ID_DIREZIONE && x.CODICE == direzione.CODICE);
                        idDirezioneRS = dirRS.ID_DIREZIONE;
                    }

                    var item = db.XR_PRV_CAMPAGNA_DIREZIONE.Where(w => idCamps.Contains(w.ID_CAMPAGNA) && w.ID_DIREZIONE.Equals(idDirezione) || w.ID_DIREZIONE.Equals(idDirezioneRS));

                    if (item != null && item.Count() > 0)
                    {
                        result = item.Sum(x => x.BUDGET);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public decimal GetBudgetPeriodoDirezione(int idDirezione, int idCamp)
        {
            decimal result = 0;

            try
            {
                using (var db = new IncentiviEntities())
                {
                    var item = db.XR_PRV_CAMPAGNA_DIREZIONE.Where(w => w.ID_CAMPAGNA.Equals(idCamp) && w.ID_DIREZIONE.Equals(idDirezione)).FirstOrDefault();

                    if (item != null)
                    {
                        result = item.BUDGET_PERIODO.GetValueOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public void GetBudgetAnnuoPeriodoDirezione(int idDirezione, int idCamp, out decimal budgetAnnuo, out decimal budgetPeriodo)
        {
            budgetAnnuo = 0;
            budgetPeriodo = 0;

            try
            {
                using (var db = new IncentiviEntities())
                {
                    var item = db.XR_PRV_CAMPAGNA_DIREZIONE.Where(w => w.ID_CAMPAGNA.Equals(idCamp) && w.ID_DIREZIONE.Equals(idDirezione)).FirstOrDefault();

                    if (item != null)
                    {
                        budgetAnnuo = item.BUDGET;
                        budgetPeriodo = item.BUDGET_PERIODO.GetValueOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void GetBudgetAnnuoPeriodoDirezione(int idDirezione, List<int> idCamp, out decimal budgetAnnuo, out decimal budgetPeriodo)
        {
            budgetAnnuo = 0;
            budgetPeriodo = 0;

            try
            {
                using (var db = new IncentiviEntities())
                {
                    bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
                    bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);
                    bool sumRS = enableQIO == enableRS;
                    int idDirRS = 0;
                    if (sumRS)
                    {
                        var dir = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.ID_DIREZIONE == idDirezione);
                        idDirRS = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.ID_DIREZIONE != idDirezione && x.CODICE == dir.CODICE).ID_DIREZIONE;
                    }


                    var item = db.XR_PRV_CAMPAGNA_DIREZIONE.Where(w => idCamp.Contains(w.ID_CAMPAGNA) && (w.ID_DIREZIONE.Equals(idDirezione) || w.ID_DIREZIONE.Equals(idDirRS))).ToList();

                    if (item != null)
                    {
                        budgetAnnuo = item.Sum(x => x.BUDGET);
                        budgetPeriodo = item.Sum(x => x.BUDGET_PERIODO.GetValueOrDefault());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void GetBudgetAnnuoPeriodoCampagna(InfoCampagna infoCampagna, out decimal budgetAnnuo, out decimal budgetPeriodo)
        {
            budgetAnnuo = 0;
            budgetPeriodo = 0;

            try
            {
                using (var db = new IncentiviEntities())
                {
                    List<XR_PRV_CAMPAGNA_BUDGET> items = null;
                    if (infoCampagna.CampagneContenute.Count() == 0)
                        items = db.XR_PRV_CAMPAGNA_BUDGET.Where(w => w.ID_CAMPAGNA.Equals(infoCampagna.Id)).ToList();
                    else
                        items = db.XR_PRV_CAMPAGNA_BUDGET.Where(w => infoCampagna.CampagneContenute.Contains(w.ID_CAMPAGNA)).ToList();

                    if (items != null && items.Any())
                    {
                        budgetAnnuo = items.Sum(w => w.BUDGET);
                        budgetPeriodo = items.Sum(w => w.BUDGET_PERIODO.GetValueOrDefault());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void GetCostoAnnuoPeriodoCampagna(InfoCampagna infoCampagna, out decimal costoAnnuo, out decimal costoPeriodo, int? anno = null)
        {
            costoAnnuo = 0;
            costoPeriodo = 0;
            try
            {
                using (var db = new IncentiviEntities())
                {
                    int? annoRifCampagna = null;
                    if (anno.HasValue)
                    {
                        if (infoCampagna.CampagneContenute.Count() == 0)
                            annoRifCampagna = db.XR_PRV_CAMPAGNA.Where(x => x.ID_CAMPAGNA == infoCampagna.Id).Select(x => x.DTA_INIZIO.Value.Year).First();
                        else
                            annoRifCampagna = db.XR_PRV_CAMPAGNA.Where(x => infoCampagna.CampagneContenute.Contains(x.ID_CAMPAGNA)).Select(x => x.DTA_INIZIO.Value.Year).First();
                    }

                    IQueryable<XR_PRV_DIPENDENTI> uss = null;
                    if (infoCampagna.CampagneContenute.Count() == 0)
                        uss = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == infoCampagna.Id);
                    else
                        uss = db.XR_PRV_DIPENDENTI.Where(x => infoCampagna.CampagneContenute.Contains(x.ID_CAMPAGNA.Value));

                    if (anno.HasValue && anno.GetValueOrDefault() != DateTime.Now.Year)
                    {
                        uss = uss.Where(x => x.DECORRENZA != null);
                        uss = uss.Where(x => x.DECORRENZA.Value.Year == anno.Value);
                    }
                    else if (anno.HasValue && anno.GetValueOrDefault() == DateTime.Now.Year)
                    {
                        uss = uss.Where(x => (x.DECORRENZA != null && x.DECORRENZA.Value.Year == anno.Value) || x.DECORRENZA == null);
                    }

                    var anyProvEff = PoliticheRetributiveHelper.AnyDipProvEff();
                    uss = uss.Where(anyProvEff);

                    uss = uss.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

                    if (uss != null && uss.Any())
                    {
                        var funcProvEff = PoliticheRetributiveHelper.GetDipProvEff();
                        var tmp = uss.Select(funcProvEff);
                        costoAnnuo = tmp.Sum(a => a.COSTO_ANNUO);

                        if (!anno.HasValue || anno.GetValueOrDefault() == annoRifCampagna)
                            costoPeriodo = tmp.Sum(a => a.COSTO_PERIODO);
                        else
                        {
                            costoPeriodo = 0;
                            var promOrAumento = PoliticheRetributiveHelper.GetProvBySigla(db, PoliticheRetributiveHelper.SIGLA_PROMOZIONI, PoliticheRetributiveHelper.SIGLA_AUMENTI);
                            costoPeriodo += tmp.Where(x => promOrAumento.Contains(x.ID_PROV))
                                            .Select(x => new { costoAnnuo = x.COSTO_ANNUO, dec = x.XR_PRV_DIPENDENTI.DECORRENZA.Value }).ToList()
                                            .Select(y => y.costoAnnuo / 14 * (decimal)(13 - y.dec.Month + (y.dec.Month < 7 ? 2.5 : 1.5)))
                                            .Sum();

                            var gratifica = PoliticheRetributiveHelper.GetProvBySigla(db, PoliticheRetributiveHelper.SIGLA_GRATIFICHE);
                            foreach (var grat in tmp.Where(x => gratifica.Contains(x.ID_PROV)))
                            {
                                decimal aliq = HRDWData.GetProvvAliq(db, (int)ProvvedimentiEnum.Gratifica, grat.XR_PRV_DIPENDENTI.SINTESI1.COD_QUALIFICA);
                                costoPeriodo += grat.DIFF_RAL + (grat.DIFF_RAL * Convert.ToDecimal(aliq) / 100);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<PRV_Dipendente> GetDipendentiInDirezione(int idDirezione, int idCampagna, int? anno = null)
        {
            return GetDipendentiInDirezione(idDirezione, new List<int> { idCampagna }, anno);
        }


        /// <summary>
        /// Reperimento dei dipendenti di tutte le campagne attive
        /// per l'anno indicato
        /// </summary>
        /// <param name="annoCampagna"></param>
        /// <param name="anno"></param>
        /// <param name="onlyDecorrenza">Se true esclude i dipendenti con decorrenza null</param>
        /// <returns></returns>
        public List<PRV_Dipendente> GetDipendentiAllCampagne(int annoCampagna, int? anno = null, bool onlyDecorrenza = false, List<int> idsCampagne = null)
        {
            List<PRV_Dipendente> output = new List<PRV_Dipendente>();
            List<PRV_Dipendente> res = new List<PRV_Dipendente>();
            List<XR_PRV_CAMPAGNA> campagne = new List<XR_PRV_CAMPAGNA>();
            List<int> anni = new List<int>();
            List<int> idCampagne = new List<int>();
            try
            {
                // reperimento delle campagne alle quali l'utente corrente è abilitato
                using (var db = new IncentiviEntities())
                {
                    var funcFilter = PoliticheRetributiveHelper.FuncFilterCampagna();
                    var tmpCampagne = db.XR_PRV_CAMPAGNA.Where(funcFilter).Where(w => w.DTA_INIZIO != null && w.DTA_INIZIO.Value.Year == annoCampagna);
                    if (idsCampagne != null && idsCampagne.Any())
                        tmpCampagne = tmpCampagne.Where(w => idsCampagne.Contains(w.ID_CAMPAGNA));
                    campagne = tmpCampagne.ToList();

                    if (campagne != null && campagne.Any())
                    {
                        idCampagne = campagne.Select(w => w.ID_CAMPAGNA).ToList();
                        res = GetListPrvDipendente(db, idCampagne, true);

                        if (res != null && res.Any())
                        {
                            if (onlyDecorrenza && !anno.HasValue)
                            {
                                res = res.Where(w => w.Decorrenza != null).ToList();
                            }
                            else if (onlyDecorrenza && anno.HasValue)
                            {
                                res = res.Where(w => w.Decorrenza != null).ToList();
                                res = res.Where(w => w.Decorrenza.Value.Year == anno).ToList();
                            }
                            else if (!onlyDecorrenza && !anno.HasValue)
                            {
                                res = res.ToList();
                            }
                            else if (!onlyDecorrenza && anno.HasValue)
                            {
                                res = res.Where(w => (w.Decorrenza == null || (w.Decorrenza != null && w.Decorrenza.Value.Year == anno))).ToList();
                            }
                        }

                    }
                }

                if (res != null && res.Any())
                {
                    res.ForEach(w =>
                   {
                       output.Add(GetDatiDipendente(w));
                   });
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return output;
        }

        private static List<PRV_Dipendente> GetListPrvDipendente(IncentiviEntities db, IEnumerable<int> idCampagne, bool filterDir, int? anno = null, int? idDirezione = null, bool loadDirRs = true, int? idDipendente = null, bool lightSelect = false)
        {
            DbQuery<XR_PRV_DIPENDENTI> dbQuery = db.XR_PRV_DIPENDENTI;
            if (!lightSelect)
                dbQuery = dbQuery.Include("SINTESI1");

            var tmp = dbQuery
                            .Where(dip => idCampagne.Contains(dip.ID_CAMPAGNA.Value))
                            .Where(dip => dip.ID_CAMPAGNA.Value > 2 || !dip.XR_PRV_OPERSTATI.Any(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE && x.ID_STATO == (int)ProvvStatoEnum.Conclusa));

            if (filterDir)
            {
                List<int> idDIR = new List<int>();
                var funcFilterDirezione = PoliticheRetributiveHelper.GetEnabledDirezioni(db, CommonHelper.GetCurrentUserMatricola(), out idDIR);

                if (funcFilterDirezione)
                    tmp = tmp.Where(dip => idDIR.Contains(dip.ID_DIREZIONE));
            }

            if (idDipendente.HasValue)
                tmp = tmp.Where(dip => dip.ID_DIPENDENTE == idDipendente);

            if (idDirezione.HasValue)
            {
                int idDirezioneRS = 0;
                if (loadDirRs)
                {
                    var direzione = db.XR_PRV_DIREZIONE.Find(idDirezione);
                    bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
                    bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);
                    if (enableQIO == enableRS && idCampagne.Count() > 1)
                    {
                        var dirRS = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.ID_DIREZIONE != direzione.ID_DIREZIONE && x.CODICE == direzione.CODICE);
                        idDirezioneRS = dirRS.ID_DIREZIONE;
                    }
                }

                tmp = tmp.Where(dip => dip.ID_DIREZIONE == idDirezione || dip.ID_DIREZIONE == idDirezioneRS);
            }

            if (anno.HasValue && anno.GetValueOrDefault() != DateTime.Now.Year)
                tmp = tmp.Where(dip => (dip.DECORRENZA.HasValue && dip.DECORRENZA.Value.Year == anno.Value));
            else if (anno.HasValue && anno.GetValueOrDefault() == DateTime.Now.Year)
                tmp = tmp.Where(dip => !dip.DECORRENZA.HasValue || (dip.DECORRENZA.HasValue && dip.DECORRENZA.Value.Year == anno.Value));

            bool usePrvVariazioniFlag = HrisHelper.GetParametro<string>(HrisParam.PRetribPrvSuVariazioni) == "TRUE";
            IQueryable<PRV_Dipendente> tmpResult = null;
            if (!lightSelect)
            {
                if (!usePrvVariazioniFlag)
                    tmpResult = tmp.Select(dip => new PRV_Dipendente()
                    {
                        Nominativo = dip.SINTESI1.DES_COGNOMEPERS + (String.IsNullOrEmpty(dip.SINTESI1.DES_SECCOGNOME) ? "" : " " + dip.SINTESI1.DES_SECCOGNOME) + " " + dip.SINTESI1.DES_NOMEPERS,
                        Matricola = dip.SINTESI1.COD_MATLIBROMAT,
                        IdPersona = dip.SINTESI1.ID_PERSONA,
                        IdDipendente = dip.ID_DIPENDENTE,
                        CostoAnnuo = 0,
                        CostoPeriodo = 0,
                        CostoConStraordinario = 0,
                        IdProvvedimento = dip.ID_PROV_EFFETTIVO.Value,
                        CustomProvv = (dip.CUSTOM_PROV.HasValue ? dip.CUSTOM_PROV.Value : false),
                        Decorrenza = dip.DECORRENZA,
                        CodRuolo = dip.SINTESI1.COD_RUOLO,
                        DescRuolo = dip.SINTESI1.DES_RUOLO,
                        Struttura = dip.SINTESI1.DES_DENOMUNITAORG,
                        LivAttuale = dip.LIV_ATTUALE,
                        DataNascita = dip.SINTESI1.DTA_NASCITAPERS,
                        DataAssunzione = dip.SINTESI1.DTA_ANZCONV,
                        AggregatoSede = dip.DES_SEDE,
                        PartTime = dip.PART_TIME,
                        IdAssQual = dip.SINTESI1.ID_ASSQUAL,
                        IdProvvedimentoRich = dip.ID_PROV_RICH,
                        Cod_Qualifica = dip.SINTESI1.COD_QUALIFICA,
                        Des_Qualifica = dip.SINTESI1.DES_QUALIFICA,
                        IdDirezione = dip.ID_DIREZIONE,
                        IdArea = null,
                        IdCampagna = dip.ID_CAMPAGNA
                    });
                else
                    tmpResult = tmp.Select(dip => new PRV_Dipendente()
                    {
                        Nominativo = dip.SINTESI1.DES_COGNOMEPERS + (String.IsNullOrEmpty(dip.SINTESI1.DES_SECCOGNOME) ? "" : " " + dip.SINTESI1.DES_SECCOGNOME) + " " + dip.SINTESI1.DES_NOMEPERS,
                        Matricola = dip.SINTESI1.COD_MATLIBROMAT,
                        IdPersona = dip.SINTESI1.ID_PERSONA,
                        IdDipendente = dip.ID_DIPENDENTE,
                        CostoAnnuo = 0,
                        CostoPeriodo = 0,
                        CostoConStraordinario = 0,
                        IdProvvedimento =
                            dip.XR_PRV_DIPENDENTI_VARIAZIONI.Any(x => x.IND_PRV_EFFETTIVO.HasValue && x.IND_PRV_EFFETTIVO.Value) ?
                                dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.IND_PRV_EFFETTIVO.HasValue && x.IND_PRV_EFFETTIVO.Value).ID_PROV :
                                    (dip.CUSTOM_PROV.HasValue && dip.CUSTOM_PROV.Value ?
                                            (int)ProvvedimentiEnum.CUSNessuno
                                            : (int)ProvvedimentiEnum.Nessuno),
                        CustomProvv = (dip.CUSTOM_PROV.HasValue ? dip.CUSTOM_PROV.Value : false),
                        Decorrenza = dip.DECORRENZA,
                        CodRuolo = dip.SINTESI1.COD_RUOLO,
                        DescRuolo = dip.SINTESI1.DES_RUOLO,
                        Struttura = dip.SINTESI1.DES_DENOMUNITAORG,
                        LivAttuale = dip.LIV_ATTUALE,
                        DataNascita = dip.SINTESI1.DTA_NASCITAPERS,
                        DataAssunzione = dip.SINTESI1.DTA_ANZCONV,
                        AggregatoSede = dip.DES_SEDE,
                        PartTime = dip.PART_TIME,
                        IdAssQual = dip.SINTESI1.ID_ASSQUAL,
                        IdProvvedimentoRich = dip.ID_PROV_RICH,
                        Cod_Qualifica = dip.SINTESI1.COD_QUALIFICA,
                        Des_Qualifica = dip.SINTESI1.DES_QUALIFICA,
                        IdDirezione = dip.ID_DIREZIONE,
                        IdArea = null,
                        IdCampagna = dip.ID_CAMPAGNA
                    });
            }
            else
            {
                tmpResult = tmp.Select(dip => new PRV_Dipendente()
                {
                    Matricola = dip.MATRICOLA,
                    IdPersona = dip.ID_PERSONA,
                    IdDipendente = dip.ID_DIPENDENTE,
                    CostoAnnuo = 0,
                    CostoPeriodo = 0,
                    CostoConStraordinario = 0,
                    IdProvvedimento = dip.ID_PROV_EFFETTIVO.Value,
                    CustomProvv = (dip.CUSTOM_PROV.HasValue ? dip.CUSTOM_PROV.Value : false),
                    Decorrenza = dip.DECORRENZA,
                    LivAttuale = dip.LIV_ATTUALE,
                    AggregatoSede = dip.DES_SEDE,
                    PartTime = dip.PART_TIME,
                    IdProvvedimentoRich = dip.ID_PROV_RICH,
                    IdDirezione = dip.ID_DIREZIONE,
                    IdArea = null,
                    IdCampagna = dip.ID_CAMPAGNA
                });
            }

            return tmpResult.ToList();
        }

        /// <summary>
        /// Reperimento dei dipendenti di tutte le campagne attive
        /// per l'anno indicato
        /// </summary>
        /// <param name="annoCampagna"></param>
        /// <param name="anno"></param>
        /// <param name="onlyDecorrenza">Se true esclude i dipendenti con decorrenza null</param>
        /// <returns></returns>
        public List<PRV_Dipendente> GetDipendentiCampagna(int idCampagna, int annoCampagna, int? anno = null, bool onlyDecorrenza = false)
        {
            List<PRV_Dipendente> output = new List<PRV_Dipendente>();
            List<PRV_Dipendente> res = new List<PRV_Dipendente>();
            List<XR_PRV_CAMPAGNA> campagne = new List<XR_PRV_CAMPAGNA>();
            List<int> anni = new List<int>();
            List<int> idCampagne = new List<int>();
            try
            {
                // reperimento delle campagne alle quali l'utente corrente è abilitato
                using (var db = new IncentiviEntities())
                {
                    var funcFilter = PoliticheRetributiveHelper.FuncFilterCampagna();
                    campagne = db.XR_PRV_CAMPAGNA.Where(funcFilter).ToList();
                    campagne.RemoveAll(w => w.ID_CAMPAGNA != idCampagna);
                    campagne = campagne.Where(w => w.DTA_INIZIO != null && w.DTA_INIZIO.Value.Year == annoCampagna).ToList();

                    if (campagne != null && campagne.Any())
                    {
                        idCampagne = campagne.Select(w => w.ID_CAMPAGNA).ToList();
                        res = GetListPrvDipendente(db, idCampagne, true);

                        if (res != null && res.Any())
                        {
                            if (onlyDecorrenza && !anno.HasValue)
                            {
                                res = res.Where(w => w.Decorrenza != null).ToList();
                            }
                            else if (onlyDecorrenza && anno.HasValue)
                            {
                                res = res.Where(w => w.Decorrenza != null).ToList();
                                res = res.Where(w => w.Decorrenza.Value.Year == anno).ToList();
                            }
                            else if (!onlyDecorrenza && !anno.HasValue)
                            {
                                res = res.ToList();
                            }
                            else if (!onlyDecorrenza && anno.HasValue)
                            {
                                res = res.Where(w => (w.Decorrenza == null || (w.Decorrenza != null && w.Decorrenza.Value.Year == anno))).ToList();
                            }
                        }

                    }
                }

                if (res != null && res.Any())
                {
                    res.ForEach(w =>
                   {
                       output.Add(GetDatiDipendente(w));
                   });
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return output;
        }


        public List<PRV_Dipendente> GetDipendentiInDirezione(int idDirezione, List<int> idCampagnas, int? anno = null)
        {
            List<PRV_Dipendente> output = new List<PRV_Dipendente>();
            try
            {
                // reperimento della direzione
                XR_PRV_DIREZIONE direzione = this.GetDirezione(idDirezione);

                if (direzione != null)
                {
                    using (IncentiviEntities db = new IncentiviEntities())
                    {
                        List<PRV_Dipendente> result = new List<PRV_Dipendente>();
                        result = GetListPrvDipendente(db, idCampagnas, false, anno, idDirezione);

                        if (result != null && result.Any())
                        {
                            foreach (var r in result)
                            {
                                output.Add(CompletaDatiDipendente(r));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return output;
        }

        public List<Provvedimento> GetProvvedimenti(int idDip)
        {
            List<Provvedimento> result = new List<Provvedimento>();
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    result = (from dip in db.XR_PRV_DIPENDENTI_PROV
                              join p in db.XR_PRV_PROV
                              on dip.ID_PROV equals p.ID_PROV
                              where dip.ID_DIPENDENTE == idDip
                              select new Provvedimento()
                              {
                                  Id = dip.ID_DIPPROV,
                                  IdProvvedimento = dip.ID_PROV,
                                  Descrizione = p.NOME,
                                  Data = dip.DT_PROV,
                                  Importo = dip.IMPORTO
                              }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public decimal GetCostoDirezione(int idDirezione, int idCampagna, int? anno = null)
        {
            return GetCostoDirezione(idDirezione, new List<int>() { idCampagna }, anno);
        }
        public decimal GetCostoDirezione(int idDirezione, List<int> idCampagnas, int? anno = null)
        {
            decimal result = 0;
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
                    bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);
                    int idDirezioneRS = 0;
                    if (enableQIO == enableRS && idCampagnas.Count() > 1)
                    {
                        XR_PRV_DIREZIONE direzione = this.GetDirezione(idDirezione);
                        var dirRS = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.ID_DIREZIONE != direzione.ID_DIREZIONE && x.CODICE == direzione.CODICE);
                        idDirezioneRS = dirRS.ID_DIREZIONE;
                    }

                    result = GetCostoDirezioni(new List<int> { idDirezione, idDirezioneRS }, idCampagnas, anno);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }



        public decimal GetCostoDirezioni(List<int> idDirezioni, List<int> idCampagne, int? anno = null)
        {
            decimal result = 0;
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var funcProvEff = PoliticheRetributiveHelper.GetDipProvEff();

                    IQueryable<XR_PRV_DIPENDENTI> uss = BudgetGetDipendenti(db, idCampagne, idDirezioni, anno);

                    if (uss != null && uss.Any())
                    {
                        result = uss.Select(funcProvEff).Sum(a => a.COSTO_ANNUO);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        public decimal GetCostoPeriodoDirezione(int idDirezione, int idCampagna, int? anno = null)
        {
            decimal result = 0;
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var anyProvEff = PoliticheRetributiveHelper.AnyDipProvEff();
                    IQueryable<XR_PRV_DIPENDENTI> uss = db.XR_PRV_DIPENDENTI.Where(x => x.ID_CAMPAGNA == idCampagna && x.ID_DIREZIONE == idDirezione)
                                                                            .Where(anyProvEff);

                    if (anno.HasValue && anno.GetValueOrDefault() != DateTime.Now.Year)
                    {
                        uss = uss.Where(x => x.DECORRENZA.Value.Year == anno.Value);
                    }
                    else if (anno.HasValue && anno.GetValueOrDefault() != DateTime.Now.Year)
                    {
                        uss = uss.Where(x => (x.DECORRENZA.Value.Year == anno.Value || x.DECORRENZA == null));
                    }
                    else
                    {

                    }

                    uss = uss.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

                    if (uss != null && uss.Any())
                    {
                        var funcProvEff = PoliticheRetributiveHelper.GetDipProvEff();
                        result = uss.Select(funcProvEff)
                                        .Sum(a => a.COSTO_PERIODO);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        public void GetCostoAnnuoPeriodoDirezione(int idDirezione, int idCampagna, out decimal costoAnnuo, out decimal costoPeriodo, int? anno = null, Expression<Func<XR_PRV_DIPENDENTI, bool>> provvSelector = null)
        {
            GetCostoAnnuoPeriodoDirezione(idDirezione, new List<int> { idCampagna }, out costoAnnuo, out costoPeriodo, anno, provvSelector);
        }
        public void GetCostoAnnuoPeriodoDirezione(int idDirezione, List<int> idCampagna, out decimal costoAnnuo, out decimal costoPeriodo, int? anno = null, Expression<Func<XR_PRV_DIPENDENTI, bool>> provvSelector = null)
        {
            costoAnnuo = 0;
            costoPeriodo = 0;


            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    int? annoRifCampagna = null;
                    if (anno.HasValue)
                    {
                        annoRifCampagna = db.XR_PRV_CAMPAGNA.Where(x => idCampagna.Contains(x.ID_CAMPAGNA)).Select(x => x.DTA_INIZIO.Value.Year).First();
                    }

                    var anyProvEff = PoliticheRetributiveHelper.AnyDipProvEff();
                    IQueryable<XR_PRV_DIPENDENTI> uss = db.XR_PRV_DIPENDENTI.Where(x => idCampagna.Contains(x.ID_CAMPAGNA.Value) && x.ID_DIREZIONE == idDirezione)
                                                                            .Where(anyProvEff);


                    if (anno.HasValue && anno.GetValueOrDefault() != DateTime.Now.Year)
                    {
                        uss = uss.Where(x => (x.DECORRENZA.HasValue && x.DECORRENZA.Value.Year == anno.Value));
                    }
                    else if (anno.HasValue && anno.GetValueOrDefault() == DateTime.Now.Year)
                    {
                        uss = uss.Where(x => ((x.DECORRENZA.HasValue && x.DECORRENZA.Value.Year == anno.Value) || x.DECORRENZA == null));
                    }
                    else
                    {
                        ;
                    }

                    uss = uss.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));
                    if (provvSelector != null)
                        uss = uss.Where(provvSelector);

                    if (uss != null && uss.Any())
                    {
                        var funcProvEff = PoliticheRetributiveHelper.GetDipProvEff();
                        var tmp = uss.Select(funcProvEff);
                        costoAnnuo = tmp.Sum(a => a.COSTO_ANNUO);

                        if (!anno.HasValue || anno.GetValueOrDefault() == annoRifCampagna)
                            costoPeriodo = tmp.Sum(a => a.COSTO_PERIODO);
                        else
                        {
                            costoPeriodo = 0;
                            var promOrAumento = PoliticheRetributiveHelper.GetProvBySigla(db, PoliticheRetributiveHelper.SIGLA_PROMOZIONI, PoliticheRetributiveHelper.SIGLA_AUMENTI);
                            costoPeriodo += tmp.Where(x => promOrAumento.Contains(x.ID_PROV))
                                            .Select(x => new { costoAnnuo = x.COSTO_ANNUO, dec = x.XR_PRV_DIPENDENTI.DECORRENZA.Value }).ToList()
                                            .Select(y => y.costoAnnuo / 14 * (decimal)(13 - y.dec.Month + (y.dec.Month < 7 ? 2.5 : 1.5)))
                                            .Sum();

                            var gratifica = PoliticheRetributiveHelper.GetProvBySigla(db, PoliticheRetributiveHelper.SIGLA_GRATIFICHE);
                            foreach (var grat in tmp.Where(x => gratifica.Contains(x.ID_PROV)))
                            {
                                decimal aliq = HRDWData.GetProvvAliq(db, (int)ProvvedimentiEnum.Gratifica, grat.XR_PRV_DIPENDENTI.SINTESI1.COD_QUALIFICA);
                                costoPeriodo += grat.DIFF_RAL + (grat.DIFF_RAL * Convert.ToDecimal(aliq) / 100);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<PRV_Dipendente> GetDipendenteInDirezione(int idDipendente, int idDirezione, int idCampagna)
        {
            List<PRV_Dipendente> output = new List<PRV_Dipendente>();
            try
            {
                // reperimento della direzione
                XR_PRV_DIREZIONE direzione = this.GetDirezione(idDirezione);

                if (direzione != null)
                {
                    using (IncentiviEntities db = new IncentiviEntities())
                    {
                        //List<PRV_Dipendente> result = (from dip in db.XR_PRV_DIPENDENTI
                        //                               join sint in db.SINTESI1
                        //                               on dip.ID_PERSONA equals sint.ID_PERSONA
                        //                               where dip.ID_CAMPAGNA == idCampagna &&
                        //                                       dip.ID_DIREZIONE == idDirezione &&
                        //                                       dip.ID_DIPENDENTE == idDipendente
                        //                               select new PRV_Dipendente()
                        //                               {
                        //                                   Nominativo = sint.DES_COGNOMEPERS + (String.IsNullOrEmpty(sint.DES_SECCOGNOME) ? "" : " " + sint.DES_SECCOGNOME) + " " + sint.DES_NOMEPERS,
                        //                                   Matricola = sint.COD_MATLIBROMAT,
                        //                                   IdPersona = sint.ID_PERSONA,
                        //                                   IdDipendente = dip.ID_DIPENDENTE,
                        //                                   CostoAnnuo = 0,
                        //                                   CostoPeriodo = 0,
                        //                                   CostoConStraordinario = 0,
                        //                                   IdProvvedimento = PoliticheRetributiveHelper.GetDipTipoProvEffettivo(dip),
                        //                                   CustomProvv = (dip.CUSTOM_PROV.HasValue ? dip.CUSTOM_PROV.Value : false),
                        //                                   Decorrenza = dip.DECORRENZA,
                        //                                   CodRuolo = sint.COD_RUOLO,
                        //                                   DescRuolo = sint.DES_RUOLO,
                        //                                   Struttura = sint.DES_DENOMUNITAORG,
                        //                                   LivAttuale = dip.LIV_ATTUALE,
                        //                                   DataNascita = sint.DTA_NASCITAPERS,
                        //                                   DataAssunzione = sint.DTA_INIZIO_CR,
                        //                                   AggregatoSede = dip.DES_SEDE,
                        //                                   PartTime = dip.PART_TIME,
                        //                                   IdAssQual = sint.ID_ASSQUAL
                        //                               }).ToList();
                        List<PRV_Dipendente> result = GetListPrvDipendente(db, new int[] { idCampagna }, false, null, idDirezione, false, idDipendente);

                        if (result != null && result.Any())
                        {
                            foreach (var r in result)
                            {

                                output.Add(CompletaDatiDipendente(r));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return output;
        }

        public List<XR_PRV_DIPENDENTI_VARIAZIONI> GetVariazioniDipendente(int idDipendete)
        {
            List<XR_PRV_DIPENDENTI_VARIAZIONI> result = new List<XR_PRV_DIPENDENTI_VARIAZIONI>();
            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    result = db.XR_PRV_DIPENDENTI_VARIAZIONI.Where(w => w.ID_DIPENDENTE.Equals(idDipendete)).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        //public XR_PRV_DIPENDENTI_VARIAZIONI GetVariazioneDipendente(int idDipendete, int idProv)
        //{
        //    XR_PRV_DIPENDENTI_VARIAZIONI result = new XR_PRV_DIPENDENTI_VARIAZIONI();
        //    try
        //    {
        //        using (IncentiviEntities db = new IncentiviEntities())
        //        {
        //            result = db.XR_PRV_DIPENDENTI_VARIAZIONI.Where(w => w.ID_DIPENDENTE.Equals(idDipendete) && w.ID_PROV.Equals(idProv)).FirstOrDefault();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }

        //    return result;
        //}

        public bool SalvaSimulazione(string currentPMatricola, int idDir, int idCamp, List<TabellaItem> rows, int? anno = null)
        {
            bool result = true;

            try
            {
                if (rows != null && rows.Any())
                {
                    CezanneHelper.GetCampiFirma(out var campiFirma);
                    using (IncentiviEntities db = new IncentiviEntities())
                    {
                        foreach (var item in rows)
                        {
                            int idDipendente = item.IdDipendente;
                            int id_provv_effettivo = item.IdTipologia;
                            List<XR_PRV_DIPENDENTI> records = new List<XR_PRV_DIPENDENTI>();

                            if (anno.HasValue && anno.GetValueOrDefault() != DateTime.Now.Year)
                            {
                                records = db.XR_PRV_DIPENDENTI.Where(w => w.ID_CAMPAGNA.HasValue && w.DECORRENZA != null).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();
                                records = records.Where(w => w.DECORRENZA.Value.Year == anno.Value).ToList();
                            }
                            else if (anno.HasValue && anno.GetValueOrDefault() == DateTime.Now.Year)
                            {
                                records = db.XR_PRV_DIPENDENTI.Where(w => w.ID_CAMPAGNA.HasValue).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();
                                records = records.Where(w => (w.DECORRENZA.HasValue && w.DECORRENZA.Value.Year == anno.Value) || w.DECORRENZA == null).ToList();
                            }
                            else
                            {
                                records = db.XR_PRV_DIPENDENTI.Where(w => w.ID_CAMPAGNA.HasValue).Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();
                            }

                            if (records != null && records.Any())
                            {
                                // reperimento del record da salvare
                                var rec = records.Where(w => w.ID_DIPENDENTE.Equals(idDipendente) &&
                                                                    w.ID_CAMPAGNA.Equals(idCamp) &&
                                                                    w.ID_DIREZIONE.Equals(idDir)).FirstOrDefault();
                                if (rec != null)
                                {
                                    PoliticheRetributiveHelper.SetProvEffettivo(db, rec, id_provv_effettivo);
                                    rec.COD_USER = campiFirma.CodUser;
                                    rec.COD_TERMID = campiFirma.CodTermid;
                                    rec.TMS_TIMESTAMP = campiFirma.Timestamp;
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                }
                else
                {
                    throw new Exception("Nessun dato da salvare");
                }
            }
            catch (Exception ex)
            {
                result = false;
                throw new Exception(ex.Message);
            }

            return result;
        }

        public bool ConsolidaSimulazione(string currentPMatricola, int idDir, int idCamp, List<TabellaItem> rows)
        {
            bool result = true;

            try
            {
                if (rows != null && rows.Any())
                {
                    CezanneHelper.GetCampiFirma(out var campiFirma);
                    using (IncentiviEntities db = new IncentiviEntities())
                    {
                        var records = db.XR_PRV_DIPENDENTI
                                        .Where(w => w.ID_CAMPAGNA.HasValue)
                                        .Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa)).ToList();

                        foreach (var item in rows)
                        {
                            CezanneHelper.GetCampiFirma(out var campiOperatore);
                            XR_PRV_OPERSTATI operStati = null;
                            int idDipendente = item.IdDipendente;
                            int id_provv_effettivo = item.IdTipologia;
                            int idPersona = 0;

                            if (records != null && records.Any())
                            {
                                // reperimento del record da salvare
                                var rec = records.Where(w => w.ID_DIPENDENTE.Equals(idDipendente) &&
                                                                    w.ID_CAMPAGNA.Equals(idCamp) &&
                                                                    w.ID_DIREZIONE.Equals(idDir)).FirstOrDefault();
                                if (rec != null)
                                {
                                    PoliticheRetributiveHelper.SetProvEffettivo(db, rec, id_provv_effettivo);
                                    rec.COD_USER = campiFirma.CodUser;
                                    rec.COD_TERMID = campiFirma.CodTermid;
                                    rec.TMS_TIMESTAMP = campiFirma.Timestamp;

                                    // FRANCESCO 23/01/2023
                                    // AGGIUNTA DELLO STATO LETTERA "LETTERA ACCETTATA" sulla richiesta.
                                    rec.STATO_LETTERA = (int)StatiLetteraEnum.LetteraAccettata;

                                    idPersona = rec.ID_PERSONA;
                                }
                                else
                                {
                                    throw new Exception("Impossibile salvare la simulazione.\nDati di contesto non trovati");
                                }
                            }

                            var openStati = db.XR_PRV_OPERSTATI.Where(w => w.ID_DIPENDENTE.Equals(item.IdDipendente)).FirstOrDefault();
                            if (openStati != null)
                            {
                                openStati.ID_STATO = (int)OperStatiEnum.Consolidato;
                                openStati.COD_USER = campiFirma.CodUser;
                                openStati.COD_TERMID = campiFirma.CodTermid;
                                openStati.TMS_TIMESTAMP = campiFirma.Timestamp;
                            }
                            else
                            {
                                openStati = new XR_PRV_OPERSTATI()
                                {
                                    ID_OPER = db.XR_PRV_OPERSTATI.GeneraPrimaryKey(9),
                                    ID_DIPENDENTE = item.IdDipendente,
                                    ID_STATO = (int)OperStatiEnum.Consolidato,
                                    DATA = campiOperatore.Timestamp,
                                    ID_PERSONA = idPersona,
                                    DATA_FINE_VALIDITA = null,
                                    COD_USER = campiOperatore.CodUser,
                                    COD_TERMID = campiOperatore.CodTermid,
                                    TMS_TIMESTAMP = campiOperatore.Timestamp
                                };

                                db.XR_PRV_OPERSTATI.Add(openStati);

                                //throw new Exception("Impossibile salvare la simulazione.\nDati di contesto non trovati");
                            }

                            // FRANCESCO 23/01/2023
                            // AGGIUNTA DELLO STATO LETTERA CONSEGNATA sulla OPERSTATI

                            operStati = new XR_PRV_OPERSTATI()
                            {
                                ID_OPER = db.XR_PRV_OPERSTATI.GeneraPrimaryKey(9),
                                ID_DIPENDENTE = item.IdDipendente,
                                ID_STATO = (int)OperStatiEnum.LetteraConsegnata,
                                DATA = campiOperatore.Timestamp,
                                ID_PERSONA = openStati.ID_PERSONA,
                                DATA_FINE_VALIDITA = null,
                                COD_USER = campiOperatore.CodUser,
                                COD_TERMID = campiOperatore.CodTermid,
                                TMS_TIMESTAMP = campiOperatore.Timestamp
                            };

                            db.XR_PRV_OPERSTATI.Add(operStati);
                        }
                        db.SaveChanges();
                    }
                }
                else
                {
                    throw new Exception("Nessun dato da salvare");
                }
            }
            catch (Exception ex)
            {
                result = false;
                throw new Exception(ex.Message);
            }

            return result;
        }

        public bool SbloccaSimulazione(string currentPMatricola, int idDir, int idCamp)
        {
            bool result = true;

            try
            {
                var rows = this.GetDipendentiInDirezioneLight(idDir, idCamp);

                if (rows != null && rows.Any())
                {
                    using (IncentiviEntities db = new IncentiviEntities())
                    {
                        foreach (var item in rows)
                        {
                            int idDipendente = item.IdDipendente;

                            var openStati = db.XR_PRV_OPERSTATI.FirstOrDefault(w => w.ID_DIPENDENTE.Equals(item.IdDipendente) && w.ID_STATO == (int)OperStatiEnum.Consolidato);
                            if (openStati != null)
                            {
                                db.XR_PRV_OPERSTATI.Remove(openStati);
                                //openStati.ID_STATO = ( int ) OperStatiEnum.Richiesta;
                                //openStati.COD_USER = Utente.Matricola( );
                                //openStati.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                                //openStati.TMS_TIMESTAMP = DateTime.Now;
                            }
                            else
                            {
                                throw new Exception("Impossibile salvare la simulazione.\nDati di contesto non trovati");
                            }
                        }
                        db.SaveChanges();
                    }
                }
                else
                {
                    throw new Exception("Nessun dato da salvare");
                }
            }
            catch (Exception ex)
            {
                result = false;
                throw new Exception(ex.Message);
            }

            return result;
        }

        public bool DirezioneIsConsolidata(int idCamp, int idDir)
        {
            bool result = false;
            try
            {
                var dip = this.GetDipendentiInDirezioneLight(idDir, idCamp);

                if (dip != null && dip.Any())
                {
                    //result = dip.Count(w => !w.IsConsolidato) == 0;
                    result = !dip.Any(w => !w.IsConsolidato);
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        public ReportDirezione GetDirezioneNelleSedi(string codice, int idCamp, int idArea)
        {
            ReportDirezione result = new ReportDirezione();

            try
            {
                XR_PRV_AREA area = this.GetArea(idArea);

                // direzione CANONE
                if (codice == "20")
                {
                    result.Id = 0;
                    result.Area = area.NOME;
                    result.Direzione = "Canone nelle sedi";
                    result.OrganicoContabile = 0;
                    result.OrganicoRipartizione = 0;
                    result.Promozioni = 0;
                    result.Aumenti = 0;
                    result.Gratifiche = 0;
                    result.TotProvv = result.Promozioni + result.Gratifiche + result.Aumenti;
                    result.BudgetPeriodoCorrente = 0;
                    result.CostoPeriodoAnnoCorrente = 0;
                    result.PercentualeSuOrganico = 0;
                    result.BudgetAnnoCorrente = 0;
                    result.CostoAnnoCorrente = 0;
                    result.DeltaSuCostoAnnoCorrente = 0;
                    result.DeltaSuCostoPeriodo = 0;
                    result.IsConsolidata = false;
                    result.CostoPeriodoAnnoPrecedente = 0;
                    result.BudgetAnnoPrecedente = 0;
                    result.CostoAnnoPrecedente = 0;
                    result.CostoRegime = 0;
                    result.BudgetPeriodoAnnoPrecedente = 0;
                    result.CostoRecuperoStraordinario = 0;
                    result.DeltaSuCostoAnnoPrecedente = 0;
                    result.ParentIdGruppo = int.Parse(codice);

                    using (var db = new IncentiviEntities())
                    {
                        var res = (from dip in db.XR_PRV_DIPENDENTI
                                   join sint in db.SINTESI1
                                   on dip.ID_PERSONA equals sint.ID_PERSONA
                                   join dir in db.XR_PRV_DIREZIONE
                                   on dip.ID_DIREZIONE equals dir.ID_DIREZIONE
                                   where dip.ID_CAMPAGNA == idCamp &&
                                   (sint.COD_UNITAORG.ToUpper().StartsWith("GA") && sint.COD_UNITAORG.ToUpper().EndsWith("1100"))
                                   select new
                                   {
                                       dip = dip,
                                       dir = dir,
                                       sint = sint
                                   }).ToList();

                        if (res != null && res.Any())
                        {
                            int sommaPromozioni = 0;
                            int sommaAumenti = 0;
                            int sommaGratifiche = 0;

                            var groups = res.GroupBy(w => w.dir.ID_DIREZIONE).Select(g => new
                            {
                                Direzione = g.Key,
                                Count = g.Select(l => l.dir).Distinct().Count(),
                                Dipendenti = g.Select(l => l.dip)
                            });

                            if (groups != null && groups.Any())
                            {
                                var isPromozione = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_PROMOZIONI)).Compile();
                                var isAumento = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_AUMENTI)).Compile();
                                var isGratifica = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_GRATIFICHE)).Compile();

                                foreach (var g in groups)
                                {
                                    sommaPromozioni += g.Dipendenti.Count(isPromozione);

                                    sommaAumenti += g.Dipendenti.Count(isAumento);

                                    sommaGratifiche += g.Dipendenti.Count(isGratifica);
                                }
                            }

                            result.Id = 0;
                            result.Area = area.NOME;
                            result.Direzione = "Canone nelle sedi";
                            result.OrganicoContabile = 0;
                            result.OrganicoRipartizione = 0;
                            result.Promozioni = sommaPromozioni;
                            result.Aumenti = sommaAumenti;
                            result.Gratifiche = sommaGratifiche;
                            result.TotProvv = result.Promozioni + result.Gratifiche + result.Aumenti;
                            result.BudgetPeriodoCorrente = 0;
                            result.CostoPeriodoAnnoCorrente = 0;
                            result.PercentualeSuOrganico = 0;
                            result.BudgetAnnoCorrente = 0;
                            result.CostoAnnoCorrente = 0;
                            result.DeltaSuCostoAnnoCorrente = 0;
                            result.DeltaSuCostoPeriodo = 0;
                            result.IsConsolidata = false;
                            result.CostoPeriodoAnnoPrecedente = 0;
                            result.BudgetAnnoPrecedente = 0;
                            result.CostoAnnoPrecedente = 0;
                            result.CostoRegime = 0;
                            result.BudgetPeriodoAnnoPrecedente = 0;
                            result.CostoRecuperoStraordinario = 0;
                            result.DeltaSuCostoAnnoPrecedente = 0;
                            result.ParentIdGruppo = int.Parse(codice);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;

        }

        public List<XR_PRV_CAMPAGNA_DECORRENZA> GetDecorrenzeCampagna(int idCamp)
        {
            List<XR_PRV_CAMPAGNA_DECORRENZA> result = new List<XR_PRV_CAMPAGNA_DECORRENZA>();

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    result = db.XR_PRV_CAMPAGNA_DECORRENZA.Where(w => w.ID_CAMPAGNA.Equals(idCamp)).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }
        public List<XR_PRV_CAMPAGNA_DECORRENZA> GetDecorrenzeCampagna(List<int> idCamp)
        {
            List<XR_PRV_CAMPAGNA_DECORRENZA> result = new List<XR_PRV_CAMPAGNA_DECORRENZA>();

            try
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    foreach (var item in db.XR_PRV_CAMPAGNA_DECORRENZA.Where(x => idCamp.Contains(x.ID_CAMPAGNA)))
                    {
                        if (!result.Any(x => x.DT_DECORRENZA == item.DT_DECORRENZA))
                            result.Add(item);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        #region PRIVATE



        private PRV_Dipendente GetDatiDipendente(PRV_Dipendente dipendente)
        {
            PRV_Dipendente r = new PRV_Dipendente();
            try
            {
                r = dipendente;

                using (IncentiviEntities db = new IncentiviEntities())
                {
                    r.Note = "";

                    #region Reperimento provvedimenti
                    // reperimento del valore del provvedimento corrente
                    //var varDip = PoliticheRetributiveHelper.GetDipProvList(db, r.IdDipendente, r.IdProvvedimento);
                    var varDip = new List<XR_PRV_DIPENDENTI_VARIAZIONI>();
                    varDip.Add(PoliticheRetributiveHelper.GetDipProvEffettivo(r.IdDipendente));
                    if (varDip != null && varDip.Any())
                    {
                        varDip = varDip.Where(w => w != null).ToList();
                    }

                    if (varDip != null & varDip.Any())
                    {
                        var variazione = varDip.OrderByDescending(w => w.ID_DIP_COSTO).FirstOrDefault();
                        r.CostoAnnuo = variazione.COSTO_ANNUO;
                        r.CostoPeriodo = variazione.COSTO_PERIODO;
                        r.CostoConStraordinario = variazione.COSTO_REC_STR.GetValueOrDefault();

                        if (String.IsNullOrEmpty(r.LivAttuale))
                        {
                            r.LivAttuale = variazione.LIV_ATTUALE;
                        }
                        r.LivPrevisto = variazione.LIV_PREVISTO;
                        r.CategoriaPrevista = variazione.CAT_PREVISTA;

                    }

                    r.Variazioni = new List<PRV_Dipendente_CostoVariazione>();

                    #endregion

                    #region reperimento RAL
                    // reperimento della ral
                    var dipRal = db.XR_PRV_DIPENDENTI_RAL.Where(w => w.ID_DIPENDENTE.Equals(r.IdDipendente)).ToList();

                    if (dipRal != null && dipRal.Any())
                    {
                        var element = dipRal.OrderByDescending(w => w.DT_RAL).FirstOrDefault();

                        if (element != null)
                        {
                            r.RAL = element.IMPORTO;
                        }
                    }
                    #endregion

                    #region Reperimento dati Direzione
                    if (r.IdDirezione.HasValue)
                    {
                        r.Direzione = this.GetDirezione(r.IdDirezione.GetValueOrDefault());

                        if (r.Direzione != null)
                        {
                            r.IdArea = r.Direzione.ID_AREA;
                            r.Area = this.GetArea(r.Direzione.ID_AREA);

                            if (r.Area != null && r.Area.NOME.ToUpper().Contains("AREA RISORSE CHIAVE"))
                            {
                                var d = db.XR_PRV_DIREZIONE.Where(w => w.CODICE.Equals(r.Direzione.CODICE) && w.ID_DIREZIONE != r.Direzione.ID_DIREZIONE).FirstOrDefault();

                                if (d != null)
                                {
                                    var a = db.XR_PRV_AREA.Where(w => w.ID_AREA.Equals(d.ID_AREA)).FirstOrDefault();
                                    if (a != null)
                                    {
                                        r.MacroArea = a.NOME;
                                    }
                                }
                            }
                            else if (r.Area != null)
                            {
                                r.MacroArea = r.Area.NOME;
                            }
                        }
                    }

                    #endregion

                    #region Dati Campagna

                    if (r.IdCampagna.HasValue)
                    {
                        var camp = db.XR_PRV_CAMPAGNA.Where(w => w.ID_CAMPAGNA.Equals(r.IdCampagna.Value)).First();
                        if (camp != null)
                        {
                            r.Campagna = camp;
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return r;
        }
        private PRV_Dipendente CompletaDatiDipendente(PRV_Dipendente dipendente)
        {
            PRV_Dipendente r = new PRV_Dipendente();
            try
            {
                r = dipendente;

                using (IncentiviEntities db = new IncentiviEntities())
                {
                    r.Note = "";

                    #region anzianità di livello
                    var asq = db.ASSQUAL.Where(w => w.ID_PERSONA.Equals(r.IdPersona)).OrderByDescending(w => w.DTA_INIZIO).FirstOrDefault();
                    if (asq != null)
                    {
                        r.IdAssQual = asq.ID_ASSQUAL;
                        r.AnzianitaLivello = HrisHelper.GetDataAnzCat(dipendente.IdPersona);
                        //if ( asq.JSSQUAL != null )
                        //{
                        //    r.AnzianitaLivello = asq.JSSQUAL.DTA_ANZCAT;
                        //}
                    }

                    //if (r.IdAssQual.HasValue)
                    //{
                    //    var anzLiv = db.JSSQUAL.Where(w => w.ID_ASSQUAL.Equals(r.IdAssQual.Value)).FirstOrDefault();

                    //    if (anzLiv != null)
                    //    {
                    //        r.AnzianitaLivello = anzLiv.DTA_ANZCAT;
                    //    }
                    //}
                    #endregion

                    #region Stato pratica

                    var stati = db.XR_PRV_OPERSTATI.Where(w => w.ID_DIPENDENTE.Equals(r.IdDipendente)).ToList();

                    if (stati != null && stati.Any())
                    {
                        r.IsConsolidato = false;
                        var _maxStato = stati.Where(w => !w.DATA_FINE_VALIDITA.HasValue).Max(w => w.ID_STATO);
                        if (_maxStato >= (int)OperStatiEnum.Consolidato)
                        {
                            r.IsConsolidato = true;
                        }
                    }
                    else
                    {
                        r.IsConsolidato = false;
                    }
                    #endregion

                    #region Reperimento provvedimenti
                    r.Variazioni = new List<PRV_Dipendente_CostoVariazione>();

                    //// reperimento dei provvedimenti associati all'utente per le possibili variazioni
                    var varDip = GetVariazioniDipendente(r.IdDipendente);
                    if (varDip != null && varDip.Any())
                    {
                        foreach (var v in varDip)
                        {
                            if (r.CustomProvv && (v.ID_PROV == (int)ProvvedimentiEnum.AumentoLivello ||
                                                     v.ID_PROV == (int)ProvvedimentiEnum.AumentoLivelloNoAssorbimento ||
                                                     v.ID_PROV == (int)ProvvedimentiEnum.Gratifica ||
                                                     v.ID_PROV == (int)ProvvedimentiEnum.AumentoMerito ||
                                                     v.ID_PROV == (int)ProvvedimentiEnum.Nessuno))
                                continue;

                            if (v.ID_PROV == r.IdProvvedimento)
                            {
                                r.CostoAnnuo = v.COSTO_ANNUO;
                                r.CostoPeriodo = v.COSTO_PERIODO;
                                r.CostoConStraordinario = v.COSTO_REC_STR.GetValueOrDefault();

                                if (String.IsNullOrEmpty(r.LivAttuale))
                                {
                                    r.LivAttuale = v.LIV_ATTUALE;
                                }
                                r.LivPrevisto = v.LIV_PREVISTO;
                            }

                            r.Variazioni.Add(new PRV_Dipendente_CostoVariazione()
                            {
                                IdProvvedimento = v.ID_PROV,
                                Costo = v.COSTO_ANNUO,
                                CostoPeriodo = v.COSTO_PERIODO,
                                CostoStraordinario = v.COSTO_REC_STR.GetValueOrDefault(),
                                LivAttuale = v.LIV_ATTUALE,
                                LivPrevisto = v.LIV_PREVISTO
                            });
                        }
                    }

                    #endregion

                    #region reperimento RAL
                    // reperimento della ral
                    var dipRal = db.XR_PRV_DIPENDENTI_RAL.Where(w => w.ID_DIPENDENTE.Equals(r.IdDipendente)).ToList();

                    if (dipRal != null && dipRal.Any())
                    {
                        var element = dipRal.OrderByDescending(w => w.DT_RAL).FirstOrDefault();

                        if (element != null)
                        {
                            r.RAL = element.IMPORTO;
                        }
                    }
                    #endregion

                    #region reperimento dei precedenti provvedimenti concessi all'utente
                    r.Provvedimenti = new List<Provvedimento>();

                    DateTime current = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    current = current.AddYears(-5);

                    var provvedimenti = db.XR_PRV_DIPENDENTI_PROV.Where(w => w.ID_DIPENDENTE.Equals(r.IdDipendente) && w.DT_PROV >= current).ToList();

                    if (provvedimenti != null && provvedimenti.Any())
                    {
                        r.NumeroProvvedimenti = provvedimenti.Count();
                        var provvedimentiOrdinati = provvedimenti.OrderByDescending(w => w.DT_PROV).ToList();
                        foreach (var p in provvedimentiOrdinati)
                        {
                            r.Provvedimenti.Add(new Provvedimento()
                            {
                                Id = p.ID_DIPPROV,
                                IdProvvedimento = p.ID_PROV,
                                Descrizione = p.DESCRIZIONE,
                                Data = p.DT_PROV,
                                Importo = p.IMPORTO
                            });
                        }
                    }
                    else
                    {
                        r.NumeroProvvedimenti = 0;
                    }
                    #endregion

                    #region reperimento Retribuzione variabile e Reperibilità

                    DateTime ultimi12mesi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                    ultimi12mesi = ultimi12mesi.AddMonths(-12);

                    List<XR_PRV_DIPENDENTI_VAR> last12mesi = new List<XR_PRV_DIPENDENTI_VAR>();

                    var mesi = db.XR_PRV_DIPENDENTI_VAR.Where(w => w.ID_DIPENDENTE.Equals(r.IdDipendente)).ToList();

                    if (mesi != null && mesi.Any())
                    {
                        foreach (var m in mesi)
                        {
                            DateTime dt;
                            if (m.MESE.GetValueOrDefault() > 0)
                            {
                                dt = new DateTime(m.ANNO, m.MESE.GetValueOrDefault(), 1);
                            }
                            else
                            {
                                dt = new DateTime(m.ANNO, 1, 1);
                            }

                            if (dt.Date >= ultimi12mesi.Date)
                            {
                                last12mesi.Add(m);
                            }
                        }

                        if (last12mesi != null && last12mesi.Any())
                        {
                            var rep = last12mesi.Where(w => w.ID_VAR_TIPO == 2).ToList();
                            var retr = last12mesi.Where(w => w.ID_VAR_TIPO == 1).ToList();

                            if (rep != null && rep.Any())
                            {
                                r.Reperibilita = rep.Sum(w => w.IMPORTO);
                            }

                            if (retr != null && retr.Any())
                            {
                                r.RetribuzioneVariabile = retr.Sum(w => w.IMPORTO);
                            }
                        }
                    }

                    #endregion

                    #region Assenze

                    var assenze = db.XR_PRV_DIPENDENTI_ASSENZE.Where(w => w.ID_DIPENDENTE.Equals(r.IdDipendente)).ToList();

                    var gruppiAssenze = assenze.GroupBy(w => w.COD_ECCEZIONE).Select(g => new
                    {
                        Assenza = g.Key
                    });

                    r.Assenze = new List<Assenza>();

                    if (gruppiAssenze != null && gruppiAssenze.Any())
                    {
                        foreach (var ga in gruppiAssenze)
                        {
                            var xx = assenze.Where(w => w.COD_ECCEZIONE == ga.Assenza);
                            double q = xx.Sum(w => w.QUANTITA);
                            r.Assenze.Add(new Assenza()
                            {
                                Codice = ga.Assenza,
                                Quantita = q
                            });
                        }
                    }

                    #endregion

                    #region reperimento note
                    var note = db.XR_PRV_DIPENDENTI_NOTE.Where(w => w.ID_DIPENDENTE.Equals(r.IdDipendente)).ToList();
                    string sNote = "";
                    if (note != null && note.Any())
                    {
                        foreach (var n in note)
                        {
                            sNote += String.Format("{0}\r\n", n.NOTA);
                        }
                    }
                    r.Note = sNote;
                    #endregion

                    #region Reperimento dati Direzione
                    if (r.IdDirezione.HasValue)
                    {
                        r.Direzione = this.GetDirezione(r.IdDirezione.GetValueOrDefault());

                        if (r.Direzione != null)
                        {
                            r.IdArea = r.Direzione.ID_AREA;
                            r.Area = this.GetArea(r.Direzione.ID_AREA);

                            if (r.Area != null && r.Area.NOME.ToUpper().Contains("AREA RISORSE CHIAVE"))
                            {
                                var d = db.XR_PRV_DIREZIONE.Where(w => w.CODICE.Equals(r.Direzione.CODICE) && w.ID_DIREZIONE != r.Direzione.ID_DIREZIONE).FirstOrDefault();

                                if (d != null)
                                {
                                    var a = db.XR_PRV_AREA.Where(w => w.ID_AREA.Equals(d.ID_AREA)).FirstOrDefault();
                                    if (a != null)
                                    {
                                        r.MacroArea = a.NOME;
                                    }
                                }
                            }
                            else if (r.Area != null)
                            {
                                r.MacroArea = r.Area.NOME;
                            }
                        }
                    }

                    #endregion

                    #region Dati Campagna

                    if (r.IdCampagna.HasValue)
                    {
                        var camp = db.XR_PRV_CAMPAGNA.Where(w => w.ID_CAMPAGNA.Equals(r.IdCampagna.Value)).First();
                        if (camp != null)
                        {
                            r.Campagna = camp;
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return r;
        }

        private List<PRV_Dipendente> GetDipendentiInDirezioneLight(int idDirezione, int idCampagna)
        {
            List<PRV_Dipendente> result = new List<PRV_Dipendente>();
            try
            {
                // reperimento della direzione
                XR_PRV_DIREZIONE direzione = this.GetDirezione(idDirezione);

                if (direzione != null)
                {
                    using (IncentiviEntities db = new IncentiviEntities())
                    {
                        //result = (from dip in db.XR_PRV_DIPENDENTI
                        //          join sint in db.SINTESI1
                        //          on dip.ID_PERSONA equals sint.ID_PERSONA
                        //          where dip.ID_CAMPAGNA == idCampagna &&
                        //                  dip.ID_DIREZIONE == idDirezione &&
                        //                  (dip.ID_CAMPAGNA.Value > 2 || !dip.XR_PRV_OPERSTATI.Any(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE && x.ID_STATO == (int)ProvvStatoEnum.Conclusa))
                        //          select new PRV_Dipendente()
                        //          {
                        //              Nominativo = sint.DES_COGNOMEPERS + (String.IsNullOrEmpty(sint.DES_SECCOGNOME) ? "" : " " + sint.DES_SECCOGNOME) + " " + sint.DES_NOMEPERS,
                        //              Matricola = sint.COD_MATLIBROMAT,
                        //              IdPersona = sint.ID_PERSONA,
                        //              IdDipendente = dip.ID_DIPENDENTE,
                        //              CostoAnnuo = 0,
                        //              CostoPeriodo = 0,
                        //              CostoConStraordinario = 0,
                        //              IdProvvedimento = PoliticheRetributiveHelper.GetDipTipoProvEffettivo(dip),
                        //              Decorrenza = dip.DECORRENZA,
                        //              CodRuolo = sint.COD_RUOLO,
                        //              DescRuolo = sint.DES_RUOLO,
                        //              Struttura = sint.DES_DENOMUNITAORG,
                        //              LivAttuale = dip.LIV_ATTUALE,
                        //              DataNascita = sint.DTA_NASCITAPERS,
                        //              DataAssunzione = sint.DTA_INIZIO_CR,
                        //              AggregatoSede = dip.DES_SEDE,
                        //              PartTime = dip.PART_TIME,
                        //              IdAssQual = sint.ID_ASSQUAL
                        //          }).ToList();
                        result = GetListPrvDipendente(db, new int[] { idCampagna }, false, null, idDirezione, false, null, true);

                        //if (result != null && result.Any())
                        //{
                        //    if (direzione.CODICE == "20")
                        //    {
                        //        foreach (var r in result)
                        //        {
                        //            var res = (from dip in db.XR_PRV_DIPENDENTI
                        //                       join sint in db.SINTESI1
                        //                       on dip.ID_PERSONA equals sint.ID_PERSONA
                        //                       join dir in db.XR_PRV_DIREZIONE
                        //                       on dip.ID_DIREZIONE equals dir.ID_DIREZIONE
                        //                       where dip.ID_CAMPAGNA == idCampagna &&
                        //                       dir.ID_DIREZIONE == idDirezione &&
                        //                       (sint.COD_UNITAORG.ToUpper().StartsWith("GA") && sint.COD_UNITAORG.ToUpper().EndsWith("1100"))
                        //                       select dip).ToList();

                        //            if (res != null && res.Any())
                        //            {
                        //                int occorrenze = res.Count(w => w.ID_DIPENDENTE.Equals(r.IdDipendente));

                        //                r.UtenteNelleSottoSedi = (occorrenze > 0);
                        //            }
                        //        }
                        //    }
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        #endregion

    }
}