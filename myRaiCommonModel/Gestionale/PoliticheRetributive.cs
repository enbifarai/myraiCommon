using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using myRai.DataAccess;
using myRaiData.Incentivi;
using myRaiHelper;

namespace myRaiCommonModel.Gestionale
{

    public enum ProvvedimentoEnum
    {
        AumentoLivello = 1,
        AumentoMerito = 2,
        Gratifica = 3,
        AumentoLivelloSenzaRiass = 4,
        Nessuno = 5
    }
    public enum VariabileTipoEnum
    {
        Maggiorazioni = 1,
        Reperibilita=2
    }

    public class RicercaAnagrafica
    {
        public RicercaAnagrafica()
        {
            EscludiCessati = true;
        }
        public bool TreeSearch { get; set; }
        public string BoxDest { get; set; }

        public bool HasFilter { get; set; }

        public string Matricola { get; set; }
        public string Cognome { get; set; }
        public string Nome { get; set; }
        public string Servizio { get; set; }
        public string Sede { get; set; }
        public string Categoria { get; set; }
        public bool EscludiCessati { get; set; }
        public int Provvedimento { get; set; }
        public string PraticaInCarico { get; set; }
        public string Direzione { get; set; }
        public int Stato { get; set; }
        public string ResultView { get; set; }

        public int IdDirezionePratica { get; set; }
        public int IdAreaPratica { get; set; }
        public int Piano { get; set; }
        public string GestioneManuale { get; set; }

        public string GestioneEsterna { get; set; }

        public string Decorrenza { get; set; }
        public bool MatricoleMultiple { get; set; }

        
    }

    public class ElencoAnagrafiche
    {
        public bool TreeSearch { get; set; }
        public int IdCampagna { get; set; }
        public DateTime Decorrenza { get; set; }
        public IEnumerable<SINTESI1> anagrafiche { get; set; }
    }

    public class SelectListGroup
    {
        public SelectListGroup()
        {
            ListItems = new List<ListItem>();
        }
        public string Name { get; set; }
        public List<ListItem> ListItems { get; set; }
    }

    public class GestioneStatiClass
    {
        public int IdPersona;
        public int CurrentState;
        public int IdCurrentState;
        public bool PraticaInLavorazione;
        public bool MiaPratica;
        public bool InServizio;
        public int percCompl;

        public bool praticaChiusa;

        public bool showPrendiInCarico;
        public bool showAvviaPratica;

        public bool showTab;

        //public string classTabCont;
        //public string classTabApp;
        //public string classTabVerbFirm;
        //public string classTabVerbUpload;
        //public string classTabPag;

        public bool showTabSceltaProvv;
        public bool showTabPanelCons;
        public bool showTabPanelLett;
    }

    public class GraphPoint
    {
        public string x { get; set; }
        public decimal y { get; set; }
    }

    public class Pratica
    {
        public XR_PRV_DIPENDENTI Dipendente { get; set; }

        public bool CanShowData { get; set; }
        public int CauseAperte { get; set; }
        public int CauseChiuse { get; set; }
        public int ProvvedimentiAperti { get; set; }
        public int ProvvedimentiChiusi { get; set; }
        public MyRaiServiceInterface.MyRaiServiceReference1.Provvedimento[] Provvedimenti { get; set; }
        public MyRaiServiceInterface.MyRaiServiceReference1.Causa[] Cause { get; set; }
        public List<XR_PRV_BOX> BoxAbilitati { get; set; }
        public List<XR_PRV_DIPENDENTI_VERTENZE> VertenzeSind { get; set; }
        public List<XR_PRV_DIPENDENTI_CAUSE> CauseDB { get; set; }
        public List<Stragiudiziale> Stragiudiziali { get; set; }
        public List<XR_PRV_LOG> LogOperazioni { get; set; }
        public List<XR_PRV_PROV_PASSCAT> PossibiliPassaggi { get; set; }
    
        public bool IsPreview { get; set; }
        public bool EnableGest { get; set; }
        public bool EnableBudgetGest { get; set; }
        public bool IsAbilAmm { get; set; }

        public List<SelectListItem> BozzeList { get; set; }

    }

    public class CampagnaWrapper
    {
        public XR_PRV_CAMPAGNA campagna { get; set; }
        public bool EnableGest { get; set; }
    }

    public class Stragiudiziale
    {
        public string Matricola { get; set; }
        public string Soggetto { get; set; }
        public string NumeroDossier { get; set; }
        public string DataCreazione { get; set; }
        public string Oggetto { get; set; }
        public string DescrizioneStato { get; set; }
        public string DataStato { get; set; }
        public string Note { get; set; }
    }

    public class PraticaOverview
    {
        public string DistroProvv { get; set; }
        public string DistroProvvPerSedeBar { get; set; }
        public string DistroProvvPerSede { get; set; }
    }

    public class Budget
    {
        public Budget()
        {
            Aree = new List<BudgetArea>();
            DateDecorrenza = new List<string>();
        }

        public string Nome { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public List<string> DateDecorrenza { get; set; }
        public List<BudgetArea> Aree  { get; set; }
        public decimal? Riserva { get; set; }

        public int? IdCampagna { get; set; }
        public bool EnableGest { get; set; }

        public string LivAbil { get; set; }
        public List<Tuple<string,string>> abilDisponibili { get; set; }
    }
    public class BudgetArea
    {
        public BudgetArea()
        {
            //Direzioni = new IEnumerable<BudgetDirezione>();
        }

        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal Importo { get; set; }
        public string ImportoStr { get; set; }
        public decimal ImportoPeriodo { get; set; }
        public string ImportoPeriodoStr { get; set; }
        public IEnumerable<BudgetDirezione> Direzioni { get; set; }

    }
    public class BudgetDirezione
    {
        public int Id { get; set; }
        public string Codice { get; set; }
        public string Nome { get; set; }
        public int Organico { get; set; }
        public int OrganicoM { get; set; }
        public int OrganicoF { get; set; }
        public decimal Budget { get; set; }
        public string BudgetStr { get; set; }
        public string BudgetPeriodoStr { get; set; }

        public int OrganicoGiaProvv { get; set; }
        public int OrganicoAD { get; set; }
        public decimal BudgetPeriodo { get; set; }
    }

    public class PolRetrLayout
    {
        public PolRetrLayout()
        {
            BoxAbilitati = new List<XR_PRV_BOX>();
            HasAnyConsolidated = false;
        }
        public List<XR_PRV_BOX> BoxAbilitati { get; set; }
        public bool HasAnyConsolidated { get; set; }
        public List<string> RichiesteDoppie { get; set; }
    }

    public class Lettera
    {
        public Lettera()
        {
            Livelli = new Dictionary<string, string>()
            {
                { "LV.0A.0", "" },
                { "LV.01.0", "1" },
                { "LV.02.0", "2" },
                { "LV.03.0", "3" },
                { "LV.04.0", "4" },
                { "LV.05.0", "5" },
                { "LV.06.0", "6" },
                { "LV.07.0", "7" }
            };
        }
        public int IdPratica { get; set; }
        public string DipName { get; set; }
        public string DipSurname { get; set; }
        public string HeaderText { get; set; }
        public string BodyText { get; set; }
        public string FooterText { get; set; }
        public string SignText { get; set; }

        public string SignImage { get; set; }
        public byte[] SignImageByte { get; set; }
        public Dictionary<string,string> Livelli { get;  }
    }

    public class Inquadramento
    {
        public string cod_divisione { get; set; }
        public string des_divisione { get; set; }
        public string cod_direzione { get; set; }
        public string des_direzione { get; set; }
        public string cod_struttura { get; set; }
        public string des_struttura { get; set; }
        public string cod_sezione { get; set; }
        public string des_sezione { get; set; }
    }
}

namespace myRaiCommonModel.Gestionale.HRDW
{
    public class HRDWRisorsa
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string CodServizio { get; set; }
        public string DesServizio { get; set; }
        public string CodSezione { get; set; }
        public string DesSezione { get; set; }
    }
    public class HRDWDirezione
    {
        public HRDWDirezione()
        {
            Gratifiche = new List<HRDWPolRetr>();
            Aumenti = new List<HRDWPolRetr>();
            Passaggi = new List<HRDWPolRetr>();
        }

        public string Codice { get; set; }
        public string Categoria { get; set; }
        public int Organico { get; set; }
        public int OrganicoMaschile { get; set; }
        public int OrganicoFemminile { get; set; }
        public int OrganicoInteressato { get; set; }
        public List<HRDWPolRetr> Gratifiche { get; set; }
        public List<HRDWPolRetr> Aumenti { get; set; }
        public List<HRDWPolRetr> Passaggi { get; set; }

        public int ProvvedimentiPerAnno(int anno)
        {
            int result = 0;
            result += Gratifiche.Any(x => x.Anno == anno) ? Gratifiche.First(x => x.Anno == anno).Numero : 0;
            result += Aumenti.Any(x => x.Anno == anno) ? Aumenti.First(x => x.Anno == anno).Numero : 0;
            result += Passaggi.Any(x => x.Anno == anno) ? Passaggi.First(x => x.Anno == anno).Numero : 0;
            return result;
        }
    }
    public class HRDWPolRetr
    {
        public int Anno { get; set; }
        public int Numero { get; set; }
    }
    public class HRDWComodo
    {
        public string Codice { get; set; }
        public int Numero { get; set; }
    }
    public class HRDWPassaggioAumento
    {
        public string Matricola { get; set; }
        public int Anno { get; set; }
        public int Mese { get; set; }
        public int Giorno { get; set; }
        public decimal Importo { get; set; }
        public string cod_aggregato_tipo_var_combinazione { get; set; }
        public string desc_aggregato_tipo_var_combinazione { get; set; }

        public string codice_tipo_var_contabili_1 { get; set; }
        public string desc_tipo_variazione_te_1 { get; set; }
        public string codice_tipo_var_contabili_2 { get; set; }
        public string desc_tipo_variazione_te_2 { get; set; }
        public string codice_tipo_var_contabili_3 { get; set; }
        public string desc_tipo_variazione_te_3 { get; set; }
        public string codice_tipo_var_contabili_4 { get; set; }
        public string desc_tipo_variazione_te_4 { get; set; }
        public string Motivo_variazione { get; set; }
        public string Flag_motivo_var { get; set; }
        public string cod_categoria { get; set; }
        public string desc_categoria { get; set; }
        public string desc_livello { get; set; }

        public string GetDescrizione()
        {
            return (!String.IsNullOrWhiteSpace(desc_aggregato_tipo_var_combinazione) ? desc_aggregato_tipo_var_combinazione + "\n" : "") +
                    (!String.IsNullOrWhiteSpace(desc_tipo_variazione_te_1) ? desc_tipo_variazione_te_1 + "\n" : "") +
                    (!String.IsNullOrWhiteSpace(desc_tipo_variazione_te_2) ? desc_tipo_variazione_te_2 + "\n" : "") +
                    (!String.IsNullOrWhiteSpace(desc_tipo_variazione_te_3) ? desc_tipo_variazione_te_3 + "\n" : "") +
                    (!String.IsNullOrWhiteSpace(desc_tipo_variazione_te_4) ? desc_tipo_variazione_te_4 + "\n" : "");
        }

    }
    public class HRDWDatiMatr
    {
        public string Matricola { get; set; }
        public string Servizio { get; set; }
        public string CodSede { get; set; }
        public string DesSede { get; set; }
        public string CodPartTime { get; set; }
        public string DesPartTime { get; set; }
        public string DesLivello { get; set; }
        public decimal RetribAnnua { get; set; }
    }
    public class HRDWVariazioni
    {
        public string Matricola { get; set; }
        public string CodPartTime { get; set; }
        public string DesPartTime { get; set; }
        public string LivelloAttuale { get; set; }
        public decimal Ral_Annua { get; set; }
        public decimal? Diff_ral_merito { get; set; }
        public decimal? Diff_ral_passaggio { get; set; }
        public decimal? Diff_ral_passaggio_assorbimento { get; set; }
        public string Cod_Categoria { get; set; }
        public string Des_Categoria { get; set; }
        public string Cod_Liv_Categoria { get; set; }
        public string Des_Liv_Categoria { get; set; }
        public string desc_indennita_importi_acquisiti { get; set; }
        public string desc_indennita_importi_persi { get; set; }
        public string desc_indennita_importi_delta { get; set; }
    }
    public class HRDWRal
    {
        public string Matricola { get; set; }
        public int Anno { get; set; }
        public decimal? Ral_media { get; set; }
    }
    public class HRDWVariabili
    {
        public string matricola_dp { get; set; }
        public int Anno { get; set; }
        public int Mese { get; set; }
        public string cod_aggregato_costi { get; set; }
        public string desc_aggregato_costi { get; set; }
        public string cod_voce_cedolino { get; set; }
        public string desc_voce_cedolino { get; set; }
        public decimal Ore { get; set; }
        public decimal Giorni { get; set; }
        public decimal NumeroPrestazioni { get; set; }
        public decimal Importo { get; set; }
    }
    public class HRDWPartTime
    {
        public string Matricola { get; set; }
        public string CodPartTime { get; set; }
        public decimal PercPartTime { get; set; }
        public decimal UnitaAnno { get; set; }
    }
    public class HRDWAssenze
    {
        public string Matricola { get; set; }
        public decimal Quantita { get; set; }
        public string cod_eccez_padre { get; set; }
        public string desc_cod_eccez_padre { get; set; }
        public string cod_eccezione { get; set; }
        public string desc_eccezione { get; set; }
        public string unita_misura { get; set; }
        public int Anno { get; set; }
        public int Mese { get; set; }
    }
    public class HRDWData
    {
        public static bool GetData(string matricola, out string partTime, out decimal ralMedia)
        {
            bool result = false;
            partTime = "";
            ralMedia = 0;

            IncentiviEntities db = new IncentiviEntities();
            string query = " SELECT t0.[matricola_dp], " +
                            " 	t0.[nominativo], " +
                            " 	t9.[cod_tipo_dipendente], " +
                            " 	t9.[desc_tipo_dipendente], " +
                            " 	t9.[desc_liv_professionale], " +
                            " 	t9.[desc_liv_tipo_categoria], " +
                            " 	t0.[data_nascita], " +
                            " 	round((Datediff(dd, t0.[data_nascita], getDate()) / 365.25), 2) AS eta, " +
                            " 	{fn year(t0.[data_nascita]) } AS anno_nascita, " +
                            " 	t11.[DATA_ASSUNZIONE], " +
                            " 	t11.[DATA_ANZ_CATEGORIA], " +
                            " 	t2.[cod_part_time], " +
                            " 	t2.[desc_part_time], " +
                            " 	t6.[cod_direzione], " +
                            " 	t6.[des_direzione], " +
                            " 	t6.[cod_struttura], " +
                            " 	t6.[des_struttura], " +
                            " 	CASE  " +
                            " 		WHEN t1.[flg_distacco] = 'E' " +
                            " 			THEN 'Distacco da Consociate ' " +
                            " 		WHEN t1.[flg_distacco] = 'D' " +
                            " 			THEN 'Distacco da RAI ' " +
                            " 		WHEN t1.[flg_distacco] = 'C' " +
                            " 			THEN 'Trasferimento temporaneo (cambio sede/servizio)' " +
                            " 		WHEN t1.[flg_distacco] = 'B' " +
                            " 			THEN 'Trasferimento temporaneo' " +
                            " 		WHEN t1.[flg_distacco] = 'A' " +
                            " 			THEN 'Assegnazione temporanea' " +
                            " 		WHEN t1.[flg_distacco] = '1' " +
                            " 			THEN 'Segnalazioni errate' " +
                            " 		WHEN t1.[flg_distacco] = 'X' " +
                            " 			THEN 'Assegnazione temporanea senza scheda 1D' " +
                            " 		ELSE t1.[flg_distacco] " +
                            " 		END, " +
                            " 	t5.[cod_serv_cont], " +
                            " 	t5.[desc_serv_cont], " +
                            " 	t5.[cod_divisione], " +
                            " 	t5.[divisione_appartenenza], " +
                            " 	t3.[cod_rapp_lav], " +
                            " 	t3.[desc_rapp_lav], " +
                            " 	t10.[desc_livello], " +
                            " 	t6.[cod_sezione], " +
                            " 	t6.[des_sezione], " +
                            " 	t1.[cod_serv_struttura], " +
                            " 	avg(t8.[tot_retrib_annua]) " +
                            " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0  " +
                            " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                            " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_PART_TIME] t2 ON (t1.[SKY_PART_TIME] = t2.[sky_part_time]) " +
                            " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_RAPPORTO_DI_LAVORO] t3 ON (t1.[SKY_RAPPORTO_LAVORO] = t3.[sky_rapp_lav]) " +
                            " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SEDE] t4 ON (t1.[SKY_sede] = t4.[sky_sede]) " +
                            " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SERVIZIO_CONTABILE] t5 ON (t1.[SKY_servizio_contabile] = t5.[sky_serv_cont]) " +
                            " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SEZIONE] t6 ON (t1.[SKY_sezione] = t6.[sky_sezione]) " +
                            " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t7 ON (t1.[SKY_MESE_CONTABILE] = t7.[sky_tempo]) " +
                            " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE_IMPORTI] t8 ON (t8.[SKY_riga_te] = t1.[SKY_riga_te]) " +
                            " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA] t9 ON (t1.[SKY_categoria] = t9.[sky_categoria]) " +
                            " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] t10 ON (t10.[sky_livello_categ] = t1.[sky_livello_categ]) " +
                            " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CONTRATTO_UNICO] t11 ON (t1.[SKY_CONTRATTO] = t11.[SKY_CONTRATTO]) " +
                            " WHERE ( " +
                            " 		t7.[cod_anno_mese_7] = '" + DateTime.Today.ToString("yyyy/MM") + "' " +
                            " 		AND [l2d_anagrafica_unica].[matricola_dp] = '" + matricola + "' " +
                            " 		) " +
                            " GROUP BY t0.[matricola_dp], " +
                            " 	t0.[nominativo], " +
                            " 	t9.[cod_tipo_dipendente], " +
                            " 	t9.[desc_tipo_dipendente], " +
                            " 	t9.[desc_liv_professionale], " +
                            " 	t9.[desc_liv_tipo_categoria], " +
                            " 	t0.[data_nascita], " +
                            " 	round((Datediff(dd, t0.[data_nascita], getDate()) / 365.25), 2), " +
                            " 	{fn year(t0.[data_nascita]) }, " +
                            " 	t11.[DATA_ASSUNZIONE], " +
                            " 	t11.[DATA_ANZ_CATEGORIA], " +
                            " 	t2.[cod_part_time], " +
                            " 	t2.[desc_part_time], " +
                            " 	t6.[cod_direzione], " +
                            " 	t6.[des_direzione], " +
                            " 	t6.[cod_struttura], " +
                            " 	t6.[des_struttura], " +
                            " 	CASE  " +
                            " 		WHEN t1.[flg_distacco] = 'E' " +
                            " 			THEN 'Distacco da Consociate ' " +
                            " 		WHEN t1.[flg_distacco] = 'D' " +
                            " 			THEN 'Distacco da RAI ' " +
                            " 		WHEN t1.[flg_distacco] = 'C' " +
                            " 			THEN 'Trasferimento temporaneo (cambio sede/servizio)' " +
                            " 		WHEN t1.[flg_distacco] = 'B' " +
                            " 			THEN 'Trasferimento temporaneo' " +
                            " 		WHEN t1.[flg_distacco] = 'A' " +
                            " 			THEN 'Assegnazione temporanea' " +
                            " 		WHEN t1.[flg_distacco] = '1' " +
                            " 			THEN 'Segnalazioni errate' " +
                            " 		WHEN t1.[flg_distacco] = 'X' " +
                            " 			THEN 'Assegnazione temporanea senza scheda 1D' " +
                            " 		ELSE t1.[flg_distacco] " +
                            " 		END, " +
                            " 	t5.[cod_serv_cont], " +
                            " 	t5.[desc_serv_cont], " +
                            " 	t5.[cod_divisione], " +
                            " 	t5.[divisione_appartenenza], " +
                            " 	t3.[cod_rapp_lav], " +
                            " 	t3.[desc_rapp_lav], " +
                            " 	t10.[desc_livello], " +
                            " 	t6.[cod_sezione], " +
                            " 	t6.[des_sezione], " +
                            " 	t1.[cod_serv_struttura]; ";

            try
            {
                var tmp = db.Database.SqlQuery<string>(query);
            }
            catch (Exception)
            {

                throw;
            }


            return result;
        }

        public static decimal GetProvvAliq(IncentiviEntities db, int provv, string cat)
        {
            decimal aliq = 0;
            foreach (var item in db.XR_PRV_PROV_ALIQ.Where(x => x.ID_PROV == provv))
            {
                List<string> catIncluse = new List<string>();
                List<string> catEscluse = new List<string>();

                if (!String.IsNullOrWhiteSpace(item.CAT_INCLUSE))
                    catIncluse.AddRange(item.CAT_INCLUSE.Split(','));

                if (!String.IsNullOrWhiteSpace(item.CAT_ESCLUSE))
                    catEscluse.AddRange(item.CAT_ESCLUSE.Split(','));

                if ((catIncluse.Count() == 0 || catIncluse.Any(a => cat.StartsWith(a)))
                  && (catEscluse.Count() == 0 || !catEscluse.Any(a => cat.StartsWith(a))))
                {
                    aliq = Convert.ToDecimal(item.ALIQ);
                    break;
                }
            }
            return aliq;
        }
        public static bool GetData(string currentPMatricola, IncentiviEntities db, int idDip, string termId)
        {
            bool result = false;
            XR_PRV_DIPENDENTI dip = db.XR_PRV_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);

            if (dip != null)
            {
                try
                {
                    string matricola = dip.SINTESI1.COD_MATLIBROMAT;
                    //EstraiSede(db, dip, false);

                    if (RecuperoProvvedimenti(currentPMatricola, db, dip, termId, false)
                        && RecuperoRal(currentPMatricola, db, termId, dip, false)
                        && RecuperoAssenze(currentPMatricola, db, termId, dip, false)
                        && RecuperoVariabili(currentPMatricola, db, dip, termId, false)
                        && RecuperoReperibilita(currentPMatricola, db, dip, termId, false))
                    {
                        CalcoloCosti(currentPMatricola, db, termId, dip, false);
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                }
            }

            return result;
        }

        public static void EstraiSede(IncentiviEntities db, XR_PRV_DIPENDENTI dip, bool isPreview)
        {
            //recupero dati persona
            string matricola = dip.MATRICOLA;

            var dati = db.Database.SqlQuery<HRDWDatiMatr>(GetQueryInfoMatricola(matricola));
            if (dati.Count() > 0)
            {
                dip.COD_SEDE = dati.First().CodSede;
                dip.DES_SEDE = dati.First().DesSede;
                dip.PART_TIME = dati.First().DesPartTime;
                if (!isPreview) DBHelper.Save(db,"PoliticheRetributive - ") ;
            }
        }

        public static bool RecuperoAssenze(string currentPMatricola, IncentiviEntities db, string termId, XR_PRV_DIPENDENTI dip, bool isPreview )
        {
            int idDip = dip.ID_DIPENDENTE;
            string matricola = dip.MATRICOLA;

            if (!isPreview)
            {
                db.XR_PRV_DIPENDENTI_ASSENZE.RemoveWhere(x => x.ID_DIPENDENTE == idDip);
                if (!DBHelper.Save(db, "PoliticheRetributive - "))
                    return false;
            }
            string eccezioniammesse = CommonHelper.GetParametri<string>(EnumParametriSistema.PoliticheEccezioniAssenze)[0];
            int numgiornieccezioniammesse = Convert.ToInt16(CommonHelper.GetParametri<string>(EnumParametriSistema.PoliticheEccezioniAssenze)[1]);
            //recupero assenze
            IEnumerable<HRDWAssenze> assenze = db.Database.SqlQuery<HRDWAssenze>(GetQueryAssenze(matricola)).ToList();
            foreach (var assenza in assenze)
            {
               
                XR_PRV_DIPENDENTI_ASSENZE ass = new XR_PRV_DIPENDENTI_ASSENZE();
                if (eccezioniammesse.Contains(assenza.cod_eccezione))//&& (Convert.ToInt16(assenza.Quantita) > numgiornieccezioniammesse))
                {
                    ass.ANNO = assenza.Anno;
                    ass.MESE = assenza.Mese;
                    ass.QUANTITA = Convert.ToDouble(assenza.Quantita);
                    ass.UNITA_MISURA = assenza.unita_misura;
                    ass.COD_ECCEZ_PADRE = assenza.cod_eccez_padre;
                    ass.DESC_COD_ECCEZ_PADRE = assenza.desc_cod_eccez_padre;
                    ass.COD_ECCEZIONE = assenza.cod_eccezione;
                    ass.DES_ECCEZIONE = assenza.desc_eccezione;
                    ass.COD_USER = currentPMatricola;
                    ass.COD_TERMID = termId;
                    ass.TMS_TIMESTAMP = DateTime.Now;
                    dip.XR_PRV_DIPENDENTI_ASSENZE.Add(ass);

                    if (!isPreview)
                    {
                        ass.ID_ASSENZA = DBHelper.GeneraOID(db, "XR_PRV_DIPENDENTI_ASSENZE", "ID_ASSENZA");
                        ass.ID_DIPENDENTE = idDip;
                        if (!DBHelper.Save(db, "PoliticheRetributive - "))
                            return false;
                    }
                }
            }

            return true;
        }

        public static bool RecuperoRal(string currentPMatricola, IncentiviEntities db, string termId, XR_PRV_DIPENDENTI dip, bool isPreview)
        {
            int idDip = dip.ID_DIPENDENTE;
            string matricola = dip.MATRICOLA;

            if (!isPreview)
            { 
                db.XR_PRV_DIPENDENTI_RAL.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
                if (!DBHelper.Save(db, "PoliticheRetributive - "))
                    return false;
            }

            //recupero ral
            IEnumerable<HRDWRal> rals = db.Database.SqlQuery<HRDWRal>(GetQueryRAL(matricola)).ToList();
            foreach (var ral in rals.Where(x=>x.Ral_media!=null))
            {
                XR_PRV_DIPENDENTI_RAL dipRal = new XR_PRV_DIPENDENTI_RAL();
                if (!isPreview) dipRal.ID_RAL = DBHelper.GeneraOID(db, "XR_PRV_DIPENDENTI_RAL", "ID_RAL");
                dipRal.ID_DIPENDENTE = idDip;
                dipRal.IMPORTO = ral.Ral_media.Value;
                dipRal.DT_RAL = new DateTime(ral.Anno, 1, 1);
                dipRal.COD_USER = currentPMatricola;
                dipRal.COD_TERMID = termId;
                dipRal.TMS_TIMESTAMP = DateTime.Now;
                dip.XR_PRV_DIPENDENTI_RAL.Add(dipRal);
                if (!isPreview)
                {
                    if (!DBHelper.Save(db, "PoliticheRetributive - "))
                        return false;
                }
            }

            return true;
        }

        public static bool RecuperoVariabili(string currentPMatricola, IncentiviEntities db, XR_PRV_DIPENDENTI dip, string termId, bool isPreview )
        {
            int idDip = dip.ID_DIPENDENTE;
            string matricola = dip.MATRICOLA;

            if (!isPreview)
            {
                db.XR_PRV_DIPENDENTI_VAR.RemoveWhere(x => x.ID_DIPENDENTE == idDip && (x.ID_VAR_TIPO == null || x.ID_VAR_TIPO.Value == (int)VariabileTipoEnum.Maggiorazioni));
                if (!DBHelper.Save(db, "PoliticheRetributive - "))
                    return false;
            }

            //recupero variabili
            IEnumerable<HRDWVariabili> vars = db.Database.SqlQuery<HRDWVariabili>(GetQueryVariabili(matricola)).ToList();
            foreach (var var in vars)
            {
                XR_PRV_DIPENDENTI_VAR variabile = new XR_PRV_DIPENDENTI_VAR();
                if (!isPreview) variabile.ID_VARIABILE = DBHelper.GeneraOID(db, "XR_PRV_DIPENDENTI_VAR", "ID_VARIABILE");
                variabile.ID_DIPENDENTE = idDip;
                variabile.ID_VAR_TIPO = (int)VariabileTipoEnum.Maggiorazioni;
                variabile.ANNO = var.Anno;
                variabile.MESE = var.Mese;
                variabile.COD_AGGR_COSTI = var.cod_aggregato_costi;
                variabile.DES_AGGR_COSTI = var.desc_aggregato_costi;
                variabile.COD_VOCE_CED = var.cod_voce_cedolino;
                variabile.DES_VOCE_CED = var.desc_voce_cedolino;
                variabile.NM_ORE = Convert.ToDouble(var.Ore);
                variabile.NM_GIORNI = Convert.ToDouble(var.Giorni);
                variabile.NM_PRESTAZIONI = Convert.ToDouble(var.NumeroPrestazioni);
                variabile.IMPORTO = var.Importo;
                variabile.COD_USER = currentPMatricola;
                variabile.COD_TERMID = termId;
                variabile.TMS_TIMESTAMP = DateTime.Now;
                dip.XR_PRV_DIPENDENTI_VAR.Add(variabile);
                if (!isPreview)
                {
                    if (!DBHelper.Save(db, "PoliticheRetributive - "))
                        return false;
                }
            }

            return true;
        }

        public static bool RecuperoReperibilita(string currentPMatricola, IncentiviEntities db, XR_PRV_DIPENDENTI dip, string termId, bool isPreview)
        {
            int idDip = dip.ID_DIPENDENTE;
            string matricola = dip.MATRICOLA;

            if (!isPreview)
            {
                db.XR_PRV_DIPENDENTI_VAR.RemoveWhere(x => x.ID_DIPENDENTE == idDip && x.ID_VAR_TIPO == (int)VariabileTipoEnum.Reperibilita);
                if (!DBHelper.Save(db, "PoliticheRetributive - "))
                    return false;
            }

            //recupero variabili
            IEnumerable<HRDWVariabili> vars = db.Database.SqlQuery<HRDWVariabili>(GetQueryReperibilita(matricola)).ToList();
            foreach (var var in vars)
            {
                XR_PRV_DIPENDENTI_VAR variabile = new XR_PRV_DIPENDENTI_VAR();
                if (!isPreview) variabile.ID_VARIABILE = DBHelper.GeneraOID(db, "XR_PRV_DIPENDENTI_VAR", "ID_VARIABILE");
                variabile.ID_DIPENDENTE = idDip;
                variabile.ID_VAR_TIPO = (int)VariabileTipoEnum.Reperibilita;
                variabile.ANNO = var.Anno;
                variabile.MESE = var.Mese;
                variabile.COD_AGGR_COSTI = "";
                variabile.DES_AGGR_COSTI = "";
                variabile.COD_VOCE_CED = "";
                variabile.DES_VOCE_CED = "";
                variabile.NM_ORE = Convert.ToDouble(var.Ore);
                variabile.NM_GIORNI = Convert.ToDouble(var.Giorni);
                variabile.NM_PRESTAZIONI = Convert.ToDouble(var.NumeroPrestazioni);
                variabile.IMPORTO = var.Importo;
                variabile.COD_USER = currentPMatricola;
                variabile.COD_TERMID = termId;
                variabile.TMS_TIMESTAMP = DateTime.Now;
                dip.XR_PRV_DIPENDENTI_VAR.Add(variabile);
                if (!isPreview)
                {
                    if (!DBHelper.Save(db, "PoliticheRetributive - "))
                        return false;
                }
            }

            return true;
        }

        public static bool RecuperoProvvedimenti(string currentPMatricola, IncentiviEntities db, XR_PRV_DIPENDENTI dip, string termId, bool isPreview)
        {
            int idDip = dip.ID_DIPENDENTE;
            string matricola = dip.MATRICOLA;

            if (!isPreview)
            {
                db.XR_PRV_DIPENDENTI_PROV.RemoveWhere(x => x.ID_DIPENDENTE == idDip);
                if (!DBHelper.Save(db, "PoliticheRetributive - "))
                    return false;
            }

            //recupero aumenti
            IEnumerable<HRDWPassaggioAumento> aumenti = db.Database.SqlQuery<HRDWPassaggioAumento>(GetQueryAumenti(matricola));
            if (aumenti.Count() > 0)
            {
                foreach (HRDWPassaggioAumento aumento in aumenti.ToList())
                {
                    XR_PRV_DIPENDENTI_PROV prov = new XR_PRV_DIPENDENTI_PROV();
                    if (!isPreview) prov.ID_DIPPROV = DBHelper.GeneraOID(db, "XR_PRV_DIPENDENTI_PROV", "ID_DIPPROV");
                    prov.ID_DIPENDENTE = idDip;
                    prov.ID_PROV = (int)ProvvedimentiEnum.AumentoMerito;
                    prov.DT_PROV = new DateTime(aumento.Anno, aumento.Mese, aumento.Giorno);
                    prov.DESCRIZIONE = aumento.GetDescrizione();
                    prov.IMPORTO = aumento.Importo;
                    prov.COD_USER = currentPMatricola;
                    prov.COD_TERMID = termId;
                    prov.TMS_TIMESTAMP = DateTime.Now;
                    if (isPreview) prov.XR_PRV_PROV = db.XR_PRV_PROV.FirstOrDefault(x => x.ID_PROV == prov.ID_PROV);
                    dip.XR_PRV_DIPENDENTI_PROV.Add(prov);
                    if (!isPreview)
                    {
                        if (!DBHelper.Save(db, "PoliticheRetributive - "))
                            return false;
                    }
                        
                }
            }
            //recupero passaggi
            IEnumerable<HRDWPassaggioAumento> passaggi = db.Database.SqlQuery<HRDWPassaggioAumento>(GetQueryPassaggi(matricola));
            if (passaggi.Count() > 0)
            {
                foreach (HRDWPassaggioAumento passaggio in passaggi.ToList())
                {
                    XR_PRV_DIPENDENTI_PROV prov = new XR_PRV_DIPENDENTI_PROV();
                    if (!isPreview) prov.ID_DIPPROV = DBHelper.GeneraOID(db, "XR_PRV_DIPENDENTI_PROV", "ID_DIPPROV");
                    prov.ID_DIPENDENTE = idDip;
                    prov.ID_PROV = (int)ProvvedimentiEnum.AumentoLivello;
                    prov.DT_PROV = new DateTime(passaggio.Anno, passaggio.Mese, passaggio.Giorno);
                    prov.DESCRIZIONE = passaggio.GetDescrizione();
                    prov.IMPORTO = passaggio.Importo;
                    prov.COD_CATEGORIA = passaggio.cod_categoria;
                    prov.DESC_CATEGORIA = passaggio.desc_categoria;
                    prov.LIV_ASSEGNATO = passaggio.desc_livello;
                    prov.COD_USER = currentPMatricola;
                    prov.COD_TERMID = termId;
                    prov.TMS_TIMESTAMP = DateTime.Now;
                    if (isPreview) prov.XR_PRV_PROV = db.XR_PRV_PROV.FirstOrDefault(x => x.ID_PROV == prov.ID_PROV);
                    dip.XR_PRV_DIPENDENTI_PROV.Add(prov);
                    if (!isPreview)
                    {
                        if (!DBHelper.Save(db, "PoliticheRetributive - "))
                            return false;
                    }
                }
            }

            //recupero gratifiche
            IEnumerable<HRDWPassaggioAumento> gratifiche = db.Database.SqlQuery<HRDWPassaggioAumento>(GetQueryGratifiche(matricola));
            if (gratifiche.Count() > 0)
            {
                foreach (HRDWPassaggioAumento gratifica in gratifiche.ToList())
                {
                    XR_PRV_DIPENDENTI_PROV prov = new XR_PRV_DIPENDENTI_PROV();
                    if (!isPreview) prov.ID_DIPPROV = DBHelper.GeneraOID(db, "XR_PRV_DIPENDENTI_PROV", "ID_DIPPROV");
                    prov.ID_DIPENDENTE = idDip;
                    prov.ID_PROV = (int)ProvvedimentiEnum.Gratifica;
                    prov.DT_PROV = new DateTime(gratifica.Anno, gratifica.Mese, gratifica.Giorno);
                    prov.IMPORTO = gratifica.Importo;
                    prov.COD_USER = currentPMatricola;
                    prov.COD_TERMID = termId;
                    prov.TMS_TIMESTAMP = DateTime.Now;
                    if (isPreview) prov.XR_PRV_PROV = db.XR_PRV_PROV.FirstOrDefault(x => x.ID_PROV == prov.ID_PROV);
                    dip.XR_PRV_DIPENDENTI_PROV.Add(prov);
                    if (!isPreview)
                    {
                        if (!DBHelper.Save(db, "PoliticheRetributive - "))
                            return false;
                    }
                }
            }

            return true;
        }

        public static void RecuperoAumenti(string currentPMatricola,IncentiviEntities db, XR_PRV_DIPENDENTI dip, string termId, bool isPreview)
        {
            int idDip = dip.ID_DIPENDENTE;
            string matricola = dip.MATRICOLA;

            if (!isPreview)
            {
                db.XR_PRV_DIPENDENTI_PROV.RemoveWhere(x => x.ID_DIPENDENTE == idDip && x.ID_PROV==(int)ProvvedimentiEnum.AumentoMerito);
                DBHelper.Save(db, "PoliticheRetributive - ");
            }

            //recupero aumenti
            IEnumerable<HRDWPassaggioAumento> aumenti = db.Database.SqlQuery<HRDWPassaggioAumento>(GetQueryAumenti(matricola));
            if (aumenti.Count() > 0)
            {
                foreach (HRDWPassaggioAumento aumento in aumenti.ToList())
                {
                    XR_PRV_DIPENDENTI_PROV prov = new XR_PRV_DIPENDENTI_PROV();
                    if (!isPreview) prov.ID_DIPPROV = DBHelper.GeneraOID(db, "XR_PRV_DIPENDENTI_PROV", "ID_DIPPROV");
                    prov.ID_DIPENDENTE = idDip;
                    prov.ID_PROV = (int)ProvvedimentiEnum.AumentoMerito;
                    prov.DT_PROV = new DateTime(aumento.Anno, aumento.Mese, aumento.Giorno);
                    prov.DESCRIZIONE = aumento.GetDescrizione();
                    prov.IMPORTO = aumento.Importo;
                    prov.COD_USER = currentPMatricola;
                    prov.COD_TERMID = termId;
                    prov.TMS_TIMESTAMP = DateTime.Now;
                    if (isPreview) prov.XR_PRV_PROV = db.XR_PRV_PROV.FirstOrDefault(x => x.ID_PROV == prov.ID_PROV);
                    dip.XR_PRV_DIPENDENTI_PROV.Add(prov);
                    if (!isPreview) DBHelper.Save(db, "PoliticheRetributive - ");

                }
            }
        }

        public static void RecuperoPassaggi(string currentPMatricola, IncentiviEntities db, XR_PRV_DIPENDENTI dip, string termId, bool isPreview)
        {
            int idDip = dip.ID_DIPENDENTE;
            string matricola = dip.MATRICOLA;

            if (!isPreview)
            {
                db.XR_PRV_DIPENDENTI_PROV.RemoveWhere(x => x.ID_DIPENDENTE == idDip && x.ID_PROV == (int)ProvvedimentiEnum.AumentoLivello);
                DBHelper.Save(db, "PoliticheRetributive - ");
            }

            //recupero passaggi
            IEnumerable<HRDWPassaggioAumento> passaggi = db.Database.SqlQuery<HRDWPassaggioAumento>(GetQueryPassaggi(matricola));
            if (passaggi.Count() > 0)
            {
                foreach (HRDWPassaggioAumento passaggio in passaggi.ToList())
                {
                    XR_PRV_DIPENDENTI_PROV prov = new XR_PRV_DIPENDENTI_PROV();
                    if (!isPreview) prov.ID_DIPPROV = DBHelper.GeneraOID(db, "XR_PRV_DIPENDENTI_PROV", "ID_DIPPROV");
                    prov.ID_DIPENDENTE = idDip;
                    prov.ID_PROV = (int)ProvvedimentiEnum.AumentoLivello;
                    prov.DT_PROV = new DateTime(passaggio.Anno, passaggio.Mese, passaggio.Giorno);
                    prov.DESCRIZIONE = passaggio.GetDescrizione();
                    prov.IMPORTO = passaggio.Importo;
                    prov.COD_CATEGORIA = passaggio.cod_categoria;
                    prov.DESC_CATEGORIA = passaggio.desc_categoria;
                    prov.LIV_ASSEGNATO = passaggio.desc_livello;
                    prov.COD_USER = currentPMatricola;
                    prov.COD_TERMID = termId;
                    prov.TMS_TIMESTAMP = DateTime.Now;
                    if (isPreview) prov.XR_PRV_PROV = db.XR_PRV_PROV.FirstOrDefault(x => x.ID_PROV == prov.ID_PROV);
                    dip.XR_PRV_DIPENDENTI_PROV.Add(prov);
                    if (!isPreview) DBHelper.Save(db, "PoliticheRetributive - ");
                }
            }
        }

        private static void CalcoloCostoManuale(IncentiviEntities db, XR_PRV_DIPENDENTI dip, ProvvedimentiEnum provv)
        {
            var customProv = dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.ID_PROV == (int)provv);
            if (customProv != null)
            {
                if (customProv.DIFF_RAL > 0)// && customProv.COSTO_PERIODO==0)
                {
                    if (dip.DECORRENZA.Value.Year > DateTime.Today.Year) return;

                    decimal costoPeriodo = 0;
                    if (customProv.ID_PROV != (int)ProvvedimentiEnum.CUSGratifica)
                        costoPeriodo = customProv.COSTO_ANNUO / 14 * ((13 - dip.DECORRENZA.Value.Month) + Convert.ToDecimal((dip.DECORRENZA.Value.Month < 7 ? 2.5 : 1.5)));
                    else
                    {
                        var aliq = HRDWData.GetProvvAliq(db, (int)ProvvedimentiEnum.Gratifica, dip.SINTESI1.COD_QUALIFICA);
                        costoPeriodo = customProv.DIFF_RAL + (customProv.DIFF_RAL * Convert.ToDecimal(aliq) / 100);
                    }

                    costoPeriodo = Decimal.Round(costoPeriodo, 2);

                    if (costoPeriodo != customProv.COSTO_PERIODO)
                    {
                        customProv.COSTO_PERIODO = costoPeriodo;
                        DBHelper.Save(db, "Politiche retributive - ");
                    }
                }
            }
        }

        private static void CalcoloCostoSingolo(string currentPMatricola, IncentiviEntities db, XR_PRV_DIPENDENTI dip, ProvvedimentiEnum provvedimento, 
            string cat, bool isPreview, string termId,
            decimal valueDiff, decimal somma80p, string livelloAttuale, string desLivCategoria, string codCategoria, 
            string indAqc, string indPerse, string indDelta,
            DateTime? dataDecorrenza = null)
        {
            bool isNew = false;

            decimal aliq = GetProvvAliq(db, (int)provvedimento, cat);
            XR_PRV_DIPENDENTI_VARIAZIONI var = dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.ID_PROV == (int)provvedimento);
            isNew = var == null;
            if (isNew)
            {
                var = new XR_PRV_DIPENDENTI_VARIAZIONI();
                if (!isPreview) var.ID_DIP_COSTO = DBHelper.GeneraOID(db, "XR_PRV_DIPENDENTI_VARIAZIONI", "ID_DIP_COSTO");
            }

            var.ID_DIPENDENTE = dip.ID_DIPENDENTE;
            var.ID_PROV = (int)provvedimento;
            var.DIFF_RAL = valueDiff;
            var.COSTO_ANNUO = var.DIFF_RAL + (var.DIFF_RAL * aliq / 100);

            if (dip.DECORRENZA.HasValue)
            {
                // se la data di decorrenza è riferita ad un anno successivo al corrente
                // allora il costo di periodo sarà pari a zero
                DateTime oggi = DateTime.Now;
                int annoCorrente = oggi.Year;
                int annoSelezionato = dip.DECORRENZA.Value.Year;

                if (annoSelezionato > annoCorrente)
                {
                    var.COSTO_PERIODO = 0;
                    if (CommonHelper.GetParametro<bool>(EnumParametriSistema.PolRetrCostoAnnuo))
                        var.COSTO_ANNUO = var.COSTO_ANNUO / 14 * ((13 - dip.DECORRENZA.Value.Month) + Convert.ToDecimal((dip.DECORRENZA.Value.Month < 7 ? 2 : 1)));
                }
                else
                {
                    var.COSTO_PERIODO = var.COSTO_ANNUO / 14 * ((13 - dip.DECORRENZA.Value.Month) + Convert.ToDecimal((dip.DECORRENZA.Value.Month < 7 ? 2.5 : 1.5)));
                }

                if (dip.CUSTOM_PROV.GetValueOrDefault())
                {
                    var varManuale = dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.XR_PRV_PROV.BASE_PROV == (int)provvedimento);
                    if (varManuale != null)
                    {
                        if (annoSelezionato > annoCorrente)
                        {
                            varManuale.COSTO_PERIODO = 0;
                            if (CommonHelper.GetParametro<bool>(EnumParametriSistema.PolRetrCostoAnnuo))
                                varManuale.COSTO_ANNUO = var.COSTO_ANNUO / 14 * ((13 - dip.DECORRENZA.Value.Month) + Convert.ToDecimal((dip.DECORRENZA.Value.Month < 7 ? 2 : 1)));
                        }
                        else
                        {
                            varManuale.COSTO_PERIODO = var.COSTO_ANNUO / 14 * ((13 - dip.DECORRENZA.Value.Month) + Convert.ToDecimal((dip.DECORRENZA.Value.Month < 7 ? 2.5 : 1.5)));
                        }
                        varManuale.COSTO_PERIODO = Decimal.Round(varManuale.COSTO_PERIODO, 2);
                    }
                }
            }
               
            var.COSTO_REC_STR = var.DIFF_RAL > 0 ? (somma80p + (somma80p * aliq / 100)) : 0;
            var.LIV_ATTUALE = livelloAttuale;

            if (isNew || desLivCategoria.Trim()!="NON CODIFICATO")
                var.LIV_PREVISTO = desLivCategoria;

            if (isNew || codCategoria.Trim() != "NON CODIFICATO")
                var.CAT_PREVISTA = codCategoria;

            var.INDENNITA_ACQUISITE = indAqc;
            var.INDENNITA_PERSE = indPerse;
            var.INDENNITA_DELTA = indDelta;

            if (isPreview) var.XR_PRV_PROV = db.XR_PRV_PROV.FirstOrDefault(x => x.ID_PROV == var.ID_PROV);
            if (isNew)
            {
                var.COD_USER = currentPMatricola;
                var.COD_TERMID = termId;
                var.TMS_TIMESTAMP = DateTime.Now;
                dip.XR_PRV_DIPENDENTI_VARIAZIONI.Add(var);
            }

            if (!isPreview)
            {
                DBHelper.Save(db, "PoliticheRetributive - ");
            }
        }

        private static void CalcoloCostoGratifica(string currentPMatricola, IncentiviEntities db, string termId, XR_PRV_DIPENDENTI dip, bool isPreview, string matricola, DateTime rif, string cat, string codPartTime, DateTime? dataDecorrenza = null)
        {
            bool isNew = false;
            decimal aliq = GetProvvAliq(db, (int)ProvvedimentiEnum.Gratifica, cat);

            var grat = db.XR_PRV_PROV_IMPORTO.FirstOrDefault(x => x.ID_PROV == 3 && x.LIVELLO.Contains(dip.LIV_ATTUALE) 
            && (x.CAT_INCLUSE==null || x.CAT_INCLUSE.Contains(cat)) 
            && (x.CAT_ESCLUSE==null || !x.CAT_ESCLUSE.Contains(cat)));
                       
            XR_PRV_DIPENDENTI_VARIAZIONI var = var = dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.ID_PROV == (int)ProvvedimentoEnum.Gratifica);
            isNew = var == null;
            if (isNew)
            {
                var = new XR_PRV_DIPENDENTI_VARIAZIONI();
                if (!isPreview) var.ID_DIP_COSTO = DBHelper.GeneraOID(db, "XR_PRV_DIPENDENTI_VARIAZIONI", "ID_DIP_COSTO");
            }
            var.ID_DIPENDENTE = dip.ID_DIPENDENTE;
            var.ID_PROV = (int)ProvvedimentiEnum.Gratifica;
            if (grat != null)
            {
                if (codPartTime == "$")
                {
                    var.DIFF_RAL = grat.IMPORTO;
                }
                else
                {
                    IEnumerable<HRDWPartTime> pt = db.Database.SqlQuery<HRDWPartTime>(GetQueryPartTime(matricola, rif.Year, rif.Month)).ToList();
                    if (codPartTime == "O")
                    {
                        var.DIFF_RAL = pt.ElementAt(0).UnitaAnno * grat.IMPORTO;
                    }   
                    else
                    {
                        var.DIFF_RAL = grat.IMPORTO / 100 * pt.ElementAt(0).PercPartTime;
                    }
                        
                    if (var.DIFF_RAL < 1000)
                        var.DIFF_RAL = 1000;
                    else
                        var.DIFF_RAL = Math.Truncate(var.DIFF_RAL / 100) * 100;
                }

                var.COSTO_ANNUO = 0;


                var.COSTO_PERIODO = var.DIFF_RAL + (var.DIFF_RAL * Convert.ToDecimal(aliq) / 100);

                if (dip.DECORRENZA.HasValue)
                {
                    // se la data di decorrenza è riferita ad un anno successivo al corrente
                    // allora il costo di periodo sarà pari a zero
                    DateTime oggi = DateTime.Now;
                    int annoCorrente = oggi.Year;
                    int annoSelezionato = dip.DECORRENZA.Value.Year;

                    if (annoSelezionato > annoCorrente)
                    {
                        var.COSTO_PERIODO = 0;
                    }
                }                
            }
            var.COSTO_REC_STR = var.COSTO_ANNUO;
            if (isPreview) var.XR_PRV_PROV = db.XR_PRV_PROV.FirstOrDefault(x => x.ID_PROV == var.ID_PROV);

            if (isNew)
            {
                var.COD_USER = currentPMatricola;
                var.COD_TERMID = termId;
                var.TMS_TIMESTAMP = DateTime.Now;
                dip.XR_PRV_DIPENDENTI_VARIAZIONI.Add(var);
            }

            if (!isPreview)
            {
                DBHelper.Save(db, "PoliticheRetributive - ");
            }
        }

        public static void CalcoloCosti(string currentPMatricola, IncentiviEntities db, string termId, XR_PRV_DIPENDENTI dip, bool isPreview, bool updateManual= false)
        {
            string matricola = dip.MATRICOLA;

            DateTime rif = DateTime.Today;
            IEnumerable<HRDWVariazioni> variazioni = db.Database.SqlQuery<HRDWVariazioni>(GetQuerySimulazioni("'"+matricola+"'", rif.Year, rif.Month)).ToList();
            if (variazioni == null || variazioni.Count() == 0)
            {
                rif = rif.AddMonths(-1);
                variazioni = db.Database.SqlQuery<HRDWVariazioni>(GetQuerySimulazioni("'"+matricola+"'", rif.Year, rif.Month)).ToList();
            }

            if (variazioni != null && variazioni.Count() > 0)
            {
                string cat = dip.SINTESI1.COD_QUALIFICA;

                HRDWVariazioni var = variazioni.ElementAt(0);

                dip.RAL_ATTUALE = var.Ral_Annua;
                dip.LIV_ATTUALE = var.LivelloAttuale.Trim();
                dip.PART_TIME = var.DesPartTime;

                string livAtt = var.LivelloAttuale;
                string codCat = var.Cod_Categoria;
                string desLivCat = var.Des_Liv_Categoria;

                string codPartTime = var.CodPartTime;

                decimal passAss = var.Diff_ral_passaggio_assorbimento.GetValueOrDefault();
                decimal merito = var.Diff_ral_merito.GetValueOrDefault();
                decimal passaggio = var.Diff_ral_passaggio.GetValueOrDefault();

                string indAcq = var.desc_indennita_importi_acquisiti;
                string indPerse = var.desc_indennita_importi_persi;
                string indDelta = var.desc_indennita_importi_delta;

                var rifDate = DateTime.Today.AddMonths(-12);

                var sommaTmp = dip.XR_PRV_DIPENDENTI_VAR.Where(x => (x.ANNO == rifDate.Year && x.MESE >= rifDate.Month) || (x.ANNO == DateTime.Today.Year)).Sum(x => x.IMPORTO);
                var somma80p = sommaTmp * 80 / 100;

                CalcoloCostoSingolo(currentPMatricola, db, dip, ProvvedimentiEnum.AumentoLivello, cat, isPreview, termId, passAss, somma80p, livAtt, desLivCat, codCat, indAcq, indPerse, indDelta);
                CalcoloCostoSingolo(currentPMatricola, db, dip, ProvvedimentiEnum.AumentoMerito, cat, isPreview, termId, merito, somma80p, livAtt, desLivCat, codCat, "", "", "");
                CalcoloCostoSingolo(currentPMatricola, db, dip, ProvvedimentiEnum.AumentoLivelloNoAssorbimento, cat, isPreview, termId, passaggio, somma80p, livAtt, desLivCat, codCat, indAcq, indPerse, indDelta);
                CalcoloCostoGratifica(currentPMatricola, db, termId, dip, isPreview, matricola, rif, cat, codPartTime);

                if (updateManual && dip.CUSTOM_PROV.GetValueOrDefault())
                {
                    CalcoloCostoManuale(db, dip, ProvvedimentiEnum.CUSAumentoLivello);
                    CalcoloCostoManuale(db, dip, ProvvedimentiEnum.CUSAumentoMerito);
                    CalcoloCostoManuale(db, dip, ProvvedimentiEnum.CUSAumentoLivelloNoAssorbimento);
                    CalcoloCostoManuale(db, dip, ProvvedimentiEnum.CUSGratifica);
                }
            }
        }       

        public static void CalcoloCostiMassivo(string currentPMatricola, IncentiviEntities db, string termId, string elencoMatricole="", bool calcLivello=true, bool calcMerito = true, bool calcLivNoAss = true, bool calcGratifica=true, DateTime? dataDecorrenza = null, bool isPreview = false, bool updateManual = false)
        {
            //int conteggioFrancesco = 0;
            //int conteggioDipendenti = 0;
            //List<string> matricole = new List<string>();
            //string esitoCalcolo = "";

            if (String.IsNullOrWhiteSpace(elencoMatricole))
                elencoMatricole = " select matricola from xr_prv_dipendenti ";
            
            DateTime rif = DateTime.Today;
            IEnumerable<HRDWVariazioni> variazioni = db.Database.SqlQuery<HRDWVariazioni>(GetQuerySimulazioni(elencoMatricole, rif.Year, rif.Month)).ToList();
            if (variazioni == null || variazioni.Count() == 0)
            {
                rif = rif.AddMonths(-1);
                variazioni = db.Database.SqlQuery<HRDWVariazioni>(GetQuerySimulazioni(elencoMatricole, rif.Year, rif.Month)).ToList();
            }

            if (variazioni.Count()>0)
            {
                foreach (var variazione in variazioni)
                {
                    string matricola = variazione.Matricola;
                    foreach (var dip in db.XR_PRV_DIPENDENTI.Where(x=>x.MATRICOLA==matricola).ToList())
                    {
                        bool isCustom = dip.CUSTOM_PROV.GetValueOrDefault();

                        if (dataDecorrenza.HasValue)
                        {
                            dip.DECORRENZA = dataDecorrenza.Value;
                        }

                        string cat = dip.SINTESI1.COD_QUALIFICA;

                        dip.RAL_ATTUALE = variazione.Ral_Annua;
                        dip.LIV_ATTUALE = variazione.LivelloAttuale.Trim();
                        dip.PART_TIME = variazione.DesPartTime;

                        string livAtt = variazione.LivelloAttuale;
                        string codCat = variazione.Cod_Categoria;
                        string desLivCat = variazione.Des_Liv_Categoria;

                        string codPartTime = variazione.CodPartTime;

                        decimal passAss = variazione.Diff_ral_passaggio_assorbimento.GetValueOrDefault();
                        decimal merito = variazione.Diff_ral_merito.GetValueOrDefault();
                        decimal passaggio = variazione.Diff_ral_passaggio.GetValueOrDefault();

                        string indAcq = variazione.desc_indennita_importi_acquisiti;
                        string indPerse = variazione.desc_indennita_importi_persi;
                        string indDelta = variazione.desc_indennita_importi_delta;

                        var rifDate = DateTime.Today.AddMonths(-12);

                        var sommaTmp = dip.XR_PRV_DIPENDENTI_VAR.Where(x => (x.ANNO == rifDate.Year && x.MESE >= rifDate.Month) || (x.ANNO == DateTime.Today.Year)).Sum(x => x.IMPORTO);
                        var somma80p = sommaTmp * 80 / 100;

                        if (calcLivello)
                        {
                            CalcoloCostoSingolo(currentPMatricola, db, dip, ProvvedimentiEnum.AumentoLivello, cat, isPreview, termId, passAss, somma80p, livAtt, desLivCat, codCat, indAcq, indPerse, indDelta, dataDecorrenza);
                            if (!isPreview && updateManual && isCustom)
                                CalcoloCostoManuale(db, dip, ProvvedimentiEnum.CUSAumentoLivello);
                        }

                        if (calcMerito)
                        {
                            CalcoloCostoSingolo(currentPMatricola, db, dip, ProvvedimentiEnum.AumentoMerito, cat, isPreview, termId, merito, somma80p, livAtt, desLivCat, codCat, "", "", "", dataDecorrenza);
                            if (!isPreview && updateManual && isCustom)
                                CalcoloCostoManuale(db, dip, ProvvedimentiEnum.CUSAumentoMerito);
                        }

                        if (calcLivNoAss)
                        {
                            CalcoloCostoSingolo(currentPMatricola, db, dip, ProvvedimentiEnum.AumentoLivelloNoAssorbimento, cat, isPreview, termId, passaggio, somma80p, livAtt, desLivCat, codCat, indAcq, indPerse, indDelta, dataDecorrenza);
                            if (!isPreview && updateManual && isCustom)
                                CalcoloCostoManuale(db, dip, ProvvedimentiEnum.CUSAumentoLivelloNoAssorbimento);
                        }

                        if (calcGratifica)
                        {
                            CalcoloCostoGratifica(currentPMatricola, db, termId, dip, isPreview, matricola, rif, cat, codPartTime, dataDecorrenza);
                            if (!isPreview && updateManual && isCustom)
                                CalcoloCostoManuale(db, dip, ProvvedimentiEnum.CUSGratifica);
                        }

                    }
                }
            }
        }


        public static void ScriviIndennita(IncentiviEntities db)
        {
            string elencoMatricole = " select matricola from xr_prv_dipendenti ";

            DateTime rif = DateTime.Today;
            IEnumerable<HRDWVariazioni> variazioni = db.Database.SqlQuery<HRDWVariazioni>(GetQuerySimulazioni(elencoMatricole, rif.Year, rif.Month)).ToList();
            if (variazioni == null || variazioni.Count() == 0)
            {
                rif = rif.AddMonths(-1);
                variazioni = db.Database.SqlQuery<HRDWVariazioni>(GetQuerySimulazioni(elencoMatricole, rif.Year, rif.Month)).ToList();
            }

            if (variazioni.Count() > 0)
            {
                foreach (var variazione in variazioni)
                {
                    bool hasChanges = false;
                    string matricola = variazione.Matricola;
                    foreach (var dip in db.XR_PRV_DIPENDENTI.Where(x => x.MATRICOLA == matricola).ToList())
                    {
                        string indAcq = variazione.desc_indennita_importi_acquisiti;
                        string indPerse = variazione.desc_indennita_importi_persi;
                        string indDelta = variazione.desc_indennita_importi_delta;


                        if (!String.IsNullOrWhiteSpace(indAcq) || !String.IsNullOrWhiteSpace(indPerse) || !String.IsNullOrWhiteSpace(indDelta))
                        {
                            hasChanges = true;
                            foreach (var item in dip.XR_PRV_DIPENDENTI_VARIAZIONI.Where(x => x.ID_PROV == 1 || x.ID_PROV == 4))
                            {
                                item.INDENNITA_ACQUISITE = indAcq;
                                item.INDENNITA_PERSE = indPerse;
                                item.INDENNITA_DELTA = indDelta;
                            }
                        }
                    }

                    if (hasChanges)
                        DBHelper.Save(db, "Pol. Retr. - Acq. Ind - ");
                }

                
            }
        }

        public static void CorreggiLivello(IncentiviEntities db, string termId)
        {
            string matricola = " select dip.matricola " +
                    " from xr_prv_dipendenti dip " +
                    " join xr_prv_dipendenti_variazioni variazione on dip.id_dipendente = variazione.id_dipendente and variazione.id_prov = dip.id_prov_effettivo "+ 
                    " where " +
                    " dip.id_prov_effettivo in (1, 4, 6, 7) " +
                    " and(variazione.cat_prevista is null or variazione.cat_prevista = '$$$') " +
                    " group by dip.matricola ";
                

            //if (!isPreview)
            //{
            //    db.XR_PRV_DIPENDENTI_VARIAZIONI.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE && (x.XR_PRV_PROV.CUSTOM == null || x.XR_PRV_PROV.CUSTOM == false));
            //    DBHelper.Save(db,"PoliticheRetributive - ") ;
            //}

            DateTime rif = DateTime.Today;
            IEnumerable<HRDWVariazioni> variazioni = db.Database.SqlQuery<HRDWVariazioni>(GetQuerySimulazioni(matricola, rif.Year, rif.Month)).ToList();
            if (variazioni == null || variazioni.Count() == 0)
            {
                rif = rif.AddMonths(-1);
                variazioni = db.Database.SqlQuery<HRDWVariazioni>(GetQuerySimulazioni(matricola, rif.Year, rif.Month)).ToList();
            }

            if (variazioni != null && variazioni.Count() > 0)
            {
                foreach (var variazione in variazioni)
                {
                    if (!String.IsNullOrWhiteSpace(variazione.Des_Liv_Categoria) && variazione.Des_Liv_Categoria != "NON CODIFICATO")
                    {
                        var dips = db.XR_PRV_DIPENDENTI.Where(x => x.MATRICOLA == variazione.Matricola).ToList();
                        foreach (var dip in dips)
                        {
                            bool hasChanges = false;

                            var varDB = dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.ID_PROV == (int)ProvvedimentiEnum.AumentoLivello);
                            if (varDB != null)
                            {
                                if (String.IsNullOrWhiteSpace(varDB.LIV_PREVISTO) || varDB.LIV_PREVISTO == "NON CODIFICATO")
                                {
                                    varDB.LIV_PREVISTO = variazione.Des_Liv_Categoria;
                                    hasChanges = true;
                                }
                                if (String.IsNullOrWhiteSpace(varDB.CAT_PREVISTA) || varDB.CAT_PREVISTA == "$$$")
                                {
                                    varDB.CAT_PREVISTA = variazione.Cod_Categoria;
                                    hasChanges = true;
                                }
                            }

                            varDB = dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.ID_PROV == (int)ProvvedimentiEnum.AumentoLivelloNoAssorbimento);
                            if (varDB != null)
                            {
                                if (String.IsNullOrWhiteSpace(varDB.LIV_PREVISTO) || varDB.LIV_PREVISTO == "NON CODIFICATO")
                                {
                                    varDB.LIV_PREVISTO = variazione.Des_Liv_Categoria;
                                    hasChanges = true;
                                }
                                if (String.IsNullOrWhiteSpace(varDB.CAT_PREVISTA) || varDB.CAT_PREVISTA == "$$$")
                                {
                                    varDB.CAT_PREVISTA = variazione.Cod_Categoria;
                                    hasChanges = true;
                                }
                            }

                            varDB = dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.ID_PROV == (int)ProvvedimentiEnum.CUSAumentoLivello);
                            if (varDB != null)
                            {
                                if (String.IsNullOrWhiteSpace(varDB.LIV_PREVISTO) || varDB.LIV_PREVISTO == "NON CODIFICATO")
                                {
                                    varDB.LIV_PREVISTO = variazione.Des_Liv_Categoria;
                                    hasChanges = true;
                                }
                                if (String.IsNullOrWhiteSpace(varDB.CAT_PREVISTA) || varDB.CAT_PREVISTA == "$$$")
                                {
                                    varDB.CAT_PREVISTA = variazione.Cod_Categoria;
                                    hasChanges = true;
                                }
                            }

                            varDB = dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.ID_PROV == (int)ProvvedimentiEnum.CUSAumentoLivelloNoAssorbimento);
                            if (varDB != null)
                            {
                                if (String.IsNullOrWhiteSpace(varDB.LIV_PREVISTO) || varDB.LIV_PREVISTO == "NON CODIFICATO")
                                {
                                    varDB.LIV_PREVISTO = variazione.Des_Liv_Categoria;
                                    hasChanges = true;
                                }
                                if (String.IsNullOrWhiteSpace(varDB.CAT_PREVISTA) || varDB.CAT_PREVISTA == "$$$")
                                {
                                    varDB.CAT_PREVISTA = variazione.Cod_Categoria;
                                    hasChanges = true;
                                }
                            }

                            if (hasChanges)
                            {
                                try
                                {
                                    db.SaveChanges();
                                }
                                catch (Exception)
                                {

                                    throw;
                                }
                            }
                        }
                    }
                }
            }
        }


        public static bool getOrganico(List<int> anni, out List<HRDWDirezione> orgDirezioni)
        {
            bool result = false;

            orgDirezioni = new List<HRDWDirezione>();
            List<HRDWComodo> listGratifiche = null;
            List<HRDWComodo> listAumento = null;
            List<HRDWComodo> listPassaggi = null;

            PolRetrParam param = HrisHelper.GetParametroJson<PolRetrParam>(HrisParam.PoliticheParametri);

            using (IncentiviEntities db = new IncentiviEntities())
            {
                try
                {
                    DateTime rif = DateTime.Today;
                    orgDirezioni = db.Database.SqlQuery<HRDWDirezione>(GetQueryOrganico(rif.Year, rif.Month)).ToList();
                    if (orgDirezioni != null)
                    {
                        rif = rif.AddMonths(-1);
                        orgDirezioni = db.Database.SqlQuery<HRDWDirezione>(GetQueryOrganico(rif.Year, rif.Month)).ToList();
                    }

                    if (anni != null)
                    {
                        foreach (var anno in anni)
                        {
                            listGratifiche = db.Database.SqlQuery<HRDWComodo>(GetQueryGratificheServ(anno.ToString())).ToList();
                            listAumento = db.Database.SqlQuery<HRDWComodo>(GetQueryAumentiServ(anno.ToString())).ToList();
                            listPassaggi = db.Database.SqlQuery<HRDWComodo>(GetQueryPassaggiServ(anno.ToString())).ToList();
                            foreach (var dir in orgDirezioni)
                            {
                                dir.Gratifiche.Add(new HRDWPolRetr()
                                {
                                    Anno = anno,
                                    Numero = listGratifiche.Any(x => x.Codice == dir.Codice) ? listGratifiche.First(x => x.Codice == dir.Codice).Numero : 0
                                });
                                dir.Aumenti.Add(new HRDWPolRetr()
                                {
                                    Anno = anno,
                                    Numero = listAumento.Any(x => x.Codice == dir.Codice) ? listAumento.FirstOrDefault(x => x.Codice == dir.Codice).Numero : 0
                                });
                                dir.Passaggi.Add(new HRDWPolRetr()
                                {
                                    Anno = anno,
                                    Numero = listPassaggi.Any(x => x.Codice == dir.Codice) ? listPassaggi.FirstOrDefault(x => x.Codice == dir.Codice).Numero : 0
                                });
                            }
                        }
                    }

                    //Serve a gestire casi come Bolzano (64), che deve rientrare sotto il CSR (24)
                    if (param!=null && param.OvverideDir!=null && param.OvverideDir.Any())
                    {
                        foreach (var rule in param.OvverideDir)
                        {
                            var dirOrig = orgDirezioni.FirstOrDefault(x => x.Codice == rule.DirOrig);
                            var dirDest = orgDirezioni.FirstOrDefault(x => x.Codice == rule.DirDest);

                            if (dirOrig!=null && dirDest!=null)
                            {
                                dirDest.Organico += dirOrig.Organico;
                                dirDest.OrganicoMaschile += dirOrig.OrganicoMaschile;
                                dirDest.OrganicoFemminile += dirOrig.OrganicoFemminile;
                                
                                if (anni!=null)
                                {
                                    foreach (var anno in anni)
                                    {
                                        dirDest.Gratifiche.FirstOrDefault(x => x.Anno == anno).Numero += dirOrig.Gratifiche.FirstOrDefault(x => x.Anno == anno).Numero;
                                        dirDest.Aumenti.FirstOrDefault(x => x.Anno == anno).Numero += dirOrig.Aumenti.FirstOrDefault(x => x.Anno == anno).Numero;
                                        dirDest.Passaggi.FirstOrDefault(x => x.Anno == anno).Numero += dirOrig.Passaggi.FirstOrDefault(x => x.Anno == anno).Numero;
                                    }
                                }

                                orgDirezioni.Remove(dirOrig);
                            }
                        }
                    }

                    result = true;
                }
                catch (Exception)
                {

                }
            }

            return result;
        }

        public static bool getOrganicoCat(string catIncluse, string catEscluse, int year, int month, out List<HRDWDirezione> orgDirezioni)
        {
            bool result = false;

            orgDirezioni = new List<HRDWDirezione>();
            List<HRDWDirezione> tmp = new List<HRDWDirezione>();

            bool allCatIncluse = String.IsNullOrWhiteSpace(catIncluse) || catIncluse == "*";
            List<string> listCatIncluse = new List<string>();
            if (!allCatIncluse)
                listCatIncluse.AddRange(catIncluse.Split(','));

            bool allCatEscluse = String.IsNullOrWhiteSpace(catEscluse) || catEscluse == "*";
            List<string> listCatEscluse = new List<string>();
            if (!allCatEscluse)
                listCatEscluse.AddRange(catEscluse.Split(','));

            using (IncentiviEntities db = new IncentiviEntities())
            {
                try
                {
                    tmp = db.Database.SqlQuery<HRDWDirezione>(GetQueryOrganicoCat(year, month)).ToList();
                    orgDirezioni.AddRange(tmp.GroupBy(x => x.Codice).Select(y => new HRDWDirezione()
                    {
                        Codice = y.Key,
                        OrganicoInteressato = y.Where(z => listCatIncluse.Any(a => z.Categoria.StartsWith(a))
                            && !listCatEscluse.Any(b => z.Categoria.StartsWith(b))).Sum(c => c.Organico),
                        Categoria = String.Join(",", y.Where(z => listCatIncluse.Any(a => z.Categoria.StartsWith(a))
                             && !listCatEscluse.Any(b => z.Categoria.StartsWith(b))).Select(x => x.Categoria))
                    }));
                    
                    result = true;
                }
                catch (Exception)
                {

                }
            }

            return result;
        }

        public static IEnumerable<HRDWRisorsa> GetRisorse(string matricola, string cognome, string nome, string servizio)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                return db.Database.SqlQuery<HRDWRisorsa>(GetQueryOrganicoPerRicerca(matricola, cognome, nome, servizio));
            }
        }

        public static List<string> GetMatricoleApprendisti()
        {
            List<string> result = new List<string>();
            using (IncentiviEntities db = new IncentiviEntities())
            {
                var tmp =db.Database.SqlQuery<string>(GetQueryApprendisti());
                if (tmp != null)
                    result.AddRange(tmp);
            }
            return result;
        }

        #region Query
        private static string GetQueryOrganico(int anno, int mese)
        {
            string query =
                    " SELECT t5.[cod_serv_cont] as Codice, " +
                    " 	count(*) as Organico, " +
                    " SUM(case when t0.sesso = 'M' then 1 else 0 end) As OrganicoMaschile, " +
                    " SUM(case when t0.sesso = 'F' then 1 else 0 end) As OrganicoFemminile " +
                    " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SERVIZIO_CONTABILE] t5 ON (t1.[SKY_servizio_contabile] = t5.[sky_serv_cont]) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t7 ON (t1.[SKY_MESE_CONTABILE] = t7.[sky_tempo]) " +
                    " WHERE (t5.societa ='RAI' " +
                    " and t7.[num_anno] = " + anno + " " +
                    " and t7.[cod_mese] = " + mese + ") " +
                    " GROUP BY t5.[cod_serv_cont], " +
                    " 	t5.[desc_serv_cont] " +
                    " ORDER BY t5.cod_serv_cont, " +
                    " 	t5.desc_serv_cont ";
            return query;
        }
        private static string GetQueryOrganicoCat(int anno, int mese)
        {
            string query =
                    " SELECT t5.[cod_serv_cont] as Codice, " +
                    "   t10.[cod_categoria] as Categoria, " + 
                    " 	count(*) as Organico, " +
                    " SUM(case when t0.sesso = 'M' then 1 else 0 end) As OrganicoMaschile, " +
                    " SUM(case when t0.sesso = 'F' then 1 else 0 end) As OrganicoFemminile " +
                    " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SERVIZIO_CONTABILE] t5 ON (t1.[SKY_servizio_contabile] = t5.[sky_serv_cont]) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t7 ON (t1.[SKY_MESE_CONTABILE] = t7.[sky_tempo]) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA] t10 ON (t1.[SKY_CATEGORIA] = t10.[sky_CATEGORIA]) " +
                    " WHERE (t5.societa ='RAI' " +
                    " and t7.[num_anno] = " + anno + " " +
                    " and t7.[cod_mese] = " + mese + ") " +
                    " GROUP BY t5.[cod_serv_cont], " +
                    " 	t10.[cod_categoria] " +
                    " ORDER BY t5.cod_serv_cont, " +
                    " 	t10.cod_categoria ";
            return query;
        }
        private static string GetQueryMatricoleInServizio()
        {
            string query =
                " SELECT a1.matricola_dp " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] a1 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] a2 ON (a2.SKY_MATRICOLA = a1.sky_anagrafica_unica) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SERVIZIO_CONTABILE] a4 ON (a2.SKY_servizio_contabile = a4.sky_serv_cont) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] a5 ON (a2.SKY_MESE_CONTABILE = a5.sky_tempo) " +
                " WHERE ( " +
                " 		a5.num_anno = " + DateTime.Today.Year + " " +
                " 		and a5.cod_mese = " + DateTime.Today.Month + " " +
                " 		AND a4.societa IN ('RAI') " +
                " 		AND (a2.num_fine_mese = 1) " +
                " 		) " +
                " GROUP BY a1.matricola_dp " +
                " HAVING sum(a2.numero) > 0 ";
            return query;
        }
        private static string GetQueryInfoMatricola(string matricola)
        {
            string query =
                " SELECT t0.[matricola_dp] as Matricola, " +
                "   t6.[cod_sede] as CodSede, " +
                "   t6.[des_sede] as DesSede, " +
                " 	t2.[cod_part_time] as CodPartTime, " +
                " 	t2.[desc_part_time] as DesPartTime, " +
                " 	t10.[desc_livello] as DesLivello, " +
                " 	avg(t8.[tot_retrib_annua]) as RetribAnnua" +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_PART_TIME] t2 ON (t1.[SKY_PART_TIME] = t2.[sky_part_time]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t7 ON (t1.[SKY_MESE_CONTABILE] = t7.[sky_tempo]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE_IMPORTI] t8 ON (t8.[SKY_riga_te] = t1.[SKY_riga_te]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] t10 ON (t10.[sky_livello_categ] = t1.[sky_livello_categ]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SEZIONE] t6 ON (t1.[SKY_sezione] = t6.[sky_sezione]) " +
                " WHERE ( " +
                " 		t7.[cod_anno_mese_7] = '" + DateTime.Today.AddMonths(-1).ToString("yyyy/MM") + "' " +
                " 		AND t0.[matricola_dp] = '" + matricola + "' " +
                " 		) " +
                " GROUP BY t0.[matricola_dp], " +
                "   t6.[cod_sede], " +
                "   t6.[des_sede], " +
                " 	t2.[cod_part_time], " +
                " 	t2.[desc_part_time], " +
                " 	t10.[desc_livello] ";
            return query;
        }
        private static string GetQuerySimulazioni(string matricola, int year, int month)
        {
            string query =
                " SELECT t0.matricola_dp as Matricola, " +
                "   t13.desc_livello as LivelloAttuale, " +
                "   t14.[cod_part_time] as CodPartTime, " +
                "   t14.[desc_part_time] as DesPartTime, " +
                "   t9.tot_retrib_annua as Ral_Annua, " +
                " 	t9.diff_ral_merito as Diff_ral_merito, " +
                " 	t9.diff_ral_passaggio as Diff_ral_passaggio, " +
                " 	t9.diff_ral_passaggio_assorbito as Diff_ral_passaggio_assorbimento, " +
                "   L2D_CATEGORIA_PREVISION.cod_categoria as Cod_Categoria, " +
                "   L2D_CATEGORIA_PREVISION.desc_categoria as Des_categoria, " +
                "   L2D_CATEGORIA_PREVISION.cod_liv_tipo_categoria as Cod_Liv_Categoria, " +
                "   L2D_CATEGORIA_PREVISION.desc_liv_tipo_categoria as Des_Liv_Categoria, " +
                "   t12.desc_indennita_importi_acquisiti, " +
                "   t12.desc_indennita_importi_persi, " +
                "   t12.desc_indennita_importi_delta " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_PART_TIME] T14 ON(t1.[SKY_PART_TIME] = T14.[sky_part_time]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t8 ON (t1.SKY_MESE_CONTABILE = t8.sky_tempo) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE_IMPORTI] t9 ON (t9.SKY_riga_te = t1.SKY_riga_te) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA]  L2D_CATEGORIA_PREVISION ON(t9.SKY_categoria_previsionale= L2D_CATEGORIA_PREVISION.sky_categoria) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_INDENNITA_PREVISIONALE] t12 ON (t9.sky_indennita_previsionale = t12.sky_indennita_previsionale) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] t13 ON (t13.sky_livello_categ=t1.sky_livello_categ) " +
                //" inner join xr_prv_dipendenti dip on(dip.matricola = t0.matricola_dp) "+
                //" inner join xr_prv_dipendenti_variazioni merito on(merito.id_dipendente = dip.id_dipendente and merito.id_prov = 2) "+
                //" inner join xr_prv_dipendenti_variazioni pass_ass on(pass_ass.id_dipendente = dip.id_dipendente and pass_ass.id_prov = 1) "+
                //" inner join xr_prv_dipendenti_variazioni pass_no_ass on(pass_no_ass.id_dipendente = dip.id_dipendente and pass_no_ass.id_prov = 4) "+
                " WHERE ( " +
                " 		t0.matricola_dp IN (" + matricola + ") " +
                " 		AND t8.num_anno = " + year + " " +
                " 		AND t8.cod_mese = " + month + " " +
                " 		AND (t1.num_fine_mese = 1) " +
                //" and ( "+
                //" (case when t9.diff_ral_merito <> merito.diff_ral then '1' else '0' end) = 1 "+ 
                //" or(case when t9.diff_ral_passaggio <> pass_no_ass.diff_ral then '1' else '0' end) = 1 " +
                //" or(case when t9.diff_ral_passaggio_assorbito <> pass_ass.diff_ral then '1' else '0' end) = 1 ) " +
                " 		) " +
                " GROUP BY t0.matricola_dp, " +
                "   t13.desc_livello , " +
                "   t14.[cod_part_time],  " +
                "   t14.[desc_part_time], " +
                "   t9.tot_retrib_annua , " +
                " 	t9.diff_ral_merito, " +
                " 	t9.diff_ral_passaggio, " +
                " 	t9.diff_ral_passaggio_assorbito, " +
                "   L2D_CATEGORIA_PREVISION.cod_categoria, " +
                "   L2D_CATEGORIA_PREVISION.desc_categoria, " +
                "   L2D_CATEGORIA_PREVISION.cod_liv_tipo_categoria, " +
                "   L2D_CATEGORIA_PREVISION.desc_liv_tipo_categoria, " +
                "   t12.desc_indennita_importi_acquisiti, " +
                "   t12.desc_indennita_importi_persi, " +
                "   t12.desc_indennita_importi_delta ";
            return query;
        }

        private static string GetQueryPartTime(string matricola, int year, int month)
        {
            string query =
                " SELECT t0.matricola_dp As Matricola, " +
                " 	t2.cod_part_time As CodPartTime, " +
                " 	cast(t1.perc_part_time as decimal(10,3)) As PercPartTime, " +
                " 	sum(t1.unita_anno) as UnitaAnno " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_PART_TIME] t2 ON (t1.SKY_PART_TIME = t2.sky_part_time) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t3 ON (t1.SKY_MESE_CONTABILE = t3.sky_tempo) " +
                " WHERE ( " +
                "       t0.matricola_dp = "+matricola+" " +
                " 		and t3.num_anno = "+year+ " " +
                " 		and t3.cod_mese = "+month+" " +
                " 		AND t2.cod_part_time NOT IN ('$') " +
                " 		AND ( " +
                " 			t1.flg_ultimo_record IN ( " +
                " 				'B', " +
                " 				'C', " +
                " 				'$' " +
                " 				) " +
                " 			) " +
                " 		) " +
                " GROUP BY t0.matricola_dp, " +
                " 	t2.cod_part_time, " +
                " 	t1.perc_part_time ";
            return query;
        }
        public static string GetQueryRAL(string matricola)
        {
            string query =
                " SELECT t0.[matricola_dp] as Matricola, " +
                " 	CAST(t7.num_anno as int) as Anno, " +
                " 	avg(t8.[tot_retrib_annua]) as Ral_media " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t7 ON (t1.[SKY_MESE_CONTABILE] = t7.[sky_tempo]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE_IMPORTI] t8 ON (t8.[SKY_riga_te] = t1.[SKY_riga_te]) " +
                " WHERE t0.matricola_dp = '" + matricola + "' " +
                //" and t8.tot_retrib_annua is not null " +
                " GROUP BY t0.[matricola_dp], " +
                " 	t7.num_anno " +
                " ORDER BY t7.num_anno ";
            return query;
        }

        public static string GetQueryRAL(string matricole, int anno)
        {
            string query =
                " SELECT t0.[matricola_dp] as Matricola, " +
                " 	CAST(t7.num_anno as int) as Anno, " +
                " 	avg(t8.[tot_retrib_annua]) as Ral_media " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t7 ON (t1.[SKY_MESE_CONTABILE] = t7.[sky_tempo]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE_IMPORTI] t8 ON (t8.[SKY_riga_te] = t1.[SKY_riga_te]) " +
                " WHERE t0.matricola_dp IN ("+matricole+") " +
                " and CAST(t7.num_anno as int)="+anno+" " +
                //" and t8.tot_retrib_annua is not null " +
                " GROUP BY t0.[matricola_dp], " +
                " 	t7.num_anno " +
                " ORDER BY t7.num_anno ";
            return query;
        }

        private static string GetQueryVariabili(string matricola)
        {
            string query =
                " SELECT t0.matricola_dp, " +
                " 	CAST(t2.num_anno as int) as Anno, " +
                "   t2.cod_mese as Mese, " +
                " 	t3.cod_aggregato_costi, " +
                " 	t3.desc_aggregato_costi, " +
                " 	t3.cod_voce_cedolino, " +
                " 	t3.desc_voce_cedolino, " +
                " 	sum(t1.quantita_ore) as Ore, " +
                " 	sum(t1.quantita_giorni) as Giorni, " +
                " 	sum(t1.quantita_numero_prestaz) as NumeroPrestazioni, " +
                " 	sum(t1.importo) as Importo " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_CEDO_CASSA] t1 ON (t1.sky_matricola = t0.sky_anagrafica_unica) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t1.sky_data_competenza = t2.sky_tempo) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_VOCE_CEDOLINO] t3 ON (t1.sky_voce_cedolino = t3.sky_voce_cedolino) " +
                " WHERE ( " +
                " 		t0.matricola_dp ='" + matricola + "' " +
                " 		AND t3.cod_aggregato_costi IN ('02', '03', '04') " +
                " 		) " +
                " GROUP BY t0.matricola_dp, " +
                " 	t2.num_anno, " +
                " 	t2.cod_mese, " +
                " 	t3.cod_aggregato_costi, " +
                " 	t3.desc_aggregato_costi, " +
                " 	t3.cod_voce_cedolino, " +
                " 	t3.desc_voce_cedolino " +
                " order by t2.num_anno, t3.cod_aggregato_costi, t3.cod_voce_cedolino ";
            return query;
        }
        private static string GetQueryAssenze(string matricola)
        {
            string query =
                " SELECT t0.matricola_dp as Matricola, " +
                //" 	CAST(t2.num_anno as int) as Anno, " +
                //" 	t2.cod_mese AS Mese, " +
                " 	0 as Anno, " +
                " 	0 AS Mese, " +
                " 	sum(t1.quantita_numero) AS Quantita, " +
                " 	CASE  " +
                " 		WHEN t3.cod_eccez_padre IS NULL " +
                " 			THEN 'Varie' " +
                " 		ELSE t3.cod_eccez_padre " +
                " 		END AS cod_eccez_padre, " +
                " 	CASE  " +
                " 		WHEN t3.desc_cod_eccez_padre IS NULL " +
                " 			THEN 'VARIE' " +
                " 		ELSE t3.desc_cod_eccez_padre " +
                " 		END AS desc_cod_eccez_padre, " +
                " 	t3.cod_eccezione, " +
                " 	t3.desc_eccezione, " +
                " 	CASE  " +
                " 		WHEN t3.unita_misura = 'G' " +
                " 			THEN 'GG' " +
                " 		WHEN t3.unita_misura = 'H' " +
                " 			THEN 'Ore' " +
                " 		WHEN t3.unita_misura = 'K' " +
                " 			THEN 'Km' " +
                " 		WHEN t3.unita_misura = 'N' " +
                " 			THEN 'Q.tà' " +
                " 		ELSE t3.unita_misura " +
                " 		END AS unita_misura " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_ECCEZIONI] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t2.sky_tempo = t1.SKY_DATA) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ECCEZIONE] t3 ON (t3.sky_eccezione = t1.SKY_eccezione) " +
                " WHERE ( " +
                " 		t0.matricola_dp IN ('" + matricola + "') " +
                "       AND ( " +
                "       t2.num_anno = year(dateadd(year, -1, getdate())) and t2.cod_mese >= month(dateadd(year, -1, getdate())) " +
                "       or t2.num_anno > year(dateadd(year, -1, getdate())) " +
                "       ) " +
                " 		AND CASE  " +
                " 			WHEN t3.desc_cod_eccez_padre IS NULL " +
                " 				THEN 'VARIE' " +
                " 			ELSE t3.desc_cod_eccez_padre " +
                " 			END IN ('3-MALATTIE / INFORTUNI(G)','4-ex legge 104 e coll.(G)','6-maternita e colleg. (G)','7-altri giustificativi(G)') " +
                " 		) " +
                " GROUP BY t0.matricola_dp, " +
                //" 	t2.num_anno, " +
                //" 	t2.cod_mese, " +
                " 	CASE  " +
                " 		WHEN t3.cod_eccez_padre IS NULL " +
                " 			THEN 'Varie' " +
                " 		ELSE t3.cod_eccez_padre " +
                " 		END, " +
                " 	CASE  " +
                " 		WHEN t3.desc_cod_eccez_padre IS NULL " +
                " 			THEN 'VARIE' " +
                " 		ELSE t3.desc_cod_eccez_padre " +
                " 		END, " +
                " 	t3.cod_eccezione, " +
                " 	t3.desc_eccezione, " +
                " 	CASE  " +
                " 		WHEN t3.unita_misura = 'G' " +
                " 			THEN 'GG' " +
                " 		WHEN t3.unita_misura = 'H' " +
                " 			THEN 'Ore' " +
                " 		WHEN t3.unita_misura = 'K' " +
                " 			THEN 'Km' " +
                " 		WHEN t3.unita_misura = 'N' " +
                " 			THEN 'Q.tà' " +
                " 		ELSE t3.unita_misura " +
                " 		END ";
                //" ORDER BY t2.num_anno ";
            return query;
        }
        private static string GetQueryReperibilita(string matricola)
        {
            string query =
                    "  SELECT [matricola] as Matricola,  " +
                    "  	[data_cont],   " +
                    " 	CAST(('20'+LEFT(data_cont,2)) as int) as Anno, " +
                    " 	CAST(RIGHT(data_cont,2) as int) as Mese, " +
                    "  	SUM([quantita_ore]) as Ore,  " +
                    "  	SUM([quantita_giorni]) as Giorni,  " +
                    "  	SUM([quantita_numero_prestaz]) as NumeroPrestazioni,  " +
                    "  	SUM([importo]) as Importo " +
                    "  FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_CEDO_DET]  " +
                    "  WHERE (  " +
                    "  		CODICE LIKE '26D%'  " +
                    "  		OR CODICE LIKE '280%'  " +
                    "  		OR CODICE LIKE '262%'  " +
                    "  		)  " +
                    " 	AND [data_cont] >= '1801'  " +
                    "  	AND [matricola] = '" + matricola + "'  " +
                    " group by matricola, data_cont ";
            return query;
        }
        private static string GetQueryOrganicoPerRicerca(string matricola, string cognome, string nome, string servizio)
        {
            string query = 
                " SELECT t0.[matricola_dp] as Matricola, " +
                " 	t0.[nominativo] as Nominativo, " +
                " 	t5.[cod_serv_cont] as CodServizio, " +
                " 	t5.[desc_serv_cont] as DesServizio, " +
                " 	t6.[cod_sezione] as CodSezione, " +
                " 	t6.[des_sezione] as DesSezione " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SERVIZIO_CONTABILE] t5 ON (t1.[SKY_servizio_contabile] = t5.[sky_serv_cont]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SEZIONE] t6 ON (t1.[SKY_sezione] = t6.[sky_sezione]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t7 ON (t1.[SKY_MESE_CONTABILE] = t7.[sky_tempo]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] t10 ON (t10.[sky_livello_categ] = t1.[sky_livello_categ]) " +
                " WHERE ( " +
                " 		des_societa = 'RAI' " +
                " 		AND t7.[num_anno] = "+DateTime.Today.Year.ToString()+" " +
                " 		AND t7.cod_mese = "+ DateTime.Today.AddMonths(-1).Month.ToString()+" " +
                " 		AND ( " +
                " 			t10.categoria NOT LIKE 'A7%' " +
                " 			AND t10.categoria NOT LIKE 'A%' " +
                " 			AND t10.categoria <> 'Q10' " +
                " 			AND t10.categoria <> 'Q12' " +
                " 			AND t10.categoria NOT LIKE 'M7%' " +
                " 			) " +
                " 		AND ( " +
                " 			t10.tipo_minimo <> '5' " +
                " 			AND t10.tipo_minimo <> '7' " +
                " 			) " +
                (!String.IsNullOrWhiteSpace(matricola)?" and t0.matricola_dp='"+matricola+"' ":"") +
                (!String.IsNullOrWhiteSpace(servizio) ? " and t5.cod_serv_cont='" + servizio + "' " : "") +
                (!String.IsNullOrWhiteSpace(cognome) || !String.IsNullOrWhiteSpace(nome)?
                    " and t0.nominativo like '" +(!String.IsNullOrWhiteSpace(cognome)?cognome+"%":"") + (!String.IsNullOrWhiteSpace(nome) ? "%"+ nome + "%":"") + "' "
                    :""
                ) +
                " 		) " +
                " GROUP BY t0.[matricola_dp], " +
                " 	t0.[nominativo], " +
                " 	t5.[cod_serv_cont], " +
                " 	t5.[desc_serv_cont], " +
                " 	t6.[cod_sezione], " +
                " 	t6.[des_sezione] " +
                " ORDER BY t0.matricola_dp ";
            return query;
        }
        private static string GetQueryApprendisti()
        {
            string query =
                " SELECT t0.[matricola_dp] " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SERVIZIO_CONTABILE] t5 ON (t1.[SKY_servizio_contabile] = t5.[sky_serv_cont]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_SEZIONE] t6 ON (t1.[SKY_sezione] = t6.[sky_sezione]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t7 ON (t1.[SKY_MESE_CONTABILE] = t7.[sky_tempo]) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] t10 ON (t10.[sky_livello_categ] = t1.[sky_livello_categ]) " +
                " WHERE ( " +
                " 		des_societa = 'RAI' " +
                " 		AND t7.[num_anno] = " + DateTime.Today.Year.ToString() + " " +
                " 		AND t7.cod_mese = " + DateTime.Today.AddMonths(-1).Month.ToString() + " " +
                " 		AND ( " +
                " 			t10.tipo_minimo = '5' " +
                " 			OR t10.tipo_minimo = '7' " +
                " 			) " +
                " 		) " +
                " GROUP BY t0.[matricola_dp] ";
            return query;
        }

        #region QueryGratifiche
        private static string GetQueryGratifiche(string matr = "")
        {
            string query =
                " SELECT t0.matricola_dp as Matricola, " +
                " 	t1.importo as Importo, " +
                "   CAST(t2.num_anno as int) as Anno, " +
                "   t2.cod_mese as Mese, " +
                "   t2.num_giorno as Giorno " +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_CEDO_CASSA] t1 ON (t1.sky_matricola = t0.sky_anagrafica_unica) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t1.sky_data_competenza = t2.sky_tempo) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_VOCE_CEDOLINO] t3 ON (t1.sky_voce_cedolino = t3.sky_voce_cedolino) " +
                " WHERE ( " +
                (!String.IsNullOrWhiteSpace(matr) ? " t0.matricola_dp='" + matr + "'" : " t0.matricola_dp IN (" + GetQueryMatricoleInServizio() + ")") + " " +
                " 		AND t3.cod_voce_cedolino = '274' " +
                " 		) ";
            return query;
        }
        private static string GetQueryGratificheServ(string anno)
        {
            string query =
                " SELECT t0.cod_serv_cont as Codice, " +
                " 		count(*) as Numero" +
                " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_CEDO_CASSA] t1 ON (t1.sky_matricola = t0.sky_anagrafica_unica) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t1.sky_data_competenza = t2.sky_tempo) " +
                " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_VOCE_CEDOLINO] t3 ON (t1.sky_voce_cedolino = t3.sky_voce_cedolino) " +
                " WHERE ( " +
                " 		t0.matricola_dp IN  " +
                " 		( " +
                GetQueryMatricoleInServizio() +
                " 		) " +
                " 		AND t2.num_anno = " + anno + " " +
                " 		AND t3.cod_voce_cedolino = '274' " +
                " 		) " +
                " GROUP BY t0.cod_serv_cont ";
            return query;
        }
        #endregion

        #region QueryAumenti
        private static string GetQueryAumenti(string matr = "")
        {
            string query =
                    " SELECT t0.matricola_dp, " +
                    " 	t2.desc_aggregato_tipo_var_combinazione, " +
                    " 	t2.cod_aggregato_tipo_var_combinazione, " +
                    " 	CASE  " +
                    " 		WHEN t1.motivo_variazione = '5' " +
                    " 			THEN '5 - da causa ' " +
                    " 		WHEN t1.motivo_variazione = '7' " +
                    " 			THEN '7 - da accordi sindacali' " +
                    " 		WHEN t1.motivo_variazione = '8' " +
                    " 			THEN '8 - Cessaz. consensuale/scambio' " +
                    " 		ELSE t1.motivo_variazione " +
                    " 		END as Motivo_variazione, " +
                    " 	t2.codice_tipo_var_contabili_1, " +
                    " 	t2.desc_tipo_variazione_te_1, " +
                    " 	t2.codice_tipo_var_contabili_2, " +
                    " 	t2.desc_tipo_variazione_te_2, " +
                    " 	t2.codice_tipo_var_contabili_3, " +
                    " 	t2.desc_tipo_variazione_te_3, " +
                    " 	t2.codice_tipo_var_contabili_4, " +
                    " 	t2.desc_tipo_variazione_te_4, " +
                    " 	t2.codice_tipo_var_contabili_5, " +
                    " 	t2.desc_tipo_variazione_te_5, " +
                    " 	SUM(t4.diff_RAL_merito_ad_pers) as Importo, " +
                    " 	CASE  " +
                    " 		WHEN t1.FLAG_MOTIVO_VAR = '5' " +
                    " 			THEN '5 - da causa' " +
                    " 		WHEN t1.FLAG_MOTIVO_VAR = '7' " +
                    " 			THEN '7 - da accordi sindacali' " +
                    " 		ELSE t1.FLAG_MOTIVO_VAR " +
                    " 		END as Flag_motivo_var, " +
                    "   CAST(t3.num_anno as int) as Anno, " +
                    "   t3.cod_mese as Mese, " +
                    "   t3.num_giorno as Giorno " +
                    " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_VARIAZIONE_COMBINAZIONE] t2 ON (t1.sky_tipo_variazione_combinazione = t2.sky_tipo_variazione_combinazione) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t3 ON (t1.SKY_MESE_CONTABILE = t3.sky_tempo) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE_IMPORTI] t4 ON (t4.SKY_riga_te = t1.SKY_riga_te) " +
                    " WHERE ( " +
                    (!String.IsNullOrWhiteSpace(matr) ? " t0.matricola_dp='" + matr + "'" : " t0.matricola_dp IN (" + GetQueryMatricoleInServizio() + ")") + " " +
                    " 		AND ( " +
                    " 			( " +

                    " 				t2.codice_tipo_var_contabili_1 IN ( " +
                    " 					'03', " +
                    " 					'84' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_2 IN ( " +
                    " 					'03', " +
                    " 					'84' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_3 IN ( " +
                    " 					'03', " +
                    " 					'84' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_4 IN ( " +
                    " 					'03', " +
                    " 					'84' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_5 IN ( " +
                    " 					'03', " +
                    " 					'84' " +
                    " 					) " +
                    " 				) " +
                    " 			) " +
                    " 		) " +
                    " GROUP BY t0.matricola_dp, " +
                    " 	t2.desc_aggregato_tipo_var_combinazione, " +
                    " 	t2.cod_aggregato_tipo_var_combinazione, " +
                    " 	CASE  " +
                    " 		WHEN t1.motivo_variazione = '5' " +
                    " 			THEN '5 - da causa ' " +
                    " 		WHEN t1.motivo_variazione = '7' " +
                    " 			THEN '7 - da accordi sindacali' " +
                    " 		WHEN t1.motivo_variazione = '8' " +
                    " 			THEN '8 - Cessaz. consensuale/scambio' " +
                    " 		ELSE t1.motivo_variazione " +
                    " 		END, " +
                    " 	t2.codice_tipo_var_contabili_1, " +
                    " 	t2.desc_tipo_variazione_te_1, " +
                    " 	t2.codice_tipo_var_contabili_2, " +
                    " 	t2.desc_tipo_variazione_te_2, " +
                    " 	t2.codice_tipo_var_contabili_3, " +
                    " 	t2.desc_tipo_variazione_te_3, " +
                    " 	t2.codice_tipo_var_contabili_4, " +
                    " 	t2.desc_tipo_variazione_te_4, " +
                    " 	t2.codice_tipo_var_contabili_5, " +
                    " 	t2.desc_tipo_variazione_te_5, " +
                    " 	CASE  " +
                    " 		WHEN t1.FLAG_MOTIVO_VAR = '5' " +
                    " 			THEN '5 - da causa' " +
                    " 		WHEN t1.FLAG_MOTIVO_VAR = '7' " +
                    " 			THEN '7 - da accordi sindacali' " +
                    " 		ELSE t1.FLAG_MOTIVO_VAR " +
                    " 		END, " +
                    " 	CAST(t3.num_anno AS INT), " +
                    " 	t3.cod_mese, " +
                    " 	t3.num_giorno ";
            return query;
        }
        private static string GetQueryAumentiServ(string anno)
        {
            string query =
                    " SELECT t0.cod_serv_cont as Codice, " +
                    " 		count(*) as Numero" +
                    " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_VARIAZIONE_COMBINAZIONE] t2 ON (t1.sky_tipo_variazione_combinazione = t2.sky_tipo_variazione_combinazione) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t3 ON (t1.SKY_MESE_CONTABILE = t3.sky_tempo) " +
                    " WHERE ( " +
                    " 		t0.matricola_dp IN  " +
                    " 		( " +
                    GetQueryMatricoleInServizio() +
                    " 		) " +
                    " 		AND t3.num_anno = " + anno + " " +
                    " 		AND ( " +
                    " 			( " +
                    " 				t2.codice_tipo_var_contabili_1 IN ( " +
                    " 					'03', " +
                    " 					'84' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_2 IN ( " +
                    " 					'03', " +
                    " 					'84' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_3 IN ( " +
                    " 					'03', " +
                    " 					'84' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_4 IN ( " +
                    " 					'03', " +
                    " 					'84' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_5 IN ( " +
                    " 					'03', " +
                    " 					'84' " +
                    " 					) " +
                    " 				) " +
                    " 			) " +
                    " 		) " +
                    " GROUP BY t0.cod_serv_cont ";
            return query;
        }
        #endregion

        #region QueryPassaggi
        private static string GetQueryPassaggi(string matr = "")
        {
            string query =
                    " SELECT t0.matricola_dp as Matricola, " +
                    " 	t2.desc_aggregato_tipo_var_combinazione, " +
                    " 	t2.cod_aggregato_tipo_var_combinazione, " +
                    " 	CASE  " +
                    " 		WHEN t1.motivo_variazione = '5' " +
                    " 			THEN '5 - da causa ' " +
                    " 		WHEN t1.motivo_variazione = '7' " +
                    " 			THEN '7 - da accordi sindacali' " +
                    " 		WHEN t1.motivo_variazione = '8' " +
                    " 			THEN '8 - Cessaz. consensuale/scambio' " +
                    " 		ELSE t1.motivo_variazione " +
                    " 		END as Motivo_variazione, " +
                    " 	t2.codice_tipo_var_contabili_1, " +
                    " 	t2.desc_tipo_variazione_te_1, " +
                    " 	t2.codice_tipo_var_contabili_2, " +
                    " 	t2.desc_tipo_variazione_te_2, " +
                    " 	t2.codice_tipo_var_contabili_3, " +
                    " 	t2.desc_tipo_variazione_te_3, " +
                    " 	t2.codice_tipo_var_contabili_4, " +
                    " 	t2.desc_tipo_variazione_te_4, " +
                    " 	t2.codice_tipo_var_contabili_5, " +
                    " 	t2.desc_tipo_variazione_te_5, " +
                    " 	SUM(t4.diff_RAL_passaggio_categ) as Importo, " +
                    " 	CASE  " +
                    " 		WHEN t1.FLAG_MOTIVO_VAR = '5' " +
                    " 			THEN '5 - da causa' " +
                    " 		WHEN t1.FLAG_MOTIVO_VAR = '7' " +
                    " 			THEN '7 - da accordi sindacali' " +
                    " 		ELSE t1.FLAG_MOTIVO_VAR " +
                    " 		END as Flag_motivo_var, " +
                    "   CAST(t3.num_anno as int) as Anno, " +
                    "   t3.cod_mese as Mese, " +
                    "   t3.num_giorno as Giorno, " +
                    "   t5.cod_categoria, " +
                    "   t5.desc_categoria, " +
                    "   t6.desc_livello " +
                    " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_VARIAZIONE_COMBINAZIONE] t2 ON (t1.sky_tipo_variazione_combinazione = t2.sky_tipo_variazione_combinazione) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t3 ON (t1.SKY_MESE_CONTABILE = t3.sky_tempo) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE_IMPORTI] t4 ON (t4.SKY_riga_te = t1.SKY_riga_te) " +
                    " inner join [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA] t5 on (t5.SKY_categoria=t1.sky_categoria) " +
                    " inner join [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] t6 on (t6.[sky_livello_categ] = t1.[sky_livello_categ]) " +
                    " WHERE ( " +
                    (!String.IsNullOrWhiteSpace(matr) ? " t0.matricola_dp='" + matr + "'" : " t0.matricola_dp IN (" + GetQueryMatricoleInServizio() + ")") + " " +
                    " 		AND ( " +
                    " 			( " +
                    " 				t2.codice_tipo_var_contabili_1 IN ( " +
                    " 					'04', " +
                    " 					'44', " +
                    " 					'80', " +
                    " 					'83', " +
                    " 					'85' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_2 IN ( " +
                    " 					'04', " +
                    " 					'44', " +
                    " 					'80', " +
                    " 					'83', " +
                    " 					'85' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_3 IN ( " +
                    " 					'04', " +
                    " 					'44', " +
                    " 					'80', " +
                    " 					'83', " +
                    " 					'85' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_4 IN ( " +
                    " 					'04', " +
                    " 					'44', " +
                    " 					'80', " +
                    " 					'83', " +
                    " 					'85' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_5 IN ( " +
                    " 					'04', " +
                    " 					'44', " +
                    " 					'80', " +
                    " 					'83', " +
                    " 					'85' " +
                    " 					) " +
                    " 				) " +
                    " 			) " +
                    " 		) " +
                    " GROUP BY t0.matricola_dp, " +
                    " 	t2.desc_aggregato_tipo_var_combinazione, " +
                    " 	t2.cod_aggregato_tipo_var_combinazione, " +
                    " 	CASE  " +
                    " 		WHEN t1.motivo_variazione = '5' " +
                    " 			THEN '5 - da causa ' " +
                    " 		WHEN t1.motivo_variazione = '7' " +
                    " 			THEN '7 - da accordi sindacali' " +
                    " 		WHEN t1.motivo_variazione = '8' " +
                    " 			THEN '8 - Cessaz. consensuale/scambio' " +
                    " 		ELSE t1.motivo_variazione " +
                    " 		END, " +
                    " 	t2.codice_tipo_var_contabili_1, " +
                    " 	t2.desc_tipo_variazione_te_1, " +
                    " 	t2.codice_tipo_var_contabili_2, " +
                    " 	t2.desc_tipo_variazione_te_2, " +
                    " 	t2.codice_tipo_var_contabili_3, " +
                    " 	t2.desc_tipo_variazione_te_3, " +
                    " 	t2.codice_tipo_var_contabili_4, " +
                    " 	t2.desc_tipo_variazione_te_4, " +
                    " 	t2.codice_tipo_var_contabili_5, " +
                    " 	t2.desc_tipo_variazione_te_5, " +
                    " 	CASE  " +
                    " 		WHEN t1.FLAG_MOTIVO_VAR = '5' " +
                    " 			THEN '5 - da causa' " +
                    " 		WHEN t1.FLAG_MOTIVO_VAR = '7' " +
                    " 			THEN '7 - da accordi sindacali' " +
                    " 		ELSE t1.FLAG_MOTIVO_VAR " +
                    " 		END, " +
                    " 	CAST(t3.num_anno AS INT), " +
                    " 	t3.cod_mese, " +
                    " 	t3.num_giorno, " +
                    "   t5.cod_categoria, " +
                    "   t5.desc_categoria, " +
                    "   t6.desc_livello ";
            return query;
        }
        private static string GetQueryPassaggiServ(string anno)
        {
            string query =
                    " SELECT t0.cod_serv_cont as Codice, " +
                    " 		count(*) as Numero " +
                    " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_VARIAZIONE_COMBINAZIONE] t2 ON (t1.sky_tipo_variazione_combinazione = t2.sky_tipo_variazione_combinazione) " +
                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t3 ON (t1.SKY_MESE_CONTABILE = t3.sky_tempo) " +
                    " WHERE ( " +
                    " 		t0.matricola_dp IN  " +
                    " 		( " +
                    GetQueryMatricoleInServizio() +
                    " 		) " +
                    " 		and t3.num_anno = " + anno + " " +
                    " 		AND ( " +
                    " 			( " +
                    " 				t2.codice_tipo_var_contabili_1 IN ( " +
                    " 					'04', " +
                    " 					'44', " +
                    " 					'80', " +
                    " 					'83', " +
                    " 					'85' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_2 IN ( " +
                    " 					'04', " +
                    " 					'44', " +
                    " 					'80', " +
                    " 					'83', " +
                    " 					'85' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_3 IN ( " +
                    " 					'04', " +
                    " 					'44', " +
                    " 					'80', " +
                    " 					'83', " +
                    " 					'85' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_4 IN ( " +
                    " 					'04', " +
                    " 					'44', " +
                    " 					'80', " +
                    " 					'83', " +
                    " 					'85' " +
                    " 					) " +
                    " 				OR t2.codice_tipo_var_contabili_5 IN ( " +
                    " 					'04', " +
                    " 					'44', " +
                    " 					'80', " +
                    " 					'83', " +
                    " 					'85' " +
                    " 					) " +
                    " 				) " +
                    " 			) " +
                    " 		) " +
                    " GROUP BY t0.cod_serv_cont ";
            return query;
        }
        #endregion
        #endregion
    }
}