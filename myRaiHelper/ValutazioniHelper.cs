using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using myRaiDataTalentia;
using myRaiServiceHub.Autorizzazioni;
using System.Data;

namespace myRaiHelper
{
    public static class ValutazioniExtension
    {
        #region XR_VAL_EVALUATION
        public static Expression<Func<XR_VAL_EVALUATION, bool>> ExprFuncValidEvaluation()
        {
            /*
             * - Controllo validità evaluator
             * - controllo validità scheda valutazione
             * - Controllo validità campagna
             */

            return x => x.VALID_DTA_INI <= DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END >= DateTime.Now)
                    && x.XR_VAL_EVALUATOR.VALID_DTA_INI<=DateTime.Now && (x.XR_VAL_EVALUATOR.VALID_DTA_END == null || x.XR_VAL_EVALUATOR.VALID_DTA_END >= DateTime.Now)
                    && x.XR_VAL_CAMPAIGN_SHEET.VALID_DTA_INI <= DateTime.Now && (x.XR_VAL_CAMPAIGN_SHEET.VALID_DTA_END == null || x.XR_VAL_CAMPAIGN_SHEET.VALID_DTA_END >= DateTime.Now)
                    && x.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN.VALID_DTA_INI <= DateTime.Now && (x.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN.VALID_DTA_END == null || x.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN.VALID_DTA_END >= DateTime.Now);
        }
        #endregion

        #region XR_VAL_EVALUATOR
        public static Expression<Func<XR_VAL_EVALUATOR, bool>> ExprFuncValidValuator()
        {
            /*
             * - Controllo validità evalator
             * - controllo validità scheda valutazione
             * - Controllo validità campagna
             */

            return x => x.VALID_DTA_INI <= DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END >= DateTime.Now)
                    && x.XR_VAL_CAMPAIGN_SHEET.VALID_DTA_INI <= DateTime.Now && (x.XR_VAL_CAMPAIGN_SHEET.VALID_DTA_END == null || x.XR_VAL_CAMPAIGN_SHEET.VALID_DTA_END >= DateTime.Now)
                    && x.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN.VALID_DTA_INI <= DateTime.Now && (x.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN.VALID_DTA_END == null || x.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN.VALID_DTA_END >= DateTime.Now);
        }

        public static Expression<Func<XR_VAL_EVALUATOR, bool>> ExprSubFuncValidValuator()
        {
            /*
             * - Controllo validità evaluator
             * - controllo validità scheda valutazione
             * - Controllo validità campagna
             */


            return x => (x.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN.VALID_DTA_INI <= DateTime.Now && (x.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN.VALID_DTA_END == null || x.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN.VALID_DTA_END >= DateTime.Now))
                        && (x.XR_VAL_CAMPAIGN_SHEET.VALID_DTA_INI <= DateTime.Now && (x.XR_VAL_CAMPAIGN_SHEET.VALID_DTA_END == null || x.XR_VAL_CAMPAIGN_SHEET.VALID_DTA_END >= DateTime.Now))
                        && (x.VALID_DTA_INI <= DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END >= DateTime.Now));
        }
        public static Expression<Func<XR_VAL_EVALUATOR, bool>> ExprFuncEvalutatorWithActiveCampaign()
        {
            return x => x.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN.DTA_START <= DateTime.Now && x.XR_VAL_CAMPAIGN_SHEET.XR_VAL_CAMPAIGN.DTA_END >= DateTime.Now;
        }

        public static bool IsDelegate(this XR_VAL_EVALUATOR x)
        {
            return x.DELEGATO != null && x.DELEGATO.Count() > 0 && x.DELEGATO.Where(y => x.VALID_DTA_END == null || y.VALID_DTA_END > DateTime.Now).Where(y=> y.DTA_START < DateTime.Now && (y.DTA_END == null || y.DTA_END > DateTime.Now)).Any();
        }

        public static bool IsExternalEvaluator(this XR_VAL_EVALUATOR x)
        {
            return x.EXT_ROLE_ASSIGNED != null && x.EXT_ROLE_ASSIGNED.Count() > 0 && x.EXT_ROLE_ASSIGNED.Where(y => y.VALID_DTA_END == null || y.VALID_DTA_END > DateTime.Now).Any();
        }
        public static bool IsDelegator(this XR_VAL_EVALUATOR x)
        {
            return x.DELEGANTE != null && x.DELEGANTE.Where(y => y.VALID_DTA_END == null || y.VALID_DTA_END > DateTime.Now).Where(y => y.DTA_START < DateTime.Now && (y.DTA_END == null || y.DTA_END > DateTime.Now)).Any();
        }
        #endregion


    }


    public class ValutazioniHelper
    {
        private const string VALUTAZ_SESSION_AUTH = "ValutazioniAuth";

        public static string GetValAbilFunc()
        {
            string func = HrisHelper.GetParametro<string>(HrisParam.ValAbilFunc);
            if (String.IsNullOrWhiteSpace(func))
                func = "VALUTAZIONI";

            return func;
        }

        public static bool IsEnabled(string matricola)
        {
            bool isEnabled = false;

            IncentiviEntities db = new IncentiviEntities();

            var tmp = db.XR_VAL_EVALUATOR.Where(ValutazioniExtension.ExprFuncValidValuator());

            isEnabled = tmp.Any(x=>x.SINTESI1.COD_MATLIBROMAT==matricola);

            if (!isEnabled)
            {
                List<string> subMatr = new List<string>();
                TalentiaEntities dbTal = new TalentiaEntities();
                var listIncarichi = dbTal.XR_STR_TINCARICO.Where(x => x.matricola == matricola).Select(y => y.id_sezione);
                if (listIncarichi != null && listIncarichi.Count() > 0)
                {
                    foreach (var str in listIncarichi)
                    {
                        var subStrList = dbTal.XR_STR_TALBERO.Where(x => x.subordinato_a == str).Select(x => x.id);
                        if (subStrList != null && subStrList.Count() > 0)
                            subMatr.AddRange(dbTal.XR_STR_TINCARICO.Where(x => subStrList.Contains(x.id_sezione)).Select(y => y.matricola));
                    }
                }
                

                isEnabled = tmp.Any(x => subMatr.Contains(x.SINTESI1.COD_MATLIBROMAT));
            }

            return isEnabled;
        }

        public static bool IsEnabledGest(string matricola)
        {
            return AuthHelper.EnabledToAnySubFunc(matricola, GetValAbilFunc(), "ADM", "GEST");
        }

        public static bool GetParametro(string chiave, out XR_VAL_PARAM param)
        {
            bool found = false;
            param = null;
            using (IncentiviEntities db = new IncentiviEntities())
            {
                param = db.XR_VAL_PARAM.FirstOrDefault(x => x.CHIAVE == chiave);
                found = param != null;
            }
            return found;
        }
    }
}
