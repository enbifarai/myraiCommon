using myRaiCommonModel;
using myRaiGestionale.RepositoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRaiData.Incentivi;
using myRaiHelper;
using myRai.DataAccess;
using myRaiHelper.GenericRepository;
using myRaiCommonManager;
using Xceed.Words.NET;
using System.IO;
using System.Data.Entity.Migrations;

namespace myRaiGestionale.Services
{
    public class PianoFormativoServizio
    {
        private IncentiviEntities db;
        private GestioneDatiDaVisualizzare gestioneDatiDaVisualizzare;
        private JobAssignRepository jobAssignRepository;
        private ImmatricolazioneRepository immatricolazioneRepository;
        private AnagraficaRepository anagraficaRepository;
        RicercaPianoFormativo viewModel = new RicercaPianoFormativo();
        string codUser = "";
        string codTermid = "";
        DateTime timestamp;
        public PianoFormativoServizio(IncentiviEntities db)
        {
            this.db = db;
            anagraficaRepository = new AnagraficaRepository(db);
            jobAssignRepository = new JobAssignRepository(db);
            immatricolazioneRepository = new ImmatricolazioneRepository(db);

        }

        public List<RicercaPianoFormativo> GetPianiFormativi()
        {
            string hrisAbil = AuthHelper.HrisFuncAbil(CommonHelper.GetCurrentUserMatricola());
            if (String.IsNullOrWhiteSpace(hrisAbil))
                hrisAbil = "HRCE";

            IQueryable<SINTESI1> tmpSint = null;
            if (hrisAbil=="HRCE")
                tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), CommonHelper.GetCurrentUserMatricola(), null, "HRCE", "APPRENDISTATO");
            else
                tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), CommonHelper.GetCurrentUserMatricola(), null, "HRIS_GEST", "PFIGES");
            List<RicercaPianoFormativo> listaPianiFormativi = new List<RicercaPianoFormativo>();
            //var pianiFormativi = from anag in db.V_XR_XANAGRA
            //                     join lo_imma in db.XR_IMM_IMMATRICOLAZIONI on anag.ID_PERSONA equals lo_imma.ID_PERSONA into i
            //                     from imma in i.DefaultIfEmpty()
            //                     join lo_jobassign in db.JOBASSIGN on anag.ID_PERSONA equals lo_jobassign.ID_PERSONA into j
            //                     from job in j.DefaultIfEmpty()
            //                     join lo_ruolo in db.RUOLO on new { job.COD_RUOLO, COD_RUOLOAGGREG = "PROFORM" } equals new { lo_ruolo.COD_RUOLO, lo_ruolo.COD_RUOLOAGGREG } into r
            //                     from ruolo in r.DefaultIfEmpty()
            //                     join lo_comp in db.COMPREL on anag.ID_PERSONA equals lo_comp.ID_PERSONA into c
            //                     from comp in c.DefaultIfEmpty()
            //                     join lo_causa in db.XR_TB_CAUSALEMOV on comp.JOMPREL.COD_CAUSALEMOV equals lo_causa.COD_CAUSALEMOV into causa
            //                     where imma != null || (ruolo != null && comp.JOMPREL.DES_CAUSALEMOV.StartsWith("APPRENDISTA"))
            //                     orderby anag.ID_PERSONA, imma.TMS_TIMESTAMP descending
            //                     select new RicercaPianoFormativo()
            //                     {
            //                         Nominativo = anag.COGNOME + " " + anag.NOME,
            //                         IdPersona = anag.ID_PERSONA,
            //                         Nome = anag.NOME,
            //                         DataInizioApprendistato = job.DTA_INIZIO,
            //                         DataFineApprendistato = job.DTA_FINE,
            //                         DataImmatricolazione = imma.DTA_IMM_CREAZIONE,
            //                         DataInizioContratto = comp.DTA_HIRE,  //imma.DTA_INIZIO,
            //                         Matricola = anag.MATRICOLA.Equals(null) ? imma.COD_MATDIP : anag.MATRICOLA,
            //                         DescrizioneRuolo = ruolo.COD_RUOLOAGGREG,
            //                         IsRuoloAggreg = ruolo.COD_RUOLOAGGREG == "PROFORM" ? true : false,
            //                         Cessato = comp.DTA_FINE > DateTime.Today ? false : true,
            //                         IdJobAssign = job == null ? 0 : job.ID_JOBASSIGN
            //                     };

            var tmp = db.JOBASSIGN.Where(x => x.RUOLO.COD_RUOLOAGGREG == "PROFORM");

            var pianiFormativi = from anag in db.V_XR_XANAGRA
                                 join lo_imma in db.XR_IMM_IMMATRICOLAZIONI on anag.ID_PERSONA equals lo_imma.ID_PERSONA into i
                                 from imma in i.DefaultIfEmpty()
                                 join lo_jobassign in db.JOBASSIGN.Where(x=>x.RUOLO.COD_RUOLOAGGREG=="PROFORM") on anag.ID_PERSONA equals lo_jobassign.ID_PERSONA into j
                                 from job in j.DefaultIfEmpty()
                                 join lo_comp in db.COMPREL on anag.ID_PERSONA equals lo_comp.ID_PERSONA into c
                                 from comp in c.DefaultIfEmpty()
                                 join lo_causa in db.XR_TB_CAUSALEMOV on comp.JOMPREL.COD_CAUSALEMOV equals lo_causa.COD_CAUSALEMOV into causa
                                 where tmpSint.Select(x => x.COD_MATLIBROMAT).Contains(imma.COD_MATDIP) && (imma != null || (job != null && comp.JOMPREL.DES_CAUSALEMOV.StartsWith("APPRENDISTA")))
                                 orderby anag.ID_PERSONA, imma.TMS_TIMESTAMP descending
                                 select new RicercaPianoFormativo()
                                 {
                                     Nominativo = anag.COGNOME + " " + anag.NOME,
                                     IdPersona = anag.ID_PERSONA,
                                     Nome = anag.NOME,
                                     DataInizioApprendistato = job.DTA_INIZIO,
                                     DataFineApprendistato = job.DTA_FINE,
                                     DataImmatricolazione = imma.DTA_IMM_CREAZIONE,
                                     DataInizioContratto = comp.DTA_HIRE,  //imma.DTA_INIZIO,
                                     Matricola = anag.MATRICOLA.Equals(null) ? imma.COD_MATDIP : anag.MATRICOLA,
                                     DescrizioneRuolo = job!=null?job.RUOLO.COD_RUOLOAGGREG:"",
                                     IsRuoloAggreg = job!=null && job.RUOLO.COD_RUOLOAGGREG == "PROFORM" ? true : false,
                                     Cessato = comp.DTA_FINE > DateTime.Today ? false : true,
                                     IdJobAssign = job == null ? 0 : job.ID_JOBASSIGN
                                 };

            listaPianiFormativi = pianiFormativi
                .DistinctBy(x => x.IdPersona)
                .OrderBy(x => x.Nominativo).ToList();

            return listaPianiFormativi;
        }
        internal List<RicercaPianoFormativo> GetTutor(string v)
        {
            List<RicercaPianoFormativo> listaTutor = new List<RicercaPianoFormativo>();
            listaTutor = db.ANAGPERS.Join(db.XR_TUTOR, x => x.ID_PERSONA, tutor => tutor.ID_PERSONA, (anagrafica, tutor)
            => new { anagrafica, tutor }).Join(db.V_XR_XANAGRA, t => t.tutor.ID_TUTOR, anag => anag.ID_PERSONA, (anag, tutor)

                => new RicercaPianoFormativo()
                {
                    /*IdPersona = anag.anagrafica.ID_PERSONA,*/
                    IdPersona = anag.tutor.ID_TUTOR,
                    SelectedTutor = tutor.MATRICOLA,
                    DataInizioApprendistato = anag.tutor.DTA_INIZIO,
                    DataFineApprendistato = anag.tutor.DTA_FINE,
                    Matricola = anag.anagrafica.JNAGPERS.COD_MATDIP,
                    Nominativo = tutor.COGNOME + " " + tutor.NOME

                }).Where(x => x.Nominativo.StartsWith(v)).DistinctBy(x => x.IdPersona).ToList();
            return listaTutor;
        }
        internal List<TutorPianoFormativoVM> GetTutor(string matricola, string nominativo)
        {
            List<TutorPianoFormativoVM> listaTutor = new List<TutorPianoFormativoVM>();


            // var resultList = db.SINTESI1.Join(db.XR_TUTOR, x => x.ID_PERSONA, tutor => tutor.ID_TUTOR, (sintesi, tutor) => new { sintesi, tutor })
            //    .Select(s => new { s }).ToList();
            listaTutor = db.SINTESI1.Where(x => (!string.IsNullOrEmpty(nominativo) && !string.IsNullOrEmpty(matricola) ? (x.DES_COGNOMEPERS + " " + x.DES_NOMEPERS).StartsWith(nominativo) && x.COD_MATLIBROMAT == matricola : !string.IsNullOrEmpty(nominativo) ? (x.DES_COGNOMEPERS + " " + x.DES_NOMEPERS).StartsWith(nominativo) : x.COD_MATLIBROMAT == matricola)).Select(grp => new TutorPianoFormativoVM()
            {
                IdTutor = grp.ID_PERSONA,
                Nominativo = grp.DES_COGNOMEPERS + " " + grp.DES_NOMEPERS,
                MatricolaTutor = grp.COD_MATLIBROMAT,
                Categoria = grp.DES_QUALIFICA
            }).Distinct().ToList();
            return listaTutor;
        }
        internal TutorPianoFormativoVM GetTutorByMatricola(string matricola)
        {
            TutorPianoFormativoVM tutorSelectedFromModaleAggiuntaTutor = new TutorPianoFormativoVM();
            try
            {
                tutorSelectedFromModaleAggiuntaTutor = db.SINTESI1
                              .Join(db.ASSQUAL, sintesi => sintesi.ID_ASSQUAL, assqual => assqual.ID_ASSQUAL, (sintesi, assqual) => new { sintesi, assqual })
                                    //.Join(db.XR_TUTOR, sintesi => sintesi.sintesi.ID_PERSONA, tutor => tutor.ID_TUTOR, (sintesi1, tutor) => new { sintesi1, tutor })
                                    .Where(w => w.sintesi.COD_MATLIBROMAT == matricola)
                                    .Select(s => new TutorPianoFormativoVM()
                                    {
                                        IdTutor = s.sintesi.ID_PERSONA,
                                        MatricolaTutor = s.sintesi.COD_MATLIBROMAT,
                                        Nominativo = s.sintesi.DES_COGNOMEPERS + " " + s.sintesi.DES_NOMEPERS,
                                        Categoria = s.sintesi.DES_QUALIFICA,
                                        Dal = s.sintesi.DTA_INIZIO_Q,
                                        Al = s.sintesi.DTA_FINE_Q,
                                        IdPersona = s.sintesi.ID_PERSONA,
                                        AnzCategoria = s.sintesi.ASSQUAL.JSSQUAL.DTA_ANZCAT,

                                    }).FirstOrDefault();
            }
            catch (Exception e)
            {
                var bo = e;

            }


            return tutorSelectedFromModaleAggiuntaTutor;
        }
        internal List<RicercaPianoFormativo> FiltraPianiFormativi(string nome, string matricola, string sezione, string tutorID)
        {
            List<RicercaPianoFormativo> pianiFormativiFiltrati = new List<RicercaPianoFormativo>();
            //TODO if query 
            string hrisAbil = AuthHelper.HrisFuncAbil(CommonHelper.GetCurrentUserMatricola());
            if (String.IsNullOrWhiteSpace(hrisAbil))
                hrisAbil = "HRCE";
            IQueryable<SINTESI1> tmpSint = null;
            if (hrisAbil=="HRCE")
                tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), CommonHelper.GetCurrentUserMatricola(), null, "HRCE", "APPRENDISTATO");
            else
                tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), CommonHelper.GetCurrentUserMatricola(), null, "HRIS_GEST", "PFIGES");

            DateTime dtFine = new DateTime(2999, 12, 31);

            if (!String.IsNullOrEmpty(nome) || !String.IsNullOrEmpty(matricola))
            {

                nome = nome.Trim().ToUpper();
                matricola = matricola.Trim();
                //var pianiFormativi = from anag in db.V_XR_XANAGRA
                //                     join lo_imma in db.XR_IMM_IMMATRICOLAZIONI on anag.ID_PERSONA equals lo_imma.ID_PERSONA into i
                //                     from imma in i.DefaultIfEmpty()
                //                     join lo_jobassign in db.JOBASSIGN on anag.ID_PERSONA equals lo_jobassign.ID_PERSONA into j
                //                     from job in j.DefaultIfEmpty()
                //                     join lo_ruolo in db.RUOLO on new { job.COD_RUOLO, COD_RUOLOAGGREG = "PROFORM" } equals new { lo_ruolo.COD_RUOLO, lo_ruolo.COD_RUOLOAGGREG }
                //                         into r
                //                     from ruolo in r.DefaultIfEmpty()
                //                     join lo_comp in db.COMPREL on anag.ID_PERSONA equals lo_comp.ID_PERSONA into c
                //                     from comp in c.DefaultIfEmpty()
                //                     join lo_causa in db.XR_TB_CAUSALEMOV on comp.JOMPREL.COD_CAUSALEMOV equals lo_causa.COD_CAUSALEMOV into causa
                //                     where (imma != null || (ruolo != null && comp.JOMPREL.DES_CAUSALEMOV.StartsWith("APPRENDISTA"))) && (!String.IsNullOrEmpty(nome) && !String.IsNullOrEmpty(matricola) ? (anag.COGNOME + " " + anag.NOME).StartsWith(nome) && (anag.MATRICOLA == matricola || imma.COD_MATDIP == matricola) : !String.IsNullOrEmpty(nome) ? (anag.COGNOME + " " + anag.NOME).StartsWith(nome) : anag.MATRICOLA == matricola || imma.COD_MATDIP == matricola)
                //                     /*  where (imma != null || ruolo != null) && (!String.IsNullOrEmpty(nome) && !String.IsNullOrEmpty(matricola) ? anag.COGNOME.StartsWith(nome) && (anag.MATRICOLA == matricola || imma.COD_MATDIP == matricola) : !String.IsNullOrEmpty(nome) ? anag.COGNOME.StartsWith(nome) : anag.MATRICOLA == matricola || imma.COD_MATDIP == matricola)*/
                //                     orderby anag.ID_PERSONA, imma.TMS_TIMESTAMP
                //                     select new RicercaPianoFormativo()
                //                     {
                //                         Nominativo = anag.COGNOME + " " + anag.NOME,
                //                         IdPersona = anag.ID_PERSONA,
                //                         Nome = anag.NOME,
                //                         DataInizioApprendistato = job.DTA_INIZIO,
                //                         DataFineApprendistato = job.DTA_FINE,
                //                         DataImmatricolazione = imma.DTA_IMM_CREAZIONE,
                //                         DataInizioContratto = comp.DTA_HIRE, //imma.DTA_INIZIO,
                //                         Matricola = anag.MATRICOLA.Equals(null) ? imma.COD_MATDIP : anag.MATRICOLA,
                //                         DescrizioneRuolo = ruolo.COD_RUOLOAGGREG,
                //                         IsRuoloAggreg = ruolo.COD_RUOLOAGGREG == "PROFORM" ? true : false,
                //                         Cessato = comp.DTA_FINE > DateTime.Today ? false : true,
                //                         IdJobAssign = job == null ? 0 : job.ID_JOBASSIGN
                //                     };
                var pianiFormativi = from anag in db.V_XR_XANAGRA
                                     join lo_imma in db.XR_IMM_IMMATRICOLAZIONI on anag.ID_PERSONA equals lo_imma.ID_PERSONA into i
                                     from imma in i.DefaultIfEmpty()
                                     join lo_jobassign in db.JOBASSIGN.Where(x => x.RUOLO.COD_RUOLOAGGREG == "PROFORM" && x.DTA_FINE == dtFine) on anag.ID_PERSONA equals lo_jobassign.ID_PERSONA into j
                                     from job in j.DefaultIfEmpty()
                                     join lo_comp in db.COMPREL on anag.ID_PERSONA equals lo_comp.ID_PERSONA into c
                                     from comp in c.DefaultIfEmpty()
                                     join lo_causa in db.XR_TB_CAUSALEMOV on comp.JOMPREL.COD_CAUSALEMOV equals lo_causa.COD_CAUSALEMOV into causa
                                     where tmpSint.Select(x => x.COD_MATLIBROMAT).Contains(anag.MATRICOLA) && ((imma != null || (job != null && comp.JOMPREL.DES_CAUSALEMOV.StartsWith("APPRENDISTA"))) && (!String.IsNullOrEmpty(nome) && !String.IsNullOrEmpty(matricola) ? (anag.COGNOME + " " + anag.NOME).StartsWith(nome) && (anag.MATRICOLA == matricola || imma.COD_MATDIP == matricola) : !String.IsNullOrEmpty(nome) ? (anag.COGNOME + " " + anag.NOME).StartsWith(nome) : anag.MATRICOLA == matricola || imma.COD_MATDIP == matricola))
                                     /*  where (imma != null || ruolo != null) && (!String.IsNullOrEmpty(nome) && !String.IsNullOrEmpty(matricola) ? anag.COGNOME.StartsWith(nome) && (anag.MATRICOLA == matricola || imma.COD_MATDIP == matricola) : !String.IsNullOrEmpty(nome) ? anag.COGNOME.StartsWith(nome) : anag.MATRICOLA == matricola || imma.COD_MATDIP == matricola)*/
                                     orderby anag.ID_PERSONA, imma.TMS_TIMESTAMP
                                     select new RicercaPianoFormativo()
                                     {
                                         Nominativo = anag.COGNOME + " " + anag.NOME,
                                         IdPersona = anag.ID_PERSONA,
                                         Nome = anag.NOME,
                                         DataInizioApprendistato = job.DTA_INIZIO,
                                         DataFineApprendistato = job.DTA_FINE,
                                         DataImmatricolazione = imma.DTA_IMM_CREAZIONE,
                                         DataInizioContratto = comp.DTA_HIRE, //imma.DTA_INIZIO,
                                         Matricola = anag.MATRICOLA.Equals(null) ? imma.COD_MATDIP : anag.MATRICOLA,
                                         DescrizioneRuolo = job!=null? job.RUOLO.COD_RUOLOAGGREG:null,
                                         IsRuoloAggreg = job!=null && job.RUOLO.COD_RUOLOAGGREG == "PROFORM" ? true : false,
                                         Cessato = comp.DTA_FINE > DateTime.Today ? false : true,
                                         IdJobAssign = job == null ? 0 : job.ID_JOBASSIGN
                                     };

                pianiFormativiFiltrati = pianiFormativi.DistinctBy(x => x.IdPersona).OrderBy(x => x.Nominativo).ToList();
            }
            else if (!String.IsNullOrEmpty(sezione))
            {
                sezione = sezione.Trim();
                pianiFormativiFiltrati = db.INCARLAV.Join(db.JOBASSIGN, inc => inc.ID_PERSONA, job => job.ID_PERSONA, (inc, job)
                       => new { inc, job }).Join(db.V_XR_XANAGRA, inc => inc.inc.ID_PERSONA, anag => anag.ID_PERSONA, (inc, anag)
                            => new { inc, anag }).Join(db.RUOLO, job => job.inc.job.COD_RUOLO, ruolo => ruolo.COD_RUOLO, (job, ruolo)
                              => new { job, ruolo }).Join(db.UNITAORG, inc => inc.job.inc.inc.ID_UNITAORG, unitaOrg => unitaOrg.ID_UNITAORG, (inc, unitaOrg)
                              => new { inc, unitaOrg }).Where(w => w.unitaOrg.COD_UNITAORG == sezione && w.inc.ruolo.COD_RUOLOAGGREG == "PROFORM").Select(s
                           => new RicercaPianoFormativo()
                           {
                               Cognome = s.inc.job.anag.COGNOME,
                               IdPersona = s.inc.job.anag.ID_PERSONA,
                               Nome = s.inc.job.anag.NOME,
                               DataInizioApprendistato = s.inc.job.inc.job.DTA_INIZIO,
                               DataFineApprendistato = s.inc.job.inc.job.DTA_FINE,
                               Matricola = s.inc.job.anag.MATRICOLA,
                               DescrizioneRuolo = s.inc.ruolo.COD_RUOLOAGGREG,
                           }).DistinctBy(d => d.IdPersona).OrderBy(o => o.Matricola).ToList();
            }
            else if (!String.IsNullOrEmpty(tutorID))
            {
                tutorID = tutorID.Trim();
                int tutorIDi = Int32.Parse(tutorID);
                pianiFormativiFiltrati = db.XR_TUTOR.Join(db.V_XR_XANAGRA.Where(y=> tmpSint.Select(x => x.COD_MATLIBROMAT).Contains(y.MATRICOLA)), tutor1 => tutor1.ID_PERSONA, anag => anag.ID_PERSONA, (tutor1, anag)
                          => new { tutor1, anag }).Join(db.JOBASSIGN, anag => anag.anag.ID_PERSONA, job => job.ID_PERSONA, (anag, job)
                            => new { anag, job }).Join(db.COMPREL, job => job.job.ID_PERSONA, comp => comp.ID_PERSONA, (job, comp) => new { job, comp.JOMPREL.DES_CAUSALEMOV, comp.DTA_FINE, comp.DTA_HIRE }).Where(w => w.job.anag.tutor1.ID_TUTOR == tutorIDi && w.DES_CAUSALEMOV.StartsWith("APPRENDISTA")).Select(s
                         => new RicercaPianoFormativo()
                         {
                             Nominativo = s.job.anag.anag.COGNOME + " " + s.job.anag.anag.NOME,
                             Cognome = s.job.anag.anag.COGNOME,
                             IdPersona = s.job.anag.anag.ID_PERSONA,
                             Nome = s.job.anag.anag.NOME,
                             DataInizioApprendistato = s.job.job.DTA_INIZIO,
                             DataFineApprendistato = s.job.job.DTA_FINE,
                             DataInizioContratto = s.DTA_HIRE,
                             Matricola = s.job.anag.anag.MATRICOLA,
                             IsRuoloAggreg = true,
                             Cessato = s.DTA_FINE > DateTime.Today ? false : true,
                             IdJobAssign = s.job.job == null ? 0 : s.job.job.ID_JOBASSIGN
                         }).DistinctBy(d => d.IdPersona).OrderBy(o => o.Nominativo).ToList();

            }
            return pianiFormativiFiltrati;
        }
        internal DatiApprendistato GetDatiApprendistato(int idPersona)
        {
            DatiApprendistato apprendistato = new DatiApprendistato();

            try
            {
                var app = db.JOBASSIGN.Join(db.RUOLO, job => job.COD_RUOLO, ruolo => ruolo.COD_RUOLO, (job, ruolo) => new { job, ruolo }).Where(x => x.job.ID_PERSONA == idPersona && x.ruolo.COD_RUOLOAGGREG == "PROFORM").SingleOrDefault();
                apprendistato.IdJobAssign = app.job.ID_JOBASSIGN;
                apprendistato.Competenze = GetCompetenze(app.ruolo.COD_RUOLO, idPersona);
                DateTime dtFine = new DateTime(2999, 12, 31);
                apprendistato.TipologiaApprendistato = db.COMPREL.Where(w => w.DTA_FINE!= null && w.DTA_FINE == dtFine).Join(db.XR_TB_CAUSALEMOV, a => a.JOMPREL.COD_CAUSALEMOV, b => b.COD_CAUSALEMOV, (x, y) => new { x.ID_PERSONA, y.DES_CAUSALEMOV, y.COD_CAUSALEMOV }).Where(x => x.ID_PERSONA == idPersona && x.DES_CAUSALEMOV.StartsWith("APPRENDISTA")).SingleOrDefault().COD_CAUSALEMOV;
                apprendistato.DurataApprendistato = apprendistato.TipologiaApprendistato == "BB" ? "36" : apprendistato.TipologiaApprendistato == "BR" ? "36" : "30";
                apprendistato.ProfiloFormativo = app.ruolo.DES_RUOLO;
                apprendistato.TipologiaApprendistati = GetTipologieApprendistati();
                apprendistato.SelectedTipologiaApprendistato.Add(apprendistato.TipologiaApprendistato);
                apprendistato.Profilo = new PianoFormativo_Pianificato()
                {
                    CodiceRuolo = app.ruolo.COD_RUOLO,
                    ProfiloFormativo = app.ruolo.DES_RUOLO

                };

            }
            catch (Exception e)
            {
                var bo = e;
            }

            return apprendistato;


        }


        internal List<Competenza> GetCompetenze(string codiceRuolo, int idPersona = 0)
        {
            List<Competenza> listaCompetenze = new List<Competenza>();
            if (idPersona == 0)
            {
                listaCompetenze = db.REQRUOLO.Join(db.TB_REQUI, ruolo => ruolo.COD_REQUISITO, req => req.COD_REQUISITO, (ruolo, requisito)
                    => new { ruolo, requisito }).Join(db.TB_SREQUI, req => req.requisito.COD_SETTORE, sreq => sreq.COD_SETTORE, (requ, settore)
                        => new Competenza()
                        {
                            DescrizioneRuolo = requ.requisito.NOT_REQUISITO.Trim(),
                            TipoRuolo = settore.DES_SETTORE,
                            CodiceRuolo = requ.ruolo.COD_RUOLO,
                            CodiceRequisito = requ.requisito.COD_REQUISITO,
                            SelectedCodiceCompetenza = true,
                        }).Where(x => x.CodiceRuolo == codiceRuolo).OrderBy(o => o.DescrizioneRuolo).ToList();
            }
            else
            {
                listaCompetenze = db.REQRUOLO.Join(db.TB_REQUI, ruolo => ruolo.COD_REQUISITO, req => req.COD_REQUISITO, (ruolo, requisito)
                  => new { ruolo, requisito }).Join(db.REQPERSONA, req => req.ruolo.COD_REQUISITO, comper => comper.COD_REQUISITO, (ruolo, comper) => new { ruolo, comper }).Where(x => x.comper.ID_PERSONA == idPersona).Join(db.TB_SREQUI, req => req.ruolo.requisito.COD_SETTORE, sreq => sreq.COD_SETTORE, (requ, settore)
                                 => new Competenza()
                                 {
                                     DescrizioneRuolo = requ.ruolo.requisito.NOT_REQUISITO,
                                     TipoRuolo = settore.DES_SETTORE,
                                     CodiceRuolo = requ.ruolo.ruolo.COD_RUOLO,
                                     CodiceRequisito = requ.ruolo.requisito.COD_REQUISITO,
                                     SelectedCodiceCompetenza = true,
                                 }).Where(x => x.CodiceRuolo == codiceRuolo).OrderBy(o => o.DescrizioneRuolo).ToList();

            }
            return listaCompetenze;
        }
        internal List<RicercaPianoFormativo> GetSezioni(string filter)
        {

            string currentDate = DateTime.Now.Date.ToString("yyyyMMdd");
            List<RicercaPianoFormativo> listaSezioni = new List<RicercaPianoFormativo>();
            using (myRaiDataTalentia.TalentiaEntities dbTal = new myRaiDataTalentia.TalentiaEntities())
            {
                listaSezioni = (from sezione in dbTal.XR_STR_TSEZIONE
                                where sezione.data_fine_validita.CompareTo(currentDate) > 0 && sezione.descrizione_lunga.ToUpper().Contains(filter.ToUpper())
                                select new RicercaPianoFormativo() { SelectedSezione = sezione.descrizione_lunga, codSezione = sezione.codice_visibile }).ToList();
            }

            return listaSezioni;
        }
        internal string GetTipologiaTitoloDiStudio(int filter)
        {
            var descrizioneTipoTitolo = db.TB_LIVSTUD.Where(w => w.COD_LIVELLOSTUDIO == filter).Select(s => s.DES_LIVELLOSTUDIO).SingleOrDefault();
            return descrizioneTipoTitolo;
        }
        internal string GetTitoliDiStudio(string filter)
        {

            var listaTitoliDiStudio = db.TB_STUDIO.Where(w => w.COD_STUDIO == filter).Select(s => s.DES_STUDIO).SingleOrDefault();

            return listaTitoliDiStudio;
        }
        internal List<StudioModel> GetTitoliDiStudioById(int idPersona)
        {
            var titoloDiStudio = db.STUPERSONA.Where(w => w.ID_PERSONA == idPersona).Join(db.TB_LIVSTUD, stu => stu.JTUPERSONA.COD_LIVELLOSTUDIO, liv => liv.COD_LIVELLOSTUDIO, (stu, liv) => new { Studio = stu, Livello = liv }).ToList().Select((s, index) => new StudioModel()
            {
                CodCitta = s.Studio.COD_CITTA,
                DesTipoTitolo = s.Livello.DES_LIVELLOSTUDIO,
                CodTitolo = s.Studio.COD_STUDIO,
                CodTitoloOld = s.Studio.COD_STUDIO,
                DesTitolo = s.Studio.TB_STUDIO.DES_STUDIO,
                CodTipoTitolo = s.Studio.JTUPERSONA.COD_LIVELLOSTUDIO.Value,
                DataInizioStr = s.Studio.JTUPERSONA.DTA_INIZIO.ToString(),
                Riconoscimento = s.Studio.JTUPERSONA.DES_RICONOSCIMENTO,
                CorsoLaurea = s.Studio.JTUPERSONA.DES_CORSO,
                Istituto = s.Studio.JTUPERSONA.DES_ISTITUTO,
                IdPersona = s.Studio.ID_PERSONA,
                DataFineStr = s.Studio.DTA_CONSEG.ToString(),
                idTitoloStudioLocal = index.ToString(),
                Scala = s.Studio.COD_TIPOPUNTEGGIO,
                Voto = s.Studio.COD_PUNTEGGIO,
                Nota = s.Studio.NOT_NOTABREVE,
                CodIstituto = s.Studio.COD_ATENEO,
                Cod_TipoPunteggio = s.Studio.COD_TIPOPUNTEGGIO,
                Lode = (s.Studio.COD_LIVELLOPESO == 1) ? true : false


            }).ToList();
            foreach (var item in titoloDiStudio)
            {
                if (item.DataInizioStr != "")
                {

                    item.DataInizioStr = DateTime.Parse(item.DataInizioStr).Month + "/" + DateTime.Parse(item.DataInizioStr).Year;
                }
                if (item.DataFineStr != "")
                {

                    item.DataFineStr = DateTime.Parse(item.DataFineStr).Month + "/" + DateTime.Parse(item.DataFineStr).Year;
                }
            }
            return titoloDiStudio;
        }
        internal List<EsperienzeLavorativeViewModel> GetEsperienzeLavorativeById(int idPersona)
        {
            var esperienze = db.ESPPREC.Where(w => w.ID_PERSONA == idPersona).ToList().Select((s, index) => new EsperienzeLavorativeViewModel()
            {
                IdPersona = idPersona,
                Azienda = s.DES_SOCIETAESP,
                Attivita = s.DES_RUOLOPROV,
                DescrizioneCitta = s.DES_SEDE,
                DataInizioStrEL = s.DTA_INIZIO.ToString(),
                DataFineStrEL = s.DTA_FINE.ToString(),
                idEspLavLocal = index.ToString(),
                Apprendistato = s.DES_LIVRETR == "Apprendistato" ? true : false


            }).ToList();

            foreach (var item in esperienze)
            {
                if (item.DataInizioStrEL != "")
                {

                    item.DataInizioStrEL = DateTime.Parse(item.DataInizioStrEL).Month + "/" + DateTime.Parse(item.DataInizioStrEL).Year;
                }
                if (item.DataFineStrEL != "")
                {

                    item.DataFineStrEL = DateTime.Parse(item.DataFineStrEL).Month + "/" + DateTime.Parse(item.DataFineStrEL).Year;
                }
            }

            return esperienze;
        }
        internal List<TutorPianoFormativoVM> GetListaTutorById(int idPersona)
        {
            var tutor = db.XR_TUTOR.Where(w => w.ID_PERSONA == idPersona).Join(db.SINTESI1, tut => tut.ID_TUTOR, anag => anag.ID_PERSONA, (tut, anag) => new { tutor = tut, anag = anag }).ToList().Select(s => new TutorPianoFormativoVM()
            {
                IdPersona = idPersona,
                Nominativo = s.anag.DES_COGNOMEPERS + " " + s.anag.DES_NOMEPERS,
                MatricolaTutor = s.anag.COD_MATLIBROMAT,
                Dal = s.tutor.DTA_INIZIO,
                DalStr = s.tutor.DTA_INIZIO.ToShortDateString(),
                AlStr = s.tutor.DTA_FINE.ToShortDateString(),
                Al = s.tutor.DTA_FINE,
                Categoria = s.anag.DES_QUALIFICA,
                Oid = s.tutor.ID_XR_TUTOR,
                Nota = s.tutor.NOT_NOTA
            }).ToList();


            return tutor;
        }

        internal bool CheckExistTitoliDiStudioApprendista(int idPersona)
        {
            bool exist = false;
            var record = db.STUPERSONA.FirstOrDefault(x => x.ID_PERSONA == idPersona);
            if (record != null)
            {
                exist = true;
            }
            return exist;
        }
        internal List<PianoFormativo_Pianificato> GetProfiliFormativi(string filter)
        {
            List<PianoFormativo_Pianificato> listaProfili = new List<PianoFormativo_Pianificato>();
            listaProfili = db.RUOLO.Where(w => w.COD_RUOLOAGGREG == "PROFORM").Select(s
                  => new PianoFormativo_Pianificato()
                  {

                      ProfiloFormativo = s.DES_RUOLO,
                      CodiceRuolo = s.COD_RUOLO
                  }).OrderBy(o => o.ProfiloFormativo).ToList();
            return listaProfili;
        }
        internal List<SelectListItem> GetTipologieApprendistati()
        {

            var tipologieApprendistato = db.XR_TB_CAUSALEMOV.OrderBy(o => o.DES_CAUSALEMOV).Take(3).Select(s =>
              new SelectListItem()
              {
                  Value = s.COD_CAUSALEMOV,
                  Text = s.DES_CAUSALEMOV,
                  Selected = false
              }).ToList();
            foreach (var item in tipologieApprendistato)
            {
                item.Text.UpperFirst();
            }
            return tipologieApprendistato;
        }
        internal DocX HandleData(int idPersona)
        {
            db = new IncentiviEntities();
            DocX doc = null;
            var data = new ModelPianoFormativoForWord();
            var studi = new List<StudioModel>();
            var competenze = new List<Competenza>();
            var esperienze = new List<EsperienzeLavorativeViewModel>();
            var anagrafica = new ModelPianoFormativoForWord();
            var immatricolazione = new XR_IMM_IMMATRICOLAZIONI();
            var codiceruolo = "";
            var codCausaleMov = "";
            var anagraficaApprendista = db.ANAGPERS.Where(w => w.ID_PERSONA == idPersona).SingleOrDefault();
            var tutor = new TutorPianoFormativoVM();
            var azienda = new Azienda();
            gestioneDatiDaVisualizzare = new GestioneDatiDaVisualizzare(db);
            DateTime dtFine = new DateTime(2999, 12, 31);
            var rcomprel = db.COMPREL.Where(w => w.DTA_FINE != null && w.DTA_FINE == dtFine).Where(x => x.ID_PERSONA == idPersona && x.JOMPREL.DES_CAUSALEMOV.StartsWith("APPRENDISTA")).FirstOrDefault();
            if (anagraficaApprendista != null)
            {
                //RECUPERO DATI DA INSERIRE NEL TEMPLATE WORD
                // immatricolazione = immatricolazioneRepository.GetById(idPersona);
                // immatricolazione = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona);
                var oggi = DateTime.Today;
                string codcitta = "000";
                if (anagraficaApprendista.CITTAD.Count() > 0)
                {

                    codcitta = anagraficaApprendista.CITTAD.Where(x => x.ID_PERSONA == idPersona && x.IND_CITTADPRIM == "Y" && /*oggi >= x.DTA_INIZIO &&*/ oggi <= x.DTA_FINE).FirstOrDefault().COD_CITTADPERS;
                }
                string citta = gestioneDatiDaVisualizzare.GetDescrizioneCittadinzaByCOD_CITTAD(codcitta); // anagraficaApprendista.CITTAD
                data.Cittadina = citta; // gestioneDatiDaVisualizzare.GetDescrizioneCittadinzaByCOD_CITTAD(immatricolazione.COD_CITTAD);
                //bdata.CodQual = gestioneDatiDaVisualizzare.GetDescrizioneQualifica(immatricolazione.COD_QUALIFICA);  //assqual
                data.CodQual = gestioneDatiDaVisualizzare.GetDescrizioneQualifica(anagraficaApprendista.ASSQUAL.Where(x => x.ID_PERSONA == idPersona && oggi <= x.DTA_FINE).FirstOrDefault().COD_QUALIFICA);  //assqual

                //   data.Prov = gestioneDatiDaVisualizzare.GetProvinciaByCodiceISTAT(immatricolazione.COD_CITTA);
                data.Prov = gestioneDatiDaVisualizzare.GetProvinciaByCodiceISTAT(anagraficaApprendista.COD_CITTA);
                anagrafica = db.ANAGPERS.Where(x => x.ID_PERSONA == idPersona).Select(s => new ModelPianoFormativoForWord()
                {
                    Cognome = s.DES_COGNOMEPERS,
                    Nome = s.DES_NOMEPERS,
                    CF = s.CSF_CFSPERSONA,
                    CittaNasc = s.COD_CITTA,
                    DataNasc = s.DTA_NASCITAPERS,
                    CittaDom = s.COD_CITTADOM,
                    ProvDom = "",
                    Indirizzo = s.DES_INDIRDOM,
                    CapDom = s.CAP_CAPDOM,
                    Telefono = s.DES_TELREC,
                    Cellulare = s.DES_CELLULARE,
                    Email = s.DES_EMCEMAIL,
                    Fax = s.DES_EMCPHONE
                }).SingleOrDefault();
                anagrafica.CittaNasc = gestioneDatiDaVisualizzare.GetDescrizioneCittaByCodiceISTAT(anagrafica.CittaNasc);
                if (!String.IsNullOrWhiteSpace(anagrafica.CittaDom))
                {
                    anagrafica.ProvDom = gestioneDatiDaVisualizzare.GetProvinciaByCodiceISTAT(anagrafica.CittaDom);
                    anagrafica.CittaDom = gestioneDatiDaVisualizzare.GetDescrizioneCittaByCodiceISTAT(anagrafica.CittaDom);
                }
                else
                    anagrafica.CittaDom = anagrafica.ProvDom = null;
                studi = db.STUPERSONA.Where(w => w.ID_PERSONA == idPersona).Select(s => new StudioModel() { DesTitolo = s.TB_STUDIO.DES_STUDIO }).ToList();
                codiceruolo = db.JOBASSIGN.Join(db.RUOLO, a => a.COD_RUOLO, b => b.COD_RUOLO, (x, y) => new { x, y.COD_RUOLOAGGREG }).Where(x => x.x.ID_PERSONA == idPersona && x.COD_RUOLOAGGREG == "PROFORM").SingleOrDefault().x.COD_RUOLO;
                //  db.JOBASSIGN.Join(lo_ruolo in db.RUOLO on new { job.COD_RUOLO, COD_RUOLOAGGREG = "PROFORM" }.Where(x => x.ID_PERSONA == idPersona).Join(db.RUOLO ).SingleOrDefault().COD_RUOLO;
                data.ProForm = gestioneDatiDaVisualizzare.GetDescrizioneRuolo(codiceruolo);
                competenze = GetCompetenze(codiceruolo, idPersona);

                codCausaleMov = db.COMPREL.Where(w => w.DTA_FINE != null && w.DTA_FINE == dtFine).Join(db.XR_TB_CAUSALEMOV, a => a.JOMPREL.COD_CAUSALEMOV, b => b.COD_CAUSALEMOV, (x, y) => new { x.ID_PERSONA, y.DES_CAUSALEMOV, y.COD_CAUSALEMOV }).Where(x => x.ID_PERSONA == idPersona && x.DES_CAUSALEMOV.StartsWith("APPRENDISTA")).SingleOrDefault().COD_CAUSALEMOV;
                DateTime maxdate = AnagraficaManager.GetDateLimitMax();
                if (/*immatricolazione.DTA_FINE*/ rcomprel.DTA_FINE < maxdate)
                { maxdate = rcomprel.DTA_FINE; }
                tutor = db.SINTESI1.Join(db.XR_TUTOR, anag => anag.ID_PERSONA, tutor_ => tutor_.ID_TUTOR, (anag, tutor_) => new
                {
                    anag,
                    tutor_.DTA_FINE,
                    tutor_.ID_PERSONA,
                    tutor_.NOT_NOTA
                })
                .Where(x => x.ID_PERSONA == idPersona &&
                x.DTA_FINE.Day == maxdate.Day && x.DTA_FINE.Month == maxdate.Month && x.DTA_FINE.Year == maxdate.Year)
                .Join(db.JSSQUAL, anag => anag.anag.ASSQUAL.ID_ASSQUAL, jss => jss.ID_ASSQUAL, (anag, jss) => new
                {
                    anag.anag,
                    jss.DTA_ANZCAT,
                    anag.NOT_NOTA
                })
                .Join(db.COMPREL, anag => anag.anag.ID_COMPREL, comprel => comprel.ID_COMPREL, (anag, comprel) => new TutorPianoFormativoVM()
                {
                    Nominativo = anag.anag.DES_NOMEPERS + " " + anag.anag.DES_COGNOMEPERS,
                    Cf = anag.anag.CSF_CFSPERSONA,
                    TutorAnzCategoria = anag.DTA_ANZCAT,
                    TutorAss = comprel.DTA_HIRE,
                    Categoria = anag.anag.DES_QUALIFICA,
                    Nota = anag.NOT_NOTA
                }).FirstOrDefault();

                // azienda = db.V_XR_SOCIETA.Where(x => x.CodSoc == immatricolazione.COD_IMPRESA).Select(s => new Azienda()
                azienda = db.V_XR_SOCIETA.Where(x => x.CodSoc == rcomprel.COD_IMPRESA).Select(s => new Azienda()
                {
                    RagioneSociale = s.RagSoc,
                    Sede = s.IndirizzoSoc,
                    Cap = s.CapSoc,
                    Fax = s.FaxSoc,
                    Email = s.MailSoc,
                    CF = s.CFSoc,
                    Legale_Rappresentante = s.RapLegSoc,
                    PartitaIva = s.IvaSoc,
                    Telefono = s.TelSoc
                }).FirstOrDefault();

            }
            esperienze = GetEsperienzeLavorativeById(idPersona);
            var tipo_minimo = codCausaleMov == "BZ" ? "5" : "7";
            var durataApprendistato = codCausaleMov == "BB" ? "36" : codCausaleMov == "BR" ? "36" : "30";
            //LOAD TEMPLATE
            byte[] template = db.CZNMMDOC.Where(w => w.NME_FILENAME == "NewPfi.docx").Select(s => s.OBJ_OBJECT).FirstOrDefault();
            MemoryStream ms = new MemoryStream(template);
            doc = DocX.Load(ms);
            #region REPLACETEXT
            doc.ReplaceText("«COGNOME»", anagrafica.Cognome);
            doc.ReplaceText("«NOME»", anagrafica.Nome);
            doc.ReplaceText("«CF»", anagrafica.CF);
            doc.ReplaceText("«Cittadina»", data.Cittadina);
            doc.ReplaceText("«CittaNasc»", anagrafica.CittaNasc);
            doc.ReplaceText("«DataNasc»", anagrafica.DataNasc.ToShortDateString());
            doc.ReplaceText("«CittaDom»", anagrafica.CittaDom ?? "-");
            doc.ReplaceText("«Prov»", anagrafica.ProvDom ?? "-");
            doc.ReplaceText("«Email»", anagrafica.Email ?? "-");
            doc.ReplaceText("«Indirizzo»", anagrafica.Indirizzo ?? "-");
            doc.ReplaceText("«Telefono»", anagrafica.Telefono ?? "-");
            doc.ReplaceText("«Fax»", anagrafica.Fax ?? "-");
            doc.ReplaceText("«Cellulare»", anagrafica.Cellulare ?? "-");
            doc.ReplaceText("«DataAss»", /*immatricolazione.DTA_INIZIO*/ rcomprel.DTA_HIRE.ToShortDateString());
            doc.ReplaceText("«ProForm»", data.ProForm);
            doc.ReplaceText("«Durata»", durataApprendistato + " mesi");
            doc.ReplaceText("«CodQual»", data.CodQual);
            doc.ReplaceText("«TipoMinimo»", tipo_minimo);
            doc.ReplaceText("«Tutor»", tutor.Nominativo);
            doc.ReplaceText("«TutorCF»", tutor.Cf);
            doc.ReplaceText("«TutorCat»", tutor.Categoria);
            doc.ReplaceText("«TutorAss»", tutor.TutorAss.Value.ToShortDateString());
            doc.ReplaceText("«TutorAnzCat»", tutor.TutorAnzCategoria.Value.ToShortDateString() == null || tutor.TutorAnzCategoria.Value.ToShortDateString() == "" ? "Da definire" : tutor.TutorAnzCategoria.Value.ToShortDateString());
            doc.ReplaceText("«Note»", (tutor.Nota == null) ? "" : tutor.Nota);
            doc.ReplaceText("«RagSoc»", (azienda.RagioneSociale == null) ? "" : azienda.RagioneSociale);
            doc.ReplaceText("«IndirizzoSoc»", (azienda.Sede == null) ? "" : azienda.Sede);
            doc.ReplaceText("«CapSoc»", (azienda.Cap == null) ? "" : azienda.Cap);
            doc.ReplaceText("«IvaSoc»", (azienda.PartitaIva == null) ? "" : azienda.PartitaIva);
            doc.ReplaceText("«CFSoc»", (azienda.CF == null) ? "" : azienda.CF);
            doc.ReplaceText("«TelSoc»", (azienda.Telefono == null) ? "" : azienda.Telefono);
            doc.ReplaceText("«FaxSoc»", (azienda.Fax == null) ? "" : azienda.Fax);
            doc.ReplaceText("«MailSoc»", (azienda.Email == null) ? "" : azienda.Email);
            doc.ReplaceText("«RapLegSoc»", (azienda.Legale_Rappresentante == null) ? "" : azienda.Legale_Rappresentante);
            #endregion
            string listaStudi = "";
            string listaCompetenze = "\r";
            string listaEsperienze = "";
            //foreach (var item in studi)
            //{
            //    listaStudi += $"{item.DesTitolo} \r\n";
            //}
            listaStudi = String.Join("\r\n", studi.Select(x => x.DesTitolo));
            //foreach (var item in esperienze)
            //{
            //    listaEsperienze += $"{item.Azienda + ": " + item.Attivita}\r\n";
            //}
            listaEsperienze = String.Join("\r\n", esperienze.Select(x => x.Azienda + ": " + x.Attivita));

            doc.ReplaceText("«Studi»", listaStudi);
            //foreach (var item in competenze)
            //{
            //    listaCompetenze += $"{"- " + item.DescrizioneRuolo} \r\n";
            //}
            listaCompetenze = String.Join("\r\n", competenze.Select(x => "- " + x.DescrizioneRuolo));
            doc.ReplaceText("«Studi»", listaStudi);
            doc.ReplaceText("«Requisiti»", listaCompetenze);
            doc.ReplaceText("«Esperienze»", listaEsperienze);

            var countTutor = db.XR_TUTOR.Count(x => x.ID_PERSONA == idPersona);
            if (countTutor > 1)
                doc.ReplaceText("«NoteAggiuntive»", "\r\n\r\nA far data dal ………………. il presente piano formativo sostituisce il precedente, consegnato il …………., per avvicendamento del Tutor.");
            else
                doc.ReplaceText("«NoteAggiuntive»", "");

            return doc;
        }
        internal bool InserimentoNuovoPianoFormativo(NuovoPianoFormativo entity)
        {

            bool result = false;
            var titoli = new StudioModel();
            var esperienze = new EsperienzeLavorativeViewModel();
            var tutors = new TutorPianoFormativoVM();
            var idPersona = entity.DatiApprendistato.IdPersona;
            db = new IncentiviEntities();
            var oldTitoli = db.STUPERSONA.Where(x => x.ID_PERSONA == idPersona).ToList();
            if (oldTitoli.Count() > 0)
            {
                foreach (var old in oldTitoli)
                {
                    if (entity.Titoli.Where(x => x.CodTitoloOld == old.COD_STUDIO).ToList().Count == 0)
                    {
                        db.JTUPERSONA.RemoveWhere(x => x.ID_PERSONA == idPersona && x.COD_STUDIO == old.COD_STUDIO);
                        db.STUPERSONA.Remove(old);
                    }
                }
            }
            if (entity.Titoli != null)
            {
                foreach (var item in entity.Titoli)
                {
                    item.IdPersona = idPersona;
                    AnagraficaManager.Save_DatiStudio(db, item, false, out string errorMsg);

                }
            }

            db.ESPPREC.RemoveWhere(x => x.ID_PERSONA == idPersona);
            if (entity.Esperienze != null)
            {
                foreach (var item in entity.Esperienze)
                {
                    esperienze = item;
                    DateTime dateTime;
                    try
                    {
                        dateTime = item.DataInizioStrEL.ToDateTime("MM/yyyy");
                    }
                    catch
                    {
                        dateTime = item.DataInizioStrEL.ToDateTime("M/yyyy");
                    }
                    ESPPREC record = new ESPPREC();
                    record.ID_PERSONA = item.IdPersona;
                    record.IMP_RETRIB = 0;
                    record.QTA_ADDETTI = 0;
                    record.QTA_RIPORTI = 0;
                    record.DES_RUOLOPROV = item.Attivita;
                    record.DES_SOCIETAESP = item.Azienda;
                    record.DES_SEDE = item.DescrizioneCitta;
                    record.DTA_INIZIO = dateTime;
                    try
                    {
                        dateTime = item.DataFineStrEL.ToDateTime("MM/yyyy");
                    }
                    catch
                    {
                        dateTime = item.DataFineStrEL.ToDateTime("M/yyyy");
                    }
                    record.DTA_FINE = dateTime;
                    if (item.Apprendistato)
                        record.DES_LIVRETR = "Apprendistato";
                    else
                        record.DES_LIVRETR = null;
                    CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
                    record.COD_USER = codUser;
                    record.COD_TERMID = codTermId;
                    record.TMS_TIMESTAMP = tmsTimestamp;
                    try
                    {
                        db.ESPPREC.Add(record);
                    }
                    catch (Exception e)
                    {
                        var bo = e;
                    }
                }
            }

            if (entity.Tutor != null)
            {

                foreach (var item in entity.Tutor)
                {
                    item.Oid = db.XR_TUTOR.GeneraOid(x => x.ID_XR_TUTOR);
                    tutors = item;
                    SaveTutor(db, tutors, out string erroMsg);
                }
            }
            if (entity.DatiApprendistato != null)
            {
                var jobAssign = db.JOBASSIGN.Join(db.RUOLO, x => x.COD_RUOLO, ruolo => ruolo.COD_RUOLO, (job, ruolo)
            => new { job, ruolo.COD_RUOLOAGGREG }).Where(x => x.job.ID_PERSONA == idPersona && x.COD_RUOLOAGGREG == "PROFORM").Select(x => x.job).FirstOrDefault();

                CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
                if (jobAssign == null)
                {
                    //db = new IncentiviEntities();
                    jobAssign = new JOBASSIGN();
                    jobAssign.ID_JOBASSIGN = db.JOBASSIGN.GeneraPrimaryKey(9);
                    jobAssign.ID_PERSONA = idPersona;
                    jobAssign.IND_INCPRINC = "N";
                    jobAssign.COD_RUOLO = entity.DatiApprendistato.ProfiloFormativo;
                    jobAssign.DTA_INIZIO = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    jobAssign.DTA_FINE = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_FINE;
                    jobAssign.COD_TERMID = codTermId;
                    //   jobAssign.DTA_INIZIO = immatricolazioneRepository.GetById(idPersona).DTA_INIZIO;
                    // jobAssign.DTA_FINE = immatricolazioneRepository.GetById(idPersona).DTA_FINE;
                    jobAssign.COD_USER = codUser;
                    jobAssign.TMS_TIMESTAMP = tmsTimestamp;
                    jobAssign.COD_EVJOB = null;
                    db.JOBASSIGN.Add(jobAssign);
                    //   result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "HRIS - jobAssign/Comprel");
                }
                var req = db.REQPERSONA.FirstOrDefault(x => x.ID_PERSONA == idPersona);
                if (req == null)
                {
                    var tiporeq = db.REQRUOLO.Where(x => x.COD_RUOLO == entity.DatiApprendistato.ProfiloFormativo).Select(x => x.COD_TIPOREQ).FirstOrDefault();
                    foreach (var comp in entity.DatiApprendistato.Competenze)
                    {
                        REQPERSONA competenza = new REQPERSONA()
                        {
                            ID_PERSONA = idPersona,
                            COD_REQUISITO = comp.CodiceRequisito,
                            COD_TIPOREQ = tiporeq,
                            COD_LIVELLOPESO = 0,
                            COD_TERMID = codTermId,
                            COD_USER = codUser,
                            COD_SOURCE = "I",
                            TMS_TIMESTAMP = tmsTimestamp
                        };
                        db.REQPERSONA.Add(competenza);

                    }
                }
                var comprel = db.COMPREL.FirstOrDefault(x => x.ID_PERSONA == idPersona);
                var destipo = db.XR_TB_CAUSALEMOV.FirstOrDefault(x => x.COD_CAUSALEMOV == entity.DatiApprendistato.TipologiaApprendistato).DES_CAUSALEMOV;

                if (comprel == null)
                {
                    JOMPREL jomprel = new JOMPREL()
                    {
                        COD_CAUSALEMOV = entity.DatiApprendistato.TipologiaApprendistato,
                        DES_CAUSALEMOV = destipo.ToUpper()
                    };

                    //db = new IncentiviEntities();
                    comprel = new COMPREL();
                    comprel.ID_PERSONA = idPersona;
                    comprel.ID_COMPREL = db.COMPREL.GeneraPrimaryKey(9);
                    comprel.TMS_TIMESTAMP = tmsTimestamp;
                    comprel.COD_TERMID = codTermId;
                    comprel.COD_IMPRESA = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).COD_IMPRESA;
                    //comprel.COD_CAUSALEMOV = entity.DatiApprendistato.TipologiaApprendistato;
                    comprel.COD_USER = codUser;
                    comprel.DTA_ANZCONV = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    comprel.DTA_FINE = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_FINE;
                    comprel.DTA_HIRE = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    comprel.DTA_INIZIO = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    comprel.COD_ATTIVRLAV = "73";
                    comprel.COD_TIPORLAV = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).COD_TIPORLAV;
                    comprel.JOMPREL = jomprel;
                    db.COMPREL.Add(comprel);
                    //     result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "HRIS - jobAssign/Comprel");

                }
                else
                {
                    comprel.TMS_TIMESTAMP = tmsTimestamp;
                    comprel.COD_TERMID = codTermId;
                    comprel.COD_IMPRESA = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).COD_IMPRESA;
                    comprel.JOMPREL.COD_CAUSALEMOV = entity.DatiApprendistato.TipologiaApprendistato;
                    comprel.JOMPREL.DES_CAUSALEMOV = destipo.ToUpper();
                    comprel.COD_USER = codUser;
                    comprel.DTA_ANZCONV = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    comprel.DTA_FINE = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_FINE;
                    comprel.DTA_HIRE = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    comprel.DTA_INIZIO = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    comprel.COD_ATTIVRLAV = "73";
                    comprel.COD_TIPORLAV = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).COD_TIPORLAV;
                    db.COMPREL.AddOrUpdate(comprel);
                }
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "HRIS - jobAssign/Comprel");


            }

            return result;
        }
        internal bool ModificaPianoFormativo(NuovoPianoFormativo entity)
        {
            bool result = false;
            var titoli = new StudioModel();
            var esperienze = new EsperienzeLavorativeViewModel();
            var tutors = new TutorPianoFormativoVM();
            var idPersona = entity.DatiApprendistato.IdPersona;
            db = new IncentiviEntities();
            var oldTitoli = db.STUPERSONA.Where(x => x.ID_PERSONA == idPersona).ToList();
            if (oldTitoli.Count() > 0)
            {
                foreach (var old in oldTitoli)
                {
                    if (entity.Titoli.Where(x => x.CodTitoloOld == old.COD_STUDIO).ToList().Count == 0)
                    {
                        db.JTUPERSONA.RemoveWhere(x => x.ID_PERSONA == idPersona && x.COD_STUDIO == old.COD_STUDIO);
                        db.STUPERSONA.Remove(old);
                    }
                }
            }
            if (entity.Titoli != null)
            {
                foreach (var item in entity.Titoli)
                {
                    titoli = item;
                    AnagraficaManager.Save_DatiStudio(db, titoli, false, out string errorMsg);

                }

            }
            db.ESPPREC.RemoveWhere(x => x.ID_PERSONA == idPersona);

            if (entity.Esperienze != null)
            {
                foreach (var item in entity.Esperienze)
                {
                    esperienze = item;
                    DateTime dateTime;
                    try
                    {
                        dateTime = item.DataInizioStrEL.PadLeft(7, '0').ToDateTime("MM/yyyy");
                    }
                    catch
                    {
                        dateTime = item.DataInizioStrEL.ToDateTime("M/yyyy");
                    }
                    //   var recordold = db.ESPPREC.FirstOrDefault(x => x.ID_PERSONA == model.IdPersona && x.DTA_INIZIO == dateTime);
                    ESPPREC record = new ESPPREC();
                    record.ID_PERSONA = item.IdPersona;
                    record.IMP_RETRIB = 0;
                    record.QTA_ADDETTI = 0;
                    record.QTA_RIPORTI = 0;
                    record.DES_RUOLOPROV = item.Attivita;
                    record.DES_SOCIETAESP = item.Azienda;
                    record.DES_SEDE = item.DescrizioneCitta;

                    record.DTA_INIZIO = dateTime;

                    try
                    {
                        dateTime = item.DataFineStrEL.PadLeft(7, '0').ToDateTime("MM/yyyy");
                    }
                    catch
                    {
                        dateTime = item.DataFineStrEL.ToDateTime("M/yyyy");
                    }
                    record.DTA_FINE = dateTime;
                    if (item.Apprendistato)
                        record.DES_LIVRETR = "Apprendistato";
                    else
                        record.DES_LIVRETR = null;
                    CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
                    record.COD_USER = codUser;
                    record.COD_TERMID = codTermId;
                    record.TMS_TIMESTAMP = tmsTimestamp;


                    try
                    {

                        //  db.ESPPREC.RemoveWhere(x => x.ID_PERSONA == record.ID_PERSONA && x.DTA_INIZIO == record.DTA_INIZIO);
                        db.ESPPREC.Add(record);
                    }
                    catch (Exception e)
                    {
                        var bo = e;
                    }
                }
            }
            var oldTutor = db.XR_TUTOR.Where(x => x.ID_PERSONA == idPersona).ToList();
            if (oldTutor.Count() > 0)
            {
                foreach (var old in oldTutor)
                {
                    if (entity.Tutor.Where(x => x.Oid == old.ID_XR_TUTOR).ToList().Count == 0)
                        db.XR_TUTOR.Remove(old);
                }
            }
            if (entity.Tutor != null)
            {

                foreach (var item in entity.Tutor)
                {
                    if (item.Oid == 0)
                        item.Oid = db.XR_TUTOR.GeneraOid(x => x.ID_XR_TUTOR);
                    item.IdPersona = idPersona;
                    tutors = item;
                    /*   if (SaveTutor(tutors, out string erroMsg)) { }
                       else
                           return result;*/
                    SaveTutor(db, tutors, out string erroMsg);
                }
            }
            if (entity.DatiApprendistato != null)
            {
                var jobAssign = db.JOBASSIGN.FirstOrDefault(x => x.ID_PERSONA == idPersona && x.ID_JOBASSIGN == entity.DatiApprendistato.IdJobAssign);
                CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
                jobAssign.COD_RUOLO = entity.DatiApprendistato.ProfiloFormativo;
                var imma = db.XR_IMM_IMMATRICOLAZIONI.Where(x => x.ID_PERSONA == idPersona && x.ESITO_CICS == true && x.ESITO == true).OrderByDescending(x => x.ID_EVENTO).FirstOrDefault(); //  immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona);
                if (imma != null)
                {
                    jobAssign.DTA_INIZIO = imma.DTA_INIZIO; // immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    jobAssign.DTA_FINE = imma.DTA_FINE; // immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_FINE;
                }
                else
                {
                    jobAssign.DTA_INIZIO = db.COMPREL.Where(x => x.ID_PERSONA == idPersona).FirstOrDefault().DTA_HIRE;
                    jobAssign.DTA_FINE = db.COMPREL.Where(x => x.ID_PERSONA == idPersona).FirstOrDefault().DTA_FINE;

                }
                jobAssign.COD_TERMID = codTermId;
                jobAssign.COD_USER = codUser;
                jobAssign.TMS_TIMESTAMP = tmsTimestamp;
                jobAssign.COD_EVJOB = null;

                db.REQPERSONA.RemoveWhere(x => x.ID_PERSONA == idPersona);

                var tiporeq = db.REQRUOLO.Where(x => x.COD_RUOLO == entity.DatiApprendistato.ProfiloFormativo).Select(x => x.COD_TIPOREQ).FirstOrDefault();
                foreach (var comp in entity.DatiApprendistato.Competenze)
                {
                    REQPERSONA competenza = new REQPERSONA()
                    {
                        ID_PERSONA = idPersona,
                        COD_REQUISITO = comp.CodiceRequisito,
                        COD_TIPOREQ = tiporeq,
                        COD_LIVELLOPESO = 0,
                        COD_TERMID = codTermId,
                        COD_USER = codUser,
                        COD_SOURCE = "I",
                        TMS_TIMESTAMP = tmsTimestamp
                    };
                    db.REQPERSONA.Add(competenza);
                }
                var comprel = db.COMPREL.FirstOrDefault(x => x.ID_PERSONA == idPersona);
                var destipo = db.XR_TB_CAUSALEMOV.FirstOrDefault(x => x.COD_CAUSALEMOV == entity.DatiApprendistato.TipologiaApprendistato).DES_CAUSALEMOV;
                if (comprel == null)
                {
                    JOMPREL jomprel = new JOMPREL()
                    {
                        COD_CAUSALEMOV = entity.DatiApprendistato.TipologiaApprendistato,
                        DES_CAUSALEMOV = destipo.ToUpper()
                    };

                    //db = new IncentiviEntities();
                    comprel = new COMPREL();
                    comprel.ID_PERSONA = idPersona;
                    comprel.ID_COMPREL = db.COMPREL.GeneraPrimaryKey(9);
                    comprel.TMS_TIMESTAMP = tmsTimestamp;
                    comprel.COD_TERMID = codTermId;
                    comprel.COD_IMPRESA = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).COD_IMPRESA;
                    //comprel.COD_CAUSALEMOV = entity.DatiApprendistato.TipologiaApprendistato;
                    comprel.COD_USER = codUser;
                    comprel.DTA_ANZCONV = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    comprel.DTA_FINE = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_FINE;
                    comprel.DTA_HIRE = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    comprel.DTA_INIZIO = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    comprel.COD_ATTIVRLAV = "73";
                    comprel.COD_TIPORLAV = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).COD_TIPORLAV;
                    comprel.JOMPREL = jomprel;
                    db.COMPREL.Add(comprel);
                    //     result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "HRIS - jobAssign/Comprel");

                }
                else
                {
                    comprel.TMS_TIMESTAMP = tmsTimestamp;
                    comprel.COD_TERMID = codTermId;
                    //  comprel.COD_IMPRESA = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).COD_IMPRESA;
                    comprel.JOMPREL.COD_CAUSALEMOV = entity.DatiApprendistato.TipologiaApprendistato;
                    comprel.JOMPREL.DES_CAUSALEMOV = destipo.ToUpper();
                    comprel.COD_USER = codUser;
                    //  comprel.DTA_ANZCONV = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    //  comprel.DTA_FINE = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_FINE;
                    //  comprel.DTA_HIRE = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    //  comprel.DTA_INIZIO = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).DTA_INIZIO;
                    comprel.COD_ATTIVRLAV = "73";
                    //  comprel.COD_TIPORLAV = immatricolazioneRepository.Get(x => x.ID_PERSONA == idPersona).COD_TIPORLAV;
                    db.COMPREL.AddOrUpdate(comprel);
                }
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "HRIS - jobAssign/Comprel");
            }

            return result;
        }
        internal bool Elimina(int idPersona, int idJobAssign)
        {
            bool result = false;
            db = new IncentiviEntities();
            // db.STUPERSONA.RemoveWhere(x => x.ID_PERSONA == idPersona);
            // db.ESPPREC.RemoveWhere(x => x.ID_PERSONA == idPersona);
            db.XR_TUTOR.RemoveWhere(x => x.ID_PERSONA == idPersona);
            db.JOBASSIGN.RemoveWhere(x => x.ID_PERSONA == idPersona && x.ID_JOBASSIGN == idJobAssign);
            db.REQPERSONA.RemoveWhere(x => x.ID_PERSONA == idPersona);
            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "HRIS - jobAssign/Comprel");
            return result;
        }
        public static bool SaveEsperienzeLavorative(IncentiviEntities db, EsperienzeLavorativeViewModel model, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";
            DateTime dateTime;
            try
            {
                dateTime = model.DataInizioStrEL.ToDateTime("MM/yyyy");
            }
            catch
            {
                dateTime = model.DataInizioStrEL.ToDateTime("M/yyyy");
            }
            //   var recordold = db.ESPPREC.FirstOrDefault(x => x.ID_PERSONA == model.IdPersona && x.DTA_INIZIO == dateTime);
            ESPPREC record = new ESPPREC();
            record.ID_PERSONA = model.IdPersona;
            record.IMP_RETRIB = 0;
            record.QTA_ADDETTI = 0;
            record.QTA_RIPORTI = 0;
            record.DES_RUOLOPROV = model.Attivita;
            record.DES_SOCIETAESP = model.Azienda;
            record.DES_SEDE = model.DescrizioneCitta;

            record.DTA_INIZIO = dateTime;

            try
            {
                dateTime = model.DataFineStrEL.ToDateTime("MM/yyyy");
            }
            catch
            {
                dateTime = model.DataFineStrEL.ToDateTime("M/yyyy");
            }
            record.DTA_FINE = dateTime;
            if (model.Apprendistato)
                record.DES_LIVRETR = "Apprendistato";
            else
                record.DES_LIVRETR = null;
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
            record.COD_USER = codUser;
            record.COD_TERMID = codTermId;
            record.TMS_TIMESTAMP = tmsTimestamp;


            try
            {

                //  db.ESPPREC.RemoveWhere(x => x.ID_PERSONA == record.ID_PERSONA && x.DTA_INIZIO == record.DTA_INIZIO);
                db.ESPPREC.Add(record);
            }
            catch (Exception e)
            {
                var bo = e;
            }
            //   result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "HRIS - Esperienze Lavorative");
            return result;
        }

        public static bool SaveTutor(IncentiviEntities db, TutorPianoFormativoVM model, out string erroMsg)
        {
            bool result = false;
            erroMsg = "";
            // var db = new IncentiviEntities();
            var record = db.XR_TUTOR.FirstOrDefault(x => x.ID_PERSONA == model.IdPersona && x.ID_XR_TUTOR == model.Oid);
            var anagraficaTutor = db.ANAGPERS.Where(x => x.JNAGPERS != null && x.JNAGPERS.COD_MATDIP == model.MatricolaTutor).SingleOrDefault().ID_PERSONA;
            if (record == null)
            {
                record = new XR_TUTOR();
                record.DTA_INIZIO = model.Dal.Value;
                record.DTA_FINE = model.Al.Value;
                record.NOT_NOTA = model.Nota;
                record.COD_EVTUTOR = null;
                record.ID_PERSONA = model.IdPersona;
                CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
                record.COD_USER = codUser;
                record.COD_TERMID = codTermId;
                record.TMS_TIMESTAMP = tmsTimestamp;
                record.ID_XR_TUTOR = model.Oid;
                record.ID_TUTOR = anagraficaTutor;
                db.XR_TUTOR.Add(record);
            }
            else
            {
                record.DTA_INIZIO = model.Dal.Value;
                record.DTA_FINE = model.Al.Value;
                record.NOT_NOTA = model.Nota;
                CezanneHelper.GetCampiFirma(out string codUser, out string codTermId, out DateTime tmsTimestamp);
                record.COD_USER = codUser;
                record.COD_TERMID = codTermId;
                record.TMS_TIMESTAMP = tmsTimestamp;
                // record.ID_XR_TUTOR = model.Oid;
            }
            //  result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "HRIS - Esperienze Lavorative");
            return result;

        }
    }
}