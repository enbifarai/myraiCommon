﻿//------------------------------------------------------------------------------
// <auto-generated>
//    Codice generato da un modello.
//
//    Le modifiche manuali a questo file potrebbero causare un comportamento imprevisto dell'applicazione.
//    Se il codice viene rigenerato, le modifiche manuali al file verranno sovrascritte.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CorsiJobEntities : DbContext
    {
        public CorsiJobEntities()
            : base("name=CorsiJobEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<tbCorsiCodice> tbCorsiCodice { get; set; }
        public DbSet<tblJobPosting> tblJobPosting { get; set; }
        public DbSet<tblRichiesteJobpostingRegisti> tblRichiesteJobpostingRegisti { get; set; }
        public DbSet<tblPartecipantiOnline> tblPartecipantiOnline { get; set; }
    }
}