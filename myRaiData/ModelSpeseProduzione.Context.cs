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
    
    public partial class SpeseDiProduzioneEntities : DbContext
    {
        public SpeseDiProduzioneEntities()
            : base("name=SpeseDiProduzioneEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<dtproperties> dtproperties { get; set; }
        public DbSet<sysdiagrams> sysdiagrams { get; set; }
        public DbSet<TAnticipo> TAnticipo { get; set; }
        public DbSet<TAnticipo_Note> TAnticipo_Note { get; set; }
        public DbSet<TAnticipo_Note_controller> TAnticipo_Note_controller { get; set; }
        public DbSet<TAnticipo_Voci> TAnticipo_Voci { get; set; }
        public DbSet<TATTI_NOTARILI> TATTI_NOTARILI { get; set; }
        public DbSet<TBeneficiario> TBeneficiario { get; set; }
        public DbSet<TBlackList> TBlackList { get; set; }
        public DbSet<TCambioValuta> TCambioValuta { get; set; }
        public DbSet<TCOMPETENZE> TCOMPETENZE { get; set; }
        public DbSet<TCrossStatiProfili> TCrossStatiProfili { get; set; }
        public DbSet<TFoglio_Spese> TFoglio_Spese { get; set; }
        public DbSet<TFSP01> TFSP01 { get; set; }
        public DbSet<TFSP01_DATI> TFSP01_DATI { get; set; }
        public DbSet<TFSP02> TFSP02 { get; set; }
        public DbSet<TFSP02_DATI> TFSP02_DATI { get; set; }
        public DbSet<TImporti_SAP> TImporti_SAP { get; set; }
        public DbSet<TLocalita> TLocalita { get; set; }
        public DbSet<TLock> TLock { get; set; }
        public DbSet<TLog> TLog { get; set; }
        public DbSet<TMail_Sent> TMail_Sent { get; set; }
        public DbSet<TMessaggi> TMessaggi { get; set; }
        public DbSet<TParametri> TParametri { get; set; }
        public DbSet<TPROCURA> TPROCURA { get; set; }
        public DbSet<TPROCURATORI> TPROCURATORI { get; set; }
        public DbSet<TPROCURE> TPROCURE { get; set; }
        public DbSet<TRendiconto> TRendiconto { get; set; }
        public DbSet<TRendiconto_Cambi> TRendiconto_Cambi { get; set; }
        public DbSet<TRendiconto_Note> TRendiconto_Note { get; set; }
        public DbSet<TRendiconto_Note_Targh> TRendiconto_Note_Targh { get; set; }
        public DbSet<TRendiconto_Targhetta> TRendiconto_Targhetta { get; set; }
        public DbSet<TRendiconto_Voci> TRendiconto_Voci { get; set; }
        public DbSet<TRestituzione_Parziale> TRestituzione_Parziale { get; set; }
        public DbSet<TRestituzione_Totale> TRestituzione_Totale { get; set; }
        public DbSet<TSequenzaStati> TSequenzaStati { get; set; }
        public DbSet<TStato> TStato { get; set; }
        public DbSet<TTabella> TTabella { get; set; }
        public DbSet<TWhiteList> TWhiteList { get; set; }
        public DbSet<TXuAbilitazioni> TXuAbilitazioni { get; set; }
        public DbSet<TXuFunzioni> TXuFunzioni { get; set; }
        public DbSet<TXuGruppi_Gestiti> TXuGruppi_Gestiti { get; set; }
        public DbSet<TXuGruppo> TXuGruppo { get; set; }
        public DbSet<TXuProfilo> TXuProfilo { get; set; }
        public DbSet<TXuUffici_Gestiti> TXuUffici_Gestiti { get; set; }
        public DbSet<TXuUtente> TXuUtente { get; set; }
        public DbSet<PROVA> PROVA { get; set; }
        public DbSet<TAnticipo_20180918> TAnticipo_20180918 { get; set; }
        public DbSet<TCoordinate_Bancarie> TCoordinate_Bancarie { get; set; }
        public DbSet<TCoordinate_Bonifico> TCoordinate_Bonifico { get; set; }
        public DbSet<TImporti_SAP_20171214> TImporti_SAP_20171214 { get; set; }
        public DbSet<TImporti_SAP_20180111> TImporti_SAP_20180111 { get; set; }
        public DbSet<TImporti_SAP_20180918> TImporti_SAP_20180918 { get; set; }
        public DbSet<TUltimo_ID> TUltimo_ID { get; set; }
        public DbSet<VCOMPETENZE> VCOMPETENZE { get; set; }
        public DbSet<VDOCUMENTO_PROCURA> VDOCUMENTO_PROCURA { get; set; }
        public DbSet<VPROCURA> VPROCURA { get; set; }
        public DbSet<VPROCURATORE> VPROCURATORE { get; set; }
        public DbSet<VPROCURE> VPROCURE { get; set; }
    }
}