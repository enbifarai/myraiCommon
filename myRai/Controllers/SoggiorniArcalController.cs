using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Globalization;
using myRaiData;
using myRaiHelper;
using myRaiCommonModel;

namespace myRai.Controllers
{
    public class SoggiorniArcalController : BaseCommonController
    {
        public ActionResult Index()
        {
            try
            {
                string matricola = CommonHelper.GetCurrentUserMatricola();
                SoggiorniArcal modello = new SoggiorniArcal();
                modello.StoricoSoggiorni = getModel_StoricoSoggiorni(matricola);

                return View(modello);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        private StoricoSoggiorni getModel_StoricoSoggiorni(string matricola)
        {
            StoricoSoggiorni modello = new StoricoSoggiorni();
            try
            {
                modello.SoggiorniRichiesti = getSoggiorniUtente(matricola);
                modello.StatiRichieste = getStatiRichieste();
                return modello;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<StatoRichiesta> getStatiRichieste()
        {
            try
            {
                List<StatoRichiesta> statiRichieste = new List<StatoRichiesta>();
                using (HRPADBEntities db = new HRPADBEntities())
                {
                    var statiDbEntity = db.THRPA_Stati
                        .Where(n => n.APPL_TSTATI == "HRARCAL" && (n.SEQUENZA_TSTATI < 500 || n.SEQUENZA_TSTATI > 899))
                        .OrderBy(n => n.SEQUENZA_TSTATI)
                        .ToList();

                    foreach (var statoDbEntity in statiDbEntity)
                    {
                        var statoRichiesta = new StatoRichiesta();
                        statoRichiesta.StatoRichiestaCode = statoDbEntity.STATO_TSTATI;
                        statoRichiesta.StatoRichiestaBreve = statoDbEntity.DESCRBREVE_TSTATI;
                        statoRichiesta.StatoRichiestaDetail = statoDbEntity.DESCR_TSTATI;
                        statoRichiesta.StatoRichiestaIcona = statoDbEntity.ICO_TSTATI;
                        statiRichieste.Add(statoRichiesta);
                    }
                }
                return statiRichieste;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Ottiene lo storico dei soggiorni Arcal fatti da una matricola Rai.
        /// </summary>
        /// <param name="targetMatricola">Matricola Rai.</param>
        /// <returns></returns>
        private List<Soggiorno> getSoggiorniUtente(string targetMatricola)
        {
            try
            {
                using (HRPADBEntities db = new HRPADBEntities())
                {
                    var TrionSoggScelta1 = db.TRION_Richiedenti
                        .Where(n => n.MatrRichiedente_Richiedenti == targetMatricola)
                        .Join(db.TRION_Richieste.Where(m => m.SceltaAttiva_Richieste == m.FlagSecondaScelta_Richieste),
                                n => new { a = n.CodCatalogo_Richiedenti, b = n.CodRichiesta_Richiedenti },
                                m => new { a = m.CodCatalogo_Richieste, b = m.CodRichiesta_Richieste },
                                (n, m) => new { richiesta = m, richiedente = n })
                        .ToList()
                        .Select(s => new Soggiorno
                        {
                            CodeRichiesta = s.richiedente.CodRichiesta_Richiedenti,
                            CodeStatusRichiesta = (s.richiesta.StatoRichiesta_Richieste ?? "").Trim().ToUpper(),
                            CodeCatalago = s.richiedente.CodCatalogo_Richiedenti,
                            CodeScelta = 1,
                            NomeCatalago = s.richiesta.DesCatalogo_Richieste,
                            NomeStruttura = s.richiesta.DesStruttura_Richieste,
                            NottiSoggiorno = s.richiesta.NumNotti_Richieste,
                            InizioSoggiorno = DateTime.ParseExact(s.richiesta.DataInizio_Richieste, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None),
                            FineSoggiorno = DateTime.ParseExact(s.richiesta.DataFine_Richieste, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None)
                        })
                        .ToList();

                    var TsoggSoggScelta1 = db.TSOGG_Soggiorno
                        .Where(n => n.flag_strutt_dom <= 1 &&
                                n.notti_strutt1_dom > 0 &&
                                n.localita_strutt1_dom != "MOSCA")
                        .Join(db.TSOGG_Richiedente.Where(m => m.matricola_ric == targetMatricola),
                                n => n.id_soggiorno_dom,
                                m => m.id_soggiorno,
                                (n, m) => new { soggiorno = n, richiedente = m })
                        .ToList()
                        .Select(s => new Soggiorno
                        {
                            CodeRichiesta = s.soggiorno.id_soggiorno_dom,
                            CodeStatusRichiesta = (s.richiedente.stato_ric ?? "").Trim().ToUpper(),
                            CodeCatalago = 0,
                            CodeScelta = 1,
                            NomeCatalago = s.soggiorno.tipo_soggiorno_dom,
                            NomeStruttura = s.soggiorno.desc_strutt1_dom + " (" + s.soggiorno.localita_strutt1_dom + ")",
                            NottiSoggiorno = s.soggiorno.notti_strutt1_dom,
                            InizioSoggiorno = DateTime.ParseExact(s.soggiorno.dalA_strutt1_dom, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                            FineSoggiorno = DateTime.ParseExact(s.soggiorno.alA_strutt1_dom, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                        })
                        .ToList();

                    var TsoggSoggScelta2 = db.TSOGG_Soggiorno
                        .Where(n => n.flag_strutt_dom > 1 &&
                                n.notti_strutt2_dom > 0 &&
                                n.localita_strutt2_dom != "MOSCA")
                        .Join(db.TSOGG_Richiedente.Where(m => m.matricola_ric == targetMatricola),
                                n => n.id_soggiorno_dom,
                                m => m.id_soggiorno,
                                (n, m) => new { soggiorno = n, richiedente = m })
                        .ToList()
                        .Select(s => new Soggiorno
                        {
                            CodeRichiesta = s.soggiorno.id_soggiorno_dom,
                            CodeStatusRichiesta = s.richiedente.stato_ric,
                            CodeCatalago = 0,
                            CodeScelta = 2,
                            NomeCatalago = s.soggiorno.tipo_soggiorno_dom,
                            NomeStruttura = s.soggiorno.desc_strutt2_dom + " (" + s.soggiorno.localita_strutt2_dom + ")",
                            NottiSoggiorno = s.soggiorno.notti_strutt2_dom,
                            InizioSoggiorno = DateTime.ParseExact(s.soggiorno.dalA_strutt2_dom, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                            FineSoggiorno = DateTime.ParseExact(s.soggiorno.alA_strutt2_dom, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                        })
                        .ToList();

                    var SoggiorniUtente = new List<Soggiorno>();
                    SoggiorniUtente.AddRange(TrionSoggScelta1);
                    SoggiorniUtente.AddRange(TsoggSoggScelta1);
                    SoggiorniUtente.AddRange(TsoggSoggScelta2);


                    //Carimento partecipanti al soggiorno
                    foreach (var sogg in SoggiorniUtente)
                    {
                        try
                        {
                            if (sogg.CodeCatalago != 0)     //Soggiorno > 2007 su TRION_Richieste
                            {
                                sogg.Partecipanti = db.TRION_Partecipanti
                                    .Where(n => n.MatrRichiedente_Partecipanti == targetMatricola &&
                                        n.CodCatalogo_Partecipanti == sogg.CodeCatalago &&
                                        n.CodRichiesta_Partecipanti == sogg.CodeRichiesta &&
                                        n.FlagSecondaScelta_Partecipanti == 1 &&
                                        n.DataNascita_Partecipanti != "00000000")
                                    .Select(n => new
                                    {
                                        n.Nominativo_Partecipanti,
                                        n.DataNascita_Partecipanti,
                                        n.ProgrAlloggio_Partecipanti,
                                        n.DesAlloggio_Partecipanti,
                                    })
                                    .OrderBy(n => n.Nominativo_Partecipanti)
                                    .ToList()
                                    .Select(n => new Partecipante
                                    {
                                        Nominativo = n.Nominativo_Partecipanti,
                                        DataDiNascita = dbDateTimeParser(n.DataNascita_Partecipanti, "yyyyMMdd"),
                                        CodeProgAlloggio = n.ProgrAlloggio_Partecipanti.ToString(),
                                        DescrizAlloggio = n.DesAlloggio_Partecipanti,
                                    })
                                    .ToList();
                            }
                            else                           //Soggiorno < 2007 su TSOGG_Soggiorno
                            {
                                sogg.Partecipanti = db.TSOGG_Partecipanti
                                    .Where(n => n.id_soggiorno == sogg.CodeRichiesta)
                                    .Select(n => new
                                    {
                                        n.nominativo_par,
                                        n.data_nasci_par,
                                        n.sist1_p1_par,
                                        n.sist1_p3_par,
                                        n.sist2_p1_par,
                                        n.sist2_p3_par,
                                    })
                                    .OrderBy(n => n.nominativo_par)
                                    .ToList()
                                    .Select(n => new Partecipante
                                    {
                                        Nominativo = n.nominativo_par,
                                        DataDiNascita = dbDateTimeParser(n.data_nasci_par, "dd/MM/yyyy"),
                                        CodeProgAlloggio = sogg.CodeScelta == 1 ? n.sist1_p1_par : n.sist2_p1_par,
                                        DescrizAlloggio = sogg.CodeScelta == 1 ? n.sist1_p3_par : n.sist2_p3_par,
                                    })
                                    .ToList();
                            }
                        }
                        catch (Exception ex)
                        {
                            sogg.Partecipanti = null;
                        }
                    }


                    //Adattamenti forzati particolari
                    foreach (var sogg in SoggiorniUtente)
                    {
                        switch (sogg.NomeCatalago.ToUpper())
                        {
                            case "EST":
                                sogg.NomeCatalago = sogg.InizioSoggiorno.HasValue ? sogg.InizioSoggiorno.Value.Year.ToString() + " - " : "";
                                sogg.NomeCatalago = sogg.NomeCatalago + "Soggiorni Estivi";
                                break;
                            case "ISO":
                                sogg.NomeCatalago = sogg.InizioSoggiorno.HasValue ? sogg.InizioSoggiorno.Value.Year.ToString() + " - " : "";
                                sogg.NomeCatalago = sogg.NomeCatalago + "Soggiorni Isole";
                                break;
                            case "INV":
                                sogg.NomeCatalago = sogg.InizioSoggiorno.HasValue ? sogg.InizioSoggiorno.Value.Year.ToString() + " - " : "";
                                sogg.NomeCatalago = sogg.NomeCatalago + "Soggiorni Invernali";
                                break;
                            default:
                                break;
                        }

                        if (sogg.CodeStatusRichiesta == "ACCETTATA ST" && sogg.FineSoggiorno < DateTime.Now)
                        {
                            sogg.CodeStatusRichiesta = "SOGTERM";
                        }

                        if (sogg.CodeStatusRichiesta == "TURNO EFF")
                        {
                            string codecat = sogg.CodeCatalago.ToString();

                            var cons = db.TSOGG_SchedaConsuntivi.FirstOrDefault(n =>
                                n.matrpar_CONS == targetMatricola &&
                                n.stagione_CONS == codecat &&
                                n.id_Soggiorno_CONS == sogg.CodeRichiesta);

                            sogg.CodeStatusRichiesta = cons != null ? cons.stato_CONS : "SOGTERM";
                        }

                        //Set Status Valutazione Soggiorno
                        if (sogg.CodeStatusRichiesta == "ACCETTATA")
                        {
                            sogg.CodeStatusValutazione = "VALUTADISABLE";
                        }
                        else if (DateTime.Now > sogg.InizioSoggiorno && sogg.CodeCatalago > 0)
                        {
                            var valutaz = db.TSOGG_ValutazioniSogg.Count(n => n.id_dom_val == "" && n.Matricola_val == targetMatricola);
                            if (valutaz < 1)
                            {
                                sogg.CodeStatusValutazione = "VALUTAABLE";
                            }
                            else
                            {
                                sogg.CodeStatusValutazione = "VALUTACOMPILATO";
                            }
                        }
                        else
                        {
                            sogg.CodeStatusValutazione = "VALUTADISABLE";
                        }
                    }

                    if (SoggiorniUtente.Any())
                    {
                        SoggiorniUtente = SoggiorniUtente.OrderByDescending(n => n.InizioSoggiorno).ToList();
                    }

                    return SoggiorniUtente;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public DateTime? dbDateTimeParser(string dateAsString, string dateFormat)
        {
            try
            {
                return DateTime.ParseExact(dateAsString, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
