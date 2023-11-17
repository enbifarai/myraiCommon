using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using myRaiData;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using myRaiHelper;
using myRaiServiceHub.Autorizzazioni;
using System.Globalization;
using myRaiCommonDatacontrollers.DataControllers;

namespace myRaiCommonModel
{

    public class daApprovareModelEccVisualizzata
    {
        public string Matricola { get; set; }
        public DateTime Data { get; set; }
        public bool Visualizzato { get; set; }
        public string Visualizzatore { get; set; }
    }

    public class daApprovareModel
    {
        WSDigigapp datiBack = new WSDigigapp( );
        public List<sedegappAbilitata> elencoSediEccezioni { get; set; }

        public int TotaleEccezioniRicerca { get; set; }
        public int TotEccezioniDaApprovare { get; set; }
        public int TotEccezioniApprovate { get; set; }
        public int TotEccezioniRifiutate { get; set; }

        public int TotEccezioniDaApprovareFeriePermessi { get; set; }
        public int TotEccezioniDaApprovareStraordinari { get; set; }
        public int TotEccezioniDaApprovareAltre { get; set; }

        digiGappEntities db = new digiGappEntities( );
        List<Eccezione> EccezioniDB = new List<Eccezione>( );

        public List<daApprovareModelEccVisualizzata> EccVisualizzate { get; set; }

        /// <summary>
        /// Costruttore del modello
        /// </summary>
        /// <param name="ProfiliUtente">Oggeto con i profili legati ad un utente</param>
        /// <param name="conEccezioni">True per estrarre sono le sedi con Eccezioni da validare</param>
        /// <param name="livelloProfilo">01 o 02 - Indicazione il livello del responsabile</param>
		public daApprovareModel ( string pMatricola , string matricola , bool conEccezioni , string livelloProfilo , Boolean RaggruppaGliStati = false , int Da = 0 , string sede = "" , int stato = 0 , string nominativo = "" ,
			string eccezione = "" , string data_da = "" , string data_a = "" , bool? visualizzati = null , string livelloDip = "" , string soloUffProd = "" , bool RichiedeVisti = false , String StatoVisti = "" )
        {
            var tt = UtenteHelper.SediGappAccessoFirma( pMatricola );
            bool appartieneAllUffDiProduzione = false;
            List<string> elencoUfficiProduzioneAppartenenza = new List<string>( );

            int maxrows = CommonHelper.GetParametro<int>( EnumParametriSistema.MaxRowsVisualizzabiliDaApprovare );

            datiBack.Credentials = CommonHelper.GetUtenteServizioCredentials( );
            this.elencoSediEccezioni = new List<sedegappAbilitata>( );
            List<string> listaSedi = new List<string>( );

            if ( !RichiedeVisti )
                listaSedi = CommonHelper.GetSediL1(pMatricola).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );
            else
                listaSedi = CommonHelper.GetSediL6().Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );

            DateTime periodoDal, periodoAl;

            int RowsNumber = CommonHelper.GetParametro<int>( EnumParametriSistema.RowsPerScroll );
            DateTime D48 = DateTime.Now.AddHours( CommonHelper.GetParametro<int>( EnumParametriSistema.OreRichiesteUrgenti ) );

            var queryRichiestePerSede = ( IQueryable<MyRai_Richieste> ) db.MyRai_Richieste;
            List<string> SediLiv2_NOT_Liv1 = new List<string>( );

            if ( !RichiedeVisti && UtenteHelper.IsBossLiv2( pMatricola ) )
            {
                SediLiv2_NOT_Liv1 = CommonHelper.GetSediL2( pMatricola ).Except( CommonHelper.GetSediL1( pMatricola ) ).ToList( );
            }

            if ( !RichiedeVisti && UtenteHelper.IsBoss( pMatricola ) )
            {
                var sediL2 = CommonHelper.GetSediL2( pMatricola );
                var sediL1 = CommonHelper.GetSediL1( pMatricola ).Select( x => x.Substring( 0 , 5 ) ).ToList( );
                ;

                if ( !string.IsNullOrEmpty( nominativo ) ||
                    !string.IsNullOrEmpty( eccezione ) ||
                    !string.IsNullOrEmpty( data_da ) ||
                    !string.IsNullOrEmpty( data_a ) )
                {
                    var ufficiDiProduzione = db.MyRai_UffProduzioni_Approvatori.Where( w => w.MatricolaApprovatore.Equals( matricola ) ).ToList( ).Select( w => w.CodUfficio ).ToList( );

                    if ( ufficiDiProduzione != null && ufficiDiProduzione.Any( ) )
                    {
                        elencoUfficiProduzioneAppartenenza.AddRange( db.MyRai_ApprovatoriProduzioni.Where( w => ufficiDiProduzione.Contains( w.MatricolaApprovatore ) ).ToList( ).Select( w => w.SedeGapp ).ToList( ) );
                        appartieneAllUffDiProduzione = true;
                    }
                }

                queryRichiestePerSede = queryRichiestePerSede.Where( x =>
                     ( sediL2.Contains( x.codice_sede_gapp ) && sediL1.Contains( x.codice_sede_gapp ) )
                     ||
                     ( sediL2.Contains( x.codice_sede_gapp ) && !sediL1.Contains( x.codice_sede_gapp ) && x.richiedente_L1 )
                     ||
                     ( !sediL2.Contains( x.codice_sede_gapp ) && sediL1.Contains( x.codice_sede_gapp ) && !x.richiedente_L1 )
                     ||
                     ( elencoUfficiProduzioneAppartenenza.Contains( x.codice_sede_gapp ) )
                    );
            }

            if ( !RichiedeVisti && livelloDip == "01" )
            {

                CategorieDatoAbilitate response = CommonHelper.Get_CategoriaDato_Net_Cached( 1 );

                var primiLivMieSedi = response.CategorieDatoAbilitate_Array.Where( x => listaSedi.Contains( x.Codice_categoria_dato ) )
                    .Select( z => z.DT_Utenti_CategorieDatoAbilitate.AsEnumerable( )
                         .Where( y => y.Field<string>( "logon_id" ) != pMatricola )
                         .Select( w => w.Field<string>( "logon_id" ).Substring( 1 ) ) )
                        .SelectMany( a => a )
                        .Distinct( );

                queryRichiestePerSede = queryRichiestePerSede.Where( x => primiLivMieSedi.Contains( x.matricola_richiesta ) );
            }

            if ( nominativo != "" )
            { queryRichiestePerSede = queryRichiestePerSede.Where( x => x.nominativo.Contains( nominativo ) ); }

            if ( sede == "" )
            {
                if ( !RichiedeVisti && appartieneAllUffDiProduzione )
                {
                    queryRichiestePerSede = queryRichiestePerSede.Where( x => ( listaSedi.Contains( x.codice_sede_gapp ) && x.richiedente_L1 ) ||
                                                                            ( SediLiv2_NOT_Liv1.Contains( x.codice_sede_gapp ) && x.richiedente_L1 ) ||
                                                                            elencoUfficiProduzioneAppartenenza.Contains( x.codice_sede_gapp ) );
                }
                else
                {
                    queryRichiestePerSede = queryRichiestePerSede.Where( x => listaSedi.Contains( x.codice_sede_gapp ) ||
                         ( SediLiv2_NOT_Liv1.Contains( x.codice_sede_gapp ) && x.richiedente_L1 )
                        );
                }
            }
            else
            {
                if ( !RichiedeVisti && appartieneAllUffDiProduzione )
                {
                    queryRichiestePerSede = queryRichiestePerSede.Where( x => ( x.codice_sede_gapp == sede && x.richiedente_L1 ) ||
                                                                        ( SediLiv2_NOT_Liv1.Contains( x.codice_sede_gapp ) && x.richiedente_L1 ) ||
                                                                        elencoUfficiProduzioneAppartenenza.Contains( x.codice_sede_gapp ) );
                }
                else
                {
                    queryRichiestePerSede = queryRichiestePerSede.Where( x => x.codice_sede_gapp == sede ||
                         ( SediLiv2_NOT_Liv1.Contains( x.codice_sede_gapp ) && x.richiedente_L1 ));
                }
            }
            var lista = queryRichiestePerSede.ToList().Where(x=>x.codice_sede_gapp=="5CD60").ToList();

            if ( !RichiedeVisti )
                queryRichiestePerSede = queryRichiestePerSede.Where( x => x.id_stato == stato );
            else
            {
                int StatoApprovazioneFiltrato = 0;
                if (SessionHelper.Get("stato") != null && (SessionHelper.Get("stato") as string) != "")
                {
                    StatoApprovazioneFiltrato = Convert.ToInt32(SessionHelper.Get("stato"));
                    queryRichiestePerSede = queryRichiestePerSede.Where(x => (x.id_stato == StatoApprovazioneFiltrato));
                }
                else
                    queryRichiestePerSede = queryRichiestePerSede.Where(x => (x.id_stato == 10 || x.id_stato == 20));

               

                if ( String.IsNullOrWhiteSpace( StatoVisti ) )
                    queryRichiestePerSede = queryRichiestePerSede.Where( x =>  x.MyRai_Eccezioni_Richieste.Any( z => z.matricola_visto == null ) );
                else if ( StatoVisti == "VP" )
                {
                    queryRichiestePerSede = queryRichiestePerSede.Where( x =>  x.MyRai_Eccezioni_Richieste.Any( z => z.data_visto_validato != null ) );
                }
                else if ( StatoVisti == "VN" )
                {
                    queryRichiestePerSede = queryRichiestePerSede.Where( x => x.MyRai_Eccezioni_Richieste.Any( z => z.data_visto_rifiutato != null ) );
                }
                stato = 0;
            }

            if ( data_da != "" && data_a == "" )
            {
                periodoDal = DateTime.Parse( data_da );
                queryRichiestePerSede = queryRichiestePerSede.Where( x => x.periodo_dal >= periodoDal && x.periodo_dal <= DateTime.Now );
            }
            else if ( data_da != "" && data_a != "" )
            {
                periodoDal = DateTime.Parse( data_da );
                periodoAl = DateTime.Parse( data_a );
                queryRichiestePerSede = queryRichiestePerSede.Where( x => x.periodo_dal >= periodoDal && x.periodo_dal <= periodoAl );
            }
            if ( data_da == "" && data_a == "" && RichiedeVisti )
            {
                string dc = UtenteHelper.GetDateBackPerEvidenze( );
                if ( DateTime.TryParseExact( dc , "ddMMyyyy" , null , DateTimeStyles.None , out periodoDal ) )
                {
                    queryRichiestePerSede = queryRichiestePerSede.Where( x => x.periodo_dal >= periodoDal );
                }
            }
            if ( eccezione != "" )
            { queryRichiestePerSede = queryRichiestePerSede.Where( x => x.MyRai_Eccezioni_Richieste.Any( y => y.cod_eccezione == eccezione ) ); }
            queryRichiestePerSede = queryRichiestePerSede.OrderBy( x => x.data_richiesta );

            this.TotEccezioniDaApprovare = db.MyRai_Richieste
                    .Where( x => x.id_stato == ( int ) EnumStatiRichiesta.InApprovazione && listaSedi.Contains( x.codice_sede_gapp ) ).Count( );

            IQueryable<MyRai_Richieste> RichiesteDaApprovare = Enumerable.Empty<MyRai_Richieste>( ).AsQueryable( );
            IQueryable<MyRai_Richieste> RichiesteApprovate = Enumerable.Empty<MyRai_Richieste>( ).AsQueryable( );
            IQueryable<MyRai_Richieste> RichiesteRifiutate = Enumerable.Empty<MyRai_Richieste>( ).AsQueryable( );

            if ( RaggruppaGliStati && !string.IsNullOrEmpty( sede ) )
            {
                RichiesteDaApprovare = db.MyRai_Richieste.Where( x => x.codice_sede_gapp == sede && x.id_stato == ( int ) EnumStatiRichiesta.InApprovazione ).OrderBy( x => x.periodo_dal ).Skip( Da ).Take( RowsNumber );

                this.TotEccezioniDaApprovare = RichiesteDaApprovare.Count( );

                RichiesteApprovate = db.MyRai_Richieste.Where( x => x.codice_sede_gapp == sede && x.id_stato == ( int ) EnumStatiRichiesta.Approvata ).OrderBy( x => x.periodo_dal ).Skip( Da ).Take( RowsNumber );

                this.TotEccezioniApprovate = RichiesteApprovate.Count( );

                RichiesteRifiutate = db.MyRai_Richieste.Where( x => x.codice_sede_gapp == sede && x.id_stato == ( int ) EnumStatiRichiesta.Rifiutata ).OrderBy( x => x.periodo_dal ).Skip( Da ).Take( RowsNumber );

                this.TotEccezioniRifiutate = RichiesteRifiutate.Count( );

                if ( stato == 10 )
                    queryRichiestePerSede = ( IQueryable<MyRai_Richieste> ) RichiesteDaApprovare;
                if ( stato == 20 )
                    queryRichiestePerSede = ( IQueryable<MyRai_Richieste> ) RichiesteApprovate;
                if ( stato == 50 )
                    queryRichiestePerSede = ( IQueryable<MyRai_Richieste> ) RichiesteRifiutate;
                if ( stato == 0 )
                    queryRichiestePerSede = ( IQueryable<MyRai_Richieste> ) db.MyRai_Richieste
                                       .Where( x => x.codice_sede_gapp == sede )
                                       .Where( x => new int[] { (int)EnumStatiRichiesta.InApprovazione,
                                           (int)EnumStatiRichiesta.Approvata,
                                           (int)EnumStatiRichiesta.Rifiutata }.Contains( x.id_stato ) )
                                       .OrderBy( x => x.periodo_dal ).Take( RowsNumber );
            }

            //30/03/2017 preparazione modello per la view "totaledaapprovare" 
            // per la prima chiamata (non scroll)
            if ( RaggruppaGliStati && !string.IsNullOrEmpty( sede ) && stato == 0 )
            {
                CaricaListaEccezioni( RichiesteDaApprovare , ( int ) EnumStatiRichiesta.InApprovazione , visualizzati );
                CaricaListaEccezioni( RichiesteApprovate , ( int ) EnumStatiRichiesta.Approvata , visualizzati );
                CaricaListaEccezioni( RichiesteRifiutate , ( int ) EnumStatiRichiesta.Rifiutata , visualizzati );
            }
            else
            {
                if ( RaggruppaGliStati ) //30/03/2017 preparazione modello per la view "totaledaapprovare" per l'infinite scrolling
                {
                    CaricaListaEccezioni( queryRichiestePerSede , stato , visualizzati );
                }
                else //30/03/2017 preparazione modello per la Home nella sezione nella view "da_approvare" 
                {
                    CaricaListaEccezioni( queryRichiestePerSede , stato , visualizzati );
                }
            }

            // EVENTUALMENTE COMMENTARE FRANCESCO
            if ( !String.IsNullOrEmpty( soloUffProd ) )
            {
                if ( EccezioniDB != null && EccezioniDB.Any( ) )
                {
                    List<string> listaEccezioniScartate = new List<string>( );
                    List<string> listaUfficiProduzione = new List<string>( );
                    string par = CommonHelper.GetParametro<string>( EnumParametriSistema.EccezioniEscluseL4 );
                    if ( !String.IsNullOrEmpty( par ) )
                    {
                        listaEccezioniScartate = par.Split( ',' ).ToList( );
                        listaEccezioniScartate.Add( "CTD" );
                    }

                    var uffProd = db.MyRai_UffProduzioni_Approvatori.Where( w => w.MatricolaApprovatore.Equals( matricola ) ).ToList( );

                    if ( uffProd != null && uffProd.Any( ) )
                    {
                        listaUfficiProduzione.AddRange( uffProd.Select( w => w.CodUfficio ).ToList( ) );
                    }

                    EccezioniDB = EccezioniDB.Where( w => listaEccezioniScartate.Contains( w.cod.Trim( ) ) || listaUfficiProduzione.Contains( w.ApprovatoreSelezionato ) ).ToList( );
                }
            }

            Eccezione[] eccezionidaValidareTotale = EccezioniDB.OrderBy( x => x.DataRichiesta ).ToArray( );

            //if (ProfiliUtente.ProfiliArray != null)
            // listaSedi = CaricaListaSedi(ProfiliUtente.ProfiliArray, eccezionidaValidareTotale, livelloProfilo);
            listaSedi = CaricaListaSedi( pMatricola , eccezionidaValidareTotale , RichiedeVisti);

            if ( SediLiv2_NOT_Liv1 != null && SediLiv2_NOT_Liv1.Count > 0 )
            {
                foreach ( string sedeL2 in SediLiv2_NOT_Liv1 )
                {
                    if ( this.elencoSediEccezioni != null && this.elencoSediEccezioni.Any( x => x.Codice_sede_gapp == sedeL2 ) )
                    {
                        continue;
                    }
                    sedegappAbilitata sliv2 = new sedegappAbilitata( );
                    sliv2.Codice_sede_gapp = sedeL2;
                    sliv2.Descrittiva_sede_gapp = db.L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp == sedeL2 )
                        .Select( x => x.desc_sede_gapp ).FirstOrDefault( );
                    sliv2.Accesso_in_scrittura = true;
                    sliv2.eccezionidaValidare = eccezionidaValidareTotale.Where( x => x.sedeGapp == sedeL2 && x.RichiedenteL1 ).ToArray( );
                    this.elencoSediEccezioni.Add( sliv2 );
                }
            }
            this.elencoSediEccezioni = this.elencoSediEccezioni.OrderBy( a => a.Codice_sede_gapp ).ToList( );

            this.FrecciaVisibile = ( this.elencoSediEccezioni != null &&
                ( this.TotEccezioniDaApprovare > maxrows || this.elencoSediEccezioni.Any( x => x.eccezionidaValidare.Count( ) > maxrows ) ) );

            this.MaxRowsVisualizzabili = maxrows;
            this.RaggruppamentiEccezioni = new List<MyRai_Raggruppamenti>( );

            this.RaggruppamentiEccezioni.Insert( 0 , new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 0 ,
                Descrizione = "Tutto"
            } );

            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 1 ,
                Descrizione = "Urgenti"
            } );
            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 2 ,
                Descrizione = "Scadute"
            } );
            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 3 ,
                Descrizione = "Ordinarie"
            } );
        }

        public daApprovareModel ( string pMatricola , string livelloProfilo , string nominativo , string sede , int stato , string giornoDa , string giornoA , bool? visualizzati = null )
        {
            datiBack.Credentials = CommonHelper.GetUtenteServizioCredentials( );
            this.elencoSediEccezioni = new List<sedegappAbilitata>( );
            List<string> listaSedi = new List<string>( );

            DateTime dalGiorno, alGiorno;

            this.MaxRowsVisualizzabili = CommonHelper.GetParametro<int>( EnumParametriSistema.RowsPerScroll );

            //costruisco la query in base ai parametri di input -------------------------------------------------------------
            var queryRichiestePerSede = ( IQueryable<MyRai_Richieste> ) db.MyRai_Richieste;

            if ( !string.IsNullOrEmpty( sede ) && sede != "0" )
                queryRichiestePerSede = queryRichiestePerSede.Where( x => x.codice_sede_gapp == sede );
            else
            {
                listaSedi = CommonHelper.GetSediL1( pMatricola ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).ToList( );

                //if (ProfiliUtente.ProfiliArray != null)
                //    foreach (myRai.Autorizzazioni.Profilo profilo in ProfiliUtente.ProfiliArray)
                //    {
                //        if (profilo.DT_ProfiliFunzioni.Rows.Count > 0)
                //        {
                //            if (livelloProfilo == profilo.DT_ProfiliFunzioni.Rows[0]["Codice_sottofunzione"].ToString().Substring(0, 2))
                //            {
                //                foreach (System.Data.DataRow riga in profilo.DT_CategorieDatoAbilitate.Rows)
                //                {

                //                    listaSedi.Add(riga["Codice_categoria_dato"].ToString());
                //                }
                //            }
                //        }
                //    }
                queryRichiestePerSede = queryRichiestePerSede.Where( x => listaSedi.Contains( x.codice_sede_gapp ) );
            }
            if ( stato != 0 )
                queryRichiestePerSede = queryRichiestePerSede.Where( x => x.id_stato == stato );

            if ( !string.IsNullOrEmpty( giornoDa ) )
            {
                dalGiorno = DateTime.Parse( giornoDa );
                queryRichiestePerSede = queryRichiestePerSede.Where( x => x.periodo_dal >= dalGiorno );
            }
            if ( !string.IsNullOrEmpty( giornoA ) )
            {
                alGiorno = DateTime.Parse( giornoA );
                queryRichiestePerSede = queryRichiestePerSede.Where( x => x.periodo_al <= alGiorno );
            }

            if ( !string.IsNullOrEmpty( nominativo ) )
                queryRichiestePerSede = queryRichiestePerSede.Where( x => x.nominativo.StartsWith( nominativo ) );

            this.TotaleEccezioniRicerca = queryRichiestePerSede.Count( );

            queryRichiestePerSede = queryRichiestePerSede.OrderBy( x => x.periodo_dal ).Take( MaxRowsVisualizzabili );
            //fine costruzione query--------------------------------------------------------------------------------------------

            //23/05/2017
            CaricaListaEccezioni( queryRichiestePerSede , 0 , visualizzati );

            Eccezione[] eccezionidaValidareTotale = EccezioniDB.OrderBy( x => x.DataRichiesta ).ToArray( );

            //if (ProfiliUtente.ProfiliArray != null)
            // listaSedi = CaricaListaSedi(ProfiliUtente.ProfiliArray, eccezionidaValidareTotale, livelloProfilo);
            listaSedi = CaricaListaSedi( pMatricola , eccezionidaValidareTotale );

            this.elencoSediEccezioni = this.elencoSediEccezioni.OrderBy( a => a.Codice_sede_gapp ).ToList( );
        }

        //costruttore per ricerca eccezioni utente
        public daApprovareModel ( string pMatricola , string matricola , string livelloProfilo , int stato , string tipoEcc , string giornoDa , string giornoA , bool? visualizzati = null )
        {
            datiBack.Credentials = CommonHelper.GetUtenteServizioCredentials( );
            this.elencoSediEccezioni = new List<sedegappAbilitata>( );
            List<string> listaSedi = new List<string>( );
            string sede = UtenteHelper.SedeGapp( DateTime.Now );

            DateTime dalGiorno, alGiorno;

            this.MaxRowsVisualizzabili = CommonHelper.GetParametro<int>( EnumParametriSistema.RowsPerScroll );

            //costruisco la query in base ai parametri di input -------------------------------------------------------------
            var queryRichiestePerSede = ( IQueryable<MyRai_Richieste> ) db.MyRai_Richieste;

            queryRichiestePerSede = queryRichiestePerSede.Where( x => x.codice_sede_gapp == sede )
                .Where( x => x.matricola_richiesta == matricola );

            if ( stato != 0 )
                queryRichiestePerSede = queryRichiestePerSede.Where( x => x.id_stato == stato );

            if ( !string.IsNullOrEmpty( tipoEcc ) && tipoEcc != "0" )
                queryRichiestePerSede = queryRichiestePerSede.Where( x => x.MyRai_Eccezioni_Richieste.Any( y => y.cod_eccezione == tipoEcc ) );

            if ( !string.IsNullOrEmpty( giornoDa ) )
            {
                dalGiorno = DateTime.Parse( giornoDa );
                queryRichiestePerSede = queryRichiestePerSede.Where( x => x.periodo_dal >= dalGiorno );
            }
            if ( !string.IsNullOrEmpty( giornoA ) )
            {
                alGiorno = DateTime.Parse( giornoA );
                queryRichiestePerSede = queryRichiestePerSede.Where( x => x.periodo_al <= alGiorno );
            }

            this.TotaleEccezioniRicerca = queryRichiestePerSede.Count( );

            queryRichiestePerSede = queryRichiestePerSede.OrderBy( x => x.periodo_dal ).Take( MaxRowsVisualizzabili );
            //fine costruzione query--------------------------------------------------------------------------------------------

            //23/05/2017
            CaricaListaEccezioni( queryRichiestePerSede , 0 , visualizzati );

            Eccezione[] eccezionidaValidareTotale = EccezioniDB.OrderBy( x => x.DataRichiesta ).ToArray( );

            // if (ProfiliUtente.ProfiliArray != null)
            //listaSedi = CaricaListaSedi(ProfiliUtente.ProfiliArray, eccezionidaValidareTotale, livelloProfilo);
            listaSedi = CaricaListaSedi( pMatricola , eccezionidaValidareTotale );


            this.elencoSediEccezioni = this.elencoSediEccezioni.OrderBy( a => a.Codice_sede_gapp ).ToList( );
        }

        /// <summary>
        /// verifica di doppi profili con sedi gapp in sovrapposizione. Vince il profilo con livello più alto
        /// </summary>
        /// <param name="appoSede">Oggetto sede nuova</param>
        /// <returns></returns>
        public bool daInserire ( sedegappAbilitata appoSede )
        {
            sedegappAbilitata sedeEstratta = this.elencoSediEccezioni.Where( g => g.Codice_sede_gapp == appoSede.Codice_sede_gapp ).FirstOrDefault( );
            if ( sedeEstratta.codFunzione != null )
            {
                // se lo trovo verifico se devo aggiungerlo
                if ( ( appoSede.Accesso_in_scrittura ) && ( !sedeEstratta.Accesso_in_scrittura ) )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public struct sedegappAbilitata
        {
            public string codProfilo;
            public string codFunzione;
            public string codSottofunzione;
            public string Codice_sede_gapp;
            public string Descrittiva_sede_gapp;
            public bool Accesso_in_scrittura; // False se è in sola visualizzazione
            public bool Accesso_firma; //True se è secondo livello
            public Eccezione[] eccezionidaValidare; //eccezioni da validare per questa sede gapp
            public string RepartoCodice { get; set; }
            public string RepartoDescrizione { get; set; }
        }

        public Boolean FrecciaVisibile { get; set; }

        public int MaxRowsVisualizzabili { get; set; }

        public List<MyRai_Raggruppamenti> RaggruppamentiEccezioni { get; set; }

        void CaricaListaEccezioniOLD ( IOrderedQueryable<MyRai_Richieste> queryRichiestePerSede , int statoRich )
        {
            List<MyRai_Eccezioni_Richieste> ListEccRic;
            foreach ( var q in queryRichiestePerSede )
            {
                if ( q.MyRai_Eccezioni_Richieste.Count == 0 )
                    continue;
                int counter = 0;

                ListEccRic = q.MyRai_Eccezioni_Richieste.Where( z => z.id_stato == statoRich ).ToList( );

                foreach ( var dett in ListEccRic )
                {
                    counter++;
                    if ( counter > 1 )
                        break;

                    Eccezione ec = new Eccezione( );

                    ec.PeriodoRichiesta = CommonHelper.GetPeriodo( q , EnumFormatoPeriodo.DaApprovare2 );// q.periodo_dal.ToString("dd/MM/yyyy") + " al " + q.periodo_al.ToString("dd/MM/yyyy");

                    ec.dalle = dett.dalle != null ? ( ( DateTime ) dett.dalle ).ToString( "HH.mm" ) : null;

                    if ( dett.alle != null )
                    {
                        if ( ( ( DateTime ) dett.alle ).Date == ( ( DateTime ) dett.dalle ).Date )
                            ec.alle = ( ( DateTime ) dett.alle ).ToString( "HH.mm" );
                        else
                            ec.alle = ( ( DateTime ) dett.alle ).ToString( "HH.mm" ) + " del " + ( ( DateTime ) dett.alle ).ToString( "dd/MM" );
                    }

                    ec.IdStato = dett.id_stato;
                    ec.cod = dett.cod_eccezione;
                    ec.descrittiva_lunga = db.L2D_ECCEZIONE.Where( x => x.cod_eccezione == dett.cod_eccezione ).Select( x => x.desc_eccezione ).FirstOrDefault( );
                    ec.ndoc = dett.numero_documento.ToString( ).PadLeft( 6 , '0' );
                    ec.dataCompleta = dett.data_eccezione;
                    ec.matricola = q.matricola_richiesta;
                    ec.data = dett.data_eccezione.ToString( "dd/MM/yyyy" );
                    ec.dipendente = new Dipendente( );
                    ec.flg_storno = dett.azione == "C" ? "*" : "";
                    ec.IdEccezioneRichiesta = dett.id_eccezioni_richieste;
                    ec.IdRichiestaPadre = q.id_richiesta;
                    ec.dipendente.nome = "";
                    ec.dipendente.cognome = q.nominativo;
                    ec.sedeGapp = q.codice_sede_gapp;
                    ec.CodiceReparto = dett.MyRai_Richieste.reparto;

                    if ( !String.IsNullOrWhiteSpace( dett.motivo_richiesta ) )
                    {
                        string[] p = dett.motivo_richiesta.Split( ' ' );
                        if ( p != null && p.Length > 10 )
                        {
                            ec.MotivoRichiesta = string.Join( " " , p.Take( 10 ) ) + "...";
                        }
                        else
                            ec.MotivoRichiesta = dett.motivo_richiesta;
                    }
                    ec.descrittivaSedeGapp = db.L2D_SEDE_GAPP.Where( x => x.cod_sede == dett.codice_sede_gapp ).Select( x => x.desc_sede_gapp ).FirstOrDefault( );
                    ec.IdRaggruppamento = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == dett.cod_eccezione ).Select( x => x.id_raggruppamento ).FirstOrDefault( );

                    if ( statoRich == 2 && dett.azione != "C" )
                    {
                        //int co = dett.MyRai_Richieste.MyRai_Eccezioni_Richieste.Count();
                        if ( dett.MyRai_Richieste.MyRai_Eccezioni_Richieste.Any( x => x.azione == "C" ) )
                        {
                            ec.StornoInRichiesta = ( dett.MyRai_Richieste.MyRai_Eccezioni_Richieste.
                                Where( x => x.azione == "C" ).First( ).id_stato == ( int ) EnumStatiRichiesta.InApprovazione ) ? true : false;
                        }
                    }


                    EccezioniDB.Add( ec );
                }

            }
        }

        void CaricaListaEccezioni ( IQueryable<MyRai_Richieste> queryRichiestePerSede , int StatoRich = 0 , bool? visualizzati = null )
        {
            NoteRichiesteDataController datacontroller = new NoteRichiesteDataController( );
            DateTime dnow = DateTime.Now;
            var joinedQuery = queryRichiestePerSede

                .Join( db.L2D_SEDE_GAPP
                        .Where( aa => aa.data_inizio_validita <= dnow && aa.data_fine_validita >= dnow ) ,
                qr => qr.codice_sede_gapp ,
                l2ds => l2ds.cod_sede_gapp ,
                ( qr , l2ds ) => new { qr , l2ds.desc_sede_gapp } )

                .Join( db.L2D_ECCEZIONE ,
                q => q.qr.MyRai_Eccezioni_Richieste.FirstOrDefault( ).cod_eccezione ,
                l2de => l2de.cod_eccezione ,
                ( q , l2de ) => new
                {
                    q ,
                    ecce = new { desc = l2de.desc_eccezione , co = l2de.cod_eccezione , mis = l2de.unita_misura }
                } ).Distinct( ).Join( db.MyRai_Eccezioni_Ammesse ,
                l => l.ecce.co ,
                eca => eca.cod_eccezione ,
                ( l , eca ) => new { l , eca.id_raggruppamento } );

            foreach ( var q in joinedQuery )
            {
                int counter = 0;

                IEnumerable<MyRai_Eccezioni_Richieste> ListEccRic = q.l.q.qr.MyRai_Eccezioni_Richieste;

                if ( StatoRich != 0 )
                    ListEccRic = ListEccRic.Where( x => x.id_stato == StatoRich );

                if ( visualizzati.HasValue )
                {
                    // solo visualizzati
                    string matricolaRic = q.l.q.qr.matricola_richiesta;
                    int id_richiesta = q.l.q.qr.id_richiesta;

                    bool visualizzato = visualizzati.Value;
                    MyRai_Visualizzazione_Giornate_Da_Segreteria vis = null;

                    using ( digiGappEntities myDb = new digiGappEntities( ) )
                    {
                        var r = db.MyRai_Richieste.Where( rr => rr.id_richiesta.Equals( id_richiesta ) ).FirstOrDefault( );

                        if ( r != null )
                        {
                            DateTime dataRic = DateTime.Now;
                            DateTime start = DateTime.Now;
                            DateTime stop = DateTime.Now;

                            dataRic = r.periodo_dal;
                            start = new DateTime( r.periodo_dal.Year , r.periodo_dal.Month , r.periodo_dal.Day , 0 , 0 , 0 );
                            stop = new DateTime( r.periodo_al.Year , r.periodo_al.Month , r.periodo_al.Day , 23 , 59 , 59 );

                            if ( r.periodo_dal.Equals( r.periodo_al ) )
                            {

                                vis = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricolaRic ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).FirstOrDefault( );

                                if ( vis != null )
                                {
                                    if ( visualizzato != vis.Visualizzato )
                                    {
                                        continue;
                                    }
                                }
                                else if ( visualizzato )
                                {
                                    // se si cerca i soli utenti con visualizzato a true e viene ritornato
                                    // l'elenco vuoto, allora non ci sono record marcati come visualizzato
                                    // per l'utente e richiesta corrente, quindi salterà l'elemento
                                    continue;
                                }
                            }
                            else
                            {
                                // se sono diversi allora bisogno controllare quanti giorni intercorrono tra
                                // data dal a data a e verificare se sono tutti presenti nella tabella visualizzazioni da segreteria. Perchè se un utente prende due giorni di ferie ed il visualizzato è stato impostato solo per un giorno allora non va mostrata la dicitura "Visualizzato".
                                var diff = ( r.periodo_al - r.periodo_dal ).TotalDays + 1;

                                int differenzaGG = ( int ) diff;

                                int elementiVisualizzati = 0;

                                if ( visualizzati.Value )
                                {
                                    elementiVisualizzati = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricolaRic ) && v.DataRichiesta >= start && v.DataRichiesta <= stop && v.Visualizzato.Equals( visualizzati.Value ) ).Count( );

                                    if ( differenzaGG != elementiVisualizzati )
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }

                foreach ( var dett in ListEccRic )
                {
                    counter++;
                    if ( counter > 1 )
                        break;

                    Eccezione ec = new Eccezione( );

                    ec.PeriodoRichiesta = CommonHelper.GetPeriodo( q.l.q.qr , EnumFormatoPeriodo.DaApprovare2 );


                    var stornoSW = dett.MyRai_Richieste.MyRai_Eccezioni_Richieste.FirstOrDefault(x =>x.azione=="C" && x.cod_eccezione.Trim() == "SW");
                    if (stornoSW != null)
                    {

                    }
                    if (stornoSW != null && stornoSW.MyRai_Richieste.id_stato == 10 && stornoSW.eccezione_rimpiazzo_storno != null)
                    {
                        ec.EccezioneSostitutivaCodice = stornoSW.eccezione_rimpiazzo_storno;
                        if (stornoSW.eccezione_rimpiazzo_dalle != null)
                        {
                            ec.EccezioneSostitutivaDalle = ((DateTime)stornoSW.eccezione_rimpiazzo_dalle).ToString("HH:mm");
                        }
                        if (stornoSW.eccezione_rimpiazzo_alle != null)
                        {
                            ec.EccezioneSostitutivaAlle = ((DateTime)stornoSW.eccezione_rimpiazzo_alle).ToString("HH:mm");
                        }

                        ec.EccezioneSostitutivaSWH = (stornoSW.eccezione_rimpiazzo_richiedeSWH == true);

                    }

                    ec.dalle = dett.dalle != null ? ( ( DateTime ) dett.dalle ).ToString( "HH.mm" ) : null;

                    if ( dett.alle != null && dett.dalle != null )
                    {
                        if ( ( ( DateTime ) dett.alle ).Date == ( ( DateTime ) dett.dalle ).Date )
                            ec.alle = ( ( DateTime ) dett.alle ).ToString( "HH.mm" );
                        else
                            ec.alle = ( ( DateTime ) dett.alle ).ToString( "HH.mm" ) + " del " + ( ( DateTime ) dett.alle ).ToString( "dd/MM" );
                    }
                    else if ( dett.alle != null )
                    {
                        ec.alle = ( ( DateTime ) dett.alle ).ToString( "HH.mm" ) + " del " + ( ( DateTime ) dett.alle ).ToString( "dd/MM" );
                    }

                    int? idDoc = null;
                    if ( dett.MyRai_Richieste.MyRai_Associazione_Richiesta_Doc.Count > 0 )
                        idDoc = dett.MyRai_Richieste.MyRai_Associazione_Richiesta_Doc.First( ).id_documento;

                    ec.DataVistoPositivo = dett.data_visto_validato;
                    ec.DataVistoNegativo = dett.data_visto_rifiutato;

                    ec.IdStato = dett.id_stato;
                    ec.cod = dett.cod_eccezione;
                    ec.descrittiva_lunga = q.l.ecce.desc;
                    ec.ndoc = dett.numero_documento.ToString( ).PadLeft( 6 , '0' );
                    ec.dataCompleta = dett.data_eccezione;
                    ec.matricola = q.l.q.qr.matricola_richiesta;
                    ec.data = dett.data_eccezione.ToString( "dd/MM/yyyy" );
                    ec.dipendente = new Dipendente( );
                    ec.flg_storno = dett.azione == "C" ? "*" : "";
                    ec.IdEccezioneRichiesta = dett.id_eccezioni_richieste;
                    ec.IdRichiestaPadre = q.l.q.qr.id_richiesta;
                    ec.dipendente.nome = "";
                    ec.dipendente.cognome = q.l.q.qr.nominativo;
                    ec.matricolaPrimoLivello = dett.matricola_primo_livello;
                    ec.DescrizioneApprovatorePrimoLivello = dett.nominativo_primo_livello;
                    ec.sedeGapp = q.l.q.qr.codice_sede_gapp;
                    ec.DataRichiesta = dett.data_eccezione;
                    ec.DataInserimento = dett.data_creazione;
                    ec.IsUrgent = dett.MyRai_Richieste.urgente;
                    ec.IsOverdue = dett.MyRai_Richieste.scaduta;
                    ec.IdDocumentoAssociato = idDoc;
                    ec.CodiceReparto = dett.MyRai_Richieste.reparto;
                    ec.unita_mis = q.l.ecce.mis;

                    var noteRichiesta = datacontroller.GetNoteRichieste(q.l.q.qr.matricola_richiesta , dett.data_eccezione);
                    ec. HasNotaSegreteria = (noteRichiesta != null && noteRichiesta.Any( ));

                    if ( q.l.ecce.mis.Equals( "H" , StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        string hh = string.Empty;
                        string mm = string.Empty;

                        if ( dett.quantita != null )
                        {
                            int comma = dett.quantita.GetValueOrDefault( ).ToString( ).IndexOf( ',' );

                            if ( comma > -1 )
                            {
                                hh = dett.quantita.GetValueOrDefault( ).ToString( ).Substring( 0 , comma );
                                mm = dett.quantita.GetValueOrDefault( ).ToString( ).Substring( comma + 1 , ( dett.quantita.GetValueOrDefault( ).ToString( ).Length - ( comma + 1 ) ) );

                                hh = hh.PadLeft( 2 , '0' );
                                mm = mm.PadLeft( 2 , '0' );

                                ec.qta = String.Format( "{0}.{1}" , hh , mm );
                            }
                            else
                            {
                                ec.qta = dett.quantita.GetValueOrDefault( ).ToString( );
                            }
                        }
                    }

                    if ( !String.IsNullOrWhiteSpace( dett.motivo_richiesta ) )
                    {
                        string[] p = dett.motivo_richiesta.Split( ' ' );
                        if ( p != null && p.Length > 10 )
                        {
                            ec.MotivoRichiesta = string.Join( " " , p.Take( 10 ) ) + "...";
                        }
                        else
                            ec.MotivoRichiesta = dett.motivo_richiesta;
                    }
                    ec.descrittivaSedeGapp = q.l.q.desc_sede_gapp;
                    ec.IdRaggruppamento = q.id_raggruppamento;

                    ec.CodiceTurno = dett.turno;
                    L2D_ORARIO orario = db.L2D_ORARIO.FirstOrDefault( x => x.cod_orario == dett.turno );
                    if ( orario != null )
                        ec.CodiceTurno += " - " + orario.desc_orario;

                    if ( dett.id_stato == ( int ) EnumStatiRichiesta.Approvata && dett.azione != "C" )
                    {
                        if ( dett.MyRai_Richieste.MyRai_Eccezioni_Richieste.Any( x => x.azione == "C" ) )
                        {
                            ec.StornoInRichiesta = ( dett.MyRai_Richieste.MyRai_Eccezioni_Richieste.
                                Where( x => x.azione == "C" ).First( ).id_stato == ( int ) EnumStatiRichiesta.InApprovazione ) ? true : false;
                        }
                    }
                    ec.RichiedenteL1 = dett.MyRai_Richieste.richiedente_L1;
                    ec.RichiedenteL2 = dett.MyRai_Richieste.richiedente_L2;

                    try
                    {
                        if ( dett.MyRai_Richieste.id_Attivita_ceiton.HasValue )
                        {
                            var ceit = dett.MyRai_Richieste.MyRai_AttivitaCeiton.Titolo;
                            var idCeit = dett.MyRai_Richieste.id_Attivita_ceiton.Value;

                            ec.IdAttivitaCeiton = idCeit;
                            ec.AttivitaCeiton = ceit;
                        }
                    }
                    catch ( Exception ex )
                    {

                    }
                    EccezioniDB.Add( ec );

                    daApprovareModelEccVisualizzata ecVisualizzata = new daApprovareModelEccVisualizzata( )
                    {
                        Matricola = ec.matricola ,
                        Data = ec.dataCompleta ,
                        Visualizzato = false
                    };

                    using ( digiGappEntities myDb = new digiGappEntities( ) )
                    {

                        MyRai_Visualizzazione_Giornate_Da_Segreteria vis = null;

                        var r = db.MyRai_Richieste.Where( rr => rr.id_richiesta.Equals( ec.IdRichiestaPadre ) ).FirstOrDefault( );

                        if ( r != null )
                        {
                            DateTime dataRic = DateTime.Now;
                            DateTime start = DateTime.Now;
                            DateTime stop = DateTime.Now;

                            dataRic = r.periodo_dal;
                            start = new DateTime( r.periodo_dal.Year , r.periodo_dal.Month , r.periodo_dal.Day , 0 , 0 , 0 );
                            stop = new DateTime( r.periodo_al.Year , r.periodo_al.Month , r.periodo_al.Day , 23 , 59 , 59 );

                            if ( r.periodo_dal.Equals( r.periodo_al ) )
                            {

                                vis = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( ec.matricola ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).FirstOrDefault( );

                                if ( vis != null )
                                {
                                    ecVisualizzata.Visualizzato = vis.Visualizzato;
                                    ecVisualizzata.Visualizzatore = vis.UtenteVisualizzatore;
                                }
                            }
                            else
                            {
                                var diff = ( r.periodo_al - r.periodo_dal ).TotalDays + 1;

                                int differenzaGG = ( int ) diff;

                                int elementiVisualizzati = 0;

                                elementiVisualizzati = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( ec.matricola ) && v.DataRichiesta >= start && v.DataRichiesta <= stop && v.Visualizzato.Equals( true ) ).Count( );

                                if ( differenzaGG == elementiVisualizzati )
                                {
                                    ecVisualizzata.Visualizzato = true;
                                }
                            }
                        }
                    }

                    if ( EccVisualizzate == null )
                    {
                        EccVisualizzate = new List<daApprovareModelEccVisualizzata>( );
                    }

                    EccVisualizzate.Add( ecVisualizzata );
                }
            }
        }

        List<string> CaricaListaSedi ( string pMatricola , Eccezione[] eccezionidaValidareTotale , bool RichiedeVisti = false )
        {
            List<string> listaSedi = new List<string>( );
            if ( RichiedeVisti )
                listaSedi = CommonHelper.GetSediL6( ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );
            else
                listaSedi = CommonHelper.GetSediL1( pMatricola).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );

            foreach ( string sede in listaSedi )
            {
                sedegappAbilitata appoSede = new sedegappAbilitata( );
                appoSede.Codice_sede_gapp = sede;
                appoSede.Descrittiva_sede_gapp = new digiGappEntities( ).L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp == appoSede.Codice_sede_gapp )
                                                             .Select( x => x.desc_sede_gapp ).FirstOrDefault( );

                appoSede.eccezionidaValidare = eccezionidaValidareTotale.Where( a => a.sedeGapp == appoSede.Codice_sede_gapp ).ToArray( );
                if ( appoSede.eccezionidaValidare.Count( ) > 0 )
                {
                    this.elencoSediEccezioni.Add( appoSede );
                }
            }
            return listaSedi;
        }

        List<string> CaricaListaSediL3 ( Eccezione[] eccezionidaValidareTotale )
        {
            List<string> listaSedi = CommonHelper.GetSediL3( CommonHelper.GetCurrentUserPMatricola()).Where( y => !this.elencoSediEccezioni.Any( a => a.Codice_sede_gapp == y ) ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );
            foreach ( string sede in listaSedi )
            {
                sedegappAbilitata appoSede = new sedegappAbilitata( );
                appoSede.Codice_sede_gapp = sede;
                appoSede.Descrittiva_sede_gapp = new digiGappEntities( ).L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp == appoSede.Codice_sede_gapp )
                                                             .Select( x => x.desc_sede_gapp ).FirstOrDefault( );

                appoSede.eccezionidaValidare = eccezionidaValidareTotale.Where( a => a.sedeGapp == appoSede.Codice_sede_gapp ).ToArray( );
                if ( appoSede.eccezionidaValidare.Count( ) > 0 )
                {
                    this.elencoSediEccezioni.Add( appoSede );

                }

            }
            return listaSedi;
        }

        /// <summary>
        /// Personalizzazione per il caricamento delle richieste per gli approvatori presi dal db e non da 
        /// CommonHelper.GetSediL1 o
        /// CommonHelper.GetSediL3
        /// </summary>
        /// <param name="eccezionidaValidareTotale"></param>
        /// <returns></returns>
        List<string> CaricaListaSediCUSTOM ( Eccezione[] eccezionidaValidareTotale )
        {
            List<string> listaSedi = CommonHelper.GetSediLApprovatoreProduzione( ).Where( y => !this.elencoSediEccezioni.Any( a => a.Codice_sede_gapp == y ) ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );
            foreach ( string sede in listaSedi )
            {
                sedegappAbilitata appoSede = new sedegappAbilitata( );
                appoSede.Codice_sede_gapp = sede;
                appoSede.Descrittiva_sede_gapp = new digiGappEntities( ).L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp == appoSede.Codice_sede_gapp )
                                                             .Select( x => x.desc_sede_gapp ).FirstOrDefault( );

                appoSede.eccezionidaValidare = eccezionidaValidareTotale.Where( a => a.sedeGapp == appoSede.Codice_sede_gapp ).ToArray( );
                if ( appoSede.eccezionidaValidare.Count( ) > 0 )
                {
                    this.elencoSediEccezioni.Add( appoSede );
                }
            }
            return listaSedi;
        }

        List<string> CaricaListaSediOldConProfili ( Profilo[] ProfiliArray , Eccezione[] eccezionidaValidareTotale ,
            string livelloProfilo )
        {
            List<string> listaSedi = new List<string>( );

            foreach ( Profilo profilo in ProfiliArray )
            {
                if ( profilo.DT_ProfiliFunzioni.Rows.Count > 0 )
                {
                    //cerco il tipo di accesso
                    string codProfilo = profilo.DT_ProfiliFunzioni.Rows[0]["Cod_Profilo"].ToString( );
                    string codFunzione = profilo.DT_ProfiliFunzioni.Rows[0]["Codice_funzione"].ToString( );
                    string codSottofunzione = profilo.DT_ProfiliFunzioni.Rows[0]["Codice_sottofunzione"].ToString( );

                    if ( livelloProfilo == codSottofunzione.Substring( 0 , 2 ) )
                    {

                        foreach ( System.Data.DataRow riga in profilo.DT_CategorieDatoAbilitate.Rows )
                        {
                            sedegappAbilitata appoSede = new sedegappAbilitata( );

                            appoSede.codProfilo = codProfilo;
                            appoSede.codFunzione = codFunzione;
                            appoSede.codSottofunzione = codSottofunzione;

                            appoSede.Codice_sede_gapp = riga["Codice_categoria_dato"].ToString( );
                            if ( appoSede.Codice_sede_gapp.Length > 5 )
                                appoSede.Codice_sede_gapp = appoSede.Codice_sede_gapp.Substring( 0 , 5 );

                            appoSede.Descrittiva_sede_gapp = new digiGappEntities( ).L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp == appoSede.Codice_sede_gapp )
                                                                .Select( x => x.desc_sede_gapp ).FirstOrDefault( );
                            //riga["Descrizione_categoria_dato"].ToString();

                            appoSede.Accesso_firma = ( codSottofunzione.Substring( 0 , 2 ) == "02" ) ? true : false;
                            appoSede.Accesso_in_scrittura = ( codSottofunzione.Substring( 2 , 3 ) == "VIS" ) ? false : true;
                            appoSede.Accesso_in_scrittura = true;
                            appoSede.eccezionidaValidare = eccezionidaValidareTotale.Where( a => a.sedeGapp == appoSede.Codice_sede_gapp ).ToArray( );

                            if ( daInserire( appoSede ) )
                            {
                                if ( appoSede.eccezionidaValidare.Count( ) > 0 )
                                {
                                    this.elencoSediEccezioni.Add( appoSede );
                                    listaSedi.Add( appoSede.Codice_sede_gapp );
                                }
                            }
                        }
                    }
                }
            }

            return listaSedi;
        }

        private bool CiSonoSedi(List<string> a)
        {
            return (a != null && a.Any());
        }

        public daApprovareModel ( bool conEccezioni , Boolean RaggruppaGliStati = false , int Da = 0 , string sede = "" , int stato = 0 , string nominativo = "" , string eccezione = "" , string data_da = "" , string data_a = "" , bool? visualizzati = null , string livelloDip = "" , string titolo = "" )
        {
            try
            {
                var L3 = CommonHelper.GetSediL3(CommonHelper.GetCurrentUserPMatricola());
                var L4 = CommonHelper.GetSediL4( );
                var L5 = CommonHelper.GetSediL5( );

                var approvatoriL4Abilitato = CommonHelper.GetParametro<bool>( EnumParametriSistema.AbilitaApprovatoriL4L5 );
                var AbilitaSceltaApprovatore = CommonHelper.GetParametro<bool>( EnumParametriSistema.AbilitaSceltaApprovatore );

                if ( approvatoriL4Abilitato && AbilitaSceltaApprovatore )
                {
                    // se AbilitaSceltaApprovatore è true allora è in vigore la nuova modalità, cioè quella con la quale è l'utente che fa
                    // la richiesta a selezionare il suo approvatore

                    if ( CiSonoSedi( L3 ) && ( CiSonoSedi( L4 ) || CiSonoSedi( L5 ) ) )
                    {
                        daApprovareModelL345( conEccezioni , RaggruppaGliStati , Da , sede , stato , nominativo , eccezione , data_da , data_a , visualizzati , livelloDip , titolo );
                    }
                    else if ( !CiSonoSedi( L3 ) && ( CiSonoSedi( L4 ) || CiSonoSedi( L5 ) ) )
                    {
                        daApprovareModelL345( conEccezioni , RaggruppaGliStati , Da , sede , stato , nominativo , eccezione , data_da , data_a , visualizzati , livelloDip , titolo );
                    }
                    else if ( CiSonoSedi( L3 ) )
                    {
                        // TORINO ha solo L3
                        daApprovareModelL3( conEccezioni , RaggruppaGliStati , Da , sede , stato , nominativo , eccezione , data_da , data_a , visualizzati , livelloDip , titolo );
                    }
                    else
                    {
                        // qui non dovrebbe arrivarci mai
                        daApprovareModelL345( conEccezioni , RaggruppaGliStati , Da , sede , stato , nominativo , eccezione , data_da , data_a , visualizzati , livelloDip , titolo );
                    }
                }
                else if ( approvatoriL4Abilitato && !AbilitaSceltaApprovatore )
                {
                    // se non è attiva la modalità AbilitaSceltaApprovatore, gli approvatori di liv4 li prende da HRGA
                    if ( CiSonoSedi( L3 ) && ( CiSonoSedi( L4 ) || CiSonoSedi( L5 ) ) )
                    {
                        daApprovareModelL4( conEccezioni , RaggruppaGliStati , Da , sede , stato , nominativo , eccezione , data_da , data_a , visualizzati , livelloDip , titolo );
                    }
                    else if ( !CiSonoSedi( L3 ) && ( CiSonoSedi( L4 ) || CiSonoSedi( L5 ) ) )
                    {
                        daApprovareModelL4( conEccezioni , RaggruppaGliStati , Da , sede , stato , nominativo , eccezione , data_da , data_a , visualizzati , livelloDip , titolo );
                    }
                    else if ( CiSonoSedi( L3 ) )
                    {
                        // TORINO ha solo L3
                        daApprovareModelL3( conEccezioni , RaggruppaGliStati , Da , sede , stato , nominativo , eccezione , data_da , data_a , visualizzati , livelloDip , titolo );
                    }
                    else
                    {
                        // qui non dovrebbe arrivarci mai
                        daApprovareModelL345( conEccezioni , RaggruppaGliStati , Da , sede , stato , nominativo , eccezione , data_da , data_a , visualizzati , livelloDip , titolo );
                    }
                }
                else if ( approvatoriL4Abilitato )
                {
                    // TORINO ha solo L3
                    daApprovareModelL3( conEccezioni , RaggruppaGliStati , Da , sede , stato , nominativo , eccezione , data_da , data_a , visualizzati , livelloDip , titolo );
                }
            }
            catch ( Exception ex )
            {
            }
        }


        [Obsolete]
        public void daApprovareModelL4 ( bool conEccezioni , Boolean RaggruppaGliStati = false , int Da = 0 , string sede = "" , int stato = 0 , string nominativo = "" , string eccezione = "" , string data_da = "" , string data_a = "" , bool? visualizzati = null , string livelloDip = "" , string titolo = "" )
        {
            int maxrows = CommonHelper.GetParametro<int>( EnumParametriSistema.MaxRowsVisualizzabiliDaApprovare );

            datiBack.Credentials = new System.Net.NetworkCredential( CommonHelper.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] , CommonHelper.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] );
            this.elencoSediEccezioni = new List<sedegappAbilitata>( );
            List<string> listaSedi = new List<string>( );
            List<string> listaTitoli = new List<string>( );
            List<int> listaAttivitaAbilitate = new List<int>( );
            List<MyRai_ApprovatoreProduzione> approvatoreProd = new List<MyRai_ApprovatoreProduzione>( );
            bool customCheck = false;

            string matricola = CommonHelper.GetCurrentUserMatricola( );

            listaSedi.AddRange( CommonHelper.GetSediL3(CommonHelper.GetCurrentUserPMatricola()).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( ) );
            listaSedi.AddRange( CommonHelper.GetSediL4( ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( ) );

            if ( listaSedi != null && listaSedi.Any( ) )
            {
                listaSedi = listaSedi.Distinct( ).ToList( );
            }

            //customCheck = true;
            //using ( digiGappEntities db = new digiGappEntities( ) )
            //{
            //    approvatoreProd = db.MyRai_ApprovatoreProduzione.Where( w => w.MatricolaApprovatore.Equals( matricola ) ).ToList( );

            //    if ( approvatoreProd != null && approvatoreProd.Any( ) )
            //    {
            //        listaTitoli.AddRange( approvatoreProd.Select( x => x.Titolo ).ToList( ) );
            //    }
            //    }

            DateTime periodoDal, periodoAl;

            int RowsNumber = CommonHelper.GetParametro<int>( EnumParametriSistema.RowsPerScroll );
            DateTime D48 = DateTime.Now.AddHours( CommonHelper.GetParametro<int>( EnumParametriSistema.OreRichiesteUrgenti ) );

            var queryRichiestePerSedeAttCeiton = ( IQueryable<MyRai_Richieste> ) db.MyRai_Richieste;

            List<MyRai_AttivitaCeiton> listaAttivita = new List<MyRai_AttivitaCeiton>( );
            List<int> iListaAttivita = new List<int>( );

            using ( digiGappEntities db = new digiGappEntities( ) )
            {
                List<MyRai_AttivitaCeiton> temp1 = new List<MyRai_AttivitaCeiton>( );
                List<MyRai_AttivitaCeiton> temp2 = new List<MyRai_AttivitaCeiton>( );

                if ( !String.IsNullOrEmpty( sede ) )
                {
                    listaSedi.Clear( );
                    listaSedi.Add( sede );
                }

                if ( String.IsNullOrEmpty( titolo ) )
                {
                    listaAttivita = ( from attivita in db.MyRai_AttivitaCeiton
                                      join richieste in db.MyRai_Richieste
                                      on attivita.id equals richieste.id_Attivita_ceiton
                                      where richieste.id_stato == stato &&
                                      ( listaSedi.Count > 0 ? listaSedi.Contains( richieste.codice_sede_gapp ) : true )
                                      //&&
                                      //( customCheck ? listaTitoli.Contains( attivita.Titolo ) : true )
                                      select attivita ).ToList( );
                }
                else
                {
                    listaAttivita = ( from attivita in db.MyRai_AttivitaCeiton
                                      join richieste in db.MyRai_Richieste
                                      on attivita.id equals richieste.id_Attivita_ceiton
                                      where richieste.id_stato == stato &&
                                      ( listaSedi.Count > 0 ? listaSedi.Contains( richieste.codice_sede_gapp ) : true ) &&
                                      attivita.Titolo == titolo
                                      //&&
                                      //( customCheck ? listaTitoli.Contains( attivita.Titolo ) : true )
                                      select attivita ).ToList( );
                }

                if ( !String.IsNullOrWhiteSpace( data_da ) ||
                        ( !String.IsNullOrWhiteSpace( data_da ) ) )
                {
                    if ( !String.IsNullOrWhiteSpace( data_da ) )
                    {
                        DateTime d;
                        DateTime.TryParseExact( data_da , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out d );
                        temp1 = listaAttivita.Where( x => x.DataAttivita >= d ).ToList( );
                    }
                    if ( !String.IsNullOrWhiteSpace( data_a ) )
                    {
                        DateTime d;
                        DateTime.TryParseExact( data_a , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out d );
                        temp2 = listaAttivita.Where( x => x.DataAttivita <= d ).ToList( );
                    }

                    listaAttivita.Clear( );
                    listaAttivita.AddRange( temp1.ToList( ) );
                    listaAttivita.AddRange( temp2.ToList( ) );

                }
            }

            if ( listaAttivita != null && listaAttivita.Any( ) )
            {
                iListaAttivita = listaAttivita.Select( x => x.id ).ToList( );

                queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( w => w.id_Attivita_ceiton != null );
                queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( w => iListaAttivita.Contains( w.id_Attivita_ceiton.Value ) );
            }
            else
            {
                queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( w => w.id_Attivita_ceiton != null );
                queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( w => iListaAttivita.Contains( -1 ) );
            }

            if ( nominativo != "" )
            {
                queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( x => x.nominativo.Contains( nominativo ) );
            }

            if ( eccezione != "" )
            {
                queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( x => x.MyRai_Eccezioni_Richieste.Any( y => y.cod_eccezione == eccezione ) );
            }

            queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( x => !x.matricola_richiesta.Equals( matricola ) );

            queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.OrderBy( x => x.data_richiesta );

            CaricaListaEccezioni( queryRichiestePerSedeAttCeiton , stato , visualizzati );

            Eccezione[] eccezionidaValidareTotale = EccezioniDB.OrderBy( x => x.DataRichiesta ).ToArray( );

            //listaSedi = CaricaListaSedi( eccezionidaValidareTotale );
            listaSedi.AddRange( CaricaListaSediCUSTOM( eccezionidaValidareTotale ) );

            this.elencoSediEccezioni = this.elencoSediEccezioni.OrderBy( a => a.Codice_sede_gapp ).ToList( );

            this.FrecciaVisibile = ( this.elencoSediEccezioni != null &&
                ( this.TotEccezioniDaApprovare > maxrows || this.elencoSediEccezioni.Any( x => x.eccezionidaValidare.Count( ) > maxrows ) ) );

            this.MaxRowsVisualizzabili = maxrows;
            this.RaggruppamentiEccezioni = new List<MyRai_Raggruppamenti>( );

            this.RaggruppamentiEccezioni.Insert( 0 , new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 0 ,
                Descrizione = "Tutto"
            } );

            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 1 ,
                Descrizione = "Urgenti"
            } );
            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 2 ,
                Descrizione = "Scadute"
            } );
            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 3 ,
                Descrizione = "Ordinarie"
            } );
        }

        [Obsolete]
        private void daApprovareModelL345OLD ( bool conEccezioni , Boolean RaggruppaGliStati = false , int Da = 0 , string sede = "" , int stato = 0 , string nominativo = "" , string eccezione = "" , string data_da = "" , string data_a = "" , bool? visualizzati = null , string livelloDip = "" , string titolo = "" )
        {
            int maxrows = CommonHelper.GetParametro<int>( EnumParametriSistema.MaxRowsVisualizzabiliDaApprovare );

            datiBack.Credentials = new System.Net.NetworkCredential( CommonHelper.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] , CommonHelper.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] );
            this.elencoSediEccezioni = new List<sedegappAbilitata>( );
            List<string> listaSedi = new List<string>( );
            List<string> listaTitoli = new List<string>( );
            List<int> listaAttivitaAbilitate = new List<int>( );
            List<MyRai_ApprovatoreProduzione> approvatoreProd = new List<MyRai_ApprovatoreProduzione>( );
            bool customCheck = false;

            string matricola = CommonHelper.GetCurrentUserMatricola( );

            listaSedi = CommonHelper.GetSediL3(CommonHelper.GetCurrentUserPMatricola()).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );

            DateTime periodoDal, periodoAl;

            int RowsNumber = CommonHelper.GetParametro<int>( EnumParametriSistema.RowsPerScroll );
            DateTime D48 = DateTime.Now.AddHours( CommonHelper.GetParametro<int>( EnumParametriSistema.OreRichiesteUrgenti ) );

            var queryRichiestePerSedeAttCeiton = ( IQueryable<MyRai_Richieste> ) db.MyRai_Richieste;

            List<MyRai_AttivitaCeiton> listaAttivita = new List<MyRai_AttivitaCeiton>( );
            List<int> iListaAttivita = new List<int>( );

            using ( digiGappEntities db = new digiGappEntities( ) )
            {
                List<MyRai_AttivitaCeiton> temp1 = new List<MyRai_AttivitaCeiton>( );
                List<MyRai_AttivitaCeiton> temp2 = new List<MyRai_AttivitaCeiton>( );

                if ( !String.IsNullOrEmpty( sede ) )
                {
                    listaSedi.Clear( );
                    listaSedi.Add( sede );
                }

                if ( String.IsNullOrEmpty( titolo ) )
                {
                    listaAttivita = ( from attivita in db.MyRai_AttivitaCeiton
                                      join richieste in db.MyRai_Richieste
                                      on attivita.id equals richieste.id_Attivita_ceiton
                                      where richieste.id_stato == stato &&
                                      ( listaSedi.Count > 0 ? listaSedi.Contains( richieste.codice_sede_gapp ) : true )
                                      select attivita ).ToList( );
                }
                else
                {
                    listaAttivita = ( from attivita in db.MyRai_AttivitaCeiton
                                      join richieste in db.MyRai_Richieste
                                      on attivita.id equals richieste.id_Attivita_ceiton
                                      where richieste.id_stato == stato &&
                                      ( listaSedi.Count > 0 ? listaSedi.Contains( richieste.codice_sede_gapp ) : true ) &&
                                      attivita.Titolo == titolo
                                      select attivita ).ToList( );
                }

                if ( !String.IsNullOrWhiteSpace( data_da ) ||
                        ( !String.IsNullOrWhiteSpace( data_da ) ) )
                {
                    if ( !String.IsNullOrWhiteSpace( data_da ) )
                    {
                        DateTime d;
                        DateTime.TryParseExact( data_da , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out d );
                        temp1 = listaAttivita.Where( x => x.DataAttivita >= d ).ToList( );
                    }
                    if ( !String.IsNullOrWhiteSpace( data_a ) )
                    {
                        DateTime d;
                        DateTime.TryParseExact( data_a , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out d );
                        temp2 = listaAttivita.Where( x => x.DataAttivita <= d ).ToList( );
                    }

                    listaAttivita.Clear( );
                    listaAttivita.AddRange( temp1.ToList( ) );
                    listaAttivita.AddRange( temp2.ToList( ) );

                }
            }

            if ( listaAttivita != null && listaAttivita.Any( ) )
            {
                iListaAttivita = listaAttivita.Select( x => x.id ).ToList( );

                queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( w => w.id_Attivita_ceiton != null );
                queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( w => iListaAttivita.Contains( w.id_Attivita_ceiton.Value ) );
            }
            else
            {
                queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( w => w.id_Attivita_ceiton != null );
                queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( w => iListaAttivita.Contains( -1 ) );
            }

            if ( nominativo != "" )
            {
                queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( x => x.nominativo.Contains( nominativo ) );
            }

            if ( eccezione != "" )
            {
                queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.Where( x => x.MyRai_Eccezioni_Richieste.Any( y => y.cod_eccezione == eccezione ) );
            }

            queryRichiestePerSedeAttCeiton = queryRichiestePerSedeAttCeiton.OrderBy( x => x.data_richiesta );

            CaricaListaEccezioni( queryRichiestePerSedeAttCeiton , stato , visualizzati );

            Eccezione[] eccezionidaValidareTotale = EccezioniDB.OrderBy( x => x.DataRichiesta ).ToArray( );

            if ( !customCheck )
            {
                listaSedi = CaricaListaSedi(CommonHelper.GetCurrentUserPMatricola(), eccezionidaValidareTotale );
                listaSedi.AddRange( CaricaListaSediL3( eccezionidaValidareTotale ) );
            }
            else
            {
                listaSedi.AddRange( CaricaListaSediCUSTOM( eccezionidaValidareTotale ) );
            }

            this.elencoSediEccezioni = this.elencoSediEccezioni.OrderBy( a => a.Codice_sede_gapp ).ToList( );

            this.FrecciaVisibile = ( this.elencoSediEccezioni != null &&
                ( this.TotEccezioniDaApprovare > maxrows || this.elencoSediEccezioni.Any( x => x.eccezionidaValidare.Count( ) > maxrows ) ) );

            this.MaxRowsVisualizzabili = maxrows;
            this.RaggruppamentiEccezioni = new List<MyRai_Raggruppamenti>( );

            this.RaggruppamentiEccezioni.Insert( 0 , new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 0 ,
                Descrizione = "Tutto"
            } );

            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 1 ,
                Descrizione = "Urgenti"
            } );
            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 2 ,
                Descrizione = "Scadute"
            } );
            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 3 ,
                Descrizione = "Ordinarie"
            } );
        }

        public void daApprovareModelL3 ( bool conEccezioni , Boolean RaggruppaGliStati = false , int Da = 0 , string sede = "" , int stato = 0 , string nominativo = "" , string eccezione = "" , string data_da = "" , string data_a = "" , bool? visualizzati = null , string livelloDip = "" , string titolo = "" )
        {
            int maxrows = CommonHelper.GetParametro<int>( EnumParametriSistema.MaxRowsVisualizzabiliDaApprovare );

            datiBack.Credentials = new System.Net.NetworkCredential( CommonHelper.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] , CommonHelper.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] );
            this.elencoSediEccezioni = new List<sedegappAbilitata>( );
            List<string> listaSedi = new List<string>( );

            listaSedi = CommonHelper.GetSediL3(CommonHelper.GetCurrentUserPMatricola()).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( );

            string matricola = CommonHelper.GetCurrentUserMatricola( );
            DateTime periodoDal, periodoAl;

            int RowsNumber = CommonHelper.GetParametro<int>( EnumParametriSistema.RowsPerScroll );
            DateTime D48 = DateTime.Now.AddHours( CommonHelper.GetParametro<int>( EnumParametriSistema.OreRichiesteUrgenti ) );

            var queryRichiestePerSedeAttCeiton = ( IQueryable<MyRai_Richieste> ) db.MyRai_Richieste;

            List<MyRai_AttivitaCeiton> listaAttivita = new List<MyRai_AttivitaCeiton>( );
            List<int> iListaAttivita = new List<int>( );

            using ( digiGappEntities db = new digiGappEntities( ) )
            {
                List<MyRai_AttivitaCeiton> temp1 = new List<MyRai_AttivitaCeiton>( );
                List<MyRai_AttivitaCeiton> temp2 = new List<MyRai_AttivitaCeiton>( );

                if ( !String.IsNullOrEmpty( sede ) )
                {
                    listaSedi.Clear( );
                    listaSedi.Add( sede );
                }

                DateTime data_da1 = new DateTime( 1753 , 1 , 1 , 12 , 0 , 0 );
                DateTime data_a1 = new DateTime( 9999 , 12 , 31 , 23 , 59 , 59 );

                if ( !String.IsNullOrWhiteSpace( data_da ) ||
                        ( !String.IsNullOrWhiteSpace( data_da ) ) )
                {
                    if ( !String.IsNullOrWhiteSpace( data_da ) )
                    {
                        DateTime.TryParseExact( data_da , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out data_da1 );
                    }
                    if ( !String.IsNullOrWhiteSpace( data_a ) )
                    {
                        DateTime.TryParseExact( data_a , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out data_a1 );
                    }
                }

                List<GetRichiesteL3_Result> risposta = db.GetRichiesteL3( matricola , String.Join( "," , listaSedi.ToList( ) ) , stato , nominativo , eccezione , titolo , data_da1 , data_a1 ).ToList( );

                if ( risposta != null && risposta.Any( ) )
                {
                    foreach ( var item in risposta )
                    {
                        if ( visualizzati.HasValue )
                        {
                            // solo visualizzati
                            string matricolaRic = item.matricola;
                            int id_richiesta = item.IdRichiestaPadre;

                            bool visualizzato = visualizzati.Value;
                            MyRai_Visualizzazione_Giornate_Da_Segreteria vis = null;

                            using ( digiGappEntities myDb = new digiGappEntities( ) )
                            {
                                var r = db.MyRai_Richieste.Where( rr => rr.id_richiesta.Equals( id_richiesta ) ).FirstOrDefault( );

                                if ( r != null )
                                {
                                    DateTime dataRic = DateTime.Now;
                                    DateTime start = DateTime.Now;
                                    DateTime stop = DateTime.Now;

                                    dataRic = r.periodo_dal;
                                    start = new DateTime( r.periodo_dal.Year , r.periodo_dal.Month , r.periodo_dal.Day , 0 , 0 , 0 );
                                    stop = new DateTime( r.periodo_al.Year , r.periodo_al.Month , r.periodo_al.Day , 23 , 59 , 59 );

                                    if ( r.periodo_dal.Equals( r.periodo_al ) )
                                    {
                                        vis = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricolaRic ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).FirstOrDefault( );

                                        if ( vis != null )
                                        {
                                            if ( visualizzato != vis.Visualizzato )
                                            {
                                                continue;
                                            }
                                        }
                                        else if ( visualizzato )
                                        {
                                            // se si cerca i soli utenti con visualizzato a true e viene ritornato
                                            // l'elenco vuoto, allora non ci sono record marcati come visualizzato
                                            // per l'utente e richiesta corrente, quindi salterà l'elemento
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        // se sono diversi allora bisogno controllare quanti giorni intercorrono tra
                                        // data dal a data a e verificare se sono tutti presenti nella tabella visualizzazioni da segreteria. Perchè se un utente prende due giorni di ferie ed il visualizzato è stato impostato solo per un giorno allora non va mostrata la dicitura "Visualizzato".
                                        var diff = ( r.periodo_al - r.periodo_dal ).TotalDays + 1;

                                        int differenzaGG = ( int ) diff;

                                        int elementiVisualizzati = 0;

                                        if ( visualizzati.Value )
                                        {
                                            elementiVisualizzati = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricolaRic ) && v.DataRichiesta >= start && v.DataRichiesta <= stop && v.Visualizzato.Equals( visualizzati.Value ) ).Count( );

                                            if ( differenzaGG != elementiVisualizzati )
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        Eccezione ec = new Eccezione( );
                        ec.IdStato = item.IdStato;
                        ec.cod = item.cod;
                        ec.descrittiva_lunga = item.descrittiva_lunga;
                        ec.ndoc = item.ndoc;
                        ec.dataCompleta = ec.dataCompleta;
                        ec.matricola = item.matricola;
                        ec.data = item.data;
                        ec.dipendente = new Dipendente( );
                        ec.dipendente.nome = "";
                        ec.dipendente.cognome = item.bustapaga;
                        ec.flg_storno = item.flg_storno;
                        ec.IdEccezioneRichiesta = item.IdEccezioneRichiesta;
                        ec.IdRichiestaPadre = item.IdRichiestaPadre;
                        ec.matricolaPrimoLivello = item.matricolaPrimoLivello;
                        ec.DescrizioneApprovatorePrimoLivello = item.DescrizioneApprovatorePrimoLivello;
                        ec.sedeGapp = item.sedeGapp;
                        ec.DataRichiesta = item.DataRichiesta;
                        ec.DataInserimento = item.DataInserimento;
                        ec.IsUrgent = item.IsUrgent;
                        ec.IsOverdue = item.IsOverdue;
                        ec.IdDocumentoAssociato = 0;
                        ec.CodiceReparto = item.CodiceReparto;
                        ec.unita_mis = item.unita_mis;
                        ec.dalle = item.dalle;
                        ec.alle = item.alle;
                        ec.qta = item.qta;
                        ec.MotivoRichiesta = item.MotivoRichiesta;
                        ec.RichiedenteL1 = item.RichiedenteL1;
                        ec.RichiedenteL2 = item.RichiedenteL2;
                        ec.IdAttivitaCeiton = item.IdAttivitaCeiton;
                        ec.AttivitaCeiton = item.AttivitaCeiton;
                        EccezioniDB.Add( ec );

                        daApprovareModelEccVisualizzata ecVisualizzata = new daApprovareModelEccVisualizzata( )
                        {
                            Matricola = ec.matricola ,
                            Data = ec.dataCompleta ,
                            Visualizzato = false
                        };

                        using ( digiGappEntities myDb = new digiGappEntities( ) )
                        {

                            MyRai_Visualizzazione_Giornate_Da_Segreteria vis = null;

                            var r = db.MyRai_Richieste.Where( rr => rr.id_richiesta.Equals( ec.IdRichiestaPadre ) ).FirstOrDefault( );

                            if ( r != null )
                            {
                                DateTime dataRic = DateTime.Now;
                                DateTime start = DateTime.Now;
                                DateTime stop = DateTime.Now;

                                dataRic = r.periodo_dal;
                                start = new DateTime( r.periodo_dal.Year , r.periodo_dal.Month , r.periodo_dal.Day , 0 , 0 , 0 );
                                stop = new DateTime( r.periodo_al.Year , r.periodo_al.Month , r.periodo_al.Day , 23 , 59 , 59 );

                                if ( r.periodo_dal.Equals( r.periodo_al ) )
                                {

                                    vis = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( ec.matricola ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).FirstOrDefault( );

                                    if ( vis != null )
                                    {
                                        ecVisualizzata.Visualizzato = vis.Visualizzato;
                                        ecVisualizzata.Visualizzatore = vis.UtenteVisualizzatore;
                                    }
                                }
                                else
                                {
                                    var diff = ( r.periodo_al - r.periodo_dal ).TotalDays + 1;

                                    int differenzaGG = ( int ) diff;

                                    int elementiVisualizzati = 0;

                                    elementiVisualizzati = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( ec.matricola ) && v.DataRichiesta >= start && v.DataRichiesta <= stop && v.Visualizzato.Equals( true ) ).Count( );

                                    if ( differenzaGG == elementiVisualizzati )
                                    {
                                        ecVisualizzata.Visualizzato = true;
                                    }
                                }
                            }
                        }

                        if ( EccVisualizzate == null )
                        {
                            EccVisualizzate = new List<daApprovareModelEccVisualizzata>( );
                        }

                        EccVisualizzate.Add( ecVisualizzata );
                    }
                }
            }

            Eccezione[] eccezionidaValidareTotale = EccezioniDB.OrderBy( x => x.DataRichiesta ).ToArray( );

            listaSedi = CaricaListaSedi(CommonHelper.GetCurrentUserPMatricola(), eccezionidaValidareTotale );
            listaSedi.AddRange( CaricaListaSediL3( eccezionidaValidareTotale ) );

            this.elencoSediEccezioni = this.elencoSediEccezioni.OrderBy( a => a.Codice_sede_gapp ).ToList( );

            this.FrecciaVisibile = ( this.elencoSediEccezioni != null &&
                ( this.TotEccezioniDaApprovare > maxrows || this.elencoSediEccezioni.Any( x => x.eccezionidaValidare.Count( ) > maxrows ) ) );

            this.MaxRowsVisualizzabili = maxrows;
            this.RaggruppamentiEccezioni = new List<MyRai_Raggruppamenti>( );

            this.RaggruppamentiEccezioni.Insert( 0 , new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 0 ,
                Descrizione = "Tutto"
            } );

            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 1 ,
                Descrizione = "Urgenti"
            } );
            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 2 ,
                Descrizione = "Scadute"
            } );
            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 3 ,
                Descrizione = "Ordinarie"
            } );
        }

        private string CalcolaDataDalle ( MyRai_Eccezioni_Richieste dett )
        {
            Eccezione ec = new Eccezione( );

            ec.dalle = dett.dalle != null ? ( ( DateTime ) dett.dalle ).ToString( "HH.mm" ) : null;

            return ec.dalle;
        }

        private string CalcolaDataAlle ( MyRai_Eccezioni_Richieste dett )
        {
            Eccezione ec = new Eccezione( );

            ec.dalle = dett.dalle != null ? ( ( DateTime ) dett.dalle ).ToString( "HH.mm" ) : null;

            if ( dett.alle != null && dett.dalle != null )
            {
                if ( ( ( DateTime ) dett.alle ).Date == ( ( DateTime ) dett.dalle ).Date )
                    ec.alle = ( ( DateTime ) dett.alle ).ToString( "HH.mm" );
                else
                    ec.alle = ( ( DateTime ) dett.alle ).ToString( "HH.mm" ) + " del " + ( ( DateTime ) dett.alle ).ToString( "dd/MM" );
            }
            else if ( dett.alle != null )
            {
                ec.alle = ( ( DateTime ) dett.alle ).ToString( "HH.mm" ) + " del " + ( ( DateTime ) dett.alle ).ToString( "dd/MM" );
            }

            return ec.alle;
        }

        private int? GetIdDoc ( MyRai_Eccezioni_Richieste dett )
        {
            int? idDoc = null;
            if ( dett.MyRai_Richieste.MyRai_Associazione_Richiesta_Doc.Count > 0 )
                idDoc = dett.MyRai_Richieste.MyRai_Associazione_Richiesta_Doc.First( ).id_documento;

            return idDoc;
        }

        private string GetQuantita ( string unita_misura , decimal? quantita )
        {
            string result = "";

            if ( unita_misura.Equals( "H" , StringComparison.InvariantCultureIgnoreCase ) )
            {
                string hh = string.Empty;
                string mm = string.Empty;

                if ( quantita != null )
                {
                    int comma = quantita.GetValueOrDefault( ).ToString( ).IndexOf( ',' );

                    if ( comma > -1 )
                    {
                        hh = quantita.GetValueOrDefault( ).ToString( ).Substring( 0 , comma );
                        mm = quantita.GetValueOrDefault( ).ToString( ).Substring( comma + 1 , ( quantita.GetValueOrDefault( ).ToString( ).Length - ( comma + 1 ) ) );

                        hh = hh.PadLeft( 2 , '0' );
                        mm = mm.PadLeft( 2 , '0' );

                        result = String.Format( "{0}.{1}" , hh , mm );
                    }
                    else
                    {
                        result = quantita.GetValueOrDefault( ).ToString( );
                    }
                }
            }

            return result;
        }

        private string GetMotivoRichiesta ( MyRai_Eccezioni_Richieste dett )
        {
            string result = "";

            if ( !String.IsNullOrWhiteSpace( dett.motivo_richiesta ) )
            {
                string[] p = dett.motivo_richiesta.Split( ' ' );
                if ( p != null && p.Length > 10 )
                {
                    result = string.Join( " " , p.Take( 10 ) ) + "...";
                }
                else
                    result = dett.motivo_richiesta;
            }

            return result;
        }

        private string GetCodiceOrario ( MyRai_Eccezioni_Richieste dett )
        {
            string result = dett.turno;
            L2D_ORARIO orario = db.L2D_ORARIO.FirstOrDefault( x => x.cod_orario == dett.turno );
            if ( orario != null )
                result += " - " + orario.desc_orario;

            return result;
        }

        private bool VerificaStorno ( MyRai_Eccezioni_Richieste dett )
        {
            bool result = false;

            if ( dett.id_stato == ( int ) EnumStatiRichiesta.Approvata && dett.azione != "C" )
            {
                if ( dett.MyRai_Richieste.MyRai_Eccezioni_Richieste.Any( x => x.azione == "C" ) )
                {
                    result = ( dett.MyRai_Richieste.MyRai_Eccezioni_Richieste.
                        Where( x => x.azione == "C" ).First( ).id_stato == ( int ) EnumStatiRichiesta.InApprovazione ) ? true : false;
                }
            }

            return result;
        }

        private Dipendente GetDatiDipendente ( MyRai_Eccezioni_Richieste dett )
        {
            Dipendente result = new Dipendente( );
            result.nome = "";
            result.cognome = dett.MyRai_Richieste.nominativo;

            return result;
        }

        private string GetPeriodo ( MyRai_Richieste r , EnumFormatoPeriodo format )
        {
            String Periodo = "";
            if ( r.MyRai_Eccezioni_Richieste.Count == 0 )
                return "";

            //se la richiesta ha giorni diversi per periodo_dal / periodo_al
            if ( r.periodo_dal != r.periodo_al )
            {
                switch ( format )
                {
                    case EnumFormatoPeriodo.MieRichieste:
                        Periodo = "Dal " + r.periodo_dal.ToString( "dd/MM/yyyy" ) + " al " + r.periodo_al.ToString( "dd/MM/yyyy" );
                        break;
                    case EnumFormatoPeriodo.DaApprovare2:
                        Periodo = "dal " +
                            " " + r.periodo_dal.ToString( "dd/MM/yyyy" ) + Environment.NewLine + " a " +
                            " "
                            + r.periodo_al.ToString( "dd/MM/yyyy" );
                        break;
                }

            }
            else //se ha giorni uguali
            {
                MyRai_Eccezioni_Richieste ecc = r.MyRai_Eccezioni_Richieste.First( );
                //se l'eccezione figlia ha dalle/alle null
                if ( ecc.dalle == null && ecc.alle == null )
                {
                    switch ( format )
                    {
                        case EnumFormatoPeriodo.MieRichieste:
                            Periodo = r.periodo_dal.ToString( "dd/MM/yyyy" );
                            break;

                        case EnumFormatoPeriodo.DaApprovare2:
                            Periodo = r.periodo_dal.ToString( "dd/MM/yyyy" );
                            break;
                    }
                }
                else if ( ecc.dalle != null && ecc.alle == null )
                {
                    DateTime D1 = ( DateTime ) ecc.dalle;
                    switch ( format )
                    {
                        case EnumFormatoPeriodo.MieRichieste:
                            Periodo = D1.ToString( "dd/MM/yyyy" ) + " dalle " + D1.ToString( "HH.mm" );
                            break;

                        case EnumFormatoPeriodo.DaApprovare2:
                            Periodo = r.periodo_dal.ToString( "dd/MM/yyyy" );
                            break;
                    }
                }
                else if ( ecc.dalle == null && ecc.alle != null )
                {
                    DateTime D2 = ( DateTime ) ecc.alle;
                    switch ( format )
                    {
                        case EnumFormatoPeriodo.MieRichieste:
                            Periodo = r.periodo_al.ToString( "dd/MM/yyyy" );
                            break;

                        case EnumFormatoPeriodo.DaApprovare2:
                            Periodo = r.periodo_al.ToString( "dd/MM/yyyy" );
                            break;
                    }
                }
                else
                //se l'eccezione figlia ha dalle/alle valorizzate
                {
                    DateTime D1 = ( DateTime ) ecc.dalle;
                    DateTime D2 = ( DateTime ) ecc.alle;
                    if ( D1.Date == D2.Date ) // se dalle/alle sono nella stessa giornata
                    {
                        switch ( format )
                        {
                            case EnumFormatoPeriodo.MieRichieste:
                                Periodo = D1.ToString( "dd/MM/yyyy" ) + " dalle " + D1.ToString( "HH.mm" ) + " alle " +
                                    D2.ToString( "HH.mm" );
                                break;
                            case EnumFormatoPeriodo.DaApprovare2:

                                Periodo = r.periodo_dal.ToString( "dd/MM/yyyy" );
                                break;
                        }
                    }
                    else //se dalle/alle sono in giorni diversi
                    {
                        switch ( format )
                        {
                            case EnumFormatoPeriodo.MieRichieste:
                                Periodo = " Dalle " + D1.ToString( "HH.mm" ) + " del " + D1.ToString( "dd/MM/yyyy" ) +
                                    " alle " + D2.ToString( "HH.mm" ) + " del " + D2.ToString( "dd/MM/yyyy" );
                                break;
                            case EnumFormatoPeriodo.DaApprovare2:
                                Periodo = "Da " + new CultureInfo( "it-IT" ).DateTimeFormat.GetDayName( r.periodo_dal.DayOfWeek ) +
                                    " " + r.periodo_dal.ToString( "dd/MM/yyyy" );
                                break;
                        }
                    }
                }

            }
            return Periodo;

        }

        public class EccezioneExt : Eccezione
        {
            public MyRai_Eccezioni_Richieste eccezioneInternal { get; set; }
            public string Nominativo { get; set; }
        }

        private void daApprovareModelL345 ( bool conEccezioni , Boolean RaggruppaGliStati = false , int Da = 0 , string sede = "" , int stato = 0 , string nominativo = "" , string eccezione = "" , string data_da = "" , string data_a = "" , bool? visualizzati = null , string livelloDip = "" , string titolo = "" )
        {
            int maxrows = CommonHelper.GetParametro<int>( EnumParametriSistema.MaxRowsVisualizzabiliDaApprovare );

            datiBack.Credentials = new System.Net.NetworkCredential( CommonHelper.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] , CommonHelper.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] );
            this.elencoSediEccezioni = new List<sedegappAbilitata>( );
            List<string> listaTitoli = new List<string>( );
            List<string> matricoleUtenti = new List<string>( );
            List<int> listaAttivitaAbilitate = new List<int>( );

            string matricola = CommonHelper.GetCurrentUserMatricola( );

            DateTime periodoDal, periodoAl;

            int RowsNumber = CommonHelper.GetParametro<int>( EnumParametriSistema.RowsPerScroll );
            DateTime D48 = DateTime.Now.AddHours( CommonHelper.GetParametro<int>( EnumParametriSistema.OreRichiesteUrgenti ) );

            List<EccezioneExt> eccEstese = new List<EccezioneExt>( );

            List<string> listaEccezioniScartate = new List<string>( );
            List<string> listaSedi3 = new List<string>( );
            List<string> listaSedi4 = new List<string>( );
            List<string> listaSedi5 = new List<string>( );

            List<GetRichiesteL3_Result> risposta3 = new List<GetRichiesteL3_Result>( );
            List<GetRichiesteL4_Result> risposta4 = new List<GetRichiesteL4_Result>( );
            List<GetRichiesteL4_Result> risposta5 = new List<GetRichiesteL4_Result>( );

            //List<MyRai_Eccezioni_Richieste> listaEccezioni = new List<MyRai_Eccezioni_Richieste>( );
            List<Eccezione> listaEccezioni = new List<Eccezione>( );

            listaSedi3.AddRange( CommonHelper.GetSediL3(CommonHelper.GetCurrentUserPMatricola()).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( ) );
            listaSedi4.AddRange( CommonHelper.GetSediL4( ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( ) );
            listaSedi5.AddRange( CommonHelper.GetSediL5( ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).Distinct( ).ToList( ) );

            DateTime dnow = DateTime.Now;

            if ( CommonHelper.IsApprovatoreProduzione( CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                DateTime data_da1 = new DateTime( 1753 , 1 , 1 , 12 , 0 , 0 );
                DateTime data_a1 = new DateTime( 9999 , 12 , 31 , 23 , 59 , 59 );

                if ( !String.IsNullOrWhiteSpace( data_da ) ||
                        ( !String.IsNullOrWhiteSpace( data_da ) ) )
                {
                    if ( !String.IsNullOrWhiteSpace( data_da ) )
                    {
                        DateTime.TryParseExact( data_da , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out data_da1 );
                    }
                    if ( !String.IsNullOrWhiteSpace( data_a ) )
                    {
                        DateTime.TryParseExact( data_a , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out data_a1 );
                    }
                }

                if ( listaSedi3.Count > 0 )
                {
                    risposta3.AddRange( db.GetRichiesteL3( matricola , String.Join( "," , listaSedi3.ToList( ) ) , stato , nominativo , eccezione , titolo , data_da1 , data_a1 ).ToList( ) );
                }

                if ( listaSedi4.Count > 0 )
                {
                    risposta4.AddRange( db.GetRichiesteL4( matricola , String.Join( "," , listaSedi4.ToList( ) ) , stato , nominativo , eccezione , titolo , data_da1 , data_a1 ).ToList( ) );
                    risposta4.AddRange( db.GetRichiesteL4UFF( matricola , String.Join( "," , listaSedi4.ToList( ) ) , stato , nominativo , eccezione , titolo , data_da1 , data_a1 ).ToList( ) );
                }

                if ( listaSedi5.Count > 0 )
                {
                    risposta5.AddRange( db.GetRichiesteL5( matricola , String.Join( "," , listaSedi5.ToList( ) ) , stato , nominativo , eccezione , titolo , data_da1 , data_a1 ).ToList( ) );
                    risposta5.AddRange( db.GetRichiesteL5UFF( matricola , String.Join( "," , listaSedi5.ToList( ) ) , stato , nominativo , eccezione , titolo , data_da1 , data_a1 ).ToList( ) );
                }
            }

            if ( risposta3 != null && risposta3.Any( ) )
            {
                foreach ( var item in risposta3 )
                {
                    if ( visualizzati.HasValue )
                    {
                        // solo visualizzati
                        string matricolaRic = item.matricola;
                        int id_richiesta = item.IdRichiestaPadre;

                        bool visualizzato = visualizzati.Value;
                        MyRai_Visualizzazione_Giornate_Da_Segreteria vis = null;

                        using ( digiGappEntities myDb = new digiGappEntities( ) )
                        {
                            var r = db.MyRai_Richieste.Where( rr => rr.id_richiesta.Equals( id_richiesta ) ).FirstOrDefault( );

                            if ( r != null )
                            {
                                DateTime dataRic = DateTime.Now;
                                DateTime start = DateTime.Now;
                                DateTime stop = DateTime.Now;

                                dataRic = r.periodo_dal;
                                start = new DateTime( r.periodo_dal.Year , r.periodo_dal.Month , r.periodo_dal.Day , 0 , 0 , 0 );
                                stop = new DateTime( r.periodo_al.Year , r.periodo_al.Month , r.periodo_al.Day , 23 , 59 , 59 );

                                if ( r.periodo_dal.Equals( r.periodo_al ) )
                                {
                                    vis = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricolaRic ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).FirstOrDefault( );

                                    if ( vis != null )
                                    {
                                        if ( visualizzato != vis.Visualizzato )
                                        {
                                            continue;
                                        }
                                    }
                                    else if ( visualizzato )
                                    {
                                        // se si cerca i soli utenti con visualizzato a true e viene ritornato
                                        // l'elenco vuoto, allora non ci sono record marcati come visualizzato
                                        // per l'utente e richiesta corrente, quindi salterà l'elemento
                                        continue;
                                    }
                                }
                                else
                                {
                                    // se sono diversi allora bisogno controllare quanti giorni intercorrono tra
                                    // data dal a data a e verificare se sono tutti presenti nella tabella visualizzazioni da segreteria. Perchè se un utente prende due giorni di ferie ed il visualizzato è stato impostato solo per un giorno allora non va mostrata la dicitura "Visualizzato".
                                    var diff = ( r.periodo_al - r.periodo_dal ).TotalDays + 1;

                                    int differenzaGG = ( int ) diff;

                                    int elementiVisualizzati = 0;

                                    if ( visualizzati.Value )
                                    {
                                        elementiVisualizzati = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricolaRic ) && v.DataRichiesta >= start && v.DataRichiesta <= stop && v.Visualizzato.Equals( visualizzati.Value ) ).Count( );

                                        if ( differenzaGG != elementiVisualizzati )
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Eccezione ec = new Eccezione( );
                    ec.IdStato = item.IdStato;
                    ec.cod = item.cod;
                    ec.descrittiva_lunga = item.descrittiva_lunga;
                    ec.ndoc = item.ndoc;
                    ec.dataCompleta = ec.dataCompleta;
                    ec.matricola = item.matricola;
                    ec.data = item.data;
                    ec.dipendente = new Dipendente( );
                    ec.dipendente.nome = "";
                    ec.dipendente.cognome = item.bustapaga;
                    ec.flg_storno = item.flg_storno;
                    ec.IdEccezioneRichiesta = item.IdEccezioneRichiesta;
                    ec.IdRichiestaPadre = item.IdRichiestaPadre;
                    ec.matricolaPrimoLivello = item.matricolaPrimoLivello;
                    ec.DescrizioneApprovatorePrimoLivello = item.DescrizioneApprovatorePrimoLivello;
                    ec.sedeGapp = item.sedeGapp;
                    ec.DataRichiesta = item.DataRichiesta;
                    ec.DataInserimento = item.DataInserimento;
                    ec.IsUrgent = item.IsUrgent;
                    ec.IsOverdue = item.IsOverdue;
                    ec.IdDocumentoAssociato = 0;
                    ec.CodiceReparto = item.CodiceReparto;
                    ec.unita_mis = item.unita_mis;
                    ec.dalle = item.dalle;
                    ec.alle = item.alle;
                    ec.qta = item.qta;

                    if ( !String.IsNullOrWhiteSpace( item.MotivoRichiesta ) )
                    {
                        string[] p = item.MotivoRichiesta.Split( ' ' );
                        if ( p != null && p.Length > 10 )
                        {
                            ec.MotivoRichiesta = string.Join( " " , p.Take( 10 ) ) + "...";
                        }
                        else
                            ec.MotivoRichiesta = item.MotivoRichiesta;
                    }

                    //ec.MotivoRichiesta = item.MotivoRichiesta;
                    ec.RichiedenteL1 = item.RichiedenteL1;
                    ec.RichiedenteL2 = item.RichiedenteL2;
                    ec.IdAttivitaCeiton = item.IdAttivitaCeiton;
                    ec.AttivitaCeiton = item.AttivitaCeiton;
                    ec.IdApprovatoreSelezionato = item.IdApprovatoreSelezionato;
                    EccezioniDB.Add( ec );

                    daApprovareModelEccVisualizzata ecVisualizzata = new daApprovareModelEccVisualizzata( )
                    {
                        Matricola = ec.matricola ,
                        Data = ec.dataCompleta ,
                        Visualizzato = false
                    };

                    using ( digiGappEntities myDb = new digiGappEntities( ) )
                    {

                        MyRai_Visualizzazione_Giornate_Da_Segreteria vis = null;

                        var r = db.MyRai_Richieste.Where( rr => rr.id_richiesta.Equals( ec.IdRichiestaPadre ) ).FirstOrDefault( );

                        if ( r != null )
                        {
                            DateTime dataRic = DateTime.Now;
                            DateTime start = DateTime.Now;
                            DateTime stop = DateTime.Now;

                            dataRic = r.periodo_dal;
                            start = new DateTime( r.periodo_dal.Year , r.periodo_dal.Month , r.periodo_dal.Day , 0 , 0 , 0 );
                            stop = new DateTime( r.periodo_al.Year , r.periodo_al.Month , r.periodo_al.Day , 23 , 59 , 59 );

                            if ( r.periodo_dal.Equals( r.periodo_al ) )
                            {

                                vis = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( ec.matricola ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).FirstOrDefault( );

                                if ( vis != null )
                                {
                                    ecVisualizzata.Visualizzato = vis.Visualizzato;
                                    ecVisualizzata.Visualizzatore = vis.UtenteVisualizzatore;
                                }
                            }
                            else
                            {
                                var diff = ( r.periodo_al - r.periodo_dal ).TotalDays + 1;

                                int differenzaGG = ( int ) diff;

                                int elementiVisualizzati = 0;

                                elementiVisualizzati = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( ec.matricola ) && v.DataRichiesta >= start && v.DataRichiesta <= stop && v.Visualizzato.Equals( true ) ).Count( );

                                if ( differenzaGG == elementiVisualizzati )
                                {
                                    ecVisualizzata.Visualizzato = true;
                                }
                            }
                        }
                    }

                    if ( EccVisualizzate == null )
                    {
                        EccVisualizzate = new List<daApprovareModelEccVisualizzata>( );
                    }

                    EccVisualizzate.Add( ecVisualizzata );
                }
            }

            if ( risposta4 != null && risposta4.Any( ) )
            {
                foreach ( var item in risposta4 )
                {
                    if ( visualizzati.HasValue )
                    {
                        // solo visualizzati
                        string matricolaRic = item.matricola;
                        int id_richiesta = item.IdRichiestaPadre;

                        bool visualizzato = visualizzati.Value;
                        MyRai_Visualizzazione_Giornate_Da_Segreteria vis = null;

                        using ( digiGappEntities myDb = new digiGappEntities( ) )
                        {
                            var r = db.MyRai_Richieste.Where( rr => rr.id_richiesta.Equals( id_richiesta ) ).FirstOrDefault( );

                            if ( r != null )
                            {
                                DateTime dataRic = DateTime.Now;
                                DateTime start = DateTime.Now;
                                DateTime stop = DateTime.Now;

                                dataRic = r.periodo_dal;
                                start = new DateTime( r.periodo_dal.Year , r.periodo_dal.Month , r.periodo_dal.Day , 0 , 0 , 0 );
                                stop = new DateTime( r.periodo_al.Year , r.periodo_al.Month , r.periodo_al.Day , 23 , 59 , 59 );

                                if ( r.periodo_dal.Equals( r.periodo_al ) )
                                {
                                    vis = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricolaRic ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).FirstOrDefault( );

                                    if ( vis != null )
                                    {
                                        if ( visualizzato != vis.Visualizzato )
                                        {
                                            continue;
                                        }
                                    }
                                    else if ( visualizzato )
                                    {
                                        // se si cerca i soli utenti con visualizzato a true e viene ritornato
                                        // l'elenco vuoto, allora non ci sono record marcati come visualizzato
                                        // per l'utente e richiesta corrente, quindi salterà l'elemento
                                        continue;
                                    }
                                }
                                else
                                {
                                    // se sono diversi allora bisogno controllare quanti giorni intercorrono tra
                                    // data dal a data a e verificare se sono tutti presenti nella tabella visualizzazioni da segreteria. Perchè se un utente prende due giorni di ferie ed il visualizzato è stato impostato solo per un giorno allora non va mostrata la dicitura "Visualizzato".
                                    var diff = ( r.periodo_al - r.periodo_dal ).TotalDays + 1;

                                    int differenzaGG = ( int ) diff;

                                    int elementiVisualizzati = 0;

                                    if ( visualizzati.Value )
                                    {
                                        elementiVisualizzati = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricolaRic ) && v.DataRichiesta >= start && v.DataRichiesta <= stop && v.Visualizzato.Equals( visualizzati.Value ) ).Count( );

                                        if ( differenzaGG != elementiVisualizzati )
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Eccezione ec = new Eccezione( );
                    ec.IdStato = item.IdStato;
                    ec.cod = item.cod;
                    ec.descrittiva_lunga = item.descrittiva_lunga;
                    ec.ndoc = item.ndoc;
                    ec.dataCompleta = ec.dataCompleta;
                    ec.matricola = item.matricola;
                    ec.data = item.data;
                    ec.dipendente = new Dipendente( );
                    ec.dipendente.nome = "";
                    ec.dipendente.cognome = item.bustapaga;
                    ec.flg_storno = item.flg_storno;
                    ec.IdEccezioneRichiesta = item.IdEccezioneRichiesta;
                    ec.IdRichiestaPadre = item.IdRichiestaPadre;
                    ec.matricolaPrimoLivello = item.matricolaPrimoLivello;
                    ec.DescrizioneApprovatorePrimoLivello = item.DescrizioneApprovatorePrimoLivello;
                    ec.sedeGapp = item.sedeGapp;
                    ec.DataRichiesta = item.DataRichiesta;
                    ec.DataInserimento = item.DataInserimento;
                    ec.IsUrgent = item.IsUrgent;
                    ec.IsOverdue = item.IsOverdue;
                    ec.IdDocumentoAssociato = 0;
                    ec.CodiceReparto = item.CodiceReparto;
                    ec.unita_mis = item.unita_mis;
                    ec.dalle = item.dalle;
                    ec.alle = item.alle;
                    ec.qta = item.qta;
                    if ( !String.IsNullOrWhiteSpace( item.MotivoRichiesta ) )
                    {
                        string[] p = item.MotivoRichiesta.Split( ' ' );
                        if ( p != null && p.Length > 10 )
                        {
                            ec.MotivoRichiesta = string.Join( " " , p.Take( 10 ) ) + "...";
                        }
                        else
                            ec.MotivoRichiesta = item.MotivoRichiesta;
                    }

                    //ec.MotivoRichiesta = item.MotivoRichiesta;
                    ec.RichiedenteL1 = item.RichiedenteL1;
                    ec.RichiedenteL2 = item.RichiedenteL2;
                    ec.IdAttivitaCeiton = item.IdAttivitaCeiton;
                    ec.AttivitaCeiton = item.AttivitaCeiton;
                    ec.IdApprovatoreSelezionato = item.IdApprovatoreSelezionato;

                    EccezioniDB.Add( ec );

                    daApprovareModelEccVisualizzata ecVisualizzata = new daApprovareModelEccVisualizzata( )
                    {
                        Matricola = ec.matricola ,
                        Data = ec.dataCompleta ,
                        Visualizzato = false
                    };

                    using ( digiGappEntities myDb = new digiGappEntities( ) )
                    {

                        MyRai_Visualizzazione_Giornate_Da_Segreteria vis = null;

                        var r = db.MyRai_Richieste.Where( rr => rr.id_richiesta.Equals( ec.IdRichiestaPadre ) ).FirstOrDefault( );

                        if ( r != null )
                        {
                            DateTime dataRic = DateTime.Now;
                            DateTime start = DateTime.Now;
                            DateTime stop = DateTime.Now;

                            dataRic = r.periodo_dal;
                            start = new DateTime( r.periodo_dal.Year , r.periodo_dal.Month , r.periodo_dal.Day , 0 , 0 , 0 );
                            stop = new DateTime( r.periodo_al.Year , r.periodo_al.Month , r.periodo_al.Day , 23 , 59 , 59 );

                            if ( r.periodo_dal.Equals( r.periodo_al ) )
                            {

                                vis = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( ec.matricola ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).FirstOrDefault( );

                                if ( vis != null )
                                {
                                    ecVisualizzata.Visualizzato = vis.Visualizzato;
                                    ecVisualizzata.Visualizzatore = vis.UtenteVisualizzatore;
                                }
                            }
                            else
                            {
                                var diff = ( r.periodo_al - r.periodo_dal ).TotalDays + 1;

                                int differenzaGG = ( int ) diff;

                                int elementiVisualizzati = 0;

                                elementiVisualizzati = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( ec.matricola ) && v.DataRichiesta >= start && v.DataRichiesta <= stop && v.Visualizzato.Equals( true ) ).Count( );

                                if ( differenzaGG == elementiVisualizzati )
                                {
                                    ecVisualizzata.Visualizzato = true;
                                }
                            }
                        }
                    }

                    if ( EccVisualizzate == null )
                    {
                        EccVisualizzate = new List<daApprovareModelEccVisualizzata>( );
                    }

                    EccVisualizzate.Add( ecVisualizzata );
                }
            }

            if ( risposta5 != null && risposta5.Any( ) )
            {
                foreach ( var item in risposta5 )
                {
                    if ( visualizzati.HasValue )
                    {
                        // solo visualizzati
                        string matricolaRic = item.matricola;
                        int id_richiesta = item.IdRichiestaPadre;

                        bool visualizzato = visualizzati.Value;
                        MyRai_Visualizzazione_Giornate_Da_Segreteria vis = null;

                        using ( digiGappEntities myDb = new digiGappEntities( ) )
                        {
                            var r = db.MyRai_Richieste.Where( rr => rr.id_richiesta.Equals( id_richiesta ) ).FirstOrDefault( );

                            if ( r != null )
                            {
                                DateTime dataRic = DateTime.Now;
                                DateTime start = DateTime.Now;
                                DateTime stop = DateTime.Now;

                                dataRic = r.periodo_dal;
                                start = new DateTime( r.periodo_dal.Year , r.periodo_dal.Month , r.periodo_dal.Day , 0 , 0 , 0 );
                                stop = new DateTime( r.periodo_al.Year , r.periodo_al.Month , r.periodo_al.Day , 23 , 59 , 59 );

                                if ( r.periodo_dal.Equals( r.periodo_al ) )
                                {
                                    vis = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricolaRic ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).FirstOrDefault( );

                                    if ( vis != null )
                                    {
                                        if ( visualizzato != vis.Visualizzato )
                                        {
                                            continue;
                                        }
                                    }
                                    else if ( visualizzato )
                                    {
                                        // se si cerca i soli utenti con visualizzato a true e viene ritornato
                                        // l'elenco vuoto, allora non ci sono record marcati come visualizzato
                                        // per l'utente e richiesta corrente, quindi salterà l'elemento
                                        continue;
                                    }
                                }
                                else
                                {
                                    // se sono diversi allora bisogno controllare quanti giorni intercorrono tra
                                    // data dal a data a e verificare se sono tutti presenti nella tabella visualizzazioni da segreteria. Perchè se un utente prende due giorni di ferie ed il visualizzato è stato impostato solo per un giorno allora non va mostrata la dicitura "Visualizzato".
                                    var diff = ( r.periodo_al - r.periodo_dal ).TotalDays + 1;

                                    int differenzaGG = ( int ) diff;

                                    int elementiVisualizzati = 0;

                                    if ( visualizzati.Value )
                                    {
                                        elementiVisualizzati = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricolaRic ) && v.DataRichiesta >= start && v.DataRichiesta <= stop && v.Visualizzato.Equals( visualizzati.Value ) ).Count( );

                                        if ( differenzaGG != elementiVisualizzati )
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Eccezione ec = new Eccezione( );
                    ec.IdStato = item.IdStato;
                    ec.cod = item.cod;
                    ec.descrittiva_lunga = item.descrittiva_lunga;
                    ec.ndoc = item.ndoc;
                    ec.dataCompleta = ec.dataCompleta;
                    ec.matricola = item.matricola;
                    ec.data = item.data;
                    ec.dipendente = new Dipendente( );
                    ec.dipendente.nome = "";
                    ec.dipendente.cognome = item.bustapaga;
                    ec.flg_storno = item.flg_storno;
                    ec.IdEccezioneRichiesta = item.IdEccezioneRichiesta;
                    ec.IdRichiestaPadre = item.IdRichiestaPadre;
                    ec.matricolaPrimoLivello = item.matricolaPrimoLivello;
                    ec.DescrizioneApprovatorePrimoLivello = item.DescrizioneApprovatorePrimoLivello;
                    ec.sedeGapp = item.sedeGapp;
                    ec.DataRichiesta = item.DataRichiesta;
                    ec.DataInserimento = item.DataInserimento;
                    ec.IsUrgent = item.IsUrgent;
                    ec.IsOverdue = item.IsOverdue;
                    ec.IdDocumentoAssociato = 0;
                    ec.CodiceReparto = item.CodiceReparto;
                    ec.unita_mis = item.unita_mis;
                    ec.dalle = item.dalle;
                    ec.alle = item.alle;
                    ec.qta = item.qta;
                    if ( !String.IsNullOrWhiteSpace( item.MotivoRichiesta ) )
                    {
                        string[] p = item.MotivoRichiesta.Split( ' ' );
                        if ( p != null && p.Length > 10 )
                        {
                            ec.MotivoRichiesta = string.Join( " " , p.Take( 10 ) ) + "...";
                        }
                        else
                            ec.MotivoRichiesta = item.MotivoRichiesta;
                    }

                    //ec.MotivoRichiesta = item.MotivoRichiesta;
                    ec.RichiedenteL1 = item.RichiedenteL1;
                    ec.RichiedenteL2 = item.RichiedenteL2;
                    ec.IdAttivitaCeiton = item.IdAttivitaCeiton;
                    ec.AttivitaCeiton = item.AttivitaCeiton;
                    ec.IdApprovatoreSelezionato = item.IdApprovatoreSelezionato;

                    EccezioniDB.Add( ec );

                    daApprovareModelEccVisualizzata ecVisualizzata = new daApprovareModelEccVisualizzata( )
                    {
                        Matricola = ec.matricola ,
                        Data = ec.dataCompleta ,
                        Visualizzato = false
                    };

                    using ( digiGappEntities myDb = new digiGappEntities( ) )
                    {

                        MyRai_Visualizzazione_Giornate_Da_Segreteria vis = null;

                        var r = db.MyRai_Richieste.Where( rr => rr.id_richiesta.Equals( ec.IdRichiestaPadre ) ).FirstOrDefault( );

                        if ( r != null )
                        {
                            DateTime dataRic = DateTime.Now;
                            DateTime start = DateTime.Now;
                            DateTime stop = DateTime.Now;

                            dataRic = r.periodo_dal;
                            start = new DateTime( r.periodo_dal.Year , r.periodo_dal.Month , r.periodo_dal.Day , 0 , 0 , 0 );
                            stop = new DateTime( r.periodo_al.Year , r.periodo_al.Month , r.periodo_al.Day , 23 , 59 , 59 );

                            if ( r.periodo_dal.Equals( r.periodo_al ) )
                            {

                                vis = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( ec.matricola ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).FirstOrDefault( );

                                if ( vis != null )
                                {
                                    ecVisualizzata.Visualizzato = vis.Visualizzato;
                                    ecVisualizzata.Visualizzatore = vis.UtenteVisualizzatore;
                                }
                            }
                            else
                            {
                                var diff = ( r.periodo_al - r.periodo_dal ).TotalDays + 1;

                                int differenzaGG = ( int ) diff;

                                int elementiVisualizzati = 0;

                                elementiVisualizzati = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( ec.matricola ) && v.DataRichiesta >= start && v.DataRichiesta <= stop && v.Visualizzato.Equals( true ) ).Count( );

                                if ( differenzaGG == elementiVisualizzati )
                                {
                                    ecVisualizzata.Visualizzato = true;
                                }
                            }
                        }
                    }

                    if ( EccVisualizzate == null )
                    {
                        EccVisualizzate = new List<daApprovareModelEccVisualizzata>( );
                    }

                    EccVisualizzate.Add( ecVisualizzata );
                }
            }

            Eccezione[] eccezionidaValidareTotale = EccezioniDB.OrderBy( w => w.DataRichiesta ).DistinctBy( w => w.IdEccezioneRichiesta ).ToArray( );

            var listaSedi = CaricaListaSediCUSTOM( eccezionidaValidareTotale );

            this.elencoSediEccezioni = this.elencoSediEccezioni.OrderBy( a => a.Codice_sede_gapp ).ToList( );

            this.FrecciaVisibile = ( this.elencoSediEccezioni != null &&
                ( this.TotEccezioniDaApprovare > maxrows || this.elencoSediEccezioni.Any( x => x.eccezionidaValidare.Count( ) > maxrows ) ) );

            this.MaxRowsVisualizzabili = maxrows;
            this.RaggruppamentiEccezioni = new List<MyRai_Raggruppamenti>( );

            this.RaggruppamentiEccezioni.Insert( 0 , new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 0 ,
                Descrizione = "Tutto"
            } );

            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 1 ,
                Descrizione = "Urgenti"
            } );
            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 2 ,
                Descrizione = "Scadute"
            } );
            this.RaggruppamentiEccezioni.Add( new MyRai_Raggruppamenti( )
            {
                IdRaggruppamento = 3 ,
                Descrizione = "Ordinarie"
            } );
        }
    }
}