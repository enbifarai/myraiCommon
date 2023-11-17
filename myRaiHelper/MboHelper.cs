using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiHelper
{
    public class MboHelper
    {
        public enum MboAbilType
        {
            Generic,
            PrimoRiporto,
            SecondoRiporto
        }

        public static bool IsEnabled(MboAbilType abilType = MboAbilType.Generic)
        {
            bool isEnabled = false;
            if (!CommonHelper.IsProduzione() && !System.Diagnostics.Debugger.IsAttached)
                return isEnabled;

            var dbTal = new IncentiviEntities();
            var recAbil = dbTal.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "MboAbilitazione");
            if (recAbil.COD_VALUE1 == "TRUE" || (recAbil.COD_VALUE1 == "LIMITED" && recAbil.COD_VALUE2.Contains(CommonHelper.GetCurrentRealUsername())))
            {
                var db = new IncentiviEntities();
                int idPers = CommonHelper.GetCurrentIdPersona();

                IQueryable<XR_MBO_SCHEDA> tmplist = db.XR_MBO_SCHEDA.Where(x => x.XR_MBO_INIZIATIVA.VALID_DTA_END == null && x.VALID_DTA_END == null);

                switch (abilType)
                {
                    case MboAbilType.Generic:
                        isEnabled = tmplist.Any(x => (x.ID_PERSONA_RESP == idPers || x.ID_PERS_RIPORTO == idPers || x.ID_PERSONA_CONSUNTIVAZIONE == idPers));
                        break;
                    case MboAbilType.PrimoRiporto:
                        isEnabled = tmplist.Any(x => x.ID_PERSONA_RESP == idPers);
                        break;
                    case MboAbilType.SecondoRiporto:
                        isEnabled = tmplist.Any(x => x.ID_PERS_RIPORTO == idPers);
                        break;
                    default:
                        break;
                }
            }

            return isEnabled;
        }
    }
}
