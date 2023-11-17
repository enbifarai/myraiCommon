using System;
using System.Collections.Generic;
using myRaiData;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System.Web.Mvc;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using myRaiHelper;
using myRaiServiceHub.Autorizzazioni;
using myRaiCommonModel.DashboardResponsabile;

namespace myRaiCommonModel
{
    public class ModelDash_Utilities
    {
        public int TotRich { get; set; }
        public int TotUrg { get; set; }
        public int TotS { get; set; }
        public int TotOrd { get; set; }

        private Dictionary<int, MyRai_AttivitaCeiton> _attivitaCeiton; 
        public MyRai_AttivitaCeiton GetAttivitaCeiton(int idRich)
        {
            MyRai_AttivitaCeiton attivitaCeiton = null;
            if (!_attivitaCeiton.TryGetValue(idRich, out attivitaCeiton))
            {
                attivitaCeiton = CeitonHelper.GetAttivitaCeiton(idRich);
                _attivitaCeiton.Add(idRich, attivitaCeiton);
            }
            return attivitaCeiton;
        }

        public ModelDash_Utilities()
        {
            _attivitaCeiton = new Dictionary<int, MyRai_AttivitaCeiton>();
        }
    }

    public class ModelDash
    {
        public string MessaggioHome { get; set; }
        public string nome { get; set; }
        public bool digiGAPP { get; set; }
		public string MatricolaVisualizzata { get; set; }
		public DateTime? DataVisualizzata { get; set; }

        public dayResponse dettaglioGiornata { get; set; }
        public dayResponse dettaglioGiornataRichiesta { get; set; }
        public MyRaiServiceInterface.it.rai.servizi.digigappws.Dipendente dettaglioDipendente { get; set; }
        public monthResponseEccezione listaEvidenze { get; set; }
        public presenzeResponse listaPresenzeSettimanali { get; set; }
        public ProfiliAssociati listaProfili { get; set; }
        public MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione[] eccezionidaValidare { get; set; }
        //int asdfsa = int.Parse("erfg");
        public daApprovareModel elencoProfilieSedi { get; set; }
        public DettaglioSettimanaleModel dettaglioSettimanaleModel {get;set;}

        public bool isBoss { get; set; }

        public List<MiaRichiesta> MieRichieste { get; set; }
        public List<B2RaiPlace_Eventi_Programma> ListaProgrammi { get; set; }
        public List<B2RaiPlace_Eventi_Evento> ListaEventi { get; set; }

        public ProgramsModel Programs { get; set; }
        public EventsModel Events { get; set; }

        public string NomeRaggruppamento1 { get; set; }
        public string NomeRaggruppamento2 { get; set; }
        public PopupDettaglioGiornata PopupDettaglioGiornataModel { get; set; }
        public Dictionary<String, String[]> SediGappAbilitateConEccezioni { get; set; }

        public List<MyRai_Raggruppamenti> Raggruppamenti { get; set; }
        public string ValidazioneGenericaEccezioni { get; set; }

        public List<MyRai_SceltaPercorso> SceltePercorso { get; set; }
        public string JsInitialFunction { get; set; }
        public sidebarModel menuSidebar { get; set; }

        public int TotaleEccezioniDaApprovare { get; set; }
        public int TotaleEvidenzeDaGiustificare { get; set; }
        public int TotaleEvidenzeDaGiustificareSoloAssIng { get; set; }
        public int TotaleEvidenzeDaGiustificareSWTIM { get; set; }
        public int TotaleEvidenzeDaGiustificareCarenze { get; set; }
     
        public Ricerca RicercaGen { get; set; }

        public PresenzaDipendenti presenzaDipendenti { get; set; }
        public List<MyRai_Sezioni_Visibili> SezioniVisibili{ get;set; }

        public RicercaDaApprovare RicercaModel { get; set; }

        public GetDipendentiResponse DipendentiAssenti { get; set; }

        public StatiModel statiModel { get; set; }
        public EccezioniModel eccezioniModel { get; set; }

        public string QuadraturaUtenteTerzo { get; set; }

        public string BilancioPoh { get; set; }
        public string BilancioDettSettimanale { get; set; }
		public bool AbilitaApprovazione { get; set; }
		public string Cod_Eccezione { get; set; }

        public MieRichiesteVM MieRichiesteVM { get; set; }
        public string openday { get;  set; }


        public RicercaDaApprovareAttivita RicercaModelAttivita { get; set; }

        public ModelDash_Utilities ViewUtility { get; set; }
        public bool NascondiCestino { get;  set; }

        public bool RichiedeVisti { get; set; }
        public String RicercaVisti { get; set; }

        public WidgetVM WidgetStatoFerie { get; set; }
   
        public ModelDash()
        {
            this.RicercaModelAttivita = new RicercaDaApprovareAttivita ( );

            ViewUtility = new ModelDash_Utilities();
        }

    }
    public class StatiModel
    {
        public SelectList ListaStati { get; set; }
        public int StatoSelezionato { get; set; }
    }
    public class EccezioniModel
    {
        public SelectList ListaEccezioni { get; set; }
        public int EccezioneSelezionata { get; set; }
    }

    /// <summary>
    /// Modello utilizzato dalla view le mie richieste in tabelle subpartial
    /// tale modello viene utilizzato per ottenere info utili come la data di chiusura2 e la
    /// data di convalida dell'ultimo pdf per la sede dell'utente corrente
    /// Queste info sono utili per far si che il cestino accanto ad una richiesta sia visibile
    /// o meno, poichè al momento il cestino per stornare una richiesta è visibile anche per
    /// richieste di mesi chiusi.
    /// </summary>
    public class MieRichiesteVM
    {
        public DateTime DataUltimaConvalida { get; set; }
    }
    public class EvidenzaDataTipo
    {
        public DateTime Data { get; set; }
        public string Tipo { get; set; } //TAMCI
    }
}
