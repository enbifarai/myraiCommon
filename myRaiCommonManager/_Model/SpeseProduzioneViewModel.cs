using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRai.Business;
using myRaiData;
using myRaiHelper;
using myRaiServiceHub;
using myRaiServiceHub.it.rai.servizi.anagraficaws1;

namespace myRaiCommonModel
{
    public class SpeseProduzioneViewModel
    {
        string matricola = CommonManager.GetCurrentUserMatricola();

        //Test su questa matricola con +100 record 
        /*"0575535";*/

        /*Metodo che chiama un Servizio per recuperera la descrizione delle spese di produzione
         WBS
        */
        private string GetProduzioneByMatricola(string term, string desc)
        {
            return "123";
            try
            {
                APWS sr = new APWS();
                sr.Credentials = new System.Net.NetworkCredential(@"srvanapro", "bc14a3", "RAI");
                ObjTVRicercaAnagrafieResult res = new ObjTVRicercaAnagrafieResult();

                ObjInputRicercaMatricola ricercaMatricola = new ObjInputRicercaMatricola();

                ricercaMatricola.Matricola = term;
                ricercaMatricola.Uorg = desc;
                ricercaMatricola.StatiInVita = true;

                res = sr.TvRicercaAnagrafiaMatricola(ricercaMatricola);

                var results = res.RisultatoTVRicercaAnagrafie.FirstOrDefault();
                if (results != null)
                {
                    return results.TITOLO;
                }
                else
                {
                    return String.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public SpeseProduzioneViewModel()
            : base()
        {
            Init();
        }
        public List<SpeseProduzioneViewModel> ListaDescrizioniAndImportiAnticipi { get; set; }
        public List<SpeseProduzioneViewModel> ListaDescrizioniAndImportiRendicontiInSegreteria { get; set; }
        public List<SpeseProduzioneViewModel> ListaDescrizioniAndImportiRendicontiPersonale { get; set; }
        public List<SpeseProduzioneViewModel> ListaDescrizioniAndImportiRendicontiContabilita { get; set; }
        public List<SpeseProduzioneViewModel> ListaDescrizioniAndImportiRendicontiDipendente { get; set; }
        public double sommaImportiPerMeseCorrente { get; set; }
        public SpeseProduzioneViewModel Data { get; set; }
        public bool isAperta { get; set; }
        public string DescrizioneTarghetta { get; set; }
        public string DescrizioneTipo { get; set; }
        public string Descrizione { get; set; }
        public bool IsSearch { get; set; }
        public decimal Totale { get; set; }
        public decimal Differenza { get; set; }
        public decimal Saldo { get; set; }
        public decimal ImportoTotaleAnticipo { get; set; }
        public bool SenzaAnticipo { get; set; }
        public bool DaRendicontare { get; set; }

        public string Località { get; set; }
        //Entity = TImporti_SAP
        public decimal Id_TImportiSap { get; set; }
        public string Tipo { get; set; }
        public string MA_Data { get; set; }
        public string MP_Data { get; set; }
        public decimal MP_Importo { get; set; }
        public string MA_Stato { get; set; }
        public decimal MA_Importo_In_Euro { get; set; }
        public string Titolo { get; set; }
        public string WBS { get; set; }
        public decimal importoRA { get; set; }
        public decimal importoRAB { get; set; }
        public decimal importoSDA { get; set; }

        //Entity = TFoglioSpese

        public decimal Id_FoglioSpese { get; set; }
        public string Matricola { get; set; }
        public string Sezione { get; set; }
        public string Stato { get; set; }
        public string Foglio_Viaggio { get; set; }
        public string Nominativo { get; set; }


        //Entity = Anticipo

        public decimal Id_Anticipo { get; set; }
        public string ProcuratoreAnticipo { get; set; }
        public System.DateTime Periodo_Dal { get; set; }
        public string StatoAnticipo { get; set; }
        public System.DateTime Periodo_Al { get; set; }
        //Entity = Rendiconto
        public decimal Id_Rendiconto { get; set; }
        public string StatoRendiconto { get; set; }
        public string TipoRendiconto { get; set; }
        public string ProcuratoreRendiconto { get; set; }
        public string DataConsegna { get; set; }
        public string TipoTarghetta { get; set; }
        public short progressivoVoce { get; set; }
        public int idFile { get; set; }
        public string valutaVoce { get; set; }
        public void Init()
        {
            ListaDescrizioniAndImportiAnticipi = new List<SpeseProduzioneViewModel>();
            ListaDescrizioniAndImportiRendicontiInSegreteria = new List<SpeseProduzioneViewModel>();
            ListaDescrizioniAndImportiRendicontiContabilita = new List<SpeseProduzioneViewModel>();
            ListaDescrizioniAndImportiRendicontiDipendente = new List<SpeseProduzioneViewModel>();
            ListaDescrizioniAndImportiRendicontiPersonale = new List<SpeseProduzioneViewModel>();

        }


    }

    public class SpeseProduzioneVoce
    {
        public decimal Id { get; set; }
        public short ProgressivoVoce { get; set; }
        public DateTime DataSpesa { get; set; }
        public List<SelectListItem> Tipologie { get; set; }
        [Required]
        public string SelectedTipologia { get; set; }
        //    public List<SelectListItem> Voci { get; set; }
        [Required]
        public string SelectedVoce { get; set; }
        public string DescVoce { get; set; }
        public List<SelectListItem> CentriDiCosto { get; set; }
        public string SelectedCentro { get; set; }
        public List<ValutaSpese> Valute { get; set; }
        [Required]
        public string SelectedValuta { get; set; }
        [Required]
        public decimal Importo { get; set; }
        public decimal CambioVoce { get; set; }
        public decimal ValoreEuro { get; set; }
        public bool ConCarta { get; set; }
        public string TipoTarghetta { get; set; }
        public int IdFile { get; set; }
        public string NomeFile { get; set; }
        public int SizeFile { get; set; }
        //      public MyRai_Files file { get; set; }
    }

    public class ValutaSpese
    {
        public string Valuta { get; set; }
        public string descrizione { get; set; }
        public decimal cambioMedio { get; set; }
    }


    public class SpeseProduzioneCambi
    {
        public List<SpeseProduzioneCambio> elencoCambi { get; set; }
        // public List<SelectListItem> valute { get; set; }
        public decimal idFoglioSpese { get; set; }
    }
    public class SpeseProduzioneCambio
    {
        public decimal id { get; set; }
        public short progressivoCambio { get; set; }
        // [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}",
        //        ApplyFormatInEditMode = true)]
        [Required]
        public DateTime dataOperazione { get; set; }
        [Required]
        public string selectedValutaCeduta { get; set; }
        [Required]
        public decimal importoCeduto { get; set; }
        public decimal cambio { get; set; }
        [Required]
        public string selectedValutaComprata { get; set; }
        [Required]
        public decimal importoComprato { get; set; }
        [Required]
        public string selectedValutaSpese { get; set; }
        [Required]
        public decimal importoSpese { get; set; }
        public decimal cambioEuro { get; set; }
        public decimal speseEuro { get; set; }

    }

    public class jsonFile
    {
        public string idFoglioSpese { get; set; }
        public string progressivoVoce { get; set; }
    }

}
