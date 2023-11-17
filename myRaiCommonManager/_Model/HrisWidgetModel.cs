using myRaiDataTalentia;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class HrisWidgetData
    {
        public int? CifraPrincipale { get; set; }
        public int? CifraSecondaria { get; set; }
    }
    public class HrisWidgetParam
    {
        public bool CheckAbilFunction { get; set; }
        public string AbilFunction { get; set; }
        public string[] AbilSubfunction { get; set; }
        public AlertModel WidgetModel { get; set; }
        public HrisWidgetDataParam Data { get; set; }
        public bool IsEnabled(string matricola)
        {
            bool enabled = false;

            if (CheckAbilFunction)
            {
                if (AuthHelper.EnabledTo(matricola, AbilFunction))
                {
                    if (AbilSubfunction == null)
                        enabled = true;
                    else
                        enabled = AuthHelper.EnabledToAnySubFunc(matricola, AbilFunction, AbilSubfunction);
                }
            }
            else
                enabled = true;

            return enabled;
        }
    }

    public class HrisWidgetDataParam
    {
        public string BaseType { get; set; }
        public string FuncOneParam { get; set; }
        public string FuncTwoParam { get; set; }
        public string FuncListParam { get; set; }
        public string QueryParam { get; set; }
    }

    public class HrisWidget : XR_HRIS_WIDGET
    {
        public bool Selezionato { get; set; }

        public HrisWidgetParam ExtraParam { get; set; }

        public HrisWidget(XR_HRIS_WIDGET from) : base()
        {
            this.COD_ABIL = from.COD_ABIL;
            this.COD_GRUPPO = from.COD_GRUPPO;
            this.COD_TERMID = from.COD_TERMID;
            this.COD_TIPOLOGIA = from.COD_TIPOLOGIA;
            this.COD_USER = from.COD_USER;
            this.COD_WIDGET = from.COD_WIDGET;
            this.DES_WIDGET = from.DES_WIDGET;
            this.ID_WIDGET = from.ID_WIDGET;
            this.IND_ATTIVO = from.IND_ATTIVO;
            this.NME_WIDGET = from.NME_WIDGET;
            this.COD_SEZIONE = from.COD_SEZIONE;
            this.TMS_TIMESTAMP = from.TMS_TIMESTAMP;

            try
            {
                if (!String.IsNullOrWhiteSpace(from.PARAMETRI))
                    this.ExtraParam = Newtonsoft.Json.JsonConvert.DeserializeObject<HrisWidgetParam>(from.PARAMETRI);
                else
                    this.ExtraParam = new HrisWidgetParam();
            }
            catch (Exception)
            {
                this.ExtraParam = new HrisWidgetParam();
            }

        }
    }
}
