using myRai.Business;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using TimbratureCore;

namespace myRaiHelper
{
    public class ServiceWrapper
    {

        public static void ModificaFittizio(monthResponseEccezione response)
        {
            //var g = response.data.giornate.Where(x => x.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza).FirstOrDefault();
            //if (g != null)
            //{
            //    g.TipoEcc = digigappWS_ws1.TipoEccezione.TimbratureInSW;
            //    g.data = new DateTime(2020, 9, 24);
            //}
           
        }
        public static monthResponseEccezione ListaEvidenzWrapper(WSDigigapp datiBack_ws1, string matricola, string datainizio, string datafine, int livelloUtente)
        {
            monthResponseEccezione response;

            if (CommonManager.GetParametro<bool>(EnumParametriSistema.AttivaListaEvidenze3VA))
            {

                response = datiBack_ws1.ListaEvidenze3VA(matricola, datainizio, datafine, myRai.Models.Utente.TipoDipendente(), myRai.Models.Utente.SedeGapp(), livelloUtente);
                ModificaFittizio(response);
            }
            else
            {
                response = datiBack_ws1.ListaEvidenze(matricola, datainizio, datafine, livelloUtente);
            }
            if (response != null && response.data!=null)
            {
                var swtim = response.data.giornate.Where(x => x.TipoEcc == TipoEccezione.TimbratureInSW).ToList();
                if (swtim.Any())
                {
                    var db = new myRaiData.digiGappEntities();
                    List<Evidenza> L = new List<Evidenza>();
                    foreach (var e in response.data.giornate)
                    {
                        if (e.TipoEcc != TipoEccezione.TimbratureInSW)
                            L.Add(e);
                        else
                        {
                            if (myRai.Models.Utente.TipoDipendente() == "D" || myRai.Models.Utente.TipoDipendente() == "G")
                                continue;

                            myRaiData.MyRai_Eccezioni_Richieste er = db.MyRai_Eccezioni_Richieste.Where(x =>
                            x.MyRai_Richieste.matricola_richiesta == matricola && x.data_eccezione == e.data && x.cod_eccezione == "SW").FirstOrDefault();
                            if (er == null)
                                continue;

                            if (er != null && er.MyRai_Richieste.MyRai_Eccezioni_Richieste.Any(x => x.azione == "C"))
                            {
                                continue;
                            }
                            else
                            {
                                e.IdSWdaStornare = er.MyRai_Richieste.id_richiesta;
                                L.Add(e);
                            }
                        }
                    }
                    response.data.giornate = L.ToArray();
                }
            }
            return response;
        }

        public static proposteResponse GetProposteAutomaticheWrapper(string matr, string data, dayResponse dayResp=null, IEnumerable<string> extraAssemblies = null)
        {
            WSDigigapp service = new WSDigigapp()
            {
                Credentials = CommonHelper.GetUtenteServizioCredentials()
            };
            DateTime D;
            DateTime.TryParseExact(data.Replace("/", ""), "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D);

            proposteResponse respPropAuto = service.getProposteAutomatiche(matr, data.Replace("/", ""), "BU", 90);
            var db = new myRaiData.digiGappEntities();

            string EccIniettateProposteAuto = CommonManager.GetParametro<string>(EnumParametriSistema.ProposteAutoIniettateRicalcolo);
            string EccRicalcolo = CommonManager.GetParametro<string>(EnumParametriSistema.ProposteAutoRicalcoloQuantita);

            List<string> EccArr = new List<string>();
            List<string> EccIniettate = new List<string>();
            List<string> EccIniettate_UnitaMisuraN = new List<string>();

            WSDigigappDataController sessionService = new WSDigigappDataController();
            if (dayResp==null)
                dayResp = sessionService.GetEccezioni(CommonHelper.GetCurrentUserMatricola(), matr, data.Replace("/", ""), "BU", 80);

            if (!String.IsNullOrWhiteSpace(EccRicalcolo))
            {
                EccArr = EccRicalcolo.Split(',').ToList();
                ///EccArr.RemoveAll(x => dayResp.ContieneEccezione(x));
            }
            if (!String.IsNullOrWhiteSpace(EccIniettateProposteAuto) && D < DateTime.Today)
            {
                var ListAllAmmesse = db.MyRai_Eccezioni_Ammesse.ToList();

                var ListaIniettate = EccIniettateProposteAuto.Split(',').ToList();
                //ListaIniettate.RemoveAll(x=>dayResp.ContieneEccezione(x));

                foreach (var item in ListaIniettate)
                {
                    var ammessa = ListAllAmmesse.Where(x => x.cod_eccezione == item.Trim()).FirstOrDefault();
                    if (ammessa == null) continue;
                    if (ammessa.Prontuario)
                    {
                        if (EccezioniManager.ConsentitaDaProntuario(dayResp.giornata))
                            EccIniettate.Add(item);
                    }
                    else
                        EccIniettate.Add(item);
                }
            }

            if (EccArr.Count == 0 && EccIniettate.Count == 0) return respPropAuto;

            string[] codiciAmmessi = null;
            if (EccIniettate.Any())
            {
                List<myRaiData.MyRai_Eccezioni_Ammesse> ammesse = EccezioniManager.GetListaEccezioniPossibili(CommonManager.GetCurrentUserMatricola(), D);
                codiciAmmessi = ammesse.Select(x => x.cod_eccezione).ToArray();
                EccIniettate.RemoveAll(x => !codiciAmmessi.Contains(x));
            }

            if (EccIniettate.Any())
            {
                foreach(string ei in EccIniettate)
                {
                    var ec = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione.Trim() == ei).FirstOrDefault();
                    if (ec != null && ec.unita_misura != null && ec.unita_misura.Trim() == "N")
                    {
                        EccIniettate_UnitaMisuraN.Add(ei);
                    }
                }
                EccIniettate.RemoveAll(x => EccIniettate_UnitaMisuraN.Contains(x));

                foreach (string eIni in EccIniettate)
                {
                    if (!String.IsNullOrWhiteSpace(eIni) && !respPropAuto.eccezioni.Any(x => x.cod.Trim() == eIni))
                    {
                        string d = CommonManager.GetDescrizioneEccezione(eIni);
                        var L = respPropAuto.eccezioni.ToList();

                        if (eIni == "FMH")
                        {
                            if (dayResp.eccezioni != null && dayResp.eccezioni.Any(x => x.cod.Trim() == "CAR"))
                            {
                                var c = dayResp.eccezioni.Where(x => x.cod.Trim() == "CAR").FirstOrDefault();
                                if (c.qta.Trim().ToMinutes() > 0)
                                {
                                    L.Add(new MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione()
                                    {
                                        qta = c.qta.Trim(),
                                        cod = eIni,
                                        IniettataDaRaiPerMe = true,
                                        data = data.Replace("/", ""),
                                        dalle = "00:00",
                                        alle = "00:00",
                                        descrittiva_lunga = d != null ? d.ToUpper() : ""
                                    });
                                }
                            }
                        }
                        else
                        {
                            L.Add(new MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione()
                            {
                                qta = "00:00",
                                cod = eIni,
                                IniettataDaRaiPerMe = true,
                                data = data.Replace("/", ""),
                                dalle = "00:00",
                                alle = "00:00",
                                descrittiva_lunga = d != null ? d.ToUpper() : ""
                            });
                        }
                        
                        respPropAuto.eccezioni = L.ToArray();
                        if (!EccArr.Contains(eIni)) EccArr.Add(eIni);
                    }
                }
            }

            string[] eccezioniframmentate = CommonManager.GetParametro<string>(EnumParametriSistema.EccezioniFrag).Split(',');
            if (eccezioniframmentate != null && eccezioniframmentate.Any())
            {
                if (codiciAmmessi == null)
                {
                    List<myRaiData.MyRai_Eccezioni_Ammesse> ammesse = EccezioniManager.GetListaEccezioniPossibili(CommonManager.GetCurrentUserMatricola(), D);
                    codiciAmmessi = ammesse.Select(x => x.cod_eccezione).ToArray();
                }

                foreach (string e in eccezioniframmentate)
                {
                    if (!codiciAmmessi.Contains(e)) continue;

                    if (!dayResp.eccezioni.Any(x => x.cod.Trim() == e)) continue;

                    var L = respPropAuto.eccezioni.ToList();
                    if (L.Any(x => x.cod.Trim() == e)) continue;

                    string d = CommonManager.GetDescrizioneEccezione(e);
                    L.Add(new MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione()
                    {
                        qta = "00:00",
                        cod = e,
                        IniettataDaRaiPerMe = true,
                        data = data.Replace("/", ""),
                        dalle = "00:00",
                        alle = "00:00",
                        descrittiva_lunga = d != null ? d.ToUpper() : ""
                    });
                    respPropAuto.eccezioni = L.ToArray();
                }
            }


            if (respPropAuto != null && respPropAuto.eccezioni != null && respPropAuto.eccezioni.Any())
            {
                List<string> DarimuoverePercheAltreOverSogliaNotturno = new List<string>();

                foreach (var eccezione in respPropAuto.eccezioni.Where(x => !String.IsNullOrWhiteSpace(x.cod)))
                {
                    string codiceEcc = eccezione.cod.Trim().ToUpper();

                    if (EccArr.Contains(codiceEcc) || EccArr.Contains("*"))
                    {
                        if (DayAnalysisFactory.DayAnalysisExists(codiceEcc))
                        {
                            if (dayResp == null)
                                dayResp = service.getEccezioni(matr, data.Replace("/", ""), "BU", 80);

                            dayResponse dayRespYesterday = null;
                            if (codiceEcc == "AT30")
                            {
                                dayRespYesterday = service.getEccezioni(matr, D.AddDays(-1).ToString("ddMMyyyy"), "BU", 80);
                                if (dayRespYesterday.orario.cod_orario == "96")
                                    dayRespYesterday = service.getEccezioni(matr, D.AddDays(-2).ToString("ddMMyyyy"), "BU", 80);
                            }

                            var DA = DayAnalysisFactory.GetDayAnalysisClass(codiceEcc, dayResp, myRai.Models.Utente.GetQuadratura() == Quadratura.Giornaliera, dayRespYesterday);
                            if (DA != null)
                            {
                                var QuantitaRicalcolata = DA.GetEccezioneQuantita();
                                if (DA.EccezioniEliminateSeOverSogliaNotturno != null)
                                    DarimuoverePercheAltreOverSogliaNotturno.AddRange(DA.EccezioniEliminateSeOverSogliaNotturno.Split(',').ToList());
                                var QuantitaRicalcolataIpotesiNoCarenza = DA.GetEccezioneQuantitaIpotesiNocarenza();
                                eccezione.qta_IpotesiNocarenza = QuantitaRicalcolataIpotesiNoCarenza.QuantitaMinutiHHMM;

                                if (QuantitaRicalcolata != null && QuantitaRicalcolata.QuantitaMinuti > 0)
                                {
                                    string eccezioni = " - TipoDip:" + myRai.Models.Utente.TipoDipendente();
                                    if (dayResp != null && dayResp.eccezioni != null && dayResp.eccezioni.Any())
                                        eccezioni += " - Ecc:" + String.Join(",", dayResp.eccezioni.Select(x => x.cod).ToArray());

                                    if (eccezione.qta != null && eccezione.qta.Trim() != QuantitaRicalcolata.QuantitaMinutiHHMM)
                                    {
                                        string iniettata = eccezione.IniettataDaRaiPerMe ? "(IN)" : "";
                                        Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
                                        {
                                            applicativo = "PORTALE",
                                            data = DateTime.Now,
                                            matricola = matr,
                                            provenienza = "GetProposteAutomaticheWrapper",
                                            operazione = "Quantita PA Rewrite",
                                            descrizione_operazione = eccezione.cod + iniettata + " del " + data + " " + eccezione.qta + " --> " + QuantitaRicalcolata.QuantitaMinutiHHMM + eccezioni
                                        });
                                        if (eccezioniframmentate != null && eccezioniframmentate.Any(x=>x==codiceEcc))
                                        {
                                            var eccPresente= dayResp.eccezioni.Where(x => x.cod.Trim() == codiceEcc).FirstOrDefault();
                                            if (eccPresente != null)
                                            {
                                                if (eccPresente.qta.ToMinutes() > 0)
                                                {
                                                    QuantitaRicalcolata.QuantitaMinuti -= eccPresente.qta.ToMinutes();
                                                    if (QuantitaRicalcolata.QuantitaMinuti < 0) QuantitaRicalcolata.QuantitaMinuti = 0;
                                                    QuantitaRicalcolata.QuantitaMinutiHHMM = QuantitaRicalcolata.QuantitaMinuti.ToHHMM();
                                                }
                                            }
                                        }

                                        eccezione.qta = " " + QuantitaRicalcolata.QuantitaMinutiHHMM;
                                        eccezione.RicalcolataDaRaiPerMe = true;
                                        if (!String.IsNullOrWhiteSpace(QuantitaRicalcolata.Alle)) eccezione.alle = QuantitaRicalcolata.Alle;
                                        if (!String.IsNullOrWhiteSpace(QuantitaRicalcolata.Dalle)) eccezione.dalle = QuantitaRicalcolata.Dalle;
                                    }
                                    else
                                    {
                                        Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
                                        {
                                            applicativo = "PORTALE",
                                            data = DateTime.Now,
                                            matricola = matr,
                                            provenienza = "GetProposteAutomaticheWrapper",
                                            operazione = "Quantita PA OK",
                                            descrizione_operazione = eccezione.cod + " del " + data + " " + eccezione.qta + eccezioni
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                var l = respPropAuto.eccezioni.ToList();
                l.RemoveAll(x => x.tipo_eccezione == null && x.qta.ToMinutes() == 0);
                if (DarimuoverePercheAltreOverSogliaNotturno.Any())
                {
                    l.RemoveAll(x => DarimuoverePercheAltreOverSogliaNotturno.Contains(x.cod));
                }
                respPropAuto.eccezioni = l.ToArray();

                respPropAuto.eccezioni = respPropAuto.eccezioni.Where(x => x.tipo_eccezione != "H" || x.qta.ToMinutes() > 0).ToArray();
            }
            respPropAuto = Check75Notturno(respPropAuto, dayResp);

            if (EccIniettate_UnitaMisuraN.Any())
            {
                List<MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione> E = respPropAuto.eccezioni.ToList();
                foreach (string en in EccIniettate_UnitaMisuraN)
                {
                    if (! E.Select(x=>x.cod.Trim()).ToList().Contains(en))
                    {
                        string d = CommonManager.GetDescrizioneEccezione(en);
                        E.Add(new MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione()
                        {
                            qta = "1",
                            cod = en,
                            IniettataDaRaiPerMe = true,
                            data = data.Replace("/", ""),
                            dalle = "",
                            alle = "",
                            descrittiva_lunga = d != null ? d.ToUpper() : ""
                        });
                    }
                }
                respPropAuto.eccezioni = E.ToArray();
            }
            return respPropAuto;
        }

      

        private static proposteResponse Check75Notturno(proposteResponse respPropAuto, dayResponse dayResp)
        {
            if (TimbratureCore.TimbratureManager.TurnoMaggioreDiSogliaNotturno(dayResp.orario.hhmm_entrata_48, dayResp.orario.hhmm_uscita_48))
            {
                foreach (string e in "LFH,SRH".Split(','))
                {
                    if (respPropAuto.eccezioni != null &&
                   respPropAuto.eccezioni.Any(x => x.cod == e + "8") &&
                   respPropAuto.eccezioni.Any(x => x.cod == e + "6"))
                    {
                        var e6 = respPropAuto.eccezioni.Where(x => x.cod == e + "6").FirstOrDefault();
                        var e8 = respPropAuto.eccezioni.Where(x => x.cod == e + "8").FirstOrDefault();
                        e8.qta = (e8.qta.ToMinutes() + e6.qta.ToMinutes()).ToHHMM();
                        respPropAuto.eccezioni = respPropAuto.eccezioni.Where(x => x.cod != e + "6").ToArray();
                    }
                }
            }
            return respPropAuto;
        }

        public static string EsponiAnagraficaRaicvWrapped(string matricola)
        {
            string response = string.Empty;
            if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("api/"))
            {
                myRaiServiceHub.it.rai.servizi.hrgb.Service service = new myRaiServiceHub.it.rai.servizi.hrgb.Service();
                response = service.EsponiAnagrafica("RAICV;" + matricola + ";;E;0");
                return response;
            }

            if (SessionHelper.Get("ANAG_RAICV") == null)
            {
                myRaiServiceHub.it.rai.servizi.hrgb.Service service = new myRaiServiceHub.it.rai.servizi.hrgb.Service();
                response = service.EsponiAnagrafica("RAICV;" + matricola + ";;E;0");
                SessionHelper.Set("ANAG_RAICV", response);
            }
            else
            {
                response = (string)SessionHelper.Get("ANAG_RAICV");
                response = response.Substring(0, 6);
                if (matricola != response)
                {
                    myRaiServiceHub.it.rai.servizi.hrgb.Service service = new myRaiServiceHub.it.rai.servizi.hrgb.Service();
                    response = service.EsponiAnagrafica("RAICV;" + matricola + ";;E;0");
                    SessionHelper.Set("ANAG_RAICV", response);
                }
            }
            return (string)SessionHelper.Get("ANAG_RAICV");
        }

        //Inserimento singola eccezione
        public static updateResponse updateEccezione(WSDigigapp service,
                    string matricola,
                    string data,
                    string cod,
                    string oldArea,
                    string quantita,
                    string dalle,
                    string alle,
                    string importo,
                    char storno,
                    string ndoc,
                    string note,
                    string uorg,
                    string df,
                    string ms,
                    string new_orario_teorico,
                    string new_orario_reale,
                    int livelloUtente,
                    dayResponse resp)
        {

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = matricola,
                operazione = "updateEccezione Request",
                provenienza = "ServiceWrapper.updateEccezione [" + timestamp + "]",
                descrizione_operazione = "UpdateEccezione matr:'" + matricola + "' data:'" + data + "' cod:'" + cod + "' oldarea:'"
                + oldArea + "' quantita:'" + quantita + "' dalle:'" + dalle + "'" +
                " alle:'" + alle + "' importo:'" + importo + "' storno:'" + storno + "' ndoc:'" + ndoc + "' note:'" + note +
                "' uorg:'" + uorg + "' df:'" + df + "' ms:'" + ms + "' new_orario_teorico:'" + new_orario_teorico +
                "' new_orario_reale:'" + new_orario_reale + "' livUtente:'" + livelloUtente + "'"
            },matricola);
            updateResponse response = new updateResponse(); 
            if (cod == "RMTR")
            {
                DateTime D;
                DateTime Dnow = DateTime.Now;

                if (DateTime.TryParseExact(data, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D))
                {
                    Dnow = D;
                }

                var db = new myRaiData.digiGappEntities();
                string sedeutente = myRai.Models.Utente.SedeGapp(Dnow);
                //var sede = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sedeutente).FirstOrDefault();

                //if (sede == null || String.IsNullOrWhiteSpace(sede.importo_rimborso))
                //{
                //    response.esito = false;
                //    response.codErrore = "999";
                //    response.descErrore = "Importo o sede gapp non trovati";
                //    return response;
                //}
                //else
                //{
                    quantita = "1";
                //if (sede.CalendarioDiSede == "VE")
                //    importo = myRaiCommonTasks.CommonTasks.GetImportoRMTR_Venezia(D);
                //else
                //    importo = sede.importo_rimborso;

                importo = EccezioniManager.GetIntervalloRMTRinsediamento(resp, "I");
                if (importo == "0")
                {
                    return new updateResponse() { esito= false, descErrore="Insediamento non trovato per importo RMTR" };
                }

                    dalle = "";
                    alle = "";
                //}
            }
            try
            {
                response = service.updateEccezione(
                             matricola,
                             data,
                             cod,
                             oldArea,
                             quantita,
                             dalle,
                             alle,
                             importo,
                             storno,
                             ndoc,
                             note,
                             uorg,
                             df,
                             ms,
                             new_orario_teorico,
                             new_orario_reale,
                             livelloUtente);

            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = matricola,
                    provenienza = "ServiceWrapper.updateEccezione [" + timestamp + "]",
                    error_message = ex.ToString()
                },matricola);
                response.esito = false;
                response.descErrore = ex.Message;
                return response;
        }

            Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
        {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = matricola,
                operazione = "updateEccezione Response ",
                provenienza = "ServiceWrapper.updateEccezione [" + timestamp + "]",
                descrizione_operazione = "Esito:" + response.esito + " Errore:" + response.descErrore + " Raw:" + response.rawInput
            },matricola);

            return response;
        }

        //Inserimento array eccezioni
        public static validationResponse aggiornaEccezioni(WSDigigapp service, string matricola, MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione[] arrayEccezioni,
            int livUtente)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string json = "";
            try
            {
                json = new JavaScriptSerializer().Serialize(arrayEccezioni);
            }
            catch (Exception ex)
            {
                json = ex.Message;
            }

            Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = matricola,
                operazione = "aggiornaEccezioni Request",
                provenienza = "ServiceWrapper.aggiornaEccezioni [" + timestamp + "]",
                descrizione_operazione = "aggiornaEccezioni matr:'" + matricola + " livUtente:" + livUtente + " ArrayEccezioni:" + json
            });

            validationResponse response = new validationResponse();

            try
            {
                response = service.aggiornaEccezioni(matricola, arrayEccezioni, livUtente);
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = matricola,
                    provenienza = "ServiceWrapper.aggiornaEccezioni [" + timestamp + "]",
                    error_message = ex.ToString()
                });
                response.esito = false;
                response.errore = ex.Message;
                return response;
            }
            Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = matricola,
                operazione = "aggiornaEccezioni Response ",
                provenienza = "ServiceWrapper.aggiornaEccezioni [" + timestamp + "]",
                descrizione_operazione = "Esito:" + response.esito + " Errore:" + response.errore
            });
            return response;
            }

        //Aprova-rifiuta eccezione
        public static validationResponse validaEccezioni(WSDigigapp service, string matricola, string livelloResponsabile, MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione[] e,
            Boolean Valida, int livUtente)
        {
            service.Credentials = CommonHelper.GetUtenteServizioCredentials();

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string json = new JavaScriptSerializer().Serialize(e);

            Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = matricola,
                operazione = "validaEccezioni..Request [" + Valida + "]",
                provenienza = "ServiceWrapper..validaEccezioni [" + timestamp + "]",
                descrizione_operazione = "validaEccezioni matr:" + matricola + " livResponsabile:" + livelloResponsabile +
                " livUtente:" + livUtente + " Valida:" + Valida + " ArrayEccezioni:" + json
            });
            validationResponse response = new validationResponse();

            try
            {
                response = service.validaEccezioni(matricola, livelloResponsabile, e, Valida, livUtente);
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = matricola,
                    provenienza = "ServiceWrapper..ValidaEccezioni [" + timestamp + "]",
                    error_message = ex.ToString()
                });
                response.esito = false;
                response.errore = ex.Message;
                return response;
            }
            Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = matricola,
                operazione = "validaEccezioni..Response [" + Valida + "]",
                provenienza = "ServiceWrapper..validaEccezioni [" + timestamp + "]",
                descrizione_operazione = "Esito:" + response.esito + " Errore:" + response.errore
            });
            return response;

        }


        public static pianoFerie GetPianoFerieWrapped(WSDigigapp service, string matricola, string datainizio, int livelloutente, string tipologiaUtente)
        {
            pianoFerie p = new pianoFerie();

            try
            {
                // se la tipologia utente è vuota o è nulla rimanda al _getPianoFerieWrappedInternal
                // che non controlla la sessione in quanto la chiamate probabilmente proviene da API
                if (String.IsNullOrEmpty(tipologiaUtente))
                {
                    return _getPianoFerieWrappedInternal(service, matricola, datainizio, livelloutente, tipologiaUtente);
                }

                if (ServiceWrapperrScope.Instance.GetPianoFerieWrapped != null)
                {
                    if (ServiceWrapperrScope.Instance.GetPianoFerieWrapped.HasData() &&
                        ServiceWrapperrScope.Instance.GetPianoFerieWrapped.IsEqualsTo(matricola, datainizio, livelloutente, tipologiaUtente))
                    {
                        return ServiceWrapperrScope.Instance.GetPianoFerieWrapped.GetResponse();
                    }
                }
                else
                {
                    ServiceWrapperrScope.Instance.GetPianoFerieWrapped = new GetPianoFerieWrapped();
                }

                ServiceWrapperrScope.Instance.GetPianoFerieWrapped.SetResponse(matricola, datainizio, livelloutente, tipologiaUtente,
                    service.getPianoFerie(matricola, datainizio, livelloutente, tipologiaUtente));

                p = ServiceWrapperrScope.Instance.GetPianoFerieWrapped.GetResponse();

                int anno = int.Parse(datainizio.Substring(4, 4));

                bool attivaRN = false;
                string tipiDipendenteEsclusiperRN = "";
                //ParametriSistemaValoreJson
                List<ParametriSistemaValoreJson> parametri = new List<ParametriSistemaValoreJson>( );
                parametri = CommonManager.GetParametriJson( EnumParametriSistema.ParametriServiceWrapper );

                if (parametri != null && parametri.Any())
                {
                    var item = parametri.Where( w => w.Attributo == "VisualizzaRN" ).FirstOrDefault( );

                    if (item != null)
                    {
                        if ( !String.IsNullOrEmpty( item.Valore1 ) )
                        {                            
                            try
                            {
                                attivaRN = bool.Parse( item.Valore1 );
                            }
                            catch ( Exception ex )
                            {
                                attivaRN = false;
                            }
                        }
                    }

                    item = parametri.Where( w => w.Attributo == "VisualizzaRNGiornalisti" ).FirstOrDefault( );

                    if ( item != null && p.dipendente.ferie.visualizzaPermessiGiornalisti == true )
                    {
                        if ( !String.IsNullOrEmpty( item.Valore1 ) )
                        {
                            try
                            {
                                attivaRN = bool.Parse( item.Valore1 );
                            }
                            catch ( Exception ex )
                            {
                                attivaRN = false;
                            }
                        }
                    }

                    item = parametri.Where( w => w.Attributo == "VisualizzaRNForzati" ).FirstOrDefault( );
                    if ( item != null )
                    {
                        if ( !String.IsNullOrEmpty( item.Valore1 ) )
                        {
                            if ( item.Valore1.Contains( matricola ) )
                            {
                                attivaRN = true;
                            }
                        }
                    }

                    item = parametri.Where( w => w.Attributo == "TipoDipendenteEscluso" ).FirstOrDefault( );

                    if ( item != null )
                    {
                        if ( !String.IsNullOrEmpty( item.Valore1 ) )
                        {
                            try
                            {
                                tipiDipendenteEsclusiperRN = item.Valore1;
                            }
                            catch ( Exception ex )
                            {
                                tipiDipendenteEsclusiperRN = "";
                            }
                        }
                    }
                }

                p.dipendente.ferie.visualizzaRecuperoNonLavorati = attivaRN;

                if (String.IsNullOrWhiteSpace(tipologiaUtente))
                {
                    return p;
                }
                if (matricola.Length == 6) matricola = "0" + matricola;

                try
                {
                    if (p.dipendente.ferie != null && CommonManager.GetCurrentUserMatricola7chars() == matricola
                        && myRai.Models.Utente.TipoDipendente() == "D")
                    {
                        p.dipendente.ferie.visualizzaRecuperoNonLavorati = false;//temporaneo
                        p.dipendente.ferie.visualizzaRecuperoFestivi = false;//temporaneo
                        p.dipendente.ferie.visualizzaRecuperoRiposi = false;//temporaneo
                        p.dipendente.ferie.visualizzaFC = false;
                    }
                }
                catch (Exception ex)
                {
                    if (p.dipendente.ferie != null && CommonManager.GetCurrentUserMatricola7chars() == matricola)
                        p.dipendente.ferie.visualizzaRecuperoNonLavorati = false;//temporaneo
                }
            }
            catch (Exception ex)
            {
                string msg = String.Format("myRai.Business - GetPianoFerieWrapped - Parametri: matricola = {0}, data = {1}, tipologiaUtente = {2}, livelloUtente = {3} \n", matricola, datainizio, tipologiaUtente, livelloutente);
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    error_message = msg + ex.ToString(),
                    provenienza = "ServiceWrapper.GetPianoFerieWrapped"
                });
                return null;
            }

            return p;
        }



        //Inserimento singola eccezione
        public static updateResponse updateEccezioneForBatch(WSDigigapp service,
                    string matricola,
                    string data,
                    string cod,
                    string oldArea,
                    string quantita,
                    string dalle,
                    string alle,
                    string importo,
                    char storno,
                    string ndoc,
                    string note,
                    string uorg,
                    string df,
                    string ms,
                    string new_orario_teorico,
                    string new_orario_reale,
                    int livelloUtente)
        {

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = matricola,
                operazione = "updateEccezione Request",
                provenienza = "ServiceWrapper.updateEccezione [" + timestamp + "]",
                descrizione_operazione = "UpdateEccezione matr:'" + matricola + "' data:'" + data + "' cod:'" + cod + "' oldarea:'"
                + oldArea + "' quantita:'" + quantita + "' dalle:'" + dalle + "'" +
                " alle:'" + alle + "' importo:'" + importo + "' storno:'" + storno + "' ndoc:'" + ndoc + "' note:'" + note +
                "' uorg:'" + uorg + "' df:'" + df + "' ms:'" + ms + "' new_orario_teorico:'" + new_orario_teorico +
                "' new_orario_reale:'" + new_orario_reale + "' livUtente:'" + livelloUtente + "'"
            }, matricola);

            updateResponse response = new updateResponse();
            if (cod == "RMTR")
            {
                DateTime D;
                DateTime Dnow = DateTime.Now;

                if (DateTime.TryParseExact(data, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D))
                {
                    Dnow = D;
                }

                var userData = BatchManager.GetUserData(matricola,DateTime .Now);

                var db = new myRaiData.digiGappEntities();
                string sedeutente = BatchManager.SedeGapp(userData);
                var sede = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sedeutente).FirstOrDefault();
                if (sede == null || String.IsNullOrWhiteSpace(sede.importo_rimborso))
                {
                    response.esito = false;
                    response.codErrore = "999";
                    response.descErrore = "Importo o sede gapp non trovati";
                    return response;
                }
                else
                {
                    quantita = "1";
                    if (sede.CalendarioDiSede == "VE")
                        importo = myRaiCommonTasks.CommonTasks.GetImportoRMTR_Venezia(D);
                    else
                        importo = sede.importo_rimborso;
                    dalle = "";
                    alle = "";
                }
            }
            try
            {
                response = service.updateEccezione(
                             matricola,
                             data,
                             cod,
                             oldArea,
                             quantita,
                             dalle,
                             alle,
                             importo,
                             storno,
                             ndoc,
                             note,
                             uorg,
                             df,
                             ms,
                             new_orario_teorico,
                             new_orario_reale,
                             livelloUtente);

            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = matricola,
                    provenienza = "ServiceWrapper.updateEccezione [" + timestamp + "]",
                    error_message = ex.ToString()
                });
                response.esito = false;
                response.descErrore = ex.Message;
                return response;
            }

            Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = matricola,
                operazione = "updateEccezione Response ",
                provenienza = "ServiceWrapper.updateEccezione [" + timestamp + "]",
                descrizione_operazione = "Esito:" + response.esito + " Errore:" + response.descErrore + " Raw:" + response.rawInput
            }, matricola);

            return response;
        }



        private static pianoFerie _getPianoFerieWrappedInternal(WSDigigapp service, string matricola, string datainizio, int livelloutente, string tipologiaUtente)
        {
            pianoFerie p = new pianoFerie();

            try
            {
                p = service.getPianoFerie(matricola, datainizio, livelloutente, tipologiaUtente);

             

                int anno = int.Parse(datainizio.Substring(4, 4));

                if (p.dipendente.ferie != null && p.dipendente.ferie.visualizzaPermessiGiornalisti == true)
                    p.dipendente.ferie.visualizzaRecuperoNonLavorati = false;//temporaneo

                if (String.IsNullOrWhiteSpace(tipologiaUtente))
                {
                    return p;
                }
                if (matricola.Length == 6) matricola = "0" + matricola;

                try
                {
                    if (p.dipendente.ferie != null && CommonManager.GetCurrentUserMatricola7chars() == matricola
                        && myRai.Models.Utente.TipoDipendente() == "D")
                    {
                        p.dipendente.ferie.visualizzaRecuperoNonLavorati = false;//temporaneo
                        p.dipendente.ferie.visualizzaRecuperoFestivi = false;//temporaneo
                        p.dipendente.ferie.visualizzaRecuperoRiposi = false;//temporaneo
                        p.dipendente.ferie.visualizzaFC = false;
                    }
                }
                catch (Exception ex)
                {
                    if (p.dipendente.ferie != null && CommonManager.GetCurrentUserMatricola7chars() == matricola)
                        p.dipendente.ferie.visualizzaRecuperoNonLavorati = false;//temporaneo
                }
            }
            catch (Exception ex)
            {
                string msg = String.Format("myRai.Business - GetPianoFerieWrapped - Parametri: matricola = {0}, data = {1}, tipologiaUtente = {2}, livelloUtente = {3} \n", matricola, datainizio, tipologiaUtente, livelloutente);
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    error_message = msg + ex.ToString(),
                    provenienza = "ServiceWrapper.GetPianoFerieWrapped"
                });
                return null;
            }

            return p;

            //// RV
            //var rv = GetRV( p.dipendente, anno );
            //p.dipendente.ferie.vacanzaCompViaggioRipo = rv;
            //p.dipendente.ferie.viaggioGiornoDiRiposo = rv;

            //// FV
            //var fv = GetFV( p.dipendente, anno );
            //p.dipendente.ferie.vacanzaCompViaggioFest = fv;
            //p.dipendente.ferie.viaggioInFestivo = fv;

            //if ( CommonManager.GetCurrentUserMatricola () ==matricola &&  myRai.Models.Utente.TipoDipendente()=="D")
            //	p.dipendente.ferie.visualizzaRecuperoNonLavorati = false;//temporaneo

            //int anno = int.Parse( datainizio.Substring( 4, 4 ) );

            //var x = GetMN( p, anno );


            //var x = GetMR( p.dipendente, anno );

            //if ( x != null && x.ListMR != null && x.ListMR.Any() )
            //{
            //	p.dipendente.ferie.recuperiMancatiRiposiRimanenti += x.ListMR.Count( mr => mr.Valido );
            //}

            //if ( x != null && x.ListMN != null && x.ListMN.Any() )
            //{
            //	float sum = 0;
            //	var temp = x.ListMN.Where( i => !i.AnnullatoDaRN && !i.Scaduto ).ToList();

            //	if ( temp != null && temp.Any() )
            //	{
            //		temp.ForEach( t =>
            //		{
            //			sum += t.Spettante;
            //		} );
            //	}

            //	p.dipendente.ferie.mancatoNonLavorato = sum;
            //}

            //var y = GetPXC( p, anno );

            //if ( y != null )
            //{
            //	p.dipendente.ferie.permRecuperoGiornalisti = y.TotalePXC;
            //}
        }






        //Aprova-rifiuta eccezione
        public static validationResponse validaEccezioniForBatch(WSDigigapp service, string matricola, string livelloResponsabile, MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione[] e,
            Boolean Valida, int livUtente)
        {
            service.Credentials = new System.Net.NetworkCredential(BatchManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], BatchManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string json = new JavaScriptSerializer().Serialize(e);

            Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = matricola,
                operazione = "validaEccezioni..Request [" + Valida + "]",
                provenienza = "ServiceWrapper..validaEccezioni [" + timestamp + "]",
                descrizione_operazione = "validaEccezioni matr:" + matricola + " livResponsabile:" + livelloResponsabile +
                " livUtente:" + livUtente + " Valida:" + Valida + " ArrayEccezioni:" + json
            }, matricola);
            validationResponse response = new validationResponse();

            try
            {
                response = service.validaEccezioni(matricola, livelloResponsabile, e, Valida, livUtente);
        }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = matricola,
                    provenienza = "ServiceWrapper..ValidaEccezioni [" + timestamp + "]",
                    error_message = ex.ToString()
                }, matricola);
                response.esito = false;
                response.errore = ex.Message;
                return response;
            }
            Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = matricola,
                operazione = "validaEccezioni..Response [" + Valida + "]",
                provenienza = "ServiceWrapper..validaEccezioni [" + timestamp + "]",
                descrizione_operazione = "Esito:" + response.esito + " Errore:" + response.errore
            }, matricola);

            return response;
    }






        //private static int GetRV ( Dipendente response, int anno )
        //{
        //	int totalVV = 0;
        //	try
        //	{
        //		List<Giornata> giorni = new List<Giornata>();

        //		var vv = response.ferie.giornate.Where( g => g.codiceVisualizzazione.Equals( "VV" ) ).ToList();

        //		if ( vv != null && vv.Any() )
        //		{
        //			vv.ForEach( v =>
        //			{
        //				int gg = int.Parse( v.dataTeorica.Substring( 0, 2 ) );
        //				int mese = int.Parse( v.dataTeorica.Substring( 3, 2 ) );

        //				// calcolo la data in cui è stato assegnato MN
        //				DateTime toCompare = new DateTime( anno, mese, gg );

        //				if ( toCompare.Date <= DateTime.Now.Date )
        //				{
        //					totalVV++;
        //				}
        //			} );
        //		}

        //		return totalVV;
        //	}
        //	catch ( Exception ex )
        //	{
        //		throw new Exception( ex.Message );
        //	}
        //}

        //private static int GetFV ( Dipendente response, int anno )
        //{
        //	int totalFV = 0;
        //	try
        //	{
        //		List<Giornata> giorni = new List<Giornata>();

        //		var vc = response.ferie.giornate.Where( g => g.codiceVisualizzazione.Equals( "VC" ) ).ToList();

        //		if ( vc != null && vc.Any() )
        //		{
        //			vc.ForEach( v =>
        //			{
        //				int gg = int.Parse( v.dataTeorica.Substring( 0, 2 ) );
        //				int mese = int.Parse( v.dataTeorica.Substring( 3, 2 ) );

        //				// calcolo la data in cui è stato assegnato MN
        //				DateTime toCompare = new DateTime( anno, mese, gg );

        //				if ( toCompare.Date <= DateTime.Now.Date )
        //				{
        //					totalFV++;
        //				}
        //			} );
        //		}

        //		return totalFV;
        //	}
        //	catch ( Exception ex )
        //	{
        //		throw new Exception( ex.Message );
        //	}
        //}

        //private static MRResult GetMR ( Dipendente response, int anno )
        //{
        //	MRResult result = new MRResult();
        //	try
        //	{
        //		List<Giornata> giorni = new List<Giornata>();
        //		DateTime t1 = DateTime.MinValue;
        //		DateTime rangeStart = DateTime.MinValue;
        //		int giorniConsecutivi = 0;

        //		response.ferie.giornate.ToList().ForEach( f =>
        //		{
        //			f.orarioReale = f.orarioReale.Trim();

        //			if ( !String.IsNullOrEmpty( f.orarioReale ) )
        //			{
        //				int codice = -1;

        //				int.TryParse( f.orarioReale, out codice );

        //				if ( codice < 90 )
        //				{
        //					int gg = int.Parse( f.dataTeorica.Substring( 0, 2 ) );
        //					int mese = int.Parse( f.dataTeorica.Substring( 3, 2 ) );

        //					// calcolo la data in cui è stato assegnato MN
        //					DateTime toCompare = new DateTime( anno, mese, gg );

        //					if ( toCompare.Date <= DateTime.Now.Date )
        //					{
        //						giorni.Add( f );
        //					}
        //				}
        //			}
        //		} );

        //		if ( giorni != null && giorni.Any() )
        //		{
        //			foreach ( var g in giorni )
        //			{
        //				int gg = int.Parse( g.dataTeorica.Substring( 0, 2 ) );
        //				int mese = int.Parse( g.dataTeorica.Substring( 3, 2 ) );

        //				// calcolo la data in cui è stato assegnato MN
        //				DateTime toCompare = new DateTime( anno, mese, gg );

        //				if ( giorniConsecutivi == 0 )
        //				{
        //					t1 = toCompare;
        //					giorniConsecutivi++;
        //					rangeStart = toCompare;
        //				}
        //				else if ( giorniConsecutivi < 7 )
        //				{
        //					if ( t1.Date == toCompare.AddDays( -1 ).Date )
        //					{
        //						t1 = toCompare;
        //						giorniConsecutivi++;
        //					}
        //					else
        //					{
        //						t1 = toCompare;
        //						giorniConsecutivi = 1;
        //						rangeStart = toCompare;
        //					}
        //				}
        //				else
        //				{
        //					result.ListMR.Add( new MRDetail()
        //					{
        //						Dal = rangeStart,
        //						Al = toCompare,
        //						Scadenza = toCompare.AddDays( 30 ),
        //						Valido = true
        //					} );

        //					rangeStart = DateTime.MinValue;
        //					giorniConsecutivi = 0;
        //				}

        //				if ( giorniConsecutivi == 7 )
        //				{
        //					result.ListMR.Add( new MRDetail()
        //					{
        //						Dal = rangeStart,
        //						Al = toCompare,
        //						Scadenza = toCompare.AddDays( 30 ),
        //						Valido = true
        //					} );

        //					rangeStart = DateTime.MinValue;
        //					giorniConsecutivi = 0;
        //				}
        //			}

        //			if ( result != null && result.ListMR.Any() )
        //			{
        //				// recupero giorni in RR
        //				var rr = response.ferie.giornate.Where( g => g.codiceVisualizzazione.Equals( "RR" ) ).ToList();

        //				if ( rr != null && rr.Any() )
        //				{
        //					result.ListMR.ForEach( mr =>
        //					{
        //						Giornata toRemove = null;
        //						foreach ( var g in rr )
        //						{
        //							int gg = int.Parse( g.dataTeorica.Substring( 0, 2 ) );
        //							int mese = int.Parse( g.dataTeorica.Substring( 3, 2 ) );

        //							// calcolo la data in cui è stato assegnato MN
        //							DateTime toCompare = new DateTime( anno, mese, gg );
        //							if ( toCompare.Date <= mr.Scadenza.Date &&
        //								toCompare.Date > mr.Al.Date )
        //							{
        //								mr.DataRecuperoRiposo = toCompare;
        //								mr.Valido = false;
        //								toRemove = new Giornata();
        //								toRemove = g;
        //								break;	// uscita forzata dal ciclo 
        //										// in quanto ormai un elemento RR è stato trovato
        //							}
        //						}
        //						// se un RR è stato preso per annullare un MR allora lo rimuoviamo dalla lista
        //						// dei RR, così da non riconteggiarlo in un eventuale ciclo successivo.
        //						if ( toRemove != null )
        //						{
        //							rr.Remove( toRemove );
        //						}
        //					} );
        //				}
        //			}				
        //		}
        //	}
        //	catch ( Exception ex )
        //	{
        //		throw new Exception( ex.Message );
        //	}

        //	return result;
        //}

        ///// <summary>
        ///// Reperimento delle info riguardanti gli MN spettanti
        ///// </summary>
        ///// <returns></returns>
        //private static MNResult GetMN ( pianoFerie response, int anno )
        //{
        //	MNResult result = new MNResult();
        //	try
        //	{
        //		if ( response != null )
        //		{
        //			if ( IsGiornalista( response.dipendente.ferie ) )
        //			{
        //				List<Giornata> giorniMN = new List<Giornata>();
        //				List<GiornataEx> giorniRN = new List<GiornataEx>();
        //				List<GiornataEx> giorniRNM = new List<GiornataEx>();
        //				List<GiornataEx> giorniRNP = new List<GiornataEx>();
        //				List<GiornataEx> giorniML = new List<GiornataEx>();
        //				List<GiornataEx> giorniIL = new List<GiornataEx>();

        //				// recupero giorni in MN
        //				giorniMN = response.dipendente.ferie.giornate.Where( g => g.codiceVisualizzazione.Equals( "MN" ) ).ToList();

        //				// recupero giorni in RN
        //				List<Giornata> _giorniRN = response.dipendente.ferie.giornate.Where( g => g.codiceVisualizzazione.Equals( "RN" ) ).ToList();

        //				// recupero giorni in RNM
        //				List<Giornata> _giorniRNM = response.dipendente.ferie.giornate.Where( g => g.codiceVisualizzazione.Equals( "RNM" ) ).ToList();

        //				// recupero giorni in RNP
        //				List<Giornata> _giorniRNP = response.dipendente.ferie.giornate.Where( g => g.codiceVisualizzazione.Equals( "RNP" ) ).ToList();

        //				if ( giorniMN != null && giorniMN.Any() )
        //				{
        //					// recupero giorni in ML
        //					List<Giornata> ml = response.dipendente.ferie.giornate.Where( g => g.codiceVisualizzazione.Equals( "ML" ) ).ToList();

        //					// se ci sono giorni in ML salvo in giorniML la data
        //					if ( ml != null && ml.Any() )
        //					{
        //						giorniML = ConvertToGiornataExList( ml, anno );
        //					}

        //					// recupero giorni in IL
        //					List<Giornata> il = response.dipendente.ferie.giornate.Where( g => g.codiceVisualizzazione.Equals( "IL" ) ).ToList();

        //					// se ci sono giorni in IL salvo in giorniIL la data
        //					if ( il != null && il.Any() )
        //					{
        //						giorniIL = ConvertToGiornataExList( il, anno );
        //					}

        //					foreach ( var g in giorniMN )
        //					{
        //						int addDays = 0;
        //						int gg = int.Parse( g.dataTeorica.Substring( 0, 2 ) );
        //						int mese = int.Parse( g.dataTeorica.Substring( 3, 2 ) );

        //						// calcolo la data in cui è stato assegnato MN
        //						DateTime toCompare = new DateTime( anno, mese, gg );

        //						MNDetail currValue = new MNDetail()
        //						{
        //							Giorno = toCompare
        //						};

        //						// la data di scadenza presunta del MN è calcolata in base alla data di
        //						// assegnazioni più 45 giorni
        //						DateTime finalDate = toCompare.AddDays( 45 );

        //						// verifica se esistono ML nell'intervallo di tempo che va dalla
        //						// data di assegnazioni alla data presunta di scadenza
        //						// e che non siano stati già contati
        //						var existsML = giorniML.Where( m => m.Data <= finalDate &&
        //															m.Data >= toCompare &&
        //															!m.GiaCalcolato ).ToList();

        //						// Per ogni ML trovato, va aggiunto un giorno alla data di scadenza presunta
        //						if ( existsML != null && existsML.Any() )
        //						{
        //							existsML.ForEach( itm =>
        //							{
        //								itm.GiaCalcolato = true;
        //								addDays++;
        //							} );
        //						}

        //						// verifica se esistono IL nell'intervallo di tempo che va dalla
        //						// data di assegnazioni alla data presunta di scadenza
        //						// e che non siano stati già contati
        //						var existsIL = giorniIL.Where( m => m.Data <= finalDate &&
        //															m.Data >= toCompare &&
        //															!m.GiaCalcolato ).ToList();

        //						// Per ogni IL trovato, va aggiunto un giorno alla data di scadenza presunta
        //						if ( existsIL != null && existsIL.Any() )
        //						{
        //							existsIL.ForEach( itm =>
        //							{
        //								itm.GiaCalcolato = true;
        //								addDays++;
        //							} );
        //						}

        //						finalDate = finalDate.AddDays( addDays );
        //						currValue.DataScadenza = finalDate;
        //						//currValue.GiorniDaAggiungere = addDays;

        //						result.ListMN.Add( currValue );
        //					}

        //					if ( result != null && result.ListMN != null && result.ListMN.Any() )
        //					{
        //						// se ci sono giorni RN
        //						if ( _giorniRN != null && _giorniRN.Any() )
        //						{
        //							giorniRN = ConvertToGiornataExList( _giorniRN, anno );
        //						}

        //						// se ci sono giorni RNM
        //						if ( _giorniRNM != null && _giorniRNM.Any() )
        //						{
        //							giorniRNM = ConvertToGiornataExList( _giorniRNM, anno );
        //						}

        //						// se ci sono giorni RNP
        //						if ( _giorniRNP != null && _giorniRNP.Any() )
        //						{
        //							giorniRNP = ConvertToGiornataExList( _giorniRNP, anno );
        //						}

        //						foreach ( var mn in result.ListMN.Where( x => !x.AnnullatoDaRN ).ToList() )
        //						{
        //							double rnParziali = 0.0;
        //							var existsGiorniRN = giorniRN.Where( m => mn.Giorno <= m.Data &&
        //																m.Data <= mn.DataScadenza &&
        //																!m.GiaCalcolato ).ToList();

        //							if ( existsGiorniRN != null && existsGiorniRN.Any() )
        //							{
        //								existsGiorniRN.First().GiaCalcolato = true;
        //								mn.AnnullatoDaRN = true;
        //								mn.DataRN = existsGiorniRN.First().Data;
        //								mn.Spettante = 0;
        //								continue;
        //							}

        //							var existsGiorniRNM = giorniRNM.Where( m => mn.Giorno <= m.Data &&
        //																m.Data <= mn.DataScadenza &&
        //																!m.GiaCalcolato ).ToList();

        //							if ( existsGiorniRNM != null && existsGiorniRNM.Any() )
        //							{
        //								foreach ( var rnm in existsGiorniRNM )
        //								{
        //									rnm.GiaCalcolato = true;
        //									rnParziali += 0.5;
        //									mn.Spettante -= 0.5f;

        //									if ( rnParziali == 1 )
        //									{
        //										mn.AnnullatoDaRN = true;
        //										mn.DataMezzoRN2 = rnm.Data;
        //										mn.Spettante = 0;
        //										continue;
        //									}
        //									else
        //									{
        //										mn.Spettante = 0.5f;
        //										mn.DataMezzoRN1 = rnm.Data;
        //									}
        //								}
        //							}

        //							var existsGiorniRNP = giorniRNP.Where( m => mn.Giorno <= m.Data &&
        //																m.Data <= mn.DataScadenza &&
        //																!m.GiaCalcolato ).ToList();

        //							if ( existsGiorniRNP != null && existsGiorniRNP.Any() )
        //							{
        //								foreach ( var rnp in existsGiorniRNP )
        //								{
        //									rnp.GiaCalcolato = true;
        //									rnParziali += 0.5;
        //									mn.Spettante -= 0.5f;

        //									if ( rnParziali == 1 )
        //									{
        //										mn.AnnullatoDaRN = true;
        //										mn.Spettante = 0;
        //										mn.DataMezzoRN2 = rnp.Data;
        //										continue;
        //									}
        //									else
        //									{
        //										mn.Spettante = 0.5f;
        //										mn.DataMezzoRN1 = rnp.Data;
        //									}
        //								}
        //							}
        //						}
        //					}
        //				}
        //			}
        //		}

        //		if ( result != null && result.ListMN != null && result.ListMN.Any() )
        //		{
        //			var items = result.ListMN.Where( i => !i.AnnullatoDaRN && !i.Scaduto ).ToList();

        //			if ( items != null && items.Any() )
        //			{
        //				items.ForEach( i =>
        //				{
        //					if ( DateTime.Now.Date > i.DataScadenza.Date )
        //					{
        //						i.Scaduto = true;
        //						i.Spettante = 0;
        //					}

        //					//int totalDays = 45 + i.GiorniDaAggiungere;

        //					//int diffDays = ( DateTime.Now - i.DataScadenza ).Days;

        //					//if ( diffDays > totalDays )
        //					//{
        //					//	i.Scaduto = true;
        //					//}
        //				} );
        //			}
        //		}
        //	}
        //	catch ( Exception ex )
        //	{
        //		throw new Exception( ex.Message );
        //	}

        //	return result;
        //}

        //	/// <summary>
        //	/// Reperimento dei PXC spettanti
        //	/// </summary>
        //	/// <returns></returns>
        //	private static PXCResult GetPXC ( pianoFerie response, int anno )
        //	{
        //		PXCResult result = new PXCResult();
        //		try
        //		{
        //			if ( response != null )
        //			{
        //				if ( IsGiornalista( response.dipendente.ferie ) )
        //				{
        //					List<Giornata> giorni = new List<Giornata>();
        //					List<GiornataEx> giorniPXC = new List<GiornataEx>();

        //					// recupero giorni con codice 95 o 96
        //					giorni = response.dipendente.ferie.giornate.Where( g => g.orarioReale.Equals( "95" ) || g.orarioReale.Equals( "96" ) ).ToList();

        //					if ( giorni != null && giorni.Any() )
        //					{
        //						foreach ( var g in giorni )
        //						{
        //							int gg = int.Parse( g.dataTeorica.Substring( 0, 2 ) );
        //							int mese = int.Parse( g.dataTeorica.Substring( 3, 2 ) );

        //							// calcolo la data in cui è stato assegnato MN
        //							DateTime toCompare = new DateTime( anno, mese, gg );

        //							if ( toCompare <= DateTime.Now.Date )
        //							{
        //								if ( FestivitaManager.IsFestivo( toCompare ) ||
        //FestivitaManager.IsSuperFestivo( toCompare ) ||
        //FestivitaManager.IsExFestivo( toCompare ) )
        //								{
        //									result.ListPXC.Add( new PXCDetail()
        //									{
        //										Giorno = toCompare
        //									} );
        //									result.TotalePXC++;
        //								}
        //							}
        //						}
        //					}

        //					if ( result.ListPXC.Any() )
        //					{
        //						int sumPXC = 0;
        //						List<Giornata> pxcDays = new List<Giornata>();
        //						pxcDays = response.dipendente.ferie.giornate.Where( g => g.codiceVisualizzazione.Equals( "PXC" ) ).ToList();

        //						if ( pxcDays != null && pxcDays.Any() )
        //						{
        //							foreach ( var g in pxcDays )
        //							{
        //								int gg = int.Parse( g.dataTeorica.Substring( 0, 2 ) );
        //								int mese = int.Parse( g.dataTeorica.Substring( 3, 2 ) );

        //								// calcolo la data in cui è stato assegnato MN
        //								DateTime toCompare = new DateTime( anno, mese, gg );

        //								if ( toCompare <= DateTime.Now.Date )
        //								{
        //									sumPXC++;										
        //								}
        //							}
        //						}

        //						result.TotalePXC = result.TotalePXC - sumPXC;
        //					}
        //				}
        //			}
        //		}
        //		catch ( Exception ex )
        //		{
        //			throw new Exception( ex.Message );
        //		}

        //		return result;
        //	}

        //	private static bool IsGiornalista ( Ferie fer )
        //	{
        //		bool result = false;
        //		try
        //		{
        //			if ( fer.visualizzaPermessiGiornalisti )
        //			{
        //				result = true;
        //			}
        //		}
        //		catch ( Exception ex )
        //		{
        //		}

        //		return result;
        //	}

        //	/// <summary>
        //	/// Converte una lista di oggetti Giornata in una lista di oggetti GiornataEx.
        //	/// Dall'oggetto viene estrapolata la sola dataTeorica e convertita in formato DateTime
        //	/// </summary>
        //	/// <param name="toConvert">Lista di oggetti Giornata</param>
        //	/// <param name="anno">Anno di riferimento</param>
        //	/// <returns></returns>
        //	private static List<GiornataEx> ConvertToGiornataExList ( List<Giornata> toConvert, int anno )
        //	{
        //		List<GiornataEx> result = new List<GiornataEx>();
        //		try
        //		{
        //			toConvert.ForEach( x =>
        //			{
        //				int gg = int.Parse( x.dataTeorica.Substring( 0, 2 ) );
        //				int mese = int.Parse( x.dataTeorica.Substring( 3, 2 ) );
        //				DateTime nuovaData = new DateTime( anno, mese, gg );

        //				result.Add( new GiornataEx()
        //				{
        //					Data = nuovaData,
        //					GiaCalcolato = false
        //				} );
        //			} );
        //		}
        //		catch ( Exception ex )
        //		{
        //			throw new Exception( ex.Message );
        //		}

        //		return result;
        //	}
    }

    public class EccezioneSplit
    {
        public bool IsTextAndNumeric { get; set; }
        public string TextPart { get; set; }
        public int NumericPart { get; set; }
        public List<string> AssorbeEventualmente { get; set; }
    }

    //public class MNResult
    //{
    //	public MNResult ()
    //	{
    //		this.ListMN = new List<MNDetail>();
    //	}

    //	public List<MNDetail> ListMN { get; set; }
    //}

    //public class MNDetail
    //{
    //	public MNDetail ()
    //	{
    //		this.Spettante = 1;
    //	}

    //	public DateTime Giorno { get; set; }
    //	public bool Scaduto { get; set; }
    //	public DateTime DataScadenza { get; set; }
    //	public bool AnnullatoDaRN { get; set; }
    //	public DateTime? DataRN { get; set; }
    //	public DateTime? DataMezzoRN1 { get; set; }
    //	public DateTime? DataMezzoRN2 { get; set; }
    //	public float Spettante { get; set; }
    //}

    //public class PXCResult
    //{
    //	public PXCResult ()
    //	{
    //		this.TotalePXC = 0;
    //		this.ListPXC = new List<PXCDetail>();
    //	}

    //	public int TotalePXC { get; set; }
    //	public List<PXCDetail> ListPXC { get; set; }
    //}

    //public class PXCDetail
    //{
    //	public DateTime Giorno { get; set; }
    //}

    //public class GiornataEx
    //{
    //	public DateTime Data { get; set; }
    //	public bool GiaCalcolato { get; set; }
    //}

    //public class MRResult
    //{
    //	public MRResult ()
    //	{
    //		this.ListMR = new List<MRDetail>();
    //	}

    //	public List<MRDetail> ListMR { get; set; }
    //}

    //public class MRDetail
    //{
    //	public DateTime Dal { get; set; }
    //	public DateTime Al { get; set; }
    //	public DateTime? DataRecuperoRiposo { get; set; }
    //	public DateTime Scadenza { get; set; }
    //	public bool Valido { get; set; }
    //}




    /// <summary>
    /// Classe per la gestione della sessione del wrapper
    /// </summary>
    public class ServiceWrapperrScope : SessionScope<ServiceWrapperrScope>
    {
        public ServiceWrapperrScope()
        {
        }

        public GetPianoFerieWrapped GetPianoFerieWrapped { get; set; }
    }

    public class GetPianoFerieWrapped
    {
        private string Matricola { get; set; }
        private string Data { get; set; }
        private string TipologiaUtente { get; set; }
        private int LivelloUtente { get; set; }
        private DateTime? DataUltimoAggiornamento { get; set; }
        private pianoFerie Response { get; set; }

        public bool HasData()
        {
            if (this.DataUltimoAggiornamento.HasValue)
            {
                DateTime current = DateTime.Now;

                double sec = (current - this.DataUltimoAggiornamento.Value).TotalSeconds;

                if (sec > 20.0)
                    return false;
                else
                    return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsEqualsTo(string matricola, string datainizio, int livelloutente, string tipologiaUtente)
        {
            if (this.Matricola.Trim().Equals(matricola.Trim()) &&
                this.Data.Trim().Equals(datainizio.Trim()) &&
                this.TipologiaUtente.Trim().Equals(tipologiaUtente.Trim()) &&
                this.LivelloUtente.Equals(livelloutente))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public pianoFerie GetResponse()
        {
             
            return this.Response;
        }

        public void SetResponse(string matricola, string datainizio, int livelloutente, string tipologiaUtente, pianoFerie value)
        {
            this.Matricola = matricola;
            this.Data = datainizio;
            this.TipologiaUtente = tipologiaUtente;
            this.LivelloUtente = livelloutente;
            this.Response = value;
            this.DataUltimoAggiornamento = DateTime.Now;
        }
    }
}