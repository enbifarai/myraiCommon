using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRai.Business;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using MyRaiServiceInterface.MyRaiServiceReference1;
using Giornata = MyRaiServiceInterface.it.rai.servizi.digigappws.Giornata;
using Utente = myRai.Models.Utente;

namespace myRaiCommonManager
{
    public class FeriePermessiControllerScope : SessionScope<FeriePermessiControllerScope>
    {
        public FeriePermessiControllerScope()
        {
            _anno = DateTime.Now.Year;
            _giorniCalendario = new List<CalendarioDay>();
        }
        private int _anno { get; set; }
        public int anno
        {
            get
            {
                return _anno;
            }
            set
            {
                _anno = value;
            }
        }
        private List<CalendarioDay> _giorniCalendario { get; set; }
        public List<CalendarioDay> giorniCalendario
        {
            get
            {
                return _giorniCalendario;
            }
            set
            {
                _giorniCalendario = value;
            }
        }
        private List<TipoPermessoFerieUsato> _tipiGiornata { get; set; }
        public List<TipoPermessoFerieUsato> tipiGiornata
        {
            get
            {
                return _tipiGiornata;
            }
            set
            {
                _tipiGiornata = value;
            }
        }
    }

    public enum EnumPrefixFeriePermessi
    {
        ferie,
        exFestivita,
        permessi,
        permessiGiornalisti,
        permessiGiornalistiCompensativi,
        mancatiRiposi,
        recuperiMancatiRiposi,
        mancatiNonLavorati,
        recuperiNonLavorati,
        mancatiFestivi,
        recuperiMancatiFestivi
    }

    public enum EnumCodiceVisualizzazioneFeriePermessi
    {
        FE,
        PF,
        PR,
        PX,
        PXC,
        MN,
        MR,
        MF,
        RN,
        RR,
        RF
    }


    public class FeriePermessiManager
    {
        public static void InviaMailDipendentePFrifiutato(string matricola, string nota, List<string> listaGiorni)
        {
            myRaiCommonTasks.GestoreMail gm = new myRaiCommonTasks.GestoreMail();
            string dest = "P" + matricola + "@rai.it";

            if (!CommonManager.IsProduzione())
                dest = "ruo.sip.presidioopen@rai.it";

            string giorni = "";
            if (listaGiorni.Any())
            {
                giorni = "<br /><br /><b>Giorni:</b><br />";
                foreach (var g in listaGiorni)
                    giorni += g + "<br />";
            }
            gm.InvioMail(
                mittente: "raiplace.selfservice@rai.it",
                oggetto: "Mancata approvazione piano ferie",
                destinatari: dest,
                destinatariCC: null,
                titolo: "Mancata approvazione piano ferie",
                sottotitolo: null,
                corpo: "Il tuo piano ferie non è stato approvato dal responsabile e necessita del tuo intervento per un nuovo invio.<br /><br />" +
                " <b>Nota del responsabile:</b> <br />" + nota + giorni
                );

            NotificheManager.InserisciNotifica("Piano ferie non approvato dal responsabile",
                                                    "PianoFerie", matricola, "Portale", 0);

        }
        public static string RimuoviGiorniPFdipendente(string matricola, List<string> ListaGiorni, string nota, int quantisel)
        {
            var db = new digiGappEntities();
            var pfdip = db.MyRai_PianoFerie.Where(x => x.matricola == matricola && x.anno == DateTime.Now.Year).FirstOrDefault();
            if (pfdip != null && pfdip.Id_pdf_pianoferie_inclusa != null)
            {
                var pfsede = db.MyRai_PianoFerieSedi.Where(x => x.id == pfdip.Id_pdf_pianoferie_inclusa).FirstOrDefault();
                if (pfsede != null)
                {
                    return "E' presente un PDF in firma, impossibile modificare piano ferie";
                }
            }
            if (pfdip != null)
            {
                pfdip.data_consolidato = null;
                pfdip.data_approvato = null;
                pfdip.approvatore = null;


                foreach (string giorno in ListaGiorni)
                {
                    DateTime D;
                    if (DateTime.TryParseExact(giorno.Replace("/", ""), "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D))
                    {
                        var daypf = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matricola && x.data == D).FirstOrDefault();
                        if (daypf != null)
                        {
                            db.MyRai_PianoFerieGiorni.Remove(daypf);
                        }
                    }
                }



                try
                {
                    db.SaveChanges();
                    if (quantisel == 0)
                        ListaGiorni = new List<string>();

                    InviaMailDipendentePFrifiutato(matricola, nota, ListaGiorni);
                }
                catch (Exception ex)
                {
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        error_message = ex.ToString()
                    });
                    return "Errore dati DB";
                }
            }
            return null;
        }

        public static GetPianoFerieMatricolaResponse GetPianoFerieMatricola(string matricola, string da, string a)
        {
            // http://localhost:53693/api/hrup/GetPianoFerieMatricola?matricola=103650&da=01012020&a=31122020

            GetPianoFerieMatricolaResponse Resp = new GetPianoFerieMatricolaResponse();
            var db = new myRaiData.digiGappEntities();

            Resp.Days = new List<DayInfo>();

            DateTime D1;
            DateTime D2;
            bool b1 = DateTime.TryParseExact(da.Replace("/", "").Replace("-", ""), "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D1);
            bool b2 = DateTime.TryParseExact(a.Replace("/", "").Replace("-", ""), "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D2);

            if (b1 == false || b2 == false)
                throw new Exception("Data invalida");

            if (D2 < D1)
                throw new Exception("Periodo non corretto");
            int Anno = D1.Year;

            MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp service =
                  new MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp()
                  {
                      Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
                  };

            MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerie response =
                ServiceWrapper.GetPianoFerieWrapped(service, matricola, "0101" + Anno, 80, "");

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
            var pfsede = db.MyRai_PianoFerieSedi.Where(x => x.sedegapp == sedegapp && x.anno == Anno).FirstOrDefault();
            var pfdip = db.MyRai_PianoFerie.Where(x => x.anno == Anno && x.matricola == matricola).FirstOrDefault();

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

        public static bool IsTorinoCavalliOK()
        {
            var db = new myRaiData.digiGappEntities();
            WSDigigapp service = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            };
            var resp = service.getEccezioni(CommonManager.GetCurrentUserMatricola(), "2506" + DateTime.Now.Year, "BU", 80);
            if (resp == null || resp.eccezioni == null || !resp.eccezioni.Any())
                return false;

            bool esisteC = false;
            foreach (var ecc in resp.eccezioni)
            {
                var e = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione == ecc.cod).FirstOrDefault();
                if (e != null && e.flag_eccez == "C")
                    esisteC = true;
            }
            return esisteC;
        }
        public static bool IsMazziniOK1()
        {
            var db = new myRaiData.digiGappEntities();
            WSDigigapp service = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            };
            var resp = service.getEccezioni(CommonManager.GetCurrentUserMatricola(), "2806" + DateTime.Now.Year, "BU", 80);
            if (resp == null || resp.eccezioni == null || !resp.eccezioni.Any())
                return false;

            bool esisteC = false;
            foreach (var ecc in resp.eccezioni)
            {
                var e = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione == ecc.cod).FirstOrDefault();
                if (e != null && e.flag_eccez == "C")
                    esisteC = true;
            }
            return esisteC;
        }


        public static bool IsMazziniOK2()
        {
            var db = new myRaiData.digiGappEntities();
            WSDigigapp service = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            };
            var resp = service.getEccezioni(CommonManager.GetCurrentUserMatricola(), "0712" + DateTime.Now.Year, "BU", 80);
            if (resp == null || resp.eccezioni == null || !resp.eccezioni.Any())
                return false;

            bool esisteC = false;
            foreach (var ecc in resp.eccezioni)
            {
                var e = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione == ecc.cod).FirstOrDefault();
                if (e != null && e.flag_eccez == "C")
                    esisteC = true;
            }
            return esisteC;

        }

        public static ArretratiHRDW_normalized GetArretratiHRDW(string matricola, float FEanniPrecedenti)
        {
            ArretratiHRDW_normalized an = new ArretratiHRDW_normalized();

            var db = new myRaiData.CurriculumVitae.cv_ModelEntities();
            string queryhrdw = CommonManager.GetParametro<string>(EnumParametriSistema.QueryArretratiHRDW);

            string q = queryhrdw.Replace("#MATR#", matricola);

            List<ArretratiHRDW> res = db.Database.SqlQuery<ArretratiHRDW>(q).ToList();
            if (res != null && res.Any())
            {
                DateTime dnow = DateTime.Now;
                var row = res.Where(x => x.data_inizio_validita <= dnow && x.data_fine_validita >= dnow).FirstOrDefault();
                if (row != null)
                {
                    if (row.ARR_FE_da_smaltire != null)
                        an.ARR_FE_da_smaltire = (decimal)row.ARR_FE_da_smaltire;


                    //IsDirigente
                    if (Utente.TipoDipendente() == "D")
                    {
                        if (FEanniPrecedenti > 20)
                        {
                            double diff = 0.15 * (FEanniPrecedenti - 20);
                            an.ARR_FE_da_smaltire = (decimal)Math.Round(diff);
                        }
                        else
                            an.ARR_FE_da_smaltire = 0;
                    }

                    if (row.ARR_MR_da_smaltire != null)
                        an.ARR_MR_da_smaltire = (decimal)row.ARR_MR_da_smaltire;

                    if (row.ARR_MN_da_smaltire != null)
                        an.ARR_MN_da_smaltire = (decimal)row.ARR_MN_da_smaltire;


                    if (row.GG_ARR_da_smaltire_giorn != null)//dic
                        an.GG_ARR_da_smaltire_giorn = (decimal)row.GG_ARR_da_smaltire_giorn;

                    if (row.GG_competenza_AP_giorn != null)//marzo
                        an.GG_competenza_AP_giorn = (decimal)row.GG_competenza_AP_giorn;

                    var cont = myRaiCommonTasks.Arretrati2019.GetFECE_MNCE_MRCE_Details(matricola, DateTime.Today.Year);

                    float FECE = cont.ContatoriEccezioni.Where(x => x.CodiceEccezione == "FECE")
                     .Select(x => Convert.ToSingle(x.Totale)).FirstOrDefault();
                    float FEDO = cont.ContatoriEccezioni.Where(x => x.CodiceEccezione == "FEDO")
                        .Select(x => Convert.ToSingle(x.Totale)).FirstOrDefault();


                    if (an.ARR_FE_da_smaltire > 0)
                    {
                        an.ARR_FE_da_smaltire = an.ARR_FE_da_smaltire - (decimal)FECE - (decimal)FEDO;
                        if (an.ARR_FE_da_smaltire < 0)
                            an.ARR_FE_da_smaltire = 0;
                    }
                    if (Utente.TipoDipendente() == "G")
                    {
                        float MRCE = cont.ContatoriEccezioni.Where(x => x.CodiceEccezione == "MRCE")
                        .Select(x => Convert.ToSingle(x.Totale)).FirstOrDefault();
                        float MRDO = cont.ContatoriEccezioni.Where(x => x.CodiceEccezione == "MRDO")
                            .Select(x => Convert.ToSingle(x.Totale)).FirstOrDefault();
                        float donate = FECE + FEDO + MRCE + MRDO; ;

                        if (donate > 0)
                        {
                            if (an.GG_competenza_AP_giorn > 0)
                            {
                                an.GG_competenza_AP_giorn -= (decimal)donate;
                                if (an.GG_competenza_AP_giorn < 0)
                                {
                                    an.GG_ARR_da_smaltire_giorn = an.GG_ARR_da_smaltire_giorn + an.GG_competenza_AP_giorn;
                                    an.GG_competenza_AP_giorn = 0;
                                    if (an.GG_ARR_da_smaltire_giorn < 0) an.GG_ARR_da_smaltire_giorn = 0;
                                }
                            }
                            else if (an.GG_ARR_da_smaltire_giorn > 0)
                            {
                                an.GG_ARR_da_smaltire_giorn -= (decimal)donate;
                                if (an.GG_ARR_da_smaltire_giorn < 0) an.GG_ARR_da_smaltire_giorn = 0;
                            }
                        }
                    }
                }
            }
            return an;
        }


        public static MyRai_ArretratiExcel2019 GetFileExcelInfo(string tipo_dipendente = null, string matr = null)
        {
            return null;
            if (tipo_dipendente == null) tipo_dipendente = Utente.TipoDipendente();

            if ("O".Contains(tipo_dipendente)) return null;

            if (matr == null) matr = CommonManager.GetCurrentUserMatricola();
            var db = new digiGappEntities();
            var item = db.MyRai_ArretratiExcel2019.Where(x => x.matricola == matr).FirstOrDefault();
            return item;
        }

        public static FerieDipendente SetYear(FerieDipendente model)
        {
            int? anno = (int?)SessionHelper.Get(SessionVariables.AnnoFeriePermessi);

            if (anno != null && (int)anno != DateTime.Now.Year)
            {
                foreach (var g in model.GiorniFerie.Where(x => x.data != default(DateTime)))
                {
                    g.data = new DateTime((int)anno, g.data.Month, g.data.Day);
                }
            }
            return model;


        }

        public static Boolean IsAPIrequest()
        {
            return (HttpContext.Current.Request.RawUrl != null && HttpContext.Current.Request.RawUrl.ToLower().Contains("api/"));
        }

        public static FerieDipendente GetFeriePermessiGeneric(pianoFerie response,
            EnumPrefixFeriePermessi prefix, EnumCodiceVisualizzazioneFeriePermessi codiceVis, Boolean FromCalendario = false)
        {

            string mf = codiceVis == EnumCodiceVisualizzazioneFeriePermessi.FE || codiceVis == EnumCodiceVisualizzazioneFeriePermessi.PF ? "e" : "i";
            FerieDipendente f = new FerieDipendente();
            f.AnnoPrec = Convert.ToSingle(CommonManager.GetPropertyByName(response.dipendente.ferie, prefix + "AnniPrecedenti"));

            f.Pianificate = Convert.ToSingle(CommonManager.GetPropertyByName(response.dipendente.ferie, prefix + "Pianificat" + mf))
                + Convert.ToSingle(CommonManager.GetPropertyByName(response.dipendente.ferie, prefix + "Richiest" + mf));

            f.Residue = Convert.ToSingle(CommonManager.GetPropertyByName(response.dipendente.ferie, prefix + "Rimanenti"));

            f.Spettanti = Convert.ToSingle(CommonManager.GetPropertyByName(response.dipendente.ferie, prefix + "Spettanti"));

            f.Usufruite = Convert.ToSingle(CommonManager.GetPropertyByName(response.dipendente.ferie, prefix + "Usufruit" + mf));

            if (IsAPIrequest()) return f;

            if (response != null && response.dipendente != null && response.dipendente.ferie != null && response.dipendente.ferie.giornate != null)
            {
                f.GiorniFerie = response.dipendente.ferie.giornate.Where(x => x.codiceVisualizzazione == codiceVis.ToString()).ToArray();

                foreach (Giornata g in f.GiorniFerie)
                {
                    GetGiornataForModel(g, FromCalendario);
                }
            }

            return f;
        }
        public static FerieDipendente GetMancatiRiposi()
        {
            pianoFerie response = GetPianoFerieAnno(DateTime.Now.Year);
            FerieDipendente f = new FerieDipendente();
            f.AnnoPrec = response.dipendente.ferie.mancatiRiposiAnniPrecedenti;
            f.Pianificate = response.dipendente.ferie.recuperiMancatiRiposiPianificati +
                            response.dipendente.ferie.recuperiMancatiRiposiRichiesti;

            f.Residue = response.dipendente.ferie.recuperiMancatiRiposiRimanenti;
            f.Spettanti = response.dipendente.ferie.mancatiRiposiSpettanti;
            f.Usufruite = response.dipendente.ferie.recuperiMancatiRiposiUsufruiti;

            f.GiorniFerie = response.dipendente.ferie.giornate.Where(x =>
                                                                        //x.codiceVisualizzazione == "MR"
                                                                        //      ||
                                                                        x.codiceVisualizzazione == "RR").ToArray();

            foreach (Giornata g in f.GiorniFerie)
            {
                GetGiornataForModel(g);
            }

            f = SetYear(f);
            return f;
        }
        public static FerieDipendente GetMancatiFestivi()
        {
            pianoFerie response = GetPianoFerieAnno(DateTime.Now.Year);
            FerieDipendente f = new FerieDipendente();
            f.AnnoPrec = response.dipendente.ferie.mancatiFestiviAnniPrecedenti;
            f.Pianificate = response.dipendente.ferie.recuperiMancatiFestiviPianificati +
                            response.dipendente.ferie.recuperiMancatiFestiviRichiesti;

            f.Residue = response.dipendente.ferie.recuperiMancatiFestiviRimanenti;
            f.Spettanti = response.dipendente.ferie.mancatiFestiviSpettanti;
            f.Usufruite = response.dipendente.ferie.recuperiMancatiFestiviUsufruiti;

            f.GiorniFerie = response.dipendente.ferie.giornate.Where(x => x.codiceVisualizzazione == "MF"
                                                                        || x.codiceVisualizzazione == "RF").ToArray();

            foreach (Giornata g in f.GiorniFerie)
            {
                GetGiornataForModel(g);
            }

            f = SetYear(f);
            return f;
        }
        public static FerieDipendente GetMancatiNonLavorati()
        {
            pianoFerie response = GetPianoFerieAnno(DateTime.Now.Year);
            FerieDipendente f = new FerieDipendente();
            f.IsGiornalista = response.dipendente.ferie.visualizzaPermessiGiornalisti;
            f.AnnoPrec = response.dipendente.ferie.mancatiNonLavoratiAnniPrecedenti;
            f.Pianificate = response.dipendente.ferie.recuperiNonLavoratiPianificati +
                            response.dipendente.ferie.recuperiNonLavoratiRichiesti;

            f.Residue = response.dipendente.ferie.recuperiNonLavoratiRimanenti;
            f.Spettanti = response.dipendente.ferie.mancatiNonLavoratiSpettanti;
            f.Usufruite = response.dipendente.ferie.recuperiNonLavoratiUsufruiti;

            f.GiorniFerie = response.dipendente.ferie.giornate.Where(x => x.codiceVisualizzazione == "MN"
                                                                        || x.codiceVisualizzazione == "RN").ToArray();

            foreach (Giornata g in f.GiorniFerie)
            {
                GetGiornataForModel(g);
            }

            f = SetYear(f);

            #region RECUPERO SCADENZE MN DA TABELLA
            f.MNScadenze = new List<MNDettaglioScadenza>();
            string matricola = CommonManager.GetCurrentUserMatricola();
            matricola = "0" + matricola;

            using (digiGappEntities db = new digiGappEntities())
            {
                var items = db.MyRai_Importazioni.Where(w => w.Matricola == "ImportazioneMN" &&
                    w.Tabella == "ImportazioneMN" &&
                    w.Parametro1 == matricola &&
                    w.Parametro6 == "true").ToList();

                if (items != null && items.Any())
                {
                    items.ForEach(w =>
                   {
                       DateTime dt1;
                       bool convertDate = DateTime.TryParseExact(w.Parametro4, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dt1);

                       DateTime dt2;
                       convertDate = DateTime.TryParseExact(w.Parametro5, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dt2);

                       DateTime dt3;
                       convertDate = DateTime.TryParseExact(w.Parametro10, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dt3);

                        // se la data è antecedente ad oggi, 
                        // allora è scaduta e non va presa 
                        // in considerazione
                        if (dt2.Date >= DateTime.Now.Date)
                       {
                           f.MNScadenze.Add(new MNDettaglioScadenza()
                           {
                               Matricola = matricola,
                               DataEccezione = dt1,
                               DataScadenza1 = dt2,
                               DataScadenza2 = dt3
                           });
                       }
                   });
                }
            }
            #endregion

            return f;
        }
        public static FerieDipendente GetFerie()
        {
            pianoFerie response = GetPianoFerieAnno(DateTime.Now.Year);

            FerieDipendente f = GetFeriePermessiGeneric(response, EnumPrefixFeriePermessi.ferie,
                EnumCodiceVisualizzazioneFeriePermessi.FE);

            f = SetYear(f);
            return f;
        }

        public static FerieDipendente GetPermessiExFest()
        {
            pianoFerie response = GetPianoFerieAnno(DateTime.Now.Year);
            FerieDipendente f = GetFeriePermessiGeneric(response, EnumPrefixFeriePermessi.exFestivita,
                EnumCodiceVisualizzazioneFeriePermessi.PF);

            f = SetYear(f);
            return f;

        }

        public static FerieDipendente GetPermessiRetr()
        {
            pianoFerie response = GetPianoFerieAnno(DateTime.Now.Year);
            FerieDipendente f = GetFeriePermessiGeneric(response, EnumPrefixFeriePermessi.permessi,
                        EnumCodiceVisualizzazioneFeriePermessi.PR);

            f = SetYear(f);
            return f;
        }
        public static FerieDipendente GetPXCompensativi()
        {
            pianoFerie response = GetPianoFerieAnno(DateTime.Now.Year);
            FerieDipendente f = new FerieDipendente();
            /*  f.AnnoPrec = response.dipendente.ferie.mancatiRiposiAnniPrecedenti;
              f.Pianificate = response.dipendente.ferie.recuperiMancatiRiposiPianificati +
                              response.dipendente.ferie.recuperiMancatiRiposiRichiesti;

              f.Residue = response.dipendente.ferie.recuperiMancatiRiposiRimanenti;
              f.Spettanti = response.dipendente.ferie.mancatiRiposiSpettanti;
              f.Usufruite = response.dipendente.ferie.recuperiMancatiRiposiUsufruiti;*/

            /*   f.GiorniFerie = response.dipendente.ferie.giornate.Where(x => x.codiceVisualizzazione == "MR"
                                                                           || x.codiceVisualizzazione == "RR").ToArray();

               foreach (Giornata g in f.GiorniFerie)
               {
                   GetGiornataForModel(g);
               }*/

            f = SetYear(f);
            return f;
        }
        public static pianoFerie GetPianoFerieAnno(int anno, Boolean Forcerequest = false, string matricola = null,
            bool FromAPI = false
            )
        {
            if (matricola == null) matricola = CommonManager.GetCurrentUserMatricola();
            if (SessionManager.Get(SessionVariables.AnnoFeriePermessi) != null)
                anno = (int)SessionManager.Get(SessionVariables.AnnoFeriePermessi);

            if (Forcerequest) // HttpContext.Current.Session["pianoferie"]
            {
                WSDigigapp service = new WSDigigapp()
                {
                    Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
                };
                pianoFerie response = new pianoFerie();
                if (anno == DateTime.Now.Year)
                {
                    response = ServiceWrapper.GetPianoFerieWrapped(service, matricola,
                   "0101" + anno.ToString(), 75,
                   //DateTime.Now.ToString("ddMMyyyy"), 75,
                   (FromAPI == true ? "" : Utente.TipoDipendente())
                   );
                    SessionManager.Set(SessionVariables.PianoFerie, response);
                    //HttpContext.Current.Session["pianoferie"] = response;


                }
                else
                {
                    response = ServiceWrapper.GetPianoFerieWrapped(service, matricola,
                        "3112" + anno.ToString(), 75,
                      (FromAPI == true ? "" : Utente.TipoDipendente())
                        );
                    SessionManager.Set(SessionVariables.PianoFerie, response);
                    //HttpContext.Current.Session["pianoferie"] = response;


                }

                return response;
            }
            else
            {
                if (SessionManager.Get(SessionVariables.PianoFerie) == null)
                //if (HttpContext.Current.Session["pianoferie"] == null)
                {
                    WSDigigapp service = new WSDigigapp()
                    {
                        Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
                    };
                    pianoFerie response = ServiceWrapper.GetPianoFerieWrapped(service, matricola,
                        "0101" + anno.ToString(), 75,
                        (FromAPI == true ? "" : Utente.TipoDipendente())
                        );
                    SessionManager.Set(SessionVariables.PianoFerie, response);
                    //HttpContext.Current.Session["pianoferie"] = response;

                    return response;
                }
                // else return (pianoFerie) HttpContext.Current.Session["pianoferie"] ;
                else
                {
                    var pf = (pianoFerie)SessionManager.Get(SessionVariables.PianoFerie);
                    if (pf.raw != null && pf.raw.Contains("0101" + anno) && pf.raw.Contains(anno.ToString()))
                        return pf;
                    else
                    {
                        WSDigigapp service = new WSDigigapp()
                        {
                            Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
                        };
                        pianoFerie response = ServiceWrapper.GetPianoFerieWrapped(service, matricola,
                            "0101" + anno.ToString(), 75,
                            (FromAPI == true ? "" : Utente.TipoDipendente())
                            );
                        SessionManager.Set(SessionVariables.PianoFerie, response);
                        //HttpContext.Current.Session["pianoferie"] = response;

                        return response;
                    }
                }
            }

        }

        public static DateTime GetDataFromGiornata(Giornata g, int year)
        {
            DateTime d;
            DateTime.TryParseExact(g.dataTeorica + "-" + year.ToString(), "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out d);
            return d;
        }

        public static Giornata GetGiornataForModel(Giornata g, Boolean FromCalendario = false)
        {
            var db = new myRaiData.digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();
            var stati = db.MyRai_Stati.ToList();

            DateTime DataGiornata = GetDataFromGiornata(g, DateTime.Now.Year);
            g.data = DataGiornata;

            if (g.codiceVisualizzazione == "FE" || g.codiceVisualizzazione == "PF")
            {
                if (g.quotaGiornata == "M" || g.quotaGiornata == "P")
                    g.Frazione = "Mezza giornata";
                else
                    g.Frazione = "Intera giornata";
            }
            if (g.codiceVisualizzazione.StartsWith("M"))
                g.Frazione = "Intera giornata";

            if (g.codiceVisualizzazione == "PR")
            {
                if (g.quotaGiornata == "M" || g.quotaGiornata == "P")
                    g.Frazione = "Mezza giornata";
                else if (g.codiceVisualizzazione == "PR" && g.quotaGiornata == "Q")
                    g.Frazione = "Un quarto di giornata";
                else
                    g.Frazione = "Intera giornata";
            }

            if (g.codiceVisualizzazione == "FE" || g.codiceVisualizzazione.StartsWith("P"))
                g.Provenienza = "";// "Piano Ferie";
            else
                g.Provenienza = "";

            if (g.codiceVisualizzazione.StartsWith("M")) g.Provenienza = "";

            if (FromCalendario) return g;


            var ecc = db.MyRai_Eccezioni_Richieste.Where(x => x.data_eccezione == DataGiornata
                && x.cod_eccezione.StartsWith(g.codiceVisualizzazione)
                && x.MyRai_Richieste.matricola_richiesta == matr).ToList();
            if (ecc.Count > 0)
            {
                var eccStorno = ecc.Where(x => x.azione == "C").FirstOrDefault();
                if (eccStorno == null)
                {
                    var st = stati.Where(x => x.id_stato == ecc[0].id_stato).FirstOrDefault();
                    if (st != null)
                    {
                        g.StatoDescrizione = st.descrizione_stato;
                        g.Stato = ecc[0].id_stato;
                        if (g.Stato == 20 && ecc[0].data_validazione_primo_livello != null) g.StatoDescrizione += " il " + ((DateTime)ecc[0].data_validazione_primo_livello).ToString("dd/MM/yyyy");
                    }
                }
                else
                {
                    var st = stati.Where(x => x.id_stato == eccStorno.id_stato).FirstOrDefault();
                    if (st != null)
                    {
                        g.StatoDescrizione = "Storno " + st.descrizione_stato;
                        g.Stato = eccStorno.id_stato;
                    }
                }
            }
            else // se non c'e nel DB
            {
                g.StatoDescrizione = " ";//No SelfService";
                g.Stato = -1;
            }

            return g;
        }

        public static CheckRipianificazione PuoStornareSenzaRipianificazione(int anno, string matricola)
        {
            CheckRipianificazione cp = new CheckRipianificazione();

            cp.FerieInserite = GetTotaleDaPianoFerie(anno, matricola);
            cp.FerieMinimo = (int)GetMinimoPianificabileFerie(anno);

            if (cp.FerieInserite <= cp.FerieMinimo)
            {
                cp.StornoPossibileSenzaRipianificazione = false;
                return cp;
            }
            cp.StorniNonRipianificati = GetStorniSenzaRipianificazione(anno, matricola);
            cp.StornoPossibileSenzaRipianificazione = (cp.StorniNonRipianificati < (cp.FerieInserite - cp.FerieMinimo));
            return cp;
        }

        public static float GetTotaleDaPianoFerie(int anno, string matricola)
        {
            WSDigigapp service = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            };
            var respFerie = ServiceWrapper.GetPianoFerieWrapped(service, matricola, "0101" + anno, 80, "");
            return respFerie.dipendente.ferie.ferieUsufruite + respFerie.dipendente.ferie.ferieRichieste + respFerie.dipendente.ferie.feriePianificate;


            var db = new digiGappEntities();
            var daPF = db.MyRai_Eccezioni_Richieste.Where(x =>
                 x.MyRai_Richieste.matricola_richiesta == matricola &&
                 x.azione == "I" &&
                 x.cod_eccezione == "FE" &&
                 x.data_eccezione.Year == anno &&
                 x.id_stato == 20 &&
                 x.MyRai_Richieste.da_pianoferie == true)
            .ToList();

            var pfbatch = db.MyRai_PianoFerieBatch.Where(x => x.matricola == matricola).ToList();
            int counter = 0;
            foreach (var ric in daPF)
            {
                var pf = pfbatch.Where(x => x.id_richiesta_db == ric.id_richiesta).FirstOrDefault();
                if (pf == null)
                {
                    counter++;
                }
                else
                {
                    if (pf.provenienza == null || !pf.provenienza.Contains("Ripianificato da"))
                    {
                        counter++;
                    }
                }
            }
            return counter;
        }
        public static MyRaiServiceInterface.it.rai.servizi.digigappws.Dipendente[]
            GetUlterioriDipendenti(string sede, string repartoReq, int anno,
            MyRaiServiceInterface.it.rai.servizi.digigappws.Dipendente[] MieiDipendenti,
            WSDigigapp wsService)
        {
            var db = new myRaiData.digiGappEntities();
            var PianoFeriePerGiroItalia = db.MyRai_PianoFerie.Where(x => x.sedegapp == sede.Substring(0, 5) + repartoReq &&
                                             x.anno == DateTime.Now.Year).ToList();
            MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerieSedeGapp resp8REC0 = null;
            foreach (var item in PianoFeriePerGiroItalia)
            {
                var sedePrecedente = HomeManager.GetSedeGappPrecedente(item.matricola, DateTime.Now);
                if (sedePrecedente != null) //è in tab
                {
                    var md = MieiDipendenti.Where(x => x.matricola == "0" + item.matricola).FirstOrDefault();
                    if (md != null)
                    {
                        //gia in list
                        continue;
                    }
                    if (resp8REC0 == null)
                    {
                        //  var respp = wsService.getPianoFerie(item.matricola, "01012021", 80, "");

                        resp8REC0 = wsService.getPianoFerieSedeGapp("P" + Utente.Matricola(),
                       "01" + "01" + anno, "", "", "8REC0", 75);
                    }
                    var dip = resp8REC0.dipendenti.Where(x => x.matricola == "0" + item.matricola).FirstOrDefault();
                    if (dip != null)
                    {
                        var list = MieiDipendenti.ToList();
                        list.Add(dip);
                        MieiDipendenti = list.OrderBy(x => x.matricola).ToArray();
                    }
                }
            }
            return MieiDipendenti;
        }
        public static int AllineaSediPF(PianoFerieApprovatoreModel model, int? anno)
        {
            var db = new digiGappEntities();
            string sede = model.SedeSelezionataCodice.Replace("-", "");
            int changed = 0;
            foreach (var dip in model.PianoFerieDipendenti)
            {
                var PFdipendente = db.MyRai_PianoFerie.Where(x => x.matricola == dip.Matricola && x.anno == anno).FirstOrDefault();
                if (PFdipendente == null)
                    continue;

                if (PFdipendente.sedegapp != sede)
                {
                    string oldsede = PFdipendente.sedegapp;
                    PFdipendente.sedegapp = sede;
                    PFdipendente.approvatore = null;
                    PFdipendente.data_approvato = null;
                    PFdipendente.Id_pdf_pianoferie_inclusa = null;
                    db.SaveChanges();
                    changed++;
                    Logger.LogAzione(new MyRai_LogAzioni()
                    {
                        operazione = "AllineamentoPF",
                        descrizione_operazione = PFdipendente.matricola + " sede modificata da " + oldsede + " a " + sede
                    });
                }
            }
            return changed;
        }

        public static int GetStorniSenzaRipianificazione(int anno, string matricola)
        {
            var db = new digiGappEntities();
            int st = db.MyRai_Eccezioni_Richieste.Where(x =>
                 x.MyRai_Richieste.matricola_richiesta == matricola &&
                 x.azione == "C" &&
                 x.cod_eccezione == "FE" &&
                 x.data_eccezione.Year == anno &&
                x.id_stato == 10 &&
                 !x.motivo_richiesta.Contains("(Ripianificazione su") &&
                 x.MyRai_Richieste.da_pianoferie == true)
                .Count();

            return st;
        }
        public static float GetMinimoPianificabileFerie(int anno)
        {
            CalendarioFerie model = GetCalendarioAnnuale(anno);
            MyRai_ArretratiExcel2019 IsOnAccordi = FeriePermessiManager.GetFileExcelInfo();
            GetpercentualiPF(model, anno);

            if (IsOnAccordi == null)
            {
                return model.PianoFerieDip.PF_InteroSuArretrati + model.PianoFerieDip.PF_InteroSuSpettanza;
            }
            else
            {
                CalendarioFerie modelPF = GetCalendarioAnnualPFmodel(anno);
                float FEaccordi = modelPF.FEperFoglioExcelArretrati;
                return model.PianoFerieDip.PF_InteroSuSpettanza + FEaccordi;
            }

        }

        public static Boolean UtenteProduzione(string sede = null)
        {
            return false;
            var db = new digiGappEntities();
            if (sede == null) sede = Utente.SedeGapp();

            var s = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sede).FirstOrDefault();
            if (s == null)
                return false;
            else
            {
                if (s.bypass_scadenze_pianoferie2020 == true)
                    return true;
                else
                    return false;
            }
        }


        public static CalendarioFerie GetCalendarioAnnuale(int anno)
        {
            CalendarioFerie model = new CalendarioFerie();
            pianoFerie response = GetPianoFerieAnno(anno, true);

            model.resocontoFerie = response.dipendente.ferie;
            string[] par = CommonManager.GetParametri<string>(EnumParametriSistema.OrariGiornateNoPianoFerie);

            string orariFree = par[0] != null ? par[0] : "";
            string giornateFree = par[1] != null ? par[1] : "";


            var riposi = response.dipendente.ferie.giornate.Where(x => orariFree.Split(',').Contains(x.orarioReale) || giornateFree.Split(',').Contains(x.tipoGiornata.Trim()));
            foreach (var r in riposi)
            {
                string data = r.dataTeorica.Trim().Replace("-", "/") + "/" + anno;
                DateTime D;
                if (DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D))
                {
                    model.DateToBeGray.Add(D);
                }
            }


            #region carica giorni calendario
            DateTime dataRif = DateTime.Now;
            DateTime currDate = DateTime.Now;
            model.DaysShowed = new List<CalendarioDay>();
            model.Anno = anno;
            MyRaiServiceInterface.it.rai.servizi.digigappws.Giornata giornata;
            string meseStr = "";
            string giornoStr = "";
            model.tipiGiornataSel = new List<TipoPermessoFerieUsato>();
            model.tipiGiornataSel = popolaTipiFerie(model);
            var prs = response.dipendente.ferie.giornate.Where(x => x.codiceVisualizzazione == "PR").ToList();
            if (prs.Any())
            {

                foreach (var pr in prs)
                {
                    FeriePermessiManager.GetGiornataForModel(pr);
                    //DateTime D;
                    //if (DateTime.TryParseExact(pr.dataTeorica + "-" + anno, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out D))
                    //{
                    //    pr.data = D;
                    //}
                }
            }
            for (int m = 1; m < 13; m++)
            {
                for (int d = 1; d <= DateTime.DaysInMonth(anno, m); d++)
                {
                    if (m == 12 && d == 7)
                    {

                    }
                    if (response.dipendente.ferie == null) continue;
                    dataRif = new DateTime(anno, m, d);
                    meseStr = dataRif.Month.ToString("00");
                    giornoStr = dataRif.Day.ToString("00");
                    giornata = response.dipendente.ferie.giornate.Where(x => x.dataTeorica == giornoStr + "-" + meseStr).FirstOrDefault();

                    if (giornata.data.CompareTo(DateTime.MinValue) > 0)
                    {

                        //  if (model.tipiGiornataSel.Find(f => getFriendlyCSS(f.sigla.ToUpper().Trim()) == getFriendlyCSS(giornata.codiceVisualizzazione.ToUpper().Trim())) == null)
                        //     model.tipiGiornataSel.Add(setTipoFeriePermesso(giornata.codiceVisualizzazione));
                        //    model.tipiGiornataSel.Add(new TipologiaGiornata { sigla = giornata.codiceVisualizzazione, siglaSemplice = getFriendlyCSS(giornata.codiceVisualizzazione.Trim()), tipoDesc = getPermessoDesc(giornata.codiceVisualizzazione) });

                        // se la data è valorizzata significa che è valorizzata la giornata come ferie, permessi ecc
                        model.DaysShowed.Add(new CalendarioDay
                        {
                            Frazione = setFrazione(giornata.codiceVisualizzazione, giornata.Frazione),
                            giorno = dataRif,
                            tipoFeriePermesso = giornata.codiceVisualizzazione,
                            tipoGiornata = giornata.tipoGiornata,
                            friendly = getFriendlyCSS(giornata.codiceVisualizzazione.Trim())
                        });

                    }
                    else
                    {
                        model.DaysShowed.Add(
                                new CalendarioDay
                                {
                                    Frazione = "",
                                    giorno = dataRif,
                                    tipoGiornata = giornata.tipoGiornata,
                                    tipoFeriePermesso = "",
                                    friendly = ""
                                }
                            );
                    }

                }
            }
            #endregion

            return model;
        }



        private static List<TipoPermessoFerieUsato> popolaTipiFerie(CalendarioFerie model)
        {
            List<TipoPermessoFerieUsato> res = new List<TipoPermessoFerieUsato>();
            FerieDipendente verif = null;
            TipoPermessoFerieUsato t;
            bool check = false;

            if (model != null && model.resocontoFerie != null && model.resocontoFerie.visualizzaPermessi)
            {
                #region permessi retribuiti
                t = setTipoFeriePermesso("PR");
                check = false;
                check = t.resoconto.Pianificate > 0;
                check = !check ? t.resoconto.Residue > 0 : check;
                check = !check ? t.resoconto.Spettanti > 0 : check;
                check = !check ? t.resoconto.Usufruite > 0 : check;
                check = !check ? t.resoconto.AnnoPrec > 0 : check;
                if (check)
                    res.Add(t);
                #endregion
            }

            if (model != null && model.resocontoFerie != null && model.resocontoFerie.visualizzaFC)
            {
                #region Permessi Retribuiti (ex F.Naz.)
                t = new TipoPermessoFerieUsato();
                t = setTipoFeriePermesso("PF");
                check = false;
                check = t.resoconto.Pianificate > 0;
                check = !check ? t.resoconto.Residue > 0 : check;
                check = !check ? t.resoconto.Spettanti > 0 : check;
                check = !check ? t.resoconto.Usufruite > 0 : check;
                check = !check ? t.resoconto.AnnoPrec > 0 : check;
                if (check)
                    res.Add(t);
                #endregion
            }


            #region Ferie
            if (model != null && model.resocontoFerie != null && model.resocontoFerie.visualizzaFerie)
            {
                t = new TipoPermessoFerieUsato();
                t = setTipoFeriePermesso("FE");
                check = false;
                check = t.resoconto.Pianificate > 0;
                check = !check ? t.resoconto.Residue > 0 : check;
                check = !check ? t.resoconto.Spettanti > 0 : check;
                check = !check ? t.resoconto.Usufruite > 0 : check;
                check = !check ? t.resoconto.AnnoPrec > 0 : check;
                if (check)
                    res.Add(t);
            }
            #endregion


            if (model != null && model.resocontoFerie != null && model.resocontoFerie.visualizzaPermessiGiornalisti)
            {
                #region Permessi Giornalisti
                t = new TipoPermessoFerieUsato();
                t = setTipoFeriePermesso("PX");
                check = false;
                check = t.resoconto.Pianificate > 0;
                check = !check ? t.resoconto.Residue > 0 : check;
                check = !check ? t.resoconto.Spettanti > 0 : check;
                check = !check ? t.resoconto.Usufruite > 0 : check;
                check = !check ? t.resoconto.AnnoPrec > 0 : check;
                if (check)
                    res.Add(t);
                #endregion

                #region permessi giornalisti extra
                t = new TipoPermessoFerieUsato();
                t = setTipoFeriePermesso("PXC");
                check = false;
                check = t.resoconto.Pianificate > 0;
                check = !check ? t.resoconto.Residue > 0 : check;
                check = !check ? t.resoconto.Spettanti > 0 : check;
                check = !check ? t.resoconto.Usufruite > 0 : check;
                check = !check ? t.resoconto.AnnoPrec > 0 : check;
                if (check)
                    res.Add(t);
                #endregion
            }

            #region Recuperi Non Lavorati (Vedi mancati non lavorati)
            if (model != null && model.resocontoFerie != null && model.resocontoFerie.visualizzaRecuperoNonLavorati)
            {
                t = new TipoPermessoFerieUsato();
                t = setTipoFeriePermesso("RN");
                check = false;
                check = t.resoconto.Pianificate > 0;
                check = !check ? t.resoconto.Residue > 0 : check;
                check = !check ? t.resoconto.Spettanti > 0 : check;
                check = !check ? t.resoconto.Usufruite > 0 : check;
                check = !check ? t.resoconto.AnnoPrec > 0 : check;
                if (check)
                    res.Add(t);
            }
            #endregion
            #region  Recuperi Riposi (Vedi mancati riposi)
            if (model != null && model.resocontoFerie != null && model.resocontoFerie.visualizzaRecuperoRiposi)
            {
                t = new TipoPermessoFerieUsato();
                t = setTipoFeriePermesso("RR");
                check = false;
                check = t.resoconto.Pianificate > 0;
                check = !check ? t.resoconto.Residue > 0 : check;
                check = !check ? t.resoconto.Spettanti > 0 : check;
                check = !check ? t.resoconto.Usufruite > 0 : check;
                check = !check ? t.resoconto.AnnoPrec > 0 : check;
                if (check)
                    res.Add(t);
            }
            #endregion
            #region   Recuperi Festivi (vedi mancati festivi)
            if (model != null && model.resocontoFerie != null && model.resocontoFerie.visualizzaRecuperoFestivi)
            {
                t = new TipoPermessoFerieUsato();
                t = setTipoFeriePermesso("RF");
                check = false;
                check = t.resoconto.Pianificate > 0;
                check = !check ? t.resoconto.Residue > 0 : check;
                check = !check ? t.resoconto.Spettanti > 0 : check;
                check = !check ? t.resoconto.Usufruite > 0 : check;
                check = !check ? t.resoconto.AnnoPrec > 0 : check;
                if (check)
                    res.Add(t);
            }
            #endregion

            return res;
        }
        public static TipoPermessoFerieUsato setTipoFeriePermesso(string codiceVis)
        {
            TipoPermessoFerieUsato res = new TipoPermessoFerieUsato();

            res.sigla = codiceVis;
            res.siglaSemplice = getFriendlyCSS(codiceVis);
            res.tipoDesc = getPermessoDesc(codiceVis);

            res.resoconto = new FerieDipendente();
            res.resoconto.AnnoPrec = 0;

            res.resoconto.Pianificate = 0;
            res.resoconto.Residue = 0;
            res.resoconto.Spettanti = 0;
            res.resoconto.Usufruite = 0;
            switch (codiceVis.ToUpper().Trim())
            {
                case "FE":
                    res.resoconto = GetFerie();
                    break;
                case "PX":
                    res.resoconto = GetPermessiGiornalisti();
                    break;
                case "PXC":
                    res.resoconto = GetPermessiGiornalistiCompensativi();
                    break;
                case "MN":
                    res.resoconto = GetMancatiNonLavorati();
                    break;
                case "RN":
                    res.resoconto = GetMancatiNonLavorati();
                    break;
                case "MR":
                    res.resoconto = GetMancatiRiposi();
                    break;
                case "RR":
                    res.resoconto = GetMancatiRiposi();
                    break;
                case "MF":
                    res.resoconto = GetMancatiFestivi();
                    break;
                case "RF":
                    res.resoconto = GetMancatiFestivi();
                    break;
                case "PR":
                    res.resoconto = GetPermessiRetr();
                    break;
                case "PF":
                    res.resoconto = GetPermessiExFest();
                    break;
            }
            return res;
        }
        private static string getFriendlyCSS(string codiceVisualizzazione)
        {
            string res = "";
            switch (codiceVisualizzazione.ToUpper())
            {
                case "PX":
                    res = "pg";
                    break;
                case "PF":
                    res = "pf";
                    break;
                case "PXC":
                    res = "pg";
                    break;
                case "MN":
                    res = "mn";
                    break;
                case "RN":
                    res = "mn";
                    break;
                case "MR":
                    res = "mr";
                    break;
                case "RR":
                    res = "mr";
                    break;
                case "MF":
                    res = "mf";
                    break;
                case "RF":
                    res = "mf";
                    break;
                default:
                    res = codiceVisualizzazione;
                    break;
            }
            return res;
        }
        private static string setFrazione(string codiceVisualizzazione, string frazione)
        {
            string res = "";
            string frazioneRes = "";

            frazioneRes = frazione == null ? "" : frazione;

            switch (codiceVisualizzazione.ToUpper())
            {
                case "MN":
                    res = "Intera giornata";
                    break;
                case "RN":
                    res = "Intera giornata";
                    break;
                case "MF":
                    res = "Intera giornata";
                    break;
                case "RF":
                    res = "Intera giornata";
                    break;
                case "MR":
                    res = "Intera giornata";
                    break;
                case "RR":
                    res = "Intera giornata";
                    break;
                default:
                    res = frazioneRes;
                    break;
            }
            return res;
        }
        private static string getPermessoDesc(string codice)
        {
            string res = "";
            switch (codice.ToUpper().Trim())
            {
                case "PX":
                    res = "Permesso Giornalisti";
                    break;
                case "PXC":
                    res = "Permesso Giornalisti";
                    break;
                case "MN":
                    res = "Mancati Non Lavorati";
                    break;
                case "RN":
                    res = "Recupero Non Lavorati";
                    break;
                case "MR":
                    res = "Mancati Riposi";
                    break;
                case "RR":
                    res = "Recupero Riposi";
                    break;
                case "MF":
                    res = "Mancati Festivi";
                    break;
                case "RF":
                    res = "Recupero Festivi";
                    break;
                case "FE":
                    res = "Ferie";
                    break;
                case "PF":
                    res = "Extra Festività";
                    break;

                default:
                    res = "Permesso";
                    break;
            }
            return res;
        }

        public static CalendarioFerie GetCalendario(int? AnnoRichiesto, int? MeseRichiesto)
        {
            CalendarioFerie model = new CalendarioFerie();
            DateTime D;
            if (AnnoRichiesto == null && MeseRichiesto == null)
                D = DateTime.Now;
            else
                D = new DateTime((int)AnnoRichiesto, (int)MeseRichiesto, 1);


            model.MeseCorrente = D.ToString("MMMM yyyy");
            model.ShowPreviousButton = (D.Month > 1);
            model.ShowNextButton = (D.Month < 12);
            model.Anno = D.Year;
            model.Mese = D.Month;
            model.DaysShowed = NormalizzaMese(model.Mese, model.Anno);

            model.MeseNext = D.AddMonths(1).Month;
            model.AnnoNext = D.AddMonths(1).Year;
            model.MesePrev = D.AddMonths(-1).Month;
            model.AnnoPrev = D.AddMonths(-1).Year;

            pianoFerie response = GetPianoFerieAnno(DateTime.Now.Year);


            FerieDipendente modelPR = GetFeriePermessiGeneric(response, EnumPrefixFeriePermessi.permessi,
                   EnumCodiceVisualizzazioneFeriePermessi.PR, true);
            FerieDipendente modelPF = GetFeriePermessiGeneric(response, EnumPrefixFeriePermessi.exFestivita,
                EnumCodiceVisualizzazioneFeriePermessi.PF, true);
            FerieDipendente modelFE = GetFeriePermessiGeneric(response, EnumPrefixFeriePermessi.ferie,
                EnumCodiceVisualizzazioneFeriePermessi.FE, true);

            FerieDipendente modelPX = GetFeriePermessiGeneric(response, EnumPrefixFeriePermessi.permessiGiornalisti,
                      EnumCodiceVisualizzazioneFeriePermessi.PX);

            FerieDipendente modelNL = GetMancatiNonLavorati();

            FerieDipendente modelMR = GetMancatiRiposi();
            FerieDipendente modelMF = GetMancatiFestivi();

            foreach (var day in model.DaysShowed)
            {
                var found = modelPR.GiorniFerie.Where(x => x.data == day.giorno).FirstOrDefault();
                if (found != null)
                {
                    day.Frazione = found.Frazione;
                    day.tipoFeriePermesso = "PR";
                }
                found = modelPF.GiorniFerie.Where(x => x.data == day.giorno).FirstOrDefault();
                if (found != null)
                {
                    day.Frazione = found.Frazione;
                    day.tipoFeriePermesso = "PF";
                }
                found = modelFE.GiorniFerie.Where(x => x.data == day.giorno).FirstOrDefault();
                if (found != null)
                {
                    day.Frazione = found.Frazione;
                    day.tipoFeriePermesso = "FE";
                }
                found = modelPX.GiorniFerie.Where(x => x.data == day.giorno).FirstOrDefault();
                if (found != null)
                {
                    day.Frazione = found.Frazione;
                    day.tipoFeriePermesso = "PG";
                }
                found = modelNL.GiorniFerie.Where(x => x.data == day.giorno).FirstOrDefault();
                if (found != null)
                {
                    day.Frazione = "Intera giornata";
                    day.tipoFeriePermesso = "MN";
                }
                found = modelMR.GiorniFerie.Where(x => x.data == day.giorno).FirstOrDefault();
                if (found != null)
                {
                    day.Frazione = "Intera giornata";
                    day.tipoFeriePermesso = "MR";
                }
                found = modelMF.GiorniFerie.Where(x => x.data == day.giorno).FirstOrDefault();
                if (found != null)
                {
                    day.Frazione = "Intera giornata";
                    day.tipoFeriePermesso = "MF";
                }
            }

            return model;
        }
        public static int GetContatoriSingolaEcc(string matricola, int anno, string ecc)
        {
            MyRaiService1Client cl = new MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            GetContatoriEccezioniResponse resp = new GetContatoriEccezioniResponse();

            try
            {
                resp = cl.GetContatoriEccezioni(matricola,
                                                new DateTime(anno, 1, 1),
                                                new DateTime(anno, 12, 31),
                                                new string[] { ecc });
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "GetContatoriSingolaEcc"
                });
                return 0;
            }

            int mr = 0;

            if (resp == null || resp.ContatoriEccezioni == null || !resp.ContatoriEccezioni.Any())
                return mr;
            else
                int.TryParse(resp.ContatoriEccezioni[0].Totale, out mr);

            return mr;
        }


        public static CalendarioFerie GetCalendarioSituazioneEccezioni(int? anno, int? mese)
        {
            CalendarioFerie model = new CalendarioFerie();
            DateTime dt;
            if (anno == null && mese == null)
                dt = DateTime.Now;
            else
                dt = new DateTime((int)anno, (int)mese, 1);

            DateTime DataLimiteLeft = new DateTime(DateTime.Now.Year, 1, 1);
            DateTime DataLimiteRight = new DateTime(DateTime.Now.Year + 1, 12, 1);


            model.MeseCorrente = dt.ToString("MMMM yyyy");
            model.ShowPreviousButton = dt > DataLimiteLeft;// (dt.Month > 1 && dt.Year == DateTime.Now.Year || dt.Year > DateTime.Now.Year);
            model.ShowNextButton = dt < DataLimiteRight;// (dt.Year < DateTime.Now.Year + 1 || (dt.Year == DateTime.Now.Year + 1 && dt.Month < 12));
            model.Anno = dt.Year;
            model.Mese = dt.Month;
            model.DaysShowed = NormalizzaMese(model.Mese, model.Anno);

            model.MeseNext = dt.AddMonths(1).Month;
            model.AnnoNext = dt.AddMonths(1).Year;
            model.MesePrev = dt.AddMonths(-1).Month;
            model.AnnoPrev = dt.AddMonths(-1).Year;

            string matricola = CommonManager.GetCurrentUserMatricola();

            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client client = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

            int lastDay = DateTime.DaysInMonth(dt.Year, dt.Month);
            DateTime dtStop = new DateTime(dt.Year, dt.Month, lastDay);

            var risposta = client.GetStatoEccezioniGiornate(matricola, dt, dtStop);

            if (risposta.Esito)
            {
                model.Giornate = risposta.StatoEccezioniGiornate.ToList();
            }

            if (dtStop.Date > DateTime.Now.Date)
            {
                dtStop = DateTime.Now;
                dtStop = dtStop.AddDays(-1);
            }

            model.GiornateDaEvidenziare = ScrivaniaManager.GetGiornateInEvidenzaPerCalendario(dtStop);

            return model;
        }

        public static List<CalendarioDay> NormalizzaMese(int mese, int anno)
        {
            DateTime primo = new DateTime(anno, mese, 1);
            DateTime mese1 = new DateTime(anno, mese, 1);

            while (primo.DayOfWeek != DayOfWeek.Monday)
            {
                primo = primo.AddDays(-1);
            }
            DateTime ultimo = mese1.AddMonths(1).AddDays(-1);
            while (ultimo.DayOfWeek != DayOfWeek.Sunday)
            {
                ultimo = ultimo.AddDays(1);
            }
            List<CalendarioDay> L = new List<CalendarioDay>();
            while (primo <= ultimo)
            {
                L.Add(new CalendarioDay() { giorno = primo, isCurrentMonth = primo.Month == mese });
                primo = primo.AddDays(1);
            }
            return L;

        }

        public static GraficiFerieModel GetGraficiFerieModel()
        {
            pianoFerie response = GetPianoFerieAnno(DateTime.Now.Year);


            FerieDipendente modelPR = GetFeriePermessiGeneric(response, EnumPrefixFeriePermessi.permessi,
                EnumCodiceVisualizzazioneFeriePermessi.PR, true);
            FerieDipendente modelPF = GetFeriePermessiGeneric(response, EnumPrefixFeriePermessi.exFestivita,
                EnumCodiceVisualizzazioneFeriePermessi.PF, true);
            FerieDipendente modelFE = GetFeriePermessiGeneric(response, EnumPrefixFeriePermessi.ferie,
                EnumCodiceVisualizzazioneFeriePermessi.FE, true);

            GraficiFerieModel model = new GraficiFerieModel();
            model.PianoFerie = response;

            ModelDash m = new ModelDash();
            m = ScrivaniaManager.GetTotaliEvidenze(m);
            string ai = CommonManager.GetParametro<string>(EnumParametriSistema.IgnoraAssenzeIngiustificatePerMatricole);
            model.HaAssenzeIngiustificate = m.TotaleEvidenzeDaGiustificareSoloAssIng > 0 && !(ai != null && ai.ToUpper().Contains(CommonManager.GetCurrentUserMatricola().ToUpper()));
            model.HaGiorniCarenza = Utente.GiornateConCarenza() != null;

            return model;
        }

        public static FeriePermessiModel GetFeriePermessiModel()
        {
            FeriePermessiModel model = new FeriePermessiModel();
            model.menuSidebar = Utente.getSidebarModel();// new sidebarModel(3);


            pianoFerie response = GetPianoFerieAnno(DateTime.Now.Year, true);
            //HttpContext.Current.Session["pianoferie"] = response;
            SessionManager.Set(SessionVariables.PianoFerie, response);
            model.pianoFerieModel = response;

            model.Raggruppamenti = HomeManager.GetRaggruppamenti();


            return model;
        }

        public static FerieDipendente GetPermessiGiornalisti()
        {
            pianoFerie response = GetPianoFerieAnno(DateTime.Now.Year);
            FerieDipendente f = GetFeriePermessiGeneric(response, EnumPrefixFeriePermessi.permessiGiornalisti,
                        EnumCodiceVisualizzazioneFeriePermessi.PX);

            f = SetYear(f);
            return f;
        }

        public static FerieDipendente GetPermessiGiornalistiCompensativi()
        {
            pianoFerie response = GetPianoFerieAnno(DateTime.Now.Year);
            FerieDipendente f = GetFeriePermessiGeneric(response, EnumPrefixFeriePermessi.permessiGiornalistiCompensativi,
                        EnumCodiceVisualizzazioneFeriePermessi.PXC);

            f = SetYear(f);
            return f;
        }

        public static GetContatoriEccezioniResponse GetContatoriEccezioni(List<string> ecc = null)
        {
            MyRaiService1Client wcf1 = new MyRaiService1Client();

            if (ecc == null)
            {
                return null;
            }

            GetContatoriEccezioniResponse ri = wcf1.GetContatoriEccezioni(CommonManager.GetCurrentUserMatricola(), new DateTime(DateTime.Now.Year, 1, 1), DateTime.Today, ecc.ToArray());

            return ri;
        }

        public static CalendarioFerie GetCalendarioAnnualPFmodel(int anno)
        {
            var db = new myRaiData.digiGappEntities();

            CalendarioFerie model = new CalendarioFerie();



            model = FeriePermessiManager.GetCalendarioAnnuale(anno);

            if (CommonHelper.IsMazzini())
            {
                model.IsMazziniDatesCoveredByC1 = FeriePermessiManager.IsMazziniOK1();
                //model.IsMazziniDatesCoveredByC2 = FeriePermessiManager.IsMazziniOK2();
                WSDigigapp service = new WSDigigapp()
                {
                    Credentials = CommonHelper.GetUtenteServizioCredentials()
                };
                var resp1 = service.getEccezioni(CommonManager.GetCurrentUserMatricola(), "2806" + DateTime.Now.Year, "BU", 80);
               // var resp2 = service.getEccezioni(CommonManager.GetCurrentUserMatricola(), "0712" + DateTime.Now.Year, "BU", 80);
                if (resp1.orario != null)
                    model.CodiceOrario1 = resp1.orario.cod_orario;

                //if (resp2.orario !=null)
                 //  model.CodiceOrario2 = resp2.orario.cod_orario;
            }
            if (Utente.IsTorinoCavalli())
            {
                model.IsTorinoCavalliCoveredByC = FeriePermessiManager.IsTorinoCavalliOK();
                WSDigigapp service = new WSDigigapp()
                {
                    Credentials = CommonHelper.GetUtenteServizioCredentials()
                };
                var resp1 = service.getEccezioni(CommonManager.GetCurrentUserMatricola(), "2506" + DateTime.Now.Year, "BU", 80);
                if (resp1.orario != null)
                    model.CodiceOrarioTorinoCavalli1 = resp1.orario.cod_orario;
            }



            model.ItemFoglioExcelArretrati = FeriePermessiManager.GetFileExcelInfo();
            model.NonPartecipantiRaggiungimentoPercheFruiteMarzo = 0;
            if (model.ItemFoglioExcelArretrati != null)
            {
                float ArretratiDaMettereDaFoglioExcel =
                    (int)model.ItemFoglioExcelArretrati.da_fare;// - (int)model.ItemFoglioExcelArretrati.fruite_11_31;

                ArretratiDaMettereDaFoglioExcel -= myRaiCommonTasks.Arretrati2019.GetFECE_MNCE_MRCE_MenoDonateSuFoglioExcel(CommonManager.GetCurrentUserMatricola());

                if (ArretratiDaMettereDaFoglioExcel > 0)
                {
                    if (Utente.TipoDipendente() == "D")
                    {
                        model.resocontoFerie.mancatiRiposiAnniPrecedenti = 0;
                        model.resocontoFerie.mancatiFestiviAnniPrecedenti = 0;
                    }
                    float[] valori;
                    if (model.HaEstensioneMarzo)
                    {
                        valori = myRaiCommonTasks.Arretrati2019.GetGiornalisti_RR_RF_FE(
                                                model.resocontoFerie.mancatiRiposiAnniPrecedenti,
                                                model.resocontoFerie.mancatiFestiviAnniPrecedenti,
                                                model.resocontoFerie.ferieAnniPrecedenti,
                                                ArretratiDaMettereDaFoglioExcel,
                                                CommonManager.GetCurrentUserMatricola());
                        if (valori[0] > 0)
                        {
                            int MRanno = FeriePermessiManager.GetContatoriSingolaEcc(CommonManager.GetCurrentUserMatricola(),
                                                anno, "MR");
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
                        valori = myRaiCommonTasks.Arretrati2019.GetRR_RF_FE(
                                                 model.resocontoFerie.mancatiRiposiAnniPrecedenti,
                                                 model.resocontoFerie.mancatiFestiviAnniPrecedenti,
                                                 ArretratiDaMettereDaFoglioExcel,
                                                CommonManager.GetCurrentUserMatricola());
                    }


                    model.RRperFoglioExcelArretrati = valori[0];
                    model.RFperFoglioExcelArretrati = valori[1];
                    model.FEperFoglioExcelArretrati = valori[2];
                    model.TOTALEperFoglioExcelArretrati = valori[0] + valori[1] + valori[2];
                }
            }


            model.DataChiusuraPianoFerie = db.MyRai_PianoFerieDefinizioni.Where(x => x.anno == anno).Select(x => x.data_chiusura).FirstOrDefault();
            string rep = myRai.Models.Utente.Reparto();
            if (String.IsNullOrWhiteSpace(rep) || rep.Trim() == "0" || rep.Trim() == "00") rep = null;

            string mysede = myRai.Models.Utente.SedeGapp(DateTime.Now) + rep;
            DateTime Dapp = new DateTime(DateTime.Now.Year, 1, 1);
            string matr = CommonManager.GetCurrentUserMatricola();

            model.DataApprovazionePfDipendente = db.MyRai_PianoFerie.Where(x => x.anno==anno && x.matricola == matr)
                .Select(x => x.data_approvato).FirstOrDefault();



            



            var rowspf = db.MyRai_PianoFerieSedi.Where(a => a.sedegapp == mysede && a.data_approvata != null &&
                         a.data_approvata > Dapp).OrderByDescending(x => x.numero_versione).ToList();

            if (rowspf.Count() == 0)
            {
                model.DataApprovazioneSedeDipendente = null;
                model.DataFirmaSedeDipendente = null;
            }
            else
            {
                model.DataApprovazioneSedeDipendente = rowspf.Select(x => x.data_approvata).FirstOrDefault();
                if (model.DataApprovazioneSedeDipendente != null)
                {
                    string ma = rowspf.Select(x => x.matricola_approvatore).FirstOrDefault();
                    if (!String.IsNullOrWhiteSpace(ma))
                        model.ApprovatoreSedeDipendente = CommonManager.GetNominativoPerMatricola(ma);
                }


                model.DataFirmaSedeDipendente = rowspf.Select(x => x.data_firma).FirstOrDefault();
                if (model.DataFirmaSedeDipendente != null)
                {
                    string mf = rowspf.Select(x => x.matricola_firma).FirstOrDefault();
                    if (!String.IsNullOrWhiteSpace(mf))
                        model.ConvalidatoreSedeDipendente = CommonManager.GetNominativoPerMatricola(mf);
                }

            }
            var mypf = db.MyRai_PianoFerie.Where(x => x.anno == anno && x.matricola == matr)
                        .FirstOrDefault();
            myRaiData.MyRai_PianoFerieSedi mypfFirmato = null;
            if (mypf != null && mypf.Id_pdf_pianoferie_inclusa != null)
            {
                mypfFirmato = db.MyRai_PianoFerieSedi.Where(x => x.id == mypf.Id_pdf_pianoferie_inclusa && x.data_firma != null).FirstOrDefault();
            }
            if (mypfFirmato != null)
            {
                model.DataFirmaSedeDipendente = mypfFirmato.data_firma;
            }
            else
                model.DataFirmaSedeDipendente = null;


            FeriePermessiControllerScope.Instance.giorniCalendario = new List<CalendarioDay>();
            FeriePermessiControllerScope.Instance.giorniCalendario = model.DaysShowed;
            FeriePermessiControllerScope.Instance.anno = anno;
            FeriePermessiControllerScope.Instance.tipiGiornata = new List<TipoPermessoFerieUsato>();
            FeriePermessiControllerScope.Instance.tipiGiornata = model.tipiGiornataSel;

            float ArretratiCumulatiFE_RR_RF =
          model.resocontoFerie.ferieAnniPrecedenti +
          model.resocontoFerie.mancatiRiposiAnniPrecedenti +
          model.resocontoFerie.mancatiFestiviAnniPrecedenti;



            GetpercentualiPF(model, anno);
            GetGiorniPianoFerieSalvati(model, anno);
            GetStatoPianoFerie(model, anno);

            

            model.DateRN = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matr && x.data.Year == anno && x.eccezione == "RN").Select(x => x.data).ToList();
            model.DateRR = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matr && x.data.Year == anno && x.eccezione == "RR").Select(x => x.data).ToList();
            model.DateRF = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matr && x.data.Year == anno && x.eccezione == "RF").Select(x => x.data).ToList();
            model.DatePF = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matr && x.data.Year == anno && x.eccezione == "PF").Select(x => x.data).ToList();
            model.DatePR = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matr && x.data.Year == anno && x.eccezione == "PR").Select(x => x.data).ToList();
            model.DatePRX = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matr && x.data.Year == anno && x.eccezione == "PX").Select(x => x.data).ToList();

            model.DateFEM_FEP = new List<DateTime>() {
                new DateTime (anno,11,2),
                FestivitaManager.GetPasqua(anno).AddDays(-2)
            };

            var px = model.tipiGiornataSel.Where(x => x.sigla == "PX").FirstOrDefault();
            if (px != null)
                model.DatePX = px.resoconto.GiorniFerie.Select(x => x.data).ToList();
            else
                model.DatePX = new List<DateTime>();

            //model.resocontoFerie.recuperiMancatiRiposiRimanenti += model.resocontoFerie.recuperiMancatiRiposiUsufruiti;
            model.resocontoFerie.recuperiMancatiRiposiRimanenti = model.resocontoFerie.mancatiRiposiAnniPrecedenti + model.resocontoFerie.mancatiRiposiSpettanti;


            //model.resocontoFerie.recuperiNonLavoratiRimanenti += model.resocontoFerie.recuperiNonLavoratiUsufruiti;
            model.resocontoFerie.recuperiNonLavoratiRimanenti = model.resocontoFerie.mancatiNonLavoratiAnniPrecedenti + model.resocontoFerie.mancatiNonLavoratiSpettanti;

            ArretratiHRDW_normalized A = FeriePermessiManager.GetArretratiHRDW(
                CommonManager.GetCurrentUserMatricola(), model.resocontoFerie.ferieAnniPrecedenti);

            if (Utente.TipoDipendente() == "G")
            {
                model.ArretratiEntroMarzoG = (int)A.GG_competenza_AP_giorn;
                model.ArretratiEntroDicembreG = (int)A.GG_ARR_da_smaltire_giorn;
            }
            else
            {
                model.ArretratiFE15 = (int)A.ARR_FE_da_smaltire;
                model.ArretratiRN15 = (int)A.ARR_MN_da_smaltire;
                model.ArretratiRR15 = (int)A.ARR_MR_da_smaltire;
                model.ArretratiRRRN15 = (int)A.GG_ARR_da_smaltire_giorn; ;// model.ArretratiRR15 + model.ArretratiRN15;
            }

            //if (CommonManager.IsProduzione() == false)//tbdmax
            //{
            //    model.ArretratiEntroMarzoG = 5;
            //    model.ArretratiEntroDicembreG = 16;
            //}
            //else
            //{
            //    model.ArretratiRR15 = (int)((model.resocontoFerie.mancatiRiposiAnniPrecedenti * 0.15) + 0.5);
            //    model.ArretratiRN15 = (int)((model.resocontoFerie.mancatiNonLavoratiAnniPrecedenti * 0.15) + 0.5);
            //    model.ArretratiFE15 = (int)((model.resocontoFerie.ferieAnniPrecedenti * 0.15) + 0.5);
            //    model.ArretratiRRRN15 = (int)(((model.resocontoFerie.mancatiRiposiAnniPrecedenti + model.resocontoFerie.mancatiNonLavoratiAnniPrecedenti) * 0.15) + 0.5);
            //}
         

            model.resocontoFerie.recuperiMancatiFestiviRimanenti = model.resocontoFerie.mancatiFestiviAnniPrecedenti + model.resocontoFerie.mancatiFestiviSpettanti;

            var def = db.MyRai_PianoFerieDefinizioni.Where(x => x.anno == anno).FirstOrDefault();


            int ArretratiInPercentuale = (int)((ArretratiCumulatiFE_RR_RF * (float)model.PianoFerieDip.PF_PercentualeSuArretrati / (float)100));
            model.PianoFerieDip.ArretratiCumulativiPercentuale = ArretratiInPercentuale;



            //int c1 = model.PianoFerieDip.GiorniPianoFerie.Where(x => x.data <= def.data_arretrati_soglia1 && (x.eccezione == "FE" || x.eccezione == "RR" || x.eccezione == "RN")).Count() +
            //         model.DaysShowed.Where(x => (x.tipoFeriePermesso == "FE" || x.tipoFeriePermesso == "RR" || x.tipoFeriePermesso == "RN") && x.giorno <= def.data_arretrati_soglia1).Count();

            //int c2 = model.PianoFerieDip.GiorniPianoFerie.Where(x => x.data <= def.data_arretrati_soglia2 && (x.eccezione == "FE" || x.eccezione == "RR" || x.eccezione == "RN")).Count() +
            //         model.DaysShowed.Where(x => (x.tipoFeriePermesso == "FE" || x.tipoFeriePermesso == "RR" || x.tipoFeriePermesso == "RN") && x.giorno <= def.data_arretrati_soglia2).Count();

            //      int c3 = model.PianoFerieDip.GiorniPianoFerie.Where(x => x.data <= new DateTime(anno, 12, 31) && (x.eccezione == "FE" || x.eccezione == "RR" || x.eccezione == "RN")).Count() +



            //               model.DaysShowed.Where(x => (x.tipoFeriePermesso == "FE" || x.tipoFeriePermesso == "RR" || x.tipoFeriePermesso == "RN") && x.giorno <= new DateTime(anno, 12, 31)).Count();


            model.PianoFerieDip.ArretratiSoglia1_Ok = true;
            model.PianoFerieDip.ArretratiSoglia2_Ok = true;
            model.PianoFerieDip.ArretratiSoglia3_Ok = true;

            //if (ArretratiInPercentuale <= 6)
            //{
            //    model.PianoFerieDip.ArretratiSoglia1_Ok = c1 >= ArretratiInPercentuale;
            //    model.PianoFerieDip.ArretratiSoglia2_Ok = true;
            //    model.PianoFerieDip.ArretratiSoglia3_Ok = true;
            //}
            //else if (ArretratiInPercentuale > 6 && ArretratiInPercentuale <= 10)
            //{
            //    model.PianoFerieDip.ArretratiSoglia1_Ok = c1 >= ArretratiInPercentuale;
            //    model.PianoFerieDip.ArretratiSoglia2_Ok = c2 >= ArretratiInPercentuale;
            //    model.PianoFerieDip.ArretratiSoglia3_Ok = true;
            //}
            //else if (ArretratiInPercentuale > 10 && ArretratiInPercentuale <= 30)
            //{
            //    model.PianoFerieDip.ArretratiSoglia1_Ok = c1 >= ArretratiInPercentuale;
            //    model.PianoFerieDip.ArretratiSoglia2_Ok = c2 >= ArretratiInPercentuale;
            //    int arrGiugnoInPoiDaAssegnare = ArretratiInPercentuale - 10;

            //    for (int mese = 6; mese <= 12; mese++)
            //    {
            //        int AssegnatiMese = model.PianoFerieDip.GiorniPianoFerie.Where(x => x.data.Month == mese && (x.eccezione == "FE" || x.eccezione == "RR" || x.eccezione == "RN")).Count() +
            //                            model.DaysShowed.Where(x => (x.tipoFeriePermesso == "FE" || x.tipoFeriePermesso == "RR" || x.tipoFeriePermesso == "RN") && x.giorno.Month == mese).Count();
            //        if (arrGiugnoInPoiDaAssegnare > 0 && AssegnatiMese == 0 || arrGiugnoInPoiDaAssegnare > 1 && AssegnatiMese <= 1)
            //        {
            //            model.PianoFerieDip.ArretratiSoglia3_Ok = false;
            //            break;
            //        }
            //        else
            //        {
            //            arrGiugnoInPoiDaAssegnare -= AssegnatiMese;
            //            if (arrGiugnoInPoiDaAssegnare < 0) arrGiugnoInPoiDaAssegnare = 0;
            //        }
            //        if (arrGiugnoInPoiDaAssegnare == 0)
            //        {
            //            model.PianoFerieDip.ArretratiSoglia3_Ok = true;
            //            break;
            //        }
            //    }
            //    if (arrGiugnoInPoiDaAssegnare == 0)
            //    {
            //        model.PianoFerieDip.ArretratiSoglia3_Ok = true;
            //    }
            //    else
            //    {
            //        model.PianoFerieDip.ArretratiSoglia3_Ok = false;
            //    }

            //}
            model.PianoFerieDip.Definizioni = def;

            string datasoglia = CommonManager.GetParametro<string>(EnumParametriSistema.SogliaModificaPF);
            DateTime Dsog;
            DateTime.TryParseExact(datasoglia, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out Dsog);

            model.OltreSogliaModifica = DateTime.Now > Dsog;

            string sediNoVincoli = CommonManager.GetParametro<string>(EnumParametriSistema.SediNoVincoliPF);
            if (!String.IsNullOrWhiteSpace(sediNoVincoli))
            {
                model.IsSedeSenzaVincoli = sediNoVincoli.Split(',').Contains(Utente.SedeGapp());
            }

            return model;
        }

        public static void GetpercentualiPF(CalendarioFerie model, int anno)
        {
            var db = new myRaiData.digiGappEntities();
            var def = db.MyRai_PianoFerieDefinizioni.Where(x => x.anno == anno).FirstOrDefault();
            if (def == null) throw new Exception("Piano ferie " + anno + " non definito");
            model.PianoFerieDip.PF_PercentualeSuSpettanza = (int)def.percentuale_spettanza;

            if (model.ItemFoglioExcelArretrati == null)
                model.PianoFerieDip.PF_PercentualeSuArretrati = 0;
            else
                model.PianoFerieDip.PF_PercentualeSuArretrati = (int)def.percentuale_arretrati;
            model.PianoFerieDip.PF_PercentualeSuTotale = (int)def.percentuale_su_totale;
            model.PianoFerieDip.PF_DataLimite = def.percentuale_entro;

            var ferie = model.tipiGiornataSel.Where(x => x.siglaSemplice == "FE").FirstOrDefault();
            if (ferie != null)
            {
                model.PianoFerieDip.PF_TotaleSpettanza = ferie.resoconto.Spettanti - ferie.resoconto.Usufruite;
                model.PianoFerieDip.PF_TotaleArretrati = ferie.resoconto.AnnoPrec;

                model.PianoFerieDip.PF_Residue = ferie.resoconto.Spettanti + ferie.resoconto.AnnoPrec;

                model.PianoFerieDip.PF_InteroSuSpettanza = (float)((ferie.resoconto.Spettanti + (ferie.resoconto.AnnoPrec < 0 ? ferie.resoconto.AnnoPrec : 0)) * (float)model.PianoFerieDip.PF_PercentualeSuSpettanza / (float)100);
                model.PianoFerieDip.PF_InteroSuArretrati = (int)((ferie.resoconto.AnnoPrec * (float)model.PianoFerieDip.PF_PercentualeSuArretrati / (float)100));
                if (ferie.resoconto.AnnoPrec > 0 && ferie.resoconto.AnnoPrec < 10)
                    model.PianoFerieDip.PF_InteroSuArretrati = 1;

                if (ferie.resoconto.AnnoPrec < 0) model.PianoFerieDip.PF_InteroSuArretrati = 0;

                model.PianoFerieDip.PF_InteroSuArretrati = 0;
                model.PianoFerieDip.PF_Intero75percento = (int)(((float)((model.PianoFerieDip.PF_InteroSuArretrati + model.PianoFerieDip.PF_InteroSuSpettanza) * model.PianoFerieDip.PF_PercentualeSuTotale) / (float)100));

                float minimoPian = 0;
                if (model.ItemFoglioExcelArretrati == null)
                {
                    minimoPian = model.PianoFerieDip.PF_InteroSuSpettanza + model.PianoFerieDip.PF_InteroSuArretrati;
                }
                else
                {
                    minimoPian = model.PianoFerieDip.PF_InteroSuSpettanza + model.FEperFoglioExcelArretrati;
                }
                model.PianoFerieDip.HaMezzaGiornata = minimoPian != (int)minimoPian;
            }
            /* @if (Model.ItemFoglioExcelArretrati == null)
                        {
                            <span id="pian-su-tot">@(Model.PianoFerieDip.PF_InteroSuSpettanza + Model.PianoFerieDip.PF_InteroSuArretrati)</span>
            }
                        else
                        {
                            <span id="pian-su-tot">@(Model.PianoFerieDip.PF_InteroSuSpettanza + Model.FEperFoglioExcelArretrati)</span>
                        }*/

        }

        public static void GetStatoPianoFerie(CalendarioFerie model, int anno)
        {
            var db = new myRaiData.digiGappEntities();
            string matricola = CommonHelper.GetCurrentUserMatricola();
            var pf = db.MyRai_PianoFerie.Where(x => x.matricola == matricola && x.anno == anno).FirstOrDefault();

            string sede = UtenteHelper.SedeGapp(DateTime.Now);
            string rep = UtenteHelper.Reparto();
            if (String.IsNullOrWhiteSpace(rep) || rep.Trim() == "0" || rep.Trim() == "00") rep = null;
            sede = sede + rep;
            var pfsede = db.MyRai_PianoFerieSedi.Where(x => x.sedegapp == sede && x.anno == anno).OrderByDescending(x => x.numero_versione).FirstOrDefault();

            if (pf == null || pf.data_consolidato == null)
                model.PianoFerieDip.StatoPianoFerie = PianoFerieDipendente.StatiPianoFerie.In_Compilazione;
            else if (pf.data_consolidato != null && pf.data_approvato ==null)
                model.PianoFerieDip.StatoPianoFerie = PianoFerieDipendente.StatiPianoFerie.In_Approvazione;
            else if (pf.data_consolidato != null && pf.data_approvato !=null)
                model.PianoFerieDip.StatoPianoFerie = PianoFerieDipendente.StatiPianoFerie.Approvato;

            //else if (pf.data_consolidato != null && pf.data_approvato == null)
            //    model.PianoFerieDip.StatoPianoFerie = PianoFerieDipendente.StatiPianoFerie.In_Approvazione;
            //else if (pf.data_approvato != null)
            //    model.PianoFerieDip.StatoPianoFerie = PianoFerieDipendente.StatiPianoFerie.Approvato;

            model.PianoFerieDip.PianoFerieDB = pf;
        }

        public static void GetGiorniPianoFerieSalvati(CalendarioFerie model, int anno)
        {
            var db = new myRaiData.digiGappEntities();
            var def = db.MyRai_PianoFerieDefinizioni.Where(x => x.anno == anno).FirstOrDefault();
            if (def == null) throw new Exception("Piano ferie " + anno + " non definito");

            string matricola = CommonManager.GetCurrentUserMatricola();

            DateTime[] giaInviatiGapp = db.MyRai_PianoFerieBatch.Where(x => x.matricola == matricola && 
                x.data_eccezione.Year == anno && x.data_inserimento_gapp != null).Select(x => x.data_eccezione).ToArray();

            DateTime dmarzo = new DateTime(2021, 3, 31);
            DateTime d1 = new DateTime(2021, 1, 1);

            if (model.HaEstensioneMarzo)
            {
                model.PianoFerieDip.GiorniPianoFerie = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matricola &&
                 (x.data.Year == anno || (x.data<=dmarzo && x.data >=d1)) && !giaInviatiGapp.Contains(x.data)).ToList();

            }
            else
            {
                model.PianoFerieDip.GiorniPianoFerie = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == matricola &&
              x.data.Year == anno && !giaInviatiGapp.Contains(x.data)).ToList();

            }



            //aggiunto10052019
            List<DateTime> LG = new List<DateTime>();
            var fegapp = model.tipiGiornataSel.Where(z => z.sigla == "FE").FirstOrDefault();
            if (fegapp != null)
            {
                LG = fegapp.resoconto.GiorniFerie.Select(a => a.data).ToList();

            }
            model.PianoFerieDip.GiorniPianoFerie.RemoveAll(x => LG.Contains(x.data));

            DateTime Dlim = def.percentuale_entro;// new DateTime(anno, Convert.ToInt32(p[1]), Convert.ToInt32(p[0]));

            if (FeriePermessiManager.UtenteProduzione())
            {
                Dlim = new DateTime(anno, 12, 31);
                model.UtenteProduzione = true;
            }

            float giaEntro = 0;


            List<DateTime> DateMarzo = new List<DateTime>();

            if (UtenteHelper.TipoDipendente() != "D")
            {
                for (int i = 11; i <= 31; i++)
                    DateMarzo.Add(new DateTime(2020, 3, i));
            }
            var giorni = model.tipiGiornataSel.Where(x => x.sigla == "FE").FirstOrDefault().resoconto.GiorniFerie.Where(x => x.data <= Dlim);
            foreach (var g in giorni)
            {
                if (g.Frazione != null && g.Frazione.StartsWith("Mezza"))
                {
                    if (DateMarzo.Contains(g.data) && model.ItemFoglioExcelArretrati != null)
                        model.NonPartecipantiRaggiungimentoPercheFruiteMarzo += 0.5f;
                    else
                        giaEntro += 0.5f;
                }
                else
                {
                    if (DateMarzo.Contains(g.data) && model.ItemFoglioExcelArretrati != null)
                        model.NonPartecipantiRaggiungimentoPercheFruiteMarzo += 1;
                    else
                        giaEntro += 1;
                }
            }
            if (model.ItemFoglioExcelArretrati != null && model.ItemFoglioExcelArretrati.rimuovi_1131 != null)
            {
                if (model.NonPartecipantiRaggiungimentoPercheFruiteMarzo > 0)
                {
                    model.NonPartecipantiRaggiungimentoPercheFruiteMarzo -= (int)model.ItemFoglioExcelArretrati.rimuovi_1131;
                    if (model.NonPartecipantiRaggiungimentoPercheFruiteMarzo < 0)
                        model.NonPartecipantiRaggiungimentoPercheFruiteMarzo = 0;
                }
                if (giaEntro > 0)
                {
                    giaEntro += (int)model.ItemFoglioExcelArretrati.rimuovi_1131;
                    if (giaEntro < 0) giaEntro = 0;
                }
            }

            //ricerca entro 31 ott
            DateTime d31ott = new DateTime(anno, 10, 31);
            if (FeriePermessiManager.UtenteProduzione())
            {
                d31ott = new DateTime(anno, 12, 31);
            }
            if (model.HaEstensioneMarzo)
            {
                d31ott = new DateTime(2021, 3, 31);
            }

            float giaEntro31ott_RR = 0;
            float giaEntro31ott_RF = 0;
            float giaEntro31ott_FE = 0;

            var giornientro31ottFE = model.tipiGiornataSel.Where(x => x.sigla == "FE").FirstOrDefault()
                .resoconto.GiorniFerie.Where(x => x.data <= d31ott);
            foreach (var g in giornientro31ottFE)
            {
                if (g.Frazione != null && g.Frazione.StartsWith("Mezza"))
                    giaEntro31ott_FE += 0.5f;
                else
                    giaEntro31ott_FE += 1;
            }

            var rrdays = model.tipiGiornataSel.Where(x => x.sigla == "RR").FirstOrDefault();
            if (rrdays != null)
            {
                var giornientro31ottRR = rrdays.resoconto.GiorniFerie.Where(x => x.data <= d31ott);
                foreach (var g in giornientro31ottRR)
                {
                    if (g.Frazione != null && g.Frazione.StartsWith("Mezza"))
                        giaEntro31ott_RR += 0.5f;
                    else
                        giaEntro31ott_RR += 1;
                }
            }


            var rfdays = model.tipiGiornataSel.Where(x => x.sigla == "RF").FirstOrDefault();
            if (rfdays != null)
            {
                var giornientro31ottRF = rfdays.resoconto.GiorniFerie.Where(x => x.data <= d31ott);
                foreach (var g in giornientro31ottRF)
                {
                    if (g.Frazione != null && g.Frazione.StartsWith("Mezza"))
                        giaEntro31ott_RF += 0.5f;
                    else
                        giaEntro31ott_RF += 1;
                }
            }



            model.PianoFerieDip.PF_GiaSalvatiTotali = (float)(model.resocontoFerie.ferieUsufruite +
                model.resocontoFerie.feriePianificate +
                model.resocontoFerie.ferieRichieste +
                model.PianoFerieDip.GiorniPianoFerie.Where(x => x.eccezione == "FE").Count()
                - model.NonPartecipantiRaggiungimentoPercheFruiteMarzo
                );

            model.PianoFerieDip.PF_GiaSalvatiEntro30Settembre = giaEntro
                + model.PianoFerieDip.GiorniPianoFerie.Where(x => x.data <= Dlim && x.eccezione == "FE").Count()
                ;

            model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre_RR = giaEntro31ott_RR
                + model.PianoFerieDip.GiorniPianoFerie.Where(x => x.data <= d31ott && (x.eccezione == "RR")).Count();

            model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre_RF = giaEntro31ott_RF
               + model.PianoFerieDip.GiorniPianoFerie.Where(x => x.data <= d31ott && (x.eccezione == "RF")).Count();

            model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre_FE = giaEntro31ott_FE
              + model.PianoFerieDip.GiorniPianoFerie.Where(x => x.data <= d31ott && (x.eccezione == "FE")).Count();

            if (model.ItemFoglioExcelArretrati == null)
            {
                model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre = model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre_RR +
               model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre_RF +
               model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre_FE;
            }
            else
            {
                if (model.HaEstensioneMarzo)
                {
                    //cambiaseg
                    float contributoFE = model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre_FE;
                    if (contributoFE > model.FEperFoglioExcelArretrati) contributoFE = model.FEperFoglioExcelArretrati;

                    float contributoRR = model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre_RR;
                    if (contributoRR > model.RRperFoglioExcelArretrati) contributoRR = model.RRperFoglioExcelArretrati;

                    float contributoRF = 0;// model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre_RF;
                    //if (contributoRF > model.RFperFoglioExcelArretrati) contributoRF = model.RFperFoglioExcelArretrati;

                    model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre =
                        contributoRR + contributoRF + contributoFE;
                }
                else
            {
                float contributoRR = model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre_RR;
                if (contributoRR > model.RRperFoglioExcelArretrati) contributoRR = model.RRperFoglioExcelArretrati;

                float contributoRF = model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre_RF;
                if (contributoRF > model.RFperFoglioExcelArretrati) contributoRF = model.RFperFoglioExcelArretrati;

                float contributoFE = model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre_FE;
                if (contributoFE > model.FEperFoglioExcelArretrati) contributoFE = model.FEperFoglioExcelArretrati;
                model.PianoFerieDip.PF_GiaSalvatiEntro31Ottobre =
                    contributoRR + contributoRF + contributoFE;
            }
               
            }

            //      + model.PianoFerieDip.GiorniPianoFerie
            //      .Where(x => x.data <= d31ott && (x.eccezione == "FE" || x.eccezione == "RR" || x.eccezione == "RF")).Count()
            //    ;

        }
    }

    public class CongediArretratiHRDW
    {
        public string matricola_dp { get; set; }
        public string nominativo { get; set; }
        public DateTime data { get; set; }
        public string cod_eccezione { get; set; }
        public string ecc_padre { get; set; }
        public string desc_ecc_padre { get; set; }
    }
    public class ArretratiHRDW
    {


        public int SKY_MATRICOLA { get; set; }
        public decimal? GG_competenza_AP_giorn { get; set; }
        public decimal? GG_ARR_da_smaltire_giorn { get; set; }
        public decimal? ARR_FE_da_smaltire { get; set; }
        public decimal? ARR_MN_da_smaltire { get; set; }
        public decimal? ARR_MR_da_smaltire { get; set; }
        public DateTime data_inizio_validita { get; set; }
        public DateTime data_fine_validita { get; set; }
        public string matricola_dp { get; set; }
    }
    public class ArretratiHRDW_normalized
    {
        public decimal GG_competenza_AP_giorn { get; set; }
        public decimal GG_ARR_da_smaltire_giorn { get; set; }
        public decimal ARR_FE_da_smaltire { get; set; }
        public decimal ARR_MN_da_smaltire { get; set; }
        public decimal ARR_MR_da_smaltire { get; set; }

    }
    public class ImportoLordoHRDW
    {
        public string cod_anno_mese_7 { get; set; }
        public string cod_aggregato_costi { get; set; }
        public string desc_aggregato_costi { get; set; }
        public decimal totale { get; set; }
    }
}
