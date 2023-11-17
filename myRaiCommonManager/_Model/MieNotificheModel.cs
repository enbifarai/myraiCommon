using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class MieNotificheModel
    {
        public int? tipo { get; set; }
        public List<NotificaPlus> Notifiche { get; set; }
    }
    public class NotificaPlus
    {
        public myRaiData.MyRai_Notifiche notifica { get; set; }
        public myRaiData.MyRai_Richieste richiesta { get; set; }
        public bool ShowDetail { get; set; }
        public NotificaDettaglio Dettaglio { get; set; }
    }
    public class NotificaDettaglio
    {
        public string Title { get; set; }
        public string Note { get; set; }
        public bool ShowButton { get; set; }
        public string AnchorText { get; set; }
        public string AnchorHref { get; set; }
    }
}