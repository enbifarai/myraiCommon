using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.Linq;
using myRaiCommonModel.Gestionale;
using myRaiHelper;
using System.Web.UI.WebControls;
using myRaiDataTalentia;
using SINTESI1 = myRaiData.Incentivi.SINTESI1;
using System.Linq.Expressions;
using myRaiCommonModel;
using System.Web.Script.Serialization;
using System.Reflection;
using myRai.Business;

namespace myRaiCommonManager
{
    public class ValutazioniManager
    {
        public static void CreazioneNuovaScheda()
        {
            string user = "ADMIN";
            string termid = "BATCHSESSION";
            DateTime tms = DateTime.Now;

            //Creazione risposte.
            List<XR_VAL_ANSWER> listAnswers = new List<XR_VAL_ANSWER>();
            List<XR_VAL_ANSWER> listPrestazioni = new List<XR_VAL_ANSWER>();
            List<XR_VAL_ANSWER> listAnswSpec = new List<XR_VAL_ANSWER>();
            using (var db = new IncentiviEntities())
            {
                //Competenze
                XR_VAL_ANSWER answer = null;
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Mai",
                    NOT_HELP = "Non mette mai in pratica il comportamento",
                    VALUE_INT = 1,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listAnswers.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Quasi mai",
                    NOT_HELP = "Mette in pratica il comportamento solo in alcune occasioni, sporadicamente",
                    VALUE_INT = 2,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listAnswers.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "A volte",
                    NOT_HELP = "Mette in pratica il comportamento mediamente/abitualmente",
                    VALUE_INT = 3,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listAnswers.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Spesso",
                    NOT_HELP = "Mette in pratica il comportamento nella maggior parte delle situazioni",
                    VALUE_INT = 4,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listAnswers.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Sempre",
                    NOT_HELP = "Mette in pratica il comportamento in tutte le occasioni che si presentano, con costanza",
                    VALUE_INT = 5,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listAnswers.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);

                //Prestazioni
                answer = null;
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Mai",
                    NOT_HELP = "Non risulta efficace in nessuna occasione",
                    VALUE_INT = 1,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listPrestazioni.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Quasi mai",
                    NOT_HELP = "Efficace solo in alcune occasioni",
                    VALUE_INT = 2,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listPrestazioni.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "A volte",
                    NOT_HELP = "Mediamente efficace",
                    VALUE_INT = 3,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listPrestazioni.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Spesso",
                    NOT_HELP = "Efficace nella maggior parte delle situazioni",
                    VALUE_INT = 4,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listPrestazioni.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Sempre",
                    NOT_HELP = "Sempre efficace",
                    VALUE_INT = 5,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listPrestazioni.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);

                //Specialistiche
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Base",
                    NOT_HELP = "Conoscenza della materia di base, applicazione non autonoma nello svolgimento delle attività subordinata al coordinamento operativo da parte di supervisori",
                    VALUE_INT = 1,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listAnswSpec.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Parziale",
                    NOT_HELP = "Possesso della competenza sufficiente per una applicazione operativa in situazioni standardizzate di cui sia prevedibile l’evoluzione, in presenza di una figura che ne indirizzi l’applicazione",
                    VALUE_INT = 2,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listAnswSpec.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Adeguata",
                    NOT_HELP = "Padronanza della materia che consente di operare pienamente di produrre risultati in sostanziale autonomia anche in situazioni non standardizzate",
                    VALUE_INT = 3,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listAnswSpec.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Avanzata",
                    NOT_HELP = "Conoscenza elevata dei diversi aspetti che caratterizzano la competenza, tale da consentire il raggiungimento di risultati anche in situazioni di complessità elevata ed in contesti di incertezza",
                    VALUE_INT = 4,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listAnswSpec.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);
                answer = new XR_VAL_ANSWER()
                {
                    ID_ANSWER = db.XR_VAL_ANSWER.GeneraPrimaryKey(9),
                    DESCRIPTION = "Eccellente",
                    NOT_HELP = "Padronanza della materia a livello di riferimento professionale all’interno dell’azienda tale da consentire la diffusione della materia stessa e da permettere il miglioramento continuo",
                    VALUE_INT = 5,
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listAnswSpec.Add(answer);
                db.XR_VAL_ANSWER.Add(answer);

                db.SaveChanges();
            }

            //Crezione gruppi
            List<XR_VAL_QUESTION_GROUP> listSubGroup = new List<XR_VAL_QUESTION_GROUP>();
            using (var db = new IncentiviEntities())
            {
                XR_VAL_QUESTION_GROUP subGroup = null;
                XR_VAL_QUESTION_GROUP osservabili = new XR_VAL_QUESTION_GROUP()
                {
                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(9),
                    NAME = "INDICATORI COMPORTAMENTALI OSSERVABILI",
                    DESCRIPTION = "INDICATORI COMPORTAMENTALI OSSERVABILI",
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                db.XR_VAL_QUESTION_GROUP.Add(osservabili);

                subGroup = new XR_VAL_QUESTION_GROUP()
                {
                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(9),
                    NAME = "TRASVERSALI",
                    DESCRIPTION = "TRASVERSALI",
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    ID_QST_GROUP_MACRO = osservabili.ID_QST_GROUP,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listSubGroup.Add(subGroup);
                db.XR_VAL_QUESTION_GROUP.Add(subGroup);

                subGroup = new XR_VAL_QUESTION_GROUP()
                {
                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(9),
                    NAME = "DIGITALI TRASVERSALI",
                    DESCRIPTION = "DIGITALI TRASVERSALI",
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    ID_QST_GROUP_MACRO = osservabili.ID_QST_GROUP,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listSubGroup.Add(subGroup);
                db.XR_VAL_QUESTION_GROUP.Add(subGroup);

                subGroup = new XR_VAL_QUESTION_GROUP()
                {
                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(9),
                    NAME = "SPECIALISTICHE",
                    DESCRIPTION = "SPECIALISTICHE",
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    ID_QST_GROUP_MACRO = osservabili.ID_QST_GROUP,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listSubGroup.Add(subGroup);
                db.XR_VAL_QUESTION_GROUP.Add(subGroup);


                XR_VAL_QUESTION_GROUP prestazione = new XR_VAL_QUESTION_GROUP()
                {
                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(9),
                    NAME = "PRESTAZIONE",
                    DESCRIPTION = "PRESTAZIONE",
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                db.XR_VAL_QUESTION_GROUP.Add(prestazione);
                subGroup = new XR_VAL_QUESTION_GROUP()
                {
                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(9),
                    NAME = "INDICATORI COMPORTAMENTALI OSSERVABILI",
                    DESCRIPTION = "INDICATORI COMPORTAMENTALI OSSERVABILI",
                    VALID_DTA_INI = tms,
                    VALID_DTA_END = null,
                    ID_QST_GROUP_MACRO = prestazione.ID_QST_GROUP,
                    COD_USER = user,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = tms
                };
                listSubGroup.Add(subGroup);
                db.XR_VAL_QUESTION_GROUP.Add(subGroup);

                db.SaveChanges();
            }

            var lines = System.IO.File.ReadAllLines(@"C:\Users\Nik\Desktop\RAI_File\HRIS\Valutazioni\domande.csv", System.Text.Encoding.GetEncoding(1252)).Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Split(';'));
            var linesDescr = System.IO.File.ReadAllLines(@"C:\Users\Nik\Desktop\RAI_File\HRIS\Valutazioni\descrittori.csv", System.Text.Encoding.GetEncoding(1252)).Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Split(';'));

            var listQuestion = new List<XR_VAL_QUESTION>();
            var listSheet = new List<XR_VAL_EVAL_SHEET>();
            XR_VAL_QUESTION_DISPLAY displayRadio = null;
            XR_VAL_QUESTION_TYPE typeIntero = null;
            using (var db = new IncentiviEntities())
            {
                //display
                displayRadio = db.XR_VAL_QUESTION_DISPLAY.FirstOrDefault(x => x.NAME == "Radio button");
                //type
                typeIntero = db.XR_VAL_QUESTION_TYPE.FirstOrDefault(x => x.NAME == "Intero");

                foreach (var grSch in lines.GroupBy(x => x[0]))
                {
                    XR_VAL_EVAL_SHEET scheda = new XR_VAL_EVAL_SHEET()
                    {
                        ID_SHEET = db.XR_VAL_EVAL_SHEET.GeneraPrimaryKey(9),
                        NAME = grSch.Key,
                        DESCRIPTION = grSch.Key,
                        VALID_DTA_INI = tms,
                        VALID_DTA_END = null,
                        COD_USER = user,
                        COD_TERMID = termid,
                        TMS_TIMESTAMP = tms
                    };
                    listSheet.Add(scheda);
                    db.XR_VAL_EVAL_SHEET.Add(scheda);

                    foreach (var grGr in grSch.GroupBy(x => x[1]))
                    {
                        var gr = listSubGroup.FirstOrDefault(x => x.NAME == grGr.Key);

                        foreach (var grSub in grGr.GroupBy(x => x[2]))
                        {
                            var sub = listSubGroup.FirstOrDefault(x => x.NAME == grSub.Key);
                            if (sub == null)
                            {
                                var subGrDes = linesDescr.FirstOrDefault(x => x[0] == gr.NAME && x[1] == grSub.Key);

                                sub = new XR_VAL_QUESTION_GROUP()
                                {
                                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(9),
                                    NAME = grSub.Key,
                                    DESCRIPTION = subGrDes != null ? subGrDes[2] : "",
                                    ID_QST_GROUP_MACRO = gr.ID_QST_GROUP,
                                    VALID_DTA_INI = tms,
                                    VALID_DTA_END = null,
                                    COD_USER = user,
                                    COD_TERMID = termid,
                                    TMS_TIMESTAMP = tms
                                };
                                listSubGroup.Add(sub);
                                db.XR_VAL_QUESTION_GROUP.Add(sub);
                            }
                        }
                    }

                    db.SaveChanges();
                }
            }

            foreach (var grSch in lines.GroupBy(x => x[0]))
            {
                XR_VAL_EVAL_SHEET scheda = listSheet.FirstOrDefault(x => x.NAME == grSch.Key);
                int orderQst = 0;
                using (var db = new IncentiviEntities())
                {
                    foreach (var grGr in grSch.GroupBy(x => x[1]))
                    {
                        var gr = listSubGroup.FirstOrDefault(x => x.NAME == grGr.Key);

                        foreach (var grSub in grGr.GroupBy(x => x[2]))
                        {
                            var sub = listSubGroup.FirstOrDefault(x => x.NAME == grSub.Key);


                            foreach (var lqst in grSub)
                            {
                                var qst = listQuestion.FirstOrDefault(x => x.NAME == lqst[3]);
                                if (qst == null)
                                {
                                    qst = new XR_VAL_QUESTION()
                                    {
                                        ID_QUESTION = db.XR_VAL_QUESTION.GeneraPrimaryKey(9),
                                        NAME = lqst[3],
                                        DESCRIPTION = "",
                                        ID_QST_DISPLAY = displayRadio.ID_QST_DISPLAY,
                                        ID_QST_TYPE = typeIntero.ID_QST_TYPE,
                                        ID_QST_GROUP = sub.ID_QST_GROUP,
                                        VALID_DTA_INI = tms,
                                        VALID_DTA_END = null,
                                        COD_USER = user,
                                        COD_TERMID = termid,
                                        TMS_TIMESTAMP = tms
                                    };
                                    listQuestion.Add(qst);
                                    db.XR_VAL_QUESTION.Add(qst);

                                    int order = 0;
                                    if (lqst[1] == "INDICATORI COMPORTAMENTALI OSSERVABILI")
                                    {
                                        foreach (var answ in listPrestazioni)
                                        {
                                            var qstAnsw = new XR_VAL_QUESTION_ANSWER()
                                            {
                                                ID_QST_ANSWER = db.XR_VAL_QUESTION_ANSWER.GeneraPrimaryKey(9),
                                                ID_QUESTION = qst.ID_QUESTION,
                                                DESCRIPTION = "",
                                                ID_ANSWER = answ.ID_ANSWER,
                                                NUM_ORDER = ++order,
                                                VALID_DTA_INI = tms,
                                                VALID_DTA_END = null,
                                                COD_USER = user,
                                                COD_TERMID = termid,
                                                TMS_TIMESTAMP = tms
                                            };
                                            db.XR_VAL_QUESTION_ANSWER.Add(qstAnsw);
                                        }
                                    }
                                    else if (lqst[1] == "SPECIALISTICHE")
                                    {
                                        foreach (var answ in listAnswSpec)
                                        {
                                            var qstAnsw = new XR_VAL_QUESTION_ANSWER()
                                            {
                                                ID_QST_ANSWER = db.XR_VAL_QUESTION_ANSWER.GeneraPrimaryKey(9),
                                                ID_QUESTION = qst.ID_QUESTION,
                                                DESCRIPTION = "",
                                                ID_ANSWER = answ.ID_ANSWER,
                                                NUM_ORDER = ++order,
                                                VALID_DTA_INI = tms,
                                                VALID_DTA_END = null,
                                                COD_USER = user,
                                                COD_TERMID = termid,
                                                TMS_TIMESTAMP = tms
                                            };
                                            db.XR_VAL_QUESTION_ANSWER.Add(qstAnsw);
                                        }
                                    }
                                    else
                                    {
                                        foreach (var answ in listAnswers)
                                        {
                                            var qstAnsw = new XR_VAL_QUESTION_ANSWER()
                                            {
                                                ID_QST_ANSWER = db.XR_VAL_QUESTION_ANSWER.GeneraPrimaryKey(9),
                                                ID_QUESTION = qst.ID_QUESTION,
                                                DESCRIPTION = "",
                                                ID_ANSWER = answ.ID_ANSWER,
                                                NUM_ORDER = ++order,
                                                VALID_DTA_INI = tms,
                                                VALID_DTA_END = null,
                                                COD_USER = user,
                                                COD_TERMID = termid,
                                                TMS_TIMESTAMP = tms
                                            };
                                            db.XR_VAL_QUESTION_ANSWER.Add(qstAnsw);
                                        }
                                    }
                                }

                                db.XR_VAL_EVAL_SHEET_QST.Add(new XR_VAL_EVAL_SHEET_QST()
                                {
                                    ID_SHEET_QST = db.XR_VAL_EVAL_SHEET_QST.GeneraPrimaryKey(9),
                                    ID_SHEET = scheda.ID_SHEET,
                                    ID_QUESTION = qst.ID_QUESTION,
                                    ORDER = ++orderQst,
                                    VALID_DTA_INI = tms,
                                    VALID_DTA_END = null,
                                    WEIGHT = 0,
                                    COD_USER = user,
                                    COD_TERMID = termid,
                                    TMS_TIMESTAMP = tms
                                });
                            }
                        }
                    }
                    db.SaveChanges();
                }
            }
        }

        public static void CreazioneSchedaMBO()
        {
            var db = new IncentiviEntities();

            var scheda = db.XR_VAL_EVAL_SHEET.Find(1);
            foreach (var qst in scheda.XR_VAL_EVAL_SHEET_QST)
            {
                if (qst.XR_VAL_QUESTION.XR_VAL_QUESTION_DISPLAY.NAME == "Select")
                {
                    for (int i = 0; i < 4; i++)
                    {
                        XR_VAL_QUESTION_ANSWER qstAnsw = new XR_VAL_QUESTION_ANSWER()
                        {
                            ID_QST_ANSWER = db.XR_VAL_QUESTION_ANSWER.GeneraPrimaryKey(),
                            ID_QUESTION = qst.ID_QUESTION,
                            NUM_ORDER = i + 1,
                            DESCRIPTION = "",
                            ID_ANSWER = 10 + i + 1,
                            VALID_DTA_INI = DateTime.Now,
                            COD_USER = "ADMIN",
                            COD_TERMID = "BATCHSESSION",
                            TMS_TIMESTAMP = DateTime.Now
                        };
                        db.XR_VAL_QUESTION_ANSWER.Add(qstAnsw);
                    }
                }
                else if (qst.XR_VAL_QUESTION.XR_VAL_QUESTION_TYPE.NAME== "Intero")
                {
                    for (int i = 0; i < 5; i++)
                    {
                        XR_VAL_QUESTION_ANSWER qstAnsw = new XR_VAL_QUESTION_ANSWER()
                        {
                            ID_QST_ANSWER = db.XR_VAL_QUESTION_ANSWER.GeneraPrimaryKey(),
                            ID_QUESTION = qst.ID_QUESTION,
                            NUM_ORDER = i + 1,
                            DESCRIPTION = "",
                            ID_ANSWER = i + 1,
                            VALID_DTA_INI = DateTime.Now,
                            COD_USER="ADMIN",
                            COD_TERMID = "BATCHSESSION",
                            TMS_TIMESTAMP = DateTime.Now
                        };
                        db.XR_VAL_QUESTION_ANSWER.Add(qstAnsw);
                    }
                }
            }

            db.SaveChanges();
        }

        public static ValutazioniPermission GetPermission(string matricola)
        {
            ValutazioniPermission permission = new ValutazioniPermission();
            using (var db = new IncentiviEntities())
            {
                var filterQual = ValutazioniManager.FilterDifferentEvaluator(db, matricola);
                var me = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
                bool anyPossibleDelegate = db.SINTESI1.Where(filterQual).Where(x => x.COD_MATLIBROMAT != matricola && x.DTA_FINE_CR > DateTime.Today && x.ID_UNITAORG == me.ID_UNITAORG).Any();

                permission.Evaluation = ValutazioniHelper.IsEnabled(matricola);
                permission.Delegation = anyPossibleDelegate && !db.XR_VAL_EVALUATOR_EXT.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)
                                                                                       .Any(x => x.IND_APPROVED.HasValue && x.IND_APPROVED.Value && x.EXT_EVALUATOR.COD_MATLIBROMAT == matricola);
                permission.Request = permission.Delegation && !db.XR_VAL_DELEGATION.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)
                                                                                   .Where(x => x.DTA_START < DateTime.Now && (x.DTA_END == null || x.DTA_END > DateTime.Now))
                                                                                   .Any(x => x.DELEGATO.SINTESI1.COD_MATLIBROMAT == matricola);
                permission.Campaign = ValutazioniHelper.IsEnabledGest(CommonManager.GetCurrentUserMatricola());
            }

            return permission;
        }

        public static List<XR_VAL_EVALUATOR> GetActiveRoles(IncentiviEntities db, string matricola, bool showEndedCampaign, ValutazioniLoader loader = ValutazioniLoader.Completo, RicercaValutazione ricerca = null)
        {
            List<XR_VAL_EVALUATOR> activeRoles = new List<XR_VAL_EVALUATOR>();

            if (db == null)
                db = new IncentiviEntities();

            IQueryable<XR_VAL_EVALUATOR> tmp = null;
            switch (loader)
            {
                case ValutazioniLoader.Minimo:
                    tmp = db.XR_VAL_EVALUATOR
                        .Include("DELEGATO")
                        .Include("SINTESI1")
                        .Where(ValutazioniExtension.ExprFuncValidValuator());
                    break;
                case ValutazioniLoader.Completo:
                case ValutazioniLoader.Report:
                    tmp = db.XR_VAL_EVALUATOR
                        .Include("SINTESI1")
                        .Include("XR_VAL_CAMPAIGN_SHEET")
                        .Include("XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN")
                        .Include("DELEGATO")
                        .Include("DELEGATO.DELEGANTE.SINTESI1")
                        .Include("DELEGATO.XR_VAL_DELEGATION_PERS")
                        .Include("DELEGATO.XR_VAL_DELEGATION_PERS.SINTESI1")
                        .Include("DELEGANTE")
                        .Include("DELEGANTE.DELEGATO.SINTESI1")
                        .Include("DELEGANTE.XR_VAL_DELEGATION_PERS")
                        .Include("DELEGANTE.XR_VAL_DELEGATION_PERS.SINTESI1")
                        .Where(ValutazioniExtension.ExprFuncValidValuator());
                    break;
                case ValutazioniLoader.MieiValutatori:
                    tmp = db.XR_VAL_EVALUATOR
                        .Include("SINTESI1")
                        .Include("DELEGATO")
                        .Include("XR_VAL_CAMPAIGN_SHEET")
                        .Include("XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN")
                        .Where(ValutazioniExtension.ExprFuncValidValuator())
                        .Where(x => (x.XR_VAL_CAMPAIGN_SHEET.EMPLOYEE_VIEW != null && x.XR_VAL_CAMPAIGN_SHEET.EMPLOYEE_VIEW.Value)
                                        || x.XR_VAL_CAMPAIGN_SHEET.EXT_EVALUATOR == (int)ValutazioniExtEvalPermission.Autorizzato
                                        || x.XR_VAL_CAMPAIGN_SHEET.EXT_EVALUATOR == (int)ValutazioniExtEvalPermission.Obbligatorio
                                        || (x.XR_VAL_CAMPAIGN_SHEET.AUTOEVAL != null && x.XR_VAL_CAMPAIGN_SHEET.AUTOEVAL != (int)ValutazioniAuto.No));
                    break;
                default:
                    break;
            }

            if (!String.IsNullOrWhiteSpace(matricola))
                tmp = tmp.Where(x => x.SINTESI1.COD_MATLIBROMAT == matricola);

            if (!showEndedCampaign)
                tmp = tmp.Where(ValutazioniExtension.ExprFuncEvalutatorWithActiveCampaign());

            if (ricerca != null && ricerca.HasFilter)
            {
                if (ricerca.Campagna > 0)
                    tmp = tmp.Where(x => x.ID_CAMPAIGN == ricerca.Campagna);

                if (ricerca.CampagnaScheda > 0)
                    tmp = tmp.Where(x => x.ID_CAMPAIGN_SHEET == ricerca.CampagnaScheda);

                //Se il filtro prevede la ricerca sulle valutazioni con richieste di valutatore esterno
                //vengono caricati solo i ruoli la cui scheda prevede il valutatore esterno
                if (ricerca.RichiestaExtVal > 0 && ricerca.RichiestaExtVal == (int)ValutazioniExtEval.ConValutatoreEsterno)
                    tmp = tmp.Where(x => x.XR_VAL_CAMPAIGN_SHEET.EXT_EVALUATOR == (int)ValutazioniExtEvalPermission.Autorizzato || x.XR_VAL_CAMPAIGN_SHEET.EXT_EVALUATOR == (int)ValutazioniExtEvalPermission.Obbligatorio);

                //Se il filtro prevede la ricerca sulle richieste gestite in delega
                //vengono caricati solo i ruoli la cui scheda prevede la gestione delle deleghe
                if (ricerca.GestitoInDelega > 0 && ricerca.GestitoInDelega == (int)ValutazioniDelega.ConDelega)
                    tmp = tmp.Where(x => x.XR_VAL_CAMPAIGN_SHEET.DELEGATION);

                if (ricerca.Valutatore > 0)
                    tmp = tmp.Where(x => x.ID_PERSONA == ricerca.Valutatore);
            }

            var tmpList = tmp.ToList()
                            .Where(x => x.DELEGATO == null || !x.DELEGATO.Any() || x.DELEGATO.Any(y => y.DTA_START <= DateTime.Now && y.DTA_END > DateTime.Now && (y.VALID_DTA_END == null || y.VALID_DTA_END > DateTime.Now)))
                            .ToList();

            activeRoles.AddRange(tmpList);

            return activeRoles;
        }

        public static List<XR_VAL_EVALUATOR> GetActiveSubRoles(IncentiviEntities db, string matricola, bool showEndedCampaign, IncarichiContainer incarichiContainer, bool loadAllInfo = true, RicercaValutazione ricerca = null)
        {
            List<XR_VAL_EVALUATOR> activeSubRoles = new List<XR_VAL_EVALUATOR>();

            List<string> subMatr = new List<string>();
            /*
             * Se il ruolo di questa persona rientra tra i seguenti, non deve considerare i sotto incarichi
             //* 000003 - Direttore
             //* 000004 - Direttore di Testata
             * 000007 - Direttore Generale
             * 000008 - Presidente
             * 000010 - Amministratore delegato
             */
            var listExclRole = new string[]
            {
                //"000003",
                //"000004",
                "000007",
                "000008",
                "000010"
            };

            var allIncarichi = incarichiContainer.ListIncarichi.Where(x => x.matricola == matricola).ToList();
            if (allIncarichi.Any(x => listExclRole.Contains(x.cod_incarico)))
            {
                //Nel caso abbia uno dei ruoli di cui sopra, viene rimosso anche ad interim
                allIncarichi.RemoveWhere(x => x.cod_incarico == "000002");
            }
            allIncarichi.RemoveWhere(x => listExclRole.Contains(x.cod_incarico));

            var listIncarichi = allIncarichi.Select(y => y.id_sezione);
            if (listIncarichi != null && listIncarichi.Count() > 0)
            {
                foreach (var str in listIncarichi)
                {
                    var subStrList = incarichiContainer.NodiAlbero.Where(x => x.subordinato_a == str).Select(x => x.id);
                    if (subStrList != null && subStrList.Count() > 0)
                        subMatr.AddRange(incarichiContainer.ListIncarichi.Where(x => subStrList.Contains(x.id_sezione)).Select(y => y.matricola));
                }
            }

            if (subMatr != null && subMatr.Count() > 0)
            {
                var idPers = CommonManager.GetCurrentIdPersona();
                var subIdPers = db.SINTESI1.Where(x => subMatr.Contains(x.COD_MATLIBROMAT)).Select(x => x.ID_PERSONA).ToList();


                IQueryable<XR_VAL_EVALUATOR> tmp = null;
                if (loadAllInfo)
                    tmp = db.XR_VAL_EVALUATOR
                            .Include("SINTESI1")
                            .Include("XR_VAL_CAMPAIGN_SHEET")
                            .Include("XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN")
                            .Include("DELEGATO")
                            .Include("DELEGATO.DELEGANTE.SINTESI1")
                            .Include("DELEGATO.XR_VAL_DELEGATION_PERS")
                            .Include("DELEGATO.XR_VAL_DELEGATION_PERS.SINTESI1")
                            .Include("DELEGANTE")
                            .Include("DELEGANTE.DELEGATO.SINTESI1")
                            .Include("DELEGANTE.XR_VAL_DELEGATION_PERS")
                            .Include("DELEGANTE.XR_VAL_DELEGATION_PERS.SINTESI1")
                            .Where(ValutazioniExtension.ExprSubFuncValidValuator())
                            //.Where(x => x.SINTESI1.COD_MATLIBROMAT != matricola && subMatr.Contains(x.SINTESI1.COD_MATLIBROMAT));
                            .Where(x => x.ID_PERSONA != idPers && subIdPers.Contains(x.ID_PERSONA));
                else
                    tmp = db.XR_VAL_EVALUATOR
                            .Where(ValutazioniExtension.ExprSubFuncValidValuator())
                            //.Where(x => x.SINTESI1.COD_MATLIBROMAT != matricola && subMatr.Contains(x.SINTESI1.COD_MATLIBROMAT));
                            .Where(x => x.ID_PERSONA != idPers && subIdPers.Contains(x.ID_PERSONA));

                if (!showEndedCampaign)
                    tmp = tmp.Where(ValutazioniExtension.ExprFuncEvalutatorWithActiveCampaign());

                tmp = tmp.Where(x => !x.DELEGATO.Any());

                if (ricerca != null && ricerca.HasFilter)
                {
                    if (ricerca.Campagna > 0)
                        tmp = tmp.Where(x => x.ID_CAMPAIGN == ricerca.Campagna);

                    if (ricerca.CampagnaScheda > 0)
                        tmp = tmp.Where(x => x.ID_CAMPAIGN_SHEET == ricerca.CampagnaScheda);
                }

                activeSubRoles.AddRange(tmp);
            }

            return activeSubRoles;
        }



        public static XR_VAL_EVAL_SHEET GetEvalSheet(IncentiviEntities db, int idEvalSheet)
        {
            return db.XR_VAL_EVAL_SHEET
                                        .Include("XR_VAL_EVAL_SHEET_QST")
                                        .Include("XR_VAL_EVAL_SHEET_QST.XR_VAL_QUESTION")
                                        .Include("XR_VAL_EVAL_SHEET_QST.XR_VAL_QUESTION.XR_VAL_QUESTION_GROUP")
                                        .Include("XR_VAL_EVAL_SHEET_QST.XR_VAL_QUESTION.XR_VAL_QUESTION_ANSWER")
                                        .Include("XR_VAL_EVAL_SHEET_QST.XR_VAL_QUESTION.XR_VAL_QUESTION_ANSWER.XR_VAL_ANSWER")
                                        .FirstOrDefault(x => x.ID_SHEET == idEvalSheet);
        }

        public static Valutazione GetPreviewValutazione(int idEvalSheet)
        {
            Valutazione valutazione = new Valutazione();
            IncentiviEntities db = new IncentiviEntities();
            string matricola = CommonHelper.GetCurrentUserMatricola();
            valutazione.Persona = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
            valutazione.Scheda = GetEvalSheet(db, idEvalSheet);
            valutazione.CanModify = true;
            valutazione.VistaResponsabile = false;
            valutazione.CanRespModify = true;
            valutazione.Preview = true;

            return valutazione;
        }

        public static Valutazione GetValutazione(int idEvaluation, bool isGest, string owner = "")
        {
            Valutazione valutazione = new Valutazione();
            valutazione.Owner = owner;

            IncentiviEntities db = new IncentiviEntities();
            XR_VAL_EVALUATION eval = db.XR_VAL_EVALUATION.Find(idEvaluation);
            XR_VAL_EVALUATOR evaluator = eval.XR_VAL_EVALUATOR;

            valutazione.IdValutazione = eval.ID_EVALUATION;
            valutazione.Persona = eval.SINTESI1;
            valutazione.CampagnaScheda = eval.XR_VAL_CAMPAIGN_SHEET;
            valutazione.IdScheda = eval.XR_VAL_CAMPAIGN_SHEET.ID_SHEET;
            valutazione.Valutatore = evaluator;

            int valState = GetMaxEvalState(eval.XR_VAL_OPER_STATE);
            valutazione.Stato = valState;
            valutazione.CanModify = valState < (int)ValutazioniState.Analizzata;

            valutazione.AnalizzataDaRuo = eval.IND_OPERCHECKED;
            valutazione.NotaResponsabile = eval.XR_VAL_EVALUATION_NOTE.Where(x => x.COD_TIPO == "PresaVisione" && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)).FirstOrDefault();
            valutazione.NotaAnalisiRuo = eval.XR_VAL_EVALUATION_NOTE.Where(x => x.COD_TIPO == "AnalisiRUO" && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)).FirstOrDefault();

            valutazione.CanRespModify = valState > (int)ValutazioniState.Analizzata && valutazione.NotaResponsabile == null;

            valutazione.Scheda = GetEvalSheet(db, valutazione.IdScheda);

            valutazione.DbEval = eval;

            IEnumerable<XR_VAL_EVAL_RATING> ratings = eval.XR_VAL_EVAL_RATING.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now);

            //Questo controllo è da fare solo da raiperme
            if (!isGest && eval.ID_PERSONA == CommonManager.GetCurrentIdPersona())
            {
                //Se è il valutato ad aprire la scheda, bisogna filtrare l'owner in base ai parametri della scheda
                if (!valutazione.CampagnaScheda.EMPLOYEE_VIEW.GetValueOrDefault())
                    ratings = ratings.Where(x => x.XR_VAL_EVAL_RATING_OWNER.NAME == valutazione.Owner);
            }
            valutazione.Rating.AddRange(ratings);//.Where(x => x.XR_VAL_EVAL_RATING_OWNER.NAME == owner));

            valutazione.Delegante = evaluator.DELEGANTE.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Where(x => x.DTA_START < DateTime.Now && (x.DTA_END == null || x.DTA_END > DateTime.Now)).FirstOrDefault();
            valutazione.Delegato = evaluator.DELEGATO.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Where(x => x.DTA_START < DateTime.Now && (x.DTA_END == null || x.DTA_END > DateTime.Now)).FirstOrDefault();
            var tmp = valutazione.Persona.EXT_VALUED.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).FirstOrDefault(x => x.ID_EVALUATOR == evaluator.ID_EVALUATOR);
            if (tmp != null)
            {
                valutazione.ValutatoreEsterno = GetRequestExtEvaluator("", tmp.ID_EVALUATOR_EXT).Valutatori.First();
            }

            if (valutazione.CampagnaScheda.IND_PIANOSVIL.GetValueOrDefault() && valutazione.Stato >= (int)ValutazioniState.Analizzata)
            {
                GetPianoSviluppo(eval, valutazione.Stato, true, isGest, out string nomePiano, out object pianoSvil);
                valutazione.NomePianoSviluppo = nomePiano;
                valutazione.PianoSviluppo = pianoSvil;

            }


            return valutazione;
        }

        public static void GetPianoSviluppo(XR_VAL_EVALUATION eval, int state, bool loadPiano, bool isGest, out string nomePiano, out object piano)
        {
            string pianoNamespace = PianoSviluppo_Base.PIANO_NAMESPACE;
            string qualifiedName = typeof(PianoSviluppo_Base).AssemblyQualifiedName;

            piano = null;
            nomePiano = "";

            if (!String.IsNullOrWhiteSpace(eval.COD_PIANOSVIL))
            {
                nomePiano = eval.COD_PIANOSVIL;
                if (loadPiano)
                {
                    Type type = Type.GetType(qualifiedName.Replace(pianoNamespace + "PianoSviluppo_Base", pianoNamespace + nomePiano));
                    piano = Newtonsoft.Json.JsonConvert.DeserializeObject(eval.OBJ_PIANOSVIL, type);
                }
            }
            else
            {
                nomePiano = eval.XR_VAL_CAMPAIGN_SHEET.COD_PIANOSVIL;
                if (loadPiano)
                {
                    Type type = Type.GetType(qualifiedName.Replace(pianoNamespace + "PianoSviluppo_Base", pianoNamespace + nomePiano));
                    piano = Activator.CreateInstance(type);
                }
            }

            PianoSviluppo_Base t = (PianoSviluppo_Base)piano;
            t.IdValutazione = eval.ID_EVALUATION;
            t.NomePiano = nomePiano;
            t.CanModify = !isGest && state < (int)ValutazioniState.SviluppoCompilato;
            t.CanApprove = !isGest && state == (int)ValutazioniState.SviluppoCompilato && t.IdPersonaAutore != CommonHelper.GetCurrentIdPersona();
            t.IsApproved = state >= (int)ValutazioniState.SviluppoApprovato;
            t.Nota = eval.XR_VAL_EVALUATION_NOTE.Where(x => x.COD_TIPO == "AnalisiPianoSviluppo" && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)).FirstOrDefault();
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;

            assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == args.Name);
            if (assembly != null)
                return assembly;

            return assembly;
        }

        public static List<SheetContainer> GetSheetContainers(string matricola, RicercaValutazione ricerca, bool loadSubRoles, bool showEndedCampaign = false, ValutazioniLoader loader = ValutazioniLoader.Completo)
        {
            IncentiviEntities db = new IncentiviEntities();
            List<SheetContainer> sheetContainers = new List<SheetContainer>();
            IncarichiContainer incarichiContainer = null;
            if (loadSubRoles)
                incarichiContainer = GetIncarichiContainer();

            List<XR_VAL_EVALUATOR> activeRoles = GetActiveRoles(db, matricola, showEndedCampaign, loader, ricerca);
            List<XR_VAL_EVALUATOR> activeSubRoles = null;
            if (loadSubRoles)
                activeSubRoles = GetActiveSubRoles(db, matricola, showEndedCampaign, incarichiContainer, true, ricerca);

            foreach (var sheetGroup in activeRoles.GroupBy(x => x.ID_CAMPAIGN_SHEET))
            {
                SheetContainer sheetContainer = new SheetContainer();
                sheetContainer.Sheet = sheetGroup.First().XR_VAL_CAMPAIGN_SHEET;
                sheetContainer.Roles.AddRange(GetRoleContainers(db, sheetContainer.Sheet, sheetGroup, ricerca, loader));
                if (loadSubRoles)
                    sheetContainer.SubRoles.AddRange(GetRoleContainers(db, sheetContainer.Sheet, activeSubRoles.Where(x => x.ID_CAMPAIGN_SHEET == sheetContainer.Sheet.ID_CAMPAIGN_SHEET), ricerca, loader, true));

                sheetContainers.Add(sheetContainer);
            }

            if (loadSubRoles)
            {
                foreach (var subSheetGroup in activeSubRoles.GroupBy(x => x.ID_CAMPAIGN_SHEET).Where(x => !sheetContainers.Any(y => y.Sheet.ID_CAMPAIGN_SHEET == x.Key)))
                {
                    SheetContainer sheetContainer = new SheetContainer();
                    sheetContainer.Sheet = subSheetGroup.First().XR_VAL_CAMPAIGN_SHEET;
                    sheetContainer.SubRoles.AddRange(GetRoleContainers(db, sheetContainer.Sheet, subSheetGroup, ricerca, loader, true));

                    sheetContainers.Add(sheetContainer);
                }
            }

            return sheetContainers;
        }
        private static List<RoleContainer> GetRoleContainers(IncentiviEntities db, XR_VAL_CAMPAIGN_SHEET campaignSheet, IEnumerable<XR_VAL_EVALUATOR> roles, RicercaValutazione ricerca, ValutazioniLoader loader, bool isSubRole = false)
        {
            int idCampaignSheet = campaignSheet.ID_CAMPAIGN_SHEET;
            bool getDelegatedPers = ricerca == null || !ricerca.HasFilter || ricerca.GestitoInDelega == 0 || ricerca.GestitoInDelega == (int)ValutazioniDelega.ConDelega;
            bool getNotDelegatedPers = ricerca == null || !ricerca.HasFilter || ricerca.GestitoInDelega == 0 || ricerca.GestitoInDelega == (int)ValutazioniDelega.SenzaDelega;

            bool getExtVal = ricerca == null || !ricerca.HasFilter || ricerca.RichiestaExtVal == 0 || ricerca.RichiestaExtVal == (int)ValutazioniExtEval.ConValutatoreEsterno;
            bool getNotExtVal = ricerca == null || !ricerca.HasFilter || ricerca.RichiestaExtVal == 0 || ricerca.RichiestaExtVal == (int)ValutazioniExtEval.SenzaValutatoreEsterno;

            var listRole = roles.Select(x => x.ID_EVALUATOR);

            //var availableEvaluation = GetAvailableEvaluation(db, campaignSheet, ricerca, loader).ToList();
            IEnumerable<XR_VAL_EVALUATION> tmpAvEval = null;

            if (loader != ValutazioniLoader.Report)
                tmpAvEval = db.XR_VAL_EVALUATION
                                            .Include("XR_VAL_OPER_STATE")
                                            .Include("SINTESI1")
                                            .Include("SINTESI1.XR_VAL_DELEGATION_PERS")
                                            .Include("SINTESI1.XR_VAL_DELEGATION_PERS.XR_VAL_DELEGATION")
                                            .Include("SINTESI1.EXT_VALUED")
                                            .Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)
                                            .Where(x => x.ID_CAMPAIGN_SHEET == campaignSheet.ID_CAMPAIGN_SHEET && listRole.Contains(x.ID_EVALUATOR));
            else
                tmpAvEval = db.XR_VAL_EVALUATION
                                .Include("XR_VAL_OPER_STATE")
                                .Include("SINTESI1")
                                .Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)
                                .Where(x => x.ID_CAMPAIGN_SHEET == campaignSheet.ID_CAMPAIGN_SHEET && listRole.Contains(x.ID_EVALUATOR));

            if (ricerca != null && ricerca.HasFilter)
            {
                if (!String.IsNullOrWhiteSpace(ricerca.Matricola))
                    tmpAvEval = tmpAvEval.Where(x => ricerca.Matricola.Contains(x.SINTESI1.COD_MATLIBROMAT));

                if (!String.IsNullOrWhiteSpace(ricerca.Nominativo))
                    tmpAvEval = tmpAvEval.Where(x => (x.SINTESI1.DES_COGNOMEPERS + " " + x.SINTESI1.DES_NOMEPERS).Contains(ricerca.Nominativo.ToUpper()));
            }

            var availableEvaluation = tmpAvEval.ToList();

            List<RoleContainer> roleContainers = new List<RoleContainer>();

            if (availableEvaluation != null && availableEvaluation.Any())
            {
                foreach (var role in roles)
                {
                    RoleContainer roleContainer = new RoleContainer();
                    roleContainer.Role = role;
                    if (role.IsDelegate())
                    {
                        if (getDelegatedPers)
                        {
                            var tmp = role.DELEGATO.First().XR_VAL_DELEGATION_PERS.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now);
                            if (ricerca != null && ricerca.HasFilter)
                            {
                                if (!String.IsNullOrWhiteSpace(ricerca.Matricola))
                                    tmp = tmp.Where(x => ricerca.Matricola.Contains(x.SINTESI1.COD_MATLIBROMAT));

                                if (!String.IsNullOrWhiteSpace(ricerca.Nominativo))
                                    tmp = tmp.Where(x => x.SINTESI1.Nominativo().ToUpper().Contains(ricerca.Nominativo));
                            }

                            roleContainer.Evaluations.AddRange(tmp.Select(x => new EvaluationContainer()
                            {
                                Person = x.SINTESI1,
                                Evaluation = x.SINTESI1.XR_VAL_EVALUATION.Where(y => y.VALID_DTA_END == null || y.VALID_DTA_END > DateTime.Now).FirstOrDefault(y => y.ID_CAMPAIGN_SHEET == idCampaignSheet)
                            }));
                        }
                    }
                    else if ((ricerca == null || ricerca.ActorView != ValutazioniView.Gestione) && role.IsExternalEvaluator())
                    {
                        var tmp = role.EXT_ROLE_ASSIGNED.First();
                        roleContainer.Evaluations.Add(new EvaluationContainer()
                        {
                            Person = tmp.EXT_VALUED,
                            Evaluation = tmp.EXT_VALUED.XR_VAL_EVALUATION.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).FirstOrDefault(y => y.ID_CAMPAIGN_SHEET == idCampaignSheet)
                        });
                    }
                    else
                    {
                        List<int> delegated = new List<int>();
                        if (role.DELEGANTE.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Any())
                            delegated.AddRange(role.DELEGANTE.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).First().XR_VAL_DELEGATION_PERS.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Select(x => x.ID_PERSONA));

                        var tmpEval = availableEvaluation.Where(x => x.ID_EVALUATOR == role.ID_EVALUATOR);
                        if (isSubRole)
                            tmpEval = tmpEval.Where(x => x.IND_OPERCHECKED.HasValue && x.IND_OPERCHECKED.Value == 1);
                        if (!getDelegatedPers)
                            tmpEval = tmpEval.Where(x => !delegated.Contains(x.ID_PERSONA));
                        else if (!getNotDelegatedPers)
                            tmpEval = tmpEval.Where(x => delegated.Contains(x.ID_PERSONA));

                        if (!getExtVal)
                            tmpEval = tmpEval.Where(x => !x.SINTESI1.EXT_VALUED.Where(y => y.VALID_DTA_END == null || y.VALID_DTA_END > DateTime.Now).Any(y => y.ID_EVALUATOR == role.ID_EVALUATOR));
                        else if (!getNotExtVal)
                            tmpEval = tmpEval.Where(x => x.SINTESI1.EXT_VALUED.Where(y => y.VALID_DTA_END == null || y.VALID_DTA_END > DateTime.Now).Any(y => y.ID_EVALUATOR == role.ID_EVALUATOR));

                        roleContainer.Evaluations.AddRange(tmpEval.Select(x => new EvaluationContainer()
                        {
                            Person = x.SINTESI1,
                            Delegation = loader == ValutazioniLoader.MieiValutatori || loader == ValutazioniLoader.Report ? 
                                                    null 
                                                    : x.SINTESI1.XR_VAL_DELEGATION_PERS
                                                                .Where(y => y.XR_VAL_DELEGATION.ID_DELEGATOR == role.ID_EVALUATOR)
                                                                .Where(w => w.VALID_DTA_END == null || w.VALID_DTA_END > DateTime.Now)
                                                                .Select(z => z.XR_VAL_DELEGATION)
                                                                .Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now)
                                                                .Where(a=>a.DTA_START<=DateTime.Now && (a.DTA_END==null || a.DTA_END>DateTime.Now)).FirstOrDefault(),
                            Evaluation = loader == ValutazioniLoader.MieiValutatori ? null : x,
                            ExternalEvaluator = loader == ValutazioniLoader.MieiValutatori || loader == ValutazioniLoader.Report ? null : x.SINTESI1.EXT_VALUED.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).FirstOrDefault(y => y.ID_EVALUATOR == role.ID_EVALUATOR)
                        }));
                    }

                    roleContainers.Add(roleContainer);
                }
            }

            return roleContainers;
        }

        public static List<SINTESI1> GetPeopleForEvaluation(IncentiviEntities db, XR_VAL_CAMPAIGN_SHEET sheet, List<int> qualificheInt, List<string> elencoServizi)
        {
            IQueryable<SINTESI1> list = null;

            list = db.SINTESI1
                    .Where(x => x.DTA_FINE_CR > DateTime.Today);

            if (qualificheInt != null && qualificheInt.Count() > 0)
            {
                var qualFilters = db.XR_VAL_QUAL_FILTER.Where(x => qualificheInt.Contains(x.ID_QUAL_FILTER)).ToList();

                IEnumerable<string> categoryIncluded = qualFilters.Where(x => !String.IsNullOrWhiteSpace(x.QUAL_INCLUDED)).SelectMany(x => x.QUAL_INCLUDED.Split(',')).Distinct();
                IEnumerable<string> categoryExcluded = qualFilters.Where(x => !String.IsNullOrWhiteSpace(x.QUAL_EXCLUDED)).SelectMany(x => x.QUAL_EXCLUDED.Split(',')).Distinct().Where(y => !categoryIncluded.Contains(y));
                list = list.Where(s => s.ASSQUAL_HISTORY.Any(x => x.DTA_INIZIO <= sheet.DTA_OBSERVATION_END && sheet.DTA_OBSERVATION_INI <= x.DTA_FINE));

                if (categoryIncluded.Any())
                    list = list.Where(x => categoryIncluded.Any(a => x.COD_QUALIFICA.StartsWith(a)));

                if (categoryExcluded.Any())
                    list = list.Where(x => !categoryExcluded.Any(b => x.COD_QUALIFICA.StartsWith(b)));
            }

            if (elencoServizi != null && elencoServizi.Count() > 0)
                list = list.Where(s => s.XR_SERVIZIO_HISTORY.Any(x => x.DTA_INIZIO <= sheet.DTA_OBSERVATION_END && sheet.DTA_OBSERVATION_INI <= x.DTA_FINE && elencoServizi.Contains(x.COD_SERVIZIO)));

            List<SINTESI1> resultList = new List<SINTESI1>();
            resultList.AddRange(list);

            return resultList;
        }

        public static List<SINTESI1> GetAvailableEvaluation(IncentiviEntities db, XR_VAL_CAMPAIGN_SHEET sheet, RicercaValutazione ricerca = null, ValutazioniLoader loader = ValutazioniLoader.Completo)
        {
            IQueryable<SINTESI1> list = null;

            switch (loader)
            {
                case ValutazioniLoader.Minimo:
                    list = db.SINTESI1
                            .Where(x => x.DTA_FINE_CR > DateTime.Today);
                    break;
                case ValutazioniLoader.Completo:
                    list = db.SINTESI1
                            .Include("XR_VAL_DELEGATION_PERS")
                            .Include("XR_VAL_DELEGATION_PERS.XR_VAL_DELEGATION")
                            .Include("XR_VAL_EVALUATION")
                            .Include("XR_VAL_EVALUATION.XR_VAL_OPER_STATE")
                            .Include("EXT_VALUED")
                            .Where(x => x.DTA_FINE_CR > DateTime.Today);
                    break;
                case ValutazioniLoader.MieiValutatori:
                    list = db.SINTESI1
                        .Where(x => x.DTA_INIZIO_CR <= sheet.DTA_OBSERVATION_END && sheet.DTA_OBSERVATION_END <= x.DTA_FINE_CR && x.DTA_FINE_CR > DateTime.Today);
                    break;
                default:
                    break;
            }

            var qualFilters = sheet.XR_VAL_CAMPAIGN_SHEET_QUAL.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now);
            if (qualFilters != null && qualFilters.Count() > 0)
            {
                IEnumerable<string> categoryIncluded = qualFilters.Where(x => !String.IsNullOrWhiteSpace(x.XR_VAL_QUAL_FILTER.QUAL_INCLUDED)).SelectMany(x => x.XR_VAL_QUAL_FILTER.QUAL_INCLUDED.Split(',')).Distinct();
                IEnumerable<string> categoryExcluded = qualFilters.Where(x => !String.IsNullOrWhiteSpace(x.XR_VAL_QUAL_FILTER.QUAL_EXCLUDED)).SelectMany(x => x.XR_VAL_QUAL_FILTER.QUAL_EXCLUDED.Split(',')).Distinct().Where(y => !categoryIncluded.Contains(y));
                list = list.Where(s => s.ASSQUAL_HISTORY.Any(x => x.DTA_INIZIO <= sheet.DTA_OBSERVATION_END && sheet.DTA_OBSERVATION_END <= x.DTA_FINE && categoryIncluded.Any(a => x.COD_QUALIFICA.StartsWith(a)) && !categoryExcluded.Any(b => x.COD_QUALIFICA.StartsWith(b))));
            }
            var servFilters = sheet.XR_VAL_CAMPAIGN_SHEET_SER.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now);
            if (servFilters != null && servFilters.Count() > 0)
            {
                var servIncluded = servFilters.Select(x => x.COD_SERVIZIO);
                list = list.Where(s => s.XR_SERVIZIO_HISTORY.Any(x => x.DTA_INIZIO <= sheet.DTA_OBSERVATION_END && sheet.DTA_OBSERVATION_END <= x.DTA_FINE && servIncluded.Contains(x.COD_SERVIZIO)));
            }

            if (ricerca != null && ricerca.HasFilter)
            {
                if (!String.IsNullOrWhiteSpace(ricerca.Matricola))
                    list = list.Where(x => ricerca.Matricola.Contains(x.COD_MATLIBROMAT));

                if (!String.IsNullOrWhiteSpace(ricerca.Nominativo))
                    list = list.Where(x => (x.DES_COGNOMEPERS + " " + x.DES_NOMEPERS).ToUpper().Contains(ricerca.Nominativo));
            }

            List<SINTESI1> resultList = new List<SINTESI1>();
            resultList.AddRange(list);

            return resultList;
        }

        public static List<SINTESI1> GetMyAvailableEvaluation(XR_VAL_CAMPAIGN_SHEET sheet, List<SINTESI1> sheetList, string matricola, IncarichiContainer incarichi)
        {
            List<SINTESI1> list = sheetList;
            //oltre i vari filtri devo prendere solo quelli della mia struttura/servizio
            List<string> uorg = new List<string>();
            List<string> matr = new List<string>();
            GetMieiIncarichi(matricola, incarichi, ref uorg, ref matr);
            if (uorg != null && uorg.Count() > 0)
            {
                list = list.Where(x => x.COD_MATLIBROMAT != matricola && (uorg.Contains(x.COD_UNITAORG) || matr.Contains(x.COD_MATLIBROMAT))).ToList();
                //list = list.Where(i =>i.COD_MATLIBROMAT!=matricola && i.INCARLAV_HISTORY.Any(x => x.DTA_INIZIO <= sheet.DTA_OBSERVATION_END && sheet.DTA_OBSERVATION_INI <= sheet.DTA_OBSERVATION_END && uorg.Contains(x.UNITAORG.COD_UNITAORG)));
            }

            return list;
        }

        public static List<XR_VAL_EVAL_SHEET> GetSchedeValutazione()
        {
            List<XR_VAL_EVAL_SHEET> schede = new List<XR_VAL_EVAL_SHEET>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                schede = db.XR_VAL_EVAL_SHEET.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).ToList();
            }

            return schede;
        }

        public static IncarichiContainer GetIncarichiContainer()
        {
            IncarichiContainer incarichiCont = new IncarichiContainer();
            using (var db = new TalentiaEntities())
            {
                string todayStr = DateTime.Today.ToString("yyyyMMdd");
                incarichiCont.ListIncarichi.AddRange(db.XR_STR_TINCARICO.Where(x => String.Compare(x.data_inizio_validita, todayStr) < 0 && String.Compare(todayStr, x.data_fine_validita) < 0 && x.flag_resp == "1"));
                incarichiCont.NodiAlbero.AddRange(db.XR_STR_TALBERO);
                incarichiCont.ListSezioni.AddRange(db.XR_STR_TSEZIONE.Where(x => String.Compare(x.data_inizio_validita, todayStr) < 0 && String.Compare(todayStr, x.data_fine_validita) < 0));
            }
            using (var db = new IncentiviEntities())
            {
                incarichiCont.QualFilter = db.XR_VAL_QUAL_FILTER.ToList().Select(x => new ValutazioniQualFilter(x)).ToList();
            }
            return incarichiCont;
        }
        public static IncarichiContainer GetIncarichiContainer2(bool loadIncarichi = true, bool loadNodi = true, bool loadSezioni = true)
        {
            IncarichiContainer incarichiCont = new IncarichiContainer();
            using (var db = new TalentiaEntities())
            {
                string todayStr = DateTime.Today.ToString("yyyyMMdd");
                if (loadIncarichi)
                    //incarichiCont.ListIncarichi.AddRange(db.XR_STR_TINCARICO.Where(x => String.Compare(x.data_inizio_validita, todayStr) < 0 && String.Compare(todayStr, x.data_fine_validita) < 0 && x.flag_resp == "1"));
                    incarichiCont.ListIncarichi = db.Database.SqlQuery<myRaiDataTalentia.XR_STR_TINCARICO>("select * from XR_STR_TINCARICO where data_inizio_validita < '" + todayStr + "' and '" + todayStr + "' < data_fine_validita and flag_resp='1'").ToList();

                if (loadNodi)
                    incarichiCont.NodiAlbero.AddRange(db.XR_STR_TALBERO);

                if (loadSezioni)
                    //incarichiCont.ListSezioni.AddRange(db.XR_STR_TSEZIONE.Where(x => String.Compare(x.data_inizio_validita, todayStr) < 0 && String.Compare(todayStr, x.data_fine_validita) < 0));
                    incarichiCont.ListSezioni = db.Database.SqlQuery<myRaiDataTalentia.XR_STR_TSEZIONE>("select *, 1 as max_responsabili from xr_str_tsezione where data_inizio_validita < '" + todayStr + "' and '" + todayStr + "' < data_fine_validita").ToList();
            }
            using (var db = new IncentiviEntities())
            {
                incarichiCont.QualFilter = db.XR_VAL_QUAL_FILTER.ToList().Select(x => new ValutazioniQualFilter(x)).ToList();
            }
            return incarichiCont;
        }

        public static Func<SINTESI1, bool> FilterCanEvaluateByQual(IncarichiContainer container, string qual)
        {
            List<string> catIncluded = new List<string>();
            List<string> catExcluded = new List<string>();
            Func<SINTESI1, bool> result = x => true;

            int level = GetLevelByQual(container, qual);

            catIncluded.AddRange(container.QualFilter.Where(x => x.Level >= level).SelectMany(x => x.CatIncluded).Distinct());
            catExcluded.AddRange(container.QualFilter.Where(x => x.Level >= level).SelectMany(x => x.CatExcluded).Distinct().Where(y => !catIncluded.Contains(y)));

            return x => (!catIncluded.Any() || catIncluded.Any(y => x.COD_QUALIFICA.StartsWith(y)))
                     && (!catExcluded.Any() || !catExcluded.Any(w => x.COD_QUALIFICA.StartsWith(w)));

        }

        private static int GetLevelByQual(IncarichiContainer container, string qual)
        {
            int level = 1000;

            var filterElement = container.QualFilter.FirstOrDefault(x => (x.CatIncluded == null || !x.CatIncluded.Any() || x.CatIncluded.Any(y => qual.StartsWith(y)))
                                        && (x.CatExcluded == null || !x.CatExcluded.Any() || !x.CatExcluded.Any(w => qual.StartsWith(w))));

            if (filterElement != null)
                level = filterElement.Level.Value;
            return level;
        }

        public static ValutatoreEsternoContainer GetEvaluatorFromPerson(string matr, bool loadExt = false)
        {
            IncentiviEntities db = new IncentiviEntities();

            if (!loadExt)
            {
                var filterValidEvaluation = ValutazioniExtension.ExprFuncValidEvaluation();
                var tmp = db.XR_VAL_EVALUATION
                            .Where(filterValidEvaluation)
                            .Where(x => x.SINTESI1.COD_MATLIBROMAT == matr
                                        && ((x.XR_VAL_CAMPAIGN_SHEET.EMPLOYEE_VIEW != null && x.XR_VAL_CAMPAIGN_SHEET.EMPLOYEE_VIEW.Value)
                                        || x.XR_VAL_CAMPAIGN_SHEET.EXT_EVALUATOR == (int)ValutazioniExtEvalPermission.Autorizzato
                                        || x.XR_VAL_CAMPAIGN_SHEET.EXT_EVALUATOR == (int)ValutazioniExtEvalPermission.Obbligatorio
                                        || (x.XR_VAL_CAMPAIGN_SHEET.AUTOEVAL != null && x.XR_VAL_CAMPAIGN_SHEET.AUTOEVAL != (int)ValutazioniAuto.No)));
                if (tmp != null && tmp.Any())
                {
                    return new ValutatoreEsternoContainer();
                }
            }
            else
            {
                var sheet = GetSheetContainers("", new RicercaValutazione() { HasFilter = true, Matricola = CommonHelper.GetCurrentUserMatricola() }, false, false, ValutazioniLoader.MieiValutatori);
                var evaluator = sheet.Where(x => x.Roles.Any(y => y.Evaluations.Any(z => z.Person.COD_MATLIBROMAT == matr))).SelectMany(x => x.Roles.Where(y => y.Evaluations.Any(z => z.Person.COD_MATLIBROMAT == matr)));
                if (evaluator != null && evaluator.Any(x => !x.Role.IsDelegate()))
                {
                    ValutatoreEsternoContainer valExt = new ValutatoreEsternoContainer();
                    foreach (var item in evaluator.Where(x => !x.Role.IsDelegate()))
                    {
                        var tmp = new ValutatoreEsterno()
                        {
                            IdValutatore = item.Role.ID_EVALUATOR,
                            IdValutazione = item.Evaluations.FirstOrDefault(x => x.Person.COD_MATLIBROMAT == matr).Evaluation.ID_EVALUATION,
                            IdPersonaValued = item.Evaluations.FirstOrDefault(x => x.Person.COD_MATLIBROMAT == matr).Person.ID_PERSONA,
                            ActualEvaluator = item,
                            AutoVal = item.Role.XR_VAL_CAMPAIGN_SHEET.AUTOEVAL.GetValueOrDefault()
                        };
                        if (loadExt)
                        {
                            var req = db.XR_VAL_EVALUATOR_EXT.FirstOrDefault(x => x.ID_EVALUATOR == item.Role.ID_EVALUATOR && x.EXT_VALUED.COD_MATLIBROMAT == matr);
                            if (req != null)
                            {
                                tmp.ExternalEvaluator = req;
                                tmp.IdPersonaSel = req.ID_PERSONA_CHOOSEN;
                                tmp.NoteRequest = req.NOT_REQUEST;
                                tmp.Approved = req.IND_APPROVED;
                                tmp.NoteApproved = req.NOT_APPROVED;
                            }
                        }
                        valExt.Valutatori.Add(tmp);
                    }

                    return valExt;
                }
                return null;
            }
            return null;
        }
        public static ValutatoreEsternoContainer GetRequestExtEvaluator(string matr, int idValExt = 0)
        {
            ValutatoreEsternoContainer valExt = new ValutatoreEsternoContainer();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                var list = db.XR_VAL_EVALUATOR_EXT
                    .Include("XR_VAL_EVALUATOR")
                    .Include("XR_VAL_EVALUATOR.SINTESI1")
                    .Include("XR_VAL_EVALUATOR.XR_VAL_CAMPAIGN_SHEET")
                    .Include("XR_VAL_EVALUATOR.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN")
                    .Include("EXT_VALUED")
                    .Include("EXT_EVALUATOR")
                    .Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now);

                if (idValExt == 0)
                    list = list.Where(x => x.XR_VAL_EVALUATOR.SINTESI1.COD_MATLIBROMAT == matr);
                else
                    list = list.Where(x => x.ID_EVALUATOR_EXT == idValExt);

                foreach (var ext in list)
                {
                    ValutatoreEsterno val = new ValutatoreEsterno()
                    {
                        IdValutatore = ext.ID_EVALUATOR,
                        IdExtVal = ext.ID_EVALUATOR_EXT,
                        IdPersonaValued = ext.ID_PERSONA_VALUED,
                        NoteRequest = ext.NOT_REQUEST,
                        Approved = ext.IND_APPROVED,
                        NoteApproved = ext.NOT_APPROVED,
                        IdPersonaSel = ext.ID_PERSONA_CHOOSEN,
                        ExternalEvaluator = ext,
                        AutoVal = ext.XR_VAL_EVALUATOR.XR_VAL_CAMPAIGN_SHEET.AUTOEVAL.GetValueOrDefault()
                    };
                    valExt.Valutatori.Add(val);
                }
            }

            return valExt;
        }

        public static Expression<Func<SINTESI1, bool>> FilterDifferentEvaluator(IncentiviEntities db, string matr)
        {
            List<string> catIncluded = new List<string>();
            List<string> catExcluded = new List<string>();

            var listQualFilter = db.XR_VAL_QUAL_FILTER.ToList()
                .Select(x => new
                {
                    CatIncluded = !String.IsNullOrWhiteSpace(x.QUAL_INCLUDED) ? x.QUAL_INCLUDED.Split(',').ToList() : new List<string>(),
                    CatExcluded = !String.IsNullOrWhiteSpace(x.QUAL_EXCLUDED) ? x.QUAL_EXCLUDED.Split(',').ToList() : new List<string>(),
                    Level = x.LEVEL
                });

            string matrQual = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matr).COD_QUALIFICA;
            var filterElement = listQualFilter.FirstOrDefault(x => (x.CatIncluded == null || !x.CatIncluded.Any() || x.CatIncluded.Any(y => matrQual.StartsWith(y)))
                                                            && (x.CatExcluded == null || !x.CatExcluded.Any() || !x.CatExcluded.Any(w => matrQual.StartsWith(w))));

            int? matrLevel = filterElement != null ? filterElement.Level : null;

            catIncluded.AddRange(listQualFilter.Where(x => x.Level <= matrLevel).SelectMany(x => x.CatIncluded).Distinct());
            catExcluded.AddRange(listQualFilter.Where(x => x.Level <= matrLevel).SelectMany(x => x.CatExcluded).Distinct().Where(y => !catIncluded.Contains(y)));

            return x => (!catIncluded.Any() || catIncluded.Any(y => x.COD_QUALIFICA.StartsWith(y)))
                     && (!catExcluded.Any() || !catExcluded.Any(w => x.COD_QUALIFICA.StartsWith(w)));
        }

        #region GestioneDeleghe
        public static List<XR_VAL_DELEGATION> GetElencoDeleghe(string matricola)
        {
            List<XR_VAL_DELEGATION> deleghe = new List<XR_VAL_DELEGATION>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                var activeRoles = GetActiveRoles(db, matricola, false);
                foreach (var role in activeRoles.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Where(x => x.IsDelegator()))
                {
                    deleghe.AddRange(role.DELEGANTE.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Where(x => x.DTA_START < DateTime.Now && (x.DTA_END == null || x.DTA_END > DateTime.Now)));
                }
            }

            return deleghe;
        }
        public static List<XR_VAL_DELEGATION> GetElencoDelegheRicevute(string matricola)
        {
            List<XR_VAL_DELEGATION> deleghe = new List<XR_VAL_DELEGATION>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                var activeRoles = GetActiveRoles(db, matricola, false);
                foreach (var role in activeRoles.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Where(x => x.IsDelegate()))
                {
                    deleghe.AddRange(role.DELEGATO.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Where(x => x.DTA_START < DateTime.Now && (x.DTA_END == null || x.DTA_END > DateTime.Now)));
                }
            }

            return deleghe;
        }
        public static List<XR_VAL_EVALUATOR> GetRuoliDelegabili(IncentiviEntities db, string matricola)
        {
            List<XR_VAL_EVALUATOR> ruoliDelegabili = new List<XR_VAL_EVALUATOR>();

            var activeRoles = GetActiveRoles(db, matricola, false);
            ruoliDelegabili.AddRange(activeRoles.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Where(x => !x.IsDelegate() && x.XR_VAL_CAMPAIGN_SHEET.DELEGATION));

            return ruoliDelegabili;
        }
        public static List<XR_VAL_EVALUATION> GetValutazioniDelegabili(IncentiviEntities db, XR_VAL_EVALUATOR eval)
        {
            var tmp = eval.XR_VAL_EVALUATION
                            .Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)
                            .Where(x => x.XR_VAL_OPER_STATE == null
                                        || !x.XR_VAL_OPER_STATE.Where(y => y.VALID_DTA_END == null || y.VALID_DTA_END > DateTime.Now).Any()
                                        || x.XR_VAL_OPER_STATE.Where(y => y.VALID_DTA_END == null || y.VALID_DTA_END > DateTime.Now).Max(y => y.ID_STATE) < (int)ValutazioniState.PresaVisione)
                                            .ToList();

            return tmp;
        }
        public static DelegaModel GetDelega(int idDelega, string matricola)
        {
            DelegaModel delega = null;
            using (IncentiviEntities db = new IncentiviEntities())
            {
                if (idDelega == 0)
                {
                    delega = new DelegaModel();
                }
                else
                {
                    var dbDelega = db.XR_VAL_DELEGATION
                        .Include("DELEGANTE")
                        .Include("DELEGANTE.XR_VAL_CAMPAIGN_SHEET")
                        .Include("DELEGANTE.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN")
                        .Include("DELEGATO")
                        .Include("DELEGATO.XR_VAL_CAMPAIGN_SHEET")
                        .Include("DELEGATO.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN")
                        .FirstOrDefault(x => x.ID_DELEGATION == idDelega);
                    if (dbDelega != null)
                    {
                        delega = new DelegaModel(dbDelega);
                        delega.ValutazioniDelegabili.AddRange(GetValutazioniDelegabili(db, delega.Delegante));
                    }
                }

                var tmp = GetRuoliDelegabili(db, matricola);
                delega.RuoliDelegabili.Add(new System.Web.Mvc.SelectListItem()
                {
                    Value = "0",
                    Text = "Seleziona il ruolo da delegare"
                });
                delega.RuoliDelegabili.AddRange(tmp.Select(x => new System.Web.Mvc.SelectListItem()
                {
                    Value = x.ID_EVALUATOR.ToString(),
                    Text = x.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN.NAME + " - " + x.XR_VAL_CAMPAIGN_SHEET.DESCRIPTION
                }));
            }

            return delega;
        }
        #endregion

        #region GestioneSchede
        public static List<Sheet> GetSchede()
        {
            List<Sheet> schede = new List<Sheet>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                foreach (var item in db.XR_VAL_EVAL_SHEET.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now))
                {
                    schede.Add(new Sheet(item));
                }
            }

            return schede;
        }
        public static Sheet GetScheda(int idSheet)
        {
            Sheet sheet = null;
            if (idSheet == 0)
            {
                sheet = new Sheet();
            }
            else
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var dbSheet = db.XR_VAL_EVAL_SHEET
                                    .Include("XR_VAL_EVAL_SHEET_QST")
                                    .Include("XR_VAL_EVAL_SHEET_QST.XR_VAL_QUESTION")
                                    .Include("XR_VAL_EVAL_SHEET_QST.XR_VAL_QUESTION.XR_VAL_QUESTION_GROUP")
                                    .Include("XR_VAL_EVAL_SHEET_QST.XR_VAL_QUESTION.XR_VAL_QUESTION_ANSWER")
                                    .FirstOrDefault(x => x.ID_SHEET == idSheet);
                    if (dbSheet != null)
                    {
                        sheet = new Sheet(dbSheet, true);
                    }
                }
            }

            return sheet;
        }
        #endregion

        #region GestioneCampagne
        public static List<Campagna> GetCampagne(string matricola)
        {
            List<Campagna> campagne = new List<Campagna>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                foreach (var item in db.XR_VAL_CAMPAIGN.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now))
                {
                    campagne.Add(new Campagna(item));
                }

            }

            return campagne;
        }
        public static Campagna GetCampagna(int idCampagna)
        {
            Campagna campagna = null;
            if (idCampagna == 0)
            {
                campagna = new Campagna();
            }
            else
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var dbCamp = db.XR_VAL_CAMPAIGN.FirstOrDefault(x => x.ID_CAMPAIGN == idCampagna);
                    if (dbCamp != null)
                    {
                        campagna = new Campagna(dbCamp);
                    }
                }
            }

            return campagna;
        }
        #endregion

        #region GestioneSchedeCampagna
        private static double CalcoloPercentuale(int tot, int num)
        {
            if (num == 0)
                return 0;
            else
                return Math.Round((double)((double)num * 100 / (double)tot), 2);
        }
        public static ReportCampagnaScheda GetReportScheda(int idCampagnaScheda)
        {
            ReportCampagnaScheda report = new ReportCampagnaScheda();

            report.CampagnaScheda = GetCampagnaScheda(0, idCampagnaScheda, true);
            report.Scheda = GetScheda(report.CampagnaScheda.Id_Sheet);

            var db = new IncentiviEntities();
            List<XR_VAL_EVALUATOR> activeRoles = GetActiveRoles(db, null, true, ValutazioniLoader.Report, new RicercaValutazione() { CampagnaScheda = idCampagnaScheda });

            var evaluator = activeRoles.Select(x => x.ID_EVALUATOR).ToList();

            var evaluations = db.XR_VAL_EVALUATION
                                .Include("XR_VAL_OPER_STATE")
                                .Include("SINTESI1")
                                .Where(x => x.ID_CAMPAIGN_SHEET == idCampagnaScheda && evaluator.Contains(x.ID_EVALUATOR))
                                .Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).ToList();
            var idEvals = evaluations.Select(x => x.ID_EVALUATION);

            var ratings = db.XR_VAL_EVAL_RATING.Where(x => idEvals.Contains(x.ID_EVALUATION)).Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).ToList();
            var qstType = db.XR_VAL_QUESTION_TYPE.FirstOrDefault(x => x.NAME == "Intero");

            var weights = report.Scheda.Groups.SelectMany(x => x.Questions.Where(y => y.Weight > 0));

            report.Valutazioni = new List<Valutazione>();
            foreach (var eval in evaluations)
            {
                Valutazione valutazione = new Valutazione();
                valutazione.IdValutazione = eval.ID_EVALUATION;
                valutazione.Rating.AddRange(ratings.Where(x => x.ID_EVALUATION == eval.ID_EVALUATION).Where(x => x.XR_VAL_EVAL_RATING_OWNER.NAME == "Superiore"));
                valutazione.Persona = eval.SINTESI1;

                valutazione.Valutatore = activeRoles.FirstOrDefault(x => x.ID_EVALUATOR == eval.ID_EVALUATOR);

                double sommaValPerPeso = 0;
                double sommaPeso = 0;
                foreach (var item in weights)
                {
                    var tmpRating = valutazione.Rating.FirstOrDefault(x => x.ID_QUESTION == item.IdEvalQst);
                    int valueQst = tmpRating != null ? tmpRating.VALUE_INT.GetValueOrDefault() : 0;
                    sommaValPerPeso += valueQst * item.Weight;
                    sommaPeso += item.Weight;
                }

                valutazione.Media = sommaValPerPeso / sommaPeso;

                int valState = GetMaxEvalState(eval.XR_VAL_OPER_STATE);
                valutazione.Stato = valState;
                report.Valutazioni.Add(valutazione);
            }

            report.ElencoDati = new List<DatoDomanda>();
            foreach (var grp in report.Scheda.Groups)
            {
                foreach (Question qst in grp.Questions)
                {
                    var tmpRisp = ratings.Where(x => x.ID_QUESTION == qst.IdEvalQst);
                    int totRisp = tmpRisp.Count();

                    DatoDomanda d = new DatoDomanda();
                    d.IdDomanda = qst.Id;
                    d.PieItems = new List<PieItem>();

                    if (qst.Type == qstType.ID_QST_TYPE)
                    {
                        double somma = tmpRisp.Sum(x => x.VALUE_INT ?? 0);
                        if (totRisp > 0)
                            d.ValoreMedio = somma / totRisp;
                    }


                    foreach (var answ in qst.Answers)
                    {
                        int tmpCount = tmpRisp.Count(x => x.VALUE_INT == answ.ValueInt);
                        d.PieItems.Add(new PieItem()
                        {
                            label = String.Format("{0} - {1}%", (!String.IsNullOrWhiteSpace(answ.Description) ? answ.Description : answ.ValueInt.Value.ToString()), CalcoloPercentuale(totRisp, tmpCount)),
                            labelObj = answ.ValueInt,
                            data = new List<List<int>>() { new List<int>() { 1, tmpCount } }
                        });
                    }
                    d.JsonPieItems = new JavaScriptSerializer().Serialize(d.PieItems);
                    report.ElencoDati.Add(d);
                }
            }

            return report;
        }

        public static ReportCampagnaScheda GetReportDomandaRisposta(int idCampagnaScheda, int idQst, object answer)
        {
            ReportCampagnaScheda report = new ReportCampagnaScheda();
            var db = new IncentiviEntities();

            var question = db.XR_VAL_EVAL_SHEET_QST
                            .FirstOrDefault(x => x.ID_QUESTION == idQst);
            report.Domanda = question.XR_VAL_QUESTION;

            List<XR_VAL_EVALUATOR> activeRoles = GetActiveRoles(db, null, true, ValutazioniLoader.Report, new RicercaValutazione() { CampagnaScheda = idCampagnaScheda });

            var evaluator = activeRoles.Select(x => x.ID_EVALUATOR).ToList();
            var tmp = db.XR_VAL_EVALUATION
                                .Include("XR_VAL_OPER_STATE")
                                .Include("SINTESI1")
                                .Where(x => x.ID_CAMPAIGN_SHEET == idCampagnaScheda && evaluator.Contains(x.ID_EVALUATOR));

            if (question.XR_VAL_QUESTION.XR_VAL_QUESTION_TYPE.NAME == "Intero"
                || question.XR_VAL_QUESTION.XR_VAL_QUESTION_TYPE.NAME == "StringaIntero")
            {
                int val = Convert.ToInt32(answer);
                tmp = tmp.Where(x => x.XR_VAL_EVAL_RATING.Any(y => y.ID_QUESTION == question.ID_SHEET_QST && y.VALUE_INT == val));
                report.Risposta = report.Domanda.XR_VAL_QUESTION_ANSWER.FirstOrDefault(x => x.VALUE_INT == val);
            }
            //else if (answer is decimal)
            //{
            //    decimal val = Convert.ToDecimal(answer);
            //    tmp = tmp.Where(x => x.XR_VAL_EVAL_RATING.Any(y => y.ID_QUESTION == idQst && y.VALUE_DECIMAL == val));
            //}
            //else if (answer is bool)
            //{
            //    bool val = Convert.ToBoolean(answer);
            //    tmp = tmp.Where(x => x.XR_VAL_EVAL_RATING.Any(y => y.ID_QUESTION == idQst && y.VALUE_BOOL == val));
            //}
            else
            {
                tmp = tmp.Where(x => x.XR_VAL_EVAL_RATING.Any(y => y.ID_QUESTION == question.ID_SHEET_QST && y.VALUE_STR == (string)answer));
                report.Risposta = report.Domanda.XR_VAL_QUESTION_ANSWER.FirstOrDefault(x => x.VALUE_STR == (string)answer);
            }

            var evaluations = tmp.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).ToList().Where(x => x.XR_VAL_OPER_STATE.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Max(z => z.ID_STATE) > (int)ValutazioniState.Bozza);
            var idEvals = evaluations.Select(x => x.ID_EVALUATION);

            report.Valutazioni = new List<Valutazione>();
            foreach (var eval in evaluations)
            {
                Valutazione valutazione = new Valutazione();
                valutazione.IdValutazione = eval.ID_EVALUATION;
                valutazione.Persona = eval.SINTESI1;

                valutazione.Valutatore = activeRoles.FirstOrDefault(x => x.ID_EVALUATOR == eval.ID_EVALUATOR);
                report.Valutazioni.Add(valutazione);
            }

            return report;
        }


        public static CampagnaScheda GetCampagnaScheda(int idCampagna, int idCampagnaScheda, bool loadForReport = false)
        {
            CampagnaScheda scheda = null;
            if (idCampagnaScheda == 0)
            {
                scheda = new CampagnaScheda(idCampagna);
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var campagna = db.XR_VAL_CAMPAIGN.FirstOrDefault(x => x.ID_CAMPAIGN == idCampagna);
                    scheda.Campagna_Name = campagna.NAME;
                    scheda.Campagna_Des = campagna.DESCRIPTION;
                    scheda.TipologiaCampagna = campagna.COD_TIPOLOGIA;
                }

            }
            else
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    XR_VAL_CAMPAIGN_SHEET dbScheda = null;
                    if (!loadForReport)
                        dbScheda = db.XR_VAL_CAMPAIGN_SHEET.FirstOrDefault(x => x.ID_CAMPAIGN_SHEET == idCampagnaScheda);
                    else
                        dbScheda = db.XR_VAL_CAMPAIGN_SHEET
                                    .Include("XR_VAL_CAMPAIGN_SHEET_SER")
                                    .Include("XR_VAL_CAMPAIGN_SHEET_SER.XR_TB_SERVIZIO")
                                    .FirstOrDefault(x => x.ID_CAMPAIGN_SHEET == idCampagnaScheda);

                    if (dbScheda != null)
                    {
                        scheda = new CampagnaScheda(dbScheda, loadForReport);
                    }
                }
            }

            return scheda;
        }

        public static List<CampagnaScheda> GetCampagnaSchede(int idCampagna)
        {
            List<CampagnaScheda> schede = new List<CampagnaScheda>();
            using (IncentiviEntities db = new IncentiviEntities())
            {
                var dbSchede = db.XR_VAL_CAMPAIGN_SHEET.Where(x => x.ID_CAMPAIGN == idCampagna && x.VALID_DTA_INI <= DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END >= DateTime.Now));
                if (dbSchede != null && dbSchede.Count() > 0)
                {
                    foreach (var dbScheda in dbSchede)
                    {
                        schede.Add(new CampagnaScheda(dbScheda));
                    }
                }
            }
            return schede;
        }

        private static List<string> GetMieiIncarichiSub(IncarichiContainer incarichiCont, int idSez, ref List<string> matr)
        {
            List<string> uorg = new List<string>();

            var sez = incarichiCont.ListSezioni.FirstOrDefault(x => x.id == idSez);
            if (sez != null)
            {
                uorg.Add(sez.codice_visibile);
                var subordinate = incarichiCont.NodiAlbero.Where(x => x.subordinato_a == idSez);
                foreach (var subordinata in subordinate)
                {
                    var subInc = incarichiCont.ListIncarichi.Where(x => x.id_sezione == subordinata.id);
                    if (subInc.Count() == 0)
                        uorg.AddRange(GetMieiIncarichiSub(incarichiCont, subordinata.id, ref matr));
                    else
                        matr.AddRange(subInc.Select(x => x.matricola));
                }
            }

            return uorg;
        }

        public static void GetMieiIncarichi(string matricola, IncarichiContainer incarichiCont, ref List<string> uorg, ref List<string> matr)
        {
            if (incarichiCont == null)
                incarichiCont = GetIncarichiContainer();

            uorg = new List<string>();
            matr = new List<string>();

            var matrIncarichi = incarichiCont.ListIncarichi.Where(x => x.matricola == matricola);
            foreach (var incarico in matrIncarichi)
            {
                uorg.AddRange(GetMieiIncarichiSub(incarichiCont, incarico.id_sezione, ref matr));
            }
        }

        public static List<string> GetResponsabili(IncarichiContainer incarichiCont, List<string> strutture, List<string> servizi, bool filterSezLen = true)
        {
            List<string> matricole = new List<string>();

            var db = new TalentiaEntities();
            string todayStr = DateTime.Today.ToString("yyyyMMdd");
            var r_incarichi = incarichiCont.ListIncarichi;
            var r_albero = incarichiCont.NodiAlbero;

            int today = DateTime.Today.Year * 10000 + DateTime.Today.Month * 100 + DateTime.Today.Day;

            var sezioni = incarichiCont.ListSezioni.Where(x => !filterSezLen || x.codice_visibile.Length == 9);

            bool anyStrutt = strutture.Any();
            bool anyServ = servizi.Any();

            if (anyStrutt)
                sezioni = sezioni.Where(x => strutture.Contains(x.codice_visibile));

            if (anyServ)
                sezioni = sezioni.Where(x => servizi.Contains(x.servizio));

            List<int> intSezioni = new List<int>();
            intSezioni.AddRange(sezioni.Select(x => x.id));

            var tmp = sezioni.Select(x => new SezResp()
            {
                Sezione = x.id,
                Responsabili = r_incarichi.Where(z => z.id_sezione == x.id && String.Compare(x.data_inizio_validita, todayStr) < 0 && String.Compare(todayStr, x.data_fine_validita) < 0 && z.flag_resp == "1").ToList()
            });

            foreach (var item in tmp.Where(x => x.Responsabili == null || x.Responsabili.Count() == 0))
            {
                int idSez = item.Sezione;
                myRaiDataTalentia.XR_STR_TALBERO albero = null;
                do
                {
                    albero = r_albero.FirstOrDefault(x => x.id == idSez);
                    item.Responsabili.AddRange(r_incarichi.Where(z => z.id_sezione == albero.subordinato_a && Convert.ToInt32(z.data_inizio_validita) <= today && today < Convert.ToInt32(z.data_fine_validita) && z.flag_resp == "1"));
                    idSez = albero.subordinato_a;
                } while (albero != null && idSez != 1 && item.Responsabili.Count() == 0);
            }

            if (tmp.Count() > 0)
                matricole.AddRange(tmp.SelectMany(x => x.Responsabili).Select(y => y.matricola).Distinct());

            if (matricole.Any())
            {
                var dbInc = new IncentiviEntities();
                matricole = dbInc.SINTESI1.Where(x => matricole.Contains(x.COD_MATLIBROMAT) && x.DTA_FINE_CR > DateTime.Today).Select(x => x.COD_MATLIBROMAT).ToList();
            }

            return matricole;
        }


        #endregion

        #region GestioneSelectList
        public static List<ListItem> GetSelectDeleghe()
        {
            List<ListItem> lista = new List<ListItem>();

            lista.Add(new ListItem()
            {
                Value = "0",
                Text = "Seleziona il tipo di gestione"
            });
            lista.Add(new ListItem()
            {
                Value = "1",
                Text = "Con delega"
            });
            lista.Add(new ListItem()
            {
                Value = "2",
                Text = "Senza delega"
            });

            return lista;
        }
        public static List<ListItem> GetSelectExtVal()
        {
            List<ListItem> lista = new List<ListItem>();

            lista.Add(new ListItem()
            {
                Value = "0",
                Text = "Seleziona il tipo di gestione"
            });
            lista.Add(new ListItem()
            {
                Value = "1",
                Text = "Con valutatore esterno"
            });
            lista.Add(new ListItem()
            {
                Value = "2",
                Text = "Senza valutatore esterno"
            });

            return lista;
        }
        #endregion

        #region CaricamentoDati
        public static void CaricamentoDati(string codUser, string codTermid)
        {
            Caricamento_STATE(codUser, codTermid);
            Caricamento_RATING_OWNER(codUser, codTermid);
            Caricamento_QUESTION_TYPE(codUser, codTermid);
            Caricamento_QUESTION_DISPLAY(codUser, codTermid);
            Caricamento_QUESTION_GROUP(codUser, codTermid);
            Caricamento_QUESTION(codUser, codTermid);
            Caricamento_SHEET(codUser, codTermid);
        }
        private static void Caricamento_SHEET_String(string codUser, string codTermid)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                XR_VAL_EVAL_SHEET sheet = db.XR_VAL_EVAL_SHEET.FirstOrDefault(x => x.NAME == "Scheda valutazione prestazione Quadri A");
                int counter = 0;
                foreach (var qst in db.XR_VAL_QUESTION.Where(x => x.XR_VAL_QUESTION_TYPE.NAME == "Stringa").OrderBy(x => x.TMS_TIMESTAMP))
                {
                    counter++;
                    sheet.XR_VAL_EVAL_SHEET_QST.Add(new XR_VAL_EVAL_SHEET_QST()
                    {
                        ID_SHEET_QST = db.XR_VAL_EVAL_SHEET_QST.GeneraPrimaryKey(),
                        ID_QUESTION = qst.ID_QUESTION,
                        ID_SHEET = sheet.ID_SHEET,
                        VALID_DTA_INI = DateTime.Today,
                        ORDER = counter,
                        WEIGHT = 0,
                        COD_USER = codUser,
                        COD_TERMID = codTermid,
                        TMS_TIMESTAMP = DateTime.Now
                    });
                }

                db.SaveChanges();
            }
        }
        private static void Caricamento_SHEET(string codUser, string codTermid)
        {
            int[] weights = { 0, 10, 4, 13, 8, 8, 9, 9, 15, 9, 9, 6 };

            using (IncentiviEntities db = new IncentiviEntities())
            {
                XR_VAL_EVAL_SHEET sheet = new XR_VAL_EVAL_SHEET();
                sheet.ID_SHEET = db.XR_VAL_EVAL_SHEET.GeneraPrimaryKey();
                sheet.NAME = "Scheda valutazione prestazione Quadri A";
                sheet.DESCRIPTION = "Scheda utile alla valutazione della prestazione dei Quadri A";
                sheet.VALID_DTA_INI = DateTime.Today;
                sheet.COD_USER = codUser;
                sheet.COD_TERMID = codTermid;
                sheet.TMS_TIMESTAMP = DateTime.Now;

                int counter = 0;
                foreach (var qst in db.XR_VAL_QUESTION.OrderBy(x => x.TMS_TIMESTAMP))
                {
                    counter++;
                    sheet.XR_VAL_EVAL_SHEET_QST.Add(new XR_VAL_EVAL_SHEET_QST()
                    {
                        ID_SHEET_QST = db.XR_VAL_EVAL_SHEET_QST.GeneraPrimaryKey(),
                        ID_QUESTION = qst.ID_QUESTION,
                        ID_SHEET = sheet.ID_SHEET,
                        VALID_DTA_INI = DateTime.Today,
                        ORDER = counter,
                        WEIGHT = counter < weights.Length ? weights[counter] : 0,
                        COD_USER = codUser,
                        COD_TERMID = codTermid,
                        TMS_TIMESTAMP = DateTime.Now
                    });
                }

                db.XR_VAL_EVAL_SHEET.Add(sheet);

                db.SaveChanges();
            }
        }
        private static void Caricamento_QUESTION_da1a5(IncentiviEntities db, int idQst, string codUser, string codTermid)
        {
            for (int i = 1; i <= 5; i++)
            {
                db.XR_VAL_QUESTION_ANSWER.Add(new XR_VAL_QUESTION_ANSWER()
                {
                    ID_QST_ANSWER = db.XR_VAL_QUESTION_ANSWER.GeneraPrimaryKey(),
                    ID_QUESTION = idQst,
                    DESCRIPTION = i.ToString(),
                    VALUE_INT = i,
                    NUM_ORDER = i,
                    VALID_DTA_INI = DateTime.Now,
                    VALID_DTA_END = null,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
            }
        }
        private static void Caricamento_QUESTION_da1a3(IncentiviEntities db, int idQst, string codUser, string codTermid)
        {
            db.XR_VAL_QUESTION_ANSWER.Add(new XR_VAL_QUESTION_ANSWER()
            {
                ID_QST_ANSWER = db.XR_VAL_QUESTION_ANSWER.GeneraPrimaryKey(),
                ID_QUESTION = idQst,
                DESCRIPTION = "Insufficiente",
                VALUE_INT = null,
                VALUE_STR = "Insufficiente",
                NUM_ORDER = 1,
                VALID_DTA_INI = DateTime.Now,
                VALID_DTA_END = null,
                COD_USER = codUser,
                COD_TERMID = codTermid,
                TMS_TIMESTAMP = DateTime.Now
            });
            db.XR_VAL_QUESTION_ANSWER.Add(new XR_VAL_QUESTION_ANSWER()
            {
                ID_QST_ANSWER = db.XR_VAL_QUESTION_ANSWER.GeneraPrimaryKey(),
                ID_QUESTION = idQst,
                DESCRIPTION = "Sufficiente",
                VALUE_INT = null,
                VALUE_STR = "Sufficiente",
                NUM_ORDER = 2,
                VALID_DTA_INI = DateTime.Now,
                VALID_DTA_END = null,
                COD_USER = codUser,
                COD_TERMID = codTermid,
                TMS_TIMESTAMP = DateTime.Now
            });
            db.XR_VAL_QUESTION_ANSWER.Add(new XR_VAL_QUESTION_ANSWER()
            {
                ID_QST_ANSWER = db.XR_VAL_QUESTION_ANSWER.GeneraPrimaryKey(),
                ID_QUESTION = idQst,
                DESCRIPTION = "Eccellente",
                VALUE_STR = "Eccellente",
                NUM_ORDER = 3,
                VALID_DTA_INI = DateTime.Now,
                VALID_DTA_END = null,
                COD_USER = codUser,
                COD_TERMID = codTermid,
                TMS_TIMESTAMP = DateTime.Now
            });
        }
        private static void Caricamento_QUESTION_da1a2(IncentiviEntities db, int idQst, string codUser, string codTermid)
        {
            db.XR_VAL_QUESTION_ANSWER.Add(new XR_VAL_QUESTION_ANSWER()
            {
                ID_QST_ANSWER = db.XR_VAL_QUESTION_ANSWER.GeneraPrimaryKey(),
                ID_QUESTION = idQst,
                DESCRIPTION = "No",
                VALUE_INT = 1,
                NUM_ORDER = 2,
                VALID_DTA_INI = DateTime.Now,
                VALID_DTA_END = null,
                COD_USER = codUser,
                COD_TERMID = codTermid,
                TMS_TIMESTAMP = DateTime.Now
            });
            db.XR_VAL_QUESTION_ANSWER.Add(new XR_VAL_QUESTION_ANSWER()
            {
                ID_QST_ANSWER = db.XR_VAL_QUESTION_ANSWER.GeneraPrimaryKey(),
                ID_QUESTION = idQst,
                DESCRIPTION = "Sì",
                NUM_ORDER = 1,
                VALUE_INT = 2,
                VALID_DTA_INI = DateTime.Now,
                VALID_DTA_END = null,
                COD_USER = codUser,
                COD_TERMID = codTermid,
                TMS_TIMESTAMP = DateTime.Now
            });
        }
        private static void Caricamento_QUESTION(string codUser, string codTermid)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                XR_VAL_QUESTION_TYPE qType = db.XR_VAL_QUESTION_TYPE.FirstOrDefault(x => x.NAME == "Intero");
                XR_VAL_QUESTION_DISPLAY disp = db.XR_VAL_QUESTION_DISPLAY.FirstOrDefault(x => x.NAME.Contains("Radio"));

                //Comportamenti
                XR_VAL_QUESTION_GROUP group = db.XR_VAL_QUESTION_GROUP.FirstOrDefault(x => x.NAME == "Comportamenti");

                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = db.XR_VAL_QUESTION.GeneraPrimaryKey(),
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = disp.ID_QST_DISPLAY,
                    NAME = "Autonomia e orientamento al problem solving",
                    DESCRIPTION = "decide e opera in ambiti non regolamentati, promuove il cambiamento e organizza in un'ottica di medio termine, elabora soluzioni alternative in circostanze di incertezza o inaspettate",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = db.XR_VAL_QUESTION.GeneraPrimaryKey(),
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = disp.ID_QST_DISPLAY,
                    NAME = "Contributo ideativo/innovativo",
                    DESCRIPTION = "individua soluzioni appropriate e innovative, introduce elementi di differenziazione e novità in un'ottica di efficientamento aziendale",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = db.XR_VAL_QUESTION.GeneraPrimaryKey(),
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = disp.ID_QST_DISPLAY,
                    NAME = "Integrazione/Team working",
                    DESCRIPTION = "sa costruire e sviluppare raporti collaborativi all'interno del gruppo di lavoro, condivide le informazioni utili al mantenimento dell'equilibrio del gruppo in funzione del raggiungimento degli obiettivi",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = db.XR_VAL_QUESTION.GeneraPrimaryKey(),
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = disp.ID_QST_DISPLAY,
                    NAME = "Leadership",
                    DESCRIPTION = "costituisce un punto di riferimento per il mantenimento e lo sviluppo di know-how interni, dimostra capacità di coordinamento di gruppi di lavoro",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = db.XR_VAL_QUESTION.GeneraPrimaryKey(),
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = disp.ID_QST_DISPLAY,
                    NAME = "Capacità di adattamento ai cambiamenti",
                    DESCRIPTION = "si adatta alle situazioni distanti dai proprio modelli culturali, accetta e promuove il cambiamento adottando metodi di lavoro diversi da quelli sperimentati per il miglioramento della propria perfomance e, se occorre, di quella del gruppo di lavoro",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = db.XR_VAL_QUESTION.GeneraPrimaryKey(),
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = disp.ID_QST_DISPLAY,
                    NAME = "Organizzazione e Pianificazione a breve-medio e lungo termine",
                    DESCRIPTION = "organizza il lavoro secondo criteri di priorità, elabora piani di azione in funzione degli obiettivi ed in tale ottica identifica e organizza mezzi e risorse",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = db.XR_VAL_QUESTION.GeneraPrimaryKey(),
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = disp.ID_QST_DISPLAY,
                    NAME = "Flessibilità",
                    DESCRIPTION = "mostra una capacità di comprendere il contesto organizzativo-produttivo rivelando una elevata capacità di adattamento personale e professionale alle mutevoli esigenze del contesto",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = db.XR_VAL_QUESTION.GeneraPrimaryKey(),
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = disp.ID_QST_DISPLAY,
                    NAME = "Orientamento ai risultati",
                    DESCRIPTION = "accetta e affronta con entusiasmo obiettivi impegnativi, focalizzandosi sui risultati attesi e suggerisce eventuali modifiche ai piani stabiliti per migliorare le performance complessive rispetto agli standard attesi",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });

                //Prestazioni
                group = db.XR_VAL_QUESTION_GROUP.FirstOrDefault(x => x.NAME == "Prestazioni");
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = db.XR_VAL_QUESTION.GeneraPrimaryKey(),
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = disp.ID_QST_DISPLAY,
                    NAME = "Assolvimento del carico di lavoro assegnato",
                    DESCRIPTION = "conseguimento degli obiettivi assegnati",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = db.XR_VAL_QUESTION.GeneraPrimaryKey(),
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = disp.ID_QST_DISPLAY,
                    NAME = "Precisione e rispetto dei tempi",
                    DESCRIPTION = "rispetto dei tempi previsti in relazione agli incarichi di volta in volta assegnati; esecuzione dei compiti precisa e puntuale",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = db.XR_VAL_QUESTION.GeneraPrimaryKey(),
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = disp.ID_QST_DISPLAY,
                    NAME = "Assiduità in termini di presenza e disponibilità, secondo necessità ed emergenze",
                    DESCRIPTION = "livello di partecipazione e disponibilità rispetto al contesto organizzativo/produttivo",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });

                foreach (var qst in db.XR_VAL_QUESTION.Local.Where(x => x.XR_VAL_QUESTION_GROUP.NAME == "Prestazioni" || x.XR_VAL_QUESTION_GROUP.NAME == "Comportamenti"))
                    Caricamento_QUESTION_da1a5(db, qst.ID_QUESTION, codUser, codTermid);

                qType = db.XR_VAL_QUESTION_TYPE.FirstOrDefault(x => x.NAME == "StringaIntero");

                group = db.XR_VAL_QUESTION_GROUP.FirstOrDefault(x => x.NAME == "Attività straordinarie");
                XR_VAL_QUESTION_DISPLAY dispC = db.XR_VAL_QUESTION_DISPLAY.FirstOrDefault(x => x.NAME.Contains("Custom"));
                int idQst = 0;
                idQst = db.XR_VAL_QUESTION.GeneraPrimaryKey();
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = idQst,
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = dispC.ID_QST_DISPLAY,
                    NAME = "Attività 1",
                    DESCRIPTION = "",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                Caricamento_QUESTION_da1a3(db, idQst, codUser, codTermid);
                idQst = db.XR_VAL_QUESTION.GeneraPrimaryKey();
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = idQst,
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = dispC.ID_QST_DISPLAY,
                    NAME = "Attività 2",
                    DESCRIPTION = "",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                Caricamento_QUESTION_da1a3(db, idQst, codUser, codTermid);
                idQst = db.XR_VAL_QUESTION.GeneraPrimaryKey();
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = idQst,
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = dispC.ID_QST_DISPLAY,
                    NAME = "Attività 3",
                    DESCRIPTION = "",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                Caricamento_QUESTION_da1a3(db, idQst, codUser, codTermid);

                group = db.XR_VAL_QUESTION_GROUP.FirstOrDefault(x => x.NAME == "Conclusione");
                idQst = db.XR_VAL_QUESTION.GeneraPrimaryKey();
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = idQst,
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = dispC.ID_QST_DISPLAY,
                    NAME = "Lavoreresti ancora con?",
                    DESCRIPTION = "",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                Caricamento_QUESTION_da1a2(db, idQst, codUser, codTermid);
                idQst = db.XR_VAL_QUESTION.GeneraPrimaryKey();
                db.XR_VAL_QUESTION.Add(new XR_VAL_QUESTION()
                {
                    ID_QUESTION = idQst,
                    ID_QST_GROUP = group.ID_QST_GROUP,
                    ID_QST_TYPE = qType.ID_QST_TYPE,
                    ID_QST_DISPLAY = dispC.ID_QST_DISPLAY,
                    NAME = "Lo/la consiglieresti?",
                    DESCRIPTION = "",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                Caricamento_QUESTION_da1a2(db, idQst, codUser, codTermid);

                db.SaveChanges();
            }
        }
        private static void Caricamento_QUESTION_GROUP(string codUser, string codTermid)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                db.XR_VAL_QUESTION_GROUP.Add(new XR_VAL_QUESTION_GROUP
                {
                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(),
                    NAME = "Comportamenti",
                    DESCRIPTION = "Comportamenti",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION_GROUP.Add(new XR_VAL_QUESTION_GROUP
                {
                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(),
                    NAME = "Prestazioni",
                    DESCRIPTION = "Prestazioni",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION_GROUP.Add(new XR_VAL_QUESTION_GROUP
                {
                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(),
                    NAME = "Descrizione attività",
                    DESCRIPTION = "Descrizione attività",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION_GROUP.Add(new XR_VAL_QUESTION_GROUP
                {
                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(),
                    NAME = "Organizzazione",
                    DESCRIPTION = "Organizzazione",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION_GROUP.Add(new XR_VAL_QUESTION_GROUP
                {
                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(),
                    NAME = "Attività straordinarie",
                    DESCRIPTION = "Attività straordinarie",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION_GROUP.Add(new XR_VAL_QUESTION_GROUP
                {
                    ID_QST_GROUP = db.XR_VAL_QUESTION_GROUP.GeneraPrimaryKey(),
                    NAME = "Conclusione",
                    DESCRIPTION = "Conclusione",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });

                db.SaveChanges();
            }
        }
        private static void Caricamento_QUESTION_DISPLAY(string codUser, string codTermid)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                db.XR_VAL_QUESTION_DISPLAY.Add(new XR_VAL_QUESTION_DISPLAY()
                {
                    ID_QST_DISPLAY = db.XR_VAL_QUESTION_DISPLAY.GeneraPrimaryKey(),
                    NAME = "Radio button",
                    DESCRIPTION = "",
                    VALID_DTA_INI = DateTime.Now,
                    VALID_DTA_END = null,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION_DISPLAY.Add(new XR_VAL_QUESTION_DISPLAY()
                {
                    ID_QST_DISPLAY = db.XR_VAL_QUESTION_DISPLAY.GeneraPrimaryKey(),
                    NAME = "Select",
                    DESCRIPTION = "",
                    VALID_DTA_INI = DateTime.Now,
                    VALID_DTA_END = null,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION_DISPLAY.Add(new XR_VAL_QUESTION_DISPLAY()
                {
                    ID_QST_DISPLAY = db.XR_VAL_QUESTION_DISPLAY.GeneraPrimaryKey(),
                    NAME = "Edit",
                    DESCRIPTION = "",
                    VALID_DTA_INI = DateTime.Now,
                    VALID_DTA_END = null,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION_DISPLAY.Add(new XR_VAL_QUESTION_DISPLAY()
                {
                    ID_QST_DISPLAY = db.XR_VAL_QUESTION_DISPLAY.GeneraPrimaryKey(),
                    NAME = "Custom",
                    DESCRIPTION = "",
                    VALID_DTA_INI = DateTime.Now,
                    VALID_DTA_END = null,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });

                db.SaveChanges();
            }
        }
        private static void Caricamento_QUESTION_TYPE(string codUser, string codTermid)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                db.XR_VAL_QUESTION_TYPE.Add(new XR_VAL_QUESTION_TYPE
                {
                    ID_QST_TYPE = db.XR_VAL_QUESTION_TYPE.GeneraPrimaryKey(),
                    NAME = "Intero",
                    DESCRIPTION = "Valore intero",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION_TYPE.Add(new XR_VAL_QUESTION_TYPE
                {
                    ID_QST_TYPE = db.XR_VAL_QUESTION_TYPE.GeneraPrimaryKey(),
                    NAME = "Stringa",
                    DESCRIPTION = "Stringa",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_QUESTION_TYPE.Add(new XR_VAL_QUESTION_TYPE
                {
                    ID_QST_TYPE = db.XR_VAL_QUESTION_TYPE.GeneraPrimaryKey(),
                    NAME = "StringaIntero",
                    DESCRIPTION = "StringaIntero",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });

                db.SaveChanges();
            }
        }
        private static void Caricamento_RATING_OWNER(string codUser, string codTermid)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                db.XR_VAL_EVAL_RATING_OWNER.Add(new XR_VAL_EVAL_RATING_OWNER()
                {
                    ID_OWNER = db.XR_VAL_EVAL_RATING_OWNER.GeneraPrimaryKey(),
                    NAME = "Superiore",
                    DESCRIPTION = "Valutazione del diretto superiore",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });

                db.XR_VAL_EVAL_RATING_OWNER.Add(new XR_VAL_EVAL_RATING_OWNER()
                {
                    ID_OWNER = db.XR_VAL_EVAL_RATING_OWNER.GeneraPrimaryKey(),
                    NAME = "Collega",
                    DESCRIPTION = "Valutazione risorsa di pari livello/mansione",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_EVAL_RATING_OWNER.Add(new XR_VAL_EVAL_RATING_OWNER()
                {
                    ID_OWNER = db.XR_VAL_EVAL_RATING_OWNER.GeneraPrimaryKey(),
                    NAME = "Collaboratore",
                    DESCRIPTION = "Valutazione risorsa alle dipendenze",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });

                db.SaveChanges();
            }
        }
        private static void Caricamento_STATE(string codUser, string codTermid)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                db.XR_VAL_STATE.Add(new XR_VAL_STATE()
                {
                    ID_STATE = 10,
                    NAME = "In carico",
                    DESCRIPTION = "Elaborazione in corso",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_STATE.Add(new XR_VAL_STATE()
                {
                    ID_STATE = 20,
                    NAME = "Salvata come bozza",
                    DESCRIPTION = "Valutazione in bozza",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_STATE.Add(new XR_VAL_STATE()
                {
                    ID_STATE = 30,
                    NAME = "Convalidata",
                    DESCRIPTION = "Valutazione elaborata",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_STATE.Add(new XR_VAL_STATE()
                {
                    ID_STATE = 40,
                    NAME = "Presa visione",
                    DESCRIPTION = "Valutazione visionata",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });
                db.XR_VAL_STATE.Add(new XR_VAL_STATE()
                {
                    ID_STATE = 50,
                    NAME = "Consolidata",
                    DESCRIPTION = "Valutazione consolidata, non più modificabile",
                    VALID_DTA_INI = DateTime.Today,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = DateTime.Now
                });

                db.SaveChanges();
            }
        }
        #endregion

        #region GestioneStati
        public static bool CanModifyState(string owner)
        {
            return new string[] { "Superiore" }.Contains(owner);
        }
        public static void SalvaStato(IncentiviEntities db, int idEval, int rifState)
        {
            ValutazioniManager.InvalidaStato(db, idEval, rifState);
            if (rifState == (int)ValutazioniState.Bozza)
                ValutazioniManager.InvalidaStato(db, idEval, (int)ValutazioniState.Convalidata);

            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);

            db.XR_VAL_OPER_STATE.Add(new XR_VAL_OPER_STATE()
            {
                ID_OPER = db.XR_VAL_OPER_STATE.GeneraPrimaryKey(),
                ID_EVALUATION = idEval,
                VALID_DTA_INI = tms,
                VALID_DTA_END = null,
                ID_STATE = rifState,
                ID_PERSONA = CommonHelper.GetCurrentIdPersona(),
                COD_USER = codUser,
                COD_TERMID = codTermid,
                TMS_TIMESTAMP = tms
            });
        }
        public static void InvalidaStato(IncentiviEntities db, int idEval, int rifState)
        {
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);

            foreach (var item in db.XR_VAL_OPER_STATE.Where(x => x.ID_EVALUATION == idEval && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)).Where(x => x.ID_STATE == rifState))
            {
                item.VALID_DTA_END = tms;
                item.COD_USER = codUser;
                item.COD_TERMID = codTermid;
                item.TMS_TIMESTAMP = tms;
            }
        }

        public static int GetMaxEvalState(ICollection<XR_VAL_OPER_STATE> states)
        {
            return states != null && states.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Any() ? states.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Max(x => x.ID_STATE) : 0;
        }

        public static XR_VAL_STATE GetCurrentEvalState(ICollection<XR_VAL_OPER_STATE> states)
        {
            return states != null && states.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Any() ? states.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Select(x => x.XR_VAL_STATE).OrderByDescending(x => x.ID_STATE).FirstOrDefault() : null;
        }
        #endregion

        public static List<CorsoPianoSvil> GetCorsi(string[] PuntiMiglioramento)
        {
            List<CorsoPianoSvil> suggerimenti = new List<CorsoPianoSvil>();
        
            var db = new IncentiviEntities();
            var dbTal = new myRaiDataTalentia.TalentiaEntities();

            if (PuntiMiglioramento != null && PuntiMiglioramento.Any())
            {
                foreach (var item in PuntiMiglioramento)
                {
                    int idGr = 0;
                    string desGr = "";
                    if (item.Contains('|'))
                    {
                        Int32.TryParse(item.Split('|')[0], out idGr);
                        desGr = item.Split('|')[1];
                    }
                    else
                        desGr = item;

                    IQueryable<XR_VAL_COURSE> elencoCorsi = null;
                    if (idGr > 0)
                        elencoCorsi = db.XR_VAL_COURSE.Where(x => x.ID_ITEM == idGr && x.COD_TIPO_ITEM == "group");
                    else
                        elencoCorsi = db.XR_VAL_COURSE.Join(db.XR_VAL_QUESTION_GROUP, x => x.ID_ITEM, y => y.ID_QST_GROUP, (x, y) => new { Corso = x, DesComp = y.NAME })
                                        .Where(x => x.Corso.COD_TIPO_ITEM == "group" && x.DesComp == desGr).Select(x => x.Corso);

                    if (elencoCorsi != null && elencoCorsi.Any())
                    {
                        foreach (var pack in elencoCorsi.GroupBy(x => x.COD_NOME_PACCHETTO))
                        {
                            if (pack.Key == null)
                            {
                                foreach (var sugg in pack)
                                {
                                    var corso = dbTal.CORSO.Find(sugg.ID_CORSO);
                                    if (corso != null)
                                    {
                                        CorsoPianoSvil corsoPianoSvil = new CorsoPianoSvil()
                                        {
                                            IdCorso = corso.ID_CORSO,
                                            CodCorso = corso.COD_CORSO,
                                            IdComp = idGr,
                                            CodComp = desGr,
                                            TipoSugg = sugg.COD_TIPO_CONS
                                        };
                                        suggerimenti.Add(corsoPianoSvil);
                                    }
                                }
                            }
                            else
                            {
                                CorsoPianoSvil pacchetto = new CorsoPianoSvil()
                                {
                                    IdCorso = 0,
                                    CodCorso = pack.Key,
                                    IdComp = idGr,
                                    CodComp = desGr,
                                    TipoSugg = pack.First().COD_TIPO_CONS
                                };
                                pacchetto.CorsiPacchetto = new List<CorsoPianoSvil>();
                                foreach (var sugg in pack)
                                {
                                    var corso = dbTal.CORSO.Find(sugg.ID_CORSO);
                                    if (corso != null)
                                    {
                                        CorsoPianoSvil corsoPianoSvil = new CorsoPianoSvil()
                                        {
                                            IdCorso = corso.ID_CORSO,
                                            CodCorso = corso.COD_CORSO,
                                            IdComp = idGr,
                                            CodComp = desGr,
                                            TipoSugg = sugg.COD_TIPO_CONS
                                        };
                                        pacchetto.CorsiPacchetto.Add(corsoPianoSvil);
                                    }
                                }
                                suggerimenti.Add(pacchetto);
                            }
                        }
                    }
                }

            }

            return suggerimenti;
        }
    }
}