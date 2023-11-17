using myRaiHelper.GenericRepository;
using System;
using System.Linq;
using System.Web.Mvc;
using myRaiCommonModel;
using myRaiData.Incentivi;
using System.Collections.Generic;
using myRaiHelper;
using ComunicaCics;
using myRaiGestionale.RepositoryServices;
using myRaiCommonManager;
using System.Net;
using myRaiGestionale.Extensions;
using myRaiGestionale.Exceptions;
using myRaiGestionale.Services;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace myRaiGestionale.Controllers
{
    public class ImmatricolazioneController : BaseCommonController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SessionHelper.Set("GEST_SECTION", "GESTIONE");
            base.OnActionExecuting(filterContext);
        }

        #region GET

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult InitImmatricolazioni()
        {
            return PartialView("_subpartial/InitImmatricolazioni");
        }
        [HttpPost]
        //public ActionResult LoadRicercaImmatricolazione(DateTime? dal, DateTime? al, string matricola, string nome, string cognome)
        public ActionResult LoadRicercaImmatricolazione(DateTime? dal, DateTime? al, string matricola, string nominativo)
        {
            List<ImmatricolazioniVM> listaImmFilter = new List<ImmatricolazioniVM>();

            string hrisAbil = AuthHelper.HrisFuncAbil(CommonHelper.GetCurrentUserMatricola());
            if (!String.IsNullOrWhiteSpace(hrisAbil))
                hrisAbil = "HRCE";

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                IEnumerable<ImmatricolazioniVM> ListaimmFilter = null;
                IQueryable<XR_IMM_IMMATRICOLAZIONI> tmpSint = null;
                if (hrisAbil=="HRCE")
                    tmpSint = AuthHelper.SintesiFilter(db.XR_IMM_IMMATRICOLAZIONI.AsQueryable(), CommonHelper.GetCurrentUserMatricola(), null, "HRCE", "IMMA");
                else
                    tmpSint = AuthHelper.SintesiFilter(db.XR_IMM_IMMATRICOLAZIONI.AsQueryable(), CommonHelper.GetCurrentUserMatricola(), null, "HRIS_GEST", "IMMGES");
                if ((dal.HasValue && al.HasValue) && (dal.Value.ToShortDateString() != "01/01/0001" || al.Value.ToShortDateString() != "01/01/0001"))
                {
                    /*  ListaimmFilter = immatricolazioneRepository.GetAll(
                          w => w.DES_COGNOMEPERS.StartsWith(nominativo) || w.DTA_INIZIO.CompareTo(dal.Value) > 0 && w.DTA_FINE.CompareTo(al.Value) > 0 || w.COD_MATDIP == matricola,
                          o => o.DTA_IMM_CREAZIONE.ToString(), null, null, null, null, null, null, null).TryConvertItemsTo(s => ImmatricolazioniVM.ConverToImmatricolazioneVM(s)).ToList();*/
                    var elenco = tmpSint.Where(imm => !imm.TIPO_OPERAZIONE.Equals("A") && imm.ESITO.Value && (imm.DTA_IMM_CREAZIONE.CompareTo(dal.Value) >= 0 && imm.DTA_IMM_CREAZIONE.CompareTo(al.Value) <= 0) && (!String.IsNullOrEmpty(nominativo) && !String.IsNullOrEmpty(matricola) ? imm.DES_COGNOMEPERS.StartsWith(nominativo) && (imm.COD_MATDIP == matricola) : !String.IsNullOrEmpty(nominativo) ? imm.DES_COGNOMEPERS.StartsWith(nominativo) : !String.IsNullOrEmpty(matricola) ? imm.COD_MATDIP == matricola : imm.COD_MATDIP != null))
                                                            .Where(imm => imm.DTA_IMM_CREAZIONE ==
                                                                            db.XR_IMM_IMMATRICOLAZIONI
                                                                                .Where(imm1 => imm1.ID_PERSONA == imm.ID_PERSONA)
                                                                                .Max(imm1 => imm1.DTA_IMM_CREAZIONE)
                                                                    )
                                                                  .OrderBy(o => o.DTA_IMM_CREAZIONE)
                                                                  .ThenBy(o => o.DES_COGNOMEPERS)
                                                                  .ThenBy(o => o.TIPO_OPERAZIONE).GroupJoin(db.JOBASSIGN, imm => imm.ID_PERSONA, job => job.ID_PERSONA, (imm, job) => new { imm, esistePianoFormativo = job.GroupJoin(db.RUOLO, j => j.COD_RUOLO, r => r.COD_RUOLO, (j, r) => new { r.FirstOrDefault().COD_RUOLOAGGREG }).Where(X => X.COD_RUOLOAGGREG.Equals("PROFORM")).Count() > 0 ? true : false })
                                                              .ToList();
                    ListaimmFilter = elenco.GroupJoin(db.V_XR_IMM_INSERVIZIO, imm => imm.imm.COD_MATDIP, servizio => servizio.matricola, (imm, servizio) => new { imm, servizio }).GroupBy(x => x.imm.imm.ID_PERSONA).Select(x => x.OrderByDescending(l => l.imm.imm.TMS_TIMESTAMP).First()).Select(entity => new ImmatricolazioniVM()
                    {
                        IdEvento = entity.imm.imm.ID_EVENTO,
                        IdPersona = entity.imm.imm.ID_PERSONA.GetValueOrDefault(),
                        Nome = entity.imm.imm.DES_NOMEPERS,
                        Cognome = entity.imm.imm.DES_COGNOMEPERS,
                        SecondoCognome = entity.imm.imm.DES_SECCOGNOME,
                        CodiceISTAT = entity.imm.imm.COD_CITTA,
                        LuogoDiNascita = entity.imm.imm.COD_CITTA,
                        DataNascita = entity.imm.imm.DTA_NASCITAPERS,
                        DataInizio = entity.imm.imm.DTA_INIZIO,
                        DataFine = entity.imm.imm.DTA_FINE,
                        CodiceFiscale = entity.imm.imm.CSF_CFSPERSONA,
                        Matricola = entity.imm.imm.COD_MATDIP,
                        DataCreazione = entity.imm.imm.DTA_IMM_CREAZIONE,
                        esitoCics = entity.imm.imm.ESITO_CICS,
                        TipoOperazione = entity.imm.imm.TIPO_OPERAZIONE,
                        SelectedSezione = entity.imm.imm.COD_SEZIONE,
                        SelectedAzienda = entity.imm.imm.COD_IMPRESA,
                        SelectedCategoria = entity.imm.imm.COD_QUALIFICA,
                        SelectedRapplavoro = entity.imm.imm.COD_TIPORLAV,
                        SelectedSede = entity.imm.imm.COD_SEDE,
                        SelectedServizio = entity.imm.imm.COD_SERVIZIO,
                        Genere = Convert.ToChar(entity.imm.imm.COD_SESSO),
                        esistePianoFormativo = entity.imm.esistePianoFormativo,
                        SelectedMansione = entity.imm.imm.COD_RUOLO,
                        InServizio = entity.servizio.Count() > 0 ? entity.servizio.FirstOrDefault().InServizio : 0

                    });
                }
                else
                {
                    var elenco = tmpSint.Where(imm => !imm.TIPO_OPERAZIONE.Equals("A") && imm.ESITO.Value && (!String.IsNullOrEmpty(nominativo) && !String.IsNullOrEmpty(matricola) ? imm.DES_COGNOMEPERS.StartsWith(nominativo) && (imm.COD_MATDIP == matricola) : !String.IsNullOrEmpty(nominativo) ? imm.DES_COGNOMEPERS.StartsWith(nominativo) : imm.COD_MATDIP == matricola))
                                                            .Where(imm => imm.DTA_IMM_CREAZIONE ==
                                                                            db.XR_IMM_IMMATRICOLAZIONI
                                                                                .Where(imm1 => imm1.ID_PERSONA == imm.ID_PERSONA)
                                                                                .Max(imm1 => imm1.DTA_IMM_CREAZIONE)
                                                                    )
                        .OrderBy(o => o.DTA_IMM_CREAZIONE)
                                                                 .ThenBy(o => o.DES_COGNOMEPERS)
                                                                 .ThenBy(o => o.TIPO_OPERAZIONE).GroupJoin(db.JOBASSIGN, imm => imm.ID_PERSONA, job => job.ID_PERSONA, (imm, job) => new { imm, esistePianoFormativo = job.GroupJoin(db.RUOLO, j => j.COD_RUOLO, r => r.COD_RUOLO, (j, r) => new { r.FirstOrDefault().COD_RUOLOAGGREG }).Where(X => X.COD_RUOLOAGGREG.Equals("PROFORM")).Count() > 0 ? true : false })
                                                              .ToList();
                    ListaimmFilter = elenco.GroupJoin(db.V_XR_IMM_INSERVIZIO, imm => imm.imm.COD_MATDIP, servizio => servizio.matricola, (imm, servizio) => new { imm, servizio }).GroupBy(x => x.imm.imm.ID_PERSONA).Select(x => x.OrderByDescending(l => l.imm.imm.TMS_TIMESTAMP).First()).Select(entity => new ImmatricolazioniVM()
                    {
                        IdEvento = entity.imm.imm.ID_EVENTO,
                        IdPersona = entity.imm.imm.ID_PERSONA.GetValueOrDefault(),
                        Nome = entity.imm.imm.DES_NOMEPERS,
                        Cognome = entity.imm.imm.DES_COGNOMEPERS,
                        SecondoCognome = entity.imm.imm.DES_SECCOGNOME,
                        CodiceISTAT = entity.imm.imm.COD_CITTA,
                        LuogoDiNascita = entity.imm.imm.COD_CITTA,
                        DataNascita = entity.imm.imm.DTA_NASCITAPERS,
                        DataInizio = entity.imm.imm.DTA_INIZIO,
                        DataFine = entity.imm.imm.DTA_FINE,
                        CodiceFiscale = entity.imm.imm.CSF_CFSPERSONA,
                        Matricola = entity.imm.imm.COD_MATDIP,
                        DataCreazione = entity.imm.imm.DTA_IMM_CREAZIONE,
                        esitoCics = entity.imm.imm.ESITO_CICS,
                        TipoOperazione = entity.imm.imm.TIPO_OPERAZIONE,
                        SelectedSezione = entity.imm.imm.COD_SEZIONE,
                        SelectedAzienda = entity.imm.imm.COD_IMPRESA,
                        SelectedCategoria = entity.imm.imm.COD_QUALIFICA,
                        SelectedRapplavoro = entity.imm.imm.COD_TIPORLAV,
                        SelectedSede = entity.imm.imm.COD_SEDE,
                        SelectedServizio = entity.imm.imm.COD_SERVIZIO,
                        Genere = Convert.ToChar(entity.imm.imm.COD_SESSO),
                        esistePianoFormativo = entity.imm.esistePianoFormativo,
                        SelectedMansione = entity.imm.imm.COD_RUOLO,
                        InServizio = entity.servizio.Count() > 0 ? entity.servizio.FirstOrDefault().InServizio : 0

                    });
                    /*   ListaimmFilter = immatricolazioneRepository.GetAll(w => w.DES_COGNOMEPERS.StartsWith(nominativo) || w.COD_MATDIP == matricola,
                     o => o.DTA_IMM_CREAZIONE.ToString(), null, null, null, null, null, null, null).TryConvertItemsTo(s => ImmatricolazioniVM.ConverToImmatricolazioneVM(s)).ToList();*/
                }
                listaImmFilter.AddRange(ListaimmFilter);
            }
            catch (Exception ex)
            {
                string msgError = ex.ToString();
                listaImmFilter = new List<ImmatricolazioniVM>();

            }
            return PartialView("_subpartial/Elenco_Immatricolazioni", listaImmFilter);
        }
        public ActionResult GetListaImmatricolazioni(int stato, string tipoOperazione = "I", bool isPianoFormativo = false)
        {
            List<ImmatricolazioniVM> Lista_Immatricolazioni = new List<ImmatricolazioniVM>();
            try
            {
                var db = new IncentiviEntities();
                ImmatricolazioneServizio immatricolazioneServizio = new ImmatricolazioneServizio(db);
                //TO DO verificare se lo stato è in attesa,firmato o respinto 
                // se in attesa allora carico la lista su piano formativo da pianificare altrimenti niente
                var imm = immatricolazioneServizio.GetUltimeImmatricolazioni(stato, tipoOperazione).OrderByDescending(o => o.DataCreazione);
                foreach (var item in imm)
                {
                    if (isPianoFormativo)
                    {

                        item.IsForPianoFormativo = true;
                    }
                    //gestioneDatiDaVisualizzare.GetDescrizioneDettagli(item);
                    var rapportoLavorativo = "";
                    var descrizioneServizio = db.XR_TB_SERVIZIO.FirstOrDefault(x => x.COD_SERVIZIO == item.SelectedServizio).DES_SERVIZIO;
                    item.SelectedServizio = descrizioneServizio;
                    if (item.SelectedRapplavoro.Equals("1") || item.SelectedRapplavoro.Equals("3"))
                    {
                        rapportoLavorativo = "TEMPO INDETERMINATO";
                    }
                    else if (item.SelectedRapplavoro.Equals("2") || item.SelectedRapplavoro.Equals("4"))
                    {
                        item.SelectedRapplavoro = "TEMPO DETERMINATO";
                    }
                    else
                    {
                        rapportoLavorativo = "CO.CO.CO";
                    }
                    item.SelectedRapplavoro = rapportoLavorativo;
                    Lista_Immatricolazioni.Add(item);
                }
            }
            catch (Exception ex)
            {
                Lista_Immatricolazioni = new List<ImmatricolazioniVM>();
            }

            return PartialView("_subpartial/Elenco_Immatricolazioni", Lista_Immatricolazioni);
        }

        public ActionResult ModaleInsertAndEditImmatricolazione(int? id, string codicefiscale, bool? exist /*char? letteraDicontrollo*/)
        {
            ImmatricolazioniVM viewmodel = new ImmatricolazioniVM();
            int fourDigitYearBirth = 0;
            string monthCaseSwitch = " ";
            bool dayAndGenerePositive = false;
            char genere = ' ';
            int day = 0;
            string daystring = " ";
            string dataDiNascita = " ";
            string codiceISTAT = " ";
            DateTime dataNascita = new DateTime();

            IncentiviEntities db = new IncentiviEntities();
            try
            {
                if (id != null)
                {
                    //Edit
                    XR_IMM_IMMATRICOLAZIONI result = db.XR_IMM_IMMATRICOLAZIONI.Find(id);
                    viewmodel = ImmatricolazioniVM.ConverToImmatricolazioneVM(result);
                    viewmodel.CodiceISTAT = db.TB_COMUNE.Find(result.COD_CITTA).COD_PROV_STATE;
                    viewmodel.Cittadinanza = result.COD_CITTAD;
                    viewmodel.Nazione = result.COD_CITTAD;
                    viewmodel.SelectedServizio = result.COD_SERVIZIO;
                    viewmodel.SelectedMansione = result.COD_RUOLO;
                    //viewmodel.SelectedSezione = gestioneDatiDaVisualizzare.GetDecodificaSezione(viewmodel.SelectedSezione);
                }
                else
                {
                    //Insert 
                    if (exist.HasValue && exist == false)
                    {


                        //Recupero le informazioni dal codice fiscale 
                        int yearBirthTwoDigit = Convert.ToInt32(codicefiscale.Substring(6, 2));
                        char monthBirth = Convert.ToChar(codicefiscale.Substring(8, 1));
                        int dayBirth = Convert.ToInt32(codicefiscale.Substring(9, 2));
                        fourDigitYearBirth = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(yearBirthTwoDigit);
                        string tmpCodiceFiscale = codicefiscale;
                        Regex CheckRegex = new Regex(@"^[A-Z]{6}[\d]{2}[A-Z][\d]{2}[A-Z][\d]{3}[A-Z]$");
                        if (!CheckRegex.Match(codicefiscale).Success)
                        {
                            // Regex failed: it can be either an omocode or an invalid Fiscal Code
                            tmpCodiceFiscale = GeneraCodiceFiscaleServizio.SostituisciLettereOmocodia(codicefiscale);
                        }
                        codiceISTAT = tmpCodiceFiscale.Substring(11, 4);
                        var comune = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == codiceISTAT).COD_CITTA;
                        switch (monthBirth)
                        {
                            case 'A':
                                monthCaseSwitch = "01";
                                break;
                            case 'B':
                                monthCaseSwitch = "02";
                                break;
                            case 'C':
                                monthCaseSwitch = "03";
                                break;
                            case 'D':
                                monthCaseSwitch = "04";
                                break;
                            case 'E':
                                monthCaseSwitch = "05";
                                break;
                            case 'H':
                                monthCaseSwitch = "06";
                                break;
                            case 'L':
                                monthCaseSwitch = "07";
                                break;
                            case 'M':
                                monthCaseSwitch = "08";
                                break;
                            case 'P':
                                monthCaseSwitch = "09";
                                break;
                            case 'R':
                                monthCaseSwitch = "10";
                                break;
                            case 'S':
                                monthCaseSwitch = "11";
                                break;
                            case 'T':
                                monthCaseSwitch = "12";
                                break;


                        }
                        dayAndGenerePositive = dayBirth - 40 > 0;
                        if (dayAndGenerePositive)
                        {
                            genere = 'F';
                            day = dayBirth - 40;
                            if (day < 10)
                            {
                                daystring = "0" + day.ToString();

                            }
                            else
                            {
                                daystring = day.ToString();
                            }
                        }
                        else
                        {
                            genere = 'M';
                            if (dayBirth < 10)
                                daystring = "0" + dayBirth.ToString();
                            else
                            {
                                daystring = dayBirth.ToString();


                            }
                        }

                        dataDiNascita = daystring + "/" + monthCaseSwitch + "/" + fourDigitYearBirth.ToString();
                        dataNascita = DateTime.ParseExact(dataDiNascita, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        viewmodel.Genere = genere;
                        viewmodel.DataNascita = dataNascita;
                        viewmodel.LuogoDiNascita = comune;
                        viewmodel.CodiceISTAT = db.TB_COMUNE.Find(comune).COD_PROV_STATE;
                        viewmodel.CodiceFiscale = codicefiscale;
                    }
                    else
                    {
                        return Json(new { Data = "", Esito = false }, JsonRequestBehavior.AllowGet);
                    }

                }
            }
            catch (Exception ex)
            {
                var message = ex;
                //Log excception 
                viewmodel = new ImmatricolazioniVM();
            }
            return PartialView("_subpartial/Modal_Inserimento", viewmodel);
        }
        public ActionResult GetDettaglioImmatricolazione(int? id)
        {
            ImmatricolazioniVM viewmodel;
            IncentiviEntities db = new IncentiviEntities();
            var dbTal = new myRaiDataTalentia.TalentiaEntities();
            try
            {
                XR_IMM_IMMATRICOLAZIONI result = db.XR_IMM_IMMATRICOLAZIONI.Find(id);
                viewmodel = ImmatricolazioniVM.ConverToImmatricolazioneVM(result);
                var provincia = (db.TB_COMUNE.FirstOrDefault(w => w.COD_CITTA == result.COD_CITTA)??new TB_COMUNE()).COD_PROV_STATE;
                viewmodel.DataCreazione = result.DTA_IMM_CREAZIONE;
                viewmodel.CodiceISTAT = result.COD_CITTA;
                viewmodel.SelectedAzienda = result.COD_IMPRESA;
                viewmodel.SelectedCategoria = result.COD_QUALIFICA;
                viewmodel.SelectedRapplavoro = result.COD_TIPORLAV;
                viewmodel.SelectedSede = result.COD_SEDE;
                viewmodel.SelectedServizio = result.COD_SERVIZIO;
                viewmodel.SelectedSezione = result.COD_SEZIONE;
                viewmodel.LuogoDiNascita = (db.TB_COMUNE.FirstOrDefault(w => w.COD_CITTA == result.COD_CITTA) ?? new TB_COMUNE()).DES_CITTA;
                viewmodel.Azienda = (db.CODIFYIMP.FirstOrDefault(w => w.COD_IMPRESA == result.COD_IMPRESA) ?? new CODIFYIMP()).COD_SOGGETTO;
                viewmodel.Categoria = (db.QUALIFICA.FirstOrDefault(w => w.COD_QUALIFICA == result.COD_QUALIFICA) ?? new QUALIFICA()).DES_QUALIFICA;
                viewmodel.RappLavoro = (db.TB_TPRLAV.FirstOrDefault(w => w.COD_TIPORLAV == result.COD_TIPORLAV) ?? new TB_TPRLAV()).DES_TIPORLAV;
                viewmodel.Servizio = (db.XR_TB_SERVIZIO.FirstOrDefault(w => w.COD_SERVIZIO == result.COD_SERVIZIO) ?? new XR_TB_SERVIZIO()).DES_SERVIZIO;
                viewmodel.Sezione = (dbTal.XR_STR_TSEZIONE.FirstOrDefault(w => w.codice_visibile == result.COD_SEZIONE) ?? new myRaiDataTalentia.XR_STR_TSEZIONE()).descrizione_lunga;
                viewmodel.Sede = (db.SEDE.FirstOrDefault(w => w.COD_SEDE == result.COD_SEDE) ?? new SEDE()).DES_SEDE;
                viewmodel.Provincia = provincia;
                viewmodel.Cittadinanza = (db.TB_CITTAD.FirstOrDefault(x => x.COD_CITTAD == result.COD_CITTAD) ?? new TB_CITTAD()).DES_CITTAD;
                viewmodel.Mansione = (db.RUOLO.FirstOrDefault(w => w.COD_RUOLO == result.COD_RUOLO) ?? new RUOLO()).DES_RUOLO;
                viewmodel.SelectedMansione = (db.RUOLO.FirstOrDefault(w => w.COD_RUOLO == result.COD_RUOLO) ?? new RUOLO()).DES_RUOLO;

                //viewmodel.Cittadinanza = dbContext.TB_CITTAD.Where()



            }
            catch (Exception ex)
            {
                viewmodel = new ImmatricolazioniVM();
                throw;
            }
            ViewBag.idPersona = viewmodel.IdPersona;
            return PartialView("_subpartial/DettaglioImmatricolazione", viewmodel);
        }
        #endregion

        #region CHECK
        public ActionResult CheckCodiceFiscaleExistOrNot(string cf)
        {
            object result = "";
            bool exist = false;
            try
            {
                if (!string.IsNullOrEmpty(cf))
                {
                    IncentiviEntities db = new IncentiviEntities();
                    cf = cf.ToUpper();
                    if (!GeneraCodiceFiscaleServizio.ControlloFormaleOK(cf))
                        throw new ImmatricolazioneException("Controllo formale NOK");
                    var idPersona = db.ANAGPERS.Where(x => x.CSF_CFSPERSONA == cf.ToUpper()).Select(x=>x.ID_PERSONA).FirstOrDefault();
                    exist = idPersona == 0 ? false : true; //db.ANAGPERS.Any(x => x.CSF_CFSPERSONA == cf.ToUpper());
                    if (exist)
                        result = new { Data = idPersona/* cf*/, Exist = true, Esito = false };
                    else
                        result = new { Data = "", CodiceFiscale = cf, Exist = false, Esito = true };
                }
            }
            catch (Exception ex)
            {
                result = new { Data = "", Esito = false };
            }
            return Json(new { Data = result, JsonRequestBehavior.AllowGet });
        }
        public ActionResult ControlloDatiAnagrafici(string nome, string cognome, string dataNascita, string genere, string cf, string comune, bool nuovo = false)
        {
            object result;
            IncentiviEntities db = new IncentiviEntities();

            var codiceFiscale = GeneraCodiceFiscaleServizio.CalcolaCodiceFiscale(nome.ToUpper(), cognome.ToUpper(), DateTime.Parse(dataNascita), Convert.ToChar(genere), comune.ToUpper());
            try
            {
                if (string.IsNullOrEmpty(nome)
                    || string.IsNullOrEmpty(cognome)
                    || string.IsNullOrEmpty(dataNascita)
                    || string.IsNullOrEmpty(genere)
                    || string.IsNullOrEmpty(cf)
                    || string.IsNullOrEmpty(comune))
                    throw new ImmatricolazioneException("Valori obbligatori");
                if (!GeneraCodiceFiscaleServizio.ControlloFormaleOK(cf))
                    throw new ImmatricolazioneException("Controllo formale NOK");
                if (!(codiceFiscale == cf))
                {
                    var omocodice = GeneraCodiceFiscaleServizio.SostituisciLettereOmocodia(cf.ToUpper());
                    omocodice = omocodice.Substring(0, 15) + GeneraCodiceFiscaleServizio.CalcolaCarattereDiControllo(omocodice.Substring(0, 15));
                    if (codiceFiscale != omocodice)
                    {
                        throw new ImmatricolazioneException(ImmatricolazioneException.COD_FISC_ERROR);
                    }
                    else throw new ImmatricolazioneException(ImmatricolazioneException.OMOCODIA);
                }
                if (nuovo && db.ANAGPERS.Any(x => x.CSF_CFSPERSONA == cf.ToUpper()))
                    throw new ImmatricolazioneException(ImmatricolazioneException.COD_FISC_ESISTENTE);

                result = new { data = "Operazione Eseguita", Esito = true };
            }
            catch (ImmatricolazioneException ex)
            {
                result = new { data = ex.Message, Esito = false };
            }
            catch (Exception ex)
            {
                result = new { data = "Operazione non riuscita", Esito = false };
            }
            return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = result };
        }

        [HttpPost]
        public ActionResult ControlloDatiAnagraficiContratto(ImmatricolazioniVM model)
        {
            object result;
            try
            {
                if (model.VerificaDatiContrattuali())
                    result = new { data = model, Esito = true };
                else
                    throw new ImmatricolazioneException("Controllo formale NOK");
            }
            catch (Exception ex)
            {
                result = new { data = ex.Message, Esito = false };
            }
            return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = result };
        }
        #endregion

        #region CRUD-POST
        [HttpPost]
        public ActionResult HandleImmatricolazione(ImmatricolazioniVM model)
        {
            var esito = "";
            var data = "";
            int idEvento = 0;
            int idPersona = 0;
            ImmatricolazioneServizio immatricolazioneServizio = new ImmatricolazioneServizio(new IncentiviEntities());

            try
            {
                model.Nome = model.Nome.ToUpper();
                model.Cognome = model.Cognome.ToUpper();
                if (model.IdEvento == 0)
                {
                    esito = "OK";
                    idEvento = immatricolazioneServizio.HandleImmatricolazione(model);
                    idPersona = model.IdPersona;
                    if (model.esitoCics && model.esito)
                    { esito = "OK"; }
                    else
                    {
                        esito = "KO";
                        data = "Non è stato possibile creare l'immatricolazione";
                    }
                }
                else
                {
                    idEvento = immatricolazioneServizio.HandleImmatricolazione(model);
                    idPersona = model.IdPersona;
                    if (model.esitoCics && model.esito)
                    { esito = "MODIFICA"; }
                    else
                    {
                        esito = "KO";
                        data = "La modifica non ha avuto effetto";
                    }


                }

            }
            catch (Exception ex)
            {
                esito = "ERRORE";
                data = ex.Message;
                HrisHelper.LogOperazione("Exception", Newtonsoft.Json.JsonConvert.SerializeObject(model), false, "Errore durante il salvataggio", null, ex);
            }
            return Json(new { Data = data, Esito = esito, idPersona = idPersona, idEvento = idEvento }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EliminaImmatricolazione(int idEvento)
        {
            string result = "";
            try
            {
                if (idEvento != 0)
                {
                    ImmatricolazioneServizio immatricolazioneServizio = new ImmatricolazioneServizio(new IncentiviEntities());
                    var imma = immatricolazioneServizio.PrepareToDelete(idEvento);
                    if (!imma.ESITO_CICS || !imma.ESITO.GetValueOrDefault())
                        result = "Si è verificato un errore durante la cancellazione";
                    else
                        result = "OK";
                }
                else
                {
                    throw new ImmatricolazioneException("Non è possibile cancellare l'immatricolazione");
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
                throw;
            }
            return Content(result);
        }
        #endregion

        public ActionResult CalcolaCF(int? id, string nome, string cognome, string istat, string dataNascita, string genere)
        {
            string cf = "";
            object result;
            try
            {

                DateTime date = DateTime.Parse(dataNascita);
                var gen = Convert.ToChar(genere);
                cf = GeneraCodiceFiscaleServizio.CalcolaCodiceFiscale(nome.ToUpper(), cognome.ToUpper(), date, gen, istat.ToUpper());
                result = new { cf = cf, Esito = true };
            }
            catch (Exception ex)
            {
                result = new { cf = "", Esito = false };
            }
            return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = result };
        }
        public JsonResult DecodProvincia(string cod)
        {
            string provincia = "";
            IncentiviEntities db = new IncentiviEntities();
            provincia = db.TB_COMUNE.FirstOrDefault(w => w.COD_CITTA == cod).COD_PROV_STATE;
            return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { codice = provincia, Esito = true } };
        }
        public ActionResult GeneraMatricola(string nome, string cognome)
        {
            object result;
            try
            {
                string Matricola = GeneraNuovaMatricolaSevizio.CalcoloImmmatricolazioneNuoviDipendenti(nome, cognome);
                result = new { data = Matricola, Esito = true };
            }
            catch (Exception ex)
            {

                result = new { data = ex, Esito = false };
            }
            return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = result };
        }


        #region DDL-SELECTASYNC

        public ActionResult GetNazioni(string filter, string value)
        {
            return Json(AnagraficaManager.GetCittadinanza(filter, value), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetComuni(string filter, string value)
        {
            return Json(AnagraficaManager.GetComuni(filter, value, false), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetImprese(string filter, string value)
        {
            return Json(AnagraficaManager.GetSocieta(filter, value, addCodDes: false), JsonRequestBehavior.AllowGet);
        }

        public static List<SelectListItem> GetListItems(string tipoValori)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            switch (tipoValori)
            {
                case "rapplav":
                    items.AddRange(AnagraficaManager.GetTipiRapportoLavorativo("", "", true, false));
                    break;
                default:
                    break;
            }

            return items;
        }

        public static List<SelectListItem> GetStatiImmatricolazione()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = "In attesa", Value = "0" });
            items.Add(new SelectListItem() { Text = "Firmata", Value = "1" });
            items.Add(new SelectListItem() { Text = "Respinta", Value = "2" });
            return items;

        }

        public ActionResult GetRapportiLavorativi(string filter, string value)
        {
            return Json(AnagraficaManager.GetTipiRapportoLavorativo(filter, value, addCodDes: false), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSezioni(string filter, string azienda, string servizio, string value)
        {
            return Json(AnagraficaManager.GetSezioni(filter.ToUpper(), value, servizio), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetServizi(string filter, string value, string azienda)
        {
            return Json(AnagraficaManager.GetServizi(filter, value, aziendaVal: azienda, addCodDes: false), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSedi(string filter, string value)
        {
            return Json(AnagraficaManager.GetSedi(filter.ToUpper(), value, addCodDes: false), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetQualifiche(string filter, string value)
        {
            return Json(AnagraficaManager.GetCategorie(filter, value, addCodDes: false, notQualStd: "ND"), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetMansioni(string filter, string value)
        {
            List<SelectListItem> listaMansioni;
            try
            {
                listaMansioni = AnagraficaManager.GetMansione(filter, value);

            }
            catch (Exception ex)
            {
                listaMansioni = new List<SelectListItem>();
            }
            return Json(listaMansioni.OrderBy(x => x.Text), JsonRequestBehavior.AllowGet);
        }
        #endregion



    }
}
