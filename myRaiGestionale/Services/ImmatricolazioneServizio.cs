using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiGestionale.RepositoryServices;
using myRaiHelper;
using System;
using System.Collections.Generic;
using myRaiGestionale.Extensions;
using System.Data.Entity.Validation;
using System.Linq;
using myRaiHelper.GenericRepository;
using myRai.DataAccess;

namespace myRaiGestionale.Services
{
    public class ImmatricolazioneServizio
    {
        private IncentiviEntities db;
        private GestioneDatiDaVisualizzare gestioneDatiDaVisualizzare;
        private ImmatricolazioneRepository immatricolazioneRepository;
        private AnagraficaRepository anagraficaRepository;
        ImmatricolazioniVM viewModel = new ImmatricolazioniVM();
        //private SezioneRepository sezioneRepository;
        private ImpresaRepository impresaRepository;
        private TipoContrattoDecodificaRepository contrattoRepository;
        private QualificaRepository qualificaRepository;
        private AssQualRepository assqualRepository;
        private AssTpContrattoRepository assTpContrattoRepository;
        private ServizioRepository servizioRepository;
        private IncarlavRepository incarlavRepository;
        private TrasfSedeRepository trasfSedeRepository;
        private TipoRapportoLavoroRepository rapportoDiLavoroRepository;
        private ComuneRepository comuneRepository;
        private UnitaORGRepository unitaOrgRepository;
        private ComprelRepository comprelRepository;
        string codUser = "";
        string codTermid = "";
        DateTime timestamp;
        public ImmatricolazioneServizio(IncentiviEntities db)
        {
            this.db = db;
            gestioneDatiDaVisualizzare = new GestioneDatiDaVisualizzare(db);
            immatricolazioneRepository = new ImmatricolazioneRepository(db);
            anagraficaRepository = new AnagraficaRepository(db);
            qualificaRepository = new QualificaRepository(db);
            assTpContrattoRepository = new AssTpContrattoRepository(db);
            unitaOrgRepository = new UnitaORGRepository(db);
            trasfSedeRepository = new TrasfSedeRepository(db);
            rapportoDiLavoroRepository = new TipoRapportoLavoroRepository(db);
            //sezioneRepository = new SezioneRepository(db);
            impresaRepository = new ImpresaRepository(db);
            contrattoRepository = new TipoContrattoDecodificaRepository(db);
            assqualRepository = new AssQualRepository(db);
            servizioRepository = new ServizioRepository(db);
            incarlavRepository = new IncarlavRepository(db);
            comuneRepository = new ComuneRepository(db);
            comprelRepository = new ComprelRepository(db);
        }
        //   internal List<ImmatricolazioniVM> LoadImmatricolazioniFilter(DateTime? dal, DateTime? al, string matricola, string nome, string cognome)
        internal List<ImmatricolazioniVM> LoadImmatricolazioniFilter(DateTime? dal, DateTime? al, string matricola, string nominativo)
        {
            string hrisAbil = AuthHelper.HrisFuncAbil(CommonHelper.GetCurrentUserMatricola());
            if (String.IsNullOrWhiteSpace(hrisAbil))
                hrisAbil = "HRCE";

            IQueryable<XR_IMM_IMMATRICOLAZIONI> tmpSint = null;
            if (hrisAbil=="HRCE")
                tmpSint= AuthHelper.SintesiFilter(db.XR_IMM_IMMATRICOLAZIONI.AsQueryable(), CommonHelper.GetCurrentUserMatricola(), null, "HRCE", "IMMA");
            else
                tmpSint = AuthHelper.SintesiFilter(db.XR_IMM_IMMATRICOLAZIONI.AsQueryable(), CommonHelper.GetCurrentUserMatricola(), null, "HRIS_GEST", "IMMGES");
            IEnumerable<ImmatricolazioniVM> ListaimmFilter = null;
            if ((dal.HasValue && al.HasValue) && (dal.Value.ToShortDateString() != "01/01/0001" || al.Value.ToShortDateString() != "01/01/0001"))
            {
                /*  ListaimmFilter = immatricolazioneRepository.GetAll(
                      w => w.DES_COGNOMEPERS.StartsWith(nominativo) || w.DTA_INIZIO.CompareTo(dal.Value) > 0 && w.DTA_FINE.CompareTo(al.Value) > 0 || w.COD_MATDIP == matricola,
                      o => o.DTA_IMM_CREAZIONE.ToString(), null, null, null, null, null, null, null).TryConvertItemsTo(s => ImmatricolazioniVM.ConverToImmatricolazioneVM(s)).ToList();*/
                
                var elenco = tmpSint.Where(imm => !imm.TIPO_OPERAZIONE.Equals("A") && imm.ESITO.Value && (imm.DTA_IMM_CREAZIONE.CompareTo(dal.Value) >= 0 && imm.DTA_IMM_CREAZIONE.CompareTo(al.Value) <= 0) && (!String.IsNullOrEmpty(nominativo) && !String.IsNullOrEmpty(matricola) ? imm.DES_COGNOMEPERS.StartsWith(nominativo) && (imm.COD_MATDIP == matricola) : !String.IsNullOrEmpty(nominativo) ? imm.DES_COGNOMEPERS.StartsWith(nominativo) : !String.IsNullOrEmpty(matricola) ? imm.COD_MATDIP == matricola : imm.COD_MATDIP != null))
                                                                                                                 .OrderBy(o => o.DTA_IMM_CREAZIONE)
                                                              .ThenBy(o => o.DES_COGNOMEPERS)
                                                              .ThenBy(o => o.TIPO_OPERAZIONE)
                                                              .GroupJoin(db.JOBASSIGN, imm => imm.ID_PERSONA, job => job.ID_PERSONA, (imm, job) => new { imm, esistePianoFormativo = job.GroupJoin(db.RUOLO, j => j.COD_RUOLO, r => r.COD_RUOLO, (j, r) => new { r.FirstOrDefault().COD_RUOLOAGGREG }).Where(X => X.COD_RUOLOAGGREG.Equals("PROFORM")).Count() > 0 ? true : false })
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
                                                                                                                   .OrderBy(o => o.DTA_IMM_CREAZIONE)
                                                             .ThenBy(o => o.DES_COGNOMEPERS)
                                                             .ThenBy(o => o.TIPO_OPERAZIONE)
                                                             .GroupJoin(db.JOBASSIGN, imm => imm.ID_PERSONA, job => job.ID_PERSONA, (imm, job) => new { imm, esistePianoFormativo = job.GroupJoin(db.RUOLO, j => j.COD_RUOLO, r => r.COD_RUOLO, (j, r) => new { r.FirstOrDefault().COD_RUOLOAGGREG }).Where(X => X.COD_RUOLOAGGREG.Equals("PROFORM")).Count() > 0 ? true : false })
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
            return ListaimmFilter.ToList();
        }
        public IEnumerable<ImmatricolazioniVM> GetUltimeImmatricolazioni(int stato, string tipoOperazione = "I")
        {
            string hrisAbil = AuthHelper.HrisFuncAbil(CommonHelper.GetCurrentUserMatricola());
            if (String.IsNullOrWhiteSpace(hrisAbil))
                hrisAbil = "HRCE";

            DateTime thisDay = DateTime.Today;
            IEnumerable<ImmatricolazioniVM> immatricolazioni = null;
            IQueryable<XR_IMM_IMMATRICOLAZIONI> tmpSint = null;
            if (hrisAbil=="HRCE")
                tmpSint = AuthHelper.SintesiFilter(db.XR_IMM_IMMATRICOLAZIONI.AsQueryable(), CommonHelper.GetCurrentUserMatricola(), null, "HRCE", "IMMA");
            else
                tmpSint = AuthHelper.SintesiFilter(db.XR_IMM_IMMATRICOLAZIONI.AsQueryable(), CommonHelper.GetCurrentUserMatricola(), null, "HRIS_GEST", "IMMGES");
            if (stato == 0)
            {

                var elenco = tmpSint.Where(imm => imm.TIPO_OPERAZIONE != "A" && imm.ESITO.Value)
                                                                          .Where(imm => imm.DTA_IMM_CREAZIONE ==
                                                                      db.XR_IMM_IMMATRICOLAZIONI
                                                                          .Where(imm1 => imm1.ID_PERSONA == imm.ID_PERSONA)
                                                                          .Max(imm1 => imm1.DTA_IMM_CREAZIONE)
                                                              )
                                                               .OrderBy(o => o.DTA_IMM_CREAZIONE)
                                                              .ThenBy(o => o.DES_COGNOMEPERS)
                                                              .ThenBy(o => o.TIPO_OPERAZIONE)
                                                              .GroupJoin(db.JOBASSIGN, imm => imm.ID_PERSONA, job => job.ID_PERSONA, (imm, job) => new { imm, esistePianoFormativo = job.GroupJoin(db.RUOLO, j => j.COD_RUOLO, r => r.COD_RUOLO, (j, r) => new { r.FirstOrDefault().COD_RUOLOAGGREG }).Where(X => X.COD_RUOLOAGGREG.Equals("PROFORM")).Count() > 0 ? true : false })
                                                                                             .ToList();
               
                immatricolazioni = elenco.GroupJoin(db.V_XR_IMM_INSERVIZIO, imm => imm.imm.COD_MATDIP, servizio => servizio.matricola, (imm, servizio) => new { imm, servizio }).GroupBy(x => x.imm.imm.ID_PERSONA).Select(x => x.OrderByDescending(l => l.imm.imm.TMS_TIMESTAMP).First()).Select(entity => new ImmatricolazioniVM()
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
                var elenco = tmpSint.Where(imm => imm.TIPO_OPERAZIONE != "A" && imm.ESITO.Value && imm.STATO == stato)
                                                    .Where(imm => imm.DTA_IMM_CREAZIONE ==
                                                                    db.XR_IMM_IMMATRICOLAZIONI
                                                                        .Where(imm1 => imm1.ID_PERSONA == imm.ID_PERSONA)
                                                                        .Max(imm1 => imm1.DTA_IMM_CREAZIONE)
                                                            ).OrderBy(o => o.DTA_IMM_CREAZIONE)
                                                            .ThenBy(o => o.DES_COGNOMEPERS)
                                                            .ThenBy(o => o.TIPO_OPERAZIONE).GroupJoin(db.JOBASSIGN, imm => imm.ID_PERSONA, job => job.ID_PERSONA, (imm, job) => new { imm, esistePianoFormativo = job.GroupJoin(db.RUOLO, j => j.COD_RUOLO, r => r.COD_RUOLO, (j, r) => new { r.FirstOrDefault().COD_RUOLOAGGREG }).Where(X => X.COD_RUOLOAGGREG.Equals("PROFORM")).Count() > 0 ? true : false }).ToList();
                immatricolazioni = elenco.GroupJoin(db.V_XR_IMM_INSERVIZIO, imm => imm.imm.COD_MATDIP, servizio => servizio.matricola, (imm, servizio) => new { imm, servizio }).GroupBy(x => x.imm.imm.ID_PERSONA).Select(x => x.OrderByDescending(l => l.imm.imm.TMS_TIMESTAMP).First()).Select(entity => new ImmatricolazioniVM()
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
        //    immatricolazioni= LinqHelper.PutInAndTogether(immatricolazioni, x => incluse.Any(y => x.SINTESI1.COD_QUALIFICA.StartsWith(y)));


            return immatricolazioni;
        }
        #region GESTIONE

        public int HandleImmatricolazione(ImmatricolazioniVM modelVM)
        {
            var operazione = "I";
            XR_IMM_IMMATRICOLAZIONI immatricolazione = null;
            if (modelVM != null)
            {

                if (modelVM.DataFine == null || string.IsNullOrEmpty(modelVM.DataFine.Value.ToString()))
                {
                    modelVM.DataFine = AnagraficaManager.GetDateLimitMax();
                }

                if (modelVM.IdEvento != 0)
                {

                    immatricolazione = modelVM.ConvertToXR_IMM_IMMATRICOlAZIONI();
                    operazione = "M";
                    PrepareToEdit(immatricolazione, operazione);
                    immatricolazione.P_MATRICOLA = "P" + immatricolazione.COD_MATDIP;
                }
                else
                {
                    immatricolazione = modelVM.ConvertToXR_IMM_IMMATRICOLAZIONEWithRelation();
                    PrepareToInsert(immatricolazione, operazione);
                    immatricolazione.P_MATRICOLA = "P" + immatricolazione.COD_MATDIP;

                }
                modelVM.esitoCics = immatricolazione.ESITO_CICS;
                modelVM.esito = immatricolazione.ESITO.GetValueOrDefault();
                modelVM.esito_ripristino = immatricolazione.ESITO_RIPRISTINO;
                if (immatricolazione.ESITO_CICS && immatricolazione.ESITO.GetValueOrDefault())
                {
                    CITTAD cittad = db.CITTAD.FirstOrDefault(x => x.ID_PERSONA == immatricolazione.ID_PERSONA);
                    immatricolazione.ESITO_DOMICILIO = AnagraficaManager.Tracciato_Domicilio(immatricolazione.COD_MATDIP, TipoAnaVar.Immatricolazione, anagraficaRepository.GetById(immatricolazione.ID_PERSONA), cittad, immatricolazione.ID_EVENTO, immatricolazione.DTA_INIZIO);

                }
                if (!modelVM.esito)
                {
                    db = new IncentiviEntities();
                    immatricolazioneRepository = new ImmatricolazioneRepository(db);
                }

                immatricolazioneRepository.Add(immatricolazione);
                immatricolazioneRepository.Save();

                modelVM.IdPersona = immatricolazione.ID_PERSONA.GetValueOrDefault();
            }

            return immatricolazione.ID_EVENTO;

        }





        public void PrepareToInsert(XR_IMM_IMMATRICOLAZIONI immatricolazione, string tipo_operazione)
        {
            var codiceUnitaOrg = unitaOrgRepository.GetBySezione(immatricolazione.COD_SEZIONE);
            var dataFineForCics = "";
            var cod_tpcontr = immatricolazione.COD_TIPORLAV;
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
            immatricolazione.COD_USER = codUser;
            immatricolazione.COD_TERMID = codTermId;
            immatricolazione.TMS_TIMESTAMP = tmsTimestamp;
            immatricolazione.TIPO_OPERAZIONE = tipo_operazione;
            immatricolazione.DTA_IMM_CREAZIONE = DateTime.Now;
            //  immatricolazione.COD_USER = CommonHelper.GetCurrentUserMatricola();
            //  immatricolazione.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            immatricolazione.P_MATRICOLA = (CommonHelper.GetCurrentUserPMatricola());
            immatricolazione.ID_EVENTO = immatricolazioneRepository.GeneraOid(x => x.ID_EVENTO);
            SetPropertyForCics(immatricolazione);
            if (immatricolazione.DTA_FINE.ToShortDateString().Equals(AnagraficaManager.GetDateLimitMax().ToShortDateString()))
                dataFineForCics = "        ";
            else
                dataFineForCics = immatricolazione.DTA_FINE.ToString("yyyyMMdd");
            var pMatricola = (CommonHelper.GetCurrentUserPMatricola());
            string rispostaCics = ImmatricolazioneCicsManager.ChiamataCics(immatricolazione, pMatricola, dataFineForCics, tipo_operazione);
            var TMP = rispostaCics.Substring(160, 3);
            immatricolazione.COD_TIPORLAV = cod_tpcontr;

            if (rispostaCics.Length < 10)
            {
                immatricolazione.ANAGPERS = null;
                immatricolazione.RISPOSTA_CICS = rispostaCics;
                //if (rispostaCics.Equals("ACK99"))
                // {
                immatricolazione.ESITO_CICS = false;
                immatricolazione.ESITO = false;
                // }
            }
            else
            {
                immatricolazione.RISPOSTA_CICS = rispostaCics.Substring(160, rispostaCics.Length - 160);
                if (rispostaCics.Substring(160, 3) == "000")
                {
                    immatricolazione.ESITO_CICS = true;
                    if (!SetPropertyForInsertImmatricolazione(immatricolazione, codiceUnitaOrg, cod_tpcontr))
                    {
                        rispostaCics = ImmatricolazioneCicsManager.ChiamataCics(immatricolazione, pMatricola, dataFineForCics, "A");
                        immatricolazione.ESITO_RIPRISTINO = rispostaCics.Length >= 163 && rispostaCics.Substring(160, 3) == "000";
                        immatricolazione.RISPOSTA_RIPRISTINO = rispostaCics;
                        immatricolazione.ESITO = false;
                        immatricolazione.ANAGPERS = null;
                    }
                    else
                        immatricolazione.ESITO = true;
                }
                else
                {
                    immatricolazione.ESITO_CICS = false;
                    immatricolazione.ESITO = false;
                    immatricolazione.ANAGPERS = null;

                }


            }

        }
        private bool SetPropertyForInsertImmatricolazione(XR_IMM_IMMATRICOLAZIONI immatricolazione, int codiceUnitaOrg, string cod_tpcont)
        {
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
            immatricolazione.COD_USER = codUser;
            immatricolazione.COD_TERMID = codTermId;
            immatricolazione.TMS_TIMESTAMP = tmsTimestamp;
            if (immatricolazione.ANAGPERS != null)
            {

                immatricolazione.ANAGPERS.ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault();
                immatricolazione.ANAGPERS.DES_COGNOMEPERS = immatricolazione.DES_COGNOMEPERS.ToUpper();
                immatricolazione.ANAGPERS.DTA_NASCITAPERS = immatricolazione.DTA_NASCITAPERS;
                immatricolazione.ANAGPERS.DES_NOMEPERS = immatricolazione.DES_NOMEPERS.ToUpper();
                immatricolazione.ANAGPERS.CSF_CFSPERSONA = immatricolazione.CSF_CFSPERSONA.ToUpper();
                immatricolazione.ANAGPERS.COD_TERMID = immatricolazione.COD_TERMID;
                immatricolazione.ANAGPERS.COD_USER = immatricolazione.COD_USER;
                immatricolazione.ANAGPERS.TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP;
                immatricolazione.ANAGPERS.COD_CITTA = immatricolazione.COD_CITTA;
                immatricolazione.ANAGPERS.COD_PLANNING = "A";
                immatricolazione.ANAGPERS.IND_DOMICILIO = "N";
                immatricolazione.ANAGPERS.IND_PATENTE = "N";
                //NC - Da verificare
                //immatricolazione.ANAGPERS.FLG_ALLERGIES = "N"; 
                //immatricolazione.ANAGPERS.FLG_DIABETIC = "N";
                immatricolazione.ANAGPERS.COD_STCIV = "---";
                immatricolazione.ANAGPERS.IND_RECAPITO = "N";
                immatricolazione.ANAGPERS.COD_SESSO = immatricolazione.COD_SESSO;
                immatricolazione.ANAGPERS.DES_FOTO = "P00" + immatricolazione.COD_MATDIP;


            }
            immatricolazione.ANAGPERS.CITTAD.Add(new CITTAD()
            {
                ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault(),
                DTA_INIZIO = immatricolazione.DTA_INIZIO,
                IND_CITTADPRIM = "Y",
                COD_CITTADPERS = immatricolazione.COD_CITTAD,
                COD_USER = immatricolazione.COD_USER,
                COD_TERMID = immatricolazione.COD_TERMID,
                TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP,
                DTA_FINE = immatricolazione.DTA_FINE
            });
            immatricolazione.ANAGPERS.ASSQUAL.Add(new ASSQUAL()
            {
                ID_ASSQUAL = assqualRepository.GeneraOid(x => x.ID_ASSQUAL),
                COD_QUALIFICA = immatricolazione.COD_QUALIFICA,
                DTA_INIZIO = immatricolazione.DTA_INIZIO,
                DTA_FINE = immatricolazione.DTA_FINE,
                ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault(),
                COD_USER = immatricolazione.COD_USER,
                COD_TERMID = immatricolazione.COD_TERMID,
                TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP,
                COD_EVQUAL = "ND",
                IND_AUTOM = "N",
            });
            /*  immatricolazione.ANAGPERS.ASSTPCONTR.Add(new ASSTPCONTR()
              {
                  ID_ASSTPCONTR = assTpContrattoRepository.GeneraOid(x => x.ID_ASSTPCONTR),
                  ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault(),
                  TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP,
                  COD_TERMID = immatricolazione.COD_TERMID,
                  COD_USER = immatricolazione.COD_USER,
                  COD_EVCNTR = "ND",
                  DTA_INIZIO = immatricolazione.DTA_INIZIO,
                  DTA_FINE = immatricolazione.DTA_FINE,
                  COD_TPCNTR = immatricolazione.COD_TPCNTR=="TI" ? "9" : immatricolazione.COD_TPCNTR=="TD" ? "1" : "14"
              });*/
            immatricolazione.ANAGPERS.INCARLAV.Add(new INCARLAV()
            {
                ID_INCARLAV = incarlavRepository.GeneraOid(x => x.ID_INCARLAV),
                ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault(),
                DTA_INIZIO = immatricolazione.DTA_INIZIO,
                DTA_FINE = immatricolazione.DTA_FINE,
                COD_TERMID = immatricolazione.COD_TERMID,
                COD_USER = immatricolazione.COD_USER,
                TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP,
                IND_INCPRINC = "Y",
                PRC_PERCOCCUPAZ = 100,
                ID_UNITAORG = codiceUnitaOrg,
            });
            immatricolazione.ANAGPERS.TRASF_SEDE.Add(new TRASF_SEDE()
            {
                ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault(),
                ID_TRASF_SEDE = trasfSedeRepository.GeneraOid(x => x.ID_TRASF_SEDE),
                DTA_INIZIO = immatricolazione.DTA_INIZIO,
                DTA_FINE = immatricolazione.DTA_FINE,
                COD_USER = immatricolazione.COD_USER,
                COD_TERMID = immatricolazione.COD_TERMID,
                TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP,
                COD_SEDE = immatricolazione.COD_SEDE,
                COD_IMPRESA = "0",
                COD_EVTRASF = "ND",
            });
            immatricolazione.ANAGPERS.XR_SERVIZIO.Add(new XR_SERVIZIO()
            {
                ID_XR_SERVIZIO = servizioRepository.GeneraOid(x => x.ID_XR_SERVIZIO),
                ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault(),
                COD_SERVIZIO = immatricolazione.COD_SERVIZIO,
                DTA_INIZIO = immatricolazione.DTA_INIZIO,
                DTA_FINE = immatricolazione.DTA_FINE,
                COD_USER = immatricolazione.COD_USER,
                COD_TERMID = immatricolazione.COD_TERMID,
                TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP,
                COD_EVSERVIZIO = "ND",
            });
            immatricolazione.ANAGPERS.COMPREL.Add(new COMPREL()
            {
                ID_COMPREL = db.COMPREL.GeneraPrimaryKey(9),
                ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault(),
                DTA_INIZIO = immatricolazione.DTA_INIZIO,
                DTA_FINE = immatricolazione.DTA_FINE,
                COD_USER = immatricolazione.COD_USER,
                COD_TERMID = immatricolazione.COD_TERMID,
                TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP,
                COD_ATTIVRLAV = "ND",
                COD_TIPORLAV = cod_tpcont,
                COD_IMPRESA = immatricolazione.COD_IMPRESA,
                DTA_ANZCONV = immatricolazione.DTA_INIZIO,
                DTA_HIRE = immatricolazione.DTA_INIZIO,
                COD_MATLIBROMAT = immatricolazione.COD_MATDIP,
                COD_CID = immatricolazione.COD_MATDIP,
                //COD_IMMATRICOLAZIONE = "N"
                //NC - Da verificare
                JOMPREL = new JOMPREL()
                {
                    COD_IMMATRICOLAZIONE = "N"
                }
            });
            JOBASSIGN jobassign = new JOBASSIGN();
            var oldjobassign = db.JOBASSIGN.FirstOrDefault(x => x.ID_PERSONA == immatricolazione.ID_PERSONA && x.IND_INCPRINC == "Y");
            jobassign.COD_RUOLO = immatricolazione.COD_RUOLO;
            jobassign.ID_PERSONA = immatricolazione.ID_PERSONA.Value;
            jobassign.IND_INCPRINC = "Y";
            jobassign.DTA_INIZIO = immatricolazione.DTA_INIZIO;
            jobassign.DTA_FINE = immatricolazione.DTA_FINE;
            jobassign.COD_USER = immatricolazione.COD_USER;
            jobassign.COD_TERMID = immatricolazione.COD_TERMID;
            jobassign.TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP;
            if (oldjobassign != null)
            {
                db.JOBASSIGN.Remove(oldjobassign);
                jobassign.ID_JOBASSIGN = oldjobassign.ID_JOBASSIGN;
            }
            else
            {
                jobassign.ID_JOBASSIGN = db.JOBASSIGN.GeneraPrimaryKey(9);
            }
            db.JOBASSIGN.Add(jobassign);

            return anagraficaRepository.Add(immatricolazione.ANAGPERS);
        }
        private void PrepareToEdit(XR_IMM_IMMATRICOLAZIONI immatricolazione, string operazione)
        {
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
            immatricolazione.COD_USER = codUser;
            immatricolazione.COD_TERMID = codTermId;
            immatricolazione.TMS_TIMESTAMP = tmsTimestamp;
            var dataFineForCics = "";
            var cod_tpcontr = immatricolazione.COD_TIPORLAV;
            var entityOld = immatricolazioneRepository.GetByIdWithRelation(immatricolazione.ID_EVENTO);
            // immatricolazione.COD_USER = CommonHelper.GetCurrentUserMatricola();
            // immatricolazione.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            immatricolazione.TIPO_OPERAZIONE = operazione;
            immatricolazione.DTA_IMM_CREAZIONE = entityOld.DTA_IMM_CREAZIONE;
            immatricolazione.P_MATRICOLA = entityOld.P_MATRICOLA;
            immatricolazione.ID_EVENTO = immatricolazioneRepository.GeneraOid(x => x.ID_EVENTO);
            string tipoRapporto = immatricolazione.COD_TIPORLAV;
            SetPropertyForCics(immatricolazione);
            var codiceUnitaOrg = unitaOrgRepository.GetBySezione(entityOld.COD_SEZIONE);

            if (immatricolazione.DTA_FINE.ToShortDateString().Equals(AnagraficaManager.GetDateLimitMax().ToShortDateString()))
                dataFineForCics = "        ";
            else
            {
                dataFineForCics = immatricolazione.DTA_FINE.ToString("yyyyMMdd");

            }
            var pMatricola = (CommonHelper.GetCurrentUserPMatricola());
            string rispostaCics = ImmatricolazioneCicsManager.ChiamataCics(immatricolazione, pMatricola, dataFineForCics, operazione);
            immatricolazione.COD_TIPORLAV = tipoRapporto;
            if (rispostaCics.Length < 10)
            {
                immatricolazione.RISPOSTA_CICS = rispostaCics;
                if (rispostaCics.Equals("ACK99"))
                {
                    immatricolazione.ESITO_CICS = false;
                    immatricolazione.ESITO = false;
                }
            }
            else
            {
                immatricolazione.RISPOSTA_CICS = rispostaCics.Substring(160, rispostaCics.Length - 160);
                if (rispostaCics.Substring(160, 3) == "000")
                {
                    immatricolazione.ESITO_CICS = true;
                    if (!SetPropertyForEditImmatricolazione(entityOld, immatricolazione, codiceUnitaOrg))
                    {

                        string tipoRapportoold = entityOld.COD_TIPORLAV;
                        SetPropertyForCics(entityOld);
                        var codiceUnitaOrgold = unitaOrgRepository.GetBySezione(entityOld.COD_SEZIONE);

                        if (entityOld.DTA_FINE.ToShortDateString().Equals(AnagraficaManager.GetDateLimitMax().ToShortDateString()))
                            dataFineForCics = "        ";
                        else
                        {
                            dataFineForCics = entityOld.DTA_FINE.ToString("yyyyMMdd");

                        }
                        rispostaCics = ImmatricolazioneCicsManager.ChiamataCics(entityOld, pMatricola, dataFineForCics, operazione);
                        immatricolazione.ESITO_RIPRISTINO = rispostaCics.Length >= 163 && rispostaCics.Substring(160, 3) == "000";
                        immatricolazione.RISPOSTA_RIPRISTINO = rispostaCics;
                        immatricolazione.ESITO = false;
                    }
                    else
                        immatricolazione.ESITO = true;
                }
                else
                {
                    immatricolazione.ESITO_CICS = false;
                    immatricolazione.ESITO = false;
                }
            }
            /*     if (rispostaCics.Length < 10)
            {
               if (rispostaCics.Equals("ACK99")) { immatricolazione.ESITO_CICS = false; }
            }

            if (rispostaCics.Substring(160, 3).Equals("000"))
            {
                immatricolazione.ESITO_CICS = true;
                SetPropertyForEditImmatricolazione(entityOld, immatricolazione, codiceUnitaOrg);
                immatricolazione.COD_TIPORLAV = cod_tpcontr;
            } else
            { immatricolazione.ESITO_CICS = false; }
            immatricolazione.RISPOSTA_CICS = rispostaCics;*/
        }
        private bool SetPropertyForEditImmatricolazione(XR_IMM_IMMATRICOLAZIONI entityOld, XR_IMM_IMMATRICOLAZIONI immatricolazione, int codiceUnitaOrg)
        {
            bool cambioDataInizio = entityOld.DTA_INIZIO != immatricolazione.DTA_INIZIO;
            bool cambioDataFine = entityOld.DTA_FINE != immatricolazione.DTA_FINE;
            if (entityOld.ANAGPERS.ASSQUAL.SingleOrDefault().COD_QUALIFICA != immatricolazione.COD_QUALIFICA || cambioDataInizio || cambioDataFine)
            {
                var item = entityOld.ANAGPERS.ASSQUAL.SingleOrDefault();

                // foreach (var item in entityOld.ANAGPERS.ASSQUAL)
                // {
                item.COD_TERMID = immatricolazione.COD_TERMID;
                item.COD_USER = immatricolazione.COD_USER;
                item.TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP;
                item.ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault();
                item.COD_QUALIFICA = immatricolazione.COD_QUALIFICA;
                item.DTA_INIZIO = immatricolazione.DTA_INIZIO;
                item.DTA_FINE = immatricolazione.DTA_FINE;
                assqualRepository.Update(item);
                // }
            }
            /*
            foreach (var item in entityOld.ANAGPERS.ASSTPCONTR)
            {
                item.ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault(); ;
                item.COD_TPCNTR = immatricolazione.COD_TPCNTR == "TI" ? "9" : immatricolazione.COD_TPCNTR == "TD" ? "1" : "14";
                item.COD_USER = immatricolazione.COD_USER;
                item.COD_TERMID = immatricolazione.COD_TERMID;
                item.TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP;
                item.DTA_INIZIO = immatricolazione.DTA_INIZIO;
                item.DTA_FINE = immatricolazione.DTA_FINE;

            }*/
            if (entityOld.ANAGPERS.INCARLAV.SingleOrDefault().ID_UNITAORG != codiceUnitaOrg || cambioDataInizio || cambioDataFine)
            {
                var item = entityOld.ANAGPERS.INCARLAV.SingleOrDefault();
                //     foreach (var item in entityOld.ANAGPERS.INCARLAV)
                // {
                item.ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault();
                item.COD_USER = immatricolazione.COD_USER;
                item.COD_TERMID = immatricolazione.COD_TERMID;
                item.TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP;
                item.DTA_INIZIO = immatricolazione.DTA_INIZIO;
                item.DTA_FINE = immatricolazione.DTA_FINE;
                item.ID_UNITAORG = codiceUnitaOrg;
                incarlavRepository.Update(item);
            }
            if (entityOld.ANAGPERS.TRASF_SEDE.SingleOrDefault().COD_IMPRESA != immatricolazione.COD_IMPRESA || entityOld.ANAGPERS.TRASF_SEDE.SingleOrDefault().COD_SEDE != immatricolazione.COD_SEDE || cambioDataInizio || cambioDataFine)
            {
                var item = entityOld.ANAGPERS.TRASF_SEDE.SingleOrDefault();
                //  foreach (var item in entityOld.ANAGPERS.TRASF_SEDE)
                //{
                item.ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault();
                item.COD_USER = immatricolazione.COD_USER;
                item.COD_TERMID = immatricolazione.COD_TERMID;
                item.TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP;
                item.DTA_INIZIO = immatricolazione.DTA_INIZIO;
                item.DTA_FINE = immatricolazione.DTA_FINE;
                item.COD_IMPRESA = "0";
                item.COD_SEDE = immatricolazione.COD_SEDE;
                trasfSedeRepository.Update(item);
            }
            if (entityOld.ANAGPERS.XR_SERVIZIO.SingleOrDefault().COD_SERVIZIO != immatricolazione.COD_SERVIZIO || cambioDataInizio || cambioDataFine)
            {
                var item = entityOld.ANAGPERS.XR_SERVIZIO.SingleOrDefault();
                //  foreach (var item in entityOld.ANAGPERS.XR_SERVIZIO)
                // {
                item.ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault(); ;
                item.COD_USER = immatricolazione.COD_USER;
                item.COD_TERMID = immatricolazione.COD_TERMID;
                item.TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP;
                item.DTA_INIZIO = immatricolazione.DTA_INIZIO;
                item.DTA_FINE = immatricolazione.DTA_FINE;
                item.COD_SERVIZIO = immatricolazione.COD_SERVIZIO;
                servizioRepository.Update(item);
            }
            if (entityOld.ANAGPERS.COMPREL.SingleOrDefault().COD_IMPRESA != immatricolazione.COD_IMPRESA || entityOld.ANAGPERS.COMPREL.SingleOrDefault().COD_TIPORLAV != immatricolazione.COD_TIPORLAV || cambioDataInizio || cambioDataFine)
            {
                var item = entityOld.ANAGPERS.COMPREL.SingleOrDefault();
                //  foreach (var item in entityOld.ANAGPERS.COMPREL)
                // {
                item.ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault(); ;
                item.COD_USER = immatricolazione.COD_USER;
                item.COD_TERMID = immatricolazione.COD_TERMID;
                item.TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP;
                item.DTA_INIZIO = immatricolazione.DTA_INIZIO;
                item.DTA_FINE = immatricolazione.DTA_FINE;
                item.DTA_HIRE = immatricolazione.DTA_INIZIO;
                item.DTA_ANZCONV = immatricolazione.DTA_INIZIO;
                item.COD_IMPRESA = immatricolazione.COD_IMPRESA;
                item.COD_TIPORLAV = immatricolazione.COD_TIPORLAV;
                comprelRepository.Update(item);
            }
            if (!entityOld.ANAGPERS.CITTAD.Any() || entityOld.ANAGPERS.CITTAD.FirstOrDefault().COD_CITTADPERS != immatricolazione.COD_CITTAD || cambioDataInizio || cambioDataFine)
            {
                var item = entityOld.ANAGPERS.CITTAD.FirstOrDefault();
                //    CITTAD cittadUpdateRecord = entityOld.ANAGPERS.CITTAD.AsEnumerable().FirstOrDefault();
                // if (cittadUpdateRecord.COD_CITTADPERS != immatricolazione.COD_CITTAD)
                //{
                if (item != null)
                    db.CITTAD.Remove(item);
                CITTAD itemn = new CITTAD();
                // db.Set<CITTAD>().Remove(item); // .Attach(cittadUpdateRecord);
                //foreach (var item in entityOld.ANAGPERS.CITTAD)
                // {
                itemn.ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault();
                itemn.COD_USER = immatricolazione.COD_USER;
                itemn.COD_TERMID = immatricolazione.COD_TERMID;
                itemn.TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP;
                // item.DTA_INIZIO = immatricolazione.DTA_INIZIO;
                itemn.DTA_FINE = immatricolazione.DTA_FINE;
                itemn.IND_CITTADPRIM = "Y";
                itemn.COD_CITTADPERS = immatricolazione.COD_CITTAD;
                itemn.DTA_INIZIO = DateTime.Today;
                // }
                // db.Set<CITTAD>().Add(item);
                db.CITTAD.Add(itemn);
                db.Entry(itemn).State = System.Data.EntityState.Added;
            }
            //  ASSQUAL assqualUpdateRecord = entityOld.ANAGPERS.ASSQUAL.AsEnumerable().FirstOrDefault();
            //  ASSTPCONTR asstpcontrUpdateRecord = entityOld.ANAGPERS.ASSTPCONTR.AsEnumerable().FirstOrDefault();
            //  INCARLAV incarlavUpdateRecord = entityOld.ANAGPERS.INCARLAV.AsEnumerable().FirstOrDefault();
            //  TRASF_SEDE trasfsedeUpdateRecord = entityOld.ANAGPERS.TRASF_SEDE.AsEnumerable().FirstOrDefault();
            //  XR_SERVIZIO servizioUpdateRecord = entityOld.ANAGPERS.XR_SERVIZIO.AsEnumerable().FirstOrDefault();
            //  COMPREL comprelUpdateRecord = entityOld.ANAGPERS.COMPREL.AsEnumerable().FirstOrDefault();
            var old = entityOld.ANAGPERS;
            entityOld.ANAGPERS.COD_CITTA = immatricolazione.COD_CITTA;
            //entityOld.ANAGPERS.COD_MATDIP = immatricolazione.COD_MATDIP;
            entityOld.ANAGPERS.COD_SESSO = immatricolazione.COD_SESSO;
            entityOld.ANAGPERS.CSF_CFSPERSONA = immatricolazione.CSF_CFSPERSONA;
            entityOld.ANAGPERS.DES_COGNOMEPERS = immatricolazione.DES_COGNOMEPERS;
            entityOld.ANAGPERS.DES_NOMEPERS = immatricolazione.DES_NOMEPERS;
            entityOld.ANAGPERS.DES_SECCOGNOME = immatricolazione.DES_SECCOGNOME;
            entityOld.ANAGPERS.ID_PERSONA = immatricolazione.ID_PERSONA.GetValueOrDefault();
            entityOld.ANAGPERS.DTA_NASCITAPERS = immatricolazione.DTA_NASCITAPERS;
            if (old != entityOld.ANAGPERS || cambioDataInizio || cambioDataFine)
            {
                entityOld.ANAGPERS.COD_TERMID = immatricolazione.COD_TERMID;
                entityOld.ANAGPERS.COD_USER = immatricolazione.COD_USER;
                entityOld.ANAGPERS.TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP;
                ANAGPERS anagUpdateRecord = entityOld.ANAGPERS;
                anagraficaRepository.Update(anagUpdateRecord);
            }
            //  db.Set<CITTAD>().Attach(cittadUpdateRecord);
            //  db.Entry(cittadUpdateRecord).State = System.Data.EntityState.Modified;
            // assqualRepository.Update(assqualUpdateRecord);
            // assTpContrattoRepository.Update(asstpcontrUpdateRecord);
            //  incarlavRepository.Update(incarlavUpdateRecord);
            //   trasfSedeRepository.Update(trasfsedeUpdateRecord);
            //  servizioRepository.Update(servizioUpdateRecord);
            //  comprelRepository.Update(comprelUpdateRecord);

            JOBASSIGN jobassign = new JOBASSIGN();
            var oldjobassign = db.JOBASSIGN.FirstOrDefault(x => x.ID_PERSONA == immatricolazione.ID_PERSONA && x.IND_INCPRINC == "Y");
            jobassign.COD_RUOLO = immatricolazione.COD_RUOLO;
            jobassign.ID_PERSONA = immatricolazione.ID_PERSONA.Value;
            jobassign.IND_INCPRINC = "Y";
            jobassign.DTA_INIZIO = immatricolazione.DTA_INIZIO;
            jobassign.DTA_FINE = immatricolazione.DTA_FINE;
            jobassign.COD_USER = immatricolazione.COD_USER;
            jobassign.COD_TERMID = immatricolazione.COD_TERMID;
            jobassign.TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP;
            if (oldjobassign != null)
            {
                db.JOBASSIGN.Remove(oldjobassign);
                jobassign.ID_JOBASSIGN = oldjobassign.ID_JOBASSIGN;
            }
            else
            {
                jobassign.ID_JOBASSIGN = db.JOBASSIGN.GeneraPrimaryKey(9);
            }
            db.JOBASSIGN.Add(jobassign);
            var oldjnoty = db.JOBASSIGN.Where(x => x.ID_PERSONA == immatricolazione.ID_PERSONA && x.IND_INCPRINC != "Y" && x.DTA_INIZIO == entityOld.DTA_INIZIO).ToList();
            BaseRepository<JOBASSIGN> bj = new BaseRepository<JOBASSIGN>(db);

            foreach (var oldjb in oldjnoty)
            {
                oldjb.DTA_INIZIO = immatricolazione.DTA_INIZIO;
                oldjb.DTA_FINE = immatricolazione.DTA_FINE;
                oldjb.COD_USER = immatricolazione.COD_USER;
                oldjb.COD_TERMID = immatricolazione.COD_TERMID;
                oldjb.TMS_TIMESTAMP = immatricolazione.TMS_TIMESTAMP;
                bj.Update(oldjb);
            }


            RESIDENZA residenza = db.RESIDENZA.Where(x => x.ID_PERSONA == immatricolazione.ID_PERSONA && x.DTA_INIZIO == entityOld.DTA_INIZIO).FirstOrDefault();
            if (residenza != null)
            {

                IndirizzoModel indirizzo = new IndirizzoModel();

                indirizzo.LoadFromDB(residenza);
                db.RESIDENZA.Remove(residenza);
                db.JESIDENZA.RemoveWhere(x => x.ID_PERSONA == immatricolazione.ID_PERSONA && x.DTA_INIZIO == entityOld.DTA_INIZIO);

                indirizzo.Decorrenza = immatricolazione.DTA_INIZIO;
                indirizzo.IsNew = true;
                indirizzo.IgnoraUltimoRecord = true;
                indirizzo.G_CambioRes = immatricolazione.DTA_INIZIO;
                AnagraficaManager.Save_DatiIndirizzo(db, indirizzo, out string err, out List<Func<bool>> postActions);
                foreach (Func<bool> fun in postActions)
                {
                    fun();
                }
            }
            var rectutor = db.XR_TUTOR.Where(x => x.ID_PERSONA == entityOld.ID_PERSONA && x.DTA_INIZIO == entityOld.DTA_INIZIO).ToList();
            foreach (var record in rectutor)
            {
                record.DTA_INIZIO = immatricolazione.DTA_INIZIO;
                CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
                record.COD_USER = codUser;
                record.COD_TERMID = codTermId;
                record.TMS_TIMESTAMP = tmsTimestamp;
            }


            return DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
        }
        private void InsertImmatricolazioneTipoOperazione_A(XR_IMM_IMMATRICOLAZIONI immatricolazione, string operazione, XR_IMM_IMMATRICOLAZIONI immatricolazioneOld)
        {
            immatricolazione.ID_PERSONA = immatricolazioneOld.ID_PERSONA.GetValueOrDefault();
            immatricolazione.ID_EVENTO = immatricolazioneRepository.GeneraOid(x => x.ID_EVENTO);
            immatricolazione.TIPO_OPERAZIONE = operazione;
            immatricolazione.ID_PERSONA = immatricolazioneOld.ID_PERSONA;
            immatricolazione.DTA_IMM_CREAZIONE = immatricolazioneOld.DTA_IMM_CREAZIONE;
            immatricolazione.TMS_TIMESTAMP = DateTime.Now;
            immatricolazione.COD_USER = CommonHelper.GetCurrentUserMatricola();
            immatricolazione.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            immatricolazione.P_MATRICOLA = immatricolazioneOld.P_MATRICOLA;
            immatricolazione.COD_CITTA = immatricolazioneOld.COD_CITTA;
            immatricolazione.COD_IMPRESA = immatricolazioneOld.COD_IMPRESA;
            immatricolazione.COD_MATDIP = immatricolazioneOld.COD_MATDIP;
            immatricolazione.COD_QUALIFICA = immatricolazioneOld.COD_QUALIFICA;
            immatricolazione.COD_SEDE = immatricolazioneOld.COD_SEDE;
            immatricolazione.COD_SERVIZIO = immatricolazioneOld.COD_SERVIZIO;
            immatricolazione.COD_SESSO = immatricolazioneOld.COD_SESSO;
            immatricolazione.COD_SEZIONE = immatricolazioneOld.COD_SEZIONE;
            immatricolazione.COD_TIPORLAV = immatricolazioneOld.COD_TIPORLAV;
            immatricolazione.CSF_CFSPERSONA = immatricolazioneOld.CSF_CFSPERSONA;
            immatricolazione.DES_COGNOMEPERS = immatricolazioneOld.DES_COGNOMEPERS;
            immatricolazione.DES_NOMEPERS = immatricolazioneOld.DES_NOMEPERS;
            immatricolazione.DES_SECCOGNOME = immatricolazioneOld.DES_SECCOGNOME;
            immatricolazione.DTA_FINE = immatricolazioneOld.DTA_FINE;
            immatricolazione.DTA_INIZIO = immatricolazioneOld.DTA_INIZIO;
            immatricolazione.DTA_NASCITAPERS = immatricolazioneOld.DTA_NASCITAPERS;
            immatricolazione.COD_RUOLO = immatricolazioneOld.COD_RUOLO;
        }
        public XR_IMM_IMMATRICOLAZIONI PrepareToDelete(int id)
        {
            var datafineforcics = "";
            var operazione = "A";
            //XR_IMM_IMMATRICOLAZIONE
            var immatricolazioneOld = immatricolazioneRepository.GetById(id);
            var idPersona = immatricolazioneOld.ID_PERSONA;
            //var viewmodel = ImmatricolazioniVM.ConverToImmatricolazioneVM(immatricolazioneOld);
            //var tmp = viewmodel.converttoxr_imm_immatricolazionewithrelation();
            //xr_imm_immatricolazioni entityold = immatricolazionerepository.getbyidwithrelation(tmp.id_evento);
            //insert record Tipo operazione A in XR_IMM_IMMATRICOLAZIONE
            XR_IMM_IMMATRICOLAZIONI immatricolazione = new XR_IMM_IMMATRICOLAZIONI();
            InsertImmatricolazioneTipoOperazione_A(immatricolazione, operazione, immatricolazioneOld);
            if (immatricolazioneOld.DTA_FINE.ToShortDateString().Equals(AnagraficaManager.GetDateLimitMax().ToShortDateString()))
                datafineforcics = "        ";
            else
                datafineforcics = immatricolazioneOld.DTA_FINE.ToString("yyyyMMdd");
            var currentDate = DateTime.Now;
            SetPropertyForCics(immatricolazioneOld);
            var pMatricola = (CommonHelper.GetCurrentUserPMatricola());
            bool callCics = immatricolazioneOld.TIPO_OPERAZIONE == "A" || !immatricolazioneOld.ESITO_CICS;
            string rispostaCics = "";
            if (!callCics)
                rispostaCics = ImmatricolazioneCicsManager.ChiamataCics(immatricolazioneOld, pMatricola, datafineforcics, operazione);
            else
                rispostaCics = immatricolazioneOld.RISPOSTA_CICS;

            if (callCics && rispostaCics.Length < 10)
            {
                immatricolazione.RISPOSTA_CICS = rispostaCics;
                if (rispostaCics.Equals("ACK99"))
                {
                    immatricolazione.ESITO_CICS = false;
                    immatricolazione.ESITO = false;
                }
            }
            else
            {
                string codRisp = "";
                if (!callCics)
                {
                    immatricolazione.RISPOSTA_CICS = rispostaCics.Substring(160, rispostaCics.Length - 160);
                    codRisp = rispostaCics.Substring(160, 3);
                }
                else
                {
                    immatricolazione.RISPOSTA_CICS = immatricolazioneOld.RISPOSTA_CICS;
                    codRisp = immatricolazione.RISPOSTA_CICS.Substring(0, 3);
                }

                //immatricolazione.RISPOSTA_CICS = rispostaCics.Substring(160, rispostaCics.Length - 160);
                //if (rispostaCics.Substring(160, 3) == "000")
                if (codRisp == "000" && immatricolazione.RISPOSTA_CICS.Contains("000MATRICOLA CANCELLATA"))
                {
                    immatricolazione.ESITO_CICS = true;

                    if (!DeleteImmatricolazioneDaTalentia(immatricolazioneOld.ID_PERSONA))
                    {
                        immatricolazione.ESITO = false;
                    }
                    else
                    {
                        immatricolazione.ESITO = true;
                        immatricolazione.ID_PERSONA = null;
                        var immod = db.XR_IMM_IMMATRICOLAZIONI.Where(x => x.ID_PERSONA == idPersona).ToList();
                        foreach (XR_IMM_IMMATRICOLAZIONI item in immod)
                        {
                            item.ID_PERSONA = null;
                            db.Set<XR_IMM_IMMATRICOLAZIONI>().Attach(item);
                            db.Entry(item).State = System.Data.EntityState.Modified;


                        }
                    }
                    //immatricolazioneRepository.Save();
                }
                else
                {
                    immatricolazione.ESITO_CICS = false;
                    immatricolazione.ESITO = false;
                }
            }
            immatricolazioneRepository.Add(immatricolazione);
            var oldjobassign = db.JOBASSIGN.FirstOrDefault(x => x.ID_PERSONA == id && x.IND_INCPRINC == "Y");
            if (oldjobassign != null)
                db.JOBASSIGN.Remove(oldjobassign);
            return immatricolazione;
        }
        private bool DeleteImmatricolazioneDaTalentia(int? id)
        {
            /*
            ASSQUAL entityObjectAssqual = db.ASSQUAL.FirstOrDefault(s => s.ID_PERSONA == id);
            db.ASSQUAL.Remove(entityObjectAssqual);
            //assqualRepository.Delete(entityObjectAssqual);
            ASSTPCONTR entityObjectAsstpcontr = db.ASSTPCONTR.FirstOrDefault(s => s.ID_PERSONA == id);
            db.ASSTPCONTR.Remove(entityObjectAsstpcontr);
            //assTpContrattoRepository.Delete(entityObjectAsstpcontr);
            INCARLAV entityObjectIncarlav = db.INCARLAV.FirstOrDefault(s => s.ID_PERSONA == id);
            db.INCARLAV.Remove(entityObjectIncarlav);
            //incarlavRepository.Delete(entityObjectIncarlav);
            TRASF_SEDE entityObjectTrasfSede = db.TRASF_SEDE.FirstOrDefault(s => s.ID_PERSONA == id);
            db.TRASF_SEDE.Remove(entityObjectTrasfSede);
            //trasfSedeRepository.Delete(entityObjectTrasfSede);
            XR_SERVIZIO entityObjectServizio = db.XR_SERVIZIO.FirstOrDefault(s => s.ID_PERSONA == id);
            db.XR_SERVIZIO.Remove(entityObjectServizio);
            //servizioRepository.Delete(entityObjectServizio);
            COMPREL entityObjectComprel = db.COMPREL.FirstOrDefault(s => s.ID_PERSONA == id);
            db.COMPREL.Remove(entityObjectComprel);
            //comprelRepository.Delete(entityObjectComprel);
            CITTAD entityObjectCittad = db.CITTAD.FirstOrDefault(s => s.ID_PERSONA == id);
            db.CITTAD.Remove(entityObjectCittad);*/


            ANAGPERS entityObjectAnagrafica = db.ANAGPERS.FirstOrDefault(s => s.ID_PERSONA == id);

            return anagraficaRepository.Delete(entityObjectAnagrafica);
        }


        public void SetPropertyForCics(XR_IMM_IMMATRICOLAZIONI immatricolazione)
        {
            if (immatricolazione.ID_PERSONA == 0)
            {
                immatricolazione.ID_PERSONA = db.XR_IMM_IMMATRICOLAZIONI.GeneraPrimaryKey(9);
                //  immatricolazione.ID_PERSONA = immatricolazioneRepository.GeneraOid(x => x.ID_PERSONA.GetValueOrDefault());

            }
            if (immatricolazione.COD_TIPORLAV == "TI" && !immatricolazione.COD_QUALIFICA.StartsWith("A"))
            {
                immatricolazione.COD_TIPORLAV = "1";
                immatricolazione.DTA_FINE = AnagraficaManager.GetDateLimitMax();
            }
            else if (immatricolazione.COD_TIPORLAV == "TI" && immatricolazione.COD_QUALIFICA.StartsWith("A"))
            {
                immatricolazione.COD_TIPORLAV = "3";
                immatricolazione.DTA_FINE = AnagraficaManager.GetDateLimitMax();
            }
            else if (immatricolazione.COD_TIPORLAV == "TD" && immatricolazione.COD_QUALIFICA.StartsWith("A"))
                immatricolazione.COD_TIPORLAV = "4";
            else if (immatricolazione.COD_TIPORLAV == "TD" && !immatricolazione.COD_QUALIFICA.StartsWith("A"))
                immatricolazione.COD_TIPORLAV = "2";
            else if (immatricolazione.COD_TIPORLAV == "TI" && immatricolazione.COD_QUALIFICA.StartsWith("A"))
            {
                immatricolazione.COD_TIPORLAV = "3";
                immatricolazione.DTA_FINE = AnagraficaManager.GetDateLimitMax();
            }
            else if (immatricolazione.COD_TIPORLAV == "CO" && immatricolazione.COD_QUALIFICA == "J21")
                immatricolazione.COD_TIPORLAV = "6";
            else if (immatricolazione.COD_TIPORLAV == "CO" && immatricolazione.COD_QUALIFICA == "J01" || immatricolazione.COD_QUALIFICA == "J11")
                immatricolazione.COD_TIPORLAV = "7";

            if (string.IsNullOrEmpty(immatricolazione.DES_SECCOGNOME))
            {
                immatricolazione.DES_SECCOGNOME = " ";
            }
        }
        #endregion


        internal ImmatricolazioniVM GetDettaglioImmatricolazione(int? id)
        {
            ImmatricolazioniVM viewmodel;
            XR_IMM_IMMATRICOLAZIONI immatricolazione = immatricolazioneRepository.GetById(id);
            viewmodel = ImmatricolazioniVM.ConverToImmatricolazioneVM(immatricolazione);
            gestioneDatiDaVisualizzare.GetDescrizioneDettagli(viewmodel);
            return viewmodel;
        }
    }
}