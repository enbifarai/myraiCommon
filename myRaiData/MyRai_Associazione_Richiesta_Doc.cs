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
    
    public partial class MyRai_Associazione_Richiesta_Doc
    {
        public int id { get; set; }
        public int id_richiesta { get; set; }
        public int id_documento { get; set; }
    
        public virtual MyRai_DocumentiDipendente MyRai_DocumentiDipendente { get; set; }
        public virtual MyRai_Richieste MyRai_Richieste { get; set; }
    }
}
