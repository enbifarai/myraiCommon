using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace myRaiCommonModel.Gestionale
{
    public enum MboState
    {
        DaAvviare = 1,
        DaCompilare = 2,
        ObiettiviSottopostiAlDir = 3,
        ProntiPerInvioRuo = 4,
        ObiettiviSottopostiRuo = 5,
        Convalidati = 6,
        Consutivati = 7
    }
    public enum MboTipologia
    {
        Mbo = 7
    }

    public enum LivelloDirigente
    {
        Top,
        ExTop,
        TopAdPersonam,
        Full,
        Manager,
        NonIndividuato
    }



    public class MboIniziativa
    {
        public MboIniziativa()
        {

        }
        public MboIniziativa(XR_MBO_INIZIATIVA dbIniz)
        {
            Id = dbIniz.ID_INIZIATIVA;
            Nome = dbIniz.NME_NOME;
            Descrizione = dbIniz.DES_DESCRIZIONE;

            DataInizioAssegnazione = dbIniz.DTA_INI_ASSEGNAZIONE;
            DataFineAssegnazione = dbIniz.DTA_END_ASSEGNAZIONE;
            DataInizioValutazione = dbIniz.DTA_INI_VALUT;
            DataFineValutazione = dbIniz.DTA_END_VALUT;

            ImportoTop = dbIniz.DEC_IMPORTO_MAX_TOP;
            ImportoFull = dbIniz.DEC_IMPORTO_MAX_FULL;
            ImportoManager = dbIniz.DEC_IMPORTO_MAX_MANAGER;

            CoeffDecurtazione = dbIniz.DEC_COEFF_DECURTAZIONE;
            CoeffGestionale = dbIniz.DEC_COEFF_GESTIONALE;

            IdSchedaValutazione = dbIniz.ID_SCHEDA_VAL;
        }


        public int Id { get; set; }
        [Required]
        public string Nome { get; set; }
        public string Descrizione { get; set; }

        [Required]
        public DateTime DataInizioAssegnazione { get; set; }

        [Required]
        public DateTime DataFineAssegnazione { get; set; }

        [Required]
        public DateTime DataInizioValutazione { get; set; }

        [Required]
        public DateTime DataFineValutazione { get; set; }

        [Required]
        public decimal ImportoTop { get; set; }
        [Required]
        public decimal ImportoFull { get; set; }
        [Required]
        public decimal ImportoManager { get; set; }
        public int? IdSchedaValutazione { get; set; }

        public decimal? CoeffDecurtazione { get; set; }
        public decimal? CoeffGestionale { get; set; }

        public List<SelectListItem> SchedePossibili { get; set; }
    }

    public class MboScheda
    {
        public MboScheda()
        {
            Obiettivi = new List<MboObiettivo>();
            Subalterni = new List<MboPersona>();
            Note = new List<MboNota>();
            ElencoSchedeSub = new List<MboScheda>();
            Stati = new List<XR_MBO_OPERSTATI>();
        }
        public MboScheda(XR_MBO_SCHEDA dbScheda)
        {
            Id = dbScheda.ID_SCHEDA;
            IdPersonaResp = dbScheda.ID_PERSONA_RESP;
            IdPersonaValutato = dbScheda.ID_PERSONA_VALUTATO;
            IdPersonaSecRiporto = dbScheda.ID_PERS_RIPORTO;
            IdPersonaConsuntivazione = dbScheda.ID_PERSONA_CONSUNTIVAZIONE;
            
            ImportoTeorico = dbScheda.DEC_IMPORTO_TEORICO;
            Approvato = dbScheda.IND_APPROVED;

            IsSent = dbScheda.IND_SENT;
            IsApproved = dbScheda.IND_APPROVED;
            IsConsuntivazioneBlocked = dbScheda.IND_INIBISCI_CONSUNTIVAZIONE.GetValueOrDefault();
            IsChief = dbScheda.IND_CHIEF.GetValueOrDefault();

            switch (dbScheda.COD_LIV)
            {
                case "Top":
                    CodLivelloDir = LivelloDirigente.Top;
                    break;
                case "Full":
                    CodLivelloDir = LivelloDirigente.Full;
                    break;
                case "Manager":
                    CodLivelloDir = LivelloDirigente.Manager;
                    break;
                default:
                    break;
            }

            Obiettivi = new List<MboObiettivo>();
            Subalterni = new List<MboPersona>();
            Note = new List<MboNota>();

            Stati = dbScheda.XR_MBO_OPERSTATI.OrderBy(x => x.XR_MBO_STATI.XR_WKF_WORKFLOW.FirstOrDefault(y => y.ID_TIPOLOGIA == dbScheda.ID_TIPOLOGIA).ORDINE).ToList();
            
            ElencoSchedeSub = new List<MboScheda>();

            Completamento = dbScheda.NMB_RESULT;
        }

        public bool IsManualModified()
        {
            return IdPersonaRespOrig.HasValue || ImportoTeoricoOrig.HasValue || CreatoManualmente.GetValueOrDefault();
        }

        public int Id { get; set; }

        public int IdPersonaResp { get; set; }
        public MboPersona PersResp { get; set; }
        public SINTESI1 DBPersResp { get; set; }
        public bool Interim { get; set; }
        public string Incarico { get; set; }

        public int IdPersonaValutato { get; set; }
        public MboPersona PersValutato { get; set; }
        public SINTESI1 DBPersVal { get; set; }

        public int? IdPersonaSecRiporto { get; set; }
        public MboPersona PersSecRiporto { get; set; }
        public SINTESI1 SintPersRiporto { get; set; }

        public int? IdPersonaConsuntivazione { get; set; }
        public MboPersona PersConsuntivazione { get; set; }
        public SINTESI1 SintPersConsuntivazione { get; set; }

        public int? IdPersonaRespOrig { get; set; }
        public MboPersona PersRespOrig { get; set; }

        public decimal ImportoTeorico { get; set; }
        public decimal? ImportoTeoricoOrig { get; set; }

        public bool? CreatoManualmente { get; set; }
        public bool? Approvato { get; set; }
        public bool? IsSent { get; set; }
        public bool? IsApproved { get; set; }
        public bool IsConsuntivazioneBlocked { get; set; }
        public LivelloDirigente CodLivelloDir { get; set; }

        public MboIniziativa Iniziativa { get; set; }
        public AnagraficaModel Anagrafica { get; set; }
        public List<MboObiettivo> Obiettivi { get; set; }

        public List<MboPersona> Subalterni { get; set; }
        public List<MboIncaricoNode> Incarichi { get; set; }

        public bool ManChanged { get; set; }
        public bool HasOper { get; set; }
        public bool DeleteOper { get; set; }
        public bool IsExtra { get; set; }

        public List<XR_MBO_OPERSTATI> Stati { get; set; }
        public XR_MBO_OPERSTATI OperStato { get; set; }
        public XR_MBO_STATI StatoCorrente { get; set; }
        public XR_WKF_WORKFLOW Ordine { get; set; }
        public XR_MBO_INIZIATIVA DBInziativa { get; set; }
        public string DBLivello { get; set; }
        public SINTESI1 DbPersRiporto { get; set; }

        public bool IsSecondoRiporto { get; set; }

        public List<XR_MBO_ALLEGATI> Allegati { get; set; }

        public List<MboNota> Note { get; set; }

        public bool IsChief { get; set; }
        public List<MboScheda> ElencoSchedeSub { get; set; }
        public MboPersona CurrentOperator { get; set; }
        public decimal? Completamento { get; set; }
        public int? IdSchedaVal { get; internal set; }
        public XR_VAL_OPER_STATE StatoVal { get; internal set; }
        public decimal? ImportoEffettivo { get; internal set; }
        public bool IsConsuntivata { get; internal set; }
        public SINTESI1 DbPersCons { get; internal set; }
        public bool IsConvalidata { get; set; }
    }

    public class MboPersona
    {
        public MboPersona()
        {

        }
        public MboPersona(SINTESI1 sint)
        {
            Id = sint.ID_PERSONA;
            Matricola = sint.COD_MATLIBROMAT;
            Nome = sint.DES_NOMEPERS.TitleCase();
            Cognome = sint.DES_COGNOMEPERS.TitleCase();
            Nominativo = sint.Nominativo();

            if (!String.IsNullOrWhiteSpace(sint.COD_SERVIZIO))
            {
                CodServizio = sint.COD_SERVIZIO;

                if (!String.IsNullOrWhiteSpace(sint.DES_SERVIZIO))
                    DesServizio = CezanneHelper.GetDes(sint.COD_SERVIZIO, sint.DES_SERVIZIO).TitleCase();
            }

            if (!String.IsNullOrWhiteSpace(sint.COD_UNITAORG))
            {
                CodStruttura = sint.COD_UNITAORG;
                if (!String.IsNullOrWhiteSpace(sint.DES_DENOMUNITAORG))
                    DesStruttura = sint.DES_DENOMUNITAORG.TitleCase();
            }
        }

        public int Id { get; set; }
        public string Matricola { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Nominativo { get; set; }

        public string CodServizio { get; set; }
        public string DesServizio { get; set; }

        public string CodStruttura { get; set; }
        public string DesStruttura { get; set; }
    }

    public class MboObiettivo
    {
        public MboObiettivo()
        {

        }
        public MboObiettivo(XR_MBO_OBIETTIVI dbOb)
        {
            Id = dbOb.ID_OBIETTIVO;
            IdScheda = dbOb.ID_SCHEDA;
            IdTipologia = dbOb.ID_TIPOLOGIA;
            Tipo = dbOb.COD_TIPO;
            Descrizione = dbOb.DES_DESCRIZIONE;
            PesoSpecifico = Convert.ToInt32(dbOb.NMB_PESO_SPEC);
            IsStrategicoDirezione = dbOb.IND_STRAT_DIR;
            RisultatoAtteso = dbOb.NOT_RISULTATO_ATTESO;
            IsConsolidato = dbOb.IND_CONSOLIDATO;
            Completamento = dbOb.NMB_PERC_COMPLETAMENTO;
            StrategicoDirezione = dbOb.NOT_STRAT_DIR;
            Annullato = dbOb.IND_ANNULLATO.GetValueOrDefault();
            PesoEffettivo = dbOb.NMB_PESO_EFFETTIVO;
        }

        public int Id { get; set; }
        public int IdScheda { get; set; }
        public int IdTipologia { get; set; }
        public string Tipo { get; set; }
        public string Descrizione { get; set; }
        public decimal PesoSpecifico { get; set; }
        public bool IsStrategicoDirezione { get; set; }
        public string StrategicoDirezione { get; set; }
        public string RisultatoAtteso { get; set; }
        public bool? IsConsolidato { get; set; }
        public decimal? Completamento { get; set; }
        public bool Annullato { get; set; }
        public decimal? PesoEffettivo { get; set; }
    }

    public class MboNota
    {
        public MboNota()
        {

        }

        public MboNota(XR_MBO_NOTE nota)
        {
            Id = nota.ID_NOTA;
            Testo = nota.NOT_NOTA;
            Visibilita = nota.VISIBILITA;
            IdPersonaAutore = nota.ID_PERSONA;
            IdPersonaDest = nota.ID_PERSONA_DEST;
            DataInserimento = nota.DTA_INSERIMENTO;
            Stato = nota.XR_MBO_OPERSTATI;
            IdObiettivo = nota.ID_OBIETTIVO;
        }

        public int Id { get; set; }
        public string Testo { get; set; }
        public string Visibilita { get; set; }
        public int IdPersonaAutore { get; set; }
        public int? IdPersonaDest { get; set; }
        public int? IdObiettivo { get; set; }
        public MboPersona Autore { get; set; }
        public MboPersona Destinatario { get; set; }
        public DateTime DataInserimento { get; set; }
        public XR_MBO_OPERSTATI Stato { get; set; }

    }

    public class MboRicerca
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public int? Stato { get; set; }
        public int? Responsabile { get; set; }
        public int? SecondoRiporto { get; set; }
        public int? Consuntivatore { get; set; }
        public string Direzione { get; set; }
        public int? IdIniziativa { get; set; }

        public bool HasFilter { get; set; }
    }

    public class MboIncaricoNode
    {
        public MboIncaricoNode()
        {
            Persone = new List<MboPersona>();
            Children = new List<MboIncaricoNode>();
        }
        public string CodIncarico { get; set; }
        public string DesIncarico { get; set; }

        public string CodSezione { get; set; }
        public string DesSezione { get; set; }

        public List<MboPersona> Persone { get; set; }

        public List<MboIncaricoNode> Children { get; set; }
    }

    public class HRDWTipologiaDir
    {
        public string Matricola { get; set; }
        public string Tipologia { get; set; }
    }

    public class MboParametri
    {
        public bool AbilitaNotificaAssegnazione { get; set; }
        public string TestoNotificaAssegnazione { get; set; }
        public List<MboMail> Mail { get; set; }
    }
    public class MboMail
    {
        public string CodTipo { get; set; }
        public string Mittente { get; set; }
        public string Oggetto { get; set; }
        public string Testo { get; set; }
        public string CC { get; set; }
        public string CCN { get; set; }
    }

    public class MboReminder
    {
        public string CodTipo { get; set; }
        public List<XR_MBO_MAIL_LOG> LogEmail { get; set; }
        public List<MboScheda> Schede { get; set; }
    }
}
