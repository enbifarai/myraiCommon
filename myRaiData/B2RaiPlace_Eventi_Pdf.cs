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
    
    public partial class B2RaiPlace_Eventi_Pdf
    {
        public int id { get; set; }
        public Nullable<int> id_evento { get; set; }
        public Nullable<int> npenotazioni { get; set; }
        public byte[] pdf { get; set; }
    
        public virtual B2RaiPlace_Eventi_Evento B2RaiPlace_Eventi_Evento { get; set; }
    }
}
