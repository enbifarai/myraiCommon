using myRaiHelper;
using System;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class AttivitaController : BaseCommonController
    {
        public ActionResult Index ( )
        {
            if ( UtenteHelper.GestitoSirio( ) && UtenteHelper.IsAbilitatoGapp( ) )
            {
                return View( );
            }
            else
                throw new Exception( "Non autorizzato" );
        }

        [HttpGet]
        public PartialViewResult GetAttivita ( string dataDa , string dataA )
        {
            string matricola = CommonHelper.GetCurrentUserPMatricola( );

            WeekPlan weekPlan = new WeekPlan( );

            DateTime startDate = DateTime.Today;//new DateTime(2018,07,02);
            DateTime endDate = startDate.AddDays( 6 );
            if ( dataDa != null && dataA != null )
            {
                DateTime.TryParseExact( dataDa , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out startDate );
                DateTime.TryParseExact( dataA , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out endDate );
            }

            weekPlan = myRaiHelper.Sirio.Helper.GetWeekPlan( matricola , startDate , endDate );

            if ( weekPlan != null && weekPlan.Days.Any( ) )
            {
                weekPlan.Days.ForEach( d =>
                 {
                     if ( d.Activities != null && d.Activities.Any( ) )
                     {
                         d.Activities.RemoveAll( w => w.idAttivita.Equals( "000000" ) );
                     }
                 } );
            }

            return PartialView( "~/Views/Attivita/subpartial/elencoAttivita.cshtml" , weekPlan );
        }

        [HttpGet]
        public PartialViewResult GetAttivitaOggi ( )
        {
            string matricola = CommonHelper.GetCurrentUserPMatricola( );

            WeekPlan weekPlan = new WeekPlan( );

            DateTime startDate = DateTime.Today;//new DateTime(2018, 07, 02);
            DateTime endDate = startDate;

            weekPlan = myRaiHelper.Sirio.Helper.GetWeekPlan( matricola , startDate , endDate );

            if ( weekPlan != null && weekPlan.Days.Any( ) )
            {
                weekPlan.Days.ForEach( d =>
                {
                    if ( d.Activities != null && d.Activities.Any( ) )
                    {
                        d.Activities.RemoveAll( w => w.idAttivita.Equals( "000000" ) );
                    }
                } );
            }
            return PartialView( "~/Views/Attivita/subpartial/attivitaOggi.cshtml" , weekPlan );
        }
    }
}