using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiGestionale.RepositoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiGestionale.Services
{
    public class GestioneDatiDaVisualizzare
    {
        //private SezioneRepository sezioneRepository;
        private ImpresaRepository impresaRepository;
        private TipoContrattoDecodificaRepository contrattoRepository;
        private QualificaRepository qualificaRepository;
        private TipoRapportoLavoroRepository rapportoDiLavoroRepository;
        private ComuneRepository comuneRepository;
        private SedeRepository sedeRepository;
        private ServizioXRRepository servizioXR_TBRepository;
        private ImmatricolazioneRepository immatricolazioneRepository;
        private nazioneRepository nazioneRepository;

        public GestioneDatiDaVisualizzare(IncentiviEntities db)
        {
            this.nazioneRepository = new nazioneRepository(db);
            this.immatricolazioneRepository = new ImmatricolazioneRepository(db);
            this.qualificaRepository = new QualificaRepository(db);
            this.rapportoDiLavoroRepository = new TipoRapportoLavoroRepository(db);
            //this.sezioneRepository = new SezioneRepository(db);    
            this.impresaRepository = new ImpresaRepository(db);
            this.contrattoRepository = new TipoContrattoDecodificaRepository(db);
            this.sedeRepository = new SedeRepository(db);
            this.comuneRepository = new ComuneRepository(db);
            this.servizioXR_TBRepository = new ServizioXRRepository(db);
        }
        #region DDL-SELECTASYNC
        public List<TB_COMUNE> GetComuni(string filter, string value)
        {
            List<TB_COMUNE> listacomuni = null;
            if (!string.IsNullOrEmpty(value))
            {
                listacomuni = comuneRepository.GetAll(s => s.COD_CITTA == value).ToList();

            }
            else
            {
                listacomuni = comuneRepository.GetAll(s => s.DES_CITTA.StartsWith(filter), o => o.DES_CITTA).ToList();

            }
            return listacomuni;
        }
        internal List<TB_CITTAD> GetNazioni(string filter, string value)
        {
            List<TB_CITTAD> listanazioni = null;
            if (!string.IsNullOrEmpty(value))
            {
                listanazioni = nazioneRepository.GetAll(s => s.COD_CITTAD == value).ToList();

            }
            else
            {

                listanazioni = nazioneRepository.GetAll(s => s.DES_CITTAD.StartsWith(filter)).ToList();
            }
            return listanazioni;
        }
        public List<QUALIFICA> GetQualifiche(string filter, string value)
        {
            List<QUALIFICA> listaQualifiche = null;
            if (!string.IsNullOrEmpty(value))
            {
                listaQualifiche = qualificaRepository.GetAll(s => s.COD_QUALIFICA == value).ToList();

            }
            else
            {
                listaQualifiche = qualificaRepository.GetAll(w => w.COD_QUALSTD != "ND" && w.DES_QUALIFICA.StartsWith(filter)).ToList();

            }
            return listaQualifiche;
        }
        public List<CODIFYIMP> GetImprese(string filter, string value)
        {
            List<CODIFYIMP> listaimprese = null;
            if (!string.IsNullOrEmpty(value))
            {
                listaimprese = impresaRepository.GetAll(s => s.COD_IMPRESA == value).ToList();

            }
            else
            {
                listaimprese = impresaRepository.GetAll(s => s.COD_SOGGETTO.ToUpper().StartsWith(filter)).ToList();
            }
            return listaimprese;
        }
        public List<TB_TPRLAV> GetRapportiLavorativi(string filter)
        {
            List<TB_TPRLAV> listarapportilavorativi = new List<TB_TPRLAV>();

            listarapportilavorativi = rapportoDiLavoroRepository.GetAll(w => w.IND_LIBROMAT == "Y" && w.DES_TIPORLAV.StartsWith(filter), o => o.COD_TIPORLAV).ToList();


            return listarapportilavorativi;
        }
        public List<ImmatricolazioniVM> GetSezioni(string filter, string azienda, string servizio, string value)
        {

            string currentDate = DateTime.Now.Date.ToString("yyyyMMdd");
            List<ImmatricolazioniVM> listaSezioni = new List<ImmatricolazioniVM>();
            using (myRaiDataTalentia.TalentiaEntities db = new myRaiDataTalentia.TalentiaEntities())
            {
                if (!string.IsNullOrEmpty(value))
                {
                    listaSezioni = (from sezione in db.XR_STR_TSEZIONE
                                    where sezione.data_fine_validita.CompareTo(currentDate) > 0 && sezione.codice_visibile.Contains(value)
                                    select new ImmatricolazioniVM() { descrizioneSezione = sezione.descrizione_lunga, codSezione = sezione.codice_visibile }).ToList();

                }
                else
                {
                    //listaSezioni = (from serv in db.XR_TB_SERVIZIO
                    //                join sezione in db.XR_STR_TSEZIONE on serv.COD_SERVIZIO equals sezione.servizio
                    //                where sezione.data_fine_validita.CompareTo(currentDate) > 0 && sezione.descrizione_lunga.ToUpper().Contains(filter.ToUpper()) && serv.COD_SERVIZIO == servizio
                    //                select new ImmatricolazioniVM() { descrizioneSezione = sezione.descrizione_lunga, codSezione = sezione.codice_visibile }).ToList();

                    if (!String.IsNullOrWhiteSpace(filter))
                    {

                        listaSezioni = (from sezione in db.XR_STR_TSEZIONE 
                                        where sezione.data_fine_validita.CompareTo(currentDate) > 0 && sezione.codice_visibile.ToUpper().StartsWith(filter.ToUpper()) && sezione.servizio == servizio
                                        select new ImmatricolazioniVM() { descrizioneSezione = sezione.descrizione_lunga, codSezione = sezione.codice_visibile }).ToList();
                    }
                    else
                    {
                        listaSezioni = (from sezione in db.XR_STR_TSEZIONE
                                        where sezione.data_fine_validita.CompareTo(currentDate) > 0 && sezione.servizio == servizio
                                        select new ImmatricolazioniVM() { descrizioneSezione = sezione.descrizione_lunga, codSezione = sezione.codice_visibile }).ToList();
                    }
                }
            }
            return listaSezioni;
        }
        public string GetDecodificaSezione(string codSezione)
        {
            List<string> sezioni = null;
            var descrizione_sezione = "";
            string currentDate = DateTime.Now.Date.ToString("yyyyMMdd");
            using (myRaiDataTalentia.TalentiaEntities db = new myRaiDataTalentia.TalentiaEntities())
            {
                sezioni = (from sezione in db.XR_STR_TSEZIONE
                           where sezione.data_fine_validita.CompareTo(currentDate) > 0 && sezione.codice_visibile == codSezione
                           select sezione.descrizione_lunga).ToList();


            }
            foreach (var item in sezioni)
            {
                descrizione_sezione = item;
            }
            return descrizione_sezione;
        }
        public List<XR_TB_SERVIZIO> GetServizi(string filter, string value, string azienda)
        {
            List<XR_TB_SERVIZIO> listaServizi = null;
            if (!string.IsNullOrEmpty(value))
            {
                listaServizi = servizioXR_TBRepository.GetAll(
                 w => w.COD_SERVIZIO == value, null, null, null, null, null, null, null, null
                 ).ToList();
            }
            else
            {
                listaServizi = servizioXR_TBRepository.GetAll(
                 w => w.COD_IMPRESA == azienda && w.QTA_ORDINE == 0 && w.DES_SERVIZIO.StartsWith(filter), o => o.DES_SERVIZIO,
                 null, null, null, null, null, null, null
                 ).ToList();

            }

            return listaServizi;
        }
        public List<SEDE> GetSedi(string filter, string value)
        {
            List<SEDE> listaSedi = null;
            if (!string.IsNullOrEmpty(value))
            {
                listaSedi = sedeRepository.GetAll(w => w.COD_SEDE == value).ToList();
            }
            else
            {
                listaSedi = sedeRepository.GetAll(w => w.COD_SEDE != "***" && w.DES_SEDE.StartsWith(filter), o => o.DES_SEDE).ToList();
            }

            return listaSedi;
        }

        #endregion

        public string GetDescrizioneCittadinzaByCOD_CITTAD(string codCittadinanza)
        {
            var descrizione_cittadinanza = "";
            var db = new IncentiviEntities();
            if (!string.IsNullOrEmpty(codCittadinanza))
                descrizione_cittadinanza = db.TB_CITTAD.Where(w => w.COD_CITTAD == codCittadinanza).SingleOrDefault().DES_CITTAD;
            return descrizione_cittadinanza;
        }
        public string GetDescrizioneProfiloFormativo(string codiceRuolo)
        {
            var descrizioneRuolo = "";
            var db = new IncentiviEntities();
            if (!string.IsNullOrEmpty(codiceRuolo))
                descrizioneRuolo = db.RUOLO.Where(w => w.COD_RUOLO == codiceRuolo).SingleOrDefault().DES_RUOLO;
            return descrizioneRuolo;
        }
        public string GetDescrizioneQualifica(string codiceQualifica)
        {
            var db = new IncentiviEntities();
            var descrizionequalifica = "";
            if (!string.IsNullOrEmpty(codiceQualifica))
                descrizionequalifica = db.QUALIFICA.Where(w => w.COD_QUALIFICA == codiceQualifica).SingleOrDefault().DES_QUALIFICA;
            return descrizionequalifica;
        }
        public string GetDescrizioneCittaByCodiceISTAT(string codiceISTAT)
        {
            var db = new IncentiviEntities();
            var descrizionecitta = "";
            if (!string.IsNullOrEmpty(codiceISTAT))
                descrizionecitta = db.TB_COMUNE.Where(w => w.COD_CITTA == codiceISTAT).SingleOrDefault().DES_CITTA;
            return descrizionecitta;
        }
        public string GetProvinciaByCodiceISTAT(string codIstat)
        {
            string provincia = "";
            provincia = comuneRepository.Get(w => w.COD_CITTA == codIstat).COD_PROV_STATE;
            return provincia;
        }
        public void GetDescrizioneDettagli(ImmatricolazioniVM immatricolazioniVM)
        {
            var rapportoLavorativo = "";
            var descrizioneServizio = servizioXR_TBRepository.Get(x => x.COD_SERVIZIO == immatricolazioniVM.SelectedServizio).DES_SERVIZIO;
            immatricolazioniVM.SelectedServizio = descrizioneServizio;
            if (immatricolazioniVM.SelectedRapplavoro.Equals("1") || immatricolazioniVM.SelectedRapplavoro.Equals("3"))
            {
                rapportoLavorativo = "TEMPO INDETERMINATO";
            }
            else if (immatricolazioniVM.SelectedRapplavoro.Equals("2") || immatricolazioniVM.SelectedRapplavoro.Equals("4"))
            {
                immatricolazioniVM.SelectedRapplavoro = "TEMPO DETERMINATO";
            }
            else
            {
                rapportoLavorativo = "CO.CO.CO";
            }
            immatricolazioniVM.SelectedRapplavoro = rapportoLavorativo;

        }
        public string GetDescrizioneRuolo(string codRuolo)
        {
            var descrizioneRuolo = "";
            var db = new IncentiviEntities();
            descrizioneRuolo = db.RUOLO.Where(x => x.COD_RUOLO == codRuolo).SingleOrDefault().DES_RUOLO;
            return descrizioneRuolo;
        }

        internal void GetDescrizioniByEntityData(XR_IMM_IMMATRICOLAZIONI result)
        {
            var db = new IncentiviEntities();
            var dbTal = new myRaiDataTalentia.TalentiaEntities();
            result.COD_CITTA = db.TB_COMUNE.FirstOrDefault(w => w.COD_CITTA == result.COD_CITTA).DES_CITTA;
            result.COD_CITTAD = db.TB_CITTAD.FirstOrDefault(x => x.COD_CITTAD == result.COD_CITTAD).DES_CITTAD;
            result.COD_TIPORLAV = db.TB_TPRLAV.FirstOrDefault(w => w.COD_TIPORLAV == result.COD_TIPORLAV).DES_TIPORLAV;
            result.COD_IMPRESA = db.CODIFYIMP.FirstOrDefault(w => w.COD_IMPRESA == result.COD_IMPRESA).COD_SOGGETTO;
            result.COD_SEDE = db.SEDE.FirstOrDefault(w => w.COD_SEDE == result.COD_SEDE).DES_SEDE;
            result.COD_SERVIZIO = db.XR_TB_SERVIZIO.FirstOrDefault(w => w.COD_SERVIZIO == result.COD_SERVIZIO).DES_SERVIZIO;
            result.COD_QUALIFICA = db.QUALIFICA.FirstOrDefault(w => w.COD_QUALIFICA == result.COD_QUALIFICA).DES_QUALIFICA;
            result.COD_SEZIONE = dbTal.XR_STR_TSEZIONE.FirstOrDefault(w => w.codice_visibile == result.COD_SEZIONE).descrizione_lunga;
            if (result.COD_RUOLO!=null)
                result.COD_RUOLO = db.RUOLO.FirstOrDefault(w => w.COD_RUOLO == result.COD_RUOLO).DES_RUOLO;
            ImmatricolazioniVM viewmodel = ImmatricolazioniVM.ConverToImmatricolazioneVM(result);

        }


    }
}