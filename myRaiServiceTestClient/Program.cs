using myRaiServiceTestClient.it.rai.servizi.svildigigappws;
using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System.IO;
using Dipendente = myRaiServiceTestClient.it.rai.servizi.svildigigappws.Dipendente;
//using static myRaiCommonTasks.CommonTasks;
//using myRaiHelper;

namespace myRaiServiceTestClient
{
    class Program
    {
        #region Variabili
        private static WSDigigapp servizioDigigapp { get; set; }
        private static MyRaiService1Client wcf { get; set; }
        private static digiGappEntities db { get; set; }
        #endregion
        
        #region Metodi

        /// <summary>
        /// Valorizza gli oggetti che potrebbero servire nei test
        /// </summary>
        private static void InitStart ( )
        {
            // SERVIZIO DIGIGAPP
            servizioDigigapp = new WSDigigapp( );
            servizioDigigapp.Credentials = new System.Net.NetworkCredential( "srvruofpo" , "zaq22?mk" );

            // SERVIZIO WCF
            wcf = new MyRaiService1Client( );

            db = new digiGappEntities( );
        }

        #endregion


        /// <summary>
        /// 874782 True 11/1/2019 3/3/2023 10
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="valoreInfo"></param>
        /// <param name="dataInizioValidita"></param>
        /// <param name="dataFineValidita"></param>
        /// <param name="idTipoInformazione"></param>
        /// <param name="noteInfo"></param>
        /// <returns></returns>
        public static CambiaInfoDipendenteResponse cambiaInformazioniDipendente ( string matricola , string valoreInfo , DateTime? dataInizioValidita , DateTime? dataFineValidita , int idTipoInformazione , string noteInfo )
        {
            var db = new digiGappEntities( );
            try
            {


                if ( matricola == null || matricola == "" )
                    return new CambiaInfoDipendenteResponse( ) { success = false , error = "Specificare la matricola" };


                // in teoria non servirebbe
                var tipo = db.MyRai_InfoDipendente_Tipologia.Where( x => x.id == idTipoInformazione ).FirstOrDefault( );

                if ( tipo == null )
                    return new CambiaInfoDipendenteResponse( ) { success = false , error = "Tipologia informazione inesistente" };

                if ( tipo.data_fine != null && tipo.data_fine.Value.CompareTo( DateTime.Today ) < 0 )
                    return new CambiaInfoDipendenteResponse( ) { success = false , error = "Tipologia informazione non più valida" };
                bool nuovoRec = false;
                #region verifica tipo
                // verifico il tipo del valore in una try catch per dare, in caso di errore, un messaggio parlante
                try
                {
                    Convert.ChangeType( valoreInfo , Type.GetType( tipo.tipo_valore ) );
                }
                catch ( Exception )
                {
                    return new CambiaInfoDipendenteResponse
                    {
                        success = false ,
                        error = "Il valore inserito non corrisponde al tipo richiesto per questa informazione"
                    };
                }
                #endregion

                var info = db.MyRai_InfoDipendente.Where( x => x.matricola == matricola && x.id_infodipendente_tipologia == idTipoInformazione ).FirstOrDefault( );

                if ( info != null )
                {
                    // imposto la data inizio solo qualora venga passata, altrimenti lascio quella che è sul db
                    if ( dataInizioValidita != null )
                        info.data_inizio = dataInizioValidita.Value;
                }
                else
                {
                    nuovoRec = true;
                    // nuovo record
                    info = new MyRai_InfoDipendente( );
                    // se presente la data inizio ok, altrimenti imposto la data odierna
                    info.data_inizio = dataInizioValidita != null ? dataInizioValidita.Value : DateTime.Today;
                    info.matricola = matricola;
                    info.id_infodipendente_tipologia = idTipoInformazione;
                }

                // nel caso in cui il valore info venga trattato come boleano ed è false 
                // verifico che la data fine sia null, se è così, imposto la data odierna altrimenti il valore passato.
                //al contrario se il valore info è diverso da false imposto la data passata (o eventualmente null)
                if ( valoreInfo.ToLower( ) == "false" )
                    info.data_fine = dataFineValidita != null ? dataFineValidita : DateTime.Today;
                else
                    info.data_fine = dataFineValidita;

                // la data inizio sicuramente non sarà null, ma la data fine può esserlo, qualora non lo sia effettuo il controllo sulla correttezza
                if ( info.data_fine != null )
                {
                    if ( info.data_fine.Value.CompareTo( info.data_inizio ) < 0 )
                        return new CambiaInfoDipendenteResponse { success = false , error = "La data fine (" + info.data_fine.Value.ToString( ) + ") non può essere minore della data inizio (" + info.data_inizio.ToString( ) + ")" };
                }
                info.valore = valoreInfo;
                info.note = noteInfo;
                if ( nuovoRec )
                    db.MyRai_InfoDipendente.Add( info );

                db.SaveChanges( );

                var response = new CambiaInfoDipendenteResponse( )
                {
                    success = true ,
                    error = null
                };
                return response;
            }
            catch ( Exception ex )
            {
                return new CambiaInfoDipendenteResponse( )
                {
                    success = false ,
                    error = ex.Message
                };
            }
        }


        public static void getPianoFerie ( )
        {
            //ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics( );

            //string S = areaSistema( "3PF" , matricola , livelloUtente );

            //matricola = matricola.PadLeft( 7 , '0' );

            //S += matricola + dataInizio;

            var risposta = servizioDigigapp.getPianoFerie( "848345" , "01012020" , 70, "" );

            string s2;
            DateTime d1 = DateTime.Now;
            //s2 = ( string ) c.ComunicaVersoCics( S.PadRight( 8000 ) );
            DateTime d2 = DateTime.Now;
            //s2 = s2.Replace( Convert.ToChar( 0x0 ).ToString( ) , " " );
            s2 = risposta.raw;
            string ret = "";

            var dipendente = new Dipendente( );

            var db = new digiGappEntities( );

            bool esito = true;           

            if ( esito )
            {
                // reperimento della sede gapp corrente
                dipendente.sedeGapp = s2.Substring( 326 , 5 );

                ret = s2;

                //Inizio Spacchettamento Dati
                int ackLenght = 12;
                int startIndex = ackLenght + 130 + 16;// Vedere il tracciato per il +16

                // Dipendente dipendente = new Dipendente();
                dipendente.ferie = new it.rai.servizi.svildigigappws.Ferie( );

                // i valori vanno divisi per 100 perchè sono in decimale
                dipendente.ferie.ferieAnniPrecedenti = float.Parse( s2.Substring( startIndex , 7 ) ) / 100;
                if ( s2.Substring( startIndex , 7 ).StartsWith( " " ) )
                    dipendente.ferie.ferieAnniPrecedenti *= -1;
                dipendente.ferie.ferieSpettanti = float.Parse( s2.Substring( startIndex + 7 , 6 ) ) / 100;
                dipendente.ferie.ferieUsufruite = float.Parse( s2.Substring( startIndex + 13 , 6 ) ) / 100;
                dipendente.ferie.feriePianificate = float.Parse( s2.Substring( startIndex + 19 , 6 ) ) / 100;
                dipendente.ferie.ferieRichieste = float.Parse( s2.Substring( startIndex + 25 , 6 ) ) / 100;

                dipendente.ferie.exFestivitaAnniPrecedenti = float.Parse( s2.Substring( startIndex + 31 , 4 ) ) / 100;
                if ( s2.Substring( startIndex + 31 , 4 ).StartsWith( " " ) )
                    dipendente.ferie.exFestivitaAnniPrecedenti *= -1;
                dipendente.ferie.exFestivitaSpettanti = float.Parse( s2.Substring( startIndex + 35 , 3 ) ) / 100;
                dipendente.ferie.exFestivitaUsufruite = float.Parse( s2.Substring( startIndex + 38 , 3 ) ) / 100;
                dipendente.ferie.exFestivitaPianificate = float.Parse( s2.Substring( startIndex + 41 , 3 ) ) / 100;
                dipendente.ferie.exFestivitaRichieste = float.Parse( s2.Substring( startIndex + 44 , 3 ) ) / 100;

                dipendente.ferie.permessiSpettanti = float.Parse( s2.Substring( startIndex + 47 , 3 ) ) / 100;
                dipendente.ferie.permessiUsufruiti = float.Parse( s2.Substring( startIndex + 50 , 3 ) ) / 100;
                dipendente.ferie.permessiPianificati = float.Parse( s2.Substring( startIndex + 53 , 3 ) ) / 100;
                dipendente.ferie.permessiRichiesti = float.Parse( s2.Substring( startIndex + 56 , 3 ) ) / 100;

                dipendente.ferie.permessiGiornalistiAnniPrecedenti = float.Parse( s2.Substring( startIndex + 59 , 5 ) ) / 100;
                if ( s2.Substring( startIndex + 59 , 5 ).StartsWith( " " ) )
                    dipendente.ferie.permessiGiornalistiAnniPrecedenti *= -1;
                dipendente.ferie.permessiGiornalistiSpettanti = float.Parse( s2.Substring( startIndex + 64 , 4 ) ) / 100;
                dipendente.ferie.permessiGiornalistiUsufruiti = float.Parse( s2.Substring( startIndex + 68 , 4 ) ) / 100;
                dipendente.ferie.permessiGiornalistiPianificati = float.Parse( s2.Substring( startIndex + 72 , 4 ) ) / 100;
                dipendente.ferie.permessiGiornalistiRichiesti = float.Parse( s2.Substring( startIndex + 76 , 4 ) ) / 100;

                dipendente.ferie.mancatiNonLavoratiAnniPrecedenti = float.Parse( s2.Substring( startIndex + 80 , 5 ) ) / 100;
                dipendente.ferie.mancatiNonLavoratiSpettanti = float.Parse( s2.Substring( startIndex + 85 , 5 ) ) / 100;
                dipendente.ferie.recuperiNonLavoratiUsufruiti = float.Parse( s2.Substring( startIndex + 90 , 5 ) ) / 100;
                dipendente.ferie.recuperiNonLavoratiPianificati = float.Parse( s2.Substring( startIndex + 95 , 5 ) ) / 100;
                dipendente.ferie.recuperiNonLavoratiRichiesti = float.Parse( s2.Substring( startIndex + 100 , 5 ) ) / 100;

                dipendente.ferie.mancatiRiposiAnniPrecedenti = float.Parse( s2.Substring( startIndex + 105 , 5 ) ) / 100;
                dipendente.ferie.mancatiRiposiSpettanti = float.Parse( s2.Substring( startIndex + 110 , 5 ) ) / 100;
                dipendente.ferie.recuperiMancatiRiposiUsufruiti = float.Parse( s2.Substring( startIndex + 115 , 5 ) ) / 100;
                dipendente.ferie.recuperiMancatiRiposiPianificati = float.Parse( s2.Substring( startIndex + 120 , 5 ) ) / 100;
                dipendente.ferie.recuperiMancatiRiposiRichiesti = float.Parse( s2.Substring( startIndex + 125 , 5 ) ) / 100;

                dipendente.ferie.mancatiFestiviAnniPrecedenti = float.Parse( s2.Substring( startIndex + 130 , 4 ) ) / 100;
                dipendente.ferie.mancatiFestiviSpettanti = float.Parse( s2.Substring( startIndex + 134 , 4 ) ) / 100;
                dipendente.ferie.recuperiMancatiFestiviUsufruiti = float.Parse( s2.Substring( startIndex + 138 , 4 ) ) / 100;
                dipendente.ferie.recuperiMancatiFestiviPianificati = float.Parse( s2.Substring( startIndex + 142 , 4 ) ) / 100;
                dipendente.ferie.recuperiMancatiFestiviRichiesti = float.Parse( s2.Substring( startIndex + 146 , 4 ) ) / 100;

                dipendente.ferie.ferieMinime = float.Parse( s2.Substring( startIndex + 150 , 6 ) ) / 100;

                dipendente.ferie.visualizzaFerie = s2[startIndex + 156].ToString( ) == "1" ? true : false;
                dipendente.ferie.visualizzaFC = s2[startIndex + 157].ToString( ) == "1" ? true : false;
                dipendente.ferie.visualizzaPermessi = s2[startIndex + 158].ToString( ) == "1" ? true : false;
                dipendente.ferie.visualizzaPermessiGiornalisti = s2[startIndex + 159].ToString( ) == "1" ? true : false;
                dipendente.ferie.visualizzaRecuperoRiposi = s2[startIndex + 160].ToString( ) == "1" ? true : false;
                dipendente.ferie.visualizzaRecuperoNonLavorati = s2[startIndex + 161].ToString( ) == "1" ? true : false;
                dipendente.ferie.visualizzaRecuperoFestivi = s2[startIndex + 162].ToString( ) == "1" ? true : false;
                //freakdebug -  test campo spgDip
                dipendente.spgDip = s2[ackLenght + 313].ToString( );
            }
        }

        public static void GetProduzioneByTitolo ( string term )
        {
            try
            {
                it.rai.servizi.anagraficaws1.APWS sr = new it.rai.servizi.anagraficaws1.APWS( );
                sr.Credentials = new System.Net.NetworkCredential( @"srvanapro" , "bc14a3" , "RAI" );
                it.rai.servizi.anagraficaws1.ObjTVRicercaAnagrafieResult res = new it.rai.servizi.anagraficaws1.ObjTVRicercaAnagrafieResult( );

                it.rai.servizi.anagraficaws1.ObjInputRicercaTitolo ricercatitolo = new it.rai.servizi.anagraficaws1.ObjInputRicercaTitolo( );
                //ricercatitolo.LivelloDiRicerca = anagraficaws.LivelloDiRicerca.Matricola;

                ricercatitolo.modalitaDiRicerca = it.rai.servizi.anagraficaws1.ModalitaDiRicercaStringa.IniziaCon;
                ricercatitolo.CodiceAnagraf = it.rai.servizi.anagraficaws1.CodiceAnagrafico.Programmi;
                //ricercatitolo.DirittiInVita = true;
                ricercatitolo.StatiInVita = true;
                ricercatitolo.Titolo = term;
                res = sr.TvRicercaAnagrafiaTitolo( ricercatitolo );

                var results = res.RisultatoTVRicercaAnagrafie.ToList( );
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
        }

        public static void GetProduzioneByMatricola ( string term )
        {
            try
            {
                it.rai.servizi.anagraficaws1.APWS sr = new it.rai.servizi.anagraficaws1.APWS( );
                sr.Credentials = new System.Net.NetworkCredential( @"srvanapro" , "bc14a3" , "RAI" );
                it.rai.servizi.anagraficaws1.ObjTVRicercaAnagrafieResult res = new it.rai.servizi.anagraficaws1.ObjTVRicercaAnagrafieResult( );

                it.rai.servizi.anagraficaws1.ObjInputRicercaMatricola ricercaMatricola = new it.rai.servizi.anagraficaws1.ObjInputRicercaMatricola( );
                //ricercatitolo.LivelloDiRicerca = anagraficaws.LivelloDiRicerca.Matricola;

                ricercaMatricola.Matricola = term;
                ricercaMatricola.StatiInVita = true;
                ricercaMatricola.RicercaAncheNelleUorgDiProduzione = true;
                ricercaMatricola.RicercaAncheNelleUorgRaiTrade = true;

                res = sr.TvRicercaAnagrafiaMatricola( ricercaMatricola );

                var results = res.RisultatoTVRicercaAnagrafie.ToList( );

            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
        }

        public static void GetProduzioneByOURG ( string term )
        {
            try
            {
                it.rai.servizi.anagraficaws1.APWS sr = new it.rai.servizi.anagraficaws1.APWS( );
                sr.Credentials = new System.Net.NetworkCredential( @"srvanapro" , "bc14a3" , "RAI" );
                it.rai.servizi.anagraficaws1.ObjTvRicercaTrasmissioniResult res = new it.rai.servizi.anagraficaws1.ObjTvRicercaTrasmissioniResult( );

                it.rai.servizi.anagraficaws1.ObjInputRicercaTxMatricola ricercaMatricola = new it.rai.servizi.anagraficaws1.ObjInputRicercaTxMatricola( );
                
                ricercaMatricola.Uorg = term;

                res = sr.TvRicercaTrasmissioneMatricola( ricercaMatricola );
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
        }

        public static string CheckMatricoleParticolari ( string pmatr )
        {
            //string differentUsers = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.FirmaMatricoleParticolari );
            string differentUsers = "";
            if ( !String.IsNullOrWhiteSpace( differentUsers ) )
            {
                Dictionary<string , string> keyValues = differentUsers.Split( ',' ).Select( x => new { Originale = x.Split( '|' )[0] , Modificata = x.Split( '|' )[1] } )
                    .ToDictionary( y => y.Originale , y => y.Modificata );

                string mod = "";
                if ( keyValues.TryGetValue( pmatr , out mod ) )
                    pmatr = mod;
            }

            return pmatr;
        }

        private static string areaSistema ( string funzione , string matricola , int livelloUtente , bool richiama = false)
        {
            string flg_chiamata = richiama ? "S" : " ";

            return "P956,3XX," + funzione + "GAPWEB" + matricola.PadLeft( 10 , '0' ) + DateTime.Now.ToString( "ddMMyyyy" ) + "".PadRight( 5 , '0' ) + flg_chiamata + "".PadRight( 6 , ' ' ) + "".PadRight( 25 , 'a' ) + livelloUtente.ToString( ) + DateTime.Now.ToString( "HH:mm:ss" ).PadLeft( 14 , ' ' ) + ",";
        }

        #region Conteggio giorni consecutivi
        //public static myRaiService.ConteggioGiorniConsecutivi_Response GetGiorniConsecutivi ( string matricola , string sedeGapp , DateTime dataPartenza )
        //{
        //    ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics( );
        //    myRaiService.ConteggioGiorniConsecutivi_Response response = new myRaiService.ConteggioGiorniConsecutivi_Response( );
        //    List<myRaiService.ConteggioGiorniConsecutivi> dip = new List<myRaiService.ConteggioGiorniConsecutivi>( );
        //    string indiceStart = "000";
        //    bool riciclo = false;
        //    try
        //    {
        //        int nRiga = 1;
        //        do
        //        {
        //            if ( nRiga > 1 )
        //            {
        //                Console.WriteLine( String.Format( "Riciclo" ) );
        //            }

        //            string S = areaSistema( "3LC" , matricola , 75 , riciclo );
        //            S += sedeGapp + dataPartenza.ToString( "ddMMyyyy" ) + indiceStart + "".PadLeft( 34 );
        //            var risposta = ( string ) c.ComunicaVersoCics( S.PadRight( 8000 ) );
        //            risposta = risposta.Replace( Convert.ToChar( 0x0 ).ToString( ) , " " );

        //            indiceStart = risposta.Substring( 105 , 3 );
        //            riciclo = ( risposta.Substring( 44 , 1 ) == "S" ) ? true : false;
        //            string esito = risposta.Substring( 45 , 2 );
        //            if ( !String.IsNullOrEmpty( esito ) && esito.Equals( "OK" ) )
        //            {
        //                // rimuove la parte iniziale della risposta per avere solo l'array di dati
        //                var elementi = risposta.Substring( 142 );
        //                do
        //                {
        //                    DateTime data = dataPartenza;

        //                    // prende la prima riga
        //                    var item = elementi.Substring( 0 , 120 );
        //                    myRaiService.ConteggioGiorniConsecutivi dipendente = new myRaiService.ConteggioGiorniConsecutivi( );
        //                    dipendente.Matricola = item.Substring( 0 , 7 );
        //                    dipendente.Nominativo = item.Substring( 7 , 25 );
        //                    dipendente.TipoDipendente = item.Substring( 32 , 1 );
        //                    // prendere l'array dei 40 giorni
        //                    item = item.Substring( 40 );
        //                    dipendente.Giorni = new List<myRaiService.DettaglioConteggioGiorniConsecutivi>( );
        //                    do
        //                    {
        //                        string val = item.Substring( 0 , 2 );
        //                        myRaiService.DettaglioConteggioGiorniConsecutivi obj = new myRaiService.DettaglioConteggioGiorniConsecutivi( );
        //                        obj.Giorno = data;

        //                        // se ha ** allora la giornata non è presente/dati non disponibili per quella data
        //                        if ( val == "**" )
        //                        {
        //                            obj.ConteggioOccorrenza = 0;
        //                            obj.Info = String.Empty;
        //                            obj.Descrizione = "Giornata non presente";
        //                        }
        //                        else if ( Regex.Matches( val , @"[a-zA-Z]" ).Count > 0 )
        //                        {
        //                            // se ci sono lettere allora è un'eccezione
        //                            obj.ConteggioOccorrenza = 0;
        //                            obj.Info = val;
        //                            obj.Descrizione = "Codice eccezione";
        //                        }
        //                        else if ( val.Contains( "00" ) || val.Contains( "  " ) )
        //                        {
        //                            // se 00 o spazio vuoto allora è un codice orario
        //                            obj.ConteggioOccorrenza = 0;
        //                            obj.Info = val;
        //                            obj.Descrizione = "Codice orario";
        //                        }
        //                        else if ( Regex.Matches( val , @"[0-9]" ).Count > 0 )
        //                        {
        //                            int dato = int.Parse( val );

        //                            // se il valore numerico è compreso tra 90 e 99 allora
        //                            // è un codice orario
        //                            if ( dato >= 90 && dato <= 99 )
        //                            {
        //                                obj.ConteggioOccorrenza = 0;
        //                                obj.Info = val;
        //                                obj.Descrizione = "Codice orario";
        //                            }
        //                            else
        //                            {
        //                                // se ha un valore numerico da 1 a 89, allora 
        //                                // è il conteggio delle giornate consecutive
        //                                obj.ConteggioOccorrenza = dato;
        //                                obj.Info = String.Empty;
        //                                obj.Descrizione = "Valore giorni";
        //                            }
        //                        }

        //                        obj.Festivo = IsFestivo( data , sedeGapp ) || IsSuperFestivo( data );

        //                        dipendente.Giorni.Add( obj );
        //                        item = item.Substring( 2 );
        //                        data = data.AddDays( 1 );
        //                    } while ( item.Length > 0 );

        //                    dip.Add( dipendente );
        //                    elementi = elementi.Substring( 120 );
        //                    Console.WriteLine( String.Format( "{0}) - {1} - {2} - {3}" , nRiga, dipendente.Matricola , dipendente.Nominativo , dipendente.TipoDipendente ) );
        //                    nRiga++;
        //                } while ( elementi.Length >= 120 );
        //             }
        //            else
        //            {
        //                string errore = risposta.Substring( 0 , 5 ) + " " + risposta.Substring( 51 , 25 );
        //                throw new Exception( errore );
        //            }

        //        } while ( riciclo );

        //        response = new myRaiService.ConteggioGiorniConsecutivi_Response( )
        //        {
        //            Errore = null ,
        //            Esito = true ,
        //            Risposta = new List<myRaiService.ConteggioGiorniConsecutivi>( )
        //        };

        //        response.Risposta.AddRange( dip.ToList( ) );
        //    }
        //    catch ( Exception ex )
        //    {
        //        response.Esito = false;
        //        response.Errore = ex.Message;
        //        response.Risposta = null;
        //        string datiInput = String.Format( "matricola {0}, sedeGapp {1}, data partenza {2}" , matricola , sedeGapp , dataPartenza.ToString( "dd/MM/yyyy" ) );
        //        Console.WriteLine( ex.ToString( ) + "\n\r" + datiInput );
        //    }

        //    return response;
        //}

        /// <summary>
        /// Verifica se una particolare data è un giorno festivo
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsFestivo ( DateTime data , string sedeGapp )
        {
            bool result = false;

            try
            {
                // Epifania
                if ( data.Day == 6 && data.Month == 1 )
                {
                    result = true;
                }

                // Liberazione
                if ( data.Day == 25 && data.Month == 4 )
                {
                    result = true;
                }

                // Festa della Repubblica
                if ( data.Day == 2 && data.Month == 6 )
                {
                    result = true;
                }

                // 1 Novembre Tutti i santi
                if ( data.Day == 1 && data.Month == 11 )
                {
                    result = true;
                }

                // Immacolata concezione
                if ( data.Day == 8 && data.Month == 12 )
                {
                    result = true;
                }

                // Santo Stefano
                if ( data.Day == 26 && data.Month == 12 )
                {
                    result = true;
                }

                DateTime pasqua = GetPasqua( data.Year );

                // Lunedì dell'angelo
                if ( data.Day + 1 == pasqua.Day && data.Month == pasqua.Month )
                {
                    result = true;
                }

                // PATRONO

                DateTime patrono = GetPatrono( sedeGapp );

                if ( patrono != DateTime.MinValue )
                {
                    if ( data.Day == patrono.Day && data.Month == patrono.Month )
                    {
                        result = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }

            return result;
        }

        /// <summary>
        /// Reperimento della data di decorrenza della festa patronale della
        /// sede gapp
        /// </summary>
        /// <returns></returns>
        private static DateTime GetPatrono ( string sede )
        {
            DateTime result = DateTime.MinValue;

            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    var row = db.L2D_SEDE_GAPP.Where( c => c.cod_sede_gapp.Equals( sede , StringComparison.InvariantCultureIgnoreCase ) ).FirstOrDefault( );

                    if ( row != null && row.Data_Patrono.HasValue )
                    {
                        result = row.Data_Patrono.Value;
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }

            return result;
        }

        /// <summary>
        /// Calcolo della pasqua
        /// </summary>
        /// <param name="year">Anno</param>
        /// <returns>Data completa in cui ricade la pasqua</returns>
        private static DateTime GetPasqua ( int year )
        {
            int month = 0;
            int day = 0;

            int g = year % 19;
            int c = year / 100;
            int h = h = ( c - ( int ) ( c / 4 ) - ( int ) ( ( 8 * c + 13 ) / 25 )
                                                + 19 * g + 15 ) % 30;
            int i = h - ( int ) ( h / 28 ) * ( 1 - ( int ) ( h / 28 ) *
                        ( int ) ( 29 / ( h + 1 ) ) * ( int ) ( ( 21 - g ) / 11 ) );

            day = i - ( ( year + ( int ) ( year / 4 ) +
                          i + 2 - c + ( int ) ( c / 4 ) ) % 7 ) + 28;
            month = 3;

            if ( day > 31 )
            {
                month++;
                day -= 31;
            }

            return new DateTime( year , month , day );
        }

        /// <summary>
        /// Verifica se una particolare data è un giorno super festivo
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsSuperFestivo ( DateTime data )
        {
            bool result = false;

            try
            {
                // Epifania
                if ( data.Day == 1 && data.Month == 1 )
                {
                    result = true;
                }

                // 1 Maggio Festa dei lavoratori
                if ( data.Day == 1 && data.Month == 5 )
                {
                    result = true;
                }

                // 15 Agosto
                if ( data.Day == 15 && data.Month == 8 )
                {
                    result = true;
                }

                // Natale
                if ( data.Day == 25 && data.Month == 12 )
                {
                    result = true;
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }

            return result;
        }

        #endregion

        private static getPDFResponse getListaPDF ( List<string> sediGapp , DateTime dataDa , DateTime dataA , List<string> tipoPDF ,
        List<string> statoPDF , Boolean PDFrequested = true )
        {

            getPDFResponse resp = new getPDFResponse( );
            List<string> statiPDFModificati = new List<string>( );

            foreach ( string stato in statoPDF )
            {
                if ( !stato.Contains( "_OK" ) )
                    statiPDFModificati.Add( stato + "_OK" );
                else
                    statiPDFModificati.Add( stato );
            }

            try
            {
                digiGappEntities context = new digiGappEntities( );
                bool IsLastPdf = false;

                var pdf = context.DIGIRESP_Archivio_PDF.Where(
                    x => x.data_fine >= dataDa && x.data_fine <= dataA
                    && sediGapp.Contains( x.sede_gapp ) && tipoPDF.Contains( x.tipologia_pdf )
                    && statiPDFModificati.Contains( x.stato_pdf ) )
                    .Select( x => new
                    {
                        ID = x.ID ,
                        data_inizio = x.data_inizio ,
                        data_fine = x.data_fine ,
                        sede_gapp = x.sede_gapp ,
                        numero_versione = x.numero_versione ,
                        data_convalida = x.data_convalida ,
                        pdf = ( PDFrequested ? x.pdf : null ) ,
                        stato_pdf = x.stato_pdf ,
                        tipologia_pdf = x.tipologia_pdf ,
                        matricola_stampa = x.matricola_stampa ,
                        matricola_convalida = x.matricola_convalida ,
                        data_stampa = x.data_stampa
                    } );

                foreach ( var row in pdf )
                {
                    recordPDF record = new recordPDF( );

                    record.ID = row.ID;
                    record.dataDa = row.data_inizio;
                    record.dataA = row.data_fine;
                    record.sedeGapp = row.sede_gapp;
                    record.sedeGappDesc = "";
                    record.numeroVersione = row.numero_versione;
                    record.dataConvalida = row.data_convalida;
                    record.PDF = row.pdf;
                    record.statoPDF = "";
                    record.tipoPDF = "";

                    //var last = context.DIGIRESP_Archivio_PDF.Where( x => sediGapp.Contains( x.sede_gapp ) && tipoPDF.Contains( x.tipologia_pdf ) ).Max( w => w.data_stampa );

                    var last = context.DIGIRESP_Archivio_PDF.Where( x => x.sede_gapp == row.sede_gapp && tipoPDF.Contains( x.tipologia_pdf ) ).Max( w => w.data_stampa );

                    if ( row.data_stampa.HasValue )
                    {
                        if ( last.GetValueOrDefault( ) > row.data_stampa.GetValueOrDefault( ) )
                        {
                            IsLastPdf = false;
                        }
                        else
                        {
                            IsLastPdf = true;
                        }
                    }
                    else
                    {
                        IsLastPdf = false;
                    }                    
                }
                resp.esito = true;
            }
            catch ( Exception e )
            {
                resp.esito = false;
                resp.errore = e.Message;
            }

            return resp;
        }


        static void Main(string[] args)
        {
            InitStart( );

            //NotificheDestinateDipendenti.SendNotificheESS_DIP( "794795" );
            //try
            //{
            //    //var miotest = wcf.cambiaInformazioniDipendente( "874782" , "True" , new DateTime( 2019 , 11 , 1 ) , new DateTime( 2023 , 3 , 3 ) , 10 , null );

            //    List<string> tipi = new List<string>( );
            //    List<string> stati = new List<string>( );
            //    List<string> sedi = new List<string>( );
            //    sedi.Add( "3CM01" );
            //    sedi.Add( "8BG00" );
            //    sedi.Add( "8BA00" );
            //    sedi.Add( "8BA11" );
            //    sedi.Add( "8BC00" );
            //    sedi.Add( "8BG10" );

            //    tipi.Add( "P" );
            //    tipi.Add( "R" );

            //    stati.Add( "S" );
            //    stati.Add( "C" );

            //    var risposta = getListaPDF( sedi , new DateTime( 2021 , 3 , 1 ) , new DateTime( 2021 , 3 , 31 ) , tipi , stati , false );

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine( ex.Message );
            //}

            //var risposta = cambiaInformazioniDipendente( "874782" , "True" , new DateTime( 2019 , 11 , 1 ) , new DateTime( 2023 , 3 , 3 ) , 10 , null );

            //return;
            //string matricolaRichiesta = "103650";
            //string matricolaRichiedente = "103650";

            ////string matricola = "P103650";
            //string matricola = "P" + matricolaRichiesta;

            //string sUserName = matricola + new string( ' ' , 10 - matricola.Length );

            //string strChr = "00000";
            ////string sUserNameRequester = "103650";
            //string sUserNameRequester = matricolaRichiedente;
            //string inputData = "P956,DT,HRPESS" + sUserName + "0" + strChr + sUserNameRequester + "," + ",";
            //inputData = inputData + new string( ' ' , 2000 );

            //Output.WriteLine( inputData );

            //return;







            //var miotest = wcf.GetSituazioneDebitoria( "103650" , "103650" );
            
            //return;
            //string matricola = "P103650";
            //string sUserName = matricola + new string( ' ' , 10 - matricola.Length );

            //string strChr = "00000";
            //string sUserNameRequester = "103650";
            //string inputData = "P956,DT,HRPESS" + sUserName + "0" + strChr + sUserNameRequester + "," + ",";
            //inputData = inputData + new string( ' ' , 2000 );




            var response = servizioDigigapp.getPianoFerie("909736" , "01012021" , 70 , "BU" );
            List<DateTime> listaGiorniPXC = new List<DateTime>( );
            List<DateTime> pxcDays = new List<DateTime>( );
            int TotalePXC = 0;

            try
            {
                if ( response != null )
                {
                    // recupero giorni con codice 95 o 96
                    var giorni = response.dipendente.ferie.giornate.Where( g => g.orarioReale.Equals( "95" ) || g.orarioReale.Equals( "96" ) ).ToList( );
                    var giorniMNMR = response.dipendente.ferie.giornate.Where( g => g.raw.Contains( "MN" ) || g.raw.Contains( "MR" ) ).ToList( );

                    if ( giorni != null && giorni.Any( ) )
                    {
                        foreach ( var g in giorni )
                        {
                            int gg = int.Parse( g.dataTeorica.Substring( 0 , 2 ) );
                            int mese = int.Parse( g.dataTeorica.Substring( 3 , 2 ) );

                            // calcolo la data in cui è stato assegnato MN
                            DateTime toCompare = new DateTime( 2021 , mese , gg );
                            if ( toCompare <= DateTime.Now.Date )
                            {
                                if ( FestivitaManager.IsFestivo( toCompare , response.dipendente.sedeGapp ) ||
                                    FestivitaManager.IsSuperFestivo( toCompare ) ||
                                    FestivitaManager.IsExFestivo( toCompare , response.dipendente.sedeGapp ) )
                                {
                                    listaGiorniPXC.Add( toCompare );
                                    TotalePXC++;
                                }
                            }
                        }
                    }

                    if ( giorniMNMR != null && giorniMNMR.Any( ) )
                    {
                        foreach ( var g in giorniMNMR )
                        {
                            int gg = int.Parse( g.dataTeorica.Substring( 0 , 2 ) );
                            int mese = int.Parse( g.dataTeorica.Substring( 3 , 2 ) );

                            // calcolo la data in cui è stato assegnato MN
                            DateTime toCompare = new DateTime( 2021 , mese , gg );
                            if ( toCompare <= DateTime.Now.Date )
            {

                                if (FestivitaManager.IsFestivo(toCompare , response.dipendente.sedeGapp) ||
                                    FestivitaManager.IsSuperFestivo(toCompare) ||
                                    FestivitaManager.IsExFestivo(toCompare , response.dipendente.sedeGapp))
                {
                                    listaGiorniPXC.Add( toCompare );
                                    TotalePXC++;
                }

                                //if ( FestivitaManager.IsFestivo( toCompare , response.dipendente.sedeGapp ) )
                                //{
                                //listaGiorniPXC.Add(toCompare);
                                //TotalePXC++;
                                //}
                }
            }
            }

                    if ( listaGiorniPXC.Any( ) )
                    {
                        int sumPXC = 0;
                        var listaEccPXC = response.dipendente.ferie.giornate.Where( g => g.raw.Contains( "PXC" ) ).ToList( );

                        if ( listaEccPXC != null && listaEccPXC.Any( ) )
                        {
                            foreach ( var g in listaEccPXC )
                            {
                                int gg = int.Parse( g.dataTeorica.Substring( 0 , 2 ) );
                                int mese = int.Parse( g.dataTeorica.Substring( 3 , 2 ) );

                                DateTime toCompare = new DateTime( 2021 , mese , gg );

                                if ( toCompare <= DateTime.Now.Date )
                                {
                                    sumPXC++;
                                }
                            }
                        }

                        int finale = TotalePXC - sumPXC;
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }


            //string matricola = "103650";
            //string dataInizio = "01102020";
            //string sedeGapp = "DDE30";
            //string rapportoLavoro = "";
            //string tipodipendente = "";

            //string inviata = areaSistema( "3FD" , "103650" , 75 );

            //inviata += matricola.PadLeft( 7 , '0' ) + dataInizio + "".PadLeft( 8 , '0' ) + sedeGapp + rapportoLavoro + "".PadLeft( 11 , '0' ) + tipodipendente;
            //InitStart( );

            //var mioServizio = servizioDigigapp.getPianoFerieSedeGapp( matricola , dataInizio , rapportoLavoro , tipodipendente , "DDE30" , 75 );

            //return;

            //List<string> sedi = new List<string>( );
            //sedi.Add( "DDE60" );

            //var result = wcf.GetRipianificazioni( sedi.ToArray( ) , new DateTime( 2020 , 1 , 1 ) , new DateTime( 2020 , 12 , 31 ) );
            //return;
        }

        private static void Rmtr()
        {
            var db = new digiGappEntities();
            var list = db.L2D_INSEDIAMENTO.ToList();

            var linee = System.IO.File.ReadAllLines(@"d:\rmtr.txt");
            foreach (var linea in linee)
            {
                string codIns = linea.Split(';')[0].PadLeft(3, '0');
                var ins= list.Where(x => x.cod_insediamento == codIns).FirstOrDefault();
                if (ins == null)
                {
                    Console.WriteLine(codIns + " NON PRESENTE");
                    continue;
                }
                string importo = linea.Split(';')[1].Replace(",", "").PadLeft(7, '0') ;
                if (importo != ins.Importo_Insediamento)
                {
                    Console.WriteLine(codIns + " IMPORTO DIVERSO :" + importo + " / " + ins.Importo_Insediamento );
                }

                Console.WriteLine(linea.Split(';')[2] + "/" + linea.Split(';')[3] + "  " + ((DateTime)ins.Entrata_Insediamento).ToString("HHmm") + "/" + ((DateTime)ins.Uscita_Insediamento).ToString("HHmm"));

            }
        }

        private static void TestWCF()
        {
            var risultato = wcf.GetReport_POH_ROH( "527786" , "1FAD0" , 9 , 2019 );
            return;
        }

        private static void ScriviFile ( string msg , string nomeFile = "" )
        {
            if ( !String.IsNullOrEmpty( msg ) )
            {
                string filelog1 = "";
                var location = System.Reflection.Assembly.GetEntryAssembly( ).Location;

                var directoryPath = Path.GetDirectoryName( location );
                var logPath = Path.Combine( directoryPath , "Passo4" + nomeFile );
                if ( !Directory.Exists( logPath ) )
                    Directory.CreateDirectory( logPath );

                filelog1 = Path.Combine( logPath , "ConsoleLog_" + DateTime.Now.ToString( "yyyyMMdd" ) + ".txt" );

                File.AppendAllText( filelog1 , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) + " " + msg + "\r\n" );
            }
        }
    }
}