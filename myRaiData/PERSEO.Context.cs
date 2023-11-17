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
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    
    public partial class PERSEOEntities : DbContext
    {
        public PERSEOEntities()
            : base("name=PERSEOEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<VHRDWLIV1_ECCEZ_BORSESTUDIO> VHRDWLIV1_ECCEZ_BORSESTUDIO { get; set; }
        public DbSet<VHRDWLIV1_TELEFONO_AZIENDALE> VHRDWLIV1_TELEFONO_AZIENDALE { get; set; }
        public DbSet<TAlbero> TAlbero { get; set; }
        public DbSet<TSezione> TSezione { get; set; }
        public DbSet<TMission> TMission { get; set; }
        public DbSet<DSedeContabile> DSedeContabile { get; set; }
        public DbSet<DServizio> DServizio { get; set; }
        public DbSet<TDipendenti> TDipendenti { get; set; }
        public DbSet<DCategoria> DCategoria { get; set; }
        public DbSet<DMansione> DMansione { get; set; }
        public DbSet<TIncarico> TIncarico { get; set; }
        public DbSet<DIncarico> DIncarico { get; set; }
        public DbSet<SEDE2> SEDE2 { get; set; }
        public DbSet<QUALIFICA2> QUALIFICA2 { get; set; }
        public DbSet<RUOLO2> RUOLO2 { get; set; }
        public DbSet<SINTESI12> SINTESI12 { get; set; }
        public DbSet<TSVIntervScheda> TSVIntervScheda { get; set; }
        public DbSet<DFiguraPro> DFiguraPro { get; set; }
        public DbSet<DUorgEsperProd> DUorgEsperProd { get; set; }
        public DbSet<TSVAnagCom> TSVAnagCom { get; set; }
        public DbSet<TSVEsperProd> TSVEsperProd { get; set; }
        public DbSet<TSVRuoliEsperProd> TSVRuoliEsperProd { get; set; }
        public DbSet<DConProf> DConProf { get; set; }
        public DbSet<TAbilitazione> TAbilitazione { get; set; }
    
        public virtual ObjectResult<sp_RicercaUtentiServizio_Result> sp_RicercaUtentiServizio(string inputTerm, string inputServ)
        {
            var inputTermParameter = inputTerm != null ?
                new ObjectParameter("inputTerm", inputTerm) :
                new ObjectParameter("inputTerm", typeof(string));
    
            var inputServParameter = inputServ != null ?
                new ObjectParameter("inputServ", inputServ) :
                new ObjectParameter("inputServ", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_RicercaUtentiServizio_Result>("sp_RicercaUtentiServizio", inputTermParameter, inputServParameter);
        }
    
        public virtual ObjectResult<string> sp_GERARSERVIZIO(string sEZIONE)
        {
            var sEZIONEParameter = sEZIONE != null ?
                new ObjectParameter("SEZIONE", sEZIONE) :
                new ObjectParameter("SEZIONE", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("sp_GERARSERVIZIO", sEZIONEParameter);
        }
    
        public virtual ObjectResult<string> sp_GERARSEZIONE(string sEZIONE)
        {
            var sEZIONEParameter = sEZIONE != null ?
                new ObjectParameter("SEZIONE", sEZIONE) :
                new ObjectParameter("SEZIONE", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("sp_GERARSEZIONE", sEZIONEParameter);
        }
    }
}
