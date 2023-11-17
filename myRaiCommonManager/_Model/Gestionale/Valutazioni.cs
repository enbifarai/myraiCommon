using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace myRaiCommonModel.Gestionale
{
    public enum ValutazioniView
    {
        Gestione,
        Valutatore
    }

    public enum ValutazioniState
    {
        Incarico = 10,
        Bozza = 20,
        Convalidata = 30,
        Analizzata = 35,
        PresaVisione = 40,
        SviluppoCompilato = 50,
        SviluppoApprovato = 60,
        Consolidata = 70
    }

    public enum ValutazioniDelega
    {
        ConDelega = 1,
        SenzaDelega = 2
    }

    public enum ValutazioniExtEval
    {
        ConValutatoreEsterno = 1,
        SenzaValutatoreEsterno = 2
    }

    public enum ValutazioniLoader
    {
        Minimo = 1,
        Completo = 2,
        MieiValutatori = 3,
        Report = 4
    }

    public enum ValutazioniExtEvalPermission
    {
        NonAutorizzato,
        Autorizzato,
        Obbligatorio
    }

    public enum ValutazioniVisualizzazioneDipendente
    {
        NonVisibile,
        Visibile
    }

    public enum ValutazioniAuto
    {
        [AmbientValue("Non richiesta")]
        No = 0,
        [AmbientValue("Richiesta")]
        Parallelo = 1
    }

    public enum ValutazioniWorkflow
    {
        Valutazioni = 14
    }

    public class ValutazioniList
    {
        public ValutazioniList()
        {
            ValutazioniEffettuate = new List<ValutazioniContainer>();
            ValutazioniPossibili = new List<ValutazioniPossibiliContainer>();

            ValutazioniDaConvalidare = new List<ValutazioniContainer>();
            ValutazioniConvalidate = new List<ValutazioniContainer>();
        }

        public List<ValutazioniContainer> ValutazioniEffettuate { get; set; }
        public List<ValutazioniPossibiliContainer> ValutazioniPossibili { get; set; }

        public List<ValutazioniContainer> ValutazioniDaConvalidare { get; set; }
        public List<ValutazioniContainer> ValutazioniConvalidate { get; set; }
    }

    public class ValutazioniContainer
    {
        public ValutazioniContainer()
        {
            Valutazioni = new List<XR_VAL_EVALUATION>();
        }
        public XR_VAL_EVALUATOR Valutatore { get; set; }

        public List<XR_VAL_EVALUATION> Valutazioni { get; set; }
    }

    public class ValutazioniPossibiliContainer
    {
        public ValutazioniPossibiliContainer()
        {
            ValutazioniPossibili = new List<SINTESI1>();
        }

        public XR_VAL_EVALUATOR Valutatore { get; set; }

        public List<SINTESI1> ValutazioniPossibili { get; set; }
    }


    public class SheetContainer
    {
        public SheetContainer()
        {
            Roles = new List<RoleContainer>();
            SubRoles = new List<RoleContainer>();
        }
        public XR_VAL_CAMPAIGN_SHEET Sheet { get; set; }

        public List<RoleContainer> Roles { get; set; }

        public List<RoleContainer> SubRoles { get; set; }
    }
    public class RoleContainer
    {
        public RoleContainer()
        {
            Evaluations = new List<EvaluationContainer>();
        }
        public XR_VAL_EVALUATOR Role { get; set; }
        public List<EvaluationContainer> Evaluations { get; set; }
    }
    public class EvaluationContainer
    {
        public SINTESI1 Person { get; set; }
        public XR_VAL_EVALUATION Evaluation { get; set; }
        public XR_VAL_DELEGATION Delegation { get; set; }
        public XR_VAL_EVALUATOR_EXT ExternalEvaluator { get; set; }
    }

    public class DelegheContainer
    {
        public DelegheContainer()
        {
            Delegator = new List<XR_VAL_DELEGATION>();
            Delegate = new List<XR_VAL_DELEGATION>();
        }

        public List<XR_VAL_DELEGATION> Delegator { get; set; }

        public List<XR_VAL_DELEGATION> Delegate { get; set; }

        public List<XR_VAL_EVALUATOR> RuoliDelegabili { get; set; }
    }

    public class DelegaModel
    {
        public DelegaModel()
        {
            RuoliDelegabili = new List<SelectListItem>();
            DateStart = DateTime.Today;
            DateEnd = DateStart.AddDays(1);
            ValutazioniDelegate = new List<int>();
            ValutazioniDelegabiliSel = new List<int>();
            ValutazioniDelegabili = new List<XR_VAL_EVALUATION>();
        }
        public DelegaModel(XR_VAL_DELEGATION dbDelegate)
        {
            ID = dbDelegate.ID_DELEGATION;
            ValidStart = dbDelegate.VALID_DTA_INI;
            ValidEnd = dbDelegate.VALID_DTA_END;
            DateStart = dbDelegate.DTA_START;
            DateEnd = dbDelegate.DTA_END;
            Delegante = dbDelegate.DELEGANTE;
            Delegato = dbDelegate.DELEGATO;
            RuoloDelegato = dbDelegate.ID_DELEGATOR;
            PersonaDelegata = dbDelegate.ID_DELEGATE;
            RuoliDelegabili = new List<SelectListItem>();
            ValutazioniDelegate = new List<int>();
            if (dbDelegate.XR_VAL_DELEGATION_PERS != null && dbDelegate.XR_VAL_DELEGATION_PERS.Count() > 0)
            {
                ValutazioniDelegate.AddRange(dbDelegate.XR_VAL_DELEGATION_PERS.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Select(x => x.ID_PERSONA));
            }
            ValutazioniDelegabili = new List<XR_VAL_EVALUATION>();

        }

        public int ID { get; set; }
        public DateTime ValidStart { get; set; }

        public DateTime? ValidEnd { get; set; }

        [Required]
        public DateTime DateStart { get; set; }

        [Required]
        public DateTime DateEnd { get; set; }

        public XR_VAL_EVALUATOR Delegante { get; set; }

        public XR_VAL_EVALUATOR Delegato { get; set; }

        public int RuoloDelegato { get; set; }
        public int PersonaDelegata { get; set; }
        public List<SelectListItem> RuoliDelegabili { get; set; }

        public List<int> ValutazioniDelegate { get; set; }
        public List<XR_VAL_EVALUATION> ValutazioniDelegabili { get; set; }
        public List<int> ValutazioniDelegabiliSel { get; set; }
    }

    public class Sheet
    {
        public Sheet()
        {
            Groups = new List<QuestionGroup>();
        }

        public Sheet(XR_VAL_EVAL_SHEET evalSheet, bool allDetails = false)
        {
            IdSheet = evalSheet.ID_SHEET;
            Name = evalSheet.NAME;
            Description = evalSheet.DESCRIPTION;
            Groups = new List<QuestionGroup>();

            if (allDetails)
            {
                var questions = evalSheet.XR_VAL_EVAL_SHEET_QST.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).OrderBy(x => x.ORDER);//.Select(x => x.XR_VAL_QUESTION);
                foreach (var item in questions.GroupBy(x => x.XR_VAL_QUESTION.ID_QST_GROUP))
                {
                    Groups.Add(new QuestionGroup(item));
                }
            }
        }

        public int IdSheet { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<QuestionGroup> Groups { get; set; }
    }

    public class QuestionGroup
    {
        public QuestionGroup()
        {
            Questions = new List<Question>();
        }

        public QuestionGroup(IGrouping<int, XR_VAL_EVAL_SHEET_QST> group) : base()
        {
            var groupData = group.First().XR_VAL_QUESTION.XR_VAL_QUESTION_GROUP;
            Id = groupData.ID_QST_GROUP;
            Name = groupData.NAME;
            Description = groupData.DESCRIPTION;
            Questions = new List<Question>();

            foreach (var item in group)
            {
                Questions.Add(new Question(item.XR_VAL_QUESTION, item.ID_SHEET_QST, item.WEIGHT));
            }
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Question> Questions { get; set; }
    }

    public class Question
    {
        public Question()
        {
            Answers = new List<QuestionAnswer>();
        }

        public Question(XR_VAL_QUESTION question, int evalQst, int evalWeight) : base()
        {
            Id = question.ID_QUESTION;
            Name = question.NAME;
            Description = question.DESCRIPTION;
            Type = question.ID_QST_TYPE;
            View = question.ID_QST_DISPLAY;
            Weight = evalWeight;
            IdEvalQst = evalQst;
            Answers = new List<QuestionAnswer>();

            foreach (var item in question.XR_VAL_QUESTION_ANSWER.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).OrderBy(x => x.NUM_ORDER))
            {
                Answers.Add(new QuestionAnswer(item));
            }
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public int View { get; set; }
        public int Weight { get; private set; }
        public int IdEvalQst { get; set; }
        public List<QuestionAnswer> Answers { get; set; }
    }

    public class QuestionAnswer
    {
        public QuestionAnswer(XR_VAL_QUESTION_ANSWER answer)
        {
            Id = answer.ID_QST_ANSWER;
            Description = answer.DESCRIPTION;
            ValueInt = answer.VALUE_INT;
            ValueStr = answer.VALUE_STR;
            ValueDec = answer.VALUE_DECIMAL;
            Note = answer.NOT_HELP;
        }

        public int Id { get; set; }
        public int? ValueInt { get; set; }
        public string ValueStr { get; set; }
        public decimal? ValueDec { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
    }

    public class Campagna
    {
        public Campagna()
        {
            ValidStart = DateTime.Today;
            DateStart = DateTime.Today;
            DateEnd = DateStart.AddDays(1);
            Schede = new List<CampagnaScheda>();
        }

        public Campagna(XR_VAL_CAMPAIGN dbCamp)
        {
            ID = dbCamp.ID_CAMPAIGN;
            Name = dbCamp.NAME;
            Description = dbCamp.DESCRIPTION;
            ValidStart = dbCamp.VALID_DTA_INI;
            ValidEnd = dbCamp.VALID_DTA_END;
            DateStart = dbCamp.DTA_START;
            DateEnd = dbCamp.DTA_END;
            Tipologia = dbCamp.COD_TIPOLOGIA;
            Schede = new List<CampagnaScheda>();
            if (dbCamp.XR_VAL_CAMPAIGN_SHEET != null && dbCamp.XR_VAL_CAMPAIGN_SHEET.Count() > 0)
            {
                foreach (var item in dbCamp.XR_VAL_CAMPAIGN_SHEET.Where(x => x.VALID_DTA_INI <= DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END >= DateTime.Now)))
                {
                    Schede.Add(new CampagnaScheda(item));
                }
            }
        }

        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public int Id_Sheet { get; set; }

        public DateTime ValidStart { get; set; }

        public DateTime? ValidEnd { get; set; }

        [Required]
        public DateTime DateStart { get; set; }

        [Required]
        public DateTime DateEnd { get; set; }

        public List<CampagnaScheda> Schede { get; set; }

        public bool IsActive()
        {
            return DateStart <= DateTime.Now && DateEnd >= DateTime.Now;
        }

        public bool IsEnded()
        {
            return DateStart <= DateTime.Now && DateEnd < DateTime.Now;
        }

        public string Tipologia { get; set; }
    }

    public class ReportCampagnaScheda
    {
        public ReportCampagnaScheda()
        {

        }

        public CampagnaScheda CampagnaScheda { get; set; }
        public List<Valutazione> Valutazioni { get; set; }
        public Sheet Scheda { get; set; }
        public List<DatoDomanda> ElencoDati { get; set; }

        public XR_VAL_QUESTION Domanda { get; set; }
        public XR_VAL_QUESTION_ANSWER Risposta { get; set; }
    }

    public class CampagnaScheda
    {
        public CampagnaScheda()
        {
            ID = 0;
            Id_Sheet = 0;
            Id_Campagna = 0;
            Nome = "";
            AllowEmployeeView = false;
            AllowExtEvaluator = (int)ValutazioniExtEvalPermission.Autorizzato;
            Autovalutazione = (int)ValutazioniAuto.No;
            AllowDelegation = false;
            OsservazioneDataInizio = null;
            OsservazioneDataFine = null;
            Qualifiche = new List<string>();
            QualificheInt = new List<int>();
            Servizi = new List<string>();
            Uorg = new List<string>();
            PianoSviluppo = false;
            //Albero = new IncarichiTreeModel();
            //Albero.GetModel();
        }

        public CampagnaScheda(int idCampagna = 0)
        {
            ID = 0;
            Id_Sheet = 0;
            Id_Campagna = idCampagna;
            Nome = "";
            OsservazioneDataInizio = null;
            OsservazioneDataFine = null;
            AllowEmployeeView = false;
            AllowExtEvaluator = (int)ValutazioniExtEvalPermission.Autorizzato;
            Autovalutazione = (int)ValutazioniAuto.No;
            AllowDelegation = false;
            Qualifiche = new List<string>();
            QualificheInt = new List<int>();
            Servizi = new List<string>();
            Uorg = new List<string>();
            PianoSviluppo = false;
            CodicePianoSviluppo = null;
            //Albero = new IncarichiTreeModel();
            //Albero.GetModel();
        }

        public CampagnaScheda(XR_VAL_CAMPAIGN_SHEET dbSheet, bool loadForReport = false)
        {
            ID = dbSheet.ID_CAMPAIGN_SHEET;
            Id_Campagna = dbSheet.ID_CAMPAIGN;
            Campagna_Name = dbSheet.XR_VAL_CAMPAIGN.NAME;
            Campagna_Des = dbSheet.XR_VAL_CAMPAIGN.DESCRIPTION;
            TipologiaCampagna = dbSheet.XR_VAL_CAMPAIGN.COD_TIPOLOGIA;
            Id_Sheet = dbSheet.ID_SHEET;
            Nome = dbSheet.XR_VAL_EVAL_SHEET.NAME;
            Descrizione = dbSheet.DESCRIPTION;
            OsservazioneDataInizio = dbSheet.DTA_OBSERVATION_INI;
            OsservazioneDataFine = dbSheet.DTA_OBSERVATION_END;
            ValidStart = dbSheet.VALID_DTA_INI;
            ValidEnd = dbSheet.VALID_DTA_END;
            AllowEmployeeView = dbSheet.EMPLOYEE_VIEW.GetValueOrDefault();
            AllowExtEvaluator = dbSheet.EXT_EVALUATOR.GetValueOrDefault();
            Autovalutazione = dbSheet.AUTOEVAL.GetValueOrDefault();
            AllowDelegation = dbSheet.DELEGATION;
            PianoSviluppo = dbSheet.IND_PIANOSVIL.GetValueOrDefault();
            CodicePianoSviluppo = dbSheet.COD_PIANOSVIL;
            IdObjColl = dbSheet.ID_OBJ_COLL;
            
            Qualifiche = new List<string>();
            QualificheInt = new List<int>();
            if (dbSheet.XR_VAL_CAMPAIGN_SHEET_QUAL != null && dbSheet.XR_VAL_CAMPAIGN_SHEET_QUAL.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Count() > 0)
            {
                Qualifiche.AddRange(dbSheet.XR_VAL_CAMPAIGN_SHEET_QUAL.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Select(x => x.XR_VAL_QUAL_FILTER.DESCRIPTION));
                QualificheInt.AddRange(dbSheet.XR_VAL_CAMPAIGN_SHEET_QUAL.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Select(x => x.XR_VAL_QUAL_FILTER.ID_QUAL_FILTER));
            }
            Servizi = new List<string>();
            if (dbSheet.XR_VAL_CAMPAIGN_SHEET_SER != null && dbSheet.XR_VAL_CAMPAIGN_SHEET_SER.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Count() > 0)
            {
                if (loadForReport)
                    Servizi.AddRange(dbSheet.XR_VAL_CAMPAIGN_SHEET_SER.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Select(x => x.XR_TB_SERVIZIO.DES_SERVIZIO));
                else
                    Servizi.AddRange(dbSheet.XR_VAL_CAMPAIGN_SHEET_SER.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Select(x => x.COD_SERVIZIO));
            }
            Uorg = new List<string>();
            if (dbSheet.XR_VAL_CAMPAIGN_SHEET_UORG != null && dbSheet.XR_VAL_CAMPAIGN_SHEET_UORG.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Count() > 0)
            {
                Uorg.AddRange(dbSheet.XR_VAL_CAMPAIGN_SHEET_UORG.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Select(x => x.COD_UNITAORG));
            }
            
            //Albero = new IncarichiTreeModel();
            //Albero.GetModel();
        }

        public int ID { get; set; }

        public string Descrizione { get; set; }

        public int Id_Campagna { get; set; }
        public string Campagna_Name { get; set; }
        public string Campagna_Des { get; set; }
        public int Id_Sheet { get; set; }

        public string Nome { get; set; }

        public DateTime? OsservazioneDataInizio { get; set; }

        public DateTime? OsservazioneDataFine { get; set; }

        public List<string> Qualifiche { get; set; }

        public List<int> QualificheInt { get; set; }

        public List<string> Servizi { get; set; }

        public List<string> Uorg { get; set; }

        public DateTime ValidStart { get; set; }

        public DateTime? ValidEnd { get; set; }

        public int AllowExtEvaluator { get; set; }

        public bool AllowEmployeeView { get; set; }

        public bool AllowDelegation { get; set; }

        public int Autovalutazione { get; set; }
        public bool PianoSviluppo { get; set; }
        public string CodicePianoSviluppo { get; set; }
        public string TipologiaCampagna { get; set; }
        public int? IdObjColl { get; set; }

        //public IncarichiTreeModel Albero { get; set; }
    }

    public class Valutazione
    {
        public Valutazione()
        {
            IdValutazione = 0;
            Rating = new List<XR_VAL_EVAL_RATING>();
        }
        public string Owner { get; set; }
        public int IdValutazione { get; set; }
        public XR_VAL_EVALUATOR Valutatore { get; set; }
        public SINTESI1 Persona { get; set; }
        public XR_VAL_CAMPAIGN_SHEET CampagnaScheda { get; set; }
        public XR_VAL_EVAL_SHEET Scheda { get; set; }
        public List<XR_VAL_EVAL_RATING> Rating { get; set; }
        public bool VistaResponsabile { get; set; }
        public bool CanModify { get; set; }
        public bool CanRespModify { get; set; }
        public XR_VAL_DELEGATION Delegato { get; set; }
        public XR_VAL_DELEGATION Delegante { get; set; }
        public ValutatoreEsterno ValutatoreEsterno { get; set; }
        public XR_VAL_EVALUATION_NOTE NotaResponsabile { get; set; }
        public bool Preview { get; set; }
        public int Stato { get; set; }
        public double Media { get; set; }
        public bool IsAutoValutazione { get; set; }
        public int IdScheda { get; set; }
        public int? AnalizzataDaRuo { get; set; }
        public XR_VAL_EVALUATION_NOTE NotaAnalisiRuo { get; set; }
        public object PianoSviluppo { get; set; }
        public string NomePianoSviluppo { get;  set; }
        public XR_VAL_EVALUATION DbEval { get; set; }

        public Valutazione GetValByOwner(string owner)
        {
            Valutazione newVal = (Valutazione)this.MemberwiseClone();
            newVal.Owner = owner;
            return newVal;
        }
    }

    public class ValutatoreEsternoContainer
    {
        public ValutatoreEsternoContainer()
        {
            Valutatori = new List<ValutatoreEsterno>();
        }
        public List<ValutatoreEsterno> Valutatori { get; set; }
    }

    public class ValutatoreEsterno
    {
        public int IdValutatore { get; set; }
        public int IdValutazione { get; set; }
        public int IdPersonaValued { get; set; }
        public int IdPersonaSel { get; set; }
        public string DatoRichiesta { get; set; }
        public string NoteRequest { get; set; }
        public bool? Approved { get; set; }
        public string NoteApproved { get; set; }
        public int IdExtVal { get; set; }

        public int AutoVal { get; set; }

        public RoleContainer ActualEvaluator { get; set; }
        public SINTESI1 SelectedEvaluator { get; set; }
        public XR_VAL_EVALUATOR_EXT ExternalEvaluator { get; set; }
    }

    public class ValutazioniQualFilter
    {
        public ValutazioniQualFilter(XR_VAL_QUAL_FILTER x)
        {
            CatIncluded = !String.IsNullOrWhiteSpace(x.QUAL_INCLUDED) ? x.QUAL_INCLUDED.Split(',').ToList() : new List<string>();
            CatExcluded = !String.IsNullOrWhiteSpace(x.QUAL_EXCLUDED) ? x.QUAL_EXCLUDED.Split(',').ToList() : new List<string>();
            Level = x.LEVEL;
        }
        public List<string> CatIncluded { get; set; }
        public List<string> CatExcluded { get; set; }
        public int? Level { get; set; }
    }

    public class IncarichiContainer
    {
        public IncarichiContainer()
        {
            ListIncarichi = new List<myRaiDataTalentia.XR_STR_TINCARICO>();
            NodiAlbero = new List<myRaiDataTalentia.XR_STR_TALBERO>();
            ListSezioni = new List<myRaiDataTalentia.XR_STR_TSEZIONE>();
        }
        public List<myRaiDataTalentia.XR_STR_TINCARICO> ListIncarichi { get; set; }
        public List<myRaiDataTalentia.XR_STR_TALBERO> NodiAlbero { get; set; }
        public List<myRaiDataTalentia.XR_STR_TSEZIONE> ListSezioni { get; set; }
        public List<ValutazioniQualFilter> QualFilter { get; set; }
    }

    public class RicercaValutazione
    {
        public RicercaValutazione()
        {
            Valutatore = -1;
        }

        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string Direzione { get; set; }
        public int Campagna { get; set; }
        public int CampagnaScheda { get; set; }
        public int DelegheEmesse { get; set; }
        public int DelegheRicevute { get; set; }
        public int GestitoInDelega { get; set; }
        public int RichiestaExtVal { get; set; }
        public int Valutatore { get; set; }
        public int Stato { get; set; }
        public bool MostraIniziativeChiuse { get; set; }

        public bool HasFilter { get; set; }
        public string BoxDest { get; set; }
        public string ResultView { get; set; }
        public ValutazioniView ActorView { get; set; }
    }

    public class SezResp
    {
        public SezResp()
        {
            Responsabili = new List<myRaiDataTalentia.XR_STR_TINCARICO>();
        }
        public int Sezione { get; set; }
        public List<myRaiDataTalentia.XR_STR_TINCARICO> Responsabili { get; set; }
    };

    public class ValutazioniPermission
    {
        public bool Evaluation { get; set; }
        public bool EvaluationResp { get; set; }
        public bool Delegation { get; set; }
        public bool Campaign { get; set; }
        public bool Request { get; set; }
    }

    public class ValutazioniParamMail
    {
        public bool Enabled { get; set; }
        public string Oggetto { get; set; }
        public string Corpo { get; set; }

        public ValutazioniParamMail(string chiave)
        {
            bool isEnabled = false;
            XR_VAL_PARAM param = null;
            if (ValutazioniHelper.GetParametro(chiave, out param) && bool.TryParse(param.VALORE1, out isEnabled))
            {
                Enabled = true;
                Oggetto = param.VALORE2;
                Corpo = param.VALORE3;
            }
        }
    }

    public class TreeNode
    {
        public TreeNode()
        {
            text = new TreeNodeText();
            //children = new List<TreeNode>();
            link = new TreeNodeLink();
            stackChildren = true;
        }
        public bool? collapsed { get; set; }
        public TreeNodeText text { get; set; }
        public TreeNodeLink link { get; set; }
        public int ordine { get; set; }

        public bool? stackChildren { get; set; }

        public List<TreeNode> children { get; set; }
    }

    public class TreeNodeText
    {
        public string name { get; set; }
        //public string title { get; set; }
        //public string desc { get; set; }
    }
    public class TreeNodeLink
    {
        public string href { get; set; }
    }
}