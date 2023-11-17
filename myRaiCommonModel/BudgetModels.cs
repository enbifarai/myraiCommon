using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace myRaiCommonModel
{
    public class PRV_Dipendente
    {
        public PRV_Dipendente ( )
        {
            this.Direzione = new XR_PRV_DIREZIONE( );
            this.Area = new XR_PRV_AREA( );
            this.Campagna = new XR_PRV_CAMPAGNA( );
        }

        public int IdPersona { get; set; }

        public string Matricola { get; set; }

        public string Nominativo { get; set; }

        public decimal CostoAnnuo { get; set; }

        public decimal CostoPeriodo { get; set; }

        public decimal CostoConStraordinario { get; set; }

        public int IdProvvedimento { get; set; }

        public bool CustomProvv { get; set; }

        public int IdDipendente { get; set; }

        public List<PRV_Dipendente_CostoVariazione> Variazioni { get; set; }

        public int NumeroProvvedimenti { get; set; }

        public decimal RAL { get; set; }

        public List<Provvedimento> Provvedimenti { get; set; }

        public string LivAttuale { get; set; }

        public string LivPrevisto { get; set; }

        public bool IsConsolidato { get; set; }

        public DateTime? Decorrenza { get; set; }

        public string CodRuolo { get; set; }

        public string DescRuolo { get; set; }

        public string Struttura { get; set; }

        public DateTime? DataNascita { get; set; }

        public DateTime? DataAssunzione { get; set; }

        public DateTime? AnzianitaLivello { get; set; }

        public decimal RetribuzioneVariabile { get; set; }

        public decimal Reperibilita { get; set; }

        public List<Assenza> Assenze { get; set; }

        public string Note { get; set; }

        public string AggregatoSede { get; set; }

        public string PartTime { get; set; }

        public bool UtenteNelleSottoSedi { get; set; }

        public int? IdAssQual { get; set; }

        public int IdProvvedimentoRich { get; set; }

        public string Cod_Qualifica { get; set; }
        public string Des_Qualifica { get; set; }

        public int? IdDirezione { get; set; }
        public int? IdArea { get; set; }

        public XR_PRV_DIREZIONE Direzione { get; set; }

        public XR_PRV_AREA Area { get; set; }

        public string MacroArea { get; set; }

        public int? IdCampagna { get; set; }

        public XR_PRV_CAMPAGNA Campagna { get; set; }

        public string CategoriaPrevista { get; set; }
    }

    public class Assenza
    {
        public string Codice { get; set; }

        public double Quantita { get; set; }
    }

    public class Causa
    {
        public int Id { get; set; }

        public string Stato { get; set; }

        public string Descrizione { get; set; }

        public DateTime? Data { get; set; }

        public int? Anno { get; set; }
    }

    public class Vertenza
    {
        public int Id { get; set; }

        public string Stato { get; set; }

        public string Descrizione { get; set; }

        public DateTime? Data { get; set; }

        public int Anno { get; set; }
    }

    public class Provvedimento
    {
        public int Id { get; set; }

        public string Descrizione { get; set; }

        public int IdProvvedimento { get; set; }

        public DateTime Data { get; set; }

        public decimal Importo { get; set; }
    }

    public class SimulazioneBudgetDirezioneVM
    {
        public int IdDirezione { get; set; }

        public int IdCampagna { get; set; }

        public bool IsConsolidata { get; set; }

        public List<PRV_Dipendente> DipendentiConProvvedimento { get; set; }

        public BudgetRiepilogo BudgetRiepilogo { get; set; }

        public RiepilogoPromozioni RiepilogoPromozioni { get; set; }

        public bool IsVisualizzazione { get; set; }

        public int? AnnoSelezionato { get; set; }
    }

    public class PRV_Dipendente_CostoVariazione
    {
        public int IdProvvedimento { get; set; }

        public decimal Costo { get; set; }

        public decimal CostoPeriodo { get; set; }

        public decimal CostoStraordinario { get; set; }

        public string LivAttuale { get; set; }

        public string LivPrevisto { get; set; }
    }
    
    public class RiepilogoPromozioni
    {
        public RiepilogoPromozioni()
        {
            this.Livello0A0 = new Promozione();
            this.Livello1 = new Promozione();
            this.Livello3 = new Promozione();
            this.Livello4 = new Promozione();
        }

        public Promozione Livello0A0 { get; set; }
        public Promozione Livello1 { get; set; }
        public Promozione Livello3 { get; set; }
        public Promozione Livello4 { get; set; }
        public Promozione Gratifiche { get; set; }
    }

    public class Promozione
    {
        public string Descrizione { get; set; }

        public int NumeroProvvedimenti { get; set; }

        public decimal Costo { get; set; }
    }

    public partial class PRV_DIREZIONE
    {
        public int ID_DIREZIONE { get; set; }
        public int ID_AREA { get; set; }
        public string CODICE { get; set; }
        public string NOME { get; set; }
        public int ORGANICO { get; set; }
        public int ORGANICO_MASCHILE { get; set; }
        public int ORGANICO_FEMMINILE { get; set; }
        public decimal BUDGET { get; set; }
        public int? Ordine { get; set; }
        public int ORGANICO_AD { get; set; }
        public int ORGANICO_MASCHILE_AD { get; set; }
        public int ORGANICO_FEMMINILE_AD { get; set; }
        public int? ORGANICO_CONTABILE { get; set; }
        public decimal BUDGET_PERIODO { get; set; }
    }

    public class RicalcoloBudgetElement
    {
        public int IdDipendente { get; set; }
        public string Valore { get; set; }

        public decimal ValoreToEuro {
            get
            {
                if (String.IsNullOrEmpty(Valore))
                {
                    return 0;
                }
                else
                {
                    try
                    {
                        string toReplace = "0";
                        if (CultureInfo.CurrentCulture.Name == "en-US")
                        {
                            toReplace = this.Valore.Replace(",", ".");
                        }
                        else
                        {
                            toReplace = this.Valore;
                        }

                        decimal newVal = decimal.Parse(toReplace);

                        return newVal;
                    }
                    catch
                    {
                        return 0;
                    }
                }
            }
        }
    }

    public class ReportItem
    {
        public string Direzione { get; set; }
        public string ProvvRichiesto { get; set; }
        public string DecorrenzaRichiesta { get; set; }
        public string LivelloRichiesto { get; set; }
        public string Struttura { get; set; }
        public string Nominativo { get; set; }
        public string Profilo { get; set; }
        public string Livello { get; set; }
        public string UltimiProvv { get; set; }
        public string RAL { get; set; }
        public string CostoRegime { get; set; }
        public string CostoPeriodo { get; set; }
        public string Note { get; set; }
        public string Vertenze { get; set; }
        public string Cause { get; set; }
        public DateTime? DataNascita { get; set; }
        public DateTime? DataAssunzione { get; set; }
        public DateTime? AnzianitaLivello { get; set; }
        public string RetribuzioneVariabile { get; set; }
        public string Reperibilita { get; set; }
        public string Assenze { get; set; }
        public string AggregatoSede { get; set; }
        public string PartTime { get; set; }
        public string Matricola { get; set; }
        public string ProvvOriginale { get; set; }
    }

    public class BudgetVM
    {
        public List<ReportDirezione> ReportDirezioni { get; set; }

        public InfoCampagna InfoCampagna { get; set; }

        public List<InfoCampagna> Campagne { get; set; }

        public int? AnnoSelezionato { get; set; }

        public bool ShowReportPoliticheRetributive { get; set; }
    }
    
    public class DettaglioDirezioneVM
    {
        public RiepilogoOrganico RiepilogoOrganico { get; set; }

        public List<DettaglioDatiOrganico> DettaglioDatiOrganico { get; set; }

        public List<DettaglioDatiOrganico> ProvvedimentiIpotizzati { get; set; }

        public List<IpotesiPoliticheRetributiveECosti> IpotesiPoliticheRetributiveECosti { get; set; }

        public List<BudgetRiepilogo> BudgetRiepilogo { get; set; }
    }

    public class ReportDirezione
    {
        public int Id { get; set; }

        public string Area { get; set; }

        public string Direzione { get; set; }

        public int OrganicoContabile { get; set; }

        public int OrganicoRipartizione { get; set; }

        public decimal BudgetAnnoPrecedente { get; set; }

        public decimal BudgetAnnoCorrente { get; set; }

        public decimal CostoAnnoPrecedente { get; set; }

        public decimal CostoAnnoCorrente { get; set; }

        public decimal BudgetPeriodoCorrente { get; set; }

        public decimal BudgetPeriodoAnnoPrecedente { get; set; }

        public decimal CostoPeriodo { get; set; }

        public decimal CostoPeriodoAnnoCorrente { get; set; }

        public decimal CostoPeriodoAnnoPrecedente { get; set; }

        public decimal CostoRegime { get; set; }

        public decimal CostoRecuperoStraordinario { get; set; }

        public int Promozioni { get; set; }

        public int Aumenti { get; set; }

        public int F1 { get; set; }

        public int Gratifiche { get; set; }

        public int TotProvv { get; set; }

        public int PercentualeSuOrganico { get; set; }

        public decimal DeltaSuCostoAnnoPrecedente { get; set; }

        public decimal DeltaSuCostoAnnoCorrente { get; set; }

        public decimal DeltaSuCostoPeriodo { get; set; }

        public bool IsConsolidata { get; set; }

        public int? IdGruppo { get; set; }

        public int? ParentIdGruppo { get; set; }

        public int? Ordine { get; set; }

        public bool Visibled { get; set; }
        public string Codice { get; set; }
    }

    public class RiepilogoOrganico
    {
        public int OrgGestAttuale { get; set; }

        public int OrgFiniBudget { get; set; }

        public int OrgAllaData { get; set; }
    }

    public class DettaglioDatiOrganico
    {
        public string Personale { get; set; }
        
        public int Totale { get; set; }

        public int PercSuOrg { get; set; }

        public int Donne { get; set; }

        public int PercOrgCatProfDonne { get; set; }

        public int Uomini { get; set; }

        public int PercOrgCatProfUomini { get; set; }
    }

    public class IpotesiPoliticheRetributiveECosti
    {
        public string ProvvedimentoRichiesto { get; set; }

        public int NumeroProvvedimenti { get; set; }

        public decimal CostoAnnoPrecedente { get; set; }

        public decimal CostoAnnoCorrenteConRecStraor { get; set; }
    }

    public class BudgetRiepilogo
    {
        public decimal Area { get; set; }
        public decimal Direzione { get; set; }
        public decimal DirezionePeriodo { get; set; }
        public decimal TotaleProvv { get; set; }
        public decimal Delta { get; set; }
    }

    public class DettaglioCampagna
    {
        public int IdArea { get; set; }

        public string NomeArea { get; set; }

        public decimal Budget { get; set; }

        public decimal BudgetSpeso { get; set; }
    }

    public class InfoCampagna
    {
        public InfoCampagna()
        {
            this.DettaglioCampagna = new List<DettaglioCampagna>();
            this.CampagneContenute = new List<int>();
            this.Decorrenze = new List<DateTime>( );
        }
        public int Id { get; set; }

        public string NomeCampagna { get; set; }

        public List<DettaglioCampagna> DettaglioCampagna { get; set; }

        public decimal BudgetPeriodo { get; set; }

        public decimal CostoPeriodo { get; set; }

        public decimal BudgetAnno { get; set; }

        public decimal CostoAnno { get; set; }

        public List<int> CampagneContenute { get; set; }

        public List<DateTime> Decorrenze { get; set; }
    }

    public enum ProvvedimentiEnum
    {
        [Display(Name = "Aumento di livello")]
        [DescriptionAttribute("Promozione")]
        AumentoLivello = 1,
        [Display(Name = "Aumento di merito")]
        [DescriptionAttribute("Aumento di merito")]
        AumentoMerito = 2,
        [Display(Name = "Gratifica")]
        [DescriptionAttribute("Gratifica")]
        Gratifica = 3,
        [Display(Name = "Aumento di livello senza assorbimento")]
        [DescriptionAttribute("Promozione senza assorbimento")]
        AumentoLivelloNoAssorbimento = 4,
        [Display(Name = "Nessuno")]
        [DescriptionAttribute("Nessuno")]
        Nessuno = 5,
        [Display(Name = "Aumento di livello")]
        [DescriptionAttribute("Promozione")]
        CUSAumentoLivello = 6,
        [Display(Name = "Aumento di livello senza assorbimento")]
        [DescriptionAttribute("Promozione senza assorbimento")]
        CUSAumentoLivelloNoAssorbimento = 7,
        [Display(Name = "Aumento di merito")]
        [DescriptionAttribute("Aumento di merito")]
        CUSAumentoMerito = 8,
        [Display(Name = "Gratifica")]
        [DescriptionAttribute("Gratifica")]
        CUSGratifica = 9,
        [Display(Name = "Nessuno")]
        [DescriptionAttribute("Nessuno")]
        CUSNessuno = 10
    }

    public class DettaglioProvvedimento
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public DateTime DataNascita { get; set; }
        public string Sesso { get; set; }
        public string CodiceTipoDipendente { get; set; }
        public string DescrizioneTipoDipendente { get; set; }
        public string DescrizioneLivelloProfessionale { get; set; }
        public string DescrizioneTipoCategoria { get; set; }
        public DateTime DataAssunzione { get; set; }
        public DateTime DataAnzianitaCategoria { get; set; }
        public string CodicePartTime { get; set; }
        public string DescrizionePartTime { get; set; }
        public string CodiceDirezione { get; set; }
        public string DescrizioneDirezione { get; set; }
        public string CodiceStruttura { get; set; }
        public string DescrizioneStruttura { get; set; }
        public string Distacco { get; set; }
        public string CodiceServizioContabile { get; set; }
        public string DescrizioneServizioContabile { get; set; }
        public string CodiceDivisione { get; set; }
        public string DivisioneAppartenenza { get; set; }
        public string CodiceRapportoLavoro { get; set; }
        public string DescrizioneRapportoLavoro { get; set; }
        public string DescrizioneLivello { get; set; }
        public string CodiceSezione { get; set; }
        public string DescrizioneSezione { get; set; }
        public string CodiceServizioStruttura { get; set; }
        public decimal RAL { get; set; }
        public int? IdProvvRichiesto { get; set; }
        public int? IdProvvEffettivo { get; set; }
    }

    public class DettaglioGestioneDirezioneVM
    {
        public List<DettaglioProvvedimento> DettaglioProvvedimento { get; set; }

        public List<BudgetRiepilogo> BudgetRiepilogo { get; set; }

        public RiepilogoOrganico RiepilogoOrganico { get; set; }

        public List<IpotesiPoliticheRetributiveECosti> IpotesiPoliticheRetributiveECosti { get; set; }
    }

    public class TabellaItem
    {
        public int IdDipendente { get; set; }

        public int IdTipologia { get; set; }

    }

    public class SalvaSimulazioneResponse
    {
        public bool Esito { get; set; }

        public string Errore { get; set; }
    }

    public enum OperStatiEnum
    {
        [Display(Name = "In carico")]
        [DescriptionAttribute("In carico")]
        InCarico = 0,
        [Display(Name = "Richiesta")]
        [DescriptionAttribute("Richiesta")]
        Richiesta = 1,
        [Display(Name = "Consolidato")]
        [DescriptionAttribute("Consolidato")]
        Consolidato = 2,
        [Display(Name = "Lettera consegnata")]
        [DescriptionAttribute("Lettera consegnata")]
        LetteraConsegnata = 3,
        [Display(Name = "Pratica conclusa")]
        [DescriptionAttribute("Pratica conclusa")]
        PraticaConclusa = 4
    }

    public class RiepilogoCampagnaItem
    {
        public string Direzione { get; set; }

        public int Richiesto { get; set; }

        public int Concordato { get; set; }

        public string Note { get; set; }

        public List<int> Decorrenze { get; set; }
    }

    public class RiepilogoCampagna
    {
        public String TipologiaProvvedimento { get; set; }

        public List<RiepilogoCampagnaItem> Items { get; set; }
    }

    public class DirezioniAccorpatePerLaStampaItem
    {
        public string Direzione { get; set; }
    }

    public class DirezioniAccorpatePerLaStampaList
    {
        public List<DirezioniAccorpatePerLaStampaItem> DirezioniAccorpatePerLaStampa { get; set; }
    }

    public class ReportPoliticheRetributiveYearItem
    {
        public decimal Budget { get; set; }
        public decimal Costo { get; set; }
        public decimal Delta { get; set; }
    }

    public class ReportPoliticheRetributiveDetailsItem
    {
        public int Passaggi { get; set; }
        public int IncrementiRetributivi { get; set; }
        public int Gratifiche { get; set; }
    }

    public class ReportPoliticheRetributiveRisorsePanelDetailsItem
    {
        public decimal Anticipazioni { get; set; }
        public int TotaleInterventi { get; set; }
    }

    public class ReportPoliticheRetributiveTipologiaItem
    {
        public string Intestazione { get; set; }
        public decimal Staff1 { get; set; }
        public decimal Edit1 { get; set; }
        public decimal Prod1 { get; set; }
        public int Staff2 { get; set; }
        public int Edit2 { get; set; }
        public int Prod2 { get; set; }
    }

    public class ReportPoliticheRetributiveDettaglioPianoItem
    {
        public string Descrizione { get; set; }
        public decimal Colonna1 { get; set; }
        public int Colonna2 { get; set; }
    }

    public class ReportPoliticheRetributiveDettaglioPiano
    {
        public string Intestazione { get; set; }
        public List<ReportPoliticheRetributiveDettaglioPianoItem> Items { get; set; }
    }

    public class ReportPoliticheRetributive
    {
        public string Intestazione { get; set; }
        public List<ReportPoliticheRetributiveYearItem> RiepilogoPerAnno { get; set; }
        public ReportPoliticheRetributiveDetailsItem RiepilogoProvvedimenti { get; set; }
        public ReportPoliticheRetributiveRisorsePanelDetailsItem RiepilogoRisorse { get; set; }
        public ReportPoliticheRetributiveTipologiaItem DettaglioFunzionariF1 { get; set; }
        public ReportPoliticheRetributiveTipologiaItem DettaglioAltroPersonale { get; set; }
        public ReportPoliticheRetributiveTipologiaItem DettaglioPoliticheRetributive { get; set; }
        public List<ReportPoliticheRetributiveDettaglioPiano> DettaglioAllCampagne { get; set; }
    }

}