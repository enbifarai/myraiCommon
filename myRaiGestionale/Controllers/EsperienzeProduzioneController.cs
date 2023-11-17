using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRaiCommonModel;
using myRaiData;
using myRaiCommonModel.raiplace;

namespace myRaiGestionale.Controllers
{
    public class EsperienzeProduzioneController : Controller
    {
        //
        // GET: /EsperienzeProduzione/

        public ActionResult Index()
        {
            EsperienzeProduzioneModel esperienzeProduzione = new EsperienzeProduzioneModel();
            RicercaUnica ricercaUnica = new RicercaUnica();
            List<DServizio> servizi = new List<DServizio>();
            var db = new PERSEOEntities();
            servizi = db.DServizio.Where(x => !x.Descrizione.Trim().Equals(" ") && x.Data_scadenza == "99991231").ToList();
            List<DFiguraPro> figure = new List<DFiguraPro>();
            figure = db.DFiguraPro.Where(x => !x.DescriFiguraPro.Equals(" ")).OrderBy(x => new { x.PosFiguraPro, x.DescriFiguraPro }).ToList();
            List<DConProf> conpro = new List<DConProf>();
            conpro = db.DConProf.Where(x => x.Stato == "0").OrderBy(x => new { x.Posizione, x.DescConProf }).ToList();
            ProduzioniTelevisive produzioni = new ProduzioniTelevisive();
            List<RicercaProgrammaResult> programmi = new List<RicercaProgrammaResult>();
            programmi = GetProduzioniTelevisive(db);
            ricercaUnica.Programmi = programmi;
            ricercaUnica.Servizi = servizi;
            ricercaUnica.DFiguras = figure;
            ricercaUnica.DConProfs = conpro;
            ricercaUnica.Programmi = programmi;
            ricercaUnica.TipoRicerca = new List<SelectListItem> {
           new SelectListItem { Selected = true, Text = "", Value = null},
         new SelectListItem { Selected = false, Text = "Disponibili", Value = "D"},
        new SelectListItem { Selected = false, Text = "Allocate", Value = "A"},
        new SelectListItem { Selected = false, Text = "Prenotate", Value = "P"},
        new SelectListItem { Selected = false, Text = "Riepilogo", Value = "R"}
    };
            ricercaUnica.TipoDipendente = new List<SelectListItem> {
           new SelectListItem { Selected = true, Text = "Tutti", Value = "T"},
        new SelectListItem { Selected = false, Text = "Tempo indeterminato", Value = "I"},
        new SelectListItem { Selected = false, Text = "Tempo determinato", Value = "D"}
   };

            esperienzeProduzione.parametriricerca = ricercaUnica;
            List<RisorsaProd> risorse = new List<RisorsaProd>();
            esperienzeProduzione.risorse = risorse;
            esperienzeProduzione.numeroRisorse = 0;



            return View("Index", esperienzeProduzione);
        }

        private List<RicercaProgrammaResult> GetProduzioniTelevisive(PERSEOEntities db)
        {
            List<RicercaProgrammaResult> lista = new List<RicercaProgrammaResult>();
            var matricola = CommonHelper.GetCurrentUserMatricola();
            List<string> abil = db.TAbilitazione.Where(x => x.login == "P" + matricola && (x.flag_validita == "1") && x.servizio != "").OrderBy(x => x.servizio).Select(x => x.servizio).ToList(); //aggiungo sempre P a matricola non credo
            List<String> vuorg = new List<String>();
            foreach (var t in abil)
            {
                if (t == "**")
                {
                    vuorg = db.DUorgEsperProd.OrderBy(x => x.Posizione).Select(x => x.Cod_Uorg).Distinct().ToList();
                }
                else
                {
                    vuorg = db.DUorgEsperProd.Where(x => x.Cod_Servizio == t).OrderBy(x => x.Posizione).Select(x => x.Cod_Uorg).Distinct().ToList();
                }
                foreach (var uorg in vuorg.Distinct())
                {
                    lista.AddRange(db.TSVAnagCom.Where(x => x.STATO == "0" && x.COD_UORG_BASE == uorg).Select(x => new RicercaProgrammaResult() { Titolo = x.DES_TITOLO_DEFINIT, UORG = x.COD_UORG_BASE, Matricola = x.COD_MATRICOLA }).ToList().OrderBy(x => x.Titolo));

                }
            }
            return lista.Distinct().ToList();
        }
        #region RENDER
        [HttpPost]
        public ActionResult RicercaSql(EsperienzeProduzioneModel model)
        {
            List<RisorsaProd> risorse = new List<RisorsaProd>();
            var db = new PERSEOEntities();
            string dataDal = model.parametriricerca.DataDal.ToString("yyyyMMdd");
            string dataAl = model.parametriricerca.DataAl.ToString("yyyyMMdd");
            string select = "SELECT distinct TDipendenti.Matricola, TDipendenti.Nominativo, DServizio.Descrizione as Direzione, TDipendenti.Figura_pro as Figura,dbo.DataMinMaxImp(" + dataDal + "," + dataAl + ",TDipendenti.MATRICOLA,'MIN') as dataInizio,dbo.DataMinMaxImp(" + dataDal + "," + dataAl + ",TDipendenti.MATRICOLA,'MAX') as dataFine, CONVERT(int, dbo.DataMinMaxImp(" + dataDal + "," + dataAl + ",TDipendenti.MATRICOLA,'CTR')) AS numProd, CONVERT(int, dbo.DataMinMaxImp(" + dataDal + "," + dataAl + ",TDipendenti.MATRICOLA,'PRC')) AS percentuale ";
            string from = "FROM TDipendenti, DServizio ";
            string query = "WHERE SUBSTRING(TDipendenti.Servizio, 1, 2) = DServizio.codice AND TDipendenti.Flag_ultimo_record = '$' AND ";

            switch (model.parametriricerca.RicercaScelta)
            {
                case "D":
                    {
                        query = query + " TDipendenti.Matricola  NOT IN (SELECT TSVEsperProdSint.MATRICOLA FROM TSVEsperProdSint WHERE '" + dataDal + "'>= TSVEsperProdSint.INIZIO_PERIODO_ESP  AND '" + dataAl + "'<= TSVEsperProdSint.FINE_PERIODO_ESP  GROUP BY TSVEsperProdSint.MATRICOLA HAVING SUM(PERC_ASSEGNAZIONE) > 99  ) AND TDipendenti.Matricola NOT IN(SELECT TSVEsperProdSint.MATRICOLA  FROM TSVEsperProdSint WHERE CALCOLO_AUTOMATICO = 'S' AND '" + dataDal + "' >= TSVEsperProdSint.INIZIO_PERIODO_ESP  AND '" + dataAl + "' <= TSVEsperProdSint.FINE_PERIODO_ESP GROUP BY TSVEsperProdSint.MATRICOLA HAVING COUNT(*) > 0) AND (TDipendenti.Data_cessazione=' ' or '" + dataDal + "' < TDipendenti.Data_cessazione) AND ";
                        break;
                    }
                case "A":
                    {
                        query = query + "TDipendenti.Matricola  IN (SELECT TSVEsperProdSint.MATRICOLA FROM TSVEsperProdSint WHERE " + dataDal + " >= CAST(TSVEsperProdSint.INIZIO_PERIODO_ESP AS int) AND " + dataAl + " <= CAST(TSVEsperProdSint.FINE_PERIODO_ESP AS int) AND (CALCOLO_AUTOMATICO = 'S' OR PERC_ASSEGNAZIONE > '99')) AND ";
                        break;
                    }
                case "P": // vedi sp non capisco
                    {
                        query = query + "TDipendenti.Matricola  IN (SELECT TSVEsperProdSint.MATRICOLA FROM TSVEsperProdSint,TSVEsperProd WHERE tsvesperprodsint.matricola=tsvesperprod.matricola and " + dataDal + " >= CAST(TSVEsperProdSint.INIZIO_PERIODO_ESP AS int) AND " + dataAl + " <= CAST(TSVEsperProdSint.FINE_PERIODO_ESP AS int) AND STATO_ALLOCAZIONE = 'P') AND ";
                        break;
                    }
                default: break;
            }
            if (!string.IsNullOrEmpty(model.parametriricerca.Nominativo))
            {
                query = query + " (TDipendenti.Nominativo LIKE '" + model.parametriricerca.Nominativo.TrimEnd() + " % ') AND ";

            }
            if (!string.IsNullOrEmpty(model.parametriricerca.Matricola))
            {
                query = query + " (TDipendenti.Matricola IN  ('" + model.parametriricerca.Matricola.TrimEnd().Replace(",", "','") + "')) AND ";
            }

            switch (model.parametriricerca.Dipendente)
            {
                case "I":
                    {
                        query = query + "TDipendenti.Forma_contratto IN ('9', 'K') AND ";
                        break;
                    }

                case "D":
                    {
                        query = query + "TDipendenti.Forma_contratto IN ('8', 'H', 'G') AND ";
                        break;
                    }
                default: break;
            }
            if (model.parametriricerca.ServiziSel != null)
            {
                query = query + " SUBSTRING(TDipendenti.Servizio, 1, 2) in (";
                for (int i = 0; i < model.parametriricerca.ServiziSel.Count(); i++)
                {
                    string servizio = model.parametriricerca.ServiziSel[i].TrimEnd();
                    if (i < model.parametriricerca.ServiziSel.Count() - 1)
                    {
                        query = query + "'" + servizio + "',";
                    }
                    else
                    {
                        query = query + "'" + servizio + "') AND ";
                    }
                }
            }
            if (model.parametriricerca.FiguraSel != null)
            {
                query = query + " TDipendenti.Figura_pro in (";
                for (int i = 0; i < model.parametriricerca.FiguraSel.Count(); i++)
                {
                    string figura = model.parametriricerca.FiguraSel[i];
                    if (i < model.parametriricerca.FiguraSel.Count() - 1)
                    {
                        query = query + "'" + figura + "',";
                    }
                    else
                    {
                        query = query + "'" + figura + "') AND ";
                    }
                }
            }
            if (model.parametriricerca.ConprofSel != null)
            {
                from = from + ",TSVEsperProd,TSVRuoliEsperProd ";
                query = query + " TDipendenti.Matricola = TSVEsperProd.MATRICOLA AND TSVEsperProd.ID_ESPERIENZE = TSVRuoliEsperProd.IdEsperProd AND TSVRuoliEsperProd.Stato IN('C') AND TSVRuoliEsperProd.RuoloPrincipale IN('S') AND TSVRuoliEsperProd.CodRuolo IN (";
                for (int i = 0; i < model.parametriricerca.ConprofSel.Count(); i++)
                {
                    string ruolo = model.parametriricerca.ConprofSel[i];
                    if (i < model.parametriricerca.ConprofSel.Count() - 1)
                    {
                        query = query + "'" + ruolo + "',";
                    }
                    else
                    {
                        query = query + "'" + ruolo + "') AND ";
                    }
                }
            }
            string queryt = select + from + query.Substring(0, query.Length - 4);
            risorse = db.Database.SqlQuery<RisorsaProd>(queryt).ToList();
            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;

            foreach (RisorsaProd risorsa in risorse)
            {
                if (risorsa.numProd != 0)
                {
                    if (DateTime.ParseExact(risorsa.dataInizio, "yyyyMMdd", provider).CompareTo(model.parametriricerca.DataDal) > 0 || DateTime.ParseExact(risorsa.dataFine, "yyyyMMdd", provider).CompareTo(model.parametriricerca.DataAl) < 0)
                    {
                        risorsa.Disponibile = "orange";
                    }
                    else risorsa.Disponibile = "red";
                }
                else risorsa.Disponibile = "green";
            }
            model.risorse = risorse;
            model.numeroRisorse = risorse.Count();
            return PartialView("subpartial/_ElencoRisorse", model);

        }

        [HttpPost]
        public ActionResult Ricerca(EsperienzeProduzioneModel model)
        {
            var db = new IncentiviEntities();

            string dataDal = model.parametriricerca.DataDal.ToString("yyyyMMdd");
            string dataAl = model.parametriricerca.DataAl.ToString("yyyyMMdd");

            var query = db.SINTESI1.GroupJoin(db.XR_ORE_TSVESPERPROD
                                                    .GroupJoin(db.XR_ORE_TSVRUOLIESPERPROD, c=>c.ID_ESPERIENZE, d=>d.IDESPERPROD, (c,d) => new { EsperProd = c, Ruoli = d }), 
                                                a => a.COD_MATLIBROMAT, b => b.EsperProd.MATRICOLA, (a, b) => new { SINTESI1 = a, Esperienze = b.DefaultIfEmpty() });

            switch (model.parametriricerca.Dipendente)
            {
                case "I":
                    query = query.Where(x => x.SINTESI1.COD_TPCNTR == "9" || x.SINTESI1.COD_TPCNTR == "K");
                    break;
                case "D":
                    query = query.Where(x => x.SINTESI1.COD_TPCNTR == "8" || x.SINTESI1.COD_TPCNTR == "H" || x.SINTESI1.COD_TPCNTR == "G");
                    break;
                default: break;
            }

            switch (model.parametriricerca.RicercaScelta)
            {
                case "D":
                    /* Tipo Ricerca = Disponibili */
                    query = query.Where(x => !db.XR_ORE_TSVESPERPRODSINT.Any(a => a.MATRICOLA == x.SINTESI1.COD_MATLIBROMAT
                                                                            && a.INIZIO_PERIODO_ESP.CompareTo(dataDal) <= 0
                                                                            && a.FINE_PERIODO_ESP.CompareTo(dataAl) >= 0)
                                            || (db.XR_ORE_TSVESPERPRODSINT.Where(a => a.MATRICOLA == x.SINTESI1.COD_MATLIBROMAT
                                                                            && a.INIZIO_PERIODO_ESP.CompareTo(dataDal) <= 0
                                                                            && a.FINE_PERIODO_ESP.CompareTo(dataAl) >= 0)
                                                                            .Sum(a => a.PERC_ASSEGNAZIONE) < 99
                                             && !db.XR_ORE_TSVESPERPRODSINT.Any(a => a.MATRICOLA == x.SINTESI1.COD_MATLIBROMAT
                                                                        && a.INIZIO_PERIODO_ESP.CompareTo(dataDal) <= 0
                                                                        && a.FINE_PERIODO_ESP.CompareTo(dataAl) >= 0
                                                                        && a.CALCOLO_AUTOMATICO == "S")));
                    break;
                case "A":
                    /* Tipo Ricerca = Allocati */
                    query = query.Where(x => db.XR_ORE_TSVESPERPRODSINT.Any(a => a.MATRICOLA == x.SINTESI1.COD_MATLIBROMAT
                                                                            && a.INIZIO_PERIODO_ESP.CompareTo(dataDal) <= 0
                                                                            && a.FINE_PERIODO_ESP.CompareTo(dataAl) >= 0
                                                                            && (a.CALCOLO_AUTOMATICO == "S" || a.PERC_ASSEGNAZIONE > 99)));
                    break;
                case "P":
                    /* Tipo Ricerca = Allocati/Prenotati */
                    query = query.Where(x => db.XR_ORE_TSVESPERPRODSINT.Any(a => a.MATRICOLA == x.SINTESI1.COD_MATLIBROMAT
                                                        && a.INIZIO_PERIODO_ESP.CompareTo(dataDal) <= 0
                                                        && a.FINE_PERIODO_ESP.CompareTo(dataAl) >= 0
                                                        && a.CALCOLO_AUTOMATICO == "P"));
                    break;
                default: break;
            }

            /* Controllo Direzione */
            if (model.parametriricerca.ServiziSel != null && model.parametriricerca.ServiziSel.Any())
                query = query.Where(x => model.parametriricerca.ServiziSel.Contains(x.SINTESI1.COD_SERVIZIO.Trim()));

            /* Controllo figure professionali */
            if (model.parametriricerca.FiguraSel != null && model.parametriricerca.FiguraSel.Any())
                query = query.Where(x => model.parametriricerca.FiguraSel.Contains(x.SINTESI1.QUALIFICA.COD_QUALSTD));

            /* Controllo ruolo */
            if (model.parametriricerca.ConprofSel != null && model.parametriricerca.ConprofSel.Any())
                query = query.Where(x => x.Esperienze.Any(a => a.Ruoli.Any(y => y.STATO == "C" && y.RUOLOPRINCIPALE == "S" && model.parametriricerca.ConprofSel.Contains(y.CODRUOLO))));

            /* Controllo matricola */
            if (!String.IsNullOrWhiteSpace(model.parametriricerca.Matricola))
                query = query.Where(x => model.parametriricerca.Matricola.Contains(x.SINTESI1.COD_MATLIBROMAT));

            /* Controllo nominativo */
            if (!String.IsNullOrWhiteSpace(model.parametriricerca.Nominativo))
                query = query.Where(x => (x.SINTESI1.DES_COGNOMEPERS + " " + x.SINTESI1.DES_NOMEPERS).StartsWith(model.parametriricerca.Nominativo.ToUpper()));
            
            var risorse = query.ToList().Select(x => new RisorsaProd()
            {
                Matricola = x.SINTESI1.COD_MATLIBROMAT,
                Nominativo = x.SINTESI1.DES_COGNOMEPERS + " " + x.SINTESI1.DES_NOMEPERS,
                Direzione = x.SINTESI1.DES_SERVIZIO,
                Figura = x.SINTESI1.QUALIFICA != null ? x.SINTESI1.QUALIFICA.TB_QUALSTD.DES_QUALSTD : null,
                dataInizio = x.Esperienze.Where(a => a != null && a.EsperProd.INIZIO_PERIODO_ESP.CompareTo(dataAl) <= 0 && dataDal.CompareTo(a.EsperProd.FINE_PERIODO_ESP) <= 0).Select(a => a.EsperProd.INIZIO_PERIODO_ESP).Min(),
                dataFine = x.Esperienze.Where(a => a != null && a.EsperProd.INIZIO_PERIODO_ESP.CompareTo(dataAl) <= 0 && dataDal.CompareTo(a.EsperProd.FINE_PERIODO_ESP) <= 0).Select(a => a.EsperProd.FINE_PERIODO_ESP).Max(),
                numProd = x.Esperienze.Count(a => a != null && a.EsperProd.INIZIO_PERIODO_ESP.CompareTo(dataAl) <= 0 && dataDal.CompareTo(a.EsperProd.FINE_PERIODO_ESP) <= 0),
                percentuale = !x.Esperienze.Any(a => a != null && a.EsperProd.INIZIO_PERIODO_ESP.CompareTo(dataAl) <= 0 && dataDal.CompareTo(a.EsperProd.FINE_PERIODO_ESP) <= 0) ? 0
                              : x.Esperienze.Any(a => a != null && a.EsperProd.INIZIO_PERIODO_ESP.CompareTo(dataAl) <= 0 && dataDal.CompareTo(a.EsperProd.FINE_PERIODO_ESP) <= 0 && a.EsperProd.CALCOLO_AUTOMATICO == "S") ? 101
                              : x.Esperienze.Where(a => a != null && a.EsperProd.INIZIO_PERIODO_ESP.CompareTo(dataAl) <= 0 && dataDal.CompareTo(a.EsperProd.FINE_PERIODO_ESP) <= 0 && a.EsperProd.CALCOLO_AUTOMATICO != "S" && a.EsperProd.PERC_ASSEGNAZIONE != null).Sum(a => a.EsperProd.PERC_ASSEGNAZIONE.Value)
            }).ToList();

            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;

            foreach (RisorsaProd risorsa in risorse)
            {
                if (risorsa.numProd != 0)
                {
                    if (DateTime.ParseExact(risorsa.dataInizio, "yyyyMMdd", provider).CompareTo(model.parametriricerca.DataDal) > 0 || DateTime.ParseExact(risorsa.dataFine, "yyyyMMdd", provider).CompareTo(model.parametriricerca.DataAl) < 0)
                    {
                        risorsa.Disponibile = "orange";
                    }
                    else risorsa.Disponibile = "red";
                }
                else risorsa.Disponibile = "green";
            }
            model.risorse = risorse;
            model.numeroRisorse = risorse.Count();
            return PartialView("subpartial/_ElencoRisorse", model);
        }

        #endregion
    }
}
