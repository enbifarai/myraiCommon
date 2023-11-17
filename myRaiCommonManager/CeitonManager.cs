using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace myRaiCommonManager
{

    public class CeitonManager
    {
        public static bool ActivityAvailableToday(DateTime date)
        {
            if (CommonHelper.GetParametro<Boolean>(EnumParametriSistema.ProponiAttivitaSeAttivitaNelGiornoAnchePerAltreEccezioni))
            {
                WeekPlan resp = myRaiHelper.Sirio.Helper.GetWeekPlanCached(CommonHelper.GetCurrentUserPMatricola(), CommonHelper.GetCurrentUserMatricola(), date);
                return resp.Days.Any(x => x.Activities.Any());
            }
            else return false;
        }
        public static bool AggiornaGiornataCeiton(int IdRichiestaPadre)
        {

            var db = new digiGappEntities();
            MyRai_Richieste ec = db.MyRai_Richieste.Where(x => x.id_richiesta == IdRichiestaPadre).FirstOrDefault();

            try
            {
                var eccList = ec.MyRai_Eccezioni_Richieste.Where(x => x.azione == "I").ToList();
                if (eccList.Count() > 1)
                {
                    bool esito = false;
                    foreach (var e in eccList.Where(x=>x.numero_documento>0))
                    {
                        esito = myRaiCommonTasks.Ceiton.AggiornaGiornataCeiton_Execute(ec.matricola_richiesta, e.data_eccezione);
                    }
                    return esito;
                }
                else
                {
                    return myRaiCommonTasks.Ceiton.AggiornaGiornataCeiton_Execute(null, null, IdRichiestaPadre);
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "CeitonManager.AggiornaGiornataCeiton",
                    error_message = ex.ToString()
                });
                return false;
            }




            try
            {
                return myRaiCommonTasks.Ceiton.AggiornaGiornataCeiton_Execute(null, null, IdRichiestaPadre);
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "CeitonManager.AggiornaGiornataCeiton",
                    error_message = ex.ToString()
                });
                return false;
        }
        }


        public static bool AggiornaGiornataCeitonForBatch ( int IdRichiestaPadre, string matricola )
        {

            var db = new digiGappEntities ( );
            MyRai_Richieste ec = db.MyRai_Richieste.Where ( x => x.id_richiesta == IdRichiestaPadre ).FirstOrDefault ( );

            try
            {
                var eccList = ec.MyRai_Eccezioni_Richieste.Where ( x => x.azione == "I" ).ToList ( );
                if ( eccList.Count ( ) > 1 )
                {
                    bool esito = false;
                    foreach ( var e in eccList.Where ( x => x.numero_documento > 0 ) )
                    {
                        esito = myRaiCommonTasks.Ceiton.AggiornaGiornataCeiton_Execute ( ec.matricola_richiesta , e.data_eccezione );
                    }
                    return esito;
                }
                else
                {
                    return myRaiCommonTasks.Ceiton.AggiornaGiornataCeiton_Execute ( null , null , IdRichiestaPadre );
                }
            }
            catch ( Exception ex )
            {
                Logger.LogErrori ( new MyRai_LogErrori ( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    matricola = matricola ,
                    provenienza = "CeitonManager.AggiornaGiornataCeiton" ,
                    error_message = ex.ToString ( )
                }, matricola );
                return false;
            }
        }

        public static bool AggiornaGiornataCeitonForBatchConStorni ( int IdRichiestaPadre , string matricola )
        {

            var db = new digiGappEntities( );
            MyRai_Richieste ec = db.MyRai_Richieste.Where( x => x.id_richiesta == IdRichiestaPadre ).FirstOrDefault( );

            try
            {
                var eccList = ec.MyRai_Eccezioni_Richieste.Where( x => x.id_richiesta.Equals( IdRichiestaPadre ) ).ToList( );
                bool esito = false;
                foreach ( var e in eccList )
                {
                    esito = myRaiCommonTasks.Ceiton.AggiornaGiornataCeiton_Execute( ec.matricola_richiesta , e.data_eccezione );
                }
                return esito;
            }
            catch ( Exception ex )
            {
                Logger.LogErrori( new MyRai_LogErrori( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    matricola = matricola ,
                    provenienza = "CeitonManager.AggiornaGiornataCeitonForBatchConStorni" ,
                    error_message = ex.ToString( )
                } , matricola );
                return false;
            }
        }

        public static void RecuperaInvioCeiton(int id)
        {
            var db = new digiGappEntities();
            var Log = db.MyRai_CeitonLog.Find(id);
            if (Log == null) return;

            MyRaiServiceInterface.CeitonWS.GappPrpService grs = new MyRaiServiceInterface.CeitonWS.GappPrpService();
            grs.Credentials = CommonHelper.GetUtenteServizioCredentials();

            MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest tupD = new MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest();
            tupD = Deserialize(Log.contenuto_json);

            Log.data_ultimo_invio = DateTime.Now;

            MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningResponse updPlRe = new MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningResponse(); ;
            try
            {
                updPlRe = grs.updateAttendanceAbsencePlanning(tupD);
            }
            catch (Exception ex)
            {
                Log.esito = false;
                Log.errore = ex.ToString();
                db.SaveChanges();

                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "CeitonManager.RecuperaInvioCeiton",
                    error_message = ex.ToString()
                });
                return;
            }


            if (updPlRe.AttendanceAbsencePlanning.ErrorNumber != null)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "CeitonManager.RecuperaInvioCeiton",
                    error_message = Log.guid + "-Error:"
                    + updPlRe.AttendanceAbsencePlanning.ErrorNumber + " " +
                    updPlRe.AttendanceAbsencePlanning.ErrorMessage
                });
                Log.esito = false;
                Log.errore = updPlRe.AttendanceAbsencePlanning.ErrorNumber + " " +
                              updPlRe.AttendanceAbsencePlanning.ErrorMessage;
                db.SaveChanges();
                return;

            }
            Log.esito = true;
            Log.errore = null;

            db.SaveChanges();
        }

        //private static bool AggiornaGiornataCeiton_Execute(int IdRichiestaPadre)
        //{
        //    var db = new digiGappEntities();
        //    var RichiestaPadre = db.MyRai_Richieste.Where(x => x.id_richiesta == IdRichiestaPadre).FirstOrDefault();

        //    if (RichiestaPadre == null || !RichiestaPadre.gestito_sirio)
        //    {
        //        return false;
        //    }

        //    Guid g = Guid.NewGuid();
        //    List<string> MacroEcc = myRaiCommonTasks.Ceiton. GetEccezioniCeiton();

        //    WSDigigapp serviceWS = new WSDigigapp()
        //    {
        //        Credentials = CommonHelper.GetUtenteServizioCredentials()
        //    };
        //    var gio = serviceWS.getEccezioni(RichiestaPadre.matricola_richiesta, RichiestaPadre.periodo_dal.ToString("ddMMyyyy"), "BU", 75);

        //    MyRaiServiceInterface.CeitonWS.GappPrpService grs = new MyRaiServiceInterface.CeitonWS.GappPrpService();

        //    // grs.Credentials = System.Net.CredentialCache.DefaultCredentials;

        //    string[] AccountUtenteServizio = CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
        //    grs.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");


        //    MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest tupD = new MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest();
        //    tupD.AttendanceAbsencePlanning = new MyRaiServiceInterface.CeitonWS.AttendanceAbsencePlanning();

        //    tupD.AttendanceAbsencePlanning.Id = g.ToString();

        //    tupD.AttendanceAbsencePlanning.DateTime = DateTime.Now;
        //    tupD.AttendanceAbsencePlanning.MessageType = MyRaiServiceInterface.CeitonWS.MessageType.Request;
        //    tupD.AttendanceAbsencePlanning.Origin = "GAPP";
        //    tupD.AttendanceAbsencePlanning.Destination = "CEITON";
        //    tupD.AttendanceAbsencePlanning.AttendanceAbsence = new MyRaiServiceInterface.CeitonWS.AttendanceAbsenceType();
        //    tupD.AttendanceAbsencePlanning.AttendanceAbsence.WorkOrderID = g.ToString();
        //    tupD.AttendanceAbsencePlanning.AttendanceAbsence.EmployeeNumber = "P" + RichiestaPadre.matricola_richiesta;
        //    tupD.AttendanceAbsencePlanning.AttendanceAbsence.PlanningDate = RichiestaPadre.periodo_dal;
        //    tupD.AttendanceAbsencePlanning.AttendanceAbsence.PlannedPostponementSpecified = false; //   IMPORTANTE *****
        //    tupD.AttendanceAbsencePlanning.AttendanceAbsence.Planning = new MyRaiServiceInterface.CeitonWS.AttendanceAbsenceTypePlanning(); //   IMPORTANTE *****


        //    string previsto = gio.giornata.orarioTeorico.PadRight(2, ' ');
        //    string effettivo = gio.giornata.orarioReale.PadRight(2, ' ');
        //    if (effettivo != "  ")
        //    {
        //        tupD.AttendanceAbsencePlanning.AttendanceAbsence.Planning.PlannedShiftCode = effettivo;
        //    }
        //    else
        //    {
        //        tupD.AttendanceAbsencePlanning.AttendanceAbsence.Planning.PlannedShiftCode = previsto;
        //    }
        //    List<MyRaiServiceInterface.CeitonWS.ExceptionItemType> lstEx = new List<MyRaiServiceInterface.CeitonWS.ExceptionItemType>();

        //    foreach (var ec in gio.eccezioni)
        //    {
        //        if (MacroEcc.Contains(ec.cod.Trim()))
        //        {
        //            DateTime? df = myRaiCommonTasks.CommonTasks.GetDateTimeWithOverflow (ec.data, ec.dalle);
        //            DateTime? dt = myRaiCommonTasks.CommonTasks.GetDateTimeWithOverflow (ec.data, ec.alle);

        //            if (df != null && dt != null)
        //            {
        //                lstEx.Add(new MyRaiServiceInterface.CeitonWS.ExceptionItemType
        //                {
        //                    ExceptionCode = ec.cod.Trim(),
        //                    FromDate = (DateTime) df,
        //                    ToDate = (DateTime) dt,
        //                    ActivityId =
        //                             (RichiestaPadre.MyRai_AttivitaCeiton == null
        //                              ? null
        //                              : RichiestaPadre.MyRai_AttivitaCeiton.idCeiton)

        //                });
        //            }
        //        }
        //    }

        //    tupD.AttendanceAbsencePlanning.AttendanceAbsence.Exceptions = lstEx.ToArray();

        //    string Serialized_tupD = Serialize(tupD);

        //    Logger.LogAzione(new MyRai_LogAzioni()
        //    {
        //        applicativo = "PORTALE",
        //        operazione = "AggiornamentoCeiton",
        //        data = DateTime.Now,
        //        matricola = CommonHelper.GetCurrentUserMatricola(),
        //        provenienza = "CeitonManager.AggiornaGiornataCeiton",
        //        descrizione_operazione = Serialized_tupD
        //    });

        //    MyRai_CeitonLog Log = new MyRai_CeitonLog()
        //    {
        //        contenuto_json = Serialized_tupD,
        //        data = RichiestaPadre.periodo_dal,
        //        matricola = RichiestaPadre.matricola_richiesta,
        //        data_ultimo_invio = DateTime.Now,
        //         guid=g.ToString(),
        //         ultimo_invio_da="PORTALE",
        //         errore=null,
        //         esito=true
        //    };

        //    MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningResponse updPlRe = new MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningResponse(); ;
        //    try
        //    {
        //          updPlRe = grs.updateAttendanceAbsencePlanning(tupD);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.esito = false;
        //        Log.errore = ex.ToString();
        //        db.MyRai_CeitonLog.Add(Log);
        //        db.SaveChanges();

        //        Logger.LogErrori(new MyRai_LogErrori()
        //        {
        //            applicativo = "PORTALE",
        //            data = DateTime.Now,
        //            matricola = CommonHelper.GetCurrentUserMatricola(),
        //            provenienza = "CeitonManager.AggiornaGiornataCeiton",
        //            error_message = ex.ToString() 
        //        });
        //        return false;
        //    }


        //    if (updPlRe.AttendanceAbsencePlanning.ErrorNumber != null)
        //    {
        //        Logger.LogErrori(new MyRai_LogErrori()
        //        {
        //            applicativo = "PORTALE",
        //            data = DateTime.Now,
        //            matricola = CommonHelper.GetCurrentUserMatricola(),
        //            provenienza = "CeitonManager.AggiornaGiornataCeiton",
        //            error_message = g.ToString() + "-Error:"
        //            + updPlRe.AttendanceAbsencePlanning.ErrorNumber + " " +
        //            updPlRe.AttendanceAbsencePlanning.ErrorMessage
        //        });
        //        Log.esito = false;
        //        Log.errore =     updPlRe.AttendanceAbsencePlanning.ErrorMessage;
        //        Log.error_code = updPlRe.AttendanceAbsencePlanning.ErrorNumber;
        //        db.MyRai_CeitonLog.Add(Log);
        //        db.SaveChanges();
        //        return false;
        //    }

        //    db.MyRai_CeitonLog.Add(Log);
        //    db.SaveChanges();
        //    return true;
        //}

        public static bool AggiungiAttivitaCeitonDB(DayActivity dayActivity, int IdRichiestaPadre)
        {
            var db = new digiGappEntities();
            try
            {
                var att = db.MyRai_AttivitaCeiton.Where(x => x.idCeiton == dayActivity.idAttivita).FirstOrDefault();
                var rich = db.MyRai_Richieste.Where(x => x.id_richiesta == IdRichiestaPadre).FirstOrDefault();

                if (att != null)
                {
                    rich.matricola_consuntivazione = att.Matricola;
                    rich.MyRai_AttivitaCeiton = att;
                }
                else
                {
                    MyRai_AttivitaCeiton attNew = new MyRai_AttivitaCeiton()
                    {
                        AttivitaPrimaria = dayActivity.MainActivity,
                        AttivitaSecondaria = dayActivity.SecActivity,
                        AttivitaSvolta = dayActivity.DoneActivity,
                        DataCreazione = DateTime.Now,
                        Note = dayActivity.Note,
                        Titolo = dayActivity.Title,
                        Uorg = dayActivity.Uorg,
                        idCeiton = dayActivity.idAttivita,
                        OraInizioAttivita = dayActivity.StartTime.Hours.ToString().PadLeft(2, '0') + ":" +
                                            dayActivity.StartTime.Minutes.ToString().PadLeft(2, '0'),

                        OraFineAttivita = dayActivity.EndTime.Hours.ToString().PadLeft(2, '0') + ":" +
                                            dayActivity.EndTime.Minutes.ToString().PadLeft(2, '0'),

                        MatricolaResponsabile = (String.IsNullOrWhiteSpace(dayActivity.Manager) ? "" : dayActivity.Manager.Trim().Replace("P", "")),
                        Matricola = dayActivity.Matricola,
                        DataAttivita = dayActivity.Date
                    };
                    db.MyRai_AttivitaCeiton.Add(attNew);
                    rich.MyRai_AttivitaCeiton = attNew;
                    rich.matricola_consuntivazione = attNew.Matricola;
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "AggiungiAttivitaCeiton"
                });
                return false;
            }
            return true;
        }

        public static string Serialize(MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest t)
        {
            string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(t);
            return serialized;
        }
        public static MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest Deserialize(string s)
        {
            MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest t = Newtonsoft.Json.JsonConvert.DeserializeObject<MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest>(s);
            return t;
        }
        public static ApprovazioniAttivitaModel GetApprovazioneAttivitaModel(string nome, string data_da,
            string data_a, string stato, string titolo, string eccezione)
        {
            ApprovazioniAttivitaModel model = new ApprovazioniAttivitaModel();
            model.matricola = CommonHelper.GetCurrentUserMatricola();
            var db = new digiGappEntities();
            string matricola = CommonHelper.GetCurrentUserMatricola();

            bool cercaNome = !String.IsNullOrWhiteSpace(nome);

            var attivita = db.MyRai_AttivitaCeiton.AsNoTracking()
             .Where(z => z.MatricolaResponsabile == matricola
                 );

            if (!String.IsNullOrWhiteSpace(titolo))
            {
                attivita = attivita.Where(x => x.Titolo == titolo);
            }

            if (!String.IsNullOrWhiteSpace(data_da))
            {
                DateTime d;
                DateTime.TryParseExact(data_da, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out d);
                attivita = attivita.Where(x => x.DataAttivita >= d);
            }
            if (!String.IsNullOrWhiteSpace(data_a))
            {
                DateTime d;
                DateTime.TryParseExact(data_a, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out d);
                attivita = attivita.Where(x => x.DataAttivita <= d);
            }
            List<MyRai_AttivitaCeiton> attivitaList = attivita.ToList();

            if (!String.IsNullOrWhiteSpace(nome))
            {
                foreach (var att in attivitaList)
                {
                    List<MyRai_Richieste> L = att.MyRai_Richieste.Where(x => x.nominativo.ToLower().Contains(nome.ToLower())).ToList();
                    att.MyRai_Richieste = L;

                }
            }
            int idstato = 10;
            if (!String.IsNullOrWhiteSpace(stato))
            {
                idstato = Convert.ToInt32(stato);
            }

            model.MostraApprovaTutti = idstato == 10;

            foreach (var att in attivitaList)
            {
                List<MyRai_Richieste> L = att.MyRai_Richieste.Where(x => x.id_stato == idstato).ToList();
                att.MyRai_Richieste = L;
            }

            if (!String.IsNullOrWhiteSpace(eccezione))
            {
                foreach (var att in attivitaList)
                {
                    List<MyRai_Richieste> L = att.MyRai_Richieste.Where(x => x.MyRai_Eccezioni_Richieste.Any
                        (z => z.cod_eccezione.Trim() == eccezione)).ToList();
                    att.MyRai_Richieste = L;

                }
            }

            attivitaList.RemoveAll(x => x.MyRai_Richieste.Count == 0);

            //atti[0].MyRai_Richieste.Remove(atti[0].MyRai_Richieste.First());


            model.ListaDate =
                attivitaList
                //db.MyRai_AttivitaCeiton
                //.Where (z=>z.Matricola ==matricola &&
                //    z.MyRai_Richieste.Any (e=>e.id_stato==10) 
                //    )
                // .ToList()
                .GroupBy(
                    d => d.DataAttivita,
                    d => d,
                    (key, g) => new DataRichiesta
                    {
                        data = (DateTime)key,
                        ListaAttivita = g.ToList(),
                        MatricoleVisualizzateQuestaGiornata = db.MyRai_Visualizzazione_Giornate_Da_Segreteria
                            .Where(x =>
                                x.DataRichiesta.Year == ((DateTime)key).Year &&
                                x.DataRichiesta.Month == ((DateTime)key).Month &&
                                x.DataRichiesta.Day == ((DateTime)key).Day &&
                                x.Visualizzato)
                            .Select(x => x.Matricola).ToList()
                    }
                ).ToList();




            return model;
        }

        public static string ControllaPosizionamento(DayActivity dayActivity, InserimentoEccezioneModel model)
        {
            if (dayActivity == null || dayActivity.StartTime == null || dayActivity.EndTime == null) return "Periodo attività non disponibile";

            var db = new digiGappEntities();
            string chiave = EnumParametriSistema.CeitonPosizionamentoEccezioni.ToString();
            var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).ToList();
            var par_eccezione = par.Where(x => x.Valore1.ToUpper().Split(',').Contains(model.cod_eccezione)).FirstOrDefault();

            if (par_eccezione == null) return "Specifiche non trovate per " + model.cod_eccezione;

            if (String.IsNullOrWhiteSpace(model.dalle) || String.IsNullOrWhiteSpace(model.alle)) return "Non è stato specificato l'intervallo della eccezione";

            if (par_eccezione.Valore2.Contains("0") && !TimeSpanEqualString(dayActivity.StartTime, model.alle))
                return "Fine intervallo non coincidente con inizio attività";

            if (par_eccezione.Valore2.Contains("2") && !TimeSpanEqualString(dayActivity.EndTime, model.dalle))
                return "Inizio intervallo non coincidente con fine attività";

            if (par_eccezione.Valore2.Contains("1") &&
                 (!StringBetweenTimeSpans(dayActivity.StartTime, dayActivity.EndTime, model.dalle) || !StringBetweenTimeSpans(dayActivity.StartTime, dayActivity.EndTime, model.alle)))
                return "Intervallo non compreso nell'attività";


            //0 prima- 1 durante- 2 dopo
            return null;

        }

        private static bool TimeSpanEqualString(TimeSpan ts, string HHmm)
        {
            if (String.IsNullOrWhiteSpace(HHmm) || HHmm.Length != 5 || !HHmm.Contains(":")) return false;
            TimeSpan t = new TimeSpan(Convert.ToInt32(HHmm.Split(':')[0]), Convert.ToInt32(HHmm.Split(':')[1]), 0);
            return ts.Equals(t);
        }

        private static bool StringBetweenTimeSpans(TimeSpan t1, TimeSpan t2, string HHmm)
        {
            if (String.IsNullOrWhiteSpace(HHmm) || HHmm.Length != 5 || !HHmm.Contains(":")) return false;
            TimeSpan t = new TimeSpan(Convert.ToInt32(HHmm.Split(':')[0]), Convert.ToInt32(HHmm.Split(':')[1]), 0);
            return (t1 <= t && t2 >= t);
        }


        public static bool ReInvioCeiton(int id)
        {
            var db = new digiGappEntities();
            var Log = db.MyRai_CeitonLog.Find(id);
            if (Log == null) return false;

            MyRaiServiceInterface.CeitonWS.GappPrpService grs = new MyRaiServiceInterface.CeitonWS.GappPrpService();
            grs.Credentials = CommonHelper.GetUtenteServizioCredentials();

            MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest tupD = new MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest();
            tupD = Deserialize(Log.contenuto_json);

            Log.data_ultimo_invio = DateTime.Now;

            MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningResponse updPlRe = new MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningResponse(); ;
            try
            {
                updPlRe = grs.updateAttendanceAbsencePlanning(tupD);
            }
            catch (Exception ex)
            {
                Log.esito = false;
                Log.errore = ex.ToString();
                db.SaveChanges();

                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "CeitonManager.RecuperaInvioCeiton",
                    error_message = ex.ToString()
                });
                return false;
            }

            if (updPlRe.AttendanceAbsencePlanning.ErrorNumber != null)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "CeitonManager.RecuperaInvioCeiton",
                    error_message = Log.guid + "-Error:"
                    + updPlRe.AttendanceAbsencePlanning.ErrorNumber + " " +
                    updPlRe.AttendanceAbsencePlanning.ErrorMessage
                });
                Log.esito = false;
                Log.errore = updPlRe.AttendanceAbsencePlanning.ErrorNumber + " " +
                              updPlRe.AttendanceAbsencePlanning.ErrorMessage;
                db.SaveChanges();
                return false;

            }
            Log.esito = true;
            Log.errore = null;

            db.SaveChanges();

            return true;
        }


    }
}