using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using myRaiCommonDatacontrollers.DataControllers;
using myRaiData;
using myRaiService.Helpers;
using System.Web.Hosting;
using myRaiService.classi;
using myRaiService.Filters;

namespace myRaiService
{
    [ServiceLoggingBehavior()]
    //[UserLogging]
    public class MyRaiService1 : IMyRaiService1
    {
        public enum CachedMethods
        {
            recuperaUtente, getOrario
        }


        public CodiceFiscaleReponse GetCodiceFiscaleInfo(string cf, string matricolaOperatore)
        {
            CodiceFiscaleReponse Response = new CodiceFiscaleReponse();
            if (!String.IsNullOrWhiteSpace(cf) && !CodiceFiscaleUtility.ControlloFormaleOK(cf))
            {
                Response.esito = false;
                Response.error = "Controllo formale errato";
            }
            string S = areaSistema("3CF", matricolaOperatore, 75);
            S += matricolaOperatore.PadLeft(7, '0') + cf;
            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();

            string RispostaCics =
                // @"ACK0000004903CFGAPWEB00001036500405202100490NOK0000ELABORAZIONE REGOLARE    75      14:12:440103650                                                 00000000 BIFANO MICHELE           230120112301201123012023010365001 000000000 000000000000000000000000000000000 00000000000000000000000000000000000000000000000000000000000000000000000BFNMHL11A23H501S                     00000000 BIFANO GIUSEPPE          040620130406201304062025010365002 000000000 000000000000000000000000000000000 00000000000000000000000000000000000000000000000000000000000000000000000BFNGPP13H04H501U";
                (string)c.ComunicaVersoCics(S.PadRight(8000));
            wApiUtility.cicsResponse cicsResp = wApiUtility.checkResponse(RispostaCics);
            //BFNMHL11A23H501S 
            Response.sent = S;
            Response.raw = RispostaCics;

            if (cicsResp.esito == false)
            {
                Response.esito = false;
                Response.error = cicsResp.codErrore + "-" + cicsResp.descErrore;
                return Response;
            }

            Response.esito = true;

            int start = 142;

            Response.CFinfo = new List<CodiceFiscaleInfo>();
            while (start + 199 < RispostaCics.Length)
            {
                CodiceFiscaleInfo C = new CodiceFiscaleInfo();

                C.CodiceUtente = RispostaCics.Substring(start, 6);
                C.DataIntervento = RispostaCics.Substring(start + 6, 8);
                C.CodiceIntervento = RispostaCics.Substring(start + 14, 1);
                C.NominativoFiglio = RispostaCics.Substring(start + 15, 25);
                C.DataNascita = RispostaCics.Substring(start + 40, 8);
                C.DataInizioOSS = RispostaCics.Substring(start + 48, 8);
                C.DataFineOSS = RispostaCics.Substring(start + 56, 8);
                C.MatricolaGenitore1 = RispostaCics.Substring(start + 64, 7);
                C.ProgressivoCarichiGenitore1 = RispostaCics.Substring(start + 71, 2);
                C.SessoGenitore1 = RispostaCics.Substring(start + 73, 1);
                C.MatricolaGenitore2 = RispostaCics.Substring(start + 74, 7);
                C.ProgressivoCarichiGenitore2 = RispostaCics.Substring(start + 81, 2);
                C.SessoGenitore2 = RispostaCics.Substring(start + 83, 1);

                C.LimiteAF = RispostaCics.Substring(start + 84, 6);
                C.NumeroGiorniExtraAF = RispostaCics.Substring(start + 90, 3);

                C.LimiteAF_BF_CF = RispostaCics.Substring(start + 93, 6);
                C.NumeroGiorniExtraAF_BF_CF = RispostaCics.Substring(start + 99, 3);
                C.LimiteAF_HF = RispostaCics.Substring(start + 102, 6);
                C.NumeroGiorniExtraHF = RispostaCics.Substring(start + 108, 3);

                C.LimiteMU = RispostaCics.Substring(start + 111, 6);
                C.Switch3MesiAF = RispostaCics.Substring(start + 117, 1);
                C.VoucherGenitore1 = RispostaCics.Substring(start + 118, 2);
                C.VoucherGenitore2 = RispostaCics.Substring(start + 120, 2);

                C.GiorniFruitiAF_genitore1 = RispostaCics.Substring(start + 122, 6);
                C.GiorniFruitiBF_genitore1 = RispostaCics.Substring(start + 128, 6);
                C.GiorniFruitiCF_genitore1 = RispostaCics.Substring(start + 134, 6);
                C.GiorniFruitiHF_genitore1 = RispostaCics.Substring(start + 140, 6);
                C.GiorniFruitiMU_genitore1 = RispostaCics.Substring(start + 146, 6);

                C.GiorniFruitiAF_genitore2 = RispostaCics.Substring(start + 152, 6);
                C.GiorniFruitiBF_genitore2 = RispostaCics.Substring(start + 158, 6);
                C.GiorniFruitiCF_genitore2 = RispostaCics.Substring(start + 164, 6);
                C.GiorniFruitiHF_genitore2 = RispostaCics.Substring(start + 170, 6);
                C.GiorniFruitiMU_genitore2 = RispostaCics.Substring(start + 176, 6);

                C.CodiceProgressivo = RispostaCics.Substring(start + 182, 7);
                C.CFfiglioACarico = RispostaCics.Substring(start + 189, 16);
                start += 220;
                Response.CFinfo.Add(C);
            }

            return Response;
        }
        public SetPagamentoEccezioneResponse SetPagamentoEccezione(string matricolaOperatore, DateTime dataEccezione, string matricola, DateTime dataPagamento, string eccezione, string numdoc)
        {
            if (numdoc != null && numdoc.Length > 6)
            {
                return new SetPagamentoEccezioneResponse() { esito = false, error = "Valore errato per numdoc" };
            }
            if (string.IsNullOrWhiteSpace(eccezione))
            {
                return new SetPagamentoEccezioneResponse() { esito = false, error = "Valore errato per eccezione" };
            }
            if (String.IsNullOrWhiteSpace(numdoc))
            {
                it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();

                service.Credentials = new System.Net.NetworkCredential(
                       Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                       Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
                       );
                var resp = service.getEccezioni(matricola, dataEccezione.ToString("ddMMyyyy"), "BU", 80);
                var ecc = resp.eccezioni.Where(x => x.cod.Trim() == eccezione.Trim()).FirstOrDefault();
                if (ecc == null)
                {
                    return new SetPagamentoEccezioneResponse() { esito = false, error = "Eccezione non trovata" };
                }
                numdoc = ecc.ndoc;
            }
            string S = areaSistema("3PE", matricolaOperatore, 75);
            S = S + matricola.PadLeft(7, '0')
                + eccezione.PadRight(4, ' ')
                + dataEccezione.ToString("ddMMyyyy")
                + numdoc.PadLeft(6, '0')
                + dataPagamento.ToString("ddMMyyyy")
                + "".PadLeft(17, ' ');

            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
            string RispostaCics = (string)c.ComunicaVersoCics(S.PadRight(8000));
            wApiUtility.cicsResponse cicsResp = wApiUtility.checkResponse(RispostaCics);

            SetPagamentoEccezioneResponse Response = new SetPagamentoEccezioneResponse()
            {
                esito = true,
                raw = RispostaCics,
                sent = S
            };

            if (cicsResp.esito == false)
            {
                Response.esito = false;
                Response.error = cicsResp.codErrore + "-" + cicsResp.descErrore;
                return Response;
            }
            else
            {
                return Response;
            }
        }
        public SetStatoEccezioneResponse SetStatoEccezione(string matricolaOperatore, DateTime data, string matricola, string numdoc, string eccezione, string stato)
        {
            if (numdoc != null && numdoc.Length > 6)
            {
                return new SetStatoEccezioneResponse() { esito = false, error = "Valore errato per numdoc" };
            }
            if (string.IsNullOrWhiteSpace(eccezione))
            {
                return new SetStatoEccezioneResponse() { esito = false, error = "Valore errato per eccezione" };
            }
            if (stato == null || stato.Length > 2)
            {
                return new SetStatoEccezioneResponse() { esito = false, error = "Valore errato per stato" };
            }
            if (String.IsNullOrWhiteSpace(numdoc))
            {
                it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();

                service.Credentials = new System.Net.NetworkCredential(
                       Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                       Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
                       );
                var resp = service.getEccezioni(matricola, data.ToString("ddMMyyyy"), "BU", 80);
                var ecc = resp.eccezioni.Where(x => x.cod.Trim() == eccezione.Trim()).FirstOrDefault();
                if (ecc == null)
                {
                    return new SetStatoEccezioneResponse() { esito = false, error = "Eccezione non trovata" };
                }
                numdoc = ecc.ndoc;
            }
            string S = areaSistema("3CC", matricolaOperatore, 75);
            S = S + matricola.PadLeft(7, '0')
                + data.ToString("ddMMyyyy")
                + eccezione.PadRight(4, ' ')
                + numdoc.PadLeft(6, '0')
                + stato
                + "".PadLeft(32, ' ');

            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
            string RispostaCics = (string)c.ComunicaVersoCics(S.PadRight(8000));
            wApiUtility.cicsResponse cicsResp = wApiUtility.checkResponse(RispostaCics);

            SetStatoEccezioneResponse Response = new SetStatoEccezioneResponse()
            {
                esito = true,
                raw = RispostaCics,
                sent = S
            };

            if (cicsResp.esito == false)
            {
                Response.esito = false;
                Response.error = cicsResp.codErrore + "-" + cicsResp.descErrore;
                return Response;
            }
            else
            {
                if (RispostaCics.Contains("ELABORAZIONE REGOLARE"))
                    return Response;
                else
                {
                    Response.esito = false;
                    Response.error = "NON CONFERMATA";
                    return Response;
                }
            }

        }
        public static string Serialize(myRaiService.it.rai.servizi.svildigigappws.Eccezione[] v)
        {
            var s = new System.Web.Script.Serialization.JavaScriptSerializer();
            s.MaxJsonLength = int.MaxValue;
            var json = s.Serialize(v);
            return json;
        }

        public string[] GetEccezioniRicalcolate()
        {
            var db = new digiGappEntities();
            string p = GetParametro<string>("ProposteAutoRicalcoloQuantita");
            if ( String.IsNullOrWhiteSpace( p ) )
                return null;

            return p.Split(',');
        }


        public SetRuoloResponse SetRuolo(string matricola, string dataMese, string sede)
        {
            try
            {
                it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();

                service.Credentials = new System.Net.NetworkCredential(
                       Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                       Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
                       );
                dataMese = dataMese.Replace("/", "");
                DateTime D;
                if (!DateTime.TryParseExact(dataMese, "ddMMyyyy", null, DateTimeStyles.None, out D))
                {
                    return new SetRuoloResponse() { esito = false, error = "Data non valida" };
                }

                ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();

                string S = areaSistema("3RU", matricola, 75);

                S = S + sede + dataMese + matricola;
                string RispostaCics = (string)c.ComunicaVersoCics(S.PadRight(8000));
                //ACK0000001003RUGAPWEB00001036500412201900100 OK0000aaaaaaaaaaaaaaaaaaaaaaaaa75      11:15:07DDE2030112019103650                               OKPASSAGGIO A RUOLO EFFETTUATO

                wApiUtility.cicsResponse cicsResp = wApiUtility.checkResponse(RispostaCics);

                if (cicsResp.esito == false)
                {
                    var re= new  SetRuoloResponse() { esito = false, raw = RispostaCics, error = cicsResp.codErrore + "-" + cicsResp.descErrore };
                    return re;
                }

                DateTime DataInizio = new DateTime(D.Year, D.Month, 1);
                DateTime DataFine = DataInizio.AddMonths(1).AddDays(-1);

                it.rai.servizi.svildigigappws.Periodo[] respScadenzario = service.getScadenzarioSingolaSede(matricola, DataInizio.ToString("ddMMyyyy"), DataFine.ToString("ddMMyyyy"), "80", sede, 80);
                return new SetRuoloResponse()
                {
                    esito = true,
                    raw = RispostaCics,
                    RispostaScadenzario = respScadenzario
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public SbloccaEccezioniResponse SbloccaEccezione(string MatricolaSegreteria, string MatricolaDipendente, DateTime Data, string Eccezione, string Motivo)
        {
            if (String.IsNullOrWhiteSpace(MatricolaSegreteria) || String.IsNullOrWhiteSpace(MatricolaDipendente) || String.IsNullOrWhiteSpace(Eccezione))
            {
                return new SbloccaEccezioniResponse() { Success = false, Error = "Dati incompleti" };
            }

            var db = new digiGappEntities();
            Data = Data.Date;
            if (db.MyRai_Eccezioni_Sbloccate.Any(x => x.data == Data && x.matricola == MatricolaDipendente && x.eccezione == Eccezione))
            {
                return new SbloccaEccezioniResponse() { Success = false, Error = "Gia presente" };
            }
            string[] eccRicalcolate = GetEccezioniRicalcolate();
            if (!eccRicalcolate.Contains(Eccezione))
            {
                return new SbloccaEccezioniResponse() { Success = false, Error = Eccezione + " non è una eccezione ricalcolata" };
            }

            try
            {
                MyRai_Eccezioni_Sbloccate es = new MyRai_Eccezioni_Sbloccate()
                {
                    data = Data,
                    data_richiesta = DateTime.Now,
                    eccezione = Eccezione.ToUpper(),
                    richiesto_da = MatricolaSegreteria,
                    matricola = MatricolaDipendente,
                    motivo = Motivo
                };
                db.MyRai_Eccezioni_Sbloccate.Add(es);
                db.SaveChanges();
                return new SbloccaEccezioniResponse() { Success = true };
            }
            catch (Exception ex)
            {
                return new SbloccaEccezioniResponse() { Success = false, Error = ex.Message };
            }
        }

        public GetPianoFeriePDFResponse GetPianoFeriePDF(string sede, int anno)
        {
            GetPianoFeriePDFResponse resp = new GetPianoFeriePDFResponse();
            var db = new digiGappEntities();
            try
            {
                var row = db.MyRai_PianoFerieSedi.Where(x => x.sedegapp == sede && x.anno == anno).OrderByDescending(a => a.numero_versione).FirstOrDefault();
                if ( row == null )
                    return resp;

                resp.pdf = row.pdf;
                resp.DataApprovazione = row.data_approvata;
                resp.MatricolaApprovazione = row.matricola_approvatore;
                resp.DataFirma = row.data_firma;
                resp.MatricolaFirma = row.matricola_firma;
                resp.DataStornoApprovazione = row.data_storno_approvazione;
                resp.MatricolaStorno = row.matricola_storno;
                return resp;
            }
            catch (Exception ex)
            {
                resp.error = ex.Message;
                return resp;
            }
        }

        public GetPianoFerieAnnoResponse GetPianoFerieAnno(string sede, int anno , bool soloStato = false )
        {
            GetPianoFerieAnnoResponse Response = new GetPianoFerieAnnoResponse();

            var db = new digiGappEntities();
            string sede5 = sede.Substring(0, 5).ToUpper();

            var giaInseriti = db.MyRai_PianoFerieBatch.Where(x => x.sedegapp.StartsWith( sede5 ) && x.data_inserimento_gapp != null).ToList();

            List<string> sedi = db.MyRai_PianoFerie.Where(x => x.sedegapp.StartsWith( sede5 ) && x.anno == anno && x.data_consolidato != null)
                                                        .Select(x => x.sedegapp).Distinct().OrderBy(x => x).ToList();

            it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();

            service.Credentials = new System.Net.NetworkCredential(
                   Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                   Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
                   );

            foreach (string s in sedi)
            {
                SedePianoFerie spf = new SedePianoFerie()
                {
                    Sede = s,
                    StatoPianoFerie = db.MyRai_PianoFerieSedi.Where(x => x.anno == anno && x.sedegapp == s).ToList()
                };

                if (spf.StatoPianoFerie.Any())
                {
                    foreach ( var item in spf.StatoPianoFerie )
                        item.pdf = null;
                }

                if ( !soloStato )
                {
                    var matricole = db.MyRai_PianoFerie.Where(x => x.sedegapp == s && x.anno == anno).OrderBy(x => x.matricola).ToList();

                    foreach (var matr in matricole)
                    {
                        Dip D = new Dip()
                        {
                            Matricola = matr.matricola,
                            Giorni = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matr.matricola && x.data.Year == anno).ToList()
                        };
                        D.Accordi = GetAccordi(matr.matricola, service, anno);
                        D.Giorni.RemoveAll(x => giaInseriti.Any(z => z.data_eccezione == x.data && z.matricola == x.matricola && z.codice_eccezione == x.eccezione));

                        spf.Dipendenti.Add(D);
                    }
                }

                Response.ListaSedi.Add(spf);
            }

            // se soloStato allora verrà restituita una risposta con meno info
            if ( soloStato )
            {
                return Response;
            }

            var tip = db.MyRai_InfoDipendente_Tipologia.Where(x => x.info == "Esente Piano Ferie").FirstOrDefault();
            if (tip != null)
            {
                DateTime D = DateTime.Now;
                int idTip = tip.id;
                Response.MatricoleEsentate = db.MyRai_InfoDipendente.Where(x =>
                                       x.id_infodipendente_tipologia == idTip &&
                                       x.valore != null &&
                                       x.valore.ToLower() == "true" &&
                                       x.data_inizio <= D
                                       && (x.data_fine >= D || x.data_fine == null)).Select(x => x.matricola).OrderBy(x => x).ToArray();
            }

            //it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();
            //service.Credentials = new System.Net.NetworkCredential(
            //      Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
            //      Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
            //      );

            Response.Definizioni = db.MyRai_PianoFerieDefinizioni.Where(x => x.anno == anno).FirstOrDefault();
            return Response;
        }

        public static float GetFECE_MNCE_MRCE(string matricola 
           )
        {
            MyRaiService1 m = new MyRaiService1();
            GetContatoriEccezioniResponse cont =m. GetContatoriEccezioni(matricola,
                                                           new DateTime(2020, 1, 1),
                                                           new DateTime(2020, 12, 31),
                                                           new string[] { "FECE", "MNCE", "MRCE" }.ToList());

            if (cont == null || cont.ContatoriEccezioni == null || cont.ContatoriEccezioni.Count == 0)
                return 0;

            float totaleDonati = 0;
            for (int i = 0; i <= 2; i++)
            {
                if (cont.ContatoriEccezioni.Count > i && cont.ContatoriEccezioni[i] != null)
                {
                    string t = cont.ContatoriEccezioni[i].Totale;
                    if (!String.IsNullOrWhiteSpace(t))
                    {
                        float tot = 0;
                        if (float.TryParse(t, out tot))
                        {
                            totaleDonati += tot;
                        }
                    }
                }
            }
            return totaleDonati;

        }

        private Accordi2020 GetAccordi(string matricola, it.rai.servizi.svildigigappws.WSDigigapp service, int anno)
        {
            var db = new digiGappEntities();
            var item = db.MyRai_ArretratiExcel2019.Where(x => x.matricola == matricola).FirstOrDefault();
            if (item == null) return null;

            var responsePF = service.getPianoFerie(matricola, "0101" + anno.ToString(), 80, "");

            int ArretratiDaMettereDaFoglioExcel = (int)item.da_fare;

            float ce = GetFECE_MNCE_MRCE(matricola);
            ArretratiDaMettereDaFoglioExcel -= (int)myRaiCommonTasks.Arretrati2019.GetFECE_MNCE_MRCE_MenoDonateSuFoglioExcel(matricola,ce);

            if (ArretratiDaMettereDaFoglioExcel < 0) ArretratiDaMettereDaFoglioExcel = 0;

            Accordi2020 acc = new Accordi2020();
            acc.EffettivamenteDaFare = ArretratiDaMettereDaFoglioExcel;
            if (acc.EffettivamenteDaFare == 0)
            {
                acc.DaFare = (int)item.da_fare;
                acc.DonateDaFoglioExcel = item.donate;
                acc.Fruite_11_31marzo = (float)item.fruite_11_31;
                acc.FECE_MRCE_MNCE_gapp = ce;
                return acc;
            }


            acc.DaFare = (int)item.da_fare;
            acc.DonateDaFoglioExcel = item.donate;
            acc.Fruite_11_31marzo = (float)item.fruite_11_31;
            acc.FECE_MRCE_MNCE_gapp = ce;//  myRaiCommonTasks.Arretrati2019. GetFECE_MNCE_MRCE(matricola);
            float[] valori =myRaiCommonTasks.Arretrati2019. GetRR_RF_FE(
                               responsePF.dipendente.ferie.mancatiRiposiAnniPrecedenti,
                               responsePF.dipendente.ferie.mancatiFestiviAnniPrecedenti,
                               ArretratiDaMettereDaFoglioExcel, matricola);

            acc.RR_ArretratiDaInserire = valori[0];
            acc.RF_ArretratiDaInserire = valori[1];
            acc.FE_ArretratiDaInserire = valori[2];
            return acc;
        }

        public InviaPianoFerieResponse InviaPianoFerie(string matricolaDipendente, int anno, string sedegapp, 
              string rep, string matricolaOperatore,string notaSegreteria)
        {
            InviaPianoFerieResponse Response = new InviaPianoFerieResponse();
            if (String.IsNullOrWhiteSpace(notaSegreteria))
            {
                Response.esito = false;
                Response.error = "Nota segreteria mancante";
                return Response;
            }
            if (String.IsNullOrWhiteSpace(sedegapp) || anno == 0 || String.IsNullOrWhiteSpace(matricolaDipendente)
                || String.IsNullOrWhiteSpace(matricolaOperatore))
            {
                Response.esito = false;
                Response.error = "dati non valorizzati correttamente";
                return Response;
            }

            sedegapp = sedegapp.ToUpper();
            if (String.IsNullOrWhiteSpace(rep) || rep.Trim() == "0" || rep.Trim() == "00") rep = null;
            var db = new myRaiData.digiGappEntities();
            var PF = db.MyRai_PianoFerie.Where(x => x.matricola == matricolaDipendente && x.anno == anno).FirstOrDefault();

            try
            {
                DateTime D = DateTime.Now;
                if (PF != null)
                {
                    PF.data_consolidato = D ;
                    PF.sedegapp = sedegapp + rep;
                    PF.data_invio_segreteria = D ;
                    PF.matricola_invio_segreteria = matricolaOperatore;
                    PF.nota_invio_segreteria = notaSegreteria;
                }
                else
                {
                    myRaiData.MyRai_PianoFerie p = new myRaiData.MyRai_PianoFerie()
                    {
                        anno = anno,
                        matricola = matricolaDipendente,
                        data_consolidato = D ,
                        sedegapp = sedegapp + rep,
                        data_invio_segreteria = D ,
                        matricola_invio_segreteria = matricolaOperatore,
                        nota_invio_segreteria = notaSegreteria
                    };
                    db.MyRai_PianoFerie.Add(p);
                }
                myRaiCommonTasks.GestoreMail gm = new myRaiCommonTasks.GestoreMail();
                gm.InvioMail("raiplace.selfservice@rai.it", 
                    "Invio piano ferie", 
                    "P" + matricolaDipendente + "@rai.it", 
                    null,
                    "Invio piano ferie da parte dell'Ufficio del Personale", 
                    null, 
                    "L'Ufficio del Personale ha provveduto all'invio del tuo piano ferie all'approvazione del responsabile.");


                
           
                //var ids = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matricolaDipendente &&
                //  x.data.Year == anno).Select(x => x.id).ToList();
                //if (ids != null && ids.Any())
                //{
                //    foreach (int id in ids)
                //    {
                //        var g = db.MyRai_PianoFerieGiorni.Where(x => x.id == id).FirstOrDefault();
                //        if (g != null) db.MyRai_PianoFerieGiorni.Remove(g);
                //    }
                //}

                db.SaveChanges();
                Response.esito = true;

                try
                {
                    MyRai_LogAzioni az = new MyRai_LogAzioni()
                    {
                        applicativo = "WCF",
                        data = DateTime.Now,
                        matricola = matricolaDipendente,
                        provenienza = "InviaPianoFerie",
                        operazione = "Invio PianoFerie HRUP",
                        descrizione_operazione = "Invio per " + matricolaDipendente + " Anno " + anno + " Sede " + sedegapp
                                                     + " Rep " + rep + " Inviato da " + matricolaOperatore
                    };
                    db.MyRai_LogAzioni.Add(az);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
                Response.esito = false;
                Response.error = ex.Message;
            }

            return Response;
        }


        public RigeneraPdfResponse RigeneraPDFpresenze(DateTime dstart, DateTime dend, string matricola, string nominativo,
            string sedeGapp, int livelloUtente)
        {
            var db = new digiGappEntities();

            DIGIRESP_Archivio_PDF pdf = null;
            try
            {
                pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.data_inizio == dstart && x.data_fine == dend &&
                      x.sede_gapp == sedeGapp && x.tipologia_pdf == "P")
                      .FirstOrDefault();
            }
            catch (Exception ex)
            {
                return new RigeneraPdfResponse() { esito = false, error = ex.Message + (ex.InnerException != null ? ex.InnerException.ToString() : "") };
            }
            if (pdf == null)
            {
                return new RigeneraPdfResponse() { esito = false, error = "Nessun PDF trovato per sede e data" };
            }
            if (pdf.stato_pdf != null && pdf.stato_pdf.Trim().ToUpper() == "C_OK")
            {
                return new RigeneraPdfResponse() { esito = false, error = "PDF già convalidato" };
            }
            it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();

            service.Credentials = new System.Net.NetworkCredential(
                   Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                   Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
                   );

            myRaiService.it.rai.servizi.svildigigappws.presenzeResponse response = null;

            try
            {
                response = service.getPresenze(matricola, "*", dstart.ToString("ddMMyyyy"), dend.ToString("ddMMyyyy"), sedeGapp, livelloUtente, null);
            }
            catch (Exception ex)
            {
                return new RigeneraPdfResponse() { esito = false, error = ex.ToString() };
            }

            try
            {
                db.MyRai_LogAzioni.Add(new MyRai_LogAzioni()
                {
                    applicativo = "WCFservice",
                    data = DateTime.Now,
                    descrizione_operazione = "RigeneraPDFpresenze per " + sedeGapp + " inizio:" + dstart.ToString("ddMMyyyy") +
                                           " fine:" + dend.ToString("ddMMyyyy") + " matr:" + matricola + " nom:" + nominativo,
                    matricola = matricola,
                    operazione = "RigeneraPDFpresenze"
                });
                db.SaveChanges();
            }
            catch { }


            if (response.esito == true)
            {
                var dbNnew = new digiGappEntities();
                var pdfrow = dbNnew.DIGIRESP_Archivio_PDF.FirstOrDefault(x => x.ID == pdf.ID);

                return new RigeneraPdfResponse()
                {
                    esito = true,
                    IdPdf = pdf.ID,
                    raw = response.raw,
                    ProgressivoStampa = pdfrow.numero_versione
                };
            }
            else
                return new RigeneraPdfResponse() { esito = false, error = response.errore, raw = response.raw };
        }

        public RigeneraPdfResponse RigeneraPDF(DateTime dstart, DateTime dend, string matricola, string nominativo,
            string sedeGapp, int livelloUtente)
        {
            var db = new digiGappEntities();

            DIGIRESP_Archivio_PDF pdf = null;

            try
            {
                pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.data_inizio == dstart && x.data_fine == dend &&
                      x.sede_gapp == sedeGapp && x.tipologia_pdf == "R")
                      .FirstOrDefault();
            }
            catch (Exception ex)
            {
                return new RigeneraPdfResponse() { esito = false, error = ex.Message + (ex.InnerException != null ? ex.InnerException.ToString() : "") };
            }

            if (pdf == null)
            {
                return new RigeneraPdfResponse() { esito = false, error = "Nessun PDF trovato per sede e data" };
            }
            if (pdf.stato_pdf != null && pdf.stato_pdf.Trim().ToUpper() == "C_OK")
            {
                return new RigeneraPdfResponse() { esito = false, error = "PDF già convalidato" };
            }

            it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();

            service.Credentials = new System.Net.NetworkCredential(
                   Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                   Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
                   );

            try
            {
                myRaiService.it.rai.servizi.svildigigappws.riepilogoSedeGappResponse response =
                      service.getStampa(matricola, nominativo, dstart.ToString("ddMMyyyy"), dend.ToString("ddMMyyyy"),
                      sedeGapp, true, livelloUtente);

                try
                {
                    db.MyRai_LogAzioni.Add(new MyRai_LogAzioni()
                    {
                        applicativo = "WCFservice",
                        data = DateTime.Now,
                        descrizione_operazione = "RigeneraPDF per " + sedeGapp + " inizio:" + dstart.ToString("ddMMyyyy") +
                                               " fine:" + dend.ToString("ddMMyyyy") + " matr:" + matricola + " nom:" + nominativo,
                        matricola = matricola,
                        operazione = "RigeneraPDF"
                    });
                    db.SaveChanges();
                }
                catch { }

                if (response.esito == true)
                {
                    var dbNnew = new digiGappEntities();
                    var pdfrow = dbNnew.DIGIRESP_Archivio_PDF.FirstOrDefault(x => x.ID == pdf.ID);
                    if (response.eccezioni != null && response.eccezioni.Count() > 0)
                    {
                        string eccezioniSerial = Serialize(response.eccezioni);
                        pdfrow.contenuto_eccezioni = eccezioniSerial;
                        db.SaveChanges();
                    }
                    return new RigeneraPdfResponse()
                    {
                        esito = true,
                        IdPdf = response.ID,
                        raw = response.raw,
                        ProgressivoStampa = pdfrow.numero_versione
                    };
                }
                else
                    return new RigeneraPdfResponse() { esito = false, error = response.errore, raw = response.raw };
            }
            catch (Exception ex)
            {
                return new RigeneraPdfResponse() { esito = false, error = ex.Message + (ex.InnerException != null ? ex.InnerException.ToString() : "") };
            }
        }

        private string GetSchedaPresenzeMeseCallString(string matricola, DateTime dstart, DateTime dend)
        {
            //P956,SKP,HRPESSP684930   171201684930,21712012171231,
            return "P956,SKP,HRPESSP" + matricola + "   " + dstart.ToString("yyMMdd") + matricola + "," +
                   dstart.ToString("yyyyMMdd").Substring(0, 1) + dstart.ToString("yyMMdd") +
                   dend.ToString("yyyyMMdd").Substring(0, 1) + dend.ToString("yyMMdd") + ",";
        }

        public GetSchedaPresenzeMeseResponse GetSchedaPresenzeMese(string matricola, DateTime dstart, DateTime dend)
        {
            GetSchedaPresenzeMeseResponse response = new GetSchedaPresenzeMeseResponse() { esito = true };
            // string r = "ACK010526000103650T DDE30BIFANO VINCENZO                             20712180000000RUO/A/SISTEMI DEL P./CDL 002                                                                                                                                                                                                       01 LU90               N    N    N     N      N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      02 MAF0 8:5018:15 8.45N    N    N    PB  0.50N      NCAR*N  0.05N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      03 MEF0 8:5317:14 7.40N    N    N    PN      N      NCAR*N  0.06NPOH N  0.14N    N      N    N      N    N      N    N      N    N      N    N      N    N      04 GIF0 8:4817:57 8.29N    N    N    PB  0.15N      NCAR*N  0.05NROH N  0.14N    N      N    N      N    N      N    N      N    N      N    N      N    N      05 VEFH               BFE  N    N     N      N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      06 SA95               N    N    N     N      N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      07 DO96               N    N    N     N      N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      08 LUF0 9:1318:04 8.12N    N    N    PB  0.15N      NCAR*N  0.04N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      09 MAF0 9:1918:20 8.21N    N    N    PB  0.25N      NCAR*N  0.05N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      10 MEF0 9:3618:39 8.10N    N    N    PB  0.20N      NCAR*N  0.03NPOH N  1.11NROH N  1.11NSEH N  0.15N    N      N    N      N    N      N    N      N    N      11 GIF0 9:2218:15 8.11N    N    N    PB  0.15N      NCAR*N  0.07N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      12 VEFH 9:3819:59 9.33N    N    N    PB  2.35N      NCAR*N  0.09NPOH N  1.13NROH N  1.13N    N      N    N      N    N      N    N      N    N      N    N      13 SA95               N    N    N     N      N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      14 DO96               N    N    N     N      N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      15 LUF0 9:1618:34 8.41N    N    N    PB  0.40N      NCAR*N  0.02N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      16 MAF0 9:3718:22 6.21N    N    N    PN      N      NPOH N  2.40NROH N  1.22NSEH N  0.21N    N      N    N      N    N      N    N      N    N      N    N      17 MEF0 9:2518:45 8.39N    N    N    PN      N      NCAR*N  0.03NROH N  0.38N    N      N    N      N    N      N    N      N    N      N    N      N    N      18 GIF0 9:0918:40 8.49N    N    N    PN      N      NCAR*N  0.03NROH N  0.40N    N      N    N      N    N      N    N      N    N      N    N      N    N      19 VEFH 9:1019:33 9.29N    N    N    PB  2.40N      NCAR*N  0.15N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      20 SA95               N    N    N     N      N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      21 DO96               N    N    N     N      N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      22 LUF0 9:1918:13 8.15N    N    N    PB  0.15N      NCAR*N  0.04N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      23 MAF0 9:1918:20 8.20N    N    N     B  0.25N      NCAR*N  0.06N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      24 MEF0 9:2018:30 8.23N    N    N    PB  0.35N      NCAR*N  0.12N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      25 GIF0 9:2718:45 8.34N    N    N    PB  0.40N      NCAR*N  0.09NPOH N  1.02NROH N  1.02N    N      N    N      N    N      N    N      N    N      N    N      26 VEFH 9:3416:58 6.45N    N    N    PN      N      NCAR*N  0.04NPOH N  1.09NROH N  0.58N    N      N    N      N    N      N    N      N    N      N    N      27 SA95               N    N    N     N      N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      28 DO96               N    N    N     N      N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      N    N      29 LUF0 9:2918:16 8.06N    N    N    PN      N      NCAR*N  0.02NPOH N  1.04NROH N  1.12N    N      N    N      N    N      N    N      N    N      N    N      30 MAF0 9:2318:36 8.32N    N    N    PB  0.30N      NCAR*N  0.06NROH N  0.03N    N      N    N      N    N      N    N      N    N      N    N      N    N      31 MEF0 9:2918:16 8.09N    N    N    PN      N      NPOH N  1.04NROH N  1.04N    N      N    N      N    N      N    N      N    N      N    N      N    N";

            string s2 = "ACK02";
            string responseText = "";
            string strFunzione = "";
            int counter = 0;
            try
            {
                while (s2.StartsWith("ACK02"))
                {
                    counter++;
                    if ( counter > 10 )
                        throw new Exception( "Eccessivo numero di chiamate." );

                    if (s2 == "ACK02")
                        strFunzione = GetSchedaPresenzeMeseCallString(matricola, dstart, dend);
                    else
                    {
                        string d = s2.Substring(10, 6);
                        DateTime D1;
                        if (DateTime.TryParseExact(d, "yyMMdd", null, DateTimeStyles.None, out D1))
                            strFunzione = GetSchedaPresenzeMeseCallString(matricola, D1, dend);
                        else
                            throw new Exception("Data errata: " + d);
                    }
                    ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
                    s2 = (string)c.ComunicaVersoCics(strFunzione);
                    response.raw += s2;
                    int bytesResponse = Convert.ToInt32(s2.Substring(5, 5)) + 10;//10 intestazione ACK compresa
                    if ( s2.Length < bytesResponse )
                        s2 = s2.PadRight( bytesResponse , ' ' );
                    if ( s2.Length > bytesResponse )
                        s2 = s2.Substring( 0 , bytesResponse );
                    responseText = s2.Substring(310);

                    List<string> giorniParziali = (from Match m in Regex.Matches(responseText, @".{160}")
                                                   select m.Value).ToList();
                    foreach (string giorno in giorniParziali)
                    {
                        // per i mesi che hanno meno di 31 giorni il servizio restituisce i dati corretti, ma imposta 
                        // l'esito a False con un messaggio di errore come segue: "Year, Month, and Day parameters describe an un-representable DateTime."
                        // a questo punto va verificato se il giorno che sta prendendo in esame è successivo all'ultimo giorno del mese
                        // Nel caso fosse successivo allora dovrà skippare tale elemento.
                        try
                        {
                            string myDay = giorno.Substring( 0 , 2 );
                            int myGiorno = int.Parse( myDay );
                            int ultimoDelMese = DateTime.DaysInMonth( dstart.Year , dstart.Month );

                            if ( myGiorno > ultimoDelMese )
                            {
                                continue;
                            }
                        }
                        catch ( Exception _ex )
                        {

                        }
                        InfoPresenza i = new InfoPresenza();
                        i.GetCampi(giorno, dstart);
                        response.Giorni.Add(i);
                    }
                }
            }
            catch (Exception ex)
            {
                response.esito = false;
                response.errore = ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : "");
            }
            return response;
        }

        private string GetTimbratureMeseCallString(string matricola, DateTime dstart, DateTime dend)
        {
            string strFunzione = "P956,CO,HRPESSP" + matricola + "   " + dstart.ToString("yyMMdd") + matricola + "," +
                 dstart.ToString("yyyy").Substring(0, 1) + dstart.ToString("yyMMdd") +
                 dend.ToString("yyyy").Substring(0, 1) + dend.ToString("yyMMdd") + "," + "".PadLeft(2000, ' ');
            return strFunzione;
        }

        public GetTimbratureMeseResponse GetTimbratureMese(string matricola, DateTime dstart, DateTime dend)
        {
            GetTimbratureMeseResponse response = new GetTimbratureMeseResponse();

            Regex R = new Regex("(.{8})(.{2})(.{2})(.{10})(.{5})(.)(.{5})(.)(.{5})(.)(.{5})(.)(.{5})(.)(.{5})(.)(.{5})(.)(.{5})(.)(.{5})(.)(.{5})(.)(.{5})(.)(.{5})(.)(.{5})(.)(.{5})(.)(.{5})(.)(.{5})");

            //P956,CO,HRPESSP684930   171201684930,21712012171231,

            string s2 = "ACK02";
            string responseText = "";
            string strFunzione = "";
            List<string> giorni = new List<string>();

            try
            {
                int counter = 0;
                while (s2.StartsWith("ACK02"))
                {
                    counter++;
                    if (counter > 10)
                    {
                        response.esito = false;
                        response.errore = "Eccessivo numero di chiamate";
                        return response;
                    }
                    if (s2 == "ACK02")
                        strFunzione = GetTimbratureMeseCallString(matricola, dstart, dend);
                    else
                    {
                        string d = s2.Substring(10, 6);
                        DateTime D1;
                        if (DateTime.TryParseExact(d, "yyMMdd", null, DateTimeStyles.None, out D1))
                        {
                            strFunzione = GetTimbratureMeseCallString(matricola, D1, dend);
                        }
                        else
                        {
                            response.esito = false;
                            response.errore = "Data errata: " + d;
                            return response;
                        }
                    }
                    ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
                    s2 = (string)c.ComunicaVersoCics(strFunzione);
                    //    s2 = r;
                    response.raw += s2;

                    int bytesResponse = Convert.ToInt32(s2.Substring(5, 5)) + 10;//10 intestazione ACK compresa
                    if ( s2.Length < bytesResponse )
                        s2 = s2.PadRight( bytesResponse , ' ' );
                    if ( s2.Length > bytesResponse )
                        s2 = s2.Substring( 0 , bytesResponse );


                    responseText = s2.Substring(134);
                    List<string> giorniParziali = (from Match m in Regex.Matches(responseText, @".{117}")
                                                   select m.Value).ToList();
                    giorni.AddRange(giorniParziali);
                }

                foreach (string giorno in giorni)
                {
                    Match campi = R.Match(giorno);
                    InfoGiornata ig = new InfoGiornata();
                    ig.CodiceOrario = campi.Groups[3].Value;
                    foreach (Group g in campi.Groups)
                    {
                        if ( g.Value == "E" )
                            ig.TimbraturaE = true;
                    }
                    DateTime D;
                    if (!DateTime.TryParseExact(campi.Groups[1].Value, "yyyyMMdd", null, DateTimeStyles.None, out D))
                    {
                        response.esito = false;
                        response.errore = "Data errata: " + campi.Groups[1].Value;
                        return response;
                    }
                    ig.data = D;
                    for (int index = 5; index <= 33; index += 4)
                    {
                        TimbraturaInOut T = new TimbraturaInOut();
                        T.Ingresso = campi.Groups[index].Value;
                        T.Uscita = campi.Groups[index + 2].Value;
                        if ( String.IsNullOrWhiteSpace( T.Ingresso ) && String.IsNullOrWhiteSpace( T.Uscita ) )
                            continue;
                        ig.Timbrature.Add(T);
                    }
                    response.Giorni.Add(ig);
                }
                response.esito = true;
            }
            catch (Exception ex)
            {
                response.esito = false;
                response.errore = ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : "");
            }

            return response;
        }

        public GetEccezioniComplessiveResponse GetEccezioniComplessive()
        {
            var db = new digiGappEntities();
            try
            {
                var e = db.L2D_ECCEZIONE.OrderBy(x => x.cod_eccezione).ToList();
                return new GetEccezioniComplessiveResponse()
                {
                    success = true,
                    EccezioniComplessive = e
                };
            }
            catch (Exception ex)
            {
                return new GetEccezioniComplessiveResponse()
                {
                    success = false,
                    error = ex.Message
                };
            }
        }

        public GetEccezioniAmmesseResponse GetEccezioniAmmesse()
        {
            var db = new digiGappEntities();
            try
            {
                var response = new GetEccezioniAmmesseResponse()
                {
                    error = null,
                    success = true,
                    EccezioniAmmesse =
                       db.MyRai_Eccezioni_Ammesse.OrderBy(x => x.cod_eccezione)
                         .Select(x => new EccezioneAmmessa()
                         {
                             attiva = x.flag_attivo,
                             codice = x.cod_eccezione,
                             dataFineValidita = x.data_fine_validita,
                             dataInizioValidita = x.data_inizio_validita,
                             descrizione = x.desc_eccezione
                         })
                         .ToList()
                };
                return response;
            }
            catch (Exception ex)
            {
                return new GetEccezioniAmmesseResponse()
                {
                    error = ex.Message,
                    success = false,
                    EccezioniAmmesse = null
                };
            }
        }

        public AllineaGiornataResponse AllineaGiornata(DateTime date, string matricola)
        {
            AllineaGiornataResponse response = new AllineaGiornataResponse();

            it.rai.servizi.svildigigappws.WSDigigapp datiBack = new it.rai.servizi.svildigigappws.WSDigigapp();

            try
            {
                datiBack.Credentials = new System.Net.NetworkCredential(
                       Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                       Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
                       );

                var dettaglioGiornata = datiBack.getEccezioni(matricola, date.ToString("ddMMyyyy"), "BU", 70);

                var db = new digiGappEntities();
                var EccDB = db.MyRai_Eccezioni_Richieste.Where(x => x.data_eccezione == date)
                             .Where(x => x.azione == "I" && x.id_stato != 60 && x.id_stato != 50 && x.MyRai_Richieste.matricola_richiesta == matricola)
                             .ToList();

                Boolean DBtoUpdate = false;
                List<int> Lid = new List<int>();
                foreach (MyRai_Eccezioni_Richieste m in EccDB)
                {
                    if ( m.numero_documento == 0 )
                        continue;

                    if (!dettaglioGiornata.eccezioni.Select(x => x.ndoc)
                        .ToList()
                        .Contains(m.numero_documento.ToString().PadLeft(6, '0')))
                    {
                        var stornoDB = db.MyRai_Eccezioni_Richieste.Where(x => x.azione == "C" &&
                                 x.numero_documento_riferimento == m.numero_documento &&
                                 x.codice_sede_gapp == m.codice_sede_gapp &&
                                 x.id_stato == (int)EnumStatiRichiesta.Approvata).FirstOrDefault();
                        if (m.id_stato == (int)EnumStatiRichiesta.Approvata && stornoDB != null)
                        {
                            //è normale che non ci sia
                        }
                        else
                        {
                            myRaiCommonTasks.CommonTasks.InviaNotificaPerEliminataSuGapp(m, "WCFservice");
                            // stornoDB.id_stato = ( int ) EnumStatiRichiesta.Eliminata;
                            m.id_stato = (int)EnumStatiRichiesta.Eliminata;
                            m.MyRai_Richieste.id_stato = (int)EnumStatiRichiesta.Eliminata;
                            if (m.MyRai_Richieste.MyRai_Eccezioni_Richieste.Any(x => x.azione == "C"))
                            {
                                foreach (var st in m.MyRai_Richieste.MyRai_Eccezioni_Richieste.Where(x => x.azione == "C"))
                                {
                                    st.id_stato = (int)EnumStatiRichiesta.Eliminata;
                                }
                            }
                            MyRai_LogAzioni la = new MyRai_LogAzioni()
                            {
                                applicativo = "WCFservice",
                                data = DateTime.Now,
                                matricola = matricola,
                                provenienza = "AllineaGiornata",
                                operazione = "ELIMINATA SU GAPP",
                                descrizione_operazione = "id rich:" + m.MyRai_Richieste.id_richiesta + " id ecc_rich:" + m.id_eccezioni_richieste
                            };
                            db.MyRai_LogAzioni.Add(la);
                            DBtoUpdate = true;
                            Lid.Add(m.MyRai_Richieste.id_richiesta);
                        }
                    }
                }
                if ( DBtoUpdate )
                    db.SaveChanges( );
                response.success = true;
                response.error = null;
                response.IdRichiesteEliminateDB = Lid.ToArray();
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error = ex.Message;
                response.IdRichiesteEliminateDB = null;
            }
            return response;

        }

        public myRaiService.classi.RecuperaPdfResponse recuperaPdf(string sedeGapp, DateTime Data, int? id)
        {
            myRaiService.classi.RecuperaPdfResponse response = new classi.RecuperaPdfResponse();

            List<myRaiService.classi.Pdf> PDFlist = new List<classi.Pdf>();
            var db = new digiGappEntities();
            List<DIGIRESP_Archivio_PDF> query = new List<DIGIRESP_Archivio_PDF>();

            try
            {
                if (id == null || id == 0)
                    query = db.DIGIRESP_Archivio_PDF.Where(x =>
                                                      x.sede_gapp == sedeGapp &&
                                                      x.data_inizio <= Data && x.data_fine >= Data)
                                                     .OrderBy(x => x.tipologia_pdf)
                                                     .ThenByDescending(x => x.numero_versione)
                                                     .ToList();
                else
                    query = db.DIGIRESP_Archivio_PDF.Where(x =>
                                                           x.ID == id)
                                                           .ToList();

                if (query.Any())
                {
                    foreach (var pdf in query)
                    {
                        myRaiService.classi.Pdf item = new classi.Pdf()
                        {
                            content = pdf.pdf,
                            ID = pdf.ID,
                            numero_versione = pdf.numero_versione,
                            stato_pdf = pdf.stato_pdf,
                            tipo = pdf.tipologia_pdf,
                            DataDa = pdf.data_inizio,
                            DataA = pdf.data_fine
                        };
                        PDFlist.Add(item);
                    }
                }
                response.PdfList = PDFlist.ToArray();
            }
            catch (Exception ex)
            {
                db.MyRai_LogErrori.Add(new MyRai_LogErrori()
                {
                    applicativo = "WCFservice",
                    data = DateTime.Now,
                    error_message = "Sede: " + sedeGapp + ",Data: " + Data + " : " + ex.ToString(),
                    provenienza = "RecuperaPDF",
                    matricola = ""
                });
                db.SaveChanges();
                response.esito = false;
                response.error = ex.Message;
            }

            return response;
        }

        public GetFerieResponse GetFerie(string matricola, string anno)
        {
            string strChr = "00000";
            if (String.IsNullOrWhiteSpace(matricola))
            {
                return new GetFerieResponse() { success = false, raw = null, datiDipendente = null, error = "Matricola non specificata" };
            }

            string UserName = matricola.PadRight(10, ' ');
            string UserNameRequester = matricola;
            string strZoomRic = "";
            if ( string.IsNullOrWhiteSpace( anno ) )
                anno = DateTime.Now.Year.ToString( );
            strZoomRic = anno.Substring(anno.Length - 2) + " TFE";

            string strFunzione = "P956,PD,HRAWEB" + UserName + "0" + strChr + UserNameRequester + "," + strZoomRic + ",";
            strFunzione += "".PadLeft(2000, ' ');

            string s2 = "";

            try
            {
                ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
                s2 = (string)c.ComunicaVersoCics(strFunzione);
            }
            catch (Exception ex)
            {
                return new GetFerieResponse() { success = false, raw = s2, datiDipendente = null, error = "Chiamata CICS in errore:" + ex.ToString() };
            }

            Boolean Esito = s2 != null && s2.StartsWith("ACK01");
            if ( Esito == false )
                return new GetFerieResponse( ) { success = false , raw = s2 , datiDipendente = null , error = "ACK01 non trovato" };

            if ( s2.Length < 1454 )
                return new GetFerieResponse( ) { success = false , raw = s2 , datiDipendente = null , error = "Lunghezza risposta non corretta" };


            string sp = s2.Substring(754);
            string svalori = s2.Substring(1454);

            int startDescrittive = 0;
            int startValori = 0;

            GetFerieResponse response = new GetFerieResponse() { success = true, raw = s2 };

            List<GetFerieRow> ferierows = new List<GetFerieRow>();

            while (true)
            {
                try
                {
                    string descrittiva = sp.Substring(startDescrittive, 35);
                    if (descrittiva.Trim().EndsWith(" E") || descrittiva.Trim() == "")
                        break;

                    GetFerieRow g = new GetFerieRow();
                    string descrittivaEccezione = descrittiva.Trim();

                    ferierows.Add(g);
                    startDescrittive += 35;

                    string valori = svalori.Substring(startValori, 21);
                    string descrittivaValori = valori.Trim();
                    startValori += 21;

                    if (!String.IsNullOrWhiteSpace(descrittivaValori))
                    {
                        string[] parti = descrittivaEccezione.Split(' ');
                        g.codiceEccezione = parti[0].Trim().Substring(1);
                        g.descEccezione = parti[1].Trim().Substring(1);

                        g.fruiti = Convert.ToSingle(descrittivaEccezione.Substring(descrittivaEccezione.Length - 5)) / Convert.ToSingle(100);

                        string[] partiValori = descrittivaValori.Replace(",", ".").Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        g.spettanti = Convert.ToSingle(partiValori[0], CultureInfo.InvariantCulture);
                        g.residui = Convert.ToSingle(partiValori[1], CultureInfo.InvariantCulture);
                        g.pianificati = Convert.ToSingle(partiValori[2], CultureInfo.InvariantCulture);
                    }
                }
                catch (Exception ex)
                {
                    return new GetFerieResponse() { success = false, raw = s2, datiDipendente = null, error = ex.ToString() };
                }
            }

            response.datiDipendente = ferierows;

            return response;

        }

        public GetDocumentoEccezioneResponse GetDocumentoEccezione(int IdDocumento)
        {
            var db = new digiGappEntities();
            try
            {
                var doc = db.MyRai_DocumentiDipendente.Where(x => x.id == IdDocumento).FirstOrDefault();
                if ( doc == null )
                    return new GetDocumentoEccezioneResponse( )
                {
                    Error = "Documento non trovato",
                    Success = false
                };

                else
                    return new GetDocumentoEccezioneResponse( )
                {
                    Success = true,
                    document = doc.byte_content
                };

            }
            catch (Exception ex)
            {
                return new GetDocumentoEccezioneResponse()
                {
                    Error = ex.Message,
                    Success = false
                };
            }
        }

        public DettaglioGiornataResponse GetDettagliGiornata(DateTime data, string matricola)
        {
            var db = new digiGappEntities();
            var sedi = db.L2D_SEDE_GAPP.Select(x => new { cod = x.cod_sede_gapp, desc = x.desc_sede_gapp });

            var query = db.MyRai_Richieste.Where(x =>
                 x.matricola_richiesta == matricola
              && x.periodo_dal <= data
              && x.periodo_al >= data);

            List<DettaglioRichiesta> D = new List<DettaglioRichiesta>();
            foreach (MyRai_Richieste q in query)
            {
                int? IdDocAssociato = null;
                if (q.MyRai_Associazione_Richiesta_Doc != null && q.MyRai_Associazione_Richiesta_Doc.Count > 0)
                {
                    var a = q.MyRai_Associazione_Richiesta_Doc.FirstOrDefault();
                    if ( a != null )
                        IdDocAssociato = a.id_documento;
                }

                DettaglioRichiesta d = CopyObject<DettaglioRichiesta>(q);
                d.IdDocumentoAllegato = IdDocAssociato;
                d.descr_sede_gapp = sedi.Where(x => x.cod == q.codice_sede_gapp).Select(x => x.desc).FirstOrDefault();

                if (q.MyRai_AttivitaCeiton != null)
                {
                    d.AttivitaCeiton = CopyObject<AttCeiton>(q.MyRai_AttivitaCeiton);
                }
                List<DettaglioEccezione> Ecc = new List<DettaglioEccezione>();
                foreach (var ecc in q.MyRai_Eccezioni_Richieste)
                {
                    if (ecc.data_eccezione == data)
                    {
                        DettaglioEccezione dett = CopyObject<DettaglioEccezione>(ecc);
                        Ecc.Add(dett);
                    }
                }
                d.DettagliEccezione = Ecc.ToArray();
                D.Add(d);
            }
            List<MyRai_Eccezioni_Ammesse> L = db.MyRai_Eccezioni_Ammesse.ToList();
            foreach (var item in D)
            {
                foreach (var child in item.DettagliEccezione)
                {
                    child.desc_eccezione = L.Where(x => x.cod_eccezione.Trim() == child.cod_eccezione.Trim()).Select(x => x.desc_eccezione).FirstOrDefault();
                }
            }

            it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();

            service.Credentials = new System.Net.NetworkCredential(
                   Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                   Eccezioni.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
                   );
            DettaglioGiornataResponse response = new DettaglioGiornataResponse();
            try
            {
                var respGapp = service.getEccezioni(matricola, data.ToString("ddMMyyyy"), "BU", 75);

                response.RelazioniEccezioni =
                        respGapp.eccezioni
                        .Select(e => new RelazioneEccezioni { numdoc = int.Parse(e.ndoc), eccezioneGapp = e })
                        .Union(D
                        .SelectMany(s => s.DettagliEccezione
                        .Select(z => new RelazioneEccezioni
                        {
                            numdoc =
                                (z.numero_documento != null ? (int)z.numero_documento : 0),
                            eccezioneDB = s
                        })))
                        .GroupBy(p => p.numdoc)
                        .Select(zz => new RelazioneEccezioni
                        {
                            numdoc = zz.Key,

                            eccezioneGapp = zz
                            .Where(x => x.eccezioneGapp != null)
                            .Select(y => y.eccezioneGapp).FirstOrDefault(),

                            eccezioneDB = zz
                            .Where(x => x.eccezioneDB != null)
                            .Select(q => q.eccezioneDB).FirstOrDefault()
                        }).ToList();

                response.success = true;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error = ex.ToString();
            }

            return response;
        }
        public GetRipianificazioniMatricolaResponse GetRipianificazioniMatricola(string matricola, DateTime DataInizio, DateTime DataFine)
        {
            var db = new digiGappEntities();
            try
            {
                var query = db.MyRai_Richieste.Where(x =>
                x.matricola_richiesta == matricola &&
                           x.MyRai_Eccezioni_Richieste.Any(
                               z => z.azione == "C"
                               && z.DataRipianificazione != null
                               && z.data_eccezione >= DataInizio
                               && z.data_eccezione <= DataFine)
                       )
                       .OrderBy(x => x.periodo_dal)
                       .ToList();

                var response = new GetRipianificazioniMatricolaResponse() { esito = true };
                foreach (var item in query)
                {
                    var eccric = item.MyRai_Eccezioni_Richieste.Where(x => x.azione == "C").FirstOrDefault();
                    response.Ripianificazioni.Add(new RipianMatricola()
                    {
                        stato = item.id_stato,
                        DataEccezione = eccric.data_eccezione,
                        CodiceEccezione = eccric.cod_eccezione,
                        motivo = eccric.motivo_richiesta,
                        DataRipianificazione = eccric.DataRipianificazione,
                        MatricolaApprovatoreStorno = eccric.matricola_primo_livello,
                        NominativoApprovatoreStorno = eccric.nominativo_primo_livello,
                        DataApprovazioneStorno = eccric.data_validazione_primo_livello
                    });
                }
                return response;
            }
            catch (Exception ex)
            {
                return new GetRipianificazioniMatricolaResponse() { esito = false, error = ex.Message };
            }
        }
        public GetRipianificazioniResponse GetRipianificazioni(string[] SediGapp, DateTime DataInizio, DateTime DataFine)
        {
            var db = new digiGappEntities();
            var query = db.MyRai_Richieste.Where(x =>

            SediGapp.Contains(x.codice_sede_gapp) &&
                   (x.MyRai_Eccezioni_Richieste.Any(
                       z => z.azione == "C"
                       && z.DataRipianificazione != null
                       && z.data_eccezione >= DataInizio
                       && z.data_eccezione <= DataFine))
               )
               .OrderBy(x => x.codice_sede_gapp).ThenBy(x => x.reparto).ThenBy(x => x.nominativo).ThenBy(x => x.periodo_dal)
               .ToList();

            GetRipianificazioniResponse response = new GetRipianificazioniResponse();
            var temp = query.Select(x => new
            {
                x.codice_sede_gapp,
                x.reparto,
                x.matricola_richiesta,
                x.nominativo,
                x.id_stato,
                eccezioni_richieste = x.MyRai_Eccezioni_Richieste.Where(a => a.azione == "C").FirstOrDefault()
            });
            var tempSede = temp.GroupBy(x => x.codice_sede_gapp);
            foreach (var s in tempSede)
            {
                SedeRipianificazioni sr = new SedeRipianificazioni();
                sr.Sede = s.Key;
                response.Sedi.Add(sr);

                var tempRep = s.GroupBy(x => x.reparto);
                foreach (var r in tempRep)
                {
                    Rep rep = new Rep();
                    rep.reparto = r.Key;
                    sr.Reparti.Add(rep);

                    foreach (var item in r.ToList())
                    {
                        if (item.eccezioni_richieste != null)
                        {
                            rep.Ripianificazioni.Add(new Ripian()
                            {
                                Matricola = item.matricola_richiesta,
                                Nominativo = item.nominativo,
                                stato = item.id_stato,
                                DataEccezione = item.eccezioni_richieste.data_eccezione,
                                CodiceEccezione = item.eccezioni_richieste.cod_eccezione,
                                motivo = item.eccezioni_richieste.motivo_richiesta,
                                DataRipianificazione = item.eccezioni_richieste.DataRipianificazione,
                                MatricolaApprovatoreStorno = item.eccezioni_richieste.matricola_primo_livello,
                                NominativoApprovatoreStorno = item.eccezioni_richieste.nominativo_primo_livello,
                                DataApprovazioneStorno = item.eccezioni_richieste.data_validazione_primo_livello
                            });
                        }
                    }
                }
            }
            return response;
        }
        public DettaglioRichiesta[] GetDettagli(string[] SediGapp, DateTime DataInizio, DateTime DataFine, EnumWorkflowRichieste WorkFlow, EnumStatiRichiesta stato, EnumTipoRicerca? Ricerca = null)
        {
            var db = new digiGappEntities();
            var sedi = db.L2D_SEDE_GAPP.Select(x => new { cod = x.cod_sede_gapp, desc = x.desc_sede_gapp });


            IQueryable<MyRai_Richieste> query = db.MyRai_Richieste;
            if (Ricerca == null || Ricerca == EnumTipoRicerca.DataRichiesta)
            {
                query = query.Where(x =>
            SediGapp.Contains(x.codice_sede_gapp) &&
                    (
                    (x.MyRai_Eccezioni_Richieste.Any(z => z.azione == "C" && z.data_creazione >= DataInizio && z.data_creazione <= DataFine))
                    ||
                    (x.data_richiesta >= DataInizio && x.data_richiesta <= DataFine)
                   )
                );
            }
            else
            {
                query = query.Where(x =>
                            SediGapp.Contains(x.codice_sede_gapp) &&
                                    (
                                    (x.MyRai_Eccezioni_Richieste.Any(z => z.azione == "C" && z.data_eccezione >= DataInizio && z.data_eccezione <= DataFine))
                                    ||
                                    (x.periodo_dal >= DataInizio && x.periodo_dal <= DataFine)
                                   )
                              );
            }


            if (stato != (EnumStatiRichiesta.TuttiGliStati))
            {
                int statoRichiesto = (int)stato;
                query = query.Where(x => x.id_stato == statoRichiesto);
            }

            switch (WorkFlow)
            {
                case EnumWorkflowRichieste.Segreteria:
                    query = query.Where(x =>
                     x.id_stato == (int)EnumStatiRichiesta.InseritoSegreteria
                  || x.id_stato == (int)EnumStatiRichiesta.InProgressSegreteria
                  || x.MyRai_Eccezioni_Richieste.FirstOrDefault().data_approvazione_segreteria != null
                  );
                    break;

                case EnumWorkflowRichieste.UfficioPersonale:
                    query = query.Where(x =>
                      x.id_stato == (int)EnumStatiRichiesta.InseritoUfficioPersonale
                   || x.id_stato == (int)EnumStatiRichiesta.InProgressUfficioPersonale
                   || x.MyRai_Eccezioni_Richieste.FirstOrDefault().data_approvazione_uff_personale != null
                   );
                    break;



            }
            List<DettaglioRichiesta> D = new List<DettaglioRichiesta>();
            foreach (MyRai_Richieste q in query)
            {
                try
                {
                    int? IdDocAssociato = null;
                    if (q.MyRai_Associazione_Richiesta_Doc != null && q.MyRai_Associazione_Richiesta_Doc.Count > 0)
                    {
                        var a = q.MyRai_Associazione_Richiesta_Doc.FirstOrDefault();
                        if ( a != null )
                            IdDocAssociato = a.id_documento;
                    }

                    DettaglioRichiesta d = CopyObject<DettaglioRichiesta>(q);
                    d.IdDocumentoAllegato = IdDocAssociato;
                    d.descr_sede_gapp = sedi.Where(x => x.cod == q.codice_sede_gapp).Select(x => x.desc).FirstOrDefault();

                    if (q.MyRai_AttivitaCeiton != null)
                    {
                        d.AttivitaCeiton = CopyObject<AttCeiton>(q.MyRai_AttivitaCeiton);
                    }

                    List<DettaglioEccezione> Ecc = new List<DettaglioEccezione>();
                    foreach (var ecc in q.MyRai_Eccezioni_Richieste)
                    {
                        DettaglioEccezione dett = CopyObject<DettaglioEccezione>(ecc);
                        Ecc.Add(dett);
                    }
                    d.DettagliEccezione = Ecc.ToArray();


                    D.Add(d);
                }
                catch (Exception e)
                {
                    int a = 10;
                }
            }
            List<MyRai_Eccezioni_Ammesse> L = db.MyRai_Eccezioni_Ammesse.ToList();
            foreach (var item in D)
            {
                foreach (var child in item.DettagliEccezione)
                {
                    try
                    {
                        child.desc_eccezione = L.Where(x => x.cod_eccezione.Trim() == child.cod_eccezione.Trim()).Select(x => x.desc_eccezione).FirstOrDefault();
                    }
                    catch (Exception ee ) 
                    {

                        int b = 20;
                    }
                }
            }
            return D.ToArray();

        }

        public CambiaStatoResponse CambiaStato(int IdRichiesta, EnumStatiRichiesta stato, string matricola, string nota)
        {
            if (stato != EnumStatiRichiesta.InProgressSegreteria
                && stato != EnumStatiRichiesta.InseritoSegreteria
                && stato != EnumStatiRichiesta.InProgressUfficioPersonale
                && stato != EnumStatiRichiesta.InseritoUfficioPersonale
                && stato != EnumStatiRichiesta.InApprovazione
                && stato != EnumStatiRichiesta.Rifiutata
                )
            {
                return new CambiaStatoResponse() { Success = false, Error = "Stato non ammesso in questo contesto" };
            }

            var db = new digiGappEntities();
            var richiesta = db.MyRai_Richieste.Where(x => x.id_richiesta == IdRichiesta).FirstOrDefault();
            if (richiesta == null)
            {
                return new CambiaStatoResponse() { Success = false, Error = "Richiesta non trovata con id:" + IdRichiesta };
            }

            if (
                (stato == EnumStatiRichiesta.InProgressSegreteria && richiesta.id_stato != (int)EnumStatiRichiesta.InseritoSegreteria)
                ||
                 (stato == EnumStatiRichiesta.InseritoSegreteria && richiesta.id_stato != (int)EnumStatiRichiesta.InProgressSegreteria)
                ||
                (stato == EnumStatiRichiesta.InProgressUfficioPersonale && richiesta.id_stato != (int)EnumStatiRichiesta.InseritoUfficioPersonale)
                ||
                 (stato == EnumStatiRichiesta.InseritoUfficioPersonale && richiesta.id_stato != (int)EnumStatiRichiesta.InProgressUfficioPersonale)
                ||
                (stato == EnumStatiRichiesta.InApprovazione
                    && richiesta.id_stato != (int)EnumStatiRichiesta.InseritoSegreteria
                    && richiesta.id_stato != (int)EnumStatiRichiesta.InProgressSegreteria
                    && richiesta.id_stato != (int)EnumStatiRichiesta.InseritoUfficioPersonale
                    && richiesta.id_stato != (int)EnumStatiRichiesta.InProgressUfficioPersonale
                )
                ||
                (stato == EnumStatiRichiesta.Rifiutata
                    && richiesta.id_stato != (int)EnumStatiRichiesta.InseritoSegreteria
                    && richiesta.id_stato != (int)EnumStatiRichiesta.InProgressSegreteria
                    && richiesta.id_stato != (int)EnumStatiRichiesta.InseritoUfficioPersonale
                    && richiesta.id_stato != (int)EnumStatiRichiesta.InProgressUfficioPersonale
                )
                )
            {
                return new CambiaStatoResponse() { Success = false, Error = "Transizione di stato non ammessa per questa richiesta" };
            }

            richiesta.id_stato = (int)stato;
            foreach (var child in richiesta.MyRai_Eccezioni_Richieste)
            {
                child.id_stato = (int)stato;
                if (stato == EnumStatiRichiesta.InseritoSegreteria)
                {
                    child.matricola_segreteria = matricola;
                    child.nota_segreteria = nota;
                    child.data_in_carico_segreteria = null;
                }
                if (stato == EnumStatiRichiesta.InseritoUfficioPersonale)
                {
                    child.matricola_uff_personale = matricola;
                    child.nota_uff_personale = nota;
                    child.data_in_carico_uff_personale = null;
                }
                if (stato == EnumStatiRichiesta.Rifiutata)
                {
                    if (child.id_stato == (int)EnumStatiRichiesta.InseritoSegreteria || child.id_stato == (int)EnumStatiRichiesta.InProgressSegreteria)
                    {
                        child.matricola_segreteria = matricola;
                        child.data_rifiuto_segreteria = DateTime.Now;
                        child.nota_segreteria = nota;
                    }
                    if (child.id_stato == (int)EnumStatiRichiesta.InseritoUfficioPersonale || child.id_stato == (int)EnumStatiRichiesta.InProgressUfficioPersonale)
                    {
                        child.matricola_uff_personale = matricola;
                        child.data_rifiuto_uff_personale = DateTime.Now;
                        child.nota_uff_personale = nota;
                    }
                }
                if (stato == EnumStatiRichiesta.InApprovazione)
                {
                    if (child.id_stato == (int)EnumStatiRichiesta.InseritoSegreteria || child.id_stato == (int)EnumStatiRichiesta.InProgressSegreteria)
                    {
                        child.matricola_segreteria = matricola;
                        child.data_approvazione_segreteria = DateTime.Now;
                        child.nota_segreteria = nota;
                    }
                    if (child.id_stato == (int)EnumStatiRichiesta.InseritoUfficioPersonale || child.id_stato == (int)EnumStatiRichiesta.InProgressUfficioPersonale)
                    {
                        child.matricola_uff_personale = matricola;
                        child.data_approvazione_uff_personale = DateTime.Now;
                        child.nota_uff_personale = nota;
                    }
                    AggiungiEccezioneResponse response = Eccezioni.aggiungiEccezione(IdRichiesta);

                    if (!String.IsNullOrWhiteSpace(response.Error))
                    {
                        var err = new MyRai_LogErrori()
                        {
                            applicativo = "MYRAISERVICE",
                            data = DateTime.Now,
                            error_message = response.Error,
                            matricola = matricola,
                            provenienza = "myRaiService AggiungiEccezioneResponse"
                        };
                        var dberr = new digiGappEntities();
                        dberr.MyRai_LogErrori.Add(err);
                        try
                        {
                            dberr.SaveChanges();
                        }
                        catch (Exception)
                        {

                        }
                        return new CambiaStatoResponse() { Success = false, Error = response.Error };

                    }

                }
                if (stato == EnumStatiRichiesta.InProgressSegreteria)
                {

                    child.matricola_in_carico_segreteria = matricola;
                    child.data_in_carico_segreteria = DateTime.Now;

                }
                if (stato == EnumStatiRichiesta.InProgressUfficioPersonale)
                {

                    child.matricola_in_carico_uff_personale = matricola;
                    child.data_in_carico_uff_personale = DateTime.Now;

                }
            }

            try
            {
                db.SaveChanges();
                return new CambiaStatoResponse() { Success = true };
            }
            catch (Exception ex)
            {
                return new CambiaStatoResponse() { Success = false, Error = ex.Message + ex.InnerException != null ? ":" + ex.InnerException.Message : "" };
            }

        }

        public Riepilogo GetRiepilogo(string[] SediGapp, DateTime? DataInizio, DateTime? DataFine, EnumWorkflowRichieste? WorkFlow = null, EnumTipoRicerca? Ricerca = null)
        {
            var db = new digiGappEntities();
            var query = db.MyRai_Richieste.Where(x => x.MyRai_Eccezioni_Richieste.Count > 0);

            if (SediGapp != null && SediGapp.Length > 0)
            {
                query = query.Where(x => SediGapp.Contains(x.codice_sede_gapp));
            }


            switch (WorkFlow)
            {
                case EnumWorkflowRichieste.Segreteria:
                    query = query.Where(x =>
                     x.id_stato == (int)EnumStatiRichiesta.InseritoSegreteria
                  || x.id_stato == (int)EnumStatiRichiesta.InProgressSegreteria
                  || x.MyRai_Eccezioni_Richieste.FirstOrDefault().data_approvazione_segreteria != null
                  );
                    break;

                case EnumWorkflowRichieste.UfficioPersonale:
                    query = query.Where(x =>
                      x.id_stato == (int)EnumStatiRichiesta.InseritoUfficioPersonale
                   || x.id_stato == (int)EnumStatiRichiesta.InProgressUfficioPersonale
                   || x.MyRai_Eccezioni_Richieste.FirstOrDefault().data_approvazione_uff_personale != null
                   );
                    break;

                case null:
                case EnumWorkflowRichieste.TuttiISettori:

                    break;
            }
            if (Ricerca == null || Ricerca == EnumTipoRicerca.DataRichiesta)
            {
            if (DataInizio != null)
            {
                query = query.Where(x => x.data_richiesta >= DataInizio);
            }
            if (DataFine != null)
            {
                query = query.Where(x => x.data_richiesta <= DataFine);
            }
            }
            else
            {
                if (DataInizio != null)
                {
                    query = query.Where(x => x.periodo_dal >= DataInizio);
                }
                if (DataFine != null)
                {
                    query = query.Where(x => x.periodo_dal <= DataFine);
                }
            }



            var res = query.GroupBy(
                      p => p.id_stato,
                      p => p.id_stato, (key, g) => new { stato = key, tot = g.Count() }).ToList();

            Riepilogo r = new Riepilogo()
            {
                StatiRichieste = new List<StatoRichiesta>(),
                SediPdf = new List<SedePDF>()
            };

            foreach (EnumStatiRichiesta s in Enum.GetValues(typeof(EnumStatiRichiesta)))
            {
                r.StatiRichieste.Add(new StatoRichiesta()
                {
                    stato = s,
                    TotaleRichieste = res.Where(x => x.stato == (int)s).Select(x => x.tot).FirstOrDefault()
                });
            }
            if (SediGapp != null && SediGapp.Any() && DataInizio != null && DataFine != null)
            {
                foreach (var sede in SediGapp.OrderBy(x => x))
                {
                    SedePDF sp = new SedePDF();
                    sp.PDFlist = new List<PDFperiodo>();
                    sp.Sede = sede;
                    r.SediPdf.Add(sp);

                    var list = db.DIGIRESP_Archivio_PDF.Where(x =>
                        x.sede_gapp == sede
                     && x.matricola_convalida == null
                     && x.data_inizio >= DataInizio
                     && x.data_fine <= DataFine
                     )
                     .OrderBy(x => x.data_inizio)
                     .Select(x => new { x.data_inizio, x.data_fine, x.tipologia_pdf })
                     .ToList();
                    foreach (var item in list)
                    {
                        sp.PDFlist.Add(new PDFperiodo()
                        {
                            DataInizio = item.data_inizio,
                            DataFine = item.data_fine,
                            Tipo = item.tipologia_pdf
                        });
                    }
                    sp.PDFlist = sp.PDFlist.OrderBy(x => x.Tipo).ThenBy(x => x.DataInizio).ToList();
                }
            }
            return r;
        }

        public DatiStorno[] GetStorni(string[] ndoc)
        {
            var db = new digiGappEntities();

            List<int> Lndoc = Array.ConvertAll(ndoc, int.Parse).ToList();


            var query = db.MyRai_Richieste.Where(x => x.MyRai_Eccezioni_Richieste.Count > 0)
                .Where(x => x.MyRai_Eccezioni_Richieste
                    .Any(z => Lndoc.Contains((int)z.numero_documento_riferimento))).ToList();

            List<DatiStorno> D = new List<DatiStorno>();
            foreach (int numdoc in Lndoc)
            {
                DatiStorno sr = new DatiStorno();
                sr.ndoc = numdoc.ToString();

                foreach (var richiesta in query)
                {
                    if (richiesta.MyRai_Eccezioni_Richieste.Any(z => z.numero_documento_riferimento == numdoc))
                    {
                        DettaglioRichiesta d = CopyObject<DettaglioRichiesta>(richiesta);
                        List<DettaglioEccezione> Ecc = new List<DettaglioEccezione>();
                        foreach (var ecc in richiesta.MyRai_Eccezioni_Richieste)
                        {
                            DettaglioEccezione dett = CopyObject<DettaglioEccezione>(ecc);
                            Ecc.Add(dett);
                        }
                        d.DettagliEccezione = Ecc.ToArray();
                        sr.Richiesta = d;
                    }
                }
                D.Add(sr);
            }
            List<MyRai_Eccezioni_Ammesse> L = db.MyRai_Eccezioni_Ammesse.ToList();
            foreach (var item in D.Where(z => z.Richiesta != null))
            {
                foreach (var child in item.Richiesta.DettagliEccezione)
                {
                    child.desc_eccezione = L.Where(x => x.cod_eccezione.Trim() == child.cod_eccezione.Trim()).Select(x => x.desc_eccezione).FirstOrDefault();
                }
            }

            return D.Where(d => d.Richiesta != null).ToArray();
        }

        private bool PohPatchEnabled(string anno)
        {
            digiGappEntities db = new digiGappEntities();
            return db.MyRai_ParametriSistema.Any(x => x.Chiave == "PohPatch" && x.Valore1 == "ON" && x.Valore2 == anno);
        }

        public ProvvedimentiCauseResponse GetProvvedimentiCause(string MatricolaChiamante, string MatricolaOggetto)
        {
            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
            string chiamata = string.Format("P956,CL,HRAWEBP{0}   000000{1},,", MatricolaChiamante, MatricolaOggetto + "".PadRight(2000, ' '));

            string RispostaCics = (string)c.ComunicaVersoCics(chiamata);

            return new ProvvedimentiCause().GetProvvedimentiCause(RispostaCics);

        }

        public GetRuoliResponse GetRuoli(string matricola, DateTime DataStart, string tipologia)
        {
            // 3RM - M9GAP035
            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
            GetRuoliResponse Response = new GetRuoliResponse();
            Response.Eccezioni = new List<GetRuoliItem>();

            string S = areaSistema("3RM", matricola, 75);
            matricola = matricola.PadLeft(7, '0');
            S += matricola + DataStart.ToString("ddMMyyyy") + tipologia;

            string StringToSend = S;

            while (StringToSend != null)
            {
                string RispostaCics = (string)c.ComunicaVersoCics(StringToSend.PadRight(8000));
                RispostaCics = RispostaCics.Replace(Convert.ToChar(0x0).ToString(), " ");
                Response.raw = RispostaCics;
                Response.sent += "      " + StringToSend;
                int Blocchi = 0;
                if (!Int32.TryParse(RispostaCics.Substring(113, 3), out Blocchi))
                {
                    Response.Success = false;
                    Response.Error = "Impossibile rilevare numero di oggetti restituiti";
                    return Response;
                }
                wApiUtility.cicsResponse cicsResp = wApiUtility.checkResponse(RispostaCics);

                if (cicsResp.esito == false)
                {
                    Response.Success = false;
                    Response.Error = cicsResp.codErrore + " - " + cicsResp.descErrore;
                    return Response;
                }
                if (cicsResp.richiama)
                    StringToSend = areaSistema("3RM", matricola, 75, true) + RispostaCics.Substring(92, 50) + ",";
                else
                    StringToSend = null;


                int startIndex = 142;
                int tot = startIndex + (Blocchi * 70);
                if (RispostaCics.Length < tot) RispostaCics = RispostaCics.PadRight(tot, ' ');

                while (startIndex + 70 <= RispostaCics.Length)
                {
                    if (RispostaCics.Substring(startIndex, 70).Trim() == "" || RispostaCics.Substring(startIndex, 70).Length < 70)
                        break;

                    GetRuoliItem item = GetItem(RispostaCics.Substring(startIndex, 70));
                    if (item.ParseError != null)
                    {
                        Response.Success = false;
                        Response.Error = item.ParseError + " (startindex " + startIndex + ")";
                        return Response;
                    }
                    else
                        Response.Eccezioni.Add(item);

                    startIndex += 70;
                }
            }

            Response.Success = true;
            return Response;
        }
        private GetRuoliItem GetItem(string itemString)
        {
            GetRuoliItem Item = new GetRuoliItem();
            try
            {
                Item.CodiceEccezione = itemString.Substring(0, 4).Trim();
                DateTime D;
                DateTime.TryParseExact(itemString.Substring(4, 8), "ddMMyyyy", null, DateTimeStyles.None, out D);
                Item.DataDocumento = D;
                Item.NumeroDocumento = itemString.Substring(12, 6);
                Item.InizioEccezioneMinuti = itemString.Substring(18, 4);
                Item.InizioEccezioneHHMM = ToHHMM(Convert.ToInt32(Item.InizioEccezioneMinuti));
                Item.InizioEccezioneDatetime = ToDateTime(D, Convert.ToInt32(Item.InizioEccezioneMinuti));

                Item.FineEccezioneMinuti = itemString.Substring(22, 4);
                Item.FineEccezioneHHMM = ToHHMM(Convert.ToInt32(Item.FineEccezioneMinuti));
                Item.FineEccezioneDatetime = ToDateTime(D, Convert.ToInt32(Item.FineEccezioneMinuti));

                Item.OraEccezioneMinuti = itemString.Substring(26, 4);
                Item.OraEccezioneHHMM = ToHHMM(Convert.ToInt32(Item.OraEccezioneMinuti));

                Item.TipoEccezione = itemString.Substring(30, 1);
                Item.FlagStorno = itemString.Substring(31, 1);
                Item.UnitaMisura = itemString.Substring(32, 1);
                Item.Quantita = itemString.Substring(33, 10);


                Item.Sede = itemString.Substring(43, 5);
                Item.CodiceRaccolta = itemString.Substring(48, 3);
                Item.CodiceEccezione1 = itemString.Substring(51, 4).Trim();
                Item.CodiceEccezione2 = itemString.Substring(55, 4).Trim();
                Item.CodiceEccezione3 = itemString.Substring(59, 4).Trim();

                Item.CodiceOrario = itemString.Substring(63, 2).Trim();
                Item.StatoEccezione = itemString.Substring(65, 2).Trim();
            }
            catch (Exception ex)
            {
                Item.ParseError = ex.Message;
            }
            return Item;

        }
        private DateTime ToDateTime(DateTime D, int minuti)
        {
            int h = (int)minuti / 60;
            int min = minuti - (h * 60);
            return D.AddHours(h).AddMinutes(min);
        }
        private string ToHHMM(int minuti)
        {
            int h = (int)minuti / 60;
            int min = minuti - (h * 60);
            return h.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0');
        }
        public GetAnalisiEccezioniResponse GetAnalisiEccezioni(string matricola, DateTime DataStart, DateTime DataEnd, string eccezione1 = "", string eccezione2 = "", string eccezione3 = "")
        {
            Boolean ZeroPOH = eccezione1 == "POH" && PohPatchEnabled(DataStart.Year.ToString());


            GetAnalisiEccezioniResponse Response = new GetAnalisiEccezioniResponse() { Success = true };

            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();

            if ( eccezione1 == null )
                eccezione1 = "";
            if ( eccezione2 == null )
                eccezione2 = "";
            if ( eccezione3 == null )
                eccezione3 = "";

            string S = areaSistema("3TE", matricola, 75);
            matricola = matricola.PadLeft(7, '0');
            S += matricola + DataStart.ToString("ddMMyyyy") + DataEnd.ToString("ddMMyyyy") + eccezione1.PadRight(4, ' ') + eccezione2.PadRight(4, ' ') + eccezione3.PadRight(4, ' ');
            string RispostaCics = (string)c.ComunicaVersoCics(S.PadRight(8000));

            RispostaCics = RispostaCics.Replace(Convert.ToChar(0x0).ToString(), " ");
            Response.raw = RispostaCics;

            wApiUtility.cicsResponse cicsResp = wApiUtility.checkResponse(RispostaCics);

            if (cicsResp.esito == false)
            {
                Response.Error = cicsResp.codErrore + "-" + cicsResp.descErrore;
                Response.Success = false;
                return Response;
            }


            try
            {
                Response.DettagliEccezioni = new List<DateMinutiEccezione>();
                List<string> Giorni = Split(RispostaCics.Substring(306), 6).Where(a => a != "******").ToList();
                List<string> Leccezioni = new List<string>() { eccezione1, eccezione2, eccezione3 };
                int days = (DateTime.IsLeapYear(DataStart.Year) ? 366 : 365);
                for (int IndiceEccezione = 0; IndiceEccezione < 3; IndiceEccezione++)
                {
                    if ( String.IsNullOrWhiteSpace( Leccezioni[IndiceEccezione] ) )
                        continue;

                    int start = IndiceEccezione * (Giorni.Count / 3);
                    int end = start + (Giorni.Count / 3);
                    for (int i = start; i < end; i++)
                    {
                        if (Convert.ToInt32(Giorni[i]) > 0)
                        {
                            DateMinutiEccezione dm = new DateMinutiEccezione()
                            // Response.DettagliEccezioni.Add(new DateMinutiEccezione()
                            {
                                eccezione = Leccezioni[IndiceEccezione],
                                minuti = Convert.ToInt32(Giorni[i]),
                                data = new DateTime(DataStart.Year, 1, 1).AddDays(i % days)
                            };
                            if (ZeroPOH && (dm.eccezione.Trim() == "POH" || dm.eccezione.Trim() == "ROH"))
                            {
                                dm.minuti = 0;
                            }
                            Response.DettagliEccezioni.Add(dm);
                        }

                    }
                }


                //List<string> quat = (from Match m in Regex.Matches(AnnoCorrente, @"\d{4}")
                //                     select m.Value).ToList();
                RispostaCics = RispostaCics.Substring(142);
                //var eccezioni = Split(RispostaCics, 211);

                Response.Matricola = RispostaCics.Substring(0, 7);
                Response.Nominativo = RispostaCics.Substring(7, 30);
                //  Response.Datainiziorapporto = new DateTime(Convert.ToInt16(RispostaCics.Substring(40, 4)),Convert.ToInt16(RispostaCics.Substring(38, 2)),Convert.ToInt16(RispostaCics.Substring(36, 2)));
                //  Response.Datainiziorapporto = new DateTime(Convert.ToInt16(RispostaCics.Substring(48, 4)), Convert.ToInt16(RispostaCics.Substring(46, 2)), Convert.ToInt16(RispostaCics.Substring(44, 2)));
                Response.Tipodipendente = RispostaCics.Substring(53, 1);
                RispostaCics = RispostaCics.Substring(54);

                var eccezioni = Split(RispostaCics, 21);
                Response.AnalisiEccezione = new List<AnalisiEccezione>();
                int x = 0;
                foreach (string s in eccezioni)
                {
                    x++;

                    if ((x > 3) || (s.Substring(4, 1).Trim() == ""))
                        break;


                    AnalisiEccezione analisiappo = new AnalisiEccezione();
                    analisiappo.codice = s.Substring(0, 4);
                    analisiappo.unitamisura = s.Substring(4, 1);
                    analisiappo.totale = s.Substring(5, 5);
                    analisiappo.massimale = s.Substring(12, 5);
                    if (ZeroPOH && (analisiappo.codice.Trim() == "POH" || analisiappo.codice.Trim() == "ROH"))
                        analisiappo.totale = "00000";

                    Response.AnalisiEccezione.Add(analisiappo);

                }
            }
            catch (Exception ED)
            {

                Response.Error = ED.Message;
                Response.Success = false;
            }

            if (Response.AnalisiEccezione == null)
                Response.AnalisiEccezione = new List<AnalisiEccezione>();

            if (Response.AnalisiEccezione.Count > 0)
            {
                foreach (AnalisiEccezione a in Response.AnalisiEccezione)
                {
                    if (a.unitamisura.Trim().ToUpper() == "G")
                    {
                        float t = Convert.ToInt32(a.totale);
                        //a.totale = (t / (float)60).ToString();
                        a.totale = (t / (float)100).ToString();
                        foreach (DateMinutiEccezione ec in Response.DettagliEccezioni)
                        {
                            if (ec.eccezione.Trim() == a.codice.Trim())
                            {
                                //ec.giorni = ((float)ec.minuti / (float)60).ToString();
                                ec.giorni = ((float)ec.minuti / (float)100).ToString();
                                ec.minuti = 0;
                            }
                        }
                    }
                }
            }
            return Response;
        }

        public GetAnalisiEccezioniResponse GetAnalisiEccezioni2(string matricola, DateTime DataStart, DateTime DataEnd, string eccezione1 = "", string eccezione2 = "", string eccezione3 = "")
        {
            GetAnalisiEccezioniResponse Response = new GetAnalisiEccezioniResponse() { Success = true };

            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();

            if ( eccezione1 == null )
                eccezione1 = "";
            if ( eccezione2 == null )
                eccezione2 = "";
            if ( eccezione3 == null )
                eccezione3 = "";

            string S = areaSistema("3RE", matricola, 75);
            matricola = matricola.PadLeft(7, '0');
            S += matricola + DataStart.ToString("ddMMyyyy") + DataEnd.ToString("ddMMyyyy") + eccezione1.PadRight(4, ' ') + eccezione2.PadRight(4, ' ') + eccezione3.PadRight(4, ' ');
            string RispostaCics = (string)c.ComunicaVersoCics(S.PadRight(8000));

            RispostaCics = RispostaCics.Replace(Convert.ToChar(0x0).ToString(), " ");
            Response.raw = RispostaCics;

            wApiUtility.cicsResponse cicsResp = wApiUtility.checkResponse(RispostaCics);

            if (cicsResp.esito == false)
            {
                Response.Error = cicsResp.codErrore + "-" + cicsResp.descErrore;
                Response.Success = false;
                return Response;
            }


            try
            {
                Response.DettagliEccezioni = new List<DateMinutiEccezione>();
                List<string> Giorni = Split(RispostaCics.Substring(306), 6).Where(a => a != "******").ToList();
                List<string> Leccezioni = new List<string>() { eccezione1, eccezione2, eccezione3 };
                for (int IndiceEccezione = 0; IndiceEccezione < 3; IndiceEccezione++)
                {
                    if ( String.IsNullOrWhiteSpace( Leccezioni[IndiceEccezione] ) )
                        continue;

                    int start = IndiceEccezione * (Giorni.Count / 3);
                    int end = start + (Giorni.Count / 3);
                    for (int i = start; i < end; i++)
                    {
                        if (Convert.ToInt32(Giorni[i]) > 0)
                            Response.DettagliEccezioni.Add(new DateMinutiEccezione()
                            {
                                eccezione = Leccezioni[IndiceEccezione],
                                minuti = Convert.ToInt32(Giorni[i]),
                                data = new DateTime(DataStart.Year, 1, 1).AddDays(i % 365)
                            });
                    }
                }


                //List<string> quat = (from Match m in Regex.Matches(AnnoCorrente, @"\d{4}")
                //                     select m.Value).ToList();
                RispostaCics = RispostaCics.Substring(142);
                //var eccezioni = Split(RispostaCics, 211);

                Response.Matricola = RispostaCics.Substring(0, 7);
                Response.Nominativo = RispostaCics.Substring(7, 30);
                //  Response.Datainiziorapporto = new DateTime(Convert.ToInt16(RispostaCics.Substring(40, 4)),Convert.ToInt16(RispostaCics.Substring(38, 2)),Convert.ToInt16(RispostaCics.Substring(36, 2)));
                //  Response.Datainiziorapporto = new DateTime(Convert.ToInt16(RispostaCics.Substring(48, 4)), Convert.ToInt16(RispostaCics.Substring(46, 2)), Convert.ToInt16(RispostaCics.Substring(44, 2)));
                Response.Tipodipendente = RispostaCics.Substring(53, 1);
                RispostaCics = RispostaCics.Substring(54);

                var eccezioni = Split(RispostaCics, 21);
                Response.AnalisiEccezione = new List<AnalisiEccezione>();
                int x = 0;
                foreach (string s in eccezioni)
                {
                    x++;

                    if ((x > 3) || (s.Substring(4, 1).Trim() == ""))
                        break;


                    AnalisiEccezione analisiappo = new AnalisiEccezione();
                    analisiappo.codice = s.Substring(0, 4);
                    analisiappo.unitamisura = s.Substring(4, 1);
                    analisiappo.totale = s.Substring(5, 5);
                    analisiappo.massimale = s.Substring(12, 5);

                    Response.AnalisiEccezione.Add(analisiappo);

                }
            }
            catch (Exception ED)
            {

                Response.Error = ED.Message;
                Response.Success = false;
            }

            if (Response.AnalisiEccezione == null)
                Response.AnalisiEccezione = new List<AnalisiEccezione>();

            if (Response.AnalisiEccezione.Count > 0)
            {
                foreach (AnalisiEccezione a in Response.AnalisiEccezione)
                {
                    if (a.unitamisura.Trim().ToUpper() == "G")
                    {
                        float t = Convert.ToInt32(a.totale);
                        //a.totale = (t / (float)60).ToString();
                        a.totale = (t / (float)100).ToString();
                        foreach (var ec in Response.DettagliEccezioni)
                        {
                            if (ec.eccezione.Trim() == a.codice.Trim())
                            {
                                //ec.giorni = ((float)ec.minuti / (float)60).ToString();
                                ec.giorni = ((float)ec.minuti / (float)100).ToString();
                                ec.minuti = 0;
                            }
                        }
                    }
                }
            }
            return Response;
        }

        public GetDipendentiResponse getDipendentiPeriodo(string matricola, string sedegapp, DateTime DataStart, DateTime DataEnd)
        {
            GetDipendentiResponse Response = new GetDipendentiResponse() { Success = true };

            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
            string S = areaSistema("3PD", matricola, 75);
            S += sedegapp + DataStart.ToString("ddMMyyyy") + DataEnd.ToString("ddMMyyyy");
            string RispostaCics = (string)c.ComunicaVersoCics(S.PadRight(8000));

            RispostaCics = RispostaCics.Replace(Convert.ToChar(0x0).ToString(), " ");
            Response.raw = RispostaCics;

            wApiUtility.cicsResponse cicsResp = wApiUtility.checkResponse(RispostaCics);

            if (cicsResp.esito == false)
            {
                Response.Error = cicsResp.codErrore + "-" + cicsResp.descErrore;
                Response.Success = false;
                return Response;
            }

            List<DataDipendente> LD = new List<DataDipendente>();
            List<DataDipendente> LDpresenti = new List<DataDipendente>();

            RispostaCics = RispostaCics.Substring(142);
            var dipendenti = Split(RispostaCics, 211);// Regex.Split(RispostaCics, "(.{211})");

            TimeSpan DiffDate = DataEnd - DataStart;
            int DaysRequested = (int)DiffDate.TotalDays;

            var db = new myRaiData.digiGappEntities();
            var AmmesseTipo1 = db.MyRai_Eccezioni_Ammesse.Where(x => x.id_raggruppamento == 1).Select(x => x.cod_eccezione.Trim()).ToList();
            foreach (string s in dipendenti)
            {
                DataDipendente d = new DataDipendente();
                d.matricola = s.Substring(1, 6);
                d.nominativo = s.Substring(7, 25);

                List<DataEccezioni> LDecc = new List<DataEccezioni>();
                for (int i = 0; i <= DaysRequested; i++)
                {
                    int startEcc = 43 + (i * 12);
                    DataEccezioni de = new DataEccezioni();
                    de.data = DataStart.AddDays(i);
                    de.eccezioni = s.Substring(startEcc, 12)
                                    .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                                    .Where(e => AmmesseTipo1.Contains(e.Trim()))
                                    .ToArray();

                    if ( de.eccezioni.Count( ) > 0 )
                        LDecc.Add( de );
                }

                d.DettaglioDate = LDecc.ToArray();
                if ( LDecc.Count( ) > 0 )
                    LD.Add( d );
                else
                    LDpresenti.Add(d);

            }
            Response.DatiDipendenti = LD.ToArray();
            Response.DatiDipendentiPresenti = LDpresenti.ToArray();


            //ribaltali per data
            List<DataDip> LdataDip = new List<DataDip>();
            List<DataDip> LdataDipPresenti = new List<DataDip>();

            for (int i = 0; i <= DaysRequested; i++)
            {
                DateTime dateSearch = DataStart.AddDays(i);
                foreach (var item in Response.DatiDipendenti)
                {
                    foreach (var itemdata in item.DettaglioDate)
                    {
                        if (itemdata.data == dateSearch)
                        {
                            LdataDip.Add(new DataDip()
                            {
                                data = dateSearch,
                                matricola = item.matricola,
                                nominativo = item.nominativo,
                                eccezioni = itemdata.eccezioni
                            });
                        }
                    }
                }
            }
            for (int i = 0; i <= DaysRequested; i++)
            {
                DateTime dateSearch = DataStart.AddDays(i);
                foreach (var item in Response.DatiDipendentiPresenti)
                {
                    if (String.IsNullOrWhiteSpace(item.matricola)) continue;

                    // foreach (var itemdata in item.DettaglioDate)
                    //{
                    //  if (itemdata.data == dateSearch)
                    //  {
                    LdataDipPresenti.Add(new DataDip()
                    {
                        data = dateSearch,
                        matricola = item.matricola,
                        nominativo = item.nominativo,
                        // eccezioni = itemdata.eccezioni
                    });
                    // }
                    //}
                }
            }
            Response.datadip = LdataDip.ToArray();
            Response.datadipPresenti = LdataDipPresenti.ToArray();

            return Response;
        }

        static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        public wApiUtility.presenzeGiornaliere_resp presenzeGiornaliere(string matricola, string sedegapp, string data)
        {

            //attenzione al riciclo
            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();

            wApiUtility.presenzeGiornaliere_resp resp = new wApiUtility.presenzeGiornaliere_resp();

            string s2;

            string Matrichiama = "";

            string S = areaSistema("3TD", matricola, 75);

            matricola = matricola.PadLeft(7, '0');
            S += sedegapp + data;

            int ack = 12;
            int startIndex = ack + 130;
            int elementLenght = 1000;
            int dayLenght = 23;
            int giorni = 7;
            int idx = 11;
            bool richiama = false;
            int innerLenght = dayLenght * giorni;


            CultureInfo culture = new CultureInfo("it-IT");

            do
            {



                try
                {

                    s2 = (string)c.ComunicaVersoCics(S.PadRight(8000));
                    //Cancella il carattere Nullo che manda a put..ane la lettura della risposta del cics
                    //viene usato per sicurezza
                    s2 = s2.Replace(Convert.ToChar(0x0).ToString(), " ");

                    wApiUtility.cicsResponse cicsResp = wApiUtility.checkResponse(s2);

                    resp.esito = cicsResp.esito;

                    richiama = cicsResp.richiama;

                    string codErrore = cicsResp.codErrore;
                    resp.errore = cicsResp.descErrore;
                    string dati = s2;
                    if (resp.dati==null)
                        resp.dati = new List<wApiUtility.presentiGiornaliere>();

                    if (resp.esito)
                    {


                        int firstElementIndex = ack + 130; // valore fisso formato da 80 di area di sistema e 50 di chiave da ricerca

                        int lunghezza = cicsResp.lunghezza;// int.Parse(s2.Substring(12 + 27, 5));



                        int LUNG = dati.Substring(firstElementIndex).Length;
                        dati.Substring(firstElementIndex).PadRight(lunghezza, ' ');


                        idx = idx + 130;
                        int COMPONENTI = Convert.ToInt32(Math.Ceiling((decimal)(LUNG / 80)));
                        var db = new digiGappEntities( );

                        for (int mese = 1; mese <= 88; mese++)
                        {

                            if (idx >= dati.Length)
                                break;
                            if (dati.Substring(idx + 1, 7).Trim() == "")
                                break;
                            if (dati.Substring(idx).Length < 80)
                                dati = dati + "".PadRight((80 - dati.Substring(idx).Length), ' ');

                            resp.dati.Add(new wApiUtility.presentiGiornaliere
                            {
                                matricola = dati.Substring(idx + 1, 7),
                                nominativo = dati.Substring(idx + 8, 25),
                                tipoDip = dati.Substring(idx + 33, 1),
                                codiceOrario = dati.Substring(idx + 34, 2),
                                formaContratto = dati.Substring(idx + 36, 1),
                                CodiceReparto = dati.Substring(idx + 37, 2),
                                CodTeminalePrimaEntrata = dati.Substring(idx + 39, 4),
                                OrarioPrimaEntrata = dati.Substring(idx + 43, 4),
                                CodTeminaleUltimaEntrata = dati.Substring(idx + 47, 4),
                                OrarioUltimaEntrata = dati.Substring(idx + 51, 4),
                                CodTeminaleUltimaUscita = dati.Substring(idx + 55, 4),
                                OrarioUltimaUscita = dati.Substring(idx + 59, 4),
                                EccezioneUno = dati.Substring(idx + 65, 4),
                                EccezioneDue = dati.Substring(idx + 69, 4),
                                EccezioneTre = dati.Substring( idx + 73 , 4 ) ,
                                DefinizioniOrario = db.L2D_ORARIO.Where( x => x.cod_orario == dati.Substring( idx + 34 , 2 ) ).FirstOrDefault( )

                            });

                            idx = idx + 80;
                        }

                        if (richiama)
                        {
                            S =  areaSistema("3TD", matricola, 75, true);
                            S += sedegapp + data;
                            S += s2.Substring(105,7);
                            idx = 11;
                        }
                    }
                    else
                    {
                        resp.esito = false;
                        resp.errore = "";
                    }
                }
                catch (Exception d)
                {
                    resp.esito = false;
                    resp.errore = d.Message;
                }
            } while (richiama);


            return resp;

        }

        //Riceve in input un orario in formato minuti e
        // restituisce la stringa in formato HH:MM
        public static string calcolaOrario(int minuti)
        {

            try
            {
                string orarioHHMM = "";
                int oraInt = 0;
                string min = "00";
                string ora = "00";

                string segno = "";

                if ( minuti < 0 )
                    segno = "-";


                minuti = Math.Abs(minuti);


                while (minuti - 60 >= 0)
                {
                    minuti -= 60;
                    oraInt++;
                }

                ora = oraInt.ToString();
                min = minuti.ToString();

                if (oraInt < 10)
                    ora = "0" + oraInt.ToString();

                if (minuti < 10)
                    min = "0" + minuti.ToString();

                orarioHHMM = ora + ":" + min;

                return segno + " " + orarioHHMM;
            }
            catch
            {
                return minuti.ToString();
            }
        }

        public getOrarioResponse getOrario(string codiceOrario, string data, string matricola, string options, int livelloUtente)
        {
            var db = new digiGappEntities();
            string funzione = CachedMethods.getOrario.ToString();
            CleanCache(codiceOrario, funzione);

            var cachedData = db.MyRai_CacheFunzioni.Where(x => x.oggetto == codiceOrario && x.funzione == funzione).FirstOrDefault();
            if (cachedData != null && cachedData.dati_serial != null && cachedData.dati_serial.Trim() != "")
            {
                try
                {
                    getOrarioResponse DBdata = Newtonsoft.Json.JsonConvert.DeserializeObject<getOrarioResponse>(cachedData.dati_serial);
                    return DBdata;
                }
                catch (Exception ex)
                {

                }
            }

            getOrarioResponse resp = new getOrarioResponse();
            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();

            wApiUtility.cicsResponse cicsResp = new wApiUtility.cicsResponse();

            string S = areaSistema("3OR", matricola, livelloUtente);
            codiceOrario = codiceOrario.PadRight(2, ' ');

            S += codiceOrario + data;
            string s2 = (string)c.ComunicaVersoCics(S.PadRight(8000));
            s2 = s2.Replace(Convert.ToChar(0x0).ToString(), " ");
            cicsResp = wApiUtility.checkResponse(s2);

            resp.errore = cicsResp.descErrore;
            resp.esito = cicsResp.esito;

            if (cicsResp.esito)
            {
                string dato = s2.Substring(142);

                int pointer = 0;
                for (int i = 0; i < resp.map.Count; i += 2)
                {
                    string pr = resp.map[i];
                    int length = Convert.ToInt32(resp.map[i + 1]);
                    string value = dato.Substring(pointer, length);
                    pointer += length;

                    PropertyInfo prop = resp.GetType().GetProperty(pr);
                    prop.SetValue(resp, value, null);
                    if (pr.EndsWith("Min"))
                    {
                        prop = resp.GetType().GetProperty(pr.Substring(0, pr.Length - 3));
                        prop.SetValue(resp, calcolaOrario(Convert.ToInt32(value)), null);
                    }
                }
            }

            MyRai_CacheFunzioni newDataCache = new MyRai_CacheFunzioni()
            {
                data_creazione = DateTime.Now,
                dati_serial = Newtonsoft.Json.JsonConvert.SerializeObject(resp),
                funzione = funzione,
                oggetto = codiceOrario
            };
            db.MyRai_CacheFunzioni.Add(newDataCache);
            db.SaveChanges();

            return resp;
        }

        public wApiUtility.presenzeSettimanali_resp PresenzeSettimanali(string matricola, string dataDa, string dataA)
        {
            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();

            wApiUtility.presenzeSettimanali_resp resp = new wApiUtility.presenzeSettimanali_resp();

            string s2;
            string S = areaSistema("3CM", matricola, 75);

            matricola = matricola.PadLeft(7, '0');
            S += matricola + dataDa + dataA;
            wApiUtility.cicsResponse cicsResp = new wApiUtility.cicsResponse();
            int ack = 12;
            int startIndex = ack + 130;
            int elementLenght = 1000;
            int dayLenght = 23;
            int giorni = 7;
            int innerLenght = dayLenght * giorni;
            CultureInfo culture = new CultureInfo("it-IT");

            do
            {
                s2 = (string)c.ComunicaVersoCics(S.PadRight(8000));
                s2 = s2.Replace(Convert.ToChar(0x0).ToString(), " ");
                cicsResp = wApiUtility.checkResponse(s2);
                resp.errore = cicsResp.descErrore;
                resp.dati.dataDa = dataDa;
                resp.dati.dataA = dataA;
                resp.esito = cicsResp.esito;
                resp.rispostaCics = s2;

                List<string> codiciOrario = new List<string>();
                if (cicsResp.esito)
                {
                    try
                    {
                        DateTime firstDay = new DateTime();
                        firstDay = DateTime.ParseExact(dataDa, "ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture);
                        DateTime lastDay = new DateTime();
                        lastDay = DateTime.ParseExact(dataA, "ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture);

                        int dayCount = 0;
                        resp.dati.items = new List<wApiUtility.presenzeSettimanali_Item>();

                        int carenza = 0;
                        int maggiorPresenza = 0;
                        int deltaCarenze = 0;
                        int deltaMaggioriPresenze = 0;

                        for (int j = startIndex + 33; j < 1000; j += dayLenght)
                        {
                            wApiUtility.presenzeSettimanali_Item giorno = new wApiUtility.presenzeSettimanali_Item();
                            // giorno.dipendente = periodo.dipendente;

                            giorno.data = firstDay.AddDays(dayCount);
                            if (giorno.data > lastDay)
                                break;


                            giorno.giornoSettimana = culture.DateTimeFormat.GetDayName(giorno.data.DayOfWeek);

                            giorno.orarioReale = s2.Substring(j, 2);
                            //  giorno.MacroAssenza1 = wApiUtility.getDescrizioneEccezione(s2.Substring(j + 2, 3));
                            //  giorno.MacroAssenza2 = wApiUtility.getDescrizioneEccezione(s2.Substring(j + 5, 3));
                            //  giorno.MacroAssenza3 = wApiUtility.getDescrizioneEccezione(s2.Substring(j + 8, 3));
                            giorno.MacroAssenza1 = s2.Substring(j + 2, 3);
                            giorno.MacroAssenza2 = s2.Substring(j + 5, 3);
                            giorno.MacroAssenza3 = s2.Substring(j + 8, 3);

                            if (giorno.orarioReale.Trim() != "")
                            {
                                string codice = codiciOrario.Where(a => a == giorno.orarioReale).FirstOrDefault();
                                if (codice != null)
                                {
                                    giorno.descOrarioReale = codice;
                                }
                                else
                                {
                                    codice = wApiUtility.getDescrizionecodOrario(giorno.orarioReale);
                                    codiciOrario.Add(codice);
                                    giorno.descOrarioReale = codice;
                                }

                                int codiint = 0;
                                if (Int32.TryParse(giorno.orarioReale.Trim(), out codiint))
                                    if (((codiint > 89) && (codiint > 99))
                                        || (codiint == 83) || (codiint == 87))
                                        giorno.MacroAssenza1 = giorno.descOrarioReale;
                            }
                            else
                            {

                                if ((giorno.MacroAssenza1 + giorno.MacroAssenza2) == " N.P. ")
                                {
                                    giorno.MacroAssenza1 = "NON PIANIFICATO";
                                    giorno.MacroAssenza2 = "";
                                }
                            }

                            maggiorPresenza = int.Parse(s2.Substring(j + 11, 4));
                            giorno.maggiorPresenza = wApiUtility.calcolaOrario(maggiorPresenza);

                            carenza = int.Parse(s2.Substring(j + 15, 4));
                            giorno.carenza = wApiUtility.calcolaOrario(carenza);

                            deltaCarenze -= carenza;
                            deltaMaggioriPresenze += maggiorPresenza;

                            string temp = s2.Substring(j + 19, 1);

                            if (temp == "A" || temp == "B")
                            {
                                giorno.assenzaIngiustificata = (temp == "A") ? true : false;
                                giorno.transitoSfasato = (temp == "B") ? true : false;
                            }

                            resp.dati.items.Add(giorno);

                            dayCount++;
                        }

                        resp.dati.deltaCarenze = deltaCarenze < 0 ? wApiUtility.calcolaOrario(deltaCarenze) : "-" + wApiUtility.calcolaOrario(deltaCarenze);
                        resp.dati.deltaMaggioriPresenze = "+" + wApiUtility.calcolaOrario(deltaMaggioriPresenze);
                        resp.dati.deltaTotale = (deltaMaggioriPresenze + deltaCarenze) < 0 ? wApiUtility.calcolaOrario(deltaMaggioriPresenze + deltaCarenze) : "+" + wApiUtility.calcolaOrario(deltaMaggioriPresenze + deltaCarenze);
                    }
                    catch (Exception e)
                    {
                        resp.esito = false;
                        resp.errore = "Errore Parsing" + " - " + e.Message + " ----- " + e.InnerException + " ----- " + e.StackTrace;
                    }
                }

                //per sicurezza
                else
                    cicsResp.richiama = false;

            } while (cicsResp.richiama);
            return resp;
        }

        public string ComunicaCICS(string S)
        {
            try
            {
                ScriviLogIn(S);
            }
            catch (Exception ex)
            {

            }

            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
            string result = (string)c.ComunicaVersoCics(S);
            try
            {
                ScriviLogOut(result);
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private void ScriviLogIn(string msg)
        {
            if (!String.IsNullOrEmpty(msg))
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    db.MyRai_LogAzioni.Add(new MyRai_LogAzioni()
                    {
                        matricola = "FRANCESCO",
                        data = DateTime.Now,
                        operazione = "LOGCOMUNICACICS_IN",
                        applicativo = "Portale",
                        descrizione_operazione = msg,
                        provenienza = "MyRaiService"
                    });
                    db.SaveChanges();
                }
            }
        }

        private void ScriviLogOut(string msg)
        {
            if (!String.IsNullOrEmpty(msg))
            {
                string toWrite = msg.Replace(Convert.ToChar(0x0).ToString(), " ");

                using (digiGappEntities db = new digiGappEntities())
                {
                    db.MyRai_LogAzioni.Add(new MyRai_LogAzioni()
                    {
                        matricola = "FRANCESCO",
                        data = DateTime.Now,
                        operazione = "LOGCOMUNICACICS_OUT",
                        applicativo = "Portale",
                        descrizione_operazione = toWrite,
                        provenienza = "MyRaiService"
                    });
                    db.SaveChanges();
                }
            }
        }

        public void CleanCache(string oggetto, string funzione)
        {
            DateTime D = DateTime.Now.Date;
            var db = new digiGappEntities();
            var oldData = db.MyRai_CacheFunzioni.Where(x => x.funzione == funzione && x.data_creazione < D).ToList();
            if (oldData.Any())
            {
                foreach ( var item in oldData )
                { db.MyRai_CacheFunzioni.Remove( item ); }

                db.SaveChanges();
            }
        }

        public void LogChiamata(string nomeFunzione, string matricola, string RawString, Boolean IsInviata)
        {
            try
            {
                

                var db = new digiGappEntities();
                LOG logRow = new LOG()
                {
                    Autore = nomeFunzione,
                    Livello = 1,
                    Timestamp = DateTime.Now,
                    Messaggio = (IsInviata ? "[I]" : "[R]") + " Utente " + matricola + " RAW:" + RawString
                };
                db.LOG.Add(logRow);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }

        public List<myRaiDataTalentia.XR_STATO_RAPPORTO> SmartWorkerTimes(string matricola)
        {
            myRaiDataTalentia.TalentiaEntities db = new myRaiDataTalentia.TalentiaEntities();
            List<myRaiDataTalentia.XR_STATO_RAPPORTO> Periods = new List<myRaiDataTalentia.XR_STATO_RAPPORTO>();

            //var item = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matricola).FirstOrDefault();
            //if (item == null) return Periods;

            //return db.XR_STATO_RAPPORTO.Where(x => x.COD_STATO_RAPPORTO == "SW" && x.ID_PERSONA == item.ID_PERSONA).ToList();
            return db.XR_STATO_RAPPORTO.Where(x => x.COD_STATO_RAPPORTO == "SW" && x.MATRICOLA == matricola).ToList();
        }
        public bool IsSmartWorker(string matricola, DateTime D)
        {
            var periodi = SmartWorkerTimes(matricola);
            return periodi.Any(x => x.DTA_INIZIO <= D.Date && x.DTA_FINE >= D.Date);
        }

        public List<PeriodoSW> GetPeriodiSW(string matricola)
        {
            List<myRaiDataTalentia.XR_STATO_RAPPORTO> Periods = SmartWorkerTimes(matricola);
            List<PeriodoSW> L = new List<PeriodoSW>();
            foreach (var p in Periods)
            {
                L.Add(new PeriodoSW() { DataInizio = p.DTA_INIZIO, DataFine = p.DTA_FINE });
            }
            return L;
        }
        /// <summary>
        /// Funzione che restituisce i dati anagrafici di un dipendente (record anagrafico dellla base dati 4000 del GAPP)
        /// </summary>
        /// <param name="matricola">Matricola da recuperare</param>
        /// <param name="data">Data relativa all'anagrafico in vigore</param>
        /// <returns></returns>
        public wApiUtility.dipendente_resp recuperaUtente(string matricola, string data)
        {
            var db = new digiGappEntities();
            string funzione = CachedMethods.recuperaUtente.ToString();
            string m7 = matricola.PadLeft(7, '0');
            data = data.Replace("/", "");

            // CleanCache(m7, funzione);

            DateTime Dtoday = DateTime.Now.Date;

            var cachedData = db.MyRai_CacheFunzioni.Where(x => x.oggetto == m7 && x.funzione == funzione).FirstOrDefault();
            cachedData = null;

            if (cachedData != null && cachedData.data_creazione > Dtoday && cachedData.dati_serial != null && cachedData.dati_serial.Trim() != "")
            {
                try
                {
                    wApiUtility.dipendente_resp DBdata = Newtonsoft.Json.JsonConvert.DeserializeObject<wApiUtility.dipendente_resp>(cachedData.dati_serial);
                    return DBdata;
                }
                catch (Exception ex)
                {

                }
            }

            bool chiuso;
            if (!wApiUtility.utenteAutorizzato())
                chiuso = false;

            string S = areaSistema("3AD", matricola, 70);// "P956,3XX,3VGGAPWEB,";

            matricola = matricola.PadLeft(7, '0');
            string s2;
            S += matricola + data;
            wApiUtility.cicsResponse cicsResp = new wApiUtility.cicsResponse();
            int ack = 12;
            int startIndex = ack + 130;
            int elementLenght = 120;
            int idx = 11;
            int giorni = 7;

            CultureInfo culture = new CultureInfo("it-IT");

            LogChiamata("recuperaUtente", matricola, S, true);
            s2 = ComunicaCICS(S.PadRight(8000));
            s2 = s2.Replace(Convert.ToChar(0x0).ToString(), " ");
            LogChiamata("recuperaUtente", matricola, s2, false);


            cicsResp = wApiUtility.checkResponse(s2);

            wApiUtility.dipendente_resp resp;
            if (s2.Length > 6)
            {
                Utente dipendente = new Utente();

                dipendente.matricola = matricola;

                string dati = s2;

                try
                {
                    string esito = dati.Substring(45, 2);

                    if ((esito != "KO"))
                    {
                        dipendente.nominativo = dati.Substring(idx + 138, 25);
                        dipendente.data_nascita = dati.Substring(idx + 163, 8);
                        dipendente.data_inizio_rapporto_lavorativo = dati.Substring(idx + 171, 8);
                        dipendente.data_fine_rapporto_lavorativo = dati.Substring(idx + 179, 8);
                        dipendente.data_inizio_validita = dati.Substring(idx + 187, 8);
                        dipendente.data_fine_validita = dati.Substring(idx + 196, 8);
                        dipendente.sede_gapp = dati.Substring(idx + 203, 5);
                        dipendente.tipo_dipendente = dati.Substring(idx + 208, 1);
                        dipendente.categoria = dati.Substring(idx + 209, 3);
                        dipendente.mansione = dati.Substring(idx + 212, 4);
                        dipendente.tipo_mansione = dati.Substring(idx + 216, 1);
                        dipendente.tipo_minimo = dati.Substring(idx + 217, 1);
                        dipendente.forma_contratto = dati.Substring(idx + 218, 1);
                        dipendente.grado = dati.Substring(idx + 219, 3);
                        dipendente.codice_inail = dati.Substring(idx + 235, 10);
                        dipendente.qualifica = dati.Substring(idx + 245, 1);
                        dipendente.indennita_113 = (dati.Substring(idx + 279, 1) == "1") ? true : false;
                        dipendente.indennita_106 = (dati.Substring(idx + 280, 1) == "1") ? true : false;
                        dipendente.indennita_107 = (dati.Substring(idx + 281, 1) == "1") ? true : false;
                        dipendente.indennita_SIRIO = (dati.Substring(idx + 282, 1) == "1") ? true : false;
                        dipendente.indennita_ex_33_perc = (dati.Substring(idx + 283, 1) == "1") ? true : false;
                        dipendente.indennita_11B = (dati.Substring(idx + 284, 1) == "1") ? true : false;

                        dipendente.forfait_straordinari = (dati.Substring(idx + 315, 1) == "1") ? true : false;
                        dipendente.indennita_lavoro_notturno_124 = (dati.Substring(idx + 316, 1) == "1") ? true : false;
                        dipendente.di_produzione = (dati.Substring(idx + 317, 1) == "1") ? true : false;
                        dipendente.indennita_turni_avvicendati_153 = (dati.Substring(idx + 318, 1) == "1") ? true : false;
                        dipendente.indennita_mancata_limitazione_oraria_11c = (dati.Substring(idx + 319, 1) == "1") ? true : false;
                        dipendente.ex_art12_in_equipe_ex_33_perc = (dati.Substring(idx + 320, 1) == "1") ? true : false;
                        dipendente.settimana_corta = (dati.Substring(idx + 321, 1) == "1") ? true : false;
                        dipendente.gestito_SIRIO = (dati.Substring(idx + 322, 1) == "1") ? true : false;
                        dipendente.part_time = (dati.Substring(idx + 323, 1) == "1") ? true : false;
                        dipendente.num_giorni_part_time = dati.Substring(idx + 324, 1);
                        dipendente.indennita_35perc_o_40perc = (dati.Substring(idx + 325, 1) == "1") ? true : false;
                        dipendente.diritto_reperibilita = (dati.Substring(idx + 326, 1) == "1") ? true : false;
                        dipendente.forfait_straordinari_TD_95_euro = (dati.Substring(idx + 327, 1) == "1") ? true : false;
                        dipendente.giornalisti_delle_reti = (dati.Substring(idx + 328, 1) == "1") ? true : false;
                        dipendente.part_time_stagionale = (dati.Substring(idx + 329, 1) == "1") ? true : false;
                        dipendente.videoterminalista = (dati.Substring(idx + 330, 1) == "1") ? true : false;
                        dipendente.squadra_pronto_intervento = (dati.Substring(idx + 331, 1) == "1") ? true : false;
                        dipendente.soggetto_a_visite_periodiche = (dati.Substring(idx + 332, 1) == "1") ? true : false;
                        dipendente.timbrature_visibili = (dati.Substring(idx + 333, 1) == "1") ? true : false;
                        dipendente.domanda_qualifica = (dati.Substring(idx + 334, 1) == "1") ? true : false;
                        dipendente.utilizza_tecnologie_leggere_rip_mont_tras = (dati.Substring(idx + 335, 1) == "1") ? true : false;
                        dipendente.ElementoRisposta = (dati.Substring(395, 1) == "1") ? true : false;

                        string dataAnzianita = dati.Substring(396, 8);

                        try
                        {
                            int YY = int.Parse(dataAnzianita.Substring(0, 4));
                            int MM = int.Parse(dataAnzianita.Substring(4, 2));
                            int DD = int.Parse(dataAnzianita.Substring(6, 2));

                            dipendente.DataAnzianitaFerie = new DateTime(YY, MM, DD);
                        }
                        catch
                        {
                            dipendente.DataAnzianitaFerie = null;
                        }

                        dipendente.CodiceReparto = dati.Substring(404, 2);
                        dipendente.CodiceRaggruppamentoSettore = dati.Substring(406, 2);

                        try
                        {
                            dipendente.ProgressivoBadge = Int32.Parse(dati.Substring(408, 2));
                        }
                        catch
                        {
                            dipendente.ProgressivoBadge = 0;
                        }

                        string dataRilascioBadge = dati.Substring(410, 8);
                        try
                        {
                            int YY2 = int.Parse(dataRilascioBadge.Substring(0, 4));
                            int MM2 = int.Parse(dataRilascioBadge.Substring(4, 2));
                            int DD2 = int.Parse(dataRilascioBadge.Substring(6, 2));

                            dipendente.DataRilascioBadge = new DateTime(YY2, MM2, DD2);
                        }
                        catch
                        {
                            dipendente.DataRilascioBadge = null;
                        }

                        dipendente.Cognome = dati.Substring(418, 25);
                        dipendente.Nome = dati.Substring(443, 18);
                        dipendente.nominativo = dati.Substring(461, 30);

                        DateTime D;
                        if (DateTime.TryParseExact(data, "ddMMyyyy", null, DateTimeStyles.None, out D))
                        {
                            dipendente.SmartWorkerAllaData = IsSmartWorker(matricola.Substring(1), D);
                            dipendente.SmartWorkerGenerico = SmartWorkerTimes(matricola.Substring(1)).Any();
                        }

                        resp.data = dipendente;
                        resp.esito = true;
                        resp.errore = "";
                    }
                    else
                    {
                        string codiceerrore = dati.Substring(47, 4);
                        string descerrore = dati.Substring(51, 25);
                        resp.data = null;
                        resp.esito = false;
                        resp.errore = codiceerrore + " - " + descerrore;
                    }
                }
                catch (Exception e)
                {
                    resp.esito = false;
                    resp.errore = e.Message + " --- " + e.StackTrace;
                    resp.data = null;
                }


                if (cachedData == null)
                {
                MyRai_CacheFunzioni c = new MyRai_CacheFunzioni()
                {
                    data_creazione = DateTime.Now,
                    dati_serial = Newtonsoft.Json.JsonConvert.SerializeObject(resp),
                    funzione = funzione,
                    oggetto = (matricola.Length == 7 && matricola.StartsWith("0") ) ? matricola : matricola.Length <= 6 ? matricola : matricola.Substring(matricola.Length - 6)
                };
                db.MyRai_CacheFunzioni.Add(c);
                db.SaveChanges();
                }
                else
                {
                    var item = db.MyRai_CacheFunzioni.Where(x => x.id == cachedData.id).FirstOrDefault();
                    item.data_creazione = DateTime.Now;
                    item.dati_serial = Newtonsoft.Json.JsonConvert.SerializeObject(resp);
                    db.SaveChanges();
                }


                return resp;
            }

            else
            {
                string errore = wApiUtility.comti_decResponse(s2.Substring(3));
                resp.esito = false;
                resp.errore = errore;
                resp.data = null;

                return resp;
            }
        }

        private T CopyObject<T>(object sourceObject)
        {

            if ( sourceObject == null )
                return default( T );

            Type sourceType = sourceObject.GetType();
            Type targetType = typeof(T);
            T destObject = (T)Activator.CreateInstance(typeof(T));

            foreach (PropertyInfo p in sourceType.GetProperties())
            {
                PropertyInfo targetObj = targetType.GetProperty(p.Name);

                if ( targetObj == null )
                    continue;

                targetObj.SetValue(destObject, p.GetValue(sourceObject, null), null);
            }
            return destObject;
        }

        private string areaSistema(string funzione, string matricola, int livelloUtente, bool richiama = false)
        {
            string flg_chiamata = richiama ? "S" : " ";

            return "P956,3XX," + funzione + "GAPWEB" + matricola.PadLeft(10, '0') + DateTime.Now.ToString("ddMMyyyy") + "".PadRight(5, '0') + flg_chiamata + "".PadRight(6, ' ') + "".PadRight(25, 'a') + livelloUtente.ToString() + DateTime.Now.ToString("HH:mm:ss").PadLeft(14, ' ') + ",";
        }

        public SituazioneDebitoriaResponse GetSituazioneDebitoria(string matricolaRichiedente, string matricolaRichiesta)
        {
            SituazioneDebitoriaResponse response = new SituazioneDebitoriaResponse();
            try
            {
                List<SituazioneDebitoria> toReturn = new List<SituazioneDebitoria>();

                ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
                //string matricola = "P103650";
                string matricola = "P" + matricolaRichiesta;

                string sUserName = matricola + new string(' ', 10 - matricola.Length);

                string strChr = "00000";
                //string sUserNameRequester = "103650";
                string sUserNameRequester = matricolaRichiedente;
                string inputData = "P956,DT,HRPESS" + sUserName + "0" + strChr + sUserNameRequester + "," + ",";
                inputData = inputData + new string(' ', 2000);

                var serviceResponse = c.ComunicaVersoCics(inputData);
                response.Risposta = serviceResponse.ToString();

                string strFunzione = serviceResponse.ToString();

                string strAppoFunzione = "";
                string strACK = strFunzione.Substring(0, 10);
                int iSpazio = (Convert.ToInt16(strFunzione.Substring(5, 5)) + 10) - strFunzione.Length;
                if (iSpazio > 0)
                {
                    strAppoFunzione = (strAppoFunzione + strFunzione.Substring(10)).PadRight(iSpazio);
                }
                else
                {
                    strAppoFunzione = (strAppoFunzione + strFunzione.Substring(10, Convert.ToInt16(strACK.Substring(5, 5))));
                }

                string strFunzioneDeb = String.Empty;
                int iNum = 0;

                if (strAppoFunzione.Length >= 360)
                {
                    strFunzioneDeb = strAppoFunzione.Substring(360);
                    iNum = strFunzioneDeb.Length / 40;
                }
                else
                {
                    // No data
                    strFunzioneDeb = strAppoFunzione;
                }

                string appoanno = "", appomese = "";
                strFunzioneDeb = strFunzioneDeb + " ";

                for (int i = 1; i <= iNum; i++)
                {
                    SituazioneDebitoria appoDebito = new SituazioneDebitoria();
                    appoDebito.Descrizione = strFunzioneDeb.Substring(0, 20);

                    string ad = strFunzioneDeb.Substring(23, 11);

                    if (ad.Trim() == "")
                        ad = "0,00";

                    ad = ad.Replace(".", "");
                    ad = ad.Replace(",", ".");

                    appoDebito.Addebito = double.Parse(ad, CultureInfo.InvariantCulture.NumberFormat);

                    if ((strFunzioneDeb.Substring(36, 2) != "00") || (strFunzioneDeb.Substring(36, 2) != ""))
                    {
                        appomese = strFunzioneDeb.Substring(36, 2);
                    }

                    if (Convert.ToInt16(strFunzioneDeb.Substring(39, 2)) > 40)
                    {
                        appoanno = "19" + strFunzioneDeb.Substring(39, 2);
                    }
                    else
                    {
                        appoanno = "20" + strFunzioneDeb.Substring(39, 2);
                    }

                    int mese = Int32.Parse(appomese);

                    appoDebito.MeseDa = "";

                    if (mese != 0)
                        appoDebito.MeseDa = ((MesiEnum)mese).GetAmbientValue() + " " + appoanno;

                    appoDebito.IntMeseDa = mese;

                    if (mese != 0)
                        appoDebito.AnnoDa = int.Parse(appoanno);

                    if ((strFunzioneDeb.Substring(43, 2) != "00") || (strFunzioneDeb.Substring(43, 2) != ""))
                    {
                        appomese = strFunzioneDeb.Substring(43, 2);
                    }

                    if (Convert.ToInt16(strFunzioneDeb.Substring(46, 2)) > 40)
                    {
                        appoanno = "19" + strFunzioneDeb.Substring(46, 2);
                    }
                    else
                    {
                        appoanno = "20" + strFunzioneDeb.Substring(46, 2);
                    }

                    mese = Int32.Parse(appomese);

                    if (mese != 0)
                        appoDebito.MeseA = ((MesiEnum)mese).GetAmbientValue() + " " + appoanno;

                    appoDebito.IntMeseA = mese;
                    appoDebito.AnnoA = int.Parse(appoanno);

                    string numRate = strFunzioneDeb.Substring(50, 3);

                    if (numRate.Trim() != "")
                        appoDebito.NumeroRate = Int32.Parse(strFunzioneDeb.Substring(50, 3));

                    string iRata = strFunzioneDeb.Substring(54, 10);

                    if (iRata.Trim() == "")
                        iRata = "0,00";

                    iRata = iRata.Replace(".", "");
                    iRata = iRata.Replace(",", ".");

                    appoDebito.ImportoRata = double.Parse(iRata, CultureInfo.InvariantCulture.NumberFormat);
                    if (strFunzioneDeb.Length > 67)
                    {
                        string ImportoRate = strFunzioneDeb.Substring(66, 3);

                        string numRateResidue = strFunzioneDeb.Substring(66, 3);

                        if (numRateResidue.Trim() != "")
                            appoDebito.NumeroRateResidue = Int32.Parse(strFunzioneDeb.Substring(66, 3));

                        string iRataRes = strFunzioneDeb.Substring(69, 11);
                        if (iRataRes.Trim() == "")
                            iRataRes = "0,00";
                        iRataRes = iRataRes.Replace(".", "");
                        iRataRes = iRataRes.Replace(",", ".");

                        appoDebito.ImportoRateResidue = double.Parse(iRataRes, CultureInfo.InvariantCulture.NumberFormat);
                    }
                    toReturn.Add(appoDebito);
                    if (strFunzioneDeb.Length > 90)
                    {
                        strFunzioneDeb = strFunzioneDeb.Substring(80);
                    }
                    else
                    {
                        break;
                    }
                }

                response.List = toReturn;
                response.Errore = String.Empty;
                response.Esito = true;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.List = new List<SituazioneDebitoria>();
            }

            return response;
        }

        public ResocontiResponse ClearCacheResoconti(List<SedeData> SediData)
        {
            if (SediData == null || !SediData.Any())
                return new ResocontiResponse() { SediData = SediData };


            var db = new digiGappEntities();
            foreach (var item in SediData)
            {
                if (String.IsNullOrWhiteSpace(item.Sede))
                {
                    item.esito = false;
                    item.error = "Sede non indicata";
                    continue;
                }
                if (item.DataInizio > DateTime.Now)
                {
                    item.esito = false;
                    item.error = "Data inizio non corretta";
                    continue;
                }
                if (!db.MyRai_Resoconti_GetPresenze.Any(x => x.sede == item.Sede && x.data_inizio == item.DataInizio))
                {
                    item.esito = false;
                    item.error = "Dati da aggiornare non presenti";
                    continue;
                }
                var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.tipologia_pdf == "P" && x.sede_gapp == item.Sede && x.data_inizio == item.DataInizio)
                    .FirstOrDefault();
                if (pdf != null)
                {
                    item.esito = false;
                    item.error = "PDF già generato";
                    continue;
                }
                var row = db.MyRai_Resoconti_GetPresenze.Where(x => x.sede == item.Sede && x.data_inizio == item.DataInizio).FirstOrDefault();
                if (row != null)
                {
                    try
                    {
                        db.MyRai_Resoconti_GetPresenze.Remove(row);
                        db.SaveChanges();
                        item.esito = true;
                    }
                    catch (Exception ex)
                    {
                        item.esito = false;
                        item.error = ex.Message;
                    }
                }
                else
                {
                    item.esito = false;
                    item.error = "Dati in cache non presenti";
                }
            }
            return new ResocontiResponse() { SediData = SediData };
        }

        public ResocontiResponse RiCreaResoconti(List<SedeData> SediData)
        {
            if (SediData == null || !SediData.Any())
                return new ResocontiResponse() { SediData = SediData };

            var db = new digiGappEntities();

            foreach (var item in SediData)
            {
                if (String.IsNullOrWhiteSpace(item.Sede))
                {
                    item.esito = false;
                    item.error = "Sede non indicata";
                    continue;
                }

                L2D_SEDE_GAPP sedeg = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == item.Sede).FirstOrDefault();

                int GiornoInizioRichiesto = (sedeg != null && sedeg.giorno_inizio_settimana == 7) ? 0 : 1;


                item.DataInizio = item.DataInizio.Date;

                if (((int)item.DataInizio.DayOfWeek) != GiornoInizioRichiesto)
                {
                    item.esito = false;
                    item.error = "Data inizio errata, per questa sede il giorno inizio settimana deve essere: " + GiornoInizioRichiesto;
                    continue;
                }
                if (item.DataInizio > DateTime.Now)
                {
                    item.esito = false;
                    item.error = "Data inizio non corretta";
                    continue;
                }
                if (!db.MyRai_Resoconti_GetPresenze.Any(x => x.sede == item.Sede && x.data_inizio == item.DataInizio))
                {
                    item.esito = false;
                    item.error = "Dati da aggiornare non presenti";
                    continue;
                }

                try
                {
                    string msg = myRaiCommonTasks.Resoconti.UpdateResocontiGetPresenze( item.Sede , new List<DateTime>( ) { item.DataInizio } );
                    if ( msg == null )
                    item.esito = true;
                    else
                    {
                        item.esito = false;
                        item.error = msg;
                    }

                }
                catch (Exception ex)
                {
                    item.esito = false;
                    item.error = ex.Message;
                }
            }
            return new ResocontiResponse() { SediData = SediData };
        }

        public TrasferteResponse GetTrasferte(string matricolaRichiedente)
        {
            TrasferteResponse response = new TrasferteResponse();

            try
            {
                ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
                //string matricola = "103650";

                string sUserName = matricolaRichiedente + new string(' ', 10 - matricolaRichiedente.Length);

                if (matricolaRichiedente.StartsWith("P", StringComparison.InvariantCultureIgnoreCase))
                {
                    matricolaRichiedente = matricolaRichiedente.Substring(1);
                }

                Trasferta t = new Trasferta();
                bool stop = false;
                string iRipartenza = "000";

                string strCalcolaRipartenza = String.Empty;

                t.Viaggi = new List<Viaggio>();
                t.CompetenzaDefinizione = new TrasfertaCompetenzaDefinizione();

                while (!stop)
                {
                    string inputData = "P956,FTR," + sUserName + "," + iRipartenza;
                    inputData = inputData + new string(' ', 2000);

                    var serviceResponse = c.ComunicaVersoCics(inputData);
                    response.ServiceResponse = serviceResponse.ToString();

                    var data = serviceResponse.ToString();
                    response.Esito = true;
                    string toSplit = data;
                    string strAppoFunzione = string.Empty;
                    string strACK = toSplit.Substring(0, 10);

                    if (!strACK.Substring(3, 1).Equals("9", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string temp = toSplit.Substring(10);
                        if (iRipartenza.Equals("000", StringComparison.InvariantCultureIgnoreCase) &&
                            toSplit.Length > 210)
                        {
                            strCalcolaRipartenza = temp.Substring(114);
                            strCalcolaRipartenza = temp.Substring(86);
                        }
                        else if (toSplit.Length > 210)
                        {
                            strCalcolaRipartenza = strCalcolaRipartenza + temp.Substring(200);
                        }

                        int avanzo = 0;
                        int countSpace = 0;
                        if (!String.IsNullOrEmpty(strCalcolaRipartenza))
                        {
                            avanzo = strCalcolaRipartenza.Length % 50;
                            countSpace = strCalcolaRipartenza.Length / 50;
                        }

                        if (avanzo > 0)
                        {
                            strCalcolaRipartenza = strCalcolaRipartenza.Substring((strCalcolaRipartenza.Length - avanzo));
                        }

                        if ((3 - countSpace) > 0)
                            iRipartenza = new string('0', (3 - countSpace)) + countSpace.ToString();

                        string daDefinire = toSplit.Substring(124);

                        /* COMPETENZE DEL MESE */
                        string cRimborso = daDefinire.Substring(0, 7) + "." + daDefinire.Substring(7, 2);
                        t.CompetenzaDefinizione.CRimborso = Math.Round(double.Parse(cRimborso, CultureInfo.InvariantCulture), 2);

                        string cAnticipo = daDefinire.Substring(10, 7) + "." + daDefinire.Substring(17, 2);
                        t.CompetenzaDefinizione.CAnticipo = Math.Round(double.Parse(cAnticipo, CultureInfo.InvariantCulture), 2);
                        /* FINE COMPETENZE DEL MESE */

                        /* IN CORSO DI DEFINIZIONE */
                        string defSP = daDefinire.Substring(19, 7) + "." + daDefinire.Substring(26, 2);
                        t.CompetenzaDefinizione.DefPreviste = Math.Round(double.Parse(defSP, CultureInfo.InvariantCulture), 2);

                        string defR = daDefinire.Substring(28, 7) + "." + daDefinire.Substring(35, 2);
                        t.CompetenzaDefinizione.DefRimborso = Math.Round(double.Parse(defR, CultureInfo.InvariantCulture), 2);

                        string defA = daDefinire.Substring(38, 7) + "." + daDefinire.Substring(45, 2);
                        t.CompetenzaDefinizione.DefRimborso = Math.Round(double.Parse(defR, CultureInfo.InvariantCulture), 2);
                        /* FINE IN CORSO DI DEFINIZIONE */

                        /* MESE COMPETENZA */
                        string mmComp = string.Empty;
                        if (toSplit.Length > 210)
                        {
                            mmComp = daDefinire.Substring(47, 39);
                        }
                        else
                        {
                            mmComp = daDefinire.Substring(47);
                        }

                        t.CompetenzaDefinizione.MeseCompetenza = mmComp;
                        /* FNIE MESE COMPETENZA */

                        if (toSplit.Length > 210)
                        {
                            strAppoFunzione = toSplit.Substring(210);

                            string[] splitted = Chop(strAppoFunzione, 50);

                            for (int i = 0; i < splitted.Length; i++)
                            {
                                Viaggio v = new Viaggio();

                                v.FoglioViaggio = splitted[i].Substring(0, 7);
                                v.Data = splitted[i].Substring(8, 8);
                                v.Note = splitted[i].Substring(26); // , 24

                                if (splitted[i].Substring(7, 1) == "P")
                                {
                                    string prevista = splitted[i].Substring(16, 8) + "." + splitted[i].Substring(24, 2);
                                    v.SpesaPrevista = Math.Round(double.Parse(prevista, CultureInfo.InvariantCulture), 2);
                                }
                                else if (splitted[i].Substring(7, 1) == "E")
                                {
                                    string rimborso = splitted[i].Substring(16, 8) + "." + splitted[i].Substring(24, 2);
                                    v.Rimborso = Math.Round(double.Parse(rimborso, CultureInfo.InvariantCulture), 2);
                                }
                                else
                                {
                                    string anticipo = splitted[i].Substring(16, 8) + "." + splitted[i].Substring(24, 2);
                                    v.Anticipo = Math.Round(double.Parse(anticipo, CultureInfo.InvariantCulture), 2);
                                }

                                t.Viaggi.Add(v);
                            }
                        }
                    }

                    if (!strACK.Substring(3, 2).Equals("02", StringComparison.InvariantCultureIgnoreCase))
                    {
                        stop = true;
                    }
                }

                response.Trasferte = t;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.Trasferte = new Trasferta();
            }

            return response;
        }



        public VersionResponse Version()
        {
            VersionResponse response = new VersionResponse();

            string[] files = Directory.GetFiles(HostingEnvironment.MapPath("~/bin/"), "*.dll");
            List<InfoVersion> dllFiles = new List<InfoVersion>();
            response.Esito = true;
            response.Errore = String.Empty;
            response.DLLFiles = new List<InfoVersion>();

            try
            {
                foreach (string file in files.OrderBy(x => x))
                {
                    InfoVersion item = new InfoVersion();
                    var fi = new FileInfo(file);
                    item.NomeFile = "\\bin\\" + System.IO.Path.GetFileName(file);
                    item.DataModifica = fi.LastWriteTime;
                    item.Dimensione = fi.Length;
                    dllFiles.Add(item);
                }
                response.DLLFiles = dllFiles;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.ToString();
                response.DLLFiles = new List<InfoVersion>();
            }

            return response;
        }

        private static string[] Chop(string value, int length)
        {
            int strLength = value.Length;
            int strCount = (strLength + length - 1) / length;
            string[] result = new string[strCount];
            for (int i = 0; i < strCount; ++i)
            {
                result[i] = value.Substring(i * length, Math.Min(length, strLength));
                strLength -= length;
            }
            return result;
        }

        #region Informazioni dipendente
        public GetInfoDipendenteResponse GetInformazioniDipendente(string matricola, Boolean soloValide)
        {
            var db = new digiGappEntities();
            try
            {

                var query = from info in db.MyRai_InfoDipendente
                            where info.matricola == matricola
                            select new InfoDipendente
                            {
                                id = info.id,
                                matricola = info.matricola,
                                dataFineInfo = info.data_fine.Value,
                                dataInizioInfo = info.data_inizio,

                                note = info.note,
                                info = info.MyRai_InfoDipendente_Tipologia.info,
                                idInfoDipendenteTipologia = info.MyRai_InfoDipendente_Tipologia.id,
                                noteTipologia = info.MyRai_InfoDipendente_Tipologia.note,
                                dataInizioValiditaTipologia = info.MyRai_InfoDipendente_Tipologia.data_inizio,
                                dataFineValiditaTipologia = info.MyRai_InfoDipendente_Tipologia.data_fine.Value,
                                valore = info.valore,
                                tipoValore = info.MyRai_InfoDipendente_Tipologia.tipo_valore
                            };

                List<InfoDipendente> listaInformazioni = new List<InfoDipendente>();
                listaInformazioni = query.ToList();
                listaInformazioni = listaInformazioni.Where(f => f.dataFineValiditaTipologia == null || (f.dataFineValiditaTipologia != null && f.dataFineValiditaTipologia.Value.CompareTo(DateTime.Today) >= 0)).ToList();
                if (soloValide)
                    listaInformazioni = listaInformazioni.Where(x => x.dataFineInfo == null || (x.dataFineInfo != null && x.dataFineInfo.Value.CompareTo(DateTime.Today) >= 0)).ToList();

                var response = new GetInfoDipendenteResponse()
                {
                    error = null,
                    success = true,
                    InformazioniDipendente = listaInformazioni.OrderBy(f => f.matricola).ToList()
                };
                return response;
            }
            catch (Exception ex)
            {
                return new GetInfoDipendenteResponse()
                {
                    error = ex.Message,
                    success = false,
                    InformazioniDipendente = null
                };
            }
        }
        public GetTipologieInfoDipendenteResponse GetTipiInfoDipendente()
        {
            var db = new digiGappEntities();
            try
            {
                var query = from tipo in db.MyRai_InfoDipendente_Tipologia
                            select new TipoInfoDipendente
                            {
                                id = tipo.id,
                                nomeTipo = tipo.info,
                                dataInizioTipo = tipo.data_inizio,
                                dataFineTipo = tipo.data_fine.Value,
                                noteTipo = tipo.note,
                                tipoValore = tipo.tipo_valore
                            };
                List<TipoInfoDipendente> listaTipiInformazioni = new List<TipoInfoDipendente>();
                listaTipiInformazioni = query.ToList();
                listaTipiInformazioni = listaTipiInformazioni.Where(x => x.dataFineTipo == null || (x.dataFineTipo != null && x.dataFineTipo.Value.CompareTo(DateTime.Today) >= 0)).ToList();
                var response = new GetTipologieInfoDipendenteResponse()
                {
                    error = null,
                    success = true,
                    listaTipologie = listaTipiInformazioni.OrderBy(x => x.nomeTipo).ToList()
                };
                return response;
            }
            catch (Exception ex)
            {
                return new GetTipologieInfoDipendenteResponse()
                {
                    success = false,
                    error = ex.Message,
                    listaTipologie = null
                };
            }
        }
        public NuoveInfoDipendenteResponse creaNuovaInfoDipendente(string matricola, int idTipoInfoDipendente, string valoreInfo, DateTime dataInizioValidita, DateTime? dataFineValidita, string noteInfo)
        {
            var db = new digiGappEntities();
            try
            {
                #region controlli
                var tipo = db.MyRai_InfoDipendente_Tipologia.Where(x => x.id == idTipoInfoDipendente).FirstOrDefault();
                if (tipo == null)
                    return new NuoveInfoDipendenteResponse() { success = false, error = "La tipologia di informazione dipendente non esiste" };

                if (tipo.data_fine != null && tipo.data_fine.Value.CompareTo(DateTime.Today) < 0)
                    return new NuoveInfoDipendenteResponse() { success = false, error = "La tipologia di informazione non è più valida" };
                if (matricola == "")
                    return new NuoveInfoDipendenteResponse() { success = false, error = "Specificare una matricola" };

                if (matricola == null)
                    return new NuoveInfoDipendenteResponse() { success = false, error = "Specificare una matricola" };


                if (dataFineValidita != null)
                {
                    if (dataFineValidita.Value.CompareTo(dataInizioValidita) < 0)
                        return new NuoveInfoDipendenteResponse() { success = false, error = "La data fine validità non può essere minore della data inizio validità" };
                }

                Convert.ChangeType(valoreInfo, Type.GetType(tipo.tipo_valore));

                var info = db.MyRai_InfoDipendente.Where(x => x.id_infodipendente_tipologia == idTipoInfoDipendente && x.matricola == matricola).FirstOrDefault();
                if (info != null)
                    return new NuoveInfoDipendenteResponse() { success = false, error = "Esiste già un'informazione di tipo " + info.MyRai_InfoDipendente_Tipologia.info + " per la matricola " + info.matricola };
                #endregion
                db.MyRai_InfoDipendente.Add(new MyRai_InfoDipendente
                {
                    matricola = matricola,
                    id_infodipendente_tipologia = idTipoInfoDipendente,
                    valore = valoreInfo.ToString(),
                    data_inizio = dataInizioValidita,
                    data_fine = dataFineValidita,
                    note = noteInfo
                });

                db.SaveChanges();
                var response = new NuoveInfoDipendenteResponse()
                {
                    success = true,
                    error = null
                };
                return response;
            }
            catch (Exception ex)
            {
                return new NuoveInfoDipendenteResponse()
                {
                    success = false,
                    error = ex.Message
                };
            }
        }
        public NuovaTipologiaInfoDipendente creaNuovaTipologiaInfoDipendente(string nomeTipologia, DateTime dataInizioValidita, DateTime? dataFineValidita, string noteTipo, string tipoValore)
        {
            var db = new digiGappEntities();
            try
            {
                #region controlli
                if (nomeTipologia == "")
                    return new NuovaTipologiaInfoDipendente() { success = false, error = "Specificare il nome della tipologia" };
                if (nomeTipologia == null)
                    return new NuovaTipologiaInfoDipendente() { success = false, error = "Specificare il nome della tipologia" };

                var check = db.MyRai_InfoDipendente_Tipologia.Where(x => x.info.ToLower() == nomeTipologia.ToLower()).FirstOrDefault();
                if (check != null)
                    return new NuovaTipologiaInfoDipendente() { success = false, error = "La tipologia inserita è già presente a sistema" };

                if (tipoValore == "")
                    return new NuovaTipologiaInfoDipendente() { success = false, error = "Specificare il tipo di valore della tipologia" };

                if (tipoValore == null)
                    return new NuovaTipologiaInfoDipendente() { success = false, error = "Specificare il tipo di valore della tipologia" };


                if (dataFineValidita != null)
                {
                    if (dataFineValidita.Value.CompareTo(dataInizioValidita) < 0)
                        return new NuovaTipologiaInfoDipendente() { success = false, error = "La data fine validità non può essere inferiore alla data inizio validità" };
                }
                #endregion
                db.MyRai_InfoDipendente_Tipologia.Add(
                    new MyRai_InfoDipendente_Tipologia()
                    {
                        info = nomeTipologia,
                        data_inizio = dataInizioValidita,
                        data_fine = dataFineValidita,
                        note = noteTipo,
                        tipo_valore = tipoValore
                    });
                db.SaveChanges();

                return new NuovaTipologiaInfoDipendente()
                {
                    success = true,
                    error = null
                };
            }
            catch (Exception ex)
            {
                return new NuovaTipologiaInfoDipendente()
                {
                    success = false,
                    error = ex.Message
                };
            }
        }
        public CambiaInfoDipendenteResponse cambiaInformazioniDipendente(string matricola, string valoreInfo, DateTime? dataInizioValidita, DateTime? dataFineValidita, int idTipoInformazione, string noteInfo)
        {
            var db = new digiGappEntities();
            try
            {


                if (matricola == null || matricola == "")
                    return new CambiaInfoDipendenteResponse() { success = false, error = "Specificare la matricola" };


                // in teoria non servirebbe
                var tipo = db.MyRai_InfoDipendente_Tipologia.Where(x => x.id == idTipoInformazione).FirstOrDefault();

                if (tipo == null)
                    return new CambiaInfoDipendenteResponse() { success = false, error = "Tipologia informazione inesistente" };

                if (tipo.data_fine != null && tipo.data_fine.Value.CompareTo(DateTime.Today) < 0)
                    return new CambiaInfoDipendenteResponse() { success = false, error = "Tipologia informazione non più valida" };
                bool nuovoRec = false;
                #region verifica tipo
                // verifico il tipo del valore in una try catch per dare, in caso di errore, un messaggio parlante
                try
                {
                    Convert.ChangeType(valoreInfo, Type.GetType(tipo.tipo_valore));
                }
                catch (Exception)
                {
                    return new CambiaInfoDipendenteResponse
                    {
                        success = false,
                        error = "Il valore inserito non corrisponde al tipo richiesto per questa informazione"
                    };
                }
                #endregion

                var info = db.MyRai_InfoDipendente.Where(x => x.matricola == matricola && x.id_infodipendente_tipologia == idTipoInformazione).FirstOrDefault();

                if (info != null)
                {
                    // imposto la data inizio solo qualora venga passata, altrimenti lascio quella che è sul db
                    if (dataInizioValidita != null)
                        info.data_inizio = dataInizioValidita.Value;
                }
                else
                {
                    nuovoRec = true;
                    // nuovo record
                    info = new MyRai_InfoDipendente();
                    // se presente la data inizio ok, altrimenti imposto la data odierna
                    info.data_inizio = dataInizioValidita != null ? dataInizioValidita.Value : DateTime.Today;
                    info.matricola = matricola;
                    info.id_infodipendente_tipologia = idTipoInformazione;
                }

                // nel caso in cui il valore info venga trattato come boleano ed è false 
                // verifico che la data fine sia null, se è così, imposto la data odierna altrimenti il valore passato.
                //al contrario se il valore info è diverso da false imposto la data passata (o eventualmente null)
                if (valoreInfo.ToLower() == "false")
                    info.data_fine = dataFineValidita != null ? dataFineValidita : DateTime.Today;
                else
                    info.data_fine = dataFineValidita;

                // la data inizio sicuramente non sarà null, ma la data fine può esserlo, qualora non lo sia effettuo il controllo sulla correttezza
                if (info.data_fine != null)
                {
                    if (info.data_fine.Value.CompareTo(info.data_inizio) < 0)
                        return new CambiaInfoDipendenteResponse { success = false, error = "La data fine (" + info.data_fine.Value.ToString() + ") non può essere minore della data inizio (" + info.data_inizio.ToString() + ")" };
                }
                info.valore = valoreInfo;
                info.note = noteInfo;
                if (nuovoRec)
                    db.MyRai_InfoDipendente.Add(info);

                db.SaveChanges();

                var response = new CambiaInfoDipendenteResponse()
                {
                    success = true,
                    error = null
                };
                return response;
            }
            catch (Exception ex)
            {
                return new CambiaInfoDipendenteResponse()
                {
                    success = false,
                    error = ex.Message
                };
            }
        }
        public EliminaInfoDipendenteResponse EliminaInfoDipendente(string matricola, int idTipologiaInformazione)
        {
            var db = new digiGappEntities();

            try
            {
                if (matricola == "")
                    return new EliminaInfoDipendenteResponse { success = false, error = "Specificare la matricola" };

                var info = db.MyRai_InfoDipendente.Where(x => x.matricola == matricola && x.id_infodipendente_tipologia == idTipologiaInformazione).FirstOrDefault();

                if (info == null)
                {
                    return new EliminaInfoDipendenteResponse
                    {
                        success = false,
                        error = "Informazione inesistente"
                    };
                }
                else
                {
                    db.MyRai_InfoDipendente.Remove(info);
                    db.SaveChanges();
                }

                return new EliminaInfoDipendenteResponse
                {
                    success = true,
                    error = "Informazione eliminata"
                };
            }
            catch (Exception ex)
            {
                return new EliminaInfoDipendenteResponse
                {
                    success = false,
                    error = ex.Message
                };
            }
        }
        #endregion

        #region sedi
        public GetSettimanaSedi GetListaSediGappSettimana(string[] codiciSede)
        {
            var db = new digiGappEntities();
            try
            {
                List<SedeGappSettimana> listaSediSettimana = new List<SedeGappSettimana>();
                List<SedeGappSettimana> listaCheck = new List<SedeGappSettimana>();
                var query = from sedi in db.L2D_SEDE_GAPP
                            select new SedeGappSettimana
                            {
                                skySedeGapp = sedi.sky_sede_gapp,
                                sedePresente = codiciSede.Contains(sedi.cod_sede_gapp),
                                scadenza = sedi.scadenza.Value,
                                codRsu = sedi.cod_rsu,
                                codSede = sedi.cod_sede,
                                codSedeGapp = sedi.cod_sede_gapp,
                                codServCont = sedi.cod_serv_cont,
                                dataAgg = sedi.Data_Agg.Value,
                                dataElim = sedi.Data_Elim.Value,
                                dataFineVal = sedi.Data_Fine_Val,
                                dataFineValidita = sedi.data_fine_validita.Value,
                                dataInizioVal = sedi.data_inizio_val.Value,
                                dataInizioValidita = sedi.data_inizio_validita.Value,
                                dataIns = sedi.Data_Ins,
                                descRsu = sedi.desc_rsu,
                                descSedeGapp = sedi.desc_sede_gapp,
                                flagIvt = sedi.flag_ivt,
                                flagPresenzaSirio = sedi.flag_presenza_sirio,
                                flgUltimo = sedi.flg_ultimo,
                                giornoInizioSettimana = sedi.giorno_inizio_settimana == null ? 1 : sedi.giorno_inizio_settimana.Value,
                                minimoCar = sedi.minimo_car,
                                partenzaFase2 = sedi.partenza_fase_2,
                                partenzaFase3 = sedi.partenza_fase_3
                            };

                listaCheck = query.ToList();
                SedeGappSettimana sede = new SedeGappSettimana();
                for (int i = 0; i < codiciSede.Length; i++)
                {
                    sede = listaCheck.Find(x => x.codSedeGapp == codiciSede[i]);
                    if (sede == null)
                        listaSediSettimana.Add(new SedeGappSettimana(codiciSede[i]));
                    else
                        listaSediSettimana.Add(sede);
                }

                var response = new GetSettimanaSedi()
                {
                    success = true,
                    error = null,
                    listaSedi = listaSediSettimana
                };

                return response;
            }
            catch (Exception ex)
            {
                return new GetSettimanaSedi()
                {
                    success = false,
                    error = ex.Message,
                    listaSedi = null
                };
            }
        }
        public NuovaSedeGappSettimana SalvaGiornoInizioSettimanaSede(string codiceSedeGapp, int? giornoInizio)
        {
            var db = new digiGappEntities();
            string connectionString = connectionStringComposer();
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connectionString);

            try
            {
                if (giornoInizio.Value < 1 || giornoInizio.Value > 7)
                    return new NuovaSedeGappSettimana { success = false, error = "Valore per giornoInizio non valido inserire un valore tra 1 e 7" };

                var sede = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp.ToUpper().Trim() == codiceSedeGapp.ToUpper().Trim()).FirstOrDefault();

                if (sede == null)
                    return new NuovaSedeGappSettimana { success = false, error = "Sede non trovata" };


                //sede.giorno_inizio_settimana = giornoInizio;


                conn.Open();

                string query = "UPDATE L2D_SEDE_GAPP ";
                query += "SET giorno_inizio_settimana=" + giornoInizio.ToString();
                query += " WHERE cod_sede_gapp='" + codiceSedeGapp + "'";


                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
                cmd.CommandText = query;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();

                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();

                return new NuovaSedeGappSettimana
                {
                    success = true,
                    error = null
                };

            }
            catch (Exception ex)
            {

                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();

                return new NuovaSedeGappSettimana
                {
                    success = false,
                    error = ex.Message
                };
            }
        }

        private string connectionStringComposer()
        {
            //metadata=res://*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=stocav420,6000;initial catalog=digiGapp;user id=dba_digigapp;password=g2w3e3r3;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient"
            string connDb = System.Configuration.ConfigurationManager.ConnectionStrings["digiGappEntities2"].ConnectionString;
            string connEdit = "";
            string provider = "";
            string dataSource = "";
            string userStr = "";
            string pwdStr = "";
            string catalog = "";
            int pos = 0;
            int pos2 = 0;
            // provider

            pos = connDb.IndexOf(";provider=");
            pos += 10;
            //System.Data.SqlClient;provider connection string=&quot;data source=stocav420,6000;initial catalog=digiGapp;user id=dba_digigapp;password=g2w3e3r3;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient"
            connEdit = connDb.Substring(pos);
            pos2 = connEdit.IndexOf(";");
            //System.Data.SqlClient
            provider = connEdit.Substring(0, pos2);

            pos = connEdit.IndexOf("data source=");
            pos += 12;
            //stocav420,6000;initial catalog=digiGapp;user id=dba_digigapp;password=g2w3e3r3;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient"
            connEdit = connEdit.Substring(pos);
            pos2 = connEdit.IndexOf(";");
            //stocav420,6000
            dataSource = connEdit.Substring(0, pos2);

            //stocav420,6000;initial catalog=digiGapp;user id=dba_digigapp;password=g2w3e3r3;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient"
            pos = connEdit.IndexOf("initial catalog=");
            pos += 16;

            //digiGapp;user id=dba_digigapp;password=g2w3e3r3;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient"
            connEdit = connEdit.Substring(pos);
            pos2 = connEdit.IndexOf(";");

            //digiGapp
            catalog = connEdit.Substring(0, pos2);

            pos = connEdit.IndexOf("user id=");
            pos += 8;
            //dba_digigapp;password=g2w3e3r3;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient"
            connEdit = connEdit.Substring(pos);
            pos2 = connEdit.IndexOf(";");
            //dba_digigapp
            userStr = connEdit.Substring(0, pos2);

            pos = connEdit.IndexOf("password=");
            pos += 9;

            //g2w3e3r3;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient"
            connEdit = connEdit.Substring(pos);
            pos2 = connEdit.IndexOf(";");

            pwdStr = connEdit.Substring(0, pos2);

            string connectionString = "";
            //connectionString+= "providerName=" + provider + ";";
            connectionString += "Data Source=" + dataSource + ";";
            connectionString += "Initial Catalog=" + catalog + ";";
            connectionString += "user id=" + userStr + ";";
            connectionString += "password=" + pwdStr;

            return connectionString;
        }
        #endregion

        #region NotaSegreteria

        /// <summary>
        /// Inserimento di una nota da segreteria
        /// </summary>
        /// <param name="matricola">Matricola del dipendente per il quale si intende inserire l'eccezione</param>
        /// <param name="data">Data di riferimento in cui il dipendente usufruirà dell'eccezione</param>
        /// <param name="codice">Codice dell'eccezione</param>
        /// <param name="dalle">Eventuale ora di inizio eccezione</param>
        /// <param name="alle">Eventuale ora di fine dell'eccezione</param>
        /// <param name="motivo">Motivo dell'eccezione</param>
        /// <param name="nota">Nota aggiuntiva all'eccezione</param>
        /// <returns></returns>
        public InserisciNotaSegreteriaResponse InserisciNotaSegreteria(string matricola, DateTime data, string codice, DateTime? dalle, DateTime? alle, string motivo, string nota)
        {
            InserisciNotaSegreteriaResponse response = new InserisciNotaSegreteriaResponse();

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 7)
                {
                    matricola = matricola.PadLeft(7, '0');
                }
                else if (matricola.Length > 7)
                {
                    throw new Exception("Matricola non valida. La lunghezza prevista è 7.");
                }

                MyRai_Note_Da_Segreteria ecc = new MyRai_Note_Da_Segreteria()
                {
                    Id = 0,
                    Matricola = matricola,
                    Data = data,
                    Codice = codice,
                    Dalle = dalle,
                    Alle = alle,
                    Motivo = motivo,
                    Nota = nota
                };

                using (digiGappEntities db = new digiGappEntities())
                {
                    db.MyRai_Note_Da_Segreteria.Add(ecc);

                    db.SaveChanges();
                }

                response.Esito = true;
                response.Errore = String.Empty;
                response.Nota = ecc;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.Nota = null;

                ScriviLog(ex.ToString(), matricola, "InserisciNotaSegreteria", null);
            }

            return response;
        }

        /// <summary>
        /// Aggiornamento di una nota da segreteria
        /// </summary>
        /// <param name="id">Identificativo univoco dell'eccezione di cui si intendono aggiornare i valori</param>
        /// <param name="matricola">Matricola del dipendente per il quale si intende inserire l'eccezione</param>
        /// <param name="data">Data di riferimento in cui il dipendente usufruirà dell'eccezione</param>
        /// <param name="codice">Codice dell'eccezione</param>
        /// <param name="dalle">Eventuale ora di inizio eccezione</param>
        /// <param name="alle">Eventuale ora di fine dell'eccezione</param>
        /// <param name="motivo">Motivo dell'eccezione</param>
        /// <param name="nota">Nota aggiuntiva all'eccezione</param>
        /// <returns></returns>
        public AggiornaNotaSegreteriaResponse AggiornaNotaSegreteria(int id, string matricola, DateTime data, string codice, DateTime? dalle, DateTime? alle, string motivo, string nota)
        {
            AggiornaNotaSegreteriaResponse response = new AggiornaNotaSegreteriaResponse();

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 7)
                {
                    matricola = matricola.PadLeft(7, '0');
                }
                else if (matricola.Length > 7)
                {
                    throw new Exception("Matricola non valida. La lunghezza prevista è 7.");
                }

                using (digiGappEntities db = new digiGappEntities())
                {
                    var item = db.MyRai_Note_Da_Segreteria.Where(e => e.Id.Equals(id)).FirstOrDefault();

                    if (item != null)
                    {
                        item.Matricola = matricola;
                        item.Data = data;
                        item.Codice = codice;
                        item.Dalle = dalle;
                        item.Alle = alle;
                        item.Motivo = motivo;
                        item.Nota = nota;
                    }

                    db.SaveChanges();

                    response.Nota = item;
                }

                response.Esito = true;
                response.Errore = String.Empty;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.Nota = null;
                ScriviLog(ex.ToString(), matricola, "AggiornaNotaSegreteria", null);
            }

            return response;
        }

        /// <summary>
        /// Cancellazione di una nota da segreteria a partire dal suo identificativo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RimuoviNotaSegreteriaResponse RimuoviNotaSegreteria(int id)
        {
            RimuoviNotaSegreteriaResponse response = new RimuoviNotaSegreteriaResponse();

            try
            {
                if (id <= 0)
                {
                    throw new Exception("Impossibile rimuovere la nota. Identificativo non valido.");
                }

                using (digiGappEntities db = new digiGappEntities())
                {
                    var item = db.MyRai_Note_Da_Segreteria.Where(e => e.Id.Equals(id)).FirstOrDefault();

                    if (item == null)
                    {
                        throw new Exception("Impossibile rimuovere la nota. Nota non trovata.");
                    }

                    db.MyRai_Note_Da_Segreteria.Remove(item);

                    db.SaveChanges();
                }

                response.Esito = true;
                response.Errore = String.Empty;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Reperimento delle note di segreteria per l'utente e la data passate
        /// </summary>
        /// <param name="matricola">Matricola dell'utente di cui si intende ottenere le informazioni</param>
        /// <param name="data">Data in cui cercare le note</param>
        /// <returns></returns>
        public NotaSegreteriaResponse GetNoteSegreteria(string matricola, DateTime data)
        {
            NotaSegreteriaResponse response = new NotaSegreteriaResponse();
            response.Note = new List<MyRai_Note_Da_Segreteria>();
            response.Esito = true;
            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 7)
                {
                    matricola = matricola.PadLeft(7, '0');
                }
                else if (matricola.Length > 7)
                {
                    throw new Exception("Matricola non valida. La lunghezza prevista è 7.");
                }

                DateTime start = new DateTime(data.Year, data.Month, data.Day, 0, 0, 0);
                DateTime stop = new DateTime(data.Year, data.Month, data.Day, 23, 59, 59);

                using (var db = new digiGappEntities())
                {
                    var item = from ecc in db.MyRai_Note_Da_Segreteria
                               where ecc.Matricola.Equals(matricola) &&
                                    (ecc.Data >= start && ecc.Data <= stop)
                               select ecc;

                    if (item != null && item.Any())
                        response.Note.AddRange(item.ToList());
                }
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Reperimento delle note di segreteria nella data indicata
        /// </summary>
        /// <param name="data">Data in cui cercare le note</param>
        /// <returns></returns>
        public NotaSegreteriaResponse GetNoteSegreteriaPerData(DateTime data)
        {
            NotaSegreteriaResponse response = new NotaSegreteriaResponse();
            response.Note = new List<MyRai_Note_Da_Segreteria>();
            response.Esito = true;
            try
            {
                DateTime start = new DateTime(data.Year, data.Month, data.Day, 0, 0, 0);
                DateTime stop = new DateTime(data.Year, data.Month, data.Day, 23, 59, 59);

                using (var db = new digiGappEntities())
                {
                    var item = from ecc in db.MyRai_Note_Da_Segreteria
                               where ecc.Data >= start && ecc.Data <= stop
                               select ecc;

                    if (item != null && item.Any())
                        response.Note.AddRange(item.ToList());
                }
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
            }

            return response;
        }

        #endregion

        #region Visualizzazione giornata da segreteria

        public VisualizzazioneGiornataResponse SetVisualizzazioneGiornata(string matricola, bool visualizzato, string matricolaVisualizzatore, string utenteVisualizzatore, DateTime dataRichiesta, int? idRichiesta = null)
        {

            VisualizzazioneGiornataResponse response = new VisualizzazioneGiornataResponse();
            response.VisualizzazioneGiornata = new MyRai_Visualizzazione_Giornate_Da_Segreteria_Custom();

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length != 6)
                {
                    throw new Exception("Matricola non valida. La lunghezza prevista è 6.");
                }

                if (String.IsNullOrEmpty(matricolaVisualizzatore))
                {
                    throw new Exception("La matricola visualizzatore è un dato obbligatorio.");
                }

                if (matricolaVisualizzatore.Length < 7)
                {
                    matricolaVisualizzatore = matricolaVisualizzatore.PadLeft(7, '0');
                }
                else if (matricolaVisualizzatore.Length > 7)
                {
                    throw new Exception("Matricola non valida. La lunghezza prevista è 7.");
                }

                // verifica la presenza del dato sul db
                MyRai_Visualizzazione_Giornate_Da_Segreteria item = this.GetVisualizzazioneGiornataDaSegreteriaInternal(matricola, dataRichiesta, null);

                if (item != null && item.Id > 0)
                {
                    // se l'elemento è presente allora si procede con l'update dei dati
                    item = this.AggiornaVisualizzazioneGiornata(item.Id, visualizzato, matricolaVisualizzatore, utenteVisualizzatore, idRichiesta);
                }
                else
                {
                    // Insert
                    item = this.InserisciVisualizzazioneGiornata(matricola, visualizzato, matricolaVisualizzatore, utenteVisualizzatore, dataRichiesta, idRichiesta);
                }

                response.Esito = true;
                response.Errore = String.Empty;
                response.VisualizzazioneGiornata = new MyRai_Visualizzazione_Giornate_Da_Segreteria_Custom()
                {
                    DataCreazione = item.DataCreazione,
                    DataUltimoAccesso = item.DataUltimoAccesso,
                    Id = item.Id,
                    IdRichiesta = item.IdRichiesta,
                    MatricolaVisualizzatore = item.MatricolaVisualizzatore,
                    UtenteVisualizzatore = item.UtenteVisualizzatore,
                    Visualizzato = item.Visualizzato,
                    DataRichiesta = item.DataRichiesta,
                    Matricola = item.Matricola
                };
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.VisualizzazioneGiornata = null;
            }

            return response;
        }

        public VisualizzazioneGiornataResponse GetVisualizzazioneGiornata(string matricola, DateTime data, bool? visualizzato = null)
        {
            VisualizzazioneGiornataResponse response = new VisualizzazioneGiornataResponse();
            response.VisualizzazioneGiornata = new MyRai_Visualizzazione_Giornate_Da_Segreteria_Custom();

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 6)
                {
                    matricola = matricola.PadLeft(6, '0');
                }
                else if (matricola.Length > 6)
                {
                    throw new Exception("Matricola non valida. La lunghezza prevista è 6.");
                }

                MyRai_Visualizzazione_Giornate_Da_Segreteria item = this.GetVisualizzazioneGiornataDaSegreteriaInternal(matricola, data, visualizzato);

                if (item != null)
                {
                    MyRai_Visualizzazione_Giornate_Da_Segreteria_Custom result = new MyRai_Visualizzazione_Giornate_Da_Segreteria_Custom()
                    {
                        DataCreazione = item.DataCreazione,
                        DataUltimoAccesso = item.DataUltimoAccesso,
                        Id = item.Id,
                        IdRichiesta = item.IdRichiesta,
                        MatricolaVisualizzatore = item.MatricolaVisualizzatore,
                        UtenteVisualizzatore = item.UtenteVisualizzatore,
                        Visualizzato = item.Visualizzato,
                        DataRichiesta = item.DataRichiesta,
                        Matricola = item.Matricola
                    };

                    using (digiGappEntities db = new digiGappEntities())
                    {
                        bool inApprovazione = ((from ecc in db.MyRai_Eccezioni_Richieste
                                                join r in db.MyRai_Richieste
                                                on ecc.id_richiesta equals r.id_richiesta
                                                where r.matricola_richiesta == matricola &&
                                                      ecc.id_stato == 10
                                                select ecc).Count() > 0);

                        bool approvati = ((from ecc in db.MyRai_Eccezioni_Richieste
                                           join r in db.MyRai_Richieste
                                           on ecc.id_richiesta equals r.id_richiesta
                                           where r.matricola_richiesta == matricola &&
                                                  ecc.id_stato == 20
                                           select ecc).Count() > 0);

                        result.InApprovazione = inApprovazione;
                        result.Approvate = inApprovazione ? false : approvati;
                    }
                    response.VisualizzazioneGiornata = result;
                }

                response.Esito = true;
                response.Errore = String.Empty;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.VisualizzazioneGiornata = null;
            }

            return response;
        }

        public VisualizzazioniGiornataResponse_Elenco GetElencoVisualizzazioneGiornata(string matricola, DateTime dataDa, DateTime dataA, bool? visualizzati = null)
        {
            VisualizzazioniGiornataResponse_Elenco response = new VisualizzazioniGiornataResponse_Elenco();

            response.Errore = String.Empty;
            response.Esito = true;
            response.VisualizzazioniGiornate = new List<MyRai_Visualizzazione_Giornate_Da_Segreteria_Custom>();

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 6)
                {
                    matricola = matricola.PadLeft(6, '0');
                }
                else if (matricola.Length > 6)
                {
                    throw new Exception("Matricola non valida. La lunghezza prevista è 6.");
                }

                if (dataDa.Date > dataA.Date)
                {
                    throw new Exception("Data inizio range non può essere superiore alla data di fine range.");
                }

                DateTime start = new DateTime(dataDa.Year, dataDa.Month, dataDa.Day, 0, 0, 0);
                DateTime stop = new DateTime(dataA.Year, dataA.Month, dataA.Day, 23, 59, 59);

                using (digiGappEntities db = new digiGappEntities())
                {
                    List<MyRai_Visualizzazione_Giornate_Da_Segreteria> items = new List<MyRai_Visualizzazione_Giornate_Da_Segreteria>();

                    if (visualizzati.HasValue && visualizzati.Value)
                    {
                        items = (from vis in db.MyRai_Visualizzazione_Giornate_Da_Segreteria
                                 where vis.DataRichiesta >= start && vis.DataRichiesta <= stop &&
                                  vis.Visualizzato == true && vis.Matricola == matricola
                                 select vis).ToList();
                    }
                    else if (visualizzati.HasValue && !visualizzati.Value)
                    {
                        items = (from vis in db.MyRai_Visualizzazione_Giornate_Da_Segreteria
                                 where vis.DataRichiesta >= start && vis.DataRichiesta <= stop &&
                                  vis.Visualizzato == false && vis.Matricola == matricola
                                 select vis).ToList();
                    }
                    else
                    {
                        items = (from vis in db.MyRai_Visualizzazione_Giornate_Da_Segreteria
                                 where vis.DataRichiesta >= start && vis.DataRichiesta <= stop &&
                                   vis.Matricola == matricola
                                 select vis).ToList();
                    }

                    if (items != null && items.Any())
                    {
                        items.ForEach(item =>
                        {
                            response.VisualizzazioniGiornate.Add(new MyRai_Visualizzazione_Giornate_Da_Segreteria_Custom()
                            {
                                DataCreazione = item.DataCreazione,
                                DataUltimoAccesso = item.DataUltimoAccesso,
                                Id = item.Id,
                                IdRichiesta = item.IdRichiesta,
                                MatricolaVisualizzatore = item.MatricolaVisualizzatore,
                                UtenteVisualizzatore = item.UtenteVisualizzatore,
                                Visualizzato = item.Visualizzato,
                                DataRichiesta = item.DataRichiesta,
                                Matricola = item.Matricola
                            });
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.VisualizzazioniGiornate = null;
            }

            return response;
        }

        public StatoEccezioniGiornateResponse GetStatoEccezioniGiornate(string matricola, DateTime dataDa, DateTime dataA)
        {
            StatoEccezioniGiornateResponse response = new StatoEccezioniGiornateResponse();

            response.Errore = String.Empty;
            response.Esito = true;
            response.StatoEccezioniGiornate = new List<MyRai_StatoEccezioniGiornate_Custom>();

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 6)
                {
                    matricola = matricola.PadLeft(6, '0');
                }
                else if (matricola.Length > 6)
                {
                    throw new Exception("Matricola non valida. La lunghezza prevista è 6.");
                }

                if (dataDa.Date > dataA.Date)
                {
                    throw new Exception("Data inizio range non può essere superiore alla data di fine range.");
                }

                DateTime start = new DateTime(dataDa.Year, dataDa.Month, dataDa.Day, 0, 0, 0);
                DateTime stop = new DateTime(dataA.Year, dataA.Month, dataA.Day, 23, 59, 59);

                using (digiGappEntities db = new digiGappEntities())
                {
                    var ectipo1 = db.MyRai_Eccezioni_Ammesse.Where(x => x.id_raggruppamento == 1)
                        .Select(x => x.cod_eccezione.Trim()).ToArray();

                    // ciclo i giorni del mese
                    // per ogni giorno verifico se c'è il visualizzato
                    // e se ci sono giorni in approvazione e/o approvati
                    for (int giorno = 1; giorno <= stop.Day; giorno++)
                    {
                        DateTime _dt = new DateTime(dataDa.Year, dataDa.Month, giorno, 0, 0, 0);

                        // verifica se ci sono visualizzati
                        var visualizzato = db.MyRai_Visualizzazione_Giornate_Da_Segreteria
                                            .Where(w => EntityFunctions.TruncateTime(w.DataRichiesta) ==
                                                        EntityFunctions.TruncateTime(_dt) &&
                                                    w.Matricola == matricola).FirstOrDefault();

                        //verifica se ci sono eccezioni in approvazione per quella giornata
                        bool inApprovazione = ((from ecc in db.MyRai_Eccezioni_Richieste
                                                join r in db.MyRai_Richieste
                                                on ecc.id_richiesta equals r.id_richiesta
                                                where r.matricola_richiesta == matricola &&
                                                    ecc.id_stato == 10 &&
                                                    EntityFunctions.TruncateTime(ecc.data_eccezione) ==
                                                    EntityFunctions.TruncateTime(_dt)
                                                select ecc).Count() > 0);

                        // verifica se ci sono eccezioni approvate per la giornata esaminata
                        bool approvati = ((from ecc in db.MyRai_Eccezioni_Richieste
                                           join r in db.MyRai_Richieste
                                           on ecc.id_richiesta equals r.id_richiesta
                                           where r.matricola_richiesta == matricola &&
                                               ecc.id_stato == 20 &&
                                               EntityFunctions.TruncateTime(ecc.data_eccezione) ==
                                               EntityFunctions.TruncateTime(_dt)
                                           select ecc).Count() > 0);


                        bool approvatiT1 = false;
                        if (!inApprovazione)
                        {
                            approvatiT1 = ((from ecc in db.MyRai_Eccezioni_Richieste
                                            join r in db.MyRai_Richieste
                                            on ecc.id_richiesta equals r.id_richiesta
                                            where r.matricola_richiesta == matricola &&
                                            !r.MyRai_Eccezioni_Richieste.Any(x => x.azione == "C" && x.id_stato == 20) &&
                                            ectipo1.Contains(ecc.cod_eccezione) &&
                                                ecc.id_stato == 20 &&
                                                EntityFunctions.TruncateTime(ecc.data_eccezione) ==
                                                EntityFunctions.TruncateTime(_dt)
                                            select ecc).Count() > 0);
                        }


                        bool empty = ((from ecc in db.MyRai_Eccezioni_Richieste
                                       join r in db.MyRai_Richieste
                                       on ecc.id_richiesta equals r.id_richiesta
                                       where r.matricola_richiesta == matricola &&
                                           EntityFunctions.TruncateTime(ecc.data_eccezione) ==
                                           EntityFunctions.TruncateTime(_dt)
                                       select ecc).Count() == 0);

                        response.StatoEccezioniGiornate.Add(new MyRai_StatoEccezioniGiornate_Custom
                        {
                            Visualizzato = visualizzato != null ? visualizzato.Visualizzato : false,
                            DataEccezione = _dt,
                            DettaglioVisualizzazione = visualizzato != null ? visualizzato : null,
                            Matricola = matricola,
                            InApprovazione = inApprovazione,
                            Approvate = inApprovazione ? false : approvati,
                            ApprovateTipo1 = approvatiT1,
                            Empty = empty
                        });

                    }
                }
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.StatoEccezioniGiornate = null;
            }

            return response;
        }

        /// <summary>
        /// Reperimento di una particolare Visualizzazione di Giornata Da Segreteria per la matricola indicata
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="data"></param>
        /// <param name="visualizzati"></param>
        /// <returns></returns>
        private MyRai_Visualizzazione_Giornate_Da_Segreteria GetVisualizzazioneGiornataDaSegreteriaInternal(string matricola, DateTime data, bool? visualizzati = null)
        {
            MyRai_Visualizzazione_Giornate_Da_Segreteria response = null;

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 6)
                {
                    matricola = matricola.PadLeft(6, '0');
                }
                else if (matricola.Length > 6)
                {
                    throw new Exception("Matricola non valida. La lunghezza prevista è 6.");
                }

                DateTime start = new DateTime(data.Year, data.Month, data.Day, 0, 0, 0);
                DateTime stop = new DateTime(data.Year, data.Month, data.Day, 23, 59, 59);

                using (digiGappEntities db = new digiGappEntities())
                {
                    MyRai_Visualizzazione_Giornate_Da_Segreteria item = new MyRai_Visualizzazione_Giornate_Da_Segreteria();

                    if (visualizzati.HasValue && visualizzati.Value)
                    {
                        item = (from vis in db.MyRai_Visualizzazione_Giornate_Da_Segreteria
                                where vis.DataRichiesta >= start && vis.DataRichiesta <= stop &&
                                 vis.Visualizzato == true && vis.Matricola == matricola
                                select vis).FirstOrDefault();
                    }
                    else if (visualizzati.HasValue && !visualizzati.Value)
                    {
                        item = (from vis in db.MyRai_Visualizzazione_Giornate_Da_Segreteria
                                where vis.DataRichiesta >= start && vis.DataRichiesta <= stop &&
                                 vis.Visualizzato == false && vis.Matricola == matricola
                                select vis).FirstOrDefault();
                    }
                    else
                    {
                        item = (from vis in db.MyRai_Visualizzazione_Giornate_Da_Segreteria
                                where vis.DataRichiesta >= start && vis.DataRichiesta <= stop &&
                                  vis.Matricola == matricola
                                select vis).FirstOrDefault();
                    }

                    if (item != null)
                    {
                        response = new MyRai_Visualizzazione_Giornate_Da_Segreteria();

                        response = item;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return response;
        }

        private MyRai_Visualizzazione_Giornate_Da_Segreteria InserisciVisualizzazioneGiornata(string matricola, bool visualizzato, string matricolaVisualizzatore, string utenteVisualizzatore, DateTime dataRichiesta, int? idRichiesta = null)
        {
            MyRai_Visualizzazione_Giornate_Da_Segreteria response = null;

            try
            {
                DateTime insertDate = DateTime.Now;

                response = new MyRai_Visualizzazione_Giornate_Da_Segreteria()
                {
                    Id = 0,
                    IdRichiesta = idRichiesta,
                    MatricolaVisualizzatore = matricolaVisualizzatore,
                    UtenteVisualizzatore = utenteVisualizzatore,
                    Visualizzato = visualizzato,
                    DataCreazione = insertDate,
                    DataUltimoAccesso = insertDate,
                    DataRichiesta = dataRichiesta,
                    Matricola = matricola
                };

                using (digiGappEntities db = new digiGappEntities())
                {
                    db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Add(response);

                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return response;
        }

        private MyRai_Visualizzazione_Giornate_Da_Segreteria AggiornaVisualizzazioneGiornata(int id, bool visualizzato, string matricolaVisualizzatore, string utenteVisualizzatore, int? idRichiesta = null)
        {
            MyRai_Visualizzazione_Giornate_Da_Segreteria response = null;
            try
            {
                DateTime updateDate = DateTime.Now;

                using (digiGappEntities db = new digiGappEntities())
                {
                    var toUpdate = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where(i => i.Id.Equals(id)).FirstOrDefault();

                    if (toUpdate == null)
                    {
                        throw new Exception("Impossibile aggiornare l'elemento. Elemento non trovato.");
                    }

                    toUpdate.MatricolaVisualizzatore = matricolaVisualizzatore;
                    toUpdate.UtenteVisualizzatore = utenteVisualizzatore;
                    toUpdate.Visualizzato = visualizzato;
                    toUpdate.IdRichiesta = idRichiesta;
                    toUpdate.DataUltimoAccesso = updateDate;

                    db.SaveChanges();

                    response = new MyRai_Visualizzazione_Giornate_Da_Segreteria();
                    response = toUpdate;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return response;
        }

        #endregion

        #region NoteRichieste

        public InserisciNotaRichiestaResponse InserisciNotaRichiesta(string matricola, string nomeUtente, string nota, DateTime giornata, string sedeGapp, string destinatario = "Utente", string tipoMittente = null)
        {
            NoteRichiesteDataController datacontroller = new NoteRichiesteDataController();
            InserisciNotaRichiestaResponse response = new InserisciNotaRichiestaResponse();
            response.Esito = true;

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 6)
                {
                    matricola = matricola.PadLeft(6, '0');
                }
                else if (matricola.Length > 6)
                {
                    if (matricola.Length == 7 && matricola.StartsWith("P"))
                    {
                        matricola = matricola.Substring(1);
                    }
                    else
                    {
                        throw new Exception("Matricola non valida. La lunghezza prevista è 6.");
                    }
                }

                response.Nota = datacontroller.InserisciNotaRichiesta(matricola, nomeUtente, nota, giornata, sedeGapp, destinatario, tipoMittente);
                if (response.Nota == null)
                {
                    throw new Exception( "Si è verificato un errore nell'invio del messaggio" );
                }
                response.Esito = true;
                response.Errore = String.Empty;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.Nota = null;
                string datiChiamata = String.Format(" matricola: {0}, nomeUtente: {1}, nota: {2}, giornata: {3}, sedeGapp: {4}, destinatario: {5}", matricola, nomeUtente, nota, giornata.ToString("dd/MM/yyyy"), sedeGapp, destinatario);
                ScriviLog(ex.ToString(), matricola, "InserisciNotaRichiesta", datiChiamata);
            }

            return response;
        }

        public ModificaNotaRichiestaResponse ModificaNotaRichiesta(int idNota, string matricola, string nota, string tipoMittente = null)
        {
            NoteRichiesteDataController datacontroller = new NoteRichiesteDataController();
            ModificaNotaRichiestaResponse response = new ModificaNotaRichiestaResponse();
            response.Nota = new MyRai_Note_Richieste();

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 6)
                {
                    matricola = matricola.PadLeft(6, '0');
                }
                else if (matricola.Length > 6)
                {
                    if (matricola.Length == 7 && matricola.StartsWith("P"))
                    {
                        matricola = matricola.Substring(1);
                    }
                    else
                    {
                        throw new Exception("Matricola non valida. La lunghezza prevista è 6.");
                    }
                }

                response.Nota = datacontroller.ModificaNotaRichiesta(idNota, matricola, nota, tipoMittente);
                response.Esito = true;
                response.Errore = String.Empty;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.Nota = null;
                string datiChiamata = String.Format(" matricola: {0}, idNota: {1}, nota: {2}", matricola, idNota, nota);
                ScriviLog(ex.ToString(), matricola, "ModificaNotaRichiesta", datiChiamata);
            }

            return response;
        }

        public EliminaNotaRichiestaResponse EliminaNotaRichiesta(int idNota, string matricola)
        {
            NoteRichiesteDataController datacontroller = new NoteRichiesteDataController();
            EliminaNotaRichiestaResponse response = new EliminaNotaRichiestaResponse();
            response.Esito = true;

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 6)
                {
                    matricola = matricola.PadLeft(6, '0');
                }
                else if (matricola.Length > 6)
                {
                    if (matricola.Length == 7 && matricola.StartsWith("P"))
                    {
                        matricola = matricola.Substring(1);
                    }
                    else
                    {
                        throw new Exception("Matricola non valida. La lunghezza prevista è 6.");
                    }
                }

                datacontroller.EliminaNotaRichiesta(idNota, matricola);
                response.Esito = true;
                response.Errore = String.Empty;
            }
            catch (Exception ex)
            {
                response.Tipologia = "FUN";
                if ( !String.IsNullOrEmpty( ex.Message ) )
                {
                    if ( ex.Message.Contains( "Si è verificato un errore. Impossibile rimuovere la nota già letta." ) ||
                        ex.Message.Contains( "Si è verificato un errore. Non si dispone dei diritti necessari per eseguire tale operazione." ) ||
                        ex.Message.Contains( "Si è verificato un errore, nota non trovata." ) )
                    {
                        response.Tipologia = "APP";
                    }
                }

                response.Esito = false;
                response.Errore = ex.Message;
                string datiChiamata = String.Format(" matricola: {0}, idNota: {1}", matricola, idNota);
                ScriviLog(ex.ToString(), matricola, "EliminaNotaRichiesta", datiChiamata);
            }

            return response;
        }

        public SetLetturaResponse SetLettura(int idNota, string matricola, string nominativo, bool letta = true)
        {
            NoteRichiesteDataController datacontroller = new NoteRichiesteDataController();
            SetLetturaResponse response = new SetLetturaResponse();
            response.Esito = true;
            response.Nota = new MyRai_Note_Richieste();

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 6)
                {
                    matricola = matricola.PadLeft(6, '0');
                }
                else if (matricola.Length > 6)
                {
                    if (matricola.Length == 7 && matricola.StartsWith("P"))
                    {
                        matricola = matricola.Substring(1);
                    }
                    else
                    {
                        throw new Exception("Matricola non valida. La lunghezza prevista è 6.");
                    }
                }

                datacontroller.SetLettura(idNota, matricola, nominativo, letta);
                response.Esito = true;
                response.Errore = String.Empty;
                response.Nota = datacontroller.GetNotaRichiesta(idNota, matricola);
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.Nota = null;
                string datiChiamata = String.Format(" matricola: {0}, idNota: {1}, nominativo: {2}", matricola, idNota, nominativo);
                ScriviLog(ex.ToString(), matricola, "ModificaNotaRichiesta", datiChiamata);
            }

            return response;
        }

        public GetNotaRichiestaResponse GetNotaRichiesta(int idNota, string matricola)
        {
            NoteRichiesteDataController datacontroller = new NoteRichiesteDataController();
            GetNotaRichiestaResponse response = new GetNotaRichiestaResponse();
            response.Nota = new MyRai_Note_Richieste();

            try
            {
                response.Nota = datacontroller.GetNotaRichiesta(idNota, matricola);

                response.Esito = true;
                response.Errore = String.Empty;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.Nota = null;
                string datiChiamata = String.Format(" matricola: {0}, idNota: {1}", matricola, idNota);
                ScriviLog(ex.ToString(), matricola, "GetNotaRichiesta", datiChiamata);
            }

            return response;
        }

        public GetNoteRichiesteResponse GetNoteRichieste(string matricola, DateTime giornata)
        {
            NoteRichiesteDataController datacontroller = new NoteRichiesteDataController();
            GetNoteRichiesteResponse response = new GetNoteRichiesteResponse();
            response.Note = new List<MyRai_Note_Richieste>();

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 6)
                {
                    matricola = matricola.PadLeft(6, '0');
                }
                else if (matricola.Length > 6)
                {
                    throw new Exception("Matricola non valida. La lunghezza prevista è 6.");
                }

                response.Note = datacontroller.GetNoteRichieste(matricola, giornata);

                response.Esito = true;
                response.Errore = String.Empty;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.Note = null;
                string datiChiamata = String.Format(" matricola: {0}, giornata: {1}", matricola, giornata.ToString("dd/MM/yyyy"));
                ScriviLog(ex.ToString(), matricola, "GetNoteRichieste", datiChiamata);
            }

            return response;
        }

        public GetNoteRichiesteResponse GetNoteRichiestePerSedeGapp(string matricola, List<string> sedi, string destinatario = "segreteria")
        {
            NoteRichiesteDataController datacontroller = new NoteRichiesteDataController();
            GetNoteRichiesteResponse response = new GetNoteRichiesteResponse();
            response.Note = new List<MyRai_Note_Richieste>();

            try
            {
                response.Note = datacontroller.GetNoteRichiestePerSedeGapp(matricola, sedi, destinatario);
                response.Esito = true;
                response.Errore = String.Empty;
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.Note = null;
                string joined = string.Join("|", sedi);
                string datiChiamata = String.Format(" matricola: {0}, sedi: {1}, destinatario: {2}", matricola, joined, destinatario);
                ScriviLog(ex.ToString(), matricola, "GetNoteRichiestePerSedeGapp", datiChiamata);
            }

            return response;
        }

        public GetNoteRichiesteResponse GetNoteRichiestePerSedeGappFiltrata(string matricola, List<string> sedi, string destinatario = "segreteria", bool onlyNonLette = true)
        {
            NoteRichiesteDataController datacontroller = new NoteRichiesteDataController();
            GetNoteRichiesteResponse response = new GetNoteRichiesteResponse();
            response.Note = new List<MyRai_Note_Richieste>();

            try
            {
                if (onlyNonLette)
                {
                    return this.GetNoteRichiestePerSedeGapp(matricola, sedi, destinatario);
                }
                else
                {
                    response.Note = datacontroller.GetNoteRichiestePerSedeGappFiltrata(matricola, sedi, onlyNonLette, destinatario);
                    response.Esito = true;
                    response.Errore = String.Empty;
                }
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.Note = null;
                string joined = string.Join("|", sedi);
                string datiChiamata = String.Format(" matricola: {0}, sedi: {1}, destinatario: {2}", matricola, joined, destinatario);
                ScriviLog(ex.ToString(), matricola, "GetNoteRichiestePerSedeGapp", datiChiamata);
            }

            return response;
        }



        #endregion

        private void ScriviLog(string message, string matricola, string provenienza, string datiChiamata = null)
        {
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    MyRai_LogErrori log = new MyRai_LogErrori();
                    log.applicativo = "Servizio WCF";
                    log.data = DateTime.Now;
                    log.error_message = (!String.IsNullOrEmpty(datiChiamata)) ? datiChiamata + "\r\n" + message : message;
                    log.matricola = matricola;
                    log.provenienza = provenienza;
                    log.feedback = null;

                    db.MyRai_LogErrori.Add(log);

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static T GetParametro<T>(string chiave)
        {
            using (digiGappEntities db = new digiGappEntities())
            {
                String NomeParametro = chiave;
                MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == NomeParametro);
                if ( p == null )
                    return default( T );
                else
                    return ( T ) Convert.ChangeType( p.Valore1 , typeof( T ) );
            }
        }

        #region Servizi per responsabile

        public GetAnalisiEccezioniResponse GetAnalisiEcc(string matricola, string ecc1, string ecc2, int? anno = null)
        {
            try
            {
                if (anno == null)
                {
                    GetAnalisiEccezioniResponse response = GetAnalisiEccezioni(matricola, new DateTime(DateTime.Now.Year, 1, 1), DateTime.Now, ecc1, ecc2, null);
                    if (response != null && response.DettagliEccezioni != null)
                    {
                        foreach (var d in response.DettagliEccezioni)
                        {
                            d.data = new DateTime(DateTime.Now.Year, d.data.Month, d.data.Day);
                        }
                    }
                    return response;
                }
                else
                {
                    GetAnalisiEccezioniResponse response = GetAnalisiEccezioni(matricola, new DateTime((int)anno, 1, 1), new DateTime((int)anno, 12, 31), ecc1, ecc2, null);

                    if (response != null && response.DettagliEccezioni != null)
                    {
                        foreach (var d in response.DettagliEccezioni)
                        {
                            d.data = new DateTime((int)anno, d.data.Month, d.data.Day);
                        }
                    }
                    return response;
                }
            }
            catch (Exception ex)
            {
                ScriviLog(ex.ToString(), matricola, "GetAnalisiEcc", null);
                return null;
            }
        }

        private Quadratura? GetQuadratura(string tipoDipendente)
        {
            string tipi = GetParametro<String>("TipiDipQuadraturaSettimanale");
            if (tipi != null && tipoDipendente != null && tipi.ToUpper().Contains(tipoDipendente.ToUpper()))
                return Quadratura.Settimanale;

            string tipiGiorn = GetParametro<String>("TipiDipQuadraturaGiornaliera");
            if (tipiGiorn != null && tipoDipendente != null && tipiGiorn.ToUpper().Contains(tipoDipendente.ToUpper()))
                return Quadratura.Giornaliera;

            return null;
        }

        private string CalcolaOreTotali(List<Report_Lista_Item> items)
        {
            int minuti = 0;
            int ore = 0;
            string tempo = "00:00";
            try
            {
            if (items != null && items.Any())
            {
                foreach (var itm in items)
                {
                    minuti += itm.Minuti;
                }

                if (minuti > 60)
                {
                    int tempH = (minuti / 60);
                    ore += tempH;
                    minuti -= (tempH * 60);
                }

                if (minuti == 60)
                {
                    ore += 1;
                    minuti = 0;
                }

                tempo = String.Format("{0:00}:{1:00}", ore, minuti);
            }
            }
            catch (Exception ex)
            {
                ScriviLog(ex.ToString(), "", "CalcolaOreTotali", null);
            }

            return tempo;
        }

        private string CalcolaOreTotali(List<ItemCarMagPresenza> items, string tipo)
        {
            int ore = 0;
            int minuti = 0;
            int tempOre = 0;
            int tempMinuti = 0;

            string tempo = "00:00";

            if (items != null && items.Any())
            {

                List<ItemCarMagPresenza> list = new List<ItemCarMagPresenza>();

                if (tipo == "CAR")
                {
                    foreach (var itm in items)
                    {
                        tempOre = Int32.Parse(itm.CAR.Trim().Substring(0, 2));
                        tempMinuti = Int32.Parse(itm.CAR.Trim().Substring(3, 2));

                        ore += tempOre;
                        minuti += tempMinuti;
                    }
                }
                else
                {
                    foreach (var itm in items)
                    {
                        tempOre = Int32.Parse(itm.MP.Trim().Substring(0, 2));
                        tempMinuti = Int32.Parse(itm.MP.Trim().Substring(3, 2));

                        ore += tempOre;
                        minuti += tempMinuti;
                    }
                }

                if (minuti > 60)
                {
                    int tempH = (minuti / 60);
                    ore += tempH;
                    minuti -= (tempH * 60);
                }

                if (minuti == 60)
                {
                    ore += 1;
                    minuti = 0;
                }

                tempo = String.Format("{0:00}:{1:00}", ore, minuti);
            }

            return tempo;
        }

        private List<Report_Lista_Item> GetFilteredDettaglioEccezioni(string matricola, int mese, int anno, string ecc)
        {
            GetAnalisiEccezioniResponse r = null;

            if (ecc.ToUpper().Equals("POH") || ecc.ToUpper().Equals("ROH"))
            {
                r = GetAnalisiEcc(matricola, "POH", "ROH", anno);
            }
            else if (ecc.ToUpper().Equals("STR") || ecc.ToUpper().Equals("STRF"))
            {
                r = GetAnalisiEcc(matricola, "STR", "STRF", anno);
            }
            else if (ecc.ToUpper().Equals("RE20") || ecc.ToUpper().Equals("RE22") || ecc.ToUpper().Equals("RE25"))
            {
                r = GetAnalisiEcc(matricola, ecc, "", anno);
            }
            else
            {
                r = GetAnalisiEcc(matricola, ecc, "", anno);
            }

            List<Report_Lista_Item> results = r.DettagliEccezioni.Where(x => x.data.Month == mese && x.eccezione == ecc && x.data.Year == anno)
                .Select(x => new Report_Lista_Item
                {
                    Giorno = x.data,
                    Minuti = x.minuti
                }).ToList();

            return results;
        }

        private string GetSaldoMesePrecedente(string matricola, int mese, int anno, string ecc)
        {
            GetAnalisiEccezioniResponse r = null;
            try
            {
            if (ecc.ToUpper().Equals("POH") || ecc.ToUpper().Equals("ROH"))
            {
                r = GetAnalisiEcc(matricola, "POH", "ROH", anno);

                List<Report_Lista_Item> results = r.DettagliEccezioni.Where(x => x.data.Month < mese && x.eccezione == ecc && x.data.Year == anno).Select(x => new Report_Lista_Item
                {
                    Giorno = x.data,
                    Minuti = x.minuti
                }).ToList();

                return CalcolaOreTotali(results);
            }
            }
            catch (Exception ex)
            {
                ScriviLog(ex.ToString(), matricola, "GetSaldoMesePrecedente", String.Format("matricola: {0}, mese: {1}, anno: {2}, eccezione: {3}", matricola, mese, anno, ecc));
            }

            return "00:00";
        }

        /// <summary>
        /// Converte una stringa in formato +/- hh:mm in un timespan
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private TimeSpan ConvertiStringToTimeSpan(string t)
        {
            TimeSpan spanT1 = new TimeSpan(0, 0, 0, 0, 0);
            bool negativo = false;

            try
            {
                List<string> splittata = t.Split(':').ToList();

                if (splittata.Count() != 2)
                {
                    throw new Exception("Lunghezza anomala");
                }

                string ore = splittata[0];
                string minuti = splittata[1];

                if (ore.Contains("-"))
                {
                    negativo = true;
                    ore = ore.Replace("-", "");
                }

                int oreT1 = Int32.Parse(ore);
                int minutiT1 = Int32.Parse(minuti);

                spanT1 = new TimeSpan(0, oreT1, minutiT1, 0, 0);

                if (negativo)
                {
                    spanT1 = spanT1.Negate();
                }
            }
            catch (Exception ex)
            {
                ScriviLog(ex.ToString(), "", "ConvertiStringToTimeSpan", String.Format("t: {0}", t));
            }

            return spanT1;
        }

        private string CalcolaSaldo(string t1, string t2)
        {
            string result = "";

            try
            {
                #region primo TimeSpan
                TimeSpan spanT1 = ConvertiStringToTimeSpan(t1);
                #endregion

                #region secondo TimeSpan
                TimeSpan spanT2 = ConvertiStringToTimeSpan(t2);
                #endregion

                TimeSpan differenza = spanT1.Subtract(spanT2);

                result = String.Format("{0:00}:{1:00}", (differenza.Hours + (differenza.Days * 24)), differenza.Minutes);

                if (result.Contains('-'))
                {
                    result = result.Replace("-", "");
                    result = String.Format("-{0}", result);
                }
            }
            catch (Exception ex)
            {
                ScriviLog(ex.ToString(), "", "CalcolaSaldo", String.Format("t1: {0}, t2: {1}", t1, t2));
            }

            return result;
        }

        //private string CalcolaSaldoSTRSTRF(string t1, string t2)
        //{
        //    string result = "";

        //    try
        //    {
        //        #region primo TimeSpan
        //        TimeSpan spanT1 = ConvertiStringToTimeSpan( t1 );
        //        #endregion

        //        #region secondo TimeSpan
        //        TimeSpan spanT2 = ConvertiStringToTimeSpan( t2 );
        //        #endregion

        //        TimeSpan differenza = spanT1.Add(spanT2);

        //        result = String.Format("{0:00}:{1:00}", (differenza.Hours + (differenza.Days * 24)), differenza.Minutes);

        //        if (result.Contains('-'))
        //        {
        //            result = result.Replace("-", "");
        //            result = String.Format("-{0}", result);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ScriviLog( ex.ToString( ) , "" , "CalcolaSaldoSTRSTRF" , String.Format( "t1: {0}, t2: {1}" , t1 , t2 ) );
        //    }

        //    return result;
        //}

        private string SommaTempo(string t1, string t2)
        {
            string result = "";

            try
            {
                #region primo TimeSpan
                TimeSpan spanT1 = ConvertiStringToTimeSpan(t1);
                #endregion

                #region secondo TimeSpan
                TimeSpan spanT2 = ConvertiStringToTimeSpan(t2);
                #endregion

                TimeSpan somma = spanT1.Add(spanT2);

                result = String.Format("{0:00}:{1:00}", (somma.Hours + (somma.Days * 24)), somma.Minutes);

                if (result.Contains('-'))
                {
                    result = result.Replace("-", "");
                    result = String.Format("-{0}", result);
                }
            }
            catch (Exception ex)
            {
                ScriviLog(ex.ToString(), "", "SommaTempo", String.Format("t1: {0}, t2: {1}", t1, t2));
            }

            return result;
        }

        private int CalcolaOccorrenze(List<Report_Lista_Item> items)
        {
            int count = 0;
            if (items != null && items.Any())
            {
                count = items.Where(i => i.Minuti > 0).Count();
            }

            return count;
        }

        private string ConvertiTempoInStringa(int ore, int minuti)
        {
            if (minuti > 60)
            {
                int temp = (minuti / 60);
                ore += temp;
                minuti -= (temp * 60);
            }
            else if (minuti == 60)
            {
                ore += 1;
                minuti = 0;
            }

            return String.Format("{0:00}:{1:00}", ore, minuti);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matricola">Matricola chiamante</param>
        /// <param name="sede">Sede di cui si intende ottenere le info</param>
        /// <param name="mese">Mese di riferimento</param>
        public Report_POH_ROH_Response GetReport_POH_ROH ( string matricola , string sede , int mese , int anno )
        {
            Report_POH_ROH_Response result = new Report_POH_ROH_Response( );
            List<Report_Item> rList = new List<Report_Item>( );
            DateTime dt = new DateTime( anno , mese , 1 );

            try
            {
                var resp = this.presenzeGiornaliere( matricola , sede , dt.ToString( "ddMMyyyy" ) );

                if ( resp.esito == false && !String.IsNullOrEmpty( resp.errore ) )
                {
                    result.Esito = false;
                    result.Errore = resp.errore;
                    result.Risposta = null;
                    return result;
                }

                if ( resp.dati == null || resp.dati.Count( ) == 0 )
                {
                    result.Esito = true;
                    result.Errore = "Non ci sono dati disponibili";
                    result.Risposta = null;
                    return result;
                }

                foreach ( var item in resp.dati )
                {
                    var quad = GetQuadratura( item.tipoDip );

                    if ( quad != null )
                    {
                        if ( quad.GetValueOrDefault( ) == Quadratura.Giornaliera )
                        {
                            string saldoPOHPrecedente = GetSaldoMesePrecedente( item.matricola , mese , anno , "POH" );
                            string saldoROHPrecedente = GetSaldoMesePrecedente( item.matricola , mese , anno , "ROH" );

                            // saldo dei poh precedenti
                            string saldoPrecedente = CalcolaSaldo( saldoROHPrecedente , saldoPOHPrecedente );
                            string saldoPOH = CalcolaSaldo( saldoPOHPrecedente , saldoROHPrecedente );


                            DateTime dtEnd = dt.AddMonths( 1 ).AddDays( -1 );

                            var presenzeMese = GetSchedaPresenzeMese( ( item.matricola.Length > 6 ) ? item.matricola.Substring( 1 ) : item.matricola , dt , dtEnd );

                            List<Report_Lista_Item> listPOH = new List<Report_Lista_Item>( );
                            List<Report_Lista_Item> listROH = new List<Report_Lista_Item>( );
                            List<Report_Lista_Item> listCAR = new List<Report_Lista_Item>( );

                            if ( presenzeMese.esito )
                            {
                                if ( presenzeMese.Giorni != null &&
                                    presenzeMese.Giorni.Any( ) )
                                {
                                    foreach ( var giorno in presenzeMese.Giorni )
                                    {
                                        if ( giorno.MicroAssenze != null && giorno.MicroAssenze.Any( ) )
                                        {
                                            foreach ( var ma in giorno.MicroAssenze )
                                            {
                                                if ( !String.IsNullOrEmpty( ma.nome ) && ma.nome.ToUpper( ).Contains( "POH" ) )
                                                {
                                                    string[] _temp = ma.quantita.Split( '.' );

                                                    string ore = _temp[0];
                                                    string min = _temp[1];

                                                    int hh = int.Parse( ore );
                                                    int minuti = int.Parse( min );

                                                    if ( hh > 1 )
                                                    {
                                                        // converti in minuti
                                                        int tempM = ( hh * 60 );
                                                        // somma i minuti
                                                        minuti += tempM;
                                                        // sottrai alle ore i minuti appena convertiti
                                                        hh -= ( tempM / 60 );
                                                    }

                                                    if ( hh == 1 )
                                                    {
                                                        // converti in minuti
                                                        int tempM = 60;
                                                        // somma i minuti
                                                        minuti += tempM;
                                                        // sottrai alle ore i minuti appena convertiti
                                                        hh = 0;
                                                    }

                                                    listPOH.Add( new Report_Lista_Item( )
                                                    {
                                                        Giorno = giorno.data ,
                                                        Minuti = minuti
                                                    } );
                                                }

                                                if ( !String.IsNullOrEmpty( ma.nome ) && ma.nome.ToUpper( ).Contains( "ROH" ) )
                                                {
                                                    string[] _temp = ma.quantita.Split( '.' );

                                                    string ore = _temp[0];
                                                    string min = _temp[1];

                                                    int hh = int.Parse( ore );
                                                    int minuti = int.Parse( min );

                                                    if ( hh > 1 )
                                                    {
                                                        // converti in minuti
                                                        int tempM = ( hh * 60 );
                                                        // somma i minuti
                                                        minuti += tempM;
                                                        // sottrai alle ore i minuti appena convertiti
                                                        hh -= ( tempM / 60 );
                                                    }

                                                    if ( hh == 1 )
                                                    {
                                                        // converti in minuti
                                                        int tempM = 60;
                                                        // somma i minuti
                                                        minuti += tempM;
                                                        // sottrai alle ore i minuti appena convertiti
                                                        hh = 0;
                                                    }

                                                    listROH.Add( new Report_Lista_Item( )
                                                    {
                                                        Giorno = giorno.data ,
                                                        Minuti = minuti
                                                    } );
                                                }

                                                if ( !String.IsNullOrEmpty( ma.nome ) && ma.nome.ToUpper( ).Contains( "CAR" ) )
                                                {
                                                    string[] _temp = ma.quantita.Split( '.' );

                                                    string ore = _temp[0];
                                                    string min = _temp[1];

                                                    int hh = int.Parse( ore );
                                                    int minuti = int.Parse( min );

                                                    if ( hh > 1 )
                                                    {
                                                        // converti in minuti
                                                        int tempM = ( hh * 60 );
                                                        // somma i minuti
                                                        minuti += tempM;
                                                        // sottrai alle ore i minuti appena convertiti
                                                        hh -= ( tempM / 60 );
                                                    }

                                                    if ( hh == 1 )
                                                    {
                                                        // converti in minuti
                                                        int tempM = 60;
                                                        // somma i minuti
                                                        minuti += tempM;
                                                        // sottrai alle ore i minuti appena convertiti
                                                        hh = 0;
                                                    }

                                                    listCAR.Add( new Report_Lista_Item( )
                                                    {
                                                        Giorno = giorno.data ,
                                                        Minuti = minuti
                                                    } );
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                            else
                            {
                                result.Esito = false;
                                result.Errore = presenzeMese.errore;
                                result.Risposta = null;
                                return result;
                            }
                            //List<Report_Lista_Item> list1 = GetFilteredDettaglioEccezioni(item.matricola, mese, anno, "POH");
                            //List<Report_Lista_Item> list2 = GetFilteredDettaglioEccezioni(item.matricola, mese, anno, "ROH");

                            string totOre1 = CalcolaOreTotali( listPOH );
                            string totOre2 = CalcolaOreTotali( listROH );
                            string totOre3 = CalcolaOreTotali( listCAR );

                            // aggiunge il saldo poh al conteggio dei poh del mese
                            string totaleConSaldoPOH = SommaTempo( saldoPOH , totOre1 );

                            rList.Add( new Report_Item( )
                            {
                                Matricola = item.matricola ,
                                ListaPOH = listPOH ,
                                ListaROH = listROH ,
                                ListaCAR = listCAR ,
                                Nominativo = item.nominativo ,
                                TotaleOreLista1 = totOre1 ,
                                TotaleOreLista2 = totOre2 ,
                                TotaleOreLista3 = totOre3 ,
                                Saldo = CalcolaSaldo( totOre2 , totaleConSaldoPOH ) ,
                                SaldoPOHPrecedente = saldoPrecedente ,
                                NumeroOccorrenze = CalcolaOccorrenze( listPOH )
                            } );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                ScriviLog( ex.ToString( ) , matricola , "GetReport_POH_ROH" , null );
                result.Esito = false;
                result.Errore = ex.ToString( );
                result.Risposta = null;
                return result;
            }

            result.Esito = true;
            result.Errore = null;
            result.Risposta = new List<Report_Item>( );
            result.Risposta = rList;

            return result;
        }

        //public Report_POH_ROH_Response GetReport_POH_ROH ( string matricola , string sede , int mese , int anno )
        //{
        //    Report_POH_ROH_Response result = new Report_POH_ROH_Response( );
        //    List<Report_Item> rList = new List<Report_Item>( );
        //    DateTime dt = new DateTime( anno , mese , 1 );

        //    try
        //    {
        //        var resp = this.presenzeGiornaliere( matricola , sede , dt.ToString( "ddMMyyyy" ) );

        //        if ( resp.esito == false && !String.IsNullOrEmpty( resp.errore ) )
        //        {
        //            result.Esito = false;
        //            result.Errore = resp.errore;
        //            result.Risposta = null;
        //            return result;
        //        }

        //        if ( resp.dati == null || resp.dati.Count( ) == 0 )
        //        {
        //            result.Esito = true;
        //            result.Errore = "Non ci sono dati disponibili";
        //            result.Risposta = null;
        //            return result;
        //        }

        //        foreach ( var item in resp.dati )
        //        {
        //            var quad = GetQuadratura( item.tipoDip );

        //            if ( quad != null )
        //            {
        //                if ( quad.GetValueOrDefault( ) == Quadratura.Giornaliera )
        //                {

        //                    string saldoPOHPrecedente = GetSaldoMesePrecedente( item.matricola , mese , anno , "POH" );
        //                    string saldoROHPrecedente = GetSaldoMesePrecedente( item.matricola , mese , anno , "ROH" );

        //                    // saldo dei poh precedenti
        //                    string saldoPrecedente = CalcolaSaldo( saldoROHPrecedente , saldoPOHPrecedente );
        //                    string saldoPOH = CalcolaSaldo( saldoPOHPrecedente , saldoROHPrecedente );

        //                    List<Report_Lista_Item> list1 = GetFilteredDettaglioEccezioni( item.matricola , mese , anno , "POH" );
        //                    List<Report_Lista_Item> list2 = GetFilteredDettaglioEccezioni( item.matricola , mese , anno , "ROH" );

        //                    string totOre1 = CalcolaOreTotali( list1 );
        //                    string totOre2 = CalcolaOreTotali( list2 );

        //                    // aggiunge il saldo poh al conteggio dei poh del mese
        //                    string totaleConSaldoPOH = SommaTempo( saldoPOH , totOre1 );

        //                    rList.Add( new Report_Item( )
        //                    {
        //                        Matricola = item.matricola ,
        //                        ListaPOH = list1 ,
        //                        ListaROH = list2 ,
        //                        ListaCAR = null ,
        //                        Nominativo = item.nominativo ,
        //                        TotaleOreLista1 = totOre1 ,
        //                        TotaleOreLista2 = totOre2 ,
        //                        Saldo = CalcolaSaldo( totOre2 , totaleConSaldoPOH ) ,
        //                        SaldoPOHPrecedente = saldoPrecedente ,
        //                        NumeroOccorrenze = CalcolaOccorrenze( list1 )
        //                    } );
        //                }
        //            }
        //        }
        //    }
        //    catch ( Exception ex )
        //    {
        //        ScriviLog( ex.ToString( ) , matricola , "GetReport_POH_ROH" , null );
        //        result.Esito = false;
        //        result.Errore = ex.ToString( );
        //        result.Risposta = null;
        //        return result;
        //    }

        //    result.Esito = true;
        //    result.Errore = null;
        //    result.Risposta = new List<Report_Item>( );
        //    result.Risposta = rList;

        //    return result;
        //}

        public Report_EccezioniGiornalisti_Response GetLista_Eccezioni_Giornalisti(string matricola, string sede, int mese, int anno)
        {
            List<Report_Lista_ItemGiornalisti> listaItemGiornalisti = new List<Report_Lista_ItemGiornalisti>();
            List<Report_ItemGiornalisti> listaGiornalisti = new List<Report_ItemGiornalisti>();
            Report_EccezioniGiornalisti_Response result = new Report_EccezioniGiornalisti_Response();
            List<string> ListaCodEccGiornalisti = new List<string>();

            using (digiGappEntities db = new digiGappEntities())
            {
                ListaCodEccGiornalisti = (
                    from Eccezioni in db.MyRai_Eccezioni_Ammesse
                    where Eccezioni.TipiDipendente == "G"
                    && Eccezioni.id_raggruppamento == 2
                    select Eccezioni.cod_eccezione).ToList();
            }

            List<Report_Item> rList = new List<Report_Item>();
            DateTime dt = new DateTime(anno, mese, 1);

            try
            {
                var resp = this.presenzeGiornaliere(matricola, sede, dt.ToString("ddMMyyyy"));

                if (resp.esito == false && !String.IsNullOrEmpty(resp.errore))
                {
                    result.Esito = false;
                    result.Errore = resp.errore;
                    result.Risposta = null;
                    return result;
                }

                if (resp.dati == null || resp.dati.Count() == 0)
                {
                    result.Esito = true;
                    result.Errore = "Non ci sono dati disponibili";
                    result.Risposta = null;
                    return result;
                }
                foreach (var item in resp.dati)
                {
                    if ( item.tipoDip == "G" ) //Tipo Giornalista
                    {
                        string totOre1 = "00:00";
                        listaItemGiornalisti = new List<Report_Lista_ItemGiornalisti>( );

                        foreach ( var i in ListaCodEccGiornalisti )
                        {
                            List<Report_Lista_Item> list1 = GetFilteredDettaglioEccezioni( item.matricola , mese , anno , i );
                            
                            Report_Lista_ItemGiornalisti toADD = new Report_Lista_ItemGiornalisti( );

                            toADD.Eccezione = i;
                            toADD.Totale = list1.Count( );
                            if ( list1 != null && list1.Any( ) )
                            {
                                toADD.Giorni = new List<Report_Lista_ItemGiornalisti_Day>( );
                                foreach ( var l in list1 )
                                {
                                    int min = l.Minuti;
                                    int ore = 0;

                                    if ( min > 60 )
                                    {
                                        int tempH = ( min / 60 );
                                        ore += tempH;
                                        min -= ( tempH * 60 );
                                    }

                                    if ( min == 60 )
                                    {
                                        ore += 1;
                                        min = 0;
                                    }

                                    string tempo = String.Format( "{0:00}:{1:00}" , ore , min );

                                    toADD.Giorni.Add( new Report_Lista_ItemGiornalisti_Day( )
                                    {
                                        Giorno = l.Giorno ,
                                        Minuti = tempo
                                    } );
                                }
                                totOre1 = CalcolaOreTotali( list1 );
                            }

                            toADD.Minuti = totOre1;
                            listaItemGiornalisti.Add( toADD );
                        }

                        listaGiornalisti.Add( new Report_ItemGiornalisti
                        {
                            Matricola = item.matricola ,
                            Nominativo = item.nominativo ,
                            ListaItemGiornalisti = listaItemGiornalisti ,
                        } );
                    }
                }
            }
            catch (Exception ex)
            {
                ScriviLog(ex.ToString(), matricola, "GetList_Eccezioni_Giornalisti", null);
                result.Esito = false;
                result.Errore = ex.ToString();
                result.Risposta = null;
                return result;
            }

            result.Esito = true;
            result.Errore = null;
            result.Risposta = new List<Report_ItemGiornalisti>();
            result.Risposta = listaGiornalisti;

            return result;
        }

        public Report_STSE_Response GetReport_STSE ( string matricola , string sede , int mese , int anno , bool inizioMese = true )
        {
            Report_STSE_Response result = new Report_STSE_Response( );
            result.Risposta = new List<Report_STSE_Risposta>( );
            List<Report_Item> rList = new List<Report_Item>( );
            DateTime dt = new DateTime( anno , mese , 1 );
            DateTime dtStart = new DateTime( anno , mese , 1 );
            DateTime dtEnd = dtStart.AddMonths( 1 );
            dtEnd = dtEnd.AddDays( -1 );

            try
            {
                var resp = this.presenzeGiornaliere( matricola , sede , dt.ToString( "ddMMyyyy" ) );

                if ( resp.esito == false && !String.IsNullOrEmpty( resp.errore ) )
                {
                    result.Esito = false;
                    result.Errore = resp.errore;
                    result.Risposta = null;
                    return result;
                }

                if ( resp.dati == null || resp.dati.Count( ) == 0 )
                {
                    result.Esito = true;
                    result.Errore = "Non ci sono dati disponibili";
                    result.Risposta = null;
                    return result;
                }

                // se il campo inizioMese è false
                // vuol dire che devono essere calcolate le settimane a partire dal Lunedì
                // quindi se ad esempio il primo del mese è un mercoledì, vanno presi anche i 2
                // giorni del mese precedente
                if ( !inizioMese )
                {
                    bool inizioAltroGiorno = false;

                    // va reperita la data di inizio dal db L2D_SEDE_GAPP
                    // se il campo giorno_inizio_settimana
                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        var sedeGapp = db.L2D_SEDE_GAPP.Where( w => w.cod_sede_gapp.Equals( sede ) ).FirstOrDefault( );
                        if ( sedeGapp != null )
                        {
                            if ( sedeGapp.giorno_inizio_settimana != null )
                            {
                                inizioAltroGiorno = true;
                                int giorno = sedeGapp.giorno_inizio_settimana.GetValueOrDefault( );
                                // sul DB viene impostato 7 per indicare la domenica, mentre c# la domenica la indica con lo 0
                                if ( giorno == 7 )
                                {
                                    giorno = 0;
                                }
                                DayOfWeek day = ( ( DayOfWeek ) giorno );
                                giorno = ( giorno == 0 ? 6 : giorno - 1 );
                                DayOfWeek day2 = ( ( DayOfWeek ) giorno );

                                if ( dtStart.DayOfWeek != day )
                                {
                                    do
                                    {
                                        dtStart = dtStart.AddDays( -1 );
                                    } while ( dtStart.DayOfWeek != day );
                                }

                                if ( dtEnd.DayOfWeek != day2 )
                                {
                                    do
                                    {
                                        dtEnd = dtEnd.AddDays( 1 );
                                    } while ( dtEnd.DayOfWeek != day2 );
                                }
                            }
                        }
                    }

                    if ( dtStart.DayOfWeek != DayOfWeek.Monday && !inizioAltroGiorno )
                    {
                        do
                        {
                            dtStart = dtStart.AddDays( -1 );
                        } while ( dtStart.DayOfWeek != DayOfWeek.Monday );
                    }

                    if ( dtEnd.DayOfWeek != DayOfWeek.Sunday && !inizioAltroGiorno )
                    {
                        do
                        {
                            dtEnd = dtEnd.AddDays( 1 );
                        } while ( dtEnd.DayOfWeek != DayOfWeek.Sunday );
                    }
                }

                dt = dtStart;

                // per ogni dipendente della sede
                foreach ( var item in resp.dati )
                {                    
                    var quad = GetQuadratura( item.tipoDip );

                    if ( quad != null )
                    {
                        // verifica se la quadratura è settimanale e se il tipo utente è L
                        if ( quad.GetValueOrDefault( ) == Quadratura.Settimanale && (item.tipoDip.ToUpper( ).Equals( "L" ) || item.tipoDip.ToUpper( ).Equals( "2" ) ))
                        {
                            // prepara l'oggetto da inserire nella lista di output
                            Report_STSE_Risposta toAdd = new Report_STSE_Risposta( );
                            toAdd.Giorni = new List<Report_STSE_Giorno_Item>( );
                            toAdd.Settimane = new List<Report_STSE_Settimana_Item>( );
                            toAdd.Matricola = item.matricola;
                            toAdd.Nominativo = item.nominativo;
                            toAdd.Ore = 0;
                            toAdd.Minuti = 0;
                            toAdd.TempoISO8601 = "";

                            // reperimento delle eccezioni di tipo STSE, per la matricola in esame per il mese richiesto
                            List<Report_Lista_Item> list1 = GetFilteredDettaglioEccezioni( item.matricola , mese , anno , "STSE" );

                            int tempMinuti = 0;
                            int conteggioGiornate = 1;
                            int settimana = 1;
                            DateTime dtMarkUp1 = dtStart;
                            DateTime dtMarkUp2 = dtEnd;

                            while ( dtStart.Date <= dtEnd.Date )
                            {
                                Report_STSE_Giorno_Item giorno = new Report_STSE_Giorno_Item( )
                                {
                                    Giorno = dtStart ,
                                    Minuti = 0 ,
                                    Ore = 0 ,
                                    TempoISO8601 = "00:00"
                                };

                                var giornata = list1.Where( g => g.Giorno == dtStart ).FirstOrDefault( );
                                if (giornata != null)
                                {
                                    // 00:00
                                    string t1 = ConvertiTempoInStringa( 0 , giornata.Minuti );
                                    TimeSpan t2 = ConvertiStringToTimeSpan( t1 );
                                    giorno.TempoISO8601 = t1;
                                    giorno.Ore = t2.Hours;
                                    giorno.Minuti = t2.Minutes;
                                    tempMinuti += giornata.Minuti;
                                }

                                toAdd.Giorni.Add( giorno );

                                // se è terminata la settimana oppure 
                                // è l'ultimo giorno del mese
                                if ( conteggioGiornate == 7 ||
                                    dtStart.Date == dtEnd.Date )
                                {
                                    conteggioGiornate = 1;
                                    
                                    string t1 = ConvertiTempoInStringa( 0 , tempMinuti );
                                    TimeSpan t2 = ConvertiStringToTimeSpan( t1 );

                                    Report_STSE_Settimana_Item sett = new Report_STSE_Settimana_Item( )
                                    {
                                        NumeroSettimana = settimana ,
                                        Minuti = t2.Minutes ,
                                        Ore = t2.Hours,
                                        TempoISO8601 = t1,
                                        DataInizio = dtMarkUp1 ,
                                        DataFine = dtStart
                                    };

                                    settimana++;
                                    tempMinuti = 0;
                                    dtMarkUp1 = dtStart;
                                    dtMarkUp1 = dtMarkUp1.AddDays( 1 );

                                    toAdd.Settimane.Add( sett );
                                }
                                else
                                {
                                    conteggioGiornate++;
                                }
                                dtStart = dtStart.AddDays( 1 );
                            }

                            result.Risposta.Add( toAdd );
                            dtStart = dt;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                ScriviLog( ex.ToString( ) , matricola , "GetReport_STSE" , null );
                result.Esito = false;
                result.Errore = ex.ToString( );
                result.Risposta = null;
                return result;
            }

            result.Esito = true;
            result.Errore = null;

            return result;
        }


        public Report_STR_STRF_Response GetReport_STR_STRF(string matricola, string sede, int mese, int anno)
        {
            Report_STR_STRF_Response result = new Report_STR_STRF_Response();
            List<Report_Item> rList = new List<Report_Item>();
            DateTime dt = new DateTime(anno, mese, 1);

            try
            {
                var resp = this.presenzeGiornaliere(matricola, sede, dt.ToString("ddMMyyyy"));

                if (resp.esito == false && !String.IsNullOrEmpty(resp.errore))
                {
                    result.Esito = false;
                    result.Errore = resp.errore;
                    result.Risposta = null;
                    return result;
                }

                if (resp.dati == null || resp.dati.Count() == 0)
                {
                    result.Esito = true;
                    result.Errore = "Non ci sono dati disponibili";
                    result.Risposta = null;
                    return result;
                }

                foreach (var item in resp.dati)
                {
                    var quad = GetQuadratura(item.tipoDip);

                    if (quad != null)
                    {
                        if (quad.GetValueOrDefault() == Quadratura.Giornaliera)
                        {
                            List<Report_Lista_Item> list1 = GetFilteredDettaglioEccezioni(item.matricola, mese, anno, "STR");
                            List<Report_Lista_Item> list2 = GetFilteredDettaglioEccezioni(item.matricola, mese, anno, "STRF");

                            string totOre1 = CalcolaOreTotali(list1);
                            string totOre2 = CalcolaOreTotali(list2);

                            rList.Add(new Report_Item()
                            {
                                Matricola = item.matricola,
                                ListaSTR = list1,
                                ListaSTRF = list2,
                                Nominativo = item.nominativo,
                                TotaleOreLista1 = totOre1,
                                TotaleOreLista2 = totOre2,
                                //Saldo = CalcolaSaldoSTRSTRF(totOre1, totOre2),
                                Saldo = SommaTempo(totOre1, totOre2),
                                NumeroOccorrenze = CalcolaOccorrenze(list1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ScriviLog(ex.ToString(), matricola, "GetReport_STR_STRF", null);
                result.Esito = false;
                result.Errore = ex.ToString();
                result.Risposta = null;
                return result;
            }

            result.Esito = true;
            result.Errore = null;
            result.Risposta = new List<Report_Item>();
            result.Risposta = rList;

            return result;
        }

        public Report_Reperibilita_Response GetReport_Reperibilita(string matricola, string sede, int mese, int anno)
        {
            Report_Reperibilita_Response result = new Report_Reperibilita_Response();
            List<Report_Item> rList = new List<Report_Item>();
            DateTime dt = new DateTime(anno, mese, 1);
            try
            {
                var resp = this.presenzeGiornaliere(matricola, sede, dt.ToString("ddMMyyyy"));

                if (resp.esito == false && !String.IsNullOrEmpty(resp.errore))
                {
                    result.Esito = false;
                    result.Errore = resp.errore;
                    result.Risposta = null;
                    return result;
                }

                if (resp.dati == null || resp.dati.Count() == 0)
                {
                    result.Esito = true;
                    result.Errore = "Non ci sono dati disponibili";
                    result.Risposta = null;
                    return result;
                }

                foreach (var item in resp.dati)
                {
                    List<Report_Lista_Item> listRE20 = GetFilteredDettaglioEccezioni( item.matricola , mese , anno , "RE20" );
                    List<Report_Lista_Item> listRE22 = GetFilteredDettaglioEccezioni( item.matricola , mese , anno , "RE22" );
                    List<Report_Lista_Item> listRE25 = GetFilteredDettaglioEccezioni( item.matricola , mese , anno , "RE25" );

                    string totOreRE20 = CalcolaOreTotali( listRE20 );
                    string totOreRE22 = CalcolaOreTotali( listRE22 );
                    string totOreRE25 = CalcolaOreTotali( listRE25 );

                    rList.Add( new Report_Item( )
                    {
                        Matricola = item.matricola ,
                        ListaRE20 = listRE20 ,
                        ListaRE22 = listRE22 ,
                        ListaRE25 = listRE25 ,
                        Nominativo = item.nominativo ,
                        TotaleOreLista1 = totOreRE20 ,
                        TotaleOreLista2 = totOreRE22 ,
                        TotaleOreLista3 = totOreRE25 ,
                        Saldo = "" ,
                        NumeroOccorrenze = 0,
                        NumeroOccorrenze20 = ( listRE20 != null && listRE20.Any( ) ) ? listRE20.Count( ) : 0 ,
                        NumeroOccorrenze22 = ( listRE22 != null && listRE22.Any( ) ) ? listRE22.Count( ) : 0 ,
                        NumeroOccorrenze25 = ( listRE25 != null && listRE25.Any( ) ) ? listRE25.Count( ) : 0 ,
                    } );
                }
            }
            catch (Exception ex)
            {
                ScriviLog(ex.ToString(), matricola, "Report_Reperibilita", null);
                result.Esito = false;
                result.Errore = ex.ToString();
                result.Risposta = null;
                return result;
            }

            result.Esito = true;
            result.Errore = null;
            result.Risposta = new List<Report_Item>();
            result.Risposta = rList;

            return result;
        }

        //public Report_Carenza_MP_Response GetReport_Carenza_MP(string matricola, string sede, int mese, int anno, bool inizioMese = true)
        //{
        //    Report_Carenza_MP_Response result = new Report_Carenza_MP_Response();
        //    DateTime dt = new DateTime(anno, mese, 1);
        //    result.Risposta = new List<Report_Item>();

        //    List<Report_Item> rList = new List<Report_Item>();

        //    try
        //    {
        //        var resp = this.presenzeGiornaliere(matricola, sede, dt.ToString("ddMMyyyy"));

        //        if (resp.esito == false && !String.IsNullOrEmpty(resp.errore))
        //        {
        //            result.Esito = false;
        //            result.Errore = resp.errore;
        //            result.Risposta = null;
        //            return result;
        //        }

        //        if (resp.dati == null || resp.dati.Count() == 0)
        //        {
        //            result.Esito = true;
        //            result.Errore = "Non ci sono dati disponibili";
        //            result.Risposta = null;
        //            return result;
        //        }

        //        foreach (var item in resp.dati)
        //        {
        //            var quad = GetQuadratura(item.tipoDip);

        //            if (quad.GetValueOrDefault() != Quadratura.Settimanale)
        //            {
        //                continue;
        //            }

        //            DateTime dtStart = new DateTime( anno , mese , 1 );
        //            DateTime dtEnd = dtStart.AddMonths( 1 );
        //            dtEnd = dtEnd.AddDays( -1 );

        //            // se il campo inizioMese è false
        //            // vuol dire che devono essere calcolate le settimane a partire dal Lunedì
        //            // quindi se ad esempio il primo del mese è un mercoledì, vanno presi anche i 2
        //            // giorni del mese precedente
        //            if ( !inizioMese )
        //            {
        //                if ( dtStart.DayOfWeek != DayOfWeek.Monday )
        //                {
        //                    do
        //                    {
        //                        dtStart = dtStart.AddDays( -1 );
        //                    } while ( dtStart.DayOfWeek != DayOfWeek.Monday );
        //                }

        //                if ( dtEnd.DayOfWeek != DayOfWeek.Sunday )
        //                {
        //                    do
        //                    {
        //                        dtEnd = dtEnd.AddDays( 1 );
        //                    } while ( dtEnd.DayOfWeek != DayOfWeek.Sunday );
        //                }
        //            }

        //            // se la richiesta è per l'anno ed il mese corrente
        //            // allora la data fine sarà oggi e non fine mese
        //            if (mese == DateTime.Now.Month &&
        //                anno == DateTime.Now.Year)
        //            {
        //                dtEnd = new DateTime(anno, mese, DateTime.Now.Day);
        //            }

        //            List<ItemCarMagPresenza> listaCarMp = new List<ItemCarMagPresenza>();

        //            var presenze = this.PresenzeSettimanali(item.matricola, dtStart.ToString("ddMMyyyy"), dtEnd.ToString("ddMMyyyy"));
        //            if (presenze.esito)
        //            {
        //                if (presenze.dati.items != null && presenze.dati.items.Any())
        //                {
        //                    int oreCarenza = 0;
        //                    int minutiCarenza = 0;
        //                    int oreMaggiorPresenza = 0;
        //                    int minutiMaggiorPresenza = 0;
        //                    int tempOre = 0;
        //                    int tempMinuti = 0;
        //                    int conteggioGiornate = 1;
        //                    int settimana = 1;
        //                    DateTime dtMarkUp1 = dtStart;
        //                    DateTime dtMarkUp2 = dtEnd;

        //                    while (dtStart.Date <= dtEnd.Date)
        //                    {
        //                        var giornata = presenze.dati.items.Where(g => g.data == dtStart).FirstOrDefault();

        //                        tempOre = Int32.Parse(giornata.carenza.Trim().Substring(0, 2));
        //                        tempMinuti = Int32.Parse(giornata.carenza.Trim().Substring(3, 2));

        //                        oreCarenza += tempOre;
        //                        minutiCarenza += tempMinuti;

        //                        tempOre = Int32.Parse(giornata.maggiorPresenza.Trim().Substring(0, 2));
        //                        tempMinuti = Int32.Parse(giornata.maggiorPresenza.Trim().Substring(3, 2));

        //                        oreMaggiorPresenza += tempOre;
        //                        minutiMaggiorPresenza += tempMinuti;

        //                        // se è terminata la settimana oppure 
        //                        // è l'ultimo giorno del mese
        //                        //if (conteggioGiornate == 7 ||
        //                        //    dtStart.AddDays(1).Date == dtEnd.Date)
        //                        if ( conteggioGiornate == 7 ||
        //                            dtStart.Date == dtEnd.Date )
        //                        {
        //                            conteggioGiornate = 1;
        //                            listaCarMp.Add(new ItemCarMagPresenza()
        //                            {
        //                                CAR = ConvertiTempoInStringa(oreCarenza, minutiCarenza),
        //                                MP = ConvertiTempoInStringa(oreMaggiorPresenza, minutiMaggiorPresenza),
        //                                Settimana = settimana,
        //                                DataInizio = dtMarkUp1 ,
        //                                DataFine = dtStart
        //                            } );

        //                            settimana++;
        //                            oreCarenza = 0;
        //                            minutiCarenza = 0;
        //                            oreMaggiorPresenza = 0;
        //                            minutiMaggiorPresenza = 0;
        //                            dtMarkUp1 = dtStart;
        //                        }
        //                        else
        //                        {
        //                            conteggioGiornate++;
        //                        }
        //                        dtStart = dtStart.AddDays(1);
        //                    }

        //                    string t1 = CalcolaOreTotali(listaCarMp, "MP");
        //                    string t2 = CalcolaOreTotali(listaCarMp, "CAR");

        //                    rList.Add(new Report_Item()
        //                    {
        //                        Matricola = item.matricola,
        //                        Nominativo = item.nominativo,
        //                        ListaCarMp = listaCarMp,
        //                        Saldo = CalcolaSaldo(t1, t2),
        //                        NumeroOccorrenze = 0,
        //                        TotaleOreLista1 = t1,
        //                        TotaleOreLista2 = t2
        //                    });
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ScriviLog(ex.ToString(), matricola, "Report_Carenza_MP", null);
        //        result.Esito = false;
        //        result.Errore = ex.ToString();
        //        result.Risposta = null;
        //        return result;
        //    }

        //    result.Esito = true;
        //    result.Errore = null;
        //    result.Risposta = new List<Report_Item>();
        //    result.Risposta = rList;

        //    return result;
        //}

        public Report_Carenza_MP_Response GetReport_Carenza_MP ( string matricola , string sede , int mese , int anno , bool inizioMese = true )
        {
            Report_Carenza_MP_Response result = new Report_Carenza_MP_Response( );
            DateTime dt = new DateTime( anno , mese , 1 );
            result.Risposta = new List<Report_Item>( );

            List<Report_Item> rList = new List<Report_Item>( );

            try
            {
                var resp = this.presenzeGiornaliere( matricola , sede , dt.ToString( "ddMMyyyy" ) );

                if ( resp.esito == false && !String.IsNullOrEmpty( resp.errore ) )
                {
                    result.Esito = false;
                    result.Errore = resp.errore;
                    result.Risposta = null;
                    return result;
                }

                if ( resp.dati == null || resp.dati.Count( ) == 0 )
                {
                    result.Esito = true;
                    result.Errore = "Non ci sono dati disponibili";
                    result.Risposta = null;
                    return result;
                }

                foreach ( var item in resp.dati )
                {
                    var quad = GetQuadratura( item.tipoDip );

                    if ( quad.GetValueOrDefault( ) != Quadratura.Settimanale )
                    {
                        continue;
                    }

                    DateTime dtStart = new DateTime( anno , mese , 1 );
                    DateTime dtEnd = dtStart.AddMonths( 1 );
                    dtEnd = dtEnd.AddDays( -1 );

                    // se il campo inizioMese è false
                    // vuol dire che devono essere calcolate le settimane a partire dal Lunedì
                    // quindi se ad esempio il primo del mese è un mercoledì, vanno presi anche i 2
                    // giorni del mese precedente
                    if ( !inizioMese )
                    {
                        bool inizioAltroGiorno = false;

                        // va reperita la data di inizio dal db L2D_SEDE_GAPP
                        // se il campo giorno_inizio_settimana
                        using (digiGappEntities db = new digiGappEntities())
                        {
                            var sedeGapp = db.L2D_SEDE_GAPP.Where(w => w.cod_sede_gapp.Equals(sede)).FirstOrDefault();
                            if (sedeGapp != null)
                            {
                                if (sedeGapp.giorno_inizio_settimana != null)
                                {
                                    inizioAltroGiorno = true;
                                    int giorno = sedeGapp.giorno_inizio_settimana.GetValueOrDefault();
                                    // sul DB viene impostato 7 per indicare la domenica, mentre c# la domenica la indica con lo 0
                                    if (giorno == 7)
                                    {
                                        giorno = 0;
                                    }
                                    DayOfWeek day = ((DayOfWeek)giorno);
                                    giorno = (giorno == 0 ? 6 : giorno - 1);
                                    DayOfWeek day2 = ((DayOfWeek)giorno);

                                    if (dtStart.DayOfWeek != day)
                                    {
                                        do
                                        {
                                            dtStart = dtStart.AddDays(-1);
                                        } while (dtStart.DayOfWeek != day);
                                    }

                                    if (dtEnd.DayOfWeek != day2)
                                    {
                                        do
                                        {
                                            dtEnd = dtEnd.AddDays(1);
                                        } while (dtEnd.DayOfWeek != day2);
                                    }
                                }
                            }
                        }

                        if (dtStart.DayOfWeek != DayOfWeek.Monday && !inizioAltroGiorno)
                        {
                            do
                            {
                                dtStart = dtStart.AddDays( -1 );
                            } while ( dtStart.DayOfWeek != DayOfWeek.Monday );
                        }

                        if (dtEnd.DayOfWeek != DayOfWeek.Sunday && !inizioAltroGiorno)
                        {
                            do
                            {
                                dtEnd = dtEnd.AddDays( 1 );
                            } while ( dtEnd.DayOfWeek != DayOfWeek.Sunday );
                        }
                    }

                    List<ItemCarMagPresenza> listaCarMp = new List<ItemCarMagPresenza>( );

                    List<wApiUtility.presenzeSettimanali_Item> items = new List<wApiUtility.presenzeSettimanali_Item>();

                    #region Recupero dati da HRDW

                    try
                    {
                        using ( var sediDB = new PERSEOEntities( ) )
                        {
                            string queryDIP = "SELECT [matricola] " +
                                                    ",[matricola_dp]" +
                                                    ",[nominativo]" +
                                                    ",[data_assunzione]" +
                                                    ",[data_cessazione]" +
                                                "FROM[LINK_HRIS_HRLIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA]" +
                                                "Where matricola_dp in ('#MATRICOLA#')";

                            queryDIP = queryDIP.Replace( "#MATRICOLA#" , item.matricola.Substring( 1 ) );

                            var rispostaDIP = sediDB.Database.SqlQuery<RispostaHRDW>( queryDIP ).FirstOrDefault( );

                            if ( rispostaDIP == null )
                            {
                                throw new Exception( " Utente " + item.matricola.Substring( 1 ) + " non trovato su HRDW " );
                            }

                            // se esiste una data cessazione
                            if ( rispostaDIP.data_cessazione.HasValue )
                            {
                                // se la data cessazione è successiva a dtStart
                                if ( rispostaDIP.data_cessazione.GetValueOrDefault( ) >= dtStart )
                                {
                                    // verifica se la data di cessazione è <= alla data fine
                                    // se si allora imposta la data fine = alla data di cessazione così da non richiedere
                                    // giorni dove sicuramente quella matricola non è presente in quanto ha terminato il rapporto lavorativo
                                    if ( rispostaDIP.data_cessazione.GetValueOrDefault( ) <= dtEnd )
                                    {
                                        dtEnd = rispostaDIP.data_cessazione.GetValueOrDefault( );
                                    }
                                }
                                else
                                {
                                    // caso limite perchè non dovrebbero esserci di questi casi
                                    // comunque nel caso il fine rapporto sia antecedente alla data di 
                                    // inizio richiest allora questa matricola non va considerata
                                    continue;
                                }
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        throw new Exception( "Si è verificato un errore su HRDW" + ex.Message );
                    }

                    #endregion

                    // verifica se il numero di giorni tra le due date è superiore a 40 giorni
                    double days = ( dtEnd - dtStart ).TotalDays;

                    if (days <= 40)
                    {
                        var presenze = this.PresenzeSettimanali( item.matricola , dtStart.ToString( "ddMMyyyy" ) , dtEnd.ToString( "ddMMyyyy" ) );

                        if ( presenze.esito )
                        {
                            if ( presenze.dati.items != null && presenze.dati.items.Any( ) )
                            {
                                items.AddRange( presenze.dati.items );
                            }
                        }
                        else
                        {
                            result.Esito = false;
                            result.Errore = presenze.errore;
                            result.Risposta = null;
                            return result;
                        }
                    }
                    else
                    {
                        /*
                         * se > di 40 giorni allora serviranno più chiamate al mtodo
                         * PresenzeSettimanali, che non può restituire più di 40 giorni per volta
                         */
                        DateTime dt2 = dtEnd;
                        dt2 = dt2.AddDays( -14 );

                        var presenze = this.PresenzeSettimanali( item.matricola , dtStart.ToString( "ddMMyyyy" ) , dt2.ToString( "ddMMyyyy" ) );

                        if (presenze.esito)
                        {
                            if ( presenze.dati.items != null && presenze.dati.items.Any( ) )
                            {
                                items.AddRange( presenze.dati.items );
                            }
                        }
                        else
                        {
                            result.Esito = false;
                            result.Errore = presenze.errore;
                            result.Risposta = null;
                            return result;
                        }

                        dt2 = dt2.AddDays( 1 );
                        presenze = this.PresenzeSettimanali( item.matricola , dt2.ToString( "ddMMyyyy" ) , dtEnd.ToString( "ddMMyyyy" ) );

                        if ( presenze.esito )
                        {
                            if ( presenze.dati.items != null && presenze.dati.items.Any( ) )
                            {
                                items.AddRange( presenze.dati.items );
                            }
                        }
                        else
                        {
                            result.Esito = false;
                            result.Errore = presenze.errore;
                            result.Risposta = null;
                            return result;
                        }
                    }

                    if (items != null && items.Any())
                    {
                        int oreCarenza = 0;
                        int minutiCarenza = 0;
                        int oreMaggiorPresenza = 0;
                        int minutiMaggiorPresenza = 0;
                        int tempOre = 0;
                        int tempMinuti = 0;
                        int conteggioGiornate = 1;
                        int settimana = 1;
                        DateTime dtMarkUp1 = dtStart;
                        DateTime dtMarkUp2 = dtEnd;

                        while ( dtStart.Date <= dtEnd.Date )
                        {
                            var giornata = items.Where( g => g.data == dtStart ).FirstOrDefault( );

                            tempOre = Int32.Parse( giornata.carenza.Trim( ).Substring( 0 , 2 ) );
                            tempMinuti = Int32.Parse( giornata.carenza.Trim( ).Substring( 3 , 2 ) );

                            oreCarenza += tempOre;
                            minutiCarenza += tempMinuti;

                            tempOre = Int32.Parse( giornata.maggiorPresenza.Trim( ).Substring( 0 , 2 ) );
                            tempMinuti = Int32.Parse( giornata.maggiorPresenza.Trim( ).Substring( 3 , 2 ) );

                            oreMaggiorPresenza += tempOre;
                            minutiMaggiorPresenza += tempMinuti;

                            // se è terminata la settimana oppure 
                            // è l'ultimo giorno del mese
                            if ( conteggioGiornate == 7 ||
                                dtStart.Date == dtEnd.Date )
                            {
                                conteggioGiornate = 1;
                                listaCarMp.Add( new ItemCarMagPresenza( )
                                {
                                    CAR = ConvertiTempoInStringa( oreCarenza , minutiCarenza ) ,
                                    MP = ConvertiTempoInStringa( oreMaggiorPresenza , minutiMaggiorPresenza ) ,
                                    Settimana = settimana ,
                                    DataInizio = dtMarkUp1 ,
                                    DataFine = dtStart
                                } );

                                settimana++;
                                oreCarenza = 0;
                                minutiCarenza = 0;
                                oreMaggiorPresenza = 0;
                                minutiMaggiorPresenza = 0;
                                dtMarkUp1 = dtStart;
                                dtMarkUp1 = dtMarkUp1.AddDays( 1 );
                            }
                            else
                            {
                                conteggioGiornate++;
                            }
                            dtStart = dtStart.AddDays( 1 );
                        }

                        string t1 = CalcolaOreTotali( listaCarMp , "MP" );
                        string t2 = CalcolaOreTotali( listaCarMp , "CAR" );

                        rList.Add( new Report_Item( )
                        {
                            Matricola = item.matricola ,
                            Nominativo = item.nominativo ,
                            ListaCarMp = listaCarMp ,
                            Saldo = CalcolaSaldo( t1 , t2 ) ,
                            NumeroOccorrenze = 0 ,
                            TotaleOreLista1 = t1 ,
                            TotaleOreLista2 = t2
                        } );
                    }
                    else
                    {
                        int conteggioGiornate = 1;
                        int settimana = 1;
                        DateTime dtMarkUp1 = dtStart;
                        DateTime dtMarkUp2 = dtEnd;

                        while ( dtStart.Date <= dtEnd.Date )
                        {
                            // se è terminata la settimana oppure 
                            // è l'ultimo giorno del mese
                            if ( conteggioGiornate == 7 ||
                                dtStart.Date == dtEnd.Date )
                            {
                                conteggioGiornate = 1;
                                listaCarMp.Add( new ItemCarMagPresenza( )
                                {
                                    CAR = "00:00" ,
                                    MP = "00:00" ,
                                    Settimana = settimana ,
                                    DataInizio = dtMarkUp1 ,
                                    DataFine = dtStart
                                } );

                                settimana++;
                                dtMarkUp1 = dtStart;
                                dtMarkUp1 = dtMarkUp1.AddDays( 1 );
                            }
                            else
                            {
                                conteggioGiornate++;
                            }
                            dtStart = dtStart.AddDays( 1 );
                        }

                        string t1 = "00:00";
                        string t2 = "00:00";

                        rList.Add( new Report_Item( )
                        {
                            Matricola = item.matricola ,
                            Nominativo = item.nominativo ,
                            ListaCarMp = listaCarMp ,
                            Saldo = "00:00" ,
                            NumeroOccorrenze = 0 ,
                            TotaleOreLista1 = t1 ,
                            TotaleOreLista2 = t2
                        } );
                    }

                }
            }
            catch ( Exception ex )
            {
                ScriviLog( ex.ToString( ) , matricola , "Report_Carenza_MP" , null );
                result.Esito = false;
                result.Errore = ex.ToString( );
                result.Risposta = null;
                return result;
            }

            result.Esito = true;
            result.Errore = null;
            result.Risposta = new List<Report_Item>( );
            result.Risposta = rList;

            return result;
        }


        #endregion

        #region Servizi Mensa
        public GetServizioMensaResponse GetInfoMensa(string matricolaCaller, string matricola, DateTime giornata)
        {
            GetServizioMensaResponse result = new GetServizioMensaResponse();
            result.Errore = "";
            result.Esito = false;
            result.Response = new EsitoMensa();
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    string m = matricola.PadLeft(8, '0');
                    DateTime dt1 = new DateTime(giornata.Year, giornata.Month, giornata.Day, 0, 0, 0);
                    DateTime dt2 = new DateTime(giornata.Year, giornata.Month, giornata.Day, 23, 59, 59);
                    var items = db.MyRai_MensaXML.Where(w => w.Badge.Equals(m) &&
                                                        (w.TransactionDateTime >= dt1 && w.TransactionDateTime <= dt2)).ToList();

                    if (items != null && items.Any())
                    {
                        string pp = "";
                        var item = items.FirstOrDefault();
                        string[] linee = item.XMLorig.Split(new string[] { "<Line>" }, StringSplitOptions.None).Where(itm => itm != "</Line>").ToArray();
                        if (linee.Count() > 4)
                        {
                            pp += linee[2].Replace("</Line>", "").Trim();
                            string daPulire = linee[3].Replace("</Line>", "").Trim();
                            pp += daPulire.Substring(daPulire.IndexOf('-') > -1 ? daPulire.IndexOf('-') + 1 : 0);
                            pp = pp.Trim();

                            result.Response.Luogo = pp;
                            result.Response.Data = item.TransactionDateTime;
                            result.Esito = true;
                        }
                        else
                        {
                            result.Esito = true;
                            result.Errore = "Informazioni non disponibili";
                            result.Response = null;
                            return result;
                        }
                    }
                    else
                    {
                        result.Esito = true;
                        result.Errore = "Informazioni non disponibili";
                        result.Response = null;
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
                result.Response = null;
                string datiCall = String.Format("matricolaCaller = {0}, matricola = {1}, giornata = {2}", matricolaCaller, matricola, giornata.ToString("dd/MM/yyyy"));
                ScriviLog(ex.ToString(), matricola, "GetInfoMensa", datiCall);
            }

            return result;
        }

        #endregion

        #region Modulo Detassazione
        public GetModuloDetassazioneResponse GetModuloDetassazione ( string pMatricolaCaller , string matricolaDestinatario )
        {
            GetModuloDetassazioneResponse result = new GetModuloDetassazioneResponse( )
            {
                Esito = false,
                Errore = "",
                Response = null,
                InputCics = null,
                OutputCics = null
            };

            try
            {
                if (String.IsNullOrEmpty(pMatricolaCaller) || String.IsNullOrEmpty(matricolaDestinatario))
                {
                    throw new Exception( "Parametri in ingresso mancanti" );
                }

                // creazione stringa per la chiamata a cics
                string pMatricola = "";

                if (pMatricolaCaller.Length < 7 && !pMatricolaCaller.StartsWith("P"))
                {
                    pMatricola = "P" + pMatricolaCaller;
                }
                else
                {
                    pMatricola = pMatricolaCaller;
                }

                if (matricolaDestinatario.Length > 6 && matricolaDestinatario.StartsWith("P"))
                {
                    matricolaDestinatario = matricolaDestinatario.Substring( 1 );
                }

                string versoCics = "P956,DST," + pMatricola + "   DET," + matricolaDestinatario;

                ScriviLogAzione( matricolaDestinatario , "GetModuloDetassazione" , "I:" + versoCics );

                ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics( );
                result.InputCics = versoCics;
                string output;
                output = ( string ) c.ComunicaVersoCics( versoCics );
                output = output.Replace( Convert.ToChar( 0x0 ).ToString( ) , " " );
                result.OutputCics = output;

                ScriviLogAzione( matricolaDestinatario , "GetModuloDetassazione" , "O:" + output );

                if (output.StartsWith("ACK01"))
                {
                    string codice = output.Substring( 74 , 2 );
                    string scelta = output.Substring( 76 , 2 );

                    if (!String.IsNullOrEmpty( scelta ) )
                    {
                        scelta = scelta.Trim( );
                    }

                    result.Esito = true;
                    result.Response = codice;
                    if (!String.IsNullOrEmpty(scelta))
                    {
                        throw new Exception( "Scelta già effettuata" );
                    }
                }
                else
                {
                    ScriviLogAzione( matricolaDestinatario , "GetModuloDetassazione" , "E:" + output );
                    throw new Exception( "Errore nella risposta CICS: " + output.Substring( 0 , 5 ) );
                }
            }
            catch(Exception ex )
            {
                result.Errore = ex.Message;
                ScriviLogAzione( matricolaDestinatario , "GetModuloDetassazione" , "E:" + ex.Message );
            }

            return result;
        }

        public SetSceltaDetassazioneResponse SetSceltaDetassazione ( string pMatricolaCaller , string matricolaDestinatario , DateTime data , string modulo , int scelta )
        {
            SetSceltaDetassazioneResponse result = new SetSceltaDetassazioneResponse( )
            {
                Esito = false ,
                Errore = "" ,
                Response = null ,
                InputCics = null ,
                OutputCics = null
            };

            try
            {
                if (modulo == "1C")
                {
                    result = SetSceltaDetassazione1C( pMatricolaCaller , matricolaDestinatario , data );
                }
                else
                {
                    result = SetSceltaDetassazione2C( pMatricolaCaller , matricolaDestinatario , data , modulo , scelta );
                }
            }
            catch ( Exception ex ) 
            {
                result.Errore = ex.Message;
            }

            return result;
        }
              
        private SetSceltaDetassazioneResponse SetSceltaDetassazione1C ( string pMatricolaCaller , string matricolaDestinatario , DateTime data )
        {
            SetSceltaDetassazioneResponse result = new SetSceltaDetassazioneResponse( )
            {
                Esito = false ,
                Errore = "" ,
                Response = null ,
                InputCics = null ,
                OutputCics = null
            };

            try
            {
                if ( String.IsNullOrEmpty( pMatricolaCaller ) || String.IsNullOrEmpty( matricolaDestinatario ) )
                {
                    throw new Exception( "Parametri in ingresso mancanti" );
                }

                // creazione stringa per la chiamata a cics
                string pMatricola = "";

                if ( pMatricolaCaller.Length < 7 && !pMatricolaCaller.StartsWith( "P" ) )
                {
                    pMatricola = "P" + pMatricolaCaller;
                }
                else
                {
                    pMatricola = pMatricolaCaller;
                }

                if ( matricolaDestinatario.Length > 6 && matricolaDestinatario.StartsWith( "P" ) )
                {
                    matricolaDestinatario = matricolaDestinatario.Substring( 1 );
                }

                string versoCics = "P956,DST," + pMatricola + "   UPD," + matricolaDestinatario + data.ToString( "yyyyMMdd" ) + "1C0N                           11  ";

                ScriviLogAzione( matricolaDestinatario , "SetSceltaDetassazione1C" , "I:" + versoCics );

                ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics( );
                result.InputCics = versoCics;
                string output;
                output = ( string ) c.ComunicaVersoCics( versoCics );
                output = output.Replace( Convert.ToChar( 0x0 ).ToString( ) , " " );
                result.OutputCics = output;
                ScriviLogAzione( matricolaDestinatario , "SetSceltaDetassazione1C" , "O:" + output );

                if ( output.StartsWith( "ACK01" ) )
                {
                    string codice = output.Substring( 37 , 2 );
                    if (codice != "00")
                    {                        
                        result.Esito = false;
                    }
                    else
                    {
                        result.Esito = true;
                    }
                    result.Response = codice;
                }
                else
                {
                    ScriviLogAzione( matricolaDestinatario , "SetSceltaDetassazione1C" , "E:" + output );
                    throw new Exception( "Errore nella risposta CICS: " + output.Substring( 0 , 5 ) );
                }
            }
            catch ( Exception ex )
            {
                result.Errore = ex.Message;
                ScriviLogAzione( matricolaDestinatario , "SetSceltaDetassazione1C" , "E:" + ex.Message );
            }

            return result;
        }

        private SetSceltaDetassazioneResponse SetSceltaDetassazione2C ( string pMatricolaCaller , string matricolaDestinatario , DateTime data , string modulo , int scelta )
        {
            SetSceltaDetassazioneResponse result = new SetSceltaDetassazioneResponse( )
            {
                Esito = false ,
                Errore = "" ,
                Response = null ,
                InputCics = null ,
                OutputCics = null
            };

            try
            {
                if ( String.IsNullOrEmpty( pMatricolaCaller ) || String.IsNullOrEmpty( matricolaDestinatario ) )
                {
                    throw new Exception( "Parametri in ingresso mancanti" );
                }

                // creazione stringa per la chiamata a cics
                string pMatricola = "";

                if ( pMatricolaCaller.Length < 7 && !pMatricolaCaller.StartsWith( "P" ) )
                {
                    pMatricola = "P" + pMatricolaCaller;
                }
                else
                {
                    pMatricola = pMatricolaCaller;
                }

                if ( matricolaDestinatario.Length > 6 && matricolaDestinatario.StartsWith( "P" ) )
                {
                    matricolaDestinatario = matricolaDestinatario.Substring( 1 );
                }

                string versoCics = "P956,DST," + pMatricola + "   UPD," + matricolaDestinatario + data.ToString( "yyyyMMdd" ) + modulo.ToUpper() + scelta + "S                           " + "  " + scelta + "1  ";

                ScriviLogAzione( matricolaDestinatario , "SetSceltaDetassazione2C" , "I:" + versoCics );

                ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics( );
                result.InputCics = versoCics;
                string output;
                output = ( string ) c.ComunicaVersoCics( versoCics );
                output = output.Replace( Convert.ToChar( 0x0 ).ToString( ) , " " );
                result.OutputCics = output;
                ScriviLogAzione( matricolaDestinatario , "SetSceltaDetassazione2C" , "O:" + output );
                if ( output.StartsWith( "ACK01" ) )
                {
                    string codice = output.Substring( 37 , 2 );
                    if ( codice != "00" )
                    {
                        result.Esito = false;
                    }
                    else
                    {
                        result.Esito = true;
                    }
                    result.Response = codice;
                }
                else
                {
                    ScriviLogAzione( matricolaDestinatario , "SetSceltaDetassazione2C" , "E:" + output );
                    throw new Exception( "Errore nella risposta CICS: " + output.Substring( 0 , 5 ) );
                }
            }
            catch ( Exception ex )
            {
                result.Errore = ex.Message;
                ScriviLogAzione( matricolaDestinatario , "SetSceltaDetassazione2C" , "E:" + ex.Message );
            }

            return result;
        }

        public ResetModuloDetassazioneResponse ResetModuloDetassazione ( string pMatricolaCaller , string matricolaDestinatario, string applicazione )
        {
            ResetModuloDetassazioneResponse result = new ResetModuloDetassazioneResponse( )
            {
                Esito = false ,
                Errore = "" ,
                Response = null
            };

            try
            {
                ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics( );

                if ( String.IsNullOrEmpty( pMatricolaCaller ) || String.IsNullOrEmpty( matricolaDestinatario ) )
                {
                    throw new Exception( "Parametri in ingresso mancanti" );
                }

                // creazione stringa per la chiamata a cics
                string pMatricola = "";

                if ( pMatricolaCaller.Length < 7 && !pMatricolaCaller.StartsWith( "P" ) )
                {
                    pMatricola = "P" + pMatricolaCaller;
                }
                else
                {
                    pMatricola = pMatricolaCaller;
                }

                if ( matricolaDestinatario.Length > 6 && matricolaDestinatario.StartsWith( "P" ) )
                {
                    matricolaDestinatario = matricolaDestinatario.Substring( 1 );
                }

                string versoCics = "P956,DST," + pMatricola + "   CAN," + matricolaDestinatario;

                ScriviLogAzione( matricolaDestinatario , "ResetModuloDetassazione" , "I:" + versoCics );

                result.Response = new List<ResetModuloDetassazioneItem>( );
                string output = "";

                result.Response.Add( new ResetModuloDetassazioneItem( )
                {
                    InputCics = versoCics
                } );
                    
                output = ( string ) c.ComunicaVersoCics( versoCics );
                output = output.Replace( Convert.ToChar( 0x0 ).ToString( ) , " " );
                result.Response[0].OutputCics = output;

                ScriviLogAzione( matricolaDestinatario , "ResetModuloDetassazione" , "O:" + output );

                if ( output.StartsWith( "ACK01" ) )
                {
                    result.Esito = true;
                }
                else
                {
                    ScriviLogAzione( matricolaDestinatario , "ResetModuloDetassazione" , "E:" + output );
                    throw new Exception( "Errore nella risposta CICS (CAN): " + output.Substring( 0 , 5 ) );
                }                             

                versoCics = "P956,DST," + pMatricola + "   UPD," + matricolaDestinatario + "           " + applicazione + "                                 ";
                ScriviLogAzione( matricolaDestinatario , "ResetModuloDetassazione" , "I:" + versoCics );

                result.Response.Add( new ResetModuloDetassazioneItem( )
                {
                    InputCics = versoCics
                } );

                output = ( string ) c.ComunicaVersoCics( versoCics );
                output = output.Replace( Convert.ToChar( 0x0 ).ToString( ) , " " );
                result.Response[1].OutputCics = output;
                ScriviLogAzione( matricolaDestinatario , "ResetModuloDetassazione" , "O:" + output );
                if ( output.StartsWith( "ACK01" ) )
                {
                    result.Esito = true;
                }
                else
                {
                    ScriviLogAzione( matricolaDestinatario , "ResetModuloDetassazione" , "E:" + output );
                    throw new Exception( "Errore nella risposta CICS (UPD): " + output.Substring( 0 , 5 ) );
                }

            }
            catch ( Exception ex )
            {
                ScriviLogAzione( matricolaDestinatario , "ResetModuloDetassazione" , "E:" + ex.Message );
                result.Errore = ex.Message;
                result.Esito = false;
            }

            return result;
        }


        private void ScriviLogAzione ( string matricola , string metodo, string msg )
        {
            if ( !String.IsNullOrEmpty( msg ) )
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    db.MyRai_LogAzioni.Add( new MyRai_LogAzioni( )
                    {
                        matricola = matricola ,
                        data = DateTime.Now ,
                        operazione = metodo ,
                        applicativo = "Portale" ,
                        descrizione_operazione = msg ,
                        provenienza = "MyRaiService"
                    } );
                    db.SaveChanges( );
                }
            }
        }
        #endregion

        #region Conteggio giorni consecutivi
        public ConteggioGiorniConsecutivi_Response GetGiorniConsecutivi(string matricola, string sedeGapp, DateTime dataPartenza)
        {
            ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
            ConteggioGiorniConsecutivi_Response response = new ConteggioGiorniConsecutivi_Response();
            List<ConteggioGiorniConsecutivi> dip = new List<ConteggioGiorniConsecutivi>();
            string indiceStart = "000";
            bool riciclo = false;
            string risposta = "";
            string S = "";
            try
            {
                do
                {
                    S = areaSistema("3LC", matricola, 75, riciclo);
                    S += sedeGapp + dataPartenza.ToString("ddMMyyyy") + indiceStart + "".PadLeft(34);
                    risposta = (string)c.ComunicaVersoCics(S.PadRight(8000));
                    risposta = risposta.Replace(Convert.ToChar(0x0).ToString(), " ");

                    indiceStart = risposta.Substring(105, 3);
                    riciclo = (risposta.Substring(44, 1) == "S") ? true : false;
                    string esito = risposta.Substring(45, 2);

                    if (!String.IsNullOrEmpty(esito) && esito.Equals("OK"))
                    {
                        // rimuove la parte iniziale della risposta per avere solo l'array di dati
                        var elementi = risposta.Substring(142);

                        do
                        {
                            DateTime data = dataPartenza;

                            // prende la prima riga
                            var item = elementi.Substring(0, 120);
                            ConteggioGiorniConsecutivi dipendente = new ConteggioGiorniConsecutivi();
                            dipendente.Matricola = item.Substring(0, 7);
                            dipendente.Nominativo = item.Substring(7, 25);
                            dipendente.TipoDipendente = item.Substring(32, 1);
                            // prendere l'array dei 40 giorni
                            item = item.Substring(40);
                            dipendente.Giorni = new List<DettaglioConteggioGiorniConsecutivi>();
                            do
                            {
                                string val = item.Substring(0, 2);
                                DettaglioConteggioGiorniConsecutivi obj = new DettaglioConteggioGiorniConsecutivi();
                                obj.Giorno = data;

                                // se ha ** allora la giornata non è presente/dati non disponibili per quella data
                                if (val == "**")
                                {
                                    obj.ConteggioOccorrenza = 0;
                                    obj.Info = String.Empty;
                                    obj.Descrizione = "Giornata non presente";
                                }
                                else if (Regex.Matches(val, @"[a-zA-Z]").Count > 0)
                                {
                                    // se ci sono lettere allora è un'eccezione
                                    obj.ConteggioOccorrenza = 0;
                                    obj.Info = val;
                                    obj.Descrizione = "Codice eccezione";
                                }
                                else if (val.Contains("00") || val.Contains("  "))
                                {
                                    // se 00 o spazio vuoto allora è un codice orario
                                    obj.ConteggioOccorrenza = 0;
                                    obj.Info = val;
                                    obj.Descrizione = "Codice orario";
                                }
                                else if (Regex.Matches(val, @"[0-9]").Count > 0)
                                {
                                    int dato = int.Parse(val);

                                    // se il valore numerico è compreso tra 90 e 99 allora
                                    // è un codice orario
                                    if (dato >= 90 && dato <= 99)
                                    {
                                        obj.ConteggioOccorrenza = 0;
                                        obj.Info = val;
                                        obj.Descrizione = "Codice orario";
                                    }
                                    else
                                    {
                                        // se ha un valore numerico da 1 a 89, allora 
                                        // è il conteggio delle giornate consecutive
                                        obj.ConteggioOccorrenza = dato;
                                        obj.Info = String.Empty;
                                        obj.Descrizione = "Valore giorni";
                                    }
                                }

                                obj.Festivo = IsFestivo(data, sedeGapp) || IsSuperFestivo(data);

                                dipendente.Giorni.Add(obj);
                                item = item.Substring(2);
                                data = data.AddDays(1);
                            } while (item.Length > 0);

                            dip.Add(dipendente);
                            elementi = elementi.Substring(120);
                            Console.WriteLine(String.Format("{0} - {1} - {2}", dipendente.Matricola, dipendente.Nominativo, dipendente.TipoDipendente));
                        } while (elementi.Length >= 120);
                    }
                    else
                    {
                        string errore = risposta.Substring(0, 5) + " " + risposta.Substring(51, 25);
                        throw new Exception(errore);
                    }

                } while (riciclo);

                response = new ConteggioGiorniConsecutivi_Response()
                {
                    Errore = null,
                    Esito = true,
                    OutputCics = risposta,
                    InputCics = S.PadRight(8000),
                    Risposta = new List<ConteggioGiorniConsecutivi>()
                };

                response.Risposta.AddRange(dip.ToList());
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.Risposta = null;
                string datiInput = String.Format("matricola {0}, sedeGapp {1}, data partenza {2}", matricola, sedeGapp, dataPartenza.ToString("dd/MM/yyyy"));
                ScriviLog(ex.ToString(), matricola, "GetGiorniConsecutivi", datiInput);
            }

            return response;
        }

        /// <summary>
        /// Verifica se una particolare data è un giorno festivo
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool IsFestivo(DateTime data, string sedeGapp)
        {
            bool result = false;

            try
            {
                // Epifania
                if (data.Day == 6 && data.Month == 1)
                {
                    result = true;
                }

                // Liberazione
                if (data.Day == 25 && data.Month == 4)
                {
                    result = true;
                }

                // Festa della Repubblica
                if (data.Day == 2 && data.Month == 6)
                {
                    result = true;
                }

                // 1 Novembre Tutti i santi
                if (data.Day == 1 && data.Month == 11)
                {
                    result = true;
                }

                // Immacolata concezione
                if (data.Day == 8 && data.Month == 12)
                {
                    result = true;
                }

                // Santo Stefano
                if (data.Day == 26 && data.Month == 12)
                {
                    result = true;
                }

                DateTime pasqua = GetPasqua(data.Year);

                // Lunedì dell'angelo
                if (data.Day + 1 == pasqua.Day && data.Month == pasqua.Month)
                {
                    result = true;
                }

                // PATRONO

                DateTime patrono = GetPatrono(sedeGapp);

                if (patrono != DateTime.MinValue)
                {
                    if (data.Day == patrono.Day && data.Month == patrono.Month)
                    {
                        result = true;
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
        /// Reperimento della data di decorrenza della festa patronale della
        /// sede gapp
        /// </summary>
        /// <returns></returns>
        private DateTime GetPatrono(string sede)
        {
            DateTime result = DateTime.MinValue;

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var row = db.L2D_SEDE_GAPP.Where(c => c.cod_sede_gapp.Equals(sede, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                    if (row != null && row.Data_Patrono.HasValue)
                    {
                        result = row.Data_Patrono.Value;
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
        /// Calcolo della pasqua
        /// </summary>
        /// <param name="year">Anno</param>
        /// <returns>Data completa in cui ricade la pasqua</returns>
        private DateTime GetPasqua(int year)
        {
            int month = 0;
            int day = 0;

            int g = year % 19;
            int c = year / 100;
            int h = h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25)
                                                + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) *
                        (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) +
                          i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }

        /// <summary>
        /// Verifica se una particolare data è un giorno super festivo
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool IsSuperFestivo(DateTime data)
        {
            bool result = false;

            try
            {
                // Epifania
                if (data.Day == 1 && data.Month == 1)
                {
                    result = true;
                }

                // 1 Maggio Festa dei lavoratori
                if (data.Day == 1 && data.Month == 5)
                {
                    result = true;
                }

                // 15 Agosto
                if (data.Day == 15 && data.Month == 8)
                {
                    result = true;
                }

                // Natale
                if (data.Day == 25 && data.Month == 12)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }


        #endregion

        #region Approvatori Produzione
        /// <summary>
        /// Reperimento dell'elenco delle produzioni e relativi approvatori
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="sedi"></param>
        /// <param name="titoli"></param>
        /// <returns></returns>
        public GetApprovatoriProduzioneResponse GetApprovatoriProduzione(string matricola)
        {
            GetApprovatoriProduzioneResponse result = new GetApprovatoriProduzioneResponse();
            result.Esito = true;
            result.Errore = String.Empty;
            result.Approvatori = new List<ApprovatoriProduzioneItem>();
            ScriviLog("GetApprovatoriProduzione", matricola, null);
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var query = db.MyRai_ApprovatoreProduzione.ToList();

                    if (query != null && query.Any())
                    {
                        var items = query.GroupBy(x => x.Titolo).Select(y => y.First());
                        if (items != null && items.Any())
                        {
                            items.ToList().ForEach(w =>
                            {
                                result.Approvatori.Add(new ApprovatoriProduzioneItem()
                                {
                                    Titolo = w.Titolo,
                                    Approvatore = w.MatricolaApprovatore
                                });
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.ToString();
                ScriviLog("GetApprovatoriProduzione - Error: " + ex.ToString(), matricola, null);
            }

            return result;
        }

        /// <summary>
        /// Inserimento o modifica di un nuovo item nella tabella MyRai_ApprovatoreProduzione
        /// </summary>
        /// <param name="item">toInsert conterrà il titolo e la matricola da aggiungere</param>
        /// <param name="checkIfExists">se tru verifica se il titolo è già presente nell'elenco</param>
        /// <returns></returns>
        public SetApprovatoreProduzioneResponse SetApprovatoreProduzione(string matricola, ApprovatoriProduzioneItem item, bool checkIfExists = false)
        {
            SetApprovatoreProduzioneResponse result = new SetApprovatoreProduzioneResponse();
            result.Esito = true;
            result.Item = new ApprovatoriProduzioneItem();

            string param = String.Format("matricola: {0}, checkIfExists: {1}, Approvatore: {2}, Titolo: {3} ", matricola, checkIfExists, item.Approvatore, item.Titolo);
            ScriviLog("SetApprovatoreProduzione", matricola, param);
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    if (item.Id > 0)
                    {
                        var exists = db.MyRai_ApprovatoreProduzione.Where(w => w.Id.Equals(item.Id)).FirstOrDefault();
                        if (exists != null && checkIfExists)
                        {
                            throw new Exception("Elemento già presente in elenco");
                        }
                        // verifica se già esiste
                        else if (exists != null)
                        {
                            exists.MatricolaApprovatore = item.Approvatore;
                            exists.MatricolaUltimaModifica = matricola;
                            exists.DataUltimaModifica = DateTime.Now;
                            db.SaveChanges();
                            result.Item = item;
                            return result;
                        }
                    }
                    MyRai_ApprovatoreProduzione newItem = new MyRai_ApprovatoreProduzione()
                    {
                        MatricolaApprovatore = item.Approvatore,
                        Titolo = item.Titolo,
                        MatricolaUltimaModifica = matricola,
                        DataUltimaModifica = DateTime.Now
                    };
                    db.SaveChanges();
                    item.Id = newItem.Id;
                    result.Item = item;
                }
            }
            catch (Exception ex)
            {
                result.Errore = ex.ToString();
                result.Esito = false;
                result.Item = null;
                ScriviLog("SetApprovatoreProduzione - Errore: " + ex.ToString(), matricola, param);
            }

            return result;
        }

        /// <summary>
        /// Reperimento dell'approvatore a partire dal titolo della produzione
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="titolo"></param>
        /// <returns></returns>
        public GetApprovatoreProduzioneResponse GetApprovatoriProduzioneByTitolo(string matricola, string titolo)
        {
            GetApprovatoreProduzioneResponse result = new GetApprovatoreProduzioneResponse();
            result.Esito = true;
            result.Items = null;

            string param = String.Format("matricola: {0}, titolo: {1} ", matricola, titolo);
            ScriviLog("GetApprovatoreProduzione", matricola, param);

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    // verifica se già esiste
                    var exists = db.MyRai_ApprovatoreProduzione.Where(w => w.Titolo.Equals(titolo)).ToList();
                    if (exists != null && exists.Any())
                    {
                        exists.ForEach(w =>
                        {
                            result.Items.Add(new ApprovatoriProduzioneItem()
                            {
                                Approvatore = w.MatricolaApprovatore,
                                Titolo = w.Titolo
                            });
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errore = ex.ToString();
                result.Esito = false;
                result.Items = null;
                ScriviLog("GetApprovatoreProduzione - Errore: " + ex.ToString(), matricola, param);
            }

            return result;
        }

        /// <summary>
        /// Reperimento dell'elenco delle produzioni a partire dalla matricola dell'approvatore
        /// </summary>
        /// <param name="matricola">Matricola chiamante</param>
        /// <param name="matricolaApprovatore">Matricola dell'approvatore per il quale si intende ottenere l'elento delle produzioni</param>
        /// <returns></returns>
        public GetApprovatoriProduzioneResponse GetProduzioniByApprovatore(string matricola, string matricolaApprovatore)
        {
            GetApprovatoriProduzioneResponse result = new GetApprovatoriProduzioneResponse();
            result.Errore = null;
            result.Esito = true;
            result.Approvatori = null;

            string param = String.Format("matricola: {0}, matricolaApprovatore: {1} ", matricola, matricolaApprovatore);
            ScriviLog("GetProduzioniByApprovatore", matricola, param);
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var elenco = db.MyRai_ApprovatoreProduzione.Where(w => w.MatricolaApprovatore.Equals(matricolaApprovatore)).ToList();

                    if (elenco != null && elenco.Any())
                    {
                        result.Approvatori = new List<ApprovatoriProduzioneItem>();

                        elenco.ForEach(w =>
                        {
                            result.Approvatori.Add(new ApprovatoriProduzioneItem()
                            {
                                Approvatore = w.MatricolaApprovatore,
                                Titolo = w.Titolo
                            });
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errore = ex.ToString();
                result.Esito = false;
                result.Approvatori = null;
                ScriviLog("GetProduzioniByApprovatore - Errore: " + ex.ToString(), matricola, param);
            }
            return result;
        }

        /// <summary>
        /// Cancellazione di un elemento a partire dal suo ID
        /// oppure tramite matricola apporvatore e titolo
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public ServiceResponseBase DeleteApprovatoreProduzione(string matricola, ApprovatoriProduzioneItem item)
        {
            ServiceResponseBase result = new ServiceResponseBase();
            result.Esito = true;
            result.Errore = String.Empty;
            MyRai_ApprovatoreProduzione exists = null;

            string param = String.Format("matricola: {0}, Id: {1}, Approvatore: {2}, Titolo: {3} ", matricola, item.Id, item.Approvatore, item.Titolo);
            ScriviLog("DeleteApprovatoreProduzione", matricola, param);
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    if (item.Id > 0)
                    {
                        exists = db.MyRai_ApprovatoreProduzione.Where(w => w.Id.Equals(item.Id)).FirstOrDefault();

                    }
                    else
                    {
                        exists = db.MyRai_ApprovatoreProduzione.Where(w => w.MatricolaApprovatore.Equals(item.Approvatore) &&
                                                                           w.Titolo.Equals(item.Titolo)).FirstOrDefault();
                    }

                    if (exists != null)
                    {
                        db.MyRai_ApprovatoreProduzione.Remove(exists);
                        db.SaveChanges();
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errore = ex.ToString();
                result.Esito = false;
                ScriviLog("DeleteApprovatoreProduzione - Errore: " + ex.ToString(), matricola, param);
            }

            return result;
        }

        #endregion

        #region Contatori

        public GetContatoriEccezioniResponse GetContatoriEccezioni ( string matricola , DateTime DataStart , DateTime DataEnd , List<string> eccezioni )
        {
            GetContatoriEccezioniResponse response = new GetContatoriEccezioniResponse( );
            response.Esito = true;
            response.Errore = string.Empty;
            response.ContatoriEccezioni = new List<GetContatoriEccezioniItem>( );

            try
            {
                #region Controlli

                if ( String.IsNullOrEmpty( matricola ) )
                {
                    throw new Exception( "Matricola assente" );
                }

                if ( eccezioni == null || !eccezioni.Any( ) )
                {
                    throw new Exception( "Lista eccezioni vuota" );
                }

                if ( DataStart > DataEnd )
                {
                    throw new Exception( "La data inizio non può essere superiore alla data fine" );
                }

                #endregion

                int count = 0;

                do
                {
                    string ecc1 = ( count < eccezioni.Count( ) ) ? eccezioni[count] : "";
                    count++;
                    string ecc2 = ( count < eccezioni.Count( ) ) ? eccezioni[count] : "";
                    count++;
                    string ecc3 = ( count < eccezioni.Count( ) ) ? eccezioni[count] : "";
                    count++;

                    var risposta = GetAnalisiEccezioni2( matricola , DataStart , DataEnd , ecc1 , ecc2 , ecc3 );

                    if ( risposta != null && risposta.Success )
                    {
                        if ( risposta.AnalisiEccezione != null && risposta.AnalisiEccezione.Any( ) )
                        {
                            foreach ( var e in risposta.AnalisiEccezione )
                            {
                                string totale = e.totale;

                                if ( e.unitamisura.ToUpper( ) == "H" )
                                {
                                    int min = int.Parse( e.totale );
                                    int ore = 0;

                                    if ( min > 60 )
                                    {
                                        int tempH = ( min / 60 );
                                        ore += tempH;
                                        min -= ( tempH * 60 );
                                    }

                                    if ( min == 60 )
                                    {
                                        ore += 1;
                                        min = 0;
                                    }

                                    totale = String.Format( "{0:00}:{1:00}" , ore , min );
                                }
                                else if ( e.unitamisura.ToUpper( ) == "N" )
                                {
                                    if ( !string.IsNullOrEmpty( e.totale ) )
                                    {
                                        float currVal = float.Parse( e.totale );

                                        if ( currVal > 0 )
                                        {
                                            currVal = currVal / 100;
                                        }

                                        bool confronta = currVal - ( int ) currVal == 0;

                                        if ( confronta )
                                        {
                                            totale = String.Format( "{0}" , ( int ) currVal );
                                        }
                                        else
                                        {
                                            totale = String.Format( "{0}" , currVal );
                                        }
                                    }
                                }

                                response.ContatoriEccezioni.Add( new GetContatoriEccezioniItem( )
                                {
                                    CodiceEccezione = e.codice.Trim( ).ToUpper( ) ,
                                    DatoOriginale = e.totale ,
                                    Errore = string.Empty ,
                                    Esito = true ,
                                    Formato = e.unitamisura ,
                                    Massimale = e.massimale ,
                                    Totale = totale ,
                                    Raw = risposta.raw ,
                                    GiorniSingoli = e.giornisingoli
                                } );
                            }
                        }
                        else
                        {
                            throw new Exception( "Si è verificato un errore nei dati di output del servizio" );
                        }
                    }
                    else if ( risposta != null && !risposta.Success )
                    {
                        if ( !String.IsNullOrEmpty( ecc1 ) )
                        {
                            response.ContatoriEccezioni.Add( new GetContatoriEccezioniItem( )
                            {
                                CodiceEccezione = ecc1 ,
                                DatoOriginale = string.Empty ,
                                Errore = risposta.Error ,
                                Esito = false ,
                                Formato = string.Empty ,
                                Massimale = string.Empty ,
                                Totale = string.Empty ,
                                Raw = risposta.raw
                            } );
                        }

                        if ( !String.IsNullOrEmpty( ecc2 ) )
                        {
                            response.ContatoriEccezioni.Add( new GetContatoriEccezioniItem( )
                            {
                                CodiceEccezione = ecc2 ,
                                DatoOriginale = string.Empty ,
                                Errore = risposta.Error ,
                                Esito = false ,
                                Formato = string.Empty ,
                                Massimale = string.Empty ,
                                Totale = string.Empty ,
                                Raw = risposta.raw
                            } );
                        }

                        if ( !String.IsNullOrEmpty( ecc3 ) )
                        {
                            response.ContatoriEccezioni.Add( new GetContatoriEccezioniItem( )
                            {
                                CodiceEccezione = ecc3 ,
                                DatoOriginale = string.Empty ,
                                Errore = risposta.Error ,
                                Esito = false ,
                                Formato = string.Empty ,
                                Massimale = string.Empty ,
                                Totale = string.Empty ,
                                Raw = risposta.raw
                            } );
                        }
                    }
                    else
                    {
                        throw new Exception( "Errore il servizio non risponde" );
                    }

                } while ( count < eccezioni.Count( ) );

            }
            catch ( Exception ex )
            {
                response.Esito = false;
                response.Errore = ex.Message;
            }

            return response;
        }

        #endregion

        #region Timbratura
        public InserisciTimbraturaResponse InserisciTimbratura(string timbratura)
        {
            //INPUT - Timbratura d'esempio
            //QS01I482    6321010023957009161200001056092100
            /*
                AREA-INPUT.                                               
                codice transazione  	char(4)
                tipo transazione       	char(1) 	[i=timbratura, r= request, c=diagnostica]
                progressivo trasmissione    num(3)
                terminale                   char(4)         sempre vuoto  

                stringa di timbratura  occurs 7 
                TORNELLO-I     char(2)             tornello       
                STATO-TERM-I   char(1)             stato del terminale di trasmissione         
                AZIENDA-I      char(1)             azienda rai      
                BADGE-I        char(2)	       tipo badge, dipendente, visit, ditta, collab...
                vuoto          char(2)             a disposizione
                MATRICOLA-I    char(6)            
                PROG-EMIS-I    char(2)             progressivo emissione per quel dipendente
                DTRILBADGE-I   char(4)             data rilascio badge non sò il formato della data
                RISERVA-1      char(1)             ???
                RISERVA-3      char(3)             ???
                DATA-I         num(4)	       [1,1 = anno, 2,3= giorno]
                ORA-I          num(4)              hhmm
                FLAG-VERSO     char(1)             verso di timbratura
                FLAG-TA-I      char(1)             ???
                (vuoto          cahr(5) 	        a disposizione)
            */

            //OUTPUT
            //ACK0100000NCK482

            InserisciTimbraturaResponse response = new InserisciTimbraturaResponse();

            if (String.IsNullOrWhiteSpace(timbratura))
            {
                response.Esito = false;
                response.Errore = "Timbratura non indicata";
            }
            else
            {
                string strFunzione = "P956,QSP,ARGOMENTO1,ARGOMENTO2," + timbratura;
                try
                {
                    ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
                    string cicsResult = (string)c.ComunicaVersoCics(strFunzione);

                    response.Raw = cicsResult;

                    if (String.IsNullOrWhiteSpace(cicsResult))
                    {
                        response.Esito = false;
                        response.Errore = "Nessuna risposta CICS";
                    }
                    else if (!cicsResult.StartsWith("ACK01"))
                    {
                        response.Esito = false;
                        response.Errore = "Errore "+cicsResult.Substring(0,5);
                    }
                    else
                    {
                        response.Esito = true;
                    }
                }
                catch (Exception ex)
                {
                    response.Esito = false;
                    response.Errore = ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : "");
                }
            }

            return response;
        }
        #endregion
    }

    public class CodiceFiscaleReponse
    {
        public string error { get; set; }
        public bool esito { get; set; }
        public string raw { get; set; }
        public string sent { get; set; }
        public List<CodiceFiscaleInfo> CFinfo { get; set; }
    }
    public class CodiceFiscaleInfo
    {
        public string CodiceUtente { get; set; }
        public string DataIntervento { get; set; }
        public string CodiceIntervento { get; set; }
        public string NominativoFiglio { get; set; }
        public string DataNascita { get; set; }
        public string DataInizioOSS { get; set; }
        public string DataFineOSS { get; set; }
        public string MatricolaGenitore1 { get; set; }
        public string ProgressivoCarichiGenitore1 { get; set; }
        public string SessoGenitore1 { get; set; }
        public string MatricolaGenitore2 { get; set; }
        public string ProgressivoCarichiGenitore2 { get; set; }
        public string SessoGenitore2 { get; set; }
        public string LimiteAF { get; set; }
        public string NumeroGiorniExtraAF { get; set; }
        public string LimiteAF_BF_CF { get; set; }
        public string NumeroGiorniExtraAF_BF_CF { get; set; }
        public string LimiteAF_HF { get; set; }
        public string NumeroGiorniExtraHF { get; set; }
        public string LimiteMU { get; set; }
        public string Switch3MesiAF { get; set; }
        public string VoucherGenitore1 { get; set; }
        public string VoucherGenitore2 { get; set; }
        public string GiorniFruitiAF_genitore1 { get; set; }
        public string GiorniFruitiBF_genitore1 { get; set; }
        public string GiorniFruitiCF_genitore1 { get; set; }
        public string GiorniFruitiHF_genitore1 { get; set; }
        public string GiorniFruitiMU_genitore1 { get; set; }
        public string GiorniFruitiAF_genitore2 { get; set; }
        public string GiorniFruitiBF_genitore2 { get; set; }
        public string GiorniFruitiCF_genitore2 { get; set; }
        public string GiorniFruitiHF_genitore2 { get; set; }
        public string GiorniFruitiMU_genitore2 { get; set; }
        public string CodiceProgressivo { get; set; }
        public string CFfiglioACarico { get; set; }
    }
    /*
     (006)	Codice Utente
(008)	Data Intervento
(001)	Codice Intervento
(025)	Nominativo del Codice Fiscale del figlio
(008)	Data Nascita ( ggmmaaaa )
(008)	Data inizio OSS ( ggmmaaa )
(008)	Data fine OSS ( ggmmaaa )
(007)	Matricola Genitore 1
(002)	Progressivo carichi Genitore 1
(001)	Sesso Genitore 1
(007)	Matricola Genitore 2
(002)	Progressivo carichi Genitore 2
(001)	Sesso Genitore 2
(004)V99	Limite  AF
(003)	Numero giorni extra   AF
(004)V99	Limite AF / BF / CF
(003)	Numero giorni extra   AF /BF /CF
(004)V99	Limite AF / HF
(003)	Numero giorni extra   HF
(004)V99	Limite MU
(001)	Switch 3 mesi AF
(002)	Voucher Genitore 1
(002)	Voucher Genitore 2
(004)V99	Giorni Fruiti AF  Genitore 1
(004)V99	Giorni Fruiti BF  Genitore 1
(004)V99	Giorni Fruiti CF  Genitore 1
(004)V99	Giorni Fruiti HF  Genitore 1
(004)V99	Giorni Fruiti MU  Genitore 1
(004)V99	Giorni Fruiti AF  Genitore 2
(004)V99	Giorni Fruiti BF  Genitore 2
(004)V99	Giorni Fruiti CF  Genitore 2
(004)V99	Giorni Fruiti HF  Genitore 2
(004)V99	Giorni Fruiti MU  Genitore 2
(007)	Codice Progressivo

         */
    public class SetPagamentoEccezioneResponse
    {
        public string error { get; set; }
        public bool esito { get; set; }
        public string raw { get; set; }
        public string sent { get; set; }
    }

    public class SetStatoEccezioneResponse
    {
        public string error { get; set; }
        public bool esito { get; set; }
        public string raw { get; set; }
        public string sent { get; set; }
    }

    public class PeriodoSW
    {
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
    }
    public class GetRipianificazioniMatricolaResponse
    {
        public GetRipianificazioniMatricolaResponse()
        {
            Ripianificazioni = new List<RipianMatricola>();
        }
        public List<RipianMatricola> Ripianificazioni { get; set; }
        public string error { get; set; }
        public bool esito { get; set; }
    }
    public class GetRipianificazioniResponse
    {
        public GetRipianificazioniResponse()
        {
            Sedi = new List<SedeRipianificazioni>();
            esito = true;
        }
        public List<SedeRipianificazioni> Sedi { get; set; }
        public string error { get; set; }
        public bool esito { get; set; }
    }
    public class SedeRipianificazioni
    {
        public SedeRipianificazioni()
        {
            Reparti = new List<Rep>();
        }
        public string Sede { get; set; }
        public List<Rep> Reparti { get; set; }
    }

    public class Rep
    {
        public Rep()
        {
            Ripianificazioni = new List<Ripian>();
        }
        public string reparto { get; set; }
        public List<Ripian> Ripianificazioni = new List<Ripian>();
    }

    public class Ripian
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string CodiceEccezione { get; set; }
        public DateTime DataEccezione { get; set; }
        public DateTime? DataRipianificazione { get; set; }
        public int stato { get; set; }
        public string motivo { get; set; }
        public string MatricolaApprovatoreStorno { get; set; }
        public string NominativoApprovatoreStorno { get; set; }
        public DateTime? DataApprovazioneStorno { get; set; }
    }
    public class RipianMatricola
    {
        public string CodiceEccezione { get; set; }
        public DateTime DataEccezione { get; set; }
        public DateTime? DataRipianificazione { get; set; }
        public int stato { get; set; }
        public string motivo { get; set; }
        public string MatricolaApprovatoreStorno { get; set; }
        public string NominativoApprovatoreStorno { get; set; }
        public DateTime? DataApprovazioneStorno { get; set; }
    }
    public class InviaPianoFerieResponse
    {
        public Boolean esito { get; set; }
        public string error { get; set; }
    }

    public class SetRuoloResponse
    {

        public bool esito { get;  set; }
        public string error { get;  set; }
        public it.rai.servizi.svildigigappws.Periodo[] RispostaScadenzario { get; set; }
        public string raw { get;  set; }
    }

    public class ResocontiResponse
    {
        public List<SedeData> SediData { get; set; }
    }

    public class SedeData
    {
        public string Sede { get; set; }
        public DateTime DataInizio { get; set; }
        public bool esito { get; set; }
        public string error { get; set; }
    }
}