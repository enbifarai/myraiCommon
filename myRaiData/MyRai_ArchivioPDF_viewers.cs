//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData
{
    using System;
    using System.Collections.Generic;
    
    public partial class MyRai_ArchivioPDF_viewers
    {
        public int id { get; set; }
        public int id_archivio_pdf { get; set; }
        public string ip { get; set; }
        public string useragent { get; set; }
        public System.DateTime data { get; set; }
        public string matricola { get; set; }
    
        public virtual DIGIRESP_Archivio_PDF DIGIRESP_Archivio_PDF { get; set; }
    }
}
