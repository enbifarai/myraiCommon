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
    
    public partial class MyRai_InvioMail_Allegati
    {
        public int Id { get; set; }
        public int IdMail { get; set; }
        public string NomeFile { get; set; }
        public string TipoFile { get; set; }
        public byte[] ContentByte { get; set; }
    
        public virtual MyRai_InvioMail MyRai_InvioMail { get; set; }
    }
}
