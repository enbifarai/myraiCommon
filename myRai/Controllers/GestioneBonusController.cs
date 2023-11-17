using myRai.Models;
using myRaiCommonModel;
using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class GestioneBonusController : Controller
    {
        #region private

        /// <summary>
        /// Reperimento di un utente a partire dalla matricola
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        private string GetUtenteByMatricola ( string term )
        {
            it.rai.servizi.hrgb.Service s = new it.rai.servizi.hrgb.Service( );
            var r = s.EsponiAnagrafica_Net( "COMINT" , term );

            if ( r.DT_Anagrafica.Rows != null )
            {
                foreach ( System.Data.DataRow item in r.DT_Anagrafica.Rows )
                {
                    return ( item[0].ToString( ) + " " + item[1].ToString( ) ).Trim( );
                }
            }
            return null;
        }

        private List<DettaglioSceltaDipendente> GetElenco ()
        {
            List<DettaglioSceltaDipendente> elenco = new List<DettaglioSceltaDipendente>( );
            List<MyRai_LogAzioni> azioni = new List<MyRai_LogAzioni>( );
            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    azioni = db.MyRai_LogAzioni.Where( w => w.operazione.Equals( "Salvataggio Bonus 100" ) ).ToList( );
                }

                if ( azioni != null && azioni.Any( ) )
                {
                    foreach ( var a in azioni )
                    {

                        elenco.Add( new DettaglioSceltaDipendente( )
                        {
                            Matricola = a.matricola ,
                            DataScelta = a.data ,
                            Scelta = a.descrizione_operazione ,
                            Nominativo = GetUtenteByMatricola( a.matricola )
                        } );
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return elenco;
        }

        #endregion


        #region Public
        public ActionResult Index ( )
        {
            return View( );
        }

        public ActionResult GetReport()
        {
            List<DettaglioSceltaDipendente> model = new List<DettaglioSceltaDipendente>( );

            model = GetElenco( );

            return View( "~/Views/GestioneBonus/subpartial/_listaDipendenti.cshtml" , model );
        }

        #endregion

    }
}