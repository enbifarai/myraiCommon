﻿//------------------------------------------------------------------------------
// <auto-generated>
//    Codice generato da un modello.
//
//    Le modifiche manuali a questo file potrebbero causare un comportamento imprevisto dell'applicazione.
//    Se il codice viene rigenerato, le modifiche manuali al file verranno sovrascritte.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData.CurriculumVitae
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    
    public partial class cv_ModelEntities : DbContext
    {
        public cv_ModelEntities()
            : base("name=cv_ModelEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<DAlboProf> DAlboProf { get; set; }
        public DbSet<DAreaGeoGio> DAreaGeoGio { get; set; }
        public DbSet<DAreaOrg> DAreaOrg { get; set; }
        public DbSet<DAreaOrgServ> DAreaOrgServ { get; set; }
        public DbSet<DAutovalutLingua> DAutovalutLingua { get; set; }
        public DbSet<DCompDigit> DCompDigit { get; set; }
        public DbSet<DCompDigitLiv> DCompDigitLiv { get; set; }
        public DbSet<DConInfo> DConInfo { get; set; }
        public DbSet<DConInfoLiv> DConInfoLiv { get; set; }
        public DbSet<DContinente> DContinente { get; set; }
        public DbSet<DFiguraPro> DFiguraPro { get; set; }
        public DbSet<DGruppoConInfo> DGruppoConInfo { get; set; }
        public DbSet<DLinguaLiv> DLinguaLiv { get; set; }
        public DbSet<DNazione> DNazione { get; set; }
        public DbSet<DRedazione> DRedazione { get; set; }
        public DbSet<DTipoDispo> DTipoDispo { get; set; }
        public DbSet<DTipoPatente> DTipoPatente { get; set; }
        public DbSet<DTipoTitolo> DTipoTitolo { get; set; }
        public DbSet<DTitolo> DTitolo { get; set; }
        public DbSet<TCVAltreInf> TCVAltreInf { get; set; }
        public DbSet<TCVAltreInfPat> TCVAltreInfPat { get; set; }
        public DbSet<TCVAreaIntAz> TCVAreaIntAz { get; set; }
        public DbSet<TCVBox> TCVBox { get; set; }
        public DbSet<TCVBox_Figuraprof> TCVBox_Figuraprof { get; set; }
        public DbSet<TCVBox_V2> TCVBox_V2 { get; set; }
        public DbSet<TCVCompDigit> TCVCompDigit { get; set; }
        public DbSet<TCVConInfo> TCVConInfo { get; set; }
        public DbSet<TCVConProf> TCVConProf { get; set; }
        public DbSet<TCVFormExRai> TCVFormExRai { get; set; }
        public DbSet<TCVLingue> TCVLingue { get; set; }
        public DbSet<TCVSpecializz> TCVSpecializz { get; set; }
        public DbSet<VDServizio> VDServizio { get; set; }
        public DbSet<VDSocieta> VDSocieta { get; set; }
        public DbSet<TCVAllegato> TCVAllegato { get; set; }
        public DbSet<TSVEsperProd> TSVEsperProd { get; set; }
        public DbSet<VDServizioCV> VDServizioCV { get; set; }
        public DbSet<VDFiguraProfCV> VDFiguraProfCV { get; set; }
        public DbSet<TCVIstruzione> TCVIstruzione { get; set; }
        public DbSet<DAteneoCV> DAteneoCV { get; set; }
        public DbSet<DTabellaCV> DTabellaCV { get; set; }
        public DbSet<TCVAreaIntAzEstero> TCVAreaIntAzEstero { get; set; }
        public DbSet<DConProf> DConProf { get; set; }
        public DbSet<TCVEsperExRai> TCVEsperExRai { get; set; }
        public DbSet<TAnagPers> TAnagPers { get; set; }
        public DbSet<TDipendenti> TDipendenti { get; set; }
        public DbSet<TSezione> TSezione { get; set; }
        public DbSet<DLingua> DLingua { get; set; }
        public DbSet<MENU> MENU { get; set; }
        public DbSet<TXLoginDB2> TXLoginDB2 { get; set; }
        public DbSet<TCVLogin> TCVLogin { get; set; }
        public DbSet<TCVEsperProd> TCVEsperProd { get; set; }
        public DbSet<TCVBox_Figuraprof_V2> TCVBox_Figuraprof_V2 { get; set; }
        public DbSet<Files> Files { get; set; }
        public DbSet<JobPostingFiles> JobPostingFiles { get; set; }
        public DbSet<TCVCertifica> TCVCertifica { get; set; }
        public DbSet<DSedeContabile> DSedeContabile { get; set; }
        public DbSet<TSVPrenElencoDip> TSVPrenElencoDip { get; set; }
        public DbSet<TSVPrenPrenota> TSVPrenPrenota { get; set; }
        public DbSet<TSVPrenSlot> TSVPrenSlot { get; set; }
        public DbSet<TSVPrenStanza> TSVPrenStanza { get; set; }
        public DbSet<V_CVCorsiRai> V_CVCorsiRai { get; set; }
        public DbSet<TAlbero> TAlbero { get; set; }
        public DbSet<TIncarico> TIncarico { get; set; }
        public DbSet<TPesoPosiz> TPesoPosiz { get; set; }
        public DbSet<TStruttura> TStruttura { get; set; }
        public DbSet<TTipologia> TTipologia { get; set; }
    
        public virtual ObjectResult<string> sp_GERARSEZIONE(string sEZIONE)
        {
            var sEZIONEParameter = sEZIONE != null ?
                new ObjectParameter("SEZIONE", sEZIONE) :
                new ObjectParameter("SEZIONE", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("sp_GERARSEZIONE", sEZIONEParameter);
        }
    
        public virtual ObjectResult<sp_GETDESCTITOLO_Result> sp_GETDESCTITOLO(string cODTITOLO)
        {
            var cODTITOLOParameter = cODTITOLO != null ?
                new ObjectParameter("CODTITOLO", cODTITOLO) :
                new ObjectParameter("CODTITOLO", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GETDESCTITOLO_Result>("sp_GETDESCTITOLO", cODTITOLOParameter);
        }
    
        public virtual ObjectResult<sp_GETDESCTITOLO1_Result> sp_GETDESCTITOLO1(string cODTITOLO)
        {
            var cODTITOLOParameter = cODTITOLO != null ?
                new ObjectParameter("CODTITOLO", cODTITOLO) :
                new ObjectParameter("CODTITOLO", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GETDESCTITOLO1_Result>("sp_GETDESCTITOLO1", cODTITOLOParameter);
        }
    
        public virtual ObjectResult<Nullable<System.DateTime>> sp_GETDTAGGCV(string mATRICOLA)
        {
            var mATRICOLAParameter = mATRICOLA != null ?
                new ObjectParameter("MATRICOLA", mATRICOLA) :
                new ObjectParameter("MATRICOLA", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<System.DateTime>>("sp_GETDTAGGCV", mATRICOLAParameter);
        }
    
        public virtual ObjectResult<sp_GETSTORFIGPRO_Result> sp_GETSTORFIGPRO(string mATRICOLA)
        {
            var mATRICOLAParameter = mATRICOLA != null ?
                new ObjectParameter("MATRICOLA", mATRICOLA) :
                new ObjectParameter("MATRICOLA", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GETSTORFIGPRO_Result>("sp_GETSTORFIGPRO", mATRICOLAParameter);
        }
    
        public virtual ObjectResult<sp_RicercaUtenti_Result> sp_RicercaUtenti(string inputTerm)
        {
            var inputTermParameter = inputTerm != null ?
                new ObjectParameter("inputTerm", inputTerm) :
                new ObjectParameter("inputTerm", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_RicercaUtenti_Result>("sp_RicercaUtenti", inputTermParameter);
        }
    
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
    }
}
