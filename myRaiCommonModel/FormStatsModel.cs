using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public class FormStatsModel
    {
        public FormStatsModel()
        {
            this.forms = new List<myRaiData.MyRai_FormPrimario>();
            this.searchModel = new FormSearchModel();
        }
        public List<myRaiData.MyRai_FormPrimario> forms { get; set; }
        public FormSearchModel searchModel { get; set; }
    }

    public class FormSearchModel
    {
        public FormSearchModel()
        {
            var db = new myRaiData.digiGappEntities();
            var list = new List<SelectListItem>();
            foreach (var item in db.MyRai_FormTipologiaForm)
            {
                list.Add(new SelectListItem()
                {
                    Selected = false,
                    Text = item.tipologia,
                    Value = item.id.ToString()
                });
            }
            this.id_tipologia_list = new SelectList(list, "Value", "Text");
        }


        public string nome { get; set; }
        public int? id_tipologia { get; set; }
        public SelectList id_tipologia_list { get; set; }

    }

    public class FormStatisticsModel
    {
        public FormStatisticsModel()
        {
            items = new List<DatoDomanda>();
            Sentiments = new List<myRaiData.MyRai_FormRisposteDate_Sentiment>();
        }
        public myRaiData.MyRai_FormPrimario formprimario { get; set; }
        public List<DatoDomanda> items { get; set; }
        public string Titolo { get; set; }

        public List<myRaiData.MyRai_FormRisposteDate_Sentiment> Sentiments { get; set; }
    }

    public class DatoDomanda
    {
        public int IdDomanda { get; set; }
        public double ValoreMedio { get; set; }
        public List<PieItem> PieItems { get; set; }
        public myRaiData.MyRai_FormDomande domanda { get; set; }
        public string JsonPieItems { get; set; }

        public bool HasSentiment { get; set; }
        public bool HasOtherCollapse { get; set; }
    }

    public class PieItem
    {
        public PieItem()
        {
            data = new List<List<int>>();
        }
        public string label { get; set; }
        public object labelObj { get; set; }
        public List<List<int>> data { get; set; }
        public string color { get; set; }
    }
     
}