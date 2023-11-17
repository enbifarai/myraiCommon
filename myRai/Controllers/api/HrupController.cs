using myRai.Business;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace myRai.Controllers.api
{
    public class HrupController : ApiController
    {
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public TestServiceResponse TestService()
        {
            TestServiceResponse t = new TestServiceResponse()
            {
                ContatoreDecimal = 10.5f,
                ContatoreIntero = 3,
                Data = DateTime.Today,
                Eccezione = "FE",
                Flag = true
            };
            return t;


        }

        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public GetPianoFerieMatricolaResponse GetPianoFerieMatricola(string matricola, string da, string a)
        {
            return FeriePermessiManager.GetPianoFerieMatricola(matricola, da, a);

            // http://localhost:53693/api/hrup/GetPianoFerieMatricola?matricola=103650&da=01012020&a=31122020

            GetPianoFerieMatricolaResponse Resp = new GetPianoFerieMatricolaResponse();
            var db = new myRaiData.digiGappEntities();

            Resp.Days = new List<DayInfo>();

            DateTime D1;
            DateTime D2;
            bool b1 = DateTime.TryParseExact(da, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D1);
            bool b2 = DateTime.TryParseExact(a, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D2);

            if (b1 == false || b2 == false)
                throw new Exception("Data invalida");

            if (D2 < D1)
                throw new Exception("Periodo non corretto");

            MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp service =
                  new MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp()
                  {
                      Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
                  };

            MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerie response =
                ServiceWrapper.GetPianoFerieWrapped(service, matricola, "0101" + D1.Year, 80, "");

            Resp.ContatoriCics = new Contatori()
            {
                exFestivitaAnniPrecedenti = response.dipendente.ferie.exFestivitaAnniPrecedenti,
                exFestivitaPianificate = response.dipendente.ferie.exFestivitaPianificate,
                exFestivitaRichieste = response.dipendente.ferie.exFestivitaRichieste,
                exFestivitaRimanenti = response.dipendente.ferie.exFestivitaRimanenti,
                exFestivitaSpettanti = response.dipendente.ferie.exFestivitaSpettanti,
                exFestivitaUsufruite = response.dipendente.ferie.exFestivitaUsufruite,
                ferieAnniPrecedenti = response.dipendente.ferie.ferieAnniPrecedenti,
                ferieMinime = response.dipendente.ferie.ferieMinime,
                feriePianificate = response.dipendente.ferie.feriePianificate,
                ferieRichieste = response.dipendente.ferie.ferieRichieste,
                ferieRimanenti = response.dipendente.ferie.ferieRimanenti,
                ferieSpettanti = response.dipendente.ferie.ferieSpettanti,
                ferieUsufruite = response.dipendente.ferie.ferieUsufruite,
                mancatiFestiviAnniPrecedenti = response.dipendente.ferie.mancatiFestiviAnniPrecedenti,
                mancatiFestiviSpettanti = response.dipendente.ferie.mancatiFestiviSpettanti,
                mancatiNonLavoratiAnniPrecedenti = response.dipendente.ferie.mancatiNonLavoratiAnniPrecedenti,
                mancatiNonLavoratiSpettanti = response.dipendente.ferie.mancatiNonLavoratiSpettanti,
                mancatiRiposiAnniPrecedenti = response.dipendente.ferie.mancatiRiposiAnniPrecedenti,
                mancatiRiposiSpettanti = response.dipendente.ferie.mancatiRiposiSpettanti,
                permessiGiornalistiAnniPrecedenti = response.dipendente.ferie.permessiGiornalistiAnniPrecedenti,
                permessiGiornalistiPianificati = response.dipendente.ferie.permessiGiornalistiPianificati,
                permessiGiornalistiRichiesti = response.dipendente.ferie.permessiGiornalistiRichiesti,
                permessiGiornalistiRimanenti = response.dipendente.ferie.permessiGiornalistiRimanenti,
                permessiGiornalistiSpettanti = response.dipendente.ferie.permessiGiornalistiSpettanti,
                permessiGiornalistiUsufruiti = response.dipendente.ferie.permessiGiornalistiUsufruiti,
                permessiPianificati = response.dipendente.ferie.permessiPianificati,
                permessiRichiesti = response.dipendente.ferie.permessiRichiesti,
                permessiRimanenti = response.dipendente.ferie.permessiRimanenti,
                permessiSpettanti = response.dipendente.ferie.permessiSpettanti,
                permessiUsufruiti = response.dipendente.ferie.permessiUsufruiti,
                recuperiMancatiFestiviPianificati = response.dipendente.ferie.recuperiMancatiFestiviPianificati,
                recuperiMancatiFestiviRichiesti = response.dipendente.ferie.recuperiMancatiFestiviRichiesti,
                recuperiMancatiFestiviRimanenti = response.dipendente.ferie.recuperiMancatiFestiviRimanenti,
                recuperiMancatiFestiviUsufruiti = response.dipendente.ferie.recuperiMancatiFestiviUsufruiti,
                recuperiMancatiRiposiPianificati = response.dipendente.ferie.recuperiMancatiRiposiPianificati,
                recuperiMancatiRiposiRichiesti = response.dipendente.ferie.recuperiMancatiRiposiRichiesti,
                recuperiMancatiRiposiRimanenti = response.dipendente.ferie.recuperiMancatiRiposiRimanenti,
                recuperiMancatiRiposiUsufruiti = response.dipendente.ferie.recuperiMancatiRiposiUsufruiti,
                recuperiNonLavoratiPianificati = response.dipendente.ferie.recuperiNonLavoratiPianificati,
                recuperiNonLavoratiRichiesti = response.dipendente.ferie.recuperiNonLavoratiRichiesti,
                recuperiNonLavoratiRimanenti = response.dipendente.ferie.recuperiNonLavoratiRimanenti,
                recuperiNonLavoratiUsufruiti = response.dipendente.ferie.recuperiNonLavoratiUsufruiti,
                totaleConvalidato = response.dipendente.ferie.totaleConvalidato,
                visualizzaFC = response.dipendente.ferie.visualizzaFC,
                visualizzaFerie = response.dipendente.ferie.visualizzaFerie,
                visualizzaPermessi = response.dipendente.ferie.visualizzaPermessi,
                visualizzaPermessiGiornalisti = response.dipendente.ferie.visualizzaPermessiGiornalisti,
                visualizzaRecuperoFestivi = response.dipendente.ferie.visualizzaRecuperoFestivi,
                visualizzaRecuperoNonLavorati = response.dipendente.ferie.visualizzaRecuperoNonLavorati,
                visualizzaRecuperoRiposi = response.dipendente.ferie.visualizzaRecuperoRiposi
            };
            var datiUtente = BatchManager.GetUserData(matricola);

            var RowExcel = FeriePermessiManager.GetFileExcelInfo(datiUtente.tipo_dipendente, matricola);
            if (RowExcel != null)
            {
                Resp.Accordi = new AccordiSindacali2020();

                float ArretratiDaMettereDaFoglioExcel = (int)RowExcel.da_fare;// - (int)model.ItemFoglioExcelArretrati.fruite_11_31;
                ArretratiDaMettereDaFoglioExcel -= myRaiCommonTasks.Arretrati2019.GetFECE_MNCE_MRCE_MenoDonateSuFoglioExcel(matricola);

                if (ArretratiDaMettereDaFoglioExcel > 0)
                {
                    float[] valori;
                    if (RowExcel.estensione_marzo == true)
                    {
                        Resp.Accordi.DataLimiteArretrati = new DateTime(2021, 3, 31);

                        valori = myRaiCommonTasks.Arretrati2019.GetGiornalisti_RR_RF_FE(
                                                Resp.ContatoriCics.mancatiRiposiAnniPrecedenti,
                                                Resp.ContatoriCics.mancatiFestiviAnniPrecedenti,
                                                Resp.ContatoriCics.ferieAnniPrecedenti,
                                                ArretratiDaMettereDaFoglioExcel,
                                                matricola);
                        if (valori[0] > 0)
                        {
                            int MRanno = FeriePermessiManager.GetContatoriSingolaEcc(matricola, D1.Year, "MR");
                            if (MRanno > 0)
                            {
                                if (MRanno >= valori[0])
                                    valori[0] += valori[0];
                                else
                                    valori[0] += MRanno;
                            }
                        }
                    }
                    else
                    {
                        Resp.Accordi.DataLimiteArretrati = new DateTime(2020, 10, 31);
                        valori = myRaiCommonTasks.Arretrati2019.GetRR_RF_FE(
                                                Resp.ContatoriCics.mancatiRiposiAnniPrecedenti,
                                                Resp.ContatoriCics.mancatiFestiviAnniPrecedenti,
                                                ArretratiDaMettereDaFoglioExcel,
                                                matricola);
                        if (FeriePermessiManager.UtenteProduzione(datiUtente.sede_gapp))
                            Resp.Accordi.DataLimiteArretrati = new DateTime(2020, 12, 31);
                    }

                    Resp.Accordi.HaEstensioneMarzo = RowExcel.estensione_marzo == true ? true : false;
                    Resp.Accordi.ArretratiRR = valori[0];
                    Resp.Accordi.ArretratiRF = valori[1];
                    Resp.Accordi.ArretratiFE = valori[2];
                    Resp.Accordi.ArretratiDaFoglioExcel = valori[0] + valori[1] + valori[2];
                }
                Resp.MinimoDaPianificareEntroDataLimite = Resp.Accordi.ArretratiFE +
                    Resp.ContatoriCics.ferieSpettanti;
            }
            else
                Resp.MinimoDaPianificareEntroDataLimite = Resp.ContatoriCics.ferieSpettanti;

            Resp.MinimoDaPianificareEntro30Settembre = (int)(Resp.ContatoriCics.ferieSpettanti * 0.75f);
            Resp.MassimoPianificabile = Resp.ContatoriCics.ferieSpettanti + Resp.ContatoriCics.ferieAnniPrecedenti;


            Resp.Donate = new Donazioni();
            var cont = myRaiCommonTasks.Arretrati2019.GetFECE_MNCE_MRCE_Details(matricola, D1.Year);

            Resp.Donate.FECE = cont.ContatoriEccezioni.Where(x => x.CodiceEccezione == "FECE")
                .Select(x => Convert.ToSingle(x.Totale)).FirstOrDefault();
            Resp.Donate.FEDO = cont.ContatoriEccezioni.Where(x => x.CodiceEccezione == "FEDO")
                .Select(x => Convert.ToSingle(x.Totale)).FirstOrDefault();
            Resp.Donate.MFCE = cont.ContatoriEccezioni.Where(x => x.CodiceEccezione == "MFCE")
                .Select(x => Convert.ToSingle(x.Totale)).FirstOrDefault();
            Resp.Donate.MNCE = cont.ContatoriEccezioni.Where(x => x.CodiceEccezione == "MNCE")
                .Select(x => Convert.ToSingle(x.Totale)).FirstOrDefault();
            Resp.Donate.MNDO = cont.ContatoriEccezioni.Where(x => x.CodiceEccezione == "MNDO")
                .Select(x => Convert.ToSingle(x.Totale)).FirstOrDefault();
            Resp.Donate.MRCE = cont.ContatoriEccezioni.Where(x => x.CodiceEccezione == "MRCE")
                .Select(x => Convert.ToSingle(x.Totale)).FirstOrDefault();
            Resp.Donate.MRDO = cont.ContatoriEccezioni.Where(x => x.CodiceEccezione == "MRDO")
               .Select(x => Convert.ToSingle(x.Totale)).FirstOrDefault();

            string sedegapp = datiUtente.sede_gapp;
            if (!String.IsNullOrWhiteSpace(datiUtente.CodiceReparto) && datiUtente.CodiceReparto.Trim() != "0"
                && datiUtente.CodiceReparto.Trim() != "00")
            {
                sedegapp += datiUtente.CodiceReparto;
            }
            var pfsede = db.MyRai_PianoFerieSedi.Where(x => x.sedegapp == sedegapp && x.anno == D1.Year).FirstOrDefault();
            var pfdip = db.MyRai_PianoFerie.Where(x => x.anno == D1.Year && x.matricola == matricola).FirstOrDefault();

            Resp.PFstatus = new PianoFerieStatus()
            {
                anno = D1.Year,
                data_approvata = (pfsede == null ? null : pfsede.data_approvata),
                data_firma = (pfsede == null ? null : pfsede.data_firma),
                data_storno_approvazione = (pfsede == null ? null : pfsede.data_storno_approvazione),
                matricola_approvatore = (pfsede == null ? null : pfsede.matricola_approvatore),
                matricola_firma = (pfsede == null ? null : pfsede.matricola_firma),
                matricola_storno = (pfsede == null ? null : pfsede.matricola_storno),
                numero_versione = (pfsede == null ? 0 : pfsede.numero_versione),
                sedegapp = sedegapp,
                data_invio_dipendente = (pfdip == null ? null : pfdip.data_consolidato),
                data_invio_segreteria = (pfdip == null ? null : pfdip.data_invio_segreteria),
                matricola_invio_segreteria = (pfdip == null ? null : pfdip.matricola_invio_segreteria),
                nota_invio_segreteria = (pfdip == null ? null : pfdip.nota_invio_segreteria)
            };
            var ListaGiorniPF = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matricola).ToList();

            DateTime Dcurr = new DateTime();
            //http://localhost:53693/api/hrup/getpianoferiematricola?matricola=103650&da=10102020&a=10022021
            Dcurr = D1;
            while (Dcurr <= D2)
            {
                string dataDigigapp = Dcurr.ToString("dd-MM");
                var giornata = response.dipendente.ferie.giornate.Where(x => x.dataTeorica == dataDigigapp).FirstOrDefault();

                DayInfo day = new DayInfo();
                day.Data = Dcurr;
                day.Gapp = new GappInfo()
                {
                    EccezioneFeriePermesso = giornata.codiceVisualizzazione.Trim(),
                    TipoGiornata = giornata.tipoGiornata,
                    OrarioReale = giornata.orarioReale,
                    OrarioTeorico = giornata.orarioTeorico
                };
                var giornoPF = ListaGiorniPF.Where(x => x.data == Dcurr).FirstOrDefault();
                if (giornoPF != null)
                {
                    day.PianoFerie = new PianoFerieInfo()
                    {
                        data_inserimento = giornoPF.data_inserimento,
                        EccezioneFeriePermesso = giornoPF.eccezione
                    };
                }
                Resp.Days.Add(day);

                Dcurr = Dcurr.AddDays(1);
                if (Dcurr.Month == 1 && Dcurr.Day == 1 && Dcurr.Year != D1.Year)
                {
                    response = ServiceWrapper.GetPianoFerieWrapped(service, matricola, "0101" + Dcurr.Year, 80, "");
                }
            }
            return Resp;
        }
    }
}
