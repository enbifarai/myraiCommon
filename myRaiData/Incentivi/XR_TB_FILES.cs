//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData.Incentivi
{
    using System;
    using System.Collections.Generic;
    
    public partial class XR_TB_FILES
    {
        public XR_TB_FILES()
        {
            this.XR_TB_ASSOCIATIVEFILES = new HashSet<XR_TB_ASSOCIATIVEFILES>();
        }
    
        public int Id { get; set; }
        public string NomeFile { get; set; }
        public string MimeType { get; set; }
        public int Length { get; set; }
        public byte[] DataCreazione { get; set; }
        public string Tipologia { get; set; }
        public byte[] ContentByte { get; set; }
        public bool Attivo { get; set; }
        public string MatricolaCreatore { get; set; }
        public bool Firmato { get; set; }
    
        public virtual ICollection<XR_TB_ASSOCIATIVEFILES> XR_TB_ASSOCIATIVEFILES { get; set; }
        public virtual XR_TB_TIPOLOGIEFILES XR_TB_TIPOLOGIEFILES { get; set; }
    }
}
