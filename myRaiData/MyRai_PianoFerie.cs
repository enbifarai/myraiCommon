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
    
    public partial class MyRai_PianoFerie
    {
        public int id { get; set; }
        public int anno { get; set; }
        public string matricola { get; set; }
        public Nullable<System.DateTime> data_consolidato { get; set; }
        public Nullable<System.DateTime> data_approvato { get; set; }
        public string approvatore { get; set; }
        public string nota_responsabile { get; set; }
        public string nota_matricola { get; set; }
        public Nullable<System.DateTime> nota_data { get; set; }
        public string sedegapp { get; set; }
        public string nota_invio_segreteria { get; set; }
        public Nullable<System.DateTime> data_invio_segreteria { get; set; }
        public string matricola_invio_segreteria { get; set; }
        public Nullable<int> Id_pdf_pianoferie_inclusa { get; set; }
    }
}
