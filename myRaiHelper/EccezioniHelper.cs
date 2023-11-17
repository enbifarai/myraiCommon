using myRaiData;
using MyRaiServiceInterface;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace myRaiHelper
{
    public class EccezioniHelper
    {
        public static bool IsEccezioneInGruppo2_3(string cod)
        {
            var db = new digiGappEntities();
            int? gruppo = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione.Trim() == cod.Trim()).Select(x => x.id_raggruppamento).FirstOrDefault();
            return gruppo == 2 || gruppo == 3;
        }
        public static L2D_SEDE_GAPP GetSedeGappFromL2D(string sede, DateTime data)
        {
            var db = new digiGappEntities();

            var sedegapp = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp.Trim() == sede
                 && x.data_inizio_validita <= data
                 && x.data_fine_validita >= data).FirstOrDefault();

            return sedegapp;
        }
        public static Ferie ConvertiWcfToAsmxFerie(MyRaiServiceInterface.MyRaiServiceReference1.GetFerieResponse wcfFerie)
        {
            var FE = wcfFerie.datiDipendente.Where(x => x.codiceEccezione == "FE").FirstOrDefault();
            var PF = wcfFerie.datiDipendente.Where(x => x.codiceEccezione == "PF").FirstOrDefault();
            var PG = wcfFerie.datiDipendente.Where(x => x.codiceEccezione == "PG").FirstOrDefault();
            var PX = wcfFerie.datiDipendente.Where(x => x.codiceEccezione == "PX").FirstOrDefault();
            var PR = wcfFerie.datiDipendente.Where(x => x.codiceEccezione == "PR").FirstOrDefault();

            Ferie asmxFerie = new Ferie();
            if (FE != null)
            {
                asmxFerie.ferieSpettanti = FE.spettanti;
                asmxFerie.ferieUsufruite = FE.fruiti;
                asmxFerie.ferieRimanenti = FE.residui;
                asmxFerie.feriePianificate = FE.pianificati;
            }
            if (PF != null)
            {
                asmxFerie.exFestivitaSpettanti = PF.spettanti;
                asmxFerie.exFestivitaUsufruite = PF.fruiti;
                asmxFerie.exFestivitaRimanenti = PF.residui;
                asmxFerie.exFestivitaPianificate = PF.pianificati;
            }
            if (PR != null)
            {
                asmxFerie.permessiSpettanti = PR.spettanti;
                asmxFerie.permessiUsufruiti = PR.fruiti;
                asmxFerie.permessiRimanenti = PR.residui;
                asmxFerie.permessiPianificati = PR.pianificati;
            }

            return asmxFerie;
        }
        public static Boolean ConsentitaDaProntuario(Giornata g)
        {
            if (g == null)
                return false;

            String tipoGiorno = "";

            if (g.tipoGiornata == "N")
                tipoGiorno = "FER";
            else if (g.tipoGiornata == "R" || g.tipoGiornata == "S")
                tipoGiorno = "FES";
            else if (g.tipoGiornata == "P")
                tipoGiorno = "SFE";
            else if (g.siglaGiornata == "DO")
                tipoGiorno = "DOM";



            string orarioTabella = g.orarioReale;
            if (new string[] { "90", "91", "92", "95", "96" }.Contains(orarioTabella) == false)
                orarioTabella = "LV";

            var db = new digiGappEntities();
            var item = db.MyRai_Prontuario.Where(x =>
                                                x.Tipo_Dip.Contains(g.tipoDipendente)
                                                && x.Tipo_Giorno.StartsWith(tipoGiorno)
                                                && x.Orario_Previsto.StartsWith(orarioTabella)).FirstOrDefault();

            if (item == null)
                return true;
            else
                return item.Consentito;
        }
        public static EsitoCompilatore QualiDaRimuovere(dayResponse resp, List<MyRai_Eccezioni_Ammesse> ammesse, string matricola, string CodiceEccezioneProvenienteDaTest = null, IEnumerable<string> extraAssemblies=null)
        {
            try
            {

                string snippetsEccezioni = "";
                if (CodiceEccezioneProvenienteDaTest != null)
                    snippetsEccezioni = CodiceEccezioneProvenienteDaTest;
                else
                {
                    string[] s = ammesse.Select(x => x.CodiceCsharp).ToArray();
                    for (int i = 0; i < s.Length; i++)
                    {
                        if (s[i] != null)
                            s[i] = s[i].Split(new string[] { "****" }, StringSplitOptions.None)[0];
                    }

                    snippetsEccezioni = String.Join("      ", s);
                }

                if (!String.IsNullOrWhiteSpace(snippetsEccezioni))
                {

                    string[] CodiceCompilatore = CommonHelper.GetParametri<string>(EnumParametriSistema.CodiceCSharp);
                    string codicec = CodiceCompilatore[0]  //codice in chiaro
                                         .Replace("////", snippetsEccezioni);

                    codicec = "using myRaiCommonManager;\r\n" + codicec;

                    dynamic oggetto = Macinatore.Compila(
                                      codicec, // codice delle singole eccezioni
                                      "myRai.Business.Filtro",
                                      CodiceCompilatore[1], //assembly necessari separati da virgola
                                      extraAssemblies
                                      );

                    if (!(oggetto is System.CodeDom.Compiler.CompilerErrorCollection))
                    {
                        //esegue il codice compilato
                        List<string> Lecce = ammesse.Select(x => x.cod_eccezione).ToList();


                        //debug macinatore :
                        var f = false;
                        if (f)
                        {
                            //TODO Nik
                            //FiltroTest fi = new FiltroTest();
                            //var testDaRimuovere = fi.EccezioniDaRimuovere(resp, Lecce);
                        }



                        List<string> DaRimuovere = oggetto.EccezioniDaRimuovere(resp, Lecce);
                        return new EsitoCompilatore() { EccezioniDaRimuovere = DaRimuovere, Errore = null };
                    }
                    else
                    {
                        System.CodeDom.Compiler.CompilerErrorCollection errori = (System.CodeDom.Compiler.CompilerErrorCollection)oggetto;
                        string erroritext = "";
                        for (int i = 0; i < errori.Count; i++)
                            erroritext += errori[i].ErrorText + " - ";
                        Logger.LogErrori(new MyRai_LogErrori()
                        {
                            applicativo = "PORTALE",
                            data = DateTime.Now,
                            matricola = CommonHelper.GetCurrentUserMatricola(),
                            provenienza = "Macinatore",
                            error_message = erroritext
                        });
                        myRaiCommonTasks.CommonTasks.PostMessage("Errore Macinatore : " + erroritext);
                        return new EsitoCompilatore() { Errore = erroritext };
                    }
                }
                else
                    return new EsitoCompilatore() { EccezioniDaRimuovere = null, Errore = null };
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "EccezioniManager.GetListaEccezioniPossibili CompilatoreRunTime",

                });
                return new EsitoCompilatore() { EccezioniDaRimuovere = null, Errore = ex.Message + ex.InnerException };
            }
        }
        public static List<string> RimuoviEccezioniPerLimiteGiornaliero(dayResponse giornata)
        {
            List<string> listaEccDaRimuovere = new List<string>();
            if (giornata == null || giornata.eccezioni == null)
                return listaEccDaRimuovere;

            string matr = CommonHelper.GetCurrentUserMatricola();
            var db = new digiGappEntities();

            //TOGLI quelle che sono gia su db per la giornata:
            //    listaEccDaRimuovere = TimbratureCore.TimbratureManager.RimuoviPerchePresentiSuDB(giornata.giornata.data, matr);

            //TOGLI quelle che sono gia nel GAPP per la giornata:
            List<string> L = TimbratureCore.TimbratureManager.RimuoviPerchePresentiSuGAPP(giornata);
            if (L != null && L.Count() > 0)
                listaEccDaRimuovere.AddRange(L);

            return listaEccDaRimuovere;
        }
        public static List<MyRai_Eccezioni_Ammesse> IniettaEccezioni(List<MyRai_Eccezioni_Ammesse> ammesse,
    List<string> codiciEccDB, DateTime Date, List<string> rimuoviPerLimiteG)
        {

            var db = new digiGappEntities();
            WSDigigapp service = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(
                  CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                  CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            };
            pianoFerie response = ServiceWrapper.GetPianoFerieWrapped(service, CommonHelper.GetCurrentUserMatricola(),
                        "0101" +

                         (Date.Year == DateTime.Now.Year ? DateTime.Now.Year.ToString() : Date.Year.ToString())

                        , 75, UtenteHelper.TipoDipendente());
            Ferie F = response.dipendente.ferie;

            var list = new[]
                {   new { codecc = "RR",visualizza=F.visualizzaRecuperoRiposi, rimanenti = F.recuperiMancatiRiposiRimanenti },
                    new { codecc = "RN",visualizza=F.visualizzaRecuperoNonLavorati, rimanenti = F.recuperiNonLavoratiRimanenti },
                    new { codecc = "RF",visualizza=F.visualizzaRecuperoFestivi, rimanenti = F.recuperiMancatiFestiviRimanenti },
                    new { codecc = "PX",visualizza=F.visualizzaPermessiGiornalisti, rimanenti = F.permessiGiornalistiRimanenti }
                }.ToList();

            foreach (var item in list)
            {
                if (!ammesse.Any(x => x.cod_eccezione == item.codecc) ||
                    !ammesse.Any(x => x.cod_eccezione == item.codecc + "M") ||
                    !ammesse.Any(x => x.cod_eccezione == item.codecc + "P"))
                {
                    if (item.visualizza)
                    {
                        if (item.rimanenti >= 0.5)
                        {
                            if (!ammesse.Any(x => x.cod_eccezione == item.codecc + "M"))
                            {
                                var ecc_m = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == item.codecc + "M").FirstOrDefault();
                                if (ecc_m != null)
                                    ammesse.Add(ecc_m);
                            }
                            if (!ammesse.Any(x => x.cod_eccezione == item.codecc + "P"))
                            {
                                var ecc_p = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == item.codecc + "P").FirstOrDefault();
                                if (ecc_p != null)
                                    ammesse.Add(ecc_p);
                            }
                        }
                        if (item.rimanenti >= 1)
                        {
                            if (!ammesse.Any(x => x.cod_eccezione == item.codecc))
                            {
                                var ecc = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == item.codecc).FirstOrDefault();
                                if (ecc != null)
                                    ammesse.Add(ecc);
                            }
                        }
                    }
                }
            }

            //if (!ammesse.Any(x => x.cod_eccezione == "RR") ||
            //    !ammesse.Any(x => x.cod_eccezione == "RRM") ||
            //    !ammesse.Any(x => x.cod_eccezione == "RRP"))
            //{
            //    response = ServiceWrapper.GetPianoFerieWrapped(service, CommonHelper.GetCurrentUserMatricola(),
            //            "0101" + DateTime.Now.Year.ToString(), 75);
            //    if (response.dipendente.ferie.visualizzaRecuperoRiposi == true)
            //    {
            //        if (response.dipendente.ferie.recuperiMancatiRiposiRimanenti >= 0.5)
            //        {
            //            if (!ammesse.Any(x => x.cod_eccezione == "RRM"))
            //            {
            //                var rrm = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "RRM").FirstOrDefault();
            //                if (rrm != null) ammesse.Add(rrm);
            //            }
            //            if (!ammesse.Any(x => x.cod_eccezione == "RRP"))
            //            {
            //                var rrp = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "RRP").FirstOrDefault();
            //                if (rrp != null) ammesse.Add(rrp);
            //            }
            //        }
            //        if (response.dipendente.ferie.recuperiMancatiRiposiRimanenti >= 1)
            //        {
            //            if (!ammesse.Any(x => x.cod_eccezione == "RR"))
            //            {
            //                var rr = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "RR").FirstOrDefault();
            //                if (rr != null) ammesse.Add(rr);
            //            }
            //        }
            //    }
            //}
            //if (!ammesse.Any(x => x.cod_eccezione == "RN") ||
            //    !ammesse.Any(x => x.cod_eccezione == "RNM") ||
            //    !ammesse.Any(x => x.cod_eccezione == "RNP"))
            //{
            //    if (response == null)
            //    {
            //        response = ServiceWrapper.GetPianoFerieWrapped(service, CommonHelper.GetCurrentUserMatricola(),
            //               "0101" + DateTime.Now.Year.ToString(), 75);
            //    }
            //    if (response.dipendente.ferie.visualizzaRecuperoNonLavorati == true)
            //    {
            //        if (response.dipendente.ferie.recuperiNonLavoratiRimanenti >= 0.5)
            //        {
            //            if (!ammesse.Any(x => x.cod_eccezione == "RNM"))
            //            {
            //                var rnm = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "RNM").FirstOrDefault();
            //                if (rnm != null) ammesse.Add(rnm);
            //            }
            //            if (!ammesse.Any(x => x.cod_eccezione == "RNP"))
            //            {
            //                var rnp = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "RNP").FirstOrDefault();
            //                if (rnp != null) ammesse.Add(rnp);
            //            }
            //        }
            //        if (response.dipendente.ferie.recuperiNonLavoratiRimanenti >= 1)
            //        {
            //            if (!ammesse.Any(x => x.cod_eccezione == "RN"))
            //            {
            //                var rn = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "RN").FirstOrDefault();
            //                if (rn != null) ammesse.Add(rn);
            //            }
            //        }
            //    }
            //}

            //if (!ammesse.Any(x => x.cod_eccezione == "RF") ||
            //    !ammesse.Any(x => x.cod_eccezione == "RFM") ||
            //    !ammesse.Any(x => x.cod_eccezione == "RFP"))
            //{
            //    if (response == null)
            //    {
            //        response = ServiceWrapper.GetPianoFerieWrapped(service, CommonHelper.GetCurrentUserMatricola(),
            //               "0101" + DateTime.Now.Year.ToString(), 75);
            //    }
            //    if (response.dipendente.ferie.visualizzaRecuperoFestivi == true)
            //    {
            //        if (response.dipendente.ferie.recuperiMancatiFestiviRimanenti >= 0.5)
            //        {
            //            if (!ammesse.Any(x => x.cod_eccezione == "RFM"))
            //            {
            //                var rfm = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "RFM").FirstOrDefault();
            //                if (rfm != null) ammesse.Add(rfm);
            //            }
            //            if (!ammesse.Any(x => x.cod_eccezione == "RFP"))
            //            {
            //                var rfp = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "RFP").FirstOrDefault();
            //                if (rfp != null) ammesse.Add(rfp);
            //            }
            //        }
            //        if (response.dipendente.ferie.recuperiMancatiFestiviRimanenti >= 1)
            //        {
            //            if (!ammesse.Any(x => x.cod_eccezione == "RF"))
            //            {
            //                var rf = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "RF").FirstOrDefault();
            //                if (rf != null) ammesse.Add(rf);
            //            }
            //        }
            //    }
            //}

            //if (!ammesse.Any(x => x.cod_eccezione == "PX") ||
            //    !ammesse.Any(x => x.cod_eccezione == "PXM") ||
            //    !ammesse.Any(x => x.cod_eccezione == "PXP"))
            //{
            //    if (response == null)
            //    {
            //        response = ServiceWrapper.GetPianoFerieWrapped(service, CommonHelper.GetCurrentUserMatricola(),
            //               "0101" + DateTime.Now.Year.ToString(), 75);
            //    }
            //    if (response.dipendente.ferie.visualizzaPermessiGiornalisti == true)
            //    {
            //        if (response.dipendente.ferie.permessiGiornalistiRimanenti >= 0.5)
            //        {
            //            if (!ammesse.Any(x => x.cod_eccezione == "PXM"))
            //            {
            //                var pxm = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "PXM").FirstOrDefault();
            //                if (pxm != null) ammesse.Add(pxm);
            //            }
            //            if (!ammesse.Any(x => x.cod_eccezione == "PXP"))
            //            {
            //                var pxp = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "PXP").FirstOrDefault();
            //                if (pxp != null) ammesse.Add(pxp);
            //            }
            //        }
            //        if (response.dipendente.ferie.permessiGiornalistiRimanenti >= 1)
            //        {
            //            if (!ammesse.Any(x => x.cod_eccezione == "PX"))
            //            {
            //                var px = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "PX").FirstOrDefault();
            //                if (px != null) ammesse.Add(px);
            //            }
            //        }
            //    }
            //}
            string eccIn = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniIniettate);
            if (!String.IsNullOrWhiteSpace(eccIn))
            {
                string[] eccArray = eccIn.Split(',');
                foreach (string e in eccArray)
                {
                    if (rimuoviPerLimiteG != null && rimuoviPerLimiteG.Any() && rimuoviPerLimiteG.Contains(e))
                        continue;

                    if (!ammesse.Any(x => x.cod_eccezione == e))
                    {
                        var dbe = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == e).FirstOrDefault();
                        if (dbe != null && codiciEccDB.Contains(e))
                            ammesse.Add(dbe);
                    }
                }
            }
            return ammesse;
        }
        public static List<MyRai_Eccezioni_Ammesse> GetListaEccezioniPossibili(string matricola, DateTime data, dayResponse d = null, IEnumerable<string> extraAssemblies=null)
        {
            List<MyRai_Eccezioni_Ammesse> L = SessionHelper.GetEccezioniPossibili(matricola, data);
            if (L != null)
                return L;

            var db = new digiGappEntities();
            var EccezioniAmmesse = db.MyRai_Eccezioni_Ammesse
                .Where(x => x.esclusa_da_eccezione != null && x.esclusa_da_eccezione != "")
                .ToList();

            WSDigigappDataController service = new WSDigigappDataController();

            dayResponse resp = new dayResponse();
            if (d != null)
                resp = d;
            else
                resp = service.GetEccezioni(CommonHelper.GetCurrentUserMatricola(), matricola, data.ToString("ddMMyyyy"), "BU", 70);
            string[] EccezioniOdierne = null;
            if (resp.eccezioni != null && resp.eccezioni.Any())
                EccezioniOdierne = resp.eccezioni.Where(x => x.cod != null).Select(x => x.cod.Trim()).ToArray();


            Boolean OKdaProntuario = ConsentitaDaProntuario(resp.giornata);

            List<DateTime> LD = UtenteHelper.GiornateAssenteIngiustificato(matricola, true);
            Boolean HaAssIng = (LD != null && LD.Contains(data));

            Eccezione[] Eccezioni;

            try
            {
                string area = resp.data.Substring(142, 62);
                Eccezioni = service.ListaEccezioni(CommonHelper.GetCurrentUserMatricola(), matricola, data.ToString("ddMMyyyy"),
                      UtenteHelper.TipoDipendente() +
                        area,
                      // "BU",
                      CommonHelper.GetParametro<int>(EnumParametriSistema.LivelloUtenteListaEccezioni)
                      );
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = matricola,
                    provenienza = "Eccezionimanager.GetListaEccezioniPossibili",
                    error_message = ex.ToString()

                });
                return new List<MyRai_Eccezioni_Ammesse>();
            }
            List<EccezioneOrdinamento> Ordinamento = Eccezioni.Select(x => new EccezioneOrdinamento()
            {
                codice = x.cod.Trim(),
                ordinamento = x.flg_ordinamento == "A" ? 10 : Convert.ToInt32(x.flg_ordinamento)
            })
            .OrderBy(x => x.ordinamento)
            .ThenBy(x => x.codice)
            .ToList();



            List<string> Rimuovi = new List<string>();
            Rimuovi = RimuoviEccezioniPerLimiteGiornaliero(resp);
            if (Rimuovi != null && Rimuovi.Count > 0)
            {
                List<Eccezione> Ltemp = Eccezioni.ToList();
                Ltemp.RemoveAll(a => a.cod != null && Rimuovi.Contains(a.cod.Trim().ToUpper()));
                Eccezioni = Ltemp.ToArray();
            }


            HashSet<string> CodiciEccezioniServizio = new HashSet<string>(
                Eccezioni.Select(x => x.cod.Trim().ToUpper()).ToList()
                 );

            string tdip = UtenteHelper.TipoDipendente();
            string tgio = resp.giornata.tipoGiornata;
            string orarioPr = resp.giornata.orarioTeorico;
            string orarioEff = resp.giornata.orarioReale;
            DateTime Dnow = data;
            bool futuro = data > DateTime.Now;

            HashSet<string> CodiciEccezioniDB = new HashSet<string>(
                db.MyRai_Eccezioni_Ammesse
                .Where(x =>
                    x.flag_attivo == true
                    &&
                    (futuro == false || x.OreInFuturo == 999999)
                    //     &&
                    //     (x.esclusa_da_eccezione== null || x.esclusa_da_eccezione=="" || EccezioniOdierne==null || ! EccezioniOdierne.Any (z=>x.esclusa_da_eccezione.Contains(z)) ) 
                    &&
                    (x.data_inizio_validita == null || x.data_inizio_validita <= Dnow)
                    &&
                    (x.data_fine_validita == null || x.data_fine_validita >= Dnow)
                    &&
                    (x.TipiDipendente == null || x.TipiDipendente.Contains(tdip))
                    &&
                    (x.TipoGiornata == null || x.TipoGiornata.Contains(tgio))
                    &&
                    (x.OrarioPrevisto == null || x.OrarioPrevisto.Contains(orarioPr))
                    &&
                     (x.OrarioEffettivo == null || x.OrarioEffettivo.Contains(orarioEff))
                    &&
                    (x.TipiDipendenteEsclusi == null || !x.TipiDipendenteEsclusi.Contains(tdip))
                    &&
                    (HaAssIng == false || x.StatoGiornata == "ASSING")  //se ha ass.ing. prendi solo quelle marcate ASSING nel db
                    &&
                    (x.Prontuario == false || (x.Prontuario == true && OKdaProntuario == true))// controlla quelle da Prontuario
                    )
                .Select(x => x.cod_eccezione.Trim().ToUpper()).ToList()
                );

            CodiciEccezioniDB = RimuoviSeEscluseDaDB(EccezioniAmmesse, EccezioniOdierne, CodiciEccezioniDB);
            bool rp = CodiciEccezioniDB.Contains("RPAF");

            List<string> Codici = CodiciEccezioniDB.Intersect(CodiciEccezioniServizio).ToList();

            List<MyRai_Eccezioni_Ammesse> ammesse = db.MyRai_Eccezioni_Ammesse.Where(x =>
                Codici.Contains(x.cod_eccezione.Trim().ToUpper())
                ).ToList();

            try
            {
                ammesse = IniettaEccezioni(ammesse, CodiciEccezioniDB.ToList<string>(), data, Rimuovi);
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "IniettaEccezioni"
                });
            }
            ammesse.RemoveAll(x => "RRM,RRP,RNM,RNP,RFM,RFP".Split(',').Contains(x.cod_eccezione));

            var darim = RimuoviFittizieGiaInDB(ammesse, data);
            if (darim.Any())
                ammesse.RemoveAll(x => darim.Contains(x.cod_eccezione));


            bool IsSuPianoFerie = data > DateTime.Now &&
                db.MyRai_PianoFerieGiorni.Any(x => x.matricola == matricola && x.data == data);
            if (IsSuPianoFerie)
            {
                string[] RemovePercheSuPF =
                    db.L2D_ECCEZIONE.Where(x => x.flag_eccez == "C").Select(x => x.cod_eccezione.Trim()).ToArray();

                ammesse.RemoveAll(x => RemovePercheSuPF.Contains(x.cod_eccezione));
            }


            //if ( EccezioniQsuGapp( resp ) )
            //{
            //    ammesse.RemoveAll( x => x.cod_eccezione.Trim( ).EndsWith( "Q" ) );
            //}


            if (ammesse.Count > 0)
            {
                EsitoCompilatore esito = QualiDaRimuovere(resp, ammesse, matricola, extraAssemblies:extraAssemblies);
                List<string> DaRimuovere = esito.EccezioniDaRimuovere;

                if (UtenteHelper.GiornateAssenteIngiustificato(matricola).Count() > 0 && DaRimuovere != null && DaRimuovere.Contains("FE"))
                {
                    MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                    cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
                    MyRaiServiceInterface.MyRaiServiceReference1.GetFerieResponse ResponseWcf = cl.GetFerie(CommonHelper.GetCurrentUserMatricola(), "");
                    var F = ConvertiWcfToAsmxFerie(ResponseWcf);
                    if (F.permessiRimanenti < 1 && F.exFestivitaRimanenti < 1)
                        DaRimuovere.RemoveAll(ecc => ecc == "FE");

                }
                if (DaRimuovere != null && DaRimuovere.Count > 0)
                {
                    ammesse.RemoveAll(x => DaRimuovere.Contains(x.cod_eccezione));
                }

                if (UtenteHelper.GestitoSirio() && SessionHelper.Get("AttivitaCeitonError") != null)
                {
                    ammesse.RemoveAll(x => x.id_raggruppamento == 2 || x.id_raggruppamento == 3);
                }
            }

            //var rohf = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "ROHF").FirstOrDefault();
            // if (rohf != null) ammesse.Add(rohf);

            //se sei in debito POH/ROH togli gli straordinari
            int poh = 0;
            int roh = 0;
            UtenteHelper.GetPOHandROH(false, data, out poh, out roh);

            bool TipoTinMNS = TimbratureCore.TimbratureManager.IsTipoTinMNS(UtenteHelper.TipoDipendente(), resp);


            if (UtenteHelper.GetQuadratura() == Quadratura.Giornaliera && poh > roh)
            {

                var rohf = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "ROHF").FirstOrDefault();
                if (rohf != null) ammesse.Add(rohf);

                string Soppresse = CommonHelper.GetParametro<String>(EnumParametriSistema.SoppressePOH).Trim().ToUpper();


                string[] SoppressePOH = Soppresse.Split(',');
                if (TipoTinMNS) SoppressePOH = SoppressePOH.Where(x => x != "STRF").ToArray();

                ammesse.RemoveAll(x => SoppressePOH.Contains(x.cod_eccezione.Trim().ToUpper()));

                if (!TipoTinMNS && ammesse.Where(x => x.cod_eccezione == "ROH").Count() == 0)
                {
                    ammesse.Add(db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == "ROH").First());
                }

            }

            //se la giornata ha carenza, togli gli straordinari
            if (UtenteHelper.GetQuadratura() == Quadratura.Giornaliera)
            {
                if (AnalisiEccezioni.GiornataHaCarenza(CommonHelper.GetCurrentUserMatricola(), resp) > 0)
                {
                    string[] SoppressePOH = CommonHelper.GetParametro<String>(EnumParametriSistema.SoppressePOH).Trim().ToUpper().Split(',');

                    if (TipoTinMNS) SoppressePOH = SoppressePOH.Where(x => x != "STRF").ToArray();
                    ammesse.RemoveAll(x => SoppressePOH.Contains(x.cod_eccezione.Trim().ToUpper()));
                }
            }

            if (TipoTinMNS)
            {
                ammesse.RemoveAll(ecc => ecc.cod_eccezione == "ROH" ||
                //db.L2D_ECCEZIONE.Where(x => x.cod_aggregato == "03").Select(x => x.cod_eccezione.Trim())
                CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniDaRimuovereTinMNS).Split(',').Contains(ecc.cod_eccezione.Trim()));
            }


            List<string> EccezioniNoQuantita = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniNoQuantita).Split(',').ToList();


            //recupera controlli specifici per eccezione:
            foreach (var amm in ammesse)
            {

                Eccezione eccezioneServ = null;
                if (amm.cod_eccezione != "ROHF")
                    eccezioneServ = Eccezioni.Where(x => x.cod.Trim().ToUpper() == amm.cod_eccezione.Trim().ToUpper()).FirstOrDefault();
                else
                    eccezioneServ = Eccezioni.Where(x => x.cod.Trim().ToUpper() == "ROH").FirstOrDefault();

                string esq = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniDigitaSoloQuantita);
                List<string> EccezioniDigitaSoloQuantita = new List<string>();
                if (!String.IsNullOrWhiteSpace(esq))
                    EccezioniDigitaSoloQuantita = esq.Split(',').ToList();

                List<string> ControlliDaVisualizzare = new List<string>();
                if (amm.cod_eccezione == "URH")
                {
                    ControlliDaVisualizzare.Add("[dalle]");
                    ControlliDaVisualizzare.Add("[alle]");
                    ControlliDaVisualizzare.Add("[quantita]");
                    amm.controlli_specifici = String.Join(",", ControlliDaVisualizzare.ToArray());
                }
                if (amm.cod_eccezione == "ROH" && eccezioneServ == null)
                {
                    ControlliDaVisualizzare.Add("[dalle]");
                    ControlliDaVisualizzare.Add("[alle]");
                    ControlliDaVisualizzare.Add("[quantita]");
                    amm.controlli_specifici = String.Join(",", ControlliDaVisualizzare.ToArray());
                }

                if (eccezioneServ != null)
                {
                    if (amm.cod_eccezione == "RKMF")
                    {

                    }
                    if (
                        (eccezioneServ.tipEcc.Trim() == "0" || eccezioneServ.tipEcc.Trim() == "7" || eccezioneServ.tipEcc.Trim() == "8"
                        || eccezioneServ.flg_macroassenza.Trim() == "Q")
                        ||
                        (eccezioneServ.tipEcc.Trim() == "A" && eccezioneServ.unita_mis.Trim() == "H")
                        )
                    {
                        ControlliDaVisualizzare.Add("[dalle]");
                    }


                    if (eccezioneServ.tipEcc.Trim() == "0" || eccezioneServ.tipEcc.Trim() == "7" || eccezioneServ.tipEcc.Trim() == "8"
                        || ((eccezioneServ.tipEcc.Trim() == "A") && (eccezioneServ.unita_mis.Trim() == "H"))
                   )
                    {
                        ControlliDaVisualizzare.Add("[alle]");
                    }


                    if (eccezioneServ.tipEcc.Trim() == "0" || eccezioneServ.tipEcc.Trim() == "A")
                    {
                        if (!EccezioniNoQuantita.Contains(eccezioneServ.cod.Trim().ToUpper()))
                        {
                            ControlliDaVisualizzare.Add("[quantita]");
                        }
                    }
                    if (eccezioneServ.flg_importo != " " && eccezioneServ.cod.Trim() != "RKMF" && eccezioneServ.cod.Trim() != "RVIV")
                        ControlliDaVisualizzare.Add("[importo]");

                    if (eccezioneServ.flg_addebito == "S")
                    {
                        ControlliDaVisualizzare.Add("[uorg]");
                        ControlliDaVisualizzare.Add("[df]");
                        ControlliDaVisualizzare.Add("[matrspett]");
                    }

                    if (eccezioneServ.cod.Trim().ToUpper() == "PGQ")
                        ControlliDaVisualizzare.Add("[dalle]");
                    else
                    {
                        if (
                            (IsEccezioneAQuarti(eccezioneServ.cod) && !ControlliDaVisualizzare.Contains("[dalle]"))
                            )
                            ControlliDaVisualizzare.Add("[dalle]");
                    }

                    if (eccezioneServ.cod.Trim().ToUpper() == "SEH")
                    {
                        ControlliDaVisualizzare.RemoveAll(x => x == "[dalle]");
                        ControlliDaVisualizzare.RemoveAll(x => x == "[alle]");
                    }
                    if (eccezioneServ.cod.Trim() == "RMTR" || eccezioneServ.cod.Trim() == "RPAF")
                        ControlliDaVisualizzare = new List<string>();


                    if (EccezioniDigitaSoloQuantita.Contains(eccezioneServ.cod))
                    {
                        ControlliDaVisualizzare.RemoveAll(x => x == "[dalle]");
                        ControlliDaVisualizzare.RemoveAll(x => x == "[alle]");
                    }

                    amm.controlli_specifici = String.Join(",", ControlliDaVisualizzare.ToArray());

                }
                //    else
                //       amm.controlli_specifici = String.Join(",", ControlliDaVisualizzare.ToArray());
                amm.importo_preimpostato = UtenteHelper.SedePuoRichiedere(amm.cod_eccezione);
            }
            //string res="";
            //foreach (var a in ammesse)
            //{ 
            //    res+=a.cod_eccezione.PadRight(6,' ') + " : "+ a.controlli_specifici+"\r\n";
            //}
            //  ammesse.RemoveAll(a => a.Prontuario == true && ProntuarioManager.AnalisiEccezioneConsentita(resp, a.cod_eccezione).esito == false);

            SessionHelper.SetEccezioniPossibili(ammesse, matricola, data);
            ammesse = ammesse.OrderBy(x => Ordinamento.IndexOf(Ordinamento.FirstOrDefault(a => a.codice == x.cod_eccezione))).ToList();

            if (UtenteHelper.TipoDipendente() == "G")
            {
                foreach (var a in ammesse)
                {
                    if (a.cod_eccezione.StartsWith("SE")) a.CaratteriMotivoRichiesta = 0;
                }
            }

            if (resp.eccezioni.Any(x => x.cod != null && (x.cod.StartsWith("SE") || x.cod.StartsWith("FS"))))
            {
                string eccobb = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniMotivoObbligatorioSeServizioEsterno);
                string descr = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniMotivoObbligatorioDescrizione);

                if (!string.IsNullOrWhiteSpace(eccobb))
                {
                    String[] EccezioniMotivoObbligatorioSeServizioEsterno = eccobb.Split(',');
                    foreach (var a in ammesse.Where(x => EccezioniMotivoObbligatorioSeServizioEsterno
                                                       .Contains(x.cod_eccezione)))
                    {
                        a.CaratteriMotivoRichiesta = 5;
                        if (!string.IsNullOrWhiteSpace(descr))
                        {
                            a.descrizione_eccezione = descr;
                        }
                    }
                }
            }

            if (
                //Utente.TipoDipendente() != "G" && Utente.TipoDipendente() != "D" && 
                data > DateTime.Now && CommonHelper.IsDateInControlloSNM_SMP(data) && !CommonHelper.SNM_SNPpresent(resp))
            {
                ammesse.RemoveAll(x => x.id_raggruppamento != 1);
                ammesse.RemoveAll(x => new string[] { "PRM", "PRP", "PFM", "PFP" }.Contains(x.cod_eccezione.Trim()));
                //ammesse.Clear();
            }


            return ammesse;

        }
        public static bool EccezioniQsuGapp(dayResponse resp)
        {
            if (resp == null || resp.eccezioni == null || !resp.eccezioni.Any())
                return false;
            else
                return resp.eccezioni.Any(x => x.cod != null && x.cod.Trim().EndsWith("Q"));
        }
        public static HashSet<string> RimuoviSeEscluseDaDB(List<MyRai_Eccezioni_Ammesse> EccezioniAmmesse, string[] EccezioniOdierne, HashSet<string> CodiciEccezioniDB)
        {
            if (EccezioniOdierne != null && EccezioniOdierne.Any() && EccezioniAmmesse != null && EccezioniAmmesse.Any())
            {
                var joined = CodiciEccezioniDB
                                .Join(EccezioniAmmesse.Where(x => EccezioniOdierne.Any(z => x.esclusa_da_eccezione.Split(',').Contains(z))),
                                        ce => ce,
                                        ea => ea.cod_eccezione,
                                        (ce, ea) => new { codecc = ce, eccamm = ea }
                                     ).ToList();

                foreach (var e in joined) CodiciEccezioniDB.Remove(e.codecc);
            }
            return CodiciEccezioniDB;
        }

        public static List<string> RimuoviFittizieGiaInDB(List<MyRai_Eccezioni_Ammesse> ammesse, DateTime data)
        {
            List<string> ToRemove = new List<string>();

            var db = new digiGappEntities();
            var nogapp = ammesse.Where(x => x.no_corrispondenza_gapp).ToList();
            string matr = CommonHelper.GetCurrentUserMatricola();
            var rich = db.MyRai_Richieste.Where(x => x.periodo_dal <= data && x.periodo_al >= data && x.matricola_richiesta == matr).ToList();
            foreach (var e in nogapp)
            {
                if (rich.Any(z => (z.id_stato == 10 || z.id_stato == 20)
                   && z.MyRai_Eccezioni_Richieste.Any(w => w.cod_eccezione.Trim() == e.cod_eccezione.Trim())))
                    ToRemove.Add(e.cod_eccezione);
            }
            return ToRemove;
        }
        public static Boolean IsEccezioneAQuarti(string codice)
        {
            var db = new digiGappEntities();
            var ecc = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione.Trim().ToUpper() == codice.Trim().ToUpper()).FirstOrDefault();
            return (ecc != null && ecc.flag_macroassen == "Q");

            //return (codice != null && codice.ToUpper().Trim().StartsWith("P") && codice.ToUpper().Trim().EndsWith("Q") && codice.ToUpper().Trim ()!="PGQ");
        }



        public static string GetIntervalloRMTRinsediamento(dayResponse R, string Orario_Importo = "O")
        {
            var db = new digiGappEntities();
            string sedeUtente = UtenteHelper.SedeGapp();




            if (R.eccezioni != null && R.eccezioni.Any(x => x.cod != null && x.cod.Trim().ToUpper() == "SE"))
            {

                string SediContabiliPerRMTRconSE = CommonHelper.GetParametro<string>(EnumParametriSistema.SediContabiliRMTR_SE);
                if (!String.IsNullOrWhiteSpace(SediContabiliPerRMTRconSE))
                {
                    if (SediContabiliPerRMTRconSE.Split(',').Contains(UtenteHelper.EsponiAnagrafica().SedeContabile))
                    {
                        if (Orario_Importo == "O")
                            return CommonHelper.GetParametro<string>(EnumParametriSistema.IntervalloRMTR_SE);//2330/0600
                        else
                        {
                            string s = UtenteHelper.SedeGapp();
                            var sede = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == s).FirstOrDefault();
                            return sede.importo_rimborso;
                        }
                    }
                }
            }


            if (R.timbrature != null &&
                R.timbrature.Count() > 0 &&
                R.timbrature.First().entrata != null &&
                R.timbrature.Last().uscita != null &&
                !String.IsNullOrWhiteSpace(R.timbrature.First().entrata.orario) &&
                !String.IsNullOrWhiteSpace(R.timbrature.Last().uscita.orario))
            {
                var Insediamenti = db.L2D_INSEDIAMENTO.ToList();


                var PrimaTimb = R.timbrature.First();
                var UltimaTimbr = R.timbrature.Last();

                string insFitt = CommonHelper.GetParametro<string>(EnumParametriSistema.InsediamentiFittizi);
                if (!String.IsNullOrWhiteSpace(insFitt))
                {
                    if (insFitt.Split(',').Contains(UltimaTimbr.uscita.insediamento))
                        UltimaTimbr.uscita.insediamento = UltimaTimbr.entrata.insediamento;

                    if (insFitt.Split(',').Contains(PrimaTimb.entrata.insediamento))
                        PrimaTimb.entrata.insediamento = PrimaTimb.uscita.insediamento;
                }
                // 2330/0600 se ha SE

                int dayW = (int)R.giornata.data.DayOfWeek;
                if (dayW == 0) dayW = 7;

                if (FestivitaHelper.IsFestivo(R.giornata.data, sedeUtente))
                {
                    var ins = Insediamenti.Where(x => int.TryParse(x.cod_insediamento, out int codi) &&
                     Convert.ToInt32(PrimaTimb.entrata.insediamento) == Convert.ToInt32(x.cod_insediamento)).FirstOrDefault();

                    if (ins != null && (ins.Provincia == "RC" || ins.Provincia == "CS"))
                    {
                        dayW = 7;
                    }
                }

                var InsedIngresso = Insediamenti.Where(x =>
                int.TryParse(x.cod_insediamento, out int codins) &&
                    Convert.ToInt32(PrimaTimb.entrata.insediamento) == Convert.ToInt32(x.cod_insediamento) &&
                    (x.periodo_dal == null || (R.giornata.data >= x.periodo_dal && R.giornata.data <= x.periodo_al)) &&
                    (x.giorni_sett == null || x.giorni_sett.Contains(dayW.ToString()))
                ).FirstOrDefault();

                if (InsedIngresso != null && InsedIngresso.Entrata_Insediamento != null && InsedIngresso.Uscita_Insediamento != null)
                {
                    string oraInizio = ((DateTime)InsedIngresso.Entrata_Insediamento).ToString("HHmm");
                    string oraFine = ((DateTime)InsedIngresso.Uscita_Insediamento).ToString("HHmm");

                    int minInizio = oraInizio.ToMinutes();
                    int minFine = oraFine.ToMinutes();

                    if (PrimaTimb.entrata.orario.ToMinutes() <= minInizio || PrimaTimb.entrata.orario.ToMinutes() >= minFine)
                    {
                        if (Orario_Importo == "O")
                            return oraInizio + "/" + oraFine;
                        else
                            return InsedIngresso.Importo_Insediamento;
                    }
                }

                var InsedUscita = Insediamenti.Where(x =>
                 int.TryParse(x.cod_insediamento, out int codins) &&
                   Convert.ToInt32(UltimaTimbr.uscita.insediamento) == Convert.ToInt32(x.cod_insediamento) &&
                   (x.periodo_dal == null || (R.giornata.data >= x.periodo_dal && R.giornata.data <= x.periodo_al)) &&
                   (x.giorni_sett == null || x.giorni_sett.Contains(dayW.ToString()))
               ).FirstOrDefault();

                if (InsedUscita != null && InsedUscita.Entrata_Insediamento != null && InsedUscita.Uscita_Insediamento != null)
                {
                    string oraInizio = ((DateTime)InsedUscita.Entrata_Insediamento).ToString("HHmm");
                    string oraFine = ((DateTime)InsedUscita.Uscita_Insediamento).ToString("HHmm");

                    int minInizio = oraInizio.ToMinutes();
                    int minFine = oraFine.ToMinutes();

                    if (UltimaTimbr.uscita.orario.ToMinutes() <= minInizio || UltimaTimbr.uscita.orario.ToMinutes() >= minFine)
                    {
                        if (Orario_Importo == "O")
                            return oraInizio + "/" + oraFine;
                        else
                            return InsedUscita.Importo_Insediamento;
                    }
                }
            }

            return "0";
        }

        public static dayResponse GetGiornata(string data, string matricola)
        {
            WSDigigappDataController service = new WSDigigappDataController();

            dayResponse resp = service.GetEccezioni(CommonHelper.GetCurrentUserPMatricola(), matricola, data.Replace("/", ""), "BU", 70);

            return resp;
        }

        public static int GetTreQuartiGiornataMinuti(string matr, string data)
        {
            WSDigigappDataController service = new WSDigigappDataController();

            dayResponse resp = service.GetEccezioni(CommonHelper.GetCurrentUserMatricola(), matr, data.Replace("/", ""), "BU", 70);

            int minutiTotali = 0;
            int.TryParse(resp.orario.prevista_presenza, out minutiTotali);
            return minutiTotali - (minutiTotali / 4);
        }

        public static Boolean IsEccezione_0_50(string codice)
        {
            var db = new digiGappEntities();
            var ecc = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione.Trim().ToUpper() == codice.Trim().ToUpper()).FirstOrDefault();
            return (ecc != null && (ecc.flag_macroassen == "M" || ecc.flag_macroassen == "P"));
        }

        public static int GetMetaGiornataMinuti(string matr, string data)
        {
            WSDigigappDataController service = new WSDigigappDataController();

            dayResponse resp = service.GetEccezioni(CommonHelper.GetCurrentUserMatricola(), matr, data.Replace("/", ""), "BU", 70);

            int minutiTotali = 0;
            int.TryParse(resp.orario.prevista_presenza, out minutiTotali);
            return minutiTotali / 2;
        }
    }

    public class EsitoCompilatore
    {
        public List<string> EccezioniDaRimuovere { get; set; }
        public string Errore { get; set; }
    }

    public class EccPoss
    {
        public List<MyRai_Eccezioni_Ammesse> EccezioniPossibili { get; set; }

        private string _matr;
        public string matricola
        {
            get
            {
                if (_matr != null && _matr.Length == 6)
                    return "0" + _matr;
                else
                    return _matr;
            }
            set { _matr = value; }
        }
        public DateTime dataEccezioni { get; set; }
        public DateTime timeStamp { get; set; }
    }

    public class EsitoInserimentoEccezione
    {
        public string response { get; set; }
        public int id_richiesta_padre { get; set; }
    }

    public class EccezioneOrdinamento
    {
        public string codice { get; set; }
        public int ordinamento { get; set; }
    }

    public class EccezioniRichiesteShadow
    {
        public int id_eccezioni_richieste { get; set; }
        public int id_richiesta { get; set; }
        public string azione { get; set; }
        public string cod_eccezione { get; set; }
        public System.DateTime data_eccezione { get; set; }
        public Nullable<int> numero_documento { get; set; }
        public Nullable<System.DateTime> dalle { get; set; }
        public Nullable<System.DateTime> alle { get; set; }
        public Nullable<decimal> quantita { get; set; }
        public Nullable<decimal> importo { get; set; }
        public string uorg { get; set; }
        public string df { get; set; }
        public string matricola_spettacolo { get; set; }
        public string motivo_richiesta { get; set; }
        public string recapito_durante_assenza { get; set; }
        public int id_stato { get; set; }
        public Nullable<System.DateTime> data_validazione_primo_livello { get; set; }
        public Nullable<System.DateTime> data_rifiuto_primo_livello { get; set; }
        public string nota_rifiuto_o_approvazione { get; set; }
        public string matricola_primo_livello { get; set; }
        public Nullable<System.DateTime> data_stampa { get; set; }
        public Nullable<System.DateTime> data_convalida { get; set; }
        public string matricola_secondo_livello { get; set; }
        public string codice_sede_gapp { get; set; }
        public string nominativo_primo_livello { get; set; }
        public string nominativo_secondo_livello { get; set; }
        public Nullable<int> numero_documento_riferimento { get; set; }
        public System.DateTime data_creazione { get; set; }
        public string tipo_eccezione { get; set; }
        public string ValoriParamExtraJSON { get; set; }
    }

    public class InserimentoEccezioneModel
    {
        public string cambioturni { get; set; }
        public string data_da { get; set; }
        public string data_a { get; set; }
        public string dalle { get; set; }
        public string alle { get; set; }
        public string quantita { get; set; }
        public string importo { get; set; }
        public string df { get; set; }
        public string uorg { get; set; }
        public string matrspett { get; set; }

        public string cod_eccezione { get; set; }
        public string nota_richiesta { get; set; }

        public Boolean IsFromProposteAuto { get; set; }
        public Boolean DontChangeQuantita { get; set; }

        public string idAttivitaCeitonDaPropAutomatiche { get; set; }

        public Boolean IsFromBatch { get; set; }
        public String MatricolaForzataBatch { get; set; }

        public string Provenienza { get; set; }

        public Boolean DaProposteAuto { get; set; }

        public void SetDalleAlle(string intv)
        {
            if (String.IsNullOrWhiteSpace(intv) || !intv.Contains("/")) return;

            intv = intv.Replace(" ", "").Replace(",", "");
            string[] p = intv.Split('/');
            if (p == null || p.Length != 2 || p[0].Length != 5 || p[1].Length != 5) return;

            this.dalle = intv.Split('/')[0];
            this.alle = intv.Split('/')[1];
        }

        public string MatricolaApprovatoreProduzione { get; set; }

        public bool MatricolaUFFPERS { get; set; }

        public DateTime? DataCreazione { get; set; }

        public DateTime? DataApprovazione { get; set; }

        public string MatricolaApprovatoreCalcolata { get; set; }
    }
}
