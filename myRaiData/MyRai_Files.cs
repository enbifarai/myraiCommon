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
    
    public partial class MyRai_Files
    {
        public int Id { get; set; }
        public string Chiave { get; set; }
        public string MatricolaCreatore { get; set; }
        public System.DateTime DataCreazione { get; set; }
        public string Tipologia { get; set; }
        public string NomeFile { get; set; }
        public byte[] ContentByte { get; set; }
        public string MimeType { get; set; }
        public int Length { get; set; }
        public string Password { get; set; }
        public string Json { get; set; }
        public bool Attivo { get; set; }
    
        public virtual MyRai_TipologieFiles MyRai_TipologieFiles { get; set; }
    }
}
