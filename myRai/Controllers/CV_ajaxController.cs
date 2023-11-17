using myRaiCommonModel.cvModels;
using myRai.Data.CurriculumVitae;
using myRaiHelper;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class CV_ajaxController : BaseCommonController
    {
        [HttpPost]
        public ActionResult GetTitoloByCodTipoTitolo ( string codTipoTitolo )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            string matricola = UtenteHelper.EsponiAnagrafica( )._matricola;

            var listaTitolo_tmp = ( from titolo in cvEnt.DTitolo
                                    where titolo.CodTipoTitolo == codTipoTitolo
                                    select titolo ).ToList( );

            List<string> list_item = new List<string>( );
            listaTitolo_tmp = listaTitolo_tmp.OrderBy( c => c.DescTitolo ).ToList( );

            List<DTitolo> listaTitolo = new List<DTitolo>( );
            list_item = cvEnt.TCVIstruzione.Where( x => x.Matricola == matricola ).OrderBy( e => e.Istituto ).Select( y => y.CodTitolo ).ToList( );

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = listaTitolo_tmp }
            };
        }

        [HttpPost]
        public ActionResult GetServizioByAreaOrgServCodServizio ( string codAreaOrg )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );

            var servizio = ( from serv in cvEnt.DAreaOrgServ
                             where serv.CodAreaOrg == codAreaOrg
                             select serv.CodServizio ).ToList( );
            var listaServizi = ( from serv in cvEnt.VDServizio
                                 where servizio.Contains( serv.Codice )
                                 select serv ).ToList( );

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = listaServizi != null ? listaServizi.OrderBy( s => s.Descrizione ).ToList( ) : null }
            };
        }

        public ActionResult getTitoli ( )
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities( );
            cvModel.Studies model = new cvModel.Studies( CommonHelper.GetCurrentUserMatricola( ) );
            return View( "~/Views/CV_Online/studies/viewStudies.cshtml" , model );
        }
    }
}