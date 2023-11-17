using System;
using System.Diagnostics;
using System.Web.Mvc;
using myRaiData;
using myRaiHelper;

namespace myRai.Controllers
{
    public class ErrorController : Controller
    {
        public ErrorController ( )
        {

        }

        public ActionResult Error ( string objExc )
        {
            ViewBag.ExcMessage = objExc;

            using ( digiGappEntities db = new digiGappEntities( ) )
            {
                MyRai_LogErrori err = new MyRai_LogErrori( );
                err.feedback = "";
                err.matricola = CommonHelper.GetCurrentUserMatricola( );
                err.data = DateTime.Now;
                err.error_message = objExc;
                err.applicativo = "Portale";
                err.provenienza = new StackFrame( 1 , true ).GetMethod( ).Name;
                Logger.LogErrori( err );

                Session["ErrorId"] = err.Id;
            }

            return View( );
        }

        public ActionResult Error2 ( string objExc )
        {
            ViewBag.ExcMessage = objExc;

            using ( digiGappEntities db = new digiGappEntities( ) )
            {
                MyRai_LogErrori err = new MyRai_LogErrori( );
                err.feedback = "";
                err.matricola = CommonHelper.GetCurrentUserMatricola( );
                err.data = DateTime.Now;

                if ( String.IsNullOrEmpty( objExc ) && ViewData.Model != null )
                {
                    Exception ex = ( ( HandleErrorInfo ) ( ViewData.Model ) ).Exception;
                    objExc = ex.Message + "\r\n" + ex.StackTrace;
                }
                err.error_message = objExc;

                err.applicativo = "Portale";
                err.provenienza = new StackFrame( 1 , true ).GetMethod( ).Name;
                Logger.LogErrori( err );

                Session["ErrorId"] = err.Id;
            }

            return View( );
        }

        public ActionResult NonAbilitatoError ( )
        {
            return View( );
        }

        public ActionResult NonAbilitatoError2 ( )
        {
            return View( );
        }

        public ActionResult NotFound ( )
        {
            return View( "404" );
        }
    }
}