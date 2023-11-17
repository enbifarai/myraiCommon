using myRaiData;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace myRaiHelper
{
    public class CeitonHelper
    {
        public static MyRai_AttivitaCeiton GetAttivitaCeiton(int idRich)
        {
            var db = new digiGappEntities();
            var rich = db.MyRai_Richieste.Find(idRich);
            if (rich != null)
                return rich.MyRai_AttivitaCeiton;
            else
                return null;

        }
        public static bool IsApprovatoreCeiton(string matr)
        {
            digiGappEntities db = new digiGappEntities();
            return db.MyRai_AttivitaCeiton.Any(a => a.MatricolaResponsabile == matr);
        }

        public static WeekPlan GetCeitonWeekPlan ( )
        {
            if ( !UtenteHelper.GestitoSirio( ) )
                return null;

            WeekPlan wp = Sirio.Helper.GetWeekPlanCached( CommonHelper.GetCurrentUserPMatricola( ), CommonHelper.GetCurrentUserMatricola( ) );
            return wp;
        }

        public static WeekPlan GetCeitonWeekPlan(string pMatricola, string matricola)
        {
            if (!UtenteHelper.GestitoSirio()) return null;

            WeekPlan wp = Sirio.Helper.GetWeekPlanCached(pMatricola, matricola);
            return wp;
        }
    }

    public class WeekPlan
    {
        public WeekPlan ( )
        {
            Days = new List<DayPlan>( );
        }
        public string Matricola { get; set; }
        public List<DayPlan> Days { get; set; }
        public string Cod_Eccezione { get; set; }
    }

    public class DayPlan
    {
        public DayPlan()
        {
            Activities = new List<DayActivity>();
        }
        public DateTime Date { get; set; }
        public List<DayActivity> Activities { get; set; }

        public override string ToString()
        {
            return String.Format("{0:dd/MM/yyyy} - {1} attività", Date, Activities.Count);
        }
    }

    public class DayActivity
    {
        public string Location { get; set; }
        public string Title { get; set; }
        public string Schedule { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string MainActivity { get; set; }
        public string SecActivity { get; set; }
        public string DoneActivity { get; set; }
        public string Manager { get; set; }
        public string Matricola { get; set; }
        public DateTime Date { get; set; }
        public void SetStartTime(string startTimeStr)
        {
            TimeSpan temp;
            TimeSpan.TryParseExact(startTimeStr, "g", null, out temp);
            StartTime = temp;
        }
        public void SetEndTime(string endTimeStr)
        {
            TimeSpan temp;
            TimeSpan.TryParseExact(endTimeStr, "g", null, out temp);
            EndTime = temp;
        }

        public string idAttivita { get; set; }
        public string Note { get; set; }
        public string Uorg { get; set; }

        public int Eccezioni { get; set; }
    }
}
