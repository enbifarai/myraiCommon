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
    
    public partial class MyRai_HeaderMenu
    {
        public MyRai_HeaderMenu()
        {
            this.MyRai_HeaderMenu1 = new HashSet<MyRai_HeaderMenu>();
        }
    
        public int Id { get; set; }
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public string Title { get; set; }
        public int Posizione { get; set; }
        public Nullable<int> IdParent { get; set; }
        public string Link { get; set; }
        public string Action { get; set; }
        public bool ExtendedModeEnabled { get; set; }
        public bool MinimizedModeEnabled { get; set; }
        public string Contesto { get; set; }
    
        public virtual ICollection<MyRai_HeaderMenu> MyRai_HeaderMenu1 { get; set; }
        public virtual MyRai_HeaderMenu MyRai_HeaderMenu2 { get; set; }
    }
}