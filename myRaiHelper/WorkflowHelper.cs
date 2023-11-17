using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace myRaiHelper
{
    public class WorkflowHelper
    {
        public static Expression<Func<XR_WKF_RICHIESTE, bool>> IsCurrentState(params int[] stato)
        {
            return x => stato.Contains(x.XR_WKF_OPERSTATI_GENERIC.Where(y => !y.VALID_DTA_END.HasValue)
                            .Select(w => w.XR_WKF_STATI)
                            .OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                            .Select(b => b.ID_STATO)
                            .FirstOrDefault());
        }
        public static Expression<Func<XR_WKF_RICHIESTE, bool>> NotIsCurrentState(params int[] stato)
        {
            return x => !stato.Contains(x.XR_WKF_OPERSTATI_GENERIC.Where(y => y.ID_STATO != 0 && !y.VALID_DTA_END.HasValue)
                            .Select(w => w.XR_WKF_STATI)
                            .OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                            .Select(b => b.ID_STATO)
                            .FirstOrDefault());
        }
        public static void AddStato(IncentiviEntities db, XR_WKF_RICHIESTE richiesta, int idStato)
        {
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);
            int tipologia = richiesta.ID_TIPOLOGIA;
            var stato = db.XR_WKF_STATI.FirstOrDefault(x => x.ID_WKF_TIPOLOGIA == tipologia && x.ID_STATO == idStato);

            XR_WKF_OPERSTATI_GENERIC oper = new XR_WKF_OPERSTATI_GENERIC()
            {
                COD_TIPO_PRATICA = richiesta.COD_TIPO,
                ID_PERSONA = CommonHelper.GetCurrentIdPersona(),
                DTA_OPERAZIONE = 0,
                VALID_DTA_INI = tms,
                COD_USER = codUser,
                COD_TERMID = codTermid,
                TMS_TIMESTAMP = tms,
                XR_WKF_STATI = stato,
                XR_WKF_RICHIESTE = richiesta
            };
            db.XR_WKF_OPERSTATI_GENERIC.Add(oper);
        }

        public static void AddStatoBis(IncentiviEntities db, XR_WKF_RICHIESTE richiesta, int idStato)
        {
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);
            int tipologia = richiesta.ID_TIPOLOGIA;
            var stato = db.XR_WKF_STATI.FirstOrDefault(x => x.ID_WKF_TIPOLOGIA == tipologia && x.ID_STATO == idStato);

            XR_WKF_OPERSTATI_GENERIC oper = new XR_WKF_OPERSTATI_GENERIC()
            {
                COD_TIPO_PRATICA = richiesta.COD_TIPO,
                ID_PERSONA = CommonHelper.GetCurrentIdPersona(),
                DTA_OPERAZIONE = 0,
                VALID_DTA_INI = tms,
                COD_USER = codUser,
                COD_TERMID = codTermid,
                TMS_TIMESTAMP = tms,
                XR_WKF_STATI = stato,
                XR_WKF_RICHIESTE = richiesta,
                NOMINATIVO = CezanneHelper.GetNominativoByMatricola(CommonHelper.GetCurrentUserMatricola())
            };
            richiesta.XR_WKF_OPERSTATI_GENERIC.Add(oper);
        }

    }
}
