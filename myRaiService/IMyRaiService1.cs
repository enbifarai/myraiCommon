using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using myRaiData;

namespace myRaiService
{
    [ServiceContract]
    public interface IMyRaiService1
    {
        [OperationContract]
        CodiceFiscaleReponse GetCodiceFiscaleInfo(string cf, string matricolaOperatore);

        [OperationContract]
        SetPagamentoEccezioneResponse SetPagamentoEccezione(string matricolaOperatore, DateTime dataEccezione, string matricola, DateTime dataPagamento, string eccezione, string numdoc);

        [OperationContract]
        SetStatoEccezioneResponse SetStatoEccezione(string matricolaOperatore, DateTime D, string matricola, string numdoc, string eccezione, string stato);

        [OperationContract]
        GetRuoliResponse GetRuoli(string matricola, DateTime DataStart, string tipologia);

        [OperationContract]
        ResocontiResponse ClearCacheResoconti(List<SedeData> SediData);

        [OperationContract]
        GetRipianificazioniMatricolaResponse GetRipianificazioniMatricola(string matricola, DateTime DataInizio, DateTime DataFine);

        [OperationContract]
        List<PeriodoSW> GetPeriodiSW(string matricola);

        [OperationContract]
        InviaPianoFerieResponse InviaPianoFerie(string matricolaDipendente, int anno, string sedegapp, 
            string rep,string matricolaOperatore, string notaSegreteria);

        [OperationContract]
        SetRuoloResponse SetRuolo(string matricola, string dataMese, string sede);

        [OperationContract]
        ResocontiResponse RiCreaResoconti(List<SedeData> SediData);

        [OperationContract]
        GetRipianificazioniResponse GetRipianificazioni(string[] SediGapp, DateTime DataInizio, DateTime DataFine);


        [OperationContract]
        string[] GetEccezioniRicalcolate ( );
        [OperationContract]
        SbloccaEccezioniResponse SbloccaEccezione ( string MatricolaSegreteria , string MatricolaDipendente , DateTime Data , string Eccezione , string Motivo );


        /// <summary>
        /// Aggiornamento di una eccezione
        /// </summary>
        /// <param name="id">Identificativo univoco dell'eccezione di cui si intendono aggiornare i valori</param>
        /// <param name="matricola">Matricola del dipendente per il quale si intende inserire l'eccezione</param>
        /// <param name="data">Data di riferimento in cui il dipendente usufruirà dell'eccezione</param>
        /// <param name="codice">Codice dell'eccezione</param>
        /// <param name="dalle">Eventuale ora di inizio eccezione</param>
        /// <param name="alle">Eventuale ora di fine dell'eccezione</param>
        /// <param name="motivo">Motivo dell'eccezione</param>
        /// <param name="nota">Nota aggiuntiva all'eccezione</param>
        /// <returns></returns>
        [OperationContract]
        AggiornaNotaSegreteriaResponse AggiornaNotaSegreteria ( int id , string matricola , DateTime data , string codice , DateTime? dalle , DateTime? alle , string motivo , string nota );

        [OperationContract]
        AllineaGiornataResponse AllineaGiornata ( DateTime date , string matricola );

        /// <summary>
        /// aggiorna le informazioni di un dipendente
        /// </summary>
        /// <param name="idInfo">id informazione</param>
        /// <param name="matricola">matricola dipendente</param>
        /// <param name="valoreInfo">valore informazione</param>
        /// <param name="dataInizioValidita">data inizio validità</param>
        /// <param name="dataFineValidita">data fine validità</param>
        /// <param name="idTipoInformazione">id tipologia informazione</param>
        /// <param name="noteInfo">note</param>
        /// <returns></returns>
        [OperationContract]
        CambiaInfoDipendenteResponse cambiaInformazioniDipendente ( string matricola , string valoreInfo , DateTime? dataInizioValidita , DateTime? dataFineValidita , int idTipoInformazione , string noteInfo );

        [OperationContract]
        CambiaStatoResponse CambiaStato ( int IdRichiesta , EnumStatiRichiesta stato , string matricola , string nota );

        [OperationContract]
        string ComunicaCICS ( string S );

        /// <summary>
        /// permette l'inserimento di una nuova informazione per il dipendente
        /// </summary>
        /// <param name="matricola">matricola del dipendente</param>
        /// <param name="idTipoInfoDipendente">id tipo informazione dipendente</param>
        /// <param name="valoreInfo">Valore info</param>
        /// <param name="dataInizioValidita">data inizio validità</param>
        /// <param name="dataFineValidita">data fine validità</param>
        /// <param name="noteInfo">note</param>
        /// <returns></returns>
        [OperationContract]
        NuoveInfoDipendenteResponse creaNuovaInfoDipendente ( string matricola , int idTipoInfoDipendente , string valoreInfo , DateTime dataInizioValidita , DateTime? dataFineValidita , string noteInfo );

        /// <summary>
        /// inserisce una nuova tipologia di informazione
        /// </summary>
        /// <param name="nomeTipologia">denominazione della tipologia</param>
        /// <param name="dataInizioValidita">data inizio validità della tipologia</param>
        /// <param name="dataFineValidita">data fine validità della tipologia</param>
        /// <param name="noteTipo">note</param>
        /// <param name="tipoValore">Tipo del valore che rappresenta la tipologia</param>
        /// <returns></returns>
        [OperationContract]
        NuovaTipologiaInfoDipendente creaNuovaTipologiaInfoDipendente ( string nomeTipologia , DateTime dataInizioValidita , DateTime? dataFineValidita , string noteTipo , string tipoValore );

        /// <summary>
        /// Elimina un'informazione per il dipendente
        /// </summary>
        /// <param name="matricola">matricola dipendente</param>
        /// <param name="idTipologiaInformazione">id tipologia informazione</param>
        /// <returns></returns>
        [OperationContract]
        EliminaInfoDipendenteResponse EliminaInfoDipendente ( string matricola , int idTipologiaInformazione );

        [OperationContract]
        GetAnalisiEccezioniResponse GetAnalisiEccezioni ( string matricola , DateTime DataStart , DateTime DataEnd , string eccezione1 , string eccezione2 , string eccezione3 );

        [OperationContract]
        GetAnalisiEccezioniResponse GetAnalisiEccezioni2 ( string matricola , DateTime DataStart , DateTime DataEnd , string eccezione1 , string eccezione2 , string eccezione3 );

        [OperationContract]
        DettaglioRichiesta[] GetDettagli ( string[] SediGapp , DateTime DataInizio , DateTime DataFine , EnumWorkflowRichieste WorkFlow , EnumStatiRichiesta stato, EnumTipoRicerca? Ricerca=null );

        [OperationContract]
        DettaglioGiornataResponse GetDettagliGiornata ( DateTime data , string matricola );

        [OperationContract]
        GetDipendentiResponse getDipendentiPeriodo ( string matricola , string sedegapp , DateTime DataStart , DateTime DataEnd );

        [OperationContract]
        GetDocumentoEccezioneResponse GetDocumentoEccezione ( int IdDocumento );

        [OperationContract]
        GetEccezioniAmmesseResponse GetEccezioniAmmesse ( );

        [OperationContract]
        GetPianoFeriePDFResponse GetPianoFeriePDF ( string sede , int anno );

        [OperationContract]
        GetPianoFerieAnnoResponse GetPianoFerieAnno ( string sede , int anno, bool soloStato = false );

        [OperationContract]
        ProvvedimentiCauseResponse GetProvvedimentiCause ( string MatricolaChiamante , string MatricolaOggetto );

        [OperationContract]
        GetEccezioniComplessiveResponse GetEccezioniComplessive ( );

        /// <summary>
        /// Reperimento dell'elenco delle visualizzazioni 
        /// per la matricola passata nel range fissato
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="dataDa"></param>
        /// <param name="dataA"></param>
        /// <param name="visualizzato"></param>
        /// <returns></returns>
        [OperationContract]
        VisualizzazioniGiornataResponse_Elenco GetElencoVisualizzazioneGiornata ( string matricola , DateTime dataDa , DateTime dataA , bool? visualizzati = null );

        /// <summary>
        /// Reperimento delle eccezioni di una
        /// particolara matricola nel range passato
        /// Includendo lo stato Visualizzato o meno e se
        /// è in approvazione o meno
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="dataDa"></param>
        /// <param name="dataA"></param>
        /// <param name="visualizzato"></param>
        /// <returns></returns>
        [OperationContract]
        StatoEccezioniGiornateResponse GetStatoEccezioniGiornate ( string matricola , DateTime dataDa , DateTime dataA );

        [OperationContract]
        GetFerieResponse GetFerie ( string matricola , string anno );

        /// <summary>
        /// Estrae le informazioni dipendente per matricola
        /// </summary>
        /// <param name="matricola">La matricola del dipendente</param>
        /// <param name="valide">Se true verranno estratte solo le info valide (con data validità futura o NULL)</param>
        /// <returns></returns>
        [OperationContract]
        GetInfoDipendenteResponse GetInformazioniDipendente ( string matricola , Boolean soloValide );

        /// <summary>
        /// torna la lista delle sedi con il rispettivo giorno della settimana (1= Lunedì ... 7=Domenica)
        /// </summary>
        /// <param name="codiciSede">lista dei codici sede gap da cercare</param>
        /// <returns></returns>
        [OperationContract]
        GetSettimanaSedi GetListaSediGappSettimana ( string[] codiciSede );

        /// <summary>
        /// Reperimento delle note di segreteria per l'utente e la data passate
        /// </summary>
        /// <param name="matricola">Matricola dell'utente di cui si intende ottenere le informazioni</param>
        /// <param name="data">Data in cui cercare le note</param>
        /// <returns></returns>
        [OperationContract]
        NotaSegreteriaResponse GetNoteSegreteria ( string matricola , DateTime data );

        /// <summary>
        /// Reperimento delle note di segreteria nella data indicata
        /// </summary>
        /// <param name="data">Data in cui cercare le note</param>
        /// <returns></returns>
        [OperationContract]
        NotaSegreteriaResponse GetNoteSegreteriaPerData ( DateTime data );

        [OperationContract]
        getOrarioResponse getOrario ( string codiceOrario , string data , string matricola , string options , int livelloUtente );

        //usati da Cristiano & Ale
        [OperationContract]
        Riepilogo GetRiepilogo ( string[] SediGapp , DateTime? DataInizio , DateTime? DataFine , EnumWorkflowRichieste? WorkFlow = null, EnumTipoRicerca? Ricerca=null);

        [OperationContract]
        GetSchedaPresenzeMeseResponse GetSchedaPresenzeMese ( string matricola , DateTime dstart , DateTime dend );

        /// <summary>
        /// Metodo per il reperimento dei dati della situazione debitoria di un utente.
        /// </summary>
        /// <param name="matricolaRichiedente">Matricola dell'utente che effettua la richiesta</param>
        /// <param name="matricolaRichiesta">Matricola dell'utente di cui si intende ottenere le informazioni</param>
        /// <returns></returns>
        [OperationContract]
        SituazioneDebitoriaResponse GetSituazioneDebitoria ( string matricolaRichiedente , string matricolaRichiesta );
        [OperationContract]
        DatiStorno[] GetStorni ( string[] ndoc );

        [OperationContract]
        GetTimbratureMeseResponse GetTimbratureMese ( string matricola , DateTime dstart , DateTime dend );

        /// <summary>
        /// torna la lista delle tipologie di informazioni dipendente
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        GetTipologieInfoDipendenteResponse GetTipiInfoDipendente ( );

        [OperationContract]
        TrasferteResponse GetTrasferte ( string matricolaRichiedente );

        /// <summary>
        /// Reperimento di una visualizzazione di giornata
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="data"></param>
        /// <param name="visualizzato"></param>
        /// <returns></returns>
        [OperationContract]
        VisualizzazioneGiornataResponse GetVisualizzazioneGiornata ( string matricola , DateTime data , bool? visualizzato = null );

        /// <summary>
        /// Inserimento di una eccezione
        /// </summary>
        /// <param name="matricola">Matricola del dipendente per il quale si intende inserire l'eccezione</param>
        /// <param name="data">Data di riferimento in cui il dipendente usufruirà dell'eccezione</param>
        /// <param name="codice">Codice dell'eccezione</param>
        /// <param name="dalle">Eventuale ora di inizio eccezione</param>
        /// <param name="alle">Eventuale ora di fine dell'eccezione</param>
        /// <param name="motivo">Motivo dell'eccezione</param>
        /// <param name="nota">Nota aggiuntiva all'eccezione</param>
        /// <returns></returns>
        [OperationContract]
        InserisciNotaSegreteriaResponse InserisciNotaSegreteria ( string matricola , DateTime data , string codice , DateTime? dalle , DateTime? alle , string motivo , string nota );

        [OperationContract]
        wApiUtility.presenzeGiornaliere_resp presenzeGiornaliere ( string matricola , string sedegapp , string data );

        [OperationContract]
        wApiUtility.presenzeSettimanali_resp PresenzeSettimanali ( string matricola , string dataDa , string dataA );

        [OperationContract]
        myRaiService.classi.RecuperaPdfResponse recuperaPdf ( string sedeGapp , DateTime data , int? id );

        //usati da portale myRai (migrato da wcfApi):
        [OperationContract]
        wApiUtility.dipendente_resp recuperaUtente ( string matricola , string data );

        [OperationContract]
        RigeneraPdfResponse RigeneraPDF ( DateTime dstart , DateTime dend , string matricola , string nominativo , string sedeGapp , int livelloUtente );

        [OperationContract]
        RigeneraPdfResponse RigeneraPDFpresenze ( DateTime dstart , DateTime dend , string matricola , string nominativo ,
            string sedeGapp , int livelloUtente );

        /// <summary>
        /// Cancellazione di una eccezione a partire dal suo identificativo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        RimuoviNotaSegreteriaResponse RimuoviNotaSegreteria ( int id );

        /// <summary>
        /// aggiorna il giorno inizio settimana di una sede 
        /// </summary>
        /// <param name="codiceSedeGapp">codice sede gap</param>
        /// <param name="giornoInizio">numero giorno settimana 1=Lunedì 7=Domenica</param>
        /// <returns></returns>
        [OperationContract]
        NuovaSedeGappSettimana SalvaGiornoInizioSettimanaSede ( string codiceSedeGapp , int? giornoInizio );

        /// <summary>
        /// Inserisce o modifica una visualizzazione di giornata
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="visualizzato"></param>
        /// <param name="matricolaVisualizzatore"></param>
        /// <param name="utenteVisualizzatore"></param>
        /// <param name="dataRichiesta"></param>
        /// <param name="idRichiesta"></param>
        /// <returns></returns>
        [OperationContract]
        VisualizzazioneGiornataResponse SetVisualizzazioneGiornata ( string matricola , bool visualizzato , string matricolaVisualizzatore , string utenteVisualizzatore , DateTime dataRichiesta , int? idRichiesta = null );

        /// <summary>
        /// Metodo per l'inserimento di una nota associata ad una richiesta
        /// </summary>
        /// <param name="matricola">matricola utente chiamante</param>
        /// <param name="nomeUtente">nome esteso dell'utente chiamante</param>
        /// <param name="nota">nota da inserire</param>
        /// <param name="giornata">data esaminata</param>
        /// <param name="sedeGapp">sede gapp dell'utente chiamante</param>
        /// <param name="destinatario">tipologia del destinatario (Utente o Segreteria). default Utente</param>
        /// <param name="tipoMittente">Segreteria oppure Personale. Indica il tipo di mittente che nel caso dovesse essere inviata la mail andrà a compottere l'intestazione del messaggio</param>
        /// <returns></returns>
        [OperationContract]
        InserisciNotaRichiestaResponse InserisciNotaRichiesta ( string matricola , string nomeUtente , string nota , DateTime giornata , string sedeGapp , string destinatario = "Utente" , string tipoMittente = null );

        /// <summary>
        /// Modifica della nota già inserita
        /// </summary>
        /// <param name="idNota">identificativo univoco della nota</param>
        /// <param name="matricola">matricola dell'utente chiamante</param>
        /// <param name="nota">nota da inserire</param>
        /// <param name="tipoMittente">Segreteria oppure Personale. Indica il tipo di mittente che nel caso dovesse essere inviata la mail andrà a compottere l'intestazione del messaggio</
        /// <returns></returns>
        [OperationContract]
        ModificaNotaRichiestaResponse ModificaNotaRichiesta ( int idNota , string matricola , string nota , string tipoMittente = null );

        /// <summary>
        /// Cancellazione di una nota
        /// </summary>
        /// <param name="idNota">identificativo univoco della nota</param>
        /// <param name="matricola">matricola dell'utente chiamante</param>
        /// <returns></returns>
        [OperationContract]
        EliminaNotaRichiestaResponse EliminaNotaRichiesta ( int idNota , string matricola );

        /// <summary>
        /// Imposta come letta una nota
        /// </summary>
        /// <param name="idNota">identificativo univoco della nota</param>
        /// <param name="matricola">matricola dell'utente chiamante</param>
        /// <param name="nominativo">nominativo per esteso dell'utente chiamante</param>
        /// <param name="letta">Se impostato a false, imposta il messaggio come non letto</param>
        /// <returns></returns>
        [OperationContract]
        SetLetturaResponse SetLettura ( int idNota , string matricola , string nominativo, bool letta = true );

        /// <summary>
        /// Reperimento di una determinata nota
        /// </summary>
        /// <param name="idNota">identificativo univoco della nota di cui si intende ottenere le informazioni</param>
        /// <param name="matricola">matricola dell'utente chiamante</param>
        /// <returns></returns>
        [OperationContract]
        GetNotaRichiestaResponse GetNotaRichiesta ( int idNota , string matricola );

        /// <summary>
        /// Reperimento delle note di cui l'utente con la matricola "matricola" è creatore o destinatario, 
        /// per la giornata passata
        /// </summary>
        /// <param name="matricola">Matricola dell'utente che effettua l'operazione</param>
        /// <param name="giornata">Data per la quale recuperare le note</param>
        /// <returns></returns>
        [OperationContract]
        GetNoteRichiesteResponse GetNoteRichieste ( string matricola , DateTime giornata );

        /// <summary>
        /// Reperimento delle note per una determinata sede gapp
        /// </summary>
        /// <param name="matricola">matricola dell'utente chiamante</param>
        /// <param name="sede">sede gaap dell'utente chiamante</param>
        /// <param name="destinatario">destinatario della nota, di default "Segreteria"</param>
        /// <returns>Restituisce le note che non sono state ancora lette</returns>
        [OperationContract]
        GetNoteRichiesteResponse GetNoteRichiestePerSedeGapp ( string matricola , List<string> sedi , string destinatario = "segreteria" );

        /// <summary>
        /// Reperimento delle note per una determinata sede gapp
        /// </summary>
        /// <param name="matricola">matricola dell'utente chiamante</param>
        /// <param name="sede">sede gaap dell'utente chiamante</param>
        /// <param name="onlyNonLette">True restituisce i soli record che risultano ancora da visualizzare, False restituisce tutti i record visualizzati e non visualizzati</param>
        /// <param name="destinatario">destinatario della nota, di default "Segreteria"</param>
        /// <returns>Restituisce le note che non sono state ancora lette</returns>
        [OperationContract]
        GetNoteRichiesteResponse GetNoteRichiestePerSedeGappFiltrata ( string matricola , List<string> sedi , string destinatario = "segreteria" , bool onlyNonLette = true );

        #region Servizi per responsabile

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="sede"></param>
        /// <param name="mese"></param>
        /// <param name="anno"></param>
        /// <returns></returns>
        [OperationContract]
        Report_POH_ROH_Response GetReport_POH_ROH ( string matricola , string sede , int mese , int anno );

        [OperationContract]
        Report_EccezioniGiornalisti_Response GetLista_Eccezioni_Giornalisti ( string matricola , string sede , int mese , int anno );

        [OperationContract]
        Report_STSE_Response GetReport_STSE ( string matricola , string sede , int mese , int anno , bool inizioMese = true );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="sede"></param>
        /// <param name="mese"></param>
        /// <param name="anno"></param>
        /// <returns></returns>
        [OperationContract]
        Report_STR_STRF_Response GetReport_STR_STRF(string matricola, string sede, int mese, int anno);

        [OperationContract]
        Report_Reperibilita_Response GetReport_Reperibilita(string matricola, string sede, int mese, int anno);

        [OperationContract]
        Report_Carenza_MP_Response GetReport_Carenza_MP ( string matricola , string sede , int mese , int anno , bool inizioMese = true );
        #endregion

        #region Servizio mensa
        [OperationContract]
        GetServizioMensaResponse GetInfoMensa(string matricolaCaller, string matricola, DateTime giornata);
        #endregion

        #region Piano Ferie

        #endregion

        #region Modulo Detassazione
        [OperationContract]
        GetModuloDetassazioneResponse GetModuloDetassazione ( string pMatricolaCaller , string matricolaDestinatario );

        [OperationContract]
        SetSceltaDetassazioneResponse SetSceltaDetassazione ( string pMatricolaCaller , string matricolaDestinatario , DateTime data , string modulo , int scelta );

        [OperationContract]
        ResetModuloDetassazioneResponse ResetModuloDetassazione ( string pMatricolaCaller , string matricolaDestinatario , string applicazione );
        #endregion

        #region Conteggio giorni consecutivi
        /// <summary>
        /// 
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="sedeGapp"></param>
        /// <param name="dataPartenza"></param>
        /// <returns></returns>
        [OperationContract]
        ConteggioGiorniConsecutivi_Response GetGiorniConsecutivi ( string matricola , string sedeGapp , DateTime dataPartenza );
        #endregion

        [OperationContract]
        VersionResponse Version ( );

        #region Approvatori Produzione

        [OperationContract]
        GetApprovatoriProduzioneResponse GetApprovatoriProduzione ( string matricola );

        [OperationContract]
        SetApprovatoreProduzioneResponse SetApprovatoreProduzione ( string matricola, ApprovatoriProduzioneItem item , bool checkIfExists = false );

        [OperationContract]
        GetApprovatoreProduzioneResponse GetApprovatoriProduzioneByTitolo ( string matricola , string titolo );

        [OperationContract]
        GetApprovatoriProduzioneResponse GetProduzioniByApprovatore ( string matricola , string matricolaApprovatore );

        [OperationContract]
        ServiceResponseBase DeleteApprovatoreProduzione ( string matricola , ApprovatoriProduzioneItem item );


        #endregion

        [OperationContract]
        GetContatoriEccezioniResponse GetContatoriEccezioni ( string matricola , DateTime DataStart , DateTime DataEnd , List<string> eccezioni );

        /// <summary>
        /// Inserimento timbratura su CICS
        /// </summary>
        /// <param name="timbratura">Transazione in formato pronto per l'upload su CICS</param>
        /// <returns></returns>
        [OperationContract]
        InserisciTimbraturaResponse InserisciTimbratura(string timbratura);
    }
}