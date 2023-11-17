using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiData;
using myRaiHelper;
using myRaiServiceHub;

namespace myRaiCommonModel
{
    public class SpeseProduzioneViewModel
    {
        string matricola = "0201590" /*CommonManager.GetCurrentUserMatricola()*/;
        //Test su questa matricola con +100 record 
        /*"0575535";*/

        /*Metodo che chiama un Servizio per recuperera la descrizione delle spese di produzione
         WBS
        */
        private string GetProduzioneByMatricola ( string term , string desc)
        {
            return "123";
            try
            {
                myRaiServiceHub.it.rai.servizi.anagraficaws1.APWS sr = new myRaiServiceHub.it.rai.servizi.anagraficaws1.APWS();
                sr.Credentials = new System.Net.NetworkCredential(@"srvanapro", "bc14a3", "RAI");
                myRaiServiceHub.it.rai.servizi.anagraficaws1.ObjTVRicercaAnagrafieResult res = new myRaiServiceHub.it.rai.servizi.anagraficaws1.ObjTVRicercaAnagrafieResult();

                myRaiServiceHub.it.rai.servizi.anagraficaws1.ObjInputRicercaMatricola ricercaMatricola = new myRaiServiceHub.it.rai.servizi.anagraficaws1.ObjInputRicercaMatricola();

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
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
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
        public List<SpeseProduzioneViewModel> ListaDescrizioniAndImportiRendicontiDipendente{ get; set; }
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
        public decimal  MA_Importo_In_Euro { get; set; }
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
       
        public void Init()
        {
            ListaDescrizioniAndImportiAnticipi = new List<SpeseProduzioneViewModel>();
            ListaDescrizioniAndImportiRendicontiInSegreteria = new List<SpeseProduzioneViewModel>();
            ListaDescrizioniAndImportiRendicontiContabilita = new List<SpeseProduzioneViewModel>();
            ListaDescrizioniAndImportiRendicontiDipendente = new List<SpeseProduzioneViewModel>();
            ListaDescrizioniAndImportiRendicontiPersonale = new List<SpeseProduzioneViewModel>();
          
        }
      

    }
}
