using myRaiData;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonTasks
{
   

    public class Ceiton
    {

        public static List<string> GetEccezioniCeiton()
        {
            var db = new digiGappEntities();
            List<string> MacroEcc = db.L2D_ECCEZIONE.Where(x => x.flag_eccez == "C").Select(x => x.cod_eccezione.Trim()).ToList();
            MacroEcc.AddRange(db.MyRai_Eccezioni_Ammesse.Where(x => x.RichiedeAttivitaCeiton).Select(x => x.cod_eccezione.Trim()).ToList());
            return MacroEcc;
        }

        public static bool AggiornaGiornataCeiton_Execute(string matricola, DateTime? data, int? IdRichiestaPadre = null)
        {
            digiGappEntities db = new digiGappEntities();
            MyRai_Richieste RichiestaPadre = null;

            if (IdRichiestaPadre != null)
            {
                RichiestaPadre = db.MyRai_Richieste.Where(x => x.id_richiesta == IdRichiestaPadre).FirstOrDefault();

                if (RichiestaPadre == null || ! RichiestaPadre.gestito_sirio)
                {
                    return false;
                }
                else
                {
                    matricola = RichiestaPadre.matricola_richiesta;
                    data = RichiestaPadre.periodo_dal;
                }
            }

            if ( RichiestaPadre != null &&
                RichiestaPadre.id_Attivita_ceiton.HasValue )
            {
                var attivitaID = RichiestaPadre.id_Attivita_ceiton.GetValueOrDefault( );
                // verifica che l'attività non sia una di quelle fittizie
                var att = db.MyRai_AttivitaCeiton.Where( w => w.id.Equals( attivitaID ) ).FirstOrDefault( );
                if (att != null)
                {
                    // se l'attività ha codice 00000, tale attività non va inviata a ceiton perchè non esiste.
                    if (att.idCeiton.Equals( "000000" ) )
                    {
                        RichiestaPadre.id_Attivita_ceiton = null;
                    }
                }
            }

            DateTime D = DateTime.Now;
            if (data != null) D = (DateTime) data;

            Boolean GestitoSirio = myRaiCommonTasks.DigigappInterface.IsGestitoSirio(matricola, D);

            if (!GestitoSirio) return false;

            string[] AccountUtenteServizio = CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.AccountUtenteServizio);

            Guid g = Guid.NewGuid();
            List<string> MacroEcc = myRaiCommonTasks.Ceiton.GetEccezioniCeiton();

            WSDigigapp serviceWS = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1])
            };
            dayResponse gio = serviceWS.getEccezioni(matricola, ((DateTime)data).ToString("ddMMyyyy"), "BU", 75);

            MyRaiServiceInterface.CeitonWS.GappPrpService grs = new MyRaiServiceInterface.CeitonWS.GappPrpService();

            grs.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");

            MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest tupD = new MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest();
            tupD.AttendanceAbsencePlanning = new MyRaiServiceInterface.CeitonWS.AttendanceAbsencePlanning();

            tupD.AttendanceAbsencePlanning.Id = g.ToString();

            tupD.AttendanceAbsencePlanning.DateTime = DateTime.Now;
            tupD.AttendanceAbsencePlanning.MessageType = MyRaiServiceInterface.CeitonWS.MessageType.Request;
            tupD.AttendanceAbsencePlanning.Origin = "GAPP";
            tupD.AttendanceAbsencePlanning.Destination = "CEITON";
            tupD.AttendanceAbsencePlanning.AttendanceAbsence = new MyRaiServiceInterface.CeitonWS.AttendanceAbsenceType();
            tupD.AttendanceAbsencePlanning.AttendanceAbsence.WorkOrderID = g.ToString();
            tupD.AttendanceAbsencePlanning.AttendanceAbsence.EmployeeNumber = "P" + matricola;
            tupD.AttendanceAbsencePlanning.AttendanceAbsence.PlanningDate = (DateTime)data;
            tupD.AttendanceAbsencePlanning.AttendanceAbsence.PlannedPostponementSpecified = false; //   IMPORTANTE *****
            tupD.AttendanceAbsencePlanning.AttendanceAbsence.Planning = new MyRaiServiceInterface.CeitonWS.AttendanceAbsenceTypePlanning(); //   IMPORTANTE *****

            string previsto = gio.giornata.orarioTeorico.PadRight(2, ' ');
            string effettivo = gio.giornata.orarioReale.PadRight(2, ' ');
            if (effettivo != "  ")
            {
                tupD.AttendanceAbsencePlanning.AttendanceAbsence.Planning.PlannedShiftCode = effettivo;
            }
            else
            {
                tupD.AttendanceAbsencePlanning.AttendanceAbsence.Planning.PlannedShiftCode = previsto;
            }
            List<MyRaiServiceInterface.CeitonWS.ExceptionItemType> lstEx = new List<MyRaiServiceInterface.CeitonWS.ExceptionItemType>();

            foreach (var ec in gio.eccezioni)
            {
                if (MacroEcc.Contains(ec.cod.Trim()))
                {
                    DateTime? df = myRaiCommonTasks.CommonTasks.GetDateTimeWithOverflow(ec.data, ec.dalle);
                    DateTime? dt = myRaiCommonTasks.CommonTasks.GetDateTimeWithOverflow(ec.data, ec.alle);
                    string idAct = null;
                    if (RichiestaPadre == null)
                    {
                        if (!String.IsNullOrWhiteSpace(ec.ndoc))
                        {
                            int n = Convert.ToInt32(ec.ndoc);
                            if (n != 0)
                            {
                                var eccNdoc = db.MyRai_Eccezioni_Richieste.Where(x => x.numero_documento == n && x.codice_sede_gapp == gio.giornata.sedeGapp).FirstOrDefault();
                                if (eccNdoc != null && eccNdoc.MyRai_Richieste.MyRai_AttivitaCeiton != null)
                                {
                                    if (!String.IsNullOrWhiteSpace(eccNdoc.MyRai_Richieste.MyRai_AttivitaCeiton.idCeiton))
                                    {
                                        if ( !eccNdoc.MyRai_Richieste.MyRai_AttivitaCeiton.idCeiton.Equals( "000000" ) )
                                        {
                                        idAct = eccNdoc.MyRai_Richieste.MyRai_AttivitaCeiton.idCeiton;
                                        }
                                    }   
                                }
                            }
                        }
                    }
                    else if (RichiestaPadre.MyRai_AttivitaCeiton != null)
                    {
                        idAct = RichiestaPadre.MyRai_AttivitaCeiton.idCeiton;
                    }

                    if (df != null && dt != null)
                    {
                        lstEx.Add(new MyRaiServiceInterface.CeitonWS.ExceptionItemType
                        {
                            ExceptionCode = ec.cod.Trim(),
                            FromDate = (DateTime)df,
                            ToDate = (DateTime)dt,
                            ActivityId =idAct
                        });
                    }
                }
            }

            tupD.AttendanceAbsencePlanning.AttendanceAbsence.Exceptions = lstEx.ToArray();

            string Serialized_tupD = Serialize(tupD);

            MyRai_LogAzioni az = new MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                operazione = "AggiornamentoCeiton",
                data = DateTime.Now,
                matricola = RichiestaPadre != null ? RichiestaPadre.matricola_richiesta : matricola,
                provenienza = "CeitonManager.AggiornaGiornataCeiton",
                descrizione_operazione = Serialized_tupD
            };
            db.MyRai_LogAzioni.Add(az);
            db.SaveChanges();

            MyRai_CeitonLog Log = new MyRai_CeitonLog()
            {
                contenuto_json = Serialized_tupD,
                data = RichiestaPadre != null ? RichiestaPadre.periodo_dal : (DateTime)data,
                matricola = RichiestaPadre != null ? RichiestaPadre.matricola_richiesta : matricola,
                data_ultimo_invio = DateTime.Now,
                guid = g.ToString(),
                ultimo_invio_da = "PORTALE",
                errore = null,
                esito = true
            };

            MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningResponse updPlRe = new MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningResponse();
            try
            {
                updPlRe = grs.updateAttendanceAbsencePlanning(tupD);
            }
            catch (Exception ex)
            {
                Log.esito = false;
                Log.errore = ex.ToString();
                db.MyRai_CeitonLog.Add(Log);
                db.SaveChanges();

                MyRai_LogErrori err = new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = RichiestaPadre != null ? RichiestaPadre.matricola_richiesta : matricola,
                    provenienza = "CeitonManager.AggiornaGiornataCeiton",
                    error_message = ex.ToString()
                };
                db.MyRai_LogErrori.Add(err);
                db.SaveChanges();
                return false;
            }


            if (updPlRe.AttendanceAbsencePlanning.ErrorNumber != null)
            {
                MyRai_LogErrori err = new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = RichiestaPadre != null ? RichiestaPadre.matricola_richiesta : matricola,
                    provenienza = "CeitonManager.AggiornaGiornataCeiton",
                    error_message = g.ToString() + "-Error:"
                    + updPlRe.AttendanceAbsencePlanning.ErrorNumber + " " +
                    updPlRe.AttendanceAbsencePlanning.ErrorMessage
                };
                Log.esito = false;
                Log.errore = updPlRe.AttendanceAbsencePlanning.ErrorMessage;
                Log.error_code = updPlRe.AttendanceAbsencePlanning.ErrorNumber;
                db.MyRai_CeitonLog.Add(Log);
                db.MyRai_LogErrori.Add(err);
                db.SaveChanges();
                return false;
            }

            db.MyRai_CeitonLog.Add(Log);
            db.SaveChanges();
            return true;
        }

        public static ScheduallResponse AggiornaGiornataCeitonScheduall_Execute(string matricola, DateTime? data, int? IdRichiestaPadre = null,string OrarioTeorico=null, string OrarioReale=null)
        {
            ScheduallResponse Response = new ScheduallResponse();

            digiGappEntities db = new digiGappEntities();
            MyRai_Richieste RichiestaPadre = null;

            if (IdRichiestaPadre != null)
            {
                RichiestaPadre = db.MyRai_Richieste.Where(x => x.id_richiesta == IdRichiestaPadre).FirstOrDefault();

                if (RichiestaPadre == null || !RichiestaPadre.gestito_sirio)
                {
                    Response.Esito = false;
                    return Response;
                }
                else
                {
                    matricola = RichiestaPadre.matricola_richiesta;
                    data = RichiestaPadre.periodo_dal;
                }
            }


            string[] AccountUtenteServizio = CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.AccountUtenteServizio);

            Guid g = Guid.NewGuid();
            List<string> MacroEcc = myRaiCommonTasks.Ceiton.GetEccezioniCeiton();

            WSDigigapp serviceWS = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1])
            };
            dayResponse gio = serviceWS.getEccezioni(matricola, ((DateTime)data).ToString("ddMMyyyy"), "BU", 75);

            MyRaiServiceInterface.CeitonScheduall.GappRadioPrpService grs = new MyRaiServiceInterface.CeitonScheduall.GappRadioPrpService();

            grs.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");


            MyRaiServiceInterface.CeitonScheduall.t_UpdateAttendanceAbsencePlanningRequest tupD = new MyRaiServiceInterface.CeitonScheduall.t_UpdateAttendanceAbsencePlanningRequest();

            
            tupD.AttendanceAbsencePlanning = new MyRaiServiceInterface.CeitonScheduall.AttendanceAbsencePlanning();

            tupD.AttendanceAbsencePlanning.Id = g.ToString();

            tupD.AttendanceAbsencePlanning.DateTime = DateTime.Now;
            tupD.AttendanceAbsencePlanning.MessageType = MyRaiServiceInterface.CeitonScheduall.MessageType.Request;
            tupD.AttendanceAbsencePlanning.Origin = "GAPP";
            tupD.AttendanceAbsencePlanning.Destination = "SCHEDUALL";
            tupD.AttendanceAbsencePlanning.AttendanceAbsence = new MyRaiServiceInterface.CeitonScheduall.AttendanceAbsenceType();
            tupD.AttendanceAbsencePlanning.AttendanceAbsence.WorkOrderID = g.ToString();
            tupD.AttendanceAbsencePlanning.AttendanceAbsence.EmployeeNumber = matricola;
           // tupD.AttendanceAbsencePlanning.AttendanceAbsence.EmployeeNumber = "P" + matricola;
            tupD.AttendanceAbsencePlanning.AttendanceAbsence.PlanningDate = (DateTime)data;
            //    tupD.AttendanceAbsencePlanning.AttendanceAbsence.PlannedPostponementSpecified = false; //   IMPORTANTE *****
            //     tupD.AttendanceAbsencePlanning.AttendanceAbsence.Planning = new MyRaiServiceInterface.CeitonScheduall.AttendanceAbsenceTypePlanning(); //   IMPORTANTE *****




            string previsto = OrarioTeorico == null ? gio.giornata.orarioTeorico.PadRight(2, ' ') : OrarioTeorico;
            string effettivo = OrarioReale == null ? gio.giornata.orarioReale.PadRight(2, ' ') : OrarioReale;

            //if (effettivo != "  ")
            //{
            //    tupD.AttendanceAbsencePlanning.AttendanceAbsence.Planning.PlannedShiftCode = effettivo;
            //}
            //else
            //{
            //    tupD.AttendanceAbsencePlanning.AttendanceAbsence.Planning.PlannedShiftCode = previsto;
            //}


            tupD.AttendanceAbsencePlanning.AttendanceAbsence.PlannedShiftCode = previsto;
            tupD.AttendanceAbsencePlanning.AttendanceAbsence.ActualShiftCode = effettivo;

            List<MyRaiServiceInterface.CeitonScheduall.ExceptionItemType> lstEx = new List<MyRaiServiceInterface.CeitonScheduall.ExceptionItemType>();

            if (gio.eccezioni != null)
            {
                foreach (var ec in gio.eccezioni)
                {
                    if (MacroEcc.Contains(ec.cod.Trim()))
                    {
                        DateTime? df = myRaiCommonTasks.CommonTasks.GetDateTimeWithOverflow(ec.data, ec.dalle);
                        DateTime? dt = myRaiCommonTasks.CommonTasks.GetDateTimeWithOverflow(ec.data, ec.alle);

                        if (df != null && dt != null)
                        {
                            lstEx.Add(new MyRaiServiceInterface.CeitonScheduall.ExceptionItemType
                            {
                                ExceptionCode = ec.cod.Trim(),
                                FromDate = (DateTime)df,
                                ToDate = (DateTime)dt,
                                //ActivityId =
                                //         (RichiestaPadre == null || RichiestaPadre.MyRai_AttivitaCeiton == null
                                //          ? null
                                //          : RichiestaPadre.MyRai_AttivitaCeiton.idCeiton)

                            });
                        }
                    }
                }
            }
            

            tupD.AttendanceAbsencePlanning.AttendanceAbsence.Exceptions = lstEx.ToArray();

            string Serialized_tupD = SerializeScheduall(tupD);

            MyRai_LogAzioni az = new MyRai_LogAzioni()
            {
                applicativo = "PORTALE",
                operazione = "AggiornamentoScheduall",
                data = DateTime.Now,
                matricola = RichiestaPadre != null ? RichiestaPadre.matricola_richiesta : matricola,
                provenienza = "CeitonManager.AggiornaGiornataCeitonScheduall",
                descrizione_operazione = Serialized_tupD
            };
            db.MyRai_LogAzioni.Add(az);
            db.SaveChanges();

            MyRai_CeitonLog Log = new MyRai_CeitonLog()
            {
                contenuto_json = Serialized_tupD,
                data = RichiestaPadre != null ? RichiestaPadre.periodo_dal : (DateTime)data,
                matricola = RichiestaPadre != null ? RichiestaPadre.matricola_richiesta : matricola,
                data_ultimo_invio = DateTime.Now,
                guid = g.ToString(),
                ultimo_invio_da = "PORTALE",
                errore = null,
                esito = true
            };

            MyRaiServiceInterface.CeitonScheduall.t_UpdateAttendanceAbsencePlanningResponse updPlRe = new MyRaiServiceInterface.CeitonScheduall.t_UpdateAttendanceAbsencePlanningResponse();
            try
            {
                updPlRe = grs.updateAttendanceAbsencePlanning(tupD);
            }
            catch (Exception ex)
            {
                Log.esito = false;
                Log.errore = ex.ToString();
                db.MyRai_CeitonLog.Add(Log);
                db.SaveChanges();

                MyRai_LogErrori err = new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = RichiestaPadre != null ? RichiestaPadre.matricola_richiesta : matricola,
                    provenienza = "CeitonManager.AggiornaGiornataCeitonScheduall",
                    error_message = ex.ToString()
                };
                db.MyRai_LogErrori.Add(err);
                db.SaveChanges();
                Response.Esito = false;
                Response.ErrorMessage = ex.ToString();
                return Response;
            }


            if (!String.IsNullOrWhiteSpace(updPlRe.AttendanceAbsencePlanning.ErrorNumber))
            {
                MyRai_LogErrori err = new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = RichiestaPadre != null ? RichiestaPadre.matricola_richiesta : matricola,
                    provenienza = "CeitonManager.AggiornaGiornataCeitonScheduall",
                    error_message = g.ToString() + "-Error:"
                    + updPlRe.AttendanceAbsencePlanning.ErrorNumber + " " +
                    updPlRe.AttendanceAbsencePlanning.ErrorMessage
                };
                Log.esito = false;
                Log.errore = updPlRe.AttendanceAbsencePlanning.ErrorMessage;
                Log.error_code = updPlRe.AttendanceAbsencePlanning.ErrorNumber;
                db.MyRai_CeitonLog.Add(Log);
                db.MyRai_LogErrori.Add(err);
                db.SaveChanges();
                Response.Esito = false;
                Response.ErrorMessage = g.ToString() + "-Error:"
                                    + updPlRe.AttendanceAbsencePlanning.ErrorNumber + " "
                                    + updPlRe.AttendanceAbsencePlanning.ErrorMessage;
                return Response;
            }

            db.MyRai_CeitonLog.Add(Log);
            db.SaveChanges();
            Response.Esito = true;
            Response.Response = updPlRe;
            return Response;

        }

        public static string Serialize(MyRaiServiceInterface.CeitonWS.t_UpdateAttendanceAbsencePlanningRequest t)
        {
            string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(t);
            return serialized;
        }
        public static string SerializeScheduall(MyRaiServiceInterface.CeitonScheduall.t_UpdateAttendanceAbsencePlanningRequest t)
        {
            string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(t);
            return serialized;
        }
    }
    public class ScheduallResponse
    {
        public Boolean Esito { get; set; }
        public string ErrorMessage { get; set; }

        public MyRaiServiceInterface.CeitonScheduall.t_UpdateAttendanceAbsencePlanningResponse Response { get; set; }
    }
}