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
    
    public partial class XR_ALLEGATI
    {
        public XR_ALLEGATI()
        {
            this.XR_DEM_ALLEGATI_VERSIONI = new HashSet<XR_DEM_ALLEGATI_VERSIONI>();
        }
    
        public int Id { get; set; }
        public string NomeFile { get; set; }
        public byte[] ContentByte { get; set; }
        public string MimeType { get; set; }
        public int Length { get; set; }
        public string PosizioneProtocollo { get; set; }
        public bool IsPrincipal { get; set; }
        public byte[] ContentBytePDF { get; set; }
        public bool GiaConvertito { get; set; }
        public int TipoFile { get; set; }
        public string PosizioneUltimaSigla { get; set; }
        public Nullable<bool> DaSiglare { get; set; }
    
        public virtual ICollection<XR_DEM_ALLEGATI_VERSIONI> XR_DEM_ALLEGATI_VERSIONI { get; set; }
    }
}