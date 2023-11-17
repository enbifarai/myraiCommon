using System;
using System.Collections.Generic;
using System.Linq;
using myRai.DataAccess;

using myRaiCommonModel;
using myRaiData;
using System.Web.Mvc;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System.Data;
using myRaiHelper;
using myRaiServiceHub.Autorizzazioni;
using System.Data.Objects;
using Newtonsoft.Json;
using myRai.Business;
using myRai.Models;
using System.Globalization;

namespace myRaiCommonManager
{
    public class HomeManager
    {
        public static string GetSedeGappPrecedente(string matricola, DateTime Date)
        {
            var db = new digiGappEntities();
            var item = db.MyRai_SediGappPrecedenti.Where(x => x.Matricola == matricola).FirstOrDefault();
            if (item == null)
                return null;
            else
            {
                DateTime D1;
                DateTime.TryParseExact(item.Dal, "ddMMyyyy", null, DateTimeStyles.None, out D1);
                DateTime D2;
                DateTime.TryParseExact(item.Al, "ddMMyyyy", null, DateTimeStyles.None, out D2);
                if (Date > D1 && Date < D2.AddDays(1))
                    return item.SedePrecedente.Trim();
                else
                    return null;
            }
        }
        public static int GetSaldoPohRohAdOggi(MyRaiServiceInterface.MyRaiServiceReference1.DateMinutiEccezione[] DettagliEccezioni, DateTime data, string codice)
        {
            int MinutiPOH = 0;
            if (DettagliEccezioni.Any(x => x.eccezione == "POH" && x.data <= data))
            {
                MinutiPOH = DettagliEccezioni.Where(x => x.eccezione == "POH" && x.data <= data).Sum(x => x.minuti);
            }

            int MinutiROH = 0;
            if (DettagliEccezioni.Any(x => x.eccezione == "ROH" && (codice == "POH" ? x.data < data : x.data <= data)))
            {
                MinutiROH = DettagliEccezioni.Where(x => x.eccezione == "ROH" && (codice == "POH" ? x.data < data : x.data <= data)).Sum(x => x.minuti);
            }
            return MinutiPOH - MinutiROH;
        }

        public static bool GetCeitonAttivitaObbligatoriaPerSede()
        {
            var db = new digiGappEntities();
            var d = DateTime.Now;
            var sede = Utente.SedeGapp(d);

            var s = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sede && x.data_inizio_validita <= d && x.data_fine_validita >= d).FirstOrDefault();

            return (s != null && s.ceiton_obbligatorio);
        }

        public static List<MyRai_Sezioni_Visibili> GetSezioniVisibili(string matricola, string siglasezione = " ")
        {
            var db = new digiGappEntities();
            var sezionivisibili = db.MyRai_Sezioni_Visibili.Where(x => x.Permessi == "*" || x.Permessi.Contains(matricola)).OrderBy(x => x.Sigla_Sezione.ToUpper()).ToList();
            if (!string.IsNullOrWhiteSpace(siglasezione))
            {
                sezionivisibili = sezionivisibili.Where(x => x.Sigla_Sezione == siglasezione).OrderBy(x => x.Sigla_Sezione).ToList();
            }
            if (!myRai.Models.Utente.IsAbilitatoGapp())
            {
                sezionivisibili.RemoveAll((x => x.Sigla_Sezione.TrimEnd() == "GIOEV"));
            }
            if (!myRai.Models.Utente.GestitoSirio())
            {
                sezionivisibili.RemoveAll((x => x.Sigla_Sezione.TrimEnd() == "APLAN"));
            }
            return sezionivisibili;
        }

        public static string GetJSfunzioneIniziale(int? idscelta)
        {
            if (idscelta == null) return null;
            var db = new digiGappEntities();
            var p = db.MyRai_SceltaPercorso.Where(x => x.Id == idscelta).FirstOrDefault();
            if (p == null) return null;
            else return p.Parametri;

        }

        public static int GetMinutiUltimaTimbratura(dayResponse resp)
        {
            if (resp == null || resp.giornata == null || resp.giornata.timbrature == null)
                return 0;
            var timbratureNonBilanciate = resp.giornata.timbrature.ToList().Where(x => x.entrata.insediamento != "082"
                && x.uscita.insediamento != "082").ToList();

            if (timbratureNonBilanciate == null || timbratureNonBilanciate.Count == 0)
                return 0;

            string uscita = timbratureNonBilanciate[timbratureNonBilanciate.Count - 1].uscita.orario;
            int min = EccezioniManager.calcolaMinuti(uscita);
            return min;
        }

        public static List<MiaRichiesta> GetMieRichiesteModel(int? idstato = null, string ecc = null, string dal = null, string al = null, string search = null)
        {

            Boolean IsFromSituazione = search == null;


            DateTime dataDa;
            DateTime dataA;

            bool convDA = DateTime.TryParseExact(dal, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataDa);
            bool convA = DateTime.TryParseExact(al, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataA);

            var richieste = new List<MiaRichiesta>();
            var db = new digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();

            string datachiusura = Utente.GetDateBackPerEvidenze();
            DateTime DataChiusura;
            Boolean conv = DateTime.TryParseExact(datachiusura, "ddMMyyyy", null,
                                    System.Globalization.DateTimeStyles.None, out DataChiusura);

            List<MyRai_Richieste> mierich = new List<MyRai_Richieste>();
            if (IsFromSituazione)
            {
                mierich = db.MyRai_Richieste.Where(x => x.matricola_richiesta == matr)
                                     .Where(x => x.periodo_dal >= DataChiusura || x.id_stato == 10)
                                     .Where(x => convA == false || (convA == true && x.periodo_al <= dataA))
                                     .Where(x => idstato == null || x.id_stato == idstato)
                                     .Where(x => ecc == null || ecc == "" || x.MyRai_Eccezioni_Richieste.Select(z => z.cod_eccezione).FirstOrDefault() == ecc)
                 .OrderByDescending(x => x.periodo_dal)
                 //.ThenByDescending(x => x.periodo_dal)
                 .ToList();
            }
            else
            {
                mierich = db.MyRai_Richieste.Where(x => x.matricola_richiesta == matr)
                                       .Where(x => convDA == false || (convDA == true && x.periodo_dal >= dataDa))
                                       .Where(x => convA == false || (convA == true && x.periodo_al <= dataA))
                                       .Where(x => idstato == null || x.id_stato == idstato)
                                       .Where(x => ecc == null || ecc == "" || x.MyRai_Eccezioni_Richieste.Select(z => z.cod_eccezione).FirstOrDefault() == ecc)
                   .OrderByDescending(x => x.periodo_dal)
                   //.ThenByDescending(x => x.periodo_dal)
                   .ToList();
            }

            var eccNoGapp = db.MyRai_Eccezioni_Ammesse.Where(x => x.no_corrispondenza_gapp).Select(x => x.cod_eccezione.Trim()).ToList();

            foreach (var r in mierich)
            {
                string cod = r.MyRai_Eccezioni_Richieste.Select(x => x.cod_eccezione).FirstOrDefault();
                MyRai_Eccezioni_Ammesse amm = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == cod).FirstOrDefault();
                if (amm == null) continue;

                //in ordine di azione prende prima gli storni azione=C
                var ri = r.MyRai_Eccezioni_Richieste.OrderBy(x => x.azione).FirstOrDefault();
                int? idDoc = null;
                if (r.MyRai_Associazione_Richiesta_Doc.Count > 0)
                {
                    idDoc = r.MyRai_Associazione_Richiesta_Doc.First().id_documento;
                }

                var newr = new MiaRichiesta()
                {
                    EccezioneCorrenteDaDB = ri,
                    Data = r.data_richiesta,
                    TestoRichiesta = amm != null ? amm.desc_eccezione : "",
                    Stato = ri.id_stato.ToString(),
                    IdRichiesta = r.id_richiesta,
                    IdStatoRichiesta = r.id_stato,
                    IsStorno = (ri.azione == "C"),
                    NumeroDocumento = ri.numero_documento,
                    DataRifiutoLiv1 = ri.data_rifiuto_primo_livello,
                    NominativoLiv1 = ri.nominativo_primo_livello,
                    DataValidazioneLiv1 = ri.data_validazione_primo_livello,
                    NotaRifiutoOApprovazione = ri.nota_rifiuto_o_approvazione,
                    NdocChildrenCsv = String.Join(",", r.MyRai_Eccezioni_Richieste.Select(x => x.numero_documento).ToArray()),
                    IdDocumentoAssociato = idDoc,
                    no_corrispondenza_gapp = eccNoGapp.Contains(cod.Trim())

                };
                if (newr.IsStorno)
                {
                    newr.EccezioneDiRiferimentoPerStorno = db.MyRai_Eccezioni_Richieste
                        .Where(x => x.numero_documento == ri.numero_documento_riferimento &&
                            x.codice_sede_gapp == ri.codice_sede_gapp
                            ).FirstOrDefault();


                }
                newr.Periodo = CommonManager.GetPeriodo(r, EnumFormatoPeriodo.MieRichieste);
                string[] col = CommonManager.GetPeriodoMieRichieste(r);
                newr.PeriodoRichiesta1 = col[0];
                newr.PeriodoRichiesta2 = col[1];
                richieste.Add(newr);
            }
            return richieste;
        }
        public static EccezioneSostitutivaInfo GetEccezioneSostitutivaInfo(int IdRichiestaEccezione )
        {
            var db = new digiGappEntities();
            MyRai_Eccezioni_Richieste m = db.MyRai_Eccezioni_Richieste.FirstOrDefault(x =>
            x.id_eccezioni_richieste == IdRichiestaEccezione);

            if (m != null && m.MyRai_Richieste.MyRai_Eccezioni_Richieste.Any(x => x.cod_eccezione.Trim() == "SW" && x.azione == "C")
                    && m.MyRai_Richieste.id_stato == 10)
            {
                var stornosw = m.MyRai_Richieste.MyRai_Eccezioni_Richieste.FirstOrDefault(x => x.cod_eccezione.Trim() == "SW" && x.azione == "C");
                if (!String.IsNullOrWhiteSpace(stornosw.eccezione_rimpiazzo_storno))
                {
                    var EccSost = new EccezioneSostitutivaInfo();
                    EccSost.EccezioneSostitutivaCodice = stornosw.eccezione_rimpiazzo_storno;
                    if (stornosw.eccezione_rimpiazzo_dalle != null)
                        EccSost.EccezioneSostitutivaDalle = ((DateTime)stornosw.eccezione_rimpiazzo_dalle).ToString("HH:mm");
                    if (stornosw.eccezione_rimpiazzo_alle != null)
                        EccSost.EccezioneSostitutivaAlle = ((DateTime)stornosw.eccezione_rimpiazzo_alle).ToString("HH:mm");
                    EccSost.EccezioneSostitutivaSWH = (stornosw.eccezione_rimpiazzo_richiedeSWH == true);
                    return EccSost;
                }
            }
            return null;
        }
        public static PopupDettaglioGiornata GetPopupDettaglioGiornataModel(int IdRichiestaEccezione)
        {
            var db = new digiGappEntities();
            MyRai_Eccezioni_Richieste m = db.MyRai_Eccezioni_Richieste.FirstOrDefault(x => x.id_eccezioni_richieste == IdRichiestaEccezione);
            PopupDettaglioGiornata model = new PopupDettaglioGiornata();

            if (m != null)
            {
                model.EccSost = GetEccezioneSostitutivaInfo(IdRichiestaEccezione);
                //if (m.MyRai_Richieste.MyRai_Eccezioni_Richieste.Any(x => x.cod_eccezione.Trim() == "SW" && x.azione == "C")
                //    && m.MyRai_Richieste.id_stato == 10)
                //{
                //    var stornosw = m.MyRai_Richieste.MyRai_Eccezioni_Richieste.FirstOrDefault(x => x.cod_eccezione.Trim() == "SW" && x.azione == "C");
                //    if ( ! String.IsNullOrWhiteSpace(stornosw.eccezione_rimpiazzo_storno))
                //    {
                //        model.EccSost = new EccezioneSostitutivaInfo();
                //        model.EccSost.EccezioneSostitutivaCodice = stornosw.eccezione_rimpiazzo_storno;
                //        if (stornosw.eccezione_rimpiazzo_dalle != null)
                //            model.EccSost.EccezioneSostitutivaDalle = ((DateTime)stornosw.eccezione_rimpiazzo_dalle).ToString("HH:mm");
                //        if (stornosw.eccezione_rimpiazzo_alle != null)
                //            model.EccSost.EccezioneSostitutivaAlle = ((DateTime)stornosw.eccezione_rimpiazzo_alle).ToString("HH:mm");
                //        model.EccSost.EccezioneSostitutivaSWH = (stornosw.eccezione_rimpiazzo_richiedeSWH==true);
                //    }
                //}
                model.NoteSegreteria = m.nota_segreteria;

                string matricola7 = m.MyRai_Richieste.matricola_richiesta.PadLeft(7, '0');

                MyRai_Note_Da_Segreteria segreteria = (from nota in db.MyRai_Note_Da_Segreteria
                                                       where (nota.Matricola == matricola7) &&
                                                       EntityFunctions.TruncateTime(nota.Data) ==
                                                           EntityFunctions.TruncateTime(m.data_eccezione)
                                                       select nota).FirstOrDefault();

                if (segreteria != null)
                {
                    model.NoteSegreteria = segreteria.Nota;
                }

                model.CodiceSedeGapp = m.codice_sede_gapp;
                model.DescrizioneSedeGapp = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == m.codice_sede_gapp).Select(x => x.desc_sede_gapp).FirstOrDefault();
                model.Nominativo = m.MyRai_Richieste.nominativo;
                model.Matricola = m.MyRai_Richieste.matricola_richiesta;
                model.TipoRichiesta = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == m.cod_eccezione).Select(x => x.desc_eccezione).FirstOrDefault();
                model.DataEccezione = m.data_eccezione;
                model.DataDalle = m.dalle;
                model.DataAlle = m.alle;
                model.MotivoRichiesta = m.motivo_richiesta;
                model.IdStatoRichiesta = m.id_stato;
                model.ImmagineBase64 = CommonManager.GetImmagineBase64(m.MyRai_Richieste.matricola_richiesta);
                model.InServizio = IsInServizio(m.MyRai_Richieste.matricola_richiesta);
                model.ShowTimbrature = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == m.cod_eccezione)
                                       .Select(x => x.id_raggruppamento).FirstOrDefault() != 1;

                if (m.MyRai_Richieste.periodo_dal != m.MyRai_Richieste.periodo_al)
                    model.PeriodoPiuGiorni = "Dal " + m.MyRai_Richieste.periodo_dal.ToString("dd/MM/yyyy") + " al " + m.MyRai_Richieste.periodo_al.ToString("dd/MM/yyyy");

                model.ApprovataDa = m.nominativo_primo_livello;

                if (m.azione == "C" && m.MyRai_Richieste.MyRai_Eccezioni_Richieste.Any(x => x.azione == "I"))
                {
                    MyRai_Eccezioni_Richieste EccezioneDaStornare = m.MyRai_Richieste.MyRai_Eccezioni_Richieste.First(x => x.azione == "I");
                    model.EccezioneDaStornareApprovataDa = EccezioneDaStornare.nominativo_primo_livello;
                    model.EccezioneDaStornareDataValidazione = EccezioneDaStornare.data_validazione_primo_livello;
                }


                if (!(string.IsNullOrEmpty(m.ValoriParamExtraJSON)))
                    model.ParametriExtra = Utility.GetDictionaryFromJson(m.ValoriParamExtraJSON);

                //List<Dictionary<string, string>> ValueList = new List<Dictionary<string,string>>();
                //if (!(string.IsNullOrEmpty(m.ValoriParamExtraJSON)))
                //    ValueList = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(m.ValoriParamExtraJSON);
                //model.ParametriExtra = ValueList;

                model.ParametriRichiesta = new List<ParametroRichiesto>();
                if (m.importo != null)
                    model.ParametriRichiesta.Add(new ParametroRichiesto()
                    {
                        NomeParametro = "Importo",
                        ValoreParametro = m.importo.ToString()
                    });
                if (m.quantita != null)
                    model.ParametriRichiesta.Add(new ParametroRichiesto()
                    {
                        NomeParametro = "Quantita",
                        ValoreParametro = m.MyRai_Richieste.MyRai_Eccezioni_Richieste.Where(x => x.azione == "I" && x.numero_documento > 0).Select(x => x.quantita).Sum().ToString()
                    });
                if (m.dalle != null)
                {
                    model.ParametriRichiesta.Add(new ParametroRichiesto()
                    {
                        NomeParametro = "Dalle",
                        ValoreParametro = ((DateTime)m.dalle).ToString("HH:mm")
                    });
                }
                if (m.alle != null)
                {
                    model.ParametriRichiesta.Add(new ParametroRichiesto()
                    {
                        NomeParametro = "Alle",
                        ValoreParametro = ((DateTime)m.alle).ToString("HH:mm")
                    });
                }
                if (m.uorg != null)
                {
                    model.ParametriRichiesta.Add(new ParametroRichiesto()
                    {
                        NomeParametro = "UORG",
                        ValoreParametro = m.uorg
                    });
                }
                if (m.df != null)
                {
                    model.ParametriRichiesta.Add(new ParametroRichiesto()
                    {
                        NomeParametro = "DF",
                        ValoreParametro = m.df
                    });
                }
                if (m.matricola_spettacolo != null)
                {
                    model.ParametriRichiesta.Add(new ParametroRichiesto()
                    {
                        NomeParametro = "Matr.Spett.",
                        ValoreParametro = m.matricola_spettacolo
                    });
                }
            }

            return model;
        }


        public static EnumPresenzaDip IsInServizio(string matricola)
        {
            //WSDigigapp datiBack = new WSDigigapp();
            // datiBack.Credentials =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            // datiBack.Url = "http://digigapp.servizi.rai.it/WSDigigapp.asmx";
            // datiBack.Credentials = new System.Net.NetworkCredential(
            //	CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
            //	CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
            //	);


            //WSDigigapp datiBack = new WSDigigapp()
            //{
            //	Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            //};

            //var resp = datiBack.getEccezioni(matricola, DateTime.Now.ToString("ddMMyyyy"), "BU", 70);

            WSDigigappDataController service = new WSDigigappDataController();

            dayResponse resp = service.GetEccezioni(CommonHelper.GetCurrentUserMatricola(), matricola, DateTime.Now.ToString("ddMMyyyy"), "BU", 70);

            List<Tuple<DateTime, Boolean>> LT = new List<Tuple<DateTime, bool>>();
            if (resp!= null && resp.timbrature != null)
            {
                foreach (var t in resp.timbrature)
                {
                    if (t.entrata != null)
                    {
                        if (t.entrata.orario.ToUpper().Contains("SI"))
                        {
                            LT.Add(new Tuple<DateTime, bool>(
                             new DateTime(DateTime.Now.Year,
                                           DateTime.Now.Month,
                                           DateTime.Now.Day),
                                           true));
                        }
                        else
                        {
                            LT.Add(new Tuple<DateTime, bool>(
                            new DateTime(DateTime.Now.Year,
                                          DateTime.Now.Month,
                                          DateTime.Now.Day,
                                          Convert.ToInt32(t.entrata.orario.Split(':')[0].Replace("<","")),
                                          Convert.ToInt32(t.entrata.orario.Split(':')[1].Replace("<", "")),
                                          0),
                                          true));
                        }
                    }


                    if (t.uscita != null)
                    {
                        if (t.uscita.orario.ToUpper().Contains("SI"))
                        {
                            LT.Add(new Tuple<DateTime, bool>(
                             new DateTime(DateTime.Now.Year,
                                           DateTime.Now.Month,
                                           DateTime.Now.Day),
                                           false));
                        }
                        else
                        {
                            if (t.entrata != null)
                                LT.Add(new Tuple<DateTime, bool>(
                                new DateTime(DateTime.Now.Year,
                                              DateTime.Now.Month,
                                              DateTime.Now.Day,
                                              Convert.ToInt32(t.entrata.orario.Split(':')[0]),
                                              Convert.ToInt32(t.entrata.orario.Split(':')[1]),
                                              0),
                                              false));
                        }
                    }
                }
            }


            EnumPresenzaDip isPresente;
            var UltimaTimbratura = LT.OrderByDescending(x => x.Item1).FirstOrDefault();
            if (UltimaTimbratura != null)
                isPresente = UltimaTimbratura.Item2 ? EnumPresenzaDip.Presente : EnumPresenzaDip.Assente;
            else
                isPresente = EnumPresenzaDip.Assente;

            if (isPresente == EnumPresenzaDip.Assente)
            {
                if (resp!=null && resp.giornata!=null && resp.giornata.eccezioni!=null)
                {
                    var eccSW = resp.giornata.eccezioni.FirstOrDefault(x => x.cod.StartsWith("SW"));
                    if (eccSW != null)
                    {
                        if (eccSW.cod == "SW")
                            isPresente = EnumPresenzaDip.SmartWorking;
                        else if (eccSW.cod == "SWH")
                        {
                            int nowHour = DateTime.Now.ToString("HHmm").ToMinutes();
                            if (eccSW.dalle.ToMinutes() <= nowHour && eccSW.alle.ToMinutes() > nowHour)
                                isPresente = EnumPresenzaDip.SmartWorking;
                        }
                    }
                }
            }

            return isPresente;
        }
        public static dayResponse GetEccezioni(string data, string matricola)
        {
            WSDigigapp service = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(myRaiHelper.EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(myRaiHelper.EnumParametriSistema.AccountUtenteServizio)[1])
            };
            var response = service.getEccezioni(matricola, data, "BU", 75);
            return response;

        }
        public static dayResponse GetTimbratureTodayModel(DateTime? datatimbrature = null, string matricola = null, bool MatchDBrichiesto = false, string sessionId=null)
        {
            //WSDigigapp datiBack = new WSDigigapp();
            //// datiBack.Credentials =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            //datiBack.Credentials = new System.Net.NetworkCredential(
            //	CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
            //	CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
            //	);
            //// datiBack.Url = "http://digigapp.servizi.rai.it/WSDigigapp.asmx";

            WSDigigappDataController service = new WSDigigappDataController();
            string serOriginale = "";

            if (matricola == null) matricola = CommonManager.GetCurrentUserMatricola();
            ModelDash model = new ModelDash();
            try
            {
                if (!String.IsNullOrWhiteSpace(sessionId) && SessionHelper.Get(sessionId) != null)
                {
                    string ser= JsonConvert.SerializeObject(SessionHelper.Get(sessionId));
                    model.dettaglioGiornata = JsonConvert.DeserializeObject<dayResponse>(ser);

                   // model.dettaglioGiornata = (dayResponse)HttpContext.Current.Session[sessionId];
                }
                else
                {
                if (datatimbrature == null)
                    //model.dettaglioGiornata = datiBack.getEccezioni(matricola, DateTime.Now.ToString("ddMMyyyy"), "BU", 70);
                        model.dettaglioGiornata = service.GetEccezioni(CommonHelper.GetCurrentUserMatricola(), matricola, DateTime.Now.ToString("ddMMyyyy"), "BU", 70);
                else
                    //model.dettaglioGiornata = datiBack.getEccezioni(matricola, ((DateTime)datatimbrature).ToString("ddMMyyyy"), "BU", 70);
                        model.dettaglioGiornata = service.GetEccezioni(CommonHelper.GetCurrentUserMatricola(), matricola, ((DateTime)datatimbrature).ToString("ddMMyyyy"), "BU", 70);
                }
                
                try { serOriginale = Newtonsoft.Json.JsonConvert.SerializeObject(model.dettaglioGiornata); }
                catch (Exception ex) { }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "portale",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "HomeManager/GetTimbratureTodayModel"
                });
            }
            if (model.dettaglioGiornata != null && model.dettaglioGiornata.timbrature != null)
            {
                foreach (Timbrature t in model.dettaglioGiornata.timbrature)
                {
                    if (t.uscita == null || String.IsNullOrWhiteSpace(t.uscita.orario) || t.uscita.orario.Split(':').Length < 2)
                        continue;
                    string orauscita = t.uscita.orario.Split(':')[0];
                    int h;
                    int.TryParse(orauscita, out h);
                    if (h > 24)
                    {
                        t.uscita.OraGiornoSuccessivo = (h - 24).ToString().PadLeft(2, '0');
                        t.uscita.OraGiornoSuccessivo += ":" + t.uscita.orario.Split(':')[1];
                    }
                }
            }
            digiGappEntities db = new digiGappEntities();
            if (model.dettaglioGiornata != null && model.dettaglioGiornata.eccezioni != null)
                foreach (Eccezione ecc in model.dettaglioGiornata.eccezioni)
                {
                    int NumeroDoc = 0;
                    if (!String.IsNullOrWhiteSpace(ecc.ndoc))
                    {
                        int.TryParse(ecc.ndoc, out NumeroDoc);
                        DateTime dataecc;
                        DateTime.TryParseExact(ecc.data, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out dataecc);

                        if (NumeroDoc != 0)
                        {
                            MyRai_Eccezioni_Richieste er = db.MyRai_Eccezioni_Richieste.Where(x =>
                                x.data_eccezione == dataecc &&
                                x.codice_sede_gapp == model.dettaglioGiornata.giornata.sedeGapp &&
                                x.numero_documento == NumeroDoc).FirstOrDefault();
                            if (er != null)
                            {
                                ecc.IdEccezioneRichiesta = er.id_eccezioni_richieste;
                                ecc.IdRichiestaPadre = er.id_richiesta;
                                ecc.IdStato = er.id_stato;
                                MyRai_Eccezioni_Richieste eccStorno = db.MyRai_Eccezioni_Richieste
                                    .Where(x => x.numero_documento_riferimento == er.numero_documento &&
                                        x.codice_sede_gapp == er.codice_sede_gapp
                                    ).FirstOrDefault();
                                if (eccStorno != null)
                                {
                                    ecc.EsisteStorno = true;
                                    ecc.IdStatoStorno = eccStorno.id_stato;
                                }
                            }
                        }
                    }
                }
            var ListaEccezioniDB = db.MyRai_Eccezioni_Richieste.Where(x => x.MyRai_Richieste.matricola_richiesta == matricola && x.data_eccezione == datatimbrature && x.id_stato != 60).ToList();
            if (ListaEccezioniDB.Any())
            {
                var nogapp = db.MyRai_Eccezioni_Ammesse.Where(x => x.no_corrispondenza_gapp).ToList();
                foreach (var edb in ListaEccezioniDB)
                {
                    if (nogapp.Select(x => x.cod_eccezione.Trim()).Contains(edb.cod_eccezione.Trim()))
                    {
                        Eccezione ec = new Eccezione() { no_corrispondenza_gapp = true };
                        ec.cod = edb.cod_eccezione;
                        ec.DataInserimento = edb.data_creazione;
                        ec.dataInserimento = edb.data_creazione.ToString("ddMMyyyy");
                        ec.IdStato = edb.id_stato;
                        ec.stato_eccezione = " ";
                        ec.EsisteStorno = false;
                        ec.IdEccezioneRichiesta = edb.id_eccezioni_richieste;
                        ec.IdRichiestaPadre = edb.id_richiesta;
                        ec.descrittivaConCodice = edb.cod_eccezione + " - " + nogapp.Where(x => x.cod_eccezione.Trim() == edb.cod_eccezione.Trim()).Select(x => x.desc_eccezione).FirstOrDefault();
                        if (model.dettaglioGiornata.eccezioni != null)
                        {
                            var list = model.dettaglioGiornata.eccezioni.ToList();
                            list.Add(ec);
                            model.dettaglioGiornata.eccezioni = list.ToArray();
                        }
                        else
                        {
                            List<Eccezione> list = new List<Eccezione>();
                            list.Add(ec);
                            model.dettaglioGiornata.eccezioni = list.ToArray();
                        }
                    }
                }
            }



            if (datatimbrature != null)
            {
                var StorniInApprovazione = db.MyRai_Eccezioni_Richieste.Where(x =>
                                           x.id_stato == (int)EnumStatiRichiesta.InApprovazione &&
                                           x.data_eccezione == datatimbrature &&
                                           x.MyRai_Richieste.matricola_richiesta == matricola &&
                                           x.azione == "C").ToList();
                if (StorniInApprovazione.Count > 0)
                {
                    foreach (var ecc in StorniInApprovazione)
                    {
                        Eccezione ec = new Eccezione();
                        ec.IdStato = 1;
                        ec.dataInserimento = ecc.data_creazione.ToString("ddMMyyyy");
                        ec.stato_eccezione = " ";
                        ec.EsisteStorno = true;
                        ec.IdStatoStorno = (int)EnumStatiRichiesta.InApprovazione;
                        ec.IdEccezioneRichiesta = ecc.id_eccezioni_richieste;
                        ec.IdRichiestaPadre = ecc.id_richiesta;
                        ec.descrittivaConCodice = ecc.cod_eccezione + " - " + db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == ecc.cod_eccezione).Select(x => x.desc_eccezione).FirstOrDefault();
                        if (model.dettaglioGiornata.eccezioni != null)
                        {
                            var list = model.dettaglioGiornata.eccezioni.ToList();
                            list.Add(ec);
                            model.dettaglioGiornata.eccezioni = list.ToArray();
                        }
                        else
                        {
                            List<Eccezione> list = new List<Eccezione>();
                            list.Add(ec);
                            model.dettaglioGiornata.eccezioni = list.ToArray();
                        }
                    }
                }
            }
            if (MatchDBrichiesto == true)
            {
                if (datatimbrature == null) datatimbrature = DateTime.Now.Date;
                else datatimbrature = ((DateTime)datatimbrature).Date;
                int rifiutata = (int)EnumStatiRichiesta.Rifiutata;
                int cancellata = (int)EnumStatiRichiesta.Cancellata;
                int eliminata = (int)EnumStatiRichiesta.Eliminata;

                var EccDB = db.MyRai_Eccezioni_Richieste.Where(x => x.data_eccezione == datatimbrature)
                           .Where(x => x.azione == "I" && x.id_stato != rifiutata && x.id_stato != cancellata 
                           && x.id_stato!=eliminata //cerchietto rosso 
                           && x.MyRai_Richieste.matricola_richiesta == matricola)
                           .ToList();
                bool DBtoUpdate = false;
                if (EccDB.Count > 0)
                {
                    foreach (MyRai_Eccezioni_Richieste m in EccDB)
                    {
                        if (m.numero_documento == 0 || m.numero_documento ==null) continue;
                        
                        if (model.dettaglioGiornata.eccezioni != null && !model.dettaglioGiornata.eccezioni.Select(x => x.ndoc).ToList().Contains(m.numero_documento.ToString().PadLeft(6, '0')))
                        {
                           

                            // se non c'e in Gapp perche è approvata (I) e ha uno storno approvato(C)
                            var stornoDB = db.MyRai_Eccezioni_Richieste.Where(x => x.azione == "C" &&
                                   x.numero_documento_riferimento == m.numero_documento &&
                                   x.codice_sede_gapp == m.codice_sede_gapp &&
                                   x.id_stato == (int)EnumStatiRichiesta.Approvata).FirstOrDefault();
                            if (m.id_stato == (int)EnumStatiRichiesta.Approvata && stornoDB != null)
                            {
                                model.dettaglioGiornata = InserisciInEccezioni_Eccezione_StornoApprovati(
                                    model.dettaglioGiornata, m);
                            }
                            else
                            {
                                model.dettaglioGiornata = InserisciInEccezioni(model.dettaglioGiornata, m);

                                myRaiCommonTasks.CommonTasks.InviaNotificaPerEliminataSuGapp(m, "Portale");

                                m.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                m.MyRai_Richieste.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                if (m.MyRai_Richieste.MyRai_Eccezioni_Richieste.Any(x => x.azione == "C"))
                                {
                                    foreach (var st in m.MyRai_Richieste.MyRai_Eccezioni_Richieste.Where(x => x.azione == "C"))
                                    {
                                        st.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                    }
                                }
                                //stornoDB.id_stato = ( int ) EnumStatiRichiesta.Eliminata;

                                string ser = "";
                                try { ser = Newtonsoft.Json.JsonConvert.SerializeObject(model.dettaglioGiornata); }
                                catch (Exception ex) { }

                                Logger.LogAzione(new MyRai_LogAzioni()
                                {
                                    applicativo = "PORTALE",
                                    data = DateTime.Now,
                                    matricola = matricola,
                                    provenienza = "GetTimbratureTodayModel",
                                    operazione = "ELIMINATA SU GAPP",
                                    descrizione_operazione = "id rich:" + m.MyRai_Richieste.id_richiesta + " id ecc_rich:" + m.id_eccezioni_richieste + "numdoc DB:" + m.numero_documento+ " dayResponseOrig:" +serOriginale+ "...............dayResponseMod:"+  ser
                                });
                                DBtoUpdate = true;
                            }
                        }
                    }
                    if (DBtoUpdate)
                        DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                }
            }
            return model.dettaglioGiornata;
        }

        public static dayResponse InserisciInEccezioni_Eccezione_StornoApprovati(dayResponse dettaglioGiornata,
            MyRai_Eccezioni_Richieste er)
        {
            var db = new digiGappEntities();
            Eccezione newEcc = new Eccezione();
            newEcc.alle = er.alle == null ? null : ((DateTime)er.alle).ToString("HH:mm");
            newEcc.cod = er.cod_eccezione;
            newEcc.dalle = er.dalle == null ? null : ((DateTime)er.dalle).ToString("HH:mm");
            newEcc.data = er.data_eccezione.ToString("ddMMyyyy");
            newEcc.dataInserimento = er.data_creazione.ToString("ddMMyyyy");
            newEcc.DataRichiesta = er.data_creazione;
            newEcc.dataStampa = er.data_stampa == null ? null : ((DateTime)er.data_stampa).ToString("ddMMyyyy");
            newEcc.dataConvalida = er.data_convalida == null ? null : ((DateTime)er.data_convalida).ToString("ddMMyyyy");
            newEcc.descrittiva_lunga = er.descrizione_eccezione;
            newEcc.descrittivaConCodice = er.cod_eccezione + " - " + db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == er.cod_eccezione.Trim()).Select(x => x.desc_eccezione).FirstOrDefault();
            newEcc.importo = er.importo.ToString();
            newEcc.qta = er.quantita == 1 ? "1" : er.quantita.ToString();
            newEcc.ndoc = er.numero_documento.ToString();
            newEcc.IdStato = (int)EnumStatiRichiesta.Approvata;
            newEcc.stato_eccezione = er.id_stato == 10 ? "D" : " ";
            newEcc.EsisteStorno = true;
            newEcc.IdStatoStorno = (int)EnumStatiRichiesta.Approvata;

            List<Eccezione> L = dettaglioGiornata.eccezioni.ToList();
            L.Add(newEcc);

            dettaglioGiornata.eccezioni = L.ToArray();
            return dettaglioGiornata;
        }
        public static dayResponse InserisciInEccezioni(dayResponse dettaglioGiornata, MyRai_Eccezioni_Richieste er)
        {
            var db = new digiGappEntities();
            Eccezione newEcc = new Eccezione();
            newEcc.alle = er.alle == null ? null : ((DateTime)er.alle).ToString("HH:mm");
            newEcc.cod = er.cod_eccezione;
            newEcc.dalle = er.dalle == null ? null : ((DateTime)er.dalle).ToString("HH:mm");
            newEcc.data = er.data_eccezione.ToString("ddMMyyyy");
            newEcc.dataInserimento = er.data_creazione.ToString("ddMMyyyy");
            newEcc.DataRichiesta = er.data_creazione;
            newEcc.dataStampa = er.data_stampa == null ? null : ((DateTime)er.data_stampa).ToString("ddMMyyyy");
            newEcc.dataConvalida = er.data_convalida == null ? null : ((DateTime)er.data_convalida).ToString("ddMMyyyy");
            newEcc.descrittiva_lunga = er.descrizione_eccezione;
            newEcc.descrittivaConCodice = er.cod_eccezione + " - " + db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == er.cod_eccezione.Trim()).Select(x => x.desc_eccezione).FirstOrDefault();
            newEcc.importo = er.importo.ToString();
            newEcc.qta = er.quantita == 1 ? "1" : er.quantita.ToString();
            newEcc.ndoc = er.numero_documento.ToString();
            newEcc.IdStato = (int)EnumStatiRichiesta.Eliminata;
            newEcc.stato_eccezione = er.id_stato == 10 ? "D" : " ";

            List<Eccezione> L = dettaglioGiornata.eccezioni.ToList();
            L.Add(newEcc);

            dettaglioGiornata.eccezioni = L.ToArray();
            return dettaglioGiornata;

        }

        public static ModelDash GetListaProgrammi(ModelDash pr, string titolo = "", string luogo = "", Int32 data_da = 0, Int32 data_a = 0)
        {
            ModelDash model = new ModelDash();
            model.Programs = new ProgramsModel(titolo, luogo, data_da, data_a);
            return model;
        }

        public static ModelDash GetListaEventi(ModelDash pr, string titolo = "", string luogo = "", DateTime? data_da = null, DateTime? data_a = null)
        {
            ModelDash model = new ModelDash();
            model.Events = new EventsModel(titolo, luogo, data_da, data_a);
            return model;
        }

        public static ModelDash GetDaApprovareModel(ModelDash pr, Boolean RaggruppaGliStati = false, int da = 0, string sede = "", int stato = 0, string nominativo = "", 
            string eccezione = "", string data_da = "", string data_a = "", bool? visualizzati = null, string livelloDip = "", string soloUffProd = null, bool RichiedeVisti=false, string StatoVisti="")
        {
            ModelDash model = new ModelDash();
            string pMatricola = CommonHelper.GetCurrentUserPMatricola();
            string matricola = CommonHelper.GetCurrentUserMatricola();

            //Autorizzazioni.Sedi SEDI = new Autorizzazioni.Sedi();
            //pr.listaProfili = SEDI.Get_ProfiliAssociati_Net(CommonHelper.GetCurrentUserPMatricola(), "DGLIVELLI");
            //model.elencoProfilieSedi = new daApprovareModel(pr.listaProfili, true, "01");
            model.elencoProfilieSedi = new daApprovareModel(pMatricola, matricola, true, "01", RaggruppaGliStati, da, sede, stato, nominativo, eccezione, data_da, data_a, visualizzati, livelloDip, soloUffProd,
		RichiedeVisti, StatoVisti );

            if (model.elencoProfilieSedi.elencoSediEccezioni.Count > 0)
            {
                var db = new digiGappEntities();
                List<int> ListaIdMassive = db.MyRai_ApprovazioneMassiva.Where(x => x.Status < 5)
                    .Select(x => x.IdRichiesta).ToList();

                List<daApprovareModel.sedegappAbilitata> LS = new List<daApprovareModel.sedegappAbilitata>();

                foreach (var e in model.elencoProfilieSedi.elencoSediEccezioni)
                {
                    var list = e.eccezionidaValidare.Where(x => !ListaIdMassive.Contains(x.IdRichiestaPadre)).ToArray();
                    LS.Add(new daApprovareModel.sedegappAbilitata()
                    {
                        Accesso_firma = e.Accesso_firma,
                        Accesso_in_scrittura = e.Accesso_in_scrittura,
                        codFunzione = e.codFunzione,
                        Codice_sede_gapp = e.Codice_sede_gapp,
                        codProfilo = e.codProfilo,
                        codSottofunzione = e.codSottofunzione,
                        Descrittiva_sede_gapp = e.Descrittiva_sede_gapp,
                        RepartoCodice = e.RepartoCodice,
                        RepartoDescrizione = e.RepartoDescrizione,
                        eccezionidaValidare = list
                    });
                }
                model.elencoProfilieSedi.elencoSediEccezioni = LS;
            }
            

            if (model.elencoProfilieSedi.elencoSediEccezioni.Count > 0)
            {
                
                //  var listatemp = model.elencoProfilieSedi.elencoSediEccezioni[0].eccezionidaValidare.Where(x => x.IdRichiestaPadre == 25047).ToList();

                using (digiGappEntities db = new digiGappEntities())
                {
                    model.NomeRaggruppamento1 = db.MyRai_Raggruppamenti.Where(x => x.IdRaggruppamento == 1).Select(x => x.Descrizione).FirstOrDefault();
                    model.NomeRaggruppamento2 = db.MyRai_Raggruppamenti.Where(x => x.IdRaggruppamento == 2).Select(x => x.Descrizione).FirstOrDefault();

                }

                List<daApprovareModel.sedegappAbilitata> NewList = new List<daApprovareModel.sedegappAbilitata>();

                foreach (var sedegapp in model.elencoProfilieSedi.elencoSediEccezioni)
                {

                    foreach (var e in sedegapp.eccezionidaValidare)
                    {
                        if (String.IsNullOrWhiteSpace(e.CodiceReparto))
                            e.CodiceReparto = "00";
                    }
                    bool hoRep = (RepartiManager.SonoResponsabileDiReparti(sedegapp.Codice_sede_gapp));

                    List<string> mieiRep = RepartiManager.RepartiDiCuiSonoResponsabile(sedegapp.Codice_sede_gapp)
                        .Select(x => x.Substring(5)).ToList(); ;

                    bool Liv2 = Utente.IsBossLiv2();

                    var results = sedegapp.eccezionidaValidare
                        .Where(ecc =>
                        (hoRep && mieiRep.Contains(ecc.CodiceReparto))
                        || hoRep == false
                        || (Liv2 && ecc.RichiedenteL1))
                        .GroupBy(
                        p => p.CodiceReparto,
                        p => p,
                        (key, g) => new { codRep = key, eccez = g.ToList() });

                    foreach (var group in results)
                    {
                        daApprovareModel.sedegappAbilitata s = new daApprovareModel.sedegappAbilitata()
                        {
                            Accesso_firma = false,// sedegapp.Accesso_firma,
                            Accesso_in_scrittura = true,//sedegapp.Accesso_in_scrittura,
                            codFunzione = sedegapp.codFunzione,
                            Codice_sede_gapp = sedegapp.Codice_sede_gapp,
                            codProfilo = sedegapp.codProfilo,
                            codSottofunzione = sedegapp.codSottofunzione,
                            Descrittiva_sede_gapp = sedegapp.Descrittiva_sede_gapp,
                            eccezionidaValidare = group.eccez.ToArray(),
                            RepartoCodice = group.codRep
                        };
                        RepartoLinkedServer r = new LinkedTableDataController()
                            .GetDettagliReparto(sedegapp.Codice_sede_gapp, group.codRep, matricola);
                        if (r == null)
                            s.RepartoDescrizione = "REP." + group.codRep + " - DESCRIZIONE REPARTO NON TROVATA";
                        else
                            s.RepartoDescrizione = (r.Descr_Reparto != null ? r.Descr_Reparto.Trim() : r.Descr_Reparto);

                        NewList.Add(s);
                    }
                }
                model.elencoProfilieSedi.elencoSediEccezioni = NewList
                    .OrderBy(x => x.Codice_sede_gapp)
                    .ThenBy(x => x.RepartoCodice).ToList();
            }

            return model;
        }

        public static string ErroreInValidationResponse(validationResponse resp)
        {
            if (resp.esito == false) return resp.errore;
            string errore = null;
            foreach (var e in resp.eccezioni)
            {
                if (e.errore == null || e.errore.codice == null || e.errore.descrizione == null) continue;
                if (e.errore.codice != "0000" && e.errore.codice != "A100") errore += "Ecc " + e.ndoc + ":" + e.errore.descrizione.Trim() + "\r\n";
            }
            return errore;
        }

        public static List<MyRai_Raggruppamenti> GetRaggruppamenti ( )
        {
            var db = new digiGappEntities( );
            return db.MyRai_Raggruppamenti.OrderBy( x => x.IdRaggruppamento ).ToList( );
        }

        public static List<MyRai_SceltaPercorso> GetSceltepercorsoModel(string sezione = null)
        {
            var db = new digiGappEntities();
            return db.MyRai_SceltaPercorso.Where(x => x.Flag_Attivo == true && (sezione == null || x.Sezione == sezione)).OrderBy(x => x.Id).ToList();
        }

        public static PresenzaDipendenti GetPresenzaDipendenti()
        {
            var db = new digiGappEntities();
            //Autorizzazioni.Sedi servicesedi = new Autorizzazioni.Sedi();
            PresenzaDipendenti presenzadip = new PresenzaDipendenti();
            List<PresenzaDipendentiPerSede> listatotale = new List<PresenzaDipendentiPerSede>();
            List<Insediamento> insediamenti = db.L2D_INSEDIAMENTO
                .Select(x => new Insediamento
                {
                    CodiceInsediamento = x.cod_insediamento,
                    DescrizioneInsediamento = x.desc_insediamento,
                    Indirizzo = x.Indirizzo,
                    Citta = x.Sede
                }).ToList();

            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client service = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

            foreach (Sede sede in Utility.GetSediGappResponsabileList())
            {
                Boolean HoReparti = RepartiManager.SonoResponsabileDiReparti(sede.CodiceSede);
                List<string> MieiReparti =
                    RepartiManager.RepartiDiCuiSonoResponsabile(sede.CodiceSede).Select(x => x.Substring(5)).ToList();

                PresenzaDipendentiPerSede macroobj = new PresenzaDipendentiPerSede();
                List<DipendentePresenzaAssenza> listapersede = new List<DipendentePresenzaAssenza>();

                MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitypresenzeGiornaliere_resp resp =
                    service.presenzeGiornaliere(Utente.Matricola(), sede.CodiceSede, DateTime.Today.ToString("ddMMyyyy"));

                if (resp.dati.Count() == 0)
                {
                    continue;
                }

                int OrarioUltimaEntrata, OrarioUltimaUscita;
                foreach (var item in resp.dati)
                {
                    if (item.matricola == CommonManager.GetCurrentUserMatricola() ||
                        item.matricola == CommonManager.GetCurrentUserMatricola7chars()) continue;

                    DipendentePresenzaAssenza obj = new DipendentePresenzaAssenza();
                    obj.Nominativo = item.nominativo.Trim();
                    obj.Foto = CommonManager.GetUrlFoto(item.matricola.Substring(1));
                    obj.matricola = item.matricola;
                    obj.tipoDip = item.tipoDip;
                    obj.NotaDaSegreteria = GetNotaDaSegreteria(item.matricola, DateTime.Now);
                    bool isOk = int.TryParse(item.OrarioUltimaEntrata, out OrarioUltimaEntrata);
                    bool isOk2 = int.TryParse(item.OrarioUltimaUscita, out OrarioUltimaUscita);
                    if (!isOk)
                        OrarioUltimaEntrata = 0;
                    if (!isOk2)
                        OrarioUltimaUscita = 0;
                    obj.OrarioPrimaEntrata = EccezioniManager.CalcolaQuantitaOreMinuti("0", item.OrarioPrimaEntrata.ToString());

                    if (OrarioUltimaEntrata == 0 || OrarioUltimaUscita > OrarioUltimaEntrata)
                        obj.Presente = false;
                    else
                    {
                        obj.Presente = true;
                    }

                    if (item.CodTeminaleUltimaEntrata != "000" && item.CodTeminaleUltimaEntrata != "0000")
                    {
                        if (OrarioUltimaEntrata == 0)
                            obj.DescrizionePresenzaDipendente = "non presente";

                        if (OrarioUltimaUscita > OrarioUltimaEntrata)
                        {
                            if (item.tipoDip != "G" && item.tipoDip != "D")
                            {
                                obj.DescrizionePresenzaDipendente = "Uscita ore " + EccezioniManager.CalcolaQuantitaOreMinuti("0", OrarioUltimaUscita.ToString());
                            }
                            else
                            {
                                obj.DescrizionePresenzaDipendente = "Uscito";
                            }
                        }

                        if (OrarioUltimaEntrata > OrarioUltimaUscita)
                        {
                            obj.DescrizionePresenzaDipendente =
                            insediamenti.Where(x => x.CodiceInsediamento == item.CodTeminaleUltimaEntrata.Substring(1))
                            .Select(x => x.Indirizzo + (!String.IsNullOrWhiteSpace(x.Citta) ? " - " + x.Citta : "")).FirstOrDefault();
                            if (string.IsNullOrEmpty(obj.DescrizionePresenzaDipendente))
                            {
                                obj.DescrizionePresenzaDipendente =
                                insediamenti.Where(x => x.CodiceInsediamento == item.CodTeminaleUltimaEntrata.Substring(1))
                                .Select(x => x.DescrizioneInsediamento).FirstOrDefault();
                            }
                        }
                    }
                    else
                    {
                        obj.DescrizionePresenzaDipendente = CommonManager.GetDescrizioneEccezione(item.EccezioneUno);
                    }

                    obj.NumeroRichieste = db.MyRai_Richieste.Where(x =>
                        x.id_stato == (int)EnumStatiRichiesta.InApprovazione &&
                        x.matricola_richiesta == item.matricola.Substring(1)).Count().ToString();

                    //if (!obj.Presente)
                    //{
                    //    if (item.codiceOrario == "90" ||
                    //        item.codiceOrario == "95" ||
                    //        item.codiceOrario == "96")
                    //    {

                    obj.codiceOrario = item.codiceOrario;

                    using (var dbDigiGapp = new digiGappEntities())
                    {
                        var myItem = dbDigiGapp.L2D_ORARIO.Where(w => w.cod_orario.Equals(item.codiceOrario)).FirstOrDefault();

                        if (myItem != null)
                        {
                            //obj.DescrizionePresenzaDipendente += " " + myItem.desc_orario;
                            //obj.DescrizionePresenzaDipendente = obj.DescrizionePresenzaDipendente.Trim();
                            obj.DescrizioneCodiceOrario = myItem.desc_orario;
                        }
                    }
                    //    }
                    //}
                    obj.CodiceReparto = item.CodiceReparto;

                    if (HoReparti == false)
                        obj.PertinenzaApprovatore = true;
                    else
                    {
                        obj.PertinenzaApprovatore = MieiReparti.Contains(obj.CodiceReparto);
                    }
                    // obj.PertinenzaApprovatore = ( ! HoReparti || MieiReparti.Contains(obj.CodiceReparto));


                    listapersede.Add(obj);
                }
                //  digiGappEntities db = new digiGappEntities();
                //  db.MyRai_MensaXML.Where(c => c.Badge == CommonManager.GetCurrentUsername().PadLeft(8)).Where(c=>c.TransactionDateTime).Count() > 
                macroobj.SedeGapp = sede.CodiceSede;
                macroobj.DescrizioneSedeGap = sede.DescrizioneSede;
                macroobj.ListaDipendentiPerSede = listapersede;

                listatotale.Add(macroobj);
            }

            presenzadip.ListaDipendenti = listatotale;

            return presenzadip;
        }

        public static PresenzaDipendenti GetPresenzaDipendenti_ElencoSedi()
        {
            //Autorizzazioni.Sedi servicesedi = new Autorizzazioni.Sedi();
            PresenzaDipendenti presenzadip = new PresenzaDipendenti();
            List<PresenzaDipendentiPerSede> listatotale = new List<PresenzaDipendentiPerSede>();

            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client service = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

            foreach (Sede sede in Utility.GetSediGappResponsabileList())
            {
                PresenzaDipendentiPerSede macroobj = new PresenzaDipendentiPerSede();
                macroobj.SedeGapp = sede.CodiceSede;
                macroobj.DescrizioneSedeGap = sede.DescrizioneSede;
                //macroobj.ListaDipendentiPerSede = listapersede;

                listatotale.Add(macroobj);
            }

            presenzadip.ListaDipendenti = listatotale;

            return presenzadip;
        }
        public static PresenzaDipendentiPerSede GetPresenzaDipendenti_Sede(string codiceSede, string desSede)
        {
            var db = new digiGappEntities();
            //Autorizzazioni.Sedi servicesedi = new Autorizzazioni.Sedi();

            string tipi = CommonManager.GetParametro<String>(EnumParametriSistema.TipiDipQuadraturaSettimanale);
            string tipiGiorn = CommonManager.GetParametro<String>(EnumParametriSistema.TipiDipQuadraturaGiornaliera);

            List<Insediamento> insediamenti = db.L2D_INSEDIAMENTO
                .Select(x => new Insediamento
                {
                    CodiceInsediamento = x.cod_insediamento,
                    DescrizioneInsediamento = x.desc_insediamento,
                    Indirizzo = x.Indirizzo,
                    Citta = x.Sede
                }).ToList();

            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client service = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

            Boolean HoReparti = RepartiManager.SonoResponsabileDiReparti(codiceSede);
            List<string> MieiReparti =
                RepartiManager.RepartiDiCuiSonoResponsabile(codiceSede).Select(x => x.Substring(5)).ToList();

            PresenzaDipendentiPerSede macroobj = new PresenzaDipendentiPerSede();
            macroobj.SedeGapp = codiceSede;
            macroobj.DescrizioneSedeGap = desSede;

            List<DipendentePresenzaAssenza> listapersede = new List<DipendentePresenzaAssenza>();

            MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitypresenzeGiornaliere_resp resp;

            try
            {
                resp = service.presenzeGiornaliere(Utente.Matricola(), codiceSede, DateTime.Today.ToString("ddMMyyyy"));
            }
            catch (Exception ex)
            {
                return macroobj;
            }
            

            if (resp.dati.Count() == 0)
            {
                return macroobj;
            }

            int OrarioUltimaEntrata, OrarioUltimaUscita;
            foreach (var item in resp.dati)
            {
                if (item.matricola == CommonManager.GetCurrentUserMatricola() ||
                    item.matricola == CommonManager.GetCurrentUserMatricola7chars()) continue;

                DipendentePresenzaAssenza obj = new DipendentePresenzaAssenza();
                obj.Nominativo = item.nominativo.Trim();
                obj.Foto = CommonManager.GetUrlFoto(item.matricola.Substring(1));
                obj.matricola = item.matricola;
                obj.tipoDip = item.tipoDip;
                obj.NotaDaSegreteria = GetNotaDaSegreteria(item.matricola, DateTime.Now);
                bool isOk = int.TryParse(item.OrarioUltimaEntrata, out OrarioUltimaEntrata);
                bool isOk2 = int.TryParse(item.OrarioUltimaUscita, out OrarioUltimaUscita);
                if (!isOk)
                    OrarioUltimaEntrata = 0;
                if (!isOk2)
                    OrarioUltimaUscita = 0;
                obj.OrarioPrimaEntrata = EccezioniManager.CalcolaQuantitaOreMinuti("0", item.OrarioPrimaEntrata.ToString());

                if (OrarioUltimaEntrata == 0 || OrarioUltimaUscita > OrarioUltimaEntrata)
                    obj.Presente = false;
                else
                {
                    obj.Presente = true;
                }

                if (item.CodTeminaleUltimaEntrata != "000" && item.CodTeminaleUltimaEntrata != "0000")
                {
                    if (OrarioUltimaEntrata == 0)
                        obj.DescrizionePresenzaDipendente = "non presente";

                    if (OrarioUltimaUscita > OrarioUltimaEntrata)
                    {
                        if (item.tipoDip != "G" && item.tipoDip != "D")
                        {
                            obj.DescrizionePresenzaDipendente = "Uscita ore " + EccezioniManager.CalcolaQuantitaOreMinuti("0", OrarioUltimaUscita.ToString());
                        }
                        else
                        {
                            obj.DescrizionePresenzaDipendente = "Uscito";
                        }
                    }

                    if (OrarioUltimaEntrata > OrarioUltimaUscita)
                    {
                        obj.DescrizionePresenzaDipendente =
                        insediamenti.Where(x => x.CodiceInsediamento == item.CodTeminaleUltimaEntrata.Substring(1))
                        .Select(x => x.Indirizzo + (!String.IsNullOrWhiteSpace(x.Citta) ? " - " + x.Citta : "")).FirstOrDefault();
                        if (string.IsNullOrEmpty(obj.DescrizionePresenzaDipendente))
                        {
                            obj.DescrizionePresenzaDipendente =
                            insediamenti.Where(x => x.CodiceInsediamento == item.CodTeminaleUltimaEntrata.Substring(1))
                            .Select(x => x.DescrizioneInsediamento).FirstOrDefault();
                        }
                    }
                }
                else
                {
                    obj.DescrizionePresenzaDipendente = CommonManager.GetDescrizioneEccezione(item.EccezioneUno);
                }

                obj.NumeroRichieste = db.MyRai_Richieste.Where(x =>
                    x.id_stato == (int)EnumStatiRichiesta.InApprovazione &&
                    x.matricola_richiesta == item.matricola.Substring(1)).Count().ToString();

                //if (!obj.Presente)
                //{
                //    if (item.codiceOrario == "90" ||
                //        item.codiceOrario == "95" ||
                //        item.codiceOrario == "96")
                //    {

                obj.codiceOrario = item.codiceOrario;

                using (var dbDigiGapp = new digiGappEntities())
                {
                    var myItem = dbDigiGapp.L2D_ORARIO.Where(w => w.cod_orario.Equals(item.codiceOrario)).FirstOrDefault();

                    if (myItem != null)
                    {
                        //obj.DescrizionePresenzaDipendente += " " + myItem.desc_orario;
                        //obj.DescrizionePresenzaDipendente = obj.DescrizionePresenzaDipendente.Trim();
                        obj.DescrizioneCodiceOrario = myItem.desc_orario;
                    }
                }
                //    }
                //}
                obj.CodiceReparto = item.CodiceReparto;

                if (HoReparti == false)
                    obj.PertinenzaApprovatore = true;
                else
                {
                    obj.PertinenzaApprovatore = MieiReparti.Contains(obj.CodiceReparto);
                }
                // obj.PertinenzaApprovatore = ( ! HoReparti || MieiReparti.Contains(obj.CodiceReparto));


                var tipoDipendente = item.tipoDip;
                if ( tipi != null && tipoDipendente != null && tipi.ToUpper( ).Contains( tipoDipendente.ToUpper( ) ) )
                    obj.Quadratura = Quadratura.Settimanale;


                if ( tipiGiorn != null && tipoDipendente != null && tipiGiorn.ToUpper( ).Contains( tipoDipendente.ToUpper( ) ) )
                    obj.Quadratura = Quadratura.Giornaliera;


                if ( obj.Quadratura == Quadratura.Giornaliera )
                {
                    UtenteTerzo u = new UtenteTerzo( );
                    var anecc = u.GetAnalisiEcc( CommonHelper.GetCurrentUserMatricola( ) , item.matricola );
                    int pohMinuti = Convert.ToInt32( anecc.AnalisiEccezione[0].totale );
                    int rohMinuti = Convert.ToInt32( anecc.AnalisiEccezione[1].totale );
                    int diff = pohMinuti - rohMinuti;
                    obj.BilancioPOH = EccezioniManager.CalcolaStringaOreMinuti(diff);
                    if (diff > 0) obj.BilancioPOH = "- " + obj.BilancioPOH;
                    if (diff < 0) obj.BilancioPOH = "+ " + obj.BilancioPOH;

                    var tmp = anecc.DettagliEccezioni.Where( x => x.eccezione == "POH" );
                    if ( tmp != null && tmp.Count( ) > 0 )
                        obj.POHMeseCorrente = tmp.Count( x => x.data.Year == DateTime.Today.Year && x.data.Month == DateTime.Today.Month );

                }
                else if ( obj.Quadratura == Quadratura.Settimanale )
                {

                }

                listapersede.Add(obj);
            }
            //  digiGappEntities db = new digiGappEntities();
            //  db.MyRai_MensaXML.Where(c => c.Badge == CommonManager.GetCurrentUsername().PadLeft(8)).Where(c=>c.TransactionDateTime).Count() > 

            macroobj.ListaDipendentiPerSede = listapersede;

            return macroobj;
        }

        public static string GetEccezioniRichiedentiCeiton()
        {
            var db = new digiGappEntities();
            var list = db.MyRai_Eccezioni_Ammesse.Where(x => x.RichiedeAttivitaCeiton == true).Select(x => x.cod_eccezione).ToList();
            return String.Join(",", list.ToArray());
        }

        static string GetDescrizioneEntrataDipendente(int UltimaEntrata, int UltimaUscita, string CodiceTerminale)
        {
            string descrizionePresenza = "non presente";
            if (UltimaEntrata == 0)
                return descrizionePresenza;

            if (UltimaUscita > UltimaEntrata)
                return descrizionePresenza = "Uscita ore " + UltimaUscita.ToString();

            if (UltimaEntrata > UltimaUscita)
            {
            }

            return descrizionePresenza;
        }

        /// <summary>
        /// Reperimento degli stati delle richieste per l'utente passato
        /// </summary>
        /// <param name="matricola">matricola dell'utente per il quale si intende ottenere la lista degli stati richiesta</param>
        /// <returns></returns>
        public static System.Web.Mvc.SelectList GetListaStati(string matricola)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            var db = new digiGappEntities();

            var query = from stati in db.MyRai_Stati
                        join listRichieste in db.MyRai_Richieste
                            on stati.id_stato equals listRichieste.id_stato
                        where listRichieste.matricola_richiesta == matricola
                        orderby stati.id_stato
                        select new { stato = stati };

            if (query != null)
            {
                query.ToList().ForEach(q =>
                {
                    if (!list.Any(x => x.Value.Equals(q.stato.id_stato.ToString())))
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = q.stato.id_stato.ToString(),
                            Text = q.stato.descrizione_stato,
                            Selected = false
                        });
                    }
                });
            }

            return new SelectList(list, "Value", "Text");
        }

        public static System.Web.Mvc.SelectList GetListaStati()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            var db = new digiGappEntities();
            foreach (var stato in db.MyRai_Stati.OrderBy(x => x.id_stato))
            {
                list.Add(new SelectListItem()
                {
                    Value = stato.id_stato.ToString(),
                    Text = stato.descrizione_stato,
                    Selected = false
                });
            }

            return new SelectList(list, "Value", "Text");
        }

        /// <summary>
        /// Reperimento dell'elenco delle eccezioni richieste dall'utente con matricola passata
        /// </summary>
        /// <param name="matricola">matricola dell'utente per il quale reperire l'elenco delle eccezioni</param>
        /// <returns></returns>
        public static System.Web.Mvc.SelectList GetListaEccezioni(string matricola)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            var db = new digiGappEntities();

            // reperimento delle eccezioni richieste effettuate dall'utente con matricola "matricola"
            var query = from listRichieste in db.MyRai_Richieste
                        join listEccRic in db.MyRai_Eccezioni_Richieste
                            on listRichieste.id_richiesta equals listEccRic.id_richiesta
                        join ammesse in db.MyRai_Eccezioni_Ammesse
                            on listEccRic.cod_eccezione equals ammesse.cod_eccezione
                        where listRichieste.matricola_richiesta == matricola
                        orderby listEccRic.cod_eccezione
                        select new { listEccRic = listEccRic, ammessa = ammesse };

            if (query != null)
            {
                query.ToList().ForEach(q =>
                {
                    if (!list.Any(x => x.Value.Equals(q.listEccRic.cod_eccezione.Trim())))
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = q.listEccRic.cod_eccezione.Trim(),
                            Text = q.listEccRic.cod_eccezione + "-" + q.ammessa.desc_eccezione,
                            Selected = false
                        });
                    }
                });
            }

            return new SelectList(list, "Value", "Text");
        }

        public static System.Web.Mvc.SelectList GetListaEccezioni()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var db = new digiGappEntities();

            foreach (var stato in db.MyRai_Eccezioni_Ammesse.OrderBy(x => x.cod_eccezione))
            {
                list.Add(new SelectListItem()
                {
                    Value = stato.cod_eccezione.Trim(),
                    Text = stato.cod_eccezione + "-" + stato.desc_eccezione,
                    Selected = false
                });
            }

            return new SelectList(list, "Value", "Text");
        }

        public static ModelDash MarcaLivelloPerRichiedenti(ModelDash model)
        {
            //if (model != null && model.elencoProfilieSedi != null && model.elencoProfilieSedi.elencoSediEccezioni != null)
            //{
            //	foreach (var s in model.elencoProfilieSedi.elencoSediEccezioni)
            //	{
            //		if (s.eccezionidaValidare != null && s.eccezionidaValidare.Count() > 0)
            //		{
            //			MatricoleSede ma = new MatricoleSede();
            //			if (s.Codice_sede_gapp != null)
            //			{
            //				ma.MatricoleL1 = GetMatricolaLivelloPerSede(s.Codice_sede_gapp.Trim().ToUpper(), 1);
            //				ma.MatricoleL2 = GetMatricolaLivelloPerSede(s.Codice_sede_gapp.Trim().ToUpper(), 2);
            //			}
            //			foreach (Eccezione ecc in s.eccezionidaValidare)
            //			{
            //				ecc.LivelloRichiedenteEccezione = 0;
            //				if (ma.MatricoleL1 != null && ma.MatricoleL1.Contains("P" + ecc.matricola))
            //					ecc.LivelloRichiedenteEccezione = 1;
            //				if (ma.MatricoleL2 != null && ma.MatricoleL2.Contains("P" + ecc.matricola))
            //					ecc.LivelloRichiedenteEccezione = 2;
            //			}
            //		}
            //	}
            //}
            return model;
        }

        public static List<String> GetMatricolaLivelloPerSede(string sedegapp, int livelloResponsabile_1_2)
        {
            Sedi service = new Sedi();
            service.Credentials = CommonHelper.GetUtenteServizioCredentials();

            CategorieDatoAbilitate response =
                CommonHelper.Get_CategoriaDato_Net_Cached(livelloResponsabile_1_2);
            //service.Get_CategoriaDato_Net("sedegapp", "", "HRUP", "0" + livelloResponsabile_1_2.ToString() + "GEST");

            var item = response.CategorieDatoAbilitate_Array
                .Where(sede => sede.Codice_categoria_dato.Trim().ToUpper() == sedegapp.Trim().ToUpper())
                       .FirstOrDefault();
            if (item == null) return null;

            List<string> Matricole = item.DT_Utenti_CategorieDatoAbilitate
                                         .AsEnumerable()
                                         .Select(p => (String)p.ItemArray[0]).ToList();
            return Matricole;
        }

        /// <summary>
        /// Reperimento dell'eventuale nota inserita dalla segreteria per l'utente nella data passata
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetNotaDaSegreteria(string matricola, DateTime data)
        {
            string result = string.Empty;

            try
            {
                DateTime start = new DateTime(data.Year, data.Month, data.Day, 0, 0, 0);
                DateTime stop = new DateTime(data.Year, data.Month, data.Day, 23, 59, 59);

                using (var db = new digiGappEntities())
                {
                    var item = from ecc in db.MyRai_Note_Da_Segreteria
                               where ecc.Matricola.Equals(matricola) &&
                                    (ecc.Data >= start && ecc.Data <= stop)
                               select ecc;

                    if (item != null && item.Any())
                        result = item.FirstOrDefault().Nota;
                }
            }
            catch (Exception ex)
            {
                result = string.Empty;
            }

            return result;
        }

        public static ModelDash GetDaApprovareProduzioneModel(ModelDash pr, Boolean RaggruppaGliStati = false, int da = 0, string sede = "", int stato = 0, string nominativo = "", string eccezione = "", string data_da = "", string data_a = "", bool? visualizzati = null, string livelloDip = "", string titolo = "")
        {
            ModelDash model = new ModelDash();
            string pMatricola = CommonHelper.GetCurrentUserPMatricola();
            string matricola = CommonHelper.GetCurrentUserMatricola();
            model.elencoProfilieSedi = new daApprovareModel(true, RaggruppaGliStati, da, sede, stato, nominativo, eccezione, data_da, data_a, visualizzati, livelloDip, titolo);
            if (model.elencoProfilieSedi.elencoSediEccezioni.Count > 0)
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    model.NomeRaggruppamento1 = db.MyRai_Raggruppamenti.Where(x => x.IdRaggruppamento == 1).Select(x => x.Descrizione).FirstOrDefault();
                    model.NomeRaggruppamento2 = db.MyRai_Raggruppamenti.Where(x => x.IdRaggruppamento == 2).Select(x => x.Descrizione).FirstOrDefault();
                }

                List<daApprovareModel.sedegappAbilitata> NewList = new List<daApprovareModel.sedegappAbilitata>();

                foreach (var sedegapp in model.elencoProfilieSedi.elencoSediEccezioni)
                {
                    foreach (var e in sedegapp.eccezionidaValidare)
                    {
                        if (String.IsNullOrWhiteSpace(e.CodiceReparto))
                            e.CodiceReparto = "00";
                    }
                    bool hoRep = (RepartiManager.SonoResponsabileDiReparti(sedegapp.Codice_sede_gapp));

                    List<string> mieiRep = RepartiManager.RepartiDiCuiSonoResponsabile(sedegapp.Codice_sede_gapp)
                        .Select(x => x.Substring(5)).ToList();
                    ;

                    var results = sedegapp.eccezionidaValidare
                        .Where(ecc => (hoRep && mieiRep.Contains(ecc.CodiceReparto)) || hoRep == false)
                        .GroupBy(
                        p => p.CodiceReparto,
                        p => p,
                        (key, g) => new { codRep = key, eccez = g.ToList() });

                    foreach (var group in results)
                    {
                        daApprovareModel.sedegappAbilitata s = new daApprovareModel.sedegappAbilitata()
                        {
                            Accesso_firma = false,// sedegapp.Accesso_firma,
                            Accesso_in_scrittura = true,//sedegapp.Accesso_in_scrittura,
                            codFunzione = sedegapp.codFunzione,
                            Codice_sede_gapp = sedegapp.Codice_sede_gapp,
                            codProfilo = sedegapp.codProfilo,
                            codSottofunzione = sedegapp.codSottofunzione,
                            Descrittiva_sede_gapp = sedegapp.Descrittiva_sede_gapp,
                            eccezionidaValidare = group.eccez.ToArray(),
                            RepartoCodice = group.codRep
                        };
                        RepartoLinkedServer r = new LinkedTableDataController().GetDettagliReparto(sedegapp.Codice_sede_gapp, group.codRep, matricola);
                        if (r == null)
                            s.RepartoDescrizione = "REP." + group.codRep + " - DESCRIZIONE REPARTO NON TROVATA";
                        else
                            s.RepartoDescrizione = (r.Descr_Reparto != null ? r.Descr_Reparto.Trim() : r.Descr_Reparto);

                        NewList.Add(s);
                    }
                }
                model.elencoProfilieSedi.elencoSediEccezioni = NewList
                    .OrderBy(x => x.Codice_sede_gapp)
                    .ThenBy(x => x.RepartoCodice).ToList();
            }

            return model;
        }

    }

    public class MatricoleSede
    {
        public MatricoleSede()
        {
            MatricoleL1 = new List<string>();
            MatricoleL2 = new List<string>();
        }
        public string sede { get; set; }
        public List<string> MatricoleL1 { get; set; }
        public List<string> MatricoleL2 { get; set; }
    }
}
