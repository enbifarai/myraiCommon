using myRaiCommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonManager
{
    public class NewsManager
    {
        public static NewsEditorModel GetNewsEditorModel()
        {
            NewsEditorModel model = new NewsEditorModel();
            var db = new myRaiData.digiGappEntities();
            model.NewsList = db.MyRai_News
                .OrderByDescending(x=>x.priorita)
                .ThenByDescending(x => x.data_info).ToList();
            return model;                
        }
    }
}