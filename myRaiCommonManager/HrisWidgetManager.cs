using myRai.DataAccess;
using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiDataTalentia;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace myRaiCommonManager
{
    public class HrisWidgetManager
    {
        public static List<HrisWidget> GetGestioneWidget(string sezione = "")
        {
            List<HrisWidget> result = new List<HrisWidget>();
            int idPersona = CommonHelper.GetCurrentIdPersona();

            var db = new IncentiviEntities();
            var selezionati = db.XR_HRIS_WIDGET_PERS.Where(x => x.ID_PERSONA == idPersona).Select(x => x.XR_HRIS_WIDGET.ID_WIDGET).ToList();

            var list = db.XR_HRIS_WIDGET.Where(x => x.IND_ATTIVO);
            if (!String.IsNullOrWhiteSpace(sezione))
                list = list.Where(x => x.COD_SEZIONE != null && (x.COD_SEZIONE == "*" || x.COD_SEZIONE.Contains(sezione)));

            var tmp = ControlloAbilitazioniWidget(list.ToList().Select(x => new HrisWidget(x)).ToList());

            foreach (var widget in tmp)
            {
                //HrisWidget widget = new HrisWidget(item);
                widget.Selezionato = selezionati.Contains(widget.ID_WIDGET);
                result.Add(widget);
            }

            return result;
        }

        public static List<HrisWidget> GetWidgetSelezionati(string tipologia = "", string sezione = "")
        {
            List<HrisWidget> result = new List<HrisWidget>();
            int idPersona = CommonHelper.GetCurrentIdPersona();

            var db = new IncentiviEntities();

            var qry = db.XR_HRIS_WIDGET_PERS
                              .Where(x => x.ID_PERSONA == idPersona && x.XR_HRIS_WIDGET.IND_ATTIVO)
                              .Select(x => x.XR_HRIS_WIDGET);

            if (!String.IsNullOrWhiteSpace(tipologia))
                qry = qry.Where(x => x.COD_TIPOLOGIA == tipologia);

            if (!String.IsNullOrWhiteSpace(sezione))
                qry = qry.Where(x => x.COD_SEZIONE != null && (x.COD_SEZIONE == "*" || x.COD_SEZIONE.Contains(sezione)));

            result.AddRange(qry.ToList().Select(x => new HrisWidget(x)));

            return ControlloAbilitazioniWidget(result);
        }

        private static List<HrisWidget> ControlloAbilitazioniWidget(List<HrisWidget> widget)
        {
            if (widget.Any())
            {
                string matricola = CommonHelper.GetCurrentUserMatricola();

                List<string> subFunc = AuthHelper.EnabledSubFunc(matricola, "HRIS");

                if (widget.Any(x => x.COD_ABIL == "AMMIN" && (!subFunc.Any(e => (e == "01GEST" || e == "01ADM")))))
                    widget.RemoveWhere(x => x.COD_ABIL == "AMMIN");

                if (widget.Any(x => x.COD_ABIL == "AMMI" && (!subFunc.Any(e => (e == "01RES")))))
                    widget.RemoveWhere(x => x.COD_ABIL == "AMMI");

                widget.RemoveWhere(x => !x.ExtraParam.IsEnabled(matricola));
            }

            return widget;
        }
        public static string GetSWAbilFunc()
        {
            string func = HrisHelper.GetParametro<string>(HrisParam.SWAbilFunc);
            if (String.IsNullOrWhiteSpace(func))
                func = "HRIS_SW";

            return func;
        }
        public static void GetData(HrisWidgetDataParam widgetData, out int cifraPrincipale, out int cifraSecondaria)
        {
            cifraPrincipale = 0;
            cifraSecondaria = 0;

            if (widgetData != null && widgetData.QueryParam != null && widgetData.QueryParam.ToUpper().Contains("XR_SW_API"))
            {
                var db = new TalentiaEntities();
                var dbInc = new IncentiviEntities();
                var res = dbInc.Database.SqlQuery<string>(widgetData.QueryParam).ToList();
                var tmp = db.SINTESI1
                        .Include("XR_STATO_RAPPORTO").Include("XR_STATO_RAPPORTO.XR_STATO_RAPPORTO_INFO")
                        .Include("QUALIFICA").Include("QUALIFICA.TB_QUALSTD")
                        .AsQueryable();
                tmp = tmp.Where(x => x.COD_MATLIBROMAT != null && x.DTA_FINE_CR != null && x.DTA_FINE_CR.Value >= DateTime.Today);

                tmp = AuthHelper.SintesiFilter(tmp, CommonHelper.GetCurrentUserMatricola(), "", GetSWAbilFunc());
                var tot = tmp.Select(x => x.COD_MATLIBROMAT).Distinct().ToList();
                cifraPrincipale = res.Select(x => tot.Contains(x)).Count();

                //var tmp2 = db.SINTESI1
                //       .Include("XR_STATO_RAPPORTO").Include("XR_STATO_RAPPORTO.XR_STATO_RAPPORTO_INFO")
                //       .Include("QUALIFICA").Include("QUALIFICA.TB_QUALSTD")
                //       .AsQueryable();

                //tmp2 = AuthHelper.SintesiFilter(tmp2, "103650", "", GetSWAbilFunc());
                //var tot2 = tmp2.Select(x => x.COD_MATLIBROMAT).Distinct().ToList();
                
                return;
                

            }
            
            if (widgetData != null)
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(widgetData.FuncOneParam))
                    {

                    }
                    else if (!String.IsNullOrWhiteSpace(widgetData.FuncTwoParam))
                    {
                        var t = typeof(IncentivazioneHelper).GetMethod(widgetData.FuncTwoParam);
                        Type c = CezanneHelper.GetTypeByName(widgetData.BaseType);
                        if (c != null)
                        {
                            var method = c.GetMethod(widgetData.FuncTwoParam);
                            if (method != null)
                            {
                                object[] args = { cifraPrincipale, cifraSecondaria };
                                method.Invoke(null, args);
                                cifraPrincipale = (int)args[0];
                                cifraSecondaria = (int)args[1];
                            }
                        }
                    }
                    else if (!String.IsNullOrWhiteSpace(widgetData.FuncListParam))
                    {

                        Type c = CezanneHelper.GetTypeByName(widgetData.BaseType);
                        if (c != null)
                        {
                            var method = c.GetMethod(widgetData.FuncListParam);
                            if (method != null)
                            {
                                object[] parameters = null;
                                var paramInfo = method.GetParameters();
                                if (paramInfo.Length > 0 && paramInfo.All(x => x.IsOptional))
                                    parameters = paramInfo.Select(x => x.DefaultValue).ToArray();
                                object res = method.Invoke(null, parameters);
                                if (res != null)
                                    cifraPrincipale = (int)res.GetType().GetMethod("get_Count").Invoke(res, null);

                            }
                        }
                    }
                    else if (!String.IsNullOrWhiteSpace(widgetData.QueryParam))
                    {
                        var db = new myRaiData.Incentivi.IncentiviEntities();
                        var res = db.Database.SqlQuery<HrisWidgetData>(widgetData.QueryParam);
                        if (res != null && res.Any())
                        {
                            cifraPrincipale = res.First().CifraPrincipale.GetValueOrDefault();
                            cifraSecondaria = res.First().CifraSecondaria.GetValueOrDefault();
                        }
                    }
                }
                catch (Exception)
                {

                }

            }
        }

        public static bool Save_GestioneWidget(string[] codWidget, out string errorMsg)
        {
            bool result = false;
            errorMsg = null;

            var idPers = CommonHelper.GetCurrentIdPersona();
            var db = new IncentiviEntities();
            if (codWidget == null)
                db.XR_HRIS_WIDGET_PERS.RemoveWhere(x => x.ID_PERSONA == idPers);
            else
                db.XR_HRIS_WIDGET_PERS.RemoveWhere(x => x.ID_PERSONA == idPers && !codWidget.Contains(x.XR_HRIS_WIDGET.COD_WIDGET));

            if (codWidget != null)
            {
                foreach (var item in db.XR_HRIS_WIDGET.Where(x => codWidget.Contains(x.COD_WIDGET) && !x.XR_HRIS_WIDGET_PERS.Any(y => y.ID_PERSONA == idPers)))
                {
                    CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tmsTimestamp);
                    db.XR_HRIS_WIDGET_PERS.Add(new XR_HRIS_WIDGET_PERS()
                    {
                        ID_WIDGET_PERS = db.XR_HRIS_WIDGET_PERS.GeneraPrimaryKey(),
                        ID_PERSONA = idPers,
                        ID_WIDGET = item.ID_WIDGET,
                        NMR_ORDINE = 0,
                        COD_USER = codUser,
                        COD_TERMID = codTermid,
                        TMS_TIMESTAMP = tmsTimestamp
                    });
                }
            }

            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "Salvataggio widget");
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            return result;
        }
    }
}
