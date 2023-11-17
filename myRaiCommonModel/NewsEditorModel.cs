using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public class NewsEditorModel
    {
        public NewsEditorModel()
        {
            NewsList = new List<myRaiData.MyRai_News>();
        }
        public List<myRaiData.MyRai_News> NewsList { get; set; }
    }
    public partial class MyRai_News_Shadow
    {

        public MyRai_News_Shadow ( )
        {
            destinatari_any = true;
            List<SelectListItem> list = new List<SelectListItem>( );
            list.Add( new SelectListItem( ) { Text = "Priorità bassa" , Value = "1" } );
            list.Add( new SelectListItem( ) { Text = "Priorità media" , Value = "2" } );
            list.Add( new SelectListItem( ) { Text = "Priorità alta" , Value = "3" } );
            priorita_list = new SelectList( list , "Value" , "Text" );
        }
        public int id { get; set; }
        public Nullable<System.DateTime> data_info { get; set; }
        public bool destinatari_any { get; set; }
        public string destinatari_matricole { get; set; }
        public string destinatari_tipodip { get; set; }
        public Nullable<bool> destinatari_L1 { get; set; }
        public Nullable<bool> destinatari_L2 { get; set; }
        public string destinatari_sedigapp { get; set; }
        public string contenuto { get; set; }
        public int priorita { get; set; }
        //public string Categoria { get; set; }
        public string controllo_aggiuntivo { get; set; }
        public string validita_inizio { get; set; }
        public string validita_fine { get; set; }
        public string titolo { get; set; }
        public string categoria{ get; set; }
        public int tipodest { get; set; }
        public SelectList priorita_list { get; set; }

    }
}